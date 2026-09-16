#!/usr/bin/env bash
# 镇邪人 1.0.3：从星光底包解密 Cocos Creator JSC（XXTEA + gzip）。
# 分析入口：不拆游戏包，直接只读 APK 内 JSC，产物落 06_dumped/镇邪人。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="${1:-$NIXIANG_ROOT/02_apks/镇邪人/镇邪人_1.0.3_starry_from_downloads_20260916.apk}"
OUT_ROOT="${2:-$NIXIANG_ROOT/06_dumped/镇邪人/starry_1.0.3/jsc_decrypted}"
KEY_HEX="c25d728a-a9d9-4c"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
"$PYTHON_BIN" -c 'import xxtea, zlib' 2>/dev/null || {
  echo "ERROR: $PYTHON_BIN missing xxtea/zlib" >&2
  exit 1
}
if [[ -e "$OUT_ROOT" ]]; then
  echo "ERROR: output already exists: $OUT_ROOT" >&2
  exit 1
fi

mkdir -p "$OUT_ROOT"
"$PYTHON_BIN" - "$APK" "$OUT_ROOT" "$KEY_HEX" <<'PY'
import hashlib
import json
import struct
import sys
import zipfile
import zlib
from pathlib import Path

import xxtea

apk = Path(sys.argv[1])
out_root = Path(sys.argv[2])
key = sys.argv[3].encode("ascii")
records = []
errors = []

with zipfile.ZipFile(apk) as z:
    for name in sorted(n for n in z.namelist() if n.endswith(".jsc")):
        rel = Path(name)
        parts = list(rel.parts)
        if parts and parts[0] == "assets":
            parts = parts[1:]
        dst = out_root / Path(*parts).with_suffix(".js")
        dst.parent.mkdir(parents=True, exist_ok=True)
        encrypted = z.read(name)
        try:
            packed = xxtea.decrypt(encrypted, key, padding=False)
            if not packed:
                raise ValueError("xxtea returned empty")
            # Cocos Creator 2.x JSC: XXTEA padding=False -> gzip stream.
            dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
            js = dec.decompress(packed) + dec.flush()
            if not dec.eof:
                raise ValueError("gzip stream did not terminate")
            if not js:
                raise ValueError("empty JS")
            dst.write_bytes(js)
            head = js[:160].decode("utf-8", "replace").replace("\r", " ").replace("\n", " ")
            records.append({
                "apk_entry": name,
                "js": str(dst),
                "encrypted_size": len(encrypted),
                "xxtea_size": len(packed),
                "decrypted_size": len(js),
                "sha256": hashlib.sha256(js).hexdigest(),
                "head": head,
            })
        except Exception as exc:
            errors.append({"apk_entry": name, "error": repr(exc)})

summary = {
    "apk": str(apk),
    "apk_sha256": hashlib.sha256(apk.read_bytes()).hexdigest(),
    "key": key.decode("ascii"),
    "algorithm": "XXTEA(padding=False) + gzip",
    "decrypted_count": len(records),
    "error_count": len(errors),
    "records": records,
    "errors": errors,
}
(out_root / "decrypt_summary.json").write_text(
    json.dumps(summary, ensure_ascii=False, indent=2) + "\n",
    encoding="utf-8",
)
(out_root / "decrypt_summary.txt").write_text(
    "\n".join(
        [f"decrypted={len(records)} errors={len(errors)}"]
        + [f"{r['decrypted_size']}\t{r['apk_entry']}\t{r['js']}" for r in records]
        + [f"ERROR\t{e['apk_entry']}\t{e['error']}" for e in errors]
    ) + "\n",
    encoding="utf-8",
)
print(f"decrypted={len(records)} errors={len(errors)}")
if errors:
    raise SystemExit(1)
PY

cat "$OUT_ROOT/decrypt_summary.txt"
