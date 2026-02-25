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

docker_compose up -d --build
docker_compose ps
