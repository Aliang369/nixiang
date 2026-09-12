#!/usr/bin/env bash
# 无悔华夏 3.8.280 星空包 - 2026-09-12 资源热更修复配套 dex
#
# 根因：修复版 libstarry_magic.so 来自 2026-09-06 后的星光框架，JNI_OnLoad
# 会注册 nativeGetPinnedCert / nativeBridgeRegistered；旧 classes2.dex 没有
# 这两个声明，RegisterNatives 直接触发 SIGABRT。
#
# 本脚本只在旧 classes2.dex 的 ModBridge 中补两个 native 空声明，
# 不替换菜单实现、不改 Manifest/resources.arsc，也不安装。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX_BASE_APK:-$NIXIANG/02_apks/无悔华夏/无悔华夏_3.8.280_九游星空_20260912.apk}"
SO="${WHRHX_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so}"
OUT_DIR="$NIXIANG/08_output/无悔华夏"
OUT_APK="$OUT_DIR/无悔华夏_3.8.280_资源热更修复_dex配对_20260912-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
SMALI="$NIXIANG/01_tools/smali/smali"
EXPECTED_BASE_SHA="d6e3e8d21e96a0ace74357b3852d81322d705c7d6fe3481bcc37965b57000997"
EXPECTED_SO_SHA="781d5e79ae28c9b1b6ca9a791bd5791f0fd6b702517290d09c33333f7ae71d81"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing mod so: $SO" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS" >&2; exit 1; }
[[ -x "$BAKSMALI" ]] || { echo "ERROR: missing baksmali: $BAKSMALI" >&2; exit 1; }
[[ -x "$SMALI" ]] || { echo "ERROR: missing smali: $SMALI" >&2; exit 1; }

BASE_SHA="$(shasum -a 256 "$BASE_APK" | awk '{print $1}')"
SO_SHA="$(shasum -a 256 "$SO" | awk '{print $1}')"
[[ "$BASE_SHA" == "$EXPECTED_BASE_SHA" ]] || {
  echo "ERROR: unexpected base apk sha256=$BASE_SHA" >&2
  exit 1
}
[[ "$SO_SHA" == "$EXPECTED_SO_SHA" ]] || {
  echo "ERROR: unexpected mod so sha256=$SO_SHA" >&2
  exit 1
}

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_pair.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE_APK" "$WORK/base.apk"
unzip -p "$BASE_APK" classes2.dex > "$WORK/classes2_old.dex"
"$BAKSMALI" list methods "$WORK/classes2_old.dex" | sort > "$WORK/methods_before.txt"
"$BAKSMALI" d -o "$WORK/smali" "$WORK/classes2_old.dex"

MODBRIDGE="$WORK/smali/com/starry/magic/bridge/ModBridge.smali"
[[ -f "$MODBRIDGE" ]] || { echo "ERROR: ModBridge.smali not found" >&2; exit 1; }

python3.11 - "$MODBRIDGE" <<'PY'
import pathlib, sys
path = pathlib.Path(sys.argv[1])
text = path.read_text(encoding="utf-8")
additions = {
    "nativeBridgeRegistered": ".method public static native nativeBridgeRegistered()Z\n.end method\n",
    "nativeGetPinnedCert": ".method public static native nativeGetPinnedCert()[B\n.end method\n",
}
for name, block in additions.items():
    if f".method public static native {name}" in text:
        continue
    anchor = ".method public static native nativeVerifyOtaSig(Ljava/lang/String;Ljava/lang/String;)Z\n.end method\n"
    if anchor not in text:
        raise SystemExit(f"ERROR: anchor for {name} not found")
    text = text.replace(anchor, anchor + "\n" + block, 1)
path.write_text(text, encoding="utf-8")
PY

