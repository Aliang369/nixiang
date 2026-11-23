# 统计口径定义（唯一权威正文）

> 2026-11-23 建立。**改口径先改这里，再改代码**；代码注释只写实现细节，不重复定义。
> 适用：bt 生产（`auth.xgmfyx.cn` / `/www/wwwroot/starlight-mod-auth`）与本地。
> 相关实现：`app/services/presence.py`、`app/services/trust.py`、
> `app/services/dropstats.py`、`app/routers/client.py`、`app/routers/admin.py`。

## 1. 三类上报（统计的唯一数据源）

| 上报 | 时机 | 是否带卡密/会话 | 写入 |
|---|---|---|---|
| `GET /v1/bootstrap` | 客户端**进程启动后自动一次**（不等打开菜单；3.11 起生效，旧包为菜单显示时） | 可带 token（登录态） | 在线表 `online_presence`；不写卡关联 |
| `POST /v1/presence` | **每 5 分钟一次**（进程活着才发；3.11 起） | token 可空（游客就是空） | 在线表；带合法 token 时顺带写认证事实 |
| `POST /v1/auth/login` / `/v1/auth/heartbeat` | 用户登录 / 冷启动对账 | 必须带 | 认证事实（`card_id`/`card_verified_at`/`last_authenticated_at`）+ `identity=card` |

## 2. 身份（每行 = 游戏 + 设备）

| 值 | 含义 | 何时写 |
|---|---|---|
| `card` | 这台设备在这款游戏**最近一次**是认证上报 | login 成功 / 带 token 的心跳成功 / `/v1/presence` 带合法 token |
| `guest` | 最近一次是**未认证**上报 | bootstrap、无 token 的 `/v1/presence`、解绑后 |
| NULL | 历史行 / 未经可信判定的直接插库 | **一律不计入任何统计** |

铁律：`card` 不会被后续游客上报覆盖（否则统计随最后一次请求漂移）；解绑/撤销时才显式回到 `guest`。

## 3. 看板每个数字管什么

| 看板指标 | 数据源 | 判定 | 去重 |
|---|---|---|---|
| 总在线 | `online_presence.last_seen_at` | 最近 **360 秒**内有上报，且 `identity` 非空，且设备号形态合法，且属已登记游戏 | 按 `device_id`（同机多游戏算 1 台） |
| 卡密在线 | 同上 + `identity='card'` | 窗口内该设备**任一游戏**有认证上报 | 按设备 |
| 游客在线 | 同上 + `identity='guest'` 且该设备窗口内**没有任何** `card` 行 | 与卡密**按设备互斥**（不是减法，是互斥子查询） | 按设备 |
| 分游戏在线 / 卡密 / 游客 | 同三行，按 `game_id` 分组 | 同上 | 按 (游戏, 设备) 行 |
| 今日 / 7 日 / 30 日 / 历史活跃 | `last_seen_at`（observed） | 自然日窗口内有上报 | 按设备 |
| 分游戏 7/30/历史活跃 | `last_seen_at` | 同上，按游戏分组 | 按 (游戏, 设备) |
| verified（认证活跃） | `card_verified_at` | 窗口内 login / 带 token 心跳成功过 | 按设备；**游客永远不计入** |
| unverified | observed − verified | 有上报但本窗口内没有认证成功 | 由前两者相减，互斥 |
| 今日激活 | `auth_events` `event_type='activate'` | 首次激活事件 | 事件条数 |
| 卡密总数/激活/未激活/封禁 | `card_keys.status` | — | 卡数 |

窗口值来自 `ONLINE_WINDOW_SECONDS`（默认 360，必须 > 心跳周期 300），看板接口返回 `online_window_seconds` 供前端展示。

## 4. 可疑数据（一律丢弃）

判定在 `app/services/trust.py`，命中即**不写在线表、不写事件明细**，只在本机累计次数：

| 原因码 | 判据 | 默认 |
|---|---|---|
| `bad_device` | 设备号不是 `dev_+32/40 位小写十六进制` | 恒开 |
| `bad_cert` | 上报证书 SHA-256 不在 `CLIENT_CERT_SHA256` 白名单 | 白名单为空 ⇒ 跳过 |
| `old_client` | `client_version_code` < `CLIENT_MIN_VERSION_CODE` | 0 ⇒ 跳过 |
| `ip_burst` | 同 IP 一小时内首次出现的设备数超上限 | 0 ⇒ 跳过 |

丢弃只影响统计，**不影响接口返回与授权/功能**。

留档（不展示在看板）：
- 进程内实时计数：`GET /v1/admin/dropped-stats`（管理员令牌，侧栏无入口）；
- 落盘累积：`data/dropped_stats.jsonl`，每行 `{ts, by_reason, by_game, total, delta}`，
  每 60 秒或累计 200 次写一次；**只有原因码与次数，没有设备号/IP/请求体**。

## 5. 设备日志（后台「设备日志」tab）

| 事件 | 含义 | 身份 |
|---|---|---|
| `login_ok` / `login_fail` | 卡密登录成功 / 失败 | card |
| `heartbeat_fail` | 心跳被拒（会话失效/设备变更等） | card |
| `activate` | 卡密首次激活 | card |
| `device_migrate` / `heartbeat_device_drift` | 设备绑定迁移 | card |
| `unbind` / `unbind_fail` | 解绑成功 / 失败 | guest（解绑成功后本机回到游客） |
| **`guest_seen`** | **游客出现（同设备同游戏 24 小时最多 1 条）** | **guest** |

筛选项：设备号（精确）、卡内部 ID、**身份（全部/卡密/游客）**、事件、结果、时间范围。
设备详情弹窗的在线表同样带「身份」列，在线判定与看板同窗口。

## 6. 已知限制（必须一起读）

1. **设备数 ≠ 人数**：同一人多台设备算多台；同机多包若算出不同 `dev_*` 也算多台。
2. 快照口径：`online_presence` 只保留最后一次上报时间，**不能还原任意历史时点**；7/30 日是「窗口内是否上报过」，不是严格 DAU。
3. 时钟：服务端按 UTC 存储，展示按北京时间（`STATS_TZ`）；自然日切分用北京时间。
4. 进程重启会清空内存态：设备漂移候选、失败聚合窗口、`ip_burst` 计数、丢弃计数的「实时值」（落盘值不丢）。
5. 未登记 `game_id` 的上报不写在线表、不签票（客户端功能锁死是预期行为）。

## 7. 改口径的最小检查清单

1. 先改本文件的口径表；
2. 同步 `ONLINE_WINDOW_SECONDS` 与客户端心跳周期（心跳 300 ⇒ 窗口 ≥ 360）；
3. 跑 `python -m unittest discover -s tests`（需 `GRANT_RSA_KEY_PATH` 指向测试密钥）；
4. 后台确认：总在线 = 卡密 + 游客；设备日志能筛出 `guest_seen`；`dropped-stats` 有数但侧栏没有入口。
