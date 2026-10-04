#!/usr/bin/env bash
# 消消英雄2 1.2.27 (com.blina.match2.tt) — 外科组装（官方 TapTap 原版 → 星光版）
#
# 原包零改动：classes.dex / classes2.dex / resources.arsc / 全部官方 lib/*.so /
#             assets/bin/Data/** (global-metadata.dat) 字节不变。
# 改动：
#   - AndroidManifest：保留官方 cn.fly.FlyApplication（不替换 Application），
#     仅追加 StarryInitProvider + MenuService + 前台服务权限
#   - 新增 classes3.dex（星光框架 smali）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#   - 保留官方 META-INF 非签名条目，移除旧签名条目（re-sign 由 sign 脚本做）
#
# 菜单功能（Games/Match2Mod.h，与 1.2.26 文档一致，不新增）：
#   1 无敌(AN_2) / 2 十倍伤害(AN_1) / 3 瞬间结束战斗(AN_3)
#
# 用法:
#   XXH2_BASE_APK=/path/to/official_1.2.27.apk bash scripts/surgical_xxh2_1.2.27.sh
# 默认底包: /tmp/xxh2_base_1.2.27.apk（真机只读拉取；身份见
#           10_docs/_state/底包再获取台账.md 的「消消英雄2」行）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
GAME="消消英雄2"
VERSION="1.2.27"
PKG="com.blina.match2.tt"
ORIG_APK="${XXH2_BASE_APK:-/tmp/xxh2_base_1.2.27.apk}"
ORIG_MD5="b02e68afe84b413810a38dbe30d36dbe"
ORIG_SHA256="f3162ee15437140c4e8cec0f6f2e0f020c7bfe3ef660268adff064ce15307f95"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes3.dex"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PY="python3.11"
command -v "$PY" >/dev/null 2>&1 || PY="python3"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺底包 ${ORIG_APK}（渠道重下或用 XXH2_BASE_APK= 指定）" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺 $SMALI_BIN" >&2; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: 缺 NDK $NDK" >&2; exit 1; }

echo "[0/6] 底包身份核对"
GOT_MD5="$(md5 -q "$ORIG_APK")"
GOT_SHA="$(shasum -a 256 "$ORIG_APK" | awk '{print $1}')"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 底包 md5 不符（$GOT_MD5 ≠ ${ORIG_MD5}）" >&2; exit 1; }
[[ "$GOT_SHA" == "$ORIG_SHA256" ]] || { echo "ERROR: 底包 sha256 不符" >&2; exit 1; }
echo "  ok 官方原版 1.2.27/vc188（md5 ${GOT_MD5}）"

# Android.mk 必须已切到 MATCH2（legacy 单例激活分支）
grep -qE '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_MATCH2' "$STARRY_SRC/app/src/main/jni/Android.mk" \
  || { echo "ERROR: Android.mk 未激活 STARRY_GAME_MATCH2" >&2; exit 1; }

LOCK_DIR="/tmp/xxh2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held（另一次 surgical 在跑）" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/xxh2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK"

# 中文路径（mod菜单/消消英雄2）会让 gradle spawn ndk-build 与 aapt2 失败 → 全程在 /tmp ASCII 目录里构建
echo "[1/6] ndk-build libstarry_magic.so（STARRY_GAME_MATCH2）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
APP="$WORK/starry/app"
LUA_TRACE="${STARRY_LUA_DUMP_TRACE:-0}"
EXTRA_CFLAGS=""
if [[ "$LUA_TRACE" == "1" ]]; then
  EXTRA_CFLAGS="-DMATCH2_LUA_DUMP_TRACE=1"
  echo "  (MATCH2_LUA_DUMP_TRACE=1 → lua dump)"
fi
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  APP_CFLAGS="$EXTRA_CFLAGS" \
  -j8 >/dev/null
SO_PREBUILT="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: ndk-build 未产出 so" >&2; exit 1; }
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$SO_PREBUILT" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
SO_MD5="$(md5 -q "$SO_PREBUILT")"
echo "  so=$(stat -f%z "$SO_PREBUILT") B md5=$SO_MD5"
grep -aq '\[Match2\]' "$SO_PREBUILT" || { echo "ERROR: so 缺 [Match2] 标签" >&2; exit 1; }
grep -aq 'xxh2' "$SO_PREBUILT" || { echo "ERROR: so 缺 game_id xxh2" >&2; exit 1; }
if grep -aq '__starry_ac_ok' "$SO_PREBUILT"; then echo "ERROR: so 仍含过检测标记 __starry_ac_ok" >&2; exit 1; fi
# 防"编错游戏"：其它游戏的日志标签一律不得出现（bash 3.2 无 ${var^}，写死清单）
for t in Neon Dzqs Cstjjs Gjc2 Whrhx Sc33 Wggls Pcr Hzsj Zxr Cwjh Shhj; do
  if grep -aq "\[$t\]" "$SO_PREBUILT" 2>/dev/null; then echo "ERROR: so 含其它游戏标签 [$t]" >&2; exit 1; fi
