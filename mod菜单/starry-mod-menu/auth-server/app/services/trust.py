"""客户端上报可信判定（2026-11-23）。

只回答一件事：这条上报是否「可疑到不该进统计」。命中即 **直接丢弃**：
不写 online_presence、不写 auth_events 明细，只进 dropstats 计数（服务器本地留档）。

判据（全部可配，默认最小侵入）：
1. `device_id` 不是客户端可能产生的两种形态           → bad_device
2. 服务端配了证书白名单，且上报**带了** `cert_sha256` 但不在名单 → bad_cert
   （未携带证书的上报跳过本判据，见 2026-11-24 说明）
3. 服务端配了最低 versionCode，上报版本低于它           → old_client
4. 服务端开了单 IP 新设备上限，本小时该 IP 新设备超限    → ip_burst

边界（必须守住）：
- 判定只影响「统计」，**不影响接口返回**，也不影响授权与功能；
- 证书判据对「未带证书」的上报不生效：签名伪装环境下客户端上报的是原厂证书，
  白名单无法区分我方包与原版包，生产默认留空（2026-11-24 裁决）；
- 未配置的判据一律跳过（默认全跳过 ⇒ 与旧版本行为一致）；
- 不记录任何可识别信息，只有原因码。
"""

from __future__ import annotations

import time
from collections import defaultdict
from threading import Lock
from typing import Optional

from ..config import get_settings
from ..deviceid import is_valid_device_id

_lock = Lock()
# ip -> {device_id: first_seen_ts}（只保留最近 1 小时，防内存膨胀）
_ip_devices: dict[str, dict[str, float]] = defaultdict(dict)
WINDOW_SECONDS = 3600.0
MAX_IPS_TRACKED = 20000


def _cert_whitelist() -> set[str]:
    raw = (get_settings().client_cert_sha256 or "").strip().lower()
    if not raw:
        return set()
    return {x.strip().replace(":", "") for x in raw.split(",") if x.strip()}


def _ip_burst(ip: Optional[str], device_id: str) -> bool:
    limit = int(get_settings().guest_max_new_devices_per_ip_per_hour or 0)
    if limit <= 0 or not ip or ip == "unknown":
        return False
    now = time.time()
    with _lock:
        if len(_ip_devices) > MAX_IPS_TRACKED:
            _ip_devices.clear()
        bucket = _ip_devices[ip]
        for dev, ts in list(bucket.items()):
            if now - ts > WINDOW_SECONDS:
                bucket.pop(dev, None)
        if device_id in bucket:
            bucket[device_id] = now
            return False
        if len(bucket) >= limit:
            return True
        bucket[device_id] = now
        return False


def judge(
    *,
    device_id: Optional[str],
    cert_sha256: Optional[str] = None,
    version_code: int = 0,
    ip: Optional[str] = None,
) -> Optional[str]:
    """返回可疑原因码；None 表示这条上报可以进统计。"""
    if not is_valid_device_id(device_id):
        return "bad_device"

    allowed = _cert_whitelist()
    if allowed:
        got = (cert_sha256 or "").strip().lower().replace(":", "")
        # 2026-11-24：**未携带证书**跳过本判据，不再算可疑。
        # 实测依据：/v1/bootstrap 从不带 cert_sha256（AuthClient 无该参数），旧包也不带；
        # 白名单一旦非空，这类上报会被 100% 丢弃（2026-11-24 生产累计 1107 次 bad_cert，
        # 看板/设备日志因此全空）。保留的能力：带了证书但不在名单 ⇒ 仍然拒绝。
        if got and got not in allowed:
            return "bad_cert"

    min_ver = int(get_settings().client_min_version_code or 0)
    if min_ver > 0 and int(version_code or 0) < min_ver:
        return "old_client"

    if _ip_burst(ip, (device_id or "").strip()):
        return "ip_burst"

    return None
