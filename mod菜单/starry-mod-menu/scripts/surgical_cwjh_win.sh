#!/usr/bin/env bash
# 超维进化 2.2（com.grinning.beta.yszl, vc12）—— 官方底包 → 星光外科包（Windows / Git Bash 版）
#
# 蓝本：scripts/surgical_cwjh_2.2.sh（mac 版）。本文件只做平台适配，
# 原脚本的断言、门禁（so 目标校验 / [Cwjh] 串校验 / 逐条目 zip 审计）一条不减少；
# 平台差异统一用 "WIN:" 注释标出。
#
# 前置：
#   1) 私有 mk app/src/main/jni/Android_cwjh.mk（不动公共 Android.mk）
#   2) libstarry_magic.so 已编出：app/src/libs_cwjh/arm64-v8a/（见 [1/6] 注释里的 ndk-build.cmd 命令）
#   3) 框架包已编出：app/build/outputs/apk/release/app-release.apk（gradle assembleRelease）
#   4) build-tools/android-15 需 junction 成 35.0.0，否则 AGP 报 SDK 缺组件
#
# 产物：08_output/超维进化/超维进化_2.2_surgical-releaseSigned.apk
# 不装机：装机是独立步骤（本脚本不处理 STARRY_INSTALL）
#
# 用法：bash scripts/surgical_cwjh_win.sh
#
#!/usr/bin/env bash
# 超维进化 2.2 (com.grinning.beta.yszl, vc12) — 外科 zip 组装（官方原包无星光注入）
# 基于 surgical_cwjh_2.1.sh（20260907 终版链），2.2 预检（20260909）：
#   - libcocos2djs.so kill 原语偏移 0x16ebdf8 前导字节一致（2a1f03e0 d10203ff）
#   - se::evalString / JNI evalString 符号均在
#   - APK 内置 game.js 六锚点全部唯一命中（仍是 2.1 首发签名系，无漂移）
#   - TapSDK classes（ClientLoginUtils/GatekeeperRepository）、AppActivity/GZQYActivity、
#     SilverdewApplication 位置不变
# 变更 vs 2.1：MenuService stopWithTask="true"（对齐 20260908 僵尸看门狗框架）
#
# 产物：08_output/超维进化/超维进化_2.2_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_cwjh_2.2.sh
#   STARRY_INSTALL=1 bash scripts/surgical_cwjh_2.2.sh
#   STARRY_ALLOW_UNINSTALL=1（签名不一致需卸载重装时必须显式给）
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
# WIN: 底包改指本仓实际落盘的原版（原 mac 版指向已清理的 _2.2_base.apk）
ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/超维进化/超维进化_taptap_原版_20261127_vc12.apk}"
OUT="$NIXIANG/08_output/超维进化"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
# WIN: NDK 走仓内 01_tools（mac 版走 ~/Library/Android/sdk）
NDK="${ANDROID_NDK_HOME:-$NIXIANG/01_tools/android-sdk/ndk/android-ndk-r27c}"
# WIN: prebuilt 目录名 darwin-x86_64 → windows-x86_64
NDK_CXX="$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.grinning.beta.yszl"
SIGNED="$OUT/超维进化_2.2_surgical-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/超维进化_2.2_starry.apk"
MF_CACHE="$NIXIANG/07_patched/超维进化/surgical_2.2/AndroidManifest.xml"

# WIN: 无 /usr/libexec/java_home，回落本机 JDK 17
if [[ -z "${JAVA_HOME:-}" ]]; then export JAVA_HOME="C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot"; fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

# WIN: build-tools 在仓内，且目录名为 android-15（AGP 认 35.0.0，需 junction 成 35.0.0）
BT="$NIXIANG/01_tools/android-sdk/build-tools/android-15"
[[ -f "$BT/zipalign.exe" ]] || { echo "ERROR: missing $BT/zipalign.exe"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
# WIN: 改读私有 mk，公共 Android.mk 不动（同仓库 100+ 游戏的既有姿势）
MK="$STARRY_SRC/app/src/main/jni/Android_cwjh.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_CWJH' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_CWJH"; exit 1; }
echo "[check] Android.mk target=cwjh"

