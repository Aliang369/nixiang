#!/usr/bin/env bash
# 宝石战争 6.0.012：提取并解密可分析热更程序集，准备 IL 注入输入。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
OUT="$NIXIANG_ROOT/06_dumped/宝石战争/6.0.012/static"
HOT_DIR="${BSZZ_HOTUPDATE_DIR:-}"

if [[ -n "${BSZZ_SOURCE_APK:-}" ]]; then
  APK="${BSZZ_SOURCE_APK}"
  [[ "$APK" = /* ]] || APK="$NIXIANG_ROOT/$APK"
else
  APK="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["apk_path"])' "$SCRIPT_DIR/official_apk.json")"
fi
[[ -f "$APK" ]] || { echo "ERROR: missing source APK: $APK" >&2; exit 1; }
mkdir -p "$OUT"

if [[ -n "$HOT_DIR" ]]; then
  [[ "$HOT_DIR" = /* ]] || HOT_DIR="$NIXIANG_ROOT/$HOT_DIR"
  for rel in Dlls/System.Network.dll.bytes Dlls/System.Platform.dll.bytes manifest.json RuntimeAssembliesReceipt.json; do
    [[ -f "$HOT_DIR/$rel" ]] || { echo "ERROR: missing hotupdate input: $HOT_DIR/$rel" >&2; exit 1; }
  done
  cp -f "$HOT_DIR/Dlls/System.Network.dll.bytes" "$OUT/System.Network.dll.bytes"
  cp -f "$HOT_DIR/Dlls/System.Platform.dll.bytes" "$OUT/System.Platform.dll.bytes"
  cp -f "$HOT_DIR/manifest.json" "$OUT/manifest.json"
  cp -f "$HOT_DIR/RuntimeAssembliesReceipt.json" "$OUT/RuntimeAssembliesReceipt.json"
else
  unzip -p "$APK" assets/Android/Dlls/System.Network.dll.bytes > "$OUT/System.Network.dll.bytes"
  unzip -p "$APK" assets/Android/Dlls/System.Platform.dll.bytes > "$OUT/System.Platform.dll.bytes"
  unzip -p "$APK" assets/Android/manifest.json > "$OUT/manifest.json"
  unzip -p "$APK" assets/Android/RuntimeAssembliesReceipt.json > "$OUT/RuntimeAssembliesReceipt.json"
fi
unzip -p "$APK" assets/bin/Data/Managed/Metadata/global-metadata.dat > "$OUT/global-metadata.dat"
unzip -p "$APK" lib/arm64-v8a/libil2cpp.so > "$OUT/libil2cpp.so"

python3 - "$OUT" "$APK" "$HOT_DIR" <<'PY'
from pathlib import Path
import hashlib, json, sys
out = Path(sys.argv[1])
hot_dir = sys.argv[3]
key = bytes((0x04, 0x10, 0x05, 0x1B, 0x0A, 0x01))
for name in ("System.Network", "System.Platform"):
    data = bytearray((out / f"{name}.dll.bytes").read_bytes())
    for i in range(min(0x100, len(data))):
        data[i] ^= key[i % len(key)]
    (out / f"{name}.dll").write_bytes(data)
summary = {
    "source_apk": sys.argv[2],
    "source_apk_md5": hashlib.md5(Path(sys.argv[2]).read_bytes()).hexdigest(),
    "source_hotupdate_dir": hot_dir,
    "source_hotupdate_manifest_version": json.loads((out / "manifest.json").read_text(encoding="utf-8")).get("version"),
    "system_network_plain_md5": hashlib.md5((out / "System.Network.dll").read_bytes()).hexdigest(),
    "system_network_plain_size": (out / "System.Network.dll").stat().st_size,
    "system_network_encrypted_md5": hashlib.md5((out / "System.Network.dll.bytes").read_bytes()).hexdigest(),
    "global_metadata_md5": hashlib.md5((out / "global-metadata.dat").read_bytes()).hexdigest(),
    "libil2cpp_md5": hashlib.md5((out / "libil2cpp.so").read_bytes()).hexdigest(),
}
(out / "static_inputs.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$OUT/static_inputs.json"
