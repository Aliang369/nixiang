# §4 游戏 Hook 主体 + 战斗功能层（L2281–L3100）

来源文件（只读）：`03_decompiled/勇士之心_cardkey_20261007/提取_补丁_v66_原始.js`
行号一律指该文件。资源版本锚点：注释自称 **`1.1.3.2`**（L2334、L2650、L3068 `REMOTE_BUILD = 'res-1.1.3.2'`、L3104）。
全局管理器（游戏侧）：**`window.$eZ`**，其战斗管理器为 **`$eZ.battleMgr`**。

---

## 0. 上一段延续：`guardExit()`（L2285–L2301）

```js
2290: g.exitProgram = function () {
2291:     if (CFG.noExit === false) return orig.apply(this, arguments);
2293:     window.__WX_EXIT_BLOCKED__ = (window.__WX_EXIT_BLOCKED__ || 0) + 1;
2294:     window.__WX_EXIT_STACK__ = (new Error()).stack || '';
2297: };
2298: g.__wxGuard = true; window.__WX_NOEXIT__ = true;
```
- 目标：`window.gdkjsb.exitProgram`（L2287）。幂等标记 `g.__wxGuard`（L2288 前置判空 + L2298 置位）。
- `CFG.noExit === false` 时才放行原函数；默认 `noExit` 为 true（见下 CFG）。
- L2281–2284 注释明确：**EmutorUtils / NativeInfo / CocosBridgeAPI 的 boolean 全改 false，以及 `System.exit` / `Runtime.exit` / `halt` / `Process.killProcess` 拦截必须在 dex 里做 —— 见 `dex_loader/Hooks.java`**。JS 侧只能截 `gdkjsb.exitProgram` 这一层。

---

## 1. `var CFG`（L2304–L2313）+ `keyOf`（L2314–L2321）+ `save`（L2322–L2330）

### 1.1 默认值来源 `DEF`（L108）

```js
108: var DEF = { on: false, value: 99999, lockHp: false, lockMp: false, autoFight: false, speed: 1.0 };
```
注意 `DEF` **没有** `noCd` / `gspeed` / `noExit` 字段。

### 1.2 `CFG` 逐字段（L2304–L2313，L2331 补 `on`）

| 字段 | 行 | 表达式 | 默认值 / 判据 |
|---|---|---|---|
| `on` | L2305 声明 `false`；L2331 赋值 | `storageGet('wx_on','0') === '1'` | 默认 **false** |
| `value` | L2305 | `parseInt(storageGet('wx_value', DEF.value), 10) \|\| DEF.value` | 默认 **99999**；解析出 0/NaN 时回落 `DEF.value` |
| `lockHp` | L2306 | `storageGet('wx_lockHp','0') === '1'` | 字符串严格等于 `'1'` 才 true |
| `lockMp` | L2307 | `storageGet('wx_lockMp','0') === '1'` | 同上 |
| `autoFight` | L2308 | `storageGet('wx_autoFight','0') === '1'` | 同上 |
| `noCd` | L2309 | `storageGet('wx_noCd','0') === '1'` | 同上 |
| `gspeed` | L2310 | `parseFloat(storageGet('wx_gspeed','1')) \|\| 1.0` | 默认 **1.0**，`0/NaN` 回落 1.0 |
| `speed` | L2311 | `parseFloat(storageGet('wx_speed','1')) \|\| 1.0` | 默认 **1.0**，同上 |
| `noExit` | L2312 | `storageGet('wx_noExit','1') !== '0'` | 默认 **true**（只有显式存 `'0'` 才关） |

`on` 是唯一没有在对象字面量里读存储的字段 —— 存储读取被挪到 L2331（因为 L2322 的 `save()` 定义在中间）。

### 1.3 `keyOf(name)`（L2314–L2321）—— 中文菜单名 → CFG 键

```js
2315: if (name === '英雄锁血') return 'lockHp';
2316: if (name === '锁定蓝量') return 'lockMp';
2317: if (name === '解锁金卡与自动战斗') return 'autoFight';
2318: if (name === '技能无CD') return 'noCd';
2319: if (name === '图内全局加速') return 'gspeed';
2320: return 'on';
```
- 兜底 `'on'`。
- **未覆盖的名字**：`'英雄攻击'` → 兜底 `'on'`（正确，因为攻击开关就是 `CFG.on`）；**`'人物移速'` 也落到 `'on'`** —— 移速没有独立 key 映射，是这份补丁的遗留缺口（复刻时若要 `keyOf` 通用化，需自己补 `'人物移速' → 'speed'`）。

### 1.4 `save()`（L2322–L2330）

```js
2323: storageSet('wx_value', CFG.value); storageSet('wx_on', CFG.on ? '1' : '0');
2324: storageSet('wx_lockHp', CFG.lockHp ? '1' : '0');
2325: storageSet('wx_lockMp', CFG.lockMp ? '1' : '0');
2326: storageSet('wx_noCd', CFG.noCd ? '1' : '0');
2327: storageSet('wx_gspeed', String(CFG.gspeed));
2328: storageSet('wx_autoFight', CFG.autoFight ? '1' : '0'); storageSet('wx_speed', CFG.speed);
2329: try { if (window.__WX_SAVE_HOOK__) window.__WX_SAVE_HOOK__(); } catch (e) {}
```
- 写入 8 个键：`wx_value`/`wx_on`/`wx_lockHp`/`wx_lockMp`/`wx_noCd`/`wx_gspeed`/`wx_autoFight`/`wx_speed`。
- **`wx_noExit` 不落盘**（只能靠外部写入）。
- 末尾回调外部钩子 `window.__WX_SAVE_HOOK__`（用于把状态同步到 UI / 其它模块）。

### 1.5 持久化载体 `storageGet` / `storageSet`（L764–L778）

```js
764: function storageGet(k, d) {
765:     try { var v = cc.sys.localStorage.getItem(k); if (v !== null && typeof v !== 'undefined') return v; } catch (e) {}
769:     var m = kvLoad(); return (m[k] === undefined) ? d : m[k];
770: }
772: function storageSet(k, v) {
773:     var ok = false;
774:     try { cc.sys.localStorage.setItem(k, String(v)); ok = true; } catch (e) {}
775:     try { var m = kvLoad(); m[k] = String(v); var f = kvFile(); if (f) writeFile(f, JSON.stringify(m)); ok = true; } catch (e) {}
777: }
```
- **双写**：`cc.sys.localStorage`（主）+ 一个文件型 KV（`kvLoad`/`kvFile`/`writeFile`，见 L750–762 一带），两者任一成功即算成功。复刻时若自己的工程没有文件 KV，只用 `cc.sys.localStorage` 即可，键名照抄。

