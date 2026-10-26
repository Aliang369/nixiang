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
    default_game_id: str = "zhmr"
    default_game_name: str = "战魂铭人"
    offline_cache_seconds: int = 86400  # grant 票 TTL=离线宽限 1 天：断网重开可玩；封禁等卡密控制下次冷启动续票时生效
    # 客户端授权短票 HMAC 密钥（须与 APK native 内置一致；生产务必改掉）
    grant_hmac_secret: str = "sg-grant-v1-7f3a9c2e"
    # 授权票 v2 非对称签名私钥路径（RSA-2048 PKCS#1 v1.5，文件为 n/e/d=hex）。
    # 留空 = 只发 v1(HMAC) 票。生产必须配置，且**绝不入库**。
    grant_rsa_key_path: str = ""
    # P2-2：载荷加密主密钥（与出包机 ~/.starry-keys/payload_master.txt 同一份）
    payload_master_path: str = ""
    # P2-2：当前在售 build 的明文载荷 sha256（改了载荷必须同步改这里，否则 v3 不签发）
    payload_sha: str = ""
    # 管理端 IP 白名单，逗号分隔；空=不限制（仅建议生产配置你家宽带 IP）
    admin_allow_ips: str = ""
    # 管理登录失败限流：窗口秒数内最多失败次数
    admin_login_max_fails: int = 5
    admin_login_window_seconds: int = 900
    # 玩家卡密登录限流（按 IP）
    card_login_max_fails: int = 12
    card_login_window_seconds: int = 600
    # 玩法 OTA 落盘根目录（每 game_id/latest 一份）
    ota_data_dir: str = ""
    # OTA 按 IP 限流（滑动窗口内次数）
    ota_manifest_max: int = 40
    ota_manifest_window_seconds: int = 60
    ota_json_max: int = 20
    ota_json_window_seconds: int = 3600
    ota_so_max: int = 8
    ota_so_window_seconds: int = 3600
    # so 下载是否要求有效卡密会话 token（仅联网无卡密游戏可设 false）
    ota_so_require_token: bool = True
    # patch.json 下载是否要求有效卡密会话 token（玩法偏移数据；默认要求）
    ota_json_require_token: bool = True


@lru_cache
def get_settings() -> Settings:
    return Settings()
