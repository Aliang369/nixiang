#!/usr/bin/env bash
# 冒险与征途 B站渠道（mxyztbili / com.my.mxyzt.bilibili 2.0.1）星空菜单 Java 桥 dex 构建
# 派生自 04_scripts/冒险与征途/mxyzt_build_dex.sh（1.5.9 / TapTap），差异三处：
#   ① 产物目录 templates/mxyztbili_menu_current，dex 名 classes4.dex
#      （底包 classes.dex/2/3 已占 ⇒ 最小空闲下标 = 4，本轮 unzip -l 现查）
#   ② BuildConfig 版本串 = 2.0.1 / 201
#   ③ 新增 [5/6] StarryApp 父类外科补丁：Landroid/app/Application; → Lcom/hdwj/afk/MainApplication;
#      依据：Task E 要求 Manifest application@name 指向 com.starry.magic.StarryApp，
#      而 StarryApp 的父类必须是官方 Application（否则官方初始化链断，游戏黑屏）。
#      官包 2.0.1 的 application@name = com.hdwj.afk.MainApplication
#      （super = com.hnyl.core.YLGameApplication，dexdump 实测）。
#      手法同 04_scripts/无悔华夏/whrhx_build_latest_dex.sh（baksmali → 改 smali → smali assemble）。
#   其余（javac/R8/字节自检/menu_tpl_gate）与 1.5.9 完全同口径。
#
# 产物：mod菜单/starry-mod-menu/templates/mxyztbili_menu_current/classes4.dex
# 用法：bash 04_scripts/冒险与征途/mxyztbili_build_dex.sh
set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../_common/env.sh
. "$HERE/../_common/env.sh"

STAR="$NIXIANG_ROOT/mod菜单/starry-mod-menu"
SRC="$STAR/app/src/main/java"
TPL_DIR="$STAR/templates/mxyztbili_menu_current"
DEX_NAME="classes4.dex"
SUPER_CLASS="Lcom/hdwj/afk/MainApplication;"
SCRATCH="$NIXIANG_ROOT/13_scratch/mxyztbili_build"
LOG="$SCRATCH/dex_build.log"

die() { echo "ERROR: $*" >&2; exit 1; }

# --- 1. 工具解析 -------------------------------------------------------------
[ -n "${NIXIANG_JAVA17:-}" ] || die "NIXIANG_JAVA17 未解析（env.sh）"
JAVAC="$NIXIANG_JAVA17/bin/javac"
[ -f "$JAVAC" ] || die "javac 不存在: $JAVAC"
JAR_BIN="$NIXIANG_JAVA17/bin/jar"
[ -f "$JAR_BIN" ] || die "jar 不存在: $JAR_BIN"
export JAVA_HOME="$NIXIANG_JAVA17"
export PATH="$NIXIANG_JAVA17/bin:$PATH"

SMALI="$NIXIANG_ROOT/01_tools/smali/smali"
BAKSMALI="$NIXIANG_ROOT/01_tools/smali/baksmali"
[ -x "$SMALI" ] || die "缺 baksmali/smali: $SMALI"
[ -x "$BAKSMALI" ] || die "缺 baksmali/smali: $BAKSMALI"

SDK_ROOT="$HOME/Library/Android/sdk"
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
  STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/mxyztbili_build/dex"
  w() { cygpath -m "$1"; }
else
  STAGE_ROOT="${TMPDIR:-/tmp}/mxyztbili_build/dex"
  w() { printf '%s' "$1"; }
fi
rm -rf "$STAGE_ROOT"
mkdir -p "$STAGE_ROOT/src" "$STAGE_ROOT/gen/com/starry/magic" "$STAGE_ROOT/classes" "$STAGE_ROOT/out"

echo "[1/6] 源拷贝 → $STAGE_ROOT/src（ASCII 路径）"
cp -a "$SRC/." "$STAGE_ROOT/src/" || die "源拷贝失败"

