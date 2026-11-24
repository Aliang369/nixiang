#!/usr/bin/env bash
# 暗影格斗3：只读提取真机当前安装的 TapTap 原版单一 base APK。
# 仅拉取，不安装、不卸载、不清数据、不启动游戏。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=../_common/env.sh
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/official_apk.json"
PKG="com.wanda.sf3.wd"
GAME="暗影格斗3"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready 阶段" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
# 点名校验（2026-11-24）：EXPECTED_SERIAL 显式给出时必须与 device_ready 落盘的 serial 一致，
# 否则多设备环境下会拿着旧 serial 去拉错设备。
if [[ -n "${EXPECTED_SERIAL:-}" && "$EXPECTED_SERIAL" != "$SERIAL" ]]; then
  echo "错误: 设备状态文件为 ${SERIAL}，与 EXPECTED_SERIAL=${EXPECTED_SERIAL} 不一致；先重跑 device_ready 阶段" >&2
  exit 1
fi
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"

[[ -n "$VERSION_NAME" ]] || { echo "错误: 无法读取 versionName: $PKG" >&2; exit 1; }
[[ -n "$VERSION_CODE" ]] || { echo "错误: 无法读取 versionCode: $PKG" >&2; exit 1; }
[[ "$VERSION_NAME" =~ ^[A-Za-z0-9._-]+$ ]] || { echo "错误: versionName 含不安全字符: $VERSION_NAME" >&2; exit 1; }
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装包: $PKG" >&2; exit 1; }

PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$PATH_COUNT" -ne 1 ]]; then
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；当前只允许完整单 base APK，拒绝漏拉 split。" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
fi

REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in
  /data/app/*) ;;
  *) echo "错误: 非预期安装路径: $REMOTE_APK" >&2; exit 1 ;;
esac

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_taptap_official_from_device.apk"
TMP="$OUT.part"

REUSE_EXISTING=0
if [[ -e "$OUT" ]]; then
  echo "==> 目标 APK 已存在，跳过重复拉取，改为校验设备一致性: $OUT"
  REUSE_EXISTING=1
fi
mkdir -p "$OUT_DIR"

if [[ "$REUSE_EXISTING" -eq 0 ]]; then
  rm -f "$TMP"
  trap 'rm -f "$TMP"' EXIT
  echo "==> 直接只读拉取: $REMOTE_APK"
  # Git Bash/MSYS 会把 argv 里的 /data/app/... 当本地路径改写（2026-11-24 实测变成
  # C:/Program Files/Git/data/app/... ⇒ adb: failed to stat remote object）；该调用必须关掉参数
  # 转换，而关掉后 adb.exe 也看不懂 /e/... 形式的本地目标，故目标先转成 Windows 形式。
  PULL_FAILED=0
  if command -v cygpath >/dev/null 2>&1; then
    MSYS_NO_PATHCONV=1 "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$(cygpath -m "$TMP")" || PULL_FAILED=1
  else
    "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP" || PULL_FAILED=1
  fi
  if [ "$PULL_FAILED" -ne 0 ]; then
    echo "==> 直接拉取失败，改用 root 流式读取（设备端不落第二份大文件）"
    rm -f "$TMP"
    "$ADB_BIN" -s "$SERIAL" exec-out su -c "cat '$REMOTE_APK'" > "$TMP"
  fi
  [[ -s "$TMP" ]] || { echo "错误: 拉取结果为空" >&2; exit 1; }
  unzip -tq "$TMP" >/dev/null
  mv "$TMP" "$OUT"
  trap - EXIT
fi
unzip -tq "$OUT" >/dev/null

# 跨平台指纹（2026-11-24）：macOS 是 stat -f '%z' / shasum / md5 -q，GNU（Git Bash / Linux）是
# stat -c '%s' / sha256sum / md5sum。本机 Win 裸环境没有 md5/shasum，GNU stat 的 -f 会被当成
# 「文件系统信息」而失败（rc=1 且把错误打到 stdout）⇒ 先探测 -c，失败再回退 BSD 写法。
_file_size() { if stat -c '%s' "$1" >/dev/null 2>&1; then stat -c '%s' "$1"; else stat -f '%z' "$1"; fi; }
_file_sha256() { if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi; }
_file_md5() { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }
SIZE="$(_file_size "$OUT")"
SHA256="$(_file_sha256 "$OUT")"
MD5="$(_file_md5 "$OUT")"
REMOTE_SIZE="$("$ADB_BIN" -s "$SERIAL" shell "stat -c %s '$REMOTE_APK'" | tr -d '\r')"
REMOTE_SHA256="$("$ADB_BIN" -s "$SERIAL" shell "sha256sum '$REMOTE_APK'" | tr -d '\r' | awk '{print $1}')"
[[ "$SIZE" == "$REMOTE_SIZE" ]] || { echo "错误: 本地大小与设备不一致: local=$SIZE remote=$REMOTE_SIZE" >&2; exit 1; }
[[ "$SHA256" == "$REMOTE_SHA256" ]] || { echo "错误: 本地 SHA-256 与设备不一致" >&2; exit 1; }

SIGNER=""
SIGNER_SHA256=""
# [ -f ] 而非 [ -x ]（2026-11-24）：Windows 的 apksigner 只有 apksigner.bat，MSYS 对 .bat 的 -x
# 恒为假 ⇒ 旧写法在 Win 上会让签名者读取与「斌哥包拦截」整段静默跳过（signer 字段留空）。
# 这与 _common/env.sh 解析 apksigner 时的判定口径一致。
if [[ -n "${NIXIANG_APKSIGNER:-}" && -f "$NIXIANG_APKSIGNER" ]]; then
  CERT_OUT="$("$NIXIANG_APKSIGNER" verify --print-certs "$OUT" 2>/dev/null || true)"
  SIGNER="$(printf '%s\n' "$CERT_OUT" | sed -n 's/^Signer #1 certificate DN: //p' | head -1)"
  SIGNER_SHA256="$(printf '%s\n' "$CERT_OUT" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
fi

BINGE_SHA256="11491cb602665c768f565bc2ae530bddbdf8365af5628d9fdb5ced80238b5eba"
SIGNER_SHA256_LC="$(printf '%s' "$SIGNER_SHA256" | tr '[:upper:]' '[:lower:]')"
if [[ -n "$SIGNER_SHA256_LC" ]]; then
  if [[ "$SIGNER_SHA256_LC" == "$BINGE_SHA256" ]]; then
    echo "错误: 设备当前安装的是斌哥签名包，不是 TapTap 原版" >&2
    exit 1
  fi
fi

python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" "$SERIAL" "$REMOTE_APK" "$SIZE" "$SHA256" "$MD5" "$PKG" "$SIGNER" "$SIGNER_SHA256" <<'PY'
import json
import sys
from pathlib import Path

path, apk, version_name, version_code, installer, abi, serial, remote, size, sha256, md5, package, signer, signer_sha256 = sys.argv[1:]
data = {
    "package": package,
    "game": "暗影格斗3",
    "kind": "taptap_official",
    "apk_path": apk,
    "version_name": version_name,
    "version_code": int(version_code) if version_code.isdigit() else version_code,
    "installer": installer,
    "primary_cpu_abi": abi,
    "serial": serial,
    "remote_path": remote,
    "size": int(size),
    "sha256": sha256,
    "md5": md5,
    "signer": signer,
    "signer_sha256": signer_sha256,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat <<EOF
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
installer=$INSTALLER
primaryCpuAbi=$PRIMARY_ABI
serial=$SERIAL
remote=$REMOTE_APK
output=$OUT
size=$SIZE
sha256=$SHA256
md5=$MD5
signer=$SIGNER
signer_sha256=$SIGNER_SHA256
meta=$META_FILE
EOF
