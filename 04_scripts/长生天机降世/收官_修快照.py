# -*- coding: utf-8 -*-
"""把 pipeline.json 快照从 1.0.3 更新到 1.0.4（收官步骤 4：修 pipeline 快照）。

只做机械替换：版本号、底包/so/正式包路径、挂点偏移；并在 notes 头追加收官事实。
不手改任何 stage 的 verify 语义。
"""
import io
import json
import re
import sys

P = "04_scripts/长生天机降世/pipeline.json"
s = io.open(P, encoding="utf-8").read()
before = s

# 1) 版本口径
s = s.replace("1.0.3 / vc1030 (TapTap 渠道)", "1.0.4 / vc1040 (TapTap 渠道)")
s = s.replace('"version": "1.0.3",', '"version": "1.0.4",')
# 2) 路径与文件名
s = s.replace("长生天机降世_1.0.3_official.apk", "长生天机降世_1.0.4_official.apk")
s = s.replace("libil2cpp_1.0.3.so", "libil2cpp_1.0.4.so")
s = s.replace("长生天机降世_1.0.3_surgical-releaseSigned.apk", "长生天机降世_1.0.4_surgical-releaseSigned.apk")
s = s.replace("surgical_cstjjs_1.0.3.sh", "surgical_cstjjs_1.0.3.sh")  # 1.0.4 无 Windows 脚本，保持原名不改
# 3) 挂点偏移
s = s.replace("--off 0x2657350 --va 0x265b350", "--off 0x2657F4C --va 0x265bf4c")
s = s.replace("file 0x2657350（VA 0x265b350", "file 0x2657F4C（VA 0x265bf4c")
s = s.replace("0x2657350", "0x2657F4C")
s = s.replace("0x265b350", "0x265bf4c")

# 4) notes 头追加收官事实
note = ("2026-10-28 收官（1.0.4 / vc1040，雷电 emulator-5554）：底包 2,277,631,016 B sha256 83dc5667115e4823… ；"
        "挂点 file 0x2657F4C（签名扫描唯一命中，运行时 hooked @0x400025a5bf4c）；"
        "正式包 2,275,713,348 B md5 5e9caf7184e88e1fcc8f69bdff83fe9d（体积红线 ≤2,277,634,048 B，见硬约束 B#7）；"
        "1.0.4 出包由 Windows 侧手工链路完成（mac 专用 surgical_cstjjs_1.0.3.sh 未派生 1.0.4 版），"
        "细节见 10_docs/长生天机降世/路线锁定.md §五 与 evidence_20261028/README.md。"
        "滚动物清理清单：10_docs/长生天机降世/滚动物清理清单_20261028.json；正式包裁剪清单：正式包裁剪清单_20261028.json。")
s = s.replace('"notes": "', '"notes": "' + note + " || ", 1)

io.open(P, "w", encoding="utf-8").write(s)
print("changed:", s != before, "len", len(s))

# 校验 JSON + 打印关键字段
d = json.load(io.open(P, encoding="utf-8"))
print("version:", d.get("version"))
print("params.version:", d.get("params", {}).get("version"))
for st in d.get("stages", []):
    cmd = st.get("command", "")
    if "1.0." in cmd:
        print(f"  [{st.get('id')}] {cmd[:160]}")
