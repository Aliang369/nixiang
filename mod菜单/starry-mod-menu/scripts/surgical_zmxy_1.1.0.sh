#!/usr/bin/env bash
# ================================================================
#  造梦西游·黎尤浩劫篇 1.1.0 (com.xm4399.zmxy6hj / vc3 / 4399 渠道) — 外科 zip 组装
#
#  底包：官方原版（真机 /data/app 拉取，留档 09_test/造梦西游/base_official_1.1.0_20260929/）
#  引擎：Cocos Creator JS + 百度保护 / 4399 FN SDK（保留 FNApplication 不动）
#
#  改动（外科，禁止对游戏整包 apktool b）：
#    - classes4.dex  patch：FNApplication.attachBaseContext → SignSpoof + loadLibrary
#    - classes5.dex  patch：AppActivity onCreate/onResume/onPause/onUserLeaveHint
#                           + get_ret/set_ret/get_val/set_val 静态转发
#    - classes7.dex  新增：com/starry/**（菜单 + ZmxyHooks）
#    - lib/arm64-v8a 新增：libstarry_magic.so + libc++_shared.so
#    - assets/certs  新增：starlight.crt（TLS pin，so 内已烘焙同名公钥）
#    - AndroidManifest：加 SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE(_SPECIAL_USE)
#                       + MenuService；**Application 保持 FNApplication**
#    - resources.arsc / classes.dex,2,3,6 / 其余资产：字节不动（脚本内断言）
#
#  玩法：ZmxyMod se::ScriptEngine::evalString 运行时拼接
#        1 Slider_伤害倍率:1:100:1（get_val）2 无敌（get_ret）3 无限技能（get_ret）
#
#  产物：08_output/造梦西游/造梦西游_1.1.0_surgical-releaseSigned.apk
#
#  用法:
#    bash scripts/surgical_zmxy_1.1.0.sh                  # 只出包，不装机
#    STARRY_INSTALL=1 bash scripts/surgical_zmxy_1.1.0.sh # 覆盖装机（异签名会停下等授权）
#    STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash …     # 已获授权时：卸载官方包（清数据）→ 全新安装
#    ZMXY_BASE_APK=/path/to/official.apk 覆盖底包路径
# ================================================================
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
ORIG_APK="${ZMXY_BASE_APK:-$NIXIANG/09_test/造梦西游/base_official_1.1.0_20260929/base.apk}"
ORIG_MD5="d5ca19093f94b47b2cced7054d34177d"
OUT="$NIXIANG/08_output/造梦西游"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
KS="$NIXIANG/01_tools/starry_release.jks"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.xm4399.zmxy6hj"
SERIAL="${SERIAL:-f9a116a2}"
OBJ_DIR="$STARRY_SRC/obj_zmxy"
LIBS_DIR="$STARRY_SRC/libs_zmxy"
MENU_JSON="$NIXIANG/10_docs/造梦西游/zmxy_menu_template.json"
MF_CACHE="$NIXIANG/07_patched/造梦西游/surgical_1.1.0/AndroidManifest.xml"
# dex 布局（官方 1.1.0：classes.dex..classes6.dex；下一空位 = classes7）
PATCH_DEX_APP="classes4.dex"   # FNApplication
PATCH_DEX_ACT="classes5.dex"   # AppActivity
NEW_DEX="classes7.dex"
SIGNED="$OUT/造梦西游_1.1.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/造梦西游_1.1.0_surgical-debugSigned.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing 官方底包 $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: missing ndk-build at $NDK"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android_zmxy.mk"
[[ -f "$MK" ]] || { echo "ERROR: missing $MK"; exit 1; }
grep -q 'Games/ZmxyMod.cpp' "$MK" || { echo "ERROR: $MK 未指向 ZmxyMod"; exit 1; }

LOCK_DIR="/tmp/zmxy_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zmxy_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[0/8] 底包指纹 + 门控读取方式自检"
ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || {
  echo "ERROR: 底包指纹不符（got $ACTUAL_MD5, want ${ORIG_MD5}）"; exit 1; }
