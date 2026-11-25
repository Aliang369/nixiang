# 鹦鹉团 1.0.7 菜单 dex 重编（2026-11-25）

> 背景：上一版《鹦鹉团》星光版组包复用了 **2026-11-20 21:13** 的 `mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk`
> （菜单 dex 停在 11-20，而框架 Java 在 11-21～11-24 有 11 次提交）。违反 `skill/starry-mod/SKILL.md`「★ 菜单 dex 新鲜度铁律」。
> 本轮按当前 HEAD 用 gradle release 重编菜单 dex，产物已落到去日期化模板目录并过 `menu_tpl_gate`。

## 0 结论

| 项 | 结果 |
|---|---|
| 旧菜单 dex 落后 | **11 个提交**（最后触及 `app/src/main/java` 的提交 = `45c80c643bfd` 2026-11-24 19:19，旧 dex 组于 11-20） |
| 新菜单 dex | `classes.dex` 590,072 B md5 `4979e004a581137c5a83466b8834d80f` |
| 新鲜度实证 | 新串（`游戏停止使用，功能已关闭` / `扩大窗口可继续查看公告` / `/v1/presence`）**只在新区 dex 命中**；旧串（`游戏维护中，功能已关闭`）**只在旧 dex 命中** |
| 门禁 | `check_release_apk.sh` = 干净正式包；`menu_tpl_gate.sh --tpl` = **PASS（rc=0）** |
| 模板目录 | `mod菜单/starry-mod-menu/templates/ywt_menu_current/` |

## 1 旧 dex 过期的取证

```bash
git log -1 --format='%h %ad' --date=short -- mod菜单/starry-mod-menu/app/src/main/java
#   45c80c64 2026-11-24
git log --since=2026-11-20 --oneline -- mod菜单/starry-mod-menu/app/src/main/java | wc -l
#   11
md5 -q mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk   # 旧包（构建前）
#   8bc18a299b20211717b628a12e94c182   size 8,785,571 B   mtime 2026-11-20 21:13
```

11 个提交覆盖 11-21～11-24（`4d6ac207` `e08b8ba1` `c0a55d25` `ee436041` `65e2b69e` `e09920ca` `8cb73955` `34ebcd8a` `f0d901f6` `e0664679` `45c80c64`），
其中直接改菜单行为的有：`8cb73955`（停用提示文案改写 + 首启隐藏账号 Tab）、`34ebcd8a`（公告三按钮可配 + 优先展示时序）、
`65e2b69e`（分类收起/展开重排 + 登录按钮居中）、`e09920ca`（清理 WebView/HTML 菜单残留）、`f0d901f6`（PresenceReporter 心跳）。

**旧 dex 的身份**（用于新鲜度反证）：上一版成品包 `08_output/鹦鹉团/鹦鹉团_1.0.7_starry-full-20261125.apk` 内的 `classes14.dex`
= `13_scratch/ywt_build_1.0.7_full/menu.dex`（组包脚本取的就是旧 `app-release.apk` 的 classes.dex），两者 md5 同为
`8e450db022db25dd36e7767b24777e74`，434,280 B。

## 2 工作区脏文件（如实记录，未提交、未还原）

`git status --porcelain -- mod菜单/starry-mod-menu/app/src/main/java`：

```
?? mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/games/mrfzzmd/GameHookImpl.java
```

- 归属：**终末地（mrfzzmd）**的 TapTap 强制内嵌网页登录 SPI（文件头注释自述），目录 mtime 2026-11-25 17:31，
  非本任务、非本次提交内容。按边界未提交、未还原。
- 影响：**该未提交源码会被编进本次 dex**（实证：新 dex 内有 `Lcom/starry/magic/games/mrfzzmd/GameHookImpl;`，旧 dex 没有）。
  它只由 `GameHooks.resolve()` 按 `com.starry.magic.games.<game_id>.GameHookImpl` 约定名反射装载，
  鹦鹉团 game_id 为 `ywt`，故在鹦鹉团包内是**未使用的驻留类**，不影响功能；但它属于他人未交付代码，是否随 dex 出货由主代理决定。
