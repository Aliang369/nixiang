#!/usr/bin/env bash
# 我本布衣：世家录 1.1.66 (vc94) TapTap 原版 → 星空魔法外科组装（Unity IL2CPP）
#
# 产出：08_output/我本布衣世家录/我本布衣世家录_1.1.66_surgical-unsigned.apk
#
# 版本沿革（1.1.56/vc71 → 1.1.66/vc94，2026-10-31）：
#   底包 md5 63863cae… → 0a1d57ed…（雷电模拟器只读拉取已装原版）
#   官方 dex 由 4 个变 3 个（classes.dex/2/3）⇒ 新增 dex 落位 classes4.dex
#   libil2cpp.so sha256 8d27a4ff… → ce2d6551…（重编 ⇒ RVA 全量重定位，
#   见 Games/我本布衣世家录Mod.cpp 头注 v5 与 10_docs/我本布衣世家录/路线锁定.md）
#
# 注入内容
#   classes4.dex                       星空菜单（templates/wbjy_menu_current，javac+d8 产出）
#   lib/arm64-v8a/libstarry_magic.so   native：6 功能 hook
#   lib/arm64-v8a/libc++_shared.so     星空 so 依赖
#   assets/certs/starlight.crt         星空证书
#   AndroidManifest.xml                分支 B（StarryInitProvider + MenuService + FGS 权限）
#                                      ★不替换官方 Application，也不改 appComponentFactory
#
# ★本作官方 dex 有 3 个，不 patch 任何官方 dex：
#   全部功能在 native 层 hook，官方 classes*.dex 必须逐字节不变。
#
# 字节不变（脚本内 sha256 断言）：classes.dex / classes2.dex / classes3.dex /
#   resources.arsc / lib/arm64-v8a/libil2cpp.so
set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"

GAME="我本布衣世家录"
VERSION="1.1.66"
PKG="com.JAGames.TalesoftheClan"
NEW_DEX="classes4.dex"             # 官方已有 classes.dex/2/3（最小空闲下标）
SERIAL="${SERIAL:-emulator-5554}"

ORIG_APK="${ORIG_APK:-${WBJY_BASE_APK:-$NIXIANG/.workbuddy/scratch/wbjy_base/${GAME}_1.1.66_vc94_base.apk}}"
ORIG_MD5="${ORIG_MD5:-0a1d57ed675ad0b955c6221343c2bc93}"
ORIG_CLASSES_MD5="${ORIG_CLASSES_MD5:-cf932e76fa03cd6d36f32ac190e69e3f}"
ORIG_CLASSES2_MD5="${ORIG_CLASSES2_MD5:-43b58e383a899ab10c03296a7227f12d}"
ORIG_CLASSES3_MD5="${ORIG_CLASSES3_MD5:-705e4c8aadd57c591c269b9ca99d6c53}"
ORIG_ARSC_MD5="${ORIG_ARSC_MD5:-97f997140707ddf7c31fc6fda30198e6}"
# §23 RVA 锚点绑定：锚点表只在本 so 上有效（Games/我本布衣世家录Mod.cpp ② 锚点常量）
ORIG_IL2CPP_SO_SHA256="${ORIG_IL2CPP_SO_SHA256:-ce2d6551a1cabb46b274d0805c2c15d48252a6d4ae48607cb6575b7d18a0e57c}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"

SO_PREBUILT="${WBJY_SO:-$STARRY_SRC/libs_wbjy/arm64-v8a/libstarry_magic.so}"
MENU_DEX="${WBJY_MENU_DEX:-$STARRY_SRC/templates/wbjy_menu_current/$NEW_DEX}"
LOGIN_DEX="${WBJY_LOGIN_DEX:-$STARRY_SRC/templates/wbjy_menu_current/classes2_loginpatched.dex}"
CERT_SRC="$STARRY_SRC/app/src/main/assets/certs/starlight.crt"

JAVA_HOME="${JAVA_HOME:-/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"

PY="${NIXIANG_PYTHON:-python3}"
[[ -n "$PY" ]] || { echo "ERROR: 无 python" >&2; exit 1; }

