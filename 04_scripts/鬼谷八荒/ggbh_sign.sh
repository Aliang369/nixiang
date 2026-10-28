#!/usr/bin/env bash
# ================================================================
#  鬼谷八荒 1.1.329 (com.guigugame.guigubahuang) — 管线 sign 阶段后端
#  引擎：Unity IL2CPP（libil2cpp.so / libunity.so / libmain.so /
#        assets/bin/Data/Managed/Metadata/global-metadata.dat）
#
#  载体：08_output/鬼谷八荒/鬼谷八荒_1.1.329_surgical-unsigned.apk
#        （由 mod菜单/starry-mod-menu/scripts/surgical_ggbh_1.1.329.sh 产出）
#  产物：08_output/鬼谷八荒/鬼谷八荒_1.1.329_surgical-releaseSigned.apk
#  留证：07_patched/鬼谷八荒/surgical/{AndroidManifest.xml,classes2.dex}
#  签名：01_tools/starry_release.jks（CN=StarryMagic），与官方 O=guigugame 不同。
#
#  底包期望（官方 TapTap 原版 1.1.329）：
#    md5 f6cc1a4eb063bc0505cb93b68020d714 / 1,678,372,111 B
#    默认路径 $NIXIANG/02_apks/鬼谷八荒/鬼谷八荒_1.1.329_taptap_official_from_device.apk
#    （该默认路径当前不存在；本机实测底包在
#      /tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk，
#      首次跑 surgical 用 GGBH_BASE_APK=/tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk
#      指定底包；本脚本不读底包，只按下列官包条目 md5 做零改动硬校验）
#
#  官包零改动硬校验：只有 AndroidManifest.xml（appComponentFactory + provider/service/权限）
#  与新增的 classes2.dex（星光 smali）允许被改写，其余官方条目必须逐字节不变。
#  官包只有 classes.dex 单 dex ⇒ 星光 dex 落位必须是 classes2.dex。
#
#  用法: bash 04_scripts/鬼谷八荒/ggbh_sign.sh
#        GGBH_UNSIGNED=/path/to/other-unsigned.apk bash 04_scripts/鬼谷八荒/ggbh_sign.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="鬼谷八荒"
VERSION="1.1.329"
PKG="com.guigugame.guigubahuang"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${GGBH_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILT="$NIXIANG/mod菜单/starry-mod-menu/libs_ggbh/arm64-v8a/libstarry_magic.so"

# 官包零改动基准（官方 1.1.329 原版条目 md5；含 resources.arsc）
EXPECT_ARSC_MD5="${GGBH_ARSC_MD5:-e93ed597d6286035ba50f161967b162e}"
EXPECT_UNTOUCHED=(
  "classes.dex:e5f75fd4f1c2a67d28ca6021099bd87b"
  "lib/arm64-v8a/libil2cpp.so:188c796e6b53b6e520a327f259ca565f"
  "lib/arm64-v8a/libunity.so:fbaf76f43affdfbf56c0ace0ca0547ed"
  "lib/arm64-v8a/libmain.so:bc0f659e042866301d31156c1f4e7c58"
  "lib/arm64-v8a/lib_burst_generated.so:85ed9acd7a04da106d11874ef978c0ea"
  "lib/arm64-v8a/libbindings-csharp.so:4f7ae38f445b64dd40421e1b5ad33af7"
  "lib/arm64-v8a/libtapsdkcore.so:a4d1dcd35cc9c200e733995082922183"
  "assets/bin/Data/Managed/Metadata/global-metadata.dat:93c74718c98c04ac238ff0eae67d5c42"
)
# 登录侧修复载体（星光 dex 内）：本作 TapSDK 走旧版路径 com.taptap.sdk.ui.TapTapActivity
# ⇒ TapLoginForceWeb 白名单 + yntt_smali 的 YnttLoginRequest 二者缺一不可
# （与 mod菜单/starry-mod-menu/scripts/surgical_ggbh_1.1.329.sh 的必需 smali 清单一致）。
LOGIN_CARRIER="com/starry/magic/sign/TapLoginForceWeb"
LOGIN_CARRIER_YNTT="com/starry/magic/sign/YnttLoginRequest"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 mod菜单/starry-mod-menu/scripts/surgical_ggbh_1.1.329.sh，首次用 GGBH_BASE_APK=/tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk 指定底包）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing ${SO_BUILT}（先跑 04_scripts/鬼谷八荒/ggbh_build_so.sh）" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

WORK="$(mktemp -d /tmp/ggbh_sign.XXXXXX)"
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
for e in "classes.dex" "classes2.dex" \
         "lib/arm64-v8a/libstarry_magic.so" "lib/arm64-v8a/libc++_shared.so" \
         "lib/arm64-v8a/libil2cpp.so" "resources.arsc"; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done

# dex 编号必须无缺口（ART 遇到第一个缺口就停止加载后续 dex）
# 底包 1 dex（只有 classes.dex）⇒ 星光 dex = classes2.dex
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
         "com/starry/magic/bridge/ModBridge" "com/starry/magic/sign/SignSpoof" \
         "com/starry/magic/sign/StarryAppComponentFactory" \
         "$LOGIN_CARRIER" "$LOGIN_CARRIER_YNTT"; do
  LC_ALL=C grep -aqF "$t" "$WORK/starry.dex" || { echo "ERROR: $DEX_NAME 缺 $t" >&2; exit 1; }
done
# 两张表必须同步（证书表 + 白名单），否则 TapTap SSO 报 signature not match
LC_ALL=C grep -aqF "com.guigugame.guigubahuang" "$WORK/starry.dex" \
  || { echo "ERROR: $DEX_NAME 内两张表未含 com.guigugame.guigubahuang" >&2; exit 1; }

# 注意：本作官包无被改写的官方 dex（仙逆模板的 classes7.dex TapSDK 补丁段不适用）——
# 登录修复全靠上面这一张新 dex（TapLoginForceWeb + YnttLoginRequest），
# 故这里只做「新增 dex 存在 + 官包 dex 原字节」两侧断言。

SO_MD5="$(md5 -q "$SO_BUILT")"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }
LC_ALL=C grep -aqF "[Ggbh]" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so) \
  || { echo "ERROR: 包内 so 不含 [Ggbh]" >&2; exit 1; }
for t in "[Xnbz]" "[Gyzw]" "[Jfqs]" "[Ysxz]" "[Dwct]" "[Tlgd]"; do
  if LC_ALL=C grep -aqF "$t" <(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so); then
    echo "ERROR: 包内 so 含别家标签 ${t}（so 顶包）" >&2; exit 1
  fi
done

# ---- 官包本体零改动硬校验（除 AndroidManifest.xml / 新增 classes2.dex）----
# AndroidManifest.xml 二进制格式逐字节会变（appComponentFactory + provider/service/权限），
# 故不在 md5 清单内；由 surgical 脚本的 aapt2 结构化比对负责。
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
