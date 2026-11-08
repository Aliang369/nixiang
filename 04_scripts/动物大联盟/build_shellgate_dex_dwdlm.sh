#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 从 Java 源码重建 ShellGate 桩 dex（classes4.dex）。
#
# 三个口径（用 DWDLM_SHELLGATE_MODE 选，默认 sl6）：
#   sl3 = 纯 Java 空实现、无 <clinit>（libsecret.so 不加载）。已实测：能进 Cocos/过数据检查，
#         但随后弹「安全提示·不安全的设备环境」并主动 finish。
#   sl4 = <clinit>: System.loadLibrary("secret") 包在 try/catch 内，require() 为空实现。
#         已实测：dlopen libsecret.so ok，但 JNI_OnLoad 返回 JNI_ERR → UnsatisfiedLinkError 被吞。
#   sl6 = <clinit>: System.loadLibrary("secret") 无 try/catch；require()/apiTable() 声明为 native，
#         不写方法体（与官方一致）。目的：让失败自然抛给调用方，判定数据检查是否依赖壳。
#
# 产物：04_scripts/动物大联盟/assets/shellgate/classes4.dex
# 门禁：d8 后 dexdump 必须按口径出现/不出现 <clinit>()V；方法表只允许 <clinit>/<init>/require/apiTable；
#       sl6 还要求 require/apiTable 的 access 含 ACC_NATIVE(0x0100) 且源码无 catch。
set -euo pipefail

MODE="${DWDLM_SHELLGATE_MODE:-sl6}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC_DIR="$SCRIPT_DIR/assets/shellgate"
JAVA_SRC="$SRC_DIR/com/mars/secure/ShellGate.java"
OUT_DEX="${DWDLM_SHELLGATE_OUT:-$SRC_DIR/classes4.dex}"
BT="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
ANDROID_JAR="${ANDROID_JAR:-$HOME/Library/Android/sdk/platforms/android-36/android.jar}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$JAVA_SRC" ]] || { echo "ERROR: 缺源码 $JAVA_SRC" >&2; exit 1; }
[[ -f "$ANDROID_JAR" ]] || { echo "ERROR: 缺 android.jar $ANDROID_JAR" >&2; exit 1; }
[[ -x "$BT/d8" ]] || { echo "ERROR: 缺 d8 $BT/d8" >&2; exit 1; }

# ---- 源码口径自检（防止拿错源码编出别的口径）--------------------------------
if [[ "$MODE" == "sl3" ]]; then
  ! grep -q 'System.loadLibrary' "$JAVA_SRC" || {
    echo "ERROR: sl3 口径源码不应含 loadLibrary" >&2; exit 1; }
elif [[ "$MODE" == "sl4" ]]; then
  grep -q 'System.loadLibrary("secret")' "$JAVA_SRC" || {
    echo 'ERROR: sl4 口径源码缺 System.loadLibrary("secret")' >&2; exit 1; }
  ! grep -q 'native ' "$JAVA_SRC" || {
    echo "ERROR: sl4 口径源码不应含 native 声明（那是 sl6）" >&2; exit 1; }
elif [[ "$MODE" == "sl6" ]]; then
  grep -q 'System.loadLibrary("secret")' "$JAVA_SRC" || {
    echo 'ERROR: sl6 口径源码缺 System.loadLibrary("secret")' >&2; exit 1; }
  grep -q 'public static native void require();' "$JAVA_SRC" || {
    echo "ERROR: sl6 口径源码缺 native require()" >&2; exit 1; }
  grep -q 'public static native long apiTable();' "$JAVA_SRC" || {
    echo "ERROR: sl6 口径源码缺 native apiTable()" >&2; exit 1; }
  ! grep -q 'catch (' "$JAVA_SRC" || {
    echo "ERROR: sl6 口径源码不得有 try/catch（要让异常自然抛出）" >&2; exit 1; }
else
  echo "ERROR: 未知 DWDLM_SHELLGATE_MODE=${MODE}（只支持 sl3/sl4/sl6）" >&2; exit 1
fi

WORK="$(mktemp -d /tmp/dwdlm_shellgate.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/classes" "$WORK/dex"

echo "[shellgate] mode=$MODE  src=$JAVA_SRC"
javac -source 8 -target 8 -nowarn -encoding UTF-8 \
  -classpath "$ANDROID_JAR" -d "$WORK/classes" "$JAVA_SRC"
"$BT/d8" --min-api 21 --lib "$ANDROID_JAR" --output "$WORK/dex" \
  "$WORK/classes/dev/mars/secure/ShellGate.class"
cp "$WORK/dex/classes.dex" "$OUT_DEX"

# ---- 门禁 -----------------------------------------------------------------
DUMP="$("$BT/dexdump" "$OUT_DEX" 2>&1)"
echo "$DUMP" | grep -q "Class descriptor  : 'Ldev/mars/secure/ShellGate;'" || {
  echo "ERROR: dex 内无 ShellGate" >&2; exit 1; }
HAS_CLINIT=0
echo "$DUMP" | grep -q "name          : '<clinit>'" && HAS_CLINIT=1
if [[ "$MODE" != "sl3" && "$HAS_CLINIT" -ne 1 ]]; then
  echo "ERROR: $MODE 口径 dex 缺 <clinit>" >&2; exit 1
fi
if [[ "$MODE" == "sl3" && "$HAS_CLINIT" -ne 0 ]]; then
  echo "ERROR: sl3 口径 dex 不应有 <clinit>" >&2; exit 1
fi
# 方法名白名单：只允许 <clinit> / <init> / require / apiTable（不得多出任何方法）
EXTRA="$(echo "$DUMP" | sed -n "s/^      name          : '\(.*\)'$/\1/p" | grep -vE '^(<clinit>|<init>|require|apiTable)$' || true)"
if [[ -n "$EXTRA" ]]; then
  echo "ERROR: dex 出现白名单外方法: $EXTRA" >&2; exit 1
fi
# sl6：require/apiTable 必须是 native（access 行含 NATIVE；ACC_NATIVE=0x0100）
if [[ "$MODE" == "sl6" ]]; then
  for m in require apiTable; do
    echo "$DUMP" | grep -A2 "name          : '$m'" | grep -q 'NATIVE' || {
      echo "ERROR: sl6 口径 $m 不是 native" >&2; exit 1; }
  done
fi
N_METHODS="$(echo "$DUMP" | grep -c "^      name          : '" || true)"
echo "[shellgate] <clinit>=$HAS_CLINIT  methods=$N_METHODS  size=$(stat -f '%z' "$OUT_DEX") B"
echo "[shellgate] sha256=$(shasum -a 256 "$OUT_DEX" | awk '{print $1}')"
echo "[shellgate] ok $OUT_DEX  (mode=$MODE)"
