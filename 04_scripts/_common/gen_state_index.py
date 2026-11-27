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
  * 活跃度（2026-11-27 起）＝git 提交时间自动判定，不再信人工维护的 `updated`：`git_last_active()` 一次
    `git log` 批量建 path→ts 索引（不是每游戏一次子进程），每个游戏取 `04_scripts/{游戏}/`、
    `10_docs/{游戏}/`、`10_docs/_state/games/{游戏}.md` 三处的最新提交日。取不到（非 git 仓库、
    浅克隆、git 不存在、该游戏无提交）才回退 frontmatter `updated`；两者都没有按「最旧」处理（进挂起）。
    日期列显示的是同一个「有效活跃日」，`--check` 仍是纯数据函数（git 日期只在提交时变）。
  * 活跃度口径（2026-11-27 二轮，被批量提交污染后收口）：git 日期必须证明「这个游戏真被改过」，
    故两处排除——① 生成物路径（任何 `_index.md`、`04_scripts/_common/scripts_index.tsv`、
    `10_docs/_state/current.md`、`10_docs/_state/archive_index.md`）是索引/清单刷新的产物，命中即丢
    弃该行；② 一条 commit 若触及 ≥ `BULK_COMMIT_GAME_DIRS`（20）个不同游戏，判为批量/工具提交，
    **整条 commit 的路径都不参与活跃度**。不做这两步时，三次全仓生成物刷新（b88e9617 / 864cab80 /
    d5a6e918）会把 110/110 个游戏的活跃日一律拉到 11 月下旬，「近 14 天活跃」等于「上次跑全仓工具的时间」。
  * 缺 frontmatter 是硬错误（--check rc=1）：否则新加的 games/x.md 忘写头部，会从两份索引双双
    消失且无人发现。frontmatter 解析同时兼容 CRLF（Win 端编辑器存 CRLF 时只认 LF 会整篇失败）。
