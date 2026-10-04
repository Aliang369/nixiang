#!/usr/bin/env bash
# 境界刀鸣 1.39.211507 (com.blade.bd, taptap 渠道, vc 39) — 外科 zip 组装（无 root 直装星光包）
#
# 注入方式：ContentProvider（StarryInitProvider），【不替换 Application】
#   官方 Application = com.bytedance.thanos.ThanosApplication（字节 Thanos 壳，替换会触发校验）
#   官方 appComponentFactory = androidx.core.app.CoreComponentFactory（保持原样）
#
# 注入 dex：官方已有 classes.dex … classes14.dex → 星光 dex 落在 classes15.dex（脚本动态推算）
# lib/arm64-v8a：新增 libstarry_magic.so（Jjdm 模块：伤害倍率 / 无敌 / 无限技能，Lua 玩法层注入）
#   官方已自带 libc++_shared.so（911696 B）→ **不覆盖**，避免与 il2cpp/game 的 C++ ABI 冲突
# 官方 classes*.dex / resources.arsc 原样保留（resources.arsc 必须 STORED）
#
# 底包：02_apks/境界刀鸣/境界刀鸣_1.39.211507_taptap_official_from_device.apk
#   1986468431 B / 14 个 dex / 官方签名 CN=ssgame, O=blade, OU=ssgame
#   SHA-256 D8527C2B53AC107970B239BF14E012461EAD2BF7C11FCE281F0878AE120AA76E
#   → 已烘焙进 app/src/main/java/com/starry/magic/sign/SignSpoof.java 的 JJDM_CERT_B64
#
# 构建：modern 注册表（Android_jjdm.mk + MainModern.cpp + Games/JjdmMod.cpp + Games/JjdmEntry.cpp）
#
# 用法:
#   bash scripts/surgical_jjdm_1.39.211507.sh
#   STARRY_INSTALL=1 bash scripts/surgical_jjdm_1.39.211507.sh
#   STARRY_ALLOW_UNINSTALL=1 ...        （签名不一致需卸载重装时显式给）
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
VER="1.39.211507"
PKG="com.blade.bd"
TAG="Jjdm"
ORIG_APK="$NIXIANG/02_apks/境界刀鸣/境界刀鸣_${VER}_taptap_official_from_device.apk"
OUT="$NIXIANG/08_output/境界刀鸣"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
SIGNED="$OUT/境界刀鸣_${VER}_surgical-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/境界刀鸣_${VER}_starry.apk"
MF_CACHE="$NIXIANG/07_patched/境界刀鸣/surgical_${VER}/AndroidManifest.xml"

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
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: missing $NDK/ndk-build"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android_jjdm.mk"
grep -q 'Games/JjdmMod.cpp' "$MK" || { echo "ERROR: Android_jjdm.mk 缺 Games/JjdmMod.cpp"; exit 1; }
grep -q 'Games/JjdmEntry.cpp' "$MK" || { echo "ERROR: Android_jjdm.mk 缺 Games/JjdmEntry.cpp"; exit 1; }
grep -q 'Games/SignalQuarantine.cpp' "$MK" || { echo "ERROR: Android_jjdm.mk 缺 Games/SignalQuarantine.cpp（MainModern.cpp 引用）"; exit 1; }
echo "[check] orig apk = $ORIG_APK"

LOCK_DIR="/tmp/jjdm_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held"; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/jjdm_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# 注入 dex 名 = 官方最大 classesN + 1（境界刀鸣官方到 classes14.dex）
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

echo "[1/6] ndk-build（Android_jjdm.mk → libs_jjdm/，绝对路径）"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_jjdm.mk \
  NDK_APPLICATION_MK=./Application.mk APP_ABI=arm64-v8a \
  NDK_OUT="$STARRY_SRC/obj_jjdm" NDK_LIBS_OUT="$STARRY_SRC/libs_jjdm" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" 2>&1 | grep -v 'Bad file descriptor' || true)
SO_BUILT="$STARRY_SRC/libs_jjdm/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q "\[$TAG\]" "$SO_BUILT" || { echo "ERROR: so 无 [$TAG] 标签（仍是上一款游戏）"; exit 1; }
for stale in '\[Wdyz\]' '\[Cwjh\]' '\[Mygwzd\]'; do
  grep -a -q "$stale" "$SO_BUILT" && { echo "ERROR: so 仍含上一款游戏标签 $stale"; exit 1; }
done
CERT_N=$(grep -a -c 'BEGIN CERTIFICATE' "$SO_BUILT" || true)
[[ "${CERT_N:-0}" -ge 1 ]] || { echo "ERROR: so 内无烘焙证书（授权 fail-closed）"; exit 1; }
for m in '伤害倍率' '无敌' '无限技能'; do
  N=$(LC_ALL=C grep -a -c "$m" "$SO_BUILT" || true)
  [[ "${N:-0}" -ge 1 ]] || { echo "ERROR: so 内缺菜单串 $m"; exit 1; }
done
echo "  so ok: $(md5 -q "$SO_BUILT") $(stat -f%z "$SO_BUILT") B (tag=$TAG cert=$CERT_N 中文串 ok)"
mkdir -p "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_BUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

