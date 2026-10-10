/*
 * 《炁术行者》战斗加速机制 —— 离线校验（Node + vm，无设备、无网络）
 *
 * 为什么单独一个闸门：
 *   战斗加速不是「推一个值」就完事，它靠两处锚点同时成立才可不破坏原版节奏：
 *     ⑭ GAME_SPEED 枚举改成实时取值器 —— 一处覆盖 6 个消费方（两套战斗 UI 的
 *        initSpeed/onClickSpeed/结算回落 + 4 份技能 CD 的除速分子）；
 *     ⑮ TweenSpeed.start 里抓 Scheduler，并在每帧「只对 >1 的 timeScale 纠偏」——
 *        场外 timeScale=1 绝不干预，结算慢镜 0.5 也保留。
 *   任一处语义写错，真机上会表现成「城里也变快 / 技能节奏飘 / 滑块没反应」，
 *   而这几类问题在静态锚点检查里看不出来。
 *
 * 用法：
 *   node 04_scripts/炁术行者/offline_speed_check.js --src <原始 assets/main/index.js>
 *   （脚本内部先跑 qsw_patch_js_star.py 生成补丁版，顺带复核 18 处锚点仍各命中 1 次）
 * 退出码：0 全过；1 有断言失败；2 参数/前置错误
 */
'use strict';

const fs = require('fs');
const os = require('os');
const path = require('path');
const vm = require('vm');
const { execFileSync } = require('child_process');

const HERE = __dirname;
const PATCHER = path.join(HERE, 'qsw_patch_js_star.py');

let pass = 0, fail = 0;
function ok(c, n) { c ? (pass++, console.log('  PASS ' + n)) : (fail++, console.log('  FAIL ' + n)); }

function argOf(name) {
    const i = process.argv.indexOf(name);
    return i >= 0 && process.argv[i + 1] ? process.argv[i + 1] : null;
}

const src = argOf('--src');
if (!src || !fs.existsSync(src)) {
    console.error('用法: node offline_speed_check.js --src <原始 assets/main/index.js>');
    process.exit(2);
}

/* ---- 0. 先跑补丁生成器：锚点失配会在这里以 rc!=0 爆出来 ---- */
const tmp = path.join(fs.mkdtempSync(path.join(os.tmpdir(), 'qswspd-')), 'patched_main.js');
try {
    const out = execFileSync('python3', [PATCHER, '--src', src, '--dst', tmp], { encoding: 'utf8' });
    ok(true, 'qsw_patch_js_star.py 跑通（锚点各命中 1 次）');
    const nHit = (out.match(/OK \(1\)/g) || []).length;
    ok(nHit === 18, '锚点命中数 = 18（实测 ' + nHit + '）');
} catch (e) {
    ok(false, 'qsw_patch_js_star.py 失败：' + String(e.stdout || e.message).slice(-400));
    console.log('\n结果: PASS=' + pass + ' FAIL=' + fail);
    process.exit(1);
}
const S = fs.readFileSync(tmp, 'utf8');

/* ---- 1. 从补丁版里抽出三段真实产物（不做任何人工复制，避免测试与产物脱节） ---- */
const hStart = S.search(/\/\*QSW-MOD v\d+s/);
const hEnd = S.indexOf('\n}();\n', hStart);
ok(hStart === 0 && hEnd > 0, '可抽出补丁 HEADER');
const HEADER = S.slice(hStart, hEnd + 6);

const gI = S.indexOf('E("GAME_SPEED",function(Q)');
const gEnd = S.indexOf('}({}))', gI);
ok(gI > 0 && gEnd > gI, '可抽出打补丁后的 GAME_SPEED 片段');
const GFRAG = S.slice(gI, gEnd + 6);

const tTail = ',t*window.__QSW_SCHED.getTimeScale())}}';
const tI = S.indexOf('n.start=function(){var e=this;window.__QSW_SCHED');
const tEnd = S.indexOf(tTail, tI);
ok(tI > 0 && tEnd > tI, '可抽出打补丁后的 TweenSpeed.start 片段');
const TFRAG = S.slice(tI, tEnd + tTail.length);

/* ---- 2. 沙盒 ---- */
const ctx = { console: { log() {} }, Object, Math, JSON, String, Number, Date, parseInt, parseFloat, Error };
ctx.window = ctx; ctx.globalThis = ctx;
vm.createContext(ctx);
vm.runInContext(HEADER, ctx, { filename: 'HEADER' });
ok(typeof ctx.__QSW_APPLY === 'function', 'HEADER 装载后 __QSW_APPLY 就绪');

