# 暗影格斗3 1.45.3（TapTap / com.wanda.sf3.wd / vc907）静态分析取证

- 时间：2026-11-24 19:37–20:0x（Windows + Git Bash）
- 角色：静态分析员（analyst）。**只读**取证；未 patch / 未重签 / 未装机 / 未改任何游戏源码。
- 产物根：`06_dumped/暗影格斗3/dump_20261124/`（大文件不入 git）
- 权威期望值来源：`mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp` 第 112–155 行（常量表）＋ 第 471–537 行（hook 体）

## 0. 结论摘要（一行一项）

| # | 核对项 | 结论 | 关键证据 |
|---|---|---|---|
| A1 | 内层两文件 md5 | **PASS** | libil2cpp `53345cdb4d31dff336fdd575fb87b667`；metadata `809ec525f0875c2092dc7f21e73fb288` |
| A2 | vaddr→file offset 换算（−0x4000） | **PASS（实测证得，非假设）** | `llvm-readelf -l`：LOAD#2 `Offset 0x2e530c0 / VirtAddr 0x2e570c0`（R E，`.text il2cpp .plt`），Δ=0x4000 |
| B1 | dump.cs | **PASS（50,644,804 B / 1,448,586 行）** | sha256 `3860c32a…5ee`；目标类全部在 |
| B2 | stringliteral.json | **PASS（辅助产物，非该 fork 输出）** | 30,975 项 / 30,950 非空；sha256 `a30afc7b…f24` |
| C1 | `IKHOBFONGIE.OGFLOMPOMCI` @file 0x5F51968 | **PASS** | 12 字节 `ff8303d1 e83b00fd fd7b08a9` 完全相同 |
| C2 | `LJMJLLJNEMK.{JDMGBLEBMAJ,JBEFILOFEKD,BAALGFECCBG}` | **PASS ×3** | RVA/Offset 精确命中；反汇编见 `eor` XOR 加解密、读写 `+0x10` |
| C3 | `UnityEngine.Time.set_timeScale` @file 0x6BB5C6C | **PASS** | RVA 0x6BB9C6C；序言 `fc1e0fe8`；同名方法仅 2 个，另一为 TimelineClip 实例重载 |
| C4 | `self+0x68`=玩家 / `+0x70`=敌人 | **PASS（偏移）/ 语义仅推断** | dump.cs:130328/130329 两个 `HJGEAPMLNAF` 字段 @0x68/@0x70；玩家/敌人字面语义不可静态证 |
| C5 | `HJGEAPMLNAF` 逻辑体、`LHOBAHKKIDA.HABGLAMIMLP` [0,1] | **PASS** | 基类是 **interface**（非 MonoBehaviour）、`+0x108`=Model；HABGLAMIMLP 反汇编有 1.0/0.0 双向夹取 |
| D1 | `IntentHelper` action 锚点 | **PASS（出现 1 次）** | classes.dex → `getAction(...)` 内 `const-string p1, "com.taptap.sdk.action"` |
| D2 | `MultiDexApplication.super` | **PASS（1 次）** | classes5.dex：`.super Landroid/app/Application;` |
| D3 | 官方证书 | **PASS** | DER 845 B、base64 头 `MIIDSTCCAjGgAwIBAgIEJhYcejAN`、SHA-1 `9078e787…8fd8`、MD5 `c9950f9c…afe7` |
| D4 | Application 链 | **PASS** | Manifest `:name="com.accesssdkwrapper.AccessSDKApplicationWrapper"` → `Landroidx/multidex/MultiDexApplication;` → `Landroid/app/Application;` |

**无 FAIL 项。** 有 3 处「与任务书预期不一致的事实」与 2 处「工具/产物偏差」，见 §3、§5。

## 1. 输入指纹（本轮本地重算）

