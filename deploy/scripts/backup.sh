#!/usr/bin/env bash
# Database Schema Export Script for PostgreSQL / SQLite Backup
set -e

echo "=== SK Fabricator & Erector - Database Backup Utility ==="
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_DIR="../../backup/${TIMESTAMP}"

mkdir -p "${BACKUP_DIR}"

if [ -n "${DATABASE_URL}" ]; then
  echo "Exporting PostgreSQL database from ${DATABASE_URL}..."
  pg_dump "${DATABASE_URL}" --schema-only > "${BACKUP_DIR}/schema_backup.sql"
  pg_dump "${DATABASE_URL}" --data-only > "${BACKUP_DIR}/data_backup.sql"
  echo "Backup successfully written to ${BACKUP_DIR}"
else
  echo "DATABASE_URL not set. Skipping live database dump."
fi

echo "Backup complete."
