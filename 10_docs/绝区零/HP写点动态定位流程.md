# 绝区零 3.3.0 — 实体 HP 写点动态定位流程（单次战斗）

> 目标：抓到「谁在写玩家 HP」的那条写指令。该写点即伤害结算写回函数，是无敌（写回旧值）与
> 伤害倍率（改写入值）的统一挂点。
> 工具：`04_scripts/绝区零/zzz_frida_tool.py`（`scan` / `read` / `watchvals` / `watchmem`）。
> 前置结论来自 `版本重定位_3.3.0_20261022.md` §12：玩家 HP 字段 = 实体 `+0xA0`（u32），
> HP 地址是 GC 堆对象字段，**每次进战斗都会漂移，必须每局重扫**（旧值 `0x780c1c5630` 已失效）。
> 本轮（2026-10-23）只做工具与流程，**未 attach**；真正扫描要等主代理协调「单次用户战斗」。

---

## 0. 环境确认（每轮开工先做，30 秒）

```bash
adb devices -l
adb -s f9a116a2 shell su -c "pidof com.miHoYo.Nap"
adb -s f9a116a2 shell su -c "ps -A -o pid,name,args | grep -i frida | grep -v grep"
```

判定：

| 现象 | 含义 | 处理 |
|---|---|---|
| `pidof` 无输出 | 游戏没跑 | 请用户开游戏进到战斗（本轮不主动拉起） |
| `pidof` 有 pid，但 `dumpsys activity` 顶层是 launcher | 游戏在后台 | 请用户切回游戏前台再 attach（后台易被冻结/降频） |
| `ps` 里没有 frida-server | frida-server 没启动 | 见下方启动命令（需 root，属设备动作，先确认） |
| 两者都在 | 可以进入 §1 | — |

frida-server（本轮实测：二进制在，进程**不在**）：

```bash
# 二进制已存在：/data/local/tmp/frida-server-17（17.16.4，与本机 frida-python 17.16.4 同版本）
adb -s f9a116a2 shell su -c "nohup /data/local/tmp/frida-server-17 > /dev/null 2>&1 &"
adb -s f9a116a2 shell su -c "ps -A | grep frida"       # 复核在跑
```

> 本机已验证：`/opt/homebrew/bin/python3.11` 下 `frida.__version__ = 17.16.4`；
> `/data/local/tmp/frida-server-17 --version` 输出 `17.16.4`（版本一致，无需换 server）。
> 只接了一台设备，`--serial` 可省；多设备时 `--serial f9a116a2`。

**attach 纪律**（上一轮有此坑）：attach 前确认进程稳定、游戏在前台、当前不在过场/加载；
attach 后若游戏卡死，先 `kill` 脚本再从 `logcat`/`dropbox` 取证，不要连着 attach 第二次。

---

## 1. 定位 HP 地址（用户只配合一次伤害）

### 1.1 记下当前血量（用户报数，或先看界面）

用户报「血量 620」（**只报整数，不带小数/百分比**；护盾、上限、临时 buff 都不要报）。

### 1.2 全量扫描该数值

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  scan --value 620 --type int --max-mb 3000 --limit 300000 --state hp.json
```

要点：
- **`--limit` 必须放宽到 30 万**（§12 的教训：默认 400 会把真值截掉；620 那次只留 300 条）。
- 3 GB 扫描量约 6 秒（实测）。输出形如 `扫描 2900 MB，原始命中 1644，收敛后 1644`，状态存 `hp.json`。
- `--max-mb` 是累计扫描字节上限，按区域大小降序扫；不够就加大（默认 2000，实测给 3000 更稳）。

### 1.3 挨一下，用新血量收敛

请用户**原地挨一次普通攻击**（不要放大招、不要被连续打），记下掉血后的数值（如 557）。

```bash
# 方式 A（流程主线）：以新值全量重扫，与上次命中求交集（交集写回 hp.json）
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  scan --narrow --value 557 --type int --max-mb 3000 --limit 300000 --state hp.json
```

- A 成功时 `收敛后` 只剩 1~几个地址，且 `hp.json` 被覆盖成这批地址（下一节直接拿它当候选）。

**A 窄不下来时**（同值噪声多、交集仍有几十上百条），用 `watchvals` 让工具盯变化——
对 §12 那种 1,644 候选最有效，一次伤害就能出唯一「下降」者：

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  watchvals --state hp.json --limit 2000 --seconds 20
# 输出里找：from=620 to=557 的那一个地址（唯一「下降」的）；其余同值者不变或变大
```

候选已经很少（≤80）时也可以直接看重读值（`read` 只打印前 80 条当前值）：

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  read --state hp.json --limit 80
```

### 1.4 确认地址就是 HP（不确认就别往下走）

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  watchvals --addr 0x<候选地址> --type u32 --seconds 20
```
再挨一下 → 只有它出现 `from=旧值 to=新值`（且与界面一致）才算锁定。
§12 实测样例：`0x780c1c5630` 605 → 557，邻域 `[0,1,0,0,557,0,0,179,0]`（整数型，非 float）。
**注意这个地址下一局就作废，本节必须每次重跑。**

---

