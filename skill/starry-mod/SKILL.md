# 星光菜单落地技能

> 📍 `skill/starry-mod/SKILL.md`
> 🎯 把逆向成果落地到星光菜单框架
> 📖 详细流程见 `WORKFLOW.md` 和 `mod菜单/starry-mod-menu/SWITCH_GAME.md`

---

## 触发场景

- "落地星光菜单"
- "切到 XXX 游戏"
- "打包/装机测试"

---

## 切游戏（双轨制，2026-09-09 起）

**新游戏一律走 modern 注册表（共享文件零接触）**——生成器一条命令产出四件套（Mod + Entry + 私有 mk）：

```bash
bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
# 填 {Game}Entry.cpp：authGameId（game_id）/ waitLib / featureList / 初始化参数
# 构建：APP_BUILD_SCRIPT=./Android_{tag}.mk NDK_OUT=<starry根>/obj_{tag} NDK_LIBS_OUT=<starry根>/libs_{tag}
#   （必须绝对路径：相对路径被 ndk-build -C 解析到 app/ 下 → 校验到旧 so 假成功，2026-09-10 事故）
```

入口 = `MainModern.cpp`（游戏差异全在 Entry 的 `StarryGameMod` 注册表）。**禁止改 `Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk`**（`Main.cpp` 连注释都禁改：`OBFUSCATE` 用 `__LINE__` 做混淆种子，行号偏移即产物漂移，20260909 实测）。

**存量游戏走 legacy**：公共 `Android.mk` 一个 `STARRY_GAME_*`（+ `ENABLE_*_HOOK` + 对应 `Games/*Mod.cpp`），`STARRY_AUTH_GAME_ID` 由 `GameConfig.h` 自动映射；重新出包时顺手迁移注册表（写 Entry + 切私有 mk；**Main.cpp 里的旧 #if 分支不删**——Main.cpp 连注释都禁改，删分支会漂移所有 legacy 产物字节，分支组保留到全部游戏迁移完成后由强 AI 统一一次性清理）。**禁止**：在 mk 里写 `-DSTARRY_AUTH_GAME_ID`、在 Java 写死默认游戏名、删 `#error` 保护。

modern 的 `Games/*Mod.cpp` 同样从生成器产出（防闪退骨架 v3，内置懒装/双路径挂点/SafeWrite，勿从旧游戏复制）。详见 `mod菜单/starry-mod-menu/SWITCH_GAME.md` 顶部。

### 硬规则：jniLibs 才是 Gradle 打包的 so（2026-08-17 血泪）

`app/build.gradle` 已 **关闭 `externalNativeBuild`**。`./gradlew assemble*` **不会**按 `Android.mk` 重编 native，只打包：

`app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`

| 步骤 | 必须做 |
|------|--------|
| 1 | 改 `Android.mk` 切到目标 `STARRY_GAME_*` |
| 2 | `ndk-build`（产出常在 `app/src/libs/arm64-v8a/`） |
| 3 | **拷贝覆盖** → `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`（建议同步 `app/src/main/libs/`、`app/libs/`） |
| 4 | 再 `gradlew` / 跑 `surgical_*.sh` / `*_so_only.sh` |
| 5 | **装机前自检**：成品 APK（或 jniLibs）里的 so，须含本游戏标签（如 `[Gjc2]` / `gjc2`），**禁止**仍出现上一款（如 `[Dwct]`）。中文菜单串用 `LC_ALL=C grep -ac '菜单名' <so>` 按字节搜——`strings` 默认 ASCII 不认 UTF-8 中文，会漏报（2026-09-10 坑） |

**漏做第 3 步 = 菜单/鉴权仍是上一款游戏**（弓箭传说2 打出动物餐厅菜单即此坑；切游戏后 gradle 十几秒「成功」往往是旧 so）。

`*_so_only.sh` 做 `ndk-build` + `zip -u` 时，应以成品的独立工作副本为目标，不默认覆盖用户最终包；仍应同步刷新 jniLibs，避免下次外科/gradle 再打错。

