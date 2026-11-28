#!/usr/bin/env bash
# 幻想九霄 1.0.212（TapTap 官包）— 外科组装：官方 dex/arsc/libil2cpp/libunity 字节级保留，只增不改
#
# 产物：08_output/幻想九霄/幻想九霄_1.0.212_surgical-releaseSigned.apk
#
# 与 surgical_awlzw_24.7.21.sh 的差异（本包结构差异，勿照抄 awlzw 参数）：
#   - 底包 2 dex（classes.dex + classes2.dex）；TapTap 改包补丁落在 classes2.dex
#     ⇒ classes.dex 原样保留，classes2.dex 换成补丁版，星光菜单 dex 落 classes3.dex
#   - <application> 无 android:name（有 appComponentFactory=androidx.core.app.CoreComponentFactory）
#     ⇒ 入口用 ContentProvider（StarryInitProvider 插在 <application> 第一个子节点，
#        必须排在 TapTap 的 TapUpdateFileProvider / OkDownloadProvider 之前），不换 Application
#   - 菜单 dex 来源：gradle assembleRelease --offline（取含 StarryInitProvider 的 dex）；
#     不可用时回落 templates/ 下最新且含 StarryInitProvider 的菜单 dex（--no-gradle 强制回落）
#   - lib/arm64-v8a/libc++_shared.so 底包没有、so 的 DT_NEEDED 里有 ⇒ 从 NDK 27.0.12077973 拷
#
# 用法:
#   bash 04_scripts/幻想九霄/surgical_hxjx_1.0.212.sh              # 出包 + 全部自检
#   bash 04_scripts/幻想九霄/surgical_hxjx_1.0.212.sh --no-gradle   # 菜单 dex 走 templates 回落
#   bash 04_scripts/幻想九霄/surgical_hxjx_1.0.212.sh --no-preflight
#   bash 04_scripts/幻想九霄/surgical_hxjx_1.0.212.sh --audit-waiver-modern   # legacy-only 两项 ✗ 记录豁免理由后继续
# 不装机：本脚本不含任何 adb 调用。
set -euo pipefail

USE_GRADLE=1
RUN_PREFLIGHT=1
AUDIT_WAIVER=0
for a in "$@"; do
  case "$a" in
    --no-gradle)    USE_GRADLE=0 ;;
    --no-preflight) RUN_PREFLIGHT=0 ;;
    --audit-waiver-modern) AUDIT_WAIVER=1 ;;
    -h|--help)      sed -n '2,21p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

GAME="幻想九霄"
PKG="com.hundunjidian.hxjx"
NIXIANG="${NIXIANG_ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"

ORIG_APK="$NIXIANG/02_apks/幻想九霄/原版/幻想九霄_1.0.212_installed_1809.apk"
DEX2_PATCHED="$NIXIANG/03_decompiled/幻想九霄/taptap/dex_out/classes2.dex"
SO_SRC="$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

ORIG_SHA_PREFIX="ff774c6e"
ORIG_SIZE=301199144
SO_SHA_PREFIX="ee491b7b"   # 2026-11-28 清空实验代码回归基线：只留 IL 字节补丁 A(0x48995)+Ap(0x1971A0)，AOT/interp/pulse 全删

OUT="$NIXIANG/08_output/幻想九霄"
SIGNED="$OUT/幻想九霄_1.0.212_surgical-releaseSigned.apk"
TMP_UNSIGNED="$OUT/.tmp_hxjx_unsigned.apk"
TMP_ALIGNED="$OUT/.tmp_hxjx_aligned.apk"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
NDK="${ANDROID_NDK_HOME:-/Users/aliang/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"

export PATH="/opt/homebrew/bin:/usr/local/bin:$PATH"
[[ -n "${HOME:-}" ]] || export HOME="$(eval echo ~"$(id -un)")"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "/Users/aliang/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="/Users/aliang/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing" >&2; exit 1; }
AAPT2="$BT/aapt2"
[[ -x "$AAPT2" ]] || { echo "ERROR: missing aapt2 in $BT" >&2; exit 1; }