md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
sha256f() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }
AAPT2=""
if command -v aapt2 >/dev/null 2>&1; then AAPT2="$(command -v aapt2)"; fi
if [[ -z "$AAPT2" ]]; then
  for v in 36.1.0 35.0.0 34.0.0; do
    c="${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk/build-tools/$v/aapt2.exe"
    if [[ -f "$c" ]]; then AAPT2="$c"; break; fi
  done
fi
NDK="${ANDROID_NDK_HOME:-${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk/ndk/27.0.12077973}"
# apktool：非登录 shell 的 PATH 里可能没有 → 回落到仓库自带 jar
APKTOOL="${APKTOOL:-}"
if [[ -z "$APKTOOL" ]]; then
  if command -v apktool >/dev/null 2>&1; then
    APKTOOL="apktool"
  else
    APKTOOL="java -jar $NIXIANG/01_tools/apktool/apktool.jar"
  fi
fi
CXX_SHARED=""
for h in windows-x86_64 darwin-x86_64 linux-x86_64; do
  c="$NDK/toolchains/llvm/prebuilt/$h/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
  if [[ -f "$c" ]]; then CXX_SHARED="$c"; break; fi
done

# 底包准备：优先本地；缺失则从设备只读拉取**已装**原版（不写 02_apks）
if [[ ! -f "$ORIG_APK" ]]; then
  echo "[check] 底包缺失 → 从设备 $SERIAL 只读拉取已装 base.apk"
  REMOTE="$(adb -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' | sed 's/^package://' | head -1)"
  [[ -n "$REMOTE" ]] || { echo "ERROR: 设备 $SERIAL 未装 ${PKG}，且无底包 $ORIG_APK" >&2; exit 1; }
  mkdir -p "$(dirname "$ORIG_APK")"
  adb -s "$SERIAL" pull "$REMOTE" "$ORIG_APK" >/dev/null || { echo "ERROR: adb pull 失败 $REMOTE" >&2; exit 1; }
fi
[[ -f "$ORIG_APK" ]]  || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT (先跑 wbjy_build_so.sh)" >&2; exit 1; }
[[ -f "$MENU_DEX" ]]  || { echo "ERROR: missing $MENU_DEX (先跑 wbjy_build_dex.sh)" >&2; exit 1; }
[[ -f "$LOGIN_DEX" ]] || { echo "ERROR: missing $LOGIN_DEX (先跑 wbjy_patch_login_dex.sh)" >&2; exit 1; }
[[ -f "$CERT_SRC" ]]  || { echo "ERROR: missing $CERT_SRC" >&2; exit 1; }

echo "[check] 底包 = $ORIG_APK"
GOT_MD5="$(md5f "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 原包 md5=$GOT_MD5 != $ORIG_MD5" >&2; exit 1; }
echo "[check] 原包 md5 ok"

# §23：底包内 libil2cpp.so 必须与锚点表绑定的 so 同源，否则 RVA 全错（按名兜底会掩盖）
GOT_IL2CPP_SHA="$(unzip -p "$ORIG_APK" lib/arm64-v8a/libil2cpp.so | (sha256sum 2>/dev/null || shasum -a 256) | awk '{print $1}')"
[[ "$GOT_IL2CPP_SHA" == "$ORIG_IL2CPP_SO_SHA256" ]] \
  || { echo "ERROR: 底包 libil2cpp.so sha256=$GOT_IL2CPP_SHA != ${ORIG_IL2CPP_SO_SHA256}（锚点表未绑定本 so）" >&2; exit 1; }