| 文件 | 字节 | md5 | sha256 |
|---|---|---|---|
| `02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk` | 2,713,267,683 | （采信侦察兵） | `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087` |
| `lib/arm64-v8a/libil2cpp.so` | 127,366,992 | `53345cdb4d31dff336fdd575fb87b667` ✅ 与侦察兵一致 | `3f28d8a61ee3ad6f49b9271b97799c6eacee618be438ab41d095176e41893080` |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 26,448,516 | `809ec525f0875c2092dc7f21e73fb288` ✅ | `af8d4cc7e1e31acdcf18440f1dab21b879dc3ccc20c6deb198d35d8f29fdf214` |

metadata 头实测：sanity `0xFAB11BAF`、**version = 39**（`af1bb1fa 27000000`）。

## 2. 产出物

| 产物 | 字节 | sha256 | 说明 |
|---|---|---|---|
| `dump_out_v39/dump.cs` | 50,644,804 | `3860c32a595f58e9eccb781b2c0e680eb3a67389dfa246e947b5ecc5a617e5ee` | 1,448,586 行；与历史记录「50.6 MB」量级一致 |
| `dump_out_v39/script.json` | 139,255,497 | `27e418c59aebf377700849a098a1f11b7ace77129fa676a52ed4d8f3757288f8` | ScriptMethod + ScriptMetadata（**无 ScriptString 段**） |
| `dump_out_v39/stringliteral.json` | 1,275,205 | `a30afc7bcf5a4e523b110870f1a1a21d04150c520ded1b497ec26a0ac5f0af24` | **辅助产物**：按 v39 头布局自生成，见 §5-D2 |

## 3. C 逐挂点核对（核心）

### C0. 先证明「file offset = vaddr − 0x4000」

```
$ llvm-readelf -l lib/arm64-v8a/libil2cpp.so
  LOAD 0x000000  0x0000000000000000  ... 0x2e530b8  R    0x4000
  LOAD 0x2e530c0 0x0000000002e570c0  ... 0x4140260  R E  0x4000   ← .text / il2cpp / .plt
  LOAD 0x6f93320 0x0000000006f9b320  ... 0x53b420   RW   0x4000
```

- 可执行段 `p_offset=0x2e530c0`、`p_vaddr=0x2e570c0` ⇒ **Δ = 0x4000**，段 vaddr 区间 `[0x2e570c0, 0x6F97320)`。
- 5 个目标 vaddr（0x5F55968 / 0x5F3EA60 / 0x5F3EAE8 / 0x5F3EB5C / 0x6BB9C6C）全部落在该区间内 ⇒ 换算成立。
- 旁证：dump.cs 每行同时打印 `RVA`、`Offset`、`VA`，其 `Offset` 恒等于 `RVA − 0x4000`（例：RVA 0x5F55968 → Offset 0x5F51968）。

### C1. `IKHOBFONGIE.OGFLOMPOMCI` — **PASS**

| 项 | 值 |
|---|---|
| dump.cs 行号 | **130689**（类 `public class IKHOBFONGIE : NDMEOEADJLF` 在 130300） |
| 声明 | `private void OGFLOMPOMCI(AJJGHOHIMGI LABDELAFELP, HJGEAPMLNAF HNCIJKIMHLE, LJMJLLJNEMK EAAIDKIHFON, string LIMCBPKDBCM, Action<LJMJLLJNEMK> NJJCJPBBDFF)` |
| dump RVA / Offset / VA | 0x5F55968 / **0x5F51968** / 0x5F55968 ← 与 `kFileChangeLife` 一致 |
| 文件字节 @0x5F51968 | `ff8303d1 e83b00fd fd7b08a9`（后接 `fc6f09a9`）← 与 `kPrologueChangeLife` **12/12 字节相同** |
| 首字 | `0xD10383FF` == `kInsChangeLife` |
| 前 3 条指令 | `sub sp, sp, #0xe0` / `str d8, [sp, #0x70]` / `stp x29, x30, [sp, #0x80]` |

语义（改血入口）证据，反汇编 0x5F55968–0x5F5652C（下一方法 `JLKENJLCFJC` @0x5F5652C 之前，共 759 条指令）：

