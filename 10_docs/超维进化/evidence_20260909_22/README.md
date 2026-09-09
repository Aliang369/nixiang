# 超维进化 2.2 星空包更新 + 功能重排 — 证据与根因（2026-09-09）

> 授权：用户「模拟器已下载最新版2.2，更新到最新版的星空包」→ 后续追加「功能重排（移除秒杀/无敌/无限技能，改 8 项属性增益）」与「收尾+正式包+清理」。装机设备 MuMu 127.0.0.1:5555（adbd 即 root）。

## 0. 终态（20260909 收尾）

- **正式包（唯一留存）**：`08_output/超维进化/超维进化_2.2_surgical-releaseSigned.apk`，SHA-256 `8d850ad5630d9130af37e2becdbde8029be2f68552474a9ef301ab51a9ad7134`（179,623,794 B），= 设备安装（覆盖安装 Success，数据保留）。
- **功能集（用户定稿终版，10 项）**：0超级回血 1超级回蓝（_timer 己方双跳 ×2）｜2高暴击率+40pp 3高暴击伤害（原倍率再×2）｜4高防御（AMR+15pp 钳后叠加+0.95 安全上限）5高抗性（净值×2 同前）6高闪避+15pp（以上仅己方）｜7免广告（原样）｜8秒杀（封号，dmg=HP+1 伪装）9无敌（封号，己方受击归零，doDmg 三签名）。8 项中间版（`60a51c1e…`）已被替代。所有增益为现有值加/乘叠加，任何情况不 nerf。
- **运行验证（进程级）**：bomb fn + kill wrapper 双中和日志；splice `2627189→2628962`（+1773，与双文件模拟 node --check 预测一致）；无 anchors missed / SyntaxError / SIGKILL；进程存活（pid 21614）。证据：`logcat_cwjh_22_feature_rework.txt`、`pid_alive_final.txt`。
- **战斗内功能实测**：待用户（回血/回蓝跳字频率、暴击频率与数值、承伤、关闭恢复）。
- **清理台账**：/tmp cwjh* 全清（含 38M 工作目录+截图）；设备 /data/local/tmp 无残留；`08_output/超维进化/` 仅存正式包（alias+idsig 已删）；保留 `07_patched/超维进化/surgical_2.2/AndroidManifest.xml`（MF 缓存构建依赖）与热更基准档 `game_js_hotupdate_20260909.js`（sha256 `0df2ee83…`，盘上档 2,602,970 B；注意运行时求值档为 2,627,189 B，服务器持续更补丁，锚点双版验证）。
- **后台待办（用户侧）**：菜单模板重传（`10_docs/超维进化/菜单模板_cwjh.json`，10 项）。

## 1. 版本与包（2.2 基线）

| 项 | 值 |
|---|---|
| 官方 2.2 base | `02_apks/超维进化/超维进化_2.2_base.apk`，SHA-256 `bcea444dc568eee604308d90a284c2e9f4af6c02fb850592796dee0810ae9da5`（176,559,657 B，vc12） |
| 交付包（中间版，已被重排版替代） | SHA-256 `5ff209a685ee9c28eefbbbd9c55d3f94cdf9e164e3cc091e9b1e4c2c77358f2e`（功能重排前，仅炸弹函数修复）；条目级差异：新增 `classes4.dex`、`lib/arm64-v8a/libstarry_magic.so`、`libc++_shared.so`、`assets/certs/starlight.crt`；改动仅 `AndroidManifest.xml`/`classes.dex`/`classes3.dex`；`classes2.dex`/`resources.arsc` 字节不动（脚本断言）。重排版（60a51c1e）条目结构相同，仅 so/classes 内容更新 |
| 装机 | 卸载官方 2.2（用户确认，无数据损失）→ 装星光包 → 修复包覆盖安装 Success；设备 base.apk hash = 交付包 |
| 复现链 | `mod菜单/starry-mod-menu/scripts/surgical_cwjh_2.2.sh`（新增，基于 2.1 终版链；MF 缓存 `07_patched/超维进化/surgical_2.2/AndroidManifest.xml`，stopWithTask=true 对齐僵尸看门狗框架） |

## 2. 2.2 预检（无需重抓锚点）

