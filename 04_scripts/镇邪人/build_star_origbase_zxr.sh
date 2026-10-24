#!/usr/bin/env bash
# 镇邪人 1.0.3(vc11 taptap 原版) → 星空版外科重建（2026-10-24）
#
# 底包 = 设备在装的**官方原版** APK（99,870,824B / md5 0dab73812162a2cab8541431fb972af5）。
# 与 09-23/10-01 两轮的区别：那些轮次底包已是星光包（只换 dex/so + 钉 JSC）；
# 本轮底包是纯原版，必须把「菜单入口 / 免广告 / TapSign 签名固定」整套重新注进去：
#   1) 二进制 AndroidManifest：加 SYSTEM_ALERT_WINDOW/FOREGROUND_SERVICE(_SPECIAL_USE)
#      权限 + MenuService（应用类保持官方 JMYBTApplication，不替换成 StarryApp）
#   2) classes7.dex：JMYBTApplication.attachBaseContext 早期 loadLibrary(starry_magic)、
#      JMYBTActivity onCreate/onResume/onPause/onUserLeaveHint + get_ret/set_ret、
#      SdkManager.SdkShowAdReward 前置 shouldSkipAd（免广告）
#   3) assets/sq_plugin_*.apk 内 classes4.dex：TapSignKit.getSign 固定原版证书 MD5
#   4) classes8.dex + libstarry_magic.so + libc++_shared.so + assets/certs（07_patched 留档产物）
#   5) 引擎「包内命中优先」补丁 + 钉入已打战斗补丁的 src bundle（默认 d5507 + cb9dc）
#
# 用法：
#   ZXR_BASE_APK=/tmp/zxr_orig.apk bash 04_scripts/镇邪人/build_star_origbase_zxr.sh
#   ZXR_USE_RELEASE_KS=0 ... （默认 1，正式签名；底包是官方签名，装机必须先卸载）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
PY="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"
[[ -x "$PY" ]] || PY="$(command -v python3)"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
BASE_APK="${ZXR_BASE_APK:-}"
OUT="$NIXIANG/08_output/镇邪人"
MENU_DIR="${ZXR_MENU_DIR:-$NIXIANG/07_patched/镇邪人/menu_latest}"
PIN_VERS="${ZXR_PIN_VERS:-d5507 cb9dc}"
ORIG_CERT_MD5="${ZXR_ORIG_CERT_MD5:-8fa00ae1397609e5e09ff34cc43030e3}"
EXPECT_BASE_MD5="${ZXR_EXPECT_BASE_MD5:-0dab73812162a2cab8541431fb972af5}"
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_cb9dc_star-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"
MF_CACHE_DIR="$NIXIANG/07_patched/镇邪人/manifest_origbase_vc11"
MF_CACHE="$MF_CACHE_DIR/AndroidManifest.xml"

KS="$NIXIANG/01_tools/uber-debug.keystore"
KS_ALIAS="androiddebugkey"
KS_PASS="android"
if [[ "${ZXR_USE_RELEASE_KS:-1}" == "1" ]]; then
  KS="$NIXIANG/01_tools/starry_release.jks"
  KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
  KS_PASS="${STARRY_KS_PASS:-starry2026}"
fi

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools with zipalign not found"; exit 1; }

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -n "$BASE_APK" ]] || { echo "ERROR: 必须指定 ZXR_BASE_APK（官方原版底包，APK 不留本地）" >&2; exit 1; }
for f in "$BASE_APK" "$KS" "$SMALI_BIN" "$BAKSMALI_BIN" "$MENU_DIR/classes8.dex" "$MENU_DIR/libstarry_magic.so"; do
  [[ -e "$f" ]] || { echo "ERROR: missing input $f" >&2; exit 1; }
done
command -v apktool >/dev/null || { echo "ERROR: apktool not in PATH" >&2; exit 1; }

LOCK_DIR="/tmp/zxr_origbase_build.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held" >&2; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_origbase.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$MF_CACHE_DIR"

echo "[1/8] 校验底包身份（官方原版 vc11）"
"$PY" - "$BASE_APK" "$EXPECT_BASE_MD5" <<'PY'
import hashlib, sys, zipfile
p, want = sys.argv[1], sys.argv[2]
h = hashlib.md5(open(p, "rb").read()).hexdigest()
if h != want:
    raise SystemExit(f"ERROR: base md5 {h} != {want}")
z = zipfile.ZipFile(p); names = set(z.namelist())
need = ["AndroidManifest.xml", "classes7.dex", "assets/jsb-adapter/jsb-engine.js",
        "assets/assets/src/index.d5507.jsc", "assets/assets/src/config.d5507.json",
        "lib/arm64-v8a/libcocos2djs.so"]
