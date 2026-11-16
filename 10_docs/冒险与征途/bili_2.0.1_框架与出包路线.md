# 冒险与征途 B站渠道 2.0.1 — 框架侧与出包路线调研（2026-11-16）

> 范围：只做静态阅读与文档核对，**未改任何代码、未出包、未碰设备运行态**。
> 目标包：`com.my.mxyzt.bilibili` / versionName 2.0.1 / versionCode 201 / arm64-v8a / Android 16（root 真机）。
> 历史交付：TapTap `com.mxyzt.gzmy` 1.5.9 (vc159)，game_id `mxyzt`，四项功能（秒杀小怪 / 无敌 / 全模式技能伤害增加 / 免广告）。
> 证据分级：**[实测]** 本轮亲自读到的文件/行 ｜ **[文档]** 仓库既有记录 ｜ **[推断]** 有证据链 ｜ **[待验证]** 无证据，需后续确认。

---

## 0. 结论先行

| 问 | 结论 | 级别 |
|---|---|---|
| 推荐 game_id | `mxyztbili`（备选 `mxyzt201`、`mxyztb`） | [推断] 规则+先例双重支持 |
| 是否要改 GameConfig.h | **不要**。走 modern 注册表：新 `Mxytz*Entry.cpp` + 私有 `Android_mxyztbili.mk` | [实测] `Games/JjdmIntlEntry.cpp:5`、`SWITCH_GAME.md` 双轨表 |
| 授权是否需后台在线 | **必须**。票由服务端私钥签，且票内 `game` 字段要与新 id 逐字节相等 | [实测] `Includes/SgAuth.h:247-259` |
| 缺后台的后果 | 登录/续票拿不到票 ⇒ `live()`=false ⇒ **四项功能全部静默失效**（不闪退、菜单照常弹出） | [实测] `SgAuth.h:176-198` |
| B站相对 TapTap 多什么 | 有出处的只有 4 条（包名后缀 / 官方 Application 父类 / 乐变类 overlay 的 manifest 双处 / dex 布局不可互推）；签名校验与反调试**未确认** | 见 §3 |
| 出包三段 | `mxyzt_build_so.sh` → `mxyzt_build_dex.sh` → `STARRY_INSTALL=1 surgical_*` + `verify_installed_apk.sh`；但现有 `pipeline.json` 是 **win+MuMu+1.5.9**，本机不适用 | [实测] `pipeline.json` params/env |
| 菜单 dex | 不是天生 classes4，是「官包 classes/1/2/3 已占 ⇒ 取最小空闲下标」；换渠道要按新底包重查下标 | [实测] `mxyzt_build_dex.sh:8-9` |

---

## 1. game_id 决策

### 1.1 规则出处（原文口径）

| 规则 | 出处 |
|---|---|
| `game_id` = 官方中文全名拼音首字母，小写、无分隔符 | `skill/starry-mod/SKILL.md:56-58` |
| 虚词「的/之/与」计入，数字原样保留；已上线 id 不改（grandfather） | `SKILL.md:60-66`；`SWITCH_GAME.md` §game_id 命名 |
| 四点唯一同步：`STARRY_AUTH_GAME_ID` = 后台 `games.game_id` = 菜单 `template_id` = 文档 JSON | `SKILL.md:65`（modern 路线下第一项由 Entry 的 `authGameId` 承担，见 1.2） |
| **新渠道/新包禁止复用其他渠道 game_id**（历史事故：临时复用同 id → 后台模板错位、返工重建 so） | `SKILL.md:304`；`AGENTS.md` §4「game_id 跨渠道独立」 |
| 宏/文件名可短，鉴权串必须合规；`.tag` 与 `authGameId` 可以不同 | `SKILL.md:70-71`；`Games/Dwct1242Entry.cpp`（tag=Dwct1242 / authGameId=dwct） |

### 1.2 仓库现有先例（`grep -rn 'authGameId' Games/*Entry.cpp` 统计）

- 单渠道（id = 全名首字母）：`mxyzt`、`hzsj`、`jjdm`、`gjc2`、`whhx`、`fkssj`、`wdjnwcf`、`lmq20fz`、`999mxcz`、`abssnmxt` 等。
- **渠道后缀型**（本轮直接对标）：

