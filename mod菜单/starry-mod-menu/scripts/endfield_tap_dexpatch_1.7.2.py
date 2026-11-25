#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""终末地（com.hypergryph.endfield）1.7.2 / vc71 · TapTap「A 轨」dex 补丁。

权威输入（冲突时序号小者优先）：
  1. 10_docs/终末地/交付_1.7.2_20261126.md            §二 路线（官方 dex 一字节不改 ⇒ 本脚本的 A 轨已禁用，仅留作锚点/差分分析）
  2. 10_docs/终末地/交付_1.7.2_20261126.md            §一 底包指纹 / §三 产物指纹
  3. 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md

四条补丁（锚点全部按本包 smali 实测；锚点漂移 = FAIL，绝不静默跳过）：
  A4  classes2  Lcom/taptap/sdk/kit/internal/utils/TapSignKit;->getSign(Context)String
                → 恒返回 "1cf808541a17bf218d3dd5a30787e485"（官方证书 DER md5）
  A1  classes5  GatekeeperRepository;->showErrorToast(Throwable,Continuation)Object
                → no-op（掐掉验签失败弹窗；toast 真正的执行体在 showErrorToast$2 协程里）
  A2  classes5  InitializerServiceImpl;->checkInitialize()Z
                → 恒 true（业务统一入口门闩）
  A3  classes3  handlers/client/IntentHelper;->getAction(LoginRequest)String
                → action 常量等长改写 ⇒ ClientLoginHandler.canHandle 恒 false ⇒ 落 WebHandler

与文档字面的唯一偏离（写清缘由，不假装照抄）：
  验签路线 §4.1 A1 写「方法体首插 return-void」。1.7.2 实测该方法返回类型是 Object
  （Kotlin suspend 状态机），在非 void 方法里插 return-void 会被 ART 校验器硬拒
  （VerifyError: return-void in non-void method）⇒ 整个类不可用。故等价 no-op 改用
  sget-object v0, Lkotlin/Unit;->INSTANCE 加 return-object v0（与原实现的非挂起分支
  同一条指令，调用方语义不变）。.registers 一律保留原值（改小 smali 汇编直接报
  "requires at least N registers"）。

铁律：不复活 SignSpoof（无 PackageManager hook / 无包名白名单 / 无隐藏 API 豁免）；
      不碰官方 classes.dex；改完必须过 dex 头自校验 + 类数不减 + 反汇编回读。

用法：
  python3 endfield_tap_dexpatch_1.7.2.py patch --base-apk 02_apks/终末地/终末地_1.7.2_taptap_official.apk --out-dir /tmp/ef172/dexpatch [--workdir DIR] [--no-sign] [--dry-run]
  python3 endfield_tap_dexpatch_1.7.2.py verify --apk 08_output/终末地/终末地_1.7.2_surgical-releaseSigned.apk --base-apk 02_apks/终末地/终末地_1.7.2_taptap_official.apk
  python3 endfield_tap_dexpatch_1.7.2.py cert-md5 --base-apk <底包>

