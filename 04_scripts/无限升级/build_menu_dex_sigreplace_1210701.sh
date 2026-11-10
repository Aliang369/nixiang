#!/usr/bin/env bash
# ============================================================
#  无限升级 12.x —— 菜单模板「签名轨插桩」派生（2026-11-10）
#
#  把基准菜单模板的 classes2.dex 加工成「带运行时签名替换」的版本：
#    ① javac 编 sigreplace/java 下的 SigReplace.java（含 android/* 桩类）
#    ② d8 → dex → baksmali，只取 com/starry/magic/sign/SigReplace*.smali（桩类丢弃）
#    ③ 并入模板 smali 树，在 StarryInitProvider.onCreate 最前面插 install()（幂等）
#    ④ smali assemble → <tpl-out>/classes2.dex，并带 build_info.txt / libc++_shared.so / starlight.crt
#
#  为什么：2026-10-28 SignSpoof 整体删除后 TapSDK 验签改走「运行时签名替换」；
#  菜单 dex 由仓库模板直接拷进包（不重编），所以插桩只能在模板层做一次。
#  基准模板保持干净，插桩版单独目录 ⇒ 其它出包不会被动带上签名替换。
#
#  用法：
#    bash 04_scripts/无限升级/build_menu_dex_sigreplace_1210701.sh
#    WXSJ_TPL_IN=... WXSJ_TPL_OUT=... WXSJ_DEX_NAME=classes2.dex bash ...
#
#  出包：
#    WXSJ_MENU_TPL="$PWD/mod菜单/starry-mod-menu/templates/wxsj_menu_current_sig" \
#    WXSJ_PHASE=release bash mod菜单/starry-mod-menu/scripts/build_wxsj_12.10701.sh
# ============================================================
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
APP="$NIXIANG/mod菜单/starry-mod-menu"
TPL_IN="${WXSJ_TPL_IN:-$APP/templates/wxsj_menu_current}"
TPL_OUT="${WXSJ_TPL_OUT:-$APP/templates/wxsj_menu_current_sig}"
DEX_NAME="${WXSJ_DEX_NAME:-classes2.dex}"
SRC="$NIXIANG/04_scripts/无限升级/sigreplace/java"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"

[[ -s "$TPL_IN/$DEX_NAME" ]] || { echo "ERROR: 缺基准模板 dex $TPL_IN/${DEX_NAME}（先跑 scripts/wxsj_refresh_menu_tpl.sh）" >&2; exit 1; }

if [[ -n "${JAVA_HOME:-}" && -x "$JAVA_HOME/bin/javac" ]]; then
  JAVAC="$JAVA_HOME/bin/javac"; JAVA="$JAVA_HOME/bin/java"
elif [[ -x /usr/libexec/java_home ]]; then
  JH="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
  [[ -n "$JH" ]] || { echo "ERROR: 找不到 JDK 17" >&2; exit 1; }
  JAVAC="$JH/bin/javac"; JAVA="$JH/bin/java"
else
  JAVAC="$(command -v javac || true)"; JAVA="$(command -v java || true)"
fi
[[ -n "$JAVAC" && -x "$JAVAC" ]] || { echo "ERROR: 缺 javac" >&2; exit 1; }

D8JAR=""
for v in 36.0.0 35.0.0 34.0.0; do
  for base in "$HOME/Library/Android/sdk/build-tools" "$HOME/AppData/Local/Android/Sdk/build-tools"; do
    if [[ -f "$base/$v/lib/d8.jar" ]]; then D8JAR="$base/$v/lib/d8.jar"; break 2; fi
  done
done
[[ -n "$D8JAR" ]] || { echo "ERROR: 找不到 d8.jar" >&2; exit 1; }

WORK="$(mktemp -d /tmp/wxsj_sigtpl.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/5] javac SigReplace + 桩类"
mkdir -p "$WORK/classes" "$WORK/dex"
"$JAVAC" -nowarn -encoding UTF-8 --release 11 -d "$WORK/classes" $(find "$SRC/android" -name '*.java') "$SRC/com/starry/magic/sign/SigReplace.java"

echo "[2/5] d8 -> classes.dex"
"$JAVA" -cp "$D8JAR" com.android.tools.r8.D8 --min-api 24 --output "$WORK/dex" $(find "$WORK/classes" -name '*.class') >/dev/null
[[ -f "$WORK/dex/classes.dex" ]] || { echo "ERROR: d8 未产出" >&2; exit 1; }

echo "[3/5] 并入模板 smali 树"
"$BAKSMALI" d -o "$WORK/smali" "$TPL_IN/$DEX_NAME"
"$BAKSMALI" d -o "$WORK/smali_new" "$WORK/dex/classes.dex"
[[ -d "$WORK/smali_new/com/starry/magic/sign" ]] || { echo "ERROR: 新 dex 无 com/starry/magic/sign" >&2; exit 1; }
mkdir -p "$WORK/smali/com/starry/magic/sign"
cp -f "$WORK"/smali_new/com/starry/magic/sign/SigReplace*.smali "$WORK/smali/com/starry/magic/sign/"

echo "[4/5] 插桩 StarryInitProvider.onCreate（幂等）"
python3 - "$WORK/smali/com/starry/magic/StarryInitProvider.smali" <<'PY'
import io, sys
p = sys.argv[1]
t = io.open(p, encoding='utf-8').read()
if 'SigReplace' in t:
    print('  已有插桩，跳过'); raise SystemExit(0)
lines = t.split(chr(10))
mi = next(i for i, l in enumerate(lines) if l.startswith('.method public onCreate()Z'))
ri = next(i for i in range(mi, len(lines)) if lines[i].strip().startswith('.registers'))
ins = [
    '',
    '    # 2026-11-10 签名轨：运行时把 PackageInfo 签名替换为官方证书（TapSDK 验签用）',
    '    invoke-virtual {p0}, Landroid/content/ContentProvider;->getContext()Landroid/content/Context;',
    '    move-result-object v0',
    '    invoke-static {v0}, Lcom/starry/magic/sign/SigReplace;->install(Landroid/content/Context;)V',
]
lines[ri+1:ri+1] = ins
io.open(p, 'w', encoding='utf-8', newline=chr(10)).write(chr(10).join(lines))
print('  插桩完成 @line', ri + 1)
PY

echo "[5/5] assemble -> $TPL_OUT/$DEX_NAME"
mkdir -p "$TPL_OUT"
"$SMALI" assemble -o "$TPL_OUT/$DEX_NAME" "$WORK/smali"
cp -f "$TPL_IN/build_info.txt" "$TPL_OUT/" 2>/dev/null || true
cp -f "$TPL_IN/libc++_shared.so" "$TPL_OUT/" 2>/dev/null || true
cp -f "$TPL_IN/starlight.crt" "$TPL_OUT/" 2>/dev/null || true

LC_ALL=C grep -aq 'SigReplace' "$TPL_OUT/$DEX_NAME" || { echo "ERROR: 产物缺 SigReplace" >&2; exit 1; }
LC_ALL=C grep -aq 'nativeSetStaticField' "$TPL_OUT/$DEX_NAME" || { echo "ERROR: 产物缺 nativeSetStaticField" >&2; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$TPL_OUT/$DEX_NAME" || { echo "ERROR: 产物缺 StarryInitProvider" >&2; exit 1; }
LC_ALL=C grep -aoE 'starry-menu:[0-9a-f]+' "$TPL_OUT/$DEX_NAME" | head -1 || true
ls -l "$TPL_OUT"
echo done
