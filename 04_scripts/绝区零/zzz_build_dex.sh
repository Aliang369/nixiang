#!/usr/bin/env bash
# 绝区零（zzz）星空菜单 Java 桥 dex 构建（adb-free / gradle-free / smali-free）
#
# 路线依据（照 2026-10-02 冒险与征途 win 本机实测）：
#   - 本机无 gradle 发行版；01_tools 有 apktool/smali，但 javac+d8 更短更快
#   - JDK17 + build-tools d8 全离线
# 产物：mod菜单/starry-mod-menu/templates/zzz_menu_20261005/classes6.dex
#   （官包 classes.dex…classes5.dex ⇒ 星光 = classes6.dex，next_free_dex.py 复核）
#
# 用法：bash 04_scripts/绝区零/zzz_build_dex.sh
set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
SRC="$STAR/app/src/main/java"
TPL_DIR="$STAR/templates/zzz_menu_20261005"
DEX_NAME="classes6.dex"
LOG="$NIXIANG_ROOT/06_dumped/绝区零/_dex_build.log"

die() { echo "ERROR: $*" >&2; exit 1; }

# --- 1. 工具解析 -------------------------------------------------------------
[ -n "${NIXIANG_JAVA17:-}" ] || die "NIXIANG_JAVA17 未解析（env.sh）"
JAVAC="$NIXIANG_JAVA17/bin/javac.exe"
[ -f "$JAVAC" ] || JAVAC="$NIXIANG_JAVA17/bin/javac"
[ -f "$JAVAC" ] || die "javac 不存在: $JAVAC"
JAR_BIN="$NIXIANG_JAVA17/bin/jar.exe"
[ -f "$JAR_BIN" ] || JAR_BIN="$NIXIANG_JAVA17/bin/jar"
[ -f "$JAR_BIN" ] || die "jar 不存在: $JAR_BIN"

[ -n "${NIXIANG_BUILD_TOOLS:-}" ] || die "NIXIANG_BUILD_TOOLS 未解析（env.sh）"
D8="$NIXIANG_BUILD_TOOLS/d8.bat"
[ -f "$D8" ] || D8="$NIXIANG_BUILD_TOOLS/d8"
[ -f "$D8" ] || die "d8 不存在: $NIXIANG_BUILD_TOOLS"

SDK_ROOT="$(cd "$NIXIANG_BUILD_TOOLS/../.." && pwd)"
AJAR=""
for cand in "$SDK_ROOT/platforms/android-36/android.jar" "$SDK_ROOT/platforms/android-35/android.jar"; do
  [ -f "$cand" ] && AJAR="$cand" && break
done
if [ -z "$AJAR" ]; then
  cand="$(ls -1d "$SDK_ROOT/platforms"/android-*/android.jar 2>/dev/null | sort -V | tail -1)"
  [ -n "$cand" ] && AJAR="$cand"
fi
[ -n "$AJAR" ] && [ -f "$AJAR" ] || die "android.jar 未找到（$SDK_ROOT/platforms）"

[ -d "$SRC" ] || die "Java 源目录不存在: $SRC"

# --- 2. ASCII 暂存（中文路径会打死 Windows 侧工具）----------------------------
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) HOST=win ;;
  *) HOST=unix ;;
esac
if [ "$HOST" = "win" ]; then
  STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/zzz_build/dex"
  w() { cygpath -m "$1"; }
else
  STAGE_ROOT="${TMPDIR:-/tmp}/zzz_build/dex"
  w() { printf '%s' "$1"; }
fi
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"

echo "[1/5] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# --- 3. 生成 R 桩（本机无 aapt2 资源编译；仅 MainActivity 用 3 个 id + 1 layout）---
cat > "$STAGE_ROOT/gen/com/starry/magic/R.java" <<'EOF'
package com.starry.magic;
/** 构建期 R 桩：本机无 aapt2 资源编译。仅 MainActivity 引用（游戏内不启动该 Activity）。 */
public final class R {
    private R() {}
    public static final class layout {
        private layout() {}
        public static final int activity_main = 0x7f010001;
    }
    public static final class id {
        private id() {}
        public static final int btn_pass_through_test = 0x7f020001;
        public static final int tv_pass_through_count  = 0x7f020002;
        public static final int btn_request_overlay    = 0x7f020003;
    }
}
EOF

