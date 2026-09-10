# 切游戏规范（game_id 单一入口）

## ⚠️ 2026-09-09 起：新游戏走 modern 注册表（双轨制）

**接新游戏禁止再改 `Main.cpp` / `GameConfig.h` / 公共 `Android.mk`。** 标准流程：

```bash
# 1. 生成四件套（Mod 骨架 + 注册表 Entry + 私有 mk）
bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh <Game> [日志标签]

# 2. 填 {Game}Entry.cpp 的 TODO（authGameId / waitLib / featureList）
#    填 {Game}Mod.cpp 的 TODO（偏移/挂点/功能组）

# 3. 构建（独立 obj，并行会话互不踩，切游戏无须清共享 obj）
#    NDK_OUT/LIBS_OUT 必须绝对路径：相对路径被 ndk-build -C 解析到 app/ 下 → 校验到旧 so 假成功（2026-09-10 事故）
cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
  APP_BUILD_SCRIPT=./Android_<tag>.mk NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT=<starry根>/obj_<tag> NDK_LIBS_OUT=<starry根>/libs_<tag>

# 4. 注册管线出包（build_so 后端照 04_scripts/魂坠深境/hzsj_build_so.sh 范本，
#    install 末尾接 _common/verify_installed_apk.sh；详见 04_scripts/_common/stages.md）
```

| 双轨 | 入口 | 谁用 |
|------|------|------|
| modern（注册表） | `MainModern.cpp` + `Games/{Game}Entry.cpp` + 私有 `Android_<tag>.mk` | **新游戏一律走这里** |
| legacy（#if 堆叠） | `Main.cpp` + 公共 `Android.mk` 激活段 | 存量 20+ 游戏；重新出包时顺手迁移（写 Entry + 切 mk + 删该游戏 #if 分支组），迁完即减堆积 |

铁律：`Main.cpp` 严禁任何改动（含注释）——`OBFUSCATE` 宏用 `__LINE__` 做混淆种子，行号偏移即产物漂移（20260909 实测注释 +12 行 md5 变）。

---

## 你只改一处（⚠️ 以下为 legacy 存量路线，新游戏勿用）

**`app/src/main/jni/Android.mk`**：启用且仅启用一个 `STARRY_GAME_*`（正式游戏再加 `ENABLE_*_HOOK` + 对应 `Games/*Mod.cpp`）。

```make
# 自测
LOCAL_CFLAGS += -DSTARRY_GAME_SELF

# 或正式，例如霓虹：
# LOCAL_CFLAGS += -DENABLE_UNITY_HOOK -DSTARRY_GAME_NEON
# LOCAL_CFLAGS += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GYWS
# LOCAL_SRC_FILES += Games/NeonMod.cpp …
```

## 其它地方怎么「看到」

| 消费者 | 来源 |
|--------|------|
| 鉴权字符串 `STARRY_AUTH_GAME_ID` | **`GameConfig.h` 按 `STARRY_GAME_*` 自动映射**（勿在 mk 再 `-D`） |
| Native 功能表 / Tick | `#if defined(STARRY_GAME_*)` |
| Java prefs / 登录 / 续票 | `ModBridge.syncAuthGameIdFromNative()` ← so |
| 后台 `games.game_id` / 单游戏卡 | **须与映射表同名**（运营侧，无法编译期代写） |

### 映射表（改 id 字符串只动 `GameConfig.h`）

| `STARRY_GAME_*` | `game_id` |
|-----------------|-----------|
| `SELF` | `starry_self` |
| `NEON` | `neon` |
| `CSTJJS` | `cstjjs` |
| `ZJMC` | `zjmc` |
| `HZSJ` | `hzsj` |
| `MILTHM` | `milthm` |
| `YWGN` | `ywgn` |
| `WZYM` | `wzym` |
| `MATCH2` | `xxh2` |
| `PCR` | `pcr` |
| `GYWS` | `gyws` |
| `ZHMR` | `zhmr` |
| `ZXR` | `zxr` |
| `ZMXY` | `zmxy` |
| `LTZJ` | `ltzj` |
| `MWDL` | `mwdl` |
| `ZMWS` | `zmws` |
| `DWCT` | `dwct` |
| `GJC2` | `gjc2` |
| `GJC2INTL` | `gjc2intl` |
| `TLGD` | `tlgd` |
| `SHLT` | `shlt` |
| `DZQS` | `dzqs` |
| `SHHJ` | `shhj` |
| `HALY` | `haly` |
| `WGGLS` | `wggls` |
| `BJDW` | `bjdw`（存量；新游戏应全名首字母，见 `.cursor/rules/starry-mod-game-id.mdc`） |

### game_id 命名（2026-08-27 起 · 新游戏）

- **全名拼音首字母**：如 伯吉的温馨厨房 → `bjdwxcf`（非 `bjdw`）
- 虚词「的/之」计入；数字原样保留
- 已上线 id **不改**；`STARRY_GAME_*` 宏可仍用短名，但 `GameConfig.h` 里 `STARRY_AUTH_GAME_ID` 必须全名 id

## 禁止再做的事

- 在 `Android.mk` 写 `-DSTARRY_AUTH_GAME_ID=...`（会编译失败）
- 在 Java 写死默认游戏名（已删除 `DEFAULT_AUTH_GAME_ID`）
- 只改 prefs / 后台、不改 mk 的 `STARRY_GAME_*`

## Checklist

- [ ] `Android.mk` 只开一个 `STARRY_GAME_*`（+ hook / src）
- [ ] **`ndk-build` 后把 so 拷进 `app/src/main/jniLibs/arm64-v8a/`**（Gradle **不**编 native，只打包 jniLibs；漏拷 = 菜单仍是上一款游戏）
- [ ] 产物 so 自检：`strings libstarry_magic.so | grep '\[Xxx\]'` 必须是本游戏标签
- [ ] 后台已有同名 `game_id`；单游戏卡绑对该游戏（或用全游戏卡）
- [ ] 注入脚本是本游戏的
- [ ] 装机 log：`auth game_id synced from native: <期望id>`

## 构建选哪条（与 WORKFLOW 阶段 6b 一致）

| 改动 | 命令 |
|------|------|
| 只改 `Games/*Mod.cpp` | `STARRY_INSTALL=1 bash scripts/{游戏}_so_only.sh`（有则用之） |
| 改菜单 / smali / 正式发布 | `STARRY_INSTALL=1 bash scripts/inject_{游戏}_{版本}.sh` / `surgical_*.sh` |

**血泪：** `externalNativeBuild` 已关。只改 `Android.mk` 再 `assembleRelease` **不会**重编 so；`jniLibs` 里仍是旧游戏 → 菜单/鉴权全错。切游戏必须 `ndk-build` → 覆盖 `jniLibs` → 再 gradle/外科。

整仓说明：工作区根目录 `WORKFLOW.md` →「阶段 6b: 星光菜单注入」。
