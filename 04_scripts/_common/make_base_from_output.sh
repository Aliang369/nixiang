#!/usr/bin/env bash
# ============================================================
#  make_base_from_output.sh — 从「已移植产物包」反推重建底包
#
#  来源：2026-09-30 伊莫 1.0.1 会话复盘（P0-4）
#  背景：工作区规范「APK 用完即删」（不长期留底包），收尾清理时底包也一并删了；
#        然而**收尾后仍可能需要再打一次包**（改一个上限、补一条日志…），
#        此时只能用交付包**减去注入物**反推底包。伊莫手工做过两次，每次都要
#        现查注入物名单 + 现算条目数——本脚本把这件事固化。
#
#  用法:
#    bash 04_scripts/_common/make_base_from_output.sh \
#         --in 08_output/伊莫/伊莫_1.0.1_surgical-releaseSigned.apk \
#         --out /tmp/yimo_base.apk [--game 伊莫] [--strip <apk 内路径>]... [--list]
#
#  说明:
#    · 默认注入物名单按游戏（见 case 分支）；--strip 可追加/覆盖任意条目。
#    · 反推底包 **不等于** 官方原版：会丢官方签名/对齐与 zip 目录字节一致性，
#      只适合作为「再次构建的输入」。台账里必须标明它是反推产物，
#      下次仍优先取官方原版（见 10_docs/_state/底包再获取台账.md 的维护规则）。
#    · 产出后打印 条目数/大小/sha256/md5，供台账登记。
#  退出码: 0 = 成功；1 = 失败；2 = 用法错误
# ============================================================
set -euo pipefail

IN=""
OUT=""
GAME=""
LIST_ONLY=0
STRIPS=()
NSTRIP=0     # ★ bash 3.2 + set -u：空数组一律用计数器（见会话复盘 P1-3）

usage() { sed -n '2,24p' "$0" >&2; exit 2; }

while [[ $# -gt 0 ]]; do
  case "$1" in
    --in)    [[ $# -ge 2 ]] || usage; IN="$2"; shift 2 ;;
    --out)   [[ $# -ge 2 ]] || usage; OUT="$2"; shift 2 ;;
    --game)  [[ $# -ge 2 ]] || usage; GAME="$2"; shift 2 ;;
    --strip) [[ $# -ge 2 ]] || usage; STRIPS[$NSTRIP]="$2"; NSTRIP=$((NSTRIP + 1)); shift 2 ;;
    --list)  LIST_ONLY=1; shift ;;
    -h|--help) usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$IN" ]] || { echo "ERROR: 需要 --in <交付包>" >&2; usage; }
[[ -f "$IN" ]] || { echo "ERROR: 找不到交付包: $IN" >&2; exit 1; }
if [[ "$LIST_ONLY" != "1" ]]; then
  [[ -n "$OUT" ]] || { echo "ERROR: 需要 --out <底包输出路径>" >&2; usage; }
  [[ -e "$OUT" ]] && { echo "ERROR: 输出已存在，先删或换路径: $OUT" >&2; exit 1; }
fi

# ---------- 各游戏默认注入物（★ 数组用计数器，bash 3.2 + set -u 下 ${arr[@]} 会炸）----------
# 依据：注入 dex（官方无 classes2.dex ⇒ 注入体顺延）、注入 so、随包 libc++_shared.so
DSTRIPS=(); ND=0
_addstrip() { DSTRIPS[$ND]="$1"; ND=$((ND + 1)); }

case "$GAME" in
  伊莫|Yimo|yimo|com.x.yimoo.official)
    _addstrip classes2.dex
    _addstrip lib/arm64-v8a/libc++_shared.so
    _addstrip lib/arm64-v8a/libstarry_magic.so
    ;;
  "")
    echo "提示: 未指定 --game，注入物名单请用 --strip 显式给出（或加 --game 用内置名单）" >&2
    ;;
  *)
    echo "提示: --game $GAME 无内置名单，按通用规则猜（注入 dex + 注入 so + libc++_shared.so）" >&2
    _addstrip classes2.dex
    _addstrip lib/arm64-v8a/libc++_shared.so
    _addstrip lib/arm64-v8a/libstarry_magic.so
    ;;
esac

STRS=(); N=0
for ((i = 0; i < ND; i++)); do STRS[$N]="${DSTRIPS[$i]}"; N=$((N + 1)); done
for ((i = 0; i < NSTRIP; i++)); do STRS[$N]="${STRIPS[$i]}"; N=$((N + 1)); done

if [[ $N -eq 0 ]]; then
  echo "ERROR: 没有可删的注入物（用 --game 或 --strip 指定）" >&2; exit 2
fi

echo "== 反推底包 =="
echo "  输入: $IN"
echo "  将删: ${STRS[@]+"${STRS[@]}"}"

if [[ "$LIST_ONLY" == "1" ]]; then
  python3 - "$IN" "${STRS[@]}" <<'PY'
import sys, zipfile
apk = sys.argv[1]; strips = sys.argv[2:]
z = zipfile.ZipFile(apk)
names = set(z.namelist())
print("  交付包条目: %d" % len(names))
for s in strips:
    print("  %-45s %s" % (s, "存在（会删）" if s in names else "★不存在（跳过）"))
print("  交付包内 deck/so 概览:")
for n in sorted(names):
    if n.endswith('.dex') or (n.startswith('lib/') and n.endswith('.so')):
        print("    %-55s %d B" % (n, z.getinfo(n).file_size))
PY
  exit 0
fi

cp -f "$IN" "$OUT"
zip -q -d "$OUT" ${STRS[@]+"${STRS[@]}"}

echo
echo "== 结果自检 =="
python3 - "$OUT" "${STRS[@]}" <<'PY'
import sys, zipfile, hashlib, os
out = sys.argv[1]; strips = sys.argv[2:]
z = zipfile.ZipFile(out)
names = z.namelist()
print("  条目数: %d" % len(names))
bad = [s for s in strips if s in set(names)]
print("  残留注入物: %s" % (bad if bad else "无 ✅"))
for n in names:
    if n.endswith('.dex'):
        print("  dex: %-20s %d B" % (n, z.getinfo(n).file_size))
print("  lib: %s" % sorted({n for n in names if n.startswith('lib/') and n.endswith('.so')}))
h = hashlib.sha256(); m = hashlib.md5()
with open(out, 'rb') as f:
    for b in iter(lambda: f.read(1 << 20), b''):
        h.update(b); m.update(b)
print("  size: %d B" % os.path.getsize(out))
print("  md5:    %s" % m.hexdigest())
print("  sha256: %s" % h.hexdigest())
PY

echo
echo "[done] 反推底包: $OUT"
echo "  ★ 它不是官方原版（zip 目录/签名/对齐已变），仅作再次构建的输入；"
echo "    台账登记时须标明「反推重建」，下次构建仍优先取官方原版。"
