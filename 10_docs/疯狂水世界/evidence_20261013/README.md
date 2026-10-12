# 疯狂水世界 1.11.6 — 阶段② 三功能【无敌 / 伤害倍率 / 战斗加速】交付与收官（2026-10-13 归档）

状态：**✅ 已交付**（三功能经用户真机实测「功能都有效」；主线真进度受服务端权威的限制已向用户说明，用户明确决定不做观感补丁）。
判定：**CLOSED**（用户 m01514 原话「不做。进行收尾+正式包+清理+git」）。
客观结果：本作是**服务端权威 + 客户端回放**架构 ⇒ 三功能中「战斗加速」全模式真实有效；「无敌 / 伤害倍率」改变战斗演出，主线胜负弹窗与关卡推进仍由服务端响应决定 ⇒ 不改变奖励与真进度。

## 1. 交付物（唯一正式包）

| 项 | 值 |
|---|---|
| APK | `08_output/疯狂水世界/疯狂水世界_1.11.6_surgical-releaseSigned.apk` |
| 大小 | 520,989,472 B |
| md5 | `a723794db9dcd90ea2ee4a0c3fda19c8` |
| sha256 | `b5e487a5d6b75459081681a61872329b77d681c59143f547e7482657b3d2e2e6` |
| 签名者 | `CN=StarryMagic`（`01_tools/starry_release.jks`） |
| 渠道块 | `id=0x64707763` 值 `"259425"`（重签后回注，v2 摘要不覆盖签名块自身） |
| 包内 so | `lib/arm64-v8a/libstarry_magic.so` md5 `7dae074d2f86cd356544dfd23114eec0` |
| 包内 dex | `classes5.dex` 553,004 B = 星光（官方 `classes.dex`~`classes4.dex` 逐字节透传） |
| 兼容性 | 与装机包**完全相同**（同 so / 同 dex / 同 Manifest），可直接覆盖安装继续玩 |
| 交付附目录 | `08_output/疯狂水世界/delivery_20261013/`（`icon_疯狂水世界_512.png` / `_144.png` + `menu_template.json`，源模板 `10_docs/疯狂水世界/fkssj_menu_template.json`） |

装机包与正式包同源：装机时设备 `base.apk` md5 与产物 md5 闭环一致（同签 `-r` 覆盖安装，悬浮窗权限「③ 非 root 直设成功」）。

## 2. 三功能实现方式（可重建）

- 路线：**运行期 JS 层补丁**（零 native hook）——因 ① `assets/**` 394 个 .js 全为 `NHPK` 密文；② `libcocos.so` dynsym 全隐藏（无按名 native hook 点）。注入经 `CocosJsBridge.evalOnGLThread` / `com.cocos.lib.CocosJavascriptJavaBridge.evalString`。
- 宿主：`mod菜单/starry-mod-menu/app/src/main/jni/Games/FkssjMod.cpp` §⑨-b 的 `kPatchJs`（4 锚点、函数身份幂等守卫）+ `kCfgJsFmt` + `FkssjBridgeThread`（常驻 2s 一轮，cfg 变化才 eval，每轮重挂）。
- 锚点：`BattleUtils.applyDamage`（无敌免伤 / 敌方伤害倍率）、`AIUnit.prototype.checkDeath`（我方恒不死）、`BattleInterfaceBase.prototype.update`（`dt × spd` 推帧加速）、`BattleInterfaceBase.prototype._onBattleEnd`（只读取证，出 `battleEnd` 行）。
- 重建：`bash 04_scripts/疯狂水世界/fkssj_build_so.sh`（内含 `fkssj_check_js.py` 的 `node --check` JS 语法门禁）→ `bash 04_scripts/疯狂水世界/fkssj_redeploy.sh`。
- 阈值：伤害倍率 1..100、战斗加速 1..10（1 = 关闭）。

## 3. 决定性证据

见同目录 `decisive_lines.txt`（四段：菜单加载/身份同步 · 四锚点挂载 · 档位下发 · **用户实测两场主线**）。核心一行：

```
fkssj-js battleEnd type=MAINLINE clientWin=1 serverWin=0 popup=FAIL frame=65 fightTime=2/90 inv=2 dmg=4
fkssj-js battleEnd type=MAINLINE clientWin=1 serverWin=0 popup=FAIL frame=65 fightTime=2/90 inv=4 dmg=8
```

