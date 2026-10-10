/* 自动生成，请勿手改 —— 由 04_scripts/无悔华夏/whrhx_rebaseline.sh 产出（P0-②）。
 * 生成时间(UTC): 2026-10-10T19:30:03Z
 * 来源 dump.cs : dump.cs
 *   sha256      : 01fc2211a49ca106176ed1bc0a0acb964b81025af9c08495143bd9bcc5f7baba
 * 来源 runtime so: libil2cpp_runtime_20261011.so
 *   sha256      : 79adda770c528633213efe26bd084d14839c58a3a5382007de19cfc3d897aec9
 * 站点解析：唯一 40 / 未解析 0；枚举漂移 0；常量条目 41（含 kExpect 行并入 50 条）。
 *
 * 本 so 的 PT_LOAD#0 是 p_offset=0/p_vaddr=0 ⇒ 文件偏移 == RVA == VA，
 * 所以 kExpectGen[].firstInsn 就是「直接 seek 到该 RVA 读 4 字节小端」的结果。
 * WhrhxMod.cpp 的 ResolveRva() 优先用 kExpectGen 做序言校验（比手写 kExpect 可信），
 * 手写 kExpect 表已删除：它的常量名+tag 被当作**输入**，首指令一律从 so 现读。
 * RVA 在按名之后，属「可选加速」；kExpectStale[] 是明确拒用名单。
 */
#ifndef WHRHX_RVA_GEN_H
#define WHRHX_RVA_GEN_H

struct WhrhxGenExpect {
    uintptr_t rva;
    uint32_t firstInsn;
    const char* tag;
};

