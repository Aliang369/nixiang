from __future__ import annotations

from datetime import datetime
from typing import Optional

from sqlalchemy import Boolean, DateTime, Integer, String, Text, UniqueConstraint, func, text
from sqlalchemy.orm import Mapped, mapped_column

from .database import Base


class Game(Base):
    __tablename__ = "games"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    game_id: Mapped[str] = mapped_column(String(64), unique=True, index=True)
    name: Mapped[str] = mapped_column(String(128))
    # 菜单模板 id，对应 menu_templates.template_id；用于一键同步功能门控
    menu_template: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    enabled: Mapped[bool] = mapped_column(Boolean, default=True)
    # True=客户端需卡密登录并显示账号 Tab；False=隐藏账号 Tab（默认关卡密），功能仍由服务端下发
    # 2026-11-23（lead 定）：默认 False —— 后台新建游戏默认关卡密，只有显式打开才要求卡密登录
    card_auth_enabled: Mapped[bool] = mapped_column(Boolean, default=False)
    # 星光包下载渠道（JSON 数组 [{name,url}]，最多 3 个；旧数据为纯 URL 字符串）
    download_url: Mapped[Optional[str]] = mapped_column(String(2048), nullable=True)
    # 游戏渠道：2026-11-22 起改为多值，统一由 GameChannel 表维护。
    # 旧的 games.channel 列已在本日删除（先重启加载新代码，再 DROP COLUMN）。
    # 「请求更新」页是否展示该游戏（2026-11-22）；False=停用更新，前台不出现
    request_updates_enabled: Mapped[bool] = mapped_column(
        Boolean, default=True, server_default=text("1"), nullable=False
    )
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    # 最近编辑时间（后台每次修改游戏自动刷新）
    updated_at: Mapped[Optional[datetime]] = mapped_column(
        DateTime(timezone=True), nullable=True, onupdate=func.now()
    )


class AdminUser(Base):
    __tablename__ = "admin_users"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    username: Mapped[str] = mapped_column(String(64), unique=True)
    password_hash: Mapped[str] = mapped_column(String(255))
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())


class CardKey(Base):
    __tablename__ = "card_keys"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    key_code: Mapped[str] = mapped_column(String(64), unique=True, index=True)
    # global | game
    scope: Mapped[str] = mapped_column(String(16), default="game")
    game_id: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    # hour | day | month | year | permanent | count
    duration_type: Mapped[str] = mapped_column(String(16), default="day")
    # 次数卡：登录扣 1；时长卡也可附加 max_uses（0=不限次）
    max_uses: Mapped[int] = mapped_column(Integer, default=0)
    used_count: Mapped[int] = mapped_column(Integer, default=0)
    # unused | active | expired | banned | exhausted
    status: Mapped[str] = mapped_column(String(16), default="unused", index=True)
    expire_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    activated_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    device_id: Mapped[Optional[str]] = mapped_column(String(128), nullable=True, index=True)
    # v5 迁移后的旧设备 ID 别名（JSON 数组字符串）；仅用于过渡期兼容，不参与当前主绑定
    legacy_device_ids: Mapped[Optional[str]] = mapped_column(Text, nullable=True)
    # 设备上限（2026-10-31 一码多设备）：1=一机一码（默认，旧行为）；N=最多 N 台；0=不限台数
    # 实际绑定关系存 card_devices；本字段只决定「能不能再绑下一台」
    max_devices: Mapped[int] = mapped_column(
        Integer, default=1, server_default=text("1"), nullable=False
    )
    unbind_used: Mapped[bool] = mapped_column(Boolean, default=False)
    note: Mapped[Optional[str]] = mapped_column(String(255), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), onupdate=func.now()
    )


