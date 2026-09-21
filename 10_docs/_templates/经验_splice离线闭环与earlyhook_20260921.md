# 经验候选卡：splice-closure-and-early-hook — JS splice 的离线闭环必须编译真源码；early hook 超时会丢反篡改补丁

> 默认状态：候选、未审批。一次成功不是通用规律；历史转录注明来源日期及“非本次实测”。本卡不自动授权修改 skill。

## 1. 来源、样本与授权

- 创建日期 / 作者 / 来源日期 / 本次实测或历史转录：2026-09-21 / 本会话 / 2026-09-21 13:09 真机实测（两次发现均为本次实测）
- 关联游戏状态 / 实验记录 / 原始证据路径与定位：`10_docs/超维进化/热更重校_20260921.md` §0、§0.1；`10_docs/超维进化/evidence_20260921_hotfix/README.md`
- 样本路径 / SHA-256 / 对照样本 hash：热更 `game.js` 2,767,392 B `2fa45bc4…`（0.3.5）；内置档 2,202,886 B `f1839bc2…`；splice 产物 2,769,947 B `34705171…`
- 包名 / versionName / versionCode / 构建提交 / 工具版本：`com.grinning.beta.yszl` / 2.2 / vc12 / 无提交（未 git）/ ndk 27.0.12077973、clang++ (host)、node（仅 --check）
- 环境：设备 f9a116a2 OPD2404 root；Cocos Creator 2.x JS（`libcocos2djs.so`）；星光菜单 so + classes4.dex 同批；干净卸载重装后首启
- 授权范围 / 授权记录 / 未授权动作：用户授权本机本包测试与出包装机；用户明确「设备上的测试我自己来，你看日志即可」⇒ 未做游戏内数值实测

## 2. 观察、解释与候选经验

- 观察事实（不夹带因果解释）：
  1. 手写 Python 版 splice 镜像对同一输入给出 `delta +2423`，真机运行时日志为 `2767392→2769947`（`+2555`）；两者产物 `node --check` 均 PASS。把 `CwjhMod.cpp` 的 `ReplaceAll`+`SpliceCombatJs` 抽出来用 clang++ 主机编译后运行，得到 `+2555`，与真机一致。
  2. 干净安装后首启停在「用户协议和隐私政策」弹窗约 60 s，日志出现 `[Cwjh] early hook timeout`，且**没有** `bomb/kill wrapper neutralized` 两行；splice 生效约 9 s 后进程消失（`WINDOW DIED`，无 Java 异常，`logcat -b crash` 为空）。同意隐私后再次启动 ⇒ `early se hook ok at iter=189` + 两行 neutralized + 进程 60 s+ 存活。
- 候选解释 H1 / 替代解释 H2：
  - 发现1：H1 = 手写转写引入等长级括号错误，语义变了但语法仍合法（`if("a","b",9&&x){…}` 是合法逗号表达式），故唯一可靠闭环是编译真源码。H2 = 设备上运行的 JS 与磁盘文件不同（已排除：运行时 `len 2767392` 与磁盘逐字节同长，且主机编译真源码复现 +2555）。
  - 发现2：H1 = early hook 是安装反篡改中和的唯一路径，其超时导致漏装，游戏反篡改随后自行 kill。H2 = MOD 注入本身崩溃（已部分排除：无 tombstone/无 Java 崩溃、重启后同样注入存活）。仍有 NOT PROVEN：kill 的确切触发点未取证（无 tombstone 可读）。
- 拟提炼的经验（带前提、动作、预期结果，不写无条件定律）：
  - 前提：任何「按锚点改写宿主 JS/字符串」的 splcie 型 MOD，其离线校验脚本是被维护的转写副本。
  - 动作：不要手写镜像；改为自动抽取真实现（函数体 + 相关宏）→ 主机编译器编译 → 对同一输入执行，比较 `delta` 与产物 hash。
  - 预期结果：离线 delta 与运行时 splice 日志 delta 逐字节相等；不等即说明转写或锚点有偏差，先查等长级语法合法错误。
  - 附带：`node --check` 只能证明语法，不能证明语义；对 `callStaticMethod(...)` 一类包裹，漏一个 `)` 仍能通过。
  - 附带：`splice` 类 MOD 启动自检若出现 `early hook timeout`，先重启游戏再判定功能是否失效；不要把该次进程死亡算作 MOD 崩溃。