PY_BIN="$(command -v python3.11 || command -v python3 || true)"
[[ -n "$PY_BIN" ]] || { echo "ERROR: python3/python3.11 missing" >&2; exit 1; }
"$PY_BIN" -c 'import zipfile,hashlib' || { echo "ERROR: $PY_BIN 缺 zipfile/hashlib" >&2; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool missing" >&2; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -f "$DEX2_PATCHED" ]] || { echo "ERROR: missing $DEX2_PATCHED" >&2; exit 1; }
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing $SO_SRC" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

md5of() { openssl md5 -r "$1" | awk '{print $1}'; }
shaof() { shasum -a 256 "$1" | awk '{print $1}'; }

echo "== [0/9] 输入指纹校验"
[[ "$(stat -f%z "$ORIG_APK")" == "$ORIG_SIZE" ]] || { echo "ERROR: 底包 size 不符" >&2; exit 1; }
ORIG_SHA="$(shaof "$ORIG_APK")"
[[ "$ORIG_SHA" == "$ORIG_SHA_PREFIX"* ]] || { echo "ERROR: 底包 sha256 前缀不符 $ORIG_SHA" >&2; exit 1; }
SO_SHA="$(shaof "$SO_SRC")"; SO_MD5="$(md5of "$SO_SRC")"
[[ "$SO_SHA" == "$SO_SHA_PREFIX"* ]] || { echo "ERROR: so sha256 前缀不符 $SO_SHA" >&2; exit 1; }
LC_ALL=C grep -aq '\[Hxjx\]' "$SO_SRC" || { echo "ERROR: so 无 [Hxjx] 标签" >&2; exit 1; }
LC_ALL=C grep -aq 'hxjx=' "$SO_SRC" || { echo "ERROR: so 无 hxjx= game_id" >&2; exit 1; }
DEX2_MD5="$(md5of "$DEX2_PATCHED")"
echo "  orig sha256=$ORIG_SHA ($ORIG_SIZE B)"
echo "  dex2 md5=$DEX2_MD5 ($(stat -f%z "$DEX2_PATCHED") B)"
echo "  so   sha256=$SO_SHA md5=$SO_MD5 ($(stat -f%z "$SO_SRC") B)"

AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 4 ]] || { echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 4G" >&2; exit 1; }

LOCK_DIR="/tmp/hxjx_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/hxjx_surgical.XXXXXX)"
cleanup() { rm -rf "$WORK" 2>/dev/null || true; rm -rf "$LOCK_DIR" 2>/dev/null || true; }
trap cleanup EXIT
mkdir -p "$OUT" "$WORK"

find_menu_dex_in_dir() {
  local d="$1" f
  for f in "$d"/classes*.dex; do
    [[ -f "$f" ]] || continue
    if LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$f" \
       && LC_ALL=C grep -aq 'com/starry/magic/menu/MenuService' "$f"; then
      echo "$f"; return 0
    fi
  done
  return 1
}

MENU_SOURCE=""
MENU_DEX=""
echo "== [1/9] 星光菜单 dex（新鲜度铁律：断言 StarryInitProvider + MenuService）"
if [[ "$USE_GRADLE" == "1" && -x "$STARRY_SRC/gradlew" ]]; then
  echo "  gradle assembleRelease --offline"
  MENU_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
  rm -f "$MENU_APK"
  T0="$(date +%s)"
  ( cd "$STARRY_SRC" && ./gradlew assembleRelease --offline --console=plain -q )
  [[ -f "$MENU_APK" ]] || { echo "ERROR: gradle 未产出 $MENU_APK" >&2; exit 1; }
  [[ "$(stat -f%m "$MENU_APK")" -ge "$T0" ]] || { echo "ERROR: app-release.apk 不是本次构建产物（新鲜度铁律）" >&2; exit 1; }
  mkdir -p "$WORK/menu_apk"
  unzip -o -q "$MENU_APK" 'classes*.dex' -d "$WORK/menu_apk"
  MENU_DEX="$(find_menu_dex_in_dir "$WORK/menu_apk" || true)"
  [[ -n "$MENU_DEX" ]] || { echo "ERROR: gradle APK 内无含 StarryInitProvider+MenuService 的 dex" >&2; exit 1; }
  MENU_SOURCE="gradle assembleRelease --offline（$(date -r "$(stat -f%m "$MENU_APK")" '+%Y-%m-%d %H:%M:%S') 新构建）: app-release.apk!/$(basename "$MENU_DEX")"
else
  echo "  --no-gradle：回落 templates/ 最新菜单 dex"
  CAND=""
  for f in $(ls -t "$STARRY_SRC"/templates/*/classes*.dex 2>/dev/null); do
    if LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$f" \
       && LC_ALL=C grep -aq 'com/starry/magic/menu/MenuService' "$f"; then CAND="$f"; break; fi
  done
  [[ -n "$CAND" ]] || { echo "ERROR: templates/ 下无含 StarryInitProvider+MenuService 的 dex" >&2; exit 1; }
  MENU_DEX="$WORK/classes3.dex"; cp -f "$CAND" "$MENU_DEX"
  MENU_SOURCE="templates 回落: $CAND"
fi
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$MENU_DEX" \
  || { echo "ERROR: 菜单 dex 缺 StarryInitProvider" >&2; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/menu/MenuService' "$MENU_DEX" \
  || { echo "ERROR: 菜单 dex 缺 MenuService" >&2; exit 1; }
MENU_MD5="$(md5of "$MENU_DEX")"
MENU_SHA="$(shaof "$MENU_DEX")"
echo "  来源 : $MENU_SOURCE"
echo "  md5  : $MENU_MD5"
echo "  sha256: $MENU_SHA  size=$(stat -f%z "$MENU_DEX") B"
echo "  → classes3.dex"

echo "== [2/9] mini apk（剔 META-INF/assets/lib/classes*.dex）→ apktool 往返只取 AndroidManifest.xml"
"$PY_BIN" - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w"); kept = 0
for item in zin.infolist():
    n = item.filename
    if n.startswith(("META-INF/", "assets/", "lib/")):
        continue
    if n.startswith("classes") and n.endswith(".dex"):
        continue
    info = zipfile.ZipInfo(item.orig_filename, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, zin.read(item)); kept += 1
zout.close(); zin.close()
print("  mini apk entries=%d" % kept)
PY
rm -rf "$WORK/orig_dec"
apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null

"$PY_BIN" - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
import re, sys
from pathlib import Path
p = Path(sys.argv[1]); t = p.read_text(encoding="utf-8")
PROV = "com.starry.magic.StarryInitProvider"
AUTH = "com.hundunjidian.hxjx.starryinit"
SVC  = "com.starry.magic.menu.MenuService"

m = re.search(r"<application\b[^>]*>", t)
if not m:
    raise SystemExit("ERROR: 找不到 <application>")
app_tag = m.group(0)
if "android:name=" in app_tag:
    raise SystemExit("ERROR: <application> 已带 android:name（版本假设失效，本包应无 name）")
if "appComponentFactory" not in app_tag:
    print("  WARN: <application> 无 appComponentFactory")

if PROV in t:
    print("  manifest: StarryInitProvider 已存在（幂等跳过）")
else:
    blk = ('\n        <provider\n'
           '            android:name="%s"\n'
           '            android:authorities="%s"\n'
           '            android:exported="false" />' % (PROV, AUTH))
    t = t[:m.end()] + blk + t[m.end():]
    print("  manifest: + StarryInitProvider（<application> 第一个子节点）")

added = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if "android.permission." + perm not in t:
        added.append('    <uses-permission android:name="android.permission.%s"/>' % perm)
if added:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("ERROR: no <uses-permission> anchor")
    t = t[:i] + "\n".join(added) + "\n    " + t[i:].lstrip()
    print("  manifest: + %d 条权限（SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE）" % len(added))

svc = '''
        <service
            android:name="%s"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
''' % SVC
if SVC not in t:
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("  manifest: + MenuService（specialUse FGS property）")

p.write_text(t, encoding="utf-8")
t2 = p.read_text(encoding="utf-8")
for need in (PROV, AUTH, SVC, "SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE_SPECIAL_USE"):
    assert need in t2, "manifest 回读缺 " + need
print("  manifest: 回读断言 ok")
PY

apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
[[ -s "$WORK/AndroidManifest.xml" ]] || { echo "ERROR: 取回 AndroidManifest.xml 失败" >&2; exit 1; }
echo "  binary AndroidManifest.xml $(stat -f%z "$WORK/AndroidManifest.xml") B（只取 Manifest，不整包重建）"

mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO_SRC" "$WORK/lib/arm64-v8a/libstarry_magic.so"
cp -f "$MENU_DEX" "$WORK/classes3.dex"

echo "== [3/9] libc++_shared.so（底包无 ⇒ 从 NDK 拷）"
if unzip -l "$ORIG_APK" | grep -q 'lib/arm64-v8a/libc++_shared.so'; then
  echo "  底包已有 libc++_shared.so，沿用底包，不新增"
else
  [[ -f "$NDK_CXX" ]] || { echo "ERROR: missing $NDK_CXX" >&2; exit 1; }
  cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
  echo "  + libc++_shared.so ← $NDK_CXX ($(stat -f%z "$NDK_CXX") B)"
fi

echo "== [4/9] 流式 zip 合并（classes.dex 原样 / classes2.dex 换补丁 / classes3.dex 星光菜单）"
"$PY_BIN" - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" "$DEX2_PATCHED" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, dex2 = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c2 = dex2.read_bytes()
extras = {"classes3.dex": c3}
libdir = work / "lib"
for p in sorted(libdir.rglob("*")):
    if p.is_file():
        extras["lib/" + str(p.relative_to(libdir)).replace("\\", "/")] = p.read_bytes()
assert "lib/arm64-v8a/libstarry_magic.so" in extras, "libstarry_magic.so not staged"
REPLACE = {"classes2.dex": c2}

class RawNameZipInfo(zipfile.ZipInfo):
    _raw_name = None
    _keep_flag = 0
    def _encodeFilenameFlags(self):
        if self._raw_name is not None:
            return self._raw_name, self._keep_flag
        return super()._encodeFilenameFlags()

def make_info(item):
    keep_flag = item.flag_bits & 0x800
    raw = item.orig_filename.encode("utf-8", "replace") if keep_flag \
        else item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if keep_flag or raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw
        info._keep_flag = 0x800 if keep_flag else 0
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    return info

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "classes.dex" in names and "classes2.dex" in names, "底包 dex 布局与假设不符"
dup = set(extras) & names
assert not dup, "版本假设失效（原包已含注入项）: %s" % sorted(dup)

zout = zipfile.ZipFile(dst, "w")
n = 0; replaced = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name == "AndroidManifest.xml":
        info = make_info(item); info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, mf); n += 1; continue
    if name in REPLACE:
        info = make_info(item)
        zout.writestr(info, REPLACE[name]); n += 1; replaced += 1; continue
    if name in extras:
        continue
    info = make_info(item)
    with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
        import shutil; shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1
assert replaced == 1, "classes2.dex 替换 %d 次（应为 1）" % replaced

for name in sorted(extras):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED
    einfo.create_system = 3
    zout.writestr(einfo, extras[name])
zout.close(); zin.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

PROTECTED = ["classes.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so"]
for nm in PROTECTED:
    h1, h2 = stream_sha(zin2, nm), stream_sha(zout2, nm)
    assert h1 == h2, "PROTECTED MUTATED: " + nm
    print("  sha256 一致: %-32s %s…" % (nm, h1[:16]))
assert stream_sha(zout2, "classes2.dex") == hashlib.sha256(c2).hexdigest(), "classes2.dex 不是补丁版"
print("  classes2.dex == 补丁 dex（sha256 一致）")
assert stream_sha(zout2, "classes3.dex") == hashlib.sha256(c3).hexdigest(), "classes3.dex 不是菜单 dex"
print("  classes3.dex == 菜单 dex（sha256 一致）")

SKIP = set(extras) | set(REPLACE) | {"AndroidManifest.xml"}
meta = raw = flag = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name in SKIP:
        continue
    m = zout2.getinfo(name)
    if item.CRC != m.CRC or item.file_size != m.file_size or item.compress_type != m.compress_type:
        meta += 1; print("  MISMATCH meta: " + name)
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw += 1
    if (item.flag_bits & 0x800) != (m.flag_bits & 0x800):
        flag += 1
assert meta == 0 and raw == 0 and flag == 0, "meta=%d rawname=%d flag=%d" % (meta, raw, flag)
missing = [x for x in ("classes3.dex", "lib/arm64-v8a/libstarry_magic.so") if x not in zout2.namelist()]
assert not missing, "missing injectables: %s" % missing
print("  合并 %d 条（替换 classes2.dex 1 条）+ 新增 %d 条" % (n, len(extras)))
print("  全量 CRC/size/ctype 差异=0，name-bytes/utf8-flag 差异=0")
zin2.close(); zout2.close()
PY

echo "== [5/9] zipalign -f -p 4 + apksigner sign"
"$BT/zipalign" -f -p 4 "$TMP_UNSIGNED" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: zipalign -c 失败" >&2; exit 1; }
rm -f "$TMP_UNSIGNED" "$SIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$TMP_ALIGNED"
rm -f "$TMP_ALIGNED"
rm -f "$SIGNED.idsig"   # v4 签名副产物，交付目录不留
[[ -f "$SIGNED" ]] || { echo "ERROR: 签名无产物" >&2; exit 1; }
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
SIZE="$(stat -f%z "$SIGNED")"
SHA="$(shaof "$SIGNED")"
MD5="$(md5of "$SIGNED")"
echo "  产物: $SIGNED"
echo "  size: $SIZE  sha256: $SHA  md5: $MD5"

echo "== [6/9] 自检① 包内 dex md5 == 补丁 dex / 菜单 dex md5"
IN_DEX2_MD5="$(unzip -p "$SIGNED" classes2.dex | openssl md5 -r | awk '{print $1}')"
[[ "$IN_DEX2_MD5" == "$DEX2_MD5" ]] || { echo "✗ classes2.dex md5 $IN_DEX2_MD5 != $DEX2_MD5" >&2; exit 1; }
echo "  ✓ classes2.dex md5=${IN_DEX2_MD5}（== 补丁源）"
IN_DEX3_MD5="$(unzip -p "$SIGNED" classes3.dex | openssl md5 -r | awk '{print $1}')"
[[ "$IN_DEX3_MD5" == "$MENU_MD5" ]] || { echo "✗ classes3.dex md5 $IN_DEX3_MD5 != $MENU_MD5" >&2; exit 1; }
echo "  ✓ classes3.dex md5=${IN_DEX3_MD5}（== 菜单 dex）"

echo "== [7/9] 自检② so 闭环：包内 so md5 == 源 so md5"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | openssl md5 -r | awk '{print $1}')"
[[ "$IN_SO_MD5" == "$SO_MD5" ]] || { echo "✗ 包内 so $IN_SO_MD5 != 源 ${SO_MD5}（旧 so 混包）" >&2; exit 1; }
echo "  ✓ libstarry_magic.so md5=${IN_SO_MD5}（== 源）"
if [[ -f "$WORK/lib/arm64-v8a/libc++_shared.so" ]]; then
  IN_CXX_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libc++_shared.so | openssl md5 -r | awk '{print $1}')"
  [[ "$IN_CXX_MD5" == "$(md5of "$WORK/lib/arm64-v8a/libc++_shared.so")" ]] \
    || { echo "✗ libc++_shared.so md5 不符" >&2; exit 1; }
  echo "  ✓ libc++_shared.so md5=$IN_CXX_MD5"
fi

echo "== [8/9] 自检③ 保留项 sha256 vs 底包"
tmpd="$WORK/sha"; mkdir -p "$tmpd"
for nm in classes.dex resources.arsc lib/arm64-v8a/libil2cpp.so lib/arm64-v8a/libunity.so; do
  unzip -p "$ORIG_APK" "$nm" > "$tmpd/a"
  unzip -p "$SIGNED" "$nm" > "$tmpd/b"
  A="$(shaof "$tmpd/a")"; B="$(shaof "$tmpd/b")"
  [[ "$A" == "$B" ]] || { echo "✗ $nm sha256 不一致: $A vs $B" >&2; exit 1; }
  echo "  ✓ $nm sha256=$A"
done

echo "== [8b/9] 自检④ aapt2 dump xmltree 复读（顺序/权限/服务）"
"$AAPT2" dump xmltree --file AndroidManifest.xml "$SIGNED" > "$WORK/tree.txt"
"$PY_BIN" - "$WORK/tree.txt" <<'PY'
import sys
lines = open(sys.argv[1], encoding="utf-8", errors="replace").read().splitlines()
i_prov = i_tap = i_ok = i_svc = -1
app_line = -1
for i, l in enumerate(lines):
    if l.strip().startswith("E: application"):
        app_line = i
    if "com.starry.magic.StarryInitProvider" in l and i_prov < 0: i_prov = i
    if "TapUpdateFileProvider" in l and i_tap < 0: i_tap = i
    if "OkDownloadProvider" in l and i_ok < 0: i_ok = i
    if "com.starry.magic.menu.MenuService" in l and i_svc < 0: i_svc = i
first_child = None
if app_line >= 0:
    for l in lines[app_line+1:]:
        s = l.strip()
        if s.startswith("E: "):
            first_child = s; break
assert i_prov > 0, "✗ StarryInitProvider 不存在"
assert i_tap > 0 and i_ok > 0, "✗ TapTap provider 缺失（复读异常）"
assert i_prov < i_tap, "✗ StarryInitProvider(%d) 不在 TapUpdateFileProvider(%d) 之前" % (i_prov, i_tap)
assert i_prov < i_ok, "✗ StarryInitProvider(%d) 不在 OkDownloadProvider(%d) 之前" % (i_prov, i_ok)
assert i_svc > 0, "✗ MenuService 不存在"
assert first_child and "provider" in first_child, "✗ <application> 第一子节点不是 provider: %s" % first_child
body = "\n".join(lines)
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE_SPECIAL_USE"):
    assert perm in body, "✗ 缺权限 " + perm
assert 'android:name(0x01010003)="android.permission.FOREGROUND_SERVICE"' in body, "✗ 缺 FOREGROUND_SERVICE"
assert 'com.hundunjidian.hxjx.starryinit' in body, "✗ StarryInitProvider authorities 不符"
svc_block = "\n".join(lines[i_svc:i_svc+8])
assert "foregroundServiceType" in svc_block and "0x40000000" in svc_block, "✗ MenuService foregroundServiceType 不是 specialUse(0x40000000)"
assert "PROPERTY_SPECIAL_USE_FGS_SUBTYPE" in svc_block, "✗ MenuService 缺 specialUse property"
print("  ✓ <application> 第一子节点 = %s" % first_child)
print("  ✓ StarryInitProvider(line %d) < TapUpdateFileProvider(line %d) < OkDownloadProvider(line %d)" % (i_prov, i_tap, i_ok))
print("  ✓ MenuService(line %d) 存在（specialUse FGS）" % i_svc)
print("  ✓ 三权限 SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE 均在")
PY

echo "== [9/9] 自检⑤ framework_entry_audit / 自检⑥ preflight"
if [[ "$RUN_PREFLIGHT" == "1" ]]; then
  bash "$NIXIANG/04_scripts/_common/preflight.sh" "$0" || { echo "✗ preflight FAIL" >&2; exit 1; }
fi
AUDIT_RC=0
bash "$NIXIANG/04_scripts/_common/framework_entry_audit.sh" --game "$GAME" --pkg "$PKG" --apk "$SIGNED" > "$WORK/audit.txt" 2>&1 || AUDIT_RC=$?
cat "$WORK/audit.txt"
if [[ "$AUDIT_RC" != "0" ]]; then
  XS="$(grep -cE '^ +✗' "$WORK/audit.txt" || true)"
  LEGACY_ONLY="$(grep -E '^ +✗' "$WORK/audit.txt" | grep -cE 'GameConfig.h 无 defined|Android.mk 无未注释的 LOCAL_CFLAGS|激活分支 = .*--game' || true)"
  if [[ "$AUDIT_WAIVER" == "1" && "$XS" == "$LEGACY_ONLY" && "$XS" != "0" ]]; then
    echo "  ⚠ WAIVER(--audit-waiver-modern)：全部 ${XS} 项 ✗ 均落在 legacy-only 路径" >&2
    echo "    [C] GameConfig.h 是 legacy 专属（该文件头明写新游戏禁止加映射；本游戏走 modern 注册表 Games/HxjxEntry.cpp authGameId=\"hxjx\"）" >&2
    echo "    [D] 私有 mk Android_hxjx.mk 走 modern 模板，本就没有 -DSTARRY_GAME_*；so 不在本脚本重编，由 [Hxjx]/hxjx= 断言与 so 闭环 md5 兜住" >&2
  else
    echo "✗ framework_entry_audit FAIL（见上方 ✗ 项）" >&2; exit 1
  fi
fi

echo
echo "===== SUMMARY ====="
echo "apk       : $SIGNED"
echo "size      : $SIZE"
echo "sha256    : $SHA"
echo "md5       : $MD5"
echo "dex2 md5  : ${IN_DEX2_MD5}（补丁版；源 ${DEX2_MD5}）"
echo "dex3 md5  : ${IN_DEX3_MD5}（星光菜单）"
echo "so   md5  : ${IN_SO_MD5}（源 ${SO_MD5}）"
echo "menu dex  : $MENU_SOURCE"
echo "install   : 未装机（脚本无 adb）"
