# 勇士之心 1.0.6 killer（签名伪装 B 轨）方案与产物

> 生成：2026-11-23 签名伪装适配子代理。**未装机、未组装 APK、未 git 提交、未改 framework 共享文件、未碰 `04_scripts/勇士之心/ysxz_anticheat/**`**。
> 授权口径：用户本轮放行恢复"签名伪造件"，**仅用于让本包能启动**，不用于骗 TapTap 登录。
> 输入底包：`02_apks/勇士之心_1.0.6_official.apk`（65,531,496 B / md5 `c06a1c1e3cc4f57e4d9ab29ecd9becfa` / vc6 / 纯 v2 签名）。
> 前序文档：`_work_20261123_壳可行性.md`（为什么必须签名源：E5/E6）、`_work_20261123_施工图.md`（出包链）。

---

## 0. 一句话结论

**现成方案能直接套，而且不用改 framework、不用改 manifest**：勇士之心 manifest 的 `appComponentFactory` 已经是 `com.starry.magic.ysxz.YsxzAppComponentFactory`，它 `extends StarryAppComponentFactory`，父类 `instantiateApplication()` 里已经有 `mountSignatureKiller(cl)`（`StarryAppComponentFactory.java:80`）。**只要把 killer dex 装进包，它就会在 ~365ms 自动挂上**，不需要任何新挂载点。本轮唯一缺口是"包内没有 killer dex"，本子代理补齐了脚本 + smali 源 + 证书 + 产物。

---

## 1. 机理（这套 killer 到底改了什么）

MT ApkSignatureKillerEx 形态，13 个类（`bin/mt/signature/KillerApplication193` + `$a..$l`），入口是**静态初始化块**（`<clinit>`），没有任何 Application/Provider 生命周期依赖：

| 步 | 动作 | 位置（smali） | 效果 |
|---|---|---|---|
| 1 | 把 `PackageInfo.CREATOR` 换成包装类 `$l`（反射置静态字段，绕过 final） | `KillerApplication193.smali` `killPM()` | 之后**本进程**从 Binder 收到的 `PackageInfo` 在反序列化时走我们的 creator |
| 1b | API>=28 额外调 `VMRuntime.setHiddenApiExemptions("Landroid/os/Parcel;","Landroid/content/pm","Landroid/app")` | `$j.b([Ljava/lang/String;)Z` | 让下面几条私有字段反射可用（失败只打日志，不中断） |
| 2 | 清 `PackageManager.sPackageInfoCache` | `killPM()` | 丢掉进程内已缓存的（星空）PackageInfo，强制重新走 Binder |
| 3 | 清 `Parcel.mCreators` / `Parcel.sPairedCreators` | `killPM()` | 丢掉 Parcel 的 creator 缓存，保证第 1 步的新 creator 真的被用上 |
| 4 | `$l.createFromParcel()`：`packageName.equals(目标包名)` 时改写 `signatures[0]` 为官方证书 | `$l` | `GET_SIGNATURES`（API<28）路径被覆盖 |
| 4b | 同一分支里：`PackageInfo.signingInfo.getApkContentsSigners()[0]` 也改写 | `$l` + `$k`（`getApkContentsSigners()`） | `GET_SIGNING_CERTIFICATES`（API>=28）路径被覆盖 —— **正好是 libsecret 的首选路径**（壳可行性 E2） |

与壳的对应关系（为什么它能救启动）：

    改签 => PackageManager 回星空证书 => key = 常量48 XOR digest(星空证书) => key unwrap failed
      => 明文 libcocos2djs.so 不落盘 => refuse empty mlib => 起不来        （壳可行性 §2）
    killer => 本进程 PackageManager 回官方 DER => key 正确 => 解密成功 => 引擎加载

作用域是**本进程内 Java 层**的 `PackageManager` 查询：不改 APK 文件签名（磁带上仍是 `CN=StarryMagic`），不改 native，不碰 13 条必保条目。

> 注：`<clinit>` 里还有一句 `killOpen(pkg,"","","")`（解压包内条目后再 `System.loadLibrary` 的自愈路径）。三个路径参数传空 => 函数第 3 行就 `return-void`（`KillerApplication193.smali` `killOpen` 的 `p1.isEmpty()` 分支），**本作不需要、也无副作用**。

---

