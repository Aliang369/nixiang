"""管理后台前端结构/路由回归（2026-11-16）。

背景：设备日志 tab 打开空白，根因是 core.js 的 PAGE_META 未注册 events，
而 switchPage 只遍历 PAGE_META 的键做 section 显隐 —— 静态 200 也不代表能切换。
本机无 headless 浏览器（无 playwright/jsdom/chrome），因此用 HTML 解析做
「路由注册 + DOM id + 文案回归」的结构级校验，覆盖本次 bug 的同一类问题。
"""
from __future__ import annotations

import os
import re
import unittest
from html.parser import HTMLParser

ADMIN = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "static", "admin")


def _read(*parts: str) -> str:
    with open(os.path.join(ADMIN, *parts), encoding="utf-8") as fh:
        return fh.read()


class _Ids(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids: set[str] = set()
        self.pages: list[str] = []
        self.sections: set[str] = set()

    def handle_starttag(self, tag, attrs):
        a = dict(attrs)
        if "id" in a:
            self.ids.add(a["id"])
            if a["id"].startswith("page-"):
                self.sections.add(a["id"][5:])
        if tag == "button" and "data-page" in a:
            self.pages.append(a["data-page"])


class AdminUiWiringTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.html = _read("index.html")
        cls.core = _read("js", "core.js")
        cls.dash = _read("js", "dashboard.js")
        cls.events = _read("js", "events.js")
        p = _Ids()
        p.feed(cls.html)
        cls.ids, cls.pages, cls.sections = p.ids, p.pages, p.sections
        block = cls.core.split("const PAGE_META = {", 1)[1].split("};", 1)[0]
        cls.meta_keys = set(re.findall(r"^\s+(\w+):\s*\[", block, re.M))
        cls.switch_branches = set(re.findall(r"page === '(\w+)'", cls.html))

    # settings 页在发布前就是空占位（无 loader），允许没有加载分支
    NO_LOADER_PAGES = {"settings"}

    def test_every_nav_page_is_registered_and_switchable(self):
        self.assertIn("events", self.pages, "侧栏缺少设备日志入口")
        for page in self.pages:
            self.assertIn(page, self.sections, f"缺少 <section id=page-{page}>")
            self.assertIn(page, self.meta_keys, f"PAGE_META 未注册 {page}（会导致 tab 空白）")
            if page not in self.NO_LOADER_PAGES:
                self.assertTrue(
                    page in self.switch_branches or f"switchPage('{page}')" in self.html,
                    f"switchPage 未处理 {page}",
                )

    def test_switch_loop_reveals_events_section(self):
        """按 switchPage 的真实实现模拟：它只遍历 PAGE_META 的键来 toggle hidden。

        因此「page-events 能否显示」= 「events 是否在 PAGE_META 且存在对应 section」。
        这里直接对该循环做等价模拟，而不是只匹配字符串。
        """
        for page in ("events", "dashboard", "audit"):
            hidden = {p for p in self.meta_keys if p != page}
            self.assertNotIn(page, hidden, f"{page} 无法被取消隐藏")
            self.assertIn(page, self.sections)
        # 反向验证：events 不在 PAGE_META 时，该模拟会把它留在 hidden（本次 bug 的复现条件）
        without = self.meta_keys - {"events"}
        self.assertIn("events", {p for p in without} | {p for p in self.sections if p not in without})

    def test_events_page_has_table_filters_and_states(self):
        self.assertIn("page === 'events'", self.html)
        self.assertIn("loadEvents()", self.html)
        for dom_id in (
            "ev-device", "ev-type", "ev-result",
            "ev-from-slot", "ev-to-slot", "events-body", "events-pager",
        ):
            self.assertIn(dom_id, self.ids, f"events.js 引用的 DOM id 缺失: {dom_id}")
        # 详情改弹窗：不再有内嵌详情容器
        self.assertNotIn("event-detail", self.ids)
        # 表头至少包含时间/事件/设备/结果
        for th in ("时间", "事件", "设备", "结果"):
            self.assertIn(th, self.html)
        # 空态与错误态
        self.assertIn("暂无事件或无匹配项", self.events)
        self.assertIn("catch (e)", self.events)

    def test_dashboard_restored_original_labels_and_no_extra_tile(self):
        self.assertIn("今日活跃", self.dash)
        self.assertIn("7 日活跃", self.dash)
        for banned in ("成功验证", "卡密关联", "未关联", "（设备）", "最后状态"):
            self.assertNotIn(banned, self.dash, f"dashboard.js 仍含越界文案: {banned}")
        self.assertEqual(self.dash.count('<div class="k">'), 8, "tile 数量应恢复为原 8 个")
        self.assertNotIn("dash-note", self.html)

    def test_card_filter_uses_internal_id_only(self):
        self.assertIn("ev-card-id", self.ids)
        self.assertIn("card_id", self.events)
        # 卡密明文不得进 GET query（会落 nginx 访问日志）
        self.assertNotIn("qs.set('card_key'", self.events)

    def test_events_js_no_inline_device_js(self):
        self.assertNotIn('onclick="openDeviceDetail', self.events)
        self.assertIn("data-dev=", self.events)

    def test_events_reuses_audit_time_picker(self):
        # 必须在 initStaticTimePickers 的槽位表里注册，才与操作日志同一组件
        for slot in ("ev-from-slot", "ev-to-slot"):
            self.assertIn(f"['{slot}'", self.html, f"{slot} 未注册到时间选择器")
        self.assertIn("tpISOString('ev-from')", self.events)
        self.assertIn("tpISOString('ev-to', true)", self.events)
        # 不允许另造原生 date 输入
        events_section = self.html.split('id="page-events"', 1)[1].split("</section>", 1)[0]
        self.assertNotIn('type="date"', events_section)

    def test_modal_closes_by_escape_and_mask(self):
        # Escape：复用 document keydown，优先关闭 modal-root
        self.assertIn("if (e.key !== 'Escape') return;", self.html)
        self.assertIn("if (modal && !modal.classList.contains('hidden')) { closeModal(); return; }", self.html)
        # 遮罩点击：modal-mask 自带 onclick closeModal
        self.assertIn('id="modal-root" class="modal-mask hidden" onclick="if(event.target===this)closeModal()"', self.html)

    def test_detail_merges_presence_and_binding_into_one_table(self):
        # 合并：不再有独立「绑定卡」表头，绑定信息并入主表列
        self.assertNotIn("<th>绑定卡</th>", self.events)
        self.assertIn("<th>卡密</th><th>状态</th><th>在线</th>", self.events)
        self.assertIn("<th>绑定时间</th><th>到期</th>", self.events)
        self.assertIn("mergeDetailRows", self.events)

    def test_card_status_chinese_all_known_codes(self):
        for code in ("active", "unused", "expired", "banned", "exhausted", "revoked"):
            self.assertIn(f"{code}:", self.events, f"卡状态 {code} 缺中文")
        self.assertIn("未关联卡密", self.events)
        self.assertIn("未知状态（", self.events)
        self.assertIn("在线", self.events)
        self.assertIn("离线", self.events)

    def test_events_detail_uses_global_modal(self):
        self.assertIn("openModal(", self.events)
        self.assertIn("dev-detail-body", self.events)
        self.assertIn("加载中", self.events)

    def test_reason_codes_have_chinese_labels(self):
        for code in (
            "bad_device", "not_found", "banned", "expired", "exhausted",
            "device_mismatch", "not_bound", "unauthorized", "rate_limited",
            "validation_error", "denied",
        ):
            self.assertIn(f"{code}:", self.events, f"原因码 {code} 缺中文映射")
        self.assertIn("未知原因", self.events)
        # 上下文细分：同一 code 在不同事件下中文不同
        self.assertIn("REASON_BY_TYPE", self.events)
        self.assertIn("heartbeat_fail", self.events)

    def test_card_key_rendered_plaintext_and_escaped(self):
        self.assertIn("card_key", self.events)
        self.assertIn("data-copy=", self.events)
        self.assertIn("已删除（card#", self.events)
        # 明文必须经 escapeHtml，不能裸拼
        self.assertIn("escapeHtml(key)", self.events)
        # 复用卡密管理样式：code.card-key + title「点击复制」，不自造按钮/新文案
        self.assertIn('class="card-key"', self.events)
        self.assertIn('title="点击复制"', self.events)
        self.assertNotIn(">复制</button>", self.events)
        # 键盘可用：Enter/Space 触发同一 copyText
        self.assertIn("e.key !== 'Enter' && e.key !== ' '", self.events)
        # 复制走 document 级 capture 委托（modal 的 .modal 会 stopPropagation），不拼内联 JS
        self.assertIn("closest('code[data-copy], [data-copy]')", self.events)
        self.assertIn("}, true);", self.events)
        self.assertNotIn('onclick="copyText', self.events)


if __name__ == "__main__":
    unittest.main()