---

## 2. 运行时状态 `ST`（L2341–L2345）与 `featureError`（L2349–L2351）

```js
2341: var ST = {
2342:     hits: 0, lock: 0, lockMp: 0, mpHook: false, noDmg: 0, noCd: false, auto: false, gold: false,
2343:     moveHook: false, moveSource: '', gsEnts: 0,
2344:     battleSerial: 0, errors: {}
2345: };
2346: var BATTLE = null;        // 当前战斗 world 对象缓存
2347: var entMark = 0;          // entities() 的 O(1) 去重标记
2349: function featureError(name, err) { try { ST.errors[name] = String(err && err.message || err); } catch (e) {} }
```
- `ST.errors` 是被 L7081–L7135 的诊断写出（`wx_state.txt`）消费的；后续动态新增的字段：`acFixed`(L2622)、`renderHits`/`renderFields`/`renderDamage`/`renderHooks`(L2696–L2706)、`hitBus`(L2631)、`lastBattleAt`(L3004)、`exitT`/`acT`(L7162/L7164)。

---

## 3. `BattleCore`（L2353–L2497）—— **怎么从游戏内部拿战斗数据（最该照抄的部分）**

### 3.1 三条取数路径，全部通过全局对象 `$eZ` 与字段名，不用 `cc` 的 API

```js
2354: function controller() {
2356:     var mgr = window.$eZ && window.$eZ.battleMgr;
2357:     return (mgr && mgr.bEu) || null;
2358: }
```
- **controller = `window.$eZ.battleMgr.bEu`**。

```js
2361: function world() {
2362:     var ctrl = controller(), out = null;
2363:     try { if (ctrl && typeof ctrl.nM === 'function') out = ctrl.nM(); } catch (e) {}
2364:     return out || (ctrl && (ctrl.nh || ctrl.ni)) || null;
2365: }
```
- **world = `ctrl.nM()`**（方法），失败回落 **`ctrl.nh` / `ctrl.ni`** 字段。

```js
2367: function bind() {
2368:     var next = world();
2369:     if (next !== BATTLE) {
2370:         BATTLE = next;
2371:         ST.battleSerial++;
2372:         ST.lock = 0;
2373:         try { FeatureManager.onBattleChanged(); } catch (e) {}
2374:     }
2375:     return BATTLE;
2376: }
```
- 每 tick 调用；**world 引用一变即视为「换战斗」**，自增 `ST.battleSerial`、清 `ST.lock`，并触发 `FeatureManager.onBattleChanged()`。

### 3.2 实体与属性访问

```js
2378: function stat(ent) { var value = ent && ent.hw; return value && typeof value === 'object' ? value : null; }
2383: function time(ent) { var value = ent && ent.hz; return value && typeof value === 'object' ? value : null; }
```
- **`ent.hw` = 数值/状态对象（stat）**；**`ent.hz` = 时间/倍速对象（time）**。这两个是全部数值改写的抓手。

```js
2388: function hero() {
2389:     var ctrl = controller(), value = null;
2390:     try { if (ctrl && typeof ctrl.ny === 'function') value = ctrl.ny(); } catch (e) {}
2391:     if (!value || !stat(value)) return null;
2393:     value.__wxHero = true;  stat(value).__wxHero = true;
2396:     return value;
2397: }
2399: function isHero(ent) {          // __wxHero 双标记 + 引用相等，三重判定
2402:     if (ent.__wxHero) return true;
2403:     var s = stat(ent); if (s && s.__wxHero) return true;
2405:     var current = hero(); if (current === ent) return true;
2408: }
```
- **hero = `ctrl.ny()`**；拿到后**立刻在实体和 stat 两处打 `__wxHero = true`**，后续所有「只对英雄生效」的功能靠这两个标记（`NoCdFeature` L2836、`MoveSpeedFeature` L2873 用的就是 `this.__wxHero`）。

```js
2411: function entityFromRef(b, ref) {
2412:     if (!b || typeof b.gi !== 'function') return null;
2413:     var id = ref;
2415:     if (ref && typeof ref === 'object') id = ref.ddZ;
2416:     return b.gi(id) || null;
2417: }
```
- 引用可以是对象（取 **`ref.ddZ`** 当 id）或裸 id；**用 `b.gi(id)` 反查实体**。

```js
2420: function entities() {
2421:     var out = [], b = BATTLE || world(), mark = ++entMark;
2422:     function add(ent) {
2423:         if (!ent || !stat(ent)) return;
2427:         try {
2428:             if (ent.__wxEntMark === mark) return;
2429:             ent.__wxEntMark = mark;
2430:         } catch (e) {
2431:             if (out.indexOf(ent) >= 0) return;      // 对象被冻结时退回原来的写法
2432:         }
2433:         out.push(ent);
2434:     }
2436:     var refs = b && b.fF;
2437:     if (refs && typeof refs.length === 'number') {
2438:         for (var i = 0; i < refs.length; i++) add(entityFromRef(b, refs[i]));
2439:     }
2441:     add(hero());
2442:     return out;
2443: }
```
- **实体表 = `world().fF`**（形如数组，元素是引用/对象）；**英雄单独追加**（L2441），保证英雄一定在里面。
- 去重是**每次调用换一个自增 `mark`，写在实体的 `__wxEntMark` 上**（L2428–L2429），冻结对象时回落 `indexOf`。注释 L2424–L2426 说明动机：该方法被 `HitBus.install`（每秒一次）与全局加速扫描（每 700ms）反复调用，实体表最大时正是「进图第一下卡住」的主要开销，原写法是 O(n²)。

### 3.3 HP/MP 字段名（**写死在这里**）

| API | 行 | 字段 | 说明 |
|---|---|---|---|
| `hp(s)` | L2445–L2447 | `s.bhd` | 非 number 返回 `-1` |
| `maxHp(s)` | L2449–L2451 | `s.bhe` | 非 number 返回 `-1` |
| `setHp(s,value)` | L2453–L2461 | 写 `s.bhd` | 要求 `typeof s.bhd === 'number' && value >= 0`，成功返回 true |
| `mp(s)` | L2463–L2465 | `s.bhf` | 同上口径 |
| `maxMp(s)` | L2467–L2469 | `s.bhg` | 同上口径 |
| `setMp(s,value)` | L2471–L2479 | 写 `s.bhf` | 同上口径 |