## 2. 挂载点（现状已就位，无需新增）

### 2.1 时序（沿用 1.0.4/1.0.6 实测时间轴）

| 时刻 | 事件 | 谁做的 |
|---|---|---|
| ~365ms | 框架调 `YsxzAppComponentFactory.instantiateApplication()` | AMS Instantiator（manifest `appComponentFactory`） |
| ~365ms | `startSecShellWatchdog()` -> `nativePrepareSecureShell()` | `YsxzAppComponentFactory.java:38` |
| **~365ms** | **`super.instantiateApplication()` -> `mountSignatureKiller(cl)` -> `Class.forName("bin.mt.signature.KillerApplication193", true, cl)`** | `YsxzAppComponentFactory.java:39` -> `StarryAppComponentFactory.java:80` -> `:43-53` |
| ~413~483ms | `libsecret.so` 自检（签名/完整性/root…） | 壳 |
| 之后 | 密钥派生（`0x4f188`）-> 解密 `assets/apksecurefile` / 引擎 so | 壳 |
| ~649ms | 真实 `WarriorApplication.attachBaseContext` / `onCreate` | 官方 Application |
| ~651ms | `StarryInitProvider.onCreate` | 星光 Provider |

**killer 早于壳自检 48~118ms 挂上**，这是本作能救回来的全部依据。

### 2.2 framework 侧（只读，不改）

| 文件:行 | 内容 | 结论 |
|---|---|---|
| `StarryAppComponentFactory.java:40` | `KILLER_CLASS = "bin.mt.signature.KillerApplication193"` | 与产物类名逐字一致（构建脚本已机器断言） |
| `StarryAppComponentFactory.java:43-53` | `mountSignatureKiller(ClassLoader)`，`Class.forName(..., true, cl)` + `catch (Throwable)` 静默跳过 | 包内无 killer 时零副作用 |
| `StarryAppComponentFactory.java:80` | `instantiateApplication()` 第一句就 `mountSignatureKiller(cl)` | **挂载点已存在** |
| `YsxzAppComponentFactory.java:36-40` | `instantiateApplication()`：先 native 看门狗，再 `super` | 继承链已通，无需改 |
| `SignatureKillerBootstrap.java` | Provider 阶段的第二入口（`sDone` 幂等） | 本作用不到（Factory 更早）；保留不动 |

### 2.3 manifest

**不需要改。** `surgical_ysxz_1.0.6.sh:138-148` 已经把 `android:appComponentFactory` 写成 `com.starry.magic.ysxz.YsxzAppComponentFactory`，并在 `L196` 断言它进了新 manifest。官方 `org.cocos2dx.javascript.WarriorApplication` 原样保留（`L197` 断言）——killer 不改 Application。

### 2.4 与 ysxz 现有组件是否冲突

| 组件 | 冲突 | 依据 |
|---|---|---|
| `YsxzAppComponentFactory`（SecShell 看门狗） | 无 | 挂载在 `super` 调用里，看门狗先跑，互不依赖 |
| `StarryInitProvider` / `MenuService` | 无 | killer 只动 `PackageInfo.CREATOR` 与三个缓存，不碰组件实例化 |
| 13 条必保条目 | 无 | 只**新增**一个 dex 文件，不改既有条目字节 |
| 菜单/授权/功能层 | 无 | 纯签名层，无 hook |
| 副作用（全局） | 轻微 | 清 `Parcel.mCreators`/`sPairedCreators` 会让本进程重建 creator 缓存（一次性开销）；仅对目标包名改写签名，其它包名走原 creator 逻辑 |

---

## 3. 所需文件（本轮已落地）

