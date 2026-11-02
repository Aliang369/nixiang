#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""我的技能无限触发 — 防沉迷/实名门禁「客户端改判」（用户 2026-11-02 点名要求跳过实名）。

背景（取证见 10_docs/我的技能无限触发/环境与底包_20261102.md §七/§八）：
  TapTap「快速认证」＝运营商一键登录，必须走 TapTap 客户端进程 + 蜂窝数据通道，本机无 SIM
  ⇒ 必失败，且改包改不了别的进程；「手动实名」走服务器，可用但用户选择跳过。
  链路：AntiAddictionUIImpl.startup → checkUserAuthIdentify
        → AntiAddictionKit.checkStandaloneCacheAndFetchUserIdentifyState(callback = AntiAddictionUIImpl$7)
        → $7.onSuccess(IdentifyState) 分流：identifyState==0 ⇒ tryLoginAntiAddiction（已实名）
                                          其它 + !hasAuthRecord ⇒ 拉实名 UI（快速认证/手动）
        → tryLoginAntiAddiction → AntiAddictionKit.login → AntiAddictionImpl.login
        → 服务端 identifyStateObservable / checkUserStateSync → $7(onNext) → notify(500) + onSuccess

改判三处（只动本 SDK，官方 dex 其余类不受影响）：
  A) $7.onSuccess 入口强制 IdentifyState = {identifyState=0, ageLimit=18,
     antiAddictionToken="standalone_anti_addiction_token", hasAuthRecord=true}
     ⇒ 永远走「已实名」分支，不再弹实名/快速认证 UI。
  B) $7.onError 整方法改写 ⇒ 实名状态查询失败（服务端无记录就是这条）也直接转 onSuccess(空对象)，
     由 A 强制成已实名。原失败路径会进 checkIfStartStandAloneMode → 实名 UI。
  C) AntiAddictionImpl.login 整方法短路 ⇒ 不请求服务端，直接构造 UserInfo（ageLimit=18、
     remainTime=0x7fffffff、clientId 取 access$800）写入 UserModel，
     notifyAntiAddictionMessage(500)（置 canPlay=true 并回调 Unity）+ loginCallback.onSuccess()。

用法：patch_antiaddiction_wdjnwcf.py <smali 根目录>   （幂等：已含 [starry] 标记则跳过）
"""
import sys
from pathlib import Path

MARK = "[starry]"

NEW_ONERROR = """.method public onError(Ljava/lang/Throwable;)V
    .registers 4

    # [starry] 实名状态查询失败 ⇒ 直接构造 IdentifyState 走成功分支（由 onSuccess 强制成已实名成年）
    new-instance v0, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;

    invoke-direct {v0}, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;-><init>()V

    invoke-virtual {p0, v0}, Lcom/tapsdk/antiaddictionui/AntiAddictionUIImpl$7;->onSuccess(Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;)V

    return-void
.end method"""

NEW_LOGIN = """.method public login(Ljava/lang/String;Ljava/lang/String;ILcom/tapsdk/antiaddiction/AntiAddictionLoginCallback;)V
    # .registers 8：3 个局部 (v0..v2) + 5 个参数 (p0=v3 … p4=v7)。
    # 上一版写成 .registers 7 ⇒ p0 落在 v2，随后 `const/4 v2, 0x0` 把 this 清空 ⇒
    # notifyAntiAddictionMessage 空引用 NPE（2026-11-02 23:02 真机实证）。
    .registers 8

    # [starry] 短路：不做服务端实名核对，直接按「成年人 + 已实名」回调
    new-instance v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;

    invoke-direct {v0}, Lcom/tapsdk/antiaddiction/entities/UserInfo;-><init>()V

    iput-object p1, v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;->userId:Ljava/lang/String;

    iput-object p2, v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;->accessToken:Ljava/lang/String;

    const/16 v1, 0x12

    iput v1, v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;->ageLimit:I

    const v1, 0x7fffffff

    iput v1, v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;->remainTime:I

    invoke-static {p0}, Lcom/tapsdk/antiaddiction/AntiAddictionImpl;->access$800(Lcom/tapsdk/antiaddiction/AntiAddictionImpl;)Ljava/lang/String;

    move-result-object v1

    iput-object v1, v0, Lcom/tapsdk/antiaddiction/entities/UserInfo;->clientId:Ljava/lang/String;

    invoke-static {p0}, Lcom/tapsdk/antiaddiction/AntiAddictionImpl;->access$000(Lcom/tapsdk/antiaddiction/AntiAddictionImpl;)Lcom/tapsdk/antiaddiction/models/UserModel;

    move-result-object v1

    invoke-virtual {v1, v0}, Lcom/tapsdk/antiaddiction/models/UserModel;->setCurrentUser(Lcom/tapsdk/antiaddiction/entities/UserInfo;)V

    const/16 v1, 0x1f4

    const/4 v2, 0x0

    invoke-virtual {p0, v1, v2}, Lcom/tapsdk/antiaddiction/AntiAddictionImpl;->notifyAntiAddictionMessage(ILjava/util/Map;)V

    invoke-interface {p4}, Lcom/tapsdk/antiaddiction/AntiAddictionLoginCallback;->onSuccess()V

    return-void
