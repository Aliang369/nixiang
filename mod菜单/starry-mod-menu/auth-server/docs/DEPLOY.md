# 部署与运维（宝塔 / systemd / MySQL）

> 当前唯一部署方式。旧的 Docker + Postgres + 自签证书方案已归档到 `_legacy_docker/`，不再使用。

## 1. 环境

| 项目 | 值 |
|---|---|
| 主机 | Ubuntu 24.04 · 4 核 / 4 GB |
| 面板 | 宝塔（nginx 1.30.6，MySQL 5.7.44，Redis） |
| Python | 3.12（系统 python3 + `python3.12-venv`） |
| 代码目录 | `/www/wwwroot/starlight-mod-auth` |
| 虚拟环境 | `/www/wwwroot/starlight-mod-auth/.venv` |
| 进程 | systemd `starlight-mod-auth`，uvicorn `127.0.0.1:8010`（**单进程**） |
| 数据库 | MySQL 库 `starlight_mod`，账号 `starlight_mod`（仅本库权限） |
| 站点 | `auth.xgmfyx.cn`，nginx vhost `starlight-mod-auth.conf` |
| 证书 | Let's Encrypt（acme.sh），`/www/server/panel/vhost/cert/auth.xgmfyx.cn/` |

## 1.1 访问入口

| 用途 | 地址 | 说明 |
|---|---|---|
| 管理后台 | `https://auth.xgmfyx.cn/admin/` | 唯一后台入口 |
| 健康检查 | `https://auth.xgmfyx.cn/health` | 只返回 `{"ok":true,...}` |
| 客户端 API | `https://auth.xgmfyx.cn/v1/...` | login / heartbeat / unbind / bootstrap |
| 根路径 | `https://auth.xgmfyx.cn/` | **返回 404**，不再跳转后台，避免暴露入口 |
| 接口文档 | `/docs`、`/redoc`、`/openapi.json` | **生产默认关闭**；需要时 `.env` 设 `ENABLE_DOCS=true` 后重启 |

> `/admin/` 本身是固定路径，若需更强保护，可在 nginx 里对该 location 加 IP 白名单或 Basic Auth。


## 2. 与社区站的隔离边界

本实例与同机的社区 WordPress 站点完全独立，互不影响：

| 维度 | 社区站 | 本实例 |
|---|---|---|
| 目录 | `/www/wwwroot/154_94_232_231` | `/www/wwwroot/starlight-mod-auth` |
| nginx 配置 | `154.94.232.231.conf` | `starlight-mod-auth.conf` |
| 域名 | `xgmfyx.cn` | `auth.xgmfyx.cn` |
| 运行方式 | PHP-FPM | systemd + uvicorn（仅监听回环） |
| 数据库 | `wp_59226015` | `starlight_mod` |
| 日志 | `/www/wwwlogs/154.94.232.231.log` | `/www/wwwlogs/starlight-mod-auth.*.log` |

数据库账号只授权 `starlight_mod.*`，用应用账号 `show databases` 只能看到 `starlight_mod` 与 `information_schema`。
部署与升级过程中**不读取、不写入、不修改**社区站的任何文件与数据。

## 3. 首次部署步骤

