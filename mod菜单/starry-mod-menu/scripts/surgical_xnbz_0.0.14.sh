#!/usr/bin/env bash
# 仙逆：本尊 0.0.14 TapTap 原版（com.qqsy.xnbz）→ 星空魔法外科组装
# 引擎：Cocos Creator 3.x / libcocos.so（JSB；无 IL2CPP、无加固壳）
#
# 产出：08_output/仙逆/仙逆_0.0.14_surgical-releaseSigned.apk
#
# 注入内容
#   classes11.dex                    星空菜单（com.starry.magic.**：StarryInitProvider / MenuService
#                                    / ModBridge / sign.SignSpoof / sign.TapLoginForceWeb）
#   lib/arm64-v8a/libstarry_magic.so   native（本阶段零 hook 环境版：注册 XnbzEntry 菜单 + 信号兜底）
#   lib/arm64-v8a/libc++_shared.so
#   assets/certs/starlight.crt
#   AndroidManifest.xml              分支 B（StarryInitProvider，**不替换官方 Application
#                                    com.cocos.game.GameApplication**）
#                                    + MenuService + SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE*
#   classes7.dex                    ★改写：TapSignKit.getSign 固定返回原厂证书 MD5（CN=qqian）
#                                    + ClientLoginUtils.isClientLogin 恒 false（强制 TapTap 网页登录）
#
# 字节不变（merge 阶段逐条 sha256 断言）：classes.dex / classes2..6.dex / classes8..10.dex /
#   resources.arsc / lib/arm64-v8a/libcocos.so
#
# 底包政策：真机只读拉取，不留本地（用完即删），只在 10_docs/_state/底包再获取台账.md 登记。
#   再获取：adb exec-out "su -c 'cat /data/app/<hash>/com.qqsy.xnbz-<hash>/base.apk'" > /tmp/xnbz/base.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# 本脚本放在 mod菜单/starry-mod-menu/scripts/ 下（与 surgical_gyzw_* 同处）
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"

GAME="仙逆"
VERSION="0.0.14"
PKG="com.qqsy.xnbz"
TARGET_DEX="classes7.dex"         # TapTap SDK 新版路径：TapSignKit + ClientLoginUtils 所在
NEW_DEX="classes11.dex"           # 官方已有 classes.dex .. classes10.dex

ORIG_SIGN_MD5="82c51c4ac68ac5d3197ef4c2b19e1ef6"   # 原厂证书 MD5（CN=qqian）

ORIG_APK="${ORIG_APK:-/tmp/xnbz/base.apk}"
ORIG_MD5="${ORIG_MD5:-756df4d27ded491e00a865687571fc53}"
ORIG_TARGET_DEX_MD5="${ORIG_TARGET_DEX_MD5:-72625d77928cf78d6abe51cee789c71d}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SO_PREBUILT="${XNBZ_SO:-$STARRY_SRC/libs_xnbz/arm64-v8a/libstarry_magic.so}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]]      || { echo "ERROR: missing ${ORIG_APK}（底包不留本地，需从设备只读拉取）" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]]     || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]]  || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]]   || { echo "ERROR: missing ${SO_PREBUILT}（先跑 04_scripts/仙逆/xnbz_build_so.sh）" >&2; exit 1; }

echo "[check] 底包 = $ORIG_APK"
GOT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 原包 md5=$GOT_MD5 != $ORIG_MD5" >&2; exit 1; }
GOT_TD="$(unzip -p "$ORIG_APK" "$TARGET_DEX" | md5 -q)"
[[ "$GOT_TD" == "$ORIG_TARGET_DEX_MD5" ]] || { echo "ERROR: $TARGET_DEX md5=$GOT_TD != $ORIG_TARGET_DEX_MD5" >&2; exit 1; }
echo "[check] 原包 md5 ok / $TARGET_DEX md5 ok"

# so 归属校验（防串包：上一款游戏的 so 顶包）
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
if ! LC_ALL=C grep -a -q '\[Xnbz\]' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 不是 Xnbz 版（缺 [Xnbz]）" >&2
  strings "$SO_PREBUILT" | grep -aE '^\[(Xnbz|Gyzw|Jfqs|Dwct|Ysxz)' | sort -u | head -20 || true
  exit 1
fi
for s in '\[Gyzw\]' '\[Jfqs\]' '\[Ysxz\]' '\[Dwct\]' '\[Tlgd\]'; do
  if LC_ALL=C grep -a -q -- "$s" "$SO_PREBUILT"; then
    echo "ERROR: $SO_PREBUILT 含别家标签 ${s}（libs_xnbz 被并行会话覆盖）" >&2; exit 1
  fi
done
SO_MD5="$(md5 -q "$SO_PREBUILT")"
echo "[check] so = $SO_PREBUILT md5=$SO_MD5"

