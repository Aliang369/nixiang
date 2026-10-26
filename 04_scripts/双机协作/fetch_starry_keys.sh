#!/usr/bin/env bash
# ============================================================================
# fetch_starry_keys.sh — 新机器（mac）落地构建机密钥 + 三项自证（bash 3.2 兼容）
#
# 背景：出包机必须持有 ~/.starry-keys/{rsa2048.txt,payload_master.txt,build_key.txt}
#       才能算出与服务端**同一把** K_build / 签同一个公钥指纹。mac 端没有这三份
#       ⇒ gen_payload_enc.py / gen_pubkey_header.py / harden_apk.py C9 全部不可用。
#
# 通道：**经 SSH 从授权服务器取，绝不走 git**。私钥入库后即使后续提交删除，
#       blob 也永久留在历史里（两台机器 .git 各一份），清理要 filter-repo+强推，
#       属 AGENTS.md §5 事故级禁区。
#
# 用法：
#   bash 04_scripts/双机协作/fetch_starry_keys.sh              # 取 + 三项自证
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --verify     # 只自证（已取过）
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --dry-run    # 只看要取什么
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --force      # 覆盖已存在的密钥
#
# 环境变量（一般不用改）：
#   SG_SSH_HOST    授权服务器 SSH 目标     默认 root@47.100.216.101
#   SG_SERVER_DIR  服务端 auth-server 目录  默认 /www/wwwroot/starlight-auth/auth-server
#   SG_GAME        用于自证的 game_id       默认 yszzx（勇士之心）
#   SG_API_BASE    v3/payload-status 基址   默认 https://47.100.216.101:8443
#   SG_EXPECT_FP   期望公钥指纹             默认 25ba10e0faca3500
#   SG_EXPECT_KFP  期望 K_build 指纹        默认 d63013cd39692ddd
#
# 退出码：0 = 三项全 PASS / 1 = 有 FAIL / 2 = 只 WARN（未取齐但没硬错）
#
# 安全：落地目录 700、文件 600；脚本不打印任何密钥内容，只打印指纹。
# ============================================================================
set -uo pipefail

SSH_HOST="${SG_SSH_HOST:-root@47.100.216.101}"
SERVER_DIR="${SG_SERVER_DIR:-/www/wwwroot/starlight-auth/auth-server}"
GAME="${SG_GAME:-yszzx}"
API_BASE="${SG_API_BASE:-https://47.100.216.101:8443}"
EXPECT_FP="${SG_EXPECT_FP:-25ba10e0faca3500}"
EXPECT_KFP="${SG_EXPECT_KFP:-d63013cd39692ddd}"

DO_FETCH=1
FORCE=0
DRY=0

PASS_N=0; WARN_N=0; FAIL_N=0
pass() { PASS_N=$((PASS_N + 1)); printf '[PASS] %s\n' "$*"; }
warn() { WARN_N=$((WARN_N + 1)); printf '[WARN] %s\n' "$*"; }
fail() { FAIL_N=$((FAIL_N + 1)); printf '[FAIL] %s\n' "$*"; }
info() { printf '       %s\n' "$*"; }
sect() { printf '\n== %s ==\n' "$*"; }

for arg in "$@"; do
  case "$arg" in
    --verify)  DO_FETCH=0 ;;
    --force)   FORCE=1 ;;
    --dry-run) DRY=1; DO_FETCH=0 ;;
    -h|--help) sed -n '2,32p' "$0"; exit 0 ;;
    *) printf '未知参数：%s（--help 看用法）\n' "$arg" >&2; exit 2 ;;
  esac
done

case "$(uname -s)" in
  Darwin) OS=mac ;;
  MINGW*|MSYS*|CYGWIN*) OS=win ;;
  *) OS=unix ;;
esac

ROOT="$(git rev-parse --show-toplevel 2>/dev/null || true)"
if [ -z "$ROOT" ] || [ ! -f "$ROOT/AGENTS.md" ]; then
  printf '[FAIL] 不在 nixiang 仓库内（先 cd 到仓库根，或设 NIXIANG_ROOT）\n' >&2
  exit 1
