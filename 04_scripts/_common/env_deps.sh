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
#    bash 04_scripts/_common/env_deps.sh --device   # 额外比对真机 frida-server 版本（默认不碰设备）
#    bash 04_scripts/_common/env_deps.sh --quick    # 只跑分析工具链（第 6 节），供每轮开工 / preflight 自动调用
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
DEVICE=0
QUICK=0
[ "${1:-}" = "-v" ] && VERBOSE=1

for _arg in "$@"; do
  case "$@" in
    -v) VERBOSE=1 ;;
    --device) DEVICE=1 ;;
    --quick) QUICK=1 ;;
  esac
done

_PASS=0; _FAIL=0; _WARN=0; _SKIP=0

ok()   { _PASS=$((_PASS + 1)); printf '  ✓ %-26s %s\n' "$1" "${2:-}"; }
bad()  { _FAIL=$((_FAIL + 1)); printf '  ✗ %-26s %s\n' "$1" "${2:-}"; }
warn() { _WARN=$((_WARN + 1)); printf '  ⚠ %-26s %s\n' "$1" "${2:-}"; }
skip() { _SKIP=$((_SKIP + 1)); printf '  – %-26s %s\n' "$1" "${2:-}"; }

has() { command -v "$1" >/dev/null 2>&1; }

echo "════════════════════════════════════════════════════════════"
echo " 环境依赖体检 — $(date '+%Y-%m-%d %H:%M:%S')"
echo "════════════════════════════════════════════════════════════"

# --quick：跳过 1-5 节，只查分析工具链（AI 每轮开工跑的那条快路径）
if [ "$QUICK" = "0" ]; then

# ---------------------------------------------------------------- 1. 主机/壳
echo
echo "[1/7] 主机与 shell"
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
echo "[2/7] Python（RE 脚本主力）"
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
echo "[3/7] Java 与 Android SDK 工具"
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
echo "[4/7] 跨平台命令（macOS 优先序不得写反）"
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
echo "[5/7] 本仓公共件"
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

fi  # --quick 边界

# ---------------------------------------------------------------- 6. 分析工具链
# 2026-11-04 新增：env_deps 原先只管收官链（python/java/apksigner/跨平台命令），
# 中间这层「逆向分析工具」一直没人管 —— 01_工具清单与安装状态.md 自己写明是历史观测。
# 这里补上：apktool/jadx 属拆包主力（缺=FAIL），IDA/IL2CPP/ilspycmd/ndk 按需（缺=WARN）。
# 只报「在不在、版本多少」，不判断「该用哪个工具」。
#
# ⚠ 双机现状（2026-11-04）：**本节只在本机（mac）验证过。win 的探测路径一条都没写。**
# 在 mac 上猜 Windows 的安装位置就是编，交给 Win 端接入时自己补：
#   Win 端第一次跑到这里，按本机实际补 case win) 分支，并把实测路径回填到
#   10_docs/tools/01_工具清单与安装状态.md「双机差异」表。
#   未补之前，win 上这些工具只能靠 PATH 命中，或在本机 04_scripts/_common/.env.local 里
#   显式写 NIXIANG_*_BIN（不进 git、各机各写；模板见同目录 env.local.example）。
# 跨平台通用、不依赖平台假设的部分：command -v 语义、$NIXIANG_ROOT/01_tools 下的仓内路径、
#   环境变量 ANDROID_NDK_HOME / ANDROID_HOME / ANDROID_SDK_ROOT。
# 默认不碰真机：frida 客户端↔服务端版本对齐必须显式加 --device 才查（AGENTS.md §4）。
echo
echo "[6/7] 分析工具链（拆包 / 反编译 / Hook / IL2CPP）  host=${NIXIANG_HOST}"

_ENV_LOCAL="${NIXIANG_ROOT}/04_scripts/_common/.env.local"   # 不进 git（.gitignore 已有 .env.*），各机各写
if [ -f "$_ENV_LOCAL" ]; then
  if . "$_ENV_LOCAL" 2>/dev/null; then
    printf '  %-28s %s\n' "本机覆盖 .env.local" "已加载"
  else
    warn ".env.local" "加载失败：${_ENV_LOCAL}（本机覆盖未生效）"
  fi
fi

_tool_ver() { "$@" 2>/dev/null | tr -d '\r' | grep -oE '[0-9]+(\.[0-9]+)+' | head -1; }

