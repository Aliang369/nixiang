#!/usr/bin/env bash
# 邂逅在迷宫 (cn.alltune.endless) TapTap 渠道 2.6.53 — 外科组装
#
# 官包零改动基线：classes.dex / resources.arsc / lib/arm64-v8a/**.so（含 libil2cpp.so /
# libtuanjie.so / libunity.so / libFairGuard.so）字节不变。
#
# 本作引擎：Unity 团结引擎 Tuanjie + IL2CPP（无 HybridCLR；热更逻辑在
#   files/Bundles/endlessstrikedll_*.bundle 的 EndlessStrike.dll 里，见 07_patched/邂逅在迷宫/）
# 加固：libFairGuard.so（FairGuard）；无 ACE、无 libtprt。
#
# 改动：
#   - AndroidManifest.xml：+StarryInitProvider(initOrder=MAX) +MenuService +3 条权限；
#     保留原 android:name="com.hotune.esgame.GameApplication" 与
#     appComponentFactory="androidx.core.app.CoreComponentFactory"（绝不替换 Application）。
#   - classes2.dex：**就地替换**（TapSDK 3.17.0 的 ui.* 旧版实现全在 c2）——
#       com/taptap/sdk/ui/ActivityHandler.authorize(LoginRequest)V → 恒抛
#       ActivityNotFoundException("starry_force_web_login")
#       （LoginClient.sendLoginRequest 的 try/catch 会接住 ⇒ 回落 WebViewHandler 内嵌网页登录）
#     方法体替换，不新增 method id ⇒ c2 的 63,901/65,536 不会被顶爆。
#   - classes3.dex：新增（星光 smali：StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb）。
#     ⚠️ 本游戏只有 classes.dex + classes2.dex，故星光 dex 落位 **classes3.dex**。
#   - lib/arm64-v8a/libstarry_magic.so：新增（本游戏无同名 so）。
#   - lib/arm64-v8a/libc++_shared.so：新增 —— ⚠️ 本游戏**不自带** libc++_shared.so，
#     而 libstarry_magic.so 的 DT_NEEDED 含它（游戏自带 so 全部只用 libstdc++.so，
#     加 libc++ 不冲突），缺它则 dlopen 失败、菜单不出现。
#   - assets/certs/starlight.crt：新增。
#   - 保留官方 META-INF/services/**（ServiceLoader 清单），剔除 META-INF/*.SF/.RSA/MANIFEST.MF。
#
# 功能全在 IL 侧（07_patched/邂逅在迷宫/EndlessStrike.patched.dll → 重打包 bundle）：
#   无敌 / 伤害倍率 / 无限技能。native 侧（Games/XhzmMod.cpp）已最小化为 0 个 native hook，
#   只负责「lib 已映射 → 菜单可用」；开关经 starry_state.txt 镜像给 IL 侧读。
#
# 产物：08_output/邂逅在迷宫/邂逅在迷宫_2.6.53_surgical-unsigned.apk（未签名）
#
# 用法:
#   bash scripts/surgical_xhzm_2.6.53.sh
#   STARRY_SIGN=1 STARRY_INSTALL=1 bash scripts/surgical_xhzm_2.6.53.sh
# 可覆盖环境变量:
#   XHZM_ORIG_APK     底包路径（默认 02_apks/邂逅在迷宫/邂逅在迷宫_2.6.53_taptap_official.apk）
#   XHZM_SO           预构建 so 路径（默认 <repo>/libs_xhzm/arm64-v8a/libstarry_magic.so）
#   XHZM_STARRY_APK   预构建星光 apk 路径（跳过 gradle 时用）
#   XHZM_SKIP_GRADLE=1 跳过 gradle（要求 XHZM_STARRY_APK 或已有 app-release.apk）
#   XHZM_BINARY_MANIFEST 直接给已注入好的二进制 manifest（跳过 apktool）
#   STARRY_SIGN=1     出包后 zipalign + apksigner 签名
#   STARRY_INSTALL=1  签名后 pm install（改签需先卸载官包，本脚本不自动卸载）
#   XHZM_PUSH_BUNDLE=1 顺带把 07_patched 的补丁 bundle 推到真机 files/Bundles
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"
GAME="邂逅在迷宫"
VERSION="2.6.53"
VER_SHORT="2.6.53"
PKG="cn.alltune.endless"
ORIG_APK="${XHZM_ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VER_SHORT}_taptap_official.apk}"
ORIG_MD5="bff15a35dc5fc6cd692b7b185b35c0e9"
# MOERUSH 签名证书 MD5(raw) —— O=Hangzhou Hotune Co Ltd / CN=MoeRush / serial 69F59CB0
OFFICIAL_CERT_MD5="d3f53f43313dc5c5e9a8af50d3ce3b67"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VER_SHORT}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VER_SHORT}_starry-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical_${VER_SHORT}/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
ADB="${ADB:-$HOME/Library/Android/sdk/platform-tools/adb}"
DEV_SERIAL="${DEV_SERIAL:-f9a116a2}"
AUTHORITY="$PKG.starryinit"
STARRY_DEX="classes3.dex"
PATCH_DEX="classes2.dex"
LIBCXX_SRC="$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libc++_shared.so"
# 补丁后的热更 bundle（IL 侧三功能：无敌 / 伤害倍率 / 无限技能）
PATCHED_BUNDLE="$NIXIANG/07_patched/$GAME/bundle/endlessstrikedll_8fa83621b75bca5af7b14bf2f4a86163.bundle"
REMOTE_BUNDLE_DIR="/storage/emulated/0/Android/data/$PKG/files/Bundles"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

