#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 壳 libsecret.so 的 KEY / IV / flag 只读 dump（/proc/<pid>/mem）
#
# 目标：把壳缓存在 .bss 的运行期三元组读出来，供「自定义 KEY/IV」路线做对照。
#   flag 4B  @ 0x786b0   u32 LE，==1 表示壳已算完 KEY/IV 并落盘
#   IV   16B @ 0x787a0
#   KEY  32B @ 0x788a0   KEY[i]=SHA256(签名串+包名+"K")[i]^const32[i]
#
# 边界（脚本层面只有读，没有任何写）：
#   * 允许：pidof / cat /proc/<pid>/maps / dd if=/proc/<pid>/mem
#   * 禁止：am start、input、pm install/uninstall/clear、kill、frida、任何设备侧文件写入
#   * 只读、幂等：重复执行结果一致，不生成设备侧文件，不改包状态
#
# 用法：
#   bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh                 # 默认包名，自动取 pid
#   bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh <包名> [pid]
#   bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh -p <包名> --pid 12345
#   bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh --self-test     # 不连设备，纯离线自测
#
# 退出码：0 成功/自测通过；1 前置条件不满足（无 adb/serial/pid/su）；2 参数错误；3 读不到 mem
set -uo pipefail

PKG_DEFAULT="com.fightingstars.games.animal"
SO_NAME="libsecret.so"
FLAG_ADDR=0x786b0; FLAG_LEN=4
IV_ADDR=0x787a0;   IV_LEN=16
KEY_ADDR=0x788a0;  KEY_LEN=32

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL=""

say()  { printf '%s\n' "$*"; }
warn() { printf '[WARN] %s\n' "$*" >&2; }
die()  { printf '[FAIL] %s\n' "$*" >&2; exit "${2:-1}"; }

usage() {
  cat <<'EOS'
用法：
  bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh [包名] [pid]
  bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh -p <包名> --pid <pid>
  bash 04_scripts/动物大联盟/dump_secret_key_dwdlm.sh --self-test   # 离线自测，不连设备
默认包名：com.fightingstars.games.animal
只读：不启动应用、不写设备任何文件。需要 root（su）与目标进程在跑。
EOS
}

# ---------- 参数解析（bash 3.2 兼容：不用 associative array / mapfile / 大小写展开） ----------
PKG="$PKG_DEFAULT"
PID=""
POS=0
SELF_TEST=0
while [ $# -gt 0 ]; do
  case "$1" in
    --self-test) SELF_TEST=1; shift ;;
    -p|--package) [ $# -ge 2 ] || die "缺 $1 的参数" 2; PKG="$2"; shift 2 ;;
    --pid)        [ $# -ge 2 ] || die "缺 --pid 的参数" 2; PID="$2"; shift 2 ;;
    -h|--help)    usage; exit 0 ;;
    -*)           die "未知参数 $1（--help 看用法）" 2 ;;
    *)            POS=$((POS+1))
                  if [ "$POS" -eq 1 ]; then PKG="$1"; else PID="$1"; fi
                  shift ;;
  esac
done

# ---------- 工具函数 ----------
find_adb() {
  if command -v "$ADB_BIN" >/dev/null 2>&1; then return 0; fi
  for c in "${HOME:-/nonexistent}/Library/Android/sdk/platform-tools/adb" "/usr/local/bin/adb" "/opt/homebrew/bin/adb"; do
    if [ -x "$c" ]; then ADB_BIN="$c"; return 0; fi
  done
  return 1
}

# stdin=maps 全文 → 逐行输出 offset=0 的 libsecret.so 基址（十六进制，保持 maps 顺序）
pick_bases() {
  awk '
    {
      if (index($0, "/libsecret.so") == 0) next;   # 必须带绝对路径，排除 [anon:libsecret.so.bss]
      split($0, f, " ");
      if (f[3] != "00000000") next;                # 只要 offset=0 的那一段
      split(f[1], r, "-");
      if (!(r[1] in seen)) { seen[r[1]] = 1; print r[1] }
    }'
}

