"""前台「请求更新」公开接口（2026-11-22）。

两条路由，都不走管理端鉴权：
    GET  /v1/public/request-games   前台下拉数据（只读，无需令牌）
    POST /v1/public/request-update  提交请求（必须带共享令牌，由 mu-plugin 服务端转发）

安全边界：
    · 提交接口的 wp_user_id / wp_username 由 WordPress mu-plugin 注入，
      浏览器拿不到 X-Request-Token，因此无法伪造身份。
    · 令牌未配置时接口直接 503（fail closed），不会退化成裸奔接口。
    · IP 一律走 security_guard.client_ip（只信受信反代写的 X-Real-IP）。
"""

from __future__ import annotations

import hashlib
import hmac
import re
import unicodedata
from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy import func
from sqlalchemy.orm import Session

from ..config import get_settings
from ..database import get_db
from ..models import Game, GameChannel, MenuTemplateFeature, UpdateRequest, WishRequest
from ..schemas import UpdateRequestCreate, WishRequestCreate
from ..security_guard import client_ip

router = APIRouter(prefix="/v1/public", tags=["public"])

# 北京时间（UTC+8）：限流按自然日计算，与用户体感一致
CN_TZ = timezone(timedelta(hours=8))
# 一个账号一天最多对几个不同游戏提交
MAX_GAMES_PER_DAY = 3
# 问题描述字数上限（用户规定 100 字）
PROBLEM_MAX_LEN = 100
# 同一账号同一天同一游戏只能提交 1 次
MAX_PER_GAME_PER_DAY = 1


def _day_range_utc() -> tuple[datetime, datetime]:
    """返回「北京时间的今天」对应的 UTC naive 区间 [start, end)。

    MySQL 里 created_at 是 server_default=func.now()，存的是 UTC naive；
    这里换算成 UTC naive 才能正确比较，避免把早上 8 点当成分界。
    """
    now_cn = datetime.now(CN_TZ)
    start_cn = now_cn.replace(hour=0, minute=0, second=0, microsecond=0)
    start_utc = start_cn.astimezone(timezone.utc).replace(tzinfo=None)
    end_utc = (start_cn + timedelta(days=1)).astimezone(timezone.utc).replace(tzinfo=None)
    return start_utc, end_utc


def _require_submit_token(request: Request) -> None:
    """校验共享令牌；未配置则拒绝服务（不放行）。"""
    expect = (get_settings().request_submit_token or "").strip()
    if not expect:
        raise HTTPException(status_code=503, detail="提交接口未配置令牌，已停用")
    got = (request.headers.get("x-request-token") or "").strip()
    if not got or not hmac.compare_digest(got, expect):
        raise HTTPException(status_code=403, detail="无权提交")


def _channels_of(db: Session, game_ids: list[str]) -> dict[str, list[str]]:
    if not game_ids:
        return {}
    rows = (
        db.query(GameChannel.game_id, GameChannel.channel)
        .filter(GameChannel.game_id.in_(game_ids))
        .order_by(GameChannel.sort_order.asc(), GameChannel.id.asc())
        .all()
    )
    out: dict[str, list[str]] = {}
    for gid, ch in rows:
        out.setdefault(gid, []).append(ch)
    return out


@router.get("/request-games")
def list_request_games(db: Session = Depends(get_db)):
    """前台下拉数据：只含【启用 + 未停用更新 + 至少 1 个渠道】的游戏。

    返回结构故意最小化：只给前台需要的三个字段，不暴露 game_id 以外的后台信息。
    """
    games = (
        db.query(Game)
        .filter(Game.enabled.is_(True), Game.request_updates_enabled.is_(True))
        .order_by(Game.id.asc())
        .all()
    )
    chan_map = _channels_of(db, [g.game_id for g in games])
    data = []
    for g in games:
        channels = chan_map.get(g.game_id) or []
        if not channels:
            # 信息不全（没配渠道）的游戏不在前台展示
            continue
        data.append({"game_id": g.game_id, "name": g.name, "channels": channels})
    return {"ok": True, "data": data}


