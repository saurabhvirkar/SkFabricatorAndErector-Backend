#!/usr/bin/env bash
# Automatic Rollback Script
set -e

ROLLBACK_IMAGE="$1"
COMPOSE_FILE="${2:-docker-compose.prod.yml}"

echo "=== Rolling back to image: ${ROLLBACK_IMAGE} ==="

if [ -z "${ROLLBACK_IMAGE}" ]; then
  echo "Error: Rollback target image not specified."
  exit 1
fi

export IMAGE_TAG=$(echo "${ROLLBACK_IMAGE}" | awk -F':' '{print $2}')

docker compose -f "${COMPOSE_FILE}" up -d --remove-orphans
echo "Rollback completed."
