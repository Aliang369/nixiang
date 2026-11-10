#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.02.01 — 星光菜单 Java 桥 dex 构建（Windows/MSYS 后端）
#
#  照 04_scripts/我本布衣世家录/wbjy_build_dex.sh（同一路线）：
#    · 本机无 gradle / 无 apktool 编译环境可用性依赖 → javac + d8 全离线
#    · 源码 = mod菜单/starry-mod-menu/app/src/main/java（通用菜单）
#    · 补 R.java / BuildConfig.java 两个构建期桩（本机不做资源编译）
#    · 本作底包是 classes.dex + classes2.dex 双 dex ⇒ 星光 dex 必须 classes3.dex
#      （ART 遇到第一个缺口就停止扫描；写 classes8 会 ClassNotFoundException）
#    · ASCII 暂存固定在工作区 .workbuddy/scratch（gitignored），不写系统盘
#
#  产物：07_patched/爆裂防线/surgical/classes3.dex（供 blfx_surgical_win.sh）
#  用法：bash 04_scripts/爆裂防线/blfx_build_dex_win.sh
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=blfx_win_common.sh
. "$HERE/blfx_win_common.sh"

SRC="$STARRY_SRC/app/src/main/java"
DEX_NAME="${BLFX_DEX_NAME:-classes3.dex}"
MIN_API=23
LOG="$NIXIANG/06_dumped/$GAME/_dex_build_win.log"

die() { echo "ERROR: $*" >&2; exit 1; }

# --- 1. 工具解析 -------------------------------------------------------------
[ -n "${JAVA_HOME_J17:-}" ] || die "NIXIANG_JAVA17 未解析（env.sh）"
JAVAC="$JAVA_HOME_J17/bin/javac.exe"
[ -f "$JAVAC" ] || JAVAC="$JAVA_HOME_J17/bin/javac"
[ -f "$JAVAC" ] || die "javac 不存在: $JAVA_HOME_J17"
JAR_BIN="$JAVA_HOME_J17/bin/jar.exe"
[ -f "$JAR_BIN" ] || JAR_BIN="$JAVA_HOME_J17/bin/jar"
[ -f "$JAR_BIN" ] || die "jar 不存在: $JAVA_HOME_J17"

[ -n "${BUILD_TOOLS:-}" ] || die "NIXIANG_BUILD_TOOLS 未解析（env.sh）"
D8="$BUILD_TOOLS/d8.bat"
[ -f "$D8" ] || D8="$BUILD_TOOLS/d8"
[ -f "$D8" ] || die "d8 不存在: $BUILD_TOOLS"

SDK_ROOT="$(cd "$BUILD_TOOLS/../.." && pwd)"
AJAR=""
for cand in "$SDK_ROOT/platforms/android-36/android.jar" \
            "$SDK_ROOT/platforms/android-35/android.jar" \
            "$SDK_ROOT/platforms/android-34/android.jar"; do
  [ -f "$cand" ] && AJAR="$cand" && break
done
if [ -z "$AJAR" ]; then
  cand="$(ls -1d "$SDK_ROOT/platforms"/android-*/android.jar 2>/dev/null | sort -V | tail -1 || true)"
  [ -n "$cand" ] && AJAR="$cand"
fi
[ -n "$AJAR" ] && [ -f "$AJAR" ] || die "android.jar 未找到（$SDK_ROOT/platforms）"

[ -d "$SRC" ] || die "Java 源目录不存在: $SRC"

STAGE_ROOT="$NIXIANG/.workbuddy/scratch/blfx_build/dex"
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"
mkdir -p "$(dirname "$LOG")"

echo "[1/5] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# --- 2. 生成 R / BuildConfig 桩（与 wbjy 同一套资源 id，菜单 UI 共用）--------
cat > "$STAGE_ROOT/gen/com/starry/magic/R.java" <<'EOF'
package com.starry.magic;
/** 构建期 R 桩：本机不做 aapt2 资源编译。仅供编译期符号解析。 */
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
/** 构建期 BuildConfig 桩：本机无 gradle。 */
public final class BuildConfig {
    private BuildConfig() {}
    public static final boolean DEBUG = false;
    public static final String  VERSION_NAME = "1.02.01";
    public static final int     VERSION_CODE = 10201;
}
EOF

