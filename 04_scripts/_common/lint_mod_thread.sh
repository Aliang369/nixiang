#!/usr/bin/env bash
# ================================================================
#  lint_mod_thread.sh — mod so 的「线程 / 装钩」静态检查（构建前闸门）
#
#  背景（2026-10-11 P1-⑧ 收口）：Games/*.cpp 里最容易犯的三类错，
#  全都能在**编译前**静态查出来，不该等装机进游戏才炸/才掉帧：
#    R1-maps   装钩/同步路径里读 /proc/self/maps、走 ImageOf() 老路径
#              —— 逐次读 maps 是 syscall 且会命中 il2cpp 前置守卫（Whrhx 实测全 null）；
#                 替代品是同文件自有的 ImageOfRaw()（见 Games/WhrhxMod.cpp:556）。
#    R2-hotlog Dobby inline hook 回调（Hooked_*）里直接 LOGx → 逐帧写 logcat
#              ⇒ 掉帧甚至阻塞调用线程；一律走 HotLog(n)/HotLogEvery(ms) 限流。
#    R3-attach 裸 pthread 起的 mod 线程里直接 runtime_invoke，但同函数既没
#              thread_attach 也没 Ensure*ThreadAttached/domain_get()
#              ⇒ 不在 il2cpp 线程表里 invoke 会异常或 abort。
#    R4-prefix 装钩表的 tag 字段值不是本作前缀（TPL_LOG）
#              —— 该值以 %s 进日志行，值不是前缀 ⇒ 事后按前缀 grep 统计时整类日志被漏掉。
#              「R4-prefix 为什么这么窄」：试过宽口径「文件用了 TPL_LOG 就不许有裸 LOGx」，
#              全仓 74 处命中几乎全是 `LOGx("%s ...", tag)` 这种**前缀经由变量传入**的正常写法
#              （tag 的值才是问题所在），宽口径等于噪声 ⇒ 收窄为「只查装钩表 tag 字段的字面量」。
#              依据：2026-10-14 潮汐守望者会话，CxswzMod 的 kSlots 用 {"ExcuteOnce", …} 作 tag，
#              `grep \[Cxswz\]` 漏掉 HookByName 的 "… class miss … degrade" 一类行，功能判读多绕一轮。
#              **存量口径（2026-10-14 立规则时实测：全仓 221 处 / 21 文件）**：多游戏装钩表的 tag
#              值原本就是裸标签，属历史存量、**不是本轮回归**；各游戏在「下一轮动该文件」时顺手
#              改成 TPL_LOG（`--strict` 前先清掉本游戏自己那几处），不为清零单独开轮。
#
#  用法:
#    bash 04_scripts/_common/lint_mod_thread.sh                 # 扫默认目录 Games/*.cpp
#    bash 04_scripts/_common/lint_mod_thread.sh a.cpp b.cpp     # 扫指定文件（相对/绝对皆可）
#    bash 04_scripts/_common/lint_mod_thread.sh --strict        # 有违规 ⇒ exit 1
#    bash 04_scripts/_common/lint_mod_thread.sh --help
#
#  输出（每行一条，便于 grep；字段用空格分隔，故修复提示内不含空格）:
#    VIOLATION <相对路径>:<行号> <规则ID> <违规符号> <修复提示>
#    LINT_OK files=<n> rules=<r>                              # 仅无违规时
#    SUMMARY files=<n> violations=<n> suppressed=<n>           # 总是打印
#
#  豁免：被命中的那一行或其上一行含 `// lint:ok` 时跳过；
#        可跟规则 ID 精确豁免（`// lint:ok R2-hotlog`），不跟 ID 则跳过该行全部规则。
#
#  退出码: 0=无违规（或默认模式下只报告）；1=有违规且 --strict；2=用法错/读不到文件
#  兼容性: macOS 自带 bash 3.2（不用关联数组 / ${v,,} / mapfile / globstar）
#  铁律: 这是"改完 mod 代码"的机器关——新增 Hooked_* 与 mod 线程代码先跑它再编译。
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

