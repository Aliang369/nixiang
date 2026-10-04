#!/usr/bin/env bash
# 伊莫 1.0.1 (com.x.yimoo.official, FunPlus + TapTap 渠道, vc 4) — 外科 zip 组装（无 root 直装星光包）
#
# 注入方式：ContentProvider（StarryInitProvider），【不替换 Application】
#   官方 Application = com.netease.nis.wrapper.MyApplication（网易易盾加固壳，替换会触发校验）
#   官方 appComponentFactory 保持原样
#
# 注入 dex：官方只有 classes.dex → 星光 dex 落在 classes2.dex（脚本动态推算）
# lib/arm64-v8a：新增 libstarry_magic.so（Yimo 模块：精灵攻击秒杀 / 移速 / 玩家无敌 / 无限跳跃 /
#   无限体力 / 瞬移采集，xLua 玩法层注入 —— Dobby hook libxlua.so 的 lua_pcall / lua_gettop）
#   ★官方包内**没有** libc++_shared.so，而 libstarry_magic.so 的 DT_NEEDED 含 libc++_shared.so
#     ⇒ 本脚本必须额外注入 lib/arm64-v8a/libc++_shared.so（取自同一次 ndk-build 产物）
# 官方 classes*.dex / resources.arsc 原样保留（resources.arsc 必须 STORED）
#
# 底包：09_test/伊莫/orig/yimoo_orig.apk（默认；可用 STARRY_ORIG_APK 覆盖）
#   385428106 B / 1 个 dex / 官方签名 C=ST=L=O=OU=CN=FUNPLUS
#   SHA-256 4657fa903c8dab5f2e5a60c53d872e90e031d3db491a604ea7bc6960bf311510
#   → 已烘焙进 app/src/main/java/com/starry/magic/sign/SignSpoof.java 的 YIMO_CERT_B64
#
# 构建：modern 注册表（Android_yimo.mk + MainModern.cpp + Games/YimoMod.cpp + Games/YimoEntry.cpp
#       + Games/NoAceStubs.cpp 兜底，官方包无 libtprt.so ⇒ B 类）
#
# 用法:
#   bash scripts/surgical_yimo_1.0.1.sh
#   STARRY_INSTALL=1 bash scripts/surgical_yimo_1.0.1.sh
#   STARRY_ALLOW_UNINSTALL=1 ...        （签名不一致需卸载重装时显式给）
#   STARRY_DIAG=1 ...                   （诊断包：DLOG/诊断 Lua chunk 进 so，
#                                       产物名带 -diag，与正式包并存不覆盖 alias）
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
VER="1.0.1"
PKG="com.x.yimoo.official"
TAG="Yimo"
GAME="伊莫"

# 底包解析：优先 02_apks/{游戏}/ 规范位，回落 09_test/{游戏}/orig/ 本轮拉取位
if [[ -n "${STARRY_ORIG_APK:-}" ]]; then
  ORIG_APK="$STARRY_ORIG_APK"
elif [[ -f "$NIXIANG/02_apks/${GAME}/${GAME}_${VER}_taptap_official_from_device.apk" ]]; then
  ORIG_APK="$NIXIANG/02_apks/${GAME}/${GAME}_${VER}_taptap_official_from_device.apk"
else
  ORIG_APK="$NIXIANG/09_test/${GAME}/orig/yimoo_orig.apk"
fi

OUT="$NIXIANG/08_output/${GAME}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
DIAG_SUFFIX=""
DIAG_MODE=0
# STARRY_KEEP_DATA=1：签名不一致必须卸载时用 `pm uninstall -k` 保住应用数据与热更资源
# （2026-09-30 实测 pm uninstall -k 会保留 /sdcard/Android/data/<pkg>）
if [[ "${STARRY_DIAG:-0}" == "1" ]]; then DIAG_SUFFIX="-diag"; DIAG_MODE=1; fi
SIGNED="$OUT/${GAME}_${VER}_surgical${DIAG_SUFFIX}-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/${GAME}_${VER}_starry.apk"
MF_CACHE="$NIXIANG/07_patched/${GAME}/surgical_${VER}/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: missing $NDK/ndk-build"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android_yimo.mk"
grep -q 'Games/YimoMod.cpp' "$MK" || { echo "ERROR: Android_yimo.mk 缺 Games/YimoMod.cpp"; exit 1; }
grep -q 'Games/YimoEntry.cpp' "$MK" || { echo "ERROR: Android_yimo.mk 缺 Games/YimoEntry.cpp"; exit 1; }
grep -q 'Games/NoAceStubs.cpp' "$MK" || { echo "ERROR: Android_yimo.mk 缺 Games/NoAceStubs.cpp（MainModern.cpp 引用 SignalQuarantine_Start/FgoBoot_InjectEarlyFromOnLoad）"; exit 1; }
echo "[check] orig apk = $ORIG_APK"

