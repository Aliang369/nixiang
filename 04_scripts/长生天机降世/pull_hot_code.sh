#!/usr/bin/env bash
# 拉取热更下载的 code bundle 并解出运行时 Code.dll
set -uo pipefail
export MSYS_NO_PATHCONV=1
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SERIAL="${SERIAL:-emulator-5554}"
SRC=/sdcard/Android/data/com.netkuu.changsheng.tjjs/files/yoo/DefaultPackage/CacheFiles/28/2881b5583742ca26faabf74502d508ae/__data
adb -s "$SERIAL" shell "su -c 'cp -f $SRC /data/local/tmp/cs104/code_hot.bundle && chmod 666 /data/local/tmp/cs104/code_hot.bundle'"
adb -s "$SERIAL" pull /data/local/tmp/cs104/code_hot.bundle _cs104/tmp/code_hot.bundle 2>&1 | tail -1
ls -l _cs104/tmp/code_hot.bundle
head -c 32 _cs104/tmp/code_hot.bundle | xxd | head -2
