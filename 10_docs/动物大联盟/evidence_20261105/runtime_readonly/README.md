# 运行时只读代码取证 · 动物大联盟 1.0.25（APKSecure 壳）

- 采集日期：2026-11-05 20:2x（本机时间）
- 设备：真机 `f9a116a2`（已 root，只读使用）；包 `com.fightingstars.games.animal`；版本 1.0.25（vc25）
- 目标进程：**pid 27292**（采集时在跑），`exe=/system/bin/app_process64`，`cmdline=com.fightingstars.games.animal`
- 底包指纹（原版单 base）：`2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539`
- 手段：只读 `/proc/<pid>/maps` + 只读拉取「maps 已证明是代码路径」的文件；**未启动/未注入/未截屏/未改数据**
- 脚本（本轮新增，均在 `04_scripts/动物大联盟/runtime_readonly/`）：
  `probe_runtime_readonly.sh`、`pull_mapped_code.sh`、`dex_probe_strings.py`（三者 `bash -n`/`compile`+`preflight.sh` 全 PASS）
- 原始产物（不入库）：`06_dumped/动物大联盟/env_1.0.25/{probe,code/raw}/`

---

## 1. 结论（一句话）

**拿到了原版运行期的真实解密 `libcocos2djs.so`（35,816,176 B，真 ELF）与壳侧 `libsecret.so`；
没拿到真实 app DEX —— 它在本机运行期既不落盘、也不在任何只读映射或 fd 上，唯一可能位置是可写匿名内存，属本轮禁止遍历范围。**

| 目标 | 结果 | 判据 |
|---|---|---|
| 真实 DEX（TapSDK 登录类） | ❌ **未取得** | 见 §4「为什么拿不到」四条排除 |
| 解密后的 `libcocos2djs.so` | ✅ **取得** | §3.1（ELF64/AArch64/ET_DYN，35.8 MB，映射字节=文件字节） |
| 壳 `libsecret.so`（双映像） | ✅ 取得 | §3.2（1.0.25 复现 1.0.23 的双映像现象） |
| 六串检索 | ⚠️ 在**已覆盖容器**内 0 命中，**不等于类不存在** | §5（含方法对照实验与取证范围） |

---

## 2. 证据范围与文件位置

原始 maps 全量：`06_dumped/动物大联盟/env_1.0.25/probe/maps.txt`（**3595 行**，不入库）。
本目录只保留可直接引用的切片：`probe_summary.txt`、`code_paths_app.tsv`、`maps_app_extract.txt`、`pull_manifest.tsv`、`dex_probe.txt`。

---

## 3. 已取得的运行期代码

### 3.1 解密后的 cocos 运行时（本阶段最关键产出）

| 项 | 值 |
|---|---|
| 设备路径 | `/data/data/com.fightingstars.games.animal/no_backup/mlib/arm64-v8a/libcocos2djs.so` |
| 映射证据 | `maps.txt:658` r-xp `6fd3a1d000-6fd5af2000` off 0；`maps.txt:660` r--p off `0x20d5000`；`maps.txt:661` rw-p off `0x21d5000` |
| 尺寸 / sha256 | **35,816,176 B** / `dd3a046291b4d4988a32ae20d7b98ebf5d46e992c70f0fa7168934c627893b44` |
| 格式 | `7f454c46` ELF64、little-endian、`e_type=3`(ET_DYN)、`e_machine=0xb7`(AArch64) |
| 与映射一致性 | 映射首 32 B 与本地文件首 32 B **逐字节相同** ⇒ 文件即运行映像 |
| 真的运行时（非空壳） | 明文含 `cocos2d` ×6632、`jsb` ×1812 |
| 与官方包静态对照 | APK 内为 `assets/apksecuresolib/*/libcocos2djs.so` 15.1 MB **非 ELF**（加密）；运行期释出后 **35.8 MB 真 ELF** ⇒ 壳已解密落盘 |

