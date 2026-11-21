"""设备可信化 / 事件审计回归（2026-11-15）。

只跑临时 SQLite + 直接调用 service/router 函数，不启动生产应用、不连线上。
覆盖：非法 device 不落库、bootstrap 不清卡、自报 logged_in 不认证、登录写 verified、
detail 无明文卡密、撤销清关联、看板排除不可信设备、事件接口鉴权/过滤/打码、
失败聚合 suppressed、IP 只信受信反代。
"""
from __future__ import annotations

import os
import sys
import tempfile
import unittest
import uuid

_TMP = tempfile.mkdtemp(prefix="auth-trust-test-")
os.environ.setdefault("DATABASE_URL", f"sqlite:///{_TMP}/t.db")
os.environ.setdefault("SECRET_KEY", "test-secret")
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi import HTTPException  # noqa: E402
from starlette.requests import Request  # noqa: E402

from app.config import get_settings  # noqa: E402
from app.database import Base, SessionLocal, engine  # noqa: E402
from app.deviceid import is_valid_device_id  # noqa: E402
from app.models import AdminAuditLog, AuthEvent, CardKey, Game, OnlinePresence  # noqa: E402
from app.routers import admin as admin_router  # noqa: E402
from app.security import utcnow  # noqa: E402
from app.security_guard import resolve_client_ip  # noqa: E402
from app.services import auth_service  # noqa: E402
from app.services.events import record_auth_event, record_failure_event  # noqa: E402
from app.services.presence import clear_card_presence, touch_presence  # noqa: E402

Base.metadata.create_all(bind=engine)

DEV32 = "dev_" + "a" * 32
DEV40 = "dev_" + "b" * 40


def _req(peer: str = "10.0.0.9", headers: dict | None = None) -> Request:
    raw = [(k.lower().encode(), v.encode()) for k, v in (headers or {}).items()]
    return Request({"type": "http", "headers": raw, "client": (peer, 1234)})


def _new_game(db, prefix: str) -> str:
    """每个测试用唯一 game_id，避免同一临时库内 UNIQUE 冲突与跨用例串味。"""
    gid = prefix + uuid.uuid4().hex[:6]
    db.add(Game(game_id=gid, name=gid))
    db.commit()
    return gid


class DeviceValidationTest(unittest.TestCase):
    def test_accepts_client_formats_only(self):
        self.assertTrue(is_valid_device_id(DEV32))
        self.assertTrue(is_valid_device_id(DEV40))
        for bad in ["devA", "devtest0", "dev-647-", "3fba095a", "dev_" + "A" * 32, ""]:
            self.assertFalse(is_valid_device_id(bad), bad)

    def test_bootstrap_invalid_device_not_persisted(self):
        db = SessionLocal()
        try:
            gid = _new_game(db, "val")
            boot = auth_service.bootstrap_with_grant(db, gid, device_id="devA", logged_in=False)
            self.assertTrue(boot)
            self.assertEqual(db.query(OnlinePresence).filter_by(game_id=gid).count(), 0)
        finally:
            db.close()


class BootstrapCardPreservationTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "boot")

    def tearDown(self):
        self.db.close()

    def test_logged_out_bootstrap_keeps_verified_card(self):
        card = CardKey(
            key_code="C-" + uuid.uuid4().hex[:10], scope="game", game_id=self.gid, status="active"
        )
        self.db.add(card)
        self.db.commit()
        touch_presence(self.db, self.gid, DEV32, card_id=card.id, verified=True)
        self.db.commit()
        auth_service.bootstrap_with_grant(self.db, self.gid, device_id=DEV32, logged_in=False)
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertEqual(row.card_id, card.id)
        self.assertIsNotNone(row.card_verified_at)

    def test_self_reported_logged_in_is_not_verification(self):
        # 需卡游戏：bootstrap 连观测行都不写（防灌水）
        auth_service.bootstrap_with_grant(self.db, self.gid, device_id=DEV40, logged_in=True)
        self.assertEqual(
            self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV40).count(), 0
        )

    def test_self_reported_logged_in_on_open_game_is_not_verification(self):
        # 免卡游戏：可写观测行，但 logged_in 自报不得产生认证/关联
        self.db.query(Game).filter(Game.game_id == self.gid).update({"card_auth_enabled": False})
        self.db.commit()
        auth_service.bootstrap_with_grant(self.db, self.gid, device_id=DEV40, logged_in=True)
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV40).one()
        self.assertIsNone(row.card_id)
        self.assertIsNone(row.card_verified_at)
        self.assertIsNone(row.last_authenticated_at)
        self.assertIsNotNone(row.created_at)


class LoginAuditTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "login")
        self.key = "SG-" + uuid.uuid4().hex[:4].upper() + "-AAAA-BBBB-CCCC"
        self.card = CardKey(
            key_code=self.key, scope="game", game_id=self.gid, status="unused",
            duration_type="day", max_uses=0, max_devices=1,
        )
        self.db.add(self.card)
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def test_login_writes_verified_and_no_plaintext_key(self):
        req = _req(headers={"user-agent": "StarMenu/1.0"})
        req.state.request_id = "rid-login-1"
        auth_service.login_card(self.db, self.key, self.gid, DEV32, request=req)
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertEqual(row.card_id, self.card.id)
        self.assertIsNotNone(row.card_verified_at)
        ev = (
            self.db.query(AuthEvent)
            .filter_by(event_type="login_ok", game_id=self.gid)
            .order_by(AuthEvent.id.desc())
            .first()
        )
        self.assertNotIn(self.key, ev.detail or "")
        self.assertEqual(ev.detail, f"card#{self.card.id}")
        # 成功事件也必须有来源元数据（否则后台无法按 IP/UA/请求号追溯）
        self.assertEqual(ev.ip, "10.0.0.9")
        self.assertEqual(ev.ua, "StarMenu/1.0")
        self.assertEqual(ev.request_id, "rid-login-1")
        self.assertEqual(ev.result, "ok")

    def test_login_rejects_invalid_device(self):
        with self.assertRaises(HTTPException) as ctx:
            auth_service.login_card(self.db, self.key, self.gid, "devA")
        self.assertEqual(ctx.exception.status_code, 400)
        self.assertEqual(
            self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id="devA").count(), 0
        )

    def test_clear_card_presence(self):
        touch_presence(self.db, self.gid, DEV32, card_id=self.card.id, verified=True)
        self.db.commit()
        clear_card_presence(self.db, self.card.id)
        self.db.commit()
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertIsNone(row.card_id)
        self.assertIsNone(row.card_verified_at)


class DashboardTrustTest(unittest.TestCase):
    def test_untrusted_devices_excluded_from_metrics(self):
        db = SessionLocal()
        try:
            # 看板是全库聚合：先清空临时库在线行，保证断言只反映本用例数据
            db.query(OnlinePresence).delete()
            db.commit()
            gid = _new_game(db, "dash")
            # 用免卡游戏验证「形态过滤」本身（需卡口径另有专门用例）
            db.query(Game).filter(Game.game_id == gid).update({"card_auth_enabled": False})
            db.commit()
            now = utcnow()
            db.add_all(
                [
                    OnlinePresence(game_id=gid, device_id=DEV32, last_seen_at=now, created_at=now),
                    OnlinePresence(game_id=gid, device_id=DEV40, last_seen_at=now, created_at=now),
                    OnlinePresence(game_id=gid, device_id="devA", last_seen_at=now, created_at=now),
                    OnlinePresence(
                        game_id=gid, device_id="' OR 1=1 --", last_seen_at=now, created_at=now
                    ),
                ]
            )
            db.commit()
            data = admin_router.dashboard(_={}, db=db)["data"]
            self.assertEqual(data["today_active_devices"], 2)
            self.assertEqual(data["today_verified_devices"], 0)
            self.assertEqual(data["today_unverified_devices"], 2)
            self.assertIn("设备数≠人数", data["data_note"])
        finally:
            db.close()


class EventsApiTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "ev")

    def tearDown(self):
        self.db.close()

    def test_failure_dedupe_and_fingerprint(self):
        for _ in range(3):
            record_failure_event(
                self.db, "login_fail", game_id=self.gid, device_id="devA",
                reason="not_found", request=_req(),
            )
        rows = self.db.query(AuthEvent).filter_by(event_type="login_fail", game_id=self.gid).all()
        self.assertEqual(len(rows), 1)
        self.assertEqual(rows[0].suppressed, 2)
        self.assertTrue(rows[0].device_id.startswith("bad_"))
        self.assertNotIn("devA", rows[0].device_id)
        self.assertIsNotNone(rows[0].last_occurred_at)

    def test_events_list_masks_device_and_detail(self):
        record_auth_event(
            self.db, "login_fail", game_id=self.gid, device_id=DEV32, card_id=7,
            detail="key SG-AAAA-BBBB-CCCC-DDDD leaked", result="deny", reason="not_found",
            request=_req(headers={"user-agent": "UA/1.0"}),
        )
        out = admin_router.list_events(
            device_id=DEV32, card_id=None, game_id=self.gid, event_type=None, result=None,
            from_at=None, to_at=None, page=1, page_size=50, _={}, db=self.db,
        )
        self.assertEqual(out["meta"]["total"], 1)
        item = out["data"][0]
        self.assertNotIn(DEV32, item["device_id"])
        self.assertNotIn("SG-AAAA-BBBB-CCCC-DDDD", item["detail"])
        self.assertEqual(item["ua"], "UA/1.0")

    def test_event_detail_no_plaintext_but_card_key_field(self):
        key = "SG-" + uuid.uuid4().hex[:4].upper() + "-AAAA-BBBB-CCCC"
        card = CardKey(key_code=key, scope="game", game_id=self.gid, status="active")
        self.db.add(card)
        self.db.commit()
        record_auth_event(
            self.db, "login_fail", game_id=self.gid, device_id=DEV32, card_id=card.id,
            detail=f"key {key} leaked", result="deny", reason="not_found", request=_req(),
        )
        out = admin_router.list_events(
            device_id=DEV32, card_id=None, game_id=None, event_type=None, result=None,
            from_at=None, to_at=None, page=1, page_size=50, _={}, db=self.db,
        )
        item = out["data"][0]
        self.assertNotIn(key, item["detail"] or "")      # detail 仍脱敏
        self.assertEqual(item["card_key"], key)           # 明文只走独立字段（require_admin）
        self.assertFalse(item["card_deleted"])

    def test_deleted_card_flagged(self):
        record_auth_event(
            self.db, "login_ok", game_id=self.gid, device_id=DEV32, card_id=999999,
            detail="card#999999", result="ok", request=_req(),
        )
        out = admin_router.list_events(
            device_id=DEV32, card_id=None, game_id=None, event_type=None, result=None,
            from_at=None, to_at=None, page=1, page_size=50, _={}, db=self.db,
        )
        item = out["data"][0]
        self.assertIsNone(item["card_key"])
        self.assertTrue(item["card_deleted"])

    def test_events_http_requires_admin(self):
        from fastapi import FastAPI
        from fastapi.testclient import TestClient

        from app.database import get_db

        mini = FastAPI()
        mini.include_router(admin_router.router)

        def _db():
            d = SessionLocal()
            try:
                yield d
            finally:
                d.close()

        mini.dependency_overrides[get_db] = _db
        with TestClient(mini) as client:
            r = client.get("/v1/admin/events")
            self.assertEqual(r.status_code, 401)
            r2 = client.get("/v1/admin/devices/dev_" + "a" * 32)
            self.assertEqual(r2.status_code, 401)

    def test_events_requires_admin(self):
        from app.deps import require_admin

        with self.assertRaises(HTTPException) as ctx:
            require_admin(request=_req(), creds=None, db=self.db)
        self.assertEqual(ctx.exception.status_code, 401)


class DeleteAuditAndUiTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "del")
        self.key = "AL-" + uuid.uuid4().hex[:4].upper() + "-9FO3-SR9C-KX8Y"
        self.card = CardKey(key_code=self.key, scope="game", game_id=self.gid, status="active")
        self.db.add(self.card)
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def test_delete_card_audit_has_no_plaintext_key(self):
        touch_presence(self.db, self.gid, DEV32, card_id=self.card.id, verified=True)
        self.db.commit()
        admin_router.delete_card(self.card.id, _req(), {"sub": "tester"}, self.db)
        row = (
            self.db.query(AdminAuditLog)
            .filter_by(action="cards.delete")
            .order_by(AdminAuditLog.id.desc())
            .first()
        )
        self.assertIsNotNone(row)
        self.assertNotIn(self.key, row.detail or "")
        self.assertNotIn("卡密=", row.detail or "")
        # 删卡后在线行不再挂该卡
        pres = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertIsNone(pres.card_id)
        self.assertIsNone(pres.card_verified_at)

    def test_events_js_has_no_inline_device_js(self):
        path = os.path.join(
            os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
            "static", "admin", "js", "events.js",
        )
        with open(path, encoding="utf-8") as fh:
            src = fh.read()
        # 设备号不得拼进内联 JS（历史 payload 含引号会逃逸）；只允许 data-* + 事件委托
        self.assertNotIn("onclick=\"openDeviceDetail", src)
        self.assertIn("data-dev=", src)
        self.assertIn("closest('button[data-dev]')", src)

    def test_request_id_from_state_not_client_header(self):
        from app.services.events import request_meta

        req = _req(headers={"x-request-id": "client-supplied"})
        req.state.request_id = "server-generated-123"
        meta = request_meta(req)
        self.assertEqual(meta["request_id"], "server-generated-123")


class UnbindAuditTest(unittest.TestCase):
    """解绑成功/失败事件必须带来源元数据，且不落卡密明文（2026-11-15 补）。"""

    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "unb")
        self.key = "AL-" + uuid.uuid4().hex[:4].upper() + "-BBBB-CCCC-DDDD"
        self.card = CardKey(
            key_code=self.key, scope="game", game_id=self.gid, status="unused",
            duration_type="day", max_uses=0, max_devices=2,
        )
        self.db.add(self.card)
        self.db.commit()
        auth_service.login_card(self.db, self.key, self.gid, DEV32, request=_req())

    def tearDown(self):
        self.db.close()

    def test_unbind_success_has_metadata_and_no_plaintext(self):
        from app.routers.client import unbind
        from app.schemas import UnbindRequest

        req = _req(headers={"user-agent": "StarMenu/2.0"})
        req.state.request_id = "rid-unbind-1"
        out = unbind(UnbindRequest(card_key=self.key, device_id=DEV32), req, self.db)
        self.assertTrue(out["ok"])
        ev = (
            self.db.query(AuthEvent)
            .filter_by(event_type="unbind", card_id=self.card.id)
            .order_by(AuthEvent.id.desc())
            .first()
        )
        self.assertIsNotNone(ev)
        self.assertEqual(ev.device_id, DEV32)
        self.assertEqual(ev.ip, "10.0.0.9")
        self.assertEqual(ev.ua, "StarMenu/2.0")
        self.assertEqual(ev.request_id, "rid-unbind-1")
        self.assertEqual(ev.result, "ok")
        self.assertEqual(ev.detail, f"card#{self.card.id}")
        self.assertNotIn(self.key, ev.detail or "")
        # 解绑后本机不再算已验证
        pres = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertIsNone(pres.card_id)
        self.assertIsNone(pres.card_verified_at)

    def test_unbind_failure_records_reason_without_plaintext(self):
        from fastapi import HTTPException as _HE

        from app.routers.client import unbind
        from app.schemas import UnbindRequest

        with self.assertRaises(_HE) as ctx:
            unbind(UnbindRequest(card_key=self.key, device_id=DEV40), _req(), self.db)
        self.assertEqual(ctx.exception.status_code, 403)
        ev = (
            self.db.query(AuthEvent)
            .filter_by(event_type="unbind_fail")
            .order_by(AuthEvent.id.desc())
            .first()
        )
        self.assertIsNotNone(ev)
        self.assertEqual(ev.reason, "device_mismatch")
        self.assertEqual(ev.result, "deny")
        self.assertIsNone(ev.detail)
        self.assertEqual(ev.ip, "10.0.0.9")


