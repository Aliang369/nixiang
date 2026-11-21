#ifndef STARRY_STAMP_H
#define STARRY_STAMP_H

// ============================================================
// 星光包构建戳（build stamp）—— 2026-11-21 已退役
// ------------------------------------------------------------
// 该戳原本只服务 OTA 守卫（PlayOtaManager.invalidateOtaIfApkUpdated 判「包内 so 是否比
// 玩家已下载的 OTA so 新」）。OTA 热更体系整体下线后：Java 侧 OtaSoStamp 已删除、so 加载
// 只走包内路径，戳不再有任何读者 ⇒ 定义、Application.mk 的 STARRY_BUILD_EPOCH 注入、
// 34 个 Android_*.mk 的 -Wl,--undefined=kStarryBuildStamp 强制保留、装机门禁「查8」一并移除。
//
// 本头文件保留为空壳：仍有历史 mk/源文件 include 它，删文件会让旧构建树直接失败。
// ============================================================

#endif
