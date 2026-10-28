import re
d = open(r"E:\_build\lvmeng\fg_dump.bin","rb").read()
print("size", len(d))
strs = re.findall(rb"[\x20-\x7e]{4,200}", d)
print("strs", len(strs))
kw = [b"magisk", b"frida", b"xposed", b"hook", b"ptrace", b"starry", b"classes", b"META-INF", b"base.apk", b"signatur", b"cert", b"tamper", b"debug", b"emulator", b"ro.build", b"/proc/", b"/data/", b"hook_detect", b"detect"]
seen = set()
for s in strs:
    low = s.lower()
    if any(k in low for k in kw):
        if s in seen: continue
        seen.add(s)
        print(s.decode("utf-8", "replace"))
