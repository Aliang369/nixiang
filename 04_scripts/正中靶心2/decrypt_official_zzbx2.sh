#!/usr/bin/env bash
# 正中靶心2 官方 1.7：XXTEA+gzip 解密 JSC。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SRC="$NIXIANG_ROOT/06_dumped/正中靶心2/official_1.7/apk_assets"
OUT="$NIXIANG_ROOT/06_dumped/正中靶心2/official_1.7/decrypted_js"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"
[[ -d "$SRC/assets/src" ]] || { echo "错误: 缺少官方 dump" >&2; exit 1; }
[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
rm -rf "$OUT"; mkdir -p "$OUT"
"$PYTHON_BIN" - "$SRC" "$OUT" <<'PY'
import hashlib, json, sys, zlib, xxtea
from pathlib import Path
src_root=Path(sys.argv[1]); out_root=Path(sys.argv[2]); key=b'd5ea8a9c17716f1a'; records=[]; errors=[]
for src in sorted((src_root/'assets'/'src').rglob('*.jsc')):
    rel=src.relative_to(src_root/'assets'); dst=out_root/rel.with_suffix('.js'); dst.parent.mkdir(parents=True,exist_ok=True)
    try:
        enc=xxtea.decrypt(src.read_bytes(),key,padding=False)
        d=zlib.decompressobj(16+zlib.MAX_WBITS); js=d.decompress(enc)+d.flush()
        if not d.eof: raise ValueError('gzip incomplete')
        dst.write_bytes(js)
        records.append({'jsc':str(src),'js':str(dst),'decrypted_size':len(js),'sha256':hashlib.sha256(js).hexdigest()})
    except Exception as e: errors.append({'jsc':str(src),'error':repr(e)})
summary={'key':key.decode(),'decrypted_count':len(records),'error_count':len(errors),'records':records,'errors':errors}
(out_root/'decrypt_summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(out_root/'decrypt_summary.txt').write_text('\n'.join([f'decrypted={len(records)} errors={len(errors)}']+[f"{r['decrypted_size']}\t{r['js']}" for r in records]+[f"ERROR\t{e['jsc']}\t{e['error']}" for e in errors])+'\n',encoding='utf-8')
print(f'decrypted={len(records)} errors={len(errors)}')
if errors: raise SystemExit(1)
PY
cat "$OUT/decrypt_summary.txt"
