#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
fkssj_check_js.py —— 疯狂水世界：把 FkssjMod.cpp 里所有裸 C++ raw string 里的 JS
（R"JS(...)JS"）抽出来逐个做 **语法检查**（node --check）。

为什么必须做：
  kPatchJs / kDumpJs 是嵌在 so 里的 JS 文本，编译期**不会**有任何语法校验
  （C++ 只当它是字符串）。一旦写错一个括号，编译照样通过、装机照样成功，
  但运行期 evalString 会静默失败 ⇒ 三功能全废，而且要到用户实测才发现。
  所以在编译前用 node 卡一道。

用法：
  python3 04_scripts/疯狂水世界/fkssj_check_js.py [FkssjMod.cpp 路径]
  退出码 0 = 全部通过（或本机无 node，跳过并告警）；1 = 有语法错误。
"""
import os
import re
import shutil
import subprocess
import sys
import tempfile

DEFAULT_SRC = os.path.join(
    "mod菜单", "starry-mod-menu", "app", "src", "main", "jni", "Games", "FkssjMod.cpp"
)

# R"JS( ... )JS" ；用非贪婪 + DOTALL 抓取
BLOCK_RE = re.compile(r'R"JS\((.*?)\)JS"', re.S)


def die(msg):
    sys.stderr.write(msg.rstrip() + "\n")
    sys.exit(1)


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_SRC
    if not os.path.isfile(src):
        die("ERROR: 找不到 %s" % src)

    with open(src, "r", encoding="utf-8") as f:
        text = f.read()

    blocks = BLOCK_RE.findall(text)
    if not blocks:
        die("ERROR: %s 里没有找到任何 R\"JS(...)JS\" 块 —— 抽取正则或源码结构变了，"
            "不能当作通过。" % src)

    node = shutil.which("node")
    if not node:
        sys.stderr.write("WARN: 本机没有 node，跳过 JS 语法检查（共 %d 块未校验）\n" % len(blocks))
        return 0

    tmpdir = tempfile.mkdtemp(prefix="fkssj_jscheck_")
    bad = 0
    for i, js in enumerate(blocks, 1):
        # 找出该块所在的 C++ 变量名，便于报错定位
        head = text[: text.find(js)] if False else ""
        path = os.path.join(tmpdir, "block%d.js" % i)
        with open(path, "w", encoding="utf-8") as f:
            f.write(js)
        r = subprocess.run([node, "--check", path], capture_output=True, text=True)
        first_line = js.strip().splitlines()[0][:70] if js.strip() else ""
        name = "block%d (首行: %s…)" % (i, first_line)
        if r.returncode == 0:
            print("OK   %s  %d 字节" % (name, len(js)))
        else:
            bad += 1
            print("FAIL %s" % name)
            sys.stderr.write(r.stderr.strip() + "\n")

    if bad:
        die("ERROR: %d/%d 块 JS 语法检查失败 —— 修好再编译，否则装机后补丁静默失效。"
            % (bad, len(blocks)))
    print("PASS: %d 块 JS 全部通过 node --check" % len(blocks))
    return 0


if __name__ == "__main__":
    sys.exit(main())
