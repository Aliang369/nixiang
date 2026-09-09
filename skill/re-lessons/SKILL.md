# 逆向经验卡索引

> 📍 `skill/re-lessons/SKILL.md`
> 📖 原始文档在 `10_docs/_templates/`

---

## 经验卡列表

| 文件 | 首次发现 | 核心教训 |
|------|---------|---------|
| `经验_对照改包只借玩法_运行时Hook.md` | 音舞光年 | 借玩法语义不借壳；运行时 Dobby Hook 优于静态 patch；HybridCLR 热更下堆扫常无效，须钩"出分路径" |
| `经验_ART连续dex与客户端完整性黑名单.md` | 长生天机降世 | ART 缺中间 dex 不加载更高号；登录警告=体积阈值+黑名单路径双条件 |
| `经验_卡密设备指纹跨包一致.md` | 造梦/雷霆（4次复发） | device_id 整卡绑定跨包必 403；v4 去掉 OTA 变化字段用共享种子；装机必须 seed |
| `全游戏卡密_复现路径与防再发.md` | 活文档 | 10 条已证实复现 + 10 条潜在；发现合法 dev_* 只补空洞不覆盖；改指纹必须重打含菜单 DEX |
| （见 `starry-mod/SKILL.md` jniLibs 硬规则） | 弓箭传说2 2026-08-17 | Gradle 关 externalNativeBuild；只改 mk 不拷 jniLibs → 菜单仍是上一款；装机前 strings 自检 so 标签 |
| `10_docs/公主连结/属性敌我过滤修复_20260817.md` | 公主连结 2026-08-17 | 属性敌我禁止「未登记全放行」；Total* 热路径禁 SafeProbe；so_only 不带菜单「游戏补丁」 |
| `10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md` | 弹力果冻 2026-08-20 | **功能不对先 Trace/log 证实挂点与偏移再写功能**；禁猜 MP/乱 +8/patch=0 当成功；HybridCLR 自注入 StarryTrace |
| `re-implement/version-update-relocate.md` | 音舞光年 2026-08-22 | 小版本更新挂点**整体搬迁**：`hits=0` 诊断树 + 旧桩 blob 匹配新 so + 禁区清单（runtime_invoke/MP 白名单/钩 get_name 导出） |
| `re-implement/hook-rules.md` §16/§17 | 无悔华夏+弓箭传说2国际版 2026-08-23/27 | **禁启动即 bulk hook**（40+ 点 → 部分机型 SIGSEGV/SIGILL，按开关懒装、默认 0 hook）；可选方法缺失不阻断整包安装 |
| （见 `starry-mod/SKILL.md` 分发与装机红线） | 无悔华夏 2026-08-28 | **分发事故：文档修了 ≠ 用户拿到的包修了**（正式包没重打就外发 → 全员闪退）；MenuService 5s 前台服务铁律；清数据 = Mod+热更资源双杀 |
| `re-analyze/protection/kwprotect.md` | 王国大作战前线 2026-08-28 | KwProtect 完整性网：base.apk 字节级/split 也扫/lib 替换 MD5 杀/patch kill 只会死转——**正解是让校验通过**，不是 patch kill |
| `re-implement/economy-pattern.md` 模式二 | 动物餐厅 2026-08-18 | 热更目录**优先于 APK assets** 加载，冲掉 jsc 静态门控；门控要在 attachBaseContext 打/热更落地后重打；wrap-only 不可靠 |
| （框架样板 bug，已修 → 模板 v2） | 星光框架 2026-08-28 | `_TemplateMod.cpp` 曾 include 不存在的 "TemplateMod.h"，照抄必编译失败 → AI 绕开模板从旧游戏抄，旧坑传染（30 个 Mod 仅 1 个有 reenter/懒装）。**框架样板必须保证「复制即可编译」，约束要配机械工具（生成器/扫描），纯文字清单管不住 AI** |
| `10_docs/_templates/经验_HybridCLR直写三铁律_20260829.md` | 战魂铭人 3.5.0 2026-08-29 | **HybridCLR 直写三铁律**：①MethodPointer hook 装上≠命中（解释器分发，必须命中计数证实）；②外部线程 invoke 热更方法=解释器 SIGSEGV（attach 不救，热更方法 0 invoke）；③字典 stride 随 TValue（0x18/0x10/0x78，步长自检，值类型闭包泛型 set_Item 直调无效→原地翻转 value）；附：稳态 Tick 零 il2cpp API + process_vm_readv 安全读、配置表驱动必须运行时枚举、field_get_offset 原值即真偏移（旧 +8 是扫描基址偏差） |

---

## 使用方式

- 接手新游戏前扫一遍，看有没有同类坑
- 遇到问题时先查这里，再从零分析
- 发现新坑 → 写新经验卡 → 加入本索引

---

## 其他重要经验（散落在各游戏文档中）

### 魂坠深境
- HybridCLR 跳板白名单方法名写字段：先调 orig，勿 return box 顶替（x0=1 SIGSEGV）
- 禁止对整类所有方法写字段（对局结束 SIGSEGV）
- 禁止改 GetFinalCostValue 返回值、秒杀写 BFT.health（未出牌怪死）

### 堡垒之战
- 战斗外挂无法稳定过官方服结算：服务端重放校验 attackHpScale/invincible
- 伪造假胜利无真奖励，关卡回滚——禁止再做

### 公主连结
- TSS/ACE 链路：AceApplication → native initialize → libtersafe2 读 sourceDir → SIGBUS
- libil2cpp.so + libunity.so 都 DT_NEEDED libtprt.so → Unity 加载自动 dlopen
- arsc 必须 STORED，header.size 自洽
- StarryApp 须继承官方 App 类
- 属性敌我（2026-08-17）：禁止 `ShouldBoostUnit`「未登记则全放行」；`IsPlayerUnit`→side 表，`Total*` 只读表，未知不改（详见 `10_docs/公主连结/属性敌我过滤修复_20260817.md`）
- `get_Total*` 热路径禁止 SafeProbe / 内层再调 `IsPlayerUnit`（跨线程 SIGSEGV）；偏移发现只允许安装期或首次钩子一次性探测
- `pcr_so_only` 只换 so，不带设置页「游戏补丁」；改菜单须刷 `classes13.dex`（+ StarryApp→官方 App）

### 弹力果冻（HybridCLR 无限技能 2026-08-20）
- **功能不对 → 先 Trace/log 证实挂点与偏移，再写功能**（详见 `10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md`）
- log 里 `patch=0` = 未写入，不是「功能弱」；`cd` offset 错一位 CD 完全不动
- 单跳板：共有 MethodPointer 禁 MP 过滤；nocd 禁止对每个跳板调用扫 Skill 字段
- 动态分析用自注入 `StarryTrace`，禁止默认 Frida 长挂

### 动物餐厅（Cocos 热更冲掉内购门控 2026-08-18/19）
- 热更目录（`files/restaurant-hot-update`）**优先于 APK assets** 加载：APK 里打好 `__starryNoIap` 门的 `index.jsc` 被整文件替换 → 免内购失效（免广告走 Java `DwctHooks` + native `evalString` 兜底，不受影响）
- 正修：热更落地后用 gate 脚本重打 `index.jsc` + 同步 `project.manifest` 的 md5/size；正式包在 `StarryApp.attachBaseContext` 里打门（Java `DwctIapGate`），抢在 Cocos 加载 JS 之前；`DwctMod` 用 `__starryW` 标记 + Tick 重 wrap
- 只 wrap `require()` 不可靠（12.37/12.38 两轮实测）；用户清数据后热更回归官方原版，门控必然丢失
