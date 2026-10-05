#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""feature_string_scan.py — 在池鸳 so/dex 里定位菜单功能名，并打印命中处上下文（只读）。

用法:
  python feature_string_scan.py FILE [FILE...]

对每个 UTF-8 / UTF-16LE 编码的模式，打印文件偏移与前后 48 字节的可读上下文，
用于判断功能名是 UI 文案、JNI 方法名还是 hook 符号。
"""
import sys

PATTERNS = ["全局速度", "无限", "大招", "定怪", "无敌", "倍率", "攻击", "暴击",
            "保存设置", "退出", "自动调整", "伤害", "速度", "技能"]


def ctx(data, i, n=48):
    lo = max(0, i - n)
    hi = min(len(data), i + n)
    out = []
    for b in data[lo:hi]:
        out.append(chr(b) if 32 <= b < 127 else ".")
    return "".join(out)

def scan(path, pat, data):
    hits = []
    for enc in ("utf-8", "utf-16le"):
        try:
            needle = pat.encode(enc)
        except Exception:
            continue
        start = 0
        while True:
            j = data.find(needle, start)
            if j < 0:
                break
            hits.append((j, enc))
            start = j + 1
            if len(hits) > 40:
                return hits
    return hits


def main():
    for path in sys.argv[1:]:
        with open(path, "rb") as f:
            data = f.read()
        print("== %s (%d bytes)" % (path, len(data)))
        for pat in PATTERNS:
            hits = scan(path, pat, data)
            if not hits:
                continue
            print("  %s: %d hit(s)" % (pat, len(hits)))
            for off, enc in hits[:8]:
                print("    0x%x [%s] %s" % (off, enc, ctx(data, off)))


if __name__ == "__main__":
    main()
