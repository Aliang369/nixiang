#!/usr/bin/env bash
# 明日方舟：终末地 1.6.4（versionCode 61）— 外科组装（禁止 apktool b 整包游戏）
#
# 复刻依据：scripts/surgical_endfield_1.5.3.sh（2026-09-04 已验收四功能）
#           + 10_docs/终末地/ 既有分析（抓点_四功能 / 初判_1.5.3 / ACE 过检 SOP）
#
# 1.6.4 差异复核（2026-10-16，逐项对照 1.5.3）：
#   · dex 布局同构：classes.dex~classes6.dex（classes6 = 720B 占位）→ 空闲下标 7
#   · ACE so 字节级同尺寸：libtersafe2 5,609,568 / libtprt 1,879,688（引擎未升级）
#   · Application 仍 com.u8.sdk.U8Application；ACE GP6Service/GP7Service/GP7Worker 均在
#   · global-metadata.dat 头仍 94 43 72 12（同款加密，71MB）；Unity 仍 2021.3.34f5
#   · 官方签名证书 MD5 1cf808541a17bf218d3dd5a30787e485 = SignSpoof 已嵌值（无需改 Java）
#   → 1.5.3 的「Provider 注入 + 外科组装 + classes7 + SignSpoof + TapLoginForceWeb」原样适用
#
# 非 1.5.3 的两处现代化（2026-10-15 m00007 铁律，不碰共享文件）：
#   · 构建走私有 Android_mrfzzmd.mk（modern 注册表），不再切公共 Android.mk
#   · StarryInitProvider 插到**首个 provider 之前**（skill/starry-mod 入口铁律：
#     Provider 顺序＝平台实例化顺序，须先回灌官方证书再让 TapSDK 读）
#
# - 保留官方 dex / resources.arsc / ACE so 字节；新增 classes{N}.dex = com/starry
# - StarryApp .super → U8Application；【不替换 Application】（ACE 校验类名，换即毒丸黑屏）
# - 产物：08_output/终末地/终末地_1.6.4_surgical-releaseSigned.apk
#
# 用法:
#   ENDFIELD_BASE_APK=/tmp/x/终末地_1.6.4_base.apk bash scripts/surgical_endfield_1.6.4.sh
#   STARRY_TRACE=1 bash scripts/surgical_endfield_1.6.4.sh            # 带 StarryTrace
#   STARRY_INSTALL=1 STARRY_CLEAN_INSTALL=1 SERIAL=f9a116a2 bash ...  # 异签名：卸载后装
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

VERSION="1.6.4"
VC="61"
TAG_MK="Android_mrfzzmd.mk"
TAG_MK_TAG="mrfzzmd"
SO_LABEL="[Endfield]"
AUTH_ID="mrfzzmd"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="${ENDFIELD_BASE_APK:-$NIXIANG/02_apks/终末地/终末地_${VERSION}_base.apk}"
# ENDFIELD_OUT 可把产物指到临时区做 A/B（默认 08_output/{游戏}，避免验证性构建误覆盖正式包）
OUT="${ENDFIELD_OUT:-$NIXIANG/08_output/终末地}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hypergryph.endfield"
OFFICIAL_APP="Lcom/u8/sdk/U8Application;"
SIGNED="$OUT/终末地_${VERSION}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/终末地_${VERSION}_surgical-debugSigned.apk"
[[ "${STARRY_TRACE:-0}" == "1" ]] && SIGNED="$OUT/终末地_${VERSION}_surgical-trace-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/终末地/surgical_${VERSION}/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no build-tools found"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK (set ENDFIELD_BASE_APK)"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS"; exit 1; }

# ---- dex 目标自动探测（禁止硬编码 classesN：换代会改 dex 数量与布局）----
DEX_N="$(python3 "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK")"
case "$DEX_N" in ''|*[!0-9]*) echo "ERROR: bad dex index '$DEX_N'"; exit 1;; esac
DEX_NAME="classes${DEX_N}.dex"

