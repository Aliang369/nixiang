#!/usr/bin/env python3
"""给 libcocos2djs.so 预置 res 解密密钥（无壳包用）。

引擎把 16 字节 xxtea key 存在 .bss 的一个 libc++ std::string（SSO）里；无壳后没人写
⇒ 全零 ⇒ 所有 .jsc 解不开 ⇒ 卡「数据检查」。
做法：把 key 字节写进文件、把 PT_LOAD#1 的 p_filesz 扩到覆盖它，加载器就会从文件映射。
副作用处理：扩 filesz 会把文件尾（节头表等）当成 .bss 映射进去 ⇒ 必须把这段"尾巴"
整体搬到文件末尾并同步所有 sh_offset/e_shoff，再把 .bss 区间清零。
"""
import struct, sys

KEY = b"d03398ba-c326-44"
GLOBAL_VADDR = 0x222a158
LOAD_INDEX = 1
TAIL_ALIGN = 0x1000


def patch(path, out):
    d = bytearray(open(path, "rb").read())
    assert d[:4] == b"ELF" and d[4] == 2, "need ELF64"
    e_phoff, = struct.unpack_from("<Q", d, 0x20)
    e_shoff, = struct.unpack_from("<Q", d, 0x28)
    e_phentsize, e_phnum = struct.unpack_from("<HH", d, 0x36)
    e_shentsize, e_shnum = struct.unpack_from("<HH", d, 0x3a)

    loads = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        (p_type, p_flags, p_offset, p_vaddr, p_paddr,
         p_filesz, p_memsz, p_align) = struct.unpack_from("<IIQQQQQQ", d, off)
        if p_type == 1:
            loads.append((off, p_offset, p_vaddr, p_filesz, p_memsz))
    phoff, p_offset, p_vaddr, p_filesz, p_memsz = loads[LOAD_INDEX]

    delta_v = GLOBAL_VADDR - p_vaddr
    assert 0 <= delta_v < p_memsz
    foff = p_offset + delta_v
    key_end = foff + 32
    seg_end_old = p_offset + p_filesz
    old_len = len(d)
    print("key@0x%x  seg_end_old=0x%x  filelen=0x%x  shoff=0x%x shnum=%d" %
          (foff, seg_end_old, old_len, e_shoff, e_shnum))

    need_move = seg_end_old < key_end
    if need_move:
        tail = bytes(d[seg_end_old:old_len])
        new_tail = (key_end + TAIL_ALIGN - 1) // TAIL_ALIGN * TAIL_ALIGN
        shift = new_tail - seg_end_old
        print("tail 0x%x..0x%x (%d B) -> 0x%x (shift +0x%x)" % (seg_end_old, old_len, len(tail), new_tail, shift))
        d.extend(bytes(new_tail - len(d)))
        d[new_tail:new_tail + len(tail)] = tail
        # 修正节头里的 sh_offset（落在被搬走区间的）
        for i in range(e_shnum):
            so = e_shoff + i * e_shentsize
            sh_offset, = struct.unpack_from("<Q", d, so + 0x18)
            if sh_offset >= seg_end_old:
                struct.pack_into("<Q", d, so + 0x18, sh_offset + shift)
        # 节头表自身位置
        struct.pack_into("<Q", d, 0x28, e_shoff + shift)
        print("e_shoff 0x%x -> 0x%x" % (e_shoff, e_shoff + shift))
        # 清空旧区间（保持 .bss 全零语义）
        n = key_end - seg_end_old
        d[seg_end_old:seg_end_old + n] = bytes(n)
        print("zeroed [0x%x,0x%x)" % (seg_end_old, seg_end_old + n))

    if key_end - p_offset > p_filesz:
        struct.pack_into("<Q", d, phoff + 32, key_end - p_offset)
        print("p_filesz 0x%x -> 0x%x" % (p_filesz, key_end - p_offset))

    payload = bytes([len(KEY) << 1]) + KEY + bytes(1)
    d[foff:foff + len(payload)] = payload
    open(out, "wb").write(d)
    print("wrote", out, len(d))


if __name__ == "__main__":
    patch(sys.argv[1], sys.argv[2])
