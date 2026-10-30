#!/usr/bin/env bash
# 霓虹深渊2 (com.veewo.neonabyss2) TapTap 渠道 — 外科组装 + TapTap 双轨（2026-10-30 r33 / 2026.10.30.c00d058f3fd / vc682）
#
# 基线：真机(OPD2404/f9a116a2)已装官方 taptap 底包只读拉取件
#   02_apks/霓虹深渊2/nhsy2_base_2026.10.30.apk  1,347,614,619 B
#   md5 9bc927e33d78fecb41e0ec555ec70fc7 / cert MD5 bbe1da8584fb2fb1017a512b2e99ff21 (O=Veewo)
#
# 官包零改动基线：classes.dex / classes2.dex / classes4.dex / resources.arsc /
#   lib/arm64-v8a/{libil2cpp.so,libunity.so} 字节不变。
#
# 改动（单趟 zip 组装，一次写盘）：
#   - AndroidManifest.xml：+StarryInitProvider(initOrder=MAX) +MenuService +3 条权限
#     + <application android:name="bin.mt.signature.SgKillerApp">（B 轨挂载点；
#       保留官方 appComponentFactory="androidx.core.app.CoreComponentFactory"）
#   - classes3.dex（就地替换，只改 1 处方法体）：A 轨登录
#       com/taptap/sdk/login/impl/utils/ClientLoginUtils.isClientLogin → 恒 false
#     ★ 2026-10-29 起旧 `TapSignKit.getSign` 回灌补丁**已退役**（同一链条禁止双修），
#       签名真伪一律走 B 轨（classes6 killer）。本脚本断言 classes3 内不得出现 starry_sign_bypass。
#   - classes5.dex：新增（星光 smali：StarryInitProvider / MenuService / TapLoginForceWeb / OTA）
#   - classes6.dex：新增（B 轨 MT KillerApplication193 + 空壳 SgKillerApp，证书＝官方 DER base64）
#   - lib/arm64-v8a/libstarry_magic.so：新增；官方包无 libc++_shared.so ⇒ 自带一份
#   - assets/certs/starlight.crt：新增
#   - 保留官方 META-INF/services/**，剔除签名件（*.SF/*.RSA/MANIFEST.MF）
#
# 产物：08_output/霓虹深渊2/霓虹深渊2_2026.10.30_surgical-unsigned.apk
# 下一步：bash 04_scripts/霓虹深渊2/nhsy2_sign_2026.10.30.sh
#
# 用法:
#   bash scripts/surgical_nhsy2_2026.10.30.sh
# 可覆盖环境变量:
#   NHSY2_ORIG_APK    底包路径
#   NHSY2_SO          预构建 so（默认 <repo>/libs_nhsy2/arm64-v8a/libstarry_magic.so）
#   NHSY2_MENU_DEX    预构建菜单 dex（=classes5.dex 内容；给了就跳过 gradle）
#   NHSY2_KILLER_SMALI  killer smali 目录（默认 07_patched/暗影格斗3/surgical/killer_smali）
#   NHSY2_STARRY_APK  预构建星光 apk（gradle 产物）
#   NHSY2_SKIP_GRADLE=1  跳过 gradle（要求 NHSY2_STARRY_APK 或已有 app-release.apk）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"
GAME="霓虹深渊2"
VERSION="2026-10-30.c00d058f3fd"
VER_SHORT="2026.10.30"
PKG="com.veewo.neonabyss2"
KILLER_APP="bin.mt.signature.SgKillerApp"
ORIG_APK="${NHSY2_ORIG_APK:-$NIXIANG/02_apks/$GAME/nhsy2_base_${VER_SHORT}.apk}"
ORIG_MD5="9bc927e33d78fecb41e0ec555ec70fc7"
OFFICIAL_CERT_MD5="bbe1da8584fb2fb1017a512b2e99ff21"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VER_SHORT}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical_${VER_SHORT}/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
KILLER_SMALI="${NHSY2_KILLER_SMALI:-$NIXIANG/07_patched/暗影格斗3/surgical/killer_smali}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
AUTHORITY="$PKG.starryinit"
STARRY_DEX="classes5.dex"
KILLER_DEX="classes6.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