- `libcocos2djs.so`（30,036,040 B）：kill 包装器 0x16ebdf8 前导字节 `2a1f03e0 d10203ff` 与 2.1 一致；`_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_` 与 JNI evalString 符号均在。
- APK 内置 game.js（2,202,886 B，sha256 `f1839bc2…`）：**六锚点全部唯一命中**，仍是 2.1 首发签名系（`doDmg(e, n, r, a, …, u = i.Normal)`）。
- TapSDK（ClientLoginUtils/GatekeeperRepository）、AppActivity/GZQYActivity、SilverdewApplication 均在原 dex 原位，外科补丁全部模式匹配通过。

## 3. 首启运行与 SIGKILL 根因（关键）

- 首启流程：隐私协议 → 同意 → 引擎加载 → **热更 0.2.89**（game.js 2,627,189 B）→ 游戏自重启。
- splice 证据：内置 JS `2202886→2204288`、热更 JS `2627189→2628591`（每进程两次求值均命中，无 anchors missed、无 SyntaxError）。
- **异常**：每次 hook 完成后约 6.03s 进程被 `SIGKILL`（Zygote: exited due to signal 9；无 tombstone、无 Java crash、DropBox 无 data_app_*）。
- 静态定位（objdump + 全文件扫描，2.2 so）：
  - 可执行段 raw-svc 包装器两处（同构前导 `mov w0,wzr; sub sp,#0x80`）：0x16ebdf8 = kill(0,9)（x8=129，已被中和）；0x16ebe24 = **openat**（x8=56，aarch64 无 fork/open，56=openat，文件存在性检查，非自杀路径）。
  - `.rodata` 0x19ab91c/0x19ab920 有 `d4000001` 字节常量，非执行段。
  - **0x16d5a08（新自杀路径实锤）**：`str x30,[sp,#-0x10]!; bl sleep@plt; bl getpid@plt; mov w1,#9; bl kill@plt; mov x0,xzr; ret` —— 炸弹函数 sleep(x0)→getpid→kill(pid,9)，**直调 libc kill，绕过被中和的自定义包装器**。无 BL/adrp 直调/指针表引用（地址运行时 mov/movk 拼装，控制流平坦化派发）。
  - libc 导入面：kill/fork/syscall/pthread_create/sigaction 均导入；kill@libc 另有 3 个调用点在 0x8dcc84/0x8e94c8/0xa710a0（引擎/SDK 区域，本次未动，观察项）。
- 时间线吻合：hook 完成 00:25:26.547 → SIGKILL 00:25:32.580 = **6.03s** ≈ 已知设计「检出 inline hook 派炸弹 sleep(6) 后 kill」。

## 4. 修复与验证

- [CwjhMod.cpp](../../../mod菜单/starry-mod-menu/app/src/main/jni/Games/CwjhMod.cpp)：中和逻辑抽 `Cwjh_PatchHead()`，新增炸弹函数入口补丁（前导守卫 `{f81f0ffe, 97c06eb9}` → 补 `{aa1f03e0, d65f03c0}` = mov x0,xzr; ret，原函数本就返回 0）。
- 修复后（pid 17531）：`bomb fn (libc kill) neutralized` + `kill wrapper neutralized` 双日志，hook 后**存活 90s+ 无死亡信号**（修复前 6.03s 必死），界面推进到 TapTap 网页登录（强制网页登录补丁 2.2 生效）。
- 证据：`logcat_cwjh_22_survival.txt`（37 行）、`pid_alive_90s.txt`。

## 5. 教训与新坑（候选经验，待审批）

- **坑 D（构建）**：切 STARRY_GAME_* 后 obj 目录残留上一游戏的对象（make 不追踪 flag 变化），链 `undefined symbol: Es2_*`。解法：切游戏后 `rm -rf app/obj` 再 ndk-build。
- **aarch64 svc 号易误判**：56=openat（非 clone）、129=kill、220=clone；反汇编确认 syscall 号才能定性。
- 2.1 教训「so 与 classes4.dex 成对出包」继续有效；本次 Java 面未动，classes4 由脚本成对重打。

## 6. 待用户验证（交付边界）

- TapTap 短信验证码登录（需用户收码）→ 进战斗实测：秒杀/无敌/无限技能/免广告。
- 功能语义维持 09-05 版；服务器权威架构下上报屏蔽维持「暂不做」（20260907 用户决定）。
