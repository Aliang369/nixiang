from __future__ import annotations

from datetime import datetime
from typing import Any, Optional

from pydantic import BaseModel, Field


class ApiOk(BaseModel):
    ok: bool = True
    message: str = "ok"
    data: Any = None


class LoginRequest(BaseModel):
    card_key: str = Field(min_length=4, max_length=64)
    game_id: str = Field(min_length=1, max_length=64)
    device_id: str = Field(min_length=4, max_length=128)
    # v5 首次迁移证据：旧 prefs / 共享种子 / v4 硬件哈希；旧客户端可省略
    legacy_device_ids: Optional[list[str]] = Field(default=None, max_length=32)


class HeartbeatRequest(BaseModel):
    token: str
    game_id: str
    device_id: str
    # 2026-10-04 云机修复：客户端把「历史设备 ID 候选」一并上报，服务端据此把
    # 同一物理身份漂移后的新 ID 迁移到既有会话/绑定，而不是回 403 踢登录。
    # 旧客户端不传（None）→ 行为与旧版逐字一致。
    legacy_device_ids: Optional[list[str]] = Field(default=None, max_length=32)


class UnbindRequest(BaseModel):
    card_key: str
    device_id: str


class PresenceRequest(BaseModel):
    """在线心跳（2026-11-23）：游客与卡密共用，5 分钟一次。

    token 可空（游客）。其余字段只用于「这条上报可不可信」的统计判定，
    服务端**不会**因此拦截功能。
    """

    game_id: str = Field(min_length=1, max_length=64)
    device_id: str = Field(min_length=4, max_length=128)
    token: Optional[str] = Field(default=None, max_length=256)
    client_version: Optional[str] = Field(default=None, max_length=64)
    client_version_code: int = 0
    cert_sha256: Optional[str] = Field(default=None, max_length=128)


class BootstrapQuery(BaseModel):
    game_id: str


class AdminLoginRequest(BaseModel):
    username: str
    password: str


class AdminChangePasswordRequest(BaseModel):
    old_password: str = Field(min_length=1)
    new_password: str = Field(min_length=6, max_length=128)


class MenuTemplateFeatureItem(BaseModel):
    feat_num: int
    name: str = Field(min_length=1, max_length=128)


class MenuTemplateCreate(BaseModel):
    template_id: str = Field(min_length=1, max_length=64)
    name: str = Field(min_length=1, max_length=128)
    package: Optional[str] = Field(default=None, max_length=128)
    source: Optional[str] = Field(default=None, max_length=255)
    features: list[MenuTemplateFeatureItem] = Field(min_length=1)


class MenuTemplateUpdate(BaseModel):
    name: Optional[str] = Field(default=None, min_length=1, max_length=128)
    package: Optional[str] = Field(default=None, max_length=128)
    source: Optional[str] = Field(default=None, max_length=255)
    # 传入则整体替换功能列表；不传则只改元数据
    features: Optional[list[MenuTemplateFeatureItem]] = None


class GameCreate(BaseModel):
    game_id: str
    name: str
    enabled: bool = True
    # 2026-11-23（lead 定）：默认关卡密，前台隐藏账号 Tab；只有显式传 True 才要求卡密
    card_auth_enabled: bool = False
    # 菜单模板 id（库表 menu_templates）；空则尝试用 game_id 匹配
    menu_template: Optional[str] = None
    # 星光包下载渠道（JSON 数组 [{name,url}]，最多 3 个；兼容旧纯 URL）
    download_url: Optional[str] = Field(default=None, max_length=2048)
    # 游戏渠道多选（2026-11-22）：旧的单一 channel 列/字段已删除，只认这个
    channels: Optional[list[str]] = None
    # 创建时是否自动写入模板功能（默认是）
    sync_features: bool = True


class GameUpdate(BaseModel):
    name: Optional[str] = None
    enabled: Optional[bool] = None
    card_auth_enabled: Optional[bool] = None
    menu_template: Optional[str] = None
    download_url: Optional[str] = Field(default=None, max_length=2048)
    # 渠道多选：None=不改；[]=清空全部渠道；数组=整体覆盖
    channels: Optional[list[str]] = None
    # 「请求更新」页是否展示（停用更新 = False）
    request_updates_enabled: Optional[bool] = None


class GameSyncFeaturesRequest(BaseModel):
    # fill=只补缺失；replace=补缺失+更新名称（保留 enabled）；
    # prune=同 replace，并删除模板里没有的多余功能
    mode: str = Field(default="fill", pattern="^(fill|replace|prune)$")
    menu_template: Optional[str] = None


class CardCreate(BaseModel):
    count: int = Field(default=1, ge=1, le=500)
    scope: str = Field(default="game", pattern="^(global|game)$")
    game_id: Optional[str] = None
    duration_type: str = Field(
        default="day", pattern="^(hour|day|month|year|permanent|count)$"
    )
    max_uses: int = Field(default=0, ge=0)
    # 设备上限（2026-10-31）：1=一机一码（默认）；N=最多 N 台；0=不限台数
    max_devices: int = Field(default=1, ge=0, le=100)
    note: Optional[str] = None
    prefix: str = Field(default="SG", max_length=8)
    # 完全自定义卡密：提供时忽略 count/prefix，按输入原样创建（整批查重，重复即拒绝）
    custom_keys: Optional[list[str]] = None


