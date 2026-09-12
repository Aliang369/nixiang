# 弹力果冻 TapTap 2.4.0 — HybridCLR 跳板重定位（`0xBAAB0C` 复核）

> 2026-09-11 · 输入：MuMu 实装 TapTap 包反编译产物（`06_dumped/弹力果冻/taptap_2.4.0/`）
> 结论：**`0xBAAB0C` 不可静态确认复用**；并且**建议根本不再硬编码偏移**，改运行时按名解析（见 §5）
> 证据分级：**PROVEN（可复核）** / **推断** / **NOT PROVEN** / **FAILED**

---

## 1. 先钉坐标规则（per `skill/re-extract/hybridclr-runtime.md` §坐标规则）

TapTap 版 `libil2cpp.so`（58,291,656 B, md5 `c067234d6526cd247691598e44aa5825`）的段布局：

| 段 | VMA | size | 说明 |
|----|-----|------|------|
| `.text` | `0x865040` | `0x3d9c3c` | → 结束于 `0xC3EC7C`；**运行时代码（含 HybridCLR 解释器）** |
| `il2cpp` | `0xC3EC7C` | `0x1bc58cc` | → 结束于 `0x2804548`；**AOT 生成的游戏代码** |
| `.rodata` | `0x2804550` | `0x386f1d` | 字符串/常量 |

**第一个 LOAD 段 `off=0x0 / vaddr=0x0, align=2**16`** → 在该 r-x 段内 **file offset == VA**。

> 与同工作区其它游戏的差异：独奏骑士 `RVA 0x20b840c / file 0x20b440c`、魂坠深境 `RVA 0x28BD0B0 / file 0x28B90B0` 都是 **差 0x4000**；弹力果冻 TapTap 版是 **off == VA**。**换算必须逐包核对，不能套用常数。**

`0xBAAB0C` < `0xC3EC7C` → 落在 **`.text`** 内（运行时区，非 AOT 生成区）。

---

## 2. 用 `.eh_frame` 取 `0xBAAB0C` 所属函数的真实边界（PROVEN）

该 so 已 **strip**（无 `.symtab`，仅 2,510 个 `.dynsym`）。用 `.eh_frame` 的 FDE 表取函数区间：

```
包含 0xBAAB0C 的 FDE = [0xBA3CE8, 0xBAB884)  size = 0x7B9C (31,644 B)
```

`0xBAAB0C` 距函数起点 `0x6E24`（89% 处）——即**靠近函数尾部**。

---

## 3. 该函数确实是 HybridCLR 解释器（PROVEN，两条独立证据）

### 证据 1：引用 HybridCLR 错误串所在页
`"hybridclr doesn't support %s"` @ `0x2B84FB9`（在 `.rodata`）。
该函数内有 **10 处 `adrp` 指向该串所在页 `0x2b84000`**，密集分布在 `0xBAAF0C ~ 0xBAAFC4`：

```
ba86a4: adrp x1, 0x2b84000      baa530: adrp x1, 0x2b84000
baa544: adrp x1, 0x2b84000      baaf0c: adrp x0, 0x2b84000
baaf34: adrp x2, 0x2b84000      baaf38: adrp x3, 0x2b84000
baaf60: adrp x0, 0x2b84000      baaf74: adrp x0, 0x2b84000
baaf9c: adrp x0, 0x2b84000      baafc4: adrp x0, 0x2b84000
```

### 证据 2：HybridCLR `StackObject` 步长（12 字节）+ 求值栈操作
```asm
baaa88: smaddl x8, w8, w10, x9     ; w10 = 0xc → 步长 12 = StackObject{int32 type; union data}
baaa90: strh   w8, [x25, #0x6]     ; 写 StackObject 的 16 位半字
baab0c: ldrh   w8, [x8, #0x2]      ; ← 旧 4399 偏移正好落在这里
baab10: strh   w8, [x25, #0x2]
baab14: b      0xbaabe8            ; 汇合到公共尾块（多处 b 0xbaabe8）
```
`w10=0xc` 的 `smaddl` 下标寻址 + `strh [xN,#2/#4/#6]` 写 `StackObject` 是 HybridCLR 解释器求值栈的**特征形态**。

→ **判定：`[0xBA3CE8, 0xBAB884)` = HybridCLR 解释器代码（含操作码处理块）** ✅

---

## 4. 对「`0xBAAB0C` 能否直接复用」的判定：**不可静态确认（FAILED）**

理由三条：

1. **落点不是公共调用入口**：`0xBAAB0C` 处是某个**操作码处理块**的尾部（16 位转换/存储后 `b 0xbaabe8` 汇合），而 `0xBAAB0C` 前后**没有** `x1 = MethodInfo` 的使用痕迹（最近的 `bl 0xbdb5cc` 已把 x0/x1 用掉）。
   而 4399 文档把该点描述为「HybridCLR **单跳板** + 方法名过滤」（`改包初判` §Hook 链、`prefs差分` 「四功能共用一条跳板」），其特征是**每次托管调用都经过、且能取到被调方法名**——与本处形态**不符**。
