#!/usr/bin/env bash
# 回滚 api 镜像 + 代码/配置/static（不动数据库结构、不动 .env/certs/data/keys）
# 用法：REMOTE=starlight ./rollback_api.sh rollback_YYYYmmdd_HHMMSS
set -euo pipefail
umask 077
REMOTE="${REMOTE:-starlight}"
TAG="${1:-}"
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s -- "$TAG" <<'EOF'
set -euo pipefail
umask 077
cd /www/wwwroot/starlight-auth/auth-server
TAG="${1:-}"
if [ -z "$TAG" ]; then
  TAG=$(docker images --format '{{.Tag}}' auth-server-api | grep '^rollback_' | sort | tail -1)
fi
[ -z "$TAG" ] && { echo '[rollback] 无可用 rollback tag' >&2; exit 1; }
TS="${TAG#rollback_}"
D="backups/pre_$TS"
[ -d "$D" ] || { echo "[rollback] 缺少备份目录 $D" >&2; exit 2; }
[ -f "$D/code.tar.gz" ] || { echo "[rollback] 缺少 $D/code.tar.gz" >&2; exit 2; }
# 1) 镜像 tag 必须存在
docker image inspect "auth-server-api:$TAG" >/dev/null || { echo "[rollback] 镜像 tag 不存在: $TAG" >&2; exit 3; }
# 2) tar 先解到临时目录并校验关键文件（此时**尚未停服**）
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
tar xzf "$D/code.tar.gz" -C "$TMP"
[ -f "$TMP/app/main.py" ] || { echo '[rollback] 快照缺少 app/main.py，中止' >&2; exit 4; }
[ -d "$TMP/static" ] || { echo '[rollback] 快照缺少 static，中止' >&2; exit 4; }
echo "[rollback] 快照校验通过：$(find "$TMP/app" -name '*.py' | wc -l) py / $(find "$TMP/static" -type f | wc -l) static"
# 3) 停服前挂 ERR trap，任何失败都尝试拉起
trap 'echo "[rollback] 异常，尝试恢复服务" >&2; docker start auth-server-api-1 >/dev/null 2>&1 || true; rm -rf "$TMP"' ERR
docker stop auth-server-api-1 >/dev/null
cp -a "$TMP/app/." ./app/
cp -a "$TMP/static/." ./static/
[ -f "$TMP/docker-compose.yml" ] && cp -a "$TMP/docker-compose.yml" ./docker-compose.yml
[ -f "$TMP/nginx/starlight.conf" ] && cp -a "$TMP/nginx/starlight.conf" ./nginx/starlight.conf
echo '[rollback] 代码/配置/static 已恢复'
docker tag "auth-server-api:$TAG" auth-server-api:latest
docker exec auth-server-nginx-1 nginx -t && docker exec auth-server-nginx-1 nginx -s reload || true
docker compose up -d --no-deps --force-recreate api
ok=0
for i in 1 2 3 4 5 6 7 8 9 10; do
  code=$(docker exec auth-server-api-1 python -c "import urllib.request as u;print(u.urlopen('http://127.0.0.1:8000/health',timeout=3).status)" 2>/dev/null || echo 000)
  echo "[health] try $i -> $code"
  if [ "$code" = "200" ]; then ok=1; break; fi
  sleep 2
done
[ "$ok" = "1" ] || { echo '[rollback] health FAIL' >&2; exit 1; }
echo '[rollback] 完成：镜像+代码已回退，DB 新列与新事件保留'
EOF