echo "[check] 底包 libil2cpp.so sha256 ok（锚点表同源）"
GOT_C1="$(unzip -p "$ORIG_APK" classes.dex | md5sum | awk '{print $1}')"
[[ "$GOT_C1" == "$ORIG_CLASSES_MD5" ]] || { echo "ERROR: classes.dex md5=$GOT_C1 != $ORIG_CLASSES_MD5" >&2; exit 1; }
GOT_C2="$(unzip -p "$ORIG_APK" classes2.dex | md5sum | awk '{print $1}')"
[[ "$GOT_C2" == "$ORIG_CLASSES2_MD5" ]] || { echo "ERROR: classes2.dex md5=$GOT_C2 != $ORIG_CLASSES2_MD5" >&2; exit 1; }
GOT_C3="$(unzip -p "$ORIG_APK" classes3.dex | md5sum | awk '{print $1}')"
[[ "$GOT_C3" == "$ORIG_CLASSES3_MD5" ]] || { echo "ERROR: classes3.dex md5=$GOT_C3 != $ORIG_CLASSES3_MD5" >&2; exit 1; }
GOT_ARSC="$(unzip -p "$ORIG_APK" resources.arsc | md5sum | awk '{print $1}')"
[[ "$GOT_ARSC" == "$ORIG_ARSC_MD5" ]] || { echo "ERROR: resources.arsc md5=$GOT_ARSC != $ORIG_ARSC_MD5" >&2; exit 1; }
echo "[check] classes*.dex / resources.arsc md5 ok"

# so 归属校验（防串包）。★用 -F，方括号是 grep 正则元字符
if ! LC_ALL=C grep -aqF -- '[wbjy]' "$SO_PREBUILT" \
   || ! LC_ALL=C grep -aq -- 'Toggle_免广告' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 不是 Wbjy 版（缺 [wbjy] / Toggle_免广告）" >&2
  strings -a "$SO_PREBUILT" | grep -aoE '^\[[A-Za-z]+\]' | sort -u | head -20 || true
  exit 1
fi
for s in '[Hzsj]' '[Gyzw]' '[Ysxz]' '[Dwct]' '[Tlgd]' '[Blfx]'; do
  if LC_ALL=C grep -aqF -- "$s" "$SO_PREBUILT"; then
    echo "ERROR: $SO_PREBUILT 含别家标签 ${s}，libs_wbjy 被覆盖" >&2; exit 1
  fi
done
SO_MD5="$(md5f "$SO_PREBUILT")"
echo "[check] so = $SO_PREBUILT md5=$SO_MD5"

# 菜单 dex 归属校验（六项功能串必须在）
LC_ALL=C grep -aq 'com/starry/magic/menu/MenuService' "$MENU_DEX" \
  || { echo "ERROR: $MENU_DEX 不像星光菜单 dex" >&2; exit 1; }

LOCK_DIR="$NIXIANG/.workbuddy/scratch/wbjy_surgical.lockdir"
mkdir -p "$(dirname "$LOCK_DIR")"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$NIXIANG/.workbuddy/scratch/wbjy_surgical.$$"
rm -rf "$WORK"; mkdir -p "$WORK"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/4
echo "[1/4] 菜单 dex + so + 证书 备料"
cp "$MENU_DEX" "$WORK/$NEW_DEX"
mkdir -p "$WORK/lib/arm64-v8a" "$WORK/assets/certs"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
if [[ -n "$CXX_SHARED" ]]; then
  cp "$CXX_SHARED" "$WORK/lib/arm64-v8a/libc++_shared.so"
else
  echo "ERROR: 找不到 libc++_shared.so（NDK=${NDK}）" >&2; exit 1
fi
cp "$CERT_SRC" "$WORK/assets/certs/starlight.crt"
ls -l "$WORK/$NEW_DEX" "$WORK/lib/arm64-v8a/" "$WORK/assets/certs/"

# ---------------------------------------------------------------- 2/4
echo "[2/4] AndroidManifest：分支 B（StarryInitProvider）+ MenuService + FGS 权限"
if [[ -n "${WBJY_BINARY_MANIFEST:-}" && -f "${WBJY_BINARY_MANIFEST:-}" ]]; then
  cp "$WBJY_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  from WBJY_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  # 本作 resources.arsc 被资源混淆，apktool b 会在 values-*/mipmaps.xml 编译期失败
  # ⇒ 直接对**二进制** manifest 打补丁（纯 stdlib，不重建资源表）
  unzip -p "$ORIG_APK" AndroidManifest.xml > "$WORK/mf_orig.xml"
  "$PY" "$NIXIANG/04_scripts/我本布衣世家录/patch_wbjy_binary_manifest.py" \
    "$WORK/mf_orig.xml" "$WORK/AndroidManifest.xml" || { echo "ERROR: 二进制 manifest 打补丁失败" >&2; exit 1; }
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -l "$WORK/AndroidManifest.xml"

