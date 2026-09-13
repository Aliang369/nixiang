#!/usr/bin/env bash
# 只读提取《大话蜀山》斌哥瘦壳落盘的已 Hook libil2cpp.so。
# 不安装、不清数据、不修改设备；只使用 debuggable 包的 run-as 读取私有目录。
set -euo pipefail

if [[ $# -ne 3 ]]; then
  echo "用法: $0 <serial> <package> <output.so>" >&2
  exit 1
fi

SERIAL="$1"
PKG="$2"
OUT="$3"
TMP="${OUT}.part"

if [[ -e "$OUT" ]]; then
  echo "错误: 目标 SO 已存在，拒绝覆盖: $OUT" >&2
  exit 1
fi

if ! adb -s "$SERIAL" get-state >/dev/null 2>&1; then
  echo "错误: 设备不可用: $SERIAL" >&2
  exit 1
fi

REMOTE_HASH="$(adb -s "$SERIAL" shell run-as "$PKG" sha256sum files/libil2cpp.so | tr -d '\r' | awk '{print $1}')"
[[ -n "$REMOTE_HASH" ]] || { echo "错误: 无法读取设备私有 libil2cpp.so 哈希" >&2; exit 1; }

mkdir -p "$(dirname "$OUT")"
rm -f "$TMP"
trap 'rm -f "$TMP"' EXIT

adb -s "$SERIAL" exec-out run-as "$PKG" cat files/libil2cpp.so > "$TMP"
[[ -s "$TMP" ]] || { echo "错误: 提取到空 SO" >&2; exit 1; }
LOCAL_HASH="$(shasum -a 256 "$TMP" | awk '{print $1}')"
[[ "$LOCAL_HASH" = "$REMOTE_HASH" ]] || {
  echo "错误: 提取哈希不一致 remote=$REMOTE_HASH local=$LOCAL_HASH" >&2
  exit 1
}
mv "$TMP" "$OUT"

echo "serial=$SERIAL"
echo "package=$PKG"
echo "remote=files/libil2cpp.so"
echo "output=$OUT"
echo "size=$(stat -f '%z' "$OUT")"
echo "sha256=$LOCAL_HASH"
echo "md5=$(md5 -q "$OUT")"
