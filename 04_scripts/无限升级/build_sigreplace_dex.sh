#!/usr/bin/env bash
# 无限升级 11.266018 —— 生成「运行时签名替换」菜单 dex（2026-11-04）
#
# 做什么：
#   1) javac 编译 com/starry/magic/sign/SigReplace.java（用本目录 java/android 下的桩类做 classpath）
#   2) d8 → classes.dex → baksmali → 合并进现用菜单 dex 的 smali 树
#   3) 在 StarryInitProvider.onCreate 最前面插 invoke-static SigReplace.install(ctx)
#   4) smali assemble → <out>/classes4.dex（供 pack_wxsj_vc1637_apk.py --dex 替换）
#
# 用法：bash 04_scripts/无限升级/build_sigreplace_dex.sh [底包apk] [输出dex]
#   默认底包 = .local/wxsj_vc1637_base_20261104/base.apk（含 classes4.dex）
#   默认输出 = .local/sigwork/classes4_sig.dex
set -euo pipefail
NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
SRC="$NIXIANG/04_scripts/无限升级/sigreplace/java"
APK="${1:-$NIXIANG/.local/wxsj_vc1637_base_20261104/base.apk}"
OUT="${2:-$NIXIANG/.local/sigwork/classes4_sig.dex}"
WORK="$NIXIANG/.local/sigwork"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
D8JAR="$NIXIANG/01_tools/android-sdk/build-tools/android-15/lib/d8.jar"
JAVA_HOME="${JAVA_HOME:-C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"
JAVAC="$JAVA_HOME/bin/javac.exe"
JAVA="$JAVA_HOME/bin/java.exe"

[[ -f "$APK" ]] || { echo "ERROR: 底包缺失 $APK" >&2; exit 1; }
mkdir -p "$WORK"

echo "[1/5] javac"
rm -rf "$WORK/classes" "$WORK/dex" "$WORK/smali_new" "$WORK/smali_base"
mkdir -p "$WORK/classes" "$WORK/dex"
STUBS=$(find "$SRC/android" -name '*.java')
"$JAVAC" -nowarn -encoding UTF-8 --release 11 -d "$WORK/classes" $STUBS "$SRC/com/starry/magic/sign/SigReplace.java"

echo "[2/5] d8（直接调 d8.jar，避开 cmd 引号/路径问题）"
mkdir -p "$WORK/dex"
# bash 3.2 无 mapfile ⇒ 用 find|xargs（本工程只有 2 个 class，不会被拆成多次调用）
find "$WORK/classes/com" -name '*.class' -print0 \
  | xargs -0 "$JAVA" -cp "$D8JAR" com.android.tools.r8.D8 --min-api 24 --output "$WORK/dex" >/dev/null
[[ -f "$WORK/dex/classes.dex" ]] || { echo "ERROR: d8 未产出"; exit 1; }

echo "[3/5] 取底包 classes4.dex + 双向 baksmali"
python - "$APK" "$WORK/base4.dex" <<'PY'
import sys, zipfile
zipfile.ZipFile(sys.argv[1]).read('classes4.dex') and open(sys.argv[2],'wb').write(zipfile.ZipFile(sys.argv[1]).read('classes4.dex'))
PY
"$BAKSMALI" d -o "$WORK/smali_base" "$WORK/base4.dex"
"$BAKSMALI" d -o "$WORK/smali_new" "$WORK/dex/classes.dex"
cp -f "$WORK/smali_new/com/starry/magic/sign/SigReplace"*.smali "$WORK/smali_base/com/starry/magic/sign/"

echo "[4/5] 插桩 StarryInitProvider.onCreate"
python - "$WORK/smali_base/com/starry/magic/StarryInitProvider.smali" <<'PY'
import sys, io
p = sys.argv[1]
t = io.open(p, encoding='utf-8').read()
if 'SigReplace' in t:
    print('  已有插桩，跳过'); raise SystemExit(0)
lines = t.split('\n')
mi = next(i for i, l in enumerate(lines) if l.startswith('.method public onCreate()Z'))
ri = next(i for i in range(mi, len(lines)) if lines[i].strip().startswith('.registers'))
ins = [
    '',
    '    # 2026-11-04 签名轨：运行时把 PackageInfo 签名替换为官方证书（TapSDK 验签用）',
    '    invoke-virtual {p0}, Landroid/content/ContentProvider;->getContext()Landroid/content/Context;',
    '    move-result-object v0',
    '    invoke-static {v0}, Lcom/starry/magic/sign/SigReplace;->install(Landroid/content/Context;)V',
]
lines[ri+1:ri+1] = ins
io.open(p, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
print('  插桩完成')
PY

echo "[5/5] assemble"
mkdir -p "$(dirname "$OUT")"
"$SMALI" assemble -o "$OUT" "$WORK/smali_base"
SIZE=$(stat -c%s "$OUT" 2>/dev/null || stat -f%z "$OUT")
LC_ALL=C grep -aq 'SigReplace' "$OUT" || { echo "ERROR: 产物缺 SigReplace"; exit 1; }
LC_ALL=C grep -aq 'StarryInitProvider' "$OUT" || { echo "ERROR: 产物缺 StarryInitProvider"; exit 1; }
echo "  -> $OUT  $SIZE B"
