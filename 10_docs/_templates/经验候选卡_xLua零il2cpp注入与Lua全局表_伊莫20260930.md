# 经验候选卡：xLua 零 il2cpp 注入 + Lua 全局表铁律

> 来源：伊莫 `com.x.yimoo.official` 1.0.1/vc4（Unity + xLua，官方包**零 il2cpp**）2026-09-30
> 状态：**候选**（未审批，未改 `skill/`）；审批后并入 `skill/re-extract/xlua-from-scratch.md`（现 68 行，本文全部内容都还没有）

## 一、符号与可钩点（`libxlua.so`）

| 事实 | 值 |
|---|---|
| 符号读取 | **`nm -D` 可读；`readelf` 读不到**（先试 `nm -D`，别在 `readelf` 上耗） |
| 可钩点 | `lua_pcall @0x1096dac` size `0xcc` ✅；`lua_gettop @0x1090be0` size `0x10`（**＝Dobby 16 字节下限，刚好**）✅ |
| 钩不了 | `lua_setglobal @0x9a8ed4` size **12 B**（< 16 B）；`lua_pcallk` **未导出** |
| 指纹 | `luaL_loadbuffer @0x10a41a8` 序言 `d103c3ff` |
| load 偏移 | **`fileoff = vaddr − 0x4000`**（LOAD2；直接按 vaddr 读文件字节必错位） |

## 二、注入方式

**不是** il2cpp 方法 patch，而是：Dobby 钩 C API 拿到 `lua_State*` ⇒ `luaL_loadstring` 加载**整段 Lua chunk**（本例 ≈19 KB）⇒ `lua_pcall` 执行。
- 参数用 `lua_gettop` 取，字符串用 `lua_tolstring`（注意 `size_t*`）。
- **每次 Lua C 调用都重新装载 19 KB chunk = ~470 次/秒，CPU 白烧**；必须「`g_patched` 标志 + 失败时最多每 500 ms 重试一次」，并把状态同步节流到 100 ms。

## 三、Lua 全局表是白名单式（最容易静默失效的坑）

`Core/Framework/Global.lua:106 __index` → `:73 __logGlobalError` → `:40`：
**裸读未声明的全局返回 `nil` 并记一条 error，不抛异常。**

- ❌ `if YM then YM.dmg = 1 end` —— 永远 false，**功能静默不生效，日志里只有 error 行**；
- ✅ 一律 `rawget(_G,key)` / `rawset(_G,key,val)`；**永远不要写 `Y = Y or {}`**；
- 判据：`adb logcat -d | grep -c "undeclared global variable"` **必须为 0**。
- 诊断计数器放 `_G.__YM_N` 之类，避免污染游戏命名空间。

## 四、Lua 侧看到的是 C# 对象的**活引用**

`pawn.getPosition()` 返回**共享的** `self.posRef`（`ClientPosRotComponent.lua:112`）：`forceSetPosRot` 传送成功后就地改写它 ⇒ **传送之后再读 `myPos.x/z` 读到的是落点**。
后果：诊断里「计划距离」恒 `0.0m`、`moved < 2` 判据恒假 ⇒ 整条「弹回原地」逻辑是**死代码**，却被当成证据用了一轮。

**铁律**：① 返回值当值用之前**先拷标量快照**（`local mx,mz = myPos.x,myPos.z`），**读的时机就是语义的一部分**；② **诊断字段恒为常数 = 诊断本身可能错**，先怀疑诊断再改功能。

## 五、重发型伤害放大的生效条件（跨游戏可复用）

服务端只认**一条上报的实例首命中** ⇒ 只有「RPC 里没有实例身份」的通道才放大得动：

- ✅ 精灵 `RPC_CS_DoActOnActorActions`（无实例身份；实测重发 55 次真生效。本作主输出是宠物）；
- ✗ 人物 `RPC_CS_ProjectileHitTarget`（首参 = projectile id，且紧跟 `RPC_CS_ProjectileDestroy` 同一 id ⇒ 重发 55 次纯浪费带宽）。

⇒ 移植「伤害倍率」类功能前，**先按通道判定实例身份**，别等用户反馈「有效果又没效果」。

## 六、其它环境事实

- 游戏自带 `Core.Log.LoggerManager` **不落 logcat** ⇒ 诊断只能自建文件通道（`/sdcard/Android/data/<pkg>/files/*.log`）。
- `adb logcat -s TAG:V` 是**前缀匹配**：`[Yimo]` 这种带方括号的 TAG **匹配不到**，必须 `adb logcat -d | grep "\[Yimo\]"`。

## 七、拟落点

`skill/re-extract/xlua-from-scratch.md` 新增「六/七/八/九」四节；`skill/re-implement/damage-pattern.md` 补「实例身份判定」小节；`skill/re-lessons/SKILL.md` 索引。
