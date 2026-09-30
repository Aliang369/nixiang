#!/usr/bin/env bash
# ============================================================
#  check_release_clean.sh — 干净正式包门禁（诊断代码不许进正式包）
#
#  来源：2026-09-30 伊莫 1.0.1 会话复盘（`10_docs/_state/规范优化_20260930_伊莫1.0.1会话复盘.md` P0-3）
#  背景：伊莫正式包要求「零调试零日志」，而调试期的诊断是**先进包、最后手术拆除**——
#        代价 115 行 / 35 处手术 + 一整轮 389 MB 重建重装，且手术本身有回归风险。
#        正解 = 诊断走 `jni/ModDiag.h` 的 DLOG 宏（正式包展开成 ((void)0)），
#        Lua 侧诊断 chunk 走 `jni/ModDiagLua.h`（正式包 MOD_DIAG_LUA_TEXT == ""）。
#        本脚本机械拦下任何**绕过这套机制**的诊断代码。
#
#  用法:
#    bash 04_scripts/_common/check_release_clean.sh --tag Yimo
#    bash 04_scripts/_common/check_release_clean.sh --tag Yimo --so <libstarry_magic.so>
#    bash 04_scripts/_common/check_release_clean.sh --src Games/YimoMod.cpp
#    bash 04_scripts/_common/check_release_clean.sh --tag Yimo --list   # 附带列出宏化诊断
#
#  判定:
#    · 命中 FORBIDDEN = 违规（手写诊断/裸日志，运行期真的会跑），
#      唯一豁免：包在 `#ifdef STARRY_DIAG` / `#if MOD_DIAG_LUA` 块内；
#      `#ifndef STARRY_DIAG` / `#if !MOD_DIAG_LUA` 块内属正式包路径 ⇒ 照查。
#    · `DLOG/DLOGN/DWARN` 不算违规（ModDiag.h 的宏本身就是隔离），只做统计。
#    · 注释不参与判定；`--so` 时附加二进制检查（不得含 moddiag.log / ym.log 等）。
#  退出码: 0 = 干净；1 = 有违规；2 = 用法错误
# ============================================================
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"

TAG=""
SO=""
LIST_ONLY=0
SRCS=()
NSRC=0        # ★ macOS bash 3.2 + set -u：空数组的 ${#arr[@]}/"${arr[@]}" 会报
              #   unbound variable ⇒ 一律用显式计数器判断，展开用 ${arr[@]+"${arr[@]}"}

usage() { sed -n '2,26p' "$0" >&2; exit 2; }

