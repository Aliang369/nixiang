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
    * **按修改时间判"当前"**：同一前缀（如「星光落地_」）只把 mtime 最新的标为【当前】，
      其余标【历史】。这比人工维护清单可靠 —— 它不会忘记更新。
    * `_index.md` 是生成物，头部写明禁止手改；`--check` 供 preflight 用。
    * 目录里已存在的人工 `README.md` 不覆盖：索引正文追加在 `_index.md`，
      两者分工＝README 写人读的说明、`_index.md` 写机器/检索用的清单。
"""
import argparse
import datetime
import re
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
    dirs = sorted([p for p in game_dir.iterdir() if p.is_dir()],
                  key=lambda p: (-p.stat().st_mtime, p.name))

    # 同名多版本 → 最新者为当前
    newest = {}
    for p in files:
        m = VERSIONED.match(p.stem)
        key = re.sub(r"[_\d.]+$", "", p.stem) if m else None
        if key:
            mt = p.stat().st_mtime
            if key not in newest or mt > newest[key][0]:
                newest[key] = (mt, p.name)

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
        "> 同名多版本（裁剪清单/星空落地）只有 mtime 最新的标【当前】，其余【历史】。",
        "> 重新生成：`python3 04_scripts/_common/gen_game_doc_index.py --game " + game_dir.name + "`",
        "",
    ]

    for label in order:
        items = groups.get(label)
        if not items:
            continue
        items.sort(key=lambda p: (-p.stat().st_mtime, p.name))
        out.append(f"## {label}")
        out.append("")
        for p in items:
            mt = datetime.date.fromtimestamp(p.stat().st_mtime)
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
        elif not args.all and nfiles < args.min_files:
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
            target.write_text(content, encoding="utf-8")
            updated += 1
        else:
            if args.check:
                stale_list.append(d.name)
                continue
            target.write_text(content, encoding="utf-8")
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
