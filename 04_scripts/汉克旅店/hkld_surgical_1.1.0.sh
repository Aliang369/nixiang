#!/usr/bin/env bash
# 汉克旅店 1.1.0 (vc337, com.chillyroom.hanksbar.yw, TapTap) — 外科组装出包
#
# 底包：02_apks/汉克旅店/汉克旅店_1.1.0_official.apk（927 MB，禁止 apktool d/b 整包）
# 产物：08_output/汉克旅店/汉克旅店_1.1.0_surgical-releaseSigned.apk
#
# 与常规 Unity 包的两点不同：
#   ① 玩法 hook 在**热更程序集**里（HybridCLR）⇒ so 内嵌 Cecil 补丁版 Assembly-CSharp.dll，
#      运行期在 System.Reflection.Assembly.Load(byte[]) 边界换字节。so 体积 ≈ 5.5 MB（含载荷）。
#   ② 入口点用 ContentProvider（com.starry.magic.StarryInitProvider），**不替换**官方
#      Application（com.chillyroomsdk.sdkbridge.BasePlayerApplication 是 ChillyRoom SDK 桥）。
#
# 用法:
#   bash 04_scripts/汉克旅店/hkld_surgical_1.1.0.sh              # 构建+签名+自检
#   HKLD_SKIP_GRADLE=1 bash ...                                  # 复用已构建的菜单 APK
#   STARRY_INSTALL=1 SERIAL=<serial> bash ...                    # 额外装机（未授权时勿用）
set -euo pipefail

VERSION="1.1.0"
PKG="com.chillyroom.hanksbar.yw"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
ORIG_APK="$NIXIANG/02_apks/汉克旅店/汉克旅店_1.1.0_official.apk"
OUT="$NIXIANG/08_output/汉克旅店"
SIGNED="$OUT/汉克旅店_${VERSION}_surgical-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/汉克旅店/surgical_${VERSION}/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
KS="$NIXIANG/01_tools/starry_release.jks"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SO_BUILT="$STARRY_SRC/libs_hkld/arm64-v8a/libstarry_magic.so"

# shellcheck source=/dev/null
[[ -f "$NIXIANG/04_scripts/_common/env.sh" ]] && source "$NIXIANG/04_scripts/_common/env.sh"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
if [[ -z "$ZIPALIGN" || ! -f "$ZIPALIGN" ]]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    d="$HOME/Library/Android/sdk/build-tools/$v"
    if [[ -f "$d/zipalign" ]]; then ZIPALIGN="$d/zipalign"; APKSIGNER="$d/apksigner"; break; fi
  done