导出（L2481–L2496）：`controller, world, bind, stat, time, hero, isHero, entities, hp, maxHp, setHp, mp, maxMp, setMp`。

---

## 4. 逐个功能

### 4.1 英雄攻击（改伤害数值，`CFG.value`）—— `DamageFeature` L2562–L2574

```js
2563: function beforeHit(ctx) {
2564:     if (!CFG.on || !(ctx.damage > 0)) return;
2565:     if (!BattleCore.isHero(ctx.attacker) || BattleCore.isHero(ctx.target)) return;
2566:     var wanted = Math.floor(Number(CFG.value));
2567:     if (!(wanted > 0) || !isFinite(wanted)) return;
2568:     ctx.damage = wanted;
2569:     ST.hits++;
2570:     DamageDisplayFeature.note(ctx.attacker, ctx.target, wanted);
2571: }
```
- 判据：`CFG.on` 开、本次 `ctx.damage > 0`、**攻击方是英雄且受击方不是英雄**。
- 值 = `Math.floor(Number(CFG.value))`，必须 `> 0` 且有限。**默认 99999**。
- 生效时机：**只在 `HitBus` 包住的 `hY` 调用链里**（不是自己的 hook），见 4.2。
- 每次改写计数 `ST.hits++`，并把「期望显示值」投递到 `DamageDisplayFeature`。

### 4.2 伤害总线 `HitBus`（L2576–L2648）—— 唯一分发器 + 过检测还原

**目标函数：`Object.getPrototypeOf(实体).hY(attacker, damage, hitInfo)`**（L2578 要求 `typeof proto.hY === 'function'`）。

```js
2577: function hookPrototype(proto) {
2578:     if (!proto || typeof proto.hY !== 'function') return false;
2579:     if (proto.hY.__wxHitBus) return true;              // 幂等
2581:     var nativeHit = proto.hY;
2582:     proto.hY = function (attacker, damage, hitInfo) {
2583:         var originalDamage = Number(damage) || 0;
2584:         var s = BattleCore.stat(this);
2585:         var ctx = { target: this, attacker: attacker, damage: originalDamage, hitInfo: hitInfo,
2586:                     targetStat: s, beforeHp: BattleCore.hp(s), protected: false };
2601:         var atkStat = null, vicStat = ctx.targetStat;
2602:         var atkBefore = null, vicBefore = null;
2604:             atkStat = BattleCore.stat(attacker);
2605:             if (atkStat && typeof atkStat.finalDamage === 'number') atkBefore = atkStat.finalDamage;
2606:             if (vicStat && typeof vicStat.finalHurtValue === 'number') vicBefore = vicStat.finalHurtValue;
2609:         try { DamageFeature.beforeHit(ctx); } catch (e) { featureError('damageBefore', e); }
2610:         try { GuardFeature.beforeHit(ctx); } catch (e2) { featureError('guardBefore', e2); }
2612:         var result = nativeHit.call(this, attacker, ctx.damage, hitInfo);   // ★ 用改写后的 ctx.damage 调原函数
2615:             if (ctx.damage !== originalDamage) {
2616:                 if (atkBefore !== null && ... atkStat.finalDamage !== atkBefore) atkStat.finalDamage = atkBefore + originalDamage;
2619:                 if (vicBefore !== null && ... vicStat.finalHurtValue !== vicBefore) vicStat.finalHurtValue = vicBefore + originalDamage;
2622:                 ST.acFixed = (ST.acFixed || 0) + 1;
2626:         try { GuardFeature.afterHit(ctx); } catch (e4) { featureError('guardAfter', e4); }
2627:         return result;
2628:     };
2629:     proto.hY.__wxHitBus = true;
2630:     proto.hY.__wxOrig = nativeHit;
2631:     ST.hitBus = true;
2632:     return true;
2633: }
```
- **关键手法（过检测）**：`hY` 内部会把本次实际伤害累加进攻击方 `hw.finalDamage`、受击者 `hw.finalHurtValue`（L2599）。所以**调用前记原值，调用后只把「原始伤害」补回去**；仅在数值确实被改动过时写回。注释（L2595–L2600）点名两个消费点：**`battleMgr.bET({damage: hw.finalDamage, takenDamage: hw.finalHurtValue, ...})` 上报** 与 **伤害统计面板 `o.cZq` / `c.cZr`**。
- 调用顺序固定：`DamageFeature.beforeHit` → `GuardFeature.beforeHit` → `nativeHit` → 字段还原 → `GuardFeature.afterHit`。

**装法：按实体原型装，不按类名装**（L2635–L2645）：
```js
2635: function install() {
2636:     var entities = BattleCore.entities(), protos = [];
2637:     for (var i = 0; i < entities.length; i++) {
2639:         try { proto = Object.getPrototypeOf(entities[i]); } catch (e) {}
2640:         if (!proto || protos.indexOf(proto) >= 0) continue;
2641:         protos.push(proto);
2642:         try { hookPrototype(proto); } catch (e2) { featureError('hitBusProto', e2); }
2644:     return protos.length > 0;
2645: }
```
- 每次 `install` 现场枚举 `entities()` 的**全部原型去重后逐个 hook** —— 绕开混淆类名，代价是每秒重跑（见 L3014 的 1000ms 节流 + L7163 注释「热更/换场景后模块会重建，每秒重装一次」）。
- 导出只有 `{ install: install }`（L2647）。

### 4.3 伤害数字显示改值 —— `DamageDisplayFeature`（L2651–L2716）

目标：**`ctrl.ne`（渲染器）原型上的 `jg(event)`**，因为 `battleMgr.bEu.ne.jg(event)` 直接读 `event.finalDamage`（L2650 注释）。

