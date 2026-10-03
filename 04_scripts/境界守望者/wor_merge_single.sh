#!/usr/bin/env bash
# 境界守望者 1.6.38.829.1（com.ap.watcherofrealms / 国际服）
# — 单 APK 合并阶段（Split → 无 root 直装单包）
#
# 背景（为什么需要这一步）：
#   本作官包是 Google Play App Bundle 分发形态，官方**只有两个文件**：
#     境界守望者_1.6.38.829.1_global_original.apk              (base，内含 0 个 .so)
#     境界守望者_1.6.38.829.1_global_split_config.arm64_v8a.apk (ABI split，35 个 .so)
#   base 的 AndroidManifest.xml 上带 `android:requiredSplitTypes="base__abi"`，
#   split 的 Manifest 上带 `android:splitTypes="base__abi"` —— 这是 Play 的
#   split-type 机制：**只装 base 会被 pm 拒（INSTALL_FAILED_MISSING_SPLIT）**，
#   这就是 e1/e2 阶段必须走 `pm install-multiple` 的原因。
#
#   本脚本把 35 个 .so 搬进 base 并摘掉 split-type 约束，产出**一个自包含单 APK**，
#   可用普通 `adb install`（无需 root、无需 install-multiple）直装。
#
# 不变式（与 surgical / sign 阶段一致，勿破坏）：
#   - 原 Application 类 com.moba.unityplugin.MobaGameApplication 保留不替换（走 StarryInitProvider）
#   - StarryInitProvider(authority=$PKG.starryinit) / MenuService / SYSTEM_ALERT_WINDOW 等保留
#   - classes.dex..classes5.dex 官方原样 + classes6.dex 星光
#   - lib/arm64-v8a/libstarry_magic.so 星光 so（[Wor] 有、[Cxswz] 无）
#   - 35 个官方 .so 字节不变（sha256 逐个比对 split）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=wor_env.sh
[[ -f "$SCRIPT_DIR/wor_env.sh" ]] && . "$SCRIPT_DIR/wor_env.sh"
WUSER="${USER:-${USERNAME:-Administrator}}"
GAME="境界守望者"
VERSION="1.6.38.829.1"
PKG="com.ap.watcherofrealms"
AUTHORITY="$PKG.starryinit"

SRC="$NIXIANG/02_apks/$GAME"
ORIG_APK="${WOR_ORIG_APK:-$SRC/${GAME}_${VERSION}_global_original.apk}"
SPLIT_APK="${WOR_SPLIT_APK:-$SRC/${GAME}_${VERSION}_global_split_config.arm64_v8a.apk}"
OUT="$NIXIANG/08_output/$GAME"
SIGNED_BASE="${WOR_SIGNED_BASE:-$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk}"
SIGNED_SPLIT="${WOR_SIGNED_SPLIT:-$OUT/${GAME}_${VERSION}_starry-splitSigned.apk}"
SINGLE_UNSIGNED="${WOR_SINGLE_UNSIGNED:-$OUT/${GAME}_${VERSION}_starry-single-unsigned.apk}"
SINGLE_SIGNED="${WOR_SINGLE_SIGNED:-$OUT/${GAME}_${VERSION}_starry-single.apk}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
NEW_DEX="${WOR_NEW_DEX:-classes6.dex}"

PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3 python py; do
    if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
  done
fi
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python（设 PYTHON=/path/to/python3）" >&2; exit 1; }

md5_of() { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }
sha256_of() { if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi; }

if command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

command -v apktool >/dev/null 2>&1 \
  || { echo "ERROR: apktool 不在 PATH（01_tools/apktool 应由 wor_env.sh 加进 PATH）" >&2; exit 1; }
for f in "$ORIG_APK" "$SPLIT_APK" "$SIGNED_BASE" "$SIGNED_SPLIT" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f" >&2; exit 1; }
done

