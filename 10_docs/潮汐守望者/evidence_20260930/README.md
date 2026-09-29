# 潮汐守望者 — 决定性证据（2026-09-30）

> 只列最短判断链与复现命令；**完整原始日志不入库**（`06_dumped/`、`*.log` 已被 .gitignore 排除，本地保留）。

## 1. 登录被服务端 `-37` 拒绝（主结论）

**输入**：设备 `f9a116a2` 上**未改动的官方原版**（`com.gf.cxswz` 1.2.56.375.3），2026-09-29 23:46–23:50 共 6 次登录尝试。

**决定性行**（每次尝试的结尾三行，取自游戏自带日志 `…/files/dragon2019/Flog/*.log`）：

```
[FLog 2026-09-29 23:50:04+08:00 371] [Info]-job Login_Auth start
[FLog 2026-09-29 23:50:04+08:00 385] <--[LoginServer]2 | 0 | -37
[FLog 2026-09-29 23:50:04+08:00 385] [Debug]-[xjy][385] Push ---> Name:Form_SystemTips  Docker:null
```

6 份日志逐一提取，**全部为 `-37`**：

| 日志文件 | LoginServer 响应 |
|---|---|
| `2026-09-29 23-46-17Z.log` | `-37` |
| `2026-09-29 23-48-01Z.log` | `-37` |
| `2026-09-29 23-48-53Z.log` | `-37` |
| `2026-09-29 23-49-12Z.log` | `-37` |
| `2026-09-29 23-49-34Z.log` | `-37` |
| `2026-09-29 23-49-53Z.log` | `-37` |

**同段日志的对照事实**：

```
[Info]-QuickSdkListener.onLoginSuccess:{"userName":"szn696n1s15","userId":"6258729",...,"msg":"success"}   ← SDK 登录成功
[Info]-CNChannelName:and_cntaptap          ← TapTap 渠道
[Info]-IsEmulator:False                    ← 客户端自检未判为模拟器
[Info]-loginip="login.cxswz.moontonapp.com" loginport="4000"  ← 失败跳是游戏登录服务器
```

**复现命令**（最短）：

```bash
PID=$(adb shell pidof com.gf.cxswz | tr -d '\r')
adb exec-out "su -c 'cd /sdcard/Android/data/com.gf.cxswz/files/dragon2019 && tar -cf - Flog'" > flog.tar
tar -xf flog.tar -C device_logs
# 注意：日志是 UTF-8 带 BOM，rg 默认按二进制跳过，必须加 -a
rg -a -o -- '\[LoginServer\][^\r\n]*' device_logs/Flog/*.log
```

## 2. 封禁文案不在客户端（故判定为服务端下发）

对以下载体逐字节搜 `角色被限制登录` / `游戏数据异常` / `限制登录` / `检测到您的`，**全部 0 命中**：

- `classes.dex` … `classes15.dex`（15 份，解压后）
- `resources.arsc`
- 全部 `assets/**`（175 MB，含 `Resources.dat`、`init.unity3d`、`localsource`）
- `liblogic.bytes`（真 IL2CPP 库 187,532,400 B，设备解压目录）
- `global-metadata.dat`（真元数据 22,448,368 B；全库仅 436 条中文串，仅含 `登录成功`/`正在登录中`/`加载技能动画数据异常` 等，无封禁类文案）

```bash
# 复现（在解压目录/包目录内）
for s in 角色被限制登录 游戏数据异常 限制登录 检测到您的; do
  printf '%s -> ' "$s"; LC_ALL=C grep -a -c "$s" <文件>
done
```

> 注意：`strings` 默认只认 ASCII，会漏掉 UTF-8 中文串；必须用 `LC_ALL=C grep -a` 按字节搜（工作区已知坑）。

## 3. 风控库能力边界（`libgp.so` = GameProtect）

| 关键字 | `libgp.so` 命中数 |
|---|---|
| `frida` | 2 |
| `xposed` / `debugger` / `hook` | 1 / 1 / 1 |
| `magisk` / `su_file` / `supersu` / `busybox` / `root` / `selinux` / `ptrace` / `emulator` | **0** |

⇒ 该库强项是**反 Hook / 反调试 / 反 Frida-Xposed**，未见经典 root 文件扫描实现。

## 4. NOT PROVEN

- `-37` 的触发面（账号级 vs 设备/环境级）——需「换账号 / 换非 root 机」区分实验。
- GameProtect 是否参与 `-37` 判定。
- 改签后 QuickSDK / TapTap 登录是否真的失败（未做改包实验）。
