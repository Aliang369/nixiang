
import frida, sys, time
PID = int(sys.argv[1]) if len(sys.argv) > 1 else 0
dev = frida.get_usb_device(timeout=10)
if not PID:
    for p in dev.enumerate_processes():
        if "abyss" in p.name:
            PID = p.pid
print("target pid", PID)
s = dev.attach(PID)
print("attached")
script = s.create_script("""
console.log("agent loaded arch=" + Process.arch + " ptrsize=" + Process.pointerSize);
var m = Process.findModuleByName("libil2cpp.so");
console.log("libil2cpp base=" + (m ? m.base : "null") + " size=" + (m ? m.size : 0));
var mods = Process.enumerateModules().length;
console.log("modules=" + mods);
send({ok: true, arch: Process.arch, base: m ? m.base.toString() : null, size: m ? m.size : 0});
""")
msgs = []
script.on("message", lambda m, d: msgs.append(m))
script.load()
time.sleep(2)
print("messages:", msgs[:3])
time.sleep(5)
s.detach()
print("detached OK")
