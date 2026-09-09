#!/usr/bin/env bash
# 快速二分: 固定 APK(清单指向 fc4ca91d), 仅换缓存文件内容
# 用法: routeB_fast.sh <rawfile路径> [标签]
set -uo pipefail
DEV="127.0.0.1:5555"
PKG="com.ztgame.yyzy"
RAW="$1"
TAG="${2:-$(basename "$RAW")}"
MD5=$(python3.11 -c "import hashlib;print(hashlib.md5(open('$RAW','rb').read()).hexdigest())")
CRC=$(python3.11 -c "import zlib;print((zlib.crc32(open('$RAW','rb').read())&0xFFFFFFFF).to_bytes(4,'little').hex())")
SIZE=$(stat -f%z "$RAW")
GUID="fc4ca91d15e97e44097322e9592b1d51"
SB="/sdcard/Android/data/$PKG/files/yoo/DefaultPackage"
D="$SB/CacheRawFiles/${GUID:0:2}/$GUID"

python3.11 -c "
import struct
crc='$CRC'; size=$SIZE
open('/tmp/fast.info','wb').write(struct.pack('<H',8)+crc.encode()+struct.pack('<q',size))
"
adb -s $DEV shell am force-stop $PKG 2>/dev/null
adb -s $DEV push "$RAW" /data/local/tmp/fast.rawfile >/dev/null
adb -s $DEV shell "mkdir -p $D; cp /data/local/tmp/fast.rawfile $D/__data.rawfile; cp /data/local/tmp/fast.info $D/__info; chmod 666 $D/*"
adb -s $DEV logcat -c
adb -s $DEV shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
for t in 12 20 30; do
  sleep $(( t == 12 ? 12 : t - ${prev:-0} )); prev=$t
  PID=$(adb -s $DEV shell pidof $PKG | tr -d '\r')
  [ -z "$PID" ] && break
done
if [ -n "${PID:-}" ]; then
  echo "[$TAG] 存活 pid=$PID ($t 秒)"
  adb -s $DEV logcat -d 2>/dev/null | grep -aE "W/Unity|E/Unity" | grep -acE "404|Failed to download" || true
else
  SIG=$(adb -s $DEV logcat -d 2>/dev/null | grep -a "E CRASH" | grep -a "signal " | head -1)
  PC=$(adb -s $DEV logcat -d 2>/dev/null | grep -a "E CRASH" | grep -aoE "#0[0-9]+ pc [0-9a-f]+" | head -3 | tr '\n' ' ')
  echo "[$TAG] 崩溃: ${SIG:-无tombstone} $PC"
fi
