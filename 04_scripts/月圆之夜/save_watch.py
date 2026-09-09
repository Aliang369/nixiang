#!/usr/bin/env python3.11
# S 路线: 监控月圆之夜存档写入 + 存盘瞬间内存明文扫描
# 用法: 后台运行, 用户进游戏触发存盘; SIGTERM 退出时自动恢复 SELinux
import os, sys, time, signal, subprocess, frida

HERE = os.path.dirname(os.path.abspath(__file__))
PKG = "com.ztgame.yyzy"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/savewatch"
os.makedirs(OUT, exist_ok=True)

def sh(cmd):
    return subprocess.run(cmd, shell=True, capture_output=True, text=True).stdout.strip()

cipher_n = 0
hit_n = 0
log_n = 0

def on_message(msg, data):
    global cipher_n, hit_n, log_n
    if msg.get("type") == "log":
        print(f"[game] {msg.get('payload','')[:200]}", flush=True)
        return
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "log":
        log_n += 1
        print(f"[game] {p.get('msg','')[:200]}", flush=True)
    elif t == "cipher":
        cipher_n += 1
        b = os.path.basename(p["path"]).replace("/", "_")
        fn = f"{OUT}/{b}_{cipher_n:03d}.bin"
        with open(fn, "wb") as f:
            f.write(data)
        print(f"[CIPHER] {p['path']} len={p['len']} → {fn}", flush=True)
    elif t == "hit":
        hit_n += 1
        fn = f"{OUT}/hit_{p['pat']}_{p['enc']}_{hit_n:03d}.bin"
        with open(fn, "wb") as f:
            f.write(data)
        print(f"[HIT#{hit_n}] {p['pat']}/{p['enc']} @{p['addr']} (range {p['range']}) → {fn}", flush=True)

def cleanup(sig, frm):
    print("\n== 退出: 恢复 SELinux ==", flush=True)
    try:
        session.detach()
    except Exception:
        pass
    print("setenforce 1 →", sh("adb shell su -c 'setenforce 1'"), flush=True)
    print("getenforce =", sh("adb shell su -c 'getenforce'"), flush=True)
    sys.exit(0)

signal.signal(signal.SIGTERM, cleanup)
signal.signal(signal.SIGINT, cleanup)

print("== setenforce 0 ==", flush=True)
print(sh("adb shell su -c 'setenforce 0'"), flush=True)

print("== 重启游戏 ==", flush=True)
sh(f"adb shell am force-stop {PKG}")
time.sleep(2)
sh(f"adb shell monkey -p {PKG} -c android.intent.category.LAUNCHER 1")

session = None
for attempt in range(5):
    time.sleep(6)
    out = sh(f"adb shell pidof {PKG}")
    if not out:
        print(f"  [{attempt+1}] 游戏未起, 等待...", flush=True)
        continue
    pid = int(out.split()[0])
    try:
        session = frida.get_usb_device(timeout=5).attach(pid)
        print(f"  [{attempt+1}] attach 成功 pid={pid}", flush=True)
        break
    except Exception as e:
        print(f"  [{attempt+1}] attach 失败: {str(e)[:120]}", flush=True)
        session = None

if session is None:
    print("attach 全失败, 恢复 SELinux 退出", flush=True)
    print("setenforce 1 →", sh("adb shell su -c 'setenforce 1'"), flush=True)
    sys.exit(1)

script = session.create_script(open(os.path.join(HERE, "save_watch.js")).read())
script.on("message", on_message)
script.load()
print("== 监控就绪: 请进游戏并触发存盘 (打一局/退出对局/切换界面) ==", flush=True)
print("== 本进程将持续运行, 结束时发 SIGTERM ==", flush=True)

try:
    while True:
        time.sleep(30)
        print(f".. 心跳 cipher={cipher_n} hit={hit_n} log={log_n}", flush=True)
except KeyboardInterrupt:
    cleanup(None, None)
