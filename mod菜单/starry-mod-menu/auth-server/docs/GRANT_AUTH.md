# 授权短票（防破解）— 给人看 + 给 AI 改代码看

> **你只要记住 3 句话**  
> 1. 玩家登录成功后，服务器发一张「短期通行证」（叫 grant）。  
> 2. 菜单里的游戏功能，必须这张票验过才开；只改 `authorized=true` 没用。  
> 3. 正常登录的玩家，功能和以前一样，不用额外操作。

---

## 一、给你自己的最简操作（日常）

### 日常发版（按顺序做）

1. **先更新服务器**（auth-server）  
   ```bash
   cd /www/wwwroot/starlight-auth/auth-server   # 路径按你实际改
   docker compose up -d --build
   ```
2. **再打手机 Release 包**（仓库一键脚本，**默认就是 release**）  
   ```bash
   ./scripts/inject_zhmr.sh              # 默认 assembleRelease + R8
   # STARRY_BUILD_TYPE=debug ./scripts/inject_zhmr.sh   # 仅联调，勿上线
   ```  
   - 调试用的 debug 包防 AI 较弱，对外请用 **release**（见 [INJECT_GUIDE.md](../../INJECT_GUIDE.md)）。
3. 玩家 **重新登录一次卡密**（旧会话没有新票）。

### 你几乎不用改的配置

`.env` 里有（没有就抄 `env.example`）：

```env
GRANT_HMAC_SECRET=sg-grant-v1-7f3a9c2e
OFFLINE_CACHE_SECONDS=86400  # 2026-09-06 起：票 TTL=离线宽限 1 天
CARD_LOGIN_MAX_FAILS=12
CARD_LOGIN_WINDOW_SECONDS=600
```

**默认密钥已经和 APK 对上了。** 不改密钥就不用动 native。

### 什么时候才要改密钥？

只有你觉得密钥泄露了、想换一把时：

| 步骤 | 做什么 |
|------|--------|
| 1 | 改服务器 `.env` 的 `GRANT_HMAC_SECRET=新密钥` |
| 2 | 改手机工程 `app/src/main/jni/Main.cpp` 里 `OBFUSCATE("旧密钥")` → 改成 **同一串新密钥** |
| 3 | 服务器 `docker compose up -d --build` |
| 4 | 重新打 **release** APK，让玩家更新 |

两边字符串必须完全一样，否则所有人登录后功能都开不了。

### 出问题怎么排查（最简）

| 现象 | 多半原因 | 你怎么做 |
|------|----------|----------|
| 登录成功但功能开不了 | 服务器没更新 / 没重新登录 / 密钥不一致 | 先更新服务器 → 玩家重登；仍不行检查密钥是否两边一致 |
| Toast「未下发授权票」 | 服务器还是旧代码 | 部署带 `grant` 的 auth-server |
| 关了卡密仍不能用 | bootstrap 没带 `device_id` 或没发 open 票 | 确认客户端已更新；bootstrap URL 带 device_id |

---

## 二、这套东西在干什么（白话）

以前：客户端里有个开关叫「已授权」。AI 一搜 `authorized`，改成 true，功能就开了。

现在：

```
服务器签字发票 (grant)
        ↓
Java 存起来，交给 Native
        ↓
Native 验签字 + 是否过期 + 是否这个设备/游戏
        ↓
通过 → 游戏修改 Tick / 开关才真正生效
```

- `g_authorized` / `setAuthorized(true)` 变成**诱饵**，单独改它不够。  
- 真正放行看 Native 里的 `session_live()`（短票有效）。  
- 票 TTL = `OFFLINE_CACHE_SECONDS`（2026-09-06 起 86400=1 天，离线宽限窗口）。
- **2026-09-06 起无周期心跳**：客户端冷启动对账一次换新票；进程内会话（`session_live`）本地永不过期，功能放行不看票剩余时间。

