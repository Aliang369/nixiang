from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session

from ..database import get_db
from ..schemas import HeartbeatRequest, LoginRequest, UnbindRequest
from ..services import auth_service
from ..services.events import record_auth_event
from ..security_guard import (
    assert_card_login_not_rate_limited,
    clear_card_login_failures,
    record_card_login_failure,
)

router = APIRouter(prefix="/v1", tags=["client"])


@router.post("/auth/login")
def login(body: LoginRequest, request: Request, db: Session = Depends(get_db)):
    assert_card_login_not_rate_limited(request)
    try:
        data = auth_service.login_card(
            db, body.card_key, body.game_id, body.device_id, body.legacy_device_ids
        )
        clear_card_login_failures(request)
        return {"ok": True, "message": "登录成功", "data": data}
    except HTTPException as exc:
        if exc.status_code in (401, 403, 404):
            record_card_login_failure(request)
        record_auth_event(
            db,
            "login_fail",
            game_id=body.game_id,
            device_id=body.device_id,
            detail=str(exc.detail)[:200],
        )
        raise


@router.post("/auth/heartbeat")
def heartbeat(body: HeartbeatRequest, db: Session = Depends(get_db)):
    data = auth_service.heartbeat(
        db, body.token, body.game_id, body.device_id, body.legacy_device_ids
    )
    return {"ok": True, "message": "ok", "data": data}


@router.post("/auth/unbind")
def unbind(body: UnbindRequest, db: Session = Depends(get_db)):
    data = auth_service.unbind_card(db, body.card_key, body.device_id)
    return {"ok": True, "message": data["message"], "data": data}


@router.get("/bootstrap")
def bootstrap(
    game_id: str,
    device_id: str | None = None,
    logged_in: bool = False,
    db: Session = Depends(get_db),
):
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
