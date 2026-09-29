from __future__ import annotations

import json
import re
import secrets
import string
from datetime import timedelta, timezone
from typing import Any, Optional

from fastapi import HTTPException
from sqlalchemy import and_, or_
from sqlalchemy.orm import Session

from ..config import get_settings
from .events import record_auth_event
from ..models import (
    Announcement,
    CardKey,
    FeatureFlag,
    Game,
    OnlinePresence,
    SessionToken,
    ToastMessage,
)
from ..security import add_duration, utcnow
from .grant_service import mint_grant


def _gen_key(prefix: str = "SG", length: int = 16) -> str:
    alphabet = string.ascii_uppercase + string.digits
    body = "".join(secrets.choice(alphabet) for _ in range(length))
    return f"{prefix}-{body[:4]}-{body[4:8]}-{body[8:12]}-{body[12:16]}"


def generate_card_keys(db: Session, count: int, prefix: str, **fields) -> list[CardKey]:
    created: list[CardKey] = []
    for _ in range(count):
        for _attempt in range(8):
            code = _gen_key(prefix=prefix)
            if not db.query(CardKey).filter(CardKey.key_code == code).first():
                break
        else:
            raise HTTPException(500, "卡密生成冲突，请重试")
        card = CardKey(key_code=code, **fields)
        db.add(card)
        created.append(card)
    db.commit()
    for c in created:
        db.refresh(c)
    return created


CUSTOM_KEY_RE = re.compile(r"^[A-Za-z0-9_-]{4,64}$")
MAX_LEGACY_DEVICE_IDS = 16


def _normalize_legacy_device_ids(raw: Optional[list[str]]) -> list[str]:
    """清洗客户端迁移候选：去重、截断、忽略非法项，不把输入异常升级成登录失败。"""
    out: list[str] = []
    seen: set[str] = set()
    for item in raw or []:
        if not isinstance(item, str):
            continue
        value = item.strip()
        if not value or len(value) > 128 or value in seen:
            continue
        seen.add(value)
        out.append(value)
        if len(out) >= MAX_LEGACY_DEVICE_IDS:
            break
    return out


def _load_legacy_device_ids(card: CardKey) -> list[str]:
    raw = card.legacy_device_ids
    if not raw:
        return []
    try:
        data = json.loads(raw)
    except (TypeError, ValueError):
        return []
    if not isinstance(data, list):
        return []
    return _normalize_legacy_device_ids(data)


def _store_legacy_device_ids(card: CardKey, ids: list[str]) -> None:
    clean = _normalize_legacy_device_ids(ids)
    card.legacy_device_ids = (
        json.dumps(clean, ensure_ascii=True, separators=(",", ":")) if clean else None
    )


def _authorize_device_binding(
    card: CardKey,
    device_id: str,
    legacy_device_ids: Optional[list[str]],
) -> Optional[str]:
    """返回需要迁移的旧主绑定；None=无需迁移，不匹配则保持原 403 语义。"""
    if not card.device_id or card.device_id == device_id:
        return None
    requested = _normalize_legacy_device_ids(legacy_device_ids)
    known = _load_legacy_device_ids(card)
    if card.device_id in requested or device_id in known:
        return card.device_id
    raise HTTPException(403, "卡密已绑定其他设备，请先解绑")


def normalize_custom_keys(raw_keys: list[str]) -> list[str]:
    """自定义卡密归一化：容忍行内空格/逗号/中文分号分隔，清洗空白。
    不去重、不查库——批内重复与库内重复都交给 create_custom_card_keys 整批拒绝。"""
    cleaned: list[str] = []
    for raw in raw_keys:
        for part in re.split(r"[\s,，;；]+", str(raw).strip()):
            part = part.strip()
            if part:
                cleaned.append(part)
    return cleaned