```bash
# 1) 目录与代码
mkdir -p /www/wwwroot/starlight-mod-auth
# 上传源码（排除 .venv / __pycache__ / *.db）后解压
chown -R www:www /www/wwwroot/starlight-mod-auth

# 2) 虚拟环境与依赖
cd /www/wwwroot/starlight-mod-auth
python3 -m venv .venv
. .venv/bin/activate
pip install -i https://mirrors.aliyun.com/pypi/simple/ -r requirements.txt pymysql

# 3) 数据库（root 登录后执行；库名/账号固定为 starlight_mod）
mysql -uroot -p -e "CREATE DATABASE IF NOT EXISTS starlight_mod DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -uroot -p -e "CREATE USER IF NOT EXISTS 'starlight_mod'@'127.0.0.1' IDENTIFIED BY '<强密码>';"
mysql -uroot -p -e "GRANT ALL PRIVILEGES ON starlight_mod.* TO 'starlight_mod'@'127.0.0.1'; FLUSH PRIVILEGES;"

# 4) 配置 .env（见下节），然后启动
systemctl daemon-reload
systemctl enable --now starlight-mod-auth

# 5) nginx 站点（vhost 文件放宝塔目录）
#    /www/server/panel/vhost/nginx/starlight-mod-auth.conf
/www/server/nginx/sbin/nginx -t && /www/server/nginx/sbin/nginx -s reload

# 6) 证书
curl -s https://get.acme.sh | sh -s email=<你的邮箱>
/root/.acme.sh/acme.sh --set-default-ca --server letsencrypt
mkdir -p /www/wwwroot/starlight-mod-auth/acme
/root/.acme.sh/acme.sh --issue -d auth.xgmfyx.cn -w /www/wwwroot/starlight-mod-auth/acme --keylength ec-256
/root/.acme.sh/acme.sh --install-cert -d auth.xgmfyx.cn --ecc \
  --key-file       /www/server/panel/vhost/cert/auth.xgmfyx.cn/privkey.pem \
  --fullchain-file /www/server/panel/vhost/cert/auth.xgmfyx.cn/fullchain.pem \
  --reloadcmd      "/www/server/nginx/sbin/nginx -s reload"
```

## 4. `.env` 关键项

| 键 | 说明 |
|---|---|
| `SECRET_KEY` | JWT 签名密钥，随机 32 字节 hex |
| `ADMIN_USERNAME` / `ADMIN_PASSWORD` | 初始管理员（首次登录后请在后台改密） |
| `DATABASE_URL` | `mysql+pymysql://starlight_mod:<密码>@127.0.0.1:3306/starlight_mod?charset=utf8mb4` |
| `DEFAULT_GAME_ID` / `DEFAULT_GAME_NAME` | 首启种子游戏，默认 `starry_self` / `星光自测App` |
| `ADMIN_ALLOW_IPS` | 后台 IP 白名单，逗号分隔；空=不限制 |
| `TRUSTED_PROXY_IPS` | 可信反代地址，**本机 nginx 反代必须填 `127.0.0.1`**；为空则忽略 `X-Real-IP`，事件日志里的 IP 会全是 127.0.0.1 |
| `ENABLE_DOCS` | 是否暴露 `/docs`、`/openapi.json`；生产保持 `false` |

改完 `.env` 必须 `systemctl restart starlight-mod-auth`。

## 5. nginx 站点要点

- 80 端口：仅用于 ACME 校验（`/.well-known/acme-challenge/` → `acme/` 目录）并 301 跳 HTTPS。
- 443 端口：TLSv1.2/1.3，反代 `http://127.0.0.1:8010`，透传 `Host` / `X-Real-IP` / `X-Forwarded-Proto`。
- `client_max_body_size 2m`（后台仅剩表单/JSON 上传；原 8m 为 OTA 上传预留，已随 OTA 下线收紧）
- 该站点为**手工 vhost**，宝塔面板「网站」列表不显示；不要在面板对它重复建站，否则会覆盖反代配置。

## 6. 运维命令

```bash
systemctl status  starlight-mod-auth      # 状态
systemctl restart starlight-mod-auth      # 重启（改 .env 后必须）
systemctl stop    starlight-mod-auth      # 停止

tail -f /www/wwwlogs/starlight-mod-auth.err.log     # 应用错误
tail -f /www/wwwlogs/starlight-mod-auth.out.log     # 标准输出
tail -f /www/wwwlogs/starlight-mod-auth.access.log  # 访问日志

/www/server/nginx/sbin/nginx -t                          # 配置检查
/www/server/nginx/sbin/nginx -s reload                   # 平滑重载

mysql -h127.0.0.1 -u starlight_mod -p starlight_mod      # 应用账号连库
/root/.acme.sh/acme.sh --cron --home /root/.acme.sh      # 手动触发证书续期

curl -s https://auth.xgmfyx.cn/health                    # 健康检查
```

