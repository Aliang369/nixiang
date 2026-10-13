#!/usr/bin/env bash
# 霓虹深渊2 (com.veewo.neonabyss2) TapTap 渠道 — 外科组装
#
# 官包零改动基线：classes.dex / classes2.dex / classes4.dex / resources.arsc /
# lib/arm64-v8a/**.so（含 libil2cpp.so / libunity.so / libc++_shared.so）字节不变。
#
# 改动：
#   - AndroidManifest.xml：+StarryInitProvider(initOrder=MAX) +MenuService +3 条权限；
#     保留原 appComponentFactory="androidx.core.app.CoreComponentFactory"（官包无 android:name）。
#   - classes3.dex：**就地替换**（本游戏关键类全在 c3）——
#       com/taptap/sdk/login/impl/utils/ClientLoginUtils.isClientLogin  → 恒 false
#         （TapSDK 新版三分支路由 LoginTypes.from(isClientLogin)；false ⇒ 回落 WebLoginFragment）
#       com/taptap/sdk/kit/internal/utils/TapSignKit.getSign            → 恒返回官方 cert MD5
#         （bbe1da8584fb2fb1017a512b2e99ff21 = O=Veewo 签名证书 MD5(raw)）
#     两处均为**方法体替换**，不新增 method id ⇒ c3 的 65,304/65,536 不会被顶爆。
#   - classes5.dex：新增（星光 smali：StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb / OTA）。
#     ⚠️ 本游戏已有 classes.dex..classes4.dex，故星光 dex 落位 **classes5.dex**。
#   - lib/arm64-v8a/libstarry_magic.so：新增（本游戏已自带 libc++_shared.so 且导出符号全覆盖
#     本 so 的全部 _Z 未定义符号 ⇒ **不替换游戏 libc++_shared.so**，避免打断 libil2cpp 链接）。
#   - assets/certs/starlight.crt：新增。
#   - 保留官方 META-INF/services/**（ServiceLoader 清单），剔除 META-INF/ANDROID-.SF/.RSA/MANIFEST.MF。
#
# 功能全在 native（Games/nhsy2Mod.cpp）：
#   战斗＝伤害倍率 / 无敌 / 全屏攻击 / 移速倍率 / 无限跳；资源＝解锁标准版 / 无限手雷 /
#   无限金币 / 无限水晶 / 无限钥匙 / 无限彩色钥匙（一功能一开关 1:1；共 13 槽 = 2 分类 + 11 功能）。
# 不改任何官方 dex 字节以外的逻辑。
#
# 产物：08_output/霓虹深渊2/霓虹深渊2_<版本>_surgical-unsigned.apk（未签名）
#
# 用法:
#   bash scripts/surgical_nhsy2_2026.10.12.sh
# 可覆盖环境变量:
#   NHSY2_ORIG_APK   底包路径（默认 02_apks/霓虹深渊2/霓虹深渊2_taptap_official.apk）
#   NHSY2_SO         预构建 so 路径（默认 <repo>/libs_nhsy2/arm64-v8a/libstarry_magic.so）
#   NHSY2_STARRY_APK 预构建星光 apk 路径（跳过 gradle 时用）
#   NHSY2_SKIP_GRADLE=1 跳过 gradle（要求 NHSY2_STARRY_APK 或已有 app-release.apk）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"
GAME="霓虹深渊2"
VERSION="2026.10.12.3c8f88d4990"
VER_SHORT="2026.10.12"
PKG="com.veewo.neonabyss2"
ORIG_APK="${NHSY2_ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_taptap_official.apk}"
ORIG_MD5="8d47959d27c696e9404770fc95e286b0"
OFFICIAL_CERT_MD5="bbe1da8584fb2fb1017a512b2e99ff21"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VER_SHORT}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical_${VER_SHORT}/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
AUTHORITY="$PKG.starryinit"
STARRY_DEX="classes5.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

