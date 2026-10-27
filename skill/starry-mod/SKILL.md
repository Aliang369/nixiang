# 星光菜单落地技能

> 📍 `skill/starry-mod/SKILL.md`
> 🎯 把逆向成果落地到星光菜单框架
> 📖 详细流程见 `WORKFLOW.md` 和 `mod菜单/starry-mod-menu/SWITCH_GAME.md`
> **边界（2026-10-15 用户 m00007 定）**：本文件只写**通用机制**（跨游戏成立的操作/判据/坑）。
> 任何**单游戏**实例与血泪（脚本段结构、实测数值、事故经过）在 `10_docs/{游戏}/技能先例.md`（或该游戏既有文档），本文件只留一行「先例：见 …」。

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
#   （必须绝对路径：相对路径被 ndk-build -C 解析到 app/ 下 → 校验到旧 so 假成功）
```

入口 = `MainModern.cpp`（游戏差异全在 Entry 的 `StarryGameMod` 注册表）。**禁止改 `Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk`**（`Main.cpp` 连注释都禁改：`OBFUSCATE` 用 `__LINE__` 做混淆种子，行号偏移即产物漂移）。

**存量游戏走 legacy**：公共 `Android.mk` 一个 `STARRY_GAME_*`（+ `ENABLE_*_HOOK` + 对应 `Games/*Mod.cpp`），`STARRY_AUTH_GAME_ID` 由 `GameConfig.h` 自动映射；重新出包时顺手迁移注册表（写 Entry + 切私有 mk；**Main.cpp 里的旧 #if 分支不删**——Main.cpp 连注释都禁改，删分支会漂移所有 legacy 产物字节，分支组保留到全部游戏迁移完成后统一一次性清理（该清理需单独授权））。**禁止**：在 mk 里写 `-DSTARRY_AUTH_GAME_ID`、在 Java 写死默认游戏名、删 `#error` 保护。

modern 的 `Games/*Mod.cpp` 同样从生成器产出（防闪退骨架 v3，内置懒装/双路径挂点/SafeWrite，勿从旧游戏复制）。详见 `mod菜单/starry-mod-menu/SWITCH_GAME.md` 顶部。

### 硬规则：jniLibs 才是 Gradle 打包的 so

`app/build.gradle` 已 **关闭 `externalNativeBuild`**。`./gradlew assemble*` **不会**按 `Android.mk` 重编 native，只打包：

`app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`

| 步骤 | 必须做 |
|------|--------|
| 1 | 改 `Android.mk` 切到目标 `STARRY_GAME_*` |
| 2 | `ndk-build`（产出常在 `app/src/libs/arm64-v8a/`） |
| 3 | **拷贝覆盖** → `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`（建议同步 `app/src/main/libs/`、`app/libs/`） |
| 4 | 再 `gradlew` / 跑 `surgical_*.sh` / `*_so_only.sh` |
| 5 | **装机前自检**：成品 APK（或 jniLibs）里的 so，须含本游戏标签（如 `[Gjc2]` / `gjc2`），**禁止**仍出现上一款（如 `[Dwct]`）。中文菜单串用 `LC_ALL=C grep -ac '菜单名' <so>` 按字节搜——`strings` 默认 ASCII 不认 UTF-8 中文，会漏报 |

**漏做第 3 步 = 菜单/鉴权仍是上一款游戏**（历史实例：打出上一款游戏的菜单；切游戏后 gradle 十几秒「成功」往往是旧 so）。

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

> **映射表不在本技能**（2026-10-15 m00007）：`STARRY_GAME_* → game_id → 游戏` 全表以 `mod菜单/starry-mod-menu/app/src/main/jni/GameConfig.h`（权威）与 `SWITCH_GAME.md` 为准；技能不复制会漂移的注册表，需要时直接读这两个文件。

### 验证

```bash
# 装机前（强制）
strings path/to/libstarry_magic.so | grep -E '\[<本游戏标签>\]|<game_id>'   # 且不得出现上一款的标签

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

**dex 合并目标一律自动探测（禁止硬编码 `classesN`）**：版本换代会改 dex 数量与布局（6 dex 变 4 dex、`classes3.dex` 顶到 ART 方法数上限 65533/65536 均实测过）。先跑
`bash mod菜单/starry-mod-menu/scripts/_lib/dex_target.sh --apk <底包> --need-methods <星光方法数>` 看各 dex 余量表（默认 need-methods=2048），`--print-entry` 只输出选定文件名（供外科脚本内联）；把它并进源码时按「余量最大且 ≥ 待合并方法数」选定。无候选满足 ⇒ **rc=2，回退重建 dex 方案**，不要硬塞进某个 dex（超限会在 ART 侧静默丢类）。
> 先例（dex 布局变化实测）：`10_docs/追迹迷城/技能先例.md` §2。

### 存量游戏快速重打

已有可构建工程和既有 native so 时，按改动范围选最短路径，不走全套标准阶段：

| 改动 | 动作 |
|------|------|
| Java / dex / Manifest / 菜单资源 | 复用现有该游戏的 `libstarry_magic.so`；直接跑该游戏 surgical 脚本完成构建、签名、验证、可选安装 |
| C++ / Hook / 菜单 native 串 | 先跑该游戏 `*_build_so.sh`，确认 so 指纹和菜单串，再跑同一条 surgical 链 |
| 只有现有正式包，要重装 | **首选统一入口** `bash 04_scripts/_common/install_starry_apk.sh --apk <正式包> --pkg <包名> --so-tag '[<Tag>]' --game-id <id> [--clean]`（签名/so 标签/md5 三查 + 可选异签名资源保护，见「分发与装机红线」节）；游戏脚本自带 `STARRY_INSTALL_ONLY=1` 的按其分支走。两者都**只跑验证+安装**，禁止重新构建后再声称“只是重装” |

```bash
bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh                 # 构建+签名+验证
STARRY_INSTALL=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh
STARRY_INSTALL_ONLY=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh
```

验证脚本必须从成品包重新解出目标 `classesN.dex` 和 so；不要把 `/tmp/*_*` 这类上一轮残留当成新构建证据。完整 APK 重新签名后 md5 必变，混包判断只看 dex md5、so md5、Manifest/包结构和签名身份。

★ **dex-only「菜单源码换新」路径**：机身包正常、只想把旧菜单 dex 换成 HEAD 版时，**不必重打 so**，但必须先证明 native 面没动，再外科置换唯一 `classesN.dex`：

1. 先证「只换 dex 安全」——`git diff <底包提交> HEAD -- app/src/main/java` 看改动文件，**且**新旧 dex 反编译后 `bridge/ModBridge`、`menu/NativeMenuController` 这类 native 面/登录体类必须逐指令相同（`baksmali` 两侧 `diff -rq`，只有业务 hook 类不同即可）。
2. 脚本六段：底包校验（zip 结构 + 星光 dex 落位正确）→ HEAD `gradlew assembleRelease --offline` 取 dex → `apktool d` + `StarryApp.smali` **宿主改写四处**（`.super`/`<init>`/`attachBaseContext`/`onCreate` → 本游戏宿主 Application）→ `smali assemble` 出新 dex + 断言 → zip 外科置换**只换该 `classesN.dex`**（逐条目 sha256 审计 + so 与底包逐字节相同断言）→ `zipalign`/`apksigner` + 证书钉 + 门禁 + 可选装机。
3. 两个必踩的坑：① **证书钉要归一化**——`apksigner` 输出小写无冒号，`keytool` 指纹大写带冒号，直接比会假失败（`tr -d ':' | tr 'A-Z' 'a-z'` 后再比）。② **判 dex 里的中文串一律用 `python3.11` 字节断言，不要 `grep -aq '中文'`**（本机 locale 下会假失败）；并且别把脚本接 `| tail`（pipeline 退出码取 tail，脚本 die 了也显示 0，会误判成功）。
4. **取号/授权类单点路径必查兜底**：只走单一设备标识（如 v5 SSAID）时，标识不可用即抛错 ⇒ 菜单整块不可用；应具备 v5 优先 + v4 硬件指纹兜底。

> 先例（脚本六段实例、宿主改写目标、反例记录）：`10_docs/独奏骑士/技能先例.md`。

★ **菜单 dex 新鲜度铁律**（2026-10-26 起，出包阶段必过）：出包脚本是「**模板优先，缺了才现编**」——
只要 `templates/<tpl>/classesN.dex` 还在，菜单 dex 就**永远不会重编**，改过的 Java 进不了包。
两天内同源三起（无限升级 vc1637「重启后功能全关」／镇邪人 `7f94233e`／`a3748024`），根因都是模板停在旧日期。

1. **模板目录去日期化**：统一 `templates/<game>_current/`，刷新即覆盖。刷新脚本默认输出就是它，
   出包脚本默认指向它（旧日期目录只许当 `WXSJ_TPL_PREV` 对照用，不许当出包输入）。
2. **刷新脚本产出的模板必带两个身份**：`build_info.txt`（`source_commit` = `git log -1 -- app/src/main/java`、
   源树 sha256、built_at、gradle 任务）＋ dex 内构建戳 `starry-menu:<commit12>`（注入到 `BuildConfig.smali`，
   release 的 `-dontobfuscate` + `-keep com.starry.magic.**` 保证类名与常量都在）。
3. **出包/装机两道机器关都会拦**：`bash 04_scripts/_common/menu_tpl_gate.sh --tpl <模板目录> --dex-name classesN.dex`
   （模板分支）与 `pre_install_gate.sh --menu-dex <成品APK> --menu-dex-name classesN.dex`（查10）。
   判据只有一条：**dex 构建戳 == 当前 Java 源码 HEAD**。不相等 ⇒ 先刷新再出包，禁止 `--allow-stale` 出正式包。
4. **现编必须用 release**：`MainActivity` 的自检入口由 `if (BuildConfig.DEBUG && …starry_selftest…)` 守卫，
   debug 包会带上该串，被 `check_release_apk` 判「调试脚手架」拦（实测踩过）。`WXSJ_GRADLE_TASK=assembleDebug`
   只做取证，不出正式包。
5. 刷新前先看 `git status -- app/src/main/java`：脏工作区会把别人未提交的 Java 打进 dex（脚本会 WARN）。

**已注册管线的一律走 `04_scripts/pipeline.sh`**（status/next/reset/verify/verify --set），自动防旧产物直装：上游产物变化 → 过期下游阶段自动回退 pending；过期链路上 `run` 下游会被陈旧度门禁拒绝。构建后端四件套（绝对路径/obj stamp/链接活动保险/菜单串字节校验）范本见 `04_scripts/_common/stages.md`；install 阶段末尾接 `04_scripts/_common/verify_installed_apk.sh` 校验设备包 md5 == 本地 APK。

★ **外科组装 dex 落位铁律**：新增 smali 编出的 dex 名必须是 base.apk 里**最小的空闲下标**（单 dex 官包 → `classes2.dex`）。写死大下标时 ART 的 `BaseDexClassLoader` 从 `classes.dex` 顺序扫、遇到第一个缺口即停止 → 星光 dex 永不加载 → 入口类 `ClassNotFoundException` **启动即崩**。
   - 可用 `python3 04_scripts/_common/next_free_dex.py <官包.apk>` 求最小空闲 N，再写 `classesN.dex`。
   - SDK 侧两条：① 官包无 `libtprt.so`/ACE 时 `Android_<tag>.mk` 必须含 `Games/NoAceStubs.cpp`（否则 `ld.lld: undefined symbol: SignalQuarantine_Start`）；② hook 日志 tag 是 `Mod_Menu`（`Includes/Logger.h` 的 `TAG=OBFUSCATE("Mod_Menu")`），不是 `StarryMagic`。
> 先例（写死 dex 下标导致启动即崩）：`10_docs/圣翼传说/技能先例.md`。

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

> **沙箱/权限**：`./gradlew` 要写家目录缓存 `~/.gradle`。只允许写工作区的沙箱（如 DSH file policy=`workspace-write`）下会直接失败：
> `java.io.FileNotFoundException: …/.gradle/wrapper/dists/gradle-8.13-bin/…zip.lck (Operation not permitted)`。
> 结论：**构建命令一开始就带一次性放宽权限运行**，不要先撞一次再重跑（重跑还要重编 so，白烧一轮）。

优先用各游戏的 `scripts/surgical_*.sh` / `*_so_only.sh`（脚本内应已含拷贝与自检）。

---

## 入口点决策表

> 四路入口的适用条件与已否决判据，**首选 ContentProvider**。
> 出包前先跑 `bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> --apk <apk>`（逐项 ✓/✗，任意 ✗ 禁止出包）。
> 对某游戏已知无害的 ✗，登记 `10_docs/{游戏}/审计豁免.json` 后用 `04_scripts/_common/audit_with_waivers.sh` 复核（过期自动恢复 ✗）。

| 入口 | 写法 | 适用条件 | 已否决判据（为什么不要先选它） |
|------|------|----------|-------------------------------|
| **ContentProvider（首选）** | `<provider android:name="com.starry.magic.StarryInitProvider" android:authorities="<包名>.starryinit" android:exported="false"/>` | 默认全场景选它。`ContentProvider.onCreate` 早于 `Activity.onCreate`，且晚于壳的 initialize 完成（安全窗口） | 仅当该包已有同 authorities 的 provider 冲突时才另议 |
| Application | `<application android:name="com.starry.magic.StarryApp">` | 仅纯正 Android 应用，且能在 logcat 看到 StarryApp 行 | **实测平台可能不采用**：aapt2 复读合法但运行时 StarryApp 从未实例化（无 StarryApp 行、`/proc/<pid>/maps` 无 so），且类缺失本应 `Unable to instantiate application` 却未崩 ⇒ 该属性被忽略。先例：`10_docs/追迹迷城/技能先例.md` §1 |
| appComponentFactory | `android:appComponentFactory="com.starry.magic.StarryAppCompFactory0"` | 宿主已自带 appComponentFactory 且可**等长替换**（`androidx.core.app.CoreComponentFactory`，38 字符） | ACE/TSS 类壳：替换 Application 会让 `AceApplication.initialize` 在 native 信号量上毒丸挂起（黑屏 Splash + 后续被击杀） |
| 宿主 Activity 注入 | 借壳自己的入口类（`ysxz/`、`conch/`、`h5box/` 等分支） | 宿主已声明自家 Provider/Factory，或需挂在宿主 Activity 生命周期点上 | 需逐壳适配、换壳即失效，不作为跨游戏默认方案 |

- **Provider 顺序铁律**：必须排在其它 Provider（如 TapTap 的 `TapTapKitInitProvider`/`TapUpdateFileProvider`）**之前** —— Provider 顺序＝平台实例化顺序，先回灌官方证书再让 SDK 读。参考实现见 `04_scripts/追迹迷城/patch_zjmc_binary_manifest_v3.py`。
- **黑屏/无菜单先查入口三件事**（都不会让构建报错）① 入口类不在 dex（`ClassNotFoundException`）② Manifest 里没写进去（用 `aapt2 dump xmltree --file AndroidManifest.xml <apk>` 复读）③ 入口被壳的 Application 挂死。
- **现有入口类清单**（`java/com/starry/magic/`）：`StarryInitProvider`（Provider，推荐）/ `StarryApp`（Application）/ `StarryAppCompFactory0`、`sign/SignSpoofAppComponentFactory`、`ysxz/YsxzAppComponentFactory`（Factory）/ `conch/DcslConchProvider`、`h5box/StarryH5Provider`、`h5box/CzbhzH5Provider`（壳专用 Provider）。

## TapTap 接入检查表

> 五条全是**静默失败**：零日志、不崩、构建全绿，装机后才发现。

1. **强制网页登录＝覆盖 intent extra**：要改的是登录 intent 里的 extra `loginType="embedded_web"`；改 `ClientLoginUtils.isClientLogin` 无效（它不参与该路径判断）。
2. **`PKG_WHITELIST` 必须含目标包名**：`sign/TapLoginForceWeb.java` 的 `PKG_WHITELIST`（`java.util.Set` 字面量）漏配该包 ⇒ 多处 `if (!PKG_WHITELIST.contains(activity.getPackageName())) return;` 静默 return、**零日志**。先例：`10_docs/追迹迷城/技能先例.md` §3。
3. **`SignSpoof` 证书表须有该包条目**：`sign/SignSpoof.java` 里 `<TAG>_PKG = "<包名>"` 常量与 `fakes.put(<TAG>_PKG, new Signature(Base64.decode(<TAG>_CERT_B64, Base64.DEFAULT)))` 两件齐备才算生效（只写了常量、漏 `fakes.put` ＝ 没有伪装）。
4. **客户端 SSO 永不可用的判据**：`TapSignKit.getSign` 走 `PackageManager.getPackageInfo(pkg, 0x40)` 取签名 ⇒ 被 `killPm` 代理覆盖 ⇒ **进程内可过、系统侧过不了**，故不要指望客户端 SSO，能通的是网页登录那条路。
5. **判据：`AppSwitchEvent(targetName=com.taptap)` 出现＝仍走客户端**（说明上面的覆盖没生效）。

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

## 授权会话（卡密）预置与诊断

> 关键教训：**「官方包卸装后卡密会话丢失」的根因可以只是 Android SharedPreferences 的 `value=` 属性写法**（先做便宜的三步，别从命名空间/看门狗查起）。实证复盘：`10_docs/_state/history/reviews/规范优化_20261015_霓虹深渊2r19会话复盘.md`。

**症状 → 首查项**

| 症状 | 首查 | 证据行 |
|---|---|---|
| 菜单能开、功能全不生效 | 授权会话 | `[验证] grant rejected`（对照 `grant ok exp=…`） |
| 连 `install pass` 都没有 | 导出摘名 | `[Il2Cpp] core api missing (xdl+dynsym)` |
| `install pass: ok=N/72` 但 N 变小 | 槽位入口形态 | `install skip: N 个槽位因「入口形态不安全」主动跳过` |

**prefs 解析规则（违反 ⇒ App 拿到空 map，现象与「被谁清空」一模一样）**

- 文件：`/data/data/<pkg>/shared_prefs/starry_magic_prefs.xml`（授权 + 功能开关的唯一载体）。
- `string` 值在**文本体**；`int/long/float/boolean` 值在 **`value=` 属性**；`set` 子元素是 `<string>`；根是 `<map>`；每项必须有 `name`。
- 写成文本体 `<boolean name="5">true</boolean>` ⇒ 解析抛异常 ⇒ 空 map ⇒ App 用代码缺省重写 9 个键、`auth_*` 全丢。
- 判据：`auth_backup_*` **不存在** ⇒ 不是 `clearAuthSession()` 干的；`StarryPrefs: 功能列表变化 → 已清功能偏好；prev=[]` ⇒ 它读到的是空 map。
- **禁止离线生成整份 XML**（实测同判空 map）；只能在 App 自己写的文件上**拼接**。

**工具（唯一入口，别再手搓 seed 脚本）**

```bash
bash 04_scripts/_common/starry_prefs_edit.sh --show                       # 列键（token/grant/card 自动脱敏）
bash 04_scripts/_common/starry_prefs_edit.sh --validate <本地 xml>         # 写前体检（rc=3 时打印修法原文）
bash 04_scripts/_common/starry_prefs_edit.sh --seed-auth --grant '<grant>' --token '<t>' --card '<卡密>' \
     [--exp '<ISO>'] [--last-ok <ms>] [--launch --verify]                  # 拼接 + 冷启 + 判据复核
bash 04_scripts/_common/starry_prefs_edit.sh --seed-features 5,7,8,9,10,11,12 [--launch --verify]
```

护栏：写前 `am force-stop` 且 pid 必须为空（否则 rc=4）；XML 不合法拒写（rc=3）；写后复核大小与关键键（rc=5）。

**编辑纪律**：同一文件在同一轮内**要么全用 bash 改写、要么 `read` 后再 `edit`**，禁混用 —— 混用会报 `file changed since it was read`（P23「编辑一把刀」）；同一文件写后要再 `edit`，先重新 `read`。

---

## 装机前置检查（G1 必查）

> 来源：18 份会话记录审计——「防封审计做了吗」「异常兜底做了吗」曾被用户重复追问才补做；重复追问 = 流程缺口。

装机（首装或覆盖）前逐项过，缺一不装机：

1. **产物机器关**：`bash 04_scripts/_common/pre_install_gate.sh`（JS 语法/JNI 配套/P/Invoke 符号/控件读取/门禁串/Manifest 形态六查，见 stages.md 专节）
   - 控件读取查优先传 `--runtime-menu-json`；后台菜单模板按规范不能带 `type`，不能单独用它判断 Slider/Toggle。
   - 门禁串写**类名时用斜杠形式**（`com/x/Y`）；写成点分名会被自动归一为斜杠并 WARN 放过。
2. **防封/上报审计**：按 `../re-implement/anti-ban-analysis.md` 输出标准结论（上报链清单 + 量级线 + 服务端权威判定），无审计结论不装机
3. **异常兜底审计**：战斗代码 + 菜单 Java 侧（`ModBridge.safeCall` 包裹、native 缺失/偏好类型不符的降级路径）都要有 try/catch 兜底结论；HybridCLR 游戏热更侧调 native 一律文件镜像通道（`starry_state.txt` 模式），桥接调用必须 try/catch
4. **TapTap 渠道游戏三查**：
   - 签名校验存在性：静态扫 `CheckSignature`/`getSign`/`getMD5`/`packageSign` 类入口，有则逐一给对策
   - `SignSpoof` 证书回灌：官方证书 SHA 已嵌入（含 SDK 商签名映射，如 Openew/TapTap）
   - `TapLoginForceWeb` 白名单含本包名；旧版 TapSDK（如 3.30.2）注意 `TapTapActivity` 客户端 SSO 分支需单独处理
5. **game_id 独立性**：新渠道/新包禁止复用其他渠道 game_id（历史事故：临时复用同 id 致后台模板错位、返工重建 so）；按命名规则独立申请并四点同步（GameConfig.h/后台/菜单模板/文档）

**收尾交付物（icon / 菜单模板为按需产出：新游戏首次交付或用户点名时才生成）**：
- icon 提取（按需）：交付副本放 `08_output/{游戏}/delivery_YYYYMMDD/icon_{游戏}_{源图边长}.png`（**边长＝源图自身像素，不放大**；2026-10-14 用户 m00631/m00668 定案，旧 `_512`/`_144` 双档已废弃）；`02_apks/` 只作为用户管理的不可再生输入，AI 不默认写入。
- 菜单模板 JSON：`10_docs/{游戏}/{游戏拼音}_menu_template.json`（`template_id`=game_id）
- 交付副本（按需）：`08_output/{游戏}/delivery_YYYYMMDD/menu_template.json`，字段严格按 `10_docs/_templates/菜单模板_规范与示例.md` 自检。
- 交付目录生成（**★收尾一律走这条，禁手工重造**）：先读 `04_scripts/_common/stages.md` 收尾节，再跑
  `bash 04_scripts/_common/finalize_game_delivery.sh --game {游戏} --apk {正式包} --prune --sync-doc --apply`
  —— 裁剪唯一正式包 + **打印**文档同步片段（共享文件仍需手动粘，脚本不自动改）。
  **icon / 菜单模板按需**：仅「新游戏首次交付」（`08_output/{游戏}` 无 `delivery_*`）或用户当轮点名时才追加 `--icon-source {原图} --menu-template 10_docs/{游戏}/{id}_menu_template.json`，生成 delivery 目录（**icon 一张原图** + 模板副本 + 字段自检）；其余收尾不传，脚本默认跳过、不建 delivery 目录。
- **icon 尺寸口径（2026-10-14 起，取代旧的 512/144 双档）**：取**官方最高密度**那份原图（`aapt2 dump badging <apk> | grep application-icon-` 看各 dpi，通常是 `res/mipmap-xxxhdpi/app_icon.png`），**不放大、不缩小**，文件名边长＝源图自身像素（192×192 → `icon_终末地_192.png`）。源图只到 192×192 属正常，**禁止放大后谎称原生 512**，注明来源密度即可（工具头注即权威：`finalize_game_delivery.sh:1-10`）。
- 收尾清单：按 `10_docs/_templates/游戏收尾与最小复现清单.md` 填写，含生命周期场景矩阵已测/未测

---

## 分发与装机红线（强制，适用于所有游戏）

### 分发自检：文档说修了 ≠ 用户拿到的包修了

「源码已修 + 本机验证」不等于用户手里的包已修。**任何游戏**对外分发前逐条执行：

1. 正式包必须**本次重打**为 `08_output/{游戏}/` 下新文件，不默认覆盖或整理既有最终包（见 AGENTS.md）；trace 包、旧包、只换 so 未刷 dex 的包一律不得当正式分发
2. 成品 APK 内 so 必须是本游戏 + 本版本：`unzip -p 成品.apk lib/arm64-v8a/libstarry_magic.so | strings | grep '<本游戏标签>'`
   - 同管道补证书自检：`... | grep -a -c 'BEGIN CERTIFICATE'` 应 ≥1；新游戏包 `assets/certs` 可省，详见 [surgical-build.md](surgical-build.md)「证书固定来源」
3. 改过 Java（MenuService / StarryApp 等）时，baksmali 成品 dex 验改动真的打进了包，不信「源码已改」

### 直装包不得「只换 libil2cpp.so」（2026-10-19 我的勇者 7.6.3「首装闪退」实证）

热更构建与包内构建是**两份资产**：`libil2cpp.so` 与 `global-metadata.dat`（及 `.dll-resources`）必须**同源**。
把热更 so 塞进底包、metadata 留底包版 ⇒ 干净机（无 `files/update/current` 副本）被游戏自带补丁框架
（如 `r2.labs.unitypatch`：`PatchCheck.NewPatchSoDir == false` → `LoadLibrary.false`）回退加载包内 so
⇒ il2cpp 互递归爆栈、启动约 7 s 崩；**已热更过的机器反而一切正常**（运行时根本不加载包内 so，见
"清数据 = Mod + 热更资源双杀"同源证据）。

- 出包二选一：① **不换 so**（用底包 so，Mod 走 `HookSlotInstall` 的按名兜底；先例 `surgical_wdyz_7.6.3.sh`
  的 `STARRY_SKIP_SO_SWAP=1`）；② so + metadata + 资源**成套**换（同一份热更抓取，并写明来源与 sha256）。
- 装前机检（必过）：`bash 04_scripts/_common/pre_install_gate.sh --apk 成品.apk --base-apk 官方底包` → 查9 配套关。
  只换一个 = FAIL；两个都换 = WARN（机器判不了同源，需文档写明抓取来源与 sha256）。
- 抓到的 RVA 必须标明属于哪份构建；写死 RVA 的补丁在换构建后必须**显式跳过并留日志**
  （先例 `Games/WdyzMod.cpp` 无CD 补丁的 `expect drift → skip`），不得静默生效。

### 干净安装才算验收（2026-10-19 起，全游戏）

**覆盖安装 ≠ 首装验收。** 我的勇者 2026-10-18 在「已热更旧机」上覆盖安装验证通过即交付，次日干净机复现启动必崩。

- 对外分发 / 换设备前必须做一次「卸载 → 全新安装 → `post_install_precheck.sh`」；覆盖安装只作附加证据。
- 只做覆盖安装时，交付结论必须标 `NOT PROVEN：仅覆盖安装验证`，不得写「用户实测通过 / 已交付」。
- 卸载会清 `/sdcard/Android/data/<pkg>` 与 app 私有数据 ⇒ 先取得用户同意；无 root 机走 `adb uninstall`
  （`install_starry_apk.sh --clean` 自 2026-10-19 起自动降级，并明示「外置数据不可归位、重装需重下资源」）。

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
- **覆盖安装后「一开就崩 / 行为像旧版」≠ 包坏，先查是不是仍在跑旧 so**（2026-10-16 终末地/小米 byron 实证，FW-13）：框架**优先加载**玩家已下载的 `files/starry_ota/<game_id>/libstarry_magic.so`（`StarryApp.java:78`「优先 OTA」、`ModBridge.java:60`「有 OTA 文件则只 load OTA」）；覆盖安装时清不清它由**包内 so 的构建戳**决定。若包内 so 缺戳（modern 入口曾因 `--gc-sections` 丢戳，已由共享 `Includes/Utils.cpp` 的 `retain` 修掉）或设备做过 root so 热替换（`so_hotswap.sh`），就会「`base.apk` 正确、跑的却是旧 so」。
  - **判据**：`su -c md5sum /data/app/*/<pkg>-*/lib/arm64/libstarry_magic.so` 与包内该 so 的 md5 比对（现由 `verify_installed_apk.sh` 第二查自动做）。
  - **玩家自救（不丢数据）**：菜单「**清除游戏补丁**」（`NativeMenuController.java:1405`）→ 确认 → **完全划掉游戏再开**。
  - **分发说明统一加一句**：*装过旧星空版的，先点菜单「清除游戏补丁」再进*。

### 异签名必须卸载重装时：先保护外置数据（2026-10-16 终末地 29,990 MB 实证；FW-12）

同证书自签包走覆盖安装；**证书不同（异签名）时覆盖装必失败**，只能卸载重装——而 `pm uninstall` 会
**连带清除** `/sdcard/Android/data/<pkg>`（大资源游戏可达数十 GB，且**静默无报错**）。标准步骤：

1. **落备胎**（同卷 `mv`，瞬时且零额外空间）：`su -c 'mv /sdcard/Android/data/<pkg> /sdcard/Android/data/<pkg>.nxreserve'`
   —— `.nxreserve` 是另一个名字，卸载的目标目录不含它，故不会被连带清掉。
2. **核对备胎非空才允许卸载**（硬闸门）：`ls -d <reserve>` 拿不到就**中止，绝不裸卸**；确认可丢要显式放行。
3. 卸载 → 安装星空包（`su -c 'pm install -r -d --user 0 <tmp.apk>'`）。
4. **归位必须 chown 到新 uid**：重装会**重新分配 app uid**（实测 11058→11059），旧属主下 app 读不到自己的
   外置目录 ⇒ `chown -R <新uid>:<新uid> <dir>` + `restorecon -R <dir>`（新 uid：`stat -c %u /data/user/0/<pkg>`）。
5. **判据**：归位后 `du -sm` 与卸载前一致（本例 29,990 MB）、`ls -ld` 属主 == 新 uid。

**统一入口（只装机 / 异签名重装都走它，FW-11）**：

```bash
bash 04_scripts/_common/install_starry_apk.sh --apk <正式包> --pkg <包名> \
    --so-tag '[<Tag>]' --game-id <id> [--clean] [--serial <S>] [--dry-run]
