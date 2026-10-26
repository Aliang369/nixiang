"""客户端授权短票（HMAC-SHA256）。

格式: 1|device|game|exp|menu|card|kid.<hex_hmac>
- exp: unix 秒，覆盖离线缓存窗口
- kid: token 指纹前 8 位，或 open（关卡密联网模式）
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

# ---------------- v2：非对称票（P1，2026-10-27 加固） ----------------
# 私钥只在服务器 / 出包机；客户端 so 里只有公钥 ⇒ 破解者签不出票。
FEAT_MASK_ALL = 0xFFFFFFFF          # 客户端只认前 32 位（SgAuth.h: feat_ok 的 idx 0..31）

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


def rsa_fingerprint():
    key = _rsa_key()
    return sg_rsa.key_fingerprint(key) if key else None


def mint_grant_v2(
    *,
    device_id: str,
    game_id: str,
    offline_cache_seconds: int,
    feat_mask: int = FEAT_MASK_ALL,
    token=None,
):
    """签发 v2 非对称票；私钥不可用时返回 None（客户端继续用 v1）。"""
    key = _rsa_key()
    if key is None:
        return None
    ttl = max(int(offline_cache_seconds or 180), 60)
    exp = int(time.time()) + ttl
    mask = int(feat_mask) & FEAT_MASK_ALL
    body = "|".join([
        "2",
        (device_id or "").strip(),
        (game_id or "").strip(),
        str(exp),
        str(mask),
        token_kid(token),
    ])
    try:
        sig = sg_rsa.sign_hex(body.encode("utf-8"), key)
    except Exception as e:  # noqa: BLE001
        log.warning("v2 票签名失败（%s）", e)
        return None
    return f"{body}.{sig}"

def payload_wrap_key(device_id, game_id, exp, mask, kid):
    """v3 票里 kx 的包裹密钥：只依赖**被签名覆盖的字段**，避免与签名的循环依赖。"""
    material = "sgpay|%s|%s|%s|%s|%s" % (
        (device_id or "").strip(), (game_id or "").strip(), exp, mask, kid)
    return hashlib.sha256(material.encode("utf-8")).digest()


def _payload_master():
    path = (get_settings().payload_master_path or "").strip()
    if not path:
        return None
    try:
        with open(path, "r", encoding="ascii") as f:
            return bytes.fromhex(f.read().strip()[:64])
    except Exception as e:  # noqa: BLE001
        log.warning("payload_master 不可用（%s）：v3 票将不签发", e)
        return None


_SHA_CACHE = {}          # game_id -> (sha, ts)
_SHA_TTL = 60.0


def _game_payload_sha(game_id):
    """按 game_id 从库里取载荷 sha（60s 缓存）；取不到回退全局配置（老部署兼容）。"""
    import time as _t
    gid = (game_id or "").strip()
    if gid:
        hit = _SHA_CACHE.get(gid)
        now = _t.time()
        if hit and (now - hit[1]) < _SHA_TTL:
            return hit[0]
        try:
            from ..database import SessionLocal
            from ..models import Game
            db = SessionLocal()
            try:
                row = db.query(Game).filter(Game.game_id == gid).first()
                sha = (row.payload_sha or "").strip().lower() if row else ""
            finally:
                db.close()
            _SHA_CACHE[gid] = (sha, now)
            return sha          # 库里没有就是没有：**不回退全局**，避免所有游戏共用一把 sha
        except Exception as e:  # noqa: BLE001
            log.warning("读 games.payload_sha 失败（%s）：本游戏不签 v3", e)
            return ""
    return (get_settings().payload_sha or "").strip().lower()   # 仅 game_id 为空时走 env（内部调用）


def payload_build_key(game_id=None):
    """K_build = HMAC(master, "sgpay-build|" + sha256(明文载荷))；没配好就返回 None。"""
    master = _payload_master()
    sha = _game_payload_sha(game_id)
    if not master or len(sha) != 64:
        return None
    try:
        digest = bytes.fromhex(sha)
    except ValueError:
        return None
    return hmac.new(master, b"sgpay-build|" + digest, hashlib.sha256).digest()


def payload_ready(game_id=None):
    return _rsa_key() is not None and payload_build_key(game_id) is not None


def mint_grant_v3(*, device_id, game_id, offline_cache_seconds,
                  feat_mask=FEAT_MASK_ALL, token=None):
    """签发 v3 票（带载荷密钥包裹）。私钥或载荷钥缺一不可，否则返回 None。"""
    key = _rsa_key()
    kb = payload_build_key(game_id)
    if key is None or kb is None:
        return None
    ttl = max(int(offline_cache_seconds or 180), 60)
    exp = int(time.time()) + ttl
    mask = int(feat_mask) & FEAT_MASK_ALL
    kid = token_kid(token)
    wrap = payload_wrap_key(device_id, game_id, exp, mask, kid)
    kx = bytes(a ^ b for a, b in zip(kb, wrap))
    body = "|".join([
        "3",
        (device_id or "").strip(),
        (game_id or "").strip(),
        str(exp),
        str(mask),
        kid,
        kx.hex(),
    ])
    try:
        sig = sg_rsa.sign_hex(body.encode("utf-8"), key)
    except Exception as e:  # noqa: BLE001
        log.warning("v3 票签名失败（%s）", e)
        return None
    return f"{body}.{sig}"


def verify_grant_v3(grant, *, device_id, game_id, skew_seconds=45):
    """服务端自测用：验签 + 还原 K_build。"""
    key = _rsa_key()
    if key is None or not grant or "." not in grant:
        return None
    body, sig = grant.rsplit(".", 1)
    if not sg_rsa.verify_hex(body.encode("utf-8"), sig, key):
        return None
    parts = body.split("|")
    if len(parts) != 7 or parts[0] != "3":
        return None
    try:
        exp = int(parts[3])
        mask = int(parts[4])
        kx = bytes.fromhex(parts[6])
    except ValueError:
        return None
    if int(time.time()) > exp + skew_seconds:
        return None
    if parts[1] != (device_id or "").strip() or parts[2] != (game_id or "").strip():
        return None
    wrap = payload_wrap_key(parts[1], parts[2], exp, mask, parts[5])
    return {"exp": exp, "feat_mask": mask, "kid": parts[5],
            "k_build": bytes(a ^ b for a, b in zip(kx, wrap))}



def verify_grant_v2(grant: str, *, device_id: str, game_id: str, skew_seconds: int = 45):
    """服务端自测用（客户端以 native 验签为准）。"""
    key = _rsa_key()
    if key is None or not grant or "." not in grant:
        return None
    body, sig = grant.rsplit(".", 1)
    if not sg_rsa.verify_hex(body.encode("utf-8"), sig, key):
        return None
    parts = body.split("|")
    if len(parts) != 6 or parts[0] != "2":
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