fi

KD="$HOME/.starry-keys"
mkdir -p "$KD" || { printf '[FAIL] 建不了 %s\n' "$KD" >&2; exit 1; }
chmod 700 "$KD" 2>/dev/null || true

sha256_of() {
  python3 - "$1" <<'PY'
import hashlib, sys
print(hashlib.sha256(open(sys.argv[1], "rb").read()).hexdigest())
PY
}

printf '宿主=%s  仓库=%s\n密钥目录=%s\n自证目标 game_id=%s\n' "$OS" "$ROOT" "$KD" "$GAME"

# ---------------------------------------------------------------------------
# 1) 取：SSH 直连授权服务器
# ---------------------------------------------------------------------------
sect "1/4 取密钥（SSH 直连，走 git 一律禁止）"

need_fetch=0
for f in rsa2048.txt payload_master.txt build_key.txt; do
  if [ -f "$KD/$f" ] && [ "$FORCE" -eq 0 ]; then
    info "已有 $KD/${f}（--force 才覆盖）"
  else
    need_fetch=1
  fi
done

if [ "$need_fetch" -eq 0 ]; then
  pass "三份密钥已在位，跳过取文件"
elif [ "$DO_FETCH" -eq 0 ]; then
  warn "缺文件但当前是 --verify/--dry-run ⇒ 不取。先跑不带参数的版本"
else
  if ! command -v ssh >/dev/null 2>&1; then
    fail "没有 ssh（mac 自带；win 端用 Git Bash）"
  elif ! ssh -o BatchMode=yes -o ConnectTimeout=8 "$SSH_HOST" true >/dev/null 2>&1; then
    fail "SSH 连不上 ${SSH_HOST}（钥匙/网络？先手工 ssh 确认）"
    info "备选通道：Syncthing 仓外目录中转，或让出包机侧人工拷 U 盘——仍不要经过 git"
  else
    pass "SSH 可达 $SSH_HOST"
    fetch_one() {
      local _remote="$1" _local="$2" _label="$3" _tmp
      _tmp="$KD/.tmp.$$.$_label"
      if [ "$DRY" -eq 1 ]; then
        info "将取 $SSH_HOST:$_remote  →  $_local"
        return 0
      fi
      if ( umask 077; ssh -o BatchMode=yes -o ConnectTimeout=8 "$SSH_HOST" "cat '$_remote'" > "$_tmp" 2>/dev/null ); then
        :
      fi
      if [ ! -s "$_tmp" ]; then
        rm -f "$_tmp"
        fail "取不到 $_remote"
        return 1
      fi
      mv -f "$_tmp" "$_local" && chmod 600 "$_local" 2>/dev/null
      pass "$_label 落地（$(sha256_of "$_local" | cut -c1-16)…）"
      return 0
    }

    fetch_one "$SERVER_DIR/keys/rsa2048.txt"        "$KD/rsa2048.txt"        "rsa2048.txt" || true
    fetch_one "$SERVER_DIR/keys/payload_master.txt" "$KD/payload_master.txt" "payload_master.txt" || true

    if [ "$DRY" -eq 1 ]; then
      info "将从 $SERVER_DIR/.env 提取 BUILD_API_KEY"
    else
      _bk="$(ssh -o BatchMode=yes -o ConnectTimeout=8 "$SSH_HOST" \
             "grep -m1 '^BUILD_API_KEY=' '$SERVER_DIR/.env' | cut -d= -f2-" 2>/dev/null | tr -d '\r"'"'"' ')"
      if [ -n "$_bk" ]; then
        ( umask 077; printf '%s\n' "$_bk" > "$KD/build_key.txt" )
        chmod 600 "$KD/build_key.txt" 2>/dev/null
        pass "build_key.txt 落地（长度 ${#_bk}）"
      else
        fail "服务端 .env 没有 BUILD_API_KEY（留空=接口关闭）⇒ harden C9 会跳过联网校验"
      fi
      unset _bk
    fi

    if [ ! -f "$KD/api_base.txt" ]; then
      ( umask 077; printf '%s\n' "$API_BASE" > "$KD/api_base.txt" )
      info "补写 api_base.txt = $API_BASE"
    fi
  fi