# 底包解析：显式路径 → 02_apks → /tmp 暂存 → 真机 adb 拉取（工作区不留 APK 的既定策略）
resolve_orig() {
  if [[ -n "${XHZM_ORIG_APK:-}" && -f "${XHZM_ORIG_APK:-}" ]]; then echo "$XHZM_ORIG_APK"; return 0; fi
  if [[ -f "$ORIG_APK" ]]; then echo "$ORIG_APK"; return 0; fi
  if [[ -f /tmp/maze_recon/base.apk ]]; then echo /tmp/maze_recon/base.apk; return 0; fi
  if [[ -f /tmp/xhzm_base/base.apk ]]; then echo /tmp/xhzm_base/base.apk; return 0; fi
  [[ -x "$ADB" ]] || return 1
  local p
  p="$("$ADB" -s "$DEV_SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' | sed -n 's|^package:||p' | head -1)"
  [[ -n "$p" ]] || return 1
  mkdir -p /tmp/xhzm_base
  echo "  pulling 真机底包 $p ..." >&2
  "$ADB" -s "$DEV_SERIAL" pull "$p" /tmp/xhzm_base/base.apk >&2 || return 1
  echo /tmp/xhzm_base/base.apk
}
ORIG_APK="$(resolve_orig)" || { echo "ERROR: 找不到底包，且 adb 拉取失败（设 XHZM_ORIG_APK=）" >&2; exit 1; }
echo "  orig apk = $ORIG_APK"
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }

echo "[check] 底包指纹"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 got=$ACTUAL_MD5 want=$ORIG_MD5" >&2; exit 1; }
CERT_MD5=$("$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner" verify --print-certs "$ORIG_APK" \
  | awk -F': ' '/MD5 digest/ {print $2}' | tr -d ' ')
[[ "$CERT_MD5" == "$OFFICIAL_CERT_MD5" ]] \
  || { echo "ERROR: 官包证书 MD5 不符 got=$CERT_MD5 want=$OFFICIAL_CERT_MD5" >&2; exit 1; }
echo "  底包 md5=$ACTUAL_MD5 cert_md5=$CERT_MD5 ✓"

