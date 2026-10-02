# 逆向分析总入口：新游戏 APK 第一步

> 路径：`re-analyze/SKILL.md`
> 目标：从线索形成候选解释，以区分证据逐步确认引擎、组件、登录路径和可测试边界。
>
> ⚡ **用户要求「先过环境 / 先别直接做功能 / 菜单组装进游戏能正常玩才算完成」时，先读
> [environment-min-path.md](environment-min-path.md)** —— 它给出环境阶段的最小充分集（五步）、
> 每步退出条件和「推迟到第二阶段」清单，避免在第一阶段顺手做第二阶段的 IL2CPP 分析。

---

## 1. 拆包

先记录输入 APK 的来源、版本、包名、架构、哈希及是否为完整包/split 集合。可使用现有拆包脚本，也可按问题直接检查归档；不强制特定工具。

```bash
bash 04_scripts/_common/decode_apk.sh <apk路径>
```

检查 Manifest 的 Application/Activity、`lib/`、DEX 形态和 assets。工具无法解析只是一项观察，还需区分版本不支持、输入不完整、压缩/封装或保护；游戏 APK 默认外科组装，整包重建的例外授权与证据条件统一见 [AGENTS.md](../../AGENTS.md)。

## 2. 引擎判定速查表

下表是候选索引，不是仅凭文件名定性；组件可能共存、改名、未使用或运行时才加载。

| 线索 | 候选 | 区分证据 |
|------|------|----------|
| `libil2cpp.so` + `global-metadata.dat` | Unity IL2CPP | ELF/metadata 身份与版本、加载引用、实际模块和方法路径 |
| IL2CPP + `GameLogic.dll.bytes`/`Assembly-CSharp.dll` | HybridCLR/热更，或普通托管资源 | 热更运行时依赖与实际程序集加载；仅见 dll 不足以确认 |
| `libcocos2djs.so` + `.jsc` | Cocos Creator JS | 初始化调用链和脚本加载路径 |
| `libcocos.so` + Creator3 资源 | Cocos Creator 3 | 版本信息、初始化及资源格式 |
| `libxlua.so` + `.bundle` | Unity + xLua | Lua 初始化/执行引用与实际加载 |
| `Assembly-CSharp.dll` | Mono、热更或未使用资源 | Mono/其他运行时装载及程序集用途 |
| 未见 `libil2cpp`/`libcocos` | split 缺失、改名、动态加载、自研/其他引擎，或壳/容器 | 包集合完整性、Manifest/DEX 加载引用、运行时模块/进程；不得直接定性为壳/双开 |

### 玩法层定位决策树（引擎定完后立刻做，别先写补丁）

> 要改的功能逻辑存在与否、在哪一层，决定后面所有工作量。**选错层的代价**：整套补丁写完才发现目标类型根本不在这个程序集里（2026-10-02 案例：先按 IL2CPP 静态 RVA 方向做了两轮探索，才发现玩法类全在 HybridCLR 热更 DLL 里）。
> 大体积反编译产物（`dump.cs`、整包反编译 `.cs`）怎么读，见 [large-decompile-index.md](large-decompile-index.md)。

按顺序问，问到「是」即停：

1. **目标类型/字符串能在 AOT metadata 里查到吗？**（`global-metadata.dat` + Il2CppDumper 出的 `dump.cs`）
   - 能 → 逻辑在 `libil2cpp.so`，走 **native RVA hook**；注意 AOT 优化/内联会让同一逻辑散落多处。
   - 不能 → 往下。**判据不是「有没有 `libil2cpp.so`」**：实测某 HybridCLR 游戏 `dump.cs` 有 7,533 类 / 40 image，却**完全没有 `Assembly-CSharp.dll` 这个 image**，玩法类一个都不在里面。
2. **有热更程序集吗？**（HybridCLR / `*.dll.bytes` / 运行时能 dump 出 `Assembly-CSharp.dll`）
   - 有 → 逻辑在热更 DLL，走 [../re-implement/hotfix-dll-replace-pattern.md](../re-implement/hotfix-dll-replace-pattern.md)：先找**唯一加载边界**（`System.Reflection.Assembly.Load(byte[])` 命中几次？）→ hook 它 → 塞 Cecil 补丁过的 DLL。
   - ⚠ HybridCLR 下的热更类**无法用静态 RVA**（见 [../re-extract/hybridclr-runtime.md](../re-extract/hybridclr-runtime.md)），别去 `libil2cpp.so` 里找它们的地址。
