#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fg_emu.py — 用 Unicorn 模拟执行 FairGuard 壳的 init，抓 decrypt 循环。

用法: python3 fg_emu.py [--insn N] [--dump out.bin]
产出: 写入 0x400..0x1F0000 的 store 统计、明文导出、syscall 轨迹
"""
import argparse, struct, sys
from unicorn import *
from unicorn.arm64_const import *

DISK='06_dumped/绿梦_20261016/libFairGuard.so'
REF ='06_dumped/绿梦_20261016/fg_runtime/libFairGuard_rwxp.bin'
BASE_DEFAULT=0x100000000
BASE=BASE_DEFAULT

def phdrs(d):
    phoff=struct.unpack_from('<Q', d, 0x20)[0]
    phentsize=struct.unpack_from('<H', d, 0x36)[0]
    phnum=struct.unpack_from('<H', d, 0x38)[0]
    out=[]
    for i in range(phnum):
        o=phoff+i*phentsize
        t,fl,off,va,pa,fs,ms,al=struct.unpack_from('<IIQQQQQQ', d, o)
        out.append(dict(type=t, off=off, va=va, filesz=fs, memsz=ms, flags=fl))
    return out

def align(x, a=0x1000): return (x + a - 1) & ~(a - 1)

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('--insn', type=int, default=2_000_000)
    ap.add_argument('--dump', default='/tmp/fg_emu_out.bin')
    ap.add_argument('--entry', default='0x413b0c')
    ap.add_argument('--no-fake-elf', action='store_true')
    ap.add_argument('--base', default='0x100000000', help='模块基址；要与 --preload 的 dump 基址一致')
    ap.add_argument('--preload', default='', help='运行期 rw 段 dump（如 /tmp/fg_rw.bin），按 --preload-va 预置')
    ap.add_argument('--preload-va', default='0x442000')
    ap.add_argument('--trace', default='', help='逗号分隔的 VA 列表，命中即打印一次（不停止）')
    a=ap.parse_args()
    d=open(DISK,'rb').read()
    global BASE
    BASE=int(a.base,16)
    mu=Uc(UC_ARCH_ARM64, UC_MODE_ARM)
    segs=[p for p in phdrs(d) if p['type']==1]
    print('PT_LOAD segs:', [(hex(p['off']), hex(p['va']), hex(p['filesz']), hex(p['memsz'])) for p in segs])
    # 一次性映射 [BASE, BASE+0x500000)，避免跨 4GB 边界的 mem_map 失效
    mu.mem_map(BASE, 0x500000, UC_PROT_ALL)
    for p in segs:
        mu.mem_write(BASE+p['va'], d[p['off']:p['off']+p['filesz']])
    # ---- 应用 R_AARCH64_RELATIVE / GLOB_DAT（否则壳里的函数指针/数据指针为 0） ----
    STUB=BASE+0x600000
    mu.mem_map(STUB, 0x10000, UC_PROT_ALL)
    mu.mem_write(STUB, (bytes.fromhex('c0035fd6')*0x4000))  # 全填 ret
    OBJ=STUB+0x10000
    mu.mem_map(OBJ, 0x10000, UC_PROT_ALL)
    RELA=0x412FC8; RELASZ=0x930
    nrel=0; gi=0
    for i in range(RELASZ//24):
        off, info, add = struct.unpack_from('<QQq', d, RELA+i*24)
        typ = info & 0xffffffff
        if typ == 1027:      # R_AARCH64_RELATIVE
            mu.mem_write(BASE+off, struct.pack('<Q', (BASE+add) & 0xffffffffffffffff)); nrel+=1
        elif typ == 1025:    # R_AARCH64_GLOB_DAT -> 指向"结构化假对象"（字段全指向 ret 桩）
            obj=OBJ+(gi*0x200)
            mu.mem_write(obj, struct.pack('<Q', STUB)*0x40)
            mu.mem_write(BASE+off, struct.pack('<Q', obj)); nrel+=1; gi+=1
    print('applied relocations:', nrel)
    if a.preload:
        blob=open(a.preload,'rb').read()
        mu.mem_write(BASE+int(a.preload_va,16), blob)
        print('preloaded %d B rw-dump at BASE+%s (base=%#x)' % (len(blob), a.preload_va, BASE))
    # maps 自检会把每段起始地址当 ELF 头比对（0x464C457F + e_machine==0xB7）。
    # BASE+0x413000 在文件中部、本来不是 ELF 头，会让自检失败 -> init 早退。
    # 这里把 ELF 头拷到该段起点，让自检全过（只为观测后续行为，不改变密文区）。
    if not a.no_fake_elf:
        mu.mem_write(BASE+0x413000, d[0:0x40])
        print('fake ELF header planted at BASE+0x413000')
    # 栈 + 堆
    STACK=0x7f000000; mu.mem_map(STACK, 0x200000, UC_PROT_ALL)
    mu.reg_write(UC_ARM64_REG_SP, STACK+0x180000)
    HEAP=0x200000000; mu.mem_map(HEAP, 0x8000000, UC_PROT_ALL)
    heap={'ptr':HEAP+0x1000}
    io={'fd':0, 'fds':{}, 'paths':{}, 'files':{}, 'reads':[]}
    def make_fake(path):
        """给 openat 打开的路径造内容。/proc/self/maps 用药一份合理的映射表。"""
        if 'maps' in path:
            mod='%012x-%012x rwxp 00000000 00:00 0' % (BASE, BASE+0x413000)
            lib='%012x-%012x r-xp 00000000 00:00 0' % (BASE+0x413000, BASE+0x445000)
            apk='%012x-%012x r--p 00000000 00:00 0' % (0x7f0000000, 0x7f0001000)
            # 注意：壳会拿 maps 里每段起始地址去解引用、比对 ELF 魔数（0x4232B0 处 ldr [x1] 比 0x464C457F，
            # 再查 e_machine==0xB7），所以 maps 里只能出现**模拟器真的映射过**的地址，否则会 UC_ERR_READ_UNMAPPED。
            lines=[
              '%012x-%012x rwxp 00000000 00:00 0       /data/app/~~abc/com.zhuziplay.lvmengnew.op-xyz/lib/arm64/libFairGuard.so' % (BASE, BASE+0x413000),
              '%012x-%012x r-xp 00000000 00:00 0       /data/app/~~abc/com.zhuziplay.lvmengnew.op-xyz/lib/arm64/libFairGuard.so' % (BASE+0x413000, BASE+0x445000),
              '%012x-%012x rw-p 00000000 00:00 0          [stack]' % (STACK, STACK+0x200000),
              '%012x-%012x rw-p 00000000 00:00 0          [heap]' % (HEAP, HEAP+0x800000),
            ]
            return ('\n'.join(lines)+'\n').encode()
        return d
    LOG=[]
    wr=dict(count=0, pages={}, first=None, last=None)
    zones={}
    def on_write(uc, access, addr, size, value, data):
        if BASE <= addr < BASE+0x1f0000:
            k='image_low'
        elif BASE+0x1f0000 <= addr < BASE+0x440000:
            k='image_zero'
        elif BASE+0x440000 <= addr < BASE+0x500000:
            k='image_data'
        elif HEAP <= addr < HEAP+0x8000000:
            k='heap'
        elif STACK <= addr < STACK+0x200000:
            k='stack'
        else:
            k='other'
        z=zones.setdefault(k, {'n':0,'first':None,'last':None})
        z['n']+=1
        if z['first'] is None: z['first']=addr
        z['last']=addr
        if BASE+0x400 <= addr < BASE+0x1f0000:
            wr['count']+=1
            pg=(addr-BASE)>>12
            wr['pages'][pg]=wr['pages'].get(pg,0)+1
            if wr['first'] is None: wr['first']=addr-BASE
            wr['last']=addr-BASE
    mu.hook_add(UC_HOOK_MEM_WRITE, on_write)
    # svc 桩：找出所有 svc #0 指令地址
    svcs={}
    off=0x413ab0
    while off < 0x431350:
        if d[off:off+4]==bytes.fromhex('010000d4'):
            svcs[BASE+off]=off
        off+=4
    syscalls=[]
    def on_code(uc, addr, size, data):
        o=svcs.get(addr)
        if o is None: return
        x8=uc.reg_read(UC_ARM64_REG_X8)
        args=[uc.reg_read(UC_ARM64_REG_X0+i) for i in range(6)]
        syscalls.append((x8, args))  # args 后追加 ret 由下方记录
        ret=-1
        if x8==0x38:  # openat -> 先把路径字符串读出来
            path=''
            if args[1]:
                try:
                    raw=bytes(mu.mem_read(args[1], 128)).split(b'\x00')[0]
                    path=raw.decode('latin1')
                except Exception:
                    path='<unreadable>'
            fd=io['fd']+10; io['fd']+=1; io['fds'][fd]=0; ret=fd
            # 为已知路径准备"假文件内容"：/proc/self/maps 必须给一份合理的 maps，
            # 否则壳会一直顺序读（真实 maps 只有几十 KB，我们的回退是整份 so）。
            io['files'][fd]=make_fake(path)
            if path not in io['paths'].values():
                print('[openat] fd=%d path=%r' % (fd, path)); io['paths'][fd]=path
        elif x8==0x3f:  # read -> 从磁盘文件按游标喂数据
            fd=args[0]; cur=io['fds'].get(fd, 0); n=args[2]
            src=io['files'].get(fd, d)
            chunk=src[cur:cur+n]
            if chunk:
                mu.mem_write(args[1], chunk); io['fds'][fd]=cur+len(chunk)
            ret=len(chunk)
            io['reads'].append((fd, cur, n, ret))
            if len(io['reads'])<=12 or len(io['reads'])%500==0:
                print('[read] fd=%d cursor=%#x n=%#x ret=%d' % (fd, cur, n, ret))
        elif x8==0x3e:  # lseek
            fd=args[0]; io['fds'][fd]=args[1]; ret=args[1]
        elif x8==0xde:  # mmap
            ret=heap['ptr']; heap['ptr']+=align(args[1]) if args[1] else 0x1000
        elif x8 in (0xe2, 0xd7):  # mprotect/munmap
            ret=0
        elif x8==0x5e:  # exit_group
            uc.emu_stop(); return
        uc.reg_write(UC_ARM64_REG_X0, ret & 0xffffffffffffffff)
        lr=uc.reg_read(UC_ARM64_REG_X30)
        uc.reg_write(UC_ARM64_REG_PC, lr)
    mu.hook_add(UC_HOOK_CODE, on_code)
    trace={BASE+int(x,16): x for x in a.trace.split(',') if x}
    if trace:
        def on_trace(uc, addr, size, data):
            print('[TRACE] hit %s pc=%#x x0=%#x x1=%#x x2=%#x x8=%#x' % (
                trace[addr], addr, uc.reg_read(UC_ARM64_REG_X0), uc.reg_read(UC_ARM64_REG_X1),
                uc.reg_read(UC_ARM64_REG_X2), uc.reg_read(UC_ARM64_REG_X8)))
        for va in trace:
            mu.hook_add(UC_HOOK_CODE, on_trace, begin=va, end=va)
        print('trace points: %s' % a.trace)
    def on_zero(uc, addr, size, data):
        # PC==0 通常不是"正常返回"：多因惰性映射把未映射读变成 0，随后 blr x0 跳到 0。
        # emu_start(until=0) 会把它当成命中 until 正常结束，从而掩盖真实崩溃（本文件 §18 的教训）。
        print('[WARN] PC=0 —— 跳到了空指针（多半是惰性零页吃掉了真实崩溃），停止')
        uc.emu_stop()
    mu.hook_add(UC_HOOK_CODE, on_zero, begin=0, end=0)
    def on_stub(uc, addr, size, data):
        print('[ret] init 返回了（LR 回到桩页），停止')
        uc.emu_stop()
    mu.hook_add(UC_HOOK_CODE, on_stub, begin=STUB, end=STUB)
    # 懒映射：预置真实 .data 后会有指向设备堆/栈的指针，遇到未映射访问就补一页零页继续跑
    lazy={'n':0}
    def lazy_map(uc, access, addr, size, value, data):
        if lazy['n'] > 4096: return False
        pg=addr & ~0xfff
        try:
            uc.mem_map(pg, 0x1000, UC_PROT_ALL)
        except UcError:
            try: uc.mem_map(pg, 0x10000, UC_PROT_ALL); pg=addr & ~0xffff
            except UcError: return False
        lazy['n']+=1
        return True
    for h in (UC_HOOK_MEM_READ_UNMAPPED, UC_HOOK_MEM_WRITE_UNMAPPED, UC_HOOK_MEM_FETCH_UNMAPPED):
        mu.hook_add(h, lazy_map)
    entries=[int(x,16) for x in a.entry.split(',')]
    pc=BASE+entries[0]
    mu.reg_write(UC_ARM64_REG_X30, STUB)
    print('start pc=%#x' % pc)
    for i,ent in enumerate(entries):
        mu.reg_write(UC_ARM64_REG_X30, STUB)
        print('--- run ctor %d at %#x (writes to payload so far: %d)' % (i, BASE+ent, wr['count']))
        try:
            mu.emu_start(BASE+ent, 0, count=a.insn)
        except UcError as e:
            print('UC error:', e, 'pc=%#x' % mu.reg_read(UC_ARM64_REG_PC))
        if mu.reg_read(UC_ARM64_REG_PC)==0:
            # until=0 会把"跳到空指针"当成命中 until 正常结束 -> 必须显式报警，否则误判为"正常返回"
            print('[WARN] ctor %d 结束时 PC=0：跳到了空指针（惰性零页吃掉了真实崩溃），本轮结果不可信' % i)
    print('final pc=%#x' % mu.reg_read(UC_ARM64_REG_PC))
    for k,z in zones.items():
        print('  zone %-11s writes=%7d first=%s last=%s' % (k, z['n'],
              hex(z['first']) if z['first'] else '-', hex(z['last']) if z['last'] else '-'))
    print('writes into payload region: %d, pages=%d, first=%s last=%s' % (
        wr['count'], len(wr['pages']),
        hex(wr['first']) if wr['first'] is not None else '-',
        hex(wr['last']) if wr['last'] is not None else '-'))
    print('lazy-mapped pages: %d (非 0 表示有未映射访问被零页顶替，结果不可信)' % lazy['n'])
    print('syscalls: %d' % len(syscalls))
    for n,ar in syscalls[:20]:
        print('  syscall x8=%#x args=%s ret=%#x' % (n, [hex(x) for x in ar[:3]], ar[6] if len(ar)>6 else 0))
    out=mu.mem_read(BASE, 0x400000)
    open(a.dump,'wb').write(bytes(out))
    ref=open(REF,'rb').read()
    same=sum(1 for i in range(0, 0x400000, 4096) if out[i:i+4096]==ref[i:i+4096])
    print('dump -> %s ; pages identical to runtime image: %d/1024' % (a.dump, same))

main()
