#!/usr/bin/env bash
# check_shared_tables.sh —— 出包前断言「本包是否已登记进两张共享表」（只读）
#
# 背景（2026-10-15 保卫向日葵 1.46 实测，用户 m00009 复盘 P1-1）：
#   首次星光包装机后 TapTap 登录失败，根因是两张**共享表**都漏了本包：
#     ① `SignSpoof.java` 无本包证书块 ⇒ 签名回灌不生效 ⇒ TapInitializer 报
#        「应用的包名、签名（certificate fingerprint）未在 TapTap 开发者中心正确配置」
#        ⇒ `JuHeSDK TapTap CONFIG_ERROR` ⇒ `TapTapException: SDK not initialized`；
#     ② `TapLoginForceWeb.java` 的 `PKG_WHITELIST` 无本包 ⇒ 不强制内嵌网页登录。
#   两者都是**纯静态可查**的，却各花掉一轮装机 + 一次用户实测。本脚本把它前置成一条断言。
#
# 用法：
#   bash check_shared_tables.sh --pkg <包名> [--channel taptap] [--root <仓库根>] [--quiet]
#
#   --pkg <包名>      必填，如 com.bwxrk.yqmy.ty4
#   --channel taptap  TapTap / 渠道包：缺任一登记 ⇒ FAIL（rc=1）
#                     默认（非渠道包）缺登记只 WARN（rc=0）—— 不依赖 TapTap 登录的游戏无此义务
#   --root <路径>     仓库根，默认由脚本位置上溯四级推导
#   --quiet           只输出结论行
#
# 退出码：0 = 通过（含仅 WARN）／1 = FAIL（渠道包缺登记）／2 = 用法错
#
# ⚠️ bash 3.2 坑（本脚本首版实测踩到）：变量后紧跟中文/全角字符时必须写 `${VAR}` ——
#    `$VAR：` 会把 `：` 的字节当变量名一部分 ⇒ `set -u` 报 unbound variable。
#    本脚本已全量加花括号，改动时勿退回 `$VAR中文` 写法。
set -uo pipefail

PKG=""
CHANNEL=""
ROOT=""
QUIET=0

usage() {
  awk 'NR>1 { if ($0 ~ /^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"
  exit 2
}

while [ $# -gt 0 ]; do
  case "$1" in
    --pkg)     [ $# -ge 2 ] || usage; PKG="$2"; shift 2 ;;
    --channel) [ $# -ge 2 ] || usage; CHANNEL="$2"; shift 2 ;;
    --root)    [ $# -ge 2 ] || usage; ROOT="$2"; shift 2 ;;
    --quiet)   QUIET=1; shift ;;
    -h|--help) usage ;;
    *) echo "✗ 未知参数 $1" >&2; usage ;;
  esac
done

[ -n "$PKG" ] || { echo "✗ 缺 --pkg" >&2; usage; }

if [ -z "$ROOT" ]; then
  SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
  ROOT="$(cd "$SELF_DIR/../../../.." && pwd)"
fi

SIGN_SPOOF="$ROOT/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java"
FORCE_WEB="$ROOT/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java"

# 表项形态：包名以带引号的字符串字面量出现
probe() {  # probe <文件> <标签>
  _f="$1"; _label="$2"
  if [ ! -f "$_f" ]; then
    echo "  ✗ ${_label}：文件不存在（${_f}）"
    return 1
  fi
  if grep -qF -- "\"${PKG}\"" "$_f"; then
    echo "  ✓ ${_label}：已登记"
    return 0
  fi
  echo "  ✗ ${_label}：**未登记**本包（${PKG}）"
  return 1
}

if [ "$QUIET" != "1" ]; then
  echo "== 共享表登记检查 pkg=${PKG} channel=${CHANNEL:-none} =="
  echo "root=${ROOT}"
fi

miss=0
probe "$SIGN_SPOOF" "SignSpoof（签名回灌证书块）" || miss=$((miss + 1))
probe "$FORCE_WEB"  "TapLoginForceWeb（强制内嵌网页白名单）" || miss=$((miss + 1))

if [ "$miss" = "0" ]; then
  echo "结论：✓ 两张共享表均已登记"
  exit 0
fi

if [ "$CHANNEL" = "taptap" ]; then
  echo "结论：✗ FAIL —— TapTap/渠道包缺 ${miss} 项登记，出包前必须补齐（否则首装登录必失败）"
  echo "      补法：SignSpoof 追加 XXX_PKG/XXX_CERT_B64 + fakes.put（证书逐字节自检见"
  echo "      skill/re-analyze/login/taptap-web-login.md「证书回灌自检」）；TapLoginForceWeb 白名单追加包名。"
  exit 1
fi

echo "结论：⚠ WARN —— 缺 ${miss} 项登记；非 TapTap 渠道包不阻断（若本作实际走 TapTap 登录，请加 --channel taptap）"
exit 0
