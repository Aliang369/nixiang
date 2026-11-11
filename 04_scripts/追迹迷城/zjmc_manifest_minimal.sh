#!/usr/bin/env bash
# script-policy: {"purpose": "追迹迷城 1.0.20261111 生成并校验「单轨最小补丁」二进制 AndroidManifest 缓存（不改 appComponentFactory）", "inputs": "04_scripts/追迹迷城/official_apk.json、patch_zjmc_binary_manifest_v5_minimal.py、04_scripts/_common/axml_inspect.py", "outputs": "07_patched/追迹迷城/surgical_1.0.20261111_singletrack/AndroidManifest.xml", "boundary": "只生成 Manifest 缓存；不打包、不签名、不装机；不改 v3/v4 历史补丁器与其缓存", "prior_art": "04_scripts/追迹迷城/zjmc_manifest.sh（同族 v4 版，本文件仅换补丁器与缓存目录）", "difference": "只换补丁器为 v5 最小集（删 Factory 改写），缓存目录加 _singletrack 隔离", "evidence": "10_docs/追迹迷城/路线锁定.md#单轨"}
# 追迹迷城 1.0.20261111：单轨最小 Manifest 补丁缓存
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
PATCHER="$SCRIPT_DIR/patch_zjmc_binary_manifest_v5_minimal.py"
AXML_INSPECT="$NIXIANG_ROOT/04_scripts/_common/axml_inspect.py"
SUFFIX="singletrack"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
[[ -f "$PATCHER" ]] || { echo "错误: 缺少 $PATCHER" >&2; exit 1; }
[[ -f "$AXML_INSPECT" ]] || { echo "错误: 缺少 $AXML_INSPECT" >&2; exit 1; }

read -r APK VERSION < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
print(d["apk_path"], d["version_name"])
PY
)

CACHE_DIR="$NIXIANG_ROOT/07_patched/追迹迷城/surgical_${VERSION}_${SUFFIX}"
OUT_XML="$CACHE_DIR/AndroidManifest.xml"
TMP_XML="$(mktemp /tmp/zjmc_manifest_min.XXXXXX.xml)"
TMP_TREE="$(mktemp /tmp/zjmc_manifest_tree.XXXXXX.txt)"
# 底包原 Manifest 解码副本也走 mktemp（2026-11-11 修复：原固定 /tmp/zjmc_manifest_orig_check.xml
# 非唯一且不清理，会与其它作业/并发会话互相覆盖）
TMP_ORIG="$(mktemp /tmp/zjmc_manifest_orig.XXXXXX.xml)"
trap 'rm -f "$TMP_XML" "$TMP_TREE" "$TMP_ORIG"' EXIT

mkdir -p "$CACHE_DIR"
unzip -p "$APK" AndroidManifest.xml > "$TMP_XML"
python3 "$PATCHER" "$TMP_XML" "$OUT_XML"

# 结构断言：真 AXML 头 + 平台 res id 未错位（axml_inspect 的 --assert-map 是真值来源机检）
python3 "$AXML_INSPECT" "$OUT_XML" --assert-map --quiet >/dev/null
python3 "$AXML_INSPECT" "$OUT_XML" > "$TMP_TREE" 2>/dev/null

python3 - "$OUT_XML" "$TMP_TREE" "$APK" "$NIXIANG_ROOT" "$TMP_ORIG" <<'PY'
import subprocess, sys, zipfile
xml, tree, apk, root, orig_xml = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5]

data = open(xml, "rb").read()
assert data[:4] == b"\x03\x00\x08\x00", data[:4].hex()

lines = open(tree, encoding="utf-8").read().splitlines()

def count(prefix):
    return sum(1 for l in lines if l.startswith(prefix))

app_block = []
inside = False
for l in lines:
    if l.startswith("E: application"):
        inside = True
        continue
    if inside and l.startswith("E: "):
        break
    if inside:
        app_block.append(l)
assert app_block, "application 子元素为空"
factory = [l for l in app_block if "appComponentFactory" in l]
assert len(factory) == 1, "application 缺 appComponentFactory（单轨要求保留原值）"
assert '"androidx.core.app.CoreComponentFactory"' in factory[0], factory[0]
assert not [l for l in app_block if "android:name" in l], "application 不得注入 android:name"
assert "StarryAppComponentFactory".encode("utf-16-le") not in data, "不得挂 killer 工厂"
assert "SignSpoofAppComponentFactory".encode("utf-16-le") not in data, "不得挂 SignSpoof 工厂"

need = (
    "com.starry.magic.StarryInitProvider",
    "com.TeaMelo.DungeonChasers.starryinit",
    "com.starry.magic.menu.MenuService",
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
    "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE",
)
for s in need:
    assert s.encode("utf-16-le") in data, "缺 " + s

# provider/service/权限只增不减（对照底包现解）
with zipfile.ZipFile(apk) as z:
    open(orig_xml, "wb").write(z.read("AndroidManifest.xml"))
orig_tree = subprocess.run(
    [sys.executable, root + "/04_scripts/_common/axml_inspect.py", orig_xml],
    check=True, capture_output=True, text=True).stdout.splitlines()
for el, want_delta in (("E: provider", 1), ("E: service", 1), ("E: activity", 0), ("E: uses-permission", 2)):
    o, n = sum(1 for l in orig_tree if l.startswith(el)), count(el)
    assert n - o == want_delta, "%s 增量 %d，期望 %d（%d -> %d）" % (el, n - o, want_delta, o, n)
    print("  %s: %d -> %d" % (el, o, n))
print("manifest binary validation ok（Factory 原值保留 / provider+1 / service+1 / 权限+2）")
PY

cat <<EOF
version=$VERSION
source_apk=$APK
patched_manifest=$OUT_XML
md5=$(md5 -q "$OUT_XML")
size=$(stat -f '%z' "$OUT_XML")
EOF
