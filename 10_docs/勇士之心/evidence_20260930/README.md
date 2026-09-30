# 勇士之心 1.0.4 星光复刻 证据（2026-09-30）

> 本目录只留决定性证据行（见 `decisive_lines.txt`）；完整 logcat 不入库。
> 复现链：`04_scripts/勇士之心/regrab_ysxz_hotupdate.sh`（TAG=20260930）→ `decrypt_hotupdate_ysxz.py`
> → `simulate_ysxz_discovery.py` → `pipeline.sh 勇士之心 next`（build_so → surgical → sign → install）
> → 非 root 机再跑 `SERIAL=96f0d883 VERIFY_MODE=sig ALLOW_UNINSTALL=1 bash 04_scripts/勇士之心/install_ysxz.sh`。

## 1. 设备与产物身份

| 项目 | 值 |
|---|---|
| 验收设备（主） | 小米 Mi 11 `M2102K1AC`，serial `96f0d883`，Android 14 / SDK 34，arm64-v8a，**无 root** |
| 对照设备 | OPD2404，serial `f9a116a2`，Android 16 / SDK 36，KernelSU root |
| 包名 | `com.fightingstars.games.warrior`（TapTap 渠道） |
| 安装版本 | `1.0.4 / vc5` |
| 官方底包（真机只读重抓） | md5 `03857f4c71544ba32d625ce445af378b`，65,472,824 B，签名 `CN=shan, OU=bingo, O=bingo, L=guangzhou, ST=guangdong, C=CN` |
| 加密 cocos so（资产位） | `assets/apksecuresolib/arm64-v8a/libcocos2djs.so` sha256 `bf1d3f2fa7c9258824b2490137adfcc024239f1815f0e99a64eb407424363752`，15,164,720 B |
| 星光 SO | md5 `5e0d30c366fdf167086f08b809ccf4d9`（sha256 `b4f806c24e5b9bc798dac8dde2e11d38d595151ce8582dc4f03e5bae49f7909f`） |
| 星光包（唯一正式包） | `08_output/勇士之心/勇士之心_1.0.4_surgical-releaseSigned.apk` md5 `e45a5a2e0efe3913a50a2058458c0e81`，67,119,895 B |
| 设备端 base.apk md5 | `e45a5a2e0efe3913a50a2058458c0e81`（两台机均与产物一致） |
| 签名 | `CN=StarryMagic`（星光发布证书 `01_tools/starry_release.jks`） |

## 2. 这份证据证明什么

- 保护壳在**未 root 的真机**上被正确解锁：`spoof hit … md5=79a6a62f…` 即官方证书 MD5，
  且壳随后成功解密并 `android_dlopen_ext` 明文 cocos so（否则 `cocos_android_app_init` 不会出现）。
- 旧版阻塞项（`AppInfo.parameters` NPE、启动 5~13 s 后干净退出）均已消除：
  日志里出现 `------------------onCreate 初始化-------------------`（真实 Application 的 onCreate 被调用），
  且进程长期存活（用户在其上实测功能）。
- 关键机制：保护线程主动 `_exit(0)` 被 libc 钩子 + seccomp 陷阱拦下，只结束该线程：
  `[YsxzExit] _exit 被调用 code=0 (#101)` / `该线程=s.games.warrior tid=20967 已被单独结束（进程存活）`。
- 菜单换位（伤害倍率 #1 滑块 1..50、秒杀 #2）在真机上按新序号上报数值，滑块可达上限 50。

## 3. 这份证据不证明什么（NOT PROVEN）

- 未做长时间战斗/多局稳定性统计；未做服务端结算重放。
- **root 设备（OPD2404）上仍会「检测到不安全的设备环境」直接闪退**（用户反馈，本次不处理）；
  小米非 root 机正常。
- Android 14 上 `VMRuntime.setHiddenApiExemptions` 已改名 → `exemptHiddenApis` 报
  `NoSuchMethodException`（日志中两条 E），但签名伪造与壳解密不依赖该豁免，属无害噪声。