# --- 3. R / BuildConfig 桩（本机无 aapt2 资源编译、无 gradle）------------------
R_GEN="$STAGE_ROOT/gen/com/starry/magic/R.java"
{
  echo 'package com.starry.magic;'
  echo '/** 构建期 R 桩（自动枚举，勿手改）。 */'
  echo 'public final class R {'
  echo '    private R() {}'
  for kind in layout id drawable string color attr dimen style; do
    names="$(grep -rhoE "R\.${kind}\.[A-Za-z0-9_]+" "$SRC" 2>/dev/null | sed "s|^R\.${kind}\.||" | sort -u)"
    [ -n "$names" ] || continue
    echo "    public static final class ${kind} {"
    echo "        private ${kind}() {}"
    i=1
    while IFS= read -r n; do
      [ -n "$n" ] || continue
      printf '        public static final int %s = 0x7f0%01x%04x;\n' "$n" "$(printf '%d' "$(echo "${kind}" | cksum | cut -d' ' -f1)" | cut -c1)" "$i"
      i=$((i + 1))
    done <<< "$names"
    echo '    }'
  done
  echo '}'
} > "$R_GEN"
echo "[1b/6] R 桩自动生成：$(grep -c 'public static final int' "$R_GEN") 个符号"

MXYZT_MENU_VER_NAME="2.0.1"
MXYZT_MENU_VER_CODE="201"
cat > "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java" <<EOF
package com.starry.magic;
/** 构建期 BuildConfig 桩（本机无 gradle）。 */
public final class BuildConfig {
    private BuildConfig() {}
    public static final boolean DEBUG = false;
    public static final String  VERSION_NAME = "${MXYZT_MENU_VER_NAME}";
    public static final int     VERSION_CODE = ${MXYZT_MENU_VER_CODE};
}
EOF

echo "[2/6] javac 编译"
(find "$STAGE_ROOT/src" -name '*.java'; printf '%s\n' "$R_GEN" "$STAGE_ROOT/gen/com/starry/magic/BuildConfig.java") > "$STAGE_ROOT/srcs.txt"
: > "$STAGE_ROOT/argv.txt"
while IFS= read -r f; do printf '\"%s\"\n' "$(w "$f")" >> "$STAGE_ROOT/argv.txt"; done < "$STAGE_ROOT/srcs.txt"

# 注意：**不能用 -bootclasspath android.jar**（android.jar 的 LambdaMetafactory 是阉割版）。
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

echo "[3/6] 打包 class → jar"
"$JAR_BIN" cf "$(w "$STAGE_ROOT/classes.jar")" -C "$(w "$STAGE_ROOT/classes")" . \
  > "$STAGE_ROOT/jar.log" 2>&1 || { tail -20 "$STAGE_ROOT/jar.log" >&2; die "jar 失败"; }

echo "[4/6] R8 混淆 → dex（min-api 21；_common/obfuscate_menu_dex.sh）"
OBF="$NIXIANG_ROOT/04_scripts/_common/obfuscate_menu_dex.sh"
[ -f "$OBF" ] || die "缺混淆脚本: $OBF"
if ! bash "$OBF" "$(w "$STAGE_ROOT/classes.jar")" "$(w "$STAGE_ROOT/out/r8.dex")" \
      > "$STAGE_ROOT/r8.log" 2>&1; then
  echo "--- R8 失败（尾部 40 行）---" >&2
  tail -40 "$STAGE_ROOT/r8.log" >&2
  die "R8 混淆失败"
fi
[ -f "$STAGE_ROOT/out/r8.dex" ] || die "R8 未产出 dex"
echo "      R8 产出 $(wc -c <"$STAGE_ROOT/out/r8.dex" | tr -d ' ') B"

echo "[5/6] StarryApp 父类外科补丁 → $SUPER_CLASS"
rm -rf "$STAGE_ROOT/smali"
"$BAKSMALI" d -o "$(w "$STAGE_ROOT/smali")" "$(w "$STAGE_ROOT/out/r8.dex")" > "$STAGE_ROOT/baksmali.log" 2>&1 \
  || { tail -20 "$STAGE_ROOT/baksmali.log" >&2; die "baksmali 失败"; }
