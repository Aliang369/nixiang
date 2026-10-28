#!/system/bin/sh
chmod 755 /data/local/tmp/ct9
echo "--- fork ?? ptrace ?? ---"
timeout 20 /data/local/tmp/ct9; echo "rc=$?"
