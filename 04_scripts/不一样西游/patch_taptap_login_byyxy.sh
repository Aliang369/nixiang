#!/usr/bin/env bash
# 不一样西游 — A 轨「登录方式」改包法：强制 TapTap 走内嵌 WebView 网页登录。
#
# 依据（AGENTS §3 TapTap 条目 + skill/re-analyze/login/taptap-web-login.md「现行口径」
# 2026-10-29 双轨制）：A 轨＝登录方式/渠道分支改包法；B 轨＝签名运行时替换（killer，另脚本）。
# **同一链条禁双修**：TapSignKit 保持官方原样，签名只由 B 轨 killer 处理。
#
# patch 点（本 SDK 形态：com.taptap.sdk.ui.Block 系）：
#   com/taptap/sdk/ui/LoginBlock;->onViewCreated(View)V 里
#     new LoginClient(delegate) → sendLoginRequest(request)        ← 客户端 SSO，改签包必失败
#   ⇒ 改成
#     new WebViewHandler(delegate) → authorize(request)            ← SDK 自带完整 web 路径
#   （WebViewHandler.authorize 内部：new WebBlock + setLoginCallback(WebViewHandler$1)
#     + TapTapLoginTrackerHelper.authorizationOpen("webview") + activity.startBlock(block)）
#   LoginClient 整个不再被创建 ⇒ 不存在 cloudGameHandler 的 bind/unbind 记账问题。
#
# ⚠️ 两个已实测失败的错法（别再走）：
#   ① 在 LoginClient.sendLoginRequest 里提前 return：跳过 cloudGameHandler 的 bind 记账，
#      销毁时 unbindService 抛 `IllegalArgumentException: Service not registered` ⇒ 点开始游戏闪退。
#   ② 把 TapTapActivity 里的 new LoginBlock 换成 new WebBlock：绕过 WebViewHandler.authorize
#      的 setLoginCallback + startBlock ⇒ 登录页永远转圈（WebBlock 拿不到回调）。
#
# 产物：07_patched/不一样西游/classes2_patched.dex
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
ORIG_APK="$NIXIANG/02_apks/不一样西游_官方_com.wingjoy.toWest.apk"
ORIG_MD5="0f5053a9c551ec643ab30eb9d8b55576"
OUT_DIR="$NIXIANG/07_patched/不一样西游"
OUT="$OUT_DIR/classes2_patched.dex"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
TARGET_REL="com/taptap/sdk/ui/LoginBlock.smali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: 缺少官方底包 ${ORIG_APK}" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 底包指纹不符" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: 缺少 smali 工具" >&2; exit 1; }

WORK="$(mktemp -d /tmp/byyxy_c2patch.XXXXXX)"
trap 'rm -rf "${WORK}"' EXIT
mkdir -p "$OUT_DIR"

unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2.dex"
"$BAKSMALI_BIN" d -o "$WORK/smali" "$WORK/classes2.dex"

T="$WORK/smali/${TARGET_REL}"
[[ -f "$T" ]] || { echo "ERROR: 目标类缺失 ${TARGET_REL}（SDK 版本变了？重取锚点）" >&2; exit 1; }

python3 "$NIXIANG/04_scripts/不一样西游/patch_loginblock.py" "$T"

"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali"

# 自检：重新反汇编产物，确认 LoginClient 不再被创建、authorize 已接管
"$BAKSMALI_BIN" d -o "$WORK/verify" "$OUT" >/dev/null
V="$WORK/verify/${TARGET_REL}"
[[ -f "$V" ]] || { echo "ERROR: 产物缺 LoginBlock" >&2; exit 1; }
grep -q "Lcom/taptap/sdk/ui/WebViewHandler;->authorize(Lcom/taptap/sdk/LoginRequest;)V" "$V" \
  || { echo "ERROR: 产物缺 WebViewHandler.authorize 调用" >&2; exit 1; }
CNT_LC=$(grep -c "Lcom/taptap/sdk/ui/LoginClient;-><init>" "$V" || true)
[[ "$CNT_LC" == "0" ]] || { echo "ERROR: 产物仍在 new LoginClient(${CNT_LC})" >&2; exit 1; }
# onViewCreated 里不得再有「把 handler 存进 client 字段」；构造函数里置 null 那条属正常，故按方法切片判定
sed -n '/method protected onViewCreated/,/end method/p' "$V" | grep -q -- "->client:Lcom/taptap/sdk/ui/LoginClient;" \
  && { echo "ERROR: onViewCreated 里类型不匹配的 iput 未删除" >&2; exit 1; } || true

echo "[taptap-a] ok $OUT ($(stat -f%z "$OUT") B, md5=$(md5 -q "$OUT"))"
