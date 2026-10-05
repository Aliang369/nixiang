# 勇士之心 2026-10-05 自动战斗修复轮（真机/MuMu 双侧）— 决定性证据

## 结论

用户反馈（m00001，2026-10-05）：「自动战斗的功能，修复一遍另一边又失效的问题，我需要让两边共都生效，
目前是真机生效，模拟器失效的」。MuMu(Houdini) 上「无效果」＝**段 B（装上了没命中 / 根本没装上）**，
经两轮定位共四个真根因，已全部修复；修复只走**运行时判据单路径，无机型/模拟器名单分叉**（硬约束#6）。
L2（机器）双机全过（同包 `precheck` PASS_TO_HUMAN）；**L3 用户实测双侧通过**（MuMu + 真机 96f0d883，2026-10-05）；已收尾交付（唯一正式包在 `08_output/勇士之心/`）。

## 真根因（时间序，全部实测）

| # | 根因 | 实锤 | 修法 / 落点 |
|---|---|---|---|
| 1 | `FileOffToVa()` 只认 `x` 段；MuMu/Houdini 下 cocos so 三段全为 `r--p/rw-p` | `evalString rva 0x3138E4 not mapped` 每约 200ms 刷屏（第 4 轮装机包） | 两轮探测：`x` 段优先 → 覆盖该偏移的最大可读段兜底 + 序言读回校验；日志 `readable-map fallback engaged`（1.0.3 重建线 + 1.0.4 主线） |
| 2 | 同 so 双映射下存在「影子映射」：Dobby 可能挂在永不执行的那份上，`DobbyHook` 返回成功但**永不命中** | 19:13 冷启（pid 29630）：`SELFCHECK … rd=1 hook=1`，但 `files/ysxz_native_ok` 不存在、bundle splice 0 次 | main.js 自举 + 两路线互斥判据：原生**真截获**才写 `files/ysxz_native_ok`（内容=pid）；自举驱动读到同 pid 证明则让位，否则 JS 接管（装钩前二次复查防竞态） |
| 3 | 自举版 main.js 加密标志字节写成 `0x01`，引擎拒收→回退按明文解析 | `D jswrapper: ERROR: Uncaught SyntaxError: Unexpected token '*', location main.js:0:0`（底包该字节=`0x00`） | `ysxz_mainjs_patch.py` 从 pristine 继承标志字节 + 断言必须 `0x00` |
| 4 | 自动战斗指纹写死旧混淆名（1.0.3 头文件曾为 `bVs(`；热更 res 1.1.4.0 已改名 `fDj`） | 旧指纹装不上 → `auto=NO` | `YsxzPatchJs.h` tryAuto 改两级指纹（主：`AUTO_BATTLE`+`BuffTableEnum` 明文；兜底：短函数 `return X.Y(...)`）+ 拒绝候选诊断行；与 1.0.4 主线同源，禁再写死混淆名（hook-rules §24） |

## 两条路线都已在 MuMu 实测生效（同一份包）

- 路线 A ＝ JS 自举（native 挂影子映射、未命中时接管）；
- 路线 B ＝ 原生 splice（native 命中执行映射时走原路线，自举让位）。

两条路线分别在不同冷启动拿到 `auto-battle privilege hooked` + `REPORT … auto=yes`：

路线 A（19:13 冷启，pid 29630）：

```text
I Mod_Menu: [Ysxz] SELFCHECK evalString va=0x7f38a327e8e4 rva=0x3138E4 via=readable rd=1 hook=1
I Ysxz    : [MODMOD] require trap installed (cur=undefined)
I Ysxz    : [MODMOD] auto-battle privilege hooked = .prototype.fDj
I Ysxz    : [MODMOD] REPORT  formula=bJV apply=undefined gate=undefined cost=undefined death=undefined movesys=NO auto=yes
```

（同进程 `ysxz_native_ok` 不存在 ⇒ 自举接管；`not mapped` 0 次、无 SyntaxError、进程稳。
完整日志 `09_test/勇士之心/_l2_run6_log.txt`）

路线 B（19:15 冷启，pid 29984；第一次 DobbyHook 在影子映射上失败，重探后命中执行映射）：