# ---- 私有 mk 自检（现代入口合同）----
MK="$STARRY_SRC/app/src/main/jni/$TAG_MK"
[[ -f "$MK" ]] || { echo "ERROR: missing $MK"; exit 1; }
grep -E "STARRY_GAME_ENDFIELD" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must define STARRY_GAME_ENDFIELD"; exit 1; }
grep -E "Games/EndfieldMod\.cpp" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must compile Games/EndfieldMod.cpp"; exit 1; }
grep -E "Games/EndfieldEntry\.cpp" "$MK" >/dev/null \
  || { echo "ERROR: $TAG_MK must compile Games/EndfieldEntry.cpp"; exit 1; }
echo "[check] $TAG_MK target=endfield dex=$DEX_NAME"

LOCK_DIR="/tmp/endfield_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/endfield_surgical.XXXXXX)"
trap 'if [[ "${ENDFIELD_KEEP_WORK:-0}" == "1" ]]; then echo "KEEP WORK=$WORK"; else rm -rf "$WORK"; fi; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ------------------------------------------------------------------
# 装机段：构建路径与「只装机」路径共用同一份逻辑（避免两处漂移）
# ------------------------------------------------------------------
endfield_install() {
    SERIAL_ARG=()
    [[ -n "${SERIAL:-}" ]] && SERIAL_ARG=(-s "$SERIAL")
    REMOTE="/data/local/tmp/endfield_${VERSION//./}_surgical.apk"
    APPEXT="/sdcard/Android/data/${PKG}"
    RESERVE="/sdcard/Android/data/${PKG}.nxreserve"
    adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" push "$SIGNED" "$REMOTE"

    if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
      echo "[install] CLEAN uninstall（异签名必须；本轮已获用户授权卸载重装）"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell am force-stop "$PKG" 2>/dev/null || true

      # ---- 外置资源保护（2026-10-16 实测 ${PKG}/files/VFS = 29,467 MB）----
      # 卸载可能连带删 /sdcard/Android/data/<pkg>；同卷 rename 瞬时完成、零额外空间。
      # **先落备胎并核对非空才继续**：备胎失败即中止，绝不裸卸（29GB 重下代价过高）。
      echo "[install] 保护外置资源目录 → $RESERVE"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell \
        "su -c 'rm -rf \"$RESERVE\"; if [ -d \"$APPEXT\" ]; then mv \"$APPEXT\" \"$RESERVE\"; fi; ls -d \"$RESERVE\" 2>/dev/null'" \
        > "$WORK/reserve_check.txt" 2>&1 || true
      cat "$WORK/reserve_check.txt"
      if grep -q "$RESERVE" "$WORK/reserve_check.txt"; then
        echo "[install] 备胎就位（mv 成功）"
      else
        echo "[WARN] 外置资源目录不存在或 mv 失败 → 卸载会丢 29GB 热更资源"
        if [[ "${ENDFIELD_ALLOW_RESOURCE_LOSS:-0}" == "1" ]]; then
          echo "[WARN] ENDFIELD_ALLOW_RESOURCE_LOSS=1 → 按用户显式授权继续"
        else
          echo "       已中止卸载。确认可丢资源时置 ENDFIELD_ALLOW_RESOURCE_LOSS=1 重跑"
          exit 1
        fi
      fi

      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'pm uninstall $PKG'" || true
    else
      echo "[install] cover install -r -d (no uninstall)"
      adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell am force-stop "$PKG" 2>/dev/null || true
    fi

    adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
      || adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" install -r -d "$SIGNED"

    # ---- 资源归位：重装后 app uid 可能变，必须 chown 到新 uid，否则 app 读不到自己的外置目录 ----
    if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
      NEW_UID="$(adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell "su -c 'stat -c %u /data/data/$PKG 2>/dev/null'" 2>/dev/null | tr -d '\r' | head -1)"
      case "$NEW_UID" in ''|*[!0-9]*) echo "[WARN] 取不到新 uid（'$NEW_UID'）→ 跳过归位，需人工处理"; NEW_UID="";; esac
      if [[ -n "$NEW_UID" ]]; then
        echo "[install] 资源归位 → $APPEXT (uid=$NEW_UID)"
        adb "${SERIAL_ARG[@]+"${SERIAL_ARG[@]}"}" shell \
          "su -c 'R=\"$RESERVE\"; T=\"$APPEXT\"; if [ ! -d \"\$R\" ]; then echo NO_RESERVE; elif [ -d \"\$T/files/VFS\" ]; then echo TARGET_ALREADY_POPULATED; else rm -rf \"\$T\"; mv \"\$R\" \"\$T\"; chown -R $NEW_UID:$NEW_UID \"\$T\"; restorecon -R \"\$T\" 2>/dev/null; du -sm \"\$T\" 2>/dev/null; echo RESTORED; fi'"
      fi
    fi

    bash "$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh" || true
    echo "log: adb logcat -s StarryApp:V StarryMagic:V StarryTrace:V Mod_Menu:V MenuService:V"
}

