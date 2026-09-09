# 知识库 03 · 签名与 APK 完整性（v1-v4 全谱）

> 📍 `re-analyze/protection/knowledge/_03_签名与APK完整性.md`
> 🎯 签名校验 v1/v2/v3/v4 全谱 + APK 完整性目标全表 + 四层绕过。核心铁律贯穿始终：**让校验通过，不 patch 结论**。

---

## 0. 先分清两个概念

- **签名校验**：验证「APK 的签名者证书」是不是官方那张证。主要发生在：安装（PMS）、运行时 Java `getPackageInfo(signatures)`、运行时 native 读 `base.apk` 签名块。
- **完整性校验**：验证「APK 文件字节 / 某个 so / 某个 dex / 某个 asset」是不是官方那几字节。**跟签名无关**，跟内容哈希有关。

两者常被混淆，混了就会走错路：改签签名保留测试，完整性校验根本不理这一套。

---

## 1. 签名方案 v1 / v2 / v3 / v4 全谱

### v1（JAR 签名）
- **机制**：`META-INF/MANIFEST.MF` 里每个条目算摘要 → `CERT.SF` 再摘要 → `CERT.RSA` 对 `CERT.SF` 签名
- **特征**：`META-INF` 有 `.RSA/.DSA/.EC`；`apksigner verify --verbose` 显示 v1 verified
- **可绕过面**：
  - v1 只校验 `META-INF` 清单里的条目（可选择性忽略某文件）
  - 追加额外 `META-INF` 文件、夹带 non-`META-INF` 条目 → 会破坏 v1（雷霆/王国已踩：追加 zip 条目 v2 块被剥或 digest 坏）
- **对抗**：SignSpoof 回灌官方签名到 Java 层；native 读签名块需另解

### v2（APK Signature Scheme）
- **机制**：签名写在 APK 的 **Signing Block**（EOCD 之前一段可扩展 block）。方案把 APK 分成 1MB chunk，每个 chunk 算 digest，签名覆盖这些 digest + 结构。
- **特征**：`apksigner verify --verbose` 显示 v2 verified；Signing Block 里有 `0x7109871a` 对
- **可绕过面**：
  - v2 **不校验全部内容**，只校验「被 chunk 覆盖到的内容」+ APK 结构头尾 → 部分追加内容可能落在校验范围外（但本工作区实测：追加条目/CD 搬移会坏整体，别盲试）
  - **不能**重签回同一张证书（没有官方私钥）
- **对抗**：这是「原生读签 + 完整性」的重灾区，Java SignSpoof 救不了 native 读 `base.apk` 的签名块

### v3（APK Signature Scheme）
- **机制**：在 v2 基础上加 **key rotation**（证书轮换），Signing Block 里有 `0xf05368c0` 对
- **特征**：`apksigner verify` 显示 v3 verified
- **可绕过面**：与 v2 基本同层，多了轮换证书链的概念，对改包的影响等同 v2
- **对抗**：同 v2

### v4（based on fs-verity，Android 11+）
- **机制**：签名数据存在 `.idsig` 旁挂文件（adb install 时生成），内核 fs-verity 校验
- **特征**：`adb install` 生成 `.idsig`；`apksigner verify` 显示 v4 verified（部分 apksigner 版本）
- **可绕过面**：
  - v4 由 **安装器**（adb/pm）在安装时通过 fs-verity 生效，改包后**重新安装**会重新计算 → 本工作区**无 root 直装重签会触发**
  - root 下 `adb install --incremental` 相关坑：incremental-fs 用 root 也写不了 `/data/app` → 必须 `--no-incremental`（王国大作战实锤）

### 状态
- ✅ v1/v2/v3 Signing Block 结构解析、chunk digest、追加条目破坏（雷霆/王国）
- ✅ v4/incremental-fs 坑（王国大作战 `--no-incremental`）
- ⚠️ fs-verity 细节待精确验证，但已掌握「重装重算」关键结论

---

## 1.5 native 读签识别线索（IDA 定位）

> 目标：改包跑起来，Java 层用 SignSpoof 能过，但 `base.apk` 字节 / 签名块在 native 又读一遍的，怎么认出、怎么定位、怎么解。

### 识别特征（拆 .so 先查这些）

| 线索 | 含义 | 命中后动作 |
|---|---|---|
| 字符串 `APK Sig Block 42` | 解 v2/v3 Signing Block（官方格式魔术串） | xref 到解析函数，追到校验入口 |
| 立即数 `0x7109871A` | v2 Signing Block ID | IDA 搜立即数，定位「读块」而非「读哈希」 |
| 立即数 `0xF05368C0` | v3 Signing Block ID | 同上，区分 v2/v3 分支 |
| 字符串 `base.apk` / `.apk` + `META-INF` / `.RSA` / `.DSA` / `.EC` | 直接开 APK 读签名 | 追 open → read → 解签 → cmp |
| 字符串 `getPackageInfo` / `PackageInfo` / `GET_SIGNATURES` / `GET_SIGNING_CERTIFICATES` | 走 PM 的签名读（Java en Java，SignSpoof 可救） | 若纯 Java 入口 → SignSpoof；若再落到 native → 记下入口 |
| `mmap` 后紧跟 CRC/MD5/`memcmp` | 读的是整包字节哈希（完整性，非签名） | 归入「字节完整性」，按 §2 表走「官方字节保留」 |

### IDA 定位四步

1. **先查字符串**：`Shift+F12` grep `APK Sig Block`、`EOCD`、`base.apk`、`.RSA`；无果就 grep 立即数 `0x7109871A` / `0xF05368C0`。
2. **xref 追链**：从字符串/立即数 xref 到解析函数，往上找 `open / mmap / read`，往下找 `cmp`，把「上游读的是 APK 还是已解出的证书」分清楚。
3. **分两条路**：
   - 读的是**签名块 parsed bytes**（X.509 证书链，比证书指纹）→ 回灌官方指纹/证书字节
   - 读的是**整包字节再算 hash**（完整性）→ 别 patch 结果，patch 输入为官方字节 / 走「官方字节保留」
