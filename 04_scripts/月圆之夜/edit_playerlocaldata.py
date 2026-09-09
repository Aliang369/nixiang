#!/usr/bin/env python3.11
# 编辑月圆之夜 Prefs_PlayerLocalData: Gold→99999, DLC→[1,2,3]
# 基于 .bak 生成修改版, 供推回设备 (xml + bak 双写)
import re, urllib.parse

SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/prefs_20260907/playerprefs.xml.bak"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/prefs_20260907/playerprefs.MOD.xml"

d = open(SRC, encoding="utf-8").read()
m = re.search(r'(<string name="Prefs_PlayerLocalData">)(.*?)(</string>)', d, re.S)
assert m, "键未找到"
j = urllib.parse.unquote(m.group(2))
assert '"Gold":0' in j and '"DLC":[]' in j, f"字段与预期不符: Gold/DLC"
j2 = j.replace('"DLC":[]', '"DLC":[1,2,3]').replace('"Gold":0', '"Gold":99999')
enc = urllib.parse.quote(j2, safe="")
d2 = d[:m.start(2)] + enc + d[m.end(2):]
open(OUT, "w", encoding="utf-8").write(d2)
print(f"原文 JSON 长度 {len(j)}, 改后 {len(j2)}")
print("关键 diff:")
print("  DLC:", '"DLC":[]' in j, "→", '"DLC":[1,2,3]' in j2)
print("  Gold:", '"Gold":0' in j, "→", '"Gold":99999' in j2)
print(f"→ {OUT} ({len(d2)}B)")
