
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 无壳包 · Manifest 文本改造（单步，可重复执行）
输入: apktool 解出的 dec/AndroidManifest.xml
输出: 就地改写后的 AndroidManifest.xml + 改动日志到 stdout
依据: 10_docs/动物大联盟/脱壳与无壳包方案_1.0.25_20261105.md §3
"""
import re, sys, json

REAL = {
 0:"com.bytedance.sdk.openadsdk.multipro.TTMultiProvider",
 1:"com.bytedance.sdk.openadsdk.TTFileProvider",
 2:"com.qq.e.comm.GDTFileProvider",
 3:"com.anythink.china.common.ApkFileProvider",
 4:"com.kwad.sdk.api.proxy.app.AdSdkFileProvider",
 5:"com.bytedance.sdk.openadsdk.stub.server.DownloaderServerManager",
 6:"com.bytedance.sdk.openadsdk.stub.server.MainServerManager",
 7:"com.bytedance.pangle.provider.MainProcessProviderProxy",
 8:"com.bytedance.pangle.FileProvider",
 9:"com.bytedance.android.openliveplugin.process.server.LiveServerManager",
 10:"com.byted.live.lite.ServerManager_bytelive",
 11:"com.byted.live.lite.ServerManager_push",
 12:"com.tapsdk.tapad.internal.TapADFileProvider",
 13:"com.tapsdk.tapad.internal.download.OkDownloadProvider",
 14:"com.tds.common.provider.CurrentConfigProvider",
 15:"com.taptap.services.update.TapUpdateFileProvider",
 16:"com.taptap.services.update.download.OkDownloadProvider",
 17:"com.tramini.plugin.api.TraminiContentProvider",
}
APP = "com.bingo.cocos.bridge.BingoApplication"
ACT = "org.cocos2dx.javascript.AppActivity"

p = sys.argv[1]
src = open(p, encoding="utf-8").read()
log = []
t = src

# 1) application@name
old = 'android:name="dev.mars.secure.ProxyApplication"'
assert t.count(old) == 1, "ProxyApplication 出现 %d 次" % t.count(old)
t = t.replace(old, 'android:name="%s"' % APP); log.append("application@name -> %s" % APP)

# 2) 18 provider @name -> apksecure.real 真类名
for i in range(18):
    stub = "dev.mars.secure.StubProvider$S%d" % i
    o = 'android:name="%s"' % stub
    assert t.count(o) == 1, "%s 出现 %d 次" % (stub, t.count(o))
    t = t.replace(o, 'android:name="%s"' % REAL[i])
log.append("18 provider @name -> apksecure.real 真类名")

# 3) 删 provider 内 apksecure.real meta-data 子项
before = t
t, n = re.subn(r'[ \t]*<meta-data android:name="apksecure\.real"[^>]*/>\n', '', t)
assert n == 18, "删 apksecure.real meta-data 条数=%d（期望 18）" % n
log.append("删 apksecure.real meta-data x%d" % n)

# 4) 删 meta-data APKSECURE_REAL_LAUNCHER
t, n = re.subn(r'[ \t]*<meta-data android:name="APKSECURE_REAL_LAUNCHER"[^>]*/>\n', '', t)
assert n == 1, "APKSECURE_REAL_LAUNCHER 条数=%d" % n
log.append("删 meta-data APKSECURE_REAL_LAUNCHER x1（APP_NAME 保留）")
assert 'android:name="APP_NAME"' in t and ('android:value="%s"' % APP) in t, "APP_NAME 丢失"

# 5) 删 StubActivity 整块
m = re.search(r'[ \t]*<activity [^>]*android:name="dev\.mars\.secure\.StubActivity"[^>]*>.*?</activity>\n',
              t, re.S)
assert m, "找不到 StubActivity 块"
t = t[:m.start()] + t[m.end():]; log.append("删 StubActivity 整块（%d 字节）" % (m.end()-m.start()))

# 6) AppActivity 的 MAIN filter 补 LAUNCHER category
am = re.search(r'(<activity [^>]*android:name="%s"[^>]*>)(.*?)(</activity>)' % re.escape(ACT), t, re.S)
assert am, "找不到 AppActivity"
body = am.group(2)
mf = re.search(r'([ \t]*)<intent-filter>\s*\n([ \t]*)<action android:name="android\.intent\.action\.MAIN"/>\s*\n([ \t]*)</intent-filter>',
               body)
assert mf, "AppActivity 的 MAIN-only filter 形态不符"
ind_c = mf.group(3)
newf = ('%s<intent-filter>\n%s<action android:name="android.intent.action.MAIN"/>\n'
        '%s<category android:name="android.intent.category.LAUNCHER"/>\n%s</intent-filter>'
        % (mf.group(1), mf.group(2), ind_c, mf.group(3)))
body2 = body[:mf.start()] + newf + body[mf.end():]
t = t[:am.start(2)] + body2 + t[am.end(2):]
log.append("AppActivity MAIN filter 补 LAUNCHER")

# 7) 兜底断言: 不得残留壳标识
for bad in ["dev.mars.secure", "apksecure.real", "APKSECURE_REAL_LAUNCHER", "StubProvider",
            "StubActivity", "ProxyApplication"]:
    assert bad not in t, "残留壳标识: %s" % bad
log.append("残留壳标识 0 命中")

open(p, "w", encoding="utf-8").write(t)
print(json.dumps({"ok": True, "file": p, "steps": log}, ensure_ascii=False, indent=2))
