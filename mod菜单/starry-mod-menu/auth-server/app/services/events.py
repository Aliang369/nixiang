"""事件与审计写入。"""

from __future__ import annotations

from typing import Optional

from sqlalchemy.orm import Session

from ..models import AdminAuditLog, AuthEvent


def record_auth_event(
    db: Session,
    event_type: str,
    *,
    game_id: str | None = None,
    device_id: str | None = None,
    card_id: int | None = None,
    detail: str | None = None,
    commit: bool = True,
) -> None:
    db.add(
        AuthEvent(
            event_type=event_type,
            game_id=game_id,
            device_id=device_id,
            card_id=card_id,
            detail=(detail or "")[:255] or None,
        )
    )
    if commit:
        db.commit()


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