done

echo "[2/6] gradle assembleRelease → com/starry smali → $NEW_DEX"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain >/dev/null)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release-unsigned.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: 缺 starry APK" >&2; exit 1; }
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for f in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/SignSpoof.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali" ; do
  [[ -f "$SMALI_ROOT/$f" ]] || { echo "ERROR: 框架 smali 缺 $f" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
grep -aq 'com.blina.match2.tt' "$WORK/$NEW_DEX" || { echo "ERROR: dex 内无 ${PKG}（SignSpoof/白名单未生效）" >&2; exit 1; }
grep -aq 'NativeMenuController' "$WORK/$NEW_DEX" || { echo "ERROR: dex 内无 NativeMenuController" >&2; exit 1; }
echo "  $NEW_DEX=$(stat -f%z "$WORK/$NEW_DEX") B"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] AndroidManifest（保留官方 cn.fly.FlyApplication）"
apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
"$PY" - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
# 悬浮窗权限：官方包没有，而 NativeMenu 用 TYPE_APPLICATION_OVERLAY 必须由 app manifest 声明，
# 否则 appops set SYSTEM_ALERT_WINDOW 会被系统忽略（adbd 侧即使 su 设了也读回 default），
# 菜单窗口 addView 会直接抛 permission denied。（2026-10-02 实测踩坑）
if "android.permission.SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if "android.permission.FOREGROUND_SERVICE\"" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0: raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
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
p.write_text(t, encoding="utf-8")
print("  manifest patched: provider + service + foreground + overlay permissions")
PY
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  "$PY" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || { echo "ERROR: FGS 权限 missing" >&2; exit 1; }
  grep -q "SYSTEM_ALERT_WINDOW" "$WORK/tree_new.txt" || { echo "ERROR: SYSTEM_ALERT_WINDOW 权限 missing（悬浮窗菜单必需）" >&2; exit 1; }
  grep -q "cn.fly.FlyApplication" "$WORK/tree_new.txt" || { echo "ERROR: 官方 Application 丢失" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $DIFF_D 行" >&2; exit 1; }
  echo "  aapt2 核对通过（官方节点 0 丢失，provider/service/权限在位）"
else
  echo "WARN: 缺 aapt2，跳过 xmltree 核对" >&2
fi

echo "[4/6] zip surgical merge → $UNSIGNED"
"$PY" - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$PKG" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, new_dex, pkg = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4], sys.argv[5]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
untouched = ["classes.dex", "classes2.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libxlua.so",
             "lib/arm64-v8a/libtapsdkcore.so", "lib/arm64-v8a/libthemis.so",
             "lib/arm64-v8a/libBugly_Native.so", "lib/arm64-v8a/libCtaApiLib.so",
             "lib/arm64-v8a/libfntvcrash.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"{new_dex} 已存在"
for n in untouched:
    assert n in names, f"底包缺 {n}"
def is_sig(name):
    if name == "META-INF/MANIFEST.MF": return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras: continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else (
        zipfile.ZIP_DEFLATED if name == "AndroidManifest.xml" else item.compress_type)
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data); n += 1
info = zipfile.ZipInfo(new_dex); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, cnew); n += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
    zout.writestr(info, data); n += 1
zin.close(); zout.close()
zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zout2.namelist())
for name in kept_meta: assert name in out_names, f"lost {name}"
assert new_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries≈{n}; {new_dex}={len(cnew)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[5/6] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$IN_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so md5 与源不一致" >&2; exit 1; }
unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/so_in.so"
grep -aq '\[Match2\]' "$WORK/so_in.so" || { echo "ERROR: 包内 so 缺 [Match2]" >&2; exit 1; }
grep -aq 'xxh2' "$WORK/so_in.so" || { echo "ERROR: 包内 so 缺 xxh2" >&2; exit 1; }
"$PY" - "$UNSIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
c3 = z.read("classes3.dex")
for s in (b"SignSpoof", b"TapLoginForceWeb", b"NativeMenuController", b"com.blina.match2.tt"):
    assert s in c3, f"classes3.dex 缺 {s!r}"
names = z.namelist()
assert "assets/web" not in "".join(n + "|" for n in names), "残留 assets/web"
print("  final apk: classes3.dex 含 SignSpoof/TapLoginForceWeb/NativeMenuController/pkg；无 assets/web")
PY

echo "[6/6] 完成（未签名）"
ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NOTE: 由 04_scripts/$GAME/xxh2_sign_$VERSION.sh 签名（starry_release.jks）"
