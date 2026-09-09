#!/usr/bin/env python3.11
# 全模块内存镜像 dump: libil2cpp.so base..base+size (VA 布局) → 本地文件
# 用法: python3.11 dump_module_full.py [包名] [模块名] [输出文件]
import frida, subprocess, sys, time, os

PKG = sys.argv[1] if len(sys.argv) > 1 else "com.ztgame.yyzy"
MOD = sys.argv[2] if len(sys.argv) > 2 else "libil2cpp.so"
OUT = sys.argv[3] if len(sys.argv) > 3 else "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"

JS = r'''
'use strict';
const mod = Process.getModuleByName(MODNAME);
const CH = 4 * 1024 * 1024;
send({type:'info', msg:'base='+mod.base+' size=0x'+mod.size.toString(16)});
let off = 0;
function step() {
    if (off >= mod.size) { send({type:'done'}); return; }
    const len = Math.min(CH, mod.size - off);
    try {
        const buf = mod.base.add(off).readByteArray(len);
        send({type:'blk', off: off, len: len}, buf);
    } catch (e) {
        // 不可读块(bss 空洞) → 发零块
        send({type:'zero', off: off, len: len});
    }
    off += len;
    setTimeout(step, 0);
}
step();
'''

def sh(cmd):
    return subprocess.run(["adb","shell",cmd], capture_output=True, text=True, timeout=30).stdout.strip()

sh("su -c 'setenforce 0'")
pid = sh(f"pidof {PKG}")
if not pid:
    print("游戏未运行, 启动中...", flush=True)
    sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
    for _ in range(40):
        time.sleep(1)
        pid = sh(f"pidof {PKG}")
        if pid: break
    print("等待 il2cpp 初始化 25s...", flush=True)
    time.sleep(25)
pid = int(pid.split()[0])
print(f"pid={pid}", flush=True)
session = frida.get_usb_device(timeout=5).attach(pid)
script = session.create_script(JS.replace("MODNAME", f"'{MOD}'"))

f = open(OUT, "wb")
total = [0]
t0 = time.time()
def on_msg(m, d):
    if m.get("type") != "send":
        print("[err]", str(m)[:200], flush=True); return
    p = m["payload"]
    if p["type"] == "info":
        print(p["msg"], flush=True)
    elif p["type"] == "blk":
        f.seek(p["off"]); f.write(d); total[0] += p["len"]
        if (p["off"] // (16*1024*1024)) != ((p["off"]+p["len"]) // (16*1024*1024)):
            print(f"  {total[0]/1048576:.0f}MB...", flush=True)
    elif p["type"] == "zero":
        f.seek(p["off"]); f.write(b"\x00" * p["len"]); total[0] += p["len"]
    elif p["type"] == "done":
        print(f"done {total[0]} bytes in {time.time()-t0:.0f}s", flush=True)
script.on("message", on_msg)
script.load()
time.sleep(120)
f.close()
try: session.detach()
except: pass
sh("su -c 'setenforce 1'")
print("getenforce =", sh("su -c 'getenforce'"), flush=True)
