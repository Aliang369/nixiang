# DEX 完整性校验 + 黑名单校验

> 📍 re-analyze/protection/dex-integrity.md
> 🎯 改包后进程被 native 杀（SIGBUS/SIGSEGV）或登录时弹"非法登录"，需要定位受保护 DEX 并绕过

---

## 一、DEX 完整性校验

### 识别特征

- 改包后 1-2s 被 native 杀（SIGBUS/SIGSEGV），无 Java 堆栈
- 零改动只重签 → 存活
- 加 asset → 存活
- 改某个 dex → 死

### A/B 测试定位受保护 DEX

1. **零改动重签** → 确认基线存活
2. **逐个 DEX 改动测试**：
   - 改 `classes.dex` → 死？标记为受保护
   - 改 `classes2.dex` → 死？标记为受保护
   - ...逐个测试
3. 确定"受保护 DEX"清单（如 `classes4`/`classes8` 不可改）
4. 菜单/注入类放进**可改的 DEX**（如 `classes12` 或新增 `classes13`）

### 连续 DEX 加载规则

ART 只连续加载 DEX，跳号会导致 ClassLoader 缺失：

- 官方有 `classes`/`classes2`/`classes3` → 星光进 `classes3` 或紧接的 `classes4`
- **禁止**造空 `classes4.dex` 再把星光放 `classes5`（ART 不加载）
- 新增 DEX 必须紧接最后一个官方 DEX 的序号

### 外科组装对策

**核心原则：只新增不修改。**

1. 官方 APK 用 `unzip` 解压
2. 受保护文件字节级保留：`classes*.dex`、`resources.arsc`
3. 只新增：新 `classesN.dex`（星光 smali）、`libstarry_magic.so`、`libc++_shared.so`、`assets/certs`、修改后的 `AndroidManifest.xml`
4. `zip` 重新打包 → `zipalign` → `apksigner`
5. **禁止 `apktool b` 整包重建**（即使零改动重签也可能 SIGBUS）
6. `resources.arsc` 不可整体替换

### 验证方法

```bash
pm install -r -d /data/local/tmp/x.apk
# 存活 >25s 即过
```

## 二、客户端完整性黑名单校验

### 识别特征

- 登录时弹"检测到非法登录"并退出
- 热更代码中检查 APK 体积 + 扫描特定文件是否存在

### 校验逻辑

1. **体积阈值**：检查 APK 文件大小 > 某阈值（如 `2181850710u`）
2. **路径黑名单**：检查特定文件是否存在
   - 例：`classes4.dex`（如果星光不该有这个文件）
   - 例：`assets/overlay_integrity.dat`
   - 例：`lib/arm64-v8a/libbattlehook.so`
3. 两个条件是**双条件**（体积 + 路径列表同时满足才触发）

### 对策

1. 星光注入只进**不冲突的 DEX**（如 `classes3`，不造 `classes4`）
2. APK 不含黑名单文件
3. 控制 APK 体积不超过阈值（必要时压缩 so）

### 验证方法

登录不弹窗、不退出。

## 案例：公主连结

- 受保护 DEX 清单：`classes4`/`classes8` 不可改
- `resources.arsc` 必须 STORED
- 只能外科 zip 组装

## 案例：长生天机降世

- 热更 `Code.dll` 中检查 APK 体积 > 阈值 + 文件黑名单
- 星光只进 `classes3`（不造 `classes4`）
- APK 不含黑名单文件即过

## 坑/弯路

- **不要用 `apktool b` 整包重建**：即使零改动也可能触发完整性校验
- **MT 精细改也杀**：MT 改 DEX 会修改字节，触发哈希校验
- 只有外科 zip 组装（官方文件字节不变）可行
- **Manifest 可改**但需用二进制格式

## 外部已公开方案（检索入口，2026-10-15 新增）

- **已核验公开项目**（只解决「拿到 dex」，不解决完整性校验）：[CodingGay/BlackDex](https://github.com/CodingGay/BlackDex)（6.5k 星，免 root，Android 5.0–12）、[tiwe0/r0dump](https://github.com/tiwe0/r0dump)（2026-09，Android 16 ART 集成 DEX dumper + 修复，FART 思路）、[hluwa/frida-dexdump](https://github.com/hluwa/frida-dexdump)（4.6k，本仓已装；**反 frida 游戏不可用**）。
- **检索词**：`<壳名> dex 完整性 校验 绕过` / `APK 体积 阈值 黑名单 检测` / `resources.arsc STORED 校验`。
- **优先级提醒**：本页铁律（外科 zip 组装、不动已加密 dex）**高于**任何外部脱壳工具；外部工具只在「需要先看清被加密的代码」时用。
- **台账**：`10_docs/_state/外部方案台账.md`（§1 X-8）。
