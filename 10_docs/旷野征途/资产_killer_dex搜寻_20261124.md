# 资产搜寻：killer dex（旷野征途 3.2 / B 轨签名伪装）
> 生成：2026-11-24 16:5x ｜ 子代理 asset-killer ｜ **只读搜寻 + 仅复制/仅本地重建到 `13_scratch/`**
> 未做：adb / 装机 / 组包 / 改仓内代码 / git 提交。本文所有 sha256 均为本机实测命令回显。

## 0. 结论（一句话）

**仓内的 killer「资产」没丢——丢的只是 kyzt 专属的那一份压好的 dex；制作它的条件全在仓内，且本次已在 `13_scratch/kyzt32_killer/` 重建成功并静态自校验通过。**

| 项 | 状态 |
|---|---|
| 参数化 smali 源码（MT ApkSignatureKillerEx 形态） | **在**，11 个游戏各一份，逐字节同源（主类 md5 `fb91a89f…`，$j md5 `efa62865…`） |
| 构建脚本（从官方底包 v2 块取证书 DER → 烘焙 pkg+cert → smali 汇编） | **在**，`04_scripts/墨守孤城/msgc_build_killer_dex.sh`（bash）、`04_scripts/爆裂防线/blfx_build_killer_dex.py`（py） |
| 已压好的 killer dex（可直接塞包） | **在，但都是别家游戏的证书**，无一份带 `com.xf.kyzt` |
| kyzt 专属 killer dex（2.4 用过的那份 13,692 B / `ac77dfc3…2150d`） | **不在**（仓内全域 dex 扫描 0 命中）；已用同配方重建出等价物 13,696 B |
| 官方证书 DER（678 B，sha256 `0a3b7825…048f`） | **在**，可从 `02_apks/旷野征途/旷野征途_3.2_taptap_official.apk` 的 v2 签名块现场解出（实测一致） |

## 1. 搜寻范围与方法（全部可复跑）

### 1.1 文本/二进制串搜索（`--no-ignore` 是必须的，`04_scripts/**/assets` 被 .gitignore 覆盖）

```bash
cd /e/nixiang
rg -l -a --no-ignore "KillerApplication" . | sed 's|^./||' | sort      # 200+ 命中
rg -l -a --no-ignore "bin/mt/signature" .
rg -l -a --no-ignore "SignatureKillerBootstrap|com/starry/magic/sign" .
```

> 坑：默认 rg 受 `.gitignore` 影响，第一次跑 `rg -l "KillerApplication"` 是**空结果**，会误判「仓内无资产」。必须加 `--no-ignore -a`。

### 1.2 APK 内 dex 扫描（`unzip -Z1` + 提取 + `grep -a`）

```bash
for a in 08_output/*/*.apk; do
  for d in $(unzip -Z1 "$a" | grep -iE '^classes[0-9]*\.dex$'); do
    unzip -o -p "$a" "$d" > /tmp/t.dex
    grep -aq "bin/mt/signature/KillerApplication193" /tmp/t.dex && echo "HIT $a :: $d"
  done
done
```

结果：

| APK | dex 数 | killer 命中 |
|---|---|---|
| `08_output/旷野征途/旷野征途_3.2_starry-releaseSigned.apk` | 8（classes…classes8） | **无**（与「3.2 不注入 killer」的 lead 决策一致） |
| `08_output/旷野征途/旷野征途_3.2_starry-unsigned.apk` | 8 | **无** |
| `08_output/勇士之心/勇士之心_1.0.6_surgical-releaseSigned-fix7-clean.apk` | 3 | `classes3.dex`，13,988 B |
| `08_output/霓虹深渊2/霓虹深渊2_2026.11.19_surgical-releaseSigned.apk` | 6 | `classes6.dex`，13,812 B |

### 1.3 全域 dex 找「kyzt 专用」killer（决定性判据：dex 内同时含 killer 类名 + `com.xf.kyzt`）

```bash
find . -name "*.dex" -not -path "./.git/*" | while read f; do
  grep -aq "bin/mt/signature/KillerApplication193" "$f" && grep -aq "com.xf.kyzt" "$f" && echo "KYZT-HIT $f"
done
```

