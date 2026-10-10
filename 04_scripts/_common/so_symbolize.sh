#!/usr/bin/env bash
# ================================================================
#  so_symbolize.sh — 把崩溃日志里的地址符号化到具体 so 符号 + 偏移
#
#  用途: logcat / tombstone 里 `pc 00000073a4c44b98  libstarry_magic.so`
#        这类行，先减模块基址拿到 so 内偏移，再查出是哪个函数、偏移多少。
#
#  用法:
#    bash 04_scripts/_common/so_symbolize.sh <so文件> <十六进制偏移或绝对地址> [更多地址...]
#    bash 04_scripts/_common/so_symbolize.sh --so <so> --addr 0x73a4c44b98 [--addr 0x…]
#    bash 04_scripts/_common/so_symbolize.sh <so> 0x73a4c44b98 --base 0x73a439d000
#    bash 04_scripts/_common/so_symbolize.sh --help
#
#  地址口径: 本仓 arm64-v8a 的 so 首段 p_offset=0 p_vaddr=0，故「十六进制小数值」按
#            文件偏移=VA 处理。若给的是运行期映射地址（明显大于文件大小），需先减模块基址
#            （maps/logcat 里 libxxx.so 的起始），或用 --base 0x… 让脚本自动减。
#
#  解析优先级: NDK llvm-symbolizer → 同目录 llvm-readelf 查最近符号 → 系统 nm/otool
#  退出码: 0=正常（含"没查到符号但工具可用"）；2=用法错/so 不存在；4=本机无任何符号化工具
#
#  输出:
#    0x73a4c44b98 JNI_OnLoad+0xc (0xc)
#    SUMMARY addrs=1 resolved=1
#    BUILD_MAP md5=… size=… buildid=… note=…        # 有 04_scripts/无悔华夏/so_build_map.txt 时
#    SYMBOLIZE_OK so=libstarry_magic.so size=219280
#
#  兼容性: macOS 自带 bash 3.2；数值比较一律走 python3（macOS awk 不认 0x 字面量）
#  铁律: 只读 so，绝不动包/不动设备；查不到符号说明 so 已 strip 且地址落在无符号区。
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

usage() {
  cat <<'EOF'
用法:
  bash 04_scripts/_common/so_symbolize.sh <so文件> <十六进制偏移> [更多地址...]
  bash 04_scripts/_common/so_symbolize.sh --so <so> --addr 0x… [--addr 0x…] [--base 0x…]

选项:
  --so <文件>     目标 .so（也可作为第一个位置参数给出）
  --addr <hex>    待符号化地址，可重复（也可作为后续位置参数给出）
  --base <hex>    运行期模块基址；给了就先做 addr-base
  --help          打印本用法

退出码: 0=正常；2=用法错/文件不存在；4=本机找不到任何符号化工具
EOF
}

SO=""
BASE="0x0"
ADDRS=()

while [ $# -gt 0 ]; do
  case "$1" in
    --so)      [ $# -ge 2 ] || { echo "未知参数: $1 缺参数" >&2; exit 2; }; SO="$2"; shift 2 ;;
    --so=*)    SO="${1#--so=}"; shift ;;
    --addr)    [ $# -ge 2 ] || { echo "未知参数: $1 缺参数" >&2; exit 2; }; ADDRS+=("$2"); shift 2 ;;
    --addr=*)  ADDRS+=("${1#--addr=}"); shift ;;
    --base)    [ $# -ge 2 ] || { echo "未知参数: $1 缺参数" >&2; exit 2; }; BASE="$2"; shift 2 ;;
    --base=*)  BASE="${1#--base=}"; shift ;;
    -h|--help) usage; exit 0 ;;
    -*)        echo "未知参数: $1" >&2; usage >&2; exit 2 ;;
    *)
      if [ -z "$SO" ]; then
        SO="$1"
      else
        ADDRS+=("$1")
      fi
      shift
      ;;
  esac
done

if [ -z "$SO" ]; then
  echo "错误: 未指定 so 文件" >&2
  usage >&2
  exit 2
fi
if [ ! -f "$SO" ]; then
  echo "错误: so 文件不存在: ${SO}" >&2
  exit 2
