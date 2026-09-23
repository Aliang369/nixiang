#!/usr/bin/env bash
# 镇邪人 1.0.3 ca0d4 修复版候选包验收：
#   1) 包身份/签名
#   2) 菜单 dex/so/资源完整（沿用 09-16 星光菜单）
#   3) 战斗补丁已进 ca0d4 与 d5507 两个 jsc
#   4) 崩溃补丁已彻底回滚（jsb-engine.js / main.js == 原版）
#   5) 关键回归：服务器 src 远端 URL 能被引擎本地优先查找命中（不再进原生下载器）
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_ca0d4_safe-releaseSigned.apk}"
ORIG_APK="${ZXR_ORIG_APK:-$ROOT/09_test/镇邪人_orig_20260923/apk/orig_base.apk}"
SRC_JSC="${ZXR_PATCHED_SRC_JSC:-$ROOT/07_patched/镇邪人/ca0d4/index.ca0d4.jsc}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: original APK missing: $ORIG_APK" >&2; exit 1; }
[[ -f "$SRC_JSC" ]] || { echo "ERROR: patched ca0d4 JSC missing: $SRC_JSC" >&2; exit 1; }

echo "[1/5] 包身份与签名"
"$AAPT2" dump badging "$APK" | grep -F "package: name='com.zxr.dlxmz.zxrwz37zgx10.gg9b' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify --verbose "$APK" | tail -3
[[ "$(unzip -p "$APK" assets/assets/src/index.ca0d4.jsc | shasum -a 256 | awk '{print $1}')" == "$(shasum -a 256 "$SRC_JSC" | awk '{print $1}')" ]] \
  || { echo "ERROR: ca0d4 jsc in APK != patched artifact" >&2; exit 1; }
echo "  ca0d4 jsc == 07_patched 产物"

echo "[2/5] 菜单/so/资源"
python3.11 - "$APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8, "classes8 非 NativeMenu"
assert b"mod-menu-v2.html" not in c8, "classes8 仍是 WebView 菜单"
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zxr]" in so and b"zxr\0" in so, "so 不是 ZXR"
assert "lib/arm64-v8a/libc++_shared.so" in z.namelist()
assert "assets/certs/starlight.crt" in z.namelist()
mf = z.read("AndroidManifest.xml")   # 二进制 XML：字符串为 UTF-16LE 存储
assert "com.starry.magic.menu.MenuService".encode("utf-16-le") in mf, "Manifest 缺 MenuService"
print("  classes8=NativeMenu+ZxrHooks; so=[Zxr]+zxr; MenuService ok")
PY

echo "[3/5] 战斗补丁落位（ca0d4 + d5507 双份）"
"$PYTHON_BIN" - "$APK" <<'PY'
import sys, zipfile, zlib, xxtea
z = zipfile.ZipFile(sys.argv[1])
key = b"c25d728a-a9d9-4c"
for entry in ("assets/assets/src/index.ca0d4.jsc", "assets/assets/src/index.d5507.jsc"):
    packed = xxtea.decrypt(z.read(entry), key, padding=False)
    js = zlib.decompress(packed, 16 + zlib.MAX_WBITS)
    assert b"starry_zxr_safe_v2" in js, f"{entry} 缺 safe marker"
    for tok in (b"__starryZxrOn(1)", b"__starryZxrOn(2)", b"__starryZxrOn(3)", b"__starryZxrOn(4)"):
        assert tok in js, f"{entry} 缺 {tok!r}"
    print(f"  {entry}: decrypt ok, marker+4 hook ok")
PY

echo "[4/5] 崩溃补丁回滚核对（与原版逐字节一致）"
"$PYTHON_BIN" - "$APK" "$ORIG_APK" <<'PY'
import sys, zipfile
new, orig = (zipfile.ZipFile(p) for p in sys.argv[1:3])
for n in ("assets/jsb-adapter/jsb-engine.js", "assets/main.js"):
    a, b = new.read(n), orig.read(n)
    assert a == b, f"{n} 与原版不一致（崩溃补丁未完全回滚）"
    print(f"  {n}: == 原版 ({len(a)}B)")
eng = new.read("assets/jsb-adapter/jsb-engine.js")
assert b'if (bundleName === "src")' not in eng
assert b"starry_zxr_main_guard_v1" not in new.read("assets/main.js")
assert not [n for n in new.namelist() if "b5278" in n], "b5278 残留"
assert not [n for n in new.namelist() if n.startswith("assets/web")], "assets/web 残留"
print("  引擎/main.js 无相对路径路由、无守卫；无 b5278/web 残留")
PY

echo "[5/5] 关键回归：远端 src URL → 包内本地命中（不再进 Cocos2dxDownloader）"
"$PYTHON_BIN" - "$APK" <<'PY'
import json, sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
# 复刻 jsb-engine.js 的 getPackagePathFromRemoteUrl + checkExistsInPackage
REMOTE = "https://ymzxr-cn-res.lansors.com/ymzxr_Android_prod_res/remote/"
def package_path(remote_url):
    path = remote_url
    i = path.find("/remote/")
    if i == -1:
        return ""
    return "assets/" + path[i + len("/remote/"):]
failed = []
for ver in ("ca0d4", "d5507"):
    for tail in (f"config.{ver}.json", f"index.{ver}.jsc"):
        cp = package_path(REMOTE + "src/" + tail)
        apk_entry = "assets/" + cp          # cocos 资源根 = APK 的 assets/
        ok = apk_entry in names
        print(f"  {REMOTE}src/{tail}\n    → cocos {cp} → APK {apk_entry} : {'命中' if ok else '未命中(会走网络下载)'}")
        if not ok:
            failed.append(tail)
assert not [f for f in failed if "ca0d4" in f], f"ca0d4 未本地命中: {failed}"
print("  ca0d4 全套本地命中 ⇒ 引擎 inLocal，永不触发原生下载 NPE")
PY

echo "verify=pass"
