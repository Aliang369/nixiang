# -*- coding: utf-8 -*-
"""Parse ELF64 program headers + section headers of libil2cpp.so.
Outputs a RVA<->file-offset mapping table for key segments.
Read-only. No third-party deps (struct only).
"""
import sys, struct, json, os

SO = r"E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so"
OUT = r"E:\nixiang\06_dumped\冒险与征途\il2cpp_sections.md"

PT = {0:"NULL",1:"LOAD",2:"DYNAMIC",3:"INTERP",4:"NOTE",5:"SHLIB",6:"PHDR",7:"TLS",
      0x60000000:"GNU_EH_FRAME",0x6474e550:"GNU_STACK",0x6474e551:"GNU_RELRO",0x6474e552:"GNU_PROPERTY"}
SHT = {0:"NULL",1:"PROGBITS",2:"SYMTAB",3:"STRTAB",4:"RELA",5:"HASH",6:"DYNAMIC",7:"NOTE",
       8:"NOBITS",9:"REL",10:"SHLIB",11:"DYNSYM",14:"INIT_ARRAY",15:"FINI_ARRAY",
       16:"PREINIT_ARRAY",17:"GROUP",18:"SYMTAB_SHNDX",19:"RELR",0x6ffffff6:"GNU_HASH",
       0x6ffffffe:"VERNEED",0x6fffffff:"VERSYM"}

with open(SO,"rb") as f:
    data = f.read(64)
    assert data[:4] == b"\x7fELF", "not ELF"
    e_ident_class = data[4]
    assert e_ident_class == 2, "not ELF64"
    (e_type,e_machine,e_version,e_entry,e_phoff,e_shoff,e_flags,e_ehsize,
     e_phentsize,e_phnum,e_shentsize,e_shnum,e_shstrndx) = struct.unpack_from("<HHIQQQIHHHHHH", data, 16)
    f.seek(e_phoff); ph_raw = f.read(e_phentsize*e_phnum)
    f.seek(e_shoff); sh_raw = f.read(e_shentsize*e_shnum)

print("e_entry=%#x phoff=%#x phnum=%d shoff=%#x shnum=%d shstrndx=%d" %
      (e_entry,e_phoff,e_phnum,e_shoff,e_shnum,e_shstrndx))

phdrs=[]
for i in range(e_phnum):
    o=i*e_phentsize
    (p_type,p_flags,p_offset,p_vaddr,p_paddr,p_filesz,p_memsz,p_align)=struct.unpack_from("<IIQQQQQQ",ph_raw,o)
    phdrs.append(dict(i=i,type=p_type,flags=p_flags,off=p_offset,vaddr=p_vaddr,filesz=p_filesz,memsz=p_memsz,align=p_align))

shdrs=[]
for i in range(e_shnum):
    o=i*e_shentsize
    (sh_name,sh_type,sh_flags,sh_addr,sh_offset,sh_size,sh_link,sh_info,sh_addralign,sh_entsize)=struct.unpack_from("<IIQQQQIIQQ",sh_raw,o)
    shdrs.append(dict(i=i,nameoff=sh_name,type=sh_type,flags=sh_flags,addr=sh_addr,off=sh_offset,size=sh_size,link=sh_link,info=sh_info,align=sh_addralign,entsize=sh_entsize))

# section name table
shstr = b""
for s in shdrs:
    if s["nameoff"]==0 and s["addr"]==0 and s["size"]!=0:
        pass
if e_shstrndx < e_shnum:
    st = shdrs[e_shstrndx]
    with open(SO,"rb") as f:
        f.seek(st["off"]); shstr = f.read(st["size"])
def sname(o):
    if o>=len(shstr): return "?"
    e = shstr.find(b"\0",o)
    return shstr[o:e].decode("utf-8","replace")

lines=[]
lines.append("# libil2cpp.so ELF 段/节映射表")
lines.append("")
lines.append("文件: `%s`" % SO)
lines.append("大小: %d bytes (%#x)" % (os.path.getsize(SO), os.path.getsize(SO)))
lines.append("")
lines.append("ELF64 little-endian, e_entry=%#x, e_phnum=%d, e_shnum=%d, e_shstrndx=%d" % (e_entry,e_phnum,e_shnum,e_shstrndx))
lines.append("")
lines.append("## 程序头 (PT_LOAD)")
lines.append("")
lines.append("| idx | type | flags | file_off | vaddr | filesz | memsz | vaddr-off (delta) |")
lines.append("|----|----|----|----|----|----|----|----|")
for p in phdrs:
    tn = PT.get(p["type"],hex(p["type"]))
    fl = ("R" if p["flags"]&4 else "-")+("W" if p["flags"]&2 else "-")+("X" if p["flags"]&1 else "-")
    d = p["vaddr"]-p["off"]
    lines.append("| %d | %s | %s | %#x | %#x | %#x | %#x | %#x |" % (p["i"],tn,fl,p["off"],p["vaddr"],p["filesz"],p["memsz"],d))
lines.append("")
lines.append("## 节头 (前 40 / 全部 %d)" % e_shnum)
lines.append("")
lines.append("| idx | name | type | flags | addr(VA) | off | size | delta(addr-off) |")
lines.append("|----|----|----|----|----|----|----|----|")
for s in shdrs:
    n = sname(s["nameoff"])
    tn = SHT.get(s["type"],hex(s["type"]))
    fl = "%#x" % s["flags"]
    d = s["addr"]-s["off"] if s["type"]!=8 else 0
    lines.append("| %d | %s | %s | %s | %#x | %#x | %#x | %#x |" % (s["i"],n,tn,fl,s["addr"],s["off"],s["size"],d))
lines.append("")

# delta analysis for text
lines.append("## delta 实测")
lines.append("")
for s in shdrs:
    n=sname(s["nameoff"])
    if n in (".text",".rodata",".data",".data.rel.ro",".bss",".dynsym",".dynstr",".rela.dyn",".gcc_except_table"):
        lines.append("- `%s`: VA=%#x  off=%#x  delta=%#x" % (n,s["addr"],s["off"],s["addr"]-s["off"]))
lines.append("")

with open(OUT,"w",encoding="utf-8") as f:
    f.write("\n".join(lines)+"\n")
print("wrote", OUT)

# also a machine-readable dump for later scripts
json.dump({"e_entry":e_entry,"phdrs":phdrs,
           "shdrs":[{**s,"name":sname(s["nameoff"])} for s in shdrs]},
          open(OUT.replace("il2cpp_sections.md","_elf_meta.json"),"w"), indent=0)
print("--- summary ---")
for p in phdrs:
    if p["type"]==1:
        fl = ("R" if p["flags"]&4 else "-")+("W" if p["flags"]&2 else "-")+("X" if p["flags"]&1 else "-")
        print("PT_LOAD %s off=%#010x vaddr=%#010x filesz=%#010x delta=%#x" % (fl,p["off"],p["vaddr"],p["filesz"],p["vaddr"]-p["off"]))
