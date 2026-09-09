#!/usr/bin/env bash
# 战魂铭人 3.5.0 TapTap — 外科组装（官方 classes1–3 / resources.arsc 字节级保留，只增不改）
#
# 产物：08_output/战魂铭人/战魂铭人_3.5.0_surgical-releaseSigned.apk
#
# 方案（2026-08-29，对比 3.4.0 inject_zhmr.sh 整包 apktool b 已禁用）：
#   - StarryApp.smali .super 改为官方 com.chillyroomsdk.sdkbridge.BasePlayerApplication
#     （Manifest 只换 android:name，官方 Application 链经 super 照常执行）
#   - 星光 smali → classes4.dex（原包 classes1–3，下一连续号；ART 连续 dex 规则）
#   - 二进制 Manifest：apktool d -s 改文本（App 名 + FGS 权限 + MenuService）→ apktool b
#     抽取并缓存到 07_patched/战魂铭人/surgical/AndroidManifest.xml
#   - zip 合并：官方 dex/arsc sha256 断言；arsc 必须 STORED
#
# 用法:
#   bash scripts/surgical_zhmr_3.5.0.sh
#   STARRY_INSTALL=1 bash scripts/surgical_zhmr_3.5.0.sh
#
# 依赖：Android.mk 已启用 STARRY_GAME_ZHMR；JDK 17；smali-2.5.2.jar
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
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/战魂铭人/战魂铭人_3.5.0_taptap_from_device.apk"
MF_CACHE="$NIXIANG/07_patched/战魂铭人/surgical/AndroidManifest.xml"
OUT="$NIXIANG/08_output/战魂铭人"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.chillyroom.zhmr.yw"
SIGNED="$OUT/战魂铭人_3.5.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/战魂铭人_3.5.0_surgical-debugSigned.apk"

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
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZHMR' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZHMR (see SWITCH_GAME.md)"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DSTARRY_GAME_SELF' "$MK" \
  && { echo "ERROR: STARRY_GAME_SELF still enabled"; exit 1; }
echo "[check] Android.mk target=zhmr"

# 装机前自检：jniLibs so 必须是本游戏（starry-mod/SKILL.md 硬规则）
if [[ -f "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so" ]]; then
  strings "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so" | grep -q '\[Zhmr\]' \
    || { echo "ERROR: jniLibs so 无 [Zhmr] 标签——先 ndk-build 并拷贝覆盖 jniLibs"; exit 1; }
  echo "[check] jniLibs so 标签=[Zhmr] ok"
fi

LOCK_DIR="/tmp/zhmr_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zhmr_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle $GRADLE_TASK (starry 小模板，唯一允许 apktool d 的工程)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
[[ -d "$WORK/starry_dec/smali" ]] || [[ -d "$WORK/starry_dec/smali_classes2" ]] \
  || { echo "ERROR: no smali in starry decode"; exit 1; }

echo "[3/6] collect com/starry smali → classes4.dex + 官方 App 继承补丁"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -d "$SMALI_ROOT/com/starry" ]] || { echo "ERROR: com/starry smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing — rebuild after adding StarryApp.java"; exit 1; }
# 运行时必须继承官方 Application（BasePlayerApplication），否则权限链/SDK init 秒崩
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
OFFICIAL = "Lcom/chillyroomsdk/sdkbridge/BasePlayerApplication;"
repls = [
    (".super Landroid/app/Application;", ".super " + OFFICIAL),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, " + OFFICIAL + "-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, " + OFFICIAL + "->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, " + OFFICIAL + "->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
        print(f"  patch: {a[:52]}…")
if OFFICIAL not in t:
    raise SystemExit("ERROR: StarryApp.smali missing official App references")
# 只有三类引用必须清零：.super / invoke-direct <init> / invoke-super。
# 其余（Application;->getProcessName、registerActivityLifecycleCallbacks 等静态/
# 虚调用）经继承链解析合法，保留。
bad = [ln.strip() for ln in t.splitlines()
       if (".super Landroid/app/Application;" in ln
           or "invoke-direct {p0}, Landroid/app/Application;" in ln
           or "invoke-super " in ln and "Landroid/app/Application;" in ln)]
if bad:
    raise SystemExit("ERROR: StarryApp.smali still has Application inheritance refs: "
                     + " | ".join(bad[:3]))
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.smali official App wiring ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -la "$WORK/classes4.dex"

echo "[4/6] binary AndroidManifest（App 名换 StarryApp + FGS 权限 + MenuService）"
if [[ -n "${ZHMR_BINARY_MANIFEST:-}" && -f "$ZHMR_BINARY_MANIFEST" ]]; then
  cp "$ZHMR_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using ZHMR_BINARY_MANIFEST=$ZHMR_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate once: apktool d -s 官方包 → 改文本 → apktool b 抽 Manifest"
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  # 只留 Manifest/arsc 所需目录，砍掉 900MB 资产加快 apktool b
  rm -rf "$WORK/orig_dec/assets" "$WORK/orig_dec/lib" "$WORK/orig_dec/unknown" 2>/dev/null || true
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
old_app = 'android:name="com.chillyroomsdk.sdkbridge.BasePlayerApplication"'
if old_app in t:
    t = t.replace(old_app, 'android:name="com.starry.magic.StarryApp"', 1)
    print("  manifest: application → com.starry.magic.StarryApp")
elif "com.starry.magic.StarryApp" in t:
    print("  manifest: application already StarryApp")
else:
    raise SystemExit("ERROR: BasePlayerApplication not found in manifest")
perms = []
if 'name="android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n    " + t[i:].lstrip()
    print("  manifest: + FGS permissions")
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
if "com.starry.magic.menu.MenuService" not in t:
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("  manifest: MenuService added")
p.write_text(t, encoding="utf-8")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4b/6] collect lib + certs from starry decode"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
strings "$SO_SRC" | grep -q '\[Zhmr\]' || { echo "ERROR: gradle 包内 so 无 [Zhmr] 标签"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/6] zip surgical merge (官方 classes1–3 + arsc 字节级保留)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