class CardDevice(Base):
    """卡密 ↔ 设备绑定（2026-10-31 一码多设备）。

    一张卡最多 `CardKey.max_devices` 行（0=不限）。`card_keys.device_id` 降级为
    「首绑设备」兼容字段（客户端 device_bound / can_unbind 仍读它），由
    auth_service.sync_primary_device 维护，不再作为唯一绑定依据。
    """

    __tablename__ = "card_devices"
    __table_args__ = (UniqueConstraint("card_id", "device_id", name="uq_card_device"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    card_id: Mapped[int] = mapped_column(Integer, index=True)
    device_id: Mapped[str] = mapped_column(String(128), index=True)
    bound_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    last_seen_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now()
    )


class SessionToken(Base):
    __tablename__ = "session_tokens"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    token: Mapped[str] = mapped_column(String(128), unique=True, index=True)
    card_id: Mapped[int] = mapped_column(Integer, index=True)
    game_id: Mapped[str] = mapped_column(String(64), index=True)
    device_id: Mapped[str] = mapped_column(String(128), index=True)
    expires_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    last_seen_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    revoked: Mapped[bool] = mapped_column(Boolean, default=False)


class Announcement(Base):
    __tablename__ = "announcements"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    scope: Mapped[str] = mapped_column(String(16), default="global")  # global | game
    game_id: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    title: Mapped[str] = mapped_column(String(128))
    content: Mapped[str] = mapped_column(Text)
    enabled: Mapped[bool] = mapped_column(Boolean, default=True)
    sort_order: Mapped[int] = mapped_column(Integer, default=0)
    # always | logged_out | logged_in
    visibility: Mapped[str] = mapped_column(String(16), default="always")
    # every_open | once_device（每设备弹一次，客户端本地记已读）
    popup_mode: Mapped[str] = mapped_column(String(16), default="every_open")
    start_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    end_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    # 有 URL 才显示「立即前往」；空则菜单只显示关闭
    action_url: Mapped[Optional[str]] = mapped_column(String(512), nullable=True)
    action_label: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), onupdate=func.now()
    )


