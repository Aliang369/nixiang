#!/usr/bin/env bash
# 正中靶心2：将斌哥包内 Cocos JSC 解密为可分析 JS。
# 解密是分析操作：只读取已提取的 06_dumped 输入，只写对应分析目录。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC_ROOT="$NIXIANG_ROOT/06_dumped/正中靶心2/binge_1.6/apk_assets"
OUT_ROOT="$NIXIANG_ROOT/06_dumped/正中靶心2/binge_1.6/decrypted_js"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
[[ -d "$SRC_ROOT/assets/src" ]] || { echo "错误: 缺少 JSC 输入目录: $SRC_ROOT/assets/src" >&2; exit 1; }
"$PYTHON_BIN" -c 'import xxtea, zlib' 2>/dev/null || {
  echo "错误: $PYTHON_BIN 缺少 xxtea 模块" >&2
  exit 1
}

rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT"

"$PYTHON_BIN" - "$SRC_ROOT" "$OUT_ROOT" <<'PY'
import hashlib
import json
import sys
import zlib
import xxtea
from pathlib import Path

src_root = Path(sys.argv[1])
out_root = Path(sys.argv[2])
key = b'd5ea8a9c17716f1a'
records = []
errors = []

for src in sorted((src_root / 'assets' / 'src').rglob('*.jsc')):
    rel = src.relative_to(src_root / 'assets')
    dst = out_root / rel.with_suffix('.js')
    dst.parent.mkdir(parents=True, exist_ok=True)
    data = src.read_bytes()
    try:
        encrypted_plain = xxtea.decrypt(data, key, padding=False)
        dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
        js = dec.decompress(encrypted_plain) + dec.flush()
        if not dec.eof:
            raise ValueError('gzip stream did not terminate')
        dst.write_bytes(js)
        records.append({
            'jsc': str(src),
            'js': str(dst),
            'jsc_size': len(data),
            'decrypted_size': len(js),
            'sha256': hashlib.sha256(js).hexdigest(),
        })
    except Exception as exc:
        errors.append({'jsc': str(src), 'error': repr(exc)})

summary = {
    'key': key.decode(),
    'decrypted_count': len(records),
    'error_count': len(errors),
    'records': records,
    'errors': errors,
}
(out_root / 'decrypt_summary.json').write_text(
    json.dumps(summary, ensure_ascii=False, indent=2) + '\n',
    encoding='utf-8',
)
(out_root / 'decrypt_summary.txt').write_text(
    '\n'.join(
        [f"decrypted={len(records)} errors={len(errors)}"]
        + [f"{r['decrypted_size']}\t{r['js']}" for r in records]
        + [f"ERROR\t{e['jsc']}\t{e['error']}" for e in errors]
    ) + '\n',
    encoding='utf-8',
)
print(f"decrypted={len(records)} errors={len(errors)}")
if errors:
    raise SystemExit(1)
PY

cat "$OUT_ROOT/decrypt_summary.txt"
