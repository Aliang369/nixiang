/* ================================================================
 *  MxytzBattle.h — 冒险与征途 (com.mxyzt.gzmy 1.5.9) 战斗三功能（全新实现）
 *
 *  设计前提（与旧版 MxytzMod.cpp 里那套战斗代码**没有任何共享状态**）：
 *    · 本模块只做三件事：①秒杀小怪 ②无敌 ③全模式技能伤害增加
 *    · 三个功能各占一条独立判据/独立开关/独立写入，互不复用「合法性」之外的量；
 *      「合法性」本身（玩家侧判定）只读游戏自己的 `_player` / `ulRoleId` / `_hitter`，
 *      不做任何扫描、不建任何演员表。
 *    · 热路径（每次命中/每次施法）只做「内存读 + 乘算 + 一次写」，**不做 maps 查询、
 *      不打日志、不做字符串比较、不遍历容器**。
 *    · 一切写入都是**可逆**的：功能关闭 / ForceClear 时把写过的值还原（不留脏数据）。
 *
 *  调用方（唯一）：Games/MxytzMod.cpp 的 HybridCLR 解释器入口分发
 *    （RVA 0x207F798 / 0x207DCCC，按「类::方法」名学习后按指针表分发）。
 *    本模块**不安装任何 hook**，只接收已命中的帧。
 * ================================================================ */
#ifndef STARRY_MXYTZ_BATTLE_H
#define STARRY_MXYTZ_BATTLE_H

#include <cstddef>
#include <cstdint>

namespace MxytzBattle {

/** 帧来源（与 MxytzMod.cpp 的 K_* 位一一对应，调用方按命中的位调用对应入口）。 */
enum Frame : int {
    FRAME_HURT = 0,      // CBattleActorUtil::Hurt             → #1 秒杀小怪
    FRAME_DOATTACK = 1,  // StarsCollider::DoAttack            → #2 无敌（命中作废）
    FRAME_PROJADD = 2,   // ActionCommand_AddProjectile::AddProjectile → #3 只观测
    FRAME_PROJEXEC = 3,  // ActionCommand_AddProjectile::Execute        → #3 真正落点
};

/** 开关同步（Mxytz_Tick 每拍调；内部只在值变化时做动作）。 */
void SetSwitches(bool killMob, bool invincible, bool skillDmg);

/** 开关变化（菜单回调路径）—— #3 关闭时**立刻**还原投射物字段。 */
void OnFeatureChanged(int featNum, bool enabled);

/** 400ms 心跳：刷新玩家身份；无敌开启时维持玩家免疫位。 */
void OnTick();

/** HybridCLR 解释器入口命中后的分发。
 *  `argBase`：静态方法 = 0、实例方法 = 1（= `this` 占的槽数）；
 *  `a1`     ：**实参值内联**的 8 字节槽区首地址（HybridCLR 入口约定，v30 血证）。 */
void OnFrame(int frame, void* a1, uint8_t argBase);

/** 授权失效 / 卸载前清场：还原一切写入、清空缓存。 */
void ForceClear();

/** 诊断汇总一行（30s 一次），写入 `out`。 */
void StatLine(char* out, size_t cap);

}  // namespace MxytzBattle

#endif  // STARRY_MXYTZ_BATTLE_H
