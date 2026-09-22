# 打包注入流程

> **版本**：1.3  
> **日期**：2026-08-08（**上线默认 release / R8**）

## ⚠ 上线必读：用 release，不要用 debug

| | 命令 | 说明 |
|--|------|------|
| **上线 / 给玩家（当前：动物餐厅）** | `./scripts/inject_dwct_12.37.sh` | **默认 `assembleRelease`**（R8；**类名保持 `com.starry.magic.*`**） |
| 战魂铭人上线 | `./scripts/inject_zhmr.sh` | 同样默认 release |
| 仅本地联调 | `STARRY_BUILD_TYPE=debug ./scripts/inject_dwct_12.37.sh` | 无 R8 |

> 注入只拷 `com/starry/**` smali。ProGuard 禁止把菜单类改名成 `d.b` 这类短名，否则游戏会闪退（`ClassNotFoundException`）。规则见 `app/proguard-rules.pro`。

发版顺序：**先更新验证服务器 → 再跑 inject（release）→ 玩家重装/更新后重新登录**。  
详见 [GRANT_AUTH.md](auth-server/docs/GRANT_AUTH.md)、[SECURITY.md](auth-server/docs/SECURITY.md)。

## 〇、一键脚本（推荐）

| 目标 | 脚本 | 说明 |
|------|------|------|
| 动物餐厅 12.37 | `scripts/inject_dwct_12.37.sh` | 编译 → 拷 so/assets/smali → 签名绕过 + 广告钩 → 签名安装 |
| 战魂铭人 | `scripts/inject_zhmr.sh` | **默认 release**；Unity 注入流程 |

动物餐厅免内购 jsc 门控另见 `scripts/patch_dwct_iap_gate.js`（改游戏 `index.jsc`，不是塞进 so）。

### 进包什么 / 不进什么

| 进游戏 APK | 不进 |
|------------|------|
| `libstarry_magic.so`（**仅** `Android.mk` 当前源文件） | 未编进 mk 的 `ZhmrMod` / `Il2CppApi` 等 |
| 自测包（SELF） | **私有 `Android_self_test.mk`**（`STARRY_GAME_SELF` + `Main.cpp`），ndk-build 时 `APP_BUILD_SCRIPT=./Android_self_test.mk` |

> ⚠ **并行会话竞争（2026-09-06）**：共享 `Android.mk` 与 `jniLibs/libstarry_magic.so` 会被其他游戏会话（如山海幻境）切换/覆盖。
> 自测构建：重编 so 后**立即** gradle 打包；打完核对 `strings so | grep starry_self`。共享 mk 不要动。
| `com.starry.magic.*` smali（原生菜单，**无** `assets/web`） | `*.md`、`scripts/`、旧 HTML 设计稿 |
| `assets/certs/`（若脚本需要钉扎证书） | 其它游戏的 apktool 工程残留 |

仓库可多存几套 `Games/*.cpp`，**只要 mk 互斥，so 体积不会随样板数量线性涨**。

---

## 一、环境准备

### 1.1 开发工具

| 工具 | 用途 | 下载 |
|------|------|------|
| Android Studio | 编译 APK | developer.android.com/studio |
| NDK r25+ | 编译 C/C++ | 通过 SDK Manager 安装 |
| apktool | 反编译/重打包 APK | ibotpeaches.github.io/Apktool |
| apksigner | APK 签名 | Android SDK build-tools |
| zipalign | APK 对齐 | Android SDK build-tools |

### 1.2 环境变量

确保以下工具在 PATH 中：
```bash
# 检查
adb version          # Android Debug Bridge
ndk-build --version  # NDK
apktool --version    # apktool
apksigner --version  # 签名工具
```

---

## 二、编译

### 2.1 编译完整 APK

```bash
cd starry-mod-menu
./gradlew assembleRelease   # 上线 / 注入用（R8）
# ./gradlew assembleDebug   # 仅调试菜单壳
```

输出：`app/build/outputs/apk/release/app-release.apk`（或 `debug/app-debug.apk`）  
日常请直接跑 `./scripts/inject_zhmr.sh`，脚本已默认 release。

### 2.2 只编译 .so（给注入用）

```bash
cd app/src/main/jni
ndk-build
```

输出：`libs/arm64-v8a/libstarry_magic.so`

### 2.3 编译参数

| 参数 | 值 | 说明 |
|------|-----|------|
| ABI | arm64-v8a | 只编译 64 位 ARM |
| minSdk | 21 | Android 5.0+ |
| targetSdk | 35 | Android 15 |
| compileSdk | 35 | |
| C++ 标准 | c++17 | |
| STL | c++_shared | |

---

## 三、注入游戏 APK（完整流程）

