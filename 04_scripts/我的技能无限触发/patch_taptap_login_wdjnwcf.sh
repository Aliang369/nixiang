#!/usr/bin/env bash
# 我的技能无限触发 A 轨（登录方式＝改包法）：
#   bakskmali 官方 classes.dex → patch_loginblock.py 改 LoginBlock → smali 回编 → 07_patched/{游戏}/classes_patched.dex
#
# 只改一处语义：TapTap SSO 客户端登录 → SDK 自带 WebView 网页登录（LoginClient 整个不被创建）。
# 其余官方类逐字节不变（baksmali/smali 往返会重排 dex 布局，故以「语义等价 + 锚点命中 1」为判据，
# 出包阶段用 dexdump 级校验：类数量、方法数量、目标方法体变化）。
#
# 用法：bash 04_scripts/我的技能无限触发/patch_taptap_login_wdjnwcf.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE_APK="${WDJNWCF_BASE_APK:-$NIXIANG/02_apks/我的技能无限触发_官方_com.SmallMill.EnMaster.apk}"
OUT_DIR="$NIXIANG/07_patched/我的技能无限触发"
OUT="$OUT_DIR/classes_patched.dex"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing $BASE_APK" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" && -x "$SMALI_BIN" ]] || { echo "ERROR: missing smali tools" >&2; exit 1; }

WORK="$(mktemp -d /tmp/wdjnwcf_login.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT_DIR"

unzip -p "$BASE_APK" classes.dex > "$WORK/classes.dex"
[[ -s "$WORK/classes.dex" ]] || { echo "ERROR: 底包无 classes.dex" >&2; exit 1; }

echo "[1/4] baksmali 官方 classes.dex"
"$BAKSMALI_BIN" d -o "$WORK/smali" "$WORK/classes.dex"

echo "[2/4] patch LoginBlock（SSO → SDK web 登录）"
python3 "$SCRIPT_DIR/patch_loginblock.py" "$WORK/smali/com/taptap/sdk/ui/LoginBlock.smali"

echo "[2b/4] patch 防沉迷/实名（客户端改判：已实名+成年，用户 2026-11-02 点名）"
python3 "$SCRIPT_DIR/patch_antiaddiction_wdjnwcf.py" "$WORK/smali"
grep -qF '[starry] 强制' "$WORK/smali/com/tapsdk/antiaddictionui/AntiAddictionUIImpl\$7.smali" \
  || { echo "ERROR: \$7 改判未写入" >&2; exit 1; }
grep -qF '短路：不做服务端实名核对' "$WORK/smali/com/tapsdk/antiaddiction/AntiAddictionImpl.smali" \
  || { echo "ERROR: AntiAddictionImpl.login 未短路" >&2; exit 1; }

echo "[3/4] 锚点复核"
grep -q 'WebViewHandler;->authorize(Lcom/taptap/sdk/LoginRequest;)V' "$WORK/smali/com/taptap/sdk/ui/LoginBlock.smali" \
  || { echo "ERROR: authoriz e 调用未写入" >&2; exit 1; }
if grep -q 'LoginClient;->sendLoginRequest' "$WORK/smali/com/taptap/sdk/ui/LoginBlock.smali"; then
  echo "ERROR: LoginClient.sendLoginRequest 仍在 ⇒ 补丁无效" >&2; exit 1
fi
# onViewCreated 内必须已无 LoginClient（<init> 里的 client=null 赋值属正常，不能误判）
awk '/^\.method protected onViewCreated/,/^\.end method/' "$WORK/smali/com/taptap/sdk/ui/LoginBlock.smali" > "$WORK/ovc.txt"
grep -q 'Lcom/taptap/sdk/ui/LoginClient' "$WORK/ovc.txt" && { echo "ERROR: onViewCreated 内仍有 LoginClient" >&2; exit 1; } || true
grep -q 'WebViewHandler' "$WORK/ovc.txt" || { echo "ERROR: onViewCreated 内无 WebViewHandler" >&2; exit 1; }

echo "[4/4] smali 回编 → $OUT"
"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"
echo "[login] ok $OUT ($(stat -f '%z' "$OUT") B, md5=$(md5 -q "$OUT"))"
