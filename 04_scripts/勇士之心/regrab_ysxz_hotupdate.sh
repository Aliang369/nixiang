#!/usr/bin/env bash
# 勇士之心：热更后只读重抓（设备 -> 本地）。
# 只做读取：拉 base.apk + files/res 下全部 *.js / version / .cocos-project.json。
# 不改包、不注入、不卸载、不清数据。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.fightingstars.games.warrior}"
TAG="${TAG:-20260921}"
ADB_BIN="${ADB_BIN:-adb}"
STATE_FILE="$SCRIPT_DIR/.device_serial"
OUT="$NIXIANG/09_test/勇士之心/regrab_$TAG"
REMOTE_ROOT="/data/data/$PKG/files"
REMOTE_RES="$REMOTE_ROOT/res"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

rm -rf "$OUT"
mkdir -p "$OUT"

echo "[1/5] 设备与安装信息"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'dumpsys package $PKG | grep -E \"versionName|versionCode|lastUpdateTime|firstInstallTime|codePath\"'" \
  | tr -d '\r' > "$OUT/device_package.txt"

echo "[2/5] 目录与 JS 清单"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'cd \"$REMOTE_RES\" && find . -type f -name \"*.js\" -exec ls -la {} \\;'" \
  | tr -d '\r' | sort > "$OUT/res_js_listing_remote.txt"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'cd \"$REMOTE_RES\" && find . -type f -name \"*.js\" -exec md5sum {} \\;'" \
  | tr -d '\r' | sort > "$OUT/res_js_md5_remote.txt"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -la \"$REMOTE_ROOT\"'" | tr -d '\r' > "$OUT/files_top_listing.txt"
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$REMOTE_RES/version\"'" > "$OUT/res_version.txt" || true
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$REMOTE_RES/.cocos-project.json\"'" > "$OUT/cocos-project.json" || true

echo "[3/5] tar 流式只读拉取 *.js"
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cd \"$REMOTE_RES\" && tar -cf - \$(find . -type f -name \"*.js\")'" > "$OUT/res_js.tar"
tar -xf "$OUT/res_js.tar" -C "$OUT"
rm -f "$OUT/res_js.tar"
JS_COUNT="$(find "$OUT" -type f -name '*.js' | wc -l | tr -d ' ')"

echo "[4/5] 只读拉取 base.apk"
APK_PATH="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'readlink -f /data/app/*/$PKG*/base.apk' | head -1" | tr -d '\r')"
[[ -n "$APK_PATH" ]] || { echo "错误: 未定位 base.apk" >&2; exit 1; }
echo "$APK_PATH" > "$OUT/base_apk_path.txt"
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$APK_PATH\"'" > "$OUT/base.apk"
[[ -s "$OUT/base.apk" ]] || { echo "错误: base.apk 拉取失败" >&2; exit 1; }

echo "[5/5] 生成清单"
python3 - "$OUT" "$PKG" "$SERIAL" "$JS_COUNT" "$APK_PATH" <<'PY'
import hashlib, json, sys
from pathlib import Path
out, pkg, serial, count, apk_path = sys.argv[1:]
out = Path(out)
def md5(p: Path) -> str:
    h = hashlib.md5()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()
files = []
for p in sorted(out.rglob("*.js")):
    files.append({"rel": str(p.relative_to(out)), "size": p.stat().st_size, "md5": md5(p),
                  "magic": p.read_bytes()[:5].decode("latin1", "replace")})
data = {
    "package": pkg, "serial": serial, "js_files": int(count),
    "base_apk_path": apk_path, "base_apk_md5": md5(out / "base.apk"),
    "base_apk_size": (out / "base.apk").stat().st_size, "files": files,
}
(out / "regrab_manifest.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"js_files": len(files), "encrypted_ze": sum(1 for f in files if f["magic"] == "*ze*"),
                  "base_apk_md5": data["base_apk_md5"], "base_apk_size": data["base_apk_size"]},
                 ensure_ascii=False, indent=2))
PY
echo "out=$OUT"
