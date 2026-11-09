#!/usr/bin/env python3
"""生成跨游戏状态索引：current.md（当前工作集）＋ archive_index.md（全量）。

真源＝ 10_docs/_state/games/*.md 头部的 YAML frontmatter：

    ---
    game: "绿梦"                 # 必填，须与文件名一致（链接用）
    title: "绿梦：时空之声"       # 可选，显示名（缺省＝game）
    status: delivered            # active | blocked | delivered
    updated: "2026-10-31"        # 最近更新日期；历史条目写 "history"
    one_line: "一句话粗粒度状态"  # 索引行正文，勿放 hash/包大小/挂点（那些留在正文）
    ---

用法：
    python3 04_scripts/_common/gen_state_index.py              # 重新生成两份索引
    python3 04_scripts/_common/gen_state_index.py --check      # 只校验是否为最新（不一致 rc=1）
    python3 04_scripts/_common/gen_state_index.py --days 30 --limit 60

设计要点（2026-11-09，用户采纳 B 方案）：
  * current.md 是生成物，禁止手改；手改会被下次生成覆盖，preflight 用 --check 拦。
  * current.md 只放当前工作集（status=active/blocked 或近 --days 天有更新，最多 --limit 行），
    因此体积与游戏总数解耦：100 个游戏和 500 个游戏都是同一量级。
  * 全量恒在 archive_index.md ＋ games/*.md，随时可查、可重建；归档不等于删数据。
"""
import argparse
import datetime
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
STATE = ROOT / "10_docs" / "_state"
GAMES = STATE / "games"
CURRENT = STATE / "current.md"
ARCHIVE = STATE / "archive_index.md"

ICON = {"active": "🟡", "blocked": "⚠️", "delivered": "✅"}
FM_RE = re.compile(r"\A---\n(.*?)\n---\n", re.S)
SKIP_FILES = {"README.md", "infrastructure.md"}  # 目录说明，不是游戏

HEAD = """<!-- GENERATED: 由 04_scripts/_common/gen_state_index.py 生成，禁止手改 -->

# 跨游戏状态索引

> **本文件是生成物**：手工修改会在下次生成时被覆盖（`preflight` 用 `--check` 校验）。
> 真源＝`10_docs/_state/games/*.md` 头部的 frontmatter（`game`/`status`/`updated`/`one_line`）。
> 只放**当前工作集**：`status: active|blocked`，或近 {days} 天有更新，最多 {limit} 行 —— 体量与游戏总数无关。
> 全量索引见 [`archive_index.md`](archive_index.md)（共 {total} 个游戏），逐个详情见 `games/{{游戏}}.md`。
> 改状态＝改对应 `games/{{游戏}}.md` 的 frontmatter，然后跑：`python3 04_scripts/_common/gen_state_index.py`

## 当前工作集

| 游戏 | 最近更新 | 状态与摘要 |
|---|---|---|
{rows}

> 本表之外的游戏＝近 {days} 天无更新且非 active，全部在 [`archive_index.md`](archive_index.md)。
"""

ARCHIVE_HEAD = """<!-- GENERATED: 由 04_scripts/_common/gen_state_index.py 生成，禁止手改 -->

# 跨游戏全量索引

> 本文件是**生成物**，由 `games/*.md` 的 frontmatter 汇总，条目只随游戏增减。
> 日常开工看 [`current.md`](current.md)；这里是全量，用于「这个游戏以前做过没有」的检索。
> 重建：`python3 04_scripts/_common/gen_state_index.py`

| 游戏 | 最近更新 | 状态与摘要 |
|---|---|---|
{rows}
"""

FOOT = """
## 基础设施与经验

- [状态目录规则](games/README.md)
- [基础设施与工作区历史](games/infrastructure.md)
- [挂起与候选经验](pending_experience.md)
- [规范普查（2026-10-27）：规则文档清单/冲突/问题](规范普查_20261027.md)

## 历史来源

- [整理前 current 完整快照](history/current_before_rules_optimization_20260905.md)
- [拆分前完整快照](history/current_before_split_20260920.md)
- [2026-10-27 去重压缩前全文](history/current_before_compact_20261027.md)
"""


def parse_frontmatter(text):
    m = FM_RE.match(text)
    if not m:
        return None
    data = {}
    for raw in m.group(1).split("\n"):
        if ":" not in raw:
            continue
        key, val = raw.split(":", 1)
        key, val = key.strip(), val.strip()
        if len(val) >= 2 and val[0] == val[-1] and val[0] in (chr(34), chr(39)):
            val = val[1:-1]
        data[key] = val
    if not data.get("game"):
        return None
    return data


