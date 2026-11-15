#!/usr/bin/env bash
# script-policy: {"purpose": "群侠传：幸存者 三个 .gdc 挂点补丁（无敌属性 / 伤害倍率 / 无限银钱）+ starry_switch 开关桥回编，按锚点最小插入并精确等长回填", "inputs": "03_decompiled/{游戏}/gd_{版本}/ 下三个反编译 .gd；gd_menu/starry_switch.gd；03_decompiled/{游戏}/pad_gdc.py；01_tools/gdre 下的 GDRE Tools；宿主 zstd", "outputs": "07_patched/{游戏}/gdc_menu_{版本}/ 下 health_component.gdc / combat_hit_pipeline.gdc / economy_store.gdc / starry_switch.gdc / starry_switch.gd.remap；中间态在 13_scratch/qxzxcz_gdc_{版本}/", "boundary": "只产出 .gdc 与 remap；不动 APK、不动 dex/smali、不动 native、不装机；不整段替换、不按旧版行号改，只按锚点插入", "prior_art": "04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.9.sh（gdc 阶段与断言骨架）；03_decompiled/群侠传：幸存者/pad_gdc.py（等长填充）", "difference": "补丁基于 0.2.3.10 新反编译源码重新打（不复用 0.2.3.9 源码：pipeline 已改动且 110 个同名 gdc 长度变化）；长度表按版本 case 参数化；新增锚点断言、zstd 帧校验与回读抽查", "evidence": "10_docs/群侠传：幸存者/星光菜单落地.md 第3节（等长铁律）；10_docs/群侠传：幸存者/_state/0.2.3.10_差异盘点.md 第6节"}
# ============================================================================
# 群侠传：幸存者（com.nenlystudio.qunxiasurvivor.cn，Godot 4.6.1，TapTap 中国）
#   三个 .gdc 挂点补丁 + 新增 starry_switch 开关桥
#
# 三个挂点（每处只加一行 preload 常量 + 判定逻辑，不改其余上下文）：
#   1) components/health_component.gdc  var invulnerable 改成属性
#         get: return _invulnerable or (_vitals != null and StarrySwitch.on(2, 0))
#         set: _invulnerable = value
#       => take_damage 本体不动，player.gd 的 DOT/受击判定读同一属性，共用一份开关
#   2) systems/combat_hit_pipeline.gdc  target_damage 计算行之后插
#         target_damage *= StarrySwitch.num(1, 1.0)
#   3) core/model/economy_store.gdc     set_silver / install_draft 的 silver 赋值
#         silver = 999999999 if StarrySwitch.on(3, 0) else <原值>
#
#   starry_switch.gdc 是新增文件（不受等长约束），读 user://starry_state.txt
#   （= /data/user/0/com.nenlystudio.qunxiasurvivor.cn/files/starry_state.txt，
#     单行 8 个空格分隔整数，下标=featNum；[1]伤害倍率 [2]无敌 [3]无限银钱）。
#
# 等长铁律（本作硬约束）：三个替换条目的解压尺寸必须精确等于原包条目，
#   否则引擎报 "Error decompressing GDScript tokenizer buffer."
#   (gdscript_tokenizer_buffer.cpp:154) -> 依赖脚本全挂 -> GLThread SIGSEGV。
#   做法：GDRE 回编 -> pad_gdc.py（zstd 高压缩级别重压 + skippable frame 补足）。
#
# 用法：
#   bash 04_scripts/群侠传：幸存者/patch_gdc_qxzxcz.sh
#   bash 04_scripts/群侠传：幸存者/patch_gdc_qxzxcz.sh --version=0.2.3.10
#   bash 04_scripts/群侠传：幸存者/patch_gdc_qxzxcz.sh --only-gdc   # 产物已在且齐 => 跳过重编
# 可覆盖变量：QXZXCZ_VER / QXZXCZ_GDRE / QXZXCZ_BYTECODE / QXZXCZ_STARRY_SRC
#             QXZXCZ_LEN_HEALTH / QXZXCZ_LEN_PIPELINE / QXZXCZ_LEN_ECONOMY
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="群侠传：幸存者"
VER="${QXZXCZ_VER:-0.2.3.10}"
ONLY_GDC=0

for arg in "$@"; do
  case "$arg" in
    --only-gdc) ONLY_GDC=1 ;;
    --version=*) VER="${arg#*=}" ;;
    -h|--help) sed -n '2,50p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 ${arg}（支持 --only-gdc / --version=X）" >&2; exit 2 ;;
  esac
