# 对齐斌哥 GenerateDamage（2026-08-12）

## 问题

直钩 `Damage.GenerateDamage` MethodPointer 会狂打 `GD#`，但 `dmg+0x10=0`、BFT 垃圾——**钩错入口**。出牌时 Frida 也证实 MethodPointer 常不进真实结算。

## 斌哥实锤

1. **唯一 il2cpp 改点**：`file 0x28B90B0` / **RVA `0x28BD0B0`**（HybridCLR 公共跳板；与音舞光年同思路）
2. Handler 序言：`x1=MethodInfo*`, `x2=params`，按方法名过滤 `GenerateDamage`
3. 字段：`Damage*=params[0]`；BFT=`Damage+0x38`；`isPlayer@+0x5c`；改 **`Damage+0x10`**（×100 / =0）

## 星光

`HzsjMod.cpp`：Dobby `0x28BD0B0` + 方法名过滤；卡牌仍钩 `GetFinalCostValue`。

```bash
STARRY_INSTALL=1 bash mod菜单/starry-mod-menu/scripts/hzsj_so_only.sh
adb logcat -s Mod_Menu   # 应见 GD#… (invoke) 且 sk/inv=1
```

## 2026-08-12 21:36
ScanInstances 旧逻辑跳过 [anon] 堆导致无敌 Tick 扫不到；已放行 anon/heap，主路径改 BFT 扫血。验证看 `bft scan raw=… seen=…`。

## 2026-08-12 21:40 去跳板
开功能 SIGSEGV 在 GD invoke 热路径。已去掉 0x28BD0B0/GenerateDamage 钩，只保留 BFT/PGSI Tick 写血。
