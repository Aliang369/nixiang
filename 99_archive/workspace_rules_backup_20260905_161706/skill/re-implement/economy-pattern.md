# 经济修改模式

> 📍 路径：`re-implement/economy-pattern.md`
> 🎯 触发场景：需要修改游戏货币、实现免支付、免广告等功能

---

## 模式一：货币 XOR 加密修改

**适用游戏**：战魂铭人（Unity IL2CPP / HybridCLR）

### 原理

`EncryptValue` 类有三个字段：`_showValue`、`mValue`、`encryptKey`

校验关系：`_showValue == mValue ^ encryptKey`

### 操作步骤

1. 运行时用 `il2cpp_class_from_name` + `il2cpp_field_get_offset` 解析字段偏移
2. 修改时**必须同步写三个字段**：

```c
void set_currency(void* obj, int64_t value) {
    int64_t key = read_field<int64_t>(obj, encryptKey_offset);
    write_field(obj, mValue_offset, value);
    write_field(obj, _showValue_offset, value ^ key);
    // encryptKey 不变
}
```

### 坑/弯路

- **逐字段写 → 触发 IsLegal()**：校验失败 → `Application.Quit()`。必须三字段一起写
- **AddResource 周期调用会累加**：游戏每帧/每周期调 `AddResource`，如果 hook 里只加不减，数值会一直涨到 1999999 甚至触发风控
- **boxed Int32 误传**：`il2cpp_runtime_invoke` 传 boxed Int32 给 setter → setter 未生效

## 模式二：免支付（Cocos JS）

**适用游戏**：动物餐厅（Cocos Creator JS）

### 操作步骤

1. Hook jsc 解密后的 JS，修改 `PayMentModel` 九个支付渠道：

```javascript
PayMentModel.prototype.pay = function(channelId, orderInfo) {
    return { status: 0, orderItem: { orderId: "fakepay" } };
};
```

2. 修改 `PayMentSystem.changeOrderStatus`：

```javascript
PayMentSystem.prototype.changeOrderStatus = function(orderId, status) {
    return Promise.resolve(true);
};
```

3. jsc 门控：`window.__starryNoIap = true`

4. smali 兜底：`AppActivity.wxPay` 拦截后回调成功

### 坑/弯路

- **只改 PayMentModel 漏 changeOrderStatus** → 验单失败。两个必须一起改
- **paymentNet 直接返回 Promise 不够**：原函数内部还调了 `purchaseSuccess`，必须走完整链路
- **热更会冲掉静态门控（所有热更游戏通用）**：引擎优先加载热更目录（Cocos `files/*-hot-update` 等），APK 内打好门的脚本被整文件替换 → 门控失效。正修按优先级：① 门控打在 Java 层 `StarryApp.attachBaseContext`，抢在引擎加载热更 JS 之前；② 热更落地后重打热更文件，并同步 manifest 的 md5/size 防校验回滚；③ 运行时函数重 wrap 仅作兜底（单独 wrap 不可靠）。用户清数据后热更回归官方原版，门控必然丢失。（实锤：动物餐厅 2026-08-18，`10_docs/动物餐厅/`）

## 模式三：免支付（Unity）

**适用游戏**：战魂铭人（Unity IL2CPP）

### 操作步骤

1. Java 层调用：

```java
PingXXPayAgent.sendPaySuccess(orderId, productId, "");
```

2. Unity 接收：`recvPaySuccess` → C# `IapProductIdEx.OnPurchasesSuccess(productId)` 按商品 ID 发奖

3. Frida 直接调 `sendPaySuccess` 即可触发发奖

### 坑/弯路

- **Java 层有日志 ≠ C# 发奖**：热更版可能不走旧 `recvPaySuccess`，需要确认 C# 层回调链路
- **il2cpp_runtime_invoke 调热更方法会 AV 0x0**：热更方法不能用 runtime_invoke 调

## 模式四：免广告

### Cocos JS 方式

Hook `AppActivity.showRewardAd` / `isRewardVideoADReady` → `callToJs("reward", 0)`

### Unity 方式

Hook `SDKLinker.ADLinker.PlayRewardedVideo` → 直接调 `successCallback`

### Java SDK 方式

Hook `SdkManager.SdkShowAdReward` → `result=true, state=1`

### 验证

点广告奖励入口不跳广告页，直接发奖。

## 附加：云存档改档（mitmproxy）

**适用游戏**：动物餐厅

1. Frida 抓 `getLatestUserKey` 平台密钥
2. mitmproxy 拦截客户端上传
3. 解密 → 修改数值 → 重算签名 → 重加密 → 原链路放行
4. **同时改写 `checkBlackInfo` 的明文上报字段**

### 坑/弯路

- 只改加密上传不改 `checkBlackInfo` → 两侧不一致 → `-2801` 风控
- 平台密钥 30s 过期
- `AddResource` 累加到 200万+ → 触发风控