- 其余 `app/src/main/java` 无 `M`/`D` 项。

## 3 构建命令与耗时

```bash
export HOME=/Users/aliang
export PATH=/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin
export TMPDIR=/tmp
export JAVA_HOME=$(/usr/libexec/java_home -v 17)     # jdk-17.0.2
export ANDROID_HOME=/Users/aliang/Library/Android/sdk
cd mod菜单/starry-mod-menu
./gradlew assembleRelease --offline
```

- `--offline` **一次通过**，未出现缺依赖（无需去 offline 重试）。
- Gradle 输出：`BUILD SUCCESSFUL in 9s` / `50 actionable tasks: 29 executed, 21 up-to-date`；
  `Task :app:compileReleaseJavaWithJavac` 实际执行（非 UP-TO-DATE），`:app:minifyReleaseWithR8` 执行。
- 从后台启动到 APK 落盘 **10 s**（daemon 热 + 增量编译；不是全量 clean build）。
- 日志：`/tmp/ywt_menu_build.log`。

## 4 新产物指纹

| 产物 | size | md5 | sha256 | 时间 |
|---|---|---|---|---|
| `app/build/outputs/apk/release/app-release.apk` | 877,842 B | `83801cf991e411d56c755d61b8ff0c1e` | — | 2026-11-25 22:25:47 |
| 解出的 `classes.dex` | 590,072 B | `4979e004a581137c5a83466b8834d80f` | `9e1287b8953ca4ee95e6c939541a2655aca2f43af1e2c5682506abc6c25e56af` | 2026-11-25 22:26:57 |

- APK 内 dex 数量：**1 个** `classes.dex`（+ `lib/arm64-v8a/libstarry_magic.so` 201,528 B、`lib/arm64-v8a/libc++_shared.so`、`assets/starry/*_patch.js` 3 个）。
- 必需类名断言：`StarryInitProvider` / `MenuService` / `ModBridge` 三个均在（新 dex 与旧 dex 都有）。
- **APK 体积落差说明**：旧包 8,785,571 B → 新包 877,842 B。组包脚本 `04_scripts/鹦鹉团/build_ywt.sh` 只从菜单 APK 取
  `classes.dex` 与 `lib/arm64-v8a/libstarry_magic.so`（见脚本 61/144/146 行），其余条目不进成品，**不影响组包**。
  现 `app/src/main/jniLibs` 只有 `arm64-v8a` 一个 ABI（且未被 git 跟踪），旧包体积差来自更早构建的额外载荷，本轮未重建它。

## 5 新鲜度实证（新旧互斥）

字节级断言用 `python3.11`（不用 `grep -a '中文'`，本机 locale 会假失败）：

| 字符串 | 新 dex | 旧 dex | 出处 |
|---|---|---|---|
| `游戏停止使用，功能已关闭` | **HIT** | — | `8cb73955` 停用提示改写 |
| `扩大窗口可继续查看公告` | **HIT** | — | `34ebcd8a` 公告几何/时序 |
| `announce_exit` | **HIT** | — | `34ebcd8a` 公告三按钮 role |
| `退出游戏` | **HIT** | — | `34ebcd8a` ANNOUNCE_EXIT_DEFAULT_LABEL |
| `/v1/presence` | **HIT** | — | `f0d901f6` PresenceReporter 心跳 |
| `starry-presence` | **HIT** | — | `f0d901f6` 线程名 |
| `游戏维护中，功能已关闭` | — | **HIT** | 同处**被替换掉的旧文案**（反证） |
| `立即前往` | HIT | HIT | 两版都有（默认 label 未变），不作判据 |

类描述符：`Lcom/starry/magic/games/mrfzzmd/GameHookImpl;` 新有旧无（= 第 2 节未提交源码被编入）；
旧包里的 `Lcom/starry/magic/menu/net/CdnVpnService;` 等 `menu/net/*` 类新包**已无**（`e08b8ba1` OTA/VPN 整链路退役，方向一致）。

