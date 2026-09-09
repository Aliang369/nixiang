#!/usr/bin/env python3.11
# 驱动: attach 现有进程 → dump il2cpp 头 8MB + cybrex RW + 匿名区字符串
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"
os.makedirs(OUT, exist_ok=True)

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

log = []

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "il2head":
        with open(f"{OUT}/il2cpp_head.bin", "r+b" if os.path.exists(f"{OUT}/il2cpp_head.bin") else "wb") as f:
            f.seek(p["off"]); f.write(data)
        return
    if t == "cbrw":
        with open(f"{OUT}/cybrex_rw.bin", "wb") as f:
            f.write(data)
        return
    log.append(p)
    print(f"[{t}] " + str({k: v for k, v in p.items() if k not in ('type',)})[:400], flush=True)

def main():
    sh("su -c 'setenforce 0'")
    sh(f"am force-stop {PKG}")
    time.sleep(1)
    sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
    time.sleep(4)
    pid = int(sh(f"pidof {PKG}").split()[0])
    print(f"attach pid={pid}")
    try:
        session = frida.get_usb_device(timeout=5).attach(pid)
    except Exception as e:
        print("attach 失败:", e)
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "dump_il2cpp_head.js")) as f:
        src = f.read()
    script = session.create_script(src)
    script.on("message", on_message)
    script.load()
    time.sleep(45)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))
    print("== done ==")

if __name__ == "__main__":
    main()