# 逐个工具：本机覆盖变量 > PATH > 仓内 01_tools 常见落点
_first_exist() {
  for _c in "$@"; do
    if [ -n "$_c" ] && [ -e "$_c" ]; then printf '%s' "$_c"; return 0; fi
  done
  return 1
}

# ---- apktool（拆/打 APK 主力：缺=FAIL）
_apktool="${NIXIANG_APKTOOL_BIN:-}"
if [ -z "$_apktool" ] || [ ! -e "$_apktool" ]; then
  if has apktool; then _apktool="$(command -v apktool)"
  else _apktool="$(_first_exist "$NIXIANG_ROOT/01_tools/apktool/apktool" "$NIXIANG_ROOT/01_tools/apktool.jar" || true)"; fi
fi
if [ -n "$_apktool" ]; then
  case "$_apktool" in
    *.jar) ok "apktool" "${_apktool}（jar，需 java -jar 或 .bat 包装）" ;;
    *)     _v="$(_tool_ver "$_apktool" --version)"; ok "apktool" "$_apktool $_v" ;;
  esac
else
  case "${NIXIANG_HOST}" in
    mac) bad "apktool" "不在 PATH 上（brew install apktool），也没 .env.local 覆盖" ;;
    win) bad "apktool" "不在 PATH 上；本机实际路径写 .env.local 的 NIXIANG_APKTOOL_BIN，并回来补 case win) 分支" ;;
    *)   bad "apktool" "不在 PATH 上" ;;
  esac
fi

# ---- jadx（Java 层反编译主力：缺=FAIL）
_jadx="${NIXIANG_JADX_BIN:-}"
if [ -z "$_jadx" ] || [ ! -e "$_jadx" ]; then
  if has jadx; then _jadx="$(command -v jadx)"
  else _jadx="$(_first_exist "$NIXIANG_ROOT/01_tools/jadx/bin/jadx" || true)"; fi
fi
if [ -n "$_jadx" ]; then
  _v="$(_tool_ver "$_jadx" --version)"; ok "jadx" "$_jadx $_v"
else
  case "${NIXIANG_HOST}" in
    mac) bad "jadx" "不在 PATH 上（brew install jadx），也没 .env.local 覆盖" ;;
    win) bad "jadx" "不在 PATH 上；本机实际路径写 .env.local 的 NIXIANG_JADX_BIN，并回来补 case win) 分支" ;;
    *)   bad "jadx" "不在 PATH 上" ;;
  esac
fi

# ---- smali/baksmali（仓内通用，双机同一套）
if [ -d "$NIXIANG_ROOT/01_tools/smali" ]; then
  ok "smali/baksmali" "01_tools/smali"
elif has baksmali; then
  ok "smali/baksmali" "$(command -v baksmali)（PATH）"
else
  warn "smali/baksmali" "未找到（01_tools/smali 或 PATH）；只走 apktool 也能干"
fi

# ---- IL2CPP 三件套（按需：改 il2cpp 游戏才用）
_have_il2cpp=0
for _d in "$NIXIANG_ROOT/01_tools/Il2CppDumper" "$NIXIANG_ROOT/11_proj_tools/Il2CppDumper_v39" "$NIXIANG_ROOT/11_proj_tools/Il2CppDumper_v39_vmpp" "${NIXIANG_IL2CPP_DIR:-}"; do
  if [ -n "$_d" ] && [ -d "$_d" ]; then _have_il2cpp=$((_have_il2cpp + 1)); fi
done
if [ "$_have_il2cpp" -gt 0 ]; then ok "Il2CppDumper" "$_have_il2cpp 套（01_tools / 11_proj_tools / env.local）"
else                            warn "Il2CppDumper" "未找到（改 il2cpp 游戏才需要）"; fi

if [ -d "$NIXIANG_ROOT/01_tools/Cpp2IL_bin" ] || [ -d "$NIXIANG_ROOT/01_tools/Cpp2IL" ]; then
  ok "Cpp2IL" "01_tools/Cpp2IL*"
else
  warn "Cpp2IL" "未找到（il2cpp 备选路线，按需）"
fi

if has ilspycmd; then ok "ilspycmd" "$(command -v ilspycmd)"
else                  warn "ilspycmd" "未找到（DummyDll → C# 需要；dotnet tool install -g ilspycmd）"; fi
if has dotnet;  then ok "dotnet" "$(dotnet --version 2>/dev/null || echo '?')"
else                  warn "dotnet" "未找到（ilspycmd / Il2CppDumper 的运行时依赖）"; fi