echo "  官方底包 md5=$ACTUAL_MD5 ✓"
JS_HDR="$STARRY_SRC/app/src/main/jni/Games/ZmxyPatchJs.h"
grep -q '"get_val","(I)I",1' "$JS_HDR" \
  || { echo "ERROR: 伤害倍率未用 get_val(I)I 读（滑块禁止走 getRet 布尔）"; exit 1; }
grep -q '"get_ret","(I)Z",2' "$JS_HDR" || { echo "ERROR: 无敌未走 get_ret(2)"; exit 1; }
grep -q '"get_ret","(I)Z",3' "$JS_HDR" || { echo "ERROR: 无限技能未走 get_ret(3)"; exit 1; }
if grep -q '"get_val","(I)I",2' "$JS_HDR" || grep -q '"get_val","(I)I",3' "$JS_HDR"; then
  echo "ERROR: toggle 走了 get_val 数值读"; exit 1
fi
echo "  控件读取方式 ✓（滑块=get_val / 无敌+无限技能=get_ret）"

python3.11 - "$JS_HDR" "$WORK/zmxy_js_spliced.js" <<'PY'
# 拼接体语法关：把 ZmxyPatchJs.h 的两段 JS 按 C++ 侧同样的锚点规则
# 插进一份「合成战斗脚本」，再交 node --check。锚点命中/插入位置/语法一次验完。
import re, sys
from pathlib import Path
src = Path(sys.argv[1]).read_text(encoding="utf-8")
dst = Path(sys.argv[2])
ACT = "com.xm4399.zm6.AppActivity"

def grab(name):
    m = re.search(r'const char\*\s+' + name + r'\s*=\s*R"ZMXYJS\((.*?)\)ZMXYJS";', src, re.S)
    if not m:
        raise SystemExit("ERROR: 未找到拼接体 " + name)
    body = m.group(1)
    if body.count("%s") != (2 if name == "kZmxyDmgHookFmt" else 1):
        raise SystemExit("ERROR: %s 的 %%s 占位数量异常" % name)
    # %s 在 JS 里已在引号内（callStaticMethod("%s",…)）→ 只替换裸类名
    return body.replace("%s", ACT)

dmg = grab("kZmxyDmgHookFmt")
uls = grab("kZmxySkillHookFmt")

DMG_ANCHORS = ["if (0 == e || this.isDead()) return 0;", "if(0==e||this.isDead())return 0;"]
SKILL_ANCHORS = [
    'if (this.mPlayer.isDead() || this.mPlayer.mBuffMgr && !this.mPlayer.mBuffMgr.isCanReleaseSkill()) return "1";',
    'if(this.mPlayer.isDead()||this.mPlayer.mBuffMgr&&!this.mPlayer.mBuffMgr.isCanReleaseSkill())return"1";',
]
# 合成脚本：把两个锚点放进真实函数体，模拟游戏战斗脚本形态
s = (
    "function _takeDamage(e) {\n"
    "  if (0 == e || this.isDead()) return 0;\n"
    "  return e;\n"
    "}\n"
    "function _trySkill() {\n"
    '  if (this.mPlayer.isDead() || this.mPlayer.mBuffMgr && !this.mPlayer.mBuffMgr.isCanReleaseSkill()) return "1";\n'
    "  return null;\n"
    "}\n"
)
hit = 0
for a in DMG_ANCHORS:
    p = s.find(a)
    if p >= 0:
        s = s[: p + len(a)] + dmg + s[p + len(a):]
        hit |= 1
        break
for a in SKILL_ANCHORS:
    p = s.find(a)
    if p >= 0:
        s = s[:p] + uls + s[p:]
        hit |= 2
        break
if hit != 3:
    raise SystemExit("ERROR: 合成脚本锚点命中失败 dmg=%d uls=%d" % (hit & 1, (hit >> 1) & 1))
dst.write_text(s, encoding="utf-8")
print("  拼接体语法样本 → %s (dmg+uls 锚点均命中)" % dst)
PY
if command -v node >/dev/null 2>&1; then
  node --check "$WORK/zmxy_js_spliced.js" \
    || { echo "ERROR: JS 拼接体语法关失败"; exit 1; }
  echo "  node --check ✓"
else
  echo "  WARN: 本机无 node，跳过 JS 语法关"
fi

