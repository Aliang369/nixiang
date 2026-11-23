"""v2-only 授权票契约与 fail-closed 回归（2026-11-21 票据切换）。

只跑本地临时 SQLite + 临时测试私钥（tests/_grant_testkey.py 现生成），
不连生产、不读 ~/.starry-keys、不打印任何密钥内容、不构造旧票。

覆盖：
- login / heartbeat / 开放模式 bootstrap 的响应只含 grant2，不含 grant / grant3
- grant2 为 2|device|game|exp|mask|kid 六段，服务端 verify_grant_v2 可验
- 缺签名私钥 / 签名异常 → HTTP 503 且文案指明「授权票签发失败」（fail closed）
- grant_service 不再暴露 v1/v3 的签发与验证入口
"""
from __future__ import annotations

import os
import sys
import tempfile
import uuid
import unittest
from unittest import mock

_TMP = tempfile.mkdtemp(prefix="grant-v2-test-")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from _grant_testkey import make_test_key  # noqa: E402

os.environ.setdefault("DATABASE_URL", "sqlite:///%s/t.db" % _TMP)
os.environ.setdefault("SECRET_KEY", "test-secret")
os.environ["GRANT_RSA_KEY_PATH"] = make_test_key(_TMP)
os.environ.setdefault("PAYLOAD_MASTER_PATH", "")
os.environ.setdefault("PAYLOAD_SHA", "")

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi import HTTPException  # noqa: E402

from app.config import get_settings  # noqa: E402
from app.database import Base, SessionLocal, engine  # noqa: E402
from app.models import (  # noqa: E402
    CardDevice,
    CardKey,
    FeatureFlag,
    Game,
    OnlinePresence,
    SessionToken,
)
from app.services import grant_service as gs  # noqa: E402
from app.services.auth_service import (  # noqa: E402
    bootstrap_with_grant,
    heartbeat,
    login_card,
)

Base.metadata.create_all(bind=engine)

# 503 对外只允许这一句通用文案，不得拼异常细节
GRANT_ERR = "授权票签发失败，请稍后重试"


