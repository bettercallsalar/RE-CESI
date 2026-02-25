#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

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

docker-compose up -d --build
docker-compose ps
