/* ================================================================
 *  ZmxyPatchJs.h — 造梦西游战斗 JS 拼接体（明文常量）
 *
 *  被 ZmxyMod.cpp 的 se::ScriptEngine::evalString hook 使用：
 *  命中锚点后在运行时把下面两段插入游戏战斗脚本，再交回原 eval。
 *
 *  ★ 占位符 %s = JS 侧门控类（ZMXY_JS_ACT）。这段 JS 在游戏进程里被
 *    Cocos 的 JS 引擎解析，语法错会让整份战斗脚本 eval 失败——
 *    因此外科脚本 surgical_zmxy_1.1.0.sh 会用 node --check 对
 *    「替换占位符后的拼接体」做语法关（pre_install_gate 查1 同源）。
 *
 *  门控链路：JS jsb.reflection → com.xm4399.zm6.AppActivity.get_ret/get_val
 *            → com.starry.magic.cocos.ZmxyHooks（classes7.dex）
 * ================================================================ */
#pragma once

// JS 侧门控类全限定名（由外科包在 AppActivity 里补静态转发方法）
#define ZMXY_JS_ACT "com.xm4399.zm6.AppActivity"

// 幂等标记：脚本里已含本前缀（重载/二次拼接）时直通，不重复插桩
static const char* kZmxyJsMarker = "starry_zmxy_";

/* ----------------------------------------------------------------
 * 伤害倍率（滑块 1..100）+ 无敌
 *   插入位置：承伤函数头 `if (0 == e || this.isDead()) return 0;` 之后
 *   语义：this = 受击对象，e = 伤害（负数即掉血）
 *     · 受击方与主玩家同营 → get_ret(2)=无敌 → return 0（免疫）
 *     · 异营（打敌人）        → get_val(1)=倍率 → e *= 倍率
 * ---------------------------------------------------------------- */
static const char* kZmxyDmgHookFmt = R"ZMXYJS(
/*starry_zmxy_dmg*/ if (e < 0 && this.mGameLogic && this.mGameLogic.getMainPlayer && "undefined" != typeof jsb && jsb.reflection) { try { var _mp = this.mGameLogic.getMainPlayer(0); if (_mp) { if (this.mCamp == _mp.mCamp) { if (jsb.reflection.callStaticMethod("%s","get_ret","(I)Z",2)) return 0; } else { var _mul = jsb.reflection.callStaticMethod("%s","get_val","(I)I",1) | 0; if (_mul > 1) e = e * _mul; } } } catch (_e) {} } )ZMXYJS";

/* ----------------------------------------------------------------
 * 无限技能（无 CD + 不耗蓝）
 *   插入位置：释放技能前置校验 `... !isCanReleaseSkill()) return "1";` 之前
 *   语义：逐帧缓存一次反射查询（mGameTimeFrame 变更才重读）；
 *         trueSkill 时清消耗计数并回灌满蓝，随后 return null 放行
 * ---------------------------------------------------------------- */
static const char* kZmxySkillHookFmt = R"ZMXYJS(
/*starry_zmxy_uls*/ if (this.mPlayer && this.mPlayer.isPlayer && this.mGameLogic) { var _f = this.mGameLogic.mGameTimeFrame; if (this.__ulsF !== _f) { this.__ulsF = _f; this.__uls = false; try { if ("undefined" != typeof jsb && jsb.reflection) this.__uls = !!jsb.reflection.callStaticMethod("%s","get_ret","(I)Z",3); } catch (_e) {} } if (this.__uls) { try { var _pl=this.mPlayer; _pl.mConsumeMpRet=0; _pl.mRetCountSkillComsumeMp=0; if (_pl.setMp) { var _v=999999; try{ if(_pl.getMaxMp)_v=_pl.getMaxMp(); else if(_pl.getMp)_v=Math.max(_pl.getMp()||0,999999);}catch(_m){} _pl.setMp(_v); } } catch (_m2) {} return null; } } )ZMXYJS";