echo "[2/6] gradle ${GRADLE_TASK}"
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

echo "[3/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[4/6] 收集 com/starry smali → ${INJECT_DEX}（Manifest 不替换 Application，StarryApp.super 保持 android.app.Application）"
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
# 签名伪装自检：com.blade.bd 证书常量必须已在 SignSpoof 里（smali 里包名是 "com.blade.bd" 点号形式）
SS_SMALI="$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali"
[[ -f "$SS_SMALI" ]] || { echo "ERROR: $SS_SMALI missing（R8 改名或 SignSpoof 未参与编译）"; ls "$SMALI_ROOT/com/starry/magic/sign/" || true; exit 1; }
if ! grep -aq -e 'com.blade.bd' -e 'com/blade/bd' "$SS_SMALI"; then
  echo "ERROR: SignSpoof.smali 内无 com.blade.bd 证书常量（先补 SignSpoof.java 的 JJDM_PKG/JJDM_CERT_B64 再 gradle）"
  grep -ao 'const-string[^,]*, "[A-Za-z0-9+/=]\{40,\}"' "$SS_SMALI" | head -3
  exit 1
fi
if ! grep -aq -e 'com.blade.bd' -e 'com/blade/bd' "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali"; then
  echo "ERROR: TapLoginForceWeb.smali 白名单缺 com.blade.bd"
  exit 1
fi
"$SMALI_BIN" assemble -o "$WORK/$INJECT_DEX" "$SMALI_ROOT"
ls -la "$WORK/$INJECT_DEX"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
# 官方已自带 libc++_shared.so → 不覆盖
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/6] binary AndroidManifest（ContentProvider 注入；不替换 Application）"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
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
assert 'com.bytedance.thanos.ThanosApplication' in t, "官方 Application 不是 ThanosApplication，需人工确认"
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
prov = f'''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="{pkg}.starryinit"
            android:exported="false" />
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
print(f"  text Manifest patched (StarryInitProvider + MenuService, perms={len(perms)})")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$INJECT_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, inject = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
dex = (work / inject).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

# --- libhhld-rt.so kill-switch NOP 补丁（0x18a44 SIGSEGV 修复，已真机验证）---
HH_PATCH = {
    "lib/arm64-v8a/libhhld-rt.so": (0x5c270, b"\x00\x01\x3f\xd6", b"\x1f\x20\x03\xd5"),
    "lib/x86_64/libhhld-rt.so":    (0x5f99d, b"\xff\x53\x08",     b"\x0f\x1f\x00"),
}
n_hh = 0

zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == inject:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    if name in HH_PATCH:
        off, old, new = HH_PATCH[name]
        if data[off:off + len(old)] != old:
            raise SystemExit(f"ERROR: hhld 补丁点不符 {name}@{hex(off)}: {data[off:off + len(old)].hex()}")
        data = data[:off] + new + data[off + len(new):]
        n_hh += 1
        print(f"  hhld NOP: {name} @{hex(off)} {old.hex()} -> {new.hex()}")
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
info = zipfile.ZipInfo(inject, (2026, 9, 22, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, dex)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 9, 22, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest(), "arsc 被改"
assert inject in zout2.namelist() and "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
inj = set(extras) | set(HH_PATCH) | {inject, "AndroidManifest.xml"}
n_same = n_checked = 0
for item in zin2.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj:
        continue
    n_checked += 1
    if hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest():
        n_same += 1
print(f"  merged ok; arsc/官方条目透传 {n_same}/{n_checked}; inject={inject}; hhld NOP {n_hh}/2")
if n_hh != 2:
    raise SystemExit("ERROR: hhld NOP 补丁未全部命中（arm64 + x86_64 各 1）")
if n_same != n_checked:
    raise SystemExit("ERROR: 官方条目 hash 不一致（外科透传破坏）")
zin2.close(); zout2.close()
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED"

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
[[ "$(md5 -q "$WORK/final.so")" == "$(md5 -q "$SO_BUILT")" ]] \
  || { echo "ERROR: 成品包内 so 与本次构建产物 md5 不一致"; exit 1; }
echo "  final apk so md5 == 构建产物 ok"
python3.11 - "$SIGNED" <<'PYV'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
want = {"lib/arm64-v8a/libhhld-rt.so": (0x5c270, b"\x1f\x20\x03\xd5"),
        "lib/x86_64/libhhld-rt.so":    (0x5f99d, b"\x0f\x1f\x00")}
for n, (off, exp) in want.items():
    got = z.read(n)[off:off + len(exp)]
    print(f"  hhld NOP 校验 {n} @{hex(off)} = {got.hex()} {'OK' if got == exp else 'FAIL'}")
    if got != exp:
        raise SystemExit("ERROR: 成品包内 hhld NOP 补丁校验失败")
PYV
shasum -a 256 "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/jjdm_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      echo "[install] signature mismatch → uninstall + reinstall"
      adb shell "su -c 'pm uninstall $PKG'" 2>/dev/null || true
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryInitProvider:V StarrySignSpoof:V StarryTapLogin:V StarryMagic:V MenuService:V Jjdm:V"
fi
echo "[done] $SIGNED"
