# 逆向提取玩法 Hook 点 — 总入口

> 📍 路径：`re-extract/SKILL.md`
> 🎯 触发场景：拿到一个游戏 APK（可能有斌哥改包也可能没有），需要提取玩法 Hook 点

---

## 第一步：判断有没有斌哥改包

检查以下特征，命中 **任意一条** 即判定为斌哥包：

| 检测项 | 特征 |
|--------|------|
| APK 签名 | `CN=binge2.keystore` |
| lib 目录 | `libSDKloading.so`（大小变体 586KB~23.7M 均有，勿用大小排除） |
| lib 目录 | `libtoolChecker.so`（RootBeer） |
| lib 目录 | 伪装 so：`libcodec2_server.so`（导出 `initUnitySdk`）/ `libunityluac.so`（瘦壳变体特征） |
| Application | `KillerApplication193` / `SignatureSpoof` / `com.srp.patch.Init`（SRPatch 变体）/ `com.pengyou.cloneapp.SplashActivity`（双开容器）等 |
| 嵌套原包 | `assets/base.apk`（全壳有，瘦壳无；与外层包 libil2cpp 字节一致，静态 dump 用它更干净） |
| 远程菜单 URL | `https://menu-1255602259.file.myqcloud.com/game/<id>.html` |
| shared_prefs | `{pkg}_preferences.xml` 含 boolean `"0"`/`"1"`/`"2"` |
| 壳配置 | `shell_config.xml` / `shell_protected*.xml`（全壳有，瘦壳无） |
| 免责声明邮箱 | `bg888@188.com` |
| Java 桥 dex | `com.android.support.{Main,Menu,Preferences,BinGe,BUFF}`（含 `kaiguan` 开关字段） |

**Application 名不能作为斌哥壳判定依据**（会出换名变体，如 `com.srp.patch.Init` + `libSRPatch.so` / `SRPatch_config.json`）：判定以**签名 + so + 免责邮箱的组合**为准，Application 变体只作线索。斌哥包与嵌套原包 `libil2cpp.so` 字节一致（2026-08-25 实锤）→ 玩法靠运行时 Hook，静态 dump 直接用嵌套原包，别用带壳外层。

### 有斌哥包 → 走 `binge-memdiff.md`

这是最通用的路径，**70% 以上游戏走这条**。斌哥壳已内置菜单功能开关，通过差分即可提取所有 Hook 点。

→ **下一步：[binge-memdiff.md](./binge-memdiff.md)**（已升级为决策树版：先查固定不变量速查表和**斌哥游戏档案表**——同游戏版本未变直接复用历史 hook 点；未命中再按引擎分流 A 注入 JS 提取 / B 磁盘 vs 内存 diff / C 三态 memdiff / D HybridCLR 方法名过滤；解密按需触发，禁止默认解密）

---

## 第二步（无斌哥包）：按引擎走 from-scratch 路径

### 引擎判定速查表

| 特征文件 | 引擎 | 玩法路径 | 跳转 |
|----------|------|----------|------|
| `libil2cpp.so` + `global-metadata.dat` | Unity IL2CPP (AOT) | Dobby hook RVA | [il2cpp-from-scratch.md](./il2cpp-from-scratch.md) |
| `libcocos2djs.so` + `.jsc` | Cocos Creator JS | evalString hook | [cocos-js-from-scratch.md](./cocos-js-from-scratch.md) |
| `libxlua.so` + `.bundle` | Unity + xLua | LuaEnv.DoString | [xlua-from-scratch.md](./xlua-from-scratch.md) |

### HybridCLR 检测（重要！）

**检测条件**：`libil2cpp.so` + `global-metadata.dat` 都在，但 Il2CppDumper dump 后找不到玩法相关方法（如 `ChangeHP`、`DeductHP`、`GenerateDamage` 等）。

**补充检测**：
- APK 内有 `GameLogic.dll.bytes` / `Assembly-CSharp.dll`（文件头 `QK` = 加密标记）
- 运行时内存中出现 `MZ` + PE 头（.NET 程序集特征）

→ 这是 HybridCLR 热更，走 [hybridclr-runtime.md](./hybridclr-runtime.md)

### 其他情况

| 情况 | 处理 |
|------|------|
| `libcocos.so` + Creator 3 | Cocos Creator 3，hook `CocosJavascriptJavaBridge.evalString`，参考 cocos-js 流程 |
| 无 `libil2cpp`/`libcocos` | 可能是双开壳，检查 Application 是否 `com.pengyou.cloneapp.SplashActivity`，attach guest 进程分析 |
| 百度加固 `libbaiduprotect.so` | DEX 被壳加密，不要改壳加密的 DEX，运行时壳解密后才有真实 DEX |

---

## 通用前置准备

1. **设备清理**：每次开始前跑 `cleanup_device_re_residue.sh`，清 frida/ptrace/KSU umount 残留
2. **KSU /proc/pid/mem dump 法**：所有动态分析用 KSU + `memdump64`，**不用 Frida**（壳反调试会杀进程）
3. **Il2CppDumper**：Unity 游戏必备工具，输入 `libil2cpp.so` + `global-metadata.dat`，输出 `dump.cs` + `script.json`

## 文件索引

| 文件 | 说明 |
|------|------|
| `binge-memdiff.md` | 斌哥壳抓点决策树（最核心：速查表 + 游戏档案表 + A/B/C/D 四路径 + 解密按需触发） |
| `il2cpp-from-scratch.md` | Unity IL2CPP 从零抓点 |
| `cocos-js-from-scratch.md` | Cocos JS 从零抓点 + jsc 解密 |
| `hybridclr-runtime.md` | HybridCLR 热更抓点 |
| `xlua-from-scratch.md` | xLua 抓点 |
