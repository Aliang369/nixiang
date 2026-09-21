#!/usr/bin/env bash
# 勇士之心 1.0.3（2026-09-21 热更重抓后）外科组装入口。
#
# 背景：02_apks/勇士之心 下的旧原版已随工作区清理移除；当前真机安装的官方
# 1.0.3 是另一份构建（base.apk md5 c20f70c8…，含 libtapsdkcore/libthedmis，
# 但 lib/arm64-v8a/libcocos2djs.so 与旧原版字节一致）。本脚本以真机只读拉取的
# base.apk 作为外科底包，环境变量只在本脚本内生效，不动 surgical_ysxz_1.0.3.sh。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
TAG="${TAG:-20260921}"
BASE="$NIXIANG/09_test/勇士之心/regrab_$TAG/base.apk"
BASE_MD5_FILE="$NIXIANG/09_test/勇士之心/regrab_$TAG/base_apk_md5.txt"

[[ -f "$BASE" ]] || { echo "ERROR: missing base apk $BASE" >&2; exit 1; }
BASE_MD5="$(md5 -q "$BASE")"
if [[ -f "$BASE_MD5_FILE" && "$(tr -d '[:space:]' < "$BASE_MD5_FILE")" != "$BASE_MD5" ]]; then
  echo "ERROR: base apk md5 与重抓记录不符: $BASE_MD5" >&2; exit 1
fi

echo "[rebase] base=$BASE"
echo "[rebase] base_md5=$BASE_MD5"
ORIG_APK="$BASE" ORIG_MD5="$BASE_MD5" \
  bash "$NIXIANG/mod菜单/starry-mod-menu/scripts/surgical_ysxz_1.0.3.sh"
