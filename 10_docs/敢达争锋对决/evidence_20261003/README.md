# 证据索引 · 敢达争锋对决 9.3.0 斌哥改包抓点（2026-10-03）

> 关联文档：[抓点总结_斌哥mod_20261003.md](../抓点总结_斌哥mod_20261003.md) · [斌哥hook锚点_20261003.json](../斌哥hook锚点_20261003.json)
> 设备：MuMu `emulator-5554`，包 `com.kingnet.gundam.mg.normal` 9.3.0/vc830，取证 pid 5716。

## 结论

斌哥对本包只有 **4 个内联钩**（全在 `libil2cpp.so` 的 RX 段，每条覆盖方法序言第 1 条指令，4 字节 `B <imm26>`）：`xRole::CalcDamage`@RVA `0x3DB99CC`（输入伤害）、`xRole::UpdateAI`@`0x3D9CF54`（超级定怪）、`Time::get_deltaTime`@`0x67BE180` + `Time::set_timeScale`@`0x67BE458`（全局加速，后者纯透传）。全 RX 段逐字节内存 diff **恰好这 4 处**，与壳 flag 页串表里的 4 组「dll+类+方法」描述**一一对应**；跳板页 stolen prologue、跳转目标 = `base+RVA+4`、diff 补丁位置**三方吻合** ⇒ 抓点完备、无遗漏、无多余。

## 决定性证据

| 证据文件 | 关键内容 | 决定性 |
|---|---|---|
| `handler_disasm.txt`（7,640 B） | 4 个 handler 的完整反汇编 + stub 的 `adrp/add` 目标解析 + flag 槽位实取值 | ★ 功能语义的**唯一直接证据**（本包壳无明文符号，只能靠反汇编） |
| `flagpage_227b8000.bin`（233,472 B） | flag 页 live dump（与 `07_patched/敢达争锋对决/shell_rw.bin` byte-diff = 0） | 证明槽位/跳板指针**稳定非瞬态**；`+0x4`=1 / `+0x8`=1 / `+0x232c`=0 / `+0x2390..0x23a8`=跳板指针 |
| `tramp_7a8c46e83000.bin`（4 KB） | 跳板页：stolen prologue 4B + `ldr x17,#8;br x17` + 8B 绝对目标，stride 0x18 | ★ 目标恒为 `0x400025000000+RVA+4` ⇒ 与 diff 位置、dump.cs RVA **三方闭合**（证明补丁只覆盖 4 字节且原函数可 100% 还原） |
| `stub237bf000.bin`（4 KB） | `0x4000237bf000→0x4000213e770c`、`+0x0c→0x4000213e774c`，`+0x18` 起 `udf #0` | 证明 H2/H3 是**二级跳**（B → 12B stub → handler） |
| `memdiff3.py`（2,718 B） | guest 镜像 vs 磁盘 so 按 `PT_LOAD` 4 字节 diff：R 段 0 处 / **RX 段 4 处** / rw 段 147,779 处（重定位噪声） | ★ 「只有 4 个钩」的**定量证据** |
| `bdecode.py`（1,160 B） | capstone 解码 4 条补丁 = `B <imm26>` → handler 地址 | 补丁形态定案（4 字节内联跳，非疾风骑士的 16 字节 `LDR X17;BR X17`） |
| `findname.py`（841 B） | `script.json` 的 `ScriptMethod[].Address`（十进制 = RVA）反查 4 个 RVA → 方法名 | 锚点与符号的绑定 |
| `shell_rw_strings.txt`（2,059 B） | flag 页串表 64 条：菜单 URL `game/578.html`、md5 门禁串、`/proc/self/maps`、`/data/local/tmp/fha`、`com.fate.AGTool`、4 组 hook 描述、`Category_1`/`Toggle_000×4`、Java 桥签名 | ★ 4 组 hook 描述串与 diff 出的 4 个钩**双向互证**（排除「存在未安装的钩=遗漏抓点」的担忧） |
| `menu_exp_decode.txt`（533 B）+ `menu_decode.js`（4,337 B） | exp blob 712 字符 → 明文 319 字符；`input_0_输入伤害` / `btn_1_超级定怪` / `input_2_全局加速` / `// input_3_未添加` | 菜单 3 功能 ↔ 4 个钩的映射闭合 |
| `menu_578.html`（6,902 B）+ `menu_app1.js`（122,962 B） | 云菜单页与壳脚本（`strencode()` 解码链、40 个函数面） | 菜单来源与运维面 |
| `maps_regions.txt`（4,332 B） | libSDKloading / handler 段 / flag 页 / stub 页 / libil2cpp guest+host / 跳板页的区间与权限 | 地址口径（guest `r--p` 无 X 位）与页定位 |
| `collect_evidence.py`（4,421 B） | 上述 handler/stub/flag/串表取证的一键脚本 | 可复现性 |

