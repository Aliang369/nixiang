# 斌哥壳抓点完整流程（决策树版）

> 📍 路径：`re-extract/binge-memdiff.md`
> 🎯 触发场景：APK 有斌哥改包（签名 `CN=binge2.keystore` / `libSDKloading.so`），需要提取所有玩法 Hook 点
> 设计原则：**先查表，命中直接用；只有表里没有的才重新分析**。固定 URL / 路径 / 机制 / 方法均为已知量，禁止重复推导。

---

## 一、固定不变量速查表（查表即用，禁止重新分析）

### 1.1 包身份特征（识别判据）

| 检测项 | 特征 | 备注 |
|--------|------|------|
| APK 签名 | `CN=binge2.keystore`（OU/O 同 binge2） | 唯一权威判据 |
| lib 目录 | `libSDKloading.so` | 大小变体极多：586KB / 976KB / 1.0M / 3.7-3.9M packed / 23.7M，字符串全加密——**不要用大小排除** |
| lib 目录 | `libtoolChecker.so`（RootBeer） | |
| 伪装 so | `libcodec2_server.so`（导出 `initUnitySdk`）、`libunityluac.so`（Dobby 载体） | 官包无此 so，瘦壳变体特征 |
| Application | `bin/mt/signature/KillerApplication193/779`（MT ApkSignatureKillerEx）、`vip/pro/QUA/login/mini/SignatureSpoof`、`com.srp.patch.Init`（SRPatch 变体）、`com.pengyou.cloneapp.SplashActivity`（双开容器变体） | Application 变体只作线索不作判据（会换名） |
| 免责声明邮箱 | `bg888@188.com`（dialog_close_log.xml） | |
| 壳配置 | `shell_config.xml` / `shell_protected*.xml` | 全壳有，瘦壳无 |
| 嵌套原包 | `assets/base.apk` | 全壳有（与外层 libil2cpp 字节一致，2026-08-25 实锤）；瘦壳无 |
| shared_prefs | `{pkg}_preferences.xml` boolean `"0"`/`"1"`/`"2"`… | 功能开关持久化 |
| Java 桥 dex | `com.android.support.{Main,Menu,Preferences,BinGe,BUFF}`（新增 classesN.dex） | Main 含 `kaiguan`~`kaiguan7` 静态开关字段 |

**判定以「签名 + so + 免责邮箱」组合为准**。斌哥包游戏 so（libil2cpp / metadata）通常与官包字节一致，hook 全部运行时注入 → 玩法提取靠运行时差分，静态 dump 用嵌套原包（全壳）或壳内 so（瘦壳，先 md5 对比官包确认一致）。

### 1.2 远程菜单 URL（固定模板）

```
https://menu-1255602259.file.myqcloud.com/game/<id>.html
```

**game_id 已知映射**（新游戏先查此表，命中即得）：

| game_id | 游戏 |
|---------|------|
| 87 | 偶像梦幻祭2 |
| 231 | Phigros |
| 720 | 独奏骑士-BUFF（双开容器） |
| 931 | 无尽探险队 |
| 1161 | 弓箭传说2（com.xq.archeroii.ua） |
| 1189 | 圣翼传说（com.aiyue.wscq） |

新游戏 game_id 查法（二选一）：① 从壳/进程内存抓 URL 串；② 浏览器直接打开菜单实测。
菜单 `app1.js` 重度混淆、配置走 exp 加密串——**不逆**（无信息增益，功能卡直接浏览器实测）。

### 1.3 壳变体分类

| 变体 | 识别特征 | 对抓点的影响 |
|------|----------|--------------|
| 全壳（经典） | 嵌套 `assets/base.apk` + `shell_config.xml` | 静态 dump 用嵌套原包更干净 |
| 瘦壳 | 无嵌套、无 shell_config；伪装 so 混入 lib | il2cpp 与官包字节一致（阿瓦隆/偶像梦幻2/Phigros 实锤），hook 运行时注入 |
| SRPatch | `com.srp.patch.Init` + `libSRPatch.so` + `SRPatch_config.json` | 同瘦壳思路 |
| 双开容器 | 宿主包（如 `com.Shimmer.BGSoloKnight.cn`）+ `com.pengyou.cloneapp` | attach guest 游戏进程再 dump，勿拆宿主当游戏 |

### 1.4 开关链（固定机制，不用重新推导）

```
菜单 WebView Changes(n, true)
  → prefs {pkg}_preferences.xml boolean "n" 写入
  → 同步壳数据段/BSS 旗标（int32/byte 0→1）
  → 游戏侧 get_ret(n) 读取（≈ prefs "n-1"，1-based JNI）
  → hook 函数内读旗标决定行为
```

