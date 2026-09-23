#!/usr/bin/env bash
# 宝石战争 6.0.012：向 System.Network.dll 注入星光桥接并同步热更 manifest 指纹。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
VERSION="${BSZZ_VERSION:-6.0.013}"
STATIC="$NIXIANG_ROOT/06_dumped/宝石战争/$VERSION/static"
DUMP="$NIXIANG_ROOT/06_dumped/宝石战争/$VERSION/il2cpp_dump"
OUT="$NIXIANG_ROOT/07_patched/宝石战争/$VERSION"
ASSETS="$OUT/assets/Android"
PATCH_TOOL="$SCRIPT_DIR/dllpatch"
DUMPER="$NIXIANG_ROOT/01_tools/Il2CppDumper/Il2CppDumper.dll"

[[ -f "$STATIC/System.Network.dll" ]] || { echo "ERROR: 先跑 dump_bszz_static" >&2; exit 1; }
mkdir -p "$DUMP" "$ASSETS/Dlls"

if [[ ! -s "$DUMP/dump.cs" ]]; then
  dotnet "$DUMPER" "$STATIC/libil2cpp.so" "$STATIC/global-metadata.dat" "$DUMP"
fi
if [[ ! -d "$DUMP/DummyDll" ]]; then
  dotnet "$DUMPER" "$STATIC/libil2cpp.so" "$STATIC/global-metadata.dat" "$DUMP"
fi
[[ -d "$DUMP/DummyDll" ]] || { echo "ERROR: Il2CppDumper DummyDll missing" >&2; exit 1; }

dotnet build "$PATCH_TOOL/BszzDllPatch.csproj" -c Release --nologo
dotnet "$PATCH_TOOL/bin/Release/net6.0/BszzDllPatch.dll" \
  "$STATIC/System.Network.dll" "$OUT/System.Network.patched.dll" "$DUMP/DummyDll" "$STATIC"
python3 - "$OUT/System.Network.patched.pinvoke.json" <<'PY'
from pathlib import Path
import json
import sys

path = Path(sys.argv[1])
data = json.loads(path.read_text(encoding="utf-8"))
data["p_invoke"] = [{"EntryPoint": entry} for entry in data["entry_points"]]
path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY
python3 "$SCRIPT_DIR/pack_network_dll.py" "$OUT/System.Network.patched.dll" "$ASSETS/Dlls/System.Network.dll.bytes"

python3 - "$STATIC" "$ASSETS" "$OUT" <<'PY'
from pathlib import Path
import hashlib, json, sys
static, assets, out = map(Path, sys.argv[1:])
enc = assets / "Dlls/System.Network.dll.bytes"
size = enc.stat().st_size
md5 = hashlib.md5(enc.read_bytes()).hexdigest()

for file_name, path_key in (("manifest.json", "Android/Dlls/System.Network.dll.bytes"),
                            ("RuntimeAssembliesReceipt.json", "Dlls/System.Network.dll.bytes")):
    data = json.loads((static / file_name).read_text(encoding="utf-8"))
    for entry in data.get("files", []):
        if entry.get("path") == path_key:
            entry["hash"], entry["size"] = md5, size
    for entry in data.get("assemblies", []):
        if entry.get("path") == path_key:
            entry["hash"], entry["size"] = md5, size
    (assets / file_name).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

summary = {
    "patched_plain_md5": hashlib.md5((out / "System.Network.patched.dll").read_bytes()).hexdigest(),
    "patched_plain_size": (out / "System.Network.patched.dll").stat().st_size,
    "patched_bytes_md5": md5,
    "patched_bytes_size": size,
    "patched_bytes_sha256": hashlib.sha256(enc.read_bytes()).hexdigest(),
}
(out / "patch_summary.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(summary, ensure_ascii=False, indent=2))
PY
cat "$OUT/patch_summary.json"
