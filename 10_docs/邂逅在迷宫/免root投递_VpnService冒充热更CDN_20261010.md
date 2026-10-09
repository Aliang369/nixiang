# 邂逅在迷宫：免 root / 免电脑投递补丁 bundle（VpnService 冒充明文热更 CDN）

日期：2026-10-10 · 设备 `f9a116a2`（OPPO OPD2404 / Android 16）· 目标 `cn.alltune.endless` v2.6.53

**最终交付**：`08_output/邂逅在迷宫/星光助手_v2.9_xhzm.apk`（md5 `9e8a3646352dd8e39438965c489a82d4`，versionCode 29）· 收尾记录见 [收尾_20261010.md](收尾_20261010.md)（阶段状态、门禁 G1–G3、保留表、清理授权记录）

## 一、要解决的问题

三个作弊功能都在热更 DLL（`Assets/EndlessStrike/Res/HotUpdate/Dll` → `endlessstrikedll_*.bundle`
里的 `EndlessStrike.dll`，HybridCLR 解释执行）里。要生效，补丁 bundle 必须出现在：

```
/sdcard/Android/data/cn.alltune.endless/files/Bundles/
```

而这条路径**免 root 写不进去**（Android 11+ FUSE 隔离他人 `Android/data`；
`MANAGE_EXTERNAL_STORAGE`/`requestLegacyExternalStorage`/SAF/MediaStore 全部无效，实测 `isDir=false canRead=false canWrite=false`）。
改包重签又被 FairGuard 文件级校验在 20–27 s 干净自退（零改动只重签同样死）。

**结论路径**：不投递文件，而是**冒充游戏的明文热更 CDN**，让游戏**自己**去下载。

## 二、机制（关键事实）

- 游戏热更 CDN：`http://install2.hotune.com/bundle3/`，**明文 HTTP/80**（无 TLS，可伪造）。
  扁平路径：`.../bundle3/Android/<裸文件名>`；版本化 `.../bundle3/2.6.53.13/Android/` 与扁平**逐字节相同**，
  故可只认扁平基址。DNS 由本机 DNS 化名 `install2.hotune.com → 10.9.9.9` 完成。
- 每次冷启动客户端**必定**请求 `versions.json`（实测 `SET download URL 1` 后立刻取）⇒ 这是注入点。
- 下载触发判据 = `data[]` 数组里的 **hash 与本地清单不一致**（`bundleVersion` 字段是死字段，改它无效）。
- 伪造三件套（自洽）：
  1. `versions.json`：BuildPatch 指向**新 hash 名** `buildpatch_v63_ba2deed1a86fd27eb34d6d517cc38962.json`；
  2. `buildpatch_v63_ba2deed1….json`：`bundles[0] = {name:"endlessstrikedll.bundle", size:9800982, hash:"9639893c…"}`；
  3. `endlessstrikedll_9639893c….bundle`：41 B xasset 前缀 hash 重算 + 内含补丁 DLL。
- ★**决定性发现（两个实验）**：
  - 关掉 VPN 重启游戏 → `diff size=0`、`load endless dll … len=24288768`（**官方**）⇒ 本地留着我们的
    新 hash 名文件也**完全不被引用**；补丁**只在经由本机伪造 CDN 的那次启动生效**。
  - 用 root 把伪造 buildpatch 的**内容**写进**官方文件名**下（`buildpatch_v63_3e1d3be6….json`）→
    `diff size=0` 却 `load endless dll … len=24680448`（**补丁**）⇒ **客户端只按「文件名是否存在」判定，
    从不校验本地文件内容哈希，也不校验 size**。
  - 推论：永久化的唯一可能是「把伪清单内容写进官方文件名」，而下载路径因存在性判定
    **永不重新下载已存在的文件名** ⇒ **免 root 的永久化做不到**，每次冷启动必须经过本机 CDN。
    （免 root 唯一办法是让官方文件名**当时不存在**，这需要删游戏目录文件，做不到。）

## 三、实现（助手 App 内，零外部服务器）

`mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/menu/net/`：

| 文件 | 职责 |
|---|---|
| `FakeCdn.java` | CDN 常量（`CDN_HOST`/`FAKE_CDN_IP=10.9.9.9`/`MSS=1400`）、覆盖表 `OVERRIDES`（3 条，按文件名 → assets）、`content()` 缓存、`PASSED/SERVED/MISSED` 计数、`realCdnAddress()` |
| `TunPackets.java` | 手写 IPv4/TCP/UDP 组包 + 一补数校验和 |
| `HttpResponder.java` | `parse()` 解析请求、`buildOverride()` 命中覆盖（未命中返 null）、`forwardRequest()` 透传改写（剔 `Connection` 行、加 `Connection: close`） |
| `DnsInterceptor.java` | DNS 化名 `install2.hotune.com → 10.9.9.9`；其余域名用 `protect()` 上游转发 |
| `TcpRelay.java` | **用户态最小 TCP 栈**（SYN 随机 ISN、窗口/重传/快速重传/零窗探测/close、每连接工作线程） |
| `CdnVpnService.java` | `VpnService`：`addRoute` 仅 `10.9.0.0/24`、`10.9.9.9/32`、`118.183.236.0/24` + 上游 DNS `/32`；`addDisallowedApplication(自己)` 防自环；非覆盖路径**回源真实 CDN**；**注入完自动关**（`AUTO_CLOSE_IDLE_MS=15000`） |