# 官方包内是否自带 libc++_shared.so（伊莫：无 → 必须随包注入）
HAS_CXXSHARED="$(python3.11 - "$ORIG_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
print("1" if "lib/arm64-v8a/libc++_shared.so" in z.namelist() else "0")
PY
)"
echo "[check] 官方包自带 libc++_shared.so = ${HAS_CXXSHARED}（0 ⇒ 本次注入）"

LOCK_DIR="/tmp/yimo_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held"; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/yimo_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# 注入 dex 名 = 官方最大 classesN + 1（伊莫官方只有 classes.dex）
INJECT_DEX="$(python3.11 - "$ORIG_APK" <<'PY'
import re, sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
nums = []
for n in z.namelist():
    m = re.fullmatch(r'classes(?:(\d+))?\.dex', n)
    if m:
        nums.append(int(m.group(1) or 1))
print(f"classes{max(nums) + 1}.dex")
PY
)"
echo "[check] 官方 dex 最大序号 → 注入 $INJECT_DEX"

echo "[1/6] ndk-build（Android_yimo.mk → libs_yimo/，绝对路径）STARRY_DIAG=$DIAG_MODE"
NDK_DIAG_ARGS=()
# ★ 诊断模式必须用**独立的 obj 目录**：ndk-build 不会因 CFLAGS 变化重编已有 .o，
#   两种模式共用 obj 会让「诊断包静默退化成正式包」（2026-09-30 实测：so 内无诊断 chunk）。
NDK_OBJ_DIR="$STARRY_SRC/obj_yimo"
if [[ "$DIAG_MODE" == "1" ]]; then
  NDK_DIAG_ARGS+=(STARRY_DIAG=1)
  NDK_OBJ_DIR="$STARRY_SRC/obj_yimo_diag"
fi
# 注意：macOS bash 3.2 + set -u 下**空数组**的 "${arr[@]}" 会报 unbound variable，
# 必须写成 ${arr[@]+"${arr[@]}"}（本仓库踩过同类坑，见 _state/规范优化_20260930_*
# 与会话复盘 P1-3）。
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_yimo.mk \
  NDK_APPLICATION_MK=./Application.mk APP_ABI=arm64-v8a \
  NDK_OUT="$NDK_OBJ_DIR" NDK_LIBS_OUT="$STARRY_SRC/libs_yimo" \
  ${NDK_DIAG_ARGS[@]+"${NDK_DIAG_ARGS[@]}"} \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" 2>&1 | grep -v 'Bad file descriptor' || true)
SO_BUILT="$STARRY_SRC/libs_yimo/arm64-v8a/libstarry_magic.so"
CXX_BUILT="$STARRY_SRC/libs_yimo/arm64-v8a/libc++_shared.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q "\[$TAG\]" "$SO_BUILT" || { echo "ERROR: so 无 [$TAG] 标签（仍是上一款游戏）"; exit 1; }
for stale in '\[Jjdm\]' '\[Cxswz\]' '\[Gyzw\]' '\[Hxwj\]' '\[Sjzz\]' '\[Blfx\]'; do
  grep -a -q "$stale" "$SO_BUILT" && { echo "ERROR: so 仍含上一款游戏标签 $stale"; exit 1; }
done
CERT_N=$(grep -a -c 'BEGIN CERTIFICATE' "$SO_BUILT" || true)
[[ "${CERT_N:-0}" -ge 1 ]] || { echo "ERROR: so 内无烘焙证书（授权 fail-closed）"; exit 1; }
for m in '精灵攻击秒杀' '移速' '玩家无敌' '无限跳跃' '无限体力' '瞬移采集'; do
  N=$(LC_ALL=C grep -a -c "$m" "$SO_BUILT" || true)
  [[ "${N:-0}" -ge 1 ]] || { echo "ERROR: so 内缺菜单串 $m"; exit 1; }
