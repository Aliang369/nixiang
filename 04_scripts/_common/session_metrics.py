#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""session_metrics.py — 会话效率度量（只读）

用途：把 AGENTS.md「## 会话效率纪律（D1–D7）」的**次数类**纪律变成可验证数字。
数据源：DSH 会话文件 `~/.dsh/sessions/--<项目>--/<session-id>/session.v4.jsonl[.zstd]`。

统计项与对应纪律：
  * calls/step 分布 + 单发占比            → D1 目标 calls/step ≥1.6、单发占比 <60%
  * bash 占比、bash 里做文本检索的占比     → D3「别把 bash 当检索器」（目标 <50%）
  * 单条工具结果 >8 KB 的条目              → D3 目标 0 条
  * 单文件触达 top-N                       → A2/D3 大文件热点（read/edit/bash 参数里出现的路径）
  * 单条 user 消息最大字符                 → D5 子代理回传目标 ≤3,000 字符
  * compress 次数 + prune 阴影条数          → D7 触发水位 ≤90 K、一块一主题
  * 上下文峰值 / 缓存命中率 / reasoning 量  → 只报告不设阈值（缓存已到顶，无单价空间）

只读：不改会话文件；只有显式 `--out` 才写一个报告文件。

用法
----
    python3 04_scripts/_common/session_metrics.py --list
    python3 04_scripts/_common/session_metrics.py --session-id 55fc139c
    python3 04_scripts/_common/session_metrics.py --latest 2 --top 15 --out /tmp/metrics.md
    python3 04_scripts/_common/session_metrics.py --session-id 62fb2110 --gate   # 违规时 rc=3

