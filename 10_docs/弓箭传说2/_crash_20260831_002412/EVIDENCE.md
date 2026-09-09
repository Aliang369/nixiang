# 弓箭传说2 闪退实锤（tombstone_02）

## 来源
- `/data/tombstones/tombstone_02`
- 本地副本：`10_docs/弓箭传说2/_crash_20260831_002412/tombstone_02.txt`

## 事实（非推断）

| 项 | 值 |
|---|---|
| 时间 | 2026-08-31 00:23:28.908 +0800 |
| 进程 | `com.xq.archeroii.ua` pid=16690 |
| 线程 | **UnityMain** tid=16817 |
| 信号 | SIGSEGV / SEGV_MAPERR |
| fault addr | `0x000657f6a9056133`（未映射） |
| x0 | `0xa90657f6a9055ffe` |
| pc (rel) | libil2cpp.so `0x3abc524` |
| BuildId libil2cpp | `bc24bd75ddc4a93542eb55cf179ef55d3dfe0990` |
| BuildId libstarry（映射） | `02e29b8b7d2a3b11b9145c1bb1b1971751348410`（00:20 新装 so） |
| Process uptime | 37s（Start proc 00:22:53 → crash 00:23:28） |

## 崩溃指令（从设备 libil2cpp 按 file off `0x3ab8524` 抽出反汇编）

```
0x3abc524: ldrb w8, [x0, #0x135]   <<< 崩溃点
```

算术核对：`x0 + 0x135 = 0xa90657f6a9056133`，与 fault 低位一致（高位/PAC 差异不改变「对脏 x0 解引用」事实）。

## 崩溃栈关键事实

- UnityMain 回溯 **#00–#14 全在 `libil2cpp.so`，随后 `libunity.so`**。
- **崩溃线程栈帧中没有 `libstarry_magic.so`**。
- 另有一线程 tid=16836 停在 `libstarry_magic.so` + `usleep`（Tick 休眠），与本次 fault 无直接栈关联。

## 与上一份 tombstone_01（22:44）对比

| | tombstone_01 | tombstone_02 |
|---|---|---|
| 时间 | 2026-08-30 22:44 | 2026-08-31 00:23 |
| starry BuildId | `4879ce09…`（旧） | `02e29b8b…`（新） |
| #00 模块 | **libstarry_magic.so** | **libil2cpp.so** |
| 含义 | 旧 so 在 starry 内崩（偷技能路径） | 新 so 崩溃点在 il2cpp 脏指针解引用 |

## 当前 log 缺口（也是事实）

- 崩溃窗口内 `adb logcat` **未抓到** 明文 `[Gjc2]` / `Mod_Menu` 行（TAG 经 `OBFUSCATE`，且缓冲可能已被冲掉）。
- 因此：**不能从 log 证明当时 feat 开关值 / hooks 是否已 Install / grace 是否到期**；只能证明进程在对局里跑了约 37s 后于 UnityMain 因脏 x0 崩在 il2cpp。

## 判决（prove-it）

```
CLAIM: 对局内开隐身/全屏导致的闪退，已有对应 tombstone 且可定位到具体指令
VERDICT: PROVEN（闪退事件与指令级原因）
       NOT PROVEN（尚未用开关 A/B 钉死是 SetTarget 置空 / Dobby 安装 / PushHurt 哪一条触发了脏 x0）

WHY: tombstone_02 时间与用户报告一致；指令与 x0/fault 算术闭合；栈在 il2cpp 非 starry。

NEXT PROOF:
1. 清 logcat 后装带明文 tag=StarryGjc2 的 so，只开隐身再抓一轮
2. A/B：InstallHooks 后 SetTarget 仅转发（不置 null、不 ClassName）— 若仍崩 → 安装/Dobby 侧；若不崩 → 再开置 null
```

## A/B so 已装 (00:30)
- `kGjc2BattleHooksEnabled=false`（无 Dobby / 无 HotFix 扫描）
- 开关日志 tag=`StarryGjc2`
- 验证：so 含 StarryGjc2 / continue A/B

## A/B2 隐身-only (01:55+)
- 用户确认 A/B1 hooks=0：不闪退、无效果
- 已装：仅 SetTarget/TrySetTarget + thread_attach；grace=8s；全屏/偷技能不装钩
