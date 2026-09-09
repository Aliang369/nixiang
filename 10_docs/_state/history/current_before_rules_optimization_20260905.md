# 会话锚点（当前状态）

> 每次会话开始先读本文件；每次会话结束更新它（几行即可）。防跑偏用。

- **当前游戏**：无限升级（`com.xingzuodaoshi.infinityUpgrade`，11.266018 / vc 1637）— 分析完成（2026-09-04），见下方「无限升级」节
- **上一游戏**：超维进化（`com.grinning.beta.yszl`，v2.1 / vc 11）— 星空包已出但**装机卡死暂缓**（2026-09-05）：换签原包也卡死 = 游戏有重包校验，非星光注入问题。校验点不在 Java/JS（已排查），疑在 GZQYActivity native qyAn/qyEn/qyDe 或 cn.gravity 自杀逻辑或 so 内。下一步：抓卡死瞬时栈（debuggerd/tombstone）→ 定位校验点 → stub 绕过。全记录在 `10_docs/超维进化/初判_2.1_20260904.md`「装机实测结论」
- **上上游戏**：追迹迷城（`com.TeaMelo.DungeonChasers`，game_id=`zjmc`）1.0.20260904 星空包已装机实测通过，可分发
- **无限升级要点**（2026-09-04）：Unity IL2CPP 2022.3.62f2c1 + HybridCLR 热更 + YooAsset；**爱加密壳（HTPX metadata 自定义加密 + bundle blocksInfo 自定义混淆 + so 剥符号）**；反作弊 ACTk(ObscuredLong/Int) + DamageCheatRecord + ItemUseAnomaly(服务端RPC) + Themis + Bugly
  - **突破路径**：运行时 `/proc/pid/mem` dump mmap 段拿明文 metadata（仅头 8 字节 HTPX 水印，主体明文；修头后 Il2CppDumper metadata v31 可读）+ 堆搜 BSJB 提取 16 个热更 dll（MZ 头被 Obfuz 抹，以 BSJB 反查 PE+SizeOfRawData 重建）
  - **热更主程序集 = `targetfuntional.dll`**（1373 类/9776 方法，全游戏逻辑，类名未混淆），已反编译 1006 cs → `03_decompiled/无限升级/decompiled_targetfuntional/`
  - 七项挂点：免广告=`AdsManager.ShowAdsNewFlow.Show/_adsFreeTimeLeft/adsCallback`；无敌=`PlayerMgr.OnHit`(hp-=val @330)；秒杀=`Enemy.OnHit`(Hp-=targetD @573，Boss/WorldBoss 有 override)；无限技能=`Character.currentCoolDown`+`GameData.GetSkillUseAmount`；广告奖励=`NetWorkPlayerMgr`1865 附近 RewardTextMgr 发放；物品=`GameData.MinusItem`@1266/AddItem@1222；技能合成升级=`SkillDetialWindow`(灵魂成本 CalculateSkillUpgrad/CanLevelUp/s.unLevelUp)
  - **风控点（⚠️ 待验证影响）**：伤害/物品异常均有服务端上报（DamageCheatRecord/ItemUseAnomaly RPC）+ 广告 15s 间隔 CheckBan + 每日广告上限 40；单机逻辑但带服务端记录
  - YooAsset 内置资源在 APK `assets/lib/andres/`（hash 文件名）；CDN 热更落 `/data/data/.../no_backup/.syscache/.pkg/andres/BundleFiles/`
  - **YAES 已全破（2026-09-05 真机 memdump）**：`[4B "YAES"][1B ver=1][16B IV][AES-CBC/PKCS7]`，Key=PBKDF2-HMAC-SHA1("gamemen2026","c698rish2210",10000,32)（live statics[8] 实锤吻合；10000 是代码硬编码，metadata 20000 未用）；YooAssetAesCrypto 15 方法 VA 全拿到；脚本 `04_scripts/无限升级/yae_crypto.py`
  - **关键分界（2026-09-05）**：APK 内置 rawfile（283207f9）头是明文 UnityFS、**不穿 YAES**（YAES 管 CDN bundle），堆里 dll 明文也不在包内 → 内置包数据块=爱加密自有混淆（未逆）→ **①重封走内置包需再逆爱加密方案；②starry hook 热更 dll 加载边界绕开全部 bundle 加密，仍是最优主路线**
  - **真机 memdump 方法已验证可复用**：文件映射 7 段按 file offset 回填 + `[anon:.bss]` 6 段（壳把 CodeRegistration 搬这里，grep 'il2cpp' 会漏！）；设备 shell 32 位算术坑（skip 必须本地算好传十进制，否则静默全零页）；详见 `10_docs/无限升级/运行时dump与YAES破解_20260905.md`
  - **方法 VA 定位路线（免 IDA，可复用）**：bss 搜「指向 rodata 模块名字符串的运行时绝对指针」→ Il2CppCodeGenModule → methodPointers[rid-1]；v31 metadata 自解析（typeDef=88B/methodDef=36B）；脚本 `resolve_il2cpp_method_va.py`
  - **rawfile 复测结论（2026-09-05）**：头+blocksInfo = 标准 UnityFS v8 未加密（skip-16-byte uncompressedDataHash 后 100% 干净解析，skip-0 全失败→hash 字段实锤；flags=0x243 无 0x1400 加密位）；**数据块 = 自定义局部加密**（按块 LZ4/手工解码/整区单流全失败，SerializedFile 头后撞高熵区，深处仍有 BSJB/mscorlib 明文字面量）→ 等价「真加密」分支
  - **路线 A 新情报**：APK 内置 rawfile 与映射表 hash 对不上（表内 targetfuntional=28417c03 / mscorlib=2b9822d2 不在 APK；实际 283207f9≈targetfuntional、8d1b50e6≈mscorlib，node size 吻合 ⚠️待验证）→ 映射表疑来自 CDN manifest；替换目标=283207f9；改后重封需防 CDN 缓存覆盖（游戏需登录不能断网）
  - **复测脚本**：`04_scripts/无限升级/retest_rawfile_blocksinfo.py` + `diag_rawfile_data.py` + `diag_rawfile_lz4mode.py`（rawfile 已解到 /tmp/wxsj/rawfile_apk/）
  - **实现路线**：改热更 dll IL + 重封 rawfile（native hook 对 HybridCLR 无效）；菜单控制=浮窗写配置+dll 内读配置开关；上报屏蔽=置空 ItemUseAnomalyMonitor/DamageCheatRecordData/CheckBan + SendBA/SendRT 黑名单过滤（保留登录心跳）