# 底包解析（★指纹优先）：显式路径 → 02_apks → /tmp 已知暂存 → /tmp 扫描 → 真机 adb 拉取
#   ★ 2026-10-11 复盘：旧版「路径存在即采用」把 /tmp/neonabyss2_base/base.apk（10.5 旧底包）
#     当成底包，直接报「官包指纹不符」让整轮打包白跑。现在**先比 md5 再采用**：指纹不符的
#     候选只打印「跳过」，绝不会被静默选中。
resolve_orig() {
  local cand=() f
  [[ -n "${NHSY2_ORIG_APK:-}" ]] && cand+=("$NHSY2_ORIG_APK")
  cand+=("$ORIG_APK")
  cand+=("/tmp/nhsy2/${GAME}_base_${VERSION}.apk" "/tmp/nhsy2/nhsy2_base_${VERSION}.apk")
  for f in /tmp/nhsy2/*.apk /tmp/neonabyss2_base/*.apk; do [[ -f "$f" ]] && cand+=("$f"); done
  for f in "${cand[@]}"; do
    [[ -f "$f" ]] || continue
    if [[ "$(md5 -q "$f" 2>/dev/null)" == "$ORIG_MD5" ]]; then echo "$f"; return 0; fi
    echo "  跳过候选（底包指纹不符）: $f" >&2
  done
  local adb="$HOME/Library/Android/sdk/platform-tools/adb"
  [[ -x "$adb" ]] || return 1
  local p
  p="$("$adb" shell pm path "$PKG" 2>/dev/null | tr -d '\r' | sed -n 's|^package:||p' | head -1)"
  [[ -n "$p" ]] || return 1
  mkdir -p /tmp/neonabyss2_base
  echo "  pulling 真机底包 $p ..." >&2
  "$adb" pull "$p" /tmp/neonabyss2_base/base.apk >&2 || return 1
  echo /tmp/neonabyss2_base/base.apk
}
ORIG_APK="$(resolve_orig)" || { echo "ERROR: 找不到底包，且 adb 拉取失败（设 NHSY2_ORIG_APK=）" >&2; exit 1; }
echo "  orig apk = $ORIG_APK"
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }

echo "[check] 底包指纹"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 got=$ACTUAL_MD5 want=$ORIG_MD5" >&2; exit 1; }
CERT_MD5=$(/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner verify --print-certs "$ORIG_APK" \
  | awk -F': ' '/MD5 digest/ {print $2}' | tr -d ' ')
[[ "$CERT_MD5" == "$OFFICIAL_CERT_MD5" ]] \
  || { echo "ERROR: 官包证书 MD5 不符 got=$CERT_MD5 want=$OFFICIAL_CERT_MD5" >&2; exit 1; }
echo "  底包 md5=$ACTUAL_MD5 cert_md5=$CERT_MD5 ✓"

LOCK_DIR="/tmp/nhsy2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held (pid $(cat "$LOCK_DIR/pid" 2>/dev/null))" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/nhsy2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/7 native so
echo "[1/7] native so（Games/nhsy2Mod.cpp → libstarry_magic.so）"
SO_PREBUILT="${NHSY2_SO:-$STARRY_SRC/libs_nhsy2/arm64-v8a/libstarry_magic.so}"
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
echo "  so ok: $(stat -f '%z' "$SO_PREBUILT") B"

# ------------------------------------------------------------- 2/7 starry apk
echo "[2/7] starry apk（Java 桥 → smali + certs）"
if [[ -n "${NHSY2_STARRY_APK:-}" && -f "${NHSY2_STARRY_APK:-}" ]]; then
  STARRY_APK="$NHSY2_STARRY_APK"
  echo "  using $STARRY_APK"
elif [[ "${NHSY2_SKIP_GRADLE:-0}" == "1" && -f "$STARRY_SRC/app/build/outputs/apk/release/app-release.apk" ]]; then
  STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
  echo "  using $STARRY_APK (skip gradle)"
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

# ------------------------------------------------------- 3/7 classes5.dex (星光)
echo "[3/7] baksmali starry → com/starry smali → $STARRY_DEX"
mkdir -p "$WORK/starry_apk" && ( cd "$WORK/starry_apk" && unzip -o -q "$STARRY_APK" )
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT/com" "$SMALI_ROOT/dev"
shopt -s nullglob
for dx in "$WORK/starry_apk"/classes*.dex; do
  bd="$WORK/starry_dec_$(basename "$dx" .dex)"
  "$BAKSMALI_BIN" d "$dx" -o "$bd" >/dev/null
  [[ -d "$bd/com/starry" ]] && rsync -a "$bd/com/starry" "$SMALI_ROOT/com/"
  [[ -d "$bd/dev" ]] && rsync -a "$bd/dev" "$SMALI_ROOT/"
done
shopt -u nullglob
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali \
         com/starry/magic/sign/SignSpoof.smali com/starry/magic/sign/TapLoginForceWeb.smali; do
  [[ -f "$SMALI_ROOT/$f" ]] || { echo "ERROR: 星光 smali 缺 $f" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/$STARRY_DEX" "$SMALI_ROOT"
echo "  $STARRY_DEX = $(stat -f '%z' "$WORK/$STARRY_DEX") B"

# ------------------------------------- 4/7 classes3.dex（强制网页登录 + 签名回灌）
echo "[4/7] baksmali 官包 classes3.dex → smali 层补丁 → classes3.dex"
( cd "$WORK/starry_apk" && unzip -o -q "$ORIG_APK" classes3.dex )
"$BAKSMALI_BIN" d "$WORK/starry_apk/classes3.dex" -o "$WORK/c3_smali" >/dev/null
python3 - "$WORK/c3_smali" "$OFFICIAL_CERT_MD5" <<'PY'
import re, sys
from pathlib import Path

root = Path(sys.argv[1]); cert = sys.argv[2]

def all_method_headers():
    hdr = {}
    for f in root.rglob("*.smali"):
        hs = [l.strip() for l in f.read_text(encoding="utf-8").splitlines() if l.startswith(".method ")]
        if hs:
            hdr[str(f.relative_to(root))] = hs
    return hdr

BEFORE = all_method_headers()

def replace_method(path: Path, sig: str, new_body: str, marker: str):
    t = path.read_text(encoding="utf-8")
    if marker in t:
        print(f"  already patched: {path.name}")
        return
    # ⚠️ 必须用 [^\n]* 限定方法头在同一行：写成 `^\.method .*` + re.DOTALL 会让 `.*`
    # 跨行贪婪吞掉本方法**之前**的 <clinit>/<init>（实测 classes3 少了 6 个 method id，
    # INSTANCE 静态字段不再赋值 ⇒ 运行期 NPE）。
    m = re.search(r'(?ms)^\.method [^\n]*' + re.escape(sig) + r'\n.*?^\.end method\n', t)
    if not m:
        raise SystemExit(f"ERROR: method {sig} not found in {path}")
    t = t[:m.start()] + new_body + t[m.end():]
    path.write_text(t, encoding="utf-8")
    print(f"  patched {path.name}::{sig.split('(')[0]}")

# ① 强制网页登录：TapSDK 新版路由 LoginTypes.from(ClientLoginUtils.isClientLogin)
#    恒 false ⇒ 不走 TapTap 客户端登录，回落 WebLoginFragment（内嵌网页）。
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

# ② 签名回灌：TapSignKit.getSign 直读 PackageManager.signatures → CommonUtils.getMD5。
#    改包后真签为星光证书，TapSDK 会判「包名/签名未在开发者中心配置」⇒ 恒返回官方证书 MD5。
replace_method(
    root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali",
    "getSign(Landroid/content/Context;)Ljava/lang/String;",
    f"""\
.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 4

    # starry_sign_bypass
    const-string p0, "{cert}"

    return-object p0
.end method
""",
    "starry_sign_bypass",
)

# 只允许替换这 2 个方法体：任何 .method 头丢失/新增都立即失败
# （smali 汇编器不会做死代码消除，头集合必须逐字相等）
AFTER = all_method_headers()
assert set(AFTER) == set(BEFORE), "smali 文件集合变化"
for rel, hs in BEFORE.items():
    if AFTER[rel] != hs:
        raise SystemExit(f"ERROR: {rel} 的 .method 头发生变化（替换越界）\n old={hs}\n new={AFTER[rel]}")
print(f"  method 头集合逐字一致（{sum(len(v) for v in BEFORE.values())} 个方法，仅 2 处方法体替换）✓")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3_smali"

# dex 头 method_ids_size 对比（只允许方法体替换，不允许新增 method id）
python3 - "$WORK/starry_apk/classes3.dex" "$WORK/classes3.dex" <<'PY'
import struct, sys
def hdr(p):
    b = open(p, 'rb').read(0x70)
    return dict(zip(('strings','types','protos','fields','methods'),
                    struct.unpack_from('<5I', b, 0x38)[::2])) if False else {
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
        raise SystemExit(f"ERROR: {k} 增长异常 {a[k]} -> {b[k]}")
assert b['methods'] < 65536, f"ERROR: methods {b['methods']} 超过 65536"
# method_ids 含**外部引用**：替换方法体后不再引用 Intent.resolveActivity /
# CommonUtils.getMD5 等，汇编器会丢掉这些未引用的外部 method id（正常、无害）。
# 定义的方法头集合已在上一步逐字比对 ⇒ 这里只允许「只减不增」且降幅极小。
delta = a['methods'] - b['methods']
assert 0 <= delta <= 16, \
    f"ERROR: method_ids 异常 {a['methods']} -> {b['methods']}（只允许小幅下降）"
print(f"  method_ids {a['methods']} -> {b['methods']}（-{delta} 外部引用，上限 65536）✓")
PY

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
if [[ -d "$WORK/starry_apk/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_apk/assets/certs/." "$WORK/assets/certs/"
fi

# ------------------------------------------------------------------ 5/7 manifest
echo "[5/7] binary AndroidManifest（保留原 appComponentFactory；+provider +service +权限）"
if [[ -n "${NHSY2_BINARY_MANIFEST:-}" && -f "${NHSY2_BINARY_MANIFEST:-}" ]]; then
  cp "$NHSY2_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"; echo "  using $NHSY2_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"; echo "  using cached $MF_CACHE"
else
  # 只解 manifest + resources（--no-assets 跳掉 1.37 GB 的 assets/，别整包 apktool d）
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
import sys
from pathlib import Path
p, pkg, auth = Path(sys.argv[1]), sys.argv[2], sys.argv[3]
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

# 官包 <application> 无 android:name、appComponentFactory="androidx.core.app.CoreComponentFactory"：
# 不替换 Application（Unity/il2cpp 与 TapSDK provider 都对 Application 有假设），走
# StarryInitProvider（ContentProvider.onCreate 早于 Activity.onCreate）。
# initOrder=MAX：声明位置留在最后，但系统最先初始化 ⇒ SignSpoof.install() 早于
# TapTapKitInitProvider / ThemisProvider，否则 TapSDK 会打「包名、签名未在 TapTap 开发者中心正确配置」。
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
print("  text Manifest patched (provider 注入, 保留原 Application / appComponentFactory)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
echo "  AndroidManifest.xml = $(stat -f '%z' "$WORK/AndroidManifest.xml") B"

# --------------------------------------------------- 5b 回读校验（节点 + 无删除）
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
           "androidx.core.app.CoreComponentFactory"; do
    grep -q "$k" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $k" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_N="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true)"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 重建丢失官方节点（$DIFF_D 行）" >&2; exit 1; }
  [[ "$DIFF_N" -ge 15 && "$DIFF_N" -le 30 ]] \
    || { echo "ERROR: manifest 新增节点数异常（$DIFF_N 行）" >&2; exit 1; }
  echo "  manifest ok（新增 $DIFF_N 行，删除 0 行，appComponentFactory 保留）✓"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

# ---------------------------------------------------------------- 6/7 zip merge
echo "[6/7] zip surgical merge → $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$STARRY_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, starry_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf  = (work / "AndroidManifest.xml").read_bytes()
c3  = (work / "classes3.dex").read_bytes()
c5  = (work / starry_dex).read_bytes()

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
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libc++_shared.so"]

def is_sig(name: str) -> bool:
    if not name.startswith("META-INF/"):
        return False
    if name == "META-INF/MANIFEST.MF":
        return True
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in ["AndroidManifest.xml", "classes3.dex", "classes4.dex"] + untouched:
    assert n in names, f"orig apk missing {n}"
assert starry_dex not in names, f"orig apk already has {starry_dex}"

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

for name, data in [(starry_dex, c5)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

# 闭环：未触碰条目逐字节一致；META-INF/services 保留；无签名残留
zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
for name in ("META-INF/services/java.security.Provider",
             "META-INF/services/kotlinx.coroutines.CoroutineExceptionHandler",
             "META-INF/services/kotlinx.coroutines.internal.MainDispatcherFactory"):
    if name in zin2.namelist():
        assert name in zout2.namelist(), f"lost {name}"
for name in (starry_dex, "classes3.dex", "lib/arm64-v8a/libstarry_magic.so", "assets/certs/starlight.crt"):
    assert name in zout2.namelist(), f"missing {name}"
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist()), "signature leftovers"
assert "META-INF/MANIFEST.MF" not in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes3={len(c3)}; {starry_dex}={len(c5)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

# ------------------------------------------------------------- 7/7 闭环校验
echo "[7/7] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] \
  || { echo "ERROR: 包内 so($IN_SO_MD5) != 构建产物($SRC_SO_MD5)——旧 so 混包" >&2; exit 1; }
echo "  [so] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

rm -rf "$WORK/v5" "$WORK/v3"
unzip -p "$UNSIGNED" "$STARRY_DEX" > "$WORK/chk5.dex"
"$BAKSMALI_BIN" d "$WORK/chk5.dex" -o "$WORK/v5" >/dev/null
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali \
         com/starry/magic/sign/SignSpoof.smali com/starry/magic/sign/TapLoginForceWeb.smali; do
  [[ -f "$WORK/v5/$f" ]] || { echo "ERROR: $STARRY_DEX 缺 $f" >&2; exit 1; }
done
echo "  [$STARRY_DEX] StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb ✓"

unzip -p "$UNSIGNED" classes3.dex > "$WORK/chk3.dex"
"$BAKSMALI_BIN" d "$WORK/chk3.dex" -o "$WORK/v3" >/dev/null
grep -q 'const/4 p0, 0x0' "$WORK/v3/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali" \
  || { echo "ERROR: classes3.dex 的 isClientLogin 未改成恒 false" >&2; exit 1; }
grep -q "$OFFICIAL_CERT_MD5" "$WORK/v3/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" \
  || { echo "ERROR: classes3.dex 的 TapSignKit.getSign 未回灌官方签名" >&2; exit 1; }
# 关键：INSTANCE 静态字段的 <clinit> 与私有 <init> 必须仍在（曾被贪婪正则误删 ⇒ 运行期 NPE）
for f in com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali \
         com/taptap/sdk/kit/internal/utils/TapSignKit.smali; do
  grep -q 'static constructor <clinit>()V' "$WORK/v3/$f" \
    || { echo "ERROR: $f 的 <clinit> 丢失（INSTANCE 不会赋值）" >&2; exit 1; }
  grep -q 'private constructor <init>()V' "$WORK/v3/$f" \
    || { echo "ERROR: $f 的私有 <init> 丢失" >&2; exit 1; }
done
echo "  [classes3] isClientLogin 恒 false + TapSignKit.getSign 回灌 $OFFICIAL_CERT_MD5 + <clinit>/<init> 完好 ✓"

ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NEXT: zipalign -f -p 4 → apksigner sign --ks 01_tools/starry_release.jks --ks-key-alias starry"
