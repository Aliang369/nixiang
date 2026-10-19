#!/usr/bin/env bash
# ============================================================================
#  surgical_equal_length_resign.sh — 绿梦：等长重签包（单变量实验载体）
#
#  目的：造一个与官方 base.apk **同长**（179,649,718 B）、内容逐字节不变、
#        只把「APK 签名块」换成星光证书的包，用来分离「尺寸 vs 签名」变量。
#
#  做法（外科，不重打包）：官方包结构 = [内容 0..0xAB3BFF0][签名块 4096][CD 0x16CA0][EOCD 22]
#   1) 取官方前段内容原样保留；
#   2) 丢掉官方签名块，接上官方 CD + EOCD（cd_off 改指 sigblock_start）；
#   3) apksigner 重签（v2+v3）→ 新签名块插在同一位置；
#   4) 若总长与官方不等，用 EOCD 注释（zip comment）补齐差额（≤65535），重签再测。
#  只改两处：签名块、EOCD 注释。条目/本地头/CD 全部原字节。
#
#  用法： bash 04_scripts/绿梦/surgical_equal_length_resign.sh [--src 官方包] [--out 输出包]
#  退出码：0=产出且长度精确相等；1=长度对不上或校验失败；2=用法/环境错误
# ============================================================================
set -euo pipefail

ROOT="${NIXIANG_ROOT:-$(cd "$(dirname "$0")/../.." && pwd -P)}"
SRC="/tmp/lvmeng_apk/base.apk"
OUT="$ROOT/07_patched/绿梦/fg_test/绿梦_equallen_resign.apk"
TARGET=179649718
# v1 用 SHA-1 摘要（=minSdk<18）才能让内容落在目标 4K 对齐窗内；v2 仍为 SHA-256。
MIN_SDK=17
KS="$ROOT/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
APKSIGNER="$BT/apksigner"

while [ $# -gt 0 ]; do
  case "$1" in
    --src) SRC="$2"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --no-pad) NOPAD=1; shift ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -f "$SRC" ] || { echo "缺官方包: $SRC" >&2; exit 2; }
[ -f "$KS" ] || { echo "缺 keystore: $KS" >&2; exit 2; }
[ -x "$APKSIGNER" ] || { echo "缺 apksigner: $APKSIGNER" >&2; exit 2; }
mkdir -p "$(dirname "$OUT")"

UNSIGNED="$(dirname "$OUT")/.equallen_unsigned.apk"
TMP="$(dirname "$OUT")/.equallen_signed.apk"

build_unsigned() {
  local comment_len="$1"
  python3 - "$SRC" "$UNSIGNED" "$comment_len" <<'PY'
import struct, sys
src, dst, clen = sys.argv[1], sys.argv[2], int(sys.argv[3])
data = open(src, 'rb').read()
eocd = data.rfind(b'PK\x05\x06')
assert eocd > 0, 'no EOCD'
cd_size, cd_off, comment_len = struct.unpack_from('<IIH', data, eocd + 12)
assert data[cd_off-16:cd_off] == b'APK Sig Block 42', 'no signing block magic'
blk_sz = struct.unpack_from('<Q', data, cd_off - 24)[0]
start = cd_off - blk_sz - 8
content = data[:start]
cd = data[cd_off:cd_off + cd_size]
eocd_fixed = bytearray(data[eocd:eocd + 22])
struct.pack_into('<I', eocd_fixed, 16, start)
struct.pack_into('<H', eocd_fixed, 20, clen)
out = content + cd + bytes(eocd_fixed) + (b'\x00' * clen)
open(dst, 'wb').write(out)
print('unsigned=%d content=%d cd=%d' % (len(out), len(content), cd_size))
PY
}

sign() {
  "$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled false \
    --min-sdk-version "$MIN_SDK" \
    --out "$TMP" "$UNSIGNED" >/dev/null
}

NOPAD="${NOPAD:-0}"
if [ "$NOPAD" = "1" ]; then
  build_unsigned 0
  sign
  cp "$TMP" "$OUT"
  if "$APKSIGNER" verify "$OUT" >/dev/null; then echo "   apksigner verify: OK"; fi
  echo "   (no-pad) md5: $(md5 -q "$OUT")   size: $(stat -f%z "$OUT")"
  rm -f "$UNSIGNED" "$TMP"
  exit 0
fi

CLEN=0
for i in 1 2 3; do
  build_unsigned "$CLEN"
  sign
  SIZE=$(stat -f%z "$TMP")
  echo "==> 尝试 $i: comment=$CLEN  signed=$SIZE  target=$TARGET  diff=$((TARGET-SIZE))"
  if [ "$SIZE" -eq "$TARGET" ]; then
    cp "$TMP" "$OUT"
    if "$APKSIGNER" verify "$OUT" >/dev/null; then echo "   apksigner verify: OK"; fi
    echo "   md5: $(md5 -q "$OUT")   size: $(stat -f%z "$OUT")"
    rm -f "$UNSIGNED" "$TMP"
    exit 0
  fi
  D=$((TARGET - SIZE))
  if [ "$D" -le 0 ] || [ "$D" -gt 65535 ]; then
    echo "   差额不可用 comment 补齐（${D}）" >&2
    exit 1
  fi
  CLEN=$((CLEN + D))
done
echo "3 次未收敛" >&2
exit 1
