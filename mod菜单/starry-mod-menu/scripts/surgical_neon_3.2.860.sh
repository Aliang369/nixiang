#!/usr/bin/env bash
# 霓虹深渊 3.2.860 TapTap — 外科组装（新版原生菜单，无 assets/web）
#
# - 保留官方 classes.dex / classes3 / classes4 + resources.arsc 字节一致
# - 官方 classes2 已顶满 65535 methods：拆分后重装
#     classes2 = com/tpf（GameActivity 生命周期 + SignSpoof/RootBeer）
#     classes5 = 原 c2 中 kotlin/okhttp/taptap/… + com/starry
# - 旧版 Tap SDK：ActivityHandler.authorize → 抛 starry_force_web_login
#     → LoginClient catch → WebViewHandler（强制网页登录）
# - 删除 assets/web；替换 libstarry_magic.so + assets/certs
# - 2026-10-01 迁移 modern 注册表：ndk-build 走 Android_neon.mk
#     （MainModern.cpp + Games/NeonMod.cpp + Games/NeonEntry.cpp + Games/NoAceStubs.cpp）
# - 2026-10-01 签名改为工作区 release 证书（01_tools/starry_release.jks，CN=StarryMagic）
#     原因：菜单 v5 设备指纹只用 SSAID，而 SSAID 按签名证书隔离 —— 与其它星光包
#     不同证书会算出不同 dev_*，服务端按整张卡绑 device_id ⇒ 403「卡密已绑定其他设备」。
#
# 产物：08_output/霓虹深渊/霓虹深渊_3.2.860_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_neon_3.2.860.sh
#   STARRY_INSTALL=1 bash scripts/surgical_neon_3.2.860.sh
#   NEON_BASE_APK=/path/to/base.apk bash scripts/surgical_neon_3.2.860.sh
#   换签不改内容（不需要官方底包）:
#   NEON_RESIGN_APK=08_output/霓虹深渊/xxx.apk [STARRY_INSTALL=1] bash scripts/surgical_neon_3.2.860.sh
#   换别的 keystore: NEON_SIGN_KS=/path/xx.jks STARRY_KS_ALIAS=xx STARRY_KS_PASS=xx
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="${NEON_BASE_APK:-$NIXIANG/02_apks/霓虹深渊/霓虹深渊_3.2.860_orig_vc104.apk}"
APKSIGNER=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    APKSIGNER="$HOME/Library/Android/sdk/build-tools/$v/apksigner"; break
  fi
done
OUT="$NIXIANG/08_output/霓虹深渊"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.shangyoo.neon"
SIGNED="$OUT/霓虹深渊_3.2.860_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/霓虹深渊_3.2.860_surgical-debugSigned.apk"
ALIAS_APK="$OUT/霓虹深渊_3.2.860_starry-releaseSigned.apk"
# TapTap 官方 cert MD5（TapSignKit.getSign）
ORIG_MD5="49af51d22ed01a4af3b4338ec385a849"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

RESIGN_FROM="${NEON_RESIGN_APK:-}"

if [[ -z "$RESIGN_FROM" && ! -f "$ORIG_APK" ]]; then
  echo "ERROR: 缺少 TapTap 官方原包。" >&2
  echo "" >&2
  echo "  1) 下载（任选）：" >&2
  echo "     TapTap 客户端搜「霓虹深渊：无限」导出 APK" >&2
  echo "     或 Uptodown: https://neon-abyss-infinite.en.uptodown.com/android/download" >&2
  echo "" >&2
  echo "  2) 保存为：" >&2
  echo "     $NIXIANG/02_apks/霓虹深渊/霓虹深渊_3.2.860_orig_vc104.apk" >&2
  echo "     详 $NIXIANG/02_apks/霓虹深渊/README.md" >&2
  echo "" >&2
  echo "  或: NEON_BASE_APK=/path/to/official.apk bash $0" >&2
  exit 1
fi
if [[ -n "$RESIGN_FROM" ]]; then
  [[ -f "$RESIGN_FROM" ]] || { echo "ERROR: NEON_RESIGN_APK 不存在: $RESIGN_FROM" >&2; exit 1; }
  echo "[resign] 换签模式：跳过 [1/6]..[4/6]，不需官方底包；输入 $RESIGN_FROM"