```

先做「签名身份 `CN=StarryMagic` / 包内 so 标签与 game_id / md5」三查，再装机；`--clean` 即含上面 1–5 步。
**优先用它，而不是逼着重跑外科脚本重建**——重签会改产物 md5、使已取证/已交付的包与证据脱钩。
自带 `STARRY_INSTALL_ONLY=1` 分支的游戏脚本（如 `surgical_endfield_1.6.4.sh`）行为等价，保留即可。

> **游戏级例外不在本技能展开**：任何「必须卸载重装」类例外只写在该游戏 `10_docs/{游戏}/硬约束.md`，此处只承认「按该文件逐条生效、只对该游戏有效、不得外推」这一条通用规则。现存先例：`10_docs/无悔华夏/硬约束.md` 第 4 条（带乐变类热更派生负载的游戏）。其它同证书自签包一律按本节默认处理（`TOOLBOOK.md` 规则 #77 三档：root 覆盖 so → `install -r` → 只有证书变了才 `uninstall`）。

---

## 设备指纹

```bash
# 仅在标识恢复的具体范围已授权且恢复前置满足时使用；见 TOOLBOOK「真机残留清理」
bash 04_scripts/_common/ensure_starry_device_id_seed.sh
```

改指纹相关代码**必须重打含菜单 DEX 的包**，只换 so 无效。

详见 `auth-chain.md`
- **框架改版总结（2026-09-05/06）**：心跳→冷启动对账、票 TTL=86400、CrashHandler、OBFUSCATE+RegisterNatives、解绑新规（首次免费/后续扣3天）等全部机制、坑与验证方式见 [10_docs/_templates/经验_星光框架大改版_20260906.md](../../10_docs/_templates/经验_星光框架大改版_20260906.md)；自测包构建用私有 `Android_self_test.mk`（jniLibs 与在跑会话竞争，勿共用）。

---

## 验证

```bash
adb logcat -s StarryApp:V StarryMagic:V AuthClient DeviceFingerprint | grep -E \
  'auth game_id|native menu shown|source=shared'
