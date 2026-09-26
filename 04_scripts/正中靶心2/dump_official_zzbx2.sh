#!/usr/bin/env bash
# 正中靶心2 官方 1.7：提取分析输入（main.js/JSC/关键 so/DEX）。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META="$SCRIPT_DIR/official_apk.json"
[[ -f "$META" ]] || { echo "错误: 缺少 $META" >&2; exit 1; }
eval "$(python3 - "$META" <<'PY'
import json,shlex,sys
d=json.load(open(sys.argv[1],encoding='utf-8'))
print('APK='+shlex.quote(d['apk_path']))
print('VERSION_NAME='+shlex.quote(d['version_name']))
PY
)"
OUT="$NIXIANG_ROOT/06_dumped/正中靶心2/official_${VERSION_NAME}"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
rm -rf "$OUT"; mkdir -p "$OUT/apk_assets" "$OUT/apk_libs" "$OUT/dex"
python3 - "$APK" "$OUT" <<'PY'
import sys, zipfile
from pathlib import Path
apk=Path(sys.argv[1]); out=Path(sys.argv[2])
with zipfile.ZipFile(apk) as z:
    infos=z.infolist()
    (out/'zip_manifest.txt').write_text('\n'.join(f'{i.file_size}\t{i.filename}' for i in infos)+'\n',encoding='utf-8')
    for info in infos:
        n=info.filename
        if info.is_dir(): continue
        keep=(n in {'assets/main.js','assets/project.json'} or n.endswith(('.js','.jsc')) or
              (n.startswith('assets/') and n.count('/')<=2 and n.endswith('.json')) or
              n in {'lib/arm64-v8a/libcocos2djs.so','lib/arm64-v8a/libtoolChecker.so'} or
              (n.startswith('classes') and n.endswith('.dex')))
        if not keep: continue
        root=out/'apk_assets' if n.startswith('assets/') else out/'apk_libs' if n.startswith('lib/') else out/'dex'
        rel=Path(n)
        if rel.is_absolute() or '..' in rel.parts: raise SystemExit(f'unsafe: {n}')
        dst=root/rel; dst.parent.mkdir(parents=True,exist_ok=True)
        with z.open(info) as src, open(dst,'wb') as f:
            while True:
                chunk=src.read(1024*1024)
                if not chunk: break
                f.write(chunk)
PY
printf 'apk=%s\nout=%s\nassets=%s\njsc=%s\ndex=%s\n' "$APK" "$OUT" \
  "$(find "$OUT/apk_assets" -type f | wc -l | tr -d ' ')" \
  "$(find "$OUT/apk_assets" -type f -name '*.jsc' | wc -l | tr -d ' ')" \
  "$(find "$OUT/dex" -type f -name '*.dex' | wc -l | tr -d ' ')"