**与热更新无关：** 本规则只约束**出包时嵌入 APK 的 so**。OTA 另走下载落盘，运行时优先加载 OTA so；改 jniLibs/文档**不改**热更下载/验签/优先加载逻辑。但上传的 OTA so 本身也必须是对应 `game_id` 编出来的。

### game_id 命名规则（新游戏，2026-08-27 起）

**`game_id` = 游戏官方中文全名的汉语拼音首字母**（小写、无分隔符、无连字符）。

| 规则 | 说明 |
|------|------|
| 用全名 | 以商店/aapt 标签为准，不擅自缩短 |
| 虚词计入 | 「的、之、与」等也取首字母（与包名片段一致） |
| 数字保留 | 名中的阿拉伯数字原样写入（如 `33` → `33`） |
| 唯一同步 | `GameConfig.h` `STARRY_AUTH_GAME_ID` = 后台 `games.game_id` = 菜单 `template_id` = 文档 JSON |

示例：伯吉的温馨厨房 → `bjdwxcf`（错：`bjdw`）；生存33天 → `sc33t`（错：`sc33`）；我嘎嘎乱杀 → `wggls`。
取 id 前在 `10_docs/{游戏}/` 写一行「全名 → game_id 推导」，避免漏字。

**存量 grandfather**：已上线/已发卡的 id 一律不改（含 `bjdw`、`zhmr`、`neon`、`pcr`、`wggls` 等），新规则只约束此后新增。
**宏与 id 分离**：`STARRY_GAME_XXX` 编译宏/文件名可用短缩写；`STARRY_AUTH_GAME_ID` 鉴权字符串必须遵守全名首字母；`GameConfig.h` 映射指向正确全名 id。

### game_id 映射表

| STARRY_GAME_* | game_id | 游戏 |
|---------------|---------|------|
| SELF | starry_self | 自测试 |
| ZXR | zxr | 镇邪人 |
| ZMXY | zmxy | 造梦西游 |
| ZMWS | zmws | 造梦无双 |
| LTZJ | ltzj | 雷霆战机 |
| NEON | neon | 霓虹深渊 |
| CSTJJS | cstjjs | 长生天机降世 |
| ZJMC | zjmc | 追迹迷城 |
| HZSJ | hzsj | 魂坠深境 |
| MILTHM | milthm | milthm |
| YWGN | ywgn | 音舞光年 |
| WZYM | wzym | 物质与魔法 |
| MATCH2 | xxh2 | 消消英雄2 |
| PCR | pcr | 公主连结 |
| GYWS | gyws | 孤勇无双 |
| ZHMR | zhmr | 战魂铭人 |
| DWCT | dwct | 动物餐厅 |
| GJC2 | gjc2 | 弓箭传说2 |
| GJC2INTL | gjc2intl | 弓箭传说2国际版 |
| TLGD | tlgd | 弹力果冻 |
| SHLT | shlt | 山河旅探 |
| DZQS | dzqs | 独奏骑士 |
| SHHJ | shhj | 山海幻境 |
| SC33 | sc33 | 生存33天 |
| HALY | haly | 黑暗领域 |
| ZMXS | zmxs | 掌门下山 |
| WGGLS | wggls | 我嘎嘎乱杀 |
| WHRHX | whhx | 无悔华夏 |
| WHRHXKB | whhxkb | 无悔华夏快爆 |
| WGDZZ | wgdzz | 王国大作战前线 |
| QLFH | qlfh | 青鸾繁华录 |
| BJDW | bjdw | 伯吉的温馨厨房 |
| ENDFIELD | mrfzzmd | 明日方舟：终末地 |
| WSCQ | sycs | 圣翼传说 |

以 `GameConfig.h` / `SWITCH_GAME.md` 为准；上表落后时以头文件为准并回写本表（2026-09-03 已按 GameConfig.h 回写至 33 款；2026-09-24 增补圣翼传说 = `sycs`，modern 注册表 `Games/WscqEntry.cpp`）。

### 验证

```bash
# 装机前（强制）
strings path/to/libstarry_magic.so | grep -E '\[Gjc2\]|gjc2|\[Dwct\]|dwct'   # 换成当前游戏标签

# 装机后
adb logcat -s StarryApp:V StarryMagic:V | grep "auth game_id synced from native"
# 期望：auth game_id synced from native: <本游戏 id>
```

