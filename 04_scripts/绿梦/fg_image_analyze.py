#!/usr/bin/env python3
"""fg_image_analyze.py — 离线比对磁盘 libFairGuard.so 与运行期解包镜像（只读）

用法: python3 fg_image_analyze.py <disk.so> <image.bin>
输出: 解包变换统计 / 关键字 XOR 扫描 / 已知指纹搜索 / 内联 svc 枚举
"""
import sys, re, struct, hashlib

def load(p):
    with open(p, 'rb') as f:
        return f.read()

def hx(b):
    return b.hex()

disk = load(sys.argv[1]); img = load(sys.argv[2])
print('disk=%d img=%d' % (len(disk), len(img)))

# ---------- 1) 基本差异 ----------
n = min(len(disk), len(img))
same = sum(1 for i in range(0, n, 4096) if disk[i:i+4096] == img[i:i+4096])
print('4K pages identical: %d / %d' % (same, n // 4096))

# ---------- 2) XOR 流周期性检验（0x400..0x1F0000 段） ----------
X = bytes(a ^ b for a, b in zip(disk[0x400:0x1F0000], img[0x400:0x1F0000]))
print('xor: zero_ratio=%.4f  entropy-ish distinct=%d' % (X.count(0) / len(X), len(set(X[:65536]))))
for period in (1, 4, 8, 16, 32, 64, 256, 4096):
    seg = X[:period * 64]
    ok = all(seg[i] == seg[i % period] for i in range(len(seg)))
    print('  period %-5d repeat_in_first_4k=%s' % (period, ok))

# ---------- 3) 关键字 XOR 扫描（单字节 key 全试） ----------
KEYS = [b'META-INF', b'base.apk', b'signature', b'AndroidManifest', b'APK Sig Block 42', b'sha256', b'/data/app']
for kw in KEYS:
    hits = []
    for k in range(1, 256):
        t = bytes(c ^ k for c in kw)
        for m in re.finditer(re.escape(t), img):
            hits.append((m.start(), k))
            if len(hits) > 4: break
        if len(hits) > 4: break
    print('XOR-sweep %-18s hits=%s' % (kw.decode(), [(hex(o), hex(k)) for o, k in hits[:4]]))

# ---------- 4) 已知指纹（二进制 + 十六进制文本） ----------
FP = {
  'cert_sha256': 'cb41e01ea39b0d81ccb17c4bc6f4ef6a6fc4fcf1942adebcb8ada2c9a8da3081',
  'cert_sha1':   '85f4a11e736d1112e912a7ad50e97c5590f115e8',
  'cert_md5':    'dbcb6bbc8cfbef125cb598fb862e207a',
  'pubkey_sha256':'520fa5f2ac199069f9de7430e5afd7d313406c1329f4ce446adabf9b7ee4ecc7',
}
for name, h in FP.items():
    raw = bytes.fromhex(h)
    print('%-14s bin(disk)=%d bin(img)=%d hex(disk)=%d hex(img)=%d' % (
        name, disk.count(raw), img.count(raw), disk.count(h.encode()), img.count(h.encode())))

# ---------- 5) 内联 svc 枚举（全镜像） ----------
SVC = 0xD4000001
found = []
for off in range(0, len(img) - 4, 4):
    if struct.unpack_from('<I', img, off)[0] == SVC:
        prev = struct.unpack_from('<I', img, off - 4)[0] if off >= 4 else 0
        if (prev & 0xFFE0001F) == 0xD2800008:
            found.append((off - 4, (prev >> 5) & 0xFFFF))
        else:
            found.append((off, None))
print('inline svc sites: %d' % len(found))
from collections import Counter
print('x8 histogram:', Counter(v for _, v in found if v is not None).most_common(12))
for off, v in found:
    if v in (0x5E, 0x75, 0xA7, 0x38, 0xDD, 0xDE):
        print('  svc @%#x x8=%#x' % (off, v))