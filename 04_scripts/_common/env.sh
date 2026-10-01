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

# Prefer brew python3.11 for RE libs (pycryptodome/xxtea/keystone/yara)
# Git Bash 没有 brew，用 PATH 上的 python3（winget 装的 Python 3.11）。
if [ "$NIXIANG_HOST" = "win" ]; then
  export NIXIANG_PYTHON="$(command -v python3 || command -v python)"
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
  [ -x "$_as" ] || _as="$NIXIANG_BUILD_TOOLS/apksigner.bat"
  [ -x "$_as" ] || _as="$NIXIANG_BUILD_TOOLS/apksigner"
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
_find_win_jdk17() {
  local cand jb
  cand="${JAVA_HOME:-}"
  for cand in "$cand" \
              "/c/Program Files/Microsoft/jdk-17"* \
              "/c/Program Files/Eclipse Adoptium/jdk-17"* \
              "/c/Program Files/Java/jdk-17"* ; do
    if [ -n "$cand" ] && { [ -f "$cand/bin/javac" ] || [ -f "$cand/bin/javac.exe" ]; }; then
      echo "$cand"
      return 0
    fi
  done
  jb="$(command -v java || true)"
  if [ -n "$jb" ]; then
    cand="$(cd "$(dirname "$jb")/.." 2>/dev/null && pwd)"
    if [ -n "$cand" ] && { [ -f "$cand/bin/javac" ] || [ -f "$cand/bin/javac.exe" ]; }; then
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
  if [ -z "${JAVA_HOME:-}" ] || ! java -version 2>&1 | head -1 | grep -q '17\.'; then
    if JAVA17="$(/usr/libexec/java_home -v 17 2>/dev/null)"; then
      export NIXIANG_JAVA17="$JAVA17"
    fi
  fi
fi