STRICT=0
MODE="default"

usage() {
  cat <<'EOF'
用法:
  bash 04_scripts/_common/lint_mod_thread.sh [--strict] [文件.cpp ...]
  --strict   有违规时 exit 1（默认只报告，exit 0）
  --help     打印本用法

默认扫描目录: mod菜单/starry-mod-menu/app/src/main/jni/Games/*.cpp
规则: R1-maps(装钩路径读 maps/ImageOf) / R2-hotlog(Hooked_* 里裸 LOG)
      / R3-attach(runtime_invoke 同函数无 attach)
退出码: 0=无违规（默认模式下有违规也 0）；1=--strict 下有违规；2=用法错
EOF
}

while [ $# -gt 0 ]; do
  case "$1" in
    --strict)   STRICT=1; shift ;;
    -h|--help)  usage; exit 0 ;;
    -*)         echo "未知参数: $1" >&2; usage >&2; exit 2 ;;
    *)          MODE="files"; break ;;
  esac
done

if [ "$MODE" = "files" ] && [ $# -lt 1 ]; then
  usage >&2
  exit 2
fi

# python3 是仓库硬依赖（preflight.sh 也用它）；扫描逻辑用 heredoc 内嵌，不新增额外文件。
scan_rc=0
python3 - "$ROOT" "$MODE" "$@" <<'PY' || scan_rc=$?
# -*- coding: utf-8 -*-
import os
import re
import sys

ROOT = sys.argv[1]
MODE = sys.argv[2]
ARGS = sys.argv[3:]

RULE_IDS = ("R1-maps", "R2-hotlog", "R3-attach", "R4-prefix")

# 修复提示（不含空格，保证 VIOLATION 行的字段可空格切分）
HINTS = {
    "R1-maps": "装钩/同步路径禁用ImageOf()与/proc/self/maps读取，改用ImageOfRaw()或挪到一次性初始化",
    "R2-hotlog": "改用 HotLog(n)/HotLogEvery(ms) 限流（见 Includes/HotLog.h）",
    "R3-attach": "调用前 EnsureIl2cppThreadAttached（见 Includes/ModThread.h）",
    "R4-prefix": "日志前缀落到日志行里必须可grep：把该表的tag字段值改成TPL_LOG（本文件已定义），或让装钩函数直接传TPL_LOG",
}

# ---- R1: 装钩/同步路径里的调用 ----
R1_PATS = (
    (re.compile(r"(?<![A-Za-z0-9_])ImageOf\s*\("), "ImageOf("),
    (re.compile(r"fopen\s*\(\s*\"/proc/self/maps\""), "fopen(/proc/self/maps)"),
    (re.compile(r"(?<![A-Za-z0-9_])isLibraryLoaded\s*\("), "isLibraryLoaded("),
    (re.compile(r"(?<![A-Za-z0-9_])getLibraryAddressFromMaps\s*\("), "getLibraryAddressFromMaps("),
)
HOOKPATH_RE = re.compile(r"(Hook|Install|Resolve|Sync|Tick|Attach|Ensure)", re.IGNORECASE)

# ---- R2: hook 回调里的裸日志 ----
LOG_RE = re.compile(r"(?<![A-Za-z0-9_])(LOGI|LOGW|LOGE|LOGD|__android_log_print)\s*\(")
HOTLOG_OK = ("HotLog(", "HotLogEvery(")

# ---- R3: 跨线程 invoke 未 attach ----
INVOKE_RE = re.compile(r"(?<![A-Za-z0-9_])runtime_invoke\s*\(")
ATTACH_OK_RE = re.compile(r"thread_attach|Ensure[A-Za-z]*ThreadAttached|domain_get\s*\(")

# ---- R4: 日志前缀（口径见文件头「R4-prefix 为什么这么窄」）----
TPL_TOKEN = "TPL_LOG"
# 装钩表里的 tag 字段：值最终以 %s 进日志行，值不是 TPL_LOG 就 grep 不到前缀。
TAG_MEMBER_RE = re.compile(r"const\s+char\s*\*\s*tag\s*;")
STRUCT_OPEN_RE = re.compile(r"^\s*(?:typedef\s+)?struct\s+([A-Za-z_][A-Za-z_0-9]*)\s*\{?\s*$")
LIT_RE = re.compile(r'"((?:[^"\\]|\\.)*)"')

# ---- 豁免标记 ----
EXEMPT_RE = re.compile(r"//\s*lint:ok(?:\s+([A-Za-z0-9_\-]+))?")

# 函数签名识别：缩进 <= 4、以标识符开头、含 `(`；函数名 = 首个 `(` 前最后一个标识符
SIG_RE = re.compile(r"^([A-Za-z_][A-Za-z_0-9_<>,\*&\s]*?)\b([A-Za-z_][A-Za-z_0-9_]*)\s*\(")


def scan_line(src, state):
    """剔注释 / 剔字符串，三种视图 + 掩码（长度与原行一致，行号列号不变）。"""
    n = len(src)
    comment = [False] * n
    string = [False] * n
    i = 0
    while i < n:
        c = src[i]
        if state[0]:                      # 块注释中
            comment[i] = True
            if c == "*" and i + 1 < n and src[i + 1] == "/":
                comment[i + 1] = True
                state[0] = False
                i += 2
            else:
                i += 1
            continue
        if c == "/" and i + 1 < n and src[i + 1] == "/":
            for k in range(i, n):
                comment[k] = True
            break
        if c == "/" and i + 1 < n and src[i + 1] == "*":
            comment[i] = True
            comment[i + 1] = True
            state[0] = True
            i += 2
            continue
        if c == '"' or c == "'":          # 字符串 / 字符字面量（含转义）
            q = c
            string[i] = True
            i += 1
            while i < n:
                string[i] = True
                if src[i] == "\\":
                    if i + 1 < n:
                        string[i + 1] = True
                    i += 2
                    continue
                if src[i] == q:
                    i += 1
                    break
                i += 1
            continue
        i += 1
    kept = "".join(" " if comment[k] else src[k] for k in range(n))
    nostr = "".join(" " if (comment[k] or string[k]) else src[k] for k in range(n))
    return kept, nostr, string


# 大括号不是函数体的：控制流 / 容器（namespace、extern "C"、struct、初始化列表…）
# 这些块当「透明容器」处理——不产出函数块，但内部大括号照常配对，避免
# `namespace { ... }` 把整个文件里的函数一起吞掉（Games/*.cpp 有 67 个文件用到）。
CTRL_WORDS = ("if", "for", "while", "switch", "else", "do", "try", "catch", "return")
CONTAINER_HEADS = ("namespace", "extern", "class", "struct", "union", "enum",
                   "template", "typedef", "using")


def sig_text(nostrs, i, limit=30):
    """从第 i 行的 `{` 向上收集签名文本；遇语句结束（; { }）或预处理行即停。"""
    parts = []
    j = i
    while j >= 0 and (i - j) < limit:
        t = nostrs[j].strip()
        if j < i:
            if not t:
                j -= 1
                continue
            if t.startswith("#") or t.endswith(";") or t.endswith("{") or t.endswith("}"):
                break
        parts.insert(0, t)
        text = " ".join(parts)
        if "(" in text and ")" in text:
            break
        j -= 1
    return " ".join(parts)


def sig_name(text):
    """返回 (函数名, 是否容器)。容器 = 不是函数体的大括号块。"""
    t = text.strip()
    if not t:
        return None, True
    head = t.split()[0]
    if head in CONTAINER_HEADS:
        return None, True
    m = SIG_RE.search(t)
    if not m:
        return None, True
    nm = m.group(2)
    if nm in CTRL_WORDS:
        return None, True
    return nm, False


def find_blocks(nostrs):
    """按大括号配平切函数体：(起始行0基, 结束行0基, 函数名)。"""
    blocks = []
    stack = []
    for i, line in enumerate(nostrs):
        for ch in line:
            if ch == "{":
                nm, is_container = sig_name(sig_text(nostrs, i))
                stack.append((None if is_container else nm, i))
            elif ch == "}":
                if stack:
                    nm, open_line = stack.pop()
                    if nm:
                        blocks.append((open_line, i, nm))
    return blocks


def is_exempt(raw, line_no, rule):
    for cand in (line_no, line_no - 1):
        if cand < 1 or cand > len(raw):
            continue
        m = EXEMPT_RE.search(raw[cand - 1])
        if m:
            rid = m.group(1)
            if not rid or rid.lower() == rule.lower():
                return True
    return False


def rel_of(path):
    r = os.path.relpath(path, ROOT)
    if r.startswith(".."):
        return path
    return r


def _prefix_structs(nostrs):
    """形如 `struct HookSlot { const char* tag; ... }` 的结构体名（tag 字段 = 日志前缀来源）。"""
    names = []
    cur = None
    for ln in nostrs:
        m = STRUCT_OPEN_RE.match(ln)
        if m:
            if TAG_MEMBER_RE.search(ln):
                names.append(m.group(1))
                cur = None
            else:
                cur = m.group(1)
            continue
        if cur is not None:
            if TAG_MEMBER_RE.search(ln):
                names.append(cur)
                cur = None
            elif re.match(r"^\s*\}", ln):
                cur = None
    return names


def r4_prefix_hits(nostrs, kepts):
    """返回 [(行号, 字面量片段)]：装钩表首字段字面量不以 '[' 开头（即不是本作前缀）。"""
    structs = _prefix_structs(nostrs)
    if not structs:
        return []
    hits = []
    for name in structs:
        arr_re = re.compile(r"\b" + re.escape(name) + r"\b[^=;]*\[\s*\]\s*=\s*\{")
        for i, ln in enumerate(nostrs):
            if not arr_re.search(ln):
                continue
            depth = ln.count("{") - ln.count("}")
            j = i + 1
            while j < len(nostrs) and depth > 0:
                body = nostrs[j]
                if depth == 1 and body.lstrip().startswith("{"):
                    for k in range(j, min(j + 3, len(nostrs))):
                        m = LIT_RE.search(kepts[k])
                        if m:
                            lit = m.group(1)
                            if lit and not lit.startswith("["):
                                hits.append((k + 1, '"' + lit[:16] + '"'))
                            break
                        if nostrs[k].lstrip().startswith("}"):
                            break
                depth += body.count("{") - body.count("}")
                j += 1
    return hits


def analyse(path):
    out = []
    suppressed = 0
    raw = open(path, encoding="utf-8", errors="replace").read().splitlines()
    has_tpl = any(TPL_TOKEN in ln for ln in raw)
    state = [False]
    kepts, nostrs, strmasks = [], [], []
    for ln in raw:
        k, ns, st = scan_line(ln, state)
        kepts.append(k)
        nostrs.append(ns)
        strmasks.append(st)

    for (b0, b1, name) in find_blocks(nostrs):
        if not name:
            continue
        body_nostr = "\n".join(nostrs[b0:b1 + 1])
        hits = []

        # R1 —— 只在「装钩/同步路径」（函数名含 Hook/Install/Resolve/Sync/Tick/Attach/Ensure）里查
        if HOOKPATH_RE.search(name):
            for j in range(b0, b1 + 1):
                found = None
                for pat, sym in R1_PATS:
                    m = pat.search(kepts[j])
                    if m is None:
                        continue
                    if strmasks[j][m.start()]:      # 命中的是字符串字面量内容，不算
                        continue
                    found = sym
                    break
                if found:
                    hits.append((j + 1, "R1-maps", found))

        # R2 —— hook 回调（Hooked_*）里出现裸日志、且同函数没有 HotLog 闸
        if name.startswith("Hooked_"):
            if not any(tok in body_nostr for tok in HOTLOG_OK):
                for j in range(b0, b1 + 1):
                    m = LOG_RE.search(nostrs[j])
                    if m:
                        hits.append((j + 1, "R2-hotlog", m.group(1) + "("))

        # R3 —— runtime_invoke 但同函数没有任何 attach / domain_get
        if INVOKE_RE.search(body_nostr) and not ATTACH_OK_RE.search(body_nostr):
            for j in range(b0, b1 + 1):
                if INVOKE_RE.search(nostrs[j]):
                    hits.append((j + 1, "R3-attach", "runtime_invoke("))

        for (ln_no, rule, sym) in hits:
            if is_exempt(raw, ln_no, rule):
                suppressed += 1
            else:
                out.append((ln_no, rule, sym))

    # R4 —— 装钩表 tag 字段必须是本作前缀（口径见文件头「R4-prefix 为什么这么窄」）。
    if has_tpl:
        for (ln_no, sym) in r4_prefix_hits(nostrs, kepts):
            if is_exempt(raw, ln_no, "R4-prefix"):
                suppressed += 1
            else:
                out.append((ln_no, "R4-prefix", sym))

    out.sort(key=lambda v: (v[0], v[1]))
    return out, suppressed


def main():
    if MODE == "default":
        gdir = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main",
                            "jni", "Games")
        if not os.path.isdir(gdir):
            print("LINT_ERR 默认扫描目录不存在: %s" % gdir)
            return 2
        files = sorted(os.path.join(gdir, f) for f in os.listdir(gdir) if f.endswith(".cpp"))
    else:
        files = []
        for a in ARGS:
            p = a if os.path.isabs(a) else os.path.join(os.getcwd(), a)
            if not os.path.isfile(p):
                print("LINT_ERR 文件不存在: %s" % a)
                return 2
            files.append(p)
        if not files:
            print("LINT_ERR 未指定待扫文件")
            return 2

    if not files:
        print("LINT_ERR 无 .cpp 可扫（默认目录为空？）")
        return 2

    viol = 0
    supp = 0
    for p in files:
        try:
            hits, s = analyse(p)
        except Exception as e:
            print("LINT_ERR 扫描失败 %s: %s" % (p, e))
            return 2
        supp += s
        rel = rel_of(p)
        for (ln_no, rule, sym) in hits:
            print("VIOLATION %s:%d %s %s %s" % (rel, ln_no, rule, sym, HINTS[rule]))
            viol += 1

    if viol == 0:
        print("LINT_OK files=%d rules=%d" % (len(files), len(RULE_IDS)))
    print("SUMMARY files=%d violations=%d suppressed=%d" % (len(files), viol, supp))
    return 1 if viol > 0 else 0


try:
    sys.exit(main())
except SystemExit:
    raise
except Exception as exc:                   # 意外异常按「扫描失败」处理，不当成"有违规"
    import traceback
    traceback.print_exc()
    print("LINT_ERR 扫描器内部异常: %s" % exc)
    sys.exit(2)
PY

case "$scan_rc" in
  0)
    echo "✓ lint 通过：无违规（${MODE} 模式）"
    exit 0
    ;;
  1)
    if [ "$STRICT" = "1" ]; then
      echo "✗ lint 违规（--strict）：修完再编译；豁免请在本行或上一行写「// lint:ok <规则ID>」"
      exit 1
    fi
    echo "⚠ lint 有违规（默认模式只报告，exit 0）；要让构建硬失败请加 --strict"
    exit 0
    ;;
  *)
    echo "✗ lint 未完成（rc=${scan_rc}）：用法错或扫描失败" >&2
    exit 2
    ;;
esac