echo "=== 霓虹深渊2 ${VER_SHORT} 外科组装 + TapTap 双轨（r33） ==="

# ------------------------------------------------------------- 0/8 输入与指纹
echo "[0/8] 输入核对"
[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺底包 $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: 缺 smali/baksmali" >&2; exit 1; }
[[ -d "$KILLER_SMALI" ]] || { echo "ERROR: 缺 killer smali 目录 $KILLER_SMALI" >&2; exit 1; }

ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 底包指纹不符 got=$ACTUAL_MD5 want=$ORIG_MD5" >&2; exit 1; }
APKSIGNER_BIN="${APKSIGNER_BIN:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
[[ -x "$APKSIGNER_BIN" ]] || { echo "ERROR: 找不到 apksigner（${APKSIGNER_BIN} ）" >&2; exit 1; }
CERT_MD5="$("$APKSIGNER_BIN" verify --print-certs "$ORIG_APK" 2>/dev/null \
  | awk -F': ' '/MD5 digest/ {print $2}' | tr -d ' ' || true)"
[[ "$CERT_MD5" == "$OFFICIAL_CERT_MD5" ]] \
  || { echo "ERROR: 底包证书 MD5 不符 got=$CERT_MD5 want=${OFFICIAL_CERT_MD5} （证书变了要先复核 TapTap 双轨前提）" >&2; exit 1; }
echo "  底包 md5=$ACTUAL_MD5 cert_md5=$CERT_MD5 ✓"

LOCK_DIR="/tmp/nhsy2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held (pid $(cat "$LOCK_DIR/pid" 2>/dev/null))" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/nhsy2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/8 native so
echo "[1/8] native so"
SO_PREBUILT="${NHSY2_SO:-$STARRY_SRC/libs_nhsy2/arm64-v8a/libstarry_magic.so}"
SO_EXPLICIT=0
if [[ -n "${NHSY2_SO:-}" ]]; then SO_EXPLICIT=1; fi
SO_STALE=0; NEWEST_SRC=""
for s in "$STARRY_SRC/app/src/main/jni/Games/nhsy2Mod.cpp" \
         "$STARRY_SRC/app/src/main/jni/Games/nhsy2Mod.h" \
         "$STARRY_SRC/app/src/main/jni/Games/nhsy2Entry.cpp" \
         "$STARRY_SRC/app/src/main/jni/Unity/Il2CppApi.cpp" \
         "$STARRY_SRC/app/src/main/jni/Unity/Il2CppApi.h"; do
  [[ -f "$s" ]] || continue
  if [[ -z "$NEWEST_SRC" || "$s" -nt "$NEWEST_SRC" ]]; then NEWEST_SRC="$s"; fi
done
if [[ -f "$SO_PREBUILT" && -n "$NEWEST_SRC" && "$NEWEST_SRC" -nt "$SO_PREBUILT" ]]; then
  SO_STALE=1
  echo "  ⚠ so 陈旧：$(basename "$NEWEST_SRC") 比预构建 so 新 ⇒ 强制重编（防「改了 cpp 却跑旧 so」）"
fi
if [[ "$SO_STALE" -eq 1 && "$SO_EXPLICIT" -eq 0 ]]; then rm -f "$SO_PREBUILT"; fi
if [[ -f "$SO_PREBUILT" ]]; then
  echo "  using $SO_PREBUILT"
else
  ( cd "$STARRY_SRC/app/src/main/jni" && "$NDK/ndk-build" \
      NDK_PROJECT_PATH="$STARRY_SRC/app" \
      APP_BUILD_SCRIPT=./Android_nhsy2.mk \
      NDK_APPLICATION_MK=./Application.mk \
      NDK_OUT="$STARRY_SRC/obj_nhsy2" NDK_LIBS_OUT="$STARRY_SRC/libs_nhsy2" ) >"$WORK/so_build.log" 2>&1 \
    || { tail -40 "$WORK/so_build.log"; echo "ERROR: ndk-build failed" >&2; exit 1; }
  SO_PREBUILT="$STARRY_SRC/libs_nhsy2/arm64-v8a/libstarry_magic.so"
fi
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing so $SO_PREBUILT" >&2; exit 1; }
LC_ALL=C grep -aq 'nhsy2' "$SO_PREBUILT" \
  || { echo "ERROR: so 内无 nhsy2 标记（旧 so / 编错 mk）" >&2; exit 1; }
NM_BIN="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
if [[ -x "$NM_BIN" ]]; then
  if "$NM_BIN" -D --undefined-only "$SO_PREBUILT" | grep -q emutls; then
    echo "ERROR: so 依赖 __emutls_get_address（缺 -fno-emulated-tls），游戏旧 libc++ 会 dlopen 失败" >&2
    exit 1
  fi
fi
SO_MD5="$(md5 -q "$SO_PREBUILT")"
echo "  so ok: $(stat -f '%z' "$SO_PREBUILT") B md5=$SO_MD5"

# ------------------------------------------------------------- 2/8 菜单 dex
echo "[2/8] 星光菜单 dex（${STARRY_DEX} ）"
if [[ -n "${NHSY2_MENU_DEX:-}" ]]; then
  [[ -f "${NHSY2_MENU_DEX}" ]] || { echo "ERROR: 缺 NHSY2_MENU_DEX 文件" >&2; exit 1; }
  cp -f "${NHSY2_MENU_DEX}" "$WORK/$STARRY_DEX"
  echo "  using prebuilt menu dex $NHSY2_MENU_DEX"
else
  if [[ -n "${NHSY2_STARRY_APK:-}" && -f "${NHSY2_STARRY_APK:-}" ]]; then
    STARRY_APK="$NHSY2_STARRY_APK"; echo "  using $STARRY_APK"
  elif [[ "${NHSY2_SKIP_GRADLE:-0}" == "1" && -f "$STARRY_SRC/app/build/outputs/apk/release/app-release.apk" ]]; then
    STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"; echo "  using $STARRY_APK (skip gradle)"
  else
    rsync -a --delete \
      --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
      --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
      --exclude 'obj_*/' --exclude 'libs_*/' \
      "$STARRY_SRC/" "$WORK/starry/"
    ( cd "$WORK/starry" && ./gradlew assembleRelease --console=plain ) >"$WORK/gradle.log" 2>&1 \
      || { tail -40 "$WORK/gradle.log"; echo "ERROR: gradle assembleRelease failed" >&2; exit 1; }
    STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
  fi
  [[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }
  mkdir -p "$WORK/starry_apk" && ( cd "$WORK/starry_apk" && unzip -o -q "$STARRY_APK" )
  SMALI_ROOT="$WORK/smali_in"
  mkdir -p "$SMALI_ROOT/com"
  shopt -s nullglob
  for dx in "$WORK/starry_apk"/classes*.dex; do
    bd="$WORK/starry_dec_$(basename "$dx" .dex)"
    "$BAKSMALI_BIN" d "$dx" -o "$bd" >/dev/null
    [[ -d "$bd/com/starry" ]] && rsync -a "$bd/com/starry" "$SMALI_ROOT/com/"
    [[ -d "$bd/dev" ]] && rsync -a "$bd/dev" "$SMALI_ROOT/"
  done
  shopt -u nullglob
  "$SMALI_BIN" assemble -o "$WORK/$STARRY_DEX" "$SMALI_ROOT"
  mkdir -p "$WORK/assets_certs_from_apk"
  [[ -d "$WORK/starry_apk/assets/certs" ]] && cp -R "$WORK/starry_apk/assets/certs/." "$WORK/assets_certs_from_apk/"
fi
python3 - "$WORK/$STARRY_DEX" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
for bad in (b"Lcom/starry/magic/sign/SignSpoof;", b"SignSpoofAppComponentFactory"):
    assert bad not in d, "菜单 dex 仍含已删类: %r" % bad
for need in (b"StarryInitProvider", b"MenuService"):
    assert need in d, "菜单 dex 缺 %r" % need
print("  菜单 dex 校验通过（无 SignSpoof，含 StarryInitProvider/MenuService）")
PY
echo "  $STARRY_DEX = $(stat -f '%z' "$WORK/$STARRY_DEX") B md5=$(md5 -q "$WORK/$STARRY_DEX")"

# ------------------------------------------- 3/8 classes3.dex（A 轨：只改登录）
echo "[3/8] classes3.dex：只打 A 轨登录补丁（isClientLogin 恒 false）"
mkdir -p "$WORK/c3"
( cd "$WORK/c3" && unzip -o -q "$ORIG_APK" classes3.dex )
"$BAKSMALI_BIN" d "$WORK/c3/classes3.dex" -o "$WORK/c3_smali" >/dev/null
python3 - "$WORK/c3_smali" <<'PY'
import re, sys
from pathlib import Path

root = Path(sys.argv[1])

def all_method_headers():
    hdr = {}
    for f in root.rglob("*.smali"):
        hs = [l.strip() for l in f.read_text(encoding="utf-8").splitlines() if l.startswith(".method ")]
        if hs:
            hdr[str(f.relative_to(root))] = hs
    return hdr

BEFORE = all_method_headers()

def replace_method(path, sig, new_body, marker):
    t = path.read_text(encoding="utf-8")
    if marker in t:
        print("  already patched: %s" % path.name)
        return
    m = re.search(r'(?ms)^\.method [^\n]*' + re.escape(sig) + r'\n.*?^\.end method\n', t)
    if not m:
        raise SystemExit("ERROR: method %s not found in %s" % (sig, path))
    path.write_text(t[:m.start()] + new_body + t[m.end():], encoding="utf-8")
    print("  patched %s::%s" % (path.name, sig.split("(")[0]))

# A 轨：TapSDK LoginTypes.from(ClientLoginUtils.isClientLogin) 恒 false ⇒ 回落 WebLoginFragment
replace_method(
    root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali",
    "isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z",
    """\
.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .registers 3

    # starry_force_web_login
    const/4 p0, 0x0

    return p0
.end method
""",
    "starry_force_web_login",
)

AFTER = all_method_headers()
assert set(AFTER) == set(BEFORE), "smali 文件集合变化"
for rel, hs in BEFORE.items():
    if AFTER[rel] != hs:
        raise SystemExit("ERROR: %s 的 .method 头发生变化（替换越界）" % rel)
print("  method 头集合逐字一致（%d 个方法，仅 1 处方法体替换）✓"
      % sum(len(v) for v in BEFORE.values()))
PY
if grep -rq "starry_sign_bypass" "$WORK/c3_smali"; then
  echo "ERROR: classes3 里仍出现 starry_sign_bypass（旧签名回灌补丁与 B 轨双修，禁止）" >&2
  exit 1
fi
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3_smali"

python3 - "$WORK/c3/classes3.dex" "$WORK/classes3.dex" <<'PY'
import struct, sys
def hdr(p):
    b = open(p, 'rb').read(0x70)
    return {
        'strings': struct.unpack_from('<I', b, 0x38)[0],
        'types':   struct.unpack_from('<I', b, 0x40)[0],
        'protos':  struct.unpack_from('<I', b, 0x48)[0],
        'fields':  struct.unpack_from('<I', b, 0x50)[0],
        'methods': struct.unpack_from('<I', b, 0x58)[0],
    }
a, b = hdr(sys.argv[1]), hdr(sys.argv[2])
print("  classes3 原:", a)
print("  classes3 新:", b)
for k in ('strings', 'types', 'protos', 'fields', 'methods'):
    if b[k] > a[k] + 8:
        raise SystemExit("ERROR: %s 增长异常 %d -> %d" % (k, a[k], b[k]))
assert b['methods'] < 65536, "ERROR: methods %d 超过 65536" % b['methods']
delta = a['methods'] - b['methods']
assert 0 <= delta <= 16, "ERROR: method_ids 异常 %d -> %d" % (a['methods'], b['methods'])
print("  method_ids %d -> %d（- %d，上限 65536）✓" % (a['methods'], b['methods'], delta))
PY

# ------------------------------------------------------------- 4/8 killer dex
echo "[4/8] ${KILLER_DEX} （B 轨）：MT killer 改包名/证书 → classes6.dex"
python3 - "$ORIG_APK" "$WORK" <<'PY'
import sys, zipfile, os
apk, work = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
cand = [n for n in z.namelist() if n.startswith("META-INF/") and n.upper().endswith(".RSA")]
assert len(cand) == 1, "META-INF *.RSA 数量异常: %s" % cand
open(os.path.join(work, "ANDROID-.RSA"), "wb").write(z.read(cand[0]))
print("  RSA 包:", cand[0], len(z.read(cand[0])), "B")
PY
openssl pkcs7 -inform DER -in "$WORK/ANDROID-.RSA" -print_certs -out "$WORK/cert.pem" 2>/dev/null \
  || { echo "ERROR: openssl 解析 PKCS7 失败" >&2; exit 1; }
openssl x509 -in "$WORK/cert.pem" -outform DER -out "$WORK/official.der"
FPR="$(openssl x509 -in "$WORK/cert.pem" -noout -fingerprint -md5 | sed 's/.*=//; s/://g' | tr 'A-Z' 'a-z')"
[[ "$FPR" == "$OFFICIAL_CERT_MD5" ]] || { echo "ERROR: 证书指纹 $FPR 不符" >&2; exit 1; }
echo "  官方证书 DER: $(stat -f '%z' "$WORK/official.der") B  MD5=$FPR ✓"

rm -rf "$WORK/killer_smali"
cp -R "$KILLER_SMALI" "$WORK/killer_smali"
python3 - "$WORK/killer_smali" "$WORK/official.der" "$PKG" <<'PY'
import base64, io, os, sys
root, der_path, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
der = open(der_path, "rb").read()
b64 = base64.b64encode(der).decode()
wrapped = "\\n".join(b64[i:i+76] for i in range(0, len(b64), 76))
p = os.path.join(root, "bin/mt/signature/KillerApplication193.smali")
t = io.open(p, encoding="utf-8").read()

# 父类必须是 android.app.Application（vivo 底包无 android:name、也无 androidx/multidex ⇒ 只能空壳挂载）
assert t.count(".super Landroid/app/Application;") == 1, "killer 父类不是 android.app.Application（源件已被改过？）"
# 包名：模板件里是 com.wanda.sf3.wd（暗影格斗3 的旧适配值），逐字换成目标包名
OLD_PKG = 'const-string v1, "com.wanda.sf3.wd"'
assert t.count(OLD_PKG) == 1, "包名锚点未命中（源件包名不是 com.wanda.sf3.wd）"
t = t.replace(OLD_PKG, 'const-string v1, "%s"' % pkg, 1)
# 证书：<clinit> 里以 MIID/MIIC 开头那条整串替换成官方 DER base64（76 字符折行）
i = t.find('const-string v0, "MI')
assert i >= 0, "证书锚点未命中"
j = t.find('"', i + len('const-string v0, '))
k = t.find('"', j + 1)
t = t[:i] + 'const-string v0, "%s"' % wrapped + t[k+1:]
io.open(p, "w", encoding="utf-8", newline="\n").write(t)
print("  killer: 父类=Application、包名=%s、证书 base64 %d 字符（DER %d B）" % (pkg, len(b64), len(der)))
PY
cat > "$WORK/killer_smali/bin/mt/signature/SgKillerApp.smali" <<'SMALI'
.class public Lbin/mt/signature/SgKillerApp;
.super Lbin/mt/signature/KillerApplication193;

.method public constructor <init>()V
    .registers 1
    invoke-direct {p0}, Lbin/mt/signature/KillerApplication193;-><init>()V
    return-void
.end method
SMALI
"$SMALI_BIN" assemble -o "$WORK/$KILLER_DEX" "$WORK/killer_smali"
rm -rf "$WORK/c6_verify"
"$BAKSMALI_BIN" d "$WORK/$KILLER_DEX" -o "$WORK/c6_verify" >/dev/null
grep -q '^\.super Landroid/app/Application;' "$WORK/c6_verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 父类未改成 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$WORK/c6_verify/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: killer 包名未改" >&2; exit 1; }
grep -q '^\.super Lbin/mt/signature/KillerApplication193;' "$WORK/c6_verify/bin/mt/signature/SgKillerApp.smali" \
  || { echo "ERROR: SgKillerApp 挂载类缺失" >&2; exit 1; }
echo "  $KILLER_DEX = $(stat -f '%z' "$WORK/$KILLER_DEX") B ✓"

# ------------------------------------------------------------- 5/8 manifest
echo "[5/8] binary AndroidManifest（+provider +service +权限 +application android:name）"
if [[ -n "${NHSY2_BINARY_MANIFEST:-}" && -f "${NHSY2_BINARY_MANIFEST:-}" ]]; then
  cp "$NHSY2_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"; echo "  using $NHSY2_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"; echo "  using cached $MF_CACHE"
else
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" "$KILLER_APP" <<'PY'
import sys
from pathlib import Path
p, pkg, auth, killer = Path(sys.argv[1]), sys.argv[2], sys.argv[3], sys.argv[4]
t = p.read_text(encoding="utf-8")

perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# B 轨挂载点：<application android:name="bin.mt.signature.SgKillerApp"（官包无 android:name）
if 'android:name="%s"' % killer not in t:
    anchor = "<application "
    assert t.count(anchor) == 1, "application 锚点异常"
    t = t.replace(anchor, '<application android:name="%s" ' % killer, 1)

if "com.starry.magic.StarryInitProvider" not in t:
    prov = f'''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="{auth}"
            android:enabled="true"
            android:exported="false"
            android:initOrder="2147483647" />
'''
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
                android:value="MOD menu overlay service" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)

