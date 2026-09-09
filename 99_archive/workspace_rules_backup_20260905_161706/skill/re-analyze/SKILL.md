# 逆向分析总入口：新游戏 APK 第一步

> 📍 re-analyze/SKILL.md
> 🎯 拿到一个新游戏 APK，需要快速判定引擎、保护、登录方式，决定"能不能改、怎么改"

---

## 1. 拆包

```bash
bash 04_scripts/_common/decode_apk.sh <apk路径>
```

拆包后检查：
- `AndroidManifest.xml` → Application 类、权限、Activity
- `lib/` 目录 → so 文件列表（引擎 + 保护）
- `classes*.dex` → DEX 数量和形态
- `assets/` → 资源文件（`.jsc`/`global-metadata.dat`/`.dll.bytes`）

## 2. 引擎判定速查表

| 特征文件 | 引擎 | 玩法路径 |
|----------|------|----------|
| `libil2cpp.so` + `global-metadata.dat`（无热更 dll） | Unity IL2CPP (AOT) | Dobby hook RVA |
| `libil2cpp.so` + `GameLogic.dll.bytes`/`Assembly-CSharp.dll` | Unity IL2CPP (HybridCLR) | 公共 invoke 跳板 / 运行时按名解析 |
| `libcocos2djs.so` + `.jsc` | Cocos Creator JS | hook `se::ScriptEngine::evalString` |
| `libcocos.so` + Creator3 | Cocos Creator 3 | `CocosJavascriptJavaBridge.evalString` |
| `libxlua.so` + `.bundle` | Unity + xLua | `LuaEnv.DoString` monkey-patch |
| `Assembly-CSharp.dll`（Mono） | Unity Mono | dnSpy 反编译 |
| 无 `libil2cpp`/`libcocos` | 壳/双开 | guest 进程内分析 |

## 3. 保护初判

先读 [`protection/_总纲_加固与校验全体系.md`](protection/_总纲_加固与校验全体系.md) 建立分层模型与死法分类，再检查 `lib/` 目录和 Application 类对号：

| 特征 | 保护类型 | 详见 |
|------|---------|------|
| 任何加固/校验卡点，或想先建立全局认知 | **分层模型 + 死法分类 + 决策树** | [protection/_总纲_加固与校验全体系.md](protection/_总纲_加固与校验全体系.md) |
| `libbaiduprotect.so` + `StubApplication` | 百度 Sagittarius 加固 | [protection/baidu-shell.md](protection/baidu-shell.md) |
| `libtersafe2.so` + `libtprt.so` + `AceApplication` | 腾讯 ACE/TSS | [protection/ace-tss.md](protection/ace-tss.md) |
| `libqkcheck.so` | QK 签名校验 | [protection/other-sign-check.md](protection/other-sign-check.md) |
| `libtoolChecker.so` | RootBeer Root 检测 | [protection/root-check.md](protection/root-check.md) |
| kiwi `StubApplication` + `libkadp.so` / `libKwProtectSDK.so` + Q1「游戏包不合法」弹窗 | Kiwi VM + KwProtect 完整性网 + PM 证书 | [protection/kwprotect.md](protection/kwprotect.md) |
| 改包后 1-2s SIGBUS 无堆栈 | DEX 完整性校验 | [protection/dex-integrity.md](protection/dex-integrity.md) |
| 登录弹"非法登录" | 客户端完整性黑名单 | [protection/dex-integrity.md](protection/dex-integrity.md) |

**保护叠加**：一个游戏可能同时有多种保护（如百度壳 + RootBeer + 签名杀手）。

## 4. 登录方式初判

检查 Manifest 和 smali：

| 特征 | 登录方式 | 详见 |
|------|---------|------|
| `com.taptap.sdk` 相关类 | TapTap 登录 | [login/taptap-web-login.md](login/taptap-web-login.md) |
| QQ 互联 SDK（`com.tencent.tauth`） | QQ SSO | [login/qq-wechat-sso.md](login/qq-wechat-sso.md) |
| 微信 SDK（`com.tencent.mm`） | 微信 SSO | [login/qq-wechat-sso.md](login/qq-wechat-sso.md) |
| `SignSpoof` / `KillerApplication` | 已有签名伪装 | [login/sign-spoof.md](login/sign-spoof.md) |

## 5. "能不能改"总判断逻辑

### 可以外科组装（最常见）
- 无 ACE/TSS 保护
- 无 DEX 完整性校验（或校验只保护部分 DEX）
- → 官方文件字节级保留，**只新增不修改**；smali 改动用 `smali assemble` 生成新 dex，zip 打包
- 说明：游戏包一律**禁止 `apktool b` 整包重建**（硬规则）；`apktool d` 只用于查看结构（且优先 MT MCP）

### 必须外科 zip 组装（不能 apktool b）
- 有 DEX 完整性校验（改包后 SIGBUS）
- 有 `resources.arsc` 完整性校验
- → 官方文件字节级保留，只新增不修改，zip 打包
- 详见 [protection/dex-integrity.md](protection/dex-integrity.md)

### 必须脱壳
- 百度壳加密的 DEX 需要修改时
- → 静态改不了，需运行时 Pine/ART hook
- 详见 [protection/baidu-shell.md](protection/baidu-shell.md)

### 极高风险 / 可能不能动
- 腾讯 ACE/TSS 全套保护
- → `apktool b` 整包重建必 SIGBUS，需要专用绕过方案
- 详见 [protection/ace-tss.md](protection/ace-tss.md)

### 不能动
- ACE 全套 + DEX 完整性 + 服务端战斗重放校验
- → 客户端无法绕过，建议放弃

## 6. 快速检查清单

```
□ Application 类是什么？（Manifest android:name）
□ lib/ 下有哪些 so？
□ 有没有 libil2cpp.so / libcocos2djs.so？→ 引擎判定
□ 有没有 libbaiduprotect.so / libtersafe2.so / libqkcheck.so？→ 保护判定
□ DEX 有几个？jadx 能看到真实代码吗？→ 壳加密判定
□ 登录 SDK 是什么？→ 登录方式判定
□ 有没有斌哥壳特征（libSDKloading.so + binge2.keystore）？→ 对照改包
```
