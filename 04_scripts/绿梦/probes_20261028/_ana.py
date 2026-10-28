import io, re
lines = io.open(r"E:\_build\lvmeng\ft3.log", encoding="utf-8", errors="replace").read().split("\n")
# 找最后一批非 [gone] 行
idx = [i for i,l in enumerate(lines) if l.startswith("[") ]
print("total", len(lines), "event lines", len(idx))
# 打印最后一个 [NN] tid= 之前的 40 行
tail_start = idx[-1] if idx else 0
for l in lines[max(0, tail_start-45):tail_start+2]:
    print(l)
