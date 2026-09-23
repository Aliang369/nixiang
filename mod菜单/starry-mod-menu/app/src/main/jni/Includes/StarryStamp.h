#ifndef STARRY_STAMP_H
#define STARRY_STAMP_H

// ============================================================
// 星光包构建戳（build stamp）
// ------------------------------------------------------------
// 用途：PlayOtaManager.invalidateOtaIfApkUpdated 需要判断「包内 so 是否比玩家已下载的
//   OTA so 新」。旧实现读 PackageInfo.lastUpdateTime，被乐变（免安装 overlay APK 的
//   mtime）抖动误判为「覆盖安装」→ 每次冷启动都把刚下载的补丁清掉，设置页永远显示
//   「尚未下载补丁」（无悔华夏 2026-09-24 实测，logcat: PlayOta clearLocalOta）。
//
//   现在改为读 so 内烘焙的 epoch 戳：
//     apkStamp > otaStamp  → 包内 so 更新（真·覆盖安装）→ 清 OTA
//     否则（相等/OTA 更新） → 保留，乐变怎么抖 lastUpdateTime 都不影响
//
// 约定：戳是 ASCII 字面量，直接躺在 .rodata 里，格式
//     STARRYSTAMP1:EPOCH=<10位秒级时间戳>;
//   Java 侧按字节扫描 so 文件（so 是 hidden visibility 且被 strip，dlsym 拿不到）。
//
// 构建系统注入：Application.mk 里 -DSTARRY_BUILD_EPOCH=<git commit 时间>；
//   同源重建戳相同 → 不会误清玩家已下载的补丁。未注入时用下面的兜底值。
// ============================================================

#ifndef STARRY_BUILD_EPOCH
#define STARRY_BUILD_EPOCH 1790184000   // 兜底：2026-09-24 04:00:00 UTC
#endif

#define STARRY_STAMP_STR2(x) #x
#define STARRY_STAMP_STR(x) STARRY_STAMP_STR2(x)
#define STARRY_STAMP_MARKER "STARRYSTAMP1:EPOCH=" STARRY_STAMP_STR(STARRY_BUILD_EPOCH) ";"

extern "C" {
// .rodata 里的戳字面量；由 starry_build_stamp_marker() 引用，保证不被 --gc-sections 丢掉
extern const char kStarryBuildStamp[];
const char *starry_build_stamp_marker(void);
long long starry_build_epoch(void);
}

#endif
