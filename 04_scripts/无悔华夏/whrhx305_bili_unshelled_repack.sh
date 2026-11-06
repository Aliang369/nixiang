#!/usr/bin/env bash
# 无悔华夏 B 站渠道 3.8.305（com.yhhy.whrhx.bilibili）— 无壳外科重建 + 星光注入。
#
# 渠道差异（2026-11-06 实测，与同版本九游/4399 对照）：
#   - 运行时 libil2cpp.so 204,509,528B sha256 af1e80fe… 与九游 3.8.305 **字节一致** ⇒ RVA 零重标；
#   - 业务 DEX 只有 **6 份**（九游 13 份、4399 6 份），ver 035，主 dex = 5161604（TPFApplication+GameActivity）；
#   - 本包**自带** libc++_shared.so 911,696B（九游 3.8.305 不带）⇒ 不注入 NDK 版，
#     mod so 由 -fno-emulated-tls 编译，UND 里只缺 __cxa_atexit/__cxa_finalize（libc.so 提供）。
#
# 背景（2026-11-05 实锤，与 B 站 3.8.280 同族）：
#   - 原版 APK 是网易易盾 NIS 壳（libnesec.so 1,051,712B，classes.dex 仅 58,788B 壳 stub）⇒ 重签后壳不解密，
#     外科换 dex 死路，只能无壳重建。
#   - 运行时 so = 乐变负载 lebian/<pkg>-1/lib/arm64/libil2cpp.so 204,509,528B
#     sha256 af1e80fe9431d209ccdccbcbaec9ccbc677eed877bc3c0ada5f20852b517ed73（== 本轮 RVA 重标基准）。
#   - 真实业务 DEX 由壳以 InMemoryDexFile 提供，已用 whrhx_mem_dex.c 从 /proc/<pid>/mem 精确导出 6 份（ver 035，039 的全是系统 framework，不注入）。
#
# 输入：底包 = 乐变重建的 base.odex.apk（含运行时明文 so + 热更后资源），不是 base.apk（内置 so 是加密的）
# 输出：08_output/无悔华夏bili/无悔华夏bili_3.8.305_无壳星光_<date>-releaseSigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
NIXIANG="$SCRIPT_DIR"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
DATE_TAG="${WHRHXBILI_DATE:-$(date +%Y%m%d)}"
RUN_DIR="${WHRHXBILI_RUN:-$NIXIANG/06_dumped/无悔华夏bili/current_20261106}"
BASE_APK="${WHRHXBILI_BASE:-$RUN_DIR/pkg/base.odex_305.apk}"
DEXDIR="${WHRHXBILI_DEXDIR:-$RUN_DIR/realdex/whrhx_dex_out}"
MENU_DEX="${WHRHXBILI_MENU_DEX:-$NIXIANG/07_patched/无悔华夏bili/menu_305_$DATE_TAG/classes7.dex}"
SO_BUILT="${WHRHXBILI_SO:-$STARRY_SRC/libs_whrhxbili/arm64-v8a/libstarry_magic.so}"
OUT="$NIXIANG/08_output/无悔华夏bili"
SIGNED="$OUT/无悔华夏bili_3.8.305_无壳星光_${DATE_TAG}-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"

# 6 份业务 DEX，按「主 dex 优先」定序（同一次 InMemoryDexFile 加载，重复类按先定义者胜）
DEX_ORDER=(
  mem_78a1712ffc_5161604.dex
  mem_789f3b8ffc_8679388.dex
  mem_78a444f000_4343864.dex
  mem_78a75c0ffc_1108844.dex
  mem_7a01b6a000_259568.dex
  mem_795baedc50_9340.dex
)

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

for f in "$BASE_APK" "$MENU_DEX" "$SO_BUILT" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing input: $f" >&2; exit 1; }
done
for d in "${DEX_ORDER[@]}"; do
  [[ -f "$DEXDIR/$d" ]] || { echo "ERROR: missing dex: $DEXDIR/$d" >&2; exit 1; }
