#!/usr/bin/env bash
# 圣翼传说 7.5.10 (com.aiyue.wscq) — 签名阶段（pipeline sign）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="圣翼传说"
VERSION="7.5.10"
PKG="com.aiyue.wscq"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${WSCQ_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$NIXIANG/mod菜单/starry-mod-menu/libs_wscq/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED（先跑 surgical_wscq_7.5.10.sh）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/wscq_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
  if [[ -x "$BT/apksigner" ]]; then
    "$BT/apksigner" sign \
      --ks "$KS" --ks-key-alias "$KS_ALIAS" \
      --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
      --out "$SIGNED" "$ALIGNED"
    "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
  fi
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

if [[ -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q " classes.dex$" "$WORK/entries.txt" || { echo "ERROR: classes.dex missing" >&2; exit 1; }
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }
grep -q "lib/arm64-v8a/libc++_shared.so" "$WORK/entries.txt" \
  || { echo "ERROR: libc++_shared.so missing（STL 运行时没进包会 UnsatisfiedLinkError）" >&2; exit 1; }

# dex 编号必须无缺口（ART 遇到第一个缺口就停止加载后续 dex）：
# 找到星光 dex 名，并确认 1..N 全部存在。
DEX_NAME="$(python3 - "$WORK/entries.txt" <<'PY'
import re, sys
names = open(sys.argv[1], encoding='utf-8', errors='replace').read()
used = set()
for n in re.findall(r'classes(\d*)\.dex', names):
    used.add(int(n) if n else 1)
if not used:
    raise SystemExit('ERROR: 包里没有任何 classesN.dex')
top = max(used)
missing = [str(j) for j in range(2, top + 1) if j not in used]
if missing:
    raise SystemExit('ERROR: dex 编号缺口 classes{%s}.dex' % ','.join(missing))
print('classes%d.dex' % top)
PY
)"
[[ -n "$DEX_NAME" ]] || { echo "ERROR: 无法确定星光 dex 名" >&2; exit 1; }
echo "[sign] 星光 dex = $DEX_NAME"
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/starry.dex"
LC_ALL=C grep -aqF "com/starry/magic/StarryInitProvider" "$WORK/starry.dex" \
  || { echo "ERROR: $DEX_NAME 缺 StarryInitProvider" >&2; exit 1; }
LC_ALL=C grep -aqF "com/taptap/sdk/LoginRequest" "$WORK/starry.dex" \
  || { echo "ERROR: $DEX_NAME 缺 YnttLoginRequest" >&2; exit 1; }

SO_MD5="$(md5 -q "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

# 官包游戏本体零改动硬校验（与原版/斌哥包同源：libil2cpp.so md5 完全一致 → 15 个锚点可直接沿用）
IL2CPP_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libil2cpp.so | md5 -q)"
[[ "$IL2CPP_MD5" == "563b40a81370833858ef2d4452f8dc3f" ]] \
  || { echo "ERROR: libil2cpp.so 被改动（$IL2CPP_MD5）" >&2; exit 1; }
META_MD5="$(unzip -p "$SIGNED" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"
[[ "$META_MD5" == "ef84cf230a6f4be6858f13cc47e2ab5b" ]] \
  || { echo "ERROR: global-metadata.dat 被改动（$META_MD5）" >&2; exit 1; }

# YnttLoginRequest 必须在星光 dex 里（TapTap 旧版强制网页登录的唯一载体）—— 已在上面校验

echo "[sign] done → $SIGNED"
echo "[sign] pkg = $PKG"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
