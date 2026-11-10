#!/usr/bin/env bash
# 宝石战争：热更后一键重注（C 方案）
#
# 背景：本作走 HybridCLR 热更，官方每次热更都会把未补丁的 System.Network.dll 落到设备
#   外部 files/HotUpdate/ 并覆盖包内补丁 DLL，三项功能失效（1.0.0.04/06/14/15 四次同型）。
#   native hook 抗热更路线已于 2026-11-10 实测失败（解释器内部分发，methodPointer 命中 0），
#   故采用「内嵌回包」的确定性路径，本脚本把该路径一步到位。
#
# 四件事：
#   1. 比对「设备外部热更版本」与「已装包内 manifest 版本」，相同则直接退出
#   2. 只读拉回真机已装包 + 外部热更增量（不重下官方 APK、不重编 dex/so）
#   3. 解密 -> 4 锚点注入 -> 外科组装（142 资产逐条校验）-> 签名
#   4. 覆盖安装（不清数据）-> 清空外部 HotUpdate -> 装机预检
#
# 用法：
#   bash 04_scripts/宝石战争/bszz_hotfix_auto.sh
#   BSZZ_SKIP_INSTALL=1 bash 04_scripts/宝石战争/bszz_hotfix_auto.sh   # 只到签名
#   BSZZ_FORCE=1 bash 04_scripts/宝石战争/bszz_hotfix_auto.sh          # 版本相同也强制重做
set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="宝石战争"
PKG="com.acing.gow.adv"
SERIES="6.0.013"
DUMP="$NIXIANG/06_dumped/$GAME/$SERIES"
CUR="$DUMP/current"
BASE_APK="$CUR/base.apk"
EXT_DIR="/sdcard/Android/data/$PKG/files/HotUpdate/Android"

FORCE="$BSZZ_FORCE"
SKIP_INSTALL="$BSZZ_SKIP_INSTALL"

step() { echo; echo "==> [$1] $2"; }

# ---- 0. 设备就绪 ----
step 0/8 "设备锁定"
DEV_OUT="$(bash "$SCRIPT_DIR/device_ready_bszz.sh")"
SERIAL="$(printf '%s\n' "$DEV_OUT" | sed -n 's/^serial=//p')"
echo "    serial=$SERIAL"

# ---- 1. 版本比对 ----
step 1/8 "比对热更版本"
EXT_MAN_TMP="$(mktemp /tmp/bszz_extman.XXXXXX)"
adb -s "$SERIAL" shell "su -c 'cat $EXT_DIR/manifest.json'" > "$EXT_MAN_TMP" 2>/dev/null || true
EXT_VER="$(python3 -c "
import json,sys
try:
    print(json.load(open(sys.argv[1]))['version'])
except Exception:
    print('')
" "$EXT_MAN_TMP")"

if [ -z "$EXT_VER" ]; then
  echo "    设备外部 HotUpdate 无 manifest（未热更或已清空）"
  echo "    结论：无需处理"
  rm -f "$EXT_MAN_TMP"
  exit 0
fi

mkdir -p "$CUR"
if [ -f "$BASE_APK" ]; then
  IN_VER="$(unzip -p "$BASE_APK" assets/Android/manifest.json | python3 -c 'import json,sys; print(json.load(sys.stdin)["version"])')"
else
  IN_VER=""
fi
echo "    设备外部热更 = $EXT_VER"
echo "    本地底包内   = $IN_VER"

if [ -n "$IN_VER" ] && [ "$EXT_VER" = "$IN_VER" ] && [ "$FORCE" != "1" ]; then
  echo "    结论：包内已是 ${EXT_VER}，无需重注（BSZZ_FORCE=1 可强制）"
  rm -f "$EXT_MAN_TMP"
  exit 0
fi
rm -f "$EXT_MAN_TMP"

VERSION="$SERIES""_hu$EXT_VER"
HOT_ROOT="$DUMP/hotupdate_$EXT_VER"
echo "    本轮版本标识 = $VERSION"

# ---- 2. 拉底包 + 增量 ----
step 2/8 "只读拉取真机底包与热更增量"
REMOTE_BASE="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
[ -n "$REMOTE_BASE" ] || { echo "ERROR: 设备未安装 $PKG" >&2; exit 1; }
rm -f "$BASE_APK.part"
adb -s "$SERIAL" pull "$REMOTE_BASE" "$BASE_APK.part" >/dev/null
mv -f "$BASE_APK.part" "$BASE_APK"
BASE_MD5="$(md5 -q "$BASE_APK")"
echo "    底包 md5=$BASE_MD5 size=$(stat -f '%z' "$BASE_APK")"

IN_VER2="$(unzip -p "$BASE_APK" assets/Android/manifest.json | python3 -c 'import json,sys; print(json.load(sys.stdin)["version"])')"
if [ "$EXT_VER" = "$IN_VER2" ] && [ "$FORCE" != "1" ]; then
  echo "    拉回后确认包内已是 ${EXT_VER}，无需重注（BSZZ_FORCE=1 可强制）"
  exit 0
