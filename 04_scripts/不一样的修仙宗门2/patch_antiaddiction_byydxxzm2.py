#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""不一样的修仙宗门2 — TapTap 防沉迷「快速认证/实名」门禁客户端改判。

链路与改判点同 04_scripts/我的技能无限触发/patch_antiaddiction_wdjnwcf.py（同一 TapSDK 3.18.x 版本，
类名与方法签名已逐项核对一致）。本文件按本包 smali 实况调整了 onSuccess 锚点：
本包是 .locals 9、无 .line 430，先例是 .registers 11 + .line 430，锚点不可跨包照抄。

三处：
  A) AntiAddictionUIImpl$7.onSuccess 入口强制 identifyState=0 / ageLimit=18 /
     antiAddictionToken / hasAuthRecord=true  ⇒ 永远走「已实名」分支，不弹快速认证 UI
  B) AntiAddictionUIImpl$7.onError 整方法 ⇒ 构造空 IdentifyState 转 onSuccess
  C) AntiAddictionImpl.login 整方法短路 ⇒ 不请求服务端，直接 UserInfo(成年/无限时长) + notify(500) + onSuccess

用法：python patch_antiaddiction_byydxxzm2.py <smali 根目录>
"""
import re
import sys
from pathlib import Path

MARK = "[starry]"

IDENT = "    iget v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->identifyState:I\n"

FORCED = (
    "    # [starry] 强制「已实名 + 成年」\n"
    "    const/4 v0, 0x0\n"
    "    iput v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->identifyState:I\n"
    "    const/16 v0, 0x12\n"
    "    iput v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->ageLimit:I\n"
    "    const-string v0, \"standalone_anti_addiction_token\"\n"
    "    iput-object v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->antiAddictionToken:Ljava/lang/String;\n"
    "    const/4 v0, 0x1\n"
    "    iput-boolean v0, p1, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;->hasAuthRecord:Z\n"
    + IDENT
)

NEW_ONERROR = """.method public onError(Ljava/lang/Throwable;)V
    .registers 4

    # [starry] 实名状态查询失败 ⇒ 直接转 onSuccess（由 onSuccess 强制成已实名）
    new-instance v0, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;

    invoke-direct {v0}, Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;-><init>()V

    invoke-virtual {p0, v0}, Lcom/tapsdk/antiaddictionui/AntiAddictionUIImpl$7;->onSuccess(Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;)V

    return-void
.end method"""

NEW_LOGIN = """.method public login(Ljava/lang/String;Ljava/lang/String;ILcom/tapsdk/antiaddiction/AntiAddictionLoginCallback;)V
    # .registers 8 = 3 局部 + 5 参数（p0=v3…p4=v7），不可写成 7（先例事故：p0 被清零 → NPE）
    .registers 8

    # [starry] 短路：不做服务端实名核对，直接按成年人 + 已实名回调
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


def assert_registers_ok(method_src, n_params, name):
    m = re.search(r"\.registers (\d+)", method_src)
    if not m:
        raise SystemExit("ERROR: %s 缺 .registers" % name)
    regs = int(m.group(1))
    locals_ = regs - (n_params + 1)
    if locals_ < 3:
        raise SystemExit("ERROR: %s .registers %d 只剩 %d 局部，最少 3" % (name, regs, locals_))
    used = set(int(x) for x in re.findall(r"(?:^|[ {])v(\d+)", method_src, re.M))
    bad = [x for x in used if x >= locals_]
    if bad:
        raise SystemExit("ERROR: %s 引用参数寄存器 v%s" % (name, ",".join(map(str, sorted(bad)))))


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
        _hdr = ".method public onSuccess(Lcom/tapsdk/antiaddiction/entities/response/IdentifyState;)V"
        _i = t7.index(_hdr)
        _j = t7.index(".end method", _i)
        _body = t7[_i:_j]
        if IDENT not in _body:
            raise SystemExit("ERROR: onSuccess 内找不到 identifyState 锚点")
        _body2 = _body.replace(IDENT, FORCED, 1)
        t7 = t7[:_i] + _body2 + t7[_j:]
        t7 = replace_method(t7, ".method public onError(Ljava/lang/Throwable;)V", NEW_ONERROR)
        p7.write_text(t7, encoding="utf-8")
        print("  $7 patched: onSuccess 强制已实名 + onError 转成功")

    ti = pimpl.read_text(encoding="utf-8")
    assert_registers_ok(NEW_LOGIN, 4, "AntiAddictionImpl.login")
    if "短路：不做服务端实名核对" in ti:
        print("  [skip] login 已短路（幂等）")
    else:
        ti = replace_method(
            ti,
            ".method public login(Ljava/lang/String;Ljava/lang/String;ILcom/tapsdk/antiaddiction/AntiAddictionLoginCallback;)V",
            NEW_LOGIN,
        )
        pimpl.write_text(ti, encoding="utf-8")
        print("  AntiAddictionImpl.login patched: 服务端核对短路")
    print("  antiaddiction 改判完成")
    return 0


if __name__ == "__main__":
    sys.exit(main())
