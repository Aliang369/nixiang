#!/usr/bin/env bash
# cwjh splice 离线主机模拟（权威版）——直接从 CwjhMod.cpp 抽出真源码编译，杜绝转写误差。
#
# 为什么需要它：20260921 手写 Python 镜像与真 C++ 在括号上出现偏差（+2423 vs 真值 +2555），
#   而 node --check 对这类偏差不敏感（`if(a,b,9&&x){…}` 也合法）。唯一可靠的闭环是：
#       主机编译真 .cpp 得到的 delta  ==  设备运行时 splice delta
#
# 用法：
#   bash scripts/cwjh_splice_hostsim.sh <game.js> [out.js]
# 输出：逐锚点命中（人读）、total delta（与运行时逐字节对齐）、node --check 结果
set -euo pipefail

SRC_CPP="${SRC_CPP:-$(cd "$(dirname "$0")/.." && pwd)/app/src/main/jni/Games/CwjhMod.cpp}"
IN="${1:?usage: cwjh_splice_hostsim.sh <game.js> [out.js]}"
OUT="${2:-/tmp/cwjh_hostsim_out.js}"
WORK="$(mktemp -d /tmp/cwjh_hostsim.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

python3 - "$SRC_CPP" "$WORK/host_splice.cpp" <<'PY'
import re, sys
cpp, dst = sys.argv[1], sys.argv[2]
lines = open(cpp, encoding='utf-8').read().split('\n')

def block(start_pred):
    """从匹配 start_pred 的行开始，取到第一行「列 0 的单个 }」为止。
    不能用花括号配平：函数体内大量 JS 字符串自带 { }（adsPlay 注入块天然不配平）。"""
    for i, l in enumerate(lines):
        if start_pred(l):
            for j in range(i + 1, len(lines)):
                if lines[j].rstrip() == '}':
                    return '\n'.join(lines[i:j+1])
            raise SystemExit('unterminated function from line %d' % (i+1))
    raise SystemExit('not found: %r' % start_pred)

macros = [l for l in lines if l.startswith('#define CWJH_')]
assert any('CWJH_RET' in m for m in macros), 'CWJH_RET 宏未找到'
body = block(lambda l: l.startswith('static void ReplaceAll')) + '\n\n' + \
       block(lambda l: l.startswith('static std::string SpliceCombatJs'))

head = '#include <string>\n#include <cstring>\n#include <cstdio>\n#include <cstdlib>\n#include <functional>\n' + '\n'.join(macros) + '\n'
main = '''
int main(int argc, char** argv) {
    FILE* f = fopen(argv[1], "rb"); if (!f) { perror("open"); return 1; }
    fseek(f, 0, SEEK_END); long n = ftell(f); fseek(f, 0, SEEK_SET);
    char* b = (char*)malloc(n + 1); size_t got = fread(b, 1, n, f); b[got] = 0; fclose(f);
    std::string out = SpliceCombatJs(b);
    FILE* g = fopen(argv[2], "wb"); fwrite(out.data(), 1, out.size(), g); fclose(g);
    printf("in=%ld out=%zu delta=%+ld\\n", n, out.size(), (long)out.size() - n);
    // 逐锚点报告：对各锚点做一次"是否仍在源里"的探测（source 侧）
    return 0;
}
'''
open(dst, 'w', encoding='utf-8').write(head + body + main)
print('  source: %s (%d macros)' % (cpp, len(macros)))
PY

clang++ -std=c++17 -O0 -o "$WORK/host_splice" "$WORK/host_splice.cpp"
"$WORK/host_splice" "$IN" "$OUT"
if command -v node >/dev/null 2>&1; then
  node --check "$OUT" && echo "  node --check PASS"
fi
echo "  out: $OUT  sha256=$(shasum -a 256 "$OUT" | cut -c1-16)"