- `bl 0x5F3EA60`（= `LJMJLLJNEMK.BAALGFECCBG(容器)→float`）出现 **4 次**：0x5F55BE0 / 0x5F55FA8 / 0x5F560A8 / 0x5F56448；
- 0x5F55BE4 `fcmp s0, #0.0` + 0x5F55BE8 `cset w8, ge` ⇒ 「容器值 ≥ 0」分支，随后 0x5F55C10 `movi d0,#0` + `bl 0x5F3EA88`（`BAALGFECCBG(float)` 构造容器）——即「把值换成另一个容器」的改血写法；
- 参数落位实证（8 寄存器 ABI）：0x5F559AC `mov x19, x1`、0x5F559A8 `mov x25, x3`、0x5F559A4 `mov x27, x4`、0x5F559A0 `mov x29, x5`、0x5F5599C `mov x22, x6`，而 **x2 被溢出到栈** `str x2, [sp, #0x8]` ⇒ 第 1 参 `AJJGHOHIMGI` 确实按值占 **x1+x2** 两个寄存器，`x3=unit`、`x4=容器` 与 §3.1 推导一致。

> **事实修正（不是 FAIL）**：任务书预期的「能看到 `ldr x?, [x?, #0x68]`/`#0x70` 这类 self 偏移读取」**在本函数体内不存在**——对全部 759 条指令做 `#0x68` 检索为 0 命中。self 的两个单位字段读取发生在 **hook 侧**（`Il2Cpp::ReadPtr(self, 0x68/0x70)`），本体并不读。这不影响挂点合法性：`x0` 就是 `this`（dump 声明为非 static 实例方法）。

### C2. `LJMJLLJNEMK` 三个方法 — **PASS ×3**

| 方法 | dump.cs 行 | RVA → Offset | 序言首字（实测） | 期望 | 结论 |
|---|---|---|---|---|---|
| `BAALGFECCBG(LJMJLLJNEMK)→float` | 128256 | 0x5F3EA60 → **0x5F3AA60** | `f81f0ffe` = `str x30,[sp,#-0x10]!` | （Sf3Mod.cpp 仅引用地址 0x5F3EA60） | PASS |
| `JDMGBLEBMAJ()→float` | 128262（`Slot: 4`，`protected override float`） | 0x5F3EAE8 → **0x5F3AAE8** | `a9be57fe` = **0xA9BE57FE** | `kInsValueGet` = 0xA9BE57FE | PASS |
| `JBEFILOFEKD(float)` | 128265（`Slot: 5`，`protected override void`） | 0x5F3EB5C → **0x5F3AB5C** | `fc1d0fe8` = **0xFC1D0FE8** | `kInsValueSet` = 0xFC1D0FE8 | PASS |

「实例方法 + XOR 加解密 + 不直写 +0x10」的实证：

- 类 `public class LJMJLLJNEMK : BLEKKHPIOKN<float>`，字段 `private int FGDFOOBPADF; // 0x10`——**+0x10 是 int 密文，不是 float**，直写 float 必错。
- getter（0x5F3EAE8）：`ldr w19, [x19, #0x10]`（取密文）→ `ldr w8, [x8]`（静态 key）→ **`eor w0, w19, w8`** → `bl 0x5807CE0`（int→float 装箱）。
- setter（0x5F3EB5C）：`bl 0x5807F54`（float→int）→ **`eor w8, w20, w8`** → **`str w8, [x19, #0x10]`**。
- 三者 x0 均为 self（`mov x19, x0`），非 static ⇒ 与 rn 的 `ValueGet_t/ValueSet_t` 调用约定一致。

### C3. `UnityEngine.Time.set_timeScale` — **PASS**

| 项 | 值 |
|---|---|
| 类 | dump.cs:926371 `// Namespace: UnityEngine` → `public class Time // TypeDefIndex: 14188` |
| 方法行号 | **926442** `public static void set_timeScale(float value)` |
| RVA / Offset | 0x6BB9C6C / **0x6BB5C6C** ← 与 `kFileSetTimeScale` 一致 |
| 序言首字 | `fc1e0fe8` = **0xFC1E0FE8** == `kInsSetTimeScale` |
| 前 3 条 | `str d8,[sp,#-0x20]!` / `stp x30,x19,[sp,#0x10]` / `fmov s8,s0` |