配套：`04_scripts/邂逅在迷宫/xhzm_forge_cdn.py`（生成三件套）、`xhzm_repack_bundle.py`（重打包 bundle）。

### 必修的三个 bug（实测踩到）

1. **SYN 的 ISN 从未初始化**（`Conn.isn=0`）⇒ SYN 占序号 0，首段数据 seq=0 被当重传丢弃 ⇒
   `curl` 报 `Received HTTP/0.9 when not allowed`、下载 29 KB/s（慢 100×、333 s）。
   修：`nc.isn = (SystemClock.elapsedRealtime()+sport) & 0x7fffffffL; sndUna = sndNxt = isn+1;`
2. **`established` 门槛挡住重传**：只在 `sndUna > isn` 时置位，握手 ACK 不会置位 ⇒ `tick()` 永不重传。修：`ack >= isn+1` 即置位。
3. **注入完不自动关**：`injectionDone` 只在 `tick()` 判定，而连接常在对端 FIN 后就被回收 ⇒ 永不置位。
   修：改为「任何一条覆盖请求下发完毕即注入完成」+ ACK/FIN 两处置位。

### 另一处必修（用户实测踩到）

`HttpResponder` 原来对非覆盖路径返 **404** ⇒ 游戏真实资源下载被打断：
`Unable to load Dependencies Assets/EndlessStrike/EndlessStrikeGame.unity` → `NullReferenceException at GameLogicEntrance.StartGame()`
→ loading 卡 `progress 0.16` 死循环。修：**非覆盖路径一律回源真实 CDN 透传**（支持 Range/206，实测 5.8 MB/s）。

## 四、验证证据（v2.9，2026-10-10）

```
04:36:56,539 InitHotDll Init Url= http://install2.hotune.com/bundle3/
04:36:56,971 ← 助手伪造 versions.json 下发（注入点，505 B）
04:36:57,182 Versions.UpdateAsync Success,diff size=0
04:36:57,808 load endless dll, using 184 ms,len=24680448   ← 补丁 DLL（官方 24288768）
04:37:10,732 goto main page → 04:37:11,412 enter game      ← 不再卡 loading
04:37:12,099 注入完成，自动关闭热更注入通道 → tun loop exit ← 15 s 自动关
```

分项：补丁包经本隧道 9,800,982 B / 1.32 s（7.4 MB/s，比真实 CDN 的 3.2 s 快）；`-r 0-99` 返 206；
未覆盖 `esdatadll_4f4c6ea2….bundle` 透传 200/4,444,030 B / 5.8 MB/s。
状态文件 `0 100 1 1 0 0 0 0` ⇒ 伤害×100 / 无敌 / 无限技能，三项实测可用。

## 五、遗留边界

- **每次开局要在助手页点一次「开启热更注入」**（点完 15 s 自动关）。永久化免 root 不可能（见 §二推论）。
- 卸载游戏清 `Android/data` ⇒ 需重新注入（会自动重新下载补丁包）。
- 助手日志有历史遗留噪声：`等待游戏库 libil2cpp.so… Ns` 轮询（in-APK 形态的遗迹，standalone 下无意义）、
  `[桥接] exempt hidden apis via JNI 失败（隐藏方法）` 告警；均无害，未清理。
- 用户明确选择**不做**「游戏启动自动开启注入」（使用情况访问权限方案），保持手动点一次。
- ✅ 已补做（2026-10-10 04:42–04:47）：**首次注入整链从零复测通过**（本地补丁文件移走 → 冷启动 →
  0.92 s 下完 9.8 MB → `load endless dll len=24680448` → 15 s 自动关），落盘 md5 与 assets 一致；
  `post_install_precheck.sh` **PASS_TO_HUMAN**（证据 `09_test/邂逅在迷宫/precheck_20261010_044339`）。
  预检要点：预检的 `native menu shown` 标记只在菜单 attach 那一刻打一次，**必须先 force-stop 助手**
  再跑预检、并在预检启动游戏后再拉起助手，否则标记落在录制窗口外会误判段=B。

## 六、失败路线（勿重复）

- 改包/重签（含零改动重签、静态 patch `libFairGuard.so` 的 `exit_group` stub @ `0x42F100`）⇒ 20–27 s 自退。
- `MANAGE_EXTERNAL_STORAGE` / `requestLegacyExternalStorage` / SAF / MediaStore / `run-as` 写他人 `Android/data` ⇒ 全失败。
- `iptables -t nat ... -m string --string "install2.hotune.com"` 做劫持 ⇒ **永不命中**（nat 表只在 SYN 无负载时遍历，拿不到 Host）。
  可用的替代是按网段 `-t nat -A OUTPUT -p tcp -d 118.183.236.0/24 --dport 80 -j DNAT`（仅在服务器侧做过验证，**
  已还原**；方案 B「外挂服务器」因用户明确禁止动其服务器而被否决）。