## 6 门禁输出

```
$ bash 04_scripts/_common/check_release_apk.sh --apk mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk
APK : ... (877842 B  md5 83801cf991e411d56c755d61b8ff0c1e)
── 查1/查3 manifest ──  ✓ 无 application-debuggable   · versionCode=41 versionName=3.11
── 查2 禁用串扫描（dex/assets/lib）──  · 扫描文件 6 个   ✓ 无禁用串命中
结论：✓ 干净正式包     (rc=0)

$ bash 04_scripts/_common/menu_tpl_gate.sh --tpl mod菜单/starry-mod-menu/templates/ywt_menu_current --dex-name classes.dex
  源码身份: java 树最近提交 45c80c643bfd
  build_info: source_commit 45c80c643bfd
  ⚠ dex 内无构建戳（javac+d8 管线不带戳；本次以 build_info.source_commit 为准）
✓ menu_tpl_gate: dex 与源码同步（45c80c643bfd）      (rc=0)
```

- 未用 `--allow-stale`、未用 `--require-stamp`。
- **关于构建戳 `starry-menu:<commit12>`**：该戳由 `wxsj_refresh_menu_tpl.sh` 经 `apktool d` + smali 改写 `BuildConfig.smali` 注入，
  属 dex 重编路径；本轮按任务口径走 gradle 直出、不注入戳，门禁据此按 `build_info.source_commit` 判定（缺戳 = WARN 放行，
  `pre_install_gate.sh` 查10 调同一脚本且不带 `--require-stamp`，同样放行）。

## 7 模板目录（刷新结果）

```
mod菜单/starry-mod-menu/templates/ywt_menu_current/
├── classes.dex        590,072 B  md5 4979e004a581137c5a83466b8834d80f
└── build_info.txt
    source_commit   : 45c80c643bfd
    built_at        : 2026-11-25 22:25:47 +0800
    gradle_task     : assembleRelease
    sha256          : 75cb3c1569d96382745b92ff74e48198fc417f2d3c07abb51735f46f42f61ea0
    source_tree_cmd : cd mod菜单/starry-mod-menu/app/src/main && tar -cf - --exclude .DS_Store java | shasum -a 256
```

- 字段格式沿既有模板（`wxsj_menu_current` / `kyzt_menu_current` 等）用 `key : value`：`menu_tpl_gate.sh` 取
  `grep '^source_commit' | awk '{print $NF}'`，写成 `source_commit=<hash>` 时 `$NF` 会得到整串 `source_commit=<hash>`、
  前 12 位变成 `source_commi` ⇒ 假 FAIL（本轮首次就是踩这个）。
- 中间产物：`13_scratch/鹦鹉团/menu_new/{classes.dex, classes_old.dex}`。

## 8 复现命令

```bash
# 1) 取新 dex
unzip -p mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk classes.dex > /tmp/new.dex
# 2) 取旧 dex（上一版成品包里的菜单 dex）
unzip -p 08_output/鹦鹉团/鹦鹉团_1.0.7_starry-full-20261125.apk classes14.dex > /tmp/old.dex
# 3) 字节断言
python3.11 -c "import sys;n=open('/tmp/new.dex','rb').read();o=open('/tmp/old.dex','rb').read();\
s='游戏停止使用，功能已关闭'.encode();t='游戏维护中，功能已关闭'.encode();\
print('new',s in n,t in n,'old',s in o,t in o)"
# 4) 门禁
bash 04_scripts/_common/menu_tpl_gate.sh --tpl mod菜单/starry-mod-menu/templates/ywt_menu_current --dex-name classes.dex
```

## 9 边界声明

未改 `app/src/main/java` 下任何 Java、未改 gradle 配置、未改 proguard/keep 规则；未打包游戏 APK、未签名游戏包、未装机；
未碰 `10_docs/终末地/**`；未 git 提交（模板目录含二进制 dex，提交与否由主代理决定）。
