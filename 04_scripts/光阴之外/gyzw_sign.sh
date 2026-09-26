#!/usr/bin/env bash
# ================================================================
#  光阴之外 1.0.3 (com.shbq.gyzw.ppyx) — 管线 sign 阶段后端
#
#  载体：08_output/光阴之外/光阴之外_1.0.3_surgical-unsigned.apk
#        （由 mod菜单/starry-mod-menu/scripts/surgical_gyzw_1.0.3.sh 产出）
#  签名：01_tools/starry_release.jks（CN=StarryMagic），与官方 CN=binqi 不同。
#
#  官包零改动硬校验：底包 02_apks/光阴之外/光阴之外_1.0.3_taptap_original.apk
#  是用户手动放入的不可再生输入；只有 classes3.dex（免广告门控）与
#  AndroidManifest.xml 允许被改写，其余官方条目必须逐字节不变。
#
#  用法: bash 04_scripts/光阴之外/gyzw_sign.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="光阴之外"
VERSION="1.0.3"
PKG="com.shbq.gyzw.ppyx"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${GYZW_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$NIXIANG/mod菜单/starry-mod-menu/libs_gyzw/arm64-v8a/libstarry_magic.so"

# 官包零改动基准（原版 1.0.3，02_apks 内不可再生输入）
EXPECT_COCOS_MD5="${GYZW_COCOS_MD5:-f198a55491ce3ec553043910796c5d93}"
EXPECT_C0_MD5="${GYZW_C0_MD5:-7287635231a9e5e5dc45c6eb345be58e}"
EXPECT_C2_MD5="${GYZW_C2_MD5:-6ba8ba52e0430f8cfeebc9b3bbe446c4}"
EXPECT_C4_MD5="${GYZW_C4_MD5:-6c9ab10baa1e34a7930206813d17a4de}"
EXPECT_ARSC_MD5="${GYZW_ARSC_MD5:-70d00c2a720b10d23ce75cd6190e471c}"
# 本作 TapSDK 走「新版」路径（TapTapLoginActivity + LoginRequest + LoginTypes${WEB}），
# 不走旧版 com.taptap.sdk.ui.TapTapActivity ⇒ 不注入 yntt_smali。
# 登录侧修复载体 = 星光 dex 内的 com.starry.magic.sign.TapLoginForceWeb。
LOGIN_CARRIER="com/starry/magic/sign/TapLoginForceWeb"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 mod菜单/starry-mod-menu/scripts/surgical_gyzw_1.0.3.sh）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing ${SO_BUILT}（先跑 build_so）" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/gyzw_sign.XXXXXX)"
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
for e in "classes.dex" "classes2.dex" "classes3.dex" "classes4.dex" \
         "lib/arm64-v8a/libstarry_magic.so" "lib/arm64-v8a/libc++_shared.so" \
         "lib/arm64-v8a/libcocos2djs.so" "assets/certs/starlight.crt" "resources.arsc"; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done

# dex 编号必须无缺口（ART 遇到第一个缺口就停止加载后续 dex）
# 底包 4 dex ⇒ 星光 dex = classes5.dex
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
[[ "$DEX_NAME" == "classes5.dex" ]] || { echo "ERROR: 星光 dex 应为 classes5.dex，实为 $DEX_NAME" >&2; exit 1; }
echo "[sign] 星光 dex = $DEX_NAME"
unzip -p "$SIGNED" "$DEX_NAME" > "$WORK/starry.dex"
for t in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" \
         "$LOGIN_CARRIER" "com/starry/magic/cocos/GyzwHooks" \
         "com/starry/magic/sign/SignSpoof"; do
  LC_ALL=C grep -aqF "$t" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $t" >&2; exit 1; }
done

# 免广告门控必须落在 classes3.dex
unzip -p "$SIGNED" classes3.dex > "$WORK/c3.dex"
LC_ALL=C grep -aqF "GyzwHooks" "$WORK/c3.dex" \
  || { echo "ERROR: classes3.dex 内无 GyzwHooks 门控（免广告补丁丢失）" >&2; exit 1; }
LC_ALL=C grep -aqF "shouldSkipAd" "$WORK/c3.dex" \
  || { echo "ERROR: classes3.dex 内无 shouldSkipAd 调用" >&2; exit 1; }

# 两张表必须同步（SignSpoof 证书表 + TapLoginForceWeb 白名单）
for t in "SignSpoof" "TapLoginForceWeb"; do
  LC_ALL=C grep -aqF "$t" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $t" >&2; exit 1; }
done

SO_MD5="$(md5 -q "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }
LC_ALL=C grep -aqF "[Gyzw]" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so) \
  || { echo "ERROR: 包内 so 不含 [Gyzw]" >&2; exit 1; }
if LC_ALL=C grep -aqF "[Ysxz]" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so); then
  echo "ERROR: 包内 so 含 [Ysxz]（别家 so 顶包）" >&2; exit 1
fi

# ---- 官包本体零改动硬校验 ----
# ★classes4.dex 自 2026-09-26 第二轮起不再零改动：免广告真入口
#   com/sp/common/SPBaseController.getRewardVideo 就在 classes4，
#   必须改它才拦得住广告（classes3 里根本没有这个类）。
COCOS_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libcocos2djs.so | md5 -q)"
[[ "$COCOS_MD5" == "$EXPECT_COCOS_MD5" ]] \
  || { echo "ERROR: libcocos2djs.so 被改动（${COCOS_MD5}，期望 ${EXPECT_COCOS_MD5}）" >&2; exit 1; }
for pair in "classes.dex:$EXPECT_C0_MD5" "classes2.dex:$EXPECT_C2_MD5" \
            "resources.arsc:$EXPECT_ARSC_MD5"; do
  name="${pair%%:*}"; want="${pair#*:}"
  [[ -n "$want" ]] || continue
  cur_md5="$(unzip -p "$SIGNED" "$name" | md5 -q)"
  [[ "$cur_md5" == "$want" ]] || { echo "ERROR: $name 被改动（${cur_md5}，期望 ${want}）" >&2; exit 1; }
done

# classes4.dex 必须"被改过"（防串包：漏了这层免广告就失效）
C4_MD5="$(unzip -p "$SIGNED" classes4.dex | md5 -q)"
[[ "$C4_MD5" != "$EXPECT_C4_MD5" ]] \
  || { echo "ERROR: classes4.dex 未被改写（免广告门控缺失）" >&2; exit 1; }
unzip -p "$SIGNED" classes4.dex > "$WORK/c4chk.dex"
LC_ALL=C grep -qa "shouldSkipAd" "$WORK/c4chk.dex" \
  || { echo "ERROR: classes4.dex 缺 SPBaseController 免广告门控" >&2; exit 1; }
echo "[sign] classes4.dex 已改写且含免广告门控（md5=${C4_MD5}）"

echo "[sign] done → $SIGNED"
echo "[sign] pkg = $PKG"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
ls -lh "$SIGNED"
