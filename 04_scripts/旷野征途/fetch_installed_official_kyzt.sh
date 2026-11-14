#!/usr/bin/env bash
# 旷野征途：只读提取真机当前安装的原版（TapTap 原版）base APK 到 02_apks/旷野征途/。
# 版本无关：不写死 versionName/versionCode（2026-11-14 起，设备实装 2.2/vc22）。
# 先例：04_scripts/迷雾大陆/fetch_official_mwdl.sh（最小适配，门禁保持不变）。
#
# 门禁：
#   1) 单 base APK：pm path 多路径直接拒收（防漏 split）
#   2) 官方性：扫描全部 ABI 的 libstarry_magic.so / libwbmod.so / libwbtext.so，命中即拒收
#   3) 官方签名基线：apksigner 现取 signer sha256，必须等于已知官方 baseline
#      （基线来源：2026-11-14 自 f9a116a2 提取 2.2 实测；DN O=xiefeng，与 1.8 官方包同一密钥）
#   4) 原子写元数据：.tmp -> os.replace
#   5) 同名不同 hash 拒覆盖（原包只读，不覆盖既有输入）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"

PKG="${PKG:-com.xf.kyzt}"
GAME="旷野征途"
SERIAL="${SERIAL:-f9a116a2}"
ADB_BIN="${ADB_BIN:-adb}"
META_FILE="$SCRIPT_DIR/official_apk.json"
# 官方签名基线（2.2 实测；换密钥必须显式改这里并在 10_docs/旷野征途/ 记录依据）
OFFICIAL_SIGNER_SHA256="${OFFICIAL_SIGNER_SHA256:-0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
[[ "$("$ADB_BIN" -s "$SERIAL" get-state 2>/dev/null | tr -d '\r')" == "device" ]] \
  || { echo "错误: 设备 $SERIAL 不在 device 状态" >&2; exit 1; }

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')" || { echo "错误: dumpsys 失败" >&2; exit 1; }
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"

[[ -n "$VERSION_NAME" ]] || { echo "错误: 读取 versionName 失败（$PKG 未安装？）" >&2; exit 1; }
[[ "$VERSION_NAME" =~ ^[A-Za-z0-9._-]+$ ]] || { echo "错误: versionName 含不安全字符: $VERSION_NAME" >&2; exit 1; }

PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$PATH_COUNT" -ne 1 ]]; then
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径，拒绝只拉其一（会漏 split）:" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
fi
REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in
  /data/app/*) ;;
  *) echo "错误: 非预期安装路径: $REMOTE_APK" >&2; exit 1 ;;
esac

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_taptap_official.apk"
TMP="$OUT.part"
mkdir -p "$OUT_DIR"
rm -f "$TMP"
trap 'rm -f "$TMP"' EXIT

NEED_PULL=1
if [[ -e "$OUT" ]]; then
  # 已有同名输入：先比设备侧 md5（只读），一致则不重拉、不覆盖
  DEV_MD5="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'md5sum $REMOTE_APK'" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  LOCAL_MD5="$(md5 -q "$OUT")"
  if [[ -n "$DEV_MD5" && "$DEV_MD5" == "$LOCAL_MD5" ]]; then
    echo "[fetch] 已存在且与设备一致，不覆盖: $OUT"
    NEED_PULL=0
  else
    echo "错误: 本地已有同名 APK 且与真机不一致，拒绝覆盖: $OUT" >&2
    echo "  本地 $LOCAL_MD5 / 设备 $DEV_MD5" >&2
    exit 1
  fi
fi

if [[ "$NEED_PULL" -eq 1 ]]; then
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"
  unzip -tq "$TMP" >/dev/null || { echo "错误: 拉取的 APK 无法通过 zip 完整性检查" >&2; exit 1; }
  STARRY_HITS="$(unzip -Z1 "$TMP" | grep -E '^lib/[^/]+/(libstarry_magic|libwbmod|libwbtext)\.so$' || true)"
  if [[ -n "$STARRY_HITS" ]]; then
    echo "错误: 设备上装的是改包/星空版（含注入 so），不是官方原版；拒绝入库:" >&2
    printf '%s\n' "$STARRY_HITS" >&2
    exit 1
  fi
  mv "$TMP" "$OUT"
fi

SIGNER_DN=""; SIGNER_SHA256=""; SIGNER_SHA1=""
if [[ -n "${NIXIANG_APKSIGNER:-}" ]]; then
  CERTS="$("$NIXIANG_APKSIGNER" verify --print-certs "$OUT" 2>/dev/null || true)"
  SIGNER_DN="$(printf '%s\n' "$CERTS" | sed -n 's/^Signer #1 certificate DN: //p' | head -1)"
  SIGNER_SHA256="$(printf '%s\n' "$CERTS" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
  SIGNER_SHA1="$(printf '%s\n' "$CERTS" | sed -n 's/^Signer #1 certificate SHA-1 digest: //p' | head -1)"
fi
[[ -n "$SIGNER_SHA256" ]] || { echo "错误: 无法读取包签名（apksigner 缺失或验签失败）" >&2; exit 1; }
if [[ "$SIGNER_SHA256" != "$OFFICIAL_SIGNER_SHA256" ]]; then
  echo "错误: 签名 SHA-256 与官方基线不符 —— 这不是已知官方包:" >&2
  echo "  实测: $SIGNER_SHA256" >&2
  echo "  基线: $OFFICIAL_SIGNER_SHA256" >&2
  exit 1
fi

python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" \
  "$SERIAL" "$REMOTE_APK" "$SIGNER_DN" "$SIGNER_SHA256" "$SIGNER_SHA1" "$OFFICIAL_SIGNER_SHA256" <<'PYMETA'
import hashlib, json, os, sys, zipfile
from pathlib import Path

(meta, apk, vn, vc, installer, abi, serial, remote, signer_dn, signer_sha256,
 signer_sha1, baseline) = sys.argv[1:]
p = Path(apk)
raw = p.read_bytes()
md5 = hashlib.md5(raw).hexdigest()
sha256 = hashlib.sha256(raw).hexdigest()
size = os.stat(p).st_size
with zipfile.ZipFile(p) as z:
    names = z.namelist()
dex_count = sum(1 for n in names if n.startswith('classes') and n.endswith('.dex'))
so_count = sum(1 for n in names if n.startswith('lib/') and n.endswith('.so'))
starry = [n for n in names if n.endswith(('/libstarry_magic.so', '/libwbmod.so', '/libwbtext.so'))]
so_sizes = {}
for n in names:
    if n in ('lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
             'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
        so_sizes[n] = z.getinfo(n).file_size
data = {
    "package": "com.xf.kyzt",
    "game": "旷野征途",
    "channel": "taptap",
    "kind": "official_from_device",
    "apk_path": apk,
    "apk_exists": p.is_file(),
    "version_name": vn,
    "version_code": int(vc) if vc.isdigit() else vc,
    "installer": installer,
    "primary_cpu_abi": abi,
    "serial": serial,
    "remote_path": remote,
    "size": size,
    "sha256": sha256,
    "md5": md5,
    "signer_dn": signer_dn,
    "signer_sha256": signer_sha256,
    "signer_sha1": signer_sha1,
    "official_signer_sha256_baseline": baseline,
    "cert_baseline_ok": signer_sha256 == baseline,
    "no_starry_lib": not starry,
    "starry_hits": starry,
    "so_count": so_count,
    "dex_count": dex_count,
    "key_entry_sizes": so_sizes,
}
tmp = Path(str(meta) + ".tmp")
tmp.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
os.replace(tmp, meta)
print(f"size={size}\nmd5={md5}\nsha256={sha256}")
print(f"cert_baseline_ok={data['cert_baseline_ok']} no_starry_lib={data['no_starry_lib']} dex={dex_count} so={so_count}")
PYMETA

cat <<EOF
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
installer=$INSTALLER
primaryCpuAbi=$PRIMARY_ABI
serial=$SERIAL
remote=$REMOTE_APK
output=$OUT
signerDN=$SIGNER_DN
signerSHA256=$SIGNER_SHA256
signerSHA1=$SIGNER_SHA1
meta=$META_FILE
EOF
echo "DONE: $OUT"
