#!/usr/bin/env python3.11
# GOT 审计驱动: 启动游戏(无需登录) → attach → 扫描指向 cybrex 的 GOT 条目
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

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
    script = session.create_script(open(os.path.join(HERE, "got_audit.js")).read())
    script.on("message", lambda m, d: print(
        ("[script-error] " + str(m.get("description"))[:200]) if m.get("type") == "error"
        else str(m.get("payload"))[:250], flush=True) if m.get("type") != "send" else print(str(m.get("payload"))[:250], flush=True))
    script.load()
    time.sleep(10)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