```js
2654: function note(attacker, target, wanted) { pending.push({ t: Date.now(), attacker, target, wanted });  if (pending.length > 48) pending.splice(0, pending.length - 48); }
2662: function take(event) {
2663:     if (!event || typeof event.finalDamage !== 'number') return null;
2665:     while (pending.length && now - pending[0].t > 1200) pending.shift();     // 1200ms 窗口
2667:     var source = event.atkEntity;  var target = event.target;
2669:     if (!source || !BattleCore.isHero(source) || !target || BattleCore.isHero(target)) return null;
2671:     for (var i = pending.length - 1; i >= 0; i--) { if (source === item.attacker && target === item.target) { pending.splice(i, 1); return item; } }
2678: }
2680: function install() {
2681:     var ctrl = BattleCore.controller(), renderer = ctrl && ctrl.ne;
2684:     owner = renderer && Object.getPrototypeOf(renderer);
2685:     if (!owner || typeof owner.jg !== 'function') owner = renderer;
2690:     owner.jg = function (event) {
2691:         var item = take(event);
2693:         if (item && event && typeof event.finalDamage === 'number') {
2694:             var old = event.finalDamage;
2695:             event.finalDamage = item.wanted;
2696:             ST.renderHits = ...; ST.renderFields = ...; ST.renderDamage = item.wanted;
2699:             try { return nativeJg.apply(this, arguments); }
2700:             finally { try { event.finalDamage = old; } catch (e2) {} }     // 用完即还原
2701:         }
2702:         return nativeJg.apply(this, arguments);
2703:     };
2704:     owner.jg.__wxDamageRender = true;  owner.jg.__wxOrig = nativeJg;
```
- 队列上限 48 条（L2659），过期窗口 1200ms（L2665），按 `attacker/target` 引用相等反查（L2673）。
- 事件字段：**`event.finalDamage` / `event.atkEntity` / `event.target`**。
- 幂等标记 `owner.jg.__wxDamageRender`（L2689 前置判、L2704 置位）；已装则 `ST.renderHooks = 1`。

### 4.4 英雄锁血 —— `GuardFeature`（L2499–L2560），**双保险**

**保险 A（拦截前）**：`beforeHit` L2503–L2513
```js
2504: if (!CFG.lockHp) return;
2505: if (!BattleCore.isHero(ctx.target) || !ctx.targetStat || !(ctx.beforeHp > 0)) return;
2506: ctx.protected = true;
2509: if (!s.__wxGuardQueued) s.__wxGuardOldMin = Number(s.lockMinHP) || 0;
2510: s.__wxGuardRestoreHp = Math.max(Number(s.__wxGuardRestoreHp) || 0, ctx.beforeHp);
2511: if (typeof s.lockMinHP === 'number') s.lockMinHP = Math.max(1, s.lockMinHP || 0);
```
- **利用游戏自身的保底字段 `stat.lockMinHP`**：命中前把它抬到 ≥1 并记下原值，让游戏自己就不把血打到 0。

**保险 B（命中后回写）**：`afterHit` L2537–L2554 + `restore` L2515–L2529 + `flush` L2531–L2535
```js
2541: if (!s.__wxGuardQueued) { s.__wxGuardQueued = true; pending.push({ stat: s, hp: ctx.beforeHp }); }
2547: if (typeof setTimeout === 'function') { if (!flushQueued) { flushQueued = true; setTimeout(function () { flushQueued = false; flush(); }, 0); } }
2519: var wanted = Number(s.__wxGuardRestoreHp) || item.hp || 1;
2520: BattleCore.setHp(s, wanted);
2521: if (typeof s.lockMinHP === 'number') s.lockMinHP = Number(s.__wxGuardOldMin) || 0;
2522: s.__wxGuardRestoreHp = 0;  s.__wxGuardQueued = false;  ST.noDmg++;
```
- 把血量**回写到命中前的值**（`ctx.beforeHp`），并把 `lockMinHP` 复原。
- 注释 L2545–L2546：一次命中风暴几百次回调，**用单个 `setTimeout(... ,0)` 合并 flush**（`flushQueued` 旗标），`pending` 在 `tick` 里也会被 flush，不会漏。
- `tick()`（L2556）= `flush(); BattleCore.hero();`（顺手保证 `__wxHero` 标记存在）。`reset()`（L2557）= `flush(); pending.length = 0;`。
- 导出 `{ beforeHit, afterHit, tick, reset }`（L2559）。**注意：`GuardFeature` 没有自己的 install/hook，它完全搭 `HitBus` 的 `hY` 便车。**

### 4.5 英雄锁血（轮询版）—— `LockHpFeature`（L2718–L2737）

```js
2721: function tick(now, force) {
2722:     if (!CFG.lockHp) return;
2723:     if (!force && now - last < 300) return;         // 300ms 节流
2725:         var hero = BattleCore.hero();  var s = BattleCore.stat(hero);  var max = BattleCore.maxHp(s);
2731:         if (max > 0 && BattleCore.hp(s) !== max && BattleCore.setHp(s, max)) ST.lock++;
```
- **直接把 `s.bhd` 写成 `s.bhe`（满血）**，300ms 一次。
- 注释 L2729–L2730：已满血就不再写 —— 每 300ms 无条件写血量会触发游戏自己的「血量变化」链路（血条/特效/状态重算），进图那一刻尤其明显。
- 与 4.4 并存：4.4 防「被打死」，4.5 保「一直满血」。

### 4.6 锁定蓝量 —— `LockManaFeature`（L2739–L2821），**唯一用 `Object.defineProperty` 改访问器的功能**

```js
2742: function install() {
2743:     if (!CFG.lockMp) return false;
2746:     if (!s || typeof s.bhf !== 'number' || typeof s.bhg !== 'number') return false;
2747:     if (s.__wxManaLock && typeof s.__wxManaLock === 'object') { ST.mpHook = true; return true; }
2749:     var desc = Object.getOwnPropertyDescriptor(s, 'bhf');
2750:     if (!desc || !Object.prototype.hasOwnProperty.call(desc, 'value') || desc.configurable === false || desc.writable === false) return false;
2751:     var state = { value: Number(desc.value), enumerable: desc.enumerable !== false, writable: desc.writable !== false };
2752:     Object.defineProperty(s, 'bhf', {
2753:         configurable: true, enumerable: state.enumerable,
2755:         get: function () { var max = Number(this.bhg); if (CFG.lockMp && max > 0 && state.value < max) state.value = max; return state.value; },
2760:         set: function (next) { var n = Number(next); if (!isFinite(n)) return; var max = Number(this.bhg);
2764:             if (CFG.lockMp && max > 0 && n < max) { state.value = max; ST.lockMp++; return; } state.value = n; }
2771:     });
2772:     Object.defineProperty(s, '__wxManaLock', { value: state, configurable: true, enumerable: false });
2775:     if (s.bhg > 0) s.bhf = s.bhg;
```
- **先在英雄 stat 实例上取 `bhf` 的数据描述符**，校验是 value 描述符且 `configurable !== false`、`writable !== false`，再换成 get/set。
- get 与 set **双向夹到 `max`（= `this.bhg`）**：get 时若低于上限就先补满再返回；set 时若写入值 < 上限就丢弃并记 `ST.lockMp++`。
- 状态对象存在 `s.__wxManaLock`（L2772），既做幂等标记也做卸载凭据。

