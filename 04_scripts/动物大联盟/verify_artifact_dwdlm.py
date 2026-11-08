#!/usr/bin/env python3
# 动物大联盟 1.0.25 — 成品包独立核验（pipeline verify_artifact 阶段）
#
# 与 surgical 脚本自检的分工：surgical 的自检发生在**写包当时**；本脚本**事后**从磁盘重读成品包，
# 独立复算 size/sha256、逐条比对官方条目透传、核 dex 内工厂类与 nativeEnvStart 声明、核二进制 Manifest
# 关键串、核包内 so 的 .dynsym 导出与 md5、核签名证书。凡是"改过就是坏的"条目一律 FAIL。
# 输出：逐项 PASS/FAIL/SKIP + 原因；有 FAIL 则 exit 1。同时写 04_scripts/动物大联盟/artifact_verify_<profile>.json。
import hashlib
import json
import os
import re
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "动物大联盟"
VER = "1.0.25"
PKG = "com.fightingstars.games.animal"
BASE = ROOT / "02_apks" / GAME / (GAME + "_" + VER + "_official.apk")
BASE_SHA = "2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539"
BASE_SIZE = 81191854
PROFILE = os.environ.get("DWDLM_SO_PROFILE", "env")
# 单变量对照轮（2026-11-05）：factory=挂 appComponentFactory；provider-only=**不挂**（独立候选名/独立报告）
MODE = os.environ.get("DWDLM_FACTORY_MODE", "factory")
MODE_SUFFIX = "_provideronly" if MODE == "provider-only" else ""
# 2026-11-05 第五轮：DWDLM_SIGSPOOF=1 ⇒ 成品名带 -sigspoof、报告带 _sigspoof（单变量＝加运行时签名替换）
SIGSPOOF = os.environ.get("DWDLM_SIGSPOOF", "0")
SPOOF_SUFFIX = "_sigspoof" if SIGSPOOF == "1" else ""
PROFILE_TAG = PROFILE + ("-provideronly" if MODE == "provider-only" else "") \
    + ("-sigspoof" if SIGSPOOF == "1" else "")
DEFAULT_APK = ROOT / "08_output" / GAME / (GAME + "_" + VER + "_starry-" + PROFILE_TAG + "-releaseSigned.apk")
APK = Path(os.environ.get("DWDLM_APK", str(DEFAULT_APK)))
ASM_REPORT = ROOT / "04_scripts" / GAME / ("assemble_" + PROFILE + MODE_SUFFIX + SPOOF_SUFFIX + ".json")
OUT_REPORT = ROOT / "04_scripts" / GAME / ("artifact_verify_" + PROFILE + MODE_SUFFIX + SPOOF_SUFFIX + ".json")
KILLER_DESC = b"Lbin/mt/signature/KillerApplication193;"
DEX_REUSE_APK = os.environ.get("DWDLM_DEX_REUSE_APK", "")
FACTORY = "com.starry.magic.dwdlm.DwdlmAppComponentFactory"
FACTORY_DESC = "L" + FACTORY.replace(".", "/") + ";"
JNI_SYM = "Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart"
SIG_RE = re.compile(r"^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC)|SIG-[^/]+)$", re.I)
INJECTED = {"AndroidManifest.xml", "classes2.dex",
            "lib/arm64-v8a/libstarry_magic.so", "lib/arm64-v8a/libc++_shared.so"}

rows = []

def rec(level, name, detail):
    rows.append({"level": level, "check": name, "detail": detail})
    print("  %-5s %-22s %s" % (level, name, detail))

def sha(p):
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()

def find_llvm_nm():
    ndk = os.environ.get("ANDROID_NDK_HOME", str(Path.home() / "Library/Android/sdk/ndk/27.0.12077973"))
    for sub in ("darwin-x86_64", "darwin-aarch64", "linux-x86_64"):
        cand = Path(ndk) / "toolchains/llvm/prebuilt" / sub / "bin/llvm-nm"
        if cand.is_file() and os.access(cand, os.X_OK):
            return cand
    return None

def find_apksigner():
    for v in ("36.0.0", "35.0.0", "34.0.0"):
        cand = Path.home() / "Library/Android/sdk/build-tools" / v / "apksigner"
        if cand.is_file():
            return cand
    return None

def check_base():
    if not BASE.is_file():
        rec("FAIL", "base", "缺官方底包 " + str(BASE)); return False
    if BASE.stat().st_size != BASE_SIZE or sha(BASE) != BASE_SHA:
        rec("FAIL", "base", "底包 size/sha256 与登记值不符（底包被替换）"); return False
    rec("PASS", "base", "官方底包 %d B sha256=%s..." % (BASE_SIZE, BASE_SHA[:16]))
    return True

