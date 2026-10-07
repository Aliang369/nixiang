#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399，vc4412）— 外科组装（2026-10-07 稳定性整改）
#
# 目的（用户 2026-10-07 指令）：
#   ① 把 TapTap 2.4.0 已实测稳定的架构移植到 4399 包（不是重写，是接线 + 换 so）
#   ② 把包内旧版菜单 dex（classes2.dex，2026-08-20，293,612 B）换成当前 HEAD 新版
#
# 底包 = 从真机 f9a116a2 拉下来的「当前已装星光包」base.apk：
#   versionCode=4412 与官方 4399 2.4.0 一致；classes.dex / resources.arsc 是官方原始字节，
#   本脚本外科置换 libstarry_magic.so + classes2.dex + assets/certs 并重签名，
#   等价于从官方包重建星光包（且天然保留玩家已装版本的数据兼容）。
#
# 与 TapTap 版（surgical_tlgd_taptap_2.4.0.sh）的差异：
#   - 底包 / 包名 / 产物名 / Manifest 独立
#   - 消费 build_so 产物 libs_tlgdm4399/arm64-v8a/libstarry_magic.so（本脚本不编 native）
#   - ★AndroidManifest.xml **默认原样保留底包字节**：底包本身已是星光包，
#     其 Manifest 已含 StarryApp + MenuService(specialUse) + SYSTEM_ALERT_WINDOW。
#     避免 apktool 对 4399 SDK 那套 Manifest 做文本→二进制往返（历史事故来源）。
#     仅当底包 Manifest 缺件时才回退到 MF_CACHE / mini-apktool 现生成。
#
# 用法:
#   bash 04_scripts/弹力果冻4399/tlgd4399_surgical.sh
#   只出包不装：默认即只出包（装机由管线 install 阶段负责）
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
ORIG_APK="$NIXIANG/02_apks/弹力果冻4399/弹力果冻4399_2.4.0_vc4412_device_base.apk"
OUT="$NIXIANG/08_output/弹力果冻4399"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.ginicat.sweet.m4399"
VC_EXPECT="4412"
TAG_OUT="4399"
STEM="弹力果冻4399_2.4.0_稳定整改_20261007"
SIGNED="$OUT/${STEM}-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/${STEM}-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/弹力果冻4399/surgical/AndroidManifest.xml"
BUILT_SO="$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libstarry_magic.so"
MK="$STARRY_SRC/app/src/main/jni/Android_tlgdm4399.mk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$MK" ]] || { echo "ERROR: missing $MK (4399 私有 mk)"; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found"; exit 1; }
echo "[check] 底包 = $ORIG_APK"
echo "[check] 私有 mk = Android_tlgdm4399.mk (modern 注册表 TlgdM4399 / game_id tlgd)"

# ---------- 底包身份断言：必须是 4399 星光包，否则后面全是错 ----------
BADGE="$("$BT/aapt2" dump badging "$ORIG_APK" 2>/dev/null | head -1 || true)"
echo "[check] $BADGE"
echo "$BADGE" | grep -q "name='$PKG'" \
  || { echo "ERROR: 底包包名不是 $PKG —— 底包拉错渠道，禁止继续"; exit 1; }
echo "$BADGE" | grep -q "versionCode='$VC_EXPECT'" \
  || { echo "ERROR: 底包 versionCode 不是 $VC_EXPECT —— 底包版本错，禁止继续"; exit 1; }

python3.11 - "$ORIG_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
need = ["classes.dex", "classes2.dex", "lib/arm64-v8a/libstarry_magic.so",
        "lib/arm64-v8a/libil2cpp.so", "META-INF/STARRY.RSA", "META-INF/STARRY.SF"]
missing = [n for n in need if n not in names]
assert not missing, f"底包缺条目 {missing} —— 不是 4399 星光包？"
dexs = sorted(n for n in names if n.endswith(".dex"))
assert dexs == ["classes.dex", "classes2.dex"], f"底包 dex 布局异常 {dexs}（本脚本固定写 classes2.dex，必须无下标缺口）"
print(f"  [check] 底包结构 ok：{len(names)} 条目；dex={dexs}")
PY

LOCK_DIR="/tmp/tlgd4399_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/tlgd4399.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] 消费 build_so 产物 -> jniLibs（本脚本不重编 native）"
[[ -f "$BUILT_SO" ]] || { echo "ERROR: missing $BUILT_SO —— 先跑 04_scripts/弹力果冻4399/tlgd4399_build_so.sh"; exit 1; }
# ⚠ macOS BSD grep 在 zh_CN.UTF-8 下匹配不了二进制里的 UTF-8 模式 → 必须 LC_ALL=C
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

echo "[1b/6] gradle ${GRADLE_TASK} (刷新菜单 dex 源码 → 包内 classes2.dex)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
LC_ALL=C grep -a -q 'Category_战斗' "$WORK/check.so" \
  || { echo "ERROR: APK 内 so 不是 TLGD 功能版 —— 菜单会错成上一款游戏"; exit 1; }
echo "  APK 内 so verified"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes2.dex（底包固定 classes2.dex 槽位）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
if ! grep -q '^\.super Landroid/app/Application;' "$SMALI_ROOT/com/starry/magic/StarryApp.smali"; then
  echo "ERROR: 官方无自定义 Application，StarryApp 必须 .super Landroid/app/Application;"
  exit 1
fi
# 4399 渠道专属：4399 包名必须在 SignSpoof 表内（否则签名伪装落空）
# 注意：smali 里字符串常量保留点号（const-string vX, "com.ginicat.sweet.m4399"）
grep -qE 'com[./]ginicat[./]sweet[./]m4399' "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 未含 4399 包名 com.ginicat.sweet.m4399"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

# 菜单 dex 换新证据：与设备上旧 dex 比大小/哈希（旧 = 2026-08-20 的 293,612 B）
if [[ -f /tmp/tlgd_probe/dev_classes2.dex ]]; then
  OLD_SHA="$(shasum -a 256 /tmp/tlgd_probe/dev_classes2.dex | awk '{print $1}')"
  NEW_SHA="$(shasum -a 256 "$WORK/classes2.dex" | awk '{print $1}')"
  echo "  [dex 换新] 旧 $(stat -f%z /tmp/tlgd_probe/dev_classes2.dex) B sha256=${OLD_SHA:0:16}…"
  echo "  [dex 换新] 新 $(stat -f%z "$WORK/classes2.dex") B sha256=${NEW_SHA:0:16}…"
  [[ "$OLD_SHA" != "$NEW_SHA" ]] || { echo "ERROR: 新 dex 与设备旧 dex 完全一致 —— 菜单没换新"; exit 1; }
fi

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so in starry apk"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
# APP_STL=c++_shared → 必须随包发 libc++_shared.so（底包已有一份，仍以新构建为准）
if [[ -f "$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libc++_shared.so" ]]; then
  cp -f "$STARRY_SRC/libs_tlgdm4399/arm64-v8a/libc++_shared.so" "$WORK/lib/arm64-v8a/"
elif [[ -f "$NDK_CXX" ]]; then
  cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
fi
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] AndroidManifest：底包已是星光包 → 原样保留字节（不做 apktool 往返）"
KEEP_MF=0
python3.11 - "$ORIG_APK" <<'PY' && KEEP_MF=1 || KEEP_MF=0
import sys, zipfile
m = zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml")
need = ["com.starry.magic.StarryApp", "com.starry.magic.menu.MenuService",
        "SYSTEM_ALERT_WINDOW", "PROPERTY_SPECIAL_USE_FGS_SUBTYPE"]
