/* ================================================================
 *  ZxrDerive.h — 镇邪人 战斗补丁「结构派生」（2026-10-25）
 *
 *  为什么存在：混淆器每版都会重排短标识符（b5278 的 `f/h/_/S/M` → d5507 的
 *  `f/h/_/I/E` → cb9dc 的 `M/E/w/D/P`），也会重排模块别名（`d`/`s`/`l`…）。
 *  字面锚点因此逐版失效，native 热更兜底曾在 2026-10-01~24 期间**从未真正命中过**。
 *  本文件不再写死任何标识符：只认「语义形状」——
 *    定点函数名（FightChangeEntityHp / DamageCommonCalcFinal / CalcFinalAtkSpeedValue，
 *    6 个历史版本全部保留）+ 形参位置 + 语句模板，标识符一律从 bundle 现取。
 *
 *  纯 C++（std::string），无 Android/JNI 依赖 ⇒ 可被 host 侧单测直接编译
 *  （04_scripts/镇邪人/test_zxr_derive.cpp）。
 * ================================================================ */
#ifndef STARRY_ZXR_DERIVE_H
#define STARRY_ZXR_DERIVE_H

#include <cstddef>
#include <cstring>
#include <string>
#include <vector>

namespace zxrderive {

struct Plan {
    std::string anchor;  // 原脚本里被替换掉的原文（1 次命中）
    std::string insert;  // 追加到 anchor 之后的补丁片段
};

static inline bool IsIdentStart(char c) {
    return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '$';
}
static inline bool IsIdentChar(char c) {
    return IsIdentStart(c) || (c >= '0' && c <= '9');
}

// 从 pos 起解析一个标识符；成功时 pos 指向标识符末尾之后。
static inline bool ParseIdent(const std::string& s, size_t& pos, std::string* out) {
    if (pos >= s.size() || !IsIdentStart(s[pos])) return false;
    size_t start = pos;
    while (pos < s.size() && IsIdentChar(s[pos])) ++pos;
    if (out) out->assign(s, start, pos - start);
    return true;
}

static inline void SkipSpace(const std::string& s, size_t& pos) {
    while (pos < s.size() && (s[pos] == ' ' || s[pos] == '\t' || s[pos] == '\n' || s[pos] == '\r')) ++pos;
}

static inline bool Expect(const std::string& s, size_t& pos, const char* lit) {
    const size_t n = strlen(lit);
    if (s.compare(pos, n, lit) != 0) return false;
    pos += n;
    return true;
}

static inline bool FindFrom(const std::string& s, const std::string& lit, size_t from, size_t* pos) {
    if (from >= s.size() || lit.empty()) return false;
    const size_t p = s.find(lit, from);
    if (p == std::string::npos) return false;
    if (pos) *pos = p;
    return true;
}

// 在 pos 之前反查最近的标识符（跳过空白）；返回其起始位置。
static inline bool IdentBefore(const std::string& s, size_t pos, std::string* out, size_t* start) {
    while (pos > 0 && (s[pos - 1] == ' ' || s[pos - 1] == '\t')) --pos;
    size_t end = pos;
    size_t p = end;
    while (p > 0 && IsIdentChar(s[p - 1])) --p;
    if (p == end) return false;
    if (!IsIdentStart(s[p])) return false;
    if (out) out->assign(s, p, end - p);
    if (start) *start = p;
    return true;
}

// 解析 `name = function(a, b, c)` 的形参名，bodyStart 指向 '{' 之后。
static bool ParseFunctionParams(const std::string& s, const char* fnPattern,
                                std::vector<std::string>* params, size_t* bodyStart) {
    size_t pos = 0;
    if (!FindFrom(s, fnPattern, 0, &pos)) return false;
    pos += strlen(fnPattern);
    params->clear();
    SkipSpace(s, pos);
    if (pos < s.size() && s[pos] == ')') {  // 无参
        ++pos;
    } else {
        for (;;) {
            SkipSpace(s, pos);
            std::string name;
            if (!ParseIdent(s, pos, &name)) return false;
            params->push_back(name);
            SkipSpace(s, pos);
            if (pos < s.size() && s[pos] == ',') { ++pos; continue; }
            if (pos < s.size() && s[pos] == ')') { ++pos; break; }
            return false;
        }
    }
    SkipSpace(s, pos);
    if (pos >= s.size() || s[pos] != '{') return false;
    if (bodyStart) *bodyStart = pos + 1;
    return true;
}

// ---- 钩子 1：伤害倍率 / 无敌（DamageCommonCalcFinal 里的 `<dmg> = <obj>.baseDmg;`） ----
static bool DeriveDamage(const std::string& s, Plan* out, std::string* why) {
    std::vector<std::string> params;
    size_t body = 0;
    if (!ParseFunctionParams(s, "DamageCommonCalcFinal = function(", &params, &body)) {
        if (why) *why = "DamageCommonCalcFinal params not found";
        return false;
    }
    if (params.size() < 6) {
        if (why) *why = "DamageCommonCalcFinal params < 6";
        return false;
    }
    const size_t winEnd = std::min(s.size(), body + 40000);

    // casterEntity / targetEntity 从对象字面量里现取（不猜形参位次）
    size_t pCaster = std::string::npos, pTarget = std::string::npos;
    std::string casterVar, targetVar;
    {
        size_t p = 0;
        if (!FindFrom(s, "casterEntity: ", body, &p) || p > winEnd) {
            if (why) *why = "casterEntity field not found";
            return false;
        }
        p += strlen("casterEntity: ");
        SkipSpace(s, p);
        if (!ParseIdent(s, p, &casterVar)) {
            if (why) *why = "casterEntity ident not found";
            return false;
        }
        pCaster = p;
        if (!FindFrom(s, "targetEntity: ", body, &p) || p > winEnd) {
            if (why) *why = "targetEntity field not found";
            return false;
        }
        p += strlen("targetEntity: ");
        SkipSpace(s, p);
        if (!ParseIdent(s, p, &targetVar)) {
            if (why) *why = "targetEntity ident not found";
            return false;
        }
        pTarget = p;
    }

    // `<dmg> = <obj>.baseDmg;`（同一函数体内、DoSkillFinalDmgCalc 之后）
    size_t anchorStart = std::string::npos, anchorEnd = std::string::npos;
    std::string dmgVar;
    {
        size_t scan = body;
        while (scan < winEnd) {
            size_t p = 0;
            if (!FindFrom(s, ".baseDmg;", scan, &p) || p > winEnd) break;
            size_t objStart = 0;
            std::string objVar;
            if (!IdentBefore(s, p, &objVar, &objStart)) { scan = p + 1; continue; }
            // objVar 之前应是 " = "
            size_t q = objStart;
            while (q > body && (s[q - 1] == ' ' || s[q - 1] == '\t')) --q;
            if (q < 1 || s[q - 1] != '=') { scan = p + 1; continue; }
            --q;
            while (q > body && (s[q - 1] == ' ' || s[q - 1] == '\t')) --q;
            size_t dmgStart = 0;
            if (!IdentBefore(s, q, &dmgVar, &dmgStart)) { scan = p + 1; continue; }
            // 前一个非空白字符应为语句边界
            size_t r = dmgStart;
            while (r > body && (s[r - 1] == ' ' || s[r - 1] == '\t')) --r;
            if (r > body) {
                const char prev = s[r - 1];
                if (prev != ';' && prev != '}' && prev != '\n') { scan = p + 1; continue; }
            }
            anchorStart = dmgStart;
            anchorEnd = p + strlen(".baseDmg;");
            break;
        }
        if (anchorStart == std::string::npos) {
            if (why) *why = "baseDmg assignment not found";
            return false;
        }
        // 结构合理性：同段里应出现 DoSkillFinalDmgCalc（防误命中别处的 .baseDmg）
        size_t probe = 0;
        if (FindFrom(s, "DoSkillFinalDmgCalc", body, &probe) && probe < anchorStart) {
            // ok
        } else if (why) {
            *why = "DoSkillFinalDmgCalc not before baseDmg (sanity)";
            return false;
        }
    }

    out->anchor.assign(s, anchorStart, anchorEnd - anchorStart);
    out->insert = "\ntry{if(window.__starryZxrOn&&window.__starryZxrOn(1)&&" + casterVar +
                  "&&" + casterVar + ".heroCfg){" + dmgVar +
                  "*=100;}if(window.__starryZxrOn&&window.__starryZxrOn(2)&&" + targetVar + "&&" +
                  targetVar + ".heroCfg){" + dmgVar + "=0;}}catch(__zxrErr){}";
    (void)pCaster;
    (void)pTarget;
    return true;
}

// ---- 钩子 2：攻速（CalcFinalAtkSpeedValue 的 `return Math.max(<v>, .01);`） ----
static bool DeriveAttackSpeed(const std::string& s, Plan* out, std::string* why) {
    std::vector<std::string> params;
    size_t body = 0;
    if (!ParseFunctionParams(s, "CalcFinalAtkSpeedValue = function(", &params, &body)) {
        if (why) *why = "CalcFinalAtkSpeedValue params not found";
        return false;
    }
    if (params.size() < 2) {
        if (why) *why = "CalcFinalAtkSpeedValue params < 2";
        return false;
    }
    const std::string& entityVar = params[0];
    const size_t winEnd = std::min(s.size(), body + 8000);

    // 状态枚举别名：`<alias>.EnumEntityStatus.bossFakeControl`
    std::string statusAlias;
    {
        size_t p = 0;
        if (!FindFrom(s, ".EnumEntityStatus.bossFakeControl", body, &p) || p > winEnd) {
            if (why) *why = "EnumEntityStatus.bossFakeControl not found";
            return false;
        }
        if (!IdentBefore(s, p, &statusAlias, nullptr)) {
            if (why) *why = "EnumEntityStatus alias not found";
            return false;
        }
    }

    // 上限变量：`var <cap> = 3,`（同一函数体）
    std::string capVar;
    {
        size_t p = body;
        bool okCap = false;
        while (p < winEnd) {
            size_t v = 0;
            if (!FindFrom(s, "var ", p, &v) || v > winEnd) break;
            size_t q = v + 4;
            std::string name;
            if (!ParseIdent(s, q, &name)) { p = v + 4; continue; }
            size_t r = q;
            SkipSpace(s, r);
            if (r < s.size() && s[r] == '=') {
                ++r;
                SkipSpace(s, r);
                if (s.compare(r, 2, "3,") == 0) { capVar = name; okCap = true; break; }
            }
            p = v + 4;
        }
        if (!okCap) {
            if (why) *why = "atk-speed cap var (var X = 3,) not found";
            return false;
        }
    }

    // `return Math.max(<value>, .01);`
    size_t rpos = 0;
    std::string valueVar;
    {
        if (!FindFrom(s, "return Math.max(", body, &rpos) || rpos > winEnd) {
            if (why) *why = "return Math.max( not found";
            return false;
        }
        size_t q = rpos + strlen("return Math.max(");
        SkipSpace(s, q);
        if (!ParseIdent(s, q, &valueVar)) {
            if (why) *why = "Math.max value ident not found";
            return false;
        }
        size_t r = q;
        SkipSpace(s, r);
        if (s.compare(r, strlen(", .01);"), ", .01);") != 0) {
            if (why) *why = "Math.max(..., .01); shape mismatch";
            return false;
        }
    }

    out->anchor = "return Math.max(" + valueVar + ", .01);";
    out->insert = "\ntry{if(window.__starryZxrOn&&window.__starryZxrOn(3)&&" + entityVar + "&&" +
                  entityVar + ".heroCfg&&!" + entityVar + ".CheckEntityHaveStatus(" + statusAlias +
                  ".EnumEntityStatus.bossFakeControl)){" + valueVar + "=Math.min(" + valueVar + "*10," +
                  capVar + ");}}catch(__zxrErr){}";
    return true;
}

// ---- 钩子 3：全屏秒杀（FightChangeEntityHp 的序言 `var A = o && o.force, ...`） ----
static bool DeriveFullScreenKill(const std::string& s, Plan* out, std::string* why) {
    std::vector<std::string> params;
    size_t body = 0;
    if (!ParseFunctionParams(s, "FightChangeEntityHp = function(", &params, &body)) {
        if (why) *why = "FightChangeEntityHp params not found";
        return false;
    }
    if (params.size() < 4) {
        if (why) *why = "FightChangeEntityHp params < 4";
        return false;
    }
    const std::string& targetVar = params[0];   // 目标实体
    const std::string& casterVar = params[1];   // 施法者 entityId
    const std::string& amountVar = params[2];   // 伤害值（负数=伤害）
    const std::string& optVar = params[3];      // options
    const size_t winEnd = std::min(s.size(), body + 40000);

    // 模块别名：`<alias>.EntityManager.Inst()` / `<alias>.EnumEntityType.<Member>`
    std::string emAlias;
    {
        size_t p = 0;
        if (!FindFrom(s, ".EntityManager.Inst()", body, &p) || p > winEnd) {
            if (why) *why = "EntityManager alias not found";
            return false;
        }
        if (!IdentBefore(s, p, &emAlias, nullptr)) {
            if (why) *why = "EntityManager alias ident not found";
            return false;
        }
    }
    std::string enumAlias, enumMember;
    {
        size_t p = 0;
        if (FindFrom(s, ".EnumEntityType.", body, &p) && p < winEnd) {
            if (!IdentBefore(s, p, &enumAlias, nullptr)) enumAlias.clear();
            if (!enumAlias.empty()) {
                size_t q = p + strlen(".EnumEntityType.");
                std::string member;
                if (ParseIdent(s, q, &member)) enumMember = member;
                else enumAlias.clear();
            }
        }
        // 英雄分支优先：若同段出现 .EnumEntityType.Player 就用它
        size_t pp = 0;
        if (!enumAlias.empty() && FindFrom(s, ".EnumEntityType.Player", body, &pp) && pp < winEnd) {
            enumMember = "Player";
        } else if (!enumAlias.empty() && FindFrom(s, ".EnumEntityType.Hero", body, &pp) && pp < winEnd) {
            enumMember = "Hero";
        }
    }

    // 序言：`var A = <opt> && <opt>.force, B = <opt> && <opt>.trueDmg, C = <amt> < 0, D = <amt> > 0, E = 0;`
    size_t anchorStart = std::string::npos, anchorEnd = std::string::npos;
    {
        size_t pForce = 0;
        const std::string forceLit = optVar + " && " + optVar + ".force";
        if (!FindFrom(s, forceLit, body, &pForce) || pForce > winEnd) {
            if (why) *why = "options.force prologue not found";
            return false;
        }
        // 反查最近的 `var `
        size_t v = pForce;
        bool ok = false;
        const size_t floorPos = pForce > 400 ? pForce - 400 : body;
        while (v > floorPos) {
            if (s.compare(v - 1, 4, "var ") == 0) { ok = true; break; }
            --v;
        }
        if (!ok) {
            if (why) *why = "prologue 'var ' not found";
            return false;
        }
        anchorStart = v - 1;
        size_t q = anchorStart;
        std::string vA, vB, vC, vD, vE;
        if (!Expect(s, q, "var ") || !ParseIdent(s, q, &vA)) {
            if (why) *why = "prologue var name parse failed";
            return false;
        }
        if (!Expect(s, q, " = ") || !Expect(s, q, forceLit.c_str()) ||
            !Expect(s, q, ", ") || !ParseIdent(s, q, &vB)) {
            if (why) *why = "prologue clause 1/2 shape mismatch";
            return false;
        }
        const std::string trueDmgLit = " = " + optVar + " && " + optVar + ".trueDmg, ";
        if (!Expect(s, q, trueDmgLit.c_str()) || !ParseIdent(s, q, &vC)) {
            if (why) *why = "prologue clause 2 shape mismatch";
            return false;
        }
        const std::string ltLit = " = " + amountVar + " < 0, ";
        if (!Expect(s, q, ltLit.c_str()) || !ParseIdent(s, q, &vD)) {
            if (why) *why = "prologue clause 3 shape mismatch";
            return false;
        }
        const std::string gtLit = " = " + amountVar + " > 0, ";
        if (!Expect(s, q, gtLit.c_str()) || !ParseIdent(s, q, &vE)) {
            if (why) *why = "prologue clause 4 shape mismatch";
            return false;
        }
        if (!Expect(s, q, " = 0;")) {
            if (why) *why = "prologue clause 5 shape mismatch";
            return false;
        }
        anchorEnd = q;
        (void)vB;
        (void)vC;
        (void)vD;
        (void)vE;
    }

    // 注意：施法者判定必须落在**取出来的实体对象** __zxrCaster 上，
    // 不是 entityId 形参（`e` 是数字，`e.heroCfg` 恒 undefined ⇒ 功能静默失效）。
    std::string casterCond = "__zxrCaster.heroCfg";
    if (!enumAlias.empty() && !enumMember.empty()) {
        casterCond = "(__zxrCaster.heroCfg || __zxrCaster.EntityType === " + enumAlias +
                     ".EnumEntityType." + enumMember + ")";
    }

    out->anchor.assign(s, anchorStart, anchorEnd - anchorStart);
    out->insert = "\ntry{if(window.__starryZxrOn&&window.__starryZxrOn(4)&&" + amountVar +
                  "<0&&" + targetVar + "&&" + targetVar + ".monsterCfg&&!this.__starryZxrAoeBusy){var "
                  "__zxrCaster=" + emAlias + ".EntityManager.Inst().GetEntity(" + casterVar + ");if("
                  "__zxrCaster&&" + casterCond + "){var __zxrMap=" + emAlias +
                  ".EntityManager.Inst().entityMap;if(__zxrMap&&typeof __zxrMap.forEach===\"function\"){"
                  "this.__starryZxrAoeBusy=true;try{Array.from(__zxrMap.values()).forEach(function("
                  "__zxrEnt){if(__zxrEnt&&__zxrEnt!==" + targetVar + "&&!__zxrEnt.IsDead&&!"
                  "__zxrEnt.IsLogicDead&&__zxrEnt.monsterCfg){try{this.FightForceKillEntity(__zxrEnt," +
                  casterVar + ");}catch(__zxrOneErr){}}},this);}finally{this.__starryZxrAoeBusy=false;}"
                  "}}}}catch(__zxrErr){}";
    return true;
}

}  // namespace zxrderive

#endif  // STARRY_ZXR_DERIVE_H
