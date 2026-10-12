#!/usr/bin/env bash
# ============================================================================
# 疯狂水世界 1.11.6（com.gx.sw.qa.fkssj004.esj，versionCode 480，arm64-v8a）
#   → 星空魔法外科组装（surgical）：官方受保护条目**逐字节透传**，只增不改
#
# 引擎：Cocos Creator 3.x / lib/arm64-v8a/libcocos.so（JSB；无 IL2CPP）
#       **无整包加固壳**（无 libjiagu / libtprt / StubApp）⇒ 走外科路线，不需要脱壳。
#
# 产出（本脚本**只产 unsigned**，签名交给后续脚本）：
#   08_output/疯狂水世界/疯狂水世界_1.11.6_surgical-unsigned.apk
#
# 注入内容
#   classes5.dex                      星空菜单（com.starry.magic.**）：
#                                     StarryInitProvider / menu.MenuService
#                                     / bridge.ModBridge / sign.SignSpoof
#                                     下标由 04_scripts/_common/next_free_dex.py 动态求得
#                                     （官方占 classes.dex ~ classes4.dex ⇒ 实测 5）
#   lib/arm64-v8a/libstarry_magic.so   native（Cocos JSB 钩子 + 菜单渲染 + 证书固定）
#   AndroidManifest.xml               入口 = 分支 B（ContentProvider）：
#                                     <provider com.starry.magic.StarryInitProvider
#                                       authorities="com.gx.sw.qa.fkssj004.esj.starryinit"
#                                       exported=false initOrder=2147483647>
#                                     + <service com.starry.magic.menu.MenuService
#                                       foregroundServiceType="specialUse">
#                                     + SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE
#                                       / FOREGROUND_SERVICE_SPECIAL_USE
#                                     ★绝不替换 application@name：官方为
#                                       android.support.multidex.MultiDexApplication（管 multidex
#                                       装载），替换即崩；Provider.onCreate 早于
#                                       Activity.onCreate，是 SKILL.md 首选入口
#
# 本作 N/A
#   · 无 smali 目标：全 dex 无 com.taptap.sdk.login.* / TapSignKit / ClientLoginUtils
#     ⇒ 第 3 步跳过（不做证书回灌、不做强制网页登录）
#   · 星光 dex 断言表**不含** sign/TapLoginForceWeb（该作无 TapTap 登录 SDK）
#
# 字节不变（第 5 步对**除 AndroidManifest.xml 与 META-INF 签名件外的全部官方条目**
#   逐条比对「原始压缩流 sha256 + CRC + 压缩方式 + 两侧长度」，共 9017 条）：
#     classes.dex / classes2.dex / classes3.dex / classes4.dex（明文，无壳）
#     resources.arsc（必须保持 STORED；官方即 Stored 975128 B —— 压缩会被 AfkAssets 拒）
#
# 底包政策 / 再获取（真机只读拉取，不留本地副本）
#   设备来源：/data/app/~~MvgqQpfN4itmIKrbuTs17Q==/com.gx.sw.qa.fkssj004.esj-2MLxV7o_dBw4RvTVvTNsdw==/base.apk
#   取包方式（真机无 root，/data/app 对 shell 可读 ⇒ 不需要 su）：
#     adb -s 96f0d883 pull /data/app/~~MvgqQpfN4itmIKrbuTs17Q==/com.gx.sw.qa.fkssj004.esj-2MLxV7o_dBw4RvTVvTNsdw==/base.apk 09_test/疯狂水世界/base/base.apk
#   硬指纹：md5 == f8ed1b954a3c5ce91dcecb4e721f3e9b（不等即 ERROR 退出，防出成上一款/半包）
#
# 调用（必须在仓库任意目录均可，路径全部根锚定）：
#   bash mod菜单/starry-mod-menu/scripts/surgical_fkssj_1.11.6.sh
# 可选环境变量
#   FKSJ_BASE_APK        底包路径（缺省 09_test/疯狂水世界/base/base.apk）
#   FKSJ_SO              星光 so（缺省 mod菜单/starry-mod-menu/libs_fkssj/arm64-v8a/libstarry_magic.so）
#   FKSJ_BINARY_MANIFEST 直接复用已注入的二进制 Manifest（跳过 apktool 路线，排障/重跑用）
#   FKSJ_KEEP_WORK=1     保留临时目录
#   FKSJ_GRADLE_OFFLINE=1 追加 --offline（默认照抄 xnbz：不加，首次需联网拉依赖）
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# 本脚本放在 mod菜单/starry-mod-menu/scripts/ 下（与 surgical_xnbz_* 同处）
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../../.." && pwd)}"

