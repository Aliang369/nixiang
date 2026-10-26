"""客户端授权短票（v1 = HMAC-SHA256 过渡 / **v2 = RSA-2048 非对称，P1 起主用**）。

v2 格式: 2|device|game|exp|feat_mask|kid.<512hex RSA 签名>
  - feat_mask: per-feature 位掩码（bit i = 功能 i），0 = 停用
  - 客户端 so 里只有公钥 ⇒ 剥出来也签不出票（这是「二次加卡密倒卖」的物理拦截）

v1 格式（保留一版过渡，旧包还能用）: 1|device|game|exp|menu|card|kid.<hex_hmac>

客户端 Java/Native 双端验签；无有效票则功能不生效。
"""
from __future__ import annotations

import hashlib
import hmac
import logging
import time

from ..config import get_settings
from . import sg_rsa

GRANT_VERSION = "1"
GRANT_VERSION_V2 = "2"

# feat_mask 上限：客户端只认前 32 位（SgAuth.h: feat_ok 的 idx 0..31）
FEAT_MASK_ALL = 0xFFFFFFFF

log = logging.getLogger(__name__)


def _rsa_key():
    """私钥不可用（未配置/读失败）时返回 None —— 调用方退化为只发 v1 票。"""
    path = (get_settings().grant_rsa_key_path or "").strip()
    if not path:
        return None
    try:
        return sg_rsa.load_key(path)
    except Exception as e:  # noqa: BLE001
        log.warning("grant_rsa_key 不可用（%s）：本次只发 v1 票", e)
        return None


def rsa_ready() -> bool:
    return _rsa_key() is not None


def rsa_fingerprint() -> str | None:
    key = _rsa_key()
    return sg_rsa.key_fingerprint(key) if key else None


def mint_grant_v2(
    *,
    device_id: str,
    game_id: str,
    offline_cache_seconds: int,
    feat_mask: int = FEAT_MASK_ALL,
    token: str | None = None,
) -> str | None:
    """签发 v2 非对称票。私钥不可用时返回 None（客户端继续用 v1）。"""
    key = _rsa_key()
    if key is None:
        return None
    ttl = max(int(offline_cache_seconds or 180), 60)
    exp = int(time.time()) + ttl
    mask = int(feat_mask) & FEAT_MASK_ALL
    body = "|".join(
        [
            GRANT_VERSION_V2,
            (device_id or "").strip(),
            (game_id or "").strip(),
            str(exp),
            str(mask),
            token_kid(token),
        ]
    )
    try:
        sig = sg_rsa.sign_hex(body.encode("utf-8"), key)
    except Exception as e:  # noqa: BLE001
        log.warning("v2 票签名失败（%s）", e)
        return None
    return f"{body}.{sig}"


def verify_grant_v2(
    grant: str,
    *,
    device_id: str,
    game_id: str,
    skew_seconds: int = 45,
) -> dict | None:
    """服务端自测/调试用（客户端以 native 验签为准）。"""
    key = _rsa_key()
    if key is None or not grant or "." not in grant:
        return None
    body, sig = grant.rsplit(".", 1)
    if not sg_rsa.verify_hex(body.encode("utf-8"), sig, key):
        return None
    parts = body.split("|")
    if len(parts) != 6 or parts[0] != GRANT_VERSION_V2:
        return None
    try:
        exp = int(parts[3])
        mask = int(parts[4])
    except ValueError:
        return None
    if int(time.time()) > exp + skew_seconds:
        return None
    if parts[1] != (device_id or "").strip() or parts[2] != (game_id or "").strip():
        return None
    return {"exp": exp, "feat_mask": mask, "kid": parts[5]}


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
