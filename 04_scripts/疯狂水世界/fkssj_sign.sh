#!/usr/bin/env bash
# ================================================================
#  疯狂水世界 1.11.6 (com.gx.sw.qa.fkssj004.esj) — 管线 sign 阶段后端
#
#  载体：08_output/疯狂水世界/疯狂水世界_1.11.6_surgical-unsigned.apk
#        （由 mod菜单/starry-mod-menu/scripts/surgical_fkssj_1.11.6.sh 产出）
#  签名：01_tools/starry_release.jks（alias starry，PKCS12，口令 starry2026），
#        与官方 CN=changwan 不同 ⇒ 装机必须卸载原版（见 fkssj_install.sh）。
#
#  ★本作特有：APK Signing Block 里有渠道自有 ID-value 对
#      id=0x64707763  value="259425"
#    apksigner 只写 0x7109871a / 0x42726577 ⇒ **重签必然丢它**，必须签名后回注。
#    回注由 04_scripts/疯狂水世界/fkssj_idblock.py 完成（Walle 原理，不破 v2 签名，
#    原因见该脚本头部注释）。回注后立刻用 apksigner verify 复验，verify 不过即 ERROR。
#
#  官包零改动硬校验：底包 = 设备只读拉取的官方 TapTap 原版 1.11.6
#  （519,961,058 B md5 f8ed1b954a3c5ce91dcecb4e721f3e9b）。
#  允许被改写的只有：新增 classes5.dex、AndroidManifest.xml、新增 libstarry_magic.so。
#  classes.dex~classes4.dex / resources.arsc 必须逐字节不变（md5 现场从底包取，不写死）。
#
#  用法: bash 04_scripts/疯狂水世界/fkssj_sign.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="疯狂水世界"
VERSION="1.11.6"
PKG="com.gx.sw.qa.fkssj004.esj"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="${FKSJ_UNSIGNED:-$OUT/${GAME}_${VERSION}_surgical-unsigned.apk}"
SIGNED="${FKSJ_SIGNED:-$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk}"
BASE_APK="${FKSJ_BASE_APK:-$NIXIANG/09_test/$GAME/base/base.apk}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
IDBLOCK="$NIXIANG/04_scripts/疯狂水世界/fkssj_idblock.py"
SO_BUILT="${FKSJ_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_fkssj/arm64-v8a/libstarry_magic.so}"

BASE_MD5="f8ed1b954a3c5ce91dcecb4e721f3e9b"
CHAN_ID="0x64707763"
CHAN_VALUE="259425"
NEW_DEX="classes5.dex"
# 必须存在于星光 dex 的类（分支 B：Provider 注入 + 悬浮菜单服务 + 桥 + 签名伪装）
NEED_CLASSES=(
  "com/starry/magic/StarryInitProvider"
  "com/starry/magic/menu/MenuService"
  "com/starry/magic/bridge/ModBridge"
  "com/starry/magic/sign/StarryAppComponentFactory"
)
ORIG_DEXES=(classes.dex classes2.dex classes3.dex classes4.dex)

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

step() { echo "[sign] $*"; }
die()  { echo "ERROR: $*" >&2; exit 1; }

# ---------------- 0) 前置 ----------------
[[ -f "$UNSIGNED" ]] || die "missing ${UNSIGNED}（先跑 mod菜单/starry-mod-menu/scripts/surgical_fkssj_1.11.6.sh）"
[[ -f "$KS" ]]       || die "missing keystore $KS"
[[ -f "$SO_BUILT" ]] || die "missing ${SO_BUILT}（先跑 04_scripts/疯狂水世界/fkssj_build_so.sh）"
[[ -f "$IDBLOCK" ]]  || die "missing ${IDBLOCK}"
[[ -f "$BASE_APK" ]] || die "missing 底包 ${BASE_APK}（签名阶段需要它做官包零改动比对）"
have="$(md5 -q "$BASE_APK")"
[[ "$have" == "$BASE_MD5" ]] || die "底包指纹不符：$have != $BASE_MD5"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || die "找不到 build-tools（zipalign/apksigner）"

WORK="$(mktemp -d /tmp/fkssj_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

# ---------------- 1) 结构硬校验（签名前，只读 unsigned） ----------------
step "[1/6] 结构校验：dex 编号无缺口、星光 dex 落位 ${NEW_DEX}"
DEX_LIST="$(unzip -Z1 "$UNSIGNED" 'classes*.dex' | sed 's#.*/##' | sort -V)"
[[ -n "$DEX_LIST" ]] || die "包内无 classes*.dex"
MAX_DEX="$(printf '%s\n' "$DEX_LIST" | tail -1)"
[[ "$MAX_DEX" == "$NEW_DEX" ]] || die "顶层 dex 最大编号是 ${MAX_DEX}，期望 ${NEW_DEX}（ART 顺序扫描遇缺口即停，星光 dex 永不加载）"
i=1
while IFS= read -r d; do
  want="classes${i}.dex"; [[ $i -eq 1 ]] && want="classes.dex"
  [[ "$d" == "$want" ]] || die "dex 编号缺口：第 ${i} 个是 ${d}，期望 ${want}"
  i=$((i+1))
