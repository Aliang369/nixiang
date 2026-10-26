from __future__ import annotations

from datetime import datetime
from typing import Optional

from sqlalchemy import Boolean, DateTime, Integer, String, Text, UniqueConstraint, func
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
    # True=客户端需卡密登录并显示账号 Tab；False=隐藏账号 Tab，功能仅要求联网 bootstrap
    card_auth_enabled: Mapped[bool] = mapped_column(Boolean, default=True)
    # True=在门户页展示；False=门户页隐藏（不影响客户端授权，仅控制展示）
    portal_visible: Mapped[bool] = mapped_column(Boolean, default=True)
    # 星光包下载渠道（JSON 数组 [{name,url}]，最多 3 个；旧数据为纯 URL 字符串）；空则门户页下载按钮置灰
    download_url: Mapped[Optional[str]] = mapped_column(String(2048), nullable=True)
    # 游戏来源渠道（TapTap/九游/4399/好游快爆/B站/国际服，选「其它」存自定义文本）；空=未设置
    channel: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    # P2-2：该游戏当前在售包的**明文载荷 sha256**（服务端据此派生载荷密钥 K_build）。
    # 后台可改，改完下一次发票即生效（无需重建镜像）；留空 = 该游戏不签 v3 票（只发 v1/v2）。
    # ⚠️ 改这一项会让该游戏所有旧包的 K_build 变化 ⇒ 必须同步出新包。
    payload_sha: Mapped[Optional[str]] = mapped_column(String(64), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    # 最近编辑时间（后台每次修改游戏自动刷新）；门户页「最新」排序依据
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
    unbind_used: Mapped[bool] = mapped_column(Boolean, default=False)
    note: Mapped[Optional[str]] = mapped_column(String(255), nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=func.now())
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), server_default=func.now(), onupdate=func.now()
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


class AuthEvent(Base):
    """登录/激活等客户端事件，供看板今日统计。"""

    __tablename__ = "auth_events"

    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    # login_ok | login_fail | activate | device_migrate
    event_type: Mapped[str] = mapped_column(String(32), index=True)
    game_id: Mapped[Optional[str]] = mapped_column(String(64), nullable=True, index=True)
    device_id: Mapped[Optional[str]] = mapped_column(String(128), nullable=True)
    card_id: Mapped[Optional[int]] = mapped_column(Integer, nullable=True)
    detail: Mapped[Optional[str]] = mapped_column(String(255), nullable=True)
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