## 复现命令

```bash
# 0. 前置（多设备必须 -s）
adb -s emulator-5554 root
adb -s emulator-5554 shell ps -A | grep kingnet        # => pid 5716

# 1. guest 别名镜像（07_patched/敢达争锋对决/memdump2/）
python memdiff3.py            # RX 段应得恰好 4 处差异（各 4 字节）
python bdecode.py             # 4 条 B 指令 → handler 0x4000213E7684 / 76B4 / 770C / 774C
python findname.py            # RVA → xRole::CalcDamage / xRole::UpdateAI / Time::get_deltaTime / set_timeScale

# 2. 三块 live 页（Windows 下十进制换算见下）
adb -s emulator-5554 shell "dd bs=1 skip=<DEC> count=<N> iflag=skip_bytes,count_bytes of=/data/local/tmp/x.bin if=/proc/5716/mem"
#  stub 页 0x4000237bf000(4K)=70369339502592   flag 页 0x4000227b8000(233472)=70369322696704
#  跳板页 0x7a8c46e83000(4K) 换算：[Convert]::ToInt64('7a8c46e83000',16)

# 3. handler / stub / flag / 串表取证
python collect_evidence.py

# 4. 菜单 exp 解码（node + vm 沙箱假 DOM，直调 app1.js 的 strencode）
node menu_decode.js

# 5. 斌哥签名判据
keytool "-J-Duser.language=en" -printcert -jarfile apk/base.apk
```

地址口径：**guest VA = `0x400025000000` + RVA；file_off = RVA − 0x4000**。补丁写在 guest 别名，host 别名（`0x7a897703c000`）与磁盘逐字节相同。

## 未证明项

- 未做人工交互交叉验证（仓库硬约束：真机界面操作由用户执行、禁截图/点屏）：开关后 `flag+0x232c` 0→1、输入值后 `flag+0x0004`、加速值 `flag+0x0008` 的变化未实测，仅由 handler 反汇编推定。
- 原版未获取 ⇒ 「原版与斌哥包 so/metadata 是否逐字节相同（RVA 零重定位）」未证，是移植前置条件。
- `flag+0x0000 = 1` 语义未证。
- 22 MB anon payload 除 4 个 handler 外的内容未分析；反外挂探测（`/data/local/tmp/fha`、`com.fate.AGTool`、`/proc/self/maps`）触发时机与后果未验证。
- `classes4.dex` 的菜单 Java 侧未反编译细读（仅字符串级确认）。

## 原始资料

- APK：**已按「APK 不留本地」清理**（原 297,860,686 B，md5 `3e6d32482af8c3e2388b9f52aabc029b`；可从设备重取：`/data/app/~~J-LOiKlB30u55KGGT1Dc0A==/com.kingnet.gundam.mg.normal-oUY4tYTfYQAncaf_OUyaPA==/base.apk`）；其 `classes4.dex` 保留在 `07_patched/敢达争锋对决/apk/classes4.dex`
- so / metadata：`07_patched/敢达争锋对决/work/libil2cpp.so`、`lib/global-metadata.dat`（28,788,552 B）
- Il2CppDumper 产物：`07_patched/敢达争锋对决/dump_out/`（dump.cs 46,276,959 B / 1,358,435 行，script.json 127,816,238 B）
- 运行态镜像现场：`07_patched/敢达争锋对决/memdump2/`（含 il2 / sdk / anon 逐段镜像与 segs.txt）
- 设备侧残留（上一轮同游戏，未清理）：`/data/local/tmp/{dbsm.sh,binge_memdump/,anon*.bin,il2cpp_mem_*.bin,il2cpp_data.bin,fs-x86_64}`
