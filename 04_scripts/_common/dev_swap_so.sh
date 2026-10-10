#!/usr/bin/env bash
# ================================================================
#  dev_swap_so.sh — 乐变（lebian）热更层 so 热替换：只改 native so 的快速迭代轮
#
#  用法:
#    bash 04_scripts/_common/dev_swap_so.sh <本地so路径> \
#         [--serial f9a116a2] [--pkg com.yhhy.whrhx.aligames] [--dry-run]
#    bash 04_scripts/_common/dev_swap_so.sh --help
#    序列号优先级: --serial > 环境变量 DEV_SERIAL > 默认 f9a116a2
#
#  为什么换的是 lebian 那一片（真机 f9a116a2 实测，直接采信）:
#    · /data/data/<pkg>/lebian/<pkg>-1/lib/arm64/libstarry_magic.so
#        ← 真正被加载执行的那份副本，**可写**，root 可 cp 覆盖
#    · /data/app/~~*/<pkg>-*/lib/arm64/libstarry_magic.so
#        ← App 侧 loadLibrary 那份，incremental-fs 挂载，**不可写**（setenforce 0 也无效）
#    ⇒ 迭代调试唯一可行路径 = 热换 lebian 副本（本脚本）。
#    进程内出现两次 JNI_OnLoad 属正常：两份 so 都被加载。
#    （已有 04_scripts/_common/so_hotswap.sh 处理的是 /data/app 下 extractNativeLibs=true
#      包的原地替换；本机 incremental-fs 形态不适用，故另开本脚本，未改动它。）
#
#  ★ 本脚本适用于所有走乐变热更的包：
#    包名与 lebian 目录都是变量，别的乐变游戏传 --pkg 即可
#    （约定：乐变派生目录固定为 /data/data/<pkg>/lebian/<pkg>-1/lib/arm64/<so名>）。
#
#  校验口径（**刻意不用 md5sum**，理由）:
#    本机 su 对 /data/data 下文件有诡异行为：`su -c 'md5sum <f>'` 与 `su -c 'cat <f>'`
#    会 Permission denied，而 `su -c 'ls -l <f>'` 与 `su -c 'cp <f> <dst>'` 正常
#    ⇒ 校验一律以 **ls -l 的尺寸**为准（本地 md5 只用于记账/命名），
#      调 md5sum 只会把成功换片误判成失败。
#
#  边界（本设备被多个并行会话共享）:
#    本脚本只做 push + cp + 尺寸核验；**不** force-stop、**不**启动游戏、**不**装/卸 APK。
#    重启游戏 / 清 logcat / 取日志只在结尾**打印**，由调用方自行决定。
#    一律 `adb -s <serial>`；取 root 只用 `adb shell "su -c '...'"` 形式（不用 adb root）。
#
#  退出码: 0=换片且尺寸核验一致；1=FAIL（含乐变负载未回填）；2=用法错
#  bash 3.2 兼容（不用 mapfile / 关联数组 / local -n / ${v^^}）
# ================================================================
set -euo pipefail

LIB_NAME="libstarry_magic.so"
DEF_SERIAL="f9a116a2"
DEF_PKG="com.yhhy.whrhx.aligames"
STAMP_MARK="STARRYSTAMP1:EPOCH="
DEV_TMP_DIR="/data/local/tmp"
REMOTE_PREFIX="libsm_"

SERIAL="${DEV_SERIAL:-$DEF_SERIAL}"
PKG="$DEF_PKG"
DRY_RUN=0
SO=""

usage() { awk 'NR>1 { if ($0 ~ /^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"; }
die()   { echo "✗ FAIL  $*" >&2; exit 1; }
bad()   { echo "✗ 用法  $*" >&2; exit 2; }

# ---------------- 参数解析 ----------------
while [ $# -gt 0 ]; do
  case "$1" in
    --serial)
      [ $# -ge 2 ] || bad "--serial 缺值"
      SERIAL="$2"; shift 2 ;;
    --pkg)
      [ $# -ge 2 ] || bad "--pkg 缺值"
      PKG="$2"; shift 2 ;;
    --dry-run) DRY_RUN=1; shift ;;
    -h|--help) usage; exit 0 ;;
    --*) bad "未知参数 $1（--help 看用法）" ;;
    *)
      if [ -z "$SO" ]; then SO="$1"; else bad "多余的位置参数 $1"; fi
      shift ;;
  esac
done

[ -n "$SO" ] || { usage; bad "缺少 <本地so路径>"; }
[ -n "$SERIAL" ] || bad "--serial 不能为空"
[ -n "$PKG" ] || bad "--pkg 不能为空"
[ -f "$SO" ] || die "本地 so 不存在: $SO"
[ -s "$SO" ] || die "本地 so 是空文件: $SO"