def load():
    items, bad = [], []
    for f in sorted(GAMES.glob("*.md")):
        if f.name in SKIP_FILES:
            continue
        data = parse_frontmatter(f.read_text(encoding="utf-8"))
        if data is None:
            bad.append(f.name)
            continue
        data.setdefault("status", "delivered")
        data.setdefault("updated", "history")
        data.setdefault("one_line", "")
        data["_file"] = f.name
        items.append(data)
    return items, bad


def sort_key(item):
    # 注意：调用方用 sorted(..., reverse=True)，故这里取负号把 active/blocked 顶到最前、
    # history 沉到最后；日期字段保持原样，reverse 后即为降序。
    st = item.get("status", "delivered")
    pri = 0 if st in ("active", "blocked") else 1
    upd = item.get("updated", "")
    hist = 0 if upd != "history" else 1
    return (-pri, -hist, upd)


def render_row(item):
    title = item.get("title") or item["game"]
    upd = item.get("updated", "history")
    upd = "历史" if upd == "history" else upd
    icon = ICON.get(item.get("status", "delivered"), "")
    text = " ".join((item.get("one_line") or "").split()) or "（待补一句话状态）"
    return "| [%s](games/%s) | %s | %s %s |" % (title, item["_file"], upd, icon, text)


def build(items, days, limit):
    dates = []
    for it in items:
        try:
            dates.append(datetime.date.fromisoformat(it.get("updated", "")))
        except ValueError:
            continue
    today = datetime.date.today()
    # 基准日取「数据里的最新 updated」而不是今天：否则每天零点后窗口滑动，没人改文件也会
    # 让 --check 报不一致（生成物必须是纯数据函数，才能幂等、才能挂进 preflight）。
    base = max(dates) if dates else today
    cutoff = base - datetime.timedelta(days=days)

    def in_window(it):
        upd = it.get("updated", "")
        if upd == "history":          # 无日期条目一律不进当前工作集，避免「历史」行常驻索引首位
            return False
        if it.get("status", "delivered") in ("active", "blocked"):
            return True
        try:
            return datetime.date.fromisoformat(upd) >= cutoff
        except ValueError:
            return False

    hot = sorted([i for i in items if in_window(i)], key=sort_key, reverse=True)
    cold = sorted([i for i in items if not in_window(i)], key=sort_key, reverse=True)
    shown = hot[:limit]
    rows_cur = "\n".join(render_row(i) for i in shown) or "| — | — | 当前没有活跃游戏 |"
    rows_all = "\n".join(render_row(i) for i in sorted(items, key=sort_key, reverse=True))
    cur = HEAD.format(days=days, limit=limit, total=len(items), rows=rows_cur).rstrip("\n") + "\n" + FOOT
    arc = ARCHIVE_HEAD.format(rows=rows_all).rstrip("\n") + "\n"
    return cur, arc, shown, cold + hot[limit:]


def main():
    ap = argparse.ArgumentParser(description="生成跨游戏状态索引")
    ap.add_argument("--days", type=int, default=14, help="非 active 游戏的活跃窗口天数（默认 14）")
    ap.add_argument("--limit", type=int, default=30, help="current.md 最多行数（默认 30）")
    ap.add_argument("--check", action="store_true", help="只校验不写盘；不一致返回 1")
    args = ap.parse_args()

    if not GAMES.is_dir():
        print("ERROR: 找不到 %s" % GAMES, file=sys.stderr)
        return 2
    items, bad = load()
    if bad:
        print("⚠ 缺少 frontmatter（已跳过）：%s" % ", ".join(bad[:8]), file=sys.stderr)

    cur, arc, shown, hidden = build(items, args.days, args.limit)

    if args.check:
        problems = []
        for path, expect in ((CURRENT, cur), (ARCHIVE, arc)):
            actual = path.read_text(encoding="utf-8") if path.exists() else ""
            if actual != expect:
                problems.append(path.name)
        if problems:
            print("✗ 索引不是最新：%s 与 games/*.md 的 frontmatter 不一致" % ", ".join(problems), file=sys.stderr)
            print("  重建：python3 04_scripts/_common/gen_state_index.py", file=sys.stderr)
            return 1
        print("✓ 索引与真源一致（%d 个游戏，current %d 行）" % (len(items), len(shown)))
        return 0

    CURRENT.write_text(cur, encoding="utf-8")
    ARCHIVE.write_text(arc, encoding="utf-8")
    print("✓ %s：%d 行 / %d B" % (CURRENT.name, len(shown), len(cur.encode("utf-8"))))
    print("✓ %s：%d 行 / %d B" % (ARCHIVE.name, len(items), len(arc.encode("utf-8"))))
    n_act = len([i for i in items if i.get("status") in ("active", "blocked")])
    print("  真源 %d 个游戏；active/blocked %d 个；窗口外 %d 个（在 archive）" % (len(items), n_act, len(hidden)))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