「写的是静态字段而非别的重载」：

- 函数体 0x6BB9C6C–0x6BB9CA4：`adrp x19, 0x7998000` → `ldr x0,[x19,#0x658]`（静态槽，懒初始化，未命中时 `bl 0x32A9B38` 建 TimeManager）→ `br x0` **尾跳 native TimeManager setter**；无 self、无实例字段。
- 全 dump.cs 中 `set_timeScale` 共 **2 处**：
  1. dump.cs:926442 `public static void set_timeScale(float)` RVA **0x6BB9C6C** ← ✅ 匹配 0x6BB5C6C；
  2. dump.cs:1271058 `public void set_timeScale(double)` RVA 0x6B07F54 —— 所属类型 `public class TimelineClip`（dump.cs:1270932）的实例方法，**不匹配**。

### C4. `self+0x68` / `self+0x70` — **PASS（偏移）；玩家/敌人语义＝推断**

```
130328	private HJGEAPMLNAF JOLEGCBKHKI; // 0x68
130329	private HJGEAPMLNAF AJBHLLFGAEA; // 0x70
```

- 两字段**类型相同**（`HJGEAPMLNAF`＝逻辑体）、**偏移连续**（0x68/0x70），与 `kOffPlayerUnit=0x68`、`kOffEnemyUnit=0x70` 完全一致；
- 相邻上下文亦是 `IKHOBFONGIE` 自身字段：`@0x60 bool`、`@0x78 MJDFKPDHOLL`、`@0x88 readonly LHOBAHKKIDA MCKNJKOIIED`（dump.cs:130332，时间管理器实例，与 C5 交叉印证）；
- **未证部分**：dump 只给「字段名 + 偏移」，字段名是混淆名，不能从静态数据判定 0x68 是玩家、0x70 是敌人。该语义来源是 `Sf3Mod.cpp` 注释与斌哥包对照，本轮**标为推断**，不当作实测。

### C5. `HJGEAPMLNAF`（逻辑体）与 `LHOBAHKKIDA.HABGLAMIMLP`（[0,1]） — **PASS**

- dump.cs:214392 `public class HJGEAPMLNAF : HDHEIMBLPDO, PDBKPCNGLKL, NIPKJLFLGEL, FOGDDBEIILK, FMKDOICOBGK, LBJAMHILPOA, DIMBIEKGLLG`；
- 首基类 `HDHEIMBLPDO` 在 dump.cs:219154 是 **`public interface HDHEIMBLPDO`** ⇒ `HJGEAPMLNAF` **不是 MonoBehaviour** ✅ 与硬约束 D-12 一致；
- `+0x108` 证据：`private Model <GAFJLJNMEHC>k__BackingField; // 0x108` ✅；回指侧 `public class Model : MonoBehaviour`（dump.cs:212485）+ `private HJGEAPMLNAF <JOHEIMCCGDC>k__BackingField; // 0x20` ✅（+0x20 回指逻辑体）。
- `LHOBAHKKIDA` dump.cs:203597；`public void HABGLAMIMLP(float GMAOMCJBGAG)` dump.cs:203773，RVA **0x62065E0**；反汇编 0x62065E0–0x62066B0：
  ```
  62065e8: 1e2e1001  fmov s1, #1.00000000
  62065f4: 1e212000  fcmp s0, s1
  62065f8: 1e20cc21  fcsel s1, s1, s0, gt      ; 上限 = min(v, 1.0)
  62065fc: 1e202008  fcmp s0, #0.0
  6206600: 1e214c48  fcsel s8, s2, s1, mi      ; 下限 = max(v, 0.0)
  6206608: bd003008  str  s8, [x0, #0x30]
  6206610: 9426cd97  bl   0x6bb9c6c            ; ← 调 set_timeScale
  ```
  ⇒ **只接受 [0,1]（只能减速）**，与 §3 记录一字不差；同时反证 C3 的 set_timeScale 才是可超 1 的底层口。

