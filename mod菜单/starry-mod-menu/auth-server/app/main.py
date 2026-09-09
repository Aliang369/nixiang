from pathlib import Path

from fastapi import FastAPI, HTTPException, Request
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse, RedirectResponse
from fastapi.staticfiles import StaticFiles

from sqlalchemy import text

from .config import get_settings
from .database import Base, SessionLocal, engine
from .models import AdminUser, FeatureFlag, Game
from .routers import admin, client, ota
from .security import hash_password
from .services.feature_sync import sync_game_features
from .services.menu_templates import resolve_template_id, seed_builtin_templates

settings = get_settings()
app = FastAPI(title=settings.app_name, version="1.0.0", docs_url="/docs", redoc_url="/redoc")

origins = [o.strip() for o in settings.cors_origins.split(",") if o.strip()]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins if origins != ["*"] else ["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.exception_handler(HTTPException)
async def http_exc_handler(_: Request, exc: HTTPException):
    detail = exc.detail
    if not isinstance(detail, str):
        detail = str(detail)
    return JSONResponse(
        status_code=exc.status_code,
        content={"ok": False, "message": detail, "data": None},
    )


@app.exception_handler(RequestValidationError)
async def validation_handler(_: Request, exc: RequestValidationError):
    return JSONResponse(
        status_code=422,
        content={"ok": False, "message": _human_validation_message(exc), "data": exc.errors()},
    )


# 请求参数中文名映射：422 提示给玩家/管理员看，必须能看懂
_FIELD_LABELS = {
    "card_key": "账号", "game_id": "游戏", "device_id": "设备标识",
    "token": "会话", "username": "用户名", "password": "密码",
    "key_code": "卡密", "count": "数量", "prefix": "前缀",
    "title": "标题", "content": "内容", "note": "备注",
    "start_at": "开始时间", "end_at": "结束时间", "expire_at": "到期时间",
    "feat_num": "功能编号", "name": "名称", "template_id": "模板ID",
    "scope": "范围", "menu_template": "菜单模板", "action": "动作",
    "max_uses": "次数上限", "used_count": "已用次数", "status": "状态",
}


def _human_validation_message(exc: RequestValidationError) -> str:
    parts = []
    for err in exc.errors()[:3]:
        loc = [str(x) for x in err.get("loc", ()) if x != "body"]
        field = loc[-1] if loc else ""
        label = _FIELD_LABELS.get(field, field or "参数")
        etype = err.get("type", "")
        ctx = err.get("ctx") or {}
        if etype == "missing":
            parts.append(f"缺少{label}")
        elif etype == "string_too_short":
            min_len = ctx.get("min_length")
            parts.append(f"{label}不能为空" if min_len == 1 else f"{label}至少{min_len}位")
        elif etype == "string_too_long":
            parts.append(f"{label}最多{ctx.get('max_length', '?')}位")
        elif etype in ("int_parsing", "float_parsing", "int_from_float"):
            parts.append(f"{label}须为数字")
        elif etype == "string_pattern_mismatch":
            parts.append(f"{label}格式不正确")
        else:
            parts.append(f"{label}参数不正确")
    return "参数错误：" + "；".join(parts) if parts else "参数校验失败"


@app.exception_handler(Exception)
async def unhandled(_: Request, exc: Exception):
    msg = str(exc) if exc else "服务器内部错误"
    if msg and not any("\u4e00" <= ch <= "\u9fff" for ch in msg):
        msg = "服务器内部错误"
    return JSONResponse(status_code=500, content={"ok": False, "message": msg, "data": None})


def ensure_schema() -> None:
    """已有库补列（create_all 不会 ALTER）。"""
    with engine.begin() as conn:
        dialect = engine.dialect.name
        if dialect == "sqlite":
            game_cols = {r[1] for r in conn.execute(text("PRAGMA table_info(games)")).fetchall()}
            if "menu_template" not in game_cols:
                conn.execute(text("ALTER TABLE games ADD COLUMN menu_template VARCHAR(64)"))
            if "card_auth_enabled" not in game_cols:
                conn.execute(
                    text("ALTER TABLE games ADD COLUMN card_auth_enabled BOOLEAN DEFAULT 1")
                )
            ann_cols = {
                r[1] for r in conn.execute(text("PRAGMA table_info(announcements)")).fetchall()
            }
            if "visibility" not in ann_cols:
                conn.execute(
                    text(
                        "ALTER TABLE announcements ADD COLUMN visibility VARCHAR(16) DEFAULT 'always'"
                    )
                )
            if "start_at" not in ann_cols:
                conn.execute(text("ALTER TABLE announcements ADD COLUMN start_at TIMESTAMP"))
            if "end_at" not in ann_cols:
                conn.execute(text("ALTER TABLE announcements ADD COLUMN end_at TIMESTAMP"))
            if "action_url" not in ann_cols:
                conn.execute(text("ALTER TABLE announcements ADD COLUMN action_url VARCHAR(512)"))
            if "action_label" not in ann_cols:
                conn.execute(text("ALTER TABLE announcements ADD COLUMN action_label VARCHAR(64)"))
            if "popup_mode" not in ann_cols:
                conn.execute(
                    text(
                        "ALTER TABLE announcements ADD COLUMN popup_mode VARCHAR(16) "
                        "DEFAULT 'every_open'"
                    )
                )
            toast_cols = {
                r[1] for r in conn.execute(text("PRAGMA table_info(toast_messages)")).fetchall()
            }
            if "sort_order" not in toast_cols:
                conn.execute(
                    text("ALTER TABLE toast_messages ADD COLUMN sort_order INTEGER DEFAULT 0")
                )
        else:
            conn.execute(
                text("ALTER TABLE games ADD COLUMN IF NOT EXISTS menu_template VARCHAR(64)")
            )
            conn.execute(
                text(
                    "ALTER TABLE games ADD COLUMN IF NOT EXISTS "
                    "card_auth_enabled BOOLEAN DEFAULT TRUE"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS "
                    "visibility VARCHAR(16) DEFAULT 'always'"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS start_at TIMESTAMPTZ"
                )
            )
            conn.execute(
                text("ALTER TABLE announcements ADD COLUMN IF NOT EXISTS end_at TIMESTAMPTZ")
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS action_url VARCHAR(512)"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS action_label VARCHAR(64)"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS "
                    "popup_mode VARCHAR(16) DEFAULT 'every_open'"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE toast_messages ADD COLUMN IF NOT EXISTS "
                    "sort_order INTEGER DEFAULT 0"
                )
            )


