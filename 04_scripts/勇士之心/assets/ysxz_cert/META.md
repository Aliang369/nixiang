# 勇士之心 1.0.6 官方签名证书（TapTap 渠道）

来源 APK：02_apks/勇士之心_1.0.6_official.apk
          65,531,496 B / md5 c06a1c1e3cc4f57e4d9ab29ecd9becfa / versionCode 6

## 指纹（2026-11-23 实测）

| 项 | 值 |
|---|---|
| Subject DN | CN=shan, OU=bingo, O=bingo, L=guangzhou, ST=guangdong, C=CN |
| DER 长度 | 877 B |
| SHA-256 (证书 DER) | de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a |
| SHA-1 | 33e2c8612b3efbdbb1d0d5b18b7fefbb8c9cc05d |
| MD5 | 79a6a62f1c189d911b2dff8324be384b |

- 与 `04_scripts/勇士之心/official_signature.txt` 记录一致；
- 与 libsecret.so `.data` VA 0x66aed 内嵌的期望值逐字节相同（见 _work_20261123_壳可行性.md E4）；
- 与旧 SignSpoof 日志 `spoof hit md5=79a6a62f…` 同一个证书。

## 复现命令（两条互补）

### A. apksigner 交叉核对（只需证书信息，不导出 DER）

```bash
"C:/Users/Administrator/AppData/Local/Android/Sdk/build-tools/35.0.0/apksigner.bat" \
  verify --print-certs "02_apks/勇士之心_1.0.6_official.apk"
# Signer #1 certificate SHA-256 digest: de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a
```

### B. 解析 APK Signing Block (v2) 导出 X.509 DER（本目录三份文件的生成方式）

```bash
python3 04_scripts/勇士之心/assets/ysxz_cert/extract_v2_cert.py \
  --apk 02_apks/勇士之心_1.0.6_official.apk \
  --out-dir 04_scripts/勇士之心/assets/ysxz_cert \
  --expect-sha256 de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a
```

产物：
- `ysxz_official_cert.der` —— 877 B X.509 DER（喂给 killer 的原始材料）
- `ysxz_official_cert.b64` —— 单行 base64
- `ysxz_official_cert.b64.wrapped` —— 每 64 字符换行的 base64（smali 字符串字面量形态）

为什么必须从 v2 签名块取：本包是**纯 v2 签名**（无 META-INF/*.RSA），
`unzip -p ... META-INF/XX.RSA` 拿不到证书。
