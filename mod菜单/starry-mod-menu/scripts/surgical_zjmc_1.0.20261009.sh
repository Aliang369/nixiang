#!/usr/bin/env bash
# 追迹迷城 1.0.20261009（vc101201，TapTap）— 外科组装（禁止对游戏 apktool b 整包）
#
# 产物：08_output/追迹迷城/追迹迷城_1.0.20261009_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_zjmc_1.0.20261009.sh
#   STARRY_INSTALL=1 bash scripts/surgical_zjmc_1.0.20261009.sh
#
# 与 0912 版脚本的关键差异（2026-10-10 定，均有实测依据）：
#   ① **星光类合并进宿主的 classes4.dex**（不新增 dex），官方 classes/2/3 + resources.arsc
#      必须逐字节不变（脚本内 sha256 断言）。
#      依据：新包 classes3.dex method_ids=65533/65536（只剩 3 个空位，绝不能进；0912 版把
#      Activity 注入 + TapSignKit 改写都放它里面，本版必然溢出），classes4.dex=37691
#      （余量 27845，唯一可行目标）；0912 版脚本注释原话「单 dex，绕开 classes7 自动发现」。
#      宿主 Activity / TapTap 登录类**完全不碰**（框架入口已上移到 Provider，见 ②）。
#   ② **入口点走 ContentProvider（com.starry.magic.StarryInitProvider），不用 Application 替换**。
#      依据：1.0.20261009 实测 <application android:name> 平台不采用（属性记录合法但 StarryApp
#      从未实例化：logcat 零 StarryApp 行、/proc/<pid>/maps 无 libstarry_magic.so，且类缺失
#      本应 "Unable to instantiate application" 崩溃而未崩）；Provider 是本框架既有入口
#      （StarryInitProvider.java 类注释：终末地 ACE 同款场景），且实测本包 TapTap/Unity 的
#      providers 都被实例化（dumpsys activity processes 有 ContentProviderRecord）。
#      规则：provider 必须排在清单里其它 provider 之前（先回灌官方签名，再让 SDK 读）。
#   ③ Manifest 走 v3 补丁器产物，补丁器已修 **resource map off-by-2 缺陷**（v3 首版 map 追加
#      基址写成 (sz-16)//4 ⇒ foregroundServiceType/stopWithTask 平台读成 0，aapt2 复读看不出来；
#      现由补丁器 + zjmc_manifest.sh 双重回读断言把住）。
#   ④ 删掉 0912 脚本里 `if false;` 包住的 20260906 遗留死块。
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"

VERSION="1.0.20261009"
VC="101201"
META="$NIXIANG/04_scripts/追迹迷城/official_apk.json"
# 底包路径以 official_apk.json 的 apk_path 为准（AGENTS：APK 不留本地，底包常驻 /tmp 临时副本）
if [[ -n "${ZJMC_ORIG_APK:-}" ]]; then
  ORIG_APK="$ZJMC_ORIG_APK"
else
  ORIG_APK="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META")"
fi
OUT="$NIXIANG/08_output/追迹迷城"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.TeaMelo.DungeonChasers"
SIGNED="$OUT/追迹迷城_${VERSION}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/追迹迷城_${VERSION}_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/追迹迷城/surgical_$VERSION/AndroidManifest.xml"
MERGE_DEX="classes4.dex"
SO_MD5_EXPECT="${ZJMC_SO_MD5:-}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（底包是 /tmp 临时副本，用完即删；见 10_docs/_state/底包再获取台账.md）"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZJMC"; exit 1; }
echo "[check] Android.mk target=zjmc"

# 底包身份核对（防拿错版本的包出包）
python3 - "$ORIG_APK" "$VERSION" "$VC" <<'PY'
import sys, zipfile, struct
apk, ver, vc = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk)
names = z.namelist()
dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
print("  底包 dex 布局:", dexes)
assert dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"], dexes
assert "classes5.dex" not in names, "底包已存在 classes5.dex，新 dex 编号需顺延"
def hdr(n):
    d = z.read(n)
    f = struct.unpack_from("<20I", d, 0x20)
    return f[14]  # method_ids_size
