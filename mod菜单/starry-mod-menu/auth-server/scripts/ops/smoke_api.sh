#!/usr/bin/env bash
# 部署后冒烟（宝塔 + systemd 实例，2026-11-24 重写）
#
# 旧版打 docker exec + psql + 8443 自签证书 + /v1/portal/games（该路由已随 portal.py 删除），
# 现在全部不成立；旧版归档在 ../_legacy_docker/ops/smoke_api.sh，仅供追溯。
#
# 只读：不写 presence、不建票、不带 token。唯一网络副作用是几条 GET。
# 用法：./smoke_api.sh        （REMOTE / PORT / BASE 可覆盖）
#       BASE=https://auth.xgmfyx.cn REMOTE=root@1.2.3.4 ./smoke_api.sh
set -uo pipefail

REMOTE="${REMOTE:-starlight}"
PORT="${PORT:-8010}"
BASE="${BASE:-https://auth.xgmfyx.cn}"
SSH_OPTS="-o BatchMode=yes -o ConnectTimeout=10"
fails=0

chk() { # name expected actual
  if [ "$2" = "$3" ]; then
    echo "[ok]   $1 = $3"
  else
    echo "[FAIL] $1 expected=$2 actual=$3"
    fails=$((fails + 1))
  fi
}
local_code() { # path
  ssh $SSH_OPTS "$REMOTE" "curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://127.0.0.1:$PORT$1 || echo 000" | tr -d '[:space:]'
}
pub_code() { # path
  curl -s -o /dev/null -w '%{http_code}' --max-time 10 "$BASE$1" || echo 000
}

echo "== 本机回环 127.0.0.1:${PORT}（经 ssh ${REMOTE}）=="
chk health 200 "$(local_code /health)"
chk root_404 404 "$(local_code /)"
chk docs_off 404 "$(local_code /docs)"

echo "== 公网 $BASE =="
chk public_health 200 "$(pub_code /health)"
chk admin_ui 200 "$(pub_code /admin/)"
chk admin_api_no_token 401 "$(pub_code /v1/admin/events)"
chk ota_removed 404 "$(pub_code /v1/ota/build)"
chk portal_removed 404 "$(pub_code /v1/portal/games)"

echo "== 结果 =="
if [ "$fails" -eq 0 ]; then
  echo "[smoke] 全过"
  exit 0
fi
echo "[smoke] $fails 项不符，先看 systemctl status 与 /www/wwwlogs/starlight-mod-auth.*.log" >&2
exit 1
