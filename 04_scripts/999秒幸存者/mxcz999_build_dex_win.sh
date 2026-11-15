#!/usr/bin/env bash
# ================================================================
#  999秒!幸存者 V0.0866 (vc866) — 星光菜单 Java 桥 dex 构建（Windows/MSYS 全离线）
#
#  照 04_scripts/爆裂防线/blfx_build_dex_win.sh（同一路线：javac + jar + R8，不跑 gradle）：
#    · 不依赖 gradle、不读也不写 app/src/main/jniLibs（那是并行会话 abssnmxt 的产物）
#    · 源码 = mod菜单/starry-mod-menu/app/src/main/java（通用框架菜单）
#    · 补 R.java / BuildConfig.java 两个构建期桩（本机不做 aapt2 资源编译）
#    · 底包 dex = classes / classes2 / classes3 三份，next_free_dex.py 实测空位 = 4
#      ⇒ 星光 dex 必须是 classes4.dex（ART 遇首个缺口即停扫，写大类号会 ClassNotFoundException）
#    · 仓库路径含中文（mod菜单）⇒ 全部编译在 ASCII 暂存目录 .workbuddy/scratch 完成
#
#  产物：
#    07_patched/999秒幸存者/surgical/classes4.dex
#    07_patched/999秒幸存者/surgical/build_info.txt（source_commit / 源码树 sha256 / built_at / dex md5）
#
#  用法：bash 04_scripts/999秒幸存者/mxcz999_build_dex_win.sh
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
[ -f "$NIXIANG/04_scripts/_common/env.sh" ] && source "$NIXIANG/04_scripts/_common/env.sh"

GAME="999秒幸存者"
PKG="com.HaloGames.Survivors999"
VERSION="V0.0866"
VC="866"
TAG="Mxcz999"
DEX_NAME="${MXCZ999_DEX_NAME:-classes4.dex}"   # next_free_dex.py 对底包实测 = 4
MIN_API=23

STARRY_SRC="${NIXIANG}/mod菜单/starry-mod-menu"
SRC="$STARRY_SRC/app/src/main/java"
OUT_DIR="${NIXIANG}/07_patched/$GAME/surgical"
DEX_OUT="$OUT_DIR/$DEX_NAME"
INFO_OUT="$OUT_DIR/build_info.txt"
STAGE_ROOT="${NIXIANG}/.workbuddy/scratch/mxcz999_build/dex"

die() { echo "ERROR: $*" >&2; exit 1; }
md5f()    { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
sha256f() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }
winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m -- "$1"; else printf '%s' "$1"; fi; }

# ---- 1. 工具解析 ----
J17="${NIXIANG_JAVA17:-${JAVA_HOME:-}}"
[ -n "$J17" ] || die "JAVA 17 未解析（env.sh 的 NIXIANG_JAVA17）"
JAVAC="$J17/bin/javac.exe"; [ -f "$JAVAC" ] || JAVAC="$J17/bin/javac"
[ -f "$JAVAC" ] || die "javac 不存在: $J17"
JAR_BIN="$J17/bin/jar.exe"; [ -f "$JAR_BIN" ] || JAR_BIN="$J17/bin/jar"
[ -f "$JAR_BIN" ] || die "jar 不存在: $J17"

BUILD_TOOLS="${NIXIANG_BUILD_TOOLS:-}"
[ -n "$BUILD_TOOLS" ] && [ -d "$BUILD_TOOLS" ] || die "NIXIANG_BUILD_TOOLS 未解析（env.sh）"
D8="$BUILD_TOOLS/d8.bat"; [ -f "$D8" ] || D8="$BUILD_TOOLS/d8"
[ -f "$D8" ] || die "d8 不存在: $BUILD_TOOLS"

SDK_ROOT="$(cd "$BUILD_TOOLS/../.." && pwd)"
AJAR=""
for cand in "$SDK_ROOT/platforms/android-36/android.jar" \
            "$SDK_ROOT/platforms/android-35/android.jar" \
            "$SDK_ROOT/platforms/android-34/android.jar"; do
  if [ -f "$cand" ]; then AJAR="$cand"; break; fi
