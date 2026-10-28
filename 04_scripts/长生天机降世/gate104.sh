#!/usr/bin/env bash
# 1.0.4 装机门禁（11 查）+ 安装（异签：当前设备无原版 ⇒ 纯安装，不涉及卸载）
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SERIAL="${SERIAL:-emulator-5554}"
APK="08_output/长生天机降世/长生天机降世_1.0.4_surgical-releaseSigned.apk"
CPP="mod菜单/starry-mod-menu/app/src/main/jni/Games/CstjjsMod.cpp"

echo "===== [1] pre_install_gate ====="
bash 04_scripts/_common/pre_install_gate.sh --game 长生天机降世 --rounds-threshold 3 \
     --cpp "$CPP" --apk "$APK"
echo "gate rc=$?"
