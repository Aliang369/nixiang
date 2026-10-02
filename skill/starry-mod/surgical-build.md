# 外科组装规则

> 📍 `skill/starry-mod/surgical-build.md`

---

## 核心原则

**官方受保护文件字节级保留，只增不改。**

游戏 APK 禁止 `apktool b` 整包重建——TSS/ACE 校验会 SIGBUS 自杀。

---

## 受保护不可动

- `classes.dex` ~ `classesN.dex`（特定号，如 classes4=ACE、classes8=TSS）
- `resources.arsc`（尾部追加都拒绝，header.size 自洽）
- 特定 `lib*.so`（如 `libtprt.so`、`libtersafe2.so`）

## 可动可增

- 新增 `classes{N+1}.dex`（放星光 smali，用 smali-2.5.2.jar 单编）
- 新增 `libstarry_magic.so`
- 新增 assets（`assets/certs/starlight.crt` 自 2026-09-06 起烘焙进 so，新游戏**可省**；旧脚本多拷无害，代码已不读）
- `AndroidManifest.xml`（可 DEFLATED）

---

## 打包细节

1. `resources.arsc` 必须 **STORED**（zip 默认 DEFLATE 会被系统 AfkAssets 拒）
2. `resources.arsc` 的 header.size 必须自洽（= 文件大小）
3. `>2G` 包注意 apksigner 32 位溢出，须 <2G
4. `AndroidManifest.xml` 可 DEFLATED
5. 菜单去资源化（arsc 不能动）：MagicRes.java 代码生成 drawable/color；PNG 走 assets

---

## 构建脚本

按游戏手写 `surgical_{游戏}_{版本}.sh`，参考已有：
- `scripts/surgical_pcr_11.7.2.sh`
- `scripts/surgical_gyws_1.0.2.sh`
- `scripts/surgical_gjc2_1.1.8.sh`
- `scripts/surgical_mwdl_14.0.0.sh`
- `scripts/surgical_wzym_6.03.sh`

### 外科脚本前置（与 `starry-mod/SKILL.md` 一致）

Gradle **不**编 native。跑 `assemble*` / 抽小模板 so 之前必须：

1. `ndk-build`——modern 游戏：`APP_BUILD_SCRIPT=./Android_{tag}.mk NDK_OUT=<starry根>/obj_{tag} NDK_LIBS_OUT=<starry根>/libs_{tag}`（私有 mk + 独立 obj；**必须绝对路径**，相对路径被 `-C` 解析到 app/ 下 → 校验到旧 so 假成功，2026-09-10 事故）
2. 覆盖 `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`
3. 标签自检：英文标签可 `strings`（如 `[Gjc2]`）；中文菜单串必须 `LC_ALL=C grep -ac '菜单名' <so>` 按字节搜（strings 默认 ASCII 不认 UTF-8 中文，会漏报）。禁止上一款残留。
   **门禁串三条铁律（2026-10-01 失落城堡踩坑）**：
   - **必须取二进制里真实存在的字符串字面量**。静态 C++ 函数名会被内联/优化掉、名字不进 `.rodata`，拿它当门禁串会报「不含 hook 点」而源码其实已编译成功。失落城堡曾把 `SetLicInt`（静态函数名）写成门禁串导致误报，换成源码里真出现过的 `"set_PrivilegeLevel"` 才对。
   - **大小写与源码字面量逐字符一致**：`op_Implicit` 不能写成 `Op_Implicit`（il2cpp 反射名以小写开头）。
   - **串要跟着功能一起增删**：功能整块被删掉（如某开关撤销）时门禁串必须同步删，否则门禁变成永久失败的死锁。
4. 出包后闭环：包内 so md5 == 构建产物 md5（`unzip -p 成品.apk lib/arm64-v8a/libstarry_magic.so | md5 -q` 对比 `md5 -q libs_{tag}/arm64-v8a/libstarry_magic.so`），不一致=旧 so 混包，禁止交付

5. **出包/外科脚本写完先跑 bash 3.2 机检（2026-10-02 消消英雄2 定）**：
   `bash 04_scripts/_common/bash32_check.sh <脚本.sh> [...]`（退出码 0=通过，1=有 ✗ 硬错误）。
   本仓所有 `04_scripts/**`、`mod菜单/**/scripts/**` 都在 macOS 自带 bash 3.2 下跑，
   新语法不是"降级"而是整套脚本起不来；三类坑：`$VAR` 紧跟非 ASCII（中文被并进变量名 → unbound）、
   bash 4+ 语法（`mapfile`/`declare -A`/`${v^^}`）、`set -u` 下裸空数组展开。
   **不要靠首跑来发现**——2026-10-02 曾把这三条全部用首跑踩了一遍。

否则成品菜单/鉴权会错成上一款游戏。**不影响** OTA 热更链路（OTA 另下 so）。

> **沙箱/权限前置（2026-09-20 实测）**：第 4 步的 gradle 出包要写家目录缓存 `~/.gradle`；
> 只允许写工作区的沙箱（DSH file policy=`workspace-write`）下必然失败于
> `…/.gradle/wrapper/dists/gradle-8.13-bin/…zip.lck (Operation not permitted)`。
> 构建脚本**一开始就带一次性放宽权限运行**，不要先撞一次再重跑（重跑还要重编 so）。

### 证书固定来源（2026-09-06 起）

`AuthClient` 的 HTTPS 证书固定公钥**编译期烘焙在 `libstarry_magic.so`**（`jni/CertPinned.h`，经 `ModBridge.nativeGetPinnedCert` 桥接读取），不再读 `assets/certs/starlight.crt`；native 未加载时 fail-closed 拒发 HTTPS，不闪退。**换证书必须重编 so**（`xxd -i ../assets/certs/starlight.crt > CertPinned.h`，见头文件注释），只换 dex/assets 无效。装机前自检可加：`unzip -p 成品.apk lib/arm64-v8a/libstarry_magic.so | grep -a -c "BEGIN CERTIFICATE"` 应 ≥1。

### 只换 so

外科底包若是用户最终包，先生成独立工作副本，不默认覆盖原文件。

有外科底包后，只改 `Games/*Mod.cpp` → `*_so_only.sh` 重编重打（脚本须同步 jniLibs）。

---

## 签名

```bash
java -jar 01_tools/uber-apk-signer.jar -a <apk>
# 内部自动 zipalign
# 或直接用 SDK build-tools：
~/Library/Android/sdk/build-tools/35.0.0/zipalign
~/Library/Android/sdk/build-tools/35.0.0/apksigner
```

---

## 装机

```bash
# 覆盖安装（保留数据），禁止卸载大包
adb install -r -d <signed.apk>
```

装机后校验（2026-09-10 起强制，防"装机成功"装了旧包）：

```bash
bash 04_scripts/_common/verify_installed_apk.sh <serial> <包名> <本地APK> [--su]
# 设备 base.apk md5 != 本地产物 → FAIL；root 真机加 --su
```

---

## StarryApp 继承

`StarryApp` 必须继承官方 Application 类（如 `com/bilibili/priconne/App;`），否则权限链崩。

## 收尾与最小复现

保留完整构建入口及其源码、脚本和必要输入，不能只留依赖旧包的 so-only 入口。操作细则见 [TOOLBOOK.md](../../TOOLBOOK.md)「清理规则」「真机残留清理」，记录使用 [游戏收尾与最小复现清单](../../10_docs/_templates/游戏收尾与最小复现清单.md)。现有设备清理脚本未改、不默认运行；构建证据不替代运行或交付证据。
