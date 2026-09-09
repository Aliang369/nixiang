#!/usr/bin/env bash
# 干净矩阵测试 v3: 安装 -> 首启等到沙箱就绪 -> force-stop -> 注入(属主对齐) -> 二启观测
# 用法: routeB_matrix.sh <rawfile> <crc_hex8> <size> [标签]
set -uo pipefail
DEV="127.0.0.1:5555"
PKG="com.ztgame.yyzy"
GUID="fc4ca91d15e97e44097322e9592b1d51"
CRC="$2"; SIZE="$3"; TAG="${4:-test}"
SB="/sdcard/Android/data/$PKG/files/yoo"
D="$SB/DefaultPackage/CacheRawFiles/${GUID:0:2}/$GUID"

python3.11 -c "
import struct
open('/tmp/m.info','wb').write(struct.pack('<H',8)+'$CRC'.encode()+struct.pack('<q',$SIZE))
"
adb -s $DEV uninstall $PKG >/dev/null 2>&1
adb -s $DEV install "/Users/aliang/Documents/nixiang/08_output/月圆之夜/月圆之夜_1.6.30.1_base_routeA_unsigned-aligned-debugSigned.apk" >/dev/null 2>&1 || { echo "[$TAG] 安装失败"; exit 1; }

# 首启: 等待游戏自建 yoo 沙箱 (轮询, 最多 150s)
adb -s $DEV logcat -c
adb -s $DEV shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
READY=0
for i in $(seq 1 30); do
  sleep 5
  if adb -s $DEV shell "ls $SB/DefaultPackage/ManifestFiles >/dev/null 2>&1"; then READY=1; break; fi
done
if [ "$READY" != "1" ]; then echo "[$TAG] 首启未就绪(沙箱未建)"; adb -s $DEV shell am force-stop $PKG; exit 1; fi
sleep 8
adb -s $DEV shell am force-stop $PKG
sleep 2

# 注入: 属主正确解析 ($3=user $4=group)
OWNER=$(adb -s $DEV shell "ls -ld $SB" | awk '{print $3":"$4}' | tr -d '\r')
adb -s $DEV push "$1" /data/local/tmp/m.rawfile >/dev/null
adb -s $DEV shell "mkdir -p $D; cp /data/local/tmp/m.rawfile $D/__data.rawfile; cp /data/local/tmp/m.info $D/__info; chown -R $OWNER $SB; chmod -R 660 $D/__data.rawfile $D/__info; chmod 770 $D ${D%/*} ${D%/*/*}"
echo "[$TAG] 注入完成 owner=$OWNER"

# 二启观测
adb -s $DEV logcat -c
adb -s $DEV shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
PID=""
LAST=0
for i in 1 2 3 4 5 6; do
  sleep 12
  LAST=$i
  PID=$(adb -s $DEV shell pidof $PKG | tr -d '\r')
  [ -z "$PID" ] && break
  ACC=$(adb -s $DEV logcat -d 2>/dev/null | grep -acE "UnauthorizedAccessException" || true)
  NET=$(adb -s $DEV logcat -d 2>/dev/null | grep -acE "GetUrl url|ProtoServer url" || true)
  [ "$NET" -ge 3 ] && break
done
if [ -z "$PID" ]; then
  PC=$(adb -s $DEV logcat -d 2>/dev/null | grep -a "E CRASH" | grep -aoE "pc 0000000001[0-9a-f]{7}" | head -4 | tr '\n' ' ')
  echo "[$TAG] 崩溃: $PC"
else
  ACC=$(adb -s $DEV logcat -d 2>/dev/null | grep -acE "UnauthorizedAccessException" || true)
  NET=$(adb -s $DEV logcat -d 2>/dev/null | grep -acE "GetUrl url|ProtoServer url" || true)
  echo "[$TAG] 存活($((LAST*12))s) 权限异常=$ACC 网络标记=$NET"
fi