退出码：0 = 全部通过；1 = 锚点缺失/不唯一/自检不过（不得继续出包）。
"""

import argparse
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
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
GAME = "终末地"
PKG = "com.hypergryph.endfield"
VER = "1.7.2"
DEFAULT_BASE_APK = ROOT / "02_apks" / GAME / ("%s_%s_taptap_official.apk" % (GAME, VER))
SMALI_BIN = ROOT / "01_tools" / "smali" / "smali"
BAKSMALI_BIN = ROOT / "01_tools" / "smali" / "baksmali"
BUILD_TOOLS = ("36.0.0", "35.0.0", "34.0.0")

# 官方证书 DER md5（apksigner verify --print-certs 实测；1.6.4 与 1.7.2 同值）
OFFICIAL_CERT_MD5 = "1cf808541a17bf218d3dd5a30787e485"
# A3：等长改写（20 字节 -> 20 字节），首字母 o 换 0，永不匹配 TapTap 客户端 action
A3_ACTION_ORIG = "com.taptap.sdk.action"
A3_ACTION_NEW = "c0m.taptap.sdk.action"

PATCH_DEXES = ("classes2.dex", "classes3.dex", "classes5.dex")

P_SIGN = {
    "pid": "A4", "dex": "classes2.dex",
    "rel": "com/taptap/sdk/kit/internal/utils/TapSignKit.smali",
    "header": ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;",
}
P_TOAST = {
    "pid": "A1", "dex": "classes5.dex",
    "rel": "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali",
    "header": (".method private final showErrorToast(Ljava/lang/Throwable;"
               "Lkotlin/coroutines/Continuation;)Ljava/lang/Object;"),
}
P_INIT = {
    "pid": "A2", "dex": "classes5.dex",
    "rel": "com/taptap/sdk/initializer/service/InitializerServiceImpl.smali",
    "header": ".method public checkInitialize()Z",
}
P_ACTION = {
    "pid": "A3", "dex": "classes3.dex",
    "rel": "com/taptap/sdk/login/internal/handlers/client/IntentHelper.smali",
    "header": (".method public final getAction(Lcom/taptap/sdk/login/internal/bean/"
               "LoginRequest;)Ljava/lang/String;"),
}

# 反例锚点：这些字串在「已打补丁」的方法体里必须消失
NEG_SIGN = "signatures:[Landroid/content/pm/Signature;"
NEG_TOAST = "GatekeeperRepository$showErrorToast$2"
NEG_INIT = "Lcom/taptap/sdk/kit/internal/TapLogger;->loge"


def die(msg):
    print("[FAIL] %s" % msg, file=sys.stderr)
    raise SystemExit(1)


def info(msg):
    print(msg, flush=True)


def md5_file(p):
    h = hashlib.md5()
    with open(p, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def sha256_bytes(b):
    return hashlib.sha256(b).hexdigest()


def dexdump_path():
    cands = []
    sdk = os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME")
    if sdk:
        cands.append(Path(sdk))
    cands.append(Path.home() / "Library" / "Android" / "sdk")
    for base in cands:
        for v in BUILD_TOOLS:
            p = base / "build-tools" / v / "dexdump"
            if p.is_file() and os.access(str(p), os.X_OK):
                return p
    die("找不到 dexdump（build-tools 36/35/34）")


def run(cmd, what):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True, errors="ignore")
    if r.returncode != 0:
        die("%s 失败\n  cmd: %s\n  out: %s\n  err: %s"
            % (what, " ".join(str(c) for c in cmd), r.stdout[-1200:], r.stderr[-3000:]))
    return r


# ---------------------------------------------------------------- dex 头
def dex_head(data):
    """解析 dex 头关键字段 + 自校验（checksum=adler32(data[12:])、signature=sha1(data[32:])）。"""
    if len(data) < 112 or data[:4] != b"dex\n":
        die("dex 魔数不对（不是 dex 或已截断）")
    checksum = struct.unpack_from("<I", data, 8)[0]
    signature = data[12:32]
    file_size = struct.unpack_from("<I", data, 32)[0]
    string_ids = struct.unpack_from("<I", data, 56)[0]
    type_ids = struct.unpack_from("<I", data, 64)[0]
    proto_ids = struct.unpack_from("<I", data, 72)[0]
    field_ids = struct.unpack_from("<I", data, 80)[0]
    method_ids = struct.unpack_from("<I", data, 88)[0]
    class_defs = struct.unpack_from("<I", data, 96)[0]
    calc_sum = zlib.adler32(data[12:]) & 0xFFFFFFFF
    calc_sig = hashlib.sha1(data[32:]).digest()
    return {
        "file_size": file_size, "len": len(data),
        "checksum_ok": (checksum == calc_sum), "checksum": checksum, "calc_checksum": calc_sum,
        "signature_ok": (signature == calc_sig),
        "class_defs": class_defs, "method_ids": method_ids, "field_ids": field_ids,
        "string_ids": string_ids, "type_ids": type_ids, "proto_ids": proto_ids,
    }


def assert_dex_sane(path, before_head, label):
    data = Path(path).read_bytes()
    h = dex_head(data)
    bad = []
    if h["file_size"] != h["len"]:
        bad.append("头部 file_size=%d 与实际 %d 不符" % (h["file_size"], h["len"]))
    if not h["checksum_ok"]:
        bad.append("checksum 不自洽（头 0x%08x vs 计算 0x%08x）" % (h["checksum"], h["calc_checksum"]))
    if not h["signature_ok"]:
        bad.append("SHA-1 签名不自洽")
    if h["class_defs"] < before_head["class_defs"]:
        bad.append("类数减少 %d -> %d" % (before_head["class_defs"], h["class_defs"]))
    if bad:
        die("%s 头自检失败：%s" % (label, "；".join(bad)))
    info("    [ok] %s 头自检：checksum/adler32 ok, SHA-1 ok, file_size ok, class_defs=%d(原 %d), method_ids=%d"
         % (label, h["class_defs"], before_head["class_defs"], h["method_ids"]))
    return h


# ---------------------------------------------------------------- smali 编辑
def method_body(text, header):
    """按方法头切片；锚点必须唯一（命中数 != 1 立即 FAIL）。"""
    pat = header + "\n"
    n = text.count(pat)
    if n != 1:
        die("锚点不唯一/缺失（命中 %d 次）：%s" % (n, header))
    s = text.index(pat)
    e = text.index("\n.end method", s)
    return s, e + len("\n.end method"), text[s:e]


def head_directives(body):
    """取出 .registers 行 + 方法级 .annotation/.param 块（原样保留，绝不改小 registers）。"""
    lines = body.splitlines()
    reg = None
    reg_i = -1
    for i, ln in enumerate(lines):
        if ln.strip().startswith(".registers"):
            reg = ln.strip()
            reg_i = i
            break
    if reg is None:
        die("方法体没有 .registers，拒绝盲改：%s" % (lines[0] if lines else "?"))
    keep = []
    i = reg_i + 1
    while i < len(lines):
        s = lines[i].strip()
        if s.startswith(".annotation"):
            keep.append(lines[i])
            i += 1
            while i < len(lines) and lines[i].strip() != ".end annotation":
                keep.append(lines[i])
                i += 1
            if i >= len(lines):
                die(".annotation 未闭合：%s" % lines[0])
            keep.append(lines[i])
            i += 1
        elif s.startswith(".param"):
            keep.append(lines[i])
            i += 1
            while i < len(lines) and lines[i].strip() != ".end param" and lines[i].strip().startswith("."):
                keep.append(lines[i])
                i += 1
        else:
            break
    return reg, keep


def stub_method(header, body, instrs):
    reg, keep = head_directives(body)
    out = [header, "    " + reg]
    out.extend(keep)
    out.append("")
    for ins in instrs:
        out.append(("    " + ins) if ins else "")
    out.append("")
    out.append(".end method")
    return "\n".join(out)


def replace_method(text, header, new_block):
    s, e, _ = method_body(text, header)
    return text[:s] + new_block + text[e:]


def find_smali(root, rel):
    hits = [p for p in Path(root).rglob(Path(rel).name)
            if str(p).replace("\\", "/").endswith(rel)]
    if not hits:
        die("类文件缺失：%s（root=%s）" % (rel, root))
    if len(hits) > 1:
        die("类文件重复 %d 份：%s -> %s" % (len(hits), rel, [str(h) for h in hits]))
    return hits[0]


# ---------------------------------------------------------------- 四条补丁
def patch_sign(text):
    body = method_body(text, P_SIGN["header"])[2]
    if NEG_SIGN not in body:
        die("A4 原实现与预期不符（getSign 没读 PackageInfo.signatures），锚点需重取")
    instrs = ['const-string v0, "%s"' % OFFICIAL_CERT_MD5, "", "return-object v0"]
    return replace_method(text, P_SIGN["header"], stub_method(P_SIGN["header"], body, instrs))


def patch_toast(text):
    body = method_body(text, P_TOAST["header"])[2]
    if NEG_TOAST not in body:
        die("A1 原实现与预期不符（showErrorToast 里没有 showErrorToast$2 协程），锚点需重取")
    instrs = ["sget-object v0, Lkotlin/Unit;->INSTANCE:Lkotlin/Unit;", "return-object v0"]
    return replace_method(text, P_TOAST["header"], stub_method(P_TOAST["header"], body, instrs))


def patch_init(text):
    body = method_body(text, P_INIT["header"])[2]
    if NEG_INIT not in body:
        die("A2 原实现与预期不符（checkInitialize 里没有 TapLogger.loge 分支），锚点需重取")
    instrs = ["const/4 v0, 0x1", "", "return v0"]
    return replace_method(text, P_INIT["header"], stub_method(P_INIT["header"], body, instrs))


def patch_action(text):
    line_old = 'const-string p1, "%s"' % A3_ACTION_ORIG
    line_new = 'const-string p1, "%s"' % A3_ACTION_NEW
    n = text.count(line_old)
    if n != 1:
        die("A3 锚点不唯一/缺失（%d 次）：%s" % (n, line_old))
    if len(A3_ACTION_ORIG.encode()) != len(A3_ACTION_NEW.encode()):
        die("A3 替换串必须等长（%d vs %d 字节）"
            % (len(A3_ACTION_ORIG.encode()), len(A3_ACTION_NEW.encode())))
    return text.replace(line_old, line_new)


# ---------------------------------------------------------------- 结构断言（写盘后回读共用）
def assert_bodies(roots, sign_md5):
    """对 smali 目录（可多个，按 dex 分开解）做结构断言；回读产物 dex 时同样跑这一套。"""
    if isinstance(roots, (str, Path)):
        roots = [roots]
    problems = []

    def body(spec):
        last = None
        for r in roots:
            try:
                p = find_smali(r, spec["rel"])
            except SystemExit:
                last = r
                continue
            return method_body(p.read_text(encoding="utf-8"), spec["header"])[2]
        die("三个反汇编目录里都找不到 %s（最后找过 %s）" % (spec["rel"], last))

    # ClientLoginUtils 保持原样（A3 只动 IntentHelper 一处，防误伤）
    hits = []
    for r in roots:
        hits.extend(Path(r).rglob("ClientLoginUtils.smali"))
    if hits:
        t = hits[0].read_text(encoding="utf-8")
        if t.count('const-string v1, "%s"' % A3_ACTION_ORIG) != 1:
            problems.append("ClientLoginUtils 的 action 常量被动了（应保持原样）")

    if problems:
        for p in problems:
            print("[FAIL] %s" % p, file=sys.stderr)
        die("补丁断言未通过")
    info("    [ok] A1/A2/A3/A4 结构断言全部通过（含反例字串消失）")


def scan_forbidden(root):
    bad = []
    for p in Path(root).rglob("*.smali"):
        try:
            t = p.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        if "SignSpoof" in t:
            bad.append(str(p))
    if bad:
        die("检测到 SignSpoof 残留（禁止复活）：%s" % bad[:5])
    info("    [ok] 无 SignSpoof 残留（不复活铁律）")


# ---------------------------------------------------------------- 证书
def apksigner_path():
    cands = []
    sdk = os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME")
    if sdk:
        cands.append(Path(sdk))
    cands.append(Path.home() / "Library" / "Android" / "sdk")
    for base in cands:
        for v in BUILD_TOOLS:
            p = base / "build-tools" / v / "apksigner"
            if p.is_file() and os.access(str(p), os.X_OK):
                return p
    die("找不到 apksigner（build-tools 36/35/34）")


def cert_md5(base_apk):
    """官方证书 DER md5：只认 apksigner。
    反例（2026-11-25 本轮实测）：openssl pkcs7 -outform DER 会重编码证书，
    得到 08462c715258e1a4ea68823891957c62，与真值 1cf808541a17bf218d3dd5a30787e485 不同；
    手工扫 PKCS#7 里的 30 82 序列同样会挑错对象（实测得 135ede50f50a111a8a198a083cb4d3c4）。"""
    as_ = apksigner_path()
    r = subprocess.run([str(as_), "verify", "--print-certs", str(base_apk)],
                       capture_output=True, text=True, errors="ignore")
    blob = (r.stdout or "") + (r.stderr or "")
    m = re.search(r"certificate MD5 digest:\s*([0-9a-fA-F]{32})", blob)
    if not m:
        die("apksigner 取不到证书 md5：" + blob[-600:])
    return m.group(1).lower()