done

GDRE="${QXZXCZ_GDRE:-$NIXIANG/01_tools/gdre/Godot RE Tools.app/Contents/MacOS/Godot RE Tools}"
BYTECODE="${QXZXCZ_BYTECODE:-4.5.0}"
PAD="$NIXIANG/03_decompiled/${GAME}/pad_gdc.py"
GD_SRC="${QXZXCZ_GD_SRC:-$NIXIANG/03_decompiled/${GAME}/gd_${VER}}"
STARRY_SRC="${QXZXCZ_STARRY_SRC:-$NIXIANG/03_decompiled/${GAME}/gd_menu/starry_switch.gd}"
SCRATCH="$NIXIANG/13_scratch/qxzxcz_gdc_${VER}"
SRC="$SCRATCH/src"
RAW="$SCRATCH/raw"
VFY="$SCRATCH/verify"
OUT="$NIXIANG/07_patched/${GAME}/gdc_menu_${VER}"

# 三个条目的未压缩原始长度（zip ZipInfo.file_size / 解压后尺寸）—— 换版本必须重取
case "$VER" in
  0.2.3.10) LEN_HEALTH=2841; LEN_PIPELINE=35739; LEN_ECONOMY=6630 ;;
  0.2.3.9)  LEN_HEALTH=2841; LEN_PIPELINE=34169; LEN_ECONOMY=6630 ;;
  *) echo "ERROR: 未知版本 $VER —— 请先在脚本 case 里补三个条目的原始解压长度（0.2.3.9=2841/34169/6630，0.2.3.10=2841/35739/6630）" >&2; exit 2 ;;
esac
LEN_HEALTH="${QXZXCZ_LEN_HEALTH:-$LEN_HEALTH}"
LEN_PIPELINE="${QXZXCZ_LEN_PIPELINE:-$LEN_PIPELINE}"
LEN_ECONOMY="${QXZXCZ_LEN_ECONOMY:-$LEN_ECONOMY}"

die(){ echo "ERROR: $*" >&2; exit 1; }
step(){ echo ""; echo "== $* =="; }

step "0/6 输入检查（版本 ${VER}）"
[ -f "$GDRE" ] || die "缺 GDRE：$GDRE"
[ -f "$PAD" ] || die "缺 pad_gdc.py：$PAD"
[ -f "$STARRY_SRC" ] || die "缺 starry_switch.gd 源：$STARRY_SRC"
command -v zstd >/dev/null 2>&1 || die "缺 zstd（pad_gdc.py 依赖）"
command -v python3 >/dev/null 2>&1 || die "缺 python3"
for n in health_component combat_hit_pipeline economy_store; do
  [ -f "$GD_SRC/$n.gd" ] || die "缺反编译源码 $GD_SRC/$n.gd（先跑 GDRE --decompile 到该目录）"
done
echo "  GDRE      = $GDRE"
echo "  源码      = $GD_SRC"
echo "  目标长度  = health $LEN_HEALTH / pipeline $LEN_PIPELINE / economy $LEN_ECONOMY"

mkdir -p "$SRC" "$RAW" "$VFY" "$OUT"

need=1
if [ "$ONLY_GDC" = "1" ]; then
  if [ -f "$OUT/health_component.gdc" ] && [ -f "$OUT/combat_hit_pipeline.gdc" ] \
     && [ -f "$OUT/economy_store.gdc" ] && [ -f "$OUT/starry_switch.gdc" ] \
     && [ -f "$OUT/starry_switch.gd.remap" ]; then
    need=0
    echo "  --only-gdc：$OUT 产物已齐，跳过重编（只跑校验）"
  fi
fi

if [ "$need" = "1" ]; then
  step "1/6 复制新版反编译源码 + 按锚点最小插入"
  cp -f "$GD_SRC/health_component.gd" "$SRC/health_component.gd"
  cp -f "$GD_SRC/combat_hit_pipeline.gd" "$SRC/combat_hit_pipeline.gd"
  cp -f "$GD_SRC/economy_store.gd" "$SRC/economy_store.gd"
  cp -f "$STARRY_SRC" "$SRC/starry_switch.gd"

  python3 - "$SRC" <<'PYEOF'
import os, sys

CONST = 'const StarrySwitch: = preload("res://scripts/starry_switch.gd")'

