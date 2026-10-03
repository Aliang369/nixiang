#!/usr/bin/env bash
# 境界守望者管线环境准备（Windows Git Bash / macOS 通用，source 使用）
#   用法: source 04_scripts/境界守望者/wor_env.sh
# 作用: 定位 JDK17 / Android SDK / NDK / build-tools / 01_tools(smali,baksmali,apktool)
#       并导出 JAVA_HOME、ANDROID_HOME、ANDROID_SDK_ROOT、ANDROID_NDK_HOME、PATH
# 说明: Windows 上 01_tools 里的 smali/baksmali/apktool 是 bash 包装脚本（调 java -jar），
#       须把 01_tools/smali 与 01_tools/apktool 加进 PATH 供 surgical 脚本直接调用。

# shellcheck disable=SC2034
WOR_ENV_SH_LOADED=1

# set -u 兼容：Git Bash 里 USER 可能未导出，退化用 USERNAME
WOR_USER="${USER:-${USERNAME:-Administrator}}"
export WOR_USER

_wor_root() { (cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd); }
NIXIANG_WOR="$(_wor_root)"
export NIXIANG_WOR

# ---- JDK ----
if [[ -z "${JAVA_HOME:-}" || ! -x "${JAVA_HOME:-}/bin/java" ]]; then
  for c in "/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot" \
           "/c/Program Files/Java/jdk-17"* \
           "/c/Program Files/Eclipse Adoptium/jdk-17"* \
           "$HOME/Library/Java/JavaVirtualMachines/jdk-17"* ; do
    [[ -x "$c/bin/java" ]] && export JAVA_HOME="$c" && break
  done
fi
# java 不在 PATH 时单独补
if ! command -v java >/dev/null 2>&1 && [[ -n "${JAVA_HOME:-}" ]]; then
  export PATH="$JAVA_HOME/bin:$PATH"
fi

# ---- Android SDK / NDK ----
if [[ -z "${ANDROID_SDK_ROOT:-}" ]]; then
  for c in "/c/Users/$WOR_USER/AppData/Local/Android/Sdk" \
           "/mnt/c/Users/$WOR_USER/AppData/Local/Android/Sdk" \
           "$HOME/Library/Android/sdk" \
           "$HOME/Android/Sdk"; do
    [[ -d "$c" ]] && export ANDROID_SDK_ROOT="$c" && break
  done
fi
export ANDROID_HOME="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}"

if [[ -z "${ANDROID_NDK_HOME:-}" && -n "${ANDROID_SDK_ROOT:-}" ]]; then
  for v in 27.0.12077973; do
    [[ -f "$ANDROID_SDK_ROOT/ndk/$v/ndk-build.cmd" || -x "$ANDROID_SDK_ROOT/ndk/$v/ndk-build" ]] \
      && export ANDROID_NDK_HOME="$ANDROID_SDK_ROOT/ndk/$v" && break
  done
  if [[ -z "${ANDROID_NDK_HOME:-}" ]]; then
    for c in "$ANDROID_SDK_ROOT/ndk"/*; do
      [[ -f "$c/ndk-build.cmd" || -x "$c/ndk-build" ]] && export ANDROID_NDK_HOME="$c"
    done
  fi
fi

# ---- build-tools（zipalign / apksigner / aapt2 优先 36.1.0 再 35.0.0）----
if [[ -n "${ANDROID_SDK_ROOT:-}" ]]; then
  for v in 36.1.0 35.0.0; do
    if [[ -f "$ANDROID_SDK_ROOT/build-tools/$v/aapt2.exe" || -x "$ANDROID_SDK_ROOT/build-tools/$v/aapt2" ]]; then
      export WOR_BUILD_TOOLS="$ANDROID_SDK_ROOT/build-tools/$v"
      break
    fi
  done
  if [[ -z "${WOR_BUILD_TOOLS:-}" ]]; then
    for c in "$ANDROID_SDK_ROOT/build-tools"/*; do
      [[ -f "$c/aapt2.exe" || -x "$c/aapt2" ]] && export WOR_BUILD_TOOLS="$c"
    done
  fi
fi

# ---- Gradle ----
# 仓库里没有 gradle-wrapper.jar（两份 .gitignore 都有 '*.jar' ⇒ 从未被跟踪），
# 换台机器就必须重新弄一份。这里优先用 01_tools/gradle 下的完整发行版（离线可用），
# 其次 PATH 上的 gradle，最后才退回项目自带的 ./gradlew。
if [[ -z "${WOR_GRADLE_CMD:-}" ]]; then
  for c in "$NIXIANG_WOR/01_tools/gradle/gradle-8.13/bin/gradle" \
           "$NIXIANG_WOR/01_tools/gradle"/*/bin/gradle; do
    [[ -x "$c" ]] && { WOR_GRADLE_CMD="$c"; break; }
  done