```js
2783: function uninstall() {
2788:     Object.defineProperty(s, 'bhf', { value: Number(state.value) || 0, writable: state.writable !== false, configurable: true, enumerable: state.enumerable !== false });
2794:     try { delete s.__wxManaLock; } catch (e0) {}
2795:     ST.mpHook = false;
2797: }
2800: function setEnabled(on) { last = 0; if (!on) return uninstall(); if (!install()) return tick(Date.now(), true); return true; }
2807: function tick(now, force) {                       // 250ms 节流
2808:     if (!CFG.lockMp) return;
2809:     if (!force && now - last < 250) return;
2812:     install();
2815:     if (max > 0 && BattleCore.mp(s) !== max && BattleCore.setMp(s, max)) ST.lockMp++;
2819: function reset() { last = 0; ST.mpHook = false; }
```
- **`setEnabled(on)` 是这一层唯一的「卸」入口**（L2800）；菜单行 L1884 直接调它（见 §6）。
- `uninstall` 把访问器还原成数据属性，值取 `state.value`。
- `tick` 是访问器装不上的兜底路径（每 250ms 写满 `bhf`）。
- 导出（L2820）：`{ install, uninstall, setEnabled, tick, reset }`。

### 4.7 技能无CD —— `NoCdFeature`（L2823–L2846）

**目标：英雄 `stat` 原型的 `bil()`**（L2828 要求 `typeof proto.bil === 'function'`）。
```js
2831: var nativeCd = proto.bil;
2832: proto.bil = function () {
2835:     var cd = nativeCd.apply(this, arguments);      // ★ 先跑原函数
2836:     try { if (CFG.noCd && this && this.__wxHero) return 0; } catch (e) {}
2837:     return cd;
2838: };
2839: proto.bil.__wxNoCd = true;  proto.bil.__wxOrig = nativeCd;  ST.noCd = true;
```
- 注释 L2833–L2834（**复刻要点**）：**必须先把原函数跑了** —— 冷却不只是读一个数，游戏会顺带推进技能状态；跳过它（旧版写法）会让技能状态机停在半路。
- 只对 `this.__wxHero` 生效（依赖 L2388 `hero()` 打的标记）。
- **注意：注释 L2337 说「无 CD 只挂 `bil`」**，与实际一致。

### 4.8 人物移速 —— `effectiveMoveScale`（L2848–L2854）+ `MoveSpeedFeature`（L2856–L2885）

```js
2848: function effectiveMoveScale() {
2849:     var move = parseFloat(CFG.speed);
2850:     var global = parseFloat(CFG.gspeed);
2851:     if (!(move > 1)) return 1;
2852:     if (!(global > 1)) return move;
2853:     return Math.max(1, move / global);
2854: }
```
- **与全局加速做除算抵消**：全局已经乘过 `gspeed`，角色移速再乘就会双重加速，所以实际倍率 = `speed / gspeed`（下限 1）。

```js
2857: function install() {
2859:     var s = BattleCore.stat(hero);  var proto = s && Object.getPrototypeOf(s);
2861:     if (!proto || typeof proto.bij !== 'function') return false;
2862:     if (proto.bij.__wxMove) { ST.moveHook = true; ST.moveSource = 'bij'; return true; }
2868:     var nativeSpeed = proto.bij;
2869:     proto.bij = function (type) {
2870:         var result = nativeSpeed.apply(this, arguments);
2872:         var scale = effectiveMoveScale();
2873:         if (type === 2 && this && this.__wxHero && scale > 1 && result > 0) result *= scale;
2875:         return result;
2876:     };
2877:     proto.bij.__wxMove = true;  proto.bij.__wxOrig = nativeSpeed;
2879:     ST.moveHook = true;  ST.moveSource = 'bij';
```
- **目标：英雄 `stat` 原型 `bij(type)`，只在 `type === 2` 时放大**（type=2 即移动速度这一路），且要求 `this.__wxHero`、`scale > 1`、`result > 0`。
- 注释 L2337：「移速只挂 **`bij(2)`**」。
- 菜单回调 `hookBattleSpeed()`（L3041–L3049）：
```js
3043: BattleCore.bind();
3044: window.__WX_MOVE_V__ = parseFloat(CFG.speed) || 1;
3045: window.__WX_MOVE_EFFECTIVE__ = effectiveMoveScale();
3046: if (!(parseFloat(CFG.speed) > 1)) return true;
3047: return MoveSpeedFeature.install();
```

### 4.9 图内全局加速 —— `GlobalSpeedFeature`（L2887–L2944），**改 `hz.mainTimeScale` 访问器**

```js
2893: function scaleNow() {
2894:     var scale = parseFloat(CFG.gspeed);
2895:     if (!(scale > 1)) return 1;
2896:     if (Date.now() - (ST.lastBattleAt || 0) < 3000) return 1;      // ★ 换战斗后头 3 秒不加速
2897:     return scale;
2898: }
2900: function hookTime(t) {
2901:     if (!t || typeof t.mainTimeScale !== 'number') return false;
2902:     if (t.__wxGlobalTime) return true;
2904:     var base = t.mainTimeScale;
2905:     Object.defineProperty(t, 'mainTimeScale', {
2906:         configurable: true, enumerable: true,
2908:         get: function () {
2909:             var s = scaleNow();
2913:             return (s > 1 && base >= 1) ? base * s : base;
2914:         },
2915:         set: function (value) {
2918:             base = (typeof value === 'number' && isFinite(value) && value >= 0) ? Math.min(value, 1) : 1;
2919:         }
2921:     });
2922:     Object.defineProperty(t, '__wxGlobalTime', { value: true, configurable: true, enumerable: false });
```
- **目标：每个实体的 `hz`（= `BattleCore.time(ent)`）上的 `mainTimeScale`**。注释 L2337：「全局加速只挂 `hz.mainTimeScale`」。
- **get 只在 `s > 1 && base >= 1` 时乘**（注释 L2910–L2912）：`base < 1` 说明游戏正在入场动画/顿帧/慢动作/暂停里自己推进状态，**原样返回、绝不参与乘法**，否则状态机停在半路 = 卡死。
- **set 把 `base` 上限压到 1**（注释 L2916–L2917）：游戏自己写法是「读出来改一改再写回」（`t.x = t.x * k`、`t.x -= dt`），不压住会把倍速一次次乘进去跑飞。
- 3 秒窗口（L2896）由 `onBattleChanged` 写 `ST.lastBattleAt = Date.now()`（L3004）供数；注释 L2891–L2892：换战斗后头 3 秒不加速，因为入场那段是游戏自己在推进状态机（进图卡死、点自动战斗卡死都出在这里）。

```js
2930: function tick(now, force) {
2931:     if (!(parseFloat(CFG.gspeed) > 1)) { ST.gsEnts = 0; return; }
2932:     if (!force && now - lastScan < 700) return;        // 700ms 扫描一次
2934:     var list = BattleCore.entities(), count = 0;
2935:     for (var i = 0; i < list.length; i++) { var t = BattleCore.time(list[i]); if (t && hookTime(t)) count++; }
2939:     ST.gsEnts = count;
2942: function reset() { lastScan = 0; }
```
- **必须轮询扫描**：新实体不断生成，靠 700ms 遍历全实体表给每个 `hz` 装访问器；`ST.gsEnts` 记录本帧装上的数量（诊断用）。
- 菜单回调 `applyGlobalSpeed()`（L3050–L3053）：`BattleCore.bind(); GlobalSpeedFeature.tick(Date.now(), true);` —— **force=true 立刻生效**。菜单滑块上限 `10.0`（L1892）。

### 4.10 解锁金卡与自动战斗 —— `unlockAuto`（L3054–L3062）+ `AutoFightFeature`（L2946–L2973）+ `GoldCardFeature`（L2975–L2997）

**数据源（两者相同）**：
```js
2948: var data = window.$eZ && window.$eZ.monCard && window.$eZ.monCard.fDn;
2949: var proto = data && Object.getPrototypeOf(data);
```
→ **`$eZ.monCard.fDn` 的原型**。

**自动战斗 = `fDj()`（权限判定）**：
```js
2952: var nativePermission = proto.fDj;
2953: proto.fDj = function () {
2954:     var allowed = nativePermission.apply(this, arguments);
2955:     try { if (!allowed && CFG.autoFight) return true; } catch (e) {}
2956:     return allowed;
2957: };
2958: proto.fDj.__wxAutoFight = true;  proto.fDj.__wxOrig = nativePermission;
```
- 只把 `false` 翻成 `true`，原本为真则不动。注释 L2338：「自动战斗只挂 **`fDn.fDj`**」。

**金卡 = `fDc(cardType)`（持有权限）**：
```js
2983: proto.fDc = function (cardType) {
2984:     var owned = nativeOwned.apply(this, arguments);
2985:     try { if (cardType === 'gold' && CFG.autoFight) return true; } catch (e) {}
2986:     return owned;
2987: };
2988: proto.fDc.__wxGoldCard = true;  proto.fDc.__wxOrig = nativeOwned;  ST.gold = true;
```
- **字面量 `'gold'` 是唯一入参判据**。注释 L2338：「金卡持有权限只挂 **`fDn.fDc("gold")`**」。

**菜单入口 `unlockAuto()`（L3054–L3062）**：形参表为空（**声明的形参是空，但调用点全部传 `true`**，见 L1886 / L1521 / L7157 —— 多余的实参被忽略，是无害的遗留）：
```js
3054: function unlockAuto() {
3056:     if (!CFG.autoFight) return true;
3057:     var autoOk = AutoFightFeature.install();
3058:     var goldOk = GoldCardFeature.install();
3059:     return autoOk && goldOk;
3061: }
```

---

## 5. 通用注入工具写法

### 5.1 本段唯一的 `hookPrototype`（L2577，作用域在 `HitBus` 内）

模式固定四件套：
1. **前置类型检查**：`if (!proto || typeof proto.hY !== 'function') return false;`（L2578）
2. **幂等标记**：`if (proto.hY.__wxHitBus) return true;`（L2579）—— 标记写在**被包装后的函数对象**上，所以重复 install 不会叠层。
3. **取原函数 + 包装**：`var nativeHit = proto.hY; proto.hY = function(...){ ... nativeHit.call(this, ...) ... };`（L2581–L2628）
4. **留痕**：`proto.hY.__wxHitBus = true; proto.hY.__wxOrig = nativeHit;`（L2629–L2630）

**全段所有 hook 的幂等标记与原名备份（统一命名法 `__wxXxx` / `__wxOrig`）**：

| 目标 | 标记（幂等判据） | 原名备份 | 行 |
|---|---|---|---|
| `proto.hY` | `__wxHitBus` | `__wxOrig` | L2579 / L2629–L2630 |
| `owner.jg` | `__wxDamageRender` | `__wxOrig` | L2689 / L2704–L2705 |
| `proto.bil` | `__wxNoCd` | `__wxOrig` | L2829 / L2839–L2840 |
| `proto.bij` | `__wxMove` | `__wxOrig` | L2862 / L2877–L2878 |
| `proto.fDj` | `__wxAutoFight` | `__wxOrig` | L2951 / L2958–L2959 |
| `proto.fDc` | `__wxGoldCard` | `__wxOrig` | L2981 / L2988–L2989 |
| `t.mainTimeScale`（访问器） | `__wxGlobalTime` | — | L2902 / L2922 |
| 英雄实体/stat | `__wxHero` | — | L2393–L2394 |
| 英雄 stat 蓝量状态 | `__wxManaLock`（存 state 对象） | — | L2747 / L2772 |
| 命中便车队列 | `__wxGuardQueued` | `__wxGuardOldMin` / `__wxGuardRestoreHp` | L2509–L2510 / L2541 |
| 实体去重 | `__wxEntMark` | — | L2428–L2429 |

- **恢复手段**：本段**只有 `LockManaFeature.uninstall()`（L2783–L2798）实现了真正的卸载**（把访问器还原成数据属性、删 `__wxManaLock`）。`__wxOrig` 只是留痕，全段没有任何地方用 `__wxOrig` 回滚函数包装 —— 复刻时若要「热卸」，得自己写。
- **另有一个通用的节流包装器**（本段之外，L625 附近）：`wrapped.__wxThrottled = true; wrapped.__wxOrig = orig;` —— 同一套命名法。

### 5.2 `install(force)` 的节流与恢复点

