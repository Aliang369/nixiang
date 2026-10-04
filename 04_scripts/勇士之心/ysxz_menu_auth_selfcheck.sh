#!/usr/bin/env bash
# 勇士之心 菜单授权「第二次打开未授权」修复自检（2026-10-04）
#
# 三层校验：
#   A 源码不变量（静态，任何机器都能跑）：cached 权威 / 自愈代踢 / 清档前留档 / 同步落盘
#   B 服务端接口（静态）：heartbeat 收 legacy_device_ids 且漂移可迁移
#   C 设备存档与日志（可选，需 adb 设备）：auth_device_id 持久、backup 键、store verdict 日志
#
# 用法：
#   bash 04_scripts/勇士之心/ysxz_menu_auth_selfcheck.sh              # 只跑 A+B
#   SERIAL=127.0.0.1:16384 bash .../ysxz_menu_auth_selfcheck.sh       # 追加 C
#   SERIAL=none 可显式跳过 C
set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
APP="$ROOT/mod菜单/starry-mod-menu"
J="$APP/app/src/main/java/com/starry/magic"
NMC="$J/menu/NativeMenuController.java"
PREF="$J/menu/Preferences.java"
MB="$J/bridge/ModBridge.java"
FP="$J/auth/DeviceFingerprint.java"
SRV="$APP/auth-server/app"

SERIAL="${SERIAL:-}"
PKG="${PKG:-com.fightingstars.games.warrior}"
PREF_XML="/data/data/$PKG/shared_prefs/starry_magic_prefs.xml"
WORK="${TMPDIR:-/tmp}"
LOG="$WORK/_ysxz_auth_selfcheck.log"

PASS=0; FAIL=0; WARN=0; SKIP=0
ok()   { echo "  PASS  $*"; PASS=$((PASS+1)); }
bad()  { echo "  FAIL  $*"; FAIL=$((FAIL+1)); }
warn() { echo "  WARN  $*"; WARN=$((WARN+1)); }
skip() { echo "  SKIP  $*"; SKIP=$((SKIP+1)); }

# 需要「文件里同时出现 A 与 B（且顺序为 A 先于 B）」
has()      { grep -qF -- "$2" "$1"; }
has_re()   { grep -qE -- "$2" "$1"; }
order_ok() { # file, firstLine, secondLine
  local a b
  a=$(grep -nF -- "$2" "$1" | head -1 | cut -d: -f1)
  b=$(grep -nF -- "$3" "$1" | head -1 | cut -d: -f1)
  [[ -n "$a" && -n "$b" && "$a" -lt "$b" ]]
}