- **详细进度**：`10_docs/无限升级/分析进度_20260904.md`
- **无限升级下一步最小动作（2026-09-05）**：**星空包已完成实测**（无敌/秒杀/免广告+×100/物品不减/翻倍/技能x1x10x100近免费 全通过）。剩余：①后台注册 `game_id=wxsj`+发卡（用户运营侧）→ 菜单开关接管配置；②分发即可（`08_output/无限升级/无限升级_11.266018_surgical-releaseSigned.apk`）
- **无限升级当前阶段（2026-09-05 收官）**：memdump 破壳 → YAES 全破 → IL 注入 → 星光包装机实测全功能通过。注意：一键升级(QuickUpgrade) 在 skill_free 下原生路径 tagged-pointer 崩溃，已禁用（x1/x10/x100 近免费可用）；菜单对未授权游戏 60s 级全量推 OFF（发卡后消失）；完整战报见 `10_docs/无限升级/运行时dump与YAES破解_20260905.md §九`
- **超维进化要点**（2026-09-05 星空包已出，待装机）：Cocos Creator 2.x JS 明文无壳无校验；功能=秒杀/无敌/回血回满/蓝无消耗/暴击/免广告（6 Toggle，featureNum=get_ret 1..6）；实现=CwjhMod（se::evalString 内存 splice game.js，7 锚点已验唯一，JS 不改包）；首包=QLFH 模板（StarryApp→SilverdewApplication，classes3 patch AppActivity，星光→**classes4.dex**）；game_id=`cwjh`；APK 自检全过（so 标签/classes4/Manifest aapt/game.js hash 一致）；**产物 `08_output/超维进化/超维进化_2.1_surgical-releaseSigned.apk`**；装机验证清单在 `10_docs/超维进化/初判_2.1_20260904.md` 末尾（logcat 看 `[Cwjh] early se hook ok` + `combat splice #1`）
- **2026-09-04 战报**：
  - 14 hook RVA 全部重算（Il2CppDumper，方法签名/ELF bias 0x4000 均未变），见 `10_docs/追迹迷城/星光落地_1.0.20260904_20260904.md`
  - 证书 MD5 与 20260824 相同，TapSign 补丁常量不变
  - **重大结构变化①**：zip 追加 classes7.dex 在本机 ColorOS 上 ART 不再自动发现 → SignSpoof CNF 闪退；改为 **starry smali（119 文件）合并进 classes4.dex 单 dex**，[4b/7] 步骤
  - **重大结构变化②**：穿山甲 service 槽 33→34 字符 → 新增 `MenuService2`（34 字符空子类），`MenuService.start()` 用 `resolveService` 探测后切换；Manifest 等长补丁已更新
  - dex 布局：TapSign/ClientLoginUtils/PrivacyActivity/UnityPlayerActivity 全挪到 classes4；无 GameActivity
