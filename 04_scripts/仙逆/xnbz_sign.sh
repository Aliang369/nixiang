#!/usr/bin/env bash
# ================================================================
#  仙逆：本尊 0.0.14 (com.qqsy.xnbz) — 管线 sign 阶段后端
#
#  载体：08_output/仙逆/仙逆_0.0.14_surgical-unsigned.apk
#        （由 mod菜单/starry-mod-menu/scripts/surgical_xnbz_0.0.14.sh 产出）
#  签名：01_tools/starry_release.jks（CN=StarryMagic），与官方 CN=qqian 不同。
#
#  官包零改动硬校验：底包 = 设备只读拉取的官方 TapTap 原版 0.0.14
#  （375,615,779 B md5 756df4d27ded491e00a865687571fc53）；
#  只有 classes7.dex（TapSDK 登录/去签名补丁）与 AndroidManifest.xml 允许被改写，
#  其余官方条目必须逐字节不变。
#
#  用法: bash 04_scripts/仙逆/xnbz_sign.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="仙逆"
VERSION="0.0.14"
PKG="com.qqsy.xnbz"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${XNBZ_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$NIXIANG/mod菜单/starry-mod-menu/libs_xnbz/arm64-v8a/libstarry_magic.so"

# 官包零改动基准（官方 0.0.14 原版条目 md5）
EXPECT_COCOS_MD5="${XNBZ_COCOS_MD5:-7ad5451d988a57b52e189d4fdc359e83}"
EXPECT_ARSC_MD5="${XNBZ_ARSC_MD5:-614d4d7e58ad92bb2a5756f28b6997cb}"
ORIG_C7_MD5="${XNBZ_C7_MD5:-72625d77928cf78d6abe51cee789c71d}"   # 原版 classes7.dex（必须被改掉）
ORIG_SIGN_MD5="82c51c4ac68ac5d3197ef4c2b19e1ef6"                 # 原厂证书 MD5（补丁写入的常量）
# 除 classes7.dex 外，全部官方 dex 必须原字节
EXPECT_UNTOUCHED=(
  "classes.dex:3ab6fb4b3a5513417cd4aaf2ab0699d0"
  "classes2.dex:f37a0461e9e31a0f1fa6e91aa9666580"
  "classes3.dex:b2ffb12c7b0ecba6371e5fb6abc816e5"
  "classes4.dex:050cf5b5380a87120d156b65c91bdfc8"
  "classes5.dex:5565ce6a6e209c852c5b9e318aea5f6c"
  "classes6.dex:67fe06f0fe441a8cb891ee154389c306"
  "classes8.dex:681267553e65bcbe455db615a3e3b4a9"
  "classes9.dex:168bbbc5d7c4d1929a1853cdc7c7ff5d"
  "classes10.dex:67c64ea790c8dad7dab10055982029e2"
)
# 登录侧修复载体 = 星光 dex 内的 com.starry.magic.sign.TapLoginForceWeb（新版 TapSDK 路径）
LOGIN_CARRIER="com/starry/magic/sign/TapLoginForceWeb"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 mod菜单/starry-mod-menu/scripts/surgical_xnbz_0.0.14.sh）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing ${SO_BUILT}（先跑 build_so）" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/xnbz_sign.XXXXXX)"
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
for e in "classes.dex" "classes7.dex" "classes10.dex" "classes11.dex" \
         "lib/arm64-v8a/libstarry_magic.so" "lib/arm64-v8a/libc++_shared.so" \
         "lib/arm64-v8a/libcocos.so" "assets/certs/starlight.crt" "resources.arsc"; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done

# dex 编号必须无缺口（ART 遇到第一个缺口就停止加载后续 dex）
# 底包 10 dex（classes.dex + classes2..10）⇒ 星光 dex = classes11.dex
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
[[ "$DEX_NAME" == "classes11.dex" ]] || { echo "ERROR: 星光 dex 应为 classes11.dex，实为 $DEX_NAME" >&2; exit 1; }
echo "[sign] 星光 dex = $DEX_NAME"
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/starry.dex"
for t in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" \
         "com/starry/magic/bridge/ModBridge" "com/starry/magic/sign/SignSpoof" \
         "$LOGIN_CARRIER"; do
  LC_ALL=C grep -aqF "$t" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $t" >&2; exit 1; }
done
# 两张表必须同步（证书表 + 白名单），否则 TapTap SSO 报 signature not match
LC_ALL=C grep -aqF "com.qqsy.xnbz" "$WORK/starry.dex" \
  || { echo "ERROR: $DEX_NAME 内两张表未含 com.qqsy.xnbz" >&2; exit 1; }

# ---- classes7.dex 必须"被改过"且带原厂证书常量（去签名 + 强制网页登录补丁）----
C7_MD5="$(unzip -p "$SIGNED" classes7.dex | md5 -q)"
[[ "$C7_MD5" != "$ORIG_C7_MD5" ]] \
  || { echo "ERROR: classes7.dex 未被改写（TapSDK 补丁缺失）" >&2; exit 1; }
unzip -p "$SIGNED" classes7.dex > "$WORK/c7.dex"
LC_ALL=C grep -aqF "$ORIG_SIGN_MD5" "$WORK/c7.dex" \
  || { echo "ERROR: classes7.dex 内无原厂证书 MD5 常量（TapSignKit.getSign 补丁缺失）" >&2; exit 1; }
LC_ALL=C grep -aqF "com/taptap/sdk/login/impl/utils/ClientLoginUtils" "$WORK/c7.dex" \
  || { echo "ERROR: classes7.dex 内无 ClientLoginUtils" >&2; exit 1; }
echo "[sign] classes7.dex 已改写（md5=${C7_MD5}，含原厂证书 MD5 常量）"

SO_MD5="$(md5 -q "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }
LC_ALL=C grep -aqF "[Xnbz]" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so) \
  || { echo "ERROR: 包内 so 不含 [Xnbz]" >&2; exit 1; }
for t in "[Gyzw]" "[Jfqs]" "[Ysxz]"; do
  if LC_ALL=C grep -aqF "$t" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so); then
    echo "ERROR: 包内 so 含别家标签 ${t}（so 顶包）" >&2; exit 1
  fi
done

# ---- 官包本体零改动硬校验（除 classes7.dex / AndroidManifest.xml）----
COCOS_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libcocos.so | md5 -q)"
[[ "$COCOS_MD5" == "$EXPECT_COCOS_MD5" ]] \
  || { echo "ERROR: libcocos.so 被改动（${COCOS_MD5}，期望 ${EXPECT_COCOS_MD5}）" >&2; exit 1; }
for pair in "${EXPECT_UNTOUCHED[@]}" "resources.arsc:$EXPECT_ARSC_MD5"; do
  name="${pair%%:*}"; want="${pair#*:}"
  cur_md5="$(unzip -p "$SIGNED" "$name" | md5 -q)"
  [[ "$cur_md5" == "$want" ]] || { echo "ERROR: $name 被改动（${cur_md5}，期望 ${want}）" >&2; exit 1; }
done

echo "[sign] done → $SIGNED"
echo "[sign] pkg = $PKG"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
