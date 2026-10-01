#!/usr/bin/env bash
# ============================================================================
# twin_status.sh - 双机协作一键体检 (mac / win 通用, bash 3.2 兼容)
#
# 用法:  bash 04_scripts/_common/twin_status.sh
# 退出码: 0 = 全绿 / 1 = 有 FAIL / 2 = 仅 WARN
#
# 覆盖: 仓库定位与兼容路径 / git 状态与远端 / 换行符白名单 / mac 命令垫片
#       env.sh 六项 / python / adb 设备 / SMB 共享 / Syncthing 进程与 GUI
#
# 说明: 本脚本是唯一被允许做"整仓 git status"的地方(体检用途, 只读)。
#       手工排查时仍按 AGENTS.md 纪律带 pathspec。
# ============================================================================
set -u

PASS_N=0; WARN_N=0; FAIL_N=0
pass() { PASS_N=$((PASS_N + 1)); printf '[PASS] %s\n' "$*"; }
warn() { WARN_N=$((WARN_N + 1)); printf '[WARN] %s\n' "$*"; }
fail() { FAIL_N=$((FAIL_N + 1)); printf '[FAIL] %s\n' "$*"; }
info() { printf '       %s\n' "$*"; }
sect() { printf '\n== %s ==\n' "$*"; }

if [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
  sed -n '2,12p' "$0"
  exit 0
fi

MAC_ROOT=/Users/aliang/Documents/nixiang
SHA_X=2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881
MD5_X=9dd4e461268c8034f5c8564e155c67a6
TMPD="${TMPDIR:-/tmp}"
TMPD="${TMPD%/}"

case "$(uname -s)" in
  Darwin) HOST=mac ;;
  MINGW*|MSYS*|CYGWIN*) HOST=win ;;
  *) HOST=unknown ;;
esac

ROOT="${NIXIANG_ROOT:-}"
if [ -z "$ROOT" ]; then
  ROOT="$(cd "$(dirname "$0")/../.." 2>/dev/null && pwd -P)"
fi

printf 'nixiang 双机体检  host=%s  root=%s\n' "$HOST" "$ROOT"

# --------------------------------------------------------------------------
sect "1. 仓库定位"
if [ -n "$ROOT" ] && [ -d "$ROOT/.git" ]; then
  pass "repo root: $ROOT"
else
  fail "不是 git 仓库: ${ROOT:-<empty>}"
fi
if [ "$HOST" = win ]; then
  if (cd "$MAC_ROOT" 2>/dev/null); then
    pass "兼容路径可进入: $MAC_ROOT"
  else
    fail "兼容路径失效: $MAC_ROOT (junction 丢了? 重跑 win_bootstrap.ps1 步骤 6)"
  fi
fi

# --------------------------------------------------------------------------
sect "2. git 状态与远端"
if [ -d "$ROOT/.git" ]; then
  cd "$ROOT" || fail "cd $ROOT 失败"
  head_short="$(git rev-parse --short HEAD 2>/dev/null)"
  head_full="$(git rev-parse HEAD 2>/dev/null)"
  branch_line="$(git status -sb 2>/dev/null | head -1)"
  info "HEAD    : $head_short"
  info "branch  : $branch_line"
  dirty="$(git status --porcelain --untracked-files=no 2>/dev/null | wc -l | tr -d ' ')"
  if [ "$dirty" = "0" ]; then
    pass "已跟踪文件无改动"
  else
    warn "已跟踪文件有 $dirty 处未提交改动 (收尾前必须处理)"
  fi
  if git rev-parse --verify -q origin/main >/dev/null 2>&1; then
    behind="$(git rev-list --left-right --count origin/main...HEAD 2>/dev/null | awk '{print $1}')"
    ahead="$(git rev-list --left-right --count origin/main...HEAD 2>/dev/null | awk '{print $2}')"
    info "origin/main: $(git rev-parse --short origin/main 2>/dev/null)  behind=$behind ahead=$ahead"
    if [ "${behind:-0}" != "0" ]; then
      warn "落后 origin/main $behind 个提交 - 开工前先 git pull --rebase --autostash"
    else
      pass "不落后远端"
    fi
    if [ "${ahead:-0}" != "0" ]; then
      warn "本地领先 $ahead 个提交未 push"
    fi
  else
    warn "本地没有 origin/main 引用 (先 git fetch)"
  fi
else
  fail "跳过 git 检查"
fi

