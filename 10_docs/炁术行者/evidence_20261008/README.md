# 炁术行者（qsxz）2026-10-08 服务端热更复刻 + 「两功能没效果」诊断证据

> 只列决定性证据、最短复现命令与 NOT PROVEN 项；不要求后续会话通读原始日志。
> 原始 logcat / precheck 输出属滚动物，已按收官清理（见游戏摘要「收官结论」），可按下文命令重跑再生。

## 结论链（按时间）

1. **服务端热更**：用户 2026-10-08 23:01 重放底包，`02_apks/炁术行者9.15.apk` md5 `f528339aadb2ba9fcdd6de477a936e06` —— **与 10-07 出包所用底包逐字节一致**（本作 APK 只是 1.4 MB 纯 WebView 壳，游戏本体在服务端）。
2. **真正变的是服务端 JS**：`/assets/main/index.js` 重新压缩（5,414,360 B，md5 `67df56a68137b11aef65f1229bdcef70`），压缩器把 `if(x){A}else B` 改写成三元式并重排局部变量名 ⇒ 12 处补丁锚点里 **5 处（⑥⑦b⑧⑪b⑨）命中 0 次**；已按新源码逐处重导（见 `04_scripts/炁术行者/qsw_patch_js_star.py` 文件头「锚点版本：2026-10-08 复刻」与逐处注释）。改名映射：`g→p`（apipath/dbid/hurtRate 模块）、`s→l`（getChannel/restartGame 模块）、`l→f`（http 句柄）、`v→b`、`e→t`，树洞分支 `A/D/O → O/A/D`。
3. **重装后用户报「两个功能都没效果」**（m00355）。逻辑上逐项排除：授权（`live 0→1`）、投值（`apply k=20 dr=50`）、桥接、`shouldInterceptRequest` 拦截（`req /assets/main/index.js` ×5 ↔ `serving qsw/main.js` ×5）、包内容（包内 `assets/qsw/main.js` md5 `c6386ccbdbb5efdad3db17dd2b6c6e7c`，三处 `window.__QSW_DMG(` 调用点齐）、补丁语法与位置（`node --check` + `getBaseReduceHp` 尾部 `…window.__QSW_DMG(Math.max(0,s)+Math.max(0,r),this.camp)}`）——**全部正常**。
4. **定位到取证缺口**：生产补丁里 `w.__QSW_LOG=function(){}` 是空实现 ⇒ ⑥⑧⑨⑩ 的取证信标全静默，**没有任何战斗期证据可对账**。这是我的失误。
5. **装 v10s_diag 诊断包取地面真值**（同签名覆盖安装，数据保留；唯一正式包未改动）⇒ 钩子在运行期**确实被调用**，camp 语义与设计一致，数值严格跟随滑块。
6. **用户复测结论**：「功能没问题了」。

## 决定性证据（诊断包 logcat，tag `StarryJs_qsxz`）

| 时间 | 原始行 | 说明 |
|---|---|---|
| 23:28:06.405 | `QSWDIAG {"ev":"loaded","tag":"v10s_diag","k":1,"dr":0,"bridge":true,"ua":"Mozilla/5.0 (Linux; Android 16; OPD2404 "}` | 包内补丁版 main.js **真的在页面里执行**（装载时是原版默认值 k=1/dr=0） |
| 23:28:06→23:32 | `QSWDIAG {"ev":"apply","k":20,"dr":0.5}` ×311（另有 dr 0.14/0.26/0.38 各 1 条＝用户拖滑块中间值） | 框架把菜单值**真的投进页面**，每次注入重投一遍 |
| 23:29:42.182 | `{"ev":"dmg","n":1,"v":1241,"c":1,"k":20,"dr":0.5,"r":620,"b":"def"}` | 玩家（camp=1）受击 → 减伤支，1241→620 ✓ |
| 23:29:42.246 | `{"ev":"dmg","n":3,"v":36,"c":2,"k":20,"dr":0.5,"r":714,"b":"atk"}` | 敌方（camp=2）受击 → 倍率支，36→714（=×20）✓ |
| 23:29:42.274 | `{"ev":"dmg","n":10,"v":0,"c":2,"k":20,"dr":0.5,"r":0,"b":"atk"}` | 前 10 次逐条打点；n=1/5/9 是 camp1，n=3/7 是 camp2 |
| 23:29:46.163 / 23:29:51.376 | `{"ev":"dmg","n":100/200,…,"k":20,"dr":0.5}` | 每 100 次打点，值仍正确 |
| 23:30:18.393 | `{"ev":"dmg","n":300,"v":0,"c":2,"k":1,"dr":0,"r":0,"b":"atk"}` | **用户 23:30:13–14 把两滑块拖到下限** ⇒ 本次结算就是原版口径 —— 与菜单值严格同步，不是「失效」 |
| 23:30:22.397 | `{"ev":"dmg","n":400,"v":0,"c":1,"k":20,"dr":0.5,"r":1,"b":"def"}` | 滑块拉回 20/50 后立刻恢复生效 |
| 23:30:39.197 / 23:30:45.422 | `{"ev":"neterr","cb":"FightAction_barrierUpdateTroop_20","code":9998,"d":"{\"code\":9998}"}` | 障碍/守卫战兵力更新被服务端拒（见 NOT PROVEN） |