LOCK_DIR="/tmp/xnbz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/xnbz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/6
echo "[1/6] gradle assembleRelease（星空 APK → smali）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }
# 星空 APK 内 so 必须是 Xnbz 版（gradle 打进 jniLibs 的那份）
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/starry_inner.so"
if ! LC_ALL=C grep -a -q '\[Xnbz\]' "$WORK/starry_inner.so"; then
  echo "ERROR: 星空 APK 内 so 不是 Xnbz 版（jniLibs 未刷新，先跑 xnbz_build_so.sh）" >&2
  strings "$WORK/starry_inner.so" | grep -aE '^\[(Xnbz|Gyzw|Jfqs|Dwct|Ysxz)' | sort -u | head || true
  exit 1
fi
echo "  星空 APK ok（内嵌 so = Xnbz 版）"

# ---------------------------------------------------------------- 2/6
echo "[2/6] com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
# 必需类（缺一即菜单/登录/去签名链条断裂）
for cls in StarryInitProvider menu/MenuService bridge/ModBridge sign/SignSpoof sign/TapLoginForceWeb; do
  find "$SMALI_ROOT/com" -type f -name "$(basename "$cls").smali" | grep -q . || {
    echo "ERROR: starry smali 缺 $cls" >&2; exit 1; }
done
# 两张表必须同步（SignSpoof 证书表 + TapLoginForceWeb 白名单），否则 TapTap SSO 报 signature not match
grep -q 'com[./]qqsy[./]xnbz' "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 未含 com.qqsy.xnbz（证书表未同步）" >&2; exit 1; }
grep -q 'com[./]qqsy[./]xnbz' "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali" \
  || { echo "ERROR: TapLoginForceWeb.smali 白名单未含 com.qqsy.xnbz" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi
[[ -f "$WORK/assets/certs/starlight.crt" ]] || { echo "ERROR: starlight.crt missing" >&2; exit 1; }

# ---------------------------------------------------------------- 3/6
# ★classes7.dex：TapTap SDK 新版路径两处改动
#   A) com/taptap/sdk/kit/internal/utils/TapSignKit.getSign(Context)Ljava/lang/String;
#      原实现 = pm.getPackageInfo(pkg, GET_SIGNATURES) → signatures[0].toByteArray() → CommonUtils.getMD5
#      ⇒ 重签后返回值变化，SDK 与服务端校验失败。固定返回原厂证书 MD5（CN=qqian）。
#      这条与 SignSpoof（PackageManager 侧回灌）互为双保险，缺一仍可能 signature not match。
#   B) com/taptap/sdk/login/impl/utils/ClientLoginUtils.isClientLogin(Activity, Actions)Z
#      原实现 = Intent(action).resolveActivity(pm) != null（即 TapTap 客户端 SSO 可用判定）
#      ⇒ 恒返回 false，强制走 WebLoginFragment 网页登录。
echo "[3/6] $TARGET_DEX TapSDK 登录/去签名补丁（TapSignKit.getSign + ClientLoginUtils.isClientLogin）"
unzip -p "$ORIG_APK" "$TARGET_DEX" > "$WORK/$TARGET_DEX"
"$BAKSMALI_BIN" d -o "$WORK/smali_c7" "$WORK/$TARGET_DEX"
python3.11 - "$WORK/smali_c7" "$ORIG_SIGN_MD5" <<'PY'
# 只在方法体内替换：保留方法签名 / .annotation / 参数声明，避免触发 dex 结构变化。
# ★寄存器纪律：用 `.locals N` 而不是 `.registers N`。带参数的方法用 .locals 时，
#   p0..pN 自动映射到 locals 之上的寄存器区，不会再踩「p 越界 / outs_size > registers_size」
#   导致 dex2oat 校验失败、装机后进程起不来（见 surgical_gyzw_1.0.5.sh 的同类踩坑记录）。
import sys
from pathlib import Path

root = Path(sys.argv[1])
sign_md5 = sys.argv[2]

TARGETS = [
    (
        root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali",
        ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;",
        '''    .locals 1

    # starry_sign_bypass: 固定返回原厂证书 MD5（CN=qqian），绕过重签导致的签名指纹变更
    const-string v0, "%s"

    return-object v0
''' % sign_md5,
        "TapSignKit.getSign",
    ),
    (
        root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali",
        ".method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z",
        '''    .locals 1

    # starry: force WebLogin —— resolveActivity 判定恒 false，强制走 WebLoginFragment 网页登录
    const/4 v0, 0x0

    return v0
''',
        "ClientLoginUtils.isClientLogin",
    ),
]

for path, sig, new_body, label in TARGETS:
    if not path.is_file():
        raise SystemExit("ERROR: missing %s (%s)" % (label, path))
    t = path.read_text(encoding="utf-8")
    if t.count(sig) != 1:
        raise SystemExit("ERROR: %s 签名命中 %d 次（期望 1）" % (label, t.count(sig)))
    start = t.index(sig)
    end = t.index("\n.end method", start)
    body = t[start:end]
    if "starry_sign_bypass" in body or "force WebLogin" in body:
        print("  %s already patched, skip" % label)
        continue
    # 保留 `.param` 声明（若有）与 .line 之外的一切前导注解；只换掉寄存器/指令区
    keep = ""
    for line in body.split("\n"):
        if line.strip().startswith(".param"):
            keep += line + "\n"
    new = sig + "\n" + keep + new_body + "\n"
    t = t[:start] + new + t[end:]
    path.write_text(t, encoding="utf-8")
    print("  %s patched" % label)
PY
TS_SMALI="$WORK/smali_c7/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
CL_SMALI="$WORK/smali_c7/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
grep -q 'starry_sign_bypass' "$TS_SMALI" || { echo "ERROR: TapSignKit 补丁未写入" >&2; exit 1; }
grep -q "$ORIG_SIGN_MD5"     "$TS_SMALI" || { echo "ERROR: 原厂证书 MD5 未写入 TapSignKit" >&2; exit 1; }
grep -q 'force WebLogin'     "$CL_SMALI" || { echo "ERROR: ClientLoginUtils 补丁未写入" >&2; exit 1; }
grep -q 'getPackageInfo(Ljava/lang/String;I)Landroid/content/pm/PackageInfo;' "$TS_SMALI" \
  && { echo "ERROR: TapSignKit 仍在读 PackageManager 签名（补丁未生效）" >&2; exit 1; } || true
echo "  两处补丁已写入并自检通过"
# 只允许目标方法被改：类集合必须与原 dex 完全一致（防误伤/防回编译丢类）
"$SMALI_BIN" assemble -o "$WORK/${TARGET_DEX%.dex}.patched.dex" "$WORK/smali_c7"
set +e
"$BAKSMALI_BIN" list classes "$WORK/${TARGET_DEX%.dex}.patched.dex" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c7_new.txt"
unzip -p "$ORIG_APK" "$TARGET_DEX" > "$WORK/c7_orig.dex"
"$BAKSMALI_BIN" list classes "$WORK/c7_orig.dex" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c7_old.txt"
set -e
diff -q "$WORK/c7_old.txt" "$WORK/c7_new.txt" >/dev/null \
  || { echo "ERROR: $TARGET_DEX 类集合在回编译后发生变化" >&2; diff "$WORK/c7_old.txt" "$WORK/c7_new.txt" | head -20; exit 1; }
echo "  $TARGET_DEX 类集合一致（$(wc -l < "$WORK/c7_new.txt" | tr -d ' ') 类），已注入登录/去签名补丁"
PATCHED_DEX="$WORK/${TARGET_DEX%.dex}.patched.dex"
ls -la "$PATCHED_DEX"

# ---------------------------------------------------------------- 4/6
echo "[4/6] AndroidManifest：分支 B（StarryInitProvider）+ MenuService + FGS/悬浮窗权限"
if [[ -n "${XNBZ_BINARY_MANIFEST:-}" && -f "${XNBZ_BINARY_MANIFEST:-}" ]]; then
  cp "$XNBZ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  from XNBZ_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  # 375MB 原包直接 apktool 太慢：用只含 Manifest+arsc+classes.dex 的迷你 zip 做一次
  # （文字 Manifest 改完再 apktool b 回二进制，落到 07_patched 缓存复用）
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name); data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    full = "android.permission." + perm
    # FOREGROUND_SERVICE 是 FOREGROUND_SERVICE_SPECIAL_USE 的前缀，必须精确匹配
    if ('android:name="%s"' % full) not in t:
        perms.append('    <uses-permission android:name="%s"/>' % full)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 分支 B：不替换官方 Application（com.cocos.game.GameApplication，负责 System.loadLibrary("cocos")