fi
[[ -f "$ZIPALIGN" ]] || { echo "ERROR: zipalign 不存在"; exit 1; }
[[ -f "$APKSIGNER" ]] || { echo "ERROR: apksigner 不存在"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺底包 $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali 缺失"; exit 1; }

LOCK_DIR="/tmp/hkld_surgical.lockdir"
mkdir "$LOCK_DIR" 2>/dev/null || { echo "ERROR: lock $LOCK_DIR held"; exit 1; }
WORK="$(mktemp -d /tmp/hkld_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[0/7] 底包身份 + dex 落点"
python3 - "$ORIG_APK" <<'PY'
import sys, zipfile, struct, hashlib
apk = sys.argv[1]
z = zipfile.ZipFile(apk)
names = z.namelist()
dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
print("  底包 dex 布局:", dexes)
assert dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"], dexes
print("  底包 md5:", hashlib.md5(open(apk,'rb').read()).hexdigest())
for n in dexes:
    d = z.read(n)
    f = struct.unpack_from("<20I", d, 0x20)
    print("   %-14s method_ids=%6d spare=%d" % (n, f[14], 65536 - f[14]))
PY
MERGE_DEX="$(bash "$STARRY_SRC/scripts/_lib/dex_target.sh" --apk "$ORIG_APK" --need-methods 2048 --print-entry)" \
  || { echo "ERROR: dex_target.sh 选不出落点"; exit 1; }
echo "  [dex] 合并落点 = ${MERGE_DEX}（dex_target.sh 按余量选定，禁硬编码）"

echo "[1/7] so 自检 / 构建"
if [[ "${HKLD_REBUILD_SO:-0}" == "1" || ! -f "$SO_BUILT" ]]; then
  echo "  ndk-build（Android_hkld.mk）"
  ( cd "$STARRY_SRC/app/src/main/jni" && \
    "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_hkld.mk \
      NDK_APPLICATION_MK=./Application.mk APP_ABI="arm64-v8a" \
      NDK_OUT="$STARRY_SRC/obj_hkld" NDK_LIBS_OUT="$STARRY_SRC/libs_hkld" -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" ) >/dev/null
fi
[[ -f "$SO_BUILT" ]] || { echo "ERROR: 缺 $SO_BUILT"; exit 1; }
LC_ALL=C grep -a -q '\[Hkld\]' "$SO_BUILT" || { echo "ERROR: so 无 [Hkld] 标签"; exit 1; }
LC_ALL=C grep -a -q '无限钥匙' "$SO_BUILT" || { echo "ERROR: so 缺菜单串 无限钥匙"; exit 1; }
SO_MD5="$(md5 -q "$SO_BUILT")"
echo "  so md5=$SO_MD5  size=$(stat -f%z "$SO_BUILT")"
for dest in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" "$STARRY_SRC/app/src/main/libs/arm64-v8a" "$STARRY_SRC/app/libs/arm64-v8a"; do
  mkdir -p "$dest"; cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done

echo "[2/7] 菜单 APK（gradle assembleRelease）"
MENU_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
if [[ "${HKLD_SKIP_GRADLE:-0}" != "1" ]]; then
  ( cd "$STARRY_SRC" && export JAVA_HOME="$(/usr/libexec/java_home -v 17)" && ./gradlew assembleRelease --console=plain -q ) || {
    echo "ERROR: gradle 失败"; exit 1; }
fi
[[ -f "$MENU_APK" ]] || { echo "ERROR: 缺 $MENU_APK"; exit 1; }
unzip -p "$MENU_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/menu.so"
cmp -s "$WORK/menu.so" "$SO_BUILT" || { echo "ERROR: gradle APK 内 so != 构建产物（漏拷 jniLibs？）"; exit 1; }
echo "  菜单 APK ok（so 与构建产物逐字节相同）"

echo "[3/7] 收集 com/starry smali（baksmali 菜单 dex，不用 apktool）"
unzip -p "$MENU_APK" classes.dex > "$WORK/menu.dex"
"$BAKSMALI_BIN" d -o "$WORK/menu_smali" "$WORK/menu.dex" >/dev/null
[[ -d "$WORK/menu_smali/com/starry" ]] || { echo "ERROR: 菜单 dex 里没有 com/starry"; exit 1; }
cp -R "$WORK/menu_smali/com/starry" "$WORK/starry_src"
echo "  starry smali 文件数: $(find "$WORK/starry_src" -name '*.smali' | wc -l | tr -d ' ')"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/menu.so" "$WORK/lib/arm64-v8a/libstarry_magic.so"
unzip -p "$MENU_APK" lib/arm64-v8a/libc++_shared.so > "$WORK/lib/arm64-v8a/libc++_shared.so"
if unzip -Z1 "$MENU_APK" | grep -q '^assets/certs/'; then
  mkdir -p "$WORK/assets/certs"
  ( cd "$WORK" && unzip -o -q "$MENU_APK" 'assets/certs/*' )
fi

echo "[4/7] 星光类合并进 ${MERGE_DEX}（不新增 dex）"
unzip -p "$ORIG_APK" "$MERGE_DEX" > "$WORK/merge_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/merge_smali" "$WORK/merge_orig.dex" >/dev/null
mkdir -p "$WORK/merge_smali/com"
cp -R "$WORK/starry_src" "$WORK/merge_smali/com/starry"
"$SMALI_BIN" assemble -o "$WORK/$MERGE_DEX" "$WORK/merge_smali"
python3 - "$WORK/$MERGE_DEX" "$WORK/merge_orig.dex" "$MERGE_DEX" <<'PY'
import struct, sys
new, orig, name = sys.argv[1], sys.argv[2], sys.argv[3]
d = open(new,'rb').read(); o = open(orig,'rb').read()
assert d[:4] == b"dex\n", d[:4]
f = struct.unpack_from("<20I", d, 0x20); fo = struct.unpack_from("<20I", o, 0x20)
print("  %s size=%d (官方 %d) method_ids=%d/65536 class_defs=%d (官方 %d)" % (
    name, len(d), len(o), f[14], f[16], fo[16]))
assert f[14] < 65536, f[14]
assert f[16] >= fo[16] + 200, (f[16], fo[16])
for s in (b"com/starry/magic/StarryApp", b"com/starry/magic/StarryInitProvider",
          b"com/starry/magic/menu/MenuService"):
    assert s in d, s
print("  合并 dex 自检通过（星光入口齐备）")
PY

echo "[5/7] AndroidManifest（binary，provider 入口；不换 Application）"
unzip -p "$ORIG_APK" AndroidManifest.xml > "$WORK/AndroidManifest.orig.xml"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  python3 "$SCRIPT_DIR/patch_hkld_binary_manifest.py" "$WORK/AndroidManifest.orig.xml" "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached -> $MF_CACHE"
fi

echo "[6/7] 外科 zip 组装（只动 Manifest + $MERGE_DEX + so/certs）"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$MERGE_DEX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src = Path(sys.argv[1]); work = Path(sys.argv[2]); dst = Path(sys.argv[3]); merge_dex = sys.argv[4]

mf = (work / "AndroidManifest.xml").read_bytes()
dex = (work / merge_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

replace = {"AndroidManifest.xml": mf, merge_dex: dex}
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    data = replace.get(name, zin.read(name))
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name in replace:
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

z2 = zipfile.ZipFile(dst, "r"); zo = zipfile.ZipFile(src, "r")
for name in ("classes.dex", "classes2.dex", "classes3.dex", "resources.arsc"):
    a = hashlib.sha256(zo.read(name)).hexdigest()
    b = hashlib.sha256(z2.read(name)).hexdigest()
    assert a == b, name + " 被改动！"
names = set(z2.namelist())
assert "classes5.dex" not in names, "不应新增 dex"
assert merge_dex in names
d = z2.read(merge_dex)
for s in (b"com/starry/magic/StarryInitProvider", b"com/starry/magic/menu/MenuService"):
    assert s in d, s
so = z2.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Hkld]" in so, "so 不是 Hkld"
assert b"[Zjmc]" not in so and b"[Wxsj" not in so
mf2 = z2.read("AndroidManifest.xml")
for s in ("com.starry.magic.StarryInitProvider",
          "com.chillyroom.hanksbar.yw.starryinit",
          "com.starry.magic.menu.MenuService",
          "com.chillyroomsdk.sdkbridge.BasePlayerApplication"):
    assert s.encode("utf-16-le") in mf2, s
print("  merge ok: classes/2/3 + arsc 逐字节一致；%s 已合并星光；no new dex；[Hkld] so；Manifest 入口齐" % merge_dex)
z2.close(); zo.close()
PY

echo "[7/7] zipalign + apksigner"
"$ZIPALIGN" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | head -6
rm -f "$WORK/aligned.apk" 2>/dev/null || true
rm -f "$OUT"/*.idsig 2>/dev/null || true

python3.11 - "$SIGNED" "$MERGE_DEX" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
d = z.read(sys.argv[2])
assert b"com/starry/magic/StarryInitProvider" in d and b"com/starry/magic/menu/MenuService" in d
assert b"[Hkld]" in z.read("lib/arm64-v8a/libstarry_magic.so")
print("  signed ok；星光入口在 %s，so 为 [Hkld]" % sys.argv[2])
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install]（需设备已授权）"
  bash "$NIXIANG/04_scripts/_common/install_starry_apk.sh" --apk "$SIGNED" --pkg "$PKG" \
    --so-tag '[Hkld]' --game-id hkld "${SERIAL:+--serial}" 2>/dev/null || \
  bash "$NIXIANG/04_scripts/_common/install_starry_apk.sh" --apk "$SIGNED" --pkg "$PKG" \
    --so-tag '[Hkld]' --game-id hkld
fi
echo "DONE $SIGNED"
