#!/usr/bin/env bash
# 安装 1.0.4 成品到雷电（无原版 ⇒ 纯安装）
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SERIAL="${SERIAL:-emulator-5554}"
APK="08_output/长生天机降世/长生天机降世_1.0.4_surgical-releaseSigned.apk"

echo "== device =="
adb -s "$SERIAL" shell getprop ro.product.model
echo "== install =="
time adb -s "$SERIAL" install -r -d "$APK"
echo "rc=$?"
echo "== verify =="
adb -s "$SERIAL" shell dumpsys package com.netkuu.changsheng.tjjs | grep -E 'versionName|versionCode|firstInstallTime|primaryCpuAbi|codePath' | head -8
