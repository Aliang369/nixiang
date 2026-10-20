#!/usr/bin/env bash
# 无悔华夏 B 站渠道（com.yhhy.whrhx.bilibili）— 无壳外科重建 + 星光注入。
#
# 背景（2026-10-21 实锤）：
#   - 真机现装包 = 网易易盾 NIS 新版壳（libnesec.so 带 .gnu.fragment/.gnu.draft/.gnu.stub
#     自定义段，且 classes.dex 只有 57,296B 壳 stub）⇒ 重签后壳静默退出，外科替换 dex 死路。
#   - 乐变 overlay 运行时 so `lebian/<pkg>-1/lib/arm64/libil2cpp.so` sha256
#     79adda770c528633213efe26bd084d14839c58a3a5382007de19cfc3d897aec9（== 九游 20261011 基准）
#     ⇒ 功能 RVA 表、WhrhxMod 分支、字段偏移全部可直接复用。
#   - 真实业务 DEX 由壳以 InMemoryDexFile 提供，已用 /proc/<pid>/mem 精确导出三份：
#       8,679,388B（乐变 lbvmrt）/ 5,491,364B（游戏主 dex，含 com/tpf/sdk/**）/ 1,108,844B
#
# 输入：
#   BASE_APK  真机运行时 base.odex.apk（含热更后资源 + 运行时 so）
#   DEX1..3   真实 DEX 三件套
#   MENU_DEX  classes4.dex（com/starry/**，StarryApp 父类 = Lcom/excelliance/open/LBApplication;）
#   SO        libs_whrhxbili/arm64-v8a/libstarry_magic.so（game_id=whhxbili）
# 输出：
#   08_output/无悔华夏bili/无悔华夏B站_3.8.280_无壳星光_<date>-releaseSigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
DATE_TAG="${WHRHX_BILI_DATE:-$(date +%Y%m%d)}"
BASE_APK="${WHRHX_BILI_BASE:-$NIXIANG/06_dumped/无悔华夏/bilibili_20261021/base.odex.apk}"
DEX_DIR="${WHRHX_BILI_DEX_DIR:-$NIXIANG/06_dumped/无悔华夏/bilibili_20261021/realdex/whrhx_dex5}"
DEX1="${WHRHX_BILI_DEX1:-$DEX_DIR/mem_736dcc2ffc_5491364.dex}"
DEX2="${WHRHX_BILI_DEX2:-$DEX_DIR/mem_736b8baffc_8679388.dex}"
DEX3="${WHRHX_BILI_DEX3:-$DEX_DIR/mem_7371e0effc_1108844.dex}"
MENU_DEX="${WHRHX_BILI_MENU_DEX:-$NIXIANG/07_patched/无悔华夏bili/menu_latest_$DATE_TAG/classes4.dex}"
SO_BUILT="${WHRHX_BILI_SO:-$STARRY_SRC/libs_whrhxbili/arm64-v8a/libstarry_magic.so}"
OUT="$NIXIANG/08_output/无悔华夏bili"
SIGNED="$OUT/无悔华夏B站_3.8.280_无壳星光_${DATE_TAG}-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