fi
if [ "${#ADDRS[@]}" -lt 1 ]; then
  echo "错误: 未指定任何地址" >&2
  usage >&2
  exit 2
fi

# 十六进制合法性校验（防 $(( )) 因非法字面量直接炸）
for a in ${ADDRS[@]+"${ADDRS[@]}"}; do
  h="${a#0x}"
  h="${h#0X}"
  case "$h" in
    ''|*[!0-9a-fA-F]*) echo "错误: 不是合法十六进制地址: ${a}" >&2; exit 2 ;;
  esac
done
bh="${BASE#0x}"
bh="${bh#0X}"
case "$bh" in
  ''|*[!0-9a-fA-F]*) echo "错误: 不是合法十六进制基址: ${BASE}" >&2; exit 2 ;;
esac

SIZE="$(wc -c < "$SO" | tr -d ' ')"

# ---------- 工具发现：NDK 优先，其次 PATH ----------
BIN_SYM=""
BIN_READELF=""
for pre in darwin-x86_64 darwin-arm64 linux-x86_64; do
  for ndk in "${NDK_HOME:-}" "${ANDROID_NDK_HOME:-}" \
             "$HOME/Library/Android/sdk/ndk/27.0.12077973" \
             "$HOME/Library/Android/sdk/ndk/26.1.10909125"; do
    [ -n "$ndk" ] || continue
    d="${ndk}/toolchains/llvm/prebuilt/${pre}/bin"
    if [ -z "$BIN_SYM" ] && [ -x "${d}/llvm-symbolizer" ]; then
      BIN_SYM="${d}/llvm-symbolizer"
    fi
    if [ -z "$BIN_READELF" ] && [ -x "${d}/llvm-readelf" ]; then
      BIN_READELF="${d}/llvm-readelf"
    fi
  done
done
[ -n "$BIN_SYM" ] || BIN_SYM="$(command -v llvm-symbolizer || true)"
[ -n "$BIN_READELF" ] || BIN_READELF="$(command -v llvm-readelf || true)"
NM="$(command -v nm || true)"
OTOOL="$(command -v otool || true)"

if [ -z "${BIN_SYM}" ] && [ -z "${BIN_READELF}" ] && [ -z "${NM}" ] && [ -z "${OTOOL}" ]; then
  echo "SYMBOLIZE_UNAVAILABLE"
  exit 4
fi

echo "TOOL symbolizer=${BIN_SYM:--} readelf=${BIN_READELF:--} nm=${NM:--} otool=${OTOOL:--}"

python3 - "$SO" "$SIZE" "$BASE" "$BIN_SYM" "$BIN_READELF" "$NM" "$OTOOL" \
  ${ADDRS[@]+"${ADDRS[@]}"} <<'PY'
# -*- coding: utf-8 -*-
import bisect
import os
import re
import subprocess
import sys

so = sys.argv[1]
size = int(sys.argv[2])
base = int(sys.argv[3], 16)
symtool = sys.argv[4]
readelf = sys.argv[5]
nm = sys.argv[6]
otool = sys.argv[7]
addrs = sys.argv[8:]

basename = os.path.basename(so)


def run(cmd):
    try:
        p = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        return p.stdout.decode("utf-8", "replace")
    except Exception:
        return ""


# ---------- 符号表：readelf → nm → otool ----------
RE_READELF = re.compile(
    r"^\s*\d+:\s+([0-9a-fA-F]+)\s+\d+\s+(\w+)\s+(\w+)\s+(\w+)\s+(\S+)\s+(\S*)")
RE_NM = re.compile(r"^([0-9a-fA-F]+)\s+\S\s+(\S+)")


def parse_syms(text, kind):
    out = []
    for ln in text.splitlines():
        m = RE_READELF.match(ln)
        if m:
            val = int(m.group(1), 16)
            ndx = m.group(5)
            name = m.group(6)
            if ndx == "UND" or val == 0 or not name:
                continue
            out.append((val, name))
            continue
        if kind != "readelf":
            m2 = RE_NM.match(ln)
            if m2:
                val = int(m2.group(1), 16)
                name = m2.group(2)
                if val != 0 and name and name != "no":
                    out.append((val, name))
    return out


syms = []
if readelf:
    syms = parse_syms(run([readelf, "--syms", so]), "readelf")
