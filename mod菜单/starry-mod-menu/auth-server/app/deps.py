from fastapi import Depends, HTTPException, Request
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.orm import Session

from .database import get_db
from .security import decode_admin_token
from .security_guard import assert_admin_ip_allowed

bearer = HTTPBearer(auto_error=False)


def require_admin(
    request: Request,
    creds: HTTPAuthorizationCredentials | None = Depends(bearer),
    db: Session = Depends(get_db),
):
    assert_admin_ip_allowed(request)
    if not creds:
        raise HTTPException(401, "未登录管理端")
    payload = decode_admin_token(creds.credentials)
    if not payload:
        raise HTTPException(401, "管理端登录已失效")
    return payload
