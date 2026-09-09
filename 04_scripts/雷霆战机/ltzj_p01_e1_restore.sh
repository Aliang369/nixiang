#!/system/bin/sh
# E1：翻字节 → 冷启 → checker 首读前恢复 → 判定 checker 是否按当前文件状态逐次判定
APP=/data/app/~~jtROrqKbEKNyRQfq6-ZBew==/com.a5game.ltzj.jj-to5UV9cd82IU3c29CbPoGw==
LIB=$APP/lib/arm64/libcocos.so
BK=/data/local/tmp/libcocos_backup.so

am force-stop com.a5game.ltzj.jj 2>/dev/null
sleep 1

SZ=$(stat -c %s $LIB)
printf '\001' > /data/local/tmp/newbyte.bin
dd if=/data/local/tmp/newbyte.bin of=$LIB bs=1 seek=$((SZ-1)) count=1 conv=notrunc 2>/dev/null
echo "flipped=$(dd if=$LIB bs=1 skip=$((SZ-1)) count=1 2>/dev/null | od -An -tu1 | tr -d ' ')"

monkey -p com.a5game.ltzj.jj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 1
PID=$(pidof com.a5game.ltzj.jj)
echo "PID=$PID start=$(date +%s.%N)"

# 等 libcocos 出现在 maps，再等 0.4s（首读在 load+~1.0s）恢复
i=0
while [ $i -lt 200 ]; do
  if grep -q libcocos /proc/$PID/maps 2>/dev/null; then
    echo "cocos_loaded t=$(date +%s.%N)"
    break
  fi
  sleep 0.05
  i=$((i+1))
done
sleep 0.4
cp $BK $LIB
echo "restored t=$(date +%s.%N) md5=$(md5sum $LIB | cut -d' ' -f1)"

sleep 60
echo "after60s alive=$(pidof com.a5game.ltzj.jj)"
md5sum $LIB
