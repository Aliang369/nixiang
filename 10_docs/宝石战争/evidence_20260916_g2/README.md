# 宝石战争 6.0.012 星光包 G2 实测证据（2026-09-16 23:26–23:32）

## 产物

- APK：`08_output/宝石战争/宝石战争_6.0.012_surgical-releaseSigned.apk`
  - md5 `83cf807dec1edae3535436e3532e1bff`
  - sha256 `2e18788faefab48c65be253aee7fb3d639b8dc5c58baf4250cfebc36fb30f3db`
  - 包内 `libstarry_magic.so` md5 `8abcd959bcf5851962d58f562b0153e7`
  - 包内 `System.Network.dll.bytes` md5 `a29e7dd8678536da56b1aed31c476baf`
- 设备 `f9a116a2` base.apk md5 == 产物 md5（23:26:33 卸载安装，`firstInstallTime=lastUpdateTime`）。

## 状态通道现场回读

`/data/data/com.acing.gow.adv/files/starry_state.txt`：

```
0 53 1 1 0 0 0 0
```

- feature1（攻击倍率）= `53`
- feature2（上帝模式）= `1`
- feature3（无限技能）= `1`
- 与菜单实际设置一致 → Java `StateMirror` 写入与热更 C# 读取链路打通。

## 运行表现（用户口径）

- 攻击倍率、上帝模式、无限技能三项全部生效。
- 上一版的“回合结束/消除后卡死”未复现。
- 清数据后重新登录 TapTap 并进入对局正常。

## 日志

- `logcat_after_fix.txt`（27,114 行，23:26 清缓冲后）
  - `FATAL EXCEPTION` / `ANR in` / `SIGSEGV` / tombstone：**0**
  - Unity 异常：**0**
- 结论：功能生效且无崩溃/无卡死。

## 复现入口

```
bash 04_scripts/pipeline.sh 宝石战争 status
# 同签名覆盖安装（设备已是星光签名时）
bash 04_scripts/pipeline.sh 宝石战争 next
# 切官方原版对照（手动，不参与 next）
bash 04_scripts/pipeline.sh 宝石战争 run install_official
```

## 关键结论（写进经验）

1. HybridCLR 解释执行的热更程序集里，`[DllImport]` 解析不到符号，调用必抛异常；
   热更 C# 不能反向调 native，改走文件镜像（Java 写 / 热更 C# 读）。
2. 桥接调用必须 try/catch 兜底：异常一旦打断战斗协程，表现就是“卡死”而非报错。
3. 桥接“没反应”时先看 native 侧调用计数：0 = 根本没打通，而不是开关逻辑写错。
