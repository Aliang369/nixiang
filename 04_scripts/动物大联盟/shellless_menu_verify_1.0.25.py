
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""动物大联盟 1.0.25 无壳包 + 星光菜单 · 静态自检（C1–C11 沿用无壳口径，C12 改为菜单注入点断言）
用法: python3 shellless_menu_verify_1.0.25.py <signed.apk>
产出: 04_scripts/动物大联盟/shellless_menu_verify.json / .txt
口径: 基座＝shellless2（已实测可进 Cocos），唯一新增变量＝菜单 dex(classes5) + 菜单 so + provider 注入点。
"""
import sys, os, re, json, zipfile, hashlib, subprocess, tempfile, struct

APK = os.path.abspath(sys.argv[1])
ROOT = "/Users/aliang/Documents/nixiang"
OFFICIAL = os.path.join(ROOT, "02_apks/动物大联盟/动物大联盟_1.0.25_official.apk")
BT = os.path.expanduser("~/Library/Android/sdk/build-tools/36.0.0")
SO_SHA = "dd3a046291b4d4988a32ae20d7b98ebf5d46e992c70f0fa7168934c627893b44"
APP = "com.bingo.cocos.bridge.BingoApplication"
ACT = "org.cocos2dx.javascript.AppActivity"
PKG = "com.fightingstars.games.animal"
PROVIDER = "com.starry.magic.StarryInitProvider"
MSVC = "com.starry.magic.menu.MenuService"
MENU_SO = "lib/arm64-v8a/libstarry_magic.so"
CXX_SO = "lib/arm64-v8a/libc++_shared.so"
MENU_SO_SRC = os.path.join(ROOT, "mod菜单/starry-mod-menu/libs_dwdlm/env/arm64-v8a/libstarry_magic.so")
REAL = {0:"com.bytedance.sdk.openadsdk.multipro.TTMultiProvider",1:"com.bytedance.sdk.openadsdk.TTFileProvider",
 2:"com.qq.e.comm.GDTFileProvider",3:"com.anythink.china.common.ApkFileProvider",
 4:"com.kwad.sdk.api.proxy.app.AdSdkFileProvider",5:"com.bytedance.sdk.openadsdk.stub.server.DownloaderServerManager",
 6:"com.bytedance.sdk.openadsdk.stub.server.MainServerManager",7:"com.bytedance.pangle.provider.MainProcessProviderProxy",
 8:"com.bytedance.pangle.FileProvider",9:"com.bytedance.android.openliveplugin.process.server.LiveServerManager",
 10:"com.byted.live.lite.ServerManager_bytelive",11:"com.byted.live.lite.ServerManager_push",
 12:"com.tapsdk.tapad.internal.TapADFileProvider",13:"com.tapsdk.tapad.internal.download.OkDownloadProvider",
 14:"com.tds.common.provider.CurrentConfigProvider",15:"com.taptap.services.update.TapUpdateFileProvider",
 16:"com.taptap.services.update.download.OkDownloadProvider",17:"com.tramini.plugin.api.TraminiContentProvider"}

def sha(b): return hashlib.sha256(b).hexdigest()
def run(cmd):
    p = subprocess.run(cmd, capture_output=True)
    return p.returncode, p.stdout.decode("utf-8","replace"), p.stderr.decode("utf-8","replace")

zo = zipfile.ZipFile(APK); zi = zipfile.ZipFile(OFFICIAL)
names = zo.namelist()

tmp = tempfile.mkdtemp(prefix="sl_verify_")
dexpaths = []
n = 1
while True:
    nm = "classes.dex" if n == 1 else "classes%d.dex" % n
    if nm not in names: break
    p = os.path.join(tmp, nm); open(p,"wb").write(zo.read(nm)); dexpaths.append((nm,p)); n += 1

def dex_parse(p):
    d = open(p,"rb").read()
    g = lambda o: struct.unpack_from("<I", d, o)[0]
    cd_size, cd_off = g(0x60), g(0x64)
    ss, so, ts, to = g(0x38), g(0x3c), g(0x40), g(0x44)
    cache = {}
    def str_at(i):
        if i in cache: return cache[i]
        o = struct.unpack_from("<I", d, so+4*i)[0]; j = o
        while d[j] & 0x80: j += 1
        j += 1; k = d.index(b"\x00", j)
        s = d[j:k].decode("utf-8","replace"); cache[i] = s; return s
    def type_at(i): return str_at(struct.unpack_from("<I", d, to+4*i)[0])
    classes = [type_at(struct.unpack_from("<I", d, cd_off+32*k)[0]) for k in range(cd_size)]
    return d, classes

dexinfo = []
for nm, p in dexpaths:
    d, cls = dex_parse(p)
    g = lambda o: struct.unpack_from("<I", d, o)[0]
    dexinfo.append({"entry": nm, "size": len(d), "zip_size": zo.getinfo(nm).file_size,
                    "magic": d[:8].decode("latin1"), "file_size_hdr": g(0x20),
                    "class_defs_size": g(0x60), "n_classes_parsed": len(cls),
                    "sha256": sha(d), "classes": cls})

def xmltree(apk):
    rc, out, err = run([os.path.join(BT,"aapt2"),"dump","xmltree",apk,"--file","AndroidManifest.xml"])
    assert rc == 0, "aapt2 dump xmltree 失败 rc=%d" % rc
    nodes, stack = [], []
    for line in out.splitlines():
        if line.startswith("warn:") or not line.strip(): continue
        m = re.match(r"^(\s*)E: (\S+)", line)
        if m:
            depth = len(m.group(1)) // 2
            node = {"tag": m.group(2), "attrs": {}, "depth": depth, "children": [], "parent": None}
            while stack and stack[-1]["depth"] >= depth: stack.pop()
            if stack: node["parent"] = stack[-1]; stack[-1]["children"].append(node)
            nodes.append(node); stack.append(node); continue
        m = re.match(r"^(\s*)A: (\S+?)=(.*)$", line)
        if m and stack:
            key = re.sub(r"\(0x[0-9a-fA-F]+\)$", "", m.group(2))
            key = key.split(":")[-1]
            raw = m.group(3)
            vm = re.search(r'"([^"]*)"', raw)
            stack[-1]["attrs"][key] = vm.group(1) if vm else raw.strip()
    return nodes

res = []
def chk(cid, name, fn):
    try:
        res.append({"id": cid, "name": name, "pass": True, "detail": fn()})
    except AssertionError as e:
        res.append({"id": cid, "name": name, "pass": False, "detail": str(e) or "assert 失败（无消息）"})

NEWTREE = xmltree(APK); OLDTREE = xmltree(OFFICIAL)

# C1
# SL2 口径（2026-11-05 起）：允许且仅允许 dev/mars/secure/ShellGate 以空实现存在
#   —— 官方该类 require() 为 native（libsecret.so），无壳包已删该 so，须由本仓补丁类顶上。
SHELLGATE_ALLOW = {"Ldev/mars/secure/ShellGate;"}

def c1():
    t = zo.read("AndroidManifest.xml").decode("utf-16-le","ignore")
    assert "dev.mars.secure" not in t, "Manifest 仍含 dev.mars.secure"
    assert "StubProvider" not in t and "StubActivity" not in t, "Manifest 仍含 Stub* 壳组件"
    allowed, stray = [], []
    for s in dexinfo:
        for c in s["classes"]:
            if not c.startswith("Ldev/mars/secure"): continue
            (allowed if c in SHELLGATE_ALLOW else stray).append((s["entry"], c))
    assert not stray, "真实 dex 内仍定义非白名单壳类: %s" % stray[:5]
    assert allowed, "缺 dev/mars/secure/ShellGate 空实现（冷启会 NoClassDefFoundError）"
    hit = []
    for nm, p in dexpaths:
        c = open(p,"rb").read().count(b"dev/mars/secure")
        if c: hit.append("%s x%d" % (nm, c))
    return ("Manifest 无 dev.mars.secure/Stub*；壳类仅白名单 %s 存在（%s）；非白名单壳类 0 个；"
            "残留 dev/mars/secure 字面量: %s" % (sorted(SHELLGATE_ALLOW), allowed, ", ".join(hit) or "无"))
chk("C1", "Manifest 无壳引用 + 仅 ShellGate 空实现（SL2 口径）", c1)

# C2
def c2():
    app = [x for x in NEWTREE if x["tag"] == "application"][0]
    assert app["attrs"].get("name") == APP, "application@name=%s" % app["attrs"].get("name")
    assert "appComponentFactory" not in app["attrs"], "出现 appComponentFactory"
    assert "ProxyApplication" not in json.dumps(app["attrs"]), "仍含 ProxyApplication"
    return "application@name=%s，无 appComponentFactory/ProxyApplication" % APP
chk("C2", "application@name=BingoApplication 且无 appComponentFactory", c2)

# C3
def c3():
    rc, out, _ = run([os.path.join(BT,"aapt2"),"dump","badging",APK])
    m = re.findall(r"launchable-activity: name='([^']+)'", out)
    assert m == [ACT], "launchable-activity=%s" % m
    acts = [x for x in NEWTREE if x["tag"] == "activity" and x["attrs"].get("name") == ACT]
    assert len(acts) == 1, "AppActivity 节点数=%d" % len(acts)
    hit = False
    for f in [c for c in acts[0]["children"] if c["tag"] == "intent-filter"]:
        kids = [c["attrs"].get("name","") for c in f["children"]]
        if "android.intent.action.MAIN" in kids and "android.intent.category.LAUNCHER" in kids: hit = True
    assert hit, "AppActivity 没有同时含 MAIN+LAUNCHER 的 intent-filter"
    assert not [x for x in NEWTREE if x["attrs"].get("name","").endswith("StubActivity")], "仍有 StubActivity 元素"
    return "launchable-activity=%s；同一 intent-filter 内 MAIN+LAUNCHER；无 StubActivity 元素" % ACT
chk("C3", "launchable=AppActivity(MAIN+LAUNCHER)，无 StubActivity", c3)

# C4
def c4():
    def prov(tree):
        return [{"name": x["attrs"].get("name"), "auth": x["attrs"].get("authorities")}
                for x in tree if x["tag"] == "provider"]
    new, old = prov(NEWTREE), prov(OLDTREE)
    assert len(old) == 18, "官方 provider 数=%d" % len(old)
    inj = [p for p in new if p["name"] == PROVIDER]
    assert len(inj) == 1, "注入 provider 条数=%d" % len(inj)
    rest = [p for p in new if p["name"] != PROVIDER]
    assert len(rest) == 18, "官方 provider 条数变了：%d" % len(rest)
    assert [p["auth"] for p in rest] == [p["auth"] for p in old], "官方 authorities 与原件不逐条相同（或顺序变了）"
    assert sorted(p["name"] for p in rest) == sorted(REAL.values()), "官方 18 provider @name 与 apksecure.real 表不符"
    assert len({p["auth"] for p in new}) == 19, "authorities 条数/重复异常（%d）" % len({p["auth"] for p in new})
    assert inj[0]["auth"] == PKG + ".starryinit", "注入 authority=%s" % inj[0]["auth"]
    return "官方 18 provider @name/authorities 逐条不变 + 注入 %s(authority=%s.starryinit)；19 条无重复" % (PROVIDER, PKG)
chk("C4", "18 官方 provider 不变 + 1 注入 provider", c4)

# C5
def c5():
    want = ["classes.dex"] + ["classes%d.dex" % i for i in range(2, len(dexpaths) + 1)]
    assert [x[0] for x in dexpaths] == want, "dex 编号不连续: %s" % [x[0] for x in dexpaths]
    assert 3 <= len(dexpaths) <= 6, "dex 数异常: %d" % len(dexpaths)
    assert len(dexpaths) == 5, "本阶段应为 classes..classes5 共 5 份（实测 %d）" % len(dexpaths)
    assert all(n in names for n, _ in dexpaths), "存在名为 classesN.dex 但未连续"
    for s in dexinfo:
        assert s["magic"].startswith("dex\n035"), "%s magic=%r" % (s["entry"], s["magic"])
        assert s["file_size_hdr"] == s["zip_size"], "%s header file_size=%d != zip=%d" % (s["entry"], s["file_size_hdr"], s["zip_size"])
        assert s["class_defs_size"] > 0, "%s class_defs=0" % s["entry"]
        assert s["zip_size"] != 1538536, "%s 仍是壳 dex(1,538,536B)" % s["entry"]
    for nm, p in dexpaths:
        rc, out, err = run([os.path.join(BT,"dexdump"),"-c",p])
        assert rc == 0, "dexdump -c 失败 %s: %s" % (nm, (out+err)[:200])
        rc2, out2, _ = run([os.path.join(BT,"dexdump"),"-f",p])
        assert rc2 == 0 and "class_defs_size" in out2, "dexdump -f 失败 %s" % nm
    return "%s 连续无空洞；magic=dex035；file_size==zip 长度；dexdump -c 校验和全过；class_defs=%s" % ("/".join(n for n, _ in dexpaths), [s["class_defs_size"] for s in dexinfo])
chk("C5", "dex 连续编号 + magic/file_size + dexdump -c", c5)

# C6
def c6():
    need = {"BingoApplication": "Lcom/bingo/cocos/bridge/BingoApplication;", "AppActivity": "Lorg/cocos2dx/javascript/AppActivity;"}
    where = {}
    for k, d in need.items():
        hit = [s["entry"] for s in dexinfo if d in s["classes"]]
        assert hit, "真实 dex 内未定义 %s" % d
        where[k] = hit[0]
    return "BingoApplication 定义在 %s；AppActivity 定义在 %s（class_defs 级）" % (where["BingoApplication"], where["AppActivity"])
chk("C6", "真实 dex 定义 BingoApplication 与 AppActivity", c6)

# C7
def c7():
    for abi in ("arm64-v8a","armeabi-v7a"):
        for n2 in ("libsecret.so","libEncryptorP.so","libzeus_direct_dex.so","libterrain.so","libluster.so"):
            assert "lib/%s/%s" % (abi,n2) not in names, "仍含壳 so lib/%s/%s" % (abi,n2)
    assert not [x for x in names if x.startswith("lib/x86/")], "仍含 lib/x86/*"
    h = sha(zo.read("lib/arm64-v8a/libcocos2djs.so"))
    assert h == SO_SHA, "libcocos2djs.so sha256=%s" % h
    assert "assets/apksecuresolib/arm64-v8a/libcocos2djs.so" not in names
    return "5 壳 so（arm64+v7a）+ lib/x86/libsecret.so 已删；lib/arm64-v8a/libcocos2djs.so sha256=dd3a0462…b44"
chk("C7", "无壳 so，libcocos2djs.so hash 对齐", c7)

# C8
def c8():
    oset = {i.filename: i.compress_type for i in zi.infolist()}
    mine = [i for i in zo.infolist() if i.filename.endswith(".so")]
    bad = [i.filename for i in mine if oset.get(i.filename, 8) != i.compress_type]
    assert not bad, "so 压缩方式与官方不同: %s" % bad[:5]
    rc, out, err = run([os.path.join(BT,"zipalign"),"-c","-p","4",APK])
    assert rc == 0, "zipalign -c -p 4 失败"
    hist = {}
    for i in mine: hist[i.compress_type] = hist.get(i.compress_type,0)+1
    return "包内 %d 个 .so 压缩方式与官方逐条一致 %s（8=DEFLATED，实测；方案原文 compress=0 与实测不符）；zipalign -c -p 4 OK" % (len(mine), hist)
chk("C8", "so 压缩方式 + zipalign 对齐", c8)

# C9
def c9():
    bad = [x for x in names if x.startswith("assets/apksecure")]
    assert not bad, "assets 仍含壳块: %s" % bad
    assert "assets/1731500040" in names, "assets/1731500040 丢失"
    h, h0 = sha(zo.read("assets/1731500040")), sha(zi.read("assets/1731500040"))
    assert h == h0, "1731500040 被改"
    return "assets 无 apksecure*；1731500040 保留，sha256=%s… 与官方一致" % h[:8]
chk("C9", "assets 去壳块、保 1731500040", c9)

# C10
def c10():
    skip = re.compile(r"lib/(arm64-v8a|armeabi-v7a)/(libsecret|libEncryptorP|libzeus_direct_dex|libterrain|libluster)\.so$")
    touched = {"AndroidManifest.xml","classes.dex","classes2.dex","classes3.dex","classes4.dex","classes5.dex",
               "lib/arm64-v8a/libcocos2djs.so","lib/arm64-v8a/libstarry_magic.so","lib/arm64-v8a/libc++_shared.so"}
    same = checked = 0; diff = []
    for it in zi.infolist():
        nm = it.filename
        if nm.endswith("/") or nm in touched or nm.startswith("META-INF/") \
           or nm in ("assets/apksecurefile","assets/apksecuresolist") \
           or nm.startswith("assets/apksecuresolib/") or nm.startswith("lib/x86/") or skip.search(nm):
            continue
        checked += 1
        if sha(zi.read(nm)) == sha(zo.read(nm)): same += 1
        else: diff.append(nm)
    assert same == checked, "白名单外条目被改: %s" % diff[:5]
    assert sha(zo.read("resources.arsc")) == sha(zi.read("resources.arsc")), "resources.arsc 与官方不同"
    return "白名单外 %d/%d 条目与官方逐字节同；resources.arsc sha256 与官方一致" % (same, checked)
chk("C10", "白名单 diff（arsc 与其余条目逐字节）", c10)

# C11
def c11():
    rc, out, err = run([os.path.join(BT,"apksigner"),"verify","-v","--print-certs",APK])
    assert rc == 0, "apksigner verify 失败: %s" % (out+err)[:200]
    for s in ("v1 scheme (JAR signing): true","v2 scheme (APK Signature Scheme v2): true","v3 scheme (APK Signature Scheme v3): true"):
        assert s in out, "缺 " + s
    assert "CN=StarryMagic" in out, "证书 DN 不含 StarryMagic"
    assert not [x for x in names if x.startswith("META-INF/BINGOGAM")], "仍含官方旧 v1 签名块"
    for nm, p in dexpaths:
        assert b"SignSpoof" not in open(p,"rb").read(), "%s 含 SignSpoof" % nm
    return "v1/v2/v3 全 true；DN=CN=StarryMagic；无 BINGOGAM 旧 v1；dex 无 SignSpoof"
chk("C11", "签名 v1/v2/v3 + 证书 + 无旧 v1", c11)

# C12（本阶段改口径）：菜单注入点齐全 —— 基座零注入包(C12 旧口径)已单独归档，本包唯一新增就是菜单
def c12():
    t = zo.read("AndroidManifest.xml").decode("utf-16-le","ignore")
    for b in (PROVIDER, MSVC, PKG + ".starryinit"):
        assert b in t, "Manifest 缺 %s" % b
    prov = [x for x in NEWTREE if x["tag"] == "provider" and x["attrs"].get("name") == PROVIDER]
    assert len(prov) == 1, "注入 provider 节点数=%d" % len(prov)
    a = prov[0]["attrs"]
    assert a.get("exported") == "false", "provider exported=%r" % a.get("exported")
    assert a.get("initOrder") == "2147483647", "provider initOrder=%r" % a.get("initOrder")
    svc = [x for x in NEWTREE if x["tag"] == "service" and x["attrs"].get("name") == MSVC]
    assert len(svc) == 1, "MenuService 节点数=%d" % len(svc)
    d5 = [s2 for s2 in dexinfo if s2["entry"] == "classes5.dex"]
    assert d5, "缺 classes5.dex（菜单 dex 未接排）"
    for k in ("Lcom/starry/magic/StarryInitProvider;", "Lcom/starry/magic/menu/MenuService;",
              "Lcom/starry/magic/cocos/DwdlmHooks;", "Lcom/starry/magic/dwdlm/DwdlmAppComponentFactory;"):
        assert k in d5[0]["classes"], "classes5.dex 未定义 %s" % k
    assert MENU_SO in names, "缺 %s" % MENU_SO
    sob = zo.read(MENU_SO)
    hs = sha(sob)
    if os.path.isfile(MENU_SO_SRC):
        assert hs == sha(open(MENU_SO_SRC,"rb").read()), "包内 so 与 libs_dwdlm/env 产物不一致：%s" % hs
    assert b"starry_dwdlm_v1" not in sob, "so 含 starry_dwdlm_v1（玩法补丁不该启用）"
    assert b"DWDLM_ENV_ONLY" in sob, "so 缺 DWDLM_ENV_ONLY 横幅（不是 env 档）"
    assert b"dwdlm" in sob, "so 无 dwdlm 标签"
    assert CXX_SO in names, "缺 libc++_shared.so（libstarry_magic.so 的 NEEDED，llvm-readelf 实测）"
    for nm2, p2 in dexpaths:
        b = open(p2,"rb").read()
        assert b"SignSpoof" not in b, "%s 含 SignSpoof" % nm2
        assert b"bin/mt/signature" not in b, "%s 含 bin/mt 签名杀手类" % nm2
    assert "apksecure" not in t, "Manifest 仍含 apksecure 字样"
    return ("provider(exported=false, initOrder=2147483647) + MenuService 就位；classes5.dex 四类齐全；"
            "so sha256=%s… starry_dwdlm_v1=0 且 DWDLM_ENV_ONLY 在；libc++_shared.so 随包；"
            "全 5 份 dex 无 SignSpoof/bin/mt；Manifest 无 apksecure" % hs[:8])
chk("C12", "菜单注入点齐全（provider/service/dex/so/依赖）", c12)

ok = all(r["pass"] for r in res)
outj = {"apk": APK, "apk_sha256": sha(open(APK,"rb").read()), "apk_size": os.path.getsize(APK),
        "official_sha256": sha(open(OFFICIAL,"rb").read()), "dex": [{k:v for k,v in s.items() if k!="classes"} for s in dexinfo],
        "checks": res, "all_pass": ok}
open(os.path.join(ROOT,"04_scripts/动物大联盟/shellless_menu_verify.json"),"w",encoding="utf-8").write(json.dumps(outj,ensure_ascii=False,indent=2))
lines = ["# 动物大联盟 1.0.25 无壳包 + 星光菜单 · 静态自检","",
  "apk      : %s" % os.path.basename(APK), "sha256   : %s" % outj["apk_sha256"],
  "size     : %d" % outj["apk_size"], "official : %s" % outj["official_sha256"], "",
  "| # | 项 | 结果 | 证据 |","|---|---|---|---|"]
for r in res:
    lines.append("| %s | %s | %s | %s |" % (r["id"], r["name"], "PASS" if r["pass"] else "FAIL", str(r["detail"]).replace("|","\\|")))
lines += ["","dex 明细:",""]
for s in outj["dex"]:
    lines.append("  %-13s size=%-9d hdr=%-9d class_defs=%-5d sha256=%s" % (s["entry"],s["size"],s["file_size_hdr"],s["class_defs_size"],s["sha256"][:16]))
lines += ["","总判定: %s" % ("C1–C12 全部 PASS" if ok else "存在 FAIL，禁止装机")]
open(os.path.join(ROOT,"04_scripts/动物大联盟/shellless_menu_verify.txt"),"w",encoding="utf-8").write("\n".join(lines)+"\n")
print("\n".join(lines))
sys.exit(0 if ok else 1)