for f in "$BASE_APK" "$DEX1" "$DEX2" "$DEX3" "$MENU_DEX" "$SO_BUILT" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing input: $f" >&2; exit 1; }
done
command -v apktool >/dev/null || { echo "ERROR: apktool missing" >&2; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools (zipalign/apksigner) not found" >&2; exit 1; }

# so 身份门禁：必须是 whhxbili 渠道、带构建戳
LC_ALL=C grep -a -q 'whhxbili' "$SO_BUILT" || { echo "ERROR: so 不是 whhxbili 渠道" >&2; exit 1; }
LC_ALL=C grep -a -q 'target=bilibili' "$SO_BUILT" || { echo "ERROR: so 缺 target=bilibili 标签" >&2; exit 1; }
LC_ALL=C grep -a -q 'STARRYSTAMP1:EPOCH=' "$SO_BUILT" || { echo "ERROR: so 缺构建戳" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_bili_repack.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] 生成 patched AndroidManifest.xml"
python3.11 - "$BASE_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
with zipfile.ZipFile(src) as zin, zipfile.ZipFile(dst, "w") as zout:
    for name in keep:
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
        zout.writestr(info, zin.read(name))
PY
apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
src = Path(sys.argv[1])
t = src.read_text(encoding="utf-8")

perms = []
for p in ("android.permission.SYSTEM_ALERT_WINDOW",
          "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"):
    if p not in t:
        perms.append(f'    <uses-permission android:name="{p}"/>')
if perms:
    anchor = t.find("<application")
    if anchor < 0:
        raise SystemExit("ERROR: <application> missing in decoded Manifest")
    t = t[:anchor] + "\n".join(perms) + "\n" + t[anchor:]

# 壳 Application → StarryApp（父类 LBApplication，见菜单 dex 构建）
OLD = 'android:name="com.netease.nis.wrapper.MyApplication"'
NEW = 'android:name="com.starry.magic.StarryApp"'
if NEW not in t:
    if OLD in t:
        t = t.replace(OLD, NEW, 1)
    else:
        t = t.replace("<application ", "<application " + NEW + " ", 1)
elif OLD in t:
    raise SystemExit("ERROR: manifest 同时含 MyApplication 与 StarryApp")

# ★ 4399 同构：**不动** meta-data LEBIAN_APPLICATION（保持 com.excelliance.open.LBApplication）。
#   乐变用它决定「谁接管 Application」，改了会让乐变的 overlay 链路判断错乱；
#   4399 的包就是「manifest android:name=StarryApp + 父类 LBApplication + meta 原样」。
#   早期初始化不依赖 Application 生命周期，改由 StarryInitProvider（ContentProvider）承担。
if 'android:name="LEBIAN_APPLICATION"' in t and \
        'android:value="com.excelliance.open.LBApplication"' not in t:
    raise SystemExit("ERROR: LEBIAN_APPLICATION meta 被改动（必须保持原值）")

PROVIDER = """
        <provider
            android:name="com.starry.magic.boot.StarryInitProvider"
            android:authorities="com.yhhy.whrhx.bilibili.starryinit"
            android:exported="false"
            android:initOrder="100" />
"""
if "com.starry.magic.boot.StarryInitProvider" not in t:
    t = t.replace("</application>", PROVIDER + "\n    </application>", 1)

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

if NEW not in t or "MenuService" not in t or "StarryInitProvider" not in t:
    raise SystemExit("ERROR: manifest patch incomplete")
src.write_text(t, encoding="utf-8")
print("  Manifest -> StarryApp(LBApplication) + MenuService + StarryInitProvider（meta 原样）")
PY
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
echo "  patched AndroidManifest.xml $(stat -f %z "$WORK/AndroidManifest.xml")B"

echo "[2/6] 外科 zip 合并（保留官方 resources/assets/so，替换真实 DEX + 注入菜单 dex/so）"
python3.11 - "$BASE_APK" "$WORK" "$DEX1" "$DEX2" "$DEX3" "$MENU_DEX" "$SO_BUILT" "$WORK/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1]); work = Path(sys.argv[2])
dex1 = Path(sys.argv[3]).read_bytes()
dex2 = Path(sys.argv[4]).read_bytes()
dex3 = Path(sys.argv[5]).read_bytes()
menu = Path(sys.argv[6]).read_bytes()
so = Path(sys.argv[7]).read_bytes()
dst = Path(sys.argv[8])
manifest = (work / "AndroidManifest.xml").read_bytes()

extras = {
    "classes.dex": dex1,
    "classes2.dex": dex2,
    "classes3.dex": dex3,
    "classes4.dex": menu,
    "lib/arm64-v8a/libstarry_magic.so": so,
}

with zipfile.ZipFile(src, "r") as zin, zipfile.ZipFile(dst, "w") as zout:
    n = 0
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/"):
            continue
        if name in extras:
            continue
        data = manifest if name == "AndroidManifest.xml" else zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
        n += 1
    for name, data in sorted(extras.items()):
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
        n += 1

with zipfile.ZipFile(src, "r") as a, zipfile.ZipFile(dst, "r") as b:
    for name in ("resources.arsc", "assets/bin/Data/Managed/Metadata/global-metadata.dat",
                 "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
                 "assets/lbvmrt.jar", "assets/lbsdk.jar"):
        if name in a.namelist() and name in b.namelist():
            assert hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest(), name
    for name, want in (("classes.dex", dex1), ("classes2.dex", dex2), ("classes3.dex", dex3),
                       ("classes4.dex", menu), ("lib/arm64-v8a/libstarry_magic.so", so)):
        assert name in b.namelist(), name
        assert hashlib.sha256(b.read(name)).hexdigest() == hashlib.sha256(want).hexdigest(), name
    print(f"  merged entries≈{n}; 官方大件逐字节一致；5 个注入条目逐字节一致")
PY

echo "[3/6] zipalign"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"

echo "[4/6] 签名"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"

echo "[5/6] 签名与产物复核"
"$BT/apksigner" verify --verbose --print-certs "$SIGNED" | sed -n '1,12p'

echo "[6/6] 摘要"
ls -lh "$SIGNED"
echo "sha256 $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "md5    $(md5 -q "$SIGNED")"
echo "so     md5=$(md5 -q "$SO_BUILT") size=$(stat -f %z "$SO_BUILT")"
echo "menu   md5=$(md5 -q "$MENU_DEX") size=$(stat -f %z "$MENU_DEX")"
echo "DONE $SIGNED"
