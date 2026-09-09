"""客户端 / 管理端：玩法 OTA（latest 单版本，点更新下载）。"""
from __future__ import annotations

from fastapi import APIRouter, Depends, File, Form, HTTPException, Request, UploadFile
from fastapi.responses import FileResponse
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.orm import Session

from ..config import get_settings
from ..database import get_db
from ..deps import require_admin
from ..models import Game, SessionToken
from ..security import utcnow
from ..security_guard import (
    assert_ota_json_allowed,
    assert_ota_manifest_allowed,
    assert_ota_so_allowed,
)
from ..services import ota_store

client_router = APIRouter(prefix="/v1/ota", tags=["ota"])
admin_router = APIRouter(prefix="/v1/admin/ota", tags=["admin-ota"])
_bearer = HTTPBearer(auto_error=False)


def _game_card_auth_enabled(db: Session, game_id: str) -> bool:
    g = db.query(Game).filter(Game.game_id == game_id).first()
    if g is None:
        return True
    return bool(getattr(g, "card_auth_enabled", True))


def _require_player_session(
    db: Session,
    creds: HTTPAuthorizationCredentials | None,
    game_id: str,
) -> None:
    """so 大文件：校验卡密会话 token（与 heartbeat 同源）。"""
    if not creds or not creds.credentials:
        raise HTTPException(401, "下载 so 请先登录卡密")
    token = creds.credentials.strip()
    session = db.query(SessionToken).filter(SessionToken.token == token).first()
    if not session:
        raise HTTPException(401, "登录已失效，请重新登录后再更新补丁")
    if getattr(session, "revoked", False):
        raise HTTPException(401, "登录已失效，请重新登录后再更新补丁")
    now = utcnow()
    exp = session.expires_at
    if exp is not None:
        if exp.tzinfo is None:
            from datetime import timezone

            exp = exp.replace(tzinfo=timezone.utc)
        if exp <= now:
            raise HTTPException(401, "登录已过期，请重新登录后再更新补丁")
    if session.game_id and session.game_id != game_id:
        raise HTTPException(403, "会话与当前游戏不匹配")


def _need_ota_token(db: Session, game_id: str, setting_flag: bool) -> bool:
    """全局开关开着，且该游戏仍启用卡密时，才要求会话 token。
    后台关掉游戏卡密（开放授权）时不应拦补丁下载。
    """
    if not setting_flag:
        return False
    return _game_card_auth_enabled(db, game_id)


@client_router.get("/{game_id}/manifest")
def get_manifest(game_id: str, request: Request):
    assert_ota_manifest_allowed(request)
    man = ota_store.read_manifest(game_id)
    if not man:
        raise HTTPException(status_code=404, detail="暂无游戏补丁")
    return {"ok": True, "message": "ok", "data": man}


@client_router.get("/{game_id}/file/{name}")
def get_file(
    game_id: str,
    name: str,
    request: Request,
    db: Session = Depends(get_db),
    creds: HTTPAuthorizationCredentials | None = Depends(_bearer),
):
    if name == "libstarry_magic.so":
        assert_ota_so_allowed(request)
        if _need_ota_token(db, game_id, get_settings().ota_so_require_token):
            _require_player_session(db, creds, game_id)
    elif name == "patch.json":
        assert_ota_json_allowed(request)
        if _need_ota_token(db, game_id, get_settings().ota_json_require_token):
            _require_player_session(db, creds, game_id)
    elif name == "manifest.json":
        assert_ota_manifest_allowed(request)
    else:
        raise HTTPException(status_code=404, detail="文件不存在")

    path = ota_store.file_path(game_id, name)
    if not path:
        raise HTTPException(status_code=404, detail="文件不存在")
    media = "application/json" if name.endswith(".json") else "application/octet-stream"
    return FileResponse(path, media_type=media, filename=name)


@admin_router.get("")
def admin_list_ota(_admin=Depends(require_admin)):
    """已发布补丁列表（每游戏 latest 一行）。"""
    rows = ota_store.list_latest()
    return {"ok": True, "message": "ok", "data": rows}


@admin_router.get("/{game_id}/manifest")
def admin_get_manifest(game_id: str, _admin=Depends(require_admin)):
    man = ota_store.read_manifest(game_id)
    if not man:
        return {"ok": True, "message": "empty", "data": None}
    return {"ok": True, "message": "ok", "data": man}


@admin_router.post("/{game_id}/publish")
async def admin_publish(
    game_id: str,
    ver: int = Form(...),
    note: str = Form(""),
    patch_json: UploadFile | None = File(None),
    so_file: UploadFile | None = File(None),
    _admin=Depends(require_admin),
):
    try:
        pj = await patch_json.read() if patch_json is not None else None
        so = await so_file.read() if so_file is not None else None
        if pj is not None and len(pj) == 0:
            pj = None
        if so is not None and len(so) == 0:
            so = None
        man = ota_store.publish(game_id, ver, note=note, patch_json=pj, so_bytes=so)
        return {"ok": True, "message": "已合并发布 latest（未上传的一侧保留旧文件）", "data": man}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e)) from e
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"发布失败: {e}") from e


@admin_router.delete("/{game_id}")
def admin_clear(
    game_id: str,
    what: str = "all",
    _admin=Depends(require_admin),
):
    """删除补丁：what=all|json|so。换整包后可 all 清掉，省磁盘。"""
    try:
        data = ota_store.clear(game_id, what=what)
        msg = {
            "all": "已删除该游戏全部补丁",
            "json": "已删除 JSON（so 保留）",
            "so": "已删除 so（JSON 保留）",
        }.get(what, "已清理")
        return {"ok": True, "message": msg, "data": data}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e)) from e
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"删除失败: {e}") from e