# ---- 只装机模式：复用现有正式包（不重建）----
# 约定见 skill/starry-mod/SKILL.md「存量游戏快速重打」：STARRY_INSTALL_ONLY=1
# 语义 = **只做验证 + 安装**；同时也避免重建覆盖已取证的正式包（重签会改 md5）。
if [[ "${STARRY_INSTALL_ONLY:-0}" == "1" ]]; then
  [[ -f "$SIGNED" ]] || { echo "ERROR: STARRY_INSTALL_ONLY=1 但正式包不存在：$SIGNED"; exit 1; }
  echo "[install-only] 复用正式包（不重建）：$SIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" > "$WORK/signer.txt" 2>&1 || true
  grep -q "CN=StarryMagic" "$WORK/signer.txt" || { echo "ERROR: 正式包签名身份不是 StarryMagic"; cat "$WORK/signer.txt"; exit 1; }
  unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/io.so"
  grep -a -F -q "$SO_LABEL" "$WORK/io.so" || { echo "ERROR: 包内 so 不是 ENDFIELD"; exit 1; }
  grep -a -F -q "$AUTH_ID" "$WORK/io.so" || { echo "ERROR: 包内 so 缺 game_id"; exit 1; }
  unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/io.dex"
  echo "  so md5=$(md5 -q "$WORK/io.so")  $DEX_NAME md5=$(md5 -q "$WORK/io.dex")"
  echo "  apk md5=$(md5 -q "$SIGNED")  size=$(stat -f %z "$SIGNED")"
  endfield_install
  echo "DONE(install-only): $SIGNED"
  exit 0
fi


echo "[1/6] ndk-build ($TAG_MK) -> libs_${TAG_MK_TAG} (STARRY_TRACE=${STARRY_TRACE:-0})"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
NDK_EXTRA=()
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
  NDK_EXTRA+=(STARRY_TRACE=1)
fi
OBJ_OUT="$STARRY_SRC/obj_${TAG_MK_TAG}"
LIBS_OUT="$STARRY_SRC/libs_${TAG_MK_TAG}"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./"$TAG_MK" NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT="$OBJ_OUT" NDK_LIBS_OUT="$LIBS_OUT" \
  "${NDK_EXTRA[@]+"${NDK_EXTRA[@]}"}" -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$LIBS_OUT/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -F -q "$SO_LABEL" "$SO_BUILT" || ! grep -a -F -q "$AUTH_ID" "$SO_BUILT"; then
  echo "ERROR: so 不是 ENDFIELD"
  strings "$SO_BUILT" | grep -E 'Endfield|mrfzzmd|Wggls|\[Wggls\]|gjc2' | sort -u | head -20
  exit 1