while [[ $# -gt 0 ]]; do
  case "$1" in
    --tag)  [[ $# -ge 2 ]] || usage; TAG="$2"; shift 2 ;;
    --so)   [[ $# -ge 2 ]] || usage; SO="$2"; shift 2 ;;
    --src)  [[ $# -ge 2 ]] || usage; SRCS[$NSRC]="$2"; NSRC=$((NSRC + 1)); shift 2 ;;
    --list) LIST_ONLY=1; shift ;;
    -h|--help) usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

if [[ $NSRC -eq 0 && -z "$TAG" && -z "$SO" ]]; then
  echo "ERROR: 需要 --tag <游戏Tag> / --src <文件> / --so <so> 至少一个" >&2; usage
fi
if [[ $NSRC -eq 0 && -n "$TAG" ]]; then
  while IFS= read -r f; do
    [[ -n "$f" ]] || continue
    SRCS[$NSRC]="$f"; NSRC=$((NSRC + 1))
  done < <(ls "$JNI/Games/${TAG}"*.cpp "$JNI/Games/${TAG}"*.h 2>/dev/null || true)
  if [[ $NSRC -eq 0 ]]; then
    echo "ERROR: 没找到扫描对象（--tag $TAG 未匹配 Games/${TAG}*.{cpp,h}）" >&2
    exit 2
  fi
fi

PATHS=()
NPATH=0
for ((i = 0; i < NSRC; i++)); do
  f="${SRCS[$i]}"
  [[ "$f" = /* ]] || f="$JNI/$f"
  PATHS[$NPATH]="$f"; NPATH=$((NPATH + 1))
done

# ---------- 静态扫描（注释剔除 + 块感知）----------
if [[ $NPATH -gt 0 ]]; then
python3 - "$LIST_ONLY" "${PATHS[@]}" <<'PY'
import re, sys
from pathlib import Path

list_only = sys.argv[1] == "1"
paths = sys.argv[2:]

# 违规：手写诊断 / 裸日志（正式包运行期真的会跑）
FORBIDDEN = [
    (r'\bio\.open\s*\(\s*"/sdcard',    '设备文件日志通道 io.open(/sdcard…)'),
    (r'\b(?:ym|moddiag)\.log\b',       '设备日志文件名'),
    (r'(?<![A-Za-z_])_dbg(?:N)?\s*\(', 'Lua 诊断函数 _dbg/_dbgN'),
    (r'(?<![A-Za-z_])LOG\s*\(',        '手写 LOG 分级诊断函数（应改用 DLOG/DLOGN）'),
    (r'LOG_LIMIT_EXTRA|__YM_N\b',      '日志上限表 / 诊断计数器'),
    (r'__android_log_print',           '裸 __android_log_print（应走 DLOG / 证据宏）'),
    (r'\bkModDiagLua\b',               '直接引用诊断 chunk 常量（应改用 MOD_DIAG_LUA_TEXT）'),
    (r'console\.log|print\s*\(\s*"\[dbg', 'Lua 调试打印'),
]
# 已是"编译期清空"的宏化诊断：不算违规，只统计（--list 时列出）
MACRO_DIAG = [r'(?<![A-Za-z_])DLOG[N]?\s*\(', r'(?<![A-Za-z_])DWARN\s*\(']
ALLOW = [r'MOD_EVIDENCE', r'LogErrThrottled', r'LOGI\(', r'LOGE\(', r'LOGW\(']

def strip_comments(text):
    """逐行剔注释（保留行数，便于报行号）；块注释跨行状态机。"""
    out, in_block = [], False
    for line in text.splitlines():
        s = line
        if in_block:
            if '*/' in s:
                s = s.split('*/', 1)[1]; in_block = False
            else:
                out.append(''); continue
        while True:
            i, j = s.find('/*'), s.find('//')
            if i != -1 and (j == -1 or i < j):
                if '*/' in s[i:]:
                    s = s[:i] + s[i:].split('*/', 1)[1]; continue
                s = s[:i]; in_block = True; break
            if j != -1:
                head = s[:j]
                if head.count('"') % 2 == 0:   # // 前引号平衡 ⇒ 是真注释
                    s = head
                break
            break
        out.append(s)
    return out

guard_re = re.compile(r'^\s*#\s*(ifdef|ifndef|if|elif|else|endif)\b(.*)$')
diag_word = re.compile(r'STARRY_DIAG|MOD_DIAG_LUA')
neg_re = re.compile(r'^\s*#\s*(?:ifndef\s+|if\s+!\s*(?:defined\s*\(?\s*)?)'
                    r'(?:STARRY_DIAG|MOD_DIAG_LUA)')

viol, macro = [], []
for p in paths:
    f = Path(p)
    if not f.is_file():
        print(f"ERROR: 文件不存在 {p}")
        sys.exit(2)
    raw = f.read_text(errors="replace").splitlines()
    code = strip_comments("\n".join(raw))
    stack = []
    for i, line in enumerate(code, 1):
        m = guard_re.match(line)
        if m:
            kind, rest = m.group(1), m.group(2)
            if kind in ("ifdef", "if", "ifndef"):
                stack.append(bool(diag_word.search(rest)) and not neg_re.match(line))
            elif kind in ("else", "elif"):
                if stack: stack[-1] = not stack[-1]
            elif kind == "endif":
                if stack: stack.pop()
            continue
        orig = raw[i - 1].strip()[:120] if i - 1 < len(raw) else line.strip()[:120]
        if any(re.search(rgx, line) for rgx in ALLOW):
            continue
        if any(re.search(rgx, line) for rgx in MACRO_DIAG):
            macro.append((p, i, orig)); continue
        guarded = any(stack)
        for rgx, desc in FORBIDDEN:
            if re.search(rgx, line):
                viol.append(("GUARDED" if guarded else "VIOLATION", p, i, desc, orig))
                break

bad = [v for v in viol if v[0] == "VIOLATION"]
if viol:
    print("--- 手写诊断清单（VIOLATION = 会进正式包；GUARDED = 已包在 STARRY_DIAG 块内）---")
    for tag, p, i, desc, text in viol:
        print(f"  {tag:9s} {p}:{i}  {desc}\n            {text}")
if list_only and macro:
    print("--- 宏化诊断 DLOG/DLOGN/DWARN（正式包编译期清空，仅诊断构建生效）---")
    for p, i, text in macro:
        print(f"  MACRO     {p}:{i}\n            {text}")
if bad:
    print(f"\nFAIL: {len(bad)} 处手写诊断会进入正式包（共扫 {len(paths)} 个文件）")
    sys.exit(1)
print(f"PASS: 干净包门禁（{len(paths)} 个文件：违规 0"
      + (f"，已守卫 {len(viol)}" if viol else "")
      + (f"，宏化诊断 {len(macro)} 处（编译期清空）" if macro else "") + "）")
PY
fi

if [[ "$LIST_ONLY" == "1" ]]; then exit 0; fi

# ---------- 二进制附加检查（可选）----------
if [[ -n "$SO" ]]; then
  [[ -f "$SO" ]] || { echo "ERROR: --so 不存在: $SO" >&2; exit 2; }
  hit=0
  for pat in 'moddiag\.log' 'ym\.log' 'StarryDiag' 'logcfg'; do
    if LC_ALL=C grep -a -q "$pat" "$SO"; then
      echo "FAIL: 正式包 so 内出现诊断痕迹 $pat  ($SO)"
      hit=1
    fi
  done
  [[ "$hit" == "0" ]] || exit 1
  echo "PASS: so 二进制干净（无诊断 chunk / 无设备日志路径）  $(basename "$SO")"
fi
