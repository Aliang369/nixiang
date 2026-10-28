import io, re
lines = io.open(r"E:\_build\lvmeng\ft4.log", encoding="utf-8", errors="replace").read().split("\n")
# 最后 exit_group 的 tid
ex = [l for l in lines if re.search(r"nr=231 ", l)]
print("exit tids:", [re.search(r"tid=(\d+)", l).group(1) for l in ex])
tid = re.search(r"tid=(\d+)", ex[-1]).group(1)
print("=== last 30 events of tid", tid, "===")
sel = [(i,l) for i,l in enumerate(lines) if ("tid="+tid+" ") in l or (l.strip().startswith("path[") )]
# 打印该 tid 的最后 30 条及其后的 path 行
idx = [i for i,l in enumerate(lines) if ("tid="+tid+" ") in l]
for i in idx[-30:]:
    print(lines[i])
    if i+1 < len(lines) and lines[i+1].strip().startswith(("path[", "==>")): print(lines[i+1])
print("=== RESULT lines count/last 12 ===")
res=[l for l in lines if "RESULT" in l]
print(len(res))
for l in res[-12:]: print(l)
