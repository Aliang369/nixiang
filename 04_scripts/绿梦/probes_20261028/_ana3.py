import io, re
lines = io.open(r"E:\_build\lvmeng\ft4.log", encoding="utf-8", errors="replace").read().split("\n")
ev = [i for i,l in enumerate(lines) if re.match(r"^\[\d+\] tid=", l) or l.strip().startswith("path[") or "RESULT" in l]
print("events:", len(ev))
# 找 exit_group(231)
ex = [i for i,l in enumerate(lines) if re.search(r"nr=231 ", l)]
print("exit_group entries:", len(ex))
if ex:
    k = ex[-1]
    print("=== context around last exit_group ===")
    for l in lines[max(0,k-24):k+4]:
        print(l)
else:
    print("=== last 24 event-ish lines ===")
    for l in lines[ev[-24:] if ev else -24:]:
        print(l)
