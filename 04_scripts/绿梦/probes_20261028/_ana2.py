import io, re
lines = io.open(r"E:\_build\lvmeng\ft3.log", encoding="utf-8", errors="replace").read().split("\n")
ent = [i for i,l in enumerate(lines) if re.match(r"^\[\d+\] tid=", l)]
print("entry lines:", len(ent))
print("=== last 30 lines before the final [gone] block ===")
last_gone = max((i for i,l in enumerate(lines) if l.startswith("[gone]")), default=len(lines)-1)
for l in lines[max(0,last_gone-32):last_gone]:
    print(l)
print("=== all RESULT lines (max 25) ===")
res = [l for l in lines if "RESULT" in l]
print("count", len(res))
for l in res[-25:]:
    print(l)
