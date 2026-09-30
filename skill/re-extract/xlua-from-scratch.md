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

---

## 六、零 il2cpp 注入路线（玩法 100% 在 Lua 时首选）

伊莫 1.0.1 实测结论：**完全不需要碰 IL2CPP**。理由是 xLua 的 C 实现
`libxlua.so` 未 strip、`dynsym` 齐全，比在 170 MB 的 `libil2cpp.so` 里找
`LuaManager`/`LuaEnv` 省事得多，也避开了 il2cpp 元数据加密与各类保护。

| 步骤 | 做法 | 要点 |
|------|------|------|
| ① 拿 Lua C API | `dlopen("libxlua.so")` + `dlsym` 取 `lua_pcall` / `lua_gettop` / `luaL_loadstring` / `lua_tolstring` / `lua_settop` | **不要抄斌哥壳的硬编码偏移**（`base+0x010a41a8` 那种）：换版本会静默错位。壳用 `adrp+add` 求地址是因为它懒得 `dlsym`，不是更优 |
| ② 拿 `lua_State*` | Dobby inline hook `lua_pcall`（可再带一个 `lua_gettop` 兜底） | hook 回调的入参 ① 就是 `lua_State*`，且**天然运行在调用 Lua 的那个线程 = 游戏主线程**，不用去 IL2CPP 找 LuaManager |
| ③ 注入补丁 | 在 hook 里 `luaL_loadstring(L, patch)` + `lua_pcall` | 整段 Lua 补丁字符串注入；补丁自身要守卫幂等（见 §七） |
| ④ 传开关 | 同一通道把菜单开关写进 Lua 全局表 | 用 `rawset`（见 §七） |

优点：不碰 IL2CPP、不改 dex 逻辑、换域名/热更不动 Lua 也能重注入。

---

## 七、Lua 全局表是「白名单式」—— 一律 rawget/rawset ★最容易翻车的一条

xLua 的 `LuaEnv.Global` 常带自定义 `__index`（伊莫实现于
`Core/Framework/Global.lua:106 __index` → `Global.lua:73 __logGlobalError`
→ `Global.lua:40`）：

- **裸读一个未声明的全局 ⇒ 返回 `nil` 且记一条 error，不抛异常**；
- 于是 `if YM then YM.dmg = 100 end` 这种写法**永远为假**，补丁「装上但功能不生效」，
  logcat 里还看不出问题（只会看到 `undeclared global variable` 噪音）；
- `Y = Y or {}` 同样会踩：它先裸读 `Y` ⇒ error + nil ⇒ 每次都新建表 ⇒
  写进去的东西下一个 chunk 读不到。

**铁律**：补丁里所有全局读写走 `rawget(_G, k)` / `rawset(_G, k, v)`：

```lua
local G = _G
local rg, rs = rawget, rawset
local Y = rg(G, "YM"); if type(Y) ~= "table" then Y = {}; rs(G, "YM", Y) end
Y.dmg = 100
```

自检：`adb logcat -d -s Unity:V | grep -c "undeclared global variable"` 必须为 **0**。

---

## 八、符号 / 偏移 / hook 尺寸（实测数据，别猜）

- 看符号尺寸：`nm -D -S libxlua.so`（`readelf` 在部分 NDK 环境不可用时就用它）。
- **Dobby 在 arm64 上 inline patch 宽 16 字节 ⇒ 只 hook 本体 ≥ 16 字节的入口**，
  否则会覆盖到下一个函数首部（伊莫实测 `lua_setglobal` 只有 **12 B**，绝不能 hook⇒SIGILL）：

  | 符号 | 偏移 | size | 可否 hook |
  |------|------|------|-----------|
  | `lua_pcall` | `0x1096dac` | `0xcc` | ✅ |
  | `lua_gettop` | `0x1090be0` | `0x10` | ✅（恰好 16，下一符号 `lua_settop` 在 +16，不越界） |
  | `luaL_loadbuffer` | `0x10a41a8` | — | （序言 `d103c3ff` 与斌哥壳指纹一致） |
  | `lua_setglobal` | `0x9a8ed4` | 12 | ✗ 太短 |
  | `lua_pcallk` | — | — | 未导出 |

- **ELF 取文件字节要减 LOAD2 的 `p_vaddr - p_offset`**：伊莫 `libxlua.so` 是
  `vaddr = fileoff + 0x4000` ⇒ 想用反汇编里的地址去文件里对照，先 `-0x4000`。
  直接按 vaddr 读文件会读到别的地方（本次踩过：三处全对不上）。
- 反汇编定位引用时，`adrp+add` / `adrp+ldr` 要**从 `.text` 起始偏移开始反汇编**
  （从文件 0 开始会因 ELF 头错位，一条都对不上）。

---

## 九、诊断与日志（xLua 专用）

1. **游戏自带 Logger 常常不落 logcat**（伊莫 `Core.Log.LoggerManager` 就是），
   Lua 侧 `print` 也常被重定向。可行的三条通道：
   ① `io.open("/sdcard/Android/data/<pkg>/files/xxx.log","a")`（最可靠，需文件权限）；
   ② `rawget(_G,"print")`；③ 游戏 logger（最后兜底）。
