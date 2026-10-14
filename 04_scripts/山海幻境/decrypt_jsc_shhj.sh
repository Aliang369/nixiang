#!/usr/bin/env bash
# 山海幻境：把官方底包内的 Cocos Creator JSC 解密为可分析 JS（XXTEA(padding=False) + gzip）。
# 只读 02_apks 内官方包，产物落 06_dumped/山海幻境/official_<ver>/jsc_decrypted（可再生，不入库）。
#
# 锚点核对用法：
#   bash 04_scripts/山海幻境/decrypt_jsc_shhj.sh
#   python3.11 04_scripts/山海幻境/check_shhj_js_anchors.py <decrypted>/assets/main/index.js
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
# 4.0 / 4.2 / 4.3 共用同一支 libcocos2djs.so（md5 09e19fcd…），故 XXTEA key 不变
KEY="376f2502-1ead-46"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
read -r APK VERSION_NAME < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
print(d["apk_path"], d["version_name"])
PY
)
OUT_ROOT="$NIXIANG_ROOT/06_dumped/山海幻境/official_${VERSION_NAME}/jsc_decrypted"

[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
"$PYTHON_BIN" -c 'import xxtea, zlib' 2>/dev/null || {
  echo "错误: $PYTHON_BIN 缺少 xxtea 模块" >&2
  exit 1
}
if [[ -e "$OUT_ROOT" ]]; then
  echo "错误: 输出目录已存在，先移走: $OUT_ROOT" >&2
  exit 1
fi

mkdir -p "$OUT_ROOT"
"$PYTHON_BIN" - "$APK" "$OUT_ROOT" "$KEY" <<'PY'
import hashlib
import json
import sys
import zipfile
import zlib
from pathlib import Path

import xxtea

apk, out_root, key = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3].encode("ascii")
records, errors = [], []

with zipfile.ZipFile(apk) as z:
    for name in sorted(n for n in z.namelist() if n.endswith(".jsc")):
        parts = list(Path(name).parts)
        if parts and parts[0] == "assets":
            parts = parts[1:]
        dst = out_root / Path(*parts).with_suffix(".js")
        dst.parent.mkdir(parents=True, exist_ok=True)
        enc = z.read(name)
        try:
            packed = xxtea.decrypt(enc, key, padding=False)
            if not packed:
                raise ValueError("xxtea returned empty")
            dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
            js = dec.decompress(packed) + dec.flush()
            if not dec.eof:
                raise ValueError("gzip stream did not terminate")
            if not js:
                raise ValueError("empty JS")
            dst.write_bytes(js)
            records.append({
                "apk_entry": name,
                "js": str(dst),
                "encrypted_size": len(enc),
                "xxtea_size": len(packed),
                "decrypted_size": len(js),
                "sha256": hashlib.sha256(js).hexdigest(),
            })
        except Exception as exc:  # noqa: BLE001
            errors.append({"apk_entry": name, "error": repr(exc)})

summary = {
    "apk": str(apk),
    "key": key.decode("ascii"),
    "algorithm": "XXTEA(padding=False) + gzip",
    "decrypted_count": len(records),
    "error_count": len(errors),
    "records": records,
    "errors": errors,
}
(out_root / "decrypt_summary.json").write_text(
    json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)
for r in records:
    print(f"  {r['apk_entry']}: {r['encrypted_size']} → xxtea {r['xxtea_size']} → js {r['decrypted_size']}")
print(f"ok={len(records)} errors={len(errors)} summary={out_root}/decrypt_summary.json")
if errors:
    for e in errors:
        print(f"  ERR {e['apk_entry']}: {e['error']}", file=sys.stderr)
    raise SystemExit(3)
PY
echo "DONE: $OUT_ROOT"
