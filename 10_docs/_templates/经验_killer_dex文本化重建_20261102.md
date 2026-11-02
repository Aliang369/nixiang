---
experience_id: exp-20261102-killer-dex-text-source
class: tech
status: candidate
scope: "Android / TapTap 类验签 B 轨：MT ApkSignatureKillerEx 形态的 killer dex 以「参数化 smali 源码 + 构建机烘焙脚本」入库，替代不可入库的 *.dex 二进制"
source: "10_docs/正中靶心2/星空落地_1.9_20261102_双轨制复刻.md"
evidence: "10_docs/正中靶心2/星空落地_1.9_20261102_双轨制复刻.md"
targets: "skill/re-analyze/login/taptap-web-login.md#现行口径 / 10_docs/_templates/经验_运行时签名替换挂载点与dex等长改包_20261029.md"
approved_by:
approved_at:
approval_ref:
merged_commit:
---

# 经验候选卡：killer dex 的「文本源码化 + 构建机烘焙」可复现方案

## 事实

- 各游戏此前把 MT ApkSignatureKillerEx 的 killer dex 当**二进制资产**放 `04_scripts/{游戏}/assets/*.dex`
  （例：`失落城堡2/assets/slcb2_signature_killer.dex`）。但仓库 `.gitignore` 有 `*.dex`（:160）⇒
  **二进制从未入库**，换机/换人重打时必须回头找原始斌哥包提取，属于「隐式单点依赖」。
- killer 的可变输入只有两项：**包名** 与 **官方证书 DER（base64，MT 写成每 64 字符带 `\n` 的常量）**；
  其余是固定的 MT 源码。因此可把 smali 里这两处替换成占位符 `@@PKG@@` / `@@CERT_B64_WRAPPED@@` 入库。
- 证书 DER 不必外部提供：直接从**官方底包 APK 的 v2/v3 签名块**取第一个证书即可
  （解析 `APK Sig Block 42` → id `0x7109871a`/`0xf05368c0` → signers → signed_data → certificates）。
  实测 SHA-256 与 `apksigner verify --print-certs` 报告值一致（正中靶心2 `57fbc23b…`）。

## 做法（正中靶心2 落地）

1. `04_scripts/{游戏}/assets/zzbx2_killer_smali/`：13 份 smali，**根目录不含 `bin/`**
   —— `.gitignore:121` 的 `04_scripts/**/bin/` 会整目录忽略；构建时再 `cp -R src/. $WORK/smali/bin/` 还原包路径结构。
2. `04_scripts/{游戏}/build_killer_dex_zzbx2.sh`：取底包证书 DER → 校验 SHA-256 → base64 换行烘焙 →
   替换两处占位符 → `smali assemble` → 断言（`.super Landroid/app/Application;`、包名常量、无残留 `@@`）+ **md5 必须等于记录值**。
3. 挂载点不变：`StarryAppComponentFactory.instantiateApplication` 里 `Class.forName(KILLER_CLASS, true, cl)`
   （killer 安装动作全在 `<clinit>`）。

## 反例 / 边界

- 换发行商或换签名证书 ⇒ 记录的 md5 断言会失败（这是**故意的**），按新证书重跑并把新 md5 写进 `ZZBX2_KILLER_MD5`。
- killer 源码本身仍来自斌哥包（MT 工具产物）；本方案只解决「可变参数 + 可复现」，不解决来源合法性/可得性。
- 未在 Windows 侧实跑（本轮只在 mac 上验证）；smali/sha 工具链两机同源，预期一致但属 NOT PROVEN。
