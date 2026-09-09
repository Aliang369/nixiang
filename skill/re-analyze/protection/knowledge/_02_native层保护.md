# 知识库 02 · Native/ELF 层保护（十块全量）

> 📍 `re-analyze/protection/knowledge/_02_native层保护.md`
> 🎯 so 层保护「怎么认、怎么动手、坑在哪」。so 是最凶的校验藏身处，本页按 10 块拆解。

---

## 块 1. so 整体加密壳

### 特征
- IDA 打开 so 只有一段 stub（几百 KB），业务逻辑看不到
- `/proc/self/maps` 里该 so 有**怪异映射**：文件映射 vs 匿名 r-x 段分离（运行时解密到匿名页）
- `readelf -S` 段表异常：可执行段 `p_filesz=0` 但 `p_memsz` 很大

### 动手
1. 运行时 dump：so 被 loader 解密 + 修 relocation 后，从 maps 里把 r-x 段 dd 出来
2. 流程记录：loader → 解密到匿名内存 → 修 GOT/rela → 跳转真入口
3. 行为替代：不逆向解密逻辑，直接 hook 它的导出函数黑盒复用

### 坑
- loader 本身有反 dump（mprotect 只执行不可读）→ 先过 mprotect
- dump 出的裸段缺动态结构（`.dynamic`/符号表）→ 只能挂 IDA 静态读，不能直接替换用
- 和「整体加密 dex」一样，so 壳常连带签名校验 → 别只 dump 不评估

### 状态
- ⚠️ 待验证（本工作区未命中纯 so 壳，最接近的是雷霆 ACE so 段摘要校验）

---

## 块 2. UPX 压缩

### 特征
- `strings so | grep UPX` 见 `UPX!` 魔数
- `readelf -S` 段名 `.UPX0` / `.UPX1`

### 动手
- `upx -d so`（若版本兼容）
- 不行则手动：UPX 头部记录原始入口 & 压缩段，手动解压再修

### 坑
- 厂商改过 UPX 魔数 / 压缩表 → 标准 `upx -d` 失败，要手动识别
- 解压后被脱壳的 so 又可能是别家的壳（叠加）

### 状态
- ⚠️ 待验证

---

## 块 3. OLLVM 混淆（全家）

### 3.1 控制流平坦化（最典型）
- 特征：IDA 里函数变成 `while(1)` + 一个大 `switch(state)` 循环，所有基本块被塞进 switch
- 对策：angr/deflat 解平（成功率依赖反混淆脚本成熟度）；更稳的是**行为替代**——不改控制流，改函数的输入/输出 hook

### 3.2 字符串加密
- 特征：IDA 的 `strings` 看不到明文，全是运行时 `decrypt_str(offset)` 解密
- 对策：Frida hook 解密函数抓明文；或在 IDA 里写脚本批量解密（找到解密 key 和算法）

### 3.3 指令替换 / 不透明谓词
- 特征：简单运算被换成复杂等价序列；插一堆永远为真的假分支
- 对策：一般不影响逆向主线，忽略即可；关键函数用动态 hook 兜底

### 坑
- 解平脚本对「循环 + 多层嵌套」常失败；别死磕，切行为替代
- 认清 OLLVM 只是「隐藏逻辑」，不是「校验」——混淆本身不杀你，杀你的是混淆里藏的校验

### 状态
- ⚠️ 待验证（本工作区遇到的是自定义段/符号表混淆，见块 8）

---

## 块 4. so 自校验 / 跨 so 校验

### 特征
- 改 so 一个字节就死
- 反汇编里能定位到对 `.text` 段做 crc/md5 的循环（对一个固定地址区域 iterate 累加）
- 跨 so 校验：A so 校验 B so 的字节（雷霆 `tprt` 内嵌 tersafe md5）

### 动手
1. A/B：改一字节 so → 确认是自校验还是他校验
2. 定位校验点：找 crc 循环（典型模式：`eor/lsl/and` 组合累加，或 BL memcpy 计算中间）
3. **让校验通过**（铁律）：patch 校验结果，不是 patch 杀进程结果。把 crc 循环结果强制置成官方值，或把「比对」的输入指向官方字节
4. 雷霆教训：`libtprt@0x16956c` 内嵌 tersafe md5，改 so 易踩

### 坑
- patch 校验点位置不对 → 反而触发死转/死循环（KwProtect 教训）
- 校验点可能是多段摘要表（25 条 size+CRC+tail），只 patch 1 处不够

### 状态
- ✅ 雷霆战机 ACE（sectionhash/filecrc/HashBySyscall、tprt 多 ABI MD5、段摘要表 0x19b7xx 已测）
- ✅ 王国大作战 KwProtect（lib MD5 校验）

---

## 块 5. Anti-inline hook（旗标门控）