4. **判死法**：native 读签 + 完整性绑定 = Java SignSpoof 无效，且 patch kill 会死转；唯一稳路是「校验读到的值在包内就是官方正确值」。

### 工作区状态

- ⚠️ native 读签定位法为通用思路，本工作区未逐游戏留完整 IDA xref 记录；命中新壳时按四步现做
- ✅ 已实锤：Java SignSpoof 救不了 native 读 `base.apk` 签名块（§3 四层表）

---

## 2. APK 完整性目标全表

| 校验目标 | 死法表现 | 对策 | 工作区 |
|---|---|---|---|
| `base.apk` 字节级（md5） | 加一条 dex ~2s 死 | 官方字节保留，只新增不修改 | ✅ KwProtect |
| APK Signing Block / CD 结构 | 追加条目即死 | 别动 EOCD/CD/SigningBlock | ✅ 王国 KwProtect |
| split APK 存在 | ~15s signal 9 | 不装 split | ✅ KwProtect |
| 指定 dex 哈希 | 改该 dex 死 | A/B 测清单，菜单放安全 dex | ✅ 公主连结 |
| `lib/` 既有 so 被替换 | MD5 杀 | 只新增 so，不替换 | ✅ KwProtect |
| 多 ABI so 摘要（MD5） | 改任一 abi so 死 | 全 abi 同步一致 | ✅ 雷霆 tprt |
| so 段摘要表（size+CRC+tail） | 改 `.data` 任一字节死 | 全量同步摘要槽/让通过 | ✅ 雷霆 tprt@0x19bc10 |
| `resources.arsc` | 整体替换/压缩错死 | STORED + 字节级保留 | ✅ 公主连结 |
| asset 单文件哈希 | 改某 asset 死 | 别动 / 新增可容忍 | ✅ 长生/多游戏 |
| APK 体积 + 黑名单路径 | 登录弹非法 | 控体积 + 不含黑名单文件 | ✅ 长生天机降世 |
| 运行时资源加密 | 静态看不懂，运行时正常 | 蜜点 hook 即破（≠保护壳） | ✅ 蜀汉 BGT1 |

---

## 3. 四层签名/完整性绕过（代价从低到高）

| 方案 | 适用 | 能力 | 代价 | 工作区 |
|---|---|---|---|---|
| SignSpoof（Java `PackageInfo.CREATOR` hook） | Java 层读签 | 过 Java getPackageInfo | 只覆盖 Java 层，native 无效 | ✅ |
| 原生回灌签名/摘要（patch 比对） | native 读签名/哈希 | 过 native 校验点 | 需定位到具体 `cmp`，且避免 patch 结论 | ✅ 物质与魔法 |
| 官方字节保留（外科不修改） | 完整性校验 | 最稳 | 只能新增，不能改被保护的字节 | ✅ KwProtect |
| packages.xml / Zygisk / 设备级 | PM 证书终局门 | 全量伪装签名 | 需设备级操作（root/重启），非无 root | ⚠️ 王国待选 |

### 各方案细节

**SignSpoof**：`attachBaseContext` 里 hook `PackageManager.getPackageInfo` 回灌官方 `Signature[]`。局限：native 用 `base.apk` 字节 + 签名块读 → 无效。

**原生回灌**：IDA 找 `deal_check_fail` 或比对 MD5 的 `cmp`，把入口 RET / 回灌官方 MD5。坑：patch 太多点或 patch 到校验结果会引发死转。

**官方字节保留**：外科组装铁律。坑：新增 so 是否被扫（A/B 测「新增容忍度」）。

**packages.xml**：root 改 `/data/system/packages.xml` 里本包签名记录为官方证书 → PM 层全量伪装，Q1 弹窗消失。坑：需 stop/start framework 或重启 system_server；操作风险中等（备份可回滚）。王国大作战方案 A。

**Zygisk**：模块在进程 specialize 时注入，可做更深的签名伪装。坑：可能被 KwProtect/Zygisk 检测；需装模块重启。

---

## 4. 金银教训（签名/完整性专篇）

1. **改签后渠道 dex 不解出（ClassNotFound）是下游症状**，根因在改签触发完整性校验失败 → 别死磕 ClassNotFound 本身（王国大作战 B路线证伪）。
2. **SigningBlock 截短 ≠ 根因**：王国大作战「保渠道 SigningBlock」假设已被实锤证伪——真正根因是 v2 整包签名校验。
3. **追加 zip 条目会坏 v2 结构化**：`zip -u` 后 CERT.SF 带 v2 stripping protection → DOES NOT VERIFY。
4. **无官方私钥就无法改内容后重签回原证**（Zeus 插件签名墙 `CN=union` 实锤）→ 正确路是「官方字节不动 + 新增」不是「改 + 重签」。
5. **让校验通过 > patch 结论**（KwProtect/ACE 均实锤 patch kill=死转，唯一例外是「patch 校验输入为官方值」而非「patch 校验结果」。

---

## 总结对照卡

| 方案 | 何时用 | 一把锁 |
|---|---|---|
| v1 | 老包 META-INF 签 | CERT 清单逐条 |
| v2 | Signing Block chunk digest | 官方私钥不可得 |
| v3 | Signing Block key rotation | 同 v2 |
| v4 | Android11+ fs-verity | 重装重算 |
| 字节完整性 | 加条目即死 | 官方字节保留 |
| 四层绕过 | 由低到高 Java→native→字节→设备 | 按校验所在层选 |