#!/usr/bin/env bash
# 镇邪人远程热更 src bundle b5278：下载、解密、留证。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
OUT="$ROOT/06_dumped/镇邪人/remote_b5278"
URL="https://ymzxr-cn-res.lansors.com/ymzxr_Android_prod_res/remote/src/index.b5278.jsc"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
mkdir -p "$OUT"
CONFIG_URL="https://ymzxr-cn-res.lansors.com/ymzxr_Android_prod_res/remote/src/config.b5278.json"
curl -fsSL --max-time 30 "$CONFIG_URL" -o "$OUT/config.b5278.json"
curl -fsSL --max-time 60 "$URL" -o "$OUT/index.b5278.encrypted.jsc"
"$PYTHON_BIN" - "$OUT/index.b5278.encrypted.jsc" "$OUT/index.b5278.js" <<'PY'
import sys
import xxtea
import zlib
from pathlib import Path

src = Path(sys.argv[1])
dst = Path(sys.argv[2])
enc = src.read_bytes()
packed = xxtea.decrypt(enc, b"c25d728a-a9d9-4c", padding=False)
dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
js = dec.decompress(packed) + dec.flush()
if not dec.eof:
    raise SystemExit("gzip stream did not terminate")
dst.write_bytes(js)
print(f"decrypted={len(js)} js={dst}")
PY
shasum -a 256 "$OUT/index.b5278.encrypted.jsc" "$OUT/index.b5278.js"
