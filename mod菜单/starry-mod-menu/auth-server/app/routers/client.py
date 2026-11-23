from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session

from ..database import get_db
from ..models import Game, SessionToken
from ..schemas import HeartbeatRequest, LoginRequest, PresenceRequest, UnbindRequest
from ..security_guard import client_ip
from ..services import auth_service, dropstats, trust
from ..services.events import record_failure_event, record_guest_seen
from ..services.presence import touch_presence
from ..security_guard import (
    assert_bootstrap_rate_allowed,
    assert_card_login_not_rate_limited,
    clear_card_login_failures,
    record_card_login_failure,
)

router = APIRouter(prefix="/v1", tags=["client"])


def _deny_reason(status_code: int, detail: str) -> str:
    """把异常映射成稳定的原因码（只存码，不存自由文本/凭证）。"""
    text = detail or ""
    if "设备标识无效" in text:
        return "bad_device"
    if status_code == 404 or "不存在" in text:
        return "not_found"
    if "封禁" in text:
        return "banned"
    if "过期" in text:
        return "expired"
    if "用尽" in text or "次数" in text:
        return "exhausted"
    if "设备" in text and "绑定" in text:
        return "device_mismatch"
    if status_code == 401:
        return "unauthorized"
    if status_code == 429:
        return "rate_limited"
    return "denied"


@router.post("/auth/login")
def login(body: LoginRequest, request: Request, db: Session = Depends(get_db)):
    assert_card_login_not_rate_limited(request)
    try:
        data = auth_service.login_card(
            db, body.card_key, body.game_id, body.device_id, body.legacy_device_ids,
            request=request,
        )
        clear_card_login_failures(request)
        return {"ok": True, "message": "登录成功", "data": data}
    except HTTPException as exc:
        detail = str(exc.detail)
        if exc.status_code in (401, 403, 404):
            record_card_login_failure(request)
        # 失败事件按 (game, device/fp, reason, ip) 聚合：首条完整，重复累加 suppressed
        record_failure_event(
            db,
            "login_fail",
            game_id=body.game_id,
            device_id=body.device_id,
            reason=_deny_reason(exc.status_code, detail),
            detail=None,
            request=request,
        )
        raise


@router.post("/auth/heartbeat")
def heartbeat(body: HeartbeatRequest, request: Request, db: Session = Depends(get_db)):
    try:
        data = auth_service.heartbeat(
            db, body.token, body.game_id, body.device_id, body.legacy_device_ids,
            request=request,
        )
        return {"ok": True, "message": "ok", "data": data}
    except HTTPException as exc:
        # 心跳成功不写事件（避免刷屏）；只在状态变化/被拒时记一条，便于按设备追溯
        record_failure_event(
            db,
            "heartbeat_fail",
            game_id=body.game_id,
            device_id=body.device_id,
            reason=_deny_reason(exc.status_code, str(exc.detail)),
            request=request,
        )
        raise


@router.post("/auth/unbind")
def unbind(body: UnbindRequest, request: Request, db: Session = Depends(get_db)):
    def _q_reason(exc: HTTPException, text: str) -> str:
        if "尚未绑定" in text:
            return "not_bound"
        if "只能由当前绑定设备" in text:
            return "device_mismatch"
        return _deny_reason(exc.status_code, text)

    try:
        data = auth_service.unbind_card(
            db, body.card_key, body.device_id, request=request
        )
        return {"ok": True, "message": data["message"], "data": data}
    except HTTPException as exc:
        # 失败也要可追溯：按 (device/fp, reason, ip) 聚合，首条完整
        record_failure_event(
            db,
            "unbind_fail",
            device_id=body.device_id,
            reason=_q_reason(exc, str(exc.detail)),
            request=request,
        )
        raise


@router.get("/bootstrap")
def bootstrap(
    request: Request,
    game_id: str,
    device_id: str | None = None,
    logged_in: bool = False,
    client_version: str | None = None,
    client_version_code: int = 0,
    cert_sha256: str | None = None,
    db: Session = Depends(get_db),
):
    """公开启动配置。

    2026-11-23（游客统计）：
    - 游客（未带卡密/会话）同样是真实用户，bootstrap 会刷新在线表（identity=guest）；
    - 可疑上报（设备号形态非法 / 证书不在白名单 / 版本过低 / IP 突发）**直接丢弃**：
      不写在线表、不写事件明细，只在服务器本地累计次数（dropstats，后台不展示）；
    - 丢弃只影响统计，**接口照常返回 200 与完整配置**，老包也不会断服。
    """
    assert_bootstrap_rate_allowed(request)
    ip = client_ip(request)
    reason = trust.judge(
        device_id=device_id,
        cert_sha256=cert_sha256,
        version_code=client_version_code,
        ip=ip,
    )
    if reason:
        dropstats.note_drop(reason, game_id)
    data = auth_service.bootstrap_with_grant(
        db,
        game_id,
        device_id=device_id,
        logged_in=logged_in,
        count_presence=(reason is None),
    )
    return {"ok": True, "message": "ok", "data": data}


@router.post("/presence")
def presence(body: PresenceRequest, request: Request, db: Session = Depends(get_db)):
    """在线心跳（2026-11-23）：游客与卡密共用，5 分钟一次，只刷新在线表。

    - 不带 token 或 token 无效 ⇒ 按游客记录（identity=guest，不写 card_id）；
    - 带合法 token 且设备/游戏一致 ⇒ 顺带写认证事实（identity=card）；
    - 可疑上报直接丢弃（同 bootstrap），**不影响功能**，客户端也不应据此做任何拦截。
    """
    reason = trust.judge(
        device_id=body.device_id,
        cert_sha256=body.cert_sha256,
        version_code=body.client_version_code,
        ip=client_ip(request),
    )
    if reason:
        dropstats.note_drop(reason, body.game_id)
        return {"ok": True, "message": "ok", "data": {"counted": False}}

    game = db.query(Game).filter(Game.game_id == body.game_id).first()
    if game is None:
        # 未登记 game_id 不进统计（与看板口径一致）
        return {"ok": True, "message": "ok", "data": {"counted": False}}

    card_id = None
    token = (body.token or "").strip()
    if token:
        session = (
            db.query(SessionToken)
            .filter(SessionToken.token == token, SessionToken.revoked.is_(False))
            .first()
        )
        if (
            session is not None
            and session.game_id == body.game_id
            and session.device_id == body.device_id
        ):
            card_id = session.card_id

    if card_id:
        touch_presence(db, body.game_id, body.device_id, card_id=card_id, verified=True)
    else:
        touch_presence(db, body.game_id, body.device_id)
        # 设备日志要能看到游客：同设备同游戏 24 小时最多 1 条
        record_guest_seen(
            db,
            game_id=body.game_id,
            device_id=body.device_id,
            request=request,
            commit=False,
        )
    db.commit()
    return {
        "ok": True,
        "message": "ok",
        "data": {
            "counted": True,
            "identity": "card" if card_id else "guest",
        },
    }
