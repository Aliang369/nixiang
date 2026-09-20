# 版本更新重定位方法论（小版本更新后功能失效怎么办）

> 📍 `re-implement/version-update-relocate.md`
> 🎯 游戏小版本更新后原挂点失效（`hits=0` / 功能无效）时的标准流程，按 A→B→C→D 顺序走
> 📖 蓝本：音舞光年 0.300.41→0.300.42（invoke 桩整体搬迁，2026-08-22）；弓箭传说2国际版 1.0.1（方法缺失，2026-08-23）；无悔华夏清数据节（导出剥离，2026-08-28）；我的勇者 7.6.1（锚点字段口径错误 + 静默退回按名，2026-09-20）

---

## 0. 原则：锚定「语义 + 运行时证据」，不锚定「上一版的 file_off」

Unity/HybridCLR 重编 `libil2cpp.so` 时，内部解释器 helper 会**整体搬迁**（下表为音舞光年实例，用来说明「搬迁」形态，数字不是通用常量）：

| 地址 | 0.300.41 | 0.300.42 磁盘 | 含义 |
|------|----------|---------------|------|
| file `0x2660BDC` | 116B HybridCLR invoke 桩 | **首条指令 = RET** | 旧槽位已废弃，不是入口 |
| file `0x2660BE0`（桩内 +4） | 同一函数内 | **另一段无关代码** | 误把「+4」当新入口 → 钩错 / `hits=0` |
| file `0x2BAC188` | 不存在 | 与 41 桩 **97% 字节相同** 的 116B 函数 | 真正的新桩 |

Il2CppDumper 看不到热更玩法方法（`SendScore` / `BubbleConfig` 等）时，**不能**指望 dump.cs 给出新 RVA。理解调用链（菜单 ON → invoke 桩 4 参 → `a1=MethodInfo*` → 方法名过滤 → 改参 → orig）比记一个 offset 更能复现功能；**换的是桩的 file_off，不是 hook 思路**。

---

## 0.1 dump.cs 地址字段口径与序言校验（2026-09-20 起）

> 来源：我的勇者 7.6.1（`10_docs/我的勇者/分析_战斗_20260906.md` §13.2；候选卡 `10_docs/_templates/经验_dump偏移口径与序言校验_20260920.md`）。此前 7.6.0 的锚点整版口径错误却「功能正常」，潜伏一个版本才被换版本时发现。

- **字段口径**：`dump.cs` 每行给 `RVA: 0x… Offset: 0x… VA: 0x…` 三个值。自写 inline hook / `FileOffToVa` 收的是 **file offset**，即 **`Offset` 字段**（本例 `RVA − Offset = 0x4000`，来自首个 PT_LOAD 的 `p_vaddr − p_offset`；**0x4000 不是通用常数**，换 so 先按程序头实测，见 `hook-rules.md` §6）。
- **序言期望值按小端读**：`Offset` 处 4 字节 `eb 2c bb 6d` → 期望值写 `0x6DBB2C6D`（`memcpy` 成 `uint32_t` 的结果），**不要**写字节序反转的形式。
- **锚点表绑定 so**：锚点只在对应 so 哈希下有效，表里必须带 `libil2cpp.so` 与 `global-metadata.dat` 的 SHA-256（`hook-rules.md` §23）。
- **填错不报错、只静默退回**：RVA 快路径序言校验不过时框架自动转按名（`hook-rules.md` §21 行为型双路径），功能照常生效。因此
  - 「功能生效」**不能**作为口径正确的证据；
  - 唯一判据 = logcat 里该锚点打印的是 `@rva` 还是 `@by-name`（例：`[Wdyz] HeroSettle hooked @rva 0x45F2308`）；
  - 版本更新后若始终是 `@by-name`，先怀疑字段口径与序言值，再怀疑运行时 so 与静态 so 不同源（`/proc/<pid>/maps` + 哈希比对）。
- **出包前自检**：`xxd -s <Offset> -l 4 <libil2cpp.so>` 与锚点表逐项对齐（脚本内做，别靠肉眼）。

