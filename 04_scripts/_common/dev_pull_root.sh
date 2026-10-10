#!/usr/bin/env bash
# ================================================================
#  dev_pull_root.sh — 把设备上 root 才能读的文件可靠取到本机（本机 macOS / Linux 通用）
#
#  用法:
#    bash 04_scripts/_common/dev_pull_root.sh <设备绝对路径> [<本地路径>] \
#         [--serial f9a116a2] [--keep-tmp]
#    bash 04_scripts/_common/dev_pull_root.sh --help
#    序列号优先级: --serial > 环境变量 DEV_SERIAL > 默认 f9a116a2
#    本地路径缺省 = ./$(basename <设备路径>)
#
#  为什么不能只 `adb pull`（真机 f9a116a2 实测，直接采信）:
#    · 对 /data/data/... 下的文件，su 下 `md5sum` / `cat` 会 Permission denied，
#      但 `ls -l` 与 `cp` 正常 ⇒ 走「root cp 到 /sdcard/Download 中转，再 adb pull」。
#    · 取文件可靠姿势: su -c 'cp <src> /sdcard/Download/<tmp>' → adb pull → su -c 'rm -f <tmp>'
#
#  行为:
#    ① 目录入参直接报错退出（本脚本只处理文件）
#    ② 先试普通 `adb pull`（有些文件无需 root），失败或结果 0 字节 ⇒ 走 /sdcard/Download 中转
#    ③ 校验：用 `su -c "ls -l <设备路径>"` 取设备侧尺寸（**不用 md5sum**：su 下必然 Permission denied），
#       与本地尺寸比对；一致打印 PULL_OK 并 exit 0，否则 PULL_FAIL + exit 1
#    ④ 设备路径含空格一律加引号；所有 adb 调用都带 -s <serial>
#
#  边界（设备被多个并行会话共享）: 本脚本只读设备文件 + 可能删自己建的中转临时件，
#    不 force-stop、不启停游戏、不装/卸 APK、不动用户其它文件。
#
#  退出码: 0=PULL_OK；1=PULL_FAIL / 目录入参；2=用法错
#  bash 3.2 兼容（不用 mapfile / 关联数组 / local -n / ${v^^}）
# ================================================================
set -euo pipefail

DEF_SERIAL="f9a116a2"
RELAY_DIR="/sdcard/Download"
RELAY_PREFIX="_dsh_pull_"

SERIAL="${DEV_SERIAL:-$DEF_SERIAL}"
KEEP_TMP=0
DEV_PATH=""
LOCAL_PATH=""

usage() { awk 'NR>1 { if ($0 ~ /^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"; }
die()   { echo "✗ FAIL  $*" >&2; exit 1; }
bad()   { echo "✗ 用法  $*" >&2; exit 2; }

# ---------------- 参数解析 ----------------
while [ $# -gt 0 ]; do
  case "$1" in
    --serial)
      [ $# -ge 2 ] || bad "--serial 缺值"
      SERIAL="$2"; shift 2 ;;
    --keep-tmp) KEEP_TMP=1; shift ;;
    -h|--help) usage; exit 0 ;;
    --*) bad "未知参数 $1（--help 看用法）" ;;
    *)
      if [ -z "$DEV_PATH" ]; then
        DEV_PATH="$1"
      elif [ -z "$LOCAL_PATH" ]; then
        LOCAL_PATH="$1"
      else
        bad "多余的位置参数 $1"
      fi
      shift ;;
  esac
done

[ -n "$DEV_PATH" ] || { usage; bad "缺少 <设备绝对路径>"; }
[ -n "$SERIAL" ] || bad "--serial 不能为空"

# ---------------- ① 目录入参直接在本地拦掉（不碰设备） ----------------
case "$DEV_PATH" in
  */) die "设备路径是目录（以 / 结尾）: $DEV_PATH —— 本脚本只处理文件，请给出具体文件名" ;;
esac

if [ -z "$LOCAL_PATH" ]; then
  LOCAL_PATH="./$(basename "$DEV_PATH")"
fi
if [ -d "$LOCAL_PATH" ]; then
  bad "本地路径 {$LOCAL_PATH} 是已存在的目录，请给出具体文件名（避免 adb pull 把它塞进目录里造成歧义）"
fi

DEV_BASE="$(basename "$DEV_PATH")"
RELAY_TMP="${RELAY_DIR}/${RELAY_PREFIX}${DEV_BASE}"

# ---------------- 设备侧工具函数 ----------------
devq() {
  # 设备侧路径安全引用：转义 shell 特殊字符（结果保证不含单引号），
  # 使整条命令能塞进 adb shell 的 "su -c '<cmd>'" 单参数形式，空格路径也能过。
  local s="$1"
  case "$s" in
    *"'"*) die "设备路径含单引号，本脚本不支持: $s" ;;
  esac
  printf '%s' "$s" | sed 's/[^A-Za-z0-9_.\/:=@,+-]/\\&/g'
}

su_run() {
  # 整条 "su -c '<cmd>'" 作为**一个**参数交给 adb shell：多参数形式在新版 adb 会被
  # 逐参数重新加引号转义（内层引号变字面量就跑不起来），单参数形式老/新 adb 都安全。
  adb -s "$SERIAL" shell "su -c '$1'"
}

