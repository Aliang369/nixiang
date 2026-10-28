#!/usr/bin/env bash
# ================================================================
#  dex_only_repack.sh — 绿梦：时空之声 1.0.5：只换 classes4.dex 的外科重打
#
#  为什么有它
#  ----------------------------------------------------------------
#  本作正式包已被真机验证过（so=笼子/伪装、assets 载荷、Manifest 三件套都是逐字节
#  定版）。当改动**只涉及菜单框架 Java（com/starry/**）**时，整包重跑
#  `surgical_lvmeng_1.0.5.sh` 会连带重编 native、重拷载荷/Manifest（那些输入在本机
#  可能是旧副本），等于把已验证状态冲掉。本脚本只做一件事：
#     正式包 → 换掉 classes4.dex → zipalign → 重签 → 逐条比对「其它条目字节不变」。
#
#  用法
#  ----------------------------------------------------------------
#    bash 04_scripts/绿梦/dex_only_repack.sh \
#        --src 08_output/绿梦/绿梦_1.0.5_starry-releaseSigned.apk \
#        --dex /path/classes4.dex \
#        [--out 08_output/绿梦/绿梦_1.0.5_starry-r2-releaseSigned.apk] [--dry-run]
#
#  ★ 不覆盖 --src：输出必须是另一个文件名，原包留作对照（逐条比对以它为基线）。
#  ★ 退出码：0=成功；1=校验/重打/比对失败；2=用法错。
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd -P)}"

SRC="" DEX="" OUT="" DRY=0
usage() { sed -n '2,30p' "$0" | sed 's/^# \{0,1\}//' >&2; exit 2; }
while [[ $# -gt 0 ]]; do
  case "$1" in
    --src) SRC="${2:-}"; shift 2 ;;
    --dex) DEX="${2:-}"; shift 2 ;;
    --out) OUT="${2:-}"; shift 2 ;;
    --dry-run) DRY=1; shift ;;
    -h|--help) usage ;;
    *) echo "错误: 未知参数 $1" >&2; usage ;;
  esac
done
[[ -n "$SRC" && -n "$DEX" ]] || usage
[[ -f "$SRC" ]] || { echo "ERROR: 源包不存在: $SRC" >&2; exit 1; }
[[ -f "$DEX" ]] || { echo "ERROR: 新 dex 不存在: $DEX" >&2; exit 1; }
SRC="$(cd "$(dirname "$SRC")" && pwd -P)/$(basename "$SRC")"
DEX="$(cd "$(dirname "$DEX")" && pwd -P)/$(basename "$DEX")"
[[ -n "$OUT" ]] || OUT="$(dirname "$SRC")/$(basename "$SRC" | sed 's/\.apk$//')-r2.apk"
OUT_DIR="$(cd "$(dirname "$OUT")" && pwd -P)"
OUT="$OUT_DIR/$(basename "$OUT")"
[[ "$OUT" != "$SRC" ]] || { echo "ERROR: --out 不得等于 --src（原包要留作逐条比对基线）" >&2; exit 1; }

# dex 魔数校验（防止把 so/apk 误当 dex 塞进包）
python3 - "$DEX" <<'PY'
import sys
b = open(sys.argv[1], 'rb').read(8)
if not (b.startswith(b'dex\n') and b[4:7].isdigit()):
    sys.exit("ERROR: %s 不是 dex（magic=%r）" % (sys.argv[1], b))
print("  dex ok: %s" % b[:8].decode('ascii', 'replace').strip())
PY

KS="$NIXIANG/01_tools/starry_release.jks"
[[ -f "$KS" ]] || { echo "ERROR: 缺签名库 $KS" >&2; exit 1; }
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools 里的 zipalign" >&2; exit 1; }

echo "== dex_only_repack =="
echo "  src = $SRC"
echo "  dex = $DEX ($(wc -c < "$DEX" | tr -d ' ') B)"
echo "  out = $OUT"
unzip -l "$SRC" classes4.dex >/dev/null 2>&1 \
  || { echo "ERROR: 源包里没有 classes4.dex（本脚本只服务本作的菜单 dex 形态）" >&2; exit 1; }

if [[ "$DRY" == "1" ]]; then
  echo "  [dry-run] 将执行：复制源包 → 删 META-INF/* → 换 classes4.dex → zipalign -p 4 → apksigner → 逐条 sha256 比对"
  exit 0
fi

WORK="$(mktemp -d /tmp/lvmeng_dexonly.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp "$SRC" "$WORK/unsigned.apk"

# 旧签名必须整体摘掉（重签会重新生成 v1/v2/v3 需要的那几个文件）
zip -q -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null
cp "$DEX" "$WORK/classes4.dex"
( cd "$WORK" && zip -q -X unsigned.apk classes4.dex >/dev/null )

"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"

echo "-- 结构校验 --"
"$BT/zipalign" -c -p 4 "$OUT" >/dev/null && echo "  zipalign -c -p 4 : OK"
"$BT/apksigner" verify --print-certs "$OUT" 2>/dev/null | head -2

echo "-- 逐条比对（除 classes4.dex / META-INF 外必须字节一致）--"
python3 - "$SRC" "$OUT" <<'PY'
import hashlib, sys, zipfile
def sig(z, name):
    with z.open(name) as f:
        h = hashlib.sha256()
        while True:
            b = f.read(1 << 20)
            if not b: break
            h.update(b)
    return h.hexdigest()
za, zb = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
na = {n for n in za.namelist() if n != 'classes4.dex' and not n.startswith('META-INF/')}
nb = {n for n in zb.namelist() if n != 'classes4.dex' and not n.startswith('META-INF/')}
missing, extra = sorted(na - nb), sorted(nb - na)
if missing: sys.exit("ERROR: 新包丢了条目 %s" % missing[:5])
if extra:   sys.exit("ERROR: 新包多了条目 %s" % extra[:5])
diff = [n for n in sorted(na) if sig(za, n) != sig(zb, n)]
if diff: sys.exit("ERROR: 以下条目内容变了（应当字节不变）: %s" % diff[:8])
print("  entries=%d 全部一致（classes4.dex 除外）；classes4.dex %d B → %d B"
      % (len(na), za.getinfo('classes4.dex').file_size, zb.getinfo('classes4.dex').file_size))
for key in ('lib/arm64-v8a/libstarry_magic.so', 'AndroidManifest.xml',
            'assets/lvmeng/__data', 'assets/lvmeng/fg_orig.apk'):
    print("   · %-42s %d B" % (key, zb.getinfo(key).file_size))
for n in ('lib/x86_64/libstarry_magic.so',):
    if n in nb: print("   ! 注意：包内还有 %s" % n)
PY

echo "-- 产物 --"
( md5 -q "$OUT" 2>/dev/null || md5sum "$OUT" | cut -d' ' -f1 ) | sed 's/^/  md5    = /'
( shasum -a 256 "$OUT" | cut -d' ' -f1 ) | sed 's/^/  sha256 = /'
echo "  size   = $(wc -c < "$OUT" | tr -d ' ') B"
echo "DONE: $OUT"
