#!/usr/bin/env bash
# 无悔华夏 4399 3.8.280：提取最小静态分析输入，并尝试对 HTPX header 做离线解析。
# 不处理 APK 内数万个 AssetBundle；只提取 IL2CPP、壳 dex、Manifest 和关键热更配置。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
FETCH_JSON="$SCRIPT_DIR/fetched_apk.json"
GAME="无悔华夏4399"
VERSION="3.8.280"
DECOMP_ROOT="$NIXIANG_ROOT/03_decompiled/$GAME/$VERSION"
DUMP_ROOT="$NIXIANG_ROOT/06_dumped/$GAME/$VERSION"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
DUMPER_DIR="$NIXIANG_ROOT/01_tools/Il2CppDumper"

[[ -f "$FETCH_JSON" ]] || { echo "错误: 缺少 $(basename "$FETCH_JSON")" >&2; exit 1; }
APK="$(python3 - "$FETCH_JSON" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding="utf-8"))["apk_path"])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

mkdir -p "$DECOMP_ROOT" "$DUMP_ROOT"
rm -rf "$DECOMP_ROOT/jadx_classes" "$DECOMP_ROOT/manifest.xmltree" \
  "$DECOMP_ROOT/badging.txt" "$DUMP_ROOT/il2cpp_dump" "$DUMP_ROOT/il2cppdumper.log"
mkdir -p "$DECOMP_ROOT/jadx_classes"

extract_entry() {
  local entry="$1" out="$2"
  unzip -p "$APK" "$entry" > "$out"
  [[ -s "$out" ]] || { echo "错误: 提取为空: $entry" >&2; exit 1; }
}

echo "==> 提取关键静态输入"
extract_entry "classes.dex" "$DECOMP_ROOT/classes.dex"
extract_entry "lib/arm64-v8a/libil2cpp.so" "$DUMP_ROOT/libil2cpp.so"
extract_entry "lib/arm64-v8a/libxlua.so" "$DUMP_ROOT/libxlua.so"
extract_entry "lib/arm64-v8a/libunity.so" "$DUMP_ROOT/libunity.so"
extract_entry "lib/arm64-v8a/libnesec.so" "$DUMP_ROOT/libnesec.so"
extract_entry "lib/arm64-v8a/libNetHTProtect.so" "$DUMP_ROOT/libNetHTProtect.so"
extract_entry "assets/bin/Data/Managed/Metadata/global-metadata.dat" "$DUMP_ROOT/global-metadata.packed.dat"
extract_entry "assets/SYResRoot/version.json" "$DUMP_ROOT/version.json"
extract_entry "assets/SYResRoot/RawBytes/ScriptData/bytes/Assembly-CSharp.patch.bytes" \
  "$DUMP_ROOT/Assembly-CSharp.patch.bytes"

echo "==> Manifest / badging"
"$AAPT2" dump badging "$APK" > "$DECOMP_ROOT/badging.txt"
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" > "$DECOMP_ROOT/manifest.xmltree"

echo "==> 壳 dex 反编译"
set +e
jadx -d "$DECOMP_ROOT/jadx_classes" "$DECOMP_ROOT/classes.dex"
JADX_RC=$?
set -e
if [[ "$JADX_RC" -ne 0 ]]; then
  echo "警告: jadx 退出码 $JADX_RC；保留可用产物" >&2
fi

echo "==> 尝试离线 header patch + Il2CppDumper"
cp "$DUMP_ROOT/global-metadata.packed.dat" "$DUMP_ROOT/global-metadata.headerpatched.dat"
printf '\xAF\x1B\xB1\xFA\x18\x00\x00\x00' | dd of="$DUMP_ROOT/global-metadata.headerpatched.dat" \
  bs=1 seek=0 conv=notrunc status=none

set +e
(
  cd "$DUMPER_DIR"
  dotnet Il2CppDumper.dll "$DUMP_ROOT/libil2cpp.so" \
    "$DUMP_ROOT/global-metadata.headerpatched.dat" "$DUMP_ROOT/il2cpp_dump"
) > "$DUMP_ROOT/il2cppdumper.log" 2>&1
DUMPER_RC=$?
set -e
if [[ "$DUMPER_RC" -eq 0 && -s "$DUMP_ROOT/il2cpp_dump/dump.cs" ]]; then
  echo "Il2CppDumper: 成功"
else
  echo "Il2CppDumper: 未成功（rc=${DUMPER_RC}），保留日志供运行时路线参考"
fi

python3 - "$DUMP_ROOT/static_hashes.json" "$DUMP_ROOT" <<'PY'
import hashlib
import json
import sys
from pathlib import Path

out = Path(sys.argv[1])
root = Path(sys.argv[2])
files = [
    "libil2cpp.so",
    "libxlua.so",
    "libunity.so",
    "libnesec.so",
    "libNetHTProtect.so",
    "global-metadata.packed.dat",
    "version.json",
    "Assembly-CSharp.patch.bytes",
]
data = {}
for name in files:
    p = root / name
    if not p.is_file():
        continue
    h = hashlib.sha256()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    data[name] = {"size": p.stat().st_size, "sha256": h.hexdigest()}
out.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "$DUMP_ROOT/static_hashes.json"
printf '\nmanifest=%s\njadx=%s\nil2cppdumper_log=%s\n' \
  "$DECOMP_ROOT/manifest.xmltree" "$DECOMP_ROOT/jadx_classes" "$DUMP_ROOT/il2cppdumper.log"
