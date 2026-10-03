# nixiang shared env — source from other _common scripts
# shellcheck shell=bash
#
# 2026-10-01 双机协作：本文件同时服务 mac 与 win(Git Bash)。
#   改动必须保持 bash 3.2 兼容（mac 侧 bash 是 3.2.57）：
#   不要用 mapfile / readarray / declare -A / local -n / ${v^^} / ;;& / &>>。
#   改动后两台机器都要跑：bash 04_scripts/_common/preflight.sh 04_scripts/_common/env.sh

NIXIANG_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
export NIXIANG_ROOT

# --- 主机判定（双机协作 2026）---
case "$(uname -s 2>/dev/null || echo unknown)" in
  Darwin)               NIXIANG_HOST=mac ;;
  MINGW*|MSYS*|CYGWIN*) NIXIANG_HOST=win ;;
  Linux)                NIXIANG_HOST=linux ;;
  *)                    NIXIANG_HOST=unknown ;;
esac
export NIXIANG_HOST

# --- 路径转换 helper（双机 2026-10-04）---
# 把 POSIX 路径转成"交给原生 Windows 程序"用的形式。mac 上没有 cygpath，直接原样
# 返回；win 上 cygpath 也必须用 command -v 守护（不能无条件调）。
# 为什么需要它：脚本一旦 `export MSYS_NO_PATHCONV=1`（为了阻止 Git Bash 擅自改写
# 传给 adb.exe / java.exe 的参数），argv 里的 POSIX 路径就会被原生程序当成
# 当前盘根路径 —— 典型报错：java FileNotFoundException: \e\nixiang\...
# 或 python FileNotFoundError: '\tmp\trash_paths.XXXXXX'。
nixiang_win_path() {
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -m "$1"
  else
    printf '%s' "$1"
  fi
}

