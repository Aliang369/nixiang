#!/usr/bin/env python3
"""生成/校验每个游戏目录的 `_index.md`：本目录有哪些文件、哪个是当前有效、哪些是历史。

用法：
    python3 04_scripts/_common/gen_game_doc_index.py            # 生成全部（默认只处理 ≥8 个文件的目录）
    python3 04_scripts/_common/gen_game_doc_index.py --all      # 含小目录
    python3 04_scripts/_common/gen_game_doc_index.py --game 勇士之心
    python3 04_scripts/_common/gen_game_doc_index.py --check    # 只校验，不一致 rc=1
    python3 04_scripts/_common/gen_game_doc_index.py --stale-days 30

为什么需要（2026-11-10 实测）：
    勇士之心 50 个文件、绿梦 42 个、蛋壳战机 37 个，全部平铺在同一层；其中
    「正式包裁剪清单」12 个版本、「滚动物清理清单」8 个版本、「星空落地」5 个版本。
    AI 读完状态摘要后不知道哪 5 个是**当前**的，只能整读或瞎猜 —— 这就是
    「文档写了一堆但没连贯运用起来」的直接成因。本索引把"当前/历史"显式标出来。

设计要点：
    * **不读正文**：只取每个文件的首个标题行（`# ...`）或首个引用块作为一句话说明，
      避免为了生成索引把 772 个文件读进上下文。
    * **按文档日期判"当前"**：同一前缀（如「星光落地_」）只把提交日期最新的标为【当前】，
      其余标【历史】。这比人工维护清单可靠 —— 它不会忘记更新。
    * **文档日期＝最后一次 git 提交日期**（`_doc_date()`），不是文件 mtime；见该函数注释。
    * `_index.md` 是生成物，头部写明禁止手改；`--check` 供 preflight 用。
    * 目录里已存在的人工 `README.md` 不覆盖：索引正文追加在 `_index.md`，
      两者分工＝README 写人读的说明、`_index.md` 写机器/检索用的清单。
"""
import argparse
import datetime
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / "10_docs"
EXCLUDE_DIRS = {"_state", "_templates", "tools", "双机协作", "_batch_binge6"}
INDEX_NAME = "_index.md"
MIN_FILES = 8            # 低于这个数不值得建索引
STALE_DAYS_DEFAULT = 30

# 文件 → 语义分类。顺序即索引中的显示顺序（越靠前越该先看）。
CATEGORIES = [
    ("契约", [r"^硬约束\.md$", r"^路线锁定\.md$", r"^GAME\.md$"]),
    ("入口与说明", [r"^README\.md$", r"^_digest\.md$", r"^技能先例\.md$"]),
    ("抓点与分析", [r"抓点", r"侦察", r"分析", r"逆向", r"锚点", r"JS结构", r"结构与功能"]),
    ("方案与路线", [r"方案", r"路线", r"选型", r"可行性", r"壳", r"脱壳", r"解密", r"密钥", r"补丁"]),
    ("落地与出包", [r"落地", r"移植记录", r"出包", r"装机", r"移交", r"热更", r"修复", r"根因", r"复现"]),
    ("验收与收尾", [r"验收", r"收尾", r"清单", r"预检", r"门禁"]),
    ("台账", [r"台账", r"轮次", r"evidence", r"证据"]),
]
# 明确的"产物/清单"类型：同名多版本时只有最新的是当前
VERSIONED = re.compile(r"^(正式包裁剪清单|滚动物清理清单|清理清单|星空落地|星光落地|交付清单)")
HEADING = re.compile(r"^#{1,3}\s+(.+?)\s*$")
QUOTE = re.compile(r"^>\s*(.+?)\s*$")

# ---------------------------------------------------------------
# 文档日期：统一用「最后一次 git 提交日期」，不用文件 mtime
#   为什么：git 不保存 mtime，换机 / 重新 checkout / 重新克隆后 mtime 全变，
#   `_index.md` 里记的每个文件日期就跟着漂 —— 2026-11-24 实测 111 个目录长期
#   被判「过期」，preflight 的 [游戏文档索引] 闸门因此常年 FAIL。
#   基准只有一个：条目日期、【当前/历史】判定、⏳ 过期判定、以及列表排序
#   全部走 _doc_date()。要改回「按本地 mtime 判 ⏳」必须四处一起改，
#   否则就是两套时间基准混用（那比漂移更难查）。
# ---------------------------------------------------------------
_DATE_CACHE = {}
_DATE_MAP = None          # 仓库内已跟踪文件 → 最后提交日期（一次 shell 调用建好）


