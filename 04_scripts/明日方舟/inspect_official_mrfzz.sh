#!/usr/bin/env bash
# 明日方舟：静态检查 TapTap 原版并提取分析所需关键组件。只读 APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT="$NIXIANG_ROOT/06_dumped/明日方舟/official/static_inspect"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，请先跑 fetch_official 阶段" >&2; exit 1; }
APK="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])' "$META_FILE")"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

rm -rf "$OUT"
mkdir -p "$OUT/key_files"
unzip -Z1 "$APK" > "$OUT/zip_entries.txt"
sed -n '1,260p' "$OUT/zip_entries.txt" > "$OUT/zip_entries_head.txt"

pick() {
  local pattern="$1"
  grep -E "$pattern" "$OUT/zip_entries.txt" | sed -n '1p' || true
}

MANIFEST="$(pick '^AndroidManifest\.xml$')"
IL2CPP="$(pick '^lib/arm64-v8a/libil2cpp\.so$')"
UNITY="$(pick '^lib/arm64-v8a/libunity\.so$')"
METADATA="$(pick '^assets/bin/Data/Managed/Metadata/global-metadata\.dat$')"
SDK="$(pick '^lib/arm64-v8a/libSDKloading\.so$')"
TOOLCHECK="$(pick '^lib/arm64-v8a/libtoolChecker\.so$')"
RXPATCH="$(pick '^lib/arm64-v8a/libSRPatch\.so$')"
NESTED="$(pick '^assets/base\.apk$')"

EXTRACT_LIST="$OUT/extract_list.txt"
: > "$EXTRACT_LIST"
for entry in "$MANIFEST" "$IL2CPP" "$UNITY" "$METADATA" "$SDK" "$TOOLCHECK" "$RXPATCH" "$NESTED"; do
  [[ -n "$entry" ]] && printf '%s\n' "$entry" >> "$EXTRACT_LIST"
done
grep -E '^classes[0-9]*\.dex$|^lib/arm64-v8a/.*\.so$|^assets/.*(\.html|\.js|\.json|\.dat|\.bin)$|^META-INF/.*\.(RSA|DSA|EC)$|^shell_.*\.xml$' "$OUT/zip_entries.txt" >> "$EXTRACT_LIST" || true
sort -u "$EXTRACT_LIST" -o "$EXTRACT_LIST"

if [[ -s "$EXTRACT_LIST" ]]; then
  while IFS= read -r entry; do
    [[ -n "$entry" ]] || continue
    unzip -p "$APK" "$entry" > "$OUT/key_files/$(printf '%s' "$entry" | tr '/' '_')"
  done < "$EXTRACT_LIST"
fi

if [[ -n "$MANIFEST" ]]; then
  unzip -p "$APK" "$MANIFEST" > "$OUT/AndroidManifest.bin"
  if command -v apkanalyzer >/dev/null 2>&1; then
    apkanalyzer manifest print "$APK" > "$OUT/AndroidManifest.xml" 2>/dev/null || true
  elif command -v aapt >/dev/null 2>&1; then
    aapt dump xmltree "$APK" "$MANIFEST" > "$OUT/AndroidManifest.xml" 2>/dev/null || true
  fi
fi

python3 - "$APK" "$OUT" "$MANIFEST" "$IL2CPP" "$UNITY" "$METADATA" "$SDK" "$TOOLCHECK" "$RXPATCH" "$NESTED" <<'PY'
import json
import re
import sys
from pathlib import Path

apk, out, manifest, il2cpp, unity, metadata, sdk, toolcheck, rxpatch, nested = sys.argv[1:]
entries = Path(out, "zip_entries.txt").read_text(encoding="utf-8", errors="replace").splitlines()
dex = [x for x in entries if re.fullmatch(r"classes\d*\.dex", x)]
lib64 = [x for x in entries if x.startswith("lib/arm64-v8a/") and x.endswith(".so")]
data = {
    "apk": apk,
    "entry_count": len(entries),
    "manifest": manifest,
    "dex_entries": dex,
    "lib64_entries": lib64,
    "libil2cpp": il2cpp,
    "global_metadata": metadata,
    "libunity": unity,
    "libSDKloading": sdk,
    "libtoolChecker": toolcheck,
    "libSRPatch": rxpatch,
    "nested_base_apk": nested,
    "binge_shell_by_path": [x for x in (sdk, toolcheck, rxpatch, nested) if x],
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
