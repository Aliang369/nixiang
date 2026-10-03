/* ================================================================
 *  GdzfdjMod.h — 敢达争锋对决（com.kingnet.gundam.mg.normal）IL2CPP mod 模块
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh Gdzfdj gdzfdj
 *  产出四件套：GdzfdjMod.h/.cpp + GdzfdjEntry.cpp + Android_gdzfdj.mk
 *
 *  目标库：libil2cpp.so（arm64-v8a；本包 native 仅 libil2cpp/libmain/libOni/
 *          libEncryptorP/libn/lib_burst_generated，无 libtprt.so ⇒ B 类无 ACE）
 *  dump 来源：07_patched/敢达争锋对决/dump_out/dump.cs（46MB）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *     默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装 / orig 必调 /
 *     reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *     v3.1 追加：并发装钩互斥 / 致命信号兜底 / 槽位熔断
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_GDZFDJ_MOD_H
#define STARRY_GDZFDJ_MOD_H

#include <jni.h>

/* featureList 下标 = featNum（Category_ 占下标不占功能名）：
 *   [0] "Category_战斗"
 *   [1] "Slider_伤害倍率:1:100:1"
 *   [2] "Toggle_超级定怪"
 *   [3] "Slider_全局加速:1:10:1"
 * 语义铁律：滑块 1 = 关（默认必须 1，禁止默认开）。 */
enum GdzfdjFeat : int {
    GDZFDJ_FEAT_DMG_MUL   = 1,   /* Slider_伤害倍率:1:100:1 */
    GDZFDJ_FEAT_ROOT      = 2,   /* Toggle_超级定怪 */
    GDZFDJ_FEAT_SPEED_MUL = 3,   /* Slider_全局加速:1:10:1 */
};

/* 与菜单声明段的 min/max 必须逐字一致，改一处必须同步改另一处。 */
#define GDZFDJ_DMG_MUL_MAX    100
#define GDZFDJ_SPEED_MUL_MAX  10

/* ModEntry.h 注册表契约（签名不可改） */
bool Gdzfdj_TryInit(JavaVM* vm);
bool Gdzfdj_IsReady();
void Gdzfdj_OnFeatureChanged(int featNum, bool enabled);
void Gdzfdj_OnValueChanged(int featNum, int value);
void Gdzfdj_Tick(const bool* features, const int* values);
void Gdzfdj_ForceClear();

#endif  /* STARRY_GDZFDJ_MOD_H */