---

## 构建路径

| 改了什么 | 用什么 |
|----------|--------|
| 只改 `Games/*Mod.cpp` | `*_so_only.sh`（内含 ndk-build；须带 jniLibs 同步 + so 标签自检） |
| 首包/改菜单/smali/Manifest | `inject_*.sh` 或 `surgical_*.sh` |
| TSS/ACE 游戏 | **必须外科组装** |
| 快速试改 smali | MT MCP |

### 存量游戏快速重打（2026-09-29）

已有可构建工程和既有 native so 时，按改动范围选最短路径，不走全套标准阶段：

| 改动 | 动作 |
|------|------|
| Java / dex / Manifest / 菜单资源 | 复用现有 `libs_sjzz/.../libstarry_magic.so` 或对应游戏 so；直接跑该游戏 surgical 脚本完成构建、签名、验证、可选安装 |
| C++ / Hook / 菜单 native 串 | 先跑该游戏 `*_build_so.sh`，确认 so 指纹和菜单串，再跑同一条 surgical 链 |
| 只有现有正式包，要重装 | `STARRY_INSTALL_ONLY=1` 复用包，只跑验证+安装；禁止重新构建后再声称“只是重装” |

```bash
bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh                 # 构建+签名+验证
STARRY_INSTALL=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh
STARRY_INSTALL_ONLY=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh
```

验证脚本必须从成品包重新解出目标 `classesN.dex` 和 so；不要把 `/tmp/sjzz_*` 这类上一轮残留当成新构建证据。完整 APK 重新签名后 md5 必变，混包判断只看 dex md5、so md5、Manifest/包结构和签名身份。

★ **dex-only「菜单源码换新」路径（2026-10-01 独奏骑士实测，脚本 `dzqs_menu_latest_1.1.768.sh`）**：机身包正常、只想把旧菜单 dex 换成 HEAD 版时，**不必重打 so**，但必须先证明 native 面没动，再外科置换唯一 `classesN.dex`：
1. 先证「只换 dex 安全」——`git diff <底包提交> HEAD -- app/src/main/java` 看改动文件，**且**新旧 dex 反编译后 `bridge/ModBridge`、`menu/NativeMenuController` 这类 native 面/登录体类必须逐指令相同（`baksmali` 两侧 `diff -rq`，只有业务 hook 类不同即可）。
2. 脚本六段：底包校验（zip 结构 + `classes2.dex` 含 `StarryApp`）→ HEAD `gradlew assembleRelease --offline` 取 dex → `apktool d` + `StarryApp.smali` 宿主改写（四处：`.super`/`<init>`/`attachBaseContext`/`onCreate` → `Lcom/sgsdk/core/SGGlobalApplication;`）→ `smali assemble` 出新 dex + 断言 → zip 外科置换只换 `classes2.dex`（逐条目 sha256 审计 + so 与底包逐字节相同断言）→ `zipalign`/`apksigner` + 证书钉 + 门禁 + 可选装机。
3. 两个必踩的坑：① **证书钉要归一化**——`apksigner` 输出小写无冒号，`keytool` 指纹大写带冒号，直接比会假失败（`tr -d ':' | tr 'A-Z' 'a-z'` 后再比）。② **判 dex 里的中文串一律用 `python3.11` 字节断言，不要 `grep -aq '中文'`**（本机 locale 下会假失败）；并且别把脚本接 `| tail`（pipeline 退出码取 tail，脚本 die 了也显示 0，会误判成功）。
4. 反例记录：旧 dex 只有 v5 SSAID 单路径、SSAID 不可用即抛错 ⇒ 菜单整块不可用；换新后 `DeviceFingerprint.getOrCreateDeviceId()` 变 v5 优先 + v4 硬件指纹兜底。凡是「取号/授权类」单点路径，都要检查有没有兜底分支。