GAME="疯狂水世界"
VERSION="1.11.6"
VERSION_CODE="480"
PKG="com.gx.sw.qa.fkssj004.esj"

ORIG_APK="${FKSJ_BASE_APK:-$NIXIANG/09_test/疯狂水世界/base/base.apk}"
ORIG_MD5="f8ed1b954a3c5ce91dcecb4e721f3e9b"
SO_PREBUILT="${FKSJ_SO:-$STARRY_SRC/libs_fkssj/arm64-v8a/libstarry_magic.so}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SIGN_SCRIPT="$NIXIANG/04_scripts/$GAME/fkssj_sign.sh"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NEXT_FREE_DEX="$NIXIANG/04_scripts/_common/next_free_dex.py"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
GRADLEW="$STARRY_SRC/gradlew"
PY_BIN="${PYTHON:-python3}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

# ---------------------------------------------------------------- 前置门禁
[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺底包 ${ORIG_APK}（见脚本头「底包政策 / 再获取」）" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺 $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: 缺 $BAKSMALI_BIN" >&2; exit 1; }
[[ -x "$GRADLEW" ]] || { echo "ERROR: 缺 $GRADLEW" >&2; exit 1; }
[[ -f "$NEXT_FREE_DEX" ]] || { echo "ERROR: 缺 $NEXT_FREE_DEX" >&2; exit 1; }
command -v apktool >/dev/null 2>&1 || { echo "ERROR: 缺 apktool（brew install apktool）" >&2; exit 1; }
command -v zip >/dev/null 2>&1 || { echo "ERROR: 缺 zip" >&2; exit 1; }
command -v "$PY_BIN" >/dev/null 2>&1 || { echo "ERROR: 缺 $PY_BIN" >&2; exit 1; }

echo "[check] 底包 = $ORIG_APK"
GOT_MD5="$(md5 -q "$ORIG_APK")"
if [[ "$GOT_MD5" != "$ORIG_MD5" ]]; then
  echo "ERROR: 底包指纹不符 —— md5=$GOT_MD5 期望 $ORIG_MD5" >&2
  echo "       期望包 = ${GAME} ${VERSION}（vc${VERSION_CODE}，${PKG}，arm64-v8a）" >&2
  exit 1
fi
echo "[check] 底包 md5 ok（${ORIG_MD5}）"

# 包名/版本号二重复读（防「指纹对但底包被替换」的极窄窗口）
if [[ -x "$AAPT2" ]]; then
  BADGE="$("$AAPT2" dump badging "$ORIG_APK" 2>/dev/null | head -1 || true)"
  case "$BADGE" in
    *"name='$PKG'"*"versionCode='$VERSION_CODE'"*"versionName='$VERSION'"*)
      echo "[check] badging ok（$PKG vc$VERSION_CODE ${VERSION}）" ;;
    *) echo "ERROR: badging 不符 —— $BADGE" >&2; exit 1 ;;
  esac
fi

# 星光 so 归属校验（防串包：上一款游戏的 so 顶包；中文标签按字节搜，strings 默认 ASCII 会漏报）
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: 缺 ${SO_PREBUILT}（先跑 04_scripts/$GAME/fkssj_build_so.sh）" >&2; exit 1; }
if ! LC_ALL=C grep -a -q '\[Fkssj\]' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 不是 Fkssj 版（缺 [Fkssj] 日志标签）" >&2
  strings "$SO_PREBUILT" | grep -aE '^\[(Fkssj|Xnbz|Haly|Gyzw|Jfqs|Dwct|Ysxz|Sjzz|Zmxy)' | sort -u | head -20 || true
  exit 1