**0 命中**（含 `03_decompiled / 07_patched / 13_scratch / 99_archive / mod菜单 / starry_link / 09_test` 全部 dex）。
⇒ kyzt 那份 2.4 用过的 killer dex 确实已随 `04_scripts/旷野征途/assets/` 消失，**仓内不可直接取用**。

## 2. 命中的可复用资产清单（逐项指纹，均为实测）

### 2.1 参数化 smali 源码（**真正的可复用资产**，11 份同源）

| 路径 | 类数 | 主类 md5 |
|---|---|---|
| `04_scripts/墨守孤城/assets/msgc_killer_smali/mt/signature/` | 13（`KillerApplication193` + $a..$l） | `fb91a89fbb9c2f6adac7c7531673c256` |
| `04_scripts/勇士之心/assets/ysxz_killer_smali/mt/signature/` | 13 | 同上 |
| `04_scripts/不一样西游`、`修仙大陆模拟器`、`动物大联盟`、`吾辈小熊猫`、`我在神界刷装备`、`我的技能无限触发`、`正中靶心2`、`追迹迷城`、`黎明前20分钟` | 13 / 每游戏 | 同上 |
| 参照实现 | — | `KillerApplication193$j.smali` 全部 md5 `efa628655a26785926d4eb691202b18f` |

- 主类 `.super Landroid/app/Application;`，`<clinit>` 内两个占位符：`@@CERT_B64_WRAPPED@@`（官方证书 DER 的 base64，按 64 字符切行、行间用**字面** `\n`）+ `@@PKG@@`（目标包名）。
- 能力：`killPM(pkg, certB64)`（包装 `PackageInfo.CREATOR` + 清 Parcel/PackageManager 缓存，回灌 DER）、`killOpen(...)`。
- 源码 URL 常量：`https://github.com/L-JINBIN/ApkSignatureKillerEx`。

### 2.2 构建脚本（烘焙配方）

| 路径 | 形态 | 关键行 |
|---|---|---|
| `04_scripts/墨守孤城/msgc_build_killer_dex.sh` | bash + 内嵌 python | 37–76 从 APK Signing Block 取 v2 证书 DER 并校验 sha256；82–98 替换占位符；100 汇编；103–109 baksmali 反查门禁 |
| `04_scripts/爆裂防线/blfx_build_killer_dex.py` | python | 同配方 |
| `04_scripts/不一样西游/build_killer_dex_byyxy.sh`、`build_killer_dex_{ysxz,dwdlm,xxdlmnq,sjzz,wdjnwcf,zzbx2,zjmc,lmq20fz}.sh`、`msgc_build_killer_dex.sh` | bash | 每游戏一份，只改 `PKG` / `CERT_SHA256` / 底包路径 |

### 2.3 已压好的 dex（可直接塞包，但证书是别家的）

| 来源（绝对路径） | 字节 | sha256 | md5 | 内嵌 pkg | 内嵌 DER |
|---|---|---|---|---|---|
| `E:\nixiang\04_scripts\墨守孤城\assets\msgc_signature_killer.dex` | 13,720 | `280e996575941629f8595282a6a71da37c6d1664849235a47623088dd4366f76` | `b1c683ba…` | `com.chillyroom.inkvasion.yw` | 684 B / `f49140d7…` |
| `E:\nixiang\04_scripts\爆裂防线\assets\killer_template.dex` | 13,884 | `879fad13b170471bf2bbc2078b3d84d1a553c4d4ea03768ff15bddb2f9f01174` | `6aaed994…` | `com.mxyzt.gzmy` | 826 B / `f49c6ca5…` |
| `08_output/勇士之心/…fix7-clean.apk :: classes3.dex` | 13,988 | `3fda698bd87a8c04ed42e81d1b87b74f77b1ed0354167506c4d4d235fa553ef4` | `c60b7d03…` | `com.fightingstars.games.warrior` | 877 B / `de8a75d3…` |
| `08_output/霓虹深渊2/…2026.11.19…apk :: classes6.dex` | 13,812 | `448ddba42e305f7535627bb54d1a9cc9720c6b014e1d5bef418ffdc2edf5dc6d` | `8ef006fe…` | `com.veewo.neonabyss2` | 674 B / `b86bbedd…` |