---

## 1. 阶段 A：先判断是「挂点搬迁」还是「路径变了」

进关开功能，抓 `adb logcat`（本游戏 tag）：

| log 表现 | 判定 |
|----------|------|
| `hooked invoke @ …` 有、`hits=0` | 高概率**仅 offset 搬迁** → 走阶段 B |
| 连 `hooked` 都没有 | maps 解析 / so 未加载 / 功能未开 / jniLibs 没刷 |
| `hits>0` 但局内 MISS | **参数布局变**（查 `+0x8` / `params[0]`），不是桩地址 |
| 旧档全失效且包内 so 不是本游戏 | 先查 jniLibs / 分发自检，别急着重定位 |

可选：`STARRY_TRACE=1` 编译，对局 30s，确认目标方法仍走原调用形态（比盲猜快）。

## 2. 阶段 B：静态 diff 找新 file_off（首选，不必进游戏）

1. 取**上一有效版**与**新版**的 `lib/arm64-v8a/libil2cpp.so`
2. 从旧版已知桩截取函数体（如 116B）
3. 在新 so 中做**子串 / 模糊匹配**（41@`0x2660BDC` vs 42@`0x2BAC188` = 113/116 字节一致）
4. **校验候选防误匹配**：
   - prologue 字节（如 `fe0f1df8…`）
   - 函数体内 BL 条数 + 末尾 RET（音舞 0.300.42 中「116B + 3BL + RET」**唯一**命中）
   - BL 目标是否也成对搬迁（41: `0x2664c50` → 42: `0x2bb01fc` 等）
5. 可写一次性脚本「两版 APK 抽 so → 导出候选 file_off」（参考 `04_scripts/_common/diff_il2cpp_mem.py`），**不进 mod 热路径**

## 3. 阶段 C：改 mod 前必须 runtime 验收

1. 只改 `FILE_INVOKE_xx` 常量（或 OTA `invoke_file`，Hzsj 已有先例）
2. `*_so_only.sh` + 进关实测
3. log 必须出现方法名命中 + `#N` 计数**随对局增长**；`hits>0 仍全 MISS` → 查 pre/post 时机
4. 验收过再关 Trace 出正式 so

## 4. 阶段 D：B 找不到同型函数 → 是「路径变了」

说明 HybridCLR 换了调用路径（不是简单搬迁），回到：

- `StarryTrace` 对局抓目标方法真实 caller / 参数布局
- 或新斌哥包 memdiff 找新跳板
- **禁止**直接上 §5 禁区清单

---

## 5. 禁区清单（历次翻车实锤，禁止重复）

| 做法 | 为何错 |
|------|--------|
| 沿用旧 file_off / 把「桩+4」当新入口 | 对局 Trace `invoke=0`；搬迁不是 +4 |
| `il2cpp_runtime_invoke` 过滤目标方法 | HybridCLR 热更方法不走导出，`score=0` |
| types 表 **MethodPointer** 白名单 | 对局 `patch=0`；且 MP 常为 invoke 4-arg，native 三参钩 → SIGSEGV |
| API 解析的 `MethodInfo*` 匹配 | 运行时 invoke 里 **MI 指针 ≠ API 解析结果** |
| Dobby 钩 `il2cpp_method_get_name` 导出 | **SIGILL**；只能在 invoke 桩内部直调 |
| 无过滤钩全局 invoke | 每秒数千次；必须 `a1=MethodInfo` + 方法名过滤，`SafePtr` 后再改参 |

## 6. 方法论边界

| 做法 | 评价 |
|------|------|
| 发版前脚本 diff 两版 il2cpp，人工确认 file_off | ✅ 推荐；可复现、可写进文档 |
| OTA 下发 `invoke_file` 常量（仍须先定位） | ✅ 可选；Hzsj 先例 |
| mod 运行时全 so 扫 prologue | ⚠️ 仅在多次验证签名唯一后考虑 |
| 无 diff 无 Trace 硬编码「小版本不变」 | ❌ 音舞 41→42 已翻车 |
