#!/usr/bin/env bash
# Deployment Script with Health Check & Automatic Rollback
set -e

NEW_TAG="$1"
COMPOSE_FILE="${2:-docker-compose.prod.yml}"

if [ -z "${NEW_TAG}" ]; then
  echo "Error: Deployment image tag must be specified."
  exit 1
fi

echo "=== Deploying Version: ${NEW_TAG} ==="
CURRENT_TAG=$(docker inspect --format='{{.Config.Image}}' skfabricator-api-prod 2>/dev/null || echo "")

export IMAGE_TAG="${NEW_TAG}"

docker compose -f "${COMPOSE_FILE}" pull
docker compose -f "${COMPOSE_FILE}" up -d --remove-orphans

echo "Performing health verification..."
if ./health-check.sh http://localhost:8080/health/live 10 3; then
  echo "=== Deployment Successful! Tag: ${NEW_TAG} ==="
else
  echo "🚨 Health check failed! Initiating Automatic Rollback..."
  if [ -n "${CURRENT_TAG}" ]; then
    ./rollback.sh "${CURRENT_TAG}" "${COMPOSE_FILE}"
  else
    echo "No prior container version available for rollback."
  fi
  exit 1
fi
