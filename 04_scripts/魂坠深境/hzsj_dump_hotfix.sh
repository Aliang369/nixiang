#!/usr/bin/env bash
# 魂坠深境 管线 dump 阶段后端：拉热更 bundle → UnityPy 抽 DLL → ilspy 反编译
# 产物: 06_dumped/魂坠深境_1.1.1/{hotfix, dlls/Assembly-CSharp.dll, src/DamageAction.cs ...}
# 依据: 10_docs/热更游戏版本更新SOP_可复用_20260903.md Step1（热更在外部存储 files/Asset/hotfix）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.chillyroom.containerofsoul.yw"
VER="1.1.1"
OUT="$NIXIANG/06_dumped/魂坠深境_$VER"
HOTFIX_REMOTE="/storage/emulated/0/Android/data/$PKG/files/Asset/hotfix"

mkdir -p "$OUT"
adb -s "$SERIAL" pull "$HOTFIX_REMOTE" "$OUT/hotfix" >/dev/null
echo "[dump] pulled hotfix: $(ls -lh "$OUT/hotfix" | awk '{print $5}')"

python3.11 - "$OUT" <<'PY'
import sys
import UnityPy
from pathlib import Path

out = Path(sys.argv[1])
dlls = out / "dlls"
dlls.mkdir(exist_ok=True)
env = UnityPy.load(str(out / "hotfix"))
n = 0
for obj in env.objects:
    if obj.type.name != "TextAsset":
        continue
    data = obj.read()
    script = data.m_Script
    raw = script.encode("utf-8", "surrogateescape") if isinstance(script, str) else bytes(script)
    if raw[:2] == b"MZ":
        name = data.m_Name
        if not name.endswith(".dll"):
            name += ".dll"
        (dlls / name).write_bytes(raw)
        n += 1
print(f"[dump] dlls={n}")
assert (dlls / "Assembly-CSharp.dll").exists(), "Assembly-CSharp.dll missing"
PY

SRC="$OUT/src"
mkdir -p "$SRC"
"$HOME/.dotnet/tools/ilspycmd" -p -o "$SRC" "$OUT/dlls/Assembly-CSharp.dll" >/dev/null
for f in DamageAction BattleFieldTransform PlayerGameStateInfo SceneInfoSingleton Item ItemRequirement GameUIManager ProtectedInt; do
  [[ -f "$SRC/$f.cs" ]] || { echo "ERROR: $f.cs missing in decompile output" >&2; exit 1; }
done
echo "[dump] decompiled → $SRC ($(find "$SRC" -name '*.cs' | wc -l | tr -d ' ') files)"
