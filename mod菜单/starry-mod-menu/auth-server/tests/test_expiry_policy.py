"""到期时间策略离线回归（2026-11-12 修复）。

只跑本地 SQLite 内存/临时库 + 直接调用 service 纯函数，不启动生产应用、
不连线上 API、不改任何真实卡。覆盖：
- 首次激活不得覆盖后台手设的 expire_at（ncshztiyan/kyzttiyan 场景）
- 未设到期时间仍按卡种默认时长激活
- 已延期/已复位 unused 的卡不被缩时
- 手设过去时间不被重激活延长（激活后仍判过期）
- 后台只改备注时不发送 expire_at，保留原值（含微秒）
- 批量延期负数/NaN/Inf 被 schema 拒收；0/None 兼容
- update/batch 审计含 old->new 到期时间
"""
from __future__ import annotations

import os
import sys
import tempfile
import unittest
from datetime import datetime, timedelta, timezone
from types import SimpleNamespace

_TMP = tempfile.mkdtemp(prefix="auth-expiry-test-")
os.environ.setdefault("DATABASE_URL", f"sqlite:///{_TMP}/t.db")
os.environ.setdefault("SECRET_KEY", "test-secret")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from _grant_testkey import make_test_key  # noqa: E402

# 2026-11-21：授权票改 v2-only，签发必须有私钥，否则 login_card 直接 503；
# 这里用临时测试密钥（现生成、随临时目录丢弃），不触碰任何真实私钥。
os.environ.setdefault("GRANT_RSA_KEY_PATH", make_test_key(_TMP))

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi import HTTPException  # noqa: E402
from pydantic import ValidationError  # noqa: E402

from app.database import Base, SessionLocal, engine  # noqa: E402
from app.models import AdminAuditLog, CardDevice, CardKey, SessionToken  # noqa: E402
from app.routers.admin import batch_cards, update_card  # noqa: E402
from app.schemas import CardBatchRequest, CardUpdate  # noqa: E402
from app.security import utcnow  # noqa: E402
from app.services.auth_service import (  # noqa: E402
    activate_if_needed,
    ensure_card_usable,
    login_card,
)

Base.metadata.create_all(bind=engine)


def _card(db, **kw):
    now = utcnow()
    defaults = dict(
        key_code=kw.pop("key_code", "TEST-" + str(now.timestamp())),
        scope="global",
        game_id=None,
        duration_type="day",
        max_uses=0,
        used_count=0,
        status="unused",
        max_devices=1,
        note=None,
    )
    defaults.update(kw)
    c = CardKey(**defaults)
    db.add(c)
    db.commit()
    db.refresh(c)
    return c


def _aware(dt):
    """SQLite 读回是 naive；比较前统一按 UTC 补齐（生产 Postgres 本来就是 aware）。"""
    if dt is None:
        return None
    return dt.replace(tzinfo=timezone.utc) if dt.tzinfo is None else dt


def _at(days: int, *, microsecond: int = 0) -> datetime:
    """相对「现在」的 UTC 时刻（微秒默认归零）。

    2026-11-21：本文件原先写死 2026-11/12 的具体日期，跑到该日期之后自然失效
    （表现为「卡密已过期」）。改为相对时钟的偏移后，**只动测试数据**，
    生产侧 activate_if_needed / ensure_card_usable / login_card 一行未改。
    """
    return (utcnow() + timedelta(days=days)).replace(microsecond=microsecond)


class ActivationPolicyTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()

    def tearDown(self):
        self.db.close()

    def test_preset_expire_survives_first_activation(self):
        """有用未用 day 卡：后台把到期时间设到未来，首次登录不得被换成 now+1d。"""
        preset = _at(3)
        c = _card(self.db, status="unused", duration_type="day", expire_at=preset)
        self.assertTrue(activate_if_needed(c))
        self.assertEqual(_aware(c.expire_at), preset)
        self.assertIsNotNone(c.activated_at)
        self.assertEqual(c.status, "active")

    def test_no_expire_uses_default_duration(self):
        """未设置到期时间的 day 卡：首次激活仍按卡种 +24h。"""
        c = _card(self.db, status="unused", duration_type="day", expire_at=None)
        before = utcnow()
        self.assertTrue(activate_if_needed(c))
        delta = _aware(c.expire_at) - before
        self.assertGreaterEqual(delta, timedelta(hours=24) - timedelta(seconds=5))
        self.assertLessEqual(delta, timedelta(hours=24) + timedelta(seconds=5))

    def test_reset_to_unused_does_not_shrink_extended_card(self):
        """已延期卡被后台改回 unused（activated_at 保留）：激活不得缩时。"""
        far = _at(60)
        c = _card(self.db, status="unused", duration_type="day",
                  expire_at=far, activated_at=_at(-30))
        self.assertTrue(activate_if_needed(c))
        self.assertEqual(_aware(c.expire_at), far)

    def test_past_expire_is_not_renewed(self):
        """手设过去时间：激活不得延长，且激活后仍判过期（抛 403）。"""
        past = _at(-30)
        c = _card(self.db, status="unused", duration_type="day", expire_at=past)
        with self.assertRaises(HTTPException) as ctx:
            activate_if_needed(c)
        self.assertEqual(ctx.exception.status_code, 403)
        self.assertEqual(_aware(c.expire_at), past)

    def test_past_expire_login_writes_nothing(self):
        """登录被拒时不得留半改状态：status/expire 不变，无 session/设备行。"""
        past = _at(-30)
        c = _card(self.db, status="unused", duration_type="day", expire_at=past,
                  key_code="PAST-ZERO-WRITE")
        before_status, before_exp = c.status, _aware(c.expire_at)
        with self.assertRaises(HTTPException) as ctx:
            # 2026-11-15：设备号改为客户端真实形态（dev_+32 位十六进制），
            # 服务端已收紧为精确形态校验；旧写法 "dev_zero_write" 不再被接受。
            login_card(self.db, "PAST-ZERO-WRITE", "any_game", "dev_" + "0" * 32)
        self.assertEqual(ctx.exception.status_code, 403)
        self.db.rollback()
        self.db.refresh(c)
        self.assertEqual(c.status, before_status)
        self.assertEqual(_aware(c.expire_at), before_exp)
        self.assertEqual(self.db.query(SessionToken).filter_by(card_id=c.id).count(), 0)
        self.assertEqual(self.db.query(CardDevice).filter_by(card_id=c.id).count(), 0)

    def test_login_keeps_preset_expire_and_issues_token(self):
        """unused + 未来手设到期：首登保留到期时间并发会话 token。"""
        preset = _at(3)
        c = _card(self.db, status="unused", duration_type="day", expire_at=preset,
                  key_code="PRESET-OK")
        out = login_card(self.db, "PRESET-OK", "any_game", "dev_" + "1" * 32)
        self.db.refresh(c)
        self.assertEqual(_aware(c.expire_at), preset)
        self.assertEqual(c.status, "active")
        self.assertTrue(out.get("token"))
        self.assertEqual(self.db.query(SessionToken).filter_by(card_id=c.id).count(), 1)

    def test_active_without_activated_at_is_filled_not_rewritten(self):
        """历史脏数据：status=active 但 activated_at 为空 → 补 activated_at，不动已有到期。"""
        preset = _at(20)
        c = _card(self.db, status="active", duration_type="day", expire_at=preset,
                  activated_at=None)
        self.assertTrue(activate_if_needed(c))
        self.assertIsNotNone(c.activated_at)
        self.assertEqual(_aware(c.expire_at), preset)

    def test_expired_card_rejected_before_activation(self):
        """已过期卡走 ensure_card_usable 先被拒，激活根本不会执行（顺序保证）。"""
        past = _at(-30)
        c = _card(self.db, status="active", duration_type="day",
                  expire_at=past, activated_at=_at(-60))
        with self.assertRaises(HTTPException) as ctx:
            ensure_card_usable(c, "any_game")
        self.assertEqual(ctx.exception.status_code, 403)
        self.assertEqual(c.status, "expired")
        self.assertEqual(_aware(c.expire_at), past)

    def test_permanent_and_count_stay_without_expire(self):
        for dtype in ("permanent", "count"):
            c = _card(self.db, status="unused", duration_type=dtype, expire_at=None,
                      max_uses=5 if dtype == "count" else 0)
            self.assertTrue(activate_if_needed(c))
            self.assertIsNone(c.expire_at)

    def test_permanent_with_explicit_expire_is_preserved(self):
        """permanent/count 卡若被后台显式设了到期时间，激活不得清成 NULL（保留用户明确设置）。"""
        preset = _at(40)
        for dtype in ("permanent", "count"):
            c = _card(self.db, status="unused", duration_type=dtype, expire_at=preset,
                      max_uses=3 if dtype == "count" else 0)
            self.assertTrue(activate_if_needed(c))
            self.assertEqual(_aware(c.expire_at), preset)

    def test_banned_not_revived(self):
        c = _card(self.db, status="banned", duration_type="day", expire_at=None)
        self.assertFalse(activate_if_needed(c))
        self.assertEqual(c.status, "banned")
        self.assertIsNone(c.expire_at)


class AdminAuditTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        self.req = SimpleNamespace(client=SimpleNamespace(host="127.0.0.1"), headers={})

    def tearDown(self):
        self.db.close()

    def _last_audit(self, action):
        return (
            self.db.query(AdminAuditLog)
            .filter(AdminAuditLog.action == action)
            .order_by(AdminAuditLog.id.desc())
            .first()
        )

    def test_update_records_old_and_new_expire(self):
        old_exp = _at(2)
        c = _card(self.db, status="active", duration_type="day",
                  expire_at=old_exp,
                  activated_at=_at(-1))
        new_exp = _at(10)
        update_card(c.id, CardUpdate(expire_at=new_exp), self.req, {"sub": "tester"}, self.db)
        row = self._last_audit("cards.update")
        self.assertIn(old_exp.strftime("%Y-%m-%dT%H:%M:%S"), row.detail)
        self.assertIn("->" + new_exp.strftime("%Y-%m-%dT%H:%M:%S"), row.detail)
        self.assertIn("来源=admin.cards.update", row.detail)

    def test_note_only_patch_keeps_expire_untouched(self):
        exact = _at(3, microsecond=123456)
        c = _card(self.db, status="active", duration_type="day", expire_at=exact,
                  activated_at=_at(-1))
        update_card(c.id, CardUpdate(note="只改备注"), self.req, {"sub": "tester"}, self.db)
        self.db.refresh(c)
        self.assertEqual(_aware(c.expire_at), exact)
        row = self._last_audit("cards.update")
        self.assertNotIn("到期=", row.detail)

    def test_clearing_expire_sets_null_and_audits(self):
        c = _card(self.db, status="active", duration_type="day",
                  expire_at=_at(2),
                  activated_at=_at(-1))
        update_card(c.id, CardUpdate(expire_at=None), self.req, {"sub": "tester"}, self.db)
        self.db.refresh(c)
        self.assertIsNone(c.expire_at)
        row = self._last_audit("cards.update")
        self.assertIn("->无", row.detail)

    def test_batch_zero_days_keeps_expire(self):
        """extend_days=0 + status：只改状态，到期时间不动。"""
        exact = _at(2, microsecond=123456)
        c = _card(self.db, status="active", duration_type="day", expire_at=exact,
                  activated_at=_at(-1))
        batch_cards(CardBatchRequest(ids=[c.id], extend_days=0, status="banned"),
                    self.req, {"sub": "tester"}, self.db)
        self.db.refresh(c)
        self.assertEqual(c.status, "banned")
        self.assertEqual(_aware(c.expire_at), exact)

    def test_batch_extend_audits_change(self):
        c = _card(self.db, status="active", duration_type="day", expire_at=None,
                  activated_at=_at(-1))
        batch_cards(CardBatchRequest(ids=[c.id], extend_days=3), self.req, {"sub": "tester"}, self.db)
        self.db.refresh(c)
        self.assertIsNotNone(c.expire_at)
        row = self._last_audit("cards.batch")
        self.assertIn("到期变更=1张", row.detail)
        self.assertIn("ID=", row.detail)
        self.assertIn("->", row.detail)