def die(msg, lines=None, idx=None):
    sys.stderr.write("ERROR: " + msg + "\n")
    if lines is not None and idx is not None:
        sys.stderr.write("--- 上下文（第 %d 行附近） ---" % (idx + 1) + chr(10))
        for i in range(max(0, idx - 3), min(len(lines), idx + 4)):
            sys.stderr.write("%5d| %s\n" % (i + 1, lines[i]))
    raise SystemExit(2)

def load(p):
    with open(p, encoding="utf-8") as f:
        return f.read().split("\n")

def save(p, lines):
    with open(p, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

def find_unique(lines, target, what):
    hits = [i for i, l in enumerate(lines) if l == target]
    if len(hits) != 1:
        # 命中 0 次时给"近似行"的上下文；命中多次时给第一处的上下文
        near = [i for i, l in enumerate(lines) if l.strip().startswith(target.strip()[:24])]
        ctx = hits[0] if hits else (near[0] if near else None)
        die("锚点 %s 期望命中 1 次，实际 %d 次：%r（反编译源码不是这个版本？）" % (what, len(hits), target),
            lines, ctx)
    return hits[0]

def insert_const(lines, ext_line):
    if any(CONST in l for l in lines):
        return 0
    i = find_unique(lines, ext_line, "extends 行")
    lines.insert(i + 1, CONST)
    return 1

def patch_health(p):
    lines = load(p)
    n = insert_const(lines, "extends Node")
    i = find_unique(lines, "var invulnerable: bool = false", "invulnerable 声明")
    block = [
        "var invulnerable: bool:",
        "\tget:",
        "\t\treturn _invulnerable or (_vitals != null and StarrySwitch.on(2, 0))",
        "\tset(value):",
        "\t\t_invulnerable = value",
        "",
        "var _invulnerable: bool = false",
    ]
    lines[i:i + 1] = block
    save(p, lines)
    return n, len(block) - 1

def patch_pipeline(p):
    lines = load(p)
    n = insert_const(lines, "extends RefCounted")
    i = find_unique(lines, "\tvar target_damage: float = hit_event.settle_damage() * damage_multiplier",
                    "target_damage 赋值")
    if "StarrySwitch.num(1, 1.0)" in lines[i + 1]:
        return n, 0
    lines.insert(i + 1, "\ttarget_damage *= StarrySwitch.num(1, 1.0)")
    save(p, lines)
    return n, 1

def patch_economy(p):
    lines = load(p)
    n = insert_const(lines, "extends CoreStore")
    fs = find_unique(lines, "func set_silver(value: int) -> void :", "set_silver")
    if lines[fs + 1] != "\tsilver = value":
        die("set_silver 首行变了：%r" % lines[fs + 1], lines, fs + 1)
    lines[fs + 1] = "\tsilver = 999999999 if StarrySwitch.on(3, 0) else value"
    fd = find_unique(lines, "func install_draft(draft: Dictionary) -> void :", "install_draft")
    if lines[fd + 1] != '\tsilver = int(draft.get("silver", silver))':
        die("install_draft 首行变了：%r" % lines[fd + 1], lines, fd + 1)
    lines[fd + 1] = '\tsilver = 999999999 if StarrySwitch.on(3, 0) else int(draft.get("silver", silver))'
    save(p, lines)
    return n, 2

d = sys.argv[1]
print("  health_component.gd    const+%d / +%d 行" % patch_health(os.path.join(d, "health_component.gd")))
print("  combat_hit_pipeline.gd const+%d / +%d 行" % patch_pipeline(os.path.join(d, "combat_hit_pipeline.gd")))
print("  economy_store.gd       const+%d / +%d 处" % patch_economy(os.path.join(d, "economy_store.gd")))
print("  PATCH OK")
PYEOF

  step "2/6 GDRE 回编（bytecode=${BYTECODE}）"
  for n in health_component combat_hit_pipeline economy_store starry_switch; do
    "$GDRE" --headless --bytecode="$BYTECODE" --compile="$SRC/$n.gd" --output="$RAW" 2>&1 \
      | grep -E "Compiled|ERROR|SCRIPT ERROR" || true
    [ -f "$RAW/$n.gdc" ] || die "回编失败：$RAW/$n.gdc 未生成（看上面 GDRE 输出）"
    echo "  $n.gdc = $(wc -c < "$RAW/$n.gdc" | tr -d ' ') B（回编原始）"
  done

  step "3/6 等长填充（pad_gdc.py）"
  python3 "$PAD" "$RAW/health_component.gdc" "$LEN_HEALTH" "$OUT/health_component.gdc"
  python3 "$PAD" "$RAW/combat_hit_pipeline.gdc" "$LEN_PIPELINE" "$OUT/combat_hit_pipeline.gdc"
  python3 "$PAD" "$RAW/economy_store.gdc" "$LEN_ECONOMY" "$OUT/economy_store.gdc"

  step "4/6 starry_switch.gdc + remap"
  cp -f "$RAW/starry_switch.gdc" "$OUT/starry_switch.gdc"
  printf '[remap]\n\npath="res://scripts/starry_switch.gdc"\n' > "$OUT/starry_switch.gd.remap"
  echo "  starry_switch.gdc = $(wc -c < "$OUT/starry_switch.gdc" | tr -d ' ') B（新增文件，不受等长约束）"
fi

step "5/6 硬校验：字节长度 / zstd 帧 / 回读抽查"
python3 - "$OUT" "$LEN_HEALTH" "$LEN_PIPELINE" "$LEN_ECONOMY" <<'PYEOF'
import os, struct, sys

out, lh, lp, le = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4])
bad = 0
for name, want in (("health_component.gdc", lh), ("combat_hit_pipeline.gdc", lp), ("economy_store.gdc", le)):
    p = os.path.join(out, name)
    got = os.path.getsize(p)
    ok = (got == want)
    head = open(p, "rb").read(12)
    magic = head[:4].decode("latin-1")
    ver = struct.unpack("<I", head[4:8])[0]
    dlen = struct.unpack("<I", head[8:12])[0]
    if magic != "GDSC":
        ok = False
    print("%s %s  %d B / 目标 %d B  (%+d)  GDSC ver=%d 解压长度=%d"
          % ("PASS" if ok else "FAIL", name, got, want, got - want, ver, dlen))
    if not ok:
        bad += 1