p.write_text(t, encoding="utf-8")
print("  text Manifest patched（permissions + android:name + provider + service；保留 appComponentFactory）")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
echo "  AndroidManifest.xml = $(stat -f '%z' "$WORK/AndroidManifest.xml") B"

# --------------------------------------------- 5b 回读校验（节点 + 无删除）
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for k in "com.starry.magic.StarryInitProvider" "com.starry.magic.menu.MenuService" \
           "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" "android.permission.SYSTEM_ALERT_WINDOW" \
           "androidx.core.app.CoreComponentFactory" "bin.mt.signature.SgKillerApp"; do
    grep -q "$k" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $k" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_N="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true)"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 重建丢失官方节点（$DIFF_D 行）" >&2; exit 1; }
  [[ "$DIFF_N" -ge 15 && "$DIFF_N" -le 40 ]] \
    || { echo "ERROR: manifest 新增节点数异常（$DIFF_N 行）" >&2; exit 1; }
  echo "  manifest ok（新增 $DIFF_N 行、删除 0 行，appComponentFactory + android:name 均在）✓"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

# ---------------------------------------------------------------- 6/8 merge
echo "[6/8] zip surgical merge → $UNSIGNED"
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
STL_SO="${NHSY2_STL_SO:-$STARRY_SRC/libs_nhsy2/arm64-v8a/libc++_shared.so}"
if unzip -l "$ORIG_APK" | grep -q "lib/arm64-v8a/libc++_shared.so"; then
  echo "  官方包自带 libc++_shared.so ⇒ 使用官方那份（不覆盖）"
