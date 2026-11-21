#!/usr/bin/env bash
# 星光验证库备份（MySQL 生产 / 本机 SQLite 开发）
# 用法：
#   ./scripts/backup_db.sh
#   KEEP_DAYS=14 ./scripts/backup_db.sh
#   SQLITE_PATH=/path/to.db ./scripts/backup_db.sh
#
# 生产连接串从 auth-server/.env 的 DATABASE_URL 解析，无需额外配置。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_DIR="${BACKUP_DIR:-$ROOT/backups}"
KEEP_DAYS="${KEEP_DAYS:-14}"
STAMP="$(date +%Y%m%d_%H%M%S)"
mkdir -p "$OUT_DIR"

cd "$ROOT"

ENV_FILE="${ENV_FILE:-$ROOT/.env}"
DATABASE_URL="${DATABASE_URL:-}"
if [[ -z "$DATABASE_URL" && -f "$ENV_FILE" ]]; then
  DATABASE_URL="$(grep -E '^DATABASE_URL=' "$ENV_FILE" | head -1 | cut -d= -f2- || true)"
fi

backup_mysql() {
  # 形如 mysql+pymysql://user:pass@127.0.0.1:3306/dbname?charset=utf8mb4
  local url="${1#mysql+*://}"
  local creds="${url%%@*}"
  local hostpart="${url#*@}"
  local user="${creds%%:*}"
  local pass="${creds#*:}"
  local hostport="${hostpart%%/*}"
  local db="${hostpart#*/}"
  db="${db%%\?*}"
  local host="${hostport%%:*}"
  local port="${hostport##*:}"
  [[ "$port" == "$host" ]] && port=3306

  local file="$OUT_DIR/starlight_mysql_${db}_${STAMP}.sql.gz"
  echo "[backup] MySQL ${user}@${host}:${port}/${db} → $file"
  MYSQL_PWD="$pass" mysqldump \
    -h "$host" -P "$port" -u "$user" \
    --single-transaction --quick --no-tablespaces \
    --default-character-set=utf8mb4 \
    "$db" | gzip -c > "$file"
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

case "$DATABASE_URL" in
  mysql*)
    backup_mysql "$DATABASE_URL"
    ;;
  sqlite*)
    backup_sqlite
    ;;
  *)
    if [[ -f "${SQLITE_PATH:-$ROOT/starlight.db}" ]]; then
      backup_sqlite
    else
      echo "[backup] 未识别 DATABASE_URL，也未找到 starlight.db" >&2
      echo "  MySQL:  在 .env 配置 DATABASE_URL=mysql+pymysql://..." >&2
      echo "  SQLite: SQLITE_PATH=/path/to.db $0" >&2
      exit 1
    fi
    ;;
esac

# 清理过期备份
find "$OUT_DIR" -type f \( -name 'starlight_mysql_*.sql.gz' -o -name 'starlight_sqlite_*.db.gz' \) \
  -mtime "+${KEEP_DAYS}" -print -delete 2>/dev/null || true
echo "[backup] 保留 ${KEEP_DAYS} 天内的文件于 $OUT_DIR"