**已注册管线的一律走 `04_scripts/pipeline.sh`**（status/next/reset/verify），2026-09-10 起自动防旧产物直装：上游产物变化 → 过期下游阶段自动回退 pending；过期链路上 `run` 下游会被陈旧度门禁拒绝。构建后端四件套（绝对路径/obj stamp/链接活动保险/菜单串字节校验）范本 = `04_scripts/魂坠深境/hzsj_build_so.sh`；install 阶段末尾接 `04_scripts/_common/verify_installed_apk.sh` 校验设备包 md5 == 本地 APK。

★ **外科组装 dex 落位铁律（2026-09-24 圣翼传说实测踩坑）**：新增 smali 编出的 dex 名必须是 base.apk 里**最小的空闲下标**（跑脚本求 `classesN.dex` 未被占用的最小 N，单 dex 官包 → `classes2.dex`）。写死 `classes8.dex` 时 ART 的 `BaseDexClassLoader` 从 `classes.dex` 顺序扫、遇到第一个缺口即停止 → 星光 dex 永不加载 → `StarryInitProvider` `ClassNotFoundException` **启动即崩**。SDK 侧另有两条：① 官包无 `libtprt.so`/ACE 时 `Android_<tag>.mk` 必须含 `Games/NoAceStubs.cpp`（否则 `ld.lld: undefined symbol: SignalQuarantine_Start`）；② hook 日志 tag 是 `Mod_Menu`（`Includes/Logger.h` 的 `TAG=OBFUSCATE("Mod_Menu")`），不是 `StarryMagic`。
   - 可用 `python3 04_scripts/_common/next_free_dex.py <官包.apk>` 求最小空闲 N，再写 `classesN.dex`。

### 构建环境

```bash
export JAVA_HOME=$(/usr/libexec/java_home -v 17)  # Gradle 需 JDK 17
cd mod菜单/starry-mod-menu

# ❌ 不要假定 gradle 会编 so / 不要写 externalNativeBuildDebug（已关闭）
# ✅ 切游戏或改 Mod 后：
ndk-build -C app/src/main/jni NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk
cp -f app/src/libs/arm64-v8a/libstarry_magic.so app/src/main/jniLibs/arm64-v8a/
./gradlew assembleRelease   # 仅打 Java/资源 + 打包已放好的 jniLibs so
```

> **沙箱/权限（2026-09-20 实测）**：`./gradlew` 要写家目录缓存 `~/.gradle`。只允许写工作区的
> 沙箱（如 DSH file policy=`workspace-write`）下会直接失败：
> `java.io.FileNotFoundException: …/.gradle/wrapper/dists/gradle-8.13-bin/…zip.lck (Operation not permitted)`。
> 结论：**构建命令一开始就带一次性放宽权限运行**，不要先撞一次再重跑（重跑还要重编 so，白烧一轮）。

优先用各游戏的 `scripts/surgical_*.sh` / `*_so_only.sh`（脚本内应已含拷贝与自检）。

---

## 环境与登录诊断入口

旧版“过检通用清单”将项目处理写成通用方法，现改为证据与授权检查；不从组件名称直接决定修改方案。

| 项 | 检查范围 |
|----|----------|
| 签名与配置 | 核对实际签名、包名及 SDK/后台支持的配置；历史签名处理不代表官方支持或通用授权路径 |
| TapTap 登录 | 按 [登录诊断与授权边界](../re-analyze/login/taptap-web-login.md) 核对版本、真实 handler 路由与完整回调，不重复旧固定修改方式 |
| 完整性错误 | 保存原始错误和样本身份，区分损坏、配置不匹配与主动拒绝，先使用受支持的测试构建 |
| 设备环境 | 记录实际环境及支持条件；仅在已授权测试范围内诊断，不默认隐藏或绕过检测 |

---

## 装机前置检查（G1 必查，2026-09-17 起，不再等用户追问）

> 来源：18 份会话记录审计——「防封审计做了吗」「异常兜底做了吗」在 3 个会话被用户重复追问才补做；凡尘西游 TapTap 游戏装机即闪退（SignSpoof 未预嵌证书+登录白名单缺失）；山海幻境装机后被服务端签名校验拦截（ret=5）返工。重复追问 = 流程缺口。

