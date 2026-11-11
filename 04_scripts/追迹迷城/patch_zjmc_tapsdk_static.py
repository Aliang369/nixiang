#!/usr/bin/env python3
# script-policy: {"purpose":"追迹迷城 1.0.20261111 TapSDK 静态改包法补丁：classes4.dex 三处最小补丁 + 门闩调用链正向断言","inputs":"baksmali 展开的 classes4.dex 目录","outputs":"原地改写 smali（由外科脚本 smali assemble 回 dex）","boundary":"只改三处方法体；禁止改 TapSignKit（同一链条禁双修）；禁止引入 killer/SignSpoof","prior_art":"04_scripts/追迹迷城/patch_zjmc_smali.py（旧 Activity 注入/ClientLoginUtils 改法，本版不用其 TapTap 部分）","difference":"结构化锚点改 classes4 三处：门闩恒 true、弹窗 return-void、provide 恒 WEB","evidence":"10_docs/追迹迷城/路线锁定.md#单轨"}
# 追迹迷城 1.0.20261111 — TapSDK 静态改包法最小补丁（单轨：不注入 killer、不做运行时签名伪装）
#
# 依据 skill/re-analyze/login/taptap-web-login.md「现行口径」(2026-11-11) 的改包法四步，本文件做 1–3：
#   1) 验签/初始化门闩恒放行：GatekeeperRepository.isGatekeeperLoadedSuccessfully()Z → const/4 v0, 0x1
#   2) 验签失败弹窗掐掉   ：GatekeeperRepository.showErrorToast(TapNetworkException)V → return-void
#   3) 登录入口恒 Web     ：DefaultTapLoginFragmentProvider.provide 的 aget v2, v3, v2 之后 const/4 v2, 0x3
#   第 4 步（不得挂 SignSpoof/StarryAppComponentFactory 工厂）由 Manifest v5 + pre_install_gate 查12 机器校验。
#
# 锚点铁律：一律按「类名 + 方法签名 + 控制流」结构化匹配，**不跨版本抄文件偏移或寄存器号**。
#   上层调用链（正向断言，运行前先核）：InitializerServiceImpl.checkInitialize(Z)
#     → TapTapSdkInitializer.isGatekeeperLoadedSuccessfully() → GatekeeperManager.isGatekeeperLoadedSuccessfully()
#     → GatekeeperRepository.isGatekeeperLoadedSuccessfully() → 字段 loadGatekeeperSuccess
#   全树只有 GatekeeperRepository 自己 iget/iput 该字段 ⇒ 改 getter 即覆盖整条链。
#
# TapSignKit（本版路径 com/taptap/sdk/kit/internal/utils/TapSignKit）**保持官方原样**：
#   它只被 GatekeeperRepository$initialize$1$networkData$result$1 用来算上报摘要，门闩已恒放行 ⇒
#   同一链条禁双修（改了它等于两个变量一起动，失效无法归因）。
#
# 用法: python3 patch_zjmc_tapsdk_static.py <baksmali 输出目录（含 com/taptap/...）>
import re
import sys
from pathlib import Path

GATE_CLASS = "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
PROVIDER_CLASS = "com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
GATE_SIG = "isGatekeeperLoadedSuccessfully()Z"
TOAST_SIG = "showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V"
PROVIDE_SIG = "provide(Lcom/taptap/sdk/login/impl/data/model/LoginRequest;)Landroid/app/Fragment;"

METHOD_RE = re.compile(r"^\.method ([^\n]+)\n(.*?)^\.end method\n", re.M | re.S)


def fail(msg):
    raise SystemExit("ERROR: " + msg)


def method_blocks(text):
    """返回 [(header_line, body_text, match_start, match_end)]。"""
    out = []
    for m in METHOD_RE.finditer(text):
        out.append((m.group(1), m.group(2), m.start(), m.end()))
    return out


def find_block(text, sig, cls):
    hits = [b for b in method_blocks(text) if sig in b[0]]
    if len(hits) != 1:
        fail("%s 中 %s 命中 %d 处（期望 1）——底包结构变了，禁按旧锚点硬改"
             % (cls, sig, len(hits)))
    return hits[0]


