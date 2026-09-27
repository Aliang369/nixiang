#!/usr/bin/env bash
# 幻想武将模拟器 1.0.8 单包构建：SecShell 条目名集合不变 + 原 CRC 回填。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="幻想武将模拟器"
VERSION="1.0.8"
PKG="com.qxjc.hxwjmlq"
ORIG_APK="${ORIG_APK:-$ROOT/02_apks/$GAME/${GAME}_${VERSION}_taptap_original.apk}"
ORIG_MD5="${ORIG_MD5:-68ed2632579329c3700559447aba77f0}"
FINAL_DIR="$ROOT/08_output/$GAME"
FINAL="$FINAL_DIR/${GAME}_${VERSION}.apk"
BT="${BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
D8="$BT/d8"
APKSIGNER="$BT/apksigner"
ZIPALIGN="$BT/zipalign"
KS="${STARRY_KS:-$ROOT/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
TOOLS="$ROOT/04_scripts/$GAME/tools"
STATIC_SO="$ROOT/mod菜单/starry-mod-menu/libs_hxwj_static/arm64-v8a/libstarry_magic.so"
SURGICAL="$FINAL_DIR/${GAME}_${VERSION}_surgical-unsigned.apk"
MANIFEST="$ROOT/07_patched/$GAME/surgical/AndroidManifest.xml"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺官方底包 $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 官方底包 md5 不符" >&2; exit 1; }
[[ -x "$D8" && -x "$APKSIGNER" && -x "$ZIPALIGN" ]] || { echo "ERROR: build-tools 缺 d8/apksigner/zipalign" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: 缺签名库 $KS" >&2; exit 1; }
[[ -f "$TOOLS/hxwj_rawzip.py" && -f "$TOOLS/hxwj_forge_crc.py" ]] || { echo "ERROR: 缺 zip 外科工具" >&2; exit 1; }

echo "[single] 1/6 构建静态 libc++ 的星光 so"
bash "$ROOT/04_scripts/$GAME/hxwj_build_so_static.sh"
[[ -f "$STATIC_SO" ]] || { echo "ERROR: 静态 so 未生成" >&2; exit 1; }

echo "[single] 2/6 重编星光 dex（复用外科脚本的 classes2.dex）"
bash "$ROOT/mod菜单/starry-mod-menu/scripts/surgical_hxwj_1.0.8.sh" >/dev/null
[[ -f "$SURGICAL" ]] || { echo "ERROR: 外科中间包未生成" >&2; exit 1; }
WORK="$(mktemp -d /tmp/hxwj_single.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
unzip -p "$SURGICAL" classes2.dex > "$WORK/classes2.dex"
unzip -p "$ORIG_APK" classes.dex > "$WORK/orig_classes.dex"

echo "[single] 3/6 d8 合并官方壳 dex + 星光 dex"
mkdir -p "$WORK/dex"
"$D8" --min-api 22 --output "$WORK/dex" "$WORK/orig_classes.dex" "$WORK/classes2.dex"

[[ -f "$MANIFEST" ]] || { echo "ERROR: 缺分支 B Manifest $MANIFEST" >&2; exit 1; }

echo "[single] 4/6 raw zip 替换三个既有条目"
python3 - "$ORIG_APK" "$WORK" "$MANIFEST" "$STATIC_SO" <<'PY'
import json, os, sys
src, work, manifest, static_so = sys.argv[1:]
spec = {
    "src": src,
    "dst": os.path.join(work, "unsigned.apk"),
    "changes": {
        "classes.dex": os.path.join(work, "dex", "classes.dex"),
        "AndroidManifest.xml": manifest,
        "lib/arm64-v8a/libSecShell-x86.so": static_so,
    },
}
with open(os.path.join(work, "spec.json"), "w", encoding="utf-8") as f:
    json.dump(spec, f, ensure_ascii=False, indent=2)
PY
python3 "$TOOLS/hxwj_rawzip.py" "$WORK/spec.json" >/dev/null
python3 "$TOOLS/hxwj_forge_crc.py" "$WORK/unsigned.apk" classes.dex 7632cc55 >/dev/null
python3 "$TOOLS/hxwj_forge_crc.py" "$WORK/unsigned.apk" AndroidManifest.xml 80bc27f3 >/dev/null
python3 "$TOOLS/hxwj_forge_crc.py" "$WORK/unsigned.apk" lib/arm64-v8a/libSecShell-x86.so b486eca9 >/dev/null

echo "[single] 5/6 签名并写入唯一正式包"
mkdir -p "$FINAL_DIR"
"$ZIPALIGN" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk" >/dev/null
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$FINAL" "$WORK/aligned.apk" >/dev/null
"$APKSIGNER" verify "$FINAL" >/dev/null

echo "[single] 6/6 硬校验"
python3 - "$ORIG_APK" "$FINAL" "$WORK/classes2.dex" "$STATIC_SO" <<'PY'
import hashlib, sys, zipfile
orig, final, classes2, static_so = sys.argv[1:]
expect_crc = {
    "classes.dex": 0x7632cc55,
    "AndroidManifest.xml": 0x80bc27f3,
    "lib/arm64-v8a/libSecShell-x86.so": 0xb486eca9,
}
zo, zf = zipfile.ZipFile(orig), zipfile.ZipFile(final)
def read_unchecked(z, name):
    with z.open(name) as f:
        f._expected_crc = None
        return f.read()
no = {n for n in zo.namelist() if not n.startswith("META-INF/")}
nf = {n for n in zf.namelist() if not n.startswith("META-INF/")}
assert no == nf, (sorted(no - nf)[:5], sorted(nf - no)[:5])
assert len(zf.namelist()) == 1055, len(zf.namelist())
for n, crc in expect_crc.items():
    assert zf.getinfo(n).CRC == crc, (n, hex(zf.getinfo(n).CRC), hex(crc))
for n in ("assets/meta-data/manifest.mf", "assets/meta-data/rsa.sig", "assets/meta-data/rsa.pub"):
    assert read_unchecked(zo, n) == read_unchecked(zf, n), n
dex = read_unchecked(zf, "classes.dex")
so = read_unchecked(zf, "lib/arm64-v8a/libSecShell-x86.so")
assert b"SecShell-x86" in dex
assert b"com.qxjc.hxwjmlq" in dex
assert b"com/starry/magic/StarryInitProvider" in dex
assert b"com/starry/magic/menu/MenuService" in dex
assert b"[hxwj]" in so
assert so == open(static_so, "rb").read()
print("  entries=1055; names unchanged; CRCs forged; meta-data untouched")
PY

# 清理外科中间包，保证 08_output 只剩唯一正式包
rm -f "$SURGICAL"

echo "[single] done"
echo "  final = $FINAL"
echo "  md5   = $(md5 -q "$FINAL")"
echo "  sha256= $(shasum -a 256 "$FINAL" | awk '{print $1}')"
