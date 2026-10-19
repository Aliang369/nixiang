# 授权链路

> 📍 `skill/starry-mod/auth-chain.md`

---

## ⛔ 排查前置条件（2026-10-01 新增，先读这条）

**授权链路不是「遇到问题就查」的排查项，它有硬前置条件。**

1. **账号必须先登录。** 星空授权需要账号输入，这是人工步骤 —— 让用户在游戏内**手工输入账号并完成登录**，不要自己去抓登录/授权流程。
2. **未登录时不要排查授权。** 未登录状态下「未授权」是预期结果，不是 bug；此时抓 token / 查 `grant` / 翻 native 门控全是白工。
3. **登录之后仍然显示未授权，才启动排查。** 这时才按本文下面各节逐层查 `nativeApplyGrant` / `session_live` / 冷启动对账 / 证书固定。
4. **除非用户明确要求，不要主动查授权链路。** 用户说「已自行登录完成」「不要查授权链路」时，立刻停止该方向，把时间放在交付物上。

> 反例（2026-10-01 失落城堡）：用户未登录前我自行追了一轮授权链路，直到用户明说「已自行输入账号完成星空授权，不要再查授权链路」才停 —— 这段调用全部无效。

判断「已登录」的日志锚点：`StarryMagic: [验证] grant ok exp=…`（有 `grant ok` 即已授权）；未授权时不会有该行。

---

## 登录永不过期（当前架构）

**核心原则：登录后只要不重启 App，功能就一直可用。**

- 后台禁卡/删卡/关游戏/关单项功能 → 运行中不影响已开功能
- 唯一断功能路径：进程重启（重新登录）
- **2026-09-05 起：60s 周期心跳已删除**，改为冷启动一次对账（见「冷启动对账机制」）；运行中不再有任何定时续票/重推

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
- **验签检查短票过期** `if (now > exp + 45) return false`（每次 apply 都查；票 TTL = 服务端 `offline_cache_seconds`，2026-09-05 起为 86400s，即离线宽限 1 天）
- 成功后 `g_rt_ok = true`

### nativeApplyGrant() 续票失败契约（2026-09-05）
- apply 被拒且会话**本来是活的** → 只记日志 `grant rejected (session kept)`，**不清会话不清功能**，等下次续票
- apply 被拒且会话本来就是死的 → 维持 `clear_runtime_session()`（新登录失败路径）

---

## Java 门控

| 函数 | 逻辑 |
|------|------|
| `requireAuthForFeature` | 只看 `hasLiveNativeSession()` |
| `featureInteractable` | 只看 native session |
| `refreshAuthStatus` | 以 native session 为准显示"授权有效/未授权" |
| `applyLocalGrantOrKickIfDead` | 只刷新 UI，不踢 |
| `maybeRenewGrant` | 续票/对账：失败不清功能不踢登录（冷启动权威判定除外，见下） |

---

## 后台控制的两个边界

| 场景 | 运行中（无心跳，2026-09-05 起） | 重启游戏后（冷启动对账） |
|------|-------------------|-----------|
| 后台关单项功能 | 不影响已开功能（UI 变灰但 C 层不动） | UI 置灰 + C 层关掉（`enforceServerFeatureFlags`） |
| 后台关游戏 | 只提示"维护中"、不锁功能 | `lockAllFeatures` 全关 |
| 卡密失效/被删/封禁 | UI 状态不变、功能不断（无定时对账） | 权威判定被拒 → `hardKickAuth` 踢回登录 |

### 关键设计
- `applyBootstrapAuthGates` 只在首次 bootstrap（重启后首次展开菜单）走一次
- 冷启动对账不走 `applyBootstrapAuthGates` → 运行中不会因后台关游戏断功能
- `enforceServerFeatureFlags` 在 bootstrap 和登录成功时调用，对账里不调

---

## 冷启动对账机制（2026-09-05 替代 60s 心跳）