## 4. D Java 侧静态核对（只读，未打包）

| # | 项 | 结论 | 证据 |
|---|---|---|---|
| D1 | `IntentHelper` action 锚点 | **PASS，出现 1 次** | 见下 |
| D2 | `MultiDexApplication.super` | **PASS，1 次** | 见下 |
| D3 | 官方证书 | **PASS** | 见下 |
| D4 | Application 链 | **PASS** | 见下 |

**D1**（classes.dex，6,642,216 B；baksmali → 4,837 个 .smali）
- 类：`Lcom/taptap/sdk/login/internal/handlers/client/IntentHelper;`，`.super Ljava/lang/Object;`，源文件 `IntentHelper.smali`（2,290 B）
- 方法：`.method public final getAction(Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Ljava/lang/String;`（.smali 第 59 行起）
- 字符串原文（.smali 第 73 行）：`const-string p1, "com.taptap.sdk.action"` —— 与 `surgical_sf3_1.45.3.sh` 的 `OLD` 逐字一致（脚本用 `t.count(OLD) != 1` 断言）
- 出现次数：该 .smali 内 **1 次**；整个 classes.dex 内匹配 `com.taptap.sdk.action` 的文件**只有这 1 个**，计数 **1 次** ⇒ 脚本锚点成立。

**D2**（classes5.dex，7,041,292 B；baksmali → 5,366 个 .smali）
- `dex5_smali/androidx/multidex/MultiDexApplication.smali`（675 B）第 2 行：`.super Landroid/app/Application;`
- `grep -c '^\.super Landroid/app/Application;'` = **1** ⇒ 脚本 `n != 1` 断言成立。

**D3**（`META-INF/AYGD3.RSA` 1,300 B）
- `openssl pkcs7 -inform DER -print_certs` → 证书 DER **845 B**
- base64 开头：`MIIDSTCCAjGgAwIBAgIEJhYcejANBgkqhkiG9w0BAQsF` ✅ 以 `MIIDSTCCAjGgAwIBAgIEJhYcejAN` 开头
- SHA-1：`90:78:E7:87:FB:52:46:BF:D1:47:19:33:43:CF:79:44:DA:DC:8F:D8` = `9078e787fb5246bfd147193343cf7944dadc8fd8` ✅
- MD5：`c9950f9c3790803c7ccf42470142afe7` ✅（补上硬约束 D-8 的 MD5 值）
- subject：`C=cn, ST=gd, L=sz, O=szu, OU=atr, CN=sagely` ✅

**D4**（AndroidManifest，binary XML）
- `aapt2 dump xmltree` → `E: application` 的 `android:name(0x01010003)="com.accesssdkwrapper.AccessSDKApplicationWrapper"` ✅
- 继承链（classes5.dex）：
  `com.accesssdkwrapper.AccessSDKApplicationWrapper` (`.super Landroidx/multidex/MultiDexApplication;`) → `androidx.multidex.MultiDexApplication` (`.super Landroid/app/Application;`) → `android.app.Application`
  ⇒ 「改 MultiDexApplication 父类即可在 Application 构造期装签名替换器，Manifest 一点不用动」这一锚点成立。

## 5. 偏差与降级（不等于 FAIL）