3. **Lua 里有吗？**（`libxlua.so` / `*.luac` / `xLua` 资源包）
   - 有 → Lua 层注入；只 hook `luaL_loadbufferx`（`libxlua.so` 的 Lua 5.3.5 **不导出** `luaL_loadbuffer`）。
   - 没有 → 往下。**先查清再决定**：实测某游戏 106 个 Lua 模块里零伤害/技能/血量逻辑，在 Lua 方向耗的每一分钟都是净损失。
4. **配置表 / 资源里有吗？**（Excel 导出的 `json`/`bytes`、AssetBundle 里的 `TextAsset`）
   - 有 → 改资源，最省事；注意热更会覆盖本地资源，要同时看服务端是否下发。
5. **都不是** → 逻辑可能在**服务端**（服务器权威模式）或自研 VM。此时先写明「客户端改动只影响表现」，再决定做不做。

**定层证据必须含运行时观测，不能只有静态推断**：

- 静态：`dump.cs`/反编译产物搜索、`strings`、程序集清单、Lua 模块列表。
- 运行时（决定性）：hook 候选入口打印参数、加载边界命中次数、日志（探针写法与三条约定见 [native-probe-on-device.md](native-probe-on-device.md)）。
- **一条铁律：hook 能拿到的明文，就不要去破壳。** 案例：热更包 `Assembly-CSharp.assetbundle` 带 8 字节头 + `0x6000` 加密前缀，AES-CBC / `abconfig` / YAES 三条破译路线全错；而**内存 dump 出来的 DLL 本来就是明文**，直接 hook 加载入口即可 —— 破壳的时间全是沉没成本。

**退出条件**：能用一句话回答「这条逻辑在 ⟨层⟩，证据是 ⟨某条运行时观测⟩」。答不出就继续补证据，不要开写补丁。

## 3. 保护初判

按“线索 → 候选 → 区分证据”记录。仅在候选需要时查阅对应案例；历史分类和死法表不是诊断结论，也不要求先扫描全部组件。

| 线索 | 候选（待验证） | 区分证据 / 参考 |
|------|----------------|-----------------|
| `libbaiduprotect.so` + `StubApplication` | 百度相关加载/保护组件 | 实际初始化与受保护范围；[baidu-shell.md](protection/baidu-shell.md) |
| `libtersafe2.so` + `libtprt.so` + `AceApplication` | ACE/TSS 相关组件 | 活跃调用路径、版本和故障关联；[ace-tss.md](protection/ace-tss.md) |
| `libqkcheck.so` | 可能的校验组件 | 实际调用和失败分支；[other-sign-check.md](protection/other-sign-check.md) |
| `libtoolChecker.so` | 可能的环境检测 | 检测结果及消费者；[root-check.md](protection/root-check.md) |
| Kiwi/KwProtect 名称及“游戏包不合法”弹窗 | 相关加载/完整性/证书校验候选 | 弹窗来源、调用链与输入对照；[kwprotect.md](protection/kwprotect.md) |
| 改包后短时间 SIGBUS、缺少堆栈 | 映射文件截断/越界、对齐故障、加载/重包问题或主动终止等 | signal code、fault address、maps、栈与原包对照；信号本身不能确认 DEX 校验 |
| 登录“非法登录” | 证书/配置不匹配、账号/服务端策略、完整性拒绝等 | 实际错误来源、SDK/服务器响应及单变量对照 |

完整性候选参考 [dex-integrity.md](protection/dex-integrity.md)，分层案例索引见 [_总纲_加固与校验全体系.md](protection/_总纲_加固与校验全体系.md)。一个游戏可能有多种组件；存在不等于启用，未发现不等于不存在。

**最小可区分实验**：先列至少能解释当前观察的合理候选，选择结果能区分候选且干预最小的下一步。例如先核对包集合/输入身份，再检查加载引用；若需要运行证据，先保存基线和故障现场，再做单变量对照。按问题选择归档检查、静态分析、日志、调试或已有探针，不强制特定工具。动态测试遵循授权、协作与清理规则。

