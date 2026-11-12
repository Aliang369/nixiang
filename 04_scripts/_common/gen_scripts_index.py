#!/usr/bin/env python3
"""生成 `04_scripts/_common/scripts_index.tsv`：每个公共脚本的用途、被谁引用、是否孤儿。

用法：
    python3 04_scripts/_common/gen_scripts_index.py            # 生成/刷新索引
    python3 04_scripts/_common/gen_scripts_index.py --check    # 只校验（preflight 用）

为什么需要（2026-11-10 实测）：
    `04_scripts/_common/` 有 103 个脚本，其中 **47 个（46%）没有任何规则文档引用**。
    它们不是没用，而是**不可发现** —— 新会话不知道有它，只能重造（AGENTS §3.3 明令禁重造）。
    索引把「有没有、干什么、谁在用」变成一行可 grep 的事实。

「孤儿」判定口径（刻意保守，避免误杀）：
    只要满足任一即**不算孤儿**：
      ① 被 AGENTS.md / PLAYBOOK.md / WORKFLOW.md / TOOLBOOK.md / skill/** / stages.md 引用；
      ② 被另一个脚本调用（`bash .../x.sh` / `python3 .../x.py`）；
      ③ 被 `04_scripts/**` 的游戏私有脚本引用；
      ④ 文件名出现在任意 `10_docs/**` 文档里。
    都没有 ⇒ 标 `orphan`，供人工判断：要么补进路由，要么迁 `legacy_tools/`。
    **本脚本只报告，不删任何文件**。
"""
import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
COMMON = ROOT / "04_scripts" / "_common"
OUT = COMMON / "scripts_index.tsv"
SELF = "gen_scripts_index.py"

# 规则/路由类文档：引用即视为"活"
ROUTE_DOCS = [
    "AGENTS.md", "PLAYBOOK.md", "WORKFLOW.md", "TOOLBOOK.md",
    "04_scripts/_common/stages.md", "skill/SKILL.md", "skill/diagnosis-index.md",
]
ROUTE_GLOBS = ["skill/**/*.md", "10_docs/_templates/*.md"]

DOC_RE = re.compile(r"^#\s+(.+)$")


def summary(path: Path) -> str:
    """取脚本头部第一个可读的描述行。"""
    try:
        with path.open(encoding="utf-8", errors="replace") as fh:
            for i, line in enumerate(fh):
                if i > 40:
                    break
                s = line.rstrip("\n")
                # 跳过 shebang / 纯分隔线 / 空。
                # 注意：分隔线的 strip 结果是 `# ====...`，含空格 —— 判定必须先把空格去掉，
                # 否则 `set("# ===") <= {"#","=","-"}` 为假，分隔线会被当成描述（实测踩过）。
                if not s.strip() or s.startswith("#!"):
                    continue
                if s.lstrip("#").strip(" =-") == "":
                    continue
                if s.startswith("#"):
                    t = s.lstrip("# ").strip()
                    if t and not t.startswith("用法") and not t.startswith("----"):
                        return t[:110]
                elif s.startswith('"""') or s.startswith("'''"):
                    t = s.strip().strip('"\'').strip()
                    if t:
                        return t[:110]
    except OSError:
        pass
    return "(无描述)"