fi

# ---------------------------------------------------------------------------
# 2) 自证 A：公钥指纹与仓内 sg_pubkey.h 一致
# ---------------------------------------------------------------------------
sect "2/4 自证 A — 公钥指纹 == 仓内 kSgPubFp"

if [ ! -f "$KD/rsa2048.txt" ]; then
  fail "缺 $KD/rsa2048.txt"
else
  PUB_OUT="$(python3 "$ROOT/04_scripts/_common/sg_pki/gen_pubkey_header.py" --check 2>&1)"
  PUB_RC=$?
  printf '%s\n' "$PUB_OUT" | sed 's/^/       /'
  if [ "$PUB_RC" -eq 0 ]; then
    case "$PUB_OUT" in
      *"$EXPECT_FP"*) pass "公钥一致 fp=${EXPECT_FP}（与服务端同钥）" ;;
      *) pass "公钥与仓内一致（但指纹不是预期 ${EXPECT_FP}，核对是否换了钥）" ;;
    esac
  else
    fail "公钥不一致（rc=${PUB_RC}）⇒ 服务器与客户端验签对不上，所有人登不进"
  fi
fi

# ---------------------------------------------------------------------------
# 3) 自证 B：自算 K_build 指纹
# ---------------------------------------------------------------------------
sect "3/4 自证 B — 自算 $GAME 的 K_build"

PAYLOAD_SHA=""
KFP=""
if [ ! -f "$KD/payload_master.txt" ]; then
  fail "缺 $KD/payload_master.txt ⇒ 算不出 K_build"
else
  KB_OUT="$(python3 - "$ROOT" <<'PY' 2>&1
import importlib.util, os, sys
root = sys.argv[1]
path = os.path.join(root, "04_scripts", "勇士之心", "gen_payload_enc.py")
spec = importlib.util.spec_from_file_location("gpe", path)
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
payload = m.extract_payload()
master = m.load_master(create=False)
key, digest = m.build_key(master, payload)
print("payload_sha=%s" % digest.hex())
print("k_build_fp=%s" % key.hex()[:16])
PY
)"
  printf '%s\n' "$KB_OUT" | sed 's/^/       /'
  PAYLOAD_SHA="$(printf '%s\n' "$KB_OUT" | sed -n 's/^payload_sha=//p' | head -1)"
  KFP="$(printf '%s\n' "$KB_OUT" | sed -n 's/^k_build_fp=//p' | head -1)"
  if [ -z "$KFP" ]; then
    fail "算不出 K_build（主密钥格式不对？应是 64 位 hex）"
  elif [ "$KFP" = "$EXPECT_KFP" ]; then
    pass "K_build 指纹 = ${KFP}（与服务端登记值一致）"
  else
    fail "K_build 指纹 = ${KFP}，与预期 $EXPECT_KFP 不符 ⇒ 主密钥不是同一份，或该游戏载荷已改动"
    info "若本轮确实改过载荷/重新加密，预期值应为新值：先看 sg_payload_enc.h 里的 sha 再对后台"
  fi

  CHK_OUT="$(python3 "$ROOT/04_scripts/勇士之心/gen_payload_enc.py" --check 2>&1)"
  CHK_RC=$?
  printf '%s\n' "$CHK_OUT" | sed 's/^/       /'
  if [ "$CHK_RC" -eq 0 ]; then
    pass "密文头与当前明文一致"
  else
    warn "密文头与当前载荷不一致（要先跑 gen_payload_enc.py 重新加密，不是密钥问题）"
  fi
fi

# ---------------------------------------------------------------------------
# 4) 自证 C：服务端 payload-status match=true 且 k_build_fp 一致
# ---------------------------------------------------------------------------
sect "4/4 自证 C — 服务端 payload-status（需要网络 + build_key）"

if [ ! -f "$KD/build_key.txt" ]; then
  warn "缺 $KD/build_key.txt ⇒ 跳过联网校验（C9 也会跳过）"
