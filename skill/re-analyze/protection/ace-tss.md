# 腾讯 ACE/TSS 保护

> 📍 re-analyze/protection/ace-tss.md
> 🎯 游戏使用腾讯 ACE（tersafe2/tprt）保护，这是最危险的保护，改包后秒杀进程

---

## 识别特征

1. `lib/` 目录同时存在 `libtersafe2.so` 和 `libtprt.so`
2. `AndroidManifest.xml` 中 Application 为 `AceApplication` 或其子类
3. `<clinit>` 中有 `loadLibrary("tersafe2")` / `loadLibrary("tprt")`
4. **关键**：`libil2cpp.so` 和 `libunity.so` 的 `DT_NEEDED` 依赖 `libtprt.so`

## 校验链路

```
attachBaseContextShell
  → AceApplication.initialize (native)
    → 读 APK sourceDir 算哈希
    → native 完整性校验
    → 不通过 → SIGBUS (1.5s 内)
```

ACE 在 native 层校验 APK 完整性，不仅是签名，还包括文件哈希。

**Manifest Application 类名也在校验链上**（终末地实锤）：把 Application 换成 StarryApp → ACE 毒丸（黑屏挂起，不是当场死，最难排查）。注入点必须走 **ContentProvider（`StarryInitProvider`）**，官方 Application 类名原样保留——写进 StarryApp 的初始化是死代码。

## 死法

- `apktool b` 整包重建 → **必死**（SIGBUS 1.5s）
- 即使零改动只重签 → **可能死**（取决于 ACE 版本）
- MT 去签（Killer643）只过 Java 层 → native tersafe 仍杀
- 干净机上 `apktool b` 全量重建也会 SIGBUS（非签名问题，是文件哈希变了）

## 绕过候选（风险高→低）

### 候选 1：替换 libtprt.so stub（风险最高）
- 编写一个空壳 `libtprt.so`，export 相同符号但不做校验
- 风险：`libil2cpp.so` DT_NEEDED 依赖 `libtprt.so`，如果符号不匹配会链接失败
- 需要完整模拟所有 export 符号

### 候选 2：patch 自杀点（风险中）
- 在 `libtersafe2.so` 中定位校验失败后的自杀代码
- patch 跳转跳过 SIGBUS
- 风险：ACE 可能有多个自杀点，漏一个就挂

### 候选 3：改 DT_NEEDED（风险较低）
- 修改 `libil2cpp.so` 和 `libunity.so` 的 ELF，移除对 `libtprt.so` 的 DT_NEEDED 依赖
- 替换 `libtprt.so` 为 stub
- 风险：如果 `libil2cpp.so` 运行时调用了 `libtprt.so` 的函数，会 dlsym 失败

> **实战正解**（终末地/公主连结/雷霆三款验证）：以上三个候选都没走。正解 = **外科 zip 组装**（官方字节原样保留、只新增不修改）+ 注入 so 运行时 hook + ContentProvider 注入点 + 会话结束清理。

## 设备残留污染问题

**ACE 会检测设备上的逆向工具残留，导致误判：**

- frida-server 残留 → ACE 判定有注入
- ptrace 残留 → ACE 判定有调试器
- KSU umount 残留 → ACE 判定有 root 框架

**每次会话结束必须清理：**
```bash
./04_scripts/_common/cleanup_device_re_residue.sh
```
清理 `/data/local/tmp/frida-server`、`/tmp/*`、KSU umount 状态。

**ACE 残留会污染其他腾讯游戏的判断**，即使换游戏也会受影响。

## 案例：终末地（TP 符号隐藏三件套，2026-09 实战破解）

TP runtime 对 libil2cpp.so 做三层手脚，`dlsym`/`xdl_sym`/readelf/IDA 全部失效：

| TP 动的手脚 | 表现 | 破解 |
|------|------|------|
| st_name 改成字符串**终止符**偏移 | 标准工具读到空名 | 从 `strtab+st_name` **回扫到上一个 NUL** 取真名 |
| st_value 真址存 b3~b6 **大端**（b7=已处理标志） | 直读地址非 4 对齐、必崩 | `addr = base + __builtin_bswap32((st_value>>24)&0xFFFFFFFF)` |
| GNU hash 不重建 + 条目重排 | hash 查找失效 | 线性扫全表，建 name→addr map |

**破解落地 = 进程内 dynsym 自解析**：`/proc/self/maps` 拿 base → PT_DYNAMIC → DT_SYMTAB/DT_STRTAB → 线性扫 → 4344 符号全恢复 → 按名 hook 零改动。解析必须 SafeProbe 包裹（`SetScanThread` 当前线程）+ d_ptr bias 兜底（PT_LOAD 范围外而 +base 落内则补 bias）。

其他实锤：

- **磁盘偏移一律不可用**：磁盘 .text 有 TP 加密岛（高熵，运行时 DT_INIT 解密），磁盘推导地址 hook 进去 = SIGILL 闪退；运行时 dynsym 地址本身可用（TP 只加垫片位移）
- **`il2cpp_domain_get_assemblies` 被 TP 整条删除** → strtab 明文反查 + `il2cpp_domain_assembly_open` 按程序集名逐个开（11 个）兜底
- **官方 dex 禁 patch**：baksmali 改 TapSDK smali 重组装 → 1.2s 闪退（进程都起不来）；回滚官方 dex 原样即正常
- 签名/强制 TapTap 网页登录走运行时 hook：`SignSpoof.install()`（hook PackageInfo.CREATOR 回灌官方证书）+ `TapLoginForceWeb`（loginHandlers 换 CloudLoginHandler → fallbackToWeb）
  — **⚠️ 2026-10-28 起 `SignSpoof` 已从源码删除**：ACE/TSS 这类"官方 dex 禁 patch"的包改包法要在 dex 侧动 SDK 门闩/登录入口（见 `login/taptap-web-login.md` 现行口径），本行保留为历史结论
- 主线程 hook 回调零解引用 + 对象学习放专用学习线程（见 `re-implement/hook-rules.md` §20）

## 案例：雷霆战机

- ACE 扫 `/proc/self/maps` 检测注入的 so
- Dobby hook `evalString` 会被 ACE 检测到
- 轻量 JS 旗标（只改 BSS 数据区开关）可以存活
- 教训：不要在 ACE 保护的游戏里做 inline hook，改用旗标门控

## 案例：公主连结

- 受保护 DEX 清单明确（`classes4`/`classes8` 不可改）
- `resources.arsc` 必须用 `STORED`（不压缩）方式打包
- 只能外科 zip 组装，禁止 `apktool b`

## 验证方法

```bash
adb logcat | grep -E "tersafe|tprt|ACE"
```
看是否过 Privacy 页、`libcocos.so` 是否正常映射。存活 >25s 即初步通过。

## 外部已公开方案（检索入口，2026-10-15 新增）

- **已核验公开项目：暂无**（2026-10-15 首查 `ACE 反外挂 过检 手游 逆向` 得 0 条，疑搜索引擎频控，**不等于无解**；空结果已记台账 §3）。
- **检索词**：`ACE 过检` / `tersafe 反调试 绕过` / `site:bbs.kanxue.com ACE` / 英文源 UnknownCheats、GameGuardian（壳类问题命中率较高）。
- **注意**：ACE/TSS 属**反作弊**而非壳（本页开头的「不走壳路」判断优先）；外部方案即便存在，也须先过 `code-review-checklist.md`「外部方案专项」四问，尤其是**痕迹面**。
- **台账**：`10_docs/_state/外部方案台账.md`；协议见 `skill/diagnosis-index.md`「外部检索协议」。
