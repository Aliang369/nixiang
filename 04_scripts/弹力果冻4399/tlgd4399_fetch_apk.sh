#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399）— 真机只读提取底包
#
# 为什么用「真机提取」而不是官方 4399 包：
#   用户 2026-10-07 明确指定 —— 真机上现装的就是 4399 星光包，versionCode=4412 与官方 2.4.0 一致，
#   其 classes.dex / resources.arsc 是官方原始字节；本渠道后续的 so 外科流程会整体置换
#   libstarry_magic.so + classes2.dex（+ assets/certs）并重签名，等价于从官方包外科重建星光包。
#   好处：与玩家已经在用的版本字节同源，覆盖安装最稳。
#
# 用法: bash 04_scripts/弹力果冻4399/tlgd4399_fetch_apk.sh
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.ginicat.sweet.m4399"
OUT_DIR="$NIXIANG/02_apks/弹力果冻4399"
OUT="$OUT_DIR/弹力果冻4399_2.4.0_vc4412_device_base.apk"
SIZE_EXPECT="${TLGD4399_SIZE_EXPECT:-2140909132}"
META="$NIXIANG/04_scripts/弹力果冻4399/fetched_apk.json"

command -v adb >/dev/null || { echo "ERROR: adb not found"; exit 1; }
mkdir -p "$OUT_DIR"

# ⚠️ set -o pipefail 下 `cmd | head -1` 会因 SIGPIPE 让整条管线返回 141 并中止脚本（同 install.sh 踩坑），故加 `|| true`
BASE="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | grep 'base\.apk' | head -1 | sed 's/^package://' || true)"
[[ -n "$BASE" ]] || { echo "ERROR: $PKG 未安装或取不到 base.apk（pm path 空）"; exit 1; }
echo "[fetch] 设备 base.apk = $BASE"

# 只读提取（不 root、不清理、不改设备）
adb -s "$SERIAL" exec-out cat "$BASE" > "$OUT"
ACTUAL="$(stat -f%z "$OUT")"
echo "[fetch] 已落地 $OUT ($ACTUAL B)"
if [[ -n "$SIZE_EXPECT" && "$ACTUAL" != "$SIZE_EXPECT" ]]; then
  echo "ERROR: 底包大小 $ACTUAL != 期望 $SIZE_EXPECT —— 设备上的包换了版本，禁止继续" >&2
  rm -f "$OUT"
  exit 1
fi

MD5="$(md5 -q "$OUT")"; SHA="$(shasum -a 256 "$OUT" | awk '{print $1}')"
cat > "$META" <<JSON
{
  "package": "$PKG",
  "serial": "$SERIAL",
  "device_base_path": "$BASE",
  "local_path": "$OUT",
  "size": $ACTUAL,
  "md5": "$MD5",
  "sha256": "$SHA",
  "source": "真机只读提取（用户 2026-10-07 授权：以已装 4399 星光包为底包）",
  "note": "versionCode=4412 与官方 4399 2.4.0 一致；classes.dex/resources.arsc 为官方原始字节"
}
JSON
rm -f "$OUT.idsig"
echo "[fetch] md5=$MD5"
echo "[fetch] meta → $META"
