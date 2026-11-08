#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 — APKSecure 壳 libsecret.so 签名串探针 runner

spawn 目标包 -> 注入 probe_sig_string_dwdlm.js -> resume -> 收 12 秒消息 -> 落盘。

只读：不写设备文件、不 patch 指令、不改包。进程崩溃（预期会崩在
ConfigurationController:342）时正常收尾并保留已收到的数据。

用法：
  python3.11 probe_sig_string_dwdlm.py [--wait 12] [--out DIR]
"""
import argparse
import json
import os
import sys
import time

import frida

PKG = "com.fightingstars.games.animal"
SO_NAME = "libsecret.so"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
DEF_OUT = os.path.join(ROOT, "09_test", "动物大联盟", "sigprobe_20261105")


def log(fh, line):
    print(line, flush=True)
    fh.write(line + "\n")
    fh.flush()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--wait", type=float, default=12.0, help="resume 后监听秒数")
    ap.add_argument("--out", default=DEF_OUT)
    ap.add_argument("--pkg", default=PKG)
    ap.add_argument("--no-spawn", action="store_true", help="改为 attach 已运行的进程")
    args = ap.parse_args()

    os.makedirs(args.out, exist_ok=True)
    log_path = os.path.join(args.out, "probe.log")
    raw_path = os.path.join(args.out, "probe_raw.jsonl")
    js_path = os.path.join(HERE, "probe_sig_string_dwdlm.js")

    with open(log_path, "w", encoding="utf-8") as fh, \
         open(raw_path, "w", encoding="utf-8") as raw:
        log(fh, "# dwdlm 签名串探针  %s" % time.strftime("%Y-%m-%d %H:%M:%S"))
        log(fh, "# pkg=%s so=%s wait=%.1fs out=%s" % (args.pkg, SO_NAME, args.wait, args.out))
        log(fh, "# frida=%s" % frida.__version__)

        dev = frida.get_usb_device(timeout=15)
        log(fh, "# device=%s %s" % (dev.id, dev.name))

        pid = None
        if args.no_spawn:
            try:
                pid = dev.get_process(args.pkg).pid
            except Exception as e:
                log(fh, "[FAIL] 找不到已运行进程: %s" % e)
                return 2
            log(fh, "# attach 已有进程 pid=%d" % pid)
        else:
            pid = dev.spawn([args.pkg])
            log(fh, "# spawn pid=%d" % pid)

        session = dev.attach(pid)

        state = {"detached": None, "errors": 0, "msgs": 0}

        def on_detached(reason, *a):
            state["detached"] = str(reason)
            log(fh, "[detached] reason=%s" % reason)

        session.on("detached", on_detached)

        with open(js_path, "r", encoding="utf-8") as f:
            source = f.read()
        script = session.create_script(source, name="probe_sig_string_dwdlm")

        def on_message(message, data):
            state["msgs"] += 1
            payload = message.get("payload")
            raw.write(json.dumps({"ts": round(time.time(), 3), "message": message},
                                 ensure_ascii=False) + "\n")
            raw.flush()
            if message.get("type") == "send":
                p = payload if isinstance(payload, dict) else {"value": payload}
                t = p.get("t", "?")
                if t == "sig_enter":
                    log(fh, "[sig_enter] #%s at=%sms sret=%s x0=%s x1=%s x2=%s x3=%s lr=%s"
                        % (p.get("n"), p.get("at_ms"), p.get("sret"), p.get("x0"),
                           p.get("x1"), p.get("x2"), p.get("x3"), p.get("lr")))
                elif t == "sig_leave":
                    log(fh, "[sig_leave] #%s at=%sms x0=%s" % (p.get("n"), p.get("at_ms"), p.get("x0")))
                elif t == "sig_ret_obj":
                    log(fh, "[sig_ret_obj] %s addr=%s raw64=%s"
                        % (p.get("tag"), p.get("addr"), p.get("raw64_hex")))
                    for s in p.get("slots") or []:
                        st = s.get("str")
                        if st and not st.get("read_fail"):
                            cls = st.get("cls") or {}
                            log(fh, "   off=0x%02x ptr=%s size=%s len=%s head4=%s der=%s hexish=%s b64ish=%s printable=%s"
                                % (s.get("off"), s.get("ptr"), s.get("size"), st.get("len"),
                                   cls.get("head4"), cls.get("der"), cls.get("hexish"),
                                   cls.get("b64ish"), cls.get("printable")))
                            log(fh, "     hex_head=%s" % st.get("hex_head"))
                            log(fh, "     ascii  =%s" % st.get("ascii_head"))
                        elif st and st.get("read_fail"):
                            log(fh, "   off=0x%02x ptr=%s size=%s READ_FAIL" % (s.get("off"), s.get("ptr"), s.get("size")))
                    if p.get("sso"):
                        log(fh, "   sso=%s" % json.dumps(p.get("sso"), ensure_ascii=False)[:300])
                    if p.get("err"):
                        log(fh, "   [ERR] %s" % p.get("err"))
                elif t == "sig_arg_head":
                    log(fh, "[sig_arg_head] #%s %s hex=%s ascii=%s"
                        % (p.get("n"), p.get("reg"), p.get("hex"), p.get("ascii")))
                elif t == "bss":
                    if p.get("err"):
                        log(fh, "[bss] at=%sms poll=%s ERR=%s" % (p.get("at_ms"), p.get("poll"), p.get("err")))
                    else:
                        log(fh, "[bss] at=%sms poll=%s why=%s flag=0x%s(u32=%s) key=%s iv=%s key_nonzero=%s"
                            % (p.get("at_ms"), p.get("poll"), p.get("why"), p.get("flag_hex"),
                               p.get("flag_u32"), p.get("key_hex"), p.get("iv_hex"), p.get("key_nonzero")))
                elif t == "module":
                    log(fh, "[module] %s base=%s size=0x%x path=%s"
                        % (p.get("name"), p.get("base"), p.get("size") or 0, p.get("path")))
                elif t == "error":
                    state["errors"] += 1
                    log(fh, "[JS ERROR] %s: %s" % (p.get("where"), p.get("msg")))
                else:
                    log(fh, "[%s] %s" % (t, json.dumps(p, ensure_ascii=False)[:400]))
            elif message.get("type") == "error":
                state["errors"] += 1
                log(fh, "[FRIDA ERROR] %s" % (message.get("stack") or message.get("description")))

        script.on("message", on_message)
        script.load()
        log(fh, "# script loaded")

        if not args.no_spawn:
            dev.resume(pid)
            log(fh, "# resumed, 监听 %.1fs" % args.wait)

        t_start = time.time()
        snap_done = False
        while time.time() - t_start < args.wait:
            if state["detached"]:
                log(fh, "# 进程已结束(%s)，提前收尾，已收到的数据全部保留" % state["detached"])
                break
            el = time.time() - t_start
            if not snap_done and el >= 3.0:
                snap_done = True
                try:
                    snap = script.exports_sync.snapshot()
                    log(fh, "[snapshot@%.1fs] %s" % (el, json.dumps(snap, ensure_ascii=False)))
                except Exception as e:
                    log(fh, "[snapshot@%.1fs] 失败(进程可能已崩): %s" % (el, e))
            time.sleep(0.2)

        # 收尾：再要一次 sigCalls
        try:
            calls = script.exports_sync.sigcalls()
            log(fh, "[final] %s" % json.dumps(calls, ensure_ascii=False))
        except Exception as e:
            log(fh, "[final] sigcalls 取不到(进程可能已崩): %s" % e)

        log(fh, "# 汇总: msgs=%d errors=%d detached=%s" % (state["msgs"], state["errors"], state["detached"]))
        try:
            if not args.no_spawn:
                dev.kill(pid)
                log(fh, "# 已 kill 探针进程 pid=%d（避免残留）" % pid)
        except Exception as e:
            log(fh, "# kill 跳过: %s" % e)

    print("log -> %s" % log_path, flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
