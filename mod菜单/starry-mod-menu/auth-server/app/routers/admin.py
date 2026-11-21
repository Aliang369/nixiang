from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends, HTTPException, Query, Request
from sqlalchemy import case, exists, func, or_
from sqlalchemy.orm import Session

from ..database import get_db
from ..deps import require_admin
from ..models import (
    AdminAuditLog,
    Announcement,
    AuthEvent,
    CardDevice,
    CardKey,
    FeatureFlag,
    Game,
    OnlinePresence,
    SessionToken,
    ToastMessage,
)
from ..schemas import (
    AnnouncementCreate,
    AnnouncementUpdate,
    CardBatchRequest,
    CardCreate,
    CardUpdate,
    FeatureCreate,
    FeatureUpdate,
    GameCreate,
    GameSyncFeaturesRequest,
    GameUpdate,
    MenuTemplateCreate,
    MenuTemplateUpdate,
    ToastCreate,
    ToastUpdate,
    AdminLoginRequest,
    AdminChangePasswordRequest,
)
from ..security import create_admin_token, hash_password, utcnow, verify_password
from ..models import AdminUser
from ..deviceid import mask_card_key, mask_device_id, trusted_device_clause
from ..services.events import failure_overflow_since_start
from ..services.presence import clear_card_presence
from ..services.feature_sync import sync_game_features
from ..services import menu_templates as menu_tpl
from ..services.auth_service import (
    generate_card_keys,
    create_custom_card_keys,
    normalize_custom_keys,
    purge_card_devices,
    refresh_card_status,
    revoke_card_sessions,
    serialize_card_public,
)
from ..services.events import record_admin_audit
from ..security_guard import (
    assert_admin_ip_allowed,
    assert_login_not_rate_limited,
    clear_login_failures,
    record_login_failure,
)

router = APIRouter(prefix="/v1/admin", tags=["admin"])


def _client_ip(request: Request | None) -> str | None:
    if request and request.client:
        return request.client.host
    return None


def _fmt_expire(v) -> str:
    """审计用到期时间格式（ISO 秒，UTC 原值）：只记时间，不写任何密钥/口令。"""
    if v is None:
        return "无"
    try:
        return v.isoformat(timespec="seconds")
    except TypeError:
        return str(v)


# 可表示时间边界（aware，UTC）：延期结果越界一律 422，不让 OverflowError 变 500
_DT_MIN = datetime.min.replace(tzinfo=timezone.utc)
_DT_MAX = datetime.max.replace(tzinfo=timezone.utc)


def _as_utc(v):
    """DB 读回的 naive 时间按 UTC 解释（SQLite 无 tz；Postgres/TIMESTAMPTZ 本来 aware）。"""
    if v is not None and v.tzinfo is None:
        return v.replace(tzinfo=timezone.utc)
    return v


def _clamp_page(page: int, page_size: int, max_size: int = 200) -> tuple[int, int]:
    return max(page, 1), min(max(page_size, 1), max_size)


def _page_meta(total: int, page: int, page_size: int) -> dict:
    return {"total": total, "page": page, "page_size": page_size}


def _parse_iso_dt(value: str | None, *, end_of_day: bool = False) -> datetime | None:
    if not value:
        return None
    raw = value.strip()
    if not raw:
        return None
    if raw.endswith("Z"):
        raw = raw[:-1] + "+00:00"
    try:
        dt = datetime.fromisoformat(raw)
    except ValueError as exc:
        raise HTTPException(400, f"时间格式无效: {value}") from exc
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    if end_of_day and len(value) <= 10:
        dt = dt.replace(hour=23, minute=59, second=59, microsecond=999999)
    return dt


def _game_dict(g: Game, feature_count: int | None = None) -> dict:
    data = {
        "id": g.id,
        "game_id": g.game_id,
        "name": g.name,
        "enabled": g.enabled,
        "card_auth_enabled": bool(getattr(g, "card_auth_enabled", True)),
        "menu_template": g.menu_template,
        "download_url": getattr(g, "download_url", None),
        "channel": getattr(g, "channel", None),
        "updated_at": g.updated_at.isoformat() if getattr(g, "updated_at", None) else None,
        "created_at": g.created_at.isoformat() if g.created_at else None,
    }
    if feature_count is not None:
        data["feature_count"] = feature_count
    return data


def _audit_dict(r: AdminAuditLog) -> dict:
    return {
        "id": r.id,
        "admin_username": r.admin_username,
        "action": r.action,
        "detail": r.detail,
        "ip": r.ip,
        "created_at": r.created_at.isoformat() if r.created_at else None,
    }


def _device_dict(row: CardDevice) -> dict:
    return {
        "device_id": row.device_id,
        "bound_at": row.bound_at.isoformat() if row.bound_at else None,
        "last_seen_at": row.last_seen_at.isoformat() if row.last_seen_at else None,
    }


def _card_dict(card: CardKey, devices: list[dict] | None = None) -> dict:
    """后台卡密行。devices 传该卡全部绑定设备（按绑定时间升序）。"""
    devs = devices or []
    base = serialize_card_public(
        card, device_count=len(devs) if devices is not None else None
    )
    base.update(
        {
            "id": card.id,
            "note": card.note,
            "device_id": card.device_id,
            "devices": devs,
            "created_at": card.created_at.isoformat() if card.created_at else None,
        }
    )
    return base


def _announcement_dict(a: Announcement) -> dict:
    return {
        "id": a.id,
        "scope": a.scope,
        "game_id": a.game_id,
        "title": a.title,
        "content": a.content,
        "enabled": a.enabled,
        "sort_order": a.sort_order,
        "visibility": getattr(a, "visibility", None) or "always",
        "popup_mode": getattr(a, "popup_mode", None) or "every_open",
        "start_at": a.start_at.isoformat() if getattr(a, "start_at", None) else None,
        "end_at": a.end_at.isoformat() if getattr(a, "end_at", None) else None,
        "action_url": getattr(a, "action_url", None) or None,
        "action_label": getattr(a, "action_label", None) or None,
    }


@router.post("/login")
def admin_login(body: AdminLoginRequest, request: Request, db: Session = Depends(get_db)):
    assert_admin_ip_allowed(request)
    assert_login_not_rate_limited(request)
    user = db.query(AdminUser).filter(AdminUser.username == body.username).first()
    if not user or not verify_password(body.password, user.password_hash):
        record_login_failure(request)
        raise HTTPException(401, "用户名或密码错误")
    clear_login_failures(request)
    token = create_admin_token(user.username)
    return {"ok": True, "message": "登录成功", "data": {"token": token, "username": user.username}}