def replace_body(text, sig, cls, new_body):
    header, body, _s, _e = find_block(text, sig, cls)
    reg = re.search(r"^    \.(registers|locals) (\d+)$", body, re.M)
    if not reg:
        fail("%s 的 %s 无 .registers/.locals 行" % (cls, sig))
    head = "    .%s %s\n\n" % (reg.group(1), reg.group(2))
    new_block = ".method %s\n%s%s.end method\n" % (header, head, new_body)
    start = _s
    old_block = text[_s:_e]
    if old_block.startswith(".method " + header):
        return text[:start] + new_block + text[_e:]
    fail("%s 的 %s 块重建失败" % (cls, sig))


def patch_gate(root: Path) -> None:
    path = root / GATE_CLASS
    if not path.is_file():
        fail("缺少 " + str(path))
    text = path.read_text(encoding="utf-8")

    # 幂等：已经是常量 true 就不重复打
    if "const/4 v0, 0x1" in text and "isGatekeeperLoadedSuccessfully" in text:
        blk = [b for b in method_blocks(text) if GATE_SIG in b[0]]
        if blk and "loadGatekeeperSuccess:Z" not in blk[0][1]:
            print("  [skip] isGatekeeperLoadedSuccessfully 已是恒 true 形态")
        else:
            text = replace_body(text, GATE_SIG, GATE_CLASS,
                                "    # starry singletrack: 门闩恒放行（改包法第1步）\n"
                                "    const/4 v0, 0x1\n\n    return v0\n")
    else:
        text = replace_body(text, GATE_SIG, GATE_CLASS,
                            "    # starry singletrack: 门闩恒放行（改包法第1步）\n"
                            "    const/4 v0, 0x1\n\n    return v0\n")

    body = [b for b in method_blocks(text) if GATE_SIG in b[0]][0][1]
    if "loadGatekeeperSuccess:Z" in body or "const/4 v0, 0x1" not in body:
        fail("isGatekeeperLoadedSuccessfully 未被改成恒 true")

    # showErrorToast：原样保留 .registers，体换 return-void
    body = [b for b in method_blocks(text) if TOAST_SIG in b[0]][0][1]
    if "return-void" not in body or "TapLogger" in body:
        text = replace_body(text, TOAST_SIG, GATE_CLASS,
                            "    # starry singletrack: 掐掉验签失败弹窗（改包法第2步）\n"
                            "    return-void\n")
    body = [b for b in method_blocks(text) if TOAST_SIG in b[0]][0][1]
    if "TapLogger" in body or "showToast" in body:
        fail("showErrorToast 未被掐掉")

    path.write_text(text, encoding="utf-8")
    print("  [1/3] GatekeeperRepository.isGatekeeperLoadedSuccessfully -> const/4 v0, 0x1")
    print("  [2/3] GatekeeperRepository.showErrorToast(TapNetworkException) -> return-void")


def patch_provider(root: Path) -> None:
    path = root / PROVIDER_CLASS
    if not path.is_file():
        fail("缺少 " + str(path))
    text = path.read_text(encoding="utf-8")
    anchor = "    aget v2, v3, v2\n"
    if text.count(anchor) != 1:
        fail("provide() 的 aget v2, v3, v2 锚点命中 %d 处（期望 1）——底包 TapSDK 结构变了"
             % text.count(anchor))
    after = text.split(anchor, 1)[1][:240]
    if "const/4 v2, 0x3" in after:
        print("  [skip] provide() 已是恒 WEB 形态")
    else:
        # 控制流前置校验：v2==3 必须落到 WebLoginFragment 分支（不是 NoWhenBranchMatched 抛异常）
        if "if-eq v2, v3" not in after or "WebLoginFragment" not in text:
            fail("provide() 控制流与预期不符（找不到 if-eq v2, v3 / WebLoginFragment）")
        text = text.replace(
            anchor,
            anchor + "\n    # starry singletrack: 恒 WEB 登录（改包法第3步）\n    const/4 v2, 0x3\n",
            1)
    path.write_text(text, encoding="utf-8")

    # 正向断言：插入点之后 v2 恒 3，且后续分支表仍是 1/2/3 → Web
    after = [b for b in method_blocks(text) if PROVIDE_SIG in b[0]][0][1].split(anchor, 1)[1][:240]
    if "const/4 v2, 0x3" not in after:
        fail("provide() 的 const/4 v2, 0x3 不在 aget 之后")
    if "if-eq v2, v3" not in after:
        fail("provide() 分支表形态变化")
    print("  [3/3] DefaultTapLoginFragmentProvider.provide -> aget 后 const/4 v2, 0x3 (LoginTypes.WEB)")