fi
# 注意：必须转义方括号，否则 grep 把 "[Xnbz]" 当字符类 → 匹配任意 X/n/b/z 字节 → 恒真误报
for s in '\[Xnbz\]' '\[Haly\]' '\[Gyzw\]' '\[Jfqs\]' '\[Dwct\]' '\[Ysxz\]' '\[Sjzz\]' '\[Zmxy\]'; do
  if LC_ALL=C grep -a -q -- "$s" "$SO_PREBUILT"; then
    echo "ERROR: $SO_PREBUILT 含别家标签 ${s}（libs_fkssj 被并行会话覆盖）" >&2; exit 1
  fi
done
SO_MD5="$(md5 -q "$SO_PREBUILT")"
SO_SIZE="$(stat -f%z "$SO_PREBUILT")"
echo "[check] so = $SO_PREBUILT md5=${SO_MD5}（${SO_SIZE} B）"

# 新 dex 下标：动态求（官方 4 个 dex 时 = 5），不写死猜测
DEX_NUM="$("$PY_BIN" "$NEXT_FREE_DEX" "$ORIG_APK")"
[[ -n "$DEX_NUM" ]] || { echo "ERROR: next_free_dex.py 无输出" >&2; exit 1; }
NEW_DEX="classes${DEX_NUM}.dex"
if [[ "$NEW_DEX" != "classes5.dex" ]]; then
  echo "  提醒：官方 dex 占用与实测（4 个）不同，新 dex = ${NEW_DEX}（动态求得，继续）"
fi
echo "[check] 新 dex = $NEW_DEX"

LOCK_DIR="/tmp/fkssj_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held（并发会话正在建同一款）" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/fkssj_surgical.XXXXXX)"
cleanup() {
  rc=$?
  rm -f "$LOCK_DIR/pid" 2>/dev/null || true
  rmdir "$LOCK_DIR" 2>/dev/null || true
  if [[ "${FKSJ_KEEP_WORK:-0}" == "1" ]]; then
    echo "[work] 保留临时目录 $WORK"
  else
    rm -rf "$WORK"
  fi
  return $rc
}
trap cleanup EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"
# 官方条目清单落盘一次（9017 行，瞬时）——后续所有「是否已存在」判定都 grep 文件，
# 不用 `unzip -Z1 | grep -q`（grep -q 提前退出会给 unzip 发 SIGPIPE，pipefail 下变假阴性）
unzip -Z1 "$ORIG_APK" > "$WORK/orig_entries.txt"
echo "[check] work = ${WORK}（官方条目 $(wc -l < "$WORK/orig_entries.txt" | tr -d ' ') 条）"

# ---------------------------------------------------------------- 1/6
echo "[1/6] gradle assembleRelease（星空 APK → smali）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
if [[ "${FKSJ_GRADLE_OFFLINE:-0}" == "1" ]]; then
  (cd "$WORK/starry" && ./gradlew assembleRelease --offline --console=plain)