装机（首装或覆盖）前逐项过，缺一不装机：

1. **产物机器关**：`bash 04_scripts/_common/pre_install_gate.sh`（JS 语法/JNI 配套/P/Invoke 符号/控件读取四查，见 stages.md 专节）
   - 控件读取查优先传 `--runtime-menu-json`；后台菜单模板按规范不能带 `type`，不能单独用它判断 Slider/Toggle。
2. **防封/上报审计**：按 `../re-implement/anti-ban-analysis.md` 输出标准结论（上报链清单 + 量级线 + 服务端权威判定），无审计结论不装机
3. **异常兜底审计**：战斗代码 + 菜单 Java 侧（`ModBridge.safeCall` 包裹、native 缺失/偏好类型不符的降级路径）都要有 try/catch 兜底结论；HybridCLR 游戏热更侧调 native 一律文件镜像通道（`starry_state.txt` 模式），桥接调用必须 try/catch
4. **TapTap 渠道游戏三查**（凡尘西游/山海幻境教训）：
   - 签名校验存在性：静态扫 `CheckSignature`/`getSign`/`getMD5`/`packageSign` 类入口，有则逐一给对策
   - `SignSpoof` 证书回灌：官方证书 SHA 已嵌入（含 SDK 商签名映射，如 Openew/TapTap）
   - `TapLoginForceWeb` 白名单含本包名；旧版 TapSDK（如 3.30.2）注意 `TapTapActivity` 客户端 SSO 分支需单独处理（一念通天教训）
5. **game_id 独立性**：新渠道/新包禁止复用其他渠道 game_id（无悔华夏 4399 事故：临时复用 `whhx` 致后台模板错位、返工重建 so）；按命名规则独立申请并四点同步（GameConfig.h/后台/菜单模板/文档）

**收尾交付物（2026-10-05 起 icon / 菜单模板为按需产出：新游戏首次交付或用户点名时才生成）**：
- icon 提取（按需）：交付副本放 `08_output/{游戏}/delivery_YYYYMMDD/icon_{游戏}_{512|144}.png`；`02_apks/` 只作为用户管理的不可再生输入，AI 不默认写入。
- 菜单模板 JSON：`10_docs/{游戏}/{游戏拼音}_menu_template.json`（`template_id`=game_id）
- 交付副本（按需）：`08_output/{游戏}/delivery_YYYYMMDD/menu_template.json`，字段严格按 `10_docs/_templates/菜单模板_规范与示例.md` 自检。
- 交付目录生成（**★收尾一律走这条，禁手工重造，2026-10-02 消消英雄2 定**）：先读 `04_scripts/_common/stages.md` 收尾节，再跑
  `bash 04_scripts/_common/finalize_game_delivery.sh --game {游戏} --apk {正式包} --prune --sync-doc --apply`
  —— 裁剪唯一正式包 + **打印**文档同步片段（共享文件仍需手动粘，脚本不自动改）。
  **icon / 菜单模板按需**：仅「新游戏首次交付」（`08_output/{游戏}` 无 `delivery_*`）或用户当轮点名时才追加 `--icon-source {原图} --menu-template 10_docs/{游戏}/{id}_menu_template.json`，生成 delivery 目录（icon 512/144 + 模板副本 + 字段自检）；其余收尾不传，脚本默认跳过、不建 delivery 目录。
  （历史浪费：2026-10-02 手工 PIL 放大 icon、手调 `prune_output_final.sh`、手写游戏摘要与 `current.md` 行 —— 三件都是它的本职。）
- icon 无 512 源时：官方资源常只到 192×192，必须放大并在交付记录里注明「由官方最高密度 XX 放大」，不谎称原生 512。
- 收尾清单：按 `10_docs/_templates/游戏收尾与最小复现清单.md` 填写，含生命周期场景矩阵已测/未测

---

## 分发与装机红线（强制，适用于所有游戏）

> 来源：2026-08-28 无悔华夏分发事故（`10_docs/无悔华夏/闪退修复_20260827.md`）。规则对所有游戏生效，不限于该游戏。

### 分发自检：文档说修了 ≠ 用户拿到的包修了

