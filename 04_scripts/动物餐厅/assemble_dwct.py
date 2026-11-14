import sys,zipfile,json,hashlib,subprocess,os,struct
from pathlib import Path
work,root=map(Path,sys.argv[1:3]);merge=sys.argv[3];base=root/'02_apks/动物餐厅/动物餐厅_12.42_taptap_official_from_device.apk';star=root/'mod菜单/starry-mod-menu'
java=str(Path(os.environ['NIXIANG_JAVA17'])/'bin/java.exe') if os.name=='nt' else 'java'
report=json.loads((work/'smali_patch_report.json').read_text());dexnames={merge}
for d in report['dexRoots']:dexnames.add(d.removesuffix('_smali')+'.dex')
for name in sorted(dexnames):
 subprocess.run([java,'-jar',str(root/'01_tools/smali/smali-fat.jar'),'a',str(work/(name.removesuffix('.dex')+'_smali')),'-o',str(work/name)],check=True)
 b=(work/name).read_bytes();methods=struct.unpack_from('<I',b,0x58)[0];assert methods<65536,(name,methods);print('DEX',name,methods)
extras={name:(work/name).read_bytes() for name in dexnames};extras['AndroidManifest.xml']=(work/'AndroidManifest.xml').read_bytes();extras['assets/assets/Script/index.jsc']=(work/'Script.patched.jsc').read_bytes();extras['assets/assets/main/index.jsc']=(work/'main.patched.jsc').read_bytes()
for n in ['Script','main']:extras['assets/dwct1242/'+n+'.hot.patched.jsc']=(work/(n+'.hot.patched.jsc')).read_bytes()
for abi in ['arm64-v8a','x86_64','armeabi-v7a']:
 for n in ['libstarry_magic.so','libc++_shared.so']:extras[f'lib/{abi}/{n}']=(star/'libs_dwct1242'/abi/n).read_bytes()
with zipfile.ZipFile(base) as zi,zipfile.ZipFile(work/'unsigned.apk','w') as zo:
 for item in zi.infolist():
  if item.filename.startswith('META-INF/'):continue
  if item.filename in extras:continue
  zo.writestr(item,zi.read(item))
 for n,b in extras.items():zo.writestr(n,b,compress_type=zipfile.ZIP_DEFLATED)
with zipfile.ZipFile(base) as zi,zipfile.ZipFile(work/'unsigned.apk') as zo:
 assert len(zo.namelist())==len(set(zo.namelist()))
 for n in zi.namelist():
  if n.startswith('META-INF/') or n in extras:continue
  assert zi.read(n)==zo.read(n),n
 assert zo.getinfo('resources.arsc').compress_type==zipfile.ZIP_STORED
 assert set(n for n in zi.namelist() if n.startswith('classes') and n.endswith('.dex'))==set(n for n in zo.namelist() if n.startswith('classes') and n.endswith('.dex'))
rec={'diagnostic':os.environ.get('DWCT_LOGIN_DIAG')=='1','baseSha256':hashlib.sha256(base.read_bytes()).hexdigest(),'changed':{n:hashlib.sha256(b).hexdigest() for n,b in extras.items()},'mergeDex':merge,'preservedOtherEntries':True}
# Dirty-source fingerprint binds every Java input and private source/script input, beyond commit-only menu stamp.
inputs=list((star/'app/src/main/java').rglob('*.java'))+list((root/'04_scripts/动物餐厅').glob('*'))+[star/'app/src/main/jni/Games/DwctMod.cpp',star/'app/src/main/jni/Games/Dwct1242Entry.cpp',star/'app/src/main/jni/Android_dwct1242.mk']
rec['sourceInputs']={str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs if p.is_file()};(work/'assembly_report.json').write_text(json.dumps(rec,ensure_ascii=False,indent=2),encoding='utf8');print('ASSEMBLY_PASS',len(extras))