用途：后续「环境版本对齐」直接以此为基准做 so 指纹/版本比对，无需再碰壳解密逻辑。
**本轮未做**：符号表、RVA、函数定位、Cocos JS 解密（按任务边界全部推迟）。

### 3.2 壳 so 双映像（1.0.23 现象在 1.0.25 复现）

| 项 | 值 |
|---|---|
| 设备路径 | `/data/app/~~xx_0v9N-fhVp7REGBZb1RQ==/com.fightingstars.games.animal-…/lib/arm64/libsecret.so` |
| 尺寸 / sha256 | **459,344 B** / `e0a31698b9c14fb7ba8e677a7af136551479ce0f203941437042a4fa7fb709ae` |
| 映像 A | `maps.txt:582` r-xp `6fc1c80000` + `:584` r--p off 0x62000 + `:586` rw-p off 0x65000 |
| 映像 B | `maps.txt:1469` r-xp `7099087000` + `:1471` r--p off 0x62000 + `:1473` rw-p off 0x65000 |

⇒ 同 inode 670118 在同一进程被映射**两次**（两处 3 段 r-xp/r--p/rw-p）。
这与 `硬约束.md` D 区 #13 记的 1.0.23「粗粒度 JNI 映像 + APKSecure 保护映像」同构，**但 1.0.25 的状态变量偏移本轮未核**（任务禁止壳状态机分析，见 `硬约束.md` E 区 #15）。

### 3.3 其余同在 `lib/arm64/` 的已映射 ELF（一并取得，供对照）

`libtapsdkcore.so` 6,106,224 B `7af3008a3986bea7b7ee76128c7bacacd51601fd74ad371d4f37ba0ef5b35fa0`；`libmsaoaidauth.so` 418,952 B；`libmsaoaidsec.so` 686,520 B；`libBugly.so` 190,656 B；`libEncryptorP.so` 75,744 B；`libluster.so` 39,568 B；`libterrain.so` 256,176 B。全部 size_dev=size_local、`PULL_OK`（见 `pull_manifest.tsv`）。

---

## 4. 为什么拿不到真实 DEX（四条排除，均为实测）

1. **无落盘 dex 文件**：`/data/data/<pkg>/no_backup/` 只有 `.sec_shell_state`(20 B) 与 `mlib/arm64-v8a/libcocos2djs.so`；顶目录只有 `app_bugly/app_crashrecord/cache/code_cache/databases/files/oat/shared_prefs`，**没有 mdex/dex 目录**。
2. **oat 里只有壳自身**：`/data/app/…/oat/arm64/base.odex`(33,688 B ELF, `maps.txt:710`) + `base.vdex`(1,567,316 B, `:711`) —— `base.vdex` 内 78 次命中 `dev/mars/secure`（**壳 stub 本身的类**），无 TapSDK。
3. **唯一的 `Anonymous-DexFile` 不含 dex 字节**：`/data/data/<pkg>/oat/arm64/Anonymous-DexFile@442084367.vdex`（476,960 B，`maps.txt:2097` rw-p）头为 `vdex027`，`num_dex=4` 但 **`dex_size=0`**、`verifier_deps=60`、`quickening=12` ⇒ 只有 ART 校验/去优化元数据，**dex 载荷不在里面**。
4. **无 memfd、无只读匿名 dex 映像**：
   - `/proc/27292/fd` 共 235 项，无 `/memfd:` dex 类条目（maps 里仅 `/memfd:jit-zygote-cache`，属 zygote 共享）。
   - maps 里**无路径**的 `r--p`/`r-xp` 段共 10 个：9 个 `r-xp`（4 KB–0x3D000）+ 1 个 `r--p`（0x6e08040000，0x1A000=106,496 B）。抽检其中 2 段首 32 B：`0x6e08040000`→`00 a0 01 00 …`、`0x6e00042000`→`c1 05 04 08 …`，**都不是 `dex\n`/`ELF` 魔数** ⇒ 不是 dex 映像。
   - 结论：dex 只可能在 **可写匿名内存（rw-p）**（InMemoryDexClassLoader 的堆内缓冲）—— 那是匿名 heap，本轮任务明令禁止遍历，且无法与账号/存档缓冲区分。