class FeatureFlag(Base):
    __tablename__ = "feature_flags"
    __table_args__ = (UniqueConstraint("game_id", "feat_num", name="uq_game_feat"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    game_id: Mapped[str] = mapped_column(String(64), index=True)
    feat_num: Mapped[int] = mapped_column(Integer)
    name: Mapped[str] = mapped_column(String(128))
    enabled: Mapped[bool] = mapped_column(Boolean, default=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())


class MenuTemplate(Base):
    """后台可维护的菜单模板（替代静态 menu_catalog 运行时读取）。"""

    __tablename__ = "menu_templates"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    template_id: Mapped[str] = mapped_column(String(64), unique=True, index=True)
    name: Mapped[str] = mapped_column(String(128))
    package: Mapped[Optional[str]] = mapped_column(String(128), nullable=True)
    source: Mapped[Optional[str]] = mapped_column(String(255), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), onupdate=func.now()
    )


class MenuTemplateFeature(Base):
    __tablename__ = "menu_template_features"
    __table_args__ = (UniqueConstraint("template_id", "feat_num", name="uq_tpl_feat"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    template_id: Mapped[str] = mapped_column(String(64), index=True)
    feat_num: Mapped[int] = mapped_column(Integer)
    name: Mapped[str] = mapped_column(String(128))


class ToastMessage(Base):
    """开屏启动 Toast（菜单启动时按 sort_order 依次弹出）。"""

    __tablename__ = "toast_messages"
    __table_args__ = (UniqueConstraint("scope", "game_id", "toast_key", name="uq_toast"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    scope: Mapped[str] = mapped_column(String(16), default="global")
    game_id: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    # 内部唯一键；管理端不再手填，创建时自动生成 startup_*
    toast_key: Mapped[str] = mapped_column(String(64))
    content: Mapped[str] = mapped_column(String(255))
    toast_type: Mapped[str] = mapped_column(String(16), default="info")  # 开屏固定 info 星光样式
    enabled: Mapped[bool] = mapped_column(Boolean, default=True)
    sort_order: Mapped[int] = mapped_column(Integer, default=0)
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), onupdate=func.now()
    )


class OnlinePresence(Base):
    __tablename__ = "online_presence"
    __table_args__ = (UniqueConstraint("game_id", "device_id", name="uq_presence"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    game_id: Mapped[str] = mapped_column(String(64), index=True)
    device_id: Mapped[str] = mapped_column(String(128), index=True)
    card_id: Mapped[Optional[int]] = mapped_column(Integer, nullable=True)
    last_seen_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    # 首次观察时间：**只有新写入的行才赋值**；加列前的历史行为 NULL（= unknown，不假装知道）
    created_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    # 最近一次「已认证」确认卡密关联的时间（login / heartbeat 成功才写）。
    # NULL = 本服务从未记录到验证证据（历史行同样是 NULL），不等价于「游客」。
    card_verified_at: Mapped[Optional[datetime]] = mapped_column(
        DateTime(timezone=True), nullable=True
    )
    # 最近一次服务端认证通过的活动时间（login 成功 / 带 token 的 heartbeat 成功）。
    # 与 card_verified_at 的区别：这是**活动事实**，解绑/撤销不会清空；
    # 看板对需卡游戏用它做窗口筛选，避免未认证 bootstrap 灌水影响活跃。
    # NULL = 无认证证据（历史行可为 NULL，不推测填充）。
    last_authenticated_at: Mapped[Optional[datetime]] = mapped_column(
        DateTime(timezone=True), nullable=True
    )


class AuthEvent(Base):
    """登录/激活等客户端事件，供看板今日统计。"""

    __tablename__ = "auth_events"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    # login_ok | login_fail | activate | device_migrate | heartbeat_fail | bootstrap_reject
    event_type: Mapped[str] = mapped_column(String(32), index=True)
    game_id: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    # 合法设备号原样存；非法/可疑只存 deviceid.device_fingerprint() 的 bad_xxx
    device_id: Mapped[Optional[str]] = mapped_column(String(128), nullable=True)
    card_id: Mapped[Optional[int]] = mapped_column(Integer, nullable=True)
    detail: Mapped[Optional[str]] = mapped_column(String(255), nullable=True)
    # ok | deny | error —— 只有结果码，不放凭证
    result: Mapped[Optional[str]] = mapped_column(String(8), nullable=True)
    # bad_device | not_found | banned | expired | exhausted | device_mismatch | rate_limited | ...
    reason: Mapped[Optional[str]] = mapped_column(String(32), nullable=True)
    # 权威来源 = socket peer；ip_forwarded 仅在受信反代下采信，否则仅留档并标记未验证
    ip: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ip_forwarded: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ua: Mapped[Optional[str]] = mapped_column(String(120), nullable=True)
    request_id: Mapped[Optional[str]] = mapped_column(String(36), nullable=True)
    # 同 (device, reason, ip) 在聚合窗口内被抑制的重复次数（保留首条，便于追溯且不刷爆）
    suppressed: Mapped[int] = mapped_column(Integer, default=0, server_default=text("0"), nullable=False)
    # 被抑制的重复失败最近一次发生时间（created_at 保留首条时间，便于"首条+抑制次数"口径）
    last_occurred_at: Mapped[Optional[datetime]] = mapped_column(
        DateTime(timezone=True), nullable=True
    )
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now(), index=True)


class AdminAuditLog(Base):
    """管理端操作审计。"""

    __tablename__ = "admin_audit_logs"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    admin_username: Mapped[str] = mapped_column(String(64), index=True)
    action: Mapped[str] = mapped_column(String(64), index=True)
    detail: Mapped[Optional[str]] = mapped_column(String(512), nullable=True)
    ip: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now(), index=True)


class SiteSetting(Base):
    """站点级配置（如定制 MOD QQ）。"""

    __tablename__ = "site_settings"

    key: Mapped[str] = mapped_column(String(64), primary_key=True)
    value: Mapped[str] = mapped_column(String(512), default="")


class GameChannel(Base):
    """游戏 ↔ 渠道（多值，2026-11-22）。

    后台游戏管理页可为一个游戏配多个渠道；前台「请求更新」表单只能选其中一个。
    渠道只由本表维护：旧的 games.channel 列已于 2026-11-22 删除。
    """

    __tablename__ = "game_channels"
    __table_args__ = (UniqueConstraint("game_id", "channel", name="uq_game_channel"),)

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    game_id: Mapped[str] = mapped_column(String(64), index=True)
    channel: Mapped[str] = mapped_column(String(64))
    # 后台展示顺序（越小越前）；留 0 表示不排序，按 id 兜底
    sort_order: Mapped[int] = mapped_column(Integer, default=0, server_default=text("0"), nullable=False)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())


class UpdateRequest(Base):
    """前台「请求更新」页的提交记录（2026-11-22）。

    一条 = 一个登录用户对某个游戏的一次请求。
    限流（自然日，按北京时间算）：
      · 同 wp_user_id + game_id → 当天最多 1 条
      · 同 wp_user_id          → 当天最多 3 个不同 game_id
    """

    __tablename__ = "update_requests"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    # 游戏：game_id 用于关联后台；game_name 是提交时快照，游戏改名/删除后仍可追溯
    game_id: Mapped[str] = mapped_column(String(64), index=True)
    game_name: Mapped[str] = mapped_column(String(128))
    # 该条选择的渠道（前台单选；游戏无渠道时该游戏不会出现在前台）
    channel: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    # 问题描述：前台限 100 字，列宽留余量存 300 字符
    problem: Mapped[str] = mapped_column(String(300))
    # 提交人（WordPress 账号；由 mu-plugin 服务端注入，前端无法伪造）
    wp_user_id: Mapped[int] = mapped_column(Integer, index=True)
    wp_username: Mapped[str] = mapped_column(String(64))
    # 权威来源 = socket peer（受信反代下取 X-Real-IP）；ip_forwarded 仅留档
    ip: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ip_forwarded: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ua: Mapped[Optional[str]] = mapped_column(String(120), nullable=True)
    # 设备指纹哈希（前端生成，仅用于「人数」去重，不在任何界面展示；可空）
    device_hash: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    # pending | done | blacklisted（已处理 / 拉黑功能暂未开放，先留字段）
    status: Mapped[str] = mapped_column(
        String(16), default="pending", server_default=text("'pending'"), nullable=False, index=True
    )
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), index=True
    )
    processed_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    processed_by: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)


