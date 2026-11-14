import sys,zipfile,hashlib,subprocess,json
from pathlib import Path
r=Path(__file__).resolve().parents[2];w=r/'07_patched/动物餐厅/dwct1242';out=w/'login_diag_compare';out.mkdir(exist_ok=True)
for label,name in [('release','动物餐厅_12.42_starry-local-releaseSigned.apk'),('diag','动物餐厅_12.42_login-observation-diag.apk')]:
 with zipfile.ZipFile(r/'08_output/动物餐厅'/name) as z:
  for tag,entry in [('main','assets/assets/main/index.jsc'),('hotmain','assets/dwct1242/main.hot.patched.jsc')]:
   p=out/(label+'.'+tag+'.jsc');p.write_bytes(z.read(entry));subprocess.run(['node',str(r/'04_scripts/动物餐厅/inspect_1242.js'),str(p),str(p.with_suffix('.js'))],check=True,stdout=subprocess.DEVNULL)
result={}
for tag in ['main','hotmain']:
 a=(out/('release.'+tag+'.js')).read_text(encoding='utf8');b=(out/('diag.'+tag+'.js')).read_text(encoding='utf8')
 lines=b.splitlines(keepends=True);diag=[x for x in lines if x.startswith('console.log("[DwctLoginDiag]')];stripped=''.join(x for x in lines if not x.startswith('console.log("[DwctLoginDiag]'))
 assert [x for x in a.splitlines() if x.strip()]==[x for x in stripped.splitlines() if x.strip()],tag
 assert len(diag)==4,(tag,len(diag))
 result[tag]={'removedLogLines':len(diag),'remainingNonblankLinesIdentical':True,'diagnosticData':['source SHA256','signature identity class','transport/status','dialog opened'],'requestMutation':False,'responseMutation':False}
print(json.dumps(result,indent=2));(w/'login_diag_equivalence.json').write_text(json.dumps(result,indent=2),encoding='utf8')