class CardUpdate(BaseModel):
    scope: Optional[str] = Field(default=None, pattern="^(global|game)$")
    game_id: Optional[str] = None
    duration_type: Optional[str] = Field(
        default=None, pattern="^(hour|day|month|year|permanent|count)$"
    )
    max_uses: Optional[int] = Field(default=None, ge=0)
    used_count: Optional[int] = Field(default=None, ge=0)
    # 设备上限：改小只拦新设备登录，不主动踢已绑定设备（要立刻收回用 force_unbind）
    max_devices: Optional[int] = Field(default=None, ge=0, le=100)
    status: Optional[str] = Field(
        default=None, pattern="^(unused|active|expired|banned|exhausted)$"
    )
    expire_at: Optional[datetime] = None
    note: Optional[str] = None
    force_unbind: bool = False


class AnnouncementCreate(BaseModel):
    scope: str = Field(default="global", pattern="^(global|game)$")
    game_id: Optional[str] = None
    title: str
    content: str
    enabled: bool = True
    sort_order: int = 0
    visibility: str = Field(default="always", pattern="^(always|logged_out|logged_in)$")
    popup_mode: str = Field(default="every_open", pattern="^(every_open|once_device)$")
    start_at: Optional[datetime] = None
    end_at: Optional[datetime] = None
    action_url: Optional[str] = Field(default=None, max_length=512)
    action_label: Optional[str] = Field(default=None, max_length=64)
    exit_enabled: bool = False
    exit_label: Optional[str] = Field(default=None, max_length=32)
    close_label: Optional[str] = Field(default=None, max_length=32)


class AnnouncementUpdate(BaseModel):
    scope: Optional[str] = Field(default=None, pattern="^(global|game)$")
    game_id: Optional[str] = None
    title: Optional[str] = None
    content: Optional[str] = None
    enabled: Optional[bool] = None
    sort_order: Optional[int] = None
    visibility: Optional[str] = Field(default=None, pattern="^(always|logged_out|logged_in)$")
    popup_mode: Optional[str] = Field(default=None, pattern="^(every_open|once_device)$")
    start_at: Optional[datetime] = None
    end_at: Optional[datetime] = None
    action_url: Optional[str] = Field(default=None, max_length=512)
    action_label: Optional[str] = Field(default=None, max_length=64)
    exit_enabled: Optional[bool] = None
    exit_label: Optional[str] = Field(default=None, max_length=32)
    close_label: Optional[str] = Field(default=None, max_length=32)


class CardBatchRequest(BaseModel):
    ids: list[int] = Field(min_length=1)
    status: Optional[str] = Field(default=None, pattern="^(unused|active|expired|banned|exhausted)$")
    # 相对延期天数；可与 status 同时用。2026-11-12：禁止负数/NaN/Inf——
    # 负数会把到期时间提前（等于扣时），0 表示只改状态不动时间（保留兼容）。
    extend_days: Optional[float] = Field(default=None, ge=0, allow_inf_nan=False)


class FeatureCreate(BaseModel):
    game_id: str
    feat_num: int
    name: str
    enabled: bool = True


class FeatureUpdate(BaseModel):
    name: Optional[str] = None
    enabled: Optional[bool] = None


class ToastCreate(BaseModel):
    scope: str = Field(default="global", pattern="^(global|game)$")
    game_id: Optional[str] = None
    content: str
    toast_type: str = Field(default="info", pattern="^(success|error|info|warning)$")
    enabled: bool = True
    sort_order: int = 0
    # 兼容旧客户端；不传则服务端自动生成 startup_*
    toast_key: Optional[str] = None


class ToastUpdate(BaseModel):
    content: Optional[str] = None
    toast_type: Optional[str] = None
    enabled: Optional[bool] = None
    scope: Optional[str] = None
    game_id: Optional[str] = None
    sort_order: Optional[int] = None


class UpdateRequestCreate(BaseModel):
    """前台「请求更新」提交（2026-11-22）。

    wp_user_id / wp_username 由 WordPress mu-plugin 在服务端注入，
    前端页面无权填写；接口侧再用共享令牌校验来源。
    """

    game_id: str = Field(..., max_length=64)
    channel: str = Field(..., max_length=64)
    # 先由 pydantic 兜一层长度，业务层再按 100 字精确校验
    problem: str = Field(..., max_length=400)
    wp_user_id: int = Field(..., ge=1)
    wp_username: str = Field(..., max_length=64)
    # 设备指纹哈希（仅去重用，不展示）；可空
    device_hash: Optional[str] = Field(default=None, max_length=64)


class WishRequestCreate(BaseModel):
    """前台「许愿游戏」提交（2026-11-22）。

    游戏名由用户自由填写（没有 game_id）；渠道必须是「定制游戏」那套固定枚举。
    wp_user_id / wp_username 由 WordPress mu-plugin 在服务端注入，
    前端页面无权填写；接口侧再用共享令牌校验来源。
    """

    # 先由 pydantic 兜一层长度，业务层再按 40 字精确校验
    game_name: str = Field(..., max_length=120)
    channel: str = Field(..., max_length=64)
    wp_user_id: int = Field(..., ge=1)
    wp_username: str = Field(..., max_length=64)
    # 设备指纹哈希（仅去重用，不展示）；可空
    device_hash: Optional[str] = Field(default=None, max_length=64)
