#!/usr/bin/env python3.11
# 写时抓明文: 重启游戏→attach→hook write→用户离线登录触发 UIA 写入
import os, sys, time, signal, subprocess, frida

HERE = os.path.dirname(os.path.abspath(__file__))
PKG = "com.ztgame.yyzy"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/savewatch"
os.makedirs(OUT, exist_ok=True)

def sh(cmd):
    return subprocess.run(cmd, shell=True, capture_output=True, text=True).stdout.strip()

cipher_n = 0
hit_n = 0

def on_message(msg, data):
    global cipher_n, hit_n
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "log":
        print(f"[game] {p.get('msg','')[:160]}", flush=True)
    elif t == "cipher":
        cipher_n += 1
        b = os.path.basename(p["path"]).replace("/", "_")
        fn = f"{OUT}/wt_{b}_{cipher_n:03d}.bin"
        with open(fn, "wb") as f:
            f.write(data)
        print(f"[CIPHER] {b} #{p['seq']} len={p['len']} → {fn}", flush=True)
    elif t == "hit":
        hit_n += 1
        fn = f"{OUT}/PT_{p['label']}_{hit_n:03d}.bin"
        with open(fn, "wb") as f:
            f.write(data)
        print(f"[PT-HIT] {p['label']} cand@{p['addr']} ct@{p['ct']} len={p['len']} → {fn}", flush=True)

def cleanup(sig, frm):
    print("\n== 退出: 恢复 SELinux ==", flush=True)
    try: session.detach()
    except Exception: pass
    print("setenforce 1 →", sh("adb shell su -c 'setenforce 1'"), flush=True)
    sys.exit(0)

signal.signal(signal.SIGTERM, cleanup)
signal.signal(signal.SIGINT, cleanup)

print("== setenforce 0 ==", flush=True)
sh("adb shell su -c 'setenforce 0'")
print("== 重启游戏 ==", flush=True)
sh(f"adb shell am force-stop {PKG}")
time.sleep(2)
sh(f"adb shell monkey -p {PKG} -c android.intent.category.LAUNCHER 1")

session = None
for attempt in range(6):
    time.sleep(6)
    out = sh(f"adb shell pidof {PKG}")
    if not out: continue
    pid = int(out.split()[0])
    try:
        session = frida.get_usb_device(timeout=5).attach(pid)
        print(f"  attach 成功 pid={pid}", flush=True)
        break
    except Exception as e:
        print(f"  [{attempt+1}] attach 失败: {str(e)[:100]}", flush=True)

if session is None:
    print("attach 全失败", flush=True)
    sh("adb shell su -c 'setenforce 1'")
    sys.exit(1)

script = session.create_script(open(os.path.join(HERE, "write_pt_watch.js")).read())
script.on("message", on_message)
script.load()
print("== 就绪: 请在真机上完成离线登录 (UIA 写入时自动抓) ==", flush=True)

try:
    while True:
        time.sleep(30)
        print(f".. 心跳 cipher={cipher_n} hit={hit_n}", flush=True)
except KeyboardInterrupt:
    cleanup(None, None)
