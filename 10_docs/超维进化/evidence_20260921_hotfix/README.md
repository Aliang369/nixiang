# evidence_20260921_hotfix — 超维进化热更重校决定性证据

> 当前基线 = **0.3.5**（下午二次热更）。上午的 0.3.3 记录保留在下方作为历史。
> 详细记录：[../热更重校_20260921.md](../热更重校_20260921.md)｜封号分析：[../封号与上报检测面_20260921.md](../封号与上报检测面_20260921.md)

## 0.3.5（本节为准）

| 文件 | 说明 | SHA-256 |
|---|---|---|
| `game_js_hotupdate_0.3.5_20260921.js` | 设备热更档 2,767,392 B，**当前校准基准** | `2fa45bc4188f49823e39fb53c36ccb6c2156772b04928f77923f1bb0ccb1dd90` |
| `spliced_0.3.5_hostsim.js` | 真 C++（主机编译）splice 产物，2,769,947 B，`node --check` PASS | `34705171232e0e75acc79e0bdf3bdc84649850aee0809a2769083b419e48d5ae` |

漂移：`doDmg` 14 参（+`C = -1`）；④⑤ 钳制行变量改名 `o`/`c`、上限改 `t.StatusDef.getMinEdr(this)`；② 暴击 3 副本、③ 暴伤 2 副本。①⑥⑦ 未漂移。

**最短复现（离线，权威）**

```bash
bash mod菜单/starry-mod-menu/scripts/cwjh_splice_hostsim.sh \
  10_docs/超维进化/evidence_20260921_hotfix/game_js_hotupdate_0.3.5_20260921.js /tmp/out.js
# 期望：in=2767392 out=2769947 delta=+2555，node --check PASS
```

该脚本**不手抄逻辑**：自动从 `CwjhMod.cpp` 抽 `ReplaceAll` + `SpliceCombatJs` + `CWJH_*` 宏，clang++ 主机编译后执行 ⇒ 与设备运行时同一份逻辑。设备实测 `2767392→2769947`（+2555）与之吻合。

## 历史：0.3.3（上午）

| 文件 | 说明 | SHA-256 |
|---|---|---|
| ~~`game_js_hotupdate_20260921.js`~~ | 上午档 2,727,219 B（已被 0.3.5 取代；2026-09-30 按热更可删规则清理，可重拉） | `d3101b26cf153ed3ba4b0d55a39965803ddad6cf97847c503e0086bd13474b4b` |
| `game_js_builtin_0.1.86.js` | APK 内置档 2,202,886 B（形态 A 回归基准） | `f1839bc2993723ab329d866252b5ad325944c3462ec67e79e3fccd397b60227c` |
| `superseded_handwritten_splice_sim.py` / `…_0.3.3.py` | **已废**手写镜像。前者对 0.3.5 算出 +2423（真值 +2555）：转写掉了 `callStaticMethod(...)` 的收尾括号，语义变而语法仍合法，`node --check` 查不出。留档仅作方法论反例，**不得当参考实现**。 | — |

## 运行时证据（设备 f9a116a2，2026-09-21 13:09）

```
[Cwjh] bomb fn (libc kill) neutralized @7437b77a08
[Cwjh] kill wrapper neutralized @7437b8ddf8
[Cwjh] early se hook ok at iter=189 splice=0
StarryMagic: 检测到 Cocos JS 引擎，ENGINE_COCOS / MOD 已就绪 engine=3 cocosGame=1
[Cwjh] combat splice #1 len 2767392→2769947 file=src/assets/script/src/logic/game.js
```

反例（同日 13:07，干净安装首启）：停在隐私弹窗 60 s → `[Cwjh] early hook timeout` → 反篡改中和未装 → splice 后 9 s `WINDOW DIED`（无 Java 崩溃、crash buffer 空）。**重启即恢复**，见 `../热更重校_20260921.md` §0.1。

## 功能验收（用户实测）

用户 2026-09-21 13:24 手动实测后确认「功能没问题了」。日志侧证据 `play_session_menu_toggles_20260921.log`：
32 条 `[Cwjh] feature #N=ON/OFF splice=2`（菜单逐项开关）、`[验证] grant ok exp=1790053894`（授权自恢复，未重输卡密）、
期间无 JS 异常。战斗内数值本身为用户口径，客户端无独立数值证据。

## NOT PROVEN

- 战斗内数值的**量化**证据（回蓝/回血倍率、暴击/闪避百分点）：仅用户主观确认，无日志级量化。
- 原生 checker 检出后是否向服务端上报：so 字符串全扫无 URL（沿用 20260905 结论，未推翻）。
