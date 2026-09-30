#!/usr/bin/env python3
"""共享文件 hunk 级（内容级）暂存：只把本任务的改动写进索引，把并行会话的改动留在工作区。

背景（2026-09-30 勇士之心实测，两次踩坑）：
  仓库有若干**跨会话共享文件**（清单见 `archive_scoped_git.sh` 的 `SHARED_FILES`），
  并行会话会在同一文件里追加自己的改动（例：伊莫会话在 `sign/SignSpoof.java` 追加
  `YIMO_PKG`/`YIMO_CERT_B64` 与 `fakes.put(YIMO_PKG, …)`）。直接 `git add -A` 会把
  他人未完成的改动一起提交。
  **不可行路径**：手工 `git diff` 拆 hunk 再 `git apply --cached` —— 自拼 patch 头不兼容，
  报 `error: patch with only garbage at line 5`。
  **可行路径**（本脚本固化）：读工作区文件 → 按行谓词丢弃他人行 → 与 `HEAD:<path>`
  做 difflib 自检 → `git hash-object -w` + `git update-index --cacheinfo` 直接写索引。
  **工作区文件不改**，他人改动原样保留。

用法：
  python3 04_scripts/_common/stage_shared_file.py \
      --path mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java \
      --drop-pattern '伊莫' --drop-pattern 'YIMO_' \
      --assert-absent 伊莫 --dry-run

  去掉 --dry-run 才真正写索引；随后用 `archive_scoped_git.sh --staged-only` 提交。

验证铁律（2026-09-30 实际漏过一次，16 行）：
  **禁止用「grep 标记串 = 0」代替逐行核对**。他人改动常含**无标记的续行**
  （例：伊莫会话的 `YIMO_CERT_B64 =` 后跟 15 行 base64 字符串续行，续行里既没有
  `伊莫` 也没有 `YIMO_`）。丢弃谓词只按标记串匹配时，声明行被丢、续行留下 ⇒
  提交里混入他人半截代码。本脚本因此加了「截断语句」检测：被丢弃的行若以
  `=` `+` `,` `(` `\\` 结尾且其下一行被保留，直接报错（用 --allow-truncated-statement 放行）。
"""
from __future__ import annotations

import argparse
import difflib
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def run_git(args: list[str], text_input: str | None = None) -> subprocess.CompletedProcess:
    return subprocess.run(
        ["git", *args], cwd=ROOT, capture_output=True, text=True, input=text_input
    )


def head_lines(path: str) -> list[str] | None:
    res = run_git(["show", f"HEAD:{path}"])
    if res.returncode != 0:
        return None
    return res.stdout.splitlines(keepends=True)


def file_mode(path: str) -> str:
    res = run_git(["ls-files", "-s", "--", path])
    line = res.stdout.strip().split("\n")[0] if res.stdout.strip() else ""
    return line.split()[0] if line else "100644"


def render_diff(before: list[str], after: list[str], path: str, limit: int) -> str:
    diff = list(difflib.unified_diff(before, after, fromfile=f"HEAD:{path}", tofile=f"index:{path}"))
    if not diff:
        return "  （与 HEAD 逐字节相同，本次将没有暂存改动）"
    out = []
    for i, line in enumerate(diff):
        if i >= limit:
            out.append(f"  … 其余 {len(diff) - limit} 行省略")
            break
        out.append("  " + line.rstrip("\n"))
    return "\n".join(out)