"""
import argparse
import datetime
import re
import subprocess
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
# 活跃度来源（2026-11-27）：这两个顶层目录覆盖游戏的脚本与文档；游戏级状态文件按精确路径单独匹配。
GIT_ACTIVITY_PATHS = ("04_scripts", "10_docs")
GIT_TIMEOUT = 60  # 秒；超时即回退 frontmatter，不让索引生成卡死
# 生成物路径（2026-11-27 二轮）：索引/清单刷新的产物，只说明「跑过一次工具」，不代表游戏活跃；
# 命中即整行丢弃——既不记活跃度，也不参与下面批量提交的「游戏数」计数。
GENERATED_PATHS = frozenset((
    "04_scripts/_common/scripts_index.tsv",
    "10_docs/_state/current.md",
    "10_docs/_state/archive_index.md",
))
# 批量提交闸（2026-11-27 二轮）：一条 commit 触及 ≥ 该数量的不同游戏即判为批量/工具提交，
# 整条 commit 的路径都不记活跃度。为什么是 20：单游戏正常改动只碰自己那一两处
# （04_scripts/{游戏}/、10_docs/{游戏}/，偶尔加自己的状态文件）；跨游戏重构实测 4–12 个
# （d6db2cad=4、2e504f5c=6、4476426c=12）；而全仓生成物刷新动辄上百——实测 b88e9617=124、
# 864cab80=109、d5a6e918=103（补 101 份 frontmatter）、2026-09-09 全仓=52。20 落在
# 「正常跨游戏改动」与「全仓刷新」之间，挡得住生成物污染，又不会把正常改动误判成批量。
BULK_COMMIT_GAME_DIRS = 20
# 被批量闸挡掉的提交数（只用于 main 汇报，不参与判定）。
BULK_SKIPPED_COMMITS = 0

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


def _parse_date(s):
    """frontmatter 里的日期；"history"/空/坏格式一律 None。"""
    try:
        return datetime.date.fromisoformat((s or "").strip())
    except ValueError:
        return None


def _is_generated(path):
    """生成物路径：索引/清单刷新的产物，不代表该游戏有进展。

    口径（2026-11-27 二轮）：① 任何 `_index.md`（按文件名判，不分目录层级）；
    ② `04_scripts/_common/scripts_index.tsv`（脚本清单）；③ `10_docs/_state/current.md`、
    `10_docs/_state/archive_index.md`（本脚本与同类生成器的输出）。命中即整行丢弃。
    """
    return path.rsplit("/", 1)[-1] == "_index.md" or path in GENERATED_PATHS


def _activity_keys(path):
    """把一条 git 路径折叠成「游戏活跃键」，与三个前缀一一对应。

    04_scripts/{游戏}/…            -> "04_scripts/{游戏}"
    10_docs/{游戏}/…               -> "10_docs/{游戏}"
    10_docs/_state/games/{游戏}.md -> 原样（精确文件）
    生成物（见 `_is_generated`）与其它路径（10_docs 顶层散文件、10_docs/tools/…、
    04_scripts/_common/…）不产生键，不参与活跃度。
    """
    if _is_generated(path):
        return ()
    if path.startswith("04_scripts/"):
        parts = path.split("/", 2)
        if len(parts) == 3:            # 必须落在子目录里，顶层散文件不算
            return ("04_scripts/" + parts[1],)
        return ()
    if path.startswith("10_docs/"):
        parts = path.split("/", 3)
        if len(parts) >= 3 and parts[1] == "_state" and parts[2] == "games":
            if len(parts) == 4 and parts[3].endswith(".md") and "/" not in parts[3]:
                return ("10_docs/_state/games/" + parts[3],)
            return ()
        if len(parts) >= 3:
            return ("10_docs/" + parts[1],)
    return ()


def _game_ident(path):
    """路径 → 它归属的「游戏」，只用于批量提交阈值计数（不是活跃度键）。

    04_scripts/{游戏}/…         -> "04_scripts/{游戏}"
    10_docs/{游戏}/…            -> "10_docs/{游戏}"
    10_docs/_state/games/{游戏}.md -> "doc:{游戏}"
    生成物、公共目录（下划线的 `_common`/`_state`/`_templates` 等）与其它路径 -> None

    状态文件按文件名算「同一个游戏」：否则「一次补齐 101 份 frontmatter」这类整仓提交因为不碰游戏
    目录，游戏数会是 0 而漏判——实测 d5a6e918 就是这样把 110 个游戏的活跃日拉到 11-09 的。
    """
    if _is_generated(path):
        return None
    if path.startswith("04_scripts/"):
        parts = path.split("/", 2)
        if len(parts) == 3 and not parts[1].startswith("_"):
            return "04_scripts/" + parts[1]
        return None
    if path.startswith("10_docs/"):
        parts = path.split("/", 3)
        if len(parts) >= 3 and parts[1] == "_state":
            if parts[2] == "games" and len(parts) == 4 and parts[3].endswith(".md"):
                return "doc:" + parts[3]
            return None
        if len(parts) >= 3 and not parts[1].startswith("_"):
            return "10_docs/" + parts[1]
    return None


def _git_log_raw():
    """一次 git log 的原始字节；任何异常/rc!=0/空输出都返回 None（不抛，由调用方回退）。"""
    cmd = (["git", "-C", str(ROOT), "log", "--pretty=format:__%ct", "--name-only", "--"]
           + list(GIT_ACTIVITY_PATHS))
    try:
        proc = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                              timeout=GIT_TIMEOUT)
    except FileNotFoundError:
        print("提示: 未找到 git 可执行文件，活跃度回退 frontmatter updated", file=sys.stderr)
        return None
    except (OSError, subprocess.SubprocessError) as exc:
        print("提示: git log 调用失败（%s），活跃度回退 frontmatter updated" % exc, file=sys.stderr)
        return None
    if proc.returncode != 0:
        err = proc.stderr.decode("utf-8", "replace").strip().splitlines()
        print("提示: git log rc=%d（%s），活跃度回退 frontmatter updated"
              % (proc.returncode, err[0] if err else "无 stderr"), file=sys.stderr)
        return None
    if not proc.stdout.strip():
        print("提示: git log 输出为空（浅克隆/新仓库？），活跃度回退 frontmatter updated",
              file=sys.stderr)
        return None
    return proc.stdout


def _iter_commits(raw):
    """把一次 `git log` 输出解析成 (提交时间戳, 该提交的路径列表) 的迭代器。

    输出形如 `__1712345678` 头行 + 若干路径行。必须按 commit 成块处理：活跃度是 per-path
    的，但「这条提交是不是批量/工具提交」只有看完整条提交触及多少个游戏才能判。
    """
    ts, paths = 0, []
    for line in raw.decode("utf-8", "replace").splitlines():
        if line.startswith("__"):
            try:
                nxt = int(line[2:])
            except ValueError:
                nxt = 0
            if ts and paths:
                yield ts, paths
            ts, paths = nxt, []
            continue
        line = line.strip()
        if line and ts:
            paths.append(line)
    if ts and paths:
        yield ts, paths


def git_last_active() -> dict:
    """按 git 提交时间给出每个游戏的最后活跃日（date），替代人工 updated。

    口径（2026-11-27 二轮，后人一看就懂）：
      * 排除生成物：任何 `_index.md`、`04_scripts/_common/scripts_index.tsv`、
        `10_docs/_state/current.md`、`10_docs/_state/archive_index.md` —— 索引/清单刷新
        只说明「跑过工具」，不代表该游戏有进展；命中即丢弃该行，不记进任何游戏。
      * 排除批量提交：一条 commit 触及 ≥ `BULK_COMMIT_GAME_DIRS`（20）个不同游戏
        （`04_scripts/{g}/`、`10_docs/{g}/` 的目录名 ∪ `10_docs/_state/games/{g}.md` 的文件名），
        判为批量/工具提交，整条 commit 的路径都不参与活跃度。原因：单游戏正常改动只碰 1–3 处，
        全仓生成物刷新动辄上百（实测 b88e9617=124 / 864cab80=109 / d5a6e918=103），不设阈值则
        「近 14 天活跃」等于「上次跑全仓工具的时间」，挂起闸恒 0。
    一次 git log 批量建 path→ts 索引，避免 110 次子进程；无 git/浅克隆时返回 {} 由调用方回退 frontmatter。
    """
    global BULK_SKIPPED_COMMITS
    raw = _git_log_raw()
    if raw is None:
        return {}
    idx = {}
    skipped = 0
    for ts, paths in _iter_commits(raw):
        idents, keys = set(), []
        for p in paths:
            gid = _game_ident(p)
            if gid:
                idents.add(gid)
            keys.extend(_activity_keys(p))
        if len(idents) >= BULK_COMMIT_GAME_DIRS:
            skipped += 1              # 批量/工具提交：整条丢弃，一条路径都不记
            continue
        # 输出是最新在前：某键首次出现即其最新提交，后续更旧的 ts 不会覆盖（省内存也省比较）。
        for key in keys:
            if ts > idx.get(key, 0):
                idx[key] = ts
    BULK_SKIPPED_COMMITS = skipped
    if not idx:
        print("提示: git log 里没有可解析的提交路径，活跃度回退 frontmatter updated", file=sys.stderr)
        return {}
    out = {}
    for f in sorted(GAMES.glob("*.md")):
        if f.name in SKIP_FILES:
            continue
        best = max(idx.get("04_scripts/" + f.stem, 0),
                   idx.get("10_docs/" + f.stem, 0),
                   idx.get("10_docs/_state/games/" + f.name, 0))
        if best:
            out[f.stem] = datetime.date.fromtimestamp(best)  # 本地日期
    return out


def attach_activity(items):
    """给每个 item 挂 _active（有效活跃日，date 或 None）与 _active_src（git|updated|none）。

    git 有日期就以 git 为准（忽略 frontmatter updated，人工声明不再参与判定）；
    取不到才回退 updated；两者都没有＝最旧，active/blocked 直接进挂起。
    """
    git_dates = git_last_active()
    for it in items:
        d = git_dates.get(it["_file"][:-3])
        src = "git"
        if d is None:
            d = _parse_date(it.get("updated"))
            src = "updated" if d else "none"
        it["_active"] = d
        it["_active_src"] = src
    return git_dates


def sort_key(item):
    # 注意：调用方用 sorted(..., reverse=True)，故这里取负号把 active/blocked 顶到最前、
    # history 沉到最后；日期字段保持原样，reverse 后即为降序。
    st = item.get("status", "delivered")
    pri = 0 if st in ("active", "blocked") else 1
    d = item.get("_active")
    nodate = 1 if d is None else 0   # 没有活跃日的（原 history 条目）沉到最后
    return (-pri, -nodate, d.isoformat() if d else "")


def render_row(item):
    title = item.get("title") or item["game"]
    d = item.get("_active")          # 有效活跃日：git 优先，回退 updated；都没有显示「历史」
    upd = d.isoformat() if d else "历史"
    icon = ICON.get(item.get("status", "delivered"), "")
    text = " ".join((item.get("one_line") or "").split()) or "（待补一句话状态）"
    return "| [%s](games/%s) | %s | %s %s |" % (title, item["_file"], upd, icon, text)


def build(items, days, limit, stale_days):
    dates = [it["_active"] for it in items if it.get("_active")]
    today = datetime.date.today()
    # 基准日取「数据里的最新活跃日」而不是今天：否则每天零点后窗口滑动，没人改文件也会
    # 让 --check 报不一致（生成物必须是纯数据函数，才能幂等、才能挂进 preflight）。
    # 活跃日来自 git 提交时间时同样成立：git 日期只在提交时变，不是运行时刻的读数。
    base = max(dates) if dates else today
    cutoff = base - datetime.timedelta(days=days)

    def as_date(it):
        return it.get("_active")

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
    attach_activity(items)   # 先挂有效活跃日（git 优先），build/render 全都读它

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
    n_git = len([i for i in items if i.get("_active_src") == "git"])
    n_upd = len([i for i in items if i.get("_active_src") == "updated"])
    print("  活跃日来源：git 提交时间 %d 个 / frontmatter updated %d 个 / 无日期 %d 个"
          % (n_git, n_upd, len(items) - n_git - n_upd))
    print("  活跃度口径：排除生成物路径（_index.md / scripts_index.tsv / current.md / archive_index.md）"
          " + 跳过 %d 条批量提交（≥%d 个游戏）" % (BULK_SKIPPED_COMMITS, BULK_COMMIT_GAME_DIRS))
    n_act = len([i for i in items if i.get("status") in ("active", "blocked")])
    print("  真源 %d 个游戏；active/blocked %d 个；挂起 %d 个；超限 %d 个（均在 archive）"
          % (len(items), n_act, len(stale), len(overflow)))
    report_notes()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
