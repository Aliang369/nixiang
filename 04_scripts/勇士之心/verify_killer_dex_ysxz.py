#!/usr/bin/env python3
"""勇士之心 killer dex 离线校验（不装机、不跑 ART）。

做的事：把 killer dex 用 baksmali 反汇编成 smali 文本，逐条核对
  1) 13 个类齐全，主类父类 = android.app.Application；
  2) 目标包名已烘焙（bin.mt.signature 的 clinit 里出现该字符串）；
  3) 没有残留 @@ 占位符；
  4) dex 里那段 base64 解出来 == assets/ysxz_cert/ysxz_official_cert.der，且 sha256 == 期望值；
  5) killPM 干了三件事：换 PackageInfo.CREATOR + 清 PackageManager.sPackageInfoCache
     + 清 Parcel.mCreators / sPairedCreators；
  6) API28+ 路径存在：creator 包装类读 PackageInfo.signingInfo ->
     SigningInfo.getApkContentsSigners()（libsecret 的首选取签名方式），并改写 signatures[0]；
  7) 包内不含已禁的 Lcom/starry/magic/sign/SignSpoof;（规范 2026-10-28 删除项）。

用法：
  python3 04_scripts/勇士之心/verify_killer_dex_ysxz.py
  python3 04_scripts/勇士之心/verify_killer_dex_ysxz.py --dex <path> --quiet

退出码：0 = 全过；1 = 有 FAIL。
"""
import argparse
import base64
import hashlib
import re
import subprocess
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent.parent
DEFAULT_DEX = ROOT / "08_output" / "勇士之心" / "killer" / "ysxz_signature_killer.dex"
DEFAULT_CERT = HERE / "assets" / "ysxz_cert" / "ysxz_official_cert.der"
BAKSMALI_JAR = ROOT / "01_tools" / "smali" / "baksmali-fat.jar"
PKG = "com.fightingstars.games.warrior"
EXPECT_CERT_SHA256 = "de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a"
MAIN = "bin/mt/signature/KillerApplication193.smali"
WRAP = "bin/mt/signature/KillerApplication193$l.smali"

fails = []
oks = []


def check(cond, label, detail=""):
    if cond:
        oks.append(label)
        return True
    fails.append("%s %s" % (label, detail))
    return False


def baksmali(dex, outdir):
    cmd = ["java", "-jar", str(BAKSMALI_JAR), "d", "-o", str(outdir), str(dex)]
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    if r.returncode != 0:
        raise SystemExit("ERROR: baksmali 失败:\n%s\n%s" % (r.stdout, r.stderr))
    return outdir


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dex", default=str(DEFAULT_DEX))
    ap.add_argument("--cert-der", default=str(DEFAULT_CERT))
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args()

    dex = Path(a.dex)
    check(dex.is_file(), "dex 存在", str(dex))
    if not dex.is_file():
        sys.exit(1)

    with tempfile.TemporaryDirectory() as tmp:
        root = baksmali(dex, Path(tmp) / "smali")
        classes = sorted(p.name for p in (root / "bin" / "mt" / "signature").glob("*.smali"))
        check(len(classes) == 13, "13 个 killer 类齐全", "实际 %d: %s" % (len(classes), classes))

        main_s = (root / MAIN).read_text(encoding="utf-8")
        check(".super Landroid/app/Application;" in main_s, "主类父类 = Application")
        check('const-string v1, "%s"' % PKG in main_s, "包名已烘焙 = %s" % PKG)
        check("@@" not in main_s, "无 @@ 残留占位符")

        # 证书：dex 里的 base64 字面量 -> DER -> sha256
        b64_lit = re.search(r'const-string v0, "([A-Za-z0-9+/=\\n]{200,})"', main_s)
        if check(b64_lit is not None, "找到证书 base64 字面量"):
            raw = b64_lit.group(1).replace("\\n", "")
            der_bytes = base64.b64decode(raw)
            got = hashlib.sha256(der_bytes).hexdigest()
            check(got == EXPECT_CERT_SHA256, "dex 内证书 DER sha256 == 官方", got)
            ref = Path(a.cert_der).read_bytes() if Path(a.cert_der).is_file() else b""
            check(der_bytes == ref, "dex 内 DER 与 assets/ysxz_cert 那份逐字节相同",
                  "dex %d B vs 参考 %d B" % (len(der_bytes), len(ref)))

        # killPM 三件事
        check('"CREATOR"' in main_s and "Ljava/lang/reflect/Field;->set" in main_s,
              "换 PackageInfo.CREATOR（反射置空静态字段）")
        check('"sPackageInfoCache"' in main_s, "清 PackageManager.sPackageInfoCache")
        check('"mCreators"' in main_s and '"sPairedCreators"' in main_s,
              "清 Parcel.mCreators / sPairedCreators")
        check('KillerApplication193$l;' in main_s, "killPM 使用 creator 包装类 l")

        wrap = (root / WRAP).read_text(encoding="utf-8")
        check('PackageInfo;->signatures:[Landroid/content/pm/Signature;' in wrap,
              "包装类改写 signatures[0]（API<28 主路径）")
        k_s = (root / "bin/mt/signature/KillerApplication193$k.smali").read_text(encoding="utf-8")
        check('PackageInfo;->signingInfo:Landroid/content/pm/SigningInfo;' in k_s,
              "读 PackageInfo.signingInfo（API>=28 入口）")
        check('SigningInfo;->getApkContentsSigners' in k_s,
              "调用 SigningInfo.getApkContentsSigners()（libsecret 首选路径）")
        check('packageName' in wrap and 'equals' in wrap,
              "仅对目标包名生效（其余应用不误伤）")

        # 规范：已删类不得复活
        banned = 'Lcom/starry/magic/sign/SignSpoof;'
        blob = dex.read_bytes()
        check(banned.encode() not in blob, "包内无 Lcom/starry/magic/sign/SignSpoof;（2026-10-28 已删）")

    if not a.quiet:
        for o in oks:
            print("  PASS  " + o)
        for f in fails:
            print("  FAIL  " + f)
        print("")
        print("结果：%d PASS / %d FAIL" % (len(oks), len(fails)))
        print("未验证（需真机，本脚本不覆盖）：")
        print("  - 隐藏 API 豁免（VMRuntime.setHiddenApiExemptions）在目标 Android 版本是否成功")
        print("  - PackageInfo.CREATOR 静态 final 字段在 ART 上是否可改（Android 14+ 风险点）")
        print("  - 实际 PackageManager 查询是否返回官方指纹（现场看 logcat [secure] runtime sig SHA-256=）")
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