@router.post("/change-password")
def change_password(
    body: AdminChangePasswordRequest,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    username = payload.get("sub")
    user = db.query(AdminUser).filter(AdminUser.username == username).first()
    if not user or not verify_password(body.old_password, user.password_hash):
        raise HTTPException(400, "原密码不正确")
    user.password_hash = hash_password(body.new_password)
    record_admin_audit(
        db,
        username or "unknown",
        "admin.change_password",
        detail="密码已更新",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "密码已更新，请重新登录"}


# 自然日按北京时间切（中国无夏令时，固定 +8）。
# 2026-09-05 修复：此前 today_start 用 UTC 0 点，等于北京时间早上 8 点才翻日，
# 「今日活跃/今日激活」凌晨 0 点不刷新。库里时间统一存 UTC，边界换算成 UTC 后再比较。
STATS_TZ = timezone(timedelta(hours=8))


@router.get("/dashboard")
def dashboard(_: dict = Depends(require_admin), db: Session = Depends(get_db)):
    now = utcnow()
    now_bj = now.astimezone(STATS_TZ)
    today_start = now_bj.replace(hour=0, minute=0, second=0, microsecond=0).astimezone(timezone.utc)
    online_cut = now - timedelta(seconds=90)

    # 只统计游戏列表里已登记的游戏：测试期客户端乱传的 game_id 不进看板
    registered_ids = {row[0] for row in db.query(Game.game_id).all()}
    # 2026-11-15：只统计形态合法的设备号（^dev_[0-9a-f]{32}$ 或 40 位）。
    trusted_device = trusted_device_clause(OnlinePresence.device_id)
    # 2026-11-16 活动口径（不新增 tile / 不改字段名）：
    # - 需卡游戏：活跃 = 服务端认证过的活动时间 last_authenticated_at（login/心跳成功写入，
    #   解绑/撤销不清），未认证的 bootstrap 不再计入；
    # - 免卡游戏：无认证链，沿用 last_seen_at 的观测值（明确不可信，但保留样本）。
    card_games = {
        gid
        for gid, card_auth in db.query(Game.game_id, Game.card_auth_enabled).all()
        if card_auth
    }
    activity_at = case(
        (OnlinePresence.game_id.in_(card_games), OnlinePresence.last_authenticated_at),
        else_=OnlinePresence.last_seen_at,
    )

    def _online_devices(extra=None) -> int:
        """总在线按设备去重（同机多游戏只计 1）。"""
        q = db.query(func.count(func.distinct(OnlinePresence.device_id))).filter(
            activity_at >= online_cut,
            OnlinePresence.game_id.in_(registered_ids),
            trusted_device,
        )
        if extra is not None:
            q = q.filter(extra)
        return q.scalar() or 0

    # 卡密设备：窗口内至少有一条带 card_id 的在线；游客 = 总设备 − 卡密设备（避免同机一卡一游客双计）
    total_online = _online_devices()
    online_card = _online_devices(OnlinePresence.card_id.isnot(None))
    online_guest = max(total_online - online_card, 0)

    # 自然日窗口（分游戏活跃与总活跃共用）
    d7_start = today_start - timedelta(days=6)
    d30_start = today_start - timedelta(days=29)

    def _by_game_count(since=None, extra=None):
        """分游戏计数。表上 (game_id, device_id) 唯一，故 count(id)=该游戏下去重设备数。"""
        q = db.query(OnlinePresence.game_id, func.count(OnlinePresence.id)).filter(
            OnlinePresence.game_id.in_(registered_ids),
            trusted_device,
            # 需卡游戏的历史活跃也只算有认证事实的行（all 窗口无 since，仍要排除 shape-only 行）
            activity_at.isnot(None),
        )
        if since is not None:
            q = q.filter(activity_at >= since)
        if extra is not None:
            q = q.filter(extra)
        return dict(q.group_by(OnlinePresence.game_id).all())

    # 分游戏：当前在线 + 7/30/历史活跃（看哪款玩的人多）
    by_game_total = _by_game_count(online_cut)
    by_game_card = _by_game_count(online_cut, OnlinePresence.card_id.isnot(None))
    by_game_guest = _by_game_count(online_cut, OnlinePresence.card_id.is_(None))
    by_game_d7 = _by_game_count(d7_start)
    by_game_d30 = _by_game_count(d30_start)
    by_game_all = _by_game_count(None)

    # 分游戏表只列已登记游戏（测试数据已在查询层过滤）
    game_ids = sorted(registered_ids)
    online_by_game = [
        {
            "game_id": gid,
            "online": by_game_total.get(gid, 0),
            "online_card": by_game_card.get(gid, 0),
            "online_guest": by_game_guest.get(gid, 0),
            "d7_active": by_game_d7.get(gid, 0),
            "d30_active": by_game_d30.get(gid, 0),
            "all_active": by_game_all.get(gid, 0),
        }
        for gid in game_ids
    ]
    # 默认按历史活跃排序（累计活跃设备数），方便看哪款累计玩的人最多
    online_by_game.sort(
        key=lambda r: (-r["all_active"], -r["d30_active"], -r["d7_active"], -r["online"], r["game_id"])
    )

    cards_total = db.query(func.count(CardKey.id)).scalar() or 0
    cards_active = db.query(func.count(CardKey.id)).filter(CardKey.status == "active").scalar() or 0
    cards_banned = db.query(func.count(CardKey.id)).filter(CardKey.status == "banned").scalar() or 0
    cards_unused = db.query(func.count(CardKey.id)).filter(CardKey.status == "unused").scalar() or 0

    # 今日激活 = 首次激活事件（activate）
    today_activate = (
        db.query(func.count(AuthEvent.id))
        .filter(AuthEvent.event_type == "activate", AuthEvent.created_at >= today_start)
        .scalar()
        or 0
    )

    def _active_split(since=None):
        """活跃按 device_id 去重。
        注意：同机若各游戏包算出不同 dev_*（指纹 v1/v2），会被当成多台设备。
        返回 (去重设备, 卡密设备, 游客设备)。
        """

        def _distinct(extra=None) -> int:
            q = db.query(func.count(func.distinct(OnlinePresence.device_id))).filter(
                OnlinePresence.game_id.in_(registered_ids),
                trusted_device,
                activity_at.isnot(None),
            )
            if since is not None:
                q = q.filter(activity_at >= since)
            if extra is not None:
                q = q.filter(extra)
            return q.scalar() or 0

        total = _distinct()
        card = _distinct(OnlinePresence.card_id.isnot(None))
        guest = max(total - card, 0)
        return total, card, guest

    today_active_devices, today_card_devices, today_guest_devices = _active_split(today_start)
    d7_active_devices, d7_card_devices, d7_guest_devices = _active_split(d7_start)
    d30_active_devices, d30_card_devices, d30_guest_devices = _active_split(d30_start)
    all_active_devices, all_card_devices, all_guest_devices = _active_split(None)

    def _verified_devices(since) -> int:
        """窗口内「成功验证过卡密」的设备：只认 login/heartbeat 写下的 card_verified_at。

        与 observed 的关系：verified ⊆ observed；unverified = observed - verified（互斥，
        不再用「card_verified_at IS NULL」单独判断，避免 stale 设备漏算）。
        """
        q = db.query(func.count(func.distinct(OnlinePresence.device_id))).filter(
            OnlinePresence.game_id.in_(registered_ids),
            trusted_device,
            OnlinePresence.last_authenticated_at.isnot(None),
        )
        if since is not None:
            q = q.filter(OnlinePresence.last_authenticated_at >= since)
        return q.scalar() or 0

    today_verified = _verified_devices(today_start)
    d7_verified = _verified_devices(d7_start)
    d30_verified = _verified_devices(d30_start)
    all_verified = _verified_devices(None)

    # 近 24 小时被拒的非法 device（只含 bad_ 指纹或计数，不含原始 payload）
    last_24h = now - timedelta(hours=24)
    untrusted_rejects_24h = (
        db.query(func.count(AuthEvent.id))
        .filter(AuthEvent.reason == "bad_device", AuthEvent.created_at >= last_24h)
        .scalar()
        or 0
    )

    games_count = db.query(func.count(Game.id)).scalar() or 0
    return {
        "ok": True,
        "data": {
            "total_online": total_online,
            "online_card": online_card,
            "online_guest": online_guest,  # 非卡密登录设备
            "online_by_game": online_by_game,
            "cards_total": cards_total,
            "cards_active": cards_active,
            "cards_banned": cards_banned,
            "cards_unused": cards_unused,
            "games_count": games_count,
            "today_activate": today_activate,
            "today_active_devices": today_active_devices,
            "today_card_devices": today_card_devices,
            "today_guest_devices": today_guest_devices,
            "d7_active_devices": d7_active_devices,
            "d7_card_devices": d7_card_devices,
            "d7_guest_devices": d7_guest_devices,
            "d30_active_devices": d30_active_devices,
            "d30_card_devices": d30_card_devices,
            "d30_guest_devices": d30_guest_devices,
            "all_active_devices": all_active_devices,
            "all_card_devices": all_card_devices,
            "all_guest_devices": all_guest_devices,
            # ---- 2026-11-15 新增：可信口径（与上面旧字段并列，不替换） ----
            "today_verified_devices": today_verified,
            "today_unverified_devices": max(today_active_devices - today_verified, 0),
            "d7_verified_devices": d7_verified,
            "d7_unverified_devices": max(d7_active_devices - d7_verified, 0),
            "d30_verified_devices": d30_verified,
            "d30_unverified_devices": max(d30_active_devices - d30_verified, 0),
            "all_verified_devices": all_verified,
            "all_unverified_devices": max(all_active_devices - all_verified, 0),
            "untrusted_rejects_24h": untrusted_rejects_24h,
            "log_overflow_since_start": failure_overflow_since_start(),
            "observed_definition": "格式合法设备号（dev_+32/40 位十六进制）且属已登记游戏",
            "verified_definition": "同一窗口内由 token/卡密登录成功确认过的设备（card_verified_at）",
            "data_note": (
                "设备数≠人数；活跃=online_presence 最后出现时间快照，不能还原任意历史日期，"
                "不声称严格 DAU；卡密关联设备/未关联设备是最后状态快照；"
                "历史行 created_at/card_verified_at 为空表示无证据（unknown），不做推测填充。"
            ),
        },
    }


# ----- menu templates (DB) -----
@router.get("/menu-templates")
def get_menu_templates(_: dict = Depends(require_admin), db: Session = Depends(get_db)):
    """菜单模板目录（数据库；与客户端 feat_num 对齐）。"""
    return {"ok": True, "data": menu_tpl.list_templates(db)}


@router.post("/menu-templates")
def create_menu_template(
    body: MenuTemplateCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    try:
        data = menu_tpl.create_template(
            db,
            template_id=body.template_id,
            name=body.name,
            package=body.package,
            source=body.source,
            features=[f.model_dump() for f in body.features],
        )
    except ValueError as e:
        raise HTTPException(400, str(e)) from e

    # 模板 → 游戏单向联动（2026-09-05）：
    # 1) 无同 game_id 游戏 → 自动创建（启用/需卡密），绑定模板并按模板同步功能
    # 2) 游戏存在但未绑模板 → 自动绑定（不覆盖已绑别的模板的游戏）
    # 反向（建游戏不建模板）不联动。
    linkage = {"game_created": False, "game_bound": False}
    g = db.query(Game).filter(Game.game_id == data["id"]).first()
    if not g:
        g = Game(
            game_id=data["id"],
            name=data["name"],
            enabled=True,
            card_auth_enabled=True,
            menu_template=data["id"],
        )
        db.add(g)
        db.flush()
        linkage["game_created"] = True
        linkage["synced_features"] = sync_game_features(
            db, g, template_id=data["id"], mode="fill"
        )
    elif not g.menu_template:
        g.menu_template = data["id"]
        linkage["game_bound"] = True

    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "menu_templates.create",
        detail=f"模板={data['id']} 名称={data['name']} 功能={data['feature_count']}",
        ip=_client_ip(request),
        commit=False,
    )
    if linkage["game_created"]:
        record_admin_audit(
            db,
            payload.get("sub") or "unknown",
            "games.create",
            detail=f"游戏={data['id']} 名称={data['name']}（模板联动自动创建并绑定）",
            ip=_client_ip(request),
            commit=False,
        )
    db.commit()
    msg = "模板已创建"
    if linkage["game_created"]:
        msg += f"，游戏 {data['id']} 已自动生成并绑定"
    elif linkage["game_bound"]:
        msg += f"，已绑定到现有游戏 {data['id']}"
    return {"ok": True, "message": msg, "data": {**data, "linkage": linkage}}


@router.patch("/menu-templates/{template_id}")
def update_menu_template(
    template_id: str,
    body: MenuTemplateUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    fields = body.model_dump(exclude_unset=True)
    try:
        data = menu_tpl.update_template(
            db,
            template_id,
            name=fields.get("name"),
            package=fields.get("package") if "package" in fields else None,
            source=fields.get("source") if "source" in fields else None,
            features=[f.model_dump() for f in body.features] if body.features is not None else None,
            clear_package="package" in fields and body.package is None,
            clear_source="source" in fields and body.source is None,
        )
    except LookupError as e:
        raise HTTPException(404, str(e)) from e
    except ValueError as e:
        raise HTTPException(400, str(e)) from e
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "menu_templates.update",
        detail=f"模板={template_id} 名称={data['name']} 功能={data['feature_count']}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "模板已更新", "data": data}


@router.delete("/menu-templates/{template_id}")
def delete_menu_template(
    template_id: str,
    request: Request,
    force: bool = False,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    try:
        data = menu_tpl.delete_template(db, template_id, force=force)
    except LookupError as e:
        raise HTTPException(404, str(e)) from e
    except ValueError as e:
        raise HTTPException(400, str(e)) from e
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "menu_templates.delete",
        detail=f"模板={template_id} force={force} unbound={data.get('unbound_games', 0)}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "模板已删除", "data": data}


@router.get("/games")
def list_games(
    q: str | None = None,
    enabled: bool | None = None,
    card_auth_enabled: bool | None = None,
    page: int = 1,
    page_size: int = 50,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    page, page_size = _clamp_page(page, page_size, max_size=500)
    query = db.query(Game)
    if q:
        like = f"%{q.strip()}%"
        query = query.filter(
            or_(Game.game_id.ilike(like), Game.name.ilike(like), Game.menu_template.ilike(like))
        )
    if enabled is not None:
        query = query.filter(Game.enabled.is_(enabled))
    if card_auth_enabled is not None:
        query = query.filter(Game.card_auth_enabled.is_(card_auth_enabled))

    total = query.count()
    rows = (
        query.order_by(Game.created_at.desc(), Game.id.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    counts = dict(
        db.query(FeatureFlag.game_id, func.count(FeatureFlag.id)).group_by(FeatureFlag.game_id).all()
    )
    return {
        "ok": True,
        "data": [_game_dict(g, counts.get(g.game_id, 0)) for g in rows],
        "meta": _page_meta(total, page, page_size),
    }


@router.post("/games")
def create_game(
    body: GameCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    if db.query(Game).filter(Game.game_id == body.game_id).first():
        raise HTTPException(400, "游戏 ID 已存在")
    tid = menu_tpl.resolve_template_id(db, body.game_id, body.menu_template)
    g = Game(
        game_id=body.game_id,
        name=body.name,
        enabled=body.enabled,
        card_auth_enabled=body.card_auth_enabled,
        menu_template=tid,
        download_url=body.download_url or None,
        channel=(body.channel or "").strip() or None,
    )
    db.add(g)
    db.flush()

    sync_result = None
    if body.sync_features and tid:
        sync_result = sync_game_features(db, g, template_id=tid, mode="fill")

    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "games.create",
        detail=f"游戏={g.game_id} 名称={g.name}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(g)

    return {
        "ok": True,
        "message": "游戏已创建",
        "data": {
            "id": g.id,
            "game_id": g.game_id,
            "name": g.name,
            "enabled": g.enabled,
            "card_auth_enabled": bool(g.card_auth_enabled),
            "menu_template": g.menu_template,
            "download_url": g.download_url,
            "channel": g.channel,
            "synced_features": sync_result,
        },
    }


@router.patch("/games/{game_pk}")
def update_game(
    game_pk: int,
    body: GameUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    g = db.query(Game).filter(Game.id == game_pk).first()
    if not g:
        raise HTTPException(404, "游戏不存在")
    if body.name is not None:
        g.name = body.name
    if body.enabled is not None:
        g.enabled = body.enabled
    if body.card_auth_enabled is not None:
        g.card_auth_enabled = body.card_auth_enabled
    if body.menu_template is not None:
        g.menu_template = body.menu_template or None
    if body.download_url is not None:
        g.download_url = body.download_url or None
    # 空串=清空渠道；null=不更新（与 download_url 同语义）
    if body.channel is not None:
        g.channel = (body.channel or "").strip() or None
    # 显式刷新编辑时间：值未变化时 SQLAlchemy 不发 UPDATE，onupdate 不会触发
    g.updated_at = datetime.now(timezone.utc)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "games.update",
        detail=(
            f"游戏={g.game_id} 名称={g.name} 启用={'是' if g.enabled else '否'}"
            f" 卡密={'是' if g.card_auth_enabled else '否'} 模板={g.menu_template or '—'}"
        ),
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {
        "ok": True,
        "message": "游戏已更新",
        "data": _game_dict(g),
    }


@router.delete("/games/{game_pk}")
def delete_game(
    game_pk: int,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    g = db.query(Game).filter(Game.id == game_pk).first()
    if not g:
        raise HTTPException(404, "游戏不存在")
    gid = g.game_id
    force = request.query_params.get("force", "").lower() in ("1", "true", "yes")

    # 引用检查：有卡密绑定该游戏时默认拒绝（防误删在售游戏），force 才连同删除
    cards_n = db.query(func.count(CardKey.id)).filter(CardKey.game_id == gid).scalar() or 0
    if cards_n > 0 and not force:
        raise HTTPException(
            409,
            f"该游戏下还有 {cards_n} 张卡密。请先删除/迁移卡密，或使用强制删除（连同卡密一起删除）",
        )

    # 关联数据：功能门控、在线统计、授权事件、会话（全局卡密的会话保留，只删该游戏的）
    feats = db.query(FeatureFlag).filter(FeatureFlag.game_id == gid).delete(synchronize_session=False)
    pres = db.query(OnlinePresence).filter(OnlinePresence.game_id == gid).delete(synchronize_session=False)
    evts = db.query(AuthEvent).filter(AuthEvent.game_id == gid).delete(synchronize_session=False)
    sess = db.query(SessionToken).filter(SessionToken.game_id == gid).delete(synchronize_session=False)
    cards = 0
    devs = 0
    if force:
        # 一码多设备：先清这些卡的设备行（card_devices 无外键级联）
        card_pks = [r[0] for r in db.query(CardKey.id).filter(CardKey.game_id == gid).all()]
        if card_pks:
            devs = (
                db.query(CardDevice)
                .filter(CardDevice.card_id.in_(card_pks))
                .delete(synchronize_session=False)
            )
        cards = db.query(CardKey).filter(CardKey.game_id == gid).delete(synchronize_session=False)

    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "games.delete",
        detail=(
            f"游戏={gid} 功能={feats} 在线记录={pres} 事件={evts} 会话={sess}"
            + (f" 卡密={cards} 设备绑定={devs}" if force else "")
            + ("（强制）" if force else "")
        ),
        ip=_client_ip(request),
        commit=False,
    )
    db.delete(g)
    db.commit()
    return {
        "ok": True,
        "message": f"游戏 {gid} 已删除（功能 {feats}、在线记录 {pres}、事件 {evts}、会话 {sess}"
        + (f"、卡密 {cards}" if force else "")
        + "）",
    }


@router.post("/games/{game_id}/sync-features")
def sync_features_for_game(
    game_id: str,
    body: GameSyncFeaturesRequest,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    g = db.query(Game).filter(Game.game_id == game_id).first()
    if not g:
        raise HTTPException(404, "游戏不存在")
    tid = menu_tpl.resolve_template_id(db, game_id, body.menu_template or g.menu_template)
    if not tid:
        raise HTTPException(400, "未绑定菜单模板，请先在「菜单模板」中创建并选择")
    result = sync_game_features(db, g, template_id=tid, mode=body.mode)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "games.sync_features",
        detail=f"游戏={game_id} 模板={tid} 模式={body.mode}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": f"已同步模板 {tid}", "data": result}


# ----- cards -----
@router.get("/cards")
def list_cards(
    q: str | None = None,
    status: str | None = None,
    game_id: str | None = None,
    scope: str | None = None,
    duration_type: str | None = None,
    bound: bool | None = None,
    page: int = 1,
    page_size: int = 50,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    page = max(page, 1)
    page_size = min(max(page_size, 1), 200)

    query = db.query(CardKey)
    if q:
        query = query.filter(CardKey.key_code.contains(q))
    if status:
        query = query.filter(CardKey.status == status)
    # scope=global|game；与 game_id 组合时不再把全游戏卡混进「某游戏」结果
    if scope in ("global", "game"):
        query = query.filter(CardKey.scope == scope)
    if game_id:
        if scope == "global":
            pass  # 已按全游戏筛，忽略 game_id
        elif scope == "game":
            query = query.filter(CardKey.game_id == game_id)
        else:
            # 未指定 scope：该游戏单卡 + 全部全游戏卡（兼容旧行为）
            query = query.filter((CardKey.game_id == game_id) | (CardKey.scope == "global"))
    # 类型筛选（hour/day/month/year/permanent/count）：后台列表原缺此项
    if duration_type in ("hour", "day", "month", "year", "permanent", "count"):
        query = query.filter(CardKey.duration_type == duration_type)
    # 绑定状态按设备表判定（一码多设备后 card_keys.device_id 只是兼容字段）
    has_device = exists().where(CardDevice.card_id == CardKey.id)
    if bound is True:
        query = query.filter(has_device)
    elif bound is False:
        query = query.filter(~has_device)

    total = query.count()
    rows = (
        query.order_by(CardKey.id.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    # 一码多设备：本页所有卡的绑定设备一次性取回，避免 N+1
    dev_map: dict[int, list[dict]] = {}
    if rows:
        card_ids = [c.id for c in rows]
        for d in (
            db.query(CardDevice)
            .filter(CardDevice.card_id.in_(card_ids))
            .order_by(CardDevice.bound_at.asc(), CardDevice.id.asc())
            .all()
        ):
            dev_map.setdefault(d.card_id, []).append(_device_dict(d))
    return {
        "ok": True,
        "data": [_card_dict(c, dev_map.get(c.id, [])) for c in rows],
        "meta": {"total": total, "page": page, "page_size": page_size},
    }


@router.post("/cards")
def create_cards(
    body: CardCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    if body.scope == "game" and not body.game_id:
        raise HTTPException(400, "单游戏卡必须指定 game_id")
    if body.duration_type == "count" and body.max_uses <= 0:
        raise HTTPException(400, "次数卡必须设置 max_uses > 0")
    common_fields = dict(
        scope=body.scope,
        game_id=body.game_id if body.scope == "game" else None,
        duration_type=body.duration_type,
        max_uses=body.max_uses,
        max_devices=body.max_devices,  # 0=不限台数；1=一机一码
        note=body.note,
        status="unused",
    )
    if body.custom_keys:
        # 自定义模式：归一化 + 整批查重后原样创建
        codes = normalize_custom_keys(body.custom_keys)
        cards = create_custom_card_keys(db, codes, **common_fields)
    else:
        cards = generate_card_keys(
            db,
            count=body.count,
            prefix=body.prefix,
            **common_fields,
        )
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "cards.create",
        detail=f"数量={len(cards)} 范围={body.scope} 游戏={body.game_id or '—'}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": f"已生成 {len(cards)} 张卡密", "data": [_card_dict(c) for c in cards]}


@router.post("/cards/batch")
def batch_cards(
    body: CardBatchRequest,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    if body.status is None and body.extend_days is None:
        raise HTTPException(400, "请至少指定 status 或 extend_days")
    now = utcnow()
    # 先算后写（2026-11-12）：延期值越大越容易在 base+delta 处溢出 datetime，
    # 一旦任何一张算不出结果就整批 422 —— 此时尚未改任何字段、未写审计、未 commit，
    # 不会出现「前半批已延期、后半批失败」的半批状态。
    delta = None
    if body.extend_days is not None:
        try:
            delta = timedelta(days=body.extend_days)
        except (OverflowError, ValueError, TypeError):
            raise HTTPException(422, "延期天数超出可计算范围")
    found = (
        db.query(CardKey).filter(CardKey.id.in_(body.ids)).all() if body.ids else []
    )
    by_id = {c.id: c for c in found}
    ordered = [by_id[i] for i in body.ids if i in by_id]
    planned: dict[int, datetime] = {}
    if delta is not None:
        for card in ordered:
            base = _as_utc(card.expire_at)
            if base is None or base <= now:
                base = now
            try:
                new_exp = base + delta
            except (OverflowError, ValueError):
                raise HTTPException(
                    422, f"卡密 ID={card.id} 延期后超出可表示时间范围，整批未修改"
                )
            if new_exp < _DT_MIN or new_exp > _DT_MAX:
                raise HTTPException(
                    422, f"卡密 ID={card.id} 延期后超出可表示时间范围，整批未修改"
                )
            planned[card.id] = new_exp
    updated = 0
    expire_changed = 0
    expire_samples: list[str] = []
    for card in ordered:
        if body.status is not None:
            card.status = body.status
        if delta is not None:
            old_expire = _as_utc(card.expire_at)
            card.expire_at = planned[card.id]
            if planned[card.id] != old_expire:
                expire_changed += 1
                if len(expire_samples) < 3:
                    expire_samples.append(
                        f"ID={card.id}:{_fmt_expire(old_expire)}->{_fmt_expire(planned[card.id])}"
                    )
        refresh_card_status(card)
        if card.status in ("banned", "expired", "exhausted"):
            revoke_card_sessions(db, card.id)
        updated += 1
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "cards.batch",
        detail=(
            f"选中数量={len(body.ids)} 已更新={updated}"
            + (f" 状态={body.status}" if body.status is not None else "")
            + (f" 延期天数={body.extend_days}" if body.extend_days is not None else "")
            + (f" 到期变更={expire_changed}张" if body.extend_days is not None else "")
            + (f" 样本={'；'.join(expire_samples)}" if expire_samples else "")
            + " 来源=admin.cards.batch"
        ),
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {
        "ok": True,
        "message": f"已批量更新 {updated} 张卡密",
        "data": {"updated": updated},
    }


@router.post("/cards/batch-delete")
def batch_delete_cards(
    body: CardBatchRequest,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    # 与单删 delete_card 同语义：先吊销会话再删（在线设备下次心跳即失效）
    deleted = 0
    for card_id in body.ids:
        card = db.query(CardKey).filter(CardKey.id == card_id).first()
        if not card:
            continue
        revoke_card_sessions(db, card.id)
        purge_card_devices(db, card.id)
        db.delete(card)
        deleted += 1
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "cards.batch_delete",
        detail=f"选中数量={len(body.ids)} 已删除={deleted}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {
        "ok": True,
        "message": f"已批量删除 {deleted} 张卡密",
        "data": {"deleted": deleted},
    }


@router.patch("/cards/{card_id}")
def update_card(
    card_id: int,
    body: CardUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    card = db.query(CardKey).filter(CardKey.id == card_id).first()
    if not card:
        raise HTTPException(404, "卡密不存在")
    data = body.model_dump(exclude_unset=True)
    force_unbind = data.pop("force_unbind", False)
    # 归一化/校验放在 setattr 之前：失败时不留脏的中间状态
    if data.get("scope") == "global":
        data["game_id"] = None
    if data.get("scope") == "game" and not data.get("game_id") and not card.game_id:
        raise HTTPException(400, "单游戏卡必须指定 game_id")
    # 审计口径（2026-11-12）：改动前后到期时间都留痕，事后可还原"谁把它改早了/改晚了"
    expire_sent = "expire_at" in data
    old_expire = card.expire_at
    for k, v in data.items():
        setattr(card, k, v)
    if force_unbind:
        # 后台强制解绑=整卡操作：清掉全部设备行与会话（与一码多设备前行为一致）
        purge_card_devices(db, card.id)
        card.device_id = None
        card.legacy_device_ids = None
    refresh_card_status(card)
    # 封禁/过期/用尽/强制解绑：立即吊销在线会话，避免离线缓存窗口内继续使用
    if force_unbind or card.status in ("banned", "expired", "exhausted"):
        revoke_card_sessions(db, card.id)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "cards.update",
        detail=(
            f"ID={card_id} 状态={card.status}"
            + (f" 强制解绑={'是' if force_unbind else '否'}")
            + (f" 到期={_fmt_expire(old_expire)}->{_fmt_expire(card.expire_at)}" if expire_sent else "")
            + " 来源=admin.cards.update"
        ),
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(card)
    devs = [
        _device_dict(d)
        for d in db.query(CardDevice)
        .filter(CardDevice.card_id == card.id)
        .order_by(CardDevice.bound_at.asc(), CardDevice.id.asc())
        .all()
    ]
    return {"ok": True, "message": "卡密已更新", "data": _card_dict(card, devs)}


@router.delete("/cards/{card_id}")
def delete_card(
    card_id: int,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    card = db.query(CardKey).filter(CardKey.id == card_id).first()
    if not card:
        raise HTTPException(404, "卡密不存在")
    revoke_card_sessions(db, card.id)
    purge_card_devices(db, card.id)
    # 删卡后在线行不再挂这张卡（否则看板仍把它算作「卡密关联设备」）
    clear_card_presence(db, card.id)
    db.delete(card)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "cards.delete",
        # 审计只记内部 ID：卡密明文不得进日志
        detail=f"ID={card_id}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "卡密已删除"}


# ----- announcements -----
@router.get("/announcements")
def list_announcements(
    q: str | None = None,
    game_id: str | None = None,
    scope: str | None = None,
    enabled: bool | None = None,
    visibility: str | None = None,
    page: int = 1,
    page_size: int = 50,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    page, page_size = _clamp_page(page, page_size)
    query = db.query(Announcement)
    if q:
        like = f"%{q.strip()}%"
        query = query.filter(
            or_(
                Announcement.title.ilike(like),
                Announcement.content.ilike(like),
                Announcement.action_url.ilike(like),
            )
        )
    if scope:
        query = query.filter(Announcement.scope == scope)
    if enabled is not None:
        query = query.filter(Announcement.enabled.is_(enabled))
    if visibility:
        query = query.filter(Announcement.visibility == visibility)
    if game_id:
        query = query.filter(
            or_(Announcement.scope == "global", Announcement.game_id == game_id)
        )

    total = query.count()
    rows = (
        query.order_by(Announcement.sort_order.asc(), Announcement.id.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    return {
        "ok": True,
        "data": [_announcement_dict(a) for a in rows],
        "meta": _page_meta(total, page, page_size),
    }


@router.post("/announcements")
def create_announcement(
    body: AnnouncementCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    if body.scope == "game" and not body.game_id:
        raise HTTPException(400, "单游戏公告必须指定 game_id")
    data = body.model_dump()
    # 全游戏公告不绑定具体游戏（防前端漏禁用选择器时带入脏数据）
    if data.get("scope") == "global":
        data["game_id"] = None
    for key in ("action_url", "action_label"):
        val = (data.get(key) or "").strip() if isinstance(data.get(key), str) else data.get(key)
        data[key] = val or None
    a = Announcement(**data)
    db.add(a)
    db.flush()
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "announcements.create",
        detail=f"ID={a.id} 标题={a.title}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(a)
    return {"ok": True, "message": "公告已创建", "data": _announcement_dict(a)}


@router.patch("/announcements/{ann_id}")
def update_announcement(
    ann_id: int,
    body: AnnouncementUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    a = db.query(Announcement).filter(Announcement.id == ann_id).first()
    if not a:
        raise HTTPException(404, "公告不存在")
    data = body.model_dump(exclude_unset=True)
    # 归一化/校验放在 setattr 之前：失败时不留脏的中间状态
    if data.get("scope") == "global":
        data["game_id"] = None
    if data.get("scope") == "game" and not data.get("game_id") and not a.game_id:
        raise HTTPException(400, "单游戏公告必须指定 game_id")
    for k, v in data.items():
        if k in ("action_url", "action_label") and isinstance(v, str):
            v = v.strip() or None
        setattr(a, k, v)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "announcements.update",
        detail=f"ID={ann_id}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(a)
    return {"ok": True, "message": "公告已更新", "data": _announcement_dict(a)}


@router.delete("/announcements/{ann_id}")
def delete_announcement(
    ann_id: int,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    a = db.query(Announcement).filter(Announcement.id == ann_id).first()
    if not a:
        raise HTTPException(404, "公告不存在")
    title = a.title
    db.delete(a)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "announcements.delete",
        detail=f"ID={ann_id} 标题={title}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "公告已删除"}


# ----- device events (2026-11-15) -----
def _card_key_map(db: Session, card_ids) -> dict[int, str]:
    """按 card_id 批量取现存卡密明文（仅管理端；已删卡自然取不到）。"""
    ids = {int(c) for c in card_ids if c}
    if not ids:
        return {}
    return {
        c.id: c.key_code
        for c in db.query(CardKey).filter(CardKey.id.in_(ids)).all()
    }


def _event_dict(r: AuthEvent, card_keys: dict[int, str] | None = None) -> dict:
    """事件对外表示：设备号打码、detail 再脱敏。

    卡密明文单独从 card_keys 现查（require_admin 才可达），不写回事件表：
    - card_key：现存卡密明文，便于管理员搜索/核对；
    - card_deleted：card_id 有值但卡已删除（明文已不可回填，只能给内部 ID）。
    """
    key = (card_keys or {}).get(r.card_id) if r.card_id else None
    return {
        "id": r.id,
        "created_at": r.created_at.isoformat() if r.created_at else None,
        "last_occurred_at": r.last_occurred_at.isoformat() if getattr(r, "last_occurred_at", None) else None,
        "event_type": r.event_type,
        "game_id": r.game_id,
        "device_id": mask_device_id(r.device_id),
        # 完整值仅供管理端「详情」按钮回查（接口本身 require_admin）；列表展示仍用打码值
        "device_id_full": r.device_id if (r.device_id or "").startswith("dev_") else None,
        "device_id_masked": True,
        "card_id": r.card_id,
        "card_key": key,
        "card_deleted": bool(r.card_id and key is None),
        "result": r.result,
        "reason": r.reason,
        "detail": mask_card_key(r.detail),
        "ip": r.ip,
        "ip_forwarded": r.ip_forwarded,
        "ua": r.ua,
        "request_id": r.request_id,
        "suppressed": int(getattr(r, "suppressed", 0) or 0),
    }


@router.get("/events")
def list_events(
    device_id: str | None = None,
    card_id: int | None = None,
    game_id: str | None = None,
    event_type: str | None = None,
    result: str | None = None,
    from_at: str | None = None,
    to_at: str | None = None,
    page: int = 1,
    page_size: int = 50,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """按设备/游戏/类型/结果/时间查客户端事件（仅管理员）。

    - device_id 需传**完整原值**过滤（返回体里打码，避免列表页泄露完整设备号）；
    - detail 不返回卡密/密码/token，历史明文按掩码回显。
    """
    page, page_size = _clamp_page(page, page_size)
    q = db.query(AuthEvent)
    if device_id and device_id.strip():
        q = q.filter(AuthEvent.device_id == device_id.strip())
    # 只支持内部 card_id 过滤：卡密明文不进 URL（GET query 会落 nginx 访问日志）
    if card_id:
        q = q.filter(AuthEvent.card_id == int(card_id))
    if game_id and game_id.strip():
        q = q.filter(AuthEvent.game_id == game_id.strip())
    if event_type and event_type.strip():
        q = q.filter(AuthEvent.event_type == event_type.strip())
    if result and result.strip():
        q = q.filter(AuthEvent.result == result.strip())
    start = _parse_iso_dt(from_at)
    end = _parse_iso_dt(to_at, end_of_day=True)
    if start is not None:
        q = q.filter(AuthEvent.created_at >= start)
    if end is not None:
        q = q.filter(AuthEvent.created_at <= end)
    total = q.count()
    rows = (
        q.order_by(AuthEvent.created_at.desc(), AuthEvent.id.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    keys = _card_key_map(db, [r.card_id for r in rows])
    return {
        "ok": True,
        "data": [_event_dict(r, keys) for r in rows],
        "meta": _page_meta(total, page, page_size),
    }


@router.get("/devices/{device_id}")
def device_detail(
    device_id: str,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """单设备视图：当前在线行 + 卡绑定 + 最近成功/失败，便于按设备追溯。

    只返回 card_id 与状态，不回传卡密明文。
    """
    did = (device_id or "").strip()
    if not did:
        raise HTTPException(400, "缺少 device_id")
    # 在线判定与看板同口径：需卡用认证活动时间，免卡用观测 last_seen（90s 窗口）
    game_modes = dict(db.query(Game.game_id, Game.card_auth_enabled).all())
    now_utc = datetime.now(timezone.utc)

    def _online(p) -> bool | None:
        card_auth = game_modes.get(p.game_id)
        if card_auth is None:
            return None
        ts = (
            getattr(p, "last_authenticated_at", None)
            if card_auth
            else p.last_seen_at
        )
        if ts is None:
            return False
        if ts.tzinfo is None:
            ts = ts.replace(tzinfo=timezone.utc)
        return (now_utc - ts).total_seconds() <= 90

    presences = [
        {
            "game_id": p.game_id,
            "card_id": p.card_id,
            "last_seen_at": p.last_seen_at.isoformat() if p.last_seen_at else None,
            "last_authenticated_at": getattr(p, "last_authenticated_at", None).isoformat()
            if getattr(p, "last_authenticated_at", None)
            else None,
            "card_auth_enabled": game_modes.get(p.game_id),
            "is_online": _online(p),
            "created_at": p.created_at.isoformat() if getattr(p, "created_at", None) else None,
            "card_verified_at": p.card_verified_at.isoformat()
            if getattr(p, "card_verified_at", None)
            else None,
        }
        for p in db.query(OnlinePresence)
        .filter(OnlinePresence.device_id == did)
        .order_by(OnlinePresence.last_seen_at.desc())
        .all()
    ]
    bindings = []
    for b in (
        db.query(CardDevice, CardKey)
        .join(CardKey, CardKey.id == CardDevice.card_id)
        .filter(CardDevice.device_id == did)
        .all()
    ):
        card = b[1]
        bindings.append(
            {
                "card_id": card.id,
                "key_code": card.key_code,
                "status": card.status,
                "scope": card.scope,
                "game_id": card.game_id,
                "expire_at": card.expire_at.isoformat() if card.expire_at else None,
                "bound_at": b[0].bound_at.isoformat() if b[0].bound_at else None,
            }
        )
    events = (
        db.query(AuthEvent)
        .filter(AuthEvent.device_id == did)
        .order_by(AuthEvent.created_at.desc(), AuthEvent.id.desc())
        .limit(30)
        .all()
    )
    last_ok = next(
        (e for e in events if e.event_type == "login_ok" and (e.result or "ok") == "ok"), None
    )
    last_fail = next((e for e in events if (e.result or "") == "deny"), None)
    return {
        "ok": True,
        "data": {
            "device_id": mask_device_id(did),
            "presences": presences,
            "bindings": bindings,
            "events": [_event_dict(e, _card_key_map(db, [e.card_id for e in events])) for e in events],
            "last_success_at": last_ok.created_at.isoformat() if last_ok and last_ok.created_at else None,
            "last_failure_at": last_fail.created_at.isoformat() if last_fail and last_fail.created_at else None,
        },
    }


# ----- audit logs -----
@router.get("/audit-logs")
def list_audit_logs(
    q: str | None = None,
    action: str | None = None,
    admin_username: str | None = None,
    from_at: str | None = None,
    to_at: str | None = None,
    page: int = 1,
    page_size: int = 50,
    limit: int | None = None,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    # 兼容旧参数 limit：等价于第一页
    if limit is not None and (page == 1 and page_size == 50):
        page, page_size = 1, min(max(limit, 1), 200)
    else:
        page, page_size = _clamp_page(page, page_size)

    query = db.query(AdminAuditLog)
    if action:
        query = query.filter(AdminAuditLog.action.startswith(action))
    if admin_username:
        query = query.filter(AdminAuditLog.admin_username.ilike(f"%{admin_username.strip()}%"))
    start = _parse_iso_dt(from_at)
    end = _parse_iso_dt(to_at, end_of_day=True)
    if start is not None:
        query = query.filter(AdminAuditLog.created_at >= start)
    if end is not None:
        query = query.filter(AdminAuditLog.created_at <= end)
    if q:
        like = f"%{q.strip()}%"
        query = query.filter(
            or_(
                AdminAuditLog.admin_username.ilike(like),
                AdminAuditLog.action.ilike(like),
                AdminAuditLog.detail.ilike(like),
                AdminAuditLog.ip.ilike(like),
            )
        )

    total = query.count()
    rows = (
        query.order_by(AdminAuditLog.id.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    return {
        "ok": True,
        "data": [_audit_dict(r) for r in rows],
        "meta": _page_meta(total, page, page_size),
    }


# ----- features -----
def _feature_dict(f: FeatureFlag) -> dict:
    return {
        "id": f.id,
        "game_id": f.game_id,
        "feat_num": f.feat_num,
        "name": f.name,
        "enabled": f.enabled,
    }


@router.get("/features")
def list_features(
    game_id: str = Query(..., description="必填：按游戏拉取功能，避免全量"),
    q: str | None = None,
    enabled: bool | None = None,
    page: int = 1,
    page_size: int = 100,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    page, page_size = _clamp_page(page, page_size)
    query = db.query(FeatureFlag).filter(FeatureFlag.game_id == game_id)
    if q:
        qstrip = q.strip()
        like = f"%{qstrip}%"
        if qstrip.isdigit():
            query = query.filter(
                or_(FeatureFlag.name.ilike(like), FeatureFlag.feat_num == int(qstrip))
            )
        else:
            query = query.filter(FeatureFlag.name.ilike(like))
    if enabled is not None:
        query = query.filter(FeatureFlag.enabled.is_(enabled))

    total = query.count()
    rows = (
        query.order_by(FeatureFlag.feat_num.asc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    return {
        "ok": True,
        "data": [_feature_dict(f) for f in rows],
        "meta": _page_meta(total, page, page_size),
    }


@router.post("/features")
def create_feature(
    body: FeatureCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    exists = (
        db.query(FeatureFlag)
        .filter(FeatureFlag.game_id == body.game_id, FeatureFlag.feat_num == body.feat_num)
        .first()
    )
    if exists:
        raise HTTPException(400, "该游戏功能编号已存在")
    f = FeatureFlag(**body.model_dump())
    db.add(f)
    db.flush()
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "features.create",
        detail=f"游戏={f.game_id} 编号={f.feat_num} 名称={f.name}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(f)
    return {"ok": True, "message": "功能已创建", "data": _feature_dict(f)}


@router.patch("/features/{feat_id}")
def update_feature(
    feat_id: int,
    body: FeatureUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    f = db.query(FeatureFlag).filter(FeatureFlag.id == feat_id).first()
    if not f:
        raise HTTPException(404, "功能不存在")
    for k, v in body.model_dump(exclude_unset=True).items():
        setattr(f, k, v)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "features.update",
        detail=f"游戏={f.game_id} 编号={f.feat_num} 可用={'是' if f.enabled else '否'} 名称={f.name}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "功能已更新", "data": _feature_dict(f)}


@router.delete("/features/{feat_id}")
def delete_feature(
    feat_id: int,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    f = db.query(FeatureFlag).filter(FeatureFlag.id == feat_id).first()
    if not f:
        raise HTTPException(404, "功能不存在")
    detail = f"游戏={f.game_id} 编号={f.feat_num} 名称={f.name}"
    db.delete(f)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "features.delete",
        detail=detail,
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "功能已删除"}


# ----- toasts（开屏启动提示） -----
def _toast_dict(t: ToastMessage) -> dict:
    return {
        "id": t.id,
        "scope": t.scope,
        "game_id": t.game_id,
        "toast_key": t.toast_key,
        "content": t.content,
        "toast_type": t.toast_type,
        "enabled": t.enabled,
        "sort_order": getattr(t, "sort_order", 0) or 0,
    }


@router.get("/toasts")
def list_toasts(
    q: str | None = None,
    game_id: str | None = None,
    scope: str | None = None,
    enabled: bool | None = None,
    page: int = 1,
    page_size: int = 50,
    _: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    page, page_size = _clamp_page(page, page_size)
    query = db.query(ToastMessage)
    if q:
        like = f"%{q.strip()}%"
        query = query.filter(
            or_(ToastMessage.content.ilike(like), ToastMessage.toast_key.ilike(like))
        )
    if scope:
        query = query.filter(ToastMessage.scope == scope)
    if enabled is not None:
        query = query.filter(ToastMessage.enabled.is_(enabled))
    if game_id:
        query = query.filter(or_(ToastMessage.scope == "global", ToastMessage.game_id == game_id))

    total = query.count()
    rows = (
        query.order_by(ToastMessage.sort_order.asc(), ToastMessage.id.asc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    return {
        "ok": True,
        "data": [_toast_dict(t) for t in rows],
        "meta": _page_meta(total, page, page_size),
    }


@router.post("/toasts")
def create_toast(
    body: ToastCreate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    import time

    data = body.model_dump()
    if body.scope == "game" and not body.game_id:
        raise HTTPException(400, "单游戏 Toast 必须指定 game_id")
    key = (data.get("toast_key") or "").strip()
    if not key:
        key = f"startup_{int(time.time() * 1000)}"
    data["toast_key"] = key
    t = ToastMessage(**data)
    db.add(t)
    db.flush()
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "toasts.create",
        detail=f"ID={t.id} 范围={t.scope} 游戏={t.game_id or '—'} 内容={t.content[:40]}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(t)
    return {"ok": True, "message": "开屏 Toast 已创建", "data": _toast_dict(t)}


@router.patch("/toasts/{toast_id}")
def update_toast(
    toast_id: int,
    body: ToastUpdate,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    t = db.query(ToastMessage).filter(ToastMessage.id == toast_id).first()
    if not t:
        raise HTTPException(404, "开屏 Toast 不存在")
    for k, v in body.model_dump(exclude_unset=True).items():
        setattr(t, k, v)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "toasts.update",
        detail=f"ID={toast_id} 启用={'是' if t.enabled else '否'} 内容={(t.content or '')[:40]}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    db.refresh(t)
    return {"ok": True, "message": "开屏 Toast 已更新", "data": _toast_dict(t)}


@router.delete("/toasts/{toast_id}")
def delete_toast(
    toast_id: int,
    request: Request,
    payload: dict = Depends(require_admin),
    db: Session = Depends(get_db),
):
    t = db.query(ToastMessage).filter(ToastMessage.id == toast_id).first()
    if not t:
        raise HTTPException(404, "开屏 Toast 不存在")
    content = (t.content or "")[:40]
    db.delete(t)
    record_admin_audit(
        db,
        payload.get("sub") or "unknown",
        "toasts.delete",
        detail=f"ID={toast_id} 内容={content}",
        ip=_client_ip(request),
        commit=False,
    )
    db.commit()
    return {"ok": True, "message": "开屏 Toast 已删除"}


# ---------------- P2-2：载荷 sha（按游戏维护，改完无需重建镜像） ----------------

@router.get("/games/{game_id}/payload")
def get_game_payload(game_id: str, db: Session = Depends(get_db), _: dict = Depends(require_admin)):
    from ..services import grant_service as gs
    row = db.query(Game).filter(Game.game_id == game_id).first()
    if not row:
        raise HTTPException(404, "游戏不存在")
    return {
        "game_id": row.game_id,
        "payload_sha": row.payload_sha or "",
        "env_fallback": (gs.get_settings().payload_sha or ""),
        "v3_ready": gs.payload_ready(row.game_id),
        "k_build_fp": (gs.payload_build_key(row.game_id) or b"").hex()[:16],
        "warning": "改 payload_sha 会改变该游戏载荷密钥 K_build ⇒ 所有旧包功能失效，必须同步出新包",
    }


@router.put("/games/{game_id}/payload")
async def put_game_payload(
    game_id: str,
    request: Request,
    sha: str = Query("", description="直接给出 64 位 sha256；留空则从上传文件计算"),
    db: Session = Depends(get_db),
    admin: dict = Depends(require_admin),
):
    import hashlib
    row = db.query(Game).filter(Game.game_id == game_id).first()
    if not row:
        raise HTTPException(404, "游戏不存在")

    value = (sha or "").strip().lower()
    if not value:
        form = await request.form()
        up = form.get("file")
        if up is None or not hasattr(up, "read"):
            raise HTTPException(400, "需要 sha 参数或上传 file 字段")
        data = await up.read()
        if not data:
            raise HTTPException(400, "上传文件为空")
        value = hashlib.sha256(data).hexdigest()

    if value == "clear":
        row.payload_sha = None
    else:
        if len(value) != 64 or any(c not in "0123456789abcdef" for c in value):
            raise HTTPException(400, "payload_sha 必须是 64 位十六进制")
        row.payload_sha = value
    db.commit()

    from ..services import grant_service as gs
    gs._SHA_CACHE.pop(game_id, None)          # 立刻失效缓存
    return {
        "ok": True,
        "game_id": game_id,
        "payload_sha": row.payload_sha or "",
        "v3_ready": gs.payload_ready(game_id),
        "reminder": "请同步出新包，旧包的 K_build 已失效",
    }