done
command -v apktool >/dev/null || { echo "ERROR: apktool missing" >&2; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found" >&2; exit 1; }

LC_ALL=C grep -a -q 'Whrhx' "$SO_BUILT" || { echo "ERROR: so 缺 Whrhx 标签（不是无悔华夏的 so）" >&2; exit 1; }
LC_ALL=C grep -a -q 'whhxbili' "$SO_BUILT" || { echo "ERROR: so 不是 bili 渠道（缺 whhxbili）" >&2; exit 1; }
LC_ALL=C grep -a -q 'STARRYSTAMP1:EPOCH=' "$SO_BUILT" || { echo "ERROR: so 缺构建戳" >&2; exit 1; }
LC_ALL=C grep -a -q 'StarryApp' "$MENU_DEX" || { echo "ERROR: 菜单 dex 缺 StarryApp" >&2; exit 1; }
# 本包自带 libc++_shared.so（911,696B）⇒ 不注入 NDK 版；只断言它在底包里（mod so DT_NEEDED 需要）
unzip -l "$BASE_APK" 'lib/arm64-v8a/libc++_shared.so' >/dev/null 2>&1 \
  || { echo "ERROR: 底包不含 libc++_shared.so（mod so DT_NEEDED 需要它）" >&2; exit 1; }
LC_ALL=C grep -a -q 'Lcom/tpf/sdk/unity/GameActivity;' "$DEXDIR/${DEX_ORDER[0]}" \
  || { echo "ERROR: 第一份 dex 不含 GameActivity（主 dex 顺序错）" >&2; exit 1; }
LC_ALL=C grep -a -q 'Lcom/excelliance/open/LBApplication;' "$DEXDIR/${DEX_ORDER[1]}" \
  || { echo "ERROR: 第二份 dex 不含 LBApplication（乐变 dex 顺序错）" >&2; exit 1; }
LC_ALL=C grep -a -q 'LBApplication' "$MENU_DEX" || { echo "ERROR: 菜单 dex 父类不是 LBApplication" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx305_repack.XXXXXX)"
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
src = Path(sys.argv[1]); t = src.read_text(encoding="utf-8")

perms = []
for p in ("android.permission.SYSTEM_ALERT_WINDOW",
          "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"):
    if p not in t:
        perms.append('    <uses-permission android:name="%s"/>' % p)
if perms:
    anchor = t.find("<application")
    if anchor < 0: raise SystemExit("ERROR: application missing")
    t = t[:anchor] + "\n".join(perms) + "\n" + t[anchor:]

OLD = 'android:name="com.netease.nis.wrapper.MyApplication"'
NEW = 'android:name="com.starry.magic.StarryApp"'
if NEW not in t:
    if OLD in t: t = t.replace(OLD, NEW, 1)
    else: t = t.replace("<application ", "<application " + NEW + " ", 1)
elif OLD in t:
    raise SystemExit("ERROR: manifest 同时含 MyApplication 与 StarryApp")

if 'android:name="LEBIAN_APPLICATION"' in t and 'android:value="com.excelliance.open.LBApplication"' not in t:
    raise SystemExit("ERROR: LEBIAN_APPLICATION meta 被改动")

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
print("  Manifest -> StarryApp(LBApplication) + MenuService + StarryInitProvider（LEBIAN_APPLICATION 原样）")
PY
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
echo "  patched AndroidManifest.xml $(stat -f %z "$WORK/AndroidManifest.xml")B"

echo "[2/6] 外科 zip 合并（保留官方 resources/assets/so，替换 DEX 14 份 + 注入 mod so）"
python3.11 - "$BASE_APK" "$WORK" "$DEXDIR" "$MENU_DEX" "$SO_BUILT" "$WORK/unsigned.apk" "${DEX_ORDER[@]}" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src = Path(sys.argv[1]); work = Path(sys.argv[2]); dexdir = Path(sys.argv[3])
menu = Path(sys.argv[4]).read_bytes(); so = Path(sys.argv[5]).read_bytes()
dst = Path(sys.argv[6]); order = sys.argv[7:]
manifest = (work / "AndroidManifest.xml").read_bytes()

extras = {}
for i, name in enumerate(order):
    dn = "classes.dex" if i == 0 else "classes%d.dex" % (i + 1)
    extras[dn] = (dexdir / name).read_bytes()
extras["classes%d.dex" % (len(order) + 1)] = menu
extras["lib/arm64-v8a/libstarry_magic.so"] = so

with zipfile.ZipFile(src, "r") as zin, zipfile.ZipFile(dst, "w", allowZip64=True) as zout:
    n = 0
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/"): continue
        if name in extras: continue
        data = manifest if name == "AndroidManifest.xml" else zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
        n += 1
    for name, data in sorted(extras.items(), key=lambda kv: (0 if kv[0].startswith("classes") else 1, kv[0])):
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
        n += 1

with zipfile.ZipFile(src, "r") as a, zipfile.ZipFile(dst, "r") as b:
    for name in ("resources.arsc", "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
                 "lib/arm64-v8a/libc++_shared.so", "assets/lbvmrt.jar", "assets/lbsdk.jar"):
        if name in a.namelist() and name in b.namelist():
            assert hashlib.sha256(a.read(name)).hexdigest() == hashlib.sha256(b.read(name)).hexdigest(), name
    for name, want in extras.items():
        assert name in b.namelist(), name
        assert hashlib.sha256(b.read(name)).hexdigest() == hashlib.sha256(want).hexdigest(), name
    got = sorted(x for x in b.namelist() if x.startswith("classes") and x.endswith(".dex"))
    print("  merged entries=%d; dex=%d 份 (%s..%s); 官方大件逐字节一致" % (n, len(got), got[0], got[-1]))
PY

echo "[3/6] zipalign -p 4"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"

echo "[4/6] apksigner 签名"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
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
