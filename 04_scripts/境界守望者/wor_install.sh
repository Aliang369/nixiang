#!/usr/bin/env bash
# 境界守望者星光包装机阶段（国际服 com.ap.watcherofrealms；Split APK：base + split_config.arm64_v8a）
#
# ⚠️ 官方包与星光包（starry_release.jks）异签名 ⇒ `pm install -r` 必然
#    INSTALL_FAILED_UPDATE_INCOMPATIBLE。按 AGENTS.md：异签名先报告、不自动转卸载；
#    卸载官方包（丢本机游戏数据）须用户明确确认。
#    本脚本要求显式 STARRY_UNINSTALL_OK=1 才执行卸载+首装；否则只做覆盖尝试并停下。
#
# ⚠️ 本作是 Qigsaw Split 包：**必须 base + split 一起装**（install-multiple）。
#    只装 base 会 INSTALL_FAILED_MISSING_SPLIT（manifest 里有
#    requiredSplitTypes="base__abi" + com.android.vending.splits.required），
#    且 base 自身**不含任何 .so**——35 个 native 库全在 split_config.arm64_v8a 里。
#
# ⚠️ 而且 split **必须重签**：同一次 install-multiple 会话内所有 APK 必须同一张证书。
#    官方 split 是 BNDLTOOL 签名（带 Play 来源戳 stamp-cert-sha256），base 被改签
#    starry_release.jks ⇒ 直接混装必然 INSTALL_FAILED_INVALID_APK。
#    故 split 由 surgical 第 [5b/6] 步剥签、wor_sign.sh 用同一把钥匙重签。
#
# 用法:
#   SERIAL=<设备序列号> bash 04_scripts/境界守望者/wor_install.sh
#   异签名需卸载官方包时： STARRY_UNINSTALL_OK=1 SERIAL=<serial> bash 04_scripts/境界守望者/wor_install.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=wor_env.sh
[[ -f "$SCRIPT_DIR/wor_env.sh" ]] && . "$SCRIPT_DIR/wor_env.sh"
GAME="境界守望者"
VERSION="${WOR_VER:-1.6.38.829.1}"
PKG="com.ap.watcherofrealms"
APK="${WOR_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk}"
# split 必须用**重签后**的那份（与 base 同证书），不是 02_apks 里的官方原件
SPLIT="${WOR_SPLIT_SIGNED:-$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-splitSigned.apk}"
SERIAL="${SERIAL:-}"
NEW_DEX="${WOR_NEW_DEX:-classes6.dex}"
# （历史遗留）曾经用 adb push + 设备侧 pm install-multiple，现已改为宿主侧 adb install-multiple
REMOTE_DIR="/data/local/tmp/wor_${VERSION}_starry"

[[ -f "$APK" ]] || { echo "ERROR: 缺少星空包 ${APK}（先跑 surgical + wor_sign）" >&2; exit 1; }
[[ -n "$SERIAL" ]] || { echo "ERROR: 必须显式指定 SERIAL=<adb 设备序列号>（本作国际服测试机未在脚本里硬编码）" >&2; exit 1; }
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线" >&2; exit 1; }
if [[ ! -f "$SPLIT" ]]; then
  echo "ERROR: 缺少重签后的 split（Split APK 必须与 base 同证书一起装）: $SPLIT" >&2
  echo "       先跑 surgical_wor_1.6.38.829.1.sh（剥签）+ wor_sign.sh（重签）；" >&2
  echo "       或用 WOR_SPLIT_SIGNED=/path/to/xxx-splitSigned.apk 指定。" >&2
  exit 1
fi

PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3 python py; do
    if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
  done
fi