elif [[ -n "$APKSIGNER" ]]; then
  set +e
  APK_MD5="$("$APKSIGNER" verify --print-certs "$ORIG_APK" 2>/dev/null | awk '/MD5 digest/ {print $NF; exit}')"
  set -e
  APK_MD5="${APK_MD5:-}"
  if [[ "$APK_MD5" != "$ORIG_MD5" ]]; then
    echo "ERROR: base APK cert MD5=${APK_MD5:-<empty>}, want official $ORIG_MD5" >&2
    echo "  not TapTap official (debug/mod apk?) see 02_apks/霓虹深渊/README.md" >&2
    exit 1
  fi
  echo "[check] official base cert MD5 ok ($ORIG_MD5)"
else
  echo "[warn] no apksigner, skip base cert check"
fi
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android_neon.mk"
[[ -f "$MK" ]] || { echo "ERROR: missing $MK (modern 注册表私有 mk)"; exit 1; }
grep -q 'Games/NeonEntry.cpp' "$MK" \
  || { echo "ERROR: Android_neon.mk 未编译 Games/NeonEntry.cpp"; exit 1; }
grep -q 'Games/NeonMod.cpp' "$MK" \
  || { echo "ERROR: Android_neon.mk 未编译 Games/NeonMod.cpp"; exit 1; }
echo "[check] Android_neon.mk target=neon (modern 注册表)"

LOCK_DIR="/tmp/neon_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/neon_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK"

if [[ -n "$RESIGN_FROM" ]]; then
  cp -f "$RESIGN_FROM" "$WORK/unsigned.apk"
else
echo "[1/6] ndk-build (Android_neon.mk -> libs_neon) ([Neon] 自检)"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
JNI_DIR="$STARRY_SRC/app/src/main/jni"
OBJ_OUT="$STARRY_SRC/app/src/obj_neon"
LIBS_OUT="$STARRY_SRC/app/src/libs_neon"
(cd "$JNI_DIR" && \
  "$NDK_BUILD" NDK_PROJECT_PATH="$STARRY_SRC/app" APP_BUILD_SCRIPT=./Android_neon.mk \
  NDK_APPLICATION_MK=./Application.mk NDK_OUT="$OBJ_OUT" NDK_LIBS_OUT="$LIBS_OUT" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$LIBS_OUT/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Neon\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 NEON — strings 未见 [Neon]" >&2
  strings "$SO_BUILT" | grep -E '\[Neon\]|\[Wggls\]|\[Shhj\]' | sort -u | head -10
  exit 1
fi
if grep -a -q '\[Wggls\]' "$SO_BUILT" || grep -a -q '\[Shhj\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签" >&2
  exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Neon] verified)"

echo "[1b/6] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
SIGN_SPOOF="$WORK/starry/app/src/main/java/com/starry/magic/sign/SignSpoof.java"
python3.11 - "$ORIG_APK" "$SIGN_SPOOF" "$ORIG_MD5" <<'PY'
import base64, hashlib, re, subprocess, sys, tempfile, zipfile
from pathlib import Path

apk, spoof_path, expect_md5 = sys.argv[1:4]
spoof = Path(spoof_path)

def extract_cert_der(apk_path: str) -> bytes:
    with zipfile.ZipFile(apk_path, "r") as z:
        names = [
            n
            for n in z.namelist()
            if n.startswith("META-INF/")
            and (n.endswith(".RSA") or n.endswith(".DSA") or n.endswith(".EC"))
        ]
        if not names:
            raise SystemExit(f"no META-INF signature block in {apk_path}")
        data = z.read(names[0])
    with tempfile.NamedTemporaryFile(suffix=".rsa", delete=False) as f:
        f.write(data)
        rsa = f.name
    pem = subprocess.check_output(
        ["openssl", "pkcs7", "-inform", "DER", "-in", rsa, "-print_certs", "-outform", "PEM"],
        text=True,
    )
    der = subprocess.check_output(
        ["openssl", "x509", "-inform", "PEM", "-outform", "DER"],
        input=pem.encode(),
    )
    Path(rsa).unlink(missing_ok=True)
    return der

der = extract_cert_der(apk)
if hashlib.md5(der).hexdigest() != expect_md5.lower():
    raise SystemExit(f"cert MD5 mismatch after export (expect {expect_md5})")
