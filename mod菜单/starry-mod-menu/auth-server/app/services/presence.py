"""在线/活跃设备写入（2026-11-15 可信化）。

三条纪律：
1. 只接受形态合法的 device_id（见 app/deviceid.py）；非法串不落库、不进统计。
2. 只有**已认证**的调用（卡密 login / 带 token 的 heartbeat 成功）才能写 card_id 与
   card_verified_at；bootstrap 的 logged_in 是客户端自报布尔，不得当认证使用。
3. 任何地方都不许把 card_id 清成 NULL 来「表示游客」——那会让统计随最后一次请求漂移。
   需要解除关联时走 clear_card_presence()，由撤销/解绑等显式业务动作触发。
"""

from __future__ import annotations

from datetime import datetime, timedelta, timezone

from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from ..deviceid import is_valid_device_id
from ..models import OnlinePresence
from ..security import utcnow

# 同一设备 5 秒内的重复上报只更新一次，降低写放大（不影响在线判定，在线窗口是 90 秒）
WRITE_COALESCE_SECONDS = 5


def _aware(value: datetime | None) -> datetime | None:
    if value is not None and value.tzinfo is None:
        return value.replace(tzinfo=timezone.utc)
    return value

IDENTITY_CARD = "card"
IDENTITY_GUEST = "guest"


def _apply_identity(
    row: OnlinePresence,
    now: datetime,
    card_id: int | None,
    verified: bool,
) -> None:
    """统一维护身份快照（2026-11-23 游客统计）。

    - 认证上报（login / 带 token 的 heartbeat 成功）→ identity=card，并写
      card_id / card_verified_at / last_authenticated_at；
    - 未认证上报（bootstrap 游客上报）→ 只写 guest_seen_at；identity 仅在**当前不是 card**
      时才置 guest —— 登录过的设备不会因为一次游客上报被改回游客（否则统计会随最后一次
      请求漂移）。
    """
    row.last_seen_at = now
    if verified:
        row.card_id = card_id
        row.card_verified_at = now
        row.identity = IDENTITY_CARD
        # 事实列只前进不倒退（并发请求携带的 now 可能先后颠倒）
        prev = _aware(row.last_authenticated_at)
        row.last_authenticated_at = now if prev is None or now > prev else prev
        return
    row.guest_seen_at = now
    if row.identity != IDENTITY_CARD:
        row.identity = IDENTITY_GUEST


def touch_presence(
    db: Session,
    game_id: str,
    device_id: str,
    *,
    card_id: int | None = None,
    verified: bool = False,
) -> bool:
    """刷新在线行。verified=True 表示调用方已通过 token/卡密认证。

    返回是否发生写入（未写=非法设备、或 5 秒内重复）。
    """
    if not is_valid_device_id(device_id):
        return False
    now = utcnow()
    row = (
        db.query(OnlinePresence)
        .filter(OnlinePresence.game_id == game_id, OnlinePresence.device_id == device_id)
        .first()
    )
    if row is not None:
        last = _aware(row.last_seen_at)
        if not verified and last is not None and (now - last) < timedelta(
            seconds=WRITE_COALESCE_SECONDS
        ):
            return False
        _apply_identity(row, now, card_id, verified)
        return True

    try:
        with db.begin_nested():
            db.add(
                OnlinePresence(
                    game_id=game_id,
                    device_id=device_id,
                    card_id=card_id if verified else None,
                    card_verified_at=now if verified else None,
                    last_authenticated_at=now if verified else None,
                    last_seen_at=now,
                    created_at=now,  # 只有新行知道「首次观察时间」；历史行为 NULL=unknown
                    # 2026-11-23：身份快照与游客上报时间（游客行 card_id 恒为 NULL）
                    identity=IDENTITY_CARD if verified else IDENTITY_GUEST,
                    guest_seen_at=None if verified else now,
                )
            )
        return True
    except IntegrityError:
        # 并发插入：唯一键冲突说明另一请求已建行，改用更新路径（不抛 500、不丢 card 写入）
        row = (
            db.query(OnlinePresence)
            .filter(OnlinePresence.game_id == game_id, OnlinePresence.device_id == device_id)
            .first()
        )
        if row is None:
            return False
        _apply_identity(row, now, card_id, verified)
        return True


def clear_card_presence(db: Session, card_id: int) -> int:
    """显式解除某张卡的设备关联（撤销/封禁/过期/解绑/迁移时调用）。

    只清 card_id 与 card_verified_at，保留在线行与 created_at：设备仍然在线，但不再算
    「卡密关联设备」。**last_authenticated_at 不清**：解绑后设备仍有过认证活动的事实，
    活跃≠持卡。调用方负责 commit。
    """
    return (
        db.query(OnlinePresence)
        .filter(OnlinePresence.card_id == card_id)
        .update(
            {"card_id": None, "card_verified_at": None, "identity": IDENTITY_GUEST},
            synchronize_session=False,
        )
    )
