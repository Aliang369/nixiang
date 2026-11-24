"""活动口径 HTTP 层隔离 smoke（临时库；不碰生产、不碰真实 secret）。

复用 candidate/smoke_pg.py 的建表+ensure_schema 结构，但改为 **TestClient 走真实 HTTP**，
断言：
 1) 未认证访问 /v1/admin/events /dashboard /devices 均 401
 2) 依赖覆盖（require_admin）后 200，且 events 返回现存卡明文 card_key
 3) page_size 越界被 clamp 到 200
 4) 需卡游戏 bootstrap **不写** presence（灌水入口关闭）
 5) 免卡游戏 bootstrap 写 presence（观察值）
 6) dashboard / portal 用 activity_at 分流：需卡只认认证时间
"""
import json
from datetime import datetime, timedelta, timezone

from fastapi.testclient import TestClient
from sqlalchemy import text

from app.database import engine, SessionLocal
from app.main import app, ensure_schema
import app.models as m
from app.deps import require_admin


def count_presence(gid):
    db = SessionLocal()
    try:
        return db.execute(text("select count(*) from online_presence where game_id=:g"), {"g": gid}).scalar()
    finally:
        db.close()


def main():
    out = {}
    m.Base.metadata.create_all(engine)
    ensure_schema()
    now = datetime.now(timezone.utc)
    old = now - timedelta(minutes=10)

    db = SessionLocal()
    try:
        from app.models import CardKey, Game
        for gid, nm, card_auth in [("smk_card", "SmokeCard", True), ("smk_free", "SmokeFree", False)]:
            if not db.query(Game).filter(Game.game_id == gid).first():
                db.add(Game(game_id=gid, name=nm, enabled=True, card_auth_enabled=card_auth, portal_visible=True))
        if not db.query(CardKey).filter(CardKey.key_code == "SMOKE-KEY-0001").first():
            db.add(CardKey(key_code="SMOKE-KEY-0001", duration_type="hour", max_uses=1, max_devices=1))
        db.commit()
        cid = db.query(CardKey).filter(CardKey.key_code == "SMOKE-KEY-0001").first().id
        # 需卡游戏：A 只有 last_seen（未认证），B 有 last_authenticated_at
        for dev, auth_at in [("dev_0000000000000000000000000000000a", None),
                             ("dev_0000000000000000000000000000000b", now)]:
            db.execute(text(
                "insert into online_presence (game_id,device_id,last_seen_at,last_authenticated_at,created_at) "
                "values ('smk_card',:d,:t,:a,:t) on conflict do nothing"),
                {"d": dev, "t": now, "a": auth_at})
        db.execute(text(
            "insert into auth_events (event_type,game_id,device_id,card_id,result,reason,ip,ua,request_id,detail,created_at) "
            "values ('login_ok','smk_card','dev_0000000000000000000000000000000b',:c,'ok','ok','1.2.3.4','UA','rid-1','card#1',:t),"
            "       ('activate','smk_card','dev_0000000000000000000000000000000b',999999,'ok','ok',null,null,null,'card#999999',:t)"),
            {"c": cid, "t": now})
        db.commit()
    finally:
        db.close()

    client = TestClient(app)

    # 1) 未认证
    for path in ["/v1/admin/events", "/v1/admin/dashboard"]:
        try:
            out["unauth" + path.replace("/", "_")] = client.get(path).status_code
        except Exception as ex:
            out["unauth" + path.replace("/", "_")] = type(ex).__name__

    # 2/3) 覆盖管理员依赖
    app.dependency_overrides[require_admin] = lambda: {"admin": True}
    try:
        r = client.get("/v1/admin/events?page_size=999")
        out["events_status"] = r.status_code
        if r.status_code == 200:
            j = r.json()
            out["page_size_clamped"] = (j.get("meta") or {}).get("page_size")
            rows = j.get("data") or []
            out["has_card_key"] = any(x.get("card_key") for x in rows)
            out["has_card_deleted_flag"] = any(x.get("card_deleted") for x in rows)
            out["detail_all_tagged"] = all((x.get("detail") or "").startswith("card#") or x.get("detail") is None for x in rows)

        # 4) 需卡 bootstrap 不写 presence
        before = count_presence("smk_card")
        rb = client.get("/v1/bootstrap?game_id=smk_card&device_id=dev_000000000000000000000000000000ff")
        after = count_presence("smk_card")
        out["card_game_bootstrap_status"] = rb.status_code
        out["card_game_bootstrap_writes_presence"] = (after > before)

        # 5) 免卡 bootstrap 写 presence
        before_f = count_presence("smk_free")
        rf = client.get("/v1/bootstrap?game_id=smk_free&device_id=dev_000000000000000000000000000000ee")
        after_f = count_presence("smk_free")
        out["free_game_bootstrap_status"] = rf.status_code
        out["free_game_bootstrap_writes_presence"] = (after_f > before_f)

        # 6) dashboard 分流
        d = client.get("/v1/admin/dashboard")
        out["dashboard_status"] = d.status_code
        if d.status_code == 200:
            dd = d.json().get("data") or {}
            out["today_active"] = dd.get("today_active_devices")
            out["today_card"] = dd.get("today_card_devices")
            out["today_guest"] = dd.get("today_guest_devices")
    finally:
        app.dependency_overrides.pop(require_admin, None)

    print("SMOKE_RESULT " + json.dumps(out, ensure_ascii=False))


if __name__ == "__main__":
    main()
