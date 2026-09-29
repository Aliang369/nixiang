# 星光魔法验证 · 对接文档

Base URL：`http://<服务器IP>:8000`  
交互式文档：`/docs`

统一响应：

```json
{ "ok": true, "message": "登录成功", "data": { } }
```

错误时 `ok=false`，HTTP 状态码非 2xx。

---

## 客户端接口

### 1. 登录 `POST /v1/auth/login`

```json
{
  "card_key": "SG-XXXX-XXXX-XXXX-XXXX",
  "game_id": "starry_self",
  "device_id": "v5 设备唯一ID",
  "legacy_device_ids": ["旧 v4/v3 设备ID"]
}
```

`legacy_device_ids` 可选，仅用于 v4 → v5 自动迁移。服务端只有当任一候选命中
`card_keys.device_id` 或该卡已保留的旧绑定时才允许迁移，否则仍返回原 403。
迁移成功后旧 ID 会保存在 `card_keys.legacy_device_ids`，供过渡期旧客户端继续登录。

成功 `data`：

- `token`：会话令牌
- `expires_at`：会话过期时间（7 天；每次冷启动对账自动续期）
- `offline_cache_seconds`：grant 短票 TTL = 离线宽限秒数（现值 86400=1 天）
- `card`：卡密公开状态（到期、次数、可否解绑等）
- `bootstrap`：公告 / 功能开关 / Toast 文案
- `grant`：**授权短票**（HMAC；客户端 Java/Native 验签后才放行游戏功能）

短票说明（最简操作 + 改代码指引）：[GRANT_AUTH.md](./GRANT_AUTH.md)

规则摘要：

- 单设备绑定；已绑其他设备且未命中合法 legacy 候选则拒绝
- 首次激活按类型写 `expire_at`（永久/次数可为 null）
- `max_uses > 0` 或 `duration_type=count`：**每次登录扣 1 次**
- `scope=global` 可用于任意 `game_id`；`scope=game` 仅匹配卡上的 `game_id`

### 2. 心跳 `POST /v1/auth/heartbeat`

```json
{
  "token": "...",
  "game_id": "starry_self",
  "device_id": "..."
}
```

用于会话续期（+7 天）、卡密状态对账，并下发新的 `grant`。
**2026-09-06 起不再周期调用**：客户端改为冷启动（进程启动后）调用一次；网络失败有界重试后放弃，功能放行不受影响（native 会话本地永不过期）。硬失败（封禁/删除/过期/解绑）→ 冷启动时踢回登录。

### 3. 解绑 `POST /v1/auth/unbind`

```json
{
  "card_key": "...",
  "device_id": "当前绑定设备ID"
}
```

**2026-09-06 起不限次数**：首次免费；之后每次从剩余有效期**扣除 3 天**（剩余不足 3 天照扣，扣完即过期；无到期时间的永久/次数卡不扣）。
仅当前绑定设备可解绑（403 保护）；管理员强制解绑（`force_unbind`）免费且不占首次额度。

### 4. 启动配置 `GET /v1/bootstrap?game_id=zhmr&device_id=...`

未登录也可拉公告等公开配置。建议带 `device_id`（关卡密时才能发 open 短票）。

响应额外字段：

- `startup_toasts`：开屏 Toast 队列（`[{content,type}]`，按排序依次弹出；空则启动不弹）
- `card_auth_enabled`：游戏是否要求卡密（`false` 时客户端隐藏账号 Tab，功能仅需联网拉取配置）
- `menu_enabled`：游戏是否启用
- `offline_cache_seconds`：离线缓存建议秒数
- `grant`：仅当 `card_auth_enabled=false` 且请求带了 `device_id` 时下发

---

## 管理端接口

均需 Header：`Authorization: Bearer <admin_token>`（登录接口除外）。

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/v1/admin/login` | 管理员登录 |
| GET | `/v1/admin/dashboard` | 看板 |
| GET/POST/PATCH | `/v1/admin/games` | 游戏 |
| GET/POST/PATCH/DELETE | `/v1/admin/cards` | 卡密 |
| GET/POST/PATCH/DELETE | `/v1/admin/announcements` | 公告 |
| GET/POST/PATCH/DELETE | `/v1/admin/features` | 功能门控 |
| GET/POST/PATCH/DELETE | `/v1/admin/toasts` | Toast |

### 生成卡密 `POST /v1/admin/cards`

```json
{
  "count": 10,
  "scope": "game",
  "game_id": "starry_self",
  "duration_type": "day",
  "max_uses": 0,
  "note": "测试批次",
  "prefix": "SG"
}
```

`duration_type`：`hour|day|month|year|permanent|count`  
次数卡必须 `max_uses > 0`。

### 修改卡密 `PATCH /v1/admin/cards/{id}`

可改 `expire_at`、`status`、`max_uses`、`used_count` 等。  
`force_unbind: true` 清除设备绑定；`reset_unbind: true` 重置自助解绑次数。

---

## 客户端建议流程

1. 冷启动 → 一次性对账（`/v1/auth/heartbeat`）：成功则续票+刷新状态；网络失败有界重试后放弃
2. 权威硬拒（封禁/删除/过期/解绑）→ 踢回登录；网络失败不影响已登录功能（会话本地不过期）
3. 登录成功 → 按 `bootstrap.features` 过滤功能开关  
4. Toast 文案优先用 `bootstrap.toasts[key].content`  

`device_id` 建议：首次生成 UUID 写入本地，不要只用易变字段。