## 7. 代码适配说明（相对通用实现）

1. **MySQL 方言**：`app/main.py` 的 `ensure_schema()` 补列逻辑原为非 SQLite 分支统一走
   PostgreSQL 语法（`ADD COLUMN IF NOT EXISTS` / `ALTER COLUMN ... TYPE`），MySQL 5.7 不支持会启动失败。
   已限定为 `elif dialect.startswith("postgres")`；MySQL 全新库由 `create_all` 建表，无需补列。
2. **单进程**：`uvicorn --workers 1`。多 worker 会同时执行 `create_all`，触发
   `1050 Table already exists` 竞争退出。
3. **驱动**：额外安装 `pymysql`。

## 8. 初始数据（从 0 开始）

| 表 | 行数 | 说明 |
|---|---|---|
| `games` | 1 | 种子游戏 `starry_self` / 星光自测App |
| `admin_users` | 1 | 初始管理员 |
| `menu_templates` | 1 | 仅内置 `starry_self` |
| `feature_flags` | 8 | 自测模板的功能项 |
| `card_keys` / `announcements` / `site_settings` | 0 | 空，由后台创建 |

不继承任何历史实例的数据。

## 9. 升级流程

推荐用脚本（远端自动快照 → 同步 → 重启 → /health 轮询；见 `scripts/ops/README.md`）：

```bash
cd auth-server/scripts/ops
./deploy_api.sh --dry-run                                   # 先看计划，零写操作
REMOTE=root@154.94.232.231 ./deploy_api.sh                  # 发布
./smoke_api.sh                                              # 冒烟
```

手工等价步骤：

```bash
# 本地改动后上传 app/ 与 static/（不要覆盖 .env / data/）
scp -r app static <user>@<host>:/www/wwwroot/starlight-mod-auth/
chown -R www:www /www/wwwroot/starlight-mod-auth/app /www/wwwroot/starlight-mod-auth/static
systemctl restart starlight-mod-auth
curl -s https://auth.xgmfyx.cn/health
```

数据库结构变更需手工执行 `ALTER TABLE`（`create_all` 不会改已存在的表）。

### 升级到 2026-11-21 版本（设备可信化 / 在线分层 / 设备日志）

`create_all` 不会给已存在的表补列，MySQL 上必须手工执行：

```sql
ALTER TABLE online_presence
  ADD COLUMN created_at DATETIME NULL,
  ADD COLUMN card_verified_at DATETIME NULL,
  ADD COLUMN last_authenticated_at DATETIME NULL;

ALTER TABLE auth_events
  ADD COLUMN result VARCHAR(8) NULL,
  ADD COLUMN reason VARCHAR(32) NULL,
  ADD COLUMN ip VARCHAR(45) NULL,
  ADD COLUMN ip_forwarded VARCHAR(45) NULL,
  ADD COLUMN ua VARCHAR(120) NULL,
  ADD COLUMN request_id VARCHAR(36) NULL,
  ADD COLUMN suppressed INT NOT NULL DEFAULT 0,
  ADD COLUMN last_occurred_at DATETIME NULL;

CREATE INDEX ix_auth_events_reason ON auth_events (reason);
CREATE INDEX ix_auth_events_result ON auth_events (result);
```

同时 `.env` 必须补 `TRUSTED_PROXY_IPS=127.0.0.1`，否则设备日志记不到真实来源 IP。

> `_legacy_docker/ops/sql/*.sql` 是 **PostgreSQL 语法**（`CONCURRENTLY`、`TIMESTAMPTZ` 等），
> MySQL 环境**不能直接执行**，只作口径参考；MySQL 侧的清理/回填需另行改写。
> 这批 SQL 与 Docker 时期的运维脚本一起归档在 `_legacy_docker/ops/`（2026-11-24）。

### 升级到 2026-11-22 版本（载荷 sha / v3 残留整体删除）

v3 票已于 2026-11-21 退役（只发 v2 非对称票），其载荷体系本次一并删除：

