#!/usr/bin/env bash
# 爆裂防线：从底包 APK 静态提取 HybridCLR 热更 DLL（UnityFS 解包 + PE 剖分）。
# 只读底包、只写 06_dumped 下指定版本目录；不启动游戏、不碰设备。
# 用法:
#   BLFX_BASE_APK=<底包路径> BLFX_VERSION=<版本> bash blfx_extract_hotfix.sh
# 默认底包 = 02_apks/爆裂防线/爆裂防线_${BLFX_VERSION}_original_from_device.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="爆裂防线"
PKG="${PKG:-com.muyegame.blfx}"
VERSION="${BLFX_VERSION:?需要 BLFX_VERSION，例如 1.02.01}"
BASE_APK="${BLFX_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_original_from_device.apk}"
OUTDIR="${BLFX_OUTDIR:-$NIXIANG/06_dumped/$GAME/hotfix_${VERSION}}"
UNITYFS="$SCRIPT_DIR/blfx_unityfs_extract.py"
AB_NAME="assets/assetbundle/hotfixdlls/android/scriptdll/scriptdll.ab"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$UNITYFS" ]] || { echo "ERROR: missing $UNITYFS" >&2; exit 1; }

WORK="$(mktemp -d /tmp/blfx_hotfix.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/3] unzip -p $AB_NAME"
unzip -p "$BASE_APK" "$AB_NAME" > "$WORK/scriptdll.ab"

echo "[2/3] UnityFS 解包（纯 Python LZ4）"
python3 "$UNITYFS" "$WORK/scriptdll.ab" "$WORK/decompressed.bin" >/dev/null

echo "[3/3] PE 剖分 hotfix_script.dll"
python3 - "$WORK/decompressed.bin" "$WORK/hotfix_script.dll" <<'PY'
import struct, sys
d = open(sys.argv[1], 'rb').read()
mz = d.find(b'MZ')
if mz < 0:
    raise SystemExit('no MZ found in decompressed bundle')
e = struct.unpack_from('<I', d, mz + 0x3c)[0]
assert d[mz + e:mz + e + 4] == b'PE\0\0', 'bad PE signature'
nsec = struct.unpack_from('<H', d, mz + e + 6)[0]
optsz = struct.unpack_from('<H', d, mz + e + 20)[0]
oh = mz + e + 24
end = struct.unpack_from('<I', d, oh + 60)[0]  # SizeOfHeaders
sec = oh + optsz
for i in range(nsec):
    _, _, rsz, raddr = struct.unpack_from('<IIII', d, sec + i * 40 + 8)
    end = max(end, raddr + rsz)
open(sys.argv[2], 'wb').write(d[mz:mz + end])
print(f'carved {end} bytes (MZ@{mz:#x}, sections={nsec})')
PY

mkdir -p "$OUTDIR"
cp -f "$WORK/hotfix_script.dll" "$OUTDIR/hotfix_script.dll"
echo "out=$OUTDIR/hotfix_script.dll"
echo "size=$(stat -f %z "$OUTDIR/hotfix_script.dll")"
echo "md5=$(md5 -q "$OUTDIR/hotfix_script.dll")"