- **D1 工具**：`11_proj_tools/Il2CppDumper_v39_vmpp` 在本机**不存在**（该目录为空，2026-11-24 实测 `ls` 仅 `.`/`..`）。`01_tools/il2cppdumper`(Perfare 6.7.46) 明确拒绝：`Metadata file supplied is not a supported version[39]`。本轮改用 GitHub `xy0-dev/Il2cppDumper_v39`（commit `cff612af1331bf46d1eabb2390f8a46dbec7ea27`，仅支持 v39、输出 dump.cs+script.json），用 `dotnet-sdk8` 的 Roslyn `csc` 直接编译（`dotnet build` 在本机 NuGet restore 报 `Value cannot be null (Parameter 'path1')`，与项目无关，属环境问题）。
- **D2 stringliteral.json**：该 fork 只输出 dump.cs/script.json，**不产 stringliteral.json**。本轮的 `stringliteral.json` 是按该 fork 自己定义的 v39 头（`(offset,size,count)` 三元组；`Il2CppStringLiteral` v39 仅 `dataIndex`，4 字节/项）用 30 行 Python 生成的**辅助产物**；校验：`count*4 == size`（30975×4 = 123900，精确成立）、30,950/30,975 项可解 UTF-8。**标注为非原版工具输出**，不作为挂点证据。
- **D3 历史行号不可直接对照**：硬约束 D-12 引用的 `dump.cs` 行号（196868 / 198614，2026-10-28 dump）与本轮行号（214392 / 212485）不一致，原因是 dumper/输入相同但输出组织不同；**内容一致**（同一类、同一 +0x108/+0x20 字段）。引用时按「本轮行号」。
- **D4 `06_dumped/暗影格斗3` 本轮前不存在**（本机无历史 dump），因此 §C 全部结论都是本轮实测，不依赖历史 dump.cs。

## 6. 若 FAIL：影响与最小修法（只描述，不改）

本轮**无 FAIL**，故本节只记录「若要补强未证部分」的最小动作（全部只读/只描述）：

1. **C4「0x68=玩家 / 0x70=敌人」语义**（现为推断）
   - 影响：若两字段实际含义相反，则「上帝模式清零」会打到敌人、「倍率」会打到玩家（现象：玩家一刀秒或被秒）。功能逻辑本身不会崩。
   - 最小取证（不改源码）：反汇编 `IKHOBFONGIE` 里**写入** `JOLEGCBKHKI/AJBHLLFGAEA` 的方法（找 `str x?, [x?, #0x68]` / `#0x70` 的 store 点），看哪一路来自 `NativePlayerHUD.Setup` 的实参；或真机 frida 在 `OGFLOMPOMCI` 命中时打印 `self+0x68`/`self+0x70` 两个指针与玩家 HUD 绑定的对象地址比对。
2. **C1 本体不读 `#0x68`**（已确认为事实）
   - 影响：无。hook 侧读的是 `IKHOBFONGIE` 实例（x0），字段表已证偏移；只是「入口函数内部能看见 self 偏移」这一预期不成立。
   - 若要复核：把 hook 的日志（`[sf3] hit player/enemy v=…`）当作运行时验证，不改本体。
3. **stringliteral.json 非官方产物**
   - 影响：仅影响「用字符串表定位 UI 文案」这类辅助用途，不影响挂点。
   - 最小修法（描述）：取得真正支持 v39 的 fork（如本轮的 `xy0-dev/Il2cppDumper_v39` 增加 ScriptString 输出），或保留本轮 Python 生成 + 上面两条自校验。
4. **原生地址漂移时的现成回退**（Sf3Mod.cpp 已内建）：file offset → `/proc/self/maps` RX 段定位 + 序言校验，失败转按名（`FindClassByShortName`/`MethodFn`）。本轮所有序言均匹配，**不会触发回退**。

## 7. 复现命令块（可直接抄）

