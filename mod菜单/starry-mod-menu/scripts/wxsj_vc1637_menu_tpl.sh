#!/usr/bin/env bash
# 无限升级 11.266018 / vc1637 — 生成专用「菜单模板 dex」
#
# 为什么需要本步骤（与 12.x 模板的差异）：
#   12.x 的 templates/wxsj_menu_20260926/classes2.dex 里，StarryApp.super 被改成
#   网易易盾壳类 Lcom/netease/nis/wrapper/MyApplication;（vc1644 官方 Application）。
#   11.266018 / vc1637 的官方包【没有】这个类（全包 grep = 0），也没有
#   application@name；若直接复用 12.x 模板，StarryInitProvider → StarryApp.isMainProcess()
#   触发 StarryApp 类初始化 → 解析父类失败 NoClassDefFoundError → 注入链全灭。
#   故本脚本把 StarryApp.super 还原为 Landroid/app/Application;，其余类逐字节沿用。
#
# 产物：templates/wxsj_menu_vc1637/classes4.dex（+ libc++_shared.so + starlight.crt）
# 断言：类集合与 12.x 模板【完全一致】（只改父类引用，不增删类）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
APP="$NIXIANG/mod菜单/starry-mod-menu"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
# 2026-10-26：默认改指向**去日期化**的 wxsj_menu_current（由 wxsj_refresh_menu_tpl.sh
# 从当前源码现编，带 build_info.txt + dex 构建戳）。历史默认值 wxsj_menu_20260926 已停在
# 2026-09-26，用它派生 = 出旧菜单（无限升级 vc1637 / 镇邪人 7f94233e 同一根因）。
SRC_TPL="${WXSJ_SRC_TPL:-$APP/templates/wxsj_menu_current}"
OUT="$APP/templates/wxsj_menu_vc1637"
WORK="$(mktemp -d /tmp/wxsj_vc1637_tpl.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

for f in "$SMALI" "$BAKSMALI" "$SRC_TPL/classes2.dex"; do
  [[ -e "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done
# 父模板必须是 refresh 脚本的新产物（带 build_info 与构建戳），否则派生出来仍是旧菜单。
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "$SRC_TPL" --dex-name classes2.dex

echo "[1/5] baksmali 12.x 模板 dex"
"$BAKSMALI" d -o "$WORK/smali" "$SRC_TPL/classes2.dex"
[[ -f "$WORK/smali/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali 缺失"; exit 1; }

echo "[2/5] StarryApp.super → android.app.Application（vc1637 无易盾壳类）"
python3 - "$WORK/smali/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
SHELL = "Lcom/netease/nis/wrapper/MyApplication;"
APP = "Landroid/app/Application;"
t = p.read_text(encoding="utf-8")
repls = [
    (f".super {SHELL}", f".super {APP}"),
    (f"invoke-direct {{p0}}, {SHELL}-><init>()V",
     f"invoke-direct {{p0}}, {APP}-><init>()V"),
    (f"invoke-super {{p0, p1}}, {SHELL}->attachBaseContext(Landroid/content/Context;)V",
     f"invoke-super {{p0, p1}}, {APP}->attachBaseContext(Landroid/content/Context;)V"),
    (f"invoke-super {{p0}}, {SHELL}->onCreate()V",
     f"invoke-super {{p0}}, {APP}->onCreate()V"),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b); changed += 1
if SHELL in t:
    raise SystemExit(f"ERROR: StarryApp.smali 仍引用 {SHELL}")
if f".super {APP}" not in t:
    raise SystemExit(f"ERROR: StarryApp.smali 未改为 .super {APP}")
p.write_text(t, encoding="utf-8")
print(f"  ok ({changed} edits)")
PY

echo "[3/5] smali assemble → classes4.dex"
mkdir -p "$OUT"
"$SMALI" assemble -o "$OUT/classes4.dex" "$WORK/smali" >/dev/null
[[ -s "$OUT/classes4.dex" ]] || { echo "ERROR: 组装失败"; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$OUT/classes4.dex" \
  || { echo "ERROR: classes4.dex 内缺 StarryInitProvider"; exit 1; }
if LC_ALL=C grep -aq 'com/netease/nis/wrapper/MyApplication' "$OUT/classes4.dex"; then
  echo "ERROR: classes4.dex 仍含易盾壳类引用"; exit 1
fi

echo "[4/5] 类集合比对（必须完全一致）"
"$BAKSMALI" list classes "$SRC_TPL/classes2.dex" | sort > "$WORK/prev_cls.txt"
"$BAKSMALI" list classes "$OUT/classes4.dex" | sort > "$WORK/new_cls.txt"
if ! diff -q "$WORK/prev_cls.txt" "$WORK/new_cls.txt" >/dev/null; then
  echo "ERROR: 类集合变化（应只改父类引用）："
  diff "$WORK/prev_cls.txt" "$WORK/new_cls.txt" | sed 's/^/    /' || true
  exit 1
fi
echo "  类集合一致 ✓（$(wc -l < "$WORK/new_cls.txt" | tr -d ' ') 类）"

echo "[5/5] 复制 so/证书配套文件 + build_info（继承父模板身份）"
cp -f "$SRC_TPL/libc++_shared.so" "$OUT/libc++_shared.so"
cp -f "$SRC_TPL/starlight.crt" "$OUT/starlight.crt"
if LC_ALL=C grep -aq 'starry_selftest' "$OUT/classes4.dex"; then
  echo "ERROR: classes4.dex 带调试脚手架 starry_selftest（父模板是 debug 包编的？）"; exit 1
fi
STAMP_LINE="$(LC_ALL=C grep -aoE 'starry-menu:[0-9a-f]{7,40}' "$OUT/classes4.dex" | head -1 || true)"
[[ -n "$STAMP_LINE" ]] || { echo "ERROR: classes4.dex 缺构建戳（父模板不是 refresh 脚本产出的？）"; exit 1; }
DEX_SHA="$(shasum -a 256 "$OUT/classes4.dex" | awk '{print $1}')"
{
  if [[ -f "$SRC_TPL/build_info.txt" ]]; then cat "$SRC_TPL/build_info.txt"; fi
  echo "dex         : classes4.dex"
  echo "derived_from: ${SRC_TPL}"
  echo "derived_at  : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "derived_note: StarryApp.super→android.app.Application（vc1637 无易盾壳类）"
  echo "dex_sha256  : ${DEX_SHA}"
  echo "stamp       : ${STAMP_LINE}"
} > "$OUT/build_info.txt"
# build_info 里可能有两行 dex_sha256/stamp（父模板 + 本层），gate 只取第一条 source_commit，
# 但为了不误导，重写去重：保留本层最后一行。
python3 - "$OUT/build_info.txt" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
lines = [l for l in p.read_text(encoding="utf-8").splitlines() if l.strip()]
out, seen = [], {}
for l in lines:
    k = l.split(":", 1)[0].strip() if ":" in l else ""
    if k in ("dex", "dex_sha256", "stamp", "derived_from", "derived_at", "derived_note"):
        seen[k] = l
    else:
        out.append(l)
for k in ("dex", "derived_from", "derived_at", "derived_note", "dex_sha256", "stamp"):
    if k in seen:
        out.append(seen[k])
p.write_text("\n".join(out) + "\n", encoding="utf-8")
PY
ls -l "$OUT"
cat "$OUT/build_info.txt"
shasum -a 256 "$OUT/classes4.dex"
echo
echo "新鲜度自检:"
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "$OUT" --dex-name classes4.dex --require-stamp