def _card(db, key_code, **kw):
    defaults = dict(
        key_code=key_code,
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


class GrantV2OnlyTest(unittest.TestCase):
    def setUp(self):
        self.db = SessionLocal()
        # 2026-11-23：掩码改为「只认后台登记位」后，游戏必须有功能记录才签得出有效票
        if not self.db.query(Game).filter(Game.game_id == "g_v2").first():
            self.db.add(Game(game_id="g_v2", name="v2", card_auth_enabled=True, enabled=True))
        if not self.db.query(FeatureFlag).filter_by(game_id="g_v2", feat_num=1).first():
            self.db.add(FeatureFlag(game_id="g_v2", feat_num=1, name="f1", enabled=True))
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def _login(self, key_code, device_id, game_id="g_v2"):
        _card(self.db, key_code)
        return login_card(self.db, key_code, game_id, device_id)

    def test_login_response_is_v2_only(self):
        out = self._login("V2-LOGIN", "dev_11111111111111111111111111111111")
        self.assertIn("grant2", out)
        self.assertNotIn("grant", out)
        self.assertNotIn("grant3", out)
        grant = out["grant2"]
        parts = grant.rsplit(".", 1)[0].split("|")
        self.assertEqual(parts[0], "2")
        self.assertEqual(len(parts), 6)
        self.assertEqual(parts[1], "dev_11111111111111111111111111111111")
        self.assertEqual(parts[2], "g_v2")
        self.assertGreater(int(parts[4]), 0)
        verified = gs.verify_grant_v2(grant, device_id="dev_11111111111111111111111111111111", game_id="g_v2")
        self.assertIsNotNone(verified)
        self.assertEqual(verified["kid"], parts[5])

    def test_heartbeat_response_is_v2_only(self):
        out = self._login("V2-HB", "dev_22222222222222222222222222222222")
        hb = heartbeat(self.db, out["token"], "g_v2", "dev_22222222222222222222222222222222")
        self.assertIn("grant2", hb)
        self.assertNotIn("grant", hb)
        self.assertNotIn("grant3", hb)
        self.assertEqual(hb["grant2"].split("|", 2)[0], "2")

    def test_bootstrap_open_mode_is_v2_only(self):
        self.db.add(Game(game_id="g_open", name="open", card_auth_enabled=False, enabled=True))
        self.db.add(FeatureFlag(game_id="g_open", feat_num=1, name="f1", enabled=True))
        self.db.commit()
        boot = bootstrap_with_grant(self.db, "g_open", device_id="dev_33333333333333333333333333333333")
        self.assertIn("grant2", boot)
        self.assertNotIn("grant", boot)
        self.assertNotIn("grant3", boot)
        self.assertEqual(boot["grant2"].split("|", 2)[0], "2")

    def test_missing_private_key_fails_closed(self):
        _card(self.db, "V2-NOKEY")
        settings = get_settings()
        old = settings.grant_rsa_key_path
        settings.grant_rsa_key_path = ""
        try:
            with self.assertRaises(HTTPException) as ctx:
                login_card(self.db, "V2-NOKEY", "g_v2", "dev_44444444444444444444444444444444")
        finally:
            settings.grant_rsa_key_path = old
        self.assertEqual(ctx.exception.status_code, 503)
        self.assertEqual(ctx.exception.detail, GRANT_ERR)
        # fail closed：既不发票，也不留会话/绑定/presence
        self.db.expire_all()
        self.assertEqual(
            self.db.query(SessionToken).filter(SessionToken.device_id == "dev_44444444444444444444444444444444").count(),
            0,
        )
        self.assertEqual(
            self.db.query(CardDevice).filter(CardDevice.device_id == "dev_44444444444444444444444444444444").count(),
            0,
        )
        self.assertEqual(
            self.db.query(OnlinePresence).filter(OnlinePresence.device_id == "dev_44444444444444444444444444444444").count(),
            0,
        )

    def test_signing_error_fails_closed(self):
        _card(self.db, "V2-SIGNFAIL")
        with mock.patch.object(gs.sg_rsa, "sign_hex", side_effect=RuntimeError("boom")):
            with self.assertRaises(HTTPException) as ctx:
                login_card(self.db, "V2-SIGNFAIL", "g_v2", "dev_55555555555555555555555555555555")
        self.assertEqual(ctx.exception.status_code, 503)
        # 对外通用文案：不拼异常文本、不泄露配置键名
        self.assertEqual(ctx.exception.detail, GRANT_ERR)
        self.assertNotIn("boom", ctx.exception.detail)
        self.assertNotIn("grant_rsa_key_path", ctx.exception.detail)
        # 签发在 commit 之前 ⇒ 失败不落任何新状态
        self.db.expire_all()
        self.assertEqual(
            self.db.query(SessionToken).filter(SessionToken.device_id == "dev_55555555555555555555555555555555").count(),
            0,
        )
        self.assertEqual(
            self.db.query(CardDevice).filter(CardDevice.device_id == "dev_55555555555555555555555555555555").count(),
            0,
        )
        self.assertEqual(
            self.db.query(OnlinePresence).filter(OnlinePresence.device_id == "dev_55555555555555555555555555555555").count(),
            0,
        )
        card = self.db.query(CardKey).filter(CardKey.key_code == "V2-SIGNFAIL").first()
        self.assertEqual(card.status, "unused")
        self.assertIsNone(card.activated_at)
        self.assertIsNone(card.expire_at)

    def _presence(self, game_id, device_id):
        return (
            self.db.query(OnlinePresence)
            .filter(OnlinePresence.game_id == game_id, OnlinePresence.device_id == device_id)
            .first()
        )

    def test_heartbeat_sign_failure_does_not_extend_session(self):
        out = self._login("V2-HBFAIL", "dev_66666666666666666666666666666666")
        token = out["token"]
        sess = self.db.query(SessionToken).filter(SessionToken.token == token).first()
        before = (sess.expires_at, sess.last_seen_at)
        pres_before = self._presence("g_v2", "dev_66666666666666666666666666666666").last_seen_at
        with mock.patch.object(gs.sg_rsa, "sign_hex", side_effect=RuntimeError("boom")):
            with self.assertRaises(HTTPException) as ctx:
                heartbeat(self.db, token, "g_v2", "dev_66666666666666666666666666666666")
        self.assertEqual(ctx.exception.status_code, 503)
        self.assertEqual(ctx.exception.detail, GRANT_ERR)
        self.db.expire_all()
        sess = self.db.query(SessionToken).filter(SessionToken.token == token).first()
        self.assertEqual((sess.expires_at, sess.last_seen_at), before)
        self.assertFalse(sess.revoked)
        self.assertEqual(self._presence("g_v2", "dev_66666666666666666666666666666666").last_seen_at, pres_before)

    def test_open_bootstrap_sign_failure_keeps_presence(self):
        self.db.add(Game(game_id="g_open2", name="open2", card_auth_enabled=False, enabled=True))
        self.db.commit()
        self.assertIn("grant2", bootstrap_with_grant(self.db, "g_open2", device_id="dev_77777777777777777777777777777777"))
        pres_before = self._presence("g_open2", "dev_77777777777777777777777777777777").last_seen_at
        with mock.patch.object(gs.sg_rsa, "sign_hex", side_effect=RuntimeError("boom")):
            with self.assertRaises(HTTPException) as ctx:
                bootstrap_with_grant(self.db, "g_open2", device_id="dev_77777777777777777777777777777777")
        self.assertEqual(ctx.exception.status_code, 503)
        self.assertEqual(ctx.exception.detail, GRANT_ERR)
        self.db.expire_all()
        self.assertEqual(self._presence("g_open2", "dev_77777777777777777777777777777777").last_seen_at, pres_before)

    def test_legacy_entrypoints_removed(self):
        for name in ("mint_grant", "verify_grant", "mint_grant_v3", "payload_wrap_key"):
            self.assertFalse(hasattr(gs, name), "%s 应已移除" % name)
        self.assertFalse(hasattr(gs, "GRANT_VERSION"))
        self.assertTrue(hasattr(gs, "mint_grant_v2"))
        self.assertTrue(hasattr(gs, "GrantSigningError"))


if __name__ == "__main__":
    unittest.main(verbosity=2)


class OpenModeUnregisteredTest(unittest.TestCase):
    """2026-11-23（lead 定）：未注册 game_id 不放行；掩码只认后台登记位。

    依据：用户 2026-11-23「功能开关必须由服务器下发……拿不到服务器授权就不放行」
    「未注册 game_id 就不能使用功能」。
    """

    def setUp(self):
        self.db = SessionLocal()

    def tearDown(self):
        self.db.close()

    def test_unregistered_game_gets_no_features_and_no_grant(self):
        gid = "unreg_" + uuid.uuid4().hex[:6]
        boot = bootstrap_with_grant(self.db, gid, device_id="dev_" + "6" * 32)
        self.assertFalse(boot["card_auth_enabled"], "未注册按关卡密处理（隐藏账号 Tab）")
        self.assertFalse(boot["menu_enabled"], "未注册不得放行菜单功能")
        self.assertEqual(boot["features"], [])
        self.assertNotIn("grant2", boot, "未注册不签票：没有票客户端就锁死功能")

    def test_registered_but_no_feature_rows_masks_zero(self):
        gid = "nofeat_" + uuid.uuid4().hex[:6]
        self.db.add(Game(game_id=gid, name=gid, card_auth_enabled=False, enabled=True))
        self.db.commit()
        boot = bootstrap_with_grant(self.db, gid, device_id="dev_" + "7" * 32)
        self.assertIn("grant2", boot)
        mask = int(boot["grant2"].rsplit(".", 1)[0].split("|")[4])
        self.assertEqual(mask, 0, "后台一条功能记录都没有 ⇒ 掩码 0（票无效，功能不可用）")

    def test_mask_only_contains_enabled_registered_bits(self):
        gid = "mask_" + uuid.uuid4().hex[:6]
        self.db.add(Game(game_id=gid, name=gid, card_auth_enabled=False, enabled=True))
        self.db.add_all([
            FeatureFlag(game_id=gid, feat_num=1, name="a", enabled=True),
            FeatureFlag(game_id=gid, feat_num=3, name="b", enabled=False),
        ])
        self.db.commit()
        boot = bootstrap_with_grant(self.db, gid, device_id="dev_" + "8" * 32)
        mask = int(boot["grant2"].rsplit(".", 1)[0].split("|")[4])
        self.assertEqual(mask, 1 << 1, "只保留后台启用的位；停用位与未登记位一律 0")