miss = [n for n in need if n not in names]
if miss: raise SystemExit(f"ERROR: base missing {miss}")
plugins = [n for n in names if n.startswith("assets/sq_plugin_") and n.endswith(".apk")]
if len(plugins) != 1: raise SystemExit(f"ERROR: sq_plugin count={len(plugins)}")
if "classes8.dex" in names: raise SystemExit("ERROR: base 已含 classes8.dex（拿错底包，这是星光包）")
if b"com.starry.magic" in z.read("classes7.dex"):
    raise SystemExit("ERROR: base classes7 已含 starry（拿错底包）")
print(f"  base md5={h} entries={len(names)} plugin={plugins[0]}")
PY

echo "[2/8] 二进制 AndroidManifest（权限 + MenuService，保留官方 JMYBTApplication）"
if [[ -f "$MF_CACHE" ]]; then
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached ${MF_CACHE}"
else
  apktool d -f -s -o "$WORK/mf_dec" "$BASE_APK" >/dev/null 2>&1
  "$PY" - "$WORK/mf_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
if 'android:name="com.sanqi.jmybt.game37.JMYBTApplication"' not in t:
    raise SystemExit("ERROR: manifest application 不是 JMYBTApplication")
if "com.starry.magic" in t:
    raise SystemExit("ERROR: manifest 已含 starry 条目（拿错底包）")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "android.permission.FOREGROUND_SERVICE\"" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
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
p.write_text(t, encoding="utf-8")
print("  text manifest patched: perms=%d + MenuService" % len(perms))
PY
  rm -rf "$WORK/mf_dec"/smali "$WORK/mf_dec"/smali_classes* 2>/dev/null || true
  apktool b -f "$WORK/mf_dec" -o "$WORK/mf_only.apk" >/dev/null 2>&1
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → ${MF_CACHE}"
fi
"$PY" - "$WORK/AndroidManifest.xml" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
if d[:4] != b"\x03\x00\x08\x00":
    raise SystemExit("ERROR: 产出不是二进制 AXML")
for probe in (b"com.starry.magic.menu.MenuService", b"com.sanqi.jmybt.game37.JMYBTApplication"):
    if probe.decode().encode("utf-16-le") not in d and probe not in d:
        raise SystemExit(f"ERROR: manifest 缺 {probe!r}")
print(f"  binary manifest ok ({len(d)}B)")
PY

echo "[3/8] classes7.dex（菜单入口 + 免广告 smali 补丁）"
mkdir -p "$WORK/c7"
unzip -o -q -p "$BASE_APK" classes7.dex > "$WORK/classes7.dex"
"$BAKSMALI_BIN" d "$WORK/classes7.dex" -o "$WORK/c7/sm" >/dev/null
"$PY" "$NIXIANG/04_scripts/镇邪人/patch_origbase_zxr_smali.py" \
  --application "$WORK/c7/sm/com/sanqi/jmybt/game37/JMYBTApplication.smali" \
  --activity "$WORK/c7/sm/com/sanqi/jmybt/game37/JMYBTActivity.smali" \
  --sdkmanager "$WORK/c7/sm/com/sanqi/jmybt/game37/sdk/SdkManager.smali"
"$SMALI_BIN" a "$WORK/c7/sm" -o "$WORK/classes7.new.dex"
ls -l "$WORK/classes7.dex" "$WORK/classes7.new.dex"

echo "[4/8] sq_plugin 内 TapSignKit.getSign（固定原版证书 MD5）"
PLUGIN_NAME="$("$PY" - "$BASE_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
print([n for n in z.namelist() if n.startswith("assets/sq_plugin_") and n.endswith(".apk")][0])
PY
)"
echo "  plugin=${PLUGIN_NAME}"
unzip -o -q -p "$BASE_APK" "$PLUGIN_NAME" > "$WORK/plugin.apk"
"$PY" - "$WORK/plugin.apk" "$WORK/plugin_dec" <<'PY'
import sys, zipfile
from pathlib import Path
src, out = Path(sys.argv[1]), Path(sys.argv[2])
out.mkdir(parents=True, exist_ok=True)
z = zipfile.ZipFile(src)
z.extract("classes4.dex", out)
print(f"  plugin classes4.dex={z.getinfo('classes4.dex').file_size}B")
PY
"$BAKSMALI_BIN" d "$WORK/plugin_dec/classes4.dex" -o "$WORK/plugin_sm" >/dev/null
"$PY" "$NIXIANG/04_scripts/镇邪人/patch_origbase_zxr_smali.py" \
  --tapsignkit "$WORK/plugin_sm/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" \
  --orig-md5 "$ORIG_CERT_MD5"