done
if [ -z "$AJAR" ]; then
  AJAR="$(ls -1d "$SDK_ROOT"/platforms/android-*/android.jar 2>/dev/null | sort -V | tail -1 || true)"
fi
[ -n "$AJAR" ] && [ -f "$AJAR" ] || die "android.jar 未找到（$SDK_ROOT/platforms）"
OBF="$NIXIANG/04_scripts/_common/obfuscate_menu_dex.sh"
[ -f "$OBF" ] || die "缺混淆脚本: $OBF"
[ -d "$SRC" ] || die "Java 源目录不存在: $SRC"

# ---- 2. 构建戳（源码树身份，供 menu_tpl_gate / build_info 用）----
SRC_COMMIT="$(git -C "$NIXIANG" log -1 --format=%H -- "mod菜单/starry-mod-menu/app/src/main/java" 2>/dev/null || true)"
SRC_COMMIT12="$(printf '%s' "$SRC_COMMIT" | cut -c1-12)"
SRC_TREE_SHA="$( (cd "$SRC" && find . -name '*.java' -print0 | sort -z | xargs -0 cat | sha256sum) 2>/dev/null | awk '{print $1}' )"
[ -n "$SRC_TREE_SHA" ] || die "源码树 sha256 计算失败"
[ -n "$SRC_COMMIT12" ] || SRC_COMMIT12="unknown"

echo "[1/6] $GAME $VERSION vc$VC → ${DEX_NAME}（tag=${TAG}）"
echo "      src commit = ${SRC_COMMIT12}  tree sha256 = $SRC_TREE_SHA"
echo "      javac      = $JAVAC"
echo "      android.jar= $AJAR"
echo "      jniLibs    = 不读不写（javac+d8 路线，与 abssnmxt 会话零冲突）"

# ---- 3. ASCII 暂存 + 源码拷贝 ----
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"
echo "[2/6] ASCII 暂存：$STAGE_ROOT"

# R.java 桩：与 blfx/wbjy 同一套资源 id（菜单 UI 共用一套资源名）
cat > "$STAGE_ROOT/gen/com/starry/magic/R.java" <<'EOF'
package com.starry.magic;
/** 构建期 R 桩：本机不做 aapt2 资源编译，仅供编译期符号解析。 */
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

# BuildConfig 桩：额外塞入构建戳 starry-menu:<commit12>（menu_tpl_gate 认这个串；
# 用 -keep 规则文件保住，见 [5/6] 的 keep.pro）
cat > "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java" <<EOF
package com.starry.magic;
/** 构建期 BuildConfig 桩：本机无 gradle。 */
public final class BuildConfig {
    private BuildConfig() {}
    public static final boolean DEBUG = false;
    public static final String  VERSION_NAME = "${VERSION}";
    public static final int     VERSION_CODE = ${VC};
    public static final String  APPLICATION_ID = "${PKG}";
    /** 菜单 dex 构建戳：与 build_info.txt 的 source_commit 同源。 */
    public static final String  STARRY_MENU_STAMP = "starry-menu:${SRC_COMMIT12}";
}
EOF

cat > "$STAGE_ROOT/keep.pro" <<'EOF'
# 保住构建戳常量（R8 会丢未引用的 static final String；menu_tpl_gate 按字节找 starry-menu:<commit12>）
-keep class com.starry.magic.BuildConfig { *; }
EOF
echo "[3/6] 生成 R/BuildConfig 桩 + keep.pro"

# ---- 4. javac ----
# 只编框架本体：`games/**` 是「单游戏 SPI」目录（GameHooks 按约定名反射装载，缺了自动退空实现）。
# 本游戏走 TapTap 改包法，不需要 GameHookImpl；且该目录当前含断链文件
# （games/qxzxcz/GameHookImpl.java 引用不存在的 core.TapTapWebLogin，仓库 HEAD 状态即如此），
# 编进来会让整批 javac 失败。要连 games/** 一起编：MXCZ999_INCLUDE_GAMES=1。
INCLUDE_GAMES="${MXCZ999_INCLUDE_GAMES:-0}"
(
  if [ "$INCLUDE_GAMES" = "1" ]; then
    find "$STAGE_ROOT/src" -name '*.java'
  else
    find "$STAGE_ROOT/src" -name '*.java' -not -path '*/com/starry/magic/games/*'
  fi
  printf '%s\n' "$STAGE_ROOT/gen/com/starry/magic/R.java" "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java"
) > "$STAGE_ROOT/srcs.txt"
echo "[3/6] 编译单元 $(wc -l < "$STAGE_ROOT/srcs.txt" | tr -d ' ') 个（INCLUDE_GAMES=${INCLUDE_GAMES}）"
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
echo "[4/6] javac OK：$NCLS 个 class"

