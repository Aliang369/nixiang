
import re
d=open("/Users/aliang/Documents/nixiang/mod\u83dc\u5355/starry-mod-menu/app/src/libs_wdyz/arm64-v8a/libstarry_magic.so","rb").read()
for pre in [b"Toggle_",b"Slider_",b"Button_",b"Switch_"]:
    i=0; out=[]
    while True:
        i=d.find(pre,i)
        if i<0: break
        e=d.index(b"\x00",i)
        out.append(d[i:e].decode('utf-8','replace')); i=e+1
    print(pre.decode(), len(out), out)