**合法玩家**：冷启动对账成功 → 票保鲜 → 功能正常（进程内一直有效，直至重启）。  
**乱改包的人**：没有服务器私钥签的票 → 功能起不来（或只能硬改 so，成本高很多）。

---

## 三、给 AI / 后续开发者：怎么改代码（结构化）

### 3.1 票长什么样

```
1|{device_id}|{game_id}|{exp_unix}|{menu}|{card}|{kid}.{hmac_hex}
```

例：

```
1|dev_abc...|zhmr|1710000180|1|1|a1b2c3d4.9f3e...
```

| 段 | 含义 |
|----|------|
| `1` | 版本号 |
| device_id | 设备指纹（见下方 §3.1.1） |
| game_id | 游戏 |
| exp | 过期时间（unix 秒） |
| menu | `1` 游戏启用 / `0` 停用（0 则 Native 拒票） |
| card | `1` 需卡密 / `0` 关卡密 open 模式 |
| kid | token 的 sha256 前 8 位，或 `open` |
| hmac | HMAC-SHA256(密钥, 点号前整段 body) 的 hex |

签发：`auth-server/app/services/grant_service.py` → `mint_grant()`  
密钥：`Settings.grant_hmac_secret` / 环境变量 `GRANT_HMAC_SECRET`

#### 3.1.1 `device_id` 指纹（全游戏卡密）

服务端按**整张卡**绑定一个 `card.device_id`（与 `game_id` 无关）。客户端必须在**同机不同宿主包**上算出同一个 `dev_*`，否则会 403「卡密已绑定其他设备，请先解绑」。

| 版本 | 算法 | 问题 |
|------|------|------|
| v1 | `ANDROID_ID` + Widevine | 包名隔离 → 换游戏必换设备 |
| v2 | Widevine + `Build.*` | 部分 OEM（ColorOS 等）Widevine 仍按应用隔离 |
| v3 | 仅 `Build.*`/`FINGERPRINT`；共享文件尽力写但不覆盖 | 系统 OTA 改变 `FINGERPRINT` 后整套 ID 漂移 |
| v4 | 共享文件优先，缺失时回落稳定 `Build.*` | 外部存储不可读时仍会生成第二套 ID |
| **v5**（当前） | **只用 SSAID**（`Settings.Secure.ANDROID_ID`） | 同签名、同系统用户跨包一致；清数据/恢复出厂/换签名才变化 |

实现：`app/.../auth/DeviceFingerprint.java`。  
经验卡片：`10_docs/_templates/经验_卡密设备指纹跨包一致.md`。

v5 客户端首次登录会同时带上旧 prefs、旧共享种子、v4 Build 哈希作为
`legacy_device_ids`。服务端命中旧绑定后把主绑定原子迁移到 v5，并把旧 ID
保留在 `card_keys.legacy_device_ids`，因此升级期间无需用户手动解绑，旧客户端也不会
因为主绑定已经迁移而立即全部失效。没有合法 legacy 证据的陌生设备仍返回原 403。

SSAID 不可用（为空、非法或已知伪造值）时客户端显式报错，不再静默回落到随机 ID
或共享文件，避免重新制造“一台机器两套 ID”。

### 3.2 谁在什么时候发票

| 接口 | 文件 | 何时带 `grant` |
|------|------|----------------|
| `POST /v1/auth/login` | `auth_service.login_card` | 登录成功 |
| `POST /v1/auth/heartbeat` | `auth_service.heartbeat` | 冷启动对账成功（不再周期调用） |
| `GET /v1/bootstrap?game_id=&device_id=` | `bootstrap_with_grant` | **仅** `card_auth_enabled=false` 时发 open 票 |

响应 `data.grant`（bootstrap 则在 `data.grant`）。

### 3.3 客户端链路（改门控时跟这条走）