- Cocos 系游戏侧调用：`jsb.reflection.callStaticMethod(AppActivity, "get_ret", "(I)Z", n)`
- 功能靠**旗标门控**，不靠增删 stub → OFF vs ON patch 数量应一致
- ⚠️ 模板残留：BSS/注入 JS 中的 Activity 类名可能是壳模板残留（如 `org.cocos2dx.javascript.AppActivity`），以当前游戏实际 Application/Activity 为准

### 1.5 Hook 注入形态（固定四型，决定抓点路径）

| 形态 | 特征 | 实例 | 对应路径 |
|------|------|------|----------|
| JS 注入型 | Dobby hook `evalString`，注入 JS 覆写 getter | 无尽探险队（与造梦无双同构） | A |
| 无条件安装型 | 启动即打桩，OFF/ON 三态 memdiff **零差异** | 偶像梦幻2 / Phigros | B |
| 旗标门控型 | hook 常驻，开关只改 .data 旗标 | 阿瓦隆之王 / 弓箭传说2 | C |
| HybridCLR 方法名过滤型 | hook 解释器/方法调用共享入口，strcmp 过滤方法名分发 | 独奏骑士 | D |

---

## 二、斌哥游戏档案表（命中直接复用，版本没变零分析）

| 游戏 / 版本 | 引擎 | 壳变体 | game_id | 抓点方法 | 抓到功能 | 文档 |
|---|---|---|---|---|---|---|
| 阿瓦隆之王 24.7.21 | Unity IL2CPP | 瘦壳 | — | C：三态 memdiff + 桩反汇编 | 秒杀 / 无限技能 | `10_docs/阿瓦隆之王/斌哥包抓点_20260911.md` |
| 无尽探险队 26.1.5 | Cocos Creator 3.8.7 | 全壳 | 931 | A：注入 JS 提取 | 超级无敌 / 超级移速 | `10_docs/无尽探险队/斌哥包抓点_20260910.md` |
| 偶像梦幻2 4.4.8684 | Unity IL2CPP + xLua | 瘦壳 | 87 | B：磁盘 vs 内存 + trampoline | 自动跳舞（判定恒 6） | `10_docs/偶像梦幻2/分析_斌哥包抓点_20260908.md` |
| Phigros 3.20.0 | Unity IL2CPP | 瘦壳 | 231 | B：磁盘 vs 内存 + trampoline | 满分 | `10_docs/phigros/分析_斌哥包抓点_20260908.md` |
| 无悔华夏 3.8.280 | Unity IL2CPP | 全壳 | — | prefs/get_ret 映射 + Java 桥 jadx | — | `10_docs/无悔华夏/斌哥包识别_20260826.md` |
| 独奏骑士-BUFF 1.1.760 | Unity IL2CPP + HybridCLR | 双开容器 | 720 | D：方法名过滤反汇编 | 百倍伤害 / 无敌 | `10_docs/独奏骑士/斌哥BUFF容器_菜单机制提取_20260820.md` |
| 弓箭传说2 1.1.8 | Unity IL2CPP + HybridCLR | 全壳 | 1161 | 壳 BSS 字符串 + 旗标差分 | 偷技能 / 隐身 / 全屏攻击 | `10_docs/弓箭传说2/斌哥功能对照_20260817.md` |
| 雷霆战机 1.25.31 | Cocos JS | 全壳 | — | 菜单早载机制分析 | — | `10_docs/雷霆战机/斌哥菜单早载机制_20260816.md` |
| 圣翼传说 7.5.10 | Unity IL2CPP | 瘦壳 | 1189 | B：磁盘 vs 内存 memdiff（15 跳桩）+ 壳内中文串 | 百倍伤害 / 玩家无敌 / 战斗速度 / 怪物+BOSS瞬刷 / 全屏攻击 | `10_docs/圣翼传说/斌哥改包抓点_20260924.md` |

> 新游戏抓点完成后，**回填本表一行**（这是每次接手的第一入口）。

---

## 三、抓点决策树

```
第 0 步：查 §二档案表 —— 同游戏且版本未变 → 直接复用文档 hook 点，跳到复刻
第 1 步：查 §一速查表识别（组合判据，几分钟出判定，不做无谓深挖）
第 2 步：判引擎 → 分流：
   Cocos JS/Creator（libcocos*.so / 改名引擎 so + .jsc）      → 路径 A（优先，成本最低）
   Unity IL2CPP（libil2cpp + global-metadata.dat）              → 先 B 后 C
   HybridCLR 热更（AOT dump 找不到玩法方法 / 热更 dll）         → 路径 D
第 3 步：功能清单确认（§3.5，三路径通用）
第 4 步：复刻衔接（starry-mod / new_game_mod.sh）
```

### 路径 A：注入 JS 提取（Cocos 系，成本最低，优先尝试）

