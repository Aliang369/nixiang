#!/usr/bin/env bash
# 境界刀鸣 国际服 1.48.211534（com.bladetw.bd，GooglePlay 渠道，vc48，split 三件套）
# 星光直装包外科组装：classes11.dex + libstarry_magic.so + manifest(provider/service)，不替换 Application
#
# 与国服 com.blade.bd 1.39 同源依据：
#   libgame.so 的 lua_gettop=0x515e38 / lua_pcallk=0x518fec / luaL_loadstring=0x51bf04 逐一相同（Lua 5.4.6）
#   libhhld-rt.so kill-switch 同样崩 fault addr 0x18a44；本版 NOP 点 0x793b8（国服 0x5c270）
#
# 差异处理：
#   * split 结构：base / split_config.arm64_v8a / split_installtime 三件套，install-multiple 安装
#   * Application = com.pairip.application.Application（Google Play PairIP 壳）→ 保持原样，只加 provider
#   * 官方证书 CN=ssgame O=blade；本包 CN=StarryMagic
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_jjdmintl_1.48.211534.sh
#   STARRY_INSTALL=1 SERIAL=96f0d883 bash ...   # 装完覆盖（同签名 -r，保留数据）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$STARRY_SRC/../.." && pwd)}"
VER="1.48.211534"
PKG="com.bladetw.bd"
TAG="Jjdm"
OUT="$NIXIANG/08_output/境界刀鸣国际"
SRC="$NIXIANG/02_apks/境界刀鸣国际"
BASE_SRC="$SRC/境界刀鸣国际_${VER}_official_from_device_base.apk"
CFG_PATCHED="$OUT/split/境界刀鸣国际_${VER}_split_config.arm64_v8a_hhldnop.apk"
IT_SIGNED="$OUT/split/境界刀鸣国际_${VER}_split_installtime.apk"
STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
SIGNED="$OUT/境界刀鸣国际_${VER}_三功能_jjdm-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
MF_CACHE="$NIXIANG/07_patched/境界刀鸣国际/surgical_${VER}/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done

for f in "$BASE_SRC" "$CFG_PATCHED" "$IT_SIGNED" "$STARRY_APK" "$SMALI_BIN" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f"; exit 1; }
done
[ -n "$BT" ] || { echo "ERROR: 找不到 build-tools"; exit 1; }

WORK="$(mktemp -d /tmp/jjdmtw_surgical.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$(dirname "$MF_CACHE")"

echo "[1/6] starry APK → smali → 注入 dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null 2>&1
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [ -d "$d/com/starry" ] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ] || { echo "ERROR: StarryInitProvider.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes11.dex" "$SMALI_ROOT"
ls -l "$WORK/classes11.dex"

echo "[2/6] 官方 base 结构探测"
/opt/homebrew/bin/python3.11 - "$BASE_SRC" <<'PY'
import sys, re, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = z.namelist()
nums = [int(m.group(1) or 1) for m in (re.fullmatch(r'classes(?:(\d+))?\.dex', n) for n in names) if m]
libs = [n for n in names if n.startswith('lib/')]
print('  dex max=%d → 注入 classes%d.dex ; 官方 lib 条目=%d' % (max(nums), max(nums)+1, len(libs)))
PY

echo "[3/6] 生成二进制 Manifest（provider + service + 权限；不替换 Application）"
if [ -f "$MF_CACHE" ]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"; echo "  cached"
else
  /opt/homebrew/bin/python3.11 - "$BASE_SRC" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in ("AndroidManifest.xml", "resources.arsc", "classes.dex"):
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print("  mini apk", dst.stat().st_size, "B")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  /opt/homebrew/bin/python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
import sys
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
assert 'com.pairip.application.Application' in t, "官方 Application 不是 PairIP，需人工确认"
assert 'com.starry.magic.StarryInitProvider' not in t
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
if perms:
    i = t.find("<uses-permission")
    if i < 0: i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:exported="false" />
''' % pkg
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
print("  manifest 文本已改：provider+service+权限 %d 条" % len(perms))
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null 2>&1 || { echo "ERROR: apktool b 失败"; exit 1; }
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -l "$WORK/AndroidManifest.xml"

echo "[4/6] so 准备"
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/libstarry_magic.so"
LC_ALL=C grep -aq "\[$TAG\]" "$WORK/libstarry_magic.so" || { echo "ERROR: so 无 [$TAG]"; exit 1; }
for m in 伤害倍率 无敌 无限技能; do
  N=$(LC_ALL=C grep -ac "$m" "$WORK/libstarry_magic.so" || true)
  [ "${N:-0}" -ge 1 ] || { echo "ERROR: so 缺菜单串 $m"; exit 1; }
done
mkdir -p "$WORK/assets"
if [ -d "$WORK/starry_dec/assets/certs" ]; then cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"; fi
echo "  so ok $(md5 -q "$WORK/libstarry_magic.so") $(stat -f%z "$WORK/libstarry_magic.so") B"

echo "[5/6] zip 外科合并 → 未签名 base"
/opt/homebrew/bin/python3.11 - "$BASE_SRC" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
dex = (work / "classes11.dex").read_bytes()
so  = (work / "libstarry_magic.so").read_bytes()
extras = {"lib/arm64-v8a/libstarry_magic.so": so}
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"): continue
    if name in extras or name == "classes11.dex": continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
for name, data in [(("classes11.dex"), dex)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 11, 7, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
# 校验
a = zipfile.ZipFile(src); b = zipfile.ZipFile(dst)
assert hashlib.sha256(a.read("resources.arsc")).hexdigest() == hashlib.sha256(b.read("resources.arsc")).hexdigest(), "arsc 被改"
inj = set(extras) | {"classes11.dex", "AndroidManifest.xml"}
n_ok = n = 0
for item in a.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj: continue
    n += 1
    if hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest(): n_ok += 1
print("  透传 %d/%d ; 注入 %s" % (n_ok, n, sorted(inj)))
assert n_ok == n, "官方条目 hash 不一致"
a.close(); b.close()
PY

echo "[6/6] zipalign + sign"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS"   --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED" 2>&1 | grep -v WARNING || true
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | grep -E "certificate DN|certificate SHA-256" | head -2
ls -lh "$SIGNED"; shasum -a 256 "$SIGNED"
/opt/homebrew/bin/python3.11 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
for n in ("classes11.dex", "lib/arm64-v8a/libstarry_magic.so"):
    print("  含", n, len(z.read(n)), "B")
print("  hhld 不在 base（在 split_config）:", "lib/arm64-v8a/libhhld-rt.so" not in z.namelist())
PY

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  ADB="${ADB:-/opt/homebrew/bin/adb}"
  SERIAL_ARG=""
  [ -n "${SERIAL:-}" ] && SERIAL_ARG="-s $SERIAL"
  echo "[install] install-multiple -r"
  "$ADB" $SERIAL_ARG install-multiple -r -d "$SIGNED" "$CFG_PATCHED" "$IT_SIGNED"
  echo "log: adb logcat -s StarryInitProvider:V StarryMagic:V MenuService:V Jjdm:V"
fi
echo "[done] $SIGNED"