for n in dexes:
    print(f"  {n:14s} method_ids={hdr(n):6d} spare={65536-hdr(n)}")
print(f"  目标版本 {ver} / vc{vc} 与底包 dex 布局断言通过")
PY

LOCK_DIR="/tmp/zjmc_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zjmc_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] verify build_so -> jniLibs"
SO_BUILT="$STARRY_SRC/libs_zjmc/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT; run 04_scripts/追迹迷城/zjmc_build_so.sh first"; exit 1; }
LC_ALL=C grep -a -q '\[Zjmc\]' "$SO_BUILT" || { echo "ERROR: so 不是 ZJMC"; exit 1; }
LC_ALL=C grep -a -q 'Toggle_免广告' "$SO_BUILT" || { echo "ERROR: so 缺少免广告菜单项"; exit 1; }
LC_ALL=C grep -a -q 'AT.GetFreeSkipAds' "$SO_BUILT" || { echo "ERROR: so 缺少免广告 Hook 标签"; exit 1; }
if LC_ALL=C grep -a -q '\[Cstjjs\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 [Cstjjs]"
  exit 1
fi
SO_MD5="$(md5 -q "$SO_BUILT")"
if [[ -n "$SO_MD5_EXPECT" && "$SO_MD5" != "$SO_MD5_EXPECT" ]]; then
  echo "ERROR: so md5=$SO_MD5 与期望 $SO_MD5_EXPECT 不符"; exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Zjmc] + 免广告 verified, md5=$SO_MD5)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Zjmc\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 ZJMC"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] collect starry smali (将打进新增 $MERGE_DEX)"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }
echo "  starry smali collected ($(find "$SMALI_ROOT" -name '*.smali' | wc -l | tr -d ' ') files)"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] merge starry smali into ${MERGE_DEX}（既有 dex 合并，不新增 dex）"
# 为什么合并进 classes4 而不是新增 classes5.dex：0912 版脚本注释原话「单 dex，绕开
# classes7 自动发现」，且宿主 Activity/Provider 更信赖「官方 dex 内容」。
# 1.0.20261009 实测：classes3.dex method_ids=65533（只剩 3 个空位，绝不能进），
# classes4.dex=37691（余量 27845，唯一可行目标）。
unzip -p "$ORIG_APK" "$MERGE_DEX" > "$WORK/merge_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/merge_smali" "$WORK/merge_orig.dex" >/dev/null
rsync -a "$SMALI_ROOT/com" "$WORK/merge_smali/"
"$SMALI_BIN" assemble -o "$WORK/$MERGE_DEX" "$WORK/merge_smali"
python3 - "$WORK/$MERGE_DEX" "$WORK/merge_orig.dex" <<'PY'
import struct, sys
p, orig = sys.argv[1], sys.argv[2]
d = open(p, "rb").read(); o = open(orig, "rb").read()
assert d[:8] in (b"dex\n035\x00", b"dex\n037\x00", b"dex\n039\x00"), d[:8]
f = struct.unpack_from("<20I", d, 0x20)
fo = struct.unpack_from("<20I", o, 0x20)
print(f"  merged dex size={len(d)} (官方 {len(o)}) method_ids={f[14]}/65536 "
      f"class_defs={f[16]} (官方 {fo[16]})")
assert f[14] < 65536, f[14]
assert f[16] >= fo[16] + 180, (f[16], fo[16])
need = [b"com/starry/magic/StarryApp", b"com/starry/magic/menu/MenuService",
        b"com/starry/magic/StarryInitProvider",
        b"com/starry/magic/sign/SignSpoof", b"com/starry/magic/sign/TapLoginForceWeb",
        b"com/starry/magic/ota/StarryNativeLoad"]
for s in need:
    assert s in d, s
