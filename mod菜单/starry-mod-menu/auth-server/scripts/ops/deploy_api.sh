#!/usr/bin/env bash
# 精确发布 auth-server（api / nginx conf / compose）
# 用法：
#   REMOTE=starlight ./deploy_api.sh --dry-run      # 只打印 plan，**零写操作**（不备份不传文件）
#   REMOTE=starlight ./deploy_api.sh                # 快照 → 同步 → nginx -t → build → recreate api → health 重试
#   REMOTE=starlight ./deploy_api.sh --with-nginx    # 额外 recreate nginx（先归档旧容器日志）
set -euo pipefail
umask 077
REMOTE="${REMOTE:-starlight}"
ROOT="/www/wwwroot/starlight-auth/auth-server"
TS="$(date +%Y%m%d_%H%M%S)"
MODE="deploy"; WITH_NGINX=0
SLICE_RE="${SLICE_RE:-2026-11-15T11:[45][0-9]}"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry-run" ;;
    --with-nginx) WITH_NGINX=1 ;;
    *) echo "未知参数: $a" >&2; exit 2 ;;
  esac
done
SYNC="app/ static/ nginx/starlight.conf docker-compose.yml Dockerfile requirements.txt"
NEVER=".env env.example certs/ data/ keys/ backups/ starlight.db _tmp_mask_test.db"
for s in $SYNC; do
  for n in $NEVER; do
    case "$s" in "$n"*) echo "[deploy] 拒绝：$s 命中禁同步 $n" >&2; exit 3 ;; esac
  done
done
plan() {
  for s in $SYNC; do
    rsync -n -az --exclude='__pycache__' --exclude='.venv' --itemize-changes -e 'ssh -o BatchMode=yes -o ConnectTimeout=10' "$s" "$REMOTE:$ROOT/$s" | sed 's/^/[plan] /'
  done
}
if [ "$MODE" = "dry-run" ]; then
  plan
  echo '[deploy] dry-run 结束：未做任何写操作（无备份、无同步、无重启）'
  exit 0
fi
echo "[deploy] remote=$REMOTE ts=$TS with_nginx=$WITH_NGINX"
# SUBNET_CHECK：TRUSTED_PROXY_IPS 必须与实际 docker 子网一致，否则应用 fail-closed 到 nginx IP（全站共享限流桶）
SUBNET=$(ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" "docker network inspect auth-server_default -f '{{range .IPAM.Config}}{{.Subnet}}{{end}}'")
TRUSTED=$(grep -E 'TRUSTED_PROXY_IPS' docker-compose.yml | sed 's/.*:-//;s/}.*//')
echo "[deploy] subnet=$SUBNET trusted_configured=$TRUSTED"
[ "$SUBNET" = "$TRUSTED" ] || { echo "[deploy] 中止：TRUSTED_PROXY_IPS($TRUSTED) 与实际子网($SUBNET) 不一致" >&2; exit 4; }
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s <<EOF
set -euo pipefail
umask 077
cd $ROOT
D=backups/pre_$TS
mkdir -p "\$D"
tar czf "\$D/code.tar.gz" app static nginx docker-compose.yml Dockerfile requirements.txt
docker tag auth-server-api auth-server-api:rollback_$TS
docker exec auth-server-db-1 pg_dump -U starlight -Fc -d starlight > "\$D/starlight.dump"
find "\$D" -type f -exec chmod 600 {} +
echo "[deploy] snapshot=\$D"
EOF
# dump 可用性校验（独立执行，避免 heredoc 变量展开冲突；只输出条目数）
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" "ENT=\$(docker run --rm -v $ROOT/backups/pre_$TS:/b:ro postgres:16-alpine pg_restore -l /b/starlight.dump 2>/dev/null | grep -c '^[0-9]' || true); ENT=\${ENT:-0}; echo \"[deploy] dump_entries=\$ENT\"; if [ \"\$ENT\" -le 0 ]; then echo '[deploy] dump 不可读，暂停数据写' >&2; exit 5; fi"
plan
for s in $SYNC; do
  rsync -az --exclude='__pycache__' --exclude='.venv' -e 'ssh -o BatchMode=yes -o ConnectTimeout=10' "$s" "$REMOTE:$ROOT/$s"
done
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" 'docker exec auth-server-nginx-1 nginx -t'
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" "cd $ROOT && docker compose build api && docker compose up -d --no-deps --force-recreate api"
if [ "$WITH_NGINX" = "1" ]; then
  ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s -- "$TS" "$SLICE_RE" <<'EOF'
set -euo pipefail
umask 077
TS="$1"; SLICE_RE="$2"
cd /www/wwwroot/starlight-auth/auth-server
CID=$(docker inspect -f '{{.Id}}' auth-server-nginx-1)
SRC=/var/lib/docker/containers/$CID/$CID-json.log
D=backups/pre_$TS
# 零停机归档：先对活跃 json log 建硬链接（同 FS 瞬时完成，不复制、不停服）
STAMP=$(date +%H%M%S)
ARCH="$D/nginx-full-$STAMP.jsonl"
if [ -f "$SRC" ]; then
  if ln "$SRC" "$ARCH" 2>/dev/null; then
    chmod 600 "$ARCH"
    if [ "$(stat -c %i "$SRC")" = "$(stat -c %i "$ARCH")" ]; then
      echo "[deploy] hardlink OK inode=$(stat -c %i "$ARCH") size=$(stat -c %s "$ARCH")"
    else
      echo '[deploy] 硬链接 inode 校验失败，中止' >&2; exit 1
    fi
  else
    echo '[deploy] 跨文件系统，硬链接不可用：改为停容器小窗口归档（记录停机时长）' >&2
    T0=$(date +%s); docker stop auth-server-nginx-1 >/dev/null
    cp "$SRC" "$ARCH"; chmod 600 "$ARCH"
    echo "[deploy] 停机归档耗时 $(( $(date +%s) - T0 ))s"
  fi
fi
docker compose up -d --no-deps --force-recreate nginx
# recreate 后再从归档副本切关键时段（副本不再增长，受限超时，不整读原文件）
if [ -f "$ARCH" ]; then
  timeout 120 grep -E "$SLICE_RE" "$ARCH" > "$D/nginx-slice-$STAMP.jsonl" || true
  chmod 600 "$D/nginx-slice-$STAMP.jsonl" 2>/dev/null || true
  echo "[deploy] slice_lines=$(wc -l < "$D/nginx-slice-$STAMP.jsonl" 2>/dev/null || echo 0)"
fi
EOF
fi
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s <<'EOF'
set -uo pipefail
ok=0
for i in 1 2 3 4 5 6 7 8 9 10; do
  code=$(docker exec auth-server-api-1 python -c "import urllib.request as u;print(u.urlopen('http://127.0.0.1:8000/health',timeout=3).status)" 2>/dev/null || echo 000)
  echo "[health] try $i -> $code"
  if [ "$code" = "200" ]; then ok=1; break; fi
  sleep 2
done
if [ "$ok" = "0" ]; then echo '[health] FAIL 10 次重试未通过' >&2; exit 1; fi
pc=$(curl -sk --fail -o /dev/null -w '%{http_code}' https://127.0.0.1:8443/v1/portal/games || echo 000)
echo "[health] portal_games -> $pc"
[ "$pc" = "200" ] || { echo '[health] portal 冒烟失败' >&2; exit 1; }
EOF
echo "[deploy] 完成。回滚：REMOTE=$REMOTE ./rollback_api.sh rollback_$TS"