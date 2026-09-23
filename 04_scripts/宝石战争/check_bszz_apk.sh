#!/usr/bin/env bash
# 宝石战争 6.0.012：官包身份、签名、Unity/HybridCLR 与保护面核对。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META="$SCRIPT_DIR/official_apk.json"
OUT="$SCRIPT_DIR/official_check.json"
APK="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["apk_path"])' "$META")"
BT="$(ls -d "$HOME"/Library/Android/sdk/build-tools/* 2>/dev/null | sort -V | tail -1)"
APKSIGNER="$BT/apksigner"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }
[[ -x "$APKSIGNER" ]] || { echo "ERROR: apksigner missing" >&2; exit 1; }
EXPECT_MD5="${BSZZ_OFFICIAL_MD5:-$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["md5"])' "$META")}"
[[ "$(md5 -q "$APK")" == "$EXPECT_MD5" ]] || { echo "ERROR: APK md5 mismatch" >&2; exit 1; }

unzip -tq "$APK" >/dev/null
unzip -Z1 "$APK" > "$SCRIPT_DIR/apk_entries.txt"
for name in classes.dex classes2.dex resources.arsc assets/bin/Data/Managed/Metadata/global-metadata.dat \
  assets/Android/manifest.json assets/Android/Dlls/System.Network.dll.bytes lib/arm64-v8a/libil2cpp.so; do
  grep -Fxq "$name" "$SCRIPT_DIR/apk_entries.txt" || { echo "ERROR: missing $name" >&2; exit 1; }
done
if grep -Eiq 'libjiagu|DexHelper|nesec|APKSecure|kwprotect|SecShell|libshell|libprotect' "$SCRIPT_DIR/apk_entries.txt"; then
  echo "ERROR: 发现疑似整包壳条目" >&2
  exit 1
fi

SIGN="$("$APKSIGNER" verify --verbose --print-certs "$APK" | tr -d '\r')"
printf '%s\n' "$SIGN" | grep -q 'Verified using v2 scheme (APK Signature Scheme v2): true' || { echo "ERROR: v2 signature missing" >&2; exit 1; }
printf '%s\n' "$SIGN" | grep -q 'Signer #1 certificate SHA-256 digest: 8943b31609712c8c7fdf583ce26b204ad8b01dad8eb09f4c7d1f276458985bb4' \
  || { echo "ERROR: signer mismatch" >&2; exit 1; }

python3 - "$META" "$OUT" "$APK" <<'PY'
import hashlib, json, sys, zipfile
from pathlib import Path
meta = json.load(open(sys.argv[1], encoding="utf-8"))
apk = Path(sys.argv[3])
with zipfile.ZipFile(apk) as z:
    manifest = json.loads(z.read("assets/Android/manifest.json"))
    receipt = json.loads(z.read("assets/Android/RuntimeAssembliesReceipt.json"))
    meta_bytes = z.read("assets/bin/Data/Managed/Metadata/global-metadata.dat")
    network_bytes = z.read("assets/Android/Dlls/System.Network.dll.bytes")
data = {
    "apk": str(apk),
    "package": meta["package"],
    "version_name": meta["version_name"],
    "version_code": meta["version_code"],
    "originating_package": meta["originating_package"],
    "size": meta["size"],
    "md5": meta["md5"],
    "sha256": meta["sha256"],
    "signer_sha256": "8943b31609712c8c7fdf583ce26b204ad8b01dad8eb09f4c7d1f276458985bb4",
    "global_metadata_size": len(meta_bytes),
    "global_metadata_md5": hashlib.md5(meta_bytes).hexdigest(),
    "system_network_size": len(network_bytes),
    "system_network_md5": hashlib.md5(network_bytes).hexdigest(),
    "hot_update_version": manifest.get("version"),
    "hybridclr_version": receipt.get("hybridCLRVersion"),
    "unity_version": receipt.get("unityVersion"),
    "traditional_shell": False,
}
Path(sys.argv[2]).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY
cat "$OUT"
