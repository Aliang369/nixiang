#!/usr/bin/env bash
# 山海幻境：解密官方底包内的 Cocos JSC（XXTEA(padding=False)+gzip）。
#
# ★ 2026-10-15 起为**薄封装**：实现已升格为公共工具
#   04_scripts/_common/cocos_jsc_decrypt.sh（换版本/换游戏零重写；见复盘 P0-1）。
# 本文件只负责「山海幻境的默认值」：底包取自 official_apk.json、输出落 06_dumped/山海幻境/official_<ver>/jsc_decrypted。
#
# 用法:
#   bash 04_scripts/山海幻境/decrypt_jsc_shhj.sh
#   SHHJ_JS_OUT=/tmp/xx bash 04_scripts/山海幻境/decrypt_jsc_shhj.sh   # 覆盖输出目录（测试用）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
# 4.0 / 4.2 / 4.3 共用同一支 libcocos2djs.so（md5 09e19fcd…），故 XXTEA key 不变
KEY="376f2502-1ead-46"
COMMON="$NIXIANG_ROOT/04_scripts/_common/cocos_jsc_decrypt.sh"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
[[ -f "$COMMON" ]] || { echo "错误: 缺少公共工具 $COMMON" >&2; exit 1; }
read -r APK VERSION_NAME < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
print(d["apk_path"], d["version_name"])
PY
)
OUT="${SHHJ_JS_OUT:-$NIXIANG_ROOT/06_dumped/山海幻境/official_${VERSION_NAME}/jsc_decrypted}"

exec bash "$COMMON" --apk "$APK" --key "$KEY" --out "$OUT"
