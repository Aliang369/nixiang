#!/usr/bin/env bash
# 无悔华夏 4399：仅替换星光 so 的增量重签。
# 用途：无壳包结构已不变，仅修正 native 鉴权 game_id（whhx -> whhx4399）。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX4399_BASE_APK:-$NIXIANG/08_output/无悔华夏4399/无悔华夏4399_3.8.280_无壳七功能-releaseSigned.apk}"
SO="${WHRHX4399_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx4399/arm64-v8a/libstarry_magic.so}"
OUT="$NIXIANG/08_output/无悔华夏4399/无悔华夏4399_3.8.280_无壳七功能_whhx4399-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base APK: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing so: $SO" >&2; exit 1; }
LC_ALL=C grep -a -q 'whhx4399' "$SO" || { echo "ERROR: so missing whhx4399" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx4399_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$(dirname "$OUT")"

python3.11 - "$BASE_APK" "$SO" "$WORK/unsigned.apk" <<'PY'
import hashlib
import sys
import zipfile
from pathlib import Path

base, so_path, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
so = so_path.read_bytes()
with zipfile.ZipFile(base, "r") as zin, zipfile.ZipFile(dst, "w") as zout:
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/"):
            continue
        if name == "lib/arm64-v8a/libstarry_magic.so":
            data = so
        else:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(dst, "r") as z:
    got = z.read("lib/arm64-v8a/libstarry_magic.so")
    assert hashlib.sha256(got).hexdigest() == hashlib.sha256(so).hexdigest()
    for name in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
                 "resources.arsc", "lib/arm64-v8a/libil2cpp.so",
                 "assets/bin/Data/Managed/Metadata/global-metadata.dat"):
        assert name in z.namelist(), name
print("  so-only merge ok")
PY

"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --verbose --print-certs "$OUT" | sed -n '1,12p'
printf 'so_sha256='; unzip -p "$OUT" lib/arm64-v8a/libstarry_magic.so | shasum -a 256 | awk '{print $1}'
printf 'apk_sha256='; shasum -a 256 "$OUT" | awk '{print $1}'
printf 'apk_md5='; md5 -q "$OUT"
echo "DONE $OUT"