LOCK_DIR="/tmp/xhzm_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held (pid $(cat "$LOCK_DIR/pid" 2>/dev/null))" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/xhzm_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/7 native so
echo "[1/7] native so（Games/XhzmMod.cpp → libstarry_magic.so）"
SO_PREBUILT="${XHZM_SO:-$STARRY_SRC/libs_xhzm/arm64-v8a/libstarry_magic.so}"
if [[ -f "$SO_PREBUILT" ]]; then
  echo "  using $SO_PREBUILT"
else
  ( cd "$STARRY_SRC/app/src/main/jni" && "$NDK/ndk-build" \
      NDK_PROJECT_PATH="$STARRY_SRC/app" \
      APP_BUILD_SCRIPT=./Android_邂逅在迷宫.mk \
      NDK_APPLICATION_MK=./Application.mk \
      NDK_OUT="$STARRY_SRC/obj_邂逅在迷宫" NDK_LIBS_OUT="$STARRY_SRC/libs_邂逅在迷宫" ) >"$WORK/so_build.log" 2>&1 \
    || { tail -40 "$WORK/so_build.log"; echo "ERROR: ndk-build failed" >&2; exit 1; }
  SO_PREBUILT="$STARRY_SRC/libs_邂逅在迷宫/arm64-v8a/libstarry_magic.so"
fi
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing so $SO_PREBUILT" >&2; exit 1; }
LC_ALL=C grep -aq 'xhzm' "$SO_PREBUILT" \
  || { echo "ERROR: so 内无 xhzm 标记（旧 so / 编错 mk）" >&2; exit 1; }
LC_ALL=C grep -aq '邂逅在迷宫' "$SO_PREBUILT" \
  || { echo "ERROR: so 内无「邂逅在迷宫」菜单标签（旧 so / 编错 mk）" >&2; exit 1; }
NM_BIN="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
if [[ -x "$NM_BIN" ]]; then
  if "$NM_BIN" -D --undefined-only "$SO_PREBUILT" | grep -q emutls; then
    echo "ERROR: so 依赖 __emutls_get_address（缺 -fno-emulated-tls），游戏旧 libc++ 会 dlopen 失败" >&2
    exit 1
  fi
fi
# libc++_shared.so 必须存在：本游戏不自带，缺了菜单 so 无法 dlopen
[[ -f "$LIBCXX_SRC" ]] || { echo "ERROR: 缺 libc++_shared.so 源 $LIBCXX_SRC" >&2; exit 1; }
echo "  so ok: $(stat -f '%z' "$SO_PREBUILT") B  + libc++_shared $(stat -f '%z' "$LIBCXX_SRC") B"

# ------------------------------------------------------------- 2/7 starry apk
echo "[2/7] starry apk（Java 桥 → smali + certs）"
if [[ -n "${XHZM_STARRY_APK:-}" && -f "${XHZM_STARRY_APK:-}" ]]; then
  STARRY_APK="$XHZM_STARRY_APK"
  echo "  using $STARRY_APK"
elif [[ "${XHZM_SKIP_GRADLE:-0}" == "1" && -f "$STARRY_SRC/app/build/outputs/apk/release/app-release.apk" ]]; then
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
# 星光 dex 必须已含本作 SignSpoof 条目（否则 TapSDK/SecVerify 读到假签名）
for dx in $(unzip -Z1 "$STARRY_APK" | grep -E '^classes[0-9]*\.dex$'); do
  if unzip -p "$STARRY_APK" "$dx" | LC_ALL=C grep -aq 'cn\.alltune\.endless'; then
    echo "  SignSpoof XHZM 条目 ✓ ($dx)"
  fi
done
unzip -p "$STARRY_APK" classes.dex | LC_ALL=C grep -aq 'cn\.alltune\.endless' \
  || { echo "ERROR: 星光 apk 未含 cn.alltune.endless（SignSpoof 未重编，先跑 gradlew assembleRelease）" >&2; exit 1; }

# ------------------------------------------------------- 3/7 classes3.dex (星光)
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

