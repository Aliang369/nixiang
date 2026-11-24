
import zipfile, random, hashlib, sys
out="/Users/aliang/Documents/nixiang/08_output/\u6211\u7684\u52c7\u8005/\u6211\u7684\u52c7\u8005_7.6.5_starry.apk"
base="/Users/aliang/Documents/nixiang/02_apks/\u6211\u7684\u52c7\u8005/\u6211\u7684\u52c7\u8005_7.6.5_base.apk"
zo=zipfile.ZipFile(out); zb=zipfile.ZipFile(base)
io={i.filename:i for i in zo.infolist()}; ib={i.filename:i for i in zb.infolist()}
injected={"classes2.dex","AndroidManifest.xml","lib/arm64-v8a/libstarry_magic.so","lib/arm64-v8a/libc++_shared.so","META-INF/STARRY.RSA","META-INF/STARRY.SF"}
common=sorted(set(ib)&set(io)-injected)
random.seed(765)
sample=random.sample(common,200)
bad=[]; full=0
for n in sample:
    a=io[n]; b=ib[n]
    if a.CRC!=b.CRC or a.file_size!=b.file_size:
        bad.append((n,a.CRC,b.CRC,a.file_size,b.file_size))
    else:
        full+=1
print("sample",len(sample),"crc+size match",full,"mismatch",len(bad))
for x in bad[:10]: print("  MISMATCH",x)
# also full count check of CRC mismatches across ALL common entries (cheap: central dir only)
allbad=[n for n in common if io[n].CRC!=ib[n].CRC or io[n].file_size!=ib[n].file_size]
print("ALL common entries",len(common),"crc/size mismatches:",len(allbad))
for n in allbad[:15]: print("  DIFF",n, io[n].CRC, ib[n].CRC, io[n].file_size, ib[n].file_size)