```text
W Mod_Menu: [Ysxz] DobbyHook evalString failed @0x7f38a30768e4
I Mod_Menu: [Ysxz] SELFCHECK evalString va=0x400020d138e4 rva=0x3138E4 via=readable rd=1 hook=1
I Ysxz    : [MODMOD] native splice active (proof pid ok) -> bootstrap stand-down
I Mod_Menu: [Ysxz] logic splice #1 len 6412291->6434368 file=assets/game/index.js
I Ysxz    : [MODMOD] auto-battle privilege hooked = .prototype.fDj
I Ysxz    : [MODMOD] REPORT  … auto=yes
```

（`files/ysxz_native_ok` 内容 `29984` ＝ 同进程 pid ⇒ 自举正确让位。
完整日志 `09_test/勇士之心/_l2_run7_native_path.txt`）

## 产物 / 指纹

| 产物 | 指纹 | 说明 |
|---|---|---|
| `08_output/勇士之心/勇士之心_1.0.3_starry-final-releaseSigned.apk` | md5 `4228fbcbff488aa7a97b64c557e151ac`；sha256 `936f611bd806a33922971f4a8b08c10fbfc37ea181ecf0f060acfd5b03e74fe0`（64,675,627 B） | **唯一正式包**（双机实测通过的就是这一份）；`delivery_20261005/` 内含 icon 512/144 + menu_template + SHA256SUMS |
| `10_docs/勇士之心/正式包裁剪清单_20261005.json`、`滚动物清理清单_20261005.json` | keep md5 `4228fbcb…`（removed=0）；本地滚动物 7 项 ≈638MB 入隔离区 | 收尾台账 |
| `real_device_decisive_lines.txt`、`precheck_decisive_lines.txt`（本目录） | 真机 L3 窗口 19:40:26–46 开关/挂点行；双机 precheck 决定性行 | 完整 logcat 已随收尾清理（隔离区可恢复） |
| ~~`09_test/勇士之心/l2_20261005/`~~、~~`09_test/_recorder/2026*`~~、~~`mod菜单/starry-mod-menu/libs_ysxz/`~~（含 so md5 `4ae5c58b…`、`ysxz_boot.js`、`main.patched.enc.js`） | 已随收尾清理入隔离区 `C:/Users/Administrator/.Trash/nixiang_..._20261005_195030` | 可再生（`ysxz_build_so_103.sh` / `ysxz_release_103.sh --out` 重建） |

## 最短复现

```bash
bash 04_scripts/勇士之心/ysxz_build_so_103.sh           # 重建 so（含自举素材；缺料就地报错）
bash 04_scripts/勇士之心/ysxz_release_103.sh --out DIR  # 出包（含三条字符串断言 + main.js 标志断言）
adb -s 127.0.0.1:16384 logcat -d | grep -a "MODMOD\|boot self-heal\|SELFCHECK\|native splice"
```

## 验收结果

- **MuMu（模拟器）**：用户 2026-10-05 进战斗实测通过 ✅（功能#6 第 5 轮＝有效，本轮主语就是模拟器修复）。
- **真机 96f0d883（小米 Mi 11）**：**同一份包**已装；`precheck` PASS_TO_HUMAN（菜单 so 加载/菜单弹出/授权通过/空转 2min 稳）；
  **用户进战斗实测通过**（功能#6 第 6 轮＝有效；决定性行 `real_device_decisive_lines.txt`——19:40:26–46 逐项开关 + 自动战斗
  `.prototype.fDj` 挂点 + `privilege granted`，与 MuMu 通过轮同型；用户原话「功能都是没问题的」）。
- **收尾**：唯一正式包 md5 `4228fbcb…`/sha256 `936f611b…` + `delivery_20261005/`；`08_output` 已裁剪；本地滚动物/设备临时目录已清理（隔离区可恢复）。

## NOT PROVEN / 限制

- 长时间连续自动战斗稳定性（未做长时间压测）；服务端对自动战斗行为的风控结论。
- 已知限制：`files/res/main.js` 自愈只认 pristine/patched 两种形态；热更换 res 后变成陌生版本则不动（防误伤），
  此时自举不生效，需按新 res 重生成素材。
