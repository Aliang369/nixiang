# 斌哥壳 memdiff 完整流程

> 📍 路径：`re-extract/binge-memdiff.md`
> 🎯 触发场景：APK 有斌哥改包（签名 `CN=binge2.keystore` / `libSDKloading.so`），需要提取所有玩法 Hook 点

这是最核心的抓点技能。斌哥壳已内置菜单功能开关，通过 OFF vs ON 内存差分即可定位每个功能的 Hook 点。

---

## 一、斌哥壳识别特征

| 检测项 | 特征 |
|--------|------|
| APK 签名 | `CN=binge2.keystore` |
| lib 目录 | `libSDKloading.so`（~3.7-3.9MB packed） |
| lib 目录 | `libtoolChecker.so`（RootBeer） |
| Application 继承链 | `KillerApplication193` 或 `SignatureSpoof` |
| 远程菜单 | `https://menu-1255602259.file.myqcloud.com/game/<id>.html` |
| prefs | `{pkg}_preferences.xml`，boolean key `"0"`/`"1"`/`"2"`... |
| 壳配置 | `shell_config.xml` / `shell_protected*.xml` |
| 免责声明邮箱 | `bg888@188.com` |

---

## 二、完整流程

### 步骤 1：静态初判

1. 拆包看 `AndroidManifest.xml` 的 Application 继承链
2. 查看 `lib/` 目录确认引擎类型（libil2cpp / libcocos2djs / libxlua）
3. 查看 `assets/` 有无 `global-metadata.dat`、`.jsc`、`.bundle` 等

### 步骤 2：基线 dump（菜单全关）

```bash
bash 04_scripts/_common/dump_binge_shell_mem.sh <pkg> <outdir>
```

- 确保游戏菜单所有功能开关均为 **OFF**
- 脚本通过 KSU `/proc/pid/mem` 读取进程内存
- dump 内容：`libil2cpp.so` / `libcocos2djs.so` 可执行段 + BSS 段

### 步骤 3：功能 ON dump（逐个开）

1. 在游戏菜单中打开**一个**功能
2. 等功能生效后，同脚本再 dump
3. 多个功能需逐个开启 + 逐个 dump（做单项差分）

### 步骤 4：prefs 差分

对比两次的 `{pkg}_preferences.xml`：
- boolean key `"0"`/`"1"`/`"2"`... 对应功能开关
- `"0"` = 第1个功能，`"1"` = 第2个，依此类推
- 值 `true` = 开启，`false` = 关闭

### 步骤 5：BSS 旗标差分

对比 `shell_rw_bss` 的内存 dump：
- 找到变化的 offset（如 `+0x14C0`、`+0x1798`、`+0x1728` 等）
- OFF 状态 = `0x00000000`，ON 状态 = `0x00000001`
- 这些 offset 就是功能开关的 BSS 旗标地址

**BSS 旗标偏移速查表**（不同游戏不同起始偏移，间隔都是 `+4`）：

| 游戏 | 旗标起始偏移 | 间隔 |
|------|-------------|------|
| 造梦西游/镇邪人/雷霆战机 | `+0x1798` | `+4` |
| 追迹迷城/霓虹深渊/物质与魔法 | `+0x14C0` | `+4` |
| 魂坠深境 | `+0x1728` | `+4` |
| 音舞光年 | `+0x172c` | — |
| 消消英雄2/孤勇无双 | `+0x4C0` | `+4` |

> ⚠️ 以上偏移仅供参考定位思路，不同版本会变，以实际差分为准。

### 步骤 6：il2cpp memdiff

```bash
python3 diff_il2cpp_mem.py <outdir> --apk <apk>
```

- 对比 OFF vs ON 的 `libil2cpp.so` 可执行段
- 找 prologue `B` stub（ARM64 `B` 跳转指令 = Dobby inline hook 特征）
- 4KB 拆页（函数入口所在页被拆分）= Dobby 典型特征

### 步骤 7：Il2CppDumper

对 `libil2cpp.so` + `global-metadata.dat` 运行 Il2CppDumper：
- 输出 `dump.cs`（类/方法/字段信息）
- 输出 `script.json`（RVA → 方法名映射）

### 步骤 8：RVA → 方法名映射

用 `script.json` 精确匹配 memdiff 找到的 RVA：
- 每个 prologue B stub 的 RVA → 对应方法名
- 方法名与功能语义匹配（如 `ChangeHP` = 伤害修改）

### 步骤 9：BSS 字符串扫描找 IL2CPP 绑定候选

- 扫描 BSS 中的字符串（方法名、类名片段）
- 斌哥壳会在 BSS 中存 IL2CPP 绑定的候选方法名
- 壳自检字符串也是可靠定位手段（如 `怪改血=0xXXXXXX`）

---

## 三、斌哥壳功能开关持久化机制

完整链路：

```
WebView Changes(...)
  → 写 {pkg}_preferences.xml boolean "0"-"N"
  → 同步 BSS 旗标 +0x14C0 起 int32 = 0/1
  → get_ret(n) 读取旗标
  → get_ret(n) = jsb.reflection.callStaticMethod(AppActivity, "get_ret", "(I)Z", n)
  → Hook 函数内读 BSS 旗标决定行为
```

- `get_ret(n)` ≈ prefs `"n-1"`（1-based JNI）
- 功能靠旗标门控，不靠增删 stub → OFF vs ON patch 数量应一致

---

## 四、坑/弯路

### 连续多个 [anon:.bss] 后者覆盖前者
dump 脚本对连续多个 `[anon:.bss]` 段，后者会覆盖前者。需手动补 dump 或写 salvage 脚本逐段保存。

### libcocos 主段字节漂移 ≠ patch
`libcocos.so` 主段大量字节漂移是 Cocos Creator 3 加载对齐差，**不是**玩法 patch。差分时需排除已知对齐漂移区域。

### dump 脚本对大段需手动补 dump
toybox `dd` 会溢出大块，大段需用 `memdump64` C 工具手动补 dump。

### 双 Dobby 叠加 SIGSEGV
星光 + 斌哥壳双 Dobby 叠同一函数会 SIGSEGV。如需替换斌哥 Hook，先恢复原入口指令。

### 壳自检字符串是可靠定位手段
壳 BSS 中的自检字符串（如 `怪改血=0x306E8FC`）记录了关键 Hook 点地址，是最可靠的定位手段之一。

---

## 五、验证方法

- OFF vs ON patch 数量一致（功能靠旗标门控，不靠增删 stub）
- BSS 旗标 OFF=0 → ON=1
- 方法名与功能语义匹配
- logcat 确认功能生效