@router.post("/request-update")
def submit_request(
    body: UpdateRequestCreate,
    request: Request,
    db: Session = Depends(get_db),
):
    _require_submit_token(request)

    game = db.query(Game).filter(Game.game_id == body.game_id).first()
    if not game or not game.enabled or not game.request_updates_enabled:
        raise HTTPException(status_code=400, detail="该游戏暂不接收更新请求")

    # 渠道必须属于该游戏（防绕过前端直接构造）
    allowed = (_channels_of(db, [game.game_id]).get(game.game_id) or [])
    channel = (body.channel or "").strip()
    if channel not in allowed:
        raise HTTPException(status_code=400, detail="渠道无效，请刷新页面重试")

    problem = (body.problem or "").strip()
    if not problem:
        raise HTTPException(status_code=400, detail="请填写游戏问题")
    if len(problem) > PROBLEM_MAX_LEN:
        raise HTTPException(
            status_code=400, detail=f"游戏问题最多 {PROBLEM_MAX_LEN} 字（当前 {len(problem)} 字）"
        )

    start_utc, end_utc = _day_range_utc()
    today = db.query(UpdateRequest).filter(
        UpdateRequest.wp_user_id == body.wp_user_id,
        UpdateRequest.created_at >= start_utc,
        UpdateRequest.created_at < end_utc,
    )
    # 限制一：同一账号 + 同一游戏 → 当天 1 次
    same_game = today.filter(UpdateRequest.game_id == game.game_id).count()
    if same_game >= MAX_PER_GAME_PER_DAY:
        raise HTTPException(status_code=429, detail="今天已提交过该游戏的请求，明天再来吧")
    # 限制二：同一账号 → 当天最多 3 个不同游戏
    distinct_games = today.with_entities(
        func.count(func.distinct(UpdateRequest.game_id))
    ).scalar() or 0
    if distinct_games >= MAX_GAMES_PER_DAY:
        raise HTTPException(
            status_code=429,
            detail=f"今天最多提交 {MAX_GAMES_PER_DAY} 个不同游戏的请求，明天再来吧",
        )

    row = UpdateRequest(
        game_id=game.game_id,
        game_name=game.name,
        channel=channel,
        problem=problem,
        wp_user_id=body.wp_user_id,
        wp_username=(body.wp_username or "").strip()[:64],
        ip=client_ip(request),
        ip_forwarded=(request.headers.get("x-forwarded-for") or request.headers.get("x-real-ip") or "")[:45] or None,
        ua=(request.headers.get("user-agent") or "")[:120] or None,
        device_hash=(body.device_hash or "").strip()[:64] or None,
        status="pending",
    )
    db.add(row)
    db.commit()
    db.refresh(row)
    return {
        "ok": True,
        "message": "已提交，感谢反馈",
        "data": {
            "id": row.id,
            "game_id": row.game_id,
            "channel": row.channel,
            "created_at": row.created_at.isoformat() if row.created_at else None,
        },
    }


# ==================== 许愿游戏（2026-11-22） ====================
# 与「请求更新」的区别：许愿的游戏可能还不存在，所以没有 game_id，
# 游戏名由用户自由填写；后台按归一化名字的哈希聚合，人数决定优先级。

# 渠道枚举与前台「定制游戏」页（page 116）一致，但只保留 5 个正式渠道：
# 国际服 / 私服 / 其他 已按运营要求移除。前端写死同一份；接口侧再校验一次。
WISH_CHANNELS = (
    "TapTap",
    "4399",
    "九游",
    "好游快爆",
    "BiliBili",
)
# 游戏名字数上限（与前台 maxlength 一致）
GAME_NAME_MAX_LEN = 40
# 每人每天最多许愿几款（全局，不分游戏）
MAX_WISH_PER_DAY = 1


def _wish_name_norm(name: str) -> str:
    """归一化游戏名：全角转半角、去掉所有空白、转小写。

    「原神」「 原神」「原　神」必须归并成一款，
    否则后台「人数」会被同一款游戏的写法差异拆散。
    """
    n = unicodedata.normalize("NFKC", name or "")
    n = re.sub(r"\s+", "", n)
    return n.strip().lower()


def _wish_key(name: str) -> str:
    """分组键 = 归一化名字的 SHA1 前 16 位；URL 安全，可直接进接口路径。"""
    return hashlib.sha1(_wish_name_norm(name).encode("utf-8")).hexdigest()[:16]


