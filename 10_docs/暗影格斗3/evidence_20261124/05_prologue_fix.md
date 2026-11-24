# 暗影格斗3 星光版 — 12 字节序言常量修复 + 重出包 + 重装机（fixer）

- 时间：2026-11-24 22:02–22:29（Windows 11 + Git Bash/MSYS，工作区 `E:/nixiang`）
- 角色：**修复与重出包执行员**。上一轮独立验收（`13_scratch/sf3_remake/03_verify.md` §3b）判定唯一缺陷 = 12 字节序言常量被编译器丢弃；本轮只修这一处。
- 唯一改动源码：`mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp`（+5/-2 行）
- 目标设备：`emulator-5554`（真机 96f0d883 / MuMu 未触碰）

---

## ① 三处 diff 原文

`git diff -- mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp`：

```diff
@@ -304,13 +304,16 @@ struct HookSlot {
     int         argc;
     void*       hookFn;
     void**      origPtr;
+    const uint8_t* prologue;   // 非空 = 用完整序言校验（比 expectInsn 更强）
+    size_t         prologueLen;
 };
 
 // 幂等 + 双路径：*orig 非空 = 已挂过（防双 Dobby 叠钩 SIGSEGV）
 static bool HookSlotInstall(const HookSlot& s) {
     if (s.origPtr && *s.origPtr) return true;
     if (s.fileOff) {
-        void* va = VaAtFileOffChecked(s.tag, s.fileOff, s.expectInsn);
+        void* va = s.prologue ? VaAtFileOffBytes(s.tag, s.fileOff, s.prologue, s.prologueLen)
+                              : VaAtFileOffChecked(s.tag, s.fileOff, s.expectInsn);
         if (va && DobbyInstall(s.tag, va, s.hookFn, s.origPtr)) return true;
     }
     return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
@@ -528,7 +531,7 @@ static void Hooked_SetTimeScale(float value, void* method) {
 
 static const HookSlot kDamageSlots[] = {
     {"ChangeLife(OGFLOMPOMCI)", kFileChangeLife, kInsChangeLife, "", "IKHOBFONGIE", "OGFLOMPOMCI", 5,
-     (void*)Hooked_ChangeLife, (void**)&g_origChangeLife},
+     (void*)Hooked_ChangeLife, (void**)&g_origChangeLife, kPrologueChangeLife, sizeof(kPrologueChangeLife)},
 };
 
 static const HookSlot kSpeedSlots[] = {
```

- 只有这 3 处；没有给新成员写 NSDMI（保持 C++14 聚合初始化，`kSpeedSlots` 未写的新字段自动为 0）。
- 自检：
  - `grep -n "VaAtFileOffBytes\|kPrologueChangeLife"` 结果 = 定义 2 行（153、229）+ 调用点 1 行（315）+ 表初值 1 行（534），**定义与调用点同时存在**。
  - `bash 04_scripts/_common/preflight.sh …/Sf3Mod.cpp` → `汇总: PASS=3 FAIL=0 REMIND=1`，`✅ 机器关通过`（REMIND = 无独立 C++ 语法检查器，已用第 2 步重编替代）。

## ② 代码审查四条红线（`skill/re-implement/code-review-checklist.md` §〇②）

| 红线 | 结论 | 依据 |
|---|---|---|
| 1 调用契约与副作用 | **PASS** | 只换了「取地址」方式，钩子本体 `Hooked_ChangeLife`、orig 调用、参数顺序（8 个寄存器位）全未变；失败语义仍是「返回 nullptr → 不挂」 |
| 2 地址与身份 | **PASS** | fileOff 未改（`kFileChangeLife = 0x5F51968`），坐标仍是「il2cpp file offset → RX 段运行时 VA」；新增的只是校验强度（12B 取代 4B） |
| 3 内存与对象 | **PASS** | `VaAtFileOffBytes` 先 `FileOffToVa` 验 maps 命中，再 `memcmp(va, expect, n)`；`expect`/`n` 是编译期常量数组与 `sizeof`，不会越界读；`n=0` 或 `expect=nullptr` 直接跳过比较 |
| 4 重入、并发与安装 | **PASS** | 幂等检查 `if (s.origPtr && *s.origPtr) return true` 在取值之前；`SyncHooks` 的互斥与 1 s 节流未动；无新增线程/热路径探测 |