fi
# 标签一律按**字面量**比（-F）：写成 grep '[Wggls]' 是字符类，会匹配任意单个 W/g/l/s
# → 正查假通过、反查假失败（2026-10-16 本轮实测踩到）
for bad in '[Wggls]' '[Gjc2]' '[Dwct]' '[Cstjjs]' '[Hzsj]' '[Wxsj]'; do
  if grep -a -F -q -- "$bad" "$SO_BUILT"; then
    echo "ERROR: so 仍含上一款标签 $bad"; exit 1
  fi
done
# 菜单串字节自检（中文必须按字节搜：strings 默认 ASCII 不认 UTF-8）
for menu in '百倍伤害' '无限跳跃' '无限技能'; do
  if ! LC_ALL=C grep -aq "$menu" "$SO_BUILT"; then
    echo "ERROR: so 缺菜单串 $menu"; exit 1
  fi
done
# OTA 守卫判据：so 内**必须**含构建戳。缺戳 ⇒ Java 侧 apkStamp=0 ⇒ 覆盖安装后永不清 OTA so
# ⇒ 玩家继续跑旧 OTA so（2026-10-16 小米 byron 实测：新包装上了、跑的却是旧 so，一开就崩）。
STAMP="$(grep -a -o 'STARRYSTAMP1:EPOCH=[0-9]*' "$SO_BUILT" | head -1)"
if [ -z "$STAMP" ]; then
  echo "ERROR: so 缺构建戳（STARRYSTAMP1:EPOCH=）—— OTA 守卫会失效"
  echo "       检查 Android_mrfzzmd.mk 是否带 -Wl,--undefined=kStarryBuildStamp"
  exit 1
fi
echo "  so 构建戳 ${STAMP}（OTA 守卫可判新旧）"
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ($SO_LABEL/$AUTH_ID) md5=$(md5 -q "$SO_BUILT")"

echo "[1b/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -F -q "$SO_LABEL" "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 ENDFIELD"; exit 1; }

echo "[2/6] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] $DEX_NAME = com/starry (StarryApp → U8Application)"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/u8/sdk/U8Application;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/u8/sdk/U8Application;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/u8/sdk/U8Application;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/u8/sdk/U8Application;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/u8/sdk/U8Application;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing U8Application")
supers = [ln for ln in t.splitlines() if ln.startswith(".super ")]
if not supers or supers[0].strip() != ".super Lcom/u8/sdk/U8Application;":
    raise SystemExit(f"ERROR: bad .super: {supers[:3]}")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → U8Application ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/$DEX_NAME" "$SMALI_ROOT"
ls -la "$WORK/$DEX_NAME"

# TapTap 强制网页登录改走运行时方案（TapLoginForceWeb + SignSpoof）：
# ACE 校验官方 dex 完整性，classes2/3 一字节都不能动（实证闪退）。

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
# 官方包无 libc++_shared.so（2026-10-16 实证），新 so 的 DT_NEEDED 需要它 → 必须注入
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest (mini apktool，不对原包 apktool b)"
if [[ -n "${ENDFIELD_BINARY_MANIFEST:-}" && -f "${ENDFIELD_BINARY_MANIFEST:-}" ]]; then
  cp "$ENDFIELD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name)
    data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 替换官方 Application 会叠加第二个 android:name；且 20260902 实测终末地 ACE