let GAME_SPEED = null;
ctx.E = function (n, v) { if (n === 'GAME_SPEED') GAME_SPEED = v; return v; };  // 等价 SystemJS _export
vm.runInContext(GFRAG, ctx, { filename: 'GAME_SPEED' });
ok(!!GAME_SPEED, 'GAME_SPEED 片段可执行');
ok(GAME_SPEED[1.2] === 'speed1' && GAME_SPEED[1.6] === 'speed2', '反向映射保留（1.2→speed1 / 1.6→speed2）');
ok(GAME_SPEED.speed1 === 1.2 && GAME_SPEED.speed2 === 1.6, '默认（滑块 1）= 原版 1.2/1.6，完全不干预');
ctx.__QSW_APPLY(1, 0, 3);
ok(GAME_SPEED.speed1 === 3 && GAME_SPEED.speed2 === 3, '滑块 3（上限）→ 两档速度常量都变 3');
ctx.__QSW_APPLY(1, 0, 5);
ok(GAME_SPEED.speed1 === 3 && ctx.__QSW_SPD === 3, '滑块 5 → 钳到上限 3（实测 5 会被服务端 9998 拒判）');
ctx.__QSW_APPLY(1, 0, 1);
ok(GAME_SPEED.speed1 === 1.2 && GAME_SPEED.speed2 === 1.6, '滑块回 1 → 恢复原版 1.2/1.6');
ctx.__QSW_APPLY(2, 10, 99);
ok(GAME_SPEED.speed1 === 3 && ctx.__QSW_SPD === 3, '滑块 99 超上限钳到 3（2026-10-10：上限 10→3）');
ctx.__QSW_APPLY(0, -5, 0);
ok(GAME_SPEED.speed1 === 1.2 && ctx.__QSW_DR === 0 && ctx.__QSWK === 1, '下界钳制：spd→1 / dr→0 / k→1');
ctx.__QSW_APPLY(5, 50);
ok(ctx.__QSW_SPD === 1 && GAME_SPEED.speed1 === 1.2, '旧式两参调用（缺 spd）→ 速度保持不干预');

/* ---- 3. TweenSpeed：抓 Scheduler + 每帧只提速不降速 ---- */
const forced = [];
const sched = { _ts: 1, getTimeScale() { return this._ts; }, setTimeScale(v) { this._ts = v; forced.push(v); } };
const ticks = [];
const r = { tick(t) { ticks.push(t); }, getScheduler() { return sched; } };
ctx.r = r; ctx.n = { oldTick: r.tick };
vm.runInContext(TFRAG, ctx, { filename: 'TweenSpeed' });
ok(ctx.__QSW_SCHED === null, '捕获前 __QSW_SCHED 为空');
ctx.n.start.call({ oldTick: r.tick });
ok(ctx.window.__QSW_SCHED === sched, 'start() 抓到 Scheduler（后续取值走同一对象）');

function run(ts, spd) { forced.length = 0; ticks.length = 0; sched._ts = ts; ctx.__QSW_APPLY(1, 0, spd); r.tick(1); return ticks[0]; }
ok(run(1, 5) === 1 && forced.length === 0, '场外 timeScale=1 → 绝不干预（城里不加速）');
ok(run(1.2, 1) === 1.2 && forced.length === 0, '滑块关(1) + 战斗(1.2) → 不接管，原版行为');
ok(run(1.2, 3) === 3 && forced[0] === 3, '滑块 3（上限）+ 战斗 → 当帧纠偏到 3，dt 按 3 计');
ok(run(3, 2) === 2 && forced[0] === 2, '战斗中改滑块（3→2）→ 当帧生效');
ok(run(1.2, 9) === 3 && forced[0] === 3, '滑块 9 → 先钳到上限 3 再纠偏（上限不可被越过）');
ok(run(0.5, 3) === 0.5 && forced.length === 0, '结算慢镜 0.5 → 不接管（保留慢镜）');

try { fs.rmSync(path.dirname(tmp), { recursive: true, force: true }); } catch (e) { /* 临时目录清理失败不影响结论 */ }

