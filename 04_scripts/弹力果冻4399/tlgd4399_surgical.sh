#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399，vc4412）— 外科组装
#
# 2026-10-26 轮（雷电模拟器 / 官方原版底包）相对 2026-10-07 轮的差异：
#   ① 底包 = 雷电 `emulator-5554` 上已装的 **4399 官方原版** base.apk（无 classes2.dex、
#      无 libstarry_magic.so、Manifest 未注入）⇒ 本轮要走完整注入而非「换 so 换 dex」。
#   ② Manifest 用二进制 AXML 结构化补丁（`tlgd4399_patch_manifest.py`，范本=追迹迷城 v3），
#      不做 apktool 文本↔二进制往返（4399 SDK 那套 Manifest 的历史事故来源）。
#   ③ Windows(Git Bash) 分支：无 rsync ⇒ cp -R；build-tools 用 .exe/.bat；中文 jni 路径不参与。
#
# 版本锚点（本脚本强制断言）：
#   - versionCode 4412 / 包名 com.ginicat.sweet.m4399
#   - 包内 lib/arm64-v8a/libil2cpp.so md5 == b3c74d18e4111fed2d0049088ae21c6d（= 4399 定稿，
#     解释器公共点文件偏移 0xBAAB0C 的前提；变了就禁止出包，必须重定位）
#
# 用法: bash 04_scripts/弹力果冻4399/tlgd4399_surgical.sh    # 默认只出包不装机
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

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
ORIG_APK="${TLGD4399_BASE:-$NIXIANG/02_apks/弹力果冻4399/弹力果冻4399_2.4.0_vc4412_device_base.apk}"
OUT="$NIXIANG/08_output/弹力果冻4399"
SMALI_BIN="${TLGD_SMALI:-$NIXIANG/01_tools/smali/smali}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.ginicat.sweet.m4399"
VC_EXPECT="4412"
IL2CPP_MD5_EXPECT="${TLGD_IL2CPP_MD5:-b3c74d18e4111fed2d0049088ae21c6d}"
TAG_OUT="4399"
STEM="${TLGD_STEM:-弹力果冻4399_2.4.0_雷电星空_20261026}"
SIGNED="$OUT/${STEM}-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/${STEM}-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/弹力果冻4399/surgical/AndroidManifest.patched.xml"
BUILT_SO="$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libstarry_magic.so"
MK="$STARRY_SRC/app/src/main/jni/Android_tlgdm4399.mk"
MANIFEST_PATCHER="$NIXIANG/04_scripts/弹力果冻4399/tlgd4399_patch_manifest.py"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（先跑 tlgd4399_fetch_apk.sh）"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$MK" ]] || { echo "ERROR: missing $MK (4399 私有 mk)"; exit 1; }
[[ -f "$MANIFEST_PATCHER" ]] || { echo "ERROR: missing $MANIFEST_PATCHER"; exit 1; }

# ---- 宿主机差异：Windows 下 build-tools 是 .exe / .bat ----
EXE=""
case "$(uname -s 2>/dev/null || echo unknown)" in
  MINGW*|MSYS*|CYGWIN*) EXE=".exe" ;;
esac
BT="${TLGD_BT:-}"
if [[ -z "$BT" ]]; then
  for v in 36.1.0 36.0.0 35.0.0 34.0.0; do
    for base in "${ANDROID_HOME:-}" "$HOME/Library/Android/sdk"; do
      [[ -n "$base" ]] || continue
      if [[ -x "$base/build-tools/$v/zipalign$EXE" ]]; then BT="$base/build-tools/$v"; break 2; fi
    done
  done
fi
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found"; exit 1; }
ZIPALIGN="$BT/zipalign$EXE"
APKSIGNER="$BT/apksigner"
[[ -x "$BT/apksigner$EXE" ]] && APKSIGNER="$BT/apksigner$EXE"
[[ -f "$BT/apksigner.bat" ]] && APKSIGNER="$BT/apksigner.bat"

echo "[check] 底包 = $ORIG_APK"
echo "[check] build-tools = $BT"
echo "[check] 私有 mk = Android_tlgdm4399.mk (modern 注册表 TlgdM4399 / game_id tlgd)"

