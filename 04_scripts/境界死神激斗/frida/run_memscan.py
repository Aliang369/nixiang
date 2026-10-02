#!/usr/bin/env python3.11
import frida, sys, os, json, time

PKG = "com.cmge.ssjd.x7sy"
JS = sys.argv[1] if len(sys.argv) > 1 else "/tmp/ssjd2/fs_memscan.js"
DUR = int(sys.argv[2]) if len(sys.argv) > 2 else 120
SPAWN = os.environ.get("SPAWN") == "1"

def main():
    dev = frida.get_usb_device(timeout=5)
    if SPAWN:
        pid = dev.spawn([PKG])
        print("spawn pid", pid)
    else:
        pid = int(os.popen("adb -s f9a116a2 shell pidof " + PKG).read().split()[0])
        print("attach pid", pid)
    print("access", dev.query_system_parameters().get("access"))
    sess = dev.attach(pid)
    with open(JS) as f:
        script = sess.create_script(f.read())
    def on_msg(m, data):
        if m["type"] == "send":
            p = m["payload"]
            if p.get("t") == "log":
                print("[js]", p["m"], flush=True)
            elif p.get("t") == "scan":
                print("[scan] %s total=%s" % (p["pat"], p["total"]), flush=True)
                for h in p["hits"]:
                    print("   base=%s size=%s prot=%s file=%s n=%s" % (h["base"], h["size"], h["prot"], h["file"], h["n"]), flush=True)
            elif p.get("t") == "dll" and data:
                d = "/tmp/ssjd2/dll"
                os.makedirs(d, exist_ok=True)
                fn = "%s/cand%d_%s%s.bin" % (d, p["idx"], p["base"], "" if p.get("net") else "_notnet")
                with open(fn, "wb") as fh:
                    fh.write(data)
                print("[dll] wrote %s %d bytes net=%s" % (fn, len(data), p.get("net")), flush=True)
            else:
                print("[pay]", json.dumps(p)[:200000], flush=True)
        else:
            print("[err]", m, flush=True)
    script.on("message", on_msg)
    script.load()
    if SPAWN:
        dev.resume(pid)
    t0 = time.time()
    while time.time() - t0 < DUR:
        time.sleep(3)
    try:
        sess.detach()
    except Exception:
        pass

main()