class WishRequest(Base):
    """前台「许愿游戏」页的提交记录（2026-11-22）。

    一条 = 一个登录用户许愿一款游戏。限流：同一 wp_user_id 每个自然日（北京时间）
    最多 1 条，且不分游戏（全局每天只能许愿一款）。

    游戏名由用户自由填写，没有 game_id：
      · game_name 是本次提交的原文快照（最新一条用于后台展示）
      · game_key  是归一化名字的哈希，把「原神」「 原神」「原　神」归并为同一款
    """

    __tablename__ = "wish_requests"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    # 分组键：归一化名字的 sha1 前 16 位（URL 安全，可直接进接口路径）
    game_key: Mapped[str] = mapped_column(String(32), index=True)
    # 展示名：提交原文快照，游戏写法变化不影响历史记录追溯
    game_name: Mapped[str] = mapped_column(String(128))
    # 归一化后的名字（排重用，不在任何界面展示）
    name_norm: Mapped[Optional[str]] = mapped_column(String(128), nullable=True)
    # 该条选择的渠道（前台单选：与「定制游戏」下拉同一套 8 项）
    channel: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    # 许愿人（WordPress 账号；由 mu-plugin 服务端注入，前端无法伪造）
    wp_user_id: Mapped[int] = mapped_column(Integer, index=True)
    wp_username: Mapped[str] = mapped_column(String(64))
    # 权威来源 = socket peer（受信反代下取 X-Real-IP）；ip_forwarded 仅留档
    ip: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ip_forwarded: Mapped[Optional[str]] = mapped_column(String(45), nullable=True)
    ua: Mapped[Optional[str]] = mapped_column(String(120), nullable=True)
    # 设备指纹哈希（前端生成，仅用于「人数」去重，不在任何界面展示；可空）
    device_hash: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    # pending | done | blacklisted（已处理 / 拉黑；拉黑本期未开放，先留字段）
    status: Mapped[str] = mapped_column(
        String(16), default="pending", server_default=text("'pending'"), nullable=False, index=True
    )
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), index=True
    )
    processed_at: Mapped[Optional[datetime]] = mapped_column(DateTime(timezone=True), nullable=True)
    processed_by: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
