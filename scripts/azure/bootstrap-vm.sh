#!/bin/sh

set -eu

readonly APP_USER="${RECESI_APP_USER:-recesi}"
readonly APP_DIRECTORY="${RECESI_APP_DIRECTORY:-/home/recesi/recesi}"
readonly DOCKER_KEYRING_DIRECTORY="/etc/apt/keyrings"
readonly DOCKER_KEYRING_PATH="$DOCKER_KEYRING_DIRECTORY/docker.asc"
readonly DOCKER_SOURCE_PATH="/etc/apt/sources.list.d/docker.sources"

require_root() {
  if [ "$(id -u)" -eq 0 ]; then
    return
  fi

  echo "This script must run as root." >&2
  exit 1
}

install_docker() {
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    return
  fi

  export DEBIAN_FRONTEND=noninteractive
  apt-get update
  apt-get install -y ca-certificates curl

  install -m 0755 -d "$DOCKER_KEYRING_DIRECTORY"
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o "$DOCKER_KEYRING_PATH"
  chmod a+r "$DOCKER_KEYRING_PATH"

  . /etc/os-release
  docker_codename="${UBUNTU_CODENAME:-$VERSION_CODENAME}"
  docker_architecture="$(dpkg --print-architecture)"

  cat >"$DOCKER_SOURCE_PATH" <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $docker_codename
Components: stable
Architectures: $docker_architecture
Signed-By: $DOCKER_KEYRING_PATH
EOF

  apt-get update
  apt-get install -y \
    docker-ce \
    docker-ce-cli \
    containerd.io \
    docker-buildx-plugin \
    docker-compose-plugin
}

configure_application_user() {
  if ! id "$APP_USER" >/dev/null 2>&1; then
    echo "Application user does not exist: $APP_USER" >&2
    exit 1
  fi

  usermod -aG docker "$APP_USER"
  install -d -o "$APP_USER" -g "$APP_USER" -m 0750 "$APP_DIRECTORY"
}

main() {
  require_root
  install_docker
  systemctl enable --now docker
  configure_application_user

  docker --version
  docker compose version
}

main "$@"
