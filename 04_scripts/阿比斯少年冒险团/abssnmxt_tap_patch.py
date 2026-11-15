#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""阿比斯少年冒险团 (com.wks.abyss 2.36 / vc236, TapTap 渠道) — TapTap「改包法」dex 补丁。

依据：skill/re-analyze/login/taptap-web-login.md 顶部「现行口径（2026-11-11）」
      + skill/starry-mod/SKILL.md「TapTap 接入检查表」条目 1/2/3/4
      + 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md
路线判定（证据见 10_docs/阿比斯少年冒险团/03_TapTap路线判定.md）：
      TapSDK 明文在 classes2.dex（com/taptap/sdk/** 2647 处、InitializerServiceImpl /
      TapGatekeeperManager / TapGatekeeperKt / LoginActivityImpl / ClientLoginHandler / TapSignKit /
      TapTapKitInitProvider 全部可直接搜到）⇒ **改包法**（静态改 dex），不走运行时 SPI。
      铁律：SignSpoof 系列禁止复活；本脚本不做任何运行时签名伪装（无 PackageManager hook、无包名白名单）。

补丁点（全部在本包实测锚点，跨版本必须现取，不许抄别的包）：
  P1 com/taptap/sdk/initializer/service/InitializerServiceImpl;->checkInitialize()Z          恒 true
  P2 com/taptap/sdk/initializer/gate/TapGatekeeperManager;->isSuccess()Z                     恒 true
  P3 com/taptap/sdk/initializer/gate/TapGatekeeperKt;->showGatekeeperError(...)Job           → 返回 null（掐弹窗/日志）
  P4 com/taptap/sdk/login/internal/LoginActivityImpl;->onCreate(Activity)V                   → 直接 fallbackToWeb 后 return
  P5 com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler;->canHandle(...)Z       恒 false（纵深防御）
  P6 com/taptap/sdk/kit/internal/utils/TapSignKit;->getSign(Context)String                    → 官方证书 MD5（可 --no-sign 关闭）

对外入口（供 T3 出包脚本调用）：
  A) 就地打补丁： patch --dec-dir <apktool 目录>
  B) 成品验收：   verify --apk <成品.apk>
两个入口都幂等：重复跑 patch 不会重复插入；verify 只读不写。

用法：
  python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py patch \
      --dec-dir 03_decompiled/阿比斯少年冒险团 --base-apk 02_apks/阿比斯少年冒险团/阿比斯少年冒险团_2.36_taptap_original.apk
  python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py patch --dec-dir <dir> --dry-run
  python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py verify --apk 07_patched/阿比斯少年冒险团/xxx.apk
  python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py verify --dec-dir 03_decompiled/阿比斯少年冒险团
  python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py sign-md5 --base-apk <官方底包>

退出码：0 = 全部成功/验收通过；1 = 锚点缺失/不唯一/断言失败（**绝不静默跳过**）。
"""
import argparse
import hashlib
import json
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "阿比斯少年冒险团"
PKG = "com.wks.abyss"
DEFAULT_BASE_APK = ROOT / "02_apks" / GAME / f"{GAME}_2.36_taptap_original.apk"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali-fat.jar"
AAPT2 = ROOT / "01_tools" / "android-sdk" / "build-tools" / "android-15" / "aapt2.exe"

MARK = "# [tapbot T4 改包法]"
# 底包官方证书 MD5（apksigner verify --print-certs 实测，2026-11-15）
OFFICIAL_CERT_MD5 = "87ef7a7df2bf22058004aea8ae4ce837"

# ---------------------------------------------------------------- smali 锚点
P_INIT = "com/taptap/sdk/initializer/service/InitializerServiceImpl.smali"
M_INIT = ".method public checkInitialize()Z"

P_GATE = "com/taptap/sdk/initializer/gate/TapGatekeeperManager.smali"
M_GATE = ".method public final isSuccess()Z"

P_GATEKT = "com/taptap/sdk/initializer/gate/TapGatekeeperKt.smali"
M_GATEKT = (".method public static final showGatekeeperError("
            "Lcom/taptap/sdk/initializer/gate/TapGatekeeperError;)Lkotlinx/coroutines/Job;")

P_LOGIN = "com/taptap/sdk/login/internal/LoginActivityImpl.smali"
M_LOGIN = ".method public final onCreate(Landroid/app/Activity;)V"

P_CLIENT = "com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler.smali"
M_CLIENT = (".method public canHandle(Landroid/app/Activity;"
            "Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z")

P_SIGN = "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
M_SIGN = ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;"


def die(msg):
    print(f"[FAIL] {msg}", file=sys.stderr)
    raise SystemExit(1)


def sha256_bytes(b):
    return hashlib.sha256(b).hexdigest()


def find_smali(dec_root: Path, rel: str) -> Path:
    """在 apktool 目录（smali/ smali_classes2/ ...）或 baksmali 扁平目录里找类文件。"""
    hits = [p for p in dec_root.rglob(Path(rel).name)
            if str(p).replace("\\", "/").endswith(rel)]
    if not hits:
        die(f"类文件缺失：{rel}（dec-dir={dec_root}）")
    if len(hits) > 1:
        die(f"类文件重复 {len(hits)} 份：{rel} -> {[str(h) for h in hits]}")
    return hits[0]


def method_span(text: str, header: str):
    """返回 (start, end) —— 方法头到 .end method（含）。锚点必须唯一。"""
    if text.count(header) != 1:
        die(f"锚点不唯一/缺失（命中 {text.count(header)} 次）：{header}")
    start = text.index(header)
    end = text.find(".end method", start)
    if end < 0:
        die(f"锚点方法没有 .end method：{header}")
    return start, end + len(".end method")


def method_body(text: str, header: str) -> str:
    s, e = method_span(text, header)
    return text[s:e]


def get_registers(body: str) -> str:
    m = re.search(r"^[ \t]*\.registers\s+\d+[ \t]*$", body, re.M)
    if not m:
        die(f"方法体没有 .registers，拒绝盲改：{body.splitlines()[0] if body else '?'}")
    return m.group(0).strip()


def stub_method(header: str, body: str, comment: str, instrs):
    """保留 .registers 原值（改小会 smali 编译失败：requires at least N registers），
    把 .registers 之后的内容整体换成 instrs。"""
    regs = get_registers(body)
    out = [header, "    " + regs, "", f"    {MARK} {comment}", ""]
    for ins in instrs:
        out.append("    " + ins)
    out += ["", ".end method"]
    return "\n".join(out)


def replace_method(text: str, header: str, new_block: str) -> str:
    s, e = method_span(text, header)
    return text[:s] + new_block + text[e:]


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def write(path: Path, text: str, backup_dir: Path, dry: bool):
    """写回；首次改动前留 .orig 原件（重复跑不会覆盖第一份原件）。"""
    if dry:
        return
    if not (backup_dir / (path.name + ".orig")).exists():
        shutil.copy2(path, backup_dir / (path.name + ".orig"))
    path.write_text(text, encoding="utf-8")


# ---------------------------------------------------------------- 各补丁点
def patch_p1(text):
    """验签门闩（业务统一入口）恒放行。"""
    body = method_body(text, M_INIT)
    if MARK in body:
        return text, "skip", "P1 已打过"
    if "TapGatekeeperManager;->isSuccess()Z" not in body:
        die("P1 原实现与预期不符（没有引用 TapGatekeeperManager.isSuccess），锚点需重取")
    new = stub_method(M_INIT, body,
                      "验签门闩恒放行（原实现: TapTapKit.hasInitialized && TapGatekeeperManager.isSuccess）",
                      ["const/4 v0, 0x1", "", "return v0"])
    return replace_method(text, M_INIT, new), "patched", "P1 InitializerServiceImpl.checkInitialize → 恒 true"


def patch_p2(text):
    """Gatekeeper 成功标志恒 true。"""
    body = method_body(text, M_GATE)
    if MARK in body:
        return text, "skip", "P2 已打过"
    if "isGatekeeperSuccess:Z" not in body:
        die("P2 原实现与预期不符（没有读 isGatekeeperSuccess），锚点需重取")
    new = stub_method(M_GATE, body, "门闩恒放行（原实现: return isGatekeeperSuccess）",
                      ["const/4 v0, 0x1", "", "return v0"])
    return replace_method(text, M_GATE, new), "patched", "P2 TapGatekeeperManager.isSuccess → 恒 true"


def patch_p3(text):
    """验签失败弹窗/日志掐掉：showGatekeeperError 直接返回 null（协程不再启动）。"""
    body = method_body(text, M_GATEKT)
    if MARK in body:
        return text, "skip", "P3 已打过"
    if "TapGatekeeperKt$showGatekeeperError$1" not in body:
        die("P3 原实现与预期不符（没有启动 showGatekeeperError$1 协程），锚点需重取")
    new = stub_method(M_GATEKT, body,
                      "掐掉验签失败弹窗（原实现: MainScope 启协程 → Toast + banner + loge）",
                      ["const/4 v0, 0x0", "", "return-object v0"])
    return replace_method(text, M_GATEKT, new), "patched", "P3 TapGatekeeperKt.showGatekeeperError → null"


def patch_p4(text):
    """登录入口恒 Web：onCreate 取到 request 后立刻 fallbackToWeb 并 return，绕过 handler 选择。"""
    body = method_body(text, M_LOGIN)
    if MARK in body:
        return text, "skip", "P4 已打过"
    # 在方法体内定位：check-cast LoginRequest → 紧随其后的 if-eqz v0, :label
    m = re.search(r"^[ \t]*check-cast v0, Lcom/taptap/sdk/login/internal/bean/LoginRequest;[ \t]*$",
                  body, re.M)
    if not m:
        die("P4 原实现与预期不符（onCreate 没有 check-cast LoginRequest），锚点需重取")
    m2 = re.search(r"^[ \t]*if-eqz v0, :(\w+)[ \t]*$", body[m.end():], re.M)
    if not m2:
        die("P4 原实现与预期不符（request 空判分支未找到），锚点需重取")
    ins_at = m.end() + m2.end()
    inject = (
        "\n\n    " + MARK + " 登录入口恒 Web（绕过 Cloud/Client/Phone handler 选择，等同 SDK 自己的 fallbackToWeb）\n"
        "    invoke-virtual {p0, p1, v0}, Lcom/taptap/sdk/login/internal/LoginActivityImpl;->"
        "fallbackToWeb(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)V\n\n"
        "    return-void"
    )
    new_body = body[:ins_at] + inject + body[ins_at:]
    return replace_method(text, M_LOGIN, new_body), "patched", "P4 LoginActivityImpl.onCreate → 直接 fallbackToWeb"


def patch_p5(text):
    """客户端 SSO 兜底关闭（P4 已经绕过，这条防「有人走 handler 循环」）。"""
    body = method_body(text, M_CLIENT)
    if MARK in body:
        return text, "skip", "P5 已打过"
    if "IntentHelper;->getAction" not in body:
        die("P5 原实现与预期不符（canHandle 没有 IntentHelper.getAction），锚点需重取")
    new = stub_method(M_CLIENT, body,
                      "禁止拉起 TapTap 客户端 SSO（原实现: Intent.resolveActivity(com.taptap...) != null）",
                      ["const/4 v0, 0x0", "", "return v0"])
    return replace_method(text, M_CLIENT, new), "patched", "P5 ClientLoginHandler.canHandle → 恒 false"


def patch_p6(text, sign_md5):
    """TapSignKit.getSign 恒返回官方证书 MD5（SDK 上报 package_sign 用）。"""
    body = method_body(text, M_SIGN)
    if MARK in body:
        return text, "skip", "P6 已打过"
    if "PackageInfo;->signatures" not in body:
        die("P6 原实现与预期不符（getSign 没读 PackageInfo.signatures），锚点需重取")
    new = stub_method(M_SIGN, body,
                      f"恒返回官方证书 MD5（原实现: pm.getPackageInfo(pkg,0x40).signatures[0] 的 MD5）",
                      [f'const-string v0, "{sign_md5}"', "", "return-object v0"])
    return replace_method(text, M_SIGN, new), "patched", f"P6 TapSignKit.getSign → {sign_md5}"


# ---------------------------------------------------------------- 断言
def assert_patched(dec_root: Path, sign_md5, require_marker=False):
    """对「已打补丁」的目录/反汇编产物做逐点结构断言；任何一条不过即 die。

    require_marker=False 时只做结构断言 —— dex 里没有注释，成品验收（baksmali 回读）
    看不到 "# [tapbot T4 改包法]" 标记，只能靠指令/引用结构判定。
    """
    problems = []

    def body_of(rel, header):
        p = find_smali(dec_root, rel)
        return method_body(read(p), header)

    def mark_ok(b):
        return (MARK in b) if require_marker else True

    b = body_of(P_INIT, M_INIT)
    if not mark_ok(b) or "const/4 v0, 0x1" not in b or "TapGatekeeperManager;->isSuccess" in b:
        problems.append("P1 InitializerServiceImpl.checkInitialize 未按预期改写")

    b = body_of(P_GATE, M_GATE)
    if not mark_ok(b) or "isGatekeeperSuccess:Z" in b or "const/4 v0, 0x1" not in b:
        problems.append("P2 TapGatekeeperManager.isSuccess 未按预期改写")

    b = body_of(P_GATEKT, M_GATEKT)
    if not mark_ok(b) or "return-object v0" not in b or "showGatekeeperError$1" in b:
        problems.append("P3 TapGatekeeperKt.showGatekeeperError 未按预期改写")

    b = body_of(P_LOGIN, M_LOGIN)
    n = b.count("LoginActivityImpl;->fallbackToWeb")
    if not mark_ok(b) or n != 2 or "return-void" not in b:
        problems.append(f"P4 LoginActivityImpl.onCreate 未按预期插桩（fallbackToWeb 出现 {n} 次，应为 2）")
    elif "LoginHandler;->canHandle" in b and b.index("fallbackToWeb") > b.index("LoginHandler;->canHandle"):
        problems.append("P4 插桩位置在 handler 循环之后（等于没绕过）")

    b = body_of(P_CLIENT, M_CLIENT)
    if not mark_ok(b) or "IntentHelper;->getAction" in b or "const/4 v0, 0x0" not in b:
        problems.append("P5 ClientLoginHandler.canHandle 未按预期改写")

    if sign_md5:
        b = body_of(P_SIGN, M_SIGN)
        if not mark_ok(b) or sign_md5 not in b or "signatures:[Landroid/content/pm/Signature;" in b:
            problems.append("P6 TapSignKit.getSign 未按预期改写")

    if problems:
        for p in problems:
            print(f"[FAIL] {p}", file=sys.stderr)
        die("补丁断言未通过")
    print("  [ok] 6 个补丁点断言全部通过" if sign_md5 else "  [ok] P1–P5 断言通过（P6 跳过）")


def scan_forbidden(dec_root: Path):
    """铁律检查：不得复活 SignSpoof 系列。"""
    bad = []
    for p in dec_root.rglob("*.smali"):
        try:
            t = p.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        if "Lcom/starry/magic/sign/SignSpoof;" in t or "SignSpoofAppComponentFactory" in t:
            bad.append(str(p))
    if bad:
        die(f"检测到 SignSpoof 残留（禁止复活）：{bad[:5]}")
    print("  [ok] 无 Lcom/starry/magic/sign/SignSpoof; 残留")


# ---------------------------------------------------------------- 子命令
def cmd_sign_md5(base_apk: Path) -> str:
    as_ = AAPT2.parent / "apksigner.bat"
    if as_.is_file():
        r = subprocess.run([str(as_), "verify", "--print-certs", str(base_apk)],
                           capture_output=True, text=True, encoding="utf-8", errors="ignore")
        m = re.search(r"certificate MD5 digest:\s*([0-9a-fA-F]{32})", (r.stdout or "") + (r.stderr or ""))
        if m:
            return m.group(1).lower()
    # 兜底：直接从 META-INF/*.RSA 的 PKCS#7 里取第一张证书 DER 算 MD5
    with zipfile.ZipFile(base_apk) as z:
        rsa = [n for n in z.namelist() if n.upper().startswith("META-INF/") and n.upper().endswith(".RSA")]
        if not rsa:
            die("底包没有 META-INF/*.RSA，取不到官方证书")
        der = z.read(rsa[0])
    # PKCS#7: SEQUENCE{ OID signedData, [0] ... certificates } —— 用「长度前缀的 SEQUENCE」粗定位证书
    i = der.find(b"\x30\x82")
    while i >= 0:
        ln = int.from_bytes(der[i + 2:i + 4], "big")
        blob = der[i:i + 4 + ln]
        if 400 < len(blob) < 4000:
            return hashlib.md5(blob).hexdigest()
        i = der.find(b"\x30\x82", i + 1)
    die("无法从 META-INF/*.RSA 解析证书 DER")


def cmd_patch(a):
    dec_root = Path(a.dec_dir).resolve()
    if not dec_root.is_dir():
        die(f"dec-dir 不存在：{dec_root}")

    if a.sign_md5:
        sign_md5 = a.sign_md5.lower()
    elif a.no_sign:
        sign_md5 = None
    else:
        base = Path(a.base_apk)
        if not base.is_file():
            die(f"缺 --base-apk（用于取官方证书 MD5），或显式加 --no-sign：{base}")
        sign_md5 = cmd_sign_md5(base)
        print(f"[0/3] 官方证书 MD5 = {sign_md5}（底包 {base.name}）")
        if sign_md5 != OFFICIAL_CERT_MD5:
            print(f"  [warn] 与记录值 {OFFICIAL_CERT_MD5} 不同；底包换版本则此为正常，需更新本脚本常量")

    backup_dir = dec_root.parent / (dec_root.name + "_tap_backup")
    if not a.dry_run:
        backup_dir.mkdir(parents=True, exist_ok=True)

    print(f"[1/3] 打补丁（dec-dir={dec_root}）")
    manifest = {"dec_dir": str(dec_root), "sign_md5": sign_md5, "patches": []}
    for rel, header, fn in (
        (P_INIT, M_INIT, lambda t: patch_p1(t)),
        (P_GATE, M_GATE, lambda t: patch_p2(t)),
        (P_GATEKT, M_GATEKT, lambda t: patch_p3(t)),
        (P_LOGIN, M_LOGIN, lambda t: patch_p4(t)),
        (P_CLIENT, M_CLIENT, lambda t: patch_p5(t)),
    ) + (((P_SIGN, M_SIGN, (lambda t: patch_p6(t, sign_md5))),) if sign_md5 else ()):
        p = find_smali(dec_root, rel)
        before = read(p)
        after, state, label = fn(before)
        manifest["patches"].append({"class": rel, "method": header, "state": state,
                                    "file": str(p), "sha256_before": sha256_bytes(before.encode()),
                                    "sha256_after": sha256_bytes(after.encode())})
        if state == "patched":
            write(p, after, backup_dir, a.dry_run)
            print(f"  [{state}] {label}")
        else:
            print(f"  [{state}] {label}")

    print("[2/3] 断言")
    if a.dry_run:
        print("  [dry-run] 跳过写盘与断言")
    else:
        assert_patched(dec_root, sign_md5, require_marker=True)
        scan_forbidden(dec_root)

    print("[3/3] 记录")
    if not a.dry_run:
        mp = backup_dir / "patch_manifest.json"
        mp.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"  备份目录：{backup_dir}")
        print(f"  清单：{mp}")
    print("[done] patch 完成。下一步（T3）：apktool b / 外科 zip 组装 → 签名 → "
          "python 本脚本 verify --apk <成品>")
    return 0


def baksmali_apk(apk: Path, tmp: Path) -> Path:
    if not BAKSMALI.is_file():
        die(f"缺 baksmali：{BAKSMALI}")
    with zipfile.ZipFile(apk) as z:
        dexes = sorted(n for n in z.namelist() if re.fullmatch(r"classes\d*\.dex", n))
        if not dexes:
            die("APK 里没有 dex")
        out_root = tmp / "smali_all"
        out_root.mkdir(parents=True, exist_ok=True)
        for n in dexes:
            dex = tmp / n
            dex.write_bytes(z.read(n))
            od = out_root / n.replace(".dex", "")
            r = subprocess.run(["java", "-jar", str(BAKSMALI), "d", "-o", str(od), str(dex)],
                               capture_output=True, text=True, encoding="utf-8", errors="ignore")
            if r.returncode != 0:
                die(f"baksmali 失败：{n}\n{r.stderr[-2000:]}")
    return out_root


def cmd_verify(a):
    fails = 0
    print("== 验收断言 ==")
    if a.apk:
        apk = Path(a.apk).resolve()
        if not apk.is_file():
            die(f"APK 不存在：{apk}")
        with tempfile.TemporaryDirectory(dir=str(ROOT / "07_tmp") if (ROOT / "07_tmp").is_dir() else None) as td:
            tmp = Path(td)
            print(f"[1/3] 反汇编成品 dex（{apk.name}）")
            dec_root = baksmali_apk(apk, tmp)
            print("[2/3] 断言")
            try:
                assert_patched(dec_root, a.sign_md5 or OFFICIAL_CERT_MD5)
                scan_forbidden(dec_root)
            except SystemExit:
                fails += 1
            print("[3/3] Manifest 检查")
            raw = apk.read_bytes()
            if b"SignSpoof" in raw:
                print("[FAIL] APK 内出现 SignSpoof 字样", file=sys.stderr)
                fails += 1
            else:
                print("  [ok] APK 内无 SignSpoof")
    else:
        dec_root = Path(a.dec_dir).resolve()
        print(f"[1/2] 断言（目录 {dec_root}）")
        try:
            assert_patched(dec_root, a.sign_md5 or OFFICIAL_CERT_MD5)
            scan_forbidden(dec_root)
        except SystemExit:
            fails += 1

    print(f"\n结果：{'FAIL' if fails else 'PASS'}")
    return 1 if fails else 0


def main():
    ap = argparse.ArgumentParser(description="阿比斯少年冒险团 TapTap 改包法补丁/验收")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p1 = sub.add_parser("patch", help="对给定 apktool/baksmali 目录就地打补丁（幂等）")
    p1.add_argument("--dec-dir", default=str(ROOT / "03_decompiled" / GAME))
    p1.add_argument("--base-apk", default=str(DEFAULT_BASE_APK))
    p1.add_argument("--sign-md5", default=None, help="直接指定官方证书 MD5（32 位小写 hex）")
    p1.add_argument("--no-sign", action="store_true", help="不打 P6（TapSignKit.getSign）")
    p1.add_argument("--dry-run", action="store_true")

    p2 = sub.add_parser("verify", help="成品 APK / 目录 验收断言（只读）")
    p2.add_argument("--apk", default=None)
    p2.add_argument("--dec-dir", default=None)
    p2.add_argument("--sign-md5", default=None)

    p3 = sub.add_parser("sign-md5", help="打印底包官方证书 MD5")
    p3.add_argument("--base-apk", default=str(DEFAULT_BASE_APK))

    a = ap.parse_args()
    if a.cmd == "patch":
        return cmd_patch(a)
    if a.cmd == "verify":
        if not a.apk and not a.dec_dir:
            die("verify 需要 --apk 或 --dec-dir")
        return cmd_verify(a)
    if a.cmd == "sign-md5":
        print(cmd_sign_md5(Path(a.base_apk)))
        return 0


if __name__ == "__main__":
    sys.exit(main())
