-- ======================== IMO MOD BEGIN ========================
-- 伊莫 本地测试 MOD:  ①百倍伤害(命中多发包) ②玩家无敌 ③无限跳跃 ④移速x3 ⑤无限体力
--                    ⑥自动开宝箱 / 自动拾取(借游戏自己的 actWhenArrive 自动交互系统)
--                    ⑦跳跃瞬移到最近的宝箱【正上方】(借游戏自己的 forbidPositionCheck 合法传送流程)
--                    ⑦-b 强制开箱(绕过客户端那一整套 canInteractive 门, 并把服务器返回码打进日志)
-- 暂不接开关, 全部默认开; 运行时可改 IMO_MOD.xxx
--
-- 【2026-09-29 实机进展】①百倍伤害: 技能已生效, 普攻未生效。
--   ★这条反馈本身证明了两件事: (a) 服务器【没有】按 combatContext.id 去重, 多发包路子成立;
--     (b) 普攻的命中上报走的是另一条 RPC, 不在 DMG_OUT 白名单里。
--   -> 已加 MOD.rpcTrace 自动对照(见下), 一轮实机即可定位那条 RPC 名。
--   ★★★【已由实机日志定位 2026-09-29 19:31】普攻走的是 RPC_CS_SetHitActionTimeline:
--     全库唯一发送点 @GameApp_Ability_ClientCombatAction.lua:2197, 而且是【从被打的那一方】发出的。
--     对 client-master 的怪, 这是 calcResult 尾部唯一的客户端->服务器上报。日志原文:
--     rpc:RPC_CS_SetHitActionTimeline from=ClientPuppet(3868246) mine=false wrap=traceonly AMP=no <== SUSPECT
--     -> 已按【谁是受害者】分流: 发送者不是我=放大; 发送者是我=丢掉
--        (顺带补上一个无敌漏口, 因为 calcResult 这条路没经过 Utils.checkValidTarget)。
--   ★★★【2026-09-29 19:41 实机】受击方上报已确认在放大:
--     rpc:RPC_CS_SetHitActionTimeline from=ClientPuppet(3918198) mine=false wrap=full AMP=x100(victim)
--     + dmg resend RPC_CS_SetHitActionTimeline x100 (victim=3918198)
--     ★这一轮日志还有个更重要的发现: 整条战斗序列里【一条 ClientMainPlayer 的战斗 RPC 都没有】,
--       全是 ClientPet 在打 —— 本作是【宠物在战斗】, 所谓"普攻"就是宠物的普攻。
--     宠物投射物技能的完整序列(实机):
--       CastAbilityOnTarget -> SetCombatActionTimeline -> AddProjectile
--       -> ProjectileHitTarget(AMP=x100) -> SetHitActionTimeline(AMP=x100(victim)) -> ProjectileDestroy
--   ★★★【2026-09-29 19:50 实机·移速已结案】只读探针给出决定性答案:
--     probe:motionAttr:getRunSpeedRatio = 3 (日志里 5 条全是它, 没有 getRunSpeed)
--     => C# 只拉【倍率】这一项, 所以是精确 x3, 没有被乘两次, speedMul 不用改成 sqrt(3)。
--   ☠同一轮教训: 分段窗口不过滤的话, 25 个名额会被 Heartbeat / SyncMapBlockId /
--     RequestCreateClientEntity / 别的怪的 CharacterStateChange 占满, 真正的命中上报一条抓不到;
--     而且后一次施法会把前一次的窗口抢走。已改成【只让战斗相关 RPC 占名额】+【每次施法打小结】。
--   ★★★【2026-09-29 20:xx 已静态坐实: "只有个别技能有连击"的结构性原因】
--     分段抓取抓到一次完整的宠物投射物施法(abilityId=13710000):
--       CastAbilityOnTarget -> SetCombatActionTimeline -> AddProjectile
--       -> ProjectileHitTarget(AMP=x100) -> SetHitActionTimeline(AMP=x100(victim))
--       -> ProjectileDestroy -> StopTimelineByTime
--     两条通道的差别【在包的参数里】:
--       近战/扫击 RPC_CS_DoActOnActorActions(NodeID, dynamicInfo) / DoActOnSweepTargetsActions
--         -> 包里没有任何实例身份, 服务器按每条跑一遍 action 节点 -> 重发 N 次真算 N 次 ✅有连击
--       投射物 RPC_CS_ProjectileHitTarget(★self.instanceId★, ...) + 紧跟 ProjectileDestroy(同 id)
--         -> 同一颗弹重发 100 次只认第一次 ❌没连击
--     -> 正解已实现(①-b): 把我方投射物改成【真的穿透 + 可重复命中同一目标】,
--        它就真的打 64 下, 每下都是合法的独立上报(改行为不改上报)。
--        三个闸门 destroyOnHit / maxHitNum / checkCanHitTarget 全是游戏自己的字段。
--   ★★【2026-09-29 20:34 实机·①-b 功能成了, 但归因没隔离开】
--     现象: 同一个技能 abilityId=13710000, 19:56 那轮一次施法只有【1 条】
--     RPC_CS_ProjectileHitTarget; 打上 projHit=64 之后同一次施法出了【2 条】
--     (burst#4 / burst#6)。★用户反馈: 换了一个宠物之后【技能有连击了】。
--   ☠但【两条变三条不能单独当成 projHit 生效的证据】—— 原版就有三条路
--     能在一次施法里出多条 ProjectileHitTarget:
--     ①分段弹: 模板带 nextTemplateId 时 destroyImmediately=false
--       (@Common_Ability_Projectile_Projectile.lua:736-740) -> onResetToProjectile 把
--       hitTargetSet={}(:1185) / curHitIdx=0(:1190) / isDestroyed=false(:1181) 全清,
--       再 :1233 重新 loadDataFromTemplate 继续飞 -> 同一个 instanceId 可以再打同一个目标。
--     ②弹射: nextReboundActorId 为真时 :1011 的守卫【跳过】curHitIdx++ 和 hitTargetSet 写入,
--       但 :1021-1022 的 RPC 照发 -> 一次施法能出 maxReboundCnt+1 条同 instanceId 上报。
--     ③多发弹: createSegmentedCircleProjectile / createSegmentedRandomProjectile 按
--       projectileNum 循环(@Common_Ability_CombatAction.lua:7106 / :7151), 每发自己一个 instanceId。
--   ★★★【下次要分清就看这四个特征】补丁造成的重复 =
--     同 instanceId + 同 hitActorId + 第 6 个参数(nextReboundActorId)=0 + 前面没有
--     RPC_CS_ProjectileReset(@GameApp_Ability_ClientProjectile.lua:606)。
--     要验就把 instanceId 和 arg6 打进 burst 日志(现在只打了 RPC 名字, 所以分不出来)。
--   ☠已知覆盖缺口: 【由 ClientCreation / ClientPuppet 拥有的我方弹】(召唤物/炮台打出的)
--     过不了 isMine / isMyActorId, 会被静默跳过不上多段命中。现在没做是因为
--     “让召唤物的弹也穿透”需要先确认召唤物归属(ownerId 链), 盲放开会把别人的
--     召唤物也放进来。
--   ☠游戏自己的一个漏口(不是我改出来的, 但和无敌有关):
--     @Common_Ability_Projectile_Projectile.lua:995 是 checkValidTarget 的【失败】分支,
--     而 :996-999 在 isReboundSelf 且 nextReboundActorId 为真时仍然发 onProjectileHitRPC ->
--     即一颗自弹射的怪物弹可以在我无敌时仍上报命中我。路径很窄(要 isReboundSelf),
--     用户已确认无敌生效, 所以暂不动; 若以后出现“偶尔掉血”先查这里。
--   ★★★【普攻连击: 已判死并删除, 按用户决定 2026-09-29】
--     普攻走的就是 RPC_CS_SetHitActionTimeline, 放大它【实测无效】:
--     19:41 / 19:50 / 19:56 / 20:34 每一轮日志里都有
--     `dmg resend RPC_CS_SetHitActionTimeline x100 (victim=...)` —— 包确实发了 100 遍,
--     但游戏内普攻伤害没变。原因看包就明白: 这条包里【连攻击者都没有】,
--     只有 finalHitTimelineId / attackForceType / combatContextId(而且 combatContextId 是
--     targetEntity:genCombatContextId() 现场生成的), 语义是"给受击方设一条受击 timeline",
--     不是一条伤害结算上报 -> 服务器不会按条数算伤害。
--     ==> 放大那半已删。☠但 VICTIM_REPORT 这张表【必须保留】, 因为它还兼着无敌的一个
--         独立漏口(calcResult 那条路不经过 Utils.checkValidTarget), 详见该表上方的说明。
--   ☠RPC_CS_AddProjectile 不是杠杆: projectileParams.instanceId 是固定的
--     (@Common_Ability_Projectile_ProjectileManager.lua:125 srcEntity.createdProjectileList[instanceId]),
--     重发会被按同一个投射物去重。
--   ★★【上一轮「技能失效」的根因也由这份日志解开】: 扫描版用 rawget(vt,"serverMsg") 取方法,
--     拿不到【继承来的】那一份 -> ClientPet 被跳过 -> 而宠物才是主输出 -> 技能百倍失效。
--     现在一律 rawget(...) or cls.serverMsg; 日志已确认 full-wrap: ClientMainPlayer,ClientPet
--     且 AMP=x100 from=ClientPet。
--
--   ☠☠☠【2026-09-29 22:24 实机·⑦瞬移只在近处生效的根因】
--     现象: 日志里有 [jump] air jump 但【一条 [tp] 和 [chest] 都没有】;
--     用户实测"距离很近的话 跳过去 就开箱子了"。
--     根因: 上一版搜的是【交互 unit】(pg.game.interaction.unitMap), 而交互 unit
--     只有实体【靠近时】才创建 -> 远处的箱子没有 unit -> 永远找不到目标。
--     修法: 改搜【宝箱实体】本身 —— pg.me:GetRangeEntsChest()(游戏自己的追踪器, 110 米)
--     + pawn:entitiesInRange(range, SEARCH_USR_TYPE_INTERACTOR, 0)。
--   ☠☠这一轮真正的教训是【我自己的日志不会喊】: tpToChest / armAutoInteract 里有十来处
--     静默 return, 一条日志都不打 -> 实机日志"什么都没有", 只能靠猜。
--     现在每一处早退都带 [tpwhy]/[chestwhy] 诊断行, 并把各来源的数量和淘汰原因计数打出来。
--     通用铁律: 排障日志必须能回答"走到哪一步停了", 而不只是"成功时打一行"。
--
--   ★★★【2026-09-29 22:38 实机·⑦瞬移已确认服务器接受】日志原文:
--     [tp] jump->chest tmpl=11011 dist=6.5 target=(-479.4,164.7,1127.0) forbidOk=true
--     [tpcheck] kept tmpl=11011 offset=1.0  <== ★没被拉回去, LEVEL_ITEM_INTERACT 这个
--                                               报备理由服务器【认了】
--     [chest] arm 1994741475 tmpl=11011 chest=true needItem=none  <== 落地后自动开箱也成了
--     => 位置校验这一关过了, "先报备再写位置"这条路成立。
--   ☠☠☠【同一轮暴露的无敌漏口: 陷阱】用户原话"怪物打我不掉血了 但是有些陷阱会掉血"。
--     元凶在日志里: [rpc:RPC_CS_OnThornsCollision] from=ClientMainPlayer mine=true wrap=full AMP=no
--     陷阱伤害是【第三条完全独立的自伤通道】, 由受害者自己发, 不经过 checkValidTarget:
--       RPC_CS_OnThornsCollision(@...ClientAbilityComponent.lua:5031) 荆棘/尖刺
--       RPC_CS_OnConductHit(同文件 :5138) 导电陷阱
--     两条都已加进 VICTIM_REPORT。
--     ☠但 RPC_CS_OnPuppetThornsCollision(同文件 :5028)【绝对不能按名字丢】: 它发送者是
--       pg.me 而受害者是第一个参数(怪的 actorId), 按名丢等于让怪免疫荆棘。已放进 HURT_IN
--       按被打者过滤, 当可执行的文档用。
--     ★通用教训: "无敌"不是一个咽喉能包住的, 要按【伤害来源分类】逐条清 ——
--       怪打我 / 我自残(摔落溺水) / 环境陷阱 / 受击 timeline 上报, 四类各走各的路。
--
--   ★【2026-09-29 用户要求两改】
--     ①落点改成宝箱【正上方】(jumpToChestUp=2.5, jumpToChestStand=0):
--       落在旁边遇到斜坡/台阶/障碍会站不住, 正上方掉下来一定落在箱子跟前。
--       ☠落点复查随之改成【只比水平距离】—— 人会从 2.5 米掉下来,
--       垂直差恒有两三米, 拿总距离当判据会把正常下落误报成 SNAPPED-BACK。
--     ②强制开箱(forceOpenChest): 绕过 canInteractive 那一整套客户端门直接调 startInteract。
--       ★真正的价值在回调里的【服务器返回码】: 1026 距离不够 / 10022 次数上限 /
--       10024 前置条件不满足 / 200143 归属者太远 ... "打不开"从此不再是黑盒。
--       ☠客户端能绕的只有客户端自己那几道门; 服务器在意的它会自己再查一遍。
--       ☠消耗型宝箱走不到强开: chestEntOk 在【上游】按 ent.needItem 就挡掉了 ——
--       这很重要, 因为 startInteract 本身是绕过成本闸门的。
--
--   ★★★【2026-09-29 找到官方流程了】全库【唯一一条把玩家搬到任意点】的代码:
--     @GameApp_DialogueGraph_DialogueGraphRuntime_Node_DialogueGraphEntitySetPosAndRotNode.lua:24-39
--     四步: ①距离检查 <= 25 米(玩家) ②ENTITY_FORBID_POSITION_CHECK
--            ③forceChangeToIdle ④setEntityPosition
--   ☠☠☠我原来【漏了第③步】。forceChangeToIdle
--     (@GameApp_DialogueGraph_DialogueGraphUtils.lua:294-302)的条件是
--     "状态属于 LOCOMOTION / 【AIRING】 / LAND 就 ForceChangeToState(IDLE)"。
--     而我们写位置的时机是 PawnController:performJump 【里面】,
--     那一刻角色正在起跳/空中 = AIRING —— 不先踢回 IDLE, 角色控制器会继续按
--     自己的运动状态解算, 把写进去的位置顶掉。和"瞬移过去又弹回原地"完全吻合。
--   ★玩家搬运上限 25 米(ENTITY_PLAYER_SET_POSITION_DISTANCE_MAX
--     @Const_DialogueGraphConst.lua:5; 怪是 40)。官方节点超了直接报"移动距离过大"。
--     ☠失败那次 dist=24.1 + 抬高 2.5, 3D 距离正好越界。
--   ☠forceSetPosRot 的第三个参数 instant 【只喂给 setRotation】
--     (@...ClientAoiComponent.lua:527-535), 对位置是死参数 —— 别指望它让位置"立即生效"。
--   ☠我曾怀疑 isTeleport(TeleportFromLua=7 vs LogicFromLua=8)是元凶 —— 【不成立】:
--     官方节点自己也没传 reason, 而 22:38 那版同样没传却是成的。
--
--   ☠☠☠【2026-09-29 23:47 实机: forceChangeToIdle 也【不是】元凶, 已整批回退】
--     那一轮的配置 = 已验证的落点(stand=1.5/up=0) + forceChangeToIdle + 25m上限 + 强开,
--     结果用户反馈“不行 位置不通过”。落点都换回去了还不通过 ->
--     元凶在剩下的差异里, 而那里最可疑的是【强开 startInteract】:
--     它绕掉的客户端门里包含 checkAirBlock(下落中拒绕交互,
--     @GameApp_Interaction_InteractionUnitBase.lua:587), 而强开在瞬移后 0.6 秒就发包。
--     ★★★通用教训: 客户端那些门不全是"限制你"的, 有些是【帮你等到
--     服务器会同意的那一刻】—— 绕掉它等于把自己提前推到服务器会拒的时间点。
--     ★★另一条教训: 上一轮我【同时】动了落点 + 强开两个变量,
--     所以 23:09 那次分不清是哪个坏的, 白掉两轮实机。以后一次只动一个。
--     当前已回退到 22:38 那一版的行为: forceOpenChest=false /
--     jumpToChestIdleFirst=false / jumpToChestMaxDist=0 / jumpToChestRange=60 /
--     stand=1.5 / up=0 / float=false。代码全留, 要排障就【一次只开一个】。
--
--   ☠☠☠【2026-09-30 01:24 实机: ⑧"吸箱子/远程开箱"已按用户决定【整块删除】】
--     这一轮探针终于跑起来了(上一轮是最外层入口没日志, 分不清按键有没有进来), 拿到两条硬信息:
--       ①[probe8] 启动: 6 个目标 距离=16.6/21.3/33.9/36.6/43.8/99.0m
--         (追踪器=9 AOI=7 候选=16 合格=16 淘汰[]) —— ★搜索链路本身完全正常, 远处箱子找得到。
--       ②[probe8] #1 回包 dist=16.6m ret=1002 —— 站在地上(站地上=true)、16.6 米, 服务器照样拒。
--     ☠但这个读数【被污染了, 不能当定论】: 用户连按了两次跳跃(相隔 0.7 秒), 第一次启动探针,
--       第二次因为轮数已用完落给了⑦瞬移 —— 而探针的第一发是在那次瞬移之后【0.17 秒】发出的。
--       所以"1002 是冲距离来的, 还是冲刚才那次瞬移来的"仍然分不清。
--       ★这正是探针设计上要避开的混淆, 结果被第二次按键破坏了 —— 教训: 实验型功能
--       必须能【独占】它的触发条件, 或者干脆别和别的功能共用一个键。
--     ==> 用户决定: 远程开箱不做了, ⑧整块删除, 保留⑦瞬移。
--
--   ★★★这一轮最有价值的是个【意外收获】, 而且关系到要保留的⑦:
--     [tpcheck] ☠弹回原地 SNAPPED-BACK tmpl=14001 打算走15.1m 实际走1.6m 还差13.6m 下落-0.1m
--     -> ⑦瞬移在 16.6 米这个距离上【被服务器弹回了】; 而 22:38 那次 dist=6.5 米是成功的
--        ([tpcheck] kept offset=1.0)。两次的落点几何【完全一样】(stand=1.5/up=0),
--        唯一的差别就是距离。
--     ==> 瞬移存在一个【真实的距离上限】, 落在 6.5 和 16.6 米之间。这正好解释了用户
--        一直以来的体感: "距离很近的话 跳过去 就开箱子了"。
--     ☠这条和我 23:47 写下的"距离不是判据"【直接冲突】。那条结论是我拿 23:09 的
--       dist=8.1 也被弹回推出来的 —— 但 23:09 那一轮落点在【箱子头顶】(非法站位),
--       距离和落点两个原因混在一起, 我当时把它归给了落点。
--       现在落点已经是合法的那一组, 16.6 米仍然被弹回 => 距离重新成为【首要嫌疑】。
--     ★想收窄就打开 jumpToChestMaxDist 试几个值(10 / 15 / 20), 每次只改一个,
--       看 [tpcheck] 是 kept 还是 SNAPPED-BACK。★这也顺带解决一个真实的副作用:
--       maxDist=0 时远处的箱子会被选中 -> 瞬移失败 -> 还白吃掉这一次跳跃;
--       打开上限的话超限直接落回普通跳跃, 严格更好。
--

-- ★本作的架构(这条决定了四个功能各自怎么做, 全部有文件:行号支撑):
--   伤害【数值】100% 服务器算  —— 客户端包里连 processInstantDmgV 都不存在,
--     attrProcessFun[instant_dmg_v=402] 恒 nil, modifyAttrib 直接 return false,0
--     (@Common_Ability_Attribute_ActorAttributeModifier.lua:37-115/146-151,
--      @GameApp_Ability_ClientActorAttributeModifier.lua:10-20 白名单只有 tp/stamina/water);
--     公式 3001/3067 各 33 个形参, 客户端三条调用路径实参上限 5/9/8, 结构上调不到。
--     => 放大任何数值都是死码。
--   命中【判定】100% 客户端算并上报  —— 游戏自己的"翻滚无敌帧"就是这么实现的
--     (lastStartDashTime 只在客户端写 @...ClientAbilityComponent.lua:4814,
--      判定在共享的 Utils.checkValidTarget @Common_Utils_Utils.lua:772)。
--     => 多报一次 = 服务器多算一次伤害(百倍伤害); 不报 = 服务器不扣血(无敌)。
--     两者都不伪造任何数值, 符合"改行为不改上报"。
--
-- 为什么宿主选 Core.Common.Time:
--   1.7KB; 被 373 个文件 require, 启动必加载; 全库只有一种 require 写法, 没有重名副本
--   (Common.Utils.Utils / Common.Const.Const 那批有两个 package.loaded 键, 补一份漏一份);
--   @Main.lua:40 每帧调 Time.getTickSecond(), 白得一个启动探测器; 零 Lua 依赖不会循环依赖。
--   ★装完立刻自卸, 不在 Time.realSecondCache 的 360 个读点热路径上留常驻包装。
--
-- 本作专属的五个坑(都实测确认):
--   ☠1 _G 装了严格 __newindex(@Core_Framework_Global.lua:48): 写未声明全局会被【静默吞掉】
--        -> 导出开关表必须 rawset
--   ☠2 用 package.loaded[...] 取模块, 不要 require: 本文件加载极早, require 会把目标模块
--        提前拉起来造成半成品/循环依赖。游戏自己也是这么取的
--   ☠3 Class.Class 实体类的 __newindex 遇到重名 key 会 error("has repeat attr")
--        (@Core_Framework_Class.lua:402-420) -> 必须走 cls.getVtbl()(:431)
--        或先 cls.forceAttrRepeat(cls, key)(:496); 且 rawset 到类本体实例看不见
--        (实例元表是 classType.__objMetatable.__index = vtbl, :303-305)
--   ☠4 Class.LiteClass 的继承是【读一次就 rawset 缓存进子类表】(:739-745)
--        -> 只改 PawnController, 已缓存的 PlayerController 会屏蔽掉补丁, 两个都要写
--   ☠5 Component 的方法在 AddComponents 时【按值拷贝】进实体类 vtbl(:904-906)
--        -> 改 Component 模块表对已建好的实体类零效果, 必须改实体类

local MOD = {
	dmgMul = 100,
	invin = true,
	infJump = true,
	speedMul = 3,
	infStamina = true,
	jumpRiseDist = 3.2,
	jumpRiseTime = 0.26,
	jumpUnGround = 0.55,
	-- ★★★①-b 投射物技能的连击: 让【我方】投射物真的穿透 + 可重复命中同一目标,
	--   于是它真的打 projHit 下, 每一下都是一条合法的、curTime/curHitIdx 各不相同的
	--   RPC_CS_ProjectileHitTarget。服务器看到的是"一颗真的打了 N 下的穿透弹", 零伪造。
	--   上限 64 = 引擎自己的 ProjectileConst.MAX_PROJECTILE_HIT_COUNT_LIMIT
	--   (@Common_Const_ProjectileConst.lua:4), 写大了也会被 math.min 砍回 64。
	--   1 或 0 = 关掉这条, 回到原版单次命中。
	projHit = 64,
	-- 是否继续把 RPC_CS_ProjectileHitTarget 重发 dmgMul 次。
	-- ☠这条【大概率无效】(推断, 未实测): 包的第一个参数是投射物实例 id
	--   (@GameApp_Ability_ClientProjectile.lua:546 srcEntity:serverMsgNoGC(..., self.instanceId, ...)),
	--   而客户端命中后紧跟着还会发 RPC_CS_ProjectileDestroy(同一个 instanceId, 同文件 :594),
	--   服务器那颗弹已经消耗/销毁 -> 重发的 99 次会被按同一颗弹去重。
	-- 先【并存不删】: 改已经在跑的链路要并存不要替换, 否则出问题没法二分(这是上一轮
	--   "技能失效"的教训)。等实机确认 projHit 那条能出连击之后, 把这个改成 false,
	--   就能省掉每次命中 99 个白发的包 —— 那是目前最大的延迟/风控开销。
	ampProjectileHit = true,
	-- ★★安全阀: 每秒最多重发这么多个包。
	-- ☠为什么需要: projHit=64 和 dmgMul=100 是【相乘】的 —— 一颗弹真打 64 下, 每下再重发 100 次,
	--   一次施法就是 64x100 = 6400 个 ProjectileHitTarget 再加 6400 个 SetHitActionTimeline,
	--   一瞬间 1.2 万个包。那不是"更强", 那是把自己的连接打死(卡顿/掉线), 而且看起来会像
	--   "MOD 把游戏搞坏了"。
	-- 2026-09-29 20:34 实机那轮只命中了 2 下 = 2x99 = 198 个重发, 【远低于 500】,
	--   所以这个上限对已经实测过的那一轮【行为完全不变】, 只掐掉 64 段那种病态情况。
	-- 预算在【每次我方施法】和【每秒 tick】各清零一次 -> 实际语义 = "每秒最多 N 个重发包"。
	-- 0 或负数 = 不限制(不建议)。
	maxResendPerCast = 500,
	-- ★⑥自动开宝箱 / 自动拾取。做法不是自己枚举+判距离, 而是借游戏【自己的】自动交互系统
	--   (unit.interactData.actWhenArrive = 1), 详见下面 armAutoInteract 的大段说明。
	autoChest = true,
	-- 是否也自动开【要花体力/道具】的宝箱。☠默认 false: unit.needItem 就是消耗,
	--   needItem[1] == 1009 是体力(Const.CommonEnergyType_Stamina @Common_Const_Const.lua:3270),
	--   打开等于让它把你的体力静默抽干。想开自己改 true。
	autoChestCost = false,
	-- 是否也自动触发 EnvObject(门/机关/采集点)。☠默认 false: 那既不是宝箱也不是道具,
	--   自动去拉机关会开出你没打算开的门。
	autoChestEnvObj = false,
	-- ★⑦跳跃瞬移: 按一下跳跃 -> 瞬移到最近的宝箱/掉落物身边并自动开启。
	--   ☠没有目标时【完全按原来的逻辑走】(正常跳跃 + 无限跳), 不会把跳跃键占掉。
	--   用的是游戏自己的合法传送流程(先 forbidPositionCheck 报备再写位置), 详见 tpToChest 说明。
	jumpToChest = true,
	-- 搜索半径(米)。实际生效的是 min(jumpToChestRange, jumpToChestMaxDist),
	--   而 jumpToChestMaxDist 现在默认 0(关) -> 这里的 60 就是实际半径。
	-- ☠☠☠【2026-09-29 23:47 回退】2 5 是我上一轮自己加的收窄, 22:38 那版能用的是 60。
	--   用户要求"还是之前那一版", 所以恢复 60。
	jumpToChestRange = 60,
	-- ☠☠☠【2026-09-29 23:09 实机: 落点改到箱子正上方是错的, 已回退】
	--   22:38 那版(stand=1.5, up=0 = 站在箱子旁边同高度)日志是 [tpcheck] kept offset=1.0,
	--   瞬移【本来就是成的】。改成正上方(stand=0, up=2.5)之后:
	--     [force] ☠ ret=1002 ERROR_POSITION_VALID(@Common_NoticeDef.lua:173) = 位置不通过
	--     [tpcheck] 水平偏移 6.2 / 22.5 ≈ 原距 8.1 / 24.1 -> 人被【弹回原地】
	--   用户原话: "瞬移过去 瞬间返回原地 说位置不行" —— 箱子头顶不是合法站位。
	--   ==> 这两个值现在是【已验证能用的那一组】, 别再动。
	-- 落点抬高(米)。0 = 和宝箱同高度(已验证)。只在明显陷进地面时才微调。
	jumpToChestUp = 0,
	-- ★侧向偏移(米): 落在宝箱【朝我这一侧】前面多远 —— 就是"瞬移过去之后离箱子多远"。
	--
	-- ★★这个算法和游戏自己的【逐字同构】, 不是我自己编的:
	--   @GameApp_Interaction_InteractionUnitBase.lua:297-345 InteractionUnitBase:getInteractPos()
	--     local forceDist = self.interactData.interactiveForceDist
	--     local xzPosOffset = playerPos - entPos;  xzPosOffset.y = 0
	--     local dir = Vector3.Normalize(xzPosOffset)
	--     targetPos = entPos + dir * forceDist        <== 就是下面那一行的形状
	--   所以 jumpToChestStand 对应的就是游戏配表里的 interactData.interactiveForceDist,
	--   而 jumpToChestUp = 0 对应游戏把 entPos.y 原样留着(dir 的 y 被清零)。
	--   ☠interact_data 表不在这份反编译产物里, 所以配表值只能运行时读 —— 已经打进 [tp] 日志
	--   当对照(标 "游戏配表=x.x"), 想跟游戏完全一致就把 jumpToChestStand 改成那个数。
	--
	-- ☠☠两条【已实机验证】的边界, 改这个值必须落在中间:
	--   1.5 ✅ 22:38 实机成功, [tpcheck] kept offset=1.0
	--   0   ✗  23:09 实机失败, 服务器回 1002 ERROR_POSITION_VALID 把人弹回原地
	--          —— 因为 stand=0 时落点就是【箱子正中】, 那是碰撞体内部, 不是合法站位。
	-- ★2026-09-30 按用户要求("瞬移到身边 不要间距那么远")从 1.5 改成 0.8。
	--   ☠0.8 【未经实机】: 它落在已验证的 1.5 和已证伪的 0 之间, 但离 0 还有多远才安全
	--   取决于箱子碰撞体半径 —— 那个在 C# 侧, Lua 读不到, 只能实机试。
	--   判据很干净: 看 [tpcheck] 那一行。kept = 成了; SNAPPED-BACK = 太近了, 往上调
	--   (0.8 -> 1.0 -> 1.2, 每次只改一个数)。
	jumpToChestStand = 0.8,
	-- ★硬下限(米): 再怎么调也不许比这个小。
	-- ☠依据: stand=0 已实机证伪(箱子正中 = 碰撞体内部 -> 1002 弹回原地)。这个 0.3 是
	--   经验余量【不是实测值】—— 作用是防止手滑填 0 或负数, 把那次已知的失败直接重演一遍。
	jumpToChestStandMin = 0.3,
	-- ★浮空: 瞬移后把重力关掉、钉在落点不下落, 过 jumpToChestFloatSec 秒再恢复。
	-- ☠【默认关】: 已验证能用的落点(旁边同高度)不需要浮空, 加了反而是在动一条正常的链路。
	--   只在遇到"落点站不住/滑下去"的箱子时才打开。
	-- ★打开还有一个附带好处: checkAirBlock(@GameApp_Interaction_InteractionUnitBase.lua:587)
	--   只在 airBlock==1 且 pawn:FALL_ST() 时拒绝交互 —— 也就是说【下落中会被拒】。
	--   浮空不是下落, 所以正常开箱那条路也能走通。
	-- ☠实现是 pawn:forceUngrounded(true/false, reason)
	--   (@...ClientAbilityComponent.lua:4208): 它同时 SetComputeGravity(GravityMask.SKill,false)
	--   和 AlwaysForceUnGround —— 是个【锁存标志, 必须配对清除】, 不清会永久不着地。
	--   所以代码里带了硬性倒计时兜底清除。
	jumpToChestFloat = false,
	jumpToChestFloatSec = 1.5,
	-- ★★★写位置之前先把角色控制器踢回 IDLE。这是【官方流程里的一步, 我原来漏了】。
	-- 依据: @GameApp_DialogueGraph_DialogueGraphRuntime_Node_DialogueGraphEntitySetPosAndRotNode.lua:24-39
	--   是全库【唯一一条把玩家搬到任意点】的代码, 顺序是:
	--     ①距离检查 ②ENTITY_FORBID_POSITION_CHECK ③forceChangeToIdle ④setEntityPosition
	--   而 forceChangeToIdle(@GameApp_DialogueGraph_DialogueGraphUtils.lua:294-302)的条件是
	--   "状态属于 LOCOMOTION / 【AIRING】 / LAND 就 ForceChangeToState(IDLE)"。
	-- ☠☠☠【2026-09-29 23:47 实机: 这条没能解决问题, 已关】
	--   那一轮落点已经回到 22:38 验证过的那一组(stand=1.5/up=0), 并且加上了这一步,
	--   服务器【仍然】回"位置不通过"。而 22:38 那版没有这一步却是成的 ->
	--   所以它不是元凶, 也没帮上忙。按用户要求回到那一版, 默认关。
	-- ★代码留着: 它确实是官方节点的第③步(上面那段依据仍然成立),
	--   以后若出现"瞬移过去被推回"类的新症状, 可以【单独】打开它再试。
	-- ☠一次只开一个开关 —— 上一轮同时动了落点和强开, 结果没法分清是哪一个在搞鬼。
	jumpToChestIdleFirst = false,
	-- ★玩家搬运距离上限(米)。25 = 游戏自己的值
	--   (DialogueGraphConst.ENTITY_PLAYER_SET_POSITION_DISTANCE_MAX @Const_DialogueGraphConst.lua:5;
	--    怪是 40)。官方节点超了就【直接拒绝并报"移动距离过大"】。
	-- ☠☠【2026-09-29 23:47 回退: 默认关(0)】理由两条:
	--   ① 22:38 能用的那一版根本没有这道检查, 用户要求回到那一版。
	--   ②距离并不是判据: 23:09 那一轮里 dist=8.1 米那次【也】被弹回了,
	--     远远低于 25 米 —— 所以把它当成失败原因是我自己推的, 没有证据。
	--   ☠而它的副作用是真的: 开着的时候 25 米外的箱子直接不去。
	-- ★数值依据仍然成立(游戏自己的 ENTITY_PLAYER_SET_POSITION_DISTANCE_MAX = 25,
	--   怪是 40, @Const_DialogueGraphConst.lua:5), 只是那是官方对话节点自己的门,
	--   未经实机证明服务器也抽同一道。想打开就写 25。
	-- 0 或负数 = 不做距离检查(= 22:38 那版的行为)。
	jumpToChestMaxDist = 0,
	-- ★★强制开箱: 瞬移到位之后【直接调 startInteract】, 绕过 canInteractive 那一整套
	--   客户端门(交互角度 / 前向角度 / 空中阻挡 / 交互阻挡 / conditionId / 宠物形态 /
	--   checkCanInteract 里的 levelCondition 和 attaching)。
	-- ★这条的价值不只是"可能开得了": 回调里能拿到【服务器的返回码】, 一眼看出是哪道门拒的
	--   (1026 距离不够 / 10022 次数上限 / 10024 前置条件 / 200143 归属者太远 ...)。
	--   服务器会自己再查一遍它在意的那些 —— 客户端能绕的只有客户端自己那几道。
	-- ☠消耗型宝箱在【上游】就被 chestEntOk 挡掉了, 不会走到这里, 所以强开不会抽体力。
	--
	-- ☠☠☠【2026-09-29 23:47 已关。这是"位置不通过"的最大嫌疑】
	--   22:38(没有强开, 只走游戏自己的 actWhenArrive) = 开箱成功;
	--   23:09 / 23:47(加了强开) = 服务器回 1002 位置不通过。
	--   23:47 这一轮落点已经换回 22:38 验证过的那组了, 还是不通过
	--   -> 剩下的差异就是强开本身。
	-- ★结构上也说得通: 强开绕掉的客户端门里【包含 checkAirBlock】
	--   (@GameApp_Interaction_InteractionUnitBase.lua:587 —— airBlock==1 且 pawn:FALL_ST()
	--    时拒绕交互, 即【下落中不允许交互】)。而强开在瞬移后第 2 个 tick
	--   (约 0.6 秒)就发包, 那时人还在下落/落地稳定中 -> 服务器判位置不合法。
	--   游戏自己的流程会一直等到 canInteractive() 全部通过才发 ——
	--   这就是它能成而强开不能的原因。★“绕客户端的门”在这里是有害的,
	--   因为那扇门不是在限制你, 是在帮你等到服务器会同意的时刻。
	-- ★代码留着当【排障工具】: 回调里能拿到服务器返回码, "打不开"不再是黑盒。
	--   要用就配着 jumpToChestFloat = true 一起开(浮空不是下落, 能过 checkAirBlock)。
	forceOpenChest = false,
	-- 强制开箱最多试几个 0.3 秒的 tick(第 2 个 tick 才发第一次, 留时间给 C# 触发体积注册)
	forceOpenTries = 6,
	-- 只在单人 space 生效(多人同屏最大的风险是被人举报, 本作没有自动反作弊但有完整的
	-- 举报->客服禁言/封号链路: RPC_CS_ReportChat @...ClientChatComponent.lua:641 +
	-- GM 的"违规分类"/"禁言或封号时长小时" @Utils_GmAnnotationI18nMap.lua:435/581)。
	-- 默认 false 是为了本地测试时不被"怎么没生效"迷惑; 上号玩建议改 true。
	soloOnly = false,
	log = true,
	-- 只读探针: 把 C# 实际向 Lua 要哪些速度项打进日志(每个名字只打前 5 条)。
	-- ★用途: 判断 speedMul 会不会被【乘两次】。如果日志里既出现 getMotionAttr:getRunSpeed
	--   又出现 getMotionAttr:getRunSpeedRatio(或 getAttributeSpeedRatio:speed_ratio_run_v),
	--   说明 C# 把"基础速度"和"倍率"都拉了, 而两者内部都过 getAttribRatioValue ->
	--   实际会变成 speedMul^2(x9)。这时把 speedMul 改成 math.sqrt(3) 即可。
	-- ☠2026-09-30 我曾经想在"开关版"里把这三个排障开关默认关掉, 已【还原】, 两个理由:
	--   ①它们每个 tag 都有条数上限, 本来就不会刷屏 —— "怕刷屏"这个理由站不住。
	--   ②rpcTrace 是【安装期门控】: 关掉它 serverSpaceMsg / reliableServerSpaceMsg 的追踪包装
	--     就根本不会装上, 之后再运行时打开也没用 —— 那等于悄悄削掉一块排障能力。
	--   ★而且用户没要求改这个。别顺手动用户没让动的东西。
	probe = true,
	-- ★★排障用(2026-09-29 实机: 技能已放大生效, 普攻没生效 -> 说明服务器【没有】按
	--   combatContext.id 去重, 多发包路子是对的, 只是普攻走的上报通道不在 DMG_OUT 白名单里)。
	-- 打开后: 从我/我宠身上发出的每一条 RPC_CS_* 都打一条日志(每个名字各前 5 次), 标注
	--   AMP=x100(已放大) / AMP=no(没放大); 没放大又像命中类的会额外打 <== SUSPECT。
	-- 用法: ①站桩用【普攻】打怪一次 -> 记下所有 SUSPECT 的名字
	--       ②再用【技能】打一次 -> 对照, 技能有而普攻没有的、或普攻独有的那条就是答案
	--       ③把那条名字加进 DMG_OUT 即可(先确认它不是"我被打到/自伤"类再加)
	-- ☠已确认【不能】直接加进 DMG_OUT 的两条:
	--   RPC_CS_OnConductHit(@...ClientAbilityComponent.lua:5138) 是【我自己被撞】播受击 timeline
	--   RPC_CS_OnTriggerAbilityCollisionHit 两个发送点里 :1952 那个用的是 hitTriggerSelfActionIds
	--     (= 撞到东西触发【自身】效果), 放大会变成自伤 x100; :2046 那个才是打中人后的附带效果
	rpcTrace = true,
	-- ☠2026-09-29 实机: 上一版把 serverMsg 从"只包主角/宠物"改成"扫描全部实体类都包完整 wrapper"
	--   之后, 原本已经生效的【技能百倍伤害失效了】, 其它功能正常。静态没能定位到是哪一环,
	--   所以默认关掉: 主角/宠物走完整 wrapper(= 已验证能用的那版), 其余实体类只上【只读追踪】。
	-- 打开 = 回到全量包装(其余实体类也参与放大/丢弃)。只在确认需要、且愿意再试一轮时才开。
	wrapAllEntities = false,
	-- ★★排障用: 按【技能】分段抓 RPC 序列。看到 RPC_CS_CastAbilityOnTarget 就开窗口,
	--   接下来这么多条【战斗相关】的 RPC 逐条打印(绕过按名字的 5 条配额), 带 abilityId 标签,
	--   结束时再打一条小结: 这次施法到底有没有走到被放大的上报。
	-- ☠2026-09-29 19:50 实机教训: 一开始不过滤, 25 个名额被 Heartbeat / SyncMapBlockId /
	--   RequestCreateClientEntity / 别的怪的 CharacterStateChange 占满, 真正的命中上报一条没抓到。
	--   现在只让名字含 Hit/Act/Sweep/Projectile/Ability/Timeline/Knock/Damage/Cast/Tp/Buff/Attack 的占名额。
	-- 用途: 现在的现象是「只有个别技能有连击」。各放一次【有连击】和【没连击】的技能,
	--   看两条 "=== CAST <id> 结束: 被放大的上报 = ... ===" 小结即可直接对照。
	-- 0 = 关闭分段抓取。
	traceBurst = 25
}

rawset(_G, "IMO_MOD", MOD)

---------------------------------------------------------------------
-- ★★★★★【开关 AN_1 ~ AN_6】由 native 侧 hook lua_pushboolean + lua_setglobal 写进来
--
--   AN_1 百倍伤害      AN_2 玩家无敌      AN_3 移速 x3
--   AN_4 无限跳跃      AN_5 无限体力      AN_6 跳跃瞬移(+自动开箱, 见下)
--
-- ☠☠☠☠☠【必须先声明这 6 个名字, 否则整套开关一个都不会通】
--   这是本作最硬的一个坑。严格 _G 的真身(@Core_Framework_Global.lua:105-122):
--     setmetatable(_G, {
--       __index    = function(t,k) pcall(报错) -> 只记日志; return nil end,
--       __newindex = function(t,k,v) pcall(报错) -> 只记日志; 值丢掉 end,
--     })
--   ★已用游戏那张元表【原样复刻 + luajit 实跑】验证过三条:
--     ①不声明就写(= lua_setglobal 的等价路径): 值【被吞掉】, rawget 读回 nil,
--       而且每次刷一条 "Attempt to write an undeclared global variable"
--       ==> 开关永远打不开, 而且看起来像"MOD 没装上"
--     ②不声明就读(= if AN_1 then): 恒为 nil, 而且【每读一次刷一条】
--       "Attempt to access an undeclared global variable"
--       ==> 放在每帧钩子里就是刷屏, 还会被 @Common_LoggerHelper.lua 按 ERROR 往内网传
--     ③先 rawset 成 false 占位再写: 写入直接落地, 反复开关全程【零报错】
--   ★为什么占位就够: Lua 的 __newindex 只在【key 不存在】时触发, 而 false 是非 nil ——
--     占位之后原生那次写就是纯 rawset, 根本不过元表; 读同理不过 __index。
--     这也正是"if AN_1 then 裸写"能安全的前提。
--
-- ★优先用游戏自己的官方 API GLDeclare(name, default)(@Core_Framework_Global.lua:54),
--   它会把名字登记进 _GlobalNames, 比裸 rawset 更正统; 拿不到就退回 rawset。
-- ☠已有值不覆盖: 别把用户刚打开的开关按回 false。
-- ☠本文件加载极早(被 373 个文件 require), 所以声明一定在 native 写入之前发生 ——
--   哪怕 Global.lua 比我们更晚装元表也没事: 那时这 6 个 key 已经存在了。
---------------------------------------------------------------------
local SWITCH_NAMES = {
	"AN_1",
	"AN_2",
	"AN_3",
	"AN_4",
	"AN_5",
	"AN_6"
}

local SWITCH_DESC = {
	AN_1 = "百倍伤害",
	AN_2 = "玩家无敌",
	AN_3 = "移速x3",
	AN_4 = "无限跳跃",
	AN_5 = "无限体力",
	AN_6 = "跳跃瞬移+自动开箱"
}

local function declareSwitches()
	local how = "rawset"
	local GD = rawget(_G, "GLDeclare")
	local parts = {}

	for i = 1, #SWITCH_NAMES do
		local n = SWITCH_NAMES[i]

		if rawget(_G, n) == nil then
			if type(GD) == "function" and pcall(GD, n, false) and rawget(_G, n) ~= nil then
				how = "GLDeclare"
			else
				pcall(rawset, _G, n, false)
			end
		end

		parts[#parts + 1] = n .. "=" .. tostring(rawget(_G, n))
	end

	return table.concat(parts, " "), how
end

-- ★立刻声明(不能等 install, install 要等 pg 就绪, 那时 native 可能已经写过一次被吞掉了)
local _swInit, _swHow = declareSwitches()
-- 上一次看到的开关状态, 用来打【改前 -> 改后】
local _swLast = {}

local _logCnt = {}
local _logChan

-- ★每个 tag 的日志条数上限。默认 5, 这里点名放大的是【会被安装期日志挤掉的那些】。
-- ☠2026-09-29 20:34 实机教训: patchLite 成功时会打一条 "patch xxx = true", 用的是调用方
--   给的同一个 tag。投射物那边 3 个类 x 2 个方法 = 6 条安装行, 而 cap 只有 5 ->
--   把运行时那条关键的 "open ... maxHitNum=64" 行【整条吃掉了】, 导致这一轮实机
--   拿不到"闸门到底放开了没有"的直接证据。
--   -> 安装期和运行期【必须用不同的 tag】, 见下面 patchProjectileMultiHit 用 "projhit"(安装)
--      和 "projopen"(运行)。这条通用教训: 排障日志的配额要留给真正想看的那一行。
local LOG_CAP = {
	install = 20,
	dmg = 40,
	dmgcap = 6,
	projhit = 10,
	projopen = 30,
	castnote = 10,
	chest = 30,
	tp = 30,
	tpcheck = 30,
	-- ★"为什么没瞬移"的诊断行。上一轮实机翻车就是因为 tpToChest 里全是静默 return,
	--   日志里一条都没有 -> 只能靠猜。教训: 每一处早退都要能说出自己是哪一处。
	tpwhy = 25,
	-- 附近没有可交互实体时这条会每 0.3 秒刷一次, 3 条足够说明情况了
	chestwhy = 3,
	force = 40,
	float = 20,
	idle = 20,
	-- ★★【跳跃键到底有没有进来】—— 最外层入口的无条件日志。
	-- ☠2026-09-30 01:14 实机的教训: 用户反馈功能没反应, 而日志里【一条相关的都没有】,
	--   我当时说不出"到底是按键没进来, 还是进来了被某处早退了", 只能靠猜, 白掉一轮实机。
	--   通用铁律: 一条链路的【最外层入口】必须有无条件日志, 否则"什么都没发生"是个哑谜。
	jumpin = 8,
	-- ★★开关: 声明结果 + 每次变化 + nil 自愈。开关是用户手点的, 给足配额。
	sw = 60
}

local _burstLeft = 0
local _burstSeq = 0
-- ★_burstAbility 现在【只由下一次施法清空】(以前是逐条名额用完就清)。
--   原因: 一颗 64 段的弹会产生 128 条战斗 RPC, 25 个逐条名额在第 12 下就用完了,
--   而老代码在名额用完时顺手把 _burstAbility 置 nil -> noteAmp 从此不再计数 ->
--   小结报出来的"被放大的上报"笔数【严重少报】, 正好在最需要它的场景下失真。
--   现在: 逐条打印有名额, 计数【没有名额】, 一直数到下一次施法或 tick 收尾。
local _burstAbility = nil
local _burstAmp = {}
local _burstAmpTotal = 0
local _burstNoted = false
local _burstTicks = 0
-- 每秒重发预算的已用量
local _castResend = 0

-- ☠☠ 不能用 print: 本作【没有】重定向 print(全库只有 @Utils_ConsoleUtils.lua:88/95/104/112
--    在调试控制台里临时换过), 所以 print 走的是 Lua 标准 stdout, Android 上直接被丢掉,
--    在 logcat 里【一行都看不到】。
-- 游戏自己的 lua 日志出口是: logger:xxx() -> LoggerManager 的 hook
--    -> @Init.lua:71-86 loggerHook -> CS.FunPlus.WorldX.Utils.LuaUtils.LogForLua(level, msg, len)
--    (C# 侧, 带 "[lua][等级]" 前缀)。所以下面【直调 LogForLua】, 和游戏自己的日志同一个出口,
--    logcat 里 grep IMOMOD 或 grep "[lua]" 都能看到。直调还能绕过 Lua 侧的等级过滤。
-- 另外 phonestcore 就在 libxlua.so 里, 内含 spdlog 且 spdlog::sinks::android_sink<std::mutex>
--    的 RTTI 存在(= 真被实例化用上了), __android_log_write/__android_log_print 也在
--    -> native 侧日志同样进 logcat。
-- 用 WARN 级别: 够高不会被等级过滤掉, 又低于 ERROR(ERROR 会触发
--    @Common_LoggerHelper.lua 往内网 10.8.43.221 传日志 + OpenErrorTraceback 抓栈)。
local function _emit(s)
	if _logChan == nil or _logChan == 1 then
		local CS = rawget(_G, "CS")
		local LC = package.loaded["Core.Log.LoggerConst"]
		local lvl = LC and LC.WARN or 3
		local ok = pcall(function()
			local msg = "[lua][IMOMOD]" .. s

			CS.FunPlus.WorldX.Utils.LuaUtils.LogForLua(lvl, msg, #msg)
		end)

		if ok then
			_logChan = 1

			return
		end
	end

	if _logChan == nil or _logChan == 2 then
		local LM = package.loaded["Core.Log.LoggerManager"]
		local ok = pcall(function()
			LM.getLogger("IMOMOD"):warn("%s", s)
		end)

		if ok then
			_logChan = 2

			return
		end
	end

	_logChan = 3

	pcall(print, s)
end

local function MLOG(tag, fmt, ...)
	if not MOD.log then
		return
	end

	local n = (_logCnt[tag] or 0) + 1

	_logCnt[tag] = n

	if n > (LOG_CAP[tag] or 5) then
		return
	end

	local ok, s = pcall(string.format, "[IMOMOD][" .. tag .. "][#" .. n .. "] " .. fmt, ...)

	_emit(ok and s or ("[IMOMOD][" .. tag .. "] log format error"))
end

local function L(name)
	return package.loaded[name]
end

local function isMine(e)
	if type(e) ~= "table" then
		return false
	end

	if e.isMainPlayer == true or e.isMainPet == true then
		return true
	end

	local p = rawget(_G, "pg")

	if p and (e == p.me or e == p.pawn) then
		return true
	end

	return false
end

-- soloOnly 的环境门控。拿不到环境信息时【放行】(门控失败要放行, 别把功能锁死)。
local function envOk()
	if not MOD.soloOnly then
		return true
	end

	local p = rawget(_G, "pg")
	local sp = p and p.me and p.me.space

	if sp and sp.isMultiPlayerEnv then
		local ok, multi = pcall(sp.isMultiPlayerEnv, sp)

		if ok then
			return multi ~= true
		end
	end

	return true
end

local function isMyActorId(id)
	if type(id) ~= "number" then
		return false
	end

	local p = rawget(_G, "pg")

	if not p then
		return false
	end

	if p.me and p.me.actorId == id then
		return true
	end

	if p.pawn and p.pawn.actorId == id then
		return true
	end

	if p.me and p.me.getCurPetEntity then
		local ok, pet = pcall(p.me.getCurPetEntity, p.me)

		if ok and type(pet) == "table" and pet.actorId == id then
			return true
		end
	end

	return false
end

-- LiteClass / LightClass / 普通模块表: 直接赋值(它们的 metatable 只有 __index/__call)
local function patchLite(cls, key, wrap, tag)
	if type(cls) ~= "table" then
		MLOG(tag, "FAIL %s: class table is nil", key)

		return nil
	end

	local orig = cls[key]

	if type(orig) ~= "function" then
		MLOG(tag, "FAIL %s: not a function (%s)", key, type(orig))

		return nil
	end

	local wrapped = wrap(orig)
	local ok, err = pcall(function()
		cls[key] = wrapped
	end)

	MLOG(tag, "patch %s = %s %s", key, tostring(ok), tostring(err or ""))

	return ok and wrapped or nil
end

-- Class.Class 实体类: 优先写 vtbl(没有 __newindex 守卫), 退化到 forceAttrRepeat
local function patchEntityClass(cls, key, wrap, tag)
	if type(cls) ~= "table" then
		MLOG(tag, "FAIL %s: class table is nil", key)

		return false
	end

	local orig = cls[key]

	-- ☠☠☠【2026-09-30 01:14 实机抓到的第六个框架坑】
	--   实机原文: [resetpos][#1] FAIL RPC_SC_NotifyResetPositionInfo: not a function (table)
	--   根因: 框架把 Config 里的【RPC 参数类型表】按【同名 key】放到了【类表】上
	--   (@Config_Config.lua:11722 RPC_SC_NotifyResetPositionInfo = {"int","int","int"}),
	--   而类表自己的 rawget 优先于 metatable 的 __index -> cls[key] 拿到的是那张【表】,
	--   真正的方法函数在 vtbl 里(@Entities_SpaceEntities_ClientMainPlayer.lua:2066 定义的那个)。
	--   ★所以拿不到函数时必须【再问一次 vtbl】, 否则会把"能包的方法"误判成"不存在"。
	--   这一条对所有 RPC_SC_* 方法都成立, 不只是这一个 —— 以后包任何 RPC_SC 处理器都会踩。
	if type(orig) ~= "function" and type(cls.getVtbl) == "function" then
		local okV, vt = pcall(cls.getVtbl)

		if okV and type(vt) == "table" and type(vt[key]) == "function" then
			MLOG(tag, "%s: 类表上是 %s(被 Config 参数表遮蔽), 改用 vtbl 里的函数", key, type(orig))

			orig = vt[key]
		end
	end

	if type(orig) ~= "function" then
		MLOG(tag, "FAIL %s: not a function (%s)", key, type(orig))

		return false
	end

	local wrapped = wrap(orig)
	local ok, err

	if type(cls.getVtbl) == "function" then
		ok, err = pcall(function()
			cls.getVtbl()[key] = wrapped
		end)
	end

	if not ok then
		if type(cls.forceAttrRepeat) == "function" then
			pcall(cls.forceAttrRepeat, cls, key)
		end

		ok, err = pcall(function()
			cls[key] = wrapped
		end)
	end

	MLOG(tag, "patch %s = %s %s", key, tostring(ok), tostring(err or ""))

	return ok
end

---------------------------------------------------------------------
-- ① 百倍伤害: 玩家/宠物【自己的输出】命中上报, 重复发 N 次
--    上报包里本来就不含伤害值 -> 多报一次就多算一次, 不伪造任何数值,
--    飘字会自然冒 N 个(飘字来自服务器的 RPC_SC_ShowDamageNumber)。
---------------------------------------------------------------------
local DMG_OUT = {
	RPC_CS_DoActOnActorActions = true,
	RPC_CS_DoActOnSweepTargetsActions = true,
	RPC_CS_OnAbilityCollisionHitTarget = true,
	RPC_CS_ProjectileHitTarget = true
}

-- DMG_OUT 里【可以单独关掉】的条目: 名字 -> MOD 里的开关名
local DMG_OUT_SWITCH = {
	RPC_CS_ProjectileHitTarget = "ampProjectileHit"
}

local function dmgOutOn(name)
	if not DMG_OUT[name] then
		return false
	end

	local sw = DMG_OUT_SWITCH[name]

	if sw and MOD[sw] == false then
		return false
	end

	return true
end

---------------------------------------------------------------------
-- ② 无敌: 把"我被打到"的上报丢掉。
--    ☠ 必须【按被打者过滤】而不是按 RPC 名整条丢 —— 同一条 RPC 常常兼职:
--      RPC_CS_OnCSProjectileHit 的第三个发送点是玩家自己的【捕捉球命中】
--      (@GameApp_Ability_ClientCombatAction.lua:7551-7562, 那里的判据是
--       ent.actorId ~= targetActorId 而不是 ~= pg.me), 整条丢会【静默废掉抓宠】。
--    表里的 value = 从变参里取出"被打者 actorId"的函数; 返回 nil 表示无条件丢。
---------------------------------------------------------------------
local HURT_IN = {
	-- (ownerActorId, ctxId, hitEventName, combatHitTargetInfo)  @Common_Ability_MultiTargetsInfo.lua:227
	RPC_CS_NotifyActOnActor = function(a1, a2, a3, a4)
		return type(a4) == "table" and a4.actorId
	end,
	-- (ownerActorId, ctxId, hitEventName, combatHitResult)      @GameApp_Ability_ClientCombatAction.lua:3719
	RPC_CS_NotifyActOnSweepTargets = function(a1, a2, a3, a4)
		return type(a4) == "table" and (a4.actorId or a4.hitActorId)
	end,
	-- (ownerActorId, ctxId, actorId, pos)                       @GameApp_Ability_ClientCombatAction.lua:7410/7496/7562
	RPC_CS_OnCSProjectileHit = function(a1, a2, a3)
		return a3
	end,
	-- (srcActorId, gameTime, ctxId, hitEventName, combatHitResult) @GameApp_Ability_ClientProjectile.lua:574
	RPC_CS_NotifyProjHit = function(a1, a2, a3, a4, a5)
		return type(a5) == "table" and a5.hitActorId
	end,
	-- (bloodProportion, reason) 摔落/溺水/濒死自残, 恒是打自己 -> 无条件丢
	RPC_CS_SpecialDamage = false,
	-- ☠☠☠(actorId, timelineId, param, isSpike) @...ClientAbilityComponent.lua:5028
	-- 【这条千万不能放进 VICTIM_REPORT】—— 它虽然也是荆棘伤害, 但结构【反过来】:
	--   发送者是 pg.me(所以 isMine(self) 恒为真), 而真正的受害者是第一个参数 actorId
	--   对应的【怪】(Utils.isPuppet(self) 分支才走这条)。
	-- 按名字丢 = 把"怪踩到荆棘"也一起丢掉 -> 怪再也不会被荆棘扎伤, 白送敌人一个免疫。
	-- 放在 HURT_IN 里按【被打者】过滤才对: a1 是怪的 actorId, isMyActorId(a1) 永远为假,
	--   所以实际不会丢 —— 这条存在的意义是【可执行的文档】, 防止以后有人手滑把它
	--   加进 VICTIM_REPORT。
	RPC_CS_OnPuppetThornsCollision = function(a1)
		return a1
	end
}

---------------------------------------------------------------------
-- ★★★【受击方上报】RPC_CS_SetHitActionTimeline —— 现在【只服务无敌】, 不再放大。
--
-- 全库【唯一发送点】: @GameApp_Ability_ClientCombatAction.lua:2197
--   targetEntity:serverMsgNoGC("RPC_CS_SetHitActionTimeline", finalHitTimelineId, attackForceType, combatContextId)
-- ★发送者是【被打的那一方】, 不是攻击方 —— 所以这张表的判据和 DMG_OUT 正好相反。
--
-- ☠☠☠【这张表千万别整块删掉】。它曾经兼两职:
--   (a) 发送者不是我 = 我打怪 -> 放大 N 次  ==> 这就是"普攻连击", 2026-09-29 已按用户决定【删除】
--   (b) 发送者就是我 = 我被打 -> 丢掉      ==> 这是【无敌的一个独立漏口补丁】, 必须保留
--   (b) 为什么不能靠 Utils.checkValidTarget 兜: calcResult 这条路【整条都没有】经过
--   checkValidTarget(:2197 之前的 :2045 只看 targetEntity.canBeHit), 所以功能②的
--   函数级咽喉拦不住它。把这张表删了 = 静默打开一个无敌漏口, 而且只在"客户端自己当
--   master 的野怪打我"时才露出来 —— 最难复现的那类。
--
-- 为什么放弃 (a): 普攻走的就是这条, 但放大它实测没有产生连击。
--   2026-09-29 19:41~20:34 连续几轮实机: `dmg resend RPC_CS_SetHitActionTimeline x100 (victim=...)`
--   每次都打出来了, 也就是包【确实发了 100 遍】, 但游戏内普攻伤害没有变化 ->
--   服务器对这条包不按"每条算一次伤害"处理(它本来就只是"给受击方设一条受击 timeline",
--   参数里连攻击者都没有, 只有 finalHitTimelineId / attackForceType / combatContextId,
--   而 combatContextId 是 targetEntity:genCombatContextId() 现生成的)。
--   ==> 判死结论: 普攻不走可放大的上报通道。技能侧已经用①-b(投射物真多段命中)拿到连击,
--       普攻这条不再投入。这符合"日志说调了 N 次≠生效, 只认游戏内实际效果"。
---------------------------------------------------------------------
---------------------------------------------------------------------
-- ★★★【2026-09-29 22:38 实机】陷阱掉血的元凶就在这里。
-- 日志原文: [rpc:RPC_CS_OnThornsCollision] from=ClientMainPlayer(8353911) mine=true wrap=full AMP=no
-- 陷阱伤害走的是【完全独立的自伤上报通道】, 一点都不经过 Utils.checkValidTarget ——
-- 所以功能②的函数级咽喉(怪打我)对它零作用。这就是"怪物打我不掉血了, 但陷阱还掉血"。
--
-- 这两条和 SetHitActionTimeline 是【一模一样的形状】: self 就是受害者, 包里没有攻击者。
--   RPC_CS_OnThornsCollision(finalHitTimelineId, hitActionTimelineParam, isSpike)
--     @Entities_SpaceEntities_CommonComponent_ClientAbilityComponent.lua:5031
--     self:serverMsgNoGC(...) —— self = 踩到荆棘/尖刺的那个实体。荆棘 + 尖刺都走这一条
--     (isSpike 只是区分表现, 不分通道)。
--   RPC_CS_OnConductHit(finalHitTimelineId, hitActionTimelineParam)
--     同文件 :5138, playConductTimeline 的尾巴 —— 导电陷阱。
--
-- ★注意: 丢掉的只是【上报】。同一个函数里的 self.actorTimeline:setTimeline(...) 是本地
--   受击动画, 照常播 —— 所以看起来还是"被扎了一下", 只是服务器不扣血。
---------------------------------------------------------------------
local VICTIM_REPORT = {
	RPC_CS_SetHitActionTimeline = true,
	-- 荆棘 / 尖刺 (@...ClientAbilityComponent.lua:5031)
	RPC_CS_OnThornsCollision = true,
	-- 导电陷阱 (@...ClientAbilityComponent.lua:5138)
	RPC_CS_OnConductHit = true
}

-- serverMsgNoGC 内部就是 self:serverMsg(...)(@...ClientAoiComponent.lua:298-300),
-- 走实体上的动态查找 -> 只包 serverMsg 一处即可全覆盖。
-- __callAoiFunc 是同步序列化(:352), 所以同一个 table 连发 N 次是安全的。
local RPC_SUSPECT_WORDS = {
	"Hit", "Actor", "Sweep", "Target", "Conduct", "Trigger", "Attack", "Damage"
}

-- ★分段窗口【只让战斗相关的 RPC 占名额】。
-- 2026-09-29 19:50 实机教训: 25 个名额被 Heartbeat / SyncMapBlockId / RequestCreateClientEntity /
-- 别的怪的 CharacterStateChange 占满了, 真正的命中上报一条都没抓到 —— 等于白抓一轮。
-- 故意不放 "Target": 那会把满屏的 RPC_CS_LockTarget 放进来, 而 ProjectileHitTarget
-- 已经被 "Hit"/"Projectile" 命中了。
local BURST_INTEREST_WORDS = {
	"Hit", "Act", "Sweep", "Projectile", "Ability", "Timeline", "Knock", "Damage", "Cast", "Tp", "Buff", "Attack"
}

local function isBurstInteresting(name)
	for i = 1, #BURST_INTEREST_WORDS do
		if string.find(name, BURST_INTEREST_WORDS[i], 1, true) then
			return true
		end
	end

	return false
end

-- 一次施法收尾时打一条小结: 这一次施法到底有没有走到【被放大的上报】。
-- ★这是最关键的一行 —— "个别技能才有连击"的答案就在这里:
--   某个技能如果小结是「一次都没有」, 说明它的伤害根本不经过我们放大的那几条通道。
local function flushBurstSummary()
	if _burstAbility == nil then
		return
	end

	local parts = {}

	for k, v in pairs(_burstAmp) do
		parts[#parts + 1] = k .. " x" .. tostring(v)
	end

	-- ★ProjectileHitTarget 的笔数 = 这颗弹【真的命中了几下】, 这就是 projHit 到底生效没有的判据:
	--   1 = 还是单段(闸门没放开); >1 = 多段命中已经成了。
	MLOG("castend" .. _burstAbility, "=== CAST %s 结束: 被放大的上报 %d 笔 = %s ===", _burstAbility, _burstAmpTotal, #parts > 0 and table.concat(parts, ", ") or "【一次都没有】")

	_burstAmp = {}
	_burstAmpTotal = 0
	_burstAbility = nil
	_burstNoted = false
	_burstTicks = 0
end

-- 记一次"这条 RPC 被放大了", 归到当前施法名下。★不受逐条打印名额影响。
local function noteAmp(name)
	if _burstAbility ~= nil then
		_burstAmp[name] = (_burstAmp[name] or 0) + 1
		_burstAmpTotal = _burstAmpTotal + 1
	end
end

-- 每秒重发预算: 返回这一次【允许】重发多少个。
-- 这是安全阀不是功能开关 —— 目的是让 projHit x dmgMul 的乘积不会一瞬间打出上万个包。
local function resendBudget(want)
	local cap = MOD.maxResendPerCast

	if type(cap) ~= "number" or cap <= 0 then
		return want
	end

	local left = cap - _castResend

	if left <= 0 then
		return 0
	end

	if want > left then
		want = left
	end

	_castResend = _castResend + want

	return want
end

-- 只读: 把这一次 RPC 记进日志。full=true 表示这个实体走的是完整 wrapper(会放大/会丢弃)。
-- a1 = 第一个变参, 只用来在 CastAbilityOnTarget 上读出 abilityId
--      (@Config_RpcCSArgDict.lua:268 RPC_CS_CastAbilityOnTarget = {int abilityId, int targetActorId, int castSource})
local function traceRpc(self, name, full, a1)
	if not MOD.rpcTrace or type(name) ~= "string" or string.sub(name, 1, 7) ~= "RPC_CS_" then
		return
	end

	-- ★看到「开始施法」就开一个抓取窗口, 把这一次施法的战斗 RPC 序列逐条打出来
	if name == "RPC_CS_CastAbilityOnTarget" and MOD.traceBurst and MOD.traceBurst > 0 then
		flushBurstSummary()

		_burstLeft = MOD.traceBurst
		_burstAbility = tostring(a1)
		_burstSeq = 0
		_burstAmp = {}
		_burstAmpTotal = 0
		_burstNoted = false
		_burstTicks = 0

		MLOG("cast" .. tostring(a1), "=== CAST abilityId=%s caster=%s(%s) mine=%s ===", tostring(a1), tostring(rawget(self, "className") or self.className or "?"), tostring(self.actorId), tostring(isMine(self)))
	end

	local mine = isMine(self)
	local amped = "no"

	if full and AN_1 and MOD.dmgMul and MOD.dmgMul > 1 and dmgOutOn(name) and mine then
		amped = "x" .. tostring(MOD.dmgMul)
	end

	if full and AN_2 and MOD.invin and VICTIM_REPORT[name] and mine then
		amped = "DROP(victim=me)"
	end
	local suspect = ""

	if amped == "no" then
		for i = 1, #RPC_SUSPECT_WORDS do
			if string.find(name, RPC_SUSPECT_WORDS[i], 1, true) then
				suspect = "  <== SUSPECT"

				break
			end
		end
	end

	local from = tostring(rawget(self, "className") or self.className or "?")

	if _burstAbility ~= nil and isBurstInteresting(name) then
		if _burstLeft > 0 then
			_burstLeft = _burstLeft - 1
			_burstSeq = _burstSeq + 1

			-- tag 带序号 => 每条一个独立 tag, 不受"同名只打 5 条"的限制
			MLOG("burst" .. tostring(_burstAbility) .. "#" .. _burstSeq, "%s from=%s(%s) mine=%s wrap=%s AMP=%s%s", name, from, tostring(self.actorId), tostring(mine), full and "full" or "traceonly", amped, suspect)

			return
		end

		-- ★名额用完: 停止逐条打印, 但【继续计数】, 小结照样准。提示一次就够。
		if not _burstNoted then
			_burstNoted = true

			MLOG("castnote" .. tostring(_burstAbility), "=== CAST %s: 逐条名额(%s)用完, 后续只计入小结 ===", tostring(_burstAbility), tostring(MOD.traceBurst))
		end

		return
	end

	MLOG("rpc:" .. name, "from=%s(%s) mine=%s wrap=%s AMP=%s%s", from, tostring(self.actorId), tostring(mine), full and "full" or "traceonly", amped, suspect)
end

-- ★★【只追踪不改行为】的包装: 给主角/宠物【以外】的实体类用。
-- 这样既能看到普攻到底从哪个实体、走哪条 RPC 发出去的, 又【完全不碰行为】——
-- 2026-09-29 实机: 上一版把"只包两个类"换成"扫描全部实体类包完整 wrapper"之后,
-- 原本已经生效的【技能百倍伤害失效了】, 而其它功能都正常。我没能静态证明是哪一环出的岔,
-- 所以按"别叠猜测"的原则: 行为回到已验证能用的那版(只有主角/宠物走完整 wrapper),
-- 扫描保留但降级成只读追踪。要再试全量包装就把 MOD.wrapAllEntities 打开。
local function wrapTraceOnly(orig)
	return function(self, name, ...)
		traceRpc(self, name, false, ...)

		return orig(self, name, ...)
	end
end

local function wrapServerMsg(orig)
	return function(self, name, ...)
		-- ★预算清零放在这里而不是 traceRpc 里: traceRpc 被 MOD.rpcTrace 门控, 关掉日志时
		--   预算就永远不会重置了 —— 安全阀不能依赖排障开关。
		if name == "RPC_CS_CastAbilityOnTarget" and isMine(self) then
			_castResend = 0
		end

		traceRpc(self, name, true, ...)

		if AN_2 and MOD.invin and envOk() then
			local picker = HURT_IN[name]

			if picker ~= nil then
				local drop

				if picker == false then
					drop = true
				else
					local ok, id = pcall(picker, ...)

					drop = ok and isMyActorId(id)
				end

				if drop then
					MLOG("invin-rpc", "drop %s", tostring(name))

					return
				end
			end
		end

		-- ★受击方上报: 发送者就是被打的那个实体 -> 发送者是我就说明【我挨了一下】, 丢掉。
		-- 这是无敌的独立漏口补丁, 不是伤害功能。放大那半已按用户决定删除(见 VICTIM_REPORT 上方说明)。
		if AN_2 and MOD.invin and VICTIM_REPORT[name] and isMine(self) and envOk() then
			MLOG("invin-victim", "drop %s (victim=me)", tostring(name))

			return
		end

		if AN_1 and MOD.dmgMul and MOD.dmgMul > 1 and dmgOutOn(name) and isMine(self) and envOk() then
			local extra = resendBudget(MOD.dmgMul - 1)

			if extra < MOD.dmgMul - 1 then
				MLOG("dmgcap", "budget clamp %s: %d -> %d (cap=%s/秒)", tostring(name), MOD.dmgMul - 1, extra, tostring(MOD.maxResendPerCast))
			end

			if extra > 0 then
				noteAmp(name)
				MLOG("dmg", "resend %s x%d", tostring(name), extra + 1)

				for _ = 1, extra do
					local ok, err = pcall(orig, self, name, ...)

					if not ok then
						MLOG("dmg", "resend broke at %s: %s", tostring(name), tostring(err))

						break
					end
				end
			end
		end

		return orig(self, name, ...)
	end
end

---------------------------------------------------------------------
-- ★ serverMsg 的覆盖面: 必须包【所有实体类】, 不能只包 ClientMainPlayer/ClientPet
--   根因: serverMsg 定义在 class.Component("ClientAoiComponent") 里, 而框架在
--   Class.AddComponents 时把组件方法【按值拷贝】进每一个实体类的 vtbl
--   (@Core_Framework_Class.lua:904-906 `cls[name] = attr`) -> 每个实体类各有一份独立的
--   serverMsg。只包两个类 = 其它实体发出去的上报完全不经过我们(普攻很可能就漏在这)。
-- 做法: 扫 package.loaded, 把【自己 vtbl 里有 serverMsg】的类全部包上。
--   廉价过滤用 rawget(mod, "typeName"): Class.Class 在 :225 就 rawset 了 typeName,
--   早于 :399 的 setmetatable(LiteClass 同理 :712), 所以 rawget 拿得到、不触发任何 __index、
--   也不会误伤普通模块表。过滤完只剩 ~200 个类表, 再逐个 pcall 探测。
-- 幂等: 包好的 wrapper 进 _wrapMark, 重复扫不会二次包装; 基类/子类 vtbl 的缓存
--   (@Core_Framework_Class.lua:517 `vtbl[key] = ret`)无论先后顺序都不会出现双重包装。
-- 每秒 tick 会补扫一次, 覆盖启动后才加载进来的实体类。
---------------------------------------------------------------------
local _wrapMark = setmetatable({}, {
	__mode = "k"
})

local function _probeAndWrap(mod, acc)
	local gv = mod.getVtbl

	if type(gv) ~= "function" then
		return
	end

	local vt = gv()

	if type(vt) ~= "table" then
		return
	end

	local f = rawget(vt, "serverMsg")

	if type(f) ~= "function" or _wrapMark[f] then
		return
	end

	-- 默认只上【只读追踪】包装; MOD.wrapAllEntities 打开才上完整 wrapper
	local wrapped = MOD.wrapAllEntities and wrapServerMsg(f) or wrapTraceOnly(f)

	_wrapMark[wrapped] = true
	vt.serverMsg = wrapped
	acc.n = acc.n + 1

	if #acc.names < 8 then
		acc.names[#acc.names + 1] = tostring(rawget(mod, "typeName"))
	end
end

---------------------------------------------------------------------
-- ①-b 投射物技能的连击: 让投射物【真的】多打几下, 不是把同一条上报重发。
--
-- ★为什么必须另开一条(2026-09-29 静态坐实, 这就是"只有个别技能有连击"的结构性原因):
--   近战/扫击的命中上报包里【没有任何实例身份】:
--     RPC_CS_DoActOnActorActions(actionData.NodeID, dynamicInfo)
--       @Common_Ability_MultiTargetsInfo.lua:222
--     RPC_CS_DoActOnSweepTargetsActions(hitResults, NodeID, dynamicInfo)
--       @GameApp_Ability_ClientCombatAction.lua:3692
--   -> 服务器按每条上报跑一遍 action 节点, 重发 N 次真的算 N 次。★有连击。
--   投射物的命中上报第一个参数就是【投射物实例 id】:
--     RPC_CS_ProjectileHitTarget(self.instanceId, combatHitResult, pos, rot, curTime, rebound)
--       @GameApp_Ability_ClientProjectile.lua:546
--   而且命中后紧跟着还会发 RPC_CS_ProjectileDestroy(同一个 instanceId, 同文件 :594)
--   -> 同一颗弹重发 100 次, 服务器只认第一次。★没连击。
--
-- ★正解 = 改行为不改上报: 把我方投射物改成【穿透 + 可重复命中同一目标】,
--   它就真的命中 maxHitNum 次, 每次都是一条合法的、curTime/curHitIdx 各不相同的上报。
--   三个闸门全是游戏自己的字段:
--     a) destroyOnHit       @Common_Ability_Projectile_Projectile.lua:35   默认 true -> 命中即销毁
--     b) maxHitNum          同文件 :767  默认 1, 上限 ProjectileConst.MAX_PROJECTILE_HIT_COUNT_LIMIT
--                           = 64 (@Common_Const_ProjectileConst.lua:4)
--     c) checkCanHitTarget  同文件 :971  return self.hitTargetSet[hitActorId] == nil
--                           -> 挡住"同一个目标打第二下"
--   这三个的读点在 ClientProjectile:raycastCallback @GameApp_Ability_ClientProjectile.lua:395-407:
--     if curHitIdx >= 1 then
--       if destroyOnHit then 销毁/回收
--       elseif curHitIdx >= maxHitNum then 销毁
--       else isEnd = false  <== 继续飞, 可以再命中
--
-- ☠只对【我方】投射物开。self.owner 就是 srcActorId 对应的实体(同文件 :82 setOwner),
--   给怪的弹开 = 怪的弹穿透我 64 下, 等于自杀。
-- ☠Projectile 是 Class.LiteClass("Projectile", AbilityObject), 子类
--   ClientProjectile(:26) / EditorProjectile(:5) 会把父类方法 rawset 缓存进自己表里
--   (@Core_Framework_Class.lua:739-745) -> 三个类都要各打一遍, 只打父类会被子类的缓存挡掉。
---------------------------------------------------------------------
local PROJ_CLASSES = {
	{
		"Common.Ability.Projectile.Projectile",
		"Projectile"
	},
	{
		"GameApp.Ability.ClientProjectile",
		"ClientProjectile"
	},
	{
		"GameApp.Ability.EditorProjectile",
		"EditorProjectile"
	}
}

-- 这颗弹是不是我方发出去的
local function isMyProjectile(p)
	if type(p) ~= "table" then
		return false
	end

	if isMine(p.owner) then
		return true
	end

	return isMyActorId(p.srcActorId)
end

local _projPatched = {}

local function patchProjectileMultiHit()
	local LIMIT = 64
	local PC = L("Common.Const.ProjectileConst")

	if PC and type(PC.MAX_PROJECTILE_HIT_COUNT_LIMIT) == "number" then
		LIMIT = PC.MAX_PROJECTILE_HIT_COUNT_LIMIT
	end

	local done = {}

	for _, it in ipairs(PROJ_CLASSES) do
		local cls = L(it[1])

		if type(cls) == "table" and not _projPatched[it[2]] then
			-- a+b) loadDataFromTemplate 之后把闸门放开(只对我方)
			local okA = patchLite(cls, "loadDataFromTemplate", function(orig)
				return function(self, ...)
					local ret = orig(self, ...)
					local n = MOD.projHit

					if ret ~= false and AN_1 and type(n) == "number" and n > 1 and isMyProjectile(self) and envOk() then
						self.destroyOnHit = false
						self.maxHitNum = math.min(LIMIT, n)

						-- ☠tag 必须和安装期的 "projhit" 分开: 安装期 3 类 x 2 方法 = 6 条,
						--   会把这条运行时的关键证据整条挤掉(2026-09-29 20:34 实机就这么丢的)。
						MLOG("projopen", "open %s tmpl=%s maxHitNum=%s destroyOnHit=false owner=%s", it[2], tostring(self.templateId or self.projectileTemplateId or "?"), tostring(self.maxHitNum), tostring(self.owner and self.owner.actorId or "?"))
					end

					return ret
				end
			end, "projhit")

			-- c) 允许同一个目标被同一颗弹反复命中(只对我方)
			local okB = patchLite(cls, "checkCanHitTarget", function(orig)
				return function(self, hitActorId, ...)
					local n = MOD.projHit

					if AN_1 and type(n) == "number" and n > 1 and isMyProjectile(self) and envOk() then
						-- ★不写 hitTargetSet, 只放行判据 -> 上限仍由 maxHitNum 兜着(raycastCallback:402)
						return true
					end

					return orig(self, hitActorId, ...)
				end
			end, "projhit")

			if okA and okB then
				_projPatched[it[2]] = true
				done[#done + 1] = it[2]
			end
		end
	end

	return done
end

---------------------------------------------------------------------
-- ⑥ 自动开宝箱 / 自动拾取
--
-- ★★★【不手写"枚举附近实体 -> 判距离 -> 调 doInteract"】—— 游戏自己就带一个自动交互系统,
--   借它的开关即可。这是本作里最干净的一处改法: 发出去的上报包和真人走过去自动开
--   【一模一样】, 因为执行的确实就是游戏自己那条路。
--
-- 机制(每一条都亲自读过):
--   pg.game.interaction = InteractionSystem, 以 0.3 秒为周期 tick
--     @GameApp_Core_GameApp.lua:399-401  self:addSystem(self.interaction, 0.3, true)
--   InteractionUnitBase:checkIsAutoInteract() 的判据只有一条:
--     self.interactData.actWhenArrive == 1     @GameApp_Interaction_InteractionUnitBase.lua:742
--   而 refreshInteractUnit【每个 tick 都重建】自动交互名单:
--     @GameApp_Interaction_InteractionSystem.lua:714 table.clear(self.curAutoInteractUnit)
--     -> :747 按 checkIsAutoInteract() 重新收集
--   ==> 所以只要把 actWhenArrive 置 1, 下一个 tick 游戏就自己接手, 不用去碰 curAutoInteractUnit。
--   然后 :698 updateAutoInteracts -> unit:tryAutoInteract()
--     @GameApp_Interaction_InteractionUnitBase.lua:731-743:
--     1 秒节流(:733) + canInteractive()(:737) + doInteract()(:740)
--
-- ★为什么这比自己调 ent:doInteract() 好: canInteractive()(:65-145) 那一整套门全都保留 ——
--   visible / ent:checkCanInteract / 交互角度 / 前向角度 / 空中阻挡 / 交互阻挡 / conditionId /
--   蛋形态白名单 / pg.pawn:checkAutoInteract 状态检查 / 组队单人世界的归属 /
--   checkDistanceValid / checkPrototypeId。
--   自己硬调 = 把这些门全绕开, 只会换来服务器拒绝 + 满屏 showBubbleMessage 报错
--   (@Entities_SpaceEntities_CommonComponent_ClientInteractComponent.lua:82-92)。
--   顺便也躲掉了 startInteract 的单槽坑: self.startInteractTimer / startInteractCallback
--   是【单个字段】(同文件 :63-64), 一次性给 N 个目标排队会互相覆盖; 走游戏自己的路
--   天然是一个一个来的。
--
-- ☠actWhenArrive 写在 unit.interactData 上是安全的: 那是【每个 unit 自己的副本】
--   @GameApp_Interaction_InteractionUnitBase.lua:43
--     self.interactData = InteractData[id] and lume.clone(InteractData[id]) or {}
--   不会污染共享的 InteractData 配置表。
--
-- ☠☠☠体力陷阱(这条最容易把好功能变成坑): 有些宝箱是【要花体力/道具开】的。
--   unit.needItem 就是消耗(同文件 :39 self.needItem = info.needItem, 来源 chest_data),
--   而 needItem[1] == 1009 就是体力(Const.CommonEnergyType_Stamina = 1009
--   @Common_Const_Const.lua:3270; InteractionConst.ChestInteractShowSpecialItemId 也点名了它)。
--   ★注意: 确认弹窗只在 chest_data.showDetail 为真时才出现
--   (@Entities_SpaceEntities_ClientChest.lua:236-259), showDetail 为空的箱子游戏自己也是
--   直接 doInteract()(:261) —— 也就是说【连原版都不一定给你看见消耗】。
--   -> 默认跳过所有带 needItem 的, 要开自己把 autoChestCost 打开。
--
-- ☠只认宝箱和掉落物: Utils.isChest(@Common_Utils_Utils.lua:1456) /
--   Utils.isCollectItem(:1464)。EnvObject(门/机关/采集点)默认不动 ——
--   自动去拉机关会开出你没打算开的门, 单独给 autoChestEnvObj 开关。
--
-- ☠客户端的距离门【绕开毫无意义】: checkDistanceValid(:159-173) 走的是 C# 触发体积
--   ent.eModel:CheckTriggerDistanceValid(triggerId); 而 RPC_CS_Interact 包里
--   根本不带坐标(@Config_RpcCSArgDict.lua:210-215) -> 服务器拿它自己那份位置判距离。
--   所以"开远处的箱子"只能【真的把人挪过去】, 不能骗判定。
---------------------------------------------------------------------

-- 被我们改过 actWhenArrive 的 unit -> 它【原来】的值。弱键: unit 销毁时自动回收。
-- 存 false 代表"原来是 nil", 这样还原时能把 nil 和 0 区分开。
local _armed = setmetatable({}, {
	__mode = "k"
})

-- 这个 unit 该不该自动开? 返回它的实体(用于日志), 不该开返回 nil。
local function autoChestPick(unit)
	if type(unit) ~= "table" or type(unit.interactData) ~= "table" then
		return nil
	end

	-- 消耗型: 默认跳过, 别抽体力
	if not MOD.autoChestCost and type(unit.needItem) == "table" and type(unit.needItem[1]) == "number" and unit.needItem[1] > 0 then
		return nil
	end

	if type(unit.getEntity) ~= "function" then
		return nil
	end

	local ok, ent = pcall(unit.getEntity, unit)

	if not ok or type(ent) ~= "table" then
		return nil
	end

	local U = L("Common.Utils.Utils") or L("Utils.Utils")

	if type(U) == "table" then
		if type(U.isChest) == "function" and U.isChest(ent) then
			return ent
		end

		if type(U.isCollectItem) == "function" and U.isCollectItem(ent) then
			return ent
		end

		if MOD.autoChestEnvObj and type(U.isEnvObj) == "function" and U.isEnvObj(ent) then
			return ent
		end

		return nil
	end

	-- 拿不到 Utils 时退化到实体自己的标记位(Utils.isChest 的判据里读的就是它)
	if ent.isChest == true or ent.isCollectItem == true then
		return ent
	end

	return nil
end

-- 把原值还原回去(开关关掉 / 这个 unit 不再符合条件时)
local function disarmAutoInteract(unit)
	local orig = _armed[unit]

	if orig == nil then
		return
	end

	if type(unit.interactData) == "table" then
		unit.interactData.actWhenArrive = orig ~= false and orig or nil
	end

	_armed[unit] = nil
end

local function armAutoInteract()
	local p = rawget(_G, "pg")
	local IS = p and p.game and p.game.interaction

	if type(IS) ~= "table" or type(IS.unitMap) ~= "table" then
		MLOG("chestwhy", "bail: pg.game.interaction%s", type(IS) ~= "table" and " 不存在" or ".unitMap 不是表")

		return
	end

	local IC = L("Common.Const.InteractionConst")
	local ENT_FUNC = IC and IC.INTERACTION_TYPE_ENT_FUNC or 1
	local list = IS.unitMap[ENT_FUNC]

	if type(list) ~= "table" then
		MLOG("chestwhy", "bail: unitMap[ENT_FUNC=%s] 还没有(附近没有可交互实体)", tostring(ENT_FUNC))

		return
	end

	-- ☠AN_6 必须同时管住自动开箱: tpToChest 【故意】不自己开箱(刚落地时交互 unit 还不存在),
	--   开箱是靠这里标上 actWhenArrive 之后由游戏自己的 InteractionSystem 做的。
	--   所以只开瞬移不开自动开箱 = 瞬移过去了但箱子不开 —— 两条必须绑在一个开关上。
	local on = AN_6 and MOD.autoChest and envOk()

	for _, unit in pairs(list) do
		if type(unit) == "table" and type(unit.interactData) == "table" then
			local ent = on and autoChestPick(unit) or nil

			if ent then
				if unit.interactData.actWhenArrive ~= 1 then
					if _armed[unit] == nil then
						_armed[unit] = unit.interactData.actWhenArrive or false
					end

					unit.interactData.actWhenArrive = 1

					MLOG("chest", "arm %s tmpl=%s chest=%s collect=%s needItem=%s", tostring(unit.targetName), tostring(ent.templateId), tostring(ent.isChest), tostring(ent.isCollectItem), tostring(type(unit.needItem) == "table" and unit.needItem[1] or "none"))
				end
			else
				disarmAutoInteract(unit)
			end
		end
	end
end

---------------------------------------------------------------------
-- ⑦ 跳跃瞬移到最近的宝箱(绑在无限跳上)
--
-- ★★★【用游戏自己的合法传送流程, 不硬写坐标】。原文照抄
--   @GameApp_Sandbox_LevelItem.lua:302-317 LevelItem:playerFaceToInteractUnit, 就两步:
--     pg.me:forbidPositionCheck({
--         Const.FORBID_POSITION_REASON.LEVEL_ITEM_INTERACT,   -- [1] 理由
--         self.id,                                            -- [2] contextId
--         levelItemInteractSB.transform.position,             -- [3] 目标坐标
--         self.sandbox.id                                     -- [4] staticId(可省)
--     })
--     pg.pawn:forceSetPosRot(targetPos, targetRotation, instant)
--   ☠两个容易搞错的地方:
--     ①【报备走 pg.me, 挪位置走 pg.pawn】—— 当前操控的可能是宠物, 游戏自己就是这么分的。
--     ②参数是【位置数组】不是键值表。@GameApp_DialogueGraph_DialogueItemCmd.lua:439
--       只传三个元素, 所以第四个 staticId 可省。
--
-- 为什么必须先报备: 服务器有位置校验。RPC_CS_ForbidPositionCheck(@Config_RpcCSArgDict.lua:974,
--   发送点 @...ClientAoiComponent.lua:583)的语义就是"我接下来要出现在 targetPos, 理由是 R"。
--   游戏自己每一次合法传送都是先发这条、再写位置(对话/对话组/关卡物件交互/载具跳下/社交动作,
--   五个理由见 Const.FORBID_POSITION_REASON @Common_Const_Const.lua:1366)。
--   不报备直接写位置 -> 服务器可能把你拉回去。
--
-- ★为什么这条路能成立, 而"远程开箱"不能:
--   RPC_CS_Interact 包里【不带坐标】(@Config_RpcCSArgDict.lua:210-215), 服务器拿它自己那份
--   位置判距离。所以先把人【真的】挪过去 -> 距离判定是真过的, 开箱那一步完全合法;
--   而想骗距离判定则必被拒(NoticeDef.ERROR_INVALID_DISTANCE = 1026 @Common_NoticeDef.lua:194)。
--   客户端那个 checkDistanceValid 走的是 C# 触发体积
--   (@GameApp_Interaction_InteractionUnitBase.lua:159-173 eModel:CheckTriggerDistanceValid),
--   绕开它也只是让客户端肯发包而已, 服务器照样拒。
--
-- ★位置是【客户端上传】的: ClientAuthorityComponent:refreshPosSyncState
--   (@...ClientAuthorityComponent.lua:84-100)把 aoi.isSender / eModel.syncToServer 交给 C#,
--   全库【没有任何 Lua 的位置上传 RPC】(RpcCSArgDict 里 SyncPos/UpdatePos/MoveTo/ReportPos
--   /PosSync/Transform 全部零命中) -> 上传在 C# 层, Lua 改完位置它自己会传。
--   forceSetPosRot 内部只是 setPosition/setRotation(@...ClientAoiComponent.lua:527-535),
--   走 EModelUtils 到 eModel, 不发任何 RPC。
--
-- ☠⏳【未实机, 有一个真实的不确定】: 服务器会不会接受我们这个 reason/contextId 组合
--   (LEVEL_ITEM_INTERACT + 宝箱实体 id)。若被拒, 症状是瞬移后被拉回原地。
--   -> 已加【落点复查探针】: 瞬移后记下目标点, 下一个 tick 回读实际位置对比,
--      日志打 [tpcheck] kept / SNAPPED, 一轮实机就能定论。这是"打改前->改后"的老规矩。
---------------------------------------------------------------------

-- 瞬移后的落点复查: {x, y, z, 模板id}
local _tpWatch = nil

-- 浮空剩余秒数(>0 表示正在浮空)。★和 _forceOpen 一样, 必须声明在 tpToChest 之前 ——
--   本作 _G 是严格的, 写未声明的全局会被静默吞掉(见文件头第①条坑)。
local _floatLeft = 0
local _floatPawn = nil

-- 待强制开的目标: { ent, kind, tries, sent }
-- ☠☠☠这个 local【必须声明在 tpToChest 之前】。上一版把它放在 tickForceOpen 旁边(更靠后),
--   结果 tpToChest 里的 `_forceOpen = {...}` 写的是【全局】, 而本作 _G 装了严格 __newindex
--   (@Core_Framework_Global.lua:48)会把未声明的全局赋值【静默吞掉】—— 强制开箱一次都没触发,
--   而且不报任何错。这就是本文件头第①条坑的现场复现。
--   ★离线自测的假环境里复刻了那个严格 _G, 所以这个 bug 是被测试抓出来的, 没上实机。
local _forceOpen = nil

-- ☠☠☠【2026-09-29 22:24 实机根因】上一版搜的是【交互 unit】(pg.game.interaction.unitMap),
--   而交互 unit 只有在实体【靠近时】才被创建 —— 远处的箱子根本没有 unit,
--   所以 nearestChestUnit 永远找不到目标, 静默返回 false, 落回普通跳跃。
--   用户实测的现象完全对得上: "距离很近的话 跳过去 就开箱子了"。
-- ==> 改成搜【宝箱实体】本身。两个来源:
--   源A: pg.me:GetRangeEntsChest() —— 游戏【自己的宝箱追踪器】, 范围
--        SysConfigData.CHEST_TRACK_MAX_DISTANCE 默认 110 米
--        (@Entities_SpaceEntities_PlayerComponent_ClientChestComponent.lua:82/:95),
--        返回 ChestDetect.rangeEntsChest = actorId -> ent 的表。比交互范围大一个数量级。
--   源B: pawn:entitiesInRange(range, Const.SEARCH_USR_TYPE_INTERACTOR, 0) —— 通用 AOI 查询
--        (@...ClientAoiComponent.lua:204, range 单位是米; INTERACTOR = 64
--        @Common_Const_Const.lua:57)。宝箱和掉落物都是 interactor, 所以这条也覆盖掉落物。
--        ☠它返回的是【actorId 数组】不是实体数组(判据: @GameApp_Ability_ClientProjectile.lua:558
--        拿它的元素和 pg.me.actorId 比), 所以要 pg.getEntityByActorId 解一次。
-- 瞬移到位之后 unit 会被游戏自己创建, 再由 armAutoInteract 标上 actWhenArrive -> 自动开。

-- 实体级过滤。返回 true/false + 原因字符串(原因会进诊断日志)。
-- ☠不调 ent:checkCanInteract(nil): ClientEnvObject 那个实现会解引用 unit 参数直接崩。
local function chestEntOk(ent)
	if type(ent) ~= "table" then
		return false, "notTable"
	end

	-- 消耗型默认跳过。★needItem 在【实体】上就有
	--   (@Entities_SpaceEntities_ClientChest.lua:85 self.needItem = cdd.needItem)
	if not MOD.autoChestCost and type(ent.needItem) == "table" and type(ent.needItem[1]) == "number" and ent.needItem[1] > 0 then
		return false, "cost"
	end

	local p = rawget(_G, "pg")
	local U = L("Common.Utils.Utils") or L("Utils.Utils")

	if type(U) ~= "table" then
		return false, "noUtils"
	end

	if type(U.isChest) == "function" and U.isChest(ent) then
		if type(ent.isChestOpened) == "function" then
			local ok, opened = pcall(ent.isChestOpened, ent)

			if ok and opened then
				return false, "opened"
			end
		end

		-- isChestVisible 里含 openChestLimit(开启次数上限)和归属判断
		-- (@Common_Utils_Utils.lua:8907)
		if type(U.isChestVisible) == "function" and p and p.me then
			local ok, vis = pcall(U.isChestVisible, p.me, ent)

			if ok and not vis then
				return false, "invisible"
			end
		end

		return true, "chest"
	end

	if type(U.isCollectItem) == "function" and U.isCollectItem(ent) then
		if type(ent.isCollectItemOpened) == "function" then
			local ok, opened = pcall(ent.isCollectItemOpened, ent)

			if ok and opened then
				return false, "opened"
			end
		end

		if type(U.isCollectItemVisible) == "function" and p and p.me then
			local ok, vis = pcall(U.isCollectItemVisible, p.me, ent)

			if ok and not vis then
				return false, "invisible"
			end
		end

		return true, "collect"
	end

	return false, "notChest"
end

-- 把一个元素(可能是实体表, 也可能是 actorId)解成实体
local function resolveEnt(v)
	if type(v) == "table" then
		return v
	end

	if type(v) == "number" then
		local p = rawget(_G, "pg")

		if p and type(p.getEntityByActorId) == "function" then
			local ok, e = pcall(p.getEntityByActorId, v)

			if ok and type(e) == "table" then
				return e
			end
		end
	end

	return nil
end

-- 收集范围内【所有】可开的宝箱/掉落物实体, 按距离升序返回。
-- 返回 list = { {ent=e, dist=d, kind=k}, ... }, diag = 一行诊断字符串(各来源数量 + 淘汰原因计数)
-- ★从 nearestChestEnt 里抽出来的: ⑦瞬移只要最近那个, ⑧距离阶梯探针要整条列表。
local function collectChestEnts(pawn, range)
	local p = rawget(_G, "pg")

	if type(pawn) ~= "table" or type(pawn.getPosition) ~= "function" then
		return nil, "pawn 没有 getPosition"
	end

	local V3 = rawget(_G, "Vector3")

	if type(V3) ~= "table" or type(V3.SqrDistance) ~= "function" then
		return nil, "拿不到 Vector3.SqrDistance"
	end

	local CONST = L("Common.Const.Const")
	local okMe, myPos = pcall(pawn.getPosition, pawn)

	if not okMe or type(myPos) ~= "table" then
		return nil, "读不到自己的位置"
	end

	local seen = {}
	local cand = {}
	local nA, nB = 0, 0

	-- 源A: 游戏自己的宝箱追踪器(110 米)
	if p and p.me and type(p.me.GetRangeEntsChest) == "function" then
		local ok, list = pcall(p.me.GetRangeEntsChest, p.me)

		if ok and type(list) == "table" then
			for _, v in pairs(list) do
				local e = resolveEnt(v)

				if e and not seen[e] then
					seen[e] = true
					nA = nA + 1
					cand[#cand + 1] = e
				end
			end
		end
	end

	-- 源B: AOI 里所有 interactor(宝箱 + 掉落物)
	if type(pawn.entitiesInRange) == "function" and CONST and CONST.SEARCH_USR_TYPE_INTERACTOR then
		local ok, list = pcall(pawn.entitiesInRange, pawn, range, CONST.SEARCH_USR_TYPE_INTERACTOR, 0)

		if ok and type(list) == "table" then
			for _, v in pairs(list) do
				local e = resolveEnt(v)

				if e and not seen[e] then
					seen[e] = true
					nB = nB + 1
					cand[#cand + 1] = e
				end
			end
		end
	end

	local out = {}
	local why = {}
	local okCnt, farCnt = 0, 0
	local maxSqr = range * range

	for i = 1, #cand do
		local ent = cand[i]
		local pass, reason = chestEntOk(ent)

		if not pass then
			why[reason] = (why[reason] or 0) + 1
		elseif type(ent.getPosition) ~= "function" then
			why["noPos"] = (why["noPos"] or 0) + 1
		else
			okCnt = okCnt + 1

			local okE, ePos = pcall(ent.getPosition, ent)

			if okE and type(ePos) == "table" then
				local okD, d = pcall(V3.SqrDistance, myPos, ePos)

				if okD and type(d) == "number" then
					if d <= maxSqr then
						out[#out + 1] = {
							ent = ent,
							dist = math.sqrt(d),
							kind = reason
						}
					else
						farCnt = farCnt + 1
					end
				end
			end
		end
	end

	table.sort(out, function(a, b)
		return a.dist < b.dist
	end)

	local parts = {}

	for k, v in pairs(why) do
		parts[#parts + 1] = k .. "=" .. tostring(v)
	end

	local okFmt, diag = pcall(string.format, "追踪器=%d AOI=%d 候选=%d 合格=%d 超出%dm=%d 淘汰[%s]", nA, nB, #cand, okCnt, range, farCnt, table.concat(parts, ","))

	return out, okFmt and diag or "diag format error"
end

-- 找最近的可开宝箱/掉落物【实体】。返回 ent, 距离, 类型("chest"/"collect")
local function nearestChestEnt(pawn)
	local range = type(MOD.jumpToChestRange) == "number" and MOD.jumpToChestRange or 60

	-- 被【真实搬运能力】夹住(jumpToChestMaxDist 默认 0 = 不夹, 见开关处说明)
	local maxD = MOD.jumpToChestMaxDist

	if type(maxD) == "number" and maxD > 0 and range > maxD then
		range = maxD
	end

	local list, diag = collectChestEnts(pawn, range)

	if not list then
		MLOG("tpwhy", "bail: %s", tostring(diag))

		return nil
	end

	if #list == 0 then
		-- ★没找到就把【每一步的数字】打出来, 一轮实机就能定位是哪一环空的
		MLOG("tpwhy", "no target: %s", tostring(diag))

		return nil
	end

	local it = list[1]

	return it.ent, it.dist, it.kind
end

-- 瞬移到最近的宝箱并把它标成自动开。成功返回 true(调用方就【不要再跳】了)。
local function tpToChest(pawn)
	if not (AN_6 and MOD.jumpToChest and envOk()) then
		return false
	end

	local p = rawget(_G, "pg")

	if not p or type(p.me) ~= "table" or type(pawn) ~= "table" then
		return false
	end

	local V3 = rawget(_G, "Vector3")
	local CONST = L("Common.Const.Const")

	if type(V3) ~= "function" and type(V3) ~= "table" then
		MLOG("tpwhy", "bail: 拿不到 Vector3")

		return false
	end

	if type(CONST) ~= "table" or type(CONST.FORBID_POSITION_REASON) ~= "table" then
		MLOG("tpwhy", "bail: 拿不到 Const.FORBID_POSITION_REASON")

		return false
	end

	local ent, dist, kind = nearestChestEnt(pawn)

	if not ent then
		return false
	end

	-- ★游戏自己的玩家搬运上限。官方节点超了就直接拒绝, 我们也别白试。
	--   @GameApp_DialogueGraph_DialogueGraphRuntime_Node_DialogueGraphEntitySetPosAndRotNode.lua:29-33
	local maxD = MOD.jumpToChestMaxDist

	if type(maxD) == "number" and maxD > 0 and type(dist) == "number" and dist > maxD then
		MLOG("tpwhy", "目标太远: %.1fm > 上限 %.1fm (游戏自己的 ENTITY_PLAYER_SET_POSITION_DISTANCE_MAX=25), 走普通跳跃", dist, maxD)

		return false
	end

	local okE, ePos = pcall(ent.getPosition, ent)
	local okM, myPos = pcall(pawn.getPosition, pawn)

	if not okE or not okM or type(ePos) ~= "table" or type(myPos) ~= "table" then
		MLOG("tpwhy", "bail: 目标或自己的位置读不到")

		return false
	end

	-- ☠getPosition 返回的是【共享引用】self.posRef(@...ClientPosRotComponent.lua:112),
	--   绝对不能在它上面调 :Sub()/:Mul() 这类【原地改写】的方法 —— 那会把实体真实坐标改坏。
	--   所以这里只读 x/y/z 标量, 自己算, 最后用 Vector3(x,y,z) 造一个新的。
	local ex, ey, ez = ePos.x, ePos.y, ePos.z
	local mx, mz = myPos.x, myPos.z

	if type(ex) ~= "number" or type(ey) ~= "number" or type(ez) ~= "number" or type(mx) ~= "number" or type(mz) ~= "number" then
		MLOG("tpwhy", "bail: 坐标不是数字(x=%s y=%s z=%s)", tostring(ex), tostring(ey), tostring(ez))

		return false
	end

	-- ★先算【从宝箱指向我】的单位方向, 侧向偏移和朝向都用它 ——
	--   分开算是为了 stand=0(正上方)时朝向依然有效(零向量喂给 LookRotation 是坏的)。
	local ux, uz = 1, 0
	local rx, rz = mx - ex, mz - ez
	local len = math.sqrt(rx * rx + rz * rz)

	if len > 0.01 then
		ux, uz = rx / len, rz / len
	end

	-- ★落在宝箱【朝我这一侧】前 stand 米、同高度(up=0)。
	--   这一段和游戏自己的 getInteractPos 同构(见 jumpToChestStand 开关处的长注释)。
	-- ☠别改成"正上方"(stand=0 + up>0): 23:09 实机证实箱子头顶不是合法站位, 服务器
	--   回 1002 ERROR_POSITION_VALID, 人被弹回原地。
	local stand = type(MOD.jumpToChestStand) == "number" and MOD.jumpToChestStand or 0.8
	local up = type(MOD.jumpToChestUp) == "number" and MOD.jumpToChestUp or 0

	-- ☠硬下限: 防止手滑填 0/负数 把 23:09 那次已知的失败直接重演一遍
	local standMin = type(MOD.jumpToChestStandMin) == "number" and MOD.jumpToChestStandMin or 0.3
	local standNote = ""

	if stand < standMin then
		standNote = string.format("(填的 %.2f 太小, 被下限 %.2f 夹住)", stand, standMin)
		stand = standMin
	end

	-- ★只读对照: 游戏配表自己认为这个箱子的交互站位距离是多少
	--   (@GameApp_Interaction_InteractionUnitBase.lua:312 读的就是这个字段)。
	--   ☠interact_data 不在反编译产物里, 所以只能运行时读; 读不到就算了, 不影响功能。
	local cfgStand = "?"
	local ID = L("Data.interact_data")

	if type(ID) == "table" and ent.actionPrototypeId then
		local row = ID[ent.actionPrototypeId]

		if type(row) == "table" and type(row.interactiveForceDist) == "number" then
			cfgStand = string.format("%.2f", row.interactiveForceDist)
		end
	end
	local tx, ty, tz = ex + ux * stand, ey + up, ez + uz * stand
	local okT, target = pcall(V3, tx, ty, tz)

	if not okT or type(target) ~= "table" then
		MLOG("tpwhy", "bail: Vector3(x,y,z) 造不出来")

		return false
	end

	-- 面朝宝箱。游戏自己算朝向也是 Quaternion.LookRotation(dir, Vector3.up)
	-- (@Common_Ability_CombatActionTool.lua:775 等十几处)
	local rot
	local Q = rawget(_G, "Quaternion")

	if type(Q) == "table" and type(Q.LookRotation) == "function" then
		-- 面朝宝箱 = 单位方向取反(ux,uz 一定是单位向量, 退化时是 (1,0), 不会喂零向量)
		local okD, dirV = pcall(V3, -ux, 0, -uz)

		if okD then
			local okR, r = pcall(Q.LookRotation, dirV, V3.up)

			if okR then
				rot = r
			end
		end
	end

	-- ①先向服务器报备(照抄 LevelItem:playerFaceToInteractUnit 的参数形状)
	local okF, errF = pcall(p.me.forbidPositionCheck, p.me, {
		CONST.FORBID_POSITION_REASON.LEVEL_ITEM_INTERACT,
		ent.id,
		target
	})

	-- ②★把角色控制器踢回 IDLE —— 官方流程的第③步, 我原来漏了这一步。
	--   直接调游戏自己的函数(照抄调用, 别自己重写状态判断)。
	if MOD.jumpToChestIdleFirst then
		local before = tostring(pawn.characterState)
		local DGU = L("GameApp.DialogueGraph.DialogueGraphUtils")
		local okIdle = false

		if DGU and type(DGU.forceChangeToIdle) == "function" then
			okIdle = pcall(DGU.forceChangeToIdle, pawn)
		end

		-- 退化: 拿不到那个模块就自己按同样的判据踢一次
		--   (@GameApp_DialogueGraph_DialogueGraphUtils.lua:299-301 的原文)
		if not okIdle then
			local CSC2 = L("Common.Const.CharacterStateConst")

			if CSC2 and pawn.eModel and CONST.COMPONENT_INDEX_CHARACETER_CONTROLLER then
				okIdle = pcall(function()
					pawn.eModel:ForceChangeToState(CONST.COMPONENT_INDEX_CHARACETER_CONTROLLER, CSC2.IDLE)
				end)
			end
		end

		-- ★打改前->改后, 这样实机一眼能看出状态到底踢动了没有
		MLOG("idle", "forceChangeToIdle ok=%s state %s -> %s", tostring(okIdle), before, tostring(pawn.characterState))
	end

	-- ③再本地写位置
	-- ☠注意 forceSetPosRot 的第三个参数 instant 只喂给 setRotation
	--   (@...ClientAoiComponent.lua:527-535), 对【位置】是死参数 —— 别指望它让位置"立即生效"。
	local moved = false

	if rot and type(pawn.forceSetPosRot) == "function" then
		moved = pcall(pawn.forceSetPosRot, pawn, target, rot, true)
	end

	if not moved and type(pawn.forceSetPos) == "function" then
		moved = pcall(pawn.forceSetPos, pawn, target, false)
	end

	if not moved then
		MLOG("tp", "FAIL no mover on pawn (forceSetPosRot/forceSetPos both missing)")

		return false
	end

	-- ④强制这一帧就把新位置传上去。游戏自己在击飞/技能位移时也这么干
	--   (@Common_Ability_SkillState_KnockUpState.lua:200 / @GameApp_Ability_ClientCombatAction.lua:1421)
	if p.world and p.world.setIsSyncPosThisFrame and pawn.actorId then
		pcall(p.world.setIsSyncPosThisFrame, pawn.actorId, true)
	end

	-- ④-b 可选浮空: 关重力 + 钉住不下落。默认关(见开关处说明)。
	if MOD.jumpToChestFloat and type(pawn.forceUngrounded) == "function" then
		local reason = CONST.ForceUnGroundReasons and CONST.ForceUnGroundReasons.Ability

		-- 先清一次再开, 避免上一次没清干净时计数错位
		if _floatLeft > 0 and _floatPawn then
			pcall(_floatPawn.forceUngrounded, _floatPawn, false, reason)
		end

		if pcall(pawn.forceUngrounded, pawn, true, reason) then
			_floatPawn = pawn
			_floatLeft = type(MOD.jumpToChestFloatSec) == "number" and MOD.jumpToChestFloatSec or 1.5

			MLOG("float", "on %.1fs (reason=%s)", _floatLeft, tostring(reason))
		else
			MLOG("float", "FAIL forceUngrounded 调不通")
		end
	end

	-- ⑤开箱不在这里做。刚落地那一刻【交互 unit 还不存在】—— 它是靠近才创建的,
	--   这正是上一版的根因。到位之后游戏会自己建 unit, 然后由 armAutoInteract(0.3 秒一次)
	--   标上 actWhenArrive, 再由游戏自己的 InteractionSystem 开箱(带全套 canInteractive 门)。
	-- ★复查要记【出发点】: 只有拿它当基准才能分清"被弹回原地"和"落地滑了两步"。
	--   上一版只记目标点, 结果 14034 明明开箱成功却被标成 SNAPPED-BACK —— 误报。
	_tpWatch = {
		tx,
		ty,
		tz,
		tostring(ent.templateId),
		mx,
		myPos.y,
		mz,
		dist or -1
	}

	-- ★排一次强制开箱。不在这里立刻开 —— 刚落地那一帧 C# 的触发体积还没注册上,
	--   而且我们是从 2.5 米高掉下来的, 得让人先落地。由 0.3 秒的快定时器接手。
	if MOD.forceOpenChest then
		_forceOpen = {
			ent = ent,
			kind = kind,
			tries = 0,
			sent = false
		}
	end

	-- ★★日志必须带上【落点离箱子多远】—— 用户要调的就是这个数, 不打出来等于调了看不见效果。
	--   顺手带上游戏配表自己的值当对照, 和 [tpcheck] 的 kept/SNAPPED-BACK 配起来一轮就能定。
	MLOG("tp", "jump->chest tmpl=%s 出发距离=%.1fm 落点离箱子=%.2fm%s (游戏配表=%s) target=(%.1f,%.1f,%.1f) forbidOk=%s %s", tostring(ent.templateId), dist or -1, stand, standNote, cfgStand, tx, ty, tz, tostring(okF), tostring(errF or ""))

	return true
end

---------------------------------------------------------------------
-- ★★强制开箱: 绕过 canInteractive 那一整套【客户端】门, 直接调游戏自己的 startInteract。
--
-- 为什么有用: canInteractive(@GameApp_Interaction_InteractionUnitBase.lua:65-145)里
--   有一堆纯客户端的门 —— 交互角度 checkInteractAngle(:481) / 前向角度 / 空中阻挡 /
--   交互阻挡 / conditionId / 蛋形态白名单 / pawn 状态 / checkPrototypeId(要特定宠物形态),
--   还有 ClientChest:checkCanInteract(@...ClientChest.lua:209)里的 levelCondition 和
--   attaching()。这些门服务器不一定再查 —— 客户端不发包, 服务器根本没机会同意。
--
-- ★但【服务器在意的那些它会自己再查一遍】, 绕不过去。而这恰恰是这条最大的价值:
--   startInteract 的回调能拿到服务器的返回码(@...ClientInteractComponent.lua:80 的 cb),
--   一眼看出被哪道门拒了:
--     1026   ERROR_INVALID_DISTANCE            距离不够 -> 瞬移没到位
--     10022  CHEST_INTERACT_COUNT_LIMIT        开启次数上限(这箱子你开够了)
--     10024  INTERACT_COND_CHECK_FAILED        前置条件不满足(任务/剧情没到)
--     200143 CHEST_INTERACT_FAILED_OWNER_FARAWAY 归属者太远
--     103005 INTERACT_ACTION_LOCKED            动作被锁
--     1014   ERROR_INTERACT_EXEC_FAIL          交互执行失败
--   -> "打不开"从此不再是黑盒, 日志直接给出原因。
--
-- ☠消耗型宝箱走不到这里: chestEntOk 在【上游】就按 ent.needItem 挡掉了, 所以强开
--   不会抽体力。这一点很重要 —— startInteract 是绕过成本闸门的
--   (成本检查在 unit 上: @GameApp_Interaction_InteractUnit_InteractionUnitEntFunc.lua
--    checkNeedItemValid), 所以【只能靠上游过滤来保证安全】。
-- ☠不每个 tick 都发: 第 2 个 tick(约 0.6 秒)才发第一次 —— 要给人从 2.5 米高落地、
--   给 C# 触发体积注册留时间。收到回调就收工, 只有 1026(距离)才再试一次。
---------------------------------------------------------------------

local NOTICE_NAME = {
	-- ★★★23:09 实机就是这个: 落点在箱子头顶 -> 位置不通过 -> 人被弹回原地
	[1002] = "ERROR_POSITION_VALID 位置不通过(落点不是合法站位 -> 会被弹回原地)",
	[1001] = "ERROR_SCENE_VALID 场景不对",
	[1003] = "ERROR_INVALID_STATUS 状态不对(可能在下落/动作中)",
	[1007] = "ERROR_IN_COOLDOWN 冷却中",
	[1008] = "ERROR_ENTITY_NOT_EXIST 实体不存在",
	[1010] = "ERROR_INVALID_TARGET 目标无效",
	[1013] = "ERROR_EXEC_COST 消耗不够",
	[1019] = "ERROR_CHECK_COND_FAILED 条件检查失败",
	[1014] = "ERROR_INTERACT_EXEC_FAIL 交互执行失败",
	[1026] = "ERROR_INVALID_DISTANCE 距离不够(瞬移没到位)",
	[10022] = "CHEST_INTERACT_COUNT_LIMIT 开启次数上限",
	[10024] = "INTERACT_COND_CHECK_FAILED 前置条件不满足",
	[60001] = "SANDBOX_INTERACT_FAILED_OWNER_LEAVE 沙盒主人离开",
	[60002] = "SANDBOX_INTERACT_FAILED_GUEST 访客无权限",
	[103005] = "INTERACT_ACTION_LOCKED 动作被锁",
	[200017] = "INTERACT_ERROR_PLAYER",
	[200143] = "CHEST_INTERACT_FAILED_OWNER_FARAWAY 归属者太远"
}

-- 浮空倒计时。dt = 调用方自己的间隔(快定时器 0.3, 主 tick 1) —— 这样无论哪个定时器在跑,
-- 扣的秒数都是对的; 两个都在跑只会让浮空【早一点】结束, 那是安全的方向。
-- ☠必须清: forceUngrounded 是锁存标志 + 关重力, 不配对清会永久不着地。
local function tickFloat(dt)
	if _floatLeft <= 0 then
		return
	end

	_floatLeft = _floatLeft - (type(dt) == "number" and dt or 0.3)

	if _floatLeft > 0 then
		return
	end

	_floatLeft = 0

	local pawn = _floatPawn

	_floatPawn = nil

	if type(pawn) ~= "table" or type(pawn.forceUngrounded) ~= "function" then
		return
	end

	local CONST = L("Common.Const.Const")
	local reason = CONST and CONST.ForceUnGroundReasons and CONST.ForceUnGroundReasons.Ability
	local ok = pcall(pawn.forceUngrounded, pawn, false, reason)

	MLOG("float", "off (cleared=%s)", tostring(ok))
end

local function tickForceOpen()
	if _forceOpen == nil then
		return
	end

	if not (AN_6 and MOD.forceOpenChest and envOk()) then
		_forceOpen = nil

		return
	end

	local f = _forceOpen
	local ent = f.ent

	if type(ent) ~= "table" then
		_forceOpen = nil

		return
	end

	f.tries = f.tries + 1

	if f.tries > (type(MOD.forceOpenTries) == "number" and MOD.forceOpenTries or 6) then
		MLOG("force", "give up tmpl=%s (%d tries)", tostring(ent.templateId), f.tries - 1)

		_forceOpen = nil

		return
	end

	-- 正常流程(actWhenArrive)已经开掉了就不用强开
	local openedFn = ent.isChestOpened or ent.isCollectItemOpened

	if type(openedFn) == "function" then
		local ok, opened = pcall(openedFn, ent)

		if ok and opened then
			MLOG("force", "tmpl=%s 已被正常流程开掉, 不强开", tostring(ent.templateId))

			_forceOpen = nil

			return
		end
	end

	-- 第 1 个 tick 只等落地, 不发包
	if f.tries < 2 or f.sent then
		return
	end

	local p = rawget(_G, "pg")
	local CONST = L("Common.Const.Const")

	if not p or type(p.me) ~= "table" or type(p.me.startInteract) ~= "function" or type(CONST) ~= "table" then
		MLOG("force", "bail: 没有 pg.me:startInteract 或 Const")

		_forceOpen = nil

		return
	end

	local iact = f.kind == "collect" and CONST.IACT_IP_COLLECT_ITEM or f.kind == "envobj" and CONST.IACT_TP_ENVOBJ or CONST.IACT_TP_CHEST

	if type(iact) ~= "number" then
		MLOG("force", "bail: 拿不到 IACT 类型 (kind=%s)", tostring(f.kind))

		_forceOpen = nil

		return
	end

	f.sent = true

	local tmpl = tostring(ent.templateId)
	local okCall, errCall = pcall(p.me.startInteract, p.me, iact, ent.id, ent.actionPrototypeId, {}, function(ret, retArgs)
		local ND = L("Common.NoticeDef")
		local success = ND and ND.SUCCESS

		if success ~= nil and ret == success then
			MLOG("force", "✅ tmpl=%s 强制开箱【成功】", tmpl)

			_forceOpen = nil

			return
		end

		MLOG("force", "☠ tmpl=%s 服务器拒了: ret=%s %s", tmpl, tostring(ret), NOTICE_NAME[ret] or "(未知码, 去 @Common_NoticeDef.lua 里搜这个数字)")

		-- 只有"距离不够"值得再试一次(可能还没落地); 其它都是服务器的硬条件, 再试没意义
		if ret == 1026 and _forceOpen == f then
			f.sent = false
		else
			_forceOpen = nil
		end
	end)

	MLOG("force", "send tmpl=%s kind=%s iact=%s try=%d call=%s %s", tmpl, tostring(f.kind), tostring(iact), f.tries, tostring(okCall), tostring(errCall or ""))

	if not okCall then
		_forceOpen = nil
	end
end

-- 落点复查: 瞬移后下一个 tick 回读实际位置。这是判断"服务器有没有把我拉回去"的判据。
local function tpVerify()
	if _tpWatch == nil then
		return
	end

	local w = _tpWatch

	_tpWatch = nil

	local p = rawget(_G, "pg")
	local pawn = p and (p.pawn or p.me)

	if type(pawn) ~= "table" or type(pawn.getPosition) ~= "function" then
		return
	end

	local ok, pos = pcall(pawn.getPosition, pawn)

	if not ok or type(pos) ~= "table" or type(pos.x) ~= "number" then
		return
	end

	-- ☠☠上一版这里只有"离目标多远"一个数, 结果 14034 明明强制开箱成功了, 却因为
	--   偏移 6.2 米被标成 SNAPPED-BACK —— 误报, 而且会把下一轮的判断带偏。
	-- ★现在拿【出发点】当基准, 三个数一起打, 才能分清三种情况:
	--   到位     : 离目标近
	--   弹回原地 : 离目标还是原来那么远, 而且【几乎没离开出发点】<= 23:09 就是这个
	--   滑走了   : 离开了出发点, 但也没到目标(落地滑坡/被挤开)
	local remainH = math.sqrt((pos.x - w[1]) ^ 2 + (pos.z - w[3]) ^ 2)
	local movedH = math.sqrt((pos.x - w[5]) ^ 2 + (pos.z - w[7]) ^ 2)
	local intendedH = math.sqrt((w[1] - w[5]) ^ 2 + (w[3] - w[7]) ^ 2)
	local drop = w[6] - pos.y
	local verdict

	if remainH < 3 then
		verdict = "到位 kept"
	elseif movedH < 2 then
		verdict = "☠弹回原地 SNAPPED-BACK(落点不是合法站位?)"
	else
		verdict = "△滑走了 DRIFTED"
	end

	MLOG("tpcheck", "%s tmpl=%s 打算走%.1fm 实际走%.1fm 还差%.1fm 下落%.1fm", verdict, tostring(w[4]), intendedH, movedH, remainH, drop)
end

-- ★★开关变化监视 + 自愈。每秒一次。
-- ☠为什么必须有这个: 开关是 native 写进来的, 如果那条链路没接通, 表现是"点了没反应",
--   而"没反应"和"功能本身失效"在日志里长得一模一样。有了这行就能一眼分清。
-- ☠自愈那一半同样重要: 万一有谁把 AN_n 写成 nil, key 就又"不存在"了,
--   下一次 native 写入会重新被严格 _G 吞掉 —— 而且是静默的。
local function tickSwitches()
	local changed = nil

	for i = 1, #SWITCH_NAMES do
		local n = SWITCH_NAMES[i]
		local v = rawget(_G, n)

		if v == nil then
			pcall(rawset, _G, n, false)

			v = false

			MLOG("sw", "☠%s 变成 nil 了, 已补回 false —— 否则下一次原生写入会被严格 _G 静默吞掉", n)
		end

		if _swLast[n] ~= v then
			changed = changed or {}
			changed[#changed + 1] = n .. "(" .. tostring(SWITCH_DESC[n]) .. ") " .. tostring(_swLast[n]) .. " -> " .. tostring(v)
			_swLast[n] = v
		end
	end

	if changed then
		MLOG("sw", "★开关变了: %s", table.concat(changed, " | "))
	end
end

local function patchAllServerMsg(tag)
	local acc = {
		n = 0,
		names = {}
	}

	for _, mod in pairs(package.loaded) do
		if type(mod) == "table" and type(rawget(mod, "typeName")) == "string" then
			pcall(_probeAndWrap, mod, acc)
		end
	end

	if acc.n > 0 then
		MLOG(tag, "wrapped serverMsg on %d class(es): %s ...", acc.n, table.concat(acc.names, ","))
	end

	return acc.n
end

---------------------------------------------------------------------
-- 安装
---------------------------------------------------------------------

local _installed = false
local _tick
-- 0.3 秒的快定时器只跑这一个: 给宝箱 unit 打 actWhenArrive
local _armTick

local function install()
	if _installed then
		return true
	end

	local pg = rawget(_G, "pg")

	if not pg or not pg.me or not pg.global then
		return false
	end

	local ACA = L("Common.Ability.Attribute.ActorCombatAttribute")
	local ATTR = L("Common.Const.AttributeConst")
	local CONST = L("Common.Const.Const")
	local PC = L("GameApp.Controller.PawnController")
	local IC = L("GameApp.Input.InputCommand")
	local CMP = L("Entities.SpaceEntities.ClientMainPlayer")

	if not (ACA and ATTR and CONST and PC and IC and CMP) then
		MLOG("install", "wait modules ACA=%s ATTR=%s CONST=%s PC=%s IC=%s CMP=%s", tostring(ACA ~= nil), tostring(ATTR ~= nil), tostring(CONST ~= nil), tostring(PC ~= nil), tostring(IC ~= nil), tostring(CMP ~= nil))

		return false
	end

	MLOG("install", "start, actorId=%s", tostring(pg.me.actorId))
	-- ★★★开关声明结果。这一行是【开关链路通没通】的第一判据:
	--   全是 =false  -> 声明成功, 等 native 写入(点开关后应该看到 [sw] ★开关变了)
	--   出现 =nil    -> 声明失败, 后面 native 写入会被严格 _G 吞掉, 开关永远打不开
	MLOG("sw", "开关声明(用 %s): %s", tostring(_swHow), tostring(_swInit))

	-----------------------------------------------------------------
	-- ④ 移速 xN
	-- 咽喉 = ActorCombatAttribute:getAttribRatioValue(speed_ratio_load_adjustment_v)
	--        (@Common_Ability_Attribute_ActorCombatAttribute.lua:576)
	-- 这个 id 在全部 11 个 *SpeedRatio getter 和 6 个绝对速度 getter 的公式里
	-- 【各出现恰好一次】(同文件 :335~531), 所以乘一次 = 精确 xN, 走/跑/冲刺/游泳/
	-- 滑翔/冲刺闪避/蹲伏 全覆盖。
	-- ☠ 不要按 speed_ratio_run_v / dash_v 做白名单: 宠物冲刺是
	--    ratio(sprint)*ratio(load)*ratio(dash), 那样会叠成 xN^2。
	-- 只改 getter 返回值不写属性 -> 不过 checkAttributeValue 的 min/max 钳制,
	-- 也不污染服务器同步下来的 entity.baseAttr(= 改行为不改上报)。
	-- 玩家的速度是 C# MotionComponent 通过 ClientMotionComponent:getMotionAttr
	-- (@...ClientMotionComponent.lua:69, 全库零 Lua 调用方 = C# 入口)拉过去的。
	-----------------------------------------------------------------
	local LOAD_ADJ = ATTR.speed_ratio_load_adjustment_v

	patchLite(ACA, "getAttribRatioValue", function(orig)
		return function(self, attributeId)
			local v = orig(self, attributeId)

			if AN_3 and MOD.speedMul and MOD.speedMul ~= 1 and attributeId == LOAD_ADJ and isMine(self.entity) and envOk() then
				MLOG("spd", "loadAdj before=%s after=%s", tostring(v), tostring(v * MOD.speedMul))

				return v * MOD.speedMul
			end

			return v
		end
	end, "spd")

	-----------------------------------------------------------------
	-- ⑤ 无限体力
	-- 咽喉 = ActorCombatAttribute:changeStamina(value)
	--        (@Common_Ability_Attribute_ActorCombatAttribute.lua:568)
	-- ★体力是【全作唯一零 RPC、纯客户端】的数值: 全库没有任何
	--   RPC_CS_*Stamina / RPC_SC_*Stamina, 而且 stamina_cur / add_cur_stamina_v /
	--   add_cur_stamina_max_p 正是 OnlyClientModifyAttributes 里仅有的三个
	--   (@Common_Ability_Attribute_ActorAttributeModifier.lua:21-25) —— 客户端说了算。
	--   所以这一条是五个功能里唯一【零服务器风险】的。
	-- 全部体力变动都过这一个函数(已 grep 全库确认):
	--   ClientStaminaComponent:costStamina -> changeStamina(-cost)          (:488 跑/冲刺/攀爬/滑翔/游泳)
	--   ClientCombatAction 的体力消耗节点  -> changeStamina(-value)          (@GameApp_Ability_ClientCombatAction.lua:7971 技能体力)
	--   ClientStaminaComponent:recoverStamina / processAddCurStamina*        (正数, 不拦)
	--   ClientStaminaComponent:restoreLockedStamina                          (delta 可负, 见下)
	-- ☠ 不要用游戏自带的 gmMode = Const.NO_COST_MODE(=1): 那三处体力判断确实认它
	--    (:331/:362/:454), 但它是【整个 GM 总开关】, 还会连带
	--    @Common_Components_AbilityComponent.lua:821 技能免费 / @CustomTypes_Ability.lua:222 /
	--    @...ClientWaterStorageComponent.lua:228 储水免费 /
	--    @...ClientPetsComponent.lua:1933/1961 无视等级解锁宠物控制,
	--    而且 gmMode 是 @Config_Config.lua:13849 的【服务器同步属性】-> 客户端单方面置 1
	--    会和服务器认知不一致(典型的"改完反而更糟")。
	-- ☠ restoringLockedStamina 为真时要放行负值: 那是"锁定体力"机制在把体力还原回锁定值
	--    (@...ClientStaminaComponent.lua:122-143), 拦了会让它以为自己还原过了。
	-- 只拦消耗不拦恢复 -> 体力就是真的满的, 下游 checkStaminaCost(:330) /
	-- checkEndofStamina(:361) / AbilityComponent 的 checkStamina 门槛(:758) 全都自然通过,
	-- 不需要再去改任何判断(= 改行为让数据自然正常)。
	-----------------------------------------------------------------
	patchLite(ACA, "changeStamina", function(orig)
		return function(self, value)
			if AN_5 and MOD.infStamina and envOk() and type(value) == "number" and value < 0 and isMine(self.entity) and self.entity.restoringLockedStamina ~= true then
				MLOG("stam", "block cost %s", tostring(value))

				return
			end

			return orig(self, value)
		end
	end, "stam")

	-- 装上之后把体力补满一次(走游戏自己的正向通道), 免得停在半格
	local function topUpStamina()
		if not (AN_5 and MOD.infStamina and envOk()) then
			return
		end

		local a = pg.me and pg.me.actorCombatAttribute

		if not a then
			return
		end

		local cur = a:getRawAttribValue(ATTR.stamina_cur)
		local max = a:getRawAttribValue(ATTR.stamina_max_cur)

		if type(cur) == "number" and type(max) == "number" and max > 0 and cur < max then
			MLOG("stam", "topup %s -> %s", tostring(cur), tostring(max))
			a:changeStamina(max - cur)
		end
	end

	-- C# 侧的速度是缓存的(靠属性变化通知刷新), 改完必须主动让它重新拉一次
	local function refreshSpeed()
		local me = pg.pawn or pg.me

		if not me then
			return
		end

		if me.onMotionAttrChange then
			pcall(function()
				me:onMotionAttrChange()
			end)
		end

		local AICU = L("Common.Utils.AIControllerUtils")

		if AICU and AICU.refreshAllSpeed then
			pcall(AICU.refreshAllSpeed, me)
		end
	end

	-----------------------------------------------------------------
	-- ③ 无限跳跃
	-- 咽喉 = PawnController:performJump(canJump, inFly)
	--        (@GameApp_Controller_PawnController.lua:193, 全库唯一的跳跃出口:
	--         键盘/手柄/手机跳跃按钮/游戏自己的高跳 combat action 全走这里)
	-- ☠ 光把 canJump 改成 true 没用: @Data_state_conflict_data.lua:4972 的 JUMP.block
	--    列表 40 项里【没有任何"在空中"项】, 说明拦二段跳的不是 Lua 状态表, 是 C# 的
	--    CharacterController。所以在空中要【自己抬】, 用游戏自带原语 playRaiseUp
	--    (= eModel:RaiseUp + eModel:ForceNotGroundCheck, @...ClientMotionComponent.lua:176)。
	-- ☠ 不硬改 canJump: 那会吃掉 :197 的 elseif inFly -> InputCommand.StopFly, 飞行停不下来。
	-- ☠ self 可能是 ControllerSystem(pg.game.controller 带 __index 代理), 一律用 self.pawn。
	-- ☠ 不要用 forceUngrounded(true) 代替: 那是 AlwaysForceUnGround 锁存标志, 不配对清会永久不着地。
	-- ☠ LiteClass 继承会把父类方法 rawset 缓存进子类 -> PlayerController 也要写一遍。
	-----------------------------------------------------------------
	local COMP_CC = CONST.COMPONENT_INDEX_CHARACETER_CONTROLLER
	local CSC = L("Common.Const.CharacterStateConst")

	local function airborne(pawn)
		local ok, g = pcall(function()
			return pawn.eModel.IsOnGround
		end)

		if ok and type(g) == "boolean" then
			return not g
		end

		if CSC and CSC.isChildOfState and CSC.AIRING then
			local ok2, r = pcall(CSC.isChildOfState, pawn.characterState, CSC.AIRING)

			if ok2 then
				return r == true
			end
		end

		return false
	end

	local jumpWrapper = patchLite(PC, "performJump", function(orig)
		return function(self, canJump, inFly)
			-- ★★★无条件记录【跳跃键进来了】, 前 8 次。
			-- ☠2026-09-30 01:14 实机的教训: 用户反馈功能没反应, 而日志里【一条相关的都没有】——
			--   下游每一条路径都有诊断日志, 所以"什么都没有"只能说明 performJump 根本没被调到,
			--   可是我的日志【说不出这件事】, 只能靠猜, 白掉一轮实机。
			-- 通用铁律: 一条链路的【最外层入口】必须有无条件日志, 否则"什么都没发生"是个哑谜。
			-- ★01:24 那一轮就是靠这行才看清: 用户【连按了两次】跳跃, 第二次才走到瞬移。
			-- ★★02:19 那一轮又证明了一次它的价值, 而且暴露了它当时还不够:
			--   用户反馈"跳跃不瞬移了", 真相是 AN_6 那一轮【一次都没被打开过】
			--   (开的是 AN_4/AN_5/AN_3/AN_1/AN_2, 唯独漏了 AN_6) —— AN_4 开着 + AN_6 关着
			--   = 按跳跃走无限跳, 当然不瞬移。代码一点问题都没有。
			--   ☠但当时这行只打 AN_4 / AN_6 两个, 得回翻 [sw] 历史才能确认别的开关,
			--   所以现在【六个全打】: "我以为开了"这种情况从此一行就能自证, 不用再问一轮。
			local _pw = self and self.pawn

			MLOG("jumpin", "按到跳跃 canJump=%s inFly=%s 开关[AN_1伤害=%s AN_2无敌=%s AN_3移速=%s AN_4跳跃=%s AN_5体力=%s AN_6(瞬移)=%s] pawn=%s isMine=%s", tostring(canJump), tostring(inFly), tostring(AN_1), tostring(AN_2), tostring(AN_3), tostring(AN_4), tostring(AN_5), tostring(AN_6), tostring(_pw and (rawget(_pw, "className") or _pw.className) or "nil"), tostring(_pw ~= nil and isMine(_pw)))

			-- ⑦跳跃瞬移: 有宝箱就瞬移过去并标成自动开, 然后【不跳】(return, 不调 orig);
			--   没有宝箱就一路落到下面, 行为和之前【完全一样】(正常跳跃 + 无限跳)。
			--   ☠这条必须放在 infJump 之前: 否则在空中按跳会先被无限跳吃掉, 永远轮不到瞬移。
			--   ☠inFly 时不接管: performJump 的 elseif inFly 分支是 StopFly(停止飞行),
			--     抢掉它会让飞行停不下来。
			if AN_6 and MOD.jumpToChest and not inFly and envOk() then
				local pawn = self and self.pawn

				if pawn and isMine(pawn) then
					local okTp, done = pcall(tpToChest, pawn)

					if okTp and done then
						return
					end

					if not okTp then
						MLOG("tp", "ERROR %s", tostring(done))
					end
				end
			end

			if AN_4 and MOD.infJump and not inFly and envOk() then
				local pawn = self and self.pawn

				if pawn and isMine(pawn) and pawn.eModel and airborne(pawn) then
					MLOG("jump", "air jump (canJump=%s)", tostring(canJump))

					local ctrl = pg.game and pg.game.controller

					if ctrl and ctrl.nextSkillAction then
						pcall(function()
							ctrl.nextSkillAction:clear()
						end)
					end

					if pawn.resetMotorTempState then
						pcall(function()
							pawn:resetMotorTempState()
						end)
					end

					if pawn.playRaiseUp then
						pcall(function()
							pawn:playRaiseUp(MOD.jumpRiseDist, MOD.jumpRiseTime, MOD.jumpUnGround)
						end)
					end

					pcall(function()
						pawn.eModel:SetInputCommand(COMP_CC, IC.Jump)
					end)

					return
				end
			end

			return orig(self, canJump, inFly)
		end
	end, "jump")

	local PLC = L("GameApp.Controller.PlayerController")

	if jumpWrapper and PLC and rawget(PLC, "performJump") ~= jumpWrapper then
		local ok, err = pcall(function()
			PLC.performJump = jumpWrapper
		end)

		MLOG("jump", "patch PlayerController.performJump = %s %s", tostring(ok), tostring(err or ""))
	end

	-----------------------------------------------------------------
	-- ② 玩家无敌
	-- (a) 函数级咽喉: Utils.checkValidTarget(@Common_Utils_Utils.lua:772)
	--     —— 近战多目标 / sweep / 抛射物三条命中链的共用闸门, 返回 false 时既不执行
	--     doActions 也不发上报 RPC(@Common_Ability_MultiTargetsInfo.lua:131 在 :222 之前)。
	--     ☠ 要带敌我判据, 否则队友治疗/自身 buff 也会被跳过。
	--     ☠ Common.Utils.Utils 和 Utils.Utils 是同一份源码的两个 package.loaded 实例, 两个都补。
	-- (b) 数据级兜底: dialoguePauseInvincibleEndTs 是游戏自己的"对话暂停无敌"字段,
	--     全库只有 checkValidTarget 读它(:797-802)、没有任何 setter、也不是 @Config_Config.lua
	--     里的同步属性 -> 写进去不会被服务器覆盖。换宠/换场景会换实体, 所以每秒重刷。
	-- (c) 摔落伤害是完全独立的客户端自残链路(客户端不发服务器就不扣), 借游戏自带的场景配置
	--     开关 ClientSpace:checkImmuneFallDamage(@Entities_SpaceEntities_ClientSpace.lua:1152)
	--     免疫 —— 返回 true 只是让当前场景表现成"免摔落伤场景", 是合法状态。
	-- (d) 溺水/濒死走 RPC_CS_SpecialDamage, 在 HURT_IN 里无条件丢。
	-- (e) ★★★陷阱(荆棘/尖刺/导电)走【第三条完全独立的自伤通道】, 见 VICTIM_REPORT 上方说明:
	--     RPC_CS_OnThornsCollision / RPC_CS_OnConductHit, 由【受害者自己】发出,
	--     全程不经过 checkValidTarget -> (a) 那个咽喉对它零作用。
	--     2026-09-29 22:38 实机就是这么暴露的: "怪物打我不掉血了 但是有些陷阱会掉血"。
	--     ★通用教训: "无敌"不是一个咽喉能包住的 —— 得按【伤害来源分类】逐条清:
	--     怪打我 / 我自残(摔落溺水) / 环境陷阱 / 受击 timeline 上报, 四类各走各的路。
	-----------------------------------------------------------------
	local function hookCheckValidTarget(U, key)
		if type(U) ~= "table" or type(U.checkValidTarget) ~= "function" then
			return
		end

		local orig = U.checkValidTarget

		U.checkValidTarget = function(target, owner)
			if AN_2 and MOD.invin and envOk() and owner ~= nil and isMine(target) and not isMine(owner) then
				local isEnemy = true
				local ok, r = pcall(U.isEnemy, owner, target)

				if ok then
					isEnemy = r == true
				end

				if isEnemy then
					MLOG("invin", "block hit on me, owner=%s", tostring(owner.actorId))

					return false
				end
			end

			return orig(target, owner)
		end

		MLOG("invin", "hooked %s.checkValidTarget", key)
	end

	-- ☠☠☠【2026-09-30 01:24 实机: RPC_SC_* 处理器包不上, 已放弃】
	--   本来想包 ClientMainPlayer:RPC_SC_NotifyResetPositionInfo(@...ClientMainPlayer.lua:2066)
	--   当"服务器把我位置重置了"的免费探针, 但实机两轮都是
	--     [resetpos] FAIL RPC_SC_NotifyResetPositionInfo: not a function (table)
	--   根因: 框架把 Config 里的 RPC 参数类型表(@Config_Config.lua:11722 = {"int","int","int"})
	--   按【同名 key】塞进了这个类, 而且【类表和 vtbl 里都是那张表】—— 加了 vtbl 退回也拿不到函数
	--   (退回逻辑留在 patchEntityClass 里, 那是通用修正, 对别的方法仍然有用)。
	--   ==> 说明这游戏的 RPC_SC 处理器不是靠实体方法查找分发的, 想包它得先找到真正的分发点。
	--   暂时不做: 判断瞬移有没有被弹回, 用 [tpcheck] 那三个数已经够了。
	

	local U1 = L("Common.Utils.Utils")
	local U2 = L("Utils.Utils")

	hookCheckValidTarget(U1, "Common.Utils.Utils")

	if U2 and U2 ~= U1 then
		hookCheckValidTarget(U2, "Utils.Utils")
	end

	patchEntityClass(L("Entities.SpaceEntities.ClientSpace"), "checkImmuneFallDamage", function(orig)
		return function(self)
			if AN_2 and MOD.invin and envOk() then
				return true
			end

			return orig(self)
		end
	end, "invin-fall")

	local function markInvincible()
		local v = AN_2 and MOD.invin and envOk() and 4e12 or 0
		local list = {
			pg.me,
			pg.pawn
		}

		if pg.me and pg.me.getCurPetEntity then
			local ok, pet = pcall(pg.me.getCurPetEntity, pg.me)

			if ok and type(pet) == "table" then
				list[#list + 1] = pet
			end
		end

		for i = 1, #list do
			local e = list[i]

			if type(e) == "table" and e.dialoguePauseInvincibleEndTs ~= v then
				e.dialoguePauseInvincibleEndTs = v

				MLOG("invin-ts", "set on actorId=%s -> %s", tostring(e.actorId), tostring(v))
			end
		end
	end

	-----------------------------------------------------------------
	-- 只读探针(MOD.probe): 把 C# 实际向 Lua 要哪些速度项打进日志。
	-- getMotionAttr / getAttributeSpeedRatio 在全库【零 Lua 调用方】
	-- (@...ClientMotionComponent.lua:69 / :73), 所以它们就是 C# -> Lua 的入口。
	-- 这两个探针只打日志、原样返回, 不改任何值。
	-----------------------------------------------------------------
	if MOD.probe then
		patchEntityClass(CMP, "getMotionAttr", function(orig)
			return function(self, attributeProp)
				local v = orig(self, attributeProp)

				MLOG("probe:motionAttr:" .. tostring(attributeProp), "= %s", tostring(v))

				return v
			end
		end, "probe")
		patchEntityClass(CMP, "getAttributeSpeedRatio", function(orig)
			return function(self, attributeName)
				local v = orig(self, attributeName)

				MLOG("probe:speedRatio:" .. tostring(attributeName), "= %s", tostring(v))

				return v
			end
		end, "probe")
	end

	-----------------------------------------------------------------
	-- ①②的 RPC 层: 主角和宠物各包一处 serverMsg
	-- serverSpaceMsg / reliableServerSpaceMsg 最终也会落到 serverMsg, 但名字会被包成
	-- "RPC_CS_SpaceMethod" + rpcIndex(@...ClientDispatcherComponent.lua:490-498),
	-- 追踪日志里看不出真名 -> 额外给这两条加【只追踪不改行为】的包装, 保证日志不半残。
	-----------------------------------------------------------------
	if MOD.rpcTrace then
		local function traceOnly(label)
			return function(orig)
				return function(self, methodName, ...)
					if isMine(self) then
						MLOG("rpc:" .. label .. ":" .. tostring(methodName), "AMP=no(space)")
					end

					return orig(self, methodName, ...)
				end
			end
		end

		patchEntityClass(CMP, "serverSpaceMsg", traceOnly("space"), "rpc-trace")
		patchEntityClass(CMP, "reliableServerSpaceMsg", traceOnly("rspace"), "rpc-trace")
	end

	-- ★顺序很重要: 先给主角/宠物上【完整 wrapper】(= 2026-09-29 实机验证过技能百倍能生效的那版),
	--   再扫描其余实体类上【只读追踪】。_wrapMark 保证这两个类不会被扫描二次包装。
	local fullNames = {}

	local function fullWrap(cls, label)
		if type(cls) ~= "table" or type(cls.getVtbl) ~= "function" then
			return
		end

		local ok, vt = pcall(cls.getVtbl)

		if not ok or type(vt) ~= "table" then
			-- 退回 patchEntityClass(会读 __index 拿到继承来的那份, 再写进自己 vtbl)
			patchEntityClass(cls, "serverMsg", wrapServerMsg, label)

			return
		end

		local f = rawget(vt, "serverMsg") or cls.serverMsg

		if type(f) ~= "function" or _wrapMark[f] then
			return
		end

		local wrapped = wrapServerMsg(f)

		_wrapMark[wrapped] = true

		if pcall(function()
			vt.serverMsg = wrapped
		end) then
			fullNames[#fullNames + 1] = label
		end
	end

	-- ★上一轮「技能失效」的根因已由实机日志定位: 扫描版用 rawget(vt,"serverMsg") 取方法,
	--   【拿不到继承来的那一份】; ClientPet 的 vtbl 很可能没有自己那份 -> 被跳过 ->
	--   而宠物才是本作的主输出 -> 技能百倍失效。所以这里一律用
	--   rawget(vt,"serverMsg") or cls.serverMsg (后者走 __index 拿到继承的那份)。
	-- ★ClientPuppet / ClientCreation 留在名单里的理由已经变了: 普攻放大已删除, 它们不再参与放大。
	--   但【继续显式包装】是有意的 —— patchAllServerMsg 的扫描用 rawget(vt,"serverMsg"),
	--   拿不到继承来的那一份(上一轮 ClientPet 就是这么被漏掉的), 把它们从名单里拿掉会连
	--   【只读追踪】一起丢, 排障时野怪那一侧就成了盲区。现在它们走完整 wrapper 只是多几次
	--   表查找(放大分支 isMine=false 直接跳过), 开销可忽略, 换来的是覆盖面的确定性。
	for _, it in ipairs({
		{
			CMP,
			"ClientMainPlayer"
		},
		{
			L("Entities.SpaceEntities.ClientPet"),
			"ClientPet"
		},
		{
			L("Entities.SpaceEntities.ClientPuppet"),
			"ClientPuppet"
		},
		{
			L("Entities.SpaceEntities.ClientCreation"),
			"ClientCreation"
		}
	}) do
		fullWrap(it[1], it[2])
	end
	MLOG("install", "full-wrap(放大/丢弃): %s", table.concat(fullNames, ",") ~= "" and table.concat(fullNames, ",") or "NONE!")

	-----------------------------------------------------------------
	-- ①-b 投射物多段命中(见上面 patchProjectileMultiHit 的大段说明)
	-----------------------------------------------------------------
	local projDone = patchProjectileMultiHit()

	MLOG("install", "proj-multihit: %s (projHit=%s, ampProjectileHit=%s)", #projDone > 0 and table.concat(projDone, ",") or "NONE(类还没加载, 每秒补打)", tostring(MOD.projHit), tostring(MOD.ampProjectileHit))

	local scanned = patchAllServerMsg("rpc-scan")

	MLOG("install", "trace-only wrap on %d other class(es), wrapAllEntities=%s", scanned, tostring(MOD.wrapAllEntities))

	_armTick = function()
		pcall(armAutoInteract)
		-- ⑦-b 强制开箱: 瞬移落地后由这里发 startInteract, 并把服务器返回码打进日志
		pcall(tickForceOpen)
		-- ⑦-c 浮空倒计时(按自己的间隔 0.3 秒扣)
		pcall(tickFloat, 0.3)
	end

	_tick = function()
		-- ★★开关监视放最前: 变化和 nil 自愈都要比下面那些功能先看到
		pcall(tickSwitches)

		-- ★每秒清一次重发预算 -> 安全阀的实际语义是"每秒最多 maxResendPerCast 个重发包"
		_castResend = 0

		-- ★给最后一次施法收尾: 一次施法的 RPC 序列实测 ~0.45 秒跑完(20:34 日志 61.08->61.53),
		--   所以连续 2 个 tick 还没有下一次施法就可以收尾了。没有这一条的话,
		--   停手之后【最后那次施法的小结永远不打印】—— 而那往往正是要看的那一次。
		if _burstAbility ~= nil then
			_burstTicks = _burstTicks + 1

			if _burstTicks >= 2 then
				pcall(flushBurstSummary)
			end
		end

		pcall(markInvincible)
		pcall(refreshSpeed)
		pcall(topUpStamina)
		-- ⑥自动开宝箱: 只负责给 unit 打上 actWhenArrive 标记, 真正的开箱由游戏自己的
		--   InteractionSystem 每 0.3 秒 tick 去做(带 1 秒节流 + 全套 canInteractive 门)
		pcall(armAutoInteract)
		-- ⑦瞬移落点复查: 上一次瞬移的目标点 vs 现在的实际位置, 判断服务器有没有把我拉回去
		pcall(tpVerify)
		-- ⑦-b 强制开箱也在主 tick 兜一次(快定时器万一没注册上)
		pcall(tickForceOpen)
		-- ⑦-c 浮空倒计时兜底(按自己的间隔 1 秒扣)。★两个定时器都扣只会让浮空早点结束,
		--   那是安全方向 —— 宁可早落地, 不能永久不着地。
		pcall(tickFloat, 1)
		-- 补扫启动后才加载进来的实体类(已包过的会被 _wrapMark 跳过, 开销可忽略)
		pcall(patchAllServerMsg, "rpc-rescan")
		-- 投射物三个类可能比 install 晚加载 -> 每秒补打一次(打过的被 _projPatched 跳过)
		pcall(patchProjectileMultiHit)
	end

	_installed = true

	pcall(_tick)
	MLOG("install", "done")

	return true
end

---------------------------------------------------------------------
-- 启动探测器: 包 Time.getTickSecond(@Main.lua:40 每帧调)。
-- 它在本文件上面 Time.realSecondCache = Time.getTickSecond() 已经被调用过一次,
-- 所以包装必须放在那之后, 否则装的时候 pg 还不存在。
-- ★装完立刻自卸, 不在 Time.realSecondCache 的 360 个读点上留常驻包装。
-- 之后的每秒重刷改用 TimerManager(单位: 秒, 判据 @GameApp_DialogueGraph_DialogueItemCmd.lua:573
-- addRepeatTimer(DIALOGUE_PAUSE_NPC_INVINCIBLE_TIME - 1) 去续 4 秒无敌, 以及
-- @Core_Client_NetHandler.lua:794 addRepeatTimer(1, 心跳))。
---------------------------------------------------------------------
local _origGetTickSecond = Time.getTickSecond
local _lastTry = 0
local _lastTick = 0

Time.getTickSecond = function()
	local t = _origGetTickSecond()

	if not _installed then
		if t - _lastTry > 0.5 then
			_lastTry = t

			local ok, done = pcall(install)

			if not ok then
				MLOG("install", "ERROR %s", tostring(done))
			elseif done then
				-- 装好了就把每秒重刷交给 TimerManager, 然后自卸帧探测器
				local TM = L("Core.Timer.TimerManager")
				local armed = false

				if TM and TM.addRepeatTimer then
					local ok2, err2 = pcall(function()
						TM.addRepeatTimer(1, _tick)
					end)

					armed = ok2

					MLOG("install", "repeat timer armed=%s %s", tostring(ok2), tostring(err2 or ""))

					-- ★额外一个 0.3 秒的快定时器, 只干"给宝箱 unit 打 actWhenArrive"这一件事。
					--   理由: 瞬移落地后 unit 才被创建, 靠 1 秒的主 tick 去标记会让开箱慢一拍;
					--   0.3 秒正好对上游戏 InteractionSystem 自己的周期
					--   (@GameApp_Core_GameApp.lua:399-401 addSystem(self.interaction, 0.3, true))。
					--   小数间隔是支持的(游戏自己就有 addRepeatTimer(0.05/0.1) 的先例)。
					local ok3, err3 = pcall(function()
						TM.addRepeatTimer(0.3, function()
							pcall(_armTick)
						end)
					end)

					MLOG("install", "fast arm timer(0.3s)=%s %s", tostring(ok3), tostring(err3 or ""))
				end

				if armed then
					Time.getTickSecond = _origGetTickSecond
					MLOG("install", "frame probe uninstalled")
				else
					-- 拿不到 TimerManager 时的回退: 继续用帧探测器每秒跑一次(见下面 elseif)
					MLOG("install", "fallback to frame tick")
				end
			end
		end
	elseif t - _lastTick > 1 then
		_lastTick = t

		if _tick then
			pcall(_tick)
		end
	end

	return t
end

-- ========================= IMO MOD END =========================

return Time