def check_apk():
    if not APK.is_file():
        rec("FAIL", "apk", "成品包不存在 " + str(APK)); return False
    rec("PASS", "apk", "%s  %d B  sha256=%s" % (APK.name, APK.stat().st_size, sha(APK)))
    return True

def check_zip():
    zb = zipfile.ZipFile(BASE); za = zipfile.ZipFile(APK)
    na = set(za.namelist())
    ok = True
    if "classes2.dex" not in na:
        rec("FAIL", "inject", "成品包无 classes2.dex（注入 dex 丢失）"); return False
    rec("PASS", "inject", "classes2.dex 在位 %d B（压缩后 %d B）"
        % (za.getinfo("classes2.dex").file_size, za.getinfo("classes2.dex").compress_size))
    # 2026-11-08 本轮：DWDLM_SEC_SWITCH=1 ⇒ libsecret.so 为「签名路径开关 0x660cd 1→0」补丁版，
    # 属**有意变更**（照 1.0.23 SignSpoof 那一步的等价物），不进透传清单，另记一条 sec.switch。
    SEC_SWITCH_SKIP = set()
    if os.environ.get("DWDLM_SEC_SWITCH", "0") == "1":
        SEC_SWITCH_SKIP.add("lib/arm64-v8a/libsecret.so")
        rec("PASS", "sec.switch", "lib/arm64-v8a/libsecret.so = 签名路径开关补丁版（v2块→PackageManager），有意变更")
    if os.environ.get("DWDLM_LUSTER_PATCH", "0") == "1":
        SEC_SWITCH_SKIP.add("lib/arm64-v8a/libluster.so")
        rec("PASS", "luster.patch", "lib/arm64-v8a/libluster.so = JNI_OnLoad 去 RegisterNatives 版（classloader 退化为系统 loader 时的规避），有意变更")
    diff = []
    checked = 0
    for name in sorted(zb.namelist()):
        if name.endswith("/") or SIG_RE.match(name) or name in INJECTED or name in SEC_SWITCH_SKIP:
            continue
        if name not in na:
            diff.append(name + "(缺失)"); continue
        checked += 1
        if hashlib.sha256(zb.read(name)).hexdigest() != hashlib.sha256(za.read(name)).hexdigest():
            diff.append(name)
    if diff:
        rec("FAIL", "passthrough", "官方条目被改/缺失 %d 项：%s" % (len(diff), diff[:5])); ok = False
    else:
        rec("PASS", "passthrough", "官方条目 %d/%d 逐字节透传（只有 manifest/dex/so 三类被替换）" % (checked, checked))
    same = hashlib.sha256(zb.read("classes.dex")).hexdigest() == hashlib.sha256(za.read("classes.dex")).hexdigest()
    if same:
        rec("PASS", "classes.dex", "官方 classes.dex sha256 一致（壳 stub 未被改）")
    else:
        rec("FAIL", "classes.dex", "官方 classes.dex 被改动"); ok = False
    for must in ("assets/apksecurefile", "lib/arm64-v8a/libsecret.so",
                 "lib/arm64-v8a/libEncryptorP.so", "lib/arm64-v8a/libzeus_direct_dex.so"):
        if must not in na:
            rec("FAIL", "shell_payload", "成品包缺壳条目 " + must); ok = False
    if ok:
        rec("PASS", "shell_payload", "壳 payload / libsecret / libEncryptorP / libzeus 四条壳关键条目在位")
    return ok

def check_dex():
    dex = zipfile.ZipFile(APK).read("classes2.dex")
    ok = True
    if FACTORY_DESC.encode() not in dex:
        rec("FAIL", "dex.factory", "classes2.dex 缺类型 " + FACTORY_DESC); ok = False
    else:
        rec("PASS", "dex.factory", FACTORY_DESC + " 在 classes2.dex 内")
    if b"nativeEnvStart" not in dex:
        rec("FAIL", "dex.jni", "classes2.dex 缺 nativeEnvStart 方法名"); ok = False
    else:
        rec("PASS", "dex.jni", "nativeEnvStart 在 classes2.dex 内（与 so 导出对应）")
    if SIGSPOOF == "1":
        if KILLER_DESC not in dex:
            rec("FAIL", "dex.sigspoof", "classes2.dex 缺 bin/mt/signature/KillerApplication193（签名杀手没进包）"); ok = False
        else:
            rec("PASS", "dex.sigspoof", "bin/mt/signature/KillerApplication193 在 classes2.dex 内（运行时签名替换）")
    else:
        if KILLER_DESC in dex:
            rec("WARN", "dex.sigspoof", "classes2.dex 含签名杀手类（本轮未声明 DWDLM_SIGSPOOF=1）")
    if b"SignSpoof" in dex:
        rec("FAIL", "dex.forbidden", "classes2.dex 含 SignSpoof（禁止复活）"); ok = False
    else:
        rec("PASS", "dex.forbidden", "classes2.dex 无 SignSpoof")
    return ok