.end method"""

ONSUCCESS_ANCHOR = """    .registers 11

    .line 430
    iget v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->identifyState:I
"""

ONSUCCESS_FORCED = """    .registers 11

    # [starry] 强制「已实名 + 成年人」：identifyState=0 / ageLimit=18 / 本地实名 token / hasAuthRecord
    const/4 v0, 0x0

    iput v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->identifyState:I

    const/16 v0, 0x12

    iput v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->ageLimit:I

    const-string v0, "standalone_anti_addiction_token"

    iput-object v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->antiAddictionToken:Ljava/lang/String;

    const/4 v0, 0x1

    iput-boolean v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->hasAuthRecord:Z

    .line 430
    iget v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->identifyState:I
"""


def assert_registers_ok(method_src, n_params, name):
    """防「局部寄存器压到参数寄存器」：.registers = locals + params(+this)。
    2026-11-02 事故：AntiAddictionImpl.login 写成 .registers 7（=5 参数 + 2 局部），
    随后 const/4 v2,0x0 把 p0(this) 清空 ⇒ notifyAntiAddictionMessage 空引用 NPE 崩游戏。"""
    import re
    m = re.search(r"\.registers (\d+)", method_src)
    if not m:
        raise SystemExit("ERROR: %s 缺 .registers" % name)
    regs = int(m.group(1))
    locals_ = regs - (n_params + 1)
    if locals_ < 3:
        raise SystemExit("ERROR: %s .registers %d 只剩 %d 个局部，最少要 3（会压到 p0）"
                         % (name, regs, locals_))
    used = set(int(x) for x in re.findall(r"(?:^|[ {])v(\d+)", method_src, re.M))
    bad = [x for x in used if x >= locals_]
    if bad:
        raise SystemExit("ERROR: %s 引用了参数寄存器 v%s（局部只有 v0..v%d）"
                         % (name, ",".join(map(str, sorted(bad))), locals_ - 1))


def replace_method(text, header, new_body):
    i = text.index(header)
    j = text.index(".end method", i) + len(".end method")
    return text[:i] + new_body + text[j:]


def main():
    root = Path(sys.argv[1])
    p7 = root / "com/tapsdk/antiaddictionui/AntiAddictionUIImpl$7.smali"
    pimpl = root / "com/tapsdk/antiaddiction/AntiAddictionImpl.smali"
    for p in (p7, pimpl):
        if not p.is_file():
            raise SystemExit("ERROR: missing " + str(p))

    t7 = p7.read_text(encoding="utf-8")
    if MARK in t7:
        print("  [skip] $7 已改判（幂等）")
    else:
        if t7.count(ONSUCCESS_ANCHOR) != 1:
            raise SystemExit("ERROR: onSuccess 锚点命中数异常 %d" % t7.count(ONSUCCESS_ANCHOR))
        t7 = t7.replace(ONSUCCESS_ANCHOR, ONSUCCESS_FORCED, 1)
        t7 = replace_method(t7, ".method public onError(Ljava/lang/Throwable;)V", NEW_ONERROR)
        p7.write_text(t7, encoding="utf-8")
        print("  $7 patched: onSuccess 强制已实名 + onError 转成功")

    ti = pimpl.read_text(encoding="utf-8")
    assert_registers_ok(NEW_LOGIN, 4, "AntiAddictionImpl.login")

    if "短路：不做服务端实名核对" in ti:
        print("  [skip] AntiAddictionImpl.login 已短路（幂等）")
    else:
        ti = replace_method(
            ti,
            ".method public login(Ljava/lang/String;Ljava/lang/String;ILcom/tapsdk/antiaddiction/AntiAddictionLoginCallback;)V",
            NEW_LOGIN,
        )
        pimpl.write_text(ti, encoding="utf-8")
        print("  AntiAddictionImpl.login patched: 服务端核对短路 → 成年+已实名")

    print("  antiaddiction 改判完成")
    return 0


if __name__ == "__main__":
    sys.exit(main())
