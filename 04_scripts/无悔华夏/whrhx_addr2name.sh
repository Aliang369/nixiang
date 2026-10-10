#!/usr/bin/env bash
# whrhx_addr2name.sh —— 把「游戏 so 内的地址」翻成「类.方法名」
#
# 背景（2026-10-11 实测）：本包的 libil2cpp.so 首段 PT_LOAD 是 p_offset=0/p_vaddr=0，
# 因此 Il2CppDumper 输出的方法注释 `// RVA: 0x... Offset: 0x... VA: 0x...` 三值相等，
# 该数值既是「文件偏移」（喂 WhrhxMod.cpp 的 FileOffToVaBest）也是「llvm-objdump 反汇编时
# 的 VMA」。于是可以：
#   1) llvm-objdump -d --start-address=A --stop-address=B libil2cpp.so   看 bl 目标
#   2) 本脚本把 bl 目标地址批量翻成 类.方法名
# 这是「读不到方法体（dump.cs 方法体为空）时唯一能追调用链」的路径。
#
# 依赖: ① dump.cs（Il2CppDumper 产物，**不入库**，见 .gitignore）
#       ② 地址映射缓存（首次运行自动生成到 /tmp，约 20 万条，几秒）
#
# 用法:
#   whrhx_addr2name.sh 0x7016B98 0x7017094            # 逐个地址翻译
#   whrhx_addr2name.sh - < addrs.txt                  # 从 stdin 读地址（每行一个/可含杂字符）
#   llvm-objdump -d ... | grep -oE 'bl\s+0x[0-9a-f]+' | grep -oE '0x[0-9a-f]+' \
#       | whrhx_addr2name.sh -                        # 反汇编 → 调用目标名（常用组合）
#
# 环境变量: WHRHX_DUMP（默认 06_dumped/无悔华夏/current_20261011/runtime/il2cpp_dump/dump.cs）
set -uo pipefail

ROOT="${WHRHX_REPO_ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
DUMP="${WHRHX_DUMP:-$ROOT/06_dumped/无悔华夏/current_20261011/runtime/il2cpp_dump/dump.cs}"
MAP="${WHRHX_ADDRMAP:-/tmp/whrhx_addr_map.txt}"

if [[ ! -f "$DUMP" ]]; then
    echo "缺 dump.cs: $DUMP（可设 WHRHX_DUMP 指定）" >&2
    exit 2
fi

build_map() {
    echo "[addr2name] 生成地址映射（首次约 3-8 秒）: $MAP" >&2
    awk '
        /^public (class|struct|enum|interface)|^internal (class|struct|enum|interface)/ {
            cls = $3; sub(/:$/, "", cls); sub(/<.*/, "", cls); next
        }
        /^[ \t]*\/\/ RVA: / {
            if (match($0, /0x[0-9A-Fa-f]+/)) { rva = tolower(substr($0, RSTART, RLENGTH)); pend = 1 }
            next
        }
        pend && /\(/ {
            line = $0; sub(/^[ \t]+/, "", line)
            p = index(line, "("); head = substr(line, 1, p - 1)
            n = split(head, a, " "); nm = a[n]
            if (nm != "" && nm != "//") printf "%s\t%s.%s\n", rva, cls, nm
            pend = 0
        }
    ' "$DUMP" > "$MAP"
}

if [[ ! -s "$MAP" || "$DUMP" -nt "$MAP" ]]; then
    build_map
fi

norm() {  # 0X00abc / 0xabc / 000abc / 12345 → 0x 小写无前导零（对齐 llvm-objdump 的 0x 输出）
    local s="$1"
    s="${s//,/}"
    [[ "$s" =~ 0[xX]([0-9A-Fa-f]+) ]] || [[ "$s" =~ ^([0-9A-Fa-f]+)$ ]] || return 1
    printf '0x%x' "$((16#${BASH_REMATCH[1]}))"
}

lookup() {
    local a="$1" hit
    hit=$(awk -v k="$a" '$1 == k { print $2; exit }' "$MAP")
    if [[ -n "$hit" ]]; then
        printf '%s\t%s\n' "$a" "$hit"
    else
        printf '%s\t(未在 dump.cs 中登记——可能是 库内非 il2cpp 函数 / 内联桩 / 地址口径不符)\n' "$a"
    fi
}

if [[ "${1:-}" == "-" || $# -eq 0 ]]; then
    while IFS= read -r raw; do
        a=$(norm "$raw") || continue
        lookup "$a"
    done
else
    for raw in "$@"; do
        a=$(norm "$raw") || { echo "跳过非法地址: $raw" >&2; continue; }
        lookup "$a"
    done
fi
