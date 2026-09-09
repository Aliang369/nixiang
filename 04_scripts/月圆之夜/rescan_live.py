#!/usr/bin/env python3.11
# 对活进程重扫 BSJB (不重启游戏)
import frida, subprocess, sys, time, os, hashlib

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

n = [0]
seen = set()

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    if p.get("type") == "pe":
        h = hashlib.md5(data).hexdigest()
        if h in seen: return
        seen.add(h)
        fn = f"{OUT}/live_pe_{n[0]}.bin"
        n[0] += 1
        open(fn, "wb").write(data)
        print(f"[PE] fsize={p['fsize']} → {fn} ({len(data)} B)", flush=True)
        return
    print(f"[{p.get('type')}] " + str({k: v for k, v in p.items()})[:200], flush=True)

def main():
    sh("su -c 'setenforce 0'")
    pid_s = sh(f"pidof {PKG}")
    if pid_s:
        pid = int(pid_s.split()[0])
        print(f"游戏已在运行, 直连 pid={pid}")
        session = frida.get_usb_device(timeout=5).attach(pid)
    else:
        session = None
        for attempt in range(3):
            sh(f"am force-stop {PKG}")
            time.sleep(1.5)
            sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
            time.sleep(5)
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
    script = session.create_script(open(os.path.join(HERE, "rescan_live.js")).read())
    script.on("message", on_message)
    script.load()
    print("== 6 分钟扫描窗口: 请登录游戏并进入主界面/任意场景 ==", flush=True)
    time.sleep(380)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
