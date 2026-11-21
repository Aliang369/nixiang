from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session

from ..database import get_db
from ..deviceid import device_fingerprint, is_valid_device_id
from ..schemas import HeartbeatRequest, LoginRequest, UnbindRequest
from ..services import auth_service
from ..services.events import record_auth_event, record_failure_event
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
    db: Session = Depends(get_db),
):
    """公开启动配置。

    2026-11-15 起：
    - device_id 形态不合法 → **不写在线表、不计活跃**，但照常返回 200 与完整配置
      （旧包未升级也不能断服）；
    - 不再依据 logged_in 自报布尔改写 card 关联。
    """
    assert_bootstrap_rate_allowed(request)
    if device_id and not is_valid_device_id(device_id):
        record_failure_event(
            db,
            "bootstrap_reject",
            game_id=game_id,
            device_id=device_id,
            reason="bad_device",
            request=request,
        )
    data = auth_service.bootstrap_with_grant(
        db, game_id, device_id=device_id, logged_in=logged_in
    )
    return {"ok": True, "message": "ok", "data": data}


@router.get("/build/payload-status")
def build_payload_status(game_id: str, sha: str = "", request: Request = None):
    """构建机自查：该游戏的载荷 sha 是否已在后台登记。需要 X-Build-Key。"""
    import hmac as _hmac
    from ..config import get_settings
    from ..services import grant_service as gs
    from ..models import Game
    from ..database import SessionLocal

    expect = (get_settings().build_api_key or "").strip()
    if not expect:
        raise HTTPException(404, "未启用")
    got = (request.headers.get("x-build-key") or "").strip()
    if not _hmac.compare_digest(got, expect):
        raise HTTPException(403, "build key 不正确")

    gid = (game_id or "").strip()
    if not gid:
        raise HTTPException(400, "缺少 game_id")

    db = SessionLocal()
    try:
        row = db.query(Game).filter(Game.game_id == gid).first()
        if not row:
            raise HTTPException(404, "游戏不存在：%s" % gid)
        registered = (row.payload_sha or "").strip().lower()
    finally:
        db.close()

    local = (sha or "").strip().lower()
    return {
        "game_id": gid,
        "registered_sha": registered,
        "local_sha": local,
        "match": bool(local) and local == registered,
        "registered": bool(registered),
        "v3_ready": gs.payload_ready(gid),
        "k_build_fp": (gs.payload_build_key(gid) or b"").hex()[:16],
    }
