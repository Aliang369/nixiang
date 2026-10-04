#!/usr/bin/env bash
# 雷霆战机混合包最小验证 — 新官方 1.25.31 业务 + 斌哥旧 ACE 过检体系
#
# 目的：验证「新官方 dex/cocos + 斌哥旧 tersafe2/tprt + Killer（无 libsdk）」能否冷启不崩
# 不加菜单（最小验证），只验证 ACE 过检 + JNI 兼容
#
# 产物：08_output/雷霆战机/雷霆战机_1.25.31_hybrid-no-sdk-releaseSigned.apk
#
# 用法:
#   bash scripts/ltzj_hybrid_ace_test.sh
#   STARRY_INSTALL=1 bash scripts/ltzj_hybrid_ace_test.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
NEW_APK="$NIXIANG/02_apks/雷霆战机/雷霆战机_原版.apk"
BINGE_APK="$NIXIANG/02_apks/雷霆战机/雷霆战机_集结_999999_mod.apk"
OUT="$NIXIANG/08_output/雷霆战机"
SIGNED="$OUT/雷霆战机_1.25.31_hybrid-no-sdk-releaseSigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.a5game.ltzj.jj"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$NEW_APK" ]] || { echo "ERROR: missing $NEW_APK"; exit 1; }
[[ -f "$BINGE_APK" ]] || { echo "ERROR: missing $BINGE_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }

WORK="$(mktemp -d /tmp/ltzj_hybrid.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT"

echo "[1/6] 提取斌哥旧 ACE 组件"
mkdir -p "$WORK/binge_lib"
unzip -q -o "$BINGE_APK" lib/arm64-v8a/libtersafe2.so lib/arm64-v8a/libtprt.so -d "$WORK/binge_lib" 2>/dev/null
unzip -q -o "$BINGE_APK" classes6.dex -d "$WORK/binge_lib" 2>/dev/null
echo "  tersafe2=$(md5 -q "$WORK/binge_lib/lib/arm64-v8a/libtersafe2.so" 2>/dev/null | cut -c1-8)"
echo "  tprt=$(md5 -q "$WORK/binge_lib/lib/arm64-v8a/libtprt.so" 2>/dev/null | cut -c1-8)"

echo "[2/6] baksmali 新官方 classes2 → 改 AceApplication"
unzip -q -o "$NEW_APK" classes2.dex -d "$WORK" 2>/dev/null
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2.dex" >/dev/null 2>&1

ACE_SMALI=$(find "$WORK/c2smali" -path "*/ace/gshell/AceApplication.smali")
[[ -f "$ACE_SMALI" ]] || { echo "ERROR: AceApplication.smali not found"; exit 1; }

# 改动 1: .super 从 Application 改成 KillerApplication193
python3 - "$ACE_SMALI" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
t = t.replace(".super Landroid/app/Application;", ".super Lbin/mt/signature/KillerApplication193;")
# 改动 2: clinit 不加 loadLibrary("sdk")（去掉 libsdk，只保留 super→Killer）
old = 'invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n    .line 30'
new = 'invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n    .line 30'
if old in t:
    t = t.replace(old, new, 1)
    print("  patch: super → KillerApplication193 (无 libsdk)")
else:
    # 兜底：在 tprt loadLibrary 后直接插
    marker = 'const-string v0, "tprt"\n\n    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V'
    if marker in t:
        t = t.replace(marker, marker, 1)
        print("  patch (fallback): no sdk loaded")
    else:
        raise SystemExit("ERROR: could not find loadLibrary insertion point")
if "KillerApplication193" not in t:
    raise SystemExit("ERROR: super patch failed")
p.write_text(t, encoding="utf-8")
PY

echo "[3/6] smali → classes2.dex"
"$SMALI_BIN" assemble -o "$WORK/classes2_patched.dex" "$WORK/c2smali" 2>/dev/null
ls -la "$WORK/classes2_patched.dex"

echo "[4/6] 外科组装：新官方底 + REPLACE tersafe2/tprt + REPLACE classes2 + ADD classes6"
cp "$NEW_APK" "$WORK/unsigned.apk"
(
  cd "$WORK"
  # REPLACE so
  cp binge_lib/lib/arm64-v8a/libtersafe2.so lib/arm64-v8a_tersafe2.so 2>/dev/null || true
  # 用 zip -u 替换
  mkdir -p tmp_lib/arm64-v8a
  cp binge_lib/lib/arm64-v8a/libtersafe2.so tmp_lib/arm64-v8a/libtersafe2.so
  cp binge_lib/lib/arm64-v8a/libtprt.so tmp_lib/arm64-v8a/libtprt.so
  zip -u unsigned.apk lib/arm64-v8a/libtersafe2.so < binge_lib/lib/arm64-v8a/libtersafe2.so 2>/dev/null || \
    zip unsigned.apk lib/arm64-v8a/libtersafe2.so 2>/dev/null || true
)
# 重新打包（用 python 做精确外科替换）
python3 - "$NEW_APK" "$WORK" "$OUT" <<'PY'
import sys, zipfile, shutil
from pathlib import Path

new_apk = Path(sys.argv[1])
work = Path(sys.argv[2])
out_dir = Path(sys.argv[3])

binge_lib = work / "binge_lib"
unsigned = work / "unsigned.apk"

# 要替换/新增的文件
replacements = {
    "lib/arm64-v8a/libtersafe2.so": binge_lib / "lib/arm64-v8a/libtersafe2.so",
    "lib/arm64-v8a/libtprt.so": binge_lib / "lib/arm64-v8a/libtprt.so",
}
additions = {
        "classes6.dex": binge_lib / "classes6.dex",
}
dex_replace = {
    "classes2.dex": work / "classes2_patched.dex",
}

tmp = work / "hybrid.apk"
zin = zipfile.ZipFile(new_apk, "r")
zout = zipfile.ZipFile(tmp, "w")

skip = set(replacements) | set(additions) | set(dex_replace) | {"META-INF/CERT.RSA", "META-INF/CERT.SF", "META-INF/CERT.DSA", "META-INF/MANIFEST.MF", "META-INF/CERT.SF"}

for item in zin.infolist():
    name = item.filename
    if name in skip:
        continue
    data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)

