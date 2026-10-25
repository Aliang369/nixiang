#!/usr/bin/env bash
# 圣翼传说：从当前真机只读拉取官方底包，并记录版本/签名/核心哈希。
# 输出默认落在 09_test/圣翼传说/base_YYYYMMDD/，不自动写入用户管理的 02_apks/。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="${WSCQ_PKG:-com.aiyue.wscq}"
DATE_TAG="${WSCQ_DATE_TAG:-$(date '+%Y%m%d')}"
OUT_DIR="${WSCQ_BASE_OUT_DIR:-$NIXIANG/09_test/圣翼传说/base_${DATE_TAG}}"
OUT_APK="$OUT_DIR/base.apk"
INFO="$OUT_DIR/base_info.txt"
ADB_BIN="${ADB_BIN:-adb}"
BT_DIR="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"

die() { echo "ERROR: $*" >&2; exit 1; }
need() { command -v "$1" >/dev/null 2>&1 || die "缺少命令: $1"; }

need "$ADB_BIN"
need unzip
need md5
need shasum
[[ -x "$BT_DIR/aapt2" ]] || die "缺少 aapt2: $BT_DIR/aapt2"
[[ -x "$BT_DIR/apksigner" ]] || die "缺少 apksigner: $BT_DIR/apksigner"
[[ "$("$ADB_BIN" -s "$SERIAL" get-state 2>/dev/null || true)" == "device" ]] \
  || die "设备不在线或 serial 不匹配: $SERIAL"

PATHS=()
while IFS= read -r path; do
  [[ -n "$path" ]] && PATHS+=("$path")
done < <("$ADB_BIN" -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | sed -n 's/^package://p')
[[ "${#PATHS[@]}" -ge 1 ]] || die "设备未安装 $PKG"
[[ "${#PATHS[@]}" -eq 1 ]] \
  || die "目标包为 split 形态（${#PATHS[@]} 个路径），当前外壳脚本只接受单 base.apk；请先补齐 split 处理"
BASE_REMOTE="${PATHS[0]}"
[[ "$BASE_REMOTE" == */base.apk ]] || die "pm path 不是 base.apk: $BASE_REMOTE"

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
[[ "$ROOT_ID" == *"uid=0"* ]] || die "未取得 root shell"
VERSION_NAME="$("$ADB_BIN" -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 versionName=" | tr -d '\r' | sed 's/.*versionName=//')"
VERSION_CODE="$("$ADB_BIN" -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 versionCode=" | tr -d '\r' | sed 's/.*versionCode=//; s/ .*//')"
FIRST_INSTALL="$("$ADB_BIN" -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 firstInstallTime=" | tr -d '\r' | sed 's/.*firstInstallTime=//')"
LAST_UPDATE="$("$ADB_BIN" -s "$SERIAL" shell "dumpsys package $PKG | grep -m1 lastUpdateTime=" | tr -d '\r' | sed 's/.*lastUpdateTime=//')"
[[ -n "$VERSION_NAME" && -n "$VERSION_CODE" ]] || die "读不到 versionName/versionCode"

mkdir -p "$OUT_DIR"
TMP="$OUT_APK.tmp"
rm -f "$TMP"
echo "[pull] $PKG $VERSION_NAME ($VERSION_CODE)"
echo "[pull] $BASE_REMOTE"
if ! "$ADB_BIN" -s "$SERIAL" pull "$BASE_REMOTE" "$TMP" >/dev/null 2>&1; then
  echo "[pull] adb pull 不可读，改用 root 单流读取"
  "$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat $BASE_REMOTE'" > "$TMP"
fi
[[ -s "$TMP" ]] || die "拉取结果为空"
mv -f "$TMP" "$OUT_APK"

unzip -Z1 "$OUT_APK" > "$OUT_DIR/entries.txt"
for entry in AndroidManifest.xml classes.dex lib/arm64-v8a/libil2cpp.so \
             assets/bin/Data/Managed/Metadata/global-metadata.dat; do
  grep -qxF "$entry" "$OUT_DIR/entries.txt" || die "底包缺少 $entry"
done

SIZE="$(stat -f %z "$OUT_APK")"
MD5="$(md5 -q "$OUT_APK")"
SHA256="$(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
SO_MD5="$(unzip -p "$OUT_APK" lib/arm64-v8a/libil2cpp.so | md5 -q)"
META_MD5="$(unzip -p "$OUT_APK" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"
DEXES="$(grep -E '^classes([0-9]+)?\.dex$' "$OUT_DIR/entries.txt" | tr '\n' ' ' | sed 's/ $//')"
"$BT_DIR/aapt2" dump badging "$OUT_APK" > "$OUT_DIR/badging.txt"
"$BT_DIR/apksigner" verify --print-certs "$OUT_APK" > "$OUT_DIR/apksigner.txt" 2>&1 || true
CERT="$(sed -n 's/^Signer #1 certificate DN: //p' "$OUT_DIR/apksigner.txt" | head -1)"
CERT_MD5="$(sed -n 's/^Signer #1 certificate MD5 digest: //p' "$OUT_DIR/apksigner.txt" | head -1)"
CERT_SHA256="$(sed -n 's/^Signer #1 certificate SHA-256 digest: //p' "$OUT_DIR/apksigner.txt" | head -1)"
[[ -n "$CERT" ]] || die "无法读取签名证书"

{
  echo "package=$PKG"
  echo "serial=$SERIAL"
  echo "versionName=$VERSION_NAME"
  echo "versionCode=$VERSION_CODE"
  echo "firstInstallTime=$FIRST_INSTALL"
  echo "lastUpdateTime=$LAST_UPDATE"
  echo "device_path=$BASE_REMOTE"
  echo "local_path=$OUT_APK"
  echo "size=$SIZE"
  echo "md5=$MD5"
  echo "sha256=$SHA256"
  echo "libil2cpp_md5=$SO_MD5"
  echo "global_metadata_md5=$META_MD5"
  echo "dexes=$DEXES"
  echo "signer_dn=$CERT"
  echo "signer_md5=$CERT_MD5"
  echo "signer_sha256=$CERT_SHA256"
} | tee "$INFO"

echo "[pull] done → $OUT_APK"
