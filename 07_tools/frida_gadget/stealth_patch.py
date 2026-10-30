#!/usr/bin/env python3
"""frida-gadget 去特征：等长替换可被内存扫描命中的标识串。
用法: python3 stealth_patch.py <in.so> <out.so>
等长替换 ⇒ 不改变任何偏移/长度，ELF 结构与重定位不受影响。
"""
import sys

# 等长替换表（长度必须一致）
MAP = [
    (b'frida', b'froda'),
    (b'Frida', b'Froda'),
    (b'FRIDA', b'FRODA'),
    (b'gum-js-loop', b'gvm-js-loop'),
    (b'gum', b'gvm'),
    (b'Gum', b'Gvm'),
    (b'GUM', b'GVM'),
    (b'gmain', b'xmain'),
    (b'gdbus', b'xdbus'),
    (b'linjector', b'l1njector'),
]

def main():
    if len(sys.argv) != 3:
        print(__doc__); return 2
    src, dst = sys.argv[1], sys.argv[2]
    data = bytearray(open(src, 'rb').read())
    total = 0
    report = []
    # 先长后短，避免 gum-js-loop 被 gum 先改掉
    for old, new in sorted(MAP, key=lambda x: -len(x[0])):
        assert len(old) == len(new)
        n = data.count(old)
        if n:
            data = bytearray(bytes(data).replace(old, new))
            total += n
            report.append('%s -> %s : %d' % (old.decode(), new.decode(), n))
    open(dst, 'wb').write(data)
    print('\n'.join(report))
    print('总替换 %d 处 → %s' % (total, dst))
    return 0

if __name__ == '__main__':
    sys.exit(main())