def assert_chain(root: Path) -> None:
    """正向断言：门闩调用链完整；全树只有 repository 自己读写 loadGatekeeperSuccess。"""
    users = []
    field_writers = []
    for p in root.rglob("*.smali"):
        try:
            t = p.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue
        if "isGatekeeperLoadedSuccessfully" in t:
            users.append(p.relative_to(root).as_posix())
        if "GatekeeperRepository;->loadGatekeeperSuccess:Z" in t:
            field_writers.append(p.relative_to(root).as_posix())
    need = {
        "com/taptap/sdk/initializer/service/InitializerServiceImpl.smali",
        "com/taptap/sdk/initializer/TapTapSdkInitializer.smali",
        "com/taptap/sdk/initializer/manager/GatekeeperManager.smali",
        GATE_CLASS,
    }
    missing = need - set(users)
    if missing:
        fail("门闩调用链不完整，缺: " + ", ".join(sorted(missing)))
    extra = set(field_writers) - {GATE_CLASS}
    if extra:
        fail("loadGatekeeperSuccess 字段被 repository 之外读写（改 getter 不足以覆盖）: "
             + ", ".join(sorted(extra)))
    print("  [chain] checkInitialize → TapTapSdkInitializer → GatekeeperManager → GatekeeperRepository 全覆盖；"
          "字段仅 repository 自读写")


def verify_patched(root: Path) -> None:
    """只验证形态（成品包回读用）：三处补丁都在，且 TapSignKit 未被改。"""
    gr = (root / GATE_CLASS).read_text(encoding="utf-8")
    gbody = [b for b in method_blocks(gr) if GATE_SIG in b[0]]
    if not gbody or "loadGatekeeperSuccess:Z" in gbody[0][1] or "const/4 v0, 0x1" not in gbody[0][1]:
        fail("isGatekeeperLoadedSuccessfully 不是恒 true")
    tbody = [b for b in method_blocks(gr) if TOAST_SIG in b[0]]
    if not tbody or "TapLogger" in tbody[0][1] or "showToast" in tbody[0][1]:
        fail("showErrorToast 未被掐掉")
    pr = (root / PROVIDER_CLASS).read_text(encoding="utf-8")
    pbody = [b for b in method_blocks(pr) if PROVIDE_SIG in b[0]]
    if not pbody or "const/4 v2, 0x3" not in pbody[0][1].split("aget v2, v3, v2", 1)[1][:200]:
        fail("provide 未强制 LoginTypes.WEB")
    tk = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
    if tk.is_file():
        for needle in ("4684a2b2249e978c74230f8895635894", "b9c07db608080d40b04254d32ac3fc5c"):
            if needle in tk.read_text(encoding="utf-8"):
                fail("TapSignKit 被静态改成官方摘要（同一链条禁双修）")
    print("  [verify] 三处补丁形态正确；TapSignKit 保持官方原样")


def main() -> None:
    args = [a for a in sys.argv[1:]]
    if args and args[0] == "--verify":
        if len(args) != 2:
            fail("usage: patch_zjmc_tapsdk_static.py --verify <smali-root>")
        root = Path(args[1])
        if not root.is_dir():
            fail("not a dir: " + str(root))
        verify_patched(root)
        assert_chain(root)
        return
    if len(sys.argv) != 2:
        fail("usage: patch_zjmc_tapsdk_static.py <smali-root> | --verify <smali-root>")
    root = Path(sys.argv[1])
    if not root.is_dir():
        fail("not a dir: " + str(root))
    patch_gate(root)
    patch_provider(root)
    assert_chain(root)
    print("  TapSignKit 保持官方原样（同一链条禁双修）")


if __name__ == "__main__":
    main()