def check_dex_reuse():
    """provider-only 对照的命门：dex 必须与同源候选逐字节相同，否则不是单变量。"""
    z = zipfile.ZipFile(APK)
    cur = hashlib.sha256(z.read("classes2.dex")).hexdigest()
    z.close()
    expect = None
    if ASM_REPORT.is_file():
        try:
            expect = json.loads(ASM_REPORT.read_text(encoding="utf-8")).get("inject_dex_sha256")
        except Exception:
            expect = None
    if MODE != "provider-only":
        rec("SKIP", "dex.reuse", "factory 模式不做 dex 复用比对；classes2.dex sha256=" + cur[:16])
        return True
    ok = True
    if expect and expect != cur:
        rec("FAIL", "dex.reuse", "classes2.dex sha256=%s… != assemble 报告 %s…（dex 被重编）"
            % (cur[:16], expect[:16])); ok = False
    if DEX_REUSE_APK and Path(DEX_REUSE_APK).is_file():
        zb = zipfile.ZipFile(DEX_REUSE_APK)
        base = hashlib.sha256(zb.read("classes2.dex")).hexdigest()
        zb.close()
        if base != cur:
            rec("FAIL", "dex.reuse", "classes2.dex 与复用源 %s 不同（%s… vs %s…）"
                % (Path(DEX_REUSE_APK).name, cur[:16], base[:16])); ok = False
        elif ok:
            rec("PASS", "dex.reuse", "classes2.dex 与复用源 %s 逐字节相同（sha256=%s…）"
                % (Path(DEX_REUSE_APK).name, cur[:16]))
    elif ok:
        rec("PASS" if expect else "SKIP", "dex.reuse",
            "classes2.dex sha256=%s…（%s）" % (cur[:16], "与 assemble 报告一致" if expect else "无复用源可比对"))
    return ok

def check_manifest():
    mf = zipfile.ZipFile(APK).read("AndroidManifest.xml")
    if mf[:4] != bytes([3, 0, 8, 0]):
        rec("FAIL", "manifest.axml", "AndroidManifest.xml 头不是 03000800（不是 AXML）"); return False
    mt = mf.decode("utf-16-le", "ignore")
    need = [("dev.mars.secure.ProxyApplication", "application@name=ProxyApplication"),
            ("com.starry.magic.StarryInitProvider", "StarryInitProvider"),
            (PKG + ".starryinit", "provider authority"),
            ("com.starry.magic.menu.MenuService", "MenuService")]
    if MODE == "factory":
        need = [(FACTORY, "appComponentFactory 类名")] + need
    elif FACTORY in mt:
        rec("FAIL", "manifest", "provider-only 但二进制 Manifest 仍含工厂类名 " + FACTORY); return False
    miss = [w for n, w in need if n not in mt]
    if miss:
        rec("FAIL", "manifest", "二进制 Manifest 缺：" + "、".join(miss)); return False
    rec("PASS", "manifest", "AXML 头 03000800；" + (
        "ProxyApplication + appComponentFactory + Provider + authority + MenuService 齐全"
        if MODE == "factory" else
        "ProxyApplication + Provider + authority + MenuService 齐全，**无 appComponentFactory（本轮有意对照）**"))
    bad = [(b, w) for b, w in (("SignSpoof", "SignSpoof"),
                              ("com.starry.magic.dwdlm.DwdlmApplication", "DwdlmApplication 替换")) if b in mt]
    if bad:
        rec("FAIL", "manifest.forbidden", "Manifest 含禁止项：" + "、".join(w for _, w in bad)); return False
    rec("PASS", "manifest.forbidden", "无 SignSpoof / 未替换 DwdlmApplication")
    return True

