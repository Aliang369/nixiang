#!/usr/bin/env bash
# 宝石战争：只换热更资产的外科重建（不动 dex / so / AndroidManifest / resources.arsc）。
# 用途：游戏热更版本前进（如 1.0.0.04 -> 1.0.0.06）后，把新版本全量资产 + 补丁 DLL 内嵌回包，
# 使包内 manifest 版本与 CDN 一致，避免启动时把外部官方 DLL 拉回覆盖补丁。
#
# 输入：
#   BSZZ_BASE_APK        已装星光正式包（默认 = 本轮从设备只读拉回的 base.apk）
#   BSZZ_HOTUPDATE_ROOT  官方新热更“根目录”（默认 = 本轮热更轮次目录，内含 Android/manifest.json）
#   BSZZ_PATCHED_NETWORK 补丁后的 System.Network.dll.bytes
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="宝石战争"
VERSION="${BSZZ_VERSION:-6.0.013_hu1.0.0.06}"

BASE_APK="${BSZZ_BASE_APK:-$NIXIANG/06_dumped/$GAME/6.0.013/current/base.apk}"
BASE_MD5_EXPECT="${BSZZ_BASE_APK_MD5:-81472a9e8933d6563a243325a79277da}"
HOT_ROOT="${BSZZ_HOTUPDATE_ROOT:-$NIXIANG/06_dumped/$GAME/6.0.013/hotupdate_1.0.0.06}"
PATCHED_NETWORK="${BSZZ_PATCHED_NETWORK:-$NIXIANG/07_patched/$GAME/6.0.013/hotupdate_1.0.0.06/assets/Android/Dlls/System.Network.dll.bytes}"
EXPECT_HOTUPDATE_VERSION="${BSZZ_EXPECT_HOTUPDATE_VERSION:-1.0.0.06}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SUMMARY_DIR="$NIXIANG/07_patched/$GAME/$VERSION"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ "$(md5 -q "$BASE_APK")" == "$BASE_MD5_EXPECT" ]] || { echo "ERROR: 底包指纹不符: $BASE_APK" >&2; exit 1; }
[[ -f "$HOT_ROOT/Android/manifest.json" ]] || { echo "ERROR: missing hotupdate manifest: $HOT_ROOT/Android/manifest.json" >&2; exit 1; }
[[ -f "$PATCHED_NETWORK" ]] || { echo "ERROR: missing patched network bytes: $PATCHED_NETWORK" >&2; exit 1; }

mkdir -p "$OUT" "$SUMMARY_DIR"

echo "[1/2] zip surgical merge -> $UNSIGNED"
python3 - "$BASE_APK" "$HOT_ROOT" "$PATCHED_NETWORK" "$UNSIGNED" "$EXPECT_HOTUPDATE_VERSION" <<'PY'
import hashlib, json, sys, zipfile
from pathlib import Path

base, hot_root, patched_network, dst, expect_version = sys.argv[1:6]
base = Path(base); hot_root = Path(hot_root); patched_network = Path(patched_network); dst = Path(dst)

official_manifest_path = hot_root / "Android/manifest.json"
official_manifest_bytes = official_manifest_path.read_bytes()
official_manifest = json.loads(official_manifest_bytes)
assert official_manifest.get("version") == expect_version, (
    f"hotupdate version mismatch: {official_manifest.get('version')} != {expect_version}")

patched_network_bytes = patched_network.read_bytes()
patched_network_md5 = hashlib.md5(patched_network_bytes).hexdigest()
patched_network_size = len(patched_network_bytes)

zin = zipfile.ZipFile(base, "r")
names = set(zin.namelist())

overrides = {}
from_delta, from_apk = [], []
for asset in official_manifest["files"]:
    rel = asset["path"]
    override_key = "assets/" + rel
    local = hot_root / rel
    if local.is_file():
        data = local.read_bytes()
        assert len(data) == asset["size"], f"delta size mismatch: {rel}"
        assert hashlib.md5(data).hexdigest() == asset["hash"], f"delta md5 mismatch: {rel}"
        from_delta.append(rel)
    else:
        assert override_key in names, f"base apk missing asset: {override_key}"
        data = zin.read(override_key)
        assert len(data) == asset["size"], f"base asset size mismatch: {rel}"
        assert hashlib.md5(data).hexdigest() == asset["hash"], f"base asset md5 mismatch: {rel}"
        from_apk.append(rel)
    overrides[override_key] = data

