# 授权链路

> 📍 `skill/starry-mod/auth-chain.md`

---

## 登录永不过期（当前架构）

**核心原则：登录后只要不重启 App，功能就一直可用。**

- 后台禁卡/删卡/关游戏/关单项功能 → 运行中不影响已开功能
- 唯一断功能路径：进程重启（重新登录）
- 心跳仍运行但只做两件事：同步卡密信息 + 刷新 bootstrap 配置

---

## Native 门控

### session_live()
```c
return g_rt_ok;  // 登录后永不过期
```
- `g_rt_ok` 在 `apply_grant_utf8()` 成功后设 true，永不自动变 false
- 仅进程重启 / `nativeClearGrant` 时清零
- Tick 线程永不进入 ForceClear 分支

### apply_grant_utf8()
解析 grant `1|device|game|exp|menu|card|kid.hexsig`：
- HMAC-SHA256 验签（pepper=`sg-grant-v1-7f3a9c2e`）
- 验 device/game/exp
- **首次验签检查短票过期** `if (now > exp + 45) return false`（不能接受已过期票登录）
- 成功后 `g_rt_ok = true`

---

## Java 门控

| 函数 | 逻辑 |
|------|------|
| `requireAuthForFeature` | 只看 `hasLiveNativeSession()` |
| `featureInteractable` | 只看 native session |
| `refreshAuthStatus` | 以 native session 为准显示"授权有效/未授权" |
| `applyLocalGrantOrKickIfDead` | 只刷新 UI，不踢 |
| `maybeRenewGrant` | 心跳只同步状态，失败不清功能不踢登录 |

---

## 后台控制的两个边界

| 场景 | 运行中（心跳同步） | 重启游戏后 |
|------|-------------------|-----------|
| 后台关单项功能 | 不影响已开功能（UI 变灰但 C 层不动） | UI 置灰 + C 层关掉（`enforceServerFeatureFlags`） |
| 后台关游戏 | 只提示"维护中"、不锁功能 | `lockAllFeatures` 全关 |
| 卡密失效/被删 | UI 显示"未授权"、功能不断 | 需重新登录 |

### 关键设计
- `applyBootstrapAuthGates` 只在首次 bootstrap（重启后首次展开菜单）走一次
- 心跳 60s 不走 `applyBootstrapAuthGates` → 运行中不会因后台关游戏断功能
- `enforceServerFeatureFlags` 在 bootstrap 和登录成功时调用，心跳里不调

---

## 心跳机制

- 后台定时器固定 60s
- 成功：更新 bootstrap json → `refreshFeatures()` 重建 UI
- 失败（401/403）：只 `refreshAuthStatus()` 显示"未授权"，功能不断
- 网络超时：软失败，等下次心跳
- grant 为空：log + return，功能继续

---

## 卡密设备指纹

**问题**：服务端 `card.device_id` 整卡绑一个（与 game_id 无关）。跨包/清机后 device_id 变化 → 403。

### 解决方案
- v4 去掉 OTA 变化的字段，用可读共享种子
- 种子优先路径：`/sdcard/Android/starry_magic` → `.starry_magic` → `/data/local/tmp/starry_magic`
- 无存储权限宿主（targetSdk≥30）→ su 旁路 + 种子落 `/data/local/tmp/starry_magic`（666）

### 装机必须
```bash
bash 04_scripts/_common/ensure_starry_device_id_seed.sh
```

详见经验卡：`10_docs/_templates/经验_卡密设备指纹跨包一致.md` 和 `10_docs/_templates/全游戏卡密_复现路径与防再发.md`
