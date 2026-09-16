#!/usr/bin/env bash
# 山海幻境 4.0：定向拆包，提取 classes.dex、Manifest 与关键 smali 触点。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
OUT_ROOT="$NIXIANG_ROOT/06_dumped/山海幻境/official_4"
MANIFEST_ROOT="$NIXIANG_ROOT/07_patched/山海幻境/surgical_4"
SMALI_BIN="$NIXIANG_ROOT/01_tools/smali/baksmali"
APKTOOL_BIN="${APKTOOL_BIN:-apktool}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
APK="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])
PY
)"
VERSION_NAME="$(python3 - "$META_FILE" <<'PY'
import json
import sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["version_name"])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "错误: 缺少 baksmali: $SMALI_BIN" >&2; exit 1; }
command -v "$APKTOOL_BIN" >/dev/null || { echo "错误: apktool 不可用" >&2; exit 1; }

rm -rf "$OUT_ROOT"
mkdir -p "$OUT_ROOT" "$MANIFEST_ROOT"

unzip -p "$APK" classes.dex > "$OUT_ROOT/classes.dex"
[[ -s "$OUT_ROOT/classes.dex" ]] || { echo "错误: classes.dex 提取失败" >&2; exit 1; }
"$SMALI_BIN" d -o "$OUT_ROOT/smali" "$OUT_ROOT/classes.dex"

"$APKTOOL_BIN" d -f -s -o "$OUT_ROOT/apktool_manifest" "$APK" >/dev/null
cp -f "$OUT_ROOT/apktool_manifest/AndroidManifest.xml" "$MANIFEST_ROOT/AndroidManifest.xml"

python3 - "$OUT_ROOT" "$APK" "$VERSION_NAME" <<'PY'
import json
import sys
from pathlib import Path

out = Path(sys.argv[1])
apk = sys.argv[2]
version = sys.argv[3]
classes = {
    "game_application": out / "smali/com/chx/shhj/GameApplication.smali",
    "app_activity": out / "smali/com/chx/shhj/AppActivity.smali",
    "tap_sign_kit": out / "smali/com/taptap/sdk/kit/internal/utils/TapSignKit.smali",
    "client_login_utils": out / "smali/com/taptap/sdk/login/internal/util/ClientLoginUtils.smali",
    "client_login_handler": out / "smali/com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler.smali",
    "cocos_activity": out / "smali/org/cocos2dx/lib/Cocos2dxActivity.smali",
}
data = {
    "apk_path": apk,
    "version_name": version,
    "classes_dex_size": (out / "classes.dex").stat().st_size,
    "classes_present": {k: p.is_file() for k, p in classes.items()},
}
more = {}
app = classes["app_activity"]
if app.is_file():
    text = app.read_text(encoding="utf-8", errors="replace")
    more["app_activity_has_onResume"] = ".method protected onResume()V" in text
    more["app_activity_has_onPause"] = ".method protected onPause()V" in text
    more["app_activity_has_get_ret"] = ".method public static get_ret(I)Z" in text
    more["app_activity_super"] = next(
        (line.split()[-1] for line in text.splitlines() if line.startswith(".super ")),
        "",
    )
tap = classes["tap_sign_kit"]
if tap.is_file():
    text = tap.read_text(encoding="utf-8", errors="replace")
    more["tap_sign_getSign_context"] = "getSign(Landroid/content/Context;)Ljava/lang/String;" in text
clu = classes["client_login_utils"]
if clu.is_file():
    text = clu.read_text(encoding="utf-8", errors="replace")
    more["client_login_no_actions"] = "isClientLogin(Landroid/app/Activity;)Z" in text
    more["client_login_with_actions"] = "isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z" in text
data["touchpoints"] = more
(out / "dump_summary.json").write_text(
    json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)
print(json.dumps(data, ensure_ascii=False, indent=2))
PY
