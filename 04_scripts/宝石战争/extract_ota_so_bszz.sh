#!/usr/bin/env bash
# 宝石战争：从正式 APK 提取后台 OTA 使用的 libstarry_magic.so。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="${BSZZ_OTA_SOURCE_APK:-$NIXIANG_ROOT/08_output/宝石战争/宝石战争_6.0.012_surgical-releaseSigned.apk}"
OUT_DIR="$NIXIANG_ROOT/08_output/宝石战争/ota/bszz"
OUT="$OUT_DIR/libstarry_magic.so"
EXPECTED_APK_MD5="${BSZZ_OTA_APK_MD5:-746a21fb844654100df9dffabbd11326}"
EXPECTED_SO_MD5="${BSZZ_OTA_SO_MD5:-2fc2873da7f268866502ce0feaaefa5c}"

[[ -f "$APK" ]] || { echo "错误: 正式 APK 不存在: $APK" >&2; exit 1; }
[[ "$(md5 -q "$APK")" == "$EXPECTED_APK_MD5" ]] || {
  echo "错误: 正式 APK 指纹不一致: $APK" >&2
  exit 1
}
if [[ -e "$OUT" ]]; then
  EXISTING_SO_MD5="$(md5 -q "$OUT")"
  [[ "$EXISTING_SO_MD5" == "$EXPECTED_SO_MD5" ]] || {
    echo "错误: OTA so 已存在且指纹异常，拒绝覆盖: $OUT" >&2
    exit 1
  }
fi

mkdir -p "$OUT_DIR"
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$OUT"
[[ -s "$OUT" ]] || { echo "错误: APK 内未提取到 libstarry_magic.so" >&2; exit 1; }

SO_MD5="$(md5 -q "$OUT")"
SO_SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
SO_SIZE="$(stat -f '%z' "$OUT")"
[[ "$SO_MD5" == "$EXPECTED_SO_MD5" ]] || {
  echo "错误: OTA so MD5 不一致: $SO_MD5" >&2
  exit 1
}
LC_ALL=C grep -aFq '[Bszz]' "$OUT" || { echo "错误: OTA so 缺少 [Bszz]" >&2; exit 1; }
LC_ALL=C grep -aFq 'bszz' "$OUT" || { echo "错误: OTA so 缺少 bszz" >&2; exit 1; }
LC_ALL=C grep -aFq 'BEGIN CERTIFICATE' "$OUT" || { echo "错误: OTA so 缺少固定证书" >&2; exit 1; }

printf '%s  %s\n' "$SO_MD5" "libstarry_magic.so" > "$OUT_DIR/libstarry_magic.so.md5"
printf '%s  %s\n' "$SO_SHA256" "libstarry_magic.so" > "$OUT_DIR/libstarry_magic.so.sha256"
python3 - "$OUT_DIR/ota_meta.json" "$APK" "$SO_MD5" "$SO_SHA256" "$SO_SIZE" "$EXPECTED_APK_MD5" <<'PY'
import json
import sys
from pathlib import Path

meta_path, apk, so_md5, so_sha256, so_size, source_apk_md5 = sys.argv[1:]
data = {
    "game": "宝石战争",
    "game_id": "bszz",
    "version": "6.0.012",
    "fix": "unlimited_skill_full_mana",
    "source_apk": apk,
    "source_apk_md5": source_apk_md5,
    "so_md5": so_md5,
    "so_sha256": so_sha256,
    "so_size": int(so_size),
}
Path(meta_path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$OUT_DIR/ota_meta.json"
echo "ota_so=$OUT"
