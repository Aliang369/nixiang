#!/usr/bin/env bash
#  fg_mem_probe2.sh — 冷启后在存活期内抓 libil2cpp 崩溃点字节 + 记录死亡时刻
set -uo pipefail
PKG="com.zhuziplay.lvmengnew.op"
ACT="com.bamboo.game.SplashActivity"
OFF=0x3b96ec0; LEN=192
adb shell am force-stop "$PKG" >/dev/null 2>&1 || true
sleep 3
adb logcat -c >/dev/null 2>&1 || true
adb shell am start -n "$PKG/$ACT" >/dev/null 2>&1
T0=$(date +%s)
DUMPED=0
for i in $(seq 1 30); do
  sleep 4
  NOW=$(( $(date +%s) - T0 ))
  PID=$(adb shell pidof "$PKG" 2>/dev/null | tr -d '\r' | awk '{print $1}')
  if [ -z "$PID" ]; then echo "[$NOW s] process gone"; break; fi
  echo "[$NOW s] pid=$PID alive"
  if [ "$DUMPED" = "0" ] && [ "$NOW" -ge 56 ]; then
    LINE=$(adb shell "su -c \"grep -m1 libil2cpp.so /proc/$PID/maps\"" 2>/dev/null | tr -d '\r')
    echo "    map: $LINE"
    if [ -n "$LINE" ]; then
      BASE=0x$(echo "$LINE" | cut -d- -f1)
      ADDR=$(python3 -c "print(hex($BASE + $OFF))")
      echo "    addr: $ADDR"
      adb shell "su -c \"dd if=/proc/$PID/mem bs=1 skip=$((ADDR)) count=$LEN 2>/dev/null | od -An -tx1 -v\"" > /tmp/memdump.txt
      echo "    bytes: $(head -c 600 /tmp/memdump.txt | tr '\n' ' ')"
      DUMPED=1
    fi
  fi
done
echo "== death log =="
adb logcat -d -v time ActivityManager:I libc:I DEBUG:I '*:S' 2>/dev/null | grep -E "lvmengnew.*(died|exited)|Fatal signal|exited due to signal 4" | head -6
echo "== disasm =="
python3 - <<'PY'
import capstone,os
p='/tmp/memdump.txt'
if not os.path.exists(p): raise SystemExit('no dump')
hx=''.join(open(p).read().split())
if len(hx)<8: raise SystemExit('empty dump')
b=bytes.fromhex(hx)
md=capstone.Cs(capstone.CS_ARCH_ARM64,capstone.CS_MODE_ARM)
n=0
for i in md.disasm(b,0):
    print('  +%03x %-8s %s' % (i.address,i.mnemonic,i.op_str)); n+=1
    if n>36: break
PY
