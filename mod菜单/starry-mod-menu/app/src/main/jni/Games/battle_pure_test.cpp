#include "旷野征途BattlePure.h"
#include <cmath>
#include <cstdio>
int fails = 0;
void ck(const char* n, bool ok) { std::printf("%s %s\n", ok ? "PASS" : "FAIL", n); if (!ok) ++fails; }
int main() {
    using kb::Decide; using kb::kBANone;
    auto d0 = Decide(false, false, -10.f, true, false);
    ck("两开关默认关：无敌目标也不改", d0.action == kBANone && d0.value == -10.f);
    auto d1 = Decide(true, false, -10.f, true, false);
    ck("只开无敌：本机受击置 0", d1.action == kb::kBAGod && d1.value == 0.f);
    auto d2 = Decide(false, true, -10.f, false, true);
    ck("只开百倍：本机输出×100", d2.action == kb::kBAX100 && d2.value == -1000.f);
    auto d3 = Decide(true, true, -10.f, true, false);
    ck("两开关同开：无敌优先（少改）", d3.action == kb::kBAGod);
    auto d4 = Decide(true, true, 25.f, true, false);
    ck("治疗正数不动", d4.action == kBANone && d4.value == 25.f);
    auto d5 = Decide(true, true, 0.f, true, false);
    ck("0 伤害不动", d5.action == kBANone);
    auto d6 = Decide(false, true, -1.0e8f, false, true);
    ck("百倍限幅", d6.value <= kb::kClampLo);
    auto d7 = Decide(false, true, -2.0e7f, false, true);
    ck("未越界不钳位", std::fabs(d7.value + 2.0e9f) < 1.f);
    auto d8 = Decide(false, true, std::nanf(""), false, true);
    ck("NaN 不动", d8.action == kBANone && std::isnan(d8.value));
    auto d9 = Decide(false, true, -10.f, true, true);
    ck("自伤（本机打自己）不乘", d9.action == kBANone);
    auto da = Decide(false, false, -1e30f, false, false);
    ck("两开关关：极值也不动", da.action == kBANone && da.value == -1e30f);
    std::printf(fails ? "SUMMARY FAIL=%d\n" : "SUMMARY PASS=0\n", fails);
    return fails ? 1 : 0;
}