class RawNameZipInfo(zipfile.ZipInfo):
    """按原始字节写条目名（flag 不置 0x800）。
    Unity APK 里部分 AssetBundle 名是「UTF-8 字节但不带 UTF-8 flag」；python zipfile
    重写时会把名字重编码并强设 0x800，Unity/Android zip 读取器路径对不上 →
    "Unable to open archive file" → 加载条卡死（3.5.0 实锤 28 条）。"""
    _raw_name = b""
    def _encodeFilenameFlags(self):
        if self._raw_name:
            return self._raw_name, 0
        return super()._encodeFilenameFlags()

def make_info(item):
    raw = item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if (item.flag_bits & 0x800) == 0 and raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw  # 名字字节原样保留，flag 保持 0
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    return info

def is_official_dex(name: str) -> bool:
    if name == "classes.dex":
        return True
    if name.startswith("classes") and name.endswith(".dex"):
        mid = name[len("classes"):-len(".dex")]
        if mid.isdigit() and 2 <= int(mid) <= 3:
            return True
    return False

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue  # 新增项在下方统一写入
    if name == "AndroidManifest.xml":
        data = mf
    elif name == "classes4.dex":
        continue  # 星光 dex 下方新写
    else:
        data = zin.read(item)
    info = make_info(item)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED  # AfkAssets 要求 STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    zout.writestr(info, data)
    seen.add(item.orig_filename)
    n += 1

c4info = RawNameZipInfo("classes4.dex")
c4info.compress_type = zipfile.ZIP_DEFLATED
c4info.create_system = 3
zout.writestr(c4info, c4)
seen.add("classes4.dex")

for name, data in sorted(extras.items()):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED  # extractNativeLibs=true，DEFLATED 可安装
    einfo.create_system = 3
    zout.writestr(einfo, data)
    seen.add(name)
    n += 1

zin.close()
zout.close()
# 自检 1：官方 dex / arsc 哈希一致（外科铁律）
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for item in zin2.infolist():
    name = item.orig_filename
    if name == "resources.arsc" or is_official_dex(name):
        h1 = hashlib.sha256(zin2.read(item)).hexdigest()
        h2 = hashlib.sha256(zout2.read(name)).hexdigest()
        assert h1 == h2, f"PROTECTED MUTATED: {name}"
# 自检 2：条目名字节与 flag 全量对齐（防 Unity bundle 名编码回归）
raw_mismatch = flag_mismatch = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name in ("AndroidManifest.xml", "classes4.dex"):
        continue
    m = zout2.getinfo(name)
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw_mismatch += 1
    if (item.flag_bits & 0x800) != (m.flag_bits & 0x800):
        flag_mismatch += 1
assert raw_mismatch == 0 and flag_mismatch == 0, \
    f"name bytes mismatch={raw_mismatch} flag mismatch={flag_mismatch}"
missing = [x for x in ("classes4.dex", "lib/arm64-v8a/libstarry_magic.so") if x not in seen]
assert not missing, f"missing injectables: {missing}"
print(f"  merged ok; entries≈{n}; classes4={len(c4)} bytes; official dex/arsc sha256 match")
print(f"  name-bytes & utf8-flag 全量一致（raw_mismatch=0 flag_mismatch=0）")
zin2.close()
zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  # 默认覆盖安装（禁卸载：清数据 = Mod + 热更资源双杀）
  REMOTE="/data/local/tmp/zhmr_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || adb install -r "$SIGNED"
  echo "seed: bash 04_scripts/_common/ensure_starry_device_id_seed.sh"
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Zhmr:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
