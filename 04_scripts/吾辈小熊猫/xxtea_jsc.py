#!/usr/bin/env python3
# script-policy: {"purpose": "吾辈小熊猫 Cocos 3.7.1 jsc 的 XXTEA(padding=False, 无 gzip) 加解密与往返自检", "inputs": "assets/assets/*/index.jsc（密文）/ 解密后的 .js（明文）", "outputs": "同名 .js 或 .jsc；selftest 只打印结论", "boundary": "只做编解码；不判断业务语义、不写回 APK", "prior_art": "04_scripts/_common/cocos_jsc_decrypt.sh（xxtea→gzip，本作缺 gzip 层 ⇒ 不可用，故另立）", "difference": "padding=False 且不做 gunzip；密钥从 10_docs/吾辈小熊猫/功能挂点分析.md §3.2 取得（a7wAilvNtHvYDTVG，30/30 文件验证）", "evidence": "10_docs/吾辈小熊猫/功能挂点分析.md#33"}
"""Cocos Creator 3.7.1 .jsc 编解码 —— XXTEA，padding=False，无 gzip 层。

本作（吾辈小熊猫 5.0）实测口径：
  · 密文 = XXTEA(明文 JS)，密钥 16 字节 ASCII "a7wAilvNtHvYDTVG"；
  · 解密后**直接**是 JS 源码（不是默认模板里的「XXTEA + gzip」）；
  · 长度必须是 4 的倍数（padding=False 不做补齐）。

用法：
  python3 xxtea_jsc.py decrypt <in.jsc> <out.js>
  python3 xxtea_jsc.py encrypt <in.js>  <out.jsc>
  python3 xxtea_jsc.py selftest <orig.jsc> [期望明文 .js]
      —— 解密→重加密→与原文逐字节比对；一致才算 enc/dec 互为逆运算。
"""
import struct
import sys

KEY = b'a7wAilvNtHvYDTVG'
DELTA = 0x9E3779B9
MASK = 0xFFFFFFFF


def _key_words(key):
    k = key[:16]
    if len(k) < 16:
        k = k + b'\x00' * (16 - len(k))   # cocos: 不足 16 字节补零
    return list(struct.unpack('<4I', k))


def _to_words(data):
    if len(data) % 4:
        raise SystemExit('ERROR: 长度 %d 不是 4 的倍数（padding=False 不支持）' % len(data))
    return list(struct.unpack('<%dI' % (len(data) // 4), data))


def _from_words(words):
    return struct.pack('<%dI' % len(words), *words)


def _mx(sum_, y, z, p, e, k):
    return ((((z >> 5) ^ ((y << 2) & MASK)) + ((y >> 3) ^ ((z << 4) & MASK))) ^
            ((sum_ ^ y) + (k[(p & 3) ^ e] ^ z))) & MASK


def decrypt(data, key=KEY):
    if len(data) < 8:
        return data
    v = _to_words(data)
    k = _key_words(key)
    n = len(v)
    rounds = 6 + 52 // n
    sum_ = (rounds * DELTA) & MASK
    y = v[0]
    while rounds > 0:
        e = (sum_ >> 2) & 3
        for p in range(n - 1, 0, -1):
            z = v[p - 1]
            v[p] = (v[p] - _mx(sum_, y, z, p, e, k)) & MASK
            y = v[p]
        z = v[n - 1]
        v[0] = (v[0] - _mx(sum_, y, z, 0, e, k)) & MASK
        y = v[0]
        sum_ = (sum_ - DELTA) & MASK
        rounds -= 1
    return _from_words(v)


def encrypt(data, key=KEY):
    if len(data) < 8:
        return data
    v = _to_words(data)
    k = _key_words(key)
    n = len(v)
    rounds = 6 + 52 // n
    sum_ = 0
    z = v[n - 1]
    while rounds > 0:
        sum_ = (sum_ + DELTA) & MASK
        e = (sum_ >> 2) & 3
        for p in range(0, n - 1):
            y = v[p + 1]
            v[p] = (v[p] + _mx(sum_, y, z, p, e, k)) & MASK
            z = v[p]
        y = v[0]
        v[n - 1] = (v[n - 1] + _mx(sum_, y, z, n - 1, e, k)) & MASK
        z = v[n - 1]
        rounds -= 1
    return _from_words(v)


def main():
    if len(sys.argv) < 3:
        raise SystemExit(__doc__)
    cmd = sys.argv[1]
    if cmd == 'decrypt':
        src, dst = sys.argv[2], sys.argv[3]
        raw = open(src, 'rb').read()
        out = decrypt(raw)
        open(dst, 'wb').write(out)
        print('[xxtea] decrypt %s (%d B) -> %s (%d B)' % (src, len(raw), dst, len(out)))
    elif cmd == 'encrypt':
        src, dst = sys.argv[2], sys.argv[3]
        raw = open(src, 'rb').read()
        out = encrypt(raw)
        open(dst, 'wb').write(out)
        print('[xxtea] encrypt %s (%d B) -> %s (%d B)' % (src, len(raw), dst, len(out)))
    elif cmd == 'selftest':
        src = sys.argv[2]
        raw = open(src, 'rb').read()
        plain = decrypt(raw)
        back = encrypt(plain)
        same = back == raw
        print('[selftest] %s' % src)
        print('  密文 %d B → 明文 %d B；重加密 %d B' % (len(raw), len(plain), len(back)))
        print('  明文首段: %r' % plain[:60])
        print('  重加密 == 原文: %s' % ('YES（enc/dec 互为逆运算）' if same else 'NO'))
        if len(sys.argv) > 3:
            ref = open(sys.argv[3], 'rb').read()
            print('  与既有解密产物一致: %s' % ('YES' if ref == plain else 'NO'))
        if not same:
            raise SystemExit(1)
    else:
        raise SystemExit(__doc__)


if __name__ == '__main__':
    main()