echo "[1/8] ndk-build（Android_zmxy.mk，独立 obj/libs）"
(cd "$STARRY_SRC/app/src/main/jni" && "$NDK/ndk-build" \
  NDK_PROJECT_PATH=../.. \
  APP_BUILD_SCRIPT=./Android_zmxy.mk \
  NDK_APPLICATION_MK=./Application.mk \
  APP_ABI=arm64-v8a \
  NDK_OUT="$OBJ_DIR" \
  NDK_LIBS_OUT="$LIBS_DIR" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$LIBS_DIR/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q '\[Zmxy\]' "$SO_BUILT" || { echo "ERROR: so 缺 [Zmxy] 标签"; exit 1; }
grep -a -q 'starry_zmxy_dmg' "$SO_BUILT" || { echo "ERROR: so 缺伤害拼接体"; exit 1; }
grep -a -q 'starry_zmxy_uls' "$SO_BUILT" || { echo "ERROR: so 缺技能拼接体"; exit 1; }
grep -a -q '_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_' "$SO_BUILT" \
  || { echo "ERROR: so 缺 se::evalString 符号串"; exit 1; }
if grep -a -q '\[Zmws\]' "$SO_BUILT" || grep -a -q 'starry_zmws_' "$SO_BUILT"; then
  echo "ERROR: so 残留上一款（造梦无双）标签，禁止装机"; exit 1
fi
echo "  so ok ($(stat -f%z "$SO_BUILT") B) $(md5 -q "$SO_BUILT")"
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done

echo "[1b/8] gradle ${GRADLE_TASK}（取 com/starry 菜单 dex）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/8] apktool d starry 模板（小程序包，非游戏包）"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/8] $NEW_DEX = com/starry（菜单 + ZmxyHooks）"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/ZmxyHooks.smali" ]] \
  || { echo "ERROR: ZmxyHooks.smali missing（Java 侧未编入？）"; exit 1; }