else
  (cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
fi
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: 缺 $STARRY_APK" >&2; exit 1; }
# 星空 APK 内 so 必须是 Fkssj 版（防 jniLibs 未刷新 / 上一款顶包）
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/starry_inner.so"
if ! LC_ALL=C grep -a -q '\[Fkssj\]' "$WORK/starry_inner.so"; then
  echo "ERROR: 星空 APK 内 so 不是 Fkssj 版（jniLibs 未刷新，先跑 04_scripts/$GAME/fkssj_build_so.sh）" >&2
  strings "$WORK/starry_inner.so" | grep -aE '^\[(Fkssj|Xnbz|Haly|Gyzw|Jfqs|Dwct|Ysxz)' | sort -u | head || true
  exit 1
fi
echo "  星空 APK ok（内嵌 so 含 [Fkssj]）"

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
# 必需类（缺一即菜单/入口或 native 桥断裂）
# 用「find 落盘 + 判文件非空」而不是 `find | grep -q`：grep -q 命中即退出会给 find 发 SIGPIPE，
# pipefail 下整条管道变非零 ⇒ 假「缺失」误拦
for cls in StarryInitProvider menu/MenuService bridge/ModBridge sign/SignSpoof; do
  find "$SMALI_ROOT/com" -type f -name "$(basename "$cls").smali" > "$WORK/f_ind.txt"
  [[ -s "$WORK/f_ind.txt" ]] || { echo "ERROR: starry smali 缺 $cls" >&2; exit 1; }
done
# 本作无 TapTap 登录 SDK ⇒ TapLoginForceWeb 不在必需表；在也属死代码，只提示不拦
find "$SMALI_ROOT/com" -type f -name 'TapLoginForceWeb.smali' > "$WORK/f_tap.txt"
if [[ -s "$WORK/f_tap.txt" ]]; then
  echo "  提示：starry dex 含 TapLoginForceWeb（本作无 TapTap 登录 SDK，属无害死代码，未列入必需表）"
fi
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
[[ -f "$WORK/$NEW_DEX" ]] || { echo "ERROR: smali 汇编未产出 $NEW_DEX" >&2; exit 1; }
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
# libc++_shared.so：官方包已自带（1292896 B）⇒ 绝不覆盖（它是受保护条目，必须逐字节透传）
if grep -qx 'lib/arm64-v8a/libc++_shared.so' "$WORK/orig_entries.txt"; then
  echo "  官方已含 lib/arm64-v8a/libc++_shared.so → 不注入（保持逐字节透传）"
elif [[ -f "$WORK/starry_dec/lib/arm64-v8a/libc++_shared.so" ]]; then
  cp "$WORK/starry_dec/lib/arm64-v8a/libc++_shared.so" "$WORK/lib/arm64-v8a/libc++_shared.so"
  echo "  官方无 libc++_shared.so → 从星空 APK 补入"
fi
# assets/certs：2026-09-06 起证书已烘焙进 so（本作 so 内 'BEGIN CERTIFICATE' 命中 1 次），
# 代码不再读该文件。仅当官方包内无同名路径时才补，避免覆盖受保护条目。
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  if grep -q '^assets/certs/' "$WORK/orig_entries.txt"; then
    echo "  官方已含 assets/certs/ → 不覆盖（保持逐字节透传）"
  else
    mkdir -p "$WORK/assets/certs"
    cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
    echo "  补入 assets/certs/（可省，证书已在 so 内）"
  fi
fi

# ---------------------------------------------------------------- 3/6
echo "[3/6] 本作 N/A：无 smali 目标（无 TapTap 登录类），跳过"
echo "  ${PKG} 全 dex 无 com.taptap.sdk.login.* / TapSignKit / ClientLoginUtils"
echo "  ⇒ 无证书回灌补丁、无强制网页登录补丁、不做任何方法体改写。"
echo "  classes.dex ~ classes4.dex 全 4 个 dex 保持逐字节透传（最终断言见 [6/6]）。"
TAP_HIT=0
for d in classes.dex classes2.dex classes3.dex classes4.dex; do
  n="$(unzip -p "$ORIG_APK" "$d" | LC_ALL=C grep -aEc 'TapSignKit|ClientLoginUtils|com/taptap/sdk/login' || true)"
  if [[ "${n:-0}" != "0" ]]; then
    echo "  注意：${d} 命中 TapTap 登录类字节串 ${n} 处"
    TAP_HIT=1
  fi
done
if [[ "$TAP_HIT" == "1" ]]; then
  echo "WARN: 命中数与「无 TapTap 登录 SDK」的实测事实不符 —— 请人工复核是否需 smali 补丁（本脚本仍按 N/A 跳过）" >&2
fi

# ---------------------------------------------------------------- 4/6
echo "[4/6] AndroidManifest：分支 B（StarryInitProvider）+ MenuService + FGS/悬浮窗权限"
if [[ -n "${FKSJ_BINARY_MANIFEST:-}" && -f "${FKSJ_BINARY_MANIFEST:-}" ]]; then
  cp "$FKSJ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  from FKSJ_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  # 520MB 原包直接 apktool 太慢：用只含 Manifest+arsc+classes.dex 的迷你 zip 做一次
  # （文字 Manifest 改完再 apktool b 回二进制，落到 07_patched 缓存复用）
  "$PY_BIN" - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
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
  "$PY_BIN" - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
if "com.starry.magic.StarryApp" in t:
    raise SystemExit("ERROR: 文字 Manifest 出现 StarryApp —— 本作禁止替换 application@name")
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
# 分支 B：不替换官方 Application（android.support.multidex.MultiDexApplication，负责
# multidex 装载），改由 ContentProvider 在进程启动时拉起星空菜单。
# initOrder 取 int 上限 = 最优先初始化（平台按 initOrder 降序实例化 Provider），
# 保证 SignSpoof.install() 早于官方其它 Provider（sigmob initOrder=200）读签名。
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
[[ -s "$WORK/AndroidManifest.xml" ]] || { echo "ERROR: 二进制 Manifest 为空" >&2; exit 1; }
ls -la "$WORK/AndroidManifest.xml"

if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  "$PY_BIN" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
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
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE"      "$WORK/tree_new.txt" || { echo "ERROR: FGS specialUse perm missing" >&2; exit 1; }
  grep -q "\"android.permission.FOREGROUND_SERVICE\"" "$WORK/tree_new.txt" || { echo "ERROR: FGS perm missing" >&2; exit 1; }
  # 本作 Application = android.support.multidex.MultiDexApplication，必须原样保留
  grep -q "android.support.multidex.MultiDexApplication" "$WORK/tree_new.txt" \
    || { echo "ERROR: 官方 Application 丢失（被替换？）" >&2; exit 1; }
  if grep -q "com.starry.magic.StarryApp" "$WORK/tree_new.txt"; then
    echo "ERROR: 新 manifest 出现 StarryApp（本作禁止替换 application@name）" >&2; exit 1
  fi
  grep -q "com.cocos.game.AppActivity" "$WORK/tree_new.txt" || { echo "ERROR: 主 Activity 丢失" >&2; exit 1; }
  grep -q "$PKG" "$WORK/tree_new.txt" || { echo "ERROR: 包名丢失" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt"  > "$WORK/n.norm"
  LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行" >&2; diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | head -10; exit 1; }
  echo "  aapt2 校验通过（官方节点零丢失，Application 未替换）"
else
  echo "  WARN: 未找到 aapt2（${AAPT2}），跳过零丢失校验" >&2
fi

# ---------------------------------------------------------------- 5/6
echo "[5/6] zip surgical merge → $UNSIGNED"
cp -f "$ORIG_APK" "$UNSIGNED"
zip -q -d "$UNSIGNED" 'META-INF/*' >/dev/null 2>&1 || true

STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$STAGE/AndroidManifest.xml"
cp -f "$WORK/$NEW_DEX" "$STAGE/$NEW_DEX"
cp -f "$WORK/lib/arm64-v8a/libstarry_magic.so" "$STAGE/lib/arm64-v8a/"
if [[ -f "$WORK/lib/arm64-v8a/libc++_shared.so" ]]; then
  cp -f "$WORK/lib/arm64-v8a/libc++_shared.so" "$STAGE/lib/arm64-v8a/"
fi
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
fi
# ★ zip -u 用 mtime 判定是否覆盖「已存在条目」（Info-ZIP 语义）：
#   暂存件若比官方 entry（2026-09-28）旧，会被**静默跳过**（实测返回 0 但不更新）
#   ⇒ 必须先把暂存件 mtime 刷成现在；下面的 manifest 内容断言是最终兜底。
find "$STAGE" -type f -exec touch {} +
(cd "$STAGE" && find . -type f | sed 's|^\./||' | sort > "$WORK/zip_list.txt")
(cd "$STAGE" && zip -q -u "$UNSIGNED" -@ < "$WORK/zip_list.txt")
ls -lh "$UNSIGNED"

"$PY_BIN" - "$ORIG_APK" "$UNSIGNED" "$NEW_DEX" "$WORK/AndroidManifest.xml" "$WORK/zip_list.txt" <<'PY'
# 逐条比对：除 AndroidManifest.xml 与 META-INF 签名件外的**全部**官方条目，
# 必须「原始压缩流 sha256 + 压缩方式 + 压缩长度 + 原始长度 + CRC32」全等
# （比对原始压缩流比比对解压内容更严：连压缩字节都要求逐字节一致）。
import sys, zipfile, hashlib, struct
from pathlib import Path

orig, out, new_dex, staged_mf, list_file = (Path(sys.argv[1]), Path(sys.argv[2]),
                                            sys.argv[3], Path(sys.argv[4]), Path(sys.argv[5]))
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")


def is_sig(name):
    if name == "META-INF/MANIFEST.MF":
        return True
    if not name.startswith("META-INF/"):
        return False
    return name.upper().endswith(SIG_EXT)


class Raw:
    def __init__(self, path):
        self.path = path
        self.zf = zipfile.ZipFile(path)
        self.f = open(path, "rb")

    def fp(self, name):
        zi = self.zf.getinfo(name)
        # 优先比对**原始压缩流**（比解压内容更强：连压缩字节都要求逐字节一致）。
        # 老/新 python 的 offset 属性名不同 ⇒ 拿不到就退回比对解压内容 + CRC + 长度。
        off = getattr(zi, "header_offset", None)
        if off is None:
            off = getattr(zi, "_header_offset", None)
        if off is not None:
            try:
                self.f.seek(off)
                h = self.f.read(30)
                if h[:4] == b"PK\x03\x04":
                    n, e = struct.unpack("<HH", h[26:30])
                    self.f.seek(off + 30 + n + e)
                    data = self.f.read(zi.compress_size)
                    if len(data) == zi.compress_size:
                        return (zi.compress_type, zi.compress_size, zi.file_size, zi.CRC,
                                hashlib.sha256(data).hexdigest(), "raw")
            except Exception:
                pass
        return (zi.compress_type, zi.compress_size, zi.file_size, zi.CRC,
                hashlib.sha256(self.zf.read(name)).hexdigest(), "content")

    def close(self):
        self.zf.close(); self.f.close()


ro, rn = Raw(orig), Raw(out)
on, nn = set(ro.zf.namelist()), set(rn.zf.namelist())

assert new_dex in nn, "ERROR: 输出缺 %s" % new_dex
assert new_dex not in on, "ERROR: %s 已存在于原包" % new_dex
for n in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "resources.arsc"):
    assert n in nn, "ERROR: 输出缺 %s" % n
assert "AndroidManifest.xml" in nn, "ERROR: 输出缺 AndroidManifest.xml"
assert "lib/arm64-v8a/libstarry_magic.so" in nn, "ERROR: 输出缺 libstarry_magic.so"

# 暂存件必须全部落地，且 AndroidManifest.xml 内容 == 暂存版本（zip -u 静默跳过兜底）
staged = [l.strip() for l in list_file.read_text().splitlines() if l.strip()]
for n in staged:
    assert n in nn, "ERROR: 暂存件未落地：%s（zip -u 静默跳过？见脚本 mtime 注释）" % n
mf = rn.zf.read("AndroidManifest.xml")
assert mf == staged_mf.read_bytes(), \
    "ERROR: 包内 AndroidManifest.xml != 暂存版本（zip -u 因 mtime 静默跳过，内容未替换）"

protected = sorted(n for n in on if n != "AndroidManifest.xml" and not is_sig(n))
missing = [n for n in protected if n not in nn]
assert not missing, "ERROR: 输出丢失官方条目 %d 条：%s" % (len(missing), missing[:5])

diff = []
for n in protected:
    if ro.fp(n) != rn.fp(n):
        diff.append(n)
assert not diff, "ERROR: 受保护条目被改写 %d 条：%s" % (len(diff), diff[:5])

arsc_method = rn.zf.getinfo("resources.arsc").compress_type
assert arsc_method == zipfile.ZIP_STORED, "ERROR: resources.arsc 不是 STORED（compress_type=%d）" % arsc_method

leftover = [n for n in nn if is_sig(n)]
assert not leftover, "ERROR: 输出残留签名件：%s" % leftover

ro.close(); rn.close()
print("  merge ok：官方 %d 条受保护条目逐字节一致（含 classes.dex~classes4.dex）；"
      "resources.arsc STORED；新增 %s + libstarry_magic.so；签名件已清" % (len(protected), new_dex))
PY

unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
for e in "$NEW_DEX" "classes.dex" "classes2.dex" "classes3.dex" "classes4.dex" \
         "resources.arsc" "AndroidManifest.xml" "lib/arm64-v8a/libstarry_magic.so"; do
  grep -q "^${e}\$" "$WORK/entries.txt" || { echo "ERROR: $e 未在包内" >&2; exit 1; }
done
echo "  条目清单 ok（$(wc -l < "$WORK/entries.txt" | tr -d ' ') 条）"

# ---------------------------------------------------------------- 6/6
echo "[6/6] 成品自检（unsigned 交付；签名交给 fkssj_sign.sh）"
PKG_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$SO_MD5" ]] || {
  echo "ERROR: 包内 so md5=$PKG_SO_MD5 != 构建产物 md5=${SO_MD5}（旧 so 混包）" >&2; exit 1; }
unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/pkg.so"
LC_ALL=C grep -a -q '\[Fkssj\]' "$WORK/pkg.so" \
  || { echo "ERROR: 包内 so 缺 [Fkssj] 标签" >&2; exit 1; }
for s in '\[Xnbz\]' '\[Haly\]' '\[Gyzw\]' '\[Jfqs\]'; do
  if LC_ALL=C grep -a -q -- "$s" "$WORK/pkg.so"; then
    echo "ERROR: 包内 so 含别家标签 ${s}" >&2; exit 1
  fi
done
echo "  包内 so md5 ok（= ${SO_MD5}，含 [Fkssj]，无别家标签）"

# classes.dex ~ classes4.dex 与原包逐字节相同（本作无 smali 补丁，必须完全未动）
"$PY_BIN" - "$ORIG_APK" "$UNSIGNED" <<'PY'
import sys, zipfile, hashlib
zn, zo = zipfile.ZipFile(sys.argv[2]), zipfile.ZipFile(sys.argv[1])
for n in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"):
    a = hashlib.sha256(zo.read(n)).hexdigest()
    b = hashlib.sha256(zn.read(n)).hexdigest()
    assert a == b, "ERROR: %s 被改写（%s != %s）" % (n, b, a)
    print("  %s sha256=%s（与原包一致）" % (n, b[:16]))