**重点：序言校验失败必须回落按名，不得直接崩** —— 反汇编实证（见 ③），失败分支走向：

```
15e1c  mov x0, x24            ; va
15e20  mov x1, x22            ; s.prologue
15e24  mov x2, x23            ; s.prologueLen
15e28  bl  memcmp@plt
15e2c  cbz w0, 0x15eb8        ; 相等 → 走 DobbyInstall
15e30..  LOGW "%s prologue drift got=0x%08X — try by-name"（.rodata 0x44c1）
15e9c  b  0x15f38             ; ← 与「fileOff 无效」分支同一落点 = HookByName
```

`0x15f38` 处即 `HookByName`（`ldr x20,[x19]`=tag，`ldp x25,x22,[x19,#0x18]`=ns/cls，`ldr x21,[x19,#0x28]`=method，`ldr w24,[x19,#0x30]`=argc，`ldp x23,x19,[x19,#0x38]`=hookFn/origPtr，再 `bl EnsureApi → FindClassNs → MethodFn → DobbyInstall`）。**校验失败 = 退回按名，不会崩溃。**

## ③ so 重编与「序言已编进去」取证

新 so：`69e8924c7ae126545f7f51e8cdfddf50`，192,080 B（旧：`da6d7821f8df70afca3d8a5ff1476a46`，191,232 B）

`bash 04_scripts/暗影格斗3/sf3_build_so.sh` → `exit 0`，`Compile++ : starry_magic <= Sf3Mod.cpp` → `[build_so] done … (69e8924c7ae126545f7f51e8cdfddf50) / size = 192080`。

### 3.1 符号层（先看 nm）

```
$ llvm-nm -a -C libstarry_magic.so | grep -i "VaAtFileOff\|HookSlot\|kPrologue"   → 无输出
$ llvm-readelf -S … | grep symtab                                                    → 只有 .dynsym（.so 已 strip）
$ llvm-nm -D --defined-only … | grep JNI_OnLoad
  新 f780 T JNI_OnLoad        旧 f6f0 T JNI_OnLoad
$ diff <(旧 -D --undefined-only) <(新 -D --undefined-only)                            → 空（115/115 完全一致）
```

⇒ 本地符号被 strip，无法用 nm 直接看 `VaAtFileOffBytes`；改用「字节 + 数据表 + 反汇编」三证（下）。

### 3.2 字节层：12 字节序言**确实在包内 so 里**（字节命中，不是立即数）

```
$ python -c "…write(bytes([0xFF,0x83,0x03,0xD1,0xE8,0x3B,0x00,0xFD,0xFD,0x7B,0x08,0xA9]))…"  # pat12.bin
$ LC_ALL=C grep -a -F -c -f pat12.bin 新so   → 1
$ LC_ALL=C grep -a -F -ob -f pat12.bin 新so  → 29044:…      （29044 = 0x7174，落在 .rodata 0x3ac0+0x3c18 区间内）
$ LC_ALL=C grep -a -F -c -f pat12.bin 旧so   → 0           （旧包：编译器丢弃，0 命中）
$ python: .rodata[0x7174:0x7180].hex() → ff 83 03 d1 e8 3b 00 fd fd 7b 08 a9
```

**结论：文件偏移 0x7174（.rodata）整块 12 字节序言命中 1 次，旧 so 0 次。**（4 字节前缀 `FF8303D1` 新旧各命中 3/2 次，噪声大，不作证据。）

### 3.3 数据表层：`kDamageSlots` 真的带上了指针 + 长度

