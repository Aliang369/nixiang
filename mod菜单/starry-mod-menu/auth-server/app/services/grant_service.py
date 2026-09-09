"""客户端授权短票（HMAC-SHA256）。

格式: 1|device|game|exp|menu|card|kid.<hex_hmac>
- exp: unix 秒，覆盖离线缓存窗口
- kid: token 指纹前 8 位，或 open（关卡密联网模式）
客户端 Java/Native 双端验签；无有效票则功能不生效。
"""
from __future__ import annotations

import hashlib
import hmac
import time

from ..config import get_settings

GRANT_VERSION = "1"


def _secret() -> bytes:
    settings = get_settings()
    raw = (settings.grant_hmac_secret or "").strip()
    if not raw:
        # 与客户端 native OBFUSCATE 默认一致；生产务必在 .env 设置并同步改 so
        raw = "sg-grant-v1-7f3a9c2e"
    return raw.encode("utf-8")


def token_kid(token: str | None) -> str:
    if not token:
        return "open"
    return hashlib.sha256(token.encode("utf-8")).hexdigest()[:8]


def mint_grant(
    *,
    device_id: str,
    game_id: str,
    offline_cache_seconds: int,
    menu_enabled: bool,
    card_auth_enabled: bool,
    token: str | None = None,
) -> str:
    """签发短票；TTL = offline_cache_seconds（至少 60s）。"""
    ttl = max(int(offline_cache_seconds or 180), 60)
    exp = int(time.time()) + ttl
    body = "|".join(
        [
            GRANT_VERSION,
            (device_id or "").strip(),
            (game_id or "").strip(),
            str(exp),
            "1" if menu_enabled else "0",
            "1" if card_auth_enabled else "0",
            token_kid(token),
        ]
    )
    sig = hmac.new(_secret(), body.encode("utf-8"), hashlib.sha256).hexdigest()
    return f"{body}.{sig}"


def verify_grant(
    grant: str,
    *,
    device_id: str,
    game_id: str,
    skew_seconds: int = 45,
) -> dict | None:
    """服务端自测/调试用；客户端以 native 验签为准。"""
    if not grant or "." not in grant:
        return None
    body, sig = grant.rsplit(".", 1)
    expect = hmac.new(_secret(), body.encode("utf-8"), hashlib.sha256).hexdigest()
    if not hmac.compare_digest(expect, sig.lower()):
        return None
    parts = body.split("|")
    if len(parts) != 7 or parts[0] != GRANT_VERSION:
        return None
    try:
        exp = int(parts[3])
    except ValueError:
        return None
    if int(time.time()) > exp + skew_seconds:
        return None
    if parts[1] != (device_id or "").strip() or parts[2] != (game_id or "").strip():
        return None
    return {
        "exp": exp,
        "menu_enabled": parts[4] == "1",
        "card_auth_enabled": parts[5] == "1",
        "kid": parts[6],
    }
