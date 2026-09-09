# xLua 抓点

> 📍 路径：`re-extract/xlua-from-scratch.md`
> 🎯 触发场景：游戏使用 xLua 引擎（`libxlua.so` + 加密 `.bundle`），玩法逻辑在 Lua 脚本中

---

## 一、识别

| 特征 | 说明 |
|------|------|
| `libxlua.so` | xLua 原生库 |
| `assets/a/*.bundle` | XOR 0x2A 加密的 UnityFS bundle，内含 Lua 脚本 |
| `LuaManager` | C# 侧 Lua 管理器类 |

---

## 二、dump 壳 BSS 找 Lua 脚本

如果游戏有斌哥壳或其他改包壳：
1. dump 壳 BSS 段
2. 搜索 Lua 脚本片段（如 `CFightSystem`、`AN_1` 等关键词）
3. 壳可能缓存了完整 Lua 脚本明文

---

## 三、功能开关 = Lua 全局变量

xLua 游戏的功能开关通过 Lua 全局变量实现：

```c
// 设置全局变量为 true = 开启功能
lua_pushboolean(L, 1);
lua_setfield(L, -2, "AN_1");  // AN_1 = 第1个功能
lua_pushboolean(L, 1);
lua_setfield(L, -2, "AN_2");  // AN_2 = 第2个功能
```

- `AN_1` / `AN_2` / `AN_3` 对应不同功能
- 原始 Lua 脚本中通过 `AN_1` 等变量做功能分支判断

---

## 四、Hook LuaManager.Update → DoString 安装 monkey-patch

### 流程
1. Hook `LuaManager.Update`（C# 侧，主线程每帧调用）
2. 在 Update 首次命中时执行 `LuaEnv.DoString` 注入 monkey-patch Lua 代码
3. patch 代码设置 `AN_1=true` 等全局变量
4. 确保只注入一次（用标志位）

### 优势
- 不需要解密 bundle
- 不需要修改原始 Lua 脚本
- 运行时动态注入

---

## 五、坑/弯路

### bundle 为 XOR 0x2A 的 UnityFS
`assets/a/*.bundle` 是 XOR `0x2A` 加密的 UnityFS 格式，**不能直接解包改包内 Lua**。需要先 XOR 0x2A 解密 UnityFS 头，再解包，改完还要重新加密。推荐用 DoString monkey-patch 方式绕过。

### 不要直接改 bundle 内 Lua
即使解密了 bundle，重新打包加密后可能校验失败。运行时 DoString 注入是更可靠的方式。

### 确认 Hook 时机
必须在 Lua 脚本加载完成后再 DoString 注入，否则全局变量会被后续脚本覆盖。Hook `LuaManager.Update` 能确保在主循环中注入。
