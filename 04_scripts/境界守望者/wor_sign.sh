#!/usr/bin/env bash
# 境界守望者 1.6.38.829.1（com.ap.watcherofrealms / 国际服）— 星光包签名阶段
# （starry_release.jks，与其它星光包同签；运行期由 SignSpoof 回灌官证 WOR_PKG）
#
# 母本：04_scripts/潮汐守望者/cxswz_sign.sh
# 差异（勿照抄母本）：
#   1) 本作是 Qigsaw Split 包 ⇒ base 与 split_config.arm64_v8a **必须用同一张证书**，
#      否则 `pm install-multiple` 报 INSTALL_FAILED_INVALID_APK / Inconsistent certificates。
#      所以本脚本一次签两份：starry-releaseSigned.apk + starry-splitSigned.apk。
#   2) build-tools 在 Windows 上是 zipalign.exe / apksigner.bat。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=wor_env.sh
[[ -f "$SCRIPT_DIR/wor_env.sh" ]] && . "$SCRIPT_DIR/wor_env.sh"
WUSER="${USER:-${USERNAME:-Administrator}}"
GAME="境界守望者"
VERSION="1.6.38.829.1"
PKG="com.ap.watcherofrealms"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${WOR_UNSIGNED:-$OUT/${GAME}_${VERSION}_starry-unsigned.apk}"
SPLIT_UNSIGNED="${WOR_SPLIT_UNSIGNED:-$OUT/${GAME}_${VERSION}_starry-split-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk"
SPLIT_SIGNED="$OUT/${GAME}_${VERSION}_starry-splitSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
NEW_DEX="${WOR_NEW_DEX:-classes6.dex}"   # 官方占用 classes.dex..classes5.dex ⇒ 必须 6（比最大号 +1）

PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3 python py; do
    if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
  done
fi
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python（设 PYTHON=/path/to/python3）" >&2; exit 1; }

md5_of() { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }
sha256_of() { if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi; }
zipcat() { "$PY" -c 'import sys,zipfile;sys.stdout.buffer.write(zipfile.ZipFile(sys.argv[1]).read(sys.argv[2]))' "$1" "$2"; }

if command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 surgical_wor_1.6.38.829.1.sh）" >&2; exit 1; }
[[ -f "$SPLIT_UNSIGNED" ]] || { echo "ERROR: missing ${SPLIT_UNSIGNED}（split 不重签会 Inconsistent certificates）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
mkdir -p "$OUT"

# build-tools 探测：mac / linux / Windows Git Bash（Windows 是 zipalign.exe / apksigner.bat）
#
# ⚠️ Windows 上 apksigner.bat 的文件位是 -rw-r--r--（**没有 x 位**），
#    所以判可用性必须用 -f，不能用 -x —— 用了 -x 会误报 "apksigner/zipalign missing"。
#    实测 Git Bash 直接调用 apksigner.bat 是可以的（内部走 cmd.exe，rc=0）。
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
ZIPALIGN=""
for c in "$BT/zipalign" "$BT/zipalign.exe"; do [[ -f "$c" ]] && { ZIPALIGN="$c"; break; }; done
APKSIGNER=""
for c in "$BT/apksigner" "$BT/apksigner.bat" "$BT/apksigner.exe"; do [[ -f "$c" ]] && { APKSIGNER="$c"; break; }; done
[[ -n "$BT" && -n "$ZIPALIGN" && -n "$APKSIGNER" ]] \
  || { echo "ERROR: apksigner/zipalign missing（BT='$BT'；设 BUILD_TOOLS=/path/to/build-tools/XX）" >&2; exit 1; }
APKSIGNER="$(cygpath -m "$APKSIGNER" 2>/dev/null || echo "$APKSIGNER")"
echo "[sign] build-tools: $BT"
echo "[sign] zipalign  : $ZIPALIGN"
"$APKSIGNER" --version | sed 's/^/[sign] apksigner /'

WORK="$(mktemp -d /tmp/wor_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

# 所有 apksigner 调用都只碰 $WORK 里的 ASCII 路径：
# apksigner.bat → cmd → java 在 Windows 上按 ANSI 代码页解 argv，非 ASCII 路径会解坏。
A_BASE="$WORK/base-signed.apk"
A_SPLIT="$WORK/split-signed.apk"

sign_one() {  # $1=unsigned  $2=输出(ASCII)  $3=label
  local in="$1" out="$2" label="$3"
  rm -f "$out" "$WORK/${label}-aligned.apk" "$WORK/${label}-unsigned.apk"
  # 输入也先搬进 ASCII 的 ${WORK}：zipalign.exe / apksigner.bat 都不保证能吃非 ASCII 路径
  cp -f "$in" "$WORK/${label}-unsigned.apk"
  "$ZIPALIGN" -f -p 4 "$WORK/${label}-unsigned.apk" "$WORK/${label}-aligned.apk"
  "$APKSIGNER" sign \
    --ks "$(cygpath -m "$KS" 2>/dev/null || echo "$KS")" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$out" "$WORK/${label}-aligned.apk"
  "$APKSIGNER" verify "$out" >/dev/null
  echo "[sign] $label 已签（ASCII 暂存 ${out}）"
}

sign_one "$UNSIGNED" "$A_BASE" "base"
sign_one "$SPLIT_UNSIGNED" "$A_SPLIT" "split"

# ---- 未签输入校验（断言「没有」签名残留；签名由下面 sign_one 产生）----
"$PY" - "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, re
apk, newdex = sys.argv[1], sys.argv[2]
names = zipfile.ZipFile(apk).namelist()
dexes = sorted(n for n in names if re.fullmatch(r"classes\d*\.dex", n))
assert newdex in names, "ERROR: %s missing（本作必须 %s；官方占 classes.dex..classes5.dex）" % (newdex, newdex)
assert "lib/arm64-v8a/libstarry_magic.so" in names, "ERROR: starry so missing"
# 这份是 **未签** 输入，必须无任何签名残留（surgical 已剥）
sigs = [n for n in names if n.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))]
assert not sigs, "ERROR: unsigned base 仍有签名残留 %s" % sigs
print("  unsigned base ok: dex=%s; so 在; 无签名残留; entries=%d" % (dexes, len(names)))
PY

