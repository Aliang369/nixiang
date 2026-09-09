#!/usr/bin/env bash
# 路线 B 变体测试: 注入缓存+沙箱清单 -> 冷启动 -> 观测
# 用法: routeB_test.sh <variant>
set -euo pipefail
V="$1"
DEV="127.0.0.1:5555"
PKG="com.ztgame.yyzy"
SB="/sdcard/Android/data/$PKG/files/yoo/DefaultPackage"
OUT="/Users/aliang/Documents/nixiang/07_patched/月圆之夜/routeB"

MD5=$(python3.11 -c "import hashlib;print(hashlib.md5(open('$OUT/$V.rawfile','rb').read()).hexdigest())")
CRC=$(python3.11 -c "import zlib;print((zlib.crc32(open('$OUT/$V.rawfile','rb').read())&0xFFFFFFFF).to_bytes(4,'little').hex())")
SIZE=$(stat -f%z "$OUT/$V.rawfile")
echo "== variant=$V md5=$MD5 crc=$CRC size=$SIZE"

adb -s $DEV shell am force-stop $PKG 2>/dev/null || true
adb -s $DEV shell "rm -rf $SB/CacheRawFiles/*"
D="$SB/CacheRawFiles/${MD5:0:2}/$MD5"
adb -s $DEV shell "mkdir -p $D"
adb -s $DEV push "$OUT/$V.rawfile" /data/local/tmp/t.rawfile >/dev/null
adb -s $DEV push "$OUT/$V.info" /data/local/tmp/t.info >/dev/null
adb -s $DEV shell "cp /data/local/tmp/t.rawfile $D/__data.rawfile; cp /data/local/tmp/t.info $D/__info; chmod 666 $D/*"
# 沙箱清单覆盖
adb -s $DEV push "$OUT/$V.manifest.bytes" /data/local/tmp/t.mf >/dev/null
adb -s $DEV push "$OUT/$V.manifest.hash" /data/local/tmp/t.mfh >/dev/null
adb -s $DEV shell "cp /data/local/tmp/t.mf $SB/ManifestFiles/PackageManifest_DefaultPackage_1.0.055.bytes; cp /data/local/tmp/t.mfh $SB/ManifestFiles/PackageManifest_DefaultPackage_1.0.055.hash; chmod 666 $SB/ManifestFiles/*"
echo "== 注入完成, 冷启动..."

adb -s $DEV logcat -c
adb -s $DEV shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
sleep 35
PID=$(adb -s $DEV shell pidof $PKG | tr -d '\r')
if [ -n "$PID" ]; then
  echo "== 结果: 进程存活 pid=$PID (热更装载通过)"
  adb -s $DEV logcat -d 2>/dev/null | grep -aE "W/Unity|E/Unity" | grep -aiE "download|404|Failed" | head -5
else
  echo "== 结果: 进程死亡"
  adb -s $DEV logcat -d 2>/dev/null | grep -aE "E CRASH" | grep -aE "signal |#0[0-9] pc|#1[0-2] pc" | head -12
  adb -s $DEV logcat -d 2>/dev/null | grep -aE "Force|has died" | grep -a yyzy | head -3
fi