class ExtendDaysValidationTest(unittest.TestCase):
    def test_negative_rejected(self):
        with self.assertRaises(ValidationError):
            CardBatchRequest(ids=[1], extend_days=-1)

    def test_non_finite_rejected(self):
        for bad in (float("inf"), float("-inf"), float("nan")):
            with self.assertRaises(ValidationError):
                CardBatchRequest(ids=[1], extend_days=bad)

    def test_zero_and_none_and_positive_ok(self):
        self.assertEqual(CardBatchRequest(ids=[1], extend_days=0).extend_days, 0)
        self.assertIsNone(CardBatchRequest(ids=[1]).extend_days)
        self.assertEqual(CardBatchRequest(ids=[1], extend_days=1.5).extend_days, 1.5)
        # 不设人为上限：有限非负数一律放行（历史调用方可能传大天数）
        self.assertEqual(CardBatchRequest(ids=[1], extend_days=100000).extend_days, 100000)


class BatchOverflowTest(unittest.TestCase):
    """极大延期参数不得变成 500；越界必须在整批任何写入前 422。"""

    def setUp(self):
        self.db = SessionLocal()
        self.req = SimpleNamespace(client=SimpleNamespace(host="127.0.0.1"), headers={})

    def tearDown(self):
        self.db.close()

    def _batch_audit_count(self):
        return self.db.query(AdminAuditLog).filter_by(action="cards.batch").count()

    def _assert_rejected_and_untouched(self, card, days):
        before_exp = _aware(card.expire_at)
        before_status = card.status
        audits = self._batch_audit_count()
        with self.assertRaises(HTTPException) as ctx:
            batch_cards(CardBatchRequest(ids=[card.id], extend_days=days),
                        self.req, {"sub": "tester"}, self.db)
        self.assertEqual(ctx.exception.status_code, 422)
        self.db.rollback()
        self.db.refresh(card)
        self.assertEqual(_aware(card.expire_at), before_exp)
        self.assertEqual(card.status, before_status)
        self.assertEqual(self._batch_audit_count(), audits, "失败批次不得留下审计")

    def test_1e7_days_rejected_422(self):
        c = _card(self.db, status="active", duration_type="day", expire_at=None,
                  activated_at=_at(-1))
        self._assert_rejected_and_untouched(c, 1e7)

    def test_1e100_days_rejected_422(self):
        c = _card(self.db, status="active", duration_type="day", expire_at=None,
                  activated_at=_at(-1))
        self._assert_rejected_and_untouched(c, 1e100)

    def test_near_datetime_max_plus_one_day_rejected(self):
        c = _card(self.db, status="active", duration_type="day",
                  expire_at=datetime(9999, 12, 31, 0, 0, 0, tzinfo=timezone.utc),
                  activated_at=_at(-1))
        self._assert_rejected_and_untouched(c, 1)

    def test_mixed_batch_all_or_nothing(self):
        """只有后项越界时，前项也不能被延期；整批零写入、零审计。"""
        ok = _card(self.db, status="active", duration_type="day", expire_at=None,
                   activated_at=_at(-1))
        near_max = _card(self.db, status="active", duration_type="day",
                         expire_at=datetime(9999, 12, 31, 0, 0, 0, tzinfo=timezone.utc),
                         activated_at=_at(-1))
        ok_exp = _aware(ok.expire_at)
        audits = self._batch_audit_count()
        with self.assertRaises(HTTPException) as ctx:
            batch_cards(CardBatchRequest(ids=[ok.id, near_max.id], extend_days=1),
                        self.req, {"sub": "tester"}, self.db)
        self.assertEqual(ctx.exception.status_code, 422)
        self.db.rollback()
        self.db.refresh(ok)
        self.assertIsNone(ok.expire_at)
        self.assertEqual(_aware(ok.expire_at), ok_exp)
        self.assertEqual(self._batch_audit_count(), audits)

    def test_normal_extend_still_works(self):
        c = _card(self.db, status="active", duration_type="day", expire_at=None,
                  activated_at=_at(-1))
        batch_cards(CardBatchRequest(ids=[c.id], extend_days=3),
                    self.req, {"sub": "tester"}, self.db)
        self.db.refresh(c)
        self.assertIsNotNone(c.expire_at)


if __name__ == "__main__":
    unittest.main(verbosity=2)
