#!/usr/bin/env python3
# script-policy: {"purpose":"追迹迷城 1.0.20261111 成品包验收器：身份/外科铁证/TapSDK 三补丁/无 killer/Manifest/so 菜单逐项断言并落 JSON","inputs":"成品 APK、官方底包 APK、patch_zjmc_tapsdk_static.py、axml_inspect.py、zjmc_menu_template.json","outputs":"07_patched/追迹迷城/surgical_1.0.20261111_singletrack/verify_report.json","boundary":"只读成品包与底包；不改任何文件；退出码 1 即验收失败","prior_art":"04_scripts/_common/verify_installed_apk.sh（装机后校验）、04_scripts/_common/check_release_apk.sh（发布包静态检查）","difference":"对成品 APK 做 20 项 CLAIM 断言，替代 file_exists 占位","evidence":"10_docs/追迹迷城/路线锁定.md#单轨"}
# 追迹迷城 1.0.20261111 — 单轨成品包验收器（补齐 pipeline verify 的实际断言）
#
# 为什么要它：原 verify 只有 18 条 file_exists（文件在 ≠ 内容对）。本脚本对**成品 APK 本身**
# 做契约断言，逐条给 CLAIM/VERDICT/EVIDENCE，并落 JSON 报告供管线 verify 的 file_contains 判定。
#
# 断言面（与 10_docs/追迹迷城/路线锁定.md「1.0.20261111 单轨」逐项对应）：
#   A 身份：包内 dex 布局、classes6 存在、so/metadata 条目
#   B 外科铁证：显式 allowlist 的增/删/改集合 + 关键 7 项流式 SHA256 + 官方 85 条 META-INF
#     非签名条目逐条流式 SHA256 相等（2026-11-11 修复：不再用「META-INF/ 前缀整体放过」，
#     该宽泛跳过一次就掩盖了外科脚本误删 85 条 services/kotlinx/Unity Ads 条目的缺陷）；
#     ZIP 路径唯一性也在此断言（重复 entry 一律 FAIL）
#   C TapSDK 改包法三处补丁（回读 classes4 反汇编，复用 patch_zjmc_tapsdk_static.py --verify）
#   D classes6 只放星光：无 bin/mt/ killer、无 SignSpoof；星光入口三类齐备
#   E Manifest：Factory 仍为 androidx.core.app.CoreComponentFactory、无 android:name、
#     provider 23→24 / service 17→18 / 权限 23→25、res id 未错位
#   F so：md5 与构建产物一致、[Zjmc] 标签、五项菜单串齐全且恰好五项
import hashlib
import json
import os
import re
import struct
import subprocess
import sys
import tempfile
import zipfile
import zlib
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
AXML = ROOT / "04_scripts/_common/axml_inspect.py"
TAPSDK = ROOT / "04_scripts/追迹迷城/patch_zjmc_tapsdk_static.py"
BAKSMALI = ROOT / "01_tools/smali/baksmali"
MENU_TPL = ROOT / "10_docs/追迹迷城/zjmc_menu_template.json"
SO_MD5_EXPECT = "f0123ac41ea151f9c6e58b719289b19d"

# ── 差异允许集（2026-11-11 修复：取消「META-INF/ + assets/certs/ 整体放过」的宽泛跳过）──
# 允许「同路径内容变」：只有 Manifest 与 classes4（改包法落点）；签名文件形态另由 SIG_RE 判。
ALLOW_DIFF = {"AndroidManifest.xml", "classes4.dex"}
# 允许「新增」的固定条目（星光容器 + 模板证书）。签名条目由 SIG_RE 判（签名者名可变）。
ALLOW_ADD = {
    "classes6.dex",
    "lib/arm64-v8a/libstarry_magic.so",
    "lib/arm64-v8a/libc++_shared.so",
    "assets/certs/starlight.crt",
}
# 顶层旧/新签名元数据形态（不含子目录 ⇒ services/kotlinx/androidx/Unity Ads 永不被放过）
SIG_RE = re.compile(r"^META-INF/(?:MANIFEST[.]MF|[^/]+[.](?:SF|RSA|DSA|EC)|SIG-[^/]+)$")
# 必须流式 SHA256 硬证的 7 项：官方 4 dex + resources.arsc + libil2cpp.so + global-metadata.dat
KEY_SHA = [
    "classes.dex", "classes2.dex", "classes3.dex", "classes5.dex",
    "resources.arsc",
    "lib/arm64-v8a/libil2cpp.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
]
# ServiceLoader / Unity Ads 广告中介校验：逐条存在且 SHA256 相等
META_CRITICAL = [
    "META-INF/services/java.security.Provider",
    "META-INF/services/kotlinx.coroutines.CoroutineExceptionHandler",
    "META-INF/services/kotlinx.coroutines.internal.MainDispatcherFactory",
    "META-INF/com/unity3d/ads-mediation/adquality-sdk/verification.properties",
    "META-INF/com/unity3d/ads-mediation/mediation-sdk/verification.properties",
]
BUF = 1 << 20