- 后台：游戏行「操作 ▾ → 载荷 sha」入口与弹窗；
- 接口：`GET/PUT /v1/admin/games/{game_id}/payload`、`GET /v1/build/payload-status`；
- 字段/配置：`games.payload_sha`、`PAYLOAD_MASTER_PATH`、`PAYLOAD_SHA`、`BUILD_API_KEY`；
- 脚本：`harden_apk.py` 的 C9 与 `fetch_starry_keys.sh` 第 4 步自证 C 标「已下线」，不再联网。

**MySQL 必须手工删列**（`create_all` 不会改已存在的表）：

```sql
-- 先备份 games 表：mysqldump --single-transaction <库> games > games_before_drop.sql
ALTER TABLE games DROP COLUMN payload_sha;
```

`.env` 里残留的 `PAYLOAD_MASTER_PATH` / `BUILD_API_KEY` 不再被读取（pydantic `extra=ignore`），
建议下次维护窗口一并删掉。**只动 bt（154.94.232.231），不动 starlight（47.100.216.101）。**


### 授权票 v2-only（重要）

本版本的 `app/services/auth_service.py` 对**卡密登录 / 心跳 / 免卡 bootstrap** 采用
v2 非对称签名票，且 `grant_rsa_key_path` 为空时**直接拒绝签发（HTTP 503）**，不再回退 v1 HMAC。

### 升级到 2026-11-23 版本（公告三按钮）

公告卡支持三个按钮：关闭程序（`exit_enabled` / `exit_label`）、跳转链接（沿用
`action_url` / `action_label`）、关闭弹窗进游戏（`close_label`）。全部可空，不配置时
客户端表现与旧版完全一致；**新按钮要生效必须重出客户端包**（Java 在 dex 内）。

**MySQL 必须手工补列**（`create_all` 不会改已存在的表；只动 bt，不动 starlight）：

```sql
ALTER TABLE announcements
  ADD COLUMN exit_enabled TINYINT(1) NOT NULL DEFAULT 0,
  ADD COLUMN exit_label VARCHAR(32) NULL,
  ADD COLUMN close_label VARCHAR(32) NULL;
```

改完 `systemctl restart starlight-mod-auth`，`/health` 200 即完成。
`app/main.py` 的 `ensure_schema()` 已同步三种方言的补列逻辑（MySQL 走
information_schema 先查后加），新装库由 `create_all` 直接建全。

**当前生产状态：已配置并验证通过。**

| 项目 | 值 |
|---|---|
| 私钥 | `/www/wwwroot/starlight-mod-auth/keys/rsa2048.txt`（600 / www:www） |
| 载荷主密钥 | ~~`keys/payload_master.txt`~~ **2026-11-22 起服务端不再读取**（v3 票退役，`payload_sha` 体系删除）；文件仅老包复刻留档 |
| 公钥指纹 | `sha256(模数)[:16] = 25ba10e0faca3500` |
| 客户端对应文件 | `app/src/main/jni/Includes/sg_pubkey.h` → `kSgPubN` / `kSgPubFp` |
| 出包机私钥来源 | `C:\Users\Administrator\.starry-keys\rsa2048.txt` |

两端模数与指纹已逐字节比对一致。换密钥时必须同时更新出包机、
服务器 `keys/`、以及 APK 的 `sg_pubkey.h`（重跑 `04_scripts/_common/sg_pki/gen_pubkey_header.py`）。

**客户端连接约束**：`AuthClient.java` 硬编码 `ALLOWED_HOST = "auth.xgmfyx.cn"`，
只允许 HTTPS + 443，其他主机在发请求前即被拒绝。因此 APK 必然连本实例，
`/v1/build/payload-status` 已于 2026-11-22 删除，`~/.starry-keys/api_base.txt` 不再被任何门禁使用（历史遗留）。
APK 运行时连接不受影响。

### 升级到 2026-11-23 版本（游客统计 / 设备日志身份 / 在线口径）

**口径正文（唯一权威）**：[STATS_DEFINITION.md](./STATS_DEFINITION.md)。改口径先改那份文档。

