#!/usr/bin/env bash
# 阶段5：收尾核对——残留清空、构建输入归位、文档改后校验
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SRC="07_patched/绿梦/_import_E_build_20261028"
BI="04_scripts/绿梦/build_inputs_20261028"
PR="04_scripts/绿梦/probes_20261028"
MAN="10_docs/绿梦/_import_E_build_清理清单_20261028.md"

echo "==== 残留内容 ===="
find "$SRC" | head -20
echo "体积: $(du -sh "$SRC" | cut -f1)"

echo
echo "==== 把构建输入从 probes 移回 build_inputs ===="
for item in classes4.dex classes4.dex.bak classes4.dex.bak2 classes4.dex.bak3 classes2_patched.dex payload2___data payload2___info jitblk.bin dllman.bin; do
  if [ -e "$PR/$item" ]; then
    mv -f -- "$PR/$item" "$BI/" && echo "  归位 $item"
  fi
done

echo
echo "==== 清空残留目录 ===="
find "$SRC" -type d -empty -delete 2>/dev/null
if [ -z "$(find "$SRC" -mindepth 1 2>/dev/null | head -1)" ]; then
  rmdir "$SRC" 2>/dev/null && echo "  已删除 $SRC ✓"
else
  echo "  仍有内容："; find "$SRC" | head -10
fi

echo
echo "==== 文档改动校验（UTF-8 + 是否还有 E:\\_build）===="
python3 - <<'PY'
import io
files = [
 "10_docs/_state/games/绿梦.md",
 "10_docs/绿梦/tapsdk签名校验与网页登录_20261028.md",
 "10_docs/绿梦/雷电houdini_FairGuard实测_20261027.md",
 "10_docs/绿梦/_import_E_build_清理清单_20261028.md",
]
for f in files:
    raw = open(f, "rb").read()
    try:
        s = raw.decode("utf-8")
        ok = "utf8-ok"
    except UnicodeDecodeError as e:
        ok = "UTF8-ERR " + str(e)
    left = s.count("E:\\_build") if ok == "utf8-ok" else -1
    print(f"  {f}: {ok}  lines={s.count(chr(10))}  仍含E:\\_build={left}")
PY

echo
echo "==== 体积汇总 ===="
du -sh 04_scripts/绿梦/probes_20261028 04_scripts/绿梦/build_inputs_20261028 08_output/绿梦/_import_20261028 01_tools/dotnet 2>/dev/null
echo "07_patched/绿梦 现状:"; ls -1 07_patched/绿梦 2>/dev/null
