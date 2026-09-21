window.__require = function t(e, n, i) {
function r(b, a) {
if (!n[b]) {
if (!e[b]) {
var u = b.split("/");
u = u[u.length - 1];
if (!e[u]) {
var l = "function" == typeof __require && __require;
if (!a && l) return l(u, !0);
if (o) return o(u, !0);
throw new Error("Cannot find module '" + b + "'");
}
b = u;
}
var h = n[b] = {
exports: {}
};
e[b][0].call(h.exports, function(t) {
return r(e[b][1][t] || t);
}, h, h.exports, t, e, n, i);
}
return n[b].exports;
}
for (var o = "function" == typeof __require && __require, b = 0; b < i.length; b++) r(i[b]);
return r;
}({
$JZ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "113574DiDtEt7I9AHonlDoQ", "$JZ");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = t("./math/$NP"), r = t("../../game/src/battle/$QM"), o = function() {
function t() {}
t.Dl = function(t, e, n, o) {
var b, a, u, l, h, f, c, s, p, d, v, $, g, y, m, j, D, G, x, E, M, N;
if ("number" == typeof t) return t;
if (Array.isArray(t)) {
switch (t[0]) {
case "attr":
return null !== (a = null === (b = e.hv) || void 0 === b ? void 0 : b.bgR(t[1])) && void 0 !== a ? a : 0;

case "ap":
return Math.ceil(i.$jG.boj((null !== (l = null === (u = e.hv) || void 0 === u ? void 0 : u.bhl) && void 0 !== l ? l : 0) * (null !== (h = t[1]) && void 0 !== h ? h : 1)));

case "HPMAX":
return Math.ceil(i.$jG.boj((null !== (c = null === (f = e.hv) || void 0 === f ? void 0 : f.bgE) && void 0 !== c ? c : 0) * (null !== (s = t[1]) && void 0 !== s ? s : 1)));

case "MPMAX":
return Math.ceil(i.$jG.boj((null !== (d = null === (p = e.hv) || void 0 === p ? void 0 : p.bgG) && void 0 !== d ? d : 0) * (null !== (v = t[1]) && void 0 !== v ? v : 1)));

case "var":
return null !== ($ = e.hE.bjJ[t[1]]) && void 0 !== $ ? $ : 0;

case "sk":
var _ = t[1], w = e.hv.bgp(_);
return w ? w.DA(t[2]) : 0;

case "range":
return i.$jG.boj(t[1] + i.$jG.boj(t[2] - t[1]) * Math.min(1, this.Dl(t[3], e, n) / this.Dl(t[4], e, n)));

case "bf0":
return null !== (g = null == n ? void 0 : n.DG[0]) && void 0 !== g ? g : 0;

case "bf1":
return null !== (y = null == n ? void 0 : n.DG[1]) && void 0 !== y ? y : 0;

case "bf2":
return null !== (m = null == n ? void 0 : n.DG[2]) && void 0 !== m ? m : 0;

case "bf3":
return null !== (j = null == n ? void 0 : n.DG[3]) && void 0 !== j ? j : 0;

case "bf4":
return null !== (D = null == n ? void 0 : n.DG[4]) && void 0 !== D ? D : 0;

case "bf5":
return null !== (G = null == n ? void 0 : n.DG[5]) && void 0 !== G ? G : 0;

case "originBf0":
return null !== (x = null == o ? void 0 : o.DG[0]) && void 0 !== x ? x : 0;

case "originBf1":
return null !== (E = null == o ? void 0 : o.DG[1]) && void 0 !== E ? E : 0;

case "originBf2":
return null !== (M = null == o ? void 0 : o.DG[2]) && void 0 !== M ? M : 0;

case "originBf3":
return null !== (N = null == o ? void 0 : o.DG[3]) && void 0 !== N ? N : 0;

case "bfEventParam0":
return null == n ? void 0 : n.DU;

case "bfEventParam1":
return null == n ? void 0 : n.DV;

case "bfEventParam2":
return null == n ? void 0 : n.DW;

case "bfOrigin":
var k = e.hi.bnu(n.DQ);
return k ? this.Dl(t[1], k, n) : 0;

case "bfOriginCreator":
var O = e.hi.bnu(n.DQ);
if (O && O.hE) {
var F = e.hi.bnu(O.hE.bjZ);
return F ? this.Dl(t[1], F, n) : 0;
}
return 0;

case "+":
return i.$jG.boj(this.Dl(t[1], e, n) + this.Dl(t[2], e, n));

case "-":
return i.$jG.boj(this.Dl(t[1], e, n) - this.Dl(t[2], e, n));

case "*":
return i.$jG.boj(this.Dl(t[1], e, n) * this.Dl(t[2], e, n));

case "/":
return i.$jG.boj(this.Dl(t[1], e, n) / this.Dl(t[2], e, n));

case "ceil":
return Math.ceil(i.$jG.boj(this.Dl(t[1], e, n)));

case "floor":
return Math.floor(i.$jG.boj(this.Dl(t[1], e, n)));

case "WBVar":
var R = e.hE.bjJ.WBState || 0, P = r.default.bGR.bIx().find(function(t) {
return t.id === R;
});
return P ? P.attackRate : 0;
}
return t;
}
return t;
};
return t;
}();
n.default = o;
cc._RF.pop();
}, {
"../../game/src/battle/$QM": void 0,
"./math/$NP": "$NP"
} ],
$KA: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5425eBldDxGg7YEGq3JbLYn", "$KA");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$fI = n.$fH = n.$fG = n.$fF = n.$fE = n.$fD = n.$fC = n.$fB = n.$fA = n.$fz = n.$fy = n.$fx = n.$fw = n.$fv = n.$fu = n.registerSpecialCmd = n.battleCmdClassCtorMap = n.$ft = void 0;
var b = t("./$KC"), a = t("./component/$NC"), u = t("../../game/src/battle/types/$WU"), l = t("../../game/src/battle/types/$WW"), h = t("../../game/src/battle/types/$WY"), f = t("../../game/src/battle/$QM"), c = t("./component/$NF"), s = t("../../main/src/$KM"), p = function() {
function t() {}
t.prototype.eY = function() {
return 0;
};
t.prototype.eZ = function() {};
t.prototype.fa = function() {};
t.eX = null;
return t;
}();
n.$ft = p;
window.$fK = window.$fK || {};
$fK.getCmd = function(t) {
return n.battleCmdClassCtorMap.get(t);
};
n.battleCmdClassCtorMap = new Map();
function d(t) {
return function(e) {
if (!n.battleCmdClassCtorMap.has(t) && t > 0) {
e.eX = t;
n.battleCmdClassCtorMap.set(t, e);
} else console.warn("registerSpecialCmd cmdId error", e.name);
};
}
n.registerSpecialCmd = d;
var v = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 0);
var n = t.call(this) || this;
n.roomId = e;
return n;
}
n = e;
e.prototype.eY = function() {
return this.roomId << 9 | n.eX;
};
e.prototype.eZ = function(t) {
this.roomId = t >> 9;
};
e.prototype.fa = function(t, e) {
t.fJ && (this.roomId && this.roomId !== t.fJ || (e.dcw = 1));
};
var n;
return n = o([ d(u.$uv.M) ], e);
}(p);
n.$fu = v;
var $ = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 1);
var n = t.call(this) || this;
n.timeScale = e;
return n;
}
n = e;
e.prototype.eY = function() {
return Math.floor(1e4 * this.timeScale) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = (t >> 9) / 1e4;
this.timeScale = e;
};
e.prototype.fa = function(t) {
t.go(this.timeScale);
};
var n;
return n = o([ d(u.$uv.B) ], e);
}(p);
n.$fv = $;
var g = function(t) {
r(e, t);
function e() {
return t.call(this) || this;
}
n = e;
e.prototype.eY = function() {
return n.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
var n = t.gi(e.dco);
n && n.hx && (n.hx.bmd = 1);
};
var n;
return n = o([ d(u.$uv.C) ], e);
}(p);
n.$fw = g;
var y = function(t) {
r(e, t);
function e() {
return t.call(this) || this;
}
n = e;
e.prototype.eY = function() {
return n.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t) {
t.fF.forEach(function(e) {
var n = t.gi(e.dco);
n && n.hv.bgd.forEach(function(t) {
if (t.Dx() > 0) {
if (t.maxCdTime > 0) {
t.cd = 0;
t.cdTest = 0;
} else t.cdTest = 1;
t.Dp > 1 ? t.Dp = 0 : t.Dp = t.Dy();
}
});
});
};
var n;
return n = o([ d(u.$uv.D) ], e);
}(p);
n.$fx = y;
var m = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 1);
var n = t.call(this) || this;
n.reviveOpt = e;
return n;
}
n = e;
e.prototype.eY = function() {
return Math.floor(this.reviveOpt) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.reviveOpt = e;
};
e.prototype.fa = function(t, e) {
switch (this.reviveOpt) {
case 1:
b.default.GP(t, e.dca);
break;

case 0:
b.default.GO(t, e.dca);
}
};
var n;
return n = o([ d(u.$uv.E) ], e);
}(p);
n.$fy = m;
var j = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = u.$uv.H;
return e;
}
n = e;
e.prototype.eY = function() {
return n.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
if (!t.fe) {
var n = t.gi(e.dco);
if (n) if (n.hv.bhP() < 3147483648) {
n.hv.bgS(f.GameAttriTableEnum.hpMax, 3147483648);
n.hv.bgD = n.hv.bgE;
n.hv.bgS(f.GameAttriTableEnum.ap, 1e5);
n.hv.bgS(f.GameAttriTableEnum.sp, 1e5);
n.hv.bgS(f.GameAttriTableEnum.speed, 15e3);
n.hv.bgS(f.GameAttriTableEnum.atkSpeed, 1e4);
n.hv.bgS(f.GameAttriTableEnum.castSpeed, 1e4);
n.hv.bgS(f.GameAttriTableEnum.finalDamageRed, 1e4);
} else {
n.hv.bgT(f.GameAttriTableEnum.hpMax, 3147483648);
n.hv.bgD = n.hv.bgE;
n.hv.bgT(f.GameAttriTableEnum.ap, 1e5);
n.hv.bgT(f.GameAttriTableEnum.sp, 1e5);
n.hv.bgT(f.GameAttriTableEnum.speed, 15e3);
n.hv.bgT(f.GameAttriTableEnum.atkSpeed, 1e4);
n.hv.bgT(f.GameAttriTableEnum.castSpeed, 1e4);
n.hv.bgT(f.GameAttriTableEnum.finalDamageRed, 1e4);
}
}
};
var n;
return n = o([ d(u.$uv.H) ], e);
}(p);
n.$fz = j;
var D = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = u.$uv.I;
return e;
}
n = e;
e.prototype.eY = function() {
return n.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
if (!t.fe) {
var n = t.gi(e.dco);
n && t.fu.bmW.forEach(function(t) {
if (t.hx && t.hD.bkR !== n.hD.bkR) {
t.hX(n, t.hv.bgD);
b.default.Hf(t, 0, 0);
}
});
}
};
var n;
return n = o([ d(u.$uv.I) ], e);
}(p);
n.$fA = D;
var G = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = u.$uv.J;
return e;
}
n = e;
e.prototype.eY = function() {
return n.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
var n = t.gi(e.dco);
if (n) {
var i = n.hw;
t.fD.bmW.forEach(function(t) {
var e = t.ho(a.default);
if (b.default.Gz(i, t.hw) < e.blg) {
var r = t.hE.bjF.interfactive;
if (r) {
r.tlg && b.default.Gg(n, null, r.tlg, null);
r.lg && b.default.Gg(t, null, r.lg, null);
}
}
});
}
};
var n;
return n = o([ d(u.$uv.J) ], e);
}(p);
n.$fB = G;
var x = function(t) {
r(e, t);
function e(e) {
var n = t.call(this) || this;
n.consumeIndex = e;
return n;
}
n = e;
e.prototype.eY = function() {
return Math.floor(1e4 * this.consumeIndex) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = (t >> 9) / 1e4;
this.consumeIndex = e;
};
e.prototype.fa = function(t, e) {
var n, i = t.gi(e.dco);
if (i) {
var r = null === (n = i.hL) || void 0 === n ? void 0 : n.blO[this.consumeIndex];
if (r) {
b.default.Fy(i, r);
t.gu(h.$uD.$vA.w, i, r.Er.id);
}
}
};
var n;
return n = o([ d(u.$uv.N) ], e);
}(p);
n.$fC = x;
var E = function(t) {
r(e, t);
function e(e) {
var n = t.call(this) || this;
n.autoNum = e;
return n;
}
n = e;
e.prototype.eY = function() {
return this.autoNum << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.autoNum = e;
};
e.prototype.fa = function(t, e) {
var n = Boolean(this.autoNum);
if (e.dcv !== n) {
e.dcv = n;
switch (t.fM.f) {
case l.$uy.e:
break;

default:
var i = t.gi(e.dco);
if (!i) return;
if (e.dcv) b.default.EM(i); else {
i.hx.blW = 0;
i.hq(c.default);
}
}
t.gu(h.$uD.$vA.o);
}
};
var n;
return n = o([ d(u.$uv.L) ], e);
}(p);
n.$fD = E;
var M = function(t) {
r(e, t);
function e(e) {
var n = t.call(this) || this;
n.level = e;
return n;
}
n = e;
e.prototype.eY = function() {
return Math.max(Math.min(Math.floor(this.level), 999), 1) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.level = e;
};
e.prototype.fa = function(t, e) {
var n = t.gi(e.dco);
if (n) {
b.default.EI(n, this.level);
t.gu(h.$uD.$vA.bl);
}
};
var n;
return n = o([ d(u.$uv.O) ], e);
}(p);
n.$fE = M;
var N = function(t) {
r(e, t);
function e(e, n) {
var i = t.call(this) || this;
i.playerPosition = e;
i.offlineState = n;
return i;
}
n = e;
e.prototype.eY = function() {
return (this.playerPosition << 8 | this.offlineState) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9, n = e >> 8, i = 255 & e;
this.playerPosition = n;
this.offlineState = i;
};
e.prototype.fa = function(t) {
var e = this, n = t.fF.find(function(t) {
return t.dcd === e.playerPosition;
});
n && (n.dcC = 1 === this.offlineState);
};
var n;
return n = o([ d(u.$uv.P) ], e);
}(p);
n.$fF = N;
var _ = function(t) {
r(e, t);
function e(e) {
var n = t.call(this) || this;
n.buffId = e;
return n;
}
n = e;
e.prototype.eY = function() {
var t;
return (null !== (t = this.buffId) && void 0 !== t ? t : 0) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.buffId = e;
};
e.prototype.fa = function(t) {
for (var e = 0, n = t.fF; e < n.length; e++) {
var i = n[e], r = t.gi(i.dco);
r && b.default.Gh(r, this.buffId, [], r.hg);
}
};
var n;
return n = o([ d(u.$uv.R) ], e);
}(p);
n.$fG = _;
var w = function(t) {
r(e, t);
function e(e, n) {
var i = t.call(this) || this;
i.cmdId = e;
i.position = n;
return i;
}
n = e;
e.prototype.eY = function() {
return (this.cmdId << 8 | this.position) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.cmdId = e >> 8;
this.position = 255 & e;
};
e.prototype.fa = function(t, e) {
var n = this;
if (t.fF.find(function(t) {
return t.dcd === n.position;
})) {
var i = t.gi(e.dco);
if (i) {
var r = i.hL.blP[this.cmdId];
r && i.hx.bmc.push({
c: this.cmdId,
d: r,
b: this.position
});
}
}
};
var n;
return n = o([ d(u.$uv.S) ], e);
}(p);
n.$fH = w;
var k = function(t) {
r(e, t);
function e(e) {
var n = t.call(this) || this;
n.eventId = e;
return n;
}
n = e;
e.prototype.eY = function() {
var t;
return (null !== (t = this.eventId) && void 0 !== t ? t : 0) << 9 | n.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.eventId = e;
};
e.prototype.fa = function(t) {
var e;
switch (this.eventId) {
case h.$uD.$vy.b:
var n = t.fF[0];
if (n) {
if (!(o = t.gi(n.dco)).hU) return;
b.default.EY(o, h.$uD.$ve.b);
var i = t.gF()[0];
if (!(null == i ? void 0 : i.hU)) return;
b.default.EY(i, "10001");
o.hx.bme = !0;
}
break;

case h.$uD.$vy.c:
var r = t.fF[0];
if (r) {
var o;
if (!(null == (o = t.gi(r.dco)) ? void 0 : o.hU)) return;
o.hx.bme = !1;
var a = null === (e = o.hv) || void 0 === e ? void 0 : e.bgd.find(function(t) {
return t.Dv.commonType === s.cvResUrl.commonSkillDodge2Cid;
});
if (b.default.Fz(o, a)) {
b.default.FA(o, a);
b.default.EY(o, h.$uD.$ve.l);
}
}
}
};
var n;
return n = o([ d(u.$uv.T) ], e);
}(p);
n.$fI = k;
cc._RF.pop();
}, {
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WU": void 0,
"../../game/src/battle/types/$WW": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../main/src/$KM": void 0,
"./$KC": "$KC",
"./component/$NC": "$NC",
"./component/$NF": "$NF"
} ],
$KB: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "0d14c0gdaRPR65TGQXWfn0g", "$KB");
var i = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$gb = void 0;
var r = t("../../bingo/src/utils/$LZ"), o = t("../../game/src/battle/$QM");
(function(t) {
var e = function() {
function t() {
this.Dm = 0;
this.Dr = !1;
this.Ds = !1;
this.Dt = 0;
this.Du = -1;
this.cdTest = 1;
}
t.prototype.Dw = function(t, e, n) {
this.Dm = t;
this.Dv = o.default.bGR.bHq(this.Dm);
this.Dt = e;
this.Dn = 0;
this.cd = 0;
this.Ds = n;
this.cdTest = 1;
this.Dr = !0;
this.maxCdTime = this.Dx();
this.Dp = this.Dy();
this.Dq = this.Dz();
this.Dv.bOriginal && (this.Dt = Math.max(this.Dv.bOriginal, this.Dt));
"number" == typeof this.Dv.storageCapacityIndex ? this.Do = this.DA(this.Dv.storageCapacityIndex) : this.Do = 0;
};
t.prototype.Dx = function() {
var t;
return this.Dv && null !== (t = this.Ds ? this.Dv.pveCD : this.Dv.pvpCD) && void 0 !== t ? t : 0;
};
t.prototype.Dy = function() {
var t;
if (!(null === (t = this.Dv) || void 0 === t ? void 0 : t.mpCost)) return 0;
var e = this.Dv.mpCost[this.Ds ? 0 : 1];
return e && e[0] + (Math.min(this.Dv.skillMaxLv, this.Dt) - 1) * e[1] || 0;
};
t.prototype.Dz = function() {
var t;
if (!(null === (t = this.Dv) || void 0 === t ? void 0 : t.hpPercentCost)) return 0;
var e = this.Dv.hpPercentCost[this.Ds ? 0 : 1];
return e && e[0] + (Math.min(this.Dv.skillMaxLv, this.Dt) - 1) * e[1] || 0;
};
t.prototype.DA = function(t) {
var e;
return (null === (e = this.Dv) || void 0 === e ? void 0 : e.pveBase) ? this.Ds ? this.Dv.pveBase[t] + (Math.min(this.Dv.skillMaxLv, this.Dt) - 1) * this.Dv.pveUpgrade[t] || 0 : this.Dv.pvpBase[t] + (Math.min(this.Dv.skillMaxLv, this.Dt) - 1) * this.Dv.pvpUpgrade[t] || 0 : 0;
};
t.prototype.DB = function() {};
t.prototype.DC = function() {
return [];
};
i([ r.pooledFloat ], t.prototype, "cd", void 0);
i([ r.pooledFloat ], t.prototype, "maxCdTime", void 0);
i([ r.pooledInt ], t.prototype, "cdTest", void 0);
return t;
}();
t.$gc = e;
var n = function() {
function t() {
this.DD = 0;
this.DE = null;
this.DF = null;
this.DG = [];
this.DH = [];
this.DI = [];
this.DJ = 0;
this.DK = 0;
this.DL = 0;
this.DM = 0;
this.DN = 0;
this.DO = 0;
this.DP = 0;
this.DQ = 0;
this.DR = 0;
this.DS = 0;
this.DT = 0;
this.DU = 0;
this.DV = 0;
this.DW = 0;
this.DX = 0;
this.DY = 0;
this.DZ = 0;
this.Ea = 0;
}
t.prototype.Eb = function(t) {
this.DE = t;
this.DF = o.default.bGR.bHu(this.DE);
this.DQ = 0;
this.DG.length = 0;
this.DH.length = 0;
this.DI.length = 0;
this.DU = 0;
this.DV = 0;
this.DW = 0;
this.DR = 0;
this.DS = 0;
this.DT = 0;
this.DX = 0;
this.DY = 0;
this.DZ = 0;
this.Ea = 0;
};
t.prototype.Ec = function() {};
t.prototype.Ed = function() {
return [];
};
return t;
}();
t.$gd = n;
t.$ge = function() {
this.atkEntity = null;
this.fromEntity = null;
this.target = null;
this.hitInfo = null;
this.hitX = 0;
this.hitY = 0;
this.hitZ = 0;
this.isCrt = !1;
this.beforeHP = 0;
this.finalDamage = 0;
this.isPhysical = !0;
this.elementValue = 0;
};
var b = function() {
function t() {
this.Ee = 0;
this.El = !1;
this.Em = !1;
this.En = 0;
this.Eo = null;
this.Ep = !1;
this.Eq = !1;
}
t.prototype.Es = function(t, e) {
var n, i;
this.Ee = t;
this.Ee > 0 ? this.Er = o.default.bGR.bHp(this.Ee) : this.Er = null;
this.Ef = 0;
this.Eh = 0;
this.Ei = null !== (i = null === (n = this.Er) || void 0 === n ? void 0 : n.cd) && void 0 !== i ? i : 0;
this.Eo = e;
this.Ep = !1;
this.Eq = !1;
if (this.Er) for (var r = 0, b = this.Er.set; r < b.length; r++) switch (b[r]) {
case 11922:
this.Ep = !0;
break;

case 11923:
this.Eq = !0;
}
};
return t;
}();
t.$gf = b;
})(n.$gb || (n.$gb = {}));
cc._RF.pop();
}, {
"../../bingo/src/utils/$LZ": void 0,
"../../game/src/battle/$QM": void 0
} ],
$KC: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "9ebdcJ6pydMOb240RkMalIf", "$KC");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = t("./BattleBehaviorLogic"), r = t("./component/$MM"), o = t("./component/$MO"), b = t("./component/$MP"), a = t("./component/$MQ"), u = t("./component/$MS"), l = t("./component/$MW"), h = t("./component/$MY"), f = t("./component/$MZ"), c = t("./component/$MV"), s = t("./component/$ND"), p = t("./component/$NH"), d = t("./component/$NJ"), v = t("./component/$NL"), $ = t("./component/$NM"), g = t("./component/$MX"), y = t("./component/$MN"), m = t("../../game/src/battle/types/$WY"), j = t("./math/$NP"), D = t("./math/$NQ"), G = t("./utils/$OG"), x = t("./$JZ"), E = t("./component/$NG"), M = t("../../game/src/battle/$QM"), N = t("./$KB"), _ = t("./math/$NR"), w = t("./component/$MR"), k = t("./component/$NI"), O = t("./component/$MT"), F = t("./system/$NW"), R = t("./component/$MU"), P = t("./component/$NF"), T = t("../../main/src/tbl/TblEnum"), I = t("../../main/src/$KM"), A = function() {
function t() {}
t.Ey = function(t, e, n) {
var i = null;
(i = 0 === this.Ex.length ? new N.$gb.$ge() : this.Ex.pop()).atkEntity = t;
i.fromEntity = e;
i.target = n;
i.hitX = 0;
i.hitY = 0;
i.hitZ = 0;
i.hitInfo = null;
i.isCrt = !1;
i.beforeHP = 0;
i.finalDamage = 0;
i.isPhysical = !0;
i.elementValue = 0;
return i;
};
t.Ez = function(t) {
t.atkEntity = null;
t.fromEntity = null;
t.target = null;
this.Ex.push(t);
};
t.EB = function() {
return 0 === this.EA.length ? new N.$gb.$gc() : this.EA.pop();
};
t.EC = function(t) {
this.EA.push(t);
};
t.ED = function() {
this.EA.length = 0;
};
t.EE = function(e, n, i, r, o) {
var b, a, u, l, h = e.hv;
h.bfY = i;
var f = h.bfZ = M.default.bGR.bHl(h.bfY);
h.bgC = f.speed;
h.bhj = f.gravity;
h.bhk = f.jump;
h.bge = null !== (b = f.defaultWeaponType) && void 0 !== b ? b : 0;
if (null === (a = null == o ? void 0 : o.wear) || void 0 === a ? void 0 : a.equipList) {
var c = M.default.bGR.bIa(o.wear.equipList);
if (c) {
var s = M.default.bGR.bIb(c.itemId);
s && (h.bge = s.childType);
}
}
h.bgH = r;
var p = e.hi.bnl.fM.l;
if ((null == o ? void 0 : o.skillDataList) && (null == o ? void 0 : o.wear)) {
if (p) {
h.bgb.daC(i, r, {
isPve: e.hi.bnl.gM,
skillDataList: o.skillDataList,
wears: [],
pet: null,
archive: null,
adventureIndex: [],
mountIds: [],
guildBuffList: null
});
h.bgb.dai.cZk();
h.bgb.dak.cZk();
h.bgb.dam.cZk();
h.bgb.dao.cZk();
h.bgb.daq.cZk();
} else h.bgb.daC(i, r, {
isPve: e.hi.bnl.gM,
skillDataList: o.skillDataList,
wears: null === (u = o.wear) || void 0 === u ? void 0 : u.equipList,
pet: null === (l = o.troop) || void 0 === l ? void 0 : l.pet,
archive: o.archive,
adventureIndex: o.adventureIndex,
mountIds: o.mountIds,
guildBuffList: o.guildBuffList
});
h.bgb.dag();
} else h.bgb.dag();
var d = h.bgb.dac;
for (var v in n) {
var $ = n[v];
d.cZO(Number(v), $);
}
h.bgD = h.bgE;
h.bgF = h.bgG;
h.bgc = null == o ? void 0 : o.skillDataList;
t.EF(h, e.hi.bnl.gM);
};
t.EF = function(e, n) {
for (var i = 0, r = e.bgd; i < r.length; i++) {
var o = r[i];
t.EC(o);
}
e.bgd.length = 0;
var b = e.bfZ.skillGroup;
b || (b = M.default.bGR.bHr());
if (e.bgc) for (var a = 0, u = b; a < u.length; a++) {
s = u[a];
var l = e.bgb.daN(s);
if (!(l < 1)) {
(c = t.EB()).Dw(s, l, n);
e.bgd.push(c);
}
} else for (var h = 0, f = b; h < f.length; h++) {
var c, s = f[h];
(c = t.EB()).Dw(s, 1, n);
e.bgd.push(c);
}
};
t.EG = function() {};
t.EH = function() {};
t.EI = function(e, n) {
var i = e.hv;
if (i) {
i.bgH = n;
if (i.bgI) {
e.hY(e, e.hv.bgE);
e.hZ(e, e.hv.bgG);
t.Gh(e, 11007, [], e.hg);
}
}
};
t.EJ = function(t, e, n, i) {
var r, o, b;
switch (e.type) {
case m.$uD.$vu.b:
return t.hi.bnu(t.hE.bjP);

case m.$uD.$vu.c:
var a = t.hE.bjQ.find(function(t) {
return t.id === e.id;
});
return a ? t.hi.bnu(a.eid) : null;

case m.$uD.$vu.d:
return !t.hG && (u = t.hi.bnu(t.hE.bjO)) ? t.hi.bnu(null === (r = u.hG) || void 0 === r ? void 0 : r.bis) : t.hi.bnu(null === (o = t.hG) || void 0 === o ? void 0 : o.bis);

case m.$uD.$vu.e:
return (null == n ? void 0 : n.target) ? n.target : null;

case m.$uD.$vu.f:
return t;

case m.$uD.$vu.g:
if (i) {
var u;
return t.hi.bnu(i.DQ);
}
return null;

case m.$uD.$vu.h:
return t.hi.bnu(t.hE.bjZ);

case m.$uD.$vu.i:
for (var l = 0, h = t.hi.bnl.fu.bmW; l < h.length; l++) {
var f = h[l];
if (f.hS) return f;
}
return null;

case m.$uD.$vu.j:
return this.EK(t, e.radius, !0, 1);

case m.$uD.$vu.k:
var c = null === (b = t.hv) || void 0 === b ? void 0 : b.bgd.find(function(t) {
return t.Dm === e.id;
});
return c ? t.hi.bnl.gi(c.Du) : null;
}
return null;
};
t.EK = function(t, e, n, i) {
var r, o, b, a, u = t.hi.bnl.fu.bmW, l = e * e;
if (0 === i) {
for (var h = Number.MAX_SAFE_INTEGER, f = -1, c = null, s = 0, p = u; s < p.length; s++) if ((null === (o = (D = p[s]).hE) || void 0 === o || !o.bkE) && D.hD.bkR !== t.hD.bkR && D.hU) {
var d = j.$jG.boj(t.hw.x - D.hw.x, 3), v = j.$jG.boj(t.hw.y - D.hw.y, 3);
if ((E = j.$jG.boj(d * d + v * v, 3)) <= l) if (n) {
if (E < h) {
h = E;
c = D;
}
} else if (E > f) {
f = E;
c = D;
}
}
return c;
}
for (var $ = ((r = {})[M.GameEnemyTypeTableEnum.Boss] = [], r[M.GameEnemyTypeTableEnum.Elite] = [], 
r[M.GameEnemyTypeTableEnum.Normal] = [], r[0] = [], r), g = -1, y = 0, m = u; y < m.length; y++) {
var D;
if ((D = m[y]).hD && (null === (b = D.hE) || void 0 === b || !b.bkE) && D.hD.bkR !== t.hD.bkR && D.hU) {
var x = D.hD.bkS;
if (!(x < g)) {
var E;
d = j.$jG.boj(t.hw.x - D.hw.x, 3), v = j.$jG.boj(t.hw.y - D.hw.y, 3);
if (!((E = j.$jG.boj(d * d + v * v, 3)) > l)) {
x > g && (g = x);
null === (a = $[x]) || void 0 === a || a.push({
e: D,
dis: E
});
}
}
}
}
var N = $[g];
if (!N || 0 === N.length) return null;
G.default.bpB(N, function(t, e) {
return t.dis - e.dis;
}, 0, N.length);
return n ? N[0].e : N[N.length - 1].e;
};
t.EL = function(t, e) {
var n, i, r, o, b, a, u = M.default.bGR.bHD(e);
if (u) {
var l = t.hn(y.default);
l.bhS = e;
l.bhZ = u;
l.bib = 0;
l.bic = 0;
l.bid = 0;
l.bie = 0;
l.bif = !0;
l.big.length = 0;
l.bih = null;
l.bia.x = null !== (i = null === (n = t.hw) || void 0 === n ? void 0 : n.x) && void 0 !== i ? i : 0;
l.bia.y = null !== (o = null === (r = t.hw) || void 0 === r ? void 0 : r.y) && void 0 !== o ? o : 0;
if (u.acts) for (var h = t.hv, f = 0, c = u.acts; f < c.length; f++) {
var s = c[f], p = h.bgr(s.cmdId);
if (p) {
p.cd = null !== (b = s.initTime) && void 0 !== b ? b : 0;
p.maxCdTime = null !== (a = s.cdTime) && void 0 !== a ? a : 0;
p.Dn = 0;
p.Do = 0;
p.Dp = 0;
p.Dq = 0;
}
} else u.playerAI;
} else M.default.bGR.bGV("not find ai", e);
};
t.EM = function(t) {
var e, n, i, r, o = t.hn(P.default);
o.blv = 0;
o.blw = 0;
o.blx = 0;
o.bly = !0;
o.blz.length = 0;
o.blA = null;
o.blB = 0;
o.blC[0] = 0;
o.blC[1] = 0;
o.blC[2] = 0;
o.blC[3] = 0;
o.blu.x = null !== (n = null === (e = t.hw) || void 0 === e ? void 0 : e.x) && void 0 !== n ? n : 0;
o.blu.y = null !== (r = null === (i = t.hw) || void 0 === i ? void 0 : i.y) && void 0 !== r ? r : 0;
};
t.EN = function(t, e) {
var n = t.hI;
if (n) {
n.bhT = e;
n.bhY = 0;
n.bhX = 0;
n.bhW = {};
}
};
t.EO = function(t, e, n, i) {
var r = t.hn(h.default);
r.bkR = e;
r.bkS = n;
r.bkT = i;
};
t.EP = function(t, e) {
var n = M.default.bGR.bHy(e), i = t.hn(o.default);
i.bik = e;
i.bil = 0;
i.bim = n;
i.bin = n ? M.default.bGR.bHC(n.actsId) : null;
i.bio = 0;
i.bip.length = 0;
i.biq = 0;
};
t.EQ = function(e, n) {
var i, r, o, b, a, u;
if (e.hv) {
e.hv.bgD = Math.ceil(j.$jG.boj(n * e.hv.bgE));
if (e.hv.bgc) for (var l = e.hi.bnl.gN, h = 0, f = e.hv.bgd; h < f.length; h++) {
var c = f[h];
c.maxCdTime = e.hv.bhK(c);
l && (c.cd = null !== (i = c.Dv.pvpInitialCD) && void 0 !== i ? i : 0);
}
for (var s = 0, p = e.hv.bgb.dac.cYU; s < p.length; s++) {
var d = p[s];
t.Gh(e, d, null, e.hg, null);
}
if (null === (o = null === (r = e.hE) || void 0 === r ? void 0 : r.bjF) || void 0 === o ? void 0 : o.buffs) for (var v = 0, $ = null === (a = null === (b = e.hE) || void 0 === b ? void 0 : b.bjF) || void 0 === a ? void 0 : a.buffs; v < $.length; v++) {
d = $[v];
var g = M.default.bGR.bHu(d);
1 === (null !== (u = null == g ? void 0 : g.addType) && void 0 !== u ? u : 1) ? t.Gh(e, d, [], 0) : M.default.bGR.bGV("behavior buffs " + d + " addType != 1 ");
}
if (e.hS && e.hi.bnl.fM.m) for (var y = 0, m = e.hi.bnl.fM.m; y < m.length; y++) {
d = m[y];
t.Gh(e, d, [], 0);
}
}
};
t.ER = function(e) {
t.Gk(e, 11002);
e.hE.bjW.forEach(function(t) {
t.destroyTime = 0;
});
e.hE.bjF.newRound && t.Gg(e, null, e.hE.bjF.newRound, e);
t.EV(e);
};
t.ES = function(e, n, i, r, o, b, a) {
var u;
void 0 === i && (i = -1);
void 0 === r && (r = -1);
void 0 === b && (b = {});
void 0 === a && (a = !1);
var h = M.default.bGR.bHF(n);
if (!h) {
h = M.default.bGR.bHF(100);
M.default.bGR.bGV("no find behaviorCfg", n);
}
var f = e.hn(l.default);
f.bjx = null;
f.bjy = t.Et;
f.bjz = null;
f.bjA = t.Et;
f.bjB.length = 0;
f.bjC = {};
f.bjD = {};
f.bjE = n;
f.bjF = h;
f.bjG = 0;
f.bjH = null;
f.bjI[0] = 0;
f.bjI[1] = 0;
f.bjJ = b;
f.bjK = -1;
f.bjL = 0;
f.bjM = {};
f.bjN = {};
f.bjO = -1 === r ? e.hg : r;
f.bjP = i;
f.bjQ.length = 0;
f.bjR.length = 0;
f.bjS = -1;
f.bjT = -1;
f.bjU = null;
f.bjV.length = 0;
f.bjW.length = 0;
f.bkA();
f.bjZ = -1;
f.bka = !1;
f.bkb = null;
f.bkc = 0;
f.bkd = 0;
f.bke = 0;
f.bkf = 1;
f.bkg = 0;
f.bkh = null;
f.bki = -1;
f.bkj = 0;
f.bkk = null;
f.bkl.length = 0;
f.bkm.b = 0;
f.bkm.c.length = 0;
f.bjF.acts && e.hF && (e.hF.bin = f.bjF.acts);
t.EY(e, String(o || f.bkB));
if (!e.hv && f.bjF.buffs) for (var c = 0, s = f.bjF.buffs; c < s.length; c++) {
var p = s[c], d = M.default.bGR.bHu(p);
1 === (null !== (u = null == d ? void 0 : d.addType) && void 0 !== u ? u : 1) ? t.Gh(e, p, [], 0) : M.default.bGR.bGV("behavior buffs " + p + " addType != 1 ");
}
};
t.ET = function(e, n, r) {
if (e.hE) {
e.hE.bjF.newRound && t.Gg(e, null, e.hE.bjF.newRound, e);
if (e.hE.bjH && e.hE.bjH.els) for (var o = 0, b = e.hE.bjH.els; o < b.length; o++) {
var a = b[o];
i.default.Dj(e, null, a);
}
var u = e.hE;
u.bjE = n;
u.bjF = M.default.bGR.bHF(n);
u.bjK = -1;
u.bjL = 0;
u.bkA();
u.bjH = null;
u.bjG = 0;
e.hv && !e.hv.bgI && (r = m.$uD.$ve.e);
t.EY(e, String(r || u.bkB));
if (u.bjF.buffs) for (var l = 0, h = u.bjF.buffs; l < h.length; l++) {
var f = h[l];
t.Gh(e, f, [], 0);
}
e.hi.bnl.gu(m.$uD.$vA.bc, e);
}
};
t.EU = function(t) {
return !!t.hv && !!t.hv.bgI && !t.hv.bga.bpD(m.$uD.$vq.c | m.$uD.$vq.b) && -1 === t.hE.bjS;
};
t.EV = function(e) {
if (e.hE && e.hv && !e.hv.bhM() && -1 === e.hE.bjS && e.hv.bgI) {
t.EY(e, e.hE.bkB);
e._if();
return !0;
}
};
t.EW = function(t) {
if (t.hv) {
if (t.hv.bhM()) return !1;
if (!t.hv.bgI) return !1;
}
return !(t.hE.bkC || t.hE.bjX.hurtbackTime > 0 && t.hE.bjX.hurtbackType !== m.$uD.$vc.b);
};
t.EX = function(e, n) {
void 0 === n && (n = !1);
var i = e.hE;
if (i) {
var r = e.hi;
t.Fl(e, null);
if (i.bjB.length > 0) for (var o = 0, b = i.bjB.slice(); o < b.length; o++) {
var a = b[o];
t.Gj(e, a, n);
}
t.Fq(e);
t.Fd(e);
i.bjV.length && t.FY(e, -1);
for (var u = 0, l = i.bkl; u < l.length; u++) {
var h = l[u], f = r.bnu(null == h ? void 0 : h.f);
f && f.hP();
}
i.bkl.length = 0;
}
};
t.EY = function(t, e) {
this.Fb(t, e);
};
t.EZ = function(t, e) {
t.hE.bjx = e;
if (e) {
var n = t.hv;
if (!n) {
var i = t.hi.bnu(t.hE.bjO);
n = null == i ? void 0 : i.hv;
}
if (n) {
if (e.mhts) {
var r = n.bhI(e.mhts);
t.hy.mainTimeScale = r;
t.hy.hTimeScale = r;
} else {
if (e.mts) {
r = n.bhI(e.mts);
t.hy.mainTimeScale = r;
}
if (e.hts) {
r = n.bhI(e.hts);
t.hy.hTimeScale = r;
}
}
if (e.vts) {
r = n.bhI(e.vts);
t.hy.vTimeScale = r;
}
}
}
};
t.Fa = function(t, e) {
if (e) {
t.hy.hTimeScale = 1;
t.hy.vTimeScale = 1;
t.hy.mainTimeScale = 1;
}
};
t.Fb = function(e, n, i, r) {
var o, b, a = e.hE, u = null !== (o = a.bjF.behaviors[n]) && void 0 !== o ? o : r;
if (i && i[0]) {
a.bjI[0] = i[0];
a.bjI[1] = i[1];
var l = M.default.bGR.bHL(a.bjI[0]);
if (!l) return;
u = l.behaviors[a.bjI[1]];
} else {
if (!u) {
switch (n) {
case m.$uD.$ve.h:
case m.$uD.$ve.g:
case m.$uD.$ve.e:
case m.$uD.$ve.f:
case m.$uD.$ve.i:
u = M.default.bGR.bHM(n);
}
if (!u) return;
}
a.bjI[0] = 0;
a.bjI[1] = 0;
}
if (a.bjX.hurtbackTime) {
a.bjX.hurtbackTime = 0;
switch (a.bjX.hurtbackType) {
case m.$uD.$vc.b:
case m.$uD.$vc.c:
e.hz && t.FG(e.hz, 0, 0, 0);
}
}
t.Fl(e, u);
a.bjG = n;
a.bjH = u;
a.bjy = u.cls;
a.bjA = u.cgs;
a.bjz = u.els;
t.Fa(e, a.bjx);
t.EZ(e, u.status);
if (null === (b = u.status) || void 0 === b ? void 0 : b.changeDirOnUse) {
var h = e.hx;
h && h.bmg && h.blW && (e.hO || (h.blX.x > .2 ? e.hw.bmF = 1 : h.blX.x < -.2 && (e.hw.bmF = -1)));
}
a.bjK = -1;
a.bjL = 0;
a.bjM = {};
a.bjN = {};
this.Fk(e);
};
t.Fc = function(e, n) {
var i, r, o = null !== (i = e.hD.bkR) && void 0 !== i ? i : 0, b = e.hE, l = e.hF;
if (null == l ? void 0 : l.bin) {
var h = l.bin[n.isHurtAction ? b.bjX.actId : n.actId];
if (h) {
var c = n.start || 0, s = "number" == typeof n.end ? n.end : h.d, p = j.$jG.boj(Math.max(s - c, 0), 4), d = "number" == typeof n.timeScale ? n.timeScale : 1, v = e.hi;
e.hy.actionTimeScale = d;
b.bkk = n;
b.bkg = 0;
b.bki = e.hi.bnl.fi;
b.bkh = {
b: p,
d: h.an,
e: n.loop,
c: d,
i: n.start,
j: n.end,
f: 0,
g: 0,
h: 0,
m: n.posLerpRatio
};
for (var g = b.bkl, y = h.cs, m = n.bodies, D = function(n) {
var i = m[n], h = y.find(function(t) {
return t.n === i.name;
});
if (!h) {
M.default.bGR.bGV("actions" + (null === (r = l.bim) || void 0 === r ? void 0 : r.actsId) + " no find:" + i.name);
h = {
n: "",
fs: []
};
}
var s = g[n];
s || (s = g[n] = {
e: 1,
b: null,
c: "",
f: -1,
d: 0,
g: []
});
s.b = i;
s.e = 1;
s.d = 0;
s.c = h.n;
s.g = [ {
region: [ 0, p ],
collider: null
} ];
for (var d = s.g, j = 0, D = h.fs; j < D.length; j++) {
var G = D[j], E = G.t - c;
if (E > 0) {
if (E > p) break;
var N = d.length - 1;
d[N].region[1] = E;
E <= p && (d[N + 1] = {
region: [ E, p ],
collider: G.c
});
} else d[0].collider = G.c;
}
var _ = v.bnu(s.f);
if (!_) {
_ = v.bnn();
s.f = _.hg;
_.hn($.default);
_.hn(f.default);
_.hV(e.hW);
}
_.hA.blc = o;
_.hA.blb = 0;
_.hA.bkZ.length = 0;
_.hq(u.default);
_.hq(w.default);
_.hq(a.default);
if (_.hM) {
F.$jQ.bpr(_);
_.hq(O.default);
}
switch (i.type) {
case 1:
_.hn(u.default).biT = e.hg;
break;

case 2:
_.hn(a.default);
var k = _.hB;
k.biC = e.hg;
k.biD = b.bjO;
k.biz = i.attr;
k.biE = {};
k.biF = 0;
k.biG = 0;
k.biH = 0;
k.biI = 0;
k.biy = i.type;
var R = i.attr.damageFactor;
if (i.attr.skId) {
k.biN = i.attr.skIndex;
var P = e.hv;
P || (I = v.bnu(k.biD)) && (P = I.hv);
if (P) {
var T = P.bgp(i.attr.skId);
if (T) {
k.biK = 2 !== T.Dv.damageType;
k.biM = i.attr.skId;
k.biJ = T.DA(i.attr.skIndex) / M.GameAttributeConstRatioRange;
}
} else {
k.biM = 0;
k.biK = 2 !== i.attr.damageType;
k.biJ = 0;
}
} else {
k.biM = 0;
var I = v.bnu(b.bjO);
k.biJ = I ? x.default.Dl(R, I, null, null) : 0;
0 === i.attr.damageType && I.hv ? k.biK = I.hv.bhl >= I.hv.bhm : k.biK = 2 !== i.attr.damageType;
}
if (i.attr.target) switch (i.attr.target.type) {
case 2:
var A = b.bjJ.atkTargetEid;
k.biL = "number" == typeof A ? A : -1;
break;

case 3:
var C = b.bjR[0];
C && (k.biL = C.eid);
} else k.biL = -1;
break;

case 3:
(S = _.hn(w.default)).biO = e.hg;
S.biP = i.attraction;
S.biQ = i.attraction.velX ? x.default.Dl(i.attraction.velX, e, null, null) : 0;
S.biR = i.attraction.velY ? x.default.Dl(i.attraction.velY, e, null, null) : 0;
S.biS = i.attraction.velZ ? x.default.Dl(i.attraction.velZ, e, null, null) : 0;
break;

case 4:
var S;
(S = _.hn(O.default)).biU = e.hg;
S.biV = i.event;
S.biW.length = 0;
}
t.FK(_.hw, e.hw);
var H = d[0].collider;
_.hA.bld = void 0 === i.scale ? 1 : x.default.Dl(i.scale, e, null, null);
H ? t.Gt(_.hA, H) : _.hA.blb = 0;
}, G = 0; G < m.length; G++) D(G);
for (G = m.length; G < g.length; G++) {
var E = g[G];
if (E) {
var N = v.bnu(E.f);
N && N.ib();
}
}
g.length = m.length;
var _ = b.bkm;
_.b = 0;
_.c = [ {
region: [ 0, p ],
anim: {}
} ];
var k = _.c, R = k[0], P = h.fs || [];
for (G = 0; G < P.length; G++) {
var T = P[G], I = T.t - c;
if (I > 0) {
if (I > p) break;
var A = k.length - 1;
k[A].region[1] = I;
if (I <= p) if (k[A + 1]) {
var C = k[A + 1];
C.region[0] = I;
C.region[1] = p;
C.anim = T.a;
} else k[A + 1] = {
region: [ I, p ],
anim: T.a
};
} else k[0].anim = T.a;
}
b.bkh.f = R.anim.x;
b.bkh.g = R.anim.y;
b.bkh.h = R.anim.a;
}
}
};
t.Fd = function(e) {
var n;
(null === (n = e.hE) || void 0 === n ? void 0 : n.bjR.length) && t.Fo(e, {
z: 320,
hitDirType: 0,
force: 300
});
};
t.Fe = function(e, n, i, r) {
var o = e.hE.bjX;
o.hurtbackType = m.$uD.$vc.b;
o.slideTime = n;
o.actId = r;
t.Fd(e);
t.Fb(e, m.$uD.$ve.f);
o.hurtbackTime = i;
};
t.Ff = function(e) {
var n = e.hE.bjX;
n.hurtbackType = m.$uD.$vc.h;
n.slideTime = 0;
n.actId = 0;
t.Fd(e);
t.Fb(e, m.$uD.$ve.i);
t.FG(e.hz, 0, 0, 0);
n.hurtbackTime = .9;
};
t.Fg = function(e) {
var n = e.hE.bjX;
n.hurtbackType = m.$uD.$vc.d;
n.slideTime = 0;
n.actId = 0;
t.Fd(e);
t.Fb(e, m.$uD.$ve.f);
n.hurtbackTime = 999999;
};
t.Fh = function(e) {
e.hE && t.EY(e, m.$uD.$ve.h);
};
t.Fi = function(e) {
t.Fp(e);
e.hE.bjR.length && t.Fo(e, {
z: 320,
hitDirType: 0,
force: 300
});
};
t.Fj = function() {};
t.Fk = function(e) {
for (var n = e.hE, r = e.hy, o = e.hv, b = n.bjy, a = n.bjM, u = 0, l = b.length; u < l; u++) {
if (void 0 === (g = b[u]).tts) {
if (a[u] >= 1) continue;
} else if (0 === g.tts) ; else if (a[u] >= g.tts) continue;
var h = !0;
if ((y = g.tgs) && y.length > 0) for (var f = 0, c = y.length; f < c; f++) {
var s = y[f], p = i.default.Di(e, null, s, null);
if (s[2] ? p : !p) {
h = !1;
break;
}
}
if (h) {
a[u] ? a[u] += 1 : a[u] = 1;
if (1 === i.default.Dj(e, null, g, null) || g.break) break;
}
}
for (var d = n.bjA, v = n.bjN, $ = (u = 0, d.length); u < $; u++) {
var g, y;
if (void 0 === (g = d[u]).tts) {
if (v[u] >= 1) continue;
} else if (0 === g.tts) ; else if (v[u] >= g.tts) continue;
for (h = !0, f = 0, c = (y = g.tgs).length; f < c; f++) {
s = y[f], p = i.default.Di(e, null, s, null);
if (s[2] ? p : !p) {
h = !1;
break;
}
}
if (h) {
if (o && (g.sid || g.csid)) {
if (0 === r.mainTimeScale) continue;
var m = null;
if (g.sid) {
var j = g.sid;
m = o.bgp(j);
} else for (var D = g.csid, G = o.bgd, x = 0, E = G.length; x < E; x++) if (G[x].Dv.commonType === D) {
m = G[x];
break;
}
if (!m) continue;
if (!t.Fz(e, m)) continue;
t.FA(e, m);
if (e.hL) {
var M = e.hL.blQ[m.Dv.id];
M && (e.hx.bmi(M).f = -9999);
}
}
v[u] ? v[u] += 1 : v[u] = 1;
if (1 === i.default.Dj(e, null, g, null) || g.break) break;
}
}
};
t.Fl = function(t, e) {
var n, r = t.hE, o = r.bjz;
r.bjz = null;
if (o) {
for (var b = null === (n = r.bjH) || void 0 === n ? void 0 : n.ssid, a = 0, u = r.bjy; a < u.length; a++) if ((s = u[a]).autoClear) {
if (s.notClearInSid && (null == e ? void 0 : e.ssid) === b) continue;
i.default.Dk(t, null, s);
}
if (r.bjA.length > 0) for (var l = 0, h = r.bjA; l < h.length; l++) if ((s = h[l]).autoClear) {
if (s.notClearInSid && (null == e ? void 0 : e.ssid) === b) continue;
i.default.Dk(t, null, s);
}
if (o.length > 0) for (var f = 0, c = o; f < c.length; f++) {
var s;
if (!(s = c[f]).notClearInSid || (null == e ? void 0 : e.ssid) !== b) if (0 === s.tgs.length) i.default.Dj(t, null, s); else {
for (var p = !0, d = 0; d < s.tgs.length; d++) {
var v = s.tgs[d], $ = i.default.Di(t, null, v);
if (v[2] ? $ : !$) {
p = !1;
break;
}
}
p && i.default.Dj(t, null, s);
}
}
}
};
t.Fm = function() {};
t.Fn = function() {};
t.Fo = function(e, n) {
var i, r = e.hE;
if (r && !(r.bjR.length < 1)) {
for (var o = 0, b = r.bjR; o < b.length; o++) {
var a = b[o], u = e.hi.bnu(a.eid);
if (u) {
u.hE.bjS = -1;
u.hq(g.default);
if (u.hw.z <= 0 && u.hv && (u.hE.bkv || u.im || (null === (i = u.hv) || void 0 === i ? void 0 : i.bhb))) t.EV(u); else {
u.hE.bkc = 0;
t.FO(e, e.ic);
var l = t.FT(e, u, n.hitDirType);
t.Ft(u, n, l);
}
}
}
r.bjR.length = 0;
}
};
t.Fp = function(t) {
var e = t.hE;
if (e) {
t.hq(g.default);
var n = t.hi.bnu(e.bjS);
if (n && n.hE) for (var i = n.hE.bjR, r = 0; r < i.length; r++) if (i[r].eid === t.hg) {
i.splice(r, 1);
break;
}
e.bjS = -1;
}
};
t.Fq = function(t) {
var e = t.hE;
if (e) {
var n = t.hi, i = n.bnu(e.bjP);
e.bjP = -1;
if (i && i.hE && (b = i.hE.bjQ).length) for (var r = 0; r < b.length; r++) if (b[r].eid === t.hg) {
b.splice(r, 1);
break;
}
t.hq(g.default);
var o = n.bnu(e.bjS);
e.bjS = -1;
if (o && o.hE) {
var b = o.hE.bjR;
for (r = 0; r < b.length; r++) if (b[r].eid === t.hg) {
b.splice(r, 1);
break;
}
}
}
};
t.Fr = function(e) {
if (e.hE.bjR.length > 0) for (var n = 0, i = e.hE.bjR; n < i.length; n++) {
var r = i[n], o = e.hi.bnu(r.eid);
o && t.Fs(e, o);
}
};
t.Fs = function(e, n) {
var i, r = e.hE, o = r.bjU;
if (o) {
var b, a = r.bjT;
if (o.createEnt) if (n.hJ) a = n.hJ.bkG; else {
var u = o.createEnt, l = t.FV(e, u, {
target: n
});
a = l.hg;
u.id && u.behaviorId && !u.ai && e.hE.bjQ.push({
id: u.id,
eid: l.hg
});
}
t.Fc(n, o.action);
if (null == (b = -1 !== a ? e.hi.bnu(a) : e) ? void 0 : b.hw) {
var h = b.hw, f = n.hw;
if ((null === (i = n.hv) || void 0 === i ? void 0 : i.bhb) || n.im || "number" != typeof o.z || !n.hE.bkv) {
o.dir && (n.hw.bmF = b.hw.bmF * o.dir);
if ("number" == typeof o.x) {
var c = n.hn(g.default);
c.bkG = b.hg;
c.bkM = null;
c.bkL = o.dir || 1;
c.bkH = 0;
c.bkI = "number" == typeof o.x ? o.x : (f.x - h.x) * (0 !== c.bkL ? h.bmF : 1);
c.bkJ = "number" == typeof o.y ? o.y : f.y - h.y;
c.bkK = "number" == typeof o.z ? o.z : f.z - h.z;
} else {
t.FG(n.hz, 0, 0, 0);
n.hq(g.default);
}
}
}
}
};
t.Ft = function(e, n, i) {
var r = e.hE.bjX;
r.hurtbackType = m.$uD.$vc.e;
r.slideTime = 0;
r.actId = 0;
t.Fd(e);
e.hE.bkf = 1;
var o = n.z, b = e.hE.bkc > 1;
if (n.fly) {
n.groundBounce && (e.hE.bkd = n.groundBounce);
n.wallBounce && (e.hE.bke = n.wallBounce);
}
n.z > 0 && e.hw.z <= 0 && (e.hw.z = 1);
if (-1 === n.groundBounce) {
e.hE.bkc = 2;
b = !0;
}
if (b) {
var a = e.hN;
if (a && e.il() <= 0 && e.hU) {
r.hurtbackTime = 0;
t.Fw(e);
a.bje && t.Gh(e, 11005, null, e.hg);
} else {
o = j.$jG.boj(.65 * o * e.ij(), 3);
t.EY(e, m.$uD.$ve.e);
t.FG(e.hz, i * n.force, 0, o);
r.hurtbackTime = 99999;
e.ig();
}
} else {
e.hE.bkc = 1;
t.EY(e, m.$uD.$ve.e);
t.FG(e.hz, i * n.force, 0, j.$jG.boj(o * e.ij()));
r.hurtbackTime = 99999;
e.ig();
}
};
t.Fu = function(e, n) {
var i = e.hE.bjX;
i.hurtbackType = m.$uD.$vc.f;
i.slideTime = 0;
i.actId = 0;
t.Fd(e);
e.hE.bkf = 1;
t.EY(e, m.$uD.$ve.g);
i.hurtbackTime = n;
e.ii();
};
t.Fv = function(t) {
t.hE.bkc = 0;
t.hE.bkd = 0;
t.hE.bke = 0;
t.hE.bkf = 1;
};
t.Fw = function(e) {
var n, i;
t.FG(e.hz, 0, 0, 0);
t.Fv(e);
if (e.hU) {
e.id ? e.hi.bnl.gN && t.Gh(e, 11003, null, e.hg) : (null === (n = e.hN) || void 0 === n ? void 0 : n.bje) && t.Gh(e, 11004, null, e.hg);
(null === (i = e.hv) || void 0 === i ? void 0 : i.bhM()) ? t.Fe(e, 0, 0, m.$uD.commonHitActions[1]) : t.EV(e);
t.GQ(e, null, m.$uD.$vx.j, null);
}
};
t.Fx = function(t, e) {
var n;
return !(null === (n = t.hv) || void 0 === n || !n.bgI || e.Eh || t.hi.bnl.fN);
};
t.Fy = function(e, n) {
var i;
if (e.hL) {
n.Eh = n.Ei;
n.Er && n.Er.buffid && t.Gh(e, n.Er.buffid, null !== (i = n.Er.type) && void 0 !== i ? i : null, e.hg, null);
}
};
t.Fz = function(t, e) {
return !(null == e || !e.Dr || t.hv.bgF < t.hv.bhL(e) || e.Dq > 0 && t.hv.bgP() < e.Dq || (e.Do > 0 ? e.Dn <= 0 : e.cd));
};
t.FA = function(e, n) {
var i, r = e.hv;
if (r) {
if (n.Do > 0) n.Dn -= 1; else {
n.maxCdTime = r.bhK(n);
n.cd = n.maxCdTime;
}
if (n.Dp) {
r.bgF = Math.max(0, r.bgF - r.bhL(n));
e.hi.bnl.gu(m.$uD.$vA.A, e);
}
if (n.Dq) {
var o = r.bgD, b = o - Math.max(1, o - Math.floor(j.$jG.boj(r.bgE * (n.Dq / M.GameAttributeConstRatioRange))));
e.hX(e, b);
}
e.hv && e.hi.bnl.gu(m.$uD.$vA.u, e, n.Dm);
if (null === (i = n.Dv) || void 0 === i ? void 0 : i.selectPlayer) {
var a = e.hx.bmc.find(function(t) {
return t.d === n.Dm;
});
if (a) {
var u = e.hi.bnl.fF.find(function(t) {
return t.dcd === a.b;
});
u && (n.Du = u.dco);
}
}
t.GQ(e, null, m.$uD.$vx.b, n);
}
};
t.FB = function() {
return !0;
};
t.FC = function() {};
t.FD = function(t, e) {
for (var n = 0; n < e.length; n++) {
var r = e[n];
if (r[0] !== m.$uD.$vv.c && r[0] !== m.$uD.$vv.m && r[0] !== m.$uD.$vv.M) {
var o = i.default.Di(t, null, r, null);
if (r[2] ? o : !o) return !1;
}
}
return !0;
};
t.FE = function(t, e, n) {
var i = t.hx;
if (!i) return 0;
var r = t.hw;
if (!r) return 0;
var o = new D.default(r.x, r.y), b = e.boz(o, new D.default()), a = j.$jG.boi(b.boy());
if (b.boC() < n * n) i.blW = 0; else {
var u = o.bow(1, 0), l = j.$jG.boj(Math.atan2(a.y, a.x), 3);
if (1 === i.blW) {
var h = j.$jG.boj(l - i.blY);
if (h > .06) {
var f = 2 * Math.PI;
(h %= f) > Math.PI ? h -= f : h < -Math.PI && (h += f);
h > Math.PI / 3 && (l = j.$jG.boj(i.blY + .5 * h, 3));
}
} else i.blW = 1;
j.$jG.bog(u, l);
i.blX.bow(u.x, u.y);
i.blY = l;
}
};
t.FF = function(t) {
return t.hy ? t.hy.actionTimeScale : 1;
};
t.FG = function(t, e, n, i) {
t.bli = e;
t.blj = n;
t.blk = i;
t.bll = 0;
t.blm = 0;
t.bln = 0;
t.blp = 0;
t.blt = null;
return t;
};
t.FH = function(t, e) {
t.blr = e;
};
t.FI = function(t, e) {
t.blq = e;
};
t.FJ = function(t, e, n, i, r, o) {
void 0 === r && (r = m.$uD.$vj.b);
void 0 === o && (o = 1e4);
t.x = e;
t.y = n;
t.z = i;
t.bmF = r;
t.bmG = 0;
t.bmH = o;
};
t.FK = function(t, e) {
t.x = e.x;
t.y = e.y;
t.z = e.z;
t.bmF = e.bmF;
t.bmG = e.bmG;
t.bmH = e.bmH;
};
t.FL = function() {};
t.FM = function() {};
t.FN = function(t, e) {
var n, i = t.hE;
if (t.hF) {
var r = e.anim;
if (void 0 !== e.transfer && t.hF.bim.transfer) {
var o = t.ho(k.default), b = null !== (n = null == o ? void 0 : o.bml) && void 0 !== n && n, a = t.hF.bim.transfer;
r = e.transfer ? b && a.bossOpen || a.open : b && a.bossClose || a.close;
}
i.bki = t.hi.bnl.fi;
i.bkh = {
b: 0,
d: r,
e: e.loop || 0,
c: "number" == typeof e.timeScale ? e.timeScale : 1,
i: e.start,
j: e.end,
f: 0,
g: 0,
h: 0,
m: 1
};
}
};
t.FO = function(t, e) {
var n;
void 0 === e && (e = !0);
var i = t.hw;
if (e) {
var r = t.hi.bnw(d.default);
i.x = Math.max(Math.min(i.x, r.bmv), r.bmu);
i.y = Math.max(Math.min(i.y, r.bmx), r.bmw);
i.z = Math.max(Math.min(i.z, 5e3), 0);
}
if (null === (n = t.hz) || void 0 === n ? void 0 : n.blt) {
var o = t.hz.blt;
if (1 === o.type) {
var b = o.param[0], a = o.param[1], u = o.param[2];
if (b > 0) {
var l = i.x - a, h = i.y - u, f = j.$jG.boj(l * l + h * h, 5);
if (f > b * b && f > 0) {
var c = j.$jG.boj(b / Math.sqrt(f));
i.x = j.$jG.boj(a + l * c, 3);
i.y = j.$jG.boj(u + h * c, 3);
}
}
}
}
};
t.FP = function(e, n, i, r) {
var o = t.Ev, b = j.$jG.boj(2 * (e * i + n * r), 5);
o.x = j.$jG.boj(e - i * b, 3);
o.y = j.$jG.boj(n - r * b, 3);
return o;
};
t.FQ = function(e, n, i, r) {
var o = this.Eu;
o.x = j.$jG.boj(n * e.bmF + e.x, 3);
o.y = j.$jG.boj(i + e.y, 3);
o.z = j.$jG.boj(r + e.z, 3);
return t.Eu;
};
t.FR = function(e, n, i, r, o, b) {
var a = j.$jG.boj(i + e, 3), u = j.$jG.boj(r + n, 3);
-1 === o && (u = j.$jG.boj(-1 * u + 2 * n, 3));
j.$jG.boe(e, n, a, u, b, t.Ev);
return t.Ev;
};
t.FS = function(e, n) {
var i = j.$jG.boj(e * e + n * n, 5);
if (i > 0) {
i = j.$jG.boj(1 / Math.sqrt(i), 5);
e = j.$jG.boj(e * i, 5);
n = j.$jG.boj(n * i, 5);
} else {
e = 1;
n = 0;
}
var r = t.Ev;
r.x = e;
r.y = n;
return r;
};
t.FT = function(t, e, n) {
var i = m.$uD.$vj.c;
switch (n) {
case m.$uD.$vo.b:
i = t.hw.bmF;
break;

case m.$uD.$vo.c:
i = -1 * t.hw.bmF;
break;

case m.$uD.$vo.d:
i = Math.sign(e.hw.x - t.hw.x);
break;

case m.$uD.$vo.e:
i = Math.sign(t.hw.x - e.hw.x);
break;

case m.$uD.$vo.f:
i = e.hw.bmF;
break;

case m.$uD.$vo.g:
i = -1 * e.hw.bmF;
}
return i || m.$uD.$vj.c;
};
t.FU = function() {
return !1;
};
t.FV = function(e, n, i) {
var o, b, a, u, l, h, f, c, p, v, g, y, m, D, G, E, N, _, w, k = e.hi, O = e.hw, F = (null !== (o = n.dir) && void 0 !== o ? o : 1) * O.bmF, R = 0, P = 0, T = 0;
if (n.createSceneRandomPos) {
var I = e.hi.bnw(d.default);
R = j.$jG.boj(k.bnl.gg(I.bmu, I.bmv), 3);
P = j.$jG.boj(k.bnl.gg(I.bmw, I.bmx), 3);
T = 0;
} else if (n.randomInCircle) {
var A = n.randomInCircle, C = j.$jG.boj(k.bnl.gg(0, 2 * Math.PI), 6), S = j.$jG.boj(k.bnl.gg(A.innerR, A.outerR));
R = j.$jG.boj(O.x + (null !== (b = n.x) && void 0 !== b ? b : 0) * O.bmF + j.$jG.boj(Math.cos(C) * S), 3);
P = j.$jG.boj(O.y + (null !== (a = n.y) && void 0 !== a ? a : 0) + j.$jG.boj(Math.sin(C) * S), 3);
T = 0;
} else if (1 === n.entPos) {
if (i) {
var H = i;
R = null !== (u = H.hitX) && void 0 !== u ? u : 0;
P = null !== (l = H.hitY) && void 0 !== l ? l : 0;
T = null !== (h = H.hitZ) && void 0 !== h ? h : 0;
}
} else if (2 === n.entPos) {
if (i) {
var B = i.target;
R = (L = t.FQ(B.hw, j.$jG.boj((null !== (f = n.x) && void 0 !== f ? f : 0) + (n.xRand ? n.xRand * k.bnl.ge() : 0), 3), j.$jG.boj((null !== (c = n.y) && void 0 !== c ? c : 0) + (n.yRand ? n.yRand * k.bnl.ge() : 0), 3), j.$jG.boj((null !== (p = n.z) && void 0 !== p ? p : 0) + (n.zRand ? n.zRand * k.bnl.ge() : 0), 3))).x;
P = L.y;
T = L.z;
}
} else {
var L;
R = (L = t.FQ(O, j.$jG.boj((null !== (v = n.x) && void 0 !== v ? v : 0) + (n.xRand ? n.xRand * k.bnl.ge() : 0), 3), j.$jG.boj((null !== (g = n.y) && void 0 !== g ? g : 0) + (n.yRand ? n.yRand * k.bnl.ge() : 0), 3), j.$jG.boj((null !== (y = n.z) && void 0 !== y ? y : 0) + (n.zRand ? n.zRand * k.bnl.ge() : 0), 3))).x;
P = L.y;
T = L.z;
}
"number" == typeof n.worldZ && (T = n.worldZ);
"number" == typeof n.worldX && (R = n.worldX);
"number" == typeof n.worldY && (P = n.worldY);
var Y = e.hD ? e.hD.bkR : 0, W = -1, z = -1;
if (!n.ai) {
W = e.hg;
z = e.hE ? e.hE.bjO : -1;
}
var X = k.bnn();
t.FZ(X);
t.Gf(X);
t.EO(X, Y, 0, -1);
var K = X.hn(s.default);
t.FG(K, 0, 0, 0);
t.FH(K, "number" != typeof n.inside || Boolean(n.inside));
var Q = X.hn($.default);
t.FJ(Q, R, P, T, F, n.zorder);
var V = {};
if (n.extendsVals) for (var Z = e.hE.bjJ, J = 0, q = n.extendsVals; J < q.length; J++) V[nt = q[J]] = Z[nt] || 0;
if (n.initVars) for (var U = 0, tt = n.initVars; U < tt.length; U++) {
var et = tt[U], nt = et[0], it = et[1];
V[nt] = x.default.Dl(it, e, null);
}
if (n.attrInit || n.valueId) {
var rt = e.hv;
if (!rt) {
var ot = e.hi.bnu(e.hE.bjO);
ot && (rt = ot.hv);
}
var bt = null, at = 0;
if (n.valueId) {
var ut = $eZ.cfgMgr.tbl.EnemyValueTable.findByKey(n.valueId);
at = ut.level;
bt = M.default.bGR.bHW(ut);
}
if (rt && n.attrInit) {
bt = bt || {};
at = at || rt.bgH;
for (var lt = rt.bgb.dac, ht = 0, ft = n.attrInit; ht < ft.length; ht++) {
var ct = ft[ht], st = ct[0], pt = x.default.Dl(null !== (m = ct[1]) && void 0 !== m ? m : 1, e, null), dt = x.default.Dl(null !== (D = ct[2]) && void 0 !== D ? D : 0, e, null);
bt[st] = Math.floor((null !== (G = lt.cZI(st)) && void 0 !== G ? G : 0) * pt) + dt;
}
}
if (bt) {
X.hn(r.default).bgm();
t.EE(X, bt, n.roleId, at, null);
}
}
if (n.projectile) if ("number" == typeof n.projectile.angle) {
C = j.$jG.boj(n.projectile.angle * Math.PI / 180, 6);
var vt = j.$jG.boj(j.$jG.boj(Math.cos(C)) * n.projectile.speed) * X.hw.bmF, $t = j.$jG.boj(j.$jG.boj(Math.sin(C)) * n.projectile.speed);
this.FG(K, vt, $t, null !== (E = n.projectile.speedZ) && void 0 !== E ? E : 0);
} else this.FG(K, j.$jG.boj((null !== (N = n.projectile.speedX) && void 0 !== N ? N : 0) * X.hw.bmF), null !== (_ = n.projectile.speedY) && void 0 !== _ ? _ : 0, null !== (w = n.projectile.speedZ) && void 0 !== w ? w : 0);
n.appearanceId && t.EP(X, n.appearanceId);
if (n.behaviorId) {
t.ES(X, n.behaviorId, W, z, n.startStateId, V);
e.hv && (X.hE.bjZ = -1 !== e.hE.bjO ? e.hE.bjO : e.hg);
}
var gt = 0;
if (X.hv) {
if ("number" == typeof n.camp) t.EO(X, n.camp, 0, -2); else {
e.hD.bkS > 0 && (gt = M.GameEnemyTypeTableEnum.Normal);
t.EO(X, Y, gt, -2);
}
t.EQ(X, 1);
}
if (n.bindHitTarget) {
var yt = i && i.target;
if (yt && yt.hw) {
var mt = j.$jG.boj(X.hw.x - yt.hw.x + 0) * yt.hw.bmF, jt = j.$jG.boj(X.hw.y - yt.hw.y + 0), Dt = j.$jG.boj(X.hw.z - yt.hw.z + 0);
t.FW(X, yt, {
dir: 1,
x: mt,
y: jt,
z: Dt
});
}
}
if (n.ai) {
t.EL(X, n.ai);
t.Gv(X, !0);
}
return X;
};
t.FW = function(e, n, i) {
var r, o, b, a;
if (n.hw) {
e.hJ && t.FX(e);
var u = e.hn(g.default);
u.bkG = n.hg;
u.bkL = null !== (r = i.dir) && void 0 !== r ? r : 1;
u.bkI = i.x;
u.bkJ = i.y;
u.bkK = i.z;
u.bkM = null !== (o = i.fz) && void 0 !== o ? o : null;
u.bkH = null !== (b = i.type) && void 0 !== b ? b : 0;
if (1 === i.type) {
u.bkN = null !== (a = i.surroundSpeed) && void 0 !== a ? a : 0;
var l = t.Ev;
l.x = j.$jG.boj(e.hw.x - n.hw.x);
l.y = j.$jG.boj(e.hw.y - n.hw.y);
var h = j.$jG.boj(Math.atan2(l.y, l.x));
u.bkP = j.$jG.boj(180 * h / Math.PI);
u.bkO = j.$jG.boj(l.boA());
u.bkQ = 0;
} else {
u.bkN = 0;
u.bkP = 0;
u.bkO = 0;
u.bkQ = 0;
}
n.hE.bjV.push({
id: 0,
eid: e.hg
});
}
};
t.FX = function(t) {
if (t.hJ) {
var e = t.hi.bnu(t.hJ.bkG);
if (null == e ? void 0 : e.hE) {
var n = e.hE.bjV.findIndex(function(e) {
return e.eid === t.hg;
});
-1 !== n && e.hE.bjV.splice(n, 1);
}
t.hq(g.default);
}
};
t.FY = function(t, e) {
if (t.hE && t.hE.bjV) for (var n = t.hE.bjV.length - 1; n >= 0; n--) {
var i = t.hE.bjV[n];
if (-1 == e || i.id === e) {
var r = t.hi.bnu(i.eid);
if (r) {
r.hq(g.default);
t.hE.bjV.splice(n, 1);
}
}
}
};
t.FZ = function(e) {
var n = e.hn(p.default);
n.blV = 0;
n.blT = "";
n.bmf = 1;
n.bme = !1;
t.Ga(e);
return n;
};
t.Ga = function(t) {
var e = t.hx;
if (e) {
e.blX.bow(0, 0);
e.blY = 0;
e.blW = -1;
e.bma.forEach(function(t) {
t.c = 0;
t.b = 0;
t.d = 0;
t.e = 0;
t.f = -9999;
});
e.bmb.forEach(function(t) {
t.c = 0;
t.b = 0;
t.d = 0;
t.e = 0;
t.f = -9999;
});
e.bmc.length = 0;
}
};
t.Gb = function(t, e) {
if (e) {
var n = t.bma.get(e);
if (!n) {
n = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
t.bma.set(e, n);
}
return n;
}
};
t.Gc = function(t, e) {
var n = t.bmb.get(e);
if (!n) {
n = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
t.bmb.set(e, n);
}
return n;
};
t.Gd = function(t, e) {
return t.hL ? t.hL.blQ[e] : 0;
};
t.Ge = function(t, e, n) {
if (t.bmD < n) {
t.hitPauseTimeScale = e;
t.bmD = n;
}
};
t.Gf = function(t) {
var e = t.hn(v.default);
e.actionTimeScale = 1;
var n = t.hi.bnw(v.default);
e.worldTimeScale = n.worldTimeScale;
e.hitPauseTimeScale = 1;
e.mainTimeScale = 1;
e.bmD = 0;
e.actionTimeScale = 1;
return e;
};
t.Gg = function(e, n, r, o) {
for (var b = 0, a = r; b < a.length; b++) {
var u = a[b], l = !0, h = u.tgs;
if (h && h.length > 0) for (var f = 0, c = h.length; f < c; f++) {
var s = h[f], p = i.default.Di(e, n, s, o);
if (s[2] ? p : !p) {
l = !1;
break;
}
}
if (l) {
u.cs ? t.Gg(e, n, u.cs, o) : i.default.Dj(e, n, u, o);
if (u.break) break;
} else u.fcs && t.Gg(e, n, u.fcs, o);
}
};
t.Gh = function(e, n, i, r, o) {
var b, a, u, l, h, f, c, s = e.hE;
if (s) {
var p = M.default.bGR.bHu(n);
if (p) if (p.dot && p.abnormalType) M.default.bGR.bGV("buff " + n + " dot 与 abnormalType 互斥，已拒绝添加"); else {
if (p.abnormalType) {
if (null === (b = e.hv) || void 0 === b ? void 0 : b.bgt) return;
if (p.abnormalType <= m.$uD.$vp.j) {
if (s.bkq || s.bkt || !e.hv || !e.hv.bgI) return;
switch (p.abnormalType) {
case m.$uD.$vp.f:
for (var d = 0, v = s.bjB; d < v.length; d++) (w = v[d]).DF.abnormalType === m.$uD.$vp.f && t.Gj(e, w, !1);
}
}
}
var $ = null !== (a = p.addType) && void 0 !== a ? a : 1;
if (0 !== $) switch ($) {
case 1:
if (s.bjC[n]) return;
break;

case 2:
if (s.bjC[n]) for (var g = 0, y = s.bjB; g < y.length; g++) if ((w = y[g]).DE === n) {
w.DL = -1;
w.DM = 0;
return;
}
break;

case 3:
s.bjC[n] && t.Gk(e, n);
break;

case m.$uD.$vr.f:
if (s.bjC[n]) {
for (var D = null != r ? r : 0, G = null, E = 0, _ = s.bjB; E < _.length; E++) {
var w;
if ((w = _[E]).DE === n && w.DQ === D && -1 !== w.DO) {
G = w;
break;
}
}
if (G) {
if (p.dot) t.Go(e, G, i, o); else {
M.default.bGR.bGU("buff " + n + " addType=MERGE 仅 dot 支持，已退化为重置时间");
G.DL = -1;
G.DM = 0;
}
return;
}
}
}
var k = new N.$gb.$gd();
k.Eb(n);
k.DP = e.hg;
k.DQ = null != r ? r : 0;
null === (u = k.DF.param) || void 0 === u || u.forEach(function(t, n) {
var r;
k.DG.push(x.default.Dl(null !== (r = i && i[n]) && void 0 !== r ? r : t, e, k, o));
});
if (p.dot) {
var O = p.dot;
k.DX = Math.ceil(j.$jG.boj(null !== (l = k.DG[O.totalIdx]) && void 0 !== l ? l : 0));
if (k.DX <= 0) return;
k.DY = Math.max(1, Math.floor(x.default.Dl(null !== (h = O.ticks) && void 0 !== h ? h : 5, e, k, o)));
k.DZ = j.$jG.boj(x.default.Dl(null !== (f = O.interval) && void 0 !== f ? f : 1, e, k, o), 4);
k.DZ <= 0 && (k.DZ = 1);
k.Ea = 0;
}
s.bky(k);
if (p.specialType) switch (p.specialType) {
case m.$uD.$vs.b:
k.DU = Math.ceil(null !== (c = k.DG[0]) && void 0 !== c ? c : 0);
e.ia();
break;

case m.$uD.$vs.c:
if (e.hv) {
var F = e.hv.bhQ();
if (F) {
k.DU = F;
e.hv.bgU(F, 1);
}
}
}
k.DL = -1;
k.DM = 0;
k.DJ = x.default.Dl(p.duration, e, k);
k.DK = p.interval ? x.default.Dl(p.interval, e, k) : 0;
k.DN = k.DK;
if (k.DF.attr) for (var R = 0, P = k.DF.attr; R < P.length; R++) {
var T = P[R];
k.DH.push([ T[0], k.DG[T[1]] ]);
}
if (k.DF.attr2) for (var I = 0, A = k.DF.attr2; I < A.length; I++) {
T = A[I];
k.DI.push([ T[0], k.DG[T[1]] ]);
}
if (e.hv) {
for (var C = e.hv, S = 0, H = k.DH; S < H.length; S++) {
var B = H[S];
C.bgS(B[0], B[1]);
}
for (var L = 0, Y = k.DI; L < Y.length; L++) {
B = Y[L];
C.bgU(B[0], B[1]);
}
}
p.abnormalType && t.Gl(e, k);
p.enterLg && k.DP === e.hg && t.Gg(e, k, k.DF.enterLg, null);
}
}
};
t.Gi = function(t, e) {
if (e.DF.status) for (var n = 0, i = e.DF.status; n < i.length; n++) {
var r = i[n];
switch (r[0]) {
case "lockBtn":
break;

case "lockMinHP":
if (!t.hv) break;
for (var o = 0, b = 0, a = t.hE.bjB; b < a.length; b++) {
var u = a[b];
if (u.DF.status) for (var l = 0, h = u.DF.status; l < h.length; l++) {
var f = h[l];
f[0] === r[0] && o < f[1] && (o = f[1]);
}
}
t.hv.lockMinHP = o;
}
}
};
t.Gj = function(e, n, i) {
var r;
if (n.DP !== e.hg) return !1;
if (-1 === n.DO) return !1;
n.DO = -1;
null === (r = e.hE) || void 0 === r || r.bkz(n.DD);
var o = n.DF;
if (i) o.deleteLg && t.Gg(e, n, o.deleteLg, null); else {
if (o.specialType) switch (o.specialType) {
case m.$uD.$vs.c:
if (e.hv) {
var b = n.DU;
b && e.hv.bgV(b, 1);
}
}
o.abnormalType && t.Gm(e, n);
if (e.hv) {
for (var a = e.hv, u = 0, l = n.DH; u < l.length; u++) {
var h = l[u];
a.bgT(h[0], h[1]);
}
for (var f = 0, c = n.DI; f < c.length; f++) {
h = c[f];
a.bgV(h[0], h[1]);
}
}
o.exitLg && t.Gg(e, n, o.exitLg, null);
o.deleteLg && t.Gg(e, n, o.deleteLg, null);
}
return !0;
};
t.Gk = function(e, n) {
for (var i = e.hE.bjB, r = 0; r < i.length; r++) {
var o = i[r];
if (o.DF.id === n) {
t.Gj(e, o, !1);
break;
}
}
};
t.Gl = function(e, n) {
var i;
if (e.hv && e.hE) {
var r = n.DF.abnormalType, o = 1 << r - 1;
if (!e.hv.bga.bpD(o)) {
e.hv.bga.bpF(o);
if (r <= m.$uD.$vp.j) switch (r) {
case m.$uD.$vp.b:
t.Fd(e);
0 === e.hE.bjX.hurtbackTime && t.Fe(e, 0, 0, m.$uD.commonHitActions[1]);
break;

case m.$uD.$vp.c:
case m.$uD.$vp.g:
t.FG(e.hz, 0, 0, 0);
t.Fd(e);
t.Fv(e);
t.Fe(e, 0, 0, m.$uD.commonHitActions[1]);
}
}
if (r === m.$uD.$vp.k) {
var b = null === (i = n.DF) || void 0 === i ? void 0 : i.abnormalAttr;
if (b) {
n.DS = 0;
n.DT = Math.ceil(j.$jG.boj(void 0 !== b[0] ? n.DG[b[0]] : 0));
n.DR = void 0 !== b[1] ? n.DG[b[1]] : 1;
}
}
}
};
t.Gm = function(e, n) {
if (e.hv && e.hE) {
var i = n.DF.abnormalType, r = 1 << i - 1;
if (e.hv.bga.bpD(r) && !e.hE.bjB.find(function(t) {
return t.DF.abnormalType === i;
})) {
e.hv.bga.bpG(r);
if (i <= m.$uD.$vp.j) switch (i) {
case m.$uD.$vp.d:
case m.$uD.$vp.e:
t.EV(e);
e.hG && (e.hG.biw = m.$uD.$vh.b);
e.hx && (e.hx.blW = 0);
break;

case m.$uD.$vp.f:
break;

case m.$uD.$vp.b:
case m.$uD.$vp.c:
case m.$uD.$vp.g:
0 === e.hE.bjX.hurtbackTime && t.EV(e);
}
}
}
};
t.Gn = function(e, n, i) {
var r;
if (e.hv) {
n.DS = j.$jG.boj(n.DS + i);
if (n.DS > n.DR) {
n.DS = j.$jG.boj(n.DS - n.DR);
if (0 !== n.DT) {
var o = n ? e.hi.bnu(n.DQ) : null;
if (!e.hv.bgI) return;
var b = e;
if ((null == o ? void 0 : o.hv) && (null === (r = null == b ? void 0 : b.hv) || void 0 === r ? void 0 : r.bgI)) {
var a = e.hi.bnl, u = t.Ey(o, o, b);
u.isPhysical = o.hv.bhl > o.hv.bhm;
u.elementValue = 0;
var l = t.Gr(a, u, n.DT / M.GameAttributeConstRatioRange, !1);
e.hX(o, l);
!b.hv.bgI && t.Hf(b, 0, 0);
a.gu(m.$uD.$vA.E, b, l, !1);
t.Ez(u);
}
}
}
}
};
t.Go = function(t, e, n, i) {
var r, o, b, a = e.DF.dot;
if (a) {
var u = e.DF.param, l = null !== (o = null !== (r = n && n[a.totalIdx]) && void 0 !== r ? r : u && u[a.totalIdx]) && void 0 !== o ? o : 0, h = Math.ceil(j.$jG.boj(x.default.Dl(l, t, e, i)));
h > 0 && (e.DX += h);
e.DY = Math.max(1, Math.floor(x.default.Dl(null !== (b = a.ticks) && void 0 !== b ? b : 5, t, e, i)));
}
};
t.Gp = function(e, n, i) {
var r, o = e.hv;
if (o && o.bgI) {
n.Ea = j.$jG.boj(n.Ea + i, 4);
if (!(n.Ea < n.DZ)) {
n.Ea = j.$jG.boj(n.Ea - n.DZ, 4);
var b = Math.max(1, n.DY), a = b <= 1 ? n.DX : Math.ceil(j.$jG.boj(n.DX / b));
if (a > 0) {
var u = e.hi.bnl, l = e.hi.bnu(n.DQ);
e.hX(l, a);
if ((null == l ? void 0 : l.hv) && l.hx) {
var h = (null === (r = n.DF.dot) || void 0 === r ? void 0 : r.skId) || m.$uD.OTHER_DAMAGE_SKILL_ID;
u.gu(m.$uD.$vA.v, l, h, a);
}
u.gu(m.$uD.$vA.E, e, a, !1);
!o.bgI && t.Hf(e, 0, 0);
}
n.DX -= a;
n.DY = b - 1;
(n.DY <= 0 || n.DX <= 0) && t.Gj(e, n, !1);
}
}
};
t.Gq = function(t) {
var e = t.hz.bli;
if (e > 0) {
var n = t.hi.bnw(d.default);
if (t.hw.x >= n.bmv) {
t.hz.bli = j.$jG.boj(-1 * e * .2, 4);
if (t.hE.bke) {
t.hE.bkc = 1;
t.hE.bkf = 1;
t.hz.blk = t.hE.bke;
t.hE.bke = 0;
}
}
} else {
n = t.hi.bnw(d.default);
if (t.hw.x <= n.bmu) {
t.hz.bli = j.$jG.boj(-1 * e * .2, 4);
if (t.hE.bke) {
t.hE.bkc = 1;
t.hE.bkf = 1;
t.hz.blk = t.hE.bke;
t.hE.bke = 0;
}
}
}
};
t.Gr = function(t, e, n, i) {
var r, o = e.atkEntity.hv, b = e.target.hv, a = M.GameAttributeConstRatioRange, u = e.isPhysical, l = M.default.bGR.bGP, h = 1;
if (e.elementValue > 0) {
var f = Number.MIN_SAFE_INTEGER;
if (e.elementValue & m.$uD.$uY.b) {
f = Math.max(f, o.bhx - b.bhB);
m.$uD.$uY.b;
}
if (e.elementValue & m.$uD.$uY.c) {
f = Math.max(f, o.bhy - b.bhC);
m.$uD.$uY.c;
}
if (e.elementValue & m.$uD.$uY.d) {
f = Math.max(f, o.bhz - b.bhD);
m.$uD.$uY.d;
}
if (e.elementValue & m.$uD.$uY.e) {
f = Math.max(f, o.bhA - b.bhE);
m.$uD.$uY.e;
}
h = Math.max(j.$jG.boj(1 + l.streArg1 + j.$jG.boj(f / l.streArg2)), 0);
}
var c = o.bhn(u), s = o.bgH, p = b.bho(u), d = Math.min(j.$jG.boj(j.$jG.boj(p / (s * l.defArg1 + p)) + o.bhp(u) / a), u ? l.maxApDefRate : l.maxSpDefRate), v = Math.ceil(o.bhq(u)), $ = j.$jG.boj(v / (b.bgH * l.critArg1 + v * l.critArg2)), g = j.$jG.boj($ + o.bhr(u) / a), y = j.$jG.boj(o.bhs / a + l.baseCritDamage), D = o.bhF, G = Math.max(a + o.bht - b.bhu, 0), x = o.bhv, E = o.bhw, N = 1;
(null === (r = e.hitInfo) || void 0 === r ? void 0 : r.skId) && (N = o.bgb.dac.cZR(e.hitInfo.skId));
var _ = i && g > t.ge();
e.isCrt = _;
var w = j.$jG.boj(G / a * (a + x) / a * (a + E) / a * (a + D) / a * h * N * j.$jG.boj(1 - d) * (_ ? y : 1));
return Math.ceil(j.$jG.boj(c * j.$jG.boj(n * w)));
};
t.Gs = function() {};
t.Gt = function(t, e, n) {
t.blb = e.e;
t.bkZ.length = 0;
t.bkX.bkW(e.y);
t.bkY.bkW(e.z);
t.bla = e.d || 0;
"number" == typeof n && (t.bld = n);
return t;
};
t.Gu = function(t) {
return t < 90 && t > -90;
};
t.Gv = function(t, e) {
var n = t.hG;
n || (n = t.hn(b.default));
n.bis = -1;
n.biw = m.$uD.$vh.b;
n.bir = e;
n.bix = 0;
};
t.Gw = function(t) {
return t.bgI;
};
t.Gx = function(t, e) {
return this.Gy(t.x, t.y, e.x, e.y);
};
t.Gy = function(t, e, n, i) {
var r = n - t, o = i - e;
return j.$jG.boj(180 * Math.atan2(o, r) / Math.PI, 4);
};
t.Gz = function(t, e) {
var n = e.x - t.x, i = e.y - t.y;
return j.$jG.boj(Math.sqrt(n * n + i * i), 3);
};
t.GA = function(t, e) {
for (var n = 0, i = e; n < i.length; n++) {
var r = i[n];
t.findAnimation(r[0]) && t.findAnimation(r[1]) && t.setMix(r[0], r[1], r[2]);
}
};
t.GB = function(t, e) {
t.bjp = e;
var n = M.default.bGR.bHi(t.bjo), i = n && n[e];
if (i) {
t.bjq = i.cls;
t.bjr = i.status;
t.bjs = 0;
t.bjt = 0;
t.bju = {};
}
};
t.GC = function(e, n, i) {
var r = t.Gb(e, n);
if (r) {
r.b = i ? 1 : 0;
i ? r.c = 1 : r.d = 1;
}
};
t.GD = function(t, e) {
t.bmp = e;
var n = M.default.bGR.bHh(e);
if (n) {
t.bmq = n.visibleArea[0];
t.bmr = n.visibleArea[1];
t.bms = n.visibleArea[2];
t.bmt = n.visibleArea[3];
t.bmu = n.walkableArea[0];
t.bmv = n.walkableArea[1];
t.bmw = M.default.bGR.bHH(n.walkableArea[2]);
t.bmx = M.default.bGR.bHH(n.walkableArea[3]);
t.bmA = n;
t.bmy.length = 0;
}
};
t.GE = function() {};
t.GF = function(e) {
e.fr.bnf.forEach(function(e) {
e.hW ? e.hv && t.EV(e) : e.hi.bnl.gB(e);
});
};
t.GG = function(e) {
var n = e.fI;
(null == n ? void 0 : n.dcV) && n.dcJ.forEach(function(i, r) {
if (i) {
var o = r + 1, b = n.dcV.transfers.find(function(t) {
return Number(t.id) == o;
});
if (b) {
var a = e.gL(n.dcH, n.dcI, o), u = e.fH.get(a);
if (u) {
var l = e.fr.bnn(), h = l.hn(k.default);
h.bmj = o;
h.bmm = 0;
h.bmn = 0;
h.bmk = !1;
h.bmo = 0;
h.bml = u.dcY;
t.Gf(l);
var f = l.hn($.default);
t.FJ(f, b.x, M.default.bGR.bHH(b.y), 0, 1);
t.EP(l, b.resId);
t.ES(l, I.cvBehaviorId.entityBehavior101);
}
}
}
});
};
t.GH = function(e, n, i) {
var o = e.fr.bnn();
n.dbT = o.hg;
t.FZ(o);
t.Gf(o);
t.Gv(o, !0);
t.EO(o, n.dbQ, n.dbG, i);
var b = o.hn(s.default);
t.FG(b, 0, 0, 0);
t.FI(b, 0);
t.FH(b, !0);
var a = o.hn($.default);
t.FJ(a, n.dbJ, n.dbK, n.dbL, n.dbN);
var u = o.hn(r.default).bgm();
t.EE(o, n.dbR, n.dbH, n.dbO, null);
t.EP(o, u.bfZ.appearanceId);
t.ES(o, u.bhR(e.gN));
t.EQ(o, 1);
var l = o.hn(R.default);
l.bjj(1);
l.bje = !1;
n.dbS && t.EL(o, n.dbS);
return o;
};
t.GI = function(e) {
var n = e.fI;
n && n.dcR.forEach(function(i) {
if (i.dbW) {
if (n.dcU) return;
var o = e.fr.bnn();
n.dcT = o.hg;
t.Gf(o);
t.EO(o, i.dbQ, 0, 0);
var b = o.hn($.default);
t.FJ(b, i.dbJ, i.dbK, i.dbL, i.dbN);
var a = o.hn(s.default);
t.FG(a, 0, 0, 0);
t.FI(a, 0);
t.FH(a, !0);
o.hn(r.default).bgm();
t.EE(o, i.dbR, i.dbH, i.dbO, null);
t.EP(o, 210200240);
t.ES(o, I.cvBehaviorId.entityBehavior203);
t.EQ(o, 1);
} else if (!n.dcO) {
var u = e.fr.bnn();
i.dbT = u.hg;
t.FZ(u);
t.Gf(u);
t.Gv(u, !0);
t.EO(u, i.dbQ, i.dbG, i.dbU);
if (!e.fM.n) if (i.dbG === M.GameEnemyTypeTableEnum.Boss) {
(l = u.hn(R.default)).bjj(2);
l.bje = !0;
} else if (i.dbG === M.GameEnemyTypeTableEnum.Elite) {
var l;
(l = u.hn(R.default)).bjj(2);
}
var h = u.hn(s.default);
t.FG(h, 0, 0, 0);
t.FI(h, 0);
t.FH(h, !0);
var f = u.hn($.default);
t.FJ(f, i.dbJ, i.dbK, i.dbL, i.dbN);
var c = u.hn(r.default).bgm();
t.EE(u, i.dbR, i.dbH, i.dbO, null);
t.EP(u, c.bfZ.appearanceId);
t.ES(u, c.bhR(e.gN));
t.EQ(u, 1);
i.dbS && t.EL(u, i.dbS);
}
});
};
t.GJ = function(t, e) {
var n = $eZ.cfgMgr.tbl.EnemyRandomTable.VOList.filter(function(t) {
return t.name === e;
});
if (0 === n.length) return null;
var i = t.gf(n.map(function(t) {
return t.weight;
}));
return n[i];
};
t.GK = function(e) {
var n = e.fI;
if (n && !n.dcU) {
n.dcU = !0;
e.gP();
e.fI.dcO = !1;
for (var i = 0, o = e.fE.bmW; i < o.length; i++) {
var b = o[i], a = b.ho(k.default);
a.bmk = !1;
a.bmm = 0;
a.bmn = 0;
t.EY(b, "1");
}
t.GB(e.fr.bnw(c.default), m.$uD.$vf.c);
e.gu(m.$uD.$vA.t);
n.dcR.forEach(function(n) {
if (n.dbW) {
var i = M.GameEnemyTypeTableEnum.Boss, o = t.GJ(e, 1);
if (o) {
var b = o.roleCfgId, a = M.default.bGR.bHl(b);
if (a) {
var u = e.fr.bnn();
n.dbT = u.hg;
t.FZ(u);
t.Gf(u);
t.Gv(u, !0);
t.EO(u, n.dbQ, i, n.dbU);
var l = u.hn(R.default);
l.bjj(2);
l.bje = !0;
var h = u.hn(s.default);
t.FG(h, 0, 0, 0);
t.FI(h, 0);
t.FH(h, !0);
var f = u.hn($.default);
t.FJ(f, n.dbJ, n.dbK, n.dbL, n.dbN);
var c = u.hn(r.default).bgm();
t.EE(u, n.dbR, b, n.dbO, null);
t.EP(u, c.bfZ.appearanceId);
t.ES(u, c.bhR(e.gN));
t.EQ(u, 1);
t.Gh(u, I.cvResUrl.buff_11003, [ 1 ], u.hg);
a.AI && t.EL(u, a.AI);
}
}
}
});
}
};
t.GL = function(e, n) {
var i = e.fr, o = e.fI;
o && e.fF.forEach(function(b, a) {
var u, l, h = b.dcB[0], f = i.bnn();
b.dco = f.hg;
b.dcp = f.hg;
t.FZ(f).blT = b.dca;
var c = f.hn(E.default);
c.blR(b.dca, null === (u = h.personal) || void 0 === u ? void 0 : u.fashion, h.wear.equipList, null === (l = h.fashion) || void 0 === l ? void 0 : l.morphList, h.troop);
c.blS(h.roleCfgId, e.gM);
t.Gf(f);
t.Gv(f, !0);
t.EO(f, b.dcf, 0, -1);
var p = f.hn(s.default);
t.FG(p, 0, 0, 0);
t.FI(p, 0);
t.FH(p, !0);
var d = f.hn($.default);
if (n) {
var v = o.dcV.transfers.find(function(t) {
return Number(t.id) === n;
});
if (v) {
b.dcx = v.x;
b.dcy = M.default.bGR.bHH(v.y);
b.dcz = 0;
b.dcA = m.$uD.$vj.b;
}
} else {
var g = o.dcV.born || [ 500, 300, 1 ];
if (g) {
b.dcx = g[0];
b.dcy = M.default.bGR.bHH(g[1]) + 300 * (a > 0 ? a % 2 == 0 ? -Math.floor(a / 2) : Math.ceil(a / 2) : 0);
b.dcz = 0;
b.dcA = g[2];
}
}
t.FJ(d, b.dcx, b.dcy, b.dcz, b.dcA);
var y = f.hn(R.default);
y.bjj(1, e.gN);
y.bje = !1;
var j = f.hn(r.default).bgm();
t.EE(f, h.baseAttrs, h.roleCfgId, h.level, h);
var D = e.gC() ? 7100001 : j.bfZ.appearanceId;
t.EP(f, D);
t.ES(f, j.bhR(e.gN));
t.EQ(f, 1);
f.hV(!0);
h.appendInfo && h.appendInfo.assistCount > 0 && t.Gh(f, 11010, [ 1e3 * h.appendInfo.assistCount, 1e3 * h.appendInfo.assistCount, 1e3 * h.appendInfo.assistCount ], f.hg);
b.dce && t.EM(f);
});
};
t.GM = function(e) {
var n = e.fr;
if (e.fI) {
var i = e.fr.bnw(d.default), o = j.$jG.boj((i.bmw + i.bmx) / 2), b = j.$jG.boj((i.bmu + i.bmv) / 2), a = {};
a[1] = [ {
x: j.$jG.boj(b - 400),
y: o,
dir: 1
} ];
a[2] = [ {
x: j.$jG.boj(b + 400),
y: o,
dir: -1
} ];
e.fF.forEach(function(t) {
t.dcf = t.dcd;
});
e.fF.forEach(function(i) {
var o, b, u = a[i.dcf];
if (u) {
var l = i.dcB[0], h = $eZ.cfgMgr.tbl.MatchPvpAttrTable.findByKey(l.roleCfgId);
if (h) {
var f = n.bnn();
i.dco = f.hg;
i.dcp = f.hg;
t.FZ(f).blT = i.dca;
var c = f.hn(E.default);
c.blR(i.dca, null === (o = l.personal) || void 0 === o ? void 0 : o.fashion, l.wear.equipList, null === (b = l.fashion) || void 0 === b ? void 0 : b.morphList, l.troop);
c.blS(l.roleCfgId, e.gM);
t.Gf(f);
t.Gv(f, !0);
t.EO(f, i.dcf, 0, -1);
var p = f.hn(s.default);
t.FG(p, 0, 0, 0);
t.FI(p, 0);
t.FH(p, !0);
var d = f.hn($.default), v = u[0];
i.dcx = v.x;
i.dcy = v.y;
i.dcz = 0;
i.dcA = v.dir;
t.FJ(d, i.dcx, i.dcy, i.dcz, i.dcA);
var g = f.hn(R.default);
g.bjj(1, e.gN);
g.bje = !1;
var y = f.hn(r.default).bgm(), m = M.default.bGR.bHW(h);
t.EE(f, m, l.roleCfgId, h.lv, l);
t.EP(f, y.bfZ.appearanceId);
t.ES(f, y.bhR(e.gN));
t.EQ(f, 1);
f.hV(!0);
i.dca < 0 && t.EM(f);
}
}
});
}
};
t.GN = function(e, n, i) {
var o = e.fr;
if (e.fI) {
var b = M.default.bGR.bHs(n).map(function(t) {
return {
id: t,
level: 1
};
}), a = {
playerId: "",
baseAttrs: {},
roleCfgId: n,
skinId: 0,
personal: null,
skillDataList: [ {
type: T.TblEnum.SkillTypeTableEnum.PVE_SKILL,
skillData: b
}, {
type: T.TblEnum.SkillTypeTableEnum.PVP_SKILL,
skillData: b
} ],
wear: {
equipList: []
},
fashion: null,
troop: null,
archive: null,
guildBuffList: [],
adventureIndex: [],
mountIds: [],
appendInfo: null,
level: 60,
ai: 0,
hpRate: 1,
hp: 1
}, u = new m.$uD.$uF();
u.id = -1;
u.roleData = a;
u.camp = 2;
u.nick = "";
u.score = 0;
u.title = 0;
u.power = 0;
u.botAILv = 0;
u.position = 2;
u.heros = [ a ];
var l = new m.$uD.$uH(u), h = e.fF.findIndex(function(t) {
return t.dcd === u.position;
});
h >= 0 ? e.fF[h] = l : e.fF.push(l);
var f = e.fr.bnw(d.default), c = j.$jG.boj((f.bmw + f.bmx) / 2), p = j.$jG.boj((f.bmu + f.bmv) / 2), v = {};
v[1] = [ {
x: j.$jG.boj(p - 400),
y: c,
dir: 1
} ];
v[2] = [ {
x: j.$jG.boj(p + 400),
y: c,
dir: -1
} ];
var g = v[2];
if (g) {
var y = $eZ.cfgMgr.tbl.MatchPvpAttrTable.findByKey(n);
if (y) {
var D = o.bnn();
l.dco = D.hg;
l.dcp = D.hg;
t.FZ(D).blT = l.dca;
var G = D.hn(E.default);
G.blR(l.dca, null, [], [], null);
G.blS(n, e.gM);
t.Gf(D);
t.Gv(D, !0);
t.EO(D, 2, 0, -1);
var x = D.hn(s.default);
t.FG(x, 0, 0, 0);
t.FI(x, 0);
t.FH(x, !0);
var N = D.hn($.default), _ = g[0], w = _.x, k = _.y, O = _.dir;
t.FJ(N, w, k, 0, O);
var F = D.hn(R.default);
F.bjj(1, e.gN);
F.bje = !1;
var P = D.hn(r.default).bgm(), I = M.default.bGR.bHW(y);
t.EE(D, I, a.roleCfgId, y.lv, a);
t.EP(D, P.bfZ.appearanceId);
t.ES(D, P.bhR(e.gN));
t.EQ(D, 1);
D.hV(!0);
i && t.EM(D);
return D;
}
}
}
};
t.GO = function(t, e) {
if (!t.fN) {
var n = t.gk(e);
if (n) {
n.dcm = 0;
n.dcn = 0;
n.dcl += 1;
var i = t.fr.bnu(n.dco);
if (i && !i.hv.bgI) {
n.dcm = 2;
t.gu(m.$uD.$vA.bb, n.dca);
}
}
}
};
t.GP = function(e, n) {
if (!e.fN) {
e.fQ = 0;
if (0 === e.fR) for (var i = 0, r = e.fF; i < r.length; i++) {
var o = r[i], b = e.gi(o.dco);
b && (b.hv.bgI || (o.dcm = 0));
}
var a = e.gk(n);
if (a) {
a.dcm = 0;
a.dcn = 0;
a.dcl += 1;
e.gu(m.$uD.$vA.bb, a.dca);
var u = e.fr.bnu(a.dco);
if (u) {
u.hv.bhO();
if (!u.hv.bgI) {
e.fr.bnw(c.default);
u.hY(u, u.hv.bgE);
u.hZ(u, u.hv.bgG);
t.EY(u, String(u.hE.bkB));
t.Gh(u, 11008, [], u.hg);
}
}
}
}
};
t.GQ = function(e, n, i, r) {
var o, b = null === (o = e.hE) || void 0 === o ? void 0 : o.bjD[i];
if (b) for (var a = e.hE.bjB, u = 0, l = 0; l < a.length && u < b; l++) {
var h = a[l];
if (h.DF.event === i) {
u++;
t.Gg(e, h, h.DF.eventLg, r);
}
}
};
t.GR = function(e, n, i, r, o, b) {
var a, u = null === (a = e.hE) || void 0 === a ? void 0 : a.bjD[n];
if (u) for (var l = e.hE.bjB, h = 0, f = 0; f < l.length && h < u; f++) {
var c = l[f];
if (c.DF.event === n) {
h++;
c.DU = r;
c.DV = o;
c.DW = b;
t.Gg(e, c, c.DF.eventLg, i);
}
}
};
t.GS = function() {};
t.GT = function(t, e, n, i) {
var r, o = t.hE, b = o && o.bjJ;
switch (e[0]) {
case "==":
return (b[e[1]] || 0) === e[2];

case ">":
return (b[e[1]] || 0) > e[2];

case "<":
return (b[e[1]] || 0) < e[2];

case ">=":
return (b[e[1]] || 0) >= e[2];

case "<=":
return (b[e[1]] || 0) <= e[2];

case "==v":
return (b[e[1]] || 0) === (b[e[2]] || 0);

case ">v":
return (b[e[1]] || 0) > (b[e[2]] || 0);

case "<v":
return (b[e[1]] || 0) < (b[e[2]] || 0);

case ">=v":
return (b[e[1]] || 0) >= (b[e[2]] || 0);

case "<=v":
return (b[e[1]] || 0) <= (b[e[2]] || 0);

case "mp>=":
return !!t.hv && j.$jG.boj(t.hv.bgF / t.hv.bgG) >= e[1];

case "hp<=":
return !!t.hv && j.$jG.boj(t.hv.bgD / t.hv.bgE) <= e[1];

case "hpRange":
if (!t.hv) return !1;
var a = j.$jG.boj(t.hv.bgD / t.hv.bgE);
return a > e[1] && a <= e[2];

case "lostHp>=":
return !!t.hv && t.hv.bgE - t.hv.bgD >= e[1];

case "hpValue<=":
return !!t.hv && t.hv.bgD <= e[1];

case "isFlipY":
return !1;

case "isPlayer":
return t.hx.bmh;

case "isBoss":
return t.hS;

case "eventSkillBtn":
case "targetBehaviorId":
var u = i;
return !!u && !!u.target && (null === (r = u.target.hE) || void 0 === r ? void 0 : r.bjE) === e[1];

case "stateTime>=":
return !!t.hE && (b[e[1]] || 0) <= t.hE.bjL;

case "vel.z<":
return t.hz.blk < (b[e[1]] || 0);

case "joy.x*dir>":
return !!t.hx.blW && t.hx.blX.x * t.hw.bmF > e[1];

case "joy.x*dir<":
return !!t.hx.blW && t.hx.blX.x * t.hw.bmF < e[1];

case "abs(joy.y)>":
return !!t.hx.blW && Math.abs(t.hx.blX.y) > e[1];

case "joy.y>":
return !!t.hx.blW && t.hx.blX.y > e[1];

case "joy.y<":
return !!t.hx.blW && t.hx.blX.y < e[1];

case "a.z<":
return t.hw.z < e[1];

case "a.z>=":
return t.hw.z >= e[1];

case "a.lv<":
return t.hv.bgH < e[1];

case "a.lv>=":
return t.hv.bgH >= e[1];
}
return !1;
};
t.GU = function() {
return !1;
};
t.GV = function(t, e, n, i) {
var r = Math.abs(j.$jG.boj(e.x - t.x, 3)), o = Math.abs(j.$jG.boj(e.y - t.y, 3));
return r < n && o < i;
};
t.GW = function(e, n) {
if (e.fI) {
var i = e.gL(e.fI.dcH, e.fI.dcI, n), r = e.fH.get(i);
if (r) {
e.fJ = r.dcG;
e.gu(m.$uD.$vA.m);
e.fF.forEach(function(t) {
t.dcw = 0;
});
t.GB(e.gs(), m.$uD.$vf.d);
} else M.default.bGR.bGV("not found next room:", e.fI.dcH, e.fI.dcI, n);
}
};
t.GX = function(e) {
if (e.fI && e.fG.find(function(t) {
return t.dcG === e.fJ;
})) {
for (var n = !0, i = 0, r = e.fF; i < r.length; i++) {
var o = r[i];
if (!o.dcC && !o.dcw) {
n = !1;
break;
}
}
n && t.GY(e);
}
};
t.GY = function(e) {
if (e.fI && 0 !== e.fJ) {
var n = e.fG.find(function(t) {
return t.dcG === e.fJ;
});
if (n) {
e.fJ = 0;
e.fI.dcP = !0;
var i = -1, r = n.dcH - e.fI.dcH, o = n.dcI - e.fI.dcI;
if (1 === o) i = 1; else if (-1 === o) i = 2; else if (-1 === r) i = 3; else {
if (1 !== r) return;
i = 4;
}
this.GF(e);
var b = e.fI = n;
e.gQ();
e.gy();
var a = e.fr.bnw(d.default);
t.GD(a, b.dcM);
t.GG(e);
t.GI(e);
var u = e.gK(i);
e.fF.forEach(function(n, r) {
var o = e.gi(n.dco);
if ((null == o ? void 0 : o.hv) && t.Gw(o.hv)) {
t.ER(o);
if (i) {
var a = b.dcV.transfers.find(function(t) {
return Number(t.id) === u;
});
if (a) {
n.dcx = a.x;
n.dcy = M.default.bGR.bHH(a.y);
n.dcz = 0;
n.dcA = o.hw.bmF;
switch (i) {
case 1:
n.dcy += 355;
break;

case 2:
n.dcy -= 355;
break;

case 3:
n.dcx -= 205;
break;

case 4:
n.dcx += 205;
}
}
} else {
var l = b.dcV.born || [ 500, 300, 1 ];
if (l) {
n.dcx = l[0] + 350 * r;
n.dcy = M.default.bGR.bHH(l[1]);
n.dcz = 0;
n.dcA = l[2];
}
}
t.FJ(o.hw, n.dcx, n.dcy, n.dcz, n.dcA);
}
});
e.fF.forEach(function(t) {
t.dcw = 0;
});
t.GB(e.gs(), m.$uD.$vf.g);
e.gu(m.$uD.$vA.n);
}
}
};
t.GZ = function(e, n) {
if (e.fI) {
var i = e.fu.bmW;
if (e.fI.dcO) {
var r = e.fF, o = e.fE.bmW, b = void 0, a = 0, u = !1, l = 0;
r.forEach(function(t) {
e.gi(t.dco).hU && !t.dcC && l++;
});
for (var h = function(n) {
var i = n.ho(k.default);
if (!i.bmk) {
i.bmk = !0;
t.EY(n, "2");
}
i.bmo = 0;
r.forEach(function(r) {
var o = e.gi(r.dco);
o.hU && !r.dcC && o.hE.bkD && t.GV(o.hw, n.hw, 200, 350) && i.bmo++;
});
if (i.bmo > a) {
a = i.bmo;
b = i;
u = !1;
} else i.bmo === a && (u = !0);
}, f = 0, s = o; f < s.length; f++) h(B = s[f]);
if (!u && b) if (b.bmo === l) for (var p = 0, d = o; p < d.length; p++) if ((L = (B = d[p]).ho(k.default)).bmj === b.bmj) {
b.bmm = j.$jG.boj(b.bmm + n);
b.bmn = 0;
if (b.bmm > .5) {
t.GW(e, L.bmj);
return;
}
} else {
L.bmn = 0;
L.bmm = 0;
} else for (var v = 0, $ = o; v < $.length; v++) if ((L = (B = $[v]).ho(k.default)).bmj === b.bmj) {
b.bmm = 0;
b.bmn = j.$jG.boj(b.bmn + n);
if (b.bmm > 5) {
t.GW(e, L.bmj);
return;
}
} else {
L.bmn = 0;
L.bmm = 0;
} else for (var g = 0, y = o; g < y.length; g++) {
(L = (B = y[g]).ho(k.default)).bmn = 0;
L.bmm = 0;
}
return !1;
}
for (var D = 0, G = 0, x = 0, E = i; x < E.length; x++) {
var M = E[x];
if (M.hD.bkS > 0) if (M.hS) {
if (M.hv.bgI) {
G++;
D++;
}
} else M.hv.bgf || D++;
}
for (var N = 0, _ = 0, w = 0, O = e.fF; w < O.length; w++) {
var F = O[w];
(A = e.gi(F.dco)) && A.hv.bgI && N++;
}
if (0 === e.fR) for (var R = 0, P = e.fF; R < P.length; R++) {
F = P[R];
if ((A = e.gi(F.dco)) && !A.hv.bgI) {
if (2 !== F.dcm) if (F.dcl < e.fP) {
if (0 === F.dcm || 1 === F.dcm) {
F.dcm = 1;
A.hi.bnl.gu(m.$uD.$vA.bb, F.dca);
}
} else F.dcm = 2;
2 !== F.dcm && _++;
}
} else for (var T = 0, I = e.fF; T < I.length; T++) {
var A;
F = I[T];
if ((A = e.gi(F.dco)) && !A.hv.bgI) {
if (2 !== F.dcm) if (F.dcl < e.fP) if (0 === e.fR) {
if (0 === F.dcm || 1 === F.dcm) {
F.dcm = 1;
A.hi.bnl.gu(m.$uD.$vA.bb, F.dca);
}
} else {
F.dcn = j.$jG.boj(F.dcn + n);
if (F.dcn >= e.fR) {
F.dcm = 2;
A.hi.bnl.gu(m.$uD.$vA.bb, F.dca);
} else if (0 === F.dcm || 1 === F.dcm) {
F.dcm = 1;
A.hi.bnl.gu(m.$uD.$vA.bb, F.dca);
}
} else F.dcm = 2;
2 !== F.dcm && _++;
}
}
var C = !1;
if (e.fn) {
if (!e.fn) return !1;
C = e.fk >= e.fn;
}
if (e.fI.dcY) {
if (!e.fI.dcO && (0 === D || 0 === G && !1 !== e.fM.o)) {
e.fI.dcO = !0;
t.Hb(e.fr.bnv());
t.GB(e.fr.bnw(c.default), m.$uD.$vf.e);
e.fI.dde = e.fk;
e.gu(m.$uD.$vA.l);
e.gu(m.$uD.$vA.p);
return !0;
}
} else if (!e.fI.dcO && 0 === D) {
e.fI.dcO = !0;
for (var S = 0, H = o = e.fE.bmW; S < H.length; S++) {
var B, L;
(L = (B = H[S]).ho(k.default)).bmk = !0;
t.EY(B, "2");
}
e.fI.dde = e.fk;
t.GB(e.fr.bnw(c.default), m.$uD.$vf.f);
e.gu(m.$uD.$vA.l);
return !0;
}
if (C && N > 0) {
e.fI.dcO = !1;
t.GB(e.fr.bnw(c.default), m.$uD.$vf.h);
return !0;
}
if (_ + N === 0) {
t.Ha(e);
return !0;
}
return !1;
}
};
t.Ha = function(e) {
e.fI.dcO = !1;
t.GB(e.fr.bnw(c.default), m.$uD.$vf.i);
};
t.Hb = function(e) {
for (var n = 0, i = e.hi.bnl.gF(); n < i.length; n++) {
var r = i[n];
r.hX(r, r.hv.bgD);
t.Hf(r, 400, 500);
}
};
t.Hc = function(t) {
var e = 0;
if (t.hE && t.hE.bkl.length) {
var n = t.hE.bkl.find(function(t) {
return 1 === t.b.type;
});
if (n) {
var i = t.hi.bnu(n.f);
(null == i ? void 0 : i.hA) && (e = i.hA.bkY.bkU[0].world.bnQ.bnP);
}
}
return e;
};
t.Hd = function() {
return !0;
};
t.He = function() {};
t.Hf = function(e, n, i) {
if (e.hv) {
e.hv.bgD = 0;
e.hE.bkf = 1;
t.Fq(e);
t.Fd(e);
var r = {
z: i,
hitDirType: 0,
force: n
};
t.Ft(e, r, -1 * e.hw.bmF);
}
};
t.Hg = function(e) {
var n;
t.FG(e.hz, 0, 0, 0);
if (e.hv) {
e.hv.bhN();
(e.hR || -2 === (null === (n = e.hD) || void 0 === n ? void 0 : n.bkT) || 1 === e.hv.bgg) && t.Fh(e);
if (e.hE) {
for (var i = 0, r = e.hE.bjB; i < r.length; i++) {
var o = r[i];
o.DF.removeBuffOnDie && t.Gj(e, o, !1);
}
t.GQ(e, null, m.$uD.$vx.e);
}
e.hi.bnl.gu(m.$uD.$vA.x, e);
}
e.hy && (e.hy.hitPauseTimeScale = 1);
};
t.Hh = function(t) {
if (t.hD.bkS > 0) {
var e = t.hi.bnl.fI, n = {
b: null == e ? void 0 : e.dcG,
c: t.hD.bkT,
d: t.hw.x,
e: t.hw.y,
f: t.hw.z
};
t.hi.bnl.gu(m.$uD.$vA.q, n, t);
}
};
t.Hi = function(t, e, n, i) {
var r = t.hE.bjJ;
switch (e[0]) {
case "=Random":
r[e[1]] = Math.floor(1e4 * t.hi.bnl.ge());
break;

case "+=dt":
var o = t.hy, b = j.$jG.boj(t.hi.bnl.fp * o.bmE, 4);
r[e[1]] = j.$jG.boj((r[e[1]] || 0) + b, 4);
break;

case "=":
r[e[1]] = e[2];
break;

case "=v":
r[e[1]] = r[e[2]] || 0;
break;

case "+=":
r[e[1]] = (r[e[1]] || 0) + e[2];
break;

case "-=":
r[e[1]] = (r[e[1]] || 0) - e[2];
break;

case "*=":
r[e[1]] = Math.floor((r[e[1]] || 0) * e[2]);
break;

case "/=":
r[e[1]] = Math.floor((r[e[1]] || 0) / e[2]);
break;

case "+":
r[e[1]] = (r[e[2]] || 0) + (r[e[3]] || 0);
break;

case "-":
r[e[1]] = (r[e[2]] || 0) - (r[e[3]] || 0);
break;

case "*":
r[e[1]] = Math.floor((r[e[2]] || 0) * (r[e[3]] || 0));
break;

case "/":
r[e[1]] = Math.floor((r[e[2]] || 0) / (r[e[3]] || 0));
break;

case "mp+":
var a = t.hv;
if (a) {
a.bgF = Math.max(Math.min(a.bgF + x.default.Dl(e[1], t, n), a.bgG), 0);
t.hi.bnl.gu(m.$uD.$vA.A, t);
}
break;

case "aimDisCenterX":
if (t.hG && -1 !== t.hG.bis) {
var u = t.hi.bnu(t.hG.bis);
t.hw && (null == u ? void 0 : u.hw) && t.hw.x;
}
break;

case "targetVar=":
if (i) {
var l = i.target;
r[e[1]] = l.hE.bjJ[e[1]];
}
break;

case "setSkVal":
r[e[1]] = x.default.Dl(e[2], t, n);
break;

case "=pos":
r[e[1]] = t.hw.x;
e[2] && (r[e[2]] = t.hw.y);
e[3] && (r[e[3]] = t.hw.z);
break;

case "+=f":
r[e[1]] = j.$jG.boj((r[e[1]] || 0) + x.default.Dl(e[2], t, n));
break;

case "v1->v2":
var h = r[e[1]] || 0, f = r[e[2]] || 0, c = e[3] || 0, s = e[4] || 0, p = e[5] || 0, d = Math.floor(j.$jG.boj(h / c));
if (d > 0) {
r[e[1]] = j.$jG.boj(h - j.$jG.boj(d * c));
r[e[2]] = Math.min(j.$jG.boj(f + d * s), p);
}
break;

case "lockHp":
t.hv.lockMinHP = Math.ceil(x.default.Dl(e[1], t, n));
break;

case "WBState":
var v = t.hv.finalHurtValue;
r.WBState = M.default.bGR.bIy(v);
break;

case "=castProg":
r[e[1]] = j.$jG.boj(t.hE.bjY.currentTime / t.hE.bjY.time);
}
};
t.Hj = function(e) {
var n = e.hi.bnl;
if (n) {
var i = n.gk(e.hx.blT);
if (i) {
var o = i.dcB[0];
if (o) {
var b = e.hE;
if (b.bjB.length > 0) for (var a = 0, u = b.bjB.slice(); a < u.length; a++) {
var l = u[a];
t.Gj(e, l, !1);
}
t.EV(e);
if (n.gM) {
e.hn(r.default).bgm();
t.EE(e, o.baseAttrs, o.roleCfgId, o.level, o);
} else {
var h = $eZ.cfgMgr.tbl.MatchPvpAttrTable.findByKey(o.roleCfgId);
if (!h) return;
e.hn(r.default).bgm();
var f = M.default.bGR.bHW(h);
t.EE(e, f, o.roleCfgId, h.lv, o);
}
t.EQ(e, 1);
for (var c = 0, s = e.hv.bgd; c < s.length; c++) {
var p = s[c];
p.maxCdTime = e.hv.bhK(p);
p.cd = 0;
}
}
}
}
};
t.Hk = function(e, n, i, r) {
var o, b, a = e.hi.bnl;
if (a) {
var u = a.gk(e.hx.blT);
if (u) {
var l = u.dcB[0];
if (l && l.troop) {
l.troop.pet = r;
l.wear.equipList = i;
l.skillDataList = n;
t.Hj(e);
var h = e.hn(E.default);
h.blR(u.dca, null === (o = l.personal) || void 0 === o ? void 0 : o.fashion, l.wear.equipList, null === (b = l.fashion) || void 0 === b ? void 0 : b.morphList, l.troop);
h.blS(l.roleCfgId, a.gM);
a.gu(m.$uD.$vA.bs, e);
}
}
}
};
t.Hl = function(t, e) {
var n, i, r = t.hi.bnl;
if (r) {
var o = r.gk(t.hx.blT);
if (o) {
var b = o.dcB[0];
if (b && b.troop) {
b.troop.skillButton = e;
var a = t.hn(E.default);
a.blR(o.dca, null === (n = b.personal) || void 0 === n ? void 0 : n.fashion, b.wear.equipList, null === (i = b.fashion) || void 0 === i ? void 0 : i.morphList, b.troop);
a.blS(b.roleCfgId, r.gM);
r.gu(m.$uD.$vA.bt, t);
}
}
}
};
t.Hm = function(e, n) {
var i = n[0];
n[1];
switch (i) {
case m.$uD.$vz.b:
t.GK(e);
break;

case m.$uD.$vz.c:
e.gu(m.$uD.$vA.bx, m.$uD.$vz.c);
}
};
t.Et = [];
t.Eu = new _.default();
t.Ev = new D.default();
t.Ew = new D.default();
t.Ex = [];
t.EA = [];
return t;
}();
n.default = A;
cc._RF.pop();
}, {
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../main/src/$KM": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$JZ": "$JZ",
"./$KB": "$KB",
"./BattleBehaviorLogic": "BattleBehaviorLogic",
"./component/$MM": "$MM",
"./component/$MN": "$MN",
"./component/$MO": "$MO",
"./component/$MP": "$MP",
"./component/$MQ": "$MQ",
"./component/$MR": "$MR",
"./component/$MS": "$MS",
"./component/$MT": "$MT",
"./component/$MU": "$MU",
"./component/$MV": "$MV",
"./component/$MW": "$MW",
"./component/$MX": "$MX",
"./component/$MY": "$MY",
"./component/$MZ": "$MZ",
"./component/$ND": "$ND",
"./component/$NF": "$NF",
"./component/$NG": "$NG",
"./component/$NH": "$NH",
"./component/$NI": "$NI",
"./component/$NJ": "$NJ",
"./component/$NL": "$NL",
"./component/$NM": "$NM",
"./math/$NP": "$NP",
"./math/$NQ": "$NQ",
"./math/$NR": "$NR",
"./system/$NW": "$NW",
"./utils/$OG": "$OG"
} ],
$KD: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "bf300swXHBGALb+VrIhFtl3", "$KD");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = t("./$KC"), r = t("./math/$NP"), o = t("./lib/$NN"), b = t("./component/$MM"), a = t("./component/$NL"), u = t("./$KG"), l = t("./system/$NS"), h = t("./system/$NV"), f = t("./system/$OA"), c = t("./system/$OB"), s = t("./system/$OC"), p = t("./system/$OE"), d = t("./system/$NY"), v = t("./system/$NU"), $ = t("./component/$MV"), g = t("./system/$NX"), y = t("./system/$NT"), m = t("./system/$NZ"), j = t("../../game/src/battle/types/$WW"), D = t("../../game/src/battle/types/$WY"), G = t("./$KF"), x = t("./component/$NJ"), E = t("../../game/src/battle/types/$WU"), M = t("./system/$OF"), N = t("./$KA"), _ = t("./component/$NM"), w = t("./component/$MO"), k = t("./component/$ND"), O = t("./component/$MZ"), F = t("./component/$MX"), R = t("./component/$MW"), P = t("./component/$MQ"), T = t("../../game/src/battle/$QM"), I = t("./component/$MS"), A = t("./component/$MR"), C = t("./component/$NI"), S = t("../../game/src/battle/$QN"), H = t("./system/$NW"), B = t("../../game/src/battle/types/$WR"), L = t("./system/$OD"), Y = t("../../bingo/src/utils/$LZ"), W = t("../../main/src/tbl/TblEnum"), z = function() {
function t() {
this.fb = !1;
this.fc = null;
this.fd = !1;
this.fe = !1;
this.ff = 0;
this.fh = 30;
this.fi = 0;
this.fj = null;
this.fk = 0;
this.fl = 0;
this.fm = 0;
this.fn = 0;
this.fo = 0;
this.fp = 0;
this.fq = 0;
this.fr = null;
this.fs = null;
this.ft = null;
this.fu = null;
this.fv = null;
this.fw = null;
this.fx = null;
this.fy = null;
this.fz = null;
this.fA = null;
this.fB = null;
this.fC = null;
this.fD = null;
this.fE = null;
this.fF = [];
this.fH = new Map();
this.fJ = 0;
this.fM = null;
this.fN = !1;
this.fO = !1;
this.fP = 0;
this.fQ = 0;
this.fR = 10;
this.fS = !1;
this.fU = 1;
this.fV = 1;
this.fW = null;
}
Object.defineProperty(t.prototype, "fg", {
get: function() {
return this.ff;
},
enumerable: !1,
configurable: !0
});
t.prototype.fX = function() {};
t.prototype.fY = function() {
return null;
};
t.prototype.fZ = function(t, e, n) {
T.default.bGR.bIP();
if (Y.ANTI_CHEAT_ENABLED) {
Y.$ia.bam.baw();
Y.$ib.baO.bbb();
Y.$ie.bbw.bbB(80);
}
this.fr = new o.$je.$ju(u.$gn, o.$je.getComponentConstructors(), this);
this.fs = new o.$je.$js();
this.fs.add(new s.$jW(this.fr));
this.fs.add(new m.$jT(this.fr));
this.fs.add(new y.$jK(this.fr));
this.fs.add(new L.$jX(this.fr));
this.fs.add(new l.$jJ(this.fr));
this.fs.add(new d.$jS(this.fr));
this.fs.add(new v.$jL(this.fr));
this.fs.add(new f.default(this.fr));
this.fs.add(new h.$jO(this.fr));
this.fs.add(new H.$jQ(this.fr));
this.fs.add(new g.$jR(this.fr));
this.fs.add(new M.$jZ(this.fr));
this.fs.add(new p.$jY(this.fr));
this.fs.add(new c.$jV(this.fr));
this.fs.init();
this.ft = this.fr.bnq(o.$je.allOf(a.default));
this.fu = this.fr.bnq(o.$je.allOf(b.default));
this.fv = this.fr.bnq(o.$je.allOf(w.default, _.default));
this.fw = this.fr.bnq(o.$je.allOf(_.default, a.default, k.default));
this.fx = this.fr.bnq(o.$je.allOf(_.default, F.default));
this.fy = this.fr.bnq(o.$je.allOf(_.default, O.default));
this.fz = this.fr.bnq(o.$je.allOf(R.default));
this.fA = this.fr.bnq(o.$je.allOf(P.default));
this.fB = this.fr.bnq(o.$je.allOf(I.default));
this.fC = this.fr.bnq(o.$je.allOf(A.default));
this.fE = this.fr.bnq(o.$je.allOf(C.default));
this.fi = 0;
this.fq = 0;
this.gc(D.$uD.LOGIC_FRAME_RATE);
this.gd(n);
this.fj = t;
this.fk = 0;
this.fF = e.map(function(t) {
return new D.$uD.$uH(t);
});
this.gw();
this.gx();
this.fb = !0;
return !0;
};
t.prototype.ga = function(t, e, n) {
this.fe = !0;
this.fZ(t, e, n);
};
t.prototype.gb = function() {
this.fb = !1;
i.default.ED();
if (this.fs) {
this.fs.destroy();
this.fs = null;
}
this.ft = null;
this.fu = null;
this.fv = null;
this.fw = null;
this.fx = null;
this.fy = null;
this.fz = null;
this.fA = null;
this.fB = null;
this.fC = null;
this.fD = null;
this.fE = null;
this.fJ = 0;
if (this.fr) {
this.fr.bny();
this.fr = null;
}
this.fi = 0;
this.fq = 0;
};
t.prototype.gc = function(t) {
this.fh = Math.max(t, 10);
this.fp = r.$jG.boj(1 / this.fh, 6);
};
t.prototype.gd = function(t) {
this.ff = t;
};
t.prototype.ge = function() {
var t = (9301 * this.ff + 49297) % 233280;
return this.ff = t, t / 233280;
};
t.prototype.gf = function(t) {
for (var e = 0, n = 0; n < t.length; n++) e += t[n];
var i = this.ge() * e, r = 0;
for (n = 0; n < t.length; n++) if (i <= (r += t[n])) return n;
return 0;
};
t.prototype.gg = function(t, e) {
return Math.floor(this.ge() * (e - t + 1)) + t;
};
t.prototype.gh = function(t, e) {
var n = this;
this.fd = e;
this.fi++;
this.fq = r.$jG.boj(this.fi * this.fp, 5);
var o = this.gs().bjr;
t && t.forEach(function(t) {
var e, o = n.gl(t.position);
if (o) {
var b = n.gi(o.dcp);
if (b) {
var a = b.hx;
if (t.events && t.events.length > 0) for (var u = 0, l = t.events; u < l.length; u++) l[u].relive && i.default.GP(n, o.dca);
for (var h = 0, f = t.frames; h < f.length; h++) {
var c = f[h], s = c & Math.pow(2, 9) - 1;
if (s > E.$uv.A) {
var p = N.battleCmdClassCtorMap.get(s);
if (p) {
var d = new p();
d.eZ(c);
d.fa(n, o);
}
} else {
if (s) {
a.blV = 1;
a.blW = 1;
var v = s - 1;
(null === (e = b.hv) || void 0 === e ? void 0 : e.bgJ) > 0 && (v += 180);
var $ = r.$jG.boj(v * Math.PI / 180, 6);
a.blY = $;
a.blZ = v;
a.blX.x = r.$jG.boj(Math.cos($));
a.blX.y = r.$jG.boj(Math.sin($));
} else {
a.blV = 0;
a.blW = 0;
}
for (var g = c >> 9, y = 1; y <= E.MAX_FRAME_SYNC_CMDBIT; y++) {
var m = y, j = g >> y - 1 & 1, D = E.FRAME_SYNC_BIT2CMD[m], G = i.default.Gb(a, D);
if (G) {
var x = Number(j);
G.b = x;
G.e = 0;
if (x) {
G.f = n.fq;
G.c = 1;
} else G.d = 1;
}
}
}
}
}
}
});
var b = o.canCtrl;
this.fF.forEach(function(t) {
var e = n.gi(t.dcp);
e && (e.hx.bmf = b);
});
this.fs.execute(this.fp);
this.fF.forEach(function(t) {
var e = n.gi(t.dcp);
e && (e.hx.bmc.length = 0);
});
this.fi % 60 == 0 && this.gH();
};
t.prototype.gi = function(t) {
return this.fr ? this.fr.bnu(t) : null;
};
t.prototype.gj = function() {
var t, e, n;
return null !== (n = null === (e = null === (t = this.fL) || void 0 === t ? void 0 : t.partyHpMultiplier) || void 0 === e ? void 0 : e[this.fF.length - 1]) && void 0 !== n ? n : 1;
};
t.prototype.gk = function(t) {
return this.fF.find(function(e) {
return e.dca === t;
});
};
t.prototype.gl = function(t) {
return this.fF.find(function(e) {
return e.dcd === t;
});
};
t.prototype.gm = function() {
return this.fr ? this.fr.bnw(a.default).worldTimeScale : 0;
};
t.prototype.gn = function(t) {
if (this.fU !== t) {
this.fU = t;
this.gp();
}
};
t.prototype.go = function(t) {
if (this.fV !== t) {
this.fV = t;
this.gp();
}
};
t.prototype.gp = function() {
if (this.fr) {
var t = this.fV, e = this.fr.bnw(a.default);
if (e.worldTimeScale !== t) {
e.worldTimeScale = t;
for (var n = 0, i = this.ft.bmW; n < i.length; n++) i[n].hy.worldTimeScale = t;
}
}
};
t.prototype.gq = function(t, e) {
this.fo = t.hg;
for (var n = 0, i = this.ft.bmW; n < i.length; n++) {
var r = i[n];
r.hg === t.hg || e && r.hg === e ? r.hy.mainTimeScale = 1 : r.hy.mainTimeScale = 0;
}
};
t.prototype.gr = function() {
this.fo = 0;
for (var t = 0, e = this.ft.bmW; t < e.length; t++) e[t].hy.mainTimeScale = 1;
};
t.prototype.gs = function() {
var t;
return null === (t = this.fr) || void 0 === t ? void 0 : t.bnw($.default);
};
t.prototype.gt = function(t) {
return this.fr ? this.fr.bnw(t) : null;
};
t.prototype.gu = function(t, e, n, i, r, o, b) {
this.fc && (void 0 !== b ? this.fc(t, e, n, i, r, o, b) : void 0 !== o ? this.fc(t, e, n, i, r, o) : void 0 !== r ? this.fc(t, e, n, i, r) : void 0 !== i ? this.fc(t, e, n, i) : void 0 !== n ? this.fc(t, e, n) : void 0 !== e ? this.fc(t, e) : this.fc(t));
};
t.prototype.gv = function() {
return [];
};
t.prototype.gw = function() {
var t, e, n, i, o, b, a, u, l, h, f, c, s, p = this;
this.fN = !1;
var d = this.fj.instanceId;
this.fK = T.default.bGR.bHc(d);
if (!this.fK) {
console.error("no instanceCfg");
return !1;
}
this.fL = T.default.bGR.bHd(this.fK.type);
if (!this.fL) {
console.error("no instanceTypeCfg");
return !1;
}
this.fM = j.BattleModeConstant[this.fK.type];
if (!this.fM) {
console.error("no modeConstant");
return !1;
}
this.fS = null !== (t = this.fM.e) && void 0 !== t && t;
this.fj.traning && this.fj.traningParam && (this.fS = null !== (n = null === (e = this.fj.traningParam) || void 0 === e ? void 0 : e.d) && void 0 !== n && n);
if (T.GameInstanceTypeTableEnum.RAID === this.fK.type) {
var v = T.default.bGR.bIE(d);
v && (this.fP = v.reviveMaxCount);
} else {
this.fP = null !== (i = this.fL.reliveCount) && void 0 !== i ? i : 2;
this.fP > 0 && S.$lS.bJc(null === (b = null === (o = this.fj) || void 0 === o ? void 0 : o.svrInstanceParams) || void 0 === b ? void 0 : b.playMode) && (this.fP = 99999);
}
var $ = T.default.bGR.bHg(d), g = !1;
switch (this.fM.f) {
case j.$uy.b:
this.fm = 1;
this.fj.guide && (this.fm = 100);
this.fn = null !== (a = this.fK.timeLimit) && void 0 !== a ? a : 0;
this.fR = 0;
var y = this.gj();
this.fG = [];
if (!(R = $[0])) return !1;
if (this.fj.traning) {
this.fm = 4;
var m = new D.$uD.$uI();
m.dcG = 1;
m.dcJ = [];
m.dcH = 0;
m.dcI = 0;
m.dcO = !1;
m.dcP = !1;
m.dcY = !0;
m.dcN = !0;
m.dcM = this.fj.sceneId;
var G = m.dcV = T.default.bGR.bHh(m.dcM);
if (!G) {
var x = $eZ.cfgMgr.Json.get("scenes");
for (var E in x) {
m.dcM = E;
m.dcV = x[E];
break;
}
}
this.fG.push(m);
this.fH.set(this.gI(m.dcH, m.dcI), m);
(null === (u = this.fj.traningParam) || void 0 === u ? void 0 : u.e) && this.gM && this.fj.traningParam.e.forEach(function(t, e) {
var n = t.heros[0], i = new D.$uD.$uE();
i.dbH = n.roleCfgId;
i.dbG = T.GameEnemyTypeTableEnum.Normal;
i.dbO = i.dbO || 60;
i.dbR = n.baseAttrs;
i.dbQ = 2;
i.dbS = 0;
i.dbT = -1;
i.dbU = e;
var r = G.born || [ 500, 300, 1 ];
i.dbJ = r[0] + 300 * (e + 1);
i.dbK = T.default.bGR.bHH(r[1]);
i.dbL = 0;
i.dbN = D.$uD.$vj.b;
m.dcR.push(i);
});
} else for (var M = function(t) {
var e, n = new D.$uD.$uI();
n.dcL = t;
n.dcO = !1;
n.dcP = !1;
n.dcG = t.room;
n.dcJ = t.transferCode.slice(0);
n.dcK = ((e = {})[1] = n.dcJ[0] ? 1 : 0, e[2] = n.dcJ[1] ? 1 : 0, e[3] = n.dcJ[2] ? 1 : 0, 
e[4] = n.dcJ[3] ? 1 : 0, e);
n.dcH = t.mapX;
n.dcI = t.mapY;
n.dcY = t.roomType === T.GameRoomTypeTableEnum.BOSS || 1 === $.length;
n.dcN = t.roomType === T.GameRoomTypeTableEnum.BEGIN || 1 === $.length;
if (!n.dcL) {
T.default.bGR.bGV("不存在的房间:" + n.dcG);
g = !0;
return "break";
}
n.dcM = n.dcL.sceneName;
var i = n.dcV = T.default.bGR.bHh(n.dcM);
if (!i) {
T.default.bGR.bGV("不存在的地图场景:" + n.dcG + "/" + n.dcM);
g = !0;
return "break";
}
var o = null !== (l = n.dcL.enemyId) && void 0 !== l ? l : [], b = null !== (h = n.dcL.enemyValue) && void 0 !== h ? h : [], a = null !== (f = n.dcL.enemyPos) && void 0 !== f ? f : [], u = i.role, d = S.$lS.bJd(null === (s = null === (c = N.fj) || void 0 === c ? void 0 : c.svrInstanceParams) || void 0 === s ? void 0 : s.playMode), v = i.walkableArea[0], m = i.walkableArea[1], j = T.default.bGR.bHH(i.walkableArea[2]), G = T.default.bGR.bHH(i.walkableArea[3]), x = m - v, E = G - j, M = Math.ceil((v + m) / 2), _ = Math.ceil((j + G) / 2), w = $eZ.cfgMgr.tbl.EnemyValueTable;
o.forEach(function(t, e) {
var i = w.findByKey(b[e]);
if (i) {
if (i.type === T.GameEnemyTypeTableEnum.Hell) {
if (!d) return;
n.dcS = !0;
}
var o = T.default.bGR.bHl(t);
if (o) {
var l = new D.$uD.$uE();
l.dbH = t;
l.dbG = i.type;
l.dbO = i.level;
l.dbR = T.default.bGR.bHW(i, y);
l.dbQ = 2;
l.dbS = o.AI;
l.dbT = -1;
l.dbU = e;
var h = a[e];
l.dbL = -1;
if (h) {
var f = u.find(function(t) {
return t.id === h;
});
if (f) {
l.dbJ = f.x;
l.dbK = T.default.bGR.bHH(f.y);
l.dbL = 0;
l.dbN = f.dir;
} else T.default.bGR.bGV("not find enemy pos:" + t + " " + h);
}
if (-1 === l.dbL) {
l.dbJ = M + Math.floor(r.$jG.boj(r.$jG.boj(.62 * r.$jG.boj(p.ge() - .5)) * x));
l.dbK = _ + Math.floor(r.$jG.boj(r.$jG.boj(.62 * r.$jG.boj(p.ge() - .5)) * E));
l.dbL = 0;
l.dbN = D.$uD.$vj.c;
if (i.type === T.GameEnemyTypeTableEnum.Hell) {
l.dbJ = M;
l.dbK = _;
l.dbL = 0;
l.dbN = D.$uD.$vj.b;
}
}
n.dcR.push(l);
}
} else {
T.default.bGR.bGV("enemy value not find:" + b[e]);
i = w.VOList[0];
}
});
N.fG.push(n);
N.fH.set(N.gI(n.dcH, n.dcI), n);
}, N = this, _ = 0, w = $; _ < w.length && "break" !== M(k = w[_]); _++) ;
break;

case j.$uy.c:
this.fm = 2;
this.fn = 0;
this.fR = 0;
this.fG = [];
var k = $[0];
(P = new D.$uD.$uI()).dcL = k;
P.dcO = !1;
P.dcP = !1;
P.dcG = k.room;
P.dcJ = [];
P.dcK = {};
P.dcH = 0;
P.dcI = 0;
P.dcY = !1;
P.dcN = !1;
var O = $eZ.cfgMgr.tbl.RoomPvpInfoTable.VOList, F = O[Math.floor(this.ge() * O.length)];
this.fn = F.time;
P.dcM = F.sceneName;
P.dcV = T.default.bGR.bHh(P.dcM);
this.fG.push(P);
this.fH.set(this.gI(P.dcH, P.dcI), P);
break;

case j.$uy.f:
this.fm = 7;
this.fn = 0;
this.fR = 0;
if (!(R = $[0])) return !1;
this.fG = [];
(P = new D.$uD.$uI()).dcG = 1;
P.dcJ = [];
P.dcH = 0;
P.dcI = 0;
P.dcO = !1;
P.dcP = !1;
P.dcY = !0;
P.dcN = !0;
P.dcM = R.sceneName;
if (!(P.dcV = T.default.bGR.bHh(P.dcM))) {
x = $eZ.cfgMgr.Json.get("scenes");
for (var E in x) {
P.dcM = E;
P.dcV = x[E];
break;
}
}
this.fG.push(P);
this.fH.set(this.gI(P.dcH, P.dcI), P);
break;

case j.$uy.g:
this.fm = 8;
this.fn = 0;
this.fR = 0;
var R, P;
if (!(R = $[0])) return !1;
this.fG = [];
(P = new D.$uD.$uI()).dcG = 1;
P.dcJ = [];
P.dcH = 0;
P.dcI = 0;
P.dcO = !1;
P.dcP = !1;
P.dcY = !0;
P.dcN = !0;
P.dcM = R.sceneName;
if (!(P.dcV = T.default.bGR.bHh(P.dcM))) {
x = $eZ.cfgMgr.Json.get("scenes");
for (var E in x) {
P.dcM = E;
P.dcV = x[E];
break;
}
}
this.fG.push(P);
this.fH.set(this.gI(P.dcH, P.dcI), P);
}
return g;
};
t.prototype.gx = function() {
switch (this.fM.f) {
case j.$uy.b:
this.fI = this.fG.find(function(t) {
return t.dcN;
});
this.fJ = 0;
if (!this.fI) return;
this.gQ();
this.fr.bnv().hV(!0);
this.gy();
(e = this.fr.bnw($.default)).bjw(0);
e.bjo = this.fm;
i.default.GB(e, D.$uD.$vf.b);
var t = this.fr.bnw(x.default);
i.default.GD(t, this.fI.dcM);
i.default.GG(this);
i.default.GL(this, 0);
i.default.GI(this);
break;

case j.$uy.c:
this.fI = this.fG[0];
this.fJ = 0;
if (!this.fI) return;
this.fr.bnv().hV(!0);
this.gy();
(e = this.fr.bnw($.default)).bjw(0);
e.bjo = this.fm;
i.default.GB(e, D.$uD.$vf.b);
t = this.fr.bnw(x.default);
i.default.GD(t, this.fI.dcM);
i.default.GM(this);
break;

case j.$uy.f:
this.fI = this.fG.find(function(t) {
return t.dcN;
});
this.fJ = 0;
if (!this.fI) return;
this.fr.bnv().hV(!0);
this.gy();
(e = this.fr.bnw($.default)).bjw(0);
e.bjo = this.fm;
i.default.GB(e, D.$uD.$vf.b);
t = this.fr.bnw(x.default);
i.default.GD(t, this.fI.dcM);
i.default.GL(this, 0);
break;

case j.$uy.g:
this.fI = this.fG[0];
this.fJ = 0;
if (!this.fI) return;
this.fr.bnv().hV(!0);
this.gy();
var e;
(e = this.fr.bnw($.default)).bjw(0);
e.bjo = this.fm;
i.default.GB(e, D.$uD.$vf.b);
t = this.fr.bnw(x.default);
i.default.GD(t, this.fI.dcM);
i.default.GM(this);
}
};
t.prototype.gy = function() {
this.fT = {
type: D.$uD.$uW.c,
target: 0,
fixedZoomRatio: 1,
cameraLerpRatio: .3,
ignoreCameraCorrection: !1,
offsetX: 0,
offsetY: 0
};
};
t.prototype.gz = function() {
this.fN = !0;
this.fW = this.gT();
this.gu(D.$uD.$vA.br);
};
t.prototype.gA = function(t, e) {
for (var n, i = t.slice(), r = t.length, o = 0; o < e; o++) {
var b = Math.floor(this.ge() * (r - o)) + o;
n = [ i[b], i[o] ], i[o] = n[0], i[b] = n[1];
}
i.length = e;
return i;
};
t.prototype.gB = function(t) {
i.default.EX(t);
t.hP();
};
t.prototype.gC = function() {
return !!this.fj.guide;
};
t.prototype.gD = function() {
var t, e, n;
return null !== (n = null === (e = null === (t = this.gs()) || void 0 === t ? void 0 : t.bjr) || void 0 === e ? void 0 : e.guideId) && void 0 !== n ? n : 0;
};
t.prototype.gE = function() {
var t;
return null === (t = this.gs()) || void 0 === t ? void 0 : t.bjr;
};
t.prototype.gF = function() {
for (var t, e = [], n = 0, i = this.fu.bmW; n < i.length; n++) {
var r = i[n];
r.hv.bgI && 1 != (null === (t = r.hD) || void 0 === t ? void 0 : t.bkR) && e.push(r);
}
return e;
};
t.prototype.gG = function() {
this.go(0);
this.fr.bnw($.default).bjw(3, [ this.fF[0].dca ]);
};
t.prototype.gH = function() {};
t.prototype.gI = function(t, e) {
return t + 1e4 * e;
};
t.prototype.gJ = function(t, e) {
return this.fH.get(this.gI(t, e));
};
t.prototype.gK = function(t) {
switch (t) {
case 1:
return 2;

case 2:
return 1;

case 3:
return 4;

case 4:
return 3;
}
return 0;
};
t.prototype.gL = function(t, e, n) {
switch (n) {
case 1:
return this.gI(t, e + 1);

case 2:
return this.gI(t, e - 1);

case 3:
return this.gI(t - 1, e);

case 4:
return this.gI(t + 1, e);
}
return 0;
};
Object.defineProperty(t.prototype, "gM", {
get: function() {
return !this.fS;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(t.prototype, "gN", {
get: function() {
return !!this.fS;
},
enumerable: !1,
configurable: !0
});
t.prototype.gO = function() {
return this.fG.find(function(t) {
return t.dcS && !t.dcU;
}) || this.fG.find(function(t) {
return t.dcY;
});
};
t.prototype.gP = function() {
for (var t = 0, e = this.fG; t < e.length; t++) e[t].ddb = -1;
this.gQ();
};
t.prototype.gQ = function() {
var t = this.fI;
if (t && -1 === t.ddb) {
t.ddb = 0;
if (this.fG.length > 1) {
for (var e = this.gO(), n = void 0, i = void 0, r = [], o = 0, b = this.fG; o < b.length; o++) {
var a = b[o], u = {
b: a.dcH,
c: a.dcI,
d: 0,
e: 0,
i: 0,
j: a.dcK
};
r[u.b] || (r[u.b] = []);
r[u.b][u.c] = u;
a === t && (n = u);
a === e && (i = u);
}
if (n && i) {
var l = B.aStar(n, i, r);
if (l.length > 0 && l[1]) {
var h = l[1].b - l[0].b, f = l[1].c - l[0].c;
t.ddb = h ? 1 === h ? 4 : 3 : 1 === f ? 1 : 2;
}
}
}
}
};
Object.defineProperty(t.prototype, "gR", {
get: function() {
var t, e;
return null !== (e = null === (t = this.fI) || void 0 === t ? void 0 : t.dcG) && void 0 !== e ? e : 0;
},
enumerable: !1,
configurable: !0
});
t.prototype.gS = function() {
var t, e = null === (t = this.fu) || void 0 === t ? void 0 : t.bmW;
if (!e) return 0;
for (var n = 0, i = 0, r = e; i < r.length; i++) {
var o = r[i];
if (o.hS) {
n += o.hv.finalHurtValue;
break;
}
}
return Math.floor(n);
};
t.prototype.gT = function() {
for (var t = [], e = 0, n = this.fF; e < n.length; e++) {
var i = n[e], r = this.gi(i.dco);
if (r) {
var o = r.hv.bgD, b = r.hv.bgE, a = r.hv.bgF, u = r.hv.bhl, l = r.hv.bhm, h = r.hw.x, f = r.hw.y, c = r.hv.finalDamage, s = r.hv.finalHurtValue;
t.push({
id: i.dca,
hp: o,
hpMax: b,
mp: a,
ap: u,
sp: l,
x: h,
y: f,
totalDamage: c,
totalHurt: s
});
}
}
var p = this.fI, d = 0;
p && (d = p.dcO ? 1 : 0);
return {
frameId: this.fi,
result: d,
randomSeed: this.ff,
battleTime: this.fk,
players: t
};
};
t.prototype.gU = function() {
return G.computeCheckNum(this);
};
t.prototype.gV = function(t, e, n, r) {
i.default.Hk(t, e, n, r);
};
t.prototype.gW = function(t, e) {
i.default.Hl(t, e);
};
t.prototype.gX = function(t, e) {
i.default.Hj(t);
this.gY(t, e);
};
t.prototype.gY = function(t, e) {
if (e.bGE[e.playerCoolingTime.bGw]) {
var n = e.bGE[e.playerCoolingTime.bGw][0];
i.default.Gk(t, 11013);
i.default.Gh(t, 11013, [ 1e4 * n / 100 ], t.hg, null);
} else i.default.Gk(t, 11013);
if (e.bGG[e.playerCriticalStrikeRateIncrease.bGw]) {
n = e.bGG[e.playerCriticalStrikeRateIncrease.bGw][0];
i.default.Gk(t, 11014);
i.default.Gh(t, 11014, [ 1e4 * n / 100 ], t.hg, null);
} else i.default.Gk(t, 11014);
if (e.bGH[e.playerSpeedIncrease.bGw]) {
n = e.bGH[e.playerSpeedIncrease.bGw][0];
i.default.Gk(t, 11015);
i.default.Gh(t, 11015, [ 1e4 * n / 100 ], t.hg, null);
} else i.default.Gk(t, 11015);
};
t.prototype.gZ = function() {
var t = this.fr.bnw($.default);
t.bjw(0);
i.default.GB(t, 102);
};
t.prototype.ha = function() {
var t = this.fr.bnw($.default);
t.bjw(0);
i.default.GB(t, 101);
};
t.prototype.hb = function() {
var t = this.fr.bnw($.default);
t.bjw(0);
i.default.GB(t, 2);
};
t.prototype.hc = function(t) {
i.default.Hj(t);
};
t.prototype.hd = function(t, e) {
if (T.default.bGR.bHl(t)) {
var n = 1 === e.enemyFight.bGw, r = i.default.GN(this, t, n);
r.hv.bgM = 1;
r.hv.bgg = 1;
if (r.hN) {
0 === e.airProcect.bGw && (r.hN.bjg = -1);
0 === e.lieProcect.bGw && (r.hN.bjh = -1);
0 === e.standProcect.bGw && (r.hN.bjf = -1);
}
}
};
t.prototype.he = function() {
var t = this.fF.findIndex(function(t) {
return 2 === t.dcd;
});
t >= 0 && this.fF.splice(t, 1);
};
Object.defineProperty(t.prototype, "hf", {
get: function() {
var t, e, n;
return null !== (n = null === (e = null === (t = this.fj) || void 0 === t ? void 0 : t.svrInstanceParams) || void 0 === e ? void 0 : e.playMode) && void 0 !== n ? n : W.TblEnum.InstancePlayModeTableEnum.NORMAL;
},
enumerable: !1,
configurable: !0
});
return t;
}();
n.default = z;
window.$fK = window.$fK || {};
$fK.$gl = z;
cc._RF.pop();
}, {
"../../bingo/src/utils/$LZ": void 0,
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/$QN": void 0,
"../../game/src/battle/types/$WR": void 0,
"../../game/src/battle/types/$WU": void 0,
"../../game/src/battle/types/$WW": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$KA": "$KA",
"./$KC": "$KC",
"./$KF": "$KF",
"./$KG": "$KG",
"./component/$MM": "$MM",
"./component/$MO": "$MO",
"./component/$MQ": "$MQ",
"./component/$MR": "$MR",
"./component/$MS": "$MS",
"./component/$MV": "$MV",
"./component/$MW": "$MW",
"./component/$MX": "$MX",
"./component/$MZ": "$MZ",
"./component/$ND": "$ND",
"./component/$NI": "$NI",
"./component/$NJ": "$NJ",
"./component/$NL": "$NL",
"./component/$NM": "$NM",
"./lib/$NN": "$NN",
"./math/$NP": "$NP",
"./system/$NS": "$NS",
"./system/$NT": "$NT",
"./system/$NU": "$NU",
"./system/$NV": "$NV",
"./system/$NW": "$NW",
"./system/$NX": "$NX",
"./system/$NY": "$NY",
"./system/$NZ": "$NZ",
"./system/$OA": "$OA",
"./system/$OB": "$OB",
"./system/$OC": "$OC",
"./system/$OD": "$OD",
"./system/$OE": "$OE",
"./system/$OF": "$OF"
} ],
$KE: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "200dcjMwZVCE7edEZDT63kp", "$KE");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.default = function() {
this._randomSeed = 0;
this.frameRate = 0;
this.logicFrameID = 0;
this.battleInfo = null;
this.battleTime = 0;
this.isPvp = 0;
this.currentRoomIndex = 0;
this.battleProcessId = 0;
this.battleDurationLimit = 0;
this.isComplete = !1;
this.frozenTimeEid = 0;
this.reviveMaxTimes = 0;
this.reviveDelayTime = 0;
this.reviveOvertimeDuration = 0;
this.context = null;
this.players = [];
this.roomDataArray = [];
};
cc._RF.pop();
}, {} ],
$KF: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "b242ezcukFN9IGNGkg5jO6l", "$KF");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.fnv1a32 = n.computeCheckNum = void 0;
n.computeCheckNum = function(t) {
for (var e = 0, n = 0, i = t.fF; n < i.length; n++) {
var r = i[n], o = t.gi(r.dco);
o && (e += o.hv.finalDamage);
}
return e;
};
n.fnv1a32 = function(t) {
for (var e = 2166136261, n = 0; n < t.length; n++) e = (e ^= 255 & t[n]) + (e << 1) + (e << 4) + (e << 7) + (e << 8) + (e << 24) >>> 0;
return e >>> 0;
};
cc._RF.pop();
}, {} ],
$KG: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "3935aJcVctE9bLr5hHbkyAG", "$KG");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$gn = void 0;
var o = t("./lib/$NN"), b = t("./component/$MM"), a = t("./component/$MO"), u = t("./component/$MP"), l = t("./component/$MQ"), h = t("./component/$MS"), f = t("./component/$MW"), c = t("./component/$MY"), s = t("./component/$MZ"), p = t("./component/$NB"), d = t("./component/$MV"), v = t("./component/$ND"), $ = t("./component/$NH"), g = t("./component/$NL"), y = t("./component/$NM"), m = t("./component/$MN"), j = t("./component/$MX"), D = t("./component/$NA"), G = t("./component/$NG"), x = t("../../game/src/battle/$QM"), E = t("./component/$NE"), M = t("../../game/src/battle/types/$WY"), N = t("./component/$MT"), _ = t("./$KC"), w = t("./system/$NW"), k = t("./component/$MU"), O = t("./math/$NP"), F = t("./component/$NF"), R = null;
function P(t, e, n) {
t[e.tid] = n;
}
function T() {
var t = [];
P(t, b.default, function(t, e) {
return t.hv = e;
});
P(t, y.default, function(t, e) {
return t.hw = e;
});
P(t, $.default, function(t, e) {
return t.hx = e;
});
P(t, g.default, function(t, e) {
return t.hy = e;
});
P(t, v.default, function(t, e) {
return t.hz = e;
});
P(t, s.default, function(t, e) {
return t.hA = e;
});
P(t, l.default, function(t, e) {
return t.hB = e;
});
P(t, h.default, function(t, e) {
return t.hC = e;
});
P(t, c.default, function(t, e) {
return t.hD = e;
});
P(t, f.default, function(t, e) {
return t.hE = e;
});
P(t, a.default, function(t, e) {
return t.hF = e;
});
P(t, u.default, function(t, e) {
return t.hG = e;
});
P(t, d.default, function(t, e) {
return t.hH = e;
});
P(t, m.default, function(t, e) {
return t.hI = e;
});
P(t, j.default, function(t, e) {
return t.hJ = e;
});
P(t, D.default, function(t, e) {
return t.hK = e;
});
P(t, G.default, function(t, e) {
return t.hL = e;
});
P(t, N.default, function(t, e) {
return t.hM = e;
});
P(t, k.default, function(t, e) {
return t.hN = e;
});
P(t, F.default, function(t, e) {
return t.hO = e;
});
return t;
}
var I = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.hv = null;
e.hw = null;
e.hx = null;
e.hy = null;
e.hz = null;
e.hA = null;
e.hB = null;
e.hC = null;
e.hD = null;
e.hE = null;
e.hF = null;
e.hG = null;
e.hH = null;
e.hI = null;
e.hJ = null;
e.hK = null;
e.hL = null;
e.hM = null;
e.hN = null;
e.hO = null;
return e;
}
e.prototype.hk = function(t, e) {
var n = (R || (R = T()))[t];
void 0 !== n && n(this, e);
};
e.prototype.hl = function() {
for (var t = R || (R = T()), e = 0; e < t.length; e++) {
var n = t[e];
void 0 !== n && n(this, this.hj[e]);
}
};
e.prototype.hP = function() {
if (this.hM) {
w.$jQ.bpr(this);
this.hq(N.default);
}
this.hn(p.$iS);
};
e.prototype.hQ = function() {
return this.hp(p.$iS);
};
Object.defineProperty(e.prototype, "hR", {
get: function() {
var t;
return (null === (t = this.hD) || void 0 === t ? void 0 : t.bkS) > 0 && this.hD.bkS !== x.GameEnemyTypeTableEnum.Boss;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hS", {
get: function() {
var t;
return (null === (t = this.hD) || void 0 === t ? void 0 : t.bkS) === x.GameEnemyTypeTableEnum.Boss;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hT", {
get: function() {
var t;
return !(null === (t = this.hD) || void 0 === t || !t.bkS || this.hD.bkS !== x.GameEnemyTypeTableEnum.Boss && this.hD.bkS !== x.GameEnemyTypeTableEnum.Elite);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hU", {
get: function() {
var t;
return null === (t = this.hv) || void 0 === t ? void 0 : t.bgI;
},
enumerable: !1,
configurable: !0
});
e.prototype.hV = function(t) {
t ? this.hn(E.default) : this.hq(E.default);
};
Object.defineProperty(e.prototype, "hW", {
get: function() {
return !!this.ho(E.default);
},
enumerable: !1,
configurable: !0
});
e.prototype.hX = function(t, e, n) {
var i, r, o;
if (this.hv && !(e <= 0)) {
for (var b = this.hv, a = e; -1 !== b.bgs; ) {
var u = this.hE.bjB[b.bgs];
if ((null === (i = null == u ? void 0 : u.DF) || void 0 === i ? void 0 : i.specialType) === M.$uD.$vs.b) {
if (!((a -= u.DU) > 0)) {
a = 0;
u.DU -= e;
break;
}
u.DU = 0;
b.bgs = -1;
_.default.Gj(this, u, !1);
this.ia();
} else b.bgs = -1;
}
var l = b.bgD;
b.bgD = Math.max(b.bgD - a, b.lockMinHP);
b.finalHurtValue += e;
if (b.bgM && b.bgD <= 0) {
b.bgD = b.bgE;
if (this.hN) {
this.hN.biX = 1;
this.hN.bjk();
this.hN.bjm();
this.hN.bjl();
}
}
if (!b.bgI) {
this.hS && this.hi.bnl.fI && this.hi.bnl.fI.ddc.push(this.hg);
_.default.Hh(this);
}
if (null == t ? void 0 : t.hv) {
t.hv.finalDamage += e;
(null === (r = null == n ? void 0 : n.hitInfo) || void 0 === r ? void 0 : r.skId) && t.hi.bnl.gu(M.$uD.$vA.v, t, n.hitInfo.skId, e);
}
if (null === (o = this.hE) || void 0 === o ? void 0 : o.bjD[M.$uD.$vx.f]) {
var h = l - b.bgD;
_.default.GR(this, M.$uD.$vx.f, null, h);
}
this.hi.bnl.gu(M.$uD.$vA.y, this);
}
};
e.prototype.hY = function(t, e) {
if (this.hv && !(e <= 0)) {
var n = this.hv;
n.bgD = Math.min(n.bgD + e, n.bgE);
this.hi.bnl.gu(M.$uD.$vA.y, this);
}
};
e.prototype.hZ = function(t, e) {
var n = this.hv;
n && (n.bgF = Math.min(n.bgF + e, n.bgG));
};
e.prototype.ia = function() {
var t, e, n = this.hv;
if (n) {
if (-1 !== n.bgs) {
var i = this.hE.bjB[n.bgs];
(null === (t = null == i ? void 0 : i.DF) || void 0 === t ? void 0 : t.specialType) !== M.$uD.$vs.b && (n.bgs = -1);
}
if (-1 === n.bgs) for (var r = 0, o = this.hE.bjB; r < o.length; r++) {
var b = o[r];
if ((null === (e = b.DF) || void 0 === e ? void 0 : e.specialType) === M.$uD.$vs.b) {
n.bgs = b.DD;
if (-1 !== n.bgs) break;
}
}
}
};
e.prototype.ib = function() {
this.hM && w.$jQ.bpr(this);
this.hi.bnp(this);
};
Object.defineProperty(e.prototype, "ic", {
get: function() {
return this.hv ? this.hv.bgx <= 0 : this.hz.blr;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "id", {
get: function() {
var t;
return (null === (t = this.hx) || void 0 === t ? void 0 : t.bmh) || !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "ie", {
get: function() {
var t;
return null === (t = this.hi.bnl) || void 0 === t ? void 0 : t.fI;
},
enumerable: !1,
configurable: !0
});
e.prototype._if = function() {
var t = this.hN;
if (t) {
t.bjm();
t.bjl();
}
};
e.prototype.ig = function() {
var t = this.hN;
if (t) {
if (-1 !== t.bjg && -1 === t.biY && t.biX > t.bjg) {
t.biY = t.biX;
t.biZ = this.hi.bnl.fk;
}
t.bjk();
}
};
e.prototype.ih = function() {
var t = this.hN;
if (t && -1 !== t.bjf) {
-1 === t.bjc && t.biX > t.bjf && (t.bjc = t.biX);
if (-1 !== t.bjc) {
t.bjd = this.hi.bnl.fk;
if (O.$jG.boj(Math.max(t.bjc - this.hv.bgO(), 0)) > t.bjf) {
t.bjk();
_.default.Ff(this);
}
}
}
};
e.prototype.ii = function() {
var t = this.hN;
if (t && -1 !== t.bjh && -1 === (null == t ? void 0 : t.bja) && t.biX > t.bjh) {
t.bja = t.biX;
t.bjb = this.hi.bnl.fk;
}
};
e.prototype.ij = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.biY) {
var e = O.$jG.boj(Math.max(t.biY - this.hv.bgO(), 0));
return e < t.bjg ? 1 : O.$jG.boj(Math.max(.7 - e, 0));
}
return 1;
};
e.prototype.ik = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.bja) {
var e = O.$jG.boj(Math.max(t.bja - this.hv.bgO(), 0));
return 0 === e ? 1 : Math.max(O.$jG.boj(1 - O.$jG.boj(e / t.bjh)), t.bji);
}
return 1;
};
e.prototype.il = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.bja) {
var e = O.$jG.boj(Math.max(t.bja - this.hv.bgO(), 0));
return 0 === e ? 1 : O.$jG.boj(1 - O.$jG.boj(e / t.bjh));
}
return 1;
};
Object.defineProperty(e.prototype, "im", {
get: function() {
var t, e;
return (null === (t = this.hv) || void 0 === t ? void 0 : t.bgN) > 0 || (null === (e = this.hE) || void 0 === e ? void 0 : e.bkr);
},
enumerable: !1,
configurable: !0
});
return e;
}(o.$je.$jh);
n.$gn = I;
cc._RF.pop();
}, {
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WY": void 0,
"./$KC": "$KC",
"./component/$MM": "$MM",
"./component/$MN": "$MN",
"./component/$MO": "$MO",
"./component/$MP": "$MP",
"./component/$MQ": "$MQ",
"./component/$MS": "$MS",
"./component/$MT": "$MT",
"./component/$MU": "$MU",
"./component/$MV": "$MV",
"./component/$MW": "$MW",
"./component/$MX": "$MX",
"./component/$MY": "$MY",
"./component/$MZ": "$MZ",
"./component/$NA": "$NA",
"./component/$NB": "$NB",
"./component/$ND": "$ND",
"./component/$NE": "$NE",
"./component/$NF": "$NF",
"./component/$NG": "$NG",
"./component/$NH": "$NH",
"./component/$NL": "$NL",
"./component/$NM": "$NM",
"./lib/$NN": "$NN",
"./math/$NP": "$NP",
"./system/$NW": "$NW"
} ],
$MM: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "d036956BoxBvKqTYvc1Z3KR", "$MM");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../utils/$OH"), u = t("../math/$NP"), l = t("../../../game/src/battle/$QM"), h = t("../../../game/src/battle/types/$WY"), f = t("../../../game/src/battle/types/$WS"), c = t("../../../bingo/src/utils/$LZ"), s = t("../$KC"), p = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bfZ = null;
e.bga = new a.$kb();
e.bgb = new f.$up();
e.bgc = null;
e.bgd = [];
e.comboTime = 0;
e.comboLimitTime = 3;
e._currentHp = 0;
e.deathCount = 0;
e.bge = 0;
e.mpInteravl = 0;
e.bgf = !1;
e.bgg = 0;
e.bgh = {};
e.bgi = !1;
e.bgj = null;
e.bgk = [];
e.bgl = {};
return e;
}
e.prototype.bgm = function() {
this.bfY = 0;
this.bfZ = null;
this.bga.bpH();
this.bgb.daf();
this.bgc = null;
for (var t = 0, e = this.bgd; t < e.length; t++) {
var n = e[t];
s.default.EC(n);
}
this.bgd.length = 0;
this.bge = 0;
this.mpInteravl = 0;
this.bgf = !1;
this.deathCount = 0;
this.bgg = 0;
this.bgo();
this.finalDamage = 0;
this.finalHurtValue = 0;
this.lockMinHP = 0;
this.hpRateRecord = -1;
this.bgj = null;
this.bgk = [];
this.bgl = {};
return this;
};
e.prototype.bgn = function() {
if (!this.bgi) {
this.bgi = !0;
c.$id.bbu(this, "totalDamageRaw", 9999);
c.$id.bbu(this, "realHp", 1e4);
c.$id.bbu(this.bgh, "__realHp__", 1e4);
c.$id.bbu(this.bgh, "__lastHurt__", 0);
}
};
e.prototype.bgo = function() {
this._currentHp = 0;
this.bgh[h.$uD.$vt.b] = 0;
this.bgh[h.$uD.$vt.c] = 0;
this.bgh[h.$uD.$vt.d] = 0;
this.bgh[h.$uD.$vt.e] = 0;
this.bgh[h.$uD.$vt.f] = 0;
this.bgh[h.$uD.$vt.g] = 500;
this.bgh[h.$uD.$vt.h] = 1;
this.bgh[h.$uD.$vt.i] = 0;
this.bgh[h.$uD.$vt.j] = 0;
this.bgh[h.$uD.$vt.k] = 0;
this.bgh[h.$uD.$vt.l] = 0;
this.bgh[h.$uD.$vt.m] = 0;
this.bgh[h.$uD.$vt.n] = -1;
this.bgh[h.$uD.$vt.o] = 0;
this.bgh[h.$uD.$vt.p] = 0;
this.bgh[h.$uD.$vt.q] = 2250;
this.bgh[h.$uD.$vt.r] = 1e3;
this.bgh[h.$uD.$vt.s] = 0;
this.bgh[h.$uD.$vt.t] = 0;
this.bgh[h.$uD.$vt.u] = 0;
this.bgh[h.$uD.$vt.v] = 0;
this.bgh[h.$uD.$vt.w] = 0;
this.bgh[h.$uD.$vt.x] = 0;
this.bgh[h.$uD.$vt.y] = 0;
this.bgh[h.$uD.$vt.z] = 0;
this.bgh[h.$uD.$vt.A] = 0;
this.bgh[h.$uD.$vt.B] = 0;
this.bgh[h.$uD.$vt.C] = 0;
c.ANTI_CHEAT_ENABLED && c.pooledObject(this.bgh);
};
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
e.prototype.bgp = function(t) {
return this.bgd.find(function(e) {
return e.Dv.id === t;
});
};
e.prototype.bgq = function(t) {
return this.bgd.find(function(e) {
return e.Dv.commonType === t;
});
};
e.prototype.bgr = function(t) {
return this.bgd.find(function(e) {
return e.Dv.cmdId === t;
});
};
Object.defineProperty(e.prototype, "bgs", {
get: function() {
return this.bgh[h.$uD.$vt.n];
},
set: function(t) {
this.bgh[h.$uD.$vt.n] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgt", {
get: function() {
return this.bgh[h.$uD.$vt.m];
},
set: function(t) {
this.bgh[h.$uD.$vt.m] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgu", {
get: function() {
return 0;
},
set: function() {},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgv", {
get: function() {
return this.bgh[h.$uD.$vt.c];
},
set: function(t) {
this.bgh[h.$uD.$vt.c] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgw", {
get: function() {
return this.bgh[h.$uD.$vt.j];
},
set: function(t) {
this.bgh[h.$uD.$vt.j] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgx", {
get: function() {
return this.bgh[h.$uD.$vt.k];
},
set: function(t) {
this.bgh[h.$uD.$vt.k] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgy", {
get: function() {
return this.bgh[h.$uD.$vt.f];
},
set: function(t) {
this.bgh[h.$uD.$vt.f] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgz", {
get: function() {
return this.bgh[h.$uD.$vt.d];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgA", {
get: function() {
return this.bgh[h.$uD.$vt.e];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgB", {
get: function() {
return this.bgh[h.$uD.$vt.i];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgC", {
get: function() {
return this.bgh[h.$uD.$vt.g];
},
set: function(t) {
this.bgh[h.$uD.$vt.g] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgD", {
get: function() {
return this._currentHp;
},
set: function(t) {
this._currentHp = Math.min(t, this.bgE);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgE", {
get: function() {
return this.bgR(l.GameAttriTableEnum.hpMax);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgF", {
get: function() {
return this.bgh[h.$uD.$vt.b];
},
set: function(t) {
t < 0 && (t = 0);
this.bgh[h.$uD.$vt.b] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgG", {
get: function() {
return this.bgR(l.GameAttriTableEnum.mpMax);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgH", {
get: function() {
return this.bgh[h.$uD.$vt.h];
},
set: function(t) {
this.bgh[h.$uD.$vt.h] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgI", {
get: function() {
return this.bgD > 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgJ", {
get: function() {
return this.bgh[h.$uD.$vt.l];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgK", {
get: function() {
return this.bgh[h.$uD.$vt.y];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgL", {
get: function() {
return this.bgh[h.$uD.$vt.z];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgM", {
get: function() {
return this.bgh[h.$uD.$vt.A];
},
set: function(t) {
this.bgh[h.$uD.$vt.A] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgN", {
get: function() {
return this.bgh[h.$uD.$vt.B];
},
enumerable: !1,
configurable: !0
});
e.prototype.bgO = function() {
return u.$jG.boj(this.bgD / this.bgE);
};
e.prototype.bgP = function() {
return Math.floor(u.$jG.boj(this.bgD / this.bgE * l.GameAttributeConstRatioRange));
};
e.prototype.bgQ = function() {
return u.$jG.boj(this.bgF / this.bgG);
};
e.prototype.bgR = function(t) {
return this.bgb.dac.cZJ(t);
};
e.prototype.bgS = function(t, e) {
switch (t) {
case l.GameAttriTableEnum.vitPercent:
case l.GameAttriTableEnum.vit:
case l.GameAttriTableEnum.hpMax:
case l.GameAttriTableEnum.hpMaxPercent:
var n = this.bgE;
this.bgb.dac.cZO(t, e);
this.bgD = Math.ceil(u.$jG.boj(u.$jG.boj(this.bgD / n) * this.bgE));
break;

case l.GameAttriTableEnum.spiritPercent:
case l.GameAttriTableEnum.spirit:
case l.GameAttriTableEnum.mpMax:
case l.GameAttriTableEnum.mpMaxPercent:
n = this.bgG;
this.bgb.dac.cZO(t, e);
this.bgF = 0 !== n ? Math.ceil(u.$jG.boj(u.$jG.boj(this.bgF / n) * this.bgG)) : 0;
break;

default:
this.bgb.dac.cZO(t, e);
}
};
e.prototype.bgT = function(t, e) {
switch (t) {
case l.GameAttriTableEnum.vitPercent:
case l.GameAttriTableEnum.vit:
case l.GameAttriTableEnum.hpMax:
case l.GameAttriTableEnum.hpMaxPercent:
var n = this.bgE;
this.bgb.dac.cZP(t, e);
this.bgD = Math.ceil(u.$jG.boj(u.$jG.boj(this.bgD / n) * this.bgE));
break;

case l.GameAttriTableEnum.spiritPercent:
case l.GameAttriTableEnum.spirit:
case l.GameAttriTableEnum.mpMax:
case l.GameAttriTableEnum.mpMaxPercent:
n = this.bgG;
this.bgb.dac.cZP(t, e);
this.bgF = 0 !== n ? Math.ceil(u.$jG.boj(u.$jG.boj(this.bgF / n) * this.bgG)) : 0;
break;

default:
this.bgb.dac.cZP(t, e);
}
};
e.prototype.bgU = function(t, e) {
if (void 0 !== this.bgh[t]) {
this.bgh[t] += e;
this.bgW(t);
}
};
e.prototype.bgV = function(t, e) {
if (void 0 !== this.bgh[t]) {
this.bgh[t] -= e;
this.bgW(t);
}
};
e.prototype.bgW = function(t) {
switch (t) {
case h.$uD.$vt.C:
this.bgb.dac.cYW = this.bgh[t] > 0;
}
};
Object.defineProperty(e.prototype, "bgX", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgY", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bgZ", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
e.prototype.bha = function() {
return 0;
};
Object.defineProperty(e.prototype, "bhb", {
get: function() {
return this.bga.bpD(h.$uD.$vq.c | h.$uD.$vq.g);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhc", {
get: function() {
return this.bga.bpD(h.$uD.$vq.c);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhd", {
get: function() {
return this.bga.bpD(h.$uD.$vq.b);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhe", {
get: function() {
return !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhf", {
get: function() {
return 1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhg", {
get: function() {
return this.bgR(l.GameAttriTableEnum.atkSpeed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhh", {
get: function() {
return this.bgR(l.GameAttriTableEnum.castSpeed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhi", {
get: function() {
return this.bgR(l.GameAttriTableEnum.speed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhj", {
get: function() {
return this.bgh[h.$uD.$vt.q];
},
set: function(t) {
this.bgh[h.$uD.$vt.q] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhk", {
get: function() {
return this.bgh[h.$uD.$vt.r];
},
set: function(t) {
this.bgh[h.$uD.$vt.r] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhl", {
get: function() {
return this.bgR(l.GameAttriTableEnum.ap);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhm", {
get: function() {
return this.bgR(l.GameAttriTableEnum.sp);
},
enumerable: !1,
configurable: !0
});
e.prototype.bhn = function(t) {
return this.bgR(t ? l.GameAttriTableEnum.ap : l.GameAttriTableEnum.sp);
};
e.prototype.bho = function(t) {
return this.bgR(t ? l.GameAttriTableEnum.apDef : l.GameAttriTableEnum.spDef);
};
e.prototype.bhp = function(t) {
return this.bgR(t ? l.GameAttriTableEnum.apDefRate : l.GameAttriTableEnum.spDefRate);
};
e.prototype.bhq = function(t) {
return this.bgR(t ? l.GameAttriTableEnum.apCrit : l.GameAttriTableEnum.spCrit);
};
e.prototype.bhr = function(t) {
return this.bgR(t ? l.GameAttriTableEnum.apCritRate : l.GameAttriTableEnum.spCritRate);
};
Object.defineProperty(e.prototype, "bhs", {
get: function() {
return this.bgR(l.GameAttriTableEnum.critDamageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bht", {
get: function() {
return this.bgR(l.GameAttriTableEnum.finalDamageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhu", {
get: function() {
return this.bgR(l.GameAttriTableEnum.finalDamageRed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhv", {
get: function() {
return this.bgR(l.GameAttriTableEnum.damageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhw", {
get: function() {
return this.bgR(l.GameAttriTableEnum.allAtkRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhx", {
get: function() {
return this.bgR(l.GameAttriTableEnum.streFire);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhy", {
get: function() {
return this.bgR(l.GameAttriTableEnum.streIce);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhz", {
get: function() {
return this.bgR(l.GameAttriTableEnum.streLight);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhA", {
get: function() {
return this.bgR(l.GameAttriTableEnum.streDark);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhB", {
get: function() {
return this.bgR(l.GameAttriTableEnum.resisFire);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhC", {
get: function() {
return this.bgR(l.GameAttriTableEnum.resisIce);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhD", {
get: function() {
return this.bgR(l.GameAttriTableEnum.resisLight);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhE", {
get: function() {
return this.bgR(l.GameAttriTableEnum.resisDark);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhF", {
get: function() {
return this.bgR(l.GameAttriTableEnum.spellAtk);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhG", {
get: function() {
return this.bgR(l.GameAttriTableEnum.mpCostRed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bhH", {
get: function() {
return this.bgR(l.GameAttriTableEnum.maxChargeTimeRed);
},
enumerable: !1,
configurable: !0
});
e.prototype.bhI = function(t) {
switch (t) {
case 1:
return Math.max(0, (l.GameAttributeConstRatioRange + this.bhg) / l.GameAttributeConstRatioRange);

case 2:
return Math.max(0, (l.GameAttributeConstRatioRange + this.bhi) / l.GameAttributeConstRatioRange);

case 3:
return Math.max(0, (l.GameAttributeConstRatioRange + this.bhh) / l.GameAttributeConstRatioRange);
}
};
e.prototype.bhJ = function(t) {
this.bgh[h.$uD.$vt.s] > 0 && (t |= h.$uD.$uY.b);
this.bgh[h.$uD.$vt.t] > 0 && (t |= h.$uD.$uY.c);
this.bgh[h.$uD.$vt.u] > 0 && (t |= h.$uD.$uY.d);
this.bgh[h.$uD.$vt.v] > 0 && (t |= h.$uD.$uY.e);
return t;
};
e.prototype.bhK = function(t) {
var e = t.Dx();
if (0 === e) return 0;
var n = this.bgb.dac.cZT(t.Dm), i = (l.GameAttributeConstRatioRange - this.bgR(l.GameAttriTableEnum.spellCd)) / l.GameAttributeConstRatioRange;
return u.$jG.boj(e * Math.max(l.default.bGR.bGP.maxSpellCd, n * i) * t.cdTest);
};
e.prototype.bhL = function(t) {
var e = t.Dp;
return 0 === this.bhG ? e : Math.max(0, Math.floor(u.$jG.boj(e * (l.GameAttributeConstRatioRange - this.bhG) / l.GameAttributeConstRatioRange)));
};
e.prototype.bhM = function() {
return this.bga.bpD(h.$uD.$vq.c | h.$uD.$vq.b | h.$uD.$vq.g);
};
e.prototype.bhN = function() {
this.deathCount++;
this.bgf = !0;
};
e.prototype.bhO = function() {
this.bgf = !1;
};
e.prototype.bhP = function() {
return this.bgb.dac.cZI(l.GameAttriTableEnum.hpMax);
};
e.prototype.bhQ = function() {
var t = this, e = h.$uD.STRE_ATTR_LIST[0], n = 0;
[ l.GameAttriTableEnum.streFire, l.GameAttriTableEnum.streIce, l.GameAttriTableEnum.streLight, l.GameAttriTableEnum.streDark ].forEach(function(i, r) {
var o = t.bgR(i);
if (o > n) {
n = o;
e = h.$uD.STRE_ATTR_LIST[r];
}
});
return e;
};
e.prototype.bhR = function(t) {
return (t ? this.bfZ.pvpBehaviorId : this.bfZ.behaviorId) || this.bfZ.behaviorId;
};
o([ c.pooledInt ], e.prototype, "lockMinHP", void 0);
o([ c.pooledFloat ], e.prototype, "finalDamage", void 0);
o([ c.pooledFloat ], e.prototype, "finalHurtValue", void 0);
o([ c.pooledInt ], e.prototype, "hpRateRecord", void 0);
o([ c.pooledFloat ], e.prototype, "comboTime", void 0);
o([ c.pooledFloat ], e.prototype, "comboLimitTime", void 0);
o([ c.pooledFloat ], e.prototype, "_currentHp", void 0);
o([ c.pooledInt ], e.prototype, "deathCount", void 0);
o([ c.pooledFloat ], e.prototype, "mpInteravl", void 0);
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = p;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../bingo/src/utils/$LZ": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WS": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../utils/$OH": "$OH"
} ],
$MN: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "e963exPUIFIF7S/s1ehlRH7", "$MN");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../math/$NQ"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bhS = 0;
e.bhT = 0;
e.bhU = -1;
e.bhV = new a.default();
e.bhW = {};
e.bhX = 0;
e.bhY = 0;
e.bhZ = null;
e.bia = new a.default();
e.bib = 0;
e.bic = 0;
e.bid = 0;
e.bie = 0;
e.bif = !0;
e.big = [];
e.bih = null;
return e;
}
e.prototype.bii = function() {};
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
e.prototype.bij = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = u;
cc._RF.pop();
}, {
"../lib/$NN": "$NN",
"../math/$NQ": "$NQ"
} ],
$MO: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "7abaazcmvVAy4QqXXXXGsm1", "$MO");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bik = 0;
e.bil = 0;
e.bin = null;
e.bio = 0;
e.bip = [];
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MP: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "16187U6ahtC04iFrAsNQlaN", "$MP");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../math/$NQ"), u = t("../../../game/src/battle/types/$WY"), l = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bir = !1;
e.bis = -1;
e.bit = new a.default();
e.biu = new a.default();
e.biv = 9999;
e.biw = u.$uD.$vh.b;
e.bix = 0;
return e;
}
e.prototype.lS = function(t) {
this.bir = t[0];
this.bis = t[1];
this.bit.bow(t[2], t[3]);
this.biu.bow(t[4], t[5]);
this.biv = t[6];
this.biw = t[7];
this.bix = t[8];
};
e.prototype.lT = function() {
return [ this.bir, this.bis, this.bit.x, this.bit.y, this.biu.x, this.biu.y, this.biv, this.biw, this.bix ];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = l;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WY": void 0,
"../lib/$NN": "$NN",
"../math/$NQ": "$NQ"
} ],
$MQ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5fbdd9GBbNNE65aL6am9Lrp", "$MQ");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biy = 0;
e.biz = null;
e.biA = null;
e.biB = 0;
e.biC = -1;
e.biD = -1;
e.biE = {};
e.biF = 0;
e.biG = 0;
e.biH = 0;
e.biI = 0;
e.biJ = 0;
e.biK = !0;
e.biL = -1;
e.biM = 0;
e.biN = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MR: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5e557rOzh1PM6fDOzwf1Ibb", "$MR");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biO = -1;
e.biQ = 0;
e.biR = 0;
e.biS = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MS: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "e27f6GSs0RGLIfxqiF2IHQA", "$MS");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biT = -1;
return e;
}
e.prototype.lS = function(t) {
this.biT = t;
};
e.prototype.lT = function() {
return this.biT;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MT: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "68ae6DmQFNLLqwLcTfcCCVR", "$MT");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biU = -1;
e.biW = [];
return e;
}
e.prototype.lS = function(t) {
this.biU = t;
};
e.prototype.lT = function() {
return this.biU;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MU: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "d3b3ftOexxLWaMAabW1CPm1", "$MU");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biX = 0;
e.biY = -1;
e.biZ = 0;
e.bja = -1;
e.bjb = 0;
e.bjc = -1;
e.bjd = 0;
e.bje = !1;
e.bjf = -1;
e.bjg = -1;
e.bjh = -1;
e.bji = .4;
return e;
}
e.prototype.bjj = function(t, e) {
void 0 === e && (e = !1);
this.bjm();
this.bjl();
this.bjk();
this.bje = !1;
if (1 === t) {
this.bjf = e ? .25 : -1;
this.bjg = .2;
this.bjh = .15;
this.bji = .5;
} else {
this.bjf = -1;
this.bjg = .2;
this.bjh = .2;
this.bji = .5;
}
};
e.prototype.bjk = function() {
this.bjc = -1;
this.bjd = 0;
};
e.prototype.bjl = function() {
this.bja = -1;
this.bjb = 0;
};
e.prototype.bjm = function() {
this.biY = -1;
this.biZ = 0;
};
e.prototype.lS = function() {};
e.prototype.lT = function() {
return !1;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MV: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "927795WxDNCUbcb50tCA3aK", "$MV");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bjn = {
time: 0,
duration: 0,
moments: []
};
e.bjo = 0;
e.bjp = 0;
e.bjq = [];
e.bjr = {};
e.bjs = 0;
e.bjt = 0;
e.bju = {};
e.bjv = {};
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
e.prototype.bjw = function(t, e) {
this.bjn.time = 0;
this.bjn.duration = t;
this.bjn.moments = null != e ? e : [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MW: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "259abY7pwtGDpTzhVsI9dHz", "$MW");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../../../game/src/battle/types/$WY"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bjx = {};
e.bjy = [];
e.bjz = [];
e.bjA = [];
e.bjB = [];
e.bjC = {};
e.bjD = {};
e.bjE = 0;
e.bjF = null;
e.bjG = 0;
e.bjH = null;
e.bjI = [ 0, 0 ];
e.bjJ = {};
e.bjK = -1;
e.bjL = 0;
e.bjM = {};
e.bjN = {};
e.bjO = -1;
e.bjP = -1;
e.bjQ = [];
e.bjR = [];
e.bjS = -1;
e.bjT = -1;
e.bjU = null;
e.bjV = [];
e.bjW = [];
e.bjX = {
hurtbackTime: 0,
slideTime: 0,
hurtbackType: a.$uD.$vc.b,
actId: 0,
hurtAnim: ""
};
e.bjY = {
type: 0,
time: 0,
currentTime: 0,
speed: 1
};
e.bjZ = -1;
e.bka = !1;
e.bkb = null;
e.bkc = 0;
e.bkd = 0;
e.bke = 0;
e.bkf = 1;
e.bkg = 0;
e.bkh = null;
e.bki = -1;
e.bkj = 0;
e.bkk = null;
e.bkl = [];
e.bkm = {
b: 0,
c: []
};
return e;
}
Object.defineProperty(e.prototype, "bkn", {
get: function() {
var t;
return null !== (t = this.bjF.dirType) && void 0 !== t ? t : a.$uD.$vd.b;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bko", {
get: function() {
var t;
return null !== (t = this.bjF.isBuilding) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkp", {
get: function() {
var t;
return null !== (t = this.bjF.ignoreAttraction) && void 0 !== t && t || this.bkq;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkq", {
get: function() {
var t;
return null !== (t = this.bjF.ignorePhysicalEffects) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkr", {
get: function() {
return this.bkq || this.bjF.igonreGrab;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bks", {
get: function() {
var t;
return null === (t = this.bjF.movable) || void 0 === t || t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkt", {
get: function() {
var t;
return 2 === (null === (t = this.bjx) || void 0 === t ? void 0 : t.superArmor);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bku", {
get: function() {
return !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkv", {
get: function() {
var t;
return this.bkq || null !== (t = this.bjF.ignoreHitFly) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkw", {
get: function() {
return this.bka;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkx", {
get: function() {
var t;
return this.bkq || null !== (t = this.bjF.ignoreHitBack) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
e.prototype.bky = function(t) {
var e, n, i = this.bjB.length;
t.DD = i;
this.bjB.push(t);
this.bjC[t.DF.id] = (null !== (e = this.bjC[t.DF.id]) && void 0 !== e ? e : 0) + 1;
var r = t.DF.event;
r && (this.bjD[r] = (null !== (n = this.bjD[r]) && void 0 !== n ? n : 0) + 1);
};
e.prototype.bkz = function(t) {
var e = this.bjB[t];
if (!e) return !1;
var n = e.DF;
this.bjC[n.id] && this.bjC[n.id]--;
var i = n.event;
i && this.bjD[i] && this.bjD[i]--;
e.DD = -1;
e.DP = 0;
var r = this.bjB.pop();
if (r && r !== e) {
r.DD = t;
this.bjB[t] = r;
}
return !0;
};
e.prototype.bkA = function() {
this.bjX.hurtbackTime = 0;
this.bjX.slideTime = 0;
this.bjX.hurtbackType = a.$uD.$vc.b;
this.bjX.actId = 0;
this.bjX.hurtAnim = "";
this.bjY.currentTime = 0;
this.bjY.time = 0;
this.bjY.speed = 1;
this.bjY.type = 0;
};
Object.defineProperty(e.prototype, "bkB", {
get: function() {
var t, e;
return null !== (e = null === (t = this.bjF) || void 0 === t ? void 0 : t.idleBehaviorId) && void 0 !== e ? e : "1";
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkC", {
get: function() {
return -1 !== this.bjS;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkD", {
get: function() {
return !!this.bjx && (this.bjx.idle || this.bjx.move);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bkE", {
get: function() {
var t, e;
return null !== (e = null === (t = this.bjF) || void 0 === t ? void 0 : t.noAutoAim) && void 0 !== e && e;
},
enumerable: !1,
configurable: !0
});
e.prototype.bkF = function() {
return this.bkl.find(function(t) {
return 1 === t.b.type;
});
};
e.prototype.lS = function() {};
e.prototype.lT = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = u;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WY": void 0,
"../lib/$NN": "$NN"
} ],
$MX: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5c8caG3sgdAdoUMWcOGcDr3", "$MX");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bkG = -1;
e.bkH = 0;
e.bkI = 0;
e.bkJ = 0;
e.bkK = 0;
e.bkL = 0;
e.bkM = null;
e.bkN = 0;
e.bkO = 0;
e.bkP = 0;
e.bkQ = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MY: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "a1338migBhJSpOfcGx94dNL", "$MY");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bkR = -1;
e.bkS = 0;
e.bkT = 0;
return e;
}
e.prototype.lS = function(t) {
this.bkR = t[0];
this.bkS = t[1];
};
e.prototype.lT = function() {
return [ this.bkR, this.bkS ];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$MZ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5a54bzVnFVM4I4QFuk9l6BK", "$MZ");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$iP = n.IsCollider1InsideCollider2 = n.rectCircleIntersect = n.rectCircle = n.circleCircle = n.IsRectInsideCircle = n.IsPointInsideCircle = n.IsCircle1InsideCircle2 = void 0;
var b = t("../../../game/src/battle/types/$WY"), a = t("../lib/$NN"), u = t("../math/$NO"), l = t("../math/$NQ"), h = new l.default(), f = new l.default();
function c(t, e, n, i, r, o) {
var b = i - t, a = r - e, u = o - n;
return b * b + a * a <= u * u;
}
n.IsCircle1InsideCircle2 = c;
function s(t, e, n, i, r) {
var o = n - t, b = i - e;
return o * o + b * b <= r * r;
}
n.IsPointInsideCircle = s;
function p(t, e) {
for (var n = 0, i = t.bnU; n < i.length; n++) {
var r = i[n];
if (!s(r.x, r.y, e.bnT.x, e.bnT.y, e.bnS)) return !1;
}
return !0;
}
n.IsRectInsideCircle = p;
n.circleCircle = function(t, e) {
var n = t.bnT.boz(e.bnT, h).boC(), i = t.bnS + e.bnS;
return n < i * i;
};
n.rectCircle = function(t, e) {
var n = t.bnQ;
f.x = n.width / 2;
f.y = n.height / 2;
h.x = n.x + f.x;
h.y = n.y + f.y;
return d(h, f, e.bnT, e.bnS);
};
function d(t, e, n, i) {
var r = Math.abs(n.x - t.x), o = Math.abs(n.y - t.y), b = Math.max(r - e.x, 0), a = Math.max(o - e.y, 0);
return b * b + a * a <= i * i;
}
n.rectCircleIntersect = d;
n.IsCollider1InsideCollider2 = function(t, e) {
for (var n = e.bkX.bkU, i = t.bkX.bkU, r = !0, o = 0; o < n.length; o++) {
var a = n[o], u = a.shapeInfo, l = a.world;
if (1 === u.neg) {
for (var h = 0; h < i.length; h++) {
var f = i[h], s = f.shapeInfo, d = f.world;
if (1 !== s.neg) {
var v = s.t === b.$uD.$vm.c, $ = u.t === b.$uD.$vm.b, g = s.t === b.$uD.$vm.b;
if (!$) {
r = !1;
break;
}
if (!(r = g ? c(d.bnT.x, d.bnT.y, d.bnS, l.bnT.x, l.bnT.y, l.bnS) : !!v && p(d, l))) break;
}
}
if (!r) break;
}
}
return r;
};
var v = function() {
function t() {
this.bkU = [];
this.bkV = !1;
}
t.prototype.bkW = function(t) {
var e, n = this.bkU;
n.length = t.length;
for (var i = 0; i < n.length; i++) {
var r = n[i];
r || (r = n[i] = {
world: new u.$jE(),
shapeInfo: null
});
r.shapeInfo = t[i];
this.bkV = !!(null === (e = t[i]) || void 0 === e ? void 0 : e.neg);
}
};
return t;
}();
n.$iP = v;
var $ = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bkX = new v();
e.bkY = new v();
e.bkZ = [];
e.bla = 0;
e.blb = 0;
e.blc = 0;
e.bld = 1;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ a.$je.register() ], e);
}(a.$je.$jf);
n.default = $;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WY": void 0,
"../lib/$NN": "$NN",
"../math/$NO": "$NO",
"../math/$NQ": "$NQ"
} ],
$NA: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "5ee18U8W0NE0oAzQl3DQQgk", "$NA");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.ble = !1;
return e;
}
e.prototype.lS = function(t) {
this.ble = t;
};
e.prototype.lT = function() {
return this.ble;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NB: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "3f5a7lBqspDLKGHMWg8gCjU", "$NB");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$iS = void 0;
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return !1;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.$iS = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NC: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "94aaaHhEclJfIqv2QvAXhaE", "$NC");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.blf = -1;
e.blg = 100;
e.blh = 0;
return e;
}
e.prototype.lS = function(t) {
this.blf = t[0];
this.blg = t[1];
this.blh = t[2];
};
e.prototype.lT = function() {
return [ this.blf, this.blg, this.blh ];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$ND: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "60495CixbpCIoG7srO/cZT8", "$ND");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bli = 0;
e.blj = 0;
e.blk = 0;
e.bll = 0;
e.blm = 0;
e.bln = 0;
e.blo = 0;
e.blp = 0;
e.blq = 0;
e.blr = !0;
e.bls = !1;
e.blt = null;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NE: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "31793I2W2xMtYqePoUvhOXm", "$NE");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return !1;
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NF: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "23b94eAsGJF3p/HDXMQVLS5", "$NF");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../math/$NQ"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.blu = new a.default();
e.blv = 0;
e.blw = 0;
e.blx = 0;
e.bly = !0;
e.blz = [];
e.blA = null;
e.blB = 0;
e.blC = [ 0, 0, 0, 0 ];
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
e.prototype.blD = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = u;
cc._RF.pop();
}, {
"../lib/$NN": "$NN",
"../math/$NQ": "$NQ"
} ],
$NG: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "0e4a3SDKYtBn4BeG6PeIrnX", "$NG");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../../../bingo/src/$JX"), a = t("../../../game/src/battle/$QA"), u = t("../../../game/src/battle/$QM"), l = t("../../../game/src/battle/types/$WU"), h = t("../$KB"), f = t("../lib/$NN"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.blE = 0;
e.blF = 0;
e.blG = 0;
e.blH = 0;
e.blI = 0;
e.blJ = 0;
e.blK = 0;
e.blO = [];
e.blP = {};
e.blQ = {};
return e;
}
e.prototype.blR = function(t, e, n, i, r) {
this.blE = t;
this.blF = 0;
this.blG = 0;
this.blI = 0;
this.blH = 0;
this.blJ = 0;
this.blK = 0;
this.blL = n.slice();
this.blM = null == i ? void 0 : i.slice();
this.blN = r;
this.blO = [];
this.blP = {};
this.blQ = {};
var o = u.default.bGR.bHZ(0, n, i, r);
this.blH = o.titleId;
this.blK = o.petId;
};
e.prototype.blS = function(t, e) {
var n = this.blN;
if (null == n ? void 0 : n.consumeItems) for (var i = 0, r = n.consumeItems; i < r.length; i++) {
var o = r[i], f = new h.$gb.$gf();
f.Es(o.itemId, o);
this.blO.push(f);
}
var c = null;
try {
if (n.skillButton) {
var s = JSON.parse(n.skillButton), p = e ? b.$fb.tblEnum.SkillTypeTableEnum.PVE_SKILL : b.$fb.tblEnum.SkillTypeTableEnum.PVP_SKILL, d = s.find(function(t) {
return t.type === p;
});
if (null == d ? void 0 : d.button) {
c = JSON.parse(d.button);
for (var v in c) {
var $ = +v;
if (!($ < 0)) {
var g = c[v][0], y = $, m = a.BATTLE_BTN_2_CMD[y];
this.blP[m] = g;
}
}
var j = null == c ? void 0 : c[-200];
if (j) {
this.blP[l.$uv.v] = j[0][0];
this.blP[l.$uv.v + 1] = j[0][1];
this.blP[l.$uv.v + 2] = j[0][2];
this.blP[l.$uv.v + 3] = j[0][3];
}
}
}
} catch (t) {}
if (!c) {
var D = u.default.bGR.bHs(t), G = l.FRAME_SYNC_CMD2BIT[l.$uv.g];
if (D) for (var x = 0, E = D; x < E.length; x++) {
g = E[x];
var M = u.default.bGR.bHq(g);
if (1 !== M.commonType) {
if (!(M.commonType > 0) && M.buttonEnable) {
if (!(m = l.FRAME_SYNC_BIT2CMD[G])) break;
this.blP[m] = M.id;
G++;
}
} else this.blP[l.$uv.d] = M.id;
}
}
for (var N in this.blP) this.blQ[this.blP[N]] = Number(N);
};
e.prototype.lS = function() {};
e.prototype.lT = function() {};
return o([ f.$je.register() ], e);
}(f.$je.$jf);
n.default = c;
cc._RF.pop();
}, {
"../$KB": "$KB",
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WU": void 0,
"../lib/$NN": "$NN"
} ],
$NH: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "eeef4yHV/dBrZP2I0FJZLtR", "$NH");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../math/$NQ"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.blT = "";
e.blU = 0;
e.blV = 0;
e.blW = -1;
e.blX = new a.default();
e.blY = 0;
e.blZ = 0;
e.bma = new Map();
e.bmb = new Map();
e.bmc = [];
e.bmd = 0;
e.bme = !1;
e.bmf = 1;
return e;
}
Object.defineProperty(e.prototype, "bmg", {
get: function() {
return this.bmf && !this.bme;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bmh", {
get: function() {
return !!this.blT;
},
enumerable: !1,
configurable: !0
});
e.prototype.lS = function() {};
e.prototype.lT = function() {};
e.prototype.bmi = function(t) {
if (t) {
var e = this.bma.get(t);
if (!e) {
e = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
this.bma.set(t, e);
}
return e;
}
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = u;
cc._RF.pop();
}, {
"../lib/$NN": "$NN",
"../math/$NQ": "$NQ"
} ],
$NI: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "b7d87ainmpCBZ4AGBrYafN/", "$NI");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmj = 0;
e.bmk = !1;
e.bml = !1;
e.bmm = 0;
e.bmn = 0;
e.bmo = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NJ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "1bc15vyseFCsa+8AHjsnY7r", "$NJ");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmp = 0;
e.bmq = 0;
e.bmr = 0;
e.bms = 0;
e.bmt = 0;
e.bmu = 0;
e.bmv = 0;
e.bmw = 0;
e.bmx = 0;
e.bmy = [];
e.bmz = [];
e.bmA = null;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NK: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "7d99fbnepFNyI4Au1MZ3ezX", "$NK");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmB = 0;
e.bmC = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = a;
cc._RF.pop();
}, {
"../lib/$NN": "$NN"
} ],
$NL: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "36d91NNP0dAnreSSNE9Sl2k", "$NL");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../lib/$NN"), a = t("../../../bingo/src/utils/$LZ"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.worldTimeScale = 1;
e.hitPauseTimeScale = 1;
e.mainTimeScale = 1;
e.actionTimeScale = 1;
e.hTimeScale = 1;
e.vTimeScale = 1;
e.bmD = 0;
return e;
}
Object.defineProperty(e.prototype, "bmE", {
get: function() {
return this.worldTimeScale * this.hitPauseTimeScale * this.mainTimeScale;
},
enumerable: !1,
configurable: !0
});
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
o([ a.pooledFloatDefault(1) ], e.prototype, "worldTimeScale", void 0);
o([ a.pooledFloatDefault(1) ], e.prototype, "hitPauseTimeScale", void 0);
o([ a.pooledFloatDefault(1) ], e.prototype, "mainTimeScale", void 0);
o([ a.pooledFloatDefault(1) ], e.prototype, "actionTimeScale", void 0);
o([ a.pooledFloatDefault(1) ], e.prototype, "hTimeScale", void 0);
o([ a.pooledFloatDefault(1) ], e.prototype, "vTimeScale", void 0);
return o([ b.$je.register() ], e);
}(b.$je.$jf);
n.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/utils/$LZ": void 0,
"../lib/$NN": "$NN"
} ],
$NM: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "f29f99BlulOoL4qoE8JGsMm", "$NM");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__decorate || function(t, e, n, i) {
var r, o = arguments.length, b = o < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) b = Reflect.decorate(t, e, n, i); else for (var a = t.length - 1; a >= 0; a--) (r = t[a]) && (b = (o < 3 ? r(b) : o > 3 ? r(e, n, b) : r(e, n)) || b);
return o > 3 && b && Object.defineProperty(e, n, b), b;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var b = t("../../../bingo/src/utils/$LZ"), a = t("../../../game/src/battle/types/$WY"), u = t("../lib/$NN"), l = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.x = 0;
e.y = 0;
e.z = 0;
e.bmF = a.$uD.$vj.b;
e.bmG = 0;
e.bmH = 0;
return e;
}
e.prototype.lS = function() {};
e.prototype.lT = function() {
return [];
};
e.prototype.bmI = function() {};
Object.defineProperty(e.prototype, "bmJ", {
get: function() {
return this.z <= 0;
},
enumerable: !1,
configurable: !0
});
o([ b.pooledFloat ], e.prototype, "x", void 0);
o([ b.pooledFloat ], e.prototype, "y", void 0);
o([ b.pooledFloat ], e.prototype, "z", void 0);
return o([ u.$je.register() ], e);
}(u.$je.$jf);
n.default = l;
cc._RF.pop();
}, {
"../../../bingo/src/utils/$LZ": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../lib/$NN": "$NN"
} ],
$NN: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "a1633L4DH1PCp2RyafJYxQc", "$NN");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
}), o = this && this.__spreadArrays || function() {
for (var t = 0, e = 0, n = arguments.length; e < n; e++) t += arguments[e].length;
var i = Array(t), r = 0;
for (e = 0; e < n; e++) for (var o = arguments[e], b = 0, a = o.length; b < a; b++, 
r++) i[r] = o[b];
return i;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$je = void 0;
(function(t) {
t.$jf = function() {};
var e = 0, n = [];
t.register = function() {
return function(t) {
if (null != t.tid) throw new Error("already contain component " + t.tid);
t.tid = e++;
n.push(t);
};
};
t.getComponentConstructors = function() {
return n;
};
t.allOf = function() {
for (var t, e = [], n = 0; n < arguments.length; n++) e[n] = arguments[n];
return (t = new c()).allOf.apply(t, e);
};
var i = function() {
function t() {
this.bmK = 0;
}
t.prototype.bmL = function(t) {
this.bmK |= 1 << t;
};
t.prototype.bmM = function(t) {
this.bmK &= ~(1 << t);
};
t.prototype.bmN = function(t) {
return 0 != (this.bmK & 1 << t);
};
t.prototype.bmO = function(t) {
return (this.bmK & t.bmK) > 0;
};
t.prototype.bmP = function(t) {
return (this.bmK & t.bmK) === this.bmK;
};
t.prototype.bmQ = function() {
this.bmK = 0;
};
return t;
}(), b = function() {
function t() {
this.hg = -1;
this.hh = new i();
this.hj = new Array(32);
}
t.prototype.hk = function() {};
t.prototype.hl = function() {};
t.prototype.hm = function(t, e) {
this.hi = t;
this.hg = e;
};
t.prototype.hn = function(t) {
var e = t.tid;
this.hh.bmN(e) && this.hq(t);
this.hh.bmL(e);
var n = this.hi.bnr(t);
this.hj[t.tid] = n;
this.hk(e, n);
this.hi.bnt(this, e);
return n;
};
t.prototype.ho = function(t) {
return this.hj[t.tid];
};
t.prototype.hp = function(t) {
return !!this.ho(t);
};
t.prototype.hq = function(t) {
if (this.hh.bmN(t.tid)) {
var e = t.tid, n = this.hj[t.tid];
this.hi.bns(e, n);
this.hj[t.tid] = null;
this.hk(e, null);
this.hh.bmM(e);
this.hi.bnt(this, e);
}
};
t.prototype.hr = function() {
for (var t = null, i = this.hh.bmK, r = 0; r < e; r++) if (0 != (i & 1 << r)) {
t = n[r];
var o = this.hj[t.tid];
this.hi.bns(r, o);
this.hj[t.tid] = null;
this.hk(r, null);
this.hh.bmM(r);
this.hi.bnt(this, r);
}
this.hh.bmQ();
this.hg = -1;
this.hi = null;
};
t.prototype.hs = function() {
for (var t = null, i = this.hh.bmK, r = 0; r < e; r++) if (0 != (i & 1 << r)) {
t = n[r];
var o = this.hj[t.tid];
this.hi.bns(r, o);
this.hj[t.tid] = null;
this.hk(r, null);
}
this.hh.bmQ();
this.hg = -1;
this.hi = null;
};
t.prototype.ht = function(t) {
this.hg = t.eid;
this.hh.bmK = t.maskFlag;
var e = null, i = t.comps;
for (var r in i) {
var o = Number(r);
e = n[o];
var b = this.hi.bnr(e);
b.lS(i[r]);
this.hj[e.tid] = b;
}
this.hl();
};
t.prototype.hu = function() {
for (var t = {}, i = this.hh.bmK, r = null, o = 0; o < e; o++) if (0 != (i & 1 << o)) {
r = n[o];
var b = this.hj[r.tid].lT();
t[o] = b;
}
return {
eid: this.hg,
maskFlag: this.hh.bmK,
comps: t
};
};
return t;
}();
t.$jh = b;
var a = function() {
function t(t, e) {
this.bmT = new Set();
this.bmU = null;
this.bmV = 0;
this.bmS = e;
this.bmR = t;
}
Object.defineProperty(t.prototype, "bmW", {
get: function() {
var t = this;
null === this.bmU && (this.bmU = Array.from(this.bmT.values()).map(function(e) {
return t.bmS.bnu(e);
}));
return this.bmU;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(t.prototype, "bmX", {
get: function() {
return this.bmW[0];
},
enumerable: !1,
configurable: !0
});
t.prototype.bmY = function(t) {
if (this.bmR.isMatch(t)) {
this.bmT.add(t.hg);
this.bmU = null;
this.bmV++;
} else if (this.bmT.has(t.hg)) {
this.bmT.delete(t.hg);
this.bmU = null;
this.bmV--;
}
};
t.prototype.bmZ = function() {
this.bmT.clear();
this.bmU = null;
this.bmV = 0;
};
t.prototype.bna = function(t) {
this.bmV = t.length;
this.bmU = null;
this.bmT.clear();
for (var e = 0, n = t; e < n.length; e++) {
var i = n[e];
this.bmT.add(i);
}
};
t.prototype.bnb = function() {
return Array.from(this.bmT.values());
};
return t;
}();
t.$ji = a;
var u = function() {
function t() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
this.lU = new i();
this.lV = [];
for (var n = -1, r = t.length, o = 0; o < r; o++) {
if (-1 == (n = t[o])) throw Error("存在没有注册的组件！");
this.lU.bmL(n);
this.lV.indexOf(t[o]) < 0 && this.lV.push(t[o]);
}
r > 1 && this.lV.sort(function(t, e) {
return t - e;
});
}
t.prototype.toString = function() {
return this.lV.join("-");
};
return t;
}(), l = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lX = function(t) {
return this.lU.bmO(t.hh);
};
e.prototype.lW = function() {
return "anyOf:" + this.toString();
};
return e;
}(u), h = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lX = function(t) {
return this.lU.bmP(t.hh);
};
e.prototype.lW = function() {
return "allOf:" + this.toString();
};
return e;
}(u), f = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lW = function() {
return "excludeOf:" + this.toString();
};
e.prototype.lX = function(t) {
return !this.lU.bmO(t.hh);
};
return e;
}(u), c = function() {
function t() {
this.rules = [];
this._indices = null;
}
Object.defineProperty(t.prototype, "b", {
get: function() {
var t = this;
if (null === this._indices) {
this._indices = [];
this.rules.forEach(function(e) {
Array.prototype.push.apply(t._indices, e.lV);
});
}
return this._indices;
},
enumerable: !1,
configurable: !0
});
t.prototype.anyOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var n = [], i = 0, r = t.length; i < r; i++) n.push(t[i].tid);
this.rules.push(new (l.bind.apply(l, o([ void 0 ], n)))());
return this;
};
t.prototype.allOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var n = [], i = 0, r = t.length; i < r; i++) n.push(t[i].tid);
this.rules.push(new (h.bind.apply(h, o([ void 0 ], n)))());
return this;
};
t.prototype.onlyOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var i = [], r = 0, b = t.length; r < b; r++) i.push(t[r].tid);
this.rules.push(new (h.bind.apply(h, o([ void 0 ], i)))());
for (var a = n, u = [], l = 0, c = a; l < c.length; l++) {
var s = c[l];
i.indexOf(s.tid) < 0 && u.push(s.tid);
}
this.rules.push(new (f.bind.apply(f, o([ void 0 ], u)))());
return this;
};
t.prototype.excludeOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var n = [], i = 0, r = t.length; i < r; i++) n.push(t[i].tid);
this.rules.push(new (f.bind.apply(f, o([ void 0 ], n)))());
return this;
};
t.prototype.getKey = function() {
for (var t = "", e = 0; e < this.rules.length; e++) {
t += this.rules[e].lW();
e < this.rules.length - 1 && (t += " && ");
}
return t;
};
t.prototype.isMatch = function(t) {
if (1 === this.rules.length) return this.rules[0].lX(t);
if (2 === this.rules.length) return this.rules[0].lX(t) && this.rules[1].lX(t);
for (var e = 0, n = this.rules; e < n.length; e++) if (!n[e].lX(t)) return !1;
return !0;
};
return t;
}(), s = function() {
function t(t) {
this.context = null;
this.dt = 0;
this.fixedUpdate = !1;
this.context = t;
this.group = this.context.bnq(this.filter(), "e");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bmV > 0 || this.fixedUpdate;
};
t.prototype.execute = function(t) {
this.dt = t;
this.update(this.group.bmW);
};
t.prototype.postEvent = function(t, e, n, i, r, o, b) {
this.context.bnl.gu(t, e, n, i, r, o, b);
};
return t;
}();
t.$jo = s;
(function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.bnq(this.filter(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bmV > 0;
};
t.prototype.execute = function(t) {
this.dt = t;
this.update(this.group.bmW);
this.group.bmZ();
};
t.prototype.postEvent = function(t, e, n, i, r, o, b) {
this.context.bnl.gu(t, e, n, i, r, o, b);
};
})(), function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.bnq(this.filter(), "e");
this.rGroup = this.context.bnq(this.filter(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bmV > 0;
};
t.prototype.execute = function(t) {
this.dt = t;
if (this.rGroup.bmV > 0) {
this.entityEnter(this.rGroup.bmW);
this.rGroup.bmZ();
}
this.update(this.group.bmW);
};
t.prototype.postEvent = function(t, e, n, i, r, o, b) {
this.context.bnl.gu(t, e, n, i, r, o, b);
};
}();
var p = function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.bnq(this.filter(), "e");
this.eGroup = this.context.bnq(this.event(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bmV > 0 || this.eGroup.bmV > 0;
};
t.prototype.clearEventEntities = function() {
if (this.eGroup.bmV > 0) for (var t = 0, e = this.eGroup.bmW; t < e.length; t++) {
var n = e[t];
this.context.bnp(n);
}
};
t.prototype.execute = function(t) {
this.dt = t;
this.eGroup.bmV > 0 && this.eventCallback(this.eGroup.bmW, this.group.bmW);
this.group.bmV > 0 && this.update(this.group.bmW);
};
t.prototype.postEvent = function(t, e, n, i, r, o, b) {
this.context.bnl.gu(t, e, n, i, r, o, b);
};
return t;
}(), d = function() {
function t() {
this.executeSystemFlows = [];
this.eventSystems = [];
this.eventSystemsCnt = 0;
this.executeCount = null;
}
t.prototype.initDebug = function() {
this.executeCount = Object.create(null);
this.debugInfo = document.createElement("debugInfo");
this.debugInfo.style.position = "absolute";
this.debugInfo.style.top = "60px";
this.debugInfo.style.left = "10px";
this.debugInfo.style.color = "#ffffff";
document.body.appendChild(this.debugInfo);
for (var t = 0, e = this.executeSystemFlows; t < e.length; t++) {
var n = e[t];
this.executeCount[n.__proto__.constructor.name] = 0;
}
};
t.prototype.add = function(t) {
var e = this;
if (t instanceof v) {
Array.prototype.push.apply(this.executeSystemFlows, t.executeSystems);
t.executeSystems.forEach(function(t) {
if (t instanceof p) {
e.eventSystems.push(t);
e.eventSystemsCnt++;
}
});
t.executeSystems.length = 0;
} else if (t instanceof p) {
this.executeSystemFlows.push(t);
this.eventSystems.push(t);
this.eventSystemsCnt++;
} else this.executeSystemFlows.push(t);
return this;
};
t.prototype.init = function() {
for (var t = 0, e = this.executeSystemFlows; t < e.length; t++) e[t].init();
};
t.prototype.destroy = function() {
for (var t = 0, e = this.executeSystemFlows; t < e.length; t++) e[t].destroy();
};
t.prototype.execute = function(t) {
for (var e = 0, n = this.executeSystemFlows; e < n.length; e++) {
var i = n[e];
i.canRun() && i.execute(t);
}
if (this.eventSystemsCnt > 0) for (var r = 0; r < this.eventSystemsCnt; r++) this.eventSystems[r].clearEventEntities();
};
t.prototype.debugExecute = function() {};
return t;
}();
t.$js = d;
var v = function() {
function t() {
this.executeSystems = [];
}
t.prototype.add = function(e) {
if (e instanceof t) {
Array.prototype.push.apply(this.executeSystems, e.executeSystems);
e.executeSystems.length = 0;
} else this.executeSystems.push(e);
return this;
};
return t;
}();
t.$jt = v;
var $ = function() {
function t(t, e, n) {
this.bnc = null;
this.bnd = null;
this.bne = [];
this.bnf = new Map();
this.bng = 0;
this.bni = null;
this.bnj = new Map();
this.bnk = null;
this.bnl = null;
this.bnm = 0;
this.bnl = n;
this.bnk = t;
this.bng = e.length;
this.bnh = e;
this.bnc = new Array(this.bng);
this.bni = new Array(this.bng);
for (var i = 0; i < this.bng; i++) {
this.bnc[i] = [];
this.bni[i] = [];
}
this.bnv();
}
t.prototype.bnn = function() {
var t = this.bne.pop() || new this.bnk(this);
this.bnm++;
t.hm(this, this.bnm);
this.bnf.set(t.hg, t);
return t;
};
t.prototype.bno = function(t) {
var e = this.bne.pop() || new this.bnk(this);
e.hm(this, this.bnm);
e.ht(t);
return e;
};
t.prototype.bnp = function(t) {
this.bne.push(t);
this.bnf.delete(t.hg);
t.hr();
};
t.prototype.bnq = function(t, e) {
void 0 === e && (e = "e");
var n = e + "_" + t.getKey(), i = this.bnj.get(n);
if (!i) {
i = new a(t, this);
this.bnj.set(n, i);
for (var r = t.b, o = 0, b = r.length; o < b; o++) this.bni[r[o]].push(i.bmY.bind(i));
}
return i;
};
t.prototype.bnr = function(t) {
return this.bnc[t.tid].pop() || new this.bnh[t.tid]();
};
t.prototype.bns = function(t, e) {
this.bnc[t].push(e);
};
t.prototype.bnt = function(t, e) {
for (var n = this.bni[e], i = n.length - 1; i >= 0; i--) n[i](t);
};
t.prototype.bnu = function(t) {
return this.bnf.get(t);
};
t.prototype.bnv = function() {
this.bnd || (this.bnd = this.bnn());
return this.bnd;
};
t.prototype.bnw = function(t) {
this.bnd.hp(t) || this.bnd.hn(t);
return this.bnd.ho(t);
};
t.prototype.bnx = function(t) {
return this.bnn().hn(t);
};
t.prototype.bny = function() {
this.bnl = null;
this.bnc = null;
this.bne = null;
this.bni = null;
this.bnh = null;
this.bnd = null;
this.bnf.clear();
this.bnj.clear();
};
t.prototype.bnz = function(t) {
var e = this;
this.bnm = t.eid;
this.bnf.forEach(function(t) {
t.hs();
e.bne.push(t);
});
this.bnf.clear();
this.bnd = null;
t.eid2EntityValues.forEach(function(t) {
var n = e.bno(t);
e.bnf.set(n.hg, n);
});
-1 !== t.singletonEid && (this.bnd = this.bnf.get(t.singletonEid));
var n = t.groups;
this.bnj.forEach(function(t, e) {
var i = n[e];
i && t.bna(i);
});
};
t.prototype.bnA = function() {
var t = -1;
this.bnd && (t = this.bnd.hg);
var e = 0, n = this.bnf.size, i = new Array(n);
this.bnf.forEach(function(t) {
i[e] = t.hu();
e++;
});
var r = {};
this.bnj.forEach(function(t, e) {
r[e] = t.bnb();
});
return {
eid: this.bnm,
singletonEid: t,
eid2EntityValues: i,
groups: r
};
};
return t;
}();
t.$ju = $;
})(n.$je || (n.$je = {}));
cc._RF.pop();
}, {} ],
$NO: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "17ef0I4cBxJAp7RlJDHUttf", "$NO");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jF = n.$jE = n.$jD = n.$jC = void 0;
var i = t("./$NP"), r = t("./$NQ"), o = function() {
function t() {
this.bnB = new r.default();
this.bnC = 0;
this.bnD = 0;
this.bnE = 0;
this.bnF = 0;
this.bnG = 0;
this.bnH = new r.default();
this.bnI = new r.default();
}
t.prototype.bnJ = function(t, e, n, i, r) {
this.bnB.x = t;
this.bnB.y = e;
this.bnH.x = Math.cos(r);
this.bnH.y = Math.sin(r);
this.bnI.x = -this.bnH.y;
this.bnI.y = this.bnH.x;
this.bnD = n;
this.bnE = i;
this.bnF = n / 2;
this.bnG = i / 2;
this.bnC = r;
};
t.prototype.bnK = function(t) {
return this.bnF * Math.abs(t.boB(this.bnH)) + this.bnG * Math.abs(t.boB(this.bnI));
};
t.prototype.bnL = function(t) {
this.bnB.bow(t[0], t[1]);
this.bnC = t[2];
this.bnD = t[3];
this.bnE = t[4];
this.bnF = t[5];
this.bnG = t[6];
this.bnH.bow(t[7], t[8]);
this.bnI.bow(t[9], t[10]);
};
t.prototype.bnM = function() {
return [ this.bnB.x, this.bnB.y, this.bnC, this.bnD, this.bnE, this.bnF, this.bnG, this.bnH.x, this.bnH.y, this.bnI.x, this.bnI.y ];
};
return t;
}();
n.$jC = o;
var b = function() {
function t(t, e, n, i) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
void 0 === n && (n = 0);
void 0 === i && (i = 0);
this.x = t;
this.y = e;
this.width = n;
this.height = i;
}
t.prototype.bnN = function(t) {
this.x = t[0];
this.y = t[1];
this.width = t[2];
this.height = t[3];
};
t.prototype.bnO = function() {
return [ this.x, this.y, this.width, this.height ];
};
Object.defineProperty(t.prototype, "bnP", {
get: function() {
return this.y + this.height;
},
enumerable: !1,
configurable: !0
});
return t;
}();
n.$jD = b;
n.$jE = function() {
this.bnQ = new b();
this.bnR = new o();
this.bnS = 0;
this.bnT = new r.default();
this.bnU = [ new r.default(), new r.default(), new r.default(), new r.default() ];
};
var a = new r.default(), u = new r.default(), l = new r.default(), h = new r.default(1, 0), f = new r.default(0, 1), c = 0, s = 0, p = 0, d = 0, v = function() {
function t() {}
t.bnV = function(t, e) {
t.bnB.boz(e.bnB, a);
var n = t.bnH;
if (t.bnK(n) + e.bnK(n) <= Math.abs(a.boB(n))) return !1;
var i = t.bnI;
if (t.bnK(i) + e.bnK(i) <= Math.abs(a.boB(i))) return !1;
var r = e.bnH;
if (t.bnK(r) + e.bnK(r) <= Math.abs(a.boB(r))) return !1;
var o = e.bnI;
return !(t.bnK(o) + e.bnK(o) <= Math.abs(a.boB(o)));
};
t.bnW = function(t, e) {
var n = t.bnT.boz(e.bnT, u).boC(), i = t.bnS + e.bnS;
return n < i * i;
};
t.bnX = function(e, n, r, o, b) {
i.$jG.boe(e.bnR.bnB.x, e.bnR.bnB.y, n.bnT.x, n.bnT.y, -r, u);
return t.boa(e.bnR.bnB, l.bow(o / 2, b / 2), u, n.bnS);
};
t.bnY = function(e, n) {
var i = n.width / 2, r = n.height / 2;
a.x = e.bnB.x - (n.x + i);
a.y = e.bnB.y - (n.y + r);
var o = e.bnH, b = e.bnI, u = h, l = f;
return !(e.bnK(o) + t.bnZ(i, r, h, f, o) <= Math.abs(a.boB(o)) || e.bnK(b) + t.bnZ(i, r, h, f, b) <= Math.abs(a.boB(b)) || e.bnK(u) + t.bnZ(i, r, h, f, u) <= Math.abs(a.boB(u)) || e.bnK(l) + t.bnZ(i, r, h, f, l) <= Math.abs(a.boB(l)));
};
t.bnZ = function(t, e, n, i, r) {
return t * Math.abs(r.boB(n)) + e * Math.abs(r.boB(i));
};
t.boa = function(t, e, n, i) {
var r = Math.abs(n.x - t.x), o = Math.abs(n.y - t.y), b = Math.max(r - e.x, 0), a = Math.max(o - e.y, 0);
return b * b + a * a <= i * i;
};
t.bob = function(t, e) {
c = t.x + t.width;
s = t.y + t.height;
p = e.x + e.width;
d = e.y + e.height;
return !(c < e.x || p < t.x || s < e.y || d < t.y);
};
return t;
}();
n.$jF = v;
cc._RF.pop();
}, {
"./$NP": "$NP",
"./$NQ": "$NQ"
} ],
$NP: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "e9efeL9RdNBBKNNq8OJaPgs", "$NP");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jG = void 0;
var i = [ 1, 10, 100, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10 ], r = function() {
function t() {}
t.boe = function(e, n, i, r, o, b) {
var a = Math.cos(o), u = Math.sin(o);
b.x = t.boj((i - e) * a - (r - n) * u + e, 5);
b.y = t.boj((r - n) * a + (i - e) * u + n, 5);
};
t.bof = function(e) {
(e %= 360) > 180 ? e -= 360 : e < -180 && (e += 360);
return t.boj(e, 5);
};
t.bog = function(e, n) {
var i = Math.sin(n), r = Math.cos(n), o = e.x, b = e.y;
e.x = t.boj(r * o - i * b, 5);
e.y = t.boj(i * o + r * b, 5);
return e;
};
t.boh = function(e, n) {
e.x = t.boj(e.x * n, 5);
e.y = t.boj(e.y * n, 5);
return e;
};
t.boi = function(e) {
this.boc = e.x;
this.bod = e.y;
var n = t.boj(this.boc * this.boc + this.bod * this.bod, 5);
if (n > 0) {
n = t.boj(1 / Math.sqrt(n), 5);
e.x = t.boj(this.boc * n, 5);
e.y = t.boj(this.bod * n, 5);
}
return e;
};
t.boj = function(t, e) {
void 0 === e && (e = 6);
var n = i[e];
return Math.round(t * n) / n;
};
t.bok = function(t, e) {
void 0 === e && (e = 6);
var n = e >= 0 && e <= 10 ? i[e] : Math.pow(10, e);
return Math.floor(t * n) / n;
};
t.bol = function(t) {
return 1 / this.boo(t);
};
t.boo = function(t) {
var e, n;
e = .5 * t;
n = t;
this.bon.setFloat64(0, t, !0);
var i = this.bon.getUint32(0, !0), r = this.bon.getUint32(4, !0);
i >>= 1;
r % 2 == 1 && (i += 2147483648);
r = 1608969040 - (r >>= 1);
if ((i = 3349813753 - i) < 0) {
r--;
i += 4294967296;
}
this.bon.setUint32(0, i, !0);
this.bon.setUint32(4, r, !0);
return (n = this.bon.getFloat64(0, !0)) * (1.5 - e * n * n);
};
t.bop = function() {
this.bon = new DataView(this.bom);
for (var t = 0; t < this.boq.length; t++) this.boq[t] = this.boq[t] / 1e3;
for (t = 0; t < this.bos.length; t++) this.bos[t] = this.bos[t] / 1e3;
};
t.bor = function(t) {
(t %= 360) < 0 && (t += 360);
return this.boq[t];
};
t.bot = function(t) {
(t %= 360) < 0 && (t += 360);
return this.bos[t];
};
t.bov = function(t, e) {
if (0 === e) return t > 0 ? 90 : -90;
var n = Math.floor(t / e * 1e3);
n < 0 && (n = -n);
for (var i = 0, r = this.bou; n > r[i]; ) i++;
t >= 0 ? e <= 0 && (i = 180 - i) : e <= 0 ? i += 180 : i = 360 - i;
return i;
};
t.boc = 0;
t.bod = 0;
t.bom = new ArrayBuffer(8);
t.boq = [ 0, 17, 35, 52, 70, 87, 105, 122, 139, 156, 174, 191, 208, 225, 242, 259, 276, 292, 309, 326, 342, 358, 375, 391, 407, 423, 438, 454, 469, 485, 500, 515, 530, 545, 559, 574, 588, 602, 616, 629, 643, 656, 669, 682, 695, 707, 719, 731, 743, 755, 766, 777, 788, 799, 809, 819, 829, 839, 848, 857, 866, 875, 883, 891, 899, 906, 914, 921, 927, 934, 940, 946, 951, 956, 961, 966, 970, 974, 978, 982, 985, 988, 990, 993, 995, 996, 998, 999, 999, 1e3, 1e3, 1e3, 999, 999, 998, 996, 995, 993, 990, 988, 985, 982, 978, 974, 970, 966, 961, 956, 951, 946, 940, 934, 927, 921, 914, 906, 899, 891, 883, 875, 866, 857, 848, 839, 829, 819, 809, 799, 788, 777, 766, 755, 743, 731, 719, 707, 695, 682, 669, 656, 643, 629, 616, 602, 588, 574, 559, 545, 530, 515, 500, 485, 469, 454, 438, 423, 407, 391, 375, 358, 342, 326, 309, 292, 276, 259, 242, 225, 208, 191, 174, 156, 139, 122, 105, 87, 70, 52, 35, 17, 0, -17, -35, -52, -70, -87, -105, -122, -139, -156, -174, -191, -208, -225, -242, -259, -276, -292, -309, -326, -342, -358, -375, -391, -407, -423, -438, -454, -469, -485, -500, -515, -530, -545, -559, -574, -588, -602, -616, -629, -643, -656, -669, -682, -695, -707, -719, -731, -743, -755, -766, -777, -788, -799, -809, -819, -829, -839, -848, -857, -866, -875, -883, -891, -899, -906, -914, -921, -927, -934, -940, -946, -951, -956, -961, -966, -970, -974, -978, -982, -985, -988, -990, -993, -995, -996, -998, -999, -999, -1e3, -1e3, -1e3, -999, -999, -998, -996, -995, -993, -990, -988, -985, -982, -978, -974, -970, -966, -961, -956, -951, -946, -940, -934, -927, -921, -914, -906, -899, -891, -883, -875, -866, -857, -848, -839, -829, -819, -809, -799, -788, -777, -766, -755, -743, -731, -719, -707, -695, -682, -669, -656, -643, -629, -616, -602, -588, -574, -559, -545, -530, -515, -500, -485, -469, -454, -438, -423, -407, -391, -375, -358, -342, -326, -309, -292, -276, -259, -242, -225, -208, -191, -174, -156, -139, -122, -105, -87, -70, -52, -35, -17 ];
t.bos = [ 1e3, 1e3, 999, 999, 998, 996, 995, 993, 990, 988, 985, 982, 978, 974, 970, 966, 961, 956, 951, 946, 940, 934, 927, 921, 914, 906, 899, 891, 883, 875, 866, 857, 848, 839, 829, 819, 809, 799, 788, 777, 766, 755, 743, 731, 719, 707, 695, 682, 669, 656, 643, 629, 616, 602, 588, 574, 559, 545, 530, 515, 500, 485, 469, 454, 438, 423, 407, 391, 375, 358, 342, 326, 309, 292, 276, 259, 242, 225, 208, 191, 174, 156, 139, 122, 105, 87, 70, 52, 35, 17, 0, -17, -35, -52, -70, -87, -105, -122, -139, -156, -174, -191, -208, -225, -242, -259, -276, -292, -309, -326, -342, -358, -375, -391, -407, -423, -438, -454, -469, -485, -500, -515, -530, -545, -559, -574, -588, -602, -616, -629, -643, -656, -669, -682, -695, -707, -719, -731, -743, -755, -766, -777, -788, -799, -809, -819, -829, -839, -848, -857, -866, -875, -883, -891, -899, -906, -914, -921, -927, -934, -940, -946, -951, -956, -961, -966, -970, -974, -978, -982, -985, -988, -990, -993, -995, -996, -998, -999, -999, -1e3, -1e3, -1e3, -999, -999, -998, -996, -995, -993, -990, -988, -985, -982, -978, -974, -970, -966, -961, -956, -951, -946, -940, -934, -927, -921, -914, -906, -899, -891, -883, -875, -866, -857, -848, -839, -829, -819, -809, -799, -788, -777, -766, -755, -743, -731, -719, -707, -695, -682, -669, -656, -643, -629, -616, -602, -588, -574, -559, -545, -530, -515, -500, -485, -469, -454, -438, -423, -407, -391, -375, -358, -342, -326, -309, -292, -276, -259, -242, -225, -208, -191, -174, -156, -139, -122, -105, -87, -70, -52, -35, -17, 0, 17, 35, 52, 70, 87, 105, 122, 139, 156, 174, 191, 208, 225, 242, 259, 276, 292, 309, 326, 342, 358, 375, 391, 407, 423, 438, 454, 469, 485, 500, 515, 530, 545, 559, 574, 588, 602, 616, 629, 643, 656, 669, 682, 695, 707, 719, 731, 743, 755, 766, 777, 788, 799, 809, 819, 829, 839, 848, 857, 866, 875, 883, 891, 899, 906, 914, 921, 927, 934, 940, 946, 951, 956, 961, 966, 970, 974, 978, 982, 985, 988, 990, 993, 995, 996, 998, 999, 999, 1e3 ];
t.bou = [ 0, 17, 35, 52, 70, 87, 105, 123, 141, 158, 176, 194, 213, 231, 249, 268, 287, 306, 325, 344, 364, 384, 404, 424, 445, 466, 488, 510, 532, 554, 577, 601, 625, 649, 675, 700, 727, 754, 781, 810, 839, 869, 900, 933, 966, 1e3, 1036, 1072, 1111, 1150, 1192, 1235, 1280, 1327, 1376, 1428, 1483, 1540, 1600, 1664, 1732, 1804, 1881, 1963, 2050, 2145, 2246, 2356, 2475, 2605, 2747, 2904, 3078, 3271, 3487, 3732, 4011, 4331, 4705, 5145, 5671, 6314, 7115, 8144, 9514, 11430, 14301, 19081, 28636, 57290, Infinity ];
return t;
}();
n.$jG = r;
r.bop();
cc._RF.pop();
}, {} ],
$NQ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "38cfc4OtbZDbri3y3oR2Xgv", "$NQ");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = t("./$NP"), r = function() {
function t(t, e) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
this.x = t;
this.y = e;
}
t.prototype.bow = function(t, e) {
this.x = t;
this.y = e;
return this;
};
t.prototype.box = function(t) {
this.x = t.x;
this.y = t.y;
return this;
};
t.prototype.boy = function() {
return new t(this.x, this.y);
};
t.prototype.boz = function(t, e) {
e.x = this.x - t.x;
e.y = this.y - t.y;
return e;
};
t.prototype.boA = function() {
return Math.sqrt(this.x * this.x + this.y * this.y);
};
t.prototype.boB = function(t) {
return this.x * t.x + this.y * t.y;
};
t.prototype.boC = function() {
return this.x * this.x + this.y * this.y;
};
t.prototype.boD = function(t) {
t.x = -this.x;
t.y = -this.y;
return t;
};
t.prototype.boE = function(t, e) {
var n = this.x, i = this.y, r = Math.sin(t), o = Math.cos(t);
e.x = o * n - r * i;
e.y = r * n + o * i;
return e;
};
t.prototype.boF = function(t, e) {
var n = this.x, r = this.y, o = i.$jG.boj(Math.sin(t)), b = i.$jG.boj(Math.cos(t));
e.x = i.$jG.boj(b * n - o * r);
e.y = i.$jG.boj(o * n + b * r);
return e;
};
t.prototype.boG = function(t) {
this.x += t.x;
this.y += t.y;
return this;
};
return t;
}();
n.default = r;
cc._RF.pop();
}, {
"./$NP": "$NP"
} ],
$NR: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "a07abgaKHJCu6M6Cjxxa2gH", "$NR");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = function() {
function t(t, e, n) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
void 0 === n && (n = 0);
this.x = t;
this.y = e;
this.z = n;
}
t.prototype.boH = function() {
return new t(this.x, this.y, this.z);
};
t.prototype.boI = function(e) {
return new t(this.x - e.x, this.y - e.y, this.z - e.z);
};
t.prototype.boJ = function() {
return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
};
return t;
}();
n.default = i;
cc._RF.pop();
}, {} ],
$NS: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "6b0e7TWtFRH9bKY+l8IOApn", "$NS");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jJ = void 0;
var o = t("../$KC"), b = t("../math/$NP"), a = t("../lib/$NN"), u = t("../component/$MM"), l = t("../../../game/src/battle/types/$WY"), h = t("../math/$NQ"), f = t("../component/$NJ"), c = t("../../../game/src/battle/$QM"), s = [ 150, 200 ], p = [ 1500, 2e3 ], d = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boK = new h.default();
return e;
}
e.prototype.filter = function() {
return a.$je.allOf(u.default);
};
e.prototype.update = function(t) {
var e;
if (this.context.bnl.gs().bjr.canCtrl) for (var n = 0, i = t; n < i.length; n++) {
var r = i[n], o = b.$jG.boj(this.dt * r.hy.worldTimeScale, 4), a = r.hv;
if (r.hN) {
r.hN.biX = a.bgO();
b.$jG.boj(r.hi.bnl.fk - r.hN.bjd) > 2 && r.hN.bjk();
}
if (a.bgR(c.GameAttriTableEnum.spirit) > 0) {
a.mpInteravl = b.$jG.boj(a.mpInteravl + o);
if (a.mpInteravl > 1) {
a.mpInteravl = b.$jG.boj(a.mpInteravl - 1);
var u = Math.floor(b.$jG.boj(a.bgR(c.GameAttriTableEnum.mpRecSp) / 60));
u && (a.bgF = Math.min(a.bgG, a.bgF + u));
}
}
if (a.bgy > 0) {
a.comboTime = b.$jG.boj(a.comboTime + o, 4);
if (a.comboTime >= a.comboLimitTime) {
a.comboTime = 0;
a.bgy = 0;
this.postEvent(l.$uD.$vA.z, r);
}
}
for (var h = r.hv.bgd, f = b.$jG.boj(Math.max(a.bgR(c.GameAttriTableEnum.spellCdRecoverRate) + c.GameAttributeConstRatioRange, 0) / c.GameAttributeConstRatioRange), s = 0, p = h; s < p.length; s++) {
var d = p[s];
if (d.Do < 2) d.cd > 0 && (d.cd = Math.max(b.$jG.boj(d.cd - b.$jG.boj(f * o), 4), 0)); else if (d.Dn < d.Do) {
d.cd = b.$jG.boj(d.cd - b.$jG.boj(f * o), 4);
if (d.cd <= 0) {
d.cd = d.maxCdTime;
d.Dn = Math.min(d.Do, d.Dn + 1);
}
}
}
var v = null === (e = r.hL) || void 0 === e ? void 0 : e.blO;
if (v) for (var $ = 0, g = v; $ < g.length; $++) {
var y = g[$];
y.Eh > 0 && (y.Eh = Math.max(b.$jG.boj(y.Eh - o, 4), 0));
}
if (r.hE) {
var m = r.hE.bjJ;
if (r.hG) {
m.aimDist = r.hG.biv;
m.aimDiffX = r.hG.bit.x;
m.aimDiffY = r.hG.bit.y;
}
}
}
};
e.prototype.boL = function(t, e) {
var n = t.hv;
if (n.bgu > 0) n.bgu = b.$jG.boj(n.bgu - e); else {
n.bgu = .5;
var i = t.hE, r = t.hw, a = t.hi.bnw(f.default), u = t.hi.bnl;
if (null == t ? void 0 : t.hG) {
var h = this.context.bnu(t.hG.bis);
if (h) {
var c = t.hG.biu.boD(this.boK), p = b.$jG.boj(1 * u.ge() - .5);
c.boF(p, c);
var d = s[0] + (s[1] > 0 ? b.$jG.boj(b.$jG.boj(u.ge() - .5) * s[1]) : 0), v = h.hw, $ = b.$jG.boj(b.$jG.boj(c.x * d) + v.x, 3), g = b.$jG.boj(b.$jG.boj(c.y * d) + v.y, 3);
$ = Math.max(Math.min($, a.bmv), a.bmu);
g = Math.max(Math.min(g, a.bmx), a.bmw);
i.bkn === l.$uD.$vd.c && (g = r.y);
this.boK.bow($, g);
o.default.FE(t, this.boK, 50);
}
}
}
};
e.prototype.boM = function(t, e) {
var n = t.hv;
if (n.bgu > 0) n.bgu = b.$jG.boj(n.bgu - e); else {
n.bgu = .3;
var i = t.hE, r = t.hw, a = t.hi.bnw(f.default), u = t.hi.bnl;
if (null == t ? void 0 : t.hG) {
var h = this.context.bnu(t.hG.bis);
if (h) {
var c = t.hG.biu.boD(this.boK), s = b.$jG.boj(1 * u.ge() - .5);
c.boF(s, c);
var d = p[0] + (p[1] > 0 ? b.$jG.boj(b.$jG.boj(u.ge() - .5) * p[1]) : 0), v = h.hw, $ = b.$jG.boj(b.$jG.boj(c.x * d) + v.x, 3), g = b.$jG.boj(b.$jG.boj(c.y * d) + v.y, 3);
$ = Math.max(Math.min($, a.bmv), a.bmu);
g = Math.max(Math.min(g, a.bmx), a.bmw);
i.bkn === l.$uD.$vd.c && (g = r.y);
this.boK.bow($, g);
o.default.FE(t, this.boK, 50);
}
}
}
};
return e;
}(a.$je.$jo);
n.$jJ = d;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../component/$MM": "$MM",
"../component/$NJ": "$NJ",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ"
} ],
$NT: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "e9472Qo3VBNVZknw44dLUGI", "$NT");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jK = void 0;
var o = t("../$KC"), b = t("../math/$NP"), a = t("../lib/$NN"), u = t("../component/$MM"), l = t("../component/$MN"), h = t("../component/$MP"), f = t("../component/$MW"), c = t("../component/$NH"), s = t("../component/$NL"), p = t("../component/$NM"), d = t("../math/$NQ"), v = t("../component/$NJ"), $ = t("../../../game/src/battle/types/$WY"), g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boN = new d.default();
e.boO = new d.default();
return e;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return a.$je.allOf(f.default, s.default, p.default, u.default, l.default, h.default, c.default);
};
e.prototype.update = function(t) {
if (this.context.bnl.gs().bjr.canCtrl) for (var e = 0, n = t; e < n.length; e++) {
o = n[e];
this.boP(o);
} else for (var i = 0, r = t; i < r.length; i++) {
var o;
(o = r[i]).hx.bmg && (o.hx.blW = 0);
}
};
e.prototype.boP = function(t) {
var e, n, i = t.hv;
if (o.default.Gw(i)) {
var r = t.hx;
if (r.bmg) {
var a = t.hi.bnl, u = t.hi.bnw(v.default), l = t.hE, h = (l.bjJ, t.hw), f = t.hI, c = f.bhZ, s = t.hy, p = (b.$jG.boj(this.dt * s.worldTimeScale * s.hitPauseTimeScale * s.mainTimeScale, 4), 
this.context.bnu(t.hG.bis)), d = t.hG, g = this.boN;
g.x = h.x;
g.y = h.y;
var y = c.move;
if (!c.playerAI && y) {
f.bie = b.$jG.boj(this.dt + (f.bie || 0), 4);
if (f.bie >= y.time) {
f.bie = 0;
var m = y.param, j = p ? p.hw : null, D = this.boO;
if (j) {
D.x = j.x;
D.y = j.y;
} else {
D.x = b.$jG.boj(g.x + 800 * b.$jG.boj(a.ge() - .5));
D.y = b.$jG.boj(g.y + 800 * b.$jG.boj(a.ge() - .5));
}
var G = g.x - D.x, x = D.x + (m[0] + b.$jG.boj((m[1] - m[0]) * a.ge(), 3)) * Math.sign(G);
x = Math.max(Math.min(x, u.bmv), u.bmu);
var E = a.ge() > .5 ? 1 : -1, M = D.y + (m[2] + b.$jG.boj((m[3] - m[2]) * a.ge(), 3)) * E;
M = Math.max(Math.min(M, u.bmx), u.bmw);
f.bia.x = x;
f.bia.y = M;
}
var N = y.lockY, _ = y.lockX, w = i.bgJ > 0, k = f.bia.x - g.x, O = f.bia.y - g.y, F = 0, R = 0;
!_ && Math.abs(k) > 30 && (F = .6 * Math.sign(k));
!N && Math.abs(O) > 30 && (R = .6 * Math.sign(O));
if (0 === F && 0 === R || w) r.blW = 0; else {
r.blW = 1;
r.blX.x = F;
r.blX.y = R;
r.blY = 0;
}
if (!w && l.bjx && (null === (e = l.bjF.aiChangeDir) || void 0 === e || e) && (l.bjx.changeDir || l.bjx.idle || l.bjx.move)) {
var P = d.bit.x;
P && -1 === Math.sign(P * t.hw.bmF) && (t.hw.bmF *= -1);
}
}
var T = c.acts;
f.bic = b.$jG.boj(this.dt + (f.bic || 0), 4);
if (T && f.bif) {
var I = c.actTime;
if (f.bic >= I) {
for (var A = Math.abs(d.bit.x), C = Math.abs(d.bit.y), S = function(e) {
if (f.bic >= I) {
var r = e.area;
if (!(A >= r[0] && A < r[1])) return "continue";
if (!(C >= r[2] && C < r[3])) return "continue";
var b = i.bgr(e.cmdId);
if (b && o.default.Fz(t, b)) {
if (!(null === (n = t.hE.bjA) || void 0 === n ? void 0 : n.find(function(t) {
if (!t.tgs) return !1;
var n = t.tgs[0];
return !!n && (n[0] === $.$uD.$vv.c && n[1][0] === e.cmdId);
}))) return "continue";
if (!(e.rate >= a.ge())) {
b.cd = Math.max(e.commonCD, 0);
return "continue";
}
b.cd = b.maxCdTime;
f.bic = 0;
f.bid = 0;
var u = o.default.Gb(t.hx, e.cmdId);
u && (u.c = 1);
}
}
}, H = 0, B = T; H < B.length; H++) S(B[H]);
f.bic = 0;
}
}
}
}
};
return e;
}(a.$je.$jo);
n.$jK = g;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../game/src/battle/types/$WY": void 0,
"../component/$MM": "$MM",
"../component/$MN": "$MN",
"../component/$MP": "$MP",
"../component/$MW": "$MW",
"../component/$NH": "$NH",
"../component/$NJ": "$NJ",
"../component/$NL": "$NL",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ"
} ],
$NU: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "eb186ls4f5K5LHPjXxGpATY", "$NU");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jL = void 0;
var o = t("../math/$NP"), b = t("../lib/$NN"), a = t("../component/$MP"), u = t("../component/$MY"), l = t("../component/$NM"), h = t("../math/$NQ"), f = t("../../../game/src/battle/types/$WY"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.boQ = new h.default();
e.boR = new h.default();
return e;
}
e.prototype.filter = function() {
return b.$je.allOf(l.default, u.default, a.default);
};
e.prototype.update = function(t) {
for (var e, n, i, r = this.context.bnl.fu.bmW, b = 0, a = t; b < a.length; b++) {
var u = a[b], l = u.hG, h = u.hD.bkR, c = u.hw, s = null;
switch (l.biw) {
case f.$uD.$vh.b:
var p = Number.MAX_SAFE_INTEGER;
if (u.hO) {
for (var d = 0, v = r; d < v.length; d++) if ((j = (m = v[d]).hv) && j.bgI && m.hD.bkR !== h) {
if (j.bgA) continue;
var $ = m.hE;
if ($) {
if (!0 === (null === (i = $.bjF) || void 0 === i ? void 0 : i.noAutoAim)) continue;
if (!$.bkF()) continue;
}
this.boQ.x = o.$jG.boj(m.hw.x - c.x, 3);
this.boQ.y = o.$jG.boj(m.hw.y - c.y, 3);
if ((D = this.boQ.boC()) < p) {
p = D;
s = m;
l.bit.bow(this.boQ.x, this.boQ.y);
}
}
} else for (var g = 0, y = r; g < y.length; g++) {
var m, j;
if ((j = (m = y[g]).hv) && j.bgI && m.hD.bkR !== h) {
if (!0 === (null === (n = null === (e = m.hE) || void 0 === e ? void 0 : e.bjF) || void 0 === n ? void 0 : n.noAutoAim)) continue;
this.boQ.x = o.$jG.boj(m.hw.x - c.x, 3);
this.boQ.y = o.$jG.boj(m.hw.y - c.y, 3);
var D;
if ((D = this.boQ.boC()) < p) {
p = D;
s = m;
l.bit.bow(this.boQ.x, this.boQ.y);
}
}
}
break;

case f.$uD.$vh.c:
if (s = this.context.bnu(l.bis)) {
this.boQ.x = s.hw.x;
this.boQ.y = s.hw.y;
l.bit.x = o.$jG.boj(this.boQ.x - c.x, 3);
l.bit.y = o.$jG.boj(this.boQ.y - c.y, 3);
}
}
if (s) {
l.bis !== s.hg && (l.bis = s.hg);
0 === l.bit.x && 0 === l.bit.y && (l.bit.x = 1);
l.biv = o.$jG.boj(l.bit.boA(), 3);
l.biu.bow(l.bit.x, l.bit.y);
o.$jG.boi(l.biu);
}
}
};
return e;
}(b.$je.$jo);
n.$jL = c;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WY": void 0,
"../component/$MP": "$MP",
"../component/$MY": "$MY",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ"
} ],
$NV: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ee20fRe8b5GR5whwhWLLvfA", "$NV");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jO = n.$jN = n.$jM = void 0;
var o, b = t("../$KC"), a = t("../math/$NP"), u = t("../lib/$NN"), l = t("../component/$MQ"), h = t("../component/$MZ"), f = t("../component/$NM"), c = t("../../../game/src/battle/types/$WY"), s = t("../utils/$OG"), p = t("../component/$MS"), d = t("../../../game/src/battle/$QM");
(function(t) {
t[t.c = 0] = "c";
t[t.e = 1] = "e";
t[t.h = 2] = "h";
t[t.i = 3] = "i";
})(o || (o = {}));
var v = function() {
this.boS = -1;
this.boT = 0;
this.boU = 0;
this.boV = o.c;
this.boW = 0;
this.boX = 0;
this.boY = 0;
this.boZ = 0;
this.bpa = 0;
this.bpb = 0;
};
n.$jM = v;
var $ = function() {
this.bpc = -1;
this.bpd = 0;
this.bpe = [];
};
n.$jN = $;
var g = function(t, e) {
return e.boW - t.boW;
}, y = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.fixedUpdate = !0;
e.bpf = new Map();
e.bpg = [];
e.bph = [];
e.bpi = [];
e.bpj = [];
e.bpk = new Map();
e.bpl = [];
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {
this.bpf.clear();
this.bpg.length = 0;
this.bph.length = 0;
this.bpi.length = 0;
this.bpj.length = 0;
this.bpk.clear();
this.bpl.length = 0;
};
e.prototype.bpm = function(t, e) {
var n = this.bpk.get(t);
if (void 0 === n) {
this.bpl.push(t);
this.bpk.set(t, e);
} else n < e && this.bpk.set(t, e);
};
e.prototype.filter = function() {
return u.$je.allOf(f.default, h.default, l.default);
};
e.prototype.bpn = function(t) {
var e = this.bpf.get(t);
if (!e) {
var n = this.bpi;
n.length > 0 ? (e = n.pop()).bpe.length = 0 : e = new $();
e.bpc = t;
e.bpd = 0;
this.bpg.push(e);
this.bpf.set(t, e);
}
return e;
};
e.prototype.bpo = function(t, e, n, i) {
var r, o = this.bpj;
(r = o.length > 0 ? o.pop() : new v()).boS = e;
r.boT = 0;
r.boU = n;
r.boW = n;
r.boV = i;
r.boX = 0;
r.boY = 0;
r.boZ = 0;
r.bpa = 0;
r.bpb = 0;
t.bpe.push(r);
return r;
};
e.prototype.bpp = function() {
for (var t = this.bpg, e = this.bpj, n = this.bpi, i = 0, r = t.length; i < r; i++) {
for (var o = t[i], b = o.bpe, a = 0, u = b.length; a < u; a++) {
var l = b[a];
l.boY = 0;
l.boZ = 0;
e.push(l);
}
b.length = 0;
n.push(o);
}
t.length = 0;
};
e.prototype.update = function(t) {
var e, n, i, r, u, l, h = this.context, f = h.bnl, v = this.dt, $ = f.ft, y = this.bpk, m = this.bpl;
y.clear();
m.length = 0;
var j = this.bpg, D = this.bph;
j.length = 0;
D.length = 0;
this.bpf.clear();
for (var G = 0, x = !1, E = 0, M = t; E < M.length; E++) {
var N = M[E];
if (h.bnu(N.hg)) {
var _ = N.hA;
if (_ && _.blb) {
var w = N.hB;
if (w && w.biy === c.$uD.$vw.b && (ee = w.biz) && (ne = h.bnu(w.biC))) {
var k = h.bnu(w.biD);
if (k && k.hD && k.hv) {
var O = ne.hy, F = k.hv, R = k.hE, P = k.hg, T = ne.hg, I = ne.hw;
G = 0;
if (F.bgI) {
if (ee.hitCount > 1 && w.biG < ee.hitCount - 1) {
var A = a.$jG.boj(v * O.bmE, 4);
w.biI += A;
if (w.biI > ee.hitInterval) {
w.biG++;
w.biI = a.$jG.boj(w.biI - ee.hitInterval, 4);
w.biH = 0;
w.biE = {};
}
}
for (var C = 0, S = _.bkZ; C < S.length; C++) {
var H = S[C], B = h.bnu(H);
if (B) {
var L = void 0, Y = B.ho(p.default);
if (Y && (L = h.bnu(Y.biT))) {
var W = L.hg, z = L.hD;
if (z && z.bkR !== w.biB && W !== T) if (L.hv) {
if (L.hw && !w.biE[W]) {
var X = L.hv, K = L.hw, Q = L.hE;
if (Q && X.bgI && !X.bgA) {
w.biE[W] = 1;
if (!w.biH) {
w.biH = 1;
w.biF += 1;
}
I.x, K.x;
var V = (K.bmF, b.default.Ey(k, ne, L));
D.push(V);
V.hitInfo = ee;
V.isPhysical = 2 !== ee.damageType;
V.elementValue = F.bhJ(null !== (e = ee.elementValue) && void 0 !== e ? e : 0);
V.beforeHP = X.bgD;
var Z = w.biJ, J = Z > 0 ? b.default.Gr(f, V, Z, !0) : 0;
V.finalDamage = J;
var q = B.hA.bkY.bkU[0].world.bnQ, U = q.x, tt = q.y, et = q.x + q.width, nt = q.y + q.height, it = _.bkY.bkU, rt = it[0].world.bnQ;
if (it.length > 1) for (var ot = 0, bt = it; ot < bt.length; ot++) {
var at = bt[ot].world.bnQ;
if (at.x + at.width >= U && et >= at.x) {
rt = at;
break;
}
}
var ut = rt.x, lt = rt.y, ht = rt.x + rt.width, ft = rt.y + rt.height, ct = Math.max(U, ut), st = Math.max(tt, lt), pt = Math.min(et, ht), dt = Math.min(nt, ft), vt = a.$jG.boj((ct + pt) / 2), $t = a.$jG.boj((st + dt) / 2);
V.hitX = vt;
V.hitY = K.y;
V.hitZ = $t;
if (Q.bjD[c.$uD.$vx.i]) {
b.default.GR(L, c.$uD.$vx.i, V, 0);
for (var gt = !1, yt = 0, mt = Q.bjB; yt < mt.length; yt++) if ((Et = mt[yt]).DF.event === c.$uD.$vx.i && 1 === Et.DU) {
gt = !0;
var jt = 0;
Et.DV > 0 && (jt = Math.floor(a.$jG.boj(J * (Et.DV / d.GameAttributeConstRatioRange))));
if (Et.DW > 0) {
var Dt = b.default.Ey(L, L, k);
Dt.isPhysical = X.bhl > X.bhm;
Dt.elementValue = 0;
jt += b.default.Gr(f, Dt, Et.DW / d.GameAttributeConstRatioRange, !1);
b.default.Ez(Dt);
}
if (!F.bgA && F.bgI) {
k.hX(k, jt);
!F.bgI && b.default.Hf(k, 0, 0);
f.gu(c.$uD.$vA.E, k, jt, !1);
}
break;
}
if (gt) {
V.finalDamage = 0;
J = 0;
}
}
if (V.elementValue & c.$uD.$uY.b && X.bhc) for (var Gt = 0, xt = Q.bjB.filter(function(t) {
return t.DF.abnormalType === c.$uD.$vp.c;
}); Gt < xt.length; Gt++) {
var Et = xt[Gt];
b.default.Gj(L, Et, !1);
}
if (J > 0) {
L.hX(k, J, V);
(null == R ? void 0 : R.bjD[c.$uD.$vx.h]) && b.default.GR(k, c.$uD.$vx.h, V, J);
}
if (Z > 0) {
F.bgy += 1;
F.comboTime = 0;
this.postEvent(c.$uD.$vA.z, k);
X.bgy = 0;
X.comboTime = 0;
this.postEvent(c.$uD.$vA.z, L);
}
var Mt = ne.hE, Nt = null !== (i = null === (n = Q.bjx) || void 0 === n ? void 0 : n.superArmor) && void 0 !== i ? i : 0;
this.bpm(T, ee.p1PauseTime);
2 !== Nt && this.bpm(W, ee.p2PauseTime);
this.postEvent(c.$uD.$vA.B, V);
X.bgv++;
var _t = X.bgI, wt = !K.bmJ, kt = X.bhb;
x = -1 !== Q.bjS;
if (ee.grab && (ee.grab.level || !L.im && !kt) && !x && Mt.bjR.length + G < ee.grab.count) if (_t) {
x = !0;
G += 1;
(Ft = this.bpn(W)).bpd = 1;
(Pt = this.bpo(Ft, P, 1e3, o.h)).boX = T;
Pt.boY = ee.grab;
} else {
var Ot = b.default.FT(ne, L, c.$uD.$vo.g), Ft = this.bpn(W);
(Pt = this.bpo(Ft, P, 1e4, o.i)).boX = a.$jG.boj(400 * Ot, 3);
Pt.boY = 600;
} else if (!(X.bgz > (ee.breakSuperArmor || 0) || x || kt) || !_t) if (ee.air && !Q.bkv && (ee.air.fly || wt && !Q.bkw || Q.bkc)) {
Ft = this.bpn(W);
(Pt = this.bpo(Ft, P, 1, o.e)).boX = b.default.FT(ne, L, ee.changeDirType);
Pt.boY = b.default.FT(ne, L, ee.air.hitDirType);
Pt.boZ = ee;
} else if (ee.ground) {
var Rt = Q.bkx;
if (_t || L.hR) if (Rt) {
if (!_t) {
Ft = this.bpn(W);
this.bpo(Ft, P, 1e4, o.i);
}
} else {
Ot = b.default.FT(ne, L, ee.ground.hitDirType), Ft = this.bpn(W);
(Pt = this.bpo(Ft, P, 0, o.c)).boX = b.default.FT(ne, L, ee.changeDirType);
Pt.boY = ee.ground.slideTime;
Pt.boZ = ee.ground.time;
Pt.bpa = a.$jG.boj(Ot * ee.ground.force, 3);
Pt.bpb = ee.ground.blockVal;
} else {
Ot = b.default.FT(ne, L, c.$uD.$vo.g), Ft = this.bpn(W);
var Pt = this.bpo(Ft, P, 1e4, o.i);
if (!Rt && !Q.bkv) {
Pt.boX = a.$jG.boj(400 * Ot, 3);
Pt.boY = 600;
}
}
} else if (!_t) {
Ot = b.default.FT(ne, L, c.$uD.$vo.g), Ft = this.bpn(W);
(Pt = this.bpo(Ft, P, 1e4, o.i)).boX = a.$jG.boj(400 * Ot, 3);
Pt.boY = 600;
}
}
}
} else {
if (w.biE[W]) continue;
w.biE[W] = 1;
ee.lg && b.default.Gg(k, null, ee.lg, ee);
b.default.GQ(L, null, c.$uD.$vx.d, ee);
}
}
}
}
}
}
}
}
}
}
for (var Tt = 0, It = $.bmW; Tt < It.length; Tt++) {
var At = It[Tt].hy;
if (1 !== At.hitPauseTimeScale) {
At.bmD = a.$jG.boj(At.bmD - v * At.worldTimeScale, 4);
if (At.bmD <= 0) {
At.bmD = 0;
At.hitPauseTimeScale = 1;
}
}
}
for (var Ct = 0, St = m.length; Ct < St; Ct++) {
var Ht = m[Ct], Bt = h.bnu(Ht);
Bt && b.default.Ge(Bt.hy, 0, y.get(Ht));
}
for (var Lt = 0, Yt = j; Lt < Yt.length; Lt++) for (var Wt = 0, zt = (le = Yt[Lt]).bpe; Wt < zt.length; Wt++) {
var Xt = zt[Wt];
if (this.bpf.has(Xt.boS)) {
Xt.boT = 1;
Xt.boW = Xt.boU - 1e3;
}
}
for (var Kt = 0, Qt = j; Kt < Qt.length; Kt++) {
var Vt = (le = Qt[Kt]).bpe;
Vt.length > 1 && s.default.bpB(Vt, g, 0, Vt.length);
var Zt = Vt[0];
switch (Zt.boV) {
case o.c:
if (!(L = h.bnu(le.bpc))) break;
if (!(te = L.hv)) break;
if (!te.bgI && L.hR) b.default.Hg(L); else {
var Jt = te.bhf, qt = Zt.boZ;
Jt && (qt = a.$jG.boj(qt * (1e3 / (Jt + 1e3))));
b.default.Fe(L, Zt.boY, Math.max(qt, .05), c.$uD.commonHitActions[Math.floor(2 * f.ge())]);
L.hw.bmF = Zt.boX;
if (!L.hE.bko) {
var Ut = L.hz;
b.default.FG(Ut, Zt.bpa, 0, 0);
Ut.blp = Zt.bpb;
L.ih();
}
}
break;

case o.e:
var te;
if (!(L = h.bnu(le.bpc))) break;
if (!(te = L.hv)) break;
L.hw.bmF = Zt.boX;
if (!te.bgI && L.hR) b.default.Hg(L); else {
var ee = Zt.boZ;
b.default.Ft(L, ee.air, Zt.boY);
}
break;

case o.h:
var ne;
if (!(L = h.bnu(le.bpc))) break;
if (!(ne = h.bnu(Zt.boX))) break;
var ie = L.hE;
if (Zt.boT) {
var re = L.hw;
if (!((null === (r = L.hv) || void 0 === r ? void 0 : r.bhb) || ie.bkv)) {
ie.bkc = 1;
b.default.Ft(L, {
z: 320,
hitDirType: 0,
force: 300
}, -1 * re.bmF);
}
} else {
if (!(Mt = ne.hE)) break;
ie.bjS = ne.hg;
Mt.bjR.push({
id: Zt.boY.id,
eid: L.hg
});
b.default.Fg(L);
b.default.Fs(ne, L);
L.ig();
}
break;

case o.i:
if (!(L = h.bnu(le.bpc))) break;
b.default.Hf(L, Math.abs(Zt.boX), Zt.boY);
}
}
for (var oe = 0, be = D; oe < be.length; oe++) {
(Xt = be[oe]).hitInfo.lg && b.default.Gg(Xt.atkEntity, null, Xt.hitInfo.lg, Xt);
Xt.hitInfo.lg2 && b.default.Gg(Xt.fromEntity, null, Xt.hitInfo.lg2, Xt);
(null === (u = Xt.atkEntity.hE) || void 0 === u ? void 0 : u.bjD[c.$uD.$vx.c]) && b.default.GQ(Xt.atkEntity, null, c.$uD.$vx.c, Xt);
(null === (l = Xt.target.hE) || void 0 === l ? void 0 : l.bjD[c.$uD.$vx.d]) && b.default.GQ(Xt.target, null, c.$uD.$vx.d, {
target: Xt.atkEntity
});
}
for (var ae = 0, ue = D; ae < ue.length; ae++) {
var le = ue[ae];
b.default.Ez(le);
}
D.length = 0;
this.bpp();
};
return e;
}(u.$je.$jo);
n.$jO = y;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../component/$MQ": "$MQ",
"../component/$MS": "$MS",
"../component/$MZ": "$MZ",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../utils/$OG": "$OG"
} ],
$NW: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "cd180imBE9POIWoGVJmytuq", "$NW");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jQ = void 0;
var o = t("../$KC"), b = t("../lib/$NN"), a = t("../component/$MZ"), u = t("../component/$MS"), l = t("../component/$MT"), h = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.fixedUpdate = !0;
e.bpq = [];
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return b.$je.allOf(a.default, l.default);
};
e.prototype.update = function(t) {
for (var n, i = this.context, r = 0, b = t; r < b.length; r++) {
var a = b[r];
if (i.bnu(a.hg)) {
var l = a.hM;
if (l) if (null === (n = a.hA) || void 0 === n ? void 0 : n.blb) {
var h = l.biV;
if (h) {
this.bpq.length = 0;
var f = i.bnu(l.biU);
if (f && f.hD) {
for (var c = 0, s = a.hA.bkZ; c < s.length; c++) {
var p = s[c], d = i.bnu(p);
if (d) {
var v, $ = d.ho(u.default);
if ($ && (null == (v = i.bnu($.biT)) ? void 0 : v.hE) && (!h.isActor || v.hv)) {
if ("boolean" == typeof h.isPlayerCamp) {
if (!v.hD) continue;
var g = this.context.bnl.fF[0].dcf;
if (v.hD.bkR === g !== h.isPlayerCamp) continue;
}
if ("boolean" == typeof h.isSelfCamp) {
if (!v.hD) continue;
if (v.hD.bkR === f.hD.bkR !== h.isSelfCamp) continue;
}
var y = v.hg;
this.bpq.push(y);
if (-1 === l.biW.indexOf(y)) {
l.biW.push(y);
for (var m = 0, j = h.buffs; m < j.length; m++) {
var D = j[m];
o.default.Gh(v, D, null, f.hg);
}
}
}
}
}
for (var G = l.biW.length - 1; G >= 0; --G) {
y = l.biW[G];
if (-1 === this.bpq.indexOf(y)) {
l.biW.splice(G, 1);
var x = i.bnu(y);
if (null == x ? void 0 : x.hE) for (var E = 0, M = h.buffs; E < M.length; E++) {
D = M[E];
o.default.Gk(x, D);
}
}
}
}
}
} else e.bpr(a);
}
}
};
e.bpr = function(t) {
var e = t.hM, n = e.biV.buffs;
if ((null == n ? void 0 : n.length) > 0 && e.biW.length > 0) {
for (var i = 0, r = e.biW; i < r.length; i++) {
var b = r[i], a = t.hi.bnu(b);
if (null == a ? void 0 : a.hE) for (var u = 0, l = n; u < l.length; u++) {
var h = l[u];
o.default.Gk(a, h);
}
}
e.biW.length = 0;
}
};
return e;
}(b.$je.$jo);
n.$jQ = h;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../component/$MS": "$MS",
"../component/$MT": "$MT",
"../component/$MZ": "$MZ",
"../lib/$NN": "$NN"
} ],
$NX: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "34a68YxPn1Kh5Pp+o/+l97i", "$NX");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jR = void 0;
var o = t("../math/$NP"), b = t("../lib/$NN"), a = t("../component/$MV"), u = t("../component/$NL"), l = t("../BattleBehaviorLogic"), h = t("../../../game/src/battle/types/$WW"), f = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return b.$je.allOf(a.default, u.default);
};
e.prototype.update = function(t) {
var e = this.context.bnl, n = t[0].hH, i = t[0].hy, r = o.$jG.boj(this.dt * i.hitPauseTimeScale * i.worldTimeScale * i.mainTimeScale, 4);
n.bjs = n.bjt;
n.bjt = o.$jG.boj(n.bjt + r, 4);
if (n.bjn.duration) {
n.bjn.time = o.$jG.boj(n.bjn.time + this.dt);
if (n.bjn.time >= n.bjn.duration) {
e.go(1);
n.bjw(0);
}
}
if (n.bjr.time) {
e.fk = o.$jG.boj(e.fk + r, 4);
e.fM.f === h.$uy.e && e.fF.forEach(function(t) {
t.dct = o.$jG.boj(t.dct + r, 4);
});
}
for (var b = n.bjq, a = n.bju, u = 0; u < b.length; u++) {
var f = b[u];
if (void 0 === f.tts) {
if (a[u] >= 1) continue;
} else if (0 === f.tts) ; else if (a[u] >= f.tts) continue;
for (var c = !0, s = 0; s < f.tgs.length; s++) {
var p = f.tgs[s], d = l.default.Di(t[0], null, p, null);
if (p[2] ? d : !d) {
c = !1;
break;
}
}
if (c) {
a[u] ? a[u] += 1 : a[u] = 1;
if (1 === l.default.Dj(t[0], null, f, null)) break;
}
}
};
return e;
}(b.$je.$jo);
n.$jR = f;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WW": void 0,
"../BattleBehaviorLogic": "BattleBehaviorLogic",
"../component/$MV": "$MV",
"../component/$NL": "$NL",
"../lib/$NN": "$NN",
"../math/$NP": "$NP"
} ],
$NY: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "11e79yFhy9Ps7Xi76E5e4qC", "$NY");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jS = void 0;
var o = t("../math/$NP"), b = t("../lib/$NN"), a = t("../$KC"), u = t("../component/$MW"), l = t("../component/$NL"), h = t("../component/$NM"), f = t("../../../game/src/battle/types/$WY"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bps = [];
return e;
}
e.prototype.filter = function() {
return b.$je.allOf(u.default, l.default, h.default);
};
e.prototype.update = function(t) {
for (var e = this.context.bnl.fi, n = 0, i = t; n < i.length; n++) {
var r = i[n], b = r.hE, u = r.hw, l = r.hy;
if (0 !== l.worldTimeScale) {
var h = o.$jG.boj(this.dt * l.worldTimeScale, 4), c = b.bjB, s = c.length;
if (s > 0) {
var p = this.bps;
p.length = s;
for (var d = 0; d < s; d++) {
(v = c[d]).DO = e;
p[d] = v;
}
for (d = 0; d < s; d++) {
var v;
if ((v = p[d]).DO === e) {
var $ = v.DF;
v.DL = v.DM;
v.DM = o.$jG.boj(h + v.DM, 4);
if ($.updateLg) {
var g = v.DK;
if (0 === g) a.default.Gg(r, v, $.updateLg, null); else {
v.DN = o.$jG.boj(h + v.DN, 4);
if (v.DN >= g) {
v.DN = o.$jG.boj(v.DN - g, 4);
a.default.Gg(r, v, $.updateLg, null);
}
}
}
$.abnormalType > f.$uD.$vp.j && a.default.Gn(r, v, h);
$.dot && a.default.Gp(r, v, h);
v.DO === e && 0 !== $.duration && v.DM >= v.DJ && a.default.Gj(r, v, !1);
}
}
if (r.hQ()) continue;
}
var y = o.$jG.boj(this.dt * l.bmE, 4);
b.bjK = b.bjL;
b.bjL = o.$jG.boj(b.bjL + y, 4);
if (b.bkk) {
var m = o.$jG.boj(y * l.actionTimeScale, 4);
b.bkg = o.$jG.boj(b.bkg + m, 4);
if (b.bkk.loop && b.bkg >= b.bkh.b) {
b.bkg = 0;
for (var j = 0, D = b.bkl; j < D.length; j++) (k = D[j]) && (k.d = 0);
b.bkm.b = 0;
}
for (var G = b.bkm, x = G.c, E = G.b + 1; x[E] && b.bkg >= x[E].region[0]; ) {
G.b += 1;
E = G.b + 1;
}
var M = x[G.b].anim, N = b.bkh;
N.f = M.x;
N.g = M.y;
N.h = M.a;
}
for (var _ = 0, w = b.bkl; _ < w.length; _++) {
var k;
if (k = w[_]) {
var O = this.context.bnu(k.f);
if (O) {
for (x = k.g, E = k.d + 1; x[E] && b.bkg >= x[E].region[0]; ) {
k.d += 1;
E = k.d + 1;
}
a.default.FK(O.hw, u);
var F = x[k.d].collider;
F && k.e ? a.default.Gt(O.hA, F) : O.hA.blb = 0;
}
}
}
if (b.bjX.hurtbackTime > 0) {
var R = b.bjX;
R.hurtbackTime = o.$jG.boj(R.hurtbackTime - y, 4);
if (R.hurtbackTime <= 0) {
R.hurtbackTime = 0;
switch (R.hurtbackType) {
case f.$uD.$vc.b:
a.default.FG(r.hz, 0, 0, 0);
a.default.EV(r);
break;

case f.$uD.$vc.c:
r.hz && a.default.FG(r.hz, 0, 0, 0);
break;

case f.$uD.$vc.e:
break;

case f.$uD.$vc.h:
case f.$uD.$vc.f:
a.default.Fw(r);
r.hU || a.default.Hg(r);
}
} else switch (R.hurtbackType) {
case f.$uD.$vc.b:
if (R.slideTime > 0) {
R.slideTime = o.$jG.boj(R.slideTime - y, 4);
if (R.slideTime <= 0) {
R.slideTime = 0;
a.default.FG(r.hz, 0, 0, 0);
}
}
break;

case f.$uD.$vc.e:
if (r.hw) {
a.default.Gq(r);
if (r.hw.z <= 0) {
r.hE.bke = 0;
if (r.hE.bkd) {
r.hE.bkc = 1;
r.hE.bkf = 1;
r.hz.blk = r.hE.bkd;
r.hE.bkd = 0;
} else if (r.hE.bkc > 1) {
a.default.FG(r.hz, 0, 0, 0);
a.default.Fu(r, f.$uD.LIE_TIME);
} else {
r.hE.bkc = 2;
r.hw.z = 1;
var P = -o.$jG.boj(.5 * r.hz.blk * r.ij(), 4);
r.hz.blk = P;
r.hz.bli = o.$jG.boj(.5 * r.hz.bli, 4);
}
}
}
}
}
for (var T = !1, I = 0, A = b.bjW; I < A.length; I++) {
var C = A[I];
C.time = o.$jG.boj(C.time + h);
if (-1 !== C.destroyTime && C.time > C.destroyTime) {
C.time = -1;
T = !0;
}
}
T && (b.bjW = b.bjW.filter(function(t) {
return -1 !== t.time;
}));
if (b.bjY.type > 0) {
var S = o.$jG.boj(h * b.bjY.speed, 4);
b.bjY.currentTime = o.$jG.boj(b.bjY.currentTime + S, 4);
}
a.default.Fk(r);
}
}
};
return e;
}(b.$je.$jo);
n.$jS = c;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../game/src/battle/types/$WY": void 0,
"../component/$MW": "$MW",
"../component/$NL": "$NL",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP"
} ],
$NZ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "eedaeXsvllPu4q22TlKmtCU", "$NZ");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jT = void 0;
var o = t("../$KC"), b = t("../math/$NP"), a = t("../lib/$NN"), u = t("../component/$MX"), l = t("../component/$NL"), h = t("../component/$NM"), f = t("../math/$NQ"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bpt = new f.default();
return e;
}
e.prototype.filter = function() {
return a.$je.allOf(u.default, h.default, l.default);
};
e.prototype.update = function(t) {
for (var e = 0, n = 0, i = t; n < i.length; n++) {
var r = i[n], a = r.hJ, u = this.context.bnu(a.bkG), l = r.hw;
if (u && u.hw) {
var h = u.hw;
switch (a.bkH) {
case 0:
var f, c = b.$jG.boj(a.bkI * h.bmF + h.x, 3), s = b.$jG.boj(a.bkJ + h.y, 3);
f = "number" == typeof a.bkM ? a.bkM : b.$jG.boj(a.bkK + h.z, 3);
e = a.bkL ? u.hw.bmF * a.bkL : 1;
o.default.FJ(l, c, s, f, e, l.bmH);
break;

case 1:
var p = r.hy, d = b.$jG.boj(this.dt * p.worldTimeScale * p.hitPauseTimeScale * p.mainTimeScale, 4);
a.bkP = b.$jG.boj(a.bkP + b.$jG.boj(d * a.bkN));
var v = b.$jG.boj(a.bkP * Math.PI / 180);
l.x = b.$jG.boj(h.x + b.$jG.boj(Math.cos(v)) * a.bkO);
l.y = b.$jG.boj(h.y + b.$jG.boj(Math.sin(v)) * a.bkO);
}
}
}
};
return e;
}(a.$je.$jo);
n.$jT = c;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../component/$MX": "$MX",
"../component/$NL": "$NL",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ"
} ],
$OA: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "3fafchsnI1CRpsnrF50/gor", "$OA");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
var o = t("../lib/$NN"), b = t("../math/$NP"), a = t("../../../game/src/battle/types/$WY"), u = t("../component/$MZ"), l = t("../component/$NM"), h = t("../math/$NO"), f = t("../math/$NQ"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bpu = new f.default();
e.bpv = new f.default();
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return o.$je.allOf(l.default, u.default);
};
e.prototype.update = function(t) {
for (var e = this.bpu, n = this.bpv, i = 0, r = !1, o = 0, l = t; o < l.length; o++) {
var f = (q = l[o]).hA;
if (f.blb) {
r = f.bla && q.hw.bmF === a.$uD.$vj.c;
n.x = q.hw.x;
n.y = q.hw.y;
i = q.hw.z;
for (var c = f.bld, s = f.bkX.bkU, p = 0; p < s.length; p++) {
var d = (A = s[p]).shapeInfo, v = (C = A.world).bnQ;
if (d.t === a.$uD.$vm.c) {
var $ = d.ax, g = d.ay, y = d.w * c, m = d.h * c, j = C.bnU, D = j[0], G = j[1], x = j[2], E = j[3];
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
D.x = e.x - $ * y;
D.y = e.y - g * m;
G.x = e.x + (1 - $) * y;
G.y = D.y;
x.x = G.x;
x.y = e.y + (1 - g) * m;
E.x = D.x;
E.y = x.y;
if (r) {
C.bnT.x = -1 * C.bnT.x + 2 * n.x;
D.x = -1 * D.x + 2 * n.x;
G.x = -1 * G.x + 2 * n.x;
x.x = -1 * x.x + 2 * n.x;
E.x = -1 * E.x + 2 * n.x;
}
var M = (H = D.x) < (B = G.x) ? H : B, N = H < B ? B : H, _ = (L = D.y) < (Y = x.y) ? L : Y, w = L < Y ? Y : L;
v.x = M;
v.y = _;
v.width = N - M;
v.height = w - _;
} else if (d.t === a.$uD.$vm.b) {
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
r && (C.bnT.x = -1 * C.bnT.x + 2 * n.x);
C.bnS = d.r * c;
v.x = C.bnT.x - C.bnS;
v.y = C.bnT.y - C.bnS;
v.width = 2 * C.bnS;
v.height = 2 * C.bnS;
} else if (d.t === a.$uD.$vm.d) {
(Q = C.bnR) || (Q = C.bnR = new h.$jC());
var k = b.$jG.boj(d.r * Math.PI / 180), O = ($ = d.ax, g = d.ay, y = d.w * c, m = d.h * c, 
C.bnU);
D = O[0], G = O[1], x = O[2], E = O[3];
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
D.x = e.x - $ * y;
D.y = e.y - g * m;
G.x = e.x + (1 - $) * y;
G.y = D.y;
x.x = G.x;
x.y = e.y + (1 - g) * m;
E.x = D.x;
E.y = x.y;
if (r) {
C.bnT.x = -1 * C.bnT.x + 2 * n.x;
D.x = -D.x + 2 * n.x;
G.x = -G.x + 2 * n.x;
x.x = -x.x + 2 * n.x;
E.x = -E.x + 2 * n.x;
Q.bnB.x = -(y * (.5 - $) + e.x) + 2 * n.x;
k = -k;
} else Q.bnB.x = y * (.5 - $) + e.x;
Q.bnB.y = m * (.5 - g) + e.y;
b.$jG.boe(C.bnT.x, C.bnT.y, Q.bnB.x, Q.bnB.y, k, Q.bnB);
b.$jG.boe(C.bnT.x, C.bnT.y, D.x, D.y, k, D);
b.$jG.boe(C.bnT.x, C.bnT.y, G.x, G.y, k, G);
b.$jG.boe(C.bnT.x, C.bnT.y, x.x, x.y, k, x);
b.$jG.boe(C.bnT.x, C.bnT.y, E.x, E.y, k, E);
var F = Math.min(D.x, G.x, x.x, E.x), R = Math.min(D.y, G.y, x.y, E.y), P = Math.max(D.x, G.x, x.x, E.x), T = Math.max(D.y, G.y, x.y, E.y);
v.x = F;
v.y = R;
v.width = P - F;
v.height = T - R;
Q.bnJ(Q.bnB.x, Q.bnB.y, y, m, k);
}
}
n.y = i;
var I = f.bkY.bkU;
for (p = 0; p < I.length; p++) {
var A, C;
d = (A = I[p]).shapeInfo, v = (C = A.world).bnQ;
if (d.t === a.$uD.$vm.c) {
$ = d.ax, g = d.ay, y = d.w * c, m = d.h * c;
var S = C.bnU;
D = S[0], G = S[1], x = S[2], E = S[3];
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
D.x = e.x - $ * y;
D.y = e.y - g * m;
G.x = e.x + (1 - $) * y;
G.y = D.y;
x.x = G.x;
x.y = e.y + (1 - g) * m;
E.x = D.x;
E.y = x.y;
if (r) {
C.bnT.x = -1 * C.bnT.x + 2 * n.x;
D.x = -1 * D.x + 2 * n.x;
G.x = -1 * G.x + 2 * n.x;
x.x = -1 * x.x + 2 * n.x;
E.x = -1 * E.x + 2 * n.x;
}
var H, B, L, Y, W = (H = D.x) < (B = G.x) ? H : B, z = H < B ? B : H, X = (L = D.y) < (Y = x.y) ? L : Y, K = L < Y ? Y : L;
v.x = W;
v.y = X;
v.width = z - W;
v.height = K - X;
} else if (d.t === a.$uD.$vm.b) {
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
r && (C.bnT.x = -1 * C.bnT.x + 2 * n.x);
C.bnS = d.r * c;
v.x = C.bnT.x - C.bnS;
v.y = C.bnT.y - C.bnS;
v.width = 2 * C.bnS;
v.height = 2 * C.bnS;
} else if (d.t === a.$uD.$vm.d) {
var Q;
(Q = C.bnR) || (Q = C.bnR = new h.$jC());
k = b.$jG.boj(d.r * Math.PI / 180), $ = d.ax, g = d.ay, y = d.w * c, m = d.h * c;
var V = C.bnU;
D = V[0], G = V[1], x = V[2], E = V[3];
e.x = n.x + d.x;
e.y = n.y + d.y;
C.bnT.bow(e.x, e.y);
D.x = e.x - $ * y;
D.y = e.y - g * m;
G.x = e.x + (1 - $) * y;
G.y = D.y;
x.x = G.x;
x.y = e.y + (1 - g) * m;
E.x = D.x;
E.y = x.y;
if (r) {
C.bnT.x = -1 * C.bnT.x + 2 * n.x;
D.x = -D.x + 2 * n.x;
G.x = -G.x + 2 * n.x;
x.x = -x.x + 2 * n.x;
E.x = -E.x + 2 * n.x;
Q.bnB.x = -(y * (.5 - $) + e.x) + 2 * n.x;
k = -k;
} else Q.bnB.x = y * (.5 - $) + e.x;
Q.bnB.y = m * (.5 - g) + e.y;
b.$jG.boe(C.bnT.x, C.bnT.y, Q.bnB.x, Q.bnB.y, k, Q.bnB);
b.$jG.boe(C.bnT.x, C.bnT.y, D.x, D.y, k, D);
b.$jG.boe(C.bnT.x, C.bnT.y, G.x, G.y, k, G);
b.$jG.boe(C.bnT.x, C.bnT.y, x.x, x.y, k, x);
b.$jG.boe(C.bnT.x, C.bnT.y, E.x, E.y, k, E);
F = Math.min(D.x, G.x, x.x, E.x), R = Math.min(D.y, G.y, x.y, E.y), P = Math.max(D.x, G.x, x.x, E.x), 
T = Math.max(D.y, G.y, x.y, E.y);
v.x = F;
v.y = R;
v.width = P - F;
v.height = T - R;
Q.bnJ(Q.bnB.x, Q.bnB.y, y, m, k);
}
}
}
}
for (var Z = 0, J = t; Z < J.length; Z++) {
var q, U = (q = J[Z]).hA;
if (U.blb) {
U.bkZ.length = 0;
for (var tt = 0, et = t; tt < et.length; tt++) {
var nt = et[tt], it = nt.hA;
if (it.blb && nt !== q && U.blc !== it.blc) {
var rt = !1, ot = (s = U.bkX.bkU, it.bkX.bkU);
for (p = 0; p < s.length; p++) {
var bt = (yt = s[p]).shapeInfo, at = yt.world;
if (!bt.neg) {
for (var ut = 0; ut < ot.length; ut++) {
var lt = (mt = ot[ut]).shapeInfo, ht = mt.world;
if (!lt.neg && h.$jF.bob(at.bnQ, ht.bnQ)) {
var ft = bt.t === a.$uD.$vm.c, ct = lt.t === a.$uD.$vm.c, st = bt.t === a.$uD.$vm.b, pt = lt.t === a.$uD.$vm.b, dt = bt.t === a.$uD.$vm.d, vt = lt.t === a.$uD.$vm.d;
ft && ct ? rt = !0 : st && pt ? rt = u.circleCircle(at, ht) : ft && pt ? rt = u.rectCircle(at, ht) : st && ct ? rt = u.rectCircle(ht, at) : dt && ct ? rt = h.$jF.bnY(at.bnR, ht.bnQ) : dt && pt ? rt = h.$jF.bnX(at, ht, at.bnR.bnC, at.bnR.bnD, at.bnR.bnE) : dt && vt ? rt = h.$jF.bnV(at.bnR, ht.bnR) : ft && vt ? rt = h.$jF.bnY(ht.bnR, at.bnQ) : st && vt && (rt = h.$jF.bnX(ht, at, ht.bnR.bnC, ht.bnR.bnD, ht.bnR.bnE));
if (rt) break;
}
}
if (rt) break;
}
}
if (rt && !(U.bkX.bkV && u.IsCollider1InsideCollider2(it, U) || it.bkX.bkV && u.IsCollider1InsideCollider2(U, it))) {
var $t = !1, gt = (I = U.bkY.bkU, it.bkY.bkU);
for (p = 0; p < I.length; p++) {
var yt;
for (bt = (yt = I[p]).shapeInfo, at = yt.world, ut = 0; ut < gt.length; ut++) {
var mt;
lt = (mt = gt[ut]).shapeInfo, ht = mt.world;
if (h.$jF.bob(at.bnQ, ht.bnQ)) {
ft = bt.t === a.$uD.$vm.c, ct = lt.t === a.$uD.$vm.c, st = bt.t === a.$uD.$vm.b, 
pt = lt.t === a.$uD.$vm.b, dt = bt.t === a.$uD.$vm.d, vt = lt.t === a.$uD.$vm.d;
ft && ct ? $t = !0 : st && pt ? $t = u.circleCircle(at, ht) : ft && pt ? $t = u.rectCircle(at, ht) : st && ct ? $t = u.rectCircle(ht, at) : dt && ct ? $t = h.$jF.bnY(at.bnR, ht.bnQ) : dt && pt ? $t = h.$jF.bnX(at, ht, at.bnR.bnC, at.bnR.bnD, at.bnR.bnE) : dt && vt ? $t = h.$jF.bnV(at.bnR, ht.bnR) : ft && vt ? $t = h.$jF.bnY(ht.bnR, at.bnQ) : st && vt && ($t = h.$jF.bnX(ht, at, ht.bnR.bnC, ht.bnR.bnD, ht.bnR.bnE));
if ($t) break;
}
}
if ($t) break;
}
$t && U.bkZ.push(nt.hg);
}
}
}
}
}
};
return e;
}(o.$je.$jo);
n.default = c;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$WY": void 0,
"../component/$MZ": "$MZ",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NO": "$NO",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ"
} ],
$OB: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "fa3caM9OflApZ21k7DQ91dr", "$OB");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jV = void 0;
var o = t("../component/$NB"), b = t("../lib/$NN"), a = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return b.$je.allOf(o.$iS);
};
e.prototype.update = function(t) {
for (var e = 0, n = t; e < n.length; e++) {
var i = n[e];
this.context.bnp(i);
}
};
return e;
}(b.$je.$jo);
n.$jV = a;
cc._RF.pop();
}, {
"../component/$NB": "$NB",
"../lib/$NN": "$NN"
} ],
$OC: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "8635dIz22BCKZ8Wddzxv8Wf", "$OC");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jW = void 0;
var o = t("../$KC"), b = t("../math/$NP"), a = t("../lib/$NN"), u = t("../component/$ND"), l = t("../component/$NL"), h = t("../component/$NM"), f = t("../component/$MR"), c = t("../component/$MS"), s = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return a.$je.allOf(h.default, l.default, u.default);
};
e.prototype.update = function(t) {
var e, n, i, r, a, u, l = this.dt;
this.bpw(this.context.bnl, l);
for (var h, f = 0, c = 0, s = 0, p = 0, d = 0, v = 0, $ = !0, g = 0, y = !1, m = !1, j = 0, D = t; j < D.length; j++) {
var G = D[j];
if (0 !== (h = b.$jG.boj(l * G.hy.worldTimeScale * G.hy.hitPauseTimeScale * G.hy.hTimeScale, 4))) {
a = G.hw;
u = G.hz;
var x = G.hE;
m = (null === (e = G.hv) || void 0 === e ? void 0 : e.bgK) > 0;
f = u.bli;
c = u.blj;
s = u.blk;
p = u.bll;
d = u.blm;
v = 0;
g = a.z;
if (y = x && x.bka && o.default.EW(G)) {
var E = null !== (n = x.bkb.z) && void 0 !== n ? n : 600, M = b.$jG.boj(x.bkb.height - g);
s = Math.abs(M) > 8 ? M > 0 ? E : -E : 0;
} else v = !a.bmJ && (null === (i = G.hv) || void 0 === i ? void 0 : i.bhj) && !(null == x ? void 0 : x.bkC) ? b.$jG.boj(u.bln + b.$jG.boj(-(null === (r = G.hv) || void 0 === r ? void 0 : r.bhj) * x.bkf), 3) : u.bln;
$ = G.ic;
if (0 !== f || 0 !== p) {
if (!m) {
u.blp > 10 && (p = -Math.sign(f) * u.blp);
a.x = b.$jG.boj(a.x + f * h + p * h * h / 2, 3);
}
if (p) {
var N = u.bli;
u.bli = b.$jG.boj(u.bli + p * h, 3);
Math.sign(u.bli) !== Math.sign(N) && u.blp > 10 && (u.bli = 0);
}
}
if (0 !== c || 0 !== d) {
m || (a.y = b.$jG.boj(a.y + c * h + d * h * h / 2, 3));
d && (u.blj = b.$jG.boj(u.blj + d * h, 3));
}
if (0 !== s || 0 !== v) {
h = b.$jG.boj(l * G.hy.worldTimeScale * G.hy.hitPauseTimeScale * G.hy.vTimeScale, 4);
a.z = b.$jG.boj(a.z + s * h + v * h * h / 2, 3);
v && (u.blk = b.$jG.boj(u.blk + v * h, 3));
}
o.default.FO(G, $);
if (y) {
x.bkb.height > g && x.bkb.height < a.z && (a.z = x.bkb.height);
x.bkb.height < g && x.bkb.height > a.z && (a.z = x.bkb.height);
}
}
}
};
e.prototype.bpw = function(t, e) {
for (var n = t.fr, i = 0, r = t.fC.bmW; i < r.length; i++) {
var o = r[i], a = o.ho(f.default);
if (a && n.bnu(o.hg) && o.hA && o.hA.blb) {
var u = n.bnu(a.biO);
if (u && u.hD) for (var l = u.hD.bkR, h = a.biP, s = u.hw, p = 0, d = o.hA.bkZ; p < d.length; p++) {
var v = d[p], $ = n.bnu(v);
if ($) {
var g = $.ho(c.default);
if (g) {
var y = n.bnu(g.biT);
if (y && y.hD && y.hD.bkR !== l) {
var m = y.hw;
if (m) {
var j = y.hy;
if (j) {
if (y.hv) {
var D = y.hv;
if (D.bgK > 0) continue;
if (y.hE) {
if (y.hE.bkp) continue;
if (y.hE.bkC) {
if (!a.biP.inGrab) continue;
if (y.hE.bjS !== a.biO) continue;
}
}
if (D && D.bgA) continue;
}
var G = b.$jG.boj(e * j.worldTimeScale * j.hitPauseTimeScale);
if (a.biQ) {
var x = b.$jG.boj(h.x * s.bmF + s.x, 3), E = b.$jG.boj(x - m.x), M = (_ = Math.sign(E)) * Math.min(_ * E, b.$jG.boj(a.biQ * G));
m.x = b.$jG.boj(m.x + M);
}
if (a.biR) {
var N = b.$jG.boj(h.y + s.y, 3);
E = b.$jG.boj(N - m.y), M = (_ = Math.sign(E)) * Math.min(_ * E, b.$jG.boj(a.biR * G));
m.y = b.$jG.boj(m.y + M);
}
if (a.biS) {
var _, w = b.$jG.boj(h.y + s.z, 3), k = (E = b.$jG.boj(w - m.z), M = (_ = Math.sign(E)) * Math.min(_ * E, b.$jG.boj(a.biS * G)), 
b.$jG.boj(m.z + M));
k < 0 && (k = 0);
m.z = k;
}
}
}
}
}
}
}
}
}
};
return e;
}(a.$je.$jo);
n.$jW = s;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../component/$MR": "$MR",
"../component/$MS": "$MS",
"../component/$ND": "$ND",
"../component/$NL": "$NL",
"../component/$NM": "$NM",
"../lib/$NN": "$NN",
"../math/$NP": "$NP"
} ],
$OD: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "4d80c2FLTlIrKiPFjyk3ofD", "$OD");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jX = void 0;
var o = t("../$KC"), b = t("../math/$NP"), a = t("../lib/$NN"), u = t("../component/$MM"), l = t("../component/$MP"), h = t("../component/$NH"), f = t("../math/$NQ"), c = t("../component/$NJ"), s = t("../component/$NF"), p = t("../component/$NI"), d = t("../utils/$OG"), v = t("../BattleBehaviorLogic"), $ = t("../../../main/src/$KM"), g = function(t, e) {
var n, i, r = t.c + (null !== (n = t.g.order) && void 0 !== n ? n : 0);
return e.c + (null !== (i = e.g.order) && void 0 !== i ? i : 0) - r;
}, y = function(t, e, n) {
return t >= n[0] && t < n[1] && e >= n[2] && e < n[3];
}, m = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bpx = new f.default();
e.bpy = new f.default();
return e;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return a.$je.allOf(s.default, u.default, l.default, h.default);
};
e.prototype.update = function(t) {
if (this.context.bnl.gs().bjr.canCtrl) for (var e = 0, n = t; e < n.length; e++) {
o = n[e];
this.bpz(o);
} else for (var i = 0, r = t; i < r.length; i++) {
var o;
(o = r[i]).hx.bmg && (o.hx.blW = 0);
}
};
e.prototype.bpz = function(t) {
var e, n, i, r, a, u, l, h, f, m, j = t.hv;
if (o.default.Gw(j)) {
var D = t.hx;
if (D.bmg) {
var G = t.hi.bnl, x = t.hi.bnw(c.default), E = t.hE;
if (E) {
E.bjJ;
var M = t.hw, N = t.ho(s.default), _ = t.hy, w = (b.$jG.boj(this.dt * _.worldTimeScale * _.hitPauseTimeScale * _.mainTimeScale, 4), 
this.context.bnu(t.hG.bis)), k = null !== (e = null == w ? void 0 : w.hU) && void 0 !== e && e, O = (t.hG, 
this.bpx);
O.x = M.x;
O.y = M.y;
var F = E.bjx, R = null, P = G.fI;
if (P.dcO) if (P.dcS && !P.dcU) {
w = this.context.bnu(P.dcT);
k = !0;
} else R = G.fE.bmW.find(function(t) {
return t.ho(p.default).bmj === P.ddb;
});
var T = N.blz;
if (E.bjF.ai && 0 === T.length) {
for (var I = 0, A = E.bjF.ai.acts; I < A.length; I++) {
var C = A[I];
(J = j.bgp(C.id)) && o.default.Gd(t, J.Dv.id) && T.push({
b: J.Dv.id,
f: J.Dv,
g: C,
c: J.Dv.upgradeLv,
d: 0,
h: 0,
i: 0
});
}
var S = j.bgd.find(function(t) {
return t.Dv.commonType === $.cvResUrl.commonSkillSquatCid;
});
S && T.push({
b: S.Dv.id,
f: S.Dv,
g: {
id: S.Dv.id,
order: 1e4,
checkDir: 0,
area: [ 0, 99999, 0, 99999 ]
},
c: S.Dv.upgradeLv,
d: 0,
h: 0,
i: 0
});
d.default.bpB(T, g, 0, T.length);
}
for (var H = new Set(), B = 0, L = E.bjA; B < L.length; B++) {
var Y = L[B];
if (Y.sid) {
if (!(J = j.bgp(Y.sid))) continue;
if (!o.default.Fz(t, J)) continue;
if (!o.default.FD(t, Y.tgs)) continue;
H.add(Y.sid);
} else if (Y.csid) {
if (!(J = j.bgq(Y.csid))) continue;
if (!o.default.Fz(t, J)) continue;
if (!o.default.FD(t, Y.tgs)) continue;
H.add(J.Dv.id);
}
}
if ((null == F ? void 0 : F.behit) && N.blA && (ht = o.default.Gb(t.hx, o.default.Gd(t, N.blA.f.id)))) {
ht.b = 0;
N.blA.h = 0;
N.blA.i = 0;
N.blA = null;
}
var W = 9999, z = 9999;
if (w && (null === (n = w.hE) || void 0 === n ? void 0 : n.bjF)) {
W = b.$jG.boj(w.hw.x + (null !== (r = null === (i = w.hE.bjF.aimAnchor) || void 0 === i ? void 0 : i.x) && void 0 !== r ? r : 0) - t.hw.x);
z = b.$jG.boj(w.hw.y + (null !== (u = null === (a = w.hE.bjF.aimAnchor) || void 0 === a ? void 0 : a.y) && void 0 !== u ? u : 0) - t.hw.y);
}
if (k && !D.blV) {
var X = N.blA;
if ((null == X ? void 0 : X.i) > 0) X.i = Math.max(b.$jG.boj(X.i - this.dt, 4), 0); else if ((null == X ? void 0 : X.h) > 0) X.h = Math.max(b.$jG.boj(X.h - this.dt, 4), 0); else if (!R) {
for (var K = !1, Q = [], V = 0, Z = T; V < Z.length; V++) {
var J, q = Z[V], U = q.g;
if ((J = j.bgp(q.b)) && o.default.Fz(t, J)) {
if (U.tgs) {
for (var tt = U.tgs, et = !0, nt = 0; nt < tt.length; nt++) {
var it = tt[nt], rt = v.default.Di(t, null, it, null);
if (it[2] ? rt : !rt) {
et = !1;
break;
}
}
if (!et) continue;
}
if (H.has(q.b)) {
Q.push(q);
var ot = U.area, bt = Math.abs(W);
if (y(bt, Math.abs(z), ot) && (!U.onlyElite || (null == w ? void 0 : w.hS))) {
if (0 !== U.checkDir && bt > 50 && -1 === Math.sign(W * t.hw.bmF)) {
if (!((null == F ? void 0 : F.changeDir) || F.move || F.idle)) continue;
t.hw.bmF *= -1;
}
if (N.blA && N.blA.b !== q.b) {
var at = o.default.Gb(t.hx, o.default.Gd(t, N.blA.f.id));
at && (at.b = 0);
}
N.blA = q;
K = !0;
N.blw = .2;
D.blV || (D.blW = 0);
var ut;
ut = J.Dv.commonType && J.Dv.cmdId ? J.Dv.cmdId : o.default.Gd(t, J.Dv.id);
if (ht = o.default.Gb(t.hx, ut)) {
ht.c = 1;
if (null == U ? void 0 : U.chargeTime) {
q.h = U.chargeTime;
ht.b = 1;
}
}
break;
}
}
}
}
if (!K) {
var lt = Q[0];
if (lt) {
if (N.blA && N.blA.b !== lt.sid) {
var ht;
(ht = o.default.Gb(t.hx, o.default.Gd(t, N.blA.f.id))) && (ht.b = 0);
}
N.blA = lt;
}
}
}
}
if (N.blw > 0) {
N.blw = b.$jG.boj(N.blw - this.dt, 4);
D.blV || (D.blW = 0);
N.blB = 0;
} else if (R) {
N.blB = .3;
N.blC[0] = 0;
N.blC[1] = 0;
N.blC[2] = 0;
N.blC[3] = 0;
} else if (k) if (N.blA) {
ot = N.blA.g.area;
N.blB = .25;
N.blC[0] = ot[0];
N.blC[1] = ot[1];
N.blC[2] = ot[2];
N.blC[3] = ot[3];
} else {
var ft = null === (h = null === (l = E.bjF) || void 0 === l ? void 0 : l.ai) || void 0 === h ? void 0 : h.move;
if (ft) {
N.blB = ft.time;
N.blC[0] = ft.param[0];
N.blC[1] = ft.param[1];
N.blC[2] = ft.param[2];
N.blC[3] = ft.param[3];
}
} else {
D.blV || (D.blW = 0);
N.blB = 0;
}
if (N.blB && !D.blV) {
N.blx = b.$jG.boj(this.dt + (N.blx || 0), 4);
if (N.blx >= N.blB) {
N.blx = 0;
var ct = N.blC, st = null == w ? void 0 : w.hw;
R && (st = R.hw);
var pt = this.bpy;
if (st) {
pt.x = st.x;
pt.y = st.y;
if (null === (m = null === (f = null == w ? void 0 : w.hE) || void 0 === f ? void 0 : f.bjF) || void 0 === m ? void 0 : m.aimAnchor) {
pt.x = b.$jG.boj(st.x + w.hE.bjF.aimAnchor.x);
pt.y = b.$jG.boj(st.y + w.hE.bjF.aimAnchor.y);
}
} else {
pt.x = b.$jG.boj(O.x + 800 * b.$jG.boj(G.ge() - .5));
pt.y = b.$jG.boj(O.y + 800 * b.$jG.boj(G.ge() - .5));
}
var dt = O.x - pt.x, vt = pt.x + (ct[0] + b.$jG.boj((ct[1] - ct[0]) * G.ge(), 3)) * Math.sign(dt);
vt = Math.max(Math.min(vt, x.bmv), x.bmu);
var $t = G.ge() > .5 ? 1 : -1, gt = pt.y + (ct[2] + b.$jG.boj((ct[3] - ct[2]) * G.ge(), 3)) * $t;
gt = Math.max(Math.min(gt, x.bmx), x.bmw);
N.blu.x = vt;
N.blu.y = gt;
}
var yt = N.blu.x - O.x, mt = N.blu.y - O.y, jt = 0, Dt = 0;
Math.abs(yt) > 30 && (jt = .6 * Math.sign(yt));
Math.abs(mt) > 30 && (Dt = .6 * Math.sign(mt));
if (0 === jt && 0 === Dt) D.blW = 0; else {
D.blW = 1;
D.blX.x = jt;
D.blX.y = Dt;
D.blY = 0;
}
}
}
}
}
};
return e;
}(a.$je.$jo);
n.$jX = m;
cc._RF.pop();
}, {
"../$KC": "$KC",
"../../../main/src/$KM": void 0,
"../BattleBehaviorLogic": "BattleBehaviorLogic",
"../component/$MM": "$MM",
"../component/$MP": "$MP",
"../component/$NF": "$NF",
"../component/$NH": "$NH",
"../component/$NI": "$NI",
"../component/$NJ": "$NJ",
"../lib/$NN": "$NN",
"../math/$NP": "$NP",
"../math/$NQ": "$NQ",
"../utils/$OG": "$OG"
} ],
$OE: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "b2e3ccGm15AIq9XN2elMQ6H", "$OE");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jY = void 0;
var o = t("../lib/$NN"), b = t("../component/$NH"), a = t("../component/$NL"), u = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return o.$je.allOf(b.default, a.default);
};
e.prototype.update = function(t) {
for (var e = 0, n = t; e < n.length; e++) {
var i = n[e];
i.hx.bma.forEach(function(t) {
t.c ? t.c = 0 : t.d = 0;
});
i.hx.bmb.forEach(function(t) {
t.c ? t.c = 0 : t.d = 0;
});
}
};
return e;
}(o.$je.$jo);
n.$jY = u;
cc._RF.pop();
}, {
"../component/$NH": "$NH",
"../component/$NL": "$NL",
"../lib/$NN": "$NN"
} ],
$OF: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ed669UyLh9HEZYhTjYceT5b", "$OF");
var i, r = this && this.__extends || (i = function(t, e) {
return (i = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
})(t, e);
}, function(t, e) {
i(t, e);
function n() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n());
});
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$jZ = void 0;
var o = t("../lib/$NN"), b = t("../component/$NJ"), a = t("../component/$NL"), u = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return o.$je.allOf(b.default);
};
e.prototype.update = function(t) {
for (var e = t[0].ho(b.default).bmy, n = e.length, i = this.context.bnw(a.default).worldTimeScale, r = n - 1; r > -1; r--) {
var o = e[r];
o.time += this.dt * i;
o.time > o.duration && e.splice(r, 1);
}
};
return e;
}(o.$je.$jo);
n.$jZ = u;
cc._RF.pop();
}, {
"../component/$NJ": "$NJ",
"../component/$NL": "$NL",
"../lib/$NN": "$NN"
} ],
$OG: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "699bdb+7wpCIKDm6kWKRwH/", "$OG");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = function() {
function t() {}
t.bpA = function(t) {
var e = Array.isArray(t) ? [] : {};
if (t && "object" == typeof t) for (var n in t) if (t.hasOwnProperty(n)) {
var i = t[n];
i instanceof Object ? e[n] = this.bpA(i) : e[n] = i;
}
return e;
};
t.bpB = function(t, e, n, i) {
for (var r = n + 1; r < i; r++) {
for (var o = t[r], b = r - 1; b >= n; b--) {
var a = t[b];
if (!(e(a, o) > 0)) break;
t[b + 1] = a;
}
t[b + 1] = o;
}
return t;
};
return t;
}();
n.default = i;
cc._RF.pop();
}, {} ],
$OH: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "80103XDbv1AO4HsZuC60nK+", "$OH");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$kb = void 0;
var i = function() {
function t(t) {
void 0 === t && (t = 0);
this.bpC = 0;
this.bpC = t;
}
t.prototype.bpD = function(t) {
return 0 != (this.bpC & t);
};
t.prototype.bpE = function(t) {
return (this.bpC & t) == t;
};
t.prototype.bpF = function(t) {
this.bpC |= t;
};
t.prototype.bpG = function(t) {
this.bpC &= ~t;
};
t.prototype.bpH = function() {
this.bpC = 0;
};
t.prototype.bpI = function(t) {
this.bpC = t;
};
t.prototype.bpJ = function() {
return this.bpC;
};
return t;
}();
n.$kb = i;
cc._RF.pop();
}, {} ],
BattleBehaviorLogic: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "02b39bTqzNBEKeSUGdGgGm1", "BattleBehaviorLogic");
Object.defineProperty(n, "__esModule", {
value: !0
});
var i = t("./$KC"), r = t("./math/$NP"), o = t("./component/$MP"), b = t("./component/$ND"), a = t("./component/$NJ"), u = t("./component/$MV"), l = t("./component/$NL"), h = t("../../game/src/battle/types/$WY"), f = t("./math/$NQ"), c = t("./$JZ"), s = t("./$KB"), p = t("../../game/src/battle/$QM"), d = t("./component/$NF"), v = t("./component/$NK"), $ = t("../../main/src/tbl/TblEnum"), g = h.$uD.$vv, y = h.$uD.$vl, m = function() {
function t() {}
t.Di = function(t, e, n, o) {
var b, l, f, s, p, v, y;
switch (n[0]) {
case g.b:
var m = t.hE;
return "number" == typeof (I = n[1])[1] ? m.bjL >= I[0] && m.bjL <= I[1] || m.bjL > I[1] && m.bjK < I[0] : m.bjK < I[0] && m.bjL >= I[0];

case g.c:
return !(!(rt = t.hx) || !rt.bmg) && (1 === (null == (at = i.default.Gb(rt, n[1][0])) ? void 0 : at.c) || (!(!n[1][2] || !at.b) || !!n[1][1] && r.$jG.boj(t.hi.bnl.fq - at.f) < n[1][1]));

case g.e:
var j = t.hz;
if (j) {
var D = t.hi.bnw(a.default), G = !1, x = n[1] && n[1][0] || 0;
0 !== j.bli && (G = j.bli > 0 ? t.hw.x >= D.bmv + x : t.hw.x <= D.bmu - x);
return !!G || 0 !== j.blj && (j.blj > 0 ? t.hw.y >= D.bmx + x : t.hw.y <= D.bmw - x);
}
return !1;

case g.f:
return t.hw.z <= 0;

case g.g:
for (var E = 0, M = (m = t.hE).bkl; E < M.length; E++) {
var N = M[E];
if (2 === N.b.type) {
var _ = t.hi.bnu(N.f);
if (_ && _.hB.biF > 0) return !0;
}
}
return !1;

case g.h:
m = t.hE;
var w = n[1];
return m.bjR.length >= w.count;

case g.i:
return !(!(rt = t.hx) || !rt.bmg) && (1 === rt.blW && (!(Math.abs(rt.blX.x) < .32) && Math.sign(t.hw.bmF * rt.blX.x) === n[1].dir));

case g.j:
return !(!t.hx || !t.hx.bmg) && 1 === t.hx.blW;

case g.m:
return !(!t.hx || !t.hx.bmg) && i.default.Gb(t.hx, n[1][0]).b;

case g.n:
return !!t.hG && t.hG.biv < n[1][0];

case g.o:
for (var k = n[1], O = k.id, F = k.count, R = 0, P = 0, T = t.hE.bjQ; P < T.length; P++) T[P].id === O && R++;
return R >= F;

case g.p:
if (t.hw) {
D = t.hi.bnw(a.default);
return t.hw.y !== D.bmw;
}
return !1;

case g.q:
return !!t.hE && -1 !== t.hE.bjS;

case g.r:
if (!t.hx || !t.hx.bmg) return !1;
if (1 !== t.hx.blW) return !1;
if (!t.hw) return !1;
var I = n[1], A = t.hx.blZ;
return I[0] > I[1] ? A > 180 ? A > I[0] : A <= I[1] : t.hx.blZ > I[0] && t.hx.blZ <= I[1];

case g.s:
m = t.hE;
var C = n[1][0];
return !!m && m.bjX[C] === n[1][1];

case g.t:
var S = t;
n[1][2] && (S = t.hi.bnu(t.hE.bjO));
return !!(m = S.hE).bjx && (void 0 === (it = n[1][1]) ? !!m.bjx[n[1][0]] : m.bjx[n[1][0]] === it);

case g.u:
var H = t.hG;
if (!H) return !1;
var B = t.hi.bnu(H.bis);
if (!B) return !1;
if (!(m = B.hE)) return !1;
switch (n[1][0]) {
case 99:
return m.bjx && m.bjx.behit;

case 98:
default:
return !1;
}

case g.v:
return !1;

case g.w:
return t.hU;

case g.x:
I = n[1];
return i.default.GT(t, I, e, o);

case g.y:
var L = null === (b = t.hv) || void 0 === b ? void 0 : b.bga;
if (0 === L.bpC) return !1;
if (it = n[1]) return L.bpD(1 << it - 1);
var Y = (1 << h.$uD.$vp.j - 1) - 1;
return (L.bpC & Y) > 0;

case g.A:
if (!(W = null == o ? void 0 : o.target)) return !1;
I = n[1];
return i.default.GT(W, I, e, o);

case g.z:
if (null == o || !o.target) return !1;
w = n[1];
var W = o.target;
switch (w[0]) {
case "dis<":
return i.default.Gz(W.hw, t.hw) < c.default.Dl(w[1], t, null);
}

case g.B:
var z = n[1];
return (null === (l = t.hE) || void 0 === l ? void 0 : l.bjC[z]) > 0;

case g.C:
if (!(null == (W = null == o ? void 0 : o.target) ? void 0 : W.hE)) return !1;
z = n[1];
return (null == W ? void 0 : W.hE.bjC[z]) > 0;

case g.D:
if (!e) return !1;
I = n[1];
var X = e.DM;
return "number" == typeof I[1] ? X >= I[0] && X <= I[1] : e.DL < I[0] && X >= I[0];

case g.E:
return (null === (f = t.hI) || void 0 === f ? void 0 : f.bhS) > 0 || !!t.ho(d.default);

case g.F:
return -1 !== (w = n[1]).indexOf(t.hv.bge);

case g.G:
return t.hi.bnl.gM;

case g.H:
var K = n[1], Q = t.hD.bkR;
return t.hi.bnl.fu.bmW.filter(function(e) {
return e.hg !== t.hg && e.hg > 0 && e.hv.bgI && Q === e.hD.bkR && (!K.roleCfgId || e.hv.bfZ.id === K.roleCfgId);
}).length >= K.count;

case g.I:
w = n[1];
var V = t.hv;
if (!V) {
var Z = t.hi.bnu(t.hE.bjO);
Z && (V = Z.hv);
}
return !!V && !!(ht = V.bgp(w.id)) && ht.Dt >= w.level;

case g.J:
W = void 0;
if ((w = n[1]).find && (W = i.default.EJ(t, w.find, o, e)) && (null == W ? void 0 : W.hw)) {
var J = t.hw.x, q = W.hw.x, U = t.hw.bmF, tt = W.hw.bmF;
switch (w.dirType) {
case 1:
return tt == h.$uD.$vj.b && J >= q || tt == h.$uD.$vj.c && J <= q;

case 2:
return tt == h.$uD.$vj.b && J <= q || tt == h.$uD.$vj.c && J >= q;

case 3:
return tt == h.$uD.$vj.b && U == h.$uD.$vj.c && J >= q || tt == h.$uD.$vj.c && U == h.$uD.$vj.b && J <= q;

case 4:
return tt == h.$uD.$vj.b && U == h.$uD.$vj.c && J <= q || tt == h.$uD.$vj.c && U == h.$uD.$vj.b && J >= q;
}
}
return !1;

case g.K:
w = n[1], W = void 0;
var et = o;
if ("boolean" != typeof et.isPhysical) return !1;
if (w.damageType) {
var nt = et.isPhysical ? 1 : 2;
if (w.damageType !== nt) return !1;
}
return !w.elementValue || 0 != (w.elementValue & et.elementValue);

case g.L:
var it;
w = n[1];
return (it = Math.floor(r.$jG.boj(1e4 * t.hi.bnl.ge()))) < c.default.Dl(w, t, e);

case g.M:
var rt;
if (!(rt = t.hx) || !rt.bmg) return !1;
if (n[1][3]) {
for (var ot = 0, bt = rt.bmc; ot < bt.length; ot++) if (bt[ot].d === n[1][0]) return !0;
return !1;
}
var at, ut = i.default.Gd(t, n[1][0]);
if (!ut) return !1;
if (1 === (at = i.default.Gb(rt, ut)).c) return !0;
if (n[1][2] && at.b) return !0;
var lt = null !== (s = n[1][1]) && void 0 !== s ? s : .25;
return !!lt && r.$jG.boj(t.hi.bnl.fq - at.f) < lt;

case g.N:
var ht;
w = n[1];
if (!(ht = o)) return !1;
if (w.levelRange) {
var ft = ht.Dv.upgradeLv;
if (ft < w.levelRange[0] || ft > w.levelRange[1]) return !1;
}
return !0;

case g.O:
return !(w = n[1]).progress || r.$jG.boj(t.hE.bjY.currentTime / t.hE.bjY.time) >= w.progress;

case g.P:
w = n[1];
return t.hE.bkk.actId === w[0];

case g.Q:
return "boolean" != typeof (w = n[1]).isAbyss || (null === (v = null === (p = t.hi.bnl.fj) || void 0 === p ? void 0 : p.svrInstanceParams) || void 0 === v ? void 0 : v.playMode) === $.TblEnum.InstancePlayModeTableEnum.ABYSS;

case g.X:
if (m = t.hI) return "number" == typeof (I = n[1])[1] ? m.bhY >= I[0] && m.bhY <= I[1] || m.bhY >= I[1] && m.bhX <= I[0] : m.bhX <= I[0] && m.bhY >= I[0];

case g.T:
I = n[1];
var ct = t.hi.bnw(u.default);
return "number" == typeof I[1] ? ct.bjt >= I[0] && ct.bjt <= I[1] || ct.bjt >= I[1] && ct.bjs <= I[0] : ct.bjs <= I[0] && ct.bjt >= I[0];

case g.R:
for (var st = null, pt = 0, dt = t.hi.bnl.fu.bmW; pt < dt.length; pt++) {
var vt = dt[pt];
if (i.default.Gw(vt.hv)) if (null === st) st = vt.hD.bkR; else if (st !== vt.hD.bkR) return !1;
}
return !0;

case g.S:
var $t = t.hi.bnl;
return !!$t.fn && $t.fk >= $t.fn;

case g.U:
var gt = t.hH, yt = (I = n[1], gt && gt.bjv);
switch (I[0]) {
case "==":
return (yt[I[1]] || 0) === I[2];

case ">":
return (yt[I[1]] || 0) > I[2];

case "<":
return (yt[I[1]] || 0) < I[2];

case ">=":
return (yt[I[1]] || 0) >= I[2];

case "<=":
return (yt[I[1]] || 0) <= I[2];

case "player1MPIsMax":
return !1;
}
return !1;

case g.V:
return null === (y = t.hi.bnl.fI) || void 0 === y ? void 0 : y.dcO;

case g.W:
for (var mt = 0, jt = t.hi.bnl.fu.bmW; mt < jt.length; mt++) if ((vt = jt[mt]).hv.bgI && 2 === vt.hD.bkR) return !1;
return !0;

case g.Y:
return t.hE.bkc >= 2;
}
return !1;
};
t.Dj = function(e, n, d, $) {
var g, m, j, D, G, x, E, M, N, _, w, k, O, F, R, P, T, I, A, C, S, H, B, L, Y, W, z, X, K, Q, V, Z, J, q, U, tt, et, nt, it, rt, ot, bt, at, ut, lt, ht, ft, ct, st, pt, dt, vt, $t;
switch (d.t) {
case y.b:
if (null === (m = e.hv) || void 0 === m ? void 0 : m.bgf) return 0;
i.default.EY(e, d.p[0]);
return 1;

case y.c:
e.hi.bnl.gu(h.$uD.$vA.F, e, d.p);
break;

case y.d:
d.p[2] ? n.DG[0] && e.hi.bnl.gu(h.$uD.$vA.H, e, n.DG[0], d.p[1]) : e.hi.bnl.gu(h.$uD.$vA.H, e, d.p[0], d.p[1]);
break;

case y.e:
i.default.Fc(e, d.p);
break;

case y.f:
i.default.EX(e);
e.hP();
return 1;

case y.g:
break;

case y.h:
e.hi.bnl.gu(h.$uD.$vA.I, e, d.p[0]);
return 0;

case y.i:
e.hE.bjX.hurtbackTime = 0;
e._if();
i.default.Fv(e);
return 0;

case y.j:
if (e.hE.bjX.hurtbackTime > 0 && 1 === (null === (j = e.hE) || void 0 === j ? void 0 : j.bjX.actId)) {
var gt = e.hz;
gt.bli = 0;
gt.blj = 0;
return 0;
}
var yt = e.hi.bnw(a.default);
if (0 !== (jt = e.hz).bli) if (e.hw.x >= yt.bmv) {
var mt = i.default.FP(jt.bli, jt.blj, -1, 0);
jt.bli = mt.x;
jt.blj = mt.y;
e.hi.bnl.gu(h.$uD.$vA.J, e, 4);
} else if (e.hw.x <= yt.bmu) {
mt = i.default.FP(jt.bli, jt.blj, 1, 0);
jt.bli = mt.x;
jt.blj = mt.y;
e.hi.bnl.gu(h.$uD.$vA.J, e, 3);
}
if (0 !== jt.blj) if (e.hw.y >= yt.bmx) {
mt = i.default.FP(jt.bli, jt.blj, 0, -1);
jt.bli = mt.x;
jt.blj = mt.y;
e.hi.bnl.gu(h.$uD.$vA.J, e, 1);
} else if (e.hw.y <= yt.bmw) {
mt = i.default.FP(jt.bli, jt.blj, 0, 1);
jt.bli = mt.x;
jt.blj = mt.y;
e.hi.bnl.gu(h.$uD.$vA.J, e, 2);
}
break;

case y.bT:
var jt, Dt = d.p;
if (!(jt = e.hz)) return 0;
if (0 === jt.bli && 0 === jt.blj) return 0;
var Gt = "_rebEnt_" + (null !== (D = Dt.tag) && void 0 !== D ? D : "0"), xt = (di = e.hE.bjJ)[Gt] || 0;
if (null != Dt.count && xt >= Dt.count) return 0;
var Et = 0, Mt = 0;
if (Dt.find) {
if (!(hn = i.default.EJ(e, Dt.find, $, n)) || !hn.hw) return 0;
Et = e.hw.x - hn.hw.x;
Mt = e.hw.y - hn.hw.y;
} else {
yt = e.hi.bnw(a.default);
if (jt.bli > 0 && e.hw.x >= yt.bmv) {
Et = -1;
Mt = 0;
} else if (jt.bli < 0 && e.hw.x <= yt.bmu) {
Et = 1;
Mt = 0;
} else if (jt.blj > 0 && e.hw.y >= yt.bmx) {
Et = 0;
Mt = -1;
} else {
if (!(jt.blj < 0 && e.hw.y <= yt.bmw)) return 0;
Et = 0;
Mt = 1;
}
}
var Nt = Math.sqrt(Et * Et + Mt * Mt);
if (Nt <= 0) return 0;
var _t = Et / Nt, wt = Mt / Nt;
if (jt.bli * _t + jt.blj * wt >= 0) return 0;
var kt = (mt = i.default.FP(jt.bli, jt.blj, _t, wt)).x, Ot = mt.y, Ft = null !== (G = Dt.accel) && void 0 !== G ? G : 1;
if (1 !== Ft) {
kt = r.$jG.boj(kt * Ft, 3);
Ot = r.$jG.boj(Ot * Ft, 3);
}
jt.bli = kt;
jt.blj = Ot;
if (!1 !== Dt.syncAngle) {
e.hw.bmF = 1;
var Rt = r.$jG.boj(180 * Math.atan2(Ot, kt) / Math.PI, 4);
e.hw.bmG = Rt;
}
di[Gt] = xt + 1;
e.hi.bnl.gu(h.$uD.$vA.J, e, 0);
break;

case y.k:
e.hi.bnl.gu(h.$uD.$vA.K, e, d.p);
break;

case y.l:
var Pt = d.p, Tt = i.default.FV(e, Pt, $);
Pt.id && Pt.behaviorId && !Pt.ai && e.hE.bjQ.push({
id: Pt.id,
eid: Tt.hg
});
break;

case y.m:
for (var It = d.p, At = 0, Ct = e.hE.bjQ.filter(function(t) {
return t.id === It.id;
}); At < Ct.length; At++) {
var St = Ct[At];
(Ut = e.hi.bnu(St.eid)) && Ut.hx && (i.default.Gb(Ut.hx, It.cmd).c = 1);
}
break;

case y.n:
Pt = d.p;
var Ht = e.hz;
Ht.blp = 0;
if (void 0 !== Pt.x) {
var Bt = Pt.x;
Pt.xr && (Bt = r.$jG.boj(Bt + e.hi.bnl.ge() * Pt.xr, 4));
Ht.bli = Bt * e.hw.bmF;
}
if (void 0 !== Pt.y) {
Bt = Pt.y;
Pt.yr && (Bt = r.$jG.boj(Bt + e.hi.bnl.ge() * Pt.yr, 4));
Ht.blj = Bt;
}
if (void 0 !== Pt.z) {
Bt = Pt.z;
Pt.zr && (Bt = r.$jG.boj(Bt + e.hi.bnl.ge() * Pt.zr, 4));
Ht.blk = Bt;
}
if (void 0 !== Pt.ax) {
Bt = Pt.ax;
Ht.bll = Bt * e.hw.bmF;
}
if (void 0 !== Pt.ay) {
Bt = Pt.ay;
Ht.blm = Bt;
}
if (void 0 !== Pt.az) {
Bt = Pt.az;
Ht.bln = Bt;
}
if (void 0 !== Pt.xyAngle || void 0 !== Pt.xyAngleRandAdd) {
var Lt = null !== (x = Pt.speed) && void 0 !== x ? x : 0;
Pt.speedRandAdd && (Lt = r.$jG.boj(Lt + e.hi.bnl.ge() * Pt.speedRandAdd, 4));
var Yt = null !== (E = Pt.speedAcc) && void 0 !== E ? E : 0, Wt = Pt.xyAngle;
Pt.xyAngleRandAdd && (Wt = r.$jG.boj(Wt + e.hi.bnl.ge() * Pt.xyAngleRandAdd, 4));
Wt = r.$jG.boj(Wt * Math.PI / 180, 4);
var zt = r.$jG.boj(Math.cos(Wt)), Xt = r.$jG.boj(Math.sin(Wt));
Ht.bli = r.$jG.boj(Lt * zt * e.hw.bmF);
Ht.blj = r.$jG.boj(Lt * Xt);
Ht.bll = r.$jG.boj(Yt * zt * e.hw.bmF);
Ht.blm = r.$jG.boj(Yt * Xt);
}
void 0 !== Pt.gzs && (e.hE.bkf = Pt.gzs);
break;

case y.o:
if ("number" == typeof (Dt = d.p).probability && !(Dt.probability > e.hi.bnl.ge())) return 0;
i.default.Gh(e, d.p.id, Dt.param, e.hg, n);
break;

case y.p:
i.default.Gk(e, d.p.id);
break;

case y.q:
if (!(br = e).hv) break;
if (!(null === (M = e.hx) || void 0 === M ? void 0 : M.bmg)) break;
if (!(Vt = br.hx).blW) {
br.hz.bli = 0;
br.hz.blj = 0;
return;
}
var Kt = d.p && d.p[0];
Lt = Kt ? c.default.Dl(Kt, e, n) : null !== (_ = null === (N = br.hv) || void 0 === N ? void 0 : N.bgC) && void 0 !== _ ? _ : 0;
e.hE.bjx && i.default.EZ(e, e.hE.bjx);
var Qt = .32;
Math.abs(Vt.blX.x) > Qt ? br.hz.bli = Math.sign(Vt.blX.x) * Lt : br.hz.bli = 0;
Math.abs(Vt.blX.y) > Qt ? br.hz.blj = Math.floor(Math.sign(Vt.blX.y) * Lt * h.$uD.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y) : br.hz.blj = 0;
break;

case y.r:
e.hG && (e.hG.bir = Boolean(d.p));
break;

case y.s:
if (!(null === (w = e.hx) || void 0 === w ? void 0 : w.bmg)) break;
if (null === (k = e.hI) || void 0 === k ? void 0 : k.bhZ) break;
var Vt = e.hx;
if (!e.hw) break;
if (Vt.blW && Math.abs(Vt.blX.x) > .07) {
var Zt = Vt.blX.x > 0;
e.hw.bmF = Zt ? h.$uD.$vj.b : h.$uD.$vj.c;
}
break;

case y.t:
e.hi.bnl.gu(h.$uD.$vA.M, e, d.p);
break;

case y.bE:
e.hi.bnl.gu(h.$uD.$vA.N, e, d.p);
break;

case y.bF:
Bt = c.default.Dl(d.p.val, e, n), Dt = {
scale: d.p.scale,
val: Bt,
effId: d.p.effId,
x: d.p.x,
y: d.p.y
};
e.hi.bnl.gu(h.$uD.$vA.O, e, Dt);
break;

case y.u:
if (e.hw) {
Dt = d.p;
var Jt = e.hw;
Dt.x && (Jt.x = r.$jG.boj(Jt.x + Dt.x * Jt.bmF));
Dt.y && (Jt.y = r.$jG.boj(Jt.y + Dt.y));
Dt.z && (Jt.z = r.$jG.boj(Jt.z + Dt.z));
"number" == typeof Dt.dir && (Jt.bmF = Dt.dir);
Dt.varX && "number" == typeof e.hE.bjJ[Dt.varX] && (Jt.x = e.hE.bjJ[Dt.varX]);
Dt.varY && "number" == typeof e.hE.bjJ[Dt.varY] && (Jt.y = e.hE.bjJ[Dt.varY]);
e.ic && i.default.FO(e, !0);
}
return 0;

case y.v:
(Dn = e.hi.bnw(a.default)).bmy.push({
eid: e.hg,
duration: d.p[0],
time: 0
});
return 0;

case y.w:
var qt = e.hi.bnl;
d.p ? qt.gq(e) : qt.gr(e.hg);
return 0;

case y.x:
e.hi.bnl.gu(h.$uD.$vA.Q, e, d.p);
return 0;

case y.y:
e.hi.bnl.gu(h.$uD.$vA.R, e, d.p);
return 0;

case y.z:
i.default.Fo(e, d.p);
return 0;

case y.A:
if (e.hw) {
var Ut = null;
if (d.p.targetIdInVar) {
var te = null === (O = e.hE) || void 0 === O ? void 0 : O.bjJ[d.p.targetIdInVar];
Ut = e.hi.bnu(te);
} else if (d.p.child) {
for (var ee = 0, ne = e.hE.bjQ; ee < ne.length; ee++) if ((Qe = ne[ee]).id === d.p.child) {
Ut = e.hi.bnu(Qe.eid);
break;
}
} else e.hG && (Ut = e.hi.bnu(e.hG.bis));
if (Ut && Ut.hw) {
Jt = e.hw;
var ie = Ut.hw, re = null !== (F = d.p.x) && void 0 !== F ? F : 0, oe = null !== (R = d.p.y) && void 0 !== R ? R : 0;
if (d.p.neartest) {
0 == (zn = ie.x - Jt.x) && (zn = 1);
Jt.x = r.$jG.boj(ie.x - Math.sign(zn) * re);
Jt.y = r.$jG.boj(ie.y + oe);
Jt.bmF = Math.sign(zn);
} else {
Jt.x = r.$jG.boj(ie.x + r.$jG.boj(ie.bmF * re));
Jt.y = r.$jG.boj(ie.y + oe);
}
e.ic && i.default.FO(e, !0);
}
}
return 0;

case y.B:
if ((Dt = d.p).raduisChange) {
e.hJ && (e.hJ.bkO = r.$jG.boj(e.hJ.bkO + Dt.raduisChange));
return;
}
if (Dt.surroundSpeedChange) {
e.hJ && (e.hJ.bkN = r.$jG.boj(e.hJ.bkN + Dt.surroundSpeedChange));
return;
}
if (Dt.cancel) {
i.default.FX(e);
return;
}
Ut = void 0;
if (!(null == (Ut = Dt.find ? i.default.EJ(e, Dt.find, $, n) : e.hi.bnu(e.hE.bjP)) ? void 0 : Ut.hw)) return 0;
i.default.FW(e, Ut, Dt);
return 0;

case y.C:
return 0;

case y.D:
break;

case y.E:
Lt = d.p[0];
d.p[1] && (Lt += r.$jG.boj(e.hi.bnl.ge() * d.p[1], 3));
var be = e.hz;
be || (be = e.hn(b.default));
0 === be.blq && (be.blq = 1);
be.blq = Math.sign(be.blq) * (Math.abs(be.blq) + Math.abs(Lt));
break;

case y.F:
e.hi.bnl.gu(h.$uD.$vA.S, e);
return 0;

case y.G:
if (d.p) {
e.hG || i.default.Gv(e, !1);
e.hG.bir = !!d.p[0];
e.hG.bix = null !== (P = d.p[1]) && void 0 !== P ? P : 0;
} else e.hq(o.default);
break;

case y.H:
qt = e.hi.bnl;
if (!e.hD) return;
var ae = e.hD.bkR, ue = (Jt = e.hw, d.p.dis), le = (Bt = d.p.val, null === (T = d.p.sameCamp) || void 0 === T || T), he = null === (I = d.p.floatValue) || void 0 === I || I, fe = Math.floor(c.default.Dl(Bt, e, n));
if (ue) for (var ce = 0, se = qt.fu.bmW; ce < se.length; ce++) {
var pe = se[ce];
if (pe.hD.bkR === ae === le && i.default.Gw(pe.hv) && i.default.Gz(pe.hw, Jt) < ue) if (fe > 0) {
pe.hY(e, fe);
he && e.hi.bnl.gu(h.$uD.$vA.E, pe, -fe, !1);
} else if (fe < 0) {
var de = Math.abs(fe);
pe.hX(e, de);
!pe.hv.bgI && i.default.Hf(pe, 0, 0);
d.p.emitEvent && i.default.GQ(pe, null, h.$uD.$vx.d, null);
he && e.hi.bnl.gu(h.$uD.$vA.E, pe, -fe, !1);
}
} else if (null === (A = e.hv) || void 0 === A ? void 0 : A.bgI) if (fe > 0) {
e.hY(e, fe);
e.hi.bnl.gu(h.$uD.$vA.E, e, -fe, !1);
} else if (fe < 0) {
de = Math.abs(fe);
e.hX(e, de);
!e.hv.bgI && i.default.Hf(e, 0, 0);
d.p.emitEvent && i.default.GQ(e, null, h.$uD.$vx.d, null);
he && e.hi.bnl.gu(h.$uD.$vA.E, e, -fe, !1);
}
break;

case y.I:
i.default.Hi(e, d.p, n, $);
return 0;

case y.J:
var ve = e.hE, $e = (Pt = d.p)[2];
(di = ve.bjJ)[$e] || (di[$e] = 0);
var ge = Pt[0].map(function(t) {
return c.default.Dl(t, e, n);
}), ye = e.hi.bnl.gf(ge);
(0 !== ge[0] || 0 !== ye) && (di[$e] = Pt[1][ye] || 0);
return 0;

case y.K:
e.hi.bnl.gu(h.$uD.$vA.T, e, d.p);
return 0;

case y.L:
ve = e.hE;
var me = null;
if (null == $ ? void 0 : $.target) me = $.target; else {
var je = null;
if (e.hG && -1 !== e.hG.bis) je = e; else {
var De = e.hi.bnu(ve.bjO);
De && (je = De);
}
je && je.hG && (me = e.hi.bnu(je.hG.bis));
}
if (me && me.hw) {
Jt = e.hw, ie = me.hw;
var Ge = r.$jG.boj(Math.atan2(ie.y - Jt.y, ie.x - Jt.x), 4);
Wt = r.$jG.boj(180 * Ge / Math.PI, 3);
i.default.FL(Jt, Wt);
}
return 0;

case y.M:
var xe = e.hE.bjJ[d.p[0]] || d.p[1] || "1";
i.default.EY(e, xe);
return 0;

case y.N:
Ut = null;
if ((Dt = d.p).find) {
if (!(Ut = i.default.EJ(e, Dt.find, $, n))) return 0;
} else {
var Ee = $;
if (!Ee) return 0;
if (!Ee.target) return 0;
Ut = Ee.target;
}
if ("number" == typeof Dt.probability && !(Dt.probability > e.hi.bnl.ge())) return 0;
var Me = Dt.param;
if (Dt.srcParam) {
var Ne = Dt.srcParam, _e = Ne.length;
(Me = t.Dh).length = _e;
for (var we = 0; we < _e; we++) Me[we] = c.default.Dl(Ne[we], e, n);
}
i.default.Gh(Ut, Dt.id, Me, e.hg, n);
return 0;

case y.O:
e.hE.bjU = d.p;
if (d.p.bindChildId) {
for (we = (Ye = e.hE.bjQ).length - 1; we >= 0; we--) if (Ye[we].id === d.p.bindChildId) {
e.hE.bjT = Ye[we].eid;
break;
}
} else e.hE.bjT = -1;
i.default.Fr(e);
return 0;

case y.P:
var ke = d.p;
switch (ke.index) {
case 1:
var Oe = e.hD.bkR, Fe = e.hi.bnl.fu.bmW.filter(function(t) {
return t.hg > 0 && t.hv.bgI && Oe !== t.hD.bkR;
});
ke.var && (Fe = Fe.filter(function(t) {
return t.hE && !!t.hE.bjJ[ke.var];
}));
if (ke.excludeEid) {
var Re = e.hE.bjJ[ke.excludeEid];
Fe = Fe.filter(function(t) {
return t.hg !== Re;
});
}
ke.isPlayer && (Fe = Fe.filter(function(t) {
return !!t.hx.blT;
}));
ke.randomNum && (Ke = Fe.length) > 1 && (Fe = e.hi.bnl.gA(Fe, Math.min(ke.randomNum, Ke)));
for (var Pe = 0, Te = Fe; Pe < Te.length; Pe++) if ((qe = Te[Pe]).hg > -1 && i.default.Gw(qe.hv)) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: qe
});
ke.tlg && i.default.Gg(qe, n, ke.tlg, {
target: e
});
}
break;

case 2:
for (var Ie = e.hE.bjR, Ae = e.hi, Ce = 0, Se = Ie; Ce < Se.length; Ce++) {
var He = Se[Ce];
if (qe = Ae.bnu(He.eid)) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: qe
});
ke.tlg && i.default.Gg(qe, n, ke.tlg, {
target: e
});
}
}
break;

case 3:
ae = e.hD.bkR;
for (var Be = 0, Le = Fe = e.hi.bnl.fu.bmW; Be < Le.length; Be++) if ((qe = Le[Be]).hg > -1 && qe.hg !== e.hg && i.default.Gw(qe.hv) && ae === qe.hD.bkR) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: qe
});
ke.tlg && i.default.Gg(qe, n, ke.tlg, {
target: e
});
}
if (ke.includeSelf) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: e
});
ke.tlg && i.default.Gg(e, n, ke.tlg, {
target: e
});
}
break;

case 4:
break;

case 5:
var Ye;
for (we = (Ye = Array.from(e.hi.bnf.values())).length - 1; we >= 0; we--) {
var We = Ye[we];
if (We.hE && We.hE.bjJ[ke.var] && (Ut = e.hi.bnu(We.hg))) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: Ut
});
ke.tlg && i.default.Gg(Ut, n, ke.tlg, {
target: e
});
}
}
break;

case 6:
break;

case 7:
var ze = (Xe = e.hi).bnu(e.hE.bjP);
if (ze) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: ze
});
ke.tlg && i.default.Gg(ze, n, ke.tlg, {
target: e
});
}
break;

case 8:
break;

case 9:
var Xe = e.hi, Ke = 0;
for (Fe = [], we = e.hE.bjQ.length - 1; we >= 0; we--) {
var Qe;
if ((Qe = e.hE.bjQ[we]).id === ke.groupId && (Ut = Xe.bnu(Qe.eid))) {
Ke++;
Fe.push(Ut);
}
if (ke.num && Ke >= ke.num) break;
}
if (ke.randomNum) {
var Ve = Fe.length;
Ve > 1 && (Fe = e.hi.bnl.gA(Fe, Math.min(ke.randomNum, Ve)));
}
for (var Ze = 0, Je = Fe; Ze < Je.length; Ze++) {
var qe = Je[Ze];
ke.lg && i.default.Gg(e, n, ke.lg, {
target: qe
});
ke.tlg && i.default.Gg(qe, n, ke.tlg, {
target: e
});
}
break;

case 10:
Fe = e.hi.bnl.fu.bmW;
ke.isPlayer && (Fe = Fe.filter(function(t) {
return !!t.hx.blT;
}));
ke.var && (Fe = Fe.filter(function(t) {
return t.hE && !!t.hE.bjJ[ke.var];
}));
if (ke.excludeEid) {
var Ue = e.hE.bjJ[ke.excludeEid];
Fe = Fe.filter(function(t) {
return t.hg !== Ue;
});
}
ke.roleId && (Fe = Fe.filter(function(t) {
return t.hv && ke.roleId === t.hv.bfZ.id;
}));
ke.isAlive && (Fe = Fe.filter(function(t) {
return i.default.Gw(t.hv);
}));
if (ke.isOrigin) {
var tn = e.hg;
Fe = Fe.filter(function(t) {
return t.hE && t.hE.bjO === tn;
});
}
ke.randomNum && (Ke = Fe.length) > 1 && (Fe = e.hi.bnl.gA(Fe, Math.min(ke.randomNum, Ke)));
for (var en = 0, nn = Fe; en < nn.length; en++) if ((qe = nn[en]).hg > -1) {
ke.lg && i.default.Gg(e, n, ke.lg, {
target: qe
});
ke.tlg && i.default.Gg(qe, n, ke.tlg, {
target: e
});
}
}
return 0;

case y.Q:
if (!(ve = e.hE)) return 0;
for (var rn = 0, on = ve.bjB.slice(); rn < on.length; rn++) {
var bn = (Di = on[rn]).DF.abnormalType;
bn && bn <= h.$uD.$vp.j && i.default.Gj(e, Di, !1);
}
if (ve.bkC) {
i.default.Fp(e);
i.default.Fv(e);
e.hz && i.default.FG(e.hz, 0, 0, 0);
}
i.default.EV(e);
return 0;

case y.R:
var an = null !== (C = (Pt = d.p).worldX) && void 0 !== C ? C : 0, un = null !== (S = Pt.worldY) && void 0 !== S ? S : 0;
if (Pt.center) {
var ln = e.hi.bnw(a.default);
an = r.$jG.boj(ln.bmu + .5 * (ln.bmv - ln.bmu));
un = r.$jG.boj(ln.bmw + .5 * (ln.bmx - ln.bmw));
}
var hn = void 0;
if (Pt.find) {
if (!(hn = i.default.EJ(e, Pt.find, $, n))) {
Pt.noTargetMoveX && (e.hw.x = r.$jG.boj(e.hw.x + Pt.noTargetMoveX * e.hw.bmF));
return 0;
}
if (null == hn ? void 0 : hn.hw) {
an = null == hn ? void 0 : hn.hw.x;
un = null == hn ? void 0 : hn.hw.y;
Pt.x && (an = r.$jG.boj(an + Pt.x * hn.hw.bmF));
Pt.y && (un = r.$jG.boj(un + Pt.y));
}
}
Jt = e.hw;
if ("number" == typeof Pt.time) if (0 !== Pt.time) {
var fn = r.$jG.boj(an - e.hw.x), cn = r.$jG.boj(un - e.hw.y);
Pt.LimitDisX && Math.abs(fn) > Pt.LimitDisX && (fn = Math.sign(fn) * Pt.LimitDisX);
Pt.LimitDisY && Math.abs(cn) > Pt.LimitDisY && (cn = Math.sign(cn) * Pt.LimitDisY);
if (Math.abs(fn) > 5) {
e.hz.bli = r.$jG.boj(fn / Pt.time, 3);
Pt.changeDir && (e.hw.bmF = Math.sign(e.hz.bli) || 1);
} else e.hz.bli = 0;
Math.abs(cn) > 5 ? e.hz.blj = r.$jG.boj(cn / Pt.time, 3) : e.hz.blj = 0;
} else {
Jt.x = an;
Jt.y = un;
Pt.changeDir && (null == hn ? void 0 : hn.hw) && (hn.hw.x - Jt.x > 0 ? e.hw.bmF = 1 : e.hw.bmF = -1);
} else if (Pt.speed) {
fn = r.$jG.boj(an - e.hw.x), cn = r.$jG.boj(un - e.hw.y);
Math.abs(fn) < 5 && (fn = 0);
Math.abs(cn) < 5 && (cn = 0);
i.default.Ev.bow(fn, cn);
var sn = r.$jG.boi(i.default.Ev);
e.hz.bli = r.$jG.boj(sn.x * Pt.speed);
e.hz.blj = r.$jG.boj(sn.y * Pt.speed);
Pt.changeDir && (e.hw.bmF = Math.sign(e.hz.bli) || 1);
if (Pt.speedAcc) {
e.hz.bll = r.$jG.boj(sn.x * Pt.speedAcc);
e.hz.blm = r.$jG.boj(sn.y * Pt.speedAcc);
} else {
e.hz.bll = 0;
e.hz.blm = 0;
}
}
return 0;

case y.S:
0 === (Dt = d.p).id ? i.default.EV(e) : i.default.Fn(e, Dt.id);
return 0;

case y.T:
(null === (L = null === (B = null === (H = e.hE) || void 0 === H ? void 0 : H.bjF) || void 0 === B ? void 0 : B.heroNoramlSkillCmds) || void 0 === L ? void 0 : L.length) > 0 && i.default.Fm(e, null === (W = null === (Y = e.hE) || void 0 === Y ? void 0 : Y.bjF) || void 0 === W ? void 0 : W.heroNoramlSkillCmds);
return 0;

case y.U:
e.hi.bnl.gu(h.$uD.$vA.X, e, d.p);
return 0;

case y.V:
var pn = d.p;
if (0 === pn.d) {
var dn = e.hE.bjW.filter(function(t) {
return t.id !== pn.id;
});
e.hE.bjW = dn;
} else e.hE.bjW.find(function(t) {
return t.id === pn.id;
}) || e.hE.bjW.push({
id: pn.id,
time: 0,
destroyTime: pn.d,
vfxInfo: pn.vfx
});
return 0;

case y.W:
(null == $ ? void 0 : $.target) && i.default.Hi(null == $ ? void 0 : $.target, d.p, n, $);
return 0;

case y.X:
Dt = d.p;
e.hE.bka = Dt.height > 0;
e.hE.bkb = Dt;
return 0;

case y.Y:
i.default.FN(e, d.p);
return 0;

case y.Z:
var vn = d.p;
if (vn.modelCfgId) if (-1 === (gn = e.hF.bip.findIndex(function(t) {
return t.id === vn.id;
}))) e.hF.bip.push({
id: vn.id,
modelId: vn.modelCfgId
}); else {
var $n = e.hF.bip[gn];
$n.id = vn.id;
$n.modelId = vn.modelCfgId;
} else {
var gn;
-1 !== (gn = e.hF.bip.findIndex(function(t) {
return t.id === vn.id;
})) && e.hF.bip.splice(gn, 1);
}
e.hF.biq = e.hi.bnl.fi;
return 0;

case y.ba:
return 0;

case y.bb:
var yn = (Pt = d.p)[0];
-1 === yn && (yn = null === (X = null === (z = e.hv) || void 0 === z ? void 0 : z.bfZ) || void 0 === X ? void 0 : X.AI);
i.default.EL(e, yn);
return 0;

case y.bc:
var mn = d.p, jn = mn.remainingPercent;
mn.skId && (null == (Xi = e.hv.bgp(mn.skId)) ? void 0 : Xi.cd) > 0 && (Xi.cd = Math.max(0, r.$jG.boj(Xi.cd - r.$jG.boj(jn * Xi.cd))));
mn.lvRange && e.hv.bgd.forEach(function(t) {
t.cd > 0 && t.Dv && t.Dv.upgradeLv >= mn.lvRange[0] && t.Dv.upgradeLv <= mn.lvRange[1] && (t.cd = Math.max(0, r.$jG.boj(t.cd - r.$jG.boj(jn * t.cd))));
});
return 0;

case y.bd:
Pt = d.p;
var Dn = e.hi.bnw(a.default);
Array.isArray(Pt.layers) && (Dn.bmz = Pt.layers);
e.hi.bnl.gu(h.$uD.$vA.Z);
return 0;

case y.be:
d.p && i.default.FL(e.hw, d.p[0]);
return 0;

case y.bf:
var Gn = d.p;
if (Gn) {
Wt = i.default.Gy(e.hw.x, e.hw.y, Gn[0], Gn[1]);
i.default.FL(e.hw, Wt);
}
return 0;

case y.bg:
return 0;

case y.bh:
Dt = d.p;
e.hi.bnl.gu(h.$uD.$vA.ba, e, Dt);
return 0;

case y.bi:
Dt = d.p;
e.hi.bnl.gu(h.$uD.$vA.bo, e, Dt);
return 0;

case y.bj:
if (d.p) {
if (hn = i.default.EJ(e, d.p.find, $)) {
i.default.Gv(e, !0);
e.hG.bis = hn.hg;
e.hG.biw = h.$uD.$vh.c;
e.hG.biv = 9999;
}
} else e.hG && (e.hG.biw = h.$uD.$vh.b);
return 0;

case y.bk:
hn = e;
d.p && (hn = null == $ ? void 0 : $.target);
if (e.hx.bmh && hn) {
var xn = e.hi.bnl.gk(e.hx.blT);
xn && (xn.dcp = hn.hg);
}
return 0;

case y.bl:
Dt = d.p;
e.hi.bnl.gu(h.$uD.$vA.bp, e, Dt);
return 0;

case y.bm:
Dt = d.p;
return 0;

case y.ck:
i.default.EN(e, d.p[0]);
return 0;

case y.cl:
e.hx && (i.default.Gb(e.hx, d.p[0]).c = 1);
return 0;

case y.cm:
var En = e.hI, Mn = e.hG, Nn = e.hw;
if (!En) return 0;
if (!Mn) return 0;
var _n = e.hi.bnu(e.hG.bis);
if (!_n) return 0;
var wn = _n.hw;
if (!wn) return 0;
yt = e.hi.bnw(a.default);
var kn = d.p[0] || 650, On = d.p[1] || 90, Fn = d.p[2] || 0, Rn = Mn.biu.boD(new f.default()), Pn = r.$jG.boj((e.hi.bnl.ge() - .5) * On, 3), Tn = r.$jG.boj(kn * (e.hi.bnl.ge() - .5) + Fn, 3), In = r.$jG.boj(Pn * Math.PI / 180, 3), An = Rn.boy();
r.$jG.bog(An, In);
r.$jG.boh(An, Tn);
var Cn = r.$jG.boj(An.x + wn.x, 3), Sn = r.$jG.boj(An.y + wn.y, 3);
Cn = Math.max(Math.min(Cn, yt.bmv), yt.bmu);
Sn = Math.max(Math.min(Sn, yt.bmx), yt.bmw);
En.bhV.bow(Cn, Sn);
return 0;

case y.cn:
if (!(Vt = e.hx)) return 0;
if (!(En = e.hI)) return 0;
if (!(Nn = e.hw)) return 0;
var Hn = new f.default(Nn.x, Nn.y), Bn = En.bhV.boz(Hn, new f.default()), Ln = r.$jG.boi(Bn.boy());
if (Bn.boC() < 2500) Vt.blW = 0; else {
var Yn = Hn.bow(1, 0), Wn = r.$jG.boj(Math.atan2(Ln.y, Ln.x), 3);
if (1 === Vt.blW) {
var zn;
if ((zn = r.$jG.boj(Wn - Vt.blY)) > .06) {
var Xn = 2 * Math.PI;
(zn %= Xn) > Math.PI ? zn -= Xn : zn < -Math.PI && (zn += Xn);
zn > Math.PI / 3 && (Wn = r.$jG.boj(Vt.blY + .5 * zn, 3));
}
} else Vt.blW = 1;
r.$jG.bog(Yn, Wn);
Vt.blX.bow(Yn.x, Yn.y);
Vt.blY = Wn;
}
return 0;

case y.co:
if (Ii = e.hE) {
var Kn = Ii.bjJ[d.p[0]];
Kn && i.default.EN(e, Kn);
}
return 0;

case y.cp:
e.hx && i.default.GC(e.hx, d.p[0], d.p[1]);
return 0;

case y.cq:
if (e.hx) {
(Vt = e.hx).blW = d.p[0];
if (d.p[0] && e.hG) {
Vt.blX.box(e.hG.biu);
Vt.blY = r.$jG.boj(Math.atan2(Vt.blX.y, Vt.blX.x), 3);
}
}
return 0;

case y.bt:
var Qn = e.hi.bnw(u.default);
i.default.GB(Qn, d.p[0]);
return 1;

case y.bn:
Object.assign(e.hi.bnl.fT, d.p);
e.hi.bnl.gu(h.$uD.$vA.P, e, d.p);
return 0;

case y.bo:
e.hi.bnl.gu(h.$uD.$vA.U, d.p);
return 0;

case y.bu:
e.hi.bnl.gu(h.$uD.$vA.V, d.p);
return 0;

case y.bq:
(qt = e.hi.bnl).gr(0);
qt.go(r.$jG.boj(d.p[0], 3));
return 0;

case y.by:
var Vn = e.hi.bnl;
Vn.gr(0);
var Zn = [];
Vn.fF.forEach(function(t) {
if (t.dcD()) {
var e = Vn.gi(t.dco);
if (e && e.hv.bgI) for (var n = 0, i = e.hv.bgk; n < i.length; n++) {
var r = i[n];
Math.abs(r.frameId - Vn.fi) < 3 && Zn.push(t.dca);
}
}
});
if (Zn.length > 0) {
Vn.go(0);
e.hi.bnw(u.default).bjw(3, Zn);
} else {
Vn.go(0);
e.hi.bnw(u.default).bjw(.5);
}
e.hi.bnl.gu(h.$uD.$vA.j);
return 0;

case y.bY:
Fe = (qt = e.hi.bnl).fu.bmW;
var Jn = 0, qn = !1;
qt.fn > 0 && 0 === Math.max(qt.fn - qt.fk, 0) && (qn = !0);
for (var Un = 0, ti = Fe; Un < ti.length; Un++) {
var ei = ti[Un];
i.default.Gw(ei.hv) && ei.hD.bkS > 0 && Jn++;
}
qt.fI.dcO = 0 === Jn && !qn;
return 0;

case y.bZ:
var ni = e.hi.bnl, ii = ni.fI.dcX[0];
ii = {
winner: 0,
duration: ni.fk,
infos: []
};
ni.fI.dcX.push(ii);
ni.fF.forEach(function(t) {
var e = t.dcD();
if (e) {
var n = ni.gi(t.dco);
if (n) {
var i = e.hpRate;
e.hpRate = n.hv.bgD > 0 ? r.$jG.boj(n.hv.bgD / n.hv.bgE) : 0;
e.hp = n.hv.bgD;
ii.infos.push({
playerId: t.dca,
heroId: e.roleCfgId,
hp: [ i, e.hpRate ]
});
}
}
});
var ri = 0, oi = 0;
ni.fF.forEach(function(t) {
var e = t.dcD();
e && e.hp > oi && (oi = e.hp);
});
ni.fF.forEach(function(t) {
var e = t.dcD();
e && e.hp === oi && ri++;
});
ni.fF.forEach(function(t) {
var e = t.dcD();
if (e) if (ri > 1) ni.fI.dcQ = -1; else if (e.hp === oi) {
ni.fI.dcQ = t.dcf;
ii.winner = t.dca;
}
});
ni.fI.dcO = !0;
return 0;

case y.ca:
case y.cb:
return 0;

case y._cc:
i.default.GZ(e.hi.bnl, e.hi.bnl.fp);
return 0;

case y.cf:
return 0;

case y.cg:
i.default.GX(e.hi.bnl, e.hi.bnl.fp);
return 0;

case y.ch:
i.default.Ha(e.hi.bnl);
return 0;

case y.ci:
(qt = e.hi.bnl).gu(h.$uD.$vA.bw);
return 0;

case y.cd:
var bi = e.hi.bnl;
mt = bi.fI.dcX[0];
mt = {
winner: 0,
duration: bi.fk,
infos: []
};
bi.fI.dcX.push(mt);
bi.fF.forEach(function(t) {
t.dcs = 0;
t.dcB.forEach(function(e) {
var n = bi.gi(e.entityId);
if (n) {
e.hpRate = n.hv.bgD > 0 ? r.$jG.boj(n.hv.bgD / n.hv.bgE) : 0;
e.hp = n.hv.bgD;
t.dcs += Math.ceil(r.$jG.boj(100 * e.hpRate));
}
});
});
var ai = 0, ui = 0;
bi.fF.forEach(function(t) {
t.dcs > ui && (ui = t.dcs);
});
bi.fF.forEach(function(t) {
t.dcs === ui && ai++;
});
if (ai > 1) bi.fI.dcQ = -1; else {
var li = bi.fF.find(function(t) {
return t.dcs === ui;
});
bi.fI.dcQ = li.dcf;
mt.winner = li.dca;
}
bi.fI.dcO = !0;
return 0;

case y.ce:
qt = e.hi.bnl;
return 0;

case y.cj:
qt = e.hi.bnl;
i.default.Hm(qt, d.p);
return 0;

case y.br:
if ((qt = e.hi.bnl).fI.dcO) if (qt.gN) for (var hi = 0, fi = Fe = qt.fu.bmW; hi < fi.length; hi++) (qe = fi[hi]).hD.bkR === qt.fI.dcQ && i.default.Gw(qe.hv) && qe.hx.blT && qe.hE && qe.hE.bjF.winBehaviorId && i.default.EY(qe, qe.hE.bjF.winBehaviorId); else for (var ci = 0, si = Fe = qt.fu.bmW; ci < si.length; ci++) {
qe = si[ci];
i.default.Gw(qe.hv) && qe.hx.blT && qe.hE && qe.hE.bjF.winBehaviorId && i.default.EY(qe, qe.hE.bjF.winBehaviorId);
}
return 0;

case y.bp:
qt = e.hi.bnl;
e.hi.bnl.gz();
return 0;

case y.bs:
var pi = e.ie;
if (pi) {
pi.dcO || (pi.ddd = e.hi.bnl.fk);
e.hi.bnl.gu(h.$uD.$vA.k, pi.dcO);
}
return 0;

case y.bv:
ve = e.hi.bnw(u.default), Pt = d.p;
var di = ve.bjv;
switch (Pt[0]) {
case "=Random":
di[Pt[1]] = Math.floor(1e4 * e.hi.bnl.ge());
break;

case "+=dt":
var vi = e.hi.bnw(l.default), $i = r.$jG.boj(e.hi.bnl.fp * vi.worldTimeScale, 4);
di[Pt[1]] = r.$jG.boj((di[Pt[1]] || 0) + $i, 4);
break;

case "=":
di[Pt[1]] = Pt[2];
break;

case "+=":
di[Pt[1]] = (di[Pt[1]] || 0) + Pt[2];
break;

case "-=":
di[Pt[1]] = (di[Pt[1]] || 0) - Pt[2];
break;

case "*=":
di[Pt[1]] = Math.floor((di[Pt[1]] || 0) * Pt[2]);
break;

case "/=":
di[Pt[1]] = Math.floor((di[Pt[1]] || 0) / Pt[2]);
break;

case "+":
di[Pt[1]] = (di[Pt[2]] || 0) + (di[Pt[3]] || 0);
break;

case "-":
di[Pt[1]] = (di[Pt[2]] || 0) - (di[Pt[3]] || 0);
break;

case "*":
di[Pt[1]] = Math.floor((di[Pt[2]] || 0) * (di[Pt[3]] || 0));
break;

case "/":
di[Pt[1]] = Math.floor((di[Pt[2]] || 0) / (di[Pt[3]] || 0));
}
return 0;

case y.bw:
e.hi.bnl.gu(h.$uD.$vA.f, d.p);
return 0;

case y.bx:
return 0;

case y.bz:
for (var gi = 0, yi = Fe = (qt = e.hi.bnl).fu.bmW; gi < yi.length; gi++) (qe = yi[gi]).hx.bmh && i.default.Gw(qe.hv) && qe.hE && qe.hE.bjF.winBehaviorId && i.default.EY(qe, qe.hE.bjF.showBehaviorId);
return 0;

case y.bA:
for (var mi = 0, ji = e.hE.bjB.slice(); mi < ji.length; mi++) {
var Di;
(Di = ji[mi]).DF.abnormalType && i.default.Gj(e, Di, !1);
}
return 0;

case y.bB:
Dt = d.p;
var Gi = $ instanceof s.$gb.$ge ? $ : null, xi = (Ut = null !== (K = Dt.target && i.default.EJ(e, Dt.target, $, n)) && void 0 !== K ? K : null == $ ? void 0 : $.target, 
null !== (Q = Dt.form && i.default.EJ(e, Dt.form, $, n)) && void 0 !== Q ? Q : e);
qt = e.hi.bnl;
if (Dt.val) {
fe = c.default.Dl(Dt.val, e, n);
if ((null == xi ? void 0 : xi.hv) && (null === (V = null == Ut ? void 0 : Ut.hv) || void 0 === V ? void 0 : V.bgI)) {
var Ei = i.default.Ey(xi, xi, Ut);
Ei.isPhysical = xi.hv.bhl > xi.hv.bhm;
Ei.elementValue = null !== (Z = Dt.elementValue) && void 0 !== Z ? Z : 0;
var Mi = i.default.Gr(qt, Ei, fe / p.GameAttributeConstRatioRange, !0);
Ei.finalDamage = Mi;
Ut.hX(xi, Mi);
!Ut.hv.bgI && i.default.Hf(Ut, 0, 0);
qt.gu(h.$uD.$vA.E, Ut, Mi, Ei.isCrt);
i.default.Ez(Ei);
}
} else if (Dt.fixVal) {
fe = c.default.Dl(Dt.fixVal, e, n);
if (null === (J = null == Ut ? void 0 : Ut.hv) || void 0 === J ? void 0 : J.bgI) {
Mi = fe;
Ut.hX(xi, Mi);
!Ut.hv.bgI && i.default.Hf(Ut, 0, 0);
qt.gu(h.$uD.$vA.E, Ut, Mi, !1);
}
} else if (Dt.addVal && (null == xi ? void 0 : xi.hv) && (null === (q = null == Ut ? void 0 : Ut.hv) || void 0 === q ? void 0 : q.bgI)) {
fe = c.default.Dl(Dt.addVal, e, n);
var Ni = xi.hv, _i = Ut.hv, wi = null !== (U = Dt.elementValue) && void 0 !== U ? U : 0, ki = 1;
if (wi > 0) {
var Oi = p.default.bGR.bGP, Fi = Number.MIN_SAFE_INTEGER;
if (wi & h.$uD.$uY.b) {
Fi = Math.max(Fi, Ni.bhx - _i.bhB);
h.$uD.$uY.b;
}
if (wi & h.$uD.$uY.c) {
Fi = Math.max(Fi, Ni.bhy - _i.bhC);
h.$uD.$uY.c;
}
if (wi & h.$uD.$uY.d) {
Fi = Math.max(Fi, Ni.bhz - _i.bhD);
h.$uD.$uY.d;
}
if (wi & h.$uD.$uY.e) {
Fi = Math.max(Fi, Ni.bhA - _i.bhE);
h.$uD.$uY.e;
}
ki = Math.max(r.$jG.boj(1 + Oi.streArg1 + r.$jG.boj(Fi / Oi.streArg2)), 0);
}
Mi = Math.ceil(r.$jG.boj(n.DU * (fe / p.GameAttributeConstRatioRange) * ki));
Ut.hX(xi, Mi, Gi);
!Ut.hv.bgI && i.default.Hf(Ut, 0, 0);
qt.gu(h.$uD.$vA.E, Ut, Mi, !1);
}
return 0;

case y.bC:
Dt = d.p;
for (var Ri = 0, Pi = e.hE.bkl; Ri < Pi.length; Ri++) {
var Ti = Pi[Ri];
Ti.c === Dt.name && (Ti.e = Dt.enable ? 1 : 0);
}
return 0;

case y.bD:
e.hw.bmF *= -1;
return 0;

case y.bG:
return 0;

case y.bH:
n.DU = 1;
n.DV = null !== (tt = d.p.reflectW) && void 0 !== tt ? tt : 0;
n.DW = null !== (et = d.p.damageW) && void 0 !== et ? et : 0;
return 0;

case y.bI:
Dt = d.p;
var Ii = e.hE, Ai = e.hy, Ci = e.hv;
if (!Ii.bjF.changeGroup) return;
if (0 === Ai.mainTimeScale) return 0;
var Si = null;
if (Dt.consumeSid) {
Si = Ci.bgp(Dt.consumeSid);
if (!i.default.Fz(e, Si)) return 0;
}
var Hi = Ii.bjF.changeGroup[Dt.changesGroup];
if (!Hi) return 0;
for (we = 0; we < Hi.length; we++) {
for (var Bi = Hi[we], Li = !0, Yi = 0; Yi < Bi.tgs.length; Yi++) {
var Wi = Bi.tgs[Yi], zi = t.Di(e, null, Wi, null);
if (Wi[2] ? zi : !zi) {
Li = !1;
break;
}
}
if (Li && (Bi.t !== y.b || Bi.p[0] !== Ii.bjG)) {
if (Ci && (Bi.sid || Bi.csid)) {
if (0 === Ai.mainTimeScale) continue;
var Xi = null;
if (Bi.sid) {
var Ki = Bi.sid;
Xi = Ci.bgp(Ki);
} else Xi = Ci.bgd.find(function(t) {
return t.Dv.commonType === d.csid;
});
if (!Xi) continue;
if (!i.default.Fz(e, Xi)) continue;
i.default.FA(e, Xi);
if (e.hL) {
var Qi = e.hL.blQ[Xi.Dv.id];
Qi && (e.hx.bmi(Qi).f = -9999);
}
Si && i.default.FA(e, Si);
}
if (1 === (mt = t.Dj(e, null, Bi, null))) {
e.hi.bnl.gu(h.$uD.$vA.bn, e, h.$uD.spawnGhostParam);
return;
}
}
}
return 0;

case y.bJ:
Dt = d.p, ve = e.hE;
if (Dt.start) {
if (!(hn = null == $ ? void 0 : $.target)) return 0;
ve.bjJ.__PosToEnemyRule1_target = hn.hg;
ve.bjJ.__PosToEnemyRule1_record = ((g = {})[hn.hg] = 1, g);
return 0;
}
Fe = e.hi.bnl.fu.bmW;
for (var Vi = 9999, Zi = 0, Ji = null, qi = (ae = e.hD.bkR, e.hw), Ui = ve.bjJ.__PosToEnemyRule1_record || {}, tr = 0, er = Fe; tr < er.length; tr++) if ((qe = er[tr]).hg > -1 && qe.hv.bgI && ae !== qe.hD.bkR && !(null === (nt = qe.hE) || void 0 === nt ? void 0 : nt.bkE)) {
if ((ue = i.default.Gz(qe.hw, qi)) > Dt.range) continue;
if (ve.bjJ.__PosToEnemyRule1_target === qe.hg) continue;
Ui[qe.hg] = Ui[qe.hg] || 0;
var nr = Ui[qe.hg];
if (Vi > nr || Zi > ue && Vi === nr) {
Ji = qe;
Vi = nr;
Zi = ue;
}
}
Ji && (Ui[Ji.hg] += 1);
ve.bjJ.__PosToEnemyRule1_target = null !== (it = null == Ji ? void 0 : Ji.hg) && void 0 !== it ? it : -1;
return 0;

case y.bK:
e.hi.bnl.gu(h.$uD.$vA.bm, e, d.p);
return 0;

case y.bL:
if (!e.hD) return;
Bt = d.p.val, fe = Math.floor(c.default.Dl(Bt, e, n));
(null === (rt = e.hv) || void 0 === rt ? void 0 : rt.bgI) && e.hZ(e, fe);
break;

case y.bM:
if (0 === d.p.size) e.hq(v.default); else {
var ir = e.hn(v.default);
ir.bmB = null !== (ot = d.p.size) && void 0 !== ot ? ot : 100;
ir.bmC = null !== (bt = d.p.offsetX) && void 0 !== bt ? bt : 0;
}
break;

case y.bN:
e.hi.bnl.gu(h.$uD.$vA.bd, e, d.p);
break;

case y.bO:
an = null !== (ut = null === (at = e.hw) || void 0 === at ? void 0 : at.x) && void 0 !== ut ? ut : 0, 
un = null !== (ht = null === (lt = e.hw) || void 0 === lt ? void 0 : lt.y) && void 0 !== ht ? ht : 0;
var rr = null !== (ct = null === (ft = e.hw) || void 0 === ft ? void 0 : ft.z) && void 0 !== ct ? ct : 0, or = {
uniqueId: 1,
dropList: d.p.slice()
};
e.hi.bnl.gu(h.$uD.$vA.bj, an, un, rr, or, !1);
break;

case y.bP:
e.hi.bnl.gu(h.$uD.$vA.bk);
break;

case y.bQ:
e.hz && (e.hz.blt = {
type: 1,
param: [ d.p.range, e.hw.x + (null !== (st = d.p.x) && void 0 !== st ? st : 0) * e.hw.bmF, e.hw.y + (null !== (pt = d.p.y) && void 0 !== pt ? pt : 0) ]
});
break;

case y.bR:
if (!(xi = e.hi.bnl.gi(null === (dt = e.hE) || void 0 === dt ? void 0 : dt.bjO))) break;
if (!xi.hv) break;
if (!(null === (vt = xi.hx) || void 0 === vt ? void 0 : vt.bmg)) break;
Vt = xi.hx;
var br = e;
if (!Vt.blW) {
br.hz.bli = 0;
br.hz.blj = 0;
return;
}
Lt = d.p && d.p[0], Qt = .32;
Math.abs(Vt.blX.x) > Qt ? br.hz.bli = Math.sign(Vt.blX.x) * Lt : br.hz.bli = 0;
Math.abs(Vt.blX.y) > Qt ? br.hz.blj = Math.floor(Math.sign(Vt.blX.y) * Lt * h.$uD.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y) : br.hz.blj = 0;
break;

case y.bS:
e.hi.bnl.gu(h.$uD.$vA.j);
return 0;

case y.bU:
Dt = d.p;
if (!(hn = i.default.EJ(e, Dt.find, null, n))) return 0;
fe = c.default.Dl(Dt.getVar, hn, n);
e.hE.bjJ[Dt.var] = fe;
return 0;

case y.bV:
if (0 === (Dt = d.p).type) e.hE.bjY.type = 0; else {
var ar = e.hE.bjY;
ar.type = Dt.type;
ar.currentTime = 0;
ar.speed = 1;
var ur = c.default.Dl(Dt.time, e, n);
if (e.hv) if (1 === Dt.type) {
ar.time = ur;
ar.speed = e.hv.bhI(3);
} else if (2 === Dt.type) {
e.hv.bhH > 0 && (ur = r.$jG.boj(ur * (Math.max(p.GameAttributeConstRatioRange - e.hv.bhH, 0) / p.GameAttributeConstRatioRange)));
ar.time = ur;
ar.speed = 1;
}
}
return 0;

case y.bX:
Dt = d.p;
if (e.hx && e.hx.blW) {
if (e.hx.blX.y < -.5) {
i.default.EY(e, Dt.state2);
return 1;
}
if (e.hx.blX.y > .5) {
i.default.EY(e, Dt.state1);
return 1;
}
}
if (!(Ut = e.hi.bnu(null === ($t = e.hG) || void 0 === $t ? void 0 : $t.bis)) || !Ut.hw) {
i.default.EY(e, Dt.state1);
return 1;
}
var lr = (Jt = Ut.hw).x - e.hw.x, hr = Jt.y - e.hw.y;
if (e.hw.bmF !== Math.sign(lr) || Math.abs(hr) > Dt.yWidth) {
i.default.EY(e, Dt.state1);
return 1;
}
(fn = Math.abs(lr)) < Dt.disXMin && i.default.Hc(Ut) < Dt.zMax ? i.default.EY(e, Dt.state2) : i.default.EY(e, Dt.state1);
return 0;

case y.cr:
e.hi.bnl.gu(h.$uD.$vA.be, e, d.p);
return 0;

case y.cs:
e.hi.bnl.gu(h.$uD.$vA.bf, e, d.p);
return 0;

case y.ct:
e.hi.bnl.gu(h.$uD.$vA.bg, e, d.p);
return 0;

case y.cu:
e.hi.bnl.gu(h.$uD.$vA.bh, d.p);
return 0;

case y._cv:
e.hi.bnl.gu(h.$uD.$vA.bi, d.p[0], {
chatType: d.p[1],
chatData: d.p[2]
});
return 0;

case y.cw:
var fr = "";
e.hE && (fr = " b:{" + e.hE.bjK + "," + e.hE.bjL + "}");
p.default.bGR.bGT("logic log:" + fr + " " + JSON.stringify(d.p));
return 0;
}
return 0;
};
t.Dk = function(t, e, n) {
switch (n.t) {
case y.c:
n.p.id && t.hi.bnl.gu(h.$uD.$vA.G, t, n.p.id);
break;

case y.V:
n.p.id && t.hi.bnl.gu(h.$uD.$vA.bq, t, n.p.id);
break;

case y.bV:
t.hE.bjY.type = 0;
break;

case y.n:
t.hz && i.default.FG(t.hz, 0, 0, 0);
void 0 !== n.p.gzs && (t.hE.bkf = 1);
break;

case y.R:
t.hz && i.default.FG(t.hz, 0, 0, 0);
break;

case y.t:
t.hi.bnl.gu(h.$uD.$vA.M, t, null);
break;

case y.o:
n.p.id && i.default.Gk(t, n.p.id);
break;

case y.bM:
t.hq(v.default);
break;

case y.Z:
var r = n.p;
if (r.id) {
var o = t.hF.bip.findIndex(function(t) {
return t.id === r.id;
});
-1 !== o && t.hF.bip.splice(o, 1);
t.hF.biq = t.hi.bnl.fi;
}
}
};
t.Dh = [];
return t;
}();
n.default = m;
cc._RF.pop();
}, {
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$JZ": "$JZ",
"./$KB": "$KB",
"./$KC": "$KC",
"./component/$MP": "$MP",
"./component/$MV": "$MV",
"./component/$ND": "$ND",
"./component/$NF": "$NF",
"./component/$NJ": "$NJ",
"./component/$NK": "$NK",
"./component/$NL": "$NL",
"./math/$NP": "$NP",
"./math/$NQ": "$NQ"
} ]
}, {}, [ "$JZ", "$KA", "$KB", "$KC", "$KD", "$KE", "$KF", "$KG", "BattleBehaviorLogic", "$MM", "$MN", "$MO", "$MP", "$MQ", "$MR", "$MS", "$MT", "$MU", "$MV", "$MW", "$MX", "$MY", "$MZ", "$NA", "$NB", "$NC", "$ND", "$NE", "$NF", "$NG", "$NH", "$NI", "$NJ", "$NK", "$NL", "$NM", "$NN", "$NO", "$NP", "$NQ", "$NR", "$NS", "$NT", "$NU", "$NV", "$NW", "$NX", "$NY", "$NZ", "$OA", "$OB", "$OC", "$OD", "$OE", "$OF", "$OG", "$OH" ]);