# ---------------- 工具函数（macOS 与 Linux 择一可用者） ----------------
hex_head4() {
  # ELF 魔数前 4 字节的十六进制。macOS 优先 od（BSD od 的 -tx1 与 GNU 一致），
  # 不假设有 file 命令。返回形如 "7f454c46"。
  local f="$1" m=""
  if command -v od >/dev/null 2>&1; then
    m="$(od -An -tx1 -N4 "$f" 2>/dev/null | tr -d ' \n')" || m=""
  fi
  if [ -z "$m" ] && command -v xxd >/dev/null 2>&1; then
    m="$(xxd -p -l 4 "$f" 2>/dev/null | tr -d ' \n')" || m=""
  fi
  printf '%s' "$m"
}

md5_of() {
  # macOS: md5 -q；Linux: md5sum。都没有则返回空（调用方按 unknown 处理）。
  local f="$1"
  if command -v md5 >/dev/null 2>&1; then
    md5 -q "$f" 2>/dev/null || true
  elif command -v md5sum >/dev/null 2>&1; then
    md5sum "$f" 2>/dev/null | awk '{print $1}' || true
  else
    printf ''
  fi
}

size_of_file() { wc -c < "$1" | tr -d '[:space:]'; }

stamp_scan() {
  # 在文件字节里找 ASCII 串 "STARRYSTAMP1:EPOCH=<数字>"（构建戳，仓库发布的必要门禁）。
  # macOS 的 BSD grep 在多字节 locale 下对二进制文件不可靠 ⇒ 必须 LC_ALL=C；
  # 首选 `grep -a -o`，失败再退回 `strings` 管道。
  local f="$1" out=""
  out="$(LC_ALL=C grep -a -o -m1 "${STAMP_MARK}[0-9][0-9]*" "$f" 2>/dev/null | head -1 || true)"
  if [ -z "$out" ] && command -v strings >/dev/null 2>&1; then
    out="$(LC_ALL=C strings -a "$f" 2>/dev/null \
           | LC_ALL=C grep -o -m1 "${STAMP_MARK}[0-9][0-9]*" 2>/dev/null | head -1 || true)"
  fi
  printf '%s' "$out"
}

MAGIC="$(hex_head4 "$SO")"
case "$MAGIC" in
  7f454c46|7F454C46) : ;;
  *) die "不是 ELF 文件（前 4 字节 = ${MAGIC:-读不到}）: $SO" ;;
esac

MD5="$(md5_of "$SO")"
LOCAL_SIZE="$(size_of_file "$SO")"
STAMP="$(stamp_scan "$SO")"

echo "== 本地 so =="
echo "  路径   : $SO"
echo "  字节数 : $LOCAL_SIZE"
echo "  md5    : ${MD5:-unknown}"
if [ -n "$STAMP" ]; then
  echo "  stamp=${STAMP};"
else
  echo "  ⚠ 未找到构建戳 ${STAMP_MARK}<n>（构建戳是仓库发布的必要门禁，OTA 守卫按它判新旧）；" >&2
  echo "    若本轮只是诊断桩，可继续；正式出包必须先修构建戳（jni/Includes/StarryStamp.h）。" >&2
fi

# ---------------- 路径推导（乐变热更层，全部走变量，便于复用到其它乐变游戏） ----------------
LEBIAN_ROOT="/data/data/${PKG}/lebian"
DST_DIR="${LEBIAN_ROOT}/${PKG}-1/lib/arm64"
DST="${DST_DIR}/${LIB_NAME}"
HASH8=""
if [ -n "$MD5" ]; then HASH8="${MD5:0:8}"; else HASH8="nohash$$"; fi
REMOTE_TMP="${DEV_TMP_DIR}/${REMOTE_PREFIX}${HASH8}.so"

echo "== 目标 =="
echo "  设备        : ${SERIAL}（可用 --serial 或 DEV_SERIAL 覆盖）"
echo "  包名        : $PKG"
echo "  乐变目录    : $LEBIAN_ROOT"
echo "  替换目标    : $DST"
echo "  设备暂存    : $REMOTE_TMP"

# 设备侧路径安全引用：转义 shell 特殊字符（结果保证不含单引号），
# 使整条命令能塞进 adb shell 的 "su -c '<cmd>'" 单参数形式。
devq() {
  local s="$1"
  case "$s" in
    *"'"*) die "设备路径含单引号，本脚本不支持: $s" ;;
  esac
  printf '%s' "$s" | sed 's/[^A-Za-z0-9_.\/:=@,+-]/\\&/g'
}

su_run() {
  # 整条 "su -c '<cmd>'" 作为**一个**参数交给 adb shell：
  # 多参数形式在新版 adb 会被逐参数重新加引号转义（把内层引号变成字面量从而跑不起来），
  # 单参数形式老/新 adb 都安全。
  adb -s "$SERIAL" shell "su -c '$1'"
}

su_ls() {
  # 只读探针。对 /data/data 下的文件，su 下只有 ls -l 这条路可靠（md5sum/cat 会 Permission denied）。
  # 失败时返回空串而不是退出（调用方按空串判定不存在）。
  adb -s "$SERIAL" shell "su -c 'ls -l $(devq "$1")'" 2>/dev/null | tr -d '\r' || true
}