```
NativeMenuController
  展开面板 / 功能被拦 → maybeRenewGrant
    → POST /v1/auth/heartbeat 或 GET /v1/bootstrap（开放模式）
    → Preferences 存新 grant → syncNativeAuthorized
    → 硬失败则 clearAuthSession + lockAllFeatures
        ↓
ModBridge.java
  Preferences 存 auth_grant
  syncNativeAuthorized()
    → nativeApplyGrant(grant, deviceId, gameId)
        ↓
Main.cpp
  apply_grant_utf8() 验 HMAC
  session_live() == true 才允许：
    - Tick 循环里的游戏修改
    - nativeSetFeature / SetValue / SetText / ExecButton 开启类操作
```

**诱饵（可被改但不单独放行）：**

- JS/Java：`setAuthorized` / `g_authorized` / `AuthState.authorized`
- Native：`nativeSetAuthorized(true)` **不会**单独把 `g_rt_ok` 打开

**真门禁：**

- Native：`session_live()`（依赖 `g_rt_ok` + 未过期）
- Java：`requireAuthForFeature` 还会看 `nativeHasLiveSession()`

### 3.4 改功能时别踩的坑

1. **不要**再只靠 `if (g_authorized)` 放行游戏逻辑；新逻辑用 `session_live()`。  
2. **不要**在 JS 里伪造 grant；没有服务器密钥签不出来。  
3. 改 `OFFLINE_CACHE_SECONDS`：服务器下发；客户端还卡了上限 600 秒。  
4. 改密钥：必须同时改  
   - `auth-server`：`GRANT_HMAC_SECRET` / `config.py` 默认  
   - `app/.../Main.cpp`：`grant_pepper()` → `OBFUSCATE("...")`  
5. UI 设置（featNum ≥ 50）仍不需要票。  
6. 游戏具体修改代码（`Zhmr_Tick` 等）**不用改**；只是外面多包了一层「有票才进 Tick」。

### 3.5 相关文件清单（给 AI 搜索用）

| 用途 | 路径 |
|------|------|
| 签发/校验（服务端） | `auth-server/app/services/grant_service.py` |
| 登录/心跳挂 grant | `auth-server/app/services/auth_service.py` |
| bootstrap + device_id | `auth-server/app/routers/client.py` |
| 配置 | `auth-server/app/config.py`, `env.example` |
| 卡密登录限流 | `auth-server/app/security_guard.py` |
| 存票 | `app/.../menu/Preferences.java`（`auth_grant`） |
| 桥接 | `app/.../bridge/ModBridge.java`（`nativeApplyGrant` 等） |
| 验票 + Tick 门禁 | `app/.../jni/Main.cpp`（`session_live` / `apply_grant_utf8`） |
| HMAC 实现 | `app/.../jni/Includes/hmac_sha256.c` |
| `game_id` 编译入口 | `Android.mk` 的 `STARRY_GAME_*` → `GameConfig.h` 映射（见 `SWITCH_GAME.md`） |
| 菜单续票 | 原生 `NativeMenuController.maybeRenewGrant` |
| R8 | `app/build.gradle` release `minifyEnabled true`, `proguard-rules.pro` |

### 3.6 自测清单（改完跑一遍）

- [ ] 登录成功 → logcat 有 `[验证] grant ok`  
- [ ] 功能开关能开，游戏修改生效  
- [ ] 后台封禁卡密 → 客户端下次冷启动对账被拒 → 踢回登录（运行中不断，见 auth-chain）  
- [ ] 只 Frida/`nativeSetAuthorized(true)` → **功能仍不应开**  
- [ ] 关卡密 + 带 device_id 的 bootstrap → 有 open grant → 功能可用  

---

## 四、和「菜单功能文档」的关系

- 菜单里有哪些开关、怎么改游戏：仍看仓库根目录 `功能说明文档.md` / `MENU_CATALOG.md`。  
- **验权、短票、防破解**：只看本文 + `SECURITY.md`。  
- 改菜单功能列表时：**不用动短票逻辑**；只要玩家仍能正常登录拿票，修改逻辑照旧。
