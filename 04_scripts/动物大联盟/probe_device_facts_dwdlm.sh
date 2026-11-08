#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 设备事实只读探测（型号 / Android 版本 / SDK / 包状态 / 空间 / 外置目录）
#
# 口径（2026-11-05 装机轮）：
#   * **完全只读**：只用 getprop / pm path / dumpsys / df / ls / du；不安装、不卸载、不清数据、
#     不 force-stop、不启动、不 screencap、不 input tap、不写设备文件。
#   * 目的：纠正历史文档里「Android 13 / 16」与型号不一致的记录 —— 一律以本脚本当轮实测为准。
#   * 产物：04_scripts/动物大联盟/device_facts.json + device_facts.txt（机读 + 人读）。
#
# 用法：
#   bash 04_scripts/动物大联盟/probe_device_facts_dwdlm.sh [--serial f9a116a2]
set -euo pipefail
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin:${PATH:-}"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SERIAL=""
while [ $# -gt 0 ]; do
  case "$1" in
    --serial) SERIAL="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,12p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $1" >&2; exit 2 ;;
  esac
done
[ -n "$SERIAL" ] || SERIAL="$(cat "${SCRIPT_DIR}/.device_serial" 2>/dev/null || true)"
[ -n "$SERIAL" ] || { echo "ERROR: 无 serial（先跑 device_ready_dwdlm.sh）" >&2; exit 1; }
PKG="${PKG:-com.fightingstars.games.animal}"
OUT_JSON="${SCRIPT_DIR}/device_facts.json"
OUT_TXT="${SCRIPT_DIR}/device_facts.txt"
ADB_BIN="${ADB_BIN:-adb}"
command -v "$ADB_BIN" >/dev/null 2>&1 || { echo "ERROR: adb 不可用（PATH=${PATH}）" >&2; exit 1; }

adbs() { "$ADB_BIN" -s "$SERIAL" shell "$@" 2>/dev/null | tr -d '\r'; }
prop() { adbs getprop "$1"; }

"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "ERROR: 设备 $SERIAL 不在线" >&2; exit 1; }

MODEL="$(prop ro.product.model)"
BRAND="$(prop ro.product.brand)"
MANUF="$(prop ro.product.manufacturer)"
DEVICE="$(prop ro.product.device)"
REL="$(prop ro.build.version.release)"
SDK="$(prop ro.build.version.sdk)"
INCR="$(prop ro.build.version.incremental)"
BUILDTYPE="$(prop ro.build.type)"
TAGS="$(prop ro.build.tags)"
ABI="$(prop ro.product.cpu.abi)"
FP="$(prop ro.build.fingerprint)"
PAGE="$(prop ro.product.cpu.page_size)"
ROOT_ID="$(adbs "su -c id")"
PM_PATH="$(adbs "pm path $PKG")"
# ⚠ 先落盘再 grep（pipefail 下 `dumpsys | grep -m1` 会让 dumpsys 吃 SIGPIPE(141) ⇒ 整脚本中断）
PKGDUMP="/tmp/dwdlm_pkgdump.$$.txt"
adbs "dumpsys package $PKG" > "$PKGDUMP" || true
VER="$(grep -E 'versionName=|versionCode=' "$PKGDUMP" | head -3 | tr -d ' ' || true)"
SRC_DIR="$(grep -m1 'codePath=' "$PKGDUMP" | tr -d ' ' || true)"
CERT="$(grep -m1 -i 'signatures' "$PKGDUMP" | tr -d '\r' || true)"
rm -f "$PKGDUMP"
DF_DATA="$(adbs "df -k /data" | tail -1)"
DF_SDCARD="$(adbs "df -k /storage/emulated/0" | tail -1)"
DF_TMP="$(adbs "df -k /data/local/tmp" | tail -1)"
EXT_DATA="/storage/emulated/0/Android/data/$PKG"
EXT_OBB="/storage/emulated/0/Android/obb/$PKG"
EXT_DATA_STATE="absent"; EXT_DATA_KB="0"
EXT_OBB_STATE="absent";  EXT_OBB_KB="0"
if [ -n "$(adbs "su -c 'ls -d $EXT_DATA 2>/dev/null'")" ]; then
  EXT_DATA_STATE="present"
  EXT_DATA_KB="$(adbs "su -c 'du -sk $EXT_DATA 2>/dev/null'" | awk '{print $1}')"