elif [[ -f "$STL_SO" ]]; then
  cp "$STL_SO" "$WORK/lib/arm64-v8a/libc++_shared.so"
  echo "  官方包无 libc++_shared.so ⇒ 自带一份（$(stat -f '%z' "$STL_SO") B）"
else
  echo "ERROR: 官方包无 libc++_shared.so，且找不到 ${STL_SO}（mod so 的 DT_NEEDED 依赖它）" >&2
  exit 1
fi
if [[ -d "$WORK/assets_certs_from_apk" ]]; then
  mkdir -p "$WORK/assets/certs"; cp -R "$WORK/assets_certs_from_apk/." "$WORK/assets/certs/"
fi
if [[ ! -f "$WORK/assets/certs/starlight.crt" ]]; then
  SRC_CRT="$STARRY_SRC/app/src/main/assets/certs/starlight.crt"
  [[ -f "$SRC_CRT" ]] || { echo "ERROR: 缺 assets/certs/starlight.crt（${SRC_CRT} ）" >&2; exit 1; }
  mkdir -p "$WORK/assets/certs"; cp -f "$SRC_CRT" "$WORK/assets/certs/starlight.crt"
  echo "  starlight.crt 取自源码树"
fi

python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$STARRY_DEX" "$KILLER_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, starry_dex, killer_dex = (
    Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4], sys.argv[5])