### 特征
- 一改 so 代码段就死（inline hook 触碰校验区）
- 官方 so 有自校验，任何 `blr` 改写都被查

### 动手
- **旗标门控**：不改代码段，只改 **BSS/.data 数据区**（全局 flag 变量），用 flag 控制逻辑路径
- 雷霆战机已验证：ACE 只校验代码段，数据区修改不触发

### 坑
- 不是所有 so 都容忍数据区改动——先 A/B 测「改 BSS 一字节」是否死
- 数据区的 flag 是运行时赋值，静态 patch 要找到写入点

### 状态
- ✅ 雷霆战机

---

## 块 6. 反调试（6 技，最常命中）

### 6.1 ptrace 检测
- 特征：`TracerPid` 非 0 判断 / 自己 ptrace 自己占位
- 对策：本工作区铁律**非 ptrace 注入**（自注入 Trace），必要时绕过

### 6.2 `/proc/self/status` 读 TracerPid
- 特征：fopen/read hook 拦截可见它读 status
- 对策：fopen/read hook 回灌伪造 status（蜀汉传 §5k 设计，未验证完）

### 6.3 maps 完整性
- 特征：扫 `/proc/self/maps` 找 frida/工具 so 名、匿名内存
- 对策：别留 frida/工具 so；自注入用「已有合法 so 里藏代码」

### 6.4 inotify / 目录监控
- 特征：监控 `/data/local/tmp` 探针文件出现 → 死
- 对策：探针文件放隐蔽路径，用完即删（cleanup 脚本）

### 6.5 Frida 特征（端口/D-Bus/maps/线程名）
- 特征：27042 端口 / `/data/local/tmp/re.frida` / frida 线程名
- 对策：别用 frida 于强反调试；自注入 Trace 优先

### 6.6 时间差 / 执行流时间
- 特征：单步断点触发超时 → 死
- 对策：不逐行调试，用 hook 抓行为

### 6.7 JDWP / 系统属性 / SELinux / 进程扫描（与 04 对齐）
- 特征：`ro.debuggable=1`、`getenforce`=Permissive、`ps` 枚举找 frida-server/magiskd
- 对策：别开 debuggable 包跑调试；SELinux 保持 Enforcing；magiskprops 回灌属性；相关细节见 `_04_设备环境检测.md`

### 坑
- 反调试往往是矛盾的：先反调试致盲了，后面才轮到校验。顺序别搞反

### 状态
- ✅ 蜀汉传卧龙：ptrace attach 加速死亡、`/proc/self/mem` EACCES（已实锤）
- ✅ Frida 检测、残留污染（多游戏）
- ⚠️ Xposed/模拟器检测待验证（见块 9 / 04）

---

## 块 7. JNI_OnLoad 保护与调用者检查

### 特征
- so 的 `JNI_OnLoad` 里查调用者（`__builtin_return_address` / GetStackTrace）
- 劫持 JNI_OnLoad 或 gJNIEnv → 触发校验

### 动手
- 别劫持 JNI_OnLoad；改走正规 `loadLibrary → JNI_OnLoad` 链
- 必须在 so 加载后注入：用 `dlopen` 句柄在 `JNI_OnLoad` 返回后动手

### 坑
- 雷霆 `CocosActivity.onCreateNative` ULE（JNI 未绑 PathCL）——双份 cocos 身份问题，与 JNI_OnLoad 保护相关但不同

### 状态
- ✅ 雷霆（JNI 绑定、双份 so 身份问题已踩）

---

## 块 8. ELF 结构技巧

### 8.1 破坏 dynsym / 符号表混淆
- 特征：`readelf -s` 符号名空/异常 st_value；字符串里是 `ELF_HOOK_*_JNI_OnLoad`（雷霆）
- 对策：静态找真地址不直接，用运行时 `dlopen + dlsym` 拿句柄走动态解析

### 8.2 `.init_array` 跑校验
- 特征：so 加载即校验（constructor 早于 JNI_OnLoad）
- 对策：改 `.init_array` 即死（雷霆 DT_NEEDED 实验证明改 `.dynamic` 秒杀）→ 用 ADD 新 so 而非改既有

### 8.3 自定义段（`.gnu.fragment/.draft/.stub`）
- 特征：易盾 nesec 新版的自定义段，抛分析
- 对策：这类段静态分析成本极高 → 走动态 dump 或脱壳重建（无悔华夏已选后者）

### 8.4 mprotect 反 dump
- 特征：可执行段设成不可读（PROT_EXEC 无 PROT_READ）
- 对策：模拟执行重放或改内存权限（需 root）

### 坑
- `.dynamic` / `.init_array` 是死面：改了秒杀，与「改代码段延迟死」不同。别把两者混淆（雷霆路线3 已区分）

