#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 设备侧落地状态**只读**核对（包 hash / 星光 so hash / 进程 / 安装时间）
#
# 口径（2026-11-05 接手轮）：
#   * 完全只读：pm path / dumpsys package / su sha256sum / pidof；不安装、不卸载、不清数据、
#     不 force-stop、不启动、不截图、不 input。
#   * 用途：接手失败子代理的 provider-only 对照轮时，核对「设备上装的是不是候选包、so 是否同一枚、
#     崩溃后进程是否已死、包是覆盖还是重装（firstInstallTime vs lastUpdateTime）」。
#   * 产物：04_scripts/动物大联盟/device_verify.json + device_verify.txt
#
# 用法：
#   bash 04_scripts/动物大联盟/verify_device_state_dwdlm.sh [--serial f9a116a2] [--apk <本地候选>]
set -euo pipefail
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin:${PATH:-}"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="com.fightingstars.games.animal"
SERIAL=""
APK="${DWDLM_APK:-$NIXIANG/08_output/动物大联盟/动物大联盟_1.0.25_starry-env-provideronly-releaseSigned.apk}"
while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --apk) APK="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $1" >&2; exit 2 ;;
  esac
done
[ -n "$SERIAL" ] || SERIAL="$(cat "$SCRIPT_DIR/.device_serial" 2>/dev/null || true)"
[ -n "$SERIAL" ] || { echo "ERROR: 无 serial" >&2; exit 1; }
command -v adb >/dev/null 2>&1 || { echo "ERROR: adb 不可用" >&2; exit 1; }
[ -f "$APK" ] || { echo "ERROR: 本地候选不存在: $APK" >&2; exit 1; }

adbs() { adb -s "$SERIAL" shell "$@" 2>/dev/null | tr -d '\r'; }
adb -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线" >&2; exit 1; }

PM_PATH="$(adbs "pm path $PKG" | grep '^package:' | head -1 | sed 's/^package://')"
[ -n "$PM_PATH" ] || { echo "ERROR: $PKG 未安装" >&2; exit 1; }
APP_DIR="$(dirname "$PM_PATH")"

DUMP="$(mktemp /tmp/dwdlm_verify.XXXXXX)"
adbs "dumpsys package $PKG" > "$DUMP" || true
VC="$(grep -m1 -o 'versionCode=[0-9]*' "$DUMP" || true)"
VN="$(grep -m1 -o 'versionName=[^ ]*' "$DUMP" || true)"
FIRST="$(grep -m1 -o 'firstInstallTime=[^\n]*' "$DUMP" || true)"
LAST="$(grep -m1 -o 'lastUpdateTime=[^\n]*' "$DUMP" || true)"
rm -f "$DUMP"

DEV_APK_SHA="$(adbs "su -c 'sha256sum $PM_PATH'" | awk '{print $1}')"
DEV_SO_SHA="$(adbs "su -c 'sha256sum $APP_DIR/lib/arm64/libstarry_magic.so'" | awk '{print $1}')"
PID="$( { adbs "pidof $PKG" || true; } | tr -d ' ' )"
LOC_APK_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
LOC_APK_SIZE="$(stat -f "%z" "$APK")"
LOC_SO="$(ls "$NIXIANG/mod菜单/starry-mod-menu/libs_dwdlm/env/arm64-v8a/libstarry_magic.so" 2>/dev/null || true)"
LOC_SO_SHA=""
if [ -n "$LOC_SO" ]; then LOC_SO_SHA="$(shasum -a 256 "$LOC_SO" | awk '{print $1}')"; fi

APK_MATCH="no"
if [ -n "$DEV_APK_SHA" ] && [ "$DEV_APK_SHA" = "$LOC_APK_SHA" ]; then APK_MATCH="yes"; fi
SO_MATCH="no"
if [ -n "$DEV_SO_SHA" ] && [ -n "$LOC_SO_SHA" ] && [ "$DEV_SO_SHA" = "$LOC_SO_SHA" ]; then SO_MATCH="yes"; fi
PROC_STATE="not-running"
if [ -n "$PID" ]; then PROC_STATE="running:$PID"; fi

cat > "$SCRIPT_DIR/device_verify.txt" <<EOF
# 动物大联盟 设备落地只读核对 $(date '+%Y-%m-%d %H:%M:%S')
serial=$SERIAL
pkg=$PKG
pm_path=$PM_PATH
$VC
$VN
$FIRST
$LAST
device_apk_sha256=${DEV_APK_SHA:-<empty>}
local_apk=$APK
local_apk_sha256=$LOC_APK_SHA
local_apk_size=$LOC_APK_SIZE
apk_match=$APK_MATCH
device_so_sha256=${DEV_SO_SHA:-<empty>}
local_so_sha256=${LOC_SO_SHA:-<empty>}
so_match=$SO_MATCH
process=$PROC_STATE
EOF

python3 - "$SCRIPT_DIR/device_verify.txt" "$SCRIPT_DIR/device_verify.json" "$(date '+%Y-%m-%d %H:%M:%S')" <<'PY'
import json, sys
txt, out, ts = sys.argv[1], sys.argv[2], sys.argv[3]
kv = {}
for line in open(txt, encoding="utf-8"):
    line = line.rstrip("\n")
    if not line or line.startswith("#") or "=" not in line:
        continue
    k, v = line.split("=", 1)
    kv[k.strip()] = v.strip()
data = {"game": "动物大联盟", "mode": "device_verify_readonly", "at": ts, "facts": kv}
json.dump(data, open(out, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
print(json.dumps({"apk_match": kv.get("apk_match"), "so_match": kv.get("so_match"),
                  "process": kv.get("process"), "versionCode": kv.get("versionCode"),
                  "firstInstallTime": kv.get("firstInstallTime"),
                  "lastUpdateTime": kv.get("lastUpdateTime")}, ensure_ascii=False))
PY
