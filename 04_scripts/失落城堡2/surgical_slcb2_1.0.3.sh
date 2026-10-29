#!/usr/bin/env bash
# 失落城堡2 1.0.3 (com.leiting.slcb2, 雷霆渠道 / 内置 TapTap SDK, vc 10)
#   — 外科 zip 组装（保留网易易盾壳，不替换 Application）
#
# 保护形态：网易易盾 NIS 4.22.117
#   官方 Application = com.netease.nis.wrapper.MyApplication（替换即无法解密 dex → 启动崩）
#   真实的 5 个 dex（dex\n037）在 APK 内**无明文**，只在运行时由壳解密为 InMemoryDexFile
#   ⇒ 任何 Java 层补丁都只能落在**我们自己的 classes2.dex**（它排在 base.apk dex 列表最前，
#      但不要重定义游戏类；本阶段只做注入 + 签名伪装 + TapTap 网页登录路由）
#
# 注入方式：ContentProvider（StarryInitProvider）
#   ★按易盾先例（10_docs/_state/games/无限升级.md）：
#     - 声明位置**保持追加在 </application> 之前**，只加 android:initOrder="2147483647"
#       （挪到第 1 位会被易盾静默跳过 → 星空层不启动）
#     - 不替换 application@name / appComponentFactory
#
# 注入物：
#   classes2.dex                          （官方只有 classes.dex）
#   lib/arm64-v8a/libstarry_magic.so      （Slcb2 模块，环境阶段 0 hook）
#   lib/arm64-v8a/libc++_shared.so        （官方包内无，so 的 DT_NEEDED 需要）
#   assets/certs/starlight.crt
# 官方 classes.dex / resources.arsc / libil2cpp.so / global-metadata.dat 一律逐字节透传
#
# 底包：02_apks/失落城堡/失落城堡_1.26.0.331_leiting_original.apk（真机只读拉取）
#   339,443,428 B / md5 1b7fa4ec04267073274e3f43101f9182
#   sha256 97109bf6177e756b19efb04572ed5d4a9af37948cfbbb9d9d6bb25fe7ce22ff2
#   签名 CN=HB / O=Hunter Studio，MD5 4c6f8f0aa4506f3db70ffe02c2399d0e
#   → 已烘焙进 app/src/main/java/com/starry/magic/sign/SignSpoof.java 的 SLCB_CERT_B64
#
# 用法:
#   bash 04_scripts/失落城堡2/surgical_slcb2_1.0.3.sh
#   STARRY_INSTALL=1 ...                 （构建后直接装机）
#   STARRY_ALLOW_UNINSTALL=1 ...         （签名不一致需卸载重装时显式给）
#   STARRY_BUILD_TYPE=release ...        （默认 debug；上线/交付用 release）
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-debug}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="失落城堡2"
PKG="com.leiting.slcb2"
VER="1.0.3"
TAG="Slcb2"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
KS="$NIXIANG/01_tools/starry_release.jks"

ORIG_APK="${STARRY_ORIG_APK:-$NIXIANG/02_apks/${GAME}/${GAME}_${VER}_leiting_original.apk}"
OUT="$NIXIANG/08_output/${GAME}"
SIGNED="$OUT/${GAME}_${VER}_surgical-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/${GAME}_${VER}_starry.apk"
MF_CACHE="$NIXIANG/07_patched/${GAME}/surgical_${VER}/AndroidManifest.xml"

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
[[ -n "$BT" ]] || { echo "ERROR: no build-tools with zipalign/apksigner"; exit 1; }
MK="$JNI/Android_slcb2.mk"
grep -q 'Games/Slcb2Mod.cpp' "$MK" || { echo "ERROR: Android_slcb2.mk 缺 Games/Slcb2Mod.cpp"; exit 1; }
grep -q 'Games/Slcb2Entry.cpp' "$MK" || { echo "ERROR: Android_slcb2.mk 缺 Games/Slcb2Entry.cpp"; exit 1; }
grep -q 'Games/NoAceStubs.cpp' "$MK" || { echo "ERROR: Android_slcb2.mk 缺 Games/NoAceStubs.cpp（MainModern.cpp 引用 SignalQuarantine_Start/FgoBoot_InjectEarlyFromOnLoad）"; exit 1; }
echo "[check] orig apk = $ORIG_APK"

