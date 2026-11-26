#!/usr/bin/env bash
# 明日方舟 - 签名阶段（pipeline sign）【版本无关，版本取自 official_apk.json】
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
# ---- 跨平台工具链与命令垫片（2026-11-27 Windows/Git Bash 适配）----
[ -f "$NIXIANG/04_scripts/_common/env.sh" ] && source "$NIXIANG/04_scripts/_common/env.sh"
if ! command -v md5 >/dev/null 2>&1; then
  md5() { case "${1:-}" in -q) shift; md5sum "$@" | awk '{print $1}' ;; *) md5sum "$@" ;; esac; }
fi
sha256_of() {
  if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'
  else sha256sum "$1" | awk '{print $1}'; fi
}
_size_bytes() { wc -c < "$1" | tr -d ' '; }
GAME="明日方舟"
PKG="com.hypergryph.arknights"
META_FILE="$SCRIPT_DIR/official_apk.json"
[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，请先跑 fetch_official 阶段" >&2; exit 1; }
VERSION="${MRFZZ_VERSION:-$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["version_name"])' "$META_FILE")}"
ORIG_APK="${MRFZZ_BASE_APK:-$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META_FILE")}"
ORIG_MD5="${MRFZZ_BASE_MD5:-$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["md5"])' "$META_FILE")}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
echo "[meta] version=$VERSION base_md5=$ORIG_MD5"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILD="$NIXIANG/mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so"

# JDK17：env.sh 已按主机解析（Win 走 JAVA_HOME/winget 目录/PATH，mac 走 java_home）
if [ -z "${JAVA_HOME:-}" ] || [ ! -x "$JAVA_HOME/bin/java" ]; then
  JAVA_HOME="${NIXIANG_JAVA17:-}"
fi
[ -n "${JAVA_HOME:-}" ] || { echo "ERROR: 找不到 JDK17（env.sh 的 NIXIANG_JAVA17 为空）" >&2; exit 1; }
export JAVA_HOME
export PATH="$JAVA_HOME/bin:$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED (run surgical first)" >&2; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILD" ]] || { echo "ERROR: missing built so $SO_BUILD" >&2; exit 1; }
ACTUAL_ORIG_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_ORIG_MD5" == "$ORIG_MD5" ]] \
  || { echo "ERROR: official apk md5 mismatch: $ACTUAL_ORIG_MD5" >&2; exit 1; }

# build-tools：Win 上是 zipalign.exe / apksigner.bat，bash 的 [ -x ] 不做扩展名补全，
# 所以一律先 [ -f ] 逐个解析真实文件名（照 env.sh「Win 上 -x 恒为假」的实测结论）
BT=""
for c in "${MRFZZ_BUILD_TOOLS:-}" \
         "$NIXIANG/01_tools/android-sdk/build-tools/android-15" \
         "${NIXIANG_BUILD_TOOLS:-}" \
         "${ANDROID_SDK_ROOT:-}"/build-tools/* "${ANDROID_HOME:-}"/build-tools/* ; do
  [ -n "$c" ] && [ -d "$c" ] || continue
  { [ -f "$c/zipalign" ] || [ -f "$c/zipalign.exe" ]; } || continue
  BT="$c"; break
done
ZA=""; AS=""
if [ -n "$BT" ]; then
  ZA="$BT/zipalign"; [ -f "$ZA" ] || ZA="$BT/zipalign.exe"
  AS="$BT/apksigner"; [ -f "$AS" ] || AS="$BT/apksigner.bat"; [ -f "$AS" ] || AS="$BT/apksigner.exe"
fi
echo "[tool] build-tools=${BT:-未解析} zipalign=${ZA:-无} apksigner=${AS:-无}"

WORK="$(mktemp -d /tmp/mrfzz_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$ZA" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
  "$AS" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$AS" verify --verbose --print-certs "$SIGNED" 2>&1 | sed -n '1,14p'
  # apksigner 会顺带写 v4 的 .idsig（apksigner 0.9 默认行为）；交付目录只留 apk
  rm -f "$SIGNED.idsig"
fi

if [[ ! -f "$SIGNED" ]]; then
  [[ -n "$BT" ]] || cp "$UNSIGNED" "$ALIGNED"
  java -jar "$SIGNER" -a "$ALIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }
if [[ -n "$AS" && -f "$AS" ]]; then
  "$AS" verify "$SIGNED" >/dev/null \
    || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for n in classes.dex classes2.dex classes3.dex classes4.dex classes5.dex \
         classes6.dex resources.arsc; do
  grep -q " $n$" "$WORK/entries.txt" || { echo "ERROR: $n missing" >&2; exit 1; }
done
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }

SO_MD5="$(md5 -q "$SO_BUILD")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

unzip -p "$SIGNED" classes6.dex > "$WORK/classes6.dex"

python3 - "$ORIG_APK" "$SIGNED" <<'PY'
import hashlib
import sys
import zipfile

orig, signed = sys.argv[1], sys.argv[2]
GUARD = [
    "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex",
    "resources.arsc", "lib/arm64-v8a/libil2cpp.so",
    "lib/arm64-v8a/libunity.so", "lib/arm64-v8a/libtapsdkcore.so",
    "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libthemis.so",
    "lib/arm64-v8a/libtprt.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
]
with zipfile.ZipFile(orig) as a, zipfile.ZipFile(signed) as b:
    for name in GUARD:
        ha = hashlib.sha256(a.read(name)).hexdigest()
        hb = hashlib.sha256(b.read(name)).hexdigest()
        if ha != hb:
            raise SystemExit(f"protected entry changed: {name}")
        print(f"  {name} sha256={hb[:16]}... ok")
print("  official protected entries unchanged")
PY

if [[ -n "$AS" && -f "$AS" ]]; then
  "$AS" verify --print-certs "$SIGNED" 2>&1 \
    | grep -q "CN=StarryMagic" \
    || { echo "ERROR: unexpected signer certificate" >&2; exit 1; }
fi

echo "[sign] done -> $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(sha256_of "$SIGNED")"
ls -lh "$SIGNED"