# ---- 未签 split 校验（.so 一个不少、无签名残留、无 stamp）----
"$PY" - "$SPLIT_UNSIGNED" <<'PY'
import sys, zipfile
apk = sys.argv[1]
names = zipfile.ZipFile(apk).namelist()
sos = sorted(n for n in names if n.endswith(".so"))
assert "lib/arm64-v8a/libil2cpp.so" in sos, "ERROR: split 缺 libil2cpp.so"
assert "AndroidManifest.xml" in names, "ERROR: split 缺 AndroidManifest.xml"
assert "stamp-cert-sha256" not in names, "ERROR: split 仍有 Play 来源戳"
sigs = [n for n in names if n.startswith("META-INF/") and n.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))]
assert not sigs, "ERROR: unsigned split 签名残留 %s" % sigs
print("  unsigned split ok: entries=%d; so=%d; 无 stamp/无签名残留" % (len(names), len(sos)))
PY

# ---- 已签输出校验（断言「有」我们自己的签名；上面两条查的是未签输入）----
"$PY" - "$A_BASE" "$A_SPLIT" <<'PY'
import sys, zipfile
for label, apk in (("base", sys.argv[1]), ("split", sys.argv[2])):
    names = zipfile.ZipFile(apk).namelist()
    sigs = sorted(n for n in names if n.startswith("META-INF/") and n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")))
    assert sigs, "ERROR: %s 已签输出里没有签名块（sign_one 没生效？）" % label
    print("  已签 %-5s ✓ 签名块 %s" % (label, sigs))
PY

# ---- 两份必须同一张证书，否则 install-multiple 必然失败 ----
CERT_BASE="$("$APKSIGNER" verify --print-certs "$A_BASE" 2>/dev/null | grep -i 'certificate SHA-256 digest' | head -1)"
CERT_SPLIT="$("$APKSIGNER" verify --print-certs "$A_SPLIT" 2>/dev/null | grep -i 'certificate SHA-256 digest' | head -1)"
[[ -n "$CERT_BASE" ]] || { echo "ERROR: 读不出 base 证书" >&2; exit 1; }
[[ "$CERT_BASE" == "$CERT_SPLIT" ]] \
  || { echo "ERROR: base/split 证书不一致，install-multiple 会失败" >&2; echo "  base : $CERT_BASE" >&2; echo "  split: $CERT_SPLIT" >&2; exit 1; }
echo "[sign] base/split 同证书 ✓  $CERT_BASE"

# ---- so 内容闭环（防国服 so 混包）----
SIGNED_TAG_N="$(zipcat "$A_BASE" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "[Wor]" || true)"
[[ "$SIGNED_TAG_N" -ge 1 ]] || { echo "ERROR: signed apk starry so missing [Wor] tag" >&2; exit 1; }
SO_CN_N="$(zipcat "$A_BASE" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "[Cxswz]" || true)"
[[ "$SO_CN_N" == "0" ]] || { echo "ERROR: 包内 so 含 [Cxswz]（国服 so 混入）" >&2; exit 1; }
SIGNED_MENU_N="$(zipcat "$A_BASE" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF -- "Slider_伤害倍率" || true)"
[[ "$SIGNED_MENU_N" -ge 1 ]] || { echo "ERROR: signed apk starry so missing menu strings" >&2; exit 1; }

cp -f "$A_BASE" "$SIGNED"
cp -f "$A_SPLIT" "$SPLIT_SIGNED"
echo "[sign] done → $SIGNED"
echo "[sign] md5=$(md5_of "$SIGNED")"
echo "[sign] sha256=$(sha256_of "$SIGNED")"
echo "[sign] split done → $SPLIT_SIGNED"
echo "[sign] split md5=$(md5_of "$SPLIT_SIGNED")"
"$APKSIGNER" verify --print-certs "$A_BASE" 2>/dev/null | grep -E 'certificate (DN|SHA-256|MD5)' | head -3 || true
ls -lh "$SIGNED" "$SPLIT_SIGNED"
echo "[sign] NOTE: 包是 starry_release.jks 签名（非官签）；运行期由 SignSpoof 回灌 WOR_PKG 官证 $PKG"