text = spoof.read_text(encoding="utf-8")
if "NEON_TT_PKG" not in text or "fakes.put(NEON_TT_PKG" not in text:
    raise SystemExit("SignSpoof.java missing NEON_TT (com.shangyoo.neon) — add cert in source first")
# 校验源码内嵌 DER 与官方底包一致
m = re.search(
    r'NEON_TT_CERT_B64\s*=\s*((?:"[^"]+"\s*\+\s*)*"[^"]+");',
    text,
    re.S,
)
if not m:
    raise SystemExit("NEON_TT_CERT_B64 parse failed")
expr = m.group(1)
parts = re.findall(r'"([^"]*)"', expr)
embedded = base64.b64decode("".join(parts))
if hashlib.md5(embedded).hexdigest() != expect_md5.lower():
    raise SystemExit("SignSpoof NEON_TT cert MD5 != official base")
if embedded != der:
    raise SystemExit("SignSpoof NEON_TT DER bytes != official base CERT.RSA")
print(f"  SignSpoof NEON_TT ok (MD5={expect_md5})")
PY
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Neon\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 NEON" >&2
  exit 1
fi
if unzip -l "$STARRY_APK" | grep -q 'assets/web/'; then
  echo "ERROR: starry 模板仍含 assets/web（应已去资源化）" >&2
  exit 1
fi
echo "  gradle APK ok (native menu, no web)"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so in starry decode"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] baksmali classes2 + 64k 拆分 → c2(com/tpf) / c5(taptap+deps+starry)"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
# 官方 c2 已 65535 methods：整包 smali 重装会 Unsigned short overflow，且曾产出缺 TapTap 的残缺 dex
mkdir -p "$WORK/c5smali/com"
for pkg in kotlin okhttp3 okio io org; do
  if [[ -d "$WORK/c2smali/$pkg" ]]; then
    rm -rf "$WORK/c5smali/$pkg"
    mv "$WORK/c2smali/$pkg" "$WORK/c5smali/"
  fi
done
for pkg in tds tapsdk tencent taptap zui vivo taptapshare unicom; do
  if [[ -d "$WORK/c2smali/com/$pkg" ]]; then
    rm -rf "$WORK/c5smali/com/$pkg"
    mv "$WORK/c2smali/com/$pkg" "$WORK/c5smali/com/"
  fi
done
[[ -d "$WORK/c5smali/com/taptap/sdk/ui" ]] || {
  echo "ERROR: split 后 c5 无 com/taptap/sdk/ui（强制网页登录锚点丢失）" >&2
  exit 1
}
[[ -d "$WORK/c2smali/com/tpf" ]] || {
  echo "ERROR: split 后 c2 无 com/tpf" >&2
  exit 1
}
# starry 菜单进 classes5（与拆出的 TapTap 同 dex，ART 连续 classesN）
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$WORK/c5smali/com/"
done
shopt -u nullglob
[[ -d "$WORK/c5smali/com/starry" ]] || { echo "ERROR: com/starry smali missing"; exit 1; }
if ! find "$WORK/c5smali" -name 'NativeMenuController.smali' | grep -q .; then
  echo "ERROR: NativeMenuController.smali missing" >&2
  exit 1
fi
echo "  split ok: c2=com/tpf…  c5=taptap+deps+starry"

echo "[3b/6] patch c5 ActivityHandler → force WebLogin；c2 GameActivity/SignSpoof/RootBeer"
python3.11 - "$WORK/c2smali" "$WORK/c5smali" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys

c2 = Path(sys.argv[1])
c5 = Path(sys.argv[2])
md5 = sys.argv[3]

def patch_force_web_login(root: Path):
    ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
    if not ah.is_file():
        raise SystemExit(f"missing {ah}")
    ah_t = ah.read_text(encoding="utf-8")
    if "starry_force_web_login" in ah_t:
        print("  ActivityHandler.authorize ok (already forced)")
        return
    # 旧版 Tap SDK（.registers 风格）：抛异常 → LoginClient catch → WebViewHandler
    FORCE_WEB = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
    ah2, n = re.subn(
        r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
        FORCE_WEB,
        ah_t,
        count=1,
    )
    if n != 1:
        raise SystemExit("ActivityHandler.authorize patch failed")
    ah.write_text(ah2, encoding="utf-8")
    print("  ActivityHandler.authorize → force WebLogin (starry_force_web_login)")

