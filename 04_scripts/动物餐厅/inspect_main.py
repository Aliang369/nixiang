import sys,zipfile,subprocess
from pathlib import Path
root=Path(__file__).resolve().parents[2]; work=root/'07_patched/动物餐厅/dwct1242'
with zipfile.ZipFile(root/'02_apks/动物餐厅/动物餐厅_12.42_taptap_official_from_device.apk') as z:
 names=[n for n in z.namelist() if n.endswith('.jsc')];print('\n'.join(names))
 for n in names:
  if n.endswith('main/index.jsc') or n=='assets/src/project.jsc':
   p=work/'main.original.jsc';p.write_bytes(z.read(n));print('MAIN',n)
