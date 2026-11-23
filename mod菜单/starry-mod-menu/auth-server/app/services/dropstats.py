"""可疑上报丢弃计数（2026-11-23）。

口径（lead 定）：
- 可疑数据 **不统计、不落库、不记录明细**，直接丢弃；
- 只在服务器本地留「原因 → 累计次数」，需要时人工能查到，**后台看板不展示**；
- 不写设备号、不写 IP、不写请求体，只有原因码与时间。

落盘文件：settings.drop_stats_path（默认 data/dropped_stats.jsonl），
每 60 秒或每累计 200 次追加一行 JSON；目录不存在时自动创建，写失败只记日志不影响请求。
"""

from __future__ import annotations

import json
import logging
import os
import time
from collections import defaultdict
from threading import Lock
from typing import Optional

from ..config import get_settings

log = logging.getLogger("starry.dropstats")

_lock = Lock()
_counts: dict[str, int] = defaultdict(int)
_by_game: dict[str, int] = defaultdict(int)
_last_flush = 0.0
_pending = 0
FLUSH_INTERVAL_SECONDS = 60.0
FLUSH_PENDING_THRESHOLD = 200


def note_drop(reason: str, game_id: Optional[str] = None) -> None:
    """记录一次「被丢弃的可疑上报」。只计数，不保存任何可识别信息。"""
    global _last_flush, _pending
    key = (reason or "unknown")[:32]
    with _lock:
        _counts[key] += 1
        if game_id:
            _by_game[game_id[:64]] += 1
        _pending += 1
        now = time.time()
        need_flush = (
            _pending >= FLUSH_PENDING_THRESHOLD
            or (now - _last_flush) >= FLUSH_INTERVAL_SECONDS
        )
    if need_flush:
        flush()


def snapshot() -> dict:
    """当前进程内的计数快照（供运维接口/排查使用，不用于看板展示）。"""
    with _lock:
        return {
            "by_reason": dict(_counts),
            "by_game": dict(_by_game),
            "total": sum(_counts.values()),
            "unsaved": _pending,
        }


def flush() -> None:
    """把累计计数追加到落盘文件；失败只记日志，绝不抛给请求路径。"""
    global _last_flush, _pending
    settings = get_settings()
    path = (settings.drop_stats_path or "").strip()
    if not path:
        return
    with _lock:
        if _pending <= 0:
            return
        payload = {
            "ts": time.strftime("%Y-%m-%dT%H:%M:%S%z"),
            "by_reason": dict(_counts),
            "by_game": dict(_by_game),
            "total": sum(_counts.values()),
            "delta": _pending,
        }
        _pending = 0
        _last_flush = time.time()
    try:
        d = os.path.dirname(os.path.abspath(path))
        if d:
            os.makedirs(d, exist_ok=True)
        with open(path, "a", encoding="utf-8") as fh:
            fh.write(json.dumps(payload, ensure_ascii=False) + "\n")
    except Exception as exc:  # pragma: no cover - 磁盘/权限问题
        log.warning("dropstats flush failed: %s", exc)