is_all_byte() {  # $1=hex 串  $2=0|f  → 全为指定半字节时 rc=0
  [ -z "$(printf '%s' "$1" | tr -d "$2")" ]
}

le_u32() {  # 8 位十六进制（小端字节序）→ 十进制
  local h out i
  h="$1"; out=""; i=0
  while [ "$i" -lt 8 ]; do
    out="$out$(printf '%s' "$h" | cut -c $((7-i))-$((8-i)))"
    i=$((i+2))
  done
  printf '%d' "$((0x$out))"
}

# 读 /proc/<pid>/mem：$1=绝对地址(十进制) $2=字节数 → 纯小写 hex（失败输出空）
mem_read() {
  local abs cnt hex
  abs="$1"; cnt="$2"; hex=""
  hex="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'dd if=/proc/$PID/mem bs=1 skip=$abs count=$cnt 2>/dev/null | xxd -p'" 2>/dev/null | tr -d '[:space:]')"
  if [ -z "$hex" ]; then    # 老 toybox 无 xxd 时退回 od
    hex="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'dd if=/proc/$PID/mem bs=1 skip=$abs count=$cnt 2>/dev/null | od -An -tx1'" 2>/dev/null | tr -d '[:space:]')"
  fi
  printf '%s' "$hex"
}

# ---------- 离线自测（不连设备、不读 mem） ----------
self_test() {
  say "== self-test（不连设备、不读 mem、不写任何文件） =="
  local FIXTURE BASES B bad
  FIXTURE='6fc1c80000-6fc1ce3000 r-xp 00000000 fe:48 670118 /data/app/~~x==/com.fightingstars.games.animal-y==/lib/arm64/libsecret.so
6fc1ce6000-6fc1cea000 r--p 00062000 fe:48 670118 /data/app/~~x==/com.fightingstars.games.animal-y==/lib/arm64/libsecret.so
6fc1ced000-6fc1cf8000 rw-p 00065000 fe:48 670118 /data/app/~~x==/com.fightingstars.games.animal-y==/lib/arm64/libsecret.so
6fc1cf8000-6fc1cf9000 rw-p 00000000 00:00 0 [anon:libsecret.so.bss]
7099087000-70990ea000 r-xp 00000000 fe:48 670118 /data/app/~~x==/com.fightingstars.games.animal-y==/lib/arm64/libsecret.so'
  BASES="$(printf '%s\n' "$FIXTURE" | pick_bases)"
  say "  pick_bases → $(printf '%s' "$BASES" | tr '\n' ' ')"
  bad=0
  if [ "$(printf '%s\n' "$BASES" | sed '/^$/d' | wc -l | tr -d ' ')" = "2" ]; then
    say "  [ok] 识别 2 个 offset=0 实体，[anon:...bss] 已排除"
  else
    say "  [bad] 基址实体数不对"; bad=1
  fi
  if [ "$(printf '%s\n' "$BASES" | head -1)" = "6fc1c80000" ]; then
    say "  [ok] 首个基址=6fc1c80000（=maps 第一条 offset=0 的起始）"
  else
    say "  [bad] 首个基址不对"; bad=1
  fi
  B=0x6fc1c80000
  say "  abs 计算：flag=0x$(printf '%x' $((B + FLAG_ADDR)))  iv=0x$(printf '%x' $((B + IV_ADDR)))  key=0x$(printf '%x' $((B + KEY_ADDR)))"
  if [ "$((B + FLAG_ADDR))" = "$((0x6fc1cf86b0))" ]; then
    say "  [ok] 与实测 maps 数值一致（flag 落在文件背书 rw 尾页之后，属 .bss 匿名补零区）"
  else
    say "  [bad] 地址加法不对"; bad=1
  fi
  if [ "$(le_u32 01000000)" = "1" ]; then say "  [ok] le_u32(01000000)=1"; else say "  [bad] le_u32"; bad=1; fi
  if [ "$(le_u32 00000001)" = "16777216" ]; then say "  [ok] le_u32 字节序方向正确"; else say "  [bad] le_u32 方向"; bad=1; fi
  if is_all_byte 0000000000000000 0; then say "  [ok] 全 0 检出"; else say "  [bad] 全 0 检出"; bad=1; fi
  if is_all_byte ffffffff f; then say "  [ok] 全 FF 检出"; else say "  [bad] 全 FF 检出"; bad=1; fi
  if is_all_byte 0a000000 0; then say "  [bad] 非全 0 被误判"; bad=1; else say "  [ok] 非全 0 不误判"; fi
  if [ "$bad" = "1" ]; then say "== self-test 未通过 =="; return 1; fi
  say "== self-test 通过 =="
  return 0
}