elif [ -z "$PAYLOAD_SHA" ]; then
  warn "没有载荷 sha ⇒ 跳过联网校验"
elif ! command -v curl >/dev/null 2>&1; then
  warn "没有 curl ⇒ 跳过联网校验"
else
  KEY="$(tr -d '\r\n' < "$KD/build_key.txt")"
  URL="$API_BASE/v1/build/payload-status?game_id=$GAME&sha=$PAYLOAD_SHA"
  CERT="$ROOT/mod菜单/starry-mod-menu/app/src/main/assets/certs/starlight.crt"
  if [ -f "$CERT" ]; then
    BODY="$(curl -sS -m 12 --cacert "$CERT" -H "X-Build-Key: $KEY" "$URL" 2>&1)"
  else
    warn "仓内没有 starlight.crt ⇒ 本次跳过证书校验（-k）"
    BODY="$(curl -sS -k -m 12 -H "X-Build-Key: $KEY" "$URL" 2>&1)"
  fi
  STATUS_OUT="$(printf '%s' "$BODY" | python3 -c '
import json, sys
try:
    d = json.load(sys.stdin)
except Exception:
    sys.exit("bad-json")
if not isinstance(d, dict):
    sys.exit("bad-json")
print("match=%s" % d.get("match"))
print("k_build_fp=%s" % d.get("k_build_fp", ""))
print("v3_ready=%s" % d.get("v3_ready"))
print("registered_sha=%s" % d.get("registered_sha", ""))
' 2>&1)"
  printf '%s\n' "$STATUS_OUT" | sed 's/^/       /'
  case "$STATUS_OUT" in
    *bad-json*)
      fail "服务端返回非 JSON：$BODY"
      info "403=build_key 不对；404=接口未启用/游戏不存在；compose 里 BUILD_API_KEY 是否为空"
      ;;
    *"match=True"*)
      SRV_KFP="$(printf '%s\n' "$STATUS_OUT" | sed -n 's/^k_build_fp=//p' | head -1)"
      if [ -n "$KFP" ] && [ "$SRV_KFP" = "$KFP" ]; then
        pass "服务端 match=true，且 k_build_fp 与本机自算一致（${KFP}）"
      else
        fail "服务端 match=true 但 k_build_fp=$SRV_KFP ≠ 本机 $KFP ⇒ 两把不同的钥"
      fi
      case "$STATUS_OUT" in
        *v3_ready=True*) pass "v3_ready=True（可签 v3 票）" ;;
        *) warn "v3_ready=False ⇒ 服务端 payload_master 未配或该游戏未登记" ;;
      esac
      ;;
    *)
      fail "服务端 match 不是 true ⇒ 载荷 sha 未登记/不一致（去后台登记 ${PAYLOAD_SHA}）"
      info "登记入口：后台 → 游戏列表 → 该游戏 → 载荷 sha；改 sha 会让旧包全失效"
      ;;
  esac
  unset KEY BODY
fi

# ---------------------------------------------------------------------------
# 汇总
# ---------------------------------------------------------------------------
sect "汇总"
printf 'PASS=%d  WARN=%d  FAIL=%d\n' "$PASS_N" "$WARN_N" "$FAIL_N"
printf '密钥目录权限检查：\n'
ls -ld "$KD" 2>/dev/null | sed 's/^/       /'
for f in rsa2048.txt payload_master.txt build_key.txt api_base.txt; do
  if [ -f "$KD/$f" ]; then
    info "$f  $(ls -l "$KD/$f" | cut -c1-10)  $(wc -c < "$KD/$f" | tr -d ' ') B"
  else
    info "$f  缺失"
  fi
done

if [ "$FAIL_N" -gt 0 ]; then
  printf '\n结论：有 FAIL，先别出包。\n'
  exit 1
fi
if [ "$WARN_N" -gt 0 ]; then
  printf '\n结论：无硬错但有 WARN，出包前自行确认上面几条。\n'
  exit 2
fi
printf '\n结论：三项自证全 PASS，本机可出包（K_build / 公钥与服务器同源）。\n'
exit 0