### 3.1 反编译游戏 APK

```bash
apktool d game.apk -o game_src
```

反编译后目录结构：
```
game_src/
├── AndroidManifest.xml
├── apktool.yml
├── res/               ← 资源文件
├── smali/             ← Java 字节码（smali 格式）
├── smali_classes2/    ← 多 dex 时会有多个
├── lib/
│   └── arm64-v8a/     ← 游戏 .so 文件
└── assets/            ← 游戏 assets
```

### 3.2 放入 .so 文件

把编译好的 `libstarry_magic.so` 放到：
```
game_src/lib/arm64-v8a/libstarry_magic.so
```

### 3.3 注入前清理旧 HTML 资源

菜单 UI 已全原生（2026-08-13 起），无 HTML 资源；若旧包残留以下内容需删除：
```
game_src/assets/web/   ← 整目录删除（旧 mod-menu-v2.html + images）
```

### 3.4 放入 smali 文件

把 Java 类编译后的 smali 放到游戏目录：
```
game_src/smali/com/starry/magic/
├── app/
│   └── MainActivity.smali
├── bridge/
│   └── ModBridge.smali
├── menu/
│   ├── MenuService.smali
│   ├── NativeMenuController.smali
│   ├── MagicRes.smali
│   └── Preferences.smali
└── cocos/                    ← Cocos 游戏用（DwctHooks / CocosJsBridge）
    ├── DwctHooks.smali
    └── CocosJsBridge.smali
```

**获取 smali 的方法**：
1. 先编译 APK：`./gradlew assembleDebug`
2. 反编译自己的 APK：`apktool d app-debug.apk -o menu_smali`
3. 从 `menu_smali/smali/com/starry/magic/` 复制

### 3.5 修改 AndroidManifest.xml

在游戏 Manifest 中添加：

**权限**（加在 `<manifest>` 标签内、`<application>` 之前）：
```xml
<uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>
<uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>
<uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>
```

**Service 声明**（加在 `<application>` 标签内，Android 14+ 需指定 `foregroundServiceType` 和 `property`）：
```xml
<service
    android:name="com.starry.magic.menu.MenuService"
    android:enabled="true"
    android:exported="false"
    android:foregroundServiceType="specialUse"
    android:stopWithTask="true">
    <property
        android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
        android:value="MOD 菜单悬浮窗服务" />
</service>
```

### 3.6 修改游戏 Activity 的 smali

找到游戏的主 Activity（通常在 Manifest 中带 `LAUNCHER` 的那个），打开对应的 smali 文件，在 `onCreate` 方法开头插入：

```smali
# 加载 native 库
const-string v0, "starry_magic"
invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

# 启动菜单服务
invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
```

**插入位置示例**：
```smali
.method public onCreate(Landroid/os/Bundle;)V
    .locals 1

    # ===== 插入这里 =====
    const-string v0, "starry_magic"
    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
    # ===== 插入结束 =====

    invoke-super {p0, p1}, Landroid/app/Activity;->onCreate(Landroid/os/Bundle;)V
    # ... 游戏原来的代码
```

**另一种方式**：不修改游戏 Activity，而是修改 Manifest 把启动 Activity 改成 `com.starry.magic.app.MainActivity`，然后在 MainActivity.smali 中设 `gameActivity` 字段为游戏原 Activity。

### 3.7 重打包

```bash
apktool b game_src -o game_mod.apk
```

如果报错资源相关，试加 `--use-aapt2`：
```bash
apktool b game_src -o game_mod.apk --use-aapt2
```

### 3.8 对齐

```bash
zipalign -v -p 4 game_mod.apk game_mod_aligned.apk
```

### 3.9 签名

**生成签名密钥（只需一次）**：
```bash
keytool -genkey -v -keystore my.keystore -alias mykey \
    -keyalg RSA -keysize 2048 -validity 10000
```

**签名 APK**：
```bash
apksigner sign --ks my.keystore --ks-key-alias mykey \
    --out game_mod_signed.apk \
    game_mod_aligned.apk
```

**验证签名**：
```bash
apksigner verify --verbose game_mod_signed.apk
```

### 3.10 安装测试

```bash
adb install game_mod_signed.apk
```

打开游戏，如果悬浮窗权限已授予，星光魔法菜单会自动弹出。

---

## 四、常见问题

### 4.1 apktool 反编译失败

- 尝试升级 apktool 到最新版
- 加 `--no-res` 跳过资源反编译（只改 smali 时）
- 游戏可能有反编译保护（加壳），需先脱壳

### 4.2 重打包后闪退

