#!/usr/bin/env bash
# 潮汐守望者星光包装机阶段（默认目标 = 无 root 小米测试机 96f0d883）
#
# ⚠️ 官方包（CN=LF）与星光包（starry_release.jks）异签名 ⇒ `pm install -r` 必然
#    INSTALL_FAILED_UPDATE_INCOMPATIBLE。按 AGENTS.md：异签名先报告、不自动转卸载；
#    卸载官方包（丢本机游戏数据）须用户明确确认。
#    本脚本要求显式 STARRY_UNINSTALL_OK=1 才执行卸载+首装；否则只做覆盖尝试并停下。
#
# 用法:
#   STARRY_UNINSTALL_OK=1 SERIAL=96f0d883 bash 04_scripts/潮汐守望者/cxswz_install.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="潮汐守望者"
VERSION="${CXSWZ_VER:-1.2.56.375.3}"
PKG="com.gf.cxswz"
APK="${CXSWZ_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk}"
SERIAL="${SERIAL:-96f0d883}"
REMOTE_APK="/data/local/tmp/cxswz_${VERSION}_starry.apk"

[[ -f "$APK" ]] || { echo "ERROR: 缺少星空包 $APK" >&2; exit 1; }
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线" >&2; exit 1; }

MODEL="$(adb -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
echo "[install] device=$SERIAL model=$MODEL apk=$APK"
if [[ "$MODEL" != "M2102K1AC" && "${ALLOW_ANY_DEVICE:-0}" != "1" ]]; then
  echo "ERROR: 目标不是约定的无 root 小米测试机（M2102K1AC）；如确认要装请加 ALLOW_ANY_DEVICE=1" >&2
  exit 1
fi

# ---- 装机前产物门禁（四查）----
TMPDIR_GATE="$(mktemp -d /tmp/cxswz_gate.XXXXXX)"
trap 'rm -rf "$TMPDIR_GATE"' EXIT
NEW_DEX="${CXSWZ_NEW_DEX:-classes16.dex}"
unzip -p "$APK" "$NEW_DEX" > "$TMPDIR_GATE/$NEW_DEX"
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$TMPDIR_GATE/libstarry_magic.so"
GATE_ARGS=(
  --cpp "$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni/Games/CxswzMod.cpp"
  --dex "$TMPDIR_GATE/$NEW_DEX"
  --so "$TMPDIR_GATE/libstarry_magic.so"
  --menu-json "$NIXIANG/10_docs/潮汐守望者/cxswz_menu_template.json"
)
RUNTIME_MENU="$NIXIANG/10_docs/潮汐守望者/cxswz_runtime_menu.json"
[[ -f "$RUNTIME_MENU" ]] && GATE_ARGS+=(--runtime-menu-json "$RUNTIME_MENU")
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" "${GATE_ARGS[@]}"

adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
INSTALLED_PATH="$(adb -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"

if [[ "$INSTALLED_PATH" == package:* ]]; then
  echo "[install] 已安装 $PKG → 先尝试同签覆盖（不卸载）"
  adb -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null
  set +e
  OUT="$(adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK" 2>&1)"
  RC=$?
  set -e
  printf '%s\n' "$OUT"
  if [[ $RC -ne 0 ]]; then
    echo "[install] 覆盖失败（预期：异签名）"
    if [[ "${STARRY_UNINSTALL_OK:-0}" != "1" ]]; then
      echo "ERROR: 需要卸载官方包才能安装星空包；未获确认，停止（未卸载、未清数据）。" >&2
      echo "       确认后重跑： STARRY_UNINSTALL_OK=1 SERIAL=$SERIAL bash $0" >&2
      exit 1
    fi
    echo "[install] 已获确认 → 卸载官方包（本机游戏数据将丢失；账号数据在服务端）"
    adb -s "$SERIAL" uninstall "$PKG"
    echo "[install] 首装星空包"
    adb -s "$SERIAL" shell pm install -d --user 0 "$REMOTE_APK"
  fi
else
  echo "[install] 未安装 → 首装"
  adb -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null
  adb -s "$SERIAL" shell pm install -d --user 0 "$REMOTE_APK"
fi

adb -s "$SERIAL" shell rm -f "$REMOTE_APK" || true

echo "[install] 设备包指纹闭环（无 root 机走 shell md5sum，实测 MIUI 可读）"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK"
adb -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=|signatures=' | head -n 8

echo "[install] done: $APK"
