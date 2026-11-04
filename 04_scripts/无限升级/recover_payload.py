# -*- coding: utf-8 -*-
"""从已装 APK 里解回明文补丁 DLL（无限升级 vc1637 专用，2026-11-04）

背景：游戏不把 targetfuntional.dll 落盘，只在运行时内存里；出包机（mac）的
07_patched/无限升级/ 不在 Windows 端。但包内 so 里嵌着三段密文载荷，密文头
（Includes/sg_payload_enc_wxsj_vc1637.h）留了每段的明文 sha256 与 MAC，本机
~/.starry-keys/payload_master.txt 是同一份主密钥 ⇒ 可解回并逐字节校验。

用法：
  python 04_scripts/无限升级/recover_payload.py --apk <base.apk> [--out 07_patched/无限升级]

只读 APK，只往 --out 写解出来的 DLL；不碰包、不碰设备。
"""
import argparse
import hashlib
import hmac
import io
import os
import re
import sys
import zipfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
HDR = os.path.join(ROOT, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni',
                   'Includes', 'sg_payload_enc_wxsj_vc1637.h')
def _find_master():
    cands = []
    if os.environ.get('STARRY_PAYLOAD_MASTER'):
        cands.append(os.environ['STARRY_PAYLOAD_MASTER'])
    for h in (os.environ.get('USERPROFILE'), os.environ.get('HOME'), os.path.expanduser('~')):
        if h and h != '~':
            cands.append(os.path.join(h, '.starry-keys', 'payload_master.txt'))
    cands.append(os.path.join('C:/Users/Administrator', '.starry-keys', 'payload_master.txt'))
    for c in cands:
        if os.path.exists(c):
            return c
    return cands[0]


MASTER = _find_master()
SO_IN_APK = 'lib/arm64-v8a/libstarry_magic.so'
PE_MAGIC = b'MZ' + bytes([0x90, 0x00, 0x03, 0x00, 0x00, 0x00])


def build_key(master, digest):
    return hmac.new(master, b'sgpay-build|' + digest, hashlib.sha256).digest()


def keystream(key, n):
    out = bytearray()
    ctr = 0
    while len(out) < n:
        out += hmac.new(key, b'sgp1' + ctr.to_bytes(4, 'big'), hashlib.sha256).digest()
        ctr += 1
    return bytes(out[:n])


def xor(data, ks):
    n = len(data)
    rep = (ks * (n // len(ks) + 1))[:n]
    out = bytes(a ^ b for a, b in zip(data, rep))
    return out


def parse_header(text):
    sha = re.search(r'kWxsjvcPayloadSha = "([0-9a-f]{64})"', text).group(1)
    seg_re = (r'/\* (\w+)[：:]([^*]*?)\s+(\d+) B \*/\s*'
              r'static const uint32_t kWxsjvc(\w+)PayloadLen = (\d+)u;\s*'
              r'static const uint8_t kWxsjvc\4PayloadMac\[32\] = \{([^}]*)\};\s*'
              r'static const char\* kWxsjvc\4PlainSha = "([0-9a-f]{64})"')
    segs = []
    for m in re.finditer(seg_re, text):
        segs.append(dict(name=m.group(1),
                         src=m.group(2).strip(),
                         outfile=os.path.basename(m.group(2).strip()),
                         length=int(m.group(5)),
                         mac=bytes(int(x) for x in m.group(6).replace(' ', '').split(',') if x),
                         plain_sha=m.group(7)))
    ab_re = (r'static const uint32_t kWxsjvcAntibanLen = (\d+)u;\s*'
             r'static const uint8_t kWxsjvcAntibanMac\[32\] = \{([^}]*)\};\s*'
             r'static const char\* kWxsjvcAntibanSha = "([0-9a-f]{64})"')
    ab = re.search(ab_re, text)
    antiban = None
    if ab:
        antiban = dict(name='antiban', length=int(ab.group(1)),
                       mac=bytes(int(x) for x in ab.group(2).replace(' ', '').split(',') if x),
                       plain_sha=ab.group(3))
    return sha, segs, antiban


def find_and_decrypt(so, key, length, mac_want, plain_sha, label):
    ks = keystream(key, length)
    pat = bytes(a ^ b for a, b in zip(PE_MAGIC, ks[:len(PE_MAGIC)]))
    hits = []
    start = 0
    while True:
        i = so.find(pat, start)
        if i < 0:
            break
        hits.append(i)
        start = i + 1
    print('  [%s] 候选偏移 %s' % (label, hits if hits else '无'))
    for off in hits:
        ct = so[off:off + length]
        if len(ct) != length:
            continue
        plain = xor(ct, ks)
        if hashlib.sha256(plain).hexdigest() != plain_sha:
            continue
        if hmac.new(key, b'sgm1' + ct, hashlib.sha256).digest() != mac_want:
            print('  [%s] @%d sha 对但 MAC 不符（跳过）' % (label, off))
            continue
        print('  [%s] 命中 @%d  sha256=%s  MAC ok' % (label, off, plain_sha[:16]))
        return plain
    return None


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument('--apk', required=True)
    ap.add_argument('--out', default=os.path.join(ROOT, '07_patched', '无限升级'))
    a = ap.parse_args(argv)

    if not os.path.exists(a.apk):
        raise SystemExit('APK 不存在: %s' % a.apk)
    if not os.path.exists(MASTER):
        raise SystemExit('主密钥缺失: %s' % MASTER)
    text = io.open(HDR, 'r', encoding='utf-8').read()
    sha_hex, segs, antiban = parse_header(text)
    master = bytes.fromhex(io.open(MASTER, 'r', encoding='ascii').read().strip()[:64])
    key = build_key(master, bytes.fromhex(sha_hex))
    print('载荷 sha=%s  段数=%d' % (sha_hex[:16], len(segs)))

    with zipfile.ZipFile(a.apk) as z:
        so = z.read(SO_IN_APK)
    print('so: %s  %d B' % (SO_IN_APK, len(so)))

    os.makedirs(a.out, exist_ok=True)
    ok, bad = 0, []
    for s in segs:
        plain = find_and_decrypt(so, key, s['length'], s['mac'], s['plain_sha'], s['name'])
        if plain is None:
            bad.append(s['name'])
            continue
        dst = os.path.join(a.out, s['outfile'])
        io.open(dst, 'wb').write(plain)
        print('    -> %s  %d B' % (dst, len(plain)))
        ok += 1
    if antiban:
        abkey = hashlib.sha256(b'starry-antiban-v1|' + sha_hex.encode('ascii')).digest()
        plain = find_and_decrypt(so, abkey, antiban['length'], antiban['mac'], antiban['plain_sha'], 'antiban')
        if plain:
            dst = os.path.join(a.out, 'targetfuntional_vc1637_antiban.dll')
            io.open(dst, 'wb').write(plain)
            print('    -> %s  %d B' % (dst, len(plain)))
            ok += 1
        else:
            bad.append('antiban')

    tail = ('；失败：' + ','.join(bad)) if bad else '；全部通过 sha+MAC 校验'
    print('解出 %d 段%s' % (ok, tail))
    return 0 if not bad else 1


if __name__ == '__main__':
    sys.exit(main())
