#!/usr/bin/env bash
# 暗影格斗3 国际服 1.46.1（com.nekki.shadowfight3 / Google Play AAB 双文件）
# — Split → 单 APK 合并阶段（不改游戏内容，只做包装形态转换）
#
# 背景：真机直拉得到两个文件
#   sf3g_1.46.1_base_from_device.apk            (base，内含 0 个 .so)
#   sf3g_1.46.1_split_arm64_v8a.apk             (ABI split，lib/arm64-v8a/*.so)
# base Manifest 带 android:requiredSplitTypes="base__abi"（只装 base 会被 pm 拒），
# 本脚本把 split 的全部 .so 搬进 base 并摘掉 split 约束，产出可用普通 adb install 直装的单包。
#
# 不变式：官方 .so 字节不变；dex/assets 全原样；只替换 AndroidManifest.xml 并删除官方 META-INF 签名。
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
# shellcheck source=../_common/env.sh
. "$NIXIANG/04_scripts/_common/env.sh" >/dev/null 2>&1 || true

GAME="暗影格斗3国际"
VERSION="1.46.1"
PKG="com.nekki.shadowfight3"
TAG="sf3g"
SRC="$NIXIANG/02_apks/$GAME"
BASE="${SF3G_BASE_APK:-$SRC/sf3g_1.46.1_base_from_device.apk}"
SPLIT="${SF3G_SPLIT_APK:-$SRC/sf3g_1.46.1_split_arm64_v8a.apk}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${SF3G_UNSIGNED:-$OUT/$GAME-$VERSION-merged-unsigned.apk}"
SIGNED="${SF3G_SIGNED:-$OUT/$GAME-$VERSION-merged-releaseSigned.apk}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

PY="$(command -v python3 || true)"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
AAPT2="${NIXIANG_AAPT2:-}"
BT="${NIXIANG_BUILD_TOOLS:-}"
if [ -z "$AAPT2" ] && [ -n "$BT" ] && [ -f "$BT/aapt2" ]; then AAPT2="$BT/aapt2"; fi
if command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

for f in "$BASE" "$SPLIT" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f" >&2; exit 1; }
done
[ -n "$PY" ] && [ -n "$ZIPALIGN" ] && [ -n "$APKSIGNER" ] && [ -n "$AAPT2" ] \
  || { echo "ERROR: 工具缺失 PY=$PY ZIPALIGN=$ZIPALIGN APKSIGNER=$APKSIGNER AAPT2=$AAPT2" >&2; exit 1; }
command -v apktool >/dev/null 2>&1 || { echo "ERROR: apktool 不在 PATH" >&2; exit 1; }

mkdir -p "$OUT"
WORK="$(mktemp -d /tmp/sf3g_merge.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/5] apktool d 官方 base（只取文本 Manifest）"
apktool d -f -s --no-assets -o "$WORK/dec" "$BASE" >/dev/null

echo "[2/5] 摘掉 Play split-type 约束 + vending.splits 标记"
"$PY" - "$WORK/dec/AndroidManifest.xml" <<'PYEOF'
import re, sys
from pathlib import Path
p = Path(sys.argv[1]); t = p.read_text(encoding="utf-8"); removed = []
t2 = re.sub(r'\s+android:requiredSplitTypes="[^"]*"', '', t)
removed.append(("requiredSplitTypes", t2 != t)); t = t2
t2 = re.sub(r'\s+android:splitTypes="[^"]*"', '', t)
removed.append(("splitTypes", t2 != t)); t = t2
pat = re.compile(r'\n\s*<meta-data\b(?:(?!</?meta-data)[\s\S])*?android:name="com\.android\.vending\.splits(?:\.required)?"[\s\S]*?(?:/>|</meta-data>)')
t2, n = pat.subn('', t); removed.append(("vending.splits meta-data x%d" % n, n > 0)); t = t2
p.write_text(t, encoding="utf-8")
print("  patched:", removed)
assert "requiredSplitTypes" not in t and "android:splitTypes=" not in t and "com.android.vending.splits" not in t
PYEOF

echo "[3/5] apktool b → 取回 binary AndroidManifest.xml"
rm -rf "$WORK/dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/dec" -o "$WORK/mf.apk" >/dev/null
"$PY" -c 'import sys,zipfile;open(sys.argv[2],"wb").write(zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml"))' \
  "$WORK/mf.apk" "$WORK/AndroidManifest.xml"

echo "[4/5] zip 外科合并 → $UNSIGNED"
"$PY" - "$BASE" "$SPLIT" "$WORK/AndroidManifest.xml" "$UNSIGNED" <<'PYEOF'
import sys, zipfile
from pathlib import Path
base, split, mf, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), Path(sys.argv[4])
new_mf = mf.read_bytes()
if dst.exists(): dst.unlink()
n_base = n_lib = n_skip = 0
with zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED) as out:
    with zipfile.ZipFile(base) as zb:
        for it in zb.infolist():
            name = it.filename
            ext = name.split("/")[-1].split(".")[-1].upper()
            if name.startswith("META-INF/") and ext in ("RSA", "SF", "MF", "DSA", "EC"):
                n_skip += 1; continue
            if name == "AndroidManifest.xml":
                out.writestr(name, new_mf); n_base += 1; continue
            zi = zipfile.ZipInfo(name, date_time=it.date_time)
            zi.compress_type = it.compress_type
            zi.external_attr = it.external_attr
            out.writestr(zi, zb.read(name)); n_base += 1
    with zipfile.ZipFile(split) as zs:
        for it in zs.infolist():
            name = it.filename
            if not name.startswith("lib/"): continue
            zi = zipfile.ZipInfo(name, date_time=it.date_time)
            zi.compress_type = zipfile.ZIP_DEFLATED
            zi.external_attr = it.external_attr
            out.writestr(zi, zs.read(name)); n_lib += 1
print("  base entries=%d  split lib entries=%d  dropped signature entries=%d" % (n_base, n_lib, n_skip))
PYEOF

echo "[5/5] 校验 + 签名"
LIBMERGED=$("$PY" -c 'import sys,zipfile;print(sum(1 for n in zipfile.ZipFile(sys.argv[1]).namelist() if n.startswith("lib/arm64-v8a/")))' "$UNSIGNED")
LIBSPLIT=$("$PY" -c 'import sys,zipfile;print(sum(1 for n in zipfile.ZipFile(sys.argv[1]).namelist() if n.startswith("lib/arm64-v8a/")))' "$SPLIT")
echo "  lib/arm64-v8a 条目: merged=$LIBMERGED split=$LIBSPLIT"
[ "$LIBMERGED" = "$LIBSPLIT" ] || { echo "ERROR: so 条目数不一致" >&2; exit 1; }
BADGING="$("$AAPT2" dump badging "$UNSIGNED" 2>/dev/null | head -1 || true)"
case "$BADGING" in
  *"package: name='"$PKG"'"*) echo "  badging ok: $BADGING" ;;
  *) echo "ERROR: 合并包包名不对: $BADGING" >&2; exit 1 ;;
esac

"$ZIPALIGN" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
rm -f "$SIGNED"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$WORK/aligned.apk"
"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,4p'
echo "  => $SIGNED"
ls -l "$SIGNED"
