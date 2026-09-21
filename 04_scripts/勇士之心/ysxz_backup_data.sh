#!/usr/bin/env bash
# 勇士之心：安装星光包前的真机数据只读备份（不含 cache/code_cache/app_webview）。
# 目的：官方包与星光包签名不同，无法覆盖安装；如需卸载重装，先用本脚本保留
# 账号/登录偏好、jsb.sqlite 与 831MB 热更资源，装完用 ysxz_restore_data.sh 还原。
#
# 用法：TAG=20260921 bash ysxz_backup_data.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.fightingstars.games.warrior}"
TAG="${TAG:-20260921}"
ADB_BIN="${ADB_BIN:-adb}"
STATE_FILE="$SCRIPT_DIR/.device_serial"
OUT="$NIXIANG/09_test/勇士之心/backup_$TAG"
REMOTE_DIR="/data/data/$PKG"
REMOTE_TAR="/data/local/tmp/ysxz_backup_$TAG.tar"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

mkdir -p "$OUT"
echo "[1/4] 记录当前安装指纹"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'dumpsys package $PKG | grep -E \"versionName|versionCode|codePath|lastUpdateTime\"'" \
  | tr -d '\r' > "$OUT/device_package.txt"

echo "[2/4] 设备端打包（排除 cache / code_cache / app_webview）"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'cd \"$REMOTE_DIR\" && tar -cf \"$REMOTE_TAR\" --exclude=cache --exclude=code_cache --exclude=app_webview .'" >/dev/null
"$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -la \"$REMOTE_TAR\"'" | tr -d '\r' | tee "$OUT/remote_tar.txt"

echo "[3/4] 拉取备份到本地"
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$REMOTE_TAR\"'" > "$OUT/data_$TAG.tar"
[[ -s "$OUT/data_$TAG.tar" ]] || { echo "错误: 备份为空" >&2; exit 1; }
md5 -q "$OUT/data_$TAG.tar" > "$OUT/data_$TAG.tar.md5"

echo "[4/4] 校验并生成清单"
tar -tf "$OUT/data_$TAG.tar" > "$OUT/tar_listing.txt"
python3 - "$OUT" "$TAG" "$SERIAL" "$REMOTE_TAR" <<'PY'
import hashlib, json, sys
from pathlib import Path
out, tag, serial, remote_tar = sys.argv[1:]
out = Path(out)
tar_path = out / f"data_{tag}.tar"
h = hashlib.md5(tar_path.read_bytes()).hexdigest()
entries = [l for l in (out / "tar_listing.txt").read_text(encoding="utf-8", errors="replace").splitlines() if l]
key = [e for e in entries if ("shared_prefs" in e or "jsb.sqlite" in e or "bingo_guest_id" in e)]
data = {"package": "com.fightingstars.games.warrior", "serial": serial, "tag": tag,
        "remote_tar": remote_tar, "local_tar": str(tar_path.relative_to(out.parent.parent.parent)),
        "size": tar_path.stat().st_size, "md5": h, "entries": len(entries), "key_entries": key[:40]}
(out / "backup_manifest.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({k: data[k] for k in ("size", "md5", "entries")}, ensure_ascii=False))
PY
echo "backup_ok=$OUT/data_$TAG.tar"
echo "设备端备份副本保留在 $REMOTE_TAR（还原脚本默认用它，可省一次 push）"