mf  = (work / "AndroidManifest.xml").read_bytes()
c3  = (work / "classes3.dex").read_bytes()
c5  = (work / starry_dex).read_bytes()
c6  = (work / killer_dex).read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

untouched = ["classes.dex", "classes2.dex", "classes4.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so"]
_zin_probe = zipfile.ZipFile(src, "r")
if "lib/arm64-v8a/libc++_shared.so" in _zin_probe.namelist():
    untouched.append("lib/arm64-v8a/libc++_shared.so")
_zin_probe.close()

def is_sig(name):
    if not name.startswith("META-INF/"):
        return False
    if name == "META-INF/MANIFEST.MF":
        return True
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in ["AndroidManifest.xml", "classes3.dex", "classes4.dex"] + untouched:
    assert n in names, "orig apk missing %s" % n
assert starry_dex not in names, "orig apk already has %s" % starry_dex
assert killer_dex not in names, "orig apk already has %s" % killer_dex

zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name):
        continue
    if name in extras:
        continue
    if name == "AndroidManifest.xml":
        data, ctype = mf, zipfile.ZIP_DEFLATED
    elif name == "classes3.dex":
        data, ctype = c3, zipfile.ZIP_DEFLATED
    else:
        data, ctype = zin.read(item), item.compress_type
        if name == "resources.arsc":
            ctype = zipfile.ZIP_STORED
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ctype
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