# build-tools（Windows 是 zipalign.exe / apksigner.bat；判可用性必须用 -f，.bat 没有 x 位）
BT="${BUILD_TOOLS:-${WOR_BUILD_TOOLS:-}}"
if [[ -z "$BT" ]]; then
  for cand in "$HOME/Library/Android/sdk/build-tools"/* \
              "$HOME/Android/Sdk/build-tools"/* \
              "/c/Users/$WUSER/AppData/Local/Android/Sdk/build-tools"/* \
              "/mnt/c/Users/$WUSER/AppData/Local/Android/Sdk/build-tools"/* \
              "${ANDROID_HOME:-}/build-tools"/* "${ANDROID_SDK_ROOT:-}/build-tools"/*; do
    if [[ -f "$cand/zipalign" || -f "$cand/zipalign.exe" ]]; then
      [[ -f "$cand/apksigner" || -f "$cand/apksigner.bat" ]] && BT="$cand"
    fi
  done
fi
ZIPALIGN=""; for c in "$BT/zipalign" "$BT/zipalign.exe"; do [[ -f "$c" ]] && { ZIPALIGN="$c"; break; }; done
APKSIGNER=""; for c in "$BT/apksigner" "$BT/apksigner.bat" "$BT/apksigner.exe"; do [[ -f "$c" ]] && { APKSIGNER="$c"; break; }; done
AAPT2=""; for c in "$BT/aapt2" "$BT/aapt2.exe"; do [[ -f "$c" ]] && { AAPT2="$c"; break; }; done
[[ -n "$BT" && -n "$ZIPALIGN" && -n "$APKSIGNER" && -n "$AAPT2" ]] \
  || { echo "ERROR: apksigner/zipalign/aapt2 missing（BT='$BT'；设 BUILD_TOOLS=/path/to/build-tools/XX）" >&2; exit 1; }
APKSIGNER="$(cygpath -m "$APKSIGNER" 2>/dev/null || echo "$APKSIGNER")"
echo "[single] build-tools: $BT"

WORK="$(mktemp -d /tmp/wor_single.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
# apksigner.bat → cmd → java 在 Windows 上按 ANSI 代码页解 argv，非 ASCII 路径会解坏 ⇒ 一律走 ASCII 暂存
A_OUT="$WORK/single-signed.apk"

echo "[1/6] apktool d 官方 base（只取文本 Manifest，不碰 dex/assets）"
apktool d -f -s --no-assets -o "$WORK/dec" "$ORIG_APK" >/dev/null

echo "[2/6] 文本 Manifest 打补丁：注入星光组件 + 摘掉 split-type 约束"
"$PY" - "$WORK/dec/AndroidManifest.xml" "$PKG" <<'PY'
import re, sys
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
orig = t

# ---- (a) 权限（与 surgical 阶段一致）----
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    assert i >= 0, "no <uses-permission> in manifest"
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# ---- (b) 星光组件（Application 不替换，走 StarryInitProvider）----
if "com.starry.magic.StarryInitProvider" not in t:
    t = t.replace("</application>", '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    t = t.replace("</application>", '''
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
''' + "\n    </application>", 1)

# ---- (c) ★ 单 APK 关键：摘掉 Play split-type 约束 ----
# <manifest ... android:requiredSplitTypes="base__abi" android:splitTypes="" ...>
removed = []
if "requiredSplitTypes" in t:
    t2 = re.sub(r'\s+android:requiredSplitTypes="[^"]*"', '', t)
    removed.append(("requiredSplitTypes", t2 != t)); t = t2
if re.search(r'\s+android:splitTypes="[^"]*"', t):
    t2 = re.sub(r'\s+android:splitTypes="[^"]*"', '', t)
    removed.append(("splitTypes", t2 != t)); t = t2

# <meta-data android:name="com.android.vending.splits[.required]" .../>（Play Core splitcompat 标记）
# 单包没有 split，留着会让 SplitCompat 去找不存在的 split APK
pat = re.compile(r'\n\s*<meta-data\b(?:(?!</?meta-data)[\s\S])*?android:name="com\.android\.vending\.splits(?:\.required)?"[\s\S]*?(?:/>|</meta-data>)')
t2, nmeta = pat.subn('', t)
removed.append(("vending.splits meta-data", nmeta)); t = t2

p.write_text(t, encoding="utf-8")
print("  text Manifest patched: 注入星光组件; 摘除 %s" % (removed,))
assert "requiredSplitTypes" not in t, "requiredSplitTypes 仍残留"
assert not re.search(r'android:splitTypes=', t), "splitTypes 仍残留"
assert "com.android.vending.splits" not in t, "vending.splits meta-data 仍残留"
PY

echo "[3/6] apktool b → 取回 binary AndroidManifest.xml"
rm -rf "$WORK/dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/dec" -o "$WORK/mf_only.apk" >/dev/null
"$PY" -c 'import sys,zipfile;open(sys.argv[2],"wb").write(zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml"))' \
  "$WORK/mf_only.apk" "$WORK/AndroidManifest.xml"
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验"
"$PY" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w"); i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED; z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
"$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
  || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
check() { grep -q "$2" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $1" >&2; exit 1; }; }
check "StarryInitProvider"            "com.starry.magic.StarryInitProvider"
check "MenuService"                   "com.starry.magic.menu.MenuService"
check "provider authority"            "$AUTHORITY"
check "FOREGROUND_SERVICE_SPECIAL_USE" "FOREGROUND_SERVICE_SPECIAL_USE"
check "原 Application 类"             "com.moba.unityplugin.MobaGameApplication"
grep -q "requiredSplitTypes" "$WORK/tree_new.txt"  && { echo "ERROR: requiredSplitTypes 仍在 binary manifest" >&2; exit 1; }
grep -q "com.android.vending.splits" "$WORK/tree_new.txt" && { echo "ERROR: vending.splits meta-data 仍在" >&2; exit 1; }
echo "   manifest ok（无 requiredSplitTypes / 无 vending.splits；星光组件与原 Application 均在）"

echo "[5/6] zip 外科合并 → $SINGLE_UNSIGNED"
"$PY" - "$SIGNED_BASE" "$SIGNED_SPLIT" "$WORK" "$SINGLE_UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

base_p, split_p, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4]), sys.argv[5]
mf = (work / "AndroidManifest.xml").read_bytes()

# split 里的全部 lib/**（35 个官方 .so），compress_type 沿用 split 原件
zs = zipfile.ZipFile(split_p, "r")
libs = {}
for it in zs.infolist():
    if it.filename.startswith("lib/") and not it.is_dir():
        libs[it.filename] = (zs.read(it), it.compress_type)
assert libs, "split 里没有 lib/** —— split 路径给错了？"
for n in libs:
    assert n.startswith("lib/arm64-v8a/"), "非 arm64-v8a 条目: %s" % n

SIG_SKIP = lambda name: (
    name == "stamp-cert-sha256"
    or name == "META-INF/MANIFEST.MF"
    or (name.startswith("META-INF/") and name.upper().endswith((".SF", ".RSA", ".DSA", ".EC")))
)

zin = zipfile.ZipFile(base_p, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names, "signed base 缺 AndroidManifest.xml"
assert new_dex in names, "signed base 缺 %s" % new_dex
assert "lib/arm64-v8a/libstarry_magic.so" in names, "signed base 缺星光 so"

zout = zipfile.ZipFile(dst, "w")
written = set()
for it in zin.infolist():
    name = it.filename
    if SIG_SKIP(name):
        continue
    if name in libs:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(it)
    info = zipfile.ZipInfo(name, it.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = it.compress_type
    info.external_attr = it.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    written.add(name)

# 加入 split 的 35 个 .so（base 原本一个 lib/ 都没有）
for name in sorted(libs):
    data, ctype = libs[name]
    info = zipfile.ZipInfo(name)
    info.compress_type = ctype
    info.external_attr = 0o100644 << 16
    info.create_system = 3
    zout.writestr(info, data)
    written.add(name)

zout.close()
print("  合并完成: base 条目 %d + split lib %d → 输出 %d 条" % (len(names), len(libs), len(written)))
PY

echo "[6/6] 自证 + zipalign + 签名"
"$PY" - "$SIGNED_SPLIT" "$SINGLE_UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
split_p, out_p, new_dex = sys.argv[1], sys.argv[2], sys.argv[3]
zs = zipfile.ZipFile(split_p); zo = zipfile.ZipFile(out_p)
zsos = {i.filename: hashlib.sha256(zs.read(i)).hexdigest() for i in zs.infolist() if i.filename.startswith("lib/")}
osos = {i.filename: hashlib.sha256(zo.read(i)).hexdigest() for i in zo.infolist() if i.filename.startswith("lib/")}
bad = [k for k in zsos if k != "lib/arm64-v8a/libstarry_magic.so" and zsos[k] != osos.get(k)]
assert not bad, "官方 .so 字节变了: %s" % bad
assert "lib/arm64-v8a/libstarry_magic.so" in osos, "缺星光 so"
names = set(zo.namelist())
assert new_dex in names, "缺 %s" % new_dex
assert "stamp-cert-sha256" not in names, "仍带 Play 来源戳"
sigs = [n for n in names if n.startswith("META-INF/") and n.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))]
assert not sigs, "未签输入仍有签名残留 %s" % sigs
assert "lib/arm64-v8a/libil2cpp.so" in names, "缺 libil2cpp.so（转发壳）"
print("  自证 ✓ 官方 .so %d 个字节不变（sha256 逐个比对）；星光 so 在；%s 在；无签名残留" % (len(zsos), new_dex))
PY
SIG_SO_N="$("$PY" -c 'import sys,zipfile;sys.stdout.buffer.write(zipfile.ZipFile(sys.argv[1]).read("lib/arm64-v8a/libstarry_magic.so"))' "$SINGLE_UNSIGNED" | LC_ALL=C grep -acF -- "[Wor]" || true)"
[[ "$SIG_SO_N" -ge 1 ]] || { echo "ERROR: 星光 so 缺 [Wor] 标记" >&2; exit 1; }
CN_SO_N="$("$PY" -c 'import sys,zipfile;sys.stdout.buffer.write(zipfile.ZipFile(sys.argv[1]).read("lib/arm64-v8a/libstarry_magic.so"))' "$SINGLE_UNSIGNED" | LC_ALL=C grep -acF -- "[Cxswz]" || true)"
[[ "$CN_SO_N" == "0" ]] || { echo "ERROR: 星光 so 含 [Cxswz]（国服 so 混入）" >&2; exit 1; }

"$ZIPALIGN" -f -p 4 "$SINGLE_UNSIGNED" "$WORK/aligned.apk"
"$APKSIGNER" sign \
  --ks "$(cygpath -m "$KS" 2>/dev/null || echo "$KS")" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$A_OUT" "$WORK/aligned.apk"
"$APKSIGNER" verify "$A_OUT" >/dev/null
cp -f "$A_OUT" "$SINGLE_SIGNED"

echo
echo "[single] done → $SINGLE_SIGNED"
echo "[single] size   = $(stat -c '%s' "$SINGLE_SIGNED" 2>/dev/null || stat -f '%z' "$SINGLE_SIGNED") B"
echo "[single] md5    = $(md5_of "$SINGLE_SIGNED")"
echo "[single] sha256 = $(sha256_of "$SINGLE_SIGNED")"
"$APKSIGNER" verify --print-certs "$A_OUT" 2>/dev/null | grep -E 'certificate (DN|SHA-256)' | head -2 || true
echo "[single] NOTE: 单 APK（无 split）⇒ 可 adb install 直装；split-type 约束已摘"
