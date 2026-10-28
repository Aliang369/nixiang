#!/system/bin/sh
chmod 755 /data/local/tmp/ct6
echo "--- arm64 guest, patch host libc exit sites ---"; timeout 10 /data/local/tmp/ct6; echo "rc=$?"
