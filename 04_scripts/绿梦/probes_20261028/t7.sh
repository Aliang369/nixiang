#!/system/bin/sh
chmod 755 /data/local/tmp/ct7
echo "--- guest code patch honored? ---"; timeout 6 /data/local/tmp/ct7; echo "rc=$?"