- **旁路（冰晶菜单试验皮 `starry-mod-menu_111`）**：账号长按剪贴板条可用；资源走 so 尾部 SMBT；**展开面板小屏等比 fit（scale≤1）已装机待验**
- **下一步最小动作**：无（本次收尾完成）。新闪退反馈进来时 → 读落地文档「挂起」节：秒杀钩跳过 Monster.PDH / 无限技能怪物侧放行 / AOE 跳流程，三嫌疑与实锤途径已记录
- **挂起（用户决策暂缓）**：分发玩家反馈「怪物技能使用后偶发闪退」——嫌疑与修复方向在 `10_docs/追迹迷城/星光落地_1.0.20260904_20260904.md` 末节，未修
- **跨签名注意**：本次已卸载官方版装星光版（数据已清一次）；此后覆盖安装即可，禁止清数据

## 关键路径速查

- 落地文档：`10_docs/追迹迷城/星光落地_1.0.20260904_20260904.md`（RVA 对照表 + 结构变化 + 教训）
- 抓点（语义参考）：`10_docs/追迹迷城/追迹迷城_菜单功能抓点_20260812.md`
- 构建：`mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20260904.sh`（classes4 合并版）
- Manifest 生成：`04_scripts/追迹迷城/gen_zjmc_binary_manifest.sh`（默认版本 20260904）
- dump 产物：`/tmp/zjmc_new/dump/`（tmp 会清；RVA 以落地文档/ZjmcMod.cpp 为准）
- logcat：`-v threadtime` 全量；关键词 `StarryApp | auth game_id | [Zjmc] | native menu`

## 终末地（2026-09-04 收尾：四功能全通 ✅）

- 1.5.3 mod 五步流程**完成**：百倍伤害 / 无敌（带敌我区分）/ 无限跳跃 / 无限技能全部实测生效
- 最终形态 = v14.3：按名 hook 四挂点（`BattleFormula.CalculateDamage` static double 返回 ×100、
  `AbilitySystem.ApplyModifier` + 按 klass 解析 `get_factionIndex` override（Good=2 玩家 / Bad=3 怪）、
  `CanJumpAgain`、`Skill.CheckCd`）；构建脚本 `surgical_endfield_1.5.3.sh`
- 修复史 v0→v14 与教训清单见 `10_docs/终末地/初判_1.5.3_20260903.md` 末节「v5-v14 修复史」
- 收尾清理（2026-09-04，用户确认）：删 1.4.3 官方包 / BG 版包×2 / 终末地BG 反编译 / off_baseline dump / logcat 大日志 / /tmp/ef_meta / **IDA 数据库（id0/id1 等 ~2.3G）**（共 ~4.4G）；**必留**：`02_apks/终末地/终末地_1.5.3_base.apk` + `libil2cpp_1.5.3.so`（重做 IDA 从 so 重开即可）、`08_output/终末地/` 终局包、on_* 小 dump 证据
- 下一步：无（分发即可）。新反馈进来先读该节「嫌疑清单」
- 宣传图（2026-09-04）：`12_images/终末地/终末地_宣传图.png`（官方 icon + 四功能文案已确认出图）

## 经验回流（下次会话处理）

- [x] 终末地 v5-v14 教训沉淀进 `skill/re-implement`（2026-09-05 用户同意后已沉淀：hook-rules §18 桩签名三查/§19 按 klass 取 override/§20 主线程零解引用 + §9 堆扫/§16 SyncHooks 并发扩充；code-review-checklist 边界②③ + 2.1/2.3/4.5）
- [ ] 「classes7 自动发现失效→合并 classes4」+「MenuService2 等长槽」沉淀进 `skill/starry-mod`（需用户同意）
- [x] 终末地 `初判_1.5.3` 文档补 v5-v14 战报（2026-09-04 已补）
