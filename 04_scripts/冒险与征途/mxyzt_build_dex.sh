#!/usr/bin/env bash
# 冒险与征途（mxyzt）星空菜单 Java 桥 dex 构建（adb-free / gradle-free / smali-free）
#
# 为什么自研这条路（2026-10-02 win 本机实测）：
#   - 本机没有 gradle 发行版（~/.gradle 无 wrapper/dists，仓库 gradle-wrapper.jar 缺失）
#   - 本机没有 smali/baksmali / apktool（01_tools 下只有 phantom-frida）
#   - 但本机有 JDK17 + build-tools 36.1.0 的 d8 → 走 javac + d8 全离线路线
# 产物：mod菜单/starry-mod-menu/templates/mxyzt_menu_current/classes4.dex
#   （dex 名必须是 base.apk 里的最小空闲下标：官包 classes.dex/1/2/3 → 我们 = classes4.dex）
#
# 用法：bash 04_scripts/冒险与征途/mxyzt_build_dex.sh
# 变更后自检：bash 04_scripts/_common/preflight.sh 04_scripts/冒险与征途/mxyzt_build_dex.sh

set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
SRC="$STAR/app/src/main/java"
TPL_DIR="$STAR/templates/mxyzt_menu_current"
DEX_NAME="classes4.dex"
LOG="$NIXIANG_ROOT/06_dumped/冒险与征途/_dex_build.log"

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

# android.jar：优先 android-36（本机唯一），否则取 platforms 下最大版本
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
  STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/mxyzt_build/dex"
  w() { cygpath -m "$1"; }          # Windows 侧工具认正斜杠
else
  STAGE_ROOT="${TMPDIR:-/tmp}/mxyzt_build/dex"
  w() { printf '%s' "$1"; }
fi
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"

echo "[1/5] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# --- 3. 生成 R 桩（本机无 aapt2 资源编译；仅 MainActivity 用到 4 个 id）-------
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
while IFS= read -r f; do printf '\"%s\"\n' "$(w "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

# 注意：**不能用 -bootclasspath android.jar**。android.jar 里的
# java.lang.invoke.LambdaMetafactory 是阉割版（没有 metafactory 方法），javac 一旦以它
# 当 bootclasspath，任何 lambda 都会报「找不到符号 metafactory」（2026-10-02 实测）。
# 正确姿势：java.* 用 JDK 平台 API（--release 11），android.* 用 -classpath android.jar。
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
NWARN="$(grep -c 'warning' "$STAGE_ROOT/javac.log" 2>/dev/null || true)"
echo "      class 文件 $NCLS 个（javac 日志含 warning 行 ${NWARN:-0}）"

echo "[3/5] 打包 class → jar"
"$JAR_BIN" cf "$(w "$STAGE_ROOT/classes.jar")" -C "$(w "$STAGE_ROOT/classes")" . \
  > "$STAGE_ROOT/jar.log" 2>&1 || { tail -20 "$STAGE_ROOT/jar.log" >&2; die "jar rc=$?"; }

echo "[4/5] d8 → dex（min-api 22）"
"$D8" --min-api 22 --release --lib "$(w "$AJAR")" \
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
    b'com.mxyzt.gzmy',                 # SignSpoof 名单里必须有本作包名
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
# 注意：不要做「本 dex 不许出现别家包名」的检查——SignSpoof 的名单本身就是
# 多游戏共享的白名单（含 com.wjtxdtt.zy 等），出现别家包名是**预期**行为。
print('OK')
PY

{
  echo "game      : 冒险与征途 (mxyzt)"
  echo "dex       : $DEX_NAME"
  echo "built_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac     : $JAVAC"
  echo "d8        : $D8"
  echo "androidjar: $AJAR"
  echo "source    : $SRC"
  echo "sha256    : $(sha256sum "$TPL_DIR/$DEX_NAME" 2>/dev/null | cut -d' ' -f1)"
  echo "source_commit: $(git -C "$NIXIANG_ROOT" log -1 --format=%H -- mod菜单/starry-mod-menu/app/src/main/java 2>/dev/null)"
} > "$TPL_DIR/build_info.txt"
cp -f "$STAGE_ROOT/d8.log" "$TPL_DIR/d8.log" 2>/dev/null || true
mkdir -p "$(dirname "$LOG")"
{ echo "=== $(date '+%Y-%m-%d %H:%M:%S') ==="; cat "$TPL_DIR/build_info.txt"; } >> "$LOG"

echo "OK → $TPL_DIR/$DEX_NAME"

# 菜单模板新鲜度自检（2026-10-26 起）：build_info.source_commit 必须等于当前 Java 源码 HEAD
bash "$NIXIANG_ROOT/04_scripts/_common/menu_tpl_gate.sh" --tpl "$TPL_DIR" || {
  echo "ERROR: 模板新鲜度门禁未过（build_info 与源码不符，先重跑本脚本）" >&2; exit 1
}
