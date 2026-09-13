#!/usr/bin/env bash
# 对比斌哥改包与 TapTap 官方包的包身份、签名、游戏 so 和 metadata。
set -euo pipefail

if [[ $# -ne 2 ]]; then
  echo "用法: $0 <binge.apk> <official.apk>" >&2
  exit 1
fi

BINGE="$1"
OFFICIAL="$2"
AAPT2="${AAPT2:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner}"
REPORT_DIR="/Users/aliang/Documents/nixiang/06_dumped/我的修仙世界"
REPORT="$REPORT_DIR/official_compare_4.5.txt"

[[ -f "$BINGE" ]] || { echo "斌哥 APK 不存在: $BINGE" >&2; exit 1; }
[[ -f "$OFFICIAL" ]] || { echo "官方 APK 不存在: $OFFICIAL" >&2; exit 1; }
mkdir -p "$REPORT_DIR"

hash_entry() {
  local apk="$1" entry="$2"
  if unzip -Z1 "$apk" | LC_ALL=C grep -Fx "$entry" >/dev/null; then
    unzip -p "$apk" "$entry" | shasum -a 256 | awk '{print $1}'
  else
    echo "MISSING"
  fi
}

dex_hash_list() {
  local apk="$1"
  unzip -Z1 "$apk" | grep -E '^classes[0-9]*\.dex$' | sort | while read -r entry; do
    printf '%s ' "$entry"
    unzip -p "$apk" "$entry" | shasum -a 256 | awk '{print $1}'
  done
}

{
  echo "# 斌哥包 vs TapTap 官方包核对 — 4.5"
  echo
  echo "binge=$BINGE"
  echo "official=$OFFICIAL"
  echo "binge_size=$(stat -f '%z' "$BINGE")"
  echo "official_size=$(stat -f '%z' "$OFFICIAL")"
  echo "binge_md5=$(md5 -q "$BINGE")"
  echo "official_md5=$(md5 -q "$OFFICIAL")"
  echo
  echo "## badging"
  echo "--- binge"
  "$AAPT2" dump badging "$BINGE" | sed -n '1,12p'
  echo "--- official"
  "$AAPT2" dump badging "$OFFICIAL" | sed -n '1,12p'
  echo
  echo "## certs"
  echo "--- binge"
  "$APKSIGNER" verify --print-certs "$BINGE" | sed -n '1,12p'
  echo "--- official"
  "$APKSIGNER" verify --print-certs "$OFFICIAL" | sed -n '1,12p'
  echo
  echo "## game payload hashes"
  for entry in \
    lib/arm64-v8a/libil2cpp.so \
    assets/bin/Data/Managed/Metadata/global-metadata.dat \
    lib/arm64-v8a/libunity.so \
    lib/arm64-v8a/libSDKloading.so; do
    echo "[$entry]"
    echo "binge=$(hash_entry "$BINGE" "$entry")"
    echo "official=$(hash_entry "$OFFICIAL" "$entry")"
  done
  echo
  echo "## dex hashes"
  echo "--- binge"
  dex_hash_list "$BINGE"
  echo "--- official"
  dex_hash_list "$OFFICIAL"
} | tee "$REPORT"

OFFICIAL_BADGING="$("$AAPT2" dump badging "$OFFICIAL" | sed -n '1p')"
case "$OFFICIAL_BADGING" in
  *"package: name='com.lingsi.xiuxiandanji' versionCode='45' versionName='4.5'"*) ;;
  *) echo "错误: 官方包身份不是 com.lingsi.xiuxiandanji 4.5/vc45" >&2; exit 1 ;;
esac

echo "报告已写入: $REPORT"
echo "注意: libil2cpp/metadata 哈希不同不等于失败，后续以官方包重新锚定。"