```bash
cd /e/nixiang
NIXIANG=/e/nixiang
APK="02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk"
OUT="06_dumped/暗影格斗3/dump_20261124"
NDK="$NIXIANG/01_tools/android-sdk/ndk/android-ndk-r27c/toolchains/llvm/prebuilt/windows-x86_64/bin"

# --- A 提取 + 校验 ---
mkdir -p "$OUT"
unzip -o "$APK" lib/arm64-v8a/libil2cpp.so \
  assets/bin/Data/Managed/Metadata/global-metadata.dat -d "$OUT"
md5sum "$OUT/lib/arm64-v8a/libil2cpp.so" \
       "$OUT/assets/bin/Data/Managed/Metadata/global-metadata.dat"
# 期望 53345cdb4d31dff336fdd575fb87b667 / 809ec525f0875c2092dc7f21e73fb288
sha256sum "$OUT/lib/arm64-v8a/libil2cpp.so" \
          "$OUT/assets/bin/Data/Managed/Metadata/global-metadata.dat"
# 3f28d8a6…93080 / af8d4cc7…f214

# --- A' 证明 file offset = vaddr - 0x4000 ---
"$NDK/llvm-readelf.exe" -l "$OUT/lib/arm64-v8a/libil2cpp.so" | head -12
# LOAD 0x2e530c0 0x2e570c0 … R E  ⇒ Δ=0x4000

# --- B dump.cs（v39 fork；Perfare 6.7.46 会拒绝 version 39）---
cd "$NIXIANG/13_scratch/sf3_remake/tools"
git clone --depth 1 https://github.com/xy0-dev/Il2cppDumper_v39
cd Il2cppDumper_v39
REF=$NIXIANG/01_tools/dotnet-sdk8/packs/Microsoft.NETCore.App.Ref/8.0.11/ref/net8.0
mkdir -p build_out
$NIXIANG/01_tools/dotnet-sdk8/dotnet.exe exec \
  $NIXIANG/01_tools/dotnet-sdk8/sdk/8.0.404/Roslyn/bincore/csc.dll \
  -nologo -langversion:latest -target:exe -optimize+ -unsafe \
  -nowarn:CS8618,CS8600,CS8602,CS8604,CS8625 \
  -out:build_out/Il2CppDumper.dll \
  $(for f in $REF/*.dll; do printf -- '-r:%s ' "$f"; done) \
  $(find . -name '*.cs' -not -path './build_out/*')
cat > build_out/Il2CppDumper.runtimeconfig.json <<'J'
{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}
J
cp config.json build_out/
cd build_out
$NIXIANG/01_tools/dotnet-sdk8/dotnet.exe Il2CppDumper.dll \
  "$NIXIANG/$OUT/lib/arm64-v8a/libil2cpp.so" \
  "$NIXIANG/$OUT/assets/bin/Data/Managed/Metadata/global-metadata.dat" \
  "$NIXIANG/$OUT/dump_out_v39"
# 注意：该 fork 忽略第 3 个参数，产物落在 cwd ⇒ 手动 mv dump.cs / script.json 到 dump_out_v39/

# --- B' stringliteral.json（辅助）---
PYTHONUTF8=1 python3 "$NIXIANG/13_scratch/sf3_remake/make_stringliteral.py" \
  "$NIXIANG/$OUT/assets/bin/Data/Managed/Metadata/global-metadata.dat" \
  "$NIXIANG/$OUT/dump_out_v39/stringliteral.json"

# --- C 反汇编 & 序言比对（vaddr，不是 file offset）---
SO="$NIXIANG/$OUT/lib/arm64-v8a/libil2cpp.so"
xxd -s 0x5F51968 -l 16 -p "$SO"          # ff8303d1e83b00fdfd7b08a9…
for a in 0x5F55968 0x5F3EA60 0x5F3EAE8 0x5F3EB5C 0x6BB9C6C 0x62065E0; do
  "$NDK/llvm-objdump.exe" -d --start-address=$a --stop-address=$((a+0x28)) "$SO" | sed -n '7,12p'
done
"$NDK/llvm-objdump.exe" -d --start-address=0x5F55968 --stop-address=0x5F5652C "$SO" > /tmp/ogf.txt
grep -c '#0x68' /tmp/ogf.txt              # 0 —— 本体不读 self+0x68
grep -n 'bl\s*0x5f3ea60' /tmp/ogf.txt     # 4 次容器取值

# --- C' dump.cs 切片（禁整读）---
cd "$NIXIANG/$OUT/dump_out_v39"
sed -n '130300,130340p' dump.cs           # IKHOBFONGIE 字段表（0x68/0x70）
sed -n '130680,130692p' dump.cs           # OGFLOMPOMCI 声明 + RVA
sed -n '128245,128270p' dump.cs           # LJMJLLJNEMK 三方法
sed -n '926371,926371p;926441,926443p' dump.cs   # UnityEngine.Time
sed -n '214392,214394p;212485,212492p' dump.cs   # HJGEAPMLNAF / Model
sed -n '203597,203600p;203772,203774p' dump.cs   # LHOBAHKKIDA / HABGLAMIMLP

# --- D Java 侧 ---
unzip -o "$APK" classes.dex classes5.dex "META-INF/MANIFEST.MF" \
  "META-INF/AYGD3.RSA" "META-INF/AYGD3.SF" -d "$OUT/apk_side"
cd "$OUT/apk_side"
java -jar "$NIXIANG/01_tools/smali/baksmali-fat.jar" d classes.dex  -o dex1_smali
java -Xmx2g -jar "$NIXIANG/01_tools/smali/baksmali-fat.jar" d classes5.dex -o dex5_smali
grep -c 'const-string p1, "com.taptap.sdk.action"' \
  dex1_smali/com/taptap/sdk/login/internal/handlers/client/IntentHelper.smali   # 1
grep -n '^\.super' dex5_smali/androidx/multidex/MultiDexApplication.smali      # Landroid/app/Application;
grep -c '^\.super Landroid/app/Application;' \
  dex5_smali/androidx/multidex/MultiDexApplication.smali                        # 1
grep -n '^\.super' dex5_smali/com/accesssdkwrapper/AccessSDKApplicationWrapper.smali
openssl pkcs7 -inform DER -in META-INF/AYGD3.RSA -print_certs -out /tmp/sf3_cert.pem
openssl x509 -in /tmp/sf3_cert.pem -outform DER -out /tmp/sf3_cert.der
base64 -w0 /tmp/sf3_cert.der | cut -c1-44     # MIIDSTCCAjGgAwIBAgIEJhYcejAN…
openssl x509 -in /tmp/sf3_cert.pem -noout -fingerprint -sha1 -subject
cd /e/nixiang
"$NIXIANG/01_tools/android-sdk/build-tools/android-15/aapt2.exe" dump xmltree \
  --file AndroidManifest.xml "$APK" | grep -A6 'E: application'
```