def create_custom_card_keys(db: Session, codes: list[str], **fields) -> list[CardKey]:
    """按输入原样创建卡密。整批拒绝语义：格式非法 / 批内重复 / 与库中重复都直接 400。"""
    if not codes:
        raise HTTPException(400, "请输入卡密")
    if len(codes) > 500:
        raise HTTPException(400, "单次最多 500 条")
    bad = [k for k in codes if not CUSTOM_KEY_RE.fullmatch(k)]
    if bad:
        raise HTTPException(400, f"卡密格式不正确（限字母/数字/-/_，4~64位）：{'、'.join(bad[:10])}")
    in_batch_dup = sorted({k for k in codes if codes.count(k) > 1})
    if in_batch_dup:
        raise HTTPException(400, f"以下卡密在输入中重复：{'、'.join(in_batch_dup[:10])}")
    existing = {k.key_code for k in db.query(CardKey).filter(CardKey.key_code.in_(codes)).all()}
    dup = [k for k in codes if k in existing]
    if dup:
        raise HTTPException(400, f"以下卡密已存在：{'、'.join(dup[:10])}")
    created: list[CardKey] = []
    for code in codes:
        card = CardKey(key_code=code, **fields)
        db.add(card)
        created.append(card)
    db.commit()
    for c in created:
        db.refresh(c)
    return created


def card_allows_game(card: CardKey, game_id: str) -> bool:
    if card.scope == "global":
        return True
    return card.game_id == game_id


def refresh_card_status(card: CardKey) -> None:
    """状态派生（2026-09-06）：banned=人工钉死、unused=未激活，两者不由时间/次数改写；
    active/expired/exhausted 由 到期时间+次数 实时推导，条件解除自动恢复正常。"""
    now = utcnow()
    if card.status in ("banned", "unused"):
        return
    if card.max_uses > 0 and card.used_count >= card.max_uses:
        card.status = "exhausted"
        return
    if card.expire_at is not None:
        exp = card.expire_at
        if exp.tzinfo is None:
            from datetime import timezone

            exp = exp.replace(tzinfo=timezone.utc)
        if exp <= now:
            card.status = "expired"
            return
    if card.status in ("expired", "exhausted"):
        # 过期被延期 / 用尽被减次数（或加次数上限）→ 条件解除，自动恢复正常
        card.status = "active"


def ensure_card_usable(card: CardKey, game_id: str) -> None:
    refresh_card_status(card)
    if not card_allows_game(card, game_id):
        raise HTTPException(403, "此卡密不适用于当前游戏")
    if card.status == "banned":
        raise HTTPException(403, "卡密已被封禁")
    if card.status == "expired":
        raise HTTPException(403, "卡密已过期")
    if card.status == "exhausted":
        raise HTTPException(403, "卡密次数已用尽")


def revoke_card_sessions(db: Session, card_id: int) -> int:
    """吊销该卡密下全部有效会话（封禁/到期/解绑/删除时调用）。"""
    return (
        db.query(SessionToken)
        .filter(SessionToken.card_id == card_id, SessionToken.revoked.is_(False))
        .update({"revoked": True})
    )


def activate_if_needed(card: CardKey) -> bool:
    """返回是否本次新激活。"""
    if card.status == "unused" or card.activated_at is None:
        now = utcnow()
        card.activated_at = now
        if card.duration_type != "count":
            card.expire_at = add_duration(now, card.duration_type)
        card.status = "active"
        return True
    return False


