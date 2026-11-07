#!/usr/bin/env bash
# 境界刀鸣 国际服 1.48.211534 —— 统一单包（universal APK）：把 split 三件套合并成一个可直装的 APK
#
# 合并内容：
#   base.apk（官方）                    → 骨架（dex / resources.arsc / assets/bin 等原样透传）
#   + classes11.dex                     → 星光 smali
#   + split_config.arm64_v8a 的 lib/*   → 70 个 arm64 so（含已 NOP 的 libhhld-rt.so）
#   + split_installtime 的 assets/*     → main.xpk / main/ / Md5List.txt
#   + lib/arm64-v8a/libstarry_magic.so  → Jjdm 三功能
#   + Manifest                          → 删 isSplitRequired / requiredSplitTypes，加 provider+service+权限
#
# 为什么要删属性：base 原带 android:isSplitRequired="true" 与 android:requiredSplitTypes="base__abi"，
#   不删则单装必被系统拒（INSTALL_FAILED_INVALID_APK / missing split）。
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_jjdmintl_universal.sh
#   STARRY_INSTALL=1 SERIAL=96f0d883 bash ...      # 装（从 split 结构切单包通常需卸载，脚本不自动卸）
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
IT_SRC="$OUT/split/境界刀鸣国际_${VER}_split_installtime.apk"
STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
SIGNED="$OUT/境界刀鸣国际_${VER}_三功能_jjdmintl_universal-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
MF_CACHE="$NIXIANG/07_patched/境界刀鸣国际/surgical_${VER}/AndroidManifest_universal.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
for f in "$BASE_SRC" "$CFG_PATCHED" "$IT_SRC" "$STARRY_APK" "$SMALI_BIN" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f"; exit 1; }
done
[ -n "$BT" ] || { echo "ERROR: 找不到 build-tools"; exit 1; }

WORK="$(mktemp -d /tmp/jjdmtw_univ.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$(dirname "$MF_CACHE")"

echo "[1/6] starry → smali → classes11.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null 2>&1
SMALI_ROOT="$WORK/smali_in"; mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [ -d "$d/com/starry" ] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
"$SMALI_BIN" assemble -o "$WORK/classes11.dex" "$SMALI_ROOT"
echo "  dex $(stat -f%z "$WORK/classes11.dex") B"

echo "[2/6] Manifest（删 isSplitRequired / requiredSplitTypes + 注 provider/service）"
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
assert 'com.pairip.application.Application' in t, "官方 Application 不是 PairIP，需人工确认"
before = len(t)
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
print("  manifest 改好：删 2 个 split 属性，加权限 %d 条（%d→%d 字节）" % (len(perms), before, len(t)))
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null 2>&1 || { echo "ERROR: apktool b 失败"; exit 1; }
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
fi
ls -l "$WORK/AndroidManifest.xml"

echo "[3/6] so 准备（星光 + split_config 全部 so）"
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/libstarry_magic.so"
LC_ALL=C grep -aq "\[$TAG\]" "$WORK/libstarry_magic.so" || { echo "ERROR: so 无 [$TAG]"; exit 1; }
LC_ALL=C grep -aq "jjdmintl" "$WORK/libstarry_magic.so" || { echo "ERROR: so 缺 jjdmintl（Entry 未切）"; exit 1; }
mkdir -p "$WORK/assets"
if [ -d "$WORK/starry_dec/assets/certs" ]; then cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"; fi
echo "  starry so $(md5 -q "$WORK/libstarry_magic.so") $(stat -f%z "$WORK/libstarry_magic.so") B"

echo "[4/6] zip 外科合并（base 骨架 + config 的 lib + installtime 的 assets）"
/opt/homebrew/bin/python3.11 - "$BASE_SRC" "$CFG_PATCHED" "$IT_SRC" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
base, cfg, it, work, dst = (Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]),
                            Path(sys.argv[4]), Path(sys.argv[5]))
mf  = (work / "AndroidManifest.xml").read_bytes()
dex = (work / "classes11.dex").read_bytes()
starso = (work / "libstarry_magic.so").read_bytes()

extras = {"classes11.dex": (dex, zipfile.ZIP_DEFLATED),
          "lib/arm64-v8a/libstarry_magic.so": (starso, zipfile.ZIP_STORED)}
# split_config: lib/**.so
zc = zipfile.ZipFile(cfg)
n_cfg = 0
for n in zc.namelist():
    if not n.startswith("lib/") or n.endswith("/"):
        continue
    extras[n] = (zc.read(n), zc.getinfo(n).compress_type)
    n_cfg += 1
# split_installtime: assets/**
zi = zipfile.ZipFile(it)
n_it = 0
for n in zi.namelist():
    if not n.startswith("assets/") or n.endswith("/"):
        continue
    extras[n] = (zi.read(n), zi.getinfo(n).compress_type)
    n_it += 1
# starry certs
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = (p.read_bytes(), zipfile.ZIP_DEFLATED)

zb = zipfile.ZipFile(base)
take = set([n for n in zb.namelist() if n.startswith("lib/")])
clash = sorted(take & set(extras))
print("  split_config so=%d  installtime assets=%d  base 自身 lib=%d  冲突=%s" % (n_cfg, n_it, len(take), clash or '无'))
assert not clash, "base 与 split 的 lib 条目冲突，需人工定夺"

zout = zipfile.ZipFile(dst, "w")
for item in zb.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in extras:
        continue
    data = mf if name == "AndroidManifest.xml" else zb.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
for name, (data, ctype) in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 11, 7, 0, 0, 0))
    info.compress_type = ctype
    info.create_system = 3
    zout.writestr(info, data)
zout.close()

# 校验：arsc 未变 + base 官方条目逐条 hash + 关键件齐
a = zipfile.ZipFile(base); b = zipfile.ZipFile(dst)
assert hashlib.sha256(a.read("resources.arsc")).hexdigest() == hashlib.sha256(b.read("resources.arsc")).hexdigest(), "arsc 被改"
inj = set(extras) | {"AndroidManifest.xml"}
n_ok = n = 0
for item in a.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj:
        continue
    n += 1
    if hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest():
        n_ok += 1
assert n_ok == n, "base 官方条目 hash 不一致 %d/%d" % (n_ok, n)
hh = b.read("lib/arm64-v8a/libhhld-rt.so")
assert hh[0x793b8:0x793bc] == bytes.fromhex("1f2003d5"), "hhld NOP 未生效"
names = set(b.namelist())
for must in ("classes11.dex", "lib/arm64-v8a/libstarry_magic.so", "assets/main.xpk", "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libgame.so"):
    assert must in names, "缺 %s" % must
print("  base 官方条目透传 %d/%d；hhld NOP OK；条目总数 %d" % (n_ok, n, len(names)))
a.close(); b.close()
PY

echo "[5/6] zipalign + sign"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED" 2>&1 | grep -v WARNING || true
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | grep -E "certificate DN|certificate SHA-256" | head -2

echo "[6/6] 结果"
ls -lh "$SIGNED"; shasum -a 256 "$SIGNED"; md5 -q "$SIGNED"
"$BT/aapt2" dump badging "$SIGNED" 2>/dev/null | head -3

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  ADB="${ADB:-/opt/homebrew/bin/adb}"; SERIAL_ARG=""
  [ -n "${SERIAL:-}" ] && SERIAL_ARG="-s $SERIAL"
  "$ADB" $SERIAL_ARG install -r -d "$SIGNED"
fi
echo "[done] $SIGNED"