def seed_db() -> None:
    db = SessionLocal()
    try:
        # 先种模板（仅补缺失，不覆盖后台已改内容）
        seed_builtin_templates(db)

        if not db.query(AdminUser).filter(AdminUser.username == settings.admin_username).first():
            db.add(
                AdminUser(
                    username=settings.admin_username,
                    password_hash=hash_password(settings.admin_password),
                )
            )

        default_gid = settings.default_game_id
        g = db.query(Game).filter(Game.game_id == default_gid).first()
        if not g:
            tid = resolve_template_id(db, default_gid, None)
            g = Game(
                game_id=default_gid,
                name=settings.default_game_name,
                enabled=True,
                menu_template=tid,
            )
            db.add(g)
            db.flush()
            if tid:
                sync_game_features(db, g, template_id=tid, mode="fill")
        elif db.query(FeatureFlag).filter(FeatureFlag.game_id == default_gid).count() == 0:
            tid = resolve_template_id(db, default_gid, g.menu_template)
            if tid:
                sync_game_features(db, g, template_id=tid, mode="fill")

        # 公告 / 开屏 Toast 均不预置：由后台自行发布；避免每次重启在空表时又插一条欢迎公告
        db.commit()
    finally:
        db.close()


@app.on_event("startup")
def on_startup():
    Base.metadata.create_all(bind=engine)
    ensure_schema()
    seed_db()


app.include_router(client.router)
app.include_router(admin.router)
app.include_router(ota.client_router)
app.include_router(ota.admin_router)

static_dir = Path(__file__).resolve().parent.parent / "static" / "admin"
if static_dir.exists():
    app.mount("/admin", StaticFiles(directory=str(static_dir), html=True), name="admin")


@app.get("/health")
def health():
    return {"ok": True, "app": settings.app_name}


@app.get("/")
def root():
    return RedirectResponse(url="/admin/")
