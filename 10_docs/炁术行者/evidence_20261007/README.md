# 炁术行者（qsxz）收官证据 2026-10-07

> 只列决定性证据、最短复现命令与 NOT PROVEN 项；不要求后续会话通读原始日志。
> 原始 logcat / precheck 输出属滚动物，已按收官清理（见游戏摘要「收官结论」），可按下文命令重跑再生。

## 结论（根因 + 修复）

- **症状**：菜单滑块拖好、关卡里没效果（用户多轮反馈「功能没效果」「两个功能都不行」）。
- **根因（框架级）**：`ModBridge` 构造函数**无条件** `Preferences.clearFeaturePrefs()`；菜单浮层每次重开都新建 `ModBridge` ⇒ 玩家刚拖的滑块值被同帧抹掉。H5/JS 补丁靠 `StarryH5.getState()` 读这些偏好取值，值被删 ⇒ 投的永远是默认。
  - 连带坑：某些 ROM 偏好 XML 往返给字符串补尾随空白，`equals` 永远判「变了」⇒ 每次启动照清。
- **修复**：
  1. `ModBridge` 改用 `Preferences.clearFeaturePrefsIfListChanged(指纹)`（指纹＝`nativeGetFeatureList()` 拼出，`canonSig()` 去全部空白后比/落盘）；
  2. `Preferences.readInt/readBoolean/readString/readLong` 遇 `ClassCastException` 只回默认值、**不再 `remove(k)`**；
  3. `StarryH5.getState()` 按 spec 前缀分流 bool/int（`isBoolSpec`/`isIntSpec`），滑块值走 int 循环。

## 决定性证据

| 证据 | 落点 | 结论 |
|---|---|---|
| 离线回归 19 项断言全绿 | `node 04_scripts/炁术行者/offline_regression.js` | 授权门控 / 越界钳制 / 幂等 / **投值通路** / 无百分号井号 |
| L2 机器预检 PASS | `bash 04_scripts/_common/post_install_precheck.sh --game 炁术行者` | 包在、启动不崩、菜单 so 加载、菜单弹出、授权墙、空转不崩 |
| 冷启自检全绿（logcat） | 飞行记录仪 `09_test/_recorder/20261007_225850/logcat.txt`（已清理，可重跑） | `StarryMagic: 星光魔法 MOD 菜单已加载 (MainModern tag=qsxz)`、`StarryBridgeTest: === 桥接自检结束: PASS=15 FAIL=0 ===`、`libstarry_magic.so … ok` |
| 用户 L3 终审 | 用户原话「现在测试功能没问题了」 | 伤害倍率 / 减伤两滑块生效、结算正常 |

## 最短复现命令

```bash
# 1) 静态 / 离线回归
node 04_scripts/炁术行者/offline_regression.js
# 2) 出包（唯一合法姿势）
bash 04_scripts/炁术行者/qsxz_build_so.sh && bash 04_scripts/炁术行者/qsw_build_star.sh
# 3) L2 机器预检
bash 04_scripts/_common/post_install_precheck.sh --game 炁术行者
# 4) 游玩取证（人工发现问题时喊「标记」，AI 执行 mark）
bash 04_scripts/_common/flight_recorder.sh start --pkg com.h5wrapper.app
```

## NOT PROVEN

- **服务端结算复核**：历史实测 `dr=0.7` 触发 `code=9988` 拒判（`FightAction_fightOver_14`）⇒ 服务端对受伤总量有阈值；本次交付档位（伤害倍率 1-20 / 减伤 0-50%）未做全档位服务端验证。
- **后台卡密授权链**：后台 `qsxz` 建游戏 / 发卡由用户操作，真实发卡后的卡密登录链未在本轮取证。