class LayeredActivityTest(unittest.TestCase):
    """分层活跃口径：需卡=认证活动时间；免卡=观测值；bootstrap 不刷认证时间。"""

    def setUp(self):
        self.db = SessionLocal()
        self.db.query(OnlinePresence).delete()
        self.db.commit()
        self.gid_card = _new_game(self.db, "cardg")
        self.gid_open = _new_game(self.db, "openg")
        self.db.query(Game).filter(Game.game_id == self.gid_open).update(
            {"card_auth_enabled": False}
        )
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def test_bootstrap_never_writes_or_refreshes_card_game(self):
        auth_service.bootstrap_with_grant(
            self.db, self.gid_card, device_id=DEV32, logged_in=True
        )
        self.assertEqual(
            self.db.query(OnlinePresence).filter_by(game_id=self.gid_card).count(), 0
        )

    def test_open_game_bootstrap_records_observed_only(self):
        auth_service.bootstrap_with_grant(self.db, self.gid_open, device_id=DEV32)
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid_open, device_id=DEV32).one()
        self.assertIsNotNone(row.last_seen_at)
        self.assertIsNone(row.card_verified_at)
        self.assertIsNone(row.last_authenticated_at)

    def test_login_sets_durable_time_and_unbind_keeps_it(self):
        key = "AL-" + uuid.uuid4().hex[:4].upper() + "-ZZZZ-YYYY-XXXX"
        card = CardKey(
            key_code=key, scope="game", game_id=self.gid_card, status="unused",
            duration_type="day", max_uses=0, max_devices=1,
        )
        self.db.add(card)
        self.db.commit()
        auth_service.login_card(self.db, key, self.gid_card, DEV32, request=_req())
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid_card, device_id=DEV32).one()
        self.assertIsNotNone(row.last_authenticated_at)
        durable = row.last_authenticated_at
        auth_service.unbind_card(self.db, key, DEV32, request=_req())
        self.db.refresh(row)
        self.assertIsNone(row.card_id)
        self.assertIsNone(row.card_verified_at)
        self.assertIsNotNone(row.last_authenticated_at, "解绑不得清空认证事实")
        self.assertEqual(row.last_authenticated_at, durable)

    def test_fake_shape_rows_do_not_enter_dashboard(self):
        now = utcnow()
        self.db.add_all(
            [
                OnlinePresence(
                    game_id=self.gid_card, device_id="dev_" + f"{i:032x}",
                    last_seen_at=now, created_at=now,
                )
                for i in range(100)
            ]
        )
        self.db.commit()
        data = admin_router.dashboard(_={}, db=self.db)["data"]
        self.assertEqual(data["today_active_devices"], 0, "无认证证据的合法形态行不得计活跃")
        self.assertEqual(data["all_active_devices"], 0)

    def test_portal_hot7d_uses_authenticated_activity(self):
        from app.routers.portal import portal_games

        now = utcnow()
        self.db.add(
            OnlinePresence(game_id=self.gid_card, device_id=DEV32, last_seen_at=now, created_at=now)
        )
        self.db.commit()
        rows = portal_games(db=self.db)["data"]
        mine = [r for r in rows if r["game_id"] == self.gid_card]
        self.assertTrue(mine)
        self.assertEqual(mine[0]["hot7d"], 0)

class ClientIpTrustTest(unittest.TestCase):
    def setUp(self):
        self._orig = os.environ.get("TRUSTED_PROXY_IPS")
        os.environ["TRUSTED_PROXY_IPS"] = ""
        get_settings.cache_clear()

    def tearDown(self):
        if self._orig is None:
            os.environ.pop("TRUSTED_PROXY_IPS", None)
        else:
            os.environ["TRUSTED_PROXY_IPS"] = self._orig
        get_settings.cache_clear()

    def test_untrusted_peer_spoofed_headers_ignored(self):
        req = _req(peer="10.0.0.9", headers={"x-real-ip": "1.2.3.4", "x-forwarded-for": "5.6.7.8"})
        self.assertEqual(resolve_client_ip(req), ("10.0.0.9", "peer"))

    def test_trusted_peer_reads_x_real_ip(self):
        os.environ["TRUSTED_PROXY_IPS"] = "172.19.0.0/16"
        get_settings.cache_clear()
        req = _req(peer="172.19.0.5", headers={"x-real-ip": "1.2.3.4"})
        self.assertEqual(resolve_client_ip(req), ("1.2.3.4", "x-real-ip"))

    def test_invalid_header_falls_back_to_peer(self):
        os.environ["TRUSTED_PROXY_IPS"] = "172.19.0.0/16"
        get_settings.cache_clear()
        req = _req(peer="172.19.0.5", headers={"x-real-ip": "not-an-ip; DROP TABLE"})
        self.assertEqual(resolve_client_ip(req), ("172.19.0.5", "peer"))


if __name__ == "__main__":
    unittest.main()