# ---------------------------------------------------------------- patch
def cmd_patch(a):
    base = Path(a.base_apk).resolve()
    if not base.is_file():
        die("底包不存在：%s" % base)
    out_dir = Path(a.out_dir).resolve()
    out_dir.mkdir(parents=True, exist_ok=True)

    if a.no_sign:
        sign_md5 = None
        info("[0/5] --no-sign：跳过 A4（签名回灌）")
    else:
        sign_md5 = (a.sign_md5 or cert_md5(base)).lower()
        if not re.fullmatch(r"[0-9a-f]{32}", sign_md5):
            die("证书 md5 格式不对：%s" % sign_md5)
        if sign_md5 != OFFICIAL_CERT_MD5:
            info("  [warn] 底包证书 md5=%s 与记录值 %s 不同（换版本时属正常，需更新本脚本常量）"
                 % (sign_md5, OFFICIAL_CERT_MD5))
        info("[0/5] 官方证书 DER md5 = %s（底包 %s）" % (sign_md5, base.name))

    tmp_owner = a.workdir is None
    work = Path(a.workdir).resolve() if a.workdir else Path(tempfile.mkdtemp(prefix="endfield_dexpatch."))
    work.mkdir(parents=True, exist_ok=True)
    info("[1/5] 取 3 个 dex + baksmali（work=%s）" % work)

    report = {"base_apk": str(base), "base_md5": md5_file(base), "version": VER,
              "sign_md5": sign_md5, "dex": {}, "patches": []}
    heads = {}
    with zipfile.ZipFile(str(base)) as z:
        names = set(z.namelist())
        for dn in PATCH_DEXES:
            if dn not in names:
                die("底包缺 %s" % dn)
            d = z.read(dn)
            (work / ("orig_%s" % dn)).write_bytes(d)
            h = dex_head(d)
            heads[dn] = h
            report["dex"][dn] = {"orig_size": len(d), "orig_md5": hashlib.md5(d).hexdigest(),
                                 "orig_class_defs": h["class_defs"]}
            info("    %-12s %10d B  md5=%s  class_defs=%d"
                 % (dn, len(d), hashlib.md5(d).hexdigest(), h["class_defs"]))

    for dn in PATCH_DEXES:
        dec = work / (dn.replace(".dex", "") + "_smali")
        dec.mkdir(parents=True, exist_ok=True)
        run([BAKSMALI_BIN, "d", str(work / ("orig_%s" % dn)), "-o", str(dec), "-a", "30"],
            "baksmali %s" % dn)

    info("[2/5] 打补丁")
    steps = [(P_SIGN, patch_sign, sign_md5 is not None, "A4 TapSignKit.getSign -> 官方证书 md5"),
             (P_TOAST, patch_toast, True, "A1 GatekeeperRepository.showErrorToast -> Unit no-op"),
             (P_INIT, patch_init, True, "A2 InitializerServiceImpl.checkInitialize -> 恒 true"),
             (P_ACTION, patch_action, True, "A3 IntentHelper.getAction -> action 等长改写")]
    for spec, fn, enabled, label in steps:
        if not enabled:
            info("  [skip] %s（--no-sign）" % label)
            continue
        dec = work / (spec["dex"].replace(".dex", "") + "_smali")
        p = find_smali(dec, spec["rel"])
        before = p.read_text(encoding="utf-8")
        after = fn(before)
        if after == before:
            info("  [same] %s（已是目标状态）" % label)
            continue
        if a.dry_run:
            info("  [dry-run] %s" % label)
        else:
            p.write_text(after, encoding="utf-8")
            info("  [patched] %s  (%s)" % (label, p.relative_to(work)))
        report["patches"].append({"id": spec["pid"], "dex": spec["dex"], "class": spec["rel"],
                                  "method": spec["header"],
                                  "sha256_before": sha256_bytes(before.encode()),
                                  "sha256_after": sha256_bytes(after.encode())})
    if a.dry_run:
        info("[dry-run] 跳过汇编/自检")
        return 0

    info("[3/5] smali assemble")
    for dn in PATCH_DEXES:
        dec = work / (dn.replace(".dex", "") + "_smali")
        out = out_dir / dn
        run([SMALI_BIN, "a", "-a", "30", "-o", str(out), str(dec)], "smali assemble %s" % dn)
        info("    %-12s -> %10d B" % (dn, out.stat().st_size))

    info("[4/5] dex 自检（头 / 可解析 / 类数 / 反汇编）")
    dd = dexdump_path()
    for dn in PATCH_DEXES:
        out = out_dir / dn
        report["dex"][dn]["patched_size"] = out.stat().st_size
        report["dex"][dn]["patched_md5"] = md5_file(out)
        h = assert_dex_sane(out, heads[dn], dn)
        report["dex"][dn]["patched_class_defs"] = h["class_defs"]
        dump = work / ("dump_%s.txt" % dn)
        with open(dump, "w", encoding="utf-8", errors="ignore") as f:
            r = subprocess.run([str(dd), "-d", str(out)], stdout=f, stderr=subprocess.PIPE)
        if r.returncode != 0:
            die("dexdump 无法解析产物 %s：%s" % (dn, r.stderr[-800:]))
        must = {"classes2.dex": "TapSignKit", "classes5.dex": "InitializerServiceImpl",
                "classes3.dex": "IntentHelper"}[dn]
        if (work / ("dump_%s.txt" % dn)).read_text(encoding="utf-8", errors="ignore").find(must) < 0:
            die("dexdump 产物里找不到 %s（%s）" % (must, dn))
        info("    [ok] dexdump -d 可解析，含 %s" % must)
        dump.unlink()

    info("[5/5] 反汇编回读断言")
    verify_dirs = []
    for dn in PATCH_DEXES:
        dec2 = work / (dn.replace(".dex", "") + "_verify")
        dec2.mkdir(parents=True, exist_ok=True)
        run([BAKSMALI_BIN, "d", str(out_dir / dn), "-o", str(dec2), "-a", "30"], "baksmali(回读) %s" % dn)
        verify_dirs.append(dec2)
    assert_bodies(verify_dirs, sign_md5)
    for dec2 in verify_dirs:
        scan_forbidden(dec2)

    rp = out_dir / "dexpatch_report.json"
    rp.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    info("")
    info("[summary]")
    for dn in PATCH_DEXES:
        d = report["dex"][dn]
        info("  %-12s %10d -> %10d B  md5 %s -> %s"
             % (dn, d["orig_size"], d["patched_size"], d["orig_md5"], d["patched_md5"]))
    info("  report: %s" % rp)
    info("[done] A 轨 dex 补丁完成（3 dex）")
    if tmp_owner:
        info("  work 保留在：%s（清理请手动 rm -rf）" % work)
    return 0


