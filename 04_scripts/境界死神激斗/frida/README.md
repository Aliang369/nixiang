# 境界死神激斗 · Frida 探针（04_scripts/境界死神激斗/frida/）

2026-10-02 第二阶段逆向时现场写的探针，收尾时从 `/tmp/ssjd2/` 转存入库，供后续复看/复用。
**这些是取证工具，不是出包链路的一部分**；正式包不依赖它们。

## 文件

| 文件 | 用途 |
| --- | --- |
| `fs_memscan.js` + `run_memscan.py` | 内存扫描驱动：按 `log/scan/pe` 等 payload 分发（**通用 `[pay] <json>` 分支必须保留**，否则会「done=true 但无输出」） |
| `fs_dumpdll.js` | 从内存 dump 热更 DLL（`cand0_…_notnet.bin`），离线分析用 |
| `fs_loadhook.js` | 定位 `Assembly.Load(byte[])` 调用点，产出 RVA + 序言字节（native hook 的锚点来源） |
| `fs_prologue.js` | 读函数入口序言，用于 hook 前的字节比对门 |
| `fs_globals.js` | 枚举 il2cpp 全局导出/符号 |
| `fs_luadump.js` + `run_luadump.py` | Lua 模块导出（`libxlua.so`） |
| `fs_checkpatch2.js` | 复核补丁程序集是否真的进了运行时（找 `Starry.StarryConfig`） |
| `fs_cfgprobe.js` | 端到端验配置通道：写 `starry_config.ini` → 调 `StarryConfig.GetBool/GetInt` 回读 |
| `fs_singleton.js` | 枚举 `Singleton<T>` / `LevelManager` / `GameManager` / `BattleMgr` 的 `get_Instance` 与静态字段数据 |

## 三条硬约定（每条都对应一次真实的崩溃/空转）

### 1. 导出解析：禁用 `Module.findExportByName` 静态形式，且失败绝不能流进 `NativeFunction`

Frida 17 移除了 `Module.findExportByName`；解析失败返回 `null` 后，`new NativeFunction(null, …)`
会以 `access violation accessing 0x0` 的形式崩掉——**看起来像目标进程的 bug，其实是探针自己的**。

- 用 `Module.getGlobalExportByName` / `Module.findGlobalExportByName` / `module.getExportByName` 之一，
  且必须先 `typeof x === 'function'` 守卫；
- 解析不到就打印缺哪个符号并**中止**，不要继续构造 `NativeFunction`。
- 现成实现见 `fs_singleton.js` 顶部的 `resolveExport()` / `resolveApiName()`（多通道回退，可整段照抄）。

### 2. 开头必须自检打印

第一件事打出一行环境自检，避免「一声不响地崩」：

```js
send({t:'log', m:'[env] frida=' + Frida.version + ' | 导出解析走 ' + resolveApiName(mod)});
send({t:'log', m:'[env] 导出 ' + ok + '/' + total + ' 可用' + (missing.length ? '，缺: ' + missing.join(',') : '')});
```

### 3. 调 il2cpp 之前必须 `il2cpp_thread_attach(il2cpp_domain_get())`

Frida 线程不在 IL2CPP 的线程表里，直接调解释器/Runtime API 会在访问 TLS 时崩
（典型 `Error: access violation accessing 0x10`）。**唯一例外**是纯反射式枚举（
`il2cpp_image_get_class*` / `il2cpp_class_get_*`），但统一 attach 更省心。

```js
var d = api.domain_get();
api.thread_attach(d);   // ★
```

## 其它踩过的坑

- **`Memory.readByteArray` / `Memory.writeByteArray` 在 Frida 17 已移除**：改 `ptr(addr).readByteArray(n)` / `ptr(addr).writeByteArray(buf)`。
- **macOS 没有 `timeout` 命令**：限时不要写 `timeout 30 frida …`；用 python 驱动 + `subprocess` 超时，或脚本内 `setTimeout` 自己收尾。
- **`il2cpp_runtime_invoke` 的 `bool` 返回是装箱对象**：值在 `r.add(0x10).readU8()`，不是返回值本身。
- **`libxlua.so` 不导出 `luaL_loadbuffer`**（只有 `luaL_loadbufferx`）；hook `luaL_loadfilex` 会把 `mode` 当 size 读，产出 `<bad>`/`sz=0` 噪音 ⇒ 只 hook `luaL_loadbufferx`。

## 运行方式

```bash
FRIDA=/Users/aliang/Library/Python/3.11/bin/frida          # frida 17.16.4 client，server 在 27042
export PATH="$HOME/.local/bin:$PATH"
"$FRIDA" -U -f com.cmge.ssjd.x7sy -l fs_singleton.js --no-pause        # spawn 方式（要抓启动期）
"$FRIDA" -U -p "$(adb shell pidof com.cmge.ssjd.x7sy)" -l fs_cfgprobe.js  # attach 方式
```

Python 驱动探针（`run_memscan.py` / `run_luadump.py`）用 `SPAWN=1` 环境变量切 spawn 模式。