# initialize 校验 Manifest Application 类名，换成 StarryApp 会 native 信号量毒丸挂起
# （黑屏 Splash）。因此【不替换 Application】，改由 StarryInitProvider 自动初始化。
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''<provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.hypergryph.endfield.starryinit"
            android:exported="false" />
        '''
    # 入口铁律：Provider 顺序＝平台实例化顺序，必须排在其它 Provider 之前
    # （TapTapKitInitProvider 等要先拿到回灌的官方证书）。取首个 <provider 之前插入。
    idx = t.find("<provider")
    if idx > 0:
        t = t[:idx] + prov + t[idx:]
    else:
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
print("  text Manifest patched (StarryInitProvider first + MenuService, Application 保留)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge (keep official dex + arsc, add $DEX_NAME)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$DEX_NAME" <<'PY'
import sys, zipfile, hashlib, re
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
dex_name = sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
c7 = (work / dex_name).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_official_dex(name: str) -> bool:
    """官方 classes*.dex 一律原型保留（ACE 校验 dex 完整性）"""
    if name == "classes.dex":
        return True
    m = re.fullmatch(r"classes(\d+)\.dex", name)
    return bool(m) and name != dex_name

def is_protected(name: str) -> bool:
    return name == "resources.arsc" or is_official_dex(name)

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == dex_name:
        continue
    if name == "AndroidManifest.xml":
        data = mf
    else:
        data = zin.read(item)
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
    seen.add(name)
    n += 1

info = zipfile.ZipInfo(dex_name)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c7)

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
checked = 0
for name in zin2.namelist():
    if is_protected(name):
        h1 = hashlib.sha256(zin2.read(name)).hexdigest()
        h2 = hashlib.sha256(zout2.read(name)).hexdigest()
        assert h1 == h2, f"PROTECTED MUTATED: {name}"
        checked += 1
print(f"  merged ok; entries≈{n}; {dex_name}={len(c7)} bytes; protected keep={checked} sha256 match")
zin2.close(); zout2.close()
PY

# Manifest 复读门禁（黑屏三件事之二）：必须从**二进制** Manifest 复读。
# ⚠️ 禁止 `aapt2 ... | grep -q`：grep -q 命中即退出 → aapt2 吃 SIGPIPE(141)，
#    在 set -o pipefail 下整条管道判失败（2026-10-16 本轮实测假失败）。
#    先落盘再 grep，绕开管道退出码语义。
"$BT/aapt2" dump xmltree --file AndroidManifest.xml "$WORK/unsigned.apk" > "$WORK/mf_dump.txt" 2>/dev/null || true
grep -q "com.starry.magic.StarryInitProvider" "$WORK/mf_dump.txt" \
  || { echo "ERROR: StarryInitProvider 未写进 Manifest"; exit 1; }
grep -q "com.starry.magic.menu.MenuService" "$WORK/mf_dump.txt" \
  || { echo "ERROR: MenuService 未写进 Manifest"; exit 1; }
echo "  Manifest 复读 ok: StarryInitProvider + MenuService 均在包内"

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true

# ---- 成品自检（从成品包重新解出，不信中间产物）----
echo "== 成品自检 =="
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
grep -a -F -q "$SO_LABEL" "$WORK/final.so" || { echo "ERROR: final so 标签错"; exit 1; }
grep -a -F -q "$AUTH_ID" "$WORK/final.so" || { echo "ERROR: final so 缺 game_id"; exit 1; }
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/final.dex"
python3.11 - "$WORK/final.dex" <<'PY'
import sys
from pathlib import Path
d = Path(sys.argv[1]).read_bytes()
need = ["Lcom/starry/magic/StarryInitProvider;", "Lcom/starry/magic/menu/MenuService;"]
miss = [n for n in need if n.encode() not in d]
if miss:
    raise SystemExit("ERROR: final dex 缺类: " + ", ".join(miss))
print("  final dex 类自检 ok (StarryInitProvider / MenuService)")
PY
"$BT/aapt2" dump badging "$SIGNED" > "$WORK/badging.txt" 2>/dev/null || true
grep -E "^package" "$WORK/badging.txt" || true
echo "  so md5=$(md5 -q "$WORK/final.so")  $DEX_NAME md5=$(md5 -q "$WORK/final.dex")"
ls -lh "$SIGNED"
md5 -q "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  endfield_install
fi

echo "DONE: $SIGNED"