if [[ -n "$AAPT2" && -x "$AAPT2" ]]; then
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
  grep -q "com.starry.magic.menu.MenuService"   "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE"      "$WORK/tree_new.txt" || { echo "ERROR: FGS perm missing" >&2; exit 1; }
  grep -q "$PKG" "$WORK/tree_new.txt" || { echo "ERROR: 包名丢失" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" | grep -vE 'platformBuildVersion(Name|Code)=' > "$WORK/o.norm" || true
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt"  | grep -vE 'platformBuildVersion(Name|Code)=' > "$WORK/n.norm" || true
  LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行" >&2; diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | head -10; exit 1; }
  echo "  aapt2 校验通过（官方节点零丢失）"
else
  echo "  [WARN] 无 aapt2，跳过 manifest 节点比对"
fi

# ---------------------------------------------------------------- 3/4
echo "[3/4] zip surgical merge → $UNSIGNED"
"$PY" - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$LOGIN_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_dex = sys.argv[4]
login_dex_path = Path(sys.argv[5])

mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
clogin = login_dex_path.read_bytes()

extras = {"classes2.dex": clogin}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

# 官方条目必须逐字节不变（classes2.dex 例外：TapSDK 登录/验签改包，A 轨）
UNTOUCHED = ["classes.dex", "classes3.dex",
             "resources.arsc", "lib/arm64-v8a/libil2cpp.so"]
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"{new_dex} already in orig"
for n in UNTOUCHED:
    assert n in names, f"orig apk missing {n}"

def is_sig(name):
    if name == "META-INF/MANIFEST.MF": return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith(SIG_EXT)

kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n_written = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    if name == "AndroidManifest.xml":
        data = mf
        ct = zipfile.ZIP_DEFLATED
    else:
        data = zin.read(item)
        ct = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ct
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
for name, data in [(new_dex, cnew)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
zin.close(); zout.close()

zi2, zo2 = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "r")
for name in UNTOUCHED:
    a = hashlib.sha256(zi2.read(name)).hexdigest()
    b = hashlib.sha256(zo2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zo2.namelist())
for name in kept_meta:
    assert name in out_names, f"lost {name}"
assert new_dex in out_names
assert zo2.read("classes2.dex") == clogin, "classes2.dex 不是登录改包版"
assert b"com.taptap.sdk.act1on" in zo2.read("classes2.dex"), "classes2.dex 缺 A 轨常量"
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert "lib/arm64-v8a/libc++_shared.so" in out_names
assert "assets/certs/starlight.crt" in out_names
assert not any(x.upper().endswith(SIG_EXT) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries={n_written}; {new_dex}={len(cnew)}; untouched {len(UNTOUCHED)} verified")
zi2.close(); zo2.close()
PY

PKG_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5sum | awk '{print $1}')"
[[ "$PKG_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != $SO_MD5" >&2; exit 1; }
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
for e in "$NEW_DEX" "classes.dex" "lib/arm64-v8a/libstarry_magic.so" \
         "lib/arm64-v8a/libc++_shared.so" "assets/certs/starlight.crt"; do
  grep -q "^$e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done
echo "  包内 so md5 ok；$NEW_DEX 到位"

# ---------------------------------------------------------------- 4/4
echo "[4/4] 完成（未签名）→ $UNSIGNED"
echo "unsigned_md5=$(md5f "$UNSIGNED")"
echo "unsigned_sha256=$(sha256f "$UNSIGNED")"
ls -lh "$UNSIGNED"
echo "DONE: ${UNSIGNED}（下一步：sign）"