# ---------- 底包身份断言：4399 官方原版 ----------
BADGE="$("$BT/aapt2$EXE" dump badging "$ORIG_APK" 2>/dev/null | head -1 || true)"
echo "[check] $BADGE"
echo "$BADGE" | grep -q "name='$PKG'" \
  || { echo "ERROR: 底包包名不是 $PKG —— 底包拉错渠道，禁止继续"; exit 1; }
echo "$BADGE" | grep -q "versionCode='$VC_EXPECT'" \
  || { echo "ERROR: 底包 versionCode 不是 $VC_EXPECT —— 底包版本错，禁止继续"; exit 1; }

echo "[check] 包内 libil2cpp.so md5（0xBAAB0C 偏移的前提）"
IL2CPP_MD5="$(unzip -p "$ORIG_APK" lib/arm64-v8a/libil2cpp.so | md5 -q)"
[[ "$IL2CPP_MD5" == "$IL2CPP_MD5_EXPECT" ]] \
  || { echo "ERROR: libil2cpp md5=$IL2CPP_MD5 != 定稿 $IL2CPP_MD5_EXPECT —— native 变了，0xBAAB0C 不可用，禁止出包" >&2; exit 1; }
echo "  libil2cpp md5=$IL2CPP_MD5 ✓"

python - "$ORIG_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
need = ["classes.dex", "lib/arm64-v8a/libil2cpp.so", "assets/AOTDLL/hotfix.bytes",
        "resources.arsc", "AndroidManifest.xml"]
missing = [n for n in need if n not in names]
assert not missing, f"底包缺条目 {missing}"
dexs = sorted(n for n in names if n.endswith(".dex"))
assert dexs == ["classes.dex"], f"底包 dex 布局异常 {dexs}（官方原版应只有 classes.dex）"
assert "classes2.dex" not in names, "底包已有 classes2.dex —— 不是官方原版"
assert not any(n.startswith("META-INF/STARRY") for n in names), "底包已是星光包，本脚本按官方原版设计"
print(f"  [check] 底包结构 ok：{len(names)} 条目；dex={dexs}；无星光残留")
PY

LOCK_DIR="/tmp/tlgd4399_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/tlgd4399.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] 消费 build_so 产物 -> jniLibs（本脚本不重编 native）"
[[ -f "$BUILT_SO" ]] || { echo "ERROR: missing $BUILT_SO —— 先跑 04_scripts/弹力果冻4399/tlgd4399_build_so.sh"; exit 1; }
for pat in '\[Tlgd\]' 'Category_战斗' 'Toggle_全屏秒杀' '解释器入口已挂' '原生桥补发'; do
  LC_ALL=C grep -a -q "$pat" "$BUILT_SO" \
    || { echo "ERROR: $BUILT_SO 缺证据串 '$pat'（不是 4399 稳定整改版 so）"; exit 1; }
done
if LC_ALL=C grep -aq 'tlgdtt' "$BUILT_SO"; then
  echo "ERROR: $BUILT_SO 含 tlgdtt —— 编进了 TapTap 入口，authGameId 会错"; exit 1
fi
if LC_ALL=C grep -aq '\[Dwct\]' "$BUILT_SO"; then
  echo "ERROR: $BUILT_SO 含 [Dwct]，jniLibs 未刷新"; exit 1
fi
BUILT_SO_MD5="$(md5 -q "$BUILT_SO")"
echo "  so 证据串齐全（[Tlgd]/Category_战斗/解释器入口已挂/原生桥补发），md5=$BUILT_SO_MD5"
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$BUILT_SO" "$dest/libstarry_magic.so"
done

echo "[2/7] gradle ${GRADLE_TASK}（刷新菜单 dex 源码 → 包内 classes2.dex）"
mkdir -p "$WORK/starry"
cp -R "$STARRY_SRC/." "$WORK/starry/"
rm -rf "$WORK/starry/.gradle" "$WORK/starry/app/build" "$WORK/starry/build" \
       "$WORK/starry/obj_tlgdm4399" "$WORK/starry/libs_tlgdm4399" \
       "$WORK/starry/auth-server/.venv" "$WORK/starry/auth-server/data" "$WORK/starry/.idea"
# sdk.dir 必须是本机路径（仓库里记的是 mac 路径）
if [[ -n "${ANDROID_HOME:-}" ]]; then
  printf 'sdk.dir=%s\n' "$(cygpath -m "$ANDROID_HOME" 2>/dev/null || echo "$ANDROID_HOME")" > "$WORK/starry/local.properties"
