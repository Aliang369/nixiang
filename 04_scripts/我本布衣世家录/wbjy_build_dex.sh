#!/usr/bin/env bash
# 我本布衣：世家录（wbjy）星光菜单 Java 桥 dex 构建（adb-free / gradle-free / smali-free）
#
# 照 04_scripts/魂坠深境/hzsj_build_dex.sh（2026-10-05 绝区零 zzz_build_dex.sh win 实测路线）：
#   - 本机无 gradle 发行版；javac + d8 全离线（JDK17 + build-tools）
#   - 源码 = mod菜单/starry-mod-menu/app/src/main/java（全量通用菜单）
#   - 补 R.java / BuildConfig.java 两个构建期桩（本机无 aapt2 资源编译）
#   - 产物业内 dex 序号 = 包内最大 classesN.dex 的下一位；1.1.66 官方只到 classes3.dex ⇒ classes4.dex
#     （WBJY_DEX_NAME 可覆盖）。**surgical_wbjy_1.1.66.sh 的 NEW_DEX 必须与此一致**
#   - ASCII 暂存固定在工作区 .workbuddy/scratch（gitignored），不写系统盘
#
# 用法：bash 04_scripts/我本布衣世家录/wbjy_build_dex.sh
set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
SRC="$STAR/app/src/main/java"
TPL_DIR="$STAR/templates/wbjy_menu_current"
DEX_NAME="${WBJY_DEX_NAME:-classes4.dex}"
LOG="$NIXIANG_ROOT/06_dumped/我本布衣世家录/_dex_build.log"

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
STAGE_ROOT="$NIXIANG_ROOT/.workbuddy/scratch/wbjy_build/dex"
if [ "$HOST" = "win" ]; then
  w() { cygpath -m "$1"; }
else
  w() { printf '%s' "$1"; }
fi
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"

echo "[1/5] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# --- 3. 生成 R / BuildConfig 桩 ---
cat > "$STAGE_ROOT/gen/com/starry/magic/R.java" <<'EOF'
package com.starry.magic;
/** 构建期 R 桩：本机无 aapt2 资源编译。仅供编译期符号解析（游戏内不启动该 Activity）。 */
public final class R {
    private R() {}
    public static final class layout {
        private layout() {}
        public static final int activity_main = 0x7f010001;
    }
    public static final class drawable {
        private drawable() {}
        public static final int ic_menu_info_details  = 0x7f040001;
        public static final int stat_sys_download_done = 0x7f040002;
    }
    public static final class string {
        private string() {}
        public static final int copy      = 0x7f050001;
        public static final int cut       = 0x7f050002;
        public static final int paste     = 0x7f050003;
        public static final int selectAll = 0x7f050004;
    }
    public static final class id {
        private id() {}
        public static final int background            = 0x7f020001;
        public static final int btn_cdn_inject        = 0x7f020002;
        public static final int btn_diag_copy         = 0x7f020003;
        public static final int btn_diag_refresh      = 0x7f020004;
        public static final int btn_diag_share        = 0x7f020005;
        public static final int btn_pass_through_test = 0x7f020006;
        public static final int btn_request_allfiles  = 0x7f020007;
        public static final int btn_request_overlay   = 0x7f020008;
        public static final int cb_cdn_auto_close     = 0x7f020009;
        public static final int copy                  = 0x7f02000a;
        public static final int cut                   = 0x7f02000b;
        public static final int paste                 = 0x7f02000c;
        public static final int progress              = 0x7f02000d;
        public static final int secondaryProgress     = 0x7f02000e;
        public static final int selectAll             = 0x7f02000f;
        public static final int tv_cdn_status         = 0x7f020010;
        public static final int tv_diag_detail        = 0x7f020011;
        public static final int tv_diag_headline      = 0x7f020012;
        public static final int tv_pass_through_count = 0x7f020013;
    }
}
EOF

cat > "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java" <<'EOF'
package com.starry.magic;
/** 构建期 BuildConfig 桩：本机无 gradle（Diag 读版本串 / MainActivity 读 DEBUG）。 */
public final class BuildConfig {
    private BuildConfig() {}
    public static final boolean DEBUG = false;
    public static final String  VERSION_NAME = "1.1.66";
    public static final int     VERSION_CODE = 94;
}
EOF

echo "[2/5] javac 编译"
(find "$STAGE_ROOT/src" -name '*.java'; printf '%s\n' "$STAGE_ROOT/gen/com/starry/magic/R.java" "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java") > "$STAGE_ROOT/srcs.txt"
: > "$STAGE_ROOT/argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(w "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

# 注意：不能用 -bootclasspath android.jar（android.jar 的 LambdaMetafactory 是阉割版）
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

echo "[4/5] d8 → dex（min-api 24 = 官包 minSdk）"
"$D8" --min-api 24 --release --lib "$(w "$AJAR")" \
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
    b'com/starry/magic/ota/StarryNativeLoad',
    b'com/starry/magic/StarryApp',
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
  echo "game      : 我本布衣世家录 (wbjy)"
  echo "dex       : $DEX_NAME"
  echo "built_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac     : $JAVAC"
  echo "d8        : $D8"
  echo "androidjar: $AJAR"
  echo "source    : $SRC"
  echo "sha256    : $( (sha256sum "$TPL_DIR/$DEX_NAME" 2>/dev/null || shasum -a 256 "$TPL_DIR/$DEX_NAME") | cut -d' ' -f1)"
  echo "source_commit: $(git -C "$NIXIANG_ROOT" log -1 --format=%H -- mod菜单/starry-mod-menu/app/src/main/java 2>/dev/null)"
} > "$TPL_DIR/build_info.txt"
cp -f "$STAGE_ROOT/d8.log" "$TPL_DIR/d8.log" 2>/dev/null || true
mkdir -p "$(dirname "$LOG")"
{ echo "=== $(date '+%Y-%m-%d %H:%M:%S') ==="; cat "$TPL_DIR/build_info.txt"; } >> "$LOG"

echo "OK → $TPL_DIR/$DEX_NAME"

# 菜单模板新鲜度自检（build_info.source_commit 必须等于当前 Java 源码 HEAD）
bash "$NIXIANG_ROOT/04_scripts/_common/menu_tpl_gate.sh" --tpl "$TPL_DIR" || {
  echo "ERROR: 模板新鲜度门禁未过（build_info 与源码不符，先重跑本脚本）" >&2; exit 1
}
