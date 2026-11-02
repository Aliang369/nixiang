#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.02.01 — 签名（Windows/MSYS 后端）
#
#  zipalign 4 → apksigner（01_tools/starry_release.jks，CN=StarryMagic）
#  失败回落 uber-apk-signer.jar。
#
#  硬校验（与 mac 侧 blfx_sign.sh 逐条一致）：
#    · 包内 libil2cpp.so / global-metadata.dat md5 == 官方原版（零改动）
#    · 包内 libstarry_magic.so == build_so 产物
#    · dex 编号无缺口（ART 遇缺口停扫）
#    · 星光 dex 含 StarryInitProvider / MenuService / TapLoginForceWeb
#
#  用法：bash 04_scripts/爆裂防线/blfx_sign_win.sh
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=blfx_win_common.sh
. "$HERE/blfx_win_common.sh"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${BLFX_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

EXPECT_IL2CPP_MD5="${BLFX_IL2CPP_MD5:-30563544a2a74351cef4378cae66cfe2}"
EXPECT_META_MD5="${BLFX_META_MD5:-1712cdbaa5a019f46e4797ca70654e9d}"
LOGIN_CARRIER="com/starry/magic/sign/TapLoginForceWeb"

ZIPALIGN="${NIXIANG_ZIPALIGN:-$BUILD_TOOLS/zipalign.exe}"
[ -f "$ZIPALIGN" ] || ZIPALIGN="$BUILD_TOOLS/zipalign"
APKSIGNER="${NIXIANG_APKSIGNER:-$BUILD_TOOLS/apksigner.bat}"
[ -f "$APKSIGNER" ] || APKSIGNER="$BUILD_TOOLS/apksigner"
JAVA_BIN="$JAVA_HOME_J17/bin/java.exe"
[ -f "$JAVA_BIN" ] || JAVA_BIN="$JAVA_HOME_J17/bin/java"
PY="${NIXIANG_PYTHON:-python}"

[ -f "$UNSIGNED" ] || { echo "ERROR: missing ${UNSIGNED}（先跑 blfx_surgical_win.sh）" >&2; exit 1; }
[ -f "$KS" ]       || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[ -f "$SO_BUILT" ] || { echo "ERROR: missing $SO_BUILT" >&2; exit 1; }
[ -f "$ZIPALIGN" ] || { echo "ERROR: missing zipalign（${ZIPALIGN}）" >&2; exit 1; }
[ -f "$APKSIGNER" ]|| { echo "ERROR: missing apksigner（${APKSIGNER}）" >&2; exit 1; }

WORK="$(mktemp -d /tmp/blfx_sign_win.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

echo "[sign] zipalign -f -p 4"
"$ZIPALIGN" -f -p 4 "$(winpath "$UNSIGNED")" "$(winpath "$ALIGNED")"

rm -f "$SIGNED"
SIGN_RC=0
"$APKSIGNER" sign \
  --ks "$(winpath "$KS")" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$(winpath "$SIGNED")" "$(winpath "$ALIGNED")" || SIGN_RC=$?

if [ "$SIGN_RC" -ne 0 ] || [ ! -f "$SIGNED" ]; then
  echo "[sign] apksigner 失败（rc=${SIGN_RC}），回落 uber-apk-signer"
  [ -f "$SIGNER" ] || { echo "ERROR: missing $SIGNER" >&2; exit 1; }
  "$JAVA_BIN" -jar "$(winpath "$SIGNER")" -a "$(winpath "$ALIGNED")" --out "$(winpath "$WORK/signed")" --allowResign \
    --ks "$(winpath "$KS")" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [ -n "${CAND:-}" ] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[ -f "$SIGNED" ] || { echo "ERROR: sign failed" >&2; exit 1; }
"$APKSIGNER" verify "$(winpath "$SIGNED")" >/dev/null \
  || { echo "ERROR: apksigner verify failed" >&2; exit 1; }

unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q " classes.dex$" "$WORK/entries.txt" || { echo "ERROR: classes.dex missing" >&2; exit 1; }
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }
grep -q "lib/arm64-v8a/libc++_shared.so" "$WORK/entries.txt" \
  || { echo "ERROR: libc++_shared.so missing（STL 运行时没进包会 UnsatisfiedLinkError）" >&2; exit 1; }

DEX_NAME="$("$PY" - "$(winpath "$SIGNED")" <<'PY'
import sys, zipfile
# 星光 dex = 含 StarryInitProvider 的那个（不能取最大下标：classes4.dex 可能是签名杀手）
with zipfile.ZipFile(sys.argv[1]) as z:
    names = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
    used = set()
    for n in names:
        d = n[len('classes'):-len('.dex')]
        used.add(int(d) if d else 1)
    top = max(used) if used else 0
    missing = [str(j) for j in range(2, top + 1) if j not in used]
    if missing:
        raise SystemExit('ERROR: dex 编号缺口 classes{%s}.dex' % ','.join(missing))
    hit = [n for n in names if b'com/starry/magic/StarryInitProvider' in z.read(n)]
    if not hit:
        raise SystemExit('ERROR: 包内无任何 dex 含 StarryInitProvider')
    print(hit[0])
PY
)"
[ -n "$DEX_NAME" ] || { echo "ERROR: 无法确定星光 dex 名" >&2; exit 1; }
echo "[sign] 星光 dex = $DEX_NAME"
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/starry.dex"
for s in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" "$LOGIN_CARRIER"; do
  LC_ALL=C grep -aqF "$s" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $s" >&2; exit 1; }
done

# 签名杀手 dex（若注入）：必须含 KillerApplication193，且包名/证书已改
if grep -q " classes4.dex$" "$WORK/entries.txt"; then
  unzip -p "$SIGNED" classes4.dex > "$WORK/killer.dex"
  LC_ALL=C grep -aqF "bin/mt/signature/KillerApplication193" "$WORK/killer.dex" \
    || { echo "ERROR: classes4.dex 缺 KillerApplication193" >&2; exit 1; }
  LC_ALL=C grep -aqF "$PKG" "$WORK/killer.dex" \
    || { echo "ERROR: classes4.dex 的 killer 包名常量不是 ${PKG}" >&2; exit 1; }
  echo "[sign] 签名杀手 dex OK（${PKG}）"
fi

SO_MD5="$(md5f "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5sum | awk '{print $1}')"
[ "$SO_MD5" = "$IN_SO_MD5" ] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

IL2CPP_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libil2cpp.so | md5sum | awk '{print $1}')"
[ "$IL2CPP_MD5" = "$EXPECT_IL2CPP_MD5" ] \
  || { echo "ERROR: libil2cpp.so 被改动（${IL2CPP_MD5}，期望 ${EXPECT_IL2CPP_MD5}）" >&2; exit 1; }
META_MD5="$(unzip -p "$SIGNED" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5sum | awk '{print $1}')"
[ "$META_MD5" = "$EXPECT_META_MD5" ] \
  || { echo "ERROR: global-metadata.dat 被改动（${META_MD5}，期望 ${EXPECT_META_MD5}）" >&2; exit 1; }

echo "[sign] done → $SIGNED"
echo "[sign] pkg    = $PKG"
echo "[sign] md5    = $(md5f "$SIGNED")"
echo "[sign] sha256 = $(sha256f "$SIGNED")"
echo "[sign] size   = $(filesize "$SIGNED") B"
