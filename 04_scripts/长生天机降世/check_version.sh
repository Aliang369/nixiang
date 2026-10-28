#!/usr/bin/env bash
# 1) 成品包 versionName/versionCode；2) 设备端 yoo 下载缓存内容
set -uo pipefail
export MSYS_NO_PATHCONV=1
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SERIAL="${SERIAL:-emulator-5554}"
BUILT="08_output/长生天机降世/长生天机降世_1.0.4_surgical-releaseSigned.apk"
TMP="_cs104/tmp"

unzip -p "$BUILT" AndroidManifest.xml > "$TMP/built_AndroidManifest.xml"
echo "== 成品包 manifest 版本 =="
python 04_scripts/_common/axml_inspect.py "$TMP/built_AndroidManifest.xml" 2>/dev/null | grep -E 'versionName|versionCode|package ' | head -5

echo "== 官方包 manifest 版本（对照）=="
python 04_scripts/_common/axml_inspect.py "$TMP/off_AndroidManifest.xml" 2>/dev/null | grep -E 'versionName|versionCode|package ' | head -5

echo "== 设备 yoo 下载缓存 =="
adb -s "$SERIAL" shell "su -c 'find /sdcard/Android/data/com.netkuu.changsheng.tjjs/files/yoo -type f -exec ls -l {} \;'" 2>&1 | awk '{print $5, $NF}' | head -20
