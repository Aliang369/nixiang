# 跨游戏状态索引

> 更新：2026-09-20。此文件只保留摘要；长状态见 `games/{游戏}.md`。
> 读取时先定位目标游戏，只加载对应摘要，不默认通读全部记录。

## 游戏索引

| 游戏 | 最近更新 | 状态 | 详情 |
|---|---|---|---|
| 我的勇者 | 2026-09-20 | 已收尾 | [摘要](games/我的勇者.md) |
| 蛋壳战机 | 2026-09-20 | 已收尾 | [摘要](games/蛋壳战机.md) |
| 正中靶心2 | 2026-09-20 | 已交付/待核验 | [摘要](games/正中靶心2.md) |
| 暗影格斗3 | 2026-09-20 | 已交付/待核验 | [摘要](games/暗影格斗3.md) |
| 宝石战争 | 2026-09-20 | 已收尾 | [摘要](games/宝石战争.md) |
| 动物大联盟 | 2026-09-21 | 已终止并清理（保留 10_docs） | [摘要](games/动物大联盟.md) |
| 一念通天 | 2026-09-20 | 已收尾 | [摘要](games/一念通天.md) |
| 明日方舟 | 2026-09-19 | 已收尾 | [摘要](games/明日方舟.md) |
| 弓箭传说2 | 2026-09-19 | 已收尾 | [摘要](games/弓箭传说2.md) |
| 燃烧纪元 | 2026-09-18 | 已收尾 | [摘要](games/燃烧纪元.md) |
| 闪耀吧噜咪 | 2026-09-17 | 已收尾 | [摘要](games/闪耀吧噜咪.md) |
| 镇邪人 | 2026-09-23 | ✅ 已收尾：启动即崩（`Cocos2dxDownloader$3.run:154` NPE；旧路由补丁把远端 src 指成相对路径 + 服务器 src 漂到 ca0d4）已修——改以官方原版包为底重建、回滚 `jsb-engine.js`/`main.js` 路由补丁、包内钉 ca0d4+d5507 双份；菜单回补星空面板 `MagicRes$PanelBgDrawable`（源码已 push `cdcd46c`）；正式包 `镇邪人_1.0.3_ca0d4_star-releaseSigned.apk`（sha256 `48979c9d…`），真机 f9a116a2 冷启动零崩溃、授权 grant ok；`08_output/镇邪人` 已按 `AGENTS.md:69` 裁剪为唯一正式包（删 7 项约 320MB，清单 `10_docs/镇邪人/正式包裁剪清单_1.0.3_20260923.json`），并完成 06/07 滚动物清理（约 54.5MB） | [摘要](games/镇邪人.md) |
| 黑暗领域 | 2026-09-15 | 已收尾 | [摘要](games/黑暗领域.md) |
| 追迹迷城 | 2026-09-15 | 已交付/待核验 | [摘要](games/追迹迷城.md) |
| 无悔华夏4399 | 2026-09-15 | 已交付/待核验 | [摘要](games/无悔华夏4399.md) |
| 勇士之心 | 2026-09-21 | ✅ 已收尾：热更 res 1.0.98.10 重抓+自动战斗指纹修复（`bVs(`→明文 `BuffTableEnum` 两级指纹）；新包装机（so `9bf52c07…`/apk `4175178f…`）+ 六钩子全部安装 + 用户实测五项功能全部可用 | [摘要](games/勇士之心.md) |
| 偶像梦幻祭2 | 2026-09-15 | 已交付/待核验 | [摘要](games/偶像梦幻2.md) |
| 大话蜀山 | 2026-09-14 | 已交付/待核验 | [摘要](games/大话蜀山.md) |
| 凡尘西游修真记 | 2026-09-14 | 已收尾 | [摘要](games/凡尘西游修真记.md) |
| 魂坠深境 | 2026-09-21 | ✅ 已收尾：热更重抓（hotfix `b2709ea7…`）+ DLL 重校 + 移除托管 getter invoke 闪退根因 + 写入回读校验；so `45a4fcfd…` 真机五功能实测通过；正式包 `f191e461…`；OTA so 已提取待上传 | [摘要](games/魂坠深境.md) |
| 我的修仙世界 | 2026-09-13 | 已收尾 | [摘要](games/我的修仙世界.md) |
| 地城狩猎 | 2026-09-13 | 失败/归档 | [摘要](games/地城狩猎.md) |
| 迷雾大陆 | 2026-09-12 | 已收尾 | [摘要](games/迷雾大陆.md) |
| 无悔华夏 | 2026-09-12 | 已收尾 | [摘要](games/无悔华夏.md) |
| 弹力果冻 | 2026-09-12 | 已交付/待核验 | [摘要](games/弹力果冻.md) |
| 城主别慌张 | 2026-09-11 | 渠道阻断/暂停 | [摘要](games/城主别慌张.md) |
| 雷霆战机 | 2026-09-06 | 暂停 | [摘要](games/雷霆战机.md) |
| 蜀汉传卧龙篇 | 2026-09-06 | 纯分析 | [摘要](games/蜀汉传卧龙篇.md) |
| 阿瓦隆之王 | 历史 | 已收尾 | [摘要](games/阿瓦隆之王.md) |
| 长生天机降世 | 历史 | 已收尾 | [摘要](games/长生天机降世.md) |
| 造梦无双 | 历史 | 已收尾 | [摘要](games/造梦无双.md) |
| 超维进化 | 2026-09-21 | ✅ 0.3.5 二次热更重校（doDmg 14 参 + 防御/抗性形态C）+ 出包装机验证（splice +2555 双向闭环）+ 用户实测通过；回血回蓝×4 / 暴击+60pp / 闪避+35pp | [摘要](games/超维进化.md) |
| 虚无之刃 | 历史 | 未完成 | [摘要](games/虚无之刃.md) |
| 终末地 | 历史 | 已交付/待核验 | [摘要](games/终末地.md) |
| 百分之一 | 历史 | 纯分析/归档 | [摘要](games/百分之一.md) |
| 月圆之夜 | 历史 | 已收尾 | [摘要](games/月圆之夜.md) |
| 遗弃之地 | 2026-09-21 | 已终止/已清理（仅留文档） | [摘要](games/遗弃之地.md) |
| 无限升级 | 2026-09-23 | ✅ 12.10701/vc1644 **v7.13「双版本内置 + 失配禁止进入」已装机验证**（APK MD5 `884db62a…`/so `b442a01f…`）：v7.12（对官方 2030080，APK MD5 `a2fc5585…`）曾获用户实测通过，但**当天下午官方热更回滚**（同一天内 dll 1960960 ⇄ 2030080 来回切）⇒ 单版本守卫必然失配；v7.13 改为 **同一 so 内内置两份补丁**（`jni/targetfuntional_dll.bin` 2024960 对 2030080、`jni/targetfuntional_dll_v711.bin` 1955328 对 1960960），`WxsjMod.cpp` 用 `DllVariant`+`PickDllVariant(len)` 按运行时长度自动选（日志 `variant=hotfix2-2030080`/`variant=v711-1960960`）；**失配行为由「自崩」改为「禁止进入」**（HookExec 不调 orig 直接 `return 0` ⇒ 热更程序集不执行；实测游戏走自身 `Scrpits.MainEntry.HotUpdateStartupBootstrap:Fail`、进程存活、`/data/tombstones` 无新增、菜单仍可用 ⇒ 玩家可当场去菜单拉补丁）；**OTA 免重装通道已启用**（菜单「游戏补丁→检查更新」拉 `/v1/ota/wxsj/manifest`+so，服务端 `auth-server` 的 `POST /v1/admin/ota/wxsj/publish` 发布；OTA 单文件=`08_output/无限升级/ota/wxsj/libstarry_magic.so`，上传后玩家点更新+划掉重开即可，无需重装 APK）；测试钩子=`files/starry_force_mismatch` 存在即强制失配（删文件恢复）。官方 09-23 白天再推热更，dll **1960960→2030080 B**，v7.11「失配即崩」守卫按设计触发（玩家见「提示热更→闪退」，marker `mismatch len=2030080 expect=1960960`）；已 DIAG 抓包 + `ildiff` IL 指纹对比（新增**任务树 MissionTree**、新反作弊 `ItemAddSourceAnomalyMonitor.CheckAddSource`（新 RPC `RecordItemAddSourceAnomaly`）、`GameData.AddItem` 签名改 4 参、云/设备存档大改）→ 重放 v1(30 patches)+δ(上报名单 14 项)，`kOrigDllLen`=2030080、内嵌 DLL 2024960 B；装机日志 `patched dll injected (hit #1, patched=2024960 pad=5120)` ✓ 无崩溃 ✓；🩸**内嵌静态库血案**=`jni/Games/WxsjPatchedDll.a` 是手工 objcopy 产物、脚本原不重建 ⇒ 换 bin 后 so 仍内嵌旧 dll（体积与旧断言都看不出，装机将用旧补丁覆盖新官方 dll）→ 已修：`wxsj_so_only.sh` 每次强制重建 .a + 双断言（`.a`=bin+552；so 内逐字节存在该 bin）；RVA `0x2b890d8` 本轮未变、注入仍 ContentProvider（易盾壳禁止替换 Application）；功能 **10 开关**（无敌/秒杀/免广告/物品不减/技能免费/广告奖励翻倍/物品翻倍/无限秘境/皮肤全解锁/功能全解锁，无限技能已移除）；静默防封 **无按钮、加载即生效** = 出站 19 通道共享门，**只拦纯上报 14 项**（反作弊/举报/日志/COS 会话/前后台状态；v7.12 增 `RecordItemAddSourceAnomaly`），**其余含服务器下发一律放行**（v7.10 白名单拦了 `GetPlayerMainData` 致异界召唤/羽灵召唤失效，已废弃白名单模型）+ 腾讯 CLS 三闸 + **无条件封号拦截 6 处**（两条封号提示与 banMask/冻结全断）；🩸v7.8 事故根因=补丁 DLL 超 `kOrigDllLen` 被静默 skip（现 2024960 B ≤ 2030080 + so 构建尺寸断言）；🩸菜单模板事故=gradle 重建 app-debug.apk 致外观变样，已回滚为 `templates/wxsj_menu_20260922/classes2.dex`（453036 B，构建脚本 `MENU_FROM_TPL` 分支，设备内 dex 实核一致）；**用户实测「功能看着没问题」（异界召唤/羽灵召唤已恢复）**；🩸「开关不跨重启保留」= 菜单既有设计（`ModBridge` 构造先跑 `Preferences.clearFeaturePrefs()`，用户明确「开关默认关闭状态就是写好的」→ 按不修处理）；出包背景模板问题按用户规划不修（菜单后续统一换新）；设备开关默认全关（`block_report=1`）、SELinux `Enforcing` | [摘要](games/无限升级.md) |
| 无尽探险队 | 历史 | 已收尾 | [摘要](games/无尽探险队.md) |
| 战魂铭人 | 历史 | 已收尾 | [摘要](games/战魂铭人.md) |
| 异世界勇者 | 历史 | 已交付/待核验 | [摘要](games/异世界勇者.md) |
| 山海幻境 | 历史 | 已收尾 | [摘要](games/山海幻境.md) |
| 动物餐厅 | 历史 | 已收尾 | [摘要](games/动物餐厅.md) |
| 冰晶菜单旁路 | 历史 | 已交付/待核验 | [摘要](games/旁路菜单.md) |
| 元启世界 | 历史 | 纯分析 | [摘要](games/元启世界.md) |
| Phigros | 20260921 修复 | ✅ TapTap 强制内嵌网页登录已修复+实机登录跑通（出包待交付） | [摘要](games/phigros.md) |
| 命运-冠位指定 | 2026-09-22 | ⏸ **ACE 卡点，用户要求暂停（晚上继续）**：装机后启动=灰屏、~10–20s 一轮 SIGBUS(7)/SIGSEGV(11) 崩溃循环（无 tombstone/无应用日志/`I Unity`=0）。单变量 AB 实测：只重签 **ALIVE ✓**、全包 3531 条目重压 **ALIVE ✓**、只加 `classes15.dex` **KILLED ✗**、只加 `lib/arm64-v8a/libstarry_magic.so` **KILLED ✗** → **标准外科 APK 路线在 FGO 2.129.0 被否决**。待跑 AB-7/8/9（中性 asset / 中性 dex / 中性 so，三包已构建）定判读树：无出口→转 KSU 注入或斌哥 VM；内容级检测→清洗载荷可救。设备现存 `ab8-minidex` 探针包、已 force-stop；星光正式包 md5 `4882e58f446a351e5609a4fa294b2762` 不在设备上。AB 工具 `04_scripts/命运/{ace_ab_probe.sh,ab_zipmerge.py}`；详情 [AB 实验](../../命运/ACE_AB实验_20260922.md) | [AB 实验](../../命运/ACE_AB实验_20260922.md) · [摘要](games/命运.md) |

## 基础设施与经验

- [状态目录规则](games/README.md)
- [基础设施与工作区历史](games/infrastructure.md)
- [挂起与待审批经验](pending_experience.md)

## 历史来源

- [整理前 current 完整快照](history/current_before_rules_optimization_20260905.md)
- [本次拆分前完整快照](history/current_before_split_20260920.md)
- 指定原始备份（历史路径，当前未保留）：
  `99_archive/workspace_rules_backup_20260905_161706/10_docs/_state/current.md`
- [x] 无限升级沉淀 4 项已合并；三份 libil2cpp so 已清理，复现路径见游戏文档。