> `killer_template.dex` 名字叫 template，但**不是**占位符模板：占位符已被烘死成 `com.mxyzt.gzmy` + 826 B 证书。不能当通用模板用。
> 这四份都**含 13 个 killer 类**（`baksmali list classes` 实测），类名与框架硬编码一致，但证书/包名不匹配 kyzt ⇒ 塞进 kyzt 包只会回灌错的证书，等价于「伪装成别家游戏」，不能用于过 TapTap 验签。

## 3. 本次重建产物（`13_scratch/kyzt32_killer/`，已静态校验通过）

### 3.1 目录

```
13_scratch/kyzt32_killer/
├─ kyzt_signature_killer.dex         ← 重建产物（可塞包用）
├─ rebuild_kyzt_killer.sh            ← 一键复现（读官方底包 → 取 DER → 烘焙 → 汇编 → 自校验）
├─ smali_src/mt/signature/*.smali    ← 复制自 04_scripts/墨守孤城/assets/msgc_killer_smali（原文件未改动）
├─ ref_msgc_build_killer_dex.sh.txt  ← 参照构建脚本副本
├─ ref_blfx_build_killer_dex.py.txt  ← 参照构建脚本副本
├─ prebuilt/                         ← 四份别家 dex 样本（对照用）
├─ logs/cert.b64 / cert.wrapped      ← 从 3.2 官方底包 v2 块解出的 DER base64（原始/带字面\n）
└─ work/                             ← 烘焙后的 smali 中间态（占位符已替换）
```

### 3.2 产物指纹

| 项 | 值 |
|---|---|
| 字节 | **13,696 B** |
| sha256 | `35ede19c2884b1e9f8919455bffc265f272966cfd2e2891c61aab64e060cbe5d`（另一次构建得到 `0a7646e4d39ae1388f14a52d22bd7dc9cbcf34908a98885dd26378b47f14c602`，见 §3.4） |
| md5 | `864a6fa81985cf43125fe2b0326402ad` |
| 类列表 | 13 个：`Lbin/mt/signature/KillerApplication193;` + `$a $b $c $d $e $f $g $h $i $j $k $l` |
| 内嵌 pkg | `com.xf.kyzt` |
| 内嵌证书 DER | **678 B**，sha256 `0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f`，md5 `360c78f563dd471fa6df02cc68045a31` |
| 占位符残留 | 0（`grep -a -c '@@'` = 0） |
| 父类 | `android.app.Application` |

证书来源**不是复用旧 dex**，而是从 3.2 官方底包现场解出（读 v2/v3 APK Signing Block → signer → signed data → certificates 首项）：

```
输入：02_apks/旷野征途/旷野征途_3.2_taptap_official.apk（198,941,707 B）
输出：DER 678 B  sha256 0a3b7825…048f  md5 360c78f5…5a31
```

⇒ **3.2 官方签名证书与 2.4 完全同一把密钥**（与 `10_docs/旷野征途/组包_2.4_20261116.md:36` 记录的指纹逐位一致），因此 2.4 的 killer 结论在 3.2 继续成立。

### 3.3 与 2.4 原产物的差异（必须写明，不要当成同一份）

`10_docs/旷野征途/组包_2.4_20261116.md:44` 记录 2.4 用的是 `classes10.dex` **13,692 B / sha256 `ac77dfc30c84dfb8aca1c9c639e1249de6eccd2cbfe12aa5bb96cd9e97b2150d`**。
本次重建为 **13,696 B**（差 4 B），哈希不同。原因未定位到字节级（候选：原 2.2 产物证书 base64 切行宽度/尾部换行与我不同，或当年的 smali 版本略有差异）。**语义等价已证**（类集合一致、pkg 一致、DER 逐字节一致），**字节等价无证据**。
⇒ 任何门禁**不要**拿 `ac77dfc3…` 去校验重建产物。

### 3.4 顺带发现：smali 汇编产物**非字节确定性**

