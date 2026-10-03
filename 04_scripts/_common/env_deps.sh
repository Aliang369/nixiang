#!/usr/bin/env bash
# ==============================================================================
#  env_deps.sh — 环境依赖体检（双机：mac / win Git Bash）
# ==============================================================================
#  为什么存在：收官链（finalize_game_delivery / trash_paths / prune_output_final /
#  sign / install）在 Win 上曾被三个环境问题分三次打断 ——
#    ① apksigner 用 [ -x ] 判定，Win 上 .bat 没有 x 位 ⇒ 误报"未找到 apksigner"
#    ② 脚本硬编码 python3.11，Win 上无此命令名 ⇒ command not found
#    ③ stat -f / md5 -q 是 BSD 专有写法，GNU 上要回退 stat -c / md5sum
#  这里把所有这些探测一次跑完，**在动真格之前**就把环境缺口列清楚。
#
#  用法：
#    bash 04_scripts/_common/env_deps.sh          # 体检当前机器
#    bash 04_scripts/_common/env_deps.sh -v        # 额外打印会话摘要
#
#  退出码：0 = 无 FAIL（WARN 不算失败）；1 = 有 FAIL。
#
#  遵守本仓双机约定：
#    - bash 3.2 兼容（mac 侧 3.2.57）：不用 mapfile/readarray/declare -A/local -n
#      /${v^^}/;;&/&>>；空数组不裸展开
#    - macOS 优先序不得写反：sips → Pillow；md5 -q → md5sum；shasum -a 256 →
#      sha256sum；stat -f → stat -c
#    - 任何 cygpath 调用都必须 command -v 守护
# ==============================================================================
set -uo pipefail

_HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
. "$_HERE/env.sh"

VERBOSE=0
[ "${1:-}" = "-v" ] && VERBOSE=1

_PASS=0; _FAIL=0; _WARN=0; _SKIP=0

ok()   { _PASS=$((_PASS + 1)); printf '  ✓ %-26s %s\n' "$1" "${2:-}"; }
bad()  { _FAIL=$((_FAIL + 1)); printf '  ✗ %-26s %s\n' "$1" "${2:-}"; }
warn() { _WARN=$((_WARN + 1)); printf '  ⚠ %-26s %s\n' "$1" "${2:-}"; }
skip() { _SKIP=$((_SKIP + 1)); printf '  – %-26s %s\n' "$1" "${2:-}"; }

has() { command -v "$1" >/dev/null 2>&1; }

echo "════════════════════════════════════════════════════════════"
echo " 环境依赖体检 — $(date '+%Y-%m-%d %H:%M:%S')"
echo "════════════════════════════════════════════════════════════"

# ---------------------------------------------------------------- 1. 主机/壳
echo
echo "[1/6] 主机与 shell"
printf '  host = %s   bash = %s\n' "$NIXIANG_HOST" "$BASH_VERSION"
printf '  root = %s\n' "$NIXIANG_ROOT"

case "$NIXIANG_HOST" in
  mac|win|linux) ok "主机判定" "$NIXIANG_HOST" ;;
  *)             bad "主机判定" "uname -s 未被识别，NIXIANG_HOST=$NIXIANG_HOST" ;;
esac

if [ -d "$NIXIANG_ROOT" ]; then ok "NIXIANG_ROOT 存在" "$NIXIANG_ROOT"
else                          bad "NIXIANG_ROOT 存在" "$NIXIANG_ROOT 不存在"; fi

# mac 侧 bash 3.2.57：本仓脚本必须能在它上面跑
_BV="${BASH_VERSION%%.*}"
case "$_BV" in
  ''|*[!0-9]*) warn "bash 版本解析" "无法解析 BASH_VERSION=$BASH_VERSION" ;;
  *)
    if [ "$_BV" -ge 4 ]; then
      ok "bash 版本" "$BASH_VERSION"
    else
      ok "bash 版本" "${BASH_VERSION}（bash 3.2 兼容模式，脚本不得用 bash 4+ 语法）"
    fi
    ;;
esac

# ---------------------------------------------------------------- 2. Python
echo
echo "[2/6] Python（RE 脚本主力）"
if [ -n "${NIXIANG_PYTHON:-}" ] && "$NIXIANG_PYTHON" -c 'pass' >/dev/null 2>&1; then
  ok "NIXIANG_PYTHON 可执行" "$("$NIXIANG_PYTHON" -c 'import sys;print(sys.version.split()[0])' 2>/dev/null)"
else
  bad "NIXIANG_PYTHON 可执行" "NIXIANG_PYTHON=${NIXIANG_PYTHON:-<空>}"
fi

