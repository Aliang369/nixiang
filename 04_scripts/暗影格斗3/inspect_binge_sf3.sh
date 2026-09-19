#!/usr/bin/env bash
# 暗影格斗3 斌哥包：静态输入检查与关键组件提取。
# 只读分析 APK，不重打包、不修改原始 APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="$NIXIANG_ROOT/02_apks/暗影格斗3/暗影格斗3_1.45.3_binge_from_device.apk"
OUT="$NIXIANG_ROOT/06_dumped/暗影格斗3/static_inspect"

[[ -f "$APK" ]] || { echo "错误: 缺少输入 APK: $APK" >&2; exit 1; }
rm -rf "$OUT"
mkdir -p "$OUT"

unzip -Z1 "$APK" > "$OUT/zip_entries.txt"
sed -n '1,220p' "$OUT/zip_entries.txt" > "$OUT/zip_entries_head.txt"

pick() {
  local pattern="$1"
  local fallback="$2"
  if rg -q "$pattern" "$OUT/zip_entries.txt"; then
    rg "$pattern" "$OUT/zip_entries.txt" | sed -n '1p'
  else
    printf '%s\n' "$fallback"
  fi
}

MANIFEST="$(pick '^AndroidManifest\.xml$' '')"
IL2CPP="$(pick '^lib/arm64-v8a/libil2cpp\.so$' '')"
METADATA="$(pick '^assets/bin/Data/Managed/Metadata/global-metadata\.dat$' '')"
SDK="$(pick '^lib/arm64-v8a/libSDKloading\.so$' '')"
TOOLCHECK="$(pick '^lib/arm64-v8a/libtoolChecker\.so$' '')"
NESTED="$(pick '^assets/base\.apk$' '')"
UNITY="$(pick '^lib/arm64-v8a/libunity\.so$' '')"

EXTRACT_LIST="$OUT/extract_list.txt"
: > "$EXTRACT_LIST"
for entry in "$MANIFEST" "$IL2CPP" "$METADATA" "$SDK" "$TOOLCHECK" "$NESTED" "$UNITY"; do
  [[ -n "$entry" ]] && printf '%s\n' "$entry" >> "$EXTRACT_LIST"
done

if [[ -s "$EXTRACT_LIST" ]]; then
  EXTRACT_ENTRIES=()
  while IFS= read -r entry; do
    EXTRACT_ENTRIES+=("$entry")
  done < "$EXTRACT_LIST"
  unzip -j -o "$APK" "${EXTRACT_ENTRIES[@]}" -d "$OUT/key_files" >/dev/null
fi

if [[ -n "$MANIFEST" ]]; then
  unzip -p "$APK" "$MANIFEST" > "$OUT/AndroidManifest.bin"
  if command -v apkanalyzer >/dev/null 2>&1; then
    apkanalyzer manifest print "$APK" > "$OUT/AndroidManifest.xml" 2>/dev/null || true
  fi
fi

python3 - "$APK" "$OUT" "$MANIFEST" "$IL2CPP" "$METADATA" "$SDK" "$TOOLCHECK" "$NESTED" "$UNITY" <<'PY'
import json
import os
import sys
from pathlib import Path

apk, out, manifest, il2cpp, metadata, sdk, toolcheck, nested, unity = sys.argv[1:]
entries = Path(out, "zip_entries.txt").read_text(encoding="utf-8", errors="replace").splitlines()
data = {
    "apk": apk,
    "entry_count": len(entries),
    "manifest": manifest,
    "libil2cpp": il2cpp,
    "global_metadata": metadata,
    "libSDKloading": sdk,
    "libtoolChecker": toolcheck,
    "nested_base_apk": nested,
    "libunity": unity,
    "binge_shell_by_path": [x for x in (sdk, toolcheck, nested) if x],
}
Path(out, "static_identity.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

cat <<EOF
apk=$APK
output=$OUT
entries=$OUT/zip_entries.txt
identity=$OUT/static_identity.json
EOF