- **时机**：进程内只对一次账。菜单 `show()` / `fetchBootstrapOnce` 成功触发 `startGrantBgRenew()`；3s 后首跑，网络失败 30s×5 次有界重试；成功/权威硬拒/重试耗尽即停（`grantRenewCycleDone` 每进程一次，hide/show 菜单不重开）
- **登录成功不触发对账**（服务端刚签发新鲜 grant）
- **成功**：同步新 grant 进 native；仅当"会话从死变活"（存档票过期的冷启动）才 `reapplySavedFeaturesToNative()` 恢复开关；会话本来就活时**零重推**——旧 60s 心跳的全量重推是"玩久了功能失效"的头号嫌疑链，已删除
- **权威硬拒（冷启动才踢）**：`isHardAuthFailureJson` 关键字命中（会话/封禁/过期/设备/不存在/用尽）→ `hardKickAuth` 清存档会话回登录页；这是封禁/删除/解绑的生效点
- **宽松失败形态不踢**：`looksLikeAuthFailure` 命中（任意 detail/error，含网关 404/429/限流/维护页）→ 只刷 UI，冷启动交给有界重试。审查修正点：AuthClient 对非 2xx 返回原始响应体，FastAPI/网关的 detail 不能当封禁判定
- **网络超时**：软失败，有界重试后放弃；存档票（TTL 86400s）在宽限期内可直接验签放行，断网重开 1 天内可玩
- **菜单展开 `maybeRenewGrant("expand")` 保留**：15s 节流，作为手动兜底续票；硬拒只刷 UI 不踢
- **logcat 诊断**：`renew ok (cold-start)` / `renew auth fail (..., authoritative=...)` / `cold-start renew scheduled`；旧 `heartbeat ok (bg-heartbeat)` 日志已不存在

## HTTPS 证书固定来源（2026-09-06 起）

- 固定公钥 `starlight.crt` **编译期烘焙在 `libstarry_magic.so`**（`jni/CertPinned.h`），`AuthClient.init()` 经 `ModBridge.nativeGetPinnedCert()`（RegisterNatives 19/19 之一）取 PEM 字节，不再读 `assets/certs/starlight.crt`
- native 未加载 / 烘焙缺失时 **fail-closed**：`request()` 对 HTTPS 一律拒绝（`HTTPS aborted (no pinned cert)`），不退回系统信任、不闪退
- 自测证据：星空魔法 SELF 包 logcat `pinned cert loaded from native so: CN=47.100.216.101` + bootstrap/heartbeat 200（2026-09-06，f9a116a2）；真实游戏外科包回归见[经验卡](../../10_docs/_templates/经验_证书嵌入so_20260906.md)
- **换证书须重编 so**：`xxd -i ../assets/certs/starlight.crt > CertPinned.h`（命令见头文件注释），只换 assets 无效；存量旧包仍读 assets，服务端已兼容

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

---

## 机器恢复（card_key 直登，无需走 App UI）

冷启/重装后要恢复授权会话，不必走登录 UI，直接 curl 换 token/grant 再 seed（实测于霓虹深渊2 r22，2026-10-19）：

```bash
CARD='AL-XXXX-XXXX-XXXX-XXXX'; GAME='<game_id>'; DEV='<device_id>'
curl -sk https://47.100.216.101:8443/v1/auth/login \
  -H 'Content-Type: application/json' \
  -d "{\"card_key\":\"$CARD\",\"game_id\":\"$GAME\",\"device_id\":\"$DEV\"}"
```

- 响应体取 `data.token` + `data.grant`（grant 形如 `1|device|game|exp|menu|card|kid.hexsig`，HMAC-SHA256 验签，pepper 见上文「apply_grant_utf8」）。
- 拿到后 seed（跨游戏务必带 `--pkg`，脚本默认 `com.veewo.neonabyss2`）：
  ```bash
  bash 04_scripts/_common/starry_prefs_edit.sh --seed-auth \
    --grant '<上一步 data.grant>' --token '<data.token>' --card "$CARD" \
    [--exp '<ISO 时间>'] [--last-ok <毫秒时间戳>] [--launch --verify]
  ```
- 功能开关恢复：`--seed-features 5,7,8,9,10,11,12`（槽位号照 `10_docs/{游戏}/{id}_menu_template.json`）。
- 卡密 / device_id / game_id 取值见 `10_docs/{游戏}/硬约束.md`；game_id 跨渠道独立（见 `SKILL.md`「分发与装机红线」）。
