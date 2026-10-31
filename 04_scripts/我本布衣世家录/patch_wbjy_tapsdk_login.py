#!/usr/bin/env python3
# 我本布衣：世家录 — TapSDK 验签/登录「改包法」smali 补丁（A 轨）
#
# 依据：10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md（2026-10-29 增补·双轨制）
#       A 轨 = 验签门闩/登录方式 ⇒ 静态改包；B 轨（运行时签名替换 killer）本包未接
#       （仓库无 bin.mt.signature.KillerApplication193 源，SignSpoof 系列已按用户指令删除、禁止复活）。
#
# 三处补丁（锚点按 1.1.66 现取；改法对照 1.1.66 实测日志）：
#   1) GatekeeperRepository$initialize$1.invokeSuspend：
#      服务端 INVALID_CLIENT（签名不匹配）分支 → 不 emit Failed，"降级成功"
#      （commitDegradedSuccess 会 emit TapInitState$Success）。
#      实测：官方包同机初始化成功；本包在 StartUp 全跑完后 Failed ⇒ C#「SDK初始化失败 1001」。
#   2) GatekeeperRepository.showErrorToast(...)V → return-void（掐验签失败弹窗，双保险）
#   3) Actions$CN.getAction() 常量 "com.taptap.sdk.action" → "com.taptap.sdk.act1on"（等长 21 字符）
#      ⇒ 唤起 TapTap 客户端的 intent 恒 resolve 不到 ⇒ 登录落内嵌网页
#
# ★不要动 checkInitialize / checkLocalInitialize（2026-10-31 实测：恒 true 会让 StartUp 提前收尾，
#   DBInitializeTask 的 ClassNotFoundException 逃出 try/catch ⇒ 初始化直接失败）
#
# 用法: python patch_wbjy_tapsdk_login.py <smali_root>
import re
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit("usage: patch_wbjy_tapsdk_login.py <smali_root>")
root = Path(sys.argv[1])

# ---- 1) INVALID_CLIENT 分支 → 降级成功 ----
p = root / "com/taptap/sdk/initializer/repository/GatekeeperRepository$initialize$1.smali"
if not p.is_file():
    raise SystemExit("ERROR: 缺 GatekeeperRepository$initialize$1.smali")
t = p.read_text(encoding="utf-8")
pat = re.compile(
    r"\n\s*invoke-static \{v0, v1\}, Lkotlin/jvm/internal/r;->a\(Ljava/lang/Object;Ljava/lang/Object;\)Z"
    r".*?if-eqz v0, :cond_8b", re.S)
n = len(pat.findall(t))
if n != 1:
    raise SystemExit("ERROR: INVALID_CLIENT 分支锚点命中 %d 次（期望 1，SDK 版本漂移）" % n)
t = pat.sub("\n\n    goto :cond_8b", t)
p.write_text(t, encoding="utf-8")
print("  [patch] GatekeeperRepository$initialize$1: INVALID_CLIENT → goto :cond_8b（降级成功）")

# ---- 2) showErrorToast → return-void ----
p = root / "com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
if not p.is_file():
    raise SystemExit("ERROR: 缺 GatekeeperRepository.smali")
t = p.read_text(encoding="utf-8")
HEAD = ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V"
i = t.find(HEAD)
if i < 0:
    raise SystemExit("ERROR: 找不到 showErrorToast 锚点")
j = t.find(".end method", i)
if j < 0:
    raise SystemExit("ERROR: showErrorToast 方法块没有 .end method")
t = t[:i] + HEAD + "\n    .registers 5\n\n    return-void\n.end method" + t[j + len(".end method"):]
p.write_text(t, encoding="utf-8")
print("  [patch] GatekeeperRepository.showErrorToast → return-void")

# ---- 3) client-SSO action 常量（等长替换） ----
acts = root / "com/taptap/sdk/login/impl/data/model/Actions$CN.smali"
if not acts.is_file():
    raise SystemExit("ERROR: 缺 Actions$CN.smali")
t = acts.read_text(encoding="utf-8")
OLD = "com.taptap.sdk.action"
NEW = "com.taptap.sdk.act1on"
if t.count(OLD) != 1:
    raise SystemExit("ERROR: Actions$CN 里 %r 命中 %d 次（锚点漂移，期望 1）" % (OLD, t.count(OLD)))
acts.write_text(t.replace(OLD, NEW), encoding="utf-8")
print("  [patch] Actions$CN.action: %s → %s（等长，客户端 SSO 恒不匹配）" % (OLD, NEW))

print("OK: 2 方法 + 1 分支 + 1 常量已改")
