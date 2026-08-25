#!/bin/sh

set -eu

readonly APP_DIRECTORY="${RECESI_APP_DIRECTORY:-/home/recesi/recesi}"
readonly COMPOSE_FILE="$APP_DIRECTORY/docker-compose.yml"
readonly ENV_FILE="$APP_DIRECTORY/.env"
readonly COMPOSE_PROJECT_NAME="recesi"
readonly HEALTHCHECK_DOMAIN="${RECESI_HEALTHCHECK_DOMAIN:?RECESI_HEALTHCHECK_DOMAIN is required}"
readonly FRONTEND_HEALTHCHECK_URL="https://$HEALTHCHECK_DOMAIN/"
readonly API_HEALTHCHECK_URL="https://$HEALTHCHECK_DOMAIN/api/departments"
readonly MAX_ATTEMPTS="${RECESI_HEALTHCHECK_ATTEMPTS:-18}"
readonly RETRY_DELAY_SECONDS="${RECESI_HEALTHCHECK_DELAY_SECONDS:-5}"
readonly EXPECTED_SERVICES="db api frontend proxy"

compose() {
  docker compose \
    --project-name "$COMPOSE_PROJECT_NAME" \
    --project-directory "$APP_DIRECTORY" \
    --env-file "$ENV_FILE" \
    --file "$COMPOSE_FILE" \
    "$@"
}

verify_services() {
  local running_services
  local service

  running_services="$(compose ps --services --status running)"

  for service in $EXPECTED_SERVICES; do
    if printf '%s\n' "$running_services" | grep -Fx "$service" >/dev/null; then
      continue
    fi

    echo "Service is not running: $service" >&2
    compose ps --all
    return 1
  done
}

verify_url() {
  local healthcheck_name="$1"
  local healthcheck_url="$2"

  attempt=1

  while [ "$attempt" -le "$MAX_ATTEMPTS" ]; do
    if curl -fsS \
      --max-time 10 \
      --noproxy '*' \
      --resolve "$HEALTHCHECK_DOMAIN:443:127.0.0.1" \
      "$healthcheck_url" >/dev/null; then
      return
    fi

    sleep "$RETRY_DELAY_SECONDS"
    attempt=$((attempt + 1))
  done

  echo "$healthcheck_name smoke test failed: $healthcheck_url" >&2
  compose logs --tail 100
  exit 1
}

main() {
  verify_services
  verify_url "Frontend" "$FRONTEND_HEALTHCHECK_URL"
  verify_url "API" "$API_HEALTHCHECK_URL"
  compose ps --all
  echo "RE-CESI smoke test succeeded."
}

main "$@"
