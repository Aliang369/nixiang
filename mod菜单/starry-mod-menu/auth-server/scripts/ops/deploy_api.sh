#!/usr/bin/env bash
# 精确发布 auth-server 到 **宝塔 + systemd + MySQL** 实例（2026-11-24 重写）
#
# 为什么重写：原版是 Docker 方案（同步 nginx/starlight.conf + docker-compose.yml + Dockerfile、
# 靠 docker 镜像 tag 回滚、健康检查打 8443）。Docker 整套已归档到 ../_legacy_docker/，
# 那些路径在仓库里已不存在，旧脚本第一行 rsync 就会失败。现行部署口径见 docs/DEPLOY.md。
#
# 用法：
#   ./deploy_api.sh --dry-run                 # 只打印要执行的命令，不连远端、零写操作
#   ./deploy_api.sh                           # 远端快照 → 同步 app/ static/ → 重启 → /health
#   ./deploy_api.sh --with-reqs               # 额外同步 requirements.txt 并在远端 .venv 里 pip install
#   REMOTE=root@154.94.232.231 ./deploy_api.sh
#   ROOT=/www/wwwroot/starlight-mod-auth SVC=starlight-mod-auth PORT=8010 OWNER=www:www ./deploy_api.sh
#
# 不做的事（刻意的）：不碰 .env / certs / data / keys / backups / *.db / .venv；
# 不建数据库表（应用 ensure_schema 幂等补列，改列仍须手工 ALTER TABLE）；不动 nginx vhost。
set -euo pipefail
umask 077

REMOTE="${REMOTE:-starlight}"
ROOT="${ROOT:-/www/wwwroot/starlight-mod-auth}"
SVC="${SVC:-starlight-mod-auth}"
PORT="${PORT:-8010}"
OWNER="${OWNER:-www:www}"
PUBLIC_HEALTH="${PUBLIC_HEALTH:-https://auth.xgmfyx.cn/health}"
TS="$(date +%Y%m%d_%H%M%S)"
MODE="deploy"
WITH_REQS=0
SSH_OPTS="-o BatchMode=yes -o ConnectTimeout=10"

for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry-run" ;;
    --with-reqs) WITH_REQS=1 ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $a" >&2; exit 2 ;;
  esac
done

SYNC="app static"
[ "$WITH_REQS" = "1" ] && SYNC="$SYNC requirements.txt"

# 禁同步清单：生产密钥/数据/备份/虚拟环境一律不上传、不覆盖
NEVER=".env env.example certs data keys backups starlight.db _tmp_mask_test.db .venv"
for s in $SYNC; do
  for n in $NEVER; do
    case "$s" in "$n"*) echo "[deploy] 拒绝：$s 命中禁同步 $n" >&2; exit 3 ;; esac
  done
done

SNAP_CMD="cd '$ROOT' && mkdir -p backups/pre_$TS && tar czf backups/pre_$TS/code.tar.gz $SYNC && find backups/pre_$TS -type f -exec chmod 600 {} + && echo '[deploy] snapshot=backups/pre_$TS'"

if [ "$MODE" = "dry-run" ]; then
  echo "[deploy] dry-run（不连远端、零写操作）"
  echo "  remote = $REMOTE"
  echo "  root   = $ROOT"
  echo "  svc    = $SVC   port = $PORT   owner = $OWNER"
  echo "  1) ssh $REMOTE \"$SNAP_CMD\""
  for s in $SYNC; do
    echo "  2) rsync -az --exclude=__pycache__ --exclude=.venv -e 'ssh $SSH_OPTS' $s $REMOTE:$ROOT/$s"
  done
  echo "  3) ssh $REMOTE \"chown -R $OWNER '$ROOT/app' '$ROOT/static'\""
  [ "$WITH_REQS" = "1" ] && echo "  4) ssh $REMOTE \"cd '$ROOT' && .venv/bin/pip install -q -r requirements.txt\""
  echo "  5) ssh $REMOTE \"systemctl restart $SVC\" → 轮询 http://127.0.0.1:$PORT/health 至 200"
  echo "  6) curl -s $PUBLIC_HEALTH"
  exit 0
fi

echo "[deploy] remote=$REMOTE root=$ROOT svc=$SVC ts=$TS with_reqs=$WITH_REQS"

# 0) 前置校验：目录、入口、systemd 单元
ssh $SSH_OPTS "$REMOTE" "set -e
  test -d '$ROOT' || { echo '[deploy] 远端目录不存在: $ROOT' >&2; exit 3; }
  test -f '$ROOT/app/main.py' || { echo '[deploy] 远端缺 app/main.py，拒绝继续' >&2; exit 3; }
  if systemctl list-unit-files '$SVC.service' | grep -q '$SVC.service'; then echo '[deploy] unit=ok'; else echo '[deploy] 中止：$SVC.service 不存在' >&2; exit 3; fi"

# 1) 远端快照（代码回滚靠它：tar 解回 app/ static/ 再 restart）
ssh $SSH_OPTS "$REMOTE" "$SNAP_CMD"

# 2) 同步（逐项，保留远端其余文件；不带 --delete，避免误删远端运行期产物）
for s in $SYNC; do
  rsync -az --exclude='__pycache__' --exclude='.venv' -e "ssh $SSH_OPTS" "$s" "$REMOTE:$ROOT/$s"
done

# 3) 权限
ssh $SSH_OPTS "$REMOTE" "chown -R $OWNER '$ROOT/app' '$ROOT/static'"

# 4) 依赖（仅 --with-reqs）
if [ "$WITH_REQS" = "1" ]; then
  ssh $SSH_OPTS "$REMOTE" "set -e; cd '$ROOT'; test -x .venv/bin/pip || { echo '[deploy] 远端 .venv/bin/pip 不存在' >&2; exit 4; }; .venv/bin/pip install -q -r requirements.txt && echo '[deploy] deps=ok'"
fi

# 5) 重启 + 健康轮询（回环 8010，避免 DNS/TLS 干扰）
ssh $SSH_OPTS "$REMOTE" "systemctl restart $SVC"
code="000"
i=1
while [ "$i" -le 15 ]; do
  code="$(ssh $SSH_OPTS "$REMOTE" "curl -s -o /dev/null -w '%{http_code}' http://127.0.0.1:$PORT/health || true" | tr -d '[:space:]')"
  [ "$code" = "200" ] && break
  sleep 2
  i=$((i + 1))
done
if [ "$code" != "200" ]; then
  echo "[deploy] 失败：/health 未返回 200（last=${code}）。回滚：ssh $REMOTE \"cd $ROOT && tar xzf backups/pre_$TS/code.tar.gz && systemctl restart $SVC\"" >&2
  exit 5
fi
echo "[deploy] health=200"

# 6) 公网自检（只读）
echo "[deploy] public: $(curl -s -m 10 -o /dev/null -w '%{http_code}' "$PUBLIC_HEALTH" || echo 000)  $PUBLIC_HEALTH"
echo "[deploy] done ts=$TS"
