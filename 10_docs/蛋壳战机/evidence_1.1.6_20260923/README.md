# 证据索引：蛋壳战机 1.1.6 星光包（2026-09-23）

真机 `f9a116a2`（OPPO OPD2404 / OnePlus OP5D77L1, Android 16, arm64），包名 `com.survivor.acecn`。

本轮共两件事：**① 偷盗技能从「纯 UI」修成真生效；② 修掉随之引入的「启动即闪退」。**
完整 logcat 按 AGENTS 约定不入库（体积 0.4 ~ 2.7 MB），只保留决定性行。

## 本目录文件（入库）

| 文件 | 行数 | 覆盖内容 |
|---|---|---|
| `decisive_pre_fix_coldstart_20260923.txt` | 11 | **修复前**冷启动 pid 21957：4 个 hook 装完、`AddSkillToCharacter resolved` 后紧跟 `[Il2Cpp] api loaded`，**`lib ready` 那行缺失** ⇒ 进程在 `ResolveSkillAux()` 内静默 SIGSEGV |
| `decisive_fixed_run_20260923.txt` | 29 | **修复后**冷启动 pid 27511：同样 4 个 hook，但多出 `[dkzj] lib ready; hooks file-offset + by-name fallback` 与 `[dkzj] 初始化成功`；含 7 条启动期 `ShowSkills count=0` 告警、8 条 `attack x100 hpdelta … -> …`、1 条 `god blocked` |
| `decisive_runtime_20260923.txt` | 41 | **局内实测**（转抄）：pid 27511 三次 + pid 29615 六次真实三选一 `steal picked=… granted=N (of M candidates)`；6 条 `god blocked`；exit-info 反证 |

`decisive_runtime_20260923.txt` 头部写明了转抄来源与原因：设备 logcat 环形缓冲
（`-b main`）已被后续 Unity 日志冲掉，无法二次回放。原始抓取命令写在文件头。

## 完整转储（不入库，在 `09_test/蛋壳战机/evidence_1.1.6_20260923/`）

| 文件 | 大小 | 说明 |
|---|---|---|
| `logcat_all_fixed_run_20260923.txt` | 2.7 MB | 修复后全 buffer（`-b all`）转储 |
| `logcat_coldstart_20260923.txt` | 0.4 MB | 修复前冷启动（20:36） |
| `logcat_fixed_run_20260923.txt` | 0.34 MB | 修复后仅 pid 27511 |
| `cold_start_203606.log` | 0.4 MB | 同上，另一份副本 |

## 崩溃取证结论（详见 `../故障复盘_启动闪退_1.1.6_20260923.md`）

- `dumpsys activity exit-info`：修复前 5 次全部 `reason=2 (SIGNALED)` + `status=11 (SIGSEGV)`（20:36 / 20:48 / 20:52 / 20:55 / 20:56）；修复后**无新增**，21:06:30 的 pid 27511 是 `reason=13 (OTHER KILLS BY SYSTEM) status=0`（系统回收）。
- `/data/tombstones/`：09-23 **无任何新文件**（最新 `tombstone_31` @2026-09-22 21:16）。
- dropbox：09-23 只有 20:46 一条 `data_app_native_crash`（6607B），栈为 `libunity → il2cpp_init+28 → abort`，证明 `il2cpp_init` 由 Unity 渲染循环调用。
- logcat / events：**无 `Fatal signal`、无 `am_crash`、无 `am_kill`** ⇒ 典型「不可 dump 进程的静默 SIGSEGV」。

## 复现命令

```bash
S=f9a116a2
adb -s $S shell 'am force-stop com.survivor.acecn'
adb -s $S shell 'am start -n com.survivor.acecn/com.gorilla.privacystartup.PrivacyStartupActivity'
adb -s $S shell 'logcat -b main -d -v time' | grep -E '\[dkzj\]|\[Il2Cpp\]|StarryMagic|IL2CPP'
adb -s $S shell 'dumpsys activity exit-info com.survivor.acecn' | head -30
```

> 入口必须用 `-n` 显式组件；`am start -p` / `-a MAIN -c LAUNCHER` 在本机解析不到
> （`PrivacyStartupActivity` 未导出 LAUNCHER intent-filter）。
