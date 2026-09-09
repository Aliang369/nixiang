from __future__ import annotations

import time
from collections import defaultdict
from threading import Lock

from fastapi import HTTPException, Request

from .config import get_settings

_lock = Lock()
# ip -> list[fail_timestamps]
_login_fails: dict[str, list[float]] = defaultdict(list)
_card_login_fails: dict[str, list[float]] = defaultdict(list)
# OTA 请求计数：key -> list[timestamps]
_ota_hits: dict[str, list[float]] = defaultdict(list)


def client_ip(request: Request) -> str:
    forwarded = request.headers.get("x-forwarded-for") or request.headers.get("x-real-ip")
    if forwarded:
        return forwarded.split(",")[0].strip()
    if request.client:
        return request.client.host or "unknown"
    return "unknown"


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


def _assert_ota_rate(request: Request, kind: str, max_n: int, window: int) -> None:
    """kind: manifest | json | so"""
    max_n = max(int(max_n), 1)
    window = max(int(window), 10)
    ip = client_ip(request)
    key = f"{kind}:{ip}"
    now = time.time()
    with _lock:
        hits = [t for t in _ota_hits[key] if now - t < window]
        if len(hits) >= max_n:
            if window >= 3600:
                tip = f"{window // 3600} 小时"
            elif window >= 60:
                tip = f"{window // 60} 分钟"
            else:
                tip = f"{window} 秒"
            raise HTTPException(429, f"游戏补丁请求过于频繁，请 {tip} 后再试")
        hits.append(now)
        _ota_hits[key] = hits


def assert_ota_manifest_allowed(request: Request) -> None:
    s = get_settings()
    _assert_ota_rate(
        request, "manifest", s.ota_manifest_max, s.ota_manifest_window_seconds
    )


def assert_ota_json_allowed(request: Request) -> None:
    s = get_settings()
    _assert_ota_rate(request, "json", s.ota_json_max, s.ota_json_window_seconds)


def assert_ota_so_allowed(request: Request) -> None:
    s = get_settings()
    _assert_ota_rate(request, "so", s.ota_so_max, s.ota_so_window_seconds)
