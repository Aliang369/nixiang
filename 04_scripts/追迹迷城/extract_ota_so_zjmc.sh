#!/usr/bin/env bash
# 追迹迷城：从免广告修复 APK 提取 OTA 使用的 libstarry_magic.so。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="$NIXIANG_ROOT/08_output/追迹迷城/追迹迷城_1.0.20260912_noAdFix-releaseSigned.apk"
OUT_DIR="$NIXIANG_ROOT/08_output/追迹迷城/ota/zjmc"
OUT="$OUT_DIR/libstarry_magic.so"
EXPECTED_MD5="a4d18d81d3230e0758304326eea457c5"

[[ -f "$APK" ]] || { echo "错误: 修复 APK 不存在: $APK" >&2; exit 1; }
if [[ -e "$OUT" ]]; then
  echo "错误: OTA so 已存在，拒绝覆盖: $OUT" >&2
  exit 1
fi

mkdir -p "$OUT_DIR"
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$OUT"
[[ -s "$OUT" ]] || { echo "错误: APK 内未提取到 so" >&2; exit 1; }

MD5="$(md5 -q "$OUT")"
[[ "$MD5" == "$EXPECTED_MD5" ]] || { echo "错误: OTA so MD5 不一致: $MD5" >&2; exit 1; }
LC_ALL=C grep -a -q '\[Zjmc\]' "$OUT" || { echo "错误: OTA so 缺少 [Zjmc]" >&2; exit 1; }
LC_ALL=C grep -a -q 'zjmc' "$OUT" || { echo "错误: OTA so 缺少 zjmc" >&2; exit 1; }
LC_ALL=C grep -a -q 'AT.GetFreeSkipAdsUsed' "$OUT" || { echo "错误: 缺少 GetFreeSkipAdsUsed Hook 标签" >&2; exit 1; }
LC_ALL=C grep -a -q 'AT.UseFreeSkipAds' "$OUT" || { echo "错误: 缺少 UseFreeSkipAds Hook 标签" >&2; exit 1; }
LC_ALL=C grep -a -q 'BEGIN CERTIFICATE' "$OUT" || { echo "错误: 缺少固定证书" >&2; exit 1; }

printf '%s  %s\n' "$MD5" "libstarry_magic.so" > "$OUT_DIR/libstarry_magic.so.md5"
shasum -a 256 "$OUT" | awk '{print $1"  libstarry_magic.so"}' > "$OUT_DIR/libstarry_magic.so.sha256"
cat > "$OUT_DIR/ota_meta.json" <<EOF
{
  "game": "追迹迷城",
  "game_id": "zjmc",
  "version": "1.0.20260912",
  "fix": "noAdFix",
  "source_apk": "$APK",
  "so_md5": "$MD5",
  "so_sha256": "$(shasum -a 256 "$OUT" | awk '{print $1}')",
  "so_size": $(stat -f '%z' "$OUT")
}
EOF

cat "$OUT_DIR/ota_meta.json"
echo "ota_so=$OUT"
