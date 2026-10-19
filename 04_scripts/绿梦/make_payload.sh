#!/usr/bin/env bash
# ============================================================================
#  make_payload.sh — 组装「投递给助手 App」的补丁载荷
#
#  载荷三件（助手 Deployer 从 assets/ 读出后写进游戏目录）：
#    __data           重打包后的热更 Dll bundle（原始索引 0 块原样保留）
#    __info           YooAsset 缓存元数据（新大小 + 新 CRC，18 B）
#    starry_lvmeng.dll 独立 mod 程序集（游戏侧从状态文件同目录 Assembly.Load）
#
#  输入:
#    --patched <Assembly-ACT.patched.dll>   默认 07_patched/绿梦/Assembly-ACT.patched.dll
#    --mod     <StarryLvmeng.dll>           默认 04_scripts/绿梦/mod/obj/Release/StarryLvmeng.dll
#    --orig    <官方 __data>                 默认 /tmp/lvmeng/dll/__data；
#                                            找不到时从设备备份 /data/local/tmp/lvmeng_backup_*/__data 拉
#  输出: 07_patched/绿梦/payload/{__data,__info,starry_lvmeng.dll}
#
#  ⚠ 必须用**官方原包**当底：bundle 里索引 0 那块不可解（见 侦察笔记 §3），必须原样搬运。
# ============================================================================
set -u

SELF_DIR="$(cd "$(dirname "$0")" && pwd -P)"
ROOT="${NIXIANG_ROOT:-$(cd "$SELF_DIR/../.." && pwd -P)}"
PY="${PYTHON:-/opt/homebrew/bin/python3.11}"
SERIAL="${ANDROID_SERIAL:-f9a116a2}"

PATCHED="$ROOT/07_patched/绿梦/Assembly-ACT.patched.dll"
MODDLL="$ROOT/04_scripts/绿梦/mod/obj/Release/StarryLvmeng.dll"
ORIG="/tmp/lvmeng/dll/__data"
PAYLOAD="$ROOT/07_patched/绿梦/payload"
ACT_OFF=649120
ACT_SIZE=14529536

while [ $# -gt 0 ]; do
  case "$1" in
    --patched) PATCHED="${2:-}"; shift 2 ;;
    --mod)     MODDLL="${2:-}"; shift 2 ;;
    --orig)    ORIG="${2:-}"; shift 2 ;;
    --out)     PAYLOAD="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,22p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

die() { echo "ERROR: $*" >&2; exit 1; }

[ -f "$PATCHED" ] || die "找不到补丁 dll: $PATCHED"
[ -f "$MODDLL" ]  || die "找不到 mod dll: ${MODDLL}（先 cd 04_scripts/绿梦/mod && dotnet build -c Release）"

# 官方原包：本地优先，其次设备备份
if [ ! -f "$ORIG" ]; then
  echo "[orig] 本地没有 ${ORIG}，尝试从设备备份拉取…"
  BK="$(adb -s "$SERIAL" shell "su -c 'ls -1d /data/local/tmp/lvmeng_backup_* 2>/dev/null | tail -1'" 2>/dev/null | tr -d '\r')"
  [ -n "$BK" ] || die "设备上也没有备份；请先确认官方 bundle 来源（见 侦察笔记 §4）"
  mkdir -p "$(dirname "$ORIG")"
  adb -s "$SERIAL" shell "su -c 'cat $BK/__data'" > "$ORIG" || die "从设备拉官方 bundle 失败"
fi

PSIZE="$(stat -f%z "$PATCHED" 2>/dev/null || stat -c %s "$PATCHED")"
[ "$PSIZE" = "$ACT_SIZE" ] || die "补丁 dll 尺寸 ${PSIZE} != ${ACT_SIZE}（必须等长，否则对象表失配）"

mkdir -p "$PAYLOAD"
echo "== 1/4 重打包 bundle =="
"$PY" "$SELF_DIR/bundle_repack.py" replace "$ORIG" "$PATCHED" "$PAYLOAD/__data" \
  --offset "$ACT_OFF" --size "$ACT_SIZE" || die "bundle 重打包失败"

echo "== 2/4 写 __info =="
"$PY" "$SELF_DIR/bundle_repack.py" write-info "$PAYLOAD/__data" "$PAYLOAD/__info" || die "写 __info 失败"

echo "== 3/4 放 mod dll =="
cp -f "$MODDLL" "$PAYLOAD/starry_lvmeng.dll"

echo "== 4/4 用落地脚本的闸门自检 =="
bash "$SELF_DIR/install_dll_bundle.sh" --bundle "$PAYLOAD/__data" --info "$PAYLOAD/__info" \
  --serial "$SERIAL" --dry-run 2>&1 | tail -8

echo "== 载荷 =="
for f in __data __info starry_lvmeng.dll; do
  printf "   %-20s %10s B  md5 %s\n" "$f" \
    "$(stat -f%z "$PAYLOAD/$f" 2>/dev/null || stat -c %s "$PAYLOAD/$f")" \
    "$(md5 -q "$PAYLOAD/$f" 2>/dev/null || md5sum "$PAYLOAD/$f" | cut -d' ' -f1)"
done
echo "✅ 载荷就绪 → $PAYLOAD"
