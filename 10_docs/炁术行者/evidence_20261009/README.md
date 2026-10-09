# 炁术行者（qsxz）2026-10-09 「标记」缓解 + 伤害倍率收窄 1–5 —— 决定性证据

本文件记录本轮交付的判定链、地面真值与不可证事项。所有结论都来自**设备 logcat 实证 + 服务端下发文件实测**，不含推断。

---

## 一、用户诉求（原始口径）

- **功能已可用，新机制会「标记」账号**（用户真机反馈，原话）：

  > 真机上面有游戏，现在的问题是功能没问题，但是新的机制会被标记，你看看能不能找到标记屏蔽或者是处理了。然后伤害可以控制在1-5呢，减伤先保持。有了标记就一直提示"参数传输遇到问题，点击确认重试"，需要吧这个处理了

- **群主（后台侧）给出的判定口径**（用户转发的 QQ 群截图）：

  > 现在新的机制的，如果你输出是1000服务器允许你到1500，有一个冗余值，但超过1500就不行。因为正常玩家，在数据传送中会有许些差异，所以设置一点冗余，但开挂那种明显异常的，肯定会被检测，提示战斗失败，然后被后台标记
  >
  > 比如你打出了1e伤害，实际后台受到的数据只要达到8000w就生效，但如果后台收到的是2000w差异过大 → 就会判定外挂 → 和你多少战力打多少伤害没关系 → 之前为什么各种异常就是因为你打出1e后台没有受到1e就判定失败

- **收官原话**（用户 L3 实测通过后）：

  > 功能测试没问题，提示也没有了，这个处理可以。进行收尾+正式包+清理+git

**结论**：服务端校验的是「客户端**上报**的结算数值」与「服务端**期望**值」的偏差，容差约 +50%，**高报与低报都会被判异常**。所以解法不是「别加伤害」，而是「加伤害但**上报原版口径**」。

---

## 二、弹窗溯源（「参数传输遇到问题，点击确认重试」）

| 项 | 值 |
|---|---|
| 文案来源 | i18n key **`netcode_9998`** = 「参数传输遇到问题，点击确定重试」 |
| 近似串 | `netcode_9999` = 「数据传输遇到问题，点击确定重试」（措辞近，一并列入白名单） |
| 语言包 | `http://43.248.140.201:8888/language/zh.zip`（zlib 压缩的 JSON，206,702 B，11,431 键；`.zip` 是假扩展名，`zipfile` 打不开，需 `zlib.decompress`） |
| 相关键 | `netcode_-10` =「系统检测到游戏客户端异常，请点击确定重新启动」；`netcode_9988/-9987/9989/9986` =「战斗验证失败，请退出重试(Err:1~4)」；`netcode_179`=「战斗目标发生改变，战斗无效」；`netcode_196`=「没有该部队」 |
| 弹出位置 | `NetGameTips.responseErrorCode` 的**最后一个 else 分支**：`...:196==e?r.toast("netcode_"+e):r.alert("netcode_"+e)` |
| 触发来源 | 服务端 `FightAction` 系列 RPC 返回 `code=9998`（未打标前无、打标后**每场战斗都回**） |

**关键判定**：`netcode_9998` 这个字符串在 JS 里**不存在**（全 JS `9998` 只有 3 处命中，均为无关枚举/md5 常量/测试战斗表），它是运行期 `"netcode_"+e` 拼出来的 ⇒ 弹窗完全由**服务端返回码**驱动，客户端只能「不弹」。

---

## 三、本轮改法（逐条）

### 3.1 伤害倍率上限 20 → 5（用户明确要求「伤害可以控制在1-5」）

| 文件 | 位置 | 改动 |
|---|---|---|
| `04_scripts/炁术行者/qsw_patch_js_star.py` | HEADER `__QSW_APPLY` | `else if(k>20)k=20;` → `else if(k>5)k=5;` |
| `mod菜单/starry-mod-menu/app/src/main/jni/Games/QsxzMod.cpp` | `Qsxz_OnValueChanged` | `if (value > 20) value = 20;` → `5` |
| 同上 | `Qsxz_Tick` | `else if (k > 20) k = 20;` → `5` |
| `mod菜单/starry-mod-menu/app/src/main/jni/Games/QsxzEntry.cpp` | 功能表 | `"SeekBar_伤害倍率_1_20_1"` → `"SeekBar_伤害倍率_1_5_1"` |
| `mod菜单/starry-mod-menu/app/src/main/assets/starry/qsxz_patch.js` | `K_MAX` | `var K_MAX = 20, ...` → `5` |
| `04_scripts/炁术行者/qsxz_build_so.sh` | so 防呆串 | `chk "SeekBar_伤害倍率_1_20_1"` → `_1_5_1` |
| `04_scripts/炁术行者/offline_regression.js` | 全部用例 | 20/50 期望值重导为 5/50 |

**理由**：k=20 时上报量级是原版的 20 倍（+1900%），远超群主所述 +50% 容差；收到 5 仍超容差，但已在同量级，且真正解决靠 ⑬。

