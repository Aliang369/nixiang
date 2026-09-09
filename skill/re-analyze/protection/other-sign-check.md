# 其他签名校验（libqkcheck + MT 签名杀手）

> 📍 re-analyze/protection/other-sign-check.md
> 🎯 游戏使用 native 签名校验（libqkcheck）或需要 Java 层签名伪装（MT Killer）

---

## 一、libqkcheck 签名校验

### 识别特征

1. `lib/` 目录存在 `libqkcheck.so`
2. 导出函数 `Java_com_game_apkverify_Check_getSignInfo`
3. 改包后游戏闪退或弹"非官方正版"

### 绕过方法

1. IDA 打开 `libqkcheck.so`
2. 定位 `deal_check_fail` 函数
3. 入口直接 `RET`（nop 掉校验失败处理）
4. 如有硬编码 MD5 校验，将原版 APK 的签名 MD5 回灌

### 验证方法

改包后游戏不闪退、登录不报"非官方正版"。

### 案例：物质与魔法

- `libqkcheck.so` 的 `deal_check_fail` 入口改 RET
- 原版签名 MD5 硬编码回灌

## 二、MT 签名杀手 KillerApplication193/643

### 识别特征

- Application 继承链中有 `KillerApplication193` 或 `KillerApplication643`
- 斌哥壳改包通常自带

### 作用机制

1. 在 Application 静态块中执行 `killPM(包名, <原厂证书 Base64 DER>)`
2. Hook `PackageManager.getPackageInfo` 返回原厂签名
3. **比 SignSpoof 更早执行**（在静态块，SignSpoof 在 `attachBaseContext`）

### 使用方法

1. 提取原版证书 DER：
   ```bash
   keytool -exportcert -jarfile original.apk -rfc > orig.b64
   ```
2. 在 Application 继承链中插入 `KillerApplication193`
3. 传入包名和原厂证书 Base64 DER

### 版本选择

- `643` 比 `193` 新
- 版本不匹配可能不生效
- 使用游戏已有的版本号，不要混用

### 适用游戏

镇邪人、消消英雄2、雷霆战机、音舞光年等斌哥壳改包游戏。

## 三、与 SignSpoof 的关系

| 维度 | MT Killer | SignSpoof |
|------|-----------|-----------|
| 执行时机 | 静态块（最早） | `attachBaseContext` |
| 覆盖范围 | Java 层 PM 查询 | Java 层 PM 查询 |
| 外部进程 | 无效 | 无效 |
| 实现方式 | 替换 Application | Hook PackageInfo.CREATOR |

两者功能重叠，一般用其一即可。MT Killer 更早更彻底，SignSpoof 更灵活可配置。

详见 [login/sign-spoof.md](../login/sign-spoof.md)。

## 坑/弯路

- Killer 只过 Java 层 PM 签名查询，**native 层自检仍需另过**（如 ACE/tersafe）
- 643 和 193 不要混用
- 提取原版证书时确保用原版 APK（未改签的）
