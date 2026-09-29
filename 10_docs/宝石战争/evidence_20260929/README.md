# evidence_20260929 — 宝石战争 HotUpdate 1.0.0.06 修复

## 决定性证据（按重要性）

1. `logcat_starry_20260929.txt`（新包冷启动 21:31:43，pid 21066）
   - `[桥接] RegisterNatives 19/19 全部成功`
   - `星光魔法 MOD 菜单已加载 (MainModern tag=Bszz)` / `MOD 已就绪 tag=Bszz ready=1`
   - `[验证] grant ok exp=1790775107`
   - `ModBridge: reapplied saved features to native`；21:32:13 用户操作 #2=1 / #3=1 / 倍率 100
   - 无 FATAL / ANR / SIGSEGV
2. `starry_state_20260929.txt`：`0 100 1 1 0 0 0 0`（攻击倍率 100 / 上帝模式 1 / 无限技能 1，覆盖安装后数据保留）
3. `screen_after_fix_20260929.png`：战斗准备界面 + 星光菜单气泡（本地视觉 QA 用，不嵌入对话）

## 修复前后对比（决定性实测）

| 状态 | 设备加载的 System.Network.dll.bytes | 外部 HotUpdate |
|---|---|---|
| 修复前（1.0.0.06 官方回写） | `f982199e1e575218f40b54e701ad7921`（官方） | 有文件 |
| 只换设备文件（被否定） | 45s 后回写官方 | 有文件 |
| 改 manifest/receipt 指纹（被否定） | 目录被整体清空后重建 | 空 |
| 修复后（新包内嵌补丁） | `0f34ea7fb157ead092efd4aff13c1c91`（补丁） | 保持空 |

## 最短复现命令

```bash
# 包内补丁确认
unzip -p 08_output/宝石战争/宝石战争_6.0.013_hu1.0.0.06_surgical-releaseSigned.apk \
  assets/Android/Dlls/System.Network.dll.bytes | md5 -q     # 0f34ea7fb157ead092efd4aff13c1c91
# 设备外部热更应为空、无官方 DLL
adb -s f9a116a2 shell 'su -c "find /storage/emulated/0/Android/data/com.acing.gow.adv/files/HotUpdate -type f"'
```

## 已验收（2026-09-29 用户回执）

- 战斗内三项行为（攻击倍率 / 上帝模式 / 无限技能）——用户口径「功能没问题了」，G2 ✓

## NOT PROVEN

- 二次冷启动后外部 HotUpdate 是否仍为空
- 结算 `submit_result`、PvP/竞技场/公会战/排行榜二次校验
