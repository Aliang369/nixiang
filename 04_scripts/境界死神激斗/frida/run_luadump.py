#!/usr/bin/env python3.11
# 拉取 jjssjd libxlua 模块 dump（frida spawn）
import frida, sys, os, base64, time, json

PKG = "com.cmge.ssjd.x7sy"
OUT = "/tmp/ssjd2/luadump"
DUR = int(sys.argv[1]) if len(sys.argv) > 1 else 90
SPAWN = os.environ.get("NO_SPAWN") != "1"

os.makedirs(OUT, exist_ok=True)
dev = frida.get_usb_device(timeout=5)
print("access:", dev.query_system_parameters().get("access"))

if SPAWN:
    pid = dev.spawn([PKG])
    print("spawned", pid)
else:
    pid = int(os.popen("adb -s f9a116a2 shell pidof " + PKG).read().strip())
    print("attach", pid)

sess = dev.attach(pid)
with open("/tmp/ssjd2/fs_luadump.js") as f:
    script = sess.create_script(f.read())

names = []
cnt = {"chunk": 0, "bytes": 0}
def on_msg(m, data):
    if m["type"] == "send":
        p = m["payload"]
        t = p.get("t")
        if t == "log":
            print("[js]", p["m"])
        elif t == "load":
            nm = p.get("name")
            names.append((nm, p.get("sz")))
            if len(names) <= 400:
                print("[load] %-70s sz=%d" % (str(nm)[:70], p.get("sz")))
        elif t == "chunk":
            nm = p.get("name") or "noname"
            safe = "".join(c if c.isalnum() or c in "._-@" else "_" for c in nm)[-120:]
            fp = os.path.join(OUT, "%08x_%s" % (abs(hash(nm)) & 0xffffffff, safe))
            cnt["chunk"] += 1
            if data:
                cnt["bytes"] += len(data)
                try:
                    open(fp, "wb").write(data)
                except Exception as e:
                    print("write fail", e)
        elif t == "pcall":
            print("[pcall] L=%s" % p.get("L"))
    else:
        print("[err]", m)

script.on("message", on_msg)
script.load()
if SPAWN:
    dev.resume(pid)
print("running %ds ..." % DUR)
t0 = time.time()
while time.time() - t0 < DUR:
    time.sleep(5)
try:
    sess.detach()
except Exception:
    pass
print("chunks=%d bytes=%d unique_names=%d" % (cnt["chunk"], cnt["bytes"], len(set(n for n, _ in names))))
open(os.path.join(OUT, "names.txt"), "w").write("\n".join("%s\t%s" % (n, s) for n, s in names))