@router.post("/wish-game")
def submit_wish(
    body: WishRequestCreate,
    request: Request,
    db: Session = Depends(get_db),
):
    """提交许愿：共享令牌 + 渠道白名单 + 每人每天全局 1 次。"""
    _require_submit_token(request)

    name = (body.game_name or "").strip()
    if not name:
        raise HTTPException(status_code=400, detail="请填写游戏名")
    if len(name) > GAME_NAME_MAX_LEN:
        raise HTTPException(
            status_code=400,
            detail=f"游戏名最多 {GAME_NAME_MAX_LEN} 字（当前 {len(name)} 字）",
        )
    # 只有空白 / 不可见字符的名字，归一化后为空，等于没填
    if not _wish_name_norm(name):
        raise HTTPException(status_code=400, detail="请填写游戏名")

    channel = (body.channel or "").strip()
    if channel not in WISH_CHANNELS:
        raise HTTPException(status_code=400, detail="渠道无效，请刷新页面重试")

    start_utc, end_utc = _day_range_utc()
    today = db.query(WishRequest).filter(
        WishRequest.wp_user_id == body.wp_user_id,
        WishRequest.created_at >= start_utc,
        WishRequest.created_at < end_utc,
    )
    if today.count() >= MAX_WISH_PER_DAY:
        raise HTTPException(status_code=429, detail="今天已经许愿过了")

    row = WishRequest(
        game_key=_wish_key(name),
        game_name=name[:128],
        name_norm=_wish_name_norm(name)[:128],
        channel=channel,
        wp_user_id=body.wp_user_id,
        wp_username=(body.wp_username or "").strip()[:64],
        ip=client_ip(request),
        ip_forwarded=(
            request.headers.get("x-forwarded-for") or request.headers.get("x-real-ip") or ""
        )[:45]
        or None,
        ua=(request.headers.get("user-agent") or "")[:120] or None,
        device_hash=(body.device_hash or "").strip()[:64] or None,
        status="pending",
    )
    db.add(row)
    db.commit()
    db.refresh(row)
    return {
        "ok": True,
        "message": "许愿成功，感谢反馈",
        "data": {
            "id": row.id,
            "game_key": row.game_key,
            "game_name": row.game_name,
            "channel": row.channel,
            "created_at": row.created_at.isoformat() if row.created_at else None,
        },
    }


@router.get("/game-info")
def game_info(game_id: str = "", db: Session = Depends(get_db)):
    """按 game_id 返回游戏名 + 功能列表（后台菜单模板功能，按 feat_num 顺序）。

    2026-11-24：文章页顶部卡片用（游戏名 / 功能缩略 / 全部功能面板）。
    只暴露游戏名与功能名称，不含模板内部结构、包名等后台字段。
    游戏不存在或已停用 → {"ok": true, "data": null}，前台保留手写文案。
    """
    gid = (game_id or "").strip()
    if not gid:
        raise HTTPException(status_code=400, detail="缺少 game_id")
    g = (
        db.query(Game)
        .filter(Game.game_id == gid, Game.enabled.is_(True))
        .first()
    )
    if not g:
        return {"ok": True, "data": None}

    features: list[str] = []
    tpl = (g.menu_template or "").strip()
    if tpl:
        rows = (
            db.query(MenuTemplateFeature.name)
            .filter(MenuTemplateFeature.template_id == tpl)
            .order_by(MenuTemplateFeature.feat_num.asc())
            .all()
        )
        features = [str(r[0]).strip() for r in rows if r and str(r[0]).strip()]

    return {
        "ok": True,
        "data": {"game_id": g.game_id, "name": g.name, "features": features},
    }