def check_so():
    z = zipfile.ZipFile(APK)
    data = z.read("lib/arm64-v8a/libstarry_magic.so")
    md5 = hashlib.md5(data).hexdigest()
    ok = True
    expect = None
    if ASM_REPORT.is_file():
        try:
            expect = json.loads(ASM_REPORT.read_text(encoding="utf-8")).get("so_md5")
        except Exception:
            expect = None
    if expect and md5 != expect:
        rec("FAIL", "so.md5", "包内 so md5=%s != assemble 报告 %s" % (md5, expect)); ok = False
    elif expect:
        rec("PASS", "so.md5", "包内 so md5=%s 与 assemble 报告一致（%d B）" % (md5, len(data)))
    else:
        rec("SKIP", "so.md5", "无 assemble 报告可比对，仅报 md5=" + md5)
    nm = find_llvm_nm()
    if not nm:
        rec("SKIP", "so.dynsym", "缺 llvm-nm，无法校验导出符号（构建脚本侧已校验）")
    else:
        tmp = Path(os.environ.get("TMPDIR", "/tmp")) / "dwdlm_verify_final.so"
        tmp.write_bytes(data)
        out = subprocess.run([str(nm), "-D", "--defined-only", str(tmp)],
                             capture_output=True, text=True).stdout
        if (" T " + JNI_SYM) in out:
            rec("PASS", "so.dynsym", "导出定义 " + JNI_SYM)
        else:
            rec("FAIL", "so.dynsym", "缺导出定义 " + JNI_SYM); ok = False
        if b"check-report" in data:
            rec("FAIL", "so.legacy", "so 内仍含旧壳 hook 标签 check-report"); ok = False
        else:
            rec("PASS", "so.legacy", "so 内无 check-report（旧 1.0.23 壳链未编入）")
        if b"starry_dwdlm_v1" in data:
            rec("FAIL", "so.playpatch", "so 内含玩法 JS 补丁标记 starry_dwdlm_v1"); ok = False
        else:
            rec("PASS", "so.playpatch", "so 内无 starry_dwdlm_v1（六项玩法未编入）")
        try:
            tmp.unlink()
        except Exception:
            pass
    return ok

def check_cert():
    sig = find_apksigner()
    if not sig:
        rec("SKIP", "cert", "缺 apksigner，未核证书"); return True
    r = subprocess.run([str(sig), "verify", "--print-certs", "--verbose", str(APK)], capture_output=True, text=True)
    out = r.stdout + r.stderr
    m = re.search(r"Signer #1 certificate SHA-256 digest: ([0-9a-f]+)", out)
    v2ok = "Verified using v2 scheme (APK Signature Scheme v2): true" in out
    v3ok = "Verified using v3 scheme (APK Signature Scheme v3): true" in out
    verified = v2ok or v3ok
    if r.returncode != 0:
        rec("FAIL", "cert", "apksigner verify 失败：%s" % out.strip().splitlines()[:3]); return False
    if not m:
        rec("FAIL", "cert", "读不到签名证书 SHA-256"); return False
    rec("PASS", "cert", "证书 SHA-256=%s（v2/v3=%s）" % (m.group(1), "yes" if verified else "unknown"))
    return True

def check_source_freshness():
    helper = ROOT / "04_scripts" / GAME / "source_fingerprint_dwdlm.py"
    if not helper.is_file():
        rec("SKIP", "source", "缺 source_fingerprint_dwdlm.py，无法判源漂移"); return True
    r = subprocess.run([sys.executable, str(helper)], capture_output=True, text=True)
    now = (r.stdout or "").strip()
    if r.returncode != 0 or not now:
        rec("FAIL", "source", "源指纹计算失败（源文件缺失？）"); return False
    built = None
    sr = ROOT / "04_scripts" / GAME / ("so_build_" + PROFILE + ".json")
    if sr.is_file():
        try:
            built = json.loads(sr.read_text(encoding="utf-8")).get("source_fingerprint")
        except Exception:
            built = None
    if not built:
        rec("SKIP", "source", "so 构建报告缺 source_fingerprint（旧报告），无法比对；当前源指纹 " + now[:16]); return True
    if built != now:
        rec("FAIL", "source", "源码在构建后变更：构建时 " + built[:16] + " / 现在 " + now[:16] + " => 候选已过期，需重构建后再验"); return False
    rec("PASS", "source", "构建源码 == 当前源码（" + now[:16] + "…），候选未过期")
    return True

def main():
    if not check_base():
        return 1
    if not check_apk():
        return 1
    ok = check_zip()
    ok = check_dex() and ok
    ok = check_dex_reuse() and ok
    ok = check_manifest() and ok
    ok = check_so() and ok
    ok = check_cert() and ok
    ok = check_source_freshness() and ok
    fails = [r for r in rows if r["level"] == "FAIL"]
    payload = {"game": GAME, "version": VER, "profile": PROFILE, "factory_mode": MODE, "apk": str(APK),
               "apk_sha256": sha(APK), "apk_size": APK.stat().st_size,
               "checks": rows, "fail_count": len(fails),
               "verdict": "PASS" if not fails else "FAIL"}
    OUT_REPORT.write_text(json.dumps(payload, ensure_ascii=False, indent=1) + chr(10), encoding="utf-8")
    print("  报告: " + str(OUT_REPORT))
    print("  结论: " + payload["verdict"] + "（PASS=%d FAIL=%d SKIP=%d）"
          % (sum(1 for r in rows if r["level"] == "PASS"), len(fails),
             sum(1 for r in rows if r["level"] == "SKIP")))
    return 0 if not fails else 1

if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception as exc:
        print("  FAIL  exception :: %r" % (exc,))
        sys.exit(1)