def stream_sha256(zf, name):
    """流式读完整条目算 SHA256，同时复核 central directory 的 CRC32/size（不只信中央目录）。"""
    info = zf.getinfo(name)
    h, crc, total = hashlib.sha256(), 0, 0
    with zf.open(name) as fp:
        while True:
            block = fp.read(BUF)
            if not block:
                break
            h.update(block)
            crc = zlib.crc32(block, crc)
            total += len(block)
    crc &= 0xFFFFFFFF
    assert crc == info.CRC and total == info.file_size, \
        "流式读取与中央目录不符: %s crc=%08x/%08x size=%d/%d" % (
            name, crc, info.CRC, total, info.file_size)
    return h.hexdigest(), crc, total


def dex_type_descriptors(d):
    """解析 dex type_ids → 类型描述符（不依赖字符串搜索）。"""
    f = struct.unpack_from("<20I", d, 0x20)
    n_type, off_type = f[8], f[9]
    str_ids_off = f[7]

    def string_at(idx):
        so = struct.unpack_from("<I", d, str_ids_off + idx * 4)[0]
        p = so
        while d[p] & 0x80:
            p += 1
        p += 1
        return d[p:d.index(b"\x00", p)].decode("utf-8", "replace")

    return [string_at(struct.unpack_from("<I", d, off_type + i * 4)[0]) for i in range(n_type)]


def dex_class_descriptors(d):
    """解析 class_defs → 类描述符列表（killer 判定用真实类定义，不用反射字符串）。"""
    f = struct.unpack_from("<20I", d, 0x20)
    n_cd, off_cd = f[16], f[17]
    types = dex_type_descriptors(d)
    out = []
    for i in range(n_cd):
        cls_idx = struct.unpack_from("<I", d, off_cd + i * 32)[0]
        out.append(types[cls_idx] if cls_idx < len(types) else "?")
    return out


class Report:
    def __init__(self):
        self.items = []
        self.ok = True

    def add(self, cid, claim, verdict, evidence):
        self.items.append({"id": cid, "claim": claim, "verdict": verdict, "evidence": evidence})
        if verdict == "FAIL":
            self.ok = False
        print("[%s] %s: %s" % (verdict, cid, claim))

    def check(self, cid, claim, cond, evidence):
        self.add(cid, claim, "PASS" if cond else "FAIL", evidence)
        return cond


def dex_header(data):
    mag = data[:8]
    assert mag in (b"dex\n035\x00", b"dex\n036\x00", b"dex\n037\x00",
                   b"dex\n038\x00", b"dex\n039\x00"), mag
    f = struct.unpack_from("<20I", data, 0x20)
    return {"method_ids": f[14], "class_defs": f[16]}


def axml_tree(xml_path):
    out = subprocess.run([sys.executable, str(AXML), str(xml_path)],
                         check=True, capture_output=True, text=True).stdout
    return out.splitlines()


def count_tree(lines, prefix):
    return sum(1 for l in lines if l.startswith(prefix))


def app_attrs(lines):
    attrs, inside = [], False
    for l in lines:
        if l.startswith("E: application"):
            inside = True
            continue
        if inside and l.startswith("E: "):
            break
        if inside:
            attrs.append(l)
    return attrs