| 先例 | id | 后缀语义 | 出处 |
|---|---|---|---|
| 无悔华夏 B站 | `whhxbili` | `bili` | `GameConfig.h:87`；`Android_whrhxbili.mk`；`10_docs/无悔华夏/B站渠道_3.8.305_无壳星光_20261106.md` |
| 无悔华夏 4399 / 快爆 | `whhx4399` / `whhxkb` | `4399` / `kb` | `GameConfig.h:85,91` |
| 弓箭传说2 国际 | `gjc2intl` | `intl` | `Games/Gjc2IntlEntry.cpp:47` |
| 境界刀鸣 国际 | `jjdmintl` | `intl` | `Games/JjdmIntlEntry.cpp:21`（注释明写「先例 gjc2 → gjc2intl，勿与国服共用」） |
| 灵魂战纪 TapTap | `tlgdtt` | `tt` | `Games/TlgdTapTapEntry.cpp:12,31` |
| 灵魂战纪 4399 | 实取 `tlgd`（非 tlgdm4399） | — | `Games/TlgdM4399Entry.cpp:12,43`（历史 id 让位，勿仿） |
| 版本/线号型 | `dwct1242`(authGameId 仍 dwct)、`wxsjdz1/2` | 版本号 / 线号 | `Games/Dwct1242Entry.cpp:9`、`Wxsjdz1Entry.cpp:5` |

**规律**：跨渠道隔离一律「基础 id + 渠道后缀」，后缀用可读词（`bili`/`intl`/`4399`/`kb`/`tt`），不用单字母。

### 1.3 候选与推荐

| 候选 | 评价 |
|---|---|
| **`mxyztbili`（推荐）** | 与 `whhxbili` 完全同构（本仓唯一 B站渠道先例）；一眼看出渠道；与 `mxyzt` 无前缀歧义；本轮 `grep -rn mxyztbili` 全仓 **零占用** |
| `mxyzt201` | 版本线型（`dwct1242` 先例），但本项目下个版本还会换 ⇒ id 会漂；TapTap 2.0.1 与 B站 2.0.1 将来并行时无法区分渠道。仅当「B站只有这一条线」时可用 |
| `mxyztb` | 可行但违反「后缀可读」惯例，与 `bilibili`/`bili` 不统一，不建议 |

> 取 id 前要在本目录留一行推导（`SKILL.md:68`）：**冒险与征途（B站渠道）→ 基础 id `mxyzt` + 渠道后缀 `bili` = `mxyztbili`**。

### 1.4 注册与失败行为

- **要后台注册**：`games.game_id = mxyztbili` 新增一行 + 卡密 + **功能位掩码**（bit1..bit4 对应 featNum 1–4，见 §2）。运营侧操作，编译期无法代写（`SWITCH_GAME.md` 映射表末行）。
- **注册不到时的行为**（证据链）：
  1. 票里 `game` 字段与新 id 不等 ⇒ `apply()` 直接 `return 0`（`SgAuth.h:259`）；
  2. 握手/续票失败 ⇒ `ticket_ok=0` ⇒ `live()` false ⇒ `feat_ok(i)` 全 false（`SgAuth.h:176-198`）；
  3. 结果 = **功能静默失效**，不闪退；菜单能弹、能点，勾了没效果（`SgAuth.h` 头注释④）；
  4. 若服务端对未知 game_id 回含「不存在」的错误 ⇒ `isHardAuthFailureJson` 命中 ⇒ 冷启动 `hardKickAuth` **踢回登录页**（`NativeMenuController.java:3408-3419` 关键字表；具体响应体 **[待验证]**）；
  5. 掩码为 0 视为后台停用，同样 `return 0`（`SgAuth.h:271`）⇒「注册了但没勾功能位」与「没注册」表现相同，排障先看 `[V2] s=1 m=<mask>` 的位。

---

## 2. 授权链是否依赖后台在线

### 2.1 现行实现（单一实现，**不要拿旧文档当现状**）

- 头注释明写：`Includes/SgAuth.h` 是授权链**唯一实现**，`Main.cpp`/`MainModern.cpp` 只留委托（`SgAuth.h:3-11`；`MainModern.cpp:40,106-131`）。
- 票格式 v2：`2|device|game|exp|mask|kid.<512hex RSA 签名>`（`SgAuth.h:24`）。
- 验签：**RSA-2048 公钥在 so 里、私钥在服务端** ⇒ so 剥出来也签不出票（`SgAuth.h:15-17,222-231`）。
- 字段校验：`strcmp(dev,device)` 且 `strcmp(gm,game)`，过期 `now > exp+45` 拒（`SgAuth.h:247-263`）。
- 会话状态：`K = sha256("sgk1|"+签名hex+"|"+device)`；`live()` 三判据（票标志 / K 自检 / 可选完整性）（`SgAuth.h:114-134,176-192`）。
- **寿命**：登录成功后**进程内永不过期**，只有进程重启 / `nativeClearGrant` / 停用票才清（`SgAuth.h:33-35`；`auth-chain.md`「登录永不过期」）。

