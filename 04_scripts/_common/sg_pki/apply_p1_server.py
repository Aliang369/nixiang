# -*- coding: utf-8 -*-
"""P1-2/P1-3 服务端：三处发票点补发 v2（RSA 非对称 + per-feature 掩码）票。

v1 票继续发（旧包还能用），v2 放 `grant2` 字段；客户端新版优先用 grant2。
每条替换都断言命中次数，不符就整体不动。
"""
import io
import os
import sys

ROOT = r"E:\nixiang"
P = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server", "app", "services", "auth_service.py")
CFG = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "auth-server", "app", "config.py")


def sub_once(s, old, new, label):
    n = s.count(old)
    if n != 1:
        raise SystemExit("[%s] 命中 %d 次（要求 1）" % (label, n))
    return s.replace(old, new, 1)


def grant2_block(token_expr):
    return (
        "    # v2 非对称票（P1）：so 里只有公钥 ⇒ 破解者剥出来也签不出票\n"
        "    grant2 = mint_grant_v2(\n"
        "        device_id=device_id,\n"
        "        game_id=game_id,\n"
        "        offline_cache_seconds=settings.offline_cache_seconds,\n"
        "        feat_mask=(FEAT_MASK_ALL if bool(boot.get(\"menu_enabled\", True)) else 0),\n"
        "        token=%s,\n"
        "    )\n" % token_expr
    )


def patch_auth_service():
    s = io.open(P, "r", encoding="utf-8").read()
    t = s

    t = sub_once(t, "from .grant_service import mint_grant\n",
                 "from .grant_service import mint_grant, mint_grant_v2, FEAT_MASK_ALL\n",
                 "import")

    # 站点 1：卡密登录
    t = sub_once(
        t,
        '        token=token,\n    )\n    return {\n        "token": token,',
        '        token=token,\n    )\n' + grant2_block("token") +
        '    return {\n        "token": token,',
        "login grant2 block")
    t = sub_once(
        t,
        '        "bootstrap": boot,\n        "grant": grant,\n    }',
        '        "bootstrap": boot,\n        "grant": grant,\n        "grant2": grant2,\n    }',
        "login return grant2")

    # 站点 2：心跳
    t = sub_once(
        t,
        '        token=session.token,\n    )\n    return {\n        "token": session.token,',
        '        token=session.token,\n    )\n' + grant2_block("session.token") +
        '    return {\n        "token": session.token,',
        "heartbeat grant2 block")
    t = sub_once(
        t,
        '        "grant": grant,\n        "authorized": True,',
        '        "grant": grant,\n        "grant2": grant2,\n        "authorized": True,',
        "heartbeat return grant2")

    # 站点 3：关卡密 bootstrap（open 票）
    t = sub_once(
        t,
        '''                card_auth_enabled=False,
                token=None,
            )
    return boot''',
        '''                card_auth_enabled=False,
                token=None,
            )
            # v2 对等：关卡密也不该是「无票自由发挥」，同样发掩码票
            _g2 = mint_grant_v2(
                device_id=did,
                game_id=game_id,
                offline_cache_seconds=int(boot.get("offline_cache_seconds") or 180),
                feat_mask=(FEAT_MASK_ALL if bool(boot.get("menu_enabled", True)) else 0),
                token=None,
            )
            if _g2:
                boot["grant2"] = _g2
    return boot''',
        "bootstrap open grant2")

    if t == s:
        raise SystemExit("auth_service.py 无改动")
    io.open(P, "w", encoding="utf-8", newline="\n").write(t)
    print("patched auth_service.py %d B -> %d B" % (len(s), len(t)))


def patch_config():
    s = io.open(CFG, "r", encoding="utf-8").read()
    old = '    grant_hmac_secret: str = "sg-grant-v1-7f3a9c2e"\n'
    new = (old +
           '    # 授权票 v2 非对称签名私钥路径（RSA-2048 PKCS#1 v1.5，文件为 n/e/d=hex）。\n'
           '    # 留空 = 只发 v1(HMAC) 票。生产必须配置，且**绝不入库**。\n'
           '    grant_rsa_key_path: str = ""\n')
    t = sub_once(s, old, new, "grant_rsa_key_path")
    io.open(CFG, "w", encoding="utf-8", newline="\n").write(t)
    print("patched config.py +grant_rsa_key_path")


if __name__ == "__main__":
    patch_config()
    patch_auth_service()
    print("done")