done
# 干净包门禁（P0-3，2026-09-30 伊莫复盘）：正式包必须零诊断代码/零日志通道
if [[ "$DIAG_MODE" != "1" ]]; then
  bash "$NIXIANG/04_scripts/_common/check_release_clean.sh" --so "$SO_BUILT" \
    || { echo "ERROR: 干净包门禁未过 —— 诊断代码/日志通道泄漏进正式包（应包进 #ifdef STARRY_DIAG）"; exit 1; }
  if grep -a -q 'moddiag\.log' "$SO_BUILT"; then
    echo "ERROR: 正式包 so 内出现诊断 Lua chunk 标记（moddiag.log）"; exit 1
  fi
  echo "  [clean] 干净包门禁 ok（无诊断符号 / 无日志通道）"
else
  echo "  [diag] 诊断包：跳过干净包门禁"
  if grep -a -q 'moddiag\.log' "$SO_BUILT"; then
    echo "  [diag] so 内已含诊断 chunk 标记 moddiag.log ok"
  elif grep -rq 'MOD_DIAG_LUA_TEXT' "$STARRY_SRC/app/src/main/jni/Games/${TAG}"*.cpp 2>/dev/null; then
    # 源码声明了要用诊断 chunk ⇒ so 里没有就是构建没生效（换模式没换 obj 目录是常见死因）
    echo "ERROR: 源码引用了 MOD_DIAG_LUA_TEXT，但 so 内无诊断 chunk —— 多半是 obj 目录串模式，"
    echo "       删掉 \$NDK_OBJ_DIR（${NDK_OBJ_DIR}）重跑 STARRY_DIAG=1"; exit 1
  else
    echo "  [diag] 提示：so 内未见诊断 chunk（本游戏未接入 ModDiagLua，可忽略）"
  fi
fi
echo "  so ok: $(md5 -q "$SO_BUILT") $(stat -f%z "$SO_BUILT") B (tag=$TAG cert=$CERT_N 中文串 ok)"
mkdir -p "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_BUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
if [[ "$HAS_CXXSHARED" == "0" ]]; then
  [[ -f "$CXX_BUILT" ]] || { echo "ERROR: missing ${CXX_BUILT}（官方无 libc++_shared.so，必须随包注入）"; exit 1; }
  cp -f "$CXX_BUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libc++_shared.so"
  echo "  + 注入 libc++_shared.so ($(stat -f%z "$CXX_BUILT") B)"
fi

echo "[2/6] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q "\[$TAG\]" "$WORK/check.so" || { echo "ERROR: gradle APK 内 so 不是 $TAG"; exit 1; }
[[ "$(md5 -q "$WORK/check.so")" == "$(md5 -q "$SO_BUILT")" ]] \
  || { echo "ERROR: gradle APK 内 so 与本次构建产物 md5 不一致（旧 so 混包）"; exit 1; }
echo "  gradle APK so verified [$TAG] + md5 == 构建产物"

echo "[3/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[4/6] 收集 com/starry smali → ${INJECT_DEX}（Manifest 不替换 Application，StarryApp.super 保持 android.app.Application）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -d "$SMALI_ROOT/com/starry/magic" ]] || { echo "ERROR: com/starry/magic smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] || { echo "ERROR: StarryInitProvider.smali missing"; exit 1; }
# 签名伪装自检：com.x.yimoo.official 证书常量必须已在 SignSpoof 里（smali 里包名是点号形式）
SS_SMALI="$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali"
[[ -f "$SS_SMALI" ]] || { echo "ERROR: $SS_SMALI missing（R8 改名或 SignSpoof 未参与编译）"; ls "$SMALI_ROOT/com/starry/magic/sign/" || true; exit 1; }
if ! grep -aq -e "$PKG" -e "${PKG//./\/}" "$SS_SMALI"; then
  echo "ERROR: SignSpoof.smali 内无 $PKG 证书常量（先补 SignSpoof.java 的 YIMO_PKG/YIMO_CERT_B64 再 gradle）"
  grep -ao 'const-string[^,]*, "[A-Za-z0-9+/=]\{40,\}"' "$SS_SMALI" | head -3
  exit 1