1. 真机跑斌哥包进游戏（MuMu 可能崩，见 §六坑 1）
2. KSU `/proc/pid/mem` dump：`libSDKloading.so` rx 段 + **BSS 附段（sdk_bss.bin，含注入 JS 池）** + rwxp stub 段
3. sdk_bss 中搜 `isChecked_RET` / `get_ret` → 直接提取注入 JS 段
4. **JS 源码即答案**：getter 名 = hook 点；`[type 数组].includes(this._type)` = 对象过滤；else 分支 = 原逻辑保留语义；`get_ret(n)` 的 n = 菜单功能位
5. 注意剔除模板残留 JS（非本游戏生效）

实例（无尽探险队）：
```javascript
get invulnerable(){var isChecked_RET=jsb.reflection.callStaticMethod("...GameActivity","get_ret","(I)Z",1);if(isChecked_RET){if([1,4,5,7].includes(this._type)){return true}}return this._invulnerable}
```

### 路径 B：磁盘官包 vs 内存 diff（IL2CPP 无条件安装型）

**适用**：三态 OFF/ON memdiff 零差异时（hook 启动时无条件安装），或瘦壳直接走此路。

1. dump 运行内存 `libil2cpp.so` 全 text 段
2. 与磁盘 so（瘦壳 = 与官包一致，先 md5 确认）逐段 diff
3. 每处差异 = patch 入口，特征固定：**12 字节 3 指令 `ADRP X17, trampoline; ADD X17, X17, #imm; BR X17`**（跳桩）；4KB 拆页 = Dobby 典型特征
4. **trampoline 反汇编**（结构固定）：
   ```
   x8 = [壳数据段 + 指针表偏移]   ← 旗标指针（或直接壳数据段偏移）
   读旗标 byte/int，bit0 门控
   ON  → BLR 增强函数（壳 rx/rw 段内，指针表另一组槽位）
   OFF → 调原函数副本（壳拷贝到 anon 段）
   ```
5. RVA → 方法名：metadata 未混淆 → Il2CppDumper `dump.cs`/`script.json` 反查；**官方加密/混淆（不可见 Unicode）→ script.json 地址体系损坏不可用**，改「字节特征 + 运行时证据」锚定（序言校验字节，版本漂移即拒）
6. 实例：偶像梦幻2（4 处 patch：3 判定 getter + 1 静态标志）、Phigros（全 36MB text 段仅 1 处 diff）

### 路径 C：三态 memdiff + 桩反汇编（IL2CPP 旗标门控型 / B 失败兜底）

三态 dump：`off`（全关）/ `on_1`（单项开）/ `on_2`（另一项开）……逐功能：

```bash
bash 04_scripts/_common/dump_binge_shell_mem.sh <pkg> <outdir>   # -s 指定 serial
```

1. **il2cpp 三态对比**：若三态一致但与磁盘有固定差异 → 实为无条件安装型，转路径 B
2. **旗标差分**：`sdk_rw.bin`（libSDKloading .data）找 off=0 → on=1 的字节；阿瓦隆实锤旗标在 `.data +0x9D0`（秒杀）/`+0x9D1`（无限技能），间隔 +1~+4
3. **桩函数反汇编**：libSDKloading **符号表直接暴露中文桩名**（`_Z6伤害Pv` @0x64754、`_Z7伤害2Pv`…），读 flag → tbnz bit0 → ON 返回大值（如 float `0x4b18967f`≈1074 万）、OFF `blr x8` 调原 getter 放行——桩名即功能语义
4. **RVA 反查**：memdiff 差异点（file 偏移 = 原函数序言，内存 = 跳桩）→ script.json 反查方法名
5. **BSS 字符串扫描**：壳 BSS 自检字符串（如 `怪改血=0x306E8FC`、`featNum=n`）记录 Hook 点地址，最可靠定位手段之一；壳 BSS 还会存 IL2CPP 绑定候选方法名（弓箭传说2：`HotFix.BattleLogic` 命名空间候选表）

### 路径 D：HybridCLR 方法名过滤型（热更游戏）

1. 壳 hook HybridCLR 方法调用/解释器共享入口（独奏骑士实锤：hook `LDR X0,[X8,#0x3F8]` 处 → B veneer）
2. replacement 逻辑固定套路：**从 x1（MethodInfo）提方法名 → 运行时 XOR 解密字符串表 → strcmp 过滤 → 命中按功能 flag 改参数/返回值 → 必调 orig**
3. 功能对照靠反汇编逐个提取（方法名 + 判定 + 行为三元组），实例见独奏骑士文档（`calTotalDamage`/`calDamageBuffer`/`AddRtHPTo`）
4. 壳 BSS 字符串给热更绑定候选（弓箭传说2：`EntityCharacter.UpdateHp`/`RandomSkillResult.get_ShowSkills` 等）

### 3.5 功能清单确认（三路径通用，防漏抓）