### 2.2 结论

> **换 game_id 后必须后台同步，否则功能静默全失效。**

- `authGameId` 不是「只在本地派生」：本地只有公钥，票必须由后台用私钥针对**新 game_id + 该设备 device_id + 功能掩码**签发。
- Java 侧 game_id 唯一来源是 so：`ModBridge.syncAuthGameIdFromNative()` 用 `nativeGetAuthGameId()` 覆盖 prefs（`ModBridge.java:196-205`；native 侧 `MainModern.cpp:483-491` 读 `mod->authGameId`）。⇒ **只改 Java/后台、不改 so 里的 Entry 不生效，反之亦然**。
- 带 id 的请求：`/v1/bootstrap?game_id=…&device_id=…&logged_in=…`（`NativeMenuController.java:2769-2776`）；登录/解绑 body `{"device_id","game_id","card_key"}`（同文件 2732-2734）。
- 离线宽限：票 TTL 由服务端 `offline_cache_seconds`（历史 86400s）决定，断网重开在宽限期内可玩；**首次授权必须联网**。

### 2.3 文档历史 vs 现状实现（§1.4 口径，登记在此不另开）

`auth-chain.md`「Native 门控」节仍描述 `session_live()` 只看 `g_rt_ok`、`apply_grant_utf8()` 走 HMAC pepper —— 那是 **2026-10-26 之前**的实现。现状：`live()` 由 `SgAuth.h` 提供（`MainModern.cpp:106-108` 只 `return sgauth::live();`），pepper 现在只剩 OTA 清单签名用途（`SgAuth.h:56-77`）。**排障按代码，不按该节。**

---

## 3. B站渠道差异面（只列有出处）

| # | 差异 | 出处 | 对本作的动作 |
|---|---|---|---|
| 1 | 包名带 `.bilibili` 后缀（`com.yhhy.whrhx.bilibili` / `com.bilibili.priconne`） | whrhx / 公主连结各文档 | 包名贯穿 surgical 脚本 / 模板 json / precheck `--pkg` |
| 2 | 渠道自有 Application 类，**StarryApp 必须继承官方 Application**（如 `Lcom/bilibili/priconne/App;`），否则权限链崩 | `skill/starry-mod/surgical-build.md:151`；`10_docs/公主连结/属性敌我GC句柄与999999输入框_20260829.md:51` | 按 2.0.1 底包 Manifest 的 `android:name` 现查父类 |
| 3 | 若底包是乐变 overlay：Manifest **两处**都要改（`android:name` + `meta-data LEBIAN_APPLICATION`），只改一处则主进程不跑 StarryApp（无 so 映射、无菜单） | `10_docs/无悔华夏/B站渠道_无壳星光_20261021.md` §5 | 先判本包是否乐变；是则双处改 |
| 4 | 渠道间 dex 布局不可互推：同版本 3.8.305 下 九游 13 份 / 4399 6 份 / **B站 6 份**，主 dex 大小各不同 | `B站渠道_3.8.305_无壳星光_20261106.md` §2 | 菜单 dex 下标必须按新底包现查 |
| 5 | B站发行包可能挂 ACE/TSS（`com.ace.gshell.AceApplication`、`libtersafe2.so`/`libtprt.so`）+ bilibili GSC SDK + Bugly | `10_docs/公主连结/公主连结_11.7.2_TSS-ACE校验定位与探针实验_20260814.md:30,73`；`公主连结_改包初判_20260813.md:17` | 若 2.0.1 带 ACE，`NoAceStubs.cpp` 的取舍要重新判定 |
| 6 | 包内是否自带 `libc++_shared.so` 因包而异：whrhx B站自带 ⇒ 不注入；mxyzt 1.5.9 官包**没有** ⇒ 必须补 | `B站渠道_3.8.305…` §3；`surgical_mxyzt_1.5.9.sh:8,70,103` | 2.0.1 底包重新 `unzip -l` 判定 |

**「无悔华夏 B站 6dex + RVA 零重标」的准确含义**：6 份 = 该包真实业务 dex 数；零重标 = B站 3.8.305 的 `libil2cpp.so` 与九游同版本 **sha256 字节一致**（`af1e80fe…`）⇒ RVA 表直接复用（同上 §1）。**本作 2.0.1 与 1.5.9 跨大版本，绝不能假设零重标。**

