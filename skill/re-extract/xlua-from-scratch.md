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
  否则会覆盖到下一个函数首部（短函数如 `lua_setglobal` 只有 12 B，hook 必 SIGILL）；
  逐个符号的 `size` 必须照下表口径实测，不猜：

  | 符号 | 看什么 | 判据 |
  |------|------|------|
  | `lua_pcall` / `lua_gettop` / `luaL_loadbuffer` 等导出符号 | `nm -D -S` 的偏移与 size | `size ≥ 16` 才可 hook；恰好 16 时确认下一符号起点不重叠 |
  | `lua_pcallk` 一类 | 是否出现 | 未导出 ⇒ 不走它 |

  > 先例：伊莫 `libxlua.so` 的逐符号实测表（偏移/size/可否 hook）见 `10_docs/伊莫/技能先例.md` §1。

- **ELF 取文件字节要减 LOAD2 的 `p_vaddr - p_offset`**：先看程序头算出该段的
  `vaddr - fileoff` 差值，再按差值换算；直接按 vaddr 读文件会读到别的地方
  （先例与实测差值见 `10_docs/伊莫/技能先例.md` §1）。
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
7. **诊断脚手架一律复用现成机制，禁新造平行宏**（2026-10-02 消消英雄2 教训）：
   统一走 `jni/ModDiag.h`（`DLOG/DLOGN/DWARN` + `-DSTARRY_DIAG=1` 编译期整体关掉）
   与运行时 `/sdcard/.starry_magic/logcfg` 上限；运行时解密 dump 的开关（§十二）
   也挂在同一宏体系下，不要再造 `XXX_DUMP_TRACE` / `XXX_DUMP_ALL` 这类平行宏
   ——本次为此重造了一套日志封顶与开关，纯属重复劳动。

---

## 十、RPC 重发式「伤害放大」（服务端权威游戏）

数值类需求（伤害/倍率）在服务端权威游戏里**不能改客户端计算**，只能
「让客户端把同一份上报多发几次」（先例：伊莫 斌哥 `MOD.dmgMul`，细节见
`10_docs/伊莫/技能先例.md` §2）：

- 抓「C→S 上报」入口：实体类的 `wrapServerMsg`，在 `RPC_CS_*` 里按白名单重发 N 次；
- **白名单必须只放「不带实例身份」的指令**：携带实例 id 的指令服务端只认首次命中，
  重发纯属浪费网络；筛选判据＝该指令参数里有没有「本次行为的实例 id」；
- **先确认哪个实体才是主输出通道**：人物本体与宠物/召唤物常走不同上报通道
  ⇒「倍率对人物没效果」可能是通道决定的，不是 hook 失败；先用诊断确认通道再改代码；
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

---

## 十二、运行时解密 Lua dump + 定制头 Lua 5.3 字节码解析（2026-10-02 消消英雄2 1.2.27）

> 适用：静态面**拿不到** Lua（`assets/*` 里没有 `.lua`、中文串零命中、打包产物是哈希名 bundle），
> 或需要**读判定逻辑**（不只是写开关）时。伊莫走的是「写开关 + 重发」；这一节是「读懂」那条路。

### 12.1 第一步永远是「先看头，再选工具」

```bash
xxd -l 32 m_xxx.lua.bin        # ← 先看这 32 字节，别急着找 luac / 反编译器
```

**定制头格式（消消英雄2 实测，与标准 5.3 的差异就是这些）**：

| 字段 | 值 | 标准 5.3 |
|---|---|---|
| magic | `1b 4c 75 61`（`\x1bLua`） | 同 |
| version | `0x53` | 同 |
| **format** | **`0x01`** | `0x00`（⇒ `luac -l` 必报 `format mismatch in precompiled chunk`） |
| 校验串 | `19 93 0d 0a 1a 0a` | 同 |
| **size 字节** | **4 个 `04 04 08 08`** | 5 个 `04 08 04 08 08` |
| `LUAC_INT` / `LUAC_NUM` | `0x5678`(8B) / `370.5`(8B) | 同 |
| 头长 | **32 B**，body 从 `0x20` 起 | 33 B |

