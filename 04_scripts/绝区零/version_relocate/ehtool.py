import struct, bisect
from elftools.elf.elffile import ELFFile

class FnTable:
    def __init__(self, path, cache=None):
        import os, pickle
        self.path=path
        if cache and os.path.exists(cache):
            self.starts=pickle.load(open(cache,'rb'))
            self.data=open(path,'rb').read()
            return
        f=open(path,'rb'); self.path=path
        e=ELFFile(f); self.elf=e
        sec=e.get_section_by_name('.eh_frame_hdr')
        data=sec.data()
        ver,ehptr_enc,fde_cnt_enc,tbl_enc = struct.unpack_from('<BBBB',data,0)
        def read_enc(buf,off,enc):
            # DW_EH_PE
            fmt=enc&0x0f; app=enc&0x70
            if fmt==0x03: v=struct.unpack_from('<i',buf,off)[0]; n=4
            elif fmt==0x0b: v=struct.unpack_from('<i',buf,off)[0]; n=4
            elif fmt==0x04: v=struct.unpack_from('<I',buf,off)[0]; n=4
            elif fmt==0x0a: v=struct.unpack_from('<h',buf,off)[0]; n=2
            elif fmt==0x02: v=struct.unpack_from('<h',buf,off)[0]; n=2
            elif fmt==0x01: v=struct.unpack_from('<b',buf,off)[0]; n=1
            elif fmt==0x00: v=0; n=0
            else: raise ValueError('enc %#x'%enc)
            return v,n
        off=4
        ehptr,n=read_enc(data,off,ehptr_enc); off+=n
        cnt,n=read_enc(data,off,fde_cnt_enc); off+=n
        # table encoding: datarel sdata4 normally
        starts=[]
        base=0  # datarel base = start of .eh_frame_hdr section address
        for i in range(cnt):
            a,n1=read_enc(data,off,tbl_enc); off+=n1
            o,n2=read_enc(data,off,tbl_enc & 0x0f | (tbl_enc & 0x70)); off+=n2
            starts.append(a)
        starts=sorted(set(starts))
        self.starts=starts
        self.data=open(path,'rb').read()
    def func_at(self,addr):
        i=bisect.bisect_right(self.starts,addr)-1
        if i<0: return None
        return self.starts[i], (self.starts[i+1] if i+1<len(self.starts) else None)