# 真导入探测：`import PIL` 会假阳性（装坏的 PIL 能 import 但 from PIL import Image 炸）
_pil_check() {
  "$1" -c 'from PIL import Image; print(Image.__version__ if hasattr(Image, "__version__") else "ok")' 2>/dev/null
}
if [ -n "${NIXIANG_PYTHON:-}" ]; then
  _pil="$(_pil_check "$NIXIANG_PYTHON" || true)"
  if [ -n "$_pil" ]; then ok "NIXIANG_PYTHON 带 Pillow" "$_pil"
  else                   warn "NIXIANG_PYTHON 带 Pillow" "不可用（finalize_game_delivery.sh 的 icon 生成会失败；win 上可换带 PIL 的解释器）"; fi
fi

# `python3.11`：mac 由 brew 提供；win 靠垫片
_p311="$(command -v python3.11 2>/dev/null || true)"
if [ -n "$_p311" ]; then
  ok "python3.11" "$_p311"
else
  case "$NIXIANG_HOST" in
    mac) bad  "python3.11" "mac 上应由 brew 提供（brew install python@3.11）" ;;
    win) warn "python3.11" "缺失！全仓 81 处调用会 command not found ⇒ bash 04_scripts/_common/win_shims/install_win_shims.sh" ;;
    *)   warn "python3.11" "缺失" ;;
  esac
fi

if [ "$NIXIANG_HOST" = "win" ] && [ -n "${NIXIANG_SHIM_DIR:-}" ]; then
  if [ -f "$NIXIANG_SHIM_DIR/python3.11" ]; then
    ok "垫片本体存在" "$NIXIANG_SHIM_DIR/python3.11"
    case ":$PATH:" in
      *":$NIXIANG_SHIM_DIR:"*) ok "垫片在 PATH 上" "（env.sh 已前置）" ;;
      *) warn "垫片在 PATH 上" "未前置，未 source env.sh 的脚本看不到" ;;
    esac
    # 垫片实际选中的解释器才是那 81 处调用会用的 python（NIXIANG_PYTHON 可能不同）
    _sp="$(NIXIANG_SHIM_INFO=1 bash "$NIXIANG_SHIM_DIR/python3.11" 2>/dev/null | head -1 || true)"
    if [ -n "$_sp" ]; then
      ok "垫片选中解释器" "$_sp"
      _spy="$(printf '%s' "$_sp" | sed -n 's/.*-> \(.*\) (libs score=.*/\1/p')"
      if [ -n "$_spy" ] && [ -f "$_spy" ]; then
        _sppil="$(_pil_check "$_spy" || true)"
        if [ -n "$_sppil" ]; then ok "垫片解释器带 Pillow" "$_sppil"
        else warn "垫片解释器带 Pillow" "不可用：$_spy"; fi
      fi
    else
      warn "垫片选中解释器" "垫片跑不通（试 NIXIANG_SHIM_INFO=1 \"$NIXIANG_SHIM_DIR/python3.11\"）"
    fi
  else
    warn "垫片本体存在" "缺失：$NIXIANG_SHIM_DIR/python3.11"
  fi
fi

# ---------------------------------------------------------------- 3. Java / SDK
echo
echo "[3/6] Java 与 Android SDK 工具"
if [ -n "${NIXIANG_JAVA17:-}" ] && [ -x "$NIXIANG_JAVA17/bin/java" ]; then
  ok "NIXIANG_JAVA17" "$NIXIANG_JAVA17"
elif [ -n "${NIXIANG_JAVA17:-}" ] && [ -f "$NIXIANG_JAVA17/bin/java.exe" ]; then
  ok "NIXIANG_JAVA17" "${NIXIANG_JAVA17}（java.exe）"
else
  case "$NIXIANG_HOST" in
    mac) warn "NIXIANG_JAVA17" "未解析到 JDK17（apksigner 需要）：${NIXIANG_JAVA17:-<空>}" ;;
    win) bad  "NIXIANG_JAVA17" "未解析到 JDK17：${NIXIANG_JAVA17:-<空>}" ;;
    *)   warn "NIXIANG_JAVA17" "未解析到：${NIXIANG_JAVA17:-<空>}" ;;
  esac
fi

# apksigner：Win 上是 apksigner.bat，没有 x 位 ⇒ 必须 [ -f ]，不能用 [ -x ]
if [ -n "${NIXIANG_APKSIGNER:-}" ] && [ -f "$NIXIANG_APKSIGNER" ]; then
  ok "apksigner" "$NIXIANG_APKSIGNER"
else
  bad "apksigner" "未解析到（NIXIANG_APKSIGNER=${NIXIANG_APKSIGNER:-<空>}）：检查 ANDROID_SDK_ROOT/build-tools/*/apksigner[.bat]"