bad = [s for s in need if m.count(s.encode("utf-16-le")) < 1 and m.count(s.encode()) < 1]
sys.exit(1 if bad else 0)
PY
if [[ "$KEEP_MF" == "1" ]]; then
  unzip -p "$ORIG_APK" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  底包 Manifest 已含 StarryApp/MenuService/specialUse/悬浮窗 → 原样保留（字节等同底包）"
else
  echo "  底包 Manifest 缺件 → 回退生成"
  if [[ -n "${TLGD_BINARY_MANIFEST:-}" && -f "${TLGD_BINARY_MANIFEST:-}" ]]; then
    cp "$TLGD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  elif [[ -f "$MF_CACHE" ]]; then
    cp "$MF_CACHE" "$WORK/AndroidManifest.xml"; echo "  using cached $MF_CACHE"
  else
    echo "ERROR: 底包 Manifest 缺星河件且无缓存 —— 需人工确认底包来源"; exit 1
  fi
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge (keep official classes.dex + arsc)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
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
# dex 链必须连续：classes.dex, classes2.dex 且无缺口
dexs = sorted(x for x in o if x.endswith(".dex"))
assert dexs == ["classes.dex", "classes2.dex"], f"输出 dex 布局异常 {dexs}"
assert hashlib.sha256(zout2.read("classes2.dex")).hexdigest() == hashlib.sha256(c2).hexdigest(), "包内 classes2.dex != 新构建菜单 dex"
print(f"  merged ok; entries≈{n}; classes2={len(c2)}")
zin2.close(); zout2.close()
PY

# ---- 旧 so 混包防线：包内 so 必须等于 build_so 产物 ----
PKG_SO_MD5="$(unzip -p "$WORK/unsigned.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$BUILT_SO_MD5" ]] \
  || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != build_so 产物 $BUILT_SO_MD5"; exit 1; }
echo "  [5b/6] 包内 so md5 == build_so 产物 ($PKG_SO_MD5)"

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -f "$KS" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*"$TAG_OUT"*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
ls -lh "$SIGNED"

# 签名指纹必须与底包同源（starry_release.jks），否则覆盖安装会被拒
echo "  签名指纹 = $("$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep -i 'SHA-256 digest' | head -1 || true)"
echo "artifact_md5=$(md5 -q "$SIGNED")"
echo "artifact_sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "DONE: $SIGNED"