echo "[2/5] javac 编译（$(basename "$AJAR")）"
(
  find "$STAGE_ROOT/src" -name '*.java'
  printf '%s\n' "$STAGE_ROOT/gen/com/starry/magic/R.java" "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java"
) > "$STAGE_ROOT/srcs.txt"
: > "$STAGE_ROOT/argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(winpath "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

JRC=0
"$JAVAC" -nowarn -encoding UTF-8 --release 11 \
  -classpath "$(winpath "$AJAR")" -d "$(winpath "$STAGE_ROOT/classes")" \
  "@$(winpath "$STAGE_ROOT/argv.txt")" > "$STAGE_ROOT/javac.log" 2>&1 || JRC=$?
if [ "$JRC" -ne 0 ]; then
  echo "--- javac 失败（尾部 40 行）---" >&2
  tail -40 "$STAGE_ROOT/javac.log" >&2
  die "javac rc=$JRC"
fi
NCLS="$(find "$STAGE_ROOT/classes" -name '*.class' | wc -l | tr -d ' ')"
echo "      class 文件 $NCLS 个"

echo "[3/5] 打包 class → jar"
"$JAR_BIN" cf "$(winpath "$STAGE_ROOT/classes.jar")" -C "$(winpath "$STAGE_ROOT/classes")" . \
  > "$STAGE_ROOT/jar.log" 2>&1 || { tail -20 "$STAGE_ROOT/jar.log" >&2; die "jar 失败"; }

echo "[4/5] R8 混淆 → dex（min-api ${MIN_API}）"
# 2026-11-10：原先裸 `d8 --release` 只去调试信息、不混淆 ⇒ 注入 dex 明文。
# 改走统一入口 obfuscate_menu_dex.sh（组件类留原名 / 业务类改名到 com/starry/magic/o/**）。
OBF="$NIXIANG_ROOT/04_scripts/_common/obfuscate_menu_dex.sh"
[ -f "$OBF" ] || die "缺混淆脚本: $OBF"
DRC=0
bash "$OBF" "$(winpath "$STAGE_ROOT/classes.jar")" "$(winpath "$STAGE_ROOT/out/classes.dex")" \
  > "$STAGE_ROOT/r8.log" 2>&1 || DRC=$?
if [ "$DRC" -ne 0 ]; then
  echo "--- R8 失败（尾部 40 行）---" >&2
  tail -40 "$STAGE_ROOT/r8.log" >&2
  die "R8 混淆失败 rc=$DRC"
fi
[ -f "$STAGE_ROOT/out/classes.dex" ] || die "R8 未产出 classes.dex"

echo "[5/5] 字节自检 + 发布 → $SURGICAL_DIR/$DEX_NAME"
mkdir -p "$SURGICAL_DIR"
cp -f "$STAGE_ROOT/out/classes.dex" "$SURGICAL_DIR/$DEX_NAME"

"${NIXIANG_PYTHON:-python}" - "$SURGICAL_DIR/$DEX_NAME" "$DEX_NAME" <<'PY' || die "dex 字节自检失败"
import hashlib, sys, re
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [
    b'com/starry/magic/StarryInitProvider',
    b'com/starry/magic/menu/MenuService',
    b'com/starry/magic/bridge/ModBridge',
    b'com/starry/magic/sign/StarryAppComponentFactory',
    b'com/starry/magic/sign/TapLoginForceWeb',
]
miss = [n.decode() for n in need if n not in b]
print('file      : %s' % name)
print('size      : %d B' % len(b))
print('sha256    : %s' % hashlib.sha256(b).hexdigest())
print('md5       : %s' % hashlib.md5(b).hexdigest())
print('magic     : %s' % b[:8].hex())
for n in need:
    print('  %-46s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
if miss:
    print('FAIL missing: %s' % ', '.join(miss)); sys.exit(1)
print('OK')
PY

{
  echo "game      : $GAME ($TAG)"
  echo "dex       : $DEX_NAME"
  echo "built_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac     : $JAVAC"
  echo "d8        : $D8"
  echo "androidjar: $AJAR"
  echo "sha256    : $(sha256f "$SURGICAL_DIR/$DEX_NAME")"
} > "$LOG"
echo "[build_dex] done → $SURGICAL_DIR/$DEX_NAME ($(md5f "$SURGICAL_DIR/$DEX_NAME"))"
