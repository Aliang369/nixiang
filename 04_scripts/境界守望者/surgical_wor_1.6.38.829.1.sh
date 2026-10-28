#!/usr/bin/env bash
# 境界守望者 1.6.38.829.1（com.ap.watcherofrealms，Google Play 国际服官包）— 星光外科组装（e2 功能阶段）
#
# 母本：04_scripts/潮汐守望者/surgical_cxswz_1.2.56.375.3.sh（国服潮汐守望者，同产品线同架构）
#
# 官包零改动：base 的 classes.dex..classes5.dex / resources.arsc / assets/** / lib/arm64-v8a/libil2cpp.so
#   - split_config.arm64_v8a.apk：只剥离官方签名（META-INF/BNDLTOOL.*）与 Play 来源戳
#     stamp-cert-sha256；35 个 .so 与 AndroidManifest.xml 字节不变。
# 改动（只作用于 base.apk）：
#   - 改 AndroidManifest（保留原 application=com.moba.unityplugin.MobaGameApplication
#     与 appComponentFactory；只 +StarryInitProvider + MenuService +
#     SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE）
#   - 新增 classes6.dex（星光 smali：StarryInitProvider / MenuService / StarryAppComponentFactory）
#     ★ 官方占用 classes.dex..classes5.dex，故新增下标必须是 6（比最大 dex 号 +1；
#       本脚本用 04_scripts/_common/next_free_dex.py 动态取，缺口会截断 ART dex 扫描）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so + assets/certs/*
#   - 去签名：删官方 META-INF/*.SF|*.RSA|*.DSA|*.EC
#     （2026-10-28：运行时签名伪装 SignSpoof 已删除；本作若需过壳验签须按改包法迁到 dex 侧）
#
# ★ 与国服母本的三点关键差异（勿照抄母本）：
#   1) 不注入 TapLoginForceWeb：本作无 TapTap/QuickSDK/TapLoginForceWeb 需求。
#      该类仍在共享 Java 桥里（StarryInitProvider 静态引用，删了编译不过），但其生效靠
#      TapLoginForceWeb.java 内的包名白名单——**白名单里没有 com.ap.watcherofrealms 即天然不生效**，
#      本脚本第 [2/6] 步会显式断言这一点（若误加入白名单则报错退出）。
#   2) Application 不替换（com.moba.unityplugin.MobaGameApplication，母本亦不替换，此处仅强调）。
#   3) Split APK（base + split_config.arm64_v8a.apk，Qigsaw）：base **不含任何 .so**，
#      全部 35 个 .so（含 libil2cpp.so / libhades.so / libshadowhook.so / libxlua.so）都在 split 里，
#      故 base 的 lib/arm64-v8a/libil2cpp.so 走「存在才纳入 untouched」分支（不硬断言）。
#      真正的 IL2CPP 主库 liblogic.so（137MB）不在任何 APK 里——base 的 assets/Resources.dat
#      (133,074,541 B) 是自解压壳载荷，运行时才落盘，所以 WorMod 的 dlopen 等待循环是对的。
#
# 功能：菜单进游戏 + 去签名 + 官证回灌（e2 功能阶段，见 Games/WorMod.cpp 的 WOR_ENV_STAGE=0）
#
# ★ split_config.arm64_v8a.apk 必须一起改（不只是"一起装"）：
#   Android 要求同一次 install-multiple 会话内所有 APK 用**同一张证书**，
#   官方 split 由 BNDLTOOL 签名、base 被我们改签 starry_release.jks ⇒ 不重签 split 必然
#   INSTALL_FAILED_INVALID_APK / Inconsistent certificates。故第 [5b/6] 步剥掉 split 的
#   META-INF/BNDLTOOL.* 与 stamp-cert-sha256（Play 来源戳，改签后必然对不上），
#   由 wor_sign.sh 用同一把钥匙一起签。split 的 .so / AndroidManifest.xml 字节不动。
#   另：官方 manifest 是 extractNativeLibs=true ⇒ .so 压缩存储合法，逐条保留原压缩方式即可。
#
# 产物：08_output/境界守望者/境界守望者_1.6.38.829.1_starry-unsigned.apk        （base）
#       08_output/境界守望者/境界守望者_1.6.38.829.1_starry-split-unsigned.apk  （split）
# 用法: bash 04_scripts/境界守望者/surgical_wor_1.6.38.829.1.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
# 环境准备（JDK/SDK/NDK/build-tools/01_tools on PATH；Windows Git Bash 与 macOS 通用）
# shellcheck source=wor_env.sh
[[ -f "$SCRIPT_DIR/wor_env.sh" ]] && . "$SCRIPT_DIR/wor_env.sh"
WUSER="${USER:-${USERNAME:-Administrator}}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="境界守望者"
VERSION="1.6.38.829.1"
PKG="com.ap.watcherofrealms"
ORIG_APK="${WOR_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_global_original.apk}"
ORIG_MD5="${WOR_BASE_MD5:-ac171aa379dcceb771150c97aadf5389}"
# split 是 Qigsaw 拆分包：只在第 [5b/6] 步剥离官方签名/stamp，其余字节全不动。
SPLIT_APK="${WOR_SPLIT_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_global_split_config.arm64_v8a.apk}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_starry-unsigned.apk"
SPLIT_UNSIGNED="$OUT/${GAME}_${VERSION}_starry-split-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
AUTHORITY="$PKG.starryinit"