# --------------------------------------------------------------------------
sect "3. 换行符白名单"
if [ -d "$ROOT/.git" ]; then
  crlf_all="$(git ls-files --eol 2>/dev/null | grep 'i/crlf' || true)"
  n_crlf="$(printf '%s\n' "$crlf_all" | grep -c . || true)"
  bad="$(printf '%s\n' "$crlf_all" | grep -v -E '06_dumped/.*/apk_assets/|06_dumped/.*/il2cpp_dump_|10_docs/掌门下山/lua_dump/' | grep . || true)"
  n_bad="$(printf '%s\n' "$bad" | grep -c . || true)"
  info "index 里 i/crlf 共 ${n_crlf} 行 (白名单上限 15)"
  if [ "${n_bad:-0}" = "0" ] && [ "${n_crlf:-0}" -le 15 ]; then
    pass "白名单外零 CRLF"
  else
    fail "白名单外出现 CRLF:"
    printf '%s\n' "$bad" | head -20 | while read -r l; do info "$l"; done
  fi
fi

# --------------------------------------------------------------------------
sect "4. mac 命令垫片 (md5 / stat -f / shasum)"
probe="$TMPD/.nx_twin_probe.$$"
rm -f "$probe"
if printf x > "$probe" 2>/dev/null; then
  m="$(md5 -q "$probe" 2>/dev/null || true)"
  sz="$(stat -f %z "$probe" 2>/dev/null || true)"
  sh="$(shasum -a 256 "$probe" 2>/dev/null | awk '{print $1}')"
  cm="$(bash -c "md5 -q $probe" 2>/dev/null || true)"
  info "md5 -q = ${m:-<空>}"; info "stat -f %z = ${sz:-<空>}"; info "shasum -a 256 = ${sh:-<空>}"
  if [ "$m" = "$MD5_X" ]; then pass "md5 垫片正确"; else fail "md5 -q 不符合预期 (期望 $MD5_X)"; fi
  if [ "$sz" = "1" ]; then pass "stat -f %z 垫片正确"; else fail "stat -f %z 不是 1 (GNU stat 没被垫住?)"; fi
  if [ "$sh" = "$SHA_X" ]; then pass "shasum -a 256 正确"; else fail "shasum -a 256 不符合预期"; fi
  if [ -n "$cm" ] && [ "$cm" = "$m" ]; then pass "垫片可被非交互子 bash 继承 (bash -c)"; else fail "bash -c 里垫片没继承 (检查 export -f / BASH_ENV)"; fi
  if [ "$HOST" = win ]; then info "BASH_ENV=${BASH_ENV:-<未设置>}"; fi
  rm -f "$probe"
else
  fail "无法在 $TMPD 写探针文件"
fi

# --------------------------------------------------------------------------
sect "5. env.sh 六项"
tmp_env="$TMPD/.nx_twin_env.$$"
cat > "$tmp_env" <<'NXEOF'
cd "$1" || exit 9
. 04_scripts/_common/env.sh
printf '%s|%s|%s|%s|%s|%s\n' \
  "${NIXIANG_HOST:-}" "${NIXIANG_PYTHON:-}" "${NIXIANG_BUILD_TOOLS:-}" \
  "${NIXIANG_ZIPALIGN:-}" "${NIXIANG_APKSIGNER:-}" "${NIXIANG_JAVA17:-}"
NXEOF
six="$(bash "$tmp_env" "$ROOT" 2>/dev/null || true)"
rm -f "$tmp_env"
IFS='|' read -r f_host f_py f_bt f_za f_ap f_j17 <<NXEOF
$six
NXEOF
info "HOST=$f_host"
info "PYTHON=$f_py"
info "BUILD_TOOLS=$f_bt"
info "ZIPALIGN=$f_za"
info "APKSIGNER=$f_ap"
info "JAVA17=$f_j17"
missing=""
[ -n "${f_host:-}" ] || missing="$missing HOST"
[ -n "${f_py:-}" ] || missing="$missing PYTHON"
[ -n "${f_bt:-}" ] || missing="$missing BUILD_TOOLS"
[ -n "${f_za:-}" ] || missing="$missing ZIPALIGN"
[ -n "${f_ap:-}" ] || missing="$missing APKSIGNER"
[ -n "${f_j17:-}" ] || missing="$missing JAVA17"
if [ -z "$missing" ]; then pass "六项全部非空"; else fail "env.sh 解析为空:$missing"; fi
if [ "${f_host:-}" = "$HOST" ]; then pass "NIXIANG_HOST 与本机一致 ($HOST)"; else warn "NIXIANG_HOST=$f_host 与本机 $HOST 不一致"; fi
if [ "${f_py:-}" != "" ] && [ -x "$f_py" ]; then pass "python 可执行: $f_py"; else warn "python 不可执行: ${f_py:-<空>}"; fi
if [ "${f_j17:-}" != "" ] && [ -x "$f_j17/bin/javac" -o -x "$f_j17/bin/javac.exe" ]; then
  pass "javac 存在: $f_j17/bin/javac"
