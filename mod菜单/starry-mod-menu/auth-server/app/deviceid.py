"""设备号形态校验与可信判定（2026-11-15）。

背景：`/v1/bootstrap` 与 `/v1/auth/login` 都接受客户端自报的 `device_id`。
历史上服务端不做任何形态校验，导致任意串（SQL 注入载荷、测试标记、4 字符短号）
都能落进 online_presence 并计入看板活跃。

本模块只回答两件事，不改变客户端协议：
1. 这个 device_id 是否可能是星光客户端产生的（精确形态匹配）；
2. 在 SQL 里如何表达同一判据（Postgres 用 ~，SQLite 用 REGEXP，见 database.py）。

客户端只会产生两种形态（均为小写十六进制）：
- `dev_` + 32 位：v5 SSAID 指纹 / v4 Build 指纹
  （app/src/main/java/com/starry/magic/auth/DeviceFingerprint.java）
- `dev_` + 40 位：本地兜底随机号
  （app/src/main/java/com/starry/magic/menu/Preferences.java localFallbackDeviceId）

注意：形态合法 ≠ 真人。可信活跃只认 token/卡密登录成功写下的 card_verified_at；
形态校验只用于「别把明显不是客户端的串计进统计」，不得当作身份认证。
"""
from __future__ import annotations

import hashlib
import re

from sqlalchemy import ColumnElement

# 精确匹配：32 位或 40 位小写十六进制，前缀 dev_
DEVICE_ID_PATTERN = r"^(?:dev_[0-9a-f]{32}|dev_[0-9a-f]{40})$"
_DEVICE_ID_RE = re.compile(DEVICE_ID_PATTERN)

# 卡密形态（SG-XXXX-XXXX-XXXX-XXXX / AL-...），用于把历史 detail 里的明文卡密脱敏
_CARD_KEY_RE = re.compile(r"\b[A-Z]{2}-[A-Z0-9]{4}(?:-[A-Z0-9]{4}){3}\b")


def is_valid_device_id(value: str | None) -> bool:
    """是否为客户端可能产生的设备号形态。"""
    if not value:
        return False
    return bool(_DEVICE_ID_RE.match(value.strip()))


def device_fingerprint(value: str | None) -> str | None:
    """不可逆短指纹：用于记录非法 device_id 来源，避免把长 payload/凭证原样入库。"""
    raw = (value or "").strip()
    if not raw:
        return None
    return "bad_" + hashlib.sha256(raw.encode("utf-8", "replace")).hexdigest()[:12]


def mask_card_key(value: str | None) -> str:
    """把卡密（含 detail 中的历史明文）替换成不可还原的掩码。"""
    if not value:
        return ""
    return _CARD_KEY_RE.sub("****-****-****-****", value)


def mask_device_id(value: str | None, keep: int = 8) -> str:
    """日志展示用：保留前 keep 位，其余打码（device_id 不是凭证，但避免整串外泄）。"""
    raw = (value or "").strip()
    if len(raw) <= keep:
        return raw
    return raw[:keep] + "…"


def trusted_device_clause(column) -> ColumnElement[bool]:
    """SQL 侧同一判据；SQLite 由 database.py 注册 regexp() 实现。"""
    return column.regexp_match(DEVICE_ID_PATTERN)
