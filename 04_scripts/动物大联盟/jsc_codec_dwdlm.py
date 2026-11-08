#!/usr/bin/env python3
"""动物大联盟 1.0.25 —— res 脚本 .jsc 编解码（离线，纯 Python）

格式（实测确定，2026-11-06）:
    文件 = XXTEA_encrypt( gzip(source) ) 的 uint32 小端字数组，末尾按 4 字节对齐（多余字节为 0 填充）
    XXTEA key = "d03398ba-c326-44"（16 字节，NUL 补齐；在解密后引擎 libcocos2djs.so 的 .rodata 里可直接搜到，
                紧邻 "Cocos Game" / "jsb-adapter/jsb-builtin.js" 常量）

依据:
    - 引擎 libcocos2djs.so:0x2ff784 -> 0x317a28(ptr,len,key,keylen,&out) -> 0x317af8 XXTEA 解密
      key 来自 .bss 全局 std::string（运行期值 = 上面的字符串，已从 pid 14242 内存读出核对）
    - 解密结果头为 1f 8b 08（gzip），解压后为 JS 源码
    - settings.jsc 220B -> 306B 源码，与运行期内存中的 window._CCSettings 明文逐字一致

用法:
    python3 jsc_codec_dwdlm.py dec <in> [out]
    python3 jsc_codec_dwdlm.py dec-tree <in_dir> <out_dir>
    python3 jsc_codec_dwdlm.py enc <in_js> [out_jsc]
    python3 jsc_codec_dwdlm.py info <in>          # 只报判定结果
"""
import struct, sys, os, zlib, gzip, io

DELTA = 0x9E3779B9
KEY = b"d03398ba-c326-44"


def _key_words(key=KEY):
    k = (key + b"\x00" * 16)[:16]
    return list(struct.unpack("<4I", k))


def _btea(v, k, decrypt=True):
    n = len(v)
    if n < 2:
        return v
    rounds = 6 + 52 // n
    M = 0xFFFFFFFF
    if decrypt:
        s = (rounds * DELTA) & M
        y = v[0]
        while s:
            e = (s >> 2) & 3
            for p in range(n - 1, 0, -1):
                z = v[p - 1]
                mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) ^ ((s ^ y) + (k[(p & 3) ^ e] ^ z))) & M
                v[p] = (v[p] - mx) & M
                y = v[p]
            z = v[n - 1]
            mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) ^ ((s ^ y) + (k[e] ^ z))) & M
            v[0] = (v[0] - mx) & M
            y = v[0]
            s = (s - DELTA) & M
    else:
        s = 0
        z = v[n - 1]
        for _ in range(rounds):
            s = (s + DELTA) & M
            e = (s >> 2) & 3
            for p in range(0, n - 1):
                y = v[p + 1]
                mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) ^ ((s ^ y) + (k[(p & 3) ^ e] ^ z))) & M
                v[p] = (v[p] + mx) & M
                z = v[p]
            y = v[0]
            mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) ^ ((s ^ y) + (k[(n - 1 & 3) ^ e] ^ z))) & M
            v[n - 1] = (v[n - 1] + mx) & M
            z = v[n - 1]
    return v


def _to_words(data):
    n = (len(data) + 3) // 4
    buf = bytearray(n * 4)
    buf[: len(data)] = data
    return list(struct.unpack("<%dI" % n, bytes(buf)))


def _from_words(v):
    return struct.pack("<%dI" % len(v), *v)


def decrypt(data, key=KEY):
    """返回 (plain_bytes, kind)；kind ∈ {'gzip','text'}；解不开抛 ValueError"""
    k = _key_words(key)
    out = _from_words(_btea(_to_words(data), k, decrypt=True))
    if out[:2] == b"\x1f\x8b":
        try:
            # 用 decompressobj：gzip 流尾部可能跟 4 字节对齐填充，必须忽略 unused_data
            d = zlib.decompressobj(16 + zlib.MAX_WBITS)
            plain = d.decompress(out) + d.flush()
            if plain:
                return plain, "gzip"
        except Exception:
            pass
    if out[:1] in (b"(", b"w", b"/", b"\n") or b"function" in out[:4096]:
        return out.rstrip(b"\x00"), "text"
    raise ValueError("decrypt failed (not gzip/text)")


def encrypt(src, key=KEY, level=6):
    """JS 源码 -> .jsc

    引擎格式（2026-11-06 由 libcocos2djs.so 0x317af8 反汇编 + 真机验证确认）：
        buf = gzip(js) || 0*pad || uint32_le(len(gzip))
        total = align4(len(gzip) + 4)          # 尾部 4 字节必须是"压缩前长度"
        jsc   = XXTEA_encrypt(buf)
    引擎解密后会校验 total-7 <= len <= total-4，不满足直接返回 NULL
    （旧实现少了这 4 字节 ⇒ 官方包报 "Can't decrypt code"）。
    """
    k = _key_words(key)
    buf = io.BytesIO()
    with gzip.GzipFile(fileobj=buf, mode="wb", compresslevel=level, mtime=0) as g:
        g.write(src)
    gz = buf.getvalue()
    n = len(gz)
    total = (n + 4 + 3) & ~3
    payload = gz + bytes(total - 4 - n) + struct.pack("<I", n)
    assert len(payload) == total
    return _from_words(_btea(_to_words(payload), k, decrypt=False))


def main(argv):
    if len(argv) < 3:
        print(__doc__)
        return 2
    cmd = argv[1]
    if cmd == "dec":
        out = decrypt(open(argv[2], "rb").read())
        dst = argv[3] if len(argv) > 3 else argv[2] + ".js"
        open(dst, "wb").write(out[0])
        print("OK %s -> %s (%s, %d B)" % (argv[2], dst, out[1], len(out[0])))
        return 0
    if cmd == "info":
        try:
            out = decrypt(open(argv[2], "rb").read())
            print("%s: %s, %d B, head=%r" % (argv[2], out[1], len(out[0]), out[0][:60]))
            return 0
        except Exception as e:
            print("%s: FAIL %s" % (argv[2], e))
            return 1
    if cmd == "dec-tree":
        src, dst = argv[2], argv[3]
        ok = bad = 0
        for root, _dirs, files in os.walk(src):
            for f in files:
                p = os.path.join(root, f)
                rel = os.path.relpath(p, src)
                try:
                    data, kind = decrypt(open(p, "rb").read())
                except Exception as e:
                    bad += 1
                    print("FAIL %-40s %s" % (rel, e))
                    continue
                name = rel[:-4] + ".js" if rel.endswith(".jsc") else rel
                tgt = os.path.join(dst, name)
                os.makedirs(os.path.dirname(tgt), exist_ok=True)
                open(tgt, "wb").write(data)
                ok += 1
                print("OK   %-40s %-5s %8d B" % (rel, kind, len(data)))
        print("done ok=%d fail=%d -> %s" % (ok, bad, dst))
        return 0 if bad == 0 else 1
    if cmd == "enc":
        dst = argv[3] if len(argv) > 3 else argv[2] + ".jsc"
        enc = encrypt(open(argv[2], "rb").read())
        open(dst, "wb").write(enc)
        # 自校验
        back, kind = decrypt(enc)
        assert back == open(argv[2], "rb").read(), "roundtrip mismatch"
        print("OK %s -> %s (%d B, roundtrip verified)" % (argv[2], dst, len(enc)))
        return 0
    print("unknown cmd", cmd)
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv))