MODEL="$(adb -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ABI="$(adb -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
SDK="$(adb -s "$SERIAL" shell getprop ro.build.version.sdk | tr -d '\r')"
echo "[install] device=$SERIAL model=$MODEL abi=$ABI sdk=$SDK"
echo "[install] base=$APK"
echo "[install] split=$SPLIT"
if [[ -n "${WOR_EXPECT_MODEL:-}" && "$MODEL" != "$WOR_EXPECT_MODEL" ]]; then
  echo "ERROR: 目标机型不是约定测试机（$MODEL != ${WOR_EXPECT_MODEL}）" >&2
  exit 1
fi
if [[ "$ABI" != "arm64-v8a" ]]; then
  echo "ERROR: 设备 ABI=${ABI}，本包只含 arm64-v8a" >&2
  exit 1
fi

# ---- base/split 证书一致性（装机前最后一道闸；不一致 install-multiple 必失败）----
# ⚠️ Windows 上 apksigner.bat 文件位是 -rw-r--r--（**没有 x 位**），必须用 -f 判存在，
#    用 -x 会静默跳过本闸。实测 Git Bash 可直接调 apksigner.bat（走 cmd.exe）。
BT="${BUILD_TOOLS:-${WOR_BUILD_TOOLS:-}}"
APKSIGNER=""
for c in "$BT/apksigner" "$BT/apksigner.bat" "$BT/apksigner.exe"; do [[ -f "$c" ]] && { APKSIGNER="$c"; break; }; done
if [[ -n "$APKSIGNER" ]]; then
  APKSIGNER="$(cygpath -m "$APKSIGNER" 2>/dev/null || echo "$APKSIGNER")"
  CB="$("$APKSIGNER" verify --print-certs "$APK" 2>/dev/null | grep -i 'certificate SHA-256 digest' | head -1)"
  CS="$("$APKSIGNER" verify --print-certs "$SPLIT" 2>/dev/null | grep -i 'certificate SHA-256 digest' | head -1)"
  if [[ -z "$CB" || "$CB" != "$CS" ]]; then
    echo "ERROR: base/split 证书不一致，拒绝装机" >&2
    echo "  base : ${CB:-<读不出>}" >&2
    echo "  split: ${CS:-<读不出>}" >&2
    exit 1
  fi
  echo "[install] base/split 同证书 ✓  $CB"
else
  echo "ERROR: 找不到 apksigner（BT='$BT'），无法验证 base/split 证书一致性" >&2
  exit 1
fi

# ---- 装机前产物门禁（四查：cpp / dex / so / 菜单模板）----
TMPDIR_GATE="$(mktemp -d /tmp/wor_gate.XXXXXX)"
trap 'rm -rf "$TMPDIR_GATE"' EXIT
if [[ -n "$PY" ]]; then
  "$PY" -c 'import sys,zipfile;open(sys.argv[3],"wb").write(zipfile.ZipFile(sys.argv[1]).read(sys.argv[2]))' "$APK" "$NEW_DEX" "$TMPDIR_GATE/$NEW_DEX"
  "$PY" -c 'import sys,zipfile;open(sys.argv[3],"wb").write(zipfile.ZipFile(sys.argv[1]).read(sys.argv[2]))' "$APK" "lib/arm64-v8a/libstarry_magic.so" "$TMPDIR_GATE/libstarry_magic.so"
else
  ( cd "$TMPDIR_GATE" && unzip -o "$APK" "$NEW_DEX" lib/arm64-v8a/libstarry_magic.so >/dev/null )
fi
GATE_ARGS=(
  --cpp "$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni/Games/WorMod.cpp"
  --dex "$TMPDIR_GATE/$NEW_DEX"
  --so "$TMPDIR_GATE/libstarry_magic.so"
)
MENU_JSON="$NIXIANG/10_docs/$GAME/wor_menu_template.json"
[[ -f "$MENU_JSON" ]] && GATE_ARGS+=(--menu-json "$MENU_JSON")
RUNTIME_MENU="$NIXIANG/10_docs/$GAME/wor_runtime_menu.json"
[[ -f "$RUNTIME_MENU" ]] && GATE_ARGS+=(--runtime-menu-json "$RUNTIME_MENU")
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" "${GATE_ARGS[@]}"

adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
INSTALLED_PATH="$(adb -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"

# ---- 装机暂存：必须是**纯 ASCII 路径** ----
# ⚠️ 两个 Windows 专属坑，都实测过：
#  (1) 设备侧 `pm install-multiple` 在 Android 14(SDK34) 上**已被移除**：
#      `adb shell pm install-multiple ...` → `Unknown command: install-multiple`。
#      ⇒ 必须用**宿主侧** `adb install-multiple`（adb 1.0.41 / 33.0.0 支持，自己走
#        install-create/write/commit 会话，也省掉 push 200MB 到 /data/local/tmp）。
#  (2) adb.exe 的 argv 走 Windows ANSI 代码页 ⇒ 非 ASCII 路径会被解坏。症状是
#      `Exception ... java.lang.IllegalArgumentException: Invalid name: .`
#      （split 名从被解坏的文件名推出来变成 "."）。
#      ⇒ 先把两份 APK cp 到纯 ASCII 目录再装。实测换成 ASCII 路径后立刻变成
#        预期的 `INSTALL_FAILED_UPDATE_INCOMPATIBLE`。
STAGE="$NIXIANG/_install_stage/wor_${VERSION}"
rm -rf "$STAGE"; mkdir -p "$STAGE"
stage_both() {
  cp -f "$APK"   "$STAGE/wor-base.apk"
  cp -f "$SPLIT" "$STAGE/wor-config.arm64_v8a.apk"   # 文件名对齐 split 名 config.arm64_v8a
}
install_multiple() {  # $1 = 额外 adb install-multiple 参数（如 -r -d / -d）
  # shellcheck disable=SC2086
  MSYS_NO_PATHCONV=1 adb -s "$SERIAL" install-multiple $1 --user 0 \
    "$(cygpath -m "$STAGE/wor-base.apk")" "$(cygpath -m "$STAGE/wor-config.arm64_v8a.apk")"
}

if [[ "$INSTALLED_PATH" == package:* ]]; then
  echo "[install] 已安装 $PKG → 先尝试同签覆盖（base+split，不卸载）"
  stage_both
  set +e
  OUT="$(install_multiple "-r -d" 2>&1)"
  RC=$?
  set -e
  printf '%s\n' "$OUT"
  if [[ $RC -ne 0 ]]; then
    echo "[install] 覆盖失败（预期：异签名 INSTALL_FAILED_UPDATE_INCOMPATIBLE）"
    if [[ "${STARRY_UNINSTALL_OK:-0}" != "1" ]]; then
      echo "ERROR: 需要卸载官方包才能安装星空包；未获确认，停止（未卸载、未清数据）。" >&2
      echo "       确认后重跑： STARRY_UNINSTALL_OK=1 SERIAL=$SERIAL bash $0" >&2
      exit 1
    fi
    echo "[install] 已获确认 → 卸载官方包（本机游戏数据将丢失；账号数据在服务端）"
    adb -s "$SERIAL" uninstall "$PKG" || adb -s "$SERIAL" shell pm uninstall "$PKG"
    echo "[install] 首装星空包（base+split）"
    install_multiple "-d"
  fi
else
  echo "[install] 未安装 → 首装（base+split）"
  stage_both
  install_multiple "-d"
fi

rm -rf "$STAGE"

echo "[install] 设备包指纹闭环（无 root 机走 shell md5sum，实测 MIUI 可读）"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK"
adb -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=|signatures=|codePath=' | head -n 10
adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r'
echo "[install] done: $APK (+ split)"
echo "[install] 验收（用户操作）：① 桌面能启动、② 菜单能弹出、③ 能正常进游戏、④ logcat 见 '[Wor] lib ready'"
