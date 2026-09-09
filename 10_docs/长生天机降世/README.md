# 长生：天机降世

| 项 | 值 |
|----|-----|
| 包名 | `com.netkuu.changsheng.tjjs` |
| 版本 | **0.9.10 / vc200**（当前） |
| 星光 `game_id` | `cstjjs` |
| 正式包 | `08_output/长生天机降世/长生天机降世_0.9.10_surgical-releaseSigned.apk`（**release**，非 debug） |

## 文档

| 文档 | 用途 |
|------|------|
| [星光落地_0.9.10_20260818.md](./星光落地_0.9.10_20260818.md) | **当前**外科包：运行时挂点、UnityApplication 原地注入 |
| [星光落地_0.9.9_20260813.md](./星光落地_0.9.9_20260813.md) | 旧版 inject（apktool，勿再对游戏整包 b） |
| [BUFF跟读_20260813.md](./BUFF跟读_20260813.md) | 斌哥壳差分 → RoleSide / delta |

## 可复用经验（模板）

- `10_docs/_templates/经验_ART连续dex与客户端完整性黑名单.md` — 禁止 `classes4` 一类黑名单
- `10_docs/_templates/经验_对照改包只借玩法_运行时Hook.md`

## 复现构建

```bash
cd mod菜单/starry-mod-menu
# Android.mk：STARRY_GAME_CSTJJS
bash scripts/surgical_cstjjs_0.9.10.sh
# 装机：STARRY_INSTALL=1（签不一致且用户同意卸载：STARRY_ALLOW_UNINSTALL=1）
```

后台：`feat_num` 1=十倍伤害、2=无敌。
