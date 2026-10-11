# 霓虹深渊2 状态速读摘要（state_digest.sh 生成，勿手改；需要原文按行号读段）

> 纪律：**不要整读**下列原文。先在本摘要里定位行号，再 `sed -n 'A,Bp'` 或
> `bash 04_scripts/_common/code_locate.sh <文件> '<关键词>' --ctx 3`。

## 台账.md（73372 B / 355 行）
  :6     ## 已成立
  :8     | 结论
  :10    | 「彩色钥匙」在 NHY2 没有独立货币字段/枚举；其原生通道 = `NEONPlayerState._keySuitInfi…
  :11    | mod 菜单 14 槽定稿（战斗 6 + 资源分类 + 资源 6），槽 13 = 无限彩色钥匙，走 Infinite…
  :12    | 真机已装底包 = 官方 2026.10.10.e3a265afd28（versionCode 565），单一 base.apk 1,344,627,662 B
  :13    | 「无限跳」开开关即闪退的真因 ≠ 挂点/RVA 选错，而是**自家字段解析段把枚举实…
  :14    | 「移速倍率/伤害倍率滑块全程无效」的真因 ≠ 挂点选错、也 ≠ 数值没写进游戏…
  :15    | 轮次2 修复包已出并装机：新 so `212256 B` md5 `c1e5dab8bacbb20dcb7cbc493bcc7e81` → signed apk m…
  :17    | NHY2 走 **modern 注册表**，不使用 `GameConfig.h` / 公共 `Android.mk` 的 `-DSTARRY_GAME_*` 单文…
  :18    | NHY2 的 TapSDK 无旧版登录类名 ⇒ 强制网页登录只能走 dex 层 `ClientLoginUtils.isClientLogi…
  :19    | 官签证书 DER（com.veewo.neonabyss2）可直接从底包 `META-INF/ANDROID-.RSA` 抽出，且与 `apksig…
  :20    | 星光 14 槽包已按用户选定方案 A 装机成功（卸载官方包 → push → `pm install -r -d`）…
  :21    | 入口表 A/B 补配在实机上确实生效（不是只过了静态审计）
  :22    | 装机层预检通过（菜单能弹、空转不崩）
  :23    | 自家 CrashTrap 的 `fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=65536)` 行会被默认 `RE_CRASH` …
  :24    | 「登录失败，请通过 taptap 客户端启动游戏」的真因 = **强制网页登录与游戏 ~37 s …
  :25    | 30~37 s 超时预算属**游戏侧 async 等待**（不是 Java/Activity 侧），且网页登录成功只…
  :26    | 「抑制 HandleLoginFailure(5040)」**只防弹框、不产出登录态**
  :27    | 本作**每次冷启都必须手动网页登录**（TapTap 客户端未安装 ⇒ 无缓存账号）
  :28    | 交互登录页的等待只在**交互登录协程存活期间**存在（与「网页页开着」不是一…
  :29    | 「全屏攻击没效果」的真因之一：上一版**根本没装 Attri 读点槽** —— 那轮日志 `…
  :30    | 键名 `projectilerange` 单独做 needle **必然空转**：游戏 AutoDict 的 104 个键里没有 Project…
  :31    | 「无限金币关着也能买」的既有串线点 = 闪光币 getter 用了水晶门禁
  :32    | 装机预检 `判定段=A` 在本作存在**假失败模式**：THEMIS 自报 `send crashlog successed`（…
  :33    | surgical 的旧 `resolve_orig()` 会把 10.5 旧底包当底包 ⇒ 整轮打包白跑
  :35    | 「打着打着闪退」（r5b/r6b/r6c 战斗/关卡切换时 SIGSEGV `fault addr 0xb8`）真因 = **Dobby …
  :36    | 同类第二颗雷 = `StandardEditionAccessPolicy.IsRestrictedEvolution` 只有 **12 字节**（`+8` 即 `re…
  :37    | r7 修复 = `HookByName` 新增两道**入口形态闸门**：A 转发器链解析（`ResolveBranchForwarder…
  :38    | r7 出包+装机闭环
  :39    | 「async_safe 坏格式串 assert 会杀进程」（r5b 的 `conversion specifier unsupported` SIGABRT）已…
  :40    ## 已否证（禁止重试区）
  :42    | 曾经的假设
  :44    | 「彩色钥匙」= `CostType.Token(28)`
  :45    | 资源「万能闸」（任一资源开关 ⇒ 全部 CostType 生效）
  :46    | 用 `01_tools/DumpRva` 从 unpacked `libil2cpp.so` 取本作方法 RVA
  :47    | 直接 grep 官方 APK 的 `assets/*` 找游戏内中文文案
  :48    | `strings` 默认能取出 `.so` 内的中文菜单串
  :49    | 「登录失败 = 轮次2 的 .so 改动 / 装机方式坏了登录」（会去回退 so、重装或改 hook…
  :50    | 「抑制 5040 弹框 = 修好登录」（会去加大 `NHSY2_LOGIN_GRACE_MAX` 或延长 180 s 窗口，反…
  :51    | 「无限跳闪退是 hook 挂点/RVA 选错」（会去删 `kSlots` 里的 Jump_* 或改 RVA）
  :52    | 「移速倍率无效是挂点选错（`get_MovementSpeed` 不是真速度源）」（会去盲改挂 `Mover…
  :53    | 「全屏攻击 = 强制 `Attri` 里的 `projectilerange` 一个键就够了」（会继续只调 `kFullKeyN…
  :54    | 「THEMIS 报 `send crashlog successed` = 游戏崩了」（会去回退 so / 重装 / 改 hook）
  :56    | 「0xb8 闪退与 async_safe 坏格式串 assert 同源」（会去调护盾阈值、封 libc 日志）
  :57    | 「0xb8 闪退是『我们的 hook 改写了游戏数值』导致」（会去关功能、回退数值写入）
  :58    ## 未验证（等什么）
  :60    | 待验证的结论
  :62    | 「登录预算延长」（`Task.Delay` 在交互登录窗口内 30 s→300 s；§6.5.3b）是否命中真…
  :63    | 「无限彩色钥匙」真机效果（HUD 钥匙变彩色/∞、消耗路径免费）
  :64    | 全屏攻击的抓点
  :65    | 2026.10.10 底包上 14 槽菜单与 53 条 by-name 挂点是否全部命中（新版本类名/方法名是…
  :66    | r5b 的 **60 槽**是否全部装上（含 5 个 `Full_Attri*`）
  :67    | 全屏攻击的**真实键名/目标值**（斌哥全屏组条目服务端推送、离线不可得）
  :68    | 「无限金币关闭却仍能买」是否被 §6.4e 根治（消耗漏斗 + StoreItem 交易闸）
  :70    | r7 修复后 0xb8 闪退**不再复现**（终审）
  :71    ## r8（2026-10-12）全屏攻击 / 无限水晶：换真通道
  :73    | 结论
  :75    | 旧「无限水晶」挂点是**死路径**：`NEONPlayerState.get_CurrentSpriteCrystalCount` 全程无人…
  :76    | 玩家水晶**不在** `NEONPlayerState`（该类 275 字段无任何 crystal 字段）；`get_CurrentSprite…
  :77    | 玩家射程是**玩家专属 stat**（不波及敌人）：`NEONPlayerState.get_ProjectileRange` + 后备…
  :78    | 旧「全屏攻击」走 Attri 键名路线（`kFullRules`）对**射程无能为力**
  :80    | 待验证的结论
  :82    | r8 全屏攻击＝强制 `get_ProjectileRange` 能否产生「子弹横穿全场」
  :83    | r8 无限水晶＝精灵通道（`GetCrystalSpriteCount`/`UseCrystalSprite`/`ConsumeSpriteAtkRaw`）是否…
  :85    ## r8b（2026-10-12）r8 真机崩溃根因 + 两处改法
  :87    | 结论
  :89    | r8 崩溃根因＝我方新增的 `free(tn)`（`api.type_name()` 的返回值）
  :90    | `NEONPlayerState.get_ProjectileRange` 返回的是**类对象** `Veewo.Stats.Stat`，不是数值
  :91    | r8b 改法（槽位 65 → 66）
  :93    | 已否证/已撤下的结论
  :95    | 「返回类型未知的 getter 可以伪造返回值（一份位型三种读法都对）」
  :96    | 「`api.type_name()` 返回的 `char*` 该由调用方 `free()`」
  :97    | 「`GetCrystalSpriteCount` 抬到 9999 是安全的水晶实现」
  :99    | 待验证的结论
  :101   | r8b 全屏攻击＝射程 stat 实例闸是否命中（子弹是否横穿全屏）
  :102   | r8b 无限水晶＝精灵通道是否真的被调用
  :104   ## r8c（2026-10-12）「无限水晶没效果」的第二个根因：槽位表被上限静默截断
  :106   | 结论
  :108   | r8b 的「无限水晶」**一个钩子都没挂上**：槽位表 66 项而 `kSlotsMax = 64`，安装循环…
  :109   | r8b 的三个全屏新槽**全部挂上了**（射程 stat 通道已就位，未生效原因不在挂点）
  :110   | r8 的 scudo abort 已消除（删掉 `free(tn)` 生效）
  :111   | 水晶精灵通道的挂点现状
  :113   | 待验证的结论
  :115   | r8c 全屏攻击＝`Veewo.Stats.Stat.get_Value` 的射程实例闸是否命中
  :116   | r8c 无限水晶＝精灵通道是否真的是「水晶」的资源通道
  :118   ## r9（2026-10-12）用户 L3 否证自创路线 ⇒ 回到斌哥机制 + 水晶改「无条件使用」
  :122   | 结论
  :124   | **全屏攻击的旧 needle 必然零命中**：`projectilerange` / `range` 在真机 81 个实测 Attri 键…
  :125   | **自创路线「改玩家射程 Stat」已被真机否证**：Stat 实例闸确实命中并强制成功，…
  :126   | **水晶不走 `NEONPlayerState` 的 cost 体系**：整局 `res probe` 48 条里 `costType=2`(Crystal) **…
  :127   | 斌哥「全屏」条目的执行面只能是 `Attri::TryGetValue` 按名抬值（服务端下发的字段…
  :128   | 槽位表上限截断已彻底闭环（r8c 修复有效）
  :130   | 已否证/已撤下的结论
  :132   | 「全屏攻击＝把玩家射程/武器射程抬高」（自创路线：`get_ProjectileRange` → `Veewo.St…
  :133   | 「无限水晶＝水晶的 cost 扣减通道」（`NEONPlayerState` cost / 消耗漏斗）
  :134   | 「返回类型未知的 getter 可以伪造返回值」/「按字段名猜 Attri 键名」
  :136   | 待验证的结论
  :138   | r9 全屏攻击（真机键名路线）是否产生观感变化
  :139   | r9 无限水晶（无条件使用）是否让「使用水晶」永远可用
  :141   ## r10（2026-10-12）用户 m04844 判 r9 两项仍无效 ⇒ 离线坐实键名体系后重定 needle
  :145   | 成立的结论
  :147   | **AutoDict 的键名＝`NEON.Game.Managers.GameConstVariables`（index 10208）的 380 个字段名小写**…
  :148   | 「攻击判定几何」一族键名确实存在：`MeleeCheckDistance`/`ExtraMeleeCheckDistance`/`FinalMele…
  :149   | 水晶计数同属该体系（`leastonespritecrystal`/`currentspritecrystalcount`），与**实测已被请…
  :151   | 已否证/已撤下的结论
  :153   | 「无限水晶＝精灵使用通道」（r9：`UseCrystalSprite` 不足即补 1 再重试 + `ConsumeSpriteA…
  :154   | 「全屏攻击＝子弹体积/寿命」（r9：`bulletsize`→100 + `lifespan`→60，走 Attri 只抬不…
  :156   | 待验证的结论
  :158   | r10 全屏攻击（几何族 needle：checkdistance/damagesize/projectilerange/orbitalradius→99999，bulle…
  :159   | r10 无限水晶（水晶键读点强制 `spritecrystal`/`crystalcount`→9999）是否让「使用水晶」…
  :161   ## r11（2026-10-12）用户 m04997「不行就给我换方法」⇒ 全屏攻击＝伤害广播、无限水晶＝挂 veneer 目标
  :166   | 成立的结论
  :168   | `SpriteManager.GetCrystalSpriteCount` 的入口是 8 字节 veneer `mov w1,#imm ; b <共享实现>`，而入…
  :169   | veneer 可以**不解开也照样拦**：解出 `b` 的符号扩展 imm26 得目标地址，按地址挂钩…
  :170   | 伤害链的最后一跳 `HitPoints::ReallyDeductHP(DamageInfo, Single)` 上可以安全做**伤害广播**…
  :171   | `Api::ScanInstances` 的现实代价边界：它按 klass 指针裸扫 `/proc/self/maps` 的 rw 段，默认…
  :172   | `NEON.Game.Killable`（index 4670）里没有 `hitPoints` 字段（HP 不在 Killable 上）⇒ 想「遍历…
  :174   | 已否证 / 已撤下的结论
  :176   | 「全屏攻击＝在 AutoDict 里抬射击几何键」（r10：checkdistance/damagesize/projectilerange/orb…
  :177   | 「无限水晶＝在 AutoDict 里抬水晶计数键」（r10：spritecrystal/crystalcount→9999）
  :178   | 「`Res_CrystalCount` 槽位里的 GetCrystalSpriteCount 能靠常规按名挂载」
  :180   | 待验证的结论
  :182   | r11 无限水晶（挂 veneer 目标 + 选择子拦截）是否让「使用水晶」不再提示不足
  :183   | r11 全屏攻击（伤害广播）是否让「打一只 → 多只同时掉血」
  :185   ## r12 / r12b（2026-10-12）用户 m05208「进游戏直接闪退了」⇒ 坐实「Dobby 16 字节补丁吃邻居方法入口」（r7 同机理），无限水昌改走 veneer 入口 4 字节补丁
  :187   ### 已成立
  :188   | 结论
  :190   | 进关闪退是**我方新钩子**引入（不是游戏/底包问题），且是**真崩溃**（不是 THEMI…
  :191   | 崩点＝**进关 3 秒**、而我方钩子一行日志都没出 ⇒ 崩在「被我们改坏的邻居方法…
  :192   | 根因坐实：veneer 目标（共享实现）的 **16 字节窗口内另有 1 个方法入口** ⇒ Dobby …
  :193   | 该共享实现的形态不是「极小叶函数」：`code=97ACC81A D101C3FF F90023FE A90557F6` = `BL` / `s…
  :194   | 正确修法：**完全不改共享实现**，只把 veneer 自己的第一条指令（`mov w1,#imm`）换…
  :195   | 真机证据：4 字节补丁路线装上、且共享实现未被修改
  :196   | 常规槽位也补上同一道闸门：入口 16 字节窗口内出现**外来方法入口**即跳过该槽…
  :197   | r12b 装机冷启全程无崩溃
  :199   ### 已否证 / 已撤下的结论
  :200   | 已否证 / 已撤下的结论
  :202   | 「挂 veneer 目标（共享实现）+ Dobby」这条 r11 路线
  :203   | 「函数体前 16 字节无终止指令 ⇒ Dobby 安全」
  :204   | `/data/tombstones/tombstone_12` 是本游戏崩点
  :206   ### 待验证的结论
  :207   | 待验证的结论
  :209   | r12b 进关不再闪退（真正判据＝用户进一局并撑过 first few seconds）
  :210   | 无限水晶是否生效（veneer 入口 4 字节补丁 + 直接调真实现）
  :211   | 全屏攻击（伤害广播）是否让「打一只 → 多只同时掉血」
  :213   ## r14（2026-10-12 03:05）全屏攻击换机制 + 无限水晶改挂「开门解锁成本」
  :219   ### 本轮结论
  :220   | 结论
  :222   | 全屏攻击此前失效且会崩，根因＝**裸内存扫描分不清活体与已释放/复用对象**
  :223   | 全屏攻击新机制只用活体必经入口：`Killable.TakeDamage` 记录 + `Killable.Update` 对同阵…
  :224   | 无限水晶真正的付费路径＝`NEONPlayerState` 的「解锁成本」三连，**不属于** HasEnough/…
  :226   ### 已否证 / 已撤下的结论（r14 追加）
  :227   | 已否证 / 已撤下的结论
  :229   | 「用 `Api::ScanInstances` 按 klass 扫内存找同类怪物再转发伤害」可实现全屏攻击
  :230   | 「无限水晶只要管住 HasEnough/GetAmount/ConsumeRaw/IsCostTypeSupported 就够了」
  :232   ### 待验证的结论（r14 追加）
  :233   | 待验证的结论
  :235   | 全屏攻击真机生效（打一只 → 场上多只同时掉血）且**不再崩**
  :236   | 开门确实经过 `CanUnlock/UnlockCosts`（p1=2 水晶）
  :240   ## r15（2026-10-12 03:28）全屏攻击「打怪就闪退」+ 无限水晶「仍提示需要更多水晶」
  :245   ### 本轮改动（诊断与修复同包）
  :259   ### 本轮结论
  :260   | 结论
  :262   | 全屏攻击闪退的根因＝留用**调用方栈上的 DamageInfo 指针**（不是内存扫描、也不…
  :263   | 门的付费判定走 `CostType.Mana(22)`，`CostType.Crystal(2)` 从未被请求
  :265   ### 已否证 / 已撤下的结论（r15 追加）
  :266   | 已否证 / 已撤下的结论
  :268   | 「r13 崩溃＝内存扫描混入已释放对象」是**完整**解释
  :269   | 「水晶门走 `CanUnlock/CanFreeUnlock/UnlockCosts`」（r14 假设）
  :271   ### 待验证的结论（r15 追加）
  :272   | 待验证的结论
  :274   | 全屏攻击真机生效且**不再崩**
  :275   | 无限水晶（开门）生效
  :277   ## r16 / r17（2026-10-12 04:10）全屏攻击「开关都闪退」+ 0xb8 崩溃终审 + 装机器丢槽
  :282   ### 本轮改动（诊断与修复同包）
  :291   ### 0xb8 崩溃的现场语义（capstone 逐帧）
  :292   | 帧
  :294   | #13
  :295   | #12
  :296   | #05
  :300   ### 本轮结论
  :301   | 结论
  :303   | 0xb8 崩溃不是我们的广播代码、也不是 THEMIS 反外挂：原始故障发生在游戏 `Killable.…
  :304   | THEMIS 只记录、不回退 PC ⇒ 同一条指令被反复触发；转发后 tombstone 的 `__memcpy_aarch…
  :305   | 「每槽位吃一整页跳板页」会在 ±124MB 窗空洞耗尽时**静默丢槽**（功能全绿但没…
  :307   ### 已否证 / 已撤下的结论（r16 追加）
  :308   | 已否证 / 已撤下的结论
  :310   | 「全屏攻击的触发闸只要挂了就会记录」（r15 的 `DamageFromLocalPlayer` 前置）
  :311   | 「ModOff 的『同路径最低映射』＝模块装载基址」（用于把帧地址写成 `库名+偏移`）
  :312   | 「kFullRules 抬高投射物属性是全屏攻击的必要手段」
  :314   ### 待验证的结论（r17 追加）
  :315   | 待验证的结论
  :317   | 全屏攻击真机生效且**开关都不再闪退**
  :318   | 伤害链护栏能在原函数崩溃时就地拦下（不闪退）
  :319   | 无限水晶／无限金币／解锁标准版在 r17 上回归无恙
  :321   ## r17 收官 · 交付结项（2026-10-12 04:25）
  :325   ### 1. L3 判定（全部通过）
  :326   | 功能
  :328   | 全屏攻击
  :329   | 无限水晶
  :330   | 0xb8 崩溃终审
  :332   ### 2. 正式包与交付物
  :341   ### 3. 清理（约 2.9 GB，全部显式名单、无通配符，删后复扫）
  :342   | 类别
  :344   | 08_output 裁剪
  :345   | /tmp 临时件
  :346   | 记录仪
  :347   | 构建产物
  :352   ### 4. 结项结论