# 写替换的 dex
for name, src in dex_replace.items():
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, src.read_bytes())

# 写替换的 so
for name, src in replacements.items():
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, src.read_bytes())

# 写新增的文件
for name, src in additions.items():
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, src.read_bytes())

zin.close()
zout.close()
shutil.move(str(tmp), str(unsigned))
print(f"  hybrid apk: {unsigned.stat().st_size} bytes")
PY

echo "[5/6] zipalign + 签名"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
else
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  cp -f "$(ls -t "$OUT"/*hybrid*Signed*.apk 2>/dev/null | head -1)" "$SIGNED" 2>/dev/null || true
fi
ls -lh "$SIGNED"

echo "[6/6] 验证包内组件"
python3 - "$SIGNED" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
for name in ["lib/arm64-v8a/libtersafe2.so", "lib/arm64-v8a/libtprt.so", "classes2.dex", "classes6.dex"]:
    if name in z.namelist():
        md5 = hashlib.md5(z.read(name)).hexdigest()[:8]
        print(f"  ✓ {name} (md5={md5})")
    else:
        print(f"  ✗ {name} MISSING")
PY

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] 卸载旧版 + 装混合包"
  adb shell pm uninstall "$PKG" 2>/dev/null || true
  REMOTE="/data/local/tmp/ltzj_hybrid.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell "su -c 'pm install -r -d --user 0 $REMOTE'" 2>/dev/null || adb install -r "$SIGNED"
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V AceApplication:V AndroidRuntime:E DEBUG:F"
fi

echo "DONE: $SIGNED"
echo "验证：冷启 → 看能不能进游戏（不崩 = 旧 tersafe/tprt 兼容（OOM 根源是 libsdk））"