1. **浏览器直接打开菜单 `game/<id>.html`**（条款同意后）——功能卡数量/名称/类型（开关 or 参数型）一目了然（无尽探险队方法）
2. prefs 差分：boolean `"0"`-`"N"` 对应功能位
3. 验证：OFF vs ON patch 数量一致（旗标门控）+ 旗标 0→1 + 方法名与功能语义匹配

---

## 四、解密按需触发原则（禁止默认解密）

**复刻功能只需要 hook 点 + 门控语义**——多数情况无需任何解密（无尽探险队实锤：注入 JS 已足够复刻全部功能）。

| 解密对象 | 触发条件（只有此时才做） | 不触发时的替代 |
|----------|--------------------------|------------------|
| `.jsc` XXTEA | 需读游戏原生 JS 逻辑（注入 JS 不够用时） | 路径 A 的 JS 已够 |
| global-metadata 官方加密 | 需离线 Il2CppDumper 反查方法名 | 运行时枚举 / 字节特征锚定 |
| libSDKloading packed 字符串 | 需静态读壳逻辑 | **一律运行时 dump，不做静态解密** |
| 斌哥 assets 菜单数据（`zlsioh.dat`/`uc_dsl.bin`/`tt_nd` ELF 伪装） | — | 菜单 HTML 实测已足够，不解 |
| 热更 dll（如 Solo.GameCode.dll 加密存储） | 需读热更原生逻辑 | 路径 D 按方法名拦截已够 |

---

## 五、BSS 旗标偏移速查表（历史实测，仅供定位思路）

| 游戏 | 旗标起始偏移 | 间隔 |
|------|-------------|------|
| 造梦西游 / 镇邪人 / 雷霆战机 | `+0x1798` | `+4` |
| 追迹迷城 / 霓虹深渊 / 物质与魔法 | `+0x14C0` | `+4` |
| 魂坠深境 | `+0x1728` | `+4` |
| 音舞光年 | `+0x172c` | — |
| 消消英雄2 / 孤勇无双 | `+0x4C0` | `+4` |
| 弓箭传说2 | `+0x13C0` | `+4` |
| 阿瓦隆之王 | `.data +0x9D0` | `+1` |
| Phigros | 壳数据段 `+0x280` | — |
| 偶像梦幻2 | sdk_rw 段 `+0x1000` 指针表，旗标槽 `+0x3E8/+0x410` | — |

> ⚠️ **偏移跨游戏/跨版本不可沿用，必须差分实测**（弓箭传说2 经典 +0x14C0 即不成立）。此表只证明结构相似（sdk_rw 段内、0/1 门控），方法固定、偏移每次实测。

---

## 六、坑/弯路（实战教训，勿重复）

1. **MuMu 上斌哥包必崩**（偶像梦幻2：12 次 native crash 全部 uptime 0s，壳 dlsym 失败/模拟器分支）→ 真机一次点同意即正常。判 MuMu 环境问题，**不修壳**，换真机 dump。
2. **三态 memdiff 零差异 ≠ 没有 hook**（无条件安装型）→ 立即转路径 B（磁盘 vs 内存）。
3. **双 Dobby 叠加 SIGSEGV**：星光 + 斌哥壳双 Dobby 叠同一函数会崩。复刻时替换斌哥 hook 须先恢复原入口指令（复刻包基于官包则无此问题）。
4. **dump 脚本坑**：连续多个 `[anon:.bss]` 后者覆盖前者；toybox `dd` 大块溢出 → 用 `memdump64` C 工具补 dump 大段。
5. **libcocos 主段字节漂移 ≠ patch**：Cocos Creator 3 加载对齐差，差分时排除已知对齐漂移区域。
6. **metadata 混淆时 Il2CppDumper 不可用**（Phigros：方法名不可见 Unicode，script.json 地址体系损坏）→ 字节特征 + 运行时证据锚定，勿反复尝试反查。
7. **模板残留**：BSS/注入 JS 中的 Activity 类名、残留 JS 段是壳模板残留，非本游戏生效，以当前游戏实际为准。
8. **va↔file 偏移必须逐 ELF 核对**：多数 Unity so `vaddr = file + 0x4000`（LOAD2 段差），但不可假设——Phigros 实测 `0x1AF0A7C` RVA / `0x1AECA7C` file。
9. **瘦壳 libil2cpp 先与官包 md5 对比**：一致（多数）则 RVA 免重定位直接复用官包 dump；不一致才重新 dump。

---

## 七、验证方法

- OFF vs ON patch 数量一致（功能靠旗标门控，不靠增删 stub）
- BSS 旗标 OFF=0 → ON=1（运行时实测）
- 方法名与功能语义匹配（或字节特征 + 运行时证据）
- 菜单 HTML 功能卡数 = 抓到的功能数（防漏抓）
- logcat 确认功能生效 / 用户实测