APP_SMALI="$STAGE_ROOT/smali/com/starry/magic/StarryApp.smali"
[ -f "$APP_SMALI" ] || die "StarryApp.smali 不存在（R8 规则是否保留了 StarryApp？）"
SUPER_CLASS="$SUPER_CLASS" "${NIXIANG_PYTHON:-python3}" - "$APP_SMALI" <<'PY' || die "父类补丁失败"
from pathlib import Path
import os, sys
p = Path(sys.argv[1]); t = p.read_text(encoding="utf-8")
S = os.environ["SUPER_CLASS"]
repls = [
    (".super Landroid/app/Application;", ".super " + S),
    ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
     "invoke-direct {p0}, " + S + "-><init>()V"),
    ("invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
     "invoke-super {p0, p1}, " + S + "->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Landroid/app/Application;->onCreate()V",
     "invoke-super {p0}, " + S + "->onCreate()V"),
]
hits = []
for a, b in repls:
    if a in t:
        t = t.replace(a, b); hits.append(a.split("->")[-1].split("(")[0])
p.write_text(t, encoding="utf-8")
print("      StarryApp 替换命中: %s" % (hits or "NONE"))
if ".super " + S not in t:
    sys.exit("ERROR: .super 未被替换成 " + S)
PY
"$SMALI" assemble -o "$(w "$STAGE_ROOT/out/classes.dex")" "$(w "$STAGE_ROOT/smali")" > "$STAGE_ROOT/smali.log" 2>&1 \
  || { tail -30 "$STAGE_ROOT/smali.log" >&2; die "smali assemble 失败"; }
echo "      smali 重组后 $(wc -c <"$STAGE_ROOT/out/classes.dex" | tr -d ' ') B"

echo "[6/6] 字节自检 + 发布"
mkdir -p "$TPL_DIR"
cp -f "$STAGE_ROOT/out/classes.dex" "$TPL_DIR/$DEX_NAME"

SUPER_CLASS="$SUPER_CLASS" "${NIXIANG_PYTHON:-python3}" - "$TPL_DIR/$DEX_NAME" "$DEX_NAME" <<'PY' || die "dex 字节自检失败"
import hashlib, os, sys, re
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [
    b'com/starry/magic/StarryInitProvider',
    b'com/starry/magic/menu/MenuService',
    b'com/starry/magic/bridge/ModBridge',
    b'com/starry/magic/ota/StarryNativeLoad',
    b'com/starry/magic/sign/SignatureKillerBootstrap',
    os.environ["SUPER_CLASS"].encode(),
]
FORBID = [b'Lcom/starry/magic/sign/SignSpoof;']
miss = [n.decode() for n in need if n not in b]
bad = [n.decode() for n in FORBID if n in b]
print('  file      : %s' % name)
print('  size      : %d B' % len(b))
print('  sha256    : %s' % hashlib.sha256(b).hexdigest())
print('  md5       : %s' % hashlib.md5(b).hexdigest())
print('  magic     : %s' % b[:8].hex())
for n in need:
    print('    %-44s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
if miss:
    print('  FAIL missing: %s' % ', '.join(miss)); sys.exit(1)
if bad:
    print('  FAIL forbidden present: %s' % ', '.join(bad)); sys.exit(1)
print('  OK')
PY

mkdir -p "$SCRATCH" "$(dirname "$LOG")"
{
  echo "game      : 冒险与征途 B站渠道 (mxyztbili)"
  echo "package   : com.my.mxyzt.bilibili  2.0.1 / vc201"
  echo "dex       : $DEX_NAME"
  echo "super     : $SUPER_CLASS"
  echo "built_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac     : $JAVAC"
  echo "androidjar: $AJAR"
  echo "source    : $SRC"
  echo "sha256    : $(shasum -a 256 "$TPL_DIR/$DEX_NAME" | awk '{print $1}')"
  echo "source_commit: $(git -C "$NIXIANG_ROOT" log -1 --format=%H -- mod菜单/starry-mod-menu/app/src/main/java 2>/dev/null)"
} > "$TPL_DIR/build_info.txt"
mkdir -p "$(dirname "$LOG")"
{ echo "=== $(date '+%Y-%m-%d %H:%M:%S') ==="; cat "$TPL_DIR/build_info.txt"; } >> "$LOG"

echo "OK → $TPL_DIR/$DEX_NAME"

bash "$NIXIANG_ROOT/04_scripts/_common/menu_tpl_gate.sh" --tpl "$TPL_DIR" --dex-name "$DEX_NAME" || {
  echo "ERROR: 模板新鲜度门禁未过（build_info 与源码不符，先重跑本脚本）" >&2; exit 1;
}
