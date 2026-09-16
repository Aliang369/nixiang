#!/usr/bin/env bash
# 山海幻境 4.0：候选包签名、结构、Manifest、包内 so 与官方字节保留核验。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT_FILE="${SHHJ_VERIFY_OUT:-$SCRIPT_DIR/candidate_verify.json}"
APK="${1:-$NIXIANG_ROOT/08_output/山海幻境/山海幻境_4_surgical-releaseSigned.apk}"
EXPECTED_C3_PATCH="${SHHJ_EXPECT_CLASSES3_PATCH:-0}"
SO="$NIXIANG_ROOT/07_patched/山海幻境/build_4/libstarry_magic.so"
PKG="com.chx.shhj"
VERSION_NAME="4"
VERSION_CODE="54"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "错误: 候选包不存在: $APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "错误: 构建 so 不存在: $SO" >&2; exit 1; }
ORIG_APK="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])
PY
)"
[[ -f "$ORIG_APK" ]] || { echo "错误: 官方底包不存在: $ORIG_APK" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "错误: 找不到 Android build-tools" >&2; exit 1; }

BADGING="$("$BT/aapt2" dump badging "$APK")"
PACKAGE_LINE="$(printf '%s\n' "$BADGING" | sed -n '1p')"
printf '%s\n' "$PACKAGE_LINE" | grep -Fq "name='$PKG'" || { echo "错误: 包名错误: $PACKAGE_LINE" >&2; exit 1; }
printf '%s\n' "$PACKAGE_LINE" | grep -Fq "versionCode='$VERSION_CODE'" || { echo "错误: versionCode 错误" >&2; exit 1; }
printf '%s\n' "$PACKAGE_LINE" | grep -Fq "versionName='$VERSION_NAME'" || { echo "错误: versionName 错误" >&2; exit 1; }

"$BT/zipalign" -c -p 4 "$APK" >/dev/null
SIGN_OUT="$("$BT/apksigner" verify --verbose --print-certs "$APK" 2>&1)"
printf '%s\n' "$SIGN_OUT" | grep -Fq 'Verified using v1 scheme (JAR signing): true' || { echo "错误: v1 签名未通过" >&2; exit 1; }
printf '%s\n' "$SIGN_OUT" | grep -Fq 'Verified using v2 scheme (APK Signature Scheme v2): true' || { echo "错误: v2 签名未通过" >&2; exit 1; }
printf '%s\n' "$SIGN_OUT" | grep -Fq 'Verified using v3 scheme (APK Signature Scheme v3): true' || { echo "错误: v3 签名未通过" >&2; exit 1; }
CERT_MD5="$(printf '%s\n' "$SIGN_OUT" | sed -n 's/.*certificate MD5 digest: //p' | head -1 | tr 'A-F' 'a-f')"
CERT_SHA256="$(printf '%s\n' "$SIGN_OUT" | sed -n 's/.*certificate SHA-256 digest: //p' | head -1 | tr 'A-F' 'a-f')"

XMLTREE="$("$BT/aapt2" dump xmltree "$APK" --file AndroidManifest.xml)"
printf '%s\n' "$XMLTREE" | grep -Fq 'com.starry.magic.StarryApp' || { echo "错误: Manifest 未指向 StarryApp" >&2; exit 1; }
printf '%s\n' "$XMLTREE" | grep -Fq 'com.starry.magic.menu.MenuService' || { echo "错误: Manifest 缺少 MenuService" >&2; exit 1; }

SO_MD5_EXPECTED="$(md5 -q "$SO")"
SO_MD5_IN_APK="$(unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5_IN_APK" == "$SO_MD5_EXPECTED" ]] || { echo "错误: 包内 so 与构建产物不一致" >&2; exit 1; }

python3 - "$ORIG_APK" "$APK" "$OUT_FILE" "$SO_MD5_IN_APK" "$CERT_MD5" "$CERT_SHA256" "$EXPECTED_C3_PATCH" <<'PY'
import hashlib
import json
import sys
import zipfile
from pathlib import Path

def file_digest(path, algo):
    h = hashlib.new(algo)
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8 * 1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()

orig_path, apk_path, out_path, so_md5, cert_md5, cert_sha256, expected_c3_patch = sys.argv[1:]
expected_c3_patch = expected_c3_patch == "1"
with zipfile.ZipFile(orig_path) as a, zipfile.ZipFile(apk_path) as b:
    an = set(a.namelist())
    bn = set(b.namelist())
    removed = sorted(an - bn)
    added = sorted(bn - an)
    changed = []
    preserved = 0
    for name in sorted(an & bn):
        skip = name in {"AndroidManifest.xml", "classes.dex"} or name.startswith("META-INF/")
        if expected_c3_patch and name == "classes3.dex":
            skip = True
        if skip:
            continue
        ha = hashlib.sha256(a.read(name)).hexdigest()
        hb = hashlib.sha256(b.read(name)).hexdigest()
        if ha != hb:
            changed.append(name)
        else:
            preserved += 1

    if expected_c3_patch:
        if hashlib.sha256(a.read("classes3.dex")).hexdigest() == hashlib.sha256(b.read("classes3.dex")).hexdigest():
            raise SystemExit("classes3.dex package-sign patch was not applied")
        if b"86d1959875555021642fcf3b0d9c161d" not in b.read("classes3.dex"):
            raise SystemExit("official package-sign MD5 missing from patched classes3.dex")

removed_non_meta = [x for x in removed if not x.startswith("META-INF/")]
unexpected_added = [
    x for x in added
    if x not in {
        "META-INF/STARRY.RSA", "META-INF/STARRY.SF",
        "assets/certs/starlight.crt", "classes4.dex",
        "lib/arm64-v8a/libc++_shared.so",
        "lib/arm64-v8a/libstarry_magic.so",
    }
]
if changed or removed_non_meta or unexpected_added:
    raise SystemExit(
        f"官方字节保留失败 changed={changed[:10]} removed={removed_non_meta[:10]} added={unexpected_added[:10]}"
    )
data = {
    "apk_path": apk_path,
    "apk_md5": file_digest(apk_path, "md5"),
    "apk_sha256": file_digest(apk_path, "sha256"),
    "package": "com.chx.shhj",
    "version_name": "4",
    "version_code": 54,
    "signer_cert_md5": cert_md5,
    "signer_cert_sha256": cert_sha256,
    "v1_verified": True,
    "v2_verified": True,
    "v3_verified": True,
    "zipalign_verified": True,
    "libstarry_magic_md5": so_md5,
    "manifest_starry_app": True,
    "manifest_menu_service": True,
    "classes3_package_sign_patch": expected_c3_patch,
    "official_common_entries_preserved": preserved,
    "official_common_entries_changed": changed,
    "official_non_meta_removed": removed_non_meta,
    "unexpected_added": unexpected_added,
}
Path(out_path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY
