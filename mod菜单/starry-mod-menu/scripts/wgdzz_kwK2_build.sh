#!/usr/bin/env bash
# 王国大作战 B 路线：KwProtect K1(sigkill) + K2(exit stub) + 外科 trace 包
#
# 用法:
#   STARRY_TRACE=1 STARRY_INSTALL=1 WGDZZ_SO_DELAY_MS=1000 bash scripts/wgdzz_kwK2_build.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
PATCH_PY="$NIXIANG/work/wgdzz_b_route/kwprotect_k2_patch.py"
K1="$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_sigkill_k1.so"
K2="$NIXIANG/07_patched/王国大作战前线/libKwProtectSDK_exit_k2.so"
OUT="$NIXIANG/08_output/王国大作战前线"
DELAY="${WGDZZ_SO_DELAY_MS:-1000}"

[[ -f "$K1" ]] || {
  echo "ERROR: missing K1 so $K1 (先完成 KwProtect sigkill K1 patch)"
  exit 1
}
[[ -f "$PATCH_PY" ]] || { echo "ERROR: missing $PATCH_PY"; exit 1; }

python3.11 "$PATCH_PY" -i "$K1" -o "$K2"

export WGDZZ_KWPROTECT_SO="$K2"
export WGDZZ_SO_DELAY_MS="$DELAY"
STARRY_TRACE="${STARRY_TRACE:-1}" STARRY_INSTALL="${STARRY_INSTALL:-0}" \
  bash "$SCRIPT_DIR/surgical_wgdzz_1.1.7646.sh"

UNSIGNED=$(ls -t /tmp/wgdzz_surgical.*/unsigned.apk 2>/dev/null | head -1 || true)
if [[ -z "${UNSIGNED:-}" ]]; then
  echo "WARN: tmp unsigned not found; skip kwK2 rename"
  exit 0
fi

# 重命名最终产物（surgical 脚本 TAG=trace → surgical-traceSigned.apk）
TRACE_APK="$OUT/王国大作战前线_1.1.7646_surgical-traceSigned.apk"
KW_APK="$OUT/王国大作战前线_1.1.7646_surgical-kwK2-${DELAY}msSigned.apk"
if [[ -f "$TRACE_APK" ]]; then
  cp -f "$TRACE_APK" "$KW_APK"
  echo "KW K2 apk: $KW_APK"
fi
