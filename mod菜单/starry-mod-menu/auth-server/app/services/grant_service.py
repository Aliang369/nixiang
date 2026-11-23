"""客户端授权短票（仅 v2 非对称；2026-11-21 起 v1/v3 退役）。

格式: 2|device_id|game_id|exp|feat_mask|kid.<hex_rsa_signature>
- exp: unix 秒，覆盖离线缓存窗口
- feat_mask: 64 位功能位掩码（bit i = 功能 i）
- kid: token 指纹前 8 位，或 open（关卡密联网模式）
- 签名: RSA-2048 PKCS#1 v1.5 + SHA-256，覆盖点号前整段 body

v1(HMAC) 不再签发、不再验证；服务端缺签名私钥或签名失败时**必须 fail closed**
（抛错、不下发票），不得退回对称票。客户端 Java/Native 双端验签；无有效票则功能不生效。
"""
from __future__ import annotations

import hashlib
import logging
import time

from ..config import get_settings
from . import sg_rsa

# v1(HMAC) 与 v3(载荷钥) 已于 2026-11-21 退役：只发 v2 非对称票。

# ---------------- v2：非对称票（P1，2026-10-27 加固） ----------------
# 私钥只在服务器 / 出包机；客户端 so 里只有公钥 ⇒ 破解者签不出票。
# 2026-11-10：客户端掩码由 32 位扩到 64 位（SgAuth.h feat_ok idx 0..63，与 g_features[64] 对齐）。
# 兼容性：全开 64 位在**老包**里经 (uint32_t)strtoul 截断后仍是 0xFFFFFFFF ⇒ 前 32 位照常全开，
# 不会让老包失效；只有"仅开高位"的票在老包里会被读成 0（老包本就没有那些功能，可接受）。
FEAT_MASK_ALL = 0xFFFFFFFFFFFFFFFF

def compute_feat_mask(db, game_id: str, *, menu_enabled: bool = True) -> int:
    """按后台 `feature_flags.enabled` 逐位算掩码（2026-11-11 起）。

    <h3>为什么改</h3>
    此前所有签发点都传 `FEAT_MASK_ALL if menu_enabled else 0` —— 管理端「功能开关」
    只影响客户端 UI 显隐（`NativeMenuController.enforceServerFeatureFlags` 把行置灰），
    <b>从不下发到掩码</b>。于是「后台关掉某功能」对已经拿到票的进程完全无效：
    `feat_ok()` 仍返回 true，玩家改回开关照样生效。这是把 UI 开关当安全手段的典型误用。

    <h3>口径（2026-11-23 改，lead 定）</h3>
    掩码**只由后台已登记且 enabled=true 的功能位构成**，其余位一律 0：
    - 后台没登记的功能位 → 0，`feat_ok(i)` 拒绝、`nativeSetFeature` 也拒绝
      （`MainModern.cpp` 的开关/滑块/按钮/文本四条入口都查 `feat_ok`）；
    - 后台登记了但停用某位 → 该位 0；
    - 后台一条功能记录都没有 → 整票掩码 0，客户端 `apply()` 拒绝（功能不可用）；
    - `menu_enabled=false` → 整票掩码 0（维护中，沿用原语义）。

    依据：用户 2026-11-23「功能开关必须由服务器下发……拿不到服务器授权就不放行」。
    旧口径「未登记位保持 1」= 后台从没配过也能全开，与上述要求冲突，已废止。
    影响：出包前必须先在后台建游戏并从模板同步功能，否则菜单点了不生效（这是预期行为）。
    """
    if not menu_enabled:
        return 0
    try:
        from ..models import FeatureFlag  # 延迟导入，避免 models ↔ services 循环
        rows = (
            db.query(FeatureFlag)
            .filter(FeatureFlag.game_id == game_id)
            .all()
        )
    except Exception:  # noqa: BLE001
        log.error("compute_feat_mask 查询失败（%s）：fail closed 返回 0", game_id, exc_info=True)
        return 0
    # 2026-11-23（lead 定）：掩码只由「后台已登记且 enabled」的功能位构成。
    # 未登记 = 后台没下发 = 不允许使用（旧口径「未登记位保持 1」等于自动全开，已废止）。
    mask = 0
    for r in rows:
        n = int(r.feat_num)
        if n < 0 or n > 63 or not bool(r.enabled):
            continue
        mask |= (1 << n)
    return mask & FEAT_MASK_ALL

log = logging.getLogger(__name__)

def _rsa_key():
    """私钥不可用（未配置/读失败）时返回 None —— 调用方必须 fail closed，不得退回对称票。"""
    path = (get_settings().grant_rsa_key_path or "").strip()
    if not path:
        return None
    try:
        return sg_rsa.load_key(path)
    except Exception as e:  # noqa: BLE001
        log.warning("grant_rsa_key 不可用（%s）：本次拒绝签发", e)
        return None

def rsa_ready() -> bool:
    return _rsa_key() is not None

def rsa_fingerprint():
    key = _rsa_key()
    return sg_rsa.key_fingerprint(key) if key else None

class GrantSigningError(RuntimeError):
    """签名私钥缺失或签名失败 —— 调用方必须转成明确的对外错误（fail closed）。"""

def ensure_grant_signing_ready() -> None:
    """签发前置检查：没有可用私钥就不往下走，避免先写库再失败。"""
    if _rsa_key() is None:
        raise GrantSigningError(
            "服务端未配置可用的签名私钥（grant_rsa_key_path），按 v2-only 策略拒绝签发授权票"
        )

def mint_grant_v2_or_raise(*, device_id, game_id, offline_cache_seconds,
                           feat_mask=FEAT_MASK_ALL, token=None) -> str:
    """mint_grant_v2 的 fail-closed 版本：签不出来就抛错，绝不返回 None。"""
    grant = mint_grant_v2(
        device_id=device_id,
        game_id=game_id,
        offline_cache_seconds=offline_cache_seconds,
        feat_mask=feat_mask,
        token=token,
    )
    if not grant:
        raise GrantSigningError("授权票签名失败（私钥不可用或签名异常）")
    return grant

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

def token_kid(token: str | None) -> str:
    if not token:
        return "open"
    return hashlib.sha256(token.encode("utf-8")).hexdigest()[:8]