@router.get("/game-infos")
def game_infos(ids: str = "", db: Session = Depends(get_db)):
    """批量版 game-info：列表页一次取多款游戏的名称与功能。

    ids=逗号分隔的 game_id（最多 100 个）。
    返回 {"ok": true, "data": {"<game_id>": {"name": ..., "features": [...]}}}
    不存在的 game_id 直接不出现在 data 里，前端据此回退。
    """
    raw = (ids or "").strip()
    if not raw:
        raise HTTPException(status_code=400, detail="缺少 ids")
    gids: list[str] = []
    for part in raw.split(","):
        g = part.strip()
        if g and g not in gids:
            gids.append(g)
        if len(gids) >= 100:
            break
    if not gids:
        raise HTTPException(status_code=400, detail="缺少 ids")

    rows = (
        db.query(Game)
        .filter(Game.game_id.in_(gids), Game.enabled.is_(True))
        .all()
    )
    tpls = sorted(
        {(r.menu_template or "").strip() for r in rows if (r.menu_template or "").strip()}
    )
    feat_map: dict[str, list[str]] = {}
    if tpls:
        feats = (
            db.query(MenuTemplateFeature.template_id, MenuTemplateFeature.name)
            .filter(MenuTemplateFeature.template_id.in_(tpls))
            .order_by(MenuTemplateFeature.feat_num.asc())
            .all()
        )
        for tid, nm in feats:
            name = str(nm or "").strip()
            if name:
                feat_map.setdefault(str(tid), []).append(name)

    data: dict[str, dict] = {}
    for r in rows:
        tpl = (r.menu_template or "").strip()
        data[r.game_id] = {"name": r.name, "features": feat_map.get(tpl, [])}
    return {"ok": True, "data": data}



# ---------------------------------------------------------------------------
# 2026-11-24 新增：文章页「下载地址」自动填充（唯一下载渠道来源 = 星光后台游戏列表）
#    GET /v1/public/game-downloads?game_id=xxx
#    只读公开接口，返回 game_id / 游戏名 / 网盘列表，不含后台任何敏感字段。
#    背景：WordPress 文章页不再手写百度/夸克链接，改由后台 games.download_url 自动下发。
# ---------------------------------------------------------------------------

import json as _json


def _guess_netdisk_name(url: str) -> str:
    """旧数据只有 URL 时按域名猜网盘名，保证前台按钮文案不留空。"""
    u = (url or "").lower()
    if "pan.baidu" in u or "baidu.com" in u:
        return "百度网盘"
    if "quark.cn" in u:
        return "夸克网盘"
    if "uc.cn" in u:
        return "UC网盘"
    if "aliyundrive" in u or "alipan" in u:
        return "阿里云盘"
    if "123pan" in u:
        return "123网盘"
    if "lanzou" in u:
        return "蓝奏云"
    if "weiyun" in u:
        return "微云"
    return "下载地址"


def _parse_downloads(raw: str | None) -> list[dict]:
    """把 games.download_url 归一成 [{"name": ..., "url": ...}]。

    兼容三种历史形态：
      · JSON 数组 [{"name": "百度网盘", "url": "https://..."}]
      · JSON 字符串数组 ["https://..."]
      · 旧数据：纯 URL 字符串 "https://..."
    任何解析不了的脏数据都返回空列表，由前台回退到静态链接。
    """
    text = (raw or "").strip()
    if not text:
        return []
    parsed = None
    if text[0] in "[{":
        try:
            parsed = _json.loads(text)
        except (ValueError, TypeError):
            parsed = None
    if isinstance(parsed, dict):
        parsed = [parsed]
    if isinstance(parsed, list):
        items: list[dict] = []
        for it in parsed:
            if isinstance(it, dict):
                url = str(it.get("url") or "").strip()
                name = str(it.get("name") or "").strip()
            else:
                url = str(it or "").strip()
                name = ""
            if not url:
                continue
            items.append(
                {"name": name or _guess_netdisk_name(url), "url": url}
            )
        return items
    if text.lower().startswith(("http://", "https://")):
        return [{"name": _guess_netdisk_name(text), "url": text}]
    return []


@router.get("/game-downloads")
def game_downloads(game_id: str = "", db: Session = Depends(get_db)):
    """按 game_id 返回该游戏在后台配置的网盘列表（只读）。

    返回：
      · 游戏不存在 / 已停用  → {"ok": true, "data": null}（前台保留静态兜底链接）
      · 游戏存在            → {"ok": true, "data": {"game_id", "name", "downloads": [...]}}
    """
    gid = (game_id or "").strip()
    if not gid:
        raise HTTPException(status_code=400, detail="缺少 game_id")
    g = (
        db.query(Game)
        .filter(Game.game_id == gid, Game.enabled.is_(True))
        .first()
    )
    if not g:
        return {"ok": True, "data": None}
    return {
        "ok": True,
        "data": {
            "game_id": g.game_id,
            "name": g.name,
            "downloads": _parse_downloads(getattr(g, "download_url", None)),
        },
    }
