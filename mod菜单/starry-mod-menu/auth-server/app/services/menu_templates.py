"""菜单模板：运行时读数据库；内置 SEED 仅在缺省时写入。"""

from __future__ import annotations

from typing import Any

from sqlalchemy.orm import Session

from ..menu_catalog import SEED_TEMPLATES
from ..models import Game, MenuTemplate, MenuTemplateFeature


def _normalize_features(features: list[dict[str, Any]] | None) -> list[dict[str, Any]]:
    out: list[dict[str, Any]] = []
    seen: set[int] = set()
    for item in features or []:
        num = int(item["feat_num"])
        name = str(item.get("name") or "").strip()
        if not name:
            raise ValueError(f"功能 #{num} 名称不能为空")
        if num in seen:
            raise ValueError(f"功能编号重复: {num}")
        seen.add(num)
        out.append({"feat_num": num, "name": name})
    out.sort(key=lambda x: x["feat_num"])
    return out


def _features_for(db: Session, template_id: str) -> list[dict[str, Any]]:
    rows = (
        db.query(MenuTemplateFeature)
        .filter(MenuTemplateFeature.template_id == template_id)
        .order_by(MenuTemplateFeature.feat_num.asc())
        .all()
    )
    return [{"feat_num": r.feat_num, "name": r.name} for r in rows]


def _template_dict(t: MenuTemplate, features: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "id": t.template_id,
        "name": t.name,
        "package": t.package,
        "source": t.source,
        "feature_count": len(features),
        "features": features,
        "created_at": t.created_at.isoformat() if t.created_at else None,
        "updated_at": t.updated_at.isoformat() if t.updated_at else None,
    }


def list_templates(db: Session) -> list[dict[str, Any]]:
    rows = (
        db.query(MenuTemplate)
        .order_by(MenuTemplate.created_at.desc(), MenuTemplate.id.desc())
        .all()
    )
    return [_template_dict(t, _features_for(db, t.template_id)) for t in rows]


def get_template(db: Session, template_id: str | None) -> dict[str, Any] | None:
    if not template_id:
        return None
    t = db.query(MenuTemplate).filter(MenuTemplate.template_id == template_id).first()
    if not t:
        return None
    return _template_dict(t, _features_for(db, t.template_id))


def resolve_template_id(
    db: Session, game_id: str, template_id: str | None
) -> str | None:
    """显式模板优先；否则若 game_id 本身是已知模板 key，则自动匹配。"""
    if template_id:
        if db.query(MenuTemplate).filter(MenuTemplate.template_id == template_id).first():
            return template_id
        return None
    if db.query(MenuTemplate).filter(MenuTemplate.template_id == game_id).first():
        return game_id
    return None


def template_features(db: Session, template_id: str) -> list[dict[str, Any]]:
    return _features_for(db, template_id)


def _replace_features(db: Session, template_id: str, features: list[dict[str, Any]]) -> None:
    db.query(MenuTemplateFeature).filter(MenuTemplateFeature.template_id == template_id).delete()
    for item in features:
        db.add(
            MenuTemplateFeature(
                template_id=template_id,
                feat_num=int(item["feat_num"]),
                name=str(item["name"]),
            )
        )


def seed_builtin_templates(db: Session) -> dict[str, int]:
    """把 menu_catalog.SEED_TEMPLATES 写入库中尚不存在的模板（不覆盖已有）。"""
    added = 0
    skipped = 0
    for tid, meta in SEED_TEMPLATES.items():
        exists = db.query(MenuTemplate).filter(MenuTemplate.template_id == tid).first()
        if exists:
            skipped += 1
            continue
        feats = _normalize_features(list(meta.get("features") or []))
        db.add(
            MenuTemplate(
                template_id=tid,
                name=str(meta.get("name") or tid),
                package=meta.get("package"),
                source=meta.get("source"),
            )
        )
        _replace_features(db, tid, feats)
        added += 1
    if added:
        db.commit()
    return {"added": added, "skipped": skipped}


def create_template(
    db: Session,
    *,
    template_id: str,
    name: str,
    package: str | None,
    source: str | None,
    features: list[dict[str, Any]],
) -> dict[str, Any]:
    tid = (template_id or "").strip()
    if not tid:
        raise ValueError("模板 id 不能为空")
    if db.query(MenuTemplate).filter(MenuTemplate.template_id == tid).first():
        raise ValueError("模板 id 已存在")
    feats = _normalize_features(features)
    if not feats:
        raise ValueError("至少需要 1 个功能项")
    t = MenuTemplate(
        template_id=tid,
        name=name.strip(),
        package=(package or None),
        source=(source or None),
    )
    db.add(t)
    _replace_features(db, tid, feats)
    db.commit()
    db.refresh(t)
    return _template_dict(t, feats)


def update_template(
    db: Session,
    template_id: str,
    *,
    name: str | None = None,
    package: str | None = None,
    source: str | None = None,
    features: list[dict[str, Any]] | None = None,
    clear_package: bool = False,
    clear_source: bool = False,
) -> dict[str, Any]:
    t = db.query(MenuTemplate).filter(MenuTemplate.template_id == template_id).first()
    if not t:
        raise LookupError("模板不存在")
    if name is not None:
        t.name = name.strip()
    if clear_package:
        t.package = None
    elif package is not None:
        t.package = package or None
    if clear_source:
        t.source = None
    elif source is not None:
        t.source = source or None
    feats = _features_for(db, template_id)
    if features is not None:
        feats = _normalize_features(features)
        if not feats:
            raise ValueError("至少需要 1 个功能项")
        _replace_features(db, template_id, feats)
    db.commit()
    db.refresh(t)
    return _template_dict(t, feats)


def delete_template(db: Session, template_id: str, *, force: bool = False) -> dict[str, Any]:
    t = db.query(MenuTemplate).filter(MenuTemplate.template_id == template_id).first()
    if not t:
        raise LookupError("模板不存在")
    used = db.query(Game).filter(Game.menu_template == template_id).count()
    if used and not force:
        raise ValueError(f"仍有 {used} 个游戏绑定此模板，确认后可强制删除")
    if used and force:
        db.query(Game).filter(Game.menu_template == template_id).update(
            {Game.menu_template: None}, synchronize_session=False
        )
    db.query(MenuTemplateFeature).filter(MenuTemplateFeature.template_id == template_id).delete()
    db.delete(t)
    db.commit()
    return {"template_id": template_id, "unbound_games": used}
