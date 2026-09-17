#!/usr/bin/env bash
# 动物大联盟 1.0.23：提取 Cocos JS/JSC、关键 so 与 DEX 分析输入。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META="$SCRIPT_DIR/official_apk.json"
OUT="$NIXIANG_ROOT/06_dumped/动物大联盟/official_1.0.23"
[[ -f "$META" ]] || { echo "错误: 缺少 $META" >&2; exit 1; }
APK="$(python3 - "$META" <<'PY'
import json,sys
print(json.load(open(sys.argv[1],encoding='utf-8'))['apk_path'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
rm -rf "$OUT"
mkdir -p "$OUT/apk_assets" "$OUT/apk_libs" "$OUT/dex"
python3 - "$APK" "$OUT" <<'PY'
import sys, zipfile
from pathlib import Path
apk = Path(sys.argv[1])
out = Path(sys.argv[2])
with zipfile.ZipFile(apk) as z:
    infos = z.infolist()
    (out / 'zip_manifest.txt').write_text(
        '\n'.join(f'{i.file_size}\t{i.filename}' for i in infos) + '\n',
        encoding='utf-8')
    for info in infos:
        n = info.filename
        if info.is_dir():
            continue
        keep = (
            n == 'assets/main.js' or
            n == 'assets/project.json' or
            n.startswith('assets/src/') and n.endswith(('.js', '.jsc')) or
            (n.startswith('assets/') and n.count('/') <= 4 and n.endswith('.json')) or
            n in {
                'lib/arm64-v8a/libcocos2djs.so',
                'lib/arm64-v8a/libEncryptorP.so',
                'lib/arm64-v8a/libsecret.so',
                'lib/arm64-v8a/libtapsdkcore.so',
            } or
            n == 'assets/apksecuresolib/arm64-v8a/libcocos2djs.so' or
            (n.startswith('classes') and n.endswith('.dex'))
        )
        if not keep:
            continue
        root = out / 'apk_assets' if n.startswith('assets/') else out / 'apk_libs' if n.startswith('lib/') else out / 'dex'
        rel = Path(n)
        if rel.is_absolute() or '..' in rel.parts:
            raise SystemExit(f'unsafe zip entry: {n}')
        dst = root / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        with z.open(info) as src, open(dst, 'wb') as f:
            while True:
                chunk = src.read(1024 * 1024)
                if not chunk:
                    break
                f.write(chunk)
PY
python3 - "$APK" "$OUT" <<'PY'
import hashlib, json, sys, zipfile
from pathlib import Path
apk = Path(sys.argv[1])
out = Path(sys.argv[2])
records = []
with zipfile.ZipFile(apk) as z:
    for info in z.infolist():
        if info.is_dir() or not info.filename.startswith('assets/'):
            continue
        if not info.filename.endswith(('.js', '.jsc', '.json')):
            continue
        data = z.read(info)
        records.append({'path': info.filename, 'size': len(data), 'sha256': hashlib.sha256(data).hexdigest()})
summary = {
    'apk': str(apk),
    'package': 'com.fightingstars.games.animal',
    'version': '1.0.23',
    'asset_count': len(records),
    'jsc_count': sum(1 for r in records if r['path'].endswith('.jsc')),
    'js_count': sum(1 for r in records if r['path'].endswith('.js')),
    'records': records,
}
(out / 'summary.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
(out / 'summary.txt').write_text('\n'.join([
    f"apk={apk}",
    f"asset_count={summary['asset_count']}",
    f"jsc_count={summary['jsc_count']}",
    f"js_count={summary['js_count']}",
] + [f"{r['size']}\t{r['sha256']}\t{r['path']}" for r in records]) + '\n', encoding='utf-8')
print((out / 'summary.txt').read_text(encoding='utf-8'))
PY
printf 'apk=%s\nout=%s\nassets=%s\njsc=%s\ndex=%s\n' "$APK" "$OUT" \
  "$(find "$OUT/apk_assets" -type f | wc -l | tr -d ' ')" \
  "$(find "$OUT/apk_assets" -type f -name '*.jsc' | wc -l | tr -d ' ')" \
  "$(find "$OUT/dex" -type f -name '*.dex' | wc -l | tr -d ' ')"
