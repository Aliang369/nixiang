#!/system/bin/sh
p=$(pidof com.zhuziplay.lvmengnew.op)
echo "pid=$p"
grep -i -E 'fairguard|il2cpp|libunity' /proc/$p/maps | head -30
echo "--- count fg exec ranges ---"
grep -i 'fairguard' /proc/$p/maps | grep '....xp\|...xp' | wc -l