「源码已修 + 本机验证」不等于用户手里的包已修。**任何游戏**对外分发前逐条执行：

1. 正式包必须**本次重打**为 `08_output/{游戏}/` 下新文件，不默认覆盖或整理既有最终包（见 AGENTS.md）；trace 包、旧包、只换 so 未刷 dex 的包一律不得当正式分发
2. 成品 APK 内 so 必须是本游戏 + 本版本：`unzip -p 成品.apk lib/arm64-v8a/libstarry_magic.so | strings | grep '<本游戏标签>'`
   - 同管道补证书自检（2026-09-06 起烘焙在 so）：`... | grep -a -c 'BEGIN CERTIFICATE'` 应 ≥1；新游戏包 `assets/certs` 可省，详见 [surgical-build.md](surgical-build.md)「证书固定来源」
3. 改过 Java（MenuService / StarryApp 等）时，baksmali 成品 dex 验改动真的打进了包，不信「源码已改」

### MenuService 前台服务铁律（框架级，全游戏生效）

Android 8+ `startForegroundService()` 后 **5 秒**内不进前台，系统直接杀进程。Application 越重的游戏（加固/热更/统计 SDK 越多）主线程越容易被占满而超时：

- `MenuService.onCreate()` **第一行业务**就是 `startForegroundCompat()`；Preferences/native 同步调用包 try/catch 放在后面
- `onStartCommand()` 再调一次 `startForegroundCompat()` 兜底
- `StarryApp` 里 `MenuService.start()` 用 `Handler.post()`，不与 `super.onCreate()` 抢主线程

### 清数据 = Mod + 热更资源双杀（全游戏默认）

分发说明必须写「**禁止清数据，只覆盖安装**」。清数据的两类普适后果：

- **Mod 失效**：运行时解密/热更后的 libil2cpp 与静态 RVA 不一致（导出剥离的游戏还需 xdl_sym/dsym + RX 段序言扫描兜底），清数据改变映射环境 → 偏移全飘
- **热更回归**：官方热更重新全量下载，打进热更目录的补丁（jsc 门控、patch 过的资源）全部丢失
- 游戏内贴图变紫/洋红 = 热更 DLC 资源校验失败（与 Mod hook 无关）：先保留唯一热更输入与故障证据；重新下载/删除须按 TOOLBOOK「清理规则」「真机残留清理」核对可取得性、范围及授权，**不默认清数据**

---

## 设备指纹

```bash
# 仅在标识恢复的具体范围已授权且恢复前置满足时使用；见 TOOLBOOK「真机残留清理」
bash 04_scripts/_common/ensure_starry_device_id_seed.sh
```

改指纹相关代码**必须重打含菜单 DEX 的包**，只换 so 无效。

详见 `auth-chain.md`
- **框架改版总结（2026-09-05/06）**：心跳→冷启动对账、票 TTL=86400、CrashHandler、OBFUSCATE+RegisterNatives、解绑新规（首次免费/后续扣3天）等全部机制、坑与验证方式见 [10_docs/_templates/经验_星光框架大改版_20260906.md](../../10_docs/_templates/经验_星光框架大改版_20260906.md)；自测包构建用私有 `Android_self_test.mk`（jniLibs 与山海幻境会话竞争）。

---

## 验证

```bash
adb logcat -s StarryApp:V StarryMagic:V AuthClient DeviceFingerprint | grep -E \
  'auth game_id|native menu shown|source=shared'
```

逐项检查：菜单弹出且**功能名对本游戏**、每个功能有效、关闭后恢复、登录正常、对局结束不闪退。

---

## 菜单索引与授权验证边界（合并 2026-09-05）

- **适用范围**：当前工作区星光框架的 `g_featureList` 数组索引协议；历史案例为无限升级 11.266018，不保证其他框架或后续版本沿用。
- **历史证据**：[无限升级记录](../../10_docs/无限升级/运行时dump与YAES破解_20260905.md) §九–十一；本轮仅文档整理，没有运行复测。“已合并”不等于所有命题 PROVEN。

### featNum 与 Category