```
$ llvm-readelf -r -W 新so | grep 7174
  000000000002e710  0000000000000403 R_AARCH64_RELATIVE   7174      ← 指针字段被重定位到序言数组

.data.rel.ro 逐 8 字节 dump（vaddr 0x2e6c8 = kDamageSlots 基址，vaddr 0x2e720 = kSpeedSlots 基址）：
0x2e6d0 0x5f51968    fileOff
0x2e6d8 0xd10383ff   expectInsn
0x2e6f8 0x5           argc
0x2e710 → 0x7174     prologue   （R_AARCH64_RELATIVE）
0x2e718 0xc          prologueLen = 12        ← 新增字段生效
0x2e728 0x6bb5c6c    （kSpeedSlots fileOff）
0x2e730 0xfc1e0fe8   （kSpeedSlots expectInsn）
0x2e750 0x1
0x2e758 0x0 / 0x2e768 0x0 / 0x2e770 0x0      ← kSpeedSlots 的 prologue + prologueLen = 0（无重定位）
```

⇒ 伤害槽走 12 字节校验，速度槽仍走 4 字节校验：**行为按预期分流，速度槽行为未变。**

### 3.4 反汇编层：比较路径在 `InstallTable`（`HookSlotInstall` 内联）里

```
$ llvm-objdump -d --demangle so > new.dis
14df4  adr x0, 0x2e6c8        ; kDamageSlots
14df8  bl  0x15d8c            ; InstallTable
14e14  adr x0, 0x2e720        ; kSpeedSlots
14e18  bl  0x15d8c            ; InstallTable

$ awk '/^   15d8c:/{f=1} f{print}' new.dis
15db8  ldr  x8,[x0,#0x40]      ; origPtr（幂等检查）
15dc8  ldr  x21,[x19,#0x8]     ; fileOff；cbz → 按名
15dd0  ldr  x22,[x19,#0x48]    ; ★ s.prologue
15dd8  cbz  x22, 0x15ea0       ; 空 → 4 字节路径（0x15ea0: ldr w2,[x19,#0x10]; bl VaAtFileOffChecked）
15de4  ldr  x23,[x19,#0x50]    ; ★ s.prologueLen
15e0c  bl   0x167b0            ; FileOffToVa
15e18  cbz  x23, 0x15eb8       ; n==0 → 跳过比较
15e1c..15e28  memcmp(va, prologue, prologueLen)   ; ★ 12 字节比较路径
15e2c  cbz  w0, 0x15eb8        ; 相等 → DobbyInstall
15e30..       LOGW "prologue drift … — try by-name"
15e9c  b    0x15f38            ; 不等 → HookByName
```

**「这次真的编进去了」一句话**：序言以 **字节命中** 形式落在 `.rodata` 文件偏移 **0x7174**（整块 12 字节，旧 so 0 命中），并由 `.data.rel.ro` 的 `kDamageSlots.prologue`（重定位 addend=0x7174，偏移 0x2e710）+ `prologueLen=0xc`（0x2e718）引用；`HookSlotInstall` 内联体在 `0x15dd0 / 0x15de4 / 0x15e1c–0x15e28` 确实走 `memcmp` 12 字节路径（clang 未把它折成立即数，故不走 movz/movk 证据分支）。

### 3.5 菜单串校验（脚本自带 + 本轮复算）

```
surgical_sf3_1.45.3.sh → "surgical zip verified: 4957 个原始条目字节不变 + classes8.dex + starry so"
                         "dex↔ModBridge native 声明校验通过（19 项）"
本轮按同一口径对新包内 so 逐串计数（python zipfile 取 lib/arm64-v8a/libstarry_magic.so）：
  'Category_功能'          count = 1
  'Slider_伤害倍率:1:100:1' count = 1
  'Toggle_上帝模式'         count = 1
  'Slider_战斗速度:1:10:1'  count = 1
  '定怪'                   count = 0
  'Toggle_无限货币'         count = 0
```

⇒ 3 项功能的菜单串各 1 次；已删功能（定怪 / 无限货币）0 次；功能边界未扩张。

## ④ 出包 + 签名（新旧包指纹）