## 硬约束.md（7038 B / 33 行）
  :6     | #
  :8     | 1
  :9     | 2
  :10    | 3
  :11    | 4
  :12    | 5
  :14    | 6
  :15    | 7
  :16    | 8
  :17    | 9
  :19    ## 落地检查（每条约束在动代码/装机前必须能回答）
  :26    ## 变更记录
  :28    | 日期
  :30    | 20261011
  :31    | 20261011
  :32    | 20261011
  :33    | 20261011

## 路线锁定.md（12085 B / 103 行）
  :7     ## 0. 两游戏身份对照（**勿混用**）
  :11    | 包名
  :12    | 发行/渠道
  :13    | 版本
  :14    | 体积
  :15    | 引擎
  :16    | 保护
  :17    | 货币面
  :18    | 技能面
  :19    | 标准版
  :26    ## 1. 证据来源
  :36    ## 2. 挂点表（每行 = nhsy2 自己 metadata 实测 token）
  :38    | 功能#
  :40    | ①
  :41    | ②
  :42    | ③
  :43    | ④
  :44    | ⑤
  :45    | ⑥
  :46    | ⑦
  :47    | ⑧
  :49    ## 3. 标准版闸门全集（nhsy2 独有，24 槽）
  :51    | 类（namespace `NEON.Framework.Login`）
  :53    | `StandardEditionClientAccess`
  :60    | `StandardEditionAccessPolicy`
  :64    | `StandardEditionAssaultAccess`
  :67    | `StandardEditionBanAccess`
  :72    | `StandardEditionPatchAccess`
  :77    | `LoginService`
  :81    ## 4. 假设矩阵（⑥ 无先例，必须靠真机验证）
  :83    | 假设
  :85    | 闸门全部是 **static** 谓词/enum 返回，无实例依赖
  :86    | 恒真即等于「已购买标准版」
  :87    | `StandardEditionAccessResult.Allowed` 枚举值运行时读字面量即可
  :89    ## 5. 已否定的方案（防重犯）
  :91    | 日期
  :93    | 2026-10-08
  :94    | 2026-10-08
  :95    | 2026-10-08
  :96    | 2026-10-08
  :97    | 2026-10-08
  :98    | 2026-10-08
  :99    | 2026-10-08
  :100   | 2026-10-08
  :101   | 2026-10-12
  :102   | 2026-10-12
  :103   | 2026-10-12