# 底包身份核对（防错包）
ORIG_MD5="$(md5 -q "$ORIG_APK")"
if [[ "$ORIG_MD5" != "1b7fa4ec04267073274e3f43101f9182" ]]; then
  echo "WARN: 底包 md5=$ORIG_MD5 ≠ 登记的 1b7fa4ec04267073274e3f43101f9182 —— 确认是否为新版本包"
fi

# 官方包内是否自带 libc++_shared.so（失落城堡：无 → 必须随包注入）
HAS_CXXSHARED="$(python3.11 - "$ORIG_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
print("1" if "lib/arm64-v8a/libc++_shared.so" in z.namelist() else "0")
PY
)"
echo "[check] 官方包自带 libc++_shared.so = ${HAS_CXXSHARED}（0 ⇒ 本次注入）"

LOCK_DIR="/tmp/slcb2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held"; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/slcb2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# 注入 dex 名 = 官方最大 classesN + 1（失落城堡官方只有 classes.dex → classes2.dex）
INJECT_DEX="$(python3.11 - "$ORIG_APK" <<'PY'
import re, sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
nums = []
for n in z.namelist():
    m = re.fullmatch(r'classes(?:(\d+))?\.dex', n)
    if m:
        nums.append(int(m.group(1) or 1))
print(f"classes{max(nums) + 1}.dex")
PY
)"
echo "[check] 官方 dex 最大序号 → 注入 $INJECT_DEX"

echo "[1/7] ndk-build（Android_slcb2.mk → libs_slcb2/，绝对路径）"
bash "$NIXIANG/04_scripts/失落城堡2/slcb2_build_so.sh"
SO_BUILT="$STARRY_SRC/libs_slcb2/arm64-v8a/libstarry_magic.so"
CXX_BUILT="$STARRY_SRC/libs_slcb2/arm64-v8a/libc++_shared.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q "\[$TAG\]" "$SO_BUILT" || { echo "ERROR: so 无 [$TAG] 标签（仍是上一款游戏）"; exit 1; }
for stale in '\[Yimo\]' '\[Cxswz\]' '\[Wxsj' '\[Jjdm\]' '\[Gyzw\]' '\[Hxwj\]' '\[Sjzz\]'; do
  grep -a -q "$stale" "$SO_BUILT" && { echo "ERROR: so 仍含上一款游戏标签 $stale"; exit 1; }
done
CERT_N=$(grep -a -c 'BEGIN CERTIFICATE' "$SO_BUILT" || true)
[[ "${CERT_N:-0}" -ge 1 ]] || { echo "ERROR: so 内无烘焙证书（授权 fail-closed）"; exit 1; }
echo "  so ok: $(md5 -q "$SO_BUILT") $(stat -f%z "$SO_BUILT") B (tag=$TAG cert=$CERT_N)"
mkdir -p "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_BUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
if [[ "$HAS_CXXSHARED" == "0" ]]; then
  [[ -f "$CXX_BUILT" ]] || { echo "ERROR: missing ${CXX_BUILT}（官方无 libc++_shared.so，必须随包注入）"; exit 1; }
  cp -f "$CXX_BUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libc++_shared.so"
  echo "  + 注入 libc++_shared.so ($(stat -f%z "$CXX_BUILT") B)"
fi

echo "[2/7] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q "\[$TAG\]" "$WORK/check.so" || { echo "ERROR: gradle APK 内 so 不是 $TAG"; exit 1; }
[[ "$(md5 -q "$WORK/check.so")" == "$(md5 -q "$SO_BUILT")" ]] \
  || { echo "ERROR: gradle APK 内 so 与本次构建产物 md5 不一致（旧 so 混包）"; exit 1; }
echo "  gradle APK so verified [$TAG] + md5 == 构建产物"