print("  合并 dex 自检通过（星光入口齐备，宿主类原位保留）")
PY

echo "[5/7] binary AndroidManifest (StarryInitProvider + MenuService, v3/v4 补丁产物)"
# 硬规则：禁止对游戏包 apktool d/b（见 apk-build-surgical.mdc）。
if [[ -n "${ZJMC_BINARY_MANIFEST:-}" && -f "$ZJMC_BINARY_MANIFEST" ]]; then
  cp "$ZJMC_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using ZJMC_BINARY_MANIFEST=$ZJMC_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "ERROR: missing $MF_CACHE"
  echo "  禁止在本脚本内对游戏包 apktool d/b。请先生成并缓存："
  echo "    bash 04_scripts/追迹迷城/zjmc_manifest.sh"
  exit 1
fi
python3 - "$WORK/AndroidManifest.xml" "$MERGE_DEX" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
new_dex = sys.argv[2].encode()
for s in ("com.starry.magic.StarryApp", "com.starry.magic.StarryInitProvider",
          "com.starry.magic.menu.MenuService",
          "com.TeaMelo.DungeonChasers.starryinit",
          "android.permission.SYSTEM_ALERT_WINDOW",
          "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
          "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"):
    assert s.encode("utf-16-le") in d, s
assert new_dex not in d  # Manifest 不需要声明 dex 名
print("  manifest 自检通过（StarryInitProvider 入口 + MenuService + FGS/悬浮窗权限）")
PY

echo "[6/7] zip surgical merge（只动 Manifest + 合并 ${MERGE_DEX} + so/assets）"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/$MERGE_DEX" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml "$MERGE_DEX" lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")

python3.11 - "$WORK/unsigned.apk" "$ORIG_APK" "$MERGE_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
dst, src, merge_dex = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
z = zipfile.ZipFile(dst); zo = zipfile.ZipFile(src)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names
d4 = z.read(merge_dex)
assert b"com/starry/magic/StarryApp" in d4 and b"com/starry/magic/menu/MenuService" in d4
assert b"com/starry/magic/StarryInitProvider" in d4
# 外科铁证：本轮只允许改 classes4.dex（合并星光）+ Manifest + so/assets，
# 其余官方条目必须逐字节未变。
for n in ["classes.dex", "classes2.dex", "classes3.dex", "resources.arsc"]:
    a, b = hashlib.sha256(zo.read(n)).hexdigest(), hashlib.sha256(z.read(n)).hexdigest()
    assert a == b, f"{n} 被改动！"
assert "classes5.dex" not in names, "不应新增 dex（走 classes4 合并路径）"
assert hashlib.sha256(zo.read(merge_dex)).hexdigest() != \
       hashlib.sha256(z.read(merge_dex)).hexdigest(), "合并 dex 未变？"
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zjmc]" in so and b"[Cstjjs]" not in so
print(f"  merged ok; classes.dex/2/3 + arsc 逐字节一致; {merge_dex} 已合并星光; 无新 dex; [Zjmc] so")
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8

python3.11 - "$SIGNED" "$MERGE_DEX" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
d4 = z.read(sys.argv[2])
assert b"com/starry/magic/StarryApp" in d4
assert b"com/starry/magic/StarryInitProvider" in d4
assert b"com/starry/magic/menu/MenuService" in d4
assert b"[Zjmc]" in z.read("lib/arm64-v8a/libstarry_magic.so")
print("  signed ok; 星光入口在 %s，菜单 so 为 [Zjmc]" % sys.argv[2])
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install"
  REMOTE=/data/local/tmp/starry_zjmc.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
    echo "ERROR: pm install failed（多半是异签名 INSTALL_FAILED_UPDATE_INCOMPATIBLE）"
    echo "       官方包签名为 O=深圳缇米乐科技有限公司,CN=蒋帅，本包为 CN=StarryMagic；"
    echo "       覆盖安装不可能成功，需用户明确同意后 uninstall 再装。"
    exit 1
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
