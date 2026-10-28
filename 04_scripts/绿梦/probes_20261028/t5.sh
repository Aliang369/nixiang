#!/system/bin/sh
chmod 755 /data/local/tmp/ct5
echo "--- arm64 ERRNO mode ---"; timeout 8 /data/local/tmp/ct5; echo "rc=$?"