# ---------------------------------------------------- 4/7 classes2.dex（强制网页登录）
echo "[4/7] baksmali 官包 $PATCH_DEX → smali 层补丁 → $PATCH_DEX"
( cd "$WORK/starry_apk" && unzip -o -q "$ORIG_APK" "$PATCH_DEX" )
"$BAKSMALI_BIN" d "$WORK/starry_apk/$PATCH_DEX" -o "$WORK/c2_smali" >/dev/null
python3 - "$WORK/c2_smali" <<'PY'
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

def replace_method(path: Path, sig: str, new_body: str, marker: str):
    t = path.read_text(encoding="utf-8")
    if marker in t:
        print(f"  already patched: {path.name}")
        return
    # ⚠️ 必须用 [^\n]* 限定方法头在同一行：写成 `^\.method .*` + re.DOTALL 会让 `.*`
    # 跨行贪婪吞掉本方法**之前**的 <clinit>/<init>（实测曾导致 INSTANCE 不再赋值 ⇒ NPE）。
    m = re.search(r'(?ms)^\.method [^\n]*' + re.escape(sig) + r'\n.*?^\.end method\n', t)
    if not m:
        raise SystemExit(f"ERROR: method {sig} not found in {path}")
    t = t[:m.start()] + new_body + t[m.end():]
    path.write_text(t, encoding="utf-8")
    print(f"  patched {path.name}::{sig.split('(')[0]}")

# 强制网页登录：TapSDK 3.17.0（com.taptap.sdk.ui.* 旧版实现）
# LoginClient.sendLoginRequest 结构：
#     try { if (cloudGameHandler.bindCloudGameService()) cloudGameHandler.authorize(req);
#           else activityHandler.authorize(req); }
#     catch (Exception) { authorizationBack(); webViewHandler.authorize(req); }
# ⇒ 让 ActivityHandler.authorize 恒抛 ActivityNotFoundException，
#   被同一个 try 接住后必然落到 webViewHandler（内嵌网页登录）。
replace_method(
    root / "com/taptap/sdk/ui/ActivityHandler.smali",
    "authorize(Lcom/taptap/sdk/LoginRequest;)V",
    """\
.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    # starry_force_web_login —— 禁止跳转 TapTap 客户端 SSO（改包后真签必然不匹配），
    # 抛异常由 LoginClient.sendLoginRequest 的 catch 分支回落 WebViewHandler 内嵌网页登录
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string v1, "starry_force_web_login"

    invoke-direct {v0, v1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method
""",
    "starry_force_web_login",
)

# 只允许替换这 1 个方法体：任何 .method 头丢失/新增都立即失败
AFTER = all_method_headers()
assert set(AFTER) == set(BEFORE), "smali 文件集合变化"
for rel, hs in BEFORE.items():
    if AFTER[rel] != hs:
        raise SystemExit(f"ERROR: {rel} 的 .method 头发生变化（替换越界）\n old={hs}\n new={AFTER[rel]}")
print(f"  method 头集合逐字一致（{sum(len(v) for v in BEFORE.values())} 个方法，仅 1 处方法体替换）✓")
PY
"$SMALI_BIN" assemble -o "$WORK/$PATCH_DEX" "$WORK/c2_smali"

# dex 头 method_ids_size 对比（只允许方法体替换，不允许新增 method id）
python3 - "$WORK/starry_apk/$PATCH_DEX" "$WORK/$PATCH_DEX" <<'PY'
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
print("  c2 原:", a)
print("  c2 新:", b)
for k in ('strings', 'types', 'protos', 'fields', 'methods'):
    if b[k] > a[k] + 8:
        raise SystemExit(f"ERROR: {k} 增长异常 {a[k]} -> {b[k]}")
assert b['methods'] < 65536, f"ERROR: methods {b['methods']} 超过 65536"
delta = b['methods'] - a['methods']
# method_ids 含**外部引用**：替换方法体后会丢掉 toIntent / setAction /
# startActivityForResult 等旧引用（-N），同时新增 ActivityNotFoundException.<init>
# 这一条新引用（+1）。只允许这个数量级的双向微动；定义的方法头集合已在 smali
# 阶段逐字比对过（那一步才是"没有越界替换"的真正证据）。
assert -16 <= delta <= 8, \
    f"ERROR: method_ids 异常 {a['methods']} -> {b['methods']}（±微动之外不允许）"
