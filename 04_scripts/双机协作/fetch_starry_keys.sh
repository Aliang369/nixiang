#!/usr/bin/env bash
# ============================================================================
# fetch_starry_keys.sh — 新机器（mac）落地构建机密钥 + 自证（bash 3.2 兼容）
#
# 背景：出包机持有 ~/.starry-keys/{rsa2048.txt,payload_master.txt,build_key.txt}；
#       mac 端没有这三份 ⇒ gen_pubkey_header.py / payload 派生脚本在本机不可用。
#       2026-11-22：v3 票退役，服务端 payload_sha / payload-status 已删除，
#       payload_master.txt 仅历史遗留（老包复刻用），不再有服务端校验环节。
#
# 通道：**经 SSH 从授权服务器取，绝不走 git**。私钥入库后即使后续提交删除，
#       blob 也永久留在历史里（两台机器 .git 各一份），清理要 filter-repo+强推，
#       属 AGENTS.md §5 事故级禁区。
#
# 用法：
#   bash 04_scripts/双机协作/fetch_starry_keys.sh              # 取 + 自证
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --verify     # 只自证（已取过）
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --dry-run    # 只看要取什么
#   bash 04_scripts/双机协作/fetch_starry_keys.sh --force      # 覆盖已存在的密钥
#
# 环境变量（一般不用改）：
#   SG_SSH_HOST    授权服务器 SSH 目标     默认 root@154.94.232.231
#   SG_SERVER_DIR  服务端 auth-server 目录  默认 /www/wwwroot/starlight-mod-auth
#   SG_GAME        用于自证的 game_id       默认 starry_self（星光自测）
#   SG_EXPECT_FP   期望公钥指纹             默认 25ba10e0faca3500
#
# 退出码：0 = 自证全 PASS / 1 = 有 FAIL / 2 = 只 WARN（未取齐但没硬错）
#
# 安全：落地目录 700、文件 600；脚本不打印任何密钥内容，只打印指纹。
# ============================================================================
set -uo pipefail

SSH_HOST="${SG_SSH_HOST:-root@154.94.232.231}"
SERVER_DIR="${SG_SERVER_DIR:-/www/wwwroot/starlight-mod-auth}"
GAME="${SG_GAME:-starry_self}"
API_BASE="${SG_API_BASE:-https://auth.xgmfyx.cn}"
EXPECT_FP="${SG_EXPECT_FP:-25ba10e0faca3500}"

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
sect "1/3 取密钥（SSH 直连，走 git 一律禁止）"

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
sect "2/3 自证 A — 公钥指纹 == 仓内 kSgPubFp"

if [ ! -f "$KD/rsa2048.txt" ]; then
  fail "缺 $KD/rsa2048.txt"
else
  # 显式传私钥路径：gen_pubkey_header.py 的默认值走 os.path.expanduser("~")，在 Windows 的
  # 某些 shell 环境下 "~" 不展开（实测报 FileNotFoundError: '~\.starry-keys\rsa2048.txt'），
  # 直接传已解析的绝对路径，两平台一致。
  _KEYF="$KD/rsa2048.txt"
  command -v cygpath >/dev/null 2>&1 && _KEYF="$(cygpath -m "$_KEYF")"
  PUB_OUT="$(python3 "$ROOT/04_scripts/_common/sg_pki/gen_pubkey_header.py" --key "$_KEYF" --check 2>&1)"
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
# 3) 载荷体系：已删除（2026-11-27）
# ---------------------------------------------------------------------------
sect "3/3 载荷加密/密文校验（已删除）"
info "2026-11-22：v3 票退役，后台 payload_sha 与 /v1/build/payload-status 删除"
info "2026-11-27：载荷加密体系整体移除（gen_payload_enc.py / harden_apk.py 已删，sha 载荷已清理）"
info "⇒ 本脚本不再自算 K_build、不再校验密文头；只剩「取密钥 + 公钥指纹自证」两件事"

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
printf '\n结论：两项自证全 PASS，本机可出包（公钥与服务端同源）。\n'
exit 0