# ---------- 可移植性垫片（mac / Windows Git Bash 都能跑） ----------
PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3 python py; do
    if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
  done
fi
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python（设 PYTHON=/path/to/python3）" >&2; exit 1; }

md5_of() {
  if command -v md5 >/dev/null 2>&1; then md5 -q "$1"
  else md5sum "$1" | awk '{print $1}'; fi
}
md5_stdin() {
  if command -v md5 >/dev/null 2>&1; then md5 -q
  else md5sum | awk '{print $1}'; fi
}

NDK="${ANDROID_NDK_HOME:-${ANDROID_NDK_ROOT:-}}"
if [[ -z "$NDK" ]]; then
  for cand in "$HOME/Library/Android/sdk/ndk"/* "$HOME/Android/Sdk/ndk"/* \
              "/c/Users/$WUSER/AppData/Local/Android/Sdk/ndk"/* \
              "/mnt/c/Users/$WUSER/AppData/Local/Android/Sdk/ndk"/*; do
    [[ -d "$cand" ]] && NDK="$cand"
  done
  # 具体版本优先 27.0.12077973（与 Android_wor.mk / 母本一致）
  if [[ -n "$NDK" && ! -d "$NDK/toolchains" ]]; then
    for v in 27.0.12077973; do
      [[ -d "$NDK/$v/toolchains" ]] && NDK="$NDK/$v" && break
    done
  fi
fi
NDK_CXX=""
for tag in darwin-x86_64 linux-x86_64 windows-x86_64; do
  c="$NDK/toolchains/llvm/prebuilt/$tag/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
  [[ -f "$c" ]] && NDK_CXX="$c" && break
done

if command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing（01_tools/smali）" >&2; exit 1; }
command -v apktool >/dev/null 2>&1 \
  || { echo "ERROR: apktool 不在 PATH（01_tools/apktool 应由 wor_env.sh 加进 PATH）" >&2; exit 1; }
command -v java >/dev/null 2>&1 || { echo "ERROR: java 不在 PATH（设 JAVA_HOME）" >&2; exit 1; }
SO_PREBUILT="${WOR_SO:-$STARRY_SRC/libs_wor/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing starry so ${SO_PREBUILT}（先跑 04_scripts/境界守望者/wor_build_so.sh）" >&2; exit 1; }
[[ -n "$NDK_CXX" && -f "$NDK_CXX" ]] || { echo "ERROR: libc++_shared.so 未找到（ANDROID_NDK_HOME=${NDK}）" >&2; exit 1; }
if [[ -f "$SPLIT_APK" ]]; then
  echo "  split present（只剥签名，.so/manifest 字节不动）: $SPLIT_APK"
else
  echo "ERROR: 缺 split_config.arm64_v8a.apk，装不上（base 无任何 .so，全在 split 里）: $SPLIT_APK" >&2
  exit 1
fi

ACTUAL_MD5="$(md5_of "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符（$ACTUAL_MD5 != ${ORIG_MD5}）" >&2; exit 1; }

NEW_DEX_NUM="$("$PY" "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK")"
NEW_DEX="classes${NEW_DEX_NUM}.dex"
echo "  base dex 占用情况 → 新增 $NEW_DEX"

# 底包自检：不得已含星光外挂物 / 已占用新 dex；TapLoginForceWeb 白名单不得含本作包名
"$PY" - "$ORIG_APK" "$NEW_DEX" "$PKG" "$NIXIANG/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java" <<'PYCHK'
import sys, zipfile, os, re
apk, newdex, pkg, tapjava = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
with zipfile.ZipFile(apk) as z:
    names = set(z.namelist())
    bad = [n for n in (newdex, 'lib/arm64-v8a/libstarry_magic.so') if n in names]
    for n in ('classes.dex', 'resources.arsc'):
        if n not in names:
            raise SystemExit('ERROR: base APK missing %s' % n)
    nlib = sorted(n for n in names if n.startswith('lib/') and n.endswith('.so'))
if bad:
    raise SystemExit('ERROR: base APK is not official (already contains %s)' % bad)
print('  base APK is official; %s free; dex=%d; libs=%s'
      % (newdex, len([n for n in names if re.fullmatch(r'classes\d*\.dex', n)]), nlib))
if os.path.isfile(tapjava):
    t = open(tapjava, encoding='utf-8').read()
    if '"%s"' % pkg in t:
        raise SystemExit('ERROR: TapLoginForceWeb 包名白名单含 %s —— 本作禁止强制网页登录' % pkg)
    print('  TapLoginForceWeb 白名单不含 %s ✓（类随包但不生效）' % pkg)
else:
    print('  WARN: 未找到 TapLoginForceWeb.java，跳过白名单断言')
PYCHK

LOCK_DIR="/tmp/wor_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wor_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease（starry Java 桥）"
copy_tree() {  # $1=src $2=dst；有 rsync 用 rsync，否则用 tar 排除法
  if command -v rsync >/dev/null 2>&1; then
    rsync -a --delete \
      --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
      --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
      --exclude 'obj_*/' --exclude 'libs_*/' \
      "$1/" "$2/"
  else
    mkdir -p "$2"
    ( cd "$1" && tar -cf - \
        --exclude='./.gradle' --exclude='./app/build' --exclude='./build' \
        --exclude='./auth-server/.venv' --exclude='./auth-server/data' --exclude='./.idea' \
        --exclude='./obj_*' --exclude='./libs_*' . ) | ( cd "$2" && tar -xf - )
  fi
}
copy_tree "$STARRY_SRC" "$WORK/starry"

