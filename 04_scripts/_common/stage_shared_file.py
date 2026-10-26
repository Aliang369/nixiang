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

  「他人改行」场景（如 current.md 索引行被别会话缩短）用 `--restore-head-pattern`：
  命中该正则的工作区行会被 HEAD 里命中同一正则的行**替换**（恢复旧版），而非 `--drop-pattern`
  的「整行删除」。例：
    --restore-head-pattern '生存33天|弓箭传说2国际版|迷雾大陆' \
    --own-pattern '炁术行者' --dry-run
  （2026-10-18 炁术行者会话实测：current.md 5 处他人改行 + 1 处本轮改行，用 restore 恢复他人行、
  own-pattern 校验本轮行，一次写索引，不再手动构造 content。）
  ★ 每个 --restore-head-pattern 必须是**单一关键词**（如 `--restore-head-pattern '生存33天'` 可重复传），
  勿用 `|` 组合（如 `'生存33天|迷雾大陆'`）：组合正则会让「HEAD 第一个命中任一关键词的行」错配到别行。

验证铁律（2026-09-30 实际漏过一次，16 行）：
  **禁止用「grep 标记串 = 0」代替逐行核对**。他人改动常含**无标记的续行**
  （例：伊莫会话的 `YIMO_CERT_B64 =` 后跟 15 行 base64 字符串续行，续行里既没有
  `伊莫` 也没有 `YIMO_`）。丢弃谓词只按标记串匹配时，声明行被丢、续行留下 ⇒
  提交里混入他人半截代码。本脚本因此加了「截断语句」检测：被丢弃的行若以
  `=` `+` `,` `(` `\\` 结尾且其下一行被保留，直接报错（用 --allow-truncated-statement 放行）。

Windows 兼容（2026-10-03 Win 端实测补齐，mac 行为不变；两处都是真崩/真写错，不是洁癖）：
  ① `git show` 等子进程输出必须显式 `encoding="utf-8"`。不写就按本机 locale 解码，Windows
     上是 cp936 → 输出里的 UTF-8 字节抛 `UnicodeDecodeError: 'gbk' codec can't decode
     byte 0xaa`；异常发生在 Popen 的读取线程里，主线程只看到 `res.stdout is None`，
     报成 `AttributeError: 'NoneType' object has no attribute 'splitlines'`（head_lines）。
     配 `errors="surrogateescape"` 保证字节级无损（含读工作区文件那一处）。
  ② 写索引的 `hash-object --stdin` 必须走 **bytes 输入**（`input=data`，非文本模式），
     并加 `--path` 让 checkin filter 生效。文本模式 stdin 会把 `\n` 翻成 `os.linesep`，
     Windows 上即 `\r\n` → 索引里写出 CRLF blob（实测 CR=1251 / 111447 B，工作区仅
     110196 B），`git diff --cached` 退化成整文件重写（1251+/1232−）并破坏「全仓 LF」。
     mac 因 `os.linesep == "\n"` 无此问题，故此缺陷只在 Win 端暴露。
  ③ `run_git` 只用于**读** git 输出；任何往 git 写字节的动作（`hash-object --stdin`）都必须
     走 `hash_blob()` 的 bytes 通道。注意 `subprocess.Popen` **没有** `newline=` 参数
     （Python 3.11 实测 `TypeError: Popen.__init__() got an unexpected keyword argument
     'newline'`），文本模式的换行翻译关不掉 ⇒ 只能绕开文本模式。写入索引后再复核 blob
     字节数（见 hash_blob），防止换行/编码被静默改写。