fi
if [[ -z "${WOR_GRADLE_CMD:-}" ]]; then
  WOR_GRADLE_CMD="$(command -v gradle 2>/dev/null || true)"
fi
export WOR_GRADLE_CMD

# ---- 01_tools: smali / baksmali / apktool 包装脚本上 PATH ----
export PATH="$NIXIANG_WOR/01_tools/smali:$NIXIANG_WOR/01_tools/apktool:$PATH"
SMALI_BIN="$NIXIANG_WOR/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG_WOR/01_tools/smali/baksmali"
APKTOOL_BIN="$NIXIANG_WOR/01_tools/apktool/apktool"

# ---- Python 输出编码 ----
# Windows 上 CPython 默认 stdout 是 GBK(cp936)，脚本里的 '✓'/'→' 等字符会让内联
# python 片段直接 UnicodeEncodeError 崩掉（实测：surgical 第 [1] 步的自检片段）。
export PYTHONIOENCODING=utf-8
export PYTHONUTF8=1

# ---- adb 与 Git Bash 的路径转换（**踩过的坑，别删**）----
# Git Bash(MSYS2) 会把传给**原生 Windows 程序**的、看起来像 unix 绝对路径的参数
# 自动翻译成本机路径：`adb shell mkdir -p /data/local/tmp/x`
#   → 设备上收到 `mkdir -p C:/Program Files/Git/data/local/tmp/x`
#   → 报 `mkdir: 'C:': Read-only file system` + `mkdir: 'Files': Read-only file system`
# 试过 MSYS2_ARG_CONV_EXCL='/data/*:/sdcard/*' —— **在 Git for Windows 上无效**。
# 有效的是 Git 自己的开关 MSYS_NO_PATHCONV=1，但它顺手也关掉了本机路径转换，
# 所以本机路径必须先 cygpath -m 变成 `E:/...` 再传给 adb.exe。
# 结论：凡是「设备侧绝对路径」的 adb 调用，一律走 adbx。
#   adbx shell mkdir -p /data/local/tmp/x                 ← 设备路径不转换 ✓
#   adbx push "$(cygpath -m "$LOCAL")" /data/local/tmp/x  ← 本机路径自己转 ✓
adbx() { MSYS_NO_PATHCONV=1 command adb "$@"; }
# 设备侧路径不进 argv（塞进 shell 命令串）时，普通 adb 也安全：
#   adb -s S shell "md5sum $DEV_PATH"      ← 参数不以 '/' 开头 ⇒ 不转换 ✓

# ---- md5/sha256（macOS 用 md5 -q / shasum，Windows Git Bash 用 md5sum/sha256sum）----
md5_of()    { if command -v md5    >/dev/null 2>&1; then md5 -q "$1";    else md5sum    "$1" | awk '{print $1}'; fi; }
sha256_of() { if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi; }

wor_env_report() {
  echo "NIXIANG_WOR      = $NIXIANG_WOR"
  echo "JAVA_HOME        = ${JAVA_HOME:-<unset>}"
  echo "ANDROID_SDK_ROOT = ${ANDROID_SDK_ROOT:-<unset>}"
  echo "ANDROID_NDK_HOME = ${ANDROID_NDK_HOME:-<unset>}"
  echo "WOR_BUILD_TOOLS  = ${WOR_BUILD_TOOLS:-<unset>}"
  echo "WOR_GRADLE_CMD   = ${WOR_GRADLE_CMD:-<unset>}"
  echo "java             = $(command -v java || echo MISSING)"
  echo "smali            = $([[ -x $SMALI_BIN ]] && echo "$SMALI_BIN OK" || echo MISSING)"
  echo "baksmali         = $([[ -x $BAKSMALI_BIN ]] && echo "$BAKSMALI_BIN OK" || echo MISSING)"
  echo "apktool          = $([[ -x $APKTOOL_BIN ]] && echo "$APKTOOL_BIN OK" || echo MISSING)"
  echo "zipalign         = $([[ -x ${WOR_BUILD_TOOLS:-/nonexist}/zipalign.exe ]] && echo "${WOR_BUILD_TOOLS}/zipalign.exe" || echo 'MISSING(exe)')"
  echo "apksigner        = $(ls "${WOR_BUILD_TOOLS:-/nonexist}"/apksigner* 2>/dev/null | head -1 || echo MISSING)"
  echo "aapt2            = $(ls "${WOR_BUILD_TOOLS:-/nonexist}"/aapt2* 2>/dev/null | head -1 || echo MISSING)"
}