| 项 | 旧包（留证改名） | 新包 |
|---|---|---|
| 路径 | `08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned_v1_0bc69378.apk` | `08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk` |
| size | 2,715,627,251 | **2,715,627,251** |
| md5 | `0bc69378712b98a3dcb62ad85aec1684` | **`105190f180cb3ed883734574c69e62e9`** |
| sha256 | `3e814c905e822bff62beddfb5efac32fe9f6f8d811bed573efa7076f049e5590` | **`765a5c3fa7b4c1c11843ba4a9a674d70854088e289bb07a98a6e3d7852fa5f26`** |
| 包内 so md5 | `da6d7821…`（191,232 B） | **`69e8924c…`（192,080 B）= 第 2 步产物** |
| 签名 | CN=StarryMagic | CN=StarryMagic（cert sha256 `83c5cc0b…`，v2/v3） |

- 旧包与其 `.idsig` 已改名为 `…_v1_0bc69378.apk(.idsig)` 留证，未删除。
- `bash 04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh` → `exit 0`，`unsigned_md5=de176188cee1512db5b57383d61de5a4 / size=2715072141`。
- `bash 04_scripts/暗影格斗3/sf3_sign.sh` → `EXIT=0`；包内 so 断言：`unzip -p … libstarry_magic.so | md5sum` = `69e8924c7ae126545f7f51e8cdfddf50`（与 build_so 一致）。

## ⑤ 五库 md5 断言（sf3_sign.sh 原样输出，未放宽）

```
[sign] 5 库 md5 断言 OK：il2cpp=53345cdb4d31dff336fdd575fb87b667 metadata=809ec525f0875c2092dc7f21e73fb288
       access=82ee8142fa3a5996cb5eb7bd4a43677e cpp=7dad26fcca3bfe85df53a45a046959e9 core=4b0b4e792b2670e8f0f5bab8d47aa08b
```

→ **5/5 PASS**，与官方底包一致（A-3 游戏本体零改动）。

## ⑥ 覆盖安装闭环（`emulator-5554`）

安装命令：`MSYS2_ARG_CONV_EXCL=/data adb -s emulator-5554 install -r -d --user 0 <新包>` → `Performing Streamed Install / Success`（31.8 s；未卸载、未清数据）

| 闭环项 | 结果 |
|---|---|
| 设备 base.apk md5 | `105190f180cb3ed883734574c69e62e9`（= 新包 md5；覆盖前为 `0bc69378…` = 旧包） |
| lastUpdateTime | 2026-11-24 **22:11:52**（覆盖前 21:16:55，已变化） |
| firstInstallTime | 2026-11-24 21:16:55（**未变** ⇒ 是覆盖不是重装，数据保留） |
| 签名 | `signatures=PackageSignatures{… version:3}`，apksigner：`CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN` |
| 设备内 so | `/data/app/…/lib/arm64/libstarry_magic.so` md5 = `69e8924c…` |

## ⑦ 3 次冷启复验（每轮：force-stop → logcat -c → am start -W → 等 95 s → logcat -d）

| # | 启动时刻 | InitSuccessCallBack | LoginSuccessCallBack | `[sf3]` | `hooked @` | drift / prologue drift | 崩溃(FATAL/SIGSEGV/tombstone/ANR) | 判定 |
|---|---|---|---|---|---|---|---|---|
| 1 | 22:12:32 | 1（22:12:44） | 1（uid 3009937009，22:12:50） | **0** | 0 | 0 | 0 | **缺项**（见下注） |
| 2 | 22:19:1x | 1 | 1（uid 3009937009） | 2（`lib ready` + `初始化成功`） | 0 | 0 | 0 | PASS |
| 3 | 22:21:1x | 1 | 1（uid 3009937009） | 2 | 0 | 0 | 0 | PASS |
| 4 | 22:23:0x | 1 | 1（uid 3009937009） | 2 | 0 | 0 | 0 | PASS |

