# 长生：天机降世

| 项 | 值 |
|----|-----|
| 包名 | `com.netkuu.changsheng.tjjs` |
| 真机官方版本 | **1.0.3 / vc1030** |
| 星光 `game_id` | `cstjjs` |
| 官方底包 | `02_apks/长生天机降世/长生天机降世_1.0.3_official.apk` |
| 正式包 | `08_output/长生天机降世/长生天机降世_1.0.3_surgical-releaseSigned.apk`（**release**，非 debug） |
| 当前状态 | 1.0.3 已按授权普通卸载重装；启动、hook、`Code.dll` MI、菜单服务通过，战斗内 `十倍伤害` / `无敌` 引擎层实测命中 |

## 文档

| 文档 | 用途 |
|------|------|
| [星光落地_1.0.3_20261014.md](./星光落地_1.0.3_20261014.md) | **当前**：1.0.3 真机底包、挂点增量复核、外科包与门禁结果 |
| [星光落地_1.0.2_20260929.md](./星光落地_1.0.2_20260929.md) | 1.0.2 复刻记录（上期） |
| [星光落地_1.0.1_20260908.md](./星光落地_1.0.1_20260908.md) | 1.0.1 历史复刻记录 |

## 功能

| 菜单 | feat_num | 实现 |
|------|----------|------|
| 十倍伤害 | 1 | `RoleValue_Health` 且 `RoleSide==2` 时负 delta ×10 |
| 无敌 | 2 | 同挂点；`RoleSide==1` 且负 delta → 0 |

## 复现构建

```bash
cd mod菜单/starry-mod-menu
# Android.mk：ENABLE_UNITY_HOOK + STARRY_GAME_CSTJJS
# 切换游戏时须注释掉其它 STARRY_GAME_* 分支，否则 CFLAGS 累加 + SRC_FILES 覆盖会编译失败
bash scripts/surgical_cstjjs_1.0.3.sh
# 装机前先确认异签名卸载授权；脚本默认不卸载
```

热更 `Code.dll` 未进域前 Init 会重试；进战斗后 `Tick` 继续 `TryInit`。签名不一致时不得自动卸载或清数据。