echo "[3/7] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[4/7] 收集 com/starry smali → ${INJECT_DEX}（不替换 Application）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -d "$SMALI_ROOT/com/starry/magic" ]] || { echo "ERROR: com/starry/magic smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] || { echo "ERROR: StarryInitProvider.smali missing"; exit 1; }
# 2026-10-28：运行时签名伪装（SignSpoof）已从源码删除 ⇒ 不再做"证书常量已烘焙"自检。
# 若要过本作的签名校验，按改包法在游戏 dex 侧处理。
# 环境阶段口径（2026-10-29）：现行双轨制下 TapLoginForceWeb 白名单属「停用件」（AGENTS §3 路由：白名单方案禁止复活），
# 本阶段不做任何登录补丁 ⇒ 只告警不阻断；若实测被 TapTap 校验拦，按「签名杀手 + 改包法」另开一轮处理。
if ! grep -aq -e "$PKG" -e "${PKG//./\/}" "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali"; then
  echo "  [warn] TapLoginForceWeb 白名单未含 ${PKG}：环境阶段不启用该方案，继续"
fi
"$SMALI_BIN" assemble -o "$WORK/$INJECT_DEX" "$SMALI_ROOT"
ls -la "$WORK/$INJECT_DEX"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
if [[ "$HAS_CXXSHARED" == "0" ]]; then
  CXX_SRC="$WORK/starry_dec/lib/arm64-v8a/libc++_shared.so"
  [[ -f "$CXX_SRC" ]] || { echo "ERROR: gradle APK 内缺 libc++_shared.so（jniLibs 未打包？）"; exit 1; }
  cp "$CXX_SRC" "$WORK/lib/arm64-v8a/"
  echo "  + extras: libc++_shared.so $(stat -f%z "$CXX_SRC") B"
fi
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
  echo "  + extras: assets/certs/$(ls "$WORK/starry_dec/assets/certs" | tr '\n' ' ')"
fi

# ---- 签名校验 killer dex（B 轨；MT ApkSignatureKillerEx 形态，取自斌哥包 classes7.dex 并把 .super 改回 Application）----
KILLER_ASSET="$NIXIANG/04_scripts/失落城堡2/assets/slcb2_signature_killer.dex"
EXTRA_DEX=""
if [[ -f "$KILLER_ASSET" ]]; then
  cp "$KILLER_ASSET" "$WORK/classes3.dex"
  EXTRA_DEX="classes3.dex"
  n=$(LC_ALL=C grep -ac "KillerApplication193" "$WORK/classes3.dex" || true)
  [[ "${n:-0}" -ge 1 ]] || { echo "ERROR: killer dex 内无 KillerApplication193"; exit 1; }
  # 官方证书必须已烘焙（CN=lt / MIID…DER base64）
  certn=$(LC_ALL=C grep -ac "MIIDOzCCAiOgAwIBAgIEDD0tNDAN" "$WORK/classes3.dex" || true)
  [[ "${certn:-0}" -ge 1 ]] || { echo "ERROR: killer dex 内无官方证书常量"; exit 1; }
  echo "  + $EXTRA_DEX (killer, $(stat -f%z "$WORK/classes3.dex") B, cert baked ok)"
else
  echo "  [warn] 无 killer dex 资产，跳过签名过检注入"
fi

echo "[5/7] binary AndroidManifest（ContentProvider 注入；不替换 Application；加 initOrder=MAX）"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile, shutil
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
import sys
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
assert 'com.netease.nis.wrapper.MyApplication' in t, "官方 Application 不是易盾 MyApplication，需人工确认"
assert 'com.starry.magic.StarryInitProvider' not in t
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append(f'    <uses-permission android:name="android.permission.{perm}"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# ★照抄斌哥包配置（2026-10-29）：斌哥包 AndroidManifest 带 android:debuggable="true"
#   （aapt2 实证）。debuggable 让本进程拿到 debug seinfo ⇒ SELinux 放行可执行匿名内存，
#   这正是他的 Dobby 跳板（trampoline）能跑、而我们之前必崩的差异点。1:1 照抄，不自创。
t = t.replace('android:debuggable="false"', 'android:debuggable="true"')
if 'android:debuggable' not in t:
    t = t.replace("<application ", '<application android:debuggable="true" ', 1)

# ★ 易盾铁律：声明位置保持追加在 </application> 之前，只加 initOrder=MAX
prov = f'''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="{pkg}.starryinit"
            android:exported="false"
            android:initOrder="2147483647" />
'''
t = t.replace("</application>", prov + "\n    </application>", 1)
svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print(f"  text Manifest patched (StarryInitProvider+initOrder + MenuService, perms={len(perms)})")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/7] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$INJECT_DEX" "$EXTRA_DEX" <<'PY'
import sys, zipfile, hashlib, re
from pathlib import Path
src, work, dst, inject = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
extra = sys.argv[5] if len(sys.argv) > 5 else ""
mf = (work / "AndroidManifest.xml").read_bytes()
dex = (work / inject).read_bytes()
injects = [inject] + ([extra] if extra else [])
inject_data = {n: (work / n).read_bytes() for n in injects}
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

