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
- 新增 assets
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

1. `ndk-build`（`Android.mk` 已切到本游戏）
2. 覆盖 `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so`
3. `strings` 自检本游戏标签（如 `[Gjc2]`），禁止上一款残留

否则成品菜单/鉴权会错成上一款游戏。**不影响** OTA 热更链路（OTA 另下 so）。

### 只换 so

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

---

## StarryApp 继承

`StarryApp` 必须继承官方 Application 类（如 `com/bilibili/priconne/App;`），否则权限链崩。