def login_card(
    db: Session,
    card_key: str,
    game_id: str,
    device_id: str,
    legacy_device_ids: Optional[list[str]] = None,
) -> dict[str, Any]:
    settings = get_settings()
    card = (
        db.query(CardKey)
        .filter(CardKey.key_code == card_key.strip())
        .with_for_update()
        .first()
    )
    if not card:
        raise HTTPException(404, "卡密不存在")

    ensure_card_usable(card, game_id)

    # 先验证迁移资格但不写库；后面次数检查失败时不能留下半迁移状态。
    migration_from = _authorize_device_binding(card, device_id, legacy_device_ids)

    freshly_activated = activate_if_needed(card)

    # 次数：每次登录扣 1（含纯 count 卡，以及 max_uses>0 的时长卡）
    if card.max_uses > 0 or card.duration_type == "count":
        # 纯 count 若 max_uses=0 不合理，默认按 1 次处理不合适；要求 max_uses>0
        if card.duration_type == "count" and card.max_uses <= 0:
            raise HTTPException(400, "次数卡未配置可用次数")
        if card.max_uses > 0:
            if card.used_count >= card.max_uses:
                card.status = "exhausted"
                db.commit()
                raise HTTPException(403, "卡密次数已用尽")
            card.used_count += 1
            if card.used_count >= card.max_uses:
                card.status = "exhausted"

    if not card.device_id:
        card.device_id = device_id
    elif migration_from:
        known = _load_legacy_device_ids(card)
        merged = [migration_from] + [x for x in known if x != device_id and x != migration_from]
        _store_legacy_device_ids(card, merged)
        card.device_id = device_id

    if migration_from:
        # 主绑定迁移后旧设备会话全部失效，旧在线行不再挂这张卡。
        revoke_card_sessions(db, card.id)
        db.query(OnlinePresence).filter(OnlinePresence.card_id == card.id).update(
            {"card_id": None}, synchronize_session=False
        )
    else:
        # 撤销同设备旧会话
        db.query(SessionToken).filter(
            SessionToken.device_id == device_id,
            SessionToken.game_id == game_id,
            SessionToken.revoked.is_(False),
        ).update({"revoked": True})

    token = secrets.token_urlsafe(32)
    expires_at = utcnow() + timedelta(minutes=settings.access_token_expire_minutes)
    session = SessionToken(
        token=token,
        card_id=card.id,
        game_id=game_id,
        device_id=device_id,
        expires_at=expires_at,
        last_seen_at=utcnow(),
    )
    db.add(session)
    _upsert_presence(db, game_id, device_id, card.id)
    db.commit()
    db.refresh(card)

    from .events import record_auth_event

    if migration_from:
        record_auth_event(
            db,
            "device_migrate",
            game_id=game_id,
            device_id=device_id,
            card_id=card.id,
            detail=f"{migration_from} -> {device_id}",
        )
    if freshly_activated:
        record_auth_event(
            db, "activate", game_id=game_id, device_id=device_id, card_id=card.id, detail=card.key_code
        )
    record_auth_event(
        db, "login_ok", game_id=game_id, device_id=device_id, card_id=card.id, detail=card.key_code
    )

    boot = build_bootstrap(db, game_id, logged_in=True)
    grant = mint_grant(
        device_id=device_id,
        game_id=game_id,
        offline_cache_seconds=settings.offline_cache_seconds,
        menu_enabled=bool(boot.get("menu_enabled", True)),
        card_auth_enabled=True,
        token=token,
    )
    return {
        "token": token,
        "expires_at": expires_at.isoformat(),
        "offline_cache_seconds": settings.offline_cache_seconds,
        "card": serialize_card_public(card),
        "bootstrap": boot,
        "grant": grant,
    }


def unbind_card(db: Session, card_key: str, device_id: str) -> dict[str, Any]:
    """自助解绑（2026-09-06 改版）：不限次数；首次免费，之后每次扣 3 天有效期。
    无到期时间的卡（永久/次数卡）不扣。管理员强制解绑不走这里、不扣时间。"""
    card = db.query(CardKey).filter(CardKey.key_code == card_key.strip()).first()
    if not card:
        raise HTTPException(404, "卡密不存在")
    if not card.device_id:
        raise HTTPException(400, "卡密尚未绑定设备")
    if card.device_id != device_id:
        raise HTTPException(403, "只能由当前绑定设备解绑")

    if card.unbind_used:
        # 非首次：扣 3 天。剩余不足 3 天则直接扣完（状态派生为已过期）
        deduct_msg = "，已扣除 3 天有效期"
        if card.expire_at is not None:
            exp = card.expire_at
            if exp.tzinfo is None:
                exp = exp.replace(tzinfo=timezone.utc)
            card.expire_at = exp - timedelta(days=3)
        else:
            deduct_msg = "（该卡无到期时间，不扣时间）"
    else:
        card.unbind_used = True
        deduct_msg = "（首次解绑不扣时间）"

    card.device_id = None
    card.legacy_device_ids = None
    db.query(SessionToken).filter(SessionToken.card_id == card.id, SessionToken.revoked.is_(False)).update(
        {"revoked": True}
    )
    # 解绑后该设备立刻算游客
    db.query(OnlinePresence).filter(OnlinePresence.device_id == device_id).update(
        {"card_id": None}
    )
    record_auth_event(
        db, "unbind", card_id=card.id,
        detail=f"device={device_id} {deduct_msg.lstrip('，')}",
    )
    db.commit()
    return {"message": "解绑成功" + deduct_msg}