#define WHRHX_GEN_RVA_ADD_RES_CAMPAIGN                 0x6EE88B0  /* AddResCampaign, was 0x6EE88B0 (same) */
#define WHRHX_GEN_RVA_ADD_RES_LEB                      0x67F0428  /* AddRes, was 0x67F0428 (same) */
#define WHRHX_GEN_RVA_ARMY_CORRECT_LEB                 0x701C990  /* ArmyCorrectMgr, was 0x701C990 (same) */
#define WHRHX_GEN_RVA_ARMY_GET_MAX_CAN_SEND            0x7016B98  /* ArmyGetMaxCanSendNum, was 0x7016B98 (same) */
#define WHRHX_GEN_RVA_ARMY_GET_MAX_SEND                0x7017094  /* ArmyGetMaxSendNum, was 0x7017094 (same) */
#define WHRHX_GEN_RVA_BATTLE_START                     0x6921C44  /* StartBattle, was 0x6921C44 (same) */
#define WHRHX_GEN_RVA_BATTLE_UPDATE_TIME               0x691BFBC  /* UpdateBattleTime, was 0x691BFBC (same) */
#define WHRHX_GEN_RVA_CHECK_RES_LEB                    0x67F47A8  /* CheckRes, was 0x67F47A8 (same) */
#define WHRHX_GEN_RVA_COST_RES_LEB                     0x67F45BC  /* CostRes, was 0x67F45BC (same) */
#define WHRHX_GEN_RVA_COST_SINGLE_LEB                  0x67E5470  /* CostSingleRes, was 0x67E5470 (same) */
#define WHRHX_GEN_RVA_COUNTRY_GET_RES_CNT              0x67F1B30  /* CountryGetResCnt, was 0x67F1B30 (same) */
#define WHRHX_GEN_RVA_EXTRA_MINISTER_TRY_ACTIVE        0x790CD04  /* ExtraMinisterTryActive, was 0x790CD04 (same) */
#define WHRHX_GEN_RVA_EXTRA_MINISTER_UI_EXECUTE        0x790E4D8  /* ExtraMinisterUiExecute, was 0x790E4D8 (same) */
#define WHRHX_GEN_RVA_GET_CITY_MGR_LEB                 0x6E0FD70  /* GetCityData, was 0x6E0FD70 (same) */
#define WHRHX_GEN_RVA_GET_COUNTRY_MGR_LEB              0x701DAE0  /* GetCountryDataById, was 0x701DAE0 (same) */
#define WHRHX_GEN_RVA_GET_DELTA_TIME                   0x63CF098  /* get_deltaTime, was 0x63CF098 (same) */
#define WHRHX_GEN_RVA_IS_PLAYER_LEB                    0x67DF754  /* IsPlayerCountry, was 0x67DF754 (same) */
#define WHRHX_GEN_RVA_KB_ADD_RES                       0x67F0428  /* AddRes, was 0x67F0428 (same) */
#define WHRHX_GEN_RVA_KB_ADD_RES_CAMP                  0x6EE88B0  /* AddResCampaign, was 0x6EE88B0 (same) */
#define WHRHX_GEN_RVA_KB_ARMY_CORRECT                  0x701C990  /* ArmyCorrectMgr, was 0x701C990 (same) */
#define WHRHX_GEN_RVA_KB_CHECK_RES                     0x67F47A8  /* CheckRes, was 0x67F47A8 (same) */
#define WHRHX_GEN_RVA_KB_COST_SINGLE                   0x67E5470  /* CostSingleRes, was 0x67E5470 (same) */
#define WHRHX_GEN_RVA_KB_GET_CITY_MGR                  0x6E0FD70  /* GetCityData, was 0x6E0FD70 (same) */
#define WHRHX_GEN_RVA_KB_GET_COUNTRY_MGR               0x701DAE0  /* GetCountryDataById, was 0x701DAE0 (same) */
#define WHRHX_GEN_RVA_KB_GET_DELTA                     0x63CF098  /* get_deltaTime, was 0x63CF098 (same) */
#define WHRHX_GEN_RVA_KB_ON_COUNTDOWN                  0x691BDEC  /* OnCountDown, was 0x691BDEC (same) */
#define WHRHX_GEN_RVA_KB_SET_END                       0x691C428  /* BattleSetEnd, was 0x691C428 (same) */
#define WHRHX_GEN_RVA_KB_START_TIMER                   0x691B9E8  /* StartTimer, was 0x691B9E8 (same) */
#define WHRHX_GEN_RVA_KB_UPDATE_COST                   0x691C348  /* UpdateBattleCostTime, was 0x691C348 (same) */
#define WHRHX_GEN_RVA_MINISTER_ADD_FAVOR               0x7633D40  /* MinisterAddFavor, was 0x7633D40 (same) */
#define WHRHX_GEN_RVA_MINISTER_GET_FAVOR               0x7CD6F58  /* MinisterGetFavorite, was 0x7CD6F58 (same) */
#define WHRHX_GEN_RVA_MINISTER_TRY_ACTIVE              0x72A08B8  /* MinisterTryActive, was 0x72A08B8 (same) */
#define WHRHX_GEN_RVA_MINISTER_UI_EXECUTE              0x72A1108  /* MinisterUiExecute, was 0x72A1108 (same) */
#define WHRHX_GEN_RVA_POLICY_CAN_ACTIVE                0x72855B0  /* PolicyCanActive, was 0x72855B0 (same) */
#define WHRHX_GEN_RVA_POLICY_CHECK_DISABLE             0x728E3EC  /* PolicyCheckDisable, was 0x728E3EC (same) */
#define WHRHX_GEN_RVA_POLICY_EXECUTE                   0x728B7B8  /* PolicyExecute, was 0x728B7B8 (same) */
#define WHRHX_GEN_RVA_POLICY_ITEM_SHOW_STATE           0x72903F4  /* PolicyItemShowState, was 0x72903F4 (same) */
#define WHRHX_GEN_RVA_POLICY_UI_ACTIVE                 0x6DFC098  /* PolicyUiActive, was 0x6DFC098 (same) */
#define WHRHX_GEN_RVA_POLICY_UI_EXECUTE                0x6DFD3B0  /* PolicyUiExecute, was 0x6DFD3B0 (same) */
#define WHRHX_GEN_RVA_RESEARCH_TECH                    0x7B55B0C  /* ResearchTech, was 0x7B55B0C (same) */
#define WHRHX_GEN_RVA_TECH_POLICY_RESEARCH             0x723FA40  /* TechPolicyResearch, was 0x723FA40 (same) */

