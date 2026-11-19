<!-- GENERATED: 由 04_scripts/_common/gen_state_index.py 生成，禁止手改 -->

# 跨游戏状态索引

> **本文件是生成物**：手工修改会在下次生成时被覆盖（`preflight` 用 `--check` 校验）。
> 真源＝`10_docs/_state/games/*.md` 头部的 frontmatter（`game`/`status`/`updated`/`one_line`）。
> 只放**当前工作集**：`status: active|blocked`（且未超 --stale-days 挂起闸），或近 14 天有更新，最多 50 行。
> 全量索引见 [`archive_index.md`](archive_index.md)（共 106 个游戏），逐个详情见 `games/{游戏}.md`。
> 改状态＝改对应 `games/{游戏}.md` 的 frontmatter，然后跑：`python3 04_scripts/_common/gen_state_index.py`

## 当前工作集

| 游戏 | 最近更新 | 状态与摘要 |
|---|---|---|
| [旷野征途](games/旷野征途.md) | 2026-11-16 | 🟡 2.4/vc24 星光版收官（2026-11-16）：已装机、Tap Init accepted=True 无 1001、7 项功能用户 L3 确认正常；战斗 argBase 槽位与网页登录路由未取判据；正式包见 08_output/旷野征途 |
| [阿比斯少年冒险团](games/阿比斯少年冒险团.md) | 2026-11-16 | 🟡 v2.1 三问题修复已交付并用户实测通过（倍率只对玩家来源生效 / 无敌改为伤害同帧拦截+属性兜底 / 无限技能两桩补开关透传）⇒ 唯一正式包 abssnmxt_2.36_v1.1.apk md5 33551ecb…(1,133,647,158 B) |
| [农场守护者](games/农场守护者.md) | 2026-11-11 | 🟡 2.1.21 星光包四项全通（md5 3a92930e / sha256 20124872）：百倍伤害=给怪补扣99Δ、无敌=回填curHp、无限技能=清CD、解锁加速=插Cid3月卡；四开战斗胜利+结算正常 |
| [动物大联盟](games/动物大联盟.md) | 2026-11-08 | 🟡 壳已过（c2 证书指纹修正），改包仍卡「数据检查」：点同意不发起资源请求。apksecurefile 已全解（MDXZ+zstd+AES，3 dex 可反编译），但 0x66210 哈希是壳必需材料，改容器须重算 KEY（派生未逆出）… |
| [消消英雄2](games/消消英雄2.md) | 2026-10-09 | 🟡 1.2.28 已出包装机；过检测与上报拦截实测通过 |
| [无限升级](games/无限升级.md) | 2026-11-19 | ✅ 2026-11-19 交付 hotblock（=allfix 载荷 2,760,704 / 0xb849f6ff6ff453d4 + 禁热更失配钉死）；秘境闪退已修复——根因补丁器短跳越界未升级（br 目标 −118）；禁热更补强：未知官方版本(PINNED)仍注入已适配变体，不再 PASSTHROUGH；历史 hf5/hf6/hf7/hf7b/oneshotfix/oneshotfix2 作废；必过关 ilraw |
| [冒险与征途](games/冒险与征途.md) | 2026-11-16 | ✅ B站渠道 2.0.1 四项功能用户全通（v2 交付 md5 52a9691e…，so 53d7e6c1…）；挂点 RVA 与 1.5.9 字节级一致；game_id=mxyztbili |
| [999秒幸存者](games/999秒幸存者.md) | 2026-11-15 | ✅ V0.0866/vc866 星光版六项收官（用户 L3：全部功能正常）：无敌(hpcheck+enemyhurt/enemyhurtnotnac 拦截)/伤害倍率(nmhurte dimmul)/移速(player.move stat.sp)/解锁皮肤(onelock.haveac)/无限券(数据型 sd.unlockcoupon=0xBC=9999)/免广告(getadcheck 保链→LoadRewardAd 拦下直调 reward)；正式包 md5 77ee50fc…（95,421,205 B，so 044fdc0e→e8f37696）；真机 arm64 实测，雷电 houdini 下 RVA hook 不可用（so 全段 r--） |
| [一念通天](games/一念通天.md) | 2026-11-15 | ✅ 789 收官：六项+TapTap 网页登录用户实测全通过；9 RVA 全漂移、序言 3 变 6 同、字段 0 变化；8/8 挂点日志留存 |
| [汉克旅店](games/汉克旅店.md) | 2026-11-15 | ✅ 1.1.0(vc337,TapTap) 星光版：IL2CPP+HybridCLR 热更程序集换字节，八项功能全部用户实测 PASS（⑦移动速度 ⑧无限蓝 于 2026-11-15 so-only 叠加后 L3 终审通过） |
| [群侠传：幸存者](games/群侠传：幸存者.md) | 2026-11-15 | ✅ 0.2.3.10 星光菜单版已装机并通过用户 L3（三功能 + TapTap 网页登录均正常）；登录＝改包法四步含签名回灌 ＋ 单游戏 SPI GameHookImpl 走 core.TapTapWebLogin；待补：云端银钱场景未单独压测 |
| [动物餐厅](games/动物餐厅.md) | 2026-11-14 | ✅ 12.42/vc190 星光版收官：正式包 sha256 3a54d35097e7bfbb4be214e4d6a58509a68a0b6ed056dcfdf1957b921dee97bf（603,849,036 B / md5 f640ebe5…），用户 L3 实测免广告+免内购均正常、登录链 status=0 无正版提示 |
| [迷雾大陆](games/迷雾大陆.md) | 2026-11-14 | ✅ 16.0.0/vc21 收官：修复包 a4e9411d…（sha256 c151413c…）用户实测「功能都没问题了」= 六项 L3 通过；根因＝R8 改名致 MwdlHooks 缺失（已修） |
| [霸海争锋](games/霸海争锋.md) | 2026-11-14 | ✅ 1.1.036 b8 已交付：用户真机实测无敌/秒杀可用且敌人不生效（2026-11-14 03:24 终审） |
| [追迹迷城](games/追迹迷城.md) | 2026-11-11 | ✅ 1.0.20261111 单轨最终版收官交付：用户 L3 原话「可以taptap登录…功能都已测试完…可以进行收尾+正式包+清理+git」；唯一正式包 sha256 cfc963e3 / md5 ab8e788b（1,149,664,487 B）已装机 f9a116a2；08 裁剪为唯一包、清理从工作区移出 3.17 GiB 至 Trash（未清空回收站、物理磁盘未释放）、官方包留 02_apks |
| [勇士之心](games/勇士之心.md) | 2026-11-10 | ✅ 热更 1.1.18.0：卡密即时生效(注入解耦/方案A)+自动重复挑战三连坑修复；自动穿戴下版本修 |
| [宝石战争](games/宝石战争.md) | 2026-11-10 | ✅ 热更 1.0.0.15 星空版替换（md5 dfc6a705，526,663,187B）：真机星光包当底包 + 3 个官方增量；4 锚点全命中、142 资产=3 增量+139 底包（140 项 hash 与底包一致）；覆盖安装保数据、外部 HotUpdate 清零；预检 PASS_TO_HUMAN，G2 ✓（用户「功能都没问题了」） |
| [大话蜀山](games/大话蜀山.md) | 2026-11-09 | ✅ 2.7/vc13 重出星空包：2.7 dump 复核抓点全一致（isSingle@0x10 / Camp@0x670 未变）；TapTap 走 A 轨改包法（classes2.dex 四处补丁，无 SignSpoof、无 B 轨 ki… |
| [我在神界刷装备](games/我在神界刷装备.md) | 2026-11-09 | ✅ 161.0 复刻完成并装机：六段 JS 锚点 5 段沿用、热更版 UploadPlaySpeed 别名 m→y 已改双候选；签名走 B 轨 killer（未用 SignSpoof）；用户验收四项功能正常，Tap SSO 未验证 |
| [境界刀鸣国际CR](games/境界刀鸣国际CR.md) | 2026-11-08 | ✅ 已交付（Crunchyroll 渠道，脱离 Google 依赖）：libgame.so 与国服/GP 全同 ⇒ 同源；无 hhld/PairIP/GSDK；单包全覆盖安装，三功能玩家实测通过。坑：CR 不自带 libc++_share… |
| [境界刀鸣国际](games/境界刀鸣国际.md) | 2026-11-07 | ✅ 已交付：与国服同源（libgame.so 挂点逐一相同），hhld kill-switch NOP；独立 id jjdmintl（先例 gjc2intl），login 200 自愈；正式包合并为统一单包（universal，删 spl… |
| [墨守孤城](games/墨守孤城.md) | 2026-11-07 | ✅ ⛔ 收尾：Unity DOTS+Burst+HybridCLR 三层封死注入面，四项功能未实现；已出可玩包，全部地址/坑位已归档 |
| [暗影格斗3国际](games/暗影格斗3国际.md) | 2026-11-07 | ✅ ⛔ 停做：Play 渠道服务端内容哈希。四条路线全失败（改包/frida/ptrace/内存读写），两次封号（1h/1d）。wrap 注入可让 hook 生效但必报 InvalidHashes，self-hide 两路无效 |
| [无悔华夏](games/无悔华夏.md) | 2026-11-06 | ✅ 3.8.305 三渠道无壳星光全部验收（九游 13dex+重标RVA；4399 6dex；B站 6dex+RVA零重标），各 11 项 |

> ⏸ 4 个 active/blocked 已超 45 天无更新，视为挂起、不进工作集：命运、遗弃之地、百分之一、虚无之刃（详见 `archive_index.md`）

> 本表之外的游戏＝近 14 天无更新、超出 50 行上限或已挂起，全部在 [`archive_index.md`](archive_index.md)。

## 基础设施与经验

- [状态目录规则](games/README.md)
- [基础设施与工作区历史](games/infrastructure.md)
- [挂起与候选经验](pending_experience.md)
- [规范普查（2026-10-27）：规则文档清单/冲突/问题](规范普查_20261027.md)

## 历史来源

- [整理前 current 完整快照](history/current_before_rules_optimization_20260905.md)
- [拆分前完整快照](history/current_before_split_20260920.md)
- [2026-10-27 去重压缩前全文](history/current_before_compact_20261027.md)
