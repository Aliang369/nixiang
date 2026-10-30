#!/bin/bash
# 失落城堡2 —— 分析专用包（frida-gadget 内嵌），不产出交付物
# 用途：进程内加载 libdcmplugin.so，用 JS 精确 hook + Thread.backtrace 取证；
#      不存在 frida-server 进程/端口特征，避开「扫 server」类检测。
# 用法：bash 04_scripts/失落城堡2/gadget_analysis_apk.sh [端口，默认 39123]
set -euo pipefail
NIXIANG="${NIXIANG:-$(cd "$(dirname "$0")/../.." && pwd)}"
PORT="${1:-39123}"
SRC_APK="$NIXIANG/08_output/失落城堡2/失落城堡2_1.0.3_surgical-releaseSigned.apk"
GADGET="$NIXIANG/07_tools/frida_gadget/gadget.so"
OUTDIR="$NIXIANG/09_test/失落城堡2/gadget_pack"
WORK="$OUTDIR/work"
BT="${NIXIANG_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/35.0.0}"
KS="$NIXIANG/01_tools/starry_release.jks"
ALIAS="${STARRY_KS_ALIAS:-starry}"; KPASS="${STARRY_KS_PASS:-starry2026}"
OUT="$OUTDIR/失落城堡2_1.0.3_gadget.apk"

[[ -f "$SRC_APK" ]] || { echo "ERROR: 缺 $SRC_APK"; exit 1; }
[[ -f "$GADGET"   ]] || { echo "ERROR: 缺 $GADGET"; exit 1; }
[[ -d "$BT"       ]] || { echo "ERROR: 找不到 build-tools: $BT"; exit 1; }
mkdir -p "$WORK"; rm -f "$WORK"/*.apk

echo "[1/4] 准备 gadget + 配置（端口 ${PORT}）"
cp -f "$GADGET" "$WORK/libdcmplugin.so"
printf '{"interaction":{"type":"listen","address":"127.0.0.1","port":%s},"log":"info"}' "$PORT" > "$WORK/libdcmplugin.config.so"

echo "[2/4] 并入 APK lib/arm64-v8a（不压缩）"
cp -f "$SRC_APK" "$WORK/merged.apk"
rm -rf "$WORK/add"; mkdir -p "$WORK/add/lib/arm64-v8a"
cp -f "$WORK/libdcmplugin.so" "$WORK/libdcmplugin.config.so" "$WORK/add/lib/arm64-v8a/"
( cd "$WORK/add" && zip -q -0 "$WORK/merged.apk" lib/arm64-v8a/libdcmplugin.so lib/arm64-v8a/libdcmplugin.config.so )

echo "[3/4] zipalign + 重新签名"
"$BT/zipalign" -f -p 4 "$WORK/merged.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$ALIAS" \
  --ks-pass "pass:$KPASS" --key-pass "pass:$KPASS" --out "$OUT" "$WORK/aligned.apk"

echo "[4/4] 校验"
unzip -l "$OUT" | grep -E "libgadget" || true
echo "[done] $OUT  (端口 ${PORT})"
echo "装机：adb install -r '$OUT'"
echo "连接：adb forward tcp:${PORT} tcp:${PORT} && frida -H 127.0.0.1:${PORT} -n Gadget -q"