def collect_corpus() -> list:
    """收集判定语料，返回 [(来源标识, 文本)]。

    必须**带来源标识**：判定「某脚本是否被引用」时要排除它自己的正文，
    否则一个在注释里写了自身文件名的脚本会把自己算成"有引用"。
    """
    chunks = []
    for rel in ROUTE_DOCS:
        p = ROOT / rel
        if p.is_file():
            chunks.append((rel, p.read_text(encoding="utf-8", errors="replace")))
    for pat in ROUTE_GLOBS:
        for p in ROOT.glob(pat):
            if p.is_file():
                chunks.append((str(p.relative_to(ROOT)),
                               p.read_text(encoding="utf-8", errors="replace")))
    # 所有脚本（含游戏私有）——用于判"被别的脚本调用"
    for p in (ROOT / "04_scripts").rglob("*"):
        if p.is_file() and p.suffix in (".sh", ".py"):
            try:
                chunks.append((str(p.relative_to(ROOT)),
                               p.read_text(encoding="utf-8", errors="replace")))
            except OSError:
                pass
    # 文档层
    for p in (ROOT / "10_docs").rglob("*.md"):
        try:
            chunks.append((str(p.relative_to(ROOT)),
                           p.read_text(encoding="utf-8", errors="replace")))
        except OSError:
            pass
    return chunks


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--quiet", action="store_true")
    args = ap.parse_args()

    if not COMMON.is_dir():
        print(f"✗ 找不到 {COMMON}", file=sys.stderr)
        return 2

    scripts = sorted(p for p in COMMON.iterdir()
                     if p.is_file() and p.suffix in (".sh", ".py") and p.name != SELF)
    corpus = collect_corpus()

    rows = ["name\tkind\tbytes\tsummary\tstatus\trefs"]
    orphans = []
    for p in scripts:
        name = p.name
        # 统计「除自己之外」有多少个来源提到它。排除自身是关键：
        # 若脚本正文/注释里写了自己的文件名（很常见），不排除就会把孤儿误判成"有引用"。
        self_rel = str(p.relative_to(ROOT))
        refs = sum(txt.count(name) for src, txt in corpus if src != self_rel)
        status = "orphan" if refs == 0 else "used"
        if status == "orphan":
            orphans.append(name)
        rows.append(f"{name}\t{p.suffix[1:]}\t{p.stat().st_size}\t{summary(p)}\t{status}\t{refs}")

    content = "\n".join(rows) + "\n"

    if args.check:
        if not OUT.exists():
            print("✗ scripts_index.tsv 不存在 —— 跑一次 gen_scripts_index.py", file=sys.stderr)
            return 1

        # 只比较「结构性列」（name/kind/bytes/summary/status），**忽略 refs 计数**。
        # 原因：refs 会随任意 10_docs 文档改动而变化，若纳入比较，
        # 那么"写完一条变更日志"就会让索引判过期 —— 门禁退化成噪音，
        # 最终所有人都学会无视它。它本该抓的是「脚本增删改导致用途/孤儿状态变了」。
        def structural(text: str) -> list:
            out = []
            for ln in text.splitlines()[1:]:      # 跳过表头
                f = ln.split("\t")
                if len(f) >= 5:
                    out.append(tuple(f[:5]))      # 前五列，丢掉 refs
            return out

        old = OUT.read_text(encoding="utf-8", errors="replace")
        if structural(old) != structural(content):
            print("✗ scripts_index.tsv 已过期（脚本有增删改，用途/孤儿状态变了）", file=sys.stderr)
            print("   修法：python3 04_scripts/_common/gen_scripts_index.py", file=sys.stderr)
            return 1
        if old != content:
            # 结构一致、仅 refs 漂移：刷新计数但不判失败
            OUT.write_text(content, encoding="utf-8", newline="\n")
        print(f"✓ 脚本索引一致（{len(scripts)} 个，孤儿 {len(orphans)} 个）")
        return 0

    OUT.write_text(content, encoding="utf-8", newline="\n")
    if not args.quiet:
        print(f"✓ 已写 {OUT.relative_to(ROOT)}：{len(scripts)} 个脚本，其中孤儿 {len(orphans)} 个")
        if orphans:
            print("  孤儿（无任何规则/脚本/文档引用，需人工判断：补路由 或 迁 legacy_tools/）：")
            for n in orphans[:15]:
                print(f"    - {n}")
            if len(orphans) > 15:
                print(f"    …另有 {len(orphans) - 15} 个，见 TSV")
    return 0


if __name__ == "__main__":
    sys.exit(main())
