#!/usr/bin/env bash
# ============================================================================
#  fg_mem_probe.sh — 在进程存活期内读 /proc/<pid>/mem 指定 VA（绿梦崩溃点取证）
#  用法: bash 04_scripts/绿梦/fg_mem_probe.sh --wait 85 --off 0x3b96ec0 --len 256
# ============================================================================
set -euo pipefail
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
WAIT=85; OFF=0x3b96ec0; LEN=256
while [ $# -gt 0 ]; do
  case "$1" in
    --wait) WAIT="$2"; shift 2 ;;
    --off) OFF="$2"; shift 2 ;;
    --len) LEN="$2"; shift 2 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done
adb shell am force-stop "$PKG" || true
adb shell am start -n "$PKG/$ACT" >/dev/null
echo "==> 等 ${WAIT}s"; sleep "${WAIT}"
PID=$(adb shell pidof "$PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}' || true)
if [ -z "$PID" ]; then echo "进程已不在（${WAIT}s 时已死）"; exit 1; fi
echo "pid=$PID"
LINE=$(adb shell "su -c \\"grep -m1 'libil2cpp.so' /proc/$PID/maps\\"" 2>/dev/null | tr -d '\r' || true)
if [ -z "$LINE" ]; then
  echo "没找到 libil2cpp 映射，现有 .so 映射："
  adb shell "su -c \\"grep -o '/[^ ]*.so' /proc/$PID/maps\\" | sort -u | head -20" | tr -d '\r'
  exit 1
fi
echo "libil2cpp map: $LINE"
BASE=0x$(echo "$LINE" | cut -d- -f1)
ADDR=$(python3 -c "print(hex($BASE + $OFF))")
echo "base=$BASE off=$OFF -> addr=$ADDR  pid=$PID"
adb shell "su -c \\"dd if=/proc/$PID/mem bs=1 skip=$((ADDR)) count=${LEN} 2>/dev/null | od -An -tx1 -v\\"" > /tmp/memdump.txt
echo "==> 字节:"; cat /tmp/memdump.txt
echo "==> 反汇编:"
python3 - <<'PY'
import capstone
hx=''.join(open('/tmp/memdump.txt').read().split())
b=bytes.fromhex(hx)
md=capstone.Cs(capstone.CS_ARCH_ARM64,capstone.CS_MODE_ARM)
n=0
for i in md.disasm(b,0):
    print('  +%03x %-8s %s' % (i.address,i.mnemonic,i.op_str)); n+=1
    if n>40: break
PY
