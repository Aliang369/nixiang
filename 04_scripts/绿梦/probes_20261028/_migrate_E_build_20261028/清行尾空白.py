# -*- coding: utf-8 -*-
"""清掉暂存区里触发 git whitespace 门禁的行尾空白（仅注释/文本类探针笔记）。

原因：`archive_scoped_git.sh` 内跑 `git diff --cached --check`，任何行尾空白/EOF 空行
都会以 rc=2 中止提交（2026-10-28 绿梦迁移的 IL 笔记实测）。
只处理 04_scripts/绿梦/probes_20261028/ 下的 .txt（分析笔记，行尾空白无语义）。
"""
import io
import os
import subprocess

out = subprocess.run(["git", "diff", "--cached", "--check"], capture_output=True, text=True, encoding="utf-8", errors="replace")
bad = set()
for line in out.stdout.splitlines():
    if ":" in line:
        p = line.split(":")[0]
        if p.startswith("04_scripts/绿梦/probes_20261028/") and p.endswith(".txt"):
            bad.add(p)
print("待清理文件数:", len(bad))
for p in sorted(bad):
    if not os.path.exists(p):
        continue
    raw = io.open(p, "rb").read()
    txt = raw.decode("utf-8", errors="replace")
    new = "\n".join(l.rstrip() for l in txt.split("\n")).rstrip("\n") + "\n"
    if new != txt:
        io.open(p, "w", encoding="utf-8", newline="\n").write(new)
        print("  已清:", p)
        subprocess.run(["git", "add", "--", p], check=False)
out2 = subprocess.run(["git", "diff", "--cached", "--check"], capture_output=True, text=True, encoding="utf-8", errors="replace")
print("复查 rc:", out2.returncode)
if out2.stdout:
    print(out2.stdout[:800])