for name, data in [(starry_dex, c5), (killer_dex, c6)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, "changed: %s" % name
for name in ("META-INF/services/java.security.Provider",
             "META-INF/services/kotlinx.coroutines.CoroutineExceptionHandler",
             "META-INF/services/kotlinx.coroutines.internal.MainDispatcherFactory"):
    if name in zin2.namelist():
        assert name in zout2.namelist(), "lost %s" % name
for name in (starry_dex, killer_dex, "classes3.dex", "lib/arm64-v8a/libstarry_magic.so",
             "lib/arm64-v8a/libc++_shared.so", "assets/certs/starlight.crt"):
    assert name in zout2.namelist(), "missing %s" % name
assert zout2.read("classes3.dex") == c3
assert zout2.read(starry_dex) == c5
assert zout2.read(killer_dex) == c6
assert zout2.read("AndroidManifest.xml") == mf
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in zout2.namelist()), "signature leftovers"
assert "META-INF/MANIFEST.MF" not in zout2.namelist()
print("  merged ok; entries≈%d; classes3=%d; %s=%d; %s=%d; untouched %d/%d verified"
      % (n, len(c3), starry_dex, len(c5), killer_dex, len(c6), len(untouched), len(untouched)))
zin2.close(); zout2.close()
PY

# ------------------------------------------------------------- 7/8 闭环校验
echo "[7/8] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$IN_SO_MD5" == "$SO_MD5" ]] \
  || { echo "ERROR: 包内 so($IN_SO_MD5) != 构建产物($SO_MD5)——旧 so 混包" >&2; exit 1; }