## 8. 体量与纪律

- `dump.cs` / `script.json` / `stringliteral.json` / `*.smali` **全部未整读**；所有 dump 结论来自 `rg -n -m N` + `sed -n 'A,Bp'` 切片（切片命令见 §7）。
- 删中间产物只用显式路径（本轮仅删过自建的 `apk_side`，删前打印 `realpath`）。
- 大文件（libil2cpp.so 127 MB、global-metadata.dat 26 MB、script.json 139 MB、smali 树）均在 `06_dumped/` / `13_scratch/` 下，二者不在 git 跟踪范围（`git status` 无新条目）。

## 9. 本轮未修改任何游戏二进制与源码（声明）

- `libil2cpp.so`、`global-metadata.dat`：**只读**（`unzip` 解出 → `md5sum`/`sha256sum`/`xxd`/`llvm-readelf`/`llvm-objdump`，无写操作）；md5 与官方值一致即证未被改动。
- APK：只做 `unzip -o` 取条目，未 `zip` 回写、未重签、未安装。
- 游戏源码（`mod菜单/**`、`04_scripts/暗影格斗3/**`）：**未执行任何编辑**。`git status` 中 `04_scripts/暗影格斗3/{device_ready,fetch_official}_sf3.sh / official_apk.json / pipeline.json` 的 `M` 状态存在，但其 mtime 为 19:20/19:32，**早于本轮开始（19:37）**，非本轮产生。
- 本轮新增/写入仅限：`06_dumped/暗影格斗3/dump_20261124/**`（dump 与 Java 侧副本）、`13_scratch/sf3_remake/**`（工具 clone、编译产物、两个脚本、本报告）。
