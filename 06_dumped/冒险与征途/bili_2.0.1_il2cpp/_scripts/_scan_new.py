# -*- coding: utf-8 -*-
"""新包 libil2cpp.so 静态结构对比 + 字符串 xref + BL 调用点统计（只读）
用法: python3 _scan_new.py elf | find | xref | bl
"""
import sys, struct, os, json

SO = "/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp"

PT = {0:"NULL",1:"LOAD",2:"DYNAMIC",3:"INTERP",4:"NOTE",5:"SHLIB",6:"PHDR",7:"TLS",
      0x6474e550:"GNU_STACK",0x6474e551:"GNU_RELRO",0x6474e552:"GNU_PROPERTY"}

def parse():
    with open(SO,"rb") as f:
        hdr = f.read(64)
        assert hdr[:4]==b"\x7fELF" and hdr[4]==2
        (e_type,e_machine,e_version,e_entry,e_phoff,e_shoff,e_flags,e_ehsize,
         e_phentsize,e_phnum,e_shentsize,e_shnum,e_shstrndx)=struct.unpack_from("<HHIQQQIHHHHHH",hdr,16)
        f.seek(e_phoff); ph_raw=f.read(e_phentsize*e_phnum)
        f.seek(e_shoff); sh_raw=f.read(e_shentsize*e_shnum)
        shstr=b""
        if e_shstrndx<e_shnum:
            o=e_shstrndx*e_shentsize
            (n,t,fl,ad,of,sz,lk,inf,al,es)=struct.unpack_from("<IIQQQQIIQQ",sh_raw,o)
            f.seek(of); shstr=f.read(sz)
    ph=[]
    for i in range(e_phnum):
        o=i*e_phentsize
        v=struct.unpack_from("<IIQQQQQQ",ph_raw,o)
        ph.append(dict(i=i,type=v[0],flags=v[1],off=v[2],vaddr=v[3],filesz=v[5],memsz=v[6],align=v[7]))
    sh=[]
    for i in range(e_shnum):
        o=i*e_shentsize
        (n,t,fl,ad,of,sz,lk,inf,al,es)=struct.unpack_from("<IIQQQQIIQQ",sh_raw,o)
        e=shstr.find(b"\0",n) if n<len(shstr) else -1
        name=shstr[n:e].decode() if e>0 else "?"
        sh.append(dict(i=i,name=name,type=t,flags=fl,addr=ad,off=of,size=sz,entsize=es))
    return dict(entry=e_entry,phoff=e_phoff,phnum=e_phnum,shoff=e_shoff,shnum=e_shnum,ph=ph,sh=sh)

OLD_LOAD = [ (0x0,0x0,0x1D7308C), (0x1D7308C,0x1D7408C,0x2F8B204),
             (0x4CFE290,0x4D00290,0x347388), (0x5045618,0x5048618,0x2B5AB0) ]
OLD_SH = {".rodata":0xCD3A40,".text":0x1D7408C,"il2cpp":0x22C6708,".data":0x5048618,
          ".data.rel.ro":0x4D00290,".gcc_except_table":0x124E72C}

def cmd_elf():
    e=parse()
    print("e_entry=%#x phnum=%d shnum=%d shstrndx=? filesize=%d (%#x)"%(e["entry"],e["phnum"],e["shnum"],os.path.getsize(SO),os.path.getsize(SO)))
    print("\n## PT_LOAD (new) vs (old)")
    for p in e["ph"]:
        if p["type"]==1:
            fl=("R" if p["flags"]&4 else "-")+("W" if p["flags"]&2 else "-")+("X" if p["flags"]&1 else "-")
            print("  LOAD %s off=%#x vaddr=%#x filesz=%#x memsz=%#x delta=%#x"%(fl,p["off"],p["vaddr"],p["filesz"],p["memsz"],p["vaddr"]-p["off"]))
    print("  old LOADs:", ["%#x/%#x/%#x"%(a,b,c) for a,b,c in OLD_LOAD])
    print("\n## sections new vs old addr")
    for s in e["sh"]:
        o=OLD_SH.get(s["name"])
        mark=""
        if o is not None:
            mark = "SAME" if o==s["addr"] else "DIFF(old=%#x)"%o
        print("  %-22s type=%#04x addr=%#010x off=%#010x size=%#010x %s"%(s["name"],s["type"],s["addr"],s["off"],s["size"],mark))
    json.dump(e,open(OUT+"/_elf_new.json","w"),indent=0)
    print("\nwrote _elf_new.json")

def load_bytes():
    with open(SO,"rb") as f: return f.read()

def cmd_find():
    d=load_bytes()
    for pat in (b"AllocFrame",b"AllocStackSlot",b"alloc_frame"):
        hits=[];i=0
        while True:
            i=d.find(pat,i)
            if i<0: break
            hits.append(i); i+=1
        print("%-16s hits=%d %s"%(pat.decode(),len(hits),["%#x"%h for h in hits[:10]]))

if __name__=="__main__":
    cmd={"elf":cmd_elf,"find":cmd_find}
    cmd[sys.argv[1]]()