fi
( cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain -x lint ) || { echo "ERROR: gradle failed"; exit 1; }
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
LC_ALL=C grep -a -q 'Category_战斗' "$WORK/check.so" \
  || { echo "ERROR: APK 内 so 不是 TLGD 功能版 —— 菜单会错成上一款游戏"; exit 1; }
echo "  APK 内 so verified"

echo "[3/7] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[4/7] collect com/starry smali → classes2.dex"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  mkdir -p "$SMALI_ROOT/com"
  cp -R "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
if ! grep -q '^\.super Landroid/app/Application;' "$SMALI_ROOT/com/starry/magic/StarryApp.smali"; then
  echo "ERROR: 官方无自定义 Application，StarryApp 必须 .super Landroid/app/Application;"
  exit 1
fi
# 2026-10-28：SignSpoof（含证书/包名表）已删除 ⇒ 不再断言 dex 内含 4399 包名映射。
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so in starry apk"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
if [[ -f "$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libc++_shared.so" ]]; then
  cp -f "$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libc++_shared.so" "$WORK/lib/arm64-v8a/"
elif [[ -f "$NDK/toolchains/llvm/prebuilt"/*/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so ]]; then
  cp "$NDK/toolchains/llvm/prebuilt"/*/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so "$WORK/lib/arm64-v8a/"
else
  echo "ERROR: libc++_shared.so 缺（APP_STL=c++_shared 必须随包）"; exit 1
fi
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/7] AndroidManifest：官方底包 → 二进制 AXML 结构化注入"
unzip -p "$ORIG_APK" AndroidManifest.xml > "$WORK/manifest_official.xml"
python "$MANIFEST_PATCHER" "$WORK/manifest_official.xml" "$WORK/AndroidManifest.xml"
cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
# 复读自检（aapt2 视角）
"$BT/aapt2$EXE" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/official_tree.txt" 2>/dev/null || true
python - "$WORK/AndroidManifest.xml" <<'PY'
import sys
m = open(sys.argv[1], "rb").read()
need = ["com.starry.magic.StarryApp", "com.starry.magic.menu.MenuService",
        "SYSTEM_ALERT_WINDOW", "PROPERTY_SPECIAL_USE_FGS_SUBTYPE",
        "com.starry.magic.StarryInitProvider"]
bad = [s for s in need if m.count(s.encode("utf-16-le")) < 1]
assert not bad, f"注入后的 Manifest 缺 {bad}"
print("  Manifest 注入自检 ok：StarryApp + MenuService + StarryInitProvider + 权限 + specialUse property")
PY

echo "[6/7] zip surgical merge (keep official classes.dex + arsc + hotfix.bytes)"
python - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_protected(name: str) -> bool:
    return name in ("classes.dex", "resources.arsc")

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == "classes2.dex":
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

info = zipfile.ZipInfo("classes2.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c2)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
o = zout2.namelist()
for name in zin2.namelist():
    if is_protected(name):
        assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes2.dex" in o
assert "lib/arm64-v8a/libstarry_magic.so" in o
assert "lib/arm64-v8a/libc++_shared.so" in o
assert not any(x.startswith("META-INF/") for x in o), "META-INF 未清干净（旧签名残留）"
dexs = sorted(x for x in o if x.endswith(".dex"))
assert dexs == ["classes.dex", "classes2.dex"], f"输出 dex 布局异常 {dexs}"
assert hashlib.sha256(zout2.read("classes2.dex")).hexdigest() == hashlib.sha256(c2).hexdigest(), "包内 classes2.dex != 新构建菜单 dex"
print(f"  merged ok; entries≈{n}; classes2={len(c2)}")
zin2.close(); zout2.close()
PY

PKG_SO_MD5="$(unzip -p "$WORK/unsigned.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$BUILT_SO_MD5" ]] \
  || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != build_so 产物 $BUILT_SO_MD5"; exit 1; }
echo "  [6b/7] 包内 so md5 == build_so 产物 ($PKG_SO_MD5)"

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$ZIPALIGN" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$APKSIGNER" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
ls -lh "$SIGNED"
echo "artifact_md5=$(md5 -q "$SIGNED")"
echo "artifact_sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "DONE: $SIGNED"