if not syms and nm:
    syms = parse_syms(run([nm, "-n", so]), "nm")
if not syms and otool:
    syms = parse_syms(run([otool, "-Iv", so]), "otool")
syms.sort()
sym_vals = [s[0] for s in syms]


def nearest(addr):
    i = bisect.bisect_right(sym_vals, addr) - 1
    if i < 0:
        return None, 0
    v, n = syms[i]
    return n, addr - v


# ---------- llvm-symbolizer：一次批量传入（不加 --functions，否则 strip 后恒 ??） ----------
def symbolize_batch(hexes):
    if not symtool or not hexes:
        return [(None, None)] * len(hexes)
    out = run([symtool, "--obj=" + so, "--demangle"] + hexes)
    lines = [l.strip() for l in out.splitlines() if l.strip() != ""]
    if len(lines) == 2 * len(hexes):
        res = []
        for i in range(len(hexes)):
            name = lines[2 * i]
            col = None
            if name not in ("??", ""):
                loc = lines[2 * i + 1]
                if loc and not loc.startswith("??"):
                    col = loc
                res.append((name, col))
            else:
                res.append((None, None))
        return res
    res = []
    for h in hexes:
        l2 = [l.strip() for l in run([symtool, "--obj=" + so, "--demangle", h]).splitlines()
              if l.strip() != ""]
        if l2 and l2[0] not in ("??", ""):
            col = l2[1] if len(l2) > 1 and not l2[1].startswith("??") else None
            res.append((l2[0], col))
        else:
            res.append((None, None))
    return res


hexes = [hex(int(a, 16)) for a in addrs]
symsym = symbolize_batch(hexes)

hinted = False
in_range_any = False
resolved = 0
for raw, eff_hex, (sname, scol) in zip(addrs, hexes, symsym):
    eff = int(eff_hex, 16) - base
    shown_addr = int(raw, 16)
    if eff < 0 or eff >= size:
        if not hinted:
            print("HINT 运行期地址请先减模块基址（maps 里 %s 起始），"
                  "或用 --base 0x… 让它自动减" % basename)
            hinted = True
        print("0x%x ??+0x0 (0x%x OUT_OF_RANGE size=0x%x)" % (shown_addr, eff, size))
        continue
    tname, toff = nearest(eff)
    in_range_any = True
    if sname:
        name = sname
        off = toff if tname == sname else 0
    elif tname:
        name = tname
        off = toff
    else:
        name = "??"
        off = 0
    if name != "??":
        resolved += 1
    tail = ""
    if scol:
        tail = " @ " + scol
    print("0x%x %s+0x%x (0x%x)%s" % (shown_addr, name, off, eff, tail))

print("SUMMARY addrs=%d resolved=%d" % (len(addrs), resolved))
if resolved == 0 and syms and in_range_any:
    print("WARN 该 so 符号区未命中任何地址（地址可能落在 .plt/桩代码区或已被 strip 到最少符号）")
PY

# ---------- 构建台账（无悔华夏）：顺带打一条，找不到就跳过 ----------
BUILD_MAP="${ROOT}/04_scripts/无悔华夏/so_build_map.txt"
if [ -f "${BUILD_MAP}" ]; then
  LAST_LINE="$(grep -v '^[[:space:]]*$' "${BUILD_MAP}" | tail -1)"
  if [ -n "${LAST_LINE}" ]; then
    b_md5="$(printf '%s\n' "${LAST_LINE}" | sed -n 's/.*md5=\([^ ]*\).*/\1/p')"
    b_size="$(printf '%s\n' "${LAST_LINE}" | sed -n 's/.*size=\([^ ]*\).*/\1/p')"
    b_bid="$(printf '%s\n' "${LAST_LINE}" | sed -n 's/.*buildid=\([^ ]*\).*/\1/p')"
    b_note="$(printf '%s\n' "${LAST_LINE}" | sed -n 's/.*note=\(.*\)$/\1/p')"
    echo "BUILD_MAP md5=${b_md5:--} size=${b_size:--} buildid=${b_bid:--} note=${b_note:--}"
  fi
fi

echo "SYMBOLIZE_OK so=${SO##*/} size=${SIZE}"