2. **`adb logcat -s <tag>` 是前缀匹配**：`-s Yimo:V` 永远匹配不到打印成
   `[Yimo] xxx` 的日志（TAG 实际是 `[Yimo]`）⇒ 一律
   `adb logcat -d | grep -E "\[Yimo\]|Mod_Menu|MenuService"`。
3. **诊断代码必须编译期可整体关掉**（伊莫为此做过一轮 115 行/35 处手术拆除）：
   native 走 `jni/ModDiag.h` 的 `DLOG/DLOGN/DWARN`（正式包展开成 `((void)0)`），
   Lua 走 `jni/ModDiagLua.h` 的独立诊断 chunk（正式包 `MOD_DIAG_LUA_TEXT == ""`）；
   诊断包 `STARRY_DIAG=1` 构建、产物名带 `-diag`，与正式包并存不覆盖。
4. **日志上限不要写死**：放设备上的运行时配置（如
   `/sdcard/.starry_magic/logcfg`，一行一个 `tag=上限`，负数=不限），
   改上限重启游戏即可，别为把 5 改成 24 重建重装一轮。
5. 诊断每行带序号（`[diag][tag]#n`）：字段恒为常数时，先怀疑诊断本身没命中，
   而不是先怀疑功能没生效。诊断启动时打一行自检（把 `io.open` 路径、目标表是否存在
   打出来）——「没装上」和「没命中」是两回事。
6. 正式包门禁：`bash 04_scripts/_common/check_release_clean.sh --tag <Tag> --so <so>`
   （源码块感知 + so 二进制双重检查），`surgical_*.sh` 已内置调用。
   内嵌 Lua chunk 的结构体检：`python3 04_scripts/_common/lua_struct_check.py`。

---

## 十、RPC 重发式「伤害放大」（服务端权威游戏）

数值类需求（伤害/倍率）在服务端权威游戏里**不能改客户端计算**，只能
「让客户端把同一份上报多发几次」（伊莫 斌哥 `MOD.dmgMul` 的做法）：

- 抓「C→S 上报」入口：实体类的 `wrapServerMsg`（伊莫挂在
  `ClientMainPlayer / ClientPet / ClientPuppet / ClientCreation` 四个类上），
  在 `RPC_CS_*` 里按白名单重发 N 次；
- **白名单必须只放「不带实例身份」的指令**：伊莫实测
  `RPC_CS_ProjectileHitTarget` 第一个参数是投射物实例 id（且紧跟一条
  `RPC_CS_ProjectileDestroy` 用同一个 id），服务端只认首次命中 ⇒ 重发 55 次纯属浪费网络；
  可放大的是 `RPC_CS_DoActOnActorActions` / `RPC_CS_DoActOnSweepTargetsActions`
  （`actionData.NodeID` 不自带身份）；
- 伊莫结论：**宠物（ClientPet）才是主输出**，人物本体基本走投射物通道 ⇒
  「伤害倍率对人物没效果」不是 bug，是通道决定的；
- 别忘了「重发预算/秒」上限与运行时开关，别把网络打爆。

---

## 十一、坑清单

| 坑 | 现象 | 处理 |
|----|------|------|
| 全局表白名单式 `__index` | 开关写了不生效、日志里只有 `undeclared global variable` | 全改 `rawget/rawset`（§七） |
| `adb logcat -s <tag>` | 明明有日志却 `-s` 过滤不到 | 前缀匹配问题，改 `grep`（§九.2） |
| 一次性安装守卫 | 游戏换场景/换战斗重建类表后，包装静默丢失 | 守卫式「每次同步都重装」，且**补丁与写开关必须是两个独立 chunk**（补丁命中守卫会提前 `return`，把写开关吃掉） |
| `luaL_loadstring` 每次 Lua C 调用都跑一遍 | 单核 CPU 飙高、帧率隐患 | 只在需要时（首次/菜单变更/节流 100 ms）注入；补丁 19 KB 重解析约 470 次/秒是不可接受的 |
| 注册表/对象接口返回**共享引用** | 传送后读位置得到的是落点 ⇒ 判据恒假、自适应逻辑永不触发 | 当值用之前先取标量快照（伊莫 `getPosition()` 返回共享 `posRef`，`ClientPosRotComponent.lua:112`） |
| 重签名换 CN | `INSTALL_FAILED_UPDATE_INCOMPATIBLE`（原版 CN=FUNPLUS vs 改包 CN=StarryMagic），且 TapTap SSO 必然失败 | `-r` 装不上时只能卸载重装；**带 `-k` 可保住数据**（`pm uninstall -k --user 0 <pkg>` 实测保留 `/sdcard/Android/data/<pkg>`，伊莫第一轮没带 `-k` 丢掉 12.98 GB 热更数据重下）；登录路径强制网页登录 |
| 官方包缺 `libc++_shared.so` | 注入的 so `dlopen` 失败 | 探测并在打包时注入（伊莫：官方无此库，stripped 1,292,896 B） |
