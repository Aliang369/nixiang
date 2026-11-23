"""游客统计 / 在线口径 / 可疑丢弃 回归（2026-11-23）。

只跑临时 SQLite + 直接调用 service/router 函数，不启动生产应用、不连线上。
覆盖：游客上报落库且不写认证、身份不被后续游客上报覆盖、/v1/presence 游客与卡密分支、
可疑上报丢弃（设备形态 / 证书白名单）且不落库、看板卡密与游客按设备互斥、
设备日志 identity 筛选、丢弃计数只在本地累计。
"""
from __future__ import annotations

import os
import sys
import tempfile
import unittest
import uuid

_TMP = tempfile.mkdtemp(prefix="auth-guest-test-")
os.environ.setdefault("DATABASE_URL", f"sqlite:///{_TMP}/t.db")
os.environ.setdefault("SECRET_KEY", "test-secret")
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from starlette.requests import Request  # noqa: E402

from app.config import get_settings  # noqa: E402
from app.database import Base, SessionLocal, engine  # noqa: E402
from app.models import AuthEvent, CardKey, Game, OnlinePresence  # noqa: E402
from app.routers import admin as admin_router  # noqa: E402
from app.routers import client as client_router  # noqa: E402
from app.schemas import PresenceRequest  # noqa: E402
from app.security import utcnow  # noqa: E402
from app.services import auth_service, dropstats, trust  # noqa: E402
from app.services.events import record_guest_seen  # noqa: E402
from app.services.presence import clear_card_presence, touch_presence  # noqa: E402

Base.metadata.create_all(bind=engine)

DEV32 = "dev_" + "c" * 32
DEV40 = "dev_" + "d" * 40


def _req(peer: str = "10.0.0.9", headers: dict | None = None) -> Request:
    raw = [(k.lower().encode(), v.encode()) for k, v in (headers or {}).items()]
    return Request({"type": "http", "headers": raw, "client": (peer, 1234)})


def _new_game(db, prefix: str, card_auth: bool = True) -> str:
    gid = prefix + uuid.uuid4().hex[:6]
    db.add(Game(game_id=gid, name=gid, card_auth_enabled=card_auth))
    db.commit()
    return gid


class GuestPresenceTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "guest")

    def tearDown(self):
        self.db.close()

    def test_guest_upload_marks_guest_without_card_facts(self):
        touch_presence(self.db, self.gid, DEV32)
        self.db.commit()
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertEqual(row.identity, "guest")
        self.assertIsNotNone(row.guest_seen_at)
        self.assertIsNone(row.card_id)
        self.assertIsNone(row.card_verified_at)
        self.assertIsNone(row.last_authenticated_at)

    def test_card_identity_not_downgraded_by_guest_upload(self):
        card = CardKey(
            key_code="G-" + uuid.uuid4().hex[:8], scope="game", game_id=self.gid, status="active"
        )
        self.db.add(card)
        self.db.commit()
        touch_presence(self.db, self.gid, DEV32, card_id=card.id, verified=True)
        self.db.commit()
        # 之后再来一次游客上报：identity 必须仍是 card（不能随最后一次请求漂移）
        touch_presence(self.db, self.gid, DEV32)
        self.db.commit()
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertEqual(row.identity, "card")
        self.assertEqual(row.card_id, card.id)
        # 注：未认证上报有 5 秒写合并（WRITE_COALESCE_SECONDS），这里的第二次上报会被合并，
        # 所以不断言 guest_seen_at 前进，只断言身份未被降级。

    def test_clear_card_presence_returns_guest_identity(self):
        card = CardKey(
            key_code="G-" + uuid.uuid4().hex[:8], scope="game", game_id=self.gid, status="active"
        )
        self.db.add(card)
        self.db.commit()
        touch_presence(self.db, self.gid, DEV32, card_id=card.id, verified=True)
        self.db.commit()
        clear_card_presence(self.db, card.id)
        self.db.commit()
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertIsNone(row.card_id)
        self.assertEqual(row.identity, "guest")

    def test_card_game_bootstrap_records_guest_row(self):
        boot = auth_service.bootstrap_with_grant(self.db, self.gid, device_id=DEV40, logged_in=False)
        self.assertTrue(boot)
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV40).one()
        self.assertEqual(row.identity, "guest")
        self.assertIsNone(row.card_id)

    def test_count_presence_false_skips_row(self):
        gid2 = _new_game(self.db, "skip", card_auth=False)
        auth_service.bootstrap_with_grant(
            self.db, gid2, device_id=DEV40, logged_in=False, count_presence=False
        )
        self.assertEqual(
            self.db.query(OnlinePresence).filter_by(game_id=gid2, device_id=DEV40).count(), 0
        )


class PresenceEndpointTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.gid = _new_game(self.db, "pres")

    def tearDown(self):
        self.db.close()

    def test_guest_presence_endpoint_counts_and_logs_event(self):
        out = client_router.presence(
            PresenceRequest(game_id=self.gid, device_id=DEV32), _req(), self.db
        )
        self.assertTrue(out["data"]["counted"])
        self.assertEqual(out["data"]["identity"], "guest")
        row = self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id=DEV32).one()
        self.assertEqual(row.identity, "guest")
        ev = (
            self.db.query(AuthEvent)
            .filter_by(event_type="guest_seen", game_id=self.gid)
            .count()
        )
        self.assertEqual(ev, 1)
        # 24 小时内重复上报不再落事件（防刷屏），但在线行照常刷新
        client_router.presence(
            PresenceRequest(game_id=self.gid, device_id=DEV32), _req(), self.db
        )
        self.assertEqual(
            self.db.query(AuthEvent).filter_by(event_type="guest_seen", game_id=self.gid).count(), 1
        )

    def test_bad_device_is_dropped_without_rows(self):
        before = dropstats.snapshot()["total"]
        out = client_router.presence(
            PresenceRequest(game_id=self.gid, device_id="devBAD"), _req(), self.db
        )
        self.assertFalse(out["data"]["counted"])
        self.assertEqual(
            self.db.query(OnlinePresence).filter_by(game_id=self.gid, device_id="devBAD").count(), 0
        )
        self.assertEqual(
            self.db.query(AuthEvent).filter_by(game_id=self.gid).count(), 0
        )
        self.assertGreater(dropstats.snapshot()["total"], before)

    def test_unknown_game_is_not_counted(self):
        out = client_router.presence(
            PresenceRequest(game_id="no_such_game", device_id=DEV32), _req(), self.db
        )
        self.assertFalse(out["data"]["counted"])


class TrustJudgeTest(unittest.TestCase):
    def test_bad_device_reason(self):
        self.assertEqual(trust.judge(device_id="devBAD"), "bad_device")
        self.assertIsNone(trust.judge(device_id=DEV32))

    def test_cert_whitelist(self):
        s = get_settings()
        old = s.client_cert_sha256
        try:
            s.client_cert_sha256 = "aa" * 32
            self.assertEqual(trust.judge(device_id=DEV32, cert_sha256=None), "bad_cert")
            self.assertEqual(trust.judge(device_id=DEV32, cert_sha256="bb" * 32), "bad_cert")
            self.assertIsNone(trust.judge(device_id=DEV32, cert_sha256="AA:" * 31 + "aa"))
        finally:
            s.client_cert_sha256 = old


class DashboardGuestSplitTest(unittest.TestCase):
    def test_card_and_guest_are_mutually_exclusive_per_device(self):
        db = SessionLocal()
        try:
            db.query(OnlinePresence).delete()
            db.commit()
            gid = _new_game(db, "dashg", card_auth=False)
            now = utcnow()
            db.add_all(
                [
                    # 卡密设备
                    OnlinePresence(
                        game_id=gid, device_id=DEV32, last_seen_at=now, created_at=now,
                        identity="card", card_id=1,
                    ),
                    # 纯游客设备
                    OnlinePresence(
                        game_id=gid, device_id=DEV40, last_seen_at=now, created_at=now,
                        identity="guest", guest_seen_at=now,
                    ),
                ]
            )
            db.commit()
            data = admin_router.dashboard(_={}, db=db)["data"]
            self.assertEqual(data["total_online"], 2)
            self.assertEqual(data["online_card"], 1)
            self.assertEqual(data["online_guest"], 1)
            self.assertEqual(data["online_window_seconds"], int(get_settings().online_window_seconds))
            self.assertEqual(data["today_guest_devices"], 1)
            self.assertEqual(data["today_card_devices"], 1)
        finally:
            db.close()


class EventsIdentityFilterTest(unittest.TestCase):
    def test_filter_by_identity(self):
        db = SessionLocal()
        try:
            gid = _new_game(db, "evid")
            record_guest_seen(
                db, game_id=gid, device_id=DEV32, request=_req(), commit=True
            )
            record_guest_seen(
                db, game_id=gid, device_id=DEV40, request=_req(), commit=True
            )
            out = admin_router.list_events(
                device_id=None, card_id=None, game_id=gid, event_type="guest_seen",
                result=None, identity="guest", from_at=None, to_at=None,
                page=1, page_size=50, _={}, db=db,
            )
            self.assertEqual(out["meta"]["total"], 2)
            self.assertEqual({r["identity"] for r in out["data"]}, {"guest"})
            out2 = admin_router.list_events(
                device_id=None, card_id=None, game_id=gid, event_type=None,
                result=None, identity="card", from_at=None, to_at=None,
                page=1, page_size=50, _={}, db=db,
            )
            self.assertEqual(out2["meta"]["total"], 0)
        finally:
            db.close()


if __name__ == "__main__":
    unittest.main()