done <<< "$DEX_LIST"

step "[2/6] 星光 dex 必需类与包名串"
DW="$WORK/$NEW_DEX"
unzip -p "$UNSIGNED" "$NEW_DEX" > "$DW"
[[ -s "$DW" ]] || die "${NEW_DEX} 抽取为空"
for c in "${NEED_CLASSES[@]}"; do
  LC_ALL=C grep -aq -- "$c" "$DW" || die "${NEW_DEX} 缺类 ${c}"
done
LC_ALL=C grep -aq -- "$PKG" "$DW" || die "${NEW_DEX} 缺包名串 ${PKG}"
# 2026-10-28：SignSpoof 证书表已删除 ⇒ 不再做"dex 携带官方证书 base64 前缀"归属校验。
# 归属改由上一条「dex 必含本作包名串」+ 星光类清单覆盖。

step "[3/6] 官包零改动：${ORIG_DEXES[*]} + resources.arsc 逐条 md5 比对底包"
for e in "${ORIG_DEXES[@]}" resources.arsc; do
  a="$(unzip -p "$UNSIGNED" "$e" | md5 -q)"
  b="$(unzip -p "$BASE_APK" "$e" | md5 -q)"
  [[ "$a" == "$b" ]] || die "受保护条目 ${e} 被改写：${a} != ${b}"
  printf '      %-16s %s  == 底包\n' "$e" "$a"
done

step "[4/6] 星光 so 归属与一致性"
IN_SO="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO="$(md5 -q "$SO_BUILT")"
[[ "$IN_SO" == "$SRC_SO" ]] || die "包内 so(${IN_SO}) != 构建产物(${SRC_SO})——旧 so 混包"
SOF="$WORK/libstarry_magic.so"
unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so > "$SOF"
LC_ALL=C grep -aq -- "\[Fkssj\]" "$SOF" || die "包内 so 不含 [Fkssj] 日志标签"
for s in "\[Xnbz\]" "\[Haly\]" "\[Sjzz\]" "\[Zmxy\]" "\[Gyzw\]" "\[Jfqs\]"; do
  LC_ALL=C grep -aq -- "$s" "$SOF" && die "包内 so 含别家标签 ${s}"
done
printf '      so md5 %s（与 libs_fkssj 一致）\n' "$IN_SO"

# ---------------- 2) zipalign + apksigner ----------------
step "[5/6] zipalign -f -p 4 + apksigner sign"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
rm -f "$SIGNED" "$SIGNED.idsig"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
[[ -f "$SIGNED" ]] || die "apksigner 未产出 ${SIGNED}"
rm -f "$SIGNED.idsig"

# ---------------- 3) 渠道块回注（必须，见文件头） ----------------
step "[6/6] 回注签名块渠道 ID-value 对 ${CHAN_ID}=\"${CHAN_VALUE}\" 并复验"
PRE_CHAN="$(python3 "$IDBLOCK" check "$SIGNED" --id "$CHAN_ID" --expect "$CHAN_VALUE" || true)"
if printf '%s' "$PRE_CHAN" | grep -q 'RESULT: OK'; then
  step "      apksigner 已保留该对（无先例，值得记录）"
else
  python3 "$IDBLOCK" inject "$SIGNED" --id "$CHAN_ID" --value "$CHAN_VALUE"
fi
python3 "$IDBLOCK" check "$SIGNED" --id "$CHAN_ID" --expect "$CHAN_VALUE" >/dev/null \
  || die "回注后渠道块校验失败"

# 回注改动了签名块尾部与 EOCD ⇒ v2 摘要输入不变，verify 必须仍通过（这是回注不破签的实证）
if ! "$BT/apksigner" verify --print-certs "$SIGNED" > "$WORK/verify.txt" 2>&1; then
  cat "$WORK/verify.txt" >&2
  die "回注后 apksigner verify 失败 ⇒ 回注破坏签名，禁止交付"
fi
sed -n '1,6p' "$WORK/verify.txt"

step "渠道块明细："
python3 "$IDBLOCK" check "$SIGNED" --id "$CHAN_ID" --expect "$CHAN_VALUE" | sed 's/^/      /'

SM="$(md5 -q "$SIGNED")"
SS="$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
SZ="$(stat -f%z "$SIGNED")"
echo
echo "[sign] DONE → $SIGNED"
echo "       size   : $SZ"
echo "       md5    : $SM"
echo "       sha256 : $SS"
echo "       signer : CN=StarryMagic (SHA-256 83C5CC0BE218D046B3B7AF0D2A9F8236297FE228171AF2974B71BAC130736F1D)"
echo "       下一步 : bash 04_scripts/疯狂水世界/fkssj_install.sh"