- 当前框架的 `featNum` 沿 `g_featureList` 数组下标（从 0 起），不是“第几个可点击控件”。`Category` 自身无功能回调，但仍占数组下标；后续功能编号不能跳过它重计。
- 对齐检查须覆盖数组、前端 `data-featnum`、native 分发/枚举、后台模板 `feat_num` 与配置键；以正常 UI 逐项开关及关闭恢复验证实际对应关系。
- **历史事实**：记录 §十称初版漏计 Category，导致后续开关错位；此前 adb 直写配置的验收掩盖了 UI 映射问题。配置消费成功不证明菜单接线正确。
- **待核对实现风险**：[Main.cpp](../../mod菜单/starry-mod-menu/app/src/main/jni/Main.cpp) 第 465 行旧注释称 Category“不占 featNum”，与同文件第 473 行“数组下标”及历史案例相反。本轮只确认存在该注释冲突、不改代码；后续核对实际渲染/分发实现，不能按旧注释改编号。完整 UI 运行链本轮 **NOT PROVEN**。

### 心跳、偏好重推与测试覆盖

> **2026-09-05 机制变更**：60s 周期心跳与每次全量重推已删除，改为冷启动一次对账（权威硬拒才踢、会话复活才重推、票 TTL=86400s）。本节下述“心跳”描述为历史记录，现行机制以 [auth-chain.md](auth-chain.md)「冷启动对账机制」为准。

- 记录 §九中的“60s 级”与心跳及偏好重推相关，属于有条件的历史现象，不是“任何未授权状态每 60s 必然全 OFF”的定律。
- 配置被覆盖须区分 bootstrap、心跳、登录成功等触发源，并核对服务是否运行、当前偏好、授权/会话状态和 native 门控；具体触发条件、值来源与时序需对应构建日志证明，未覆盖部分 **NOT PROVEN**。
- adb 配置旁路只证明该次输入经过的配置消费与局部行为；不验证 UI 映射、正常授权、关闭恢复或重启恢复链。
- 不收录停服务来绕开授权的做法。测试隔离仅用于明确授权的局部诊断，不代替正常端到端验证；正式验证须经正常授权与菜单入口，覆盖授权状态变化及恢复行为。
- 正常链按 [auth-chain.md](auth-chain.md) 核对，但不能仅因服务运行或菜单可见就宣告授权成功；需要对应 game_id、实际会话与目标功能的证据。
- **历史报告限制**：§十一虽记载最后五功能通过，仍将后台注册与发卡移交用户。因此不能据此证明正式授权全链路已通过，也不能把测试隔离结果提升为正式交付证明。

## 收尾

按 [TOOLBOOK.md](../../TOOLBOOK.md)「清理规则」「真机残留清理」收尾，填写 [游戏收尾与最小复现清单](../../10_docs/_templates/游戏收尾与最小复现清单.md)。保留输入、隔离重建、具体授权和设备分责以该处为准；现有清理脚本未改，不默认运行，缺证据或无法约束范围时记录待处理。

收尾入口统一为：

1. 清理后先跑 `04_scripts/_common/pipeline_record_cleanup.py` 修管线快照。
2. Git 固化用 `04_scripts/_common/archive_scoped_git.sh` 白名单提交；脚本默认提交后推送 `origin/main`，只有明确不想推送时才加 `--no-push`。
   - 若白名单文件混有其它会话改动（常见于 `10_docs/_state/current.md` 和共享 Java 白名单），先做 hunk 级暂存，再跑 `--staged-only`。
   - 建议加 `--require-clean-paths`：白名单路径仍有未暂存改动时直接停止，避免路径级 `git add -A` 误带无关内容。
3. pipeline 写回 `archive=done` 后，用
   `04_scripts/_common/archive_close_git.sh --game {游戏}` 补状态提交。
4. 游戏详情更新到 `10_docs/_state/games/{游戏}.md`，`current.md` 只保留一行；
   功能反复反馈和长证据分别补功能验收卡、证据 README。
5. 收尾后用 `python3 04_scripts/_common/check_state_hash_sync.py --game {游戏} --manifest {裁剪清单}.json` 核对摘要、索引行与正式包 hash 一致。