### 3.2 ★ ⑬ 新增：结算上报回流原版口径（「屏蔽标记」的实质实现）

- 锚点（唯一命中）：`n.createHurtLog=function(){return{pl:t.instance.petFontLog,hl:t.instance.heroFontLog,th:t.instance.totalHurt,tbh:t.instance.totalBeHurt}}`
- 替换为：构造**新对象**，`th`/`hl`/`pl` 各值 ÷k、`tbh` ÷(1−dr)，k=1 且 dr=0 时**字节级等价原版**：

```js
n.createHurtLog=function(){var _k=window.__QSWK||1,_d=window.__QSW_DR||0,_p={},_h={},_i;
 if(_k>1){for(_i in t.instance.petFontLog)_p[_i]=Math.round(t.instance.petFontLog[_i]/_k);
          for(_i in t.instance.heroFontLog)_h[_i]=Math.round(t.instance.heroFontLog[_i]/_k);}
 else{_p=t.instance.petFontLog;_h=t.instance.heroFontLog;}
 return{pl:_p,hl:_h,
        th:_k>1?Math.round(t.instance.totalHurt/_k):t.instance.totalHurt,
        tbh:_d>0?Math.round(t.instance.totalBeHurt/(1-_d)):t.instance.totalBeHurt}}
```

**为什么这样改是对的**
1. `createHurtLog()` 的返回值就是上报体 `fightpro`：`o.fightpro=st.instance.createHurtLog()`（@1386688 / @1464565 两份镜像）。折算它 = 直接改上报内容。
2. **只折算返回值、不动累加器**（`totalHurt`/`heroFontLog`/`petFontLog` 本身）⇒ 多次调用幂等，不会反复折算叠加。
3. 反篡改自洽：`o.worldScale=K.md5str(Ft.seedstr+o.fightpro+o.herotimes+o.pettimes)` 是在**返回值字符串化之后**参与 md5 的，所以折算后的 `fightpro` 与 `worldScale` 天然一致，不会触发校验失败。
4. 口径严格对应伤害钩子：`__QSW_DMG(v,c)` 中 `c===1`（玩家受击）走 `v*(1-dr)`、其余（敌方受击）走 `v*k` ⇒ 所以 `tbh`（我承受）÷(1−dr)、`th`/`hl`/`pl`（我打出）÷k。

> ⚠️ 这条同时说明了**为什么减伤也必须参与折算**：`dr=0.7` 时 `tbh` 只有原版的 30%，服务端一样算出「差异过大」。历史实测 `dr=0.7` 曾收到 `code=9988 拒判` 正是同一机制。

### 3.3 ⑫ 新增：netcode 弹窗白名单静默

- 锚点：`196==e?r.toast("netcode_"+e):r.alert("netcode_"+e)},e}())`
- 替换：中间插入 `window.__QSW_NETGUARD(e)?window.__QSW_LOG({ev:"netalert_suppressed",code:e}):`
- 白名单 `w.__QSW_NETSKIP={9998:1,9999:1,9997:1}`，**其余错误码照旧弹窗**（不吞真实错误）；`<0 || 9995` 致命分支（异地登录 / 客户端不是最新版本）**刻意不动**，那里必须 alert+restartGame。

### 3.4 ⑨ 重做：被标后不再弹 -10 也不再重启

原来 ⑨ 只中和了 hurtCheck 上报，但**保留了** `m.alert("netcode_-10",(function(){l.restartGame()}))` ⇒ 被标账号每场都弹「系统检测到游戏客户端异常」并重启。改为与「接受」分支完全一致：

```
old: f.http.post(p.apipath+"c=cheat&a=hurtCheck",{...},(function(t){m.alert("netcode_-10",...)}))
new: (window.__QSW_LOG({ev:"hurtcheck_blocked",hurtRate:t.result.hurtRate}),r.applyFightResultData(t),e(t))
```

不弹窗、不重启，且结算 promise 正常 resolve（若改成 reject 会以 `await` 抛出「结算响应异常」，比原来更糟）。

### 3.5 ⑥ 锚点重导

服务端 2026-10-09 再次重压缩，锚点局部变量 `b`→`y`、resolve 回调 `a(t)`→`e(t)`：

```
old: !t.result.isReject||b?(r.applyFightResultData(t),a(t)):
new: !t.result.isReject||y?(r.applyFightResultData(t),e(t)):
```

（`y` = `t.DEBUG` 常量，生产环境恒 false，**不能**靠它绕过 isReject。）

---

## 四、地面真值（设备 `f9a116a2` 实测）

**记录仪** `09_test/炁术行者/_recorder_20261009`（2026-10-09 18:22:21 → 18:34:13，11 分钟，`logcat.txt` 5,206,943 B）