### 状态
- ✅ 雷霆战机（dynsym 混淆、DT_NEEDED 死面、延迟窗 vs 秒死）
- ✅ 无悔华夏（易盾自定义段）
- ⚠️ mprotect 反 dump 待验证

---

## 块 9. 运行环境检测（so 内嵌）

- so 里查 root/frida/模拟器/Xposed，与 Java 层检测同源但更难 hook
- 对策见 `_04_设备环境检测.md`；so 层重点是**native 隐藏**而非 Java 层伪装

### 状态
- ✅ root/frida 检测（多游戏）
- ⚠️ Xposed native 层检测待验证

---

## 块 10. 注入检测 / 其他进程杀

### 特征
- 进程非自杀：其他进程（守护进程/ACE 服务）扫描后 kill
- logcat 看不到本进程 exit 栈，但进程死

### 动手
- 先判「自杀」还是「他杀」：hook 本进程 exit 系列无命中 + 进程仍死 → 他杀
- 找监控进程，处理之

### 坑
- 雷霆 ACE 更像 raw SVC / `_exit`（自杀软补未命中投递路径，H5i6）——但「他杀」线未完全排除，别早下结论

### 状态
- ⚠️ 雷霆 ACE 部分（exit 线无效，他杀线待测）

---

## 块 11. PLT / GOT 层 hook 对抗

### 特征
- 游戏校验 GOT/PLT 表项，比对 `dlopen`/`dlsym`/`__system_property_get` 等是否指向真实地址（被重定向即判 hook）

### 动手
- **优先「不打代码」**：旗标门控改 BSS/.data 数据区，不进函数入口、不动 GOT（雷霆实战）
- 必须在代码层 hook 时：等长跳板 + 必调原函数，且避开「比对函数的入口码」

### 坑
- GOT 重定向裸眼可见，比对型校验一步抓到；patch 掉比对函数自己也会暴露（它就是被 hook 目标）
- 雷霆/ACE 经验：`.dynamic`/DT_NEEDED 是死面（改即秒杀），与「改 GOT 延迟暴露」不同层级，别混淆

### 状态
- ✅ 旗标门控（雷霆战机）
- ⚠️ 显式 GOT 比对检测未实锤（自注入 Trace 未命中）

---

## 块 12. seccomp / syscall 拦截 与 优雅停机

### 特征
- seccomp BPF 拦截 `exit`/`exit_group` 等 syscall，试图阻止进程终结
- 优雅停机：检测命中后置 flag，所有线程**自然 return 结束** → 内核自动回收进程，全程**无 exit_group syscall**

### 动手
- seccomp 拦 exit 是**错方向**：拦了 exit，bionic 的 exit wrapper 会陷入死循环黑屏（已经实测）
- 优雅停机没有可拦的 syscall → 只能**废掉检测源头**（在"置 flag 前"断掉），而不是拦停机结果
- 定位源头：静态不可见（运行时生成）时，走反-反调试 → 动态取证找「生成并写 flag」的代码

### 坑
- 蜀汉传卧龙实锤：seccomp TRAP 取证无 SIGSYS tombstone → exit_group 根本没发生；拦截全落空
- 把「优雅停机」当「自杀 exit」去拦 → 时间黑洞，正确方向是废检测

### 状态
- ✅ 蜀汉传卧龙（优雅停机已实锤，seccomp/GOT 拦截无效）
- ⚠️ 「反-反调试 → 废检测源头」路径待验证（项目已终止，未走完）

---

## 总结对照卡

| 块 | 一句话识别 | 一句话对策 | 状态 |
|---|---|---|---|
| 整体加密 | maps 匿名 r-x 分离 | 运行时 dump + 行为替代 | ⚠️ |
| UPX | `UPX!` 魔数 | upx -d / 手动 | ⚠️ |
| OLLVM | 大 switch 循环 | 解平 or 行为替代 | ⚠️ |
| 自校验/跨so | 改一字节死 | 让校验通过 | ✅ |
| Anti-inline | 改代码段死 | 旗标门控（改 BSS） | ✅雷霆 |
| 反调试 | 被跟踪/有 frida 即崩 | 自注入、隐藏特征 | ✅ |
| JNI_OnLoad | 劫持即崩 | 正规加载链 | ✅雷霆 |
| ELF 结构 | dynsym/init_array/自定义段 | 动态解析、ADD 不改 | ✅ |
| 环境检测 | root/frida 被扫 | native 隐藏 | ✅部分 |
| 注入/他杀 | 本进程无 exit 栈但死 | 判自杀/他杀 | ⚠️ |
| PLT/GOT hook | GOT 表项被重定向 | 旗标门控不打代码 | ✅部分 |
| seccomp/优雅停机 | 无 exit_group 却进程终结 | 废检测源头不拦停机 | ✅蜀汉 |