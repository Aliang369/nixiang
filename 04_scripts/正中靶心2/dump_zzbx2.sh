#!/usr/bin/env bash
# 正中靶心2：从斌哥 APK 提取分析与复刻所需输入。
# 只读取 02_apks 中原包，只写本游戏独立的 06_dumped 产物目录。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/binge_apk.json"
OUT_ROOT="$NIXIANG_ROOT/06_dumped/正中靶心2/binge_1.6"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，请先跑 pull_mod" >&2; exit 1; }
APK="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['apk_path'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT/apk_assets" "$OUT_ROOT/apk_libs" "$OUT_ROOT/dex"

python3 - "$APK" "$OUT_ROOT" <<'PY'
import sys
import zipfile
from pathlib import Path

apk = Path(sys.argv[1])
out_root = Path(sys.argv[2])

with zipfile.ZipFile(apk) as zf:
    infos = zf.infolist()
    (out_root / 'zip_manifest.txt').write_text(
        '\n'.join(f'{i.file_size}\t{i.filename}' for i in infos) + '\n',
        encoding='utf-8',
    )
    selected = []
    for info in infos:
        if info.is_dir():
            continue
        name = info.filename
        keep = name in {'assets/main.js', 'assets/project.json'}
        keep = keep or (name.startswith('assets/jsb-adapter/') and name.endswith('.js'))
        keep = keep or name.endswith(('.js', '.jsc'))
        keep = keep or (name.startswith('assets/') and name.count('/') <= 2 and name.endswith('.json'))
        keep = keep or name in {
            'lib/arm64-v8a/libSDKloading.so',
            'lib/arm64-v8a/libcocos2djs.so',
            'lib/arm64-v8a/libtoolChecker.so',
        }
        keep = keep or (name.startswith('classes') and name.endswith('.dex'))
        if keep:
            selected.append(info)

    for info in selected:
        if info.filename.startswith('assets/'):
            target_root = out_root / 'apk_assets'
        elif info.filename.startswith('lib/'):
            target_root = out_root / 'apk_libs'
        elif info.filename.startswith('classes') and info.filename.endswith('.dex'):
            target_root = out_root / 'dex'
        else:
            target_root = out_root
        rel = Path(info.filename)
        if rel.is_absolute() or '..' in rel.parts:
            raise SystemExit(f'unsafe zip path: {info.filename}')
        target = target_root / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        with zf.open(info) as src, open(target, 'wb') as dst:
            while True:
                chunk = src.read(1024 * 1024)
                if not chunk:
                    break
                dst.write(chunk)
PY

ASSET_COUNT="$(find "$OUT_ROOT/apk_assets" -type f | wc -l | tr -d ' ')"
JSC_COUNT="$(find "$OUT_ROOT/apk_assets" -type f -name '*.jsc' | wc -l | tr -d ' ')"
JS_COUNT="$(find "$OUT_ROOT/apk_assets" -type f -name '*.js' | wc -l | tr -d ' ')"
DEX_COUNT="$(find "$OUT_ROOT/dex" -type f -name '*.dex' | wc -l | tr -d ' ')"
LIB_COUNT="$(find "$OUT_ROOT/apk_libs" -type f -name '*.so' | wc -l | tr -d ' ')"

cat > "$OUT_ROOT/summary.txt" <<EOF
apk=$APK
assets=$ASSET_COUNT
js=$JS_COUNT
jsc=$JSC_COUNT
dex=$DEX_COUNT
native_libs=$LIB_COUNT
main_js=$OUT_ROOT/apk_assets/assets/main.js
out_root=$OUT_ROOT
EOF
cat "$OUT_ROOT/summary.txt"
