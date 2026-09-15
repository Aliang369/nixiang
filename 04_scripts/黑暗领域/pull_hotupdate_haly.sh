#!/usr/bin/env bash
# 黑暗领域：只读提取真机 files/gg-hot-update。仅拉取，不安装、不清数据、不启动游戏。
set -euo pipefail

trap 'rc=$?; echo "错误: pull_hotupdate_haly.sh 第 ${LINENO} 行失败 rc=${rc}" >&2' ERR

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="${PKG:-com.haly.tap}"
GAME="黑暗领域"
ADB_BIN="${ADB_BIN:-adb}"
OUT="$NIXIANG_ROOT/06_dumped/$GAME/hotupdate_current"
TARBALL="$NIXIANG_ROOT/06_dumped/$GAME/hotupdate_current.tar.gz"
META="$SCRIPT_DIR/hotupdate_meta.json"
REMOTE_TMP="/data/local/tmp/haly_hotupdate_current.tar.gz"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready 阶段" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

INDEX="$OUT/gg-hot-update/assets/main/index.js"
if [[ -f "$INDEX" ]]; then
  echo "已存在有效热更 dump，复用: $OUT"
else
  REMOTE_DIR="/data/data/$PKG/files/gg-hot-update"
  if ! "$ADB_BIN" -s "$SERIAL" shell "su -c 'test -d \"$REMOTE_DIR\"'"; then
    echo "错误: 真机不存在 $REMOTE_DIR，请确认游戏至少进入过一次并完成热更" >&2
    exit 1
  fi
  mkdir -p "$(dirname "$OUT")"
  rm -f "$TARBALL"
  trap "rm -f \"$TARBALL\"; \"$ADB_BIN\" -s \"$SERIAL\" shell \"su -c 'rm -f $REMOTE_TMP'\" >/dev/null 2>&1 || true" EXIT
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'tar -C \"$REMOTE_DIR/..\" -czf \"$REMOTE_TMP\" gg-hot-update'"
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TARBALL"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f \"$REMOTE_TMP\"'" || true
  mkdir -p "$OUT"
  tar -xzf "$TARBALL" -C "$OUT"
fi

[[ -f "$INDEX" ]] || { echo "错误: 热更主脚本缺失: $INDEX" >&2; exit 1; }
SIZE="$(stat -f '%z' "$INDEX")"
MD5="$(md5 -q "$INDEX")"
SHA256="$(shasum -a 256 "$INDEX" | awk '{print $1}')"
TAR_SHA256=""
[[ -f "$TARBALL" ]] && TAR_SHA256="$(shasum -a 256 "$TARBALL" | awk '{print $1}')"

python3 - "$META" "$OUT" "$INDEX" "$SERIAL" "$PKG" "$SIZE" "$MD5" "$SHA256" "$TAR_SHA256" <<'PY'
import json
import sys
from pathlib import Path

path, out, index, serial, package, size, md5, sha256, tar_sha256 = sys.argv[1:]
data = {
    "game": "黑暗领域",
    "package": package,
    "serial": serial,
    "dump_dir": out,
    "index_path": index,
    "index_size": int(size),
    "index_md5": md5,
    "index_sha256": sha256,
    "tarball_sha256": tar_sha256 or None,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

printf 'dump=%s\nindex=%s\nsize=%s\nmd5=%s\nsha256=%s\nmeta=%s\n' \
  "$OUT" "$INDEX" "$SIZE" "$MD5" "$SHA256" "$META"
