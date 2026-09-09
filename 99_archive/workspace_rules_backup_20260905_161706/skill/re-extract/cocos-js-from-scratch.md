# Cocos JS 从零抓点 + jsc 解密

> 📍 路径：`re-extract/cocos-js-from-scratch.md`
> 🎯 触发场景：游戏使用 Cocos Creator JS 引擎（`libcocos2djs.so` + `.jsc` 文件），**没有斌哥改包**，需要自己定位玩法 Hook 点

---

## 一、dlsym 查找 evalString 符号

Cocos JS 引擎的核心入口是 `se::ScriptEngine::evalString`，所有 JS 脚本通过它执行。

### 符号名
```
_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_
```

### dlsym 查找
```c
void *handle = dlopen("libcocos2djs.so", RTLD_NOW);
void *evalString = dlsym(handle, "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_");
```

> ⚠️ 星光 dlsym 同名符号，**勿写死 runtime 地址**。每次启动 so 加载地址不同，必须用 dlsym 动态查找。

---

## 二、Hook evalString → 打印 JS 脚本字符串

```c
DobbyHook(evalString, replacement, &orig_evalString);
// 在 replacement 中打印第1个参数（const char* script）
// 可看到所有执行的 JS 脚本字符串
```

从打印的 JS 中找：
- `get_ret(n)` 调用 → 功能开关映射
- `getAtk` / `damage` / `health` 等关键词 → 伤害/血量逻辑
- 功能开关的 prefs ↔ `get_ret(n)` ↔ 功能映射

---

## 三、BSS 旗标地址定位

1. Hook `get_ret(n)` 的返回处
2. 在返回处追踪 BSS 写入点
3. `get_ret(n)` 读取的 BSS offset = 功能旗标地址
4. 多个 `get_ret(n)` 对应不同 offset，间隔通常 `+4`

---

## 四、jsc 解密

当 `.jsc` 文件被 XXTEA 加密时：

### 步骤
1. 在 `libcocos2djs.so` 中找 `jsb_set_xxtea_key` 调用
2. 提取密钥参数（如 `431FEWJOTe-gecTW`）
3. 解密：`XXTEA(padding=False)` + `gzip` 解压
4. 解密后为可读 JS，包含完整业务逻辑

### 工具
```bash
node 04_scripts/cocos_jsc_decrypt/decode.js <jsc_file> <key>
```

---

## 五、从解密 JS 中提取玩法

解密后的 JS 包含完整游戏逻辑：
- 找功能函数名（`getAtk`、`isCanReleaseSkill` 等）
- 找功能开关变量（`get_ret(n)` 返回值控制分支）
- 找伤害计算路径（`damage`、`reduceHp` 等）

---

## 六、坑/弯路

### JNI evalString 不是战斗主路径
JNI 层的 `evalString` 是 Java→JS 桥调，**不是战斗主路径**。必须 hook `se::ScriptEngine::evalString`（native 层），否则漏掉战斗中的 JS 调用。

### get_ret(n) ≈ prefs "n-1"（1-based JNI）
`get_ret(1)` 对应 prefs 的 `"0"`，`get_ret(2)` 对应 `"1"`，依此类推。JNI 是 1-based，prefs key 是 0-based。

### 斌哥模板复用
不同游戏可能共用同一个 `game_id`（如 1129）。不要假设 `game_id` 唯一对应游戏。

### 不要 Keypatch libcocos2djs.so
memdiff 证明玩法逻辑在 JS 中，不在 so 中。Keypatch so 无效，应该通过 evalString splice JS 来实现功能。

### Cocos Creator 3 差异
Creator 3 用 `libcocos.so`（非 `libcocos2djs.so`），hook 点是 `CocosJavascriptJavaBridge.evalString`，符号名不同。