patched_network_key = "assets/Android/Dlls/System.Network.dll.bytes"
assert patched_network_key in overrides, "official manifest missing System.Network entry"
overrides[patched_network_key] = patched_network_bytes
overrides["assets/Android/manifest.json"] = official_manifest_bytes

untouched = [
    "classes.dex", "classes2.dex", "classes3.dex", "resources.arsc", "AndroidManifest.xml",
    "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so", "lib/arm64-v8a/libmain.so",
    "lib/arm64-v8a/libthemis.so", "lib/arm64-v8a/libstarry_magic.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
]
for n in untouched:
    assert n in names, f"base apk missing {n}"

def is_sig(name):
    if name == "META-INF/MANIFEST.MF":
        return True
    return name.startswith("META-INF/") and name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
with zipfile.ZipFile(dst, "w") as zout:
    for item in zin.infolist():
        name = item.filename
        if is_sig(name) or name.startswith("assets/Android/"):
            continue
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, zin.read(item))
    for name, data in sorted(overrides.items()):
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
zin.close()

zin2 = zipfile.ZipFile(base, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), f"changed: {name}"
expected_android = {f"assets/{a['path']}" for a in official_manifest["files"]}
expected_android.add("assets/Android/manifest.json")
actual_android = {n for n in zout2.namelist() if n.startswith("assets/Android/")}
assert actual_android == expected_android, (
    f"assets/Android mismatch: missing={sorted(expected_android - actual_android)}, "
    f"extra={sorted(actual_android - expected_android)}")
for asset in official_manifest["files"]:
    name = "assets/" + asset["path"]
    data = zout2.read(name)
    if name == patched_network_key:
        continue
    assert len(data) == asset["size"], f"packaged size mismatch: {name}"
    assert hashlib.md5(data).hexdigest() == asset["hash"], f"packaged md5 mismatch: {name}"
assert zout2.read("assets/Android/manifest.json") == official_manifest_bytes
assert len(zout2.read(patched_network_key)) == patched_network_size
assert hashlib.md5(zout2.read(patched_network_key)).hexdigest() == patched_network_md5
for name in kept_meta:
    assert name in zout2.namelist(), f"lost META-INF entry: {name}"
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist())
zin2.close(); zout2.close()

print(json.dumps({
    "hotupdate_version": official_manifest["version"],
    "assets_total": len(official_manifest["files"]),
    "assets_from_delta": from_delta,
    "assets_from_apk_count": len(from_apk),
    "patched_network_md5": patched_network_md5,
    "patched_network_size": patched_network_size,
}, ensure_ascii=False, indent=2))
PY

echo "[2/2] 产物校验"
IN_MANIFEST_VERSION="$(unzip -p "$UNSIGNED" assets/Android/manifest.json | python3 -c 'import json,sys; print(json.load(sys.stdin)["version"])')"
[[ "$IN_MANIFEST_VERSION" == "$EXPECT_HOTUPDATE_VERSION" ]] || { echo "ERROR: 包内 manifest 版本异常: $IN_MANIFEST_VERSION" >&2; exit 1; }
IN_NET_MD5="$(unzip -p "$UNSIGNED" assets/Android/Dlls/System.Network.dll.bytes | md5 -q)"
SRC_NET_MD5="$(md5 -q "$PATCHED_NETWORK")"
[[ "$IN_NET_MD5" == "$SRC_NET_MD5" ]] || { echo "ERROR: 包内 System.Network 与补丁产物不一致" >&2; exit 1; }

python3 - "$UNSIGNED" "$BASE_APK" "$PATCHED_NETWORK" "$SUMMARY_DIR/hotupdate_rebuild_summary.json" <<'PY'
import hashlib, json, sys
from pathlib import Path
unsigned, base, patched, summary = sys.argv[1:5]
un = Path(unsigned)

def digest(path, algo):
    h = hashlib.new(algo)
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

data = {
    "base_apk": str(Path(base).resolve()),
    "base_apk_md5": digest(base, "md5"),
    "unsigned_apk": str(un.resolve()),
    "unsigned_apk_size": un.stat().st_size,
    "unsigned_apk_md5": digest(un, "md5"),
    "unsigned_apk_sha256": digest(un, "sha256"),
    "patched_network_bytes_md5": digest(patched, "md5"),
    "patched_network_bytes_size": Path(patched).stat().st_size,
}
Path(summary).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

ls -lh "$UNSIGNED"