def heartbeat(
    db: Session,
    token: str,
    game_id: str,
    device_id: str,
) -> dict[str, Any]:
    settings = get_settings()
    session = db.query(SessionToken).filter(SessionToken.token == token).first()
    if not session or session.revoked:
        raise HTTPException(401, "会话无效，请重新登录")
    if session.device_id != device_id or session.game_id != game_id:
        raise HTTPException(401, "会话与设备/游戏不匹配")

    now = utcnow()
    exp = session.expires_at
    if exp.tzinfo is None:
        from datetime import timezone

        exp = exp.replace(tzinfo=timezone.utc)
    if exp <= now:
        session.revoked = True
        db.commit()
        raise HTTPException(401, "会话已过期，请重新登录")

    card = db.query(CardKey).filter(CardKey.id == session.card_id).first()
    if not card:
        session.revoked = True
        db.commit()
        raise HTTPException(401, "卡密不存在")
    try:
        ensure_card_usable(card, game_id)
    except HTTPException:
        # 封禁/过期/用尽：立刻吊销会话，避免客户端离线缓存继续放行
        session.revoked = True
        db.commit()
        raise
    if card.device_id and card.device_id != device_id:
        session.revoked = True
        db.commit()
        raise HTTPException(403, "设备绑定已变更，请重新登录")

    session.last_seen_at = now
    session.expires_at = now + timedelta(minutes=settings.access_token_expire_minutes)
    _upsert_presence(db, game_id, device_id, card.id)
    db.commit()
    db.refresh(card)

    boot = build_bootstrap(db, game_id, logged_in=True)
    grant = mint_grant(
        device_id=device_id,
        game_id=game_id,
        offline_cache_seconds=settings.offline_cache_seconds,
        menu_enabled=bool(boot.get("menu_enabled", True)),
        card_auth_enabled=True,
        token=session.token,
    )
    return {
        "token": session.token,
        "expires_at": session.expires_at.isoformat(),
        "offline_cache_seconds": settings.offline_cache_seconds,
        "card": serialize_card_public(card),
        "bootstrap": boot,
        "grant": grant,
        "authorized": True,
    }


def _upsert_presence(
    db: Session,
    game_id: str,
    device_id: str,
    card_id: Optional[int] = None,
    *,
    update_card: bool = True,
) -> None:
    """刷新在线心跳。update_card=False 时只续期 last_seen，保留原有 card_id（供 bootstrap 轮询）。"""
    row = (
        db.query(OnlinePresence)
        .filter(OnlinePresence.game_id == game_id, OnlinePresence.device_id == device_id)
        .first()
    )
    now = utcnow()
    if row:
        row.last_seen_at = now
        if update_card:
            row.card_id = card_id
    else:
        db.add(
            OnlinePresence(
                game_id=game_id,
                device_id=device_id,
                card_id=card_id if update_card else None,
                last_seen_at=now,
            )
        )


def serialize_card_public(card: CardKey) -> dict[str, Any]:
    refresh_card_status(card)
    remaining_uses = None
    if card.max_uses > 0:
        remaining_uses = max(card.max_uses - card.used_count, 0)
    return {
        "key_code": card.key_code,
        "scope": card.scope,
        "game_id": card.game_id,
        "duration_type": card.duration_type,
        "status": card.status,
        "expire_at": card.expire_at.isoformat() if card.expire_at else None,
        "activated_at": card.activated_at.isoformat() if card.activated_at else None,
        "max_uses": card.max_uses,
        "used_count": card.used_count,
        "remaining_uses": remaining_uses,
        "device_bound": bool(card.device_id),
        "unbind_used": card.unbind_used,
        "can_unbind": bool(card.device_id),  # 2026-09-06 解绑不限次数后：绑定的卡随时可自助解绑
    }


