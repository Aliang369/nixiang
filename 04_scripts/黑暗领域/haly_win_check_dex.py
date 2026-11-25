
import re
from pathlib import Path
import sys
dex = Path(sys.argv[1]).read_bytes()
for n in (b'com/starry/magic/cocos/CocosJsBridge', b'evalOnGLThread', b'com/starry/magic/bridge/ModBridge'):
    print(n.decode(), '=', dex.count(n))
java = Path(sys.argv[2]).read_text(encoding='utf-8')
b64 = re.search(r'FAB_ICON_WEBP_B64\s*=\s*"([A-Za-z0-9+/=]+)"', java).group(1)
print('fab_prefix_in_dex =', b64[:192].encode() in dex)
print('DEX_CHECK_DONE')
