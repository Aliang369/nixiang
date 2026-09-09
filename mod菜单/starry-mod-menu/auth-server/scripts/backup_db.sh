#!/usr/bin/env bash
# 星光验证库备份（Docker Postgres 或本机 SQLite）
# 用法：
#   ./scripts/backup_db.sh
#   KEEP_DAYS=14 ./scripts/backup_db.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_DIR="${BACKUP_DIR:-$ROOT/backups}"
KEEP_DAYS="${KEEP_DAYS:-14}"
STAMP="$(date +%Y%m%d_%H%M%S)"
mkdir -p "$OUT_DIR"

cd "$ROOT"

backup_postgres() {
  local file="$OUT_DIR/starlight_pg_${STAMP}.sql.gz"
  echo "[backup] Postgres → $file"
  docker compose exec -T db pg_dump -U starlight -d starlight --no-owner --no-acl \
    | gzip -c > "$file"
  echo "[backup] OK $(du -h "$file" | awk '{print $1}')"
}

backup_sqlite() {
  local src="${SQLITE_PATH:-$ROOT/starlight.db}"
  if [[ ! -f "$src" ]]; then
    echo "[backup] 未找到 SQLite: $src" >&2
    return 1
  fi
  local file="$OUT_DIR/starlight_sqlite_${STAMP}.db.gz"
  echo "[backup] SQLite → $file"
  gzip -c "$src" > "$file"
  echo "[backup] OK $(du -h "$file" | awk '{print $1}')"
}

if docker compose ps --status running 2>/dev/null | grep -qE '[[:space:]]db[[:space:]]'; then
  backup_postgres
elif [[ -f "${SQLITE_PATH:-$ROOT/starlight.db}" ]]; then
  backup_sqlite
else
  echo "[backup] 未检测到运行中的 db 容器，也没有 starlight.db" >&2
  echo "  Docker: 先 docker compose up -d" >&2
  echo "  SQLite: SQLITE_PATH=/path/to.db $0" >&2
  exit 1
fi

# 清理过期备份
find "$OUT_DIR" -type f \( -name 'starlight_pg_*.sql.gz' -o -name 'starlight_sqlite_*.db.gz' \) \
  -mtime "+${KEEP_DAYS}" -print -delete 2>/dev/null || true
echo "[backup] 保留 ${KEEP_DAYS} 天内的文件于 $OUT_DIR"