**未确认（无出处，不要当结论用）**：
- B站渠道自有的签名校验 / 反调试 / 登录壳 —— 全仓 grep（`B站|bilibili`）只命中上表与 `com.bilibili.priconne`，无其他出处；
- 本包 2.0.1 是否内置 B站 SDK so（`libbili*.so` 等）、是否走 B站账号登录 —— **待分析线静态确认**。

---

## 4. 出包管线

### 4.1 现状：`pipeline.json` 是「win + MuMu + 1.5.9」的，本机不适用

`04_scripts/冒险与征途/pipeline.json` 关键字段（`python3 -c` 提取，未 `cat`）：

```
params: game_id=mxyzt  package=com.mxyzt.gzmy  tag=mxyzt  serial=127.0.0.1:16384  version=1.5.9  version_code=159
env:    NIXIANG=E:/nixiang   ANDROID_NDK_HOME=C:/Users/Administrator/.../ndk/27.0.12077973
workdir: E:/nixiang/04_scripts/冒险与征途
```

⇒ 本机（mac + 真机 `com.my.mxyzt.bilibili`）应新建 `04_scripts/冒险与征途bili/` 的 pipeline，或复制本 pipeline 只改 params/env（`pipeline.sh` 头注释第 4 条：合法参数修改走 `params`）。**机械操作仍只能经 `bash 04_scripts/pipeline.sh <游戏> …` 或该游戏 `surgical_*` 脚本，禁裸敲 adb/zip/ndk-build**（`AGENTS.md` §3.2）。

阶段链（现 pipeline 11 阶段）：`check_apk → dump → anchor → impl_mod → build_menu_dex → build_so → surgical → device_ready → install → archive → archive_close`。
用法：`pipeline.sh <游戏> {status|next|run <stage>|reset|verify|log|round|rounds|gate}`（`04_scripts/pipeline.sh:21-31`）；上游产物变化 → 下游自动回退 pending（陈旧度门禁）。

### 4.2 build_so（`04_scripts/冒险与征途/mxyzt_build_so.sh`，179 行）

```bash
bash 04_scripts/冒险与征途/mxyzt_build_so.sh
# 产物：mod菜单/starry-mod-menu/libs_mxyzt/arm64-v8a/libstarry_magic.so
```
- 直编私有 `Android_mxyzt.mk`（不切公共 `Android.mk`），独立 `obj_mxyzt` / `libs_mxyzt`；
- 防呆四件套：① `NDK_OUT/NDK_LIBS_OUT` 绝对路径（相对路径被解析到 `app/` → 校验到旧 so 假成功）；② mk/开关/EPOCH 变化 → 清 obj 全量重编（obj stamp）；③ 有新 `.o` 但 so 字节未变 = 旧 so 顶包 → FAIL；④ 四条菜单串 `LC_ALL=C grep -ac` 按字节校验（`strings` 不认 UTF-8 中文）；
- 中文路径（`mod菜单`）自动转 `/tmp/mxyzt_build` ASCII 暂存构建再拷回（mac 分支已实现，不依赖 cygpath）；
- 构建戳 `STARRY_BUILD_EPOCH` 从仓库根 `git log -1 --format=%ct` 取（查8 / OTA 守卫依赖）。

### 4.3 build_dex（`04_scripts/冒险与征途/mxyzt_build_dex.sh`，218 行）

```bash
bash 04_scripts/冒险与征途/mxyzt_build_dex.sh
# 产物：mod菜单/starry-mod-menu/templates/mxyzt_menu_current/classes4.dex
```
- 无 gradle/smali：`javac --release 11 -classpath android.jar`（**不能** `-bootclasspath`，否则 lambda 缺 `LambdaMetafactory`）→ `jar` → **R8 混淆**（`_common/obfuscate_menu_dex.sh`，业务类改到 `com/starry/magic/o/**`，组件与 native FindClass 目标留原名）；
- 字节自检：必须含 `StarryInitProvider / MenuService / ModBridge / SignatureKillerBootstrap / StarryNativeLoad`，**禁止**出现 `Lcom/starry/magic/sign/SignSpoof;`（已删类复活即 FAIL）；
- 末步 `_common/menu_tpl_gate.sh`：`build_info.source_commit` 必须 = 当前 Java 源码 HEAD（查10）。

### 4.4 surgical / install

