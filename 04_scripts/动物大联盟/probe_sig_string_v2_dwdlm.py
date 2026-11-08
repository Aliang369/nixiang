#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm 1.0.25 — libsecret.so 签名串探针 v2 runner

v1 失败根因（本轮实测复现）：SELinux=Enforcing 时 frida-agent 注入目标 app 被拒，
dev.attach(pid) 抛 TransportError: agent connection closed unexpectedly，
驱动在 attach 处异常退出 => probe.log 只有头 + probe_raw.jsonl 0 字节。

v2 流程：
  getenforce 存档 -> 必要时 setenforce 0（permissive）-> spawn -> attach ->
  create_script/load（resume 前装好，才能抓到早期 dlopen）-> resume ->
  监听 --wait 秒 -> 收尾（kill + 恢复 SELinux 原状态）。
attach / load 失败时不 resume（等于没启动游戏），保留现场供下一步。

用法：
  python3.11 probe_sig_string_v2_dwdlm.py [--wait 30] [--no-setenforce]
"""
import argparse
import json
import os
import shutil
import subprocess
import sys
import time

import frida

PKG = "com.fightingstars.games.animal"
SERIAL = "f9a116a2"
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
DEF_OUT = os.path.join(ROOT, "09_test", "动物大联盟", "sigprobe_20261105")
ADB = shutil.which("adb") or "/opt/homebrew/bin/adb"


def adb(args, timeout=60):
    return subprocess.run([ADB, "-s", SERIAL] + args, capture_output=True, text=True,
                          timeout=timeout, errors="replace")


def su(cmd, timeout=60):
    return adb(["shell", "su -c " + json.dumps(cmd)], timeout=timeout)


def getenforce():
    r = adb(["shell", "getenforce"])
    return (r.stdout or "").strip()


def setenforce(v):
    r = su("setenforce %d; getenforce" % v)
    return (r.stdout or "").strip()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--wait", type=float, default=30.0)
    ap.add_argument("--out", default=DEF_OUT)
    ap.add_argument("--pkg", default=PKG)
    ap.add_argument("--no-setenforce", action="store_true", help="不动 SELinux（若原本 permissive 亦可）")
    ap.add_argument("--attach-pid", type=int, default=0, help="改 attach 指定 pid（不 spawn）")
    args = ap.parse_args()

    os.makedirs(args.out, exist_ok=True)
    log_path = os.path.join(args.out, "probe_v2.log")
    raw_path = os.path.join(args.out, "probe_v2.jsonl")
    js_path = os.path.join(HERE, "probe_sig_string_v2_dwdlm.js")
    src = open(js_path, "r", encoding="utf-8").read()

    t_start = time.time()
    sel_orig = None
    pid = None
    session = None
    script = None

    with open(log_path, "w", encoding="utf-8") as fh, open(raw_path, "w", encoding="utf-8") as raw:
        def log(s):
            line = "[%7.2fs] %s" % (time.time() - t_start, s)
            print(line, flush=True)
            fh.write(line + "\n")
            fh.flush()

        st = {"msgs": 0, "errs": 0, "detached": None, "sig_enter": 0, "sig_leave": 0,
              "call": 0, "strobj": 0, "attach_ok": 0, "attach_fail": 0, "bss": 0,
              "hex64_hits": 0, "der_hits": 0, "key_nonzero": 0, "hex_strings": [], "ret_lens": []}

        log("# dwdlm 签名串探针 v2  %s" % time.strftime("%Y-%m-%d %H:%M:%S"))
        log("# pkg=%s so=libsecret.so js=%s" % (args.pkg, os.path.basename(js_path)))

        try:
            sel_orig = getenforce()
            log("# SELinux 原状态 = %s" % sel_orig)
            if sel_orig != "Permissive" and not args.no_setenforce:
                got = setenforce(0)
                log("# setenforce 0 -> %s（收尾会恢复）" % got)

            dev = frida.get_usb_device(timeout=15)
            log("# device=%s %s  frida=%s" % (dev.id, dev.name, frida.__version__))

            if args.attach_pid:
                pid = args.attach_pid
                log("# 改为 attach 已有 pid=%d" % pid)
            else:
                pid = dev.spawn([args.pkg])
                log("# spawn pid=%d（挂起，未 resume）" % pid)

            try:
                session = dev.attach(pid)
                log("# attach OK pid=%d" % pid)
            except Exception as e:
                log("[FAIL] attach 失败: %s: %s" % (type(e).__name__, e))
                log("# 未 resume，游戏未真正启动；保留现场")
                return 3

            def on_detached(reason, *a):
                st["detached"] = str(reason)
                log("[detached] %s" % reason)

            session.on("detached", on_detached)
            script = session.create_script(src, name="dwdlm_sig_probe_v2")

            def on_message(message, data):
                st["msgs"] += 1
                raw.write(json.dumps({"ts": round(time.time() - t_start, 3), "m": message},
                                     ensure_ascii=False) + "\n")
                raw.flush()
                mt = message.get("type")
                if mt == "error":
                    st["errs"] += 1
                    log("[FRIDA ERROR] %s" % (message.get("stack") or message.get("description")))
                    return
                p = message.get("payload") or {}
                if not isinstance(p, dict):
                    log("[raw] %r" % (p,))
                    return
                t = p.get("t")
                if t == "base":
                    log("[base] %s base=%s src=%s size=0x%x path=%s"
                        % (p.get("tag"), p.get("base"), p.get("src"), p.get("size") or 0, p.get("path")))
                elif t == "attach_ok":
                    st["attach_ok"] += 1
                    log("[attach_ok] %s %s addr=%s (base=%s)" % (p.get("what"), p.get("tag"), p.get("addr"), p.get("base")))
                elif t == "attach_fail":
                    st["attach_fail"] += 1
                    log("[attach_fail] %s %s addr=%s msg=%s" % (p.get("what"), p.get("tag"), p.get("addr"), p.get("msg")))
                elif t == "sig_enter":
                    st["sig_enter"] += 1
                    log("[sig_enter] %s #%s at=%sms sret=%s x0=%s x1=%s x2=%s x3=0x%x lr=%s sp=%s"
                        % (p.get("tag"), p.get("n"), p.get("at_ms"), p.get("sret"), p.get("x0"),
                           p.get("x1"), p.get("x2"), p.get("x3_u32") or 0, p.get("lr"), p.get("sp")))
                elif t == "sig_leave":
                    st["sig_leave"] += 1
                    log("[sig_leave] %s #%s at=%sms x0=%s" % (p.get("tag"), p.get("n"), p.get("at_ms"), p.get("x0")))
                elif t == "sig_arg":
                    log("[sig_arg] %s #%s %s hex=%s ascii=%s" % (p.get("tag"), p.get("n"), p.get("reg"), p.get("hex"), p.get("ascii")))
                elif t == "call_enter":
                    st["call"] += 1
                    log("[call_enter] %s #%s at=%sms obj=%s x23=%s" % (p.get("tag"), p.get("n"), p.get("at_ms"), p.get("obj"), p.get("x23")))
                elif t == "strobj":
                    st["strobj"] += 1
                    cls = p.get("cls") or {}
                    ln = p.get("len")
                    if isinstance(ln, int):
                        st["ret_lens"].append(ln)
                    if cls.get("is_hex64"):
                        st["hex64_hits"] += 1
                        if p.get("data_ascii") and p["data_ascii"] not in st["hex_strings"]:
                            st["hex_strings"].append(p["data_ascii"])
                    if cls.get("der_3082"):
                        st["der_hits"] += 1
                    log("[strobj] %s/%s #%s addr=%s len=%s cap=%s cap_bit0=%s ptr=%s"
                        % (p.get("tag"), p.get("kind"), p.get("n"), p.get("addr"), p.get("len"), p.get("cap"), p.get("cap_bit0"), p.get("ptr")))
                    log("   raw48 = %s" % p.get("raw48_hex"))
                    log("   inline_ascii = %r" % p.get("inline_ascii"))
                    if p.get("data_hex"):
                        log("   data_hex  = %s" % p.get("data_hex"))
                        log("   data_ascii= %r" % p.get("data_ascii"))
                        log("   cls = %s" % json.dumps(cls, ensure_ascii=False))
                    if p.get("sso_ascii"):
                        log("   sso_ascii = %r" % p.get("sso_ascii"))
                    if p.get("data_err") or p.get("err"):
                        log("   [ERR] %s %s" % (p.get("data_err"), p.get("err")))
                elif t == "bss":
                    st["bss"] += 1
                    if p.get("key_hex") and set(p["key_hex"]) != {"0"}:
                        st["key_nonzero"] += 1
                    log("[bss] %s at=%sms flag=%s p3=%s key=%s iv=%s hex_str=%r"
                        % (p.get("tag"), p.get("at_ms"), p.get("flag_u32"), p.get("p3_u32"),
                           p.get("key_hex"), p.get("iv_hex"), p.get("hex_str")))
                elif t == "note":
                    log("[note] %s" % p.get("msg"))
                elif t == "error":
                    st["errs"] += 1
                    log("[JS ERROR] %s: %s" % (p.get("where"), p.get("msg")))
                else:
                    log("[%s] %s" % (t, json.dumps(p, ensure_ascii=False)[:300]))

            script.on("message", on_message)
            try:
                script.load()
                log("# script loaded（resume 前已注入）")
            except Exception as e:
                log("[FAIL] script.load 失败: %s: %s" % (type(e).__name__, e))
                log("# 未 resume，游戏未真正启动")
                return 4

            try:
                tab = script.exports_sync.addrtab()
                log("# 地址表: %s" % json.dumps(tab, ensure_ascii=False))
            except Exception as e:
                log("[WARN] addrtab 取不到: %s" % e)

            if not args.attach_pid:
                dev.resume(pid)
                log("# resume 完成，开始监听 %.0fs" % args.wait)

            last_snap = 0.0
            while time.time() - t_start < args.wait:
                if st["detached"]:
                    log("# 进程已结束(%s)，提前收尾，已收数据全部保留" % st["detached"])
                    break
                el = time.time() - t_start
                if el - last_snap >= 5.0:
                    last_snap = el
                    try:
                        s = script.exports_sync.snap()
                        log("[snap@%.0fs] %s" % (el, json.dumps(s, ensure_ascii=False)))
                    except Exception as e:
                        log("[snap@%.0fs] 取不到(进程可能已崩): %s" % (el, e))
                time.sleep(0.1)

            try:
                log("[final] %s" % json.dumps(script.exports_sync.stats(), ensure_ascii=False))
            except Exception as e:
                log("[final] stats 取不到: %s" % e)

            log("# 汇总: msgs=%d errs=%d detached=%s attach_ok=%d attach_fail=%d"
                % (st["msgs"], st["errs"], st["detached"], st["attach_ok"], st["attach_fail"]))
            log("# 汇总: sig_enter=%d sig_leave=%d call=%d strobj=%d bss_samples=%d"
                % (st["sig_enter"], st["sig_leave"], st["call"], st["strobj"], st["bss"]))
            log("# 汇总: hex64命中=%d DER(3082)命中=%d KEY非零样本=%d"
                % (st["hex64_hits"], st["der_hits"], st["key_nonzero"]))
            if st["ret_lens"]:
                log("# 返回长度分布: %s" % (sorted(set(st["ret_lens"]))[:20],))
            if st["hex_strings"]:
                log("# 捕获到的 64 字符 hex 串:")
                for h in st["hex_strings"][:10]:
                    log("   %s" % h)
            return 0
        finally:
            try:
                if script is not None:
                    pass
            except Exception:
                pass
            if pid is not None and not args.attach_pid:
                try:
                    dev.kill(pid)
                    log("# 已 kill 探针进程 pid=%d" % pid)
                except Exception as e:
                    log("# kill 跳过: %s" % e)
            if sel_orig and sel_orig != "Permissive" and not args.no_setenforce:
                try:
                    got = setenforce(1)
                    log("# SELinux 已恢复 -> %s" % got)
                except Exception as e:
                    log("# [!] SELinux 恢复失败，请手工 setenforce 1 : %s" % e)


if __name__ == "__main__":
    sys.exit(main())
