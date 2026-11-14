// 战斗决策纯函数（无 il2cpp/无全局状态），供 Mod.cpp 与宿主测试共用。
// 规则：无敌只对"本机为目标且 d<0"；百倍只对"本机为攻击者、目标非本机且 d<0"。
// 治疗(d>0)、0、NaN、inf 一律不改。
#ifndef KYZT_BATTLE_PURE_H
#define KYZT_BATTLE_PURE_H
namespace kb {

enum BattleAction { kBANone = 0, kBAGod = 1, kBAX100 = 2 };

constexpr float kClampLo = -2.1474835e9f;

struct BattleDecision { BattleAction action; float value; };

inline BattleDecision Decide(bool godOn, bool x100On, float d,
                             bool targetIsLocal, bool atkerIsLocal) {
    if (!(d < 0.0f)) return { kBANone, d };        // 0/正数/NaN 都不动
    if (godOn && targetIsLocal) return { kBAGod, 0.0f };
    if (x100On && atkerIsLocal && !targetIsLocal) {
        float m = d * 100.0f;
        if (m < kClampLo) m = kClampLo;
        return { kBAX100, m };
    }
    return { kBANone, d };
}

}  // namespace kb
#endif