fi

if [ -n "${NIXIANG_ZIPALIGN:-}" ] && [ -f "$NIXIANG_ZIPALIGN" ]; then
  ok "zipalign" "$NIXIANG_ZIPALIGN"
elif has zipalign; then
  ok "zipalign" "$(command -v zipalign)（PATH）"
else
  warn "zipalign" "未解析到（需要签名前对齐时才有用）"
fi

if [ -n "${NIXIANG_UBER_SIGNER:-}" ] && [ -f "$NIXIANG_UBER_SIGNER" ]; then
  ok "uber-apk-signer.jar" "$NIXIANG_UBER_SIGNER"
else
  warn "uber-apk-signer.jar" "缺失：${NIXIANG_UBER_SIGNER:-<未设置>}"
fi

if has adb; then ok "adb" "$(command -v adb)"
else             bad "adb" "不在 PATH 上（真机脚本全靠它）"; fi

# ---------------------------------------------------------------- 4. 跨平台工具
echo
echo "[4/6] 跨平台命令（macOS 优先序不得写反）"
if has cygpath; then
  ok "cygpath" "$(command -v cygpath)"
  printf '    nixiang_win_path /e/x/a.apk -> %s\n' "$(nixiang_win_path /e/x/a.apk)"
else
  case "$NIXIANG_HOST" in
    win) bad  "cygpath" "Win 上必须有（无它则传给 adb.exe/java.exe 的路径不可转换）" ;;
    mac) skip "cygpath" "mac 不需要（nixiang_win_path 原样返回）" ;;
    *)   skip "cygpath" "不存在" ;;
  esac
fi

if has sips; then ok "sips" "$(command -v sips)（mac icon 生成首选）"
elif [ "$NIXIANG_HOST" = "mac" ]; then warn "sips" "mac 上应有（/usr/bin/sips）"
else skip "sips" "非 mac，走 Pillow 回退（正常）"; fi

if has md5; then ok "md5" "（首选）"
elif has md5sum; then ok "md5" "无 md5，用 md5sum"
else bad "md5 / md5sum" "两个都没有，finalize_game_delivery.sh 算不出指纹"; fi

if has shasum; then ok "shasum" "（首选）"
elif has sha256sum; then ok "shasum" "无 shasum，用 sha256sum"
else warn "shasum / sha256sum" "两个都没有，sha256 校验会失败"; fi

# stat：BSD(-f) 优先，GNU(-c) 回退
_bsd="$(stat -f '%z' "$_HERE/env_deps.sh" 2>/dev/null || true)"
_gnu="$(stat -c '%s' "$_HERE/env_deps.sh" 2>/dev/null || true)"
if [ -n "$_bsd" ]; then
  ok "stat" "BSD 形式 stat -f '%z' 生效（mac 风格）"
elif [ -n "$_gnu" ]; then
  ok "stat" "GNU 形式 stat -c '%s' 回退生效（win 风格，脚本里的顺序是对的）"
else
  bad "stat" "两种形式都失败，_file_size 之类 helper 会挂"
fi

# ---------------------------------------------------------------- 5. 本仓公共件
echo
echo "[5/6] 本仓公共件"
for _f in env.sh preflight.sh bash32_check.sh; do
  if [ -f "$_HERE/$_f" ]; then ok "$_f" "$_HERE/$_f"
  else                         bad "$_f" "缺失"; fi
done
if [ -f "$_HERE/pipeline.lock" ]; then
  ok "pipeline.lock" "存在（改 pipeline.sh 必须同步刷 lock；未改则勿动）"
else
  skip "pipeline.lock" "不存在"
fi

for _f in finalize_game_delivery.sh trash_paths.sh prune_output_final.sh; do
  if [ -f "$_HERE/$_f" ]; then ok "$_f" ""
  else                         warn "$_f" "缺失（收官链会断）"; fi
done

# ---------------------------------------------------------------- 6. 摘要
echo
echo "[6/6] 摘要"
printf '  PASS=%d  FAIL=%d  WARN=%d  SKIP=%d\n' "$_PASS" "$_FAIL" "$_WARN" "$_SKIP"
if [ "$VERBOSE" = "1" ]; then
  echo "  host=$NIXIANG_HOST  py=$NIXIANG_PYTHON"
  echo "  apksigner=$NIXIANG_APKSIGNER"
  echo "  zipalign=$NIXIANG_ZIPALIGN"
fi

if [ "$_FAIL" -gt 0 ]; then
  echo
  echo "❌ 环境有 $_FAIL 项 FAIL —— 先修再动收官链。"
  exit 1
fi
echo
echo "✅ 环境体检通过（WARN 不阻塞，但请读完）。"
exit 0