- 检查 smali 插入位置是否正确
- 检查 `libstarry_magic.so` 是否在 `lib/arm64-v8a/` 下
- 检查 HTML 资源是否在 `assets/web/` 下
- 查看 logcat：`adb logcat -s StarryMagic:*`

### 4.3 悬浮窗不显示

- 检查是否授予悬浮窗权限（设置 → 应用 → 权限）
- Android 6.0+ 必须运行时申请
- 查看 logcat：`adb logcat -s MenuService:*`

### 4.4 菜单白屏 / 不显示（原生）

- 查看 logcat：`adb logcat -s NativeMenu:* MenuService:* ModBridge:*`
- 确认 `auth game_id synced from native:` 与后台 `games.game_id` 一致
- 确认 `bootstrap fetch` 返回 200（鉴权服务器可达）
- 悬浮窗模式检查 `Settings.canDrawOverlays` 权限

### 4.5 .so 加载失败

- 确认目标设备是 arm64（`adb shell getprop ro.product.cpu.abi`）
- 检查 `libstarry_magic.so` 架构匹配
- 查看 logcat：`adb logcat -s StarryMagic:* | grep -i load`

---

## 五、快速注入脚本

把以下内容保存为 `inject.sh`：

```bash
#!/bin/bash
# 用法: ./inject.sh game.apk
set -e

GAME_APK="$1"
GAME_SRC="game_src"
MOD_APK="game_mod.apk"
ALIGNED_APK="game_mod_aligned.apk"
FINAL_APK="game_mod_signed.apk"
KEYSTORE="my.keystore"

# 检查参数
if [ -z "$GAME_APK" ]; then
    echo "用法: ./inject.sh game.apk"
    exit 1
fi

echo "=== 1. 反编译 ==="
apktool d "$GAME_APK" -o "$GAME_SRC" -f

echo "=== 2. 复制 .so ==="
mkdir -p "$GAME_SRC/lib/arm64-v8a/"
cp app/src/main/jni/libs/arm64-v8a/libstarry_magic.so "$GAME_SRC/lib/arm64-v8a/"

echo "=== 3. 复制 HTML 资源 ==="
mkdir -p "$GAME_SRC/assets/web/pages/"
mkdir -p "$GAME_SRC/assets/web/images/"
cp app/src/main/assets/web/pages/*.html "$GAME_SRC/assets/web/pages/"
cp app/src/main/assets/web/images/*.jpg "$GAME_SRC/assets/web/images/"

echo "=== 4. 复制 smali ==="
# 先编译自己的 APK 获取 smali
./gradlew assembleDebug
apktool d app/build/outputs/apk/debug/app-debug.apk -o menu_smali -f
cp -r menu_smali/smali/com/starry/magic "$GAME_SRC/smali/com/starry/"

echo "=== 5. 重打包 ==="
apktool b "$GAME_SRC" -o "$MOD_APK"

echo "=== 6. 对齐 ==="
zipalign -v -p 4 "$MOD_APK" "$ALIGNED_APK"

echo "=== 7. 签名 ==="
if [ ! -f "$KEYSTORE" ]; then
    echo "生成签名密钥..."
    keytool -genkey -v -keystore "$KEYSTORE" -alias mykey \
        -keyalg RSA -keysize 2048 -validity 10000 \
        -storepass 123456 -keypass 123456 \
        -dname "CN=StarryMagic, OU=Mod, O=Starry, L=CN, ST=CN, C=CN"
fi
apksigner sign --ks "$KEYSTORE" --ks-key-alias mykey \
    --ks-pass pass:123456 --key-pass pass:123456 \
    --out "$FINAL_APK" "$ALIGNED_APK"

echo "=== 完成 ==="
echo "输出: $FINAL_APK"
echo "安装: adb install $FINAL_APK"
```

**注意**：此脚本不会修改游戏 Manifest 和 smali，这两步需要手动完成（步骤 3.5 和 3.6）。

---

## 六、已有 .so 集成

如果你有已编译好的作弊 .so（如之前 LGL 的 `libjzgame.so`）：

1. 放到 `app/src/main/jniLibs/arm64-v8a/libxxx.so`
2. 在 `Android.mk` 添加预构建声明：
   ```makefile
   include $(CLEAR_VARS)
   LOCAL_MODULE := xxx
   LOCAL_SRC_FILES := ../jniLibs/arm64-v8a/libxxx.so
   include $(PREBUILT_SHARED_LIBRARY)
   ```
3. 在 `Main.cpp` 中 `dlopen` 调用其函数，或直接合并代码到 Main.cpp（⚠️ Main.cpp 为 legacy 入口，新游戏在 `Games/{Game}Entry.cpp` 注册表或自身 Mod 内处理，勿改 Main.cpp）
