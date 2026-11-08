#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm 1.0.25 — 经 su 域 helper 读目标 app 的 /proc/<pid>/mem，快照壳 .bss 三元组。

为什么这样做：frida 直接注入 app 进程被 SELinux 阻断（agent 在 untrusted_app 域起不来，
frida-server 也装不上自己的 selinux policy），但注入 su/ksu 域进程可行，
且 root 可 open /proc/<app>/mem。于是把 frida 挂在 helper 上，用 File API 2ms 轮询目标 .bss，
抢在崩溃前抓住 KEY/IV/flag。

只读：不写目标内存、不改包、不卸载、不清数据。只启动一次游戏（monkey 1 event）。
"""
import argparse
import json
import os
import re
import subprocess
import sys
import threading
import time

import frida

PKG = "com.fightingstars.games.animal"
ADB = "/Users/aliang/Library/Android/sdk/platform-tools/adb"
SERIAL = "f9a116a2"
REMOTE = "127.0.0.1:27042"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "09_test", "动物大联盟", "sigprobe_20261105")


def adb(args, timeout=60):
    try:
        p = subprocess.run([ADB, "-s", SERIAL, "shell"] + args, capture_output=True, text=True, timeout=timeout)
        return (p.stdout or "") + (p.stderr or "")
    except Exception as e:
        return "[ADB-ERR] %s" % e


def adb_su(cmd, timeout=60):
    return adb(["su", "-c", cmd], timeout=timeout)


def pkg_pids():
    """返回目标包所有进程 pid（含 :子进程）"""
    out = adb_su("ps -A -o PID,NAME | grep '%s' | grep -v grep" % PKG)
    pids = []
    for line in out.splitlines():
        parts = line.split()
        if len(parts) >= 2 and parts[0].isdigit() and parts[1].startswith(PKG):
            pids.append(int(parts[0]))
    return pids


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--wait", type=float, default=30.0)
    ap.add_argument("--out", default=OUT)
    ap.add_argument("--no-launch", action="store_true")
    args = ap.parse_args()

    os.makedirs(args.out, exist_ok=True)
    log_path = os.path.join(args.out, "probe_memread.log")
    raw_path = os.path.join(args.out, "probe_memread.jsonl")
    js_path = os.path.join(HERE, "probe_bss_memread_dwdlm.js")

    with open(log_path, "w", encoding="utf-8") as fh, open(raw_path, "w", encoding="utf-8") as raw:
        lock = threading.Lock()

        def log(s):
            with lock:
                print(s, flush=True)
                fh.write(s + "\n")
                fh.flush()

        log("# dwdlm .bss memread 探针  %s" % time.strftime("%Y-%m-%d %H:%M:%S"))
        log("# frida=%s remote=%s pkg=%s" % (frida.__version__, REMOTE, PKG))
        pre = pkg_pids()
        log("# 启动前目标 pid 列表: %r" % pre)

        helper = None
        m = re.search(r"(\d+)", adb_su("nohup sleep 900 >/dev/null 2>&1 & echo $!"))
        if m:
            helper = int(m.group(1))
        log("# helper(sleep, su/ksu 域) pid=%s" % helper)

        dev = frida.get_device_manager().add_remote_device(REMOTE)
        # helper 刚 fork 出来时可能还没 exec 完，attach 会撞 "refused to load frida-agent"
        sess = None
        for attempt in range(1, 6):
            time.sleep(0.3)
            alive = adb_su("ps -p %d -o PID,NAME 2>/dev/null | tail -n +2" % helper).strip()
            if not alive:
                log("[WARN] helper %d 不在，重启一个" % helper)
                m = re.search(r"(\d+)", adb_su("nohup sleep 900 >/dev/null 2>&1 & echo $!"))
                if not m:
                    log("[FAIL] 起不来 helper")
                    return 4
                helper = int(m.group(1))
                log("# helper 重起 pid=%s" % helper)
                continue
            try:
                sess = dev.attach(helper)
                log("# attach helper 成功(第 %d 次) pid=%d" % (attempt, helper))
                break
            except Exception as e:
                log("[WARN] attach helper 第 %d 次失败: %s" % (attempt, e))
        if sess is None:
            log("[FAIL] 无法 attach helper，通道不可用")
            return 4
        src = open(js_path, "r", encoding="utf-8").read()
        script = sess.create_script(src, name="probe_bss_memread")
        ev = {"ready": False, "bss": 0, "found": 0, "msgs": 0, "detached": None, "watched": []}

        def on_message(message, data):
            ev["msgs"] += 1
            raw.write(json.dumps({"ts": round(time.time(), 3), "m": message}, ensure_ascii=False) + "\n")
            raw.flush()
            if message.get("type") != "send":
                log("[FRIDA ERR] %s" % (message.get("stack") or message.get("description")))
                return
            p = message.get("payload") or {}
            t = p.get("t")
            if t == "ready":
                ev["ready"] = True
                log("[ready] %s" % json.dumps(p, ensure_ascii=False))
            elif t == "found":
                ev["found"] += 1
                log("[found] pid=%s base=%s switch=%s const32=%s"
                    % (p.get("pid"), p.get("base"), p.get("switch_byte"), p.get("const32_hex")))
                for line in (p.get("maps") or []):
                    log("   map: %s" % line)
            elif t == "bss":
                ev["bss"] += 1
                log("[bss] pid=%s at=%sms flag=0x%s(u32=%s) key=%s iv=%s first_key=%s"
                    % (p.get("pid"), p.get("at_ms"), p.get("flag_hex"), p.get("flag_u32"),
                       p.get("key_hex"), p.get("iv_hex"), p.get("first_key")))
            elif t == "dead":
                log("[dead] pid=%s at=%sms %s" % (p.get("pid"), p.get("at_ms"), p.get("msg")))
            elif t == "note":
                log("[note] %s" % json.dumps(p, ensure_ascii=False)[:300])
            elif t == "error":
                log("[JS ERR] %s %s" % (p.get("where"), p.get("msg")))
            else:
                log("[%s] %s" % (t, json.dumps(p, ensure_ascii=False)[:300]))

        def on_detached(reason, *a):
            ev["detached"] = str(reason)
            log("[detached] %s" % reason)

        sess.on("detached", on_detached)
        script.on("message", on_message)
        script.load()
        t_end = time.time() + 5
        while time.time() < t_end and not ev["ready"]:
            time.sleep(0.05)
        if not ev["ready"]:
            log("[FAIL] watcher 未就绪")
            return 3

        # 只启动一次游戏
        if not args.no_launch:
            out = adb_su("monkey -p %s -c android.intent.category.LAUNCHER 1" % PKG, timeout=90)
            log("# 启动游戏(monkey 1 event) 输出: %s" % out.replace("\n", " | ")[:400])

        # 边等边把新出现的 pid 交给 watcher（frida17 无进程枚举，只能这样拿 pid）
        t0 = time.time()
        next_pid_poll = 0.0
        next_status = 8.0
        while time.time() - t0 < args.wait:
            if ev["detached"]:
                log("# helper session 断开，提前收尾")
                break
            el = time.time() - t0
            if el >= next_pid_poll:
                next_pid_poll = el + 0.05
                for pid in pkg_pids():
                    if pid not in ev["watched"]:
                        ev["watched"].append(pid)
                        try:
                            script.exports_sync.watch(pid)
                            log("# watch pid=%d (t=%.2fs)" % (pid, el))
                        except Exception as e:
                            log("# watch pid=%d 失败: %s" % (pid, e))
            if el >= next_status:
                next_status = el + 10.0
                try:
                    log("[status@%.0fs] %s" % (el, json.dumps(script.exports_sync.status(), ensure_ascii=False)))
                except Exception as e:
                    log("[status@%.0fs] 失败: %s" % (el, e))
            time.sleep(0.02)

        try:
            log("[final status] %s" % json.dumps(script.exports_sync.status(), ensure_ascii=False))
        except Exception as e:
            log("[final status] 失败: %s" % e)
        log("# 汇总: found=%d bss=%d msgs=%d watched=%r" % (ev["found"], ev["bss"], ev["msgs"], ev["watched"]))
        log("# 目标进程现状: %r" % pkg_pids())

        try:
            script.exports_sync.stop()
        except Exception:
            pass
        try:
            sess.detach()
        except Exception:
            pass
        if helper:
            adb_su("kill %d 2>/dev/null" % helper)
            log("# helper 已清理 pid=%d" % helper)

    print("log -> %s" % log_path, flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