fi
if [ -n "$(adbs "su -c 'ls -d $EXT_OBB 2>/dev/null'")" ]; then
  EXT_OBB_STATE="present"
  EXT_OBB_KB="$(adbs "su -c 'du -sk $EXT_OBB 2>/dev/null'" | awk '{print $1}')"
fi

ROOT_STATE="unavailable"
case "$ROOT_ID" in *uid=0*) ROOT_STATE="available" ;; esac
PKG_INSTALLED="no"
if [ -n "$PM_PATH" ]; then PKG_INSTALLED="yes"; fi

cat > "$OUT_TXT" <<EOF
# 动物大联盟 设备事实（只读探测，$(date '+%Y-%m-%d %H:%M:%S')）
serial=${SERIAL}
root=${ROOT_STATE}
root_id=${ROOT_ID:-<empty>}
model=${MODEL}
brand=${BRAND}
manufacturer=${MANUF}
device=${DEVICE}
android_release=${REL}
sdk=${SDK}
build_incremental=${INCR}
build_type=${BUILDTYPE}
build_tags=${TAGS}
cpu_abi=${ABI}
page_size=${PAGE}
fingerprint=${FP}
pkg=${PKG}
pkg_installed=${PKG_INSTALLED}
pm_path=${PM_PATH:-<none>}
version=${VER:-<none>}
source_dir=${SRC_DIR:-<none>}
signatures=${CERT:-<none>}
df_data=${DF_DATA}
df_sdcard=${DF_SDCARD}
df_tmp=${DF_TMP}
ext_data=${EXT_DATA_STATE} (${EXT_DATA_KB} KB)
ext_obb=${EXT_OBB_STATE} (${EXT_OBB_KB} KB)
EOF

python3 - "$OUT_TXT" "$OUT_JSON" <<'PY'
import json, sys
txt, out = sys.argv[1], sys.argv[2]
kv = {}
for line in open(txt, encoding="utf-8"):
    line = line.rstrip("\n")
    if not line or line.startswith("#") or "=" not in line:
        continue
    k, v = line.split("=", 1)
    kv[k.strip()] = v.strip()
data = {
    "game": "动物大联盟",
    "pkg": kv.get("pkg"),
    "serial": kv.get("serial"),
    "device": {
        "model": kv.get("model"), "brand": kv.get("brand"),
        "manufacturer": kv.get("manufacturer"), "device": kv.get("device"),
        "android_release": kv.get("android_release"), "sdk": kv.get("sdk"),
        "build_incremental": kv.get("build_incremental"),
        "build_type": kv.get("build_type"), "build_tags": kv.get("build_tags"),
        "cpu_abi": kv.get("cpu_abi"), "page_size": kv.get("page_size"),
        "fingerprint": kv.get("fingerprint"),
        "root": kv.get("root"), "root_id": kv.get("root_id"),
    },
    "package_state": {
        "installed": kv.get("pkg_installed") == "yes",
        "pm_path": kv.get("pm_path"), "version": kv.get("version"),
        "source_dir": kv.get("source_dir"), "signatures": kv.get("signatures"),
    },
    "storage": {
        "df_data": kv.get("df_data"), "df_sdcard": kv.get("df_sdcard"),
        "df_tmp": kv.get("df_tmp"),
        "ext_data": kv.get("ext_data"), "ext_obb": kv.get("ext_obb"),
    },
    "readonly": True,
    "probe_at": __import__("time").strftime("%Y-%m-%d %H:%M:%S"),
}
json.dump(data, open(out, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
print("device_facts.json ->", out)
PY

echo "---- 设备事实（只读）----"
cat "$OUT_TXT"