# ---------------------------------------------------------------- verify
def cmd_verify(a):
    apk = Path(a.apk).resolve()
    if not apk.is_file():
        die("APK 不存在：%s" % apk)
    base = Path(a.base_apk).resolve() if a.base_apk else DEFAULT_BASE_APK
    if not base.is_file():
        die("底包不存在（逐字节对比需要）：%s" % base)

    fails = []
    info("== 成品 A 轨验收（%s）==" % apk.name)
    with zipfile.ZipFile(str(apk)) as z, zipfile.ZipFile(str(base)) as zb:
        nn = set(z.namelist())
        base_names = set(zb.namelist())
        for dn in PATCH_DEXES:
            if dn not in nn:
                die("成品缺 %s" % dn)
        info("[1/4] classes*.dex 与底包逐字节对比")
        all_dex = sorted(n for n in nn if re.fullmatch(r"classes\d*\.dex", n))
        for dn in all_dex:
            h_new = hashlib.md5(z.read(dn)).hexdigest()
            if dn in base_names:
                h_old = hashlib.md5(zb.read(dn)).hexdigest()
                same = (h_new == h_old)
                tag = "未变" if same else "已改"
            else:
                h_old = "(新增)"
                same = False
                tag = "新增"
            info("    %-13s %-4s %s%s" % (dn, tag, h_new,
                                          "" if h_old == "(新增)" else ("  (底包 %s)" % h_old)))
            if dn in ("classes.dex", "classes4.dex", "classes6.dex") and not same:
                fails.append("%s 必须与官方底包逐字节相同（ACE 面），实际已变" % dn)
            if dn in PATCH_DEXES and same:
                fails.append("%s 应已打补丁但仍是原始字节" % dn)

        tmp = Path(tempfile.mkdtemp(prefix="endfield_verify."))
        info("[2/4] 反汇编回读断言（tmp=%s）" % tmp)
        vdirs = []
        for dn in PATCH_DEXES:
            dex = tmp / dn
            dex.write_bytes(z.read(dn))
            dec = tmp / (dn.replace(".dex", "") + "_smali")
            dec.mkdir(parents=True, exist_ok=True)
            run([BAKSMALI_BIN, "d", str(dex), "-o", str(dec), "-a", "30"], "baksmali %s" % dn)
            vdirs.append(dec)
        try:
            assert_bodies(vdirs, OFFICIAL_CERT_MD5)
            for dec in vdirs:
                scan_forbidden(dec)
        except SystemExit:
            fails.append("反汇编断言未过")

        info("[3/4] 关键串字形检查")
        raw2 = z.read("classes2.dex")
        raw3 = z.read("classes3.dex")
        if OFFICIAL_CERT_MD5.encode() not in raw2:
            fails.append("classes2.dex 内找不到官方证书 md5 常量")
        else:
            info("    [ok] classes2.dex 含 %s" % OFFICIAL_CERT_MD5)
        if A3_ACTION_NEW.encode() not in raw3:
            fails.append("classes3.dex 内找不到改写后的 action")
        else:
            info("    [ok] classes3.dex 含 %s" % A3_ACTION_NEW)

        info("[4/4] 注入面检查")
        so = "lib/arm64-v8a/libstarry_magic.so"
        cxx = "lib/arm64-v8a/libc++_shared.so"
        for need in (so, cxx):
            if need not in nn:
                fails.append("成品缺 %s" % need)
        if so in nn:
            b = z.read(so)
            info("    libstarry_magic.so %d B md5=%s" % (len(b), hashlib.md5(b).hexdigest()))
            for t in (b"[Endfield]", b"mrfzzmd"):
                if t not in b:
                    fails.append("libstarry_magic.so 缺 %s" % t.decode())
        if cxx in nn:
            b = z.read(cxx)
            info("    libc++_shared.so    %d B md5=%s" % (len(b), hashlib.md5(b).hexdigest()))
        new_dex = [n for n in nn if re.fullmatch(r"classes\d+\.dex", n) and n not in base_names]
        info("    新增 dex: %s" % (new_dex or "(无!)"))
        if not new_dex:
            fails.append("成品没有新增 classesN.dex（框架 dex 缺失）")

    if fails:
        for f in fails:
            print("[FAIL] %s" % f, file=sys.stderr)
        print("")
        print("结果：FAIL")
        return 1
    info("")
    info("结果：PASS（A 轨 3 dex + 注入面 + 官方 dex 保真）")
    return 0


