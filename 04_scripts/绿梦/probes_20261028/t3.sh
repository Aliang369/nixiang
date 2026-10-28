#!/system/bin/sh
chmod 755 /data/local/tmp/ct3_arm64 /data/local/tmp/ct3_x64
echo "--- arm64 ---"; timeout 6 /data/local/tmp/ct3_arm64; echo "rc=$?"
echo "--- x86_64 ---"; timeout 6 /data/local/tmp/ct3_x64; echo "rc=$?"
