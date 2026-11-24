# nhsy2 killer smali（B 轨签名杀手源件，占位符版）

用途：给 `mod菜单/starry-mod-menu/scripts/surgical_nhsy2_2026.11.19.sh` 的 **[4/8] killer dex** 提供源件，
由该步在构建时把占位符替换成 `com.veewo.neonabyss2` + 官方底包证书 DER（base64，76 字符折行）后 smali 汇编成
`classes6.dex`。

- 源：`04_scripts/我在神界刷装备/assets/sjzz_killer_smali/mt/signature/`（13 文件，MT ApkSignatureKillerEx 193 版；
  11 个游戏同源同 md5 `fb91a89fbb9c2f6adac7c7531673c256`），按本脚本 [4/8] 期望的目录层级下沉到 `bin/` 之下。
- 占位符（**不要手工替换**，构建时自动烘焙）：
  - `const-string v1, "@@PKG@@"` → 目标包名
  - `const-string v0, "@@CERT_B64_WRAPPED@"` → 官方证书 DER base64
- 判据（构建后回读）：父类 `android.app.Application`、包名 `com.veewo.neonabyss2`、证书串 base64 解码后 DER md5
  `bbe1da8584fb2fb1017a512b2e99ff21`、无 `@@` 残留。
- 独立烘焙（生成可归档的 `assets/nhsy2_signature_killer.dex`）：`bash 04_scripts/霓虹深渊2/build_killer_dex_nhsy2.sh`
