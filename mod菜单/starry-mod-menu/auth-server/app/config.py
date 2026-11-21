from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_name: str = "星光魔法验证"
    secret_key: str = "dev-secret-change-me"
    admin_username: str = "admin"
    admin_password: str = "admin123"
    database_url: str = "sqlite:///./starlight.db"
    access_token_expire_minutes: int = 10080  # 7 天：避免杀进程后频繁重输卡密（心跳会续期）
    cors_origins: str = "*"
    default_game_id: str = "starry_self"
    default_game_name: str = "星光自测App"
    offline_cache_seconds: int = 86400  # grant 票 TTL=离线宽限 1 天：断网重开可玩；封禁等卡密控制下次冷启动续票时生效
    # 授权票 v2 非对称签名私钥路径（RSA-2048 PKCS#1 v1.5，文件为 n/e/d=hex）。
    # 生产必须配置；留空 = 拒绝签发（503，fail closed），不再回退对称票。**绝不入库**。
    grant_rsa_key_path: str = ""
    # P2-2：载荷加密主密钥（与出包机 ~/.starry-keys/payload_master.txt 同一份）
    payload_master_path: str = ""
    # P2-2：当前在售 build 的明文载荷 sha256（改了载荷必须同步改这里，否则 v3 不签发）
    payload_sha: str = ""
    # 构建机专用：出包门禁查询"该游戏载荷 sha 是否已登记"用；留空=该接口关闭
    build_api_key: str = ""
    # 管理端 IP 白名单，逗号分隔；空=不限制（仅建议生产配置你家宽带 IP）
    admin_allow_ips: str = ""
    # 是否对外暴露 /docs、/redoc、/openapi.json（生产默认关闭，避免泄露接口清单）
    enable_docs: bool = False
    # 管理登录失败限流：窗口秒数内最多失败次数
    admin_login_max_fails: int = 5
    admin_login_window_seconds: int = 900
    # 玩家卡密登录限流（按 IP）
    card_login_max_fails: int = 12
    card_login_window_seconds: int = 600
    # ---- 2026-11-15 遥测/审计可信化 ----
    # 可信反代地址（逗号分隔，精确 IP 或 CIDR）。只有 peer 属于此列表才采信 X-Real-IP /
    # X-Forwarded-For；默认空 = 一律用 socket peer，避免客户端伪造头部刷 IP。
    # 生产在 nginx 反代场景下填反代容器/本机地址（如 127.0.0.1,172.18.0.0/16）。
    trusted_proxy_ips: str = ""
    # 公开 bootstrap 每 IP 滑动窗口上限；0=关闭（默认关闭，避免 NAT 大出口被误杀）
    bootstrap_rate_max: int = 0
    bootstrap_rate_window_seconds: int = 60
    # 同一 (device, reason, ip) 的登录失败聚合窗口：窗口内只留首条并累加 suppressed
    login_fail_dedupe_seconds: int = 600
    # 每个 IP 在聚合窗口内最多记录多少条「不同」失败事件，超出只计一次 rate_limited 标记
    login_fail_max_keys_per_ip: int = 60
    # 事件保留天数（仅用于后台展示与人工清理脚本口径，不做自动删除）
    auth_event_retention_days: int = 90


@lru_cache
def get_settings() -> Settings:
    return Settings()