| 文件 | 作用 | 指纹 |
|---|---|---|
| `04_scripts/勇士之心/assets/ysxz_cert/ysxz_official_cert.der` | 官方证书 X.509 DER（877 B） | sha256 `de8a75d3…5083e0a`，md5 `79a6a62f…` |
| `…/ysxz_cert/ysxz_official_cert.b64(.wrapped)` | base64 / 每 64 字符换行 | 同源 |
| `…/ysxz_cert/extract_v2_cert.py` | 从 APK Signing Block(v2) 导 DER + 指纹断言（可复现） | 见 META.md |
| `…/ysxz_cert/META.md` | 来源命令 + 三路指纹对照 + 为什么 v2 块才取得到 | — |
| `04_scripts/勇士之心/assets/ysxz_killer_smali/mt/signature/*.smali` | 13 个 killer 类（参数化，含 `@@PKG@@` / `@@CERT_B64_WRAPPED@@` 占位符） | 与 sjzz/byyxy 同源逐字节一致（已 diff） |
| `04_scripts/勇士之心/build_killer_dex_ysxz.sh` | 烘焙包名+证书 -> assemble -> 回读断言 | `preflight` PASS |
| `04_scripts/勇士之心/verify_killer_dex_ysxz.py` | 离线校验 17 项（详见 §5） | `preflight` PASS |
| `08_output/勇士之心/killer/ysxz_signature_killer.dex` | **产物** | 13,988 B；sha256 `3fda698b…53ef4`；内容指纹 `a86dead8…4e2a5b` |

> 为什么 smali 源放 `assets/ysxz_killer_smali/mt/signature/` 而不是带 `bin/` 前缀：`.gitignore` 有 `04_scripts/**/bin/` 规则，带 `bin/` 会被整目录忽略（byyxy 脚本里记录了同一个坑）。构建脚本负责还原 `bin/` 结构。

---

## 4. 现场挂载方式 & 需要谁改哪几行

### 4.1 挂进 APK：新增 `classes3.dex`（推荐，改动最小）

官方包只有 `classes.dex`，星光已占 `classes2.dex`（`surgical_ysxz_1.0.6.sh:39`），**`classes3.dex` 空着**。ART 会把所有 `classesN.dex` 都放进类路径，所以 killer 独立成一个 dex 即可被 `Class.forName(..., cl)` 找到。

**`mod菜单/starry-mod-menu/scripts/surgical_ysxz_1.0.6.sh` 需要改 4 处（我不改文件，只给行号）：**

1. **L39 之后**新增一个变量（`NEW_DEX="classes2.dex"` 保持不动）：

```bash
KILLER_DEX="${YSXZ_KILLER_DEX:-$NIXIANG/08_output/$GAME/killer/ysxz_signature_killer.dex}"
```

2. **L100 之后**（`ls -la "$WORK/$NEW_DEX"` 下一行）新增「缺就构建 + 断言」：

```bash
[[ -f "$KILLER_DEX" ]] || bash "$NIXIANG/04_scripts/勇士之心/build_killer_dex_ysxz.sh"
[[ -f "$KILLER_DEX" ]] || { echo "ERROR: killer dex 缺失 $KILLER_DEX" >&2; exit 1; }
```

3. **L205** 传参多一个：`python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$BOOT_ENC" "$KILLER_DEX" <<'PY'`；
   对应 python 里在 `boot_enc = ...`（L209）后加 `killer = Path(sys.argv[6]).read_bytes()`，
   并在 `assert new_dex not in names`（L237）后加：

```python
assert b'bin/mt/signature/KillerApplication193' in killer, 'killer dex 不含目标类'
```

4. **L259 之后**（`zout.writestr(info, cnew); n += 1` 下一行）写第三个 dex：

```python
info = zipfile.ZipInfo("classes3.dex"); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, killer); n += 1
```

   并在 **L273**（`assert new_dex in out_names`）后补 `assert "classes3.dex" in out_names`，
   在 **L283** 附近补 `grep -q '^classes3.dex$' "$WORK/entries.txt"`。

**顺带一个已存在的阻断项（不属本方案，但会先炸）**：`surgical_ysxz_1.0.6.sh:94` 的必查类表里还留着 `SignSpoofAppComponentFactory`（该类 2026-10-28 已删）=> 星光的 `classes2.dex` 里没有它，脚本会在 L94-98 `exit 1`。改法：把 L94 的 `SignSpoofAppComponentFactory` 换成 `StarryAppComponentFactory`。

### 4.2 另一条可行路线（不推荐，留档）

把 killer 的 smali 与 `com/starry` 一起 assemble 进 `classes2.dex`（`SMALI_ROOT/bin/`）。代价：`classes2.dex` 变成"星光+killer 混合体"，killer 指纹无法独立核对，且需要把烘焙逻辑复制进 surgical -> 违背"一处烘焙"。**建议用 §4.1。**

### 4.3 不需要谁改什么