def main() -> int:
    ap = argparse.ArgumentParser(description="共享文件内容级暂存（不动工作区）")
    ap.add_argument("--path", required=True, help="仓库相对路径（单个文件）")
    ap.add_argument("--drop-pattern", action="append", default=[], help="正则：命中该行的整行丢弃（可重复）")
    ap.add_argument(
        "--drop-block",
        action="append",
        default=[],
        metavar="START::END",
        help="正则块：从命中 START 的行到其后首个命中 END 的行（含两端）整块丢弃（可重复）",
    )
    ap.add_argument("--assert-absent", action="append", default=[], help="结果中不得出现的字面串（可重复，失败即中止）")
    ap.add_argument("--expect-present", action="append", default=[], help="结果中必须出现的字面串（可重复，失败即中止）")
    ap.add_argument("--dry-run", action="store_true", help="只打印将要暂存的内容与差异，不写索引")
    ap.add_argument(
        "--allow-truncated-statement",
        action="store_true",
        help="放行「丢弃行以 = + , ( \\ 结尾而下一行保留」的截断风险（默认报错）",
    )
    ap.add_argument("--allow-new-file", action="store_true", help="HEAD 中不存在该文件时允许继续")
    ap.add_argument("--diff-limit", type=int, default=60, help="差异打印上限行数（默认 60）")
    ap.add_argument("--sample", type=int, default=10, help="丢弃行样例打印条数（默认 10）")
    args = ap.parse_args()

    path = args.path
    if path.startswith("/"):
        print(f"错误: --path 只接受仓库相对路径: {path}", file=sys.stderr)
        return 2
    target = ROOT / path
    if not target.is_file():
        print(f"错误: 文件不存在: {target}", file=sys.stderr)
        return 2

    base = head_lines(path)
    if base is None and not args.allow_new_file:
        print(f"错误: HEAD 中没有 {path}（新文件请加 --allow-new-file 确认）", file=sys.stderr)
        return 2

    blocks: list[tuple[str, str]] = []
    for spec in args.drop_block:
        if "::" not in spec:
            print(f"错误: --drop-block 需要 START::END 形式: {spec}", file=sys.stderr)
            return 2
        start, end = spec.split("::", 1)
        blocks.append((start, end))

    lines = target.read_text(encoding="utf-8").splitlines(keepends=True)

    kept: list[str] = []
    dropped: list[tuple[str, int, str]] = []
    dropped_idx: set[int] = set()
    skip_until: str | None = None
    for idx, line in enumerate(lines, 1):
        if skip_until is not None:
            dropped.append(("block", idx, line))
            dropped_idx.add(idx)
            if re.search(skip_until, line):
                skip_until = None
            continue
        hit_block = next(((s, e) for s, e in blocks if re.search(s, line)), None)
        if hit_block is not None:
            skip_until = hit_block[1]
            dropped.append(("block", idx, line))
            dropped_idx.add(idx)
            continue
        if any(re.search(p, line) for p in args.drop_pattern):
            dropped.append(("pattern", idx, line))
            dropped_idx.add(idx)
            continue
        kept.append(line)

    # 截断语句检测：丢弃行以 = + , ( \ 结尾，但其下一行被保留 ⇒ 谓词不完整（续行遗留）
    truncated: list[tuple[int, str, int, str]] = []
    for _, idx, line in dropped:
        if idx >= len(lines):
            continue
        if idx + 1 in dropped_idx:
            continue
        if re.search(r"[=+,(\\]\s*$", line.rstrip("\n")):
            truncated.append((idx, line.rstrip(), idx + 1, lines[idx].rstrip()))

    content = "".join(kept)

    print(f"== 共享文件内容级暂存: {path} ==")
    print(f"   工作区 {len(lines)} 行 → 拟暂存 {len(kept)} 行，丢弃 {len(dropped)} 行")
    by_kind: dict[str, int] = {}
    for kind, _, _ in dropped:
        by_kind[kind] = by_kind.get(kind, 0) + 1
    for kind, count in sorted(by_kind.items()):
        label = "块丢弃" if kind == "block" else "行丢弃"
        print(f"   {label}: {count} 行")
    for kind, idx, line in dropped[: args.sample]:
        print(f"   - [{kind}] L{idx}: {line.rstrip()[:120]}")
    if len(dropped) > args.sample:
        print(f"   … 其余 {len(dropped) - args.sample} 行省略")

    failures = [s for s in args.assert_absent if s in content]
    missing = [s for s in args.expect_present if s not in content]
    if failures:
        print(f"错误: 结果中仍出现禁用串（说明丢弃谓词不完整）: {failures}", file=sys.stderr)
        return 3
    if missing:
        print(f"错误: 结果中缺少必需串: {missing}", file=sys.stderr)
        return 3

    if truncated and not args.allow_truncated_statement:
        print("错误: 疑似截断语句 —— 丢弃行的下一行被保留，可能是他人改动的无标记续行:", file=sys.stderr)
        for i, text, j, nxt in truncated[:10]:
            print(f"       L{i} 已丢弃: {text[:100]}", file=sys.stderr)
            print(f"       L{j} 仍保留: {nxt[:100]}", file=sys.stderr)
        print("       处置：补全 --drop-pattern/--drop-block 覆盖整段，或确认无误后加 --allow-truncated-statement", file=sys.stderr)
        return 4

    print("--- 拟暂存内容 vs HEAD 的差异 ---")
    print(render_diff(base or [], content.splitlines(keepends=True), path, args.diff_limit))

    if args.dry_run:
        print("==> dry-run：未写索引（去掉 --dry-run 生效）")
        return 0

    blob = run_git(["hash-object", "-w", "--stdin"], text_input=content)
    if blob.returncode != 0:
        print(f"错误: git hash-object 失败: {blob.stderr}", file=sys.stderr)
        return 1
    blob_sha = blob.stdout.strip()
    mode = file_mode(path)
    upd = run_git(["update-index", "--cacheinfo", f"{mode},{blob_sha},{path}"])
    if upd.returncode != 0:
        print(f"错误: git update-index 失败: {upd.stderr}", file=sys.stderr)
        return 1

    print(f"==> 已写入索引: {path} mode={mode} blob={blob_sha[:8]}")
    print("--- 索引 vs HEAD（即将提交的内容）---")
    print(run_git(["diff", "--cached", "--stat", "--", path]).stdout.rstrip() or "  （无差异）")
    residual = run_git(["diff", "--numstat", "--", path]).stdout.strip()
    print("--- 工作区 vs 索引（本次提交不含这些行，仍留在工作区）---")
    print("  " + (residual.replace("\n", "\n  ") if residual else "（无）"))
    print("下一步: bash 04_scripts/_common/archive_scoped_git.sh --staged-only --message \"…\" --path " + path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
