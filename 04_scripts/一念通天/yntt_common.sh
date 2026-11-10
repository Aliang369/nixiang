#!/usr/bin/env bash
# 一念通天（com.qinglv.Immortal）阶段脚本公共环境 —— 被 source，不单独执行。
#
# 设计口径（2026-11-11 重写，替换 728/739/747/759/768/779 的每版本复制件）：
#   * 版本号唯一来源 = official_apk.json（pull_official 阶段产物），脚本里**不再硬编码版本**；
#   * 根目录由 BASH_SOURCE 相对推算，**不硬编码用户主目录**；
#   * bash 3.2 兼容（mac 侧 bash 3.2.57）：不用 mapfile/readarray/declare -A/local -n/${v^^}/;;&/&>>；
#   * 变量后紧跟中文一律写 ${VAR}（避免 bash 把中文并进变量名 ⇒ unbound variable）；
#   * 改动本文件后必跑：bash 04_scripts/_common/preflight.sh 04_scripts/一念通天/yntt_common.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
export NIXIANG_ROOT

# shellcheck source=/dev/null
source "${NIXIANG_ROOT}/04_scripts/_common/env.sh"

GAME="一念通天"
PKG="com.qinglv.Immortal"
GAME_ID="yntt"
META_FILE="${SCRIPT_DIR}/official_apk.json"

yntt_log() { printf '[yntt] %s\n' "$*"; }

# 退出并报错。注意：在 $( ) 子命令里调用只退出子 shell，调用点必须用 || 承接。
yntt_die() { printf '[yntt][ERROR] %s\n' "$*" >&2; exit 1; }

# 从 official_apk.json 取一个字段（字符串原样输出）
yntt_meta() {
  python3 - "${META_FILE}" "$1" <<'PY'
import json
import sys

path, key = sys.argv[1], sys.argv[2]
with open(path, encoding="utf-8") as f:
    data = json.load(f)
if key not in data:
    raise SystemExit("official_apk.json missing key: " + key)
sys.stdout.write(str(data[key]))
PY
}

# 单文件必须存在且非空
yntt_need_file() {
  [ -f "$1" ] || yntt_die "缺少文件: $1"
  [ -s "$1" ] || yntt_die "文件为空: $1"
}

yntt_need_exec() {
  [ -x "$1" ] || yntt_die "缺少可执行文件: $1"
}

yntt_md5() { md5 -q "$1"; }
yntt_sha256() { shasum -a 256 "$1" | awk '{print $1}'; }
yntt_size() { stat -f '%z' "$1"; }

# 解析 dotnet：PATH 优先，其次 ~/.dotnet（mac 常见手装位置）
yntt_dotnet() {
  local cand
  if command -v dotnet >/dev/null 2>&1; then
    command -v dotnet
    return 0
  fi
  for cand in "${HOME}/.dotnet/dotnet" "/usr/local/share/dotnet/dotnet" "/opt/homebrew/bin/dotnet"; do
    if [ -x "${cand}" ]; then
      printf '%s' "${cand}"
      return 0
    fi
  done
  return 1
}

# --- 版本与路径（依赖 official_apk.json）---
[ -f "${META_FILE}" ] || yntt_die "缺少 ${META_FILE}：先跑 pull_official 阶段（fetch_installed_official_yntt.sh）"
YNTT_VER="${YNTT_VER:-$(yntt_meta version_name)}"
export YNTT_VER
case "${YNTT_VER}" in
  ''|*[!0-9A-Za-z._-]*) yntt_die "official_apk.json 的 version_name 非法: ${YNTT_VER}" ;;
esac

ORIG_APK="${NIXIANG_ROOT}/02_apks/${GAME}/${GAME}_${YNTT_VER}_official_from_device.apk"
ORIG_MD5="$(yntt_meta md5)"
ORIG_SHA256="$(yntt_meta sha256)"
OUT_DIR="${NIXIANG_ROOT}/08_output/${GAME}"
DUMP_DIR="${NIXIANG_ROOT}/06_dumped/${GAME}/official_${YNTT_VER}"
export ORIG_APK ORIG_MD5 YNTT_VER OUT_DIR DUMP_DIR
