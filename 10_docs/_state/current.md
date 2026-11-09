<!-- GENERATED: 由 04_scripts/_common/gen_state_index.py 生成，禁止手改 -->

# 跨游戏状态索引

> **本文件是生成物**：手工修改会在下次生成时被覆盖（`preflight` 用 `--check` 校验）。
> 真源＝`10_docs/_state/games/*.md` 头部的 frontmatter（`game`/`status`/`updated`/`one_line`）。
> 只放**当前工作集**：`status: active|blocked`，或近 14 天有更新，最多 30 行 —— 体量与游戏总数无关。
> 全量索引见 [`archive_index.md`](archive_index.md)（共 101 个游戏），逐个详情见 `games/{游戏}.md`。
> 改状态＝改对应 `games/{游戏}.md` 的 frontmatter，然后跑：`python3 04_scripts/_common/gen_state_index.py`

## 当前工作集

| 游戏 | 最近更新 | 状态与摘要 |
|---|---|---|
| [农场守护者](games/农场守护者.md) | 2026-11-09 | 🟡 斌哥经典壳识别完成：3 项开关（百倍伤害/无敌/无限技能）位 0/1/2 已确认；hook 方法待在热更 dll（已加密） |
| [动物大联盟](games/动物大联盟.md) | 2026-11-08 | 🟡 壳已过（c2 证书指纹修正），改包仍卡「数据检查」：点同意不发起资源请求。apksecurefile 已全解（MDXZ+zstd+AES，3 dex 可反编译），但 0x66210 哈希是壳必需材料，改容器须重算 KEY（派生未逆出）… |
| [消消英雄2](games/消消英雄2.md) | 2026-10-09 | 🟡 1.2.28 已出包装机；过检测与上报拦截实测通过 |
| [命运](games/命运.md) | 2026-09-22 | ⚠️ 已装机但被 ACE 击杀（灰屏崩溃循环），2026-09-22 用户要求暂停；ACE AB 实验结论见正文 |
| [遗弃之地](games/遗弃之地.md) | 2026-09-21 | ⚠️ 用户决定终止：收尾＋清理全部产物只留 10_docs 文档；标准外科路线「有窄口径可能，未证明可行」 |
| [百分之一](games/百分之一.md) | 2026-09-11 | ⚠️ 1.1.78/vc428：产物已清理（02_apks/03_decompiled 清空），分析结论见正文 |
| [虚无之刃](games/虚无之刃.md) | 2026-09-09 | ⚠️ 1.39.160 未完成（20260909 盘点更正）：仅有 xwzr_so_only.sh 重编 so 塞已有外科签包 |
| [大话蜀山](games/大话蜀山.md) | 2026-11-09 | ✅ 2.7/vc13 重出星空包：2.7 dump 复核抓点全一致（isSingle@0x10 / Camp@0x670 未变）；TapTap 走 A 轨改包法（classes2.dex 四处补丁，无 SignSpoof、无 B 轨 ki… |
| [我在神界刷装备](games/我在神界刷装备.md) | 2026-11-09 | ✅ 161.0 复刻完成并装机：六段 JS 锚点 5 段沿用、热更版 UploadPlaySpeed 别名 m→y 已改双候选；签名走 B 轨 killer（未用 SignSpoof）；用户验收四项功能正常，Tap SSO 未验证 |
| [旷野征途](games/旷野征途.md) | 2026-11-09 | ✅ 斌哥 WB 菜单抓点完成：9 项功能（道具/资源×3/战斗×3）静态解密 + 实测闭环，目标符号明文可取；等官方 1.4 原包做星空版 |
| [境界刀鸣国际CR](games/境界刀鸣国际CR.md) | 2026-11-08 | ✅ 已交付（Crunchyroll 渠道，脱离 Google 依赖）：libgame.so 与国服/GP 全同 ⇒ 同源；无 hhld/PairIP/GSDK；单包全覆盖安装，三功能玩家实测通过。坑：CR 不自带 libc++_share… |
| [宝石战争](games/宝石战争.md) | 2026-11-08 | ✅ 热更 1.0.0.14 星空版替换（md5 4a23163f，526,663,187B）：真机成品包当底包 + 9 个官方增量；4 锚点全命中、142 资产逐条校验、覆盖安装保数据；预检 PASS_TO_HUMAN（killer 挂载… |
| [境界刀鸣国际](games/境界刀鸣国际.md) | 2026-11-07 | ✅ 已交付：与国服同源（libgame.so 挂点逐一相同），hhld kill-switch NOP；独立 id jjdmintl（先例 gjc2intl），login 200 自愈；正式包合并为统一单包（universal，删 spl… |
| [墨守孤城](games/墨守孤城.md) | 2026-11-07 | ✅ ⛔ 收尾：Unity DOTS+Burst+HybridCLR 三层封死注入面，四项功能未实现；已出可玩包，全部地址/坑位已归档 |
| [暗影格斗3国际](games/暗影格斗3国际.md) | 2026-11-07 | ✅ ⛔ 停做：Play 渠道服务端内容哈希。四条路线全失败（改包/frida/ptrace/内存读写），两次封号（1h/1d）。wrap 注入可让 hook 生效但必报 InvalidHashes，self-hide 两路无效 |
| [无悔华夏](games/无悔华夏.md) | 2026-11-06 | ✅ 3.8.305 三渠道无壳星光全部验收（九游 13dex+重标RVA；4399 6dex；B站 6dex+RVA零重标），各 11 项 |
| [一念通天](games/一念通天.md) | 2026-11-05 | ✅ 779 六项实测通过收官；9 RVA 漂移但序言 0 变化（只换 8 kRva + adRequest 0x478） |
| [修仙大陆模拟器](games/修仙大陆模拟器.md) | 2026-11-04 | ✅ 1.0.5 星空版收官：5 项＝资源/灵石/仙晶（按钮加值）+体力回满+免广告；jsc key 6aMz7p1tlGtr6MeR（解出即明文）；TapTap 双轨（A 改包 isClientLogin 恒 false + B 运行时 … |
| [勇士之心](games/勇士之心.md) | 2026-11-04 | ✅ 加固/v3/sha 全下线+ 热更 1.1.14.1 自动战斗修复；已装雷电，L3 待实测 |
| [挂机打怪兽](games/挂机打怪兽.md) | 2026-11-04 | ✅ 1.1.65 星空版收官：7 项（金币/宝石/碎片满值＝只加不改、战斗加速、无敌、百倍伤害、免广告＝jrggcs≥28）；jsc 解密 key 已取得（74f38ae4-b95f-42）；收尾交付唯一正式包 |
| [无限升级](games/无限升级.md) | 2026-11-04 | ✅ 11 线 vc1637 定制两版已过 L3（正常版 feat22 24 项 / 屏蔽版 feat21 23 项）：三功能+三开关+签名轨（TapSDK 验签走运行时签名替换）；12 线 wxsjdz2 十四项仍等收尾口令 |
| [生存33天](games/生存33天.md) | 2026-11-04 | ✅ 重装官方 1.0.24 后复刻星空版（热更 v203）：五项功能同前；fight 锚点再次漂移已改结构派生，首启 fight=1 baseDmg=1 atk=1，L3 用户实测通过 ⇒ 已收尾 |
| [黎明前20分钟](games/黎明前20分钟.md) | 2026-11-04 | ✅ 1.0.106 星空版收官：4 项＝无敌/百倍伤害(1-100)/战斗加速(1-5)/免广告（ShowRewardAd 拦截 + 广告卷恒 99）；TapTap 双轨（B killer 挂 appComponentFactory + … |
| [修仙王朝模拟器](games/修仙王朝模拟器.md) | 2026-11-03 | ✅ 1.0.4 收官：Cocos 3.8.7 XXTEA key 已解全量 JS（185 模块）；功能＝免广告（广告入口直发奖）+ 超级无敌（清剿/深海胜率抬 100）；TapTap 新 SDK 五处改包法… |
| [异世界当欧皇](games/异世界当欧皇.md) | 2026-11-03 | ✅ 1.0.104 收官（md5 4c762bab，103,336,514 B）：Cocos jsc 四项补丁（伤害倍率1-100/无敌/加速5×/免广告）+ jsb.sqlite 通道 + TapTap 改包法强制网页登录；门禁 PAS… |
| [战魂铭人](games/战魂铭人.md) | 2026-11-03 | ✅ 八功能收官（新增无限技能/局内无限金币，用户 L3 通过；菜单并入「战斗」不新开分类）。⚠️ hotfix 3.7.0.3 程序集再漂移（20,048,400 B 加密包）；frida 取程序集必须先 il2cpp_thread_at… |
| [明日方舟](games/明日方舟.md) | 2026-11-03 | ✅ 2.7.91 七项 L3 通过并收官（hooked 10/10、hit 5、零 ANR；唯一正式包 md5 b689894c） |
| [梦幻回忆（梦幻封神）](games/梦幻回忆.md) | 2026-11-03 | ✅ ⛔ 已收尾：服务端权威（ReqBattleCommand→服务端算→快照覆盖），客户端功能不可实现；四层防护（GDRC签名/protected_paths/12文件清单/base_id热更新）逐层实测 |
| [不一样修仙2](games/不一样修仙2.md) | 2026-11-02 | ✅ 1.4.135/vc1007 星空版收官（直装，官包 dex/arsc/so 逐字节未动、星光在 classes6.dex）；14 项全过，L3 用户「功能都没问题了」；含免广告（钩 JedgeCanBlockAd@0xF7A5C0）… |
| [不一样的修仙宗门2](games/不一样的修仙宗门2.md) | 2026-11-02 | ✅ TapTap 1.1.8 收官（外科：只换 project.jsc + 新增 classes6.dex/so）；无限仙玉/灵石/免广告，L3 通过；关键结论：雷电 houdini 下 native hook 与 evalString … |

> 本表之外的游戏＝近 14 天无更新且非 active，全部在 [`archive_index.md`](archive_index.md)。

## 基础设施与经验

- [状态目录规则](games/README.md)
- [基础设施与工作区历史](games/infrastructure.md)
- [挂起与候选经验](pending_experience.md)
- [规范普查（2026-10-27）：规则文档清单/冲突/问题](规范普查_20261027.md)

## 历史来源

- [整理前 current 完整快照](history/current_before_rules_optimization_20260905.md)
- [拆分前完整快照](history/current_before_split_20260920.md)
- [2026-10-27 去重压缩前全文](history/current_before_compact_20261027.md)
