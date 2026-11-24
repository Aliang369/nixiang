#!/usr/bin/env bash
# 按已取证 CSV 的固定 id 清单清理注入 presence（断言 357，不符回滚）
# 用法：REMOTE=starlight ./run_purge.sh <backup_dir>
set -euo pipefail
umask 077
REMOTE="${REMOTE:-starlight}"
DIR="$(cd "$(dirname "$0")" && pwd)"
BDIR="${1:-}"
[ -n "$BDIR" ] || { echo '用法: run_purge.sh /www/.../backups/pre_<TS>' >&2; exit 2; }
SQLF="$DIR/sql/purge_payload_presence.sql"
[ -r "$SQLF" ] || { echo "缺少 $SQLF" >&2; exit 3; }
# 1) 从取证 CSV 提取 id（第 1 列），生成清单
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" "set -e; cd $BDIR; test -f abnormal_presence_bhzf.csv || { echo '缺少取证 CSV' >&2; exit 4; }; tail -n +2 abnormal_presence_bhzf.csv | cut -d, -f1 | tr -d '\"' > /tmp/purge_ids.csv; wc -l < /tmp/purge_ids.csv"
# 2) 送 SQL 与被清理清单进 db 容器
cat "$SQLF" | ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'cat > /tmp/purge.sql; chmod 600 /tmp/purge.sql'
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'docker cp /tmp/purge_ids.csv auth-server-db-1:/tmp/purge_ids.csv >/dev/null && docker exec auth-server-db-1 chown postgres:postgres /tmp/purge_ids.csv && docker exec auth-server-db-1 chmod 644 /tmp/purge_ids.csv && echo ids_in_container=OK'
# 3) 事务执行（SQL 内断言 357）
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'docker exec -i auth-server-db-1 psql -q -U starlight -d starlight -v ON_ERROR_STOP=1 -A -F"|" -f - < /tmp/purge.sql'
# 4) 清理临时文件
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'rm -f /tmp/purge.sql; docker exec auth-server-db-1 rm -f /tmp/purge_ids.csv'