```js
3012: function install(force) {
3013:     var now = Date.now();
3014:     if (!force && now - lastInstall < 1000) return;     // ★ 1 秒节流，force 时跳过
3015:     lastInstall = now;
3016:     BattleCore.hero();
3017:     if (CFG.on || CFG.lockHp) HitBus.install();
3018:     if (CFG.on) DamageDisplayFeature.install();
3019:     if (CFG.lockMp) LockManaFeature.install();
3020:     if (CFG.noCd) NoCdFeature.install();
3021:     if (parseFloat(CFG.speed) > 1) MoveSpeedFeature.install();
3022:     if (CFG.autoFight) { AutoFightFeature.install(); GoldCardFeature.install(); }
3023: }
```
- **`HitBus.install()` 的判据是 `CFG.on || CFG.lockHp`** —— 锁血单独开也要挂伤害总线（因为锁血搭 `hY` 便车）。
- 全部 install 都是**幂等**的（各自标记），所以每秒重跑不叠层；这也解释了 L7163 注释「热更/换场景后模块会重建，每秒重装一次」。
- 需要「立刻生效」的菜单回调一律传 `true`：L1881/1882/1883/1884/1885（`FeatureManager.install(true)`）、L7178（启动）。

### 5.3 `FeatureManager` 其余两个入口

```js
3002: function onBattleChanged() {
3003:     lastInstall = 0;
3004:     ST.lastBattleAt = Date.now();      // 进图窗口计时（全局加速前 3 秒不加速）
3005:     GuardFeature.reset();
3006:     LockHpFeature.reset();
3007:     LockManaFeature.reset();
3008:     GlobalSpeedFeature.reset();
3009:     DamageDisplayFeature.reset();
3010: }
3028: function tick(now) {
3029:     BattleCore.bind();
3030:     install(false);
3031:     GuardFeature.tick();
3032:     LockHpFeature.tick(now, false);
3033:     LockManaFeature.tick(now, false);
3034:     GlobalSpeedFeature.tick(now, false);
3035: }
3037: return { onBattleChanged: onBattleChanged, install: install, tick: tick };
```
- **按需装/卸的机制 = 「每 tick 用 CFG 重算一次装什么」+「各 hook 自身幂等」**，不是显式 uninstall；唯一例外是 `LockManaFeature.setEnabled(false)`。
- 三种手段分布明确：**伤害/锁血 = 一次性 hook `hY`（事件驱动）；锁血满血/蓝量/加速 = 轮询 tick（300ms / 250ms / 700ms）；移速/无CD/自动战斗/金卡 = 一次性 hook 原型方法**。

### 5.4 谁在驱动 `tick`（L7143–L7186）

```js
7143: window.__WX_TICK__ = function () {
7144:     var now = Date.now();
7147:     try { FeatureManager.tick(now); } catch (e3) { featureError('featureTick', e3); }
7160: }
7170: window.__DMG_TICK__ = window.__WX_TICK__;      // 兼容旧调用名
7173: function startFeatures() {
7177:         tries++;
7178:         try { BattleCore.bind(); FeatureManager.install(true); } catch (e) { featureError('startFeatures', e); }
7180:         var done = false; try { done = buildMenu(); } catch (e) {}
7181:         if ((done || tries > 600) && t !== null) { try { clearInterval(t); } catch (e) {} }
7185:     try { if (typeof setInterval === 'function') t = setInterval(attempt, 300); } catch (e) {}
```
- 启动重试：**300ms 一次 `setInterval`，最多 600 次**（≈3 分钟）直到 `buildMenu()` 成功；每次重试都 `BattleCore.bind(); FeatureManager.install(true);`。
- 菜单回调（L1881–L1893）是**改 CFG → `save()` → 立刻 `install(true)` / 模块函数**，不重启不重登。

---

## 6. 菜单行 ←→ 功能的对应（L1879–L1893）

```js
1881: function (v) { CFG.value = v; CFG.on = true; save(); try { FeatureManager.install(true); } catch (e) {} },     // 英雄攻击（输入框，勾选回调见下行）
1882: function (on) { CFG.on = on; save(); try { FeatureManager.install(true); } catch (e) {} },                      // 英雄攻击 开关
1883: '英雄锁血' → CFG.lockHp = on; save(); FeatureManager.install(true);
1884: '锁定蓝量' → CFG.lockMp = on; save(); LockManaFeature.setEnabled(on); FeatureManager.install(true);
1885: '技能无CD' → CFG.noCd = on; save(); FeatureManager.install(true);
1886: '解锁金卡与自动战斗' → CFG.autoFight = on; save(); unlockAuto(true);
1887: '人物移速' 滑块 → CFG.speed = v; save(); hookBattleSpeed();
1890: '图内全局加速' 滑块（上限 10.0） → CFG.gspeed = v; save(); applyGlobalSpeed();
```
- **只有「锁定蓝量」调了模块级 `setEnabled(on)`**（因为它有真 uninstall）；其余靠 `install(true)` 的 CFG 重算。
- 另一处会写 `CFG.autoFight = true; save(); unlockAuto(true);` 的是 L1521（自动引擎开关）与 L7157（引擎 state.enabled 时联动）。

---

## 7. 命令总线 `W.__REMOTE_COMMANDS__`（L3064–L3101）

### 7.1 容器与前置（L3066–L3083）

```js
3066: (function (W) {
3067:     if (!W || W.__WX_AUTO_ENGINE__) return;          // 幂等：整个自动引擎只装一次
3068:     var REMOTE_BUILD = 'res-1.1.3.2';
3069:     function lsGet(k, d) { return storageGet(k, d); }
3070:     function lsSet(k, v) { return storageSet(k, v); }
3071:     function wxFlag(k) { return W.__WX_CARD_PERMANENT__ === true && String(lsGet(k, '0')) === '1'; }
3072:     function log(text) { try { console.log('[AUTO] ' + String(text == null ? '' : text)); } catch (e) {} }
3073:     function nativeCall() { return null; }
3074:     W.RemoteUI = W.RemoteUI || {};
3075:     W.RemoteUI.notify = W.RemoteUI.notify || function () {};
3076:     W.RemoteUI.mergeState = W.RemoteUI.mergeState || function (patch) { try { W.__WX_AUTO_STATE__ = patch || {}; } catch (e) {} };
3077:     W.RemoteUI.updateState = W.RemoteUI.updateState || W.RemoteUI.mergeState;
3078:     function state(patch) { var out = W.__WX_AUTO_STATE__ || (W.__WX_AUTO_STATE__ = {}), p = patch || {}; for (var k in p) ... out[k] = p[k]; }
3084:     W.__REMOTE_COMMANDS__ = W.__REMOTE_COMMANDS__ || {};
```
- **总线 = 挂在 `window` 上的一个普通对象 `window.__REMOTE_COMMANDS__`，键是命令名字符串，值是「函数(payload) → 返回状态对象」**。
- **`wxFlag(k)` 是权限闸门**：必须 `window.__WX_CARD_PERMANENT__ === true`（永久卡）**且** localStorage 里该键为 `'1'`。
- 每个命令返回的对象里固定带 **`lastAction`** 字段（`'jobs'`/`'jobSelect'`/`'quality'`/`'cleanNow'`/`'cfgDiag'` 等），是给远端 UI 显示「刚才干了什么」的回执。
- `W.RemoteUI.mergeState/updateState` + `W.__WX_AUTO_STATE__` 是给外部（远程面板）同步状态用的单向写入口。

### 7.2 两个基础命令（本段）

```js
3085: W.__REMOTE_COMMANDS__['wx.flag'] = function (payload) {
3086:     var key = payload && payload.key;
3087:     if (!key) return { lastAction: 'no-key' };
3088:     var enabled = !!(payload && payload.value);
3089:     if (enabled && W.__WX_CARD_PERMANENT__ !== true) {
3090:         lsSet(key, '0'); W[key] = false;                                       // ★ 非永久卡：强制关闭并写 0
3091:         return { lastAction: 'permanent-required', value: false };
3092:     }
3093:     lsSet(key, enabled ? '1' : '0'); W[key] = enabled;                         // ★ 双写：localStorage + window
3094:     return { lastAction: key + '=' + enabled, value: enabled };
3095: };
3096: W.__REMOTE_COMMANDS__['wx.num'] = function (payload) {
3097:     var key = payload && payload.key, value = payload && payload.value;
3098:     if (!key) return { lastAction: 'no-key' };
3099:     lsSet(key, String(value)); W[key] = parseFloat(value) || 0;
3100:     return { lastAction: key + '=' + W[key] };
3101: };
```
- **`wx.flag`**：布尔开关总入口。`payload = {key, value}`；**开启方向被 `__WX_CARD_PERMANENT__` 卡死**（非永久卡直接写 `'0'` 并回 `permanent-required`），**关闭方向永远允许**。
- **`wx.num`**：数值设置总入口。**没有权限检查**（数值项不走卡密闸门），`payload = {key, value}`，`lsSet(key, String(value))` 同时 `W[key] = parseFloat(value) || 0`。
- 同一 `key` 既进 localStorage 又挂到 `window` 上 —— 所以游戏侧/其它模块可以直接读 `window.__WX_AUTO_SELL__` 这类全局量。
- 后续命令（**属下一段，不在本范围**，此处仅登记位置以免遗漏）：`wx.jobs` L3159、`wx.jobSelect` L3194、`wx.jobInput` L3209、`wx.quality` L3221、`wx.cleanNow` L3250、`wx.cfgDiag` L3269；同段还有 `wxFlag` 消费的 CFG（L3127–L3139：`__WX_AUTO_FLIP__`/`__WX_AUTO_REPEAT__`/`__WX_AUTO_REVIVE__`/`__WX_AUTO_DECOMPOSE__`/`__WX_AUTO_SELL__`/`__WX_AUTO_WEAR__`/`__WX_AUTO_SKILL__`/`__WX_AUTO_JOBSWITCH__`/`__WX_AUTO_JOB__`、`__WX_SELL_Q__`/`__WX_DECOMP_Q__`（默认 `'1,2,3'`）、`__WX_JOB_SELECT__`）与 `get`（L3102 `__require(m)`）/`F()`（L3105 `globalThis.$eZ`）两个新助手。

### 7.3 命令总线与 §4 战斗层的耦合点

- 总线的 `wx.flag` 只做「写开关」，**它不直接调 `FeatureManager`**；真正的联动在 `__WX_TICK__`（L7149–L7161）：
  - 授权不通过且非永久卡 → `AutoBridge.disableAll()`（一次性，旗标 `__WX_AUTO_GATE_CLEARED__`）。
  - 通过且 `window.__WX_AUTO_ENGINE_TICK__` 存在 → 若 `autoEngine.state.enabled && !CFG.autoFight`，则 **`CFG.autoFight = true; save(); unlockAuto(true);`**（L7156–7157）—— 自动引擎一开，战斗层的「金卡+自动战斗」跟着开。

---

## 8. 一句话复刻清单（照着写）

1. 拿战斗数据只有一条路：`window.$eZ.battleMgr.bEu` → **controller**；`ctrl.nM()` → **world**；`ctrl.ny()` → **hero**；`world.fF` → **实体引用表**；`b.gi(id)` 反查；实体属性 **`hw`（数值）/ `hz`（时间）**。
2. 改伤害 = 包 `Object.getPrototypeOf(实体).hY(attacker, damage, hitInfo)`，`nativeHit.call(this, attacker, ctx.damage, hitInfo)`；改完必须把 `atkStat.finalDamage` / `vicStat.finalHurtValue` 按原始伤害补回。
3. 锁血双保险 = 抬高 `stat.lockMinHP` + 命中后 `stat.bhd` 回写 `beforeHp`；另有 300ms 轮询写满 `bhd = bhe`。
4. 锁蓝 = 在英雄 stat 实例上把 `bhf` 从数据属性换成 get/set 访问器，双向夹到 `bhg`；`uninstall` 还原数据属性。
5. 无CD = 包 `stat` 原型 `bil()`，**先跑原函数再返回 0**，只对 `this.__wxHero`。
6. 移速 = 包 `stat` 原型 `bij(type)`，**只 `type === 2`** 时乘 `speed / gspeed`。
7. 全局加速 = 给每个实体的 `hz.mainTimeScale` 装访问器：**get 只在 `base >= 1` 时乘**，**set 把 base 压到 ≤1**；换战斗后 3 秒不加速。
8. 自动战斗/金卡 = `window.$eZ.monCard.fDn` 原型的 `fDj()` 返回 true、`fDc('gold')` 返回 true。
9. 装法统一「按原型装 + 幂等标记 `__wx*` + 备份 `__wxOrig`」，`install(force)` 1 秒节流，CFG 一变就 `install(true)`。
