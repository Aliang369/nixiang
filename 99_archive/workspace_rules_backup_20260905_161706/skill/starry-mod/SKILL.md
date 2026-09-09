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

## 切游戏（单一入口）

**只改 `app/src/main/jni/Android.mk` 一个 `STARRY_GAME_*`**（+ `ENABLE_*_HOOK` + 对应 `Games/*Mod.cpp`）

```makefile
LOCAL_CFLAGS += -DENABLE_UNITY_HOOK -DSTARRY_GAME_{GAME}
LOCAL_SRC_FILES := Main.cpp Games/XxxMod.cpp Unity/Il2CppApi.cpp   # 按引擎增减
```

`STARRY_AUTH_GAME_ID` 由 `GameConfig.h` 按 `STARRY_GAME_*` **自动映射**，**禁止**：
- 在 mk 里写 `-DSTARRY_AUTH_GAME_ID`
- 在 Java 里写死默认游戏名
- 删掉 `#error` 保护

新游戏的 `Games/*Mod.cpp` 用生成器产出（防闪退骨架 v2，内置懒装/双路径挂点/SafeWrite，勿从旧游戏复制）：

```bash
bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
```

### 硬规则：jniLibs 才是 Gradle 打包的 so（2026-08-17 血泪）

`app/build.gradle` 已 **关闭 `externalNativeBuild`**。`./gradlew assemble*` **不会**按 `Android.mk` 重编 native，只打包：

`app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`

| 步骤 | 必须做 |
|------|--------|
| 1 | 改 `Android.mk` 切到目标 `STARRY_GAME_*` |
| 2 | `ndk-build`（产出常在 `app/src/libs/arm64-v8a/`） |
| 3 | **拷贝覆盖** → `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`（建议同步 `app/src/main/libs/`、`app/libs/`） |
| 4 | 再 `gradlew` / 跑 `surgical_*.sh` / `*_so_only.sh` |
| 5 | **装机前自检**：`strings` 成品 APK（或 jniLibs）里的 so，须含本游戏标签（如 `[Gjc2]` / `gjc2`），**禁止**仍出现上一款（如 `[Dwct]`） |

**漏做第 3 步 = 菜单/鉴权仍是上一款游戏**（弓箭传说2 打出动物餐厅菜单即此坑；切游戏后 gradle 十几秒「成功」往往是旧 so）。

`*_so_only.sh` 直接 `ndk-build` + `zip -u` 成品包时，仍应同步刷新 jniLibs，避免下次外科/gradle 再打错。

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

以 `GameConfig.h` / `SWITCH_GAME.md` 为准；上表落后时以头文件为准并回写本表（2026-09-03 已按 GameConfig.h 回写至 33 款）。

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

优先用各游戏的 `scripts/surgical_*.sh` / `*_so_only.sh`（脚本内应已含拷贝与自检）。

---

## 过检通用清单

| 项 | 方法 |
|----|------|
| 签名校验 | `SignSpoof`（星光内置，Activity onCreate 最早替换 PackageInfo.CREATOR） |
| TapTap 登录 | hook `ClientLoginUtils.isClientLogin` → 恒 false（或旧版 `ClientLoginHandler.canHandle`）；并 patch `TapSignKit.getSign` → 原包 cert MD5 |
| 完整性黑名单 | 确保注入文件不在黑名单路径中 |
| Root | RootBeer / KSU 隐藏 |

---

## 分发与装机红线（强制，适用于所有游戏）

> 来源：2026-08-28 无悔华夏分发事故（`10_docs/无悔华夏/闪退修复_20260827.md`）。规则对所有游戏生效，不限于该游戏。

### 分发自检：文档说修了 ≠ 用户拿到的包修了

「源码已修 + 本机验证」不等于用户手里的包已修。**任何游戏**对外分发前逐条执行：

1. 正式包必须**本次重打**进 `08_output/{游戏}/`；trace 包、旧包、只换 so 未刷 dex 的包一律不得当正式分发
2. 成品 APK 内 so 必须是本游戏 + 本版本：`unzip -p 成品.apk lib/arm64-v8a/libstarry_magic.so | strings | grep '<本游戏标签>'`
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
- 游戏内贴图变紫/洋红 = 热更 DLC 资源校验失败（与 Mod hook 无关）：只删该游戏的热更资源目录重新下载（目录名因游戏而异），**别清数据**

---

## 设备指纹

```bash
# 装机后必须跑
bash 04_scripts/_common/ensure_starry_device_id_seed.sh
```

改指纹相关代码**必须重打含菜单 DEX 的包**，只换 so 无效。

详见 `auth-chain.md`

---

## 验证

```bash
adb logcat -s StarryApp:V StarryMagic:V AuthClient DeviceFingerprint | grep -E \
  'auth game_id|native menu shown|source=shared'
```

逐项检查：菜单弹出且**功能名对本游戏**、每个功能有效、关闭后恢复、登录正常、对局结束不闪退。

---

## 收尾

```bash
# 真机残留清理（强制）
./04_scripts/_common/cleanup_device_re_residue.sh
```