echo "  [so] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

rm -rf "$WORK/v5" "$WORK/v3" "$WORK/v6"
unzip -p "$UNSIGNED" "$STARRY_DEX" > "$WORK/chk5.dex"
"$BAKSMALI_BIN" d "$WORK/chk5.dex" -o "$WORK/v5" >/dev/null
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali; do
  [[ -f "$WORK/v5/$f" ]] || { echo "ERROR: $STARRY_DEX 缺 $f" >&2; exit 1; }
done
echo "  [$STARRY_DEX] StarryInitProvider / MenuService ✓"

unzip -p "$UNSIGNED" classes3.dex > "$WORK/chk3.dex"
"$BAKSMALI_BIN" d "$WORK/chk3.dex" -o "$WORK/v3" >/dev/null
# ★门禁串不能是 smali `#` 注释（汇编器整行丢弃 ⇒ 假阴性，2026-10-30 r33 实测栽过一次）
#   ⇒ 直接看方法体：必须只有 `const/4 p0, 0x0` + `return p0`，且不得有 invoke/iget 等真实逻辑
python3 - "$WORK/v3/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali" <<'PYCHK'
import re, sys
t = open(sys.argv[1], encoding="utf-8").read()
m = re.search(r'(?ms)^\.method [^\n]*isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z\n.*?^\.end method$', t)
assert m, "回读找不到 isClientLogin 方法"
body = m.group(0)
assert "const/4 p0, 0x0" in body, "isClientLogin 方法体缺 const/4 p0, 0x0"
assert "return p0" in body, "isClientLogin 方法体缺 return p0"
for bad in ("invoke-", "iget", "sget", "iput"):
    assert bad not in body, "isClientLogin 方法体仍有真实逻辑: %s" % bad
