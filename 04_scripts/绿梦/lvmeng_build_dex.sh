#!/usr/bin/env bash
# ================================================================
#  lvmeng_build_dex.sh — 绿梦：时空之声 菜单 dex（classes4.dex）离线构建
#
#  照 2026-10-05 绝区零 zzz_build_dex.sh / 2026-10-30 保卫向日葵 bwxrk_build_dex.sh
#  的「adb-free / gradle-free / smali-free」路线：javac + d8 全离线。
#
#  为什么不用 gradle：本机（Windows）工程路径含中文（mod菜单）⇒ AGP 直接拒编；
#  且 gradlew 缺 wrapper jar。菜单 dex 只需要 java 源 + android.jar + d8。
#
#  用法：
#    bash 04_scripts/绿梦/lvmeng_build_dex.sh [--src <java 源目录>] [--out <dex 路径>] \
#         [--min-api 24] [--check-strings '串1,串2']
#  默认：
#    --src  mod菜单/starry-mod-menu/app/src/main/java
#    --out  07_patched/绿梦/classes4.dex
#
#  ⚠ 源目录可用 --src 指向历史提交的 worktree（例：r3 su 修复那一版），
#    以便「只改一个变量」地复刻某个已装机版本的 dex。
#  退出码：0=成功；1=构建/自检失败；2=用法错。
# ================================================================
set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

SRC="$NIXIANG_ROOT/mod菜单/starry-mod-menu/app/src/main/java"
OUT="$NIXIANG_ROOT/07_patched/绿梦/classes4.dex"
MIN_API=24
CHECKS=""

while [ $# -gt 0 ]; do
  case "$1" in
    --src) SRC="${2:-}"; shift 2 ;;
    --out) OUT="${2:-}"; shift 2 ;;
    --min-api) MIN_API="${2:-}"; shift 2 ;;
    --check-strings) CHECKS="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,24p' "$0"; exit 0 ;;
    *) echo "错误: 未知参数 $1" >&2; exit 2 ;;
  esac
done

die() { echo "ERROR: $*" >&2; exit 1; }

[ -n "${NIXIANG_JAVA17:-}" ]      || die "NIXIANG_JAVA17 未解析（env.sh）"
[ -n "${NIXIANG_BUILD_TOOLS:-}" ] || die "NIXIANG_BUILD_TOOLS 未解析（env.sh）"
JAVAC="$NIXIANG_JAVA17/bin/javac.exe"; [ -f "$JAVAC" ] || JAVAC="$NIXIANG_JAVA17/bin/javac"
[ -f "$JAVAC" ] || die "javac 不存在: $NIXIANG_JAVA17"
JAR_BIN="$NIXIANG_JAVA17/bin/jar.exe"; [ -f "$JAR_BIN" ] || JAR_BIN="$NIXIANG_JAVA17/bin/jar"
[ -f "$JAR_BIN" ] || die "jar 不存在"
D8="$NIXIANG_BUILD_TOOLS/d8.bat"; [ -f "$D8" ] || D8="$NIXIANG_BUILD_TOOLS/d8"
[ -f "$D8" ] || die "d8 不存在: $NIXIANG_BUILD_TOOLS"

SDK_ROOT="$(cd "$NIXIANG_BUILD_TOOLS/../.." && pwd)"
AJAR=""
for cand in "$SDK_ROOT/platforms/android-36/android.jar" "$SDK_ROOT/platforms/android-35/android.jar"; do
  [ -f "$cand" ] && AJAR="$cand" && break
done
if [ -z "$AJAR" ]; then
  AJAR="$(ls -1d "$SDK_ROOT/platforms"/android-*/android.jar 2>/dev/null | sort -V | tail -1)"
fi
[ -n "$AJAR" ] && [ -f "$AJAR" ] || die "android.jar 未找到（$SDK_ROOT/platforms）"
[ -d "$SRC" ] || die "Java 源目录不存在: $SRC"

# ---- ASCII 暂存（中文路径会打死 Windows 侧工具）----
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/lvmeng_dex" ;;
  *) STAGE_ROOT="${TMPDIR:-/tmp}/lvmeng_dex" ;;
esac
w() { if command -v cygpath >/dev/null 2>&1; then cygpath -m "$1"; else printf '%s' "$1"; fi; }

rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"

