#!/system/bin/sh
echo "--- arm64 (houdini) ---"
timeout 8 /data/local/tmp/ct2_arm64
echo "rc=$?"
echo "--- x86_64 native ---"
timeout 8 /data/local/tmp/ct2_x64
echo "rc=$?"
echo "--- arm64 with stderr ---"
timeout 8 /data/local/tmp/ct2_arm64 2>&1
echo "rc2=$?"