if [ "$SELF_TEST" = "1" ]; then self_test; exit $?; fi

# ---------- 前置条件 ----------
find_adb || die "找不到 adb（设 ADB_BIN=/path/to/adb 或装 platform-tools）"
if [ -z "$SERIAL" ] && [ -f "$SCRIPT_DIR/.device_serial" ]; then
  SERIAL="$(tr -d '[:space:]' < "$SCRIPT_DIR/.device_serial")"
fi
if [ -z "$SERIAL" ]; then
  SERIAL="$("$ADB_BIN" devices 2>/dev/null | awk 'NR>1 && $2=="device"{print $1; exit}')"
fi
[ -n "$SERIAL" ] || die "没有可用设备 serial（先跑 device_ready_dwdlm.sh，或在 adb devices 里确认）"
STATE="$("$ADB_BIN" -s "$SERIAL" get-state 2>&1 | tr -d '[:space:]')"
[ "$STATE" = "device" ] || die "设备 $SERIAL 状态=${STATE}（不是 device）"

if [ -z "$PID" ]; then
  PID="$("$ADB_BIN" -s "$SERIAL" shell "pidof $PKG" 2>/dev/null | tr -d '[:space:]')"
fi
PID="$(printf '%s' "$PID" | awk '{print $1}')"
[ -n "$PID" ] || die "包 $PKG 当前没有进程（pidof 为空）；脚本不启动应用 —— 请先在设备上把游戏跑到 app-ready 再执行"
[ "$("$ADB_BIN" -s "$SERIAL" shell "test -d /proc/$PID && echo ok" 2>/dev/null | tr -d '[:space:]')" = "ok" ] \
  || die "pid=$PID 已退出（/proc/$PID 不存在）"

ROOT_UID="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'id -u'" 2>/dev/null | tr -d '[:space:]')"
[ "$ROOT_UID" = "0" ] || die "su 不可用或被拒（su -c 'id -u' 返回 '$ROOT_UID'）：读 /proc/<pid>/mem 需要 root"

MAPS="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'cat /proc/$PID/maps'" 2>/dev/null | tr -d '\r')"
if [ -z "$MAPS" ]; then
  MAPS="$("$ADB_BIN" -s "$SERIAL" shell "cat /proc/$PID/maps" 2>/dev/null | tr -d '\r')"
fi
[ -n "$MAPS" ] || die "读不到 /proc/$PID/maps（root 失效或进程刚退出）"

BASES="$(printf '%s\n' "$MAPS" | pick_bases)"
[ -n "$BASES" ] || die "/proc/$PID/maps 里没有 $SO_NAME 的 offset=0 映射（壳未加载）"
BASE_CNT="$(printf '%s\n' "$BASES" | sed '/^$/d' | wc -l | tr -d ' ')"
BASE="$(printf '%s\n' "$BASES" | head -1)"

say "package       = $PKG"
say "serial        = $SERIAL"
say "pid           = $PID"
say "$SO_NAME 实体  = $BASE_CNT 个 offset=0 映射"
say "base(首个)    = $BASE   ← /proc/$PID/maps 里 $SO_NAME 第一条 offset=00000000 的起始地址"
say "地址计算       = abs = base + vaddr；flag=base+0x786b0  iv=base+0x787a0  key=base+0x788a0"
if [ "$BASE_CNT" -gt 1 ]; then
  warn "壳被加载 $BASE_CNT 次（实测存在双实体）：下面逐个实体都读一遍，取 flag==1 且 KEY 非全 0 的那个"