- 不需要改 `StarryApp*` / `StarryInitProvider` / `StarryAppComponentFactory` / `YsxzAppComponentFactory`（已就位）。
- 不需要改 manifest。
- 不需要改 native / so。

---

## 5. 验证结果（离线，全部本机实测）

| # | 检查 | 结果 |
|---|---|---|
| V1 | 证书 DER 双路取得一致（`apksigner verify --print-certs` + v2 签名块解析） | PASS，两端都是 `de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a`；DN `CN=shan, OU=bingo, O=bingo, L=guangzhou, ST=guangdong, C=CN`；DER 877 B |
| V2 | 与壳 `.data` 期望常量同一证书（壳可行性 E4） | PASS（同 sha256；md5 `79a6a62f…` 亦与旧 SignSpoof 日志同值） |
| V3 | 构建脚本 5 步全绿（含 framework 类名核对、证书指纹、包名烘焙、回读断言） | PASS |
| V4 | `dexdump -f` 结构合法 | PASS：`dex\n035`，`class_defs_size = 13`，file_size 13988 |
| V5 | `baksmali` 反查 13 类齐全（`bin/mt/signature/KillerApplication193{,a..l}`） | PASS |
| V6 | 产物里证书 base64 解码后 **与 DER 文件逐字节相同** | PASS（验证脚本第 8 条） |
| V7 | 包名烘焙 = `com.fightingstars.games.warrior`，无占位符残留 | PASS |
| V8 | hook 逻辑静态核对：换 CREATOR / 清 `sPackageInfoCache` / 清 `Parcel.mCreators`+`sPairedCreators` / `signatures[0]` 改写 / `signingInfo`+`getApkContentsSigners()` 改写 / 仅对目标包名生效 | PASS（6 条分开断言） |
| V9 | 规范关：产物不含 `Lcom/starry/magic/sign/SignSpoof;` | PASS |
| V10 | 离线校验脚本汇总 | **17 PASS / 0 FAIL** |
| V11 | `preflight.sh`（3 个新脚本） | PASS=6 FAIL=0 |

复现命令：

```bash
bash 04_scripts/勇士之心/build_killer_dex_ysxz.sh          # 烘焙+汇编+回读断言，产物进 08_output/勇士之心/killer/
python 04_scripts/勇士之心/verify_killer_dex_ysxz.py       # 17 项离线校验
python 04_scripts/勇士之心/assets/ysxz_cert/extract_v2_cert.py \
  --apk 02_apks/勇士之心_1.0.6_official.apk \
  --out-dir 04_scripts/勇士之心/assets/ysxz_cert \
  --expect-sha256 de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a
```

**一个必须写下来的观测**：同一份 smali + 同一证书，Windows 上连续构建 5 次得到 3 个不同 md5（`c60b7d03…` / `f39d2033…` / `5a912a98…`），体积恒为 13,988 B。所以**门禁不能用 dex md5**，脚本改用「烘焙后 smali 树排序摘要」= `YSXZ_KILLER_FP`（实测 5 次恒为 `a86dead85b5a724d3ef3c64099d24bd83af962b54ba55657be52db473b4e2a5b`）。与 `10_docs/暗影格斗3/星空落地_1.45.3_20260919.md:165` 记的"dex md5 非同源可复现"同一现象。

---

## 6. 风险

| # | 风险 | 等级 | 说明 / 处置 |
|---|---|---|---|
| R1 | `PackageInfo.CREATOR` 是 **static final**，`Field.set` 在 Android 14+ 可能抛 `IllegalAccessException` | 高 | 抛了 => `<clinit>` 失败 => `ExceptionInInitializerError` => mount 静默跳过 => **回到"起不来"**（不会更坏，但等于没修好）。同族包（宝石战争/正中靶心2/伊莫）在雷电模拟器上实测 `SignatureKiller mounted` PASS；**Android 14 user 真机未验证** |
| R2 | `VMRuntime.setHiddenApiExemptions` 在 Android 14+ 失败 | 中 | 失败只打 `HiddenApiBypass` W 日志并返回 false，第 2/3 步的私有字段反射会随之失败，但都被 catch 掉；CREATOR 交换本身不依赖它 |
| R3 | 时序：若某版本壳把签名读取提到 `instantiateApplication` 之前 | 低 | 壳自检在 413ms，比它更早的 Java 入口没有（~365ms 已是最早）。真机验证看 `[secure] runtime sig SHA-256=` 是否等于 `de8a75d3…` |
| R4 | 可以起包但 APK 完整性 bit9(0x200) 仍会置位 | 中 | 与现状一致（任何重打包都置位），`YsxzSecShell` 看门狗清状态字即可；**不属本层职责** |
| R5 | 出包链 L94 的 `SignSpoofAppComponentFactory` 必查项会先 `exit 1` | 高（阻断） | 见 §4.1 末段，一行改法 |
| R6 | dex md5 不可复现 | 低 | 用 `YSXZ_KILLER_FP` 做门禁（已内置默认值）；报告里别拿 md5 当唯一指纹 |
| R7 | 只覆盖 Java 层 `PackageManager` | 中 | 若壳改用"直接解析自己 APK 的签名块 / META-INF"就骗不到（壳可行性 E1 已确认本作走 Java PackageManager，所以够用） |