def build_bootstrap(db: Session, game_id: str, logged_in: bool = False) -> dict[str, Any]:
    now = utcnow()
    anns = (
        db.query(Announcement)
        .filter(
            Announcement.enabled.is_(True),
            or_(
                and_(Announcement.scope == "global"),
                and_(Announcement.scope == "game", Announcement.game_id == game_id),
            ),
        )
        .order_by(Announcement.sort_order.asc(), Announcement.id.desc())
        .all()
    )
    filtered = []
    for a in anns:
        vis = getattr(a, "visibility", None) or "always"
        if vis == "logged_in" and not logged_in:
            continue
        if vis == "logged_out" and logged_in:
            continue
        start = getattr(a, "start_at", None)
        end = getattr(a, "end_at", None)
        if start is not None:
            s = start if start.tzinfo else start.replace(tzinfo=__import__("datetime").timezone.utc)
            if now < s:
                continue
        if end is not None:
            e = end if end.tzinfo else end.replace(tzinfo=__import__("datetime").timezone.utc)
            if now > e:
                continue
        filtered.append(a)

    features = (
        db.query(FeatureFlag)
        .filter(FeatureFlag.game_id == game_id)
        .order_by(FeatureFlag.feat_num.asc())
        .all()
    )
    toasts = (
        db.query(ToastMessage)
        .filter(
            ToastMessage.enabled.is_(True),
            or_(
                and_(ToastMessage.scope == "global"),
                and_(ToastMessage.scope == "game", ToastMessage.game_id == game_id),
            ),
        )
        .order_by(
            ToastMessage.sort_order.asc(),
            ToastMessage.id.asc(),
        )
        .all()
    )
    # 仅 startup_* 进入开屏队列（忽略历史 login_success 等事件 Key）
    toasts = [t for t in toasts if (t.toast_key or "").startswith("startup")]
    startup_toasts = [
        {
            "id": t.id,
            "content": t.content,
            "type": t.toast_type or "info",
        }
        for t in toasts
    ]

    game = db.query(Game).filter(Game.game_id == game_id).first()
    menu_enabled = True if game is None else bool(game.enabled)
    card_auth_enabled = True if game is None else bool(getattr(game, "card_auth_enabled", True))

    return {
        "game_id": game_id,
        "announcements": [
            {
                "id": a.id,
                "scope": a.scope,
                "title": a.title,
                "content": a.content,
                "visibility": getattr(a, "visibility", None) or "always",
                "popup_mode": getattr(a, "popup_mode", None) or "every_open",
                "action_url": getattr(a, "action_url", None) or None,
                "action_label": getattr(a, "action_label", None) or None,
            }
            for a in filtered
        ],
        "features": [
            {
                "feat_num": f.feat_num,
                "name": f.name,
                "enabled": f.enabled,
            }
            for f in features
        ],
        "startup_toasts": startup_toasts,
        "toasts": {},  # 兼容旧字段；业务提示改由客户端固定文案
        "menu_enabled": menu_enabled,
        "card_auth_enabled": card_auth_enabled,
        "offline_cache_seconds": get_settings().offline_cache_seconds,
    }


def bootstrap_with_grant(
    db: Session,
    game_id: str,
    device_id: str | None = None,
    logged_in: bool = False,
) -> dict[str, Any]:
    """公开 bootstrap；带 device_id 时刷新在线（打开菜单即计总在线）。

    卡密在线由登录/心跳写入 card_id；退出登录或关卡密时清掉 card_id（游客）。
    关卡密时额外下发 open 模式短票。
    """
    boot = build_bootstrap(db, game_id, logged_in=logged_in)
    did = (device_id or "").strip()
    if did:
        open_mode = not boot.get("card_auth_enabled", True)
        if open_mode or not logged_in:
            # 关卡密 / 已退出登录：记为游客，必须清掉残留 card_id
            _upsert_presence(db, game_id, did, card_id=None, update_card=True)
        else:
            # 仍卡密登录：只续期 last_seen，保留心跳写入的 card_id
            _upsert_presence(db, game_id, did, update_card=False)
        db.commit()
        if open_mode:
            boot["grant"] = mint_grant(
                device_id=did,
                game_id=game_id,
                offline_cache_seconds=int(boot.get("offline_cache_seconds") or 180),
                menu_enabled=bool(boot.get("menu_enabled", True)),
                card_auth_enabled=False,
                token=None,
            )
    return boot