```bash
bash mod菜单/starry-mod-menu/scripts/surgical_mxyzt_1.5.9.sh               # 只出包
STARRY_INSTALL=1 SERIAL=<serial> bash …/surgical_mxyzt_1.5.9.sh            # 出包 + 装机
bash 04_scripts/_common/verify_installed_apk.sh <serial> <pkg> <本地APK> --su  # 设备 base.apk md5 == 本地
bash 04_scripts/_common/post_install_precheck.sh --game <游戏> [--pkg <pkg>]    # 预检全过才请用户上手
```
外科组装（`surgical_mxyzt_1.5.9.sh` 结构）：逐条目复制底包 → 只替换 `classes?dex` / `lib/arm64-v8a/libstarry_magic.so` / `libc++_shared.so` / `assets/certs/starlight.crt` → `zipalign -f -p 4` → `apksigner`（`CN=StarryMagic`，v1/v2/v3）；so 指纹**现算**（可用 `SO_SHA256_EXPECT` 锁定），打包前三查：四条菜单串、`[mxyzt]` 标签、`libil2cpp.so` 等待库名。

### 4.5 装机门禁（`04_scripts/_common/stages.md`）

- **`pre_install_gate.sh` 现 11 查生效**：查1 JS 语法 / 查2 JNI 配套 / 查3 P/Invoke 符号 / 查4 控件读取 / 查5 门禁串字面量 / 查6 轮次根因 / 查7 Manifest AXML 形态 / 查8 so 构建戳 / 查9 so–metadata 配套 / 查10 菜单 dex 新鲜度 / 查12 TapTap 方案关；**查11「加固凭证」2026-10-27 已下线**（`stages.md:74-91`）。有 FAIL 即禁装机（exit 1）；IL2CPP 直装包必须传 `--apk … --base-apk …`（查9）。
- **干净安装口径**：「任何对外分发 / 换设备的包，install 阶段必须包含一次 卸载 → 全新安装 → `post_install_precheck.sh`」（`stages.md:136`）。本轮为只读调研，卸载/装机由 lead 统一决定，本文件不建议越权执行。
- 预检包名解析优先级：`--pkg` > 标记文件 `pkg|` 行 > `10_docs/<游戏>/*_menu_template.json` 的 `package` 字段（`post_install_precheck.sh:27`）——**这就是模板 json 必须同步的实际原因**。

---

## 5. 菜单 dex：怎么编进去、换 id 怎么同步

### 5.1 机制