print("  [classes3] isClientLogin 方法体＝const/4 p0,0x0 + return p0（无真实逻辑）✓")
PYCHK
if grep -rq 'starry_sign_bypass' "$WORK/v3"; then
  echo "ERROR: classes3.dex 仍带 TapSignKit 回灌补丁（双轨双修）" >&2; exit 1
fi
for f in com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali; do
  grep -q 'static constructor <clinit>()V' "$WORK/v3/$f" \
    || { echo "ERROR: $f 的 <clinit> 丢失（INSTANCE 不会赋值）" >&2; exit 1; }
  grep -q 'private constructor <init>()V' "$WORK/v3/$f" \
    || { echo "ERROR: $f 的私有 <init> 丢失" >&2; exit 1; }
done
echo "  [classes3] isClientLogin 恒 false；无 starry_sign_bypass；<clinit>/<init> 完好 ✓"

unzip -p "$UNSIGNED" "$KILLER_DEX" > "$WORK/chk6.dex"
"$BAKSMALI_BIN" d "$WORK/chk6.dex" -o "$WORK/v6" >/dev/null
grep -q '^\.super Landroid/app/Application;' "$WORK/v6/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: 包内 killer 父类不是 Application" >&2; exit 1; }
grep -q "const-string v1, \"$PKG\"" "$WORK/v6/bin/mt/signature/KillerApplication193.smali" \
  || { echo "ERROR: 包内 killer 包名不对" >&2; exit 1; }
grep -q '^\.super Lbin/mt/signature/KillerApplication193;' "$WORK/v6/bin/mt/signature/SgKillerApp.smali" \
  || { echo "ERROR: 包内缺 SgKillerApp" >&2; exit 1; }
echo "  [$KILLER_DEX] killer(Application/$PKG/官方证书) + SgKillerApp ✓"

# 官方 dex 未被改动（除 classes3 与新增 5/6）
for d in classes.dex classes2.dex classes4.dex; do
  A="$(unzip -p "$ORIG_APK" "$d" | md5 -q)"; B="$(unzip -p "$UNSIGNED" "$d" | md5 -q)"
  [[ "$A" == "$B" ]] || { echo "ERROR: $d 被改动（$A -> ${B} ）" >&2; exit 1; }
done
echo "  [dex] classes/classes2/classes4 与官包逐字节一致 ✓"

echo
ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NEXT: bash 04_scripts/霓虹深渊2/nhsy2_sign_2026.10.30.sh"