# ---- 机器本地属性改写（只改 $WORK 里的临时副本，不动仓库源文件）----
# 仓库里的 local.properties / gradle.properties 是 macOS 机器专属路径
# （sdk.dir=/Users/aliang/Library/Android/sdk、org.gradle.java.home=/Users/aliang/.../jdk-17.0.2.jdk）
# 在 Windows 上会让 Gradle 直接失败（Java home does not exist / SDK location not found）。
# gradle.properties 是 Java Properties：反斜杠是转义符 ⇒ 必须用正斜杠（cygpath -m）。
JH_WIN="$(cygpath -m "${JAVA_HOME:-}" 2>/dev/null || echo "${JAVA_HOME:-}")"; JH_WIN="${JH_WIN%/}"
SDK_WIN="$(cygpath -m "${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}" 2>/dev/null || echo "${ANDROID_SDK_ROOT:-}")"; SDK_WIN="${SDK_WIN%/}"
if [[ -n "$SDK_WIN" && "$SDK_WIN" != "/" ]]; then
  printf 'sdk.dir=%s\n' "$SDK_WIN" > "$WORK/starry/local.properties"
  echo "  local.properties → sdk.dir=$SDK_WIN"
fi
if [[ -f "$WORK/starry/gradle.properties" && -n "$JH_WIN" && "$JH_WIN" != "/" ]]; then
  "$PY" - "$WORK/starry/gradle.properties" "$JH_WIN" <<'PYGP'