p = os.path.join(out, "starry_switch.gdc")
print("PASS starry_switch.gdc  %d B（新增，无等长要求）" % os.path.getsize(p))
r = os.path.join(out, "starry_switch.gd.remap")
txt = open(r, encoding="utf-8").read()
if 'path="res://scripts/starry_switch.gdc"' in txt and txt.lstrip().startswith("[remap]"):
    print("PASS starry_switch.gd.remap  %d B" % os.path.getsize(r))
else:
    print("FAIL starry_switch.gd.remap 内容不对：%r" % txt)
    bad += 1
raise SystemExit(1 if bad else 0)
PYEOF

for f in health_component combat_hit_pipeline economy_store starry_switch; do
  tail -c +13 "$OUT/$f.gdc" | zstd -d -c >/dev/null 2>&1 || die "zstd 流无法解析（帧结构坏了）：$OUT/$f.gdc"
done
echo "PASS zstd 帧可解析（4/4，尾随 skippable frame 被正常跳过）"

rm -f "$VFY/health_component.gd" "$VFY/combat_hit_pipeline.gd" "$VFY/economy_store.gd"
for f in health_component combat_hit_pipeline economy_store; do
  "$GDRE" --headless --bytecode="$BYTECODE" --decompile="$OUT/$f.gdc" --output="$VFY" 2>&1 \
    | grep -E "Decompiled|ERROR" || true
  [ -f "$VFY/$f.gd" ] || die "回读失败：$VFY/$f.gd 未生成"
done
grep -Fq '_invulnerable or (_vitals != null and StarrySwitch.on(2, 0))' "$VFY/health_component.gd" || die "回读缺无敌属性判定"
grep -Fq 'if invulnerable:' "$VFY/health_component.gd" || die "回读缺 take_damage 的 invulnerable 判定（被误改？）"
grep -Fq 'target_damage *= StarrySwitch.num(1, 1.0)' "$VFY/combat_hit_pipeline.gd" || die "回读缺伤害倍率插入"
grep -Fq 'silver = 999999999 if StarrySwitch.on(3, 0) else value' "$VFY/economy_store.gd" || die "回读缺 set_silver 判定"
grep -Fq 'silver = 999999999 if StarrySwitch.on(3, 0) else int(draft.get("silver", silver))' "$VFY/economy_store.gd" || die "回读缺 install_draft 判定"
echo "PASS 回读抽查（4 条关键行存在 + take_damage 未被改动）"

step "6/6 完成"
echo "  产物目录：$OUT"
ls -l "$OUT"
