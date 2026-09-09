#!/usr/bin/env bash
# 星光魔法 → 物质与魔法 6.03 原包：构建 + 注入 + 签名安装
#
# 注意：Gradle/apktool/aapt2 在含中文的路径下会把目录弄成 ?? 并失败。
# 本脚本把 native/gradle/apktool 都放到 /tmp ASCII 路径完成，再拷回 08_output。
#
# 用法:
#   bash scripts/inject_wzym_6.03.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_wzym_6.03.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_wzym_6.03.sh
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug (got: $BUILD_TYPE)"
  exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"
  APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"
  APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
SRC_DECODE="$NIXIANG/03_decompiled/物质与魔法/apktool_6.03_orig"
FINAL_PATCH="$NIXIANG/07_patched/物质与魔法/apktool_6.03_modmenu"
OUT="$NIXIANG/08_output/物质与魔法"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.huanshits.materialMagic"
SIGNED="$OUT/物质与魔法_6.03_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/物质与魔法_6.03_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/wzym_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/wzym_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -d "$SRC_DECODE" ]] || { echo "ERROR: missing decode $SRC_DECODE"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
mkdir -p "$OUT" "$OUT_ASCII"

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"

# Gradle 在中文路径下无法 spawn ndk-build：预编译 so → jniLibs，并关掉 externalNativeBuild
APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK) in $STARRY_BUILD"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$APP/build/intermediates/ndk/libs/arm64-v8a/"*.so "$APP/src/main/jniLibs/arm64-v8a/"
python3.11 - <<PY
from pathlib import Path
import re
p = Path("$APP/build.gradle")
t = p.read_text()
t2 = re.sub(
    r"\n    externalNativeBuild \{[\s\S]*?\n    \}\n",
    "\n    // externalNativeBuild disabled (ASCII CI): so from jniLibs\n",
    t,
    count=1,
)
p.write_text(t2)
PY
(cd "$STARRY_BUILD" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY_BUILD/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }
echo "using: $APK"

echo "[2/7] decode starry"
apktool d -f -o "$WORK/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi
rm -rf "$PATCH/smali_classes3/com/starry"
mkdir -p "$PATCH/smali_classes3/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes3/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes3/com/"
done
[[ -d "$PATCH/smali_classes3/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / MainActivity / SplashActivity / qkcheck fail"
python3.11 <<PY
from pathlib import Path
import re
import struct

PATCH = Path(r"$PATCH")

manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("manifest: perms")
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
    print("manifest: MenuService")
manifest.write_text(t, encoding="utf-8")

def inject_activity(path: Path, tag: str):
    s = path.read_text(encoding="utf-8")
    if "MenuService;->start" not in s:
        m = re.search(
            r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n.end method)",
            s)
        if not m:
            raise SystemExit(f"{tag}.onCreate not found")
        # .locals 至少 1
        head = m.group(1)
        head2, n = re.subn(r"(\.locals )(\d+)", lambda mm: mm.group(1) + str(max(int(mm.group(2)), 1)), head, count=1)
        inject = """
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
        s = s[: m.start(1)] + head2 + inject + s[m.start(2) :]
        print(f"hook: {tag}.onCreate")
    if "MenuService;->notifyHostLeaving" not in s:
        s += """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method
"""
        print(f"hook: {tag}.onUserLeaveHint")
    if "MenuService;->notifyHostResumed" not in s:
        # 避免与已有 onResume 冲突
        if ".method protected onResume()V" not in s and ".method public onResume()V" not in s:
            s += """
.method protected onResume()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onResume()V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V

    return-void
.end method
"""
            print(f"hook: {tag}.onResume")
    path.write_text(s, encoding="utf-8")

inject_activity(PATCH / "smali/com/qk/game/MainActivity.smali", "MainActivity")
inject_activity(PATCH / "smali/com/qk/game/SplashActivity.smali", "SplashActivity")

# libqkcheck: nop deal_check_fail 入口（改签后避免踢出）
import struct
so = PATCH / "lib/arm64-v8a/libqkcheck.so"
if so.exists():
    data = bytearray(so.read_bytes())
    assert data[:4] == b"\x7fELF"
    e_shoff = struct.unpack_from("<Q", data, 40)[0]
    e_shentsize = struct.unpack_from("<H", data, 58)[0]
    e_shnum = struct.unpack_from("<H", data, 60)[0]
    e_shstrndx = struct.unpack_from("<H", data, 62)[0]

    def sh(i):
        off = e_shoff + i * e_shentsize
        return struct.unpack_from("<IIQQQQIIQQ", data, off)

    shstr = sh(e_shstrndx)
    strtab = data[shstr[4] : shstr[4] + shstr[5]]
    dynsym = dynstr = None
    for i in range(e_shnum):
        n, t, f, a, o, sz, l, info, al, es = sh(i)
        name = strtab[n : strtab.find(b"\0", n)].decode()
        if name == ".dynsym":
            dynsym = (o, sz, es, l)
        if name == ".dynstr":
            dynstr = (o, sz)
    e_phoff = struct.unpack_from("<Q", data, 32)[0]
    e_phentsize = struct.unpack_from("<H", data, 54)[0]
    e_phnum = struct.unpack_from("<H", data, 56)[0]
    loads = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags, p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from(
            "<IIQQQQQQ", data, off
        )
        if p_type == 1:
            loads.append((p_offset, p_vaddr, p_filesz))

    def va2off(va):
        for po, pv, pf in loads:
            if pv <= va < pv + pf:
                return po + (va - pv)
        return None

    patched = False
    if dynsym and dynstr:
        o, sz, es, link = dynsym
        stro = dynstr[0]
        for off in range(o, o + sz, es):
            st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from(
                "<IBBHQQ", data, off
            )
            name = data[stro + st_name : data.find(b"\0", stro + st_name)].decode(errors="ignore")
            if "deal_check_fail" not in name:
                continue
            fo = va2off(st_value)
            if fo is None:
                break
            ret = bytes.fromhex("c0035fd6")  # ARM64 ret
            if data[fo : fo + 4] != ret:
                data[fo : fo + 4] = ret
                so.write_bytes(data)
                print(f"sign: nop deal_check_fail @ file 0x{fo:x}")
            else:
                print("sign: deal_check_fail already RET")
            patched = True
            break
    if not patched:
        print("WARN: deal_check_fail patch skipped")
print("hooks done")
PY

echo "[5/7] (no extra asset gate)"
echo "[6/7] apktool rebuild (ASCII)"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + install"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
# uber writes *-aligned-debugSigned.apk next to input; prefer *-Signed.apk
SIGNED_SRC=""
for c in \
  "${UNSIGNED%.apk}-aligned-debugSigned.apk" \
  "${UNSIGNED%.apk}-aligned-releaseSigned.apk" \
  "${UNSIGNED%.apk}-debugSigned.apk" \
  "${UNSIGNED%.apk}-releaseSigned.apk" \
  "$UNSIGNED"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
# uber --overwrite 常直接覆盖 UNSIGNED
cp -f "$SIGNED_SRC" "$SIGNED"
rm -rf "$FINAL_PATCH"
mkdir -p "$(dirname "$FINAL_PATCH")"
cp -a "$PATCH" "$FINAL_PATCH"
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_wzym_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"; then
  echo "signature mismatch? uninstall + reinstall"
  adb uninstall "$PKG" 2>/dev/null || true
  adb install --no-incremental "$SIGNED"
fi
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Wzym Mod_Menu MenuService"