import re, sys
p, jh = sys.argv[1], sys.argv[2]
t = open(p, encoding="utf-8").read()
if re.search(r'^\s*org\.gradle\.java\.home\s*=', t, re.M):
    t = re.sub(r'^\s*org\.gradle\.java\.home\s*=.*$', 'org.gradle.java.home=%s' % jh, t, flags=re.M)
else:
    t = t.rstrip("\n") + "\norg.gradle.java.home=%s\n" % jh
open(p, "w", encoding="utf-8").write(t)
print("  gradle.properties → org.gradle.java.home=%s" % jh)
PYGP
fi

# 镜像 + SDK 本地化（只改临时副本 $WORK/starry，不动仓库源文件）。
# 背景：项目 build.gradle 只有 google()+mavenCentral()，而 dl.google.com 在国内下大 jar 会挂在
# socket read 上（maven-metadata.xml 能 200，jar 下不来）⇒ Gradle daemon 全线程 Wait 不动。
# 另：项目写死 compileSdkVersion 35、且不写 buildToolsVersion（AGP 8.6.0 默认 34.0.0），
# 本机 SDK 只有 platforms/android-36 + build-tools/{35.0.0,36.1.0} 且无 cmdline-tools（装不了），
# AGP 会去 dl.google.com/android/repository 自动下载 ⇒ 卡在
# "Still waiting for package manifests to be fetched remotely."。回落即可。
"$PY" "$NIXIANG/04_scripts/_common/gradle_localize.py" "$WORK/starry" \
  --sdk "${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}" || exit 1

# gradle 入口：优先离线发行版（wor_env.sh 的 WOR_GRADLE_CMD），否则项目的 ./gradlew。
# 仓库两份 .gitignore 都含 '*.jar' ⇒ gradle-wrapper.jar 从未被跟踪，换机器会缺。
GRADLE_CMD="${WOR_GRADLE_CMD:-./gradlew}"
if [[ "$GRADLE_CMD" == "./gradlew" && ! -f "$WORK/starry/gradle/wrapper/gradle-wrapper.jar" ]]; then
  echo "ERROR: 既没有独立 gradle（01_tools/gradle/gradle-*/bin/gradle），也没有 gradle-wrapper.jar" >&2
  exit 1
fi
echo "  gradle: $GRADLE_CMD"

(cd "$WORK/starry" && "$GRADLE_CMD" assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/" 2>/dev/null || { mkdir -p "$SMALI_ROOT/com"; cp -R "$d/com/starry" "$SMALI_ROOT/com/"; }
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] \
  || { echo "ERROR: StarryInitProvider.smali missing（Java 桥没编进 starry app）" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/StarryAppComponentFactory.smali" ]] \
  || { echo "ERROR: StarryAppComponentFactory.smali missing" >&2; exit 1; }
# WOR 只断言三类（TapLoginForceWeb 不参与本作；类文件存在与否都不影响）
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] binary AndroidManifest（保留原 application=com.moba.unityplugin.MobaGameApplication）"
if [[ -n "${WOR_BINARY_MANIFEST:-}" && -f "$WOR_BINARY_MANIFEST" ]]; then
  cp "$WOR_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $WOR_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s --no-assets + b extract)"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  "$PY" - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
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
# 本作 application name=MobaGameUnityPlugin 系（com.moba.unityplugin.MobaGameApplication）：
# 不替换 Application，走 StarryInitProvider（ContentProvider.onCreate 早于 Application.onCreate 之后 SDK）
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
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
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (provider 注入, 保留原 Application)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  "$PY" -c 'import sys,zipfile;open(sys.argv[2],"wb").write(zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml"))' "$WORK/mf_only.apk" "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验（provider/service/权限 + 官方属性不变）"
AAPT2="${AAPT2:-}"
if [[ -z "$AAPT2" ]]; then
  # 优先用 wor_env.sh 定好的 build-tools；Windows 上是 aapt2.exe，mac/Linux 是 aapt2
  for cand in "${WOR_BUILD_TOOLS:-/nonexist}/aapt2.exe" \
              "${WOR_BUILD_TOOLS:-/nonexist}/aapt2" \
              "$HOME/Library/Android/sdk/build-tools"/*/aapt2 \
              "$HOME/Android/Sdk/build-tools"/*/aapt2 \
              "/c/Users/$WUSER/AppData/Local/Android/Sdk/build-tools"/*/aapt2.exe \
              "/c/Users/$WUSER/AppData/Local/Android/Sdk/build-tools"/*/aapt2 \
              "$(command -v aapt2 2>/dev/null || true)"; do
    [[ -n "$cand" && -x "$cand" ]] && { AAPT2="$cand"; break; }
  done