echo "[1/6] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
echo "      SRC = $SRC"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# ---- R / BuildConfig 桩（本机无 aapt2 资源编译、无 gradle）----
# 取值可任意：这些 id/string 只被游戏内不启动的 MainActivity / 剪贴板对话框引用。
echo "[2/6] 生成 R / BuildConfig 桩"
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
        public static final int ic_menu_info_details   = 0x7f040001;
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
        public static final int background           = 0x7f020001;
        public static final int btn_cdn_inject       = 0x7f020002;
        public static final int btn_diag_copy        = 0x7f020003;
        public static final int btn_diag_refresh     = 0x7f020004;
        public static final int btn_diag_share       = 0x7f020005;
        public static final int btn_request_allfiles = 0x7f020006;
        public static final int btn_request_overlay  = 0x7f020007;
        public static final int cb_cdn_auto_close    = 0x7f020008;
        public static final int copy                 = 0x7f020009;
        public static final int cut                  = 0x7f02000a;
        public static final int paste                = 0x7f02000b;
        public static final int progress             = 0x7f02000c;
        public static final int secondaryProgress    = 0x7f02000d;
        public static final int selectAll            = 0x7f02000e;
        public static final int tv_cdn_status        = 0x7f02000f;
        public static final int tv_diag_detail       = 0x7f020010;
        public static final int tv_diag_headline     = 0x7f020011;
    }
}
EOF

cat > "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java" <<'EOF'
package com.starry.magic;
/** 构建期 BuildConfig 桩：本机无 gradle（Diag 读版本串 / MainActivity 读 DEBUG）。 */
public final class BuildConfig {
    private BuildConfig() {}
    public static final boolean DEBUG = false;
    public static final String  VERSION_NAME = "1.0.5";
    public static final int     VERSION_CODE = 12035;
}
EOF

echo "[3/6] javac 编译（--release 11，android.* 走 -classpath android.jar）"
(find "$STAGE_ROOT/src" -name '*.java'; \
 printf '%s\n' "$STAGE_ROOT/gen/com/starry/magic/R.java" "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java") \
  > "$STAGE_ROOT/srcs.txt"
: > "$STAGE_ROOT/argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(w "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

"$JAVAC" -nowarn -encoding UTF-8 --release 11 \
  -classpath "$(w "$AJAR")" -d "$(w "$STAGE_ROOT/classes")" \
  "@$(w "$STAGE_ROOT/argv.txt")" > "$STAGE_ROOT/javac.log" 2>&1
if [ $? -ne 0 ]; then
  echo "--- javac 失败（尾部 30 行）---" >&2
  tail -30 "$STAGE_ROOT/javac.log" >&2
  die "javac 失败"
fi
echo "      class 文件 $(find "$STAGE_ROOT/classes" -name '*.class' | wc -l | tr -d ' ') 个"

echo "[4/6] jar + d8（min-api ${MIN_API}）"
"$JAR_BIN" cf "$(w "$STAGE_ROOT/classes.jar")" -C "$(w "$STAGE_ROOT/classes")" . \
  || die "jar 失败"
"$D8" --min-api "$MIN_API" --release --lib "$(w "$AJAR")" \
  --output "$(w "$STAGE_ROOT/out")" "$(w "$STAGE_ROOT/classes.jar")" \
  > "$STAGE_ROOT/d8.log" 2>&1 || { tail -20 "$STAGE_ROOT/d8.log" >&2; die "d8 失败"; }
[ -f "$STAGE_ROOT/out/classes.dex" ] || die "d8 未产出 classes.dex"
EXTRA="$(ls "$STAGE_ROOT/out" | grep -c 'classes[0-9]*\.dex' || true)"
[ "$EXTRA" = "1" ] || die "d8 产出多个 dex（$EXTRA 个）—— 需要调 --min-api 或拆 dex"

echo "[5/6] 落盘 → $OUT"
mkdir -p "$(dirname "$OUT")"
cp -f "$STAGE_ROOT/out/classes.dex" "$OUT"

echo "[6/6] 字节自检"
"${NIXIANG_PYTHON:-python}" - "$OUT" "$CHECKS" <<'PY' || die "dex 自检失败"
import hashlib, sys
p, checks = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
if not (b[:4] == b'dex\n'):
    sys.exit('ERROR: 不是 dex')
need = [b'com/starry/magic/StarryInitProvider',
        b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/bridge/ModBridge',
        b'com/starry/magic/StarryApp',
        b'com/starry/magic/lvmeng/LvmengPayloadProvider']
miss = [n.decode() for n in need if n not in b]
if miss:
    sys.exit('ERROR: dex 缺类 %s' % miss)
print('  size   = %d B' % len(b))
print('  md5    = %s' % hashlib.md5(b).hexdigest())
print('  sha256 = %s' % hashlib.sha256(b).hexdigest())
if checks:
    for s in [x for x in checks.split(',') if x.strip()]:
        ok = s.encode('utf-8') in b
        print('  %s 串「%s」' % ('✓' if ok else '✗', s))
        if not ok:
            sys.exit('ERROR: dex 缺串 %s' % s)
PY
echo "DONE: $OUT"
