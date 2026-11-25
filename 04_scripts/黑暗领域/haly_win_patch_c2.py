
import os, re, sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else r'E:\nixiang\13_scratch\haly_stage\c2smali')
ORIG_MD5 = 'd639fb03a75167fbe77bc75e2b4a4304'

def inject_activity(path: Path, tag: str, sign_spoof: bool = False):
    if not path.is_file():
        print(f'  skip {tag} (missing)')
        return
    s = path.read_text(encoding='utf-8')
    if 'MenuService;->start' not in s:
        m = re.search(r"(\.method[^\n]*\bonCreate\(Landroid/os/Bundle;\)V\n)([\s\S]*?)(\n\.end method)", s)
        if not m:
            print(f'  WARN {tag}.onCreate not found')
        else:
            head, body, end = m.group(1), m.group(2), m.group(3)
            def bump(mm):
                kind = mm.group(1); n = int(mm.group(2))
                need = 3 if 'registers' in kind else 1
                return kind + str(max(n, need))
            body2, n = re.subn(r"(\.(?:locals|registers) )(\d+)", bump, body, count=1)
            if n == 0:
                print(f'  WARN {tag}.onCreate missing .locals/.registers')
            if sign_spoof and 'SignSpoof;->install' not in body2:
                early = '\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n'
                if 'invoke-super' in body2:
                    body2 = body2.replace('invoke-super', early + '    invoke-super', 1)
                else:
                    body2 = early + body2
            rets = list(re.finditer(r"(\n\s*)return-void\b", body2))
            if rets:
                mm = rets[-1]
                body2 = (body2[:mm.start()] + mm.group(1)
                         + 'invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V'
                         + mm.group(0) + body2[mm.end():])
            else:
                body2 = body2 + '\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V\n'
            s = s[:m.start()] + head + body2 + end + s[m.end():]
            print(f'  hook {tag}.onCreate')
    if 'MenuService;->notifyHostLeaving' not in s:
        s += '''
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
'''
        print(f'  hook {tag}.onUserLeaveHint')
    if 'MenuService;->notifyHostResumed' not in s:
        if not re.search(r"\.method[^\n]*\bonResume\(\)V", s):
            s += '''
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
'''
            print(f'  hook {tag}.onResume (new)')
        else:
            m2 = re.search(r"(\.method[^\n]*\bonResume\(\)V\n)([\s\S]*?)(\n\.end method)", s)
            if m2 and 'notifyHostResumed' not in m2.group(0):
                head2, body_r, end2 = m2.group(1), m2.group(2), m2.group(3)
                rets = list(re.finditer(r"(\n\s*)return-void\b", body_r))
                if rets:
                    mm = rets[-1]
                    body_r = (body_r[:mm.start()] + mm.group(1)
                              + 'invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V'
                              + mm.group(0) + body_r[mm.end():])
                else:
                    body_r = body_r + '\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n'
                s = s[:m2.start()] + head2 + body_r + end2 + s[m2.end():]
                print(f'  hook {tag}.onResume (inject)')
    path.write_text(s, encoding='utf-8')

TAP_GETSIGN = '''.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "%s"

    return-object v0
.end method''' % ORIG_MD5

taps = list(root.glob('com/taptap/sdk/kit/internal/utils/TapSignKit.smali'))
if not taps:
    print('  WARN TapSignKit.smali not found')
for tap in taps:
    ts = tap.read_text(encoding='utf-8')
    if ORIG_MD5 in ts and 'starry_sign_bypass' in ts:
        print('  TapSignKit already patched'); continue
    ts2, n = re.subn(r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method", TAP_GETSIGN, ts, count=1)
    if n != 1:
        raise SystemExit('TapSignKit patch failed: %s' % tap)
    tap.write_text(ts2, encoding='utf-8')
    print('  TapSignKit.getSign -> %s' % ORIG_MD5)

clu = next(root.glob('com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali'), None)
if clu is None:
    clu = next(root.glob('com/taptap/sdk/login/internal/util/ClientLoginUtils.smali'), None)
if clu is None:
    print('  WARN ClientLoginUtils.smali missing')
else:
    s = clu.read_text(encoding='utf-8')
    if 'force WebLogin' not in s:
        new_icl = '''.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method'''
        s2, n = re.subn(r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method", new_icl, s, count=1)
        if n != 1:
            new_icl_old = '''.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method'''
            s2, n = re.subn(r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method", new_icl_old, s, count=1)
        if n != 1:
            raise SystemExit('ClientLoginUtils.isClientLogin patch failed')
        clu.write_text(s2, encoding='utf-8')
        print('  ClientLoginUtils -> force WebLogin')
    else:
        print('  ClientLoginUtils already patched')

inject_activity(root / 'com/cocos/game/AppActivity.smali', 'AppActivity', sign_spoof=False)
inject_activity(root / 'com/cocos/game/PrivacyGateActivity.smali', 'PrivacyGateActivity')
priv = root / 'com/cocos/game/PrivacyActivity.smali'
if priv.is_file():
    inject_activity(priv, 'PrivacyActivity')
else:
    print('  WARN PrivacyActivity.smali missing')
print('PATCH_C2_DONE')