def patch_tapsign(root: Path):
    taps = list(root.glob("**/TapSignKit.smali"))
    if not taps:
        print("  TapSignKit: skip (本包旧版 SDK 无 TapSignKit，靠 SignSpoof)")
        return
    body = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
    for tap in taps:
        ts = tap.read_text(encoding="utf-8")
        if "starry_sign_bypass" in ts:
            print(f"  TapSignKit ok ({tap.relative_to(root)})")
            continue
        ts2, n = re.subn(
            r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
            body,
            ts,
            count=1,
        )
        if n == 1:
            tap.write_text(ts2, encoding="utf-8")
            print(f"  TapSignKit.getSign → {md5} ({tap.relative_to(root)})")
        else:
            print(f"  WARN TapSignKit patch miss ({tap})")

def patch_tpf_application(root: Path):
    app = root / "com/tpf/sdk/TPFApplication.smali"
    if not app.is_file():
        print("  WARN TPFApplication missing")
        return
    s = app.read_text(encoding="utf-8")
    s = re.sub(
        r"\n\.method static constructor <clinit>\(\)V\n(?:.|\n)*?SignSpoof;->install\(\)V\n(?:.|\n)*?\.end method\n",
        "\n",
        s,
        count=1,
    )
    if "SignSpoof;->install" in s and "attachBaseContext" in s:
        print("  TPFApplication SignSpoof ok")
        app.write_text(s, encoding="utf-8")
        return
    load_lib = (
        "\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n\n"
        '    const-string v0, "starry_magic"\n\n'
        "    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n"
    )
    s2, n = re.subn(
        r"(\.method public attachBaseContext\(Landroid/content/Context;\)V\n(?:    \.locals \d+\n)?)",
        r"\1" + load_lib,
        s,
        count=1,
    )
    if n != 1:
        # baksmali 常用 .registers
        s2, n = re.subn(
            r"(\.method public attachBaseContext\(Landroid/content/Context;\)V\n(?:    \.registers \d+\n)?)",
            r"\1" + load_lib,
            s,
            count=1,
        )
    if n != 1:
        print("  WARN TPFApplication.attachBaseContext patch miss")
        return
    app.write_text(s2, encoding="utf-8")
    print("  TPFApplication.attachBaseContext → SignSpoof + loadLibrary")

def bump_method_regs(head: str) -> str:
    """GameActivity.onCreate 等 .registers 2 时 v0==p0；loadLibrary 须先 +1 腾寄存器。"""
    head2, n = re.subn(
        r"(\.locals )(\d+)",
        lambda mm: mm.group(1) + str(int(mm.group(2)) + 1),
        head,
        count=1,
    )
    if n:
        return head2
    head2, n = re.subn(
        r"(\.registers )(\d+)",
        lambda mm: mm.group(1) + str(int(mm.group(2)) + 1),
        head,
        count=1,
    )
    return head2 if n else head

def patch_rootbeer(root: Path):
    for rb in root.glob("**/RootBeerNative.smali"):
        s = rb.read_text(encoding="utf-8")
        if "starry_rootbeer_bypass" in s:
            print(f"  RootBeerNative ok ({rb.relative_to(root)})")
            continue
        body = """.method public checkForRoot([Ljava/lang/String;)Z
    .locals 1

    # starry_rootbeer_bypass
    const/4 v0, 0x0

    return v0
.end method"""
        s2, n = re.subn(
            r"\.method public (?:native )?checkForRoot\(\[Ljava/lang/String;\)Z[\s\S]*?\.end method",
            body,
            s,
            count=1,
        )
        if n == 1:
            rb.write_text(s2, encoding="utf-8")
            print(f"  RootBeerNative bypass ({rb.relative_to(root)})")

