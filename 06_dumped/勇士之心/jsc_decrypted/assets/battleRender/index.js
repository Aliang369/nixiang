window.__require = function t(e, i, o) {
function n(s, a) {
if (!i[s]) {
if (!e[s]) {
var c = s.split("/");
c = c[c.length - 1];
if (!e[c]) {
var l = "function" == typeof __require && __require;
if (!a && l) return l(c, !0);
if (r) return r(c, !0);
throw new Error("Cannot find module '" + s + "'");
}
s = c;
}
var h = i[s] = {
exports: {}
};
e[s][0].call(h.exports, function(t) {
return n(e[s][1][t] || t);
}, h, h.exports, t, e, i, o);
}
return i[s].exports;
}
for (var r = "function" == typeof __require && __require, s = 0; s < o.length; s++) n(o[s]);
return n;
}({
$DQK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c6ff4Z1W/hILpveZlsAAEK0", "$DQK");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jX = i.LevelShakePoints = void 0;
var r = t("../../battleLogic/component/$DPL"), s = t("../../battleLogic/component/$DPN"), a = t("../../../game/src/battle/types/$DYS"), c = t("../$KQ"), l = t("../../../game/src/battle/$DSO"), h = cc.v2(), u = 1, b = (cc.v2(), 
cc.v2(), cc.v2(), cc.v2()), p = cc.v2();
i.LevelShakePoints = new Object(null);
i.LevelShakePoints[1] = [ [ -2, -3 ], [ 7, 4 ], [ -4, -5 ], [ 5, 0 ], [ -3, 5 ], [ 0, 1 ], [ 5, 0 ], [ 3, -2 ], [ -1, 0 ], [ 0, 0 ] ];
i.LevelShakePoints[2] = [ [ 12, 4 ], [ 30, -9 ], [ 5, 0 ], [ -15, 5 ], [ 0, 1 ], [ 10, 0 ], [ 3, -2 ], [ -1, 0 ], [ 0, 0 ] ];
i.LevelShakePoints[3] = [ [ 10, 30 ], [ -25, 60 ], [ 5, -2 ], [ 20, -30 ], [ -8, 40 ], [ 6, -10 ], [ 4, 7 ], [ 0, -3 ], [ -1.5, 4.5 ], [ -1, 0 ], [ -.5, 1 ], [ 0, 0 ] ];
i.LevelShakePoints[4] = [ [ 60, 20 ], [ 120, -50 ], [ -4, 10 ], [ -60, 40 ], [ 80, -16 ], [ -20, 12 ], [ 60, 20 ], [ 120, -50 ], [ -4, 10 ], [ -60, 40 ], [ 80, -16 ], [ -20, 12 ], [ 7, 4 ], [ -3, 0 ], [ 4.5, -1.5 ], [ 0, -1 ], [ 1, -.5 ], [ 0, 0 ] ];
var f = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bnB = cc.v2();
e.bnC = .2;
e.bnD = -1;
e.bnE = 0;
e.bnF = -1;
e.bnG = cc.v2();
e.bnH = cc.v2();
return e;
}
e.prototype.lY = function() {
this.lX.ni.byU(a.$uo.$vk.P, this.bnI, this);
this.lX.ni.byU(a.$uo.$vk.Q, this.bnJ, this);
this.lX.ni.byU(a.$uo.$vk.S, this.bnK, this);
this.lX.ni.byU(a.$uo.$vk.R, this.bnL, this);
this.lX.ni.byU(a.$uo.$vk.i, this.bnM, this);
this.lX.ni.byU(a.$uo.$vk.n, this.bnN, this);
};
e.prototype.lZ = function() {
var t, e;
null === (t = this.lX) || void 0 === t || t.ni.byW(this);
null === (e = this.lX) || void 0 === e || e.nj.byW(this);
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.prototype.md = function() {};
e.prototype.ma = function(t) {
var e, o, n, c, f, d = this.lX.ne.gt(r.default), v = this.lX.ne.gt(s.default).worldTimeScale;
this.lW.is.forEach(function(e) {
for (var i = e.GW, o = i.length - 1; o > -1; o--) if (0 !== (r = i[o]).d) {
r.c += t * v;
r.c > r.d && i.splice(o, 1);
}
var n = e.GV;
for (o = n.length - 1; o > -1; o--) {
var r;
if (0 !== (r = n[o]).e) {
r.d += t * v;
r.d > r.e && n.splice(o, 1);
}
}
});
var y = this.lW.is.get(this.lW.iu);
if (y) {
var g = y.GW, m = g[g.length - 1];
y.GZ = m ? m.e : 0;
var _ = this.bnB;
if (-1 !== y.GY) {
var $ = i.LevelShakePoints[y.GX] && i.LevelShakePoints[y.GX][y.GY];
y.GY += 1;
if ($) {
_.x = $[0];
_.y = $[1];
} else {
_.x = 0;
_.y = 0;
y.GY = -1;
}
} else if (y.GZ > 0) {
_.x = Math.random() * y.GZ;
_.y = Math.random() * y.GZ;
} else {
_.x = 0;
_.y = 0;
}
var w = y.GV, D = w[w.length - 1];
if (D) {
this.bnE = D.g;
this.bnD = D.h;
this.bnC = D.i;
if (D.f === a.$uo.$uH.d) {
u = D.h;
h.x = d.bkh + (d.bki - d.bkh) / 2;
h.y = d.bkj + (d.bkk - d.bkj) / 2;
} else {
var P = this.lX.ne.gi(this.bnE);
if (!P) return;
var R = this.lW.iM(P);
if (!R) return;
if (-1 === this.bnF) {
h.x = R.Hu + R.Hx * (null !== (e = D.k) && void 0 !== e ? e : 0);
h.y = l.default.bEC.bFu(R.Hv, 0) + (null !== (o = D.l) && void 0 !== o ? o : 0);
if (B = null === (n = R.Hq) || void 0 === n ? void 0 : n.camera) {
h.x += null !== (c = B[0]) && void 0 !== c ? c : 0;
h.y += null !== (f = B[1]) && void 0 !== f ? f : 0;
}
u = 1.5;
-1 !== this.bnD && (u = this.bnD);
u = Math.max(u + .1, 0);
this.lW.it.div(u, b);
if (!D.j) {
var S = h.x - .5 * b.x, E = h.x + .5 * b.x, C = h.y - .5 * b.y, O = h.y + .5 * b.y, T = d.bkh - S, I = E - d.bki, M = d.bkj - C, x = O - d.bkk;
if (b.x > d.bki - d.bkh) h.x = .5 * (d.bki - d.bkh) + d.bkh; else {
T > 0 && (h.x += T);
I > 0 && (h.x -= I);
}
if (b.y > d.bkk - d.bkj) h.y = .5 * (d.bkk - d.bkj) + d.bkj; else {
M > 0 && (h.y += M);
x > 0 && (h.y -= x);
}
}
} else {
u = this.bnF;
h.set(this.bnG);
}
}
var k = this.lW._in;
if (k && k.mainCamera) {
if (0 === t) {
p.x = h.x;
p.y = h.y;
} else this.bnH.lerp(h, this.bnC, p);
this.bnH.set(p);
var H = k.mainCamera.zoomRatio, N = H + (u - H) * this.bnC;
p.addSelf(this.bnB);
k.mainCamera.node.setPosition(p);
k.mainCamera.zoomRatio = N;
for (var j = p, F = N, L = 0, A = this.lW.iz; L < A.length; L++) {
var B, X = (B = A[L]).camera;
if (X && X.isValid && X !== k.mainCamera) {
X.node.setPosition(j.x * B.motion, 1 * j.y);
X.zoomRatio = F;
}
}
}
}
}
};
e.prototype.bnI = function(t, e) {
var i = this;
this.lW.iJ();
this.lW.is.forEach(function(t) {
var o = t.GV.find(function(t) {
return 0 === t.c;
});
if (o) {
if (o.f === a.$uo.$uH.d) return;
o.b = i.lX.ne.fi;
o.f = e.type;
o.g = t.GS;
o.h = e.fixedZoomRatio;
o.i = e.cameraLerpRatio;
o.j = e.ignoreCameraCorrection || !1;
o.k = e.offsetX || 0;
o.l = e.offsetY || 0;
} else {
var n = {
b: i.lX.ne.fi,
c: 0,
d: 0,
e: 0,
f: e.type,
g: t.GS,
h: e.fixedZoomRatio,
i: e.cameraLerpRatio,
j: e.ignoreCameraCorrection || !1,
k: e.offsetX || 0,
l: e.offsetY || 0
};
t.GV = [ n ];
}
});
};
e.prototype.bnJ = function(t, e) {
var i = this, o = e[3] || 2;
if (!this.lX.ne.fM.i) switch (o) {
case 1:
this.lW.is.forEach(function(o) {
if (o.GS === t.hg) {
var n = {
b: i.lX.ne.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uo.$uH.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
o.GV.push(n);
}
});
break;

case 2:
this.lW.is.forEach(function(o) {
var n = {
b: i.lX.ne.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uo.$uH.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
o.GV.push(n);
});
break;

case 3:
t.hE.bhI.forEach(function(o) {
i.lW.is.forEach(function(n) {
if (n.GS === o.eid) {
var r = {
b: i.lX.ne.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uo.$uH.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
n.GV.push(r);
}
});
});
}
};
e.prototype.bnK = function(t) {
var e = t.hg;
this.lW.is.forEach(function(t) {
for (var i = t.GV, o = i.length - 1; o > -1; o--) i[o].c === e && i.splice(o, 1);
});
};
e.prototype.bnL = function(t, e) {
var i = this.lW.is.get(this.lW.iu);
i && (2 === e.length ? i.GW.push({
b: t.hg,
e: e[1] || 6,
d: e[0],
c: 0
}) : i.Ha(e[0]));
};
e.prototype.bnM = function() {
this.ma(0);
};
e.prototype.bnN = function() {
this.ma(0);
};
return e;
}(c.$gt);
i.$jX = f;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../battleLogic/component/$DPL": void 0,
"../../battleLogic/component/$DPN": void 0
} ],
$DQL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "641a0kUC3hCYrkjWq5dIqK/", "$DQL");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$ka = void 0;
var r = t("../../../bingo/src/$KG"), s = t("../../../game/src/battle/$DSC"), a = t("../../../game/src/battle/types/$DYO"), c = t("../../../game/src/battle/types/$DYQ"), l = t("../../../game/src/battle/types/$DYR"), h = t("../../../game/src/battle/types/$DYS"), u = t("../../../game/src/battle/$DSP"), b = t("../../../main/src/$KV"), p = t("../../battleLogic/$KL"), f = t("../$KQ"), d = cc.v2(), v = cc.v2(), y = function(t) {
t.cgSid = 0;
}, g = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bnO = new Set();
e.bnP = 0;
e.bnQ = -1;
e.bnR = 0;
e.bnS = -1;
e.bnT = 0;
e.bnU = 0;
e.bnV = "";
e.bnW = [];
e.bnX = [];
return e;
}
e.prototype.lY = function() {
this.bnY();
this.lX.ni.byU(h.$uo.$vk.o, this.bnZ, this);
this.lX.ni.byU(h.$uo.$vk.n, this.boa, this);
this.lX.ni.byU(h.$uo.$vk.t, this.boc, this);
this.lX.ni.byU(h.$uo.$vk.bc, this.bod, this);
this.lX.ni.byU(h.$uo.$vk.B, this.boe, this);
this.lX.ni.byU(h.$uo.$vk.l, this.bob, this);
this.lX.ni.byU(h.$uo.$vk.bl, this.bom, this);
this.lX.ni.byU(h.$uo.$vk.bt, this.bov, this);
this.lX.ni.byU(h.$uo.$vk.bA, this.bow, this);
};
e.prototype.lZ = function() {
var t;
null === (t = this.lX) || void 0 === t || t.ni.byW(this);
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.prototype.ma = function(t) {
this.bog(t);
this.lX.ne.gN ? this.boh(t) : this.boi(t);
};
e.prototype.md = function() {
this.boo();
};
e.prototype.bnY = function() {
var t, e, i, o, n = this.me, s = this.mf;
if (null == s ? void 0 : s.isValid) {
this.bnS = -1;
this.lX.nk && s.bxA();
if (n.gN) {
s.bwF();
s.minmapNode.parent.active = !1;
s.nodeTime.active = !0;
s.nodeLeftTop.active = !1;
s.nodePvpTop.active = !0;
s.node_autoTxt.active = !1;
this.box();
if (n.fM.f === c.$uj.g) {
s.nodePvpTop.getChildByName("btnPause").active = !1;
s.bxO();
}
} else {
s.bwE();
this.mf.minmapNode.parent.active = !0;
this.mf.nodeTime.active = n.fn > 0;
this.mf.nodeLeftTop.active = !0;
this.mf.nodePvpTop.active = !1;
s.node_autoTxt.active = !1;
var a = this.lX.nv();
a && (null === (t = s.bwq) || void 0 === t || t.byA(a.hv.beu, a.hv.bev));
s.btnPause.active = !n.gC();
s.btnAutoBattle.active = n.fM.h && !n.gC();
if (n.fM.h) {
s.bxD();
this.bon();
s.bxF();
}
this.mf.bxL();
if (n.fK.type === r.$fe.tblEnum.InstanceTypeTableEnum.WORLD_BOSS_TEAM) {
this.mf.nodeTime.active = !1;
s.bxM();
}
if (n.fM.f === c.$uj.f) {
s.bxN();
s.minmapNode.parent.active = !1;
this.mf.nodeLeftTop.active = !1;
}
var l = null === (o = null === (i = null === (e = this.lX.mw) || void 0 === e ? void 0 : e.bAt) || void 0 === i ? void 0 : i.params) || void 0 === o ? void 0 : o.playMode;
s.nodePractice.active = u.$lN.bGN(l);
s.abyssInfoUI.node.active = u.$lN.bGO(l);
}
s.bxs(!1);
if (n.fn) {
s.battleTime.node.active = !0;
this.bnV = "";
s.battleTime.string = "";
} else {
this.bnV = " --";
s.battleTime.string = " --";
}
s.bxt(this.lX.mx);
this.lX.nn() && s.bwG();
this.bop();
this.boo();
}
};
e.prototype.bnZ = function() {
this.bon();
this.boo();
};
e.prototype.boa = function() {
var t;
null === (t = this.mf) || void 0 === t || t.bxL();
};
e.prototype.bob = function() {
var t;
null === (t = this.mf) || void 0 === t || t.bxL();
};
e.prototype.boc = function() {
var t;
null === (t = this.mf) || void 0 === t || t.bxL();
};
e.prototype.bod = function(t) {
this.lW.iT(t);
};
e.prototype.boe = function(t) {
var e, i, o, n, r;
if (this.lW.iT(t.atkEntity) && !0 !== (null === (o = null === (i = null === (e = t.target) || void 0 === e ? void 0 : e.hE) || void 0 === i ? void 0 : i.bhw) || void 0 === o ? void 0 : o.hideInfo) && this.bnQ !== (null === (n = t.target) || void 0 === n ? void 0 : n.hg)) {
this.bnQ = null === (r = t.target) || void 0 === r ? void 0 : r.hg;
this.bnR = t.beforeHP;
}
};
e.prototype.bof = function(t, e) {
for (var i = 0; i < t.length; i++) {
var o = t[i];
if (o.Dg.commonType === e) return o;
}
return null;
};
e.prototype.bog = function() {
var t, e, i, o = this.me, n = this.mf;
if (null == n ? void 0 : n.isValid) {
if (o.fn) {
var r = Math.floor(Math.max(0, o.fn - o.fk)), a = "";
if (r > 60) {
var c = r % 60;
a = Math.floor(r / 60) + ":" + c;
} else a = "" + r;
n.bxs(r < 20);
if (this.bnV !== a) {
this.bnV = a;
n.battleTime.string = a;
}
}
var l = this.lX.nv();
if (l) {
var u = l.hv, f = u.bdU, g = l.hE;
if (g) {
var m = n.bwn;
if (m.splitOpenPanel.node.active && ($ = null == (_ = this.lW.iN(l.hg)) ? void 0 : _.HI) && $.isValid) {
$.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R, d);
this.lW._in.mainCamera.getWorldToScreenPoint(d, v);
m.splitOpenPanel.node.parent.convertToNodeSpaceAR(v, d);
d.y += 117;
m.splitOpenPanel.node.setPosition(d);
}
if (m.selectPlayerPanel.node.active) {
var _, $;
if (($ = null == (_ = this.lW.iN(l.hg)) ? void 0 : _.HI) && $.isValid) {
$.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R, d);
this.lW._in.mainCamera.getWorldToScreenPoint(d, v);
m.selectPlayerPanel.node.parent.convertToNodeSpaceAR(v, d);
d.y += 117;
m.selectPlayerPanel.node.setPosition(d);
}
}
for (var w = null, D = null, P = 0; P < f.length; P++) {
var R = (it = f[P]).Dg.commonType;
w || R !== b.cvResUrl.commonSkillDodge2Cid ? D || R !== b.cvResUrl.commonSkillDodgeCid || (D = it) : w = it;
}
var S = !!(null === (t = g.bho) || void 0 === t ? void 0 : t.dodgeSkill);
m.fzw.forEach(y);
if (null === (e = g.bho) || void 0 === e ? void 0 : e.skBtnCges) for (var E = 0, C = g.bho.skBtnCges; E < C.length; E++) {
var O = C[E];
if (m.fzw.has(O.originBtnId)) {
var T = O.sid;
if (O.csid) (q = this.bof(f, O.csid)) && (T = q.CX); else if (!u.beg(O.sid)) continue;
if (!T) continue;
m.fzw.get(O.originBtnId).cgSid = T;
}
}
for (var I = 0, M = m.fzy; I < M.length; I++) if (!(W = M[I]).AG && m.fzw.has(W.AF)) {
T = (O = m.fzw.get(W.AF)).cgSid > 0 ? O.cgSid : O.sid;
W.AL !== T && W.AO(T);
}
var x = this.bnO;
x.clear();
for (var k = 0, H = g.bhr; k < H.length; k++) {
var N = H[k];
N.sid ? N.tgs.length > 1 ? p.default.Fj(l, N.tgs) && x.add(N.sid) : x.add(N.sid) : N.csid && (q = this.bof(f, N.csid)) && x.add(q.CX);
if (N.t === h.$uo.$uW.bH) {
var j = N.p.changesGroup, F = !0;
if (N.p.consumeSid) {
var L = u.beg(N.p.consumeSid);
F = L && p.default.Ff(l, L);
}
if (F) {
var A = g.bhw.changeGroup[N.p.changesGroup];
if (A && A.length > 0) for (var B = 0, X = A; B < X.length; B++) {
var U = X[B];
U.sid && x.add(U.sid);
if (U.csid) {
var q;
(q = this.bof(f, U.csid)) && x.add(q.CX);
}
}
"Dodge" === j && (S = !0);
}
}
}
x.has(b.cvResUrl.commonDodge2Sid) && (S = !0);
for (var G = 0, Y = m.fzy; G < Y.length; G++) {
var W = Y[G];
if (it = u.beg(W.AL)) if (W.AG || W.AF !== s.BATTLE_BTN_$0.DODGE) {
var K = x.has(it.CX) && p.default.Ff(l, it);
W.AY(it.CZ > 1, it.CY, 1 - it.cd / it.maxCdTime, it.CZ === it.CY, K, it.cd);
} else if (S && w) {
w.CX !== W.AL && W.AO(w.CX);
var Q = p.default.Ff(l, w);
W.AY(w.CZ > 1, w.CY, 1 - w.cd / w.maxCdTime, w.CZ === w.CY, Q, w.cd);
} else {
var V = !!w;
D.CX !== W.AL && W.AO(D.CX);
var z = x.has(it.CX) && p.default.Ff(l, D);
W.AY(V && w.CZ > 1, V ? w.CY : 0, 1 - it.cd / it.maxCdTime, !0, z, D.cd);
}
}
if (m.splitBtn.node.active) for (var J = m.splitBtn.fAW, Z = 0; Z < J.length; Z++) if (it = u.beg(J[Z])) {
K = x.has(it.CX) && p.default.Ff(l, it);
m.splitBtn.fBc(Z, it.CZ > 1, it.CY, 1 - it.cd / it.maxCdTime, it.CZ === it.CY, K, it.cd);
}
for (var tt = 0, et = f; tt < et.length; tt++) {
var it;
if ((null === (i = null == (it = et[tt]) ? void 0 : it.Dg) || void 0 === i ? void 0 : i.isShowPassiveSkillIcon) && (W = m.fzj[it.CX])) {
W.fAg(it.CX);
W.fAn(!0, it.CY, 1 - it.cd / it.maxCdTime, it.CZ === it.CY, !0, it.cd);
}
}
}
}
}
};
e.prototype.boh = function(t) {
var e = this.me, i = this.mf;
if (null == i ? void 0 : i.isValid) {
if (this.lX.mx) {
this.bnU += t;
this.bnU > .2 && (this.bnU = 0);
}
this.lX.ne.fF.forEach(function(t) {
var o = e.gi(t.cXZ);
if (null == o ? void 0 : o.hv) {
var n = o.hv, r = t.cXO - 1, s = i.bws[r];
if (s) {
s.bym(n.beu, n.bev);
s.byn(n.bew, n.bex);
if (o.hN) {
var a = o.hN;
s.byr(-1 === a.bgT ? 0 : Math.max(a.bgT - a.bgW, 0), -1 === a.bgP ? 0 : Math.max(a.bgP - a.bgX, 0), -1 === a.bgR ? 0 : Math.max(a.bgR - a.bgY, 0));
}
}
}
});
}
};
e.prototype.boi = function(t) {
var e, i, o, n, r, s, a, c = this.me, l = this.mf;
if (null == l ? void 0 : l.isValid) {
var h = l.bwn, u = this.lX.nv();
if (u) {
var b = u.hv;
null === (e = l.bwq) || void 0 === e || e.byA(b.beu, b.bev);
null === (i = l.bwq) || void 0 === i || i.byB(b.bew, b.bex);
null === (o = l.bwq) || void 0 === o || o.byz(b.bey);
null === (n = l.bwq) || void 0 === n || n.byE(this.bok(u, this.bnW));
this.lX.nn() && (null === (r = l.bwr) || void 0 === r || r.bvy(this.bol(this.lX.ne.fF)));
if (-1 !== this.bnQ) {
var f = this.bnQ !== this.bnS;
this.bnS = this.bnQ;
var d = c.gi(this.bnQ);
if (f) this.bnP = 0; else if (!(null == d ? void 0 : d.hU)) {
this.bnP += t;
if (this.bnP > 1) {
this.bnP = 0;
this.bnQ = -1;
}
}
if (d) {
l.bwp.node.active || (l.bwp.node.active = !0);
var v = d.hv, y = v.bdQ, g = $fc.i18n.JN(y.nameText), m = null === (s = d.hx) || void 0 === s ? void 0 : s.bjK;
if (m) {
var _ = this.lX.nu(m);
_ && (g = _.cXJ.nick + " - " + g);
}
if (l.bwp) {
l.bwp.byd(y.id, v.bey);
l.bwp.byc(this.bok(d, this.bnX));
var $ = Math.min(1e3, v.bev), w = v.beu;
if (f) {
l.bwp.bxX.brt(this.bnR, $);
if (this.bnR !== w) {
l.bwp.bxX.brm > w && l.bwp.bye();
l.bwp.bxX.brs(w, $, v.beD > 0);
}
} else {
l.bwp.bxX.brm > w && l.bwp.bye();
l.bwp.bxX.brs(w, $, v.beD > 0);
}
}
}
} else l.bwp.node.active && (l.bwp.node.active = !1);
var D = u.hL, P = b.beF(), R = b.beH(), S = $fc.timeMgr.cHr;
if (D.bjF.length > 0 && this.lX.ne.fM.q) for (var E = 0; E < D.bjF.length; E++) {
var C = D.bjF[E], O = h.fzi[E];
if (O) {
var T = $fc.bag.bUH(C.DL), I = p.default.Fd(u, C) && T > 0;
O.AZ(!0, T, 1 - C.DO / C.DP, !1, I, C.DO);
if (I && (null === (a = C.DV) || void 0 === a ? void 0 : a.isAuto)) {
var M = !1;
C.DW && 100 * P < C.DV.applyValue && (M = !0);
C.DX && 100 * R < C.DV.applyValue && (M = !0);
if (M && S - this.bnT > 500) {
this.bnT = S;
this.bou(E);
}
}
}
}
}
}
};
e.prototype.boj = function(t) {
return this.bok(t, []);
};
e.prototype.bok = function(t, e) {
var i = 0, o = t && t.hE;
if (o) for (var n = o.bhs, r = 0; r < n.length; r++) {
var s = n[r], a = s.Dq && s.Dq.icon;
if (a) {
var c = s.Du > 0 ? cc.misc.clampf((s.Du - s.Dx) / s.Du, 0, 1) : -1, l = e[i];
if (!l) {
l = {
b: "",
c: 0
};
e[i] = l;
}
l.b = "ccui:icon/buff/" + a;
l.c = c;
i++;
}
}
e.length = i;
return e;
};
e.prototype.bol = function(t) {
var e = [], i = this.me;
if (!i) return e;
for (var o = 0, n = t; o < n.length; o++) {
var r = n[o], s = i.gi(r.cXZ);
if (null == s ? void 0 : s.hv) {
var a = s.hv;
e.push({
b: r.cXL,
c: a.bdQ.avatar,
d: r.cXM,
e: a.beu,
f: a.bev,
g: a.finalDamage,
h: this.boj(s),
i: $fc.battleServer.getNetworkStatus(r.cXL),
j: r.cXN
});
}
}
return e;
};
e.prototype.bom = function() {
var t, e = this.mf;
if (e) {
var i = this.lX.nv();
if (i) {
var o = i.hv;
o && (null === (t = e.bwq) || void 0 === t || t.byz(o.bey));
}
}
};
e.prototype.bon = function() {
var t, e = this.mf;
if (e) {
var i = this.lX.nJ();
if (i && i.fM.h) {
var o = this.lX.nw();
e.bxC(null !== (t = null == o ? void 0 : o.cYg) && void 0 !== t && t);
}
}
};
e.prototype.boo = function() {
var t = this.mf;
if (t && this.lX.nJ()) {
this.lX.nw();
t.bwm.node.active = !this.lX.nk;
t.bxK(!this.lX.nk);
}
};
e.prototype.bop = function() {
var t, e, i, o, n, c, h, u, b, p, f, d, v, y, g, m, _, $, w, D, P, R, S, E, C, O, T = this.lX.nv();
if (T) {
this.lX.nb.bzy.clear();
var I, M, x = new l.$un(), k = new l.$um(), H = T.hL;
if (!(null === (t = H.bjE) || void 0 === t ? void 0 : t.skillButton) || this.lX.ne.fj.traning) {
k.cXb();
var N = T.hv.bdU.find(function(t) {
return 1 === t.Dg.commonType;
}), j = T.hv.bdU.find(function(t) {
return 2 === t.Dg.commonType;
}), F = T.hv.bdU.find(function(t) {
return 3 === t.Dg.commonType;
});
k.cWT[s.BATTLE_BTN_$0.NORMAL].cWL = null !== (e = null == N ? void 0 : N.CX) && void 0 !== e ? e : 0;
k.cWT[s.BATTLE_BTN_$0.JUMP].cWL = null !== (i = null == j ? void 0 : j.CX) && void 0 !== i ? i : 0;
k.cWT[s.BATTLE_BTN_$0.DODGE].cWL = null !== (o = null == F ? void 0 : F.CX) && void 0 !== o ? o : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL1].cWL = null !== (n = H.bjG[a.$ug.g]) && void 0 !== n ? n : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL2].cWL = null !== (c = H.bjG[a.$ug.h]) && void 0 !== c ? c : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL3].cWL = null !== (h = H.bjG[a.$ug.i]) && void 0 !== h ? h : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL4].cWL = null !== (u = H.bjG[a.$ug.j]) && void 0 !== u ? u : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL5].cWL = null !== (b = H.bjG[a.$ug.k]) && void 0 !== b ? b : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL6].cWL = null !== (p = H.bjG[a.$ug.l]) && void 0 !== p ? p : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL7].cWL = null !== (f = H.bjG[a.$ug.m]) && void 0 !== f ? f : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL8].cWL = null !== (d = H.bjG[a.$ug.n]) && void 0 !== d ? d : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL9].cWL = null !== (v = H.bjG[a.$ug.o]) && void 0 !== v ? v : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL10].cWL = null !== (y = H.bjG[a.$ug.p]) && void 0 !== y ? y : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL11].cWL = null !== (g = H.bjG[a.$ug.q]) && void 0 !== g ? g : 0;
k.cWT[s.BATTLE_BTN_$0.SKILL12].cWL = null !== (m = H.bjG[a.$ug.r]) && void 0 !== m ? m : 0;
k.cWU.cWJ([ null !== (_ = H.bjG[a.$ug.v]) && void 0 !== _ ? _ : 0, null !== ($ = H.bjG[a.$ug.v + 1]) && void 0 !== $ ? $ : 0, null !== (w = H.bjG[a.$ug.v + 2]) && void 0 !== w ? w : 0, null !== (D = H.bjG[a.$ug.v + 3]) && void 0 !== D ? D : 0 ]);
k.cWT.forEach(function(t) {
t.cWL > 0 && (T.hv.beg(t.cWL) || (t.cWL = 0));
});
} else {
var L = $fc.skill.cqk.cqf(r.$fe.tblEnum.SkillTypeTableEnum.PVE_SKILL), A = $fc.skill.cqk.cqf(this.lX.ne.gN ? r.$fe.tblEnum.SkillTypeTableEnum.PVP_SKILL : r.$fe.tblEnum.SkillTypeTableEnum.PVE_SKILL);
k.cWX(L);
k.cWY(A);
}
I = [ x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.d] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.e] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.f] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.g] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.h] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.i] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.j] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.k] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.l] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.m] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.n] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.o] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.p] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.q] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.r] - 1) ];
M = [ x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.v] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.v + 1] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.v + 2] - 1), x.cXn(a.FRAME_SYNC_CMD2BIT[a.$ug.v + 3] - 1) ];
for (var B in a.FRAME_SYNC_CMD2BIT) {
var X = a.FRAME_SYNC_CMD2BIT[B];
this.lX.nb.bzy.set(x.cXn(X - 1), Number(B));
}
var U = (null === (P = T.hD) || void 0 === P ? void 0 : P.biI) || 1, q = [];
this.lX.ne.fF.forEach(function(t) {
t.cXQ === U && q.push({
position: t.cXO,
baseInfo: {
confId: t.cYm[0].roleCfgId,
level: t.cXN,
fightpoint: 0,
name: t.cXM,
roleId: t.cXL,
version: "",
personal: null,
skill: []
}
});
});
null === (R = this.mf.bwn) || void 0 === R || R.fzC(k, q, 0, this.bor.bind(this), this.bos.bind(this), this.bot.bind(this));
(null === (S = this.lX.mw.bAO.e) || void 0 === S ? void 0 : S.c) || null === (E = this.mf.bwn) || void 0 === E || E.fzU(I, M);
T.hL && this.lX.ne.fM.q && (null === (C = this.mf.bwn) || void 0 === C || C.fzE(T.hL.bjF, this.bou.bind(this)));
var G = this.boq();
null === (O = this.mf.bwn) || void 0 === O || O.fzF(G, T.hv.bdQ.id);
}
};
e.prototype.boq = function() {
var t, e = this.lX.nv();
if (!(null == e ? void 0 : e.hv)) return [];
for (var i = [], o = 0, n = e.hv.bdU; o < n.length; o++) {
var r = n[o];
(null === (t = null == r ? void 0 : r.Dg) || void 0 === t ? void 0 : t.isShowPassiveSkillIcon) && i.push(r);
}
return i;
};
e.prototype.bor = function(t, e, i) {
if (this.lX.nv() && t) {
var o = s.BATTLE_BTN_2_CMD[i];
o && this.lX.nb.bzQ(e, o);
}
};
e.prototype.bos = function(t, e, i) {
if (this.lX.nv() && t) {
var o = a.$ug.v + i;
this.lX.nb.bzQ(e, o);
}
};
e.prototype.bot = function(t, e, i, o, n) {
if (this.lX.nv() && t) {
var r = s.BATTLE_BTN_2_CMD[e];
r && n && this.lX.nb.bAc(r, n);
}
};
e.prototype.bou = function(t) {
var e, i, o, n = this.lX.nv();
if (null == n ? void 0 : n.hL) {
var r = n.hL.bjF[t];
r && p.default.Fd(n, r) && ($fc.bag.bUH(r.DL) <= 0 || this.lX.nb.bAn(t, r.DL, null === (o = null === (i = null === (e = this.lX.mw) || void 0 === e ? void 0 : e.bAt) || void 0 === i ? void 0 : i.params) || void 0 === o ? void 0 : o.playMode));
}
};
e.prototype.bov = function() {
this.bop();
};
e.prototype.bow = function() {
this.mf && this.box();
};
e.prototype.box = function() {
var t = this, e = this.mf;
e && [ 0, 1 ].forEach(function(i) {
var o = e.bws[i];
if (o) {
var n = t.lX.ne.fF.find(function(t) {
return t.cXO === i + 1;
});
if (n) {
var r = t.lX.ne.gi(n.cXZ);
if (null == r ? void 0 : r.hv) {
var s = r.hv;
o.node.active = !0;
o.byh(s.bey, s.bdQ.id, n.cXM, n.cXL);
}
} else o.node.active = !1;
}
});
};
return e;
}(f.$gt);
i.$ka = g;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSP": void 0,
"../../../game/src/battle/types/$DYO": void 0,
"../../../game/src/battle/types/$DYQ": void 0,
"../../../game/src/battle/types/$DYR": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../../main/src/$KV": void 0,
"../../battleLogic/$KL": void 0
} ],
$DQM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "697e9U5gwBAMaQoizQ+s6HS", "$DQM");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kb = void 0;
var r = function(t) {
n(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lY = function() {
this.boy();
};
e.prototype.lZ = function() {
var t;
null === (t = this.lX) || void 0 === t || t.ni.byW(this);
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.prototype.md = function() {};
e.prototype.ma = function() {};
e.prototype.boy = function() {};
e.prototype.boz = function() {
if (this.lW._in) {
var t = this.me, e = this.mf, i = t.gs().bhe, o = 0, n = 0;
if (i && i.duration > 0) {
var r = i.moments[0], s = t.gk(r);
if (s) {
var a = s.cYo();
if (a) {
o = a.roleCfgId;
n = a.skinId;
}
}
}
e.bxy(o, n);
if (t.fn) {
var c = Math.floor(Math.max(0, t.fn - t.fk)), l = "";
if (c > 60) {
var h = c % 60;
l = Math.floor(c / 60) + ":" + h;
} else l = "" + c;
e.bxs(c < 20);
e.battleTime.string = l;
}
}
};
return e;
}(t("../$KQ").$gt);
i.$kb = r;
cc._RF.pop();
}, {
"../$KQ": "$KQ"
} ],
$DQN: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "f35c1fLq0NKoY5C+gcdsCYA", "$DQN");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kc = void 0;
var r = t("../../../game/src/battle/$DSO"), s = t("../../../game/src/battle/types/$DYS"), a = t("../../../game/src/$KU"), c = t("../../../game/src/perfab/town/$FDZ"), l = t("../../battleLogic/component/$DPK"), h = t("../$KQ"), u = cc.v2(), b = (new Set(), 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boA = 0;
e.boB = !1;
e.boC = [];
e.boE = 120;
e.boF = cc.v2();
return e;
}
e.prototype.md = function() {};
e.prototype.lY = function() {
this.boA = 0;
};
e.prototype.lZ = function() {
this.lX.ni.byW(this);
this.boD = null;
this.boC.length = 0;
};
e.prototype.ma = function(t) {
if (this.lW._in) {
var e = this.me.fI;
if (e) if (e.cYz && 0 !== e.cYM) {
this.boA += 3 * t;
var i = this.lX.nv();
if (i) {
var o = this.lW.iM(i);
u.x = o.Hu;
u.y = o.Hv;
var n = this.me.fE.bkN.find(function(t) {
return t.ho(l.default).bka === e.cYM;
});
if (n) {
this.boB = !0;
this.boH(n.hw.x, n.hw.y);
this.boG();
}
}
} else this.boB && this.boI();
}
};
e.prototype.boG = function() {
if (this.boC.length) for (var t = 0, e = this.boC; t < e.length; t++) e[t].opacity = 255 * (.5 * (.5 * Math.sin(this.boA) + .5) + .5);
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.prototype.boH = function(t, e) {
var i = this.lW._in;
if (i) {
var o = this.lX.nv();
if (o) {
var n = this.lW.iM(o), l = t - n.Hu > 0 ? 1 : -1, h = e - n.Hv > 0 ? 1 : -1, u = Math.abs(n.Hu - t), b = Math.abs(n.Hv - e), p = Math.sqrt(u * u + b * b), f = this.boE, d = Math.floor(p / f);
this.boF.x = n.Hu;
this.boF.y = n.Hv;
for (var v = 0; v < d; v++) {
var y = this.boF.x, g = this.boF.y, m = this.boF.x + f * l, _ = this.boF.y + f * s.$uo.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y * h;
(1 == l && m > t || -1 == l && m < t) && (m = t);
(1 == h && _ > e || -1 == h && _ < e) && (_ = e);
this.boF.x = m;
this.boF.y = _;
var $ = Math.abs(m - t) + Math.abs(_ - e) < 15, w = r.default.bEC.bFt(this.boF.y);
if (P = this.boC[v]) P.setPosition(cc.misc.lerp(P.position.x, this.boF.x, .3), cc.misc.lerp(P.position.y, w, .3)); else {
(P = $fc.prefabMgr.cLm(a.default.sceneComp.Arrow)).__cvResCfg = a.default.sceneComp.Arrow;
this.boC.push(P);
i.sceneLayer.addChild(P, s.$uo.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
P.setPosition(this.boF.x, w);
}
P.angle = this.boJ(y, r.default.bEC.bFt(g), this.boF.x, w);
if (v == d - 1 || $) {
if (!this.boD) {
var D = $fc.prefabMgr.cLm(a.default.sceneComp.GO);
D.__cvResCfg = a.default.sceneComp.GO;
i.sceneLayer.addChild(D, 0, "goArrow");
this.boD = D.getComponent(c.default);
}
this.boD.node.active = !0;
this.boD.node.zIndex = s.$uo.TOWN_SCENE_DEFINE.FIGHT_TEXT;
if (u > b) {
1 == l ? this.boD.fHM() : this.boD.fHL();
this.boD.node.setPosition(1 == l ? t - 200 : t + 200, r.default.bEC.bFt(e));
} else {
this.boD.node.setPosition(t, r.default.bEC.bFt(e) + 100);
this.boD.fHN();
}
}
if ($) break;
}
for (v = this.boC.length - 1; v >= d; v--) {
var P = this.boC[v];
$fc.prefabMgr.cLp(P);
this.boC.splice(v, 1);
}
}
}
};
e.prototype.boI = function() {
var t;
this.boB = !1;
(null === (t = this.boD) || void 0 === t ? void 0 : t.isValid) && (this.boD.node.active = !1);
this.boC.forEach(function(t) {
$fc.prefabMgr.cLp(t);
});
this.boC.length = 0;
};
e.prototype.boJ = function(t, e, i, o) {
return 180 * Math.atan2(o - e, i - t) / Math.PI;
};
return e;
}(h.$gt));
i.$kc = b;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../../game/src/$KU": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../../game/src/perfab/town/$FDZ": void 0,
"../../battleLogic/component/$DPK": void 0
} ],
$DQO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ee9deZ6XbhBEYdrcl1bTO+l", "$DQO");
var o, n, r = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kd = void 0;
var s = t("../../../bingo/src/$KG"), a = t("../../../game/src/battle/$DSO"), c = t("../../../game/src/battle/types/$DYS"), l = t("../../../main/src/$KV"), h = t("../../../main/src/tbl/TblEnum"), u = t("../../battleLogic/component/$DPL"), b = t("../../battleLogic/component/$DPN"), p = t("../$KQ"), f = t("../render/$DQW"), d = t("../render/$DQX"), v = t("../render/$DQY"), y = (cc.v2(), 
new Set(), (n = {})[h.TblEnum.QualityTypeTableEnum.COMMON] = l.cvResUrl.mc_drop_PuTong_pick, 
n[h.TblEnum.QualityTypeTableEnum.SENIOR] = l.cvResUrl.mc_drop_Gaoji_pick, n[h.TblEnum.QualityTypeTableEnum.RARE] = l.cvResUrl.mc_drop_XiYou_pick, 
n[h.TblEnum.QualityTypeTableEnum.EPIC] = l.cvResUrl.mc_drop_ShenQi_pick, n[h.TblEnum.QualityTypeTableEnum.LEGEND] = l.cvResUrl.mc_drop_ShiShi_pick, 
n), g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boK = [];
return e;
}
e.prototype.md = function() {};
e.prototype.lY = function() {
this.boK.length = 0;
this.lX.ni.byU(c.$uo.$vk.bj, this.boQ, this);
this.lX.ni.byU(c.$uo.$vk.bk, this.boR, this);
this.lX.ni.byU(c.$uo.$vk.m, this.boT, this);
this.lX.ni.byU(c.$uo.$vk.p, this.boU, this);
};
e.prototype.lZ = function() {
this.lX.ni.byW(this);
this.boK = null;
};
e.prototype.ma = function(t) {
var i, o = this.lW._in;
if (o) {
var n = this.lX.nv();
if (n) for (var r = this.lW.iM(n), l = this.lX.ne.gt(u.default), p = this.lX.ne.gt(b.default).worldTimeScale, f = this.lX.mF ? 0 : 1, y = t * p * f, g = this.lW.ir.length - 1; g >= 0; --g) {
var m = this.lW.ir[g], _ = a.default.bEC.bEY(m.IB.itemId);
if (_) {
var $ = null !== (i = _.quality) && void 0 !== i ? i : 0;
if (!m.IC) {
var w = this.lX.mZ.bDE(s.$fe.ResUrl.DropItemRender);
if (!w) continue;
o.sceneLayer.addChild(w, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + $);
m.IC = w.getComponent(d.default);
m.IC.bra(m.IB);
}
if ($ >= h.TblEnum.QualityTypeTableEnum.EPIC && !m.ID && (P = this.lX.mZ.bDE(s.$fe.ResUrl.DropItemText))) {
o.sceneLayer.addChild(P, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + 10 + $);
m.ID = P.getComponent(v.default);
m.ID.brg(m.IB);
}
m.IC.setTimeScale(p * f);
if (m.Iz) {
if (this.boM(r.Hu, r.Hv, m.Is, m.It) && m.Iy) {
var D = this.lW.ir.splice(g, 1)[0];
this.boN(D);
}
} else {
m.Is += m.Iv * y;
m.It += m.Iw * y;
m.Iu += m.Ix * y + -2300 * y * y / 2;
if (m.Iu <= 0) {
m.Iu = 0;
m.Iz = !0;
if (m.IC) {
m.IC.brd();
m.IC.node.zIndex = m.Iy ? a.default.bEC.bFF(m.It) : c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT;
}
if (!m.ID) {
var P;
if (P = this.lX.mZ.bDE(s.$fe.ResUrl.DropItemText)) {
o.sceneLayer.addChild(P, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + 10 + $);
m.ID = P.getComponent(v.default);
m.ID.brg(m.IB);
}
}
}
m.Ix += -2300 * y;
e.boL(m, l);
m.IH();
}
}
}
}
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.boL = function(t, e) {
t.Is = Math.max(Math.min(t.Is, e.bkm), e.bkl);
t.It = Math.max(Math.min(t.It, e.bko), e.bkn);
};
e.prototype.boM = function(t, e, i, o) {
return Math.abs(i - t) < c.$uo.TOWN_SCENE_DEFINE.dropRange[0] && Math.abs(o - e) < c.$uo.TOWN_SCENE_DEFINE.dropRange[1];
};
e.prototype.boN = function(t, e) {
var i;
void 0 === e && (e = !1);
if (t) {
var o = this.lX.nw();
if (o.cXZ) {
var n = this.lW.iN(o.cXZ);
if ((null == n ? void 0 : n.HI) && t.IC) {
var r = this.lX.mZ.bDE(l.cvResUrl.DropItemPickBall);
if (r) {
this.lW._in.sceneLayer.addChild(r, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT);
r.x = t.Is;
r.y = t.IC.node.y;
r.getComponent(f.default).bqT(t.IB, o.cXZ);
}
}
}
var s = null === (i = t.IC) || void 0 === i ? void 0 : i.bqX, a = y[null == s ? void 0 : s.quality];
a && this.lW.jU(a, t.Is, t.It, t.Iu, 1, 0, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM);
t.IF();
}
};
e.prototype.boO = function(t, e) {
var i;
if (t) {
if (e) {
var o = this.lW.iN(e.hg);
if (null == o ? void 0 : o.HI) {
var n = this.lX.mZ.bDE(l.cvResUrl.DropItemPickBall);
if (n) {
this.lW._in.sceneLayer.addChild(n, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM_TXT);
n.x = t.Is;
n.y = t.IC.node.y;
n.getComponent(f.default).bqT(t.IB, e.hg);
}
}
}
var r = null === (i = t.IC) || void 0 === i ? void 0 : i.bqX, s = y[null == r ? void 0 : r.quality];
s && this.lW.jU(s, t.Is, t.It, t.Iu, 1, 0, c.$uo.TOWN_SCENE_DEFINE.DROP_ITEM);
t.IF();
}
};
e.prototype.boP = function() {
for (var t = 0, e = this.lW.ir.slice(); t < e.length; t++) {
var i = e[t];
this.boN(i);
}
this.lW.ir.length = 0;
};
e.prototype.boQ = function(t, i, o, n, r) {
var c, l;
void 0 === r && (r = !0);
if (null == n ? void 0 : n.dropList) {
var h = null === (l = null === (c = this.lX) || void 0 === c ? void 0 : c.ne) || void 0 === l ? void 0 : l.gt(u.default);
if (h) for (var b = 0, f = n.dropList; b < f.length; b++) {
var d = f[b], v = a.default.bEC.bEY(d.itemId);
if (v && v.id !== s.$fe.tblEnum.ItemTableEnum.EXP_ID) {
var y = new p.$gx();
y.Iq = this.lW.ip;
this.lW.ir.push(y);
y.IA = n.uniqueId;
y.IB = d;
y.Is = t;
y.It = i;
y.Iu = Math.max(o, 70);
e.boL(y, h);
var g = 360 * Math.random() * Math.PI / 180, m = 50 + Math.floor(350 * Math.random());
y.Iv = Math.cos(g) * m;
y.Iw = Math.sin(g) * m;
y.Ix = 600 + Math.floor(300 * Math.random());
y.Iy = r;
}
}
}
};
e.prototype.boR = function(t) {
for (var e = 0, i = this.lW.ir.slice(); e < i.length; e++) {
var o = i[e];
this.boO(o, t);
}
this.lW.ir.length = 0;
};
e.prototype.boS = function() {};
e.prototype.boT = function() {
this.boP();
};
e.prototype.boU = function() {
var t = this;
this.lW.ip.ne.gC() || $fc.timeMgr.cHv(3e3, 1, function() {
var e, i;
(null === (i = null === (e = t.lW) || void 0 === e ? void 0 : e.ip) || void 0 === i ? void 0 : i.nl()) && t.lW.ir && t.lW.ir.length > 0 && t.boP();
});
};
return e;
}(p.$gt);
i.$kd = g;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../../main/src/$KV": void 0,
"../../../main/src/tbl/TblEnum": void 0,
"../../battleLogic/component/$DPL": void 0,
"../../battleLogic/component/$DPN": void 0,
"../render/$DQW": "$DQW",
"../render/$DQX": "$DQX",
"../render/$DQY": "$DQY"
} ],
$DQP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b2b5c8GSCBLlZ1Gw+yq++06", "$DQP");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$ke = void 0;
var r = t("../../battleLogic/$KL"), s = t("../../battleLogic/component/$DOZ"), a = t("../../battleLogic/component/$DPL"), c = t("../$KQ"), l = t("../../battleLogic/component/$DOT"), h = t("../../battleLogic/component/$DOU"), u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boV = cc.v2();
e.boW = !0;
return e;
}
e.prototype.lY = function() {};
e.prototype.lZ = function() {};
e.prototype.ma = function() {
for (var t, i, o, n, s, c, l, h = this.lX.ne, u = this.lX.nG(), b = h.gt(a.default), p = this.lX.mF ? 0 : 1, f = 0, d = 0, v = 0, y = 0, g = 0, m = 0, _ = !0, $ = 0, w = 0, D = !1, P = 0, R = h.fw.bkN; P < R.length; P++) {
var S = R[P];
if (!S.hJ && (l = this.lW.iM(S))) {
$ = u * (S = l.HY() || S).hy.worldTimeScale * S.hy.hitPauseTimeScale * S.hy.hTimeScale * p;
s = S.hw;
f = (c = S.hz).biZ;
d = c.bja;
v = c.bjb;
y = c.bjc;
g = c.bjd;
m = 0;
var E = S.hE;
D = (null === (t = S.hv) || void 0 === t ? void 0 : t.beB) > 0;
w = s.z;
if (E && E.bhR && r.default.ED(S)) {
var C = null !== (i = E.bhS.z) && void 0 !== i ? i : 600, O = E.bhS.height - w;
v = Math.abs(O) > 8 ? O > 0 ? C : -C : c.bjb = 0;
} else m = !s.bkA && (null === (o = S.hv) || void 0 === o ? void 0 : o.bfa) && !(null == E ? void 0 : E.bit) ? c.bje + -(null === (n = S.hv) || void 0 === n ? void 0 : n.bfa) * S.hE.bhW : c.bje;
_ = S.ic;
if (0 === f && 0 === y || D) l.Hu = s.x; else {
c.bjg > 10 && (y = -Math.sign(f) * c.bjg);
l.Hu = s.x + f * $ + y * $ * $ / 2;
}
l.Hv = 0 === d && 0 === g || D ? s.y : s.y + d * $ + g * $ * $ / 2;
if (0 !== v || 0 !== m) {
$ = u * S.hy.worldTimeScale * S.hy.hitPauseTimeScale * S.hy.vTimeScale * p;
l.Hw = s.z + v * $ + m * $ * $ / 2;
} else l.Hw = s.z;
_ && e.boX(l, b);
if (c.bjk) {
var T = c.bjk;
if (1 === T.type) {
var I = T.param[0], M = T.param[1], x = T.param[2];
if (I > 0) {
var k = l.Hu - M, H = l.Hv - x, N = k * k + H * H;
if (N > I * I && N > 0) {
var j = I / Math.sqrt(N);
l.Hu = M + k * j;
l.Hv = x + H * j;
}
}
}
}
}
}
this.boY(u);
this.boZ(u);
};
e.boX = function(t, e) {
t.Hu = Math.max(Math.min(t.Hu, e.bkm), e.bkl);
t.Hv = Math.max(Math.min(t.Hv, e.bko), e.bkn);
t.Hw = Math.max(Math.min(t.Hw, 5e3), 0);
};
e.prototype.mb = function() {};
e.prototype.mc = function() {};
e.prototype.md = function() {};
e.prototype.boY = function(t) {
for (var e, i = this.lX.ne, o = i.fr, n = 0, r = 0, s = i.fC.bkN; r < s.length; r++) {
var a = s[r], c = a.ho(l.default);
if (c && o.bll(a.hg) && a.hA && a.hA) {
var u = o.bll(c.bgF);
if (u && u.hD) for (var b = u.hD.biI, p = c.bgG, f = u.hw, d = 0, v = a.hA.biQ; d < v.length; d++) {
var y = v[d], g = o.bll(y);
if (g) {
var m = g.ho(h.default);
if (m) {
var _ = o.bll(m.bgK);
if (_ && _.hD && _.hD.biI !== b) {
e = this.lW.iM(_);
if (_.hy) {
if (_.hv) {
var $ = _.hv;
if ($.beB > 0) continue;
if (_.hE) {
if (_.hE.big) continue;
if (_.hE.bit) continue;
}
if ($ && $.ber) continue;
}
n = t * _.hy.worldTimeScale * _.hy.hitPauseTimeScale;
if (c.bgH) {
var w = p.x * f.bkw + f.x - e.Hu, D = (P = Math.sign(w)) * Math.min(P * w, c.bgH * n);
e.Hu = e.Hu + D;
}
if (c.bgI) {
w = p.y + f.y - e.Hv, D = (P = Math.sign(w)) * Math.min(P * w, c.bgI * n);
e.Hv = e.Hv + D;
}
if (c.bgJ) {
w = p.z + f.z - e.Hw, D = (P = Math.sign(w)) * Math.min(P * w, c.bgJ * n);
var P, R = e.Hw + D;
R < 0 && (R = 0);
e.Hw = R;
}
}
}
}
}
}
}
}
};
e.prototype.boZ = function(t) {
for (var e, i = this.lX.ne, o = 0, n = (this.boV, 0), r = i.fx.bkN; n < r.length; n++) {
var a = r[n];
if (e = this.lW.iM(a)) {
var c = a.ho(s.default);
if (c) {
var l = i.fr.bll(c.bix);
if (l) {
var h = this.lW.iM(l);
if (h) {
var u = h;
switch (c.biy) {
case 0:
var b, p = c.biz * u.Hx + u.Hu, f = c.biA + u.Hv;
b = "number" == typeof c.biD ? c.biD : c.biB + u.Hw;
o = c.biC ? l.hw.bkw * c.biC : 1;
e.Hu = p;
e.Hv = f;
e.Hw = b;
e.Hx = o;
break;

case 1:
var d = this.lX.mF ? 0 : 1, v = t * a.hy.worldTimeScale * a.hy.hitPauseTimeScale * a.hy.mainTimeScale * d, y = (c.biG + c.biE * v) * Math.PI / 180;
e.Hu = u.Hu + Math.cos(y) * c.biF;
e.Hv = u.Hv + Math.sin(y) * c.biF;
}
}
}
}
}
}
};
return e;
}(c.$gt);
i.$ke = u;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../battleLogic/$KL": void 0,
"../../battleLogic/component/$DOT": void 0,
"../../battleLogic/component/$DOU": void 0,
"../../battleLogic/component/$DOZ": void 0,
"../../battleLogic/component/$DPL": void 0
} ],
$DQQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b60f61atiBOGJRERHvBSVsX", "$DQQ");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kf = void 0;
var r = t("../../../game/src/perf/$DVH"), s = t("../../../game/src/battle/types/$DYS"), a = t("../$KQ"), c = t("../render/$DRB"), l = t("../../../game/src/$KT"), h = t("../../../game/src/manager/$DUP"), u = t("../../../game/src/battle/$DSO"), b = t("../../../bingo/src/$KG"), p = t("../../../game/src/perfab/common/$FDM"), f = t("../render/$DRL"), d = t("../render/$DRH"), v = t("../../../game/src/battle/$DSC"), y = t("../render/$DRG"), g = t("../../../game/src/$KU"), m = t("../../../game/src/perfab/town/$FEB"), _ = t("../../battleLogic/component/$DPM"), $ = t("../../../game/src/perfab/common/$FDW"), w = t("../../battleLogic/$KI"), D = t("../render/$DQR"), P = t("../render/$DRI"), R = (cc.v2(), 
new Set()), S = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bpe = -1;
return e;
}
e.prototype.bpb = function(t, e, i) {
var o, n, r, a, c = null === (o = e.HI) || void 0 === o ? void 0 : o.node;
if (c) {
var h, f = c.x, v = c.y, y = t.hD.biI;
h = e.HO ? 1 : y === i ? 2 : t.hS ? 3 : 2;
if (t.hL) {
if (!e.HQ && t.hx.bjK) {
var _ = this.lX.nu(t.hx.bjK);
if (_) {
var $ = $fc.prefabMgr.cLm(g.default.sceneComp.RoleTopInfoRender);
$.__cvResCfg = g.default.sceneComp.RoleTopInfoRender;
this.lW._in.sceneLayer.addChild($, s.$uo.TOWN_SCENE_DEFINE.ROLE_TOP_INFO);
e.HQ = $.getComponent(m.default);
(null === (n = this.lX.mw.bAO.e) || void 0 === n ? void 0 : n.c) ? e.HQ.fHV("", 0) : e.HQ.fHV(_.cXJ.nick, t.hL.bjy);
e.HQ.node.setPosition(f, v + e.If());
}
}
e.HQ && e.HQ.node.setPosition(f, v + e.If());
if (t.hN && e.Hn.ne.gN) {
if (!e.HT && (R = l.default.Ji.mZ.bDE(b.$fe.ResUrl.RoleTopProtectProgress))) {
e.HT = R.getComponent(P.$kE);
l.default.Ji.nc._in.sceneLayer.addChild(e.HT.node, s.$uo.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 2);
}
if (e.HT) {
var w = t.hN, D = t.hv.beF();
e.HT.btV(-1 !== w.bgT, (w.bgT - D) / w.bgW);
e.HT.btW(-1 !== w.bgP, (w.bgP - D) / w.bgX);
e.HT.btX(-1 !== w.bgR, (w.bgR - D) / w.bgY);
e.HT.node.setPosition(f, v + e.Ie() + 80);
}
}
}
if (!e.HP && (R = this.lW.ip.mZ.bDE(b.$fe.ResUrl.EntityShadow))) {
e.HP = R.getComponent(p.default);
this.lW._in.sceneLayer.addChild(e.HP.node, s.$uo.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
e.HP.fCi(h, null === (r = e.Hq) || void 0 === r ? void 0 : r.shadowSize);
R.setPosition(f, (R.y, u.default.bEC.bFu(e.Hv, 0)));
}
if ((null === (a = e.HP) || void 0 === a ? void 0 : a.node) && e.HI.brO) {
(R = e.HP.node).setPosition(f, cc.misc.lerp(R.y, u.default.bEC.bFu(e.Hv, 0), .37));
R.opacity = e.HI.brS;
}
if (!e.HR) {
var R;
if (R = l.default.Ji.mZ.bDE(b.$fe.ResUrl.RoleTopContent)) {
e.HR = R.getComponent(d.default);
l.default.Ji.nc._in.sceneLayer.addChild(e.HR.node, s.$uo.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 1);
}
}
if (e.HR) {
e.HR.node.setPosition(f, u.default.bEC.bFu(e.Hv, e.Hw + e.Ie()));
e.HR.btS(e.HS);
}
}
};
e.prototype.lY = function() {
this.bpe = -1;
this.lX.ni.byU(s.$uo.$vk.F, this.bpf, this);
this.lX.ni.byU(s.$uo.$vk.G, this.bpc, this);
this.lX.ni.byU(s.$uo.$vk.bq, this.bpd, this);
this.lX.ni.byU(s.$uo.$vk.M, this.bph, this);
this.lX.ni.byU(s.$uo.$vk.N, this.bpi, this);
this.lX.ni.byU(s.$uo.$vk.O, this.bpj, this);
this.lX.ni.byU(s.$uo.$vk.bo, this.bpm, this);
this.lX.ni.byU(s.$uo.$vk.bp, this.bpn, this);
this.lX.ni.byU(s.$uo.$vk.bm, this.bpp, this);
this.lX.ni.byU(s.$uo.$vk.n, this.bpq, this);
this.lX.ni.byU(s.$uo.$vk.bd, this.bpr, this);
this.lX.ni.byU(s.$uo.$vk.bs, this.bps, this);
$fc.eventMgr.kl(l.default.Jg.cDr, this.bpo, this);
$fc.eventMgr.kl(l.default.Jg.cDt, this.bpo, this);
this.bpa = this.bpb;
};
e.prototype.lZ = function() {
this.bpe = -1;
this.lX.ni.byW(this);
this.lX.nj.byW(this);
$fc.eventMgr.kn(this);
};
e.prototype.ma = function(t) {
var e, i, o, n, l, h, d, v = this, y = this.lW._in;
if (y) for (var g = this.lX, m = (g.nv(), null !== (i = null === (e = g.nw()) || void 0 === e ? void 0 : e.cXQ) && void 0 !== i ? i : 0), $ = g.ne.fv.bkN, P = this.lW.iA > 0, S = this.lX.mF ? 0 : 1, E = this.lW.iv, C = function(e) {
var i = O.lW.iM(e);
i.HC = e.hw.bky;
var $ = i.HI, C = i.HY() || e, T = C.hE, I = C.hF, M = C.hv, x = C.hy.worldTimeScale * S, k = x * C.hy.hitPauseTimeScale * C.hy.mainTimeScale;
(null == M ? void 0 : M.bdW) && (k = 0);
if (!$) {
if (!(Z = g.mZ.bDE(b.$fe.ResUrl.EntityRender))) return "continue";
$ = Z.getComponent(c.$kw);
y.sceneLayer.addChild(Z);
i.HI = $;
$.brV(i, I.bgb, g.mZ);
$.node.setPosition(i.Hu, u.default.bEC.bFu(i.Hv, i.Hw));
$.bsc(i.Hy);
if (e.hL) {
var H = $.bsM(), N = u.default.bEC.bFK(0, e.hL.bjC, e.hL.bjD, e.hL.bjE);
if (null == H ? void 0 : H.bqp) {
H.bqp(N.models, 2);
$.bsS(N.weaponUpgrade);
}
i.HL = e.hL.bjB;
}
}
var j = $.node;
if ($.brW !== k) {
$.brY(k);
i.HF.forEach(function(t) {
t.Hi && t.Hg && t.Hi.setTimeScale(k * t.Hf);
});
i.HG.forEach(function(t) {
t.Hi && t.Hi.setTimeScale(k * t.Hf);
});
}
if ($.brX !== x) {
$.brX = x;
i.HF.forEach(function(t) {
t.Hi && !t.Hg && t.Hi.setTimeScale(x * t.Hf);
});
i.HH.forEach(function(t) {
t.Hi && t.Hi.setWorldTimeScale(x);
});
}
var F = null == T ? void 0 : T.bho, L = !1;
if (T) {
L = !!(null === (st = T.bho) || void 0 === st ? void 0 : st.move);
var A = T.bhY;
A && $.bsv(A, T.bhZ);
}
var B = 1;
i.Hx = e.hw.bkw;
if ((null == F ? void 0 : F.xzAngleSyncMove) && C.hz) {
if (0 !== C.hz.bjb && 0 !== C.hz.biZ) {
var X = Math.atan2(C.hz.bjb, C.hz.biZ);
$.bsc(180 * X / Math.PI);
}
} else $.bsc(i.Hy);
$.bsd(i.Hx);
var U = i.Hu, q = u.default.bEC.bFu(i.Hv, i.Hw);
if (!i.HM || !E) {
if (O.lX.mx) if (L) B = .32; else {
var G = C.hz, Y = G ? G.biZ * G.biZ + G.bja * G.bja : 0;
B = .2 + Math.min(Y / O.lW.ix, .4);
} else B = L ? .5 : 1;
if (B < 1) {
U = cc.misc.lerp(j.x, U, B);
q = cc.misc.lerp(j.y, q, B);
}
}
j.x !== U && (j.x = U);
j.y !== q && (j.y = q);
if (i.HC === s.$uo.TOWN_SCENE_DEFINE.ROLE_ZORDER) {
if (!r.HeatIsolate.cwo) {
var W = u.default.bEC.bFF(i.Hv + I.bgf);
j.zIndex !== W && (j.zIndex = W);
}
} else j.zIndex !== i.HC && (j.zIndex = i.HC);
var K = i.HV;
if (K) switch (K.IJ) {
case 1:
var Q = $.node.x, V = $.node.y, z = Q - K.IL.x, J = V - K.IL.y;
if (z * z + J * J > K.IM * K.IM) {
K.IL.x = Q;
K.IL.y = V;
var Z = $.node;
O.lW.jK($.mainNode, Z.x, Z.y, Z.zIndex, K);
}
break;

case 2:
K.IK += t;
if (K.IK > K.IM) {
K.IK -= K.IM;
Z = $.node;
O.lW.jK($.mainNode, Z.x, Z.y, Z.zIndex, K);
}
}
var tt = i.HE && !P ? 255 : 0;
j.opacity !== tt && (j.opacity = tt);
if ($.bsr() && (null == (H = $.bsM()) ? void 0 : H.bqp) && I.bgh > 0 && I.bgh !== i.Ht) {
i.Ht = I.bgh;
H.bqp(I.bgg.map(function(t) {
return t.modelId;
}), 1e3);
H.bqr();
}
if (M) {
$.bsj(M.bes > 0);
var et = 0;
M.bez && (M.ber > 0 ? et = 2 : M.beq > 0 && (et = 3));
$.bso(et);
R.clear();
if (T) for (var it = 0, ot = T.bhs; it < ot.length; it++) {
var nt = ot[it];
if (nt.Dq.vfx) for (var rt = 0, st = nt.Dq.vfx; rt < st.length; rt++) {
var at = st[rt];
R.add(at);
}
}
i.HG.forEach(function(t, e) {
if (R.has(e)) R.delete(e); else {
var o = i.HG.get(e);
if (o.Hi) {
i.Hn.mZ.bDO(o.Hh, o.Hi.node);
o.Hi = null;
}
i.HG.delete(o.Hb);
}
});
R.forEach(function(t) {
var o, n, r = i.HG.get(t);
if (!r || !r.Hi) {
var s = u.default.bEC.bFl(t);
if (s && (!s.visOnlySelf || v.lW.iU(e))) {
if (!r) {
(r = new a.$gv()).Hb = s.id;
r.Hl = null !== (o = s.sx) && void 0 !== o ? o : 1;
r.Hm = null !== (n = s.sy) && void 0 !== n ? n : 1;
s.sxEx && (r.Hj = w.default.CW(s.sxEx, e, null));
s.syEx && (r.Hk = w.default.CW(s.syEx, e, null));
i.HG.set(r.Hb, r);
}
$.bsi(r, s, s.ignoreDir ? $.node : $.dirNode, !0);
}
}
});
}
i.HL && i.Ii(t);
if (e.hD && e.hv) O.bpa && O.bpa(e, i, m); else {
var ct = e.ho(_.default);
if (i.HI) if (ct) {
if (!i.HP && (Z = O.lW.ip.mZ.bDE(b.$fe.ResUrl.EntityShadow))) {
i.HP = Z.getComponent(p.default);
O.lW._in.sceneLayer.addChild(i.HP.node, s.$uo.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
i.HP.fCi(2, ct.bks);
}
i.HP.fCh !== ct.bks && i.HP.fCi(2, ct.bks);
i.HP.node.setPosition(i.HI.node.x + ct.bkt * i.Hx, u.default.bEC.bFu(i.Hv, 0));
} else i.HP && i.Ik();
}
i.HF.forEach(function(t) {
if (t.Hc.length) {
for (var i = 0, o = 0; o < t.Hc.length; o++) {
var n = t.Hc[o];
n.visOnlySelf && !v.lW.iU(e) || $.bsi(t, n, n.ignoreDir ? $.node : $.dirNode, n.syncMainTime) || (t.Hc[i++] = n);
}
t.Hc.length = i;
}
});
if (T) {
R.clear();
var lt = T.bhN;
if (lt.length > 0) for (var ht = 0; ht < lt.length; ht++) {
var ut = lt[ht];
R.add(ut.id);
var bt = i.HH.get(ut.id);
if (!bt) {
(bt = new a.$gv()).Hb = ut.id;
i.HH.set(bt.Hb, bt);
}
if (!bt.Hi) {
$.bsi(bt, ut.vfxInfo, y.sceneLayer, !1);
if (bt.Hi) if (null === (o = ut.vfxInfo.warningArea) || void 0 === o ? void 0 : o.lockLine) {
var pt = bt.Hi;
pt && pt.buF && pt.buF(C);
} else {
bt.Hi.node.removeComponent(f.default);
bt.Hi.node.addComponent(f.default).buf(null !== (n = ut.vfxInfo.x) && void 0 !== n ? n : 0, null !== (l = ut.vfxInfo.y) && void 0 !== l ? l : 0, null !== (h = ut.vfxInfo.sx) && void 0 !== h ? h : 1, null !== (d = ut.vfxInfo.sy) && void 0 !== d ? d : 1);
}
}
}
i.HH.forEach(function(t, e) {
if (!R.has(e)) {
var o = i.HH.get(e);
if (o.Hi) {
i.Hn.mZ.bDO(o.Hh, o.Hi.node);
o.Hi = null;
}
i.HH.delete(o.Hb);
}
});
if (T.bhP.type > 0) {
if (!i.HU && (Z = O.lW.ip.mZ.bDE(b.$fe.ResUrl.BattleCastBar))) {
i.HU = Z.getComponent(D.default);
O.lW._in.sceneLayer.addChild(i.HU.node, s.$uo.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 3);
}
if (i.HU) {
i.HU.bpw(T.bhP.type, T.bhP.time, T.bhP.speed, x);
i.HU.node.setPosition($.node.x, $.node.y + i.Ig());
}
} else i.HU && i.HU.bpy();
}
}, O = this, T = 0, I = $; T < I.length; T++) C(I[T]);
};
e.prototype.mb = function(t) {
this.lW.iq.forEach(function(e) {
e.HF.forEach(function(e) {
e.Hd > t && (e.He = 0);
});
});
};
e.prototype.mc = function() {
this.lW.iq.forEach(function(t) {
t.HI;
t.HF.forEach(function(e) {
0 === e.He && t.Ic(e);
});
});
};
e.prototype.md = function() {};
e.prototype.bpc = function(t, e) {
var i = this.lW.iM(t), o = i.HF.get(e);
o && i.Ic(o);
};
e.prototype.bpd = function(t, e) {
var i = this.lW.iM(t), o = i.HH.get(e);
o && i.Ic(o);
};
e.prototype.bpf = function(t, e) {
var i, o;
if (0 !== e.mount) {
var n = this.lW.iM(t);
if (n && (!this.lX.mC || !n.HN)) {
var r = n.HF.get(e.id);
if (e.destroy) r && n.Ic(r); else if (e.ignoreTeamVfxSwitch || !$fc.localMgr.cjU(h.$oD.i) || !this.lW.iV(t)) {
if (r) this.lX.mU || r.Hc.push(e); else {
(r = new a.$gv()).Hb = e.id || this.bpe--;
r.Hc.length = 0;
n.HF.set(r.Hb, r);
r.Hc.push(e);
}
r.Hl = null !== (i = e.sx) && void 0 !== i ? i : 1;
r.Hm = null !== (o = e.sy) && void 0 !== o ? o : 1;
e.sxEx && (r.Hj = w.default.CW(e.sxEx, t, null));
e.syEx && (r.Hk = w.default.CW(e.syEx, t, null));
r.Hd = this.lX.ne.fi;
r.He = 1;
}
}
}
};
e.prototype.bpg = function(t, e) {
if (0 !== e.mount) {
var i = this.lW.iM(t);
if (i) {
var o = i.HF.get(e.id);
if (e.destroy) o && i.Ic(o); else {
if (!o) {
(o = new a.$gv()).Hb = e.id || this.bpe--;
o.Hc.length = 0;
i.HF.set(o.Hb, o);
o.Hc.push(e);
}
o.Hd = this.lX.nf.fi;
o.He = 1;
}
}
}
};
e.prototype.bph = function(t, e) {
if (!this.lX.mT && !this.lX.mU) {
var i = this.lW.iN(null == t ? void 0 : t.hg);
i && (this.lX.mC && i.HN || this.bpl(i, e));
}
};
e.prototype.bpi = function(t, e) {
var i = this.lW.iM(t);
i && i.HI && i.HI.bsx(e.time, e.val);
};
e.prototype.bpj = function(t, e) {
var i = this.lW.iM(t);
i && i.HI && i.HS.set(e.effId, e);
};
e.prototype.bpk = function(t, e) {
if (!this.lX.mT) {
var i = this.lW.iN(null == t ? void 0 : t.hg);
i && (null == i ? void 0 : i.HN) && this.bpl(i, e);
}
};
e.prototype.bpl = function(t, e) {
var i, o, n, r, s, c, l, h, u, b;
if (e) {
var p = t.HV;
p || (p = t.HV = new a.default());
if (p.II !== e) {
p.II = e;
var f = e.mode || 1;
p.IJ = f;
p.IN.r = null !== (i = e.r) && void 0 !== i ? i : 255;
p.IN.g = null !== (o = e.g) && void 0 !== o ? o : 255;
p.IN.b = null !== (n = e.b) && void 0 !== n ? n : 255;
p.IO = null !== (r = e.a) && void 0 !== r ? r : 200;
p.IP = null !== (s = e.size) && void 0 !== s ? s : 660;
p.IR = 280;
p.IS = null !== (c = e.shaderType) && void 0 !== c ? c : 0;
p.IT = null !== (l = e.blend) && void 0 !== l ? l : 0;
p.IU = null !== (h = e.offsetZIndex) && void 0 !== h ? h : 0;
p.IV = null !== (u = e.t1) && void 0 !== u ? u : .05;
p.IW = null !== (b = e.t2) && void 0 !== b ? b : .12;
switch (f) {
case 1:
p.IL.x = 999999;
p.IL.y = 999999;
p.IM = e.param || 130;
break;

case 2:
var d = e.param || .07;
p.IK = d + 1e-6;
p.IM = d;
}
}
} else t.HV && (t.HV = null);
};
e.prototype.bpm = function(t, e) {
var i = this.lW.iM(t);
i && i.HI && i.HI.bsw(e.name, e.visible);
};
e.prototype.bpn = function(t, e) {
this.lW.iM(t).HE = e.visible;
};
e.prototype.bpo = function() {
var t;
null === (t = this.lW) || void 0 === t || t.iq.forEach(function(t) {
if (t.HI) {
t.HI.bsC = $fc.localMgr.cjU(h.$oD.h);
t.HI.bsH = $fc.localMgr.cjU(h.$oD.g);
}
});
};
e.prototype.bpp = function(t, e) {
var i, o = this, n = t.hg, r = null === (i = this.lW) || void 0 === i ? void 0 : i.iN(t.hg);
if (r && t.hE) {
var s = t.hw, c = t.hE.bhA[e.varX] || 0, l = t.hE.bhA[e.varY] || 0, h = c, b = u.default.bEC.bFt(l) + e.offsetY, p = s.x, f = u.default.bEC.bFu(s.y, s.z) + e.offsetY, d = Math.floor((b + s.y) / 2), g = p - h, m = f - b;
Math.abs(g) < 5 && (g = p - (h = s.x + -20 * s.bkw));
var _ = e.extendLength || 0, $ = cc.v2(g, m).negSelf().normalizeSelf().mul(.5 * _), w = Math.sqrt(g * g + m * m) + _, D = 180 * Math.atan2(m, g) / Math.PI, P = Math.abs(D) > 90 ? -1 : 1;
-1 == P && (D = Math.sign(D) * (180 - Math.abs(D)));
var R = e.height || 20;
e.heightRandAdd && (R += Math.random() * e.heightRandAdd);
var S = u.default.bEC.bFG(v.DISPLAY_RES_TYPE.MOVIE_CLIP), E = this.lX.mZ.bDE(S);
if (E) {
var C = new a.$gv();
C.Hb = this.bpe--;
C.Hc.length = 0;
r.HF.set(C.Hb, C);
E.x = h + $.x;
E.y = b + $.y;
E.scaleX = 1 * P;
E.scaleY = 1;
E.angle = 0;
var O = C.Hi = E.getComponent(y.default), T = (this.lX.mF ? 0 : 1) * t.hy.worldTimeScale, I = u.default.bEC.bFF(d - 2);
this.lW._in.sceneLayer.addChild(E, I);
O.sprite.spriteFrame = null;
O.sprite.sizeMode = cc.Sprite.SizeMode.CUSTOM;
O.sprite.node.angle = D;
O.sprite.node.width = w;
O.sprite.node.height = R;
O.setTimeScale(T);
O.btN(e.resId, !1, function() {
var t, e;
if (O.isValid) {
O.sprite.sizeMode = cc.Sprite.SizeMode.RAW;
O.sprite.node.angle = 0;
var i = null === (t = o.lW) || void 0 === t ? void 0 : t.iN(n);
if (i) i.Ic(C); else {
C.Hi = null;
null === (e = O.node) || void 0 === e || e.destroy();
}
}
});
}
}
};
e.prototype.bpq = function() {
var t;
null === (t = this.lW) || void 0 === t || t.iq.forEach(function(t) {
t.HK && t.HL && t.HJ && (t.HK.f = 1);
});
};
e.prototype.bpr = function(t, e) {
var i, o = null === (i = this.lW) || void 0 === i ? void 0 : i.iN(t.hg);
(null == o ? void 0 : o.HI) && o.HI.bsQ($.$Qp.c, e[0]);
};
e.prototype.bps = function(t) {
var e, i = null === (e = this.lW) || void 0 === e ? void 0 : e.iN(t.hg);
i && this.lW.iO(i);
};
return e;
}(a.$gt);
i.$kf = S;
cc._RF.pop();
}, {
"../$KQ": "$KQ",
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/$KU": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../../game/src/manager/$DUP": void 0,
"../../../game/src/perf/$DVH": void 0,
"../../../game/src/perfab/common/$FDM": void 0,
"../../../game/src/perfab/common/$FDW": void 0,
"../../../game/src/perfab/town/$FEB": void 0,
"../../battleLogic/$KI": void 0,
"../../battleLogic/component/$DPM": void 0,
"../render/$DQR": "$DQR",
"../render/$DRB": "$DRB",
"../render/$DRG": "$DRG",
"../render/$DRH": "$DRH",
"../render/$DRI": "$DRI",
"../render/$DRL": "$DRL"
} ],
$DQR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e0d0cLHNp5N16d7JJ2Welcf", "$DQR");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.skeleton = null;
e.bpt = 0;
e.bpu = 1;
e.bpv = 0;
return e;
}
e.prototype.kq = function() {
this.skeleton.setCompleteListener(this.bpx.bind(this));
};
e.prototype.bpw = function(t, e, i, o) {
if (0 !== t) {
if (this.bpt !== t) {
this.bpv = e;
this.bpt = t;
switch (t) {
case 1:
this.skeleton.setAnimation(0, "laoding", !1);
break;

case 2:
this.skeleton.setAnimation(0, "laoding3", !1);
}
this.bpu = Math.min(Math.max(1 / e, .01), 100);
}
this.node.active = this.bpt > 0;
this.skeleton.timeScale = i * o * this.bpu;
}
};
e.prototype.bpx = function() {
2 === this.bpt && this.skeleton.setAnimation(0, "laoding4", !0);
};
e.prototype.bpy = function() {
if (0 !== this.bpt) {
this.bpt = 0;
this.node.active = !1;
}
};
e.prototype.reuseSelf = function() {
this.bpt = 0;
};
r([ l(sp.Skeleton) ], e.prototype, "skeleton", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DQS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "01e74SrYoxFK4IVa8EMFfbG", "$DQS");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("../ui/$DRO"), c = cc._decorator, l = c.ccclass, h = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.avatar = null;
e.curHpProgressBar = null;
e.lblHp = null;
e.buffList = null;
e.buffTemplate = null;
e.lblName = null;
e.lblLevel = null;
e.networkStatusNodeList = [];
e.bpz = new a.$kR();
e.bpA = "";
return e;
}
e.prototype.kq = function() {
this.bpB();
};
e.prototype.bpB = function() {
this.bpz.buY || this.bpz.buX(this.buffList ? this.buffList.node : null, this.buffTemplate);
};
e.prototype.bpC = function(t) {
this.bpB();
this.bpE(t.c);
this.bpF(t.d, t.e);
this.bpz.buZ(t.f || []);
this.bpD(t.b);
this.lblLevel && (this.lblLevel.string = t.g.toString());
this.lblName && (this.lblName.string = t.h);
};
e.prototype.bpD = function(t) {
this.networkStatusNodeList && this.networkStatusNodeList.forEach(function(e, i) {
e.active = i === t;
});
};
e.prototype.bpE = function(t) {
if (this.avatar && t && this.bpA !== t) {
this.bpA = t;
this.lC(this.avatar, t);
}
};
e.prototype.bpF = function(t, e) {
var i = e > 0 ? cc.misc.clampf(t / e, 0, 1) : 0;
this.curHpProgressBar && (this.curHpProgressBar.progress = i);
this.lblHp && (this.lblHp.string = Math.round(100 * i) + "%");
};
r([ h(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ h(cc.ProgressBar) ], e.prototype, "curHpProgressBar", void 0);
r([ h(cc.Label) ], e.prototype, "lblHp", void 0);
r([ h(cc.Layout) ], e.prototype, "buffList", void 0);
r([ h(cc.Node) ], e.prototype, "buffTemplate", void 0);
r([ h(cc.Label) ], e.prototype, "lblName", void 0);
r([ h(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ h([ cc.Node ]) ], e.prototype, "networkStatusNodeList", void 0);
return r([ l ], e);
}(s.$fe.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../ui/$DRO": "$DRO"
} ],
$DQT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "bc315BXFNZPo58wcXXAqKDU", "$DQT");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.avatar = null;
e.lblName = null;
e.lblLevel = null;
e.damageProgressBar = null;
e.lblDamagePercent = null;
e.lblDamageValue = null;
e.bpG = "";
e.networkStatusNodeList = [];
return e;
}
e.prototype.bpH = function(t) {
this.bpI(t.b);
this.lblName && (this.lblName.string = t.c || "");
this.bpJ(t.f);
var e = cc.misc.clampf(t.e, 0, 1);
this.damageProgressBar && (this.damageProgressBar.progress = e);
this.lblDamagePercent && (this.lblDamagePercent.string = (100 * e).toFixed(1) + "%");
this.lblDamageValue && (this.lblDamageValue.string = s.$fe.utils.bcO(Math.max(0, Math.floor(t.d))));
this.lblLevel && (this.lblLevel.string = t.g.toString());
};
e.prototype.bpI = function(t) {
if (this.avatar && t && this.bpG !== t) {
this.bpG = t;
this.lC(this.avatar, t);
}
};
e.prototype.bpJ = function(t) {
this.networkStatusNodeList && this.networkStatusNodeList.forEach(function(e, i) {
e.active = i === t;
});
};
r([ l(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ l(cc.Label) ], e.prototype, "lblName", void 0);
r([ l(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ l(cc.ProgressBar) ], e.prototype, "damageProgressBar", void 0);
r([ l(cc.Label) ], e.prototype, "lblDamagePercent", void 0);
r([ l(cc.Label) ], e.prototype, "lblDamageValue", void 0);
r([ l([ cc.Node ]) ], e.prototype, "networkStatusNodeList", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DQU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "28e39LvYH1Ca4nQ0pjQWDC1", "$DQU");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kl = void 0;
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/battle/$DSO"), h = t("../../../game/src/perfab/common/$FDO"), u = t("../../../game/src/perfab/common/$FDU"), b = t("../../../main/src/$KV"), p = t("../../../game/src/utils/$DWA"), f = t("../../../game/src/utils/$DWB"), d = t("./$DRK"), v = t("../../../game/src/perf/$DVH"), y = cc._decorator, g = y.ccclass, m = (y.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = b.cvResUrl.changeableSpine;
e.bpL = [];
e.bpM = new Map();
e.bpN = !1;
e.bpO = null;
e.bpP = 0;
e.bpQ = 0;
e.bpR = !1;
e.bpS = [];
e.bpT = new Map();
e.bpU = 1;
e.bqm = [];
e.bqn = -1;
e.bqo = new Map();
e.bqF = null;
return e;
}
e.prototype.onLoad = function() {
this.bpU = this.sp.node.scale;
};
e.prototype.kw = function() {
t.prototype.kw.call(this);
this.bpT.clear();
this.kE();
};
e.prototype.kE = function() {
this.bpV();
this.bpK && this.bpK.isValid && this.bpK.destroy();
this.sp && f.$sf.cEO(this.sp);
this.bpT.clear();
this.bpK = null;
this.bpM.clear();
this.bqn = -1;
this.bqm.length = 0;
this.bqo.clear();
this.bpR = !1;
this.bpS.length = 0;
this.bqE();
};
e.prototype.bpV = function() {
this.bpM.forEach(function(t) {
u.$Qf.fGp(t.k);
});
};
e.prototype.reuseSelf = function() {
t.prototype.reuseSelf.call(this);
this.kE();
};
e.prototype.kF = function(t) {
this.bpR && this.bpW();
if (0 !== this.bpS.length) for (var e = 0, i = this.bpS; e < i.length; e++) {
var o = i[e], n = o.b;
if (u.$Qf.fGt(n, t)) if (n.b === u.$Qk.e) this.bqg(o.c, !1); else for (var r = n.c[n.e], s = 0, a = o.c; s < a.length; s++) {
var c = a[s];
this.bqd(c, r);
}
}
};
e.prototype.bpW = function() {
var t = this;
this.bpR = !1;
this.bpS.length = 0;
var e = new Map();
this.bpM.forEach(function(i) {
var o = i.k;
if (u.$Qf.fGo(o)) {
var n = e.get(o);
if (!n) {
var r = {
b: o,
c: []
};
e.set(o, r);
t.bpS.push(r);
n = r;
}
n.c.push(i);
}
});
};
e.prototype.bpX = function() {
var t = [];
this.bpM.forEach(function(e) {
t.push(e);
});
return t;
};
e.prototype.bpY = function(t) {
var e = [];
if (!t) return e;
this.bpM.forEach(function(i) {
i.k === t && e.push(i);
});
return e;
};
e.prototype.bpZ = function(t) {
var e = t.k;
t.k = u.$Qf.fGm();
t.j = null;
e && (this.bpY(e).length > 0 || u.$Qf.fGp(e));
};
e.prototype.bqa = function(t) {
var e = this.sp.animation, i = this.sp.loop, o = new sp.SkeletonData();
this.bpK = o;
t.nativeUrl;
cc.js.mixin(o, t);
o._uuid = t._uuid + "_" + Date.now() % 1e5 + Math.floor(1e4 * Math.random()) + "_copy";
var n = o.name + "_copy";
o.name = n;
o.atlasText = o.atlasText.replace(".png", "_copy.png");
var r = o.atlasText.match(/\w+\.png/);
o.textureNames = [ r[0] ];
o._ref = 0;
var s = o.textures;
s && s.length && (o.textures = s.slice());
this.bqC() && s && s[0] && this.bqD(s[0]) && (o.textures[0] = this.bpO);
p.$se.cED(this.sp, p.$se.cEy);
this.sp.skeletonData = o;
this.bpK.addRef();
this.sp.animation = e;
this.sp.loop = i;
};
e.prototype.mq = function(t) {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF1(){this.bqa(t);
this.bqr();
return [ 2 ];
});
});
};
e.prototype.bqb = function(t, e) {
if (this.sp) return this.sp.getAttachment(t, e);
};
e.prototype.bqc = function(t, e) {
if (t && e && t.e !== e) {
t.e = e;
this.bqd(t, e);
}
};
e.prototype.bqd = function(t, e, i) {
void 0 === i && (i = !1);
if (this.sp && e) {
var o = t.k, n = t.j;
if (i || n !== e) {
var r = e.getTexture();
if (r) {
r.packable = !1;
t.j = e;
var s = t.b;
cc.sys.isNative ? this.bqJ(t, e, s.slot, s.attachment) : u.$Qf.fGn(o) ? o.b === u.$Qk.c ? this.bqf(t, e) : this.bqi(t, e) : this.bpN ? this.bqj(t, e) : this.bqe(t, e, n) || this.bqi(t, e);
}
}
}
};
e.prototype.bqe = function(t, e, i) {
var o = t.c;
if (!o || !o.texture) return !1;
if (o === t.d) return !1;
if (!i) return !1;
if (i.isRotated() !== e.isRotated()) return !1;
var n = i.getRect(), r = e.getRect();
if (n.width !== r.width || n.height !== r.height) return !1;
var s = i.getOriginalSize(), a = e.getOriginalSize();
if (s.width !== a.width || s.height !== a.height) return !1;
var c = i.uv, l = e.uv;
if (!c || !l || c.length !== l.length) return !1;
for (var h = 0; h < l.length; h++) if (c[h] !== l[h]) return !1;
var u = this.bqb(t.b.slot, t.b.attachment);
if (!u) return !1;
u.width = o.width;
u.height = o.height;
o.texture.setRealTexture(e.getTexture());
return !0;
};
e.prototype.bqf = function(t, e) {
var i = t.b, o = this.bqb(i.slot, i.attachment);
if (o) {
var n = e.getTexture();
if (n) {
var r = t.k, s = t.c;
if (!s || s === t.d) {
(s = new sp.spine.TextureAtlasRegion()).renderObject = s;
t.c = s;
r.w = null;
}
if (!r.w || r.x !== n) {
var a = new sp.SkeletonTexture({
width: n.width,
height: n.height
});
a.setRealTexture(n);
r.w = a;
r.x = n;
}
s.texture = r.w;
var c = e.getRect(), l = e.getOriginalSize(), h = e.getOffset(), u = e.isRotated(), b = e.uv;
if (b && b.length >= 8) if (u) {
s.u = b[4];
s.v = b[3];
s.u2 = b[2];
s.v2 = b[5];
} else {
s.u = b[0];
s.v = b[7];
s.u2 = b[6];
s.v2 = b[1];
}
s.rotate = u;
s.width = c.width;
s.height = c.height;
s.originalWidth = l.width;
s.originalHeight = l.height;
s.offsetX = (l.width - c.width) / 2 + h.x + (i.offsetX || 0);
s.offsetY = (l.height - c.height) / 2 + h.y + (i.offsetY || 0);
if (o instanceof sp.spine.MeshAttachment) {
o.region = s;
o.updateUVs();
} else {
o.width = l.width * (i.scaleX || 1);
o.height = l.height * (i.scaleY || 1);
o.setRegion(s);
o.updateOffset();
}
}
} else $fc.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bqg = function(t, e) {
var i = this;
if (t.length) {
var o = t[0].k, n = o.c[o.e];
if (n) if (!e || u.$Qc.fFL()) {
o.u = 0;
o.p = o.e;
o.q = o.j;
u.$Qe.fGf(o, n, function(t) {
if (i.isValid && i.sp && o.b === u.$Qk.e) {
var e = i.bpY(o);
if (e.length) if (t) for (var n = 0, r = e; n < r.length; n++) {
var s = r[n];
i.bqd(s, o.o);
} else i.bqh(e);
}
}) || this.bqh(t);
} else {
o.u += 1;
o.u >= u.$Qf.fGj && this.bqh(t);
}
}
};
e.prototype.bqh = function(t) {
if (t.length) {
var e = t[0].k;
u.$Qf.fGq(e);
e.h = null;
e.b = u.$Qk.c;
this.bpR = !0;
for (var i = 0, o = t; i < o.length; i++) {
var n = o[i];
this.bqd(n, e.c[e.e], !0);
}
}
};
e.prototype.bqi = function(t, e) {
var i = t.b, o = this.bqb(i.slot, i.attachment);
if (o) {
var n = e.getTexture();
if (n) {
var r = e.getOriginalSize();
e.isRotated() || r.width !== n.width || r.height !== n.height || (e = new cc.SpriteFrame(n));
var s = t.c;
if (!s || s === t.d) {
s = new sp.spine.TextureAtlasRegion();
t.c = s;
}
var a = e.isRotated(), c = a ? r.height : r.width, l = a ? r.width : r.height, h = e.uv;
if (h && h.length >= 8) if (a) {
s.u = h[4];
s.v = h[3];
s.u2 = h[2];
s.v2 = h[5];
} else {
s.u = h[0];
s.v = h[7];
s.u2 = h[6];
s.v2 = h[1];
} else {
var u = e.getRect();
s.u = u.x / n.width;
s.u2 = (u.x + u.width) / n.width;
s.v = (n.height - u.y - u.height) / n.height;
s.v2 = (n.height - u.y) / n.height;
}
s.width = c * (i.scaleX || 1);
s.height = l * (i.scaleY || 1);
s.originalWidth = c;
s.originalHeight = l;
s.rotate = a;
s.offsetX = i.offsetX || 0;
s.offsetY = i.offsetY || 0;
s.renderObject = s;
var b = new sp.SkeletonTexture({
width: n.width,
height: n.height
});
b.setRealTexture(n);
s.texture = b;
if (o instanceof sp.spine.MeshAttachment) {
o.region = s;
o.updateUVs();
} else {
var p = e.getOriginalSize();
o.width = p.width * (i.scaleX || 1);
o.height = p.height * (i.scaleY || 1);
o.setRegion(s);
o.updateOffset();
}
return s;
}
} else $fc.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bqj = function(t, e) {
var i = t.b, o = this.bqb(i.slot, i.attachment);
if (o) {
if (!t.d) {
var n = o.region;
n && n.page && (t.d = n);
}
var r = t.d;
if (r) if (r.rotate) this.bqi(t, e); else {
var s = r.x, a = r.y, c = r.originalWidth, l = r.originalHeight, h = e.getOriginalSize();
if (h.width * h.height / (c * l) > 2) this.bqi(t, e); else {
if (null == s || null == a) {
s = Math.round(r.u * this.bpP);
a = Math.round(r.v * this.bpQ);
c = Math.round((r.u2 - r.u) * this.bpP);
l = Math.round((r.v2 - r.v) * this.bpQ);
}
var u = a, b = e.getOriginalSize();
o.width = b.width;
o.height = b.height;
if (this.bqI(e, s, u, c, l)) {
if (o.region !== r) if (o instanceof sp.spine.MeshAttachment) {
o.region = r;
o.updateUVs();
} else {
o.setRegion(r);
o.updateOffset();
} else o instanceof sp.spine.MeshAttachment ? o.updateUVs() : o.updateOffset();
t.c = r;
} else {
cc.log("尺寸不匹配:", i.url, i.key || "", "设计尺寸：", c, l, "实际尺寸：", h.width, h.height);
this.bqi(t, e);
}
}
} else this.bqi(t, e);
} else $fc.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bqk = function(t, e) {
var i = this.sp.findBone(t);
if (i) if (cc.sys.isNative) {
i.scaleX = e ? 1 : 0;
i.scaleY = e ? 1 : 0;
i.setUserActive && i.setUserActive(e);
} else {
i.active = e;
i.scaleX = e ? 1 : 0;
i.scaleY = e ? 1 : 0;
}
};
e.prototype.bql = function(t, e) {
var i = this.sp.findSlot(t);
if (i) if (cc.sys.isNative) {
e ? this.bpT.delete(t) : this.bpT.has(t) || (s = i.getAttachment()) && this.bpT.set(t, s);
i.setColorA && i.setColorA(e ? 1 : 0);
} else {
if (e) {
var o = this.bpT.get(t);
if (o) {
i.setAttachment(o);
this.bpT.delete(t);
} else if (!i.getAttachment()) {
var n = i.data && i.data.attachmentName;
if (n) {
var r = this.sp.getAttachment(t, n);
r && i.setAttachment(r);
}
}
} else {
if (this.bpT.has(t)) return;
var s;
(s = i.getAttachment()) && this.bpT.set(t, s);
i.setAttachment(null);
}
i.color.a = e ? 1 : 0;
}
};
e.prototype.bqp = function(t, e) {
this.bqm = this.bqm.filter(function(t) {
return t.c !== e;
});
for (var i = 0, o = t; i < o.length; i++) {
var n = {
b: o[i],
c: e
};
this.bqm.push(n);
}
};
e.prototype.bqq = function() {
this.sp && (f.$sf.cEG ? f.$sf.cEN(this.sp, this.bpT) : f.$sf.cEO(this.sp));
};
e.prototype.bqr = function() {
for (var t, e, i = this, o = [], n = function(e) {
var i = l.default.bEC.bFh(e.b);
if (!i) return "continue";
var n = null !== (t = i.type) && void 0 !== t ? t : i.id, r = o.find(function(t) {
return t.type === n;
});
r ? r.data.c < e.c && (r.data = e) : o.push({
data: e,
type: n
});
}, r = 0, h = this.bqm; r < h.length; r++) n(d = h[r]);
for (var b = [], p = 0, f = o; p < f.length; p++) {
var d = f[p].data;
(m = l.default.bEC.bFh(d.b)) && b.push.apply(b, m.hideType);
}
for (var v = 0, y = o; v < y.length; v++) {
var g = y[v];
if (-1 === b.indexOf(g.type)) {
var m;
d = g.data;
if (m = l.default.bEC.bFh(d.b)) {
if (m.boneVis) for (var _ = 0, $ = m.boneVis; _ < $.length; _++) {
var w = $[_];
this.bqk(w.name, w.visible);
}
if (m.slotVis) for (var D = 0, P = m.slotVis; D < P.length; D++) {
w = P[D];
this.bql(w.name, w.visible);
}
for (var R = 0, S = m.skins; R < S.length; R++) {
var E = S[R], C = E.slot + "_" + E.attachment, O = this.bpM.get(C);
if (!O) {
O = {
h: 0,
b: E,
c: null,
e: null,
f: "",
g: d.b,
i: null,
j: null,
k: u.$Qf.fGm()
};
this.bpM.set(C, O);
}
O.h = null !== (e = m.type) && void 0 !== e ? e : 0;
var T = u.$Qf.fGl(O.b.movieClipId), I = u.$Qf.fGl(E.movieClipId);
if (O.b.url !== E.url || O.b.key !== E.key || T !== I) {
O.b = E;
O.i = null;
O.e = null;
this.bpZ(O);
this.bpR = !0;
} else O.b = E;
}
}
}
}
this.bqq();
u.$Qg.fGz(this.bpX());
this.bpR = !0;
var M = new Map();
this.bpM.forEach(function(t) {
var e = t.b;
u.$Qf.fGl(e.movieClipId) || (e.key ? M.set(e.url, new c.$fe.ResCfg(e.url, cc.SpriteAtlas)) : M.set(e.url, new c.$fe.ResCfg(e.url, cc.SpriteFrame)));
});
var x = 1, k = function() {
(x -= 1) > 0 || i.isValid && i.sp && i.sp.node && (i.sp.node.scale = i.bpU);
}, H = [];
this.bpM.forEach(function(t) {
u.$Qf.fGl(t.b.movieClipId) && -1 === H.indexOf(t.k) && H.push(t.k);
});
for (var N = 0, j = H; N < j.length; N++) {
var F = j[N];
x += 1;
this.bqs(F, k);
}
this.sp.node.scale = this.bpM.size > 0 ? 0 : this.bpU;
this.bqu(M, function(t) {
i.isValid && i.bpM.forEach(function(e) {
return s(i, void 0, void 0, function() {
var i, o, n, r, s;
return a(this,function AWAITF2(){i = e.b;
o = this.bqv(i);
e.f = o;
if (u.$Qf.fGl(i.movieClipId)) return [ 2 ];
if (n = t.get(i.url)) {
r = null;
if (i.key) {
s = n.CM;
r = s.getSpriteFrame(i.key);
} else r = n.CM;
if (r) {
this.bqc(e, r);
this.bqy(e);
}
}
return [ 2 ];
});
});
});
k();
});
};
e.prototype.bqs = function(t, e) {
var i = this, o = this.bpY(t);
if (o.length) {
var n = o[0].b, r = u.$Qf.fGl(n.movieClipId);
if (r) {
n.url;
if (t.A === r && t.c.length > 0) {
var s = o[0].h ? this.bqo.get(o[0].h) : null;
this.bqt(o, s);
e();
} else u.$Qf.fGr(t, r, function() {
return i.isValid;
}, function(o) {
if (o && i.isValid && i.sp) {
var n = i.bpY(t);
if (n.length) {
for (var r = 0, s = n; r < s.length; r++) s[r].e = t.c[0];
var a = n[0].h ? i.bqo.get(n[0].h) : null;
i.bqt(n, a);
e();
} else e();
} else e();
});
} else e();
} else e();
};
e.prototype.bqt = function(t, e) {
if (t.length) {
var i = t[0].k;
this.bpR = !0;
if (e || i.v || cc.sys.isNative) {
for (var o = 0, n = t; o < n.length; o++) (l = n[o]).i = e;
if (u.$Qf.fGs(i, e)) {
this.bqg(t, !1);
return;
}
$fc.logMgr.warn("[ChangeableSpine] 合成目标建立失败，部位降级为逐帧 UV，slot=" + t[0].b.slot);
} else for (var r = 0, s = t; r < s.length; r++) (l = s[r]).i = null;
i.h = null;
i.b = u.$Qk.c;
for (var a = 0, c = t; a < c.length; a++) {
var l = c[a];
this.bqd(l, i.c[i.e], !0);
}
}
};
e.prototype.bqu = function(t, e) {
var i = this, o = 0, n = t.size, r = new Map();
v.HeatIsolate.cwp ? e(r) : 0 !== n ? t.forEach(function(t, s) {
i.lz(t, function(t, i) {
o++;
i && i.CM && r.set(s, i);
o == n && e(r);
});
}) : e(r);
};
e.prototype.bqv = function(t) {
if (!t) return "";
var e = u.$Qf.fGl(t.movieClipId);
return e ? "mc#" + e : t.url ? t.key ? t.url + "#" + t.key : t.url : "";
};
e.prototype.bqw = function(t, e) {
null === e ? this.bqo.delete(t) : this.bqo.set(t, e);
this.bqx();
};
e.prototype.bqx = function() {
var t = this;
this.bpM.forEach(function(e) {
0 !== e.h && e.e && t.bqy(e);
});
};
e.prototype.bqy = function(t) {
var e;
if (t.h && t.e) {
var i = this.bqo.get(t.h), o = t.k;
u.$Qf.fGl(t.b.movieClipId) ? u.$Qf.fGn(o) ? this.bqB(t, i) : t.i = i : i ? i === t.i && i.mode === (null === (e = o.h) || void 0 === e ? void 0 : e.mode) || this.bqz(t, i) : this.bqA(t);
}
};
e.prototype.bqz = function(t, e) {
var i = t.k;
t.i = e;
i.c = [ t.e ];
i.d = 0;
i.e = 0;
i.g = 0;
i.f = 0;
i.v = t.e.isRotated();
this.bqt([ t ], e);
};
e.prototype.bqA = function(t) {
var e = t.k;
if (e.b !== u.$Qk.b || t.i || e.h) {
u.$Qf.fGq(e);
this.bpR = !0;
e.b = u.$Qk.b;
e.c = [];
e.e = 0;
e.f = 0;
e.v = !1;
t.i = null;
this.bqd(t, t.e, !0);
}
};
e.prototype.bqB = function(t, e) {
var i;
(e !== t.i || e && e.mode !== (null === (i = t.k.h) || void 0 === i ? void 0 : i.mode)) && this.bqt(this.bpY(t.k), e);
};
e.prototype.bqC = function() {
return !cc.sys.isNative && !!h.$PJ.fCo;
};
e.prototype.bqD = function(t) {
if (!t || !t.isValid) return !1;
var e = t.width, i = t.height;
if (!e || !i) return !1;
var o = new cc.RenderTexture();
o.initWithSize(e, i);
o.packable = !1;
o.setFilters(cc.Texture2D.Filter.LINEAR, cc.Texture2D.Filter.LINEAR);
o.setWrapMode(cc.Texture2D.WrapMode.CLAMP_TO_EDGE, cc.Texture2D.WrapMode.CLAMP_TO_EDGE);
this.bpO = o;
this.bpP = e;
this.bpQ = i;
this.bpN = !0;
if (!this.bqI(new cc.SpriteFrame(t), 0, 0, e, i, !0)) {
console.log("拷贝 atlas 失败");
this.bqE();
return !1;
}
return !0;
};
e.prototype.bqE = function() {
this.bpO && this.bpO.isValid && this.bpO.destroy();
this.bpO = null;
this.bpN = !1;
this.bpP = 0;
this.bpQ = 0;
this.bqH();
};
e.prototype.bqG = function(t, e) {
void 0 === t && (t = null);
void 0 === e && (e = 2048);
if (!this.bpO || !this.bpO.isValid) {
$fc.logMgr.warn("[ChangeableSpine] showDebugAtlas: _atlasRT not available");
return null;
}
var i = t || cc.find("Canvas");
if (!this.bqF || !this.bqF.isValid) {
this.bqF = new cc.Node("DEBUG_AtlasRT");
var o = this.bqF.addComponent(cc.Sprite);
o.sizeMode = cc.Sprite.SizeMode.CUSTOM;
o.trim = !1;
}
if (this.bqF.parent !== i) {
this.bqF.removeFromParent();
i.addChild(this.bqF);
}
var n = new cc.SpriteFrame(this.bpO);
this.bqF.getComponent(cc.Sprite).spriteFrame = n;
var r = this.bpP, s = this.bpQ;
if (e > 0) {
var a = Math.min(1, e / Math.max(r, s));
r = Math.round(r * a);
s = Math.round(s * a);
}
this.bqF.setContentSize(r, s);
this.bqF.zIndex = 9999;
return this.bqF;
};
e.prototype.bqH = function() {
this.bqF && this.bqF.isValid && this.bqF.destroy();
this.bqF = null;
};
e.prototype.bqI = function(t, e, i, o, n, r) {
void 0 === r && (r = !1);
if (!this.bpO || !this.bpO.isValid) return !1;
if (!t) return !1;
var s = t.getTexture();
if (!s || !s.isValid) return !1;
var a = t.getOriginalSize(), c = t;
t.isRotated() || a.width !== s.width || a.height !== s.height || (c = new cc.SpriteFrame(s));
if (!h.$PJ.fCu()) return !1;
var l = h.$PJ.fCr, u = h.$PJ.fCs, b = h.$PJ.fCt;
if (!(l && l.isValid && b && b.isValid)) return !1;
l.orthoSize = this.bpQ / 2;
l.node.setPosition(this.bpP / 2, this.bpQ / 2);
b.spriteFrame = c;
u.setAnchorPoint(.5, .5);
u.setContentSize(o, n);
u.setPosition(e + o / 2 - this.bpP / 2, i + n / 2 - this.bpQ / 2);
u.scaleY = -1;
u.active = !0;
b.srcBlendFactor = cc.macro.BlendFactor.ONE;
b.dstBlendFactor = cc.macro.BlendFactor.ZERO;
l.clearFlags = r ? cc.Camera.ClearFlags.COLOR | cc.Camera.ClearFlags.DEPTH | cc.Camera.ClearFlags.STENCIL : 0;
l.backgroundColor = cc.color(0, 0, 0, 0);
l.targetTexture = this.bpO;
var p = !1;
try {
l.render(u);
p = !0;
} catch (t) {
p = !1;
console.log("blitToAtlas camera.render error", t);
} finally {
l.targetTexture = null;
u.scaleY = 1;
b.spriteFrame = null;
b.srcBlendFactor = cc.macro.BlendFactor.SRC_ALPHA;
b.dstBlendFactor = cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
}
return p;
};
e.prototype.bqJ = function(t, e, i, o) {
var n = this.sp.getAttachment(i, o);
if (n) {
var r = e.getTexture();
if (r) {
var s = e.getRect(), a = e.getOffset(), c = e.isRotated(), l = new cc.Rect();
l.x = e.uv[c ? 4 : 0];
l.y = e.uv[c ? 3 : 7];
l.w = e.uv[c ? 2 : 6];
l.h = e.uv[c ? 5 : 1];
var h = {
rect: s,
origSize: s,
offset: a,
degrees: c,
uv: l
};
if (n.updateUVs) {
if (!t.d) {
var u = n.region;
u && u.page && (t.d = u);
}
var b = t.d;
if (b) {
var p = cc.rect(b.originalWidth, b.originalHeight);
h.origSize = p;
h.offset = cc.v2((p.width - s.width) / 2 + a.x, (p.height - s.height) / 2 + a.y);
}
}
s.w = s.width;
s.h = s.height;
if (!n.updateUVs) {
var f = t.b;
n.width = s.width * (f.scaleX || 1);
n.height = s.height * (f.scaleY || 1);
}
var d = t.k, v = d && d.z === r ? d.y : null;
if (!v) {
(v = new middleware.Texture2D()).setPixelsHigh(r.height);
v.setPixelsWide(r.width);
v.setNativeTexture(r.getImpl());
if (d) {
d.y = v;
d.z = r;
}
}
this.sp._nativeSkeleton.updateRegionSp(n, v, h.rect, h.origSize, h.offset, h.uv, h.degrees);
}
} else $fc.logMgr.warn("[ChangeableSpine] updateRegionSp attachment null, slot=" + i + ", att=" + o);
};
return r([ g ], e);
}(d.$kG));
i.$kl = m;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/perf/$DVH": void 0,
"../../../game/src/perfab/common/$FDO": void 0,
"../../../game/src/perfab/common/$FDU": void 0,
"../../../game/src/utils/$DWA": void 0,
"../../../game/src/utils/$DWB": void 0,
"../../../main/src/$KV": void 0,
"./$DRK": "$DRK"
} ],
$DQV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "db72btlVR9NgZawEg0rZJH4", "$DQV");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kp = void 0;
var s = t("../../../game/src/battle/$DSC"), a = cc._decorator, c = a.ccclass, l = (a.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
return e;
}
e.prototype.ks = function() {};
e.prototype.kw = function() {};
e.prototype.setTimeScale = function() {};
e.prototype.reuseSelf = function() {
this.lL();
};
return r([ c ], e);
}(s.IVFXItem));
i.$kp = l;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0
} ],
$DQW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "dcbbbHptf9PqY/HecCOqBEK", "$DQW");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../game/src/battle/$DSC"), a = t("../../../game/src/battle/$DSO"), c = t("../../../game/src/$KT"), l = t("../../../main/src/$KV"), h = t("./$DRG"), u = cc._decorator, b = u.ccclass, p = (u.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bqK = 0;
e.bqM = null;
e.bqN = null;
e.bqO = 0;
e.bqP = 0;
e.bqQ = 0;
e.bqR = 0;
e.bqS = 0;
return e;
}
e.prototype.ks = function() {};
e.prototype.bqT = function(t, e) {
this.bqQ = e;
this.bqO = this.node.x;
this.bqP = this.node.y;
this.bqR = 400;
this.bqS = 300;
this.bqK = 1;
var i = this.bqL = a.default.bEC.bEY(t.itemId);
if (i) {
if (!this.bqN) {
var o = c.default.Ji.mZ.bDE(l.cvResUrl.movieClip);
this.node.addChild(o, -1);
this.bqM = o;
this.bqN = o.getComponent(h.default);
}
this.bqN.node.scale = 1;
this.bqN.node.angle = 0;
this.bqN.node.setPosition(0, 0);
this.bqN.setTimeScale(1);
this.bqN.btL(a.default.bEC.bEZ(i.quality));
this.bqN.btN(l.cvResUrl.mc_drop_PuTong_ball, !0, function() {});
}
};
e.prototype.bqU = function() {};
e.prototype.kF = function(t) {
var e, i, o;
switch (this.bqK) {
case 1:
(b = this.node.y - this.bqP) > 50 ? this.bqK = 2 : this.node.y += 400 * t;
break;

case 2:
var n = null === (i = null === (e = c.default.Ji) || void 0 === e ? void 0 : e.nc) || void 0 === i ? void 0 : i.iN(this.bqQ);
if (!n) {
c.default.Ji.mZ.bDO(l.cvResUrl.DropItemPickBall, this.node);
return;
}
var r = null === (o = n.HI) || void 0 === o ? void 0 : o.node;
if (r) {
var s = this.node, a = r.x, h = r.y + 105, u = a - s.x, b = h - s.y, p = Math.abs(u), f = Math.abs(b), d = Math.min(t * this.bqR, p), v = Math.min(t * this.bqS, f);
s.x = s.x + Math.sign(u) * d;
s.y = s.y + Math.sign(b) * v;
this.bqR += 3e3 * t;
this.bqS += 3e3 * t;
if (p < 10 && f < 10) {
this.bqK = 3;
this.bqV();
}
}
}
};
e.prototype.bqV = function() {
var t = this;
this.bqN.btN(l.cvResUrl.mc_drop_PuTong_ball_light, !1, function() {
var e;
(null === (e = t.node) || void 0 === e ? void 0 : e.isValid) && c.default.Ji.mZ.bDO(l.cvResUrl.DropItemPickBall, t.node);
});
};
e.prototype.reuseSelf = function() {
this.bqU();
this.lL();
};
return r([ b ], e);
}(s.IVFXItem));
i.default = p;
cc._RF.pop();
}, {
"../../../game/src/$KT": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../main/src/$KV": void 0,
"./$DRG": "$DRG"
} ],
$DQX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7d48bi6MARPz5LzFsOYoXtP", "$DQX");
var o, n, r = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), s = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../../../bingo/src/$KG"), c = t("../../../game/src/battle/$DSC"), l = t("../../../game/src/battle/$DSO"), h = t("../../../game/src/$KT"), u = t("../../../main/src/$KV"), b = t("../../../main/src/tbl/TblEnum"), p = t("./$DRG"), f = cc._decorator, d = f.ccclass, v = f.property, y = ((n = {})[b.TblEnum.QualityTypeTableEnum.EPIC] = [ u.cvResUrl.mc_drop_ShenQi_light0, u.cvResUrl.mc_drop_ShenQi_light1, u.cvResUrl.mc_drop_ShenQi_light2, 3, 1.4, 2 ], 
n[b.TblEnum.QualityTypeTableEnum.LEGEND] = [ u.cvResUrl.mc_drop_ShiShi_light0, u.cvResUrl.mc_drop_ShiShi_light1, u.cvResUrl.mc_drop_ShiShi_light2, 4, 1.5, 2 ], 
n), g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.icon = null;
e.main = null;
e.bqW = 0;
e.bqY = null;
e.bqZ = null;
e.brb = 1;
return e;
}
e.prototype.ks = function() {};
e.prototype.bra = function(t) {
var e = this;
this.main.scale = .62;
this.main.angle = 0;
this.bqW = 0;
this.node.opacity = 255;
this.icon.spriteFrame = null;
var i = this.bqX = l.default.bEC.bEY(t.itemId);
if (i) {
this.lC(this.icon, i.icon);
this.icon.node.angle = 0;
if (i.type === a.$fe.tblEnum.ItemTypeTableEnum.EQUIP) {
if (this.bqZ) return;
var o = y[i.quality];
i.quality >= b.TblEnum.QualityTypeTableEnum.EPIC && h.default.Ji.na.byM(u.cvResUrl.legendary_epic_item_fall);
if (o) {
var n = h.default.Ji.mZ.bDE(u.cvResUrl.movieClip);
this.node.addChild(n, 100);
n.scale = o[3];
n.angle = 0;
n.setPosition(0, 0);
var r = n.getComponent(p.default);
this.bqZ = r;
this.brc();
r.btN(o[0], !1, function() {
e.bqZ = null;
n.isValid && h.default.Ji.mZ.bDO(u.cvResUrl.movieClip, n);
}, .2 * Math.random());
}
}
}
};
e.prototype.setTimeScale = function(t) {
if (this.brb !== t) {
this.brb = t;
this.brc();
}
};
e.prototype.brc = function() {
var t;
(null === (t = this.bqZ) || void 0 === t ? void 0 : t.isValid) && this.bqZ.setTimeScale(1 * this.brb);
};
e.prototype.brd = function() {
var t;
if (!this.bqW) {
this.main.angle = 0;
this.bqW = 1;
if ((null === (t = this.bqX) || void 0 === t ? void 0 : t.type) === a.$fe.tblEnum.ItemTypeTableEnum.EQUIP) {
var e = y[this.bqX.quality];
if (e) {
var i = h.default.Ji.mZ.bDE(u.cvResUrl.movieClip);
this.node.addChild(i, 100);
i.scale = e[5];
i.angle = 0;
i.setPosition(0, 0);
(o = i.getComponent(p.default)).setTimeScale(1);
o.btN(e[2], !1, function() {
i.isValid && h.default.Ji.mZ.bDO(u.cvResUrl.movieClip, i);
});
var o, n = this.bqY;
if (!this.bqY) {
var r = h.default.Ji.mZ.bDE(u.cvResUrl.movieClip);
this.node.addChild(r, -1);
n = this.bqY = r;
}
n.scale = e[4];
n.angle = 0;
n.setPosition(0, 0);
(o = n.getComponent(p.default)).setTimeScale(1);
o.btN(e[1], !0, function() {});
}
}
}
};
e.prototype.bre = function() {
var t;
if (this.icon) {
this.icon._cv_loading_sp = null;
this.icon.spriteFrame = null;
}
(null === (t = this.bqY) || void 0 === t ? void 0 : t.isValid) && h.default.Ji.mZ.bDO(u.cvResUrl.movieClip, this.bqY);
this.bqY = null;
};
e.prototype.kF = function(t) {
this.bqW || (this.main.angle += 450 * t);
};
e.prototype.reuseSelf = function() {
this.bre();
this.lL();
};
s([ v(cc.Sprite) ], e.prototype, "icon", void 0);
s([ v(cc.Node) ], e.prototype, "main", void 0);
return s([ d ], e);
}(c.IVFXItem);
i.default = g;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../main/src/$KV": void 0,
"../../../main/src/tbl/TblEnum": void 0,
"./$DRG": "$DRG"
} ],
$DQY: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ec6907avmJJoY/qk1PpX6qH", "$DQY");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../game/src/battle/$DSC"), a = t("../../../game/src/battle/$DSO"), c = cc._decorator, l = c.ccclass, h = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.itemName = null;
e.brf = null;
return e;
}
i = e;
e.prototype.start = function() {};
e.prototype.brg = function(t) {
var e = this.brf = a.default.bEC.bEY(t.itemId);
if (e) {
var o = $fc.i18n.JN(e.text);
t.amount > 1 && (o = o + "x" + t.amount);
this.itemName.string = o;
this.itemName.node.color = a.default.bEC.bEZ(e.quality);
i.brh(this.itemName);
this.node.getChildByName("img").width = this.itemName.node.width + 14;
}
};
e.brh = function(t) {
t._forceUpdateRenderData();
};
e.prototype.reuseSelf = function() {
this.lL();
};
var i;
r([ h(cc.Label) ], e.prototype, "itemName", void 0);
return i = r([ l ], e);
}(s.IVFXItem);
i.default = u;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0
} ],
$DQZ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "035e735NfBM6KIuNvi6ifxm", "$DQZ");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.sp = null;
e.sfArray = [];
e.bri = null;
return e;
}
e.prototype.kq = function() {};
e.prototype.onEnable = function() {
var t = this.sp.node;
t.y = 0;
cc.Tween.stopAllByTarget(t);
cc.tween(t).repeatForever(cc.tween().to(.5, {
y: 20
}).to(.5, {
y: 0
})).start();
};
e.prototype.brj = function(t) {
this.sp.spriteFrame = this.sfArray[t];
};
r([ l(cc.Sprite) ], e.prototype, "sp", void 0);
r([ l(cc.SpriteFrame) ], e.prototype, "sfArray", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRA: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "116ec6gzcFD/4snNFoR/gbp", "$DRA");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = [ cc.color(216, 0, 0), cc.color(248, 128, 0), cc.color(144, 200, 0), cc.color(0, 120, 200), cc.color(136, 128, 248) ], u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.current = null;
e.mid = null;
e.next = null;
e.text = null;
e.brk = 0;
e.brl = 0;
e.brm = 0;
e.brn = 0;
e.bro = 0;
e.brp = 360;
e.brq = !1;
e.brr = [];
return e;
}
e.prototype.kq = function() {
this.mid.node.active = !1;
this.text.node.zIndex = 100;
this.next.node.active = !1;
this.brp = this.current.node.width;
};
e.prototype.kF = function() {
if (this.brl > this.brm) {
var t = this.brl - this.brm;
this.brl = this.brl + .128 * (this.brm - this.brl);
t / this.brn < .05 && (this.brl = Math.max(this.brl - .05 * this.brn, this.brm));
this.brk = this.brl % this.brn;
0 === this.brk && this.brl > 0 && (this.brk = this.brn);
this.brv();
}
};
e.prototype.brs = function(t, e, i) {
this.brn = e;
if (this.brq !== i) {
this.brq = i;
this.text.string = "";
}
this.bru(t);
};
e.prototype.brt = function(t, e) {
this.brn = e;
this.brl = this.brm = t;
this.brk = this.brl % this.brn;
0 === this.brk && this.brl > 0 && (this.brk = this.brn);
this.bry();
this.brv();
};
e.prototype.bru = function(t) {
if (this.brm !== t) {
this.brm = t;
var e = t - this.brl, i = e < 0;
if (Math.abs(e) > this.brn && i) ; else {
var o = this.brk;
this.brl = t;
this.brk = this.brl % this.brn;
0 === this.brk && this.brl > 0 && (this.brk = this.brn);
this.brv();
i && this.brw(this.brk, Math.abs(e), o);
}
}
};
e.prototype.brv = function() {
this.bro = Math.ceil(this.brl / this.brn);
var t = Math.max(0, this.bro - 1);
if (0 === t) this.next.node.active = !1; else {
this.next.node.active = !0;
this.next.node.color = h[(t - 1 + h.length) % h.length];
}
this.brB(this.current, this.brk);
this.current.node.color = h[t % h.length];
this.text.string = this.brq ? "" : "x " + this.bro;
};
e.prototype.brw = function(t, e, i) {
if (e > this.brn || i <= e) {
this.brx(0, i / this.brn);
if (this.brm > this.brn) {
var o = t / this.brn, n = 1 - o;
this.brx(o, n);
}
} else this.brx(t / this.brn, e / this.brn);
};
e.prototype.brx = function(t, e) {
var i = this, o = this.brz();
o.active = !0;
this.node.addChild(o, 10);
var n = o.getComponent(cc.Sprite);
n.fillStart = t;
n.fillRange = e;
cc.Tween.stopAllByTarget(o);
o.opacity = 255;
cc.tween(o).to(.4, {
opacity: 0
}).call(function() {
i.brA(o);
}).start();
};
e.prototype.bry = function() {
for (var t = 3; t < this.node.childrenCount; t++) this.brA(this.node.children[t]);
};
e.prototype.brz = function() {
return this.brr.length > 0 ? this.brr.pop() : cc.instantiate(this.mid.node);
};
e.prototype.brA = function(t) {
cc.Tween.stopAllByTarget(t);
t.removeFromParent(!1);
this.brr.push(t);
};
e.prototype.brB = function(t, e) {
t.fillRange = e / this.brn;
};
r([ l(cc.Sprite) ], e.prototype, "current", void 0);
r([ l(cc.Sprite) ], e.prototype, "mid", void 0);
r([ l(cc.Sprite) ], e.prototype, "next", void 0);
r([ l(cc.Label) ], e.prototype, "text", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRB: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "1efb89RQJBCjoU8TxTtFvGs", "$DRB");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kw = void 0;
var c = t("./$DRG"), l = t("./$DRJ"), h = t("./$DRK"), u = t("../../../game/src/battle/$DSM"), b = t("./$DRD"), p = t("../../../game/src/battle/$DSC"), f = t("../../../game/src/$KT"), d = t("../../../bingo/src/$KG"), v = t("../../../bingo/src/utils/$DON"), y = t("../../../game/src/utils/$DVY"), g = t("../../../game/src/battle/$DSO"), m = t("./$DQU"), _ = t("../../../game/src/utils/$DWA"), $ = t("../../../game/src/perfab/common/$FDW"), w = t("./$DRM"), D = t("../../../game/src/perf/$DVH"), P = cc._decorator, R = P.ccclass, S = P.property, E = (cc.color(255, 80, 80), 
[ cc.color().fromHEX("00f0ff"), cc.color().fromHEX("ffff30"), cc.color().fromHEX("ff4848") ]), C = cc.color(), O = function() {
function t(t, e) {
this.brC = 0;
this.brD = null;
this.brE = null;
this.brF = null;
this.brD = t;
this.brC = e;
}
t.prototype.brG = function() {
this.brJ();
this.brD = null;
this.brF = null;
this.brE = null;
};
t.prototype.brH = function(t) {
var e, i, o, n;
this.brF = t;
if (this.brF && (null === (e = this.brD) || void 0 === e ? void 0 : e.isValid)) switch (null !== (i = this.brF.resType) && void 0 !== i ? i : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
if (!this.brE) {
this.brE = f.default.Ji.mZ.bDE(d.$fe.ResUrl.spineEffect);
if (!this.brE) return;
this.brD.addChild(this.brE, -10);
}
this.brE.zIndex = this.brF.zIndex;
var r = this.brE.getComponent(h.$kG);
if (!r) return;
var s = r.node, a = this.brF;
cc.Tween.stopAllByTarget(s);
s.active = !1;
if (a.delay || a.interval) cc.tween(s).delay(a.delay || 0).call(function() {
cc.tween(s).call(function() {
r.mm(a.url, a.anim || "idle", !1);
s.active = !0;
}).delay(a.interval).union().repeatForever().start();
}).start(); else {
r.mm(a.url, a.anim || "idle", !0);
s.active = !0;
}
r.setTimeScale(null !== (o = a.tScale) && void 0 !== o ? o : 1);
r.mk(Boolean(a.premultAlpha));
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
if (!this.brE) {
this.brE = f.default.Ji.mZ.bDE(d.$fe.ResUrl.movieClip);
if (!this.brE) return;
this.brD.addChild(this.brE, -10);
}
this.brE.zIndex = this.brF.zIndex;
var l = this.brE.getComponent(c.default);
if (!l) return;
var u = l.node, b = this.brF;
cc.Tween.stopAllByTarget(u);
u.active = !1;
if (b.delay || b.interval) cc.tween(u).delay(b.delay || 0).call(function() {
cc.tween(u).call(function() {
l.btN(b.url, !0);
u.active = !0;
}).delay(b.interval).union().repeatForever().start();
}).start(); else {
l.btN(b.url, !0);
u.active = !0;
}
l.setTimeScale(null !== (n = b.tScale) && void 0 !== n ? n : 1);
}
};
t.prototype.brI = function() {
var t, e, i, o, n, r, s, a;
if (this.brD && this.brE) {
var c = g.default.bEC.bFj(this.brC);
if (c) {
var l = c.fashion, h = null !== (t = null == l ? void 0 : l.scale) && void 0 !== t ? t : 1, u = null !== (e = null == l ? void 0 : l.x) && void 0 !== e ? e : 0, b = null !== (i = null == l ? void 0 : l.y) && void 0 !== i ? i : 0, p = this.brF, f = this.brD.scaleX, d = this.brE;
d.zIndex = null !== (o = p.zIndex) && void 0 !== o ? o : -10;
d.scaleX = h * (null !== (n = p.scaleX) && void 0 !== n ? n : 1) * f;
d.scaleY = h * (null !== (r = p.scaleY) && void 0 !== r ? r : 1);
d.x = ((null !== (s = p.x) && void 0 !== s ? s : 0) + u) * f;
d.y = (null !== (a = p.y) && void 0 !== a ? a : 0) + c.offsetY + b;
}
}
};
t.prototype.brJ = function() {
var t;
if (this.brE && cc.isValid(this.brE)) if (this.brF) switch (null !== (t = this.brF.resType) && void 0 !== t ? t : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
cc.Tween.stopAllByTarget(this.brE);
f.default.Ji.mZ.bDO(d.$fe.ResUrl.spineEffect, this.brE);
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
cc.Tween.stopAllByTarget(this.brE);
f.default.Ji.mZ.bDO(d.$fe.ResUrl.movieClip, this.brE);
break;

default:
this.brE.destroy();
} else this.brE.destroy();
this.brF = null;
this.brE = null;
};
return t;
}(), T = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.dirNode = null;
e.mainNode = null;
e.brK = 1;
e.brL = !0;
e.brN = null;
e.brO = null;
e.brP = null;
e.brQ = 0;
e.brR = !1;
e.brS = 255;
e.brT = null;
e.brW = 1;
e.brX = 1;
e.bsb = 0;
e.bse = [];
e.bsk = null;
e.bsl = 0;
e.bsm = 0;
e.bsn = 0;
e.bss = -1;
e.bst = "";
e.bsu = !1;
e.bsy = !0;
e.bsz = 0;
e.bsA = [];
e.bsD = !0;
e.bsE = 0;
e.bsF = [];
e.bsP = null;
return e;
}
e.prototype.kq = function() {};
e.prototype.ks = function() {
cc.director.on(cc.Director.EVENT_AFTER_DRAW, this.bsp, this);
};
e.prototype.kw = function() {
cc.director.targetOff(this);
this.bsg();
this.brO = null;
this.brP = null;
this.brM = null;
this.brN = null;
this.bsG(0);
this.bsB(0);
};
e.prototype.brU = function(t) {
for (var e = p.COMMON_SKEL_MIX_ANIM.filter(function(e) {
return t.findAnimation(e);
}), i = 0, o = p.ALL_SKEL_ANIM_LIST.filter(function(e) {
return t.findAnimation(e);
}); i < o.length; i++) for (var n = o[i], r = 0, s = e; r < s.length; r++) {
var a = s[r];
n !== a && t.setMix(n, a, .15);
}
};
e.prototype.brV = function(t, e, i) {
var o;
this.bst = "";
this.brM = t;
this.brP = i;
this.brO = g.default.bEC.bFj(e);
if (this.brO) {
this.brW = 1;
this.brX = 1;
this.brK = 1;
this.mainNode.opacity = this.brS = 255;
this.brT = null;
this.mainNode.destroyAllChildren();
this.brN = null;
this.mainNode.active = !f.default.Jh.cbh;
this.mainNode.setPosition(0, 0);
this.brL = this.brO.resType !== p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE;
switch (this.brO.resType) {
case p.DISPLAY_RES_TYPE.SPINE:
var n = g.default.bEC.bFG(this.brO.resType), r = this.brP.bDE(n);
if (!r) break;
this.mainNode.addChild(r, 1);
(s = r.getComponent(h.$kG)).ml(_.$se.cEB());
r.scaleX = this.brO.scaleX;
r.scaleY = this.brO.scaleY;
r.setPosition(this.brO.offsetX, this.brO.offsetY);
r.angle = 0;
r.active = !1;
s.mg = null;
s.mk(Boolean(this.brO.premultAlpha));
s.mo(this.brO.resUrl).then(function(t) {
t && (r.active = !0);
});
this.brN = s;
this.brN.resUrl = n;
break;

case p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE:
n = g.default.bEC.bFG(this.brO.resType);
var s, a = this.brP.bDE(n);
if (!a) break;
this.mainNode.addChild(a, 1);
(s = a.getComponent(m.$kl)).ml(_.$se.cEy);
a.scaleX = this.brO.scaleX;
a.scaleY = this.brO.scaleY;
a.setPosition(this.brO.offsetX, this.brO.offsetY);
a.angle = 0;
a.active = !1;
s.mg = null;
s.mk(Boolean(this.brO.premultAlpha));
s.mo(this.brO.resUrl).then(function(t) {
t && (a.active = !0);
});
this.brN = s;
this.brN.resUrl = n;
s.bqp(null !== (o = this.brO.models) && void 0 !== o ? o : [], 1);
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
n = g.default.bEC.bFG(this.brO.resType);
var l = this.brP.bDE(n);
if (!l) break;
this.mainNode.addChild(l, 1);
var u = l.getComponent(c.default);
l.scaleX = this.brO.scaleX;
l.scaleY = this.brO.scaleY;
l.setPosition(this.brO.offsetX, this.brO.offsetY);
l.angle = 0;
u.btN(this.brO.resUrl, !0);
this.brN = u;
this.brN.resUrl = n;
}
this.bsa();
}
};
e.prototype.brY = function(t) {
if (this.brW !== t) {
this.brW = t;
this.bsa();
}
};
e.prototype.brZ = function(t) {
if (this.brK !== t) {
this.brK = t;
this.bsa();
}
};
e.prototype.bsa = function() {
this.brN && this.brN.setTimeScale(this.brW * this.brK);
};
e.prototype.bsc = function(t) {
t = v.default.bci(t);
if (this.bsb !== t) {
this.bsb = t;
this.dirNode.angle = t;
}
};
e.prototype.bsd = function(t) {
if (this.dirNode.scaleX !== t) {
this.dirNode.scaleX = t;
this.bsR();
}
};
e.prototype.bsf = function(t) {
var e = this.bse.findIndex(function(e) {
return e.id === t;
}), i = this.bse[e];
if (i) {
this.bse.splice(e, 1);
cc.isValid(i.node) && this.brP.bDO(i.nodeUrl, i.node);
}
};
e.prototype.bsg = function() {
for (var t = 0; t < this.bse.length; t++) {
var e = this.bse[t];
cc.isValid(e.node) && this.brP.bDO(e.nodeUrl, e.node);
}
this.bse.length = 0;
};
e.prototype.bsh = function(t) {
for (var e = 0; e < this.bse.length; e++) this.bse[e].node.active = t;
if (this.bsF.length > 0) for (var i = 0, o = this.bsF; i < o.length; i++) (s = o[i]).brE && (s.brE.opacity = t ? 255 : 0);
if (this.bsA.length > 0) for (var n = 0, r = this.bsA; n < r.length; n++) {
var s;
(s = r[n]).brE && (s.brE.opacity = t ? 255 : 0);
}
};
e.prototype.bsi = function(t, e, i, o) {
var n, r, s = this;
if (!t) return !1;
if (t.Hi) {
r = t.Hi.node;
"number" == typeof e.z && (r.zIndex = e.z);
"number" == typeof e.x && (r.x = e.x);
"number" == typeof e.y && (r.y = e.y);
"number" == typeof e.angle && (r.angle = e.angle);
} else {
var a = g.default.bEC.bFG(e.resType);
e.resType === p.DISPLAY_RES_TYPE.PREFAB && (a = String(e.resUrl));
if (!(r = this.brP.bDE(a))) return !1;
var u = "number" == typeof e.z ? e.z : 1;
i.addChild(r, u);
cc.Tween.stopAllByTarget(r);
r.opacity = 255;
r.x = "number" == typeof e.x ? e.x : 0;
r.y = "number" == typeof e.y ? e.y : 0;
r.angle = "number" == typeof e.angle ? e.angle : 0;
t.Hh = a;
}
r.scaleX = t.Hl * t.Hj;
r.scaleY = t.Hm * t.Hk;
var b = null !== (n = e.timeScale) && void 0 !== n ? n : 1;
t.Hf = b;
t.Hg = null == o || o;
var f = t.Hg ? this.brW : this.brX;
switch (e.resType) {
case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
if (!e.resUrl) break;
var d = r.getComponent(c.default);
if (!d) return !0;
"string" == typeof e.color && d.btL(C.fromHEX(e.color));
t.Hi = d;
d.setTimeScale(f * b);
e.autoDestroy ? d.btN(e.resUrl, !1, function() {
s.brM && s.brM.Ic(t);
}, e.animStart, e.animEnd) : d.btN(e.resUrl, Boolean(e.loop), null, e.animStart, e.animEnd);
break;

case p.DISPLAY_RES_TYPE.SPINE:
if (!e.resUrl) break;
if (!e.anim) break;
var v = r.getComponent(h.$kG);
if (!v) return !0;
t.Hi = v;
v.ml(null == e.animStart && null == e.animEnd);
v.setTimeScale(f * b);
v.mk(Boolean(e.premultAlpha));
e.autoDestroy ? v.mm(e.resUrl, e.anim, !1, function() {
s.brM.Ic(t);
}, e.animStart, e.animEnd) : v.mm(e.resUrl, e.anim, Boolean(e.loop), null, e.animStart, e.animEnd);
break;

case p.DISPLAY_RES_TYPE.SINGLE_TEXTURE:
if (!e.resUrl) break;
var y = r.getComponent(l.$kF);
if (!y) return !0;
t.Hi = y;
y.setTimeScale(f);
y.btY({
resUrl: e.resUrl,
blendMode: e.blend
});
"string" == typeof e.color && y.btZ(C.fromHEX(e.color));
break;

case p.DISPLAY_RES_TYPE.PREFAB:
var m = r.getComponent(p.IVFXItem);
t.Hi = m;
m.prefabPlay && m.prefabPlay(function() {
e.autoDestroy && s.brM.Ic(t);
});
break;

case p.DISPLAY_RES_TYPE.VIDEO:
break;

case p.DISPLAY_RES_TYPE.WARNING_AREA:
if (!e.warningArea) break;
var _ = r.getComponent(w.$kI);
if (!_) return !0;
t.Hi = _;
_.setTimeScale(f * b);
_.buE(e.warningArea, function() {
e.autoDestroy && s.brM.Ic(t);
});
}
r.active = !0;
e.stopAllAct && cc.Tween.stopAllByTarget(r);
if (e.acts) for (var $ = 0, D = e.acts; $ < D.length; $++) {
for (var P = D[$], R = cc.tween(r), S = 0, E = P; S < E.length; S++) {
var O = E[S];
R[O.t].apply(R, O.p);
}
R.start();
}
return !0;
};
e.prototype.bsj = function(t) {
if (this.brR !== t) {
this.brR = t;
this.brR || (this.mainNode.opacity = this.brS);
}
};
e.prototype.bso = function(t) {
if (this.bsl !== t) {
this.bsl = t;
this.bsl || this.bsk && (this.bsk.active = !1);
}
};
e.prototype.kF = function(t) {
if (this.brT) {
this.brS += this.brT.speed * t;
if (this.brT.speed > 0 && this.brS >= this.brT.val || this.brT.speed < 0 && this.brS <= this.brT.val) {
this.brS = this.brT.val;
this.brT = null;
}
this.mainNode.opacity = this.brS;
}
if (this.brR) {
this.brQ += t;
if (this.brQ > .07) {
this.brQ -= .07;
this.mainNode.opacity === this.brS ? this.mainNode.opacity = this.brS : this.mainNode.opacity = .2 * this.brS;
}
}
};
e.prototype.bsp = function() {
this.bsq();
};
e.prototype.bsq = function() {
if (this.brO) if (this.bsl) if (0 === this.bsm) {
this.bsm = this.bsn;
if (!this.bsk) {
var t = this.brP.bDE(d.$fe.ResUrl.ghost);
if (!t) return;
this.bsk = t;
t.groupIndex = y.CC_GROUP_INDEX.BATTLE;
t.zIndex = -10;
t.parent = this.node;
}
this.bsk.active = !0;
var e = 660, i = 0, o = 280;
if (this.brO.ghostParam) {
e = this.brO.ghostParam[0];
i = this.brO.ghostParam[1];
o = this.brO.ghostParam[2];
}
var n = u.default.bEe(this.mainNode, e, i, o, 1.6);
this.bsk.x = i;
this.bsk.y = o;
this.bsk.setScale(1.6);
this.bsk.opacity = 255;
var r = this.bsk.getComponent(b.default), s = 480 / e * .006, a = E[this.bsl - 1];
if (a) {
r.btd(1, a, s);
r.btc(n);
}
} else this.bsm--; else this.bsk && (this.bsk.active = !1);
};
e.prototype.reuseSelf = function() {
this.bss = -1;
this.bst = "";
this.bsG(0);
this.bsB(0);
this.bsP && (this.bsP.node.active = !1);
};
e.prototype.bsr = function() {
var t = this.brO;
if (!t) return !1;
if (t.resType !== p.DISPLAY_RES_TYPE.SPINE && t.resType !== p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE) return !1;
var e = this.brN;
return null == e ? void 0 : e.mn(t.resUrl);
};
e.prototype.bsv = function(t, e) {
void 0 === e && (e = 0);
var i = t.h || 0;
this.mainNode.angle !== i && (this.mainNode.angle = i);
var o = this.brO;
if (o && (o.resType === p.DISPLAY_RES_TYPE.SPINE || o.resType === p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE)) {
this.bst = t.d;
var n = this.brN;
if (n && n.mn(o.resUrl)) {
this.brZ(t.c);
var r = n.sp, s = Boolean(t.e);
if (t.d !== r.animation || e !== this.bss) {
this.bss = e;
this.bsu = s;
if (a = r.setAnimation(0, t.d, s)) {
t.i && (a.animationStart = t.i);
"number" == typeof t.j && (a.animationEnd = t.j);
}
} else if (!_.$se.cEx && this.bsu !== s) {
this.bsu = s;
var a;
if ((a = r.getCurrent(0)) && a.loop !== s) {
r.setAnimation(0, t.d, s);
t.i && (a.animationStart = t.i);
"number" == typeof t.j && (a.animationEnd = t.j);
}
}
}
n && n.mp(t.f || 0, t.g || 0);
}
};
e.prototype.bsw = function(t, e) {
var i = this.brO;
if (i && i.resType === p.DISPLAY_RES_TYPE.SPINE) {
var o = this.brN;
if (!o || o.mn(i.resUrl)) {
var n = o.sp.findBone(t);
if (n) {
n.scaleX = e ? 1 : 0;
n.scaleY = e ? 1 : 0;
}
}
}
};
e.prototype.bsx = function(t, e) {
0 == t ? this.brS = e : this.brT = {
speed: (e - this.brS) / t,
val: e
};
};
e.prototype.bsB = function(t) {
var e = this;
this.bsz = t;
this.bsy && t ? this.bsJ(this.bsz).then(function() {
e.isValid && e.bsz === t && e.bsK(e.bsy ? t : 0, e.bsA);
}) : this.bsK(0, this.bsA);
};
Object.defineProperty(e.prototype, "bsC", {
set: function(t) {
if (this.bsy !== t) {
this.bsy = t;
this.bsz && this.bsB(this.bsz);
}
},
enumerable: !1,
configurable: !0
});
e.prototype.bsG = function(t) {
var e = this;
this.bsE = t;
this.bsD && t ? this.bsJ(t).then(function() {
e.isValid && e.bsE === t && e.bsK(e.bsD ? t : 0, e.bsF);
}) : this.bsK(0, this.bsF);
};
Object.defineProperty(e.prototype, "bsH", {
set: function(t) {
if (this.bsD !== t) {
this.bsD = t;
this.bsE && this.bsG(this.bsE);
}
},
enumerable: !1,
configurable: !0
});
e.prototype.bsI = function(t) {
for (var e = 0, i = t; e < i.length; e++) i[e].brI();
};
e.prototype.bsJ = function(t) {
var e;
return s(this, void 0, void 0, function() {
var i, o, n, r, s;
return a(this,function AWAITF3(a){switch (a.label) {
case 0:
if (D.HeatIsolate.cwp) return [ 2, !0 ];
if (!(i = $fc.gameRes.cfe(t))) return [ 2, !0 ];
o = [];
for (n = 0, r = i.res; n < r.length; n++) {
s = r[n];
switch (null !== (e = s.resType) && void 0 !== e ? e : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
o.push(this.lI(new d.$fe.ResCfg(s.url, sp.SkeletonData), !1));
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
}
}
return [ 4, Promise.all(o) ];

case 1:
a.sent();
return [ 2 ];
}
});
});
};
e.prototype.bsK = function(t, e) {
var i;
if (t) {
if (!this.brO) return;
var o = $fc.gameRes.cfe(t);
if (!o) return;
for (var n = 0; n < o.res.length; n++) {
var r = o.res[n], s = e[n];
if (!s) {
s = new O(this.dirNode, this.brO.id);
e.push(s);
}
s.brE && (null === (i = s.brF) || void 0 === i ? void 0 : i.resType) !== r.resType && s.brJ();
s.brH(r);
}
for (;n < e.length; n++) e[n].brG();
e.length = o.res.length;
this.bsI(e);
} else if (e.length) {
for (var a = 0, c = e; a < c.length; a++) (r = c[a]).brG();
e.length = 0;
}
};
e.prototype.bsL = function() {
return this.brN;
};
e.prototype.bsM = function() {
return this.brN;
};
e.prototype.bsN = function(t) {
if (this.brO.resType === p.DISPLAY_RES_TYPE.SPINE || p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE) {
var e = this.brN;
e && e.ms(t);
}
};
e.prototype.bsO = function() {
if (this.brO && (this.brO.resType === p.DISPLAY_RES_TYPE.SPINE || p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE)) {
var t = this.brN;
t && t.mu(.3);
}
};
e.prototype.bsQ = function(t, e) {
return s(this, void 0, void 0, function() {
var i, o;
return a(this,function AWAITF4(n){switch (n.label) {
case 0:
return D.HeatIsolate.cwp && !this.bsP ? [ 2 ] : this.bsP ? [ 3, 2 ] : [ 4, $fc.prefabMgr.cLn(new d.$fe.ResCfg(d.$fe.ResUrl.SceneBubble, cc.Prefab)) ];

case 1:
i = n.sent();
if (!this.isValid) return [ 2 ];
if (this.bsP) return [ 2 ];
o = i.getComponent($.default);
this.bsP = o;
this.node.addChild(i, 101);
n.label = 2;

case 2:
this.bsP.node.active = !0;
this.bsP.fHe(t, e);
this.bsR();
return [ 2 ];
}
});
});
};
e.prototype.bsR = function() {
var t, e, i, o;
if (this.bsP) if (null === (t = this.brO) || void 0 === t ? void 0 : t.topPos) {
var n = null === (e = this.brO) || void 0 === e ? void 0 : e.topPos;
this.bsP.node.x = (null !== (i = n[0]) && void 0 !== i ? i : 0) * this.dirNode.scaleX;
this.bsP.node.y = (null !== (o = n[1]) && void 0 !== o ? o : 0) + 50;
} else this.bsP.node.y = 200;
};
e.prototype.bsS = function(t) {
var e = this.bsM();
if (null == e ? void 0 : e.bqw) {
var i = $fc.gameHelper.ceM(t);
e.bqw(10, i);
}
};
r([ S(cc.Node) ], e.prototype, "dirNode", void 0);
r([ S(cc.Node) ], e.prototype, "mainNode", void 0);
return r([ R ], e);
}(d.$fe.Component);
i.$kw = T;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../bingo/src/utils/$DON": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSM": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/perf/$DVH": void 0,
"../../../game/src/perfab/common/$FDW": void 0,
"../../../game/src/utils/$DVY": void 0,
"../../../game/src/utils/$DWA": void 0,
"./$DQU": "$DQU",
"./$DRD": "$DRD",
"./$DRG": "$DRG",
"./$DRJ": "$DRJ",
"./$DRK": "$DRK",
"./$DRM": "$DRM"
} ],
$DRC: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4c5b7K4v6lP5qT9taT2082Q", "$DRC");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.text = null;
e.fonts = [];
e.bsT = null;
e.bsU = !1;
e.bsV = 0;
e.bsW = 0;
e.bsX = 0;
e.bsY = 0;
e.bsZ = 0;
return e;
}
e.prototype.bta = function(t, e, i, o, n) {
void 0 === i && (i = 1);
var r = this.text;
r.string = t;
r.spacingX = -4;
var s = 0, a = 0;
switch (i) {
case 1:
r.fontSize = 26;
r.font = this.fonts[0];
a = 140 + 40 * Math.random();
s = .3;
break;

case 2:
r.fontSize = 36;
r.font = this.fonts[1];
a = 160 + 60 * Math.random();
s = .3;
break;

case 3:
r.fontSize = 26;
r.font = this.fonts[3];
a = 140 + 40 * Math.random();
s = 0;
break;

case 4:
r.fontSize = 26;
r.font = this.fonts[2];
a = 140 + 40 * Math.random();
s = 0;
}
var c = (Math.random() - .5) * Math.PI * s, l = Math.cos(c), h = Math.sin(c), u = o * l - n * h, b = o * h + n * l, p = this.node;
this.bsW = p.x;
this.bsX = p.y;
this.bsY = u * a;
this.bsZ = b * a;
this.bsV = 0;
this.bsU = !0;
this.bsT = e;
p.scale = 0;
p.opacity = 255;
};
e.prototype.kF = function(t) {
if (this.bsU) {
var e, i = this.node, o = this.bsV + t;
this.bsV = o;
if (o < .15) e = o / .15 * .7; else {
var n = (o - .15) / .45;
e = .7 + (1 - .7) * (n > 1 ? 1 : n);
}
i.setPosition(this.bsW + this.bsY * e, this.bsX + this.bsZ * e);
o < .15 ? i.scale = o / .15 * 2.25 : o < .3 ? i.scale = 2.25 + (o - .15) / .15 * -.75 : 1.5 !== i.scale && (i.scale = 1.5);
if (o >= .52) {
i.opacity = 0;
this.bsU = !1;
var r = this.bsT;
this.bsT = null;
r && r(i);
} else o >= .42 && (i.opacity = Math.round(255 * (1 - (o - .42) / .1)));
}
};
e.prototype.kv = function() {
this.bsU = !1;
this.bsT = null;
};
r([ l(cc.Label) ], e.prototype, "text", void 0);
r([ l(cc.BitmapFont) ], e.prototype, "fonts", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRD: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c20e7qR6XNHyaPlwAfho8l/", "$DRD");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("../../../game/src/battle/$DSM"), c = cc._decorator, l = c.ccclass, h = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.ghostSprite = null;
e.materials = [];
e.btb = cc.v2();
return e;
}
e.prototype.kq = function() {};
e.prototype.onEnable = function() {};
e.prototype.onDestroy = function() {
this.reuseSelf();
};
e.prototype.btc = function(t) {
var e = this.ghostSprite;
if (e.spriteFrame) {
var i = e.spriteFrame, o = i.getTexture();
e.spriteFrame = null;
o instanceof cc.RenderTexture && a.default.bEb(o);
i.destroy();
}
var n = new cc.SpriteFrame(t);
n.setFlipY(!0);
e.spriteFrame = n;
};
e.prototype.btd = function(t, e, i) {
var o = this.materials[t];
if (o) {
if (1 === t) {
this.btb.x = this.btb.y = i;
o.setProperty("outlineSize", this.btb);
}
this.ghostSprite.setMaterial(0, o);
this.ghostSprite.node.color = e;
}
};
e.prototype.bte = function(t) {
this.ghostSprite.dstBlendFactor = t ? cc.macro.BlendFactor.ONE : cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
};
e.prototype.reuseSelf = function() {
var t = this.ghostSprite;
if (t.spriteFrame) {
var e = t.spriteFrame, i = e.getTexture();
t.spriteFrame = null;
i instanceof cc.RenderTexture && a.default.bEb(i);
e.destroy();
}
};
r([ h(cc.Sprite) ], e.prototype, "ghostSprite", void 0);
r([ h(cc.Material) ], e.prototype, "materials", void 0);
return r([ l ], e);
}(s.$fe.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSM": void 0
} ],
$DRE: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "51257geVWlFkZTmi7QnRjyd", "$DRE");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../game/src/battle/$DSC"), a = t("./$DRG"), c = cc._decorator, l = c.ccclass, h = (c.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.btf = null;
return e;
}
e.prototype.kw = function() {
this.btf = null;
};
e.prototype.prefabPlay = function(t) {
var e = this;
this.btf = t;
var i = this.node.getChildByName("Flare");
i.opacity = 0;
i.scale = 1;
i.angle = 360 * Math.random();
i.stopAllActions();
cc.tween(i).to(.1, {
opacity: 255,
scale: 1.7
}).to(.1, {
opacity: 0
}).start();
var o = this.node.getChildByName("Hit"), n = o.getComponent(a.default);
n.setTimeScale(1);
n.btN(14, !1);
o.opacity = 255;
o.scale = 2;
o.angle = 360 * Math.random();
o.stopAllActions();
cc.tween(o).delay(.1).to(.2, {
opacity: 0
}).call(function() {
e.btf && e.btf();
}).start();
};
return r([ l ], e);
}(s.IVFXItem));
i.default = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0,
"./$DRG": "$DRG"
} ],
$DRF: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "827886jTORAv5jBUCh0Qew+", "$DRF");
var o, n, r, s = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), a = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kA = void 0;
var c = t("../../../bingo/src/$KG"), l = cc._decorator, h = l.ccclass, u = l.property, b = function(t) {
s(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.iconSF = [];
e.lineSF = [];
e.icon = null;
e.pathNode = null;
e.pathDotNode = null;
e.mask = null;
e.arrow = null;
return e;
}
i = e;
e.prototype.onLoad = function() {};
e.prototype.bth = function(t, e, o, n, r) {
var s = t.cYJ;
if (t.cYD) {
var a = $fc.battleMgr.bBm.bAU >= 1;
this.icon.node.active = !0;
this.icon.node.scale = .95;
this.icon.spriteFrame = this.iconSF[a ? 3 : 2];
} else if (s) {
this.icon.node.active = !0;
this.icon.node.scale = 1.3;
this.icon.spriteFrame = this.iconSF[1];
} else {
this.icon.node.active = e;
this.icon.node.scale = 1.3;
e && (this.icon.spriteFrame = this.iconSF[0]);
}
this.btj(t);
if (r && o) this.btk(); else {
cc.Tween.stopAllByTarget(this.mask);
this.mask.active = !(e || t.cYz);
this.mask.opacity = 110;
}
if (e && o && t.cYM > 0) {
this.arrow.active = !0;
var c = i.btg[t.cYM];
if (c) {
this.arrow.setPosition(c.x, c.y);
this.arrow.angle = c.angle;
}
} else this.arrow.active = !1;
};
e.prototype.btj = function(t) {
for (var e = this.pathNode.children, o = 0, n = 1; n <= 4; n++) if (t.cYv[n]) {
var r = e[++o - 1];
if (!r) {
r = cc.instantiate(e[0]);
this.pathNode.addChild(r);
}
r.active = !0;
r.angle = i.bti[n].angle;
}
for (n = o; n < e.length; n++) e[n].active = !1;
this.pathDotNode.active = 1 === o;
};
e.prototype.btk = function() {
this.mask.active = !0;
cc.Tween.stopAllByTarget(this.mask);
cc.tween(this.mask).repeatForever(cc.tween().to(.25, {
opacity: 0
}).to(.25, {
opacity: 110
})).start();
};
var i;
e.btg = ((n = {})[1] = {
angle: 90,
x: 0,
y: 30
}, n[2] = {
angle: -90,
x: 0,
y: -30
}, n[3] = {
angle: 180,
x: -30,
y: 0
}, n[4] = {
angle: 0,
x: 30,
y: 0
}, n);
e.bti = ((r = {})[1] = {
angle: 0
}, r[2] = {
angle: 180
}, r[3] = {
angle: 90
}, r[4] = {
angle: -90
}, r);
a([ u(cc.SpriteFrame) ], e.prototype, "iconSF", void 0);
a([ u(cc.SpriteFrame) ], e.prototype, "lineSF", void 0);
a([ u(cc.Sprite) ], e.prototype, "icon", void 0);
a([ u(cc.Node) ], e.prototype, "pathNode", void 0);
a([ u(cc.Node) ], e.prototype, "pathDotNode", void 0);
a([ u(cc.Node) ], e.prototype, "mask", void 0);
a([ u(cc.Node) ], e.prototype, "arrow", void 0);
return i = a([ h ], e);
}(c.$fe.Component);
i.$kA = b;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRG: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "8aad7/28v5DAo1DQY73uJxb", "$DRG");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
}, c = this && this.__spreadArrays || function() {
for (var t = 0, e = 0, i = arguments.length; e < i; e++) t += arguments[e].length;
var o = Array(t), n = 0;
for (e = 0; e < i; e++) for (var r = arguments[e], s = 0, a = r.length; s < a; s++, 
n++) o[n] = r[s];
return o;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kB = void 0;
var l = t("../../../bingo/src/$KG"), h = t("../../../game/src/battle/$DSC"), u = t("../../../game/src/battle/$DSO"), b = t("../../../game/src/perf/$DVH"), p = cc._decorator, f = p.ccclass, d = p.property, v = function() {
function t() {
this.btn = new Map();
this.bto = new Map();
this.btr = null;
}
t.btm = function() {
null == t.btl && (t.btl = new t());
return t.btl;
};
t.prototype.btp = function() {
this.bto.forEach(function(t) {
for (var e = 0, i = t; e < i.length; e++) i[e].decRef();
});
this.bto.clear();
this.btn.forEach(function(t, e) {
var i = u.default.bEC.bFz(e);
i && $fc.resMgr.gaJ(i);
});
this.btn.clear();
};
t.prototype.btq = function(t) {
return s(this, void 0, Promise, function() {
var e, i, o, n, r, s, c, h, p, f, d;
return a(this,function AWAITF5(a){switch (a.label) {
case 0:
if (b.HeatIsolate.cwp) return [ 2, [] ];
if (!(e = this.bto.get(t.id))) {
e = [];
this.bto.set(t.id, e);
}
if (e.length) return [ 2, e ];
if (!(i = u.default.bEC.bFy(t.resName))) return [ 2, e ];
o = u.default.bEC.bFz(t.resName);
return (n = this.btn.get(t.resName)) ? [ 3, 2 ] : [ 4, $fc.resMgr.fZF(new l.$fe.ResCfg(o, cc.Texture2D)) ];

case 1:
r = a.sent();
n = r.CM;
e.length && n && r.CR();
a.label = 2;

case 2:
if (e.length) return [ 2, e ];
if (!n) return [ 2, e ];
this.btn.has(t.resName) || this.btn.set(t.resName, n);
s = i.fs;
if (t.slice) {
h = t.slice[0];
if ((p = t.slice[1]) > h) for (d = h; d <= p; d++) (c = s[d]) && e.push(new cc.SpriteFrame(n, cc.rect(c[0], c[1], c[2], c[3]), c[4], cc.v2(c[5], c[6]), cc.size(c[7], c[8]))); else for (d = h; d >= p; d--) (c = s[d]) && e.push(new cc.SpriteFrame(n, cc.rect(c[0], c[1], c[2], c[3]), c[4], cc.v2(c[5], c[6]), cc.size(c[7], c[8])));
} else {
f = Math.min(s.length, t.frameCount || 999);
for (d = 0; d < f; d++) {
c = s[d];
e.push(new cc.SpriteFrame(n, cc.rect(c[0], c[1], c[2], c[3]), c[4], cc.v2(c[5], c[6]), cc.size(c[7], c[8])));
}
}
return [ 2, e ];
}
});
});
};
t.prototype.bts = function() {
this.btr || (this.btr = this.btt());
return this.btr;
};
t.prototype.btt = function() {
var t = new cc.Node(), e = new cc.Node();
t.addChild(e);
var i = t.addComponent(y), o = e.addComponent(cc.Sprite);
i.sprite = o;
return i;
};
t.btl = null;
return t;
}();
i.$kB = v;
window.$fN = window.$fN || {};
$fN.mcMgr = v.btm();
var y = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sprite = null;
e.btu = null;
e.btv = .05;
e.btw = 1;
e.btx = 0;
e.bty = -1;
e.btz = 0;
e.btA = !1;
e.btB = [];
e.btC = 0;
e.btD = -1;
e.btE = !1;
e.btF = 0;
e.btG = 1;
e.btH = null;
return e;
}
e.prototype.ks = function() {
this.sprite.trim = !1;
};
e.prototype.kw = function() {
this.btB = null;
this.btH = null;
this.btu = null;
this.btE = !1;
};
e.prototype.btI = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF6(o){switch (o.label) {
case 0:
t = this.btu;
this.sprite.spriteFrame = null;
this.btB.length = 0;
return [ 4, v.btm().btq(t) ];

case 1:
e = o.sent();
if (!this.isValid) return [ 2 ];
if (this.btu.id !== t.id) return [ 2 ];
this.btB = c(e);
(i = this.btB[this.btC]) && (this.sprite.spriteFrame = i);
return [ 2 ];
}
});
});
};
e.prototype.kF = function(t) {
var e = this;
if (this.btE) {
this.btF += t * this.btG;
if (this.btG >= 0 ? this.btF >= this.btx : this.btF <= this.btz) if (this.btA) this.btG >= 0 ? this.btF = this.btz : this.btF = this.btx; else if (this.btu.fateOut) {
this.btE = !1;
cc.tween(this.sprite.node).to(this.btu.fateOut, {
opacity: 0,
scale: 0
}).call(function() {
e.btO();
if (e.btH) {
e.btH(e);
e.btH = null;
}
}).start();
} else {
this.btO();
if (this.btH) {
this.btH(this);
this.btH = null;
}
}
this.btC = Math.floor(this.btF / this.btv);
this.btC < 0 ? this.btC = 0 : this.btC > this.btw - 1 && (this.btC = this.btw - 1);
if (this.btD !== this.btC) {
this.btD = this.btC;
var i = this.btB[this.btC];
i && (this.sprite.spriteFrame = i);
}
}
};
e.prototype.btJ = function(t) {
var e = u.default.bEC.bFB(t);
if (e) {
var i = u.default.bEC.bFy(e.resName);
if (i) {
this.btu = e;
this.btv = 1 / (e.fps || 30);
var o = i.fs.length;
if (e.slice) {
var n = e.slice[0], r = e.slice[1];
o = (r - n) * (r > n ? 1 : -1) + 1;
} else e.frameCount && (o = Math.min(e.frameCount, o));
this.btK(o, this.bty);
var s = this.sprite.node;
cc.Tween.stopAllByTarget(s);
s.opacity = 255;
s.scale = 1;
s.anchorX = "number" == typeof e.ax ? e.ax : .5;
s.anchorY = "number" == typeof e.ay ? e.ay : .5;
s.x = e.ox || 0;
s.y = e.oy || 0;
s.scaleX = e.sx || 1;
s.scaleY = e.sy || 1;
e.blendDdditive ? this.sprite.dstBlendFactor = cc.macro.BlendFactor.ONE : this.sprite.dstBlendFactor = cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
this.btI();
}
} else $fc.logMgr.cHZ("not find:" + t);
};
e.prototype.btK = function(t, e) {
this.btw = t;
this.btx = this.btw * this.btv;
-1 !== e && (this.btx = Math.max(Math.min(this.btx, e), this.btz));
};
e.prototype.reuseSelf = function() {
cc.Tween.stopAllByTarget(this.node);
this.sprite.node.color = h.BATTLE_COLOR_R.WHITE;
this.node.angle = 0;
this.btE = !1;
this.btH = null;
};
e.prototype.btL = function(t) {
this.sprite.node.color = t;
};
e.prototype.btM = function(t) {
if (this.node.groupIndex !== t) {
this.node.groupIndex = t;
this.sprite.node.groupIndex = t;
}
};
e.prototype.setTimeScale = function(t) {
this.btG = t;
};
e.prototype.btN = function(t, e, i, o, n) {
void 0 === e && (e = !1);
void 0 === o && (o = 0);
void 0 === n && (n = -1);
this.btH = i;
this.bty = n;
this.btz = o;
this.btJ(t);
if (this.btu) {
this.btE = !0;
this.btC = 0;
this.btD = -1;
this.btA = e;
this.btG >= 0 ? this.btF = this.btz : this.btF = this.btx;
}
};
e.prototype.btO = function() {
this.btE = !1;
};
r([ d(cc.Sprite) ], e.prototype, "sprite", void 0);
return r([ f ], e);
}(h.IVFXItem);
i.default = y;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/perf/$DVH": void 0
} ],
$DRH: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b900cFNsHRNybavll2BuoQg", "$DRH");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("../../../game/src/$KT"), c = t("./$DRG"), l = cc._decorator, h = l.ccclass, u = l.property, b = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.state = null;
e.btP = new Map();
e.btQ = 40;
e.btR = 10;
return e;
}
e.prototype.start = function() {};
e.prototype.update = function() {};
e.prototype.btS = function(t) {
var e = this, i = 0;
t.forEach(function(t, o) {
var n, r, l, h = e.btP.get(o), u = t.val;
if (u > 0) {
for (var b = h || [], p = Math.max(0, u - b.length), f = 0; f < p && (y = a.default.Ji.mZ.bDE(s.$fe.ResUrl.movieClip)); f++) {
var d = y.getComponent(c.default);
y.angle = 0;
y.scale = null !== (n = t.scale) && void 0 !== n ? n : 1;
e.state.addChild(y);
d.btN(Number(o), !0);
b.push(y);
}
var v = u * e.btQ / 2 - .5 * e.btQ;
for (f = 0; f < b.length; f++) {
var y;
(y = b[f]).active = f < u;
f < u && y.setPosition(f * e.btQ - v + (null !== (r = t.x) && void 0 !== r ? r : 0), i * e.btQ + (i - 1) * e.btR + .5 * e.btQ + (null !== (l = t.y) && void 0 !== l ? l : 0));
}
e.btP.set(o, b);
i++;
} else h && e.btT(o);
});
this.state.height = i * this.btQ + (i - 1) * this.btR;
};
e.prototype.btT = function(t) {
var e = this.btP.get(t);
if (e) {
for (var i = e.length - 1; i >= 0; i--) a.default.Ji.mZ.bDO(s.$fe.ResUrl.movieClip, e[i]);
this.btP.delete(t);
}
};
e.prototype.btU = function() {
var t = this;
this.btP.forEach(function(e, i) {
t.btT(i);
});
this.btP.clear();
};
r([ u(cc.Node) ], e.prototype, "state", void 0);
return r([ h ], e);
}(cc.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0,
"./$DRG": "$DRG"
} ],
$DRI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "83952fISRtDDo7qVcE2A0nK", "$DRI");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kE = void 0;
var s = t("../../../game/src/battle/$DSC"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.standProgressSprite = null;
e.airProgressSprite = null;
e.lieProgressSprite = null;
return e;
}
e.prototype.ks = function() {};
e.prototype.btV = function(t, e) {
this.standProgressSprite.node.parent.active = t;
t && (this.standProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
e.prototype.btW = function(t, e) {
this.airProgressSprite.node.parent.active = t;
t && (this.airProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
e.prototype.btX = function(t, e) {
this.lieProgressSprite.node.parent.active = t;
t && (this.lieProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
r([ l(cc.Sprite) ], e.prototype, "standProgressSprite", void 0);
r([ l(cc.Sprite) ], e.prototype, "airProgressSprite", void 0);
r([ l(cc.Sprite) ], e.prototype, "lieProgressSprite", void 0);
return r([ c ], e);
}(s.IVFXItem);
i.$kE = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0
} ],
$DRJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7bf1eZ54FJH2rZWJDZJgaN1", "$DRJ");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kF = void 0;
var s = t("../../../game/src/battle/$DSC"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sprite = null;
return e;
}
e.prototype.btY = function(t) {
var e = t.angle || 0, i = t.offsetX || 0, o = t.offsetY || 0, n = t.scaleX || 1, r = t.scaleY || 1, s = this.node.children[0], a = s.getComponent(cc.Sprite);
a.spriteFrame = null;
s.x = i;
s.y = o;
s.angle = e;
s.scaleX = n;
s.scaleY = r;
1 === t.blendMode ? a.dstBlendFactor = cc.macro.BlendFactor.ONE : a.dstBlendFactor = cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
cc.Tween.stopAllByTarget(s);
if (t.rotate) {
s.angle = 0;
cc.tween(s).repeatForever(cc.tween().to(t.rotate, {
angle: 360
}).call(function() {
s.angle = 0;
})).start();
}
this.lC(a, t.resUrl);
};
e.prototype.setTimeScale = function() {};
e.prototype.btZ = function(t) {
this.sprite.node.color = t;
};
e.prototype.reuseSelf = function() {
this.sprite.node.color = s.BATTLE_COLOR_R.WHITE;
this.lL();
};
r([ l(cc.Sprite) ], e.prototype, "sprite", void 0);
return r([ c ], e);
}(s.IVFXItem);
i.$kF = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0
} ],
$DRK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "da352tRwgVGvosTaekbsIP5", "$DRK");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kG = void 0;
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/battle/$DSC"), h = t("../../../game/src/utils/$DWA"), u = t("../../../game/src/perf/$DVH"), b = cc._decorator, p = b.ccclass, f = b.property, d = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sp = null;
e.mask = null;
e.mg = null;
e.mh = "";
e.mi = "";
e.mr = !1;
e.mt = !1;
return e;
}
e.prototype.onLoad = function() {
this.sp.setCompleteListener(this.mj.bind(this));
};
e.prototype.kw = function() {
this.mg = null;
this.mh = "";
};
e.prototype.mj = function() {
this.mg && this.mg();
};
e.prototype.mk = function(t) {
this.sp.premultipliedAlpha = t;
};
e.prototype.ml = function(t) {
h.$se.cED(this.sp, t);
};
e.prototype.mm = function(t, e, i, o, n, r) {
return s(this, void 0, void 0, function() {
var s, c;
return a(this,function AWAITF7(a){switch (a.label) {
case 0:
a.trys.push([ 0, 2, , 3 ]);
this.mg = o;
this.sp.node.active = !1;
return [ 4, this.mo(t) ];

case 1:
a.sent();
if (this.isValid && this.mn(t)) {
this.sp.node.active = !0;
if (s = this.sp.setAnimation(0, e, i)) {
n && (s.animationStart = n);
r && (s.animationEnd = r);
}
}
return [ 3, 3 ];

case 2:
c = a.sent();
$fc.logMgr.info("SpineEffect play:", c);
return [ 3, 3 ];

case 3:
return [ 2 ];
}
});
});
};
e.prototype.setTimeScale = function(t) {
this.sp.timeScale = t;
};
e.prototype.mn = function(t) {
return this.mi === t;
};
e.prototype.mo = function(t) {
return s(this, void 0, void 0, function() {
var e;
return a(this,function AWAITF8(i){switch (i.label) {
case 0:
if (u.HeatIsolate.cwp) return [ 2, !1 ];
if (this.mi === t) return [ 2, !0 ];
if (this.mh === t) return [ 2, !1 ];
this.mh = t;
i.label = 1;

case 1:
i.trys.push([ 1, 3, , 4 ]);
return [ 4, this.lI(new c.$fe.ResCfg(t, sp.SkeletonData)) ];

case 2:
if ((e = i.sent()) && e.CM) {
if (this.mh === t && this.isValid) {
this.mh = "";
this.mi = t;
this.mq(e.CM);
return [ 2, !0 ];
}
return [ 2, !1 ];
}
return [ 3, 4 ];

case 3:
i.sent();
return [ 2, !1 ];

case 4:
return [ 2 ];
}
});
});
};
e.prototype.mp = function(t, e) {
var i = this.sp.node;
if (i.x !== t || i.y !== e) {
i.x = t;
i.y = e;
}
};
e.prototype.mq = function(t) {
this.sp.skeletonData = t;
};
e.prototype.ms = function(t) {
if (this.mr !== t) {
this.mr = t;
var e = this.sp.getMaterial(0);
e.setProperty("enableFlashWhite", t ? 1 : 0);
this.sp.setMaterial(0, e);
}
};
e.prototype.mu = function(t) {
this.mt = !0;
this.ms(!0);
this.scheduleOnce(this.mv.bind(this), t);
};
e.prototype.mv = function() {
this.ms(!1);
this.mt = !1;
};
e.prototype.reuseSelf = function() {
this.mp(0, 0);
this.node.x = 0;
this.node.y = 0;
this.mask.enabled = !1;
this.node.anchorX = .5;
this.node.opacity = 255;
this.node.angle = 0;
this.ms(!1);
this.mi = "";
this.mh = "";
this.sp.isValid && (this.sp.skeletonData = null);
this.lL();
};
r([ f({
type: sp.Skeleton
}) ], e.prototype, "sp", void 0);
r([ f(cc.Mask) ], e.prototype, "mask", void 0);
return r([ p ], e);
}(l.IVFXItem);
i.$kG = d;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/perf/$DVH": void 0,
"../../../game/src/utils/$DWA": void 0
} ],
$DRL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c8e50z3cxdBaoOLdXCXkpsK", "$DRL");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = cc._decorator, a = s.ccclass, c = (s.property, function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bua = 0;
e.bub = 0;
e.buc = 1;
e.bud = 1;
e.bue = cc.v2();
return e;
}
e.prototype.buf = function(t, e, i, o) {
this.bua = t;
this.bub = e;
this.buc = i;
this.bud = o;
this.bug();
};
e.prototype.bug = function() {
var t = cc.Camera.findCamera(this.node);
if (t) {
var e = t.zoomRatio;
t.zoomRatio = 1;
var i = t.getScreenToWorldPoint(cc.Vec2.ZERO_R, this.bue);
t.zoomRatio = e;
var o = 1 / e;
this.node.scaleX = o * this.buc;
this.node.scaleY = o * this.bud;
this.node.setPosition(this.bua * o + i.x, this.bub * o + i.y);
}
};
e.prototype.onDisable = function() {
this.destroy();
};
e.prototype.update = function() {
this.bug();
};
return r([ a ], e);
}(cc.Component));
i.default = c;
cc._RF.pop();
}, {} ],
$DRM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "6529cVI+DBM+LiFS1V4swgQ", "$DRM");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kI = i.$kN = i.$kM = void 0;
var s, a, c, l = t("../../../game/src/battle/$DSC"), h = t("../../battleLogic/$KL"), u = t("../../../game/src/battle/types/$DYS"), b = cc._decorator, p = b.ccclass, f = b.property;
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(s = i.$kM || (i.$kM = {}));
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(a = i.$kN || (i.$kN = {}));
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
t[t.d = 2] = "d";
t[t.e = 3] = "e";
t[t.f = 4] = "f";
})(c || (c = {}));
var d = cc.color(255, 0, 0, 255), v = cc.color(255, 0, 0, 127.5), y = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.commonSprite = null;
e.negSprite = null;
e.fanSprite = null;
e.resUrl = "";
e.buh = null;
e.bui = null;
e.buj = c.f;
e.buk = 0;
e.bul = 0;
e.bum = 0;
e.bun = 0;
e.buo = null;
e.bup = 1;
e.buq = null;
e.bur = null;
e.bus = 80;
e.but = .333;
e.buu = 4;
return e;
}
e.prototype.ks = function() {};
e.prototype.buv = function(t) {
var e = null;
this.commonSprite && (this.commonSprite.node.active = !1);
this.negSprite && (this.negSprite.node.active = !1);
this.fanSprite && (this.fanSprite.node.active = !1);
"common" === t ? e = this.commonSprite : "neg" === t ? e = this.negSprite : "fan" === t && (e = this.fanSprite);
this.buh = e;
if (this.buh) {
cc.isValid(this.buh.node) && (this.buh.node.active = !0);
this.bui = this.buh.getMaterial(0);
this.bui.setProperty("fillProgress", 0);
this.bui.setProperty("alpha", 1);
this.bui.setProperty("shapeType", s.b);
this.bui.setProperty("fillMode", 0);
this.bui.setProperty("glowSize", 4);
this.bui.setProperty("glowStrength", 2);
this.bui.setProperty("outlineWidth", 2);
this.bui.setProperty("outlineColor", d);
this.bui.setProperty("fillColor", v);
this.bui.setProperty("nodeSize", cc.v2(0, 0));
this.bui.setProperty("shapePadding", cc.v2(0, 0));
this.buh.node.width = 0;
this.buh.node.height = 0;
this.buh.node.anchorX = .5;
this.buh.node.anchorY = .5;
this.buh.node.angle = 0;
this.buh.node.scaleX = 1;
this.buh.node.scaleY = 1;
this.buh.node.color = cc.Color.WHITE;
this.buh.node.opacity = 255;
} else this.bui = null;
};
e.prototype.buw = function(t, e) {
this.buv("common");
if (this.bui) {
this.buy(t);
this.buH(null != t.k ? t.k : 1, null != t.l ? t.l : .4, null != t.m ? t.m : .1, e);
}
};
e.prototype.bux = function(t, e) {
this.buv("common");
if (this.bui) {
this.buz(t);
this.buH(null != t.l ? t.l : 1, null != t.m ? t.m : .4, null != t.n ? t.n : .1, e);
}
};
e.prototype.buy = function(t) {
var e = null != t.j ? t.j : .333, i = null != t.h ? t.h : 4, o = 2 * t.b + 2 * i, n = 2 * t.b * e + 2 * i, r = this.buh.node;
r.width = o;
r.height = n;
r.anchorX = .5;
r.anchorY = .5;
this.bui.setProperty("shapeType", s.b);
this.bui.setProperty("fillMode", 0);
this.buG(o, n, i / o, i / n, t.d || d, t.e || v, null != t.f ? t.f : 2, null != t.g ? t.g : 0, i, null != t.i ? t.i : 2);
};
e.prototype.buz = function(t) {
var e = null != t.k ? t.k : .333, i = null != t.i ? t.i : 4, o = t.c * e, n = t.b, r = o + 2 * i, c = this.buh.node;
c.width = n;
c.height = r;
c.anchorX = t.o;
c.anchorY = t.p;
var l = null != t.d ? t.d : a.b;
this.bui.setProperty("shapeType", s.c);
this.bui.setProperty("fillMode", l);
this.buG(n, r, 0, i / r, t.e || d, t.f || v, null != t.g ? t.g : 2, null != t.h ? t.h : 0, i, null != t.j ? t.j : 2);
};
e.prototype.buA = function(t, e) {
this.buv("neg");
if (this.bui) {
this.buy(t);
var i = t.b, o = null != t.o ? t.o : .3 * i, n = i > 0 ? o / i : .3;
this.bui.setProperty("innerRatio", Math.max(0, Math.min(1, n)));
this.buH(null != t.k ? t.k : 1, null != t.l ? t.l : .4, null != t.m ? t.m : .1, e);
}
};
e.prototype.buB = function(t, e) {
this.buv("neg");
if (this.bui) {
this.buz(t);
var i = null != t.k ? t.k : .333, o = t.c * i * .5, n = null != t.r ? t.r : .3 * o, r = o > 0 ? n / o : .3;
this.bui.setProperty("innerRatio", Math.max(0, Math.min(1, r)));
this.buH(null != t.l ? t.l : 1, null != t.m ? t.m : .4, null != t.n ? t.n : .1, e);
}
};
e.prototype.buC = function(t, e) {
this.buv("fan");
if (this.bui) {
this.buD(t);
this.buH(null != t.n ? t.n : 1, null != t.o ? t.o : .4, null != t.p ? t.p : .1, e);
}
};
e.prototype.buD = function(t) {
var e = null != t.m ? t.m : .333, i = null != t.k ? t.k : 4, o = 2 * t.b + 2 * i, n = 2 * t.b * e + 2 * i, r = this.buh.node;
r.width = o;
r.height = n;
r.anchorX = .5;
r.anchorY = .5;
var s = t.b, a = 0;
t.c && (a = null != t.d ? t.d : .3 * s);
var c = s > 0 ? a / s : 0, l = null != t.e ? t.e : 0, h = null != t.f ? t.f : 60, u = Math.PI / 180, b = l * u, p = .5 * Math.max(0, Math.min(180, h)) * u;
this.bui.setProperty("innerRatio", Math.max(0, Math.min(1, c)));
this.bui.setProperty("fanAngle", b);
this.bui.setProperty("fanArcHalf", p);
this.buG(o, n, i / o, i / n, t.g || d, t.h || v, null != t.i ? t.i : 2, null != t.j ? t.j : 0, i, null != t.l ? t.l : 2);
};
e.prototype.prefabPlay = function() {};
e.prototype.buE = function(t, e) {
var i, o, n;
switch (t.type) {
case 1:
var r = {
b: t.w,
c: t.h,
d: t.fillMode,
o: null !== (i = t.ax) && void 0 !== i ? i : 0,
p: null !== (o = t.ay) && void 0 !== o ? o : .5,
l: t.fillTime,
m: t.holdTime,
n: t.fadeTime,
g: t.outlineWidth,
q: t.isNeg,
r: t.negInnerW
};
if (t.lockLine) {
r.m = Number.MAX_SAFE_INTEGER;
r.n = 0;
}
t.isNeg ? this.buB(r, e) : this.bux(r, e);
if (t.lockLine) {
this.bur = t.lockLine;
this.bus = null !== (n = t.h) && void 0 !== n ? n : 80;
this.but = .333;
this.buu = 4;
}
break;

case 2:
var s = {
b: t.w,
c: t.h || t.w,
k: t.fillTime,
l: t.holdTime,
f: t.outlineWidth,
m: t.fadeTime,
n: t.isNeg,
o: t.negInnerW
};
t.isNeg ? this.buA(s, e) : this.buw(s, e);
break;

case 3:
var a = {
b: t.w,
c: t.isNeg,
d: t.negInnerW,
e: t.angle,
f: t.arcAngle,
n: t.fillTime,
o: t.holdTime,
p: t.fadeTime,
i: t.outlineWidth
};
this.buC(a, e);
}
};
e.prototype.setTimeScale = function(t) {
this.bup = t;
};
e.prototype.buF = function(t) {
this.buq = t;
this.bur && this.buI();
};
e.prototype.reuseSelf = function() {
this.buj = c.f;
this.buk = 0;
this.buo = null;
this.bup = 1;
var t = this.commonSprite ? this.commonSprite.getMaterial(0) : null;
if (t) {
t.setProperty("fillProgress", 0);
t.setProperty("alpha", 1);
}
var e = this.negSprite ? this.negSprite.getMaterial(0) : null;
if (e) {
e.setProperty("fillProgress", 0);
e.setProperty("alpha", 1);
}
var i = this.fanSprite ? this.fanSprite.getMaterial(0) : null;
if (i) {
i.setProperty("fillProgress", 0);
i.setProperty("alpha", 1);
}
this.buh = null;
this.bui = null;
this.buq = null;
this.bur = null;
this.lL();
};
e.prototype.buG = function(t, e, i, o, n, r, s, a, c, l) {
this.bui.setProperty("nodeSize", cc.v2(t, e));
this.bui.setProperty("shapePadding", cc.v2(i, o));
this.bui.setProperty("outlineColor", n);
this.bui.setProperty("fillColor", r);
this.bui.setProperty("outlineWidth", s);
this.bui.setProperty("innerGradient", a);
this.bui.setProperty("glowSize", c);
this.bui.setProperty("glowStrength", l);
};
e.prototype.buH = function(t, e, i, o) {
this.bul = t;
this.bum = e;
this.bun = i;
this.buo = o || null;
this.buj = c.b;
this.buk = 0;
this.bui.setProperty("fillProgress", 0);
this.bui.setProperty("alpha", 1);
};
e.prototype.update = function(t) {
if (this.buj !== c.f && this.bui) {
this.bur && this.buq && this.buI();
this.buk += t * this.bup * this.worldTimeScale;
switch (this.buj) {
case c.b:
this.buj = c.c;
this.buk = 0;
break;

case c.c:
var e = Math.min(this.buk / this.bul, 1);
this.bui.setProperty("fillProgress", e);
if (e >= 1) {
this.buj = c.d;
this.buk = 0;
}
break;

case c.d:
if (this.buk >= this.bum) {
this.buj = c.e;
this.buk = 0;
}
break;

case c.e:
e = Math.min(this.buk / this.bun, 1);
this.bui.setProperty("alpha", 1 - e);
if (e >= 1) {
this.buj = c.f;
if (this.buo) {
this.buo();
this.buo = null;
}
}
}
}
};
e.prototype.buI = function() {
if (this.buh && this.bui) {
var t = this.buq;
if (t && t.hw) {
var e = this.bur, i = e.from || {
type: u.$uo.$ve.f
}, o = h.default.Eq(t, i), n = h.default.Eq(t, e.to);
if (o && o.hw && n && n.hw) {
var r = u.$uo.TOWN_SCENE_DEFINE.SCENE_SCALE_Y, s = n.hw.x - o.hw.x, a = (n.hw.y - o.hw.y) * r, c = Math.sqrt(s * s + a * a);
e.extend && (c += e.extend);
var l = 180 * Math.atan2(a, s) / Math.PI, b = this.bus * this.but + 2 * this.buu, p = this.buh.node;
p.width = c;
p.height = b;
p.anchorX = 0;
p.anchorY = .5;
p.angle = l;
this.node.x = o.hw.x;
this.node.y = o.hw.y * r + (o.hw.z || 0);
this.bui.setProperty("nodeSize", cc.v2(c, b));
}
}
}
};
r([ f(cc.Sprite) ], e.prototype, "commonSprite", void 0);
r([ f(cc.Sprite) ], e.prototype, "negSprite", void 0);
r([ f(cc.Sprite) ], e.prototype, "fanSprite", void 0);
return r([ p ], e);
}(l.IVFXItem);
i.$kI = y;
cc._RF.pop();
}, {
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../battleLogic/$KL": void 0
} ],
$DRN: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7b739nic0tHhJhDEs2wKhEj", "$DRN");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/manager/$DTX"), h = cc._decorator, u = h.ccclass, b = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.btn_again = null;
e.progressBar_abyss = null;
e.lbl_abyss_progress = null;
e.buJ = !1;
return e;
}
e.prototype.kq = function() {};
e.prototype.ks = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF9(){this.kN(this.btn_again.node, this.buN, this);
this.btn_again.node.active = !1;
this.buJ = !1;
this.buO();
$fc.chapter.kl(l.$nxCHAPTER_EVENT$nv.b, this.buP, this);
return [ 2 ];
});
});
};
e.prototype.kw = function() {
$fc.chapter.kn(this);
};
e.prototype.kF = function() {
this.buK();
};
e.prototype.buK = function() {
var t = $fc.battleMgr.bBl.nx;
if (t) {
var e = t.hi.blc.fI;
if (e && !$fc.battleMgr.bBm.bAJ && t.hi.blc.hf === c.$fe.tblEnum.InstancePlayModeTableEnum.ABYSS) {
var i = t.hi.blc.gi(e.cYE);
if (null == i ? void 0 : i.hw) {
var o = e.cYF && e.cYz && this.buL(t.hw.x, t.hw.y, i.hw.x - 100, i.hw.y - 150, 200, 300);
this.buM(o);
}
}
}
};
e.prototype.buL = function(t, e, i, o, n, r) {
return t >= i && t <= i + n && e >= o && e <= o + r;
};
e.prototype.buM = function(t) {
if (this.buJ !== t) {
this.buJ = t;
this.btn_again.node.active = t;
}
};
e.prototype.buN = function() {
var t, e, i, o, n;
if (!$fc.battleMgr.bBm.bAJ) {
var r = null === (e = null === (t = $fc.battleMgr.bBm) || void 0 === t ? void 0 : t.bAu) || void 0 === e ? void 0 : e.instanceId, s = null === (n = null === (o = null === (i = $fc.battleMgr.bBm) || void 0 === i ? void 0 : i.bAu) || void 0 === o ? void 0 : o.svrInstanceParams) || void 0 === n ? void 0 : n.playMode;
if (r && s) {
var a = {
id: r,
playMode: s
};
$fc.battleMgr.bCf(a);
}
}
};
e.prototype.buO = function() {
var t, e = $fc.chapter.bYt(null === (t = $fc.battleMgr.bBm) || void 0 === t ? void 0 : t.bAL);
this.progressBar_abyss.progress = e;
this.lbl_abyss_progress.string = Math.floor(100 * Math.max(0, Math.min(e, 1))) + "%";
};
e.prototype.buP = function() {
this.buO();
};
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_again", void 0);
r([ b({
type: cc.ProgressBar,
displayName: ""
}) ], e.prototype, "progressBar_abyss", void 0);
r([ b({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl_abyss_progress", void 0);
return r([ u ], e);
}(c.$fe.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/manager/$DTX": void 0
} ],
$DRO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ded44IYS4xNEowmwHimGe+K", "$DRO");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kR = void 0;
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.buffIcon = null;
e.cdSprite = null;
e.buQ = "";
e.buR = NaN;
return e;
}
e.prototype.buS = function() {
if (!this.buffIcon) {
var t = this.node.getChildByName("icon");
t && (this.buffIcon = t.getComponent(cc.Sprite));
}
if (!this.cdSprite) {
var e = this.node.getChildByName("mask"), i = e && e.children.length > 0 ? e.children[0] : null;
i && (this.cdSprite = i.getComponent(cc.Sprite));
}
};
e.prototype.buT = function(t, e) {
this.buS();
if (this.buffIcon && this.buQ !== t) {
this.buQ = t;
this.lC(this.buffIcon, t);
}
if (this.cdSprite) if (e < 0) {
this.cdSprite.node.active && (this.cdSprite.node.active = !1);
this.buR = e;
} else {
this.cdSprite.node.active || (this.cdSprite.node.active = !0);
var i = cc.misc.clampf(e, 0, 1);
if (this.buR !== i) {
this.buR = i;
this.cdSprite.fillRange = i;
}
}
};
r([ l(cc.Sprite) ], e.prototype, "buffIcon", void 0);
r([ l(cc.Sprite) ], e.prototype, "cdSprite", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
var u = function() {
function t() {
this.buU = null;
this.buV = null;
this.buW = [];
}
t.prototype.buX = function(t, e) {
this.buU = t;
this.buV = e;
this.buV && this.buV.isValid && (this.buV.active = !1);
};
Object.defineProperty(t.prototype, "buY", {
get: function() {
return !!(this.buU && this.buU.isValid && this.buV && this.buV.isValid);
},
enumerable: !1,
configurable: !0
});
t.prototype.buZ = function(t) {
if (this.buY) {
for (var e = this.buW.length; e < t.length; e++) {
var i = cc.instantiate(this.buV);
i.active = !0;
this.buU.addChild(i);
var o = i.getComponent(h) || i.addComponent(h);
this.buW.push(o);
}
for (e = this.buW.length - 1; e >= t.length; e--) {
var n = this.buW[e];
n && n.node && n.node.isValid && n.node.destroy();
this.buW.splice(e, 1);
}
for (e = 0; e < t.length; e++) this.buW[e].buT(t[e].b, t[e].c);
}
};
t.prototype.bva = function() {
this.buZ([]);
};
return t;
}();
i.$kR = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "17ca3N+5L5OwpGVw/qJGWri", "$DRP");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kU = void 0;
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/$KT"), h = cc._decorator, u = h.ccclass, b = h.property;
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
t[t.d = 2] = "d";
})(i.$kU || (i.$kU = {}));
var p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.node_emojiContainer = null;
return e;
}
Object.defineProperty(e.prototype, "bvc", {
set: function(t) {
this.bvb = t;
},
enumerable: !1,
configurable: !0
});
e.prototype.kq = function() {
this.bvd = l.default.personal.personalData.getCurSettingEmoji();
};
e.prototype.ks = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF10(n){switch (n.label) {
case 0:
t = 0;
n.label = 1;

case 1:
return t < this.bvd.length ? (e = this.node_emojiContainer.children[t]) ? [ 3, 3 ] : [ 4, this.lA("game:res/prefab/common/emojiItem") ] : [ 3, 5 ];

case 2:
i = n.sent();
if (!this.isValid) return [ 2 ];
(e = cc.instantiate(i)).setParent(this.node_emojiContainer);
e.scale = .65;
n.label = 3;

case 3:
(o = e.getComponent(EmojiItem)).initEmoji(this.bvd[t]);
o.clickCallBack = this.bve.bind(this);
o.setBg(!1);
n.label = 4;

case 4:
t++;
return [ 3, 1 ];

case 5:
return [ 2 ];
}
});
});
};
e.prototype.bve = function(t) {
var e;
return s(this, void 0, void 0, function() {
var i;
return a(this,function AWAITF11(o){switch (o.label) {
case 0:
i = JSON.stringify({
type: 2,
chatType: 1,
chatData: t
});
return [ 4, $fc.battleServer.cRh(i) ];

case 1:
o.sent();
this.node.active = !1;
null === (e = this.bvb) || void 0 === e || e.call(this, t);
return [ 2 ];
}
});
});
};
r([ b({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_emojiContainer", void 0);
return r([ u ], e);
}(c.$fe.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0
} ],
$DRQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "17f04VWJMNKvoT4LgJJYOyC", "$DRQ");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kY = void 0;
var s, a = t("../../../bingo/src/$KG"), c = t("../render/$DQS"), l = t("../render/$DQT"), h = cc._decorator, u = h.ccclass, b = h.property, p = function(t, e) {
return e.g - t.g;
};
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(s = i.$kY || (i.$kY = {}));
var f = function() {
function t() {
this.bvf = null;
this.bvg = null;
this.bvh = null;
this.bvi = [];
}
t.prototype.bvj = function(t, e, i) {
this.bvf = t;
this.bvg = e;
this.bvh = i;
this.bvg && this.bvg.isValid && (this.bvg.active = !1);
};
Object.defineProperty(t.prototype, "bvk", {
get: function() {
return !!(this.bvf && this.bvf.isValid && this.bvg && this.bvg.isValid);
},
enumerable: !1,
configurable: !0
});
t.prototype.bvl = function(t) {
if (!this.bvk) return this.bvi;
for (var e = this.bvi.length; e < t; e++) {
var i = cc.instantiate(this.bvg);
i.active = !0;
this.bvf.addChild(i);
this.bvi.push(this.bvh(i));
}
for (e = this.bvi.length - 1; e >= t; e--) {
var o = this.bvi[e];
o && o.node && o.node.isValid && o.node.destroy();
this.bvi.splice(e, 1);
}
return this.bvi;
};
t.prototype.bvm = function() {
this.bvl(0);
};
return t;
}(), d = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.cell1Prefab = null;
e.cell2Prefab = null;
e.cell1Container = null;
e.cell2Container = null;
e.switchToggleContainer = null;
e.bvn = new f();
e.bvo = new f();
e.bvp = [];
e.bvq = [];
e.bvr = s.b;
e.bvs = !1;
return e;
}
e.prototype.kq = function() {
this.bvt();
};
e.prototype.bvt = function() {
if (!this.bvs) {
this.bvs = !0;
this.bvn.bvj(this.cell1Container ? this.cell1Container.node : null, this.cell1Prefab, function(t) {
return t.getComponent(c.default);
});
this.bvo.bvj(this.cell2Container ? this.cell2Container.node : null, this.cell2Prefab, function(t) {
return t.getComponent(l.default);
});
this.bvv();
this.bvw(this.bvu(), !1);
}
};
e.prototype.bvu = function() {
var t = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
if (t) for (var e = 0; e < t.length; e++) if (t[e] && t[e].isChecked) return e === s.c ? s.c : s.b;
return s.b;
};
e.prototype.bvv = function() {
var t = this, e = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
e && e.forEach(function(e, i) {
e && e.node.on("toggle", function() {
e.isChecked && t.bvw(i === s.c ? s.c : s.b, !1);
}, t);
});
};
e.prototype.bvw = function(t, e) {
void 0 === e && (e = !0);
this.bvr = t;
this.cell1Container && (this.cell1Container.node.active = t === s.b);
this.cell2Container && (this.cell2Container.node.active = t === s.c);
e && this.bvx(t);
};
e.prototype.bvx = function(t) {
var e = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
if (e) for (var i = 0; i < e.length; i++) e[i] && (e[i].isChecked = i === t);
};
e.prototype.bvy = function(t) {
this.bvt();
this.bvp = t || [];
this.bvz();
this.bvA();
};
e.prototype.bvz = function() {
for (var t, e = this.bvn.bvl(this.bvp.length), i = 0; i < e.length; i++) {
var o = this.bvp[i];
e[i].bpC({
c: o.c,
d: o.e,
e: o.f,
f: o.h || [],
b: o.i,
g: null !== (t = o.j) && void 0 !== t ? t : 0,
h: o.d
});
}
};
e.prototype.bvA = function() {
for (var t, e = 0, i = 0; i < this.bvp.length; i++) e += Math.max(0, this.bvp[i].g);
var o = this.bvq, n = this.bvp.length;
o.length = n;
for (i = 0; i < n; i++) o[i] = this.bvp[i];
o.sort(p);
var r = this.bvo.bvl(o.length);
for (i = 0; i < r.length; i++) {
var s = o[i], a = e > 0 ? Math.max(0, s.g) / e : 0;
r[i].bpH({
b: s.c,
c: s.d,
d: s.g,
e: a,
f: s.i,
g: null !== (t = s.j) && void 0 !== t ? t : 0
});
}
};
e.prototype.bvB = function() {
this.bvp = [];
this.bvn.bvm();
this.bvo.bvm();
};
r([ b(cc.Node) ], e.prototype, "cell1Prefab", void 0);
r([ b(cc.Node) ], e.prototype, "cell2Prefab", void 0);
r([ b(cc.Layout) ], e.prototype, "cell1Container", void 0);
r([ b(cc.Layout) ], e.prototype, "cell2Container", void 0);
r([ b(cc.ToggleContainer) ], e.prototype, "switchToggleContainer", void 0);
return r([ u ], e);
}(a.$fe.Component);
i.default = d;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../render/$DQS": "$DQS",
"../render/$DQT": "$DQT"
} ],
$DRR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "400fa8nzi9Njb6GM/gmVxWq", "$DRR");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = cc._decorator, c = a.ccclass, l = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.avatarbg = null;
e.avatar = null;
e.avatarbg2 = null;
e.line = null;
return e;
}
e.prototype.kq = function() {};
e.prototype.ks = function() {};
e.prototype.bvC = function(t, e, i) {
s.$fe.utils.bco(this.avatar, !i);
if (i) {
this.avatarbg.node.active = !0;
this.avatarbg2 && (this.avatarbg2.node.active = !1);
this.line.node.active = !1;
} else {
this.avatarbg.node.active = !1;
this.avatarbg2 && (this.avatarbg2.node.active = !0);
this.line.node.active = !0;
}
};
e.prototype.bvD = function(t, e) {
if (t._battleUI_sprite !== e) {
t._battleUI_sprite = e;
this.lz(new s.$fe.ResCfg(e, cc.SpriteFrame), function(i, o) {
i || t.isValid && t._battleUI_sprite === e && (t.spriteFrame = o.CM);
});
}
};
r([ l(cc.Sprite) ], e.prototype, "avatarbg", void 0);
r([ l(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ l(cc.Sprite) ], e.prototype, "avatarbg2", void 0);
r([ l(cc.Sprite) ], e.prototype, "line", void 0);
return r([ c ], e);
}(s.$fe.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0
} ],
$DRS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "33b70HR+JdEaK6TqtRwrQVT", "$DRS");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/$KT"), h = cc._decorator, u = h.ccclass, b = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.btn_pause = null;
e.btn_option_0 = null;
e.btn_option_1 = null;
e.btn_option_2 = null;
e.btn_option_3 = null;
e.lbl_progress = null;
e.btn_changeCamera = null;
e.bvE = [];
e.bvF = [ 1, 2, 4, 8 ];
return e;
}
e.prototype.kq = function() {};
e.prototype.ks = function() {
return s(this, void 0, void 0, function() {
var t = this;
return a(this,function AWAITF12(){this.kN(this.btn_pause.node, this.bvH, this);
this.kN(this.btn_changeCamera.node, this.bvI, this);
this.bvE = [ this.btn_option_0, this.btn_option_1, this.btn_option_2, this.btn_option_3 ];
this.bvF.forEach(function(e, i) {
var o = t.bvE[i];
o.node.getChildByName("lblTitle").getComponent(cc.Label).string = "x" + e;
o.node.on("click", function() {
l.default.Ji.mV = e;
t.bvG();
});
});
this.bvG();
this.btn_pause.node.children[1].getComponent(cc.Label).string = "暂停";
return [ 2 ];
});
});
};
e.prototype.kF = function() {
this.lbl_progress.string = l.default.Ji.mW + "/" + l.default.Ji.mw.bAK.length;
};
e.prototype.bvG = function() {
var t = this;
this.bvE.forEach(function(e, i) {
var o = t.bvF.indexOf(l.default.Ji.mV);
e.node.children[0].active = i !== o;
e.node.children[1].active = i === o;
});
};
e.prototype.bvH = function() {
l.default.Ji.mF = !l.default.Ji.mF;
l.default.Ji.mF && $fc.fguiMgr.cOd({
msgType: c.$fe.MsgBoxType.BattlePlaybackExitConfirm,
message: "是否退出战斗回放？",
btnOK: function() {
$fc.battleMgr.bBP();
},
btnCancel: function() {
l.default.Ji.mF = !1;
},
okStr: " 确定",
cancelStr: "取消"
});
this.bvG();
};
e.prototype.bvI = function() {
var t = l.default.Ji.nc.iu, e = l.default.Ji.ne.fF.findIndex(function(e) {
return e.cXL === t;
});
if (-1 !== e) {
e = (e + 1) % l.default.Ji.ne.fF.length;
var i = l.default.Ji.ne.fF[e].cXL;
l.default.Ji.nc.jZ(i);
}
};
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pause", void 0);
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_0", void 0);
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_1", void 0);
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_2", void 0);
r([ b({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_3", void 0);
r([ b(cc.Label) ], e.prototype, "lbl_progress", void 0);
r([ b(cc.Button) ], e.prototype, "btn_changeCamera", void 0);
return r([ u ], e);
}(c.$fe.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0
} ],
$DRT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4e496nVfq1EzImioNNqu/qx", "$DRT");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/battle/types/$DYS"), h = t("../../../game/src/$KT"), u = cc._decorator, b = u.ccclass, p = u.property, f = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.node_left = null;
e.list = null;
e.btn_pause = null;
e.lbl = null;
e.btn_record = null;
e.btn_resetStatus = null;
e.btn_option = null;
e.btn_start = null;
e.btn_stop = null;
e.node_right = null;
e.btn_pet = null;
e.btn_skill = null;
e.btn_bag = null;
e.node_centerTop = null;
e.node_time = null;
e.lbl_time = null;
e.bvJ = !1;
return e;
}
e.prototype.ks = function() {
this.kN(this.btn_pet.node, this.bvK, this);
this.kN(this.btn_skill.node, this.bvL, this);
this.kN(this.btn_bag.node, this.bvM, this);
this.kN(this.btn_resetStatus.node, this.bvN, this);
this.kN(this.btn_pause.node, this.bvO, this);
this.kN(this.btn_record.node, this.bvP, this);
this.kN(this.btn_start.node, this.bvR, this);
this.kN(this.btn_stop.node, this.bvS, this);
this.node_time.active = !1;
this.bvJ = !1;
this.bvT();
$fc.battleMgr.bBl.ni.byU(l.$uo.$vk.by, this.bvU, this);
$fc.battleMgr.bBl.nx && ($fc.battleMgr.bBl.nx.hv.beD = 1);
};
e.prototype.kw = function() {
$fc.battleMgr.bBl.ni.byW(this);
};
e.prototype.kF = function() {};
e.prototype.bvK = function() {
$fc.fguiMgr.cNA(h.default.Jf.PetPanel);
};
e.prototype.bvL = function() {
$fc.fguiMgr.cNA(h.default.Jf.SkillPanel);
};
e.prototype.bvM = function() {
$fc.fguiMgr.cNA(h.default.Jf.BagPanel);
};
e.prototype.bvN = function() {
if ($fc.battleMgr.bBl.nx) {
$fc.battleMgr.bBl.ne.hc($fc.battleMgr.bBl.nx);
$fc.battleMgr.bBl.nx.hv.beD = 1;
$fc.fguiMgr.cNt("重置状态");
}
};
e.prototype.bvO = function() {
var t = this;
$fc.battleMgr.bBl.mF = !0;
$fc.fguiMgr.cOe({
msgType: c.$fe.MsgBoxType.BattlePvpTrainingExitConfirm,
message: "确实退出战斗？",
okStr: "确定",
cancelStr: "取消",
btnOK: function() {
return s(t, void 0, void 0, function() {
return a(this,function AWAITF13(){$fc.battleMgr.bBQ();
return [ 2 ];
});
});
},
btnCancel: function() {
$fc.battleMgr.bBl.mF = !1;
}
});
};
e.prototype.bvP = function() {
this.bvQ();
};
e.prototype.bvQ = function() {
var t, e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw;
if (e) {
var i = {
b: e.bBe(),
c: $fc.battleMgr.bBl.ne ? $fc.battleMgr.bBl.ne.fk : 0
};
$fc.fguiMgr.cNA(h.default.Jf.DamagestatisticsPanel, i);
}
};
e.prototype.bvR = function() {
$fc.fguiMgr.cNA(h.default.Jf.PvpTrainingSettingPanel);
};
e.prototype.bvS = function() {
$fc.battleMgr.bBl.ne.he();
$fc.battleMgr.bBl.nb.bAk();
this.bvJ = !1;
this.bvT(!0);
};
e.prototype.bvT = function(t) {
void 0 === t && (t = !1);
this.btn_bag.node.active = !this.bvJ;
this.btn_skill.node.active = !this.bvJ;
this.btn_start.node.active = !this.bvJ;
this.btn_stop.node.active = this.bvJ;
t && $fc.battleMgr.bBl.ni.byY(l.$uo.$vk.bA, this.bvJ);
};
e.prototype.bvU = function(t) {
$fc.battleMgr.bBl.ne.hd(t, $fc.battleMgr.bBm.bAI);
this.bvJ = !0;
this.bvT(!0);
this.bvN();
};
r([ p({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_left", void 0);
r([ p({
type: cc.ScrollView,
displayName: ""
}) ], e.prototype, "list", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pause", void 0);
r([ p({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_record", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_resetStatus", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_start", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_stop", void 0);
r([ p({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_right", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pet", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_skill", void 0);
r([ p({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_bag", void 0);
r([ p({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_centerTop", void 0);
r([ p({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_time", void 0);
r([ p({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl_time", void 0);
return r([ b ], e);
}(c.$fe.Component);
i.default = f;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/battle/types/$DYS": void 0
} ],
$DRU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "d85f9hQYLZBKJOsGardUq2e", "$DRU");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/battle/$DSO"), h = t("../../../game/src/battle/types/$DYS"), u = t("../../../game/src/$KT"), b = t("../../battleLogic/$KL"), p = cc._decorator, f = p.ccclass, d = p.property, v = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.node_left = null;
e.list = null;
e.btn_pause = null;
e.lbl = null;
e.btn_record = null;
e.btn_resetStatus = null;
e.btn_option = null;
e.btn_summon = null;
e.btn_clear = null;
e.btn_cancel = null;
e.node_right = null;
e.btn_pet = null;
e.btn_skill = null;
e.btn_bag = null;
e.node_centerTop = null;
e.node_time = null;
e.lbl_time = null;
e.bvV = !1;
return e;
}
e.prototype.ks = function() {
this.kN(this.btn_pet.node, this.bvW, this);
this.kN(this.btn_skill.node, this.bvX, this);
this.kN(this.btn_bag.node, this.bvY, this);
this.kN(this.btn_resetStatus.node, this.bvZ, this);
this.kN(this.btn_pause.node, this.bwa, this);
this.kN(this.btn_record.node, this.bwb, this);
this.kN(this.btn_option.node, this.bwd, this);
this.kN(this.btn_summon.node, this.bwe, this);
this.kN(this.btn_clear.node, this.bwf, this);
this.kN(this.btn_cancel.node, this.bwg, this);
this.node_time.active = !1;
$fc.battleMgr.bBl.ni.byU(h.$uo.$vk.bu, this.bwh, this);
$fc.battleMgr.bBl.ni.byU(h.$uo.$vk.bw, this.bwj, this);
this.bwk();
$fc.battleMgr.bBl.nx && ($fc.battleMgr.bBl.nx.hv.beD = 1);
};
e.prototype.kw = function() {
$fc.battleMgr.bBl.ni.byW(this);
};
e.prototype.kF = function() {
this.bwi();
};
e.prototype.bvW = function() {
$fc.fguiMgr.cNA(u.default.Jf.PetPanel);
};
e.prototype.bvX = function() {
$fc.fguiMgr.cNA(u.default.Jf.SkillPanel);
};
e.prototype.bvY = function() {
$fc.fguiMgr.cNA(u.default.Jf.BagPanel);
};
e.prototype.bvZ = function() {
if ($fc.battleMgr.bBl.nx && !$fc.battleMgr.bBl.mw.bAH.bEy()) {
$fc.battleMgr.bBl.ne.gX($fc.battleMgr.bBl.nx, $fc.battleMgr.bBl.mw.bAH);
$fc.battleMgr.bBl.nx.hv.beD = 1;
$fc.fguiMgr.cNt("重置状态");
}
};
e.prototype.bwa = function() {
var t = this;
$fc.battleMgr.bBl.mF = !0;
$fc.fguiMgr.cOe({
msgType: c.$fe.MsgBoxType.BattleTrainingExitConfirm,
message: "确实退出战斗？",
okStr: "确定",
cancelStr: "取消",
btnOK: function() {
return s(t, void 0, void 0, function() {
return a(this,function AWAITF14(){$fc.battleMgr.bBQ();
return [ 2 ];
});
});
},
btnCancel: function() {
$fc.battleMgr.bBl.mF = !1;
}
});
};
e.prototype.bwb = function() {
$fc.battleMgr.bBl.mw.bAH.bEy() || this.bwc();
};
e.prototype.bwc = function() {
var t, e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw;
if (e) {
var i = {
b: e.bBe(),
c: $fc.battleMgr.bBl.ne ? $fc.battleMgr.bBl.ne.fk : 0
};
$fc.fguiMgr.cNA(u.default.Jf.DamagestatisticsPanel, i);
}
};
e.prototype.bwd = function() {
$fc.battleMgr.bBl.mw.bAH.bEy() || $fc.fguiMgr.cNA(u.default.Jf.PveTrainingSettingPanel);
};
e.prototype.bwe = function() {
var t, e, i, o, n;
if ($fc.battleMgr.bBl.ne.gF().length >= 6) $fc.fguiMgr.cNt("已达到召唤怪物上限"); else {
var r = null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAH;
if (r) {
var s = r.bEy();
if (!s || this.bvV) {
var a = null !== (i = r.bEm[r.enemyIndex.bEh]) && void 0 !== i ? i : 0;
if (a) {
var u = null !== (n = null === (o = r.bEn[r.enemyLevel.bEh]) || void 0 === o ? void 0 : o[0]) && void 0 !== n ? n : 1, p = a;
if (l.default.bEC.bEW(p)) {
var f = new h.$uo.$up();
f.cXs = p;
f.cXz = u;
f.cXD = 0;
f.cXB = 2;
f.cXr = 0;
f.cXC = l.default.bEC.bFJ($fc.cfgMgr.tbl.RoleTable.VOList[1].id, f.cXz);
f.cXC[c.$fe.tblEnum.AttributeTableEnum.hpMax] = 1e7;
var d = $fc.battleMgr.bBl.nx;
f.cXu = d.hw.x + 200 * d.hw.bkw;
f.cXv = d.hw.y;
f.cXw = 0;
f.cXy = -1 * d.hw.bkw;
var v = b.default.Gl($fc.battleMgr.bBl.ne, f, 0);
v.hv.beD = 1;
v.hv.bdX = 1;
1 === r.enemyFight.bEh && b.default.Es(v, 21001011);
1 === r.enemyCannotGrab.bEh && b.default.FN(v, 11012, null, v.hg, null);
1 === r.enemyStiff.bEh && b.default.FN(v, 11011, null, v.hg, null);
if (s) {
this.bvV = !1;
$fc.battleMgr.bBl.ne.gX($fc.battleMgr.bBl.nx, $fc.battleMgr.bBl.mw.bAH);
$fc.battleMgr.bBm.bBf();
$fc.battleMgr.bBl.ne.gZ();
$fc.battleMgr.bBl.ni.byY(h.$uo.$vk.bv);
this.bwk();
}
}
}
}
}
}
};
e.prototype.bwf = function() {
$fc.battleMgr.bBl.mw.bAH.bEy() || $fc.battleMgr.bBl.nb.bAk();
};
e.prototype.bwg = function() {
var t, e, i = null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAH;
if (i && $fc.battleMgr.bBl.mw.bAH.bEy()) {
$fc.battleMgr.bBl.ne.hb();
this.bvV = !1;
i.modeIndex.bEh = 0;
this.bwk();
}
};
e.prototype.bwh = function() {
var t, e, i, o, n = null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAH;
if (n && n.bEy()) {
var r = $fc.battleMgr.bBl.ne;
if (r) {
var s = null !== (o = null === (i = n.bEt[n.modeTimeLimitIndex.bEh]) || void 0 === i ? void 0 : i[0]) && void 0 !== o ? o : 0;
r.fn = s;
r.fk = 0;
$fc.battleMgr.bBl.nb.bAk();
$fc.battleMgr.bBl.ne.ha();
this.bvV = !0;
this.bwk();
this.bwi();
}
}
};
e.prototype.bwi = function() {
if (this.node_time.active) {
var t = $fc.battleMgr.bBl.ne;
if (t) {
var e = Math.floor(Math.max(0, t.fn - t.fk)), i = "";
if (e > 60) {
var o = e % 60;
i = Math.floor(e / 60) + ":" + o;
} else i = "" + e;
this.lbl_time.string = i;
}
}
};
e.prototype.bwj = function() {
var t, e, i = null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAH;
if (i) {
this.bwc();
this.bvV = !1;
i.modeIndex.bEh = 0;
this.bwk();
}
};
e.prototype.bwk = function() {
var t, e, i = null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAH;
if (i) {
var o = i.bEy();
this.btn_bag.node.active = !o;
this.btn_pet.node.active = !o;
this.btn_skill.node.active = !o;
this.node_time.active = o;
this.btn_clear.node.opacity = o ? 150 : 255;
this.btn_resetStatus.node.opacity = o ? 150 : 255;
this.btn_option.node.opacity = o ? 150 : 255;
this.btn_record.node.opacity = o ? 150 : 255;
$fc.battleMgr.bBl.nb.bzw = !this.bvV;
this.btn_clear.node.active = !o;
this.btn_cancel.node.active = o;
}
};
r([ d({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_left", void 0);
r([ d({
type: cc.ScrollView,
displayName: ""
}) ], e.prototype, "list", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pause", void 0);
r([ d({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_record", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_resetStatus", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_summon", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_clear", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_cancel", void 0);
r([ d({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_right", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pet", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_skill", void 0);
r([ d({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_bag", void 0);
r([ d({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_centerTop", void 0);
r([ d({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_time", void 0);
r([ d({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl_time", void 0);
return r([ f ], e);
}(c.$fe.Component);
i.default = v;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../../battleLogic/$KL": void 0
} ],
$DRV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "937bdIjLgRN1Y25Z8NwtYvG", "$DRV");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$lh = void 0;
var c, l = t("../../../game/src/perfab/battleUI/$FDG"), h = t("../../../bingo/src/$KG"), u = t("../../../game/src/$KT"), b = t("../../../game/src/$KU"), p = t("../../../game/src/manager/$DUP"), f = t("../../../game/src/battle/$DSC"), d = t("../../../bingo/src/utils/$DON"), v = t("../../../game/src/perfab/battleUI/$FCY"), y = t("./$DRX"), g = t("./$DRZ"), m = t("../render/$DRF"), _ = t("./$DRY"), $ = t("./$DRW"), w = t("./$DRQ"), D = t("./$DRU"), P = t("./$DRT"), R = t("./$DRN"), S = t("../../../game/src/perfab/battleUI/$FDD"), E = t("../../../game/src/perfab/battleUI/$FDC"), C = t("../../../game/src/perfab/common/$FDS"), O = t("../../../game/src/perfab/common/$FDT"), T = cc._decorator, I = T.ccclass, M = T.property;
(function(t) {
t[t.b = 1] = "b";
t[t.c = 2] = "c";
})(c = i.$lh || (i.$lh = {}));
var x = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bwl = [];
e.bwm = null;
e.joystickPrefab = null;
e.optBtnUIPrefab = null;
e.panelEnemyInfoPrefab = null;
e.panelSelfPlayerInfoPrefab = null;
e.battleGroupStatusPanelPrefab = null;
e.selfHero = null;
e.enemyHero = null;
e.battleTime = null;
e.lblLevel = null;
e.btnPause = null;
e.btnExpress = null;
e.baoXiang = null;
e.panelSkillConf = null;
e.emoticonFrame = null;
e.comboCount = null;
e.comboNode = null;
e.nodeDefeatLayer = null;
e.tipArrowArray = [];
e.btnAutoBattle = null;
e.node_autoTxt = null;
e.lbl_uid = null;
e.minmapNode = null;
e.nodeLeftTop = null;
e.nodePvpTop = null;
e.nodeTime = null;
e.nodePractice = null;
e.abyssInfoUI = null;
e.bwn = null;
e.bwo = null;
e.bwp = null;
e.bwq = null;
e.bwr = null;
e.bws = [];
e.bwt = null;
e.bwu = null;
e.bwv = null;
e.bww = null;
e.bwx = null;
e.bwy = null;
e.bwz = null;
e.bwA = cc.color();
e.bwB = new Map();
e.bwC = c.b;
e.uiSP = null;
e.uiSPDatas = [ null ];
e.bxi = null;
e.bxj = null;
e.bxk = !1;
e.bxn = "";
e.bxr = !1;
e.bxz = null;
e.bxE = null;
return e;
}
e.prototype.kq = function() {
var t, e;
null === (t = cc.MultTextures) || void 0 === t || t.addScope(this.node);
this.bwy = this.node.getChildByName("layerArrow");
var i = cc.instantiate(this.joystickPrefab);
this.node.addChild(i, -1, "joysitck");
this.bwm = i.getComponent(l.default);
var o = null === (e = $fc.skill.cqk.cqf(u.default.Ji.ne.gN ? h.$fe.tblEnum.SkillTypeTableEnum.PVP_SKILL : h.$fe.tblEnum.SkillTypeTableEnum.PVE_SKILL)) || void 0 === e ? void 0 : e.cWS;
if (o) {
this.bwm.node.scale = o.cWO;
this.bwm.node.x = o.cWM;
this.bwm.node.y = o.cWN;
} else {
this.bwm.node.scale = 1;
this.bwm.node.x = -.35 * cc.winSize.width;
this.bwm.node.y = -180;
}
u.default.Ji.nb.bzK(this.bwm);
this.bwM();
this.comboNode.opacity = 0;
this.kX("skillInsert").zIndex = 100;
this.uiSP.node.zIndex = 101;
this.kX("nodeTips").y = 120;
var n = cc.instantiate(this.optBtnUIPrefab);
this.node.addChild(n);
this.bwn = n.getComponent(v.default);
this.bwn.node.x = cc.winSize.width / 2 - $fc.fguiMgr.cOj;
this.bwn.node.y = -cc.winSize.height / 2;
this.kN(this.btnPause, this.bwD, this);
this.kN(this.nodePvpTop.getChildByName("btnPause"), this.bwD, this);
this.kN(this.kX("chatBtn"), this.bxu, this);
this.kN(this.btnAutoBattle, this.bxH, this);
$fc.eventMgr.kl(u.default.Jg.cDq, this.bwM, this);
this.abyssInfoUI.node.active = !1;
};
e.prototype.ks = function() {
var t, e, i, o;
this.lbl_uid.string = "" + (null !== (o = null === (i = null === (e = null === (t = u.default.Ji) || void 0 === t ? void 0 : t.mw) || void 0 === e ? void 0 : e.bAt) || void 0 === i ? void 0 : i.instUid) && void 0 !== o ? o : "");
this.lbl_uid.node.zIndex = 2e3;
};
e.prototype.kw = function() {
var t;
this.bwB.clear();
if (this.bxE) {
O.default.fFE(this.bxE, this.bxE.node);
this.bxE = null;
}
$fc.eventMgr.cGV(this);
cc.systemEvent.targetOff(this);
null === (t = cc.MultTextures) || void 0 === t || t.removeScope(this.node);
};
e.prototype.bwD = function() {
u.default.Ji.nk || $fc.fguiMgr.cNA(u.default.Jf.BattleSettingPanel);
};
e.prototype.bwE = function() {
if (!this.bwp) {
var t = cc.instantiate(this.panelEnemyInfoPrefab);
this.node.addChild(t);
t.x = -200;
t.y = cc.winSize.height / 2 - 30;
this.bwp = t.getComponent(y.default);
}
if (!this.bwq) {
t = cc.instantiate(this.panelSelfPlayerInfoPrefab);
this.node.addChild(t);
t.x = 0;
t.y = -cc.winSize.height / 2 + 50;
this.bwq = t.getComponent(g.default);
}
};
e.prototype.bwF = function() {
if (!(this.bws.length > 0)) {
var t = this.node.getChildByName("panelHero1");
t.active = !0;
var e = cc.instantiate(t);
this.node.addChild(e);
e.x = -t.x;
e.y = t.y;
e.scaleX = -1;
this.bws = [ t.getComponent(_.default), e.getComponent(_.default) ];
this.bws.forEach(function(t, e) {
t.byg(e);
});
}
};
e.prototype.bwG = function() {
if (!this.bwr) {
var t = cc.instantiate(this.battleGroupStatusPanelPrefab);
this.node.addChild(t);
this.bwr = t.getComponent(w.default);
}
};
e.prototype.bwH = function(t) {
this.bwC = t;
var e = this.kX("nodeCenterTop");
if (e) {
e.active = !0;
e.getChildByName("pause").active = t === c.b;
e.getChildByName("skip").active = t === c.c;
}
};
e.prototype.bwI = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF15(n){switch (n.label) {
case 0:
return (t = this.node.getChildByName("__GAMEDEBUGUI__")) ? [ 3, 2 ] : [ 4, this.lI(new h.$fe.ResCfg("editor:prefabs/EditorMainUI", cc.Prefab)) ];

case 1:
if ((e = n.sent()) && e.CM) {
i = e.CM;
if (!this.isValid) return [ 2 ];
if (this.node.getChildByName("__GAMEDEBUGUI__")) return [ 2 ];
o = cc.instantiate(i);
this.node.addChild(o, 9999, "__GAMEDEBUGUI__");
}
return [ 3, 3 ];

case 2:
t.active = !t.active;
n.label = 3;

case 3:
return [ 2 ];
}
});
});
};
e.prototype.bwJ = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF16(n){switch (n.label) {
case 0:
return (t = this.node.getChildByName("__GAMEGMUI__")) ? [ 3, 2 ] : [ 4, this.lI(new h.$fe.ResCfg("game:res/panel/battle/BattleGMUI", cc.Prefab)) ];

case 1:
e = n.sent();
i = e.CM;
if (this.isValid) {
if (this.node.getChildByName("__GAMEGMUI__")) return [ 2 ];
o = cc.instantiate(i);
this.node.addChild(o, 1e3, "__GAMEGMUI__");
} else 0 === i.refCount && i.decRef();
return [ 3, 3 ];

case 2:
t.active = !t.active;
n.label = 3;

case 3:
return [ 2 ];
}
});
});
};
e.prototype.bwK = function() {
this.bwB.clear();
};
e.prototype.bwL = function(t, e) {
cc.systemEvent.emit(t, e);
};
e.prototype.bwM = function() {
this.isValid && this.bwm && this.bwm.fBD($fc.localMgr.cjU(p.$oD.f) ? l.$PA.b : l.$PA.c);
};
e.prototype.bwN = function(t, e) {
return s(this, void 0, void 0, function() {
var i, o, n, r;
return a(this,function AWAITF17(){t > 1 && (t = 1);
if (!(i = this.bwl[t])) return [ 2 ];
(o = i.getChildByName("networkStatus")).active = !0;
n = cc.Color.RED;
e < 96 ? n = cc.Color.GREEN : e < 320 && (n = cc.Color.ORANGE);
(r = o.getChildByName("lbl").getComponent(cc.Label)).string = Math.min(e, 999) + "ms";
r.node.color = n;
return [ 2 ];
});
});
};
e.prototype.bwO = function(t, e) {
var i = this;
if (this.uiSPDatas[t]) {
this.uiSP.skeletonData = this.uiSPDatas[t];
this.uiSP.node.active = !0;
this.uiSP.premultipliedAlpha = !1;
var o = this.uiSP.setAnimation(0, e || "animation", !1);
this.uiSP.setTrackCompleteListener(o, function() {
i.uiSP.node.active = !1;
});
}
};
e.prototype.bwP = function(t, e, i) {
var o = this;
void 0 === e && (e = "animation");
void 0 === i && (i = !0);
if (t) {
this.uiSP.skeletonData = t;
this.uiSP.node.active = !0;
this.uiSP.premultipliedAlpha = !1;
var n = this.uiSP.setAnimation(0, e || "animation", !1);
this.unschedule(this.bwQ);
i ? this.uiSP.setTrackCompleteListener(n, function() {
o.uiSP.node.active = !1;
}) : this.scheduleOnce(this.bwQ, 1);
} else console.warn("no skeletonData");
};
e.prototype.bwQ = function() {
this.uiSP.node.active = !1;
};
e.prototype.bwR = function() {
this.uiSP.timeScale = 0;
};
e.prototype.bwS = function() {
this.uiSP.timeScale = 1;
};
e.prototype.bwT = function(t) {
cc.Tween.stopAllByTarget(this.comboNode);
if (t > 0) {
this.comboCount.string = "" + t;
this.comboNode.opacity = 255;
cc.tween(this.comboNode).to(.04, {
scale: 1.4
}).to(.04, {
scale: 1
}).start();
} else cc.tween(this.comboNode).to(.3, {
opacity: 0
}).start();
};
e.prototype.bwU = function(t) {
if (this.isValid && this.bwy) {
for (var e = 0; e < t.length; e++) {
var i = this.bwy.children[e], o = t[e];
if (!i) {
i = cc.instantiate(this.bwy.children[0]);
this.bwy.addChild(i, 99);
}
i.active = !0;
i.setPosition(o.b);
i.angle = 180 * o.c / Math.PI + 90;
i.getComponent(cc.Sprite).spriteFrame = this.tipArrowArray[o.d];
}
for (;e < this.bwy.children.length; e++) this.bwy.children[e].active = !1;
}
};
e.prototype.bwV = function() {};
e.prototype.bwW = function() {};
e.prototype.bwX = function() {};
e.prototype.bwY = function() {};
e.prototype.bwZ = function(t, e) {
if (t._battleUI_sprite !== e) {
t._battleUI_sprite = e;
this.lz(new h.$fe.ResCfg(e, cc.SpriteFrame), function(i, o) {
if (!i) {
var n = o.CM;
t.isValid && t._battleUI_sprite === e && (t.spriteFrame = n);
}
});
}
};
e.prototype.bxa = function(t, e, i, o) {
var n, r;
void 0 === o && (o = 100);
var s = this.bwB.get(t);
if (!s) {
var a = u.default.Ji.mZ.bDE(e);
if (!a) return;
this.node.addChild(a, o);
s = {
id: t,
node: a
};
this.bwB.set(t, s);
}
var c = this.node;
if (i.left) {
var l = i.left;
Math.abs(l) < 1 && (l = c.width * l);
s.node.x = -c.width / 2 + l;
}
if (i.right) {
l = i.right;
Math.abs(l) < 1 && (l = c.width * l);
s.node.x = c.width / 2 - l;
}
if (i.top) {
l = i.top;
Math.abs(l) < 1 && (l = c.height * l);
s.node.y = c.height / 2 - l;
}
if (i.bottom) {
l = i.bottom;
Math.abs(l) < 1 && (l = c.height * l);
s.node.y = -c.height / 2 + l;
}
i.offsetX && (s.node.x += i.offsetX);
i.offsetY && (s.node.y += i.offsetY);
if ("number" == typeof i.btn) {
var h = null === (n = this.bwn) || void 0 === n ? void 0 : n.fzh[i.btn];
if (null === (r = null == h ? void 0 : h.frame) || void 0 === r ? void 0 : r.node) {
var b = h.frame.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R), p = c.convertToNodeSpaceAR(b);
s.node.setPosition(p);
}
}
};
e.prototype.bxb = function(t, e, i) {
var o = this.bwB.get(t);
o && o.node.emit(e, i);
};
e.prototype.bxc = function(t) {
var e = this.bwB.get(t);
if (e) {
this.bwB.delete(t);
e.node.destroy();
}
};
e.prototype.bxd = function(t, e, i, o, n, r, s) {
var a = this.kX("skillInsert");
if (a) {
cc.Tween.stopAllByTarget(a);
a.active = !0;
cc.tween(a).delay(t).call(function() {
a.isValid && (a.active = !1);
}).start();
var c = a.children[0];
c.getComponent(cc.Sprite).spriteFrame = u.default.Ji.mZ.bDs(e);
c.x = i || 0;
c.y = o || 0;
c.angle = n || 0;
c.scaleX = r || 1;
c.scaleY = s || 1;
}
};
e.prototype.bxe = function(t) {
var e, i, o, n, r;
if (this.node.isValid) {
var s = this.node.getChildByName("nodeMask");
s.active = !0;
cc.Tween.stopAllByTarget(s);
switch (null !== (e = t.type) && void 0 !== e ? e : 0) {
case 0:
var a = .1;
s.color = f.BATTLE_COLOR_R.BLACK;
s.setAnchorPoint(.5, .5);
s.setPosition(0, 0);
cc.tween(s).set({
opacity: 255
}).delay(a).to(.3, {
opacity: 0
}).call(function() {
s.active = !1;
}).start();
s.zIndex = 999;
break;

case 1:
s.color = cc.Color.fromHEX(this.bwA, t.color);
s.setAnchorPoint(.5, .5);
s.opacity = 0;
var c = null !== (i = t.param[0]) && void 0 !== i ? i : .1, l = (a = null !== (o = t.param[1]) && void 0 !== o ? o : .1, 
null !== (n = t.param[2]) && void 0 !== n ? n : .1), h = null !== (r = t.param[3]) && void 0 !== r ? r : 255;
cc.tween(s).to(c, {
opacity: h
}).delay(a).to(l, {
opacity: 0
}).start();
s.zIndex = -1;
}
}
};
e.prototype.bxf = function(t, e, i, o) {
void 0 === o && (o = 0);
var n = this.bwl[t];
n.active = !0;
n.getChildByName("lblHeroName").getComponent(cc.Label).string = "" + i;
e && this.bwZ(n.getChildByName("avatar").getComponent(cc.Sprite), e);
n.getChildByName("lbl_uid").getComponent(cc.Label).string = "" + (o > 0 ? o : "");
};
e.prototype.bxg = function(t, e, i, o) {
void 0 === o && (o = !0);
var n = this.bwl[t], r = e / i, s = n.getChildByName("progHP").getComponent(cc.ProgressBar);
n.getChildByName("lblHP").getComponent(cc.Label).string = e + "/" + i;
if (s.progress !== r) {
var a = n.getChildByName("progHPback").getComponent(cc.ProgressBar);
cc.Tween.stopAllByTarget(a);
a.progress > r && o ? cc.tween(a).delay(2).to(.5, {
progress: r
}).start() : a.progress = r;
}
s.progress = r;
};
e.prototype.bxh = function(t, e, i, o) {
var n = this.bwl[t], r = e / i, s = n.getChildByName("progMP").getComponent(cc.ProgressBar);
n.getChildByName("lblMP").getComponent(cc.Label).string = e + "/" + i;
d.default.bco(s.barSprite, !o);
s.progress = r;
};
e.prototype.bxl = function() {};
e.prototype.bxm = function() {};
e.prototype.bxo = function(t, e, i) {
var o = this, n = this.kX("nodeTips"), r = n.getChildByName("txt1").getComponent(cc.RichText);
r.string = e;
if (n) {
this.bxn = t;
n.opacity = 255;
cc.Tween.stopAllByTarget(n);
cc.tween(n).show().set({
opacity: 0
}).to(.2, {
opacity: 255
}).delay(i).to(.2, {
opacity: 0
}).call(function() {
r.string = "";
o.bxn = "";
}).hide().start();
}
};
e.prototype.bxp = function(t, e) {
var i = this.kX("nodeTips");
i.getChildByName("txt1").getComponent(cc.RichText).string = e;
this.bxn = t;
if (i) {
i.opacity = 255;
cc.Tween.stopAllByTarget(i);
}
};
e.prototype.bxq = function() {
var t = this.kX("nodeTips");
if (t) {
t.opacity = 0;
cc.Tween.stopAllByTarget(t);
this.bxn = "";
}
};
e.prototype.bxs = function(t) {
if (this.bxr !== t) {
this.bxr = t;
this.battleTime.node.color = this.bxr ? f.BATTLE_COLOR_R.RED : f.BATTLE_COLOR_R.YELLOW2;
cc.Tween.stopAllByTarget(this.battleTime.node);
this.battleTime.node.opacity = 255;
this.bxr && cc.tween(this.battleTime.node).set({
opacity: 0
}).delay(.1).set({
opacity: 255
}).delay(.25).union().repeatForever().start();
}
};
e.prototype.bxt = function() {};
e.prototype.bxu = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF18(){this.node.getChildByName("BattleChatPanel");
return [ 2 ];
});
});
};
e.prototype.bxv = function(t, e) {
1 == e.chatType ? this.bxw(t, e.chatData) : this.bxx(t, e.chatData);
};
e.prototype.bxw = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF19(){return [ 2 ];
});
});
};
e.prototype.bxx = function() {};
e.prototype.bxy = function() {};
e.prototype.bxA = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF20(o){switch (o.label) {
case 0:
return this.bxz ? [ 3, 2 ] : [ 4, this.lI(new h.$fe.ResCfg("battleRes:prefabs/battle/BattlePlaybackUI", cc.Prefab)) ];

case 1:
t = o.sent();
e = t.CM;
if (this.isValid) {
if (this.bxz) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 1e3);
i.x = -.5 * this.node.width;
i.y = -.5 * this.node.height;
} else 0 === e.refCount && e.decRef();
return [ 3, 2 ];

case 2:
return [ 2 ];
}
});
});
};
e.prototype.bxB = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF21(o){switch (o.label) {
case 0:
return this.bwo ? [ 3, 2 ] : [ 4, this.lI(new h.$fe.ResCfg("game:res/battleRes/prefabs/battle/BattleScoringCompetitionUI", cc.Prefab)) ];

case 1:
t = o.sent();
e = t.CM;
if (this.isValid) {
if (this.bwo) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 1e3);
} else 0 === e.refCount && e.decRef();
return [ 3, 2 ];

case 2:
return [ 2 ];
}
});
});
};
e.prototype.bxC = function(t) {
this.btnAutoBattle.children[0].active = !t;
this.btnAutoBattle.children[1].active = t;
this.btnAutoBattle.children[2].getComponent(cc.Label).string = t ? "取消自动" : "自动战斗";
this.node_autoTxt.active = t;
};
e.prototype.bxD = function() {
if (!(this.node_autoTxt.getNumberOfRunningActions() > 0)) {
var t = this.node_autoTxt.getChildByName("circle");
cc.Tween.stopAllByTarget(t);
cc.tween(t).set({
angle: 0
}).to(1.5, {
angle: -360
}).union().repeatForever().start();
cc.Tween.stopAllByTarget(this.node_autoTxt);
for (var e = cc.tween(this.node_autoTxt), i = function(t) {
var i = t.y, o = i + 8;
e.delay(.2).call(function() {
cc.tween(t).set({
y: i
}).to(.15, {
y: o
}).to(.15, {
y: i
}).start();
});
}, o = 0, n = this.node_autoTxt.getChildByName("txt").children; o < n.length; o++) i(n[o]);
e.delay(.3).union().repeatForever().start();
}
};
e.prototype.bxF = function() {
if (this.btnAutoBattle.active && !u.default.Ji.nk && $fc.monCard.fsp.fsl() && $fc.funcOpen.cdr(h.$fe.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN, !1) && !$fc.localMgr.cjt()) {
if (!this.bxE) {
var t = $fc.prefabMgr.cLm(b.default.commonComp.movieClip2);
if (!t) return;
t.setPosition(0, 0);
this.btnAutoBattle.addChild(t, 100);
this.bxE = t.getComponent(C.default);
}
$fc.localMgr.cju(!0);
this.bxE.node.active = !0;
this.bxE.play(h.$fe.ResUrl.MC_3021, !0);
}
};
e.prototype.bxG = function() {
this.bxE && O.default.fFD(this.bxE);
};
e.prototype.bxH = function() {
var t, e, i;
if (u.default.Ji.nl()) {
var o = u.default.Ji.nw();
if (o && !u.default.Ji.nk) if ((null === (t = u.default.Ji.mw.bAO) || void 0 === t ? void 0 : t.d) || $fc.monCard.fsp.fsl()) {
this.bxG();
$fc.battleMgr.bBv = !o.cYg;
u.default.Ji.nb.bAh(!o.cYg);
h.$fe.tracking.cGe($fc.battleMgr.bBv ? 1 : 0, (null === (i = u.default.Ji.mw) || void 0 === i ? void 0 : i.bBh()) || "");
} else {
if (!$fc.funcOpen.cdr(h.$fe.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN, !1)) {
var n = $fc.cfgMgr.tbl.FunctionCondTable.findFirst(function(t) {
return t.funcId == h.$fe.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN;
});
if (n && (null === (e = n.taskIds) || void 0 === e ? void 0 : e.length) > 0) {
var r = $fc.cfgMgr.tbl.TaskTable.findByKey(n.taskIds[0]);
$fc.fguiMgr.cNt($fc.i18n.JN(null == r ? void 0 : r.desc));
}
return;
}
$fc.fguiMgr.cNt("激活特权后解锁");
}
}
};
e.prototype.bxI = function(t) {
this.bwz && (this.bwz.active = t);
};
e.prototype.bxJ = function(t) {
t && !(t.length <= 0) && this.bwz;
};
e.prototype.bxK = function(t) {
this.bwn && (this.bwn.node.active = t);
};
e.prototype.bxL = function() {
var t, e = null === (t = u.default.Ji) || void 0 === t ? void 0 : t.ne;
if (e) {
var i = e.fI;
if (i) {
this.minmapNode.parent.getChildByName("lblTitle").getComponent(cc.Label).string = $fc.i18n.JN(e.fK.name) || "";
for (var o = [ 3, 3 ], n = [ 1, 1 ], r = this.minmapNode.getChildByName("content"), s = r.children[0], a = s.width + 2, c = [], l = 1; l <= 4; l++) i.cYv[l] && c.push(i.cYR(l));
var h = -1, b = i.cYM ? i.cYR(i.cYM) : null, p = b ? e.gJ(b.x, b.y) : null;
for (l = 0; l < o[0]; l++) for (var f = function(t) {
var o = l - n[0], u = t - n[1], b = o + i.cYs, f = u + i.cYt;
h++;
var d = r.children[h];
d || ((d = cc.instantiate(s)).parent = r);
var v = e.gJ(b, f);
d.active = !!v;
if (v) {
var y = v === i;
d.active && (d.zIndex = y ? 1 : 0);
d.x = o * a;
d.y = u * a;
var g = d.getComponent(m.$kA), _ = -1 !== c.findIndex(function(t) {
return t.x == b && t.y == f;
}), $ = p === v;
g.bth(v, y, i.cYz, _, $);
}
}, d = 0; d < o[1]; d++) f(d);
}
}
};
e.prototype.bxM = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF22(o){switch (o.label) {
case 0:
return this.bwt ? [ 2 ] : [ 4, this.lI(new h.$fe.ResCfg("battleRes:prefabs/battle/ui/BattleWorldBossPanel", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.bwt) return [ 2 ];
(i = cc.instantiate(e)).x = .5 * this.node.width - 60;
i.y = .5 * this.node.height - 20;
this.node.addChild(i, 2);
this.bwt = i.getComponent($.default);
return [ 2 ];
}
});
});
};
e.prototype.bxN = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF23(o){switch (o.label) {
case 0:
return this.bwu ? [ 2 ] : [ 4, this.lI(new h.$fe.ResCfg("battleRes:prefabs/battle/ui/BattleTrainingUI", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.bwu) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 2);
this.bwu = i.getComponent(D.default);
return [ 2 ];
}
});
});
};
e.prototype.bxO = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF24(o){switch (o.label) {
case 0:
return this.bwv ? [ 2 ] : [ 4, this.lI(new h.$fe.ResCfg("battleRes:prefabs/battle/ui/BattlePvpTrainingUI", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.bwv) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 2);
this.bwv = i.getComponent(P.default);
return [ 2 ];
}
});
});
};
e.prototype.bxP = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF25(o){switch (o.label) {
case 0:
return this.bww ? [ 2 ] : [ 4, this.lI(new h.$fe.ResCfg("game:res/prefab/battle/BattleSkillGuidePanel", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.bww) return [ 2 ];
(i = cc.instantiate(e)).y = .5 * this.node.height - 150;
this.node.addChild(i, 2);
this.bww = i.getComponent(S.default);
return [ 2 ];
}
});
});
};
e.prototype.bxQ = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF26(o){switch (o.label) {
case 0:
return this.bwx ? [ 2 ] : this.bwn && this.bwn.node ? [ 4, this.lI(new h.$fe.ResCfg("game:res/prefab/battle/guide/BattleSkillClickTip", cc.Prefab)) ] : [ 2 ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.bwx) return [ 2 ];
if (!this.bwn || !this.bwn.node) return [ 2 ];
i = cc.instantiate(e);
this.bwn.node.addChild(i, 1e3);
this.bwx = i.getComponent(E.default);
return [ 2 ];
}
});
});
};
e.prototype.bxR = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF27(o){switch (o.label) {
case 0:
return [ 4, this.lI(new h.$fe.ResCfg("battleRes:prefabs/battle/other/AbyssEnterText", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.CM)) return [ 2 ];
if (!this.isValid) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 1e3);
cc.tween(i).set({
opacity: 0
}).delay(.1).to(.5, {
opacity: 255
}).delay(1.2).to(.5, {
opacity: 0
}).removeSelf().start();
return [ 2 ];
}
});
});
};
r([ M(cc.Prefab) ], e.prototype, "joystickPrefab", void 0);
r([ M(cc.Prefab) ], e.prototype, "optBtnUIPrefab", void 0);
r([ M(cc.Prefab) ], e.prototype, "panelEnemyInfoPrefab", void 0);
r([ M(cc.Prefab) ], e.prototype, "panelSelfPlayerInfoPrefab", void 0);
r([ M(cc.Prefab) ], e.prototype, "battleGroupStatusPanelPrefab", void 0);
r([ M(cc.Node) ], e.prototype, "selfHero", void 0);
r([ M(cc.Node) ], e.prototype, "enemyHero", void 0);
r([ M(cc.Label) ], e.prototype, "battleTime", void 0);
r([ M(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ M(cc.Node) ], e.prototype, "btnPause", void 0);
r([ M(cc.Node) ], e.prototype, "btnExpress", void 0);
r([ M(cc.Node) ], e.prototype, "baoXiang", void 0);
r([ M(cc.Node) ], e.prototype, "panelSkillConf", void 0);
r([ M(cc.Node) ], e.prototype, "emoticonFrame", void 0);
r([ M(cc.Label) ], e.prototype, "comboCount", void 0);
r([ M(cc.Node) ], e.prototype, "comboNode", void 0);
r([ M(cc.Node) ], e.prototype, "nodeDefeatLayer", void 0);
r([ M(cc.SpriteFrame) ], e.prototype, "tipArrowArray", void 0);
r([ M(cc.Node) ], e.prototype, "btnAutoBattle", void 0);
r([ M(cc.Node) ], e.prototype, "node_autoTxt", void 0);
r([ M(cc.Label) ], e.prototype, "lbl_uid", void 0);
r([ M(cc.Node) ], e.prototype, "minmapNode", void 0);
r([ M(cc.Node) ], e.prototype, "nodeLeftTop", void 0);
r([ M(cc.Node) ], e.prototype, "nodePvpTop", void 0);
r([ M(cc.Node) ], e.prototype, "nodeTime", void 0);
r([ M(cc.Node) ], e.prototype, "nodePractice", void 0);
r([ M(R.default) ], e.prototype, "abyssInfoUI", void 0);
r([ M(sp.Skeleton) ], e.prototype, "uiSP", void 0);
r([ M(sp.SkeletonData) ], e.prototype, "uiSPDatas", void 0);
return r([ I ], e);
}(h.$fe.Component);
i.default = x;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../bingo/src/utils/$DON": void 0,
"../../../game/src/$KT": void 0,
"../../../game/src/$KU": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/manager/$DUP": void 0,
"../../../game/src/perfab/battleUI/$FCY": void 0,
"../../../game/src/perfab/battleUI/$FDC": void 0,
"../../../game/src/perfab/battleUI/$FDD": void 0,
"../../../game/src/perfab/battleUI/$FDG": void 0,
"../../../game/src/perfab/common/$FDS": void 0,
"../../../game/src/perfab/common/$FDT": void 0,
"../render/$DRF": "$DRF",
"./$DRN": "$DRN",
"./$DRQ": "$DRQ",
"./$DRT": "$DRT",
"./$DRU": "$DRU",
"./$DRW": "$DRW",
"./$DRX": "$DRX",
"./$DRY": "$DRY",
"./$DRZ": "$DRZ"
} ],
$DRW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "3b718OKVV9DTprYt5MM3wzH", "$DRW");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
}, s = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, a = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("../../../bingo/src/$KG"), l = t("../../../game/src/battle/$DSO"), h = cc._decorator, u = h.ccclass, b = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblRecord = null;
e.lblName = null;
e.lblState = null;
e.lblLeftTime = null;
e.imgBox = null;
e.progressSprite = null;
e.bxS = 0;
return e;
}
e.prototype.ks = function() {
this.lblName.string = "";
this.bxS = 0;
};
e.prototype.kF = function() {
var t, e, i, o, n = l.default.bEC.bGi(), r = (null === (e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.ne) || void 0 === e ? void 0 : e.gS()) || 0, s = l.default.bEC.bGj(r), a = s === n[n.length - 1].id, c = this.bxS;
this.bxS = s;
s > c && this.bxU();
this.lblState.string = "阶段" + s;
if (a) this.bxT(1); else {
var h = (null === (i = n[s - 1]) || void 0 === i ? void 0 : i.damage) || 0, u = (r - h) / (((null === (o = n[s]) || void 0 === o ? void 0 : o.damage) || 0) - h);
this.bxT(u > 1e-4 ? u : 0);
}
this.lblRecord.string = "" + r;
this.bxW();
};
e.prototype.bxT = function(t) {
this.progressSprite.fillRange = t;
};
e.prototype.bxU = function() {
this.bxV();
};
e.prototype.bxV = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF28(){return [ 2 ];
});
});
};
e.prototype.bxW = function() {
var t, e = null === (t = $fc.battleMgr.bBl) || void 0 === t ? void 0 : t.ne;
if (e && e.fn) {
var i, o = Math.floor(Math.max(0, e.fn - e.fk)), n = o % 60, r = Math.floor(o / 60);
i = (r < 10 ? "0" + r : r) + ":" + (n < 10 ? "0" + n : n);
this.lblLeftTime.string = i;
}
};
r([ b(cc.Label) ], e.prototype, "lblRecord", void 0);
r([ b(cc.Label) ], e.prototype, "lblName", void 0);
r([ b(cc.Label) ], e.prototype, "lblState", void 0);
r([ b(cc.Label) ], e.prototype, "lblLeftTime", void 0);
r([ b(cc.Sprite) ], e.prototype, "imgBox", void 0);
r([ b(cc.Sprite) ], e.prototype, "progressSprite", void 0);
return r([ u ], e);
}(c.$fe.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSO": void 0
} ],
$DRX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "28635SOoEhGspua5e5dvyfr", "$DRX");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("../../../game/src/battle/$DSO"), c = t("../render/$DRA"), l = t("./$DRO"), h = cc._decorator, u = h.ccclass, b = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblName = null;
e.lblLevel = null;
e.spAvatar = null;
e.hpBar = null;
e.buffList = null;
e.buffItem = null;
e.bxX = null;
e.bxY = 0;
e.bxZ = -1;
e.bya = new l.$kR();
return e;
}
e.prototype.ks = function() {
this.bxX = this.hpBar.getComponent(c.default);
this.byb();
};
e.prototype.byb = function() {
this.bya.buX(this.buffList ? this.buffList.node : null, this.buffItem);
};
e.prototype.byc = function(t) {
this.bya.buZ(t);
};
e.prototype.byd = function(t, e) {
if (this.bxZ !== e) {
this.bxZ = e;
this.lblLevel.string = "Lv." + e;
}
if (this.bxY !== t) {
this.bxY = t;
var i = a.default.bEC.bEW(t);
if (i) {
this.lblName.string = $fc.i18n.JN(i.nameText);
this.lC(this.spAvatar, i.avatar);
}
}
};
e.prototype.bye = function() {
if (!this.node.getNumberOfRunningActions()) {
var t = Math.random() - .5, e = Math.random() - .5, i = 12 * (t + .2 * Math.sign(t)), o = 20 * (e + .2 * Math.sign(e));
cc.tween(this.node).by(.05, {
x: i,
y: o
}).by(.03, {
x: -i,
y: -o
}).start();
}
};
r([ b(cc.Label) ], e.prototype, "lblName", void 0);
r([ b(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ b(cc.Sprite) ], e.prototype, "spAvatar", void 0);
r([ b(cc.Node) ], e.prototype, "hpBar", void 0);
r([ b(cc.Layout) ], e.prototype, "buffList", void 0);
r([ b(cc.Node) ], e.prototype, "buffItem", void 0);
return r([ u ], e);
}(s.$fe.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSO": void 0,
"../render/$DRA": "$DRA",
"./$DRO": "$DRO"
} ],
$DRY: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7277aKfXLdKUbaG25i1igsM", "$DRY");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("../../../game/src/battle/$DSO"), c = cc._decorator, l = c.ccclass, h = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblLevel = null;
e.lblHP = null;
e.lblMP = null;
e.HPSprite = null;
e.MPSprite = null;
e.uid = null;
e.nick = null;
e.avatar = null;
e.backHurtProgressArrow = null;
e.flyHurtProgressArrow = null;
e.lieHurtProgressArrow = null;
e.byf = 0;
e.byi = -1;
e.byj = -1;
e.byk = -1;
e.byl = -1;
e.byq = 263;
return e;
}
e.prototype.kq = function() {
this.byq = this.HPSprite.node.width;
};
e.prototype.ks = function() {};
e.prototype.kv = function() {};
e.prototype.kF = function() {};
e.prototype.byg = function(t) {
this.byf = t;
if (1 === t) {
this.nick.node.anchorX = 1;
this.nick.node.scaleX = -1;
this.nick.horizontalAlign = cc.Label.HorizontalAlign.RIGHT;
this.uid.node.scaleX = -1;
this.lblHP.node.scaleX = -1;
this.lblMP && (this.lblMP.node.scaleX = -1);
this.lblLevel && (this.lblLevel.node.scaleX = -1);
}
};
e.prototype.byh = function(t, e, i, o) {
var n = a.default.bEC.bEW(e);
this.lC(this.avatar, n.avatar);
this.nick.string = i;
this.uid.string = "" + (o > 0 ? o : "");
};
e.prototype.bym = function(t, e) {
if (this.byi !== t || this.byj !== e) {
this.byi = t;
this.byj = e;
this.lblHP.string = t + "/" + e;
var i = 0 !== e ? t / e : 0;
this.byo(i);
}
};
e.prototype.byn = function(t, e) {
if (this.byk !== t || this.byl !== e) {
this.byk = t;
this.byl = e;
var i = 0 !== e ? t / e : 0;
this.byp(i);
}
};
e.prototype.byo = function(t) {
this.HPSprite.fillRange = t;
};
e.prototype.byp = function(t) {
this.MPSprite.fillRange = t;
};
e.prototype.byr = function(t, e, i) {
var o = 0;
if (t > 0) {
o = Math.max(t, 0) * this.byq;
this.backHurtProgressArrow.node.x = o;
this.backHurtProgressArrow.node.opacity = 255;
} else this.backHurtProgressArrow.node.opacity = 0;
if (e > 0) {
o = Math.max(e, 0) * this.byq;
this.flyHurtProgressArrow.node.x = o;
this.flyHurtProgressArrow.node.opacity = 255;
} else this.flyHurtProgressArrow.node.opacity = 0;
if (i > 0) {
o = Math.max(i, 0) * this.byq;
this.lieHurtProgressArrow.node.x = o;
this.lieHurtProgressArrow.node.opacity = 255;
} else this.lieHurtProgressArrow.node.opacity = 0;
};
r([ h(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ h(cc.Label) ], e.prototype, "lblHP", void 0);
r([ h(cc.Label) ], e.prototype, "lblMP", void 0);
r([ h(cc.Sprite) ], e.prototype, "HPSprite", void 0);
r([ h(cc.Sprite) ], e.prototype, "MPSprite", void 0);
r([ h(cc.Label) ], e.prototype, "uid", void 0);
r([ h(cc.Label) ], e.prototype, "nick", void 0);
r([ h(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ h(cc.Sprite) ], e.prototype, "backHurtProgressArrow", void 0);
r([ h(cc.Sprite) ], e.prototype, "flyHurtProgressArrow", void 0);
r([ h(cc.Sprite) ], e.prototype, "lieHurtProgressArrow", void 0);
return r([ l ], e);
}(s.$fe.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSO": void 0
} ],
$DRZ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "821c3cK+ANF7ZnGaT3Y8idh", "$DRZ");
var o, n = this && this.__extends || (o = function(t, e) {
return (o = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
o(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), r = this && this.__decorate || function(t, e, i, o) {
var n, r = arguments.length, s = r < 3 ? e : null === o ? o = Object.getOwnPropertyDescriptor(e, i) : o;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) s = Reflect.decorate(t, e, i, o); else for (var a = t.length - 1; a >= 0; a--) (n = t[a]) && (s = (r < 3 ? n(s) : r > 3 ? n(e, i, s) : n(e, i)) || s);
return r > 3 && s && Object.defineProperty(e, i, s), s;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var s = t("../../../bingo/src/$KG"), a = t("./$DRO"), c = cc._decorator, l = c.ccclass, h = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblLevel = null;
e.lblHP = null;
e.lblMP = null;
e.HPSprite = null;
e.MPSprite = null;
e.buffList = null;
e.buffItem = null;
e.bys = new a.$kR();
e.byt = -1;
e.byu = -1;
e.byv = -1;
e.byw = -1;
e.byx = -1;
return e;
}
e.prototype.kq = function() {
this.buffItem || (this.buffItem = this.node.getChildByName("buffNode"));
this.bys.buX(this.buffList ? this.buffList.node : null, this.buffItem);
};
Object.defineProperty(e.prototype, "byy", {
get: function() {
this.buffItem || (this.buffItem = this.node.getChildByName("buffNode"));
return this.buffItem;
},
enumerable: !1,
configurable: !0
});
e.prototype.byz = function(t) {
if (this.byx !== t) {
this.byx = t;
this.lblLevel.string = t.toString();
}
};
e.prototype.byA = function(t, e) {
if (this.byt !== t || this.byu !== e) {
this.byt = t;
this.byu = e;
this.lblHP.string = t + "/" + e;
var i = 0 !== e ? t / e : 0;
this.byC(i);
}
};
e.prototype.byB = function(t, e) {
if (this.byv !== t || this.byw !== e) {
this.byv = t;
this.byw = e;
var i = 0 !== e ? t / e : 0;
this.byD(i);
}
};
e.prototype.byC = function(t) {
this.HPSprite.fillRange = t;
};
e.prototype.byD = function(t) {
this.MPSprite.fillRange = t;
};
e.prototype.byE = function(t) {
this.bys.buZ(t);
};
r([ h(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ h(cc.Label) ], e.prototype, "lblHP", void 0);
r([ h(cc.Label) ], e.prototype, "lblMP", void 0);
r([ h(cc.Sprite) ], e.prototype, "HPSprite", void 0);
r([ h(cc.Sprite) ], e.prototype, "MPSprite", void 0);
r([ h(cc.Layout) ], e.prototype, "buffList", void 0);
r([ h(cc.Node) ], e.prototype, "buffItem", void 0);
return r([ l ], e);
}(s.$fe.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$KG": void 0,
"./$DRO": "$DRO"
} ],
$KQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "0ead7OHxpRBh42JtoPK3BRM", "$KQ");
var o = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, n = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$gx = i.$gw = i.$gv = i.$gu = i.$gt = void 0;
var r = t("../../bingo/src/$KG"), s = t("../../game/src/perf/$DVH"), a = t("../../game/src/$KU"), c = t("../../game/src/perfab/common/$FDV"), l = t("../../game/src/utils/$DVY"), h = t("../battleLogic/$KL"), u = t("../battleLogic/math/$DPR"), b = t("../../game/src/battle/$DSO"), p = t("../../game/src/battle/types/$DYS"), f = function() {
function t() {}
Object.defineProperty(t.prototype, "me", {
get: function() {
var t;
return null === (t = this.lX) || void 0 === t ? void 0 : t.ne;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(t.prototype, "mf", {
get: function() {
var t;
return null === (t = this.lW) || void 0 === t ? void 0 : t.jM;
},
enumerable: !1,
configurable: !0
});
return t;
}();
i.$gt = f;
var d = function() {
function t() {
this.GT = 0;
this.GU = 0;
this.GV = [];
this.GW = [];
this.GX = 0;
this.GY = 0;
this.GZ = 0;
}
t.prototype.Ha = function(t) {
this.GY = 0;
this.GX = t;
};
return t;
}();
i.$gu = d;
i.$gv = function() {
this.Hc = [];
this.Hf = 1;
this.Hg = !0;
this.Hj = 1;
this.Hk = 1;
this.Hl = 1;
this.Hm = 1;
};
var v = function() {
function t() {
this.HD = new Map();
this.HE = !0;
this.HF = new Map();
this.HG = new Map();
this.HH = new Map();
this.HK = null;
this.HL = 0;
this.HM = !1;
this.HN = !1;
this.HO = !1;
this.HP = null;
this.HQ = null;
this.HR = null;
this.HS = new Map();
this.HT = null;
this.HU = null;
this.HV = null;
this.HW = {};
}
t.prototype.HX = function() {
return this.Hn.nf.gi(this.Ho);
};
t.prototype.HY = function() {
return this.Hn.ne.gi(this.Ho);
};
t.prototype.HZ = function() {
return this.Hn.mC ? this.HN && !this.HM ? Math.min(3.2 * this.Hn.ne.fp, this.Hn.nF() / 1e3) : Math.min(this.Hn.mL, .5) : Math.min(3.2 * this.Hn.ne.fp, this.Hn.nF() / 1e3);
};
t.prototype.Ia = function() {
var t, e = this.Hn.ne.gi(this.Ho);
e && (this.HM = -1 !== (null === (t = e.hE) || void 0 === t ? void 0 : t.bhJ));
};
t.prototype.Ib = function() {};
t.prototype.Ic = function(t) {
if (t.Hi) {
this.Hn.mZ.bDO(t.Hh, t.Hi.node);
t.Hi = null;
}
this.HF.delete(t.Hb);
};
t.prototype.Id = function() {
var t, e = this;
if (this.HF.size > 0) {
this.HF.forEach(function(t) {
e.Ic(t);
});
this.HF.clear();
}
if (this.HI) {
if (this.HI.brN) {
this.HI.brL ? this.Hn.mZ.bDO(this.HI.brN.resUrl, this.HI.brN.node) : this.HI.brN.node.destroy();
this.HI.brN = null;
}
this.Hn.mZ.bDO(r.$fe.ResUrl.EntityRender, this.HI.node);
this.HI.brM = null;
this.HI = null;
}
this.Ik();
this.Il();
this.Im();
if (this.HH) {
this.HH.forEach(function(t) {
t.Hi && t.Hi.node.destroy();
});
this.HH.clear();
}
this.HW = null;
this.HG.forEach(function(t) {
t.Hi && t.Hi.node.destroy();
});
if (null === (t = this.HJ) || void 0 === t ? void 0 : t.isValid) {
this.HJ.fGI(l.CC_GROUP_INDEX.UI);
$fc.prefabMgr.cLo(a.default.commonComp.petModel, this.HJ.node);
}
this.HJ = null;
if (this.HU) {
this.HU.isValid && this.Hn.mZ.bDO(r.$fe.ResUrl.BattleCastBar, this.HU.node);
this.HU = null;
}
};
t.prototype.Ie = function() {
var t, e, i = null === (t = this.Hq) || void 0 === t ? void 0 : t.topPos;
return null !== (e = i ? i[1] : 0) && void 0 !== e ? e : 0;
};
t.prototype.If = function() {
var t, e, i = null === (t = this.Hq) || void 0 === t ? void 0 : t.topPos;
return (null !== (e = i ? i[1] : 0) && void 0 !== e ? e : 0) + p.$uo.TOWN_SCENE_DEFINE.TITLE_TOP_OFFSET;
};
t.prototype.Ig = function() {
return this.Ie() + 80;
};
t.prototype.Ih = function() {
return o(this, void 0, void 0, function() {
var t;
return n(this,function AWAITF29(e){switch (e.label) {
case 0:
return [ 4, $fc.prefabMgr.cLn(a.default.commonComp.petModel) ];

case 1:
if (!(t = e.sent())) return [ 2 ];
if (!this.HL) return [ 2 ];
if (this.HJ) return [ 2 ];
t.scale = 1;
this.Hn.nc._in.sceneLayer.addChild(t, p.$uo.TOWN_SCENE_DEFINE.ROLE_ZORDER);
this.HJ = t.getComponent(c.$Ql);
this.HJ.fGI(l.CC_GROUP_INDEX.BATTLE);
this.HJ.fGJ(this.HL);
return [ 2 ];
}
});
});
};
t.prototype.Ii = function(t) {
var e, i, o;
if (this.HJ) {
this.HJ.fGA !== this.HL && this.HJ.fGJ(this.HL);
var n = this.HJ.fGB;
if (n) {
var r = n.pet, a = this.HJ.node, l = this.Hu + this.Hx * (null !== (e = r.x) && void 0 !== e ? e : -150), h = this.Hv + (null !== (i = r.y) && void 0 !== i ? i : 0), u = null !== (o = r.z) && void 0 !== o ? o : 0, f = .15;
if (!this.HK) {
this.HK = {
b: l,
c: h,
d: u,
e: this.Hx,
f: 0
};
f = 1;
}
if (this.HK.f) {
this.HK.f = 0;
f = 1;
this.HK.b = l;
this.HK.c = h;
this.HK.d = u;
}
var d = l - this.HK.b, v = h - this.HK.c, y = Math.abs(d), g = Math.abs(v), m = 0;
if (y > 30) {
var _ = Math.min(5 * y, 2e3);
m = Math.sign(d) * _;
}
var $ = 0;
g > 50 && ($ = (_ = Math.min(5 * g, 2e3)) * Math.sign(v));
var w = 0;
if (m) {
w = t * m;
Math.abs(w) > y ? this.HK.b = l : this.HK.b += w;
}
if ($) {
w = t * $;
Math.abs(w) > g ? this.HK.c = h : this.HK.c += w;
}
var D = Math.abs(Math.abs(m)) + Math.abs($) > 200;
D && Math.abs(m) > 350 ? this.HK.e = m > 0 ? p.$uo.$uU.b : p.$uo.$uU.c : this.HK.e !== this.Hx && (this.HK.e = this.Hx);
this.HJ.fGP(D ? c.$Qn.d : c.$Qn.c);
this.HJ.fGW(this.HK.e);
a.x = this.Ij(a.x, this.HK.b, f);
a.y = this.Ij(a.y, b.default.bEC.bFu(this.HK.c, this.HK.d), f);
s.HeatIsolate.cwo || (a.zIndex = b.default.bEC.bFF(this.HK.c));
}
} else this.Ih();
};
t.prototype.Ij = function(t, e, i) {
if (1 === i) return e;
var o = e - t;
return Math.abs(o) < 4 ? e : t + o * i;
};
t.prototype.Ik = function() {
if (this.HP) {
this.HP.isValid && this.Hn.mZ.bDO(r.$fe.ResUrl.EntityShadow, this.HP.node);
this.HP = null;
}
};
t.prototype.Il = function() {
if (this.HQ) {
this.HQ.isValid && $fc.prefabMgr.cLp(this.HQ.node);
this.HQ = null;
}
};
t.prototype.Im = function() {
if (this.HR) {
if (this.HR.isValid) {
this.HR.btU();
this.Hn.mZ.bDO(r.$fe.ResUrl.RoleTopContent, this.HR.node);
}
this.HR = null;
}
if (this.HT) {
this.HT.isValid && this.Hn.mZ.bDO(r.$fe.ResUrl.RoleTopProtectProgress, this.HT.node);
this.HT = null;
}
};
t.prototype.In = function(t) {
this.Hy = u.$jB.blW(t);
this.Hz = t * Math.PI / 180;
this.HB = h.default.FY(t) ? 1 : -1;
};
t.prototype.Io = function(t, e) {
this.Hy = u.$jB.blW(t);
this.Hz = t * Math.PI / 180;
this.HB = e;
};
t.prototype.Ip = function() {
if (this.HI) {
var t = this.HI.node, e = this.HY();
if (null == e ? void 0 : e.hw) {
this.Hu = e.hw.x;
this.Hv = e.hw.y;
this.Hw = e.hw.z;
t.x = this.Hu;
t.y = b.default.bEC.bFu(this.Hv, this.Hw);
}
}
};
return t;
}();
i.$gw = v;
var y = function() {
function t() {
this.Iy = !0;
this.Iz = !1;
}
t.prototype.IE = function() {};
t.prototype.IF = function() {
if (this.IC) {
this.IC.isValid && this.Iq.mZ.bDO(r.$fe.ResUrl.DropItemRender, this.IC.node);
this.IC = null;
}
if (this.ID) {
this.ID.isValid && this.Iq.mZ.bDO(r.$fe.ResUrl.DropItemText, this.ID.node);
this.ID = null;
}
};
t.prototype.IG = function() {};
t.prototype.IH = function() {
var t, e;
if (null === (t = this.IC) || void 0 === t ? void 0 : t.isValid) {
var i = this.IC.node;
i.x = this.Is;
i.y = b.default.bEC.bFu(this.It, this.Iu);
if (null === (e = this.ID) || void 0 === e ? void 0 : e.isValid) {
var o = this.ID.node;
o.x = this.Is;
o.y = b.default.bEC.bFu(this.It, this.Iu) - 40;
}
}
};
return t;
}();
i.$gx = y;
i.default = function() {
this.II = null;
this.IJ = 0;
this.IK = 0;
this.IL = cc.v2();
this.IM = 0;
this.IN = cc.color();
this.IO = 255;
this.IP = 660;
this.IQ = 0;
this.IR = 0;
this.IS = 0;
this.IT = 0;
this.IU = 0;
this.IV = .05;
this.IW = .12;
};
cc._RF.pop();
}, {
"../../bingo/src/$KG": void 0,
"../../game/src/$KU": void 0,
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../game/src/perf/$DVH": void 0,
"../../game/src/perfab/common/$FDV": void 0,
"../../game/src/utils/$DVY": void 0,
"../battleLogic/$KL": void 0,
"../battleLogic/math/$DPR": void 0
} ],
$KR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5ff5aK2+8JOwpmTKQaiMa7V", "$KR");
var o = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
c(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
c(o.throw(t));
} catch (t) {
r(t);
}
}
function c(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
c((o = o.apply(t, e || [])).next());
});
}, n = this && this.__generator || function(t, e) {
var i, o, n, r, s = {
label: 0,
sent: function() {
if (1 & n[0]) throw n[1];
return n[1];
},
trys: [],
ops: []
};
return r = {
next: a(0),
throw: a(1),
return: a(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function a(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(r) {
if (i) throw new TypeError("Generator is already executing.");
for (;s; ) try {
if (i = 1, o && (n = 2 & r[0] ? o.return : r[0] ? o.throw || ((n = o.return) && n.call(o), 
0) : o.next) && !(n = n.call(o, r[1])).done) return n;
(o = 0, n) && (r = [ 2 & r[0], n.value ]);
switch (r[0]) {
case 0:
case 1:
n = r;
break;

case 4:
s.label++;
return {
value: r[1],
done: !1
};

case 5:
s.label++;
o = r[1];
r = [ 0 ];
continue;

case 7:
r = s.ops.pop();
s.trys.pop();
continue;

default:
if (!(n = s.trys, n = n.length > 0 && n[n.length - 1]) && (6 === r[0] || 2 === r[0])) {
s = 0;
continue;
}
if (3 === r[0] && (!n || r[1] > n[0] && r[1] < n[3])) {
s.label = r[1];
break;
}
if (6 === r[0] && s.label < n[1]) {
s.label = n[1];
n = r;
break;
}
if (n && s.label < n[2]) {
s.label = n[2];
s.ops.push(r);
break;
}
n[2] && s.ops.pop();
s.trys.pop();
continue;
}
r = e.call(t, s);
} catch (t) {
r = [ 6, t ];
o = 0;
} finally {
i = n = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$gz = void 0;
var r = t("../../game/src/$KT"), s = t("../../bingo/src/$KG"), a = (t("../battleLogic/$KL"), 
t("../battleLogic/component/$DPL")), c = t("../../game/src/battle/types/$DYQ"), l = t("../../game/src/battle/types/$DYS"), h = t("./compnents/$DQK"), u = t("./$KQ"), b = t("./compnents/$DQP"), p = t("./compnents/$DQQ"), f = t("../../game/src/battle/$DSM"), d = t("./ui/$DRV"), v = t("../../bingo/src/utils/$DON"), y = t("../../game/src/battle/$DSO"), g = t("../../game/src/battle/$DSC"), m = t("./compnents/$DQO"), _ = t("./compnents/$DQL"), $ = t("../../game/src/utils/$DVY"), w = t("./render/$DRD"), D = t("./render/$DRC"), P = t("./render/$DRG"), R = t("./render/$DRK"), S = t("../../main/src/$KV"), E = t("../../game/src/manager/$DUP"), C = t("./compnents/$DQN"), O = t("../../game/src/manager/role/$FBX"), T = t("../../game/src/perf/$DVH"), I = function() {
function t(t) {
var e = this;
this.io = [];
this.iq = new Map();
this.ir = [];
this.is = new Map();
this.it = cc.v2();
this.iu = -1;
this.iv = !1;
this.iw = 102400;
this.ix = 324e4;
this.iy = 200;
this.iz = [];
this.iA = 0;
this.iB = !1;
this.iC = new Map();
this.iD = null;
this.iQ = "";
this.jc = 1;
this.jE = [];
this.jS = function(t) {
e.ip.mZ.bDO(s.$fe.ResUrl.fightText, t);
};
this.ip = t;
this.iD = new u.default();
}
t.prototype.iE = function() {
var t = this;
$fc.adMgr.Tg();
if ($fc.adMgr.Rv() && !$fc.platform.cne.cmS.isReportScene) {
var e = $fc.adMgr.Rf();
if (e && e.enter_scene && -1 != e.enter_scene.indexOf("3041") && "2" == e.feed_game_channel) {
$fc.platform.cne.cmS.isReportScene = !0;
var i = Math.max(Math.ceil(Date.now() - s.$fe.startTimer), 1e3);
$fc.adMgr.Uk(i);
}
}
var o = cc.find("Canvas/GameRoot"), n = o.getChildByName("battleScene");
if (!n) {
var a = this.ip.mZ.bDs(s.$fe.ResUrl.battleScene);
if (!a) return;
n = cc.instantiate(a);
o.addChild(n, -1, "battleScene");
}
this._in || (this._in = n.getComponent(f.default));
this.iQ = "";
this.iR();
this.iu = this.ip.mz;
this.ip.ne.fF.forEach(function(e) {
var i = new u.$gu();
i.GR = e.cXL;
i.GS = e.cXZ;
t.is.set(e.cXL, i);
});
this.it.x = cc.winSize.width;
this.it.y = cc.winSize.height;
this.jG();
this._in.bDT();
this._in.bDU();
this.jN();
this.jI();
$fc.localMgr.cjU(E.$oD.e);
this.iv = this.ip.mx;
this.io.push(new h.$jX());
this.io.push(new b.$ke());
this.io.push(new p.$kf());
$fN.ClassCollisionDebugDrawHelper && this.io.push(new $fN.ClassCollisionDebugDrawHelper());
switch (this.ip.ne.fM.f) {
case c.$uj.b:
this.io.push(new C.$kc());
this.io.push(new m.$kd());
this.io.push(new _.$ka());
break;

case c.$uj.c:
case c.$uj.f:
case c.$uj.g:
this.io.push(new _.$ka());
}
this.io.forEach(function(e) {
if (!e.lW) {
e.lX = r.default.Ji;
e.lW = t;
e.lY();
}
});
this.ip.ni.byU(l.$uo.$vk.x, this.iZ, this);
this.ip.ni.byU(l.$uo.$vk.B, this.ja, this);
this.ip.ni.byU(l.$uo.$vk.C, this.iY, this);
this.ip.ni.byU(l.$uo.$vk.D, this.je, this);
this.ip.ni.byU(l.$uo.$vk.K, this.jf, this);
this.ip.ni.byU(l.$uo.$vk.L, this.jg, this);
this.ip.ni.byU(l.$uo.$vk.H, this.jl, this);
this.ip.ni.byU(l.$uo.$vk.I, this.jm, this);
this.ip.ni.byU(l.$uo.$vk.z, this.jv, this);
this.ip.ni.byU(l.$uo.$vk.J, this.jh, this);
this.ip.ni.byU(l.$uo.$vk.T, this.jj, this);
this.ip.ni.byU(l.$uo.$vk.E, this.ji, this);
this.ip.ni.byU(l.$uo.$vk.V, this.jn, this);
this.ip.ni.byU(l.$uo.$vk.U, this.jk, this);
this.ip.ni.byU(l.$uo.$vk.f, this.jo, this);
this.ip.ni.byU(l.$uo.$vk.be, this.jp, this);
this.ip.ni.byU(l.$uo.$vk.bf, this.jq, this);
this.ip.ni.byU(l.$uo.$vk.bg, this.jr, this);
this.ip.ni.byU(l.$uo.$vk.bh, this.js, this);
this.ip.ni.byU(l.$uo.$vk.X, this.ju, this);
this.ip.ni.byU(l.$uo.$vk.i, this.jw, this);
this.ip.ni.byU(l.$uo.$vk.m, this.jx, this);
this.ip.ni.byU(l.$uo.$vk.n, this.jy, this);
this.ip.ni.byU(l.$uo.$vk.Z, this.jz, this);
this.ip.ni.byU(l.$uo.$vk.ba, this.jA, this);
this.ip.ni.byU(l.$uo.$vk.bi, this.jB, this);
this.ip.ni.byU(l.$uo.$vk.p, this.jD, this);
this.ip.ni.byU(l.$uo.$vk.bn, this.jC, this);
this.ip.ni.byU(l.$uo.$vk.br, this.jY, this);
this.ip.ni.byU(l.$uo.$vk.s, this.jX, this);
$fc.role.kl(O.$Ol.$Oq.c, this.jW, this);
};
t.prototype.iF = function() {
var t, e;
this.io.forEach(function(t) {
if (t.lW) {
t.lZ();
t.lW = null;
}
});
this.io.length = 0;
this.iq.clear();
this.ir.length = 0;
this.is.clear();
this.jM = null;
null === (t = this._in) || void 0 === t || t.bDR();
null === (e = this._in) || void 0 === e || e.bDV();
this.iC.clear();
this.ip.ni.byW(this);
this.ip.nj.byW(this);
$fc.battleServer.kn(this);
$fc.role.kn(this);
this.iB = !1;
$fc.role.fqy(!1);
};
t.prototype.iG = function(t) {
this.io.forEach(function(e) {
return e.mb(t);
});
};
t.prototype.iH = function(t) {
var e = this;
this.iq.forEach(function(t, i) {
e.ip.ne.gi(i) || e.iO(t);
});
this.io.forEach(function(e) {
return e.mc(t);
});
};
t.prototype.iI = function() {
this.io.forEach(function(t) {
return t.md();
});
this.iJ();
this.jG();
this.iR();
this.iS();
};
t.prototype.iJ = function() {
var t = this;
this.is.forEach(function(e) {
var i = t.ip.ne.gk(e.GR);
i && (e.GS = i.cXZ);
});
};
t.prototype.iK = function(t) {
var e = this, i = this.ip.ne;
if (i) {
this.iq.forEach(function(t, o) {
i.gi(o) || e.iO(t);
});
this.io.forEach(function(e) {
return e.ma(t);
});
this.iR();
this.jM || this.jN();
this.jL(t);
}
};
t.prototype.iL = function(t) {
var e = this.ip.ne.gi(t);
return e ? this.iM(e) : null;
};
t.prototype.iM = function(t) {
var e;
if (t.hF) {
var i = t.hg, o = t.hF.bgb, n = this.iq.get(i);
if (n && n.Hp !== o) if (n.HN) {
var r = n.HY();
if (r) {
o = null === (e = (t = r).hF) || void 0 === e ? void 0 : e.bgb;
if (n.Hp !== o) {
this.iO(n);
n = null;
}
}
} else {
this.iO(n);
n = null;
}
if (!n) {
var s = t, a = !1;
n = new u.$gw();
if (t.hx && t.hx.bjK) {
a = t.hx.bjK === this.ip.mz;
n.HN = !a && this.ip.mx;
}
n.HO = a;
n.Hn = this.ip;
n.Ho = i;
n.Hp = t.hF.bgb;
n.Hq = t.hF.bgd;
n.Ia();
n.HN && (s = n.HY() || s);
var c = s.hw;
n.Hu = c.x;
n.Hv = c.y;
n.Hw = c.z;
n.Hx = c.bkw;
n.Hy = c.bkx;
n.HC = c.bky;
this.iq.set(i, n);
}
return n;
}
};
t.prototype.iN = function(t) {
return this.iq.get(t);
};
t.prototype.iO = function(t) {
t.Id();
this.iq.delete(t.Ho);
};
t.prototype.iP = function() {
if (0 !== this.ir.length) {
for (var t = 0, e = this.ir; t < e.length; t++) e[t].IF();
this.ir.length = 0;
}
};
t.prototype.iR = function() {
var t = this.ip.ne.gt(a.default).bkg;
if (this.iQ !== t) {
var e = this.ip.ne.gt(a.default).bkr;
if (e) {
var i = r.default.Ji.mZ.bDs(e.resUrl);
if (i) {
this.iQ = t;
var o = this._in.sceneLayer.getChildByName("mapNode");
o && o.destroy();
this.iP();
var n = cc.instantiate(i);
this._in.sceneLayer.addChild(n, 0, "mapNode");
var s = this._in.mainCamera;
s.depth = 30;
s.node.active = !0;
s.backgroundColor = cc.Color.TRANSPARENT;
s.node.setPosition(0, 0);
for (var c = s.node.parent, l = c.children, h = 1; h < l.length; h++) l[h].destroy();
this.iz = [ {
camera: s,
motion: 1,
depth: 30,
group: 1
} ];
for (var u = 0, b = n.children; u < b.length; u++) {
var p = b[u], f = p.name.split("_");
if ("layer" === f[0]) {
var d = Number(f[1]), v = Number(f[2] || 0);
if (0 == d) p.groupIndex = 1; else {
var y = this.iz.length + 1;
p.groupIndex = y;
p.x = .5 * this.it.x * (v - 1);
var g = cc.instantiate(s.node);
c.addChild(g);
if (!g) break;
g.groupIndex = y;
var m = g.getComponent(cc.Camera);
m.cullingMask = 1 << y;
m.depth = 30 + d;
this.iz.push({
camera: m,
motion: v,
depth: 30 + d,
group: y
});
}
} else p.active = !1;
}
this.iS();
}
}
}
};
t.prototype.iS = function() {
var t, e = null === (t = this.ip.ne) || void 0 === t ? void 0 : t.gt(a.default);
if (e && 0 !== e.bkq.length) {
var i = this._in.sceneLayer.getChildByName("mapNode");
if (i) for (var o = 0, n = i.children; o < n.length; o++) {
var r = n[o], s = r.name.split("_");
if ("layer" === s[0]) {
var c = Number(s[1]);
0 !== c && (-1 !== e.bkq.indexOf(c) ? r.active = !0 : r.active = !1);
}
}
}
};
t.prototype.iT = function(t) {
return t.hx && t.hx.bjK === this.iu;
};
t.prototype.iU = function(t) {
var e, i, o, n = null === (e = t.hx) || void 0 === e ? void 0 : e.bjK;
if (!n) {
var r = this.ip.ne.gi(null === (i = t.hE) || void 0 === i ? void 0 : i.bhF);
r && (n = null === (o = r.hx) || void 0 === o ? void 0 : o.bjK);
}
return n === this.iu;
};
t.prototype.iV = function(t) {
var e, i, o, n, r, a = t;
(null === (e = a.hx) || void 0 === e ? void 0 : e.bjK) || (a = this.ip.ne.gi(null === (i = t.hE) || void 0 === i ? void 0 : i.bhF));
var c = null === (o = null == a ? void 0 : a.hx) || void 0 === o ? void 0 : o.bjK;
if (!c) return !1;
if (c === this.iu) return !1;
var l = null === (n = this.ip.nw()) || void 0 === n ? void 0 : n.cXQ;
return !!s.$fe.utils.bck(l) && (null === (r = a.hD) || void 0 === r ? void 0 : r.biI) === l;
};
t.prototype.iW = function() {
return this.ip.ne.gi(this.iu);
};
t.prototype.iX = function() {
this.jM && this.jM.bxe({
type: 0
});
};
t.prototype.iY = function() {
this.ip.mT || this.ip.mU;
};
t.prototype.iZ = function(t) {
var e, i;
if (!this.ip.mT && !this.ip.mU) {
var o = null === (i = null === (e = t.hv) || void 0 === e ? void 0 : e.bdQ) || void 0 === i ? void 0 : i.deadSound;
o && this.ip.na.byM(o[Math.floor(o.length * Math.random())]);
var n = this.iN(t.hg);
(null == n ? void 0 : n.HI) && n.HI.bsO();
}
};
t.prototype.ja = function(t) {
this.ip.mT || this.ip.mU || this.jd(t);
};
t.prototype.jb = function() {
this.ip.mT;
};
t.prototype.jd = function(t) {
var e, i, o, n, s, a, c, h, u;
if (!T.HeatIsolate.cwm) {
var b = t.fromEntity, p = t.atkEntity, f = t.target, d = t.finalDamage;
if (b.hw) {
var v = $fc.localMgr.cjU(E.$oD.e), g = t.hitInfo, m = null === (e = t.hitInfo) || void 0 === e ? void 0 : e.vfx, _ = Date.now();
if (f && (!(P = this.iC.get(f.hg)) || _ - P >= 1e3 * this.jc)) {
this.iC.set(f.hg, _);
var $ = null === (o = null === (i = f.hv) || void 0 === i ? void 0 : i.bdQ) || void 0 === o ? void 0 : o.hitSound;
$ && this.ip.na.byM($[Math.floor($.length * Math.random())]);
}
var w = t.hitInfo.audio;
if (w) {
var D = null !== (n = w.minInterval) && void 0 !== n ? n : .03;
if (b) {
var P;
if (!(P = this.iC.get(b.hg)) || _ - P >= 1e3 * D) {
this.iC.set(b.hg, _);
var R = w.name;
if (Array.isArray(R)) {
var C = R[Math.floor(R.length * Math.random())];
C && this.ip.na.byM(C);
} else this.ip.na.byM(R);
}
}
}
if (g.shakeCamera) {
var O = this.is.get(this.iu);
if (O && ((null === (s = null == f ? void 0 : f.hx) || void 0 === s ? void 0 : s.bjK) === this.iu || (null === (a = null == p ? void 0 : p.hx) || void 0 === a ? void 0 : a.bjK) === this.iu)) {
O.Ha(g.shakeCamera);
r.default.Ji.nB(g.shakeCamera);
}
}
var I = null !== (h = null === (c = t.fromEntity.hw) || void 0 === c ? void 0 : c.bkw) && void 0 !== h ? h : 1;
if (null == m ? void 0 : m.id) {
var M = null !== (u = m.angle) && void 0 !== u ? u : 0;
m.angleRand && (M += m.angleRand * Math.random());
var x = m.id;
m.id2 > 0 && Math.random() > .5 && (x = m.id2);
this.jU(x, t.hitX, t.hitY - 4, t.hitZ, I, M);
}
t.elementValue & l.$uo.$uJ.b && this.jU(S.cvResUrl.vfx_fire, t.hitX, t.hitY - 4, t.hitZ, I, 0);
t.elementValue & l.$uo.$uJ.c && this.jU(S.cvResUrl.vfx_ice, t.hitX, t.hitY - 4, t.hitZ, I, 0);
t.elementValue & l.$uo.$uJ.d && this.jU(S.cvResUrl.vfx_light, t.hitX, t.hitY - 4, t.hitZ, I, 0);
t.elementValue & l.$uo.$uJ.e && this.jU(S.cvResUrl.vfx_dark, t.hitX, t.hitY - 4, t.hitZ, I, 0);
v && d && this.jO(t.hitX, y.default.bEC.bFu(t.hitY, t.hitZ), d + "", t.isCrt);
}
}
};
t.prototype.je = function(t, e, i, o) {
o.hw;
};
t.prototype.jf = function() {
this.ip.mT || this.ip.mU;
};
t.prototype.jg = function() {
this.ip.mT || this.ip.mU;
};
t.prototype.jh = function(t, e) {
if (!this.ip.mT && !this.ip.mU) {
var i = 0;
switch (e) {
case 1:
i = 90;
break;

case 1:
i = -90;
break;

case 1:
i = 180;
break;

case 1:
i = 0;
}
var o = t.hw;
this.jU(8, o.x, o.y, i, 0, 1);
}
};
t.prototype.ji = function(t, e) {
var i;
if (!this.ip.mT && $fc.localMgr.cjU(E.$oD.e)) {
var o = this.ip.ne.gi(t.hg);
if (o) {
var n = this.iN(o.hg);
if (n && n.HI) {
var r = null === (i = n.Hq) || void 0 === i ? void 0 : i.topPos, s = r ? r[1] : 0;
e >= 0 ? this.jT(n.HI.node.x, n.HI.node.y + s, Math.abs(e) + "", 4) : this.jT(n.HI.node.x, n.HI.node.y + s, Math.abs(e) + "", 3);
}
}
}
};
t.prototype.jj = function(t, e) {
this._in && this.jM.bxd(e.time, e.url, e.x, e.y, e.angle, e.sx, e.sy);
};
t.prototype.jk = function(t) {
if (this._in) {
var e = this.ip.mZ.bDs(t[0]);
e && this.jM.bwP(e, t[1], t[2]);
}
};
t.prototype.jl = function(t, e, i) {
return o(this, void 0, void 0, function() {
var o, r, s;
return n(this,function AWAITF30(n){switch (n.label) {
case 0:
if (this.ip.mU) return [ 2 ];
o = 0;
if (!Array.isArray(e)) return [ 3, 2 ];
r = e[Math.floor(e.length * Math.random())];
return [ 4, this.ip.na.byM(r) ];

case 1:
o = n.sent();
return [ 3, 4 ];

case 2:
return [ 4, this.ip.na.byM(e) ];

case 3:
o = n.sent();
n.label = 4;

case 4:
if (!i) return [ 2 ];
if (!o) return [ 2 ];
if (!(s = this.iN(t.hg))) return [ 2 ];
s.HW[i] = o;
return [ 2 ];
}
});
});
};
t.prototype.jm = function(t, e) {
return o(this, void 0, void 0, function() {
var i, o;
return n(this,function AWAITF31(){if (!e) return [ 2 ];
i = this.iN(t.hg);
if (!(o = null == i ? void 0 : i.HW[e])) return [ 2 ];
$fc.audioMgr.cGy(o);
return [ 2 ];
});
});
};
t.prototype.jn = function(t) {
if (3 === t[1]) this.ip.na.byO(); else if (1 === t[1]) this.ip.na.byN(t[0]); else {
if (!t[0]) return 0;
this.ip.na.byM(t[0]);
}
};
t.prototype.jo = function() {};
t.prototype.jp = function(t, e) {
var i = e[2];
this.jM.bxa(e[0], e[1], i);
};
t.prototype.jq = function(t, e) {
var i = this.jM, o = e;
if (e[2]) {
o = v.default.bcq(e[2]);
if (Array.isArray(o)) for (var n = t.hH.bhm, r = 0; r < o.length; r++) {
var s = o[r];
"string" == typeof s && (o[r] = s.replace(/#{([A-Za-z]+)}/, function(t, e) {
return n[e];
}));
}
}
i.bxb(e[0], e[1], o);
};
t.prototype.jr = function(t, e) {
var i;
null === (i = this.jM) || void 0 === i || i.bxc(e[0]);
};
t.prototype.js = function(t) {
var e;
null === (e = this.jM) || void 0 === e || e.bwY(t[0], t[1]);
};
t.prototype.jt = function(t) {
var e;
null === (e = this.jM) || void 0 === e || e.bxm(t);
};
t.prototype.ju = function(t, e) {
var i;
e.isOnlySelf && !this.iT(t) || null === (i = this.jM) || void 0 === i || i.bxe(e);
};
t.prototype.jv = function(t) {
this.ip.mU || t.hx && this.iT(t) && this.jM.bwT(t.hv.bep);
};
t.prototype.jw = function() {
this.jV();
$fc.battleMgr.bBm.bBj() && this.jM.bxR();
};
t.prototype.jx = function() {};
t.prototype.jy = function() {
this.iR();
this.jG();
this.iq.forEach(function(t) {
var e;
(null === (e = t.HI) || void 0 === e ? void 0 : e.isValid) && t.Ip();
});
this.iX(0);
this.jV();
};
t.prototype.jz = function() {
this.iS();
};
t.prototype.jA = function(t, e) {
var i = this.jM;
i.isValid && i.bxo(t.hg + "_" + e[0], e[2], e[3]);
};
t.prototype.jB = function(t, e) {
var i, o = this.jM;
if (o.isValid) {
var n = 0;
if (null === (i = this.ip.ne) || void 0 === i ? void 0 : i.gN) {
n = 1 === this.ip.mA ? 0 : 1;
n = t ? n : 0 === n ? 1 : 0;
}
o.bxv(n, e);
}
};
t.prototype.jC = function(t, e) {
var i, o, n, r, s, a, c, l, h, u, b, p = this.iN(t.hg);
if (p) {
var f = null === (i = p.HI) || void 0 === i ? void 0 : i.mainNode;
if (f) {
var d = p.HI.node, v = this.iD;
v.IN.r = null !== (o = e.r) && void 0 !== o ? o : 255;
v.IN.g = null !== (n = e.g) && void 0 !== n ? n : 255;
v.IN.b = null !== (r = e.b) && void 0 !== r ? r : 255;
v.IO = null !== (s = e.a) && void 0 !== s ? s : 200;
v.IP = null !== (a = e.size) && void 0 !== a ? a : 660;
v.IR = 280;
v.IS = null !== (c = e.shaderType) && void 0 !== c ? c : 0;
v.IT = null !== (l = e.blend) && void 0 !== l ? l : 0;
v.IU = null !== (h = e.offsetZIndex) && void 0 !== h ? h : 0;
v.IV = null !== (u = e.t1) && void 0 !== u ? u : .05;
v.IW = null !== (b = e.t2) && void 0 !== b ? b : .12;
this.jK(f, d.x, d.y, d.zIndex, this.iD);
}
}
};
t.prototype.jD = function() {
if (!this.ip.mT && !this.ip.mU) {
var t = this.ip.ne.fI;
if (t) {
var e = t.cYN[t.cYN.length - 1], i = this.ip.ne.gi(e);
if (i) {
var o = {
id: 1999,
resType: g.DISPLAY_RES_TYPE.SPINE,
resUrl: "battleRes:skeleton/deadLine/dead",
anim: "dead",
autoDestroy: 1,
sx: 1,
sy: 1,
loop: !1,
timeScale: 1.2,
x: 0,
y: 0,
syncMainTime: !1
};
i.hi.blc.gu(l.$uo.$vk.F, i, o);
}
}
}
};
t.prototype.jF = function() {};
t.prototype.jG = function() {
var t, e = null === (t = this.ip.ne) || void 0 === t ? void 0 : t.fT;
e && this.is.forEach(function(t) {
var i = {
b: 0,
c: 0,
d: 0,
e: 0,
f: e.type,
g: t.GS,
h: e.fixedZoomRatio,
i: e.cameraLerpRatio
};
t.GV = [ i ];
});
};
t.prototype.jH = function() {};
t.prototype.jI = function() {
this._in.uiLayer.opacity = 255;
0 !== this.iA && (this.iA = 0);
};
t.prototype.jJ = function(t) {
var e = this.is.get(this.iu);
if (e) {
e.Ha(t);
r.default.Ji.nB(t);
}
};
t.prototype.jK = function(t, e, i, o, n) {
var r = this, a = n.IQ, c = n.IR, l = f.default.bEe(t, n.IP, a, c, 1.6), h = this.ip.mZ.bDE(s.$fe.ResUrl.ghost);
if (h) {
h.opacity = n.IO;
h.groupIndex = $.CC_GROUP_INDEX.BATTLE;
h.zIndex = o + n.IU;
h.x = e + a;
h.y = i + c;
h.parent = this._in.sceneLayer;
h.setScale(1.6);
var u = h.getComponent(w.default);
u.btd(n.IS, n.IN);
u.btc(l);
u.bte(n.IT);
cc.Tween.stopAllByTarget(h);
cc.tween(h).delay(n.IV).to(n.IW, {
opacity: 0
}).call(function() {
r.ip.mZ.bDO(s.$fe.ResUrl.ghost, h);
}).start();
}
};
t.prototype.jL = function() {
this._in.bDP || this._in.bDU();
var t = this.ip.ne.gt(a.default).bkp.length > 0;
this._in.bDX(t);
};
t.prototype.jN = function() {
if (!this.jM) {
var t = r.default.Ji.mZ.bDs(s.$fe.ResUrl.BattleUI);
if (t) {
var e = cc.instantiate(t), i = $fc.fguiMgr.cOi();
this._in.uiLayer.setContentSize(i.width, i.height);
this._in.uiLayer.setPosition(this._in.uiLayer.parent.convertToNodeSpaceAR(cc.Canvas.instance.node.convertToWorldSpaceAR(cc.v2())));
this._in.uiLayer.destroyAllChildren();
this._in.uiLayer.addChild(e);
this.jM = e.getComponent(d.default);
}
}
};
t.prototype.jO = function(t, e, i, o) {
this.jT(t, e, i, o ? 2 : 1);
};
t.prototype.jR = function(e, i) {
var o, n = null === (o = this._in) || void 0 === o ? void 0 : o.mainCamera;
if (!n || !n.isValid) return !0;
var r = n.zoomRatio || 1, s = .5 + t.jP, a = this.it.x / r * s, c = this.it.y / r * s, l = n.node;
return Math.abs(e - l.x) <= a && Math.abs(i - l.y) <= c;
};
t.prototype.jT = function(t, e, i, o) {
if (this.jR(t, e)) {
var n = this.ip.mZ.bDE(s.$fe.ResUrl.fightText);
if (n) {
n.zIndex = l.$uo.TOWN_SCENE_DEFINE.FIGHT_TEXT;
n.setPosition(t, e);
n.getComponent(D.default).bta(i, this.jS, o, 0, 1);
n.parent = this._in.sceneLayer;
}
}
};
t.prototype.jU = function(t, e, i, o, n, r, s) {
var a;
void 0 === s && (s = l.$uo.TOWN_SCENE_DEFINE.ROLE_ZORDER);
var c = null === (a = this._in) || void 0 === a ? void 0 : a.sceneLayer;
if (c) {
var h = this.ip.mZ, u = y.default.bEC.bFD(t);
if (u) {
var b, p = r, f = e + (u.posX || 0) * n, d = y.default.bEC.bFu(i, o) + (u.posY || 0), v = (u.scaleX || 1) * n, m = u.scaleY || 1, _ = s;
_ === l.$uo.TOWN_SCENE_DEFINE.ROLE_ZORDER && (_ = y.default.bEC.bFF(i));
var $ = y.default.bEC.bFG(u.resType) || u.resUrl;
if (!(b = h.bDE($))) return this;
c.addChild(b, _);
b.setPosition(f, d);
switch (u.resType) {
case g.DISPLAY_RES_TYPE.MOVIE_CLIP:
b.angle = p * n;
b.scaleX = v;
b.scaleY = m;
var w = b.getComponent(P.default);
w.setTimeScale(1);
w.btN(u.movieClipId, !1, function() {
h.bDO($, b);
});
break;

case g.DISPLAY_RES_TYPE.SPINE:
b.angle = p * n;
b.scaleX = v;
b.scaleY = m;
var D = b.getComponent(R.$kG);
D.setTimeScale(1);
D.mk(Boolean(u.premultAlpha));
D.mm(u.resUrl, u.animName, !1, function() {
h.bDO($, b);
});
break;

case g.DISPLAY_RES_TYPE.PREFAB:
b.angle = p;
b.scaleX = v;
b.scaleY = m;
var S = b.getComponent(g.IVFXItem);
S.prefabPlay && S.prefabPlay(function() {
h.bDO($, b);
});
}
return this;
}
}
};
t.prototype.jV = function() {
var t, e, i, o = null === (e = null === (t = this.ip) || void 0 === t ? void 0 : t.ne) || void 0 === e ? void 0 : e.fI;
if (o) {
this.ip.ne.fI;
var n = null === (i = o.cYw) || void 0 === i ? void 0 : i.BGM;
n ? this.ip.na.byN(n) : this.ip.na.byN("");
}
};
t.prototype.jW = function() {
if (this.ip && this.iT(this.ip.nx) && $fc.role.fqg) if (this.ip.ne.fN) this.iB = !0; else {
var t = $fc.role.fqg.fpV.level;
this.ip.nb.bAm(t);
}
};
t.prototype.jX = function() {
$fc.role.fqy(!1);
if (this.iB) {
this.iB = !1;
var t = $fc.role.fqg.fpV.level;
this.ip.nb.bAm(t);
}
};
t.prototype.jY = function() {
$fc.role.fqy(!0);
};
t.prototype.jZ = function(t) {
if (this.iu !== t) {
this.ip.mz = t;
this.iu = t;
this.ip.ne.gu(l.$uo.$vk.o);
}
};
t.jP = .1;
t.jQ = 18;
return t;
}();
i.$gz = I;
window.$fN = window.$fN || {};
$fN.$gA = I;
cc._RF.pop();
}, {
"../../bingo/src/$KG": void 0,
"../../bingo/src/utils/$DON": void 0,
"../../game/src/$KT": void 0,
"../../game/src/battle/$DSC": void 0,
"../../game/src/battle/$DSM": void 0,
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYQ": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../game/src/manager/$DUP": void 0,
"../../game/src/manager/role/$FBX": void 0,
"../../game/src/perf/$DVH": void 0,
"../../game/src/utils/$DVY": void 0,
"../../main/src/$KV": void 0,
"../battleLogic/$KL": void 0,
"../battleLogic/component/$DPL": void 0,
"./$KQ": "$KQ",
"./compnents/$DQK": "$DQK",
"./compnents/$DQL": "$DQL",
"./compnents/$DQN": "$DQN",
"./compnents/$DQO": "$DQO",
"./compnents/$DQP": "$DQP",
"./compnents/$DQQ": "$DQQ",
"./render/$DRC": "$DRC",
"./render/$DRD": "$DRD",
"./render/$DRG": "$DRG",
"./render/$DRK": "$DRK",
"./ui/$DRV": "$DRV"
} ]
}, {}, [ "$KQ", "$KR", "$DQK", "$DQL", "$DQM", "$DQN", "$DQO", "$DQP", "$DQQ", "$DQR", "$DQS", "$DQT", "$DQU", "$DQV", "$DQW", "$DQX", "$DQY", "$DQZ", "$DRA", "$DRB", "$DRC", "$DRD", "$DRE", "$DRF", "$DRG", "$DRH", "$DRI", "$DRJ", "$DRK", "$DRL", "$DRM", "$DRN", "$DRO", "$DRP", "$DRQ", "$DRR", "$DRS", "$DRT", "$DRU", "$DRV", "$DRW", "$DRX", "$DRY", "$DRZ" ]);