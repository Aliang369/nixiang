#!/usr/bin/env python3
"""一次性补丁：manifest 加 android:appComponentFactory → StarryAppComponentFactory。

2026-10-31：签名轨挂载点。必须早于真实 Application 与所有 Provider。
"""
from pathlib import Path

P = Path("mod菜单/starry-mod-menu/scripts/surgical_mwdl_15.0.0.sh")
s = P.read_text(encoding="utf-8")

OLD = '''    t = t.replace("</application>", svc + "\\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (keep GameApplication)")'''

NEW = '''    t = t.replace("</application>", svc + "\\n    </application>", 1)
# 签名轨挂载点：appComponentFactory 早于真实 Application 与一切 Provider，
# 由它触发 bin.mt.signature.KillerApplication193.<clinit> 完成运行时签名替换。
if "appComponentFactory" not in t:
    import re as _re
    _m = _re.search(r"<application\\b[^>]*>", t)
    if not _m:
        raise SystemExit("application tag not found")
    _tag = _m.group(0)
    t = t.replace(_tag, _tag[:-1] + '\\n        android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory">', 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (keep GameApplication + appComponentFactory)")'''

if 'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"' in s:
    print("already patched")
    raise SystemExit(0)

assert OLD in s, "anchor not found"
s = s.replace(OLD, NEW, 1)
P.write_text(s, encoding="utf-8")
print("patched manifest step")