else
  warn "javac 不存在于 ${f_j17:-<空>}/bin"
fi

# --------------------------------------------------------------------------
sect "6. 设备 (adb)"
if command -v adb >/dev/null 2>&1; then
  info "ANDROID_SERIAL=${ANDROID_SERIAL:-<未设置>}"
  devs="$(adb devices 2>/dev/null | awk 'NR>1 && NF {printf "%s(%s) ", $1, $2}')"
  if [ -n "$devs" ]; then
    pass "adb 设备: $devs"
    if [ -n "${ANDROID_SERIAL:-}" ]; then
      if (adb -s "$ANDROID_SERIAL" get-state >/dev/null 2>&1); then
        pass "ANDROID_SERIAL=$ANDROID_SERIAL 可达"
      else
        fail "ANDROID_SERIAL=$ANDROID_SERIAL 不可达 (模拟器/adb server 重启后要先 adb connect)"
      fi
    fi
  else
    warn "adb 没有列出设备 (模拟器没开 / 手机没插 / 要先 adb connect 127.0.0.1:16384)"
  fi
else
  warn "adb 不在 PATH"
fi

# --------------------------------------------------------------------------
sect "7. SMB 互传 (E:\\nixiang 共享)"
if (exec 3<>/dev/tcp/127.0.0.1/445) 2>/dev/null; then
  exec 3>&- 2>/dev/null || true
  pass "SMB 服务在监听 (445)"
else
  warn "本机 445 未监听 (LanmanServer 没起?)"
fi
if [ "$HOST" = win ]; then
  if net share nixiang >/dev/null 2>&1; then pass "共享 nixiang 存在"; else warn "共享 nixiang 不存在 (New-SmbShare -Name nixiang -Path E:\\nixiang)"; fi
else
  if smbutil statshares -a 2>/dev/null | grep -q nixiang; then pass "已挂载 nixiang"; else warn "未挂载 nixiang (访达 cmd+K -> smb://192.168.10.9/nixiang)"; fi
fi

# --------------------------------------------------------------------------
sect "8. Syncthing 大文件同步"
if [ "$HOST" = win ]; then
  proc="$(ps -W 2>/dev/null | grep -c '[s]yncthing.exe' || true)"
else
  proc="$(pgrep -f '[s]yncthing' 2>/dev/null | wc -l | tr -d ' ')"
fi
if [ "${proc:-0}" != "0" ]; then pass "syncthing 进程数 $proc"; else fail "syncthing 没在跑 (带出门回来/重启后最容易静默挂掉的就是它)"; fi
if command -v curl >/dev/null 2>&1; then
  code="$(curl -s -m 4 -o /dev/null -w '%{http_code}' http://127.0.0.1:8384/rest/noauth/health 2>/dev/null || true)"
  if [ "$code" = "200" ]; then pass "GUI 健康检查 200 (http://127.0.0.1:8384)"; else warn "GUI 健康检查返回 ${code:-<空>}"; fi
else
  warn "没有 curl, 跳过 GUI 检查"
fi
if [ "$HOST" = win ]; then
  # 注意: MSYS 会把 /query 当成路径改写掉, 必须写 //query
  if schtasks //query //tn nixiang-syncthing >/dev/null 2>&1; then pass "自启任务 nixiang-syncthing 已注册"; else warn "自启任务 nixiang-syncthing 不存在"; fi
fi

# --------------------------------------------------------------------------
printf '\n== 汇总 ==\n'
printf 'PASS=%s  WARN=%s  FAIL=%s\n' "$PASS_N" "$WARN_N" "$FAIL_N"
if [ "$FAIL_N" -gt 0 ]; then
  printf '结论: 有 FAIL, 先修它们\n'
  exit 1
fi
if [ "$WARN_N" -gt 0 ]; then
  printf '结论: 无 FAIL, 有 WARN\n'
  exit 2
fi
printf '结论: 全绿\n'
exit 0