本版本做四件事：

1. **需卡游戏的游客也落在线表**（`identity=guest`，不写 `card_id`/认证时间）；
2. **新增在线心跳** `POST /v1/presence`（5 分钟一次，游客与卡密共用），在线窗口 90 秒 → **360 秒**
   （`ONLINE_WINDOW_SECONDS`）；
3. **设备日志可区分身份**：新增 `guest_seen` 事件，列表加「身份」列，筛选加「卡密/游客」；
4. **可疑上报直接丢弃**（非法设备号 / 证书不符 / 版本过低 / IP 突发）：不落库、不记明细，
   只在 `data/dropped_stats.jsonl` 留累计次数；`GET /v1/admin/dropped-stats` 可查实时值（侧栏无入口）。

**MySQL 必须手工补列**（`create_all` 不会改已存在的表；只动 bt，不动 starlight）：

```sql
ALTER TABLE online_presence
  ADD COLUMN identity VARCHAR(8) NULL,
  ADD COLUMN guest_seen_at DATETIME NULL;

ALTER TABLE auth_events
  ADD COLUMN identity VARCHAR(8) NULL;
```

完整脚本（含「从零开始」清空自测数据）：`scripts/ops/sql/mysql_migrate_guest_stats_20261123.sql`。

`.env` 新增（都可留空 = 不启用对应判据）：

```ini
ONLINE_WINDOW_SECONDS=360
# 证书白名单：空=不校验；配置后不在名单内的上报按可疑丢弃（只影响统计）
CLIENT_CERT_SHA256=
CLIENT_MIN_VERSION_CODE=0
DROP_STATS_PATH=data/dropped_stats.jsonl
GUEST_MAX_NEW_DEVICES_PER_IP_PER_HOUR=0
```

升级后验证（只读）：

```sql
-- 身份分布（应为 card / guest 两种，NULL 只允许出现在升级前的历史行）
SELECT game_id, identity, COUNT(*) FROM online_presence GROUP BY game_id, identity;
SELECT event_type, identity, COUNT(*) FROM auth_events GROUP BY event_type, identity;
```

后台确认：总在线 = 卡密 + 游客（按设备互斥）；设备日志筛选「游客」能看到 `游客出现`；
侧栏**不应**出现 dropped-stats 入口。

### 2026-11-24 热修（证书判据默认关闭 / 游客统计恢复）

**现象**：后台「数据统计 / 看板 / 设备日志」几乎无数据，但线上持续有人使用。

**根因（实测）**：
1. 2026-11-24 02:01 起 `.env` 配了 `CLIENT_CERT_SHA256`（`83c5cc0b…`=`starry_release.jks` + `32fb600a…` 测试证书）；
2. `/v1/bootstrap` **从不携带** `cert_sha256`（`AuthClient` 无该参数，nginx 日志 368/368 条无此 query）；
3. 包做了签名伪装，`PresenceReporter` 上报的是**游戏官方证书**（yszzx `de8a75d3…`、wdyz `ce7afa48…`、
   sf3 `b147f6a4…`、nhsy2 `b86bbedf…`），不在白名单；
4. 结果 `trust.judge` 把每条上报判 `bad_cert` 丢弃：`data/dropped_stats.jsonl` 累计 1107 次，
   `online_presence` 仅 4 行、`auth_events` 仅 12 条。

**改动**：
- `app/services/trust.py`：证书判据只在**上报带了证书**时生效，未携带 ⇒ 跳过（不再判可疑）；
- 生产 `.env`：`CLIENT_CERT_SHA256=` 留空（保留注释说明原因），备份 `.env.bak_nocert_20261124`；
- 口径同步：`docs/STATS_DEFINITION.md` §4/§6。

**不需要改表结构**。验证：`tail data/dropped_stats.jsonl` 的 `delta` 归零；
`SELECT COUNT(*) FROM online_presence` 在 10 分钟内增长；看板总在线与 nginx 360 秒内独立 `device_id` 数一致。