fi
if ! grep -aq -e "$PKG" -e "${PKG//./\/}" "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali"; then
  echo "ERROR: TapLoginForceWeb.smali 白名单缺 $PKG"
  exit 1
fi
"$SMALI_BIN" assemble -o "$WORK/$INJECT_DEX" "$SMALI_ROOT"
ls -la "$WORK/$INJECT_DEX"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
if [[ "$HAS_CXXSHARED" == "0" ]]; then
  CXX_SRC="$WORK/starry_dec/lib/arm64-v8a/libc++_shared.so"
  [[ -f "$CXX_SRC" ]] || { echo "ERROR: gradle APK 内缺 libc++_shared.so（jniLibs 未打包？）"; exit 1; }
  cp "$CXX_SRC" "$WORK/lib/arm64-v8a/"
  echo "  + extras: libc++_shared.so $(stat -f%z "$CXX_SRC") B"
fi
# 官方若已自带 libc++_shared.so → 不覆盖
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/6] binary AndroidManifest（ContentProvider 注入；不替换 Application）"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
import sys
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
assert 'com.netease.nis.wrapper.MyApplication' in t, "官方 Application 不是易盾 MyApplication，需人工确认"
assert 'com.starry.magic.StarryInitProvider' not in t
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append(f'    <uses-permission android:name="android.permission.{perm}"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
prov = f'''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="{pkg}.starryinit"
            android:exported="false" />
'''
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
print(f"  text Manifest patched (StarryInitProvider + MenuService, perms={len(perms)})")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$INJECT_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, inject = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
dex = (work / inject).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r"); zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == inject:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
info = zipfile.ZipInfo(inject, (2026, 9, 30, 0, 0, 0)); info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, dex)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name, (2026, 9, 30, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest(), "arsc 被改"
assert inject in zout2.namelist() and "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
inj = set(extras) | {inject, "AndroidManifest.xml"}
n_same = n_checked = 0
for item in zin2.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj:
        continue
    n_checked += 1
    if hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest():
        n_same += 1
print(f"  merged ok; arsc/官方条目透传 {n_same}/{n_checked}; inject={sorted(inj)}")
if n_same != n_checked:
    raise SystemExit("ERROR: 官方条目 hash 不一致（外科透传破坏）")
zin2.close(); zout2.close()
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi
if [[ "$DIAG_MODE" == "1" ]]; then
  echo "  [diag] 诊断包不覆盖 alias（$ALIAS_APK 保持正式包）"
else
  cp -f "$SIGNED" "$ALIAS_APK"
fi
ls -lh "$SIGNED"

unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
[[ "$(md5 -q "$WORK/final.so")" == "$(md5 -q "$SO_BUILT")" ]] \
  || { echo "ERROR: 成品包内 so 与本次构建产物 md5 不一致"; exit 1; }
echo "  final apk so md5 == 构建产物 ok"
shasum -a 256 "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/yimo_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      if [[ "${STARRY_KEEP_DATA:-0}" == "1" ]]; then
        # -k = 保留 data/cache：实测保住 /sdcard/Android/data/<pkg>（含 GB 级热更数据），
        # 避免重装后整包重下（伊莫第一轮不带 -k 丢了 12.98 GB）
        echo "[install] signature mismatch → uninstall -k（保留数据/热更）+ reinstall"
        adb shell "su -c 'pm uninstall -k --user 0 $PKG'" 2>/dev/null || true
      else
        echo "[install] signature mismatch → uninstall + reinstall（数据/热更会丢；要保住设 STARRY_KEEP_DATA=1）"
        adb shell "su -c 'pm uninstall $PKG'" 2>/dev/null || true
      fi
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "日志（★ logcat -s 是【前缀匹配】：'[Yimo]' 这类带括号的 TAG 用 -s 永远匹配不到，必须 grep）："
  echo '  adb logcat -d | grep -E "\[Yimo\]|Mod_Menu|MenuService|StarryTapLogin"'
  if [[ "$DIAG_MODE" == "1" ]]; then
    echo '  adb logcat -d | grep "StarryDiag"                       # 诊断包（DLOG）'
    echo '  adb shell cat /sdcard/.starry_magic/moddiag.log          # 诊断包（Lua 侧）'
  fi
fi
echo "[done] $SIGNED"