"""
from __future__ import annotations

import argparse
import difflib
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


# 见文件头「Windows 兼容」①：显式编码，避免按本机 locale（Windows=cp936）解码
GIT_TEXT_KW: dict[str, str] = {
    "encoding": "utf-8",
    "errors": "surrogateescape",
}


def run_git(args: list[str]) -> subprocess.CompletedProcess:
    """只读调用 git（不传 stdin）。写对象请用 hash_blob。"""
    return subprocess.run(
        ["git", *args],
        cwd=ROOT,
        capture_output=True,
        text=True,
        **GIT_TEXT_KW,
    )


def hash_blob(content: str, path: str) -> tuple[str | None, str]:
    """把内容写成 git blob（字节级），返回 (blob sha, 错误信息)。

    见文件头「Windows 兼容」②：bytes 输入 + `--path`（filter 生效），与 `git add`
    的落库结果一致；写完用 `cat-file -s` 复核字节数，不符即拒绝写索引。
    """
    data = content.encode("utf-8", "surrogateescape")
    res = subprocess.run(
        ["git", "hash-object", "-w", "--path", path, "--stdin"],
        cwd=ROOT,
        capture_output=True,
        input=data,
    )
    if res.returncode != 0:
        return None, res.stderr.decode("utf-8", "replace").strip()
    blob_sha = res.stdout.decode("ascii", "replace").strip()
    size_res = run_git(["cat-file", "-s", blob_sha])
    got = size_res.stdout.strip() if size_res.returncode == 0 else ""
    # filter 归一化（CRLF→LF）是唯一允许的字节数变化，其余一律视为被改写
    allowed = {len(data), len(data.replace(b"\r\n", b"\n"))}
    if not got.isdigit() or int(got) not in allowed:
        return None, (
            f"blob 字节数不符：写入 {len(data)} B，git 报 {got or '?'} B —— "
            "换行/编码被改写，拒绝写索引"
        )
    return blob_sha, ""


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
    # Windows 控制台默认 cp936：打印无法编码的字符（他人改动的原样字节）会抛
    # UnicodeEncodeError，这里只把「无法编码」的字符降级为 \xNN，不影响正常输出。
    for stream in (sys.stdout, sys.stderr):
        if hasattr(stream, "reconfigure"):
            stream.reconfigure(errors="backslashreplace")
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
    ap.add_argument(
        "--own-pattern",
        action="append",
        default=[],
        help="正则：相对 HEAD 的**每一条改动行**都必须命中其中之一，否则中止（逐行归属校验，防混入别会话行）",
    )
    ap.add_argument(
        "--restore-head-pattern",
        action="append",
        default=[],
        help="正则：命中该正则的工作区行，用 HEAD 里命中同一正则的行替换（他人改行→恢复 HEAD 旧版，而非删除；可重复）",
    )
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

    lines = target.read_text(encoding="utf-8", errors="surrogateescape").splitlines(keepends=True)

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
        # --restore-head-pattern：他人改行 → 用 HEAD 里命中同一正则的行替换（而非删除）。
        # 供 current.md 等共享文件「别会话改行」场景：把该行恢复成 HEAD 旧版，工作区原样保留。
        restored = False
        for p in args.restore_head_pattern:
            if re.search(p, line):
                head_line = next((h for h in (base or []) if re.search(p, h)), None)
                if head_line is not None:
                    kept.append(head_line)          # 恢复 HEAD 旧版
                    restored = True
                else:
                    dropped.append(("restore-no-head", idx, line))  # HEAD 无对应行（他人新增）→ 丢弃
                    dropped_idx.add(idx)
                    restored = True
                break
        if restored:
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

    # ---- 归属过滤（2026-10-16 P0-1）：防「并行会话的行随本次内容级暂存一起入库」 ----
    # 背景：复盘会话实测——current.md 里并行会话的「冒险与征途 v48」行被一并暂存并提交（d1905e3b）。
    # --own-pattern 给出「只属于本轮」的正则后，**每一条相对 HEAD 的改动行都必须命中**其中至少一条，
    # 否则中止并逐行列出，供补 --drop-pattern 后重跑。
    changed: list[str] = []
    for dl in difflib.unified_diff(base or [], content.splitlines(keepends=True), n=0):
        if dl.startswith(("+++", "---")):
            continue
        if dl[:1] in ("+", "-"):
            changed.append(dl[1:])
    if args.own_pattern:
        unmatched = [ln for ln in changed if not any(re.search(p, ln) for p in args.own_pattern)]
        if unmatched:
            print(f"错误: 有 {len(unmatched)} 行改动未命中任何 --own-pattern（疑似别会话内容）:", file=sys.stderr)
            for ln in unmatched[:20]:
                print(f"       {ln.rstrip()[:120]}", file=sys.stderr)
            print("       处置：补 --drop-pattern/--drop-block 丢弃他人行，或补全 --own-pattern 后重跑。", file=sys.stderr)
            return 5
        print(f"归属过滤: 改动行 {len(changed)} 行全部命中 --own-pattern ✓")
    elif changed and not (args.drop_pattern or args.drop_block):
        print("⚠ 未提供 --own-pattern/--drop-pattern：该文件**全部**改动行都会进入暂存；"
              "并行会话也在改此文件时其行会一并入库。建议加 --own-pattern '<只属本轮的串>'。")

    print("--- 拟暂存内容 vs HEAD 的差异 ---")
    print(render_diff(base or [], content.splitlines(keepends=True), path, args.diff_limit))

    if args.dry_run:
        print("==> dry-run：未写索引（去掉 --dry-run 生效）")
        return 0

    blob_sha, err = hash_blob(content, path)
    if blob_sha is None:
        print(f"错误: git hash-object 失败: {err}", file=sys.stderr)
        return 1
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