ls_size() {
  # 从 ls -l 输出取尺寸字段（字段序: 权限 链接数 owner group 尺寸 …），非数字则视为空。
  printf '%s\n' "$1" | awk 'NF >= 5 && $5 ~ /^[0-9]+$/ { print $5; exit }'
}

# ---------------- dry-run：只打印将执行的命令，不碰设备 ----------------
if [ "$DRY_RUN" = "1" ]; then
  cat <<EOF

== DRY-RUN（以下命令均未执行）==
  0) adb -s ${SERIAL} get-state
  1) adb -s ${SERIAL} shell "su -c 'ls -l ${LEBIAN_ROOT}'"        # 缺失 ⇒ 打印「负载未回填，先冷启游戏 ~60s」并退出 1
  2) adb -s ${SERIAL} push ${SO} ${REMOTE_TMP}
  3) adb -s ${SERIAL} shell "su -c 'cp ${REMOTE_TMP} ${DST}'"     # 覆盖 lebian 那份（可写）
  4) adb -s ${SERIAL} shell "su -c 'ls -l ${DST}'"                # 取尺寸核验（不用 md5sum）
  5) 尺寸一致 ⇒ 打印 SWAP_OK size=<n> md5=${MD5:-unknown} dst=${DST}; exit 0
EOF
  echo
  echo "[dry-run] 未连接设备、未做任何改动。"
  exit 0
fi

command -v adb >/dev/null 2>&1 || die "本机找不到 adb（PATH 里没有）"
adb -s "$SERIAL" get-state >/dev/null 2>&1 || die "设备 $SERIAL 不可达（adb devices 看一眼；被别的会话占用/掉线都可能在）"

# ---------------- 1. 乐变负载必须先回填 ----------------
if [ -z "$(su_ls "$LEBIAN_ROOT")" ]; then
  cat >&2 <<EOF
✗ FAIL  乐变负载目录不存在或读不到: ${LEBIAN_ROOT}
        ⇒ 负载未回填：先冷启游戏、等约 60s 让乐变派生解包，再重跑本脚本。
        排查: adb -s ${SERIAL} shell "su -c 'ls -l /data/data/${PKG}/lebian'"
        （若只有 resourceInfo.cfg 与几个 zip：那是覆盖安装把派生负载搞丢了，
          本游戏硬约束是必须卸载重装——见 10_docs/无悔华夏/硬约束.md。）
EOF
  exit 1
fi

# ---------------- 2. push 到设备暂存 ----------------
echo "→ adb -s ${SERIAL} push ${SO} ${REMOTE_TMP}"
adb -s "$SERIAL" push "$SO" "$REMOTE_TMP" || die "push 失败: $SO → $REMOTE_TMP"

# ---------------- 3. 覆盖 lebian 副本 ----------------
# cp 覆盖已存在的目标文件：toybox cp 走 O_TRUNC 就地写，目标原有 mode/owner 保持不变，
# 因此不需要 chmod/chcon（若目标不存在会新建，届时权限可能不对，输出里会以尺寸不符暴露）。
echo "→ su -c 'cp ${REMOTE_TMP} ${DST}'"
su_run "cp $(devq "$REMOTE_TMP") $(devq "$DST")" || die "su cp 失败: ${REMOTE_TMP} → ${DST}"

# ---------------- 4. 尺寸核验（不用 md5sum，理由见文件头） ----------------
LS_DST="$(su_ls "$DST")"
SZ_DST="$(ls_size "$LS_DST")"

if [ -n "$SZ_DST" ] && [ "$SZ_DST" = "$LOCAL_SIZE" ]; then
  echo "SWAP_OK size=${SZ_DST} md5=${MD5:-unknown} dst=${DST}"
  cat <<EOF

下一步（本脚本**不执行**，只打印，请按需自己跑）:
  1) adb -s ${SERIAL} shell 'su -c "logcat -G 16M; logcat -c"'
  2) adb -s ${SERIAL} shell am force-stop ${PKG}
  3) adb -s ${SERIAL} shell monkey -p ${PKG} -c android.intent.category.LAUNCHER 1
  4) sleep 95
  5) 取日志: adb -s ${SERIAL} logcat -d > <日志文件>

残留（设备暂存 so，可选清理）:
  adb -s ${SERIAL} shell "su -c 'rm -f ${REMOTE_TMP}'"
EOF
  exit 0
fi

echo "SWAP_FAIL size_dst=${SZ_DST:-未知} size_local=${LOCAL_SIZE} dst=${DST}" >&2
echo "  · size_dst 空 ⇒ 目标读不到（su 不通 / 路径不对 / 负载又被热更换掉了）" >&2
echo "  · 两侧都非空但不等 ⇒ 写入被截断或写到了别处，重跑一次不行就人工看一眼" >&2
exit 1