fi

mkdir -p "$HOT_ROOT"
rm -rf "$HOT_ROOT/Android"
adb -s "$SERIAL" shell "su -c 'rm -rf /data/local/tmp/bszz_hu && cp -r $EXT_DIR /data/local/tmp/bszz_hu && chmod -R 644 /data/local/tmp/bszz_hu/* 2>/dev/null; chmod 755 /data/local/tmp/bszz_hu/Dlls 2>/dev/null'" >/dev/null
adb -s "$SERIAL" pull /data/local/tmp/bszz_hu "$HOT_ROOT/Android" >/dev/null
adb -s "$SERIAL" shell "su -c 'rm -rf /data/local/tmp/bszz_hu'" >/dev/null
echo "    增量文件数 = $(find "$HOT_ROOT/Android" -type f | wc -l | tr -d ' ')"

# Platform 未变化时官方不下发，但 dump_bszz_static 要求四件齐全 => 从底包补齐
if [ ! -f "$HOT_ROOT/Android/Dlls/System.Platform.dll.bytes" ]; then
  unzip -p "$BASE_APK" assets/Android/Dlls/System.Platform.dll.bytes \
    > "$HOT_ROOT/Android/Dlls/System.Platform.dll.bytes"
  echo "    Platform 未在增量中，已从底包补齐"
fi

# ---- 3. 解密 ----
step 3/8 "解密热更程序集"
BSZZ_SOURCE_APK="$BASE_APK" BSZZ_HOTUPDATE_DIR="$HOT_ROOT/Android" \
  bash "$SCRIPT_DIR/dump_bszz_static.sh" >/dev/null
python3 -c "
import json
d=json.load(open('$DUMP/static/static_inputs.json'))
print('    manifest=%s 明文 md5=%s size=%s' % (d['source_hotupdate_manifest_version'], d['system_network_plain_md5'], d['system_network_plain_size']))
"

# ---- 4. 注入 ----
step 4/8 "4 锚点 IL 注入"
BSZZ_VERSION="$SERIES" bash "$SCRIPT_DIR/bszz_patch_dll.sh" 2>&1 | grep -E 'AdjustDamage/Call|SetMana/Call|SetHealth/Call|^md5=' | head -6 || true

# ---- 5. 组装 ----
step 5/8 "外科组装（只换热更资产）"
BSZZ_VERSION="$VERSION" \
BSZZ_BASE_APK="$BASE_APK" \
BSZZ_BASE_APK_MD5="$BASE_MD5" \
BSZZ_HOTUPDATE_ROOT="$HOT_ROOT" \
BSZZ_PATCHED_NETWORK="$NIXIANG/07_patched/$GAME/$SERIES/assets/Android/Dlls/System.Network.dll.bytes" \
BSZZ_EXPECT_HOTUPDATE_VERSION="$EXT_VER" \
  bash "$SCRIPT_DIR/bszz_surgical_hotupdate_only.sh" 2>&1 | grep -E 'assets_total|assets_from_delta|from_apk_count|unsigned_apk_md5|hotupdate_version' | head -12

# ---- 6. 签名 ----
step 6/8 "签名"
BSZZ_VERSION="$VERSION" bash "$SCRIPT_DIR/bszz_sign.sh" 2>&1 | tail -4

if [ "$SKIP_INSTALL" = "1" ]; then
  echo; echo "BSZZ_SKIP_INSTALL=1 => 停在签名，未装机"
  exit 0
fi

# ---- 7. 装机 + 清外部 ----
step 7/8 "覆盖安装（不清数据）"
BSZZ_VERSION="$VERSION" bash "$SCRIPT_DIR/bszz_install.sh" 2>&1 | tail -3

echo "    清空外部 HotUpdate（防官方未补丁 DLL 覆盖）"
adb -s "$SERIAL" shell "su -c 'find $EXT_DIR -type f -delete'" >/dev/null
LEFT="$(adb -s "$SERIAL" shell "su -c 'find $EXT_DIR -type f | wc -l'" | tr -d '\r ')"
echo "    外部剩余文件 = $LEFT"

# ---- 8. 预检 ----
step 8/8 "装机预检（空转 1 分钟）"
bash "$NIXIANG/04_scripts/_common/post_install_precheck.sh" --game "$GAME" --idle-min 1 2>&1 | tail -8 || true

echo
echo "================ 完成 ================"
echo "热更版本 : $EXT_VER"
echo "正式包   : 08_output/$GAME/""$GAME"_"$VERSION""_surgical-releaseSigned.apk"
echo "底包 md5 : $BASE_MD5"
echo "下一步   : 请进战斗实测三项功能（攻击倍率 / 上帝模式 / 无限技能），"
echo "           有效/无效请回一句，我记轮次台账。"
