#!/usr/bin/env bash
# 部署后冒烟：不写 presence、不需要管理员凭证；唯一副作用是非法 device 的 bootstrap 会写一条脱敏 bootstrap_reject 事件
# 用法：REMOTE=starlight ./smoke_api.sh
set -euo pipefail
REMOTE="${REMOTE:-starlight}"
ssh -o BatchMode=yes -o ConnectTimeout=10 "$REMOTE" bash -s <<'EOF'
set -uo pipefail
fails=0
chk() { # name expected actual
  if [ "$2" = "$3" ]; then echo "[ok]   $1 = $3"; else echo "[FAIL] $1 expected=$2 actual=$3"; fails=$((fails+1)); fi
}
H=$(docker exec auth-server-api-1 python -c "import urllib.request as u;print(u.urlopen('http://127.0.0.1:8000/health',timeout=3).status)" 2>/dev/null || echo 000)
chk health 200 "$H"
P=$(curl -sk --fail -o /dev/null -w '%{http_code}' https://127.0.0.1:8443/v1/portal/games || echo 000)
chk portal_games 200 "$P"
# 未带 token 访问管理接口必须 401（不写库）
A=$(curl -sk -o /dev/null -w '%{http_code}' https://127.0.0.1:8443/v1/admin/events || echo 000)
chk admin_events_no_token 401 "$A"
# 非法 device_id 的 bootstrap 仍须 200（软处理），且不得新增 presence 行
B=$(curl -sk -o /dev/null -w '%{http_code}' "https://127.0.0.1:8443/v1/bootstrap?game_id=bhzf&device_id=sqli_probe_smoke" || echo 000)
chk bootstrap_bad_device_still_200 200 "$B"
ROW=$(docker exec auth-server-db-1 psql -U starlight -d starlight -tAc "SELECT count(*) FROM online_presence WHERE device_id='sqli_probe_smoke'" | tr -d '[:space:]')
# 说明：该非法请求只会留下一条 bootstrap_reject 事件（device 指纹化），不写 presence
chk bad_device_not_persisted 0 "$ROW"
# 迁移后列数（部署前为 0，部署后应为 10）
COLS=$(docker exec auth-server-db-1 psql -U starlight -d starlight -tAc "SELECT count(*) FROM information_schema.columns WHERE (table_name='online_presence' AND column_name IN ('created_at','card_verified_at')) OR (table_name='auth_events' AND column_name IN ('result','reason','ip','ip_forwarded','ua','request_id','suppressed','last_occurred_at'))" | tr -d '[:space:]')
echo "[info] new_columns=$COLS"
echo "[smoke] fails=$fails"
[ "$fails" = "0" ]
EOF