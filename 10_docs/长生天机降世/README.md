# 长生：天机降世

| 项 | 值 |
|----|-----|
| 包名 | `com.netkuu.changsheng.tjjs` |
| 真机官方版本 | **1.0.2 / vc1020** |
| 星光 `game_id` | `cstjjs` |
| 官方底包 | `02_apks/长生天机降世/长生天机降世_1.0.2_official.apk` |
| 正式包 | `08_output/长生天机降世/长生天机降世_1.0.2_surgical-releaseSigned.apk`（**release**，非 debug） |
| 当前状态 | 已按授权普通卸载重装；启动、hook、`Code.dll` MI、菜单服务通过，战斗内功能待验 |
| 当前状态 | 已按授权普通卸载重装；启动、hook、`Code.dll` MI、菜单服务通过，战斗内 `十倍伤害` / `无敌` 实测通过 |

## 文档

| 文档 | 用途 |
|------|------|
| [星光落地_1.0.2_20260929.md](./星光落地_1.0.2_20260929.md) | **当前**：1.0.2 真机底包、挂点重定位、外科包与门禁结果 |
| [星光落地_1.0.1_20260908.md](./星光落地_1.0.1_20260908.md) | 1.0.1 历史复刻记录 |
| [星光落地_0.9.10_20260818.md](./星光落地_0.9.10_20260818.md) | 0.9.10 外科包、UnityApplication 原地注入 |
| [星光落地_0.9.9_20260813.md](./星光落地_0.9.9_20260813.md) | 旧版 inject（apktool，勿再对游戏整包 b） |
| [挂点实锤_0.9.10_20260818.md](./挂点实锤_0.9.10_20260818.md) | EnterFrameFromInterpreter 与 `RoleValue_Health` 证据 |
| [BUFF跟读_20260813.md](./BUFF跟读_20260813.md) | 斌哥壳差分 → RoleSide / delta |

## 功能

| 菜单 | feat_num | 实现 |
|------|----------|------|
| 十倍伤害 | 1 | `RoleValue_Health` 且 `RoleSide==2` 时负 delta ×10 |
| 无敌 | 2 | 同挂点；`RoleSide==1` 且负 delta → 0 |

## 复现构建

```bash
cd mod菜单/starry-mod-menu
# Android.mk：ENABLE_UNITY_HOOK + STARRY_GAME_CSTJJS
bash scripts/surgical_cstjjs_1.0.2.sh
# 装机前先确认异签名卸载授权；脚本默认不卸载
```

热更 `Code.dll` 未进域前 Init 会重试；进战斗后 `Tick` 继续 `TryInit`。签名不一致时不得自动卸载或清数据。
