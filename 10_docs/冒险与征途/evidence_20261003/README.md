# 冒险与征途 决定性证据（2026-10-03）

> 只列决定性证据行、最短复现命令、NOT PROVEN 项。
> 结构见 `10_docs/_templates/功能验收卡.md`；机制解释见 `../验收卡_v44_20261003.md`。
> 说明：v43 那轮原始 logcat 全量（7864 行）属临时采集物，收尾时已按「临时文件不留本地」清理；
> 下列行是当时的原样摘录（决定性行，均可在下一轮复现命令中重新取到）。

## 1. 装机指纹

| 项 | 值 |
|---|---|
| 包 | `com.mxyzt.gzmy` 1.5.9 / vc159（Unity 2022.3.14f1 + HybridCLR 热更解释执行，arm64-v8a） |
| 正式包 | `08_output/冒险与征途/冒险与征途_1.5.9_starry-final-releaseSigned.apk` 885,310,399 B<br>sha256 `74147b363f9687139f14f73bb6c99ba82ccfe34aca5e11a6b2ea7bd9ece767ea` / md5 `b4e5ccb5f1010b5de33cceb53bbb9a9e` |
| 包内 so | `lib/arm64-v8a/libstarry_magic.so` 2,218,032 B<br>md5 `a97d27d48de5ae3f88e600a0f5822896` / sha256 `d16bff8ac8a102561126c4d91b7edce77920e330827e0fbd22a6782fc050c744` |
| 签名 | `CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`<br>SHA-256 `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d` |
| 装机后设备侧 so md5 | `a97d27d48de5ae3f88e600a0f5822896`（= 构建产物 ✓，安装路径 `/data/app/~~SoVi-8XwwWQrEKWQ8ukgdg==/com.mxyzt.gzmy-a2f1u2IcLsEgg_7-3z_B5w==/`） |
| 验签 | `apksigner verify` 退出码 0（build-tools 36.1.0），仅 version 文件的常规 WARNING |

## 2. 决定性证据行

### 2.1 入口 hook 装配（每次冷启动第二条生效）

```
[mxyzt] interp base=0x400023200000 entry=0x40002527f798 rd=1 rva=0x207f798
[mxyzt] interp DobbyHook rc=0 errno=0 orig=0x7d4763581000
[mxyzt] pushframe base=0x400023200000 fn=0x40002527dccc rd=1 rva=0x207dccc
[mxyzt] pushframe DobbyHook rc=0 errno=0 orig=0x7d476358101c
[mxyzt] pushframe armed（托管→托管全覆盖：Hurt/Damage/DoAttack 从这条路进来）
[mxyzt] interp learn 窗口开启（600 秒，14 个目标）
```

### 2.2 #1 秒杀小怪（BigInt ×100，仅 `characterType==2`）

v43 计数：`打怪#`=32 行；样例（首领不进这条分支，见 NOT PROVEN 第 3 条）。

### 2.3 #2 无敌（`StarsCollider::DoAttack` 槽 2 置 null）

v43 计数：`INV cancel`=11 行；决定性行——**`playerHitter=0x0` 仍能取消**，证明判据走的是 roleId 而不是缓存指针：

```
INV cancel DoAttack 槽 2 accepter=0x7d4487ead990 playerHitter=0x7d4487ead990
```

### 2.4 #3 全模式技能伤害增加（`ActionCommand_AddProjectile::Execute` 的 `get_count()` 循环）

v43 计数：`投射物#`=14 行；发送者闸 fail-closed（解不出玩家侧就不写）+ 关功能时 `ProjRestoreV41()` 还原 count。

### 2.5 #4 免广告（四目标预登记 + 代回调）

v43 计数：`v43 预登记`=4、`AD43`=8、`代回调`=4。

```
↑v43 预登记 PlayAD2 kind=0x0010 base=1 mi=0x7d43533c1530 新建
↑v43 预登记 OnResult7 kind=0x0020 base=1 mi=0x7d43559defc0 新建
↑v43 预登记 Dispatch2 kind=0x0040 base=1 mi=0x7d43559df110 新建
↑v43 预登记 CanClaim kind=0x0080 base=1 mi=0x7d4356af9660 已存在
↑v43 广告四目标预登记**全部完成**（免广告 A1+A2 已就绪）
AD43 PlayADVideo 免广告=1 adid=17 uuidStr=0x7d448a68c080 uuid=131202524823967974 解析=1 mi=0x7d43559defc0
AD43 ★已代回调 OnADVideoResult(true,17,131202524823967974)★ 第1次（不出画也照发 cmd2762，服务端发奖）
```

### 2.6 v44 清理版烟测（pid 30714，attempt 1 即干净）

```
[mxyzt] lib ready (il2cpp+HybridCLR, lazy hooks)
[mxyzt] interp learn 窗口开启（600 秒，14 个目标）
[mxyzt] feat 1..4 -> 0
StarryMagic: 星光魔法 MOD 菜单已加载 (MainModern tag=mxyzt)
[桥接] RegisterNatives 19/19 全部成功
[验证] grant ok exp=1791059367
```

启动日志总量 179 行（其中 162 行是跨游戏共享的 `[Il2Cpp]` 解析器行）。

## 3. 最短复现命令（Windows / MuMu，serial `127.0.0.1:16384`）

```bash
adb=C:\Users\Administrator\AppData\Local\Android\Sdk\platform-tools\adb.exe

# 设备侧 so 指纹
"$adb" -s 127.0.0.1:16384 shell "md5sum /data/app/*/com.mxyzt.gzmy*/lib/arm64/libstarry_magic.so"
# 期望 a97d27d48de5ae3f88e600a0f5822896

# 采集：冷启动第一条会 SIGSEGV（已知），force-stop 后第二条起干净
"$adb" -s 127.0.0.1:16384 shell am force-stop com.mxyzt.gzmy
"$adb" -s 127.0.0.1:16384 logcat -b all -c
"$adb" -s 127.0.0.1:16384 shell monkey -p com.mxyzt.gzmy -c android.intent.category.LAUNCHER 1
"$adb" -s 127.0.0.1:16384 logcat -d | grep -E "mxyzt|AD43|INV cancel|打怪#|投射物#|代回调|STATS"

# 正式包验签
& 'C:\Users\Administrator\AppData\Local\Android\Sdk\build-tools\36.1.0\apksigner.bat' verify `
  'E:\nixiang\08_output\冒险与征途\冒险与征途_1.5.9_starry-final-releaseSigned.apk'
```

观察点：菜单点开功能 → `feat N -> 1`；战斗中 30s 内 `STATS …命中 打怪≥1`；
点「看广告」→ `AD43 ★已代回调 OnADVideoResult(true,adid,uuid)★`。

## 4. NOT PROVEN

1. **服务端结算**：免广告代回调后「服务端发奖」是由恢复出的调用链推断（游戏照发 `cmd 2762`），未抓到服务端回包。
2. **长时间稳定性**：只做了单局 + 冷启动烟测；未做长时挂机、跨关卡、退出重进。
3. **首领边界**：`#1` 只对 `characterType==2`(MONSTER) 生效是 IL 层面判定，未在首领战中逐条核对日志（`#1` 在首领战中应无效）。
4. **免广告是否仍会出视频**：代回调让奖励秒到；SDK 是否仍播视频未做逐帧确认。
5. **`MXYTZ_VERBOSE=0` 下的取证明细**：交付版默认静音，逐次明细需重编（`MXYTZ_VERBOSE 1`）才能取到。
