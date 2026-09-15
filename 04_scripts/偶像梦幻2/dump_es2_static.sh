#!/usr/bin/env bash
# 偶像梦幻祭2：提取 4.5.8861 静态重定位输入。
# 官方 metadata 历史记录为高熵加密，故只保留原始输入，不假设 Il2CppDumper 可用。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
GAME="偶像梦幻2"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
APK="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['apk_path'])
PY
)"
VERSION="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['version_name'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

OUT_ROOT="$NIXIANG_ROOT/06_dumped/$GAME/official_${VERSION}"
STATIC="$OUT_ROOT/static"
rm -rf "$STATIC"
mkdir -p "$STATIC"
unzip -Z1 "$APK" > "$STATIC/entries.txt"


for entry in \
  'lib/arm64-v8a/libil2cpp.so' \
  'lib/arm64-v8a/libunity.so' \
  'lib/arm64-v8a/libxlua.so' \
  'assets/bin/Data/Managed/Metadata/global-metadata.dat'; do
  name="$(basename "$entry")"
  if grep -Fxq "$entry" "$STATIC/entries.txt"; then
    unzip -p "$APK" "$entry" > "$STATIC/$name"
  fi
done

mkdir -p "$STATIC/dex"
while IFS= read -r dex; do
  [[ -n "$dex" ]] || continue
  unzip -p "$APK" "$dex" > "$STATIC/dex/$dex"
done < <(unzip -Z1 "$APK" | grep -E '^classes([0-9]*)\.dex$' | sort -V)
unzip -p "$APK" AndroidManifest.xml > "$STATIC/AndroidManifest.xml"
zipinfo -v "$APK" resources.arsc > "$STATIC/resources_arsc_zipinfo.txt"

[[ -s "$STATIC/libil2cpp.so" ]] || { echo "错误: libil2cpp.so 未提取" >&2; exit 1; }
[[ -s "$STATIC/global-metadata.dat" ]] || { echo "错误: global-metadata.dat 未提取" >&2; exit 1; }
python3 - "$STATIC/dump_inputs.json" "$APK" "$VERSION" "$STATIC" <<'PY'
import hashlib, json, sys
from pathlib import Path
out, apk, version, static = sys.argv[1:]
root = Path(static)
def digest(name, alg):
    h = hashlib.new(alg)
    with (root / name).open('rb') as f:
        for chunk in iter(lambda: f.read(1 << 20), b''):
            h.update(chunk)
    return h.hexdigest()
data = {
    'game': '偶像梦幻2',
    'version': version,
    'apk_path': apk,
    'static_dir': str(root),
    'libil2cpp_md5': digest('libil2cpp.so', 'md5'),
    'libil2cpp_sha256': digest('libil2cpp.so', 'sha256'),
    'global_metadata_md5': digest('global-metadata.dat', 'md5'),
    'global_metadata_sha256': digest('global-metadata.dat', 'sha256'),
    'dex_files': sorted(p.name for p in (root / 'dex').glob('*.dex')),
    'il2cpp_dumper_applicable': False,
    'reason': '历史 4.4.8684 记录官方 global-metadata.dat 为高熵加密；本阶段仅保留静态输入，重定位按机器码/运行时证据完成。',
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
PY

cat <<EOF
static_dir=$STATIC
libil2cpp=$(stat -f '%z' "$STATIC/libil2cpp.so") bytes
metadata=$(stat -f '%z' "$STATIC/global-metadata.dat") bytes
summary=$STATIC/dump_inputs.json
EOF
