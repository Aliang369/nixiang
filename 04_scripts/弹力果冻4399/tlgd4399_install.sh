#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399，vc4412）— 覆盖安装（2026-10-07 稳定性整改）
#
# 口径（用户 2026-10-07 明确授权）：
#   - **只覆盖安装，绝不卸载**（本游戏 ~2.1 GB，卸载会丢存档且重下载）
#   - 装机前过 pre_install_gate.sh（查2 JNI 配套 / 查5 门禁串字面量）
#   - 装机后 verify_installed_apk.sh 闭环：设备 base.apk md5 == 本地产物 md5
#   - 再解出设备 base.apk 内的 libstarry_magic.so / classes2.dex，与本地构建产物逐一比对，
#     证明「新 so + 新菜单 dex」真的落到设备上（不是装了旧包）
#
# 用法: bash 04_scripts/弹力果冻4399/tlgd4399_install.sh
#   TLGD4399_APK 可覆盖产物路径；SERIAL 可覆盖设备序列号
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.ginicat.sweet.m4399"
VC_EXPECT="4412"
STEM="弹力果冻4399_2.4.0_稳定整改_20261007"
APK="${TLGD4399_APK:-$NIXIANG/08_output/弹力果冻4399/${STEM}-releaseSigned.apk}"
BUILT_SO="$NIXIANG/mod菜单/starry-mod-menu/libs_tlgdm4399/arm64-v8a/libstarry_magic.so"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$APK" ]] || { echo "ERROR: 产物不存在 $APK —— 先跑 tlgd4399_surgical.sh"; exit 1; }
[[ -f "$BUILT_SO" ]] || { echo "ERROR: 缺 build_so 产物 $BUILT_SO"; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found"; exit 1; }
command -v adb >/dev/null || { echo "ERROR: adb not found"; exit 1; }

echo "=== [0/5] 产物身份与签名 ==="
# ⚠️ set -o pipefail 下 `cmd | head -N` 会因 SIGPIPE 让整条管线返回 141 并中止脚本
#    （2026-10-07 首次装机踩坑：脚本在第 0 步就 exit 141）。所有 `| head` 一律 `|| true` 兜底。
BADGE="$("$BT/aapt2" dump badging "$APK" 2>/dev/null | head -1 || true)"
echo "  $BADGE"
echo "$BADGE" | grep -q "name='$PKG'" || { echo "ERROR: 产物包名不是 $PKG"; exit 1; }
echo "$BADGE" | grep -q "versionCode='$VC_EXPECT'" || { echo "ERROR: 产物 versionCode 不是 $VC_EXPECT"; exit 1; }
"$BT/apksigner" verify --print-certs "$APK" 2>&1 | head -4 || true
"$BT/apksigner" verify "$APK" >/dev/null 2>&1 || { echo "ERROR: apksigner verify 失败"; exit 1; }
"$BT/apksigner" verify --print-certs "$APK" 2>/dev/null | grep -q 'StarryMagic' \
  || { echo "ERROR: 证书 CN 不是 StarryMagic —— 签名被换过，覆盖安装会被拒"; exit 1; }

echo "=== [1/5] 装机前门禁 pre_install_gate（查2 JNI 配套 + 查5 门禁串）==="
# ⚠️ 门禁串必须选 **dex 内真实存在的字面量**（pre_install_gate 查5 只搜 apk 里的 classes*.dex）。
#    不可用 `Category_战斗` / `Toggle_全屏秒杀` 这类 native 侧串 —— 它们在 libstarry_magic.so 里，
#    在 dex 中计数为 0 ⇒ 必然 FAIL 并拦装机（2026-10-07 首次装机前发现并修正）。
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
  --cpp "$JNI/MainModern.cpp" \
  --dex "$APK" \
  --apk "$APK" \
  --needle 'com/starry/magic/StarryApp' \
  --needle 'com/starry/magic/bridge/ModBridge' \
  --needle 'com/starry/magic/menu/MenuService' \
  --needle 'com/starry/magic/menu/NativeMenuController' \
  --needle 'com/starry/magic/sign/SignSpoof' \
  --needle 'com/starry/magic/ota' \
  || { echo "ERROR: 装机前门禁 FAIL —— 禁止装机"; exit 1; }

echo "=== [2/5] 设备在线检查 ==="
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线"; exit 1; }
# ⚠️ 必须 `grep -o 'versionCode=[0-9]*'` 先切出字段再取数字：直接 `tr -dc '0-9'` 会把
#    `versionCode=4412 minSdk=21 targetSdk=28` 整行数字拼成 44122128（2026-10-07 首跑踩坑）。
CUR_VC="$(adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' | grep -m1 -o 'versionCode=[0-9]*' | tr -dc '0-9' || true)"
echo "  设备已装 $PKG versionCode=${CUR_VC:-未知}（将覆盖安装，不卸载）"
[[ "${CUR_VC:-}" == "$VC_EXPECT" ]] || echo "  ⚠ 设备当前 versionCode 与 $VC_EXPECT 不同，覆盖安装仍可继续（同签名）"

echo "=== [3/5] 覆盖安装（install -r -d，保留数据；**本脚本永不卸载**）==="
adb -s "$SERIAL" push "$APK" /data/local/tmp/tlgd4399_new.apk
RC=0
adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/tlgd4399_new.apk'" 2>&1 | tail -5 || RC=$?
if [[ $RC -ne 0 ]]; then
  echo "  直接 su pm install 未成功，回退 adb install -r -d"
  adb -s "$SERIAL" install -r -d /data/local/tmp/tlgd4399_new.apk 2>&1 | tail -5 || RC=$?
fi
[[ $RC -eq 0 ]] || { echo "ERROR: 安装失败（rc=${RC}）。注意：本游戏约 2.1 GB，禁止卸载重装"; exit 1; }
adb -s "$SERIAL" shell rm -f /data/local/tmp/tlgd4399_new.apk >/dev/null 2>&1 || true

echo "=== [4/5] 装机完整性（设备 base.apk md5 == 本地产物）==="
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" \
  || bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "=== [5/5] 设备包内 so / 菜单 dex 逐项比对 ==="
BASE="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | grep 'base\.apk' | head -1 | sed 's/^package://' || true)"
[[ -n "$BASE" ]] || { echo "ERROR: 取不到设备 base.apk 路径"; exit 1; }
echo "  设备 base.apk = $BASE"
DEV_SO_MD5="$(adb -s "$SERIAL" exec-out "unzip -p '$BASE' lib/arm64-v8a/libstarry_magic.so" 2>/dev/null | md5 -q)"
LOC_SO_MD5="$(md5 -q "$BUILT_SO")"
echo "  so:   设备=$DEV_SO_MD5  本地构建=$LOC_SO_MD5"
[[ "$DEV_SO_MD5" == "$LOC_SO_MD5" ]] \
  || { echo "ERROR: 设备上不是本次构建的 so —— 装机没换新"; exit 1; }

DEV_DEX_SHA="$(adb -s "$SERIAL" exec-out "unzip -p '$BASE' classes2.dex" 2>/dev/null | shasum -a 256 | awk '{print $1}')"
LOC_DEX_SHA="$(unzip -p "$APK" classes2.dex | shasum -a 256 | awk '{print $1}')"
echo "  dex:  设备=${DEV_DEX_SHA:0:16}…  本地产物=${LOC_DEX_SHA:0:16}…"
[[ "$DEV_DEX_SHA" == "$LOC_DEX_SHA" ]] \
  || { echo "ERROR: 设备上不是本次构建的菜单 dex —— 菜单没换新"; exit 1; }

echo "  设备 dex 集合：$(adb -s "$SERIAL" exec-out "unzip -l '$BASE'" 2>/dev/null | tr -d '\r' | awk '{print $NF}' | grep -c '^classes.*\.dex$') 个（应为 2）"

echo "DONE: 覆盖安装 + 三项闭环校验全部通过（so / 菜单 dex / base.apk md5）"
echo "      下一步：用户装机实测（L3）+ logcat 抓 StarryApp/StarryMagic/Tlgd/MenuService 标签"
