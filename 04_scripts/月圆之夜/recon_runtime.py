#!/usr/bin/env python3.11
# 月圆之夜 MHP 被动侦察驱动：启动游戏 → attach → 采集 → 恢复现场
# 用法: /tmp/fridaenv/bin/python3.11 recon_runtime.py
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"
os.makedirs(OUT, exist_ok=True)
cx_path = os.path.join(OUT, "cybrex_mem.bin")
arr_path = os.path.join(OUT, "hotfix_arr_0.bin")

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

log = []
cx_written = set()

def on_message(msg, data):
    if msg.get("type") == "send":
        p = msg["payload"]
        t = p.get("type")
        if t == "cxdump":
            with open(cx_path, "r+b" if os.path.exists(cx_path) else "wb") as f:
                f.seek(p["off"])
                f.write(data)
            cx_written.add(p["off"])
            return
        if t == "arrdump":
            with open(arr_path, "wb") as f:
                f.write(data)
            return
        log.append(p)
        line = f"[{t}] " + {k: v for k, v in p.items() if k != 'type'}.__str__()
        if len(line) > 400: line = line[:400] + "…"
        print(line, flush=True)
    elif msg.get("type") == "error":
        print("[script-error]", msg.get("description", "")[:300], flush=True)

def main():
    print("== 恢复 Enforcing ==>", sh("su -c 'setenforce 0'"))
    sh(f"am force-stop {PKG}")
    time.sleep(1)
    print("== 启动游戏 ==", sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")[:80])

    session = None
    deadline = time.time() + 120
    last_err = None
    while time.time() < deadline and session is None:
        try:
            pid = int(sh(f"pidof {PKG}").split()[0])
            session = frida.get_usb_device(timeout=2).attach(pid)
        except Exception as e:
            last_err = f"{type(e).__name__}: {str(e)[:160]}"
            time.sleep(0.8)
    if session is None:
        print(f"attach 失败, last_err={last_err}"); sys.exit(1)
    print("== attached ==")

    with open(os.path.join(HERE, "recon_runtime.js")) as f:
        src = f.read()
    script = session.create_script(src)
    script.on("message", on_message)
    script.load()

    # 采集窗口：thunk 触发（热更加载）+ 两次 late pass（20s/40s）+ cybrex dump
    time.sleep(75)
    try:
        session.detach()
    except Exception:
        pass

    print("\n== 汇总 ==")
    slots = [p for p in log if p.get("type") in ("slot", "slot2")]
    for p in slots:
        print("槽位:", p)
    opens = [p for p in log if p.get("type") == "open"]
    print(f"open 命中: {len(opens)}")
    for p in opens[:12]:
        print(" ", p["fn"], p["path"])
        print("   bt:", " <- ".join(p["bt"][:5]))
    th = [p for p in log if p.get("type") == "thunk_arr"]
    print("thunk 入参:", th)
    cx_bytes = sum(1 for _ in cx_written)
    print(f"cybrex dump 块数: {cx_bytes} → {cx_path}")
    print("== 现场恢复 ==")
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
