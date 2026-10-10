#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
lint_shell_traps.py — shell 环境坑位静态检查（2026-10-10 复盘 P2）

为什么要有：本会话（明日方舟 2.7.81）被同一批「跑起来才炸」的坑拖了 2 轮返工，
它们都能**静态**查出来，不该等到装完机才发现：

  R1 用了 `rg` 但本机没装            → 静默空结果，误判「没命中」（曾白跑一次判读）  ERROR
  R2 设备侧算术 `$((0x…))`           → Android mksh 的算术是 32 位，地址被截断        WARN
  R3 未加 -F/-E 的 BRE 方括号 grep    → `grep -c "[tag] …"` 里 `[tag]` 是字符类，恒 0   WARN
  R4 直接对 .apk 做哈希              → 1.8 GB 包哈希 >60s，中断后又要重来             WARN
  R5 用 `timeout`                    → macOS 无此命令（GNU coreutils 才有）          WARN
  R6 `grep -c` 的结果直接进 `&&`     → 无命中时 grep rc=1 ⇒ `&&` 短路，后面的活白跑    WARN

用法:
  python3 04_scripts/_common/lint_shell_traps.py <文件.py|.sh|.mk...>
  python3 04_scripts/_common/lint_shell_traps.py --selfcheck
退出码: 0=只有 WARN 或干净；1=存在 ERROR；2=用法错
仅在 .sh/.bash/.mk/.py 上跑（其它扩展名直接放行）。
"""
import os
import re
import shutil
import sys
import tempfile

EXTS = (".sh", ".bash", ".mk", ".py", ".bashrc")

R2_DEVICE_HINT = re.compile(r"(adb[^\n]*\bshell\b|su\s+-c|\bsh\s+-c\s+['\"])")
# R4: 哈希命令必须真的出现在 .apk 参数之前（否则只是文案里提到了 md5/apk，别误报）
R4_RE = re.compile(r"(^|[\s|&;(])(md5|md5sum|shasum|sha256sum)\s+(?:-[A-Za-z]+\s+)*[^|;&]*\.apk")


def rule_r1(text):
    """用了 rg 但本机没装。"""
    if shutil.which("rg"):
        return []
    out = []
    for i, ln in enumerate(text.splitlines(), 1):
        s = ln.strip()
        if s.startswith("#"):
            continue
        if re.search(r"(^|[^-A-Za-z0-9_])rg\s", ln):
            out.append(("ERROR", "R1 rg不可用", i,
                        "本机没有 rg：这一行会静默失败（曾把「没命中」当成「没问题」）⇒ 改 grep -E / grep -aF"))
    return out


def rule_r2(text):
    """只有脚本会把命令下发到设备 shell 时，$((0x…)) 才有 32 位截断风险。"""
    if not R2_DEVICE_HINT.search(text):
        return []
    out = []
    for i, ln in enumerate(text.splitlines(), 1):
        if "$((0x" in ln or "$(( 0x" in ln:
            out.append(("WARN", "R2 设备侧算术", i,
                        "$((0x…)) 在 Android mksh 里是 32 位，地址会被截断 ⇒ 用 printf/awk 算"))
    return out


def rule_r3(text):
    """grep 的字面串以 [ 开头却没加 -F/-E ⇒ BRE 字符类。"""
    out = []
    pat = re.compile(r'\bgrep\b((?:\s+-[A-Za-z]+)*)\s+(--\s+)?"(\[[^"]*)"')
    for i, ln in enumerate(text.splitlines(), 1):
        s = ln.strip()
        if s.startswith("#"):
            continue
        for m in pat.finditer(ln):
            flags = m.group(1) or ""
            if ("F" in flags) or ("E" in flags) or ("e" in flags) or ("P" in flags):
                continue
            inner = m.group(3)[1:]
            # `[^…]`（取非）与含 `-` 的区间（如 `[^ -~]`、`[a-z]`）是**有意**用字符类，别误报
            if inner.startswith("^") or "-" in inner:
                continue
            out.append(("WARN", "R3 BRE方括号", i,
                        '%s 里 "%s" 在 BRE 下是字符类（恒不中）⇒ 加 -F 或 -E' % (m.group(0).strip(), m.group(3))))
    return out


def rule_r4(text):
    """直接对大包做哈希。"""
    out = []
    for i, ln in enumerate(text.splitlines(), 1):
        s = ln.strip()
        if s.startswith("#"):
            continue
        if R4_RE.search(ln) is not None:
            out.append(("WARN", "R4 大包哈希", i,
                        "对 .apk 直接哈希：1.8 GB 包会超 60s 命令上限 ⇒ 先比 size，或后台跑/写进阶段产物"))
    return out


def rule_r5(text):
    """macOS 没有 timeout。"""
    out = []
    for i, ln in enumerate(text.splitlines(), 1):
        s = ln.strip()
        if s.startswith("#"):
            continue
        if re.search(r"(^|\||&&|;|\btime\b)\s*timeout\s+[0-9]", ln):
            out.append(("WARN", "R5 timeout", i,
                        "本仓库在 macOS 上跑，没有 timeout(GNU) ⇒ 用后台任务 + 轮询，或 gtimeout"))
    return out


def rule_r6(text):
    """grep -c 的结果进 && 链。"""
    out = []
    for i, ln in enumerate(text.splitlines(), 1):
        s = ln.strip()
        if s.startswith("#"):
            continue
        if re.search(r"\bgrep\s+-[A-Za-z]*c[A-Za-z]*\b[^|]*&&", ln):
            out.append(("WARN", "R6 grep -c 短路", i,
                        "grep 无命中时 rc=1 ⇒ `&&` 后面的命令被跳过（曾整段检查被静默跳过）⇒ 改用 if 判 -n 或加 || true"))
    return out


RULES = (rule_r1, rule_r2, rule_r3, rule_r4, rule_r5, rule_r6)


def lint_text(path, text):
    hits = []
    for fn in RULES:
        try:
            hits.extend(fn(text))
        except Exception as e:  # 规则自身出错不能挡关，但要显式说明
            hits.append(("WARN", "LINT自身异常", 0, "%s: %s" % (fn.__name__, e)))
    hits.sort(key=lambda h: (h[2], h[1]))
    seen = set()
    lines = []
    for lvl, tag, ln, msg in hits:
        key = (lvl, tag, ln)
        if key in seen:
            continue
        seen.add(key)
        lines.append("%s [%s] %s:%d  %s" % ("✗ ERROR" if lvl == "ERROR" else "⚠ WARN ", tag, path, ln, msg))
    return lines, any(h[0] == "ERROR" for h in hits)


def selfcheck():
    """两组夹具：坑文件必须被逐条抓出、干净文件必须零命中。"""
    bad = """#!/usr/bin/env bash
set -uo pipefail
X=$(rg -n foo file.txt)
A=$((0x754ea38000))
adb -s S shell "echo hi"
N=$(grep -c "[mrfzz] hooked x" log.txt) && echo "$N"
md5 -q out.apk
timeout 30 bash slow.sh
"""
    good = """#!/usr/bin/env bash
set -uo pipefail
X=$(grep -aF -- "[mrfzz] hooked " log.txt | wc -l | tr -d ' ')
A=$(printf '%d' 0x754ea38000)
N=$(grep -acF -- "[mrfzz] hooked x" log.txt || true)
"""
    fail = 0
    d = tempfile.mkdtemp(prefix="lint_traps_")
    try:
        p_bad = os.path.join(d, "bad.sh")
        p_good = os.path.join(d, "good.sh")
        open(p_bad, "w", encoding="utf-8").write(bad)
        open(p_good, "w", encoding="utf-8").write(good)
        lines_bad, err_bad = lint_text(p_bad, bad)
        tags_bad = set(l.split("[")[1].split("]")[0] for l in lines_bad)
        need = {"R2 设备侧算术", "R3 BRE方括号", "R4 大包哈希", "R5 timeout", "R6 grep -c 短路"}
        if not shutil.which("rg"):
            need.add("R1 rg不可用")
        missing = need - tags_bad
        if missing:
            print("✗ 自检失败：夹具没被抓出 → %s" % ", ".join(sorted(missing)))
            for l in lines_bad:
                print("    " + l)
            fail = 1
        else:
            print("✓ 自检[坑夹具] 6 条规则全部命中（ERROR=%s）" % ("是" if err_bad else "否"))
        lines_good, err_good = lint_text(p_good, good)
        if lines_good or err_good:
            print("✗ 自检失败：干净文件被误报")
            for l in lines_good:
                print("    " + l)
            fail = 1
        else:
            print("✓ 自检[干净文件] 零命中")
    finally:
        for f in os.listdir(d):
            os.unlink(os.path.join(d, f))
        os.rmdir(d)
    if fail:
        print("❌ 坑位 lint 自检失败：误报/漏报都会让「检查关」失去意义，先修规则。")
        return 1
    print("✅ 坑位 lint 自检通过。")
    return 0


def main(argv):
    if len(argv) >= 2 and argv[1] == "--selfcheck":
        return selfcheck()
    if len(argv) < 2:
        print(__doc__)
        return 2
    rc = 0
    files = [a for a in argv[1:] if not a.startswith("--")]
    for f in files:
        if not os.path.isfile(f):
            print("⚠ WARN  [缺失] %s" % f)
            continue
        if not f.endswith(EXTS):
            continue
        try:
            text = open(f, encoding="utf-8", errors="replace").read()
        except Exception as e:
            print("⚠ WARN  [读失败] %s: %s" % (f, e))
            continue
        lines, has_err = lint_text(f, text)
        for l in lines[:12]:
            print("       " + l)
        if len(lines) > 12:
            print("       …（还有 %d 条，同类规则已去重）" % (len(lines) - 12))
        if has_err:
            rc = 1
    if rc == 0:
        pass
    return rc


if __name__ == "__main__":
    sys.exit(main(sys.argv))
