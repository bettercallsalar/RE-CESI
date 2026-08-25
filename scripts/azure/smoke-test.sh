#!/bin/sh

set -eu

readonly APP_DIRECTORY="${RECESI_APP_DIRECTORY:-/home/recesi/recesi}"
readonly COMPOSE_FILE="$APP_DIRECTORY/docker-compose.yml"
readonly ENV_FILE="$APP_DIRECTORY/.env"
readonly COMPOSE_PROJECT_NAME="recesi"
readonly HEALTHCHECK_URL="${RECESI_HEALTHCHECK_URL:-http://127.0.0.1/}"
readonly MAX_ATTEMPTS="${RECESI_HEALTHCHECK_ATTEMPTS:-18}"
readonly RETRY_DELAY_SECONDS="${RECESI_HEALTHCHECK_DELAY_SECONDS:-5}"
readonly EXPECTED_SERVICES="db api frontend"

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

verify_http() {
  attempt=1

  while [ "$attempt" -le "$MAX_ATTEMPTS" ]; do
    if curl -fsS --max-time 10 "$HEALTHCHECK_URL" >/dev/null; then
      return
    fi

    sleep "$RETRY_DELAY_SECONDS"
    attempt=$((attempt + 1))
  done

  echo "HTTP smoke test failed: $HEALTHCHECK_URL" >&2
  compose logs --tail 100
  exit 1
}

main() {
  verify_services
  verify_http
  compose ps --all
  echo "RE-CESI smoke test succeeded."
}

main "$@"