su_ls() {
  # 只读探针（su 下读 /data/data 文件唯一可靠的一条路）；失败返回空串。
  adb -s "$SERIAL" shell "su -c 'ls -l $(devq "$1")'" 2>/dev/null | tr -d '\r' || true
}

ls_size() {
  # 从 ls -l 输出取尺寸（字段序: 权限 链接数 owner group 尺寸 …），非数字视为空。
  printf '%s\n' "$1" | awk 'NF >= 5 && $5 ~ /^[0-9]+$/ { print $5; exit }'
}

local_size() {
  [ -f "$1" ] || { printf ''; return 0; }
  wc -c < "$1" | tr -d '[:space:]'
}

command -v adb >/dev/null 2>&1 || die "本机找不到 adb（PATH 里没有）"
adb -s "$SERIAL" get-state >/dev/null 2>&1 || die "设备 $SERIAL 不可达（adb devices 看一眼）"

echo "== 目标 =="
echo "  设备        : $SERIAL"
echo "  设备路径    : $DEV_PATH"
echo "  本地路径    : $LOCAL_PATH"

# ---------------- 设备侧探针：存在性 / 类型 / 尺寸 ----------------
LS_DEV="$(su_ls "$DEV_PATH")"
if [ -z "$LS_DEV" ]; then
  echo "PULL_FAIL 设备侧读不到: $DEV_PATH" >&2
  echo "  · 路径写错 / 文件不存在 / su 不通（root 掉了）三种都在这一条上" >&2
  echo "  · 若是 /data/data 下文件，确认写法: adb -s ${SERIAL} shell \"su -c 'ls -l ${DEV_PATH}'\"" >&2
  exit 1
fi
case "$LS_DEV" in
  d*) echo "PULL_FAIL 设备路径是目录: $DEV_PATH —— 本脚本只处理文件" >&2; exit 1 ;;
esac

DEV_SIZE="$(ls_size "$LS_DEV")"
echo "  设备侧尺寸  : ${DEV_SIZE:-未知（ls -l 解析失败，稍后以本地尺寸判定）}"

mkdir -p "$(dirname "$LOCAL_PATH")"

# ---------------- ② 先试普通 adb pull ----------------
PULL_MODE="direct"
echo "→ adb -s ${SERIAL} pull \"${DEV_PATH}\" \"${LOCAL_PATH}\""
set +e
adb -s "$SERIAL" pull "$DEV_PATH" "$LOCAL_PATH"
RC_PULL=$?
set -e
L_SIZE="$(local_size "$LOCAL_PATH")"

if [ "$RC_PULL" -ne 0 ] || [ -z "$L_SIZE" ] || [ "$L_SIZE" = "0" ]; then
  # ---------------- ③ 中转：root cp 到 /sdcard/Download 再 pull ----------------
  PULL_MODE="relay"
  echo "  · 直连 pull 没成（rc=${RC_PULL} 本地尺寸=${L_SIZE:-无}）⇒ 走 root 中转"
  echo "→ su -c 'cp \"${DEV_PATH}\" \"${RELAY_TMP}\"'"
  su_run "cp $(devq "$DEV_PATH") $(devq "$RELAY_TMP")" \
    || die "su cp 到中转目录失败: ${DEV_PATH} → ${RELAY_TMP}（root 不通？路径不对？）"

  rm -f "$LOCAL_PATH" 2>/dev/null || true
  echo "→ adb -s ${SERIAL} pull \"${RELAY_TMP}\" \"${LOCAL_PATH}\""
  set +e
  adb -s "$SERIAL" pull "$RELAY_TMP" "$LOCAL_PATH"
  RC_PULL=$?
  set -e
  L_SIZE="$(local_size "$LOCAL_PATH")"

  if [ "$KEEP_TMP" = "1" ]; then
    echo "  · --keep-tmp：保留中转件 ${RELAY_TMP}"
  elif [ -z "$L_SIZE" ] || [ "$L_SIZE" = "0" ]; then
    # 中转 pull 也失败：保留现场供排查，不删（否则证据没了）
    echo "  · 中转 pull 也失败 ⇒ 保留中转件 ${RELAY_TMP} 供排查（未删）" >&2
  else
    echo "→ su -c 'rm -f \"${RELAY_TMP}\"'"
    su_run "rm -f $(devq "$RELAY_TMP")" || echo "  · 中转件删除失败（不影响结果）: ${RELAY_TMP}" >&2
  fi
fi

# ---------------- ④ 尺寸核验（不用 md5sum，理由见文件头） ----------------
DEV_SIZE="$(ls_size "$LS_DEV")"
L_SIZE="$(local_size "$LOCAL_PATH")"

if [ -n "$L_SIZE" ] && [ -n "$DEV_SIZE" ] && [ "$L_SIZE" = "$DEV_SIZE" ]; then
  echo "PULL_OK path=${LOCAL_PATH} size=${L_SIZE} mode=${PULL_MODE}"
  exit 0
fi

echo "PULL_FAIL path=${LOCAL_PATH} size_local=${L_SIZE:-无} size_dev=${DEV_SIZE:-未知} mode=${PULL_MODE}" >&2
echo "  · 本地 0 字节/缺文件 ⇒ 两条 pull 路都没成；设备侧 ls 读得到 ⇒ 大概率是 /sdcard 中转读写或 adb 传输问题" >&2
echo "  · 设备侧尺寸解析不到 ⇒ 用一条确认能 ls -l 的路径复核（su 不通时先修 root）" >&2
exit 1
