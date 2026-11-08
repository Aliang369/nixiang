#!/usr/bin/env bash
# 境界刀鸣 国际服 Crunchyroll 渠道 1.50.212144（com.crunchyroll.bleachsoulres，vc50）—— 星光统一单包
#
# 与 GP 版（com.bladetw.bd 1.48）的差异：
#   * 无 libhhld-rt.so（无 kill-switch）→ 不需要 NOP，零注入重签实测可直接启动
#   * 无 com.pairip.*（PairIP）→ 无 licensecheck
#   * 无字节 GSDK/ttnet（19 个 so vs GP 70 个）
#   * 资源直接打在 split_installtime（1.47 GB，486/491 条目 STORED）→ 装完即玩，无首次热更
#   * libgame.so 与国服/GP 版 md5 完全相同（9b436aee01fdeab9b7a916e430fe8fa1）→ JjdmMod 零改动
# 合并要点同 GP 版：删 isSplitRequired / requiredSplitTypes，把 config 的 so 与 installtime 的 assets 并进 base。
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_jjdmcr_universal.sh
#   STARRY_INSTALL=1 SERIAL=96f0d883 bash ...
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$STARRY_SRC/../.." && pwd)}"
VER="1.50.212144"
PKG="com.crunchyroll.bleachsoulres"
TAG="Jjdm"
OUT="$NIXIANG/08_output/境界刀鸣国际CR"
SRC="$NIXIANG/02_apks/境界刀鸣国际CR"
BASE_SRC="$SRC/境界刀鸣国际CR_${VER}_official_from_device_base.apk"
CFG_SRC="$SRC/境界刀鸣国际CR_${VER}_official_from_device_split_config.arm64_v8a.apk"
IT_SRC="$SRC/境界刀鸣国际CR_${VER}_official_from_device_split_installtime.apk"
STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
SIGNED="$OUT/境界刀鸣国际CR_${VER}_三功能_jjdmintl_universal-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
INJECT_DEX="classes6.dex"
MF_CACHE="$NIXIANG/07_patched/境界刀鸣国际CR/surgical_${VER}/AndroidManifest_universal.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
for f in "$BASE_SRC" "$CFG_SRC" "$IT_SRC" "$STARRY_APK" "$SMALI_BIN" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f"; exit 1; }
done
[ -n "$BT" ] || { echo "ERROR: 找不到 build-tools"; exit 1; }

WORK="$(mktemp -d /tmp/jjdmcr_univ.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$(dirname "$MF_CACHE")"

echo "[1/5] starry → smali → $INJECT_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null 2>&1
SMALI_ROOT="$WORK/smali_in"; mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [ -d "$d/com/starry" ] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
"$SMALI_BIN" assemble -o "$WORK/$INJECT_DEX" "$SMALI_ROOT"
echo "  dex $(stat -f%z "$WORK/$INJECT_DEX") B"

echo "[2/5] Manifest（删 split 属性 + 注 provider/service）"
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
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  /opt/homebrew/bin/python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
import sys, re
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
assert 'com.starry.magic.StarryInitProvider' not in t
m = re.search(r'<application[^>]*', t, re.S)
print("  application 标签:", (m.group(0)[:200].replace("\n", " ") if m else "?"))
t = re.sub(r'\s+android:isSplitRequired="[^"]*"', '', t)
t = re.sub(r'\s+android:requiredSplitTypes="[^"]*"', '', t)
assert 'isSplitRequired' not in t and 'requiredSplitTypes' not in t, "split 属性未删净"
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
print("  manifest 改好：删 2 个 split 属性，加权限 %d 条" % len(perms))
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null 2>&1 || { echo "ERROR: apktool b 失败"; exit 1; }
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
fi

echo "[3/5] so 准备"
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/libstarry_magic.so"
LC_ALL=C grep -aq "\[$TAG\]" "$WORK/libstarry_magic.so" || { echo "ERROR: so 无 [$TAG]"; exit 1; }
LC_ALL=C grep -aq "jjdmintl" "$WORK/libstarry_magic.so" || { echo "ERROR: so 缺 jjdmintl"; exit 1; }
CXXSH="${STARRY_CXX_SHARED:-$STARRY_SRC/libs_jjdmintl/arm64-v8a/libc++_shared.so}"
[ -f "$CXXSH" ] || { echo "ERROR: 缺 libc++_shared.so（CR 版包内不自带，必须随包注入）"; exit 1; }
cp -f "$CXXSH" "$WORK/libc++_shared.so"
mkdir -p "$WORK/assets"
if [ -d "$WORK/starry_dec/assets/certs" ]; then cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"; fi
echo "  so $(md5 -q "$WORK/libstarry_magic.so") $(stat -f%z "$WORK/libstarry_magic.so") B"
echo "  cxx $(md5 -q "$WORK/libc++_shared.so") $(stat -f%z "$WORK/libc++_shared.so") B（CR 版不自带，随包注入）"

