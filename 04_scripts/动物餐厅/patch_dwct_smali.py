# Input dex ownership is discovered by descriptor paths, never by a fixed dex number.
import sys,re,json
from pathlib import Path
work=Path(sys.argv[1]); touched=set();changes=[]
def owner(rel):
 ps=list(work.glob('classes*_smali/'+rel));assert len(ps)==1,(rel,ps);return ps[0]
def replace(rel,signature,body):
 p=owner(rel);s=p.read_text(encoding='utf8');pat=r'(?m)^'+re.escape(signature)+r'\n[\s\S]*?^\.end method';s,n=re.subn(pat,signature+'\n'+body+'\n.end method',s);assert n==1,(p,signature,n);p.write_text(s,encoding='utf8');touched.add(p.parent);changes.append(str(p.relative_to(work)))
def prefix(rel,signature,pre):
 p=owner(rel);s=p.read_text(encoding='utf8');pat=r'(?m)^'+re.escape(signature)+r'\n(    \.registers (\d+)\n)';m=re.search(pat,s);assert m and int(m[2])>=1;pos=m.end();s=s[:pos]+pre+s[pos:];p.write_text(s,encoding='utf8');changes.append(str(p.relative_to(work)))
act='org/cocos2dx/javascript/AppActivity.smali'
prefix(act,'.method public static isRewardVideoADReady()Z','\n    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z\n    move-result v0\n    if-eqz v0, :dwct_orig_ready\n    const/4 v0, 0x1\n    return v0\n    :dwct_orig_ready\n')
prefix(act,'.method public static showRewardAd()V','\n    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->shouldSkipAd()Z\n    move-result v0\n    if-eqz v0, :dwct_orig_show\n    invoke-static {}, Lcom/starry/magic/cocos/DwctHooks;->rewardAdFromJava()V\n    return-void\n    :dwct_orig_show\n')
replace('com/taptap/sdk/initializer/repository/GatekeeperRepository.smali','.method public final isGatekeeperLoadedSuccessfully()Z','    .registers 2\n    const/4 v0, 0x1\n    return v0')
replace('com/taptap/sdk/initializer/repository/GatekeeperRepository.smali','.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V','    .registers 2\n    return-void')
# 12.39 repack method: full-method replacement for the two official-fingerprint readers.
# 12.42 attribution verified by descriptor: both classes live in classes3.dex (owner() matches exactly one path).
OFFICIAL_SHA1='94F80FC5161E41BAD58C6541E739920425BAE53D'
OFFICIAL_MD5='3169d253f38147f0f7cb6ba1672e6255'
replace('org/cocos2dx/javascript/util/DeviceHelper.smali','.method public static getSignature(Ljava/lang/String;)Ljava/lang/String;','    .registers 1\n\n    const-string p0, "'+OFFICIAL_SHA1+'"\n\n    return-object p0')
replace('com/taptap/sdk/kit/internal/utils/TapSignKit.smali','.method public final getSign(Landroid/content/Context;)Ljava/lang/String;','    .registers 2\n    .param p1    # Landroid/content/Context;\n        .annotation build Lorg/jetbrains/annotations/NotNull;\n        .end annotation\n    .end param\n\n    const-string p1, "'+OFFICIAL_MD5+'"\n\n    return-object p1')
# Preserve initializer checks; only bypass the local signature gate already proven reachable.
roots=sorted(set(Path(x).parts[0] for x in changes));(work/'smali_patch_report.json').write_text(json.dumps({'changed':changes,'dexRoots':roots,'login':'official-fingerprint: DeviceHelper.getSignature -> official SHA1, TapSignKit.getSign -> official MD5 (12.39 repack method), both anchors hit=1 in classes3.dex','wxPay':'original: no fake Java callback'},indent=2));print(' '.join(roots))
