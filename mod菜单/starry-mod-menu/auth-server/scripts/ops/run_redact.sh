#!/usr/bin/env bash
# 分批脱敏 runner：每批独立事务、按 id 区间推进、可中断可重入
# 用法：REMOTE=starlight ./run_redact.sh equal|substring|verify
set -euo pipefail
umask 077
REMOTE="${REMOTE:-starlight}"
PHASE="${1:-verify}"
BATCH="${BATCH:-50000}"
DIR="$(cd "$(dirname "$0")" && pwd)"
case "$PHASE" in
  equal) SQL="$DIR/sql/redact_step_equal.sql" ;;
  substring) SQL="$DIR/sql/redact_step_substring.sql" ;;
  verify) SQL="$DIR/sql/redact_verify.sql" ;;
  *) echo '用法: run_redact.sh equal|substring|verify' >&2; exit 2 ;;
esac
[ -r "$SQL" ] || { echo "缺少 SQL: $SQL" >&2; exit 3; }
# 只把 SQL 原文经 stdin 送到远端临时文件（不嵌 base64、不截断）
cat "$SQL" | ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'umask 077; cat > /tmp/redact_step.sql; chmod 600 /tmp/redact_step.sql; echo "[sync] bytes=$(wc -c < /tmp/redact_step.sql)"'
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s -- "$PHASE" "$BATCH" <<'REMOTE_EOF'
set -euo pipefail
PHASE="$1"; BATCH="$2"; F=/tmp/redact_step.sql
PSQL="docker exec -i auth-server-db-1 psql -q -U starlight -d starlight"
PSQL_NOI="docker exec auth-server-db-1 psql -q -U starlight -d starlight"
if [ "$PHASE" = "verify" ]; then
  $PSQL -A -F'|' -f - < "$F"; rm -f "$F"; exit 0
fi
MAXID=$($PSQL_NOI -tAc 'SELECT coalesce(max(id),0) FROM auth_events' | tr -d '[:space:]')
case "$MAXID" in ''|*[!0-9]*) echo "MAXID 非法: $MAXID" >&2; exit 1 ;; esac
echo "[redact:$PHASE] max_id=$MAXID batch=$BATCH"
lo=0; total=0; batches=0
while [ "$lo" -le "$MAXID" ]; do
  hi=$((lo+BATCH))
  out=$($PSQL -v lo="$lo" -v hi="$hi" -tAf - < "$F" | tr -d '[:space:]' | grep -xE '[0-9]+' || true)
  case "$out" in ''|*[!0-9]*) echo "affected 非法(lo=$lo hi=$hi): [$out]" >&2; exit 1 ;; esac
  total=$((total+out)); batches=$((batches+1))
  echo "[redact:$PHASE] [$lo,$hi) affected=$out cumulative=$total"
  lo=$hi
done
echo "[redact:$PHASE] DONE batches=$batches total=$total"
rm -f "$F"
exit 0
REMOTE_EOF
