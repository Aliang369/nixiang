# -*- coding: utf-8 -*-
"""用 .eh_frame_hdr 的 FDE 起始地址表判定调用点归属函数（只读）"""
import struct, json, bisect
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
sec={s["name"]:s for s in ELF["sh"]}
h=sec[".eh_frame_hdr"]; blob=open(SO,"rb").read()
o=h["off"]; va=h["addr"]
assert blob[o]==1
eh_enc,cnt_enc,tab_enc=blob[o+1],blob[o+2],blob[o+3]
print("eh_frame_ptr_enc=%#x fde_count_enc=%#x table_enc=%#x"%(eh_enc,cnt_enc,tab_enc))
p=o+4
if eh_enc==0x1b: p+=4
elif eh_enc==0x03: p+=4
cnt=struct.unpack_from("<I",blob,p)[0]; p+=4
tab_va=va+(p-o)
print("fde_count=%d table_va=%#x (datarel base=%#x?)"%(cnt,tab_va,va))
starts=[]
for i in range(cnt):
    a=struct.unpack_from("<i",blob,p+i*8)[0]
    # table_enc 0x3b = datarel|sdata4 -> base = eh_frame_hdr addr
    starts.append(a+va)
starts.sort()
print("first 3:",["%#x"%s for s in starts[:3]],"last:",["%#x"%s for s in starts[-2:]])
def near(x,k=3):
    i=bisect.bisect_right(starts,x)-1
    print("  around %#x:"%x)
    for j in range(max(0,i-k),min(len(starts),i+k+2)):
        mark=" <==" if starts[j]<=x<(starts[j+1] if j+1<len(starts) else 1<<64) else ""
        print("     start %#x%s"%(starts[j],mark))
for t in (0x207F798,0x208AD70,0x208BD78,0x208E018,0x207F640,0x207DCCC,0x207F04C):
    near(t)
i=bisect.bisect_right(starts,0x207F798)-1
print("\nENTRY function: start=%#x  next_start=%#x  size=%#x"%(starts[i],starts[i+1],starts[i+1]-starts[i]))