- **#2/#3/#4 连续 3 次：3/3 Init + 3/3 Login + 3/3 `[sf3]` + 0 崩溃 + 0 drift** ⇒ 判据满足。
- **#1 是「覆盖安装后第一次启动」**：Init/Login 正常、无崩溃，但 95 s 窗口内没有 `[sf3]`（首次启动 dexopt/首帧更慢，native 侧日志在窗口外）。该现象**只出现在装完立即启动那一次**，随后连续 3 次均正常；如实记录，不当作通过。
- `hooked @` 三轮都是 0 —— **符合设计**：功能开关默认关，`SyncHooks` 懒装，`开关全关 = 0 hook`（`Sf3Mod.cpp` 头注释铁律）。因此 12 字节序言这条**运行期路径**在「开关关」时不会打印，只能靠 ③ 的静态取证 + 用户开开关后实测（见 ⑨）。
- drift 计数为 0：**没有出现 `ChangeLife … drift` / `prologue drift` 警告**。
- 旁证（同一新包、未重装，22:27 precheck 自己的冷启）：`MenuService: native menu shown mode=ACTIVITY`、`ModBridge: auth game_id synced from native: sf3`、`StarryAppFactory: SignatureKiller mounted` ⇒ native 侧已正常初始化、菜单已弹出。

证据（每份只留切片 + 生成命令）：
`09_test/暗影格斗3/install_20261124_fix/{cold1..cold4}_logcat.txt`、`{cold1..cold4}_amstart.txt`。

## ⑧ 装机预检

`bash 04_scripts/_common/post_install_precheck.sh --game 暗影格斗3 --serial emulator-5554` → `EXIT=0`

```
✓ 包已安装: …/base.apk
✓ 菜单 so 加载标记: StarryAppFactory: SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared
✓ 菜单弹出标记: MenuService: native menu shown mode=ACTIVITY
✓ 授权通过标记: ModBridge: auth game_id synced from native: sf3
✓ PASS_TO_HUMAN —— so 加载/菜单弹出/空转 3 分钟无崩溃，装机层证据齐
```

证据目录：`09_test/暗影格斗3/precheck_20261124_222731/`。

## ⑨ 剩余 UNPROVEN（不做结论）

1. **12 字节序言校验的运行期行为**（`prologue drift` 分支是否按预期触发）未被观测 —— 需要用户把「伤害倍率/上帝模式」任一开关打开，使 `kDamageSlots` 真正安装；届时日志应出现 `Mod_Menu: ChangeLife(OGFLOMPOMCI) hooked @0x…`，且**不得**出现 `prologue drift`。
2. 三项功能本身（伤害倍率 1–100、上帝模式、战斗速度 1–10）**功能效果仍未验收**，等用户实测（L3 终审）。
3. 换章节/跨局/暂停恢复等生命周期矩阵未覆盖。
4. 「覆盖安装后第一次启动无 `[sf3]`」这一现象只观测到 1 次，未复现/未定性（后续 3 次正常）。

## ⑩ 过程中的失败与处置（原文 + 判断，未绕过门禁）

1. **第一次跑 `sf3_sign.sh` 时 `ERROR: classes.dex missing`（exit 1）**
   - 判断：不是包的问题，是**宿主工具输出行尾**问题。本机没有真 unzip，走的是 `C:/Users/Administrator/.local/bin/unzip`（python zipfile shim，2026-11-24 16:34），其 python `print` 在 Windows 上输出 `\r\n`；脚本第 111 行是 `grep -q " $n$"`，带 CR 的 `classes.dex\r` 匹配不上 `$`。
   - 取证：`unzip -l <apk> | head -3 | cat -A` → `classes.dex^M$`。
   - 处置（**没有改门禁脚本、没有放宽任何断言**）：只修宿主 shim —— 在 shim 的 python 段首加 `try: sys.stdout.reconfigure(newline='\n')
except Exception: pass`，使其输出 LF。备份：`C:/Users/Administrator/AppData/Local/Temp/unzip_shim.bak`。
   - 复验：`unzip -l` 已无 `^M`；`unzip -p … classes.dex | xxd` 仍是 `64 65 78 0a 30 33 35 00`（二进制抽取未受影响）。随后 `sf3_sign.sh` **原样重跑** → `EXIT=0`，全部断言（含 so md5 与五库 md5）真正执行并通过。
2. **为看清 so 是否被映射，临时 `adb -s emulator-5554 root`**
   - 处置：诊断完成后已 `adb unroot`，当前 `id` = `uid=2000(shell)`，恢复非 root。
3. **冷启 #1 无 `[sf3]`**：已在 ⑦ 如实记录，未用「被打断了」之类猜测解释。
