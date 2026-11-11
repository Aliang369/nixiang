#!/usr/bin/env bash
# 追迹迷城：安装已签名的星空外科包并校验设备 base.apk 指纹。
set -euo pipefail

# ── 本轮装机确认门（2026-11-11 修复 F4）────────────────────────────────────────
# 原实现只判断 uninstall_done.json「存在」，任何历史标记都能放行（含 2026-11-02 旧标记）。
# 现在要求三件事同时成立才允许任何设备写动作：
#   ① 显式本轮令牌 --confirm zjmc-install:1.0.20261111:com.TeaMelo.DungeonChasers:f9a116a2
#      （或环境变量 ZJMC_INSTALL_CONFIRM）；
#   ② .device_serial == f9a116a2（本轮授权设备）；
#   ③ 卸载标记必须是**本轮带令牌卸载**生成的：serial/package/version 全等，且
#      confirm == zjmc-uninstall:1.0.20261111:com.TeaMelo.DungeonChasers:f9a116a2。
# 默认（未传令牌）在任何设备动作前 exit 3；--dry-run 只打印计划、零设备调用。
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
UNINSTALL_MARK="$SCRIPT_DIR/uninstall_done.json"
VERSION="1.0.20261111"
SERIAL_EXPECT="${ZJMC_SERIAL:-f9a116a2}"
DRY_RUN=0
CONFIRM="${ZJMC_INSTALL_CONFIRM:-}"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --confirm) CONFIRM="${2:-}"; shift 2 ;;
    --confirm=*) CONFIRM="${1#*=}"; shift ;;
    --dry-run) DRY_RUN=1; shift ;;
    *) echo "错误: 未知参数 $1（支持 --confirm <token> / --dry-run）" >&2; exit 2 ;;
  esac
done
APK="${ZJMC_APK:-}"
if [[ -z "$APK" ]]; then
  # 只接受单轨最终包；final 缺失即失败（禁止回退到已作废的单轨候选 ba63f0aa…：缺 85 条业务 META-INF）
  # 无匹配时 ls 非零 + pipefail 会让 set -e 提前退出 ⇒ 显式兜底，交给后面的存在性检查（缺包 exit 1，零设备调用）
  APK="$(ls -t "$NIXIANG_ROOT"/08_output/追迹迷城/追迹迷城_*_starry-singletrack-final-releaseSigned.apk 2>/dev/null | head -1 || true)"
fi
PKG="com.TeaMelo.DungeonChasers"
REMOTE="/data/local/tmp/zjmc_surgical.apk"
ADB_BIN="${ADB_BIN:-adb}"
TOKEN_EXPECT="zjmc-install:${VERSION}:${PKG}:${SERIAL_EXPECT}"
UNINSTALL_TOKEN_EXPECT="zjmc-uninstall:${VERSION}:${PKG}:${SERIAL_EXPECT}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
if [[ "$SERIAL" != "$SERIAL_EXPECT" ]]; then
  echo "拒绝: 当前设备 $SERIAL 不是本轮授权设备 $SERIAL_EXPECT" >&2
  exit 3
fi

if [[ "$DRY_RUN" == "1" ]]; then
  echo "[dry-run] 未调用任何设备工具、未安装任何包"
  echo "[dry-run] 设备: $SERIAL / 包: $PKG / 版本: $VERSION"
  echo "[dry-run] APK: ${APK:-<未找到 final 包 —— 真装会 exit 1，不回退旧候选>}"
  echo "[dry-run] 令牌格式: $TOKEN_EXPECT"
  echo "[dry-run] 卸载标记要求: $UNINSTALL_MARK 内含 version=$VERSION 且 confirm=$UNINSTALL_TOKEN_EXPECT"
  exit 0
fi

if [[ -z "$CONFIRM" ]]; then
  echo "拒绝: 未提供本轮装机确认令牌（当前无用户装机授权）" >&2
  echo "  需要: --confirm $TOKEN_EXPECT" >&2
  echo "  依据: AGENTS §4 装机属设备写操作且异签名需先卸载；本脚本不含任何自动放行开关" >&2
  exit 3
fi
if [[ "$CONFIRM" != "$TOKEN_EXPECT" ]]; then
  echo "拒绝: 确认令牌与「本轮/设备 $SERIAL_EXPECT/版本 $VERSION/包 ${PKG}」不匹配" >&2
  exit 3
fi
[[ -f "$UNINSTALL_MARK" ]] || { echo "拒绝: 缺少本轮带令牌卸载生成的标记 $UNINSTALL_MARK" >&2; exit 3; }
python3 - "$UNINSTALL_MARK" "$VERSION" "$PKG" "$SERIAL" "$UNINSTALL_TOKEN_EXPECT" <<'PY'
import json, sys
mark, ver, pkg, serial, utok = sys.argv[1:6]
try:
    d = json.load(open(mark, encoding="utf-8"))
except Exception as e:
    print("拒绝: 卸载标记不可解析: %s" % e, file=sys.stderr)
    sys.exit(3)
want = {"serial": serial, "package": pkg, "version": ver, "confirm": utok}
bad = ["%s: 期望 %r 实际 %r" % (k, v, d.get(k)) for k, v in want.items() if d.get(k) != v]
if bad:
    print("拒绝: 卸载标记不是本轮带令牌卸载生成的（旧标记不得复用）", file=sys.stderr)
    for b in bad:
        print("  " + b, file=sys.stderr)
    sys.exit(3)
print("卸载标记校验通过（本轮/本设备/本版本 + 令牌一致）：%s" % mark)
PY
[[ -n "$APK" && -f "$APK" ]] || {
  echo "错误: 缺少单轨最终包（追迹迷城_*_starry-singletrack-final-releaseSigned.apk）；不回退旧候选，也不做任何设备调用" >&2
  echo "  期望目录: $NIXIANG_ROOT/08_output/追迹迷城/ ；或用 ZJMC_APK 显式指定 final 包" >&2
  exit 1
}
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

echo "==> 设备余量"
"$ADB_BIN" -s "$SERIAL" shell df -h /data | sed -n '1,4p'

echo "==> push $APK"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE"

echo "==> pm install"
if ! "$ADB_BIN" -s "$SERIAL" shell "pm install -r -d --user 0 '$REMOTE'"; then
  "$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" || true
  echo "错误: 安装失败；未执行额外清数据或卸载其他内容" >&2
  exit 1
fi
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" || true

echo "==> 验证设备包指纹"
bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "==> 安装身份"
"$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|primaryCpuAbi=|lastUpdateTime=' | head -10

cat <<EOF
serial=$SERIAL
package=$PKG
apk=$APK
apk_md5=$(md5 -q "$APK")
install=ok
EOF