def main():
    apk = Path(sys.argv[1]) if len(sys.argv) > 1 else None
    if apk is None:
        apk = ROOT / "08_output/追迹迷城/追迹迷城_1.0.20261111_starry-singletrack-final-releaseSigned.apk"
    base = Path(sys.argv[2]) if len(sys.argv) > 2 else None
    if base is None:
        meta = json.load(open(ROOT / "04_scripts/追迹迷城/official_apk.json", encoding="utf-8"))
        base = Path(meta["apk_path"])
    report_path = Path(sys.argv[3]) if len(sys.argv) > 3 else apk.parent / "verify_singletrack.json"

    rep = Report()
    if not apk.is_file():
        print("ERROR: missing APK " + str(apk))
        return 1
    if not base.is_file():
        print("ERROR: missing base APK " + str(base))
        return 1

    raw = apk.read_bytes()
    sha, md5 = hashlib.sha256(raw).hexdigest(), hashlib.md5(raw).hexdigest()
    print("APK      : %s" % apk)
    print("size     : %d B" % len(raw))
    print("sha256   : %s" % sha)
    print("md5      : %s" % md5)

    z, zo = zipfile.ZipFile(apk), zipfile.ZipFile(base)
    names, onames = set(z.namelist()), set(zo.namelist())

    # ---- A 身份 -------------------------------------------------------------
    dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
    rep.check("A1", "dex 布局 = classes.dex..classes6.dex（无跳号）",
              dexes == ["classes.dex", "classes2.dex", "classes3.dex",
                        "classes4.dex", "classes5.dex", "classes6.dex"], str(dexes))
    rep.check("A2", "包内存在 libil2cpp.so 与 global-metadata.dat",
              "lib/arm64-v8a/libil2cpp.so" in names and
              "assets/bin/Data/Managed/Metadata/global-metadata.dat" in names, "entries ok")

    # ---- B 外科铁证：显式增删改集合 + 关键项流式 SHA256 ----------------------
    nlist, olist = z.namelist(), zo.namelist()
    cnew, cold = Counter(nlist), Counter(olist)
    dup_new = sorted(n for n, c in cnew.items() if c > 1)
    dup_old = sorted(n for n, c in cold.items() if c > 1)
    rep.check("B0", "ZIP 路径唯一（成品与底包均无重复 entry）",
              not dup_new and not dup_old, "new_dup=%s base_dup=%s" % (dup_new, dup_old))

    removed, added = set(olist) - set(nlist), set(nlist) - set(olist)
    base_sig = {n for n in removed if SIG_RE.match(n)}
    unexpected_removed = sorted(removed - base_sig)
    rep.check("B1a", "删除集合 ⊆ 旧签名允许集（META-INF 业务条目一条都不能少）",
              not unexpected_removed,
              "removed=%d（其中旧签名 %d）非签名被删=%s"
              % (len(removed), len(base_sig), unexpected_removed[:10]))

    new_sig = {n for n in added if SIG_RE.match(n)}
    unexpected_add = sorted(added - ALLOW_ADD - new_sig)
    rep.check("B1b", "新增集合 ⊆ {classes6.dex, 两个 so, assets/certs/starlight.crt, 签名文件}",
              not unexpected_add, "added=%s" % sorted(added))
    rep.check("B1c", "成品带新签名（META-INF/MANIFEST.MF + 至少一个 .SF/.RSA）",
              "META-INF/MANIFEST.MF" in names and
              any(n.endswith((".SF", ".RSA")) for n in new_sig),
              "sig=%s" % sorted(new_sig))

    diffs, missing = [], []
    for info in zo.infolist():
        n = info.filename
        if n in ALLOW_DIFF or SIG_RE.match(n):
            continue
        if n not in names:
            missing.append(n)
            continue
        ni = z.getinfo(n)
        if ni.CRC != info.CRC or ni.file_size != info.file_size:
            diffs.append(n)
    rep.check("B1", "除允许改动项（Manifest/classes4/签名）外所有条目 CRC32+size 与官方底包一致",
              not diffs and not missing, "diff=%s missing=%s" % (diffs, missing[:10]))

    key_fail, key_ev = [], []
    for n in KEY_SHA:
        if n not in names or n not in set(olist):
            key_fail.append(n + "(missing)")
            continue
        a, b = stream_sha256(zo, n), stream_sha256(z, n)
        key_ev.append("%s=%s" % (n.split("/")[-1], b[0][:12]))
        if a[0] != b[0]:
            key_fail.append(n)
    rep.check("B2a", "关键 %d 项流式 SHA256 与底包逐字节相等（classes/2/3/5 + arsc + il2cpp + metadata）"
              % len(KEY_SHA), not key_fail,
              ("fail=%s " % key_fail if key_fail else "") + " ".join(key_ev))

    # ---- B3 META-INF 业务条目：全量保留 + 逐条流式 SHA256 ---------------------
    meta_base = [n for n in olist if n.startswith("META-INF/") and not SIG_RE.match(n)]
    meta_new = set(n for n in nlist if n.startswith("META-INF/") and not SIG_RE.match(n))
    lost = sorted(set(meta_base) - meta_new)
    rep.check("B3a", "官方 META-INF 非签名条目 %d 条全部存在（services/kotlinx/androidx/Unity Ads）"
              % len(meta_base), not lost and len(meta_base) > 0, "lost=%s" % lost[:10])
    meta_bad = []
    for n in meta_base:
        if n in meta_new and stream_sha256(zo, n)[0] != stream_sha256(z, n)[0]:
            meta_bad.append(n)
    rep.check("B3b", "META-INF 非签名条目逐条流式 SHA256 相等",
              not meta_bad, "changed=%s" % meta_bad[:10])
    crit_ev, crit_fail = [], []
    for n in META_CRITICAL:
        if n not in meta_new:
            crit_fail.append(n)
            continue
        a, b = stream_sha256(zo, n), stream_sha256(z, n)
        crit_ev.append("%s=%s" % (n.split("META-INF/")[-1], b[0][:16]))
        if a[0] != b[0]:
            crit_fail.append(n)
    rep.check("B3c", "ServiceLoader×3 + Unity Ads verification.properties×2 存在且 SHA256 相等",
              not crit_fail, "fail=%s | %s" % (crit_fail, " ; ".join(crit_ev)))

    rep.check("B2b", "classes4.dex 确已改动（改包法落点）",
              zo.getinfo("classes4.dex").CRC != z.getinfo("classes4.dex").CRC,
              "base CRC=%08x new CRC=%08x" % (zo.getinfo("classes4.dex").CRC,
                                              z.getinfo("classes4.dex").CRC))

    tmp = Path(tempfile.mkdtemp(prefix="zjmc_verify_"))
    (tmp / "c4.dex").write_bytes(z.read("classes4.dex"))
    subprocess.run([str(BAKSMALI), "d", "-o", str(tmp / "c4"), str(tmp / "c4.dex")],
                   check=True, stdout=subprocess.DEVNULL)
    # ---- C TapSDK 三处补丁 --------------------------------------------------
    r = subprocess.run([sys.executable, str(TAPSDK), "--verify", str(tmp / "c4")],
                       capture_output=True, text=True)
    rep.check("C1", "TapSDK 改包法三处补丁在（门闩恒 true / 弹窗 return-void / provide 恒 WEB）",
              r.returncode == 0,
              (r.stdout + r.stderr).strip().replace("\n", " | ")[:300])

    # ---- D classes6 只放星光 ------------------------------------------------
    d6 = z.read("classes6.dex")
    h6 = dex_header(d6)
    rep.check("D1", "classes6.dex 含星光入口类（StarryApp/MenuService/StarryInitProvider）",
              all(s in d6 for s in (b"com/starry/magic/StarryApp",
                                    b"com/starry/magic/menu/MenuService",
                                    b"com/starry/magic/StarryInitProvider")),
              "class_defs=%d method_ids=%d" % (h6["class_defs"], h6["method_ids"]))
    cls6 = dex_class_descriptors(d6)
    killer6 = [c for c in cls6 if c.startswith("Lbin/mt/")]
    rep.check("D2", "classes6.dex class_defs 无 killer 类（Lbin/mt/**，解析而非字符串搜索）",
              not killer6, "class_defs=%d killer=%s" % (len(cls6), killer6[:5]))
    killer_all, spoof_all, cls_total = [], [], 0
    for n in dexes:
        cd = dex_class_descriptors(z.read(n))
        cls_total += len(cd)
        killer_all += ["%s:%s" % (n, c) for c in cd if c.startswith("Lbin/mt/")]
        spoof_all += ["%s:%s" % (n, c) for c in cd if c == "Lcom/starry/magic/sign/SignSpoof;"]
    rep.check("D3", "全包 %d 个 dex（共 %d 个 class_defs）无 bin/mt killer 类定义"
              % (len(dexes), cls_total), not killer_all, "hits=%s" % killer_all[:5])
    rep.check("D4", "全包 class_defs 无已删除的 SignSpoof",
              not spoof_all, "hits=%s" % spoof_all[:5])

    # ---- E Manifest ---------------------------------------------------------
    (tmp / "mf.xml").write_bytes(z.read("AndroidManifest.xml"))
    (tmp / "mf_base.xml").write_bytes(zo.read("AndroidManifest.xml"))
    subprocess.run([sys.executable, str(AXML), str(tmp / "mf.xml"), "--assert-map", "--quiet"],
                   check=True, stdout=subprocess.DEVNULL)
    t_new, t_old = axml_tree(tmp / "mf.xml"), axml_tree(tmp / "mf_base.xml")
    attrs = app_attrs(t_new)
    fac = [l for l in attrs if "appComponentFactory" in l]
    rep.check("E1", "application@appComponentFactory 保持 androidx.core.app.CoreComponentFactory",
              len(fac) == 1 and '"androidx.core.app.CoreComponentFactory"' in fac[0],
              fac[0].strip() if fac else "missing")
    rep.check("E2", "application 无 android:name（不注入 StarryApp）",
              not [l for l in attrs if "android:name" in l], "no android:name")
    mf_raw = z.read("AndroidManifest.xml")
    rep.check("E3", "Manifest 不挂 StarryAppComponentFactory / SignSpoofAppComponentFactory",
              "StarryAppComponentFactory".encode("utf-16-le") not in mf_raw and
              "SignSpoofAppComponentFactory".encode("utf-16-le") not in mf_raw,
              "factory strings absent")
    counts = {}
    for el, want in (("E: provider", 1), ("E: service", 1), ("E: uses-permission", 2)):
        o, n = count_tree(t_old, el), count_tree(t_new, el)
        counts[el] = (o, n)
        rep.check("E4" + el[-1], "%s 官方→成品 = +%d（%d→%d）" % (el, want, o, n),
                  n - o == want, "%d -> %d" % (o, n))
    rep.check("E7", "activity 数量未变（未新增宿主 Activity）",
              count_tree(t_old, "E: activity") == count_tree(t_new, "E: activity"),
              "%d -> %d" % (count_tree(t_old, "E: activity"), count_tree(t_new, "E: activity")))

    # ---- F so 与菜单 --------------------------------------------------------
    so = z.read("lib/arm64-v8a/libstarry_magic.so")
    so_md5 = hashlib.md5(so).hexdigest()
    rep.check("F1", "包内 so md5 == 构建产物 %s" % SO_MD5_EXPECT, so_md5 == SO_MD5_EXPECT, so_md5)
    rep.check("F2", "so 带本游戏标签 [Zjmc] 且无上一款 [Cstjjs]",
              b"[Zjmc]" in so and b"[Cstjjs]" not in so, "tag ok")
    tpl = json.load(open(MENU_TPL, encoding="utf-8"))
    feats = tpl["features"]
    labels = ["Toggle_" + f["name"] for f in feats]
    hit = [lb for lb in labels if lb.encode("utf-8") in so]
    rep.check("F3", "菜单恰 5 项且全部在 so 里（%s）" % ", ".join(f["name"] for f in feats),
              len(feats) == 5 and len(hit) == 5, "hit=%d/5: %s" % (len(hit), hit))
    rep.check("F4", "so 含免广告 Hook 标签 AT.GetFreeSkipAds",
              b"AT.GetFreeSkipAds" in so, "present")

    verdict = "PASS" if rep.ok else "FAIL"
    out = {"apk": str(apk), "size": len(raw), "sha256": sha, "md5": md5,
           "base_apk": str(base), "verdict": verdict, "checks": rep.items}
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print("\nCLAIM/VERDICT: %s（%d 项，FAIL %d）" % (
        verdict, len(rep.items), sum(1 for i in rep.items if i["verdict"] == "FAIL")))
    print("report: %s" % report_path)
    return 0 if rep.ok else 1


if __name__ == "__main__":
    sys.exit(main())
