#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dexprobe.py — 从 classes.dex / dexdump.txt 里挖「登录面」证据。

用法:
  python dexprobe.py cn   <classes.dex>                 # 提取含中文 UI 关键词的字符串
  python dexprobe.py cls  <dexdump.txt>  <ClassDesc>    # 打印某类的 方法名 + const-string + const-class
  python dexprobe.py find <dexdump.txt>  <regex>        # 在 dexdump 里搜正则（带类上下文行号）
"""
import re
import sys

CN_KW = ["登录", "帐号", "账号", "游客", "一键", "网页", "微信", "密码", "注册",
         "退出", "切换", "授权", "手机", "渠道", "实名", "公告", "服务器"]

CJK = (rb"(?:[\x20-\x7e]"
       rb"|[\xc2-\xdf][\x80-\xbf]"
       rb"|[\xe0-\xef][\x80-\xbf][\x80-\xbf]"
       rb"|[\xf0-\xf4][\x80-\xbf][\x80-\xbf][\x80-\xbf])")


def cmd_cn(path):
    data = open(path, "rb").read()
    pat = re.compile(CJK + rb"{3,}")
    seen = set()
    for m in pat.finditer(data):
        try:
            s = m.group().decode("utf-8")
        except UnicodeDecodeError:
            continue
        if any(k in s for k in CN_KW) and s not in seen:
            seen.add(s)
    print("candidates:", len(seen))
    for s in sorted(seen):
        print(repr(s))


def cmd_cls(path, want):
    cur = ""
    keep = []
    for line in open(path, encoding="utf-8", errors="replace"):
        m = re.search(r"Class descriptor  : '(.*)'", line)
        if m:
            cur = m.group(1)
            continue
        if cur == want or cur.startswith(want.rstrip(";") + "$"):
            keep.append(line.rstrip())
    print("=" * 12, want, "lines", len(keep))
    for l in keep:
        if re.search(r"\bname\s+:", l) or "const-string" in l or "const-class" in l:
            print("   ", l.strip()[:190])


def cmd_find(path, rx):
    pat = re.compile(rx, re.I)
    cls = ""
    for i, line in enumerate(open(path, encoding="utf-8", errors="replace"), 1):
        m = re.search(r"Class descriptor  : '(.*)'", line)
        if m:
            cls = m.group(1)
        if pat.search(line):
            print("%d [%s] %s" % (i, cls, line.strip()[:180]))


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(2)
    mode = sys.argv[1]
    if mode == "cn":
        cmd_cn(sys.argv[2])
    elif mode == "cls":
        cmd_cls(sys.argv[2], sys.argv[3])
    elif mode == "find":
        cmd_find(sys.argv[2], sys.argv[3])
    else:
        print(__doc__)