def _git_date_map():
    """一次性拿到全仓已跟踪文件的最后提交日期（path -> YYYY-MM-DD）。

    用 git log --format=%x00%cs --name-only --no-renames：提交按新→旧排列，
    路径第一次出现即「最后一次改动它的提交」，所以先到先得。
    992 个提交实测 0.2 s；逐个文件 git log -1 -- <path> 要 1900 次 ≈ 60 s。
    git 不可用 / 不在仓库里 / 解析失败 => 返回空表，_doc_date 全部回落 mtime。
    """
    global _DATE_MAP
    if _DATE_MAP is not None:
        return _DATE_MAP
    _DATE_MAP = {}
    try:
        r = subprocess.run(
            ["git", "-C", str(ROOT), "-c", "core.quotepath=false",
             "log", "--format=%x00%cs", "--name-only", "--no-renames"],
            capture_output=True, encoding="utf-8", errors="replace", timeout=180)
        if r.returncode == 0:
            cur = None
            for line in (r.stdout or "").splitlines():
                if line.startswith("\x00"):
                    cur = line[1:].strip()
                    continue
                if not line.strip() or cur is None:
                    continue
                _DATE_MAP.setdefault(line, cur)   # 先到先得＝最近一次提交
    except Exception:                             # noqa: BLE001 —— 取不到日期不是错误
        _DATE_MAP = {}
    return _DATE_MAP


def _doc_date(path: Path) -> datetime.date:
    """文件/目录的「文档日期」＝最后一次 git 提交日期；取不到才回落 mtime。

    - 文件：改过它的最后一次提交日期；未跟踪 / 无提交 => 本地 mtime。
    - 目录：git 不记目录日期，取该目录下已跟踪文件里最新的提交日期；空目录 => mtime。
    - 两条路径都不抛异常：拿不到提交日期 => mtime；mtime 也读不到 => 今天。
    同一路径只算一次（_DATE_CACHE）。
    """
    key = str(path)
    hit = _DATE_CACHE.get(key)
    if hit is not None:
        return hit
    d = None
    try:
        rel = path.resolve().relative_to(ROOT).as_posix()
    except (ValueError, OSError):
        rel = None
    if rel:
        try:
            dates = _git_date_map()
            s = dates.get(rel)
            if s is None and path.is_dir():
                pref = rel.rstrip("/") + "/"
                sub = [v for k, v in dates.items() if k.startswith(pref)]
                if sub:
                    s = max(sub)              # 目录日期＝其下最新提交日期
            if s:
                d = datetime.date.fromisoformat(s)
        except (ValueError, OSError):
            d = None
    if d is None:
        try:
            d = datetime.date.fromtimestamp(path.stat().st_mtime)
        except (OSError, OverflowError, ValueError):
            d = datetime.date.today()
    _DATE_CACHE[key] = d
    return d


def one_line(path: Path) -> str:
    """文件的一句话说明：首个标题，否则首个引用块，否则留空。只读前 40 行。"""
    try:
        with path.open(encoding="utf-8", errors="replace") as fh:
            for i, line in enumerate(fh):
                if i > 40:
                    break
                m = HEADING.match(line)
                if m:
                    t = m.group(1).strip()
                    t = re.sub(r"^[📌📍⚠️✅🔴]+\s*", "", t)
                    return t[:80]
            fh.seek(0)
            for i, line in enumerate(fh):
                if i > 10:
                    break
                m = QUOTE.match(line)
                if m and len(m.group(1)) > 6:
                    return m.group(1)[:80]
    except OSError:
        pass
    return ""


def size_label(path: Path) -> str:
    try:
        n = path.stat().st_size
    except OSError:
        return ""
    if n >= 1024 * 1024:
        return f"{n / 1024 / 1024:.1f}MB"
    if n >= 1024:
        return f"{n / 1024:.0f}KB"
    return f"{n}B"


def classify(name: str) -> str:
    for label, pats in CATEGORIES:
        if any(re.search(p, name) for p in pats):
            return label
    return "其他"