grep -q "starry_sign_bypass" "$WORK/plugin_sm/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" \
  || { echo "ERROR: TapSignKit smali 未打补丁" >&2; exit 1; }
"$SMALI_BIN" a "$WORK/plugin_sm" -o "$WORK/plugin_dec/classes4.new.dex"
"$PY" - "$WORK/plugin.apk" "$WORK/plugin_dec/classes4.new.dex" "$WORK/plugin.patched.apk" "$ORIG_CERT_MD5" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src, dex, out, md5 = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
newdex = dex.read_bytes()
if md5.encode() not in newdex:
    raise SystemExit("ERROR: plugin classes4 未含签名固定常量")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(out, "w")
kept = 0
for item in zin.infolist():
    if item.filename.startswith("META-INF/"):
        continue          # 改过 dex ⇒ 旧签名必然失效，按 apktool b 同口径丢弃
    if item.filename == "classes4.dex":
        continue          # 后面单独写入打好补丁的版本
    data = zin.read(item.filename)
    info = zipfile.ZipInfo(item.filename, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data); kept += 1
info = zipfile.ZipInfo("classes4.dex"); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, newdex)
zin.close(); zout.close()
print(f"  plugin patched: kept={kept} sha256={hashlib.sha256(out.read_bytes()).hexdigest()[:16]}…")
PY

echo "[5/8] 引擎「包内命中优先」补丁"
unzip -o -q -p "$BASE_APK" assets/jsb-adapter/jsb-engine.js > "$WORK/jsb-engine.js"
"$PY" - "$WORK/jsb-engine.js" <<'PY'
import hashlib, sys
from pathlib import Path
p = Path(sys.argv[1])
with p.open("r", encoding="utf-8", newline="") as fh:   # 保留原始 CRLF/LF 混排，逐字节可复现
    s = fh.read()
old = """    var cache = cacheManager.getCache(url);
    if (cache) {
      inCache = true;
      url = cache;
    } else {
      var packagePath = getPackagePathFromRemoteUrl(url);
      if (checkExistsInPackage(packagePath)) {
        inLocal = true;
        url = packagePath;
      }
    }"""
new = """    var packagePath = getPackagePathFromRemoteUrl(url);
    if (checkExistsInPackage(packagePath)) {
      inLocal = true;
      url = packagePath;
    } else {
      var cache = cacheManager.getCache(url);
      if (cache) {
        inCache = true;
        url = cache;
      }
    }"""
if s.count(old) != 1:
    raise SystemExit(f"ERROR: engine anchor count={s.count(old)}")
t = s.replace(old, new, 1)
b = t.encode("utf-8")
p.write_bytes(b)
want = "22a6732420b27457c585e144c548d19ad5d2ff84a8df1d14c1320e9d329f9319"
h = hashlib.sha256(b).hexdigest()
if h != want:
    raise SystemExit(f"ERROR: engine sha256 {h} != {want}（与 09-29/10-01 正式包不一致）")
print(f"  engine package-first ok size={len(b)} sha256={h}")
PY

echo "[6/8] 外科合并（zip 级）"
"$PY" - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" "$MENU_DIR" "$PIN_VERS" "$PLUGIN_NAME" \
  "$NIXIANG/07_patched/镇邪人" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

base, work, dst, menu, pins, plugin = (
    Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4]), sys.argv[5].split(),
    sys.argv[6],
)
pins_dir = Path(sys.argv[7])
work = Path(work)
extras = {}
extras["AndroidManifest.xml"] = (work / "AndroidManifest.xml").read_bytes()
extras["classes7.dex"] = (work / "classes7.new.dex").read_bytes()
extras["classes8.dex"] = (menu / "classes8.dex").read_bytes()
extras["assets/jsb-adapter/jsb-engine.js"] = (work / "jsb-engine.js").read_bytes()
extras[plugin] = (work / "plugin.patched.apk").read_bytes()
extras["lib/arm64-v8a/libstarry_magic.so"] = (menu / "libstarry_magic.so").read_bytes()
cxx = menu / "libc++_shared.so"
if cxx.is_file():
    extras["lib/arm64-v8a/libc++_shared.so"] = cxx.read_bytes()
certs = menu / "certs"
if certs.is_dir():
    for p in sorted(certs.rglob("*")):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs))] = p.read_bytes()
