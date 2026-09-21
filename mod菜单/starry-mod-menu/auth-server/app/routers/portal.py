"""门户公开接口：只读，供 /portal/ 前端页展示游戏与菜单功能。"""

import json
import re
from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends
from sqlalchemy import func
from sqlalchemy.orm import Session

from ..database import get_db
from ..models import FeatureFlag, Game, OnlinePresence

router = APIRouter(prefix="/v1/portal", tags=["portal"])

# 下载渠道 JSON 解析失败的兜底渠道名
_FALLBACK_CHANNEL = "其它"

_PROTO_RE = re.compile(r"^https?://", re.IGNORECASE)


def _normalize_url(url: str) -> str:
    """无协议链接补 https://，否则 <a href> 会当站内相对路径。"""
    s = url.strip()
    return s if _PROTO_RE.match(s) else "https://" + s


def parse_downloads(raw: str | None) -> list[dict]:
    """把 games.download_url 解析成渠道列表 [{name,url}]，最多 3 个。

    兼容两种历史数据：JSON 数组（后台多渠道编辑器写入）与纯 URL 字符串。
    """
    if not raw:
        return []
    s = str(raw).strip()
    if s.startswith("["):
        try:
            arr = json.loads(s)
        except (ValueError, TypeError):
            arr = None
        if isinstance(arr, list):
            out: list[dict] = []
            for item in arr:
                if isinstance(item, dict) and str(item.get("url") or "").strip():
                    out.append(
                        {
                            "name": str(item.get("name") or _FALLBACK_CHANNEL)[:32],
                            "url": _normalize_url(str(item["url"])),
                        }
                    )
                elif isinstance(item, str) and item.strip():
                    out.append({"name": _FALLBACK_CHANNEL, "url": _normalize_url(item)})
                if len(out) >= 3:
                    break
            return out
    return [{"name": _FALLBACK_CHANNEL, "url": _normalize_url(s)}]


@router.get("/games")
def portal_games(db: Session = Depends(get_db)):
    """启用游戏 + 各自已启用的功能列表（公开只读，无需登录）。

    附带两种排序依据：updated_at（后台最近编辑时间，「最新」）、
    hot7d（近 7 天活跃设备数，「最热」）。具体排序由前端完成。
    """
    games = (
        db.query(Game)
        .filter(Game.enabled.is_(True), Game.portal_visible.is_(True))
        .order_by(Game.created_at.desc())
        .all()
    )
    flags = (
        db.query(FeatureFlag)
        .filter(FeatureFlag.enabled.is_(True))
        .order_by(FeatureFlag.game_id, FeatureFlag.feat_num)
        .all()
    )
    feats_by_game: dict[str, list[dict]] = {}
    for f in flags:
        feats_by_game.setdefault(f.game_id, []).append(
            {"feat_num": f.feat_num, "name": f.name}
        )
    # 近 7 天活跃设备数（表上 (game_id, device_id) 唯一，count(id)=去重设备数）
    hot7_cut = datetime.now(timezone.utc) - timedelta(days=7)
    hot7 = dict(
        db.query(OnlinePresence.game_id, func.count(OnlinePresence.id))
        .filter(OnlinePresence.last_seen_at >= hot7_cut)
        .group_by(OnlinePresence.game_id)
        .all()
    )
    return {
        "ok": True,
        "message": "ok",
        "data": [
            {
                "game_id": g.game_id,
                "name": g.name,
                "menu_template": g.menu_template,
                "download_url": g.download_url,
                "downloads": parse_downloads(g.download_url),
                "channel": getattr(g, "channel", None),
                "updated_at": g.updated_at.isoformat() if g.updated_at else None,
                "hot7d": hot7.get(g.game_id, 0),
                "features": feats_by_game.get(g.game_id, []),
            }
            for g in games
        ],
    }