def inject_activity(path: Path, tag: str):
    if not path.is_file():
        print(f"  skip {tag} (missing)")
        return
    s = path.read_text(encoding="utf-8")
    if "MenuService;->start" not in s:
        m = re.search(
            r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(    return-void\n\.end method)",
            s,
        )
        if not m:
            m = re.search(
                r"(\.method public onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(    return-void\n\.end method)",
                s,
            )
        if not m:
            print(f"  WARN {tag}.onCreate not found")
            return
        head = m.group(1)
        head2 = bump_method_regs(head)
        # loadLibrary 已在 TPFApplication.attachBaseContext；Activity 只挂 MenuService/SignSpoof
        inject = """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
        s = s[: m.start(1)] + head2 + inject + s[m.start(2) :]
        print(f"  hook {tag}.onCreate")
    elif "SignSpoof;->install" not in s:
        s2, n = re.subn(
            r"(invoke-static \{p0\}, Lcom/starry/magic/menu/MenuService;->start\(Landroid/content/Context;\)V)",
            "    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n\n    \\1",
            s,
            count=1,
        )
        if n == 1:
            s = s2
            print(f"  hook {tag}.onCreate +SignSpoof")
    if "MenuService;->notifyHostResumed" not in s:
        s2, n = re.subn(
            r"(\.method protected onResume\(\)V[\s\S]*?)(    return-void\n\.end method)",
            r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
            s,
            count=1,
        )
        if n == 1:
            s = s2
            print(f"  hook {tag}.onResume")
    if "MenuService;->notifyHostLeaving" not in s and ".method protected onPause()V" in s:
        s2, n = re.subn(
            r"(\.method protected onPause\(\)V[\s\S]*?)(    return-void\n\.end method)",
            r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
            s,
            count=1,
        )
        if n == 1:
            s = s2
            print(f"  hook {tag}.onPause")
    if "MenuService;->notifyHostLeaving" not in s and "onUserLeaveHint" not in s:
        s += """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method