grep -q 'getVal\|setVal' "$SMALI_ROOT/com/starry/magic/cocos/ZmxyHooks.smali" \
  || { echo "ERROR: ZmxyHooks 缺 getVal/setVal"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
echo "  $NEW_DEX $(stat -f%z "$WORK/$NEW_DEX") B"

echo "[3b/8] patch ${PATCH_DEX_APP}（FNApplication：SignSpoof + loadLibrary）"
unzip -p "$ORIG_APK" "$PATCH_DEX_APP" > "$WORK/${PATCH_DEX_APP%.dex}_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c4smali" "$WORK/${PATCH_DEX_APP%.dex}_orig.dex"
python3.11 - "$WORK/c4smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])
app = root / "com/ssjj/fnsdk/tool/privacy_policy/FNApplication.smali"
if not app.is_file():
    raise SystemExit("FNApplication.smali missing")
t = app.read_text(encoding="utf-8")
if "SignSpoof;->install" in t and "starry_magic" in t:
    print("  FNApplication already patched"); raise SystemExit(0)
needle = ("    invoke-super {p0, p1}, Lcom/ssjj/fnsdk/platform/FN4399HzApplication;"
          "->attachBaseContext(Landroid/content/Context;)V\n")
if t.count(needle) != 1:
    raise SystemExit("FNApplication.attachBaseContext super 不唯一")
inject = needle + """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
app.write_text(t.replace(needle, inject, 1), encoding="utf-8")
print("  FNApplication ← SignSpoof.install + loadLibrary(starry_magic)")
PY
"$SMALI_BIN" assemble -o "$WORK/$PATCH_DEX_APP" "$WORK/c4smali"
echo "  $PATCH_DEX_APP $(stat -f%z "$WORK/$PATCH_DEX_APP") B"

echo "[3c/8] patch ${PATCH_DEX_ACT}（AppActivity：菜单生命周期 + get_ret/get_val）"
unzip -p "$ORIG_APK" "$PATCH_DEX_ACT" > "$WORK/${PATCH_DEX_ACT%.dex}_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c5smali" "$WORK/${PATCH_DEX_ACT%.dex}_orig.dex"
python3.11 - "$WORK/c5smali" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])
act = root / "com/xm4399/zm6/AppActivity.smali"
if not act.is_file():
    raise SystemExit("AppActivity.smali missing")
s = act.read_text(encoding="utf-8")
if "ZmxyHooks;->getRet" in s:
    print("  AppActivity already patched"); raise SystemExit(0)

# ---- onCreate：super 前早载 so；尾部起 MenuService
super_on = ("    invoke-super {p0, p1}, Lorg/cocos2dx/lib/Cocos2dxActivity;"
            "->onCreate(Landroid/os/Bundle;)V\n")
if s.count(super_on) != 1:
    raise SystemExit("AppActivity.onCreate super 不唯一")
early = ('    const-string v0, "starry_magic"\n\n'
         '    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n'
         + super_on)
s = s.replace(super_on, early, 1)
tail = ("    invoke-virtual {p1, v0}, Landroid/view/Window;->addFlags(I)V\n\n"
        "    return-void\n.end method")
if s.count(tail) != 1:
    raise SystemExit("AppActivity.onCreate 尾部标记不唯一")
s = s.replace(tail, tail.replace(
    "    return-void\n.end method",
    "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;"
    "->start(Landroid/content/Context;)V\n\n    return-void\n.end method"), 1)
print("  onCreate → loadLibrary + MenuService.start")

def inject_before_final_return(text, method_sig, inject, tag):
    start = text.find(method_sig)
    if start < 0:
        raise SystemExit("%s: method not found" % tag)
    end = text.find(".end method", start)
    block = text[start:end]
    pos = block.rfind("    return-void")
    if pos < 0:
        raise SystemExit("%s: no return-void" % tag)
    return text[:start] + block[:pos] + inject + block[pos:] + text[end:]

s = inject_before_final_return(
    s, ".method protected onResume()V",
    "    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;"
    "->notifyHostResumed(Landroid/app/Activity;)V\n\n",
    "onResume")
s = inject_before_final_return(
    s, ".method protected onPause()V",
    "    invoke-static {}, Lcom/starry/magic/menu/MenuService;"
    "->notifyHostLeaving()V\n\n",
    "onPause")
print("  onResume/onPause → MenuService 生命周期")

if "onUserLeaveHint" not in s:
    leave = """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method

"""
    marker = ".method protected onPause()V"
    if marker not in s:
        raise SystemExit("onPause marker missing")
    s = s.replace(marker, leave + marker, 1)
    print("  +onUserLeaveHint")

# ---- JS 门控转发（斌哥同构：get_ret(I)Z；本作 slider 另加 get_val(I)I）
s += """
.method public static get_ret(I)Z
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZmxyHooks;->getRet(I)Z

    move-result v0

    return v0
.end method

.method public static set_ret(IZ)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "on"    # Z

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZmxyHooks;->setRet(IZ)V

    return-void
.end method

.method public static get_val(I)I
    .locals 1
    .param p0, "index"    # I

    invoke-static {p0}, Lcom/starry/magic/cocos/ZmxyHooks;->getVal(I)I

    move-result v0

    return v0
.end method

.method public static set_val(II)V
    .locals 0
    .param p0, "index"    # I
    .param p1, "value"    # I

    invoke-static {p0, p1}, Lcom/starry/magic/cocos/ZmxyHooks;->setVal(II)V

    return-void
.end method
"""
print("  +get_ret/set_ret/get_val/set_val → ZmxyHooks")
act.write_text(s, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/$PATCH_DEX_ACT" "$WORK/c5smali"
echo "  $PATCH_DEX_ACT $(stat -f%z "$WORK/$PATCH_DEX_ACT") B"

echo "[3d/8] 备齐 lib/ 与 assets/certs"
mkdir -p "$WORK/lib/arm64-v8a" "$WORK/assets/certs"
cp -f "$SO_BUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -f "$WORK/starry_dec/assets/certs/starlight.crt" ]]; then
  cp -f "$WORK/starry_dec/assets/certs/starlight.crt" "$WORK/assets/certs/starlight.crt"
fi
ls -la "$WORK/lib/arm64-v8a" "$WORK/assets/certs"

echo "[4/8] 二进制 AndroidManifest（保留 FNApplication + 加 MenuService/权限）"
if [[ -f "$MF_CACHE" ]]; then
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  用缓存 $MF_CACHE"
else
  # stub-apktool：只解 manifest+arsc+res（游戏包本体 670MB，不整包 decode）
  rm -rf "$WORK/stub_src" "$WORK/stub_dec"
  mkdir -p "$WORK/stub_src"
  (cd "$WORK/stub_src" && unzip -o -q "$ORIG_APK" 'AndroidManifest.xml' 'resources.arsc' 'res/*')
  (cd "$WORK/stub_src" && zip -q -r -X "$WORK/stub.apk" AndroidManifest.xml resources.arsc res)
  apktool d -f -s -o "$WORK/stub_dec" "$WORK/stub.apk" >/dev/null
  python3.11 - "$WORK/stub_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
if "com.ssjj.fnsdk.tool.privacy_policy.FNApplication" not in t:
    raise SystemExit("ERROR: 官方 Application（FNApplication）不在 Manifest")
if "com.starry.magic.StarryApp" in t:
    raise SystemExit("ERROR: 不得替换 Application 为 StarryApp")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t \
        and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
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
print("  Manifest: 权限 + MenuService（Application 保持 FNApplication）")
PY
  apktool b -f "$WORK/stub_dec" -o "$WORK/stub_patched.apk" >/dev/null
  unzip -p "$WORK/stub_patched.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  if [[ -n "$BT" && -x "$BT/aapt2" ]]; then
    "$BT/aapt2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" 2>/dev/null \
      | grep -oE "@0x[0-9a-f]+" | sort -u > "$WORK/ref_orig.txt" || true
    "$BT/aapt2" dump xmltree --file AndroidManifest.xml "$WORK/stub_patched.apk" 2>/dev/null \
      | grep -oE "@0x[0-9a-f]+" | sort -u > "$WORK/ref_new.txt" || true
    if [[ -s "$WORK/ref_orig.txt" && -s "$WORK/ref_new.txt" ]]; then
      diff -q "$WORK/ref_orig.txt" "$WORK/ref_new.txt" >/dev/null \
        || { echo "ERROR: 重建 manifest 资源引用漂移"; diff "$WORK/ref_orig.txt" "$WORK/ref_new.txt"; exit 1; }
      echo "  资源引用自检 ✓（$(wc -l < "$WORK/ref_orig.txt" | tr -d ' ') refs 一致）"
    fi
  fi
  rm -rf "$WORK/stub_src" "$WORK/stub.apk" "$WORK/stub_dec" "$WORK/stub_patched.apk"
  cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  缓存 → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/8] zip 外科合并（+ 零漂移断言）"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$PATCH_DEX_APP" "$PATCH_DEX_ACT" "$NEW_DEX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
patch_app, patch_act, new_dex = sys.argv[4], sys.argv[5], sys.argv[6]

mf = (work / "AndroidManifest.xml").read_bytes()
c_app = (work / patch_app).read_bytes()
c_act = (work / patch_act).read_bytes()
c_new = (work / new_dex).read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

replace = {"AndroidManifest.xml": mf, patch_app: c_app, patch_act: c_act}
skip = set(extras) | {new_dex}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
orig = {}
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    orig[name] = hashlib.sha256(zin.read(name)).hexdigest()
    if name in skip:
        continue
    data = replace[name] if name in replace else zin.read(name)
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

info = zipfile.ZipInfo(new_dex)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c_new)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED if not name.endswith(".so") else zipfile.ZIP_STORED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
changed = 0
for name, h in orig.items():
    got = hashlib.sha256(zout2.read(name)).hexdigest()
    if name in replace:
        assert got != h, "%s 应被改写但没有变化" % name
        changed += 1
        continue
    assert got == h, "字节漂移: %s" % name
assert hashlib.sha256(zout2.read("resources.arsc")).hexdigest() == orig["resources.arsc"]
assert new_dex in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert "lib/arm64-v8a/libc++_shared.so" in zout2.namelist()
assert "assets/certs/starlight.crt" in zout2.namelist()
print("  合并 ok：改写 %d 项，新增 %s + %d 个附加文件；其余 %d 项逐字节一致"
      % (changed, new_dex, len(extras), len(orig) - changed))
zin2.close(); zout2.close()
PY

echo "[6/8] zipalign + 签名 → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp -f "$WORK/unsigned.apk" "$ALIGNED"
fi
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --v4-signing-enabled false \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8 || true
else
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi
rm -f "$ALIGNED"; rm -f "$OUT/造梦西游_1.1.0_surgical-"*.idsig 2>/dev/null || true
ls -lh "$SIGNED"

echo "[7/8] 产物闭环校验"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_BUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] || {
  echo "ERROR: 包内 so($IN_SO_MD5) != 构建产物($SRC_SO_MD5)——旧 so 混包"; exit 1; }
unzip -p "$SIGNED" "$NEW_DEX" > "$WORK/check7.dex"
unzip -p "$SIGNED" "$PATCH_DEX_ACT" > "$WORK/check5.dex"
cmp -s "$WORK/check7.dex" "$WORK/$NEW_DEX" || { echo "ERROR: 包内 $NEW_DEX 与构建产物不一致"; exit 1; }
# dex 内名字是 Java 驼峰（ZmxyHooks.getVal）；JS 调的是 AppActivity 蛇形转发（get_val）
grep -aq 'getVal' "$WORK/check7.dex" || { echo "ERROR: classes7 ZmxyHooks 缺 getVal"; exit 1; }
grep -aq 'getRet' "$WORK/check7.dex" || { echo "ERROR: classes7 ZmxyHooks 缺 getRet"; exit 1; }
for sym in get_val set_val get_ret set_ret; do
  grep -aq "$sym" "$WORK/check5.dex" || { echo "ERROR: classes5 AppActivity 缺 $sym 转发"; exit 1; }
done
"$BT/aapt2" dump badging "$SIGNED" 2>/dev/null | head -3 || true
echo "  包内 so md5 == 构建产物（${IN_SO_MD5}）✓；${NEW_DEX} 字节一致 ✓"
echo "  产物 md5=$(md5 -q "$SIGNED")  size=$(stat -f%z "$SIGNED")"

echo "[8/8] 装机（STARRY_INSTALL=${STARRY_INSTALL:-0}）"
if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
    --js "$WORK/zmxy_js_spliced.js" \
    --cpp "$STARRY_SRC/app/src/main/jni/Games/ZmxyMod.cpp" \
    --cpp "$STARRY_SRC/app/src/main/jni/MainModern.cpp" \
    --dex "$WORK/$NEW_DEX" --dex "$WORK/${PATCH_DEX_ACT}" \
    --so "$SO_BUILT" \
    --menu-json "$MENU_JSON" \
    || { echo "ERROR: 装机前门禁未过，停止"; exit 1; }

  REMOTE="/data/local/tmp/zmxy_surgical.apk"
  adb -s "$SERIAL" push "$SIGNED" "$REMOTE"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  INSTALL_OUT="$(adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>&1 || true)"
  echo "$INSTALL_OUT" | tail -3
  if echo "$INSTALL_OUT" | grep -q 'Success'; then
    echo "[install] 覆盖安装 ok"
  elif echo "$INSTALL_OUT" | grep -qE 'INSTALL_FAILED_UPDATE_INCOMPATIBLE|INCONSISTENT_CERTIFICATES|signatures do not match'; then
    # 异签名：卸载会清游戏本地数据，属破坏性操作 —— 只在用户当轮明确授权时走这条路
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" != "1" ]]; then
      echo "ERROR: 覆盖安装失败=签名不一致（官方包已装）。按工作区规则不自动卸载/清数据——"
      echo "       取得用户对「卸载 ${PKG}（清游戏数据）并装星光包」的明确授权后，重跑："
      echo "       STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_zmxy_1.1.0.sh"
      exit 2
    fi
    echo "[install] 已获授权：卸载 ${PKG}（清游戏数据）→ 全新安装星光包"
    adb -s "$SERIAL" shell "su -c 'pm uninstall --user 0 \"$PKG\"'" \
      || { echo "ERROR: 卸载失败，停止；如需人工处理后再重跑"; exit 1; }
    adb -s "$SERIAL" shell "su -c 'pm install -d --user 0 \"$REMOTE\"'" \
      || { echo "ERROR: 卸载后全新安装仍失败，停止"; exit 1; }
    echo "[install] 卸载→全新安装 ok"
  else
    echo "ERROR: 覆盖安装失败，且非签名不一致（不卸载，避免误清数据）；原始输出见上"
    exit 1
  fi
  adb -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su
  # shellcheck source=/dev/null
  source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG"
  echo "log: adb -s $SERIAL logcat -s StarryMagic:V Zmxy:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
