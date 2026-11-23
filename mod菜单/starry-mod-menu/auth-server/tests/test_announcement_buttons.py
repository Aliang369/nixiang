"""公告三按钮字段（2026-11-23）：后台入参 → bootstrap 下发 → 不配置时保持旧行为。

只跑本地临时 SQLite，不连生产、不读写 .env。
覆盖：
- 默认值：exit_enabled=False、exit_label/close_label=None（旧包/旧公告表现不变）
- 三按钮配置原样下发（exit_enabled / exit_label / close_label 与旧字段共存）
- 入参限长与部分更新语义（Update 只改传入字段）
"""
from __future__ import annotations

import os
import sys
import tempfile
import unittest

_TMP = tempfile.mkdtemp(prefix="ann-buttons-test-")
os.environ.setdefault("DATABASE_URL", "sqlite:///%s/t.db" % _TMP)
os.environ.setdefault("SECRET_KEY", "test-secret")

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from app.database import Base, SessionLocal, engine  # noqa: E402
from app.models import Announcement  # noqa: E402
from app.schemas import AnnouncementCreate, AnnouncementUpdate  # noqa: E402
from app.services.auth_service import build_bootstrap  # noqa: E402


class AnnouncementButtonsTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        Base.metadata.create_all(engine)

    def setUp(self):
        self.db = SessionLocal()
        self.db.query(Announcement).delete()
        self.db.commit()

    def tearDown(self):
        self.db.close()

    def _announcements(self):
        return build_bootstrap(self.db, "starry_self", logged_in=False)["announcements"]

    def test_defaults_keep_old_behavior(self):
        self.db.add(Announcement(title="t", content="c", action_url="https://example.com"))
        self.db.commit()
        a = self._announcements()[0]
        self.assertFalse(a["exit_enabled"])
        self.assertIsNone(a["exit_label"])
        self.assertIsNone(a["close_label"])
        # 旧字段不受影响
        self.assertEqual(a["action_url"], "https://example.com")
        self.assertEqual(a["popup_mode"], "every_open")

    def test_three_buttons_passthrough(self):
        self.db.add(
            Announcement(
                title="t",
                content="c",
                action_url="https://example.com",
                action_label="去官网",
                exit_enabled=True,
                exit_label="退出",
                close_label="进游戏",
            )
        )
        self.db.commit()
        a = self._announcements()[0]
        self.assertTrue(a["exit_enabled"])
        self.assertEqual(a["exit_label"], "退出")
        self.assertEqual(a["close_label"], "进游戏")
        self.assertEqual(a["action_label"], "去官网")

    def test_schema_defaults_and_limits(self):
        created = AnnouncementCreate(title="t", content="c")
        self.assertFalse(created.exit_enabled)
        self.assertIsNone(created.exit_label)
        self.assertIsNone(created.close_label)

        with self.assertRaises(Exception):
            AnnouncementCreate(title="t", content="c", exit_label="x" * 33)

        upd = AnnouncementUpdate(exit_enabled=False)
        self.assertEqual(upd.model_dump(exclude_unset=True), {"exit_enabled": False})

    def test_closed_label_only(self):
        """只配关闭按钮文案（不配退出、不填链接）：客户端应显示单按钮自定义文案。"""
        self.db.add(Announcement(title="t", content="c", close_label="进入游戏"))
        self.db.commit()
        a = self._announcements()[0]
        self.assertFalse(a["exit_enabled"])
        self.assertIsNone(a["action_url"])
        self.assertEqual(a["close_label"], "进入游戏")


if __name__ == "__main__":
    unittest.main()
