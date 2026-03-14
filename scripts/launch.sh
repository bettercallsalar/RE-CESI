#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

docker_compose() {
  if docker compose version >/dev/null 2>&1; then
    docker compose "$@"
  elif command -v docker-compose >/dev/null 2>&1; then
    docker-compose "$@"
  else
    echo "Neither 'docker compose' nor 'docker-compose' was found."
    exit 1
  fi
}

wait_for_service_health() {
  local service="$1"
  local timeout_seconds="${2:-180}"
  local start_ts current_ts container_id status last_status

  container_id="$(docker_compose ps -q "$service" 2>/dev/null || true)"
  if [ -z "${container_id}" ]; then
    echo "Could not find container for service '$service'."
    return 1
  fi

  echo "Waiting for '$service' to become healthy (timeout: ${timeout_seconds}s)..."
  start_ts="$(date +%s)"
  last_status=""

  while true; do
    status="$(
      docker inspect \
        --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' \
        "$container_id" 2>/dev/null || echo "missing"
    )"

    if [ "$status" != "$last_status" ]; then
      echo "  $service status: $status"
      last_status="$status"
    fi

    case "$status" in
      healthy|running)
        return 0
        ;;
      exited|dead|missing)
        echo "Service '$service' is not running (status: $status)."
        docker_compose logs --no-color "$service" || true
        return 1
        ;;
    esac

    current_ts="$(date +%s)"
    if [ $((current_ts - start_ts)) -ge "$timeout_seconds" ]; then
      echo "Timed out waiting for '$service' to become healthy."
      docker_compose ps
      docker_compose logs --no-color "$service" || true
      return 1
    fi

    sleep 2
  done
}

wait_for_service_completion() {
  local service="$1"
  local timeout_seconds="${2:-180}"
  local start_ts current_ts container_id status exit_code

  container_id="$(docker_compose ps -q "$service" 2>/dev/null || true)"
  if [ -z "${container_id}" ]; then
    echo "Could not find container for service '$service'."
    return 1
  fi

  echo "Waiting for '$service' to complete (timeout: ${timeout_seconds}s)..."
  start_ts="$(date +%s)"

  while true; do
    status="$(docker inspect --format '{{.State.Status}}' "$container_id" 2>/dev/null || echo "missing")"

    case "$status" in
      exited)
        exit_code="$(docker inspect --format '{{.State.ExitCode}}' "$container_id" 2>/dev/null || echo "1")"
        if [ "$exit_code" = "0" ]; then
          return 0
        fi

        echo "Service '$service' failed with exit code $exit_code."
        docker_compose logs --no-color "$service" || true
        return 1
        ;;
      dead|missing)
        echo "Service '$service' is unavailable (status: $status)."
        docker_compose logs --no-color "$service" || true
        return 1
        ;;
    esac

    current_ts="$(date +%s)"
    if [ $((current_ts - start_ts)) -ge "$timeout_seconds" ]; then
      echo "Timed out waiting for '$service' to complete."
      docker_compose logs --no-color "$service" || true
      return 1
    fi

    sleep 2
  done
}

if [ ! -f .env ]; then
  echo "Missing .env. Creating from template..."
  if [ -f .env.example ]; then
    cp .env.example .env
  elif [ -f .example.env ]; then
    cp .example.env .env
  else
    echo "No env template found (.env.example or .example.env)."
    exit 1
  fi
fi

docker_compose up -d --build db
wait_for_service_health db "${DB_STARTUP_TIMEOUT_SECONDS:-180}"

# MySQL only applies MYSQL_DATABASE/MYSQL_USER on first init of an empty datadir.
# If a reused volume is missing the target schema, create it explicitly.
db_name="${MYSQL_DATABASE:-resr}"
db_user="${MYSQL_USER:-resr}"
db_password="${MYSQL_PASSWORD:-resr}"
root_password="${MYSQL_ROOT_PASSWORD:-root}"

echo "Ensuring database '${db_name}' exists..."
docker_compose exec -T db mysql -uroot "-p${root_password}" -e \
  "CREATE DATABASE IF NOT EXISTS \`${db_name}\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; \
   CREATE USER IF NOT EXISTS '${db_user}'@'%' IDENTIFIED BY '${db_password}'; \
   GRANT ALL PRIVILEGES ON \`${db_name}\`.* TO '${db_user}'@'%'; \
   FLUSH PRIVILEGES;"

# Run migrations first and fail fast if Flyway cannot apply them.
docker_compose up -d --build --force-recreate migrate
wait_for_service_completion migrate "${MIGRATE_TIMEOUT_SECONDS:-180}"

# Start long-running application services after a successful migration pass.
docker_compose up -d --build --force-recreate api frontend
wait_for_service_health api "${API_STARTUP_TIMEOUT_SECONDS:-180}"
wait_for_service_health frontend "${FRONTEND_STARTUP_TIMEOUT_SECONDS:-180}"
docker_compose ps

frontend_port="${FRONTEND_PORT:-5173}"
api_port="${API_PORT:-8080}"

echo
echo "Frontend available at: http://localhost:${frontend_port}"
echo "API available at: http://localhost:${api_port}"
