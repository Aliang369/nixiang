#!/usr/bin/env bash
# ================================================================
#  probe_runtime_readonly.sh — 目标进程运行时【只读】探针（单次，不轮询）
#
#  场景：动物大联盟 1.0.25（com.fightingstars.games.animal，TapTap 原版，APKSecure 壳）
#        环境阶段取证第 1 步：确认进程在跑 + 拿 /proc/<pid>/maps 里
#        **映射到真实文件路径**的代码（ELF / DEX）清单与映射段。
#
#  只回答三件事，拿到就停：
#    ① 目标 pid 是否存在（pidof，一次）
#    ② maps 里哪些**有路径的代码文件**被映射（.so/.dex/.odex/.vdex/.oat）
#    ③ 这些路径在 su 下能否 stat（尺寸/mtime），不做内容读取
#
#  ★ 严禁（本脚本结构性保证）：
#    · 不启动 / 不 force-stop / 不装机 / 不卸载 / 不清数据
#    · 不 screencap / 不 input / 不 monkey（零屏幕交互）
#    · 不遍历进程内存 / 不 dump 匿名 heap / 不读 /proc/<pid>/mem
#    · 不读 shared_prefs / cache / files / databases 里的个人内容
#    · 不注入 Frida、不加载任何服务、不改应用数据
#    · 不轮询：单次执行；目标不在就 exit 3 交回上层，不 sleep 重试
#
#  用法:
#    bash 04_scripts/动物大联盟/runtime_readonly/probe_runtime_readonly.sh [--serial f9a116a2] [--pkg PKG] [--out DIR]
#
#  退出码: 0=已取得 maps 摘要 | 3=目标未运行（NOT_RUNNING，待后续交互）
#          1=设备/环境错误 | 2=用法错
#  bash 3.2 兼容（不用 mapfile / 关联数组）
# ================================================================
set -euo pipefail

SERIAL="${DEV_SERIAL:-f9a116a2}"
PKG="com.fightingstars.games.animal"
OUT=""

usage() {
  awk 'NR>1 { if ($0 ~ /^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"
}
die() { echo "✗ FAIL  $*" >&2; exit 1; }
bad() { echo "✗ 用法  $*" >&2; exit 2; }

while [ $# -gt 0 ]; do
  case "$1" in
    --serial) [ $# -ge 2 ] || bad "--serial 缺值"; SERIAL="$2"; shift 2 ;;
    --pkg)    [ $# -ge 2 ] || bad "--pkg 缺值";    PKG="$2";    shift 2 ;;
    --out)    [ $# -ge 2 ] || bad "--out 缺值";    OUT="$2";    shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) bad "未知参数 $1" ;;
  esac
done

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SELF_DIR/../../.." && pwd)"
[ -n "$OUT" ] || OUT="$ROOT_DIR/06_dumped/动物大联盟/env_1.0.25/probe"
mkdir -p "$OUT"

ADB="${ADB:-adb}"
command -v "$ADB" >/dev/null 2>&1 || die "找不到 adb（PATH 里没有）"

# ---------- ① 设备在位（一次，不轮询） ----------
DEV_STATE="$("$ADB" -s "$SERIAL" get-state 2>&1 | tr -d "\r" | head -1)"
[ "$DEV_STATE" = "device" ] || die "设备 $SERIAL 状态=${DEV_STATE:-<空>}（非 device）"
echo "DEVICE $SERIAL state=device"

# ---------- ② pid（单次 pidof） ----------
PID="$("$ADB" -s "$SERIAL" shell "pidof $PKG" 2>/dev/null | tr -d "\r" | awk "{print \$1}")"

if [ -z "$PID" ]; then
  {
    echo "STATUS=NOT_RUNNING"
    echo "PKG=$PKG"
    echo "SERIAL=$SERIAL"
    echo "NOTE=目标进程当前未运行；未启动、未 force-stop、未轮询。"
    echo "NEXT=待后续交互阶段（用户人工进入游戏后）再执行本脚本一次。"
  } > "$OUT/probe_summary.txt"
  echo "STATUS=NOT_RUNNING pkg=${PKG}（当前未运行，待后续交互）"
  exit 3
fi
echo "PID $PID"

# ---------- ③ 只读取 maps / exe / cmdline ----------
"$ADB" -s "$SERIAL" shell "su -c 'cat /proc/"$PID"/maps'" 2>/dev/null | tr -d "\r" > "$OUT/maps.txt"
MAPS_LINES=$(wc -l < "$OUT/maps.txt" | tr -d " ")
[ "$MAPS_LINES" -gt 0 ] || die "maps 读取为空（su 是否可用？）"

"$ADB" -s "$SERIAL" shell "su -c 'readlink /proc/"$PID"/exe'" 2>/dev/null | tr -d "\r" > "$OUT/exe.txt"
"$ADB" -s "$SERIAL" shell "su -c 'cat /proc/"$PID"/cmdline'" 2>/dev/null | tr -d "\r" | tr "\000" " " > "$OUT/cmdline.txt"

# ---------- ④ 只挑「有路径的代码文件」 ----------
# maps 字段: range perms offset dev inode [path]
awk '{ p=$6; if (p=="") next;
       if (p !~ /\.(so|dex|odex|vdex|oat)$/) next;
       print $1"\t"$2"\t"$3"\t"$5"\t"p }' "$OUT/maps.txt" > "$OUT/segments.tsv"

