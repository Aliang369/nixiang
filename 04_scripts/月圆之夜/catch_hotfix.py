#!/usr/bin/env python3.11
# 等登录 → 自动检测热更元数据堆 → 倾倒
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

idx = [0]

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "dump":
        fn = f"{OUT}/hotfix_meta_{idx[0]}.bin"
        idx[0] += 1
        open(fn, "wb").write(data)
        print(f"[DUMP] @{p['at']} → {fn} ({len(data)} B)", flush=True)
        return
    if t == "tick":
        return  # 静默轮询
    print(str(p)[:200], flush=True)

def main():
    sh("su -c 'setenforce 0'")
    session = None
    for attempt in range(3):
        sh(f"am force-stop {PKG}")
        time.sleep(1.5)
        sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
        time.sleep(6)
        pid = int(sh(f"pidof {PKG}").split()[0])
        print(f"attach 尝试 {attempt+1}, pid={pid}")
        try:
            session = frida.get_usb_device(timeout=5).attach(pid)
            break
        except Exception as e:
            print("  失败:", e)
            session = None
    if session is None:
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    script = session.create_script(open(os.path.join(HERE, "catch_hotfix.js")).read())
    script.on("message", on_message)
    script.load()
    print("== 轮询 7.5 分钟: 请登录游戏并到主界面 ==", flush=True)
    time.sleep(460)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