echo "[2/5] javac 编译"
(find "$STAGE_ROOT/src" -name '*.java'; printf '%s\n' "$STAGE_ROOT/gen/com/starry/magic/R.java") > "$STAGE_ROOT/srcs.txt"
: > "$STAGE_ROOT/argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(w "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

# 注意：**不能用 -bootclasspath android.jar**（android.jar 的 LambdaMetafactory 是阉割版，
# 一用就 lambda 找不到符号）。java.* 走 JDK 平台 API（--release 11），android.* 用 -classpath。
"$JAVAC" -nowarn -encoding UTF-8 --release 11 \
  -classpath "$(w "$AJAR")" -d "$(w "$STAGE_ROOT/classes")" \
  "@$(w "$STAGE_ROOT/argv.txt")" > "$STAGE_ROOT/javac.log" 2>&1
JRC=$?
if [ $JRC -ne 0 ]; then
  echo "--- javac 失败（尾部 40 行）---" >&2
  tail -40 "$STAGE_ROOT/javac.log" >&2
  die "javac rc=$JRC"
fi
NCLS="$(find "$STAGE_ROOT/classes" -name '*.class' | wc -l | tr -d ' ')"
echo "      class 文件 $NCLS 个"

echo "[3/5] 打包 class → jar"
"$JAR_BIN" cf "$(w "$STAGE_ROOT/classes.jar")" -C "$(w "$STAGE_ROOT/classes")" . \
  > "$STAGE_ROOT/jar.log" 2>&1 || { tail -20 "$STAGE_ROOT/jar.log" >&2; die "jar 失败"; }

echo "[4/5] d8 → dex（min-api 23 = 官包 minSdk）"
"$D8" --min-api 23 --release --lib "$(w "$AJAR")" \
  --output "$(w "$STAGE_ROOT/out")" "$(w "$STAGE_ROOT/classes.jar")" \
  > "$STAGE_ROOT/d8.log" 2>&1
DRC=$?
if [ $DRC -ne 0 ]; then
  echo "--- d8 失败（尾部 40 行）---" >&2
  tail -40 "$STAGE_ROOT/d8.log" >&2
  die "d8 rc=$DRC"
fi
[ -f "$STAGE_ROOT/out/classes.dex" ] || die "d8 未产出 classes.dex"

echo "[5/5] 字节自检 + 发布"
mkdir -p "$TPL_DIR"
cp -f "$STAGE_ROOT/out/classes.dex" "$TPL_DIR/$DEX_NAME"

"${NIXIANG_PYTHON:-python}" - "$TPL_DIR/$DEX_NAME" "$DEX_NAME" <<'PY' || die "dex 字节自检失败"
import hashlib, sys, re
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [
    b'com/starry/magic/StarryInitProvider',
    b'com/starry/magic/menu/MenuService',
    b'com/starry/magic/bridge/ModBridge',
    b'com/starry/magic/sign/SignSpoof',
    b'com/starry/magic/ota/StarryNativeLoad',
]
miss = [n.decode() for n in need if n not in b]
print('file      : %s' % name)
print('size      : %d B' % len(b))
print('sha256    : %s' % hashlib.sha256(b).hexdigest())
print('md5       : %s' % hashlib.md5(b).hexdigest())
print('magic     : %s' % b[:8].hex())
print('classes   : %d' % len(re.findall(rb'com/starry/magic/', b)))
for n in need:
    print('  %-42s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
if miss:
    print('FAIL missing: %s' % ', '.join(miss)); sys.exit(1)
print('OK')
PY

{
  echo "game      : 绝区零 (zzz)"
  echo "dex       : $DEX_NAME"
  echo "built_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac     : $JAVAC"
  echo "d8        : $D8"
  echo "androidjar: $AJAR"
  echo "source    : $SRC"
  echo "sha256    : $(sha256sum "$TPL_DIR/$DEX_NAME" 2>/dev/null | cut -d' ' -f1)"
} > "$TPL_DIR/build_info.txt"
cp -f "$STAGE_ROOT/d8.log" "$TPL_DIR/d8.log" 2>/dev/null || true
mkdir -p "$(dirname "$LOG")"
{ echo "=== $(date '+%Y-%m-%d %H:%M:%S') ==="; cat "$TPL_DIR/build_info.txt"; } >> "$LOG"

echo "OK → $TPL_DIR/$DEX_NAME"