awk -F"\t" '!seen[$5]++ { print $5 }' "$OUT/segments.tsv" > "$OUT/paths.uniq.txt"
NPATH=$(wc -l < "$OUT/paths.uniq.txt" | tr -d " ")

printf "path\tsegments\tperms\tsize_bytes\tmtime\tstat_as_root\n" > "$OUT/code_paths.tsv"

if [ "$NPATH" -gt 0 ]; then
  PATHS_JOINED="$(tr "\n" " " < "$OUT/paths.uniq.txt")"
  "$ADB" -s "$SERIAL" shell "su -c 'ls -ld $PATHS_JOINED'" 2>/dev/null | tr -d "\r" > "$OUT/device_stat.txt" || true
  while IFS= read -r p; do
    [ -n "$p" ] || continue
    nseg=$(awk -F"\t" -v p="$p" '$5==p' "$OUT/segments.tsv" | wc -l | tr -d " ")
    perms=$(awk -F"\t" -v p="$p" '$5==p {printf "%s,", $2}' "$OUT/segments.tsv" | sed "s/,$//")
    statline=$(grep -F "$p" "$OUT/device_stat.txt" | head -1 || true)
    size=$(printf "%s" "$statline" | awk '{print $5}')
    mtime=$(printf "%s" "$statline" | awk '{print $6" "$7" "$8}')
    if [ -n "$statline" ]; then st="yes"; else st="no"; fi
    printf "%s\t%s\t%s\t%s\t%s\t%s\n" "$p" "$nseg" "$perms" "${size:-?}" "${mtime:-?}" "$st" >> "$OUT/code_paths.tsv"
  done < "$OUT/paths.uniq.txt"
fi

# ---------- ⑤ 摘要（只打命中行，≤8KB） ----------
{
  echo "STATUS=MAPS_OK"
  echo "SERIAL=$SERIAL"
  echo "PKG=$PKG"
  echo "PID=$PID"
  echo "MAPS_LINES=$MAPS_LINES"
  echo "CODE_PATHS=$NPATH"
  echo "GENERATED=$(date '+%Y-%m-%d %H:%M:%S %z')"
} > "$OUT/probe_summary.txt"

echo "---- 关注映射（带行号便于回查 maps.txt）----"
grep -n "libsecret\.so\|libcocos2djs\.so\|\.dex$\|\.odex$\|\.vdex$" "$OUT/maps.txt" | head -40 || true
echo "---- code_paths.tsv ----"
head -30 "$OUT/code_paths.tsv"
echo "SUMMARY $OUT/probe_summary.txt"
exit 0
