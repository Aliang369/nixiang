#!/usr/bin/env bash
# 绿梦：时空之声 1.0.5 — 外科组装 + 签名（禁止对游戏 apktool b 整包）
#
# 产物：08_output/绿梦/绿梦_1.0.5_starry-releaseSigned.apk
#
# 本作特殊性（Unity il2cpp + HybridCLR + FairGuard，业务全在热更 Assembly-ACT.dll）：
#   ★ 不 patch 游戏任何 dex：classes.dex / classes2.dex / classes3.dex 逐字节不变
#     （启动时机靠 Manifest 声明 Provider 拿，见
#      com/starry/magic/lvmeng/LvmengPayloadProvider.java 的类注释）
#   新增 classes4.dex = 全部 com/starry/**（框架 + 本作 com/starry/magic/lvmeng 落地引导）
#   换 AndroidManifest.xml（+3 权限 + MenuService + StarryInitProvider + LvmengPayloadProvider）
#   加 lib/arm64-v8a/{libstarry_magic.so,libc++_shared.so}
#   加 assets/lvmeng/{__data,__info,starry_lvmeng.dll}
#     （补丁 bundle 与 __info 由 Provider 落到 YooAsset Dll 缓存目录，并补 CacheRecords.snapshot）
#
# 用法:
#   bash scripts/surgical_lvmeng_1.0.5.sh
#   STARRY_INSTALL=1 bash scripts/surgical_lvmeng_1.0.5.sh
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
ORIG_APK="/tmp/lvmeng_apk/base.apk"
OUT="$NIXIANG/08_output/绿梦"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.zhuziplay.lvmengnew.op"
SIGNED="$OUT/绿梦_1.0.5_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/绿梦_1.0.5_starry-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/绿梦/surgical_1.0.5/AndroidManifest.xml"
ORIG_MD5="e59d4250747bd4e2ed1d80e231e6b272"
SO_PREBUILT="${LVMENG_SO:-$STARRY_SRC/libs_lvmeng/arm64-v8a/libstarry_magic.so}"
PAYLOAD_DIR="$NIXIANG/07_patched/绿梦/payload2"
MOD_DLL_SRC="$NIXIANG/04_scripts/绿梦/mod/bin/Release/StarryLvmeng.dll"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no zipalign in build-tools"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 build_so 阶段）"; exit 1; }
[[ -f "$PAYLOAD_DIR/__data" ]] || { echo "ERROR: missing $PAYLOAD_DIR/__data"; exit 1; }
[[ -f "$PAYLOAD_DIR/__info" ]] || { echo "ERROR: missing $PAYLOAD_DIR/__info"; exit 1; }
[[ -f "$MOD_DLL_SRC" ]] || { echo "ERROR: missing $MOD_DLL_SRC"; exit 1; }
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] \
  || { echo "ERROR: 官包指纹不符（期望 ${ORIG_MD5}，实际 ${ACTUAL_MD5}）"; exit 1; }

# 载荷自洽：__info（u16LE 长度 + ASCII CRC + i64LE 大小）必须与 __data 对得上
python3 - "$PAYLOAD_DIR/__data" "$PAYLOAD_DIR/__info" <<'PY'
import struct, sys, zlib
b = open(sys.argv[1], 'rb').read()
info = open(sys.argv[2], 'rb').read()
if len(info) != 18:
    sys.exit("ERROR: __info 应为 18 B，实际 %d" % len(info))
n = struct.unpack('<H', info[:2])[0]
crc = info[2:2 + n].decode('ascii', 'replace')
size = struct.unpack('<q', info[2 + n:2 + n + 8])[0]
want_crc = struct.pack('<I', zlib.crc32(b) & 0xFFFFFFFF).hex()
if size != len(b):
    sys.exit("ERROR: __info size=%d != __data %d" % (size, len(b)))
if crc != want_crc:
    sys.exit("ERROR: __info crc=%s != 实算 %s" % (crc, want_crc))
print("  payload 自洽：__data %d B crc=%s" % (len(b), crc))
PY

LOCK_DIR="/tmp/lvmeng_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/lvmeng_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle ${GRADLE_TASK}（starry → smali）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_lvmeng/' --exclude 'libs_lvmeng/' --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → 收集 com/starry smali"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing（starry 代码没编进 apk）"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/lvmeng/LvmengPayloadProvider.smali" ]] \
  || { echo "ERROR: lvmeng/LvmengPayloadProvider.smali missing（落地引导没编进 starry app）"; exit 1; }
echo "[2/6] assemble → classes4.dex"
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -la "$WORK/classes4.dex"

# ---- 暂存要加进游戏包的资产 ----
mkdir -p "$WORK/lib/arm64-v8a" "$WORK/assets/lvmeng"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so" \
  || echo "  WARN: libc++_shared.so 未找到（${NDK_CXX}），若启动崩再补"
# 补丁 bundle ＋ mod dll：Provider 首启把它们落到 YooAsset Dll 缓存
cp "$PAYLOAD_DIR/__data" "$WORK/assets/lvmeng/__data"
cp "$PAYLOAD_DIR/__info" "$WORK/assets/lvmeng/__info"
cp "$MOD_DLL_SRC" "$WORK/assets/lvmeng/starry_lvmeng.dll"
# Stage 6：把完整原厂 base.apk 作为 STORED 资产嵌入（FairGuard 输入侧伪装用；运行时抽到 files/fg_orig.apk）
[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺原厂包 ${ORIG_APK}（输入伪装需要它）" >&2; exit 1; }
cp "$ORIG_APK" "$WORK/assets/lvmeng/fg_orig.apk"
echo "  assets/lvmeng: __data $(wc -c < "$WORK/assets/lvmeng/__data" | tr -d ' ') B" \
  "/ __info $(wc -c < "$WORK/assets/lvmeng/__info" | tr -d ' ') B" \
  "/ starry_lvmeng.dll $(wc -c < "$WORK/assets/lvmeng/starry_lvmeng.dll" | tr -d ' ') B"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] binary AndroidManifest（加菜单权限/服务/Provider，保留原 Application）"