同一份输入连续构建两次，字节数恒为 13,696，但 sha256 在 `35ede19c…` 与 `0a7646e4…` 之间摆动（已实测 3 次：A/B/B、B/A/A）。`01_tools/smali/smali-fat.jar` 在该 JDK 下 map 项顺序不稳定。
⇒ killer dex 门禁只能钉「类集合 + 内嵌 DER sha256 + pkg」，**不能钉 dex 的 md5**（`msgc_build_killer_dex.sh` 里的 `EXPECT_MD5` 模式对本机不可靠）。

## 4. 三条重建路径与成本判断

| # | 路径 | 需要什么条件 | 成本判断 |
|---|---|---|---|
| ① | 手机用 MT 管理器重新生成 killer 并导出 dex | MT 管理器（ApkSignatureKillerEx 功能）+ 把 3.2 官方包或官方证书给它 + 一次导出 + 拷贝到 PC；**要开手机操作，违本仓「禁装机」纪律，需你本人做** | 10–20 分钟人工，零技术风险，但产出仍要回到 PC 侧校验 pkg/DER（否则容易导出「已 hook 自身 App」而非模板 dex） |
| ② | 用本仓 `01_tools/smali` + 参数化 smali 从零写等价类 | **已具备，且已跑通**：`13_scratch/kyzt32_killer/rebuild_kyzt_killer.sh`（smali-fat.jar + 3.2 官方底包取 DER + 11 份同源 smali 之一）；不需联网、不需手机 | **最低（本次 ~5 分钟，纯本地只读输入）**；本路径已产出静态自校验通过的 dex，唯一未证是运行期效果 |
| ③ | 从其它游戏正式包借现成 killer dex | 需目标是**同包名同证书**；仓内四份现成 dex 的 pkg/DER 全是别家（`com.chillyroom…`/`com.mxyzt.gzmy`/…），换成 kyzt 必须重烘焙 ⇒ 等价于路径②，只多一次 baksmali/smali 往返 | 对 kyzt **不可用**（借来即错证书）；只有在「新游戏与某旧游戏同密钥」时才有意义，那条场景下 ≈0 成本 |

**推荐**：②（已出活）；①当交叉验证用（MT 导出的 dex 与重建产物类集合应一致）。

## 5. 框架侧：`SignatureKillerBootstrap` 触发链与可配置性

### 5.1 触发链（代码位置）

| 环节 | 文件:行 | 内容 |
|---|---|---|
| 唯一调用点 | `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java:79` | `com.starry.magic.sign.SignatureKillerBootstrap.install(application);`（在 `ModBridge.setApkPath` 之后、注册 ActivityLifecycle 之前） |
| 入口 | `.../sign/SignatureKillerBootstrap.java:33` | `public static void install(Context ctx)`，`sDone` 幂等；失败只记日志不抛 |
| 类名常量 | `.../sign/SignatureKillerBootstrap.java:27` | `private static final String KILLER_CLASS = "bin.mt.signature.KillerApplication193";` |
| 真正的 `<clinit>` 触发 | `.../sign/SignatureKillerBootstrap.java:38` | `Class.forName(KILLER_CLASS);`（不带 `initialize` 形参，仍会初始化类 ⇒ 跑 `<clinit>` ⇒ killPM/killOpen） |
| 取证标记 | `.../sign/SignatureKillerBootstrap.java:49–61` | 写 `filesDir/sigkiller.txt`（`ok …` / `fail: …`），无 logcat 时用它判定是否装载 |
| R8 保留 | `mod菜单/starry-mod-menu/app/inject-dex-rules.pro:55` | `-keep class com.starry.magic.sign.SignatureKillerBootstrap { *; }`（未 `-keep` killer 类：它由 `Class.forName` 加载，**killer dex 本身不参与混淆**，无影响） |
| 并存/历史入口（同功能，别处仍在用） | `.../sign/StarryAppComponentFactory.java:40,47`；`.../dwdlm/DwdlmAppComponentFactory.java:77,158`；`.../dwdlm/DwdlmKillerFactory.java:25,41` | 各自硬编码同一类名，用 `Class.forName(KILLER_CLASS, true, cl)` |

