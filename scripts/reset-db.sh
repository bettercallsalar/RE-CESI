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

echo "Stopping containers and removing volumes..."
docker_compose down -v

echo "Recreating database and restarting services..."
"$ROOT_DIR/scripts/launch.sh"