print(f"  method_ids {a['methods']} -> {b['methods']}（{delta:+d} 外部引用微动，上限 65536）✓")
PY

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
cp "$LIBCXX_SRC" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_apk/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_apk/assets/certs/." "$WORK/assets/certs/"
fi

# ------------------------------------------------------------------ 5/7 manifest
echo "[5/7] binary AndroidManifest（保留 GameApplication + appComponentFactory；+provider +service +权限）"
if [[ -n "${XHZM_BINARY_MANIFEST:-}" && -f "${XHZM_BINARY_MANIFEST:-}" ]]; then
  cp "$XHZM_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"; echo "  using $XHZM_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"; echo "  using cached $MF_CACHE"
else
  # 只解 manifest + resources（--no-assets 跳掉 962 MB 的 assets/，别整包 apktool d）
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

# 官包 <application android:name="com.hotune.esgame.GameApplication"
#           appComponentFactory="androidx.core.app.CoreComponentFactory">
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
                android:value="MOD 菜单悬浮窗服务，需要持续显示悬浮窗供用户操作游戏辅助功能" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)

p.write_text(t, encoding="utf-8")
print("  text Manifest patched (provider 注入, 保留 GameApplication / appComponentFactory)")
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
           "androidx.core.app.CoreComponentFactory" "com.hotune.esgame.GameApplication"; do
    grep -q "$k" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $k" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_N="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true)"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 重建丢失官方节点（$DIFF_D 行）" >&2; exit 1; }
  [[ "$DIFF_N" -ge 15 && "$DIFF_N" -le 40 ]] \
    || { echo "ERROR: manifest 新增节点数异常（$DIFF_N 行）" >&2; exit 1; }
  echo "  manifest ok（新增 $DIFF_N 行，删除 0 行，GameApplication / appComponentFactory 保留）✓"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

# ---------------------------------------------------------------- 6/7 zip merge
echo "[6/7] zip surgical merge → $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$STARRY_DEX" "$PATCH_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst, starry_dex, patch_dex = (
    Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4], sys.argv[5])
mf  = (work / "AndroidManifest.xml").read_bytes()
c2  = (work / patch_dex).read_bytes()
c3  = (work / starry_dex).read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

untouched = ["classes.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libtuanjie.so",
             "lib/arm64-v8a/libunity.so", "lib/arm64-v8a/libFairGuard.so",
             "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libsecsdk.so"]

def is_sig(name: str) -> bool:
    if not name.startswith("META-INF/"):
        return False
    if name == "META-INF/MANIFEST.MF":
        return True
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in ["AndroidManifest.xml", patch_dex] + untouched:
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
    elif name == patch_dex:
        data, ctype = c2, zipfile.ZIP_DEFLATED
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

for name, data in [(starry_dex, c3)] + sorted(extras.items()):
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
for name in zin2.namelist():
    if name.startswith("META-INF/services/"):
        assert name in zout2.namelist(), f"lost {name}"
