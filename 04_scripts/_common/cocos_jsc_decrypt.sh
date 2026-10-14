#!/usr/bin/env bash
# ================================================================
#  cocos_jsc_decrypt.sh — Cocos Creator JSC 通用解密（XXTEA(padding=False) + gzip）
#
#  为什么有它（2026-10-15 山海幻境 4.3 复盘）：
#    4.0/4.2 两轮各写一份「本轮临时 XXTEA 探针脚本」，收尾清理时被删；4.3 又从零写一遍。
#    JSC 解法与游戏无关（只有 key 与 entry 路径不同）⇒ 升格为公共工具，换版本/换游戏零重写。
#
#  用法:
#    bash 04_scripts/_common/cocos_jsc_decrypt.sh --apk <apk> --key <xxtea-key> --out <目录>
#        [--entry assets/assets/main/index.jsc]   # 只解主包（默认行为，锚点核对只要它）
#        [--all]                                  # 解全部 *.jsc（分析用，产物大）
#  产物: <out>/<entry 去掉 assets/ 前缀、后缀改 .js>  + <out>/decrypt_summary.json
#  退出码: 0=OK；2=用法错；3=某条解密失败；4=--out 已存在（防覆盖）
#
#  key 从哪来：`libcocos2djs.so` 内 `jsb_set_xxtea_key` 的参数字面量（同一支 so 换版本 key 不变）
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
APK="" KEY="" OUT="" ENTRY="assets/assets/main/index.jsc" ALL=0
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

usage() {
  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//' >&2
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apk)   APK="${2:-}"; shift 2 ;;
    --key)   KEY="${2:-}"; shift 2 ;;
    --out)   OUT="${2:-}"; shift 2 ;;
    --entry) ENTRY="${2:-}"; shift 2 ;;
    --all)   ALL=1; shift ;;
    -h|--help) usage ;;
    *) echo "错误: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$APK" && -n "$KEY" && -n "$OUT" ]] || usage
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 2; }
[[ -x "$PYTHON_BIN" ]] || PYTHON_BIN="$(command -v python3)"
"$PYTHON_BIN" -c 'import xxtea, zlib' 2>/dev/null || {
  echo "错误: $PYTHON_BIN 缺少 xxtea 模块（pip install xxtea）" >&2
  exit 2
}
[[ ! -e "$OUT" ]] || { echo "错误: 输出目录已存在，先移走: $OUT" >&2; exit 4; }
mkdir -p "$OUT"

"$PYTHON_BIN" - "$APK" "$OUT" "$KEY" "$ENTRY" "$ALL" <<'PY'
import hashlib, json, sys, zipfile, zlib
from pathlib import Path
import xxtea

apk, out_root, key, entry, all_flag = (Path(sys.argv[1]), Path(sys.argv[2]),
                                       sys.argv[3].encode("ascii"), sys.argv[4], sys.argv[5] == "1")
records, errors = [], []
with zipfile.ZipFile(apk) as z:
    names = [n for n in z.namelist() if n.endswith(".jsc")]
    if not all_flag:
        if entry not in names:
            raise SystemExit(f"ERROR: 包内没有 {entry}（可用条目: {[n for n in names][:6]}…）")
        names = [entry]
    for name in sorted(names):
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
            records.append({"apk_entry": name, "js": str(dst), "encrypted_size": len(enc),
                            "decrypted_size": len(js), "sha256": hashlib.sha256(js).hexdigest()})
            print(f"  {name}: {len(enc)} → {len(js)} B")
        except Exception as exc:  # noqa: BLE001
            errors.append({"apk_entry": name, "error": repr(exc)})
summary = {"apk": str(apk), "key": key.decode("ascii"), "algorithm": "XXTEA(padding=False) + gzip",
           "decrypted_count": len(records), "error_count": len(errors),
           "records": records, "errors": errors}
(out_root / "decrypt_summary.json").write_text(
    json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"ok={len(records)} errors={len(errors)} summary={out_root}/decrypt_summary.json")
if errors:
    for e in errors:
        print(f"  ERR {e['apk_entry']}: {e['error']}", file=sys.stderr)
    raise SystemExit(3)
PY
echo "DONE: $OUT"
