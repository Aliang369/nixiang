#!/usr/bin/env bash
# 动物大联盟 1.0.25 · shellless4 出包（只出包，不装机）
#
# 唯一变量（相对 shellless3）：
#   ① lib/arm64-v8a/libsecret.so  ← 官方包原样（459,344 B / STORED），让 System.loadLibrary("secret") 能成
#   ② classes4.dex                ← 带 <clinit>: System.loadLibrary("secret") 的 ShellGate 桩
#   其余条目逐字节透传（shellless4_assemble_1.0.25.py 内断言）。
# 目的：验证原版 libsecret.so 的 JNI_OnLoad（环境伪装 / hook）是否能让「安全提示·不安全的设备环境」不再出现。
#
# 用法：
#   bash 04_scripts/动物大联盟/shellless4_build_1.0.25.sh --dry-run
#   bash 04_scripts/动物大联盟/shellless4_build_1.0.25.sh
set -euo pipefail

MODE="build"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry" ;;
    -h|--help) sed -n '2,14p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
VER="1.0.25"
KS="$ROOT/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OFFICIAL="$ROOT/02_apks/$GAME/${GAME}_${VER}_official.apk"
BASE="$ROOT/08_output/$GAME/${GAME}_${VER}_shellless3-releaseSigned.apk"
DEX="$SCRIPT_DIR/assets/shellgate/classes4.dex"
OUT_DIR="$ROOT/08_output/$GAME"
TAG="${GAME}_${VER}_shellless4"
UNSIGNED="$OUT_DIR/${TAG}-unsigned.apk"
SIGNED="$OUT_DIR/${TAG}-releaseSigned.apk"
REPORT="$SCRIPT_DIR/shellless4_verify.json"
EXPECT_OFFICIAL_SHA="2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539"
EXPECT_BASE_SHA="d8cce35d008b1ea5638d2790c7dc8119beff4259b8eddd419b56ea1292fca660"
EXPECT_DEX_SHA="282a97c61582bd66d5dd7fb29e9b31b40026ae203f93b22c589c5e4a7d56b974"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
DD="${BT}/dexdump"

say() { printf "[sl4_build] %s\n" "$*"; }
die() { printf "ERROR: %s\n" "$*" >&2; exit 1; }

# ---- 0. 静态预检 -----------------------------------------------------------
[[ -f "$OFFICIAL" ]] || die "缺官方包 $OFFICIAL"
[[ "$(shasum -a 256 "$OFFICIAL" | awk '{print $1}')" == "$EXPECT_OFFICIAL_SHA" ]] || die "官方包 sha256 不符"
[[ -f "$BASE" ]] || die "缺基座 ${BASE}（shellless3）"
BASE_SHA="$(shasum -a 256 "$BASE" | awk '{print $1}')"
[[ "$BASE_SHA" == "$EXPECT_BASE_SHA" ]] || die "基座 sha256=$BASE_SHA 与登记值不符"
[[ -f "$DEX" ]] || die "缺 ${DEX}（先跑 build_shellgate_dex_dwdlm.sh，mode=sl4）"
DEX_SHA="$(shasum -a 256 "$DEX" | awk '{print $1}')"
[[ "$DEX_SHA" == "$EXPECT_DEX_SHA" ]] || die "classes4.dex sha256=$DEX_SHA 与登记值不符"
[[ -f "$KS" ]] || die "缺签名库 $KS"
[[ -n "$BT" ]] || die "缺 build-tools"
"$DD" "$DEX" 2>&1 | grep -q "name          : '<clinit>'" || die "classes4.dex 缺 <clinit>（不是 sl4 口径）"
unzip -p "$OFFICIAL" lib/arm64-v8a/libsecret.so > /dev/null || die "官方包无 lib/arm64-v8a/libsecret.so"

say "official  ${EXPECT_OFFICIAL_SHA:0:16}…"
say "base(sl3) ${BASE_SHA:0:16}…"
say "classes4  ${DEX_SHA:0:16}… ($(stat -f '%z' "$DEX") B)"

if [[ "$MODE" == "dry" ]]; then
  say "[dry-run] 无写入。若执行将产出："
  say "  unsigned : $UNSIGNED"
  say "  signed   : $SIGNED"
  say "  +lib/arm64-v8a/libsecret.so（官方原样，STORED）"
  say "  ~classes4.dex（带 <clinit> loadLibrary(\"secret\")）"
  say "  其余条目逐字节透传"
  exit 0
fi

WORK="$(mktemp -d /tmp/dwdlm_shellless4.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT_DIR"

# ---- 1. 外科组装 -----------------------------------------------------------
say "[1/4] shellless4_assemble（透传断言内置）"
python3 "$SCRIPT_DIR/shellless4_assemble_1.0.25.py" "$BASE" "$OFFICIAL" "$DEX" "$UNSIGNED" | tee "$WORK/assemble.json"

# ---- 2. zipalign -p 4 + apksigner -----------------------------------------
say "[2/4] zipalign -f -p 4 + apksigner（starry_release / ${KS_ALIAS}）"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -6
"$BT/zipalign" -c -p 4 "$SIGNED" > /dev/null || die "zipalign -c 校验失败"

# ---- 3. 摘要 ---------------------------------------------------------------
FINAL_SHA="$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
FINAL_SIZE="$(stat -f '%z' "$SIGNED")"
say "[3/4] OK $SIGNED"
say "        size=$FINAL_SIZE sha256=$FINAL_SHA"

# ---- 4. 静态自检（写 shellless4_verify.json）------------------------------
say "[4/4] 静态自检"
APK="$SIGNED" BASE_APK="$BASE" OFFICIAL_APK="$OFFICIAL" DEX_FILE="$DEX" \
  BT_DIR="$BT" OUT_JSON="$REPORT" \
  python3 "$SCRIPT_DIR/shellless4_verify_1.0.25.py" | tee "$WORK/verify.txt"
say "report: $REPORT"
