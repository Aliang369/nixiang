#!/usr/bin/env python3.11
# H路线 v5: 拉起游戏 → attach → 等用户联网登录 → yyzy_hotfix 出现自动 BSJB 整像倾倒
import frida, subprocess, sys, time, os, hashlib

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

idx = [0]
md5s = []

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:300], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "dump":
        fn = f"{OUT}/hotfix_online_{idx[0]:02d}_{p['size']}.bin"
        idx[0] += 1
        m = hashlib.md5(data).hexdigest()
        md5s.append((fn, p["size"], m, p.get("ctx", 0)))
        open(fn, "wb").write(data)
        print(f"[DUMP] @{p['at']} size={p['size']} ctx={p.get('ctx',0)} md5={m[:12]} → {os.path.basename(fn)}", flush=True)
        return
    if t == "tick":
        return
    if t == "done":
        print(f"== DONE imgs={p.get('imgs')}, 共 {idx[0]} 个文件 ==", flush=True)
        for fn, sz, m, ctx in md5s:
            print(f"   ctx={ctx} size={sz} md5={m} {os.path.basename(fn)}", flush=True)
        return
    print(str(p)[:200], flush=True)

def main():
    win = int(sys.argv[1]) if len(sys.argv) > 1 else 600
    mode = sys.argv[2] if len(sys.argv) > 2 else "restart"  # attach=不重启直接附加
    sh("su -c 'setenforce 0'")
    session = None
    if mode == "attach":
        pids = sh(f"pidof {PKG}")
        pid = int(pids.split()[0])
        print(f"attach 已运行 pid={pid}", flush=True)
        try:
            session = frida.get_usb_device(timeout=5).attach(pid)
        except Exception as e:
            print("  attach 失败:", e, flush=True)
    else:
        for attempt in range(3):
            sh(f"am force-stop {PKG}")
            time.sleep(1.5)
            sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
            time.sleep(6)
            pid = sh(f"pidof {PKG}")
            if not pid:
                print("游戏未启动, 重试", flush=True)
                continue
            pid = int(pid.split()[0])
            print(f"attach 尝试 {attempt+1}, pid={pid}", flush=True)
            try:
                session = frida.get_usb_device(timeout=5).attach(pid)
                break
            except Exception as e:
                print("  失败:", e, flush=True)
    if session is None:
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    script = session.create_script(open(os.path.join(HERE, "dump_online_v5.js")).read())
    script.on("message", on_message)
    script.load()
    print(f"== 轮询窗口 {win}s: 请在设备上联网登录游戏并进入主界面 ==", flush=True)
    time.sleep(win)
    try: session.detach()
    except Exception: pass
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"), "(游戏保持运行, 未 force-stop)", flush=True)

if __name__ == "__main__":
    main()