echo "[4/5] zip 合并（流式，installtime 1.47 GB）"
/opt/homebrew/bin/python3.11 - "$BASE_SRC" "$CFG_SRC" "$IT_SRC" "$WORK" "$WORK/unsigned.apk" "$INJECT_DEX" <<'PY'
import sys, zipfile, hashlib, shutil
from pathlib import Path
base, cfg, it, work, dst, inject = (Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]),
                                    Path(sys.argv[4]), Path(sys.argv[5]), sys.argv[6])
mf  = (work / "AndroidManifest.xml").read_bytes()
dex = (work / inject).read_bytes()
starso = (work / "libstarry_magic.so").read_bytes()
cxx = (work / "libc++_shared.so").read_bytes()

zc = zipfile.ZipFile(cfg)
zi = zipfile.ZipFile(it)
plan = []   # (zinfo, data|None, src_zip|None, name)
n_cfg = n_it = 0
for n in zc.namelist():
    if not n.startswith("lib/") or n.endswith("/"): continue
    plan.append((zc.getinfo(n), zc, n)); n_cfg += 1
for n in zi.namelist():
    if not n.startswith("assets/") or n.endswith("/"): continue
    plan.append((zi.getinfo(n), zi, n)); n_it += 1
# starry certs
certs = work / "assets" / "certs"
extra_certs = []
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extra_certs.append(("assets/certs/" + str(p.relative_to(certs)).replace("\\", "/"), p.read_bytes()))

zb = zipfile.ZipFile(base)
owned = set(n for _, _, n in plan)
clash = sorted(set(n for n in zb.namelist() if n.startswith("lib/")) & owned)
print("  split_config so=%d  installtime assets=%d  base 自带 lib=%s" % (n_cfg, n_it, clash or '无'))
assert not clash, "base 与 split 的 lib 冲突"

zout = zipfile.ZipFile(dst, "w", allowZip64=True)
for item in zb.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in owned or name == inject:
        continue
    data = mf if name == "AndroidManifest.xml" else zb.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
info = zipfile.ZipInfo(inject, (2026, 11, 8, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3; zout.writestr(info, dex)
info = zipfile.ZipInfo("lib/arm64-v8a/libstarry_magic.so", (2026, 11, 8, 0, 0, 0)); info.compress_type = zipfile.ZIP_STORED
info.create_system = 3; zout.writestr(info, starso)
info = zipfile.ZipInfo("lib/arm64-v8a/libc++_shared.so", (2026, 11, 8, 0, 0, 0)); info.compress_type = zipfile.ZIP_STORED
info.create_system = 3; zout.writestr(info, cxx)
for name, data in extra_certs:
    info = zipfile.ZipInfo(name, (2026, 11, 8, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3; zout.writestr(info, data)
# 流式搬运大条目
moved = 0
for src_info, sz, name in plan:
    info = zipfile.ZipInfo(name, src_info.date_time)
    info.compress_type = src_info.compress_type
    info.external_attr = src_info.external_attr
    info.create_system = 3
    info.file_size = src_info.file_size
    with sz.open(name) as fsrc, zout.open(info, "w") as fdst:
        shutil.copyfileobj(fsrc, fdst, 1024 * 1024)
    moved += 1
zout.close()
print("  流式搬运 %d 条" % moved)

a = zipfile.ZipFile(base); b = zipfile.ZipFile(dst)
assert hashlib.sha256(a.read("resources.arsc")).hexdigest() == hashlib.sha256(b.read("resources.arsc")).hexdigest(), "arsc 被改"
inj = owned | {inject, "AndroidManifest.xml"} | set(n for n, _ in extra_certs)
n_ok = n = 0
for item in a.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj:
        continue
    n += 1
    if hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest():
        n_ok += 1
assert n_ok == n, "base 官方条目 hash 不一致 %d/%d" % (n_ok, n)
names = set(b.namelist())
for must in (inject, "lib/arm64-v8a/libstarry_magic.so", "lib/arm64-v8a/libc++_shared.so", "lib/arm64-v8a/libgame.so",
             "lib/arm64-v8a/libil2cpp.so", "assets/main.xpk", "assets/Md5List.txt"):
    assert must in names, "缺 %s" % must
print("  base 透传 %d/%d ; 条目总数 %d ; main.xpk %d B" % (n_ok, n, len(names), b.getinfo("assets/main.xpk").file_size))
a.close(); b.close()
PY

echo "[5/5] zipalign + sign（1.5 GB，耐心）"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED" 2>&1 | grep -v WARNING || true
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | grep -E "certificate DN|certificate SHA-256" | head -2
ls -lh "$SIGNED"; shasum -a 256 "$SIGNED"; md5 -q "$SIGNED"
"$BT/aapt2" dump badging "$SIGNED" 2>/dev/null | head -2

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  ADB="${ADB:-/opt/homebrew/bin/adb}"; SERIAL_ARG=""
  [ -n "${SERIAL:-}" ] && SERIAL_ARG="-s $SERIAL"
  "$ADB" $SERIAL_ARG install -r -d "$SIGNED"
fi
echo "[done] $SIGNED"
