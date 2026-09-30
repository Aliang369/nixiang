#!/usr/bin/env python3
"""境界刀鸣 Lua 层运行时探针驱动（分析用，只读）
用法: run_lua_probe.py <包名> <秒数> <设备ID>
产物: 06_dumped/境界刀鸣/lua_probe/{messages.jsonl, summary.txt}
"""
import json
import os
import sys
import time

import frida

PKG = sys.argv[1] if len(sys.argv) > 1 else "com.blade.bd"
DUR = int(sys.argv[2]) if len(sys.argv) > 2 else 120
SERIAL = sys.argv[3] if len(sys.argv) > 3 else "f9a116a2"
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
OUT = os.path.join(ROOT, "06_dumped", "境界刀鸣", "lua_patch")
JS = os.path.join(ROOT, "06_dumped", "境界刀鸣", "tools", "jjdm_lua_patch.js")
os.makedirs(OUT, exist_ok=True)

msgs = []
summary = {"states": [], "modules": [], "globals": [], "hooks": [], "errors": []}


def on_message(message, data):
    msgs.append(message)
    if message.get("type") == "error":
        summary["errors"].append(message.get("stack") or message.get("description"))
        print("[js-error]", message.get("description"))
        return
    p = message.get("payload") or {}
    t = p.get("t")
    if t == "chunk":
        summary["chunks"].append(p)
        print(f"[chunk] {p.get('name')} size={p.get('size')} dump={(p.get('dump') or {}).get('path')}")
    elif t == "modules":
        summary["modules"] = p.get("list") or []
        print(f"[modules] {p.get('n')}")
    elif t == "globals":
        summary["globals"] = p.get("list") or []
        print(f"[globals] {p.get('n')}")
    elif t in ("hook", "L", "info", "miss", "ready"):
        summary["hooks"].append(p)
        print(f"[{t}] {json.dumps(p, ensure_ascii=False)[:200]}")
    elif t == "err":
        summary["errors"].append(p)
        print(f"[err] {p}")


dev = frida.get_device(SERIAL, timeout=10)
MODE = "spawn" if "spawn" in sys.argv[4:] else "attach"


def alive(pid):
    try:
        return any(p.pid == pid for p in dev.enumerate_processes())
    except Exception:
        return False


if MODE == "spawn":
    pid = dev.spawn([PKG])
    name = PKG
else:
    pid = None
    name = PKG
    if PKG.isdigit():
        pid = int(PKG)
    else:
        try:
            pid = dev.get_process(PKG).pid
        except Exception:
            for p in dev.enumerate_processes():
                if p.name == PKG or PKG in p.name or "刀鸣" in p.name:
                    pid, name = p.pid, p.name
                    break
if pid is None:
    raise SystemExit(f"process not found for {PKG!r} (try passing the PID instead)")
print(f"[driver] mode={MODE} pid={pid} name={name}")
session = dev.attach(pid)
script = session.create_script(open(JS, encoding="utf-8").read())
script.on("message", on_message)
script.load()
if MODE == "spawn":
    dev.resume(pid)
    print("[driver] resumed")
print(f"[driver] loaded, observing {DUR}s")
t0 = time.time()
while time.time() - t0 < DUR:
    time.sleep(5)
    if not alive(pid):
        print("[driver] process gone")
        break
    if summary["modules"]:
        break
try:
    script.unload()
except Exception:
    pass

with open(os.path.join(OUT, "messages.jsonl"), "w", encoding="utf-8") as f:
    for m in msgs:
        f.write(json.dumps(m, ensure_ascii=False) + "\n")
with open(os.path.join(OUT, "summary.json"), "w", encoding="utf-8") as f:
    json.dump(summary, f, ensure_ascii=False, indent=1)

print(f"[driver] chunks={len(summary['chunks'])} modules={len(summary['modules'])} "
      f"globals={len(summary['globals'])} errors={len(summary['errors'])}")
print(f"[driver] wrote {OUT}")