**伤害链对照（从包内 main.js 读出的原文，证明 `B` 就是飘字数值）**：
`B=this.getBaseReduceHp(t.damage,t.enemyProp,!!t.noFloor)*t.damage.attenuation`（@2824872）→ 累加进 `R` →
`this.showHpFont(-B,t.damage.critical>0,!1,t.damage.skillId)`（**飘的伤害数字就是 B**）→
`1==t.enemy.camp ? P.instance.totalHurt+=q : P.instance.totalBeHurt+=q`（**反证玩家 camp=1**，减伤/倍率两支没判反）。

## 诊断包（可复现）

```bash
# 只换 assets/qsw/main.js，把 __QSW_LOG 从空实现改成转发 StarryH5.log（→ logcat tag StarryJs_qsxz），
# 并在伤害钩子加 n/camp/k/dr/返回值的计数打点；不写 08_output/，唯一正式包一字节不动
bash 04_scripts/炁术行者/qsw_diag_build.sh            # 出包 /tmp/qsw_diag/炁术行者_1.0_diag.apk
bash 04_scripts/炁术行者/qsw_diag_build.sh --install  # 覆盖安装（同签，保留卡密数据）
bash 04_scripts/炁术行者/qsw_diag_build.sh --restore  # 测完把唯一正式包装回设备并闭环校验
```

## 最短复现命令（正式交付链）

```bash
python3.11 04_scripts/炁术行者/qsw_patch_js_star.py --src /tmp/qsw_build/main_index.js --dst /tmp/probe.js  # 只读体检：12 锚点必须全 OK (1)
node 04_scripts/炁术行者/offline_regression.js                  # 19 项断言
bash 04_scripts/炁术行者/qsxz_build_so.sh                       # 重编 libstarry_magic.so
QSW_FETCH=1 bash 04_scripts/炁术行者/qsw_build_star.sh --install # 重拉服务端 JS → 出包 → 装
bash 04_scripts/_common/post_install_precheck.sh --game 炁术行者 --serial f9a116a2 --pkg com.h5wrapper.app
```

## NOT PROVEN

- **上一轮「两个功能都没效果」未归因**：同一份代码在本轮复测中取到正向地面真值（钩子被调用、camp 正确、值随滑块走）。上一轮唯一缺口是取证信标静默（`__QSW_LOG` 空实现），因此**无法回填那次战斗的真值**，最可能是判读样本不可对账（当时只反复确认了「投值/拦截/包内容」而没有任何战斗期数据）。下次遇到同类反馈，先装 `qsw_diag_build.sh` 而不是先改代码。
- **`barrierUpdateTroop` code=9998**：本次 k=20/dr=50 下障碍战出现 2 次服务端拒绝（历史已知 `hurtCheck` 侧在 `dr=0.7` 触发 `code=9988` 拒判）。未做全档位服务端验证，交付档位（伤害倍率 1-20 / 减伤 0-50%）与障碍战模式的组合仍是开放项。
- **device_id 两处不一致**（10-07 起遗留）：sdcard/tmp 种子 `dev_760f1cc7de536cd8c34dd8f79c6f62b4` vs 应用内 `auth_device_id=dev_22ed5cb056092cd538058ae406bd81c5`（`auth_device_id_source=cached`）。本轮未动，卡密未复现被拒。