- 补丁在战斗内真实生效：`inv`/`dmg` 命中计数逐场递增（`inv=2→4`、`dmg=4→8`），且 `frame=65 / fightTime=2/90` ⇒ **演出正常收场、无超时空转**。
- 主线判定归属：`popup=FAIL` 而 `clientWin=1` ⇒ 弹窗听服务端 `serverData.stats.isLeftWin`，客户端演出胜负不参与结算（分模式结论见 §4）。
- 用户实测口径（m01455 原话）：「功能都有效，但是现在的问题是服务器好像返回时战斗失败，这个怎么处理？有办法吗？」

## 4. 语义结论（架构级，勿再重查）

- 服务端权威：`ChallengeStageRequest{stage}` 是唯一请求字段，客户端对结算**零输入**；全包 663 个消息名中**无任何请求携带战果**；`BattleService.Execute/StreamExecute → BattleResult` 只存在于共享 proto（`chunks_____virtual_proto-bundle.js.js` @1268582），**客户端零调用者**。服务端用它自存的账号数据 + 自生成的 `randomSeed` 复算，客户端只回放 ⇒ **我们的流量与原版逐字节同构**（不多字段、不伪造上报），无识别/封号风险。
- 弹窗归属分模式：**主线**读服务端 `stats.isLeftWin`（`StageMainLineBattleInterface._showBattleResult()`）；**其余模式**读客户端 `t[0].victorCamp === MY_CAMP`（`BattleInterfaceBase._onBattleEnd`）。
- 可达语义：战斗加速 = 全模式真有效（`fightTime` 由 `_accumulateTime.add(FRAME_DELTA)` 累加 = 帧计数，放大 `dt` 不改判定）；无敌 / 伤害倍率 = 主线仅演出层（奖励与关卡推进仍由服务端定），其余模式连弹窗胜负也变胜利，但**奖励/进度仍由服务端响应决定**；服务端判负 + 我方无敌时演出靠 `maxFightTime` 兜底收场（本次实测未触发，`frame=65`）。
- 真进度提升只能靠**服务端认的账号战力**（阵容/等级/宠物/芯片），mod 帮不上；「本地判胜推进」观感补丁已被用户否决（m01514）。

## 5. 收官清理（本次台账）

- `08_output` 裁剪：仅保留唯一正式包；中间包 `…_surgical-unsigned.apk`（md5 `44619541eacef197b8764d752cc31753`）已删，清单 `10_docs/疯狂水世界/正式包裁剪清单_20261013.json`。
- 本地清理（`trash_paths.sh` 10 条显式路径，quarantine `~/.Trash/nixiang_fkssj_20261013_20261013_001024/manifest.json`）：`07_patched/疯狂水世界`、`09_test/疯狂水世界/base`（519,961,058 B 官方底包副本，APK 不留本地）、两个旧 `precheck_*`、`mod菜单/starry-mod-menu/{build, libs_fkssj, obj_fkssj, app/src/main/libs, app/libs}`、`…/jniLibs/arm64-v8a/libstarry_magic.so`。
- 保留（不可再生 / 决定性证据）：`09_test/疯狂水世界/jsdump/`（23 MB 解密热更样本，原包删除后无法重建）、`battle_20261012_2355*.log`、最新 `precheck_20261012_233156/`。
- `04_scripts/疯狂水世界/pipeline.json` 已按 prune 清单更新（surgical verify `file_exists`→`not_exists`、删除已清产物快照、追加 2026-10-13 收尾清理 note）。

## 6. 遗留与风险

1. 服务端判负 + 我方无敌的极端组合下，若演出无法经 `checkFail()` 收场，会跑到 `maxFightTime`（`frame>2e5` 兜底）——本次实测未出现；缓解手段 = 开战斗加速。
2. 主线真进度/奖励不受 mod 影响（架构决定，非缺陷）。
3. 若将来需要「主线本地判胜 + 本地推进」变体：锚点④ 内置 `serverData.stats.isLeftWin` 置真 + `ModelManager.model.setMainlineStage(level+1)` 即可实现，代价 = 奖励仍空、重登回退、下一关按钮仍被服务端判负。用户已明确不做。
4. 去 TapTap 签名任务：底包无 `com.taptap.sdk.login.*`/`TapSignKit`（只有 `com.taptap.services.update.*`）⇒ 既有 patch 点全不适用，**无补点可做**，且不改包内 assets 亦可正常登录（用户实测可玩）。
