#!/usr/bin/env bash
# 无悔华夏 3.8.305 九游（com.yhhy.whrhx.aligames）— 无壳外科重建 + 星光注入。
#
# 背景（2026-11-05 实锤，与 B 站 3.8.280 同族）：
#   - 原版 APK 是网易易盾 NIS 壳（libnesec.so 1,051,712B，classes.dex 仅 58,140B 壳 stub）⇒ 重签后壳不解密，
#     外科换 dex 死路，只能无壳重建。
#   - 运行时 so = 乐变负载 lebian/<pkg>-1/lib/arm64/libil2cpp.so 204,509,528B
#     sha256 af1e80fe9431d209ccdccbcbaec9ccbc677eed877bc3c0ada5f20852b517ed73（== 本轮 RVA 重标基准）。
#   - 真实业务 DEX 由壳以 InMemoryDexFile 提供，已用 whrhx_mem_dex.c 从 /proc/<pid>/mem 精确导出 13 份。
#
# 输入：底包 = 乐变重建的 base.odex.apk（含运行时明文 so + 热更后资源），不是 base.apk（内置 so 是加密的）
# 输出：08_output/无悔华夏/无悔华夏_3.8.305_无壳星光_<date>-releaseSigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
NIXIANG="$SCRIPT_DIR"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
DATE_TAG="${WHRHX305_DATE:-$(date +%Y%m%d)}"
RUN_DIR="${WHRHX305_RUN:-$NIXIANG/06_dumped/无悔华夏/current_20261105}"
BASE_APK="${WHRHX305_BASE:-$RUN_DIR/pkg/base.odex_305.apk}"
DEXDIR="${WHRHX305_DEXDIR:-$RUN_DIR/realdex/whrhx_dex_out}"
MENU_DEX="${WHRHX305_MENU_DEX:-$NIXIANG/07_patched/无悔华夏/menu_latest_$DATE_TAG/classes14.dex}"
SO_BUILT="${WHRHX305_SO:-$STARRY_SRC/libs_whrhx/arm64-v8a/libstarry_magic.so}"
CXX_SO="${WHRHX305_CXX:-$STARRY_SRC/libs_whrhx/arm64-v8a/libc++_shared.so}"
OUT="$NIXIANG/08_output/无悔华夏"
SIGNED="$OUT/无悔华夏_3.8.305_无壳星光_${DATE_TAG}-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"

# 13 份业务 DEX，按「主 dex 优先」定序（全部来自同一次 InMemoryDexFile 加载，重复类按先定义者胜）
DEX_ORDER=(
  mem_6fd0761ffc_6934884.dex
  mem_6fd4fdb000_4343864.dex
  mem_6fd6051ffc_1108844.dex
  mem_6f39e7b000_6155884.dex
  mem_6fc5334000_833108.dex
  mem_6fc5609000_1099536.dex
  mem_6fc57b4000_881528.dex
  mem_7085271000_1632804.dex
  mem_714d2aa000_259568.dex
  mem_7160897000_65180.dex
  mem_71609b0000_112856.dex
  mem_7162b43000_126620.dex
  mem_7169c00000_74408.dex
)

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

for f in "$BASE_APK" "$MENU_DEX" "$SO_BUILT" "$CXX_SO" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing input: $f" >&2; exit 1; }
done
for d in "${DEX_ORDER[@]}"; do
  [[ -f "$DEXDIR/$d" ]] || { echo "ERROR: missing dex: $DEXDIR/$d" >&2; exit 1; }
done
command -v apktool >/dev/null || { echo "ERROR: apktool missing" >&2; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found" >&2; exit 1; }

LC_ALL=C grep -a -q 'Whrhx' "$SO_BUILT" || { echo "ERROR: so 缺 Whrhx 标签（不是无悔华夏的 so）" >&2; exit 1; }
LC_ALL=C grep -a -q 'STARRYSTAMP1:EPOCH=' "$SO_BUILT" || { echo "ERROR: so 缺构建戳" >&2; exit 1; }
LC_ALL=C grep -a -q 'StarryApp' "$MENU_DEX" || { echo "ERROR: 菜单 dex 缺 StarryApp" >&2; exit 1; }
# 3.8.305 原包**不再自带** libc++_shared.so（3.8.280 自带）；mod so 的 DT_NEEDED 含它且引用 __emutls_get_address，
# 缺它 ⇒ dlopen: library "libc++_shared.so" not found（2026-11-05 实测）。必须随包注入并导出 emutls。
LC_ALL=C grep -a -q '__emutls_get_address' "$CXX_SO" || { echo "ERROR: libc++_shared.so 不导出 __emutls_get_address（旧版，会加载失败）" >&2; exit 1; }
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
            android:authorities="com.yhhy.whrhx.aligames.starryinit"
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
python3.11 - "$BASE_APK" "$WORK" "$DEXDIR" "$MENU_DEX" "$SO_BUILT" "$WORK/unsigned.apk" "$CXX_SO" "${DEX_ORDER[@]}" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src = Path(sys.argv[1]); work = Path(sys.argv[2]); dexdir = Path(sys.argv[3])
menu = Path(sys.argv[4]).read_bytes(); so = Path(sys.argv[5]).read_bytes()
dst = Path(sys.argv[6]); cxx = Path(sys.argv[7]).read_bytes(); order = sys.argv[8:]
manifest = (work / "AndroidManifest.xml").read_bytes()

extras = {}
for i, name in enumerate(order):
    dn = "classes.dex" if i == 0 else "classes%d.dex" % (i + 1)
    extras[dn] = (dexdir / name).read_bytes()
extras["classes%d.dex" % (len(order) + 1)] = menu
extras["lib/arm64-v8a/libstarry_magic.so"] = so
extras["lib/arm64-v8a/libc++_shared.so"] = cxx

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
                 "assets/lbvmrt.jar", "assets/lbsdk.jar"):
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
echo "cxx    md5=$(md5 -q "$CXX_SO") size=$(stat -f %z "$CXX_SO")"
echo "DONE $SIGNED"