# ---- 5. jar → R8 混淆 dex ----
"$JAR_BIN" cf "$(winpath "$STAGE_ROOT/classes.jar")" -C "$(winpath "$STAGE_ROOT/classes")" . \
  > "$STAGE_ROOT/jar.log" 2>&1 || { tail -20 "$STAGE_ROOT/jar.log" >&2; die "jar 失败"; }

# --- 5a. 先找 r8.jar（与 obfuscate_menu_dex.sh 候选路径一致 + NIXIANG_R8_JAR 覆盖）---
R8_JAR="${NIXIANG_R8_JAR:-}"
if [ -z "$R8_JAR" ]; then
  for c in \
    "${ANDROID_SDK_ROOT:-}/cmdline-tools/latest/lib/r8.jar" \
    "$SDK_ROOT"/cmdline-tools/*/lib/r8.jar \
    "${LOCALAPPDATA:-}/Android/Sdk/cmdline-tools/latest/lib/r8.jar" \
    "$NIXIANG/01_tools/android-sdk/cmdline-tools/latest/lib/r8.jar"; do
    if [ -f "$c" ]; then R8_JAR="$c"; break; fi
  done
fi

OBF_MODE=""
if [ -n "$R8_JAR" ] && [ -f "$R8_JAR" ]; then
  echo "[5/6] R8 混淆（${R8_JAR}）"
  DRC=0
  NIXIANG_R8_JAR="$R8_JAR" bash "$OBF" "$(winpath "$STAGE_ROOT/classes.jar")" \
    "$(winpath "$STAGE_ROOT/out/classes.dex")" \
    --keep "$(winpath "$STAGE_ROOT/keep.pro")" > "$STAGE_ROOT/r8.log" 2>&1 || DRC=$?
  if [ "$DRC" -ne 0 ]; then
    echo "--- R8 失败（尾部 40 行）---" >&2
    tail -40 "$STAGE_ROOT/r8.log" >&2
    die "R8 混淆失败 rc=$DRC"
  fi
  [ -f "$STAGE_ROOT/out/classes.dex" ] || die "R8 未产出 classes.dex"
  OBF_MODE="enabled(r8.jar=$R8_JAR)"
else
  echo "" >&2
  echo "############################################################" >&2
  echo "#  !! R8 不可用 => 降级 d8 --release（不混淆）" >&2
  echo "#     类名会暴露 com.starry.magic.**（协议串可被 strings 读出）" >&2
  echo "#     本产物只许做「功能验证包」，不得当正式分发包" >&2
  echo "#     放行开关：MXCZ999_ALLOW_UNOBFUSCATED=1" >&2
  echo "############################################################" >&2
  echo "" >&2
  if [ "${MXCZ999_ALLOW_UNOBFUSCATED:-0}" != "1" ]; then
    die "缺 r8.jar 且未放行未混淆降级（装 cmdline-tools 或设 MXCZ999_ALLOW_UNOBFUSCATED=1）"
  fi
  echo "[5/6] R8 不可用 → d8 --release（未混淆，已显式放行）"
  DRC=0
  ( cd "$STAGE_ROOT" && "$D8" --release --min-api "$MIN_API" --output "$(winpath "$STAGE_ROOT/out")" \
      "$(winpath "$STAGE_ROOT/classes.jar")" ) > "$STAGE_ROOT/d8.log" 2>&1 || DRC=$?
  if [ "$DRC" -ne 0 ]; then
    echo "--- d8 失败（尾部 40 行）---" >&2
    tail -40 "$STAGE_ROOT/d8.log" >&2
    die "d8 rc=$DRC"
  fi
  [ -f "$STAGE_ROOT/out/classes.dex" ] || die "d8 未产出 classes.dex"
  OBF_MODE="disabled(reason: r8.jar missing)"
fi

# ---- 6. 发布 + 字节自检 ----
mkdir -p "$OUT_DIR"
cp -f "$STAGE_ROOT/out/classes.dex" "$DEX_OUT"
echo "[6/6] 发布 → $DEX_OUT"

python3 - "$DEX_OUT" "$DEX_NAME" <<'PY' || die "dex 字节自检失败"
import hashlib, sys
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [
    b'com/starry/magic/StarryInitProvider',
    b'com/starry/magic/menu/MenuService',
    b'com/starry/magic/bridge/ModBridge',
    b'com/starry/magic/sign/StarryAppComponentFactory',
]
must_not = [
    b'com/starry/magic/games/',                  # SPI 目录已排除（本游戏不需要 GameHookImpl）
    b'com/HaloGames',
    b'com/unity3d/player',
    b'Lcom/starry/magic/sign/SignSpoof;',      # 已删除类，禁止复活
]
miss = [n.decode() for n in need if n not in b]
bad  = [n.decode() for n in must_not if n in b]
print('file      : %s' % name)
print('size      : %d B' % len(b))
print('md5       : %s' % hashlib.md5(b).hexdigest())
print('sha256    : %s' % hashlib.sha256(b).hexdigest())
print('magic     : %s' % b[:8].hex())
for n in need:
    print('  %-44s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
for n in must_not:
    print('  %-44s %s' % (n.decode(), 'ABSENT' if n not in b else 'PRESENT!'))
import re
m = re.search(rb'starry-menu:[0-9a-f]{7,40}', b)
print('  stamp     : %s' % (m.group(0).decode() if m else 'NONE'))
if miss: print('FAIL missing: %s' % ', '.join(miss)); sys.exit(1)
if bad:  print('FAIL forbidden: %s' % ', '.join(bad)); sys.exit(1)
print('OK')
PY

# 类总数（dexdump 头 class_defs_size；取不到就写 unknown）
DEXDUMP="$BUILD_TOOLS/dexdump.exe"
[ -f "$DEXDUMP" ] || DEXDUMP="$BUILD_TOOLS/dexdump"
CLS_COUNT="unknown"
if [ -f "$DEXDUMP" ]; then
  CLS_COUNT="$("$DEXDUMP" -f "$DEX_OUT" 2>/dev/null | grep -i 'class_defs_size' | head -1 | awk -F: '{print $2}' | tr -d ' ' || true)"
  [ -n "$CLS_COUNT" ] || CLS_COUNT="unknown"
fi
echo "[build_dex] class_defs = ${CLS_COUNT}；obfuscation = $OBF_MODE"

{
  echo "game         : $GAME ($PKG $VERSION vc$VC)"
  echo "tag          : $TAG"
  echo "dex          : $DEX_NAME"
  echo "built_at     : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "source_commit: $SRC_COMMIT"
  echo "source_commit12: $SRC_COMMIT12"
  echo "source_tree_sha256: $SRC_TREE_SHA"
  echo "src_dir      : mod菜单/starry-mod-menu/app/src/main/java"
  echo "build_cmd    : bash 04_scripts/999秒幸存者/mxcz999_build_dex_win.sh"
  echo "toolchain    : javac $(basename "$(dirname "$JAVAC")")/$(java -version 2>&1 | head -1 | sed 's/.*"\\(.*\\)".*/\\1/') + R8(obfuscate_menu_dex.sh) + $(basename "$AJAR")"
  echo "dex_md5      : $(md5f "$DEX_OUT")"
  echo "dex_sha256   : $(sha256f "$DEX_OUT")"
  echo "dex_size     : $(stat -c %s "$DEX_OUT" 2>/dev/null || wc -c < "$DEX_OUT") B"
  echo "obfuscation  : $OBF_MODE"
  echo "class_count  : ${CLS_COUNT}"
} > "$INFO_OUT"
echo "[build_dex] done → $DEX_OUT"
echo "[build_dex] md5 = $(md5f "$DEX_OUT")"
echo "[build_dex] info = $INFO_OUT"