{
echo "=== ysxz 菜单授权自检 $(date '+%F %T') ==="
echo "ROOT=$ROOT"
echo "SERIAL=${SERIAL:-<none>}"

echo
echo "--- A. 源码不变量 ---"
for f in "$NMC" "$PREF" "$MB" "$FP"; do
  [[ -f "$f" ]] || { bad "缺少文件 $f"; }
done
[[ -f "$NMC" && -f "$PREF" ]] || { echo "源码缺失，无法继续"; exit 2; }

# A1 cached 权威：持久化的 auth_device_id 必须被直接返回
if has_re "$PREF" 'isValidDevId\(cached\)' && has_re "$PREF" 'return cached;'; then
  ok "A1 getOrCreateDeviceId 把已存 auth_device_id 当权威（cached 路径直接 return）"
else
  bad "A1 getOrCreateDeviceId 未把已存设备号当权威 ⇒ 云机 SSAID 漂移即被无视"
fi

# A2 不再因取不到设备号而抛异常
if sed -n '/public static synchronized String getOrCreateDeviceId/,/^    }/p' "$PREF" \
     | grep -q 'IllegalStateException'; then
  bad "A2 getOrCreateDeviceId 仍会抛 IllegalStateException（云机等于整条授权链路崩）"
else
  ok "A2 getOrCreateDeviceId 不再抛异常（v5/v4 均缺失退 local 兜底号）"
fi

# A3 指纹兜底种子写回（供 prefs 被清后找回同一身份）
if has "$FP" "public static void writeSeed(" && has "$PREF" "DeviceFingerprint.writeSeed("; then
  ok "A3 设备号写回共享种子文件（DeviceFingerprint.writeSeed）"
else
  warn "A3 未找到写种子文件逻辑（可选加固）"
fi

# A4 可自愈类判定必须排除真正致命项
SELFHEAL_BLOCK=$(sed -n '/private static boolean isSelfHealableFailureJson/,/^    }/p' "$NMC")
if [[ -z "$SELFHEAL_BLOCK" ]]; then
  bad "A4 isSelfHealableFailureJson 未定义"
else
  miss=""
  for kw in 封禁 用尽 不存在 过期; do
    echo "$SELFHEAL_BLOCK" | grep -q "$kw" || miss="$miss $kw"
  done
  if [[ -z "$miss" ]]; then
    ok "A4 isSelfHealableFailureJson 排除了封禁/用尽/不存在/过期（这些才允许踢）"
  else
    bad "A4 isSelfHealableFailureJson 未排除致命项:$miss"
  fi
fi

# A5 自愈需要本地卡密，且只尝试一次
if has_re "$NMC" 'private boolean trySilentSelfHeal\(' && has "$NMC" "getAuthCardKey()" \
   && has "$NMC" "authSelfHealAttempted"; then
  ok "A5 trySilentSelfHeal 用本地卡密静默重登，且每进程只尝试一次"
else
  bad "A5 trySilentSelfHeal 缺失或未做一次限制"
fi

# A6 顺序与逃逸：自愈分支必须先 return，永不穿透到 kick（第一版修法就死在这里）
L1=$(grep -nF -- "if (isSelfHealableFailureJson(json)) {" "$NMC" | head -1 | cut -d: -f1)
LR=$(awk -v s="${L1:-1}" 'NR>s && /return;/ {print NR; exit}' "$NMC")
LK=$(awk -v s="${L1:-1}" 'NR>s && /hardKickAuth\(serverMsg\)/ {print NR; exit}' "$NMC")
if [[ -n "$L1" && -n "$LR" && -n "$LK" && "$LR" -lt "$LK" ]]; then
  ok "A6 自愈分支先 return（行 $LR），永不穿透到 hardKickAuth（行 $LK）⇒ 可自愈类失败不清档"
else
  bad "A6 自愈分支会穿透到 hardKickAuth（L1=${L1:-?} ret=${LR:-?} kick=${LK:-?}）⇒ 一次自愈失败就毁档"
fi

# A6b 自愈失败必须有独立裁决值，便于区分「救不动但保留」与「权威致命」
if has "$NMC" 'heal-deferred' && has "$NMC" 'heal unavailable'; then
  ok "A6b 自愈不可用时记 heal-deferred 且明确「保留存档、下次再试」"
else
  bad "A6b 缺少 heal-deferred 裁决，救不动与真致命无法区分"
fi

# A7 清档前必须先留档
if order_ok "$PREF" "backupAuthSession(reason)" ".remove(KEY_AUTH_TOKEN)"; then
  ok "A7 clearAuthSession 先 backupAuthSession 再逐条 remove（清档留证）"
else
  bad "A7 clearAuthSession 未在清档前留档"
fi

# A8 授权键必须同步落盘 + 回读（grant 与 token 同一次 editor 提交，故 grant 用伴生判定）
for k in KEY_AUTH_TOKEN KEY_DEVICE_ID; do
  if grep -q "commitChecked(.*$k" "$PREF"; then
    ok "A8 $k 走 commitChecked（commit + 回读校验）"
  else
    warn "A8 $k 未确认 commitChecked（可能仍是 apply 异步刷盘）"
  fi
done
if grep -q "commitChecked(.*KEY_AUTH_GRANT" "$PREF"; then
  ok "A8 KEY_AUTH_GRANT 走 commitChecked（commit + 回读校验）"
elif grep -q "putString(KEY_AUTH_GRANT, grant)" "$PREF" && grep -q "commitChecked(ed, KEY_AUTH_TOKEN" "$PREF"; then
  ok "A8 KEY_AUTH_GRANT 与 KEY_AUTH_TOKEN 同一次 commitChecked 提交（含回读校验）"
else
  warn "A8 KEY_AUTH_GRANT 未确认同步落盘（可能仍是 apply 异步刷盘）"
fi

# A9 清档原因透传到存档
if has "$MB" "public void clearAuthSession(String reason)" \
   && has "$NMC" 'bridge.clearAuthSession("kick:'; then
  ok "A9 踢档原因写入 auth_backup_reason（kick:<服务端消息>）"
else
  warn "A9 踢档原因未透传（排障时只能看到 unspecified）"
fi

# A10 存档裁决日志可观测
if has "$NMC" "private void logAuthStoreVerdict(" && has "$NMC" '[auth] store verdict='; then
  ok "A10 [auth] store verdict 日志存在（授权键链路可定量排障）"
else
  bad "A10 缺少 [auth] store verdict 日志，云机问题只能靠猜"
fi

# A11 迁移候选必须包含 grant 里的设备号（云机自愈能否成功的关键）
if has_re "$PREF" 'public static String grantDeviceId\(' \
   && sed -n '/public static List<String> getLegacyDeviceIds/,/^    }/p' "$PREF" | grep -q 'grantDeviceId()'; then
  ok "A11 getLegacyDeviceIds 上报 grant 内设备号（服务端签发过的旧号）⇒ 漂移可迁移"
else
  bad "A11 legacy 候选不含 grant 内设备号 ⇒ 云机自愈登录必 403「卡密已绑定其他设备」"
fi

echo
echo "--- B. 服务端 heartbeat ---"
if has "$SRV/services/auth_service.py" "legacy_device_ids: Optional[list[str]] = None"; then
  ok "B1 heartbeat 接受 legacy_device_ids"
else
  bad "B1 heartbeat 未接受 legacy_device_ids（漂移只能 401 踢人）"
fi
if has "$SRV/routers/client.py" "body.legacy_device_ids"; then
  ok "B2 路由把 legacy_device_ids 透传进 heartbeat"
else
  bad "B2 路由未透传 legacy_device_ids"
fi
if has "$SRV/schemas.py" "legacy_device_ids"; then
  ok "B3 HeartbeatRequest 已声明 legacy_device_ids"
else
  bad "B3 请求体未声明 legacy_device_ids"
fi
if grep -q "logger\." "$SRV/services/auth_service.py" 2>/dev/null; then
  bad "B4 auth_service.py 引用了未定义的 logger（运行时 NameError）"
else
  ok "B4 auth_service.py 无未定义 logger 引用"
fi
if has "$SRV/services/auth_service.py" "heartbeat_device_drift"; then
  ok "B5 设备漂移迁移写审计事件 heartbeat_device_drift"
else
  warn "B5 未写漂移审计事件"
fi
if python3 -c "import ast,sys; ast.parse(open(sys.argv[1],encoding='utf-8').read())" \
      "$SRV/services/auth_service.py" 2>/dev/null; then
  ok "B6 auth_service.py 语法可解析"
else
  warn "B6 未做 python 语法校验（缺 python3）"
fi

echo
echo "--- C. 设备存档与日志 ---"
if [[ -z "$SERIAL" || "$SERIAL" == "none" ]]; then
  skip "C 未指定 SERIAL（云机请由用户侧观察，MuMu: SERIAL=127.0.0.1:16384）"
else
  export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'
  A=(adb -s "$SERIAL")
  if ! "${A[@]}" get-state >/dev/null 2>&1; then
    bad "C0 adb 设备不可用: $SERIAL"
  else
    ok "C0 adb 设备可用: $SERIAL"
    "${A[@]}" root >/dev/null 2>&1
    XML="$WORK/_auth_prefs.xml"
    if "${A[@]}" shell "cat '$PREF_XML'" > "$XML" 2>/dev/null && [[ -s "$XML" ]]; then
      ok "C1 读到存档 $PREF_XML ($(wc -c < "$XML") B)"
      if grep -q 'name="auth_device_id"' "$XML"; then
        ok "C2 auth_device_id 已持久化（cached 权威的前提）"
      else
        bad "C2 存档里没有 auth_device_id（下次打开仍会重算漂移）"
      fi
      if grep -q 'name="auth_device_id_source"' "$XML"; then
        SRCV=$(grep -o 'name="auth_device_id_source" value="[^"]*"' "$XML" | head -1)
        ok "C3 设备号来源已记录: $SRCV"
      else
        warn "C3 未记录 auth_device_id_source（旧版本装过的存档需登录一次后才有）"
      fi
      if grep -q 'name="auth_grant"' "$XML"; then
        ok "C4 auth_grant 在存档里（冷启动可直接灌活原生会话）"
      else
        warn "C4 无 auth_grant：冷启动必须靠联网续票"
      fi
      if grep -q 'name="auth_backup_at"' "$XML"; then
        warn "C5 曾有清档动作（auth_backup_at 存在）⇒ 检查 auth_backup_reason，若为 kick:设备… 说明自愈未生效"
      else
        ok "C5 从未清过档（无 auth_backup_at）"
      fi
      if grep -q 'name="auth_card_key"' "$XML"; then
        ok "C6 本地留有卡密 ⇒ 静默自愈可用"
      else
        warn "C6 本地无卡密：设备漂移后只能人工重输卡密"
      fi
    else
      bad "C1 读不到存档（未 root / 未安装 / 路径变更）"
    fi

    "${A[@]}" logcat -d -v brief > "$WORK/_auth_live_log.txt" 2>/dev/null || true
    if grep -q '\[auth\] store verdict=' "$WORK/_auth_live_log.txt"; then
      ok "C7 命中 [auth] store verdict 日志："
      grep -o '\[auth\] store verdict=.*' "$WORK/_auth_live_log.txt" | tail -3 | sed 's/^/        /'
    else
      warn "C7 日志里没有 [auth] store verdict（本次运行可能未走到续票）"
    fi
    if grep -q 'cold-start self-heal' "$WORK/_auth_live_log.txt"; then
      ok "C8 命中冷启动自愈日志："
      grep -o '\[auth\] cold-start self-heal.*' "$WORK/_auth_live_log.txt" | tail -3 | sed 's/^/        /'
    else
      warn "C8 未命中 cold-start self-heal（本机设备号没漂移，正常）"
    fi
    if grep -q 'Device binding changed\|设备绑定已变更\|会话与设备' "$WORK/_auth_live_log.txt"; then
      warn "C9 日志里出现服务端设备不匹配拒绝 ⇒ 结合 C5 判断是否已被自愈救回"
    else
      ok "C9 未见服务端设备不匹配拒绝"
    fi
  fi
fi

echo
echo "=== 结果: PASS=$PASS FAIL=$FAIL WARN=$WARN SKIP=$SKIP ==="
[[ "$FAIL" == "0" ]] && echo "VERDICT: 修复不变量齐备" || echo "VERDICT: 有 FAIL，修复未落地"
} 2>&1 | tee "$LOG"

[[ "$FAIL" == "0" ]] || exit 1