static const WhrhxGenExpect kExpectGen[] = {
    {0x63CF098, 0xF81E0FF3, "get_deltaTime"},
    {0x67DF754, 0xA9BE4FF4, "IsPlayerCountry"},
    {0x67E5470, 0xD10283FF, "CostSingleRes"},
    {0x67F0428, 0xA9BC5FF8, "AddRes"},
    {0x67F1B30, 0xA9BE4FF4, "CountryGetResCnt"},
    {0x67F45BC, 0xD10143FF, "CostRes"},
    {0x67F47A8, 0xD10183FF, "CheckRes"},
    {0x691B9E8, 0xD10143FF, "StartTimer"},
    {0x691BDEC, 0xA9BE4FF4, "OnCountDown"},
    {0x691BFBC, 0xD10143FF, "UpdateBattleTime"},
    {0x691C348, 0xFC1D0FE8, "UpdateBattleCostTime"},
    {0x691C428, 0xD10243FF, "BattleSetEnd"},
    {0x6921C44, 0xD10143FF, "StartBattle"},
    {0x6DFC098, 0xD10303FF, "PolicyUiActive"},
    {0x6DFD3B0, 0xF81D0FF5, "PolicyUiExecute"},
    {0x6E0FD70, 0xF81D0FF5, "GetCityData"},
    {0x6EE88B0, 0xF81D0FF5, "AddResCampaign"},
    {0x7016B98, 0xD101C3FF, "ArmyGetMaxCanSendNum"},
    {0x7017094, 0xFC1C0FE8, "ArmyGetMaxSendNum"},
    {0x701C990, 0xD10303FF, "ArmyCorrectMgr"},
    {0x701DAE0, 0xF81D0FF5, "GetCountryDataById"},
    {0x723FA40, 0xA9BE4FF4, "TechPolicyResearch"},
    {0x72855B0, 0xA9BC5FF8, "PolicyCanActive"},
    {0x728B7B8, 0xFC1C0FE8, "PolicyExecute"},
    {0x728E3EC, 0xA9BB67FA, "PolicyCheckDisable"},
    {0x72903F4, 0xD104C3FF, "PolicyItemShowState"},
    {0x72A08B8, 0xD101C3FF, "MinisterTryActive"},
    {0x72A1108, 0xFC1D0FE8, "MinisterUiExecute"},
    {0x7633D40, 0xA9BE4FF4, "MinisterAddFavor"},
    {0x790CD04, 0xD101C3FF, "ExtraMinisterTryActive"},
    {0x790E4D8, 0xFC1D0FE8, "ExtraMinisterUiExecute"},
    {0x7B55B0C, 0xF81D0FF5, "ResearchTech"},
    {0x7CD6F58, 0xF81D0FF5, "MinisterGetFavorite"},
};
static const size_t kExpectGenCount = sizeof(kExpectGen) / sizeof(kExpectGen[0]);

// 按 rva 查自动生成的期望序言（命中即代表这份 RVA 对本机运行时 so 有效）。
static inline const WhrhxGenExpect* FindExpectGen(uintptr_t rva) {
    for (size_t i = 0; i < kExpectGenCount; ++i) {
        if (kExpectGen[i].rva == rva) return &kExpectGen[i];
    }
    return nullptr;
}

// RVA 已漂移的常量（手写表声明的序言与本机运行时 so 该地址的实际字节不符）：
// 这些地址上已经不是目标函数，绝不能当「加速路径」用 —— 显式列出便于 ResolveRva 拒绝，
// 并让日志一眼看出「这条 RVA 表已过期」，而不是静默钩到别的函数上。
static const WhrhxGenExpect kExpectStale[] = {
    {0x3394D5C, 0x00000000, "UpdateBattleTime (RVA_UPDATE_BATTLE_LEB)"},
    {0x33951C8, 0x00000000, "BattleSetEnd (RVA_BATTLE_SET_END)"},
    {0x339A9E4, 0x00000000, "StartBattle (RVA_START_BATTLE_LEB)"},
    {0x3BDDBBC, 0x00000000, "OnBattleEnd (RVA_ON_BATTLE_END_LEB)"},
    {0x3C506AC, 0x00000000, "PolicyDataMgr.GetPolicyData (RVA_POLICY_GET_DATA)"},
    {0x3C51A6C, 0x00000000, "PolicyDataMgr.UpdatePolicyTime (RVA_POLICY_UPDATE_TIME)"},
    {0x3C538B8, 0x00000000, "PolicyDataMgr.FinishPolicyTimeAnyway (RVA_POLICY_FINISH_ANYWAY)"},
    {0x8BB9860, 0x00000000, "get_deltaTime (RVA_GET_DELTA_LEB)"},
};
static const size_t kExpectStaleCount = sizeof(kExpectStale) / sizeof(kExpectStale[0]);

static inline const WhrhxGenExpect* FindStaleGen(uintptr_t rva) {
    for (size_t i = 0; i < kExpectStaleCount; ++i) {
        if (kExpectStale[i].rva == rva) return &kExpectStale[i];
    }
    return nullptr;
}

#endif  // WHRHX_RVA_GEN_H
