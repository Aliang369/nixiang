#!/usr/bin/env python3.11
# 驱动: thunk 抓 base.bytes 入参 + 槽位读 icall 实现地址
import frida, subprocess, sys, time, os

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

arr_n = 0
pe_n = 0
seen_md5 = set()
import hashlib

def on_message(msg, data):
    global arr_n, pe_n
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "arrdump":
        fn = f"{OUT}/icall_input_{arr_n}.bin"
        arr_n += 1
        open(fn, "wb").write(data)
        print(f"[arrdump] → {fn} ({len(data)} B)", flush=True)
        return
    if t == "pe":
        h = hashlib.md5(data).hexdigest()
        if h in seen_md5:
            return
        seen_md5.add(h)
        fn = f"{OUT}/pe_dump_{pe_n}.bin"
        pe_n += 1
        open(fn, "wb").write(data)
        print(f"[PE] mz={p['mz']} n={p.get('n')} → {fn} ({len(data)} B)", flush=True)
        return
    if t == "info" and "scanPE done, PE dumps=0" in str(p.get("msg")):
        return  # 静默空轮
    print(f"[{t}] " + str({k: v for k, v in p.items()})[:300], flush=True)

def main():
    sh("su -c 'setenforce 0'")
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
    if session is None:
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    script = session.create_script(open(os.path.join(HERE, "slotcheck.js")).read())
    script.on("message", on_message)
    script.load()
    print("== 监控中 12 分钟: 请在设备上登录游戏到主界面 ==", flush=True)
    time.sleep(720)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
