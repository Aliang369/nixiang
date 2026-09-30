# 境界刀鸣 直装包闪退：根因定位与修复（libhhld-rt.so TLS 析构 kill-switch）

- 游戏：`com.blade.bd` v1.39.211507（vc 39，TapTap 渠道）
- 设备：`f9a116a2`（OPD2404 / Android 16 / 已 root，仅用于取证与对照）
- 日期：2026-09-23
- 交付形态：**无 root 星光直装包**（重签 APK，注入 dex + `libstarry_magic.so` + Manifest）

---

## CLAIM

非官方签名会触发 `libhhld-rt.so` 内一条 TLS 析构回调路径，其函数指针被破坏为野地址
`0x18a44`，随后 `blr x8` 跳过去必崩。把该 `blr x8` 原地 NOP 掉，闪退**彻底消失**，
且热更（Hades/TGRP）、Unity、il2cpp、Lua 玩法层全链路正常。

## VERDICT

**已验证（真机 4 组对照 + 成品包 120s 实跑）**

## EVIDENCE

### 1. 三组决定性对照（同一设备，均 `am kill-all` + `drop_caches` 后冷启）

| # | 包 / uid | 数据 | 结果 |
|---|---|---|---|
| A | 官方签名原版（1986468431 B），uid 10882 | 全新 | VmRSS 437MB@2s → 830MB@20s → 1.03GB@80s；**80s+ 存活，CRASH=0，无 lmkd kill** |
| B | 零注入重签带 hhld（1986464335 B），uid 10883 | **全新（不恢复）** | 内存曲线与 A 一致，**t=14s DEAD**：`F libc: Fatal signal 11 (SIGSEGV), code 1 (SEGV_MAPERR), fault addr 0x18a44 in tid 22448 (Thread-8), pid 22217` |
| C | 同 B，但把**已解包的** `libhhld-rt.so` 就地打补丁 | 同 B | **存活 80s+，CRASH=0**；VmRSS 545MB@2s → 830MB@24s → 平稳 750~775MB → 1.13GB@80s |
| D | **完整星光包（含 hhld NOP）+ 数据恢复** | 热更数据 | **存活 120s，CRASH=0 / OOM=0**；`Hades=37 TGRP=37`；GBridge 显示 `sdk_login` 成功 → `download_server_list_end`（`server_id: 11001`）⇒ 游戏已进到登录后流程 |

- B 用**全新数据**仍崩 ⇒ 排除「脏数据 / 热更缓存损坏」假设。
- 早前 v2-only 重签包同样崩且崩溃行逐字节一致 ⇒ **签名方案（v2 vs v3）不是触发条件，任何非官方证书都触发**。

### 2. 崩溃现场（四份 tombstone 逐字节一致）

- `signal 11 (SIGSEGV), code 1 (SEGV_MAPERR), fault addr 0x18a44`；`x9=x10=0x18a44`、`lr=pc=0x18a44`、`1 total frames`。
- `x29=0x79677b6440`，`memory near x29` 第 3 行 `...6440  00000079677b6460  00000078c6ee6274` ⇒ 保存的 `lr = 0x78c6ee6274`。
- `libhhld-rt.so` r-x 段基址 `0x78c6e8a000` ⇒ **调用者 = `libhhld-rt.so + 0x5c274`**（正是 `blr x8`(0x5c270) 的下一条）。

### 3. 崩溃点反汇编（`libhhld-rt.so`，`llvm-objdump -d`）

```
5c240: d65f03c0  ret
5c244: 97fed6cb  bl  0x11d70 <__stack_chk_fail@plt>
5c248: a9be4ff4  stp x20, x19, [sp, #-0x20]!
5c24c: a9017bfd  stp x29, x30, [sp, #0x10]
5c250: 910043fd  add x29, sp, #0x10
5c254: aa0003f3  mov x19, x0
5c258: 94015b26  bl  0xb2ef0
5c25c: f9400261  ldr x1, [x19]
5c260: f900027f  str xzr, [x19]
5c264: b9400000  ldr w0, [x0]
5c268: 97fed552  bl  0x117b0 <pthread_setspecific@plt>
5c26c: f9400668  ldr x8, [x19, #0x8]
5c270: d63f0100  blr x8            <-- CRASH: x8 = 0x18a44
5c274: b4000153  cbz x19, 0x5c29c
...
5c2a8: d65f03c0  ret
```

- 语义：**thread_local 对象析构执行路径**——`pthread_setspecific(key, NULL)` 清槽后，取记录里的
  `[x19+8]` 当析构函数指针 `blr`。记录布局 ⇒ `[x19+0]`=TLS 值/arg，`[x19+8]`=dtor。dtor 被破坏成 `0x18a44`。
- `0x18a44` 落在函数中部（`0x18a40: mov w8,#0x63a4` / `0x18a44: movk w8,#0xd99c,lsl#16`），**不是函数起点**。
- 全文件搜 `0x18a44` 的 u64le/u32le/u32be、`0xD99C63A4` 的 u32le = **0 命中**；97 个 arm64 `.so` 的
  `.rela.dyn` 里**没有任何 addend == 0x18a44** ⇒ 该值不是静态存储，是运行时被写坏/算出的。
- ⇒ **`libhhld-rt.so` 是受害点而非校验者**；真正的校验者仍未定位（候选 `libgame.so` / `libAPSE_7.0.1.so` / `libthemis.so`），但不影响修复。

### 4. 修复（两 ABI 各一处，均已真机验证）