```

逐项检查：菜单弹出且**功能名对本游戏**、每个功能有效、关闭后恢复、登录正常、对局结束不闪退。

---

## 菜单索引与授权验证边界

- **适用范围**：当前工作区星光框架的 `g_featureList` 数组索引协议；不保证其他框架或后续版本沿用。
- 历史案例与证据：见 `10_docs/_state/games/无限升级.md` 的 YAES/运行时 dump 段（原 `10_docs/无限升级/运行时dump与YAES破解_20260905.md` 已按清理口径下移，正文以该摘要为准）（“已合并”不等于所有命题 PROVEN）。

### featNum 与 Category

- 当前框架的 `featNum` 沿 `g_featureList` 数组下标（从 0 起），不是“第几个可点击控件”。`Category` 自身无功能回调，但仍占数组下标；后续功能编号不能跳过它重计。
- 对齐检查须覆盖数组、前端 `data-featnum`、native 分发/枚举、后台模板 `feat_num` 与配置键；以正常 UI 逐项开关及关闭恢复验证实际对应关系。
- 历史上的编号错位事故源于漏计 Category，且曾被「adb 直写配置」的验收掩盖 —— **配置消费成功不证明菜单接线正确**。
- **待核对实现风险**：`Main.cpp` 旧注释称 Category“不占 featNum”，与同文件“数组下标”说法及历史案例相反。**不改代码、不按旧注释改编号**，后续核对实际渲染/分发实现。完整 UI 运行链 **NOT PROVEN**。

### 心跳、偏好重推与测试覆盖

> **2026-09-05 机制变更**：60s 周期心跳与每次全量重推已删除，改为冷启动一次对账（权威硬拒才踢、会话复活才重推、票 TTL=86400s）。下述“心跳”描述为历史记录，现行机制以 [auth-chain.md](auth-chain.md)「冷启动对账机制」为准。

- 历史上的“60s 级”心跳与偏好重推属有条件的历史现象，不是“任何未授权状态每 60s 必然全 OFF”的定律。
- 配置被覆盖须区分 bootstrap、心跳、登录成功等触发源，并核对服务是否运行、当前偏好、授权/会话状态和 native 门控；具体触发条件、值来源与时序需对应构建日志证明，未覆盖部分 **NOT PROVEN**。
- adb 配置旁路只证明该次输入经过的配置消费与局部行为；不验证 UI 映射、正常授权、关闭恢复或重启恢复链。
- 不收录停服务来绕开授权的做法。测试隔离仅用于明确授权的局部诊断，不代替正常端到端验证；正式验证须经正常授权与菜单入口，覆盖授权状态变化及恢复行为。
- 正常链按 [auth-chain.md](auth-chain.md) 核对，但不能仅因服务运行或菜单可见就宣告授权成功；需要对应 game_id、实际会话与目标功能的证据。

## 收尾

按 [TOOLBOOK.md](../../TOOLBOOK.md)「清理规则」「真机残留清理」收尾，填写 [游戏收尾与最小复现清单](../../10_docs/_templates/游戏收尾与最小复现清单.md)。保留输入、隔离重建、具体授权和设备分责以该处为准；清理脚本不默认运行，缺证据或无法约束范围时记录待处理。

收尾入口统一为：

1. 清理后先跑 `04_scripts/_common/pipeline_record_cleanup.py` 修管线快照。
2. Git 固化用 `04_scripts/_common/archive_scoped_git.sh` 白名单提交；脚本默认提交后推送 `origin/main`，只有明确不想推送时才加 `--no-push`。
   - 若白名单文件混有其它会话改动（常见于 `10_docs/_state/current.md` 和共享 Java 白名单），先做 hunk 级暂存，再跑 `--staged-only`。
3. pipeline 写回 `archive=done` 后，用
   `04_scripts/_common/archive_close_git.sh --game {游戏}` 补状态提交。
4. 游戏详情更新到 `10_docs/_state/games/{游戏}.md`，`current.md` 只保留一行；
   功能反复反馈和长证据分别补功能验收卡、证据 README。
5. 收尾后用 `python3 04_scripts/_common/check_state_hash_sync.py --game {游戏} --manifest {裁剪清单}.json` 核对摘要、索引行与正式包 hash 一致。