# ---- IDA Pro（按需：只看 smali/JS 层可不装）
_ida="${NIXIANG_IDA_HOME:-}"
if [ -z "$_ida" ] || [ ! -d "$_ida" ]; then
  _ida=""
  case "${NIXIANG_HOST}" in
    mac)
      for _p in /Applications/IDA*.app; do
        if [ -d "$_p" ]; then _ida="$_p"; break; fi
      done
      ;;
    win)
      # TODO(Win 端)：本机 IDA 装在哪由你实测后补这里（Program Files / 便携版 / 别的盘）。
      #   补之前只用 .env.local 的 NIXIANG_IDA_HOME，否则这里报 WARN。
      ;;
    *)
      for _p in "$NIXIANG_ROOT/01_tools/IDA"*; do
        if [ -d "$_p" ]; then _ida="$_p"; break; fi
      done
      ;;
  esac
fi
if [ -n "$_ida" ]; then ok "IDA Pro" "$_ida"
else                     warn "IDA Pro" "未探测到（按需；便携版请在 env.local.sh 写 NIXIANG_IDA_HOME）"; fi

# ---- frida 客户端（Hook / 动态取证）
_frida="${NIXIANG_FRIDA_BIN:-}"
if [ -z "$_frida" ] || [ ! -e "$_frida" ]; then
  if has frida; then _frida="$(command -v frida)"; else _frida=""; fi
fi
if [ -n "$_frida" ]; then
  _fv="$(_tool_ver "$_frida" --version)"
  ok "frida 客户端" "$_frida $_fv"
else
  warn "frida 客户端" "不在 PATH 上（Hook / 动态取证全靠它；win 上 pip install frida-tools）"
  _fv=""
fi

# ---- NDK（只有改 .so 才需要）
_ndk="${NIXIANG_NDK_HOME:-}"
if [ -n "$_ndk" ] && [ -d "$_ndk" ]; then
  ok "ndk-build" "${_ndk}（来自 env.local.sh）"
else
  _ndk=""
  if has ndk-build; then
    _ndk="$(command -v ndk-build)"
  else
    for _sdk in "${ANDROID_NDK_HOME:-}" "${ANDROID_HOME:-}" "${ANDROID_SDK_ROOT:-}" "$HOME/Library/Android/sdk"; do
      [ -n "$_sdk" ] || continue
      [ -d "$_sdk" ] || continue
      for _d in "$_sdk"/ndk/*; do
        if [ -d "$_d" ]; then _ndk="${_d}（未设 NIXIANG_NDK_HOME，用前 export）"; fi
      done
    done
  fi
  if [ -n "$_ndk" ]; then ok "ndk-build" "$_ndk"
  else                    warn "ndk-build" "未找到（改 .so 才需要）；装 NDK，或 env.local.sh 写 NIXIANG_NDK_HOME"; fi
fi

# ---- 真机 frida-server（只在显式 --device 时碰设备）
if [ "$DEVICE" = "1" ]; then
  if has adb; then
    _serial="$(adb get-serialno 2>/dev/null | tr -d '\r' | head -1)"
    case "$_serial" in
      ''|*error*|*unknown*|*offline*)
        warn "真机 frida-server" "adb 无可用设备（先装机 / 授权 USB 调试）" ;;
      *)
        _srv="$(adb -s "$_serial" shell "frida-server --version 2>/dev/null || /data/local/tmp/frida-server --version 2>/dev/null" 2>/dev/null | tr -d '\r' | grep -oE '[0-9]+(\.[0-9]+)+' | head -1)"
        if [ -z "$_srv" ]; then
          warn "真机 frida-server" "设备 $_serial 取不到版本（未推 frida-server 或不可执行）"
        elif [ -z "$_fv" ]; then
          warn "真机 frida-server" "设备 ${_srv}（本机客户端未知，无法比对）"
        elif [ "$_srv" = "$_fv" ]; then
          ok "frida 版本对齐" "本机 = 设备 = $_srv"
        else
          bad "frida 版本对齐" "本机 $_fv ≠ 设备 ${_srv}（不同版本 hook 必失败）"
        fi
        ;;
    esac
  else
    skip "真机 frida-server" "adb 不在 PATH"
  fi
else
  skip "真机 frida-server" "未加 --device（默认不碰真机，遵守 AGENTS.md §4）"
fi

# ---------------------------------------------------------------- 7. 摘要
echo
echo "[7/7] 摘要"
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