fi
say ""

RC=0
IDX=0
for B in $BASES; do
  [ -n "$B" ] || continue
  IDX=$((IDX+1))
  BABS=$((0x$B))
  FABS=$((BABS + FLAG_ADDR)); IABS=$((BABS + IV_ADDR)); KABS=$((BABS + KEY_ADDR))
  say "---- 实例 ${IDX}：$B ----"
  FHEX="$(mem_read "$FABS" "$FLAG_LEN")"
  IHEX="$(mem_read "$IABS" "$IV_LEN")"
  KHEX="$(mem_read "$KABS" "$KEY_LEN")"
  if [ -z "$FHEX" ] || [ -z "$IHEX" ] || [ -z "$KHEX" ]; then
    say "  [FAIL] 读不到 /proc/$PID/mem：hex 长度 flag=$(printf '%s' "$FHEX" | wc -c | tr -d ' ') iv=$(printf '%s' "$IHEX" | wc -c | tr -d ' ') key=$(printf '%s' "$KHEX" | wc -c | tr -d ' ')（应为 8/32/64）"
    say "         诊断：$("$ADB_BIN" -s "$SERIAL" shell "su -c 'dd if=/proc/$PID/mem bs=1 skip=$FABS count=4 2>&1 >/dev/null | head -1'" 2>&1 | tr -d '\r\n')"
    say "         常见原因：SELinux/ptrace 限制、目标进程已退出、该地址尚未建立映射"
    RC=3
    continue
  fi
  FVAL="$(le_u32 "$FHEX")"
  say "  flag  0x786b0 (4B)   abs=0x$(printf '%x' $FABS)  hex=$FHEX  u32_le=$FVAL"
  say "  iv    0x787a0 (16B)  abs=0x$(printf '%x' $IABS)  hex=$IHEX"
  say "  key   0x788a0 (32B)  abs=0x$(printf '%x' $KABS)  hex=$KHEX"
  if [ "$FVAL" = "1" ]; then
    say "  [ok] flag==1 → 壳已把 KEY/IV 落盘，本次读值可信"
  else
    say "  [WARN] flag=$FVAL != 1 → 壳还没写入 .bss（或写入路径未走），本次 KEY/IV 不可信；app-ready 后 1-2s 再跑一次"
    RC=3
  fi
  if is_all_byte "$IHEX" 0; then say "  [WARN] IV 全 0 → 无效（未初始化或被壳清零）"; RC=3; fi
  if is_all_byte "$KHEX" 0; then say "  [WARN] KEY 全 0 → 无效（未初始化或被壳清零）"; RC=3; fi
  if is_all_byte "$IHEX" f; then say "  [WARN] IV 全 FF → 疑似读到填充/未写入区，基址可能取错"; RC=3; fi
  if is_all_byte "$KHEX" f; then say "  [WARN] KEY 全 FF → 疑似读到填充/未写入区，基址可能取错"; RC=3; fi
  say ""
  say "  # 贴进报告的 YAML 块"
  say "  shell_libsecret:"
  say "    package: \"$PKG\""
  say "    pid: $PID"
  say "    base: \"0x$B\""
  say "    flag: {addr: \"0x786b0\", hex: \"$FHEX\", u32_le: $FVAL}"
  say "    iv:   {addr: \"0x787a0\", hex: \"$IHEX\"}"
  say "    key:  {addr: \"0x788a0\", hex: \"$KHEX\"}"
  say ""
done

say "本次全部操作只读：未启动应用、未写设备任何文件、未改包状态。"
say "下一步：把 KEY/IV hex 贴进报告的 YAML 块（10_docs/动物大联盟/密钥常量与dump准备_1.0.25_20261105.md 的「运行期实测」小节）。"
exit $RC