"""
        print(f"  hook {tag}.onUserLeaveHint")
    path.write_text(s, encoding="utf-8")

patch_force_web_login(c5)
patch_tapsign(c5)
patch_tapsign(c2)
patch_tpf_application(c2)
patch_rootbeer(c2)
patch_rootbeer(c5)
for rel in (
    "com/tpf/sdk/unity/GameActivity.smali",
    "com/tpf/sdk/activity/TPFSplashActivity.smali",
):
    p = c2 / rel
    if p.is_file():
        inject_activity(p, rel)
PY

echo "[3c/6] assemble classes2.dex + classes5.dex（校验 method < 65535）"
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$WORK/c5smali"
ls -la "$WORK/classes2.dex" "$WORK/classes5.dex"
python3.11 - "$WORK/classes2.dex" "$WORK/classes5.dex" <<'PY'
import struct, sys
from pathlib import Path

def check(path: Path, need_force: bool):
    data = path.read_bytes()
    methods = struct.unpack_from("<I", data, 0x58)[0]
    classes = struct.unpack_from("<I", data, 0x60)[0]
    print(f"  {path.name}: methods={methods} classes={classes} size={len(data)}")
    if methods == 0 or classes == 0:
        raise SystemExit(f"{path.name}: corrupt dex (methods/classes=0) — smali assemble failed?")
    if methods >= 65535:
        raise SystemExit(f"{path.name}: methods={methods} still at/over 64k limit")
    if need_force and b"starry_force_web_login" not in data:
        raise SystemExit(f"{path.name}: missing starry_force_web_login")
    if need_force and b"ActivityHandler" not in data:
        raise SystemExit(f"{path.name}: missing ActivityHandler")
    if not need_force and b"MenuService" not in data:
        raise SystemExit(f"{path.name}: missing MenuService hook")

check(Path(sys.argv[1]), need_force=False)
check(Path(sys.argv[2]), need_force=True)
print("  dex limits + WebLogin patch ok")
PY

echo "[3d/6] AndroidManifest → MenuService + perms（apktool 只编 Manifest，不整包 b 游戏）"
MF_CACHE="$NIXIANG/07_patched/霓虹深渊/surgical_3.2.860/AndroidManifest.xml"
mkdir -p "$(dirname "$MF_CACHE")"
if [[ -f "$MF_CACHE" ]] && grep -q 'com.starry.magic.menu.MenuService' "$MF_CACHE"; then
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  Manifest from cache"
else
  # 勿加 -r：须解码 Manifest 为明文 XML（-r 会复制 binary AXML → Python utf-8 失败）
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
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
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("  +perms")
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
    print("  +MenuService")
p.write_text(t, encoding="utf-8")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f -o "$WORK/mf_only.apk" "$WORK/orig_dec" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
  # 也缓存明文便于 diff
  cp -f "$WORK/orig_dec/AndroidManifest.xml" "${MF_CACHE%.xml}.decoded.xml"
  echo "  Manifest rebuilt binary"
fi

echo "[4/6] zip surgical merge (strip assets/web)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
c2 = (work / "classes2.dex").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
mf = (work / "AndroidManifest.xml").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

PROTECTED = ("classes.dex", "classes3.dex", "classes4.dex", "resources.arsc")

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name.startswith("assets/web/"):
        continue
    if name in extras or name in ("classes2.dex", "classes5.dex", "AndroidManifest.xml"):
        continue
    data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

for name, blob in [
    ("AndroidManifest.xml", mf),
    ("classes2.dex", c2),
    ("classes5.dex", c5),
]:
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, blob)
    n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

zin.close()
zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("classes.dex", "classes3.dex", "classes4.dex", "resources.arsc"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "assets/web/pages/mod-menu-v2.html" not in zout2.namelist()
assert "classes5.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
nc = zout2.read("classes5.dex")
assert b"NativeMenuController" in nc, "classes5 missing NativeMenuController"
assert b"SignSpoof" in nc or b"SignSpoof" in zout2.read("classes5.dex")
assert b"starry_force_web_login" in nc, "classes5 missing TapTap WebLogin patch"
assert b"ActivityHandler" in nc, "classes5 missing ActivityHandler (64k split failed?)"
c2b = zout2.read("classes2.dex")
assert b"SignSpoof" in c2b, "classes2 missing SignSpoof install"
assert b"MenuService" in c2b, "classes2 missing MenuService"
assert b"ActivityHandler" not in c2b, "classes2 still has ActivityHandler — should be split to classes5"
print(f"  merged ok; entries≈{n}; Manifest+classes5; WebLogin forced; protected dex/arsc intact")
zin2.close()
zout2.close()
PY

fi

echo "[5/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
mkdir -p "$OUT"
# 统一用工作区 release 证书（CN=StarryMagic）：v5 指纹只用 SSAID，SSAID 按签名证书隔离，
# 与其它星光包同证书才能算出同一个 dev_*（否则卡密 403 已绑定其他设备）。
SIGN_KS="${NEON_SIGN_KS:-$KS}"
[[ -f "$SIGN_KS" ]] || { echo "ERROR: keystore 不存在: $SIGN_KS" >&2; exit 1; }
[[ -n "$APKSIGNER" ]] || { echo "ERROR: 找不到 apksigner" >&2; exit 1; }
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$APKSIGNER" sign \
  --ks "$SIGN_KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
CERT_MD5="$("$APKSIGNER" verify --print-certs "$SIGNED" 2>/dev/null | awk '/MD5 digest/ {print $NF; exit}')"
CERT_SHA1="$("$APKSIGNER" verify --print-certs "$SIGNED" 2>/dev/null | awk '/SHA-1 digest/ {print $NF; exit}')"
echo "[sign] ks=$(basename "$SIGN_KS") alias=$KS_ALIAS certMD5=$CERT_MD5 certSHA1=$CERT_SHA1"
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED"
if unzip -l "$SIGNED" | grep -q 'assets/web/'; then
  echo "ERROR: 成品仍含 assets/web" >&2
  exit 1
fi
echo "done → $SIGNED"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  exit 0
fi

echo "[6/6] adb install"
REMOTE="/data/local/tmp/starry_neon_surgical.apk"
adb push "$SIGNED" "$REMOTE"
set +e
OUT="$(adb shell su -c "pm install -r -d --user 0 '$REMOTE'" 2>&1)"
RC=$?
if [[ $RC -ne 0 ]]; then
  OUT2="$(adb install -r --no-incremental "$SIGNED" 2>&1)"
  RC=$?
  OUT="$OUT"$'\n'"$OUT2"
fi
set -e
echo "$OUT"
if echo "$OUT" | grep -qE 'INSTALL_FAILED_UPDATE_INCOMPATIBLE|signatures do not match'; then
  echo "ERROR: 真机当前是官方签，星光包是 debug 签，覆盖装失败。" >&2
  echo "【请配合】确认可以卸载 com.shangyoo.neon 后回复「可以卸载」——大包数据可能要重下。" >&2
  exit 2
fi
if [[ $RC -ne 0 ]]; then
  echo "ERROR: install failed rc=$RC" >&2
  exit 1
fi
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell am force-stop "$PKG" 2>/dev/null || true
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "log: adb logcat -s StarryApp:V StarryMagic:V Mod_Menu MenuService Neon StarrySignSpoof"
