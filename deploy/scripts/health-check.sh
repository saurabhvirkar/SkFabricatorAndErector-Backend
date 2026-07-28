#!/usr/bin/env bash
# Automated Health Probe Verification Script
TARGET_URL="${1:-http://localhost:8080/health/live}"
MAX_RETRIES="${2:-10}"
DELAY="${3:-3}"

echo "Checking health probe at ${TARGET_URL}..."

for ((i=1; i<=MAX_RETRIES; i++)); do
  HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${TARGET_URL}" || echo "000")
  if [ "${HTTP_STATUS}" -eq 200 ]; then
    echo "Health probe PASSED (HTTP 200) on attempt ${i}."
    exit 0
  fi
  echo "Attempt ${i}/${MAX_RETRIES}: Received HTTP ${HTTP_STATUS}. Waiting ${DELAY}s..."
  sleep "${DELAY}"
done

echo "Health probe FAILED after ${MAX_RETRIES} attempts."
exit 1
