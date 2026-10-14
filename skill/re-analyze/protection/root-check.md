# Root 检测（RootBeer）

> 📍 re-analyze/protection/root-check.md
> 🎯 游戏使用 RootBeer 检测 root，需要识别并绕过

---

## 识别特征

1. `lib/` 目录存在 `libtoolChecker.so`
2. smali 中有 `com.scottyab.rootbeer` 包引用
3. 调用 `RootBeer.isRooted()` 返回 true 时触发退出

## KSU 隐藏

KernelSU 的默认隐藏方案通常可以过 RootBeer：
- KSU 的 `umount` 机制隐藏 su/magisk 痕迹
- 默认配置下 `RootBeer.isRooted()` 返回 false

## 加强版绕过

如果 KSU 隐藏不够（如游戏有额外检测）：

1. 找到 `RootBeer.isRooted()` 的 smali
2. patch 恒返回 `false`：
   ```smali
   .method public isRooted()Z
       .locals 1
       const/4 v0, 0x0
       return v0
   .end method
   ```
3. 或运行时 hook `RootBeer.isRooted()` → 返回 false

## 验证方法

游戏不弹 root 检测提示、正常运行。

## 坑/弯路

- RootBeer 只是基础检测，通常不单独使用
- 斌哥壳自带 `libtoolChecker.so`（RootBeer），KSU 隐藏默认过
- 如果游戏同时有 ACE/TSS，root 检测在 ACE 面前是小问题，优先处理 ACE

## 外部已公开方案（检索入口，2026-10-15 新增）

- **已核验公开来源**：检测侧为公开库 [RootBeer](https://github.com/scottyab/rootbeer)（本页面对的即它的 `isRooted()`）；**绕过侧本仓首选 KSU 隐藏**（已实测默认过斌哥壳的 `libtoolChecker.so`），未核验独立绕过项目。
- **检索词**：`RootBeer 绕过` / `KSU 隐藏 root 手游` / `<检测库名> bypass`。
- **注意**：本页做法（patch 恒 false / 运行时 hook `isRooted()`）已够用；外部「Magisk/KSU 隐藏模块」类方案属**设备级**改动，按 `AGENTS.md` 授权边界与「设备画像先行」评估，不默认引入。
- **台账**：`10_docs/_state/外部方案台账.md`；协议见 `skill/diagnosis-index.md`「外部检索协议」。