"$SMALI" a "$WORK/smali" -o "$WORK/classes2_new.dex"
"$BAKSMALI" list methods "$WORK/classes2_new.dex" | sort > "$WORK/methods_after.txt"
comm -23 "$WORK/methods_before.txt" "$WORK/methods_after.txt" > "$WORK/methods_removed.txt"
comm -13 "$WORK/methods_before.txt" "$WORK/methods_after.txt" > "$WORK/methods_added.txt"
[[ ! -s "$WORK/methods_removed.txt" ]] || {
  echo "ERROR: dex methods were removed during smali roundtrip" >&2
  cat "$WORK/methods_removed.txt" >&2
  exit 1
}
cat > "$WORK/methods_expected.txt" <<'EOF'
Lcom/starry/magic/bridge/ModBridge;->nativeBridgeRegistered()Z
Lcom/starry/magic/bridge/ModBridge;->nativeGetPinnedCert()[B
EOF
LC_ALL=C sort -o "$WORK/methods_added.txt" "$WORK/methods_added.txt"
LC_ALL=C sort -o "$WORK/methods_expected.txt" "$WORK/methods_expected.txt"
cmp -s "$WORK/methods_added.txt" "$WORK/methods_expected.txt" || {
  echo "ERROR: unexpected dex method delta" >&2
  diff -u "$WORK/methods_expected.txt" "$WORK/methods_added.txt" >&2 || true
  exit 1
}

mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/classes2_new.dex" "$WORK/classes2.dex"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(
  cd "$WORK"
  zip -q -u base.apk classes2.dex lib/arm64-v8a/libstarry_magic.so
)

python3.11 - "$BASE_APK" "$WORK/base.apk" <<'PY'
import sys, zipfile
src, dst = sys.argv[1], sys.argv[2]
expected = {"classes2.dex", "lib/arm64-v8a/libstarry_magic.so"}
with zipfile.ZipFile(src) as a, zipfile.ZipFile(dst) as b:
    names_a = a.namelist()
    names_b = b.namelist()
    if names_a != names_b:
        raise SystemExit("ERROR: zip entry list changed")
    changed = []
    for name in names_a:
        ia, ib = a.getinfo(name), b.getinfo(name)
        if ia.CRC != ib.CRC or ia.file_size != ib.file_size:
            changed.append(name)
    if set(changed) != expected:
        raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
print("zip entry audit: only classes2.dex + libstarry_magic.so changed")
PY

"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$WORK/aligned.apk"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT_APK" "$WORK/aligned.apk"
"$BT/apksigner" verify --verbose --print-certs "$OUT_APK" > "$WORK/verify.txt"

LC_ALL=C grep -q "CN=StarryMagic" "$WORK/verify.txt" || {
  echo "ERROR: signed apk does not use StarryMagic certificate" >&2
  cat "$WORK/verify.txt" >&2
  exit 1
}
LC_ALL=C grep -q "SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d" "$WORK/verify.txt" || {
  echo "ERROR: unexpected signer SHA-1" >&2
  cat "$WORK/verify.txt" >&2
  exit 1
}

unzip -p "$OUT_APK" classes2.dex > "$WORK/final_classes2.dex"
unzip -p "$OUT_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
cmp -s "$WORK/classes2_new.dex" "$WORK/final_classes2.dex" || {
  echo "ERROR: packaged classes2.dex differs" >&2
  exit 1
}
cmp -s "$SO" "$WORK/final.so" || { echo "ERROR: packaged so differs" >&2; exit 1; }
"$BAKSMALI" list methods "$WORK/final_classes2.dex" | rg -q \
  '^Lcom/starry/magic/bridge/ModBridge;->nativeGetPinnedCert\(\)\[B$' || {
  echo "ERROR: packaged dex missing nativeGetPinnedCert" >&2
  exit 1
}
"$BAKSMALI" list methods "$WORK/final_classes2.dex" | rg -q \
  '^Lcom/starry/magic/bridge/ModBridge;->nativeBridgeRegistered\(\)Z$' || {
  echo "ERROR: packaged dex missing nativeBridgeRegistered" >&2
  exit 1
}

echo "[pair] done -> $OUT_APK"
echo "[pair] apk sha256 = $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[pair] apk md5    = $(md5 -q "$OUT_APK")"
echo "[pair] dex sha256 = $(shasum -a 256 "$WORK/classes2_new.dex" | awk '{print $1}')"
echo "[pair] so  sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')"
