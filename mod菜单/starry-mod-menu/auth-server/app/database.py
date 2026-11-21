import re

from sqlalchemy import create_engine, event
from sqlalchemy.orm import DeclarativeBase, sessionmaker

from .config import get_settings

settings = get_settings()

connect_args = {"check_same_thread": False} if settings.database_url.startswith("sqlite") else {}
engine = create_engine(settings.database_url, pool_pre_ping=True, connect_args=connect_args)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# SQLite 没有内置 REGEXP：注册后 deviceid.trusted_device_clause() 在本地/测试库同样可用。
# 调用顺序是 regexp(pattern, value)（SQL 写作 value REGEXP pattern）。
if settings.database_url.startswith("sqlite"):

    @event.listens_for(engine, "connect")
    def _sqlite_regexp(dbapi_conn, _record):  # pragma: no cover - 驱动回调
        dbapi_conn.create_function(
            "regexp",
            2,
            lambda pattern, value: 1
            if value is not None and re.search(pattern, str(value))
            else 0,
        )


class Base(DeclarativeBase):
    pass


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