| 断言 | 实测 | 结论 |
|---|---|---|
| `netcode_9998` / 「参数传输」出现次数 | **0** | 弹窗已消除（用户主诉解决） |
| `FATAL EXCEPTION` / `beginning of crash` / `tombstone` | **0** | 无崩溃 |
| 菜单 spec | `"SeekBar_伤害倍率_1_5_1"` | 滑条已达新上限 |
| 用户实际拖到的值 | `apply k=5 dr=50 live=1` ×2、`k=4 dr=50`、`k=2 dr=50`、`k=5 dr=46`、`k=1 dr=0` | 值域全部 ≤5 |
| 心跳 | `beat k=5 dr=50 live=1` ×65、`beat k=1 dr=0 live=1` ×61 | 投值通路稳定 |
| 授权 | `live:1` 全程 | 卡密已持久化 |
| 注入链 | `QSW_MOD : serving qsw/main.js` ×2 | 包内补丁版主 JS 被接管 |

**用户 L3 结论**（原话）：「功能测试没问题，提示也没有了，这个处理可以。」

---

## 五、产物与门禁

- 正式包 `08_output/炁术行者/炁术行者_1.0_星光版.apk`：**6,833,647 B，md5 `7cf4b27b8865b64bedabd20bd8853647`**；签名证书 SHA-256 `42:6F:A3:FB:6B:14:09:E0:B2:E8:4E:9C:DB:EF:A8:63:A0:87:34:6D:87:88:46:E6:32:80:DB:3C:15:79:2B:88`（与设备已装包同签）
- 包内不变量：`assets/qsw/main.js` md5 `803409037afd31ba080f22d3e88b622c`（5,401,818 → 5,404,066 字符，delta +2248）· `lib/arm64-v8a/libstarry_magic.so` md5 `b84adaface6e03bbc2a6e49be47a3256` · `assets/starry/qsxz_patch.js` md5 `a63981fa584bdce4690d6df9215332ba`
- 包内符号核对：`__QSW_NETGUARD`×2、`__QSW_NETSKIP`×2、`netalert_suppressed`×1、`hurtcheck_blocked`×1、`createHurtLog=function(){var _k`×1、`k>5)k=5`×1
- 门禁：`node 04_scripts/炁术行者/offline_regression.js` → **PASS=19 FAIL=0**；`pre_install_gate.sh` → **PASS=5 FAIL=0**；`verify_installed_apk.sh` → 设备 base.apk md5 == 本地产物 ✓；`post_install_precheck.sh` → **PASS_TO_HUMAN**
- 设备：`lastUpdateTime=2026-10-09 18:18:50`（覆盖安装，保留卡密数据）

---

## 六、最短复现命令

```bash
NIXIANG=/Users/aliang/Documents/nixiang; cd $NIXIANG

# 1) 补丁生成器只读体检（14 锚点必须全 OK(1)）
curl -fsS http://43.248.140.201:8888/assets/main/index.js -o /tmp/qsw_now_index.js
python3.11 04_scripts/炁术行者/qsw_patch_js_star.py --src /tmp/qsw_now_index.js --dst /tmp/qsw_probe.js

# 2) 离线回归 + 重编 so + 出包
node 04_scripts/炁术行者/offline_regression.js
bash 04_scripts/炁术行者/qsxz_build_so.sh
QSW_FETCH=1 bash 04_scripts/炁术行者/qsw_build_star.sh --install

# 3) 装机闭环 + 机器层预检
bash 04_scripts/_common/verify_installed_apk.sh f9a116a2 com.h5wrapper.app \
     08_output/炁术行者/炁术行者_1.0_星光版.apk --su
bash 04_scripts/_common/post_install_precheck.sh --game 炁术行者 --serial f9a116a2 --pkg com.h5wrapper.app
```

---

## 七、NOT PROVEN（明确未证实）

1. **「标记」是服务端账号级状态，客户端无法解除。** 本轮做的是「不让它触发」（⑬ 回流 + k≤5）+「触发后不弹窗」（⑫）。**已被打标的账号服务端可能仍继续返回 9998** —— ⑫ 只保证不弹窗，不代表账号恢复清白，也无法验证后台标记是否已清除。
2. **⑬ 的实际防标效果未做服务端侧验证。** 用户 L3 体感「没问题、不弹窗」是客户端现象；服务端是否仍记录异常，本轮无观测手段。
3. **障碍战 `FightAction_barrierUpdateTroop` 的历史 `code=9998`（2026-10-08 诊断期 k=20/dr=50 下 2 次）未复现也未归因**；本轮 11 分钟记录仪内 0 次。
4. **⑫ 静默的副作用未验证**：若 9998 确实代表「数据真的缺失」，表现会从「反复弹窗」变成「无提示卡住」。本轮用户实测未出现，但属未证。
5. **`device_id` 两处不一致**（10-07 起遗留）：sdcard/tmp 种子 `dev_760f1cc7de536cd8c34dd8f79c6f62b4` vs 应用内 `auth_device_id=dev_22ed5cb056092cd538058ae406bd81c5`（`source=cached`）—— 本轮未动，卡片授权未被拒故未复现。若将来卡密被拒，跑 `bash 04_scripts/_common/ensure_starry_device_id_seed.sh` 统一种子后重启游戏。