1. 菜单 Java 全部来自共享树 `mod菜单/starry-mod-menu/app/src/main/java`（`com.starry.magic.*`：StarryApp / StarryInitProvider / MenuService / ModBridge / NativeMenuController / AuthClient / ota/*…）；**按游戏裁剪的不是 Java 源码**，菜单项由 so 里的 `featureList` 决定。
2. `mxyzt_build_dex.sh` 把它编成**一个** dex：javac → jar → R8 → `templates/mxyzt_menu_current/classes4.dex`。
3. 文件名 `classes4.dex` 不是约定俗成：**官包 `classes.dex/1/2/3` 已占，取最小空闲下标**（`mxyzt_build_dex.sh:8-9`）。每个新底包都要现查。
4. `surgical_*` 把该 dex 以同名条目写进 APK，并补 Manifest（`android:name`→`com.starry.magic.StarryApp`、`StarryInitProvider` authorities、`MenuService`、两个权限）。
5. 文档侧清单 `10_docs/冒险与征途/mxyzt_menu_template.json`：`template_id=mxyzt`、`package=com.mxyzt.gzmy`、features = [1 秒杀小怪, 2 无敌, 3 全模式技能伤害增加, 4 免广告]；门禁/预检/发卡都读它。

### 5.2 换 game_id 后的同步清单（7 处，缺一处 = 菜单/鉴权错位）

| # | 位置 | 改什么 | 先例 |
|---|---|---|---|
| 1 | `Games/MxytzBiliEntry.cpp`（新建） | `.tag`、`.authGameId="mxyztbili"`、`featureList`（4 项不变） | `JjdmIntlEntry.cpp` |
| 2 | `Android_mxyztbili.mk`（新建） | 复制 `Android_mxyzt.mk`，`LOCAL_SRC_FILES` 中 `MxytzEntry.cpp`→`MxytzBiliEntry.cpp` | `Android_jjdmintl.mk:45` |
| 3 | `GameConfig.h` | **不动**（modern 路线零修改） | `JjdmIntlEntry.cpp:5`、`AGENTS.md` §7 |
| 4 | `10_docs/冒险与征途/mxyztbili_menu_template.json`（新建） | `template_id=mxyztbili`、`package=com.my.mxyzt.bilibili`、features 同名 | whrhxbili 同构 |
| 5 | `mxyzt_build_dex.sh` 派生 | `TPL_DIR` / `DEX_NAME`（按新底包空闲下标）/ 脚本头 game 行 | `whrhx_build_latest_dex.sh`（`WHRHX_DEX_NAME=classes7.dex`） |
| 6 | 新 surgical 脚本 | 包名、底包路径、dex 名、so 源目录（`libs_mxyztbili`）、输出名、`[mxyztbili]` 标签断言 | `surgical_jjdmintl_*` |
| 7 | 验证 | `LC_ALL=C grep -a 'mxyztbili' so` + `[<tag>]`；装机后 `logcat | grep 'auth game_id synced from native'` 期望 `mxyztbili` | `SKILL.md:77-83` |

> Java 侧（`app/src/main/java`）**不需要**改动：game_id 由 so 烘焙，运行时 `syncAuthGameIdFromNative()` 覆盖 prefs（`ModBridge.java:196-205`）。只换 id 时 dex 内容可沿用，但若新底包的 dex 下标变了，必须重编到新名字（重编顺带走查10 的新鲜度门禁）。

---

## 6. 待验证清单

1. [待验证] 2.0.1 底包真实业务 dex 数与**最小空闲下标**（决定菜单 dex 名，不能沿用 `classes4`）。
2. [待验证] 底包 Manifest 的 `android:name` 与父类链（决定 StarryApp 父类）、是否乐变 `LEBIAN_APPLICATION`、是否 ACE/TSS。
3. [待验证] 包内 `libc++_shared.so` 有无（决定是否注入 NDK 版）。
4. [待验证] 后台对未知 game_id 的具体错误响应体（决定「静默失效」还是「踢回登录」）。
5. [推断] `mxyztbili` 全仓无占用（本轮 grep 命中 0），后台是否已存在同名 id 需运营侧确认。

## 7. 出处索引

- `skill/starry-mod/SKILL.md:40-90,290-310`｜`skill/starry-mod/auth-chain.md`｜`skill/starry-mod/surgical-build.md:130-155`
- `mod菜单/starry-mod-menu/SWITCH_GAME.md`（双轨制 / 映射表 / 构建命令）
- `mod菜单/starry-mod-menu/app/src/main/jni/Includes/SgAuth.h`｜`MainModern.cpp:40,106-131,483-491`
- `Games/MxytzEntry.cpp`｜`Android_mxyzt.mk`｜`Games/JjdmIntlEntry.cpp`｜`Android_jjdmintl.mk`
- `04_scripts/冒险与征途/{pipeline.json,mxyzt_build_so.sh,mxyzt_build_dex.sh}`｜`mod菜单/…/scripts/surgical_mxyzt_1.5.9.sh`
- `04_scripts/_common/{stages.md,pre_install_gate.sh,post_install_precheck.sh,verify_installed_apk.sh,menu_tpl_gate.sh}`
- `10_docs/无悔华夏/B站渠道_3.8.305_无壳星光_20261106.md`｜`10_docs/无悔华夏/B站渠道_无壳星光_20261021.md`
- `10_docs/公主连结/公主连结_11.7.2_TSS-ACE校验定位与探针实验_20260814.md`｜`10_docs/公主连结/公主连结_改包初判_20260813.md`

---

## 8. §授权落地与后台注册（2026-11-16 追加·只读排查）

> 本节回答「授权必须后台在线」这条结论能不能落地：**本机能不能自己签票、没有后台时到底怎样、用户最少要点几下、有没有自测捷径**。
> 全部为源码/文件只读核对；未运行任何签票、未写数据库、未碰设备。

### 8.1 ① 本机是否具备签发票 / 部署后台的能力

| 能力 | 结论 | 证据 |
|---|---|---|
| **票签名私钥** | **有**，且与服务端同一把 | `~/.starry-keys/rsa2048.txt`（2336 B, 600）；`python3 04_scripts/_common/sg_pki/gen_pubkey_header.py --check` → **OK 公钥一致 fp=25ba10e0faca3500**，与 so 内 `Includes/sg_pubkey.h:38 kSgPubFp` 同值 |
| **签票脚本/库** | **有** | `04_scripts/_common/sg_pki/sg_rsa_pkcs1.py`（`load_key/sign/verify`）；`selftest_p1.py` T7 就在本机离线签过 v2 票（`P.sign(body, key)`，sig 512 hex）；服务端同算法 `auth-server/app/services/grant_service.py:95 mint_grant_v2` |
| **后端源码** | **全在仓里** | `mod菜单/starry-mod-menu/auth-server/`（FastAPI + `starlight.db` + `certs/` + `docs/API.md` + `docs/ADMIN_UI_SPEC.md`）；可 `uv venv + uvicorn` 本机起（README「本机无 Docker 调试」） |
| **改服务端源码的补丁脚本** | 有，但**不是**注册游戏的工具 | `sg_pki/apply_p1_{server,client,gate,integrity,delegate,pipeline}.py`；`apply_p1_server.py` 的 `ROOT` 硬编码 `E:\\nixiang`（Windows 侧），用途是给 `auth_service.py` 加 `grant2` 签发点，**不创建 games 行** |
| **游戏注册（games 行）** | **只能走后台/管理 API**，本机无现成脚本 | `auth-server/app/routers/admin.py:620 create_game`（`POST /v1/admin/games`，需 admin token）；`Game` 模型见 `app/models.py:13-40` |
| **本地 mock 授权服务** | **无现成脚本**；可用 auth-server 本机跑，但客户端 base url 内置且证书固定（`starlight.crt` CN=47.100.216.101、`CertPinned.h`），改指向需改包 prefs | `~/.starry-keys/api_base.txt` = `https://47.100.216.101:8443` |
| **服务器访问通道** | 有先例脚本（SSH 取钥匙），本轮未执行 | `04_scripts/双机协作/fetch_starry_keys.sh`（`root@47.100.216.101:/www/wwwroot/starlight-auth/auth-server`） |

> 一句话：**签票能力在本机（私钥 + 算法都在），"让票被服务端承认"这件事不在本机**——差的是 `games` 表里那一行，只有管理后台 / admin API 能给。

### 8.2 ② 无后台时功能到底会怎样

`SgAuth::apply()` 只做本地验签（公钥在 so 里，`SgAuth.h:222-231`），**从不联网**；联网只发生在 Java 侧拿票。

| 场景 | 结果 | 证据 |
|---|---|---|
| 后台**没有** `mxyztbili` 这一行 | `/v1/bootstrap?game_id=mxyztbili` → `build_bootstrap` 取不到 Game ⇒ `menu_enabled=True, card_auth_enabled=True`（None 兜底）⇒ 不发 open 票；登录必然失败（无卡密可绑） | `auth_service.py:809-810,861-899` |
| 票拿不到 / 票里 game 串不符 | `apply()` = 0 ⇒ `ticket_ok=0` ⇒ `live()` false ⇒ `feat_ok(i)` 全 false | `SgAuth.h:259,176-198` |
| 表现 | **四项功能静默失效**：不闪退、菜单照常弹、开关能勾但没有效果 | `SgAuth.h` 头注释④；`ModBridge.java:296-303`（`blocked feat #n — no live session`） |
| 会不会被踢回登录 | 只有**冷启动 + 服务端权威错误关键字**（会话/登录/封禁/过期/设备/不存在/权限/用尽）才 `hardKickAuth`；纯网络失败、免卡无票、续票宽松失败**都不踢**，只刷 UI | `NativeMenuController.java:3239-3253,3369-3374,3408-3419` |
| 后台有行但 `enabled=false` | `compute_feat_mask` 返回 0 ⇒ `apply()` 拒（掩码 0 = 停用） | `grant_service.py:74-75`；`SgAuth.h:271` |
| 后台有行、功能位未录 | **仍全开**：`compute_feat_mask` 默认 `FEAT_MASK_ALL`，只清"显式 disabled"的位 | `grant_service.py:20,77-96` |

⇒ **掩码不需要手算**：新建游戏不建 `feature_flags` 记录时票里 mask = `0xFFFFFFFFFFFFFFFF`，featNum 1–4 全为 1；只有后台主动停用某项才逐位清 0。

### 8.3 ③ 用户最小操作清单（5 条，其中可省 2 条）

前置：需要**后台管理账号**（`/admin/` 或 `POST /v1/admin/login` 拿 token 后 curl）。我无该凭据，本轮未执行。

| # | 在哪 | 填什么 | 必做 |
|---|---|---|---|
| 1 | 管理后台 → **游戏列表 → 新增** | `game_id = mxyztbili`；`name = 冒险与征途(B站)`；`channel = B站`（下拉有此项，见 `ADMIN_UI_SPEC.md`「渠道字段」）；`enabled = 开` | ✅ |
| 2 | 同一弹窗 | `card_auth_enabled`：**true = 需卡密**（正式交付）／**false = 免卡**（内部自测，连网 bootstrap 即发全掩码票）；`menu_template` 留空或填 `mxyztbili`；`payload_sha` **留空**（本作 native 型不签 v3）；`sync_features` 勾不勾都行（不勾 = 默认全开） | ✅ |
| 3 | 管理后台 → **卡密管理 → 新增** | 绑单游戏 `mxyztbili`（或"全游戏"卡）；时长按需 | 仅需卡时 |
| 4 | 设备上（用户操作） | 菜单 → 账号 Tab → 输入卡密登录 | 仅需卡时 |
| 5 | 验证（我来看日志） | `logcat -s StarryApp:V Mod_Menu:V` 抓 `auth game_id synced from native: mxyztbili` 与 `[V2] s=1 m=<mask>`（全开 = 18446744073709551615） | ✅ |

> 走**免卡（card_auth_enabled=false）**：清单缩到 2 条（第 1、2 项），登录步骤全省；代价是客户端不显示账号 Tab、无卡密计费/停权手段。

### 8.4 ④ 临时自测通道（有，三条；按合规度排序）

**T1（推荐·零改包·只需后台一行）免卡模式**
`card_auth_enabled=false` ⇒ `bootstrap` 直接发 v2 掩码票（`auth_service.py:861-899`），与正式链路同一条代码路径。风险低；**不可当交付证据**（无卡密链）。

**T2（无后台·需 root 写 prefs）自签票注入**
本机私钥能签服务端承认的票；写进客户端 prefs 后，冷启动 `syncNativeAuthorized()` 用 `Preferences.getAuthGrant/getOrCreateDeviceId/getAuthGameId` 三件套调 `nativeApplyGrant`（`ModBridge.java:308-313`），native `live()` 即通过：
```bash
# 仅示意（本轮未执行）：签 24h、bit1..bit4 全开的 v2 票
python3 - <<'PY'
import os,sys,time
sys.path.insert(0,'04_scripts/_common/sg_pki'); import sg_rsa_pkcs1 as P
key=P.load_key(os.path.expanduser('~/.starry-keys/rsa2048.txt'))
dev='dev_xxxxxxxx'            # 设备 prefs 的 auth_device_id（需 root 读）
body='|'.join(['2',dev,'mxyztbili',str(int(time.time())+86400),str((1<<1)|(1<<2)|(1<<3)|(1<<4)),'dev'])
print(body+'.'+P.sign(body.encode(),key).hex())
PY
# 再把该串写入 /data/data/com.my.mxyzt.bilibili/shared_prefs/starry_magic_prefs.xml 的 auth_grant
```
风险：① **绕过整条授权链，只能自测，绝不可作功能验收/交付证据**；② 需 root 写 `/data/data`（设备写操作，须逐次确认）；③ 票有 `exp`（默认 86400s），过期需重签重写；④ 后台无该游戏时冷启动续票**不踢但也不续**，票到期即静默失效；⑤ 会干扰排障（`[V2] m=` 不再代表后台真实配置）。

**T3（无后台·隔离环境）本机跑 auth-server**
`uv venv + uvicorn` 起服务、sqlite 自建 `mxyztbili`。缺点：客户端 base url 内置 `https://47.100.216.101:8443` 且 `CertPinned.h` 固定证书 ⇒ 必须改包改指向，等于引入"非交付形态"的包。**不建议**用于本作。

### 8.5 关于「共用 game_id / 预注册脚本」

- **共用 game_id 配不同 mask**：技术能跑（`apply()` 只比对票面 `game` 串 + 掩码位），但掩码粒度是 `(game_id, feat_num)`，**没有渠道维度**（`compute_feat_mask` 只按 game_id 查 `FeatureFlag`）⇒ 做不到「TapTap 开 1-3 / B站 开 1-4」；且共用 id 有 SKILL.md:304 的模板错位事故先例 + 卡密/停权互相串。**结论：技术可行、规则禁止，不采用。**
- **出包前本地预注册**：无先例脚本。等价替代是直接调 `POST /v1/admin/games`（body = `GameCreate`）一次 curl；菜单模板可用 `auth-server/scripts/export_menu_from_cpp.py` 从 `featureList` 导出 JSON，再在「菜单模板」页按 `编号|名称` 录入（脚本注释明确：写数据库，无需改 `menu_catalog.py`）。

### 8.6 本节新增待验证

1. [待验证] 后台是否已有 `mxyztbili` 同名行（`create_game` 对重复 id 直接 400「游戏 ID 已存在」）。
2. [待验证] 生产 `starlight.db` 的 `games` 实际字段值（`card_auth_enabled` 惯例）——仓库里那份只是快照，不作现网事实。
3. [待验证] 设备 prefs 里 `auth_device_id` 实际值（T2 通道前置输入，本轮未读设备）。