# Prefer brew python3.11 for RE libs (pycryptodome/xxtea/keystone/yara)
# Git Bash 没有 brew，用 PATH 上的 python3（winget 装的 Python 3.11）。
if [ "$NIXIANG_HOST" = "win" ]; then
  # --- 2026-10-04 双机：`python3.11` 垫片 ---
  # 全仓 43 个 .sh 里共 81 处直接调 `python3.11`（mac 侧由 brew 提供，带
  # pycryptodome/xxtea/keystone/yara）。Windows 上没有这个命令名，那些脚本
  # 全部 command not found。改 81 处会动 43 个文件、且与 mac 侧大范围冲突，
  # 因此改为把一个垫片目录前置进 PATH：81 处一行不改，mac 侧永不进这个分支。
  # 垫片本体 = 04_scripts/_common/win_shims/python3.11（按"真导入"打分挑
  # 可用 python，并透明转发 argv/stdin/退出码）。
  # 不 source env.sh 的脚本看不到这个前置，需要机器级安装：
  #   bash 04_scripts/_common/win_shims/install_win_shims.sh
  NIXIANG_SHIM_DIR="$NIXIANG_ROOT/04_scripts/_common/win_shims"
  if [ -d "$NIXIANG_SHIM_DIR" ]; then
    case ":$PATH:" in
      *":$NIXIANG_SHIM_DIR:"*) : ;;
      *) PATH="$NIXIANG_SHIM_DIR:$PATH" ;;
    esac
  fi
  export NIXIANG_SHIM_DIR PATH
  # 2026-10-01 双机协作实测修正：%LOCALAPPDATA%\Microsoft\WindowsApps\ 下的
  # python3.exe 是 App Execution Alias 存根（0 字节 ReparsePoint），执行它会弹
  # Microsoft Store、退出码 9009、`--version` 无输出。旧写法 `command -v python3`
  # 恰好命中它，于是 NIXIANG_PYTHON 不可用 —— 而下面的 `${NIXIANG_PYTHON:-...}`
  # 兜底是 macOS 路径，在 Win 上救不回来。
  # 逐个候选解析，跳过落在 WindowsApps 下的那个；都没有就留空（比指向存根好）。
  NIXIANG_PYTHON=""
  for _pc in python3 python python3.11 python3.12; do
    _pv="$(command -v "$_pc" 2>/dev/null || true)"
    [ -n "$_pv" ] || continue
    case "$_pv" in
      *Microsoft/WindowsApps/*)    continue ;;
      *Microsoft\\WindowsApps\\*)  continue ;;
    esac
    NIXIANG_PYTHON="$_pv"
    break
  done
  unset _pc _pv
  export NIXIANG_PYTHON
elif [ -x /opt/homebrew/bin/python3.11 ]; then
  export NIXIANG_PYTHON=/opt/homebrew/bin/python3.11
else
  export NIXIANG_PYTHON="$(command -v python3)"
fi

# Android SDK build-tools (zipalign / apksigner) — not always on PATH
# Win 上它们是 zipalign.exe / apksigner.bat，而 bash 的 [ -x ] / [ -f ] 不做扩展名
# 补全，必须显式带扩展名判定；否则 SDK 装好了也解析不到（旧写法在 Win 上恒为假）。
# 另外：可执行时 MSYS 会自动补 .exe，所以 NIXIANG_ZIPALIGN 仍可给无扩展名路径，
# 但这里显式解析到真实文件，便于 -x 校验与日志可读。
_resolve_build_tools() {
  local sdk _default_sdk
  if [ "$NIXIANG_HOST" = "win" ]; then
    _default_sdk="${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk"
  else
    _default_sdk="$HOME/Library/Android/sdk"
  fi
  sdk="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-$_default_sdk}}"
  local preferred=(36.0.0 35.0.0 34.0.0)
  local v dir
  if [ ! -d "$sdk/build-tools" ]; then
    return 1
  fi
  for v in "${preferred[@]}"; do
    dir="$sdk/build-tools/$v"
    if [ -x "$dir/zipalign" ] || [ -x "$dir/zipalign.exe" ]; then
      echo "$dir"
      return 0
    fi
  done
  # newest directory name
  dir="$(ls -1 "$sdk/build-tools" 2>/dev/null | sort -V | tail -1)"
  if [ -n "$dir" ] && { [ -x "$sdk/build-tools/$dir/zipalign" ] || [ -x "$sdk/build-tools/$dir/zipalign.exe" ]; }; then
    echo "$sdk/build-tools/$dir"
    return 0
  fi
  return 1
}

NIXIANG_BUILD_TOOLS="$(_resolve_build_tools || true)"
export NIXIANG_BUILD_TOOLS

if [ -n "${NIXIANG_BUILD_TOOLS:-}" ]; then
  _za="$NIXIANG_BUILD_TOOLS/zipalign"
  [ -x "$_za" ] || _za="$NIXIANG_BUILD_TOOLS/zipalign.exe"
  _as="$NIXIANG_BUILD_TOOLS/apksigner"
  # Win 上 apksigner 只有 apksigner.bat，而 MSYS 对 .bat 的 [ -x ] 恒为假（实测
  # zipalign.exe -x=yes / apksigner.bat -x=no,-f=yes），故这里一律用 [ -f ] 判定；
  # 用 [ -x ] 会退回无扩展名路径，脚本里再 [-x] 校验就报 missing apksigner。
  [ -f "$_as" ] || _as="$NIXIANG_BUILD_TOOLS/apksigner.bat"
  [ -f "$_as" ] || _as="$NIXIANG_BUILD_TOOLS/apksigner.exe"
  export NIXIANG_ZIPALIGN="$_za"
  export NIXIANG_APKSIGNER="$_as"
  unset _za _as
else
  export NIXIANG_ZIPALIGN="$(command -v zipalign || true)"
  export NIXIANG_APKSIGNER="$(command -v apksigner || true)"
fi

export NIXIANG_UBER_SIGNER="$NIXIANG_ROOT/01_tools/uber-apk-signer.jar"

# Mod-menu / Gradle builds need JDK 17
# Win：/usr/libexec/java_home 不存在，改从 JAVA_HOME / winget 安装位置 / PATH 探测，
#      并强制要求同级目录有 javac（可过滤掉 C:\Windows\System32\java.exe 这个存根）。
# 2026-10-01 双机协作实测修正：旧写法只校验 bin/javac 存在、并且把 ${JAVA_HOME:-} 当
#      第一候选 —— 于是 JAVA_HOME 指向 JDK 25 时它也会被当作 17。本机当初"解析正确"
#      纯粹是 MSI 装 JDK 17 时顺手改了 JAVA_HOME 的巧合，不是这个函数的功劳。
#      现在每个候选都必须能证明自己是 17：目录名含 17，或 bin/java -version 首行含 "17."。
_is_win_jdk17() {
  local d first
  d="${1//\\//}"          # JAVA_HOME 可能是 C:\...\ 这种反斜杠 + 尾斜杠写法
  d="${d%/}"
  [ -n "$d" ] || return 1
  { [ -f "$d/bin/javac" ] || [ -f "$d/bin/javac.exe" ]; } || return 1
  case "$(basename "$d")" in
    *17*) return 0 ;;
  esac
  first="$("$d/bin/java" -version 2>&1 | head -1)"
  case "$first" in
    *17.*) return 0 ;;
  esac
  return 1
}

_find_win_jdk17() {
  local cand
  for cand in "${JAVA_HOME:-}" \
              "/c/Program Files/Microsoft/jdk-17"* \
              "/c/Program Files/Eclipse Adoptium/jdk-17"* \
              "/c/Program Files/Java/jdk-17"* ; do
    if _is_win_jdk17 "$cand"; then
      cand="${cand//\\//}"
      echo "${cand%/}"
      return 0
    fi
  done
  cand="$(command -v java || true)"
  if [ -n "$cand" ]; then
    cand="$(cd "$(dirname "$cand")/.." 2>/dev/null && pwd)"
    if _is_win_jdk17 "$cand"; then
      echo "$cand"
      return 0
    fi
  fi
  return 1
}

_is_mac_jdk17() {
  local d first
  d="${1:-}"
  d="${d%/}"
  [ -n "$d" ] || return 1
  [ -f "$d/bin/javac" ] || return 1
  case "$(basename "$d")" in
    *17*) return 0 ;;
  esac
  first="$("$d/bin/java" -version 2>&1 | head -1)"
  case "$first" in
    *17.*) return 0 ;;
  esac
  return 1
}

_find_mac_jdk17() {
  local cand
  if _is_mac_jdk17 "${JAVA_HOME:-}"; then
    echo "${JAVA_HOME%/}"
    return 0
  fi
  cand="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
  if _is_mac_jdk17 "$cand"; then
    echo "${cand%/}"
    return 0
  fi
  for cand in /Library/Java/JavaVirtualMachines/*17*/Contents/Home \
              "$HOME/Library/Java/JavaVirtualMachines/"*17*/Contents/Home \
              /opt/homebrew/opt/openjdk@17 \
              /usr/local/opt/openjdk@17 \
              "$HOME"/jdk*17* ; do
    if _is_mac_jdk17 "$cand"; then
      echo "${cand%/}"
      return 0
    fi
  done
  cand="$(command -v java || true)"
  if [ -n "$cand" ]; then
    cand="$(cd "$(dirname "$cand")/.." 2>/dev/null && pwd)"
    if _is_mac_jdk17 "$cand"; then
      echo "$cand"
      return 0
    fi
  fi
  return 1
}

if [ "$NIXIANG_HOST" = "win" ]; then
  _jdk="$( _find_win_jdk17 || true )"
  if [ -n "$_jdk" ]; then
    export NIXIANG_JAVA17="$_jdk"
  fi
  unset _jdk
else
  _jdk="$( _find_mac_jdk17 || true )"
  if [ -n "$_jdk" ]; then
    export NIXIANG_JAVA17="$_jdk"
  fi
  unset _jdk
fi