- 适用范围：Cocos 2.x JS 明文 `game.js` 锚点 splice（星光系列 so）；Android + root/注入；需要能主机编译该实现（纯 std::string 逻辑或等价可抽取）。
- 不适用范围 / 已知例外：IL2CPP/Unity 内存 patch 不适用；抽取依赖「函数体不引用 Android/JNI 符号」——若引用需补 stub；弹窗导致 early hook 超时只对「启动即弹窗/需交互」的游戏成立。

## 3. 反证、失效与最小区分实验

- 可推翻该经验的观察或反例：出现「手写镜像 delta 与真机一致」而真源码编译不一致的样本；或 `early hook timeout` 后进程仍长期存活的样本。
- 已收集反证及其证据位置：无（本次两次观察同向）。
- 失效条件：实现改为大量依赖平台 API（无法主机抽取）、日志不再打印 splice delta、或菜单把反篡改中和改为独立于 early hook 的路径。
- 失效后的处置：降为待验证 / 撤销 / 替代卡（链接），保留历史。
- 最小区分实验：对同一 `game.js`，A 组用手写镜像、B 组用主机编译真源码，比较 `delta` 与产物 sha256（本次已跑，B 与真机一致）；early hook 组：A 组首启停在弹窗、B 组先点同意再启动，比较是否出现 `neutralized` 两行与进程存活时长。
- 停止条件：证据达标、出现反证、超出授权、两种方法仍无进展或预算耗尽时如何停止。

## 4. 证据门禁

| CLAIM（限定范围） | EVIDENCE（可定位原始记录） | VERDICT | 缺口 / 反证 |
|---|---|---|---|
| 主机编译真源码的 delta 与真机运行时 splice 一致（cwjh 0.3.5） | 设备日志 `combat splice #1 len 2767392→2769947`；`scripts/cwjh_splice_hostsim.sh` 输出 `delta=+2555` | PROVEN（样本内） | 仅 1 个游戏 1 个版本 |
| 手写 Python 镜像会把语义错误藏在合法语法里 | `superseded_handwritten_splice_sim.py` 输出 +2423；`node --check` 两版均 PASS | PROVEN（样本内） | 未做多语言/多实现对照 |
| `early hook timeout` ⇒ 反篡改中和缺失 | 13:07 日志无 `neutralized` + 9 s 后 `WINDOW DIED`；13:09 有 `neutralized` + 60 s 存活 | NOT PROVEN（相关性有，kill 触发点未取证） | 无 tombstone；未做「有 timeout 但仍存活的对照」 |

VERDICT：PROVEN / REFUTED / NOT PROVEN。明确区分样本内观察、因果解释与跨样本推广；未完成推广验证时不得声明普遍适用。

## 5. 审批与生命周期

- 证据状态：样本内支持（第 3 条待补证）
- 审批状态：未申请
- 申请写入目标 skill 文件与章节（仅提案）：`skill/re-implement/hook-rules.md`（splice 类变更的离线闭环要求）、`skill/starry-mod/SKILL.md`（出包后启动自检判读：`early hook timeout` 处置）
- 拟修改内容与适用限制：仅加「离线闭环须编译真实现」与「early hook timeout ⇒ 重启再判」两条，不写成无条件定律
- 审批人 / 日期 / 明确批准记录 / 批准范围：
- 落地状态：未合并
- 替代卡 / 被替代卡 / 重新验证触发条件：下一个 splice 型游戏出包时回归
- 下一步责任人 / 最小动作 / 状态索引链接：用户审批；`10_docs/_state/current.md` 超维进化行 → `games/超维进化.md`

审批前不修改 skill；已批准不等于已合并。反证或失效时即时同步状态并保留证据，不能把候选条目长期伪装成已验证规则。
