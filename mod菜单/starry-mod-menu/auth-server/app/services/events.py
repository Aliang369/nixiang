"""事件与审计写入（2026-11-15 扩展：可信化 + 可追溯 + 不落凭证）。"""

from __future__ import annotations

import time
import uuid
from collections import defaultdict
from threading import Lock
from typing import Optional

from fastapi import Request
from sqlalchemy.orm import Session

from ..config import get_settings
from ..deviceid import device_fingerprint, is_valid_device_id, mask_card_key
from ..models import AdminAuditLog, AuthEvent
from ..security_guard import resolve_client_ip

_lock = Lock()
# ip -> {失败 key: 首条时间}；只用于限制「不同失败 key」的写放大，不丢首条
_fail_keys: dict[str, dict[str, float]] = defaultdict(dict)
# 触发 key 上限而未落库的累计次数（不静默丢弃：后台看板可见）
_overflow: dict[str, int] = defaultdict(int)


def failure_overflow_since_start() -> int:
    """本进程启动以来因「每 IP 不同失败 key 超上限」而未落库的次数。"""
    with _lock:
        return sum(_overflow.values())


def _clip(value: str | None, limit: int) -> str | None:
    if not value:
        return None
    # 去控制字符，避免把二进制/换行塞进日志列
    clean = "".join(ch for ch in value if ch >= " " and ch != "\x7f")
    clean = mask_card_key(clean).strip()
    return clean[:limit] or None


def request_meta(request: Request | None) -> dict:
    """解析请求元数据；IP 只认受信反代，其余一律 socket peer。"""
    if request is None:
        return {"ip": None, "ip_forwarded": None, "ua": None, "request_id": None}
    ip, _source = resolve_client_ip(request)
    forwarded = request.headers.get("x-forwarded-for") or request.headers.get("x-real-ip")
    # request_id 由中间件统一生成（不接受客户端自传任意串），这里只读 state
    rid = getattr(getattr(request, "state", None), "request_id", None) or uuid.uuid4().hex[:32]
    return {
        "ip": _clip(ip, 45),
        "ip_forwarded": _clip(forwarded, 45),
        "ua": _clip(request.headers.get("user-agent"), 120),
        "request_id": _clip(rid, 36),
    }


def record_auth_event(
    db: Session,
    event_type: str,
    *,
    game_id: str | None = None,
    device_id: str | None = None,
    card_id: int | None = None,
    detail: str | None = None,
    result: str | None = None,
    reason: str | None = None,
    request: Request | None = None,
    ip: str | None = None,
    ip_forwarded: str | None = None,
    ua: str | None = None,
    request_id: str | None = None,
    commit: bool = True,
) -> None:
    """写一条客户端事件。

    约定：detail 只放内部标识/脱敏文案（如 `card#12`），禁止写入卡密、密码、token。
    """
    meta = request_meta(request) if request is not None else {}
    db.add(
        AuthEvent(
            event_type=event_type[:32],
            game_id=(game_id or None) and game_id[:64],
            device_id=_clip(device_id, 128),
            card_id=card_id,
            detail=_clip(detail, 255),
            result=_clip(result, 8),
            reason=_clip(reason, 32),
            ip=_clip(ip or meta.get("ip"), 45),
            ip_forwarded=_clip(ip_forwarded or meta.get("ip_forwarded"), 45),
            ua=_clip(ua or meta.get("ua"), 120),
            request_id=_clip(request_id or meta.get("request_id"), 36),
        )
    )
    if commit:
        db.commit()


def record_failure_event(
    db: Session,
    event_type: str,
    *,
    device_id: str | None,
    reason: str,
    game_id: str | None = None,
    card_id: int | None = None,
    detail: str | None = None,
    request: Request | None = None,
    commit: bool = True,
) -> str:
    """登录/心跳失败等高频事件：同 (event_type, device, reason, ip) 在窗口内只留首条。

    返回 `inserted` | `suppressed` | `capped`：
    - inserted：新 key，落一条完整记录；
    - suppressed：重复 key，累加 suppressed 计数（后台可见「抑制 N 次」）；
    - capped：该 IP 窗口内不同 key 数已达上限，本次不落库（防灌水），调用方仍返回正常错误。
    成功事件不走本函数，一律完整记录。
    """
    settings = get_settings()
    window = max(int(settings.login_fail_dedupe_seconds), 30)
    max_keys = max(int(settings.login_fail_max_keys_per_ip), 1)
    meta = request_meta(request)
    ip = meta.get("ip") or "unknown"
    now = time.time()
    # 非法设备号不落原文，只留不可逆短指纹（可据此聚合同一来源，不能还原 payload）
    if device_id and not is_valid_device_id(device_id):
        device_id = device_fingerprint(device_id)

    with _lock:
        bucket = _fail_keys[ip]
        for k, ts in list(bucket.items()):
            if now - ts > window:
                bucket.pop(k, None)
        key = "|".join([event_type, game_id or "", device_id or "", reason, ip])
        if key not in bucket:
            if len(bucket) >= max_keys:
                _overflow[ip] += 1
                return "capped"
            bucket[key] = now

    from datetime import datetime, timedelta, timezone

    since = datetime.now(timezone.utc) - timedelta(seconds=window)
    query = db.query(AuthEvent).filter(
        AuthEvent.event_type == event_type,
        AuthEvent.reason == reason,
        AuthEvent.ip == meta.get("ip"),
        AuthEvent.created_at >= since,
    )
    query = (
        query.filter(AuthEvent.device_id == device_id)
        if device_id
        else query.filter(AuthEvent.device_id.is_(None))
    )
    query = (
        query.filter(AuthEvent.game_id == game_id)
        if game_id
        else query.filter(AuthEvent.game_id.is_(None))
    )
    existing = query.order_by(AuthEvent.id.desc()).first()
    if existing is not None:
        existing.suppressed = int(getattr(existing, "suppressed", 0) or 0) + 1
        existing.last_occurred_at = datetime.now(timezone.utc)
        if commit:
            db.commit()
        return "suppressed"

    record_auth_event(
        db,
        event_type,
        game_id=game_id,
        device_id=device_id,
        card_id=card_id,
        detail=detail,
        result="deny",
        reason=reason,
        request=request,
        commit=commit,
    )
    return "inserted"


def record_admin_audit(
    db: Session,
    username: str,
    action: str,
    *,
    detail: str | None = None,
    ip: str | None = None,
    commit: bool = True,
) -> None:
    db.add(
        AdminAuditLog(
            admin_username=username or "unknown",
            action=action,
            detail=(detail or "")[:512] or None,
            ip=ip,
        )
    )
    if commit:
        db.commit()
