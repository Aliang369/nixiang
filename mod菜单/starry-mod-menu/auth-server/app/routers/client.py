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
