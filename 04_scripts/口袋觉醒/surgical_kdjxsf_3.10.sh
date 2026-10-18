#!/usr/bin/env bash
# ============================================================
# 口袋觉醒（私服 com.zcdsg.xmgm 3.10 / vc310）星光外科组装
#
# 原包逐字节保留：classes.dex / resources.arsc / assets/**（含 1.83GB 的
#   assets/ۦۖ۫/... 单一包体）/ lib/arm64-v8a/libcocos2dlua.so / libnp_protect_res.so。
# 仅做四件事：
#   1) AndroidManifest.xml：追加 StarryInitProvider(initOrder=2000) + MenuService
#      + 前台服务权限；保留原 Application 与 appComponentFactory
#   2) 新增 classes2.dex（星光 Java/smali；原包只有 classes.dex）
#   3) 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#   4) 删旧签名条目（META-INF/*.SF|RSA|MF），交由 sign 脚本重签
#
# 产物：08_output/口袋觉醒/口袋觉醒_私服_3.10_starry-unsigned.apk
#
# 为什么用 zip CLI 而不是 python zipfile 重写：
#   原包 1.93GB，其中单一 asset 条目 1.83GB。zipfile 重写会把该条目重新 deflate
#   （10 分钟级 CPU + 峰值磁盘翻倍）；zip CLI 只做条目搬运（压缩流原样复制）。
# ============================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="口袋觉醒"
VERSION="3.10"
PKG="com.zcdsg.xmgm"
ORIG_APK="${KDJXSF_BASE_APK:-$NIXIANG/02_apks/口袋觉醒/口袋觉醒_私服_3.10_official_from_device.apk}"
ORIG_MD5="${KDJXSF_BASE_MD5:-3e5c21450ff770de4cc27815dc52854f}"
NEW_DEX="classes2.dex"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_私服_${VERSION}_starry-unsigned.apk"
STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical_私服_${VERSION}"
MF_CACHE="$STAGE_DIR/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so"
[[ -f "$NDK_CXX" ]] || NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${KDJXSF_SO:-$STARRY_SRC/libs_kdjx/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT" >&2; exit 1; }
command -v zip >/dev/null || { echo "ERROR: zip not found" >&2; exit 1; }

ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] \
  || { echo "ERROR: 原包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }
echo "[check] base apk md5 ok ($ACTUAL_MD5)"
echo "[check] starry so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
# 过检补丁 dex（可选）：把 np 壳的 4 处退出调用改成 nop 后重编的 classes.dex。
# 不传 = 用原包 classes.dex（原样保留）。
PATCHED_DEX="${KDJXSF_PATCHED_DEX:-}"
if [[ -n "$PATCHED_DEX" ]]; then
  [[ -f "$PATCHED_DEX" ]] || { echo "ERROR: KDJXSF_PATCHED_DEX 不存在: $PATCHED_DEX" >&2; exit 1; }
  cp -f "$PATCHED_DEX" "$STAGE_DIR/classes.dex"
  echo "[check] 过检补丁 classes.dex = $STAGE_DIR/classes.dex ($(md5 -q "$STAGE_DIR/classes.dex"))"
fi
LC_ALL=C grep -ac 'Kdjx' "$SO_PREBUILT" >/dev/null \
  || { echo "ERROR: so 内找不到本游戏标签 [Kdjx]（可能打的是上一款）" >&2; exit 1; }

LOCK_DIR="/tmp/kdjxsf_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/kdjxsf_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$STAGE_DIR"

echo "[1/6] gradle assembleRelease（星光 Java 侧 → smali → ${NEW_DEX}）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for need in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/SignSpoof.smali"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
cp -f "$WORK/$NEW_DEX" "$STAGE_DIR/$NEW_DEX"
echo "  $NEW_DEX → $STAGE_DIR/$NEW_DEX ($(stat -f%z "$STAGE_DIR/$NEW_DEX") B, md5 $(md5 -q "$STAGE_DIR/$NEW_DEX"))"

echo "[3/6] binary AndroidManifest（保留原 Application / factory）"
if [[ -n "${KDJXSF_BINARY_MANIFEST:-}" && -f "${KDJXSF_BINARY_MANIFEST:-}" ]]; then
  cp "$KDJXSF_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $KDJXSF_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once（--no-assets 避免搬运 1.8GB 包体）"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
authority = sys.argv[3]
t = p.read_text(encoding="utf-8")

perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# 私服自有无校验登录链：**不替换 appComponentFactory**，只加 ContentProvider 入口
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="2000" />
''' % authority
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

# 签名伪装：私服壳读 PackageManager 签名做完整性判定，异签必自退
# ⇒ 走 SignSpoof（按包名回灌原厂证书），必须由 appComponentFactory 在
#    Application 创建之前装上。原包无自定义 factory，这里安全接管。
factory = "com.starry.magic.sign.SignSpoofAppComponentFactory"
if "appComponentFactory" not in t:
    i = t.find("<application")
    if i < 0:
        raise SystemExit("no <application> in manifest")
    j = t.find(">", i)
    t = t[:j] + ' android:appComponentFactory="%s"' % factory + t[j:]

p.write_text(t, encoding="utf-8")
print("  text Manifest patched (Application preserved, SignSpoof factory installed)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for need in \
    "com.starry.magic.StarryInitProvider" \
    "com.starry.magic.menu.MenuService"; do
    grep -q "$need" "$WORK/tree_new.txt" \
      || { echo "ERROR: 新 manifest 缺 $need" >&2; exit 1; }
  done
  python3 - "$WORK/tree_orig.txt" "$WORK/tree_new.txt" <<'PY'
import sys
from collections import Counter
from pathlib import Path

def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8').splitlines():
        if not ln.strip():
            continue
        ind = len(ln) - len(ln.lstrip(' '))
        core = ln.strip()
        if core.startswith('N:'):
            continue
        if core.startswith('E:'):
            lv = (ind + 2) // 4
            stack = stack[:lv - 1]
            stack.append(core[2:].strip().split(' ')[0])
            out.append('/'.join(stack))
        elif core.startswith('A:') and stack:
            out.append('/'.join(stack[:ind // 4]) + '|' + core)
    return Counter(out)

o, n = canon(sys.argv[1]), canon(sys.argv[2])
dropped = o - n
added = sum((n - o).values())
if dropped:
    for k, v in list(dropped.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建丢失官方节点（{sum(dropped.values())} 项）')
if added < 10:
    raise SystemExit(f'manifest 新增节点数过少（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 未丢弃官方节点)')
PY
else
  echo "  WARN: aapt2 不在 ${AAPT2}，跳过 manifest 回读校验" >&2
fi

echo "[5/6] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
ADD="$WORK/add"
mkdir -p "$ADD/lib/arm64-v8a"
cp "$WORK/AndroidManifest.xml" "$ADD/AndroidManifest.xml"
[[ -n "$PATCHED_DEX" ]] && cp "$STAGE_DIR/classes.dex" "$ADD/classes.dex"
cp "$STAGE_DIR/$NEW_DEX" "$ADD/$NEW_DEX"
cp "$SO_PREBUILT" "$ADD/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$ADD/lib/arm64-v8a/libc++_shared.so"

rm -f "$UNSIGNED"
cp "$ORIG_APK" "$UNSIGNED"
zip -q -d "$UNSIGNED" "META-INF/*.SF" "META-INF/*.RSA" "META-INF/*.DSA" "META-INF/MANIFEST.MF" AndroidManifest.xml 2>/dev/null || true
[[ -n "$PATCHED_DEX" ]] && { zip -q -d "$UNSIGNED" classes.dex 2>/dev/null || true; }
(cd "$ADD" && zip -q -X -r "$UNSIGNED" .)

echo "[5b] 结构校验"
python3 - "$ORIG_APK" "$UNSIGNED" "$STAGE_DIR/$NEW_DEX" "$SO_PREBUILT" "${PATCHED_DEX:--}" <<'PY'
import sys, zipfile, hashlib
orig, new, dexp, sop = sys.argv[1:5]
patched = sys.argv[5] if len(sys.argv) > 5 else "-"
with zipfile.ZipFile(orig) as zo, zipfile.ZipFile(new) as zn:
    on, nn = set(zo.namelist()), set(zn.namelist())
    keep = ['resources.arsc',
            'lib/arm64-v8a/libcocos2dlua.so', 'lib/arm64-v8a/libnp_protect_res.so']
    if patched == "-":
        keep.append('classes.dex')
    for k in keep:
        if k not in nn:
            raise SystemExit(f'{k} 在产物中缺失')
        a, b = zo.getinfo(k), zn.getinfo(k)
        if (a.CRC, a.file_size) != (b.CRC, b.file_size):
            raise SystemExit(f'{k} 字节被改动（CRC/size 不一致）')
    for need in ['classes2.dex', 'lib/arm64-v8a/libstarry_magic.so',
                 'lib/arm64-v8a/libc++_shared.so', 'AndroidManifest.xml']:
        if need not in nn:
            raise SystemExit(f'{need} 缺失')
    if zn.read('lib/arm64-v8a/libstarry_magic.so') != open(sop, 'rb').read():
        raise SystemExit('包内 starry so != 构建产物')
    if zn.read('classes2.dex') != open(dexp, 'rb').read():
        raise SystemExit('包内 classes2.dex != 构建产物')
    if patched != "-":
        if zn.read('classes.dex') != open(patched, 'rb').read():
            raise SystemExit('包内 classes.dex != 过检补丁产物')
    lost = on - nn
    for n in lost:
        if not (n.startswith('META-INF/') or n == 'AndroidManifest.xml'):
            raise SystemExit(f'原始条目丢失: {n[:80]}')
    print(f'  ok: {len(nn)} 条目录入（原 {len(on)}），关键条目字节未变')
PY

echo "[6/6] surgical done"
echo "[surgical] unsigned = $UNSIGNED"
echo "[surgical] size = $(stat -f%z "$UNSIGNED") B  md5 = $(md5 -q "$UNSIGNED")"
ls -lh "$UNSIGNED"
