#!/usr/bin/env python3.11
# 定位热更元数据堆并倾倒
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

hits_all = []

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    if p.get("type") in ("hits_yyzy", "hits_us"):
        print(f"[{p['type']}] {len(p['hits'])} 处", flush=True)
        for h in p["hits"][:10]:
            print("   ", h, flush=True)
            hits_all.append(h)
        return
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
    script = session.create_script(open(os.path.join(HERE, "find_hotfix_meta.js")).read())
    script.on("message", on_message)
    script.load()
    print("== 30 秒后二次扫描(给热更加载时间), 共 90 秒 ==", flush=True)
    time.sleep(30)
    script.load() if False else None
    # 二次扫描: 重新 eval 一遍(简单做法: 再 load 一个实例)
    try:
        s2 = session.create_script(open(os.path.join(HERE, "find_hotfix_meta.js")).read())
        s2.on("message", on_message)
        s2.load()
    except Exception as e:
        print("二次扫描失败:", e, flush=True)
    time.sleep(60)
    try: session.detach()
    except Exception: pass

if __name__ == "__main__":
    main()