if [[ -n "${LVMENG_BINARY_MANIFEST:-}" && -f "$LVMENG_BINARY_MANIFEST" ]]; then
  cp "$LVMENG_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $LVMENG_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  首次生成 binary Manifest（apktool d -s + b 抽取 Manifest）"
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")

perms = []
for perm in ("android.permission.SYSTEM_ALERT_WINDOW",
             "android.permission.FOREGROUND_SERVICE",
             "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append('    <uses-permission android:name="%s"/>' % perm)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("  +permissions: %d" % len(perms))

# 悬浮菜单服务（框架）
if "com.starry.magic.menu.MenuService" not in t:
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
    print("  +MenuService")

# 框架启动 Provider：加载 libstarry_magic.so + SignSpoof + 起菜单
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.zhuziplay.lvmengnew.op.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("  +StarryInitProvider")

# 本作落地引导：把补丁 bundle / mod dll 写进游戏缓存并补 CacheRecords.snapshot
if "com.starry.magic.lvmeng.LvmengPayloadProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.lvmeng.LvmengPayloadProvider"
            android:authorities="com.zhuziplay.lvmengnew.op.lvmengpayload"
            android:exported="false"
            android:initOrder="101" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("  +LvmengPayloadProvider")
p.write_text(t, encoding="utf-8")
print("  text Manifest patched（保留原 Application）")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] zip 外科合并（游戏 dex 零改动）"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib, shutil
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c4 = (work / "classes4.dex").read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p
for sub in ("lvmeng", "certs"):
    base = work / "assets" / sub
    if base.is_dir():
        for p in base.rglob("*"):
            if p.is_file():
                extras["assets/%s/%s" % (sub, str(p.relative_to(base)).replace("\\", "/"))] = p

replace = {"AndroidManifest.xml": mf}
untouched = ["classes.dex", "classes2.dex", "classes3.dex", "resources.arsc"]

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue                      # 旧签名：重签前必须剔除
    if name in extras:
        continue                      # 由下面的新增块写入
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name in replace:
        zout.writestr(info, replace[name])
    else:                             # 流式拷贝，避免把 179MB 资源读进内存
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1

info = zipfile.ZipInfo("classes4.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
n += 1

for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    # fg_orig.apk 必须 STORED：native 侧用最小 zip 解析按"本地头 + 数据偏移"直接 pread，不解压
    info.compress_type = zipfile.ZIP_STORED if name.endswith("assets/lvmeng/fg_orig.apk") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name].read_bytes())
    n += 1
zin.close(); zout.close()

# ---- 校验：三个游戏 dex 与 arsc 字节不变 + 新件到位 ----
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), "changed: " + name
names = zout2.namelist()
for need in ("classes4.dex", "lib/arm64-v8a/libstarry_magic.so",
             "assets/lvmeng/__data", "assets/lvmeng/__info",
             "assets/lvmeng/starry_lvmeng.dll", "assets/lvmeng/fg_orig.apk", "AndroidManifest.xml"):
    assert need in names, "missing: " + need
assert hashlib.sha256(zin2.read("AndroidManifest.xml")).hexdigest() != \
       hashlib.sha256(zout2.read("AndroidManifest.xml")).hexdigest(), "manifest not patched"
print("  merged ok; entries≈%d; classes4=%d B; 未触碰 4/4 字节不变" % (n, len(c4)))
zin2.close(); zout2.close()
PY

echo "[5/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
echo "[5/6] done → $SIGNED (md5 $(md5 -q "$SIGNED"))"

echo "[6/6] 闭环校验"
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/extracted.so"
SO_BUILD_MD5=$(md5 -q "$SO_PREBUILT")
SO_PKG_MD5=$(md5 -q "$WORK/extracted.so")
[[ "$SO_BUILD_MD5" == "$SO_PKG_MD5" ]] \
  || { echo "ERROR: 包内 so != 构建产物 md5 ($SO_BUILD_MD5 vs $SO_PKG_MD5)"; exit 1; }
echo "  包内 so md5 一致 = $SO_BUILD_MD5"
unzip -l "$SIGNED" | grep -E "classes4.dex|assets/lvmeng/|libstarry_magic.so" || true
"$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | head -4 || true

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  SERIAL="${SERIAL:-127.0.0.1:16384}"
  echo "[install] push + pm install -r → $SERIAL"
  adb -s "$SERIAL" push "$SIGNED" /data/local/tmp/lvmeng_surgical.apk >/dev/null
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "pm install -r -d --user 0 /data/local/tmp/lvmeng_surgical.apk" \
    || { echo "ERROR: 覆盖安装失败（异签名须先卸载并获确认）"; exit 1; }
  adb -s "$SERIAL" shell rm -f /data/local/tmp/lvmeng_surgical.apk
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" \
    || { echo "ERROR: 装机指纹校验失败"; exit 1; }
  echo "[install] done"
fi

echo "✅ 外科组装完成：$SIGNED"
