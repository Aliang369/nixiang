"""游戏功能门控：按菜单模板同步到 feature_flags。"""

from __future__ import annotations

from sqlalchemy.orm import Session

from ..models import FeatureFlag, Game
from .menu_templates import get_template, resolve_template_id, template_features


def sync_game_features(
    db: Session,
    game: Game,
    *,
    template_id: str | None = None,
    mode: str = "fill",
) -> dict:
    """
    mode=fill: 只插入缺失的 feat_num
    mode=replace: 补缺失 + 按模板更新已有项的 name（不改 enabled）
    mode=prune: 同 replace，并删除模板中不存在的 feat_num（换错模板后清理用）
    """
    tid = resolve_template_id(db, game.game_id, template_id or game.menu_template)
    if not tid or not get_template(db, tid):
        return {"template": None, "added": 0, "updated": 0, "removed": 0, "features": []}

    if game.menu_template != tid:
        game.menu_template = tid

    feats = template_features(db, tid)
    want_nums = {int(item["feat_num"]) for item in feats}
    update_names = mode in ("replace", "prune")
    added = 0
    updated = 0
    removed = 0
    for item in feats:
        num = int(item["feat_num"])
        name = str(item["name"])
        row = (
            db.query(FeatureFlag)
            .filter(FeatureFlag.game_id == game.game_id, FeatureFlag.feat_num == num)
            .first()
        )
        if not row:
            db.add(
                FeatureFlag(
                    game_id=game.game_id,
                    feat_num=num,
                    name=name,
                    enabled=True,
                )
            )
            added += 1
        elif update_names and row.name != name:
            row.name = name
            updated += 1

    if mode == "prune":
        q = db.query(FeatureFlag).filter(FeatureFlag.game_id == game.game_id)
        if want_nums:
            q = q.filter(~FeatureFlag.feat_num.in_(want_nums))
        for row in q.all():
            db.delete(row)
            removed += 1

    db.commit()
    return {
        "template": tid,
        "added": added,
        "updated": updated,
        "removed": removed,
        "features": feats,
    }
