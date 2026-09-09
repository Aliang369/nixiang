#!/usr/bin/env python3.11
# v6 驱动: 快速 attach(0.5s 重试) → 装钩子 → 用户操作 → 收集
import frida, subprocess, sys, time, os, hashlib

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

idx = [0]

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:300], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "dump":
        fn = f"{OUT}/v6_{p.get('tag','x')}_{idx[0]:02d}_{p['size']}.bin"
        idx[0] += 1
        open(fn, "wb").write(data)
        print(f"[DUMP] {os.path.basename(fn)} @{p['at']} md5={hashlib.md5(data).hexdigest()[:10]}", flush=True)
        return
    if t in ("err", "poll"):
        if t == "poll" and p.get("new") == 0:
            return
    print(str(p)[:220], flush=True)

def main():
    win = int(sys.argv[1]) if len(sys.argv) > 1 else 600
    sh("su -c 'setenforce 0'")
    session = None
    sh(f"am force-stop {PKG}")
    time.sleep(1)
    sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
    time.sleep(6)
    for attempt in range(40):
        pid = sh(f"pidof {PKG}")
        if not pid:
            time.sleep(1)
            continue
        pid = int(pid.split()[0])
        print(f"attach 尝试 {attempt+1} pid={pid}", flush=True)
        try:
            session = frida.get_usb_device(timeout=3).attach(pid)
            print(f"attach ok", flush=True)
            break
        except Exception as e:
            print(f"  fail: {str(e)[:100]}", flush=True)
            time.sleep(1)
            continue
    if session is None:
        print("attach 失败", flush=True)
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    script = session.create_script(open(os.path.join(HERE, "catch_load_v6.js")).read())
    script.on("message", on_message)
    script.load()
    print(f"== 监控 {win}s: 请登录并游玩(建议也试 PVP/联网模式) ==", flush=True)
    time.sleep(win)
    try: session.detach()
    except Exception: pass
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"), flush=True)

if __name__ == "__main__":
    main()
