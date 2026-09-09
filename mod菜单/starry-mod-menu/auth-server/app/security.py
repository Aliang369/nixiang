from datetime import datetime, timedelta, timezone
from typing import Any, Optional

from jose import JWTError, jwt
from passlib.context import CryptContext

from .config import get_settings

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")
ALGORITHM = "HS256"


def utcnow() -> datetime:
    return datetime.now(timezone.utc)


def hash_password(password: str) -> str:
    return pwd_context.hash(password)


def verify_password(plain: str, hashed: str) -> bool:
    return pwd_context.verify(plain, hashed)


def create_admin_token(username: str) -> str:
    settings = get_settings()
    expire = utcnow() + timedelta(hours=12)
    payload = {"sub": username, "role": "admin", "exp": expire}
    return jwt.encode(payload, settings.secret_key, algorithm=ALGORITHM)


def decode_admin_token(token: str) -> Optional[dict[str, Any]]:
    settings = get_settings()
    try:
        payload = jwt.decode(token, settings.secret_key, algorithms=[ALGORITHM])
        if payload.get("role") != "admin":
            return None
        return payload
    except JWTError:
        return None


def add_duration(base: datetime, duration_type: str) -> Optional[datetime]:
    if duration_type == "hour":
        return base + timedelta(hours=1)
    if duration_type == "day":
        return base + timedelta(days=1)
    if duration_type == "month":
        return base + timedelta(days=30)
    if duration_type == "year":
        return base + timedelta(days=365)
    if duration_type == "permanent":
        return None
    if duration_type == "count":
        # 纯次数卡无固定到期；由次数耗尽控制
        return None
    raise ValueError(f"未知时长类型: {duration_type}")
