# -*- coding: utf-8 -*-
"""收尾：清掉剩余两处 whitespace 门禁触发点（fg_cage.c 的 EOF 空行、evidence README 的行尾空格）。"""
import io
import subprocess

p1 = "04_scripts/绿梦/fg_cage.c"
s = io.open(p1, encoding="utf-8").read()
s2 = s.rstrip("\n") + "\n"
if s2 != s:
    io.open(p1, "w", encoding="utf-8", newline="\n").write(s2)
    print("已清 EOF 空行:", p1)
subprocess.run(["git", "add", "--", p1], check=False)

p2 = "10_docs/长生天机降世/evidence_20261028/README.md"
lines = io.open(p2, encoding="utf-8").read().split("\n")
out = [l.rstrip() for l in lines]
new = "\n".join(out)
io.open(p2, "w", encoding="utf-8", newline="\n").write(new)
subprocess.run(["git", "add", "--", p2], check=False)
print("已清行尾空白:", p2)

r = subprocess.run(["git", "diff", "--cached", "--check"], capture_output=True, text=True, encoding="utf-8", errors="replace")
print("复查 rc:", r.returncode)
print(r.stdout[:600] if r.stdout else "(clean)")
