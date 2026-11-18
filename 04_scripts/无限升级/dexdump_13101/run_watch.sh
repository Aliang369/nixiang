#!/system/bin/sh
PKG=com.xingzuodaoshi.infinityUpgrade
am force-stop $PKG
sleep 1
rm -rf /data/local/tmp/watch
mkdir -p /data/local/tmp/watch
monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 &
i=0
while [ $i -lt 400 ]; do
  pid=$(pidof $PKG)
  if [ -n "$pid" ]; then break; fi
  i=$((i+1))
done
echo "pid=$pid wait_iters=$i"
/data/local/tmp/wxsj_dex_watch $pid /data/local/tmp/watch 30 skipinit
echo done