LOCK_DIR="/tmp/cwjh_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/cwjh_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] reuse prebuilt cwjh so -> jniLibs"
# WIN: Windows 只有 ndk-build.cmd（无 bash 版 ndk-build），且本仓惯用私有 obj/libs 目录；
#      这里默认复用已编好的 so，未编时按下面命令编（各游戏独立 obj，不污染 app/obj）：
#      cd app/src/main/jni && "$NDK/ndk-build.cmd" NDK_PROJECT_PATH=../.. \
#        APP_BUILD_SCRIPT=./Android_cwjh.mk NDK_APPLICATION_MK=./Application.mk \
#        APP_ABI=arm64-v8a NDK_OUT=../../obj_cwjh NDK_LIBS_OUT=../../libs_cwjh -j4
SO_BUILT="$STARRY_SRC/app/src/libs_cwjh/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Cwjh\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 CWJH（可能仍是上一款游戏）。strings 检查失败。"
  exit 1
fi
if grep -a -q '\[Zxr\]' "$SO_BUILT" || grep -a -q '\[Qlfh\]' "$SO_BUILT" || grep -a -q 'ZxrMod' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签"
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
echo "  so -> jniLibs ok (cwjh verified)"

echo "[1b/6] reuse prebuilt framework APK (WIN: 本机 SDK 无 platforms，gradle 步跳过)"
STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK (先 gradle assembleRelease)"; exit 1; }
echo "  framework APK size=$(stat -c%s "$STARRY_APK") B"