### 6.1 TapTap 登录是否会被这层影响 —— **明确结论：不会变好，也不会变坏**

1. **本层作用域只有本进程的 Java `PackageManager`**。TapTap 的"包名、签名未配置"由 TapTap 客户端/服务端判定，读的是 APK **文件**签名；我们进程改的只是自己查到的值。
2. **历史实测已给答案**：1.0.4/1.0.6 带 `SignSpoof` 的包（同一机制、更宽的实现）冷启动日志里仍有 `spoof hit md5=79a6a62f…`，而 TapTap 依旧报「包名、签名未正确配置」并打印星空指纹 `161fca32…`（`_work_20261123_壳可行性.md` §5.5）。换成 killer 不改变这条链路。
3. **唯一可能的边际变化**：若 TapSDK 在**本进程内**用 `pm.getPackageInfo(pkg, GET_SIGNATURES)` 自查，现在会拿到官方值 => 它可能不再本地报错、改为继续发登录请求（然后被服务端拒）。这属于"报错形态变化"，不是"登录成功"。
4. 因此登录仍按 A 轨口径（改包法 / 网页登录 / 短信登录），与 `AGENTS.md` §3.3「TapTap 渠道」条一致；killer 只为"引擎能解密、游戏能起"服务。

### 6.2 未验证（必须真机才能确认）

1. CREATOR 交换在目标设备/Android 版本是否成功（判据：`logcat -s StarryAppFactory` 出现 `SignatureKiller mounted: ...`）。
2. 壳实际读到的运行时签名是否变成官方：`logcat | grep "\[secure\] runtime sig SHA-256"`，期望 `de8a75d3…`。
3. `key unwrap failed` / `refuse empty mlib` 是否消失、明文 `libcocos2djs.so`（35,844,552 B）是否落到 `no_backup/mlib/arm64-v8a/`。
4. 装机后 `files/sigkiller.txt` 是否存在（`SignatureKillerBootstrap` 写的取证标记；本作主走 Factory 路径，该文件可能不生成——不算失败）。
5. Android 14+ 真机（雷电是 userdebug/较低 API，不能代表）。

---

## 7. 本轮产出清单（供组包代理接管）

```
04_scripts/勇士之心/build_killer_dex_ysxz.sh                     # 构建（烘焙 PKG + 官方证书 DER）
04_scripts/勇士之心/verify_killer_dex_ysxz.py                    # 离线校验（17 项）
04_scripts/勇士之心/assets/ysxz_cert/                            # DER / b64 / META.md / extract_v2_cert.py
04_scripts/勇士之心/assets/ysxz_killer_smali/mt/signature/*.smali # 13 个参数化 killer 类
08_output/勇士之心/killer/ysxz_signature_killer.dex              # 13,988 B, sha256 3fda698b…, fp a86dead8…
10_docs/勇士之心/_work_20261123_killer方案.md                    # 本文件
```

---

## 8. T4c 执行记录（2026-11-23 22:01 组装 + 签名，未装机）

用户授权卸载官方包后由 lead 指派继续；本轮**只组装+签名，未装机/未卸载**。

### 8.1 对组装脚本的改动（`mod菜单/starry-mod-menu/scripts/surgical_ysxz_1.0.6.sh`）