运行时观测通道与探针写法见 [native-probe-on-device.md](native-probe-on-device.md)（**目标机无 root 就跑不了 frida-server ⇒ 把探针编进自家 so 走 logcat**）；同一改包换机异常、隐藏 API 豁免失败、JNI 符号找不到、MIUI 不能注入点击等版本/厂商差异对照 [android-version-gotchas.md](android-version-gotchas.md)。

## 4. 登录方式初判

| 线索 | 候选 | 区分证据 / 参考 |
|------|------|-----------------|
| `com.taptap.sdk` 类 | TapTap 集成 | 实际入口和 SDK 版本；[taptap-web-login.md](login/taptap-web-login.md) |
| `com.tencent.tauth` | QQ SSO 集成 | 调用路径和回调配置；[qq-wechat-sso.md](login/qq-wechat-sso.md) |
| `com.tencent.mm` | 微信集成 | 登录/分享用途及实际路径；同上 |
| `SignSpoof` / `KillerApplication` | 可能有已有签名处理 | 类实现与初始化关系；[sign-spoof.md](login/sign-spoof.md) |
| 登录页能进但登录服务器返回错误码，或用户描述像“设备被检测” | 服务端拒绝、账号/设备策略、客户端环境检测候选 | 原始返回码、客户端文案存在性、同账号换设备/同设备换账号；[server-rejection-triage.md](login/server-rejection-triage.md) |

SDK 类存在不证明当前登录一定经过它；界面提示也不自动证明客户端完整性黑名单。

## 5. “能不能改”总判断逻辑

以下保留历史小节入口，判断以范围和证据为准，不由保护品牌作二元结论。本入口不提供新增规避或作弊操作。

### 可以外科组装（候选，需验证）

外科 zip 组装是工作区打包约束，不是“没有保护”的证明。尽量保留未改文件字节，核对改动清单、签名、加载和产物身份；新增文件也可能影响校验，不能保证“只新增就能运行”。

### 默认外科 zip 组装（例外统一授权）

默认策略适用于游戏包，不仅限于疑似有 DEX/resources 校验的包；例外条件统一见 [AGENTS.md](../../AGENTS.md)，不由本节另设禁令或授权。SIGBUS 不足以判定校验；外科组装也不证明能通过运行检查。

### 必须脱壳（历史分类名，不作自动结论）

只有在任务确实需要、且证据表明目标代码不可由当前静态输入获得时，才评估运行时取证的必要性。先区分输入缺失、工具不兼容与代码封装，不强制某种运行时工具或修改方案。

### 极高风险 / 可能不能动

根据已观测约束、授权边界、测试隔离条件和所需干预作评估。ACE/TSS 等名称不能单独推出“重包必 SIGBUS”或必须采用某种方案；缺少区分证据时标“待验证”。

### 不能动

授权不足、无法建立受控测试边界或目标依赖不可满足的服务端契约时停止相应操作，说明具体限制。不能仅凭组件组合断言技术上绝对不可行；客户端证据不能证明可改变服务端裁决。

## 6. 快速检查清单

```text
□ 输入身份与完整性（包/split/版本/架构/哈希）是什么？
□ Manifest、库、DEX、资源提供哪些已观察线索？
□ 每条线索支持哪些候选，还有哪些替代解释？
□ 库缺失、解析失败、SIGBUS 是否被错误当作定性证据？
□ 下一项最小实验能区分哪两个候选，停止条件是什么？
□ 是否确需运行/用户协作，工具是否按问题选择？
□ 输出 CLAIM + 适用范围 + 证据 + VERDICT，并列未覆盖范围。
□ 首次受控测试走 G1；运行 G2、交付 G3 见 ../re-implement/code-review-checklist.md。
□ 真机取证按 `device-forensics-batch.md` 批量取（一条命令一屏，禁逐条 adb 往返；`-s` 前缀匹配是坑）。
□ 结论落盘：截图/日志进 `/tmp` 或 `09_test/{游戏}/`，不许只留在终端里。
```
