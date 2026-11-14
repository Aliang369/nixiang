#!/usr/bin/env bash
# 迷雾大陆：只读提取真机当前安装的官方原版 base APK 到 02_apks/迷雾大陆/。
# 版本无关（2026-11-14 起）：不写死 versionName/versionCode。
# 旧 fetch_official_15.sh 硬编码 15.0.0/vc20 且输出到 06_dumped，仅作历史留档。
#
# 门禁（2026-11-14 加强，按 lead/reviewer 要求）：
#   1) 单 base APK：pm path 多路径直接拒收（防漏 split）
#   2) 官方性：**扫描全部 ABI** 的 libstarry_magic.so，命中即拒收
#   3) 官方签名基线：apksigner 现取 signer sha256，必须等于已知官方 baseline
#      （16.0.0 与 15.0.0 同一签名密钥：67cf2df1…，由 06_dumped/…/official_15.0.0_cert.txt 对照）
#   4) 原子写元数据：.tmp → os.replace，避免半截 JSON 被下游当证据
#   5) 同名不同 hash 拒覆盖
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"

PKG="${PKG:-com.hortor.mwdl}"
GAME="迷雾大陆"
SERIAL="${SERIAL:-f9a116a2}"
ADB_BIN="${ADB_BIN:-adb}"
META_FILE="$SCRIPT_DIR/official_apk.json"
# 官方签名基线（16.0.0 实测 = 15.0.0 = 67cf2df1…；换密钥必须显式改这里并记录依据）
OFFICIAL_SIGNER_SHA256="${OFFICIAL_SIGNER_SHA256:-67cf2df158336cc0310613041d3f12f6cd98f07e0f9822d4c4164fe08f3ecb04}"

if [[ -n "$OFFICIAL_SIGNER_SHA256:-}" ]]; then
  echo "WARN: OFFICIAL_SIGNER_SHA256 被环境变量覆盖 —— 偏离官方基线，需在文档说明依据" >&2
fi

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

"$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"
unzip -tq "$TMP" >/dev/null || { echo "错误: 拉取的 APK 无法通过 zip 完整性检查" >&2; exit 1; }

# 2) 官方性：全部 ABI 的 starry so 一律拒收（不能只看 arm64-v8a）
STARRY_HITS="$(unzip -Z1 "$TMP" | grep -E '^lib/[^/]+/libstarry_magic\.so$' || true)"
if [[ -n "$STARRY_HITS" ]]; then
  echo "错误: 设备上装的是星空版（含 libstarry_magic.so），不是官方原版；拒绝入库:" >&2
  printf '%s\n' "$STARRY_HITS" >&2
  exit 1
fi

if [[ -e "$OUT" ]]; then
  if [[ "$(md5 -q "$OUT")" != "$(md5 -q "$TMP")" ]]; then
    echo "错误: 本地已有同名 APK 且与真机不一致，拒绝覆盖: $OUT" >&2
    exit 1
  fi
  rm -f "$TMP"
else
  mv "$TMP" "$OUT"
fi

# 3) 官方签名基线断言
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

BADGING=""
if [[ -n "${NIXIANG_BUILD_TOOLS:-}" && -x "${NIXIANG_BUILD_TOOLS}/aapt" ]]; then
  BADGING="$("${NIXIANG_BUILD_TOOLS}/aapt" dump badging "$OUT" 2>/dev/null | sed -n '1,6p' || true)"
fi

# 4) 元数据：python 现算 hash/尺寸 + 原子写
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
starry = [n for n in names if n.endswith('/libstarry_magic.so')]
data = {
    "package": "com.hortor.mwdl",
    "game": "迷雾大陆",
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
[[ -n "$BADGING" ]] && printf '%s\n' "$BADGING"
echo "DONE: $OUT"