| 位置 | 改动 |
|---|---|
| L30-38 | NDK 根改为多候选探测（`ANDROID_NDK_HOME` → `ANDROID_HOME/ndk/27.0.12077973` → mac 老路径 → `01_tools/android-sdk/ndk/android-ndk-r27c`） |
| L48-52 | 新增 `KILLER_DEX`（默认 `08_output/勇士之心/killer/ysxz_signature_killer.dex`）、`KILLER_DEX_SLOT=classes3.dex`、`EXTRA_ABIS="x86 armeabi-v7a"` |
| L57-62 | `apktool` 不在 PATH 时把 `01_tools/apktool` 前置 |
| L94-96 | 必查类表 `SignSpoofAppComponentFactory` → `StarryAppComponentFactory`（前者 2026-10-28 已删） |
| L119-133 | killer dex 缺失则调 `build_killer_dex_ysxz.sh`，并对 dex 字节断言（含 `bin/mt/signature/KillerApplication193` / 包名 / 证书 `MIID` 头） |
| L135-157 | 多 ABI：arm64 必须，x86/armeabi-v7a 的 `libs_ysxz/*/libstarry_magic.so` 存在就带，各带本 ABI 的 `libc++_shared.so` |
| L232-242 | aapt2 路径探测（`ANDROID_HOME/build-tools/*/aapt2.exe` 等） |
| L246-268 | zip merge 传 `killer`+槽位；内存里先断言 dex 类锚点与槽位不冲突 |
| L300-302 | 写 `classes3.dex` |
| L310-316 | 回读断言：`classes3` 内容==killer、classes2 含 `YsxzAppComponentFactory`、classes3 含 killer 类 |
| L357-372 | entries 断言加 `classes3.dex`；末尾 size/md5/sha256 改 python（win 无 `md5 -q`/`stat -f`/`shasum`） |

`04_scripts/勇士之心/ysxz_sign.sh` 同步去 mac 化（`md5 -q`→python、`stat -f '{z}'`→python、`shasum`→python）。两脚本 `preflight`：PASS=5 FAIL=0。

### 8.2 产物与指纹

| 阶段 | 路径 | size | md5 | sha256 |
|---|---|---|---|---|
| unsigned | `08_output/勇士之心/勇士之心_1.0.6_surgical-unsigned.apk` | 67,729,196 | `56d752edc5b01a5110fffda34fb03eb9` | `ae9ccb5a147184f0703fe6edaaa0773815dc62a58fd85bf5beca69fb95a9dc00` |
| signed | `08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned.apk` | 67,850,597 | `6bd8e97e39d1b15549867461e3c31d42` | `099ffd0cbd739643853924f1956999eb7a43fd48c0357ec97fe61b6e7618aa91` |

签名：v2+v3，`CN=StarryMagic`，证书 sha256 `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d`；`zipalign -c -p 4` OK。

### 8.3 包内断言（全部 PASS）

- dex：`classes.dex` 1,538,536 / `classes2.dex` 646,132（含 `YsxzAppComponentFactory`+`StarryAppComponentFactory`，无 `SignSpoof`）/ `classes3.dex` 13,988（含 killer 类+包名+证书 `MIID` 头，md5 `c60b7d036f4fdc72366f660446add104`）。
- so：arm64 `4a2d9460…` / x86 `8eefafcd…` / v7a `887dbd5c…`，三份与 `libs_ysxz` 逐字节相同；包内 arm64 so 无 `__starry_ac`、无 `Toggle_自动穿戴`、有 `battle-speed`、菜单串 13/13。
- `assets/main.js` = 补丁版（33,601 B / `a30040db444bde06d3226c86df471508`）。
- 12 条 untouched 逐字节一致 12/12（`classes.dex`/`resources.arsc`/5 个官方 so/`apksecure*`×3/`assets/src/*`×2）。
- manifest：`appComponentFactory=com.starry.magic.ysxz.YsxzAppComponentFactory`，官方 `org.cocos2dx.javascript.WarriorApplication` 保留，`StarryInitProvider`+`MenuService` 在位。
- 包内 `classes3.dex` 再跑 `verify_killer_dex_ysxz.py`：17 PASS / 0 FAIL。

### 8.4 未做

未装机、未卸载、未清数据、未 git 提交；下一步（装机+预检+真机取证）由 lead 统一安排。