PY

# 新增 dex 内容自检（入口类必须在里面，否则装机后 ClassNotFoundException=黑屏无菜单）
unzip -p "$UNSIGNED" "$NEW_DEX" > "$WORK/pkg_new.dex"
for cls in 'Lcom/starry/magic/StarryInitProvider;' 'Lcom/starry/magic/menu/MenuService;' \
           'Lcom/starry/magic/bridge/ModBridge;' 'Lcom/starry/magic/sign/SignSpoof;'; do
  LC_ALL=C grep -a -q "$cls" "$WORK/pkg_new.dex" || {
    echo "ERROR: $NEW_DEX 内缺 $cls" >&2; exit 1; }
done
echo "  $NEW_DEX 含 4 个必需类（StarryInitProvider / MenuService / ModBridge / SignSpoof）"

# 注意：awk 里不要 `exit`（提前退出会给 unzip 发 SIGPIPE，pipefail 下赋值失败 → 无提示中断）
ARSC_METHOD="$(unzip -v "$UNSIGNED" resources.arsc | awk '/resources[.]arsc/ {print $2}')"
[[ "$ARSC_METHOD" == "Stored" ]] || { echo "ERROR: resources.arsc 压缩方式=${ARSC_METHOD}（必须 Stored）" >&2; exit 1; }
echo "  resources.arsc = Stored ok"

ls -lh "$UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_sha256=$(shasum -a 256 "$UNSIGNED" | awk '{print $1}')"
echo "unsigned_size=$(stat -f%z "$UNSIGNED")"

echo
echo "DONE（unsigned 已就绪，本脚本不签名）"
echo "下一步跑签名脚本："
echo "  bash 04_scripts/$GAME/fkssj_sign.sh"
if [[ ! -f "$SIGN_SCRIPT" ]]; then
  echo "  （提示：$SIGN_SCRIPT 目前不存在，需先生成；本脚本只产 unsigned）"
fi
echo "装机（签名与官方不同，冲突时需用户明确同意卸载官方包）："
echo "  adb install -r -d 08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
