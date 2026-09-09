# 星光魔法 MOD 菜单 — 代码对接与问题清单

**日期：** 2026-08-07（最后更新：2026-08-08 v1.6）  
**项目路径：** `/Users/aliang/Documents/nixiang/mod菜单/starry-mod-menu/`

---

## 〇、现状摘要

| 层级 | 状态 |
|------|------|
| 菜单主框架（穿透/动态列表/偏好/设置隔离） | ✅ 已落地（原 11 项） |
| Unity IL2CPP 公共层 `Unity/Il2CppApi.*` | ✅ 已抽出 |
| 战魂铭人 `Games/ZhmrMod.cpp` | ✅ 已对接，功能可用 |
| 悬浮层 / OEM「挡操作」 | ✅ 始终全屏 OVERLAY + hit 区穿透；不再走「改小窗口」方案 |
| 网络验证 / 卡密 / 授权短票 | ✅ 真实对接（非 UI 演示） |
| Cocos 特征码 / Unreal 专用 Hook | ⏸ 有意保留（不阻塞当前样板） |

### 原框架 11 项（均已完成）

触摸穿透、偏好单写、C 层动态功能列表、偏好补全、死代码删除、Unreal 说明+Generic 兜底、文档、仅 SP、featNum≥50 不进 C、JDK17、设计页不打包。

### 战魂实战补齐

- `mod_thread` 已接 `Zhmr_TryInit` / `Zhmr_Tick`（不再是空壳）
- 等 `libil2cpp` 再认引擎；**禁止** Dobby 内联短导出桩 `il2cpp_runtime_invoke`
- HybridCLR 字段 bias=+8；战斗堆扫描，货币/解锁 setter 直调

### 网络验证与菜单收尾（已完成）

- 卡密登录 / 心跳 / 自助解绑 / 退出登录（退出后在线状态回游客）
- 公告 `action_url` 跳转；`popup_mode`：`every_open` / `once_device`
- 设置「定制 MOD」复制 QQ（站点配置 `custom_mod_qq`）
- 关闭菜单前 `lockAllFeatures` + 取消授权

### 仍有意不做 / 可选后续

- Cocos KittyScanner 通用扫 so / Unreal 专用 Hook
- 管理后台锦上添花：见 `auth-server/docs/ADMIN_BACKLOG.md`

---

## 一、适配下一款 Unity 游戏

1. `bash scripts/new_game_mod.sh <Game>` 生成四件套（Mod + Entry + 私有 mk）  
2. `Games/{Game}Mod.cpp` 用 `Il2Cpp::GetApi()`（Load / FindClass / FieldOffset / ScanInstances / runtime_invoke 直调）  
3. `Games/{Game}Entry.cpp` 填注册表：`authGameId` / `waitLib` / `featureList` / 初始化参数  
4. **先 `nm` 看导出符号体积**，再决定能否 inline hook；审查红线=共享文件（MainModern/Main/GameConfig/公共 mk）必须零改动  

详见 `ARCHITECTURE.md` §4.4。