2. **`0xBAAB0C` 到底是不是 4399 包的「函数序言」存疑**：旧文档写「仅 1 处 **prologue** `B` stub」（mem vs APK diff），暗示该点在 4399 包中像**函数入口**；而在 TapTap 包中它是**函数中段 89% 处**。两者不可能同时成立 → 至少有一个包的 `.text` 布局不同。
3. **跨渠道 `.text` 是否相同未证**：
   - 支持相同：`.text` 内**零渠道特征串**（`4399/m4399/UDP/tds/TapTap/ginicat` 全 0）；而渠道相关代码/字符串在**其后的 `il2cpp` 段**（`global-metadata.dat` 里 `UDP` 42 / `tds` 39 / `TapTap` 35 次命中）→ 渠道差异落在 `.text` 之后，理论上不影响 `.text` 地址。
   - 不支持「直接可用」：上述第 1、2 条形态矛盾。
   - **4399 版 `libil2cpp.so` 已确认不可恢复**：磁盘无副本（`find` 全盘）、真机 `96f0d883` 未安装、Git 从未提交（`git log --diff-filter=D` 空）、`99_archive/` 与 keys 备份均无。

**⇒ 结论：不得复用 `0xBAAB0C` 作为 TapTap 版挂点；且静态路线无法定论。**

---

## 5. 推荐路线：**不再硬编码偏移，改运行时按名解析**（v2.4.0 已具备全部 API）

`nm -D libil2cpp.so` 实测 **15/15 必需 API 全部导出**：

```
il2cpp_domain_get                il2cpp_domain_get_assemblies     il2cpp_assembly_get_image
il2cpp_class_from_name           il2cpp_class_get_method_from_name il2cpp_method_get_name
il2cpp_method_get_param_count    il2cpp_class_get_field_from_name il2cpp_field_get_offset
il2cpp_class_get_fields          il2cpp_object_new                il2cpp_runtime_invoke
il2cpp_resolve_icall             il2cpp_image_get_class           il2cpp_thread_attach
```

### 5.1 定位跳板的直接办法（替代 memdiff）
HybridCLR 会给热更方法挂上**同一个解释器入口**作为 `methodPointer`。因此：

1. `il2cpp_domain_get()` → `il2cpp_domain_get_assemblies()` 找热更程序集（`hotfix` / `HybridCLR.Runtime` 之外那个）→ `il2cpp_assembly_get_image()`
2. `il2cpp_class_from_name(image, "", "BattleServer")`
3. `il2cpp_class_get_method_from_name(klass, "HpHandle", 1)` → `MethodInfo*`
4. **读 `MethodInfo.methodPointer`**（偏移取自 `il2cpp.h`/dump）→ 即**共享解释器入口地址**
5. `file_offset = methodPointer - module_base`（本包 off == VA，可直接与文件对照）
6. `DobbyHook(methodPointer, ...)`，**hits 计数 > 0 才算挂上**（铁律一）

**优点**：不依赖任何硬编码偏移；官方热更换包时只要类名/方法名不变就自动跟随（直接缓解「hotfix 被服务端替换 → 锚点漂移」这一最高新风险）；且可用同一套 API 拿 `ObscuredFloat` 字段偏移（`il2cpp_field_get_offset`）。

### 5.2 顺带能定死的事
- `il2cpp_thread_attach` 已导出 → **R-1（未 attach 的 mod 线程）有正解**，不再是"照着别的游戏抄"。
- 因此 `TlgdMod.cpp` 的 `FILE_TRAMPOLINE` 常量应整体废弃，改为解析结果缓存。

---

## 6. 待你决定的验证路线（需要授权项已标出）

| 路线 | 做法 | 成本 | 是否需要设备操作 |
|------|------|------|------------------|
| **A（最省，静态决定性）** | 你手上若还有 **4399 vc4412 的包 / 旧 `libil2cpp.so` / 旧星光产物** → 我 diff 两个 `.text`：<br>· 相同 → 4399 挂点在 TapTap 版也成立，且能确证「0xBAAB0C 就是那个操作码块」<br>· 不同 → 按函数相似度匹配解释器入口，仍可静态定论 | 低（几分钟） | ❌ 不需要 |
| **B（不依赖旧包，运行时决定性）** | **先注册 `04_scripts/弹力果冻/pipeline.json`**，再出一个**最小探针包**：只做 §5.1 的按名解析 + 打印 `methodPointer`/文件偏移 + hits 计数，**不做任何功能**；装到 MuMu 进一局看 logcat | 中（一次构建/装机） | ✅ 需要：**卸载官方包并装机**（改包签名不同，无法覆盖安装）＋数据保留范围确认 |
| **C（合并步骤）** | 直接用 §5.1 的按名解析写正式功能，把 hits 验证并进首次功能构建 | 中 | ✅ 同上 |

> 说明：B 的价值不只是拿偏移——它同时验证**换渠道本身**的三件事：TapTap 证书回灌能否过校验、`appComponentFactory` 保留是否正确、外科 zip 组装在新包上是否成立。C 省一次装机但把两类问题混在一次测试里。

---

## 7. 附：已确认不可用的既有线索

| 线索 | 状态 |
|------|------|
| 4399 版 `libil2cpp.so`（diff 用） | **不可恢复**（磁盘/真机/Git/备份四处皆无） |
| 从 4399 官方渠道现下同版本包 | 4399 仅自家渠道分发，未取得；`shouji.com.cn` 的 v2.4.0 大小 2029.95M 与 TapTap 包一致（=同版），非 4399 渠道 |
| 旧探针脚本 `04_scripts/弹力果冻/tlgd_trampoline_probe.js` | 已随 2026-09-09 Git 分层清理，不在工作区 |
| MuMu root（Frida 直挂用） | `su` 存在但 `ro.secure=1` / `ro.debuggable=0`，未授权 → 需在 MuMu 设置里打开 ROOT 权限 |
| 硬编码 `FILE_TRAMPOLINE = 0xBAAB0C` | **FAILED（不复用）** |