# 与引擎 native 初始化），改由 ContentProvider 在进程启动时拉起星空菜单。
# initOrder 取 int 上限 = 最优先初始化，保证 SignSpoof.install() 早于 TapTapKitInitProvider/ThemisProvider
# 读签名（否则 TapSignKit 可能已缓存真签名）。
if "com.starry.magic.StarryInitProvider" not in t:
    prov = ('''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="2147483647" />
''' % pkg)
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
print("  text Manifest patched (provider + service + perms)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  python3.11 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService"   "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "SYSTEM_ALERT_WINDOW"                 "$WORK/tree_new.txt" || { echo "ERROR: overlay perm missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE"      "$WORK/tree_new.txt" || { echo "ERROR: FGS perm missing" >&2; exit 1; }
  # 本作 Application 是 Cocos 标准生成类，必须保持原样（无壳，不走 application@name 替换）
  grep -q "com.cocos.game.GameApplication"      "$WORK/tree_new.txt" || { echo "ERROR: 官方 Application 丢失" >&2; exit 1; }
  grep -q "com.cocos.game.MyAppActivity"        "$WORK/tree_new.txt" || { echo "ERROR: 主 Activity 丢失" >&2; exit 1; }
  # TapTap 新版登录组件与渠道 SDK 组件必须原样保留
  grep -q "com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity" "$WORK/tree_new.txt" \
    || { echo "ERROR: TapTapLoginActivity 丢失" >&2; exit 1; }
  grep -q "com.taptap.sdk.kit.internal.TapTapKitInitProvider" "$WORK/tree_new.txt" \
    || { echo "ERROR: TapTapKitInitProvider 丢失" >&2; exit 1; }
  grep -q "$PKG" "$WORK/tree_new.txt" || { echo "ERROR: 包名丢失" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt"  > "$WORK/n.norm"
  LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行" >&2; diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | head -10; exit 1; }
  echo "  aapt2 校验通过（官方节点零丢失）"
fi

# ---------------------------------------------------------------- 5/6
echo "[5/6] zip surgical merge → $UNSIGNED"
python3.11 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$TARGET_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_dex, target_dex = sys.argv[4], sys.argv[5]

mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
cpatched = (work / (target_dex[:-4] + ".patched.dex")).read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

# 官方条目零改动基准：除 classes7.dex / AndroidManifest.xml 外全部逐字节保持
UNTOUCHED = [
    "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
    "classes5.dex", "classes6.dex", "classes8.dex", "classes9.dex", "classes10.dex",
    "resources.arsc",
    "lib/arm64-v8a/libcocos.so",
]
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"{new_dex} already in orig"
assert target_dex in names, f"orig apk missing {target_dex}"
for n in UNTOUCHED:
    assert n in names, f"orig apk missing {n}"

def is_sig(name):
    if name == "META-INF/MANIFEST.MF": return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith(SIG_EXT)

kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n_written = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    if name == "AndroidManifest.xml":
        data = mf
        ct = zipfile.ZIP_DEFLATED
    elif name == target_dex:
        data = cpatched
        ct = item.compress_type
    else:
        data = zin.read(item)
        ct = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ct
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
for name, data in [(new_dex, cnew)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
zin.close(); zout.close()

zi2, zo2 = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "r")
for name in UNTOUCHED:
    a = hashlib.sha256(zi2.read(name)).hexdigest()
    b = hashlib.sha256(zo2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zo2.namelist())
for name in kept_meta:
    assert name in out_names, f"lost {name}"
assert new_dex in out_names and target_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert "assets/certs/starlight.crt" in out_names
assert not any(x.upper().endswith(SIG_EXT) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries={n_written}; {new_dex}={len(cnew)}; {target_dex}={len(cpatched)}; untouched {len(UNTOUCHED)} verified")
zi2.close(); zo2.close()
PY

PKG_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != $SO_MD5" >&2; exit 1; }
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
for e in "$NEW_DEX" "$TARGET_DEX" "lib/arm64-v8a/libstarry_magic.so" "assets/certs/starlight.crt" \
         "classes.dex" "classes10.dex" "lib/arm64-v8a/libcocos.so"; do
  grep -q "^$e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done
echo "  包内 so md5 ok；$TARGET_DEX + $NEW_DEX 到位"

# ---------------------------------------------------------------- 6/6
echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
  KS_PASS="${STARRY_KS_PASS:-starry2026}"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,4p' || true
else
  java -jar "$SIGNER" -a "$UNSIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "${STARRY_KS_ALIAS:-starry}" \
    --ksPass "${STARRY_KS_PASS:-starry2026}" --ksKeyPass "${STARRY_KS_PASS:-starry2026}"
  CAND="$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)"
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
[[ -f "$SIGNED" ]] || { echo "ERROR: signing failed" >&2; exit 1; }
ls -lh "$SIGNED"
echo "signed_md5=$(md5 -q "$SIGNED")"
echo "signed_sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"

cat <<'EOM'
DONE.
装机（覆盖安装；官方 CN=qqian vs 星空 CN=StarryMagic 签名不同，冲突时需用户授权卸载）：
  bash 04_scripts/仙逆/xnbz_install.sh
  # 授权卸载重装（清本地数据）：
  XNBZ_ALLOW_UNINSTALL=1 bash 04_scripts/仙逆/xnbz_install.sh
EOM