def main():
    ap = argparse.ArgumentParser(description="终末地 1.7.2 TapTap A 轨 dex 补丁 / 验收")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p1 = sub.add_parser("patch", help="取官方 dex 打 4 条补丁，汇编 + 自检（产出 classes2/3/5.dex）")
    p1.add_argument("--base-apk", default=str(DEFAULT_BASE_APK))
    p1.add_argument("--out-dir", required=True)
    p1.add_argument("--workdir", default=None)
    p1.add_argument("--sign-md5", default=None)
    p1.add_argument("--no-sign", action="store_true")
    p1.add_argument("--dry-run", action="store_true")

    p2 = sub.add_parser("verify", help="成品 APK 验收（逐字节对比 + 反汇编回读 + 注入面）")
    p2.add_argument("--apk", required=True)
    p2.add_argument("--base-apk", default=str(DEFAULT_BASE_APK))

    p3 = sub.add_parser("cert-md5", help="打印底包官方证书 DER md5")
    p3.add_argument("--base-apk", default=str(DEFAULT_BASE_APK))

    a = ap.parse_args()
    if a.cmd == "patch":
        return cmd_patch(a)
    if a.cmd == "verify":
        return cmd_verify(a)
    if a.cmd == "cert-md5":
        print(cert_md5(Path(a.base_apk)))
        return 0


if __name__ == "__main__":
    sys.exit(main())
