import uuid
from pathlib import Path

from fastapi import FastAPI, HTTPException, Request
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles

from sqlalchemy import text

from .config import get_settings
from .database import Base, SessionLocal, engine
from .models import AdminUser, CardDevice, FeatureFlag, Game
from .routers import admin, client, public
from .security import hash_password
from .services.feature_sync import sync_game_features
from .services.menu_templates import resolve_template_id, seed_builtin_templates

settings = get_settings()
app = FastAPI(
    title=settings.app_name,
    version="1.0.0",
    # 生产默认不暴露接口清单；本地调试可在 .env 设 ENABLE_DOCS=true
    docs_url="/docs" if settings.enable_docs else None,
    redoc_url="/redoc" if settings.enable_docs else None,
    openapi_url="/openapi.json" if settings.enable_docs else None,
)

origins = [o.strip() for o in settings.cors_origins.split(",") if o.strip()]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins if origins != ["*"] else ["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.middleware("http")
async def request_id_middleware(request: Request, call_next):
    """为每个请求生成服务端 request_id，并回写响应头，便于日志串联。

    不接受客户端自传的 X-Request-Id（可伪造），一律服务端生成。
    """
    rid = uuid.uuid4().hex[:32]
    request.state.request_id = rid
    response = await call_next(request)
    response.headers["X-Request-Id"] = rid
    return response


@app.exception_handler(RequestValidationError)
async def validation_exc_handler(request: Request, exc: RequestValidationError):
    """Pydantic 校验失败（如缺字段/超长）也要留痕，避免「坏请求无审计」。"""
    from .services.events import record_failure_event

    db = SessionLocal()
    try:
        record_failure_event(
            db,
            "request_invalid",
            device_id=None,
            reason="validation_error",
            request=request,
        )
    except Exception:  # 审计失败不得影响错误响应
        db.rollback()
    finally:
        db.close()
    return JSONResponse(
        status_code=422,
        content={"ok": False, "message": "请求参数不合法", "data": None},
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
            if "download_url" not in game_cols:
                conn.execute(
                    text("ALTER TABLE games ADD COLUMN download_url VARCHAR(512)")
                )
            if "updated_at" not in game_cols:
                conn.execute(
                    text("ALTER TABLE games ADD COLUMN updated_at TIMESTAMP")
                )
            card_cols = {
                r[1] for r in conn.execute(text("PRAGMA table_info(card_keys)")).fetchall()
            }
            if "legacy_device_ids" not in card_cols:
                conn.execute(text("ALTER TABLE card_keys ADD COLUMN legacy_device_ids TEXT"))
            if "max_devices" not in card_cols:
                conn.execute(
                    text("ALTER TABLE card_keys ADD COLUMN max_devices INTEGER DEFAULT 1")
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
            # 2026-11-23 公告卡三按钮（关闭程序 / 跳转 / 关闭进游戏）
            if "exit_enabled" not in ann_cols:
                conn.execute(
                    text(
                        "ALTER TABLE announcements ADD COLUMN exit_enabled "
                        "BOOLEAN DEFAULT 0"
                    )
                )
            if "exit_label" not in ann_cols:
                conn.execute(
                    text("ALTER TABLE announcements ADD COLUMN exit_label VARCHAR(32)")
                )
            if "close_label" not in ann_cols:
                conn.execute(
                    text("ALTER TABLE announcements ADD COLUMN close_label VARCHAR(32)")
                )
            toast_cols = {
                r[1] for r in conn.execute(text("PRAGMA table_info(toast_messages)")).fetchall()
            }
            if "sort_order" not in toast_cols:
                conn.execute(
                    text("ALTER TABLE toast_messages ADD COLUMN sort_order INTEGER DEFAULT 0")
                )
        elif dialect.startswith("mysql") or dialect.startswith("mariadb"):
            # MySQL 5.7 不支持 ADD COLUMN IF NOT EXISTS，只能先查 information_schema 再决定。
            # 新表（game_channels / update_requests）由上面的 create_all 自动建立，这里只补老表新增列。
            # 2026-11-22：「请求更新」用的游戏级开关
            game_cols = {
                r[0]
                for r in conn.execute(
                    text(
                        "SELECT COLUMN_NAME FROM information_schema.COLUMNS "
                        "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'games'"
                    )
                ).fetchall()
            }
            if "request_updates_enabled" not in game_cols:
                conn.execute(
                    text(
                        "ALTER TABLE games ADD COLUMN request_updates_enabled "
                        "TINYINT(1) NOT NULL DEFAULT 1"
                    )
                )
            # 2026-11-23 公告卡三按钮：MySQL 5.7 无 IF NOT EXISTS，先查 information_schema
            ann_cols = {
                r[0]
                for r in conn.execute(
                    text(
                        "SELECT COLUMN_NAME FROM information_schema.COLUMNS "
                        "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'announcements'"
                    )
                ).fetchall()
            }
            if "exit_enabled" not in ann_cols:
                conn.execute(
                    text(
                        "ALTER TABLE announcements ADD COLUMN exit_enabled "
                        "TINYINT(1) NOT NULL DEFAULT 0"
                    )
                )
            if "exit_label" not in ann_cols:
                conn.execute(
                    text("ALTER TABLE announcements ADD COLUMN exit_label VARCHAR(32) NULL")
                )
            if "close_label" not in ann_cols:
                conn.execute(
                    text("ALTER TABLE announcements ADD COLUMN close_label VARCHAR(32) NULL")
                )
        elif dialect.startswith("postgres"):
            # 注意：以下均为 PostgreSQL 专有语法（ADD COLUMN IF NOT EXISTS /
            # ALTER COLUMN ... TYPE）。MySQL 不支持，会在启动时抛错。
            # 全新 MySQL 库的表结构完全由 create_all 建立，无需补列，故仅限 PG。
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
                text("ALTER TABLE games ADD COLUMN IF NOT EXISTS download_url VARCHAR(2048)")
            )
            conn.execute(
                text("ALTER TABLE games ALTER COLUMN download_url TYPE VARCHAR(2048)")
            )
            conn.execute(
                text("ALTER TABLE games ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ")
            )
            conn.execute(
                text("ALTER TABLE card_keys ADD COLUMN IF NOT EXISTS legacy_device_ids TEXT")
            )
            conn.execute(
                text("ALTER TABLE card_keys ADD COLUMN IF NOT EXISTS max_devices INTEGER DEFAULT 1")
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
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS "
                    "exit_enabled BOOLEAN DEFAULT FALSE"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS "
                    "exit_label VARCHAR(32)"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE announcements ADD COLUMN IF NOT EXISTS "
                    "close_label VARCHAR(32)"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE toast_messages ADD COLUMN IF NOT EXISTS "
                    "sort_order INTEGER DEFAULT 0"
                )
            )

        # 2026-11-15 遥测可信化：只做 additive 可空列（历史行保持 NULL = unknown，
        # 不把「加列时刻」伪装成首次观察时间）。索引不在这里建：auth_events 已是百万级，
        # 由 ops 用 CREATE INDEX CONCURRENTLY 单独执行，避免启动时锁表/超时。
        if dialect == "sqlite":
            pcols = {
                r[1]
                for r in conn.execute(text("PRAGMA table_info(online_presence)")).fetchall()
            }
            if "created_at" not in pcols:
                conn.execute(text("ALTER TABLE online_presence ADD COLUMN created_at TIMESTAMP"))
            if "card_verified_at" not in pcols:
                conn.execute(
                    text("ALTER TABLE online_presence ADD COLUMN card_verified_at TIMESTAMP")
                )
            if "last_authenticated_at" not in pcols:
                conn.execute(
                    text(
                        "ALTER TABLE online_presence ADD COLUMN "
                        "last_authenticated_at TIMESTAMP"
                    )
                )
            # 2026-11-23 游客统计：identity + guest_seen_at
            if "guest_seen_at" not in pcols:
                conn.execute(
                    text("ALTER TABLE online_presence ADD COLUMN guest_seen_at TIMESTAMP")
                )
            if "identity" not in pcols:
                conn.execute(
                    text("ALTER TABLE online_presence ADD COLUMN identity VARCHAR(8)")
                )
            ecols = {
                r[1] for r in conn.execute(text("PRAGMA table_info(auth_events)")).fetchall()
            }
            for col, ddl in (
                ("result", "VARCHAR(8)"),
                ("reason", "VARCHAR(32)"),
                ("ip", "VARCHAR(45)"),
                ("ip_forwarded", "VARCHAR(45)"),
                ("ua", "VARCHAR(120)"),
                ("request_id", "VARCHAR(36)"),
                ("suppressed", "INTEGER DEFAULT 0"),
                ("last_occurred_at", "TIMESTAMP"),
                ("identity", "VARCHAR(8)"),
            ):
                if col not in ecols:
                    conn.execute(text(f"ALTER TABLE auth_events ADD COLUMN {col} {ddl}"))
        elif dialect == "postgresql":
            conn.execute(
                text("ALTER TABLE online_presence ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ")
            )
            conn.execute(
                text(
                    "ALTER TABLE online_presence ADD COLUMN IF NOT EXISTS "
                    "card_verified_at TIMESTAMPTZ"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE online_presence ADD COLUMN IF NOT EXISTS "
                    "last_authenticated_at TIMESTAMPTZ"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE online_presence ADD COLUMN IF NOT EXISTS "
                    "guest_seen_at TIMESTAMPTZ"
                )
            )
            conn.execute(
                text(
                    "ALTER TABLE online_presence ADD COLUMN IF NOT EXISTS identity VARCHAR(8)"
                )
            )
            for col, ddl in (
                ("result", "VARCHAR(8)"),
                ("reason", "VARCHAR(32)"),
                ("ip", "VARCHAR(45)"),
                ("ip_forwarded", "VARCHAR(45)"),
                ("ua", "VARCHAR(120)"),
                ("request_id", "VARCHAR(36)"),
                ("suppressed", "INTEGER DEFAULT 0"),
                ("last_occurred_at", "TIMESTAMPTZ"),
                ("identity", "VARCHAR(8)"),
            ):
                conn.execute(
                    text(f"ALTER TABLE auth_events ADD COLUMN IF NOT EXISTS {col} {ddl}")
                )

        elif dialect in ("mysql", "mariadb"):
            # MySQL 5.7 不支持 ADD COLUMN IF NOT EXISTS：查 information_schema 再补。
            # 生产库历史上靠手工 ALTER，这里补齐后新环境可自动就位（幂等）。
            def _cols(table: str) -> set[str]:
                rows = conn.execute(
                    text(
                        "SELECT COLUMN_NAME FROM information_schema.COLUMNS "
                        "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = :t"
                    ),
                    {"t": table},
                ).fetchall()
                return {(r[0] or "").lower() for r in rows}

            pcols = _cols("online_presence")
            for col, ddl in (
                ("created_at", "DATETIME NULL"),
                ("card_verified_at", "DATETIME NULL"),
                ("last_authenticated_at", "DATETIME NULL"),
                ("identity", "VARCHAR(8) NULL"),
                ("guest_seen_at", "DATETIME NULL"),
            ):
                if col not in pcols:
                    conn.execute(text(f"ALTER TABLE online_presence ADD COLUMN {col} {ddl}"))
            ecols = _cols("auth_events")
            for col, ddl in (
                ("result", "VARCHAR(8) NULL"),
                ("reason", "VARCHAR(32) NULL"),
                ("ip", "VARCHAR(45) NULL"),
                ("ip_forwarded", "VARCHAR(45) NULL"),
                ("ua", "VARCHAR(120) NULL"),
                ("request_id", "VARCHAR(36) NULL"),
                ("suppressed", "INT NOT NULL DEFAULT 0"),
                ("last_occurred_at", "DATETIME NULL"),
                ("identity", "VARCHAR(8) NULL"),
            ):
                if col not in ecols:
                    conn.execute(text(f"ALTER TABLE auth_events ADD COLUMN {col} {ddl}"))

        # 一码多设备（2026-10-31）：新表由 create_all 建好；这里兜底建一次并回填老数据
        CardDevice.__table__.create(bind=conn, checkfirst=True)
        conn.execute(
            text(
                "INSERT INTO card_devices (card_id, device_id, bound_at, last_seen_at) "
                "SELECT c.id, c.device_id, "
                "COALESCE(c.activated_at, c.created_at), "
                "COALESCE(c.updated_at, c.activated_at, c.created_at) "
                "FROM card_keys c WHERE c.device_id IS NOT NULL AND c.device_id <> '' "
                "AND NOT EXISTS (SELECT 1 FROM card_devices d WHERE d.card_id = c.id)"
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
# 前台「请求更新」公开接口（2026-11-22）
app.include_router(public.router)

static_dir = Path(__file__).resolve().parent.parent / "static" / "admin"
if static_dir.exists():
    app.mount("/admin", StaticFiles(directory=str(static_dir), html=True), name="admin")


@app.get("/health")
def health():
    return {"ok": True, "app": settings.app_name}


@app.get("/")
def root():
    # 故意不跳转后台：根路径一律 404，避免直接暴露管理入口
    raise HTTPException(status_code=404, detail="Not Found")
