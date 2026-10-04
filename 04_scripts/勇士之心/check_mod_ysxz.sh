#!/usr/bin/env bash
# 勇士之心：核对斌哥改包身份、签名、引擎与壳特征。只读本地 APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
META_FILE="$SCRIPT_DIR/binge_apk.json"
CHECK_JSON="$SCRIPT_DIR/mod_check.json"
MANIFEST_TXT="$SCRIPT_DIR/mod_manifest.txt"
SIGNATURE_TXT="$SCRIPT_DIR/mod_signature.txt"
ENTRIES_TXT="$SCRIPT_DIR/mod_entries.txt"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 binge_apk.json" >&2; exit 1; }
APK="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META_FILE")"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -x "$AAPT2" ]] || { echo "错误: aapt2 不可执行: $AAPT2" >&2; exit 1; }
[[ -x "$APKSIGNER" ]] || { echo "错误: apksigner 不可执行: $APKSIGNER" >&2; exit 1; }

"$AAPT2" dump badging "$APK" > "$MANIFEST_TXT"
"$APKSIGNER" verify --print-certs "$APK" > "$SIGNATURE_TXT"
unzip -Z1 "$APK" > "$ENTRIES_TXT"

PACKAGE="$(sed -n "s/^package: name='\([^']*\)'.*/\1/p" "$MANIFEST_TXT" | head -1)"
VERSION_NAME="$(sed -n "s/^package: .* versionName='\([^']*\)'.*/\1/p" "$MANIFEST_TXT" | head -1)"
VERSION_CODE="$(sed -n "s/^package: .* versionCode='\([^']*\)'.*/\1/p" "$MANIFEST_TXT" | head -1)"
LAUNCHABLE_ACTIVITY="$(sed -n "s/^launchable-activity: name='\([^']*\)'.*/\1/p" "$MANIFEST_TXT" | head -1)"
SIGNER_DN="$(sed -n 's/^Signer #1 certificate DN: //p' "$SIGNATURE_TXT" | head -1)"
SIGNER_SHA256="$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$SIGNATURE_TXT" | head -1)"

if ! grep -Fxq "lib/arm64-v8a/libil2cpp.so" "$ENTRIES_TXT" && ! grep -Fxq "lib/arm64-v8a/libcocos2djs.so" "$ENTRIES_TXT" && ! grep -Eq '^lib/arm64-v8a/libcocos[^/]*\.so$' "$ENTRIES_TXT"; then
  echo "错误: 未识别到已知 IL2CPP/Cocos 引擎核心，停止定性" >&2
  sed -n '1,80p' "$ENTRIES_TXT" >&2
  exit 1
fi

python3 - "$CHECK_JSON" "$ENTRIES_TXT" "$MANIFEST_TXT" "$SIGNATURE_TXT" "$PACKAGE" "$VERSION_NAME" "$VERSION_CODE" "$LAUNCHABLE_ACTIVITY" "$SIGNER_DN" "$SIGNER_SHA256" "$APK" <<'PY'
import hashlib
import json
import sys
from pathlib import Path

out, entries_path, manifest_path, signature_path, package, version_name, version_code, activity, signer_dn, signer_sha256, apk = sys.argv[1:]
entries = [line.strip() for line in Path(entries_path).read_text(encoding="utf-8", errors="replace").splitlines() if line.strip()]

def find_exact(value):
    return value in entries

def find_prefixes(prefix):
    return [x for x in entries if x.startswith(prefix)]

il2cpp = find_exact("lib/arm64-v8a/libil2cpp.so")
metadata = find_exact("assets/bin/Data/Managed/Metadata/global-metadata.dat")
cocos = [x for x in entries if x.startswith("lib/arm64-v8a/libcocos") or x == "lib/arm64-v8a/libcocos2djs.so"]
cocos_assets = [x for x in entries if x.endswith(".jsc") or x in {"assets/main.js", "assets/index.js"}]
shell_hits = [x for x in entries if any(marker in x for marker in ("libSDKloading.so", "libtoolChecker.so", "libcodec2_server.so", "libunityluac.so", "assets/base.apk", "shell_config.xml", "shell_protected"))]
engine = "unity_il2cpp" if il2cpp and metadata else "cocos" if cocos else "unknown"

manifest_text = Path(manifest_path).read_text(encoding="utf-8", errors="replace")
signature_text = Path(signature_path).read_text(encoding="utf-8", errors="replace")
data = {
    "game": "勇士之心",
    "apk_path": apk,
    "apk_md5": hashlib.md5(Path(apk).read_bytes()).hexdigest(),
    "apk_sha256": hashlib.sha256(Path(apk).read_bytes()).hexdigest(),
    "apk_size": Path(apk).stat().st_size,
    "package": package,
    "version_name": version_name,
    "version_code": version_code,
    "launchable_activity": activity,
    "engine": engine,
    "engine_evidence": {
        "libil2cpp": il2cpp,
        "global_metadata": metadata,
        "cocos_native": cocos,
        "cocos_assets": cocos_assets,
    },
    "shell_hits": shell_hits,
    "signer_dn": signer_dn,
    "signer_sha256": signer_sha256,
    "binge_signature_hit": "binge2" in signer_dn.lower(),
    "manifest_badging": manifest_text.splitlines()[:80],
    "signature_verify": signature_text.splitlines()[:80],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$CHECK_JSON"
