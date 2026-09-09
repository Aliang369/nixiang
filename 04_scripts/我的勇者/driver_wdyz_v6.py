#!/usr/bin/env python3.11
# 我的勇者 v6 python 驱动 — attach + 命令文件轮询（/tmp/wdyz_cmds.txt 每行一条命令）
import frida
import sys
import time

JS = "/Users/aliang/Documents/nixiang/04_scripts/我的勇者/verify_wdyz_v6_attach.js"
CMD_FILE = "/tmp/wdyz_cmds.txt"

def on_message(msg, data):
    if msg.get("type") == "send":
        print("[SEND]", msg.get("payload"), flush=True)
    elif msg.get("type") == "log":
        print("[LOG]", msg.get("payload"), flush=True)
    elif msg.get("type") == "error":
        print("[ERR]", msg.get("description"), (msg.get("stack") or "")[:400], flush=True)

def main():
    pid = int(sys.argv[1])
    dev = frida.get_usb_device(timeout=10)
    session = dev.attach(pid)
    print("[*] attached", pid, flush=True)
    with open(JS) as fp:
        src = fp.read()
    script = session.create_script(src)
    script.on("message", on_message)
    script.load()
    print("[*] script loaded", flush=True)
    exp = script.exports_sync
    pos = 0
    while True:
        time.sleep(0.4)
        try:
            with open(CMD_FILE) as fp:
                fp.seek(pos)
                chunk = fp.read()
                pos = fp.tell()
        except FileNotFoundError:
            open(CMD_FILE, "w").close()
            continue
        except Exception:
            continue
        for ln in chunk.splitlines():
            ln = ln.strip()
            if not ln:
                continue
            try:
                if ln == "STAT":
                    print("[STAT]", exp.stat(), flush=True)
                else:
                    r = exp.cmd(ln)
                    print("[CMD]", ln, "->", r, flush=True)
            except Exception as e:
                print("[CMDERR]", ln, repr(e), flush=True)

main()