# ★只删 v1 签名文件（MANIFEST.MF / *.SF / *.RSA / *.DSA / SIG-*），
#   必须保留 META-INF/services/**（kotlinx-coroutines 等 ServiceLoader 需要）
SIG_RE = re.compile(r'^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$', re.I)
dropped = []
zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if SIG_RE.match(name) or name.endswith("/"):
        dropped.append(name); continue
    if name in extras or name in injects:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
for _n in injects:
    info = zipfile.ZipInfo(_n, (2026, 10, 1, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, inject_data[_n])
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 10, 1, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
print(f"  dropped v1 signatures: {dropped}")

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest(), "arsc 被改"
for must in ("classes.dex", "lib/arm64-v8a/libil2cpp.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"):
    assert hashlib.sha256(zin2.read(must)).hexdigest() == hashlib.sha256(zout2.read(must)).hexdigest(), f"{must} 被改"
assert inject in zout2.namelist() and "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
inj = set(extras) | {inject, "AndroidManifest.xml"}
n_same = n_checked = 0
for item in zin2.infolist():
    name = item.filename
    if SIG_RE.match(name) or name.endswith("/") or name in inj:
        continue
    n_checked += 1
    if hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest():
        n_same += 1
print(f"  merged ok; 官方条目透传 {n_same}/{n_checked}; inject={sorted(inj)}")
if n_same != n_checked:
    raise SystemExit("ERROR: 官方条目 hash 不一致（外科透传破坏）")
zin2.close(); zout2.close()
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8 || true
else
  echo "ERROR: missing $KS"; exit 1
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED"

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
[[ "$(md5 -q "$WORK/final.so")" == "$(md5 -q "$SO_BUILT")" ]] \
  || { echo "ERROR: 成品包内 so 与本次构建产物 md5 不一致"; exit 1; }
echo "  final apk so md5 == 构建产物 ok"
shasum -a 256 "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/slcb2_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      # 🩸 2026-10-01 事故：`pm uninstall -k --user 0` 会留下"保留数据"的包记录
      # （pm list packages 查不到、pm path 为空、pm list packages -u 仍在），此后
      # 任何 `pm install -r -d --user 0` 都报
      #   INSTALL_FAILED_UPDATE_INCOMPATIBLE: Existing package ... signatures do not match
      # 删 /data/data、/data/user/0、/data/user_de/0 均无效。
      # 唯一有效解法 = `pm uninstall <pkg>`（不带 --user 0）清掉记录，再 install。
      # 故 -k 路线不可靠：KEEP_DATA 只用于"同签重装"，异签一律走全量卸载。
      if [[ "${STARRY_KEEP_DATA:-0}" == "1" ]]; then
        echo "[install] 异签 → 先试 uninstall -k（保留数据）"
        adb shell "su -c 'pm uninstall -k --user 0 $PKG'" 2>/dev/null || true
        if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
          echo "[install] cover ok (keep-data)"
        else
          echo "[install] keep-data 记录已污染，清除包记录后重装（★ 数据会丢）"
          adb shell "su -c 'pm uninstall $PKG'" 2>/dev/null || true
          adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
        fi
      else
        echo "[install] 异签 → uninstall（不带 --user 0，清包记录）+ reinstall（数据会丢）"
        adb shell "su -c 'pm uninstall -k --user 0 $PKG'" 2>/dev/null || true
        adb shell "su -c 'pm uninstall $PKG'" 2>/dev/null || true
        adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
      fi
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "日志（★ logcat -s 是前缀匹配，带方括号的 TAG 用 grep）："
  echo '  adb logcat -d | grep -E "\[Slcb2\]|Mod_Menu|MenuService|StarryTapLogin|StarryInitProvider"'
fi
echo "[done] $SIGNED"