## 2. 抓写点：`watchmem`

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  watchmem --addr 0x<已确认地址> --seconds 30
```

- 这时**用户只做一个动作：再挨一下**（一次普通伤害即可）。30 秒窗口从脚本打印
  `已 arm：……` 开始算，别让用户错过窗口。
- 输出实时打印，每条命中一行：

```
  [   3.41s] op=write  addr=0x780c1c5630  by libil2cpp.so!0x14be3708  il2cpp_rva=0x14be3708
        #0  libil2cpp.so!0x14be3708
        #1  libil2cpp.so!0x14be3600
        #2  libil2cpp.so!0x188f8488
        #3  base.apk!0x872f50c
```

- 收尾行汇总：`收尾 reason=timeout：访问 N（读 a / 写 b / exec c / 其他 d）命中 h 忽略 i rearm k 次（失败 0）`
  以及 `当前值 0x... u32=<掉血后的值>`（收尾时读，安全）。

读结果的口径：
- 重装策略（工具内置）：每次命中后 `disable → enable` 重新 arm（frida 的监视器命中后不会自动续命），
  `progress` 行的 `rearm` 计数随访问增长属正常；突发访问被合并到每 `--rearm-delay`（默认 10ms）一次，
  因此极端情况下有 ≤10ms 的盲窗——伤害写回是单次事件，正常能抓到。
- **`op=write` + `addr` 等于监视地址 + `instr_rva`** → 这条就是伤害结算写回点。
  `instr_rva` 是相对 `libil2cpp.so` 模块基址的偏移（`from - module.base`），与 §7.1「离线口径 = 真机口径」
  同一套 RVA 空间：IDB 里把加载基址设 0 后直接 `g <instr_rva 的十六进制>` 即可，**不要再叠加进程基址**。
  `il2cpp_rva` 列是同一数值的另一路换算（专对 libil2cpp.base），两列一致时可信度最高。
- `op=read` 的是读血量的代码（血条/UI/读血路径），**不是**挂点。上一轮抓到的 `0x15075480`
  就是 `ldr x20,[x21,#0xa0]` 的读侧，已确认无用（且那版只记 `from` 没记 `address`）。
- `bt` 是命中瞬间的栈回溯（`Backtracer.FUZZY`，最多 12 帧，已折成 `模块!RVA`），用来找上层调用者。

### 2.1 抓不到 write 的处置（按顺序试，别反复消耗用户）

| 迹象 | 判断 | 动作 |
|---|---|---|
| `reason=timeout`、命中里有 read 无 write | 期间没发生结算，或该字段只是血条副本 | 再给一次 30 秒窗口，只挨一下；仍无 → 换法 |
| `reason=max-accesses`（访问数打满 2 万） | 同页太热（同页还有位置/朝向等热点字段），信号风暴会掉帧 | 缩短 `--seconds 10`，并把 `--max-accesses` 提到 50000；或直接走 2.2 |
| `rearmErrors` 持续增长 | enable/disable 竞争 | `--rearm-delay 40` |
| 命中里全是 `ignored` 计数暴涨、hits=0 | 页内没有对该 4 字节的访问 | 地址可能已失效（GC 搬了），回 §1.4 重确认 |

### 2.2 兜底：从读点反推

```bash
/opt/homebrew/bin/python3.11 04_scripts/绝区零/zzz_frida_tool.py \
  watch --rva 0x15075418 --seconds 30
```
`watch` 会打印 `args[0..3]`（x0~x3）。在 `onEnter` 里 x0 往往是 `this`（实体对象），
`ldr x20,[x21,#0xa0]` 的 x21 即对象指针 → 用实体对象反推「偏移 0xA0 的写入者」需要静态跟进；
若静态也找不到，再用 `watchmem --addr <x21+0xA0>`（把对象地址也一并监视）交叉验证。

---

## 3. 落点回填

拿到写点后立刻回填，不要只留在会话里：
1. `10_docs/绝区零/版本重定位_3.3.0_20261022.md` 追加 §13：写点 RVA + backtrace + 时间 + 设备/版本 + 命令原文。
2. `10_docs/绝区零/路线锁定.md`：把「无敌」「伤害倍率」的挂点标注为 `watchmem` 实测写点（附证据行）。
3. `10_docs/_state/games/绝区零.md`：状态从「数值扫描已锁 HP」推进到「写点已定位/待装机」。

---

## 4. 一页速查

```bash
export Z=/opt/homebrew/bin/python3.11
T=04_scripts/绝区零/zzz_frida_tool.py
# 0 环境
adb devices -l && adb -s f9a116a2 shell su -c "pidof com.miHoYo.Nap"
adb -s f9a116a2 shell su -c "nohup /data/local/tmp/frida-server-17 >/dev/null 2>&1 &"
# 1 扫血（用户报 620）
$Z $T scan --value 620 --type int --max-mb 3000 --limit 300000 --state hp.json
# 2 挨打后（用户报 557）
$Z $T scan --narrow --value 557 --type int --max-mb 3000 --limit 300000 --state hp.json
# 3 确认
$Z $T watchvals --addr 0x<候选> --type u32 --seconds 20
# 4 抓写点（这次窗口内只挨一下）
$Z $T watchmem --addr 0x<候选> --seconds 30
```

成本：一次战斗内 2 次伤害动作（§1.3 挨一下、§2 挨一下），符合「用户配合 ≤2 次」；
若 §1.4 需要第三次确认，改用 §1.3 方式 B 的静态判定，不额外消耗用户。