/* ---- 4. 弹窗总闸：GameUtil.alert 包装只静默白名单键（含「参数传输」原文） ---- */
ok(/e\.alert=function\(e,n,o,i\)\{try\{if\("string"==typeof e[^]{0,200}__QSW_ALERTSKIP\(e\)\)\{[^]{0,120}return\}\}catch/.test(S),
    'GameUtil.alert 定义处已插入静默包装（单一锚点覆盖全部调用方）');
const skip = ctx.__QSW_ALERTSKIP;
ok(typeof skip === 'function', 'HEADER 装载后 __QSW_ALERTSKIP 就绪');
ok(typeof ctx.__QSW_AUDIT === 'function', 'HEADER 装载后 __QSW_AUDIT 就绪（证据行 QSWALERT <key>）');
ok(skip('netcode_9998') === true, '白名单 9998 -> 静默（用户主诉「参数传输遇到问题」）');
ok(ctx.__QSW_NETGUARD(9998) === true && ctx.__QSW_NETGUARD(9997) === true,
    '原 ⑫ 守卫 __QSW_NETGUARD(9998) 也命中（旧值 1 与 !0 严格比较恒 false = 上轮静默失效根因）');
ok(ctx.__QSW_NETGUARD(179) === false, '原守卫对非白名单码仍为 false（未扩大静默范围）');
ok(skip('netcode_9999') === true && skip('netcode_9997') === true, '白名单 9999/9997 -> 静默');
ok(skip('参数传输遇到问题，点击确定重试') === true, '服务端 body 直接给中文原文 -> 也静默');
ok(skip('netcode_-10') === false, '致命 -10 不静默（客户端异常仍要弹+重启）');
ok(skip('netcode_-8') === false && skip('netcode_-9') === false && skip('netcode_-12') === false
    && skip('netcode_-13') === false, '其它致命码不静默');
ok(skip('netcode_9988') === false && skip('netcode_9987') === false && skip('netcode_9986') === false
    && skip('netcode_9989') === false, '战斗验证失败族不静默（真实错误照弹）');
ok(skip('netcode_179') === false && skip('netcode_196') === false && skip('netcode_-2') === false,
    '其它 netcode 键不静默');
ok(skip(undefined) === false && skip(null) === false && skip(123) === false && skip({}) === false,
    '非字符串入参不误吞');
ok(skip('server_unlist') === false && skip('UI_STANDARD_1') === false, '非 netcode 文案不误吞');

/* ---- 5. 网络埋点（v13s）：只记日志、不改行为；心跳不上报以免刷屏 ---- */
const NLOG = [];
ctx.window.StarryH5 = { log(m) { NLOG.push(String(m)); } };
ctx.window.__QSW_NETREQ('FightAction_act', { callback: 'FightAction_actReq', uid: 123 });
ok(NLOG.length === 1 && /^QSWALERT QSWREQ cmd=FightAction_act cb=FightAction_actReq body=/.test(NLOG[0]),
    'QSWREQ 埋点：rspCmd + callback + body 进日志');
NLOG.length = 0;
ctx.window.__QSW_NETREQ('LoginAction_heart', { callback: 'LoginAction_heart' });
ok(NLOG.length === 0, '心跳请求不打点（callback 含 heart，避免刷屏）');
NLOG.length = 0;
ctx.window.__QSW_NETERR(9998, 'FightAction_actRsp', 'FightAction_act', { a: 1 });
ok(NLOG.length === 1 && /QSWERR code=9998 cmd=FightAction_actRsp cb=FightAction_act data=/.test(NLOG[0]),
    'QSWERR 埋点：服务端错误码 + rspCmd + callback + data 进日志');
NLOG.length = 0;
ctx.__QSW_AUDIT('任意文案');
ok(NLOG.length === 1 && NLOG[0] === 'QSWALERT 任意文案', '__QSW_AUDIT 对任意 alert 文案都记（不再只记 netcode_）');
ok(ctx.__QSW_TRUNC('x'.repeat(300), 10) === 'xxxxxxxxxx~', '超长内容截断为 n 字符 + ~（logcat 不被刷爆）');
ok(S.indexOf('window.__QSW_NETREQ&&window.__QSW_NETREQ(t,e)') > 0, '⑰ 发送埋点已进产物（base_request 内）');
ok(S.indexOf('window.__QSW_NETERR&&window.__QSW_NETERR(i.code,r,i.callback,i.data)') > 0,
    '错误码证据行已进产物（⑦ neterr 注入体，位于 responseErrorCode 调用之前）');
ok(S.indexOf('__QSW_AUDIT("SKIPPED "+e)') > 0, '被吞的弹窗额外留一条 SKIPPED 证据');

console.log('\n结果: PASS=' + pass + ' FAIL=' + fail);

process.exit(fail === 0 ? 0 : 1);
