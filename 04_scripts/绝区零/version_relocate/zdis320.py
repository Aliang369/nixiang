import sys
from capstone import *
data=open('v320/lib/arm64-v8a/libil2cpp.so','rb').read()
start=int(sys.argv[1],16); count=int(sys.argv[2])
md=Cs(CS_ARCH_ARM64,CS_MODE_ARM)
code=data[start:start+count*4]
for i in md.disasm(code,start):
    print('%#010x (rva %#x): %s %s'%(i.address,i.address+0x4000,i.mnemonic,i.op_str))
