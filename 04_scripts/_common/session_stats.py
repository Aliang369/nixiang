#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
session_stats.py — 会话体检器（只读）

用途：把一次 DSH 会话的 jsonl(.zstd 自动解压) 汇总成「工具调用数 / 输出体量 /
热点文件 / 关键操作类 / 每轮时长 / 未使用工具」几张表，用于会话复盘与规范优化
（依据：2026-10-12 霓虹深渊2 会话效率复盘）。

用法：
    python3 04_scripts/_common/session_stats.py <session.jsonl|session.v4.jsonl.zstd> [--top N]
    python3 04_scripts/_common/session_stats.py --session-id 9a67c280-7e10-4d93-bfc7-69b16958c226
    python3 04_scripts/_common/session_stats.py --list

只读，不写任何文件（除非 --out）。
"""
import argparse
import collections
import io
import json
import os
import re
import subprocess
import sys

SESS_ROOT = os.path.expanduser("~/.dsh/sessions/--Users-aliang-Documents-nixiang--")

# 关键操作类：正则 -> 人话名（可重叠分类，统计各自的输出体量）
KEY_PATTERNS = [
    ("logcat 切片检索", r"logcat"),
    ("内联 python 分析", r"python3\s+-"),
    ("md5/shasum", r"md5|shasum|sha256"),
    ("readelf/objdump", r"readelf|objdump|otool"),
    ("文档/台账读取", r"10_docs|TOOLBOOK|AGENTS"),
    ("adb/设备", r"\badb\b"),
    ("编译出包", r"ndk-build|surgical|gradle|apksigner|zipalign"),
    ("frida", r"frida"),
    ("内存/映射读", r"/proc/\d+/(maps|mem)|libil2cpp"),
]

# 仓库已有但本会话没用的工具（用于「机制被旁路」判定）
KNOWN_TOOLS = [
    ("session_start.sh", r"session_start\.sh"),
    ("prior_art.sh", r"prior_art\.sh"),
    ("code_locate.sh", r"code_locate\.sh"),
    ("pipeline.sh", r"pipeline\.sh"),
    ("install_guard.sh", r"install_guard\.sh"),
    ("rootcause_note.sh", r"rootcause_note\.sh"),
    ("pre_install_gate.sh", r"pre_install_gate\.sh"),
    ("device_profile.sh", r"device_profile\.sh"),
    ("selftest_env.sh", r"selftest_env\.sh"),
    ("flight_recorder.sh", r"flight_recorder\.sh"),
    ("gate_note.sh", r"gate_note\.sh"),
    ("post_install_precheck.sh", r"post_install_precheck\.sh"),
    ("check_release_apk.sh", r"check_release_apk\.sh"),
    ("check_release_clean.sh", r"check_release_clean\.sh"),
    ("verify_installed_apk.sh", r"verify_installed_apk\.sh"),
    ("finalize_game_delivery.sh", r"finalize_game_delivery\.sh"),
    ("prune_output_final.sh", r"prune_output_final\.sh"),
    ("git_push_main.sh", r"git_push_main\.sh"),
]


def open_lines(path):
    if path.endswith(".zstd") or path.endswith(".zst"):
        p = subprocess.run(["zstd", "-d", "-c", path], stdout=subprocess.PIPE)
        data = p.stdout.decode("utf-8", "replace")
        return io.StringIO(data)
    return open(path, encoding="utf-8", errors="replace")


def load(path):
    calls = {}
    order = []
    res = {}
    events = collections.Counter()
    turns = {}
    turns_start = {}
    span = [None, None]
    with open_lines(path) as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                o = json.loads(line)
            except Exception:
                continue
            t = o.get("type", "?")
            events[t] += 1
            ts = o.get("time")
            if ts:
                if span[0] is None:
                    span[0] = ts
                span[1] = ts
            d = o.get("data", {}) or {}
            if t == "tool/call":
                calls[d.get("callId")] = (d.get("name"), d.get("arguments", ""), d.get("turn"))
                order.append(d.get("callId"))
            elif t == "tool/result":
                m = d.get("message", {}) or {}
                cid = d.get("toolCallId") or (m.get("source") or {}).get("callId")
                nbytes = sum(len(c.get("text", "")) for c in m.get("content", [])
                             if c.get("type") == "text")
                txt = " ".join(c.get("text", "") for c in m.get("content", [])
                               if c.get("type") == "text")
                res[cid] = (nbytes, bool(m.get("isError")), txt[:200])
            elif t == "turn/start":
                turns_start[d.get("turn")] = ts
            elif t == "turn/end":
                k = d.get("turn")
                turns[k] = (turns_start.get(k), ts, d.get("reason"))
    return calls, order, res, events, turns, span


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("path", nargs="?")
    ap.add_argument("--session-id")
    ap.add_argument("--list", action="store_true")
    ap.add_argument("--top", type=int, default=12)
    ap.add_argument("--out")
    a = ap.parse_args()

    if a.list:
        for n in sorted(os.listdir(SESS_ROOT)):
            d = os.path.join(SESS_ROOT, n)
            f = os.path.join(d, "session.v4.jsonl.zstd")
            if os.path.isdir(d) and os.path.exists(f):
                print("%10.1f MB  %s" % (os.path.getsize(f) / 1e6, n))
        return 0

    path = a.path
    if a.session_id:
        path = os.path.join(SESS_ROOT, "session-" + a.session_id, "session.v4.jsonl.zstd")
    if not path or not os.path.exists(path):
        print("缺少会话文件；用 --list 看可用会话", file=sys.stderr)
        return 2

    calls, order, res, events, turns, span = load(path)
    out = []
    w = out.append

    w("== 规模 ==")
    if span[0]:
        w("  起止: %s → %s（挂钟 %.1f h）" % (span[0], span[1], (span[1] - span[0]) / 3.6e6))
    w("  事件 %d / 工具调用 %d / 工具结果 %d / 轮次 %d"
      % (sum(events.values()), len(order), len(res), len(turns)))
    w("  压缩: prune=%d compaction=%d | LLM 重试=%d"
      % (events.get("compaction/prune", 0), events.get("compaction/start", 0),
         events.get("llm/retry", 0)))

    per_call = collections.Counter()
    per_out = collections.Counter()
    per_err = collections.Counter()
    read_b = collections.Counter()
    read_n = collections.Counter()
    bash_b = collections.Counter()
    bash_n = collections.Counter()
    per_turn = collections.Counter()
    total_out = 0
    err_text = collections.Counter()
    for cid in order:
        nm, args, turn = calls[cid]
        per_call[nm] += 1
        per_turn[turn] += 1
        b, iserr, txt = res.get(cid, (0, False, ""))
        total_out += b
        per_out[nm] += b
        if iserr:
            per_err[nm] += 1
            err_text[re.sub(r"\d+", "N", txt)[:80]] += 1
        try:
            ja = json.loads(args)
        except Exception:
            ja = {}
        if nm == "read":
            fp = ja.get("file_path", "?")
            read_b[fp] += b
            read_n[fp] += 1
        elif nm == "bash":
            cmd = ja.get("command", "")
            hit = False
            for label, pat in KEY_PATTERNS:
                if re.search(pat, cmd):
                    bash_b[label] += b
                    bash_n[label] += 1
                    hit = True
            if not hit:
                bash_b["其它"] += b
                bash_n["其它"] += 1

    w("")
    w("== 每工具：调用 / 输出 / 平均 / 最大 / 错误 ==")
    for nm, c in per_call.most_common():
        w("  %-16s calls=%5d out=%8.2fMB avg=%7.1fKB err=%d"
          % (nm, c, per_out[nm] / 1e6, per_out[nm] / c / 1024 if c else 0, per_err[nm]))
    w("  TOTAL 工具输出 = %.2f MB" % (total_out / 1e6))

    w("")
    w("== read 热点（累计读取体量）==")
    for fp, b in read_b.most_common(a.top):
        w("  %8.0fKB  %4d 次  %s" % (b / 1024, read_n[fp], fp))
    w("  read 累计 %.0f KB / %d 次 / %d 个文件"
      % (sum(read_b.values()) / 1024, sum(read_n.values()), len(read_b)))

    w("")
    w("== bash 输出分类（可重叠）==")
    for label, b in bash_b.most_common():
        w("  %8.0fKB  %5d 次  %s" % (b / 1024, bash_n[label], label))

    w("")
    w("== 每轮：调用数 / 时长 ==")
    for t in sorted([x for x in turns if x is not None]):
        s, e, reason = turns[t]
        dur = (e - s) / 60000 if s and e else 0
        w("  turn %3d calls=%5d dur=%7.1f min" % (t, per_turn[t], dur))
    active = sum((turns[t][1] - turns[t][0]) / 60000 for t in turns
                 if t is not None and turns[t][0] and turns[t][1])
    w("  活跃时长 %.1f min (%.1f h)" % (active, active / 60))

    w("")
    w("== 已有工具使用情况（0 次 = 机制被旁路）==")
    allcmd = []
    for cid in order:
        nm, args, _ = calls[cid]
        if nm != "bash":
            continue
        try:
            allcmd.append(json.loads(args).get("command", ""))
        except Exception:
            pass
    zero = []
    for label, pat in KNOWN_TOOLS:
        n = sum(1 for c in allcmd if re.search(pat, c))
        if n == 0:
            zero.append(label)
        w("  %5d 次  %s" % (n, label))
    if zero:
        w("  ⚠ 未使用：%s" % "、".join(zero))

    w("")
    w("== 工具错误（去数字后归并）==")
    for txt, n in err_text.most_common(8):
        w("  %4d x %s" % (n, txt))

    text = "\n".join(out)
    print(text)
    if a.out:
        with open(a.out, "w", encoding="utf-8") as f:
            f.write(text + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