def build(game_dir: Path, stale_days: int) -> str:
    now = datetime.date.today()
    files = [p for p in game_dir.iterdir() if p.is_file() and p.name != INDEX_NAME]
    dates = {p: _doc_date(p) for p in files}      # 每个文件只取一次日期
    dirs = sorted([p for p in game_dir.iterdir() if p.is_dir()],
                  key=lambda p: (-_doc_date(p).toordinal(), p.name))

    # 同名多版本 → 最新者为当前
    newest = {}
    for p in files:
        m = VERSIONED.match(p.stem)
        key = re.sub(r"[_\d.]+$", "", p.stem) if m else None
        if key:
            seen = (dates[p].toordinal(), p.name)   # 同日期时按名字定序，避免跨机不定
            if key not in newest or seen > newest[key]:
                newest[key] = seen

    groups = {}
    for p in files:
        groups.setdefault(classify(p.name), []).append(p)

    order = [c[0] for c in CATEGORIES] + ["其他"]
    out = [
        "<!-- GENERATED: 由 04_scripts/_common/gen_game_doc_index.py 生成，禁止手改 -->",
        "",
        f"# {game_dir.name} · 文档索引",
        "",
        f"> 生成于 {now.isoformat()}｜{len(files)} 个文件、{len(dirs)} 个子目录。",
        "> 本文件是**检索用清单**，不是分析结论；结论读 `_state/games/" + game_dir.name + ".md`。",
        "> 同名多版本（裁剪清单/星空落地）只有提交日期最新的标【当前】，其余【历史】。",
        "> 重新生成：`python3 04_scripts/_common/gen_game_doc_index.py --game " + game_dir.name + "`",
        "",
    ]

    for label in order:
        items = groups.get(label)
        if not items:
            continue
        items.sort(key=lambda p: (-dates[p].toordinal(), p.name))
        out.append(f"## {label}")
        out.append("")
        for p in items:
            mt = dates[p]
            age = (now - mt).days
            tag = ""
            m = VERSIONED.match(p.stem)
            key = re.sub(r"[_\d.]+$", "", p.stem) if m else None
            if key and key in newest:
                tag = "**【当前】** " if newest[key][1] == p.name else "【历史】 "
            stale = " ⏳" if age >= stale_days else ""
            desc = one_line(p)
            line = f"- {tag}`{p.name}`"
            if desc:
                line += f" — {desc}"
            line += f"（{size_label(p)}，{mt.isoformat()}{stale}）"
            out.append(line)
        out.append("")

    if dirs:
        out.append("## 子目录")
        out.append("")
        for d in dirs:
            n = sum(1 for _ in d.rglob("*") if _.is_file())
            out.append(f"- `{d.name}/`（{n} 个文件）")
        out.append("")

    out.append("---")
    out.append("")
    out.append("**阅读建议**：先读「契约」→ 再看「落地与出包」里的【当前】→ 需要追因才翻「历史」与子目录。")
    return "\n".join(out) + "\n"


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--all", action="store_true", help="含文件数低于阈值的目录")
    ap.add_argument("--game", help="只处理指定游戏")
    ap.add_argument("--check", action="store_true", help="只校验，不一致 rc=1")
    ap.add_argument("--stale-days", type=int, default=STALE_DAYS_DEFAULT)
    ap.add_argument("--min-files", type=int, default=MIN_FILES)
    args = ap.parse_args()

    if not DOCS.is_dir():
        print(f"✗ 找不到 {DOCS}", file=sys.stderr)
        return 2

    if args.game:
        targets = [DOCS / args.game]
        if not targets[0].is_dir():
            print(f"✗ 没有这个游戏目录: {args.game}", file=sys.stderr)
            return 2
    else:
        targets = sorted(p for p in DOCS.iterdir()
                         if p.is_dir() and p.name not in EXCLUDE_DIRS)

    made = updated = same = skipped = 0
    stale_list = []
    for d in targets:
        if not d.is_dir():
            continue
        nfiles = sum(1 for p in d.iterdir() if p.is_file())
        # 选择规则：显式 --all ⇒ 全部；--check ⇒ 已建索引的必查（否则小目录的索引会静默腐烂）；
        # 生成模式 ⇒ 低于阈值的跳过，避免给只有 2 个文件的目录制造噪音。
        has_index = (d / INDEX_NAME).exists()
        if args.game:
            pass
        elif args.check:
            if not (args.all or has_index):
                skipped += 1
                continue
        elif not args.all and nfiles < args.min_files and not has_index:
            skipped += 1
            continue
        content = build(d, args.stale_days)
        target = d / INDEX_NAME
        if target.exists():
            old = target.read_text(encoding="utf-8", errors="replace")
            # 忽略"生成于 <日期>"那一行，否则每天都会判不一致
            norm = lambda s: re.sub(r"生成于 \d{4}-\d{2}-\d{2}", "生成于 X", s)
            if norm(old) == norm(content):
                same += 1
                continue
            if args.check:
                stale_list.append(d.name)
                continue
            # 3.9 兼容：write_text 的 newline 参数 3.10 才有；newline="" 与 "\n" 写入等价（都保持 LF，
            # 也避免 Win 上按 os.linesep 写 CRLF 让生成物比 HEAD 多字节）
            with target.open("w", encoding="utf-8", newline="") as fh:
                fh.write(content)
            updated += 1
        else:
            if args.check:
                stale_list.append(d.name)
                continue
            # 3.9 兼容：write_text 的 newline 参数 3.10 才有；newline="" 与 "\n" 写入等价（都保持 LF，
            # 也避免 Win 上按 os.linesep 写 CRLF 让生成物比 HEAD 多字节）
            with target.open("w", encoding="utf-8", newline="") as fh:
                fh.write(content)
            made += 1

    if args.check:
        if stale_list:
            print(f"✗ {len(stale_list)} 个游戏的 _index.md 缺失或过期：")
            for g in stale_list[:20]:
                print(f"   {g}")
            if len(stale_list) > 20:
                print(f"   …另有 {len(stale_list) - 20} 个")
            print("   修法：python3 04_scripts/_common/gen_game_doc_index.py")
            return 1
        print(f"✓ 游戏文档索引一致（{same} 个已是最新）")
        return 0

    print(f"✓ 新建 {made} 个 / 更新 {updated} 个 / 已最新 {same} 个 / 跳过（文件数<{args.min_files}）{skipped} 个")
    return 0


if __name__ == "__main__":
    sys.exit(main())