fi
if [[ -n "$AAPT2" && -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  "$PY" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 StarryInitProvider" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 MenuService" >&2; exit 1; }
  grep -q "com.ap.watcherofrealms.starryinit" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 的 provider authority 不是 $AUTHORITY" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 FOREGROUND_SERVICE_SPECIAL_USE" >&2; exit 1; }
  grep -q "com.moba.unityplugin.MobaGameApplication" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 丢了原 Application 类（应保留不替换）" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_N="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true)"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 重建丢失官方节点（$DIFF_D 行）" >&2; exit 1; }
  [[ "$DIFF_N" -ge 17 && "$DIFF_N" -le 24 ]] \
    || { echo "ERROR: manifest 新增节点数异常（$DIFF_N 行，期望 17~24）" >&2; exit 1; }
  echo "  manifest ok（新增 $DIFF_N 行，无删除）"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

echo "[5/6] zip surgical merge → $UNSIGNED"
"$PY" - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib, re
from pathlib import Path

src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

SIG_NAMES = {"META-INF/MANIFEST.MF"}

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names, "orig apk missing AndroidManifest.xml"
assert new_dex not in names, "orig apk already has %s" % new_dex
# untouched 动态生成：官方全部 classesN.dex + resources.arsc + （若在 base）native 壳与 Unity 资源
untouched = sorted(n for n in names if re.fullmatch(r"classes\d*\.dex", n))
untouched.append("resources.arsc")
for opt in ("lib/arm64-v8a/libil2cpp.so", "assets/Resources.dat"):
    if opt in names:
        untouched.append(opt)
for n in untouched:
    assert n in names, f"orig apk missing {n}"

def is_sig(name: str) -> bool:
    if name in SIG_NAMES:
        return True
    if not name.startswith("META-INF/"):
        return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name):
        continue
    if name in extras or name == new_dex:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

info = zipfile.ZipInfo(new_dex)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, cnew)
n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
for name in ("META-INF/services/kotlinx.coroutines.CoroutineExceptionHandler",
             "META-INF/services/kotlinx.coroutines.internal.MainDispatcherFactory"):
    if name in zin2.namelist():
        assert name in zout2.namelist(), f"lost {name}"
assert new_dex in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist()), "signature leftovers"
assert "META-INF/MANIFEST.MF" not in zout2.namelist()
print(f"  merged ok; entries≈{n}; {new_dex}={len(cnew)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[5b/6] split 去签名 → $SPLIT_UNSIGNED"
"$PY" - "$SPLIT_APK" "$SPLIT_UNSIGNED" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, dst = Path(sys.argv[1]), Path(sys.argv[2])
SIG_EXACT = {"stamp-cert-sha256"}


def is_sig(name):
    if name in SIG_EXACT:
        return True
    if not name.startswith("META-INF/"):
        return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))


zin = zipfile.ZipFile(src, "r")
names = zin.namelist()
so_before = sorted(n for n in names if n.endswith(".so"))
if not so_before:
    raise SystemExit("ERROR: split 里没有任何 .so —— 拿错文件了？")
if "lib/arm64-v8a/libil2cpp.so" not in names:
    raise SystemExit("ERROR: split 缺 libil2cpp.so")
if "AndroidManifest.xml" not in names:
    raise SystemExit("ERROR: split 缺 AndroidManifest.xml")

zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    if is_sig(item.filename) or item.filename == "META-INF/MANIFEST.MF":
        continue
    info = zipfile.ZipInfo(item.filename, item.date_time)
    # 逐条保留原压缩方式：extractNativeLibs=true 下 deflated 的 .so 是官方原样，别改
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, zin.read(item))
    n += 1
zin.close()
zout.close()

a = zipfile.ZipFile(src, "r")
b = zipfile.ZipFile(dst, "r")
out_names = b.namelist()
for name in so_before:
    assert hashlib.sha256(a.read(name)).digest() == hashlib.sha256(b.read(name)).digest(), "changed: %s" % name
assert sorted(n for n in out_names if n.endswith(".so")) == so_before, "split .so 集合被改动"
assert not any(is_sig(x) or x == "META-INF/MANIFEST.MF" for x in out_names), "signature leftovers"
assert "AndroidManifest.xml" in out_names, "lost AndroidManifest.xml"
a.close()
b.close()
print("  split ok; entries=%d (so=%d); 官方签名与 stamp-cert-sha256 已剥离"
      % (n, len(so_before)))
PY

echo "[6/6] 闭环校验（包内 so == 构建产物；$NEW_DEX 含星光三类）"
zipcat() { "$PY" -c 'import sys,zipfile;sys.stdout.buffer.write(zipfile.ZipFile(sys.argv[1]).read(sys.argv[2]))' "$1" "$2"; }
IN_SO_MD5="$(zipcat "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5_stdin)"
SRC_SO_MD5="$(md5_of "$SO_PREBUILT")"
if [[ "$IN_SO_MD5" != "$SRC_SO_MD5" ]]; then
  echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SRC_SO_MD5)——旧 so 混包，禁止交付" >&2
  exit 1
fi
echo "[so-verify] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"
SO_TAG_N="$(zipcat "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "[Wor]" || true)"
[[ "$SO_TAG_N" -ge 1 ]] || { echo "ERROR: 包内 so 缺 [Wor] 标签（是不是拿了国服 [Cxswz] 的 so？）" >&2; exit 1; }
SO_CN_N="$(zipcat "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "[Cxswz]" || true)"
[[ "$SO_CN_N" == "0" ]] || { echo "ERROR: 包内 so 含 [Cxswz]（国服 so 混入本作包）" >&2; exit 1; }
SO_MENU_N="$(zipcat "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "Slider_伤害倍率" || true)"
[[ "$SO_MENU_N" -ge 1 ]] || { echo "ERROR: 包内 so 缺菜单串" >&2; exit 1; }
SO_LIB_N="$(zipcat "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "liblogic.so" || true)"
[[ "$SO_LIB_N" -ge 1 ]] || { echo "ERROR: 包内 so 缺 liblogic.so 等待库名" >&2; exit 1; }

rm -rf "$WORK/c6_check"
zipcat "$UNSIGNED" "$NEW_DEX" > "$WORK/${NEW_DEX%.dex}_check.dex"
"$BAKSMALI_BIN" d -o "$WORK/c6_check" "$WORK/${NEW_DEX%.dex}_check.dex" >/dev/null
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali \
         com/starry/magic/sign/StarryAppComponentFactory.smali; do
  [[ -f "$WORK/c6_check/$f" ]] || { echo "ERROR: $NEW_DEX 缺 ${f}（smali 未打进包）" >&2; exit 1; }
done
# 2026-10-28：运行时签名伪装已删除 ⇒ 不再断言"dex 里有 WOR 证书常量"。
# 本作要过壳验签须按改包法在游戏 dex 侧处理（见 skill/re-analyze/login/taptap-web-login.md）。
echo "[dex-verify] $NEW_DEX 含 StarryInitProvider / MenuService / StarryAppComponentFactory ✓"

ls -lh "$UNSIGNED" "$SPLIT_UNSIGNED"
echo "DONE(unsigned base) : $UNSIGNED"
echo "DONE(unsigned split): $SPLIT_UNSIGNED"
echo "NOTE: 两份都未签名；由 04_scripts/境界守望者/wor_sign.sh **用同一把钥匙一起签**，"
echo "      再 wor_install.sh 安装（必须 install-multiple 一起装；证书不一致会 Inconsistent certificates）。"