- body 起始＝`sizeupvalues`(1B) + source 串（1B 短长度，`0xFF` 转 8B 长长度）。
- code 段前是 **4 字节 `int32` 计数**（标准 5.3 是 size_t）；指令 4B。
- 常量 tag：`0`nil `1`bool `3`float8 `19`int8 `4`短串 `20`长串。
- `sBx` = `GETARG_Bx - 131071`（`MAXARG_sBx = MAXARG_Bx>>1`）。
- class 方法表编译形态：`CLOSURE R -> proto#N` 紧邻 `SETTABLE R R "名字"`
  ⇒ **名字行 k 对应 CLOSURE 行 k-1**（按 k+1..k+3 找会整体错位，本次踩过一轮）。

### 12.2 opcode 序「未混淆」自证（别凭记忆抄 5.3 表）

```bash
nm -D -S lib/arm64-v8a/libxlua.so | grep -i opname    # 未 strip 时 .symtab 里就有
```
消消英雄2 实测：`luaP_opnames`@`0x666e8`（384 B = 48 指针）+ `luaP_opmodes`@`0x10888`（47 B），
`.rela.dyn` 提供 addend ⇒ 读出的名字序 = 标准 5.3（`0 MOVE … 44 CLOSURE 45 VARARG 46 EXTRAARG`）。
**5.3 第 25 位是 `UNM`，不是 BNOT**（漏掉会全表错位）。

### 12.3 工具（已入仓，别再重写）

```bash
python3 04_scripts/_common/lua/lua53_dis.py m_xxx.lua.bin > dis.txt   # 整模块反汇编（解析完必须 consumed == total）
python3 04_scripts/_common/lua/lua53_fn.py dis.txt <函数名>            # 从反汇编里提某个函数的块
python3 04_scripts/_common/lua_struct_check.py --disasm m_xxx.lua.bin   # 同一解析器的体检入口
```

> 工具住 `04_scripts/_common/lua/`（**入库、两机可复用**）。**别放 `01_tools/`** —— `.gitignore:23` 忽略整个
> `01_tools/`（本地重装麻烦的工具不进 git），2026-10-02 曾先落那里、提交前才发现不入库。
> 选落点目录前先 `git check-ignore -v <路径>` 确认。

### 12.4 dump 怎么来（静态面无 Lua 时）

- 在 mod so 里挂**编译期可整体关掉**的 dump：`#if XX_LUA_DUMP_TRACE` + `XX_DUMP_ALL`，
  目录 `/sdcard/Android/data/<pkg>/files/starry_lua_dump`，
  文件名 `m_<fnv1a32(源路径)>_<len>.lua.bin`（同名 `.path` 仅
  `SignatureLoader.load_and_verify` 路径才有）。
- 模块内嵌**源路径串**（如 `Assets/Application/Scripts/Lua/Application/Fight/View/GameViewUpperUI.lua.txt`）
  ⇒ 用它反查模块用途；中文 UI 文案**不在 Lua**（在 C#/文本表侧，别在 Lua 里死找）。
- **dump 收窄纪律（2026-10-02 新增）**：先按模块路径白名单（如 `Fight|Input|UserData`）收窄
  `ShouldDumpModule()`，再决定要不要全量；全量 1102 模块 ≈ 16 MB，只有 `grep` 价值。
  大产物一律落 `/tmp`，只把「候选模块清单 + 命中函数名」写回上下文（别把 dump 内容贴进对话）。
- 日志量必须封顶（`n <= 400 || ShouldDumpModule(name)`、`n <= 400 || (n % 500) == 1`），
  否则刷屏丢事件；上限优先走运行时 `logcfg`（§九.4）。

### 12.5 读出门控之后怎么落地

判定类需求（「解锁某功能」）**不要改数值、不要替换逻辑**，见
`re-implement/unlock-pattern.md` **模式三**：幂等覆盖判定函数（`rawget` 守卫 + `AN_x` 为真时恒真），
开关关掉逐帧回退原判定。