| ABI | 文件偏移 | 原字节 | 指令 | 补丁字节 | 指令 |
|---|---|---|---|---|---|
| arm64-v8a | `0x5c270` | `00 01 3f d6` | `blr x8` | `1f 20 03 d5` | `nop` |
| x86_64 | `0x5f99d` | `ff 53 08` | `callq *0x8(%rbx)` | `0f 1f 00` | `nopl (%rax)` |

- x86_64 对应点上下文（同构）：
  ```
  5f984: 48 89 fb     movq %rdi, %rbx
  5f987: e8 44 ac 05 00 callq 0xba5d0
  5f98c: 48 8b 33     movq (%rbx), %rsi
  5f98f: 48 c7 03 00 00 00 00 movq $0x0, (%rbx)
  5f996: 8b 38        movl (%rax), %edi
  5f998: e8 13 11 fb 00 callq 0x10ab0 <pthread_setspecific@plt>
  5f99d: ff 53 08     callq *0x8(%rbx)   <-- CRASH 点
  ```
- 设备侧即时验证（原地覆盖解包后的 libdir 副本，保 owner/mode/context）：
  `md5 9e51e753952d619007c6846d2195b600`，`dd bs=1 skip=377456 count=8 | od -An -tx1` → `1f 20 03 d5 53 01 00 b4` ✓

### 5. 构建脚本已内置（可复现）

`mod菜单/starry-mod-menu/scripts/surgical_jjdm_1.39.211507.sh` 的 `[6/6] zip surgical merge` 段新增 `HH_PATCH`：

```python
HH_PATCH = {
    "lib/arm64-v8a/libhhld-rt.so": (0x5c270, b"\x00\x01\x3f\xd6", b"\x1f\x20\x03\xd5"),
    "lib/x86_64/libhhld-rt.so":    (0x5f99d, b"\xff\x53\x08",     b"\x0f\x1f\x00"),
}
```

- 补丁点不匹配即 `SystemExit`；补丁条目从「官方条目透传 hash 校验」中排除。
- 签名后二次校验成品包内两处字节，失败即报错。
- 本次构建输出：`merged ok; arsc/官方条目透传 3109/3109; inject=classes15.dex; hhld NOP 2/2`
  ```
  hhld NOP 校验 lib/arm64-v8a/libhhld-rt.so @0x5c270 = 1f2003d5 OK
  hhld NOP 校验 lib/x86_64/libhhld-rt.so @0x5f99d = 0f1f00 OK
  ```

## 产物

| 文件 | 大小 | sha256 / md5 |
|---|---|---|
| `08_output/境界刀鸣/境界刀鸣_1.39.211507_surgical-releaseSigned.apk` | 1989 MB | sha256 `01f33f6e9535d44d919688f9ec4c9ee5768396e991682552d476e99b29b0cc0f` |
| 其中 `lib/arm64-v8a/libstarry_magic.so` | 2211888 B | md5 `917e28cd799ba687b9ebf0bda116d9de` |

## 附带发现（重要，直接影响装机流程）

1. **`pm uninstall` 前必须先 `mv /sdcard/Android/data/<pkg>` 挪开**，否则热更缓存（本次 2.9G）被删。
2. **恢复 `/data/data/<pkg>` 时绝不能 `rm -rf` 顶层目录再 `mkdir`**：installd 建立的顶层目录带
   `u:object_r:app_data_file:s0:c116,c259,c512,c768` 类别，手工重建后子项会被 `restorecon` 打成
   `u:object_r:system_data_file:s0`，app 读 `shared_prefs/tt_game.xml` 报
   `EACCES (Permission denied)`，导致 Unity→原生桥的 `requestCheckHasAgreePrivacy` **每 33ms 死循环**
   （1980 次/100s），游戏卡在隐私门，表现为「不崩但永远进不去」。
   正确做法：先安装（让系统建好顶层目录）→ 覆盖内容 → `chown -R <新uid>:<新uid>` →
   `CTX=$(ls -Zd /data/data/<pkg> | awk '{print $1}'); chcon -R "$CTX" /data/data/<pkg>`。
3. `/sdcard/Android/data/<pkg>` 的 context 本就是 `u:object_r:fuse:s0`，`restorecon` 报
   `Operation not supported on transport endpoint` 是 FUSE 正常现象，可忽略。
4. `libhhld-rt.so` + `lib/x86_64/libhhld-rt.so` + `assets/hhld_info.json` 是 APK 内唯三 **18:27** 时间戳
   （其余 18:22~18:23）⇒ 构建后 4-5 分钟注入；`assets/hhld_info.json`（240 B）非 JSON，首 8 字节
   `00 00 00 e1 00 00 00 e8` 后接 232 B 高熵 ⇒ 加密 blob。
5. 所有 `lib/*.so` 均为 DEFLATED，`AndroidManifest.xml` **无 `extractNativeLibs`**（默认 true）
   ⇒ 安装时解包到 libdir，因此**可以直接替换 APK 内 .so 再签名**，无需重打包结构。

## 待验证 / 未解决

- 真正的签名校验者（谁把 `[x19+8]` 写成 `0x18a44`）仍未定位；候选 `libgame.so` / `libAPSE_7.0.1.so` / `libthemis.so`。
- NOP 后 TLS 析构回调不再执行 ⇒ 理论上每次线程退出会漏一点 hhld 自身 TLS 对象；实测 120s 内
  VmRSS 曲线正常（~1.5GB 稳定），暂未观察到异常增长。
- 三个功能（伤害倍率 / 无敌 / 无限技能）在本成品包上的开关生效性，需人工进战斗确认。
- `CrashTrap_LogMaps` 仍是单次 `read()`，恒打 `[maps] 共 0 行`，未修。
- 待验证风险：Thanos `attachBaseContext` 早期检查、MIRA 完整性、TapTap Themis、服务端帧同步上报。