for ver in pins:
    for fname, tail in ((f"index.{ver}.jsc", "jsc"), (f"config.{ver}.json", "json")):
        p = pins_dir / ver / fname
        if not p.is_file():
            raise SystemExit(f"ERROR: missing pin {p}")
        data = p.read_bytes()
        if tail == "jsc" and len(data) < 1_000_000:
            raise SystemExit(f"ERROR: pin {fname} 太小")
        extras[f"assets/assets/src/{fname}"] = data

KEEP_VERIFY = ["assets/main.js", "resources.arsc", "classes.dex", "classes2.dex",
               "classes3.dex", "classes4.dex", "classes5.dex", "classes6.dex",
               "lib/arm64-v8a/libcocos2djs.so"]
zin = zipfile.ZipFile(base, "r")
names = set(zin.namelist())
for n in KEEP_VERIFY:
    if n not in names:
        raise SystemExit(f"ERROR: base missing {n}")
keep_sha = {n: hashlib.sha256(zin.read(n)).hexdigest() for n in KEEP_VERIFY}
zout = zipfile.ZipFile(dst, "w")
kept = dropped = replaced = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        dropped += 1
        continue
    if name in extras:
        replaced += 1
        continue
    data = zin.read(name)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data); kept += 1
for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name])
zin.close(); zout.close()

zin = zipfile.ZipFile(base); zout = zipfile.ZipFile(dst)
out_names = set(zout.namelist())
for n, h in keep_sha.items():
    if hashlib.sha256(zout.read(n)).hexdigest() != h:
        raise SystemExit(f"ERROR: kept entry drifted: {n}")
if zout.read("classes7.dex") == zin.read("classes7.dex"):
    raise SystemExit("ERROR: classes7 未更新")
if zout.read("classes8.dex") != (menu / "classes8.dex").read_bytes():
    raise SystemExit("ERROR: classes8 未就位")
if zout.read(plugin) != (work / "plugin.patched.apk").read_bytes():
    raise SystemExit("ERROR: plugin 未替换")
if zout.read("lib/arm64-v8a/libstarry_magic.so") != (menu / "libstarry_magic.so").read_bytes():
    raise SystemExit("ERROR: so 未就位")
c7 = zout.read("classes7.dex")
for probe in (b"com/starry/magic/menu/MenuService", b"com/starry/magic/cocos/ZxrHooks", b"starry_magic"):
    if probe not in c7:
        raise SystemExit(f"ERROR: classes7 缺引用 {probe!r}")
eng = zout.read("assets/jsb-adapter/jsb-engine.js")
if b"var packagePath = getPackagePathFromRemoteUrl(url);" not in eng:
    raise SystemExit("ERROR: 引擎包内命中优先缺失")
for ver in pins:
    for tail in (f"index.{ver}.jsc", f"config.{ver}.json"):
        if f"assets/assets/src/{tail}" not in out_names:
            raise SystemExit(f"ERROR: pin 缺 {tail}")
print(f"  merge ok kept={kept} replaced={replaced} dropped_meta_inf={dropped} "
      f"classes7={len(c7)}B classes8={len(zout.read('classes8.dex'))}B pins={','.join(pins)}")
zin.close(); zout.close()
PY

echo "[7/8] zipalign + 签名"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "Signer #1 certificate (SHA-256|MD5) digest"

echo "[8/8] 后检"
"$PY" - "$SIGNED" "$BASE_APK" "$ORIG_CERT_MD5" <<'PY'
import hashlib, sys, zipfile
apk, base, md5 = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk); b = zipfile.ZipFile(base)
n = z.namelist()
mf = z.read("AndroidManifest.xml")
if b"com.starry.magic.menu.MenuService".decode().encode("utf-16-le") not in mf and b"com.starry.magic.menu.MenuService" not in mf:
    raise SystemExit("ERROR: 最终包 Manifest 缺 MenuService")
plugin = [x for x in n if x.startswith("assets/sq_plugin_") and x.endswith(".apk")][0]
pz = zipfile.ZipFile(__import__("io").BytesIO(z.read(plugin)))
pdex = pz.read("classes4.dex")
if md5.encode() not in pdex:
    raise SystemExit("ERROR: 最终包 plugin 未打签名固定")
assert z.read("assets/main.js") == b.read("assets/main.js"), "main.js drifted"
print("  final size=%dB sha256=%s" % (len(open(apk, "rb").read()), hashlib.sha256(open(apk, "rb").read()).hexdigest()))
print("  classes8 sha256=", hashlib.sha256(z.read("classes8.dex")).hexdigest())
print("  so sha256=", hashlib.sha256(z.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest())
print("  plugin sha256=", hashlib.sha256(z.read(plugin)).hexdigest())
PY
ls -lh "$SIGNED"
echo "done → ${SIGNED}"