echo "[2/6] apktool d starry → smali + so + certs"
java -jar "$NIXIANG/01_tools/apktool/apktool.jar" d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes4.dex (StarryApp → SilverdewApplication)"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT/com"  # WIN: 目标父目录缺失时 cp/rsync 会静默不落地
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  cp -r "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
python - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/grinning/game/SilverdewApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/grinning/game/SilverdewApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/grinning/game/SilverdewApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/grinning/game/SilverdewApplication;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if "Lcom/grinning/game/SilverdewApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing SilverdewApplication")
if ".super Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still .super Landroid/app/Application;")
if "Landroid/app/Application;-><init>()V" in t:
    raise SystemExit("ERROR: StarryApp.smali still ctor Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp → SilverdewApplication ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
ls -la "$WORK/classes4.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/6] patch classes3.dex: AppActivity get_ret/set_ret + MenuService 生命周期"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python - "$WORK/c3smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

act = root / "org/cocos2dx/javascript/AppActivity.smali"
if not act.is_file():
    raise SystemExit(f"missing {act}")
s = act.read_text(encoding="utf-8")

if "MenuService;->notifyHostResumed" not in s:
    s2, n = re.subn(
        r"(\.method protected onResume\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onResume patch failed")
    s = s2
    print("  onResume → notifyHostResumed")
else:
    print("  onResume already patched")

if "MenuService;->notifyHostLeaving" not in s:
    s2, n = re.subn(
        r"(\.method protected onPause\(\)V[\s\S]*?)(    return-void\n\.end method)",
        r"\1    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V\n\n\2",
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("AppActivity.onPause patch failed")
    s = s2
    print("  onPause → notifyHostLeaving")
else:
    print("  onPause already patched")

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Lcom/cocos/game/GZQYActivity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("  +onUserLeaveHint")

if "CwjhHooks;->getRet" not in s:
    s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/CwjhHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/CwjhHooks;->setRet(IZ)V

    return-void
.end method
"""
    print("  +get_ret/set_ret → CwjhHooks")
else:
    print("  get_ret already present")

act.write_text(s, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

echo "[3c/6] patch classes.dex: TapSDK 强制网页登录 + gatekeeper"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes1_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes1_orig.dex"
python - "$WORK/c1smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])

clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not clu.is_file():
    raise SystemExit(f"missing {clu}")
s = clu.read_text(encoding="utf-8")
if "starry_force_web_login" not in s:
    import re
    m = re.search(r"(\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?)(    return[^\n]*\n\.end method)", s)
    if not m:
        raise SystemExit("isClientLogin method body not matched")
    body = (
        ".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z\n"
        "    .locals 1\n\n"
        "    # starry_force_web_login: 恒 false → LoginTypes.WEB → WebLoginFragment\n"
        "    const/4 v0, 0x0\n\n"
        "    return v0\n"
        ".end method\n"
    )
    s = s[:m.start()] + body + s[m.end():]
    print("  isClientLogin → false (force web)")
else:
    print("  isClientLogin already patched")
clu.write_text(s, encoding="utf-8")

gk = root / "com/taptap/sdk/initializer/repository/GatekeeperRepository$initialize$1.smali"
if not gk.is_file():
    raise SystemExit(f"missing {gk}")
g = gk.read_text(encoding="utf-8")
if "starry_gatekeeper_pass" not in g:
    blk = (
        "    const/4 v1, 0x0\n\n"
        "    invoke-static {v0, v1}, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;->access$setLoadGatekeeperSuccess$p(Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;Z)V\n"
    )
    if blk not in g:
        raise SystemExit("gatekeeper setLoadGatekeeperSuccess(false) block not matched")
    # v1=1 → loadGatekeeperSuccess=true（跳过 showGatekeeperError 的唯一闸门）
    g = g.replace(blk, blk.replace("const/4 v1, 0x0", "# starry_gatekeeper_pass: keep success=true\n    const/4 v1, 0x1"), 1)
    import re
    pat = re.compile(
        r"(        )?\.line \d+\n"
        r"    iget-object v0, p0, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository\$initialize\$1;->this\$0:Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;\n"
        r"\n"
        r"    # invokes: Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;->showErrorToast\(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;\)V\n"
        r"    invoke-static \{v0, p1\}, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;->access\$showErrorToast\(Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;Lcom/taptap/sdk/common/network/throwable/TapNetworkException;\)V\n"
    )
    # 先删 INVALID_CLIENT 分支内的 setGatekeeperError（p1 版本；v2 是成功路径，保留）
    pat2 = re.compile(
        r"    iget-object v0, p0, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository\$initialize\$1;->this\$0:Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;\n"
        r"\n"
        r"    invoke-static \{v0, p1\}, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;->access\$setGatekeeperError\$p\(Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;Lcom/taptap/sdk/common/network/throwable/TapNetworkException;\)V\n"
    )
    g2, n2 = pat2.subn("", g, count=1)
    if n2 != 1:
        raise SystemExit(f"setGatekeeperError(p1) sequence matched {n2} times")
    g3, n3 = pat.subn("    # starry_gatekeeper_pass: no toast\n", g2, count=1)
    if n3 != 1:
        raise SystemExit(f"showErrorToast sequence matched {n3} times")
    g = g3
    if g.count("access$setGatekeeperError$p") != 1:
        raise SystemExit("setGatekeeperError(v2 success path) unexpectedly altered")
    print("  gatekeeper: success stays true, error/toast skipped")
else:
    print("  gatekeeper already patched")
gk.write_text(g, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/classes1.dex" "$WORK/c1smali"
ls -la "$WORK/classes1.dex"

echo "[4/6] binary AndroidManifest (SilverdewApplication → StarryApp + MenuService, stopWithTask=true)"
if [[ -n "${CWJH_BINARY_MANIFEST:-}" && -f "$CWJH_BINARY_MANIFEST" ]]; then
  cp "$CWJH_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s + b extract)"
  java -jar "$NIXIANG/01_tools/apktool/apktool.jar" d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
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
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
t = t.replace(
    'android:name="com.grinning.game.SilverdewApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if 'android:name="com.starry.magic.StarryApp"' not in t:
    raise SystemExit("ERROR: failed to set StarryApp as application")
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="true">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  java -jar "$NIXIANG/01_tools/apktool/apktool.jar" b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge"
python - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c1 = (work / "classes1.dex").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

replace = {
    "AndroidManifest.xml": mf,
    "classes3.dex": c3,
    "classes.dex": c1,
}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == "classes4.dex":
        continue
    if name in replace:
        data = replace[name]
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
    n += 1

info = zipfile.ZipInfo("classes4.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    # .so 必须 STORED（原包 extractNativeLibs=false，压缩 so 会导致 INSTALL_FAILED_INVALID_APK）
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("resources.arsc", "classes2.dex"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert hashlib.sha256(zout2.read("classes3.dex")).hexdigest() != hashlib.sha256(zin2.read("classes3.dex")).hexdigest()
assert hashlib.sha256(zout2.read("classes.dex")).hexdigest() != hashlib.sha256(zin2.read("classes.dex")).hexdigest()
assert "classes4.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes3={len(c3)} classes4={len(c4)}")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign.exe" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -f "$BT/apksigner.bat" ]]; then
  "$BT/apksigner.bat" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner.bat" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED" "$ALIAS_APK"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/cwjh_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      echo "[install] signature mismatch → uninstall + reinstall (ALLOW_UNINSTALL=1)"
      adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。"
      echo "  需卸载原版才能装星光包。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Cwjh:V MenuService:V"
fi
echo "[done] $SIGNED"
