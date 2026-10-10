#!/usr/bin/env bash
# 追迹迷城：生成并校验二进制 AndroidManifest 补丁缓存。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
# v3（2026-10-10）：1.0.20261009 起穿山甲 33/34 字符 service 槽已从 Manifest 与全部 dex
# 消失，v2 的等长槽替换路径必然失败 ⇒ 改为「凭空新增 <service> 元素」（见 v3 文件头）。
PATCHER="$SCRIPT_DIR/patch_zjmc_binary_manifest_v3.py"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
[[ -f "$PATCHER" ]] || { echo "错误: 缺少 $PATCHER" >&2; exit 1; }

while IFS=$'\t' read -r key value; do
  case "$key" in
    apk_path) APK="$value" ;;
    version_name) VERSION="$value" ;;
  esac
done < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
for k in ("apk_path", "version_name"):
    print(f"{k}\t{d[k]}")
PY
)

CACHE_DIR="$NIXIANG_ROOT/07_patched/追迹迷城/surgical_$VERSION"
OUT_XML="$CACHE_DIR/AndroidManifest.xml"
TMP_XML="$(mktemp /tmp/zjmc_manifest.XXXXXX.xml)"
trap 'rm -f "$TMP_XML"' EXIT

mkdir -p "$CACHE_DIR"
unzip -p "$APK" AndroidManifest.xml > "$TMP_XML"
python3 "$PATCHER" "$TMP_XML" "$OUT_XML"

python3 - "$OUT_XML" <<'PY'
import struct
import sys

path = sys.argv[1]
data = open(path, "rb").read()
assert data[:4] == b"\x03\x00\x08\x00", data[:4].hex()

# Minimal AXML validation: expected strings exist and forbidden original slot is gone.
for text in (
    "com.starry.magic.StarryApp",
    "com.starry.magic.StarryInitProvider",
    "com.TeaMelo.DungeonChasers.starryinit",
    "com.starry.magic.menu.MenuService",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
    "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE",
    "foregroundServiceType",
    "stopWithTask",
    "specialUse",
):
    assert text.encode("utf-16-le") in data, text

# resource map 按「字符串索引稠密」语义回读：属性 res id 必须正确，
# 否则属性对平台等于不存在（v3 首版 off-by-2 缺陷即此，aapt2 复读看不出来）。
def pool_strings(d, base=8):
    # AXML 文件头 8 字节（type=0x0003, headerSize=8），字符串池是紧随其后的第一个 chunk
    assert struct.unpack_from("<H", d, base)[0] == 0x0001, hex(struct.unpack_from("<H", d, base)[0])
    cnt = struct.unpack_from("<I", d, base + 8)[0]
    str_start = base + struct.unpack_from("<I", d, base + 20)[0]
    offs = [struct.unpack_from("<I", d, base + 28 + 4 * i)[0] for i in range(cnt)]
    out = []
    for o in offs:
        p = str_start + o
        n = struct.unpack_from("<H", d, p)[0]
        out.append(d[p + 2:p + 2 + 2 * n].decode("utf-16-le"))
    return out

strs = pool_strings(data)
q = 8 + struct.unpack_from("<I", data, 12)[0]   # 池 chunk 的 size ⇒ 下一个 chunk 起点
t, _hs, sz = struct.unpack_from("<HHI", data, q)
while t != 0x0180:  # RES_XML_RESOURCE_MAP_TYPE
    q += sz
    t, _hs, sz = struct.unpack_from("<HHI", data, q)
rmap = [struct.unpack_from("<I", data, q + 8 + 4 * i)[0] for i in range((sz - 8) // 4)]
for an, want in (("name", 0x01010003), ("exported", 0x01010010),
                 ("foregroundServiceType", 0x01010599), ("stopWithTask", 0x01010281)):
    i = strs.index(an)
    got = rmap[i] if i < len(rmap) else 0
    assert got == want, f"resmap[{i}]={an} 得 0x{got:X}，期望 0x{want:X}"

# 1.0.20261009 起壳内已无穿山甲 service 槽；此处只作「不应出现」的反向断言。
assert "com.byazt.b.DownloadHandleService".encode("utf-16-le") not in data
assert data.count("android.permission.SYSTEM_ALERT_WINDOW".encode("utf-16-le")) == 1
assert data.count("com.starry.magic.menu.MenuService".encode("utf-16-le")) == 1
assert data.count("com.starry.magic.StarryInitProvider".encode("utf-16-le")) == 1

print("manifest binary validation ok")
PY

cat <<EOF
version=$VERSION
source_apk=$APK
patched_manifest=$OUT_XML
md5=$(md5 -q "$OUT_XML")
size=$(stat -f '%z' "$OUT_XML")
EOF