for name in (starry_dex, patch_dex, "lib/arm64-v8a/libstarry_magic.so",
             "lib/arm64-v8a/libc++_shared.so"):
    assert name in zout2.namelist(), f"missing {name}"
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist()), "signature leftovers"
assert "META-INF/MANIFEST.MF" not in zout2.namelist()
print(f"  merged ok; entries≈{n}; {patch_dex}={len(c2)}; {starry_dex}={len(c3)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

# ------------------------------------------------------------- 7/7 闭环校验
echo "[7/7] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] \
  || { echo "ERROR: 包内 so($IN_SO_MD5) != 构建产物($SRC_SO_MD5)——旧 so 混包" >&2; exit 1; }
echo "  [so] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
grep -qx 'lib/arm64-v8a/libc++_shared.so' "$WORK/entries.txt" \
  || { echo "ERROR: 缺 libc++_shared.so（本游戏不自带，菜单 so 会 dlopen 失败）" >&2; exit 1; }
echo "  [libc++] libc++_shared.so 已随包 ✓"

rm -rf "$WORK/v3" "$WORK/v2"
unzip -p "$UNSIGNED" "$STARRY_DEX" > "$WORK/chk3.dex"
"$BAKSMALI_BIN" d "$WORK/chk3.dex" -o "$WORK/v3" >/dev/null
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali \
         com/starry/magic/sign/SignSpoof.smali com/starry/magic/sign/TapLoginForceWeb.smali; do
  [[ -f "$WORK/v3/$f" ]] || { echo "ERROR: $STARRY_DEX 缺 $f" >&2; exit 1; }
done
echo "  [$STARRY_DEX] StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb ✓"

unzip -p "$UNSIGNED" "$PATCH_DEX" > "$WORK/chk2.dex"
"$BAKSMALI_BIN" d "$WORK/chk2.dex" -o "$WORK/v2" >/dev/null
grep -q 'starry_force_web_login' "$WORK/v2/com/taptap/sdk/ui/ActivityHandler.smali" \
  || { echo "ERROR: $PATCH_DEX 的 ActivityHandler.authorize 未改成恒抛" >&2; exit 1; }
# 关键：<clinit>/<init> 必须仍在（历史上贪婪正则会误删 ⇒ 运行期 NPE）
grep -q 'constructor <init>' "$WORK/v2/com/taptap/sdk/ui/ActivityHandler.smali" \
  || { echo "ERROR: ActivityHandler 的 <init> 丢失" >&2; exit 1; }
grep -q 'toIntent' "$WORK/v2/com/taptap/sdk/ui/ActivityHandler.smali" \
  || { echo "ERROR: ActivityHandler.toIntent 丢失（替换越界）" >&2; exit 1; }
echo "  [$PATCH_DEX] ActivityHandler.authorize 恒抛 starry_force_web_login + <init>/toIntent 完好 ✓"
unzip -p "$UNSIGNED" classes.dex > "$WORK/chk_orig_class1.dex"
unzip -p "$ORIG_APK" classes.dex > "$WORK/chk_src_class1.dex"
[[ "$(md5 -q "$WORK/chk_orig_class1.dex")" == "$(md5 -q "$WORK/chk_src_class1.dex")" ]] \
  || { echo "ERROR: classes.dex 被改动" >&2; exit 1; }
echo "  [classes.dex] 官方字节不变 ✓"

ls -lh "$UNSIGNED"

# ------------------------------------------------------------- 8 签名（可选）
if [[ "${STARRY_SIGN:-0}" == "1" ]]; then
  echo "[8] zipalign + 签名"
  BT=""
  for v in 36.0.0 35.0.0 34.0.0; do
    if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
  done
  ALIGNED="$UNSIGNED"
  if [[ -n "$BT" ]]; then
    "$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
    ALIGNED="$WORK/aligned.apk"
  else
    echo "  WARN: 无 zipalign，跳过对齐"
  fi
  rm -rf "$WORK/signed"; mkdir -p "$WORK/signed"
  # ⚠️ 必须显式给 keystore：uber-apk-signer 不带 --ks 会回落到自带 debug keystore，
  # 出来的包 cert 变成 CN=Android Debug（实测踩过）。
  [[ -f "$KS" ]] || { echo "ERROR: 缺密钥库 $KS" >&2; exit 1; }
  java -jar "$SIGNER" -a "$ALIGNED" --allowResign --out "$WORK/signed" \
    --ks "$KS" --ksAlias starry --ksPass starry2026 --ksKeyPass starry2026 \
    ${BT:+--zipAlignPath "$BT/zipalign"} >/dev/null
  NEWEST="$(ls -t "$WORK/signed"/*Signed*.apk 2>/dev/null | head -1 || true)"
  [[ -n "$NEWEST" ]] || NEWEST="$(ls -t "$WORK/signed"/*.apk | head -1)"
  cp "$NEWEST" "$SIGNED"
  "$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner" verify --print-certs "$SIGNED" | head -6
  # ⚠️ 不要写 `unzip -p … | grep -q`：grep -q 命中即退，上游 unzip 收到 SIGPIPE，
  # 在 set -o pipefail 下整条管道返回非 0 ⇒ 假失败（实测踩过）。先落盘再 grep。
  unzip -p "$SIGNED" "$PATCH_DEX" > "$WORK/chk_signed_c2.dex"
  LC_ALL=C grep -aq 'starry_force_web_login' "$WORK/chk_signed_c2.dex" \
    || { echo "ERROR: 签名包内 $PATCH_DEX 补丁丢失" >&2; exit 1; }
  [[ "$(md5 -q "$WORK/chk_signed_c2.dex")" == "$(md5 -q "$WORK/chk2.dex")" ]] \
    || { echo "ERROR: 签名包内 $PATCH_DEX 与未签名包不一致" >&2; exit 1; }
  "$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner" verify --print-certs "$SIGNED" \
    | awk -F': ' '/MD5 digest/ {print $2}' | tr -d ' ' > "$WORK/signed_cert_md5.txt"
  echo "  signed cert MD5 = $(cat "$WORK/signed_cert_md5.txt")（应为 starry_release.jks，非 Android Debug）"
  ls -lh "$SIGNED"
  echo "DONE(signed): $SIGNED"
else
  echo "DONE(unsigned): $UNSIGNED"
  echo "NEXT: STARRY_SIGN=1 STARRY_INSTALL=1 bash $0"
fi

# ------------------------------------------------------- 9 装机 + 推补丁 bundle
if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  [[ -f "$SIGNED" ]] || { echo "ERROR: STARRY_INSTALL=1 需要先 STARRY_SIGN=1" >&2; exit 1; }
  echo "[9] 装机（改签后与官包签名冲突：需先手工卸载官包，本脚本不自动卸载）"
  "$ADB" -s "$DEV_SERIAL" push "$SIGNED" /data/local/tmp/xhzm_starry.apk >/dev/null
  "$ADB" -s "$DEV_SERIAL" shell am force-stop "$PKG" || true
  if ! "$ADB" -s "$DEV_SERIAL" shell su -c "pm install -r -d --user 0 /data/local/tmp/xhzm_starry.apk"; then
    echo "ERROR: cover-install failed —— 不自动卸载；请人工处理签名冲突后重试" >&2
    exit 1
  fi
  "$ADB" -s "$DEV_SERIAL" shell rm -f /data/local/tmp/xhzm_starry.apk || true
fi

if [[ "${XHZM_PUSH_BUNDLE:-0}" == "1" ]]; then
  [[ -f "$PATCHED_BUNDLE" ]] || { echo "ERROR: 缺补丁 bundle $PATCHED_BUNDLE" >&2; exit 1; }
  echo "[bundle] 推送 IL 补丁 bundle → $REMOTE_BUNDLE_DIR"
  "$ADB" -s "$DEV_SERIAL" push "$PATCHED_BUNDLE" "/data/local/tmp/xhzm_patched.bundle" >/dev/null
  "$ADB" -s "$DEV_SERIAL" shell su -c "mkdir -p '$REMOTE_BUNDLE_DIR'" || true
  "$ADB" -s "$DEV_SERIAL" shell su -c "cp /data/local/tmp/xhzm_patched.bundle '$REMOTE_BUNDLE_DIR/$(basename "$PATCHED_BUNDLE")'" \
    || { echo "ERROR: push bundle failed（su 权限/路径不符）" >&2; exit 1; }
  "$ADB" -s "$DEV_SERIAL" shell rm -f /data/local/tmp/xhzm_patched.bundle || true
  "$ADB" -s "$DEV_SERIAL" shell su -c "ls -l '$REMOTE_BUNDLE_DIR'" | tail -5
fi