退出码：0 正常；2 参数/文件问题（zstd 缺失等）；3 仅 `--gate` 且有违规。
"""

from __future__ import annotations

import argparse
import glob as _glob
import json
import re
import shutil
import subprocess
import sys
from collections import Counter
from pathlib import Path

SESS_ROOT = Path.home() / ".dsh" / "sessions"

GATES = {
    "calls_per_step": 1.6,        # D1
    "single_call_ratio": 0.60,    # D1
    "single_result_kb": 8.0,      # D3
    "bash_search_ratio": 0.50,    # D3（现状 73–75%）
    "subagent_msg_chars": 3000,   # D5
}
SEARCH_RE = re.compile(r"\b(grep|rg|awk|sed\s+-n|head|tail|cut|sort|uniq|wc|cat\s+)\b")
FILE_RE = re.compile(r"[\w./\-]{4,}\.(?:cs|json|txt|log|md|py|sh|cpp|h|xml|tsv|java|so|apk)\b")

_WS_HINT = ("/Users/", "10_docs/", "06_dumped/", "04_scripts/", "09_test/", "mod菜单/",
            "AGENTS.md", "TOOLBOOK.md", "skill/", "_templates/", "_state/", "08_output/", "02_apks/")
_KEEP_EXT = (".cs", ".json", ".txt", ".log", ".md", ".py", ".sh", ".cpp", ".h", ".xml",
             ".tsv", ".java", ".so", ".apk")


def extract_files(args: str) -> set[str]:
    """从工具参数（JSON 字符串）里抽出「工作区文件」路径，过滤 /tmp、系统路径与噪声。"""
    out: set[str] = set()
    for raw in FILE_RE.findall(args):
        tok = raw.strip('\\"\'),:;')
        if not any(h in tok for h in _WS_HINT):
            continue
        if "/tmp/" in tok or "/var/" in tok or "/opt/" in tok or "/usr/" in tok:
            continue
        # 归一化：截到扩展名结尾（丢掉尾随的 quote/空格等）
        low = tok.lower()
        for e in _KEEP_EXT:
            i = low.rfind(e)
            if i >= 0:
                tok = tok[: i + len(e)]
                break
        out.add(tok)
    return out


# --------------------------------------------------------------------------- #
# 会话发现与读取
# --------------------------------------------------------------------------- #
def find_sessions() -> list[Path]:
    best: dict[str, Path] = {}
    for pat in ("**/session.v4.jsonl", "**/session.v4.jsonl.zstd"):
        for p in _glob.glob(str(SESS_ROOT / pat), recursive=True):
            p = Path(p)
            sid = p.parent.name
            if sid not in best or p.suffix == ".jsonl":
                best[sid] = p
    return sorted(best.values(), key=lambda p: p.stat().st_mtime, reverse=True)


def sid_of(p: Path) -> str:
    return p.parent.name.replace("session-", "")[:8]


def load_events(p: Path) -> list[dict]:
    if p.suffix == ".zstd":
        if shutil.which("zstd") is None:
            raise SystemExit("需要 zstd 解压 .jsonl.zstd（brew install zstd）")
        raw = subprocess.run(["zstd", "-dc", str(p)], capture_output=True, check=True).stdout
        lines = raw.decode("utf-8", "replace").splitlines()
    else:
        lines = p.read_text(encoding="utf-8", errors="replace").splitlines()
    evs = []
    for ln in lines:
        try:
            evs.append(json.loads(ln))
        except Exception:
            pass
    return evs


def text_of(content) -> str:
    """从 message.content（str | list[dict]）取纯文本。"""
    if isinstance(content, str):
        return content
    if isinstance(content, list):
        return "".join(str(c.get("text") or "") for c in content if isinstance(c, dict))
    return ""


# --------------------------------------------------------------------------- #
# 统计
# --------------------------------------------------------------------------- #
def collect(paths: list[Path], top: int) -> dict:
    m = {
        "sessions": [], "steps": 0, "calls": 0, "call_by_step": Counter(), "tool": Counter(),
        "bash_total": 0, "bash_search": 0, "bash_fail": 0,
        "bash_bytes": 0, "read_bytes": 0, "read": 0, "edit": 0,
        "result_kb": [], "hot": Counter(), "compress": 0, "prune": 0, "prune_tokens": 0,
        "user_msgs": [], "input_tokens": 0, "cached_tokens": 0, "output_tokens": 0,
        "peak_ctx": 0, "reasoning_bytes": 0, "turns": 0,
    }
    for path in paths:
        evs = load_events(path)
        sid = sid_of(path)
        inf = {"id": sid, "lines": len(evs), "steps": 0, "calls": 0, "turns": 0}
        calls: dict[str, tuple[str, str]] = {}
        per_step: Counter = Counter()

        for ev in evs:
            t = ev.get("type", "")
            d = ev.get("data") or {}
            if not isinstance(d, dict):
                continue
            if t == "turn/start":
                m["turns"] += 1
                inf["turns"] += 1
            elif t == "step/start":
                m["steps"] += 1
                inf["steps"] += 1
            elif t == "tool/call":
                name = str(d.get("name") or "?")
                args = d.get("arguments")
                if not isinstance(args, str):
                    args = json.dumps(args, ensure_ascii=False)
                calls[str(d.get("callId") or "")] = (name, args)
                m["tool"][name] += 1
                m["calls"] += 1
                inf["calls"] += 1
                per_step[(d.get("turn"), d.get("step"))] += 1   # turn+step 唯一（step 号每轮重置）
            elif t == "tool/result":
                cid = str(d.get("toolCallId") or (d.get("message") or {}).get("source", {}).get("callId") or "")
                name, args = calls.get(cid, ("?", ""))
                body = text_of(d.get("content")) or text_of((d.get("message") or {}).get("content"))
                nb = len(body.encode("utf-8"))
                if name == "bash":
                    m["bash_total"] += 1
                    m["bash_bytes"] += nb
                    if SEARCH_RE.search(args):
                        m["bash_search"] += 1
                    rc = re.search(r"\[exit code:\s*(\d+)", body)
                    if rc and rc.group(1) != "0":
                        m["bash_fail"] += 1
                    for f in extract_files(args):
                        m["hot"][f] += 1
                elif name == "read":
                    m["read"] += 1
                    m["read_bytes"] += nb
                    for f in extract_files(args):
                        m["hot"][f] += 1
                elif name == "edit":
                    m["edit"] += 1
                    for f in extract_files(args):
                        m["hot"][f] += 1
                if nb > int(GATES["single_result_kb"] * 1024):
                    m["result_kb"].append((nb / 1024.0, name))
            elif t == "assistant/message":
                u = d.get("usage")
                if isinstance(u, dict) and u.get("inputTokens"):
                    m["input_tokens"] += int(u.get("inputTokens") or 0)
                    m["cached_tokens"] += int(u.get("cacheReadTokens") or 0)
                    m["output_tokens"] += int(u.get("outputTokens") or 0)
                    m["peak_ctx"] = max(m["peak_ctx"], int(u.get("totalTokens") or 0))
                for c in (d.get("message") or {}).get("content") or []:
                    if isinstance(c, dict) and c.get("type") == "reasoning":
                        m["reasoning_bytes"] += len(str(c.get("text") or "").encode("utf-8"))
            elif t == "user/message":
                m["user_msgs"].append((len(text_of(d.get("content")).encode("utf-8")), sid))
            elif t == "compaction/start":
                m["compress"] += 1
            elif t == "compaction/prune":
                m["prune"] += 1
                m["prune_tokens"] += int(d.get("shadowedTokenCount") or 0)

        # 分布：键 = 该 step 的调用数（0 值不参与），值 = 这样的 step 有几个
        m["call_by_step"] += Counter(c for k, c in per_step.items() if k is not None and c)
        m["sessions"].append(inf)

    m["hot_top"] = m["hot"].most_common(top)
    m["big_msgs"] = sorted(m["user_msgs"], reverse=True)[:top]
    return m


def report(m: dict, top: int) -> tuple[str, list[str]]:
    bad: list[str] = []
    o: list[str] = []
    A = o.append
    # 每 step 调用数分布：由 call_by_step 还原（键=调用数，值=step 数）
    A("# 会话效率度量（session_metrics.py，只读）")
    A("")
    A("| 会话 | 事件行 | turn | step | 调用 |")
    A("|---|---|---|---|---|")
    for s in m["sessions"]:
        A("| `%s` | %d | %d | %d | %d |" % (s["id"], s["lines"], s["turns"], s["steps"], s["calls"]))
    A("")
    steps = max(1, m["steps"])
    cps = m["calls"] / steps

    A("## D1 并行批次纪律")
    A("- calls/step = **%.2f**（阈值 ≥%.1f）" % (cps, GATES["calls_per_step"]))
    if cps < GATES["calls_per_step"]:
        bad.append("D1 calls/step %.2f < %.1f" % (cps, GATES["calls_per_step"]))
    dist = m["call_by_step"]
    nsteps = max(1, sum(dist.values()))
    single = dist.get(1, 0) / nsteps
    A("- 每 step 调用数分布：%s ｜ 单发占比 **%.1f%%**（阈值 <%.0f%%）"
      % (", ".join("%d个:%d步" % (k, v) for k, v in sorted(dist.items())),
         single * 100, GATES["single_call_ratio"] * 100))
    if single > GATES["single_call_ratio"]:
        bad.append("D1 单发占比 %.1f%% > %.0f%%" % (single * 100, GATES["single_call_ratio"] * 100))
    A("- 工具用量：%s" % "、".join("`%s`×%d" % (k, v) for k, v in m["tool"].most_common(8)))
    if m["bash_total"]:
        A("- bash 占全部调用 %.1f%%（原生 `read` 仅 %d 次）"
          % (100.0 * m["bash_total"] / max(1, m["calls"]), m["read"]))
    A("- 子代理类工具：task/subagent×%d、workflow×%d、job_output×%d（判断是否把主线程的活外包）"
      % (m["tool"].get("subagent", 0) + m["tool"].get("task", 0),
         m["tool"].get("workflow", 0), m["tool"].get("job_output", 0)))

    A("")
    A("## D3 检索纪律")
    bt = max(1, m["bash_total"])
    ratio = 100.0 * m["bash_search"] / bt
    A("- bash %d 次，其中含 grep/rg/awk/sed -n/head/tail/cut/sort/wc 的 **%d 次（%.1f%%）**（阈值 <%.0f%%）"
      % (m["bash_total"], m["bash_search"], ratio, GATES["bash_search_ratio"] * 100))
    A("- 入上下文体量：bash %.0f KB + read %.0f KB" % (m["bash_bytes"] / 1024.0, m["read_bytes"] / 1024.0))
    if ratio > GATES["bash_search_ratio"] * 100:
        bad.append("D3 bash 检索占比 %.1f%% > %.0f%%（改用 grep/read 原生工具）"
                   % (ratio, GATES["bash_search_ratio"] * 100))
    A("- 单条工具结果 >%.0f KB：**%d 条**（阈值 0 条）" % (GATES["single_result_kb"], len(m["result_kb"])))
    for kb, name in sorted(m["result_kb"], reverse=True)[:5]:
        A("    - %.1f KB  `%s`" % (kb, name))
    if m["result_kb"]:
        bad.append("D3 单条工具结果 >8 KB：%d 条（最大 %.1f KB）"
                   % (len(m["result_kb"]), max(k for k, _ in m["result_kb"])))
    A("- bash 非零退出 %d 次" % m["bash_fail"])

    if m["hot_top"]:
        A("")
        A("## 单文件触达 Top（read/edit/bash 参数中的路径）")
        for f, c in m["hot_top"]:
            A("- %d 次  `%s`" % (c, f))

    A("")
    A("## D5 单条入父上下文体量")
    A("- 单条 user 消息最大 **%d 字符**（阈值 ≤%d）"
      % (m["big_msgs"][0][0] if m["big_msgs"] else 0, GATES["subagent_msg_chars"]))
    if m["big_msgs"] and m["big_msgs"][0][0] > GATES["subagent_msg_chars"]:
        bad.append("D5 单条消息 %d 字符 > %d（改「结论 ≤20 行 + 落盘路径」）"
                   % (m["big_msgs"][0][0], GATES["subagent_msg_chars"]))

    A("")
    A("## D7 压缩与预算")
    A("- compress %d 次 ｜ compaction/prune %d 条 / 阴影 %d tokens（触发水位目标 ≤90 K）"
      % (m["compress"], m["prune"], m["prune_tokens"]))
    A("- 上下文峰值 %d tokens ｜ 未命中缓存输入 %d ｜ 缓存读 %d ｜ 输出 %d"
      % (m["peak_ctx"], m["input_tokens"], m["cached_tokens"], m["output_tokens"]))
    if m["input_tokens"] + m["cached_tokens"]:
        A("- 缓存命中率 %.1f%%（已到顶，单价无优化空间）"
          % (100.0 * m["cached_tokens"] / (m["input_tokens"] + m["cached_tokens"])))
    A("- reasoning 输出 %.0f KB" % (m["reasoning_bytes"] / 1024.0))

    A("")
    if bad:
        A("## 闸门结果：FAIL（%d 项）" % len(bad))
        for b in bad:
            A("- ✗ %s" % b)
    else:
        A("## 闸门结果：PASS（D1/D3/D5 全部达阈）")
    return "\n".join(o), bad


def pick_sessions(args) -> list[Path]:
    sessions = find_sessions()
    if not sessions:
        raise SystemExit("在 %s 下没找到 session.v4.jsonl[.zstd]" % SESS_ROOT)
    if args.path:
        return [Path(args.path).expanduser()]
    if args.session_id:
        hits = [p for p in sessions if args.session_id in str(p)]
        if not hits:
            raise SystemExit("没有匹配 %r 的会话；用 --list 看全部" % args.session_id)
        return hits
    if args.latest:
        return sessions[: max(1, args.latest)]
    raise SystemExit("用 --session-id / --latest N / --path 指定会话（或 --list）")


def main() -> int:
    ap = argparse.ArgumentParser(description="DSH 会话效率度量（只读）")
    ap.add_argument("--list", action="store_true", help="列出全部会话文件")
    ap.add_argument("--session-id", help="会话 id 片段")
    ap.add_argument("--latest", type=int, help="统计最近 N 个会话（合计）")
    ap.add_argument("--path", help="直接指定 session.v4.jsonl[.zstd]")
    ap.add_argument("--top", type=int, default=10, help="top-N（默认 10）")
    ap.add_argument("--out", help="同时写一份报告（默认只打 stdout）")
    ap.add_argument("--gate", action="store_true", help="有违规时退出码 3")
    args = ap.parse_args()

    if args.list:
        for p in find_sessions():
            print("%s  %8.1f MB  %s" % (sid_of(p), p.stat().st_size / 1048576.0, p))
        return 0

    paths = pick_sessions(args)
    m = collect(paths, args.top)
    text, bad = report(m, args.top)
    print(text)
    if args.out:
        Path(args.out).write_text(text + "\n", encoding="utf-8")
        print("\n[已写] %s" % args.out)
    return 3 if (bad and args.gate) else 0


if __name__ == "__main__":
    sys.exit(main())