### 5.2 类名可否配置（决定「借用/改名」可行性）

- **不可配置**：`KILLER_CLASS` 是 `private static final String` 字面量（`:27`），类里没有读 `BuildConfig`、`System.getProperty`、资源或 manifest meta-data 的代码（整文件 62 行，已通读）。四个入口类（Bootstrap / StarryAppComponentFactory / DwdlmAppComponentFactory / DwdlmKillerFactory）全部硬编码。
- 因此「换个类名的 killer」需要**改 Java + 重编框架 dex**（动 `mod菜单/**` 就算改代码，本任务禁做）。
- 但 MT 生成的 killer 类名**恒为** `bin.mt.signature.KillerApplication193`（仓内 200+ 命中、11 个游戏的 smali 全部同名）⇒ **借用不需要改名**，只要包名/证书烘焙对，直接塞即可。
- 装载顺序风险已由设计规避：`SignatureKillerBootstrap` 在 Provider 内被调用，而 Android 的 Provider 早于 `Application.onCreate`/Activity，`PackageInfo.CREATOR` 在首次签名查询前就已被替换。

## 6. 若要让 3.2 用上（供 lead 决策，未执行任何改动）

现状：`04_scripts/旷野征途/surgical_kyzt_3.2.sh:18–19,207` 明确「**不注入 killer dex**（lead 2026-11-24 决策）」——2.4 的 `KYZT_KILLER_DEX`/`classes10.dex` 分支已被整段删除。

若改为注入，需要三处配合（**本轮一行未改**）：

1. **dex 序号必须是 `classes9.dex`**：3.2 官方 7 dex + 框架 `classes8.dex`（`surgical_kyzt_3.2.sh:49,51`），脚本第 246 行硬校验「1..8 连续无缺口」；ART 遇首个缺口即停扫 ⇒ killer 只能接在 8 之后。
2. 取回 `KYZT_KILLER_DEX` 分支：把 `13_scratch/kyzt32_killer/kyzt_signature_killer.dex` 作为输入，门禁沿用 2.4 版（`surgical_kyzt_2.4.sh:267–278`：dex 名合法性、含 `bin/mt/signature/KillerApplication193` 与 `com.xf.kyzt`、父类为 `android.app.Application`）。
3. `SignatureKillerBootstrap` 已在框架里、已被 `StarryInitProvider:79` 调用，无需改 Java；只需 dex 里真有那个类。

## 7. 未证项（不得当作已成立）

1. **运行期效果未实测**：重建 dex 只做了静态校验；3.2 底包（7 dex、API 36、`appComponentFactory=androidx.core.app.CoreComponentFactory`）上 `PackageInfo.CREATOR` 替换是否仍成功、TapTap Init 是否 `accepted=True`，均无装机证据（本任务禁装机）。
2. **C# 侧 `ET.AndroidPackageIntegrity` 的读取路径未查**：它是否走 `PackageManager`/`getPackageInfo(GET_SIGNATURES)`、是否另有 native 校验，本轮未读 `06_dumped` 的 `dump.cs`；若它绕开 `PackageManager`（直读 APK 或 native 自校验），killer 可能不覆盖这条路径。
3. **13,696 vs 13,692 的 4 字节差异未定位**（见 §3.3），重建产物与 2.4 原物**不是同一字节串**。
4. **`killer_template.dex` 的命名与实际内容不符**（名字像通用模板，实际已烘死 mxyzt 证书）；是否有别处脚本把它当模板用，未逐一核查。
5. 未评估 3.2 是否**真的需要** B 轨：3.2 已用改包法覆盖 4 个 Tap 锚点（`静态_Java_Tap改包_3.2_20261124.md`），killer 属叠加上去的第二道；是否叠加是策略问题，不是资产问题。

## 8. 复现命令（一条）

```bash
cd /e/nixiang && bash 13_scratch/kyzt32_killer/rebuild_kyzt_killer.sh
# 期望尾部：13 个 Lbin/mt/signature/… 类 + "13696 B" + sha256/md5 各一行
```
