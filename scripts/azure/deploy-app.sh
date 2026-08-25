#!/bin/sh

set -eu

readonly APP_USER="${RECESI_APP_USER:-recesi}"
readonly APP_DIRECTORY="${RECESI_APP_DIRECTORY:-/home/recesi/recesi}"
readonly COMPOSE_FILE="$APP_DIRECTORY/docker-compose.yml"
readonly ENV_FILE="$APP_DIRECTORY/.env"
readonly COMPOSE_PROJECT_NAME="recesi"
readonly MYSQL_DATABASE="resr"
readonly MYSQL_USER="resr"
readonly JWT_ISSUER="RESR.WebAPI"
readonly JWT_AUDIENCE="RESR.Client"
readonly JWT_EXPIRATION_MINUTES="60"

temporary_directory=""

cleanup() {
  docker logout ghcr.io >/dev/null 2>&1 || true

  if [ -z "$temporary_directory" ] || [ ! -d "$temporary_directory" ]; then
    return
  fi

  rm -r -- "$temporary_directory"
}

require_inputs() {
  : "${COMPOSE_FILE_BASE64:?COMPOSE_FILE_BASE64 is required}"
  : "${COMPOSE_FILE_SHA256:?COMPOSE_FILE_SHA256 is required}"
  : "${IMAGE_TAG:?IMAGE_TAG is required}"
  : "${REGISTRY_OWNER:?REGISTRY_OWNER is required}"
  : "${GHCR_TOKEN:?GHCR_TOKEN is required}"
  : "${MYSQL_PASSWORD:?MYSQL_PASSWORD is required}"
  : "${MYSQL_ROOT_PASSWORD:?MYSQL_ROOT_PASSWORD is required}"
  : "${JWT_SECRET:?JWT_SECRET is required}"
}

validate_image_tag() {
  if printf '%s' "$IMAGE_TAG" | grep -Eq '^[0-9a-f]{40}$'; then
    return
  fi

  echo "IMAGE_TAG must be a full Git commit SHA." >&2
  exit 1
}

prepare_configuration() {
  local actual_compose_sha
  local compose_candidate
  local env_candidate

  temporary_directory="$(mktemp -d)"
  compose_candidate="$temporary_directory/docker-compose.yml"
  env_candidate="$temporary_directory/.env"

  printf '%s' "$COMPOSE_FILE_BASE64" | base64 --decode >"$compose_candidate"
  actual_compose_sha="$(sha256sum "$compose_candidate" | awk '{print $1}')"

  if [ "$actual_compose_sha" != "$COMPOSE_FILE_SHA256" ]; then
    echo "Docker Compose checksum mismatch." >&2
    exit 1
  fi

  {
    printf 'MYSQL_DATABASE=%s\n' "$MYSQL_DATABASE"
    printf 'MYSQL_USER=%s\n' "$MYSQL_USER"
    printf 'MYSQL_PASSWORD=%s\n' "$MYSQL_PASSWORD"
    printf 'MYSQL_ROOT_PASSWORD=%s\n' "$MYSQL_ROOT_PASSWORD"
    printf 'JWT_ISSUER=%s\n' "$JWT_ISSUER"
    printf 'JWT_AUDIENCE=%s\n' "$JWT_AUDIENCE"
    printf 'JWT_SECRET=%s\n' "$JWT_SECRET"
    printf 'JWT_EXPIRATION_MINUTES=%s\n' "$JWT_EXPIRATION_MINUTES"
    printf 'REGISTRY_OWNER=%s\n' "$REGISTRY_OWNER"
    printf 'IMAGE_TAG=%s\n' "$IMAGE_TAG"
  } >"$env_candidate"

  install -d -o "$APP_USER" -g "$APP_USER" -m 0750 "$APP_DIRECTORY"
  install -o "$APP_USER" -g "$APP_USER" -m 0640 "$compose_candidate" "$COMPOSE_FILE"
  install -o "$APP_USER" -g "$APP_USER" -m 0600 "$env_candidate" "$ENV_FILE"
}

compose() {
  docker compose \
    --project-name "$COMPOSE_PROJECT_NAME" \
    --project-directory "$APP_DIRECTORY" \
    --env-file "$ENV_FILE" \
    --file "$COMPOSE_FILE" \
    "$@"
}

deploy() {
  printf '%s' "$GHCR_TOKEN" | docker login ghcr.io --username "$REGISTRY_OWNER" --password-stdin
  compose config --quiet
  compose pull
  compose up -d --remove-orphans
  docker image prune -f
  compose ps --all
}

main() {
  trap cleanup EXIT INT TERM
  require_inputs
  validate_image_tag
  prepare_configuration
  deploy
}

main "$@"
