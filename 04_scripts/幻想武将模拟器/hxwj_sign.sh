#!/usr/bin/env bash
# ================================================================
#  幻想武将模拟器 1.0.8 (com.qxjc.hxwjmlq) — 管线 sign 阶段后端
#
#  载体：08_output/幻想武将模拟器/幻想武将模拟器_1.0.8_surgical-unsigned.apk
#        （由 mod菜单/starry-mod-menu/scripts/surgical_hxwj_1.0.8.sh 产出）
#  签名：01_tools/starry_release.jks（CN=StarryMagic），与官方 CN=sy 不同。
#
#  官包零改动硬校验：底包 02_apks/幻想武将模拟器/幻想武将模拟器_1.0.8_taptap_original.apk
#  是用户从真机拉取的不可再生输入；只有 AndroidManifest.xml 允许被改写，
#  classes.dex / resources.arsc / lib/arm64-v8a/libil2cpp.so 必须逐字节不变。
#
#  用法: bash 04_scripts/幻想武将模拟器/hxwj_sign.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="幻想武将模拟器"
VERSION="1.0.8"
PKG="com.qxjc.hxwjmlq"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${HXWJ_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$NIXIANG/mod菜单/starry-mod-menu/libs_hxwj/arm64-v8a/libstarry_magic.so"

# 官包零改动基准（原版 1.0.8，02_apks 内不可再生输入）
EXPECT_CLASSES_MD5="${HXWJ_C0_MD5:-390a9958866b76c7c497f35bc9a979a9}"
EXPECT_ARSC_MD5="${HXWJ_ARSC_MD5:-985fbb5dde4ff61df449241b3eb11aae}"
EXPECT_IL2CPP_SHA256="${HXWJ_IL2CPP_SHA256:-9bd65a76bb74045247e576780de36d4a66873cd5a2a6ecd42fcc9eab645b535c}"

# 登录侧修复载体 = 星光 dex 内 com.starry.magic.sign.TapLoginForceWeb + SignSpoof
LOGIN_CARRIER="com/starry/magic/sign/TapLoginForceWeb"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 scripts/surgical_hxwj_1.0.8.sh）" >&2; exit 1; }
[[ -f "$KS" ]]       || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing ${SO_BUILT}（先跑 build_so）" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/hxwj_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED" "$SIGNED.idsig"
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
  "$BT/apksigner" verify "$SIGNED" >/dev/null 2>&1 || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for e in "classes.dex" "classes2.dex" "lib/arm64-v8a/libstarry_magic.so" \
         "lib/arm64-v8a/libc++_shared.so" "lib/arm64-v8a/libil2cpp.so" \
         "assets/certs/starlight.crt" "resources.arsc"; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done

# dex 编号必须无缺口（ART 遇到第一个缺口就停止加载后续 dex）
# 底包 1 dex（classes.dex）⇒ 星光 dex = classes2.dex
DEX_NAME="$(python3.11 - "$WORK/entries.txt" <<'PY'
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
[[ "$DEX_NAME" == "classes2.dex" ]] || { echo "ERROR: 星光 dex 应为 classes2.dex，实为 $DEX_NAME" >&2; exit 1; }
echo "[sign] 星光 dex = $DEX_NAME"
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/starry.dex"
for t in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" \
         "$LOGIN_CARRIER" "com/starry/magic/sign/SignSpoof"; do
  LC_ALL=C grep -aqF "$t" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $t" >&2; exit 1; }
done

# 两张表必须同步（SignSpoof 证书表 + TapLoginForceWeb 白名单）
for f in SignSpoof TapLoginForceWeb; do
  LC_ALL=C grep -aqF "$PKG" "$WORK/starry.dex" \
    || { echo "ERROR: $DEX_NAME 缺 $f 的 $PKG 白名单/证书表" >&2; exit 1; }
done

SO_MD5="$(md5 -q "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }
LC_ALL=C grep -aqF -- '[hxwj]' <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so) \
  || { echo "ERROR: 包内 so 不含 [hxwj]" >&2; exit 1; }
if LC_ALL=C grep -aqF -- '[Gyzw]' <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so); then
  echo "ERROR: 包内 so 含 [Gyzw]（别家 so 顶包）" >&2; exit 1
fi

# ---- 官包本体零改动硬校验 ----
CLS_MD5="$(unzip -p "$SIGNED" classes.dex | md5 -q)"
[[ "$CLS_MD5" == "$EXPECT_CLASSES_MD5" ]] \
  || { echo "ERROR: classes.dex 被改动（${CLS_MD5}，期望 ${EXPECT_CLASSES_MD5}）" >&2; exit 1; }
ARSC_MD5="$(unzip -p "$SIGNED" resources.arsc | md5 -q)"
[[ "$ARSC_MD5" == "$EXPECT_ARSC_MD5" ]] \
  || { echo "ERROR: resources.arsc 被改动（${ARSC_MD5}，期望 ${EXPECT_ARSC_MD5}）" >&2; exit 1; }
IL2CPP_SHA="$(unzip -p "$SIGNED" lib/arm64-v8a/libil2cpp.so | shasum -a 256 | awk '{print $1}')"
[[ "$IL2CPP_SHA" == "$EXPECT_IL2CPP_SHA256" ]] \
  || { echo "ERROR: libil2cpp.so 被改动（${IL2CPP_SHA}，期望 ${EXPECT_IL2CPP_SHA256}）" >&2; exit 1; }
echo "[sign] classes.dex / resources.arsc / libil2cpp.so 均零改动"

echo "[sign] done → $SIGNED"
echo "[sign] pkg = $PKG"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
