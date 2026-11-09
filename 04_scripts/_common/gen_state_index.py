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
    python3 04_scripts/_common/gen_state_index.py --days 30 --limit 60 --stale-days 60

设计要点（2026-11-09，用户采纳 B 方案）：
  * current.md 是生成物，禁止手改；手改会被下次生成覆盖，preflight 用 --check 拦。
  * current.md 只放当前工作集（status=active/blocked 或近 --days 天有更新，最多 --limit 行），
    因此体积与游戏总数解耦：100 个游戏和 500 个游戏都是同一量级。
  * 全量恒在 archive_index.md ＋ games/*.md，随时可查、可重建；归档不等于删数据。
  * 挂起闸（--stale-days，2026-11-09 补）：active/blocked 超该天数无更新即视为「挂起」，
    不再顶格占工作集（旧行为下 9 月的 blocked 永久占据前几行，把近两周更新的游戏挤出 30 行配额）。
  * 缺 frontmatter 是硬错误（--check rc=1）：否则新加的 games/x.md 忘写头部，会从两份索引双双
    消失且无人发现。frontmatter 解析同时兼容 CRLF（Win 端编辑器存 CRLF 时只认 LF 会整篇失败）。
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
# 兼容 CRLF：Win 端编辑器把 games/*.md 存成 CRLF 时，只认 \n 的正则会整篇解析失败，该游戏随即
# 静默掉出两份索引（.gitattributes 的 eol=lf 只在提交时归一化，救不了工作区里那一份）。
FM_RE = re.compile(r"\A---\r?\n(.*?)\r?\n---\r?\n", re.S)
SKIP_FILES = {"README.md", "infrastructure.md"}  # 目录说明，不是游戏

HEAD = """<!-- GENERATED: 由 04_scripts/_common/gen_state_index.py 生成，禁止手改 -->

# 跨游戏状态索引

> **本文件是生成物**：手工修改会在下次生成时被覆盖（`preflight` 用 `--check` 校验）。
> 真源＝`10_docs/_state/games/*.md` 头部的 frontmatter（`game`/`status`/`updated`/`one_line`）。
> 只放**当前工作集**：`status: active|blocked`（且未超 --stale-days 挂起闸），或近 {days} 天有更新，最多 {limit} 行。
> 全量索引见 [`archive_index.md`](archive_index.md)（共 {total} 个游戏），逐个详情见 `games/{{游戏}}.md`。
> 改状态＝改对应 `games/{{游戏}}.md` 的 frontmatter，然后跑：`python3 04_scripts/_common/gen_state_index.py`

## 当前工作集

| 游戏 | 最近更新 | 状态与摘要 |
|---|---|---|
{rows}

{tail}"""

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
        raw = raw.rstrip("\r")
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


def build(items, days, limit, stale_days):
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

    def as_date(it):
        try:
            return datetime.date.fromisoformat(it.get("updated", ""))
        except ValueError:
            return None

    hot, stale = [], []
    for it in items:
        st = it.get("status", "delivered")
        d = as_date(it)
        if st in ("active", "blocked"):
            # 挂起闸（2026-11-09）：有 active/blocked 标记但长期无更新，说明已经停在那里了。
            # 仍留在 archive（可检索、可接手），只是不再占 current 的行位。
            if d is None or (base - d).days > stale_days:
                stale.append(it)
            else:
                hot.append(it)
        elif d is not None and d >= cutoff:
            hot.append(it)

    hot_sorted = sorted(hot, key=sort_key, reverse=True)
    shown = hot_sorted[:limit]
    overflow = hot_sorted[limit:]
    rows_cur = "\n".join(render_row(i) for i in shown) or "| — | — | 当前没有活跃游戏 |"
    rows_all = "\n".join(render_row(i) for i in sorted(items, key=sort_key, reverse=True))

    def name_list(seq):
        seq = sorted(seq, key=sort_key, reverse=True)
        names = "、".join(i["game"] for i in seq[:6])
        return names + ("…" if len(seq) > 6 else "")

    notes = []
    if stale:
        notes.append("> ⏸ %d 个 active/blocked 已超 %d 天无更新，视为挂起、不进工作集：%s（详见 `archive_index.md`）"
                     % (len(stale), stale_days, name_list(stale)))
    if overflow:
        notes.append("> ⏸ %d 个近 %d 天有更新但超出 %d 行上限，只在 `archive_index.md`：%s"
                     % (len(overflow), days, limit, name_list(overflow)))
    tail = "\n".join(notes)
    tail = (tail + "\n\n") if tail else ""
    tail += "> 本表之外的游戏＝近 %d 天无更新、超出 %d 行上限或已挂起，全部在 [`archive_index.md`](archive_index.md)。" % (days, limit)

    cur = HEAD.format(days=days, limit=limit, total=len(items), rows=rows_cur, tail=tail).rstrip("\n") + "\n" + FOOT
    arc = ARCHIVE_HEAD.format(rows=rows_all).rstrip("\n") + "\n"
    return cur, arc, shown, overflow, stale

def main():
    ap = argparse.ArgumentParser(description="生成跨游戏状态索引")
    ap.add_argument("--days", type=int, default=14, help="非 active 游戏的活跃窗口天数（默认 14）")
    # 2026-11-09 用户定 A 方案：上限 30 → 50。30 是上一版按体积倒推拍的数（8.7 KB），14 天窗口
    # 实际有 49 条时必然截断，19 个近两周更新的游戏只能去 archive 找。50 行 ≈ 13.3 KB，仍在 15 KB 内。
    ap.add_argument("--limit", type=int, default=50, help="current.md 最多行数（默认 50，安全阀；体积上限见 AGENTS.md §6）")
    ap.add_argument("--stale-days", type=int, default=45,
                    help="active/blocked 超过该天数无更新即视为挂起、移出工作集（默认 45；0=停用挂起闸）")
    ap.add_argument("--check", action="store_true", help="只校验不写盘；不一致返回 1")
    args = ap.parse_args()

    if not GAMES.is_dir():
        print("ERROR: 找不到 %s" % GAMES, file=sys.stderr)
        return 2
    items, bad = load()

    stale_days = args.stale_days if args.stale_days > 0 else 10 ** 6
    cur, arc, shown, overflow, stale = build(items, args.days, args.limit, stale_days)

    def report_notes():
        if stale:
            print("⚠ %d 个 active/blocked 超 %d 天无更新，已移出工作集（仍在 archive）：%s"
                  % (len(stale), args.stale_days, "、".join(i["game"] for i in stale[:8])), file=sys.stderr)
        if overflow:
            print("⚠ %d 个近 %d 天有更新但超出 %d 行上限，只在 archive：%s"
                  % (len(overflow), args.days, args.limit, "、".join(i["game"] for i in overflow[:8])), file=sys.stderr)

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
        if bad:
            # 缺 frontmatter 不能只警告：该游戏会从 current/archive 双双消失，--check 若照样 rc=0，
            # preflight 就永远发现不了（本次补闸的直接原因）。按硬错误处理。
            print("✗ %d 个 games/*.md 缺 frontmatter，会从索引中静默消失：%s"
                  % (len(bad), ", ".join(bad[:8]) + ("…" if len(bad) > 8 else "")), file=sys.stderr)
            print("  补头部四行 game/status/updated/one_line（模板见 games/README.md）", file=sys.stderr)
            return 1
        report_notes()
        print("✓ 索引与真源一致（%d 个游戏，current %d 行）" % (len(items), len(shown)))
        return 0

    # newline="" 保持字节级 LF：Path.write_text 在 Win 上按 os.linesep 写 CRLF，会让工作区文件
    # 比 HEAD 多出 N 个字节（current 62 / archive 111），体积关虚高、两机工作区不一致。
    # 不用 write_text(newline=...) 是因为它 3.10 才有，mac 侧可能是更老的 python3。
    for path, text in ((CURRENT, cur), (ARCHIVE, arc)):
        with open(str(path), "w", encoding="utf-8", newline="\n") as fh:
            fh.write(text)
    print("✓ %s：%d 行 / %d B" % (CURRENT.name, len(shown), len(cur.encode("utf-8"))))
    print("✓ %s：%d 行 / %d B" % (ARCHIVE.name, len(items), len(arc.encode("utf-8"))))
    n_act = len([i for i in items if i.get("status") in ("active", "blocked")])
    print("  真源 %d 个游戏；active/blocked %d 个；挂起 %d 个；超限 %d 个（均在 archive）"
          % (len(items), n_act, len(stale), len(overflow)))
    report_notes()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