---

## 5. 六串检索（只查这 6 条，不枚举类/方法/metadata/ABI/RVA）

探针：`dex_probe_strings.py`（NEEDLES 硬编码 6 条）。覆盖容器 = 本轮能拿到的全部 DEX/VDEX 容器（3 个）：

| 容器 | sha256(前16) | 六串 |
|---|---|---|
| `base.vdex` 1,567,316 B | `3c06b7ab1a731e2f…` | 0/6 |
| `Anonymous-DexFile@442084367.vdex` 476,960 B | `fa8260bd0dce6a85…` | 0/6 |
| `base.odex` 33,688 B | `c9db615c10a6fd5e…` | 0/6 |

逐条：`GatekeeperRepository`、`isGatekeeperLoadedSuccessfully`、`IntentHelper`、`LoginActivityImpl`、`WebHandler`、`dev.mars.secure` —— 全部 miss。

### 5.1 方法对照实验（证明"没命中"不是因为探针坏了）

对同一批文件用**同一套字节搜索**去查 `dev/mars/secure` 的 **DEX 内部斜杠形式** `dev/mars/secure`（类描述符以 `Ldev/mars/secure/…;` 存储）：
`base.vdex` 命中 **78 次**（首个偏移 `0x1214c7`）。

⇒ 两点必须一起读：
- 探针有效（同样的搜索能命中壳自己的类）；
- 任务书给的 **点号形式 `dev.mars.secure` 在 DEX 里天然不会命中**（DEX 存斜杠形式），所以"点号 miss"**不能**当作"没有这个包"的证据。

### 5.2 取证范围（"不见字符串 ≠ 类不存在"）

本轮只覆盖上面 3 个**壳侧**容器。**真实 app DEX（含 TapSDK / Gatekeeper 实现）不在覆盖范围内**（§4）。
因此：`GatekeeperRepository` 等 5 条未命中，**既不能证明类不存在，也不能证明存在** —— 结论只能是"本轮未覆盖"。
这与 `硬约束.md` E 区 #14 一致：该项仍待运行期证据，不得据此改判 `pre_install_gate` 查12。

---

## 6. 最小下一步（只报条件，不擅自执行）

要拿到可检索的真实 DEX，只剩一条路，需**主代理/用户明确授权**其一：

- **A1｜单段定向只读（最小动作）**：授权后仅对 maps 中 **rw-p 匿名段**做"首 8 字节魔数 + 尺寸指纹"定位，命中 `dex\n035` 后**只读该单段**（≤ 解密后 dex 体量），不做全内存扫描。
  前置声明：rw-p 属进程堆，理论上可能夹带账号/会话缓冲 ⇒ 必须先明确"按段限界、只取 dex 结构、不落无关缓冲"的边界，并保留 segment→文件映射。
- **A2｜反向静态**：分析壳解密路径（`libEncryptorP.so`/`libsecret.so` 如何把 `assets/apksecurefile` 解成 dex）——属第二阶段，需"开始做功能"口令外**单独**授权，且与"阶段闸门"冲突，需主代理判断。
- **A3｜绕开 dex（环境交付可能不需要它）**：1.0.23 的先例是注入独立 `libstarry_magic.so` 从 `JNI_OnLoad` 侧介入，**不依赖 Java 类名**；若登录 patch 也走 native/Provider 侧，则"取不到 dex"不构成环境阶段阻断 —— 建议主代理优先评估这条。

---

## 7. 本轮明确未做（边界自证）

未启动/未 force-stop 应用；未装机/卸载/清数据；未 `screencap`/`input tap`；未注入 Frida、未加载任何服务；未读 `shared_prefs`/`databases`/`cache`/`files` 内容（仅列目录名）；未遍历全进程内存、未做全量 dump；未做 Cocos 玩法/JS 解密分析；未查函数 RVA/符号表；未枚举类型与 metadata。
