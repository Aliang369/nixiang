#!/system/bin/sh
chmod 755 /data/local/tmp/ct8
echo "--- self-process ptrace ---"; timeout 15 /data/local/tmp/ct8; echo "rc=$?"
