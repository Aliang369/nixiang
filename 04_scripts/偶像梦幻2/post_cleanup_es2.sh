#!/usr/bin/env bash
# 偶像梦幻祭2 4.5.8861 归档后清理：仅删除静态大件，保留小文本与用户管理的 08_output。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="偶像梦幻2"
VERSION="4.5.8861"
STATIC="$NIXIANG/06_dumped/$GAME/official_${VERSION}/static"
FINAL="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SUMMARY="$NIXIANG/04_scripts/$GAME/post_cleanup_summary.json"
EXPECTED_MD5="29598948a08e234b83cca24a4a7fbd1c"

[[ -f "$FINAL" ]] || { echo "ERROR: missing final APK" >&2; exit 1; }
[[ "$(md5 -q "$FINAL")" == "$EXPECTED_MD5" ]] || { echo "ERROR: final APK md5 mismatch" >&2; exit 1; }
[[ -d "$STATIC" ]] || { echo "ERROR: missing static input directory" >&2; exit 1; }

size_of_path() {
  local p="$1"
  if [[ -e "$p" ]]; then
    du -sk "$p" | awk '{print $1 * 1024}'
  else
    echo 0
  fi
}

TARGETS=(
  "$STATIC/libil2cpp.so"
  "$STATIC/libunity.so"
  "$STATIC/libxlua.so"
  "$STATIC/global-metadata.dat"
  "$STATIC/dex"
)

FREED_BYTES=0
for p in "${TARGETS[@]}"; do
  size="$(size_of_path "$p")"
  FREED_BYTES=$((FREED_BYTES + size))
done

for p in "${TARGETS[@]}"; do
  case "$p" in
    "$STATIC/libil2cpp.so"|\
    "$STATIC/libunity.so"|\
    "$STATIC/libxlua.so"|\
    "$STATIC/global-metadata.dat"|\
    "$STATIC/dex")
      rm -rf -- "$p"
      ;;
    *)
      echo "ERROR: unsafe post-archive cleanup target: $p" >&2
      exit 1
      ;;
  esac
done

[[ -f "$STATIC/dump_inputs.json" ]] || { echo "ERROR: dump_inputs.json was not preserved" >&2; exit 1; }
[[ -f "$STATIC/entries.txt" ]] || { echo "ERROR: entries.txt was not preserved" >&2; exit 1; }
[[ -f "$STATIC/AndroidManifest.xml" ]] || { echo "ERROR: AndroidManifest.xml was not preserved" >&2; exit 1; }

python3 - "$SUMMARY" "$FINAL" "$EXPECTED_MD5" "$STATIC" "$FREED_BYTES" <<'PY'
import json, sys
from pathlib import Path
(out, final, md5, static, freed) = sys.argv[1:]
data = {
    "game": "偶像梦幻2",
    "version": "4.5.8861",
    "status": "post_archive_cleanup_done",
    "final_apk": final,
    "final_apk_md5": md5,
    "static_dir": static,
    "estimated_freed_bytes": int(freed),
    "preserved_text_inputs": [
        "dump_inputs.json",
        "entries.txt",
        "AndroidManifest.xml",
        "resources_arsc_zipinfo.txt",
    ],
    "output_policy": "08_output untouched (user-managed)",
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "[post-cleanup] estimated_freed_bytes=$FREED_BYTES"
echo "[post-cleanup] summary=$SUMMARY"
