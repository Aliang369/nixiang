from __future__ import annotations

import ipaddress
import time
from collections import defaultdict
from threading import Lock

from fastapi import HTTPException, Request

from .config import get_settings

_lock = Lock()
# ip -> list[fail_timestamps]
_login_fails: dict[str, list[float]] = defaultdict(list)
_card_login_fails: dict[str, list[float]] = defaultdict(list)
# 限流请求计数：key -> list[timestamps]（bootstrap 限流共用；原 OTA 限流已删）
_rate_hits: dict[str, list[float]] = defaultdict(list)


def _trusted_proxy_nets():
    raw = (get_settings().trusted_proxy_ips or "").strip()
    if not raw:
        return []
    nets = []
    for item in raw.split(","):
        item = item.strip()
        if not item:
            continue
        try:
            nets.append(ipaddress.ip_network(item, strict=False))
        except ValueError:
            continue
    return nets


def _parse_ip(value: str | None) -> str | None:
    """只接受可解析的 IP 字面量，返回规范化短串（<=45）；否则 None。"""
    if not value:
        return None
    raw = value.strip()
    if len(raw) > 45 or any(ch < " " for ch in raw):
        return None
    try:
        return str(ipaddress.ip_address(raw))
    except ValueError:
        return None


def resolve_client_ip(request: Request) -> tuple[str, str]:
    """返回 (ip, source)。source: peer | x-real-ip | xff。

    安全口径：只有 socket peer 命中 trusted_proxy_ips（精确 IP 或 CIDR）时才采信转发头；
    否则一律以 peer 为准，客户端自带 X-Forwarded-For 无法伪造。转发值只取单个 IP 字面量
    并做长度/格式校验，不保存任意长原文。
    """
    peer = "unknown"
    if request.client and request.client.host:
        peer = _parse_ip(request.client.host) or "unknown"
    nets = _trusted_proxy_nets()
    if not nets:
        return peer, "peer"
    try:
        peer_ip = ipaddress.ip_address(peer)
    except ValueError:
        return peer, "peer"
    if not any(peer_ip in net for net in nets):
        return peer, "peer"
    # 只认 nginx 覆盖写的 X-Real-IP；X-Forwarded-For 客户端可自传最左值，一律不采信。
    xri = _parse_ip(request.headers.get("x-real-ip"))
    if xri:
        return xri, "x-real-ip"
    return peer, "peer"


def _trusted_proxy_nets():
    raw = (get_settings().trusted_proxy_ips or "").strip()
    if not raw:
        return []
    nets = []
    for item in raw.split(","):
        item = item.strip()
        if not item:
            continue
        try:
            nets.append(ipaddress.ip_network(item, strict=False))
        except ValueError:
            continue
    return nets


def _parse_ip(value: str | None) -> str | None:
    """只接受可解析的 IP 字面量，返回规范化短串（<=45）；否则 None。"""
    if not value:
        return None
    raw = value.strip()
    if len(raw) > 45 or any(ch < " " for ch in raw):
        return None
    try:
        return str(ipaddress.ip_address(raw))
    except ValueError:
        return None


def resolve_client_ip(request: Request) -> tuple[str, str]:
    """返回 (ip, source)。source: peer | x-real-ip | xff。

    安全口径：只有 socket peer 命中 trusted_proxy_ips（精确 IP 或 CIDR）时才采信转发头；
    否则一律以 peer 为准，客户端自带 X-Forwarded-For 无法伪造。转发值只取单个 IP 字面量
    并做长度/格式校验，不保存任意长原文。
    """
    peer = "unknown"
    if request.client and request.client.host:
        peer = _parse_ip(request.client.host) or "unknown"
    nets = _trusted_proxy_nets()
    if not nets:
        return peer, "peer"
    try:
        peer_ip = ipaddress.ip_address(peer)
    except ValueError:
        return peer, "peer"
    if not any(peer_ip in net for net in nets):
        return peer, "peer"
    # 只认 nginx 覆盖写的 X-Real-IP；X-Forwarded-For 客户端可自传最左值，一律不采信。
    xri = _parse_ip(request.headers.get("x-real-ip"))
    if xri:
        return xri, "x-real-ip"
    return peer, "peer"


def client_ip(request: Request) -> str:
    """限流审计用的客户端 IP：**只认受信反代**，未配置时一律 socket peer。

    安全优先（2026-11-15 lead 定）：默认不信任任何可伪造请求头。反代场景必须由部署侧把
    TRUSTED_PROXY_IPS 配成精确 Docker 网段（如 172.19.0.0/16），此时才能读到真实的
    X-Real-IP；否则所有请求按 peer 计（宁可限流桶变粗，也不接受伪造 IP）。
    """
    ip, _source = resolve_client_ip(request)
    return ip


def assert_admin_ip_allowed(request: Request) -> None:
    settings = get_settings()
    raw = (settings.admin_allow_ips or "").strip()
    if not raw:
        return
    allow = {x.strip() for x in raw.split(",") if x.strip()}
    ip = client_ip(request)
    if ip not in allow:
        raise HTTPException(403, f"管理端禁止此 IP 访问（{ip}）")


def assert_login_not_rate_limited(request: Request) -> None:
    settings = get_settings()
    window = max(settings.admin_login_window_seconds, 60)
    max_fails = max(settings.admin_login_max_fails, 1)
    ip = client_ip(request)
    now = time.time()
    with _lock:
        fails = [t for t in _login_fails[ip] if now - t < window]
        _login_fails[ip] = fails
        if len(fails) >= max_fails:
            raise HTTPException(429, f"登录失败次数过多，请 {window // 60} 分钟后再试")


def record_login_failure(request: Request) -> None:
    ip = client_ip(request)
    with _lock:
        _login_fails[ip].append(time.time())


def clear_login_failures(request: Request) -> None:
    ip = client_ip(request)
    with _lock:
        _login_fails.pop(ip, None)


def assert_card_login_not_rate_limited(request: Request) -> None:
    settings = get_settings()
    window = max(settings.card_login_window_seconds, 60)
    max_fails = max(settings.card_login_max_fails, 1)
    ip = client_ip(request)
    now = time.time()
    with _lock:
        fails = [t for t in _card_login_fails[ip] if now - t < window]
        _card_login_fails[ip] = fails
        if len(fails) >= max_fails:
            raise HTTPException(429, f"卡密尝试过多，请 {window // 60} 分钟后再试")


def record_card_login_failure(request: Request) -> None:
    ip = client_ip(request)
    with _lock:
        _card_login_fails[ip].append(time.time())


def clear_card_login_failures(request: Request) -> None:
    ip = client_ip(request)
    with _lock:
        _card_login_fails.pop(ip, None)


def assert_bootstrap_rate_allowed(request: Request) -> None:
    """公开 bootstrap 的可选限流（默认关闭：bootstrap_rate_max=0）。

    默认不启用是为了避免 NAT 大出口被误杀；需要时由部署侧配置阈值。
    """
    settings = get_settings()
    max_n = int(settings.bootstrap_rate_max)
    if max_n <= 0:
        return
    window = max(int(settings.bootstrap_rate_window_seconds), 10)
    ip = client_ip(request)
    key = f"bootstrap:{ip}"
    now = time.time()
    with _lock:
        hits = [t for t in _rate_hits[key] if now - t < window]
        if len(hits) >= max_n:
            _rate_hits[key] = hits
            raise HTTPException(429, "启动配置请求过于频繁，请稍后再试")
        hits.append(now)
        _rate_hits[key] = hits
