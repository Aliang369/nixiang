window.__require = function t(e, i, n) {
function r(a, u) {
if (!i[a]) {
if (!e[a]) {
var b = a.split("/");
b = b[b.length - 1];
if (!e[b]) {
var h = "function" == typeof __require && __require;
if (!u && h) return h(b, !0);
if (o) return o(b, !0);
throw new Error("Cannot find module '" + a + "'");
}
a = b;
}
var l = i[a] = {
exports: {}
};
e[a][0].call(l.exports, function(t) {
return r(e[a][1][t] || t);
}, l, l.exports, t, e, i, n);
}
return i[a].exports;
}
for (var o = "function" == typeof __require && __require, a = 0; a < n.length; a++) r(n[a]);
return r;
}({
$DOO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "d036956BoxBvKqTYvc1Z3KR", "$DOO");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../utils/$DQJ"), b = t("../math/$DPR"), h = t("../../../game/src/battle/$DSO"), l = t("../../../game/src/battle/types/$DYS"), f = t("../../../game/src/battle/types/$DYM"), c = t("../../../bingo/src/utils/$DOB"), s = t("../$KL"), p = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bdQ = null;
e.bdR = new u.$jW();
e.bdS = new f.$ua();
e.bdT = null;
e.bdU = [];
e.comboTime = 0;
e.comboLimitTime = 3;
e._currentHp = 0;
e.deathCount = 0;
e.bdV = 0;
e.mpInteravl = 0;
e.bdW = !1;
e.bdX = 0;
e.bdY = {};
e.bdZ = !1;
e.bea = null;
e.beb = [];
e.bec = {};
return e;
}
e.prototype.bed = function() {
this.bdP = 0;
this.bdQ = null;
this.bdR.bny();
this.bdS.cVQ();
this.bdT = null;
for (var t = 0, e = this.bdU; t < e.length; t++) {
var i = e[t];
s.default.Ej(i);
}
this.bdU.length = 0;
this.bdV = 0;
this.mpInteravl = 0;
this.bdW = !1;
this.deathCount = 0;
this.bdX = 0;
this.bef();
this.finalDamage = 0;
this.finalHurtValue = 0;
this.lockMinHP = 0;
this.hpRateRecord = -1;
this.bea = null;
this.beb = [];
this.bec = {};
return this;
};
e.prototype.bee = function() {
if (!this.bdZ) {
this.bdZ = !0;
c.$hY.Zl(this, "totalDamageRaw", 9999);
c.$hY.Zl(this, "realHp", 1e4);
c.$hY.Zl(this.bdY, "__realHp__", 1e4);
c.$hY.Zl(this.bdY, "__lastHurt__", 0);
}
};
e.prototype.bef = function() {
this._currentHp = 0;
this.bdY[l.$uo.$vd.b] = 0;
this.bdY[l.$uo.$vd.c] = 0;
this.bdY[l.$uo.$vd.d] = 0;
this.bdY[l.$uo.$vd.e] = 0;
this.bdY[l.$uo.$vd.f] = 0;
this.bdY[l.$uo.$vd.g] = 500;
this.bdY[l.$uo.$vd.h] = 1;
this.bdY[l.$uo.$vd.i] = 0;
this.bdY[l.$uo.$vd.j] = 0;
this.bdY[l.$uo.$vd.k] = 0;
this.bdY[l.$uo.$vd.l] = 0;
this.bdY[l.$uo.$vd.m] = 0;
this.bdY[l.$uo.$vd.n] = -1;
this.bdY[l.$uo.$vd.o] = 0;
this.bdY[l.$uo.$vd.p] = 0;
this.bdY[l.$uo.$vd.q] = 2250;
this.bdY[l.$uo.$vd.r] = 1e3;
this.bdY[l.$uo.$vd.s] = 0;
this.bdY[l.$uo.$vd.t] = 0;
this.bdY[l.$uo.$vd.u] = 0;
this.bdY[l.$uo.$vd.v] = 0;
this.bdY[l.$uo.$vd.w] = 0;
this.bdY[l.$uo.$vd.x] = 0;
this.bdY[l.$uo.$vd.y] = 0;
this.bdY[l.$uo.$vd.z] = 0;
this.bdY[l.$uo.$vd.A] = 0;
this.bdY[l.$uo.$vd.B] = 0;
this.bdY[l.$uo.$vd.C] = 0;
c.ANTI_CHEAT_ENABLED && c.pooledObject(this.bdY);
};
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
e.prototype.beg = function(t) {
return this.bdU.find(function(e) {
return e.Dg.id === t;
});
};
e.prototype.beh = function(t) {
return this.bdU.find(function(e) {
return e.Dg.commonType === t;
});
};
e.prototype.bei = function(t) {
return this.bdU.find(function(e) {
return e.Dg.cmdId === t;
});
};
Object.defineProperty(e.prototype, "bej", {
get: function() {
return this.bdY[l.$uo.$vd.n];
},
set: function(t) {
this.bdY[l.$uo.$vd.n] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bek", {
get: function() {
return this.bdY[l.$uo.$vd.m];
},
set: function(t) {
this.bdY[l.$uo.$vd.m] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bel", {
get: function() {
return 0;
},
set: function() {},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bem", {
get: function() {
return this.bdY[l.$uo.$vd.c];
},
set: function(t) {
this.bdY[l.$uo.$vd.c] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "ben", {
get: function() {
return this.bdY[l.$uo.$vd.j];
},
set: function(t) {
this.bdY[l.$uo.$vd.j] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beo", {
get: function() {
return this.bdY[l.$uo.$vd.k];
},
set: function(t) {
this.bdY[l.$uo.$vd.k] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bep", {
get: function() {
return this.bdY[l.$uo.$vd.f];
},
set: function(t) {
this.bdY[l.$uo.$vd.f] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beq", {
get: function() {
return this.bdY[l.$uo.$vd.d];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "ber", {
get: function() {
return this.bdY[l.$uo.$vd.e];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bes", {
get: function() {
return this.bdY[l.$uo.$vd.i];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bet", {
get: function() {
return this.bdY[l.$uo.$vd.g];
},
set: function(t) {
this.bdY[l.$uo.$vd.g] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beu", {
get: function() {
return this._currentHp;
},
set: function(t) {
this._currentHp = Math.min(t, this.bev);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bev", {
get: function() {
return this.beI(h.GameAttriTableEnum.hpMax);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bew", {
get: function() {
return this.bdY[l.$uo.$vd.b];
},
set: function(t) {
t < 0 && (t = 0);
this.bdY[l.$uo.$vd.b] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bex", {
get: function() {
return this.beI(h.GameAttriTableEnum.mpMax);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bey", {
get: function() {
return this.bdY[l.$uo.$vd.h];
},
set: function(t) {
this.bdY[l.$uo.$vd.h] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bez", {
get: function() {
return this.beu > 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beA", {
get: function() {
return this.bdY[l.$uo.$vd.l];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beB", {
get: function() {
return this.bdY[l.$uo.$vd.y];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beC", {
get: function() {
return this.bdY[l.$uo.$vd.z];
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beD", {
get: function() {
return this.bdY[l.$uo.$vd.A];
},
set: function(t) {
this.bdY[l.$uo.$vd.A] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beE", {
get: function() {
return this.bdY[l.$uo.$vd.B];
},
enumerable: !1,
configurable: !0
});
e.prototype.beF = function() {
return b.$jB.bma(this.beu / this.bev);
};
e.prototype.beG = function() {
return Math.floor(b.$jB.bma(this.beu / this.bev * h.GameAttributeConstRatioRange));
};
e.prototype.beH = function() {
return b.$jB.bma(this.bew / this.bex);
};
e.prototype.beI = function(t) {
return this.bdS.cVN.cVu(t);
};
e.prototype.beJ = function(t, e) {
switch (t) {
case h.GameAttriTableEnum.vitPercent:
case h.GameAttriTableEnum.vit:
case h.GameAttriTableEnum.hpMax:
case h.GameAttriTableEnum.hpMaxPercent:
var i = this.bev;
this.bdS.cVN.cVz(t, e);
this.beu = Math.ceil(b.$jB.bma(b.$jB.bma(this.beu / i) * this.bev));
break;

case h.GameAttriTableEnum.spiritPercent:
case h.GameAttriTableEnum.spirit:
case h.GameAttriTableEnum.mpMax:
case h.GameAttriTableEnum.mpMaxPercent:
i = this.bex;
this.bdS.cVN.cVz(t, e);
this.bew = 0 !== i ? Math.ceil(b.$jB.bma(b.$jB.bma(this.bew / i) * this.bex)) : 0;
break;

default:
this.bdS.cVN.cVz(t, e);
}
};
e.prototype.beK = function(t, e) {
switch (t) {
case h.GameAttriTableEnum.vitPercent:
case h.GameAttriTableEnum.vit:
case h.GameAttriTableEnum.hpMax:
case h.GameAttriTableEnum.hpMaxPercent:
var i = this.bev;
this.bdS.cVN.cVA(t, e);
this.beu = Math.ceil(b.$jB.bma(b.$jB.bma(this.beu / i) * this.bev));
break;

case h.GameAttriTableEnum.spiritPercent:
case h.GameAttriTableEnum.spirit:
case h.GameAttriTableEnum.mpMax:
case h.GameAttriTableEnum.mpMaxPercent:
i = this.bex;
this.bdS.cVN.cVA(t, e);
this.bew = 0 !== i ? Math.ceil(b.$jB.bma(b.$jB.bma(this.bew / i) * this.bex)) : 0;
break;

default:
this.bdS.cVN.cVA(t, e);
}
};
e.prototype.beL = function(t, e) {
if (void 0 !== this.bdY[t]) {
this.bdY[t] += e;
this.beN(t);
}
};
e.prototype.beM = function(t, e) {
if (void 0 !== this.bdY[t]) {
this.bdY[t] -= e;
this.beN(t);
}
};
e.prototype.beN = function(t) {
switch (t) {
case l.$uo.$vd.C:
this.bdS.cVN.cUH = this.bdY[t] > 0;
}
};
Object.defineProperty(e.prototype, "beO", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beP", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beQ", {
get: function() {
return 0;
},
enumerable: !1,
configurable: !0
});
e.prototype.beR = function() {
return 0;
};
Object.defineProperty(e.prototype, "beS", {
get: function() {
return this.bdR.bnu(l.$uo.$vb.c | l.$uo.$vb.g);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beT", {
get: function() {
return this.bdR.bnu(l.$uo.$vb.c);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beU", {
get: function() {
return this.bdR.bnu(l.$uo.$vb.b);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beV", {
get: function() {
return !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beW", {
get: function() {
return 1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beX", {
get: function() {
return this.beI(h.GameAttriTableEnum.atkSpeed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beY", {
get: function() {
return this.beI(h.GameAttriTableEnum.castSpeed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "beZ", {
get: function() {
return this.beI(h.GameAttriTableEnum.speed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfa", {
get: function() {
return this.bdY[l.$uo.$vd.q];
},
set: function(t) {
this.bdY[l.$uo.$vd.q] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfb", {
get: function() {
return this.bdY[l.$uo.$vd.r];
},
set: function(t) {
this.bdY[l.$uo.$vd.r] = t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfc", {
get: function() {
return this.beI(h.GameAttriTableEnum.ap);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfd", {
get: function() {
return this.beI(h.GameAttriTableEnum.sp);
},
enumerable: !1,
configurable: !0
});
e.prototype.bfe = function(t) {
return this.beI(t ? h.GameAttriTableEnum.ap : h.GameAttriTableEnum.sp);
};
e.prototype.bff = function(t) {
return this.beI(t ? h.GameAttriTableEnum.apDef : h.GameAttriTableEnum.spDef);
};
e.prototype.bfg = function(t) {
return this.beI(t ? h.GameAttriTableEnum.apDefRate : h.GameAttriTableEnum.spDefRate);
};
e.prototype.bfh = function(t) {
return this.beI(t ? h.GameAttriTableEnum.apCrit : h.GameAttriTableEnum.spCrit);
};
e.prototype.bfi = function(t) {
return this.beI(t ? h.GameAttriTableEnum.apCritRate : h.GameAttriTableEnum.spCritRate);
};
Object.defineProperty(e.prototype, "bfj", {
get: function() {
return this.beI(h.GameAttriTableEnum.critDamageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfk", {
get: function() {
return this.beI(h.GameAttriTableEnum.finalDamageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfl", {
get: function() {
return this.beI(h.GameAttriTableEnum.finalDamageRed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfm", {
get: function() {
return this.beI(h.GameAttriTableEnum.damageRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfn", {
get: function() {
return this.beI(h.GameAttriTableEnum.allAtkRaise);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfo", {
get: function() {
return this.beI(h.GameAttriTableEnum.streFire);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfp", {
get: function() {
return this.beI(h.GameAttriTableEnum.streIce);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfq", {
get: function() {
return this.beI(h.GameAttriTableEnum.streLight);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfr", {
get: function() {
return this.beI(h.GameAttriTableEnum.streDark);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfs", {
get: function() {
return this.beI(h.GameAttriTableEnum.resisFire);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bft", {
get: function() {
return this.beI(h.GameAttriTableEnum.resisIce);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfu", {
get: function() {
return this.beI(h.GameAttriTableEnum.resisLight);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfv", {
get: function() {
return this.beI(h.GameAttriTableEnum.resisDark);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfw", {
get: function() {
return this.beI(h.GameAttriTableEnum.spellAtk);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfx", {
get: function() {
return this.beI(h.GameAttriTableEnum.mpCostRed);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bfy", {
get: function() {
return this.beI(h.GameAttriTableEnum.maxChargeTimeRed);
},
enumerable: !1,
configurable: !0
});
e.prototype.bfz = function(t) {
switch (t) {
case 1:
return Math.max(0, (h.GameAttributeConstRatioRange + this.beX) / h.GameAttributeConstRatioRange);

case 2:
return Math.max(0, (h.GameAttributeConstRatioRange + this.beZ) / h.GameAttributeConstRatioRange);

case 3:
return Math.max(0, (h.GameAttributeConstRatioRange + this.beY) / h.GameAttributeConstRatioRange);
}
};
e.prototype.bfA = function(t) {
this.bdY[l.$uo.$vd.s] > 0 && (t |= l.$uo.$uJ.b);
this.bdY[l.$uo.$vd.t] > 0 && (t |= l.$uo.$uJ.c);
this.bdY[l.$uo.$vd.u] > 0 && (t |= l.$uo.$uJ.d);
this.bdY[l.$uo.$vd.v] > 0 && (t |= l.$uo.$uJ.e);
return t;
};
e.prototype.bfB = function(t) {
var e = t.Di();
if (0 === e) return 0;
var i = this.bdS.cVN.cVE(t.CX), n = (h.GameAttributeConstRatioRange - this.beI(h.GameAttriTableEnum.spellCd)) / h.GameAttributeConstRatioRange;
return b.$jB.bma(e * Math.max(h.default.bEC.bEA.maxSpellCd, i * n) * t.cdTest);
};
e.prototype.bfC = function(t) {
var e = t.Da;
return 0 === this.bfx ? e : Math.max(0, Math.floor(b.$jB.bma(e * (h.GameAttributeConstRatioRange - this.bfx) / h.GameAttributeConstRatioRange)));
};
e.prototype.bfD = function() {
return this.bdR.bnu(l.$uo.$vb.c | l.$uo.$vb.b | l.$uo.$vb.g);
};
e.prototype.bfE = function() {
this.deathCount++;
this.bdW = !0;
};
e.prototype.bfF = function() {
this.bdW = !1;
};
e.prototype.bfG = function() {
return this.bdS.cVN.cVt(h.GameAttriTableEnum.hpMax);
};
e.prototype.bfH = function() {
var t = this, e = l.$uo.STRE_ATTR_LIST[0], i = 0;
[ h.GameAttriTableEnum.streFire, h.GameAttriTableEnum.streIce, h.GameAttriTableEnum.streLight, h.GameAttriTableEnum.streDark ].forEach(function(n, r) {
var o = t.beI(n);
if (o > i) {
i = o;
e = l.$uo.STRE_ATTR_LIST[r];
}
});
return e;
};
e.prototype.bfI = function(t) {
return (t ? this.bdQ.pvpBehaviorId : this.bdQ.behaviorId) || this.bdQ.behaviorId;
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
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = p;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../bingo/src/utils/$DOB": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYM": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../utils/$DQJ": "$DQJ"
} ],
$DOP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e963exPUIFIF7S/s1ehlRH7", "$DOP");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../math/$DPS"), b = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bfJ = 0;
e.bfK = 0;
e.bfL = -1;
e.bfM = new u.default();
e.bfN = {};
e.bfO = 0;
e.bfP = 0;
e.bfQ = null;
e.bfR = new u.default();
e.bfS = 0;
e.bfT = 0;
e.bfU = 0;
e.bfV = 0;
e.bfW = !0;
e.bfX = [];
e.bfY = null;
return e;
}
e.prototype.bfZ = function() {};
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
e.prototype.bga = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = b;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP",
"../math/$DPS": "$DPS"
} ],
$DOQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7abaazcmvVAy4QqXXXXGsm1", "$DOQ");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgb = 0;
e.bgc = 0;
e.bge = null;
e.bgf = 0;
e.bgg = [];
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "16187U6ahtC04iFrAsNQlaN", "$DOR");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../math/$DPS"), b = t("../../../game/src/battle/types/$DYS"), h = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgi = !1;
e.bgj = -1;
e.bgk = new u.default();
e.bgl = new u.default();
e.bgm = 9999;
e.bgn = b.$uo.$uS.b;
e.bgo = 0;
return e;
}
e.prototype.lQ = function(t) {
this.bgi = t[0];
this.bgj = t[1];
this.bgk.bmn(t[2], t[3]);
this.bgl.bmn(t[4], t[5]);
this.bgm = t[6];
this.bgn = t[7];
this.bgo = t[8];
};
e.prototype.lR = function() {
return [ this.bgi, this.bgj, this.bgk.x, this.bgk.y, this.bgl.x, this.bgl.y, this.bgm, this.bgn, this.bgo ];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = h;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYS": void 0,
"../lib/$DPP": "$DPP",
"../math/$DPS": "$DPS"
} ],
$DOS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5fbdd9GBbNNE65aL6am9Lrp", "$DOS");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgp = 0;
e.bgq = null;
e.bgr = null;
e.bgs = 0;
e.bgt = -1;
e.bgu = -1;
e.bgv = {};
e.bgw = 0;
e.bgx = 0;
e.bgy = 0;
e.bgz = 0;
e.bgA = 0;
e.bgB = !0;
e.bgC = -1;
e.bgD = 0;
e.bgE = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5e557rOzh1PM6fDOzwf1Ibb", "$DOT");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgF = -1;
e.bgH = 0;
e.bgI = 0;
e.bgJ = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e27f6GSs0RGLIfxqiF2IHQA", "$DOU");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgK = -1;
return e;
}
e.prototype.lQ = function(t) {
this.bgK = t;
};
e.prototype.lR = function() {
return this.bgK;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "68ae6DmQFNLLqwLcTfcCCVR", "$DOV");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgL = -1;
e.bgN = [];
return e;
}
e.prototype.lQ = function(t) {
this.bgL = t;
};
e.prototype.lR = function() {
return this.bgL;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "d3b3ftOexxLWaMAabW1CPm1", "$DOW");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bgO = 0;
e.bgP = -1;
e.bgQ = 0;
e.bgR = -1;
e.bgS = 0;
e.bgT = -1;
e.bgU = 0;
e.bgV = !1;
e.bgW = -1;
e.bgX = -1;
e.bgY = -1;
e.bgZ = .4;
return e;
}
e.prototype.bha = function(t, e) {
void 0 === e && (e = !1);
this.bhd();
this.bhc();
this.bhb();
this.bgV = !1;
if (1 === t) {
this.bgW = e ? .25 : -1;
this.bgX = .2;
this.bgY = .15;
this.bgZ = .5;
} else {
this.bgW = -1;
this.bgX = .2;
this.bgY = .2;
this.bgZ = .5;
}
};
e.prototype.bhb = function() {
this.bgT = -1;
this.bgU = 0;
};
e.prototype.bhc = function() {
this.bgR = -1;
this.bgS = 0;
};
e.prototype.bhd = function() {
this.bgP = -1;
this.bgQ = 0;
};
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return !1;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "927795WxDNCUbcb50tCA3aK", "$DOX");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bhe = {
time: 0,
duration: 0,
moments: []
};
e.bhf = 0;
e.bhg = 0;
e.bhh = [];
e.bhi = {};
e.bhj = 0;
e.bhk = 0;
e.bhl = {};
e.bhm = {};
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
e.prototype.bhn = function(t, e) {
this.bhe.time = 0;
this.bhe.duration = t;
this.bhe.moments = null != e ? e : [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DOY: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "259abY7pwtGDpTzhVsI9dHz", "$DOY");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../../../game/src/battle/types/$DYS"), b = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bho = {};
e.bhp = [];
e.bhq = [];
e.bhr = [];
e.bhs = [];
e.bht = {};
e.bhu = {};
e.bhv = 0;
e.bhw = null;
e.bhx = 0;
e.bhy = null;
e.bhz = [ 0, 0 ];
e.bhA = {};
e.bhB = -1;
e.bhC = 0;
e.bhD = {};
e.bhE = {};
e.bhF = -1;
e.bhG = -1;
e.bhH = [];
e.bhI = [];
e.bhJ = -1;
e.bhK = -1;
e.bhL = null;
e.bhM = [];
e.bhN = [];
e.bhO = {
hurtbackTime: 0,
slideTime: 0,
hurtbackType: u.$uo.$uN.b,
actId: 0,
hurtAnim: ""
};
e.bhP = {
type: 0,
time: 0,
currentTime: 0,
speed: 1
};
e.bhQ = -1;
e.bhR = !1;
e.bhS = null;
e.bhT = 0;
e.bhU = 0;
e.bhV = 0;
e.bhW = 1;
e.bhX = 0;
e.bhY = null;
e.bhZ = -1;
e.bia = 0;
e.bib = null;
e.bic = [];
e.bid = {
b: 0,
c: []
};
return e;
}
Object.defineProperty(e.prototype, "bie", {
get: function() {
var t;
return null !== (t = this.bhw.dirType) && void 0 !== t ? t : u.$uo.$uO.b;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bif", {
get: function() {
var t;
return null !== (t = this.bhw.isBuilding) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "big", {
get: function() {
var t;
return null !== (t = this.bhw.ignoreAttraction) && void 0 !== t && t || this.bih;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bih", {
get: function() {
var t;
return null !== (t = this.bhw.ignorePhysicalEffects) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bii", {
get: function() {
return this.bih || this.bhw.igonreGrab;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bij", {
get: function() {
var t;
return null === (t = this.bhw.movable) || void 0 === t || t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bik", {
get: function() {
var t;
return 2 === (null === (t = this.bho) || void 0 === t ? void 0 : t.superArmor);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bil", {
get: function() {
return !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bim", {
get: function() {
var t;
return this.bih || null !== (t = this.bhw.ignoreHitFly) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bin", {
get: function() {
return this.bhR;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bio", {
get: function() {
var t;
return this.bih || null !== (t = this.bhw.ignoreHitBack) && void 0 !== t && t;
},
enumerable: !1,
configurable: !0
});
e.prototype.bip = function(t) {
var e, i, n = this.bhs.length;
t.Do = n;
this.bhs.push(t);
this.bht[t.Dq.id] = (null !== (e = this.bht[t.Dq.id]) && void 0 !== e ? e : 0) + 1;
var r = t.Dq.event;
r && (this.bhu[r] = (null !== (i = this.bhu[r]) && void 0 !== i ? i : 0) + 1);
};
e.prototype.biq = function(t) {
var e = this.bhs[t];
if (!e) return !1;
var i = e.Dq;
this.bht[i.id] && this.bht[i.id]--;
var n = i.event;
n && this.bhu[n] && this.bhu[n]--;
e.Do = -1;
e.DA = 0;
var r = this.bhs.pop();
if (r && r !== e) {
r.Do = t;
this.bhs[t] = r;
}
return !0;
};
e.prototype.bir = function() {
this.bhO.hurtbackTime = 0;
this.bhO.slideTime = 0;
this.bhO.hurtbackType = u.$uo.$uN.b;
this.bhO.actId = 0;
this.bhO.hurtAnim = "";
this.bhP.currentTime = 0;
this.bhP.time = 0;
this.bhP.speed = 1;
this.bhP.type = 0;
};
Object.defineProperty(e.prototype, "bis", {
get: function() {
var t, e;
return null !== (e = null === (t = this.bhw) || void 0 === t ? void 0 : t.idleBehaviorId) && void 0 !== e ? e : "1";
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bit", {
get: function() {
return -1 !== this.bhJ;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "biu", {
get: function() {
return !!this.bho && (this.bho.idle || this.bho.move);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "biv", {
get: function() {
var t, e;
return null !== (e = null === (t = this.bhw) || void 0 === t ? void 0 : t.noAutoAim) && void 0 !== e && e;
},
enumerable: !1,
configurable: !0
});
e.prototype.biw = function() {
return this.bic.find(function(t) {
return 1 === t.b.type;
});
};
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = b;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYS": void 0,
"../lib/$DPP": "$DPP"
} ],
$DOZ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5c8caG3sgdAdoUMWcOGcDr3", "$DOZ");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bix = -1;
e.biy = 0;
e.biz = 0;
e.biA = 0;
e.biB = 0;
e.biC = 0;
e.biD = null;
e.biE = 0;
e.biF = 0;
e.biG = 0;
e.biH = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPA: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "a1338migBhJSpOfcGx94dNL", "$DPA");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biI = -1;
e.biJ = 0;
e.biK = 0;
return e;
}
e.prototype.lQ = function(t) {
this.biI = t[0];
this.biJ = t[1];
};
e.prototype.lR = function() {
return [ this.biI, this.biJ ];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPB: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5a54bzVnFVM4I4QFuk9l6BK", "$DPB");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$iK = i.IsCollider1InsideCollider2 = i.rectCircleIntersect = i.rectCircle = i.circleCircle = i.IsRectInsideCircle = i.IsPointInsideCircle = i.IsCircle1InsideCircle2 = void 0;
var a = t("../../../game/src/battle/types/$DYS"), u = t("../lib/$DPP"), b = t("../math/$DPQ"), h = t("../math/$DPS"), l = new h.default(), f = new h.default();
function c(t, e, i, n, r, o) {
var a = n - t, u = r - e, b = o - i;
return a * a + u * u <= b * b;
}
i.IsCircle1InsideCircle2 = c;
function s(t, e, i, n, r) {
var o = i - t, a = n - e;
return o * o + a * a <= r * r;
}
i.IsPointInsideCircle = s;
function p(t, e) {
for (var i = 0, n = t.blL; i < n.length; i++) {
var r = n[i];
if (!s(r.x, r.y, e.blK.x, e.blK.y, e.blJ)) return !1;
}
return !0;
}
i.IsRectInsideCircle = p;
i.circleCircle = function(t, e) {
var i = t.blK.bmq(e.blK, l).bmt(), n = t.blJ + e.blJ;
return i < n * n;
};
i.rectCircle = function(t, e) {
var i = t.blH;
f.x = i.width / 2;
f.y = i.height / 2;
l.x = i.x + f.x;
l.y = i.y + f.y;
return d(l, f, e.blK, e.blJ);
};
function d(t, e, i, n) {
var r = Math.abs(i.x - t.x), o = Math.abs(i.y - t.y), a = Math.max(r - e.x, 0), u = Math.max(o - e.y, 0);
return a * a + u * u <= n * n;
}
i.rectCircleIntersect = d;
i.IsCollider1InsideCollider2 = function(t, e) {
for (var i = e.biO.biL, n = t.biO.biL, r = !0, o = 0; o < i.length; o++) {
var u = i[o], b = u.shapeInfo, h = u.world;
if (1 === b.neg) {
for (var l = 0; l < n.length; l++) {
var f = n[l], s = f.shapeInfo, d = f.world;
if (1 !== s.neg) {
var v = s.t === a.$uo.$uX.c, $ = b.t === a.$uo.$uX.b, m = s.t === a.$uo.$uX.b;
if (!$) {
r = !1;
break;
}
if (!(r = m ? c(d.blK.x, d.blK.y, d.blJ, h.blK.x, h.blK.y, h.blJ) : !!v && p(d, h))) break;
}
}
if (!r) break;
}
}
return r;
};
var v = function() {
function t() {
this.biL = [];
this.biM = !1;
}
t.prototype.biN = function(t) {
var e, i = this.biL;
i.length = t.length;
for (var n = 0; n < i.length; n++) {
var r = i[n];
r || (r = i[n] = {
world: new b.$jz(),
shapeInfo: null
});
r.shapeInfo = t[n];
this.biM = !!(null === (e = t[n]) || void 0 === e ? void 0 : e.neg);
}
};
return t;
}();
i.$iK = v;
var $ = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biO = new v();
e.biP = new v();
e.biQ = [];
e.biR = 0;
e.biS = 0;
e.biT = 0;
e.biU = 1;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ u.$iZ.register() ], e);
}(u.$iZ.$ja);
i.default = $;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYS": void 0,
"../lib/$DPP": "$DPP",
"../math/$DPQ": "$DPQ",
"../math/$DPS": "$DPS"
} ],
$DPC: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5ee18U8W0NE0oAzQl3DQQgk", "$DPC");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biV = !1;
return e;
}
e.prototype.lQ = function(t) {
this.biV = t;
};
e.prototype.lR = function() {
return this.biV;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPD: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "3f5a7lBqspDLKGHMWg8gCjU", "$DPD");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$iN = void 0;
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return !1;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.$iN = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPE: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "94aaaHhEclJfIqv2QvAXhaE", "$DPE");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biW = -1;
e.biX = 100;
e.biY = 0;
return e;
}
e.prototype.lQ = function(t) {
this.biW = t[0];
this.biX = t[1];
this.biY = t[2];
};
e.prototype.lR = function() {
return [ this.biW, this.biX, this.biY ];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPF: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "60495CixbpCIoG7srO/cZT8", "$DPF");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.biZ = 0;
e.bja = 0;
e.bjb = 0;
e.bjc = 0;
e.bjd = 0;
e.bje = 0;
e.bjf = 0;
e.bjg = 0;
e.bjh = 0;
e.bji = !0;
e.bjj = !1;
e.bjk = null;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPG: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "31793I2W2xMtYqePoUvhOXm", "$DPG");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return !1;
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPH: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "23b94eAsGJF3p/HDXMQVLS5", "$DPH");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../math/$DPS"), b = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bjl = new u.default();
e.bjm = 0;
e.bjn = 0;
e.bjo = 0;
e.bjp = !0;
e.bjq = [];
e.bjr = null;
e.bjs = 0;
e.bjt = [ 0, 0, 0, 0 ];
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
e.prototype.bju = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = b;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP",
"../math/$DPS": "$DPS"
} ],
$DPI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "0e4a3SDKYtBn4BeG6PeIrnX", "$DPI");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../../../bingo/src/$KG"), u = t("../../../game/src/battle/$DSC"), b = t("../../../game/src/battle/$DSO"), h = t("../../../game/src/battle/types/$DYO"), l = t("../$KK"), f = t("../lib/$DPP"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bjv = 0;
e.bjw = 0;
e.bjx = 0;
e.bjy = 0;
e.bjz = 0;
e.bjA = 0;
e.bjB = 0;
e.bjF = [];
e.bjG = {};
e.bjH = {};
return e;
}
e.prototype.bjI = function(t, e, i, n, r) {
this.bjv = t;
this.bjw = 0;
this.bjx = 0;
this.bjz = 0;
this.bjy = 0;
this.bjA = 0;
this.bjB = 0;
this.bjC = i.slice();
this.bjD = null == n ? void 0 : n.slice();
this.bjE = r;
this.bjF = [];
this.bjG = {};
this.bjH = {};
var o = b.default.bEC.bFK(0, i, n, r);
this.bjy = o.titleId;
this.bjB = o.petId;
};
e.prototype.bjJ = function(t, e) {
var i = this.bjE;
if (null == i ? void 0 : i.consumeItems) for (var n = 0, r = i.consumeItems; n < r.length; n++) {
var o = r[n], f = new l.$ge.$gi();
f.DZ(o.itemId, o);
this.bjF.push(f);
}
var c = null;
try {
if (i.skillButton) {
var s = JSON.parse(i.skillButton), p = e ? a.$fe.tblEnum.SkillTypeTableEnum.PVE_SKILL : a.$fe.tblEnum.SkillTypeTableEnum.PVP_SKILL, d = s.find(function(t) {
return t.type === p;
});
if (null == d ? void 0 : d.button) {
c = JSON.parse(d.button);
for (var v in c) {
var $ = +v;
if (!($ < 0)) {
var m = c[v][0], y = $, g = u.BATTLE_BTN_2_CMD[y];
this.bjG[g] = m;
}
}
var D = null == c ? void 0 : c[-200];
if (D) {
this.bjG[h.$ug.v] = D[0][0];
this.bjG[h.$ug.v + 1] = D[0][1];
this.bjG[h.$ug.v + 2] = D[0][2];
this.bjG[h.$ug.v + 3] = D[0][3];
}
}
}
} catch (t) {}
if (!c) {
var j = b.default.bEC.bFd(t), P = h.FRAME_SYNC_CMD2BIT[h.$ug.g];
if (j) for (var E = 0, x = j; E < x.length; E++) {
m = x[E];
var O = b.default.bEC.bFb(m);
if (1 !== O.commonType) {
if (!(O.commonType > 0) && O.buttonEnable) {
if (!(g = h.FRAME_SYNC_BIT2CMD[P])) break;
this.bjG[g] = O.id;
P++;
}
} else this.bjG[h.$ug.d] = O.id;
}
}
for (var k in this.bjG) this.bjH[this.bjG[k]] = Number(k);
};
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
return o([ f.$iZ.register() ], e);
}(f.$iZ.$ja);
i.default = c;
cc._RF.pop();
}, {
"../$KK": "$KK",
"../../../bingo/src/$KG": void 0,
"../../../game/src/battle/$DSC": void 0,
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYO": void 0,
"../lib/$DPP": "$DPP"
} ],
$DPJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "eeef4yHV/dBrZP2I0FJZLtR", "$DPJ");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../math/$DPS"), b = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bjK = "";
e.bjL = 0;
e.bjM = 0;
e.bjN = -1;
e.bjO = new u.default();
e.bjP = 0;
e.bjQ = 0;
e.bjR = new Map();
e.bjS = new Map();
e.bjT = [];
e.bjU = 0;
e.bjV = !1;
e.bjW = 1;
return e;
}
Object.defineProperty(e.prototype, "bjX", {
get: function() {
return this.bjW && !this.bjV;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "bjY", {
get: function() {
return !!this.bjK;
},
enumerable: !1,
configurable: !0
});
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
e.prototype.bjZ = function(t) {
if (t) {
var e = this.bjR.get(t);
if (!e) {
e = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
this.bjR.set(t, e);
}
return e;
}
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = b;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP",
"../math/$DPS": "$DPS"
} ],
$DPK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b7d87ainmpCBZ4AGBrYafN/", "$DPK");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bka = 0;
e.bkb = !1;
e.bkc = !1;
e.bkd = 0;
e.bke = 0;
e.bkf = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "1bc15vyseFCsa+8AHjsnY7r", "$DPL");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bkg = 0;
e.bkh = 0;
e.bki = 0;
e.bkj = 0;
e.bkk = 0;
e.bkl = 0;
e.bkm = 0;
e.bkn = 0;
e.bko = 0;
e.bkp = [];
e.bkq = [];
e.bkr = null;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7d99fbnepFNyI4Au1MZ3ezX", "$DPM");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bks = 0;
e.bkt = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = u;
cc._RF.pop();
}, {
"../lib/$DPP": "$DPP"
} ],
$DPN: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "36d91NNP0dAnreSSNE9Sl2k", "$DPN");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../lib/$DPP"), u = t("../../../bingo/src/utils/$DOB"), b = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.worldTimeScale = 1;
e.hitPauseTimeScale = 1;
e.mainTimeScale = 1;
e.actionTimeScale = 1;
e.hTimeScale = 1;
e.vTimeScale = 1;
e.bku = 0;
return e;
}
Object.defineProperty(e.prototype, "bkv", {
get: function() {
return this.worldTimeScale * this.hitPauseTimeScale * this.mainTimeScale;
},
enumerable: !1,
configurable: !0
});
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
o([ u.pooledFloatDefault(1) ], e.prototype, "worldTimeScale", void 0);
o([ u.pooledFloatDefault(1) ], e.prototype, "hitPauseTimeScale", void 0);
o([ u.pooledFloatDefault(1) ], e.prototype, "mainTimeScale", void 0);
o([ u.pooledFloatDefault(1) ], e.prototype, "actionTimeScale", void 0);
o([ u.pooledFloatDefault(1) ], e.prototype, "hTimeScale", void 0);
o([ u.pooledFloatDefault(1) ], e.prototype, "vTimeScale", void 0);
return o([ a.$iZ.register() ], e);
}(a.$iZ.$ja);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/utils/$DOB": void 0,
"../lib/$DPP": "$DPP"
} ],
$DPO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "f29f99BlulOoL4qoE8JGsMm", "$DPO");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var a = t("../../../bingo/src/utils/$DOB"), u = t("../../../game/src/battle/types/$DYS"), b = t("../lib/$DPP"), h = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.x = 0;
e.y = 0;
e.z = 0;
e.bkw = u.$uo.$uU.b;
e.bkx = 0;
e.bky = 0;
return e;
}
e.prototype.lQ = function() {};
e.prototype.lR = function() {
return [];
};
e.prototype.bkz = function() {};
Object.defineProperty(e.prototype, "bkA", {
get: function() {
return this.z <= 0;
},
enumerable: !1,
configurable: !0
});
o([ a.pooledFloat ], e.prototype, "x", void 0);
o([ a.pooledFloat ], e.prototype, "y", void 0);
o([ a.pooledFloat ], e.prototype, "z", void 0);
return o([ b.$iZ.register() ], e);
}(b.$iZ.$ja);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/utils/$DOB": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../lib/$DPP": "$DPP"
} ],
$DPP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "a1633L4DH1PCp2RyafJYxQc", "$DPP");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__spreadArrays || function() {
for (var t = 0, e = 0, i = arguments.length; e < i; e++) t += arguments[e].length;
var n = Array(t), r = 0;
for (e = 0; e < i; e++) for (var o = arguments[e], a = 0, u = o.length; a < u; a++, 
r++) n[r] = o[a];
return n;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$iZ = void 0;
(function(t) {
t.$ja = function() {};
var e = 0, i = [];
t.register = function() {
return function(t) {
if (null != t.tid) throw new Error("already contain component " + t.tid);
t.tid = e++;
i.push(t);
};
};
t.getComponentConstructors = function() {
return i;
};
t.allOf = function() {
for (var t, e = [], i = 0; i < arguments.length; i++) e[i] = arguments[i];
return (t = new c()).allOf.apply(t, e);
};
var n = function() {
function t() {
this.bkB = 0;
}
t.prototype.bkC = function(t) {
this.bkB |= 1 << t;
};
t.prototype.bkD = function(t) {
this.bkB &= ~(1 << t);
};
t.prototype.bkE = function(t) {
return 0 != (this.bkB & 1 << t);
};
t.prototype.bkF = function(t) {
return (this.bkB & t.bkB) > 0;
};
t.prototype.bkG = function(t) {
return (this.bkB & t.bkB) === this.bkB;
};
t.prototype.bkH = function() {
this.bkB = 0;
};
return t;
}(), a = function() {
function t() {
this.hg = -1;
this.hh = new n();
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
this.hh.bkE(e) && this.hq(t);
this.hh.bkC(e);
var i = this.hi.bli(t);
this.hj[t.tid] = i;
this.hk(e, i);
this.hi.blk(this, e);
return i;
};
t.prototype.ho = function(t) {
return this.hj[t.tid];
};
t.prototype.hp = function(t) {
return !!this.ho(t);
};
t.prototype.hq = function(t) {
if (this.hh.bkE(t.tid)) {
var e = t.tid, i = this.hj[t.tid];
this.hi.blj(e, i);
this.hj[t.tid] = null;
this.hk(e, null);
this.hh.bkD(e);
this.hi.blk(this, e);
}
};
t.prototype.hr = function() {
for (var t = null, n = this.hh.bkB, r = 0; r < e; r++) if (0 != (n & 1 << r)) {
t = i[r];
var o = this.hj[t.tid];
this.hi.blj(r, o);
this.hj[t.tid] = null;
this.hk(r, null);
this.hh.bkD(r);
this.hi.blk(this, r);
}
this.hh.bkH();
this.hg = -1;
this.hi = null;
};
t.prototype.hs = function() {
for (var t = null, n = this.hh.bkB, r = 0; r < e; r++) if (0 != (n & 1 << r)) {
t = i[r];
var o = this.hj[t.tid];
this.hi.blj(r, o);
this.hj[t.tid] = null;
this.hk(r, null);
}
this.hh.bkH();
this.hg = -1;
this.hi = null;
};
t.prototype.ht = function(t) {
this.hg = t.eid;
this.hh.bkB = t.maskFlag;
var e = null, n = t.comps;
for (var r in n) {
var o = Number(r);
e = i[o];
var a = this.hi.bli(e);
a.lQ(n[r]);
this.hj[e.tid] = a;
}
this.hl();
};
t.prototype.hu = function() {
for (var t = {}, n = this.hh.bkB, r = null, o = 0; o < e; o++) if (0 != (n & 1 << o)) {
r = i[o];
var a = this.hj[r.tid].lR();
t[o] = a;
}
return {
eid: this.hg,
maskFlag: this.hh.bkB,
comps: t
};
};
return t;
}();
t.$jc = a;
var u = function() {
function t(t, e) {
this.bkK = new Set();
this.bkL = null;
this.bkM = 0;
this.bkJ = e;
this.bkI = t;
}
Object.defineProperty(t.prototype, "bkN", {
get: function() {
var t = this;
null === this.bkL && (this.bkL = Array.from(this.bkK.values()).map(function(e) {
return t.bkJ.bll(e);
}));
return this.bkL;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(t.prototype, "bkO", {
get: function() {
return this.bkN[0];
},
enumerable: !1,
configurable: !0
});
t.prototype.bkP = function(t) {
if (this.bkI.isMatch(t)) {
this.bkK.add(t.hg);
this.bkL = null;
this.bkM++;
} else if (this.bkK.has(t.hg)) {
this.bkK.delete(t.hg);
this.bkL = null;
this.bkM--;
}
};
t.prototype.bkQ = function() {
this.bkK.clear();
this.bkL = null;
this.bkM = 0;
};
t.prototype.bkR = function(t) {
this.bkM = t.length;
this.bkL = null;
this.bkK.clear();
for (var e = 0, i = t; e < i.length; e++) {
var n = i[e];
this.bkK.add(n);
}
};
t.prototype.bkS = function() {
return Array.from(this.bkK.values());
};
return t;
}();
t.$jd = u;
var b = function() {
function t() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
this.lS = new n();
this.lT = [];
for (var i = -1, r = t.length, o = 0; o < r; o++) {
if (-1 == (i = t[o])) throw Error("存在没有注册的组件！");
this.lS.bkC(i);
this.lT.indexOf(t[o]) < 0 && this.lT.push(t[o]);
}
r > 1 && this.lT.sort(function(t, e) {
return t - e;
});
}
t.prototype.toString = function() {
return this.lT.join("-");
};
return t;
}(), h = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lV = function(t) {
return this.lS.bkF(t.hh);
};
e.prototype.lU = function() {
return "anyOf:" + this.toString();
};
return e;
}(b), l = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lV = function(t) {
return this.lS.bkG(t.hh);
};
e.prototype.lU = function() {
return "allOf:" + this.toString();
};
return e;
}(b), f = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.lU = function() {
return "excludeOf:" + this.toString();
};
e.prototype.lV = function(t) {
return !this.lS.bkF(t.hh);
};
return e;
}(b), c = function() {
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
Array.prototype.push.apply(t._indices, e.lT);
});
}
return this._indices;
},
enumerable: !1,
configurable: !0
});
t.prototype.anyOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var i = [], n = 0, r = t.length; n < r; n++) i.push(t[n].tid);
this.rules.push(new (h.bind.apply(h, o([ void 0 ], i)))());
return this;
};
t.prototype.allOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var i = [], n = 0, r = t.length; n < r; n++) i.push(t[n].tid);
this.rules.push(new (l.bind.apply(l, o([ void 0 ], i)))());
return this;
};
t.prototype.onlyOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var n = [], r = 0, a = t.length; r < a; r++) n.push(t[r].tid);
this.rules.push(new (l.bind.apply(l, o([ void 0 ], n)))());
for (var u = i, b = [], h = 0, c = u; h < c.length; h++) {
var s = c[h];
n.indexOf(s.tid) < 0 && b.push(s.tid);
}
this.rules.push(new (f.bind.apply(f, o([ void 0 ], b)))());
return this;
};
t.prototype.excludeOf = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
for (var i = [], n = 0, r = t.length; n < r; n++) i.push(t[n].tid);
this.rules.push(new (f.bind.apply(f, o([ void 0 ], i)))());
return this;
};
t.prototype.getKey = function() {
for (var t = "", e = 0; e < this.rules.length; e++) {
t += this.rules[e].lU();
e < this.rules.length - 1 && (t += " && ");
}
return t;
};
t.prototype.isMatch = function(t) {
if (1 === this.rules.length) return this.rules[0].lV(t);
if (2 === this.rules.length) return this.rules[0].lV(t) && this.rules[1].lV(t);
for (var e = 0, i = this.rules; e < i.length; e++) if (!i[e].lV(t)) return !1;
return !0;
};
return t;
}(), s = function() {
function t(t) {
this.context = null;
this.dt = 0;
this.fixedUpdate = !1;
this.context = t;
this.group = this.context.blh(this.filter(), "e");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bkM > 0 || this.fixedUpdate;
};
t.prototype.execute = function(t) {
this.dt = t;
this.update(this.group.bkN);
};
t.prototype.postEvent = function(t, e, i, n, r, o, a) {
this.context.blc.gu(t, e, i, n, r, o, a);
};
return t;
}();
t.$jj = s;
(function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.blh(this.filter(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bkM > 0;
};
t.prototype.execute = function(t) {
this.dt = t;
this.update(this.group.bkN);
this.group.bkQ();
};
t.prototype.postEvent = function(t, e, i, n, r, o, a) {
this.context.blc.gu(t, e, i, n, r, o, a);
};
})(), function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.blh(this.filter(), "e");
this.rGroup = this.context.blh(this.filter(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bkM > 0;
};
t.prototype.execute = function(t) {
this.dt = t;
if (this.rGroup.bkM > 0) {
this.entityEnter(this.rGroup.bkN);
this.rGroup.bkQ();
}
this.update(this.group.bkN);
};
t.prototype.postEvent = function(t, e, i, n, r, o, a) {
this.context.blc.gu(t, e, i, n, r, o, a);
};
}();
var p = function() {
function t(t) {
this.context = null;
this.dt = 0;
this.context = t;
this.group = this.context.blh(this.filter(), "e");
this.eGroup = this.context.blh(this.event(), "r");
}
t.prototype.init = function() {};
t.prototype.destroy = function() {};
t.prototype.canRun = function() {
return this.group.bkM > 0 || this.eGroup.bkM > 0;
};
t.prototype.clearEventEntities = function() {
if (this.eGroup.bkM > 0) for (var t = 0, e = this.eGroup.bkN; t < e.length; t++) {
var i = e[t];
this.context.blg(i);
}
};
t.prototype.execute = function(t) {
this.dt = t;
this.eGroup.bkM > 0 && this.eventCallback(this.eGroup.bkN, this.group.bkN);
this.group.bkM > 0 && this.update(this.group.bkN);
};
t.prototype.postEvent = function(t, e, i, n, r, o, a) {
this.context.blc.gu(t, e, i, n, r, o, a);
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
var i = e[t];
this.executeCount[i.__proto__.constructor.name] = 0;
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
for (var e = 0, i = this.executeSystemFlows; e < i.length; e++) {
var n = i[e];
n.canRun() && n.execute(t);
}
if (this.eventSystemsCnt > 0) for (var r = 0; r < this.eventSystemsCnt; r++) this.eventSystems[r].clearEventEntities();
};
t.prototype.debugExecute = function() {};
return t;
}();
t.$jn = d;
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
t.$jo = v;
var $ = function() {
function t(t, e, i) {
this.bkT = null;
this.bkU = null;
this.bkV = [];
this.bkW = new Map();
this.bkX = 0;
this.bkZ = null;
this.bla = new Map();
this.blb = null;
this.blc = null;
this.bld = 0;
this.blc = i;
this.blb = t;
this.bkX = e.length;
this.bkY = e;
this.bkT = new Array(this.bkX);
this.bkZ = new Array(this.bkX);
for (var n = 0; n < this.bkX; n++) {
this.bkT[n] = [];
this.bkZ[n] = [];
}
this.blm();
}
t.prototype.ble = function() {
var t = this.bkV.pop() || new this.blb(this);
this.bld++;
t.hm(this, this.bld);
this.bkW.set(t.hg, t);
return t;
};
t.prototype.blf = function(t) {
var e = this.bkV.pop() || new this.blb(this);
e.hm(this, this.bld);
e.ht(t);
return e;
};
t.prototype.blg = function(t) {
this.bkV.push(t);
this.bkW.delete(t.hg);
t.hr();
};
t.prototype.blh = function(t, e) {
void 0 === e && (e = "e");
var i = e + "_" + t.getKey(), n = this.bla.get(i);
if (!n) {
n = new u(t, this);
this.bla.set(i, n);
for (var r = t.b, o = 0, a = r.length; o < a; o++) this.bkZ[r[o]].push(n.bkP.bind(n));
}
return n;
};
t.prototype.bli = function(t) {
return this.bkT[t.tid].pop() || new this.bkY[t.tid]();
};
t.prototype.blj = function(t, e) {
this.bkT[t].push(e);
};
t.prototype.blk = function(t, e) {
for (var i = this.bkZ[e], n = i.length - 1; n >= 0; n--) i[n](t);
};
t.prototype.bll = function(t) {
return this.bkW.get(t);
};
t.prototype.blm = function() {
this.bkU || (this.bkU = this.ble());
return this.bkU;
};
t.prototype.bln = function(t) {
this.bkU.hp(t) || this.bkU.hn(t);
return this.bkU.ho(t);
};
t.prototype.blo = function(t) {
return this.ble().hn(t);
};
t.prototype.blp = function() {
this.blc = null;
this.bkT = null;
this.bkV = null;
this.bkZ = null;
this.bkY = null;
this.bkU = null;
this.bkW.clear();
this.bla.clear();
};
t.prototype.blq = function(t) {
var e = this;
this.bld = t.eid;
this.bkW.forEach(function(t) {
t.hs();
e.bkV.push(t);
});
this.bkW.clear();
this.bkU = null;
t.eid2EntityValues.forEach(function(t) {
var i = e.blf(t);
e.bkW.set(i.hg, i);
});
-1 !== t.singletonEid && (this.bkU = this.bkW.get(t.singletonEid));
var i = t.groups;
this.bla.forEach(function(t, e) {
var n = i[e];
n && t.bkR(n);
});
};
t.prototype.blr = function() {
var t = -1;
this.bkU && (t = this.bkU.hg);
var e = 0, i = this.bkW.size, n = new Array(i);
this.bkW.forEach(function(t) {
n[e] = t.hu();
e++;
});
var r = {};
this.bla.forEach(function(t, e) {
r[e] = t.bkS();
});
return {
eid: this.bld,
singletonEid: t,
eid2EntityValues: n,
groups: r
};
};
return t;
}();
t.$jp = $;
})(i.$iZ || (i.$iZ = {}));
cc._RF.pop();
}, {} ],
$DPQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "17ef0I4cBxJAp7RlJDHUttf", "$DPQ");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jA = i.$jz = i.$jy = i.$jx = void 0;
var n = t("./$DPR"), r = t("./$DPS"), o = function() {
function t() {
this.bls = new r.default();
this.blt = 0;
this.blu = 0;
this.blv = 0;
this.blw = 0;
this.blx = 0;
this.bly = new r.default();
this.blz = new r.default();
}
t.prototype.blA = function(t, e, i, n, r) {
this.bls.x = t;
this.bls.y = e;
this.bly.x = Math.cos(r);
this.bly.y = Math.sin(r);
this.blz.x = -this.bly.y;
this.blz.y = this.bly.x;
this.blu = i;
this.blv = n;
this.blw = i / 2;
this.blx = n / 2;
this.blt = r;
};
t.prototype.blB = function(t) {
return this.blw * Math.abs(t.bms(this.bly)) + this.blx * Math.abs(t.bms(this.blz));
};
t.prototype.blC = function(t) {
this.bls.bmn(t[0], t[1]);
this.blt = t[2];
this.blu = t[3];
this.blv = t[4];
this.blw = t[5];
this.blx = t[6];
this.bly.bmn(t[7], t[8]);
this.blz.bmn(t[9], t[10]);
};
t.prototype.blD = function() {
return [ this.bls.x, this.bls.y, this.blt, this.blu, this.blv, this.blw, this.blx, this.bly.x, this.bly.y, this.blz.x, this.blz.y ];
};
return t;
}();
i.$jx = o;
var a = function() {
function t(t, e, i, n) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
void 0 === i && (i = 0);
void 0 === n && (n = 0);
this.x = t;
this.y = e;
this.width = i;
this.height = n;
}
t.prototype.blE = function(t) {
this.x = t[0];
this.y = t[1];
this.width = t[2];
this.height = t[3];
};
t.prototype.blF = function() {
return [ this.x, this.y, this.width, this.height ];
};
Object.defineProperty(t.prototype, "blG", {
get: function() {
return this.y + this.height;
},
enumerable: !1,
configurable: !0
});
return t;
}();
i.$jy = a;
i.$jz = function() {
this.blH = new a();
this.blI = new o();
this.blJ = 0;
this.blK = new r.default();
this.blL = [ new r.default(), new r.default(), new r.default(), new r.default() ];
};
var u = new r.default(), b = new r.default(), h = new r.default(), l = new r.default(1, 0), f = new r.default(0, 1), c = 0, s = 0, p = 0, d = 0, v = function() {
function t() {}
t.blM = function(t, e) {
t.bls.bmq(e.bls, u);
var i = t.bly;
if (t.blB(i) + e.blB(i) <= Math.abs(u.bms(i))) return !1;
var n = t.blz;
if (t.blB(n) + e.blB(n) <= Math.abs(u.bms(n))) return !1;
var r = e.bly;
if (t.blB(r) + e.blB(r) <= Math.abs(u.bms(r))) return !1;
var o = e.blz;
return !(t.blB(o) + e.blB(o) <= Math.abs(u.bms(o)));
};
t.blN = function(t, e) {
var i = t.blK.bmq(e.blK, b).bmt(), n = t.blJ + e.blJ;
return i < n * n;
};
t.blO = function(e, i, r, o, a) {
n.$jB.blV(e.blI.bls.x, e.blI.bls.y, i.blK.x, i.blK.y, -r, b);
return t.blR(e.blI.bls, h.bmn(o / 2, a / 2), b, i.blJ);
};
t.blP = function(e, i) {
var n = i.width / 2, r = i.height / 2;
u.x = e.bls.x - (i.x + n);
u.y = e.bls.y - (i.y + r);
var o = e.bly, a = e.blz, b = l, h = f;
return !(e.blB(o) + t.blQ(n, r, l, f, o) <= Math.abs(u.bms(o)) || e.blB(a) + t.blQ(n, r, l, f, a) <= Math.abs(u.bms(a)) || e.blB(b) + t.blQ(n, r, l, f, b) <= Math.abs(u.bms(b)) || e.blB(h) + t.blQ(n, r, l, f, h) <= Math.abs(u.bms(h)));
};
t.blQ = function(t, e, i, n, r) {
return t * Math.abs(r.bms(i)) + e * Math.abs(r.bms(n));
};
t.blR = function(t, e, i, n) {
var r = Math.abs(i.x - t.x), o = Math.abs(i.y - t.y), a = Math.max(r - e.x, 0), u = Math.max(o - e.y, 0);
return a * a + u * u <= n * n;
};
t.blS = function(t, e) {
c = t.x + t.width;
s = t.y + t.height;
p = e.x + e.width;
d = e.y + e.height;
return !(c < e.x || p < t.x || s < e.y || d < t.y);
};
return t;
}();
i.$jA = v;
cc._RF.pop();
}, {
"./$DPR": "$DPR",
"./$DPS": "$DPS"
} ],
$DPR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e9efeL9RdNBBKNNq8OJaPgs", "$DPR");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jB = void 0;
var n = [ 1, 10, 100, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10 ], r = function() {
function t() {}
t.blV = function(e, i, n, r, o, a) {
var u = Math.cos(o), b = Math.sin(o);
a.x = t.bma((n - e) * u - (r - i) * b + e, 5);
a.y = t.bma((r - i) * u + (n - e) * b + i, 5);
};
t.blW = function(e) {
(e %= 360) > 180 ? e -= 360 : e < -180 && (e += 360);
return t.bma(e, 5);
};
t.blX = function(e, i) {
var n = Math.sin(i), r = Math.cos(i), o = e.x, a = e.y;
e.x = t.bma(r * o - n * a, 5);
e.y = t.bma(n * o + r * a, 5);
return e;
};
t.blY = function(e, i) {
e.x = t.bma(e.x * i, 5);
e.y = t.bma(e.y * i, 5);
return e;
};
t.blZ = function(e) {
this.blT = e.x;
this.blU = e.y;
var i = t.bma(this.blT * this.blT + this.blU * this.blU, 5);
if (i > 0) {
i = t.bma(1 / Math.sqrt(i), 5);
e.x = t.bma(this.blT * i, 5);
e.y = t.bma(this.blU * i, 5);
}
return e;
};
t.bma = function(t, e) {
void 0 === e && (e = 6);
var i = n[e];
return Math.round(t * i) / i;
};
t.bmb = function(t, e) {
void 0 === e && (e = 6);
var i = e >= 0 && e <= 10 ? n[e] : Math.pow(10, e);
return Math.floor(t * i) / i;
};
t.bmc = function(t) {
return 1 / this.bmf(t);
};
t.bmf = function(t) {
var e, i;
e = .5 * t;
i = t;
this.bme.setFloat64(0, t, !0);
var n = this.bme.getUint32(0, !0), r = this.bme.getUint32(4, !0);
n >>= 1;
r % 2 == 1 && (n += 2147483648);
r = 1608969040 - (r >>= 1);
if ((n = 3349813753 - n) < 0) {
r--;
n += 4294967296;
}
this.bme.setUint32(0, n, !0);
this.bme.setUint32(4, r, !0);
return (i = this.bme.getFloat64(0, !0)) * (1.5 - e * i * i);
};
t.bmg = function() {
this.bme = new DataView(this.bmd);
for (var t = 0; t < this.bmh.length; t++) this.bmh[t] = this.bmh[t] / 1e3;
for (t = 0; t < this.bmj.length; t++) this.bmj[t] = this.bmj[t] / 1e3;
};
t.bmi = function(t) {
(t %= 360) < 0 && (t += 360);
return this.bmh[t];
};
t.bmk = function(t) {
(t %= 360) < 0 && (t += 360);
return this.bmj[t];
};
t.bmm = function(t, e) {
if (0 === e) return t > 0 ? 90 : -90;
var i = Math.floor(t / e * 1e3);
i < 0 && (i = -i);
for (var n = 0, r = this.bml; i > r[n]; ) n++;
t >= 0 ? e <= 0 && (n = 180 - n) : e <= 0 ? n += 180 : n = 360 - n;
return n;
};
t.blT = 0;
t.blU = 0;
t.bmd = new ArrayBuffer(8);
t.bmh = [ 0, 17, 35, 52, 70, 87, 105, 122, 139, 156, 174, 191, 208, 225, 242, 259, 276, 292, 309, 326, 342, 358, 375, 391, 407, 423, 438, 454, 469, 485, 500, 515, 530, 545, 559, 574, 588, 602, 616, 629, 643, 656, 669, 682, 695, 707, 719, 731, 743, 755, 766, 777, 788, 799, 809, 819, 829, 839, 848, 857, 866, 875, 883, 891, 899, 906, 914, 921, 927, 934, 940, 946, 951, 956, 961, 966, 970, 974, 978, 982, 985, 988, 990, 993, 995, 996, 998, 999, 999, 1e3, 1e3, 1e3, 999, 999, 998, 996, 995, 993, 990, 988, 985, 982, 978, 974, 970, 966, 961, 956, 951, 946, 940, 934, 927, 921, 914, 906, 899, 891, 883, 875, 866, 857, 848, 839, 829, 819, 809, 799, 788, 777, 766, 755, 743, 731, 719, 707, 695, 682, 669, 656, 643, 629, 616, 602, 588, 574, 559, 545, 530, 515, 500, 485, 469, 454, 438, 423, 407, 391, 375, 358, 342, 326, 309, 292, 276, 259, 242, 225, 208, 191, 174, 156, 139, 122, 105, 87, 70, 52, 35, 17, 0, -17, -35, -52, -70, -87, -105, -122, -139, -156, -174, -191, -208, -225, -242, -259, -276, -292, -309, -326, -342, -358, -375, -391, -407, -423, -438, -454, -469, -485, -500, -515, -530, -545, -559, -574, -588, -602, -616, -629, -643, -656, -669, -682, -695, -707, -719, -731, -743, -755, -766, -777, -788, -799, -809, -819, -829, -839, -848, -857, -866, -875, -883, -891, -899, -906, -914, -921, -927, -934, -940, -946, -951, -956, -961, -966, -970, -974, -978, -982, -985, -988, -990, -993, -995, -996, -998, -999, -999, -1e3, -1e3, -1e3, -999, -999, -998, -996, -995, -993, -990, -988, -985, -982, -978, -974, -970, -966, -961, -956, -951, -946, -940, -934, -927, -921, -914, -906, -899, -891, -883, -875, -866, -857, -848, -839, -829, -819, -809, -799, -788, -777, -766, -755, -743, -731, -719, -707, -695, -682, -669, -656, -643, -629, -616, -602, -588, -574, -559, -545, -530, -515, -500, -485, -469, -454, -438, -423, -407, -391, -375, -358, -342, -326, -309, -292, -276, -259, -242, -225, -208, -191, -174, -156, -139, -122, -105, -87, -70, -52, -35, -17 ];
t.bmj = [ 1e3, 1e3, 999, 999, 998, 996, 995, 993, 990, 988, 985, 982, 978, 974, 970, 966, 961, 956, 951, 946, 940, 934, 927, 921, 914, 906, 899, 891, 883, 875, 866, 857, 848, 839, 829, 819, 809, 799, 788, 777, 766, 755, 743, 731, 719, 707, 695, 682, 669, 656, 643, 629, 616, 602, 588, 574, 559, 545, 530, 515, 500, 485, 469, 454, 438, 423, 407, 391, 375, 358, 342, 326, 309, 292, 276, 259, 242, 225, 208, 191, 174, 156, 139, 122, 105, 87, 70, 52, 35, 17, 0, -17, -35, -52, -70, -87, -105, -122, -139, -156, -174, -191, -208, -225, -242, -259, -276, -292, -309, -326, -342, -358, -375, -391, -407, -423, -438, -454, -469, -485, -500, -515, -530, -545, -559, -574, -588, -602, -616, -629, -643, -656, -669, -682, -695, -707, -719, -731, -743, -755, -766, -777, -788, -799, -809, -819, -829, -839, -848, -857, -866, -875, -883, -891, -899, -906, -914, -921, -927, -934, -940, -946, -951, -956, -961, -966, -970, -974, -978, -982, -985, -988, -990, -993, -995, -996, -998, -999, -999, -1e3, -1e3, -1e3, -999, -999, -998, -996, -995, -993, -990, -988, -985, -982, -978, -974, -970, -966, -961, -956, -951, -946, -940, -934, -927, -921, -914, -906, -899, -891, -883, -875, -866, -857, -848, -839, -829, -819, -809, -799, -788, -777, -766, -755, -743, -731, -719, -707, -695, -682, -669, -656, -643, -629, -616, -602, -588, -574, -559, -545, -530, -515, -500, -485, -469, -454, -438, -423, -407, -391, -375, -358, -342, -326, -309, -292, -276, -259, -242, -225, -208, -191, -174, -156, -139, -122, -105, -87, -70, -52, -35, -17, 0, 17, 35, 52, 70, 87, 105, 122, 139, 156, 174, 191, 208, 225, 242, 259, 276, 292, 309, 326, 342, 358, 375, 391, 407, 423, 438, 454, 469, 485, 500, 515, 530, 545, 559, 574, 588, 602, 616, 629, 643, 656, 669, 682, 695, 707, 719, 731, 743, 755, 766, 777, 788, 799, 809, 819, 829, 839, 848, 857, 866, 875, 883, 891, 899, 906, 914, 921, 927, 934, 940, 946, 951, 956, 961, 966, 970, 974, 978, 982, 985, 988, 990, 993, 995, 996, 998, 999, 999, 1e3 ];
t.bml = [ 0, 17, 35, 52, 70, 87, 105, 123, 141, 158, 176, 194, 213, 231, 249, 268, 287, 306, 325, 344, 364, 384, 404, 424, 445, 466, 488, 510, 532, 554, 577, 601, 625, 649, 675, 700, 727, 754, 781, 810, 839, 869, 900, 933, 966, 1e3, 1036, 1072, 1111, 1150, 1192, 1235, 1280, 1327, 1376, 1428, 1483, 1540, 1600, 1664, 1732, 1804, 1881, 1963, 2050, 2145, 2246, 2356, 2475, 2605, 2747, 2904, 3078, 3271, 3487, 3732, 4011, 4331, 4705, 5145, 5671, 6314, 7115, 8144, 9514, 11430, 14301, 19081, 28636, 57290, Infinity ];
return t;
}();
i.$jB = r;
r.bmg();
cc._RF.pop();
}, {} ],
$DPS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "38cfc4OtbZDbri3y3oR2Xgv", "$DPS");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("./$DPR"), r = function() {
function t(t, e) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
this.x = t;
this.y = e;
}
t.prototype.bmn = function(t, e) {
this.x = t;
this.y = e;
return this;
};
t.prototype.bmo = function(t) {
this.x = t.x;
this.y = t.y;
return this;
};
t.prototype.bmp = function() {
return new t(this.x, this.y);
};
t.prototype.bmq = function(t, e) {
e.x = this.x - t.x;
e.y = this.y - t.y;
return e;
};
t.prototype.bmr = function() {
return Math.sqrt(this.x * this.x + this.y * this.y);
};
t.prototype.bms = function(t) {
return this.x * t.x + this.y * t.y;
};
t.prototype.bmt = function() {
return this.x * this.x + this.y * this.y;
};
t.prototype.bmu = function(t) {
t.x = -this.x;
t.y = -this.y;
return t;
};
t.prototype.bmv = function(t, e) {
var i = this.x, n = this.y, r = Math.sin(t), o = Math.cos(t);
e.x = o * i - r * n;
e.y = r * i + o * n;
return e;
};
t.prototype.bmw = function(t, e) {
var i = this.x, r = this.y, o = n.$jB.bma(Math.sin(t)), a = n.$jB.bma(Math.cos(t));
e.x = n.$jB.bma(a * i - o * r);
e.y = n.$jB.bma(o * i + a * r);
return e;
};
t.prototype.bmx = function(t) {
this.x += t.x;
this.y += t.y;
return this;
};
return t;
}();
i.default = r;
cc._RF.pop();
}, {
"./$DPR": "$DPR"
} ],
$DPT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "a07abgaKHJCu6M6Cjxxa2gH", "$DPT");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = function() {
function t(t, e, i) {
void 0 === t && (t = 0);
void 0 === e && (e = 0);
void 0 === i && (i = 0);
this.x = t;
this.y = e;
this.z = i;
}
t.prototype.bmy = function() {
return new t(this.x, this.y, this.z);
};
t.prototype.bmz = function(e) {
return new t(this.x - e.x, this.y - e.y, this.z - e.z);
};
t.prototype.bmA = function() {
return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
};
return t;
}();
i.default = n;
cc._RF.pop();
}, {} ],
$DPU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "6b0e7TWtFRH9bKY+l8IOApn", "$DPU");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jE = void 0;
var o = t("../$KL"), a = t("../math/$DPR"), u = t("../lib/$DPP"), b = t("../component/$DOO"), h = t("../../../game/src/battle/types/$DYS"), l = t("../math/$DPS"), f = t("../component/$DPL"), c = t("../../../game/src/battle/$DSO"), s = [ 150, 200 ], p = [ 1500, 2e3 ], d = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmB = new l.default();
return e;
}
e.prototype.filter = function() {
return u.$iZ.allOf(b.default);
};
e.prototype.update = function(t) {
var e;
if (this.context.blc.gs().bhi.canCtrl) for (var i = 0, n = t; i < n.length; i++) {
var r = n[i], o = a.$jB.bma(this.dt * r.hy.worldTimeScale, 4), u = r.hv;
if (r.hN) {
r.hN.bgO = u.beF();
a.$jB.bma(r.hi.blc.fk - r.hN.bgU) > 2 && r.hN.bhb();
}
if (u.beI(c.GameAttriTableEnum.spirit) > 0) {
u.mpInteravl = a.$jB.bma(u.mpInteravl + o);
if (u.mpInteravl > 1) {
u.mpInteravl = a.$jB.bma(u.mpInteravl - 1);
var b = Math.floor(a.$jB.bma(u.beI(c.GameAttriTableEnum.mpRecSp) / 60));
b && (u.bew = Math.min(u.bex, u.bew + b));
}
}
if (u.bep > 0) {
u.comboTime = a.$jB.bma(u.comboTime + o, 4);
if (u.comboTime >= u.comboLimitTime) {
u.comboTime = 0;
u.bep = 0;
this.postEvent(h.$uo.$vk.z, r);
}
}
for (var l = r.hv.bdU, f = a.$jB.bma(Math.max(u.beI(c.GameAttriTableEnum.spellCdRecoverRate) + c.GameAttributeConstRatioRange, 0) / c.GameAttributeConstRatioRange), s = 0, p = l; s < p.length; s++) {
var d = p[s];
if (d.CZ < 2) d.cd > 0 && (d.cd = Math.max(a.$jB.bma(d.cd - a.$jB.bma(f * o), 4), 0)); else if (d.CY < d.CZ) {
d.cd = a.$jB.bma(d.cd - a.$jB.bma(f * o), 4);
if (d.cd <= 0) {
d.cd = d.maxCdTime;
d.CY = Math.min(d.CZ, d.CY + 1);
}
}
}
var v = null === (e = r.hL) || void 0 === e ? void 0 : e.bjF;
if (v) for (var $ = 0, m = v; $ < m.length; $++) {
var y = m[$];
y.DO > 0 && (y.DO = Math.max(a.$jB.bma(y.DO - o, 4), 0));
}
if (r.hE) {
var g = r.hE.bhA;
if (r.hG) {
g.aimDist = r.hG.bgm;
g.aimDiffX = r.hG.bgk.x;
g.aimDiffY = r.hG.bgk.y;
}
}
}
};
e.prototype.bmC = function(t, e) {
var i = t.hv;
if (i.bel > 0) i.bel = a.$jB.bma(i.bel - e); else {
i.bel = .5;
var n = t.hE, r = t.hw, u = t.hi.bln(f.default), b = t.hi.blc;
if (null == t ? void 0 : t.hG) {
var l = this.context.bll(t.hG.bgj);
if (l) {
var c = t.hG.bgl.bmu(this.bmB), p = a.$jB.bma(1 * b.ge() - .5);
c.bmw(p, c);
var d = s[0] + (s[1] > 0 ? a.$jB.bma(a.$jB.bma(b.ge() - .5) * s[1]) : 0), v = l.hw, $ = a.$jB.bma(a.$jB.bma(c.x * d) + v.x, 3), m = a.$jB.bma(a.$jB.bma(c.y * d) + v.y, 3);
$ = Math.max(Math.min($, u.bkm), u.bkl);
m = Math.max(Math.min(m, u.bko), u.bkn);
n.bie === h.$uo.$uO.c && (m = r.y);
this.bmB.bmn($, m);
o.default.Fk(t, this.bmB, 50);
}
}
}
};
e.prototype.bmD = function(t, e) {
var i = t.hv;
if (i.bel > 0) i.bel = a.$jB.bma(i.bel - e); else {
i.bel = .3;
var n = t.hE, r = t.hw, u = t.hi.bln(f.default), b = t.hi.blc;
if (null == t ? void 0 : t.hG) {
var l = this.context.bll(t.hG.bgj);
if (l) {
var c = t.hG.bgl.bmu(this.bmB), s = a.$jB.bma(1 * b.ge() - .5);
c.bmw(s, c);
var d = p[0] + (p[1] > 0 ? a.$jB.bma(a.$jB.bma(b.ge() - .5) * p[1]) : 0), v = l.hw, $ = a.$jB.bma(a.$jB.bma(c.x * d) + v.x, 3), m = a.$jB.bma(a.$jB.bma(c.y * d) + v.y, 3);
$ = Math.max(Math.min($, u.bkm), u.bkl);
m = Math.max(Math.min(m, u.bko), u.bkn);
n.bie === h.$uo.$uO.c && (m = r.y);
this.bmB.bmn($, m);
o.default.Fk(t, this.bmB, 50);
}
}
}
};
return e;
}(u.$iZ.$jj);
i.$jE = d;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DOO": "$DOO",
"../component/$DPL": "$DPL",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS"
} ],
$DPV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e9472Qo3VBNVZknw44dLUGI", "$DPV");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jF = void 0;
var o = t("../$KL"), a = t("../math/$DPR"), u = t("../lib/$DPP"), b = t("../component/$DOO"), h = t("../component/$DOP"), l = t("../component/$DOR"), f = t("../component/$DOY"), c = t("../component/$DPJ"), s = t("../component/$DPN"), p = t("../component/$DPO"), d = t("../math/$DPS"), v = t("../component/$DPL"), $ = t("../../../game/src/battle/types/$DYS"), m = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmE = new d.default();
e.bmF = new d.default();
return e;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return u.$iZ.allOf(f.default, s.default, p.default, b.default, h.default, l.default, c.default);
};
e.prototype.update = function(t) {
if (this.context.blc.gs().bhi.canCtrl) for (var e = 0, i = t; e < i.length; e++) {
o = i[e];
this.bmG(o);
} else for (var n = 0, r = t; n < r.length; n++) {
var o;
(o = r[n]).hx.bjX && (o.hx.bjN = 0);
}
};
e.prototype.bmG = function(t) {
var e, i, n = t.hv;
if (o.default.Ga(n)) {
var r = t.hx;
if (r.bjX) {
var u = t.hi.blc, b = t.hi.bln(v.default), h = t.hE, l = (h.bhA, t.hw), f = t.hI, c = f.bfQ, s = t.hy, p = (a.$jB.bma(this.dt * s.worldTimeScale * s.hitPauseTimeScale * s.mainTimeScale, 4), 
this.context.bll(t.hG.bgj)), d = t.hG, m = this.bmE;
m.x = l.x;
m.y = l.y;
var y = c.move;
if (!c.playerAI && y) {
f.bfV = a.$jB.bma(this.dt + (f.bfV || 0), 4);
if (f.bfV >= y.time) {
f.bfV = 0;
var g = y.param, D = p ? p.hw : null, j = this.bmF;
if (D) {
j.x = D.x;
j.y = D.y;
} else {
j.x = a.$jB.bma(m.x + 800 * a.$jB.bma(u.ge() - .5));
j.y = a.$jB.bma(m.y + 800 * a.$jB.bma(u.ge() - .5));
}
var P = m.x - j.x, E = j.x + (g[0] + a.$jB.bma((g[1] - g[0]) * u.ge(), 3)) * Math.sign(P);
E = Math.max(Math.min(E, b.bkm), b.bkl);
var x = u.ge() > .5 ? 1 : -1, O = j.y + (g[2] + a.$jB.bma((g[3] - g[2]) * u.ge(), 3)) * x;
O = Math.max(Math.min(O, b.bko), b.bkn);
f.bfR.x = E;
f.bfR.y = O;
}
var k = y.lockY, _ = y.lockX, w = n.beA > 0, F = f.bfR.x - m.x, B = f.bfR.y - m.y, M = 0, I = 0;
!_ && Math.abs(F) > 30 && (M = .6 * Math.sign(F));
!k && Math.abs(B) > 30 && (I = .6 * Math.sign(B));
if (0 === M && 0 === I || w) r.bjN = 0; else {
r.bjN = 1;
r.bjO.x = M;
r.bjO.y = I;
r.bjP = 0;
}
if (!w && h.bho && (null === (e = h.bhw.aiChangeDir) || void 0 === e || e) && (h.bho.changeDir || h.bho.idle || h.bho.move)) {
var R = d.bgk.x;
R && -1 === Math.sign(R * t.hw.bkw) && (t.hw.bkw *= -1);
}
}
var Y = c.acts;
f.bfT = a.$jB.bma(this.dt + (f.bfT || 0), 4);
if (Y && f.bfW) {
var T = c.actTime;
if (f.bfT >= T) {
for (var C = Math.abs(d.bgk.x), G = Math.abs(d.bgk.y), S = function(e) {
if (f.bfT >= T) {
var r = e.area;
if (!(C >= r[0] && C < r[1])) return "continue";
if (!(G >= r[2] && G < r[3])) return "continue";
var a = n.bei(e.cmdId);
if (a && o.default.Ff(t, a)) {
if (!(null === (i = t.hE.bhr) || void 0 === i ? void 0 : i.find(function(t) {
if (!t.tgs) return !1;
var i = t.tgs[0];
return !!i && (i[0] === $.$uo.$vf.c && i[1][0] === e.cmdId);
}))) return "continue";
if (!(e.rate >= u.ge())) {
a.cd = Math.max(e.commonCD, 0);
return "continue";
}
a.cd = a.maxCdTime;
f.bfT = 0;
f.bfU = 0;
var b = o.default.FH(t.hx, e.cmdId);
b && (b.c = 1);
}
}
}, A = 0, X = Y; A < X.length; A++) S(X[A]);
f.bfT = 0;
}
}
}
}
};
return e;
}(u.$iZ.$jj);
i.$jF = m;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DOO": "$DOO",
"../component/$DOP": "$DOP",
"../component/$DOR": "$DOR",
"../component/$DOY": "$DOY",
"../component/$DPJ": "$DPJ",
"../component/$DPL": "$DPL",
"../component/$DPN": "$DPN",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS"
} ],
$DPW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "eb186ls4f5K5LHPjXxGpATY", "$DPW");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jG = void 0;
var o = t("../math/$DPR"), a = t("../lib/$DPP"), u = t("../component/$DOR"), b = t("../component/$DPA"), h = t("../component/$DPO"), l = t("../math/$DPS"), f = t("../../../game/src/battle/types/$DYS"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bmH = new l.default();
e.bmI = new l.default();
return e;
}
e.prototype.filter = function() {
return a.$iZ.allOf(h.default, b.default, u.default);
};
e.prototype.update = function(t) {
for (var e, i, n, r = this.context.blc.fu.bkN, a = 0, u = t; a < u.length; a++) {
var b = u[a], h = b.hG, l = b.hD.biI, c = b.hw, s = null;
switch (h.bgn) {
case f.$uo.$uS.b:
var p = Number.MAX_SAFE_INTEGER;
if (b.hO) {
for (var d = 0, v = r; d < v.length; d++) if ((D = (g = v[d]).hv) && D.bez && g.hD.biI !== l) {
if (D.ber) continue;
var $ = g.hE;
if ($) {
if (!0 === (null === (n = $.bhw) || void 0 === n ? void 0 : n.noAutoAim)) continue;
if (!$.biw()) continue;
}
this.bmH.x = o.$jB.bma(g.hw.x - c.x, 3);
this.bmH.y = o.$jB.bma(g.hw.y - c.y, 3);
if ((j = this.bmH.bmt()) < p) {
p = j;
s = g;
h.bgk.bmn(this.bmH.x, this.bmH.y);
}
}
} else for (var m = 0, y = r; m < y.length; m++) {
var g, D;
if ((D = (g = y[m]).hv) && D.bez && g.hD.biI !== l) {
if (!0 === (null === (i = null === (e = g.hE) || void 0 === e ? void 0 : e.bhw) || void 0 === i ? void 0 : i.noAutoAim)) continue;
this.bmH.x = o.$jB.bma(g.hw.x - c.x, 3);
this.bmH.y = o.$jB.bma(g.hw.y - c.y, 3);
var j;
if ((j = this.bmH.bmt()) < p) {
p = j;
s = g;
h.bgk.bmn(this.bmH.x, this.bmH.y);
}
}
}
break;

case f.$uo.$uS.c:
if (s = this.context.bll(h.bgj)) {
this.bmH.x = s.hw.x;
this.bmH.y = s.hw.y;
h.bgk.x = o.$jB.bma(this.bmH.x - c.x, 3);
h.bgk.y = o.$jB.bma(this.bmH.y - c.y, 3);
}
}
if (s) {
h.bgj !== s.hg && (h.bgj = s.hg);
0 === h.bgk.x && 0 === h.bgk.y && (h.bgk.x = 1);
h.bgm = o.$jB.bma(h.bgk.bmr(), 3);
h.bgl.bmn(h.bgk.x, h.bgk.y);
o.$jB.blZ(h.bgl);
}
}
};
return e;
}(a.$iZ.$jj);
i.$jG = c;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DOR": "$DOR",
"../component/$DPA": "$DPA",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS"
} ],
$DPX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ee20fRe8b5GR5whwhWLLvfA", "$DPX");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jJ = i.$jI = i.$jH = void 0;
var o, a = t("../$KL"), u = t("../math/$DPR"), b = t("../lib/$DPP"), h = t("../component/$DOS"), l = t("../component/$DPB"), f = t("../component/$DPO"), c = t("../../../game/src/battle/types/$DYS"), s = t("../utils/$DQI"), p = t("../component/$DOU"), d = t("../../../game/src/battle/$DSO");
(function(t) {
t[t.c = 0] = "c";
t[t.e = 1] = "e";
t[t.h = 2] = "h";
t[t.i = 3] = "i";
})(o || (o = {}));
var v = function() {
this.bmJ = -1;
this.bmK = 0;
this.bmL = 0;
this.bmM = o.c;
this.bmN = 0;
this.bmO = 0;
this.bmP = 0;
this.bmQ = 0;
this.bmR = 0;
this.bmS = 0;
};
i.$jH = v;
var $ = function() {
this.bmT = -1;
this.bmU = 0;
this.bmV = [];
};
i.$jI = $;
var m = function(t, e) {
return e.bmN - t.bmN;
}, y = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.fixedUpdate = !0;
e.bmW = new Map();
e.bmX = [];
e.bmY = [];
e.bmZ = [];
e.bna = [];
e.bnb = new Map();
e.bnc = [];
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {
this.bmW.clear();
this.bmX.length = 0;
this.bmY.length = 0;
this.bmZ.length = 0;
this.bna.length = 0;
this.bnb.clear();
this.bnc.length = 0;
};
e.prototype.bnd = function(t, e) {
var i = this.bnb.get(t);
if (void 0 === i) {
this.bnc.push(t);
this.bnb.set(t, e);
} else i < e && this.bnb.set(t, e);
};
e.prototype.filter = function() {
return b.$iZ.allOf(f.default, l.default, h.default);
};
e.prototype.bne = function(t) {
var e = this.bmW.get(t);
if (!e) {
var i = this.bmZ;
i.length > 0 ? (e = i.pop()).bmV.length = 0 : e = new $();
e.bmT = t;
e.bmU = 0;
this.bmX.push(e);
this.bmW.set(t, e);
}
return e;
};
e.prototype.bnf = function(t, e, i, n) {
var r, o = this.bna;
(r = o.length > 0 ? o.pop() : new v()).bmJ = e;
r.bmK = 0;
r.bmL = i;
r.bmN = i;
r.bmM = n;
r.bmO = 0;
r.bmP = 0;
r.bmQ = 0;
r.bmR = 0;
r.bmS = 0;
t.bmV.push(r);
return r;
};
e.prototype.bng = function() {
for (var t = this.bmX, e = this.bna, i = this.bmZ, n = 0, r = t.length; n < r; n++) {
for (var o = t[n], a = o.bmV, u = 0, b = a.length; u < b; u++) {
var h = a[u];
h.bmP = 0;
h.bmQ = 0;
e.push(h);
}
a.length = 0;
i.push(o);
}
t.length = 0;
};
e.prototype.update = function(t) {
var e, i, n, r, b, h, l = this.context, f = l.blc, v = this.dt, $ = f.ft, y = this.bnb, g = this.bnc;
y.clear();
g.length = 0;
var D = this.bmX, j = this.bmY;
D.length = 0;
j.length = 0;
this.bmW.clear();
for (var P = 0, E = !1, x = 0, O = t; x < O.length; x++) {
var k = O[x];
if (l.bll(k.hg)) {
var _ = k.hA;
if (_ && _.biS) {
var w = k.hB;
if (w && w.bgp === c.$uo.$vg.b && (ee = w.bgq) && (ie = l.bll(w.bgt))) {
var F = l.bll(w.bgu);
if (F && F.hD && F.hv) {
var B = ie.hy, M = F.hv, I = F.hE, R = F.hg, Y = ie.hg, T = ie.hw;
P = 0;
if (M.bez) {
if (ee.hitCount > 1 && w.bgx < ee.hitCount - 1) {
var C = u.$jB.bma(v * B.bkv, 4);
w.bgz += C;
if (w.bgz > ee.hitInterval) {
w.bgx++;
w.bgz = u.$jB.bma(w.bgz - ee.hitInterval, 4);
w.bgy = 0;
w.bgv = {};
}
}
for (var G = 0, S = _.biQ; G < S.length; G++) {
var A = S[G], X = l.bll(A);
if (X) {
var N = void 0, z = X.ho(p.default);
if (z && (N = l.bll(z.bgK))) {
var Z = N.hg, L = N.hD;
if (L && L.biI !== w.bgs && Z !== Y) if (N.hv) {
if (N.hw && !w.bgv[Z]) {
var K = N.hv, Q = N.hw, V = N.hE;
if (V && K.bez && !K.ber) {
w.bgv[Z] = 1;
if (!w.bgy) {
w.bgy = 1;
w.bgw += 1;
}
T.x, Q.x;
var J = (Q.bkw, a.default.Ef(F, ie, N));
j.push(J);
J.hitInfo = ee;
J.isPhysical = 2 !== ee.damageType;
J.elementValue = M.bfA(null !== (e = ee.elementValue) && void 0 !== e ? e : 0);
J.beforeHP = K.beu;
var H = w.bgA, U = H > 0 ? a.default.FV(f, J, H, !0) : 0;
J.finalDamage = U;
var W = X.hA.biP.biL[0].world.blH, q = W.x, tt = W.y, et = W.x + W.width, it = W.y + W.height, nt = _.biP.biL, rt = nt[0].world.blH;
if (nt.length > 1) for (var ot = 0, at = nt; ot < at.length; ot++) {
var ut = at[ot].world.blH;
if (ut.x + ut.width >= q && et >= ut.x) {
rt = ut;
break;
}
}
var bt = rt.x, ht = rt.y, lt = rt.x + rt.width, ft = rt.y + rt.height, ct = Math.max(q, bt), st = Math.max(tt, ht), pt = Math.min(et, lt), dt = Math.min(it, ft), vt = u.$jB.bma((ct + pt) / 2), $t = u.$jB.bma((st + dt) / 2);
J.hitX = vt;
J.hitY = Q.y;
J.hitZ = $t;
if (V.bhu[c.$uo.$vh.i]) {
a.default.Gv(N, c.$uo.$vh.i, J, 0);
for (var mt = !1, yt = 0, gt = V.bhs; yt < gt.length; yt++) if ((xt = gt[yt]).Dq.event === c.$uo.$vh.i && 1 === xt.DF) {
mt = !0;
var Dt = 0;
xt.DG > 0 && (Dt = Math.floor(u.$jB.bma(U * (xt.DG / d.GameAttributeConstRatioRange))));
if (xt.DH > 0) {
var jt = a.default.Ef(N, N, F);
jt.isPhysical = K.bfc > K.bfd;
jt.elementValue = 0;
Dt += a.default.FV(f, jt, xt.DH / d.GameAttributeConstRatioRange, !1);
a.default.Eg(jt);
}
if (!M.ber && M.bez) {
F.hX(F, Dt);
!M.bez && a.default.GJ(F, 0, 0);
f.gu(c.$uo.$vk.E, F, Dt, !1);
}
break;
}
if (mt) {
J.finalDamage = 0;
U = 0;
}
}
if (J.elementValue & c.$uo.$uJ.b && K.beT) for (var Pt = 0, Et = V.bhs.filter(function(t) {
return t.Dq.abnormalType === c.$uo.$va.c;
}); Pt < Et.length; Pt++) {
var xt = Et[Pt];
a.default.FP(N, xt, !1);
}
if (U > 0) {
N.hX(F, U, J);
(null == I ? void 0 : I.bhu[c.$uo.$vh.h]) && a.default.Gv(F, c.$uo.$vh.h, J, U);
}
if (H > 0) {
M.bep += 1;
M.comboTime = 0;
this.postEvent(c.$uo.$vk.z, F);
K.bep = 0;
K.comboTime = 0;
this.postEvent(c.$uo.$vk.z, N);
}
var Ot = ie.hE, kt = null !== (n = null === (i = V.bho) || void 0 === i ? void 0 : i.superArmor) && void 0 !== n ? n : 0;
this.bnd(Y, ee.p1PauseTime);
2 !== kt && this.bnd(Z, ee.p2PauseTime);
this.postEvent(c.$uo.$vk.B, J);
K.bem++;
var _t = K.bez, wt = !Q.bkA, Ft = K.beS;
E = -1 !== V.bhJ;
if (ee.grab && (ee.grab.level || !N.im && !Ft) && !E && Ot.bhI.length + P < ee.grab.count) if (_t) {
E = !0;
P += 1;
(Mt = this.bne(Z)).bmU = 1;
(Rt = this.bnf(Mt, R, 1e3, o.h)).bmO = Y;
Rt.bmP = ee.grab;
} else {
var Bt = a.default.Fz(ie, N, c.$uo.$uZ.g), Mt = this.bne(Z);
(Rt = this.bnf(Mt, R, 1e4, o.i)).bmO = u.$jB.bma(400 * Bt, 3);
Rt.bmP = 600;
} else if (!(K.beq > (ee.breakSuperArmor || 0) || E || Ft) || !_t) if (ee.air && !V.bim && (ee.air.fly || wt && !V.bin || V.bhT)) {
Mt = this.bne(Z);
(Rt = this.bnf(Mt, R, 1, o.e)).bmO = a.default.Fz(ie, N, ee.changeDirType);
Rt.bmP = a.default.Fz(ie, N, ee.air.hitDirType);
Rt.bmQ = ee;
} else if (ee.ground) {
var It = V.bio;
if (_t || N.hR) if (It) {
if (!_t) {
Mt = this.bne(Z);
this.bnf(Mt, R, 1e4, o.i);
}
} else {
Bt = a.default.Fz(ie, N, ee.ground.hitDirType), Mt = this.bne(Z);
(Rt = this.bnf(Mt, R, 0, o.c)).bmO = a.default.Fz(ie, N, ee.changeDirType);
Rt.bmP = ee.ground.slideTime;
Rt.bmQ = ee.ground.time;
Rt.bmR = u.$jB.bma(Bt * ee.ground.force, 3);
Rt.bmS = ee.ground.blockVal;
} else {
Bt = a.default.Fz(ie, N, c.$uo.$uZ.g), Mt = this.bne(Z);
var Rt = this.bnf(Mt, R, 1e4, o.i);
if (!It && !V.bim) {
Rt.bmO = u.$jB.bma(400 * Bt, 3);
Rt.bmP = 600;
}
}
} else if (!_t) {
Bt = a.default.Fz(ie, N, c.$uo.$uZ.g), Mt = this.bne(Z);
(Rt = this.bnf(Mt, R, 1e4, o.i)).bmO = u.$jB.bma(400 * Bt, 3);
Rt.bmP = 600;
}
}
}
} else {
if (w.bgv[Z]) continue;
w.bgv[Z] = 1;
ee.lg && a.default.FM(F, null, ee.lg, ee);
a.default.Gu(N, null, c.$uo.$vh.d, ee);
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
for (var Yt = 0, Tt = $.bkN; Yt < Tt.length; Yt++) {
var Ct = Tt[Yt].hy;
if (1 !== Ct.hitPauseTimeScale) {
Ct.bku = u.$jB.bma(Ct.bku - v * Ct.worldTimeScale, 4);
if (Ct.bku <= 0) {
Ct.bku = 0;
Ct.hitPauseTimeScale = 1;
}
}
}
for (var Gt = 0, St = g.length; Gt < St; Gt++) {
var At = g[Gt], Xt = l.bll(At);
Xt && a.default.FK(Xt.hy, 0, y.get(At));
}
for (var Nt = 0, zt = D; Nt < zt.length; Nt++) for (var Zt = 0, Lt = (he = zt[Nt]).bmV; Zt < Lt.length; Zt++) {
var Kt = Lt[Zt];
if (this.bmW.has(Kt.bmJ)) {
Kt.bmK = 1;
Kt.bmN = Kt.bmL - 1e3;
}
}
for (var Qt = 0, Vt = D; Qt < Vt.length; Qt++) {
var Jt = (he = Vt[Qt]).bmV;
Jt.length > 1 && s.default.bns(Jt, m, 0, Jt.length);
var Ht = Jt[0];
switch (Ht.bmM) {
case o.c:
if (!(N = l.bll(he.bmT))) break;
if (!(te = N.hv)) break;
if (!te.bez && N.hR) a.default.GK(N); else {
var Ut = te.beW, Wt = Ht.bmQ;
Ut && (Wt = u.$jB.bma(Wt * (1e3 / (Ut + 1e3))));
a.default.EL(N, Ht.bmP, Math.max(Wt, .05), c.$uo.commonHitActions[Math.floor(2 * f.ge())]);
N.hw.bkw = Ht.bmO;
if (!N.hE.bif) {
var qt = N.hz;
a.default.Fm(qt, Ht.bmR, 0, 0);
qt.bjg = Ht.bmS;
N.ih();
}
}
break;

case o.e:
var te;
if (!(N = l.bll(he.bmT))) break;
if (!(te = N.hv)) break;
N.hw.bkw = Ht.bmO;
if (!te.bez && N.hR) a.default.GK(N); else {
var ee = Ht.bmQ;
a.default.EZ(N, ee.air, Ht.bmP);
}
break;

case o.h:
var ie;
if (!(N = l.bll(he.bmT))) break;
if (!(ie = l.bll(Ht.bmO))) break;
var ne = N.hE;
if (Ht.bmK) {
var re = N.hw;
if (!((null === (r = N.hv) || void 0 === r ? void 0 : r.beS) || ne.bim)) {
ne.bhT = 1;
a.default.EZ(N, {
z: 320,
hitDirType: 0,
force: 300
}, -1 * re.bkw);
}
} else {
if (!(Ot = ie.hE)) break;
ne.bhJ = ie.hg;
Ot.bhI.push({
id: Ht.bmP.id,
eid: N.hg
});
a.default.EN(N);
a.default.EY(ie, N);
N.ig();
}
break;

case o.i:
if (!(N = l.bll(he.bmT))) break;
a.default.GJ(N, Math.abs(Ht.bmO), Ht.bmP);
}
}
for (var oe = 0, ae = j; oe < ae.length; oe++) {
(Kt = ae[oe]).hitInfo.lg && a.default.FM(Kt.atkEntity, null, Kt.hitInfo.lg, Kt);
Kt.hitInfo.lg2 && a.default.FM(Kt.fromEntity, null, Kt.hitInfo.lg2, Kt);
(null === (b = Kt.atkEntity.hE) || void 0 === b ? void 0 : b.bhu[c.$uo.$vh.c]) && a.default.Gu(Kt.atkEntity, null, c.$uo.$vh.c, Kt);
(null === (h = Kt.target.hE) || void 0 === h ? void 0 : h.bhu[c.$uo.$vh.d]) && a.default.Gu(Kt.target, null, c.$uo.$vh.d, {
target: Kt.atkEntity
});
}
for (var ue = 0, be = j; ue < be.length; ue++) {
var he = be[ue];
a.default.Eg(he);
}
j.length = 0;
this.bng();
};
return e;
}(b.$iZ.$jj);
i.$jJ = y;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../game/src/battle/$DSO": void 0,
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DOS": "$DOS",
"../component/$DOU": "$DOU",
"../component/$DPB": "$DPB",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../utils/$DQI": "$DQI"
} ],
$DPY: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "cd180imBE9POIWoGVJmytuq", "$DPY");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jL = void 0;
var o = t("../$KL"), a = t("../lib/$DPP"), u = t("../component/$DPB"), b = t("../component/$DOU"), h = t("../component/$DOV"), l = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.fixedUpdate = !0;
e.bnh = [];
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return a.$iZ.allOf(u.default, h.default);
};
e.prototype.update = function(t) {
for (var i, n = this.context, r = 0, a = t; r < a.length; r++) {
var u = a[r];
if (n.bll(u.hg)) {
var h = u.hM;
if (h) if (null === (i = u.hA) || void 0 === i ? void 0 : i.biS) {
var l = h.bgM;
if (l) {
this.bnh.length = 0;
var f = n.bll(h.bgL);
if (f && f.hD) {
for (var c = 0, s = u.hA.biQ; c < s.length; c++) {
var p = s[c], d = n.bll(p);
if (d) {
var v, $ = d.ho(b.default);
if ($ && (null == (v = n.bll($.bgK)) ? void 0 : v.hE) && (!l.isActor || v.hv)) {
if ("boolean" == typeof l.isPlayerCamp) {
if (!v.hD) continue;
var m = this.context.blc.fF[0].cXQ;
if (v.hD.biI === m !== l.isPlayerCamp) continue;
}
if ("boolean" == typeof l.isSelfCamp) {
if (!v.hD) continue;
if (v.hD.biI === f.hD.biI !== l.isSelfCamp) continue;
}
var y = v.hg;
this.bnh.push(y);
if (-1 === h.bgN.indexOf(y)) {
h.bgN.push(y);
for (var g = 0, D = l.buffs; g < D.length; g++) {
var j = D[g];
o.default.FN(v, j, null, f.hg);
}
}
}
}
}
for (var P = h.bgN.length - 1; P >= 0; --P) {
y = h.bgN[P];
if (-1 === this.bnh.indexOf(y)) {
h.bgN.splice(P, 1);
var E = n.bll(y);
if (null == E ? void 0 : E.hE) for (var x = 0, O = l.buffs; x < O.length; x++) {
j = O[x];
o.default.FQ(E, j);
}
}
}
}
}
} else e.bni(u);
}
}
};
e.bni = function(t) {
var e = t.hM, i = e.bgM.buffs;
if ((null == i ? void 0 : i.length) > 0 && e.bgN.length > 0) {
for (var n = 0, r = e.bgN; n < r.length; n++) {
var a = r[n], u = t.hi.bll(a);
if (null == u ? void 0 : u.hE) for (var b = 0, h = i; b < h.length; b++) {
var l = h[b];
o.default.FQ(u, l);
}
}
e.bgN.length = 0;
}
};
return e;
}(a.$iZ.$jj);
i.$jL = l;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../component/$DOU": "$DOU",
"../component/$DOV": "$DOV",
"../component/$DPB": "$DPB",
"../lib/$DPP": "$DPP"
} ],
$DPZ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "34a68YxPn1Kh5Pp+o/+l97i", "$DPZ");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jM = void 0;
var o = t("../math/$DPR"), a = t("../lib/$DPP"), u = t("../component/$DOX"), b = t("../component/$DPN"), h = t("../BattleBehaviorLogic"), l = t("../../../game/src/battle/types/$DYQ"), f = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return a.$iZ.allOf(u.default, b.default);
};
e.prototype.update = function(t) {
var e = this.context.blc, i = t[0].hH, n = t[0].hy, r = o.$jB.bma(this.dt * n.hitPauseTimeScale * n.worldTimeScale * n.mainTimeScale, 4);
i.bhj = i.bhk;
i.bhk = o.$jB.bma(i.bhk + r, 4);
if (i.bhe.duration) {
i.bhe.time = o.$jB.bma(i.bhe.time + this.dt);
if (i.bhe.time >= i.bhe.duration) {
e.go(1);
i.bhn(0);
}
}
if (i.bhi.time) {
e.fk = o.$jB.bma(e.fk + r, 4);
e.fM.f === l.$uj.e && e.fF.forEach(function(t) {
t.cYe = o.$jB.bma(t.cYe + r, 4);
});
}
for (var a = i.bhh, u = i.bhl, b = 0; b < a.length; b++) {
var f = a[b];
if (void 0 === f.tts) {
if (u[b] >= 1) continue;
} else if (0 === f.tts) ; else if (u[b] >= f.tts) continue;
for (var c = !0, s = 0; s < f.tgs.length; s++) {
var p = f.tgs[s], d = h.default.CT(t[0], null, p, null);
if (p[2] ? d : !d) {
c = !1;
break;
}
}
if (c) {
u[b] ? u[b] += 1 : u[b] = 1;
if (1 === h.default.CU(t[0], null, f, null)) break;
}
}
};
return e;
}(a.$iZ.$jj);
i.$jM = f;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYQ": void 0,
"../BattleBehaviorLogic": "BattleBehaviorLogic",
"../component/$DOX": "$DOX",
"../component/$DPN": "$DPN",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR"
} ],
$DQA: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "11e79yFhy9Ps7Xi76E5e4qC", "$DQA");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jN = void 0;
var o = t("../math/$DPR"), a = t("../lib/$DPP"), u = t("../$KL"), b = t("../component/$DOY"), h = t("../component/$DPN"), l = t("../component/$DPO"), f = t("../../../game/src/battle/types/$DYS"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bnj = [];
return e;
}
e.prototype.filter = function() {
return a.$iZ.allOf(b.default, h.default, l.default);
};
e.prototype.update = function(t) {
for (var e = this.context.blc.fi, i = 0, n = t; i < n.length; i++) {
var r = n[i], a = r.hE, b = r.hw, h = r.hy;
if (0 !== h.worldTimeScale) {
var l = o.$jB.bma(this.dt * h.worldTimeScale, 4), c = a.bhs, s = c.length;
if (s > 0) {
var p = this.bnj;
p.length = s;
for (var d = 0; d < s; d++) {
(v = c[d]).Dz = e;
p[d] = v;
}
for (d = 0; d < s; d++) {
var v;
if ((v = p[d]).Dz === e) {
var $ = v.Dq;
v.Dw = v.Dx;
v.Dx = o.$jB.bma(l + v.Dx, 4);
if ($.updateLg) {
var m = v.Dv;
if (0 === m) u.default.FM(r, v, $.updateLg, null); else {
v.Dy = o.$jB.bma(l + v.Dy, 4);
if (v.Dy >= m) {
v.Dy = o.$jB.bma(v.Dy - m, 4);
u.default.FM(r, v, $.updateLg, null);
}
}
}
$.abnormalType > f.$uo.$va.j && u.default.FT(r, v, l);
v.Dz === e && 0 !== $.duration && v.Dx >= v.Du && u.default.FP(r, v, !1);
}
}
if (r.hQ()) continue;
}
var y = o.$jB.bma(this.dt * h.bkv, 4);
a.bhB = a.bhC;
a.bhC = o.$jB.bma(a.bhC + y, 4);
if (a.bib) {
var g = o.$jB.bma(y * h.actionTimeScale, 4);
a.bhX = o.$jB.bma(a.bhX + g, 4);
if (a.bib.loop && a.bhX >= a.bhY.b) {
a.bhX = 0;
for (var D = 0, j = a.bic; D < j.length; D++) (F = j[D]) && (F.d = 0);
a.bid.b = 0;
}
for (var P = a.bid, E = P.c, x = P.b + 1; E[x] && a.bhX >= E[x].region[0]; ) {
P.b += 1;
x = P.b + 1;
}
var O = E[P.b].anim, k = a.bhY;
k.f = O.x;
k.g = O.y;
k.h = O.a;
}
for (var _ = 0, w = a.bic; _ < w.length; _++) {
var F;
if (F = w[_]) {
var B = this.context.bll(F.f);
if (B) {
for (E = F.g, x = F.d + 1; E[x] && a.bhX >= E[x].region[0]; ) {
F.d += 1;
x = F.d + 1;
}
u.default.Fq(B.hw, b);
var M = E[F.d].collider;
M && F.e ? u.default.FX(B.hA, M) : B.hA.biS = 0;
}
}
}
if (a.bhO.hurtbackTime > 0) {
var I = a.bhO;
I.hurtbackTime = o.$jB.bma(I.hurtbackTime - y, 4);
if (I.hurtbackTime <= 0) {
I.hurtbackTime = 0;
switch (I.hurtbackType) {
case f.$uo.$uN.b:
u.default.Fm(r.hz, 0, 0, 0);
u.default.EC(r);
break;

case f.$uo.$uN.c:
r.hz && u.default.Fm(r.hz, 0, 0, 0);
break;

case f.$uo.$uN.e:
break;

case f.$uo.$uN.h:
case f.$uo.$uN.f:
u.default.Fc(r);
r.hU || u.default.GK(r);
}
} else switch (I.hurtbackType) {
case f.$uo.$uN.b:
if (I.slideTime > 0) {
I.slideTime = o.$jB.bma(I.slideTime - y, 4);
if (I.slideTime <= 0) {
I.slideTime = 0;
u.default.Fm(r.hz, 0, 0, 0);
}
}
break;

case f.$uo.$uN.e:
if (r.hw) {
u.default.FU(r);
if (r.hw.z <= 0) {
r.hE.bhV = 0;
if (r.hE.bhU) {
r.hE.bhT = 1;
r.hE.bhW = 1;
r.hz.bjb = r.hE.bhU;
r.hE.bhU = 0;
} else if (r.hE.bhT > 1) {
u.default.Fm(r.hz, 0, 0, 0);
u.default.Fa(r, f.$uo.LIE_TIME);
} else {
r.hE.bhT = 2;
r.hw.z = 1;
var R = -o.$jB.bma(.5 * r.hz.bjb * r.ij(), 4);
r.hz.bjb = R;
r.hz.biZ = o.$jB.bma(.5 * r.hz.biZ, 4);
}
}
}
}
}
for (var Y = !1, T = 0, C = a.bhN; T < C.length; T++) {
var G = C[T];
G.time = o.$jB.bma(G.time + l);
if (-1 !== G.destroyTime && G.time > G.destroyTime) {
G.time = -1;
Y = !0;
}
}
Y && (a.bhN = a.bhN.filter(function(t) {
return -1 !== t.time;
}));
if (a.bhP.type > 0) {
var S = o.$jB.bma(l * a.bhP.speed, 4);
a.bhP.currentTime = o.$jB.bma(a.bhP.currentTime + S, 4);
}
u.default.ER(r);
}
}
};
return e;
}(a.$iZ.$jj);
i.$jN = c;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DOY": "$DOY",
"../component/$DPN": "$DPN",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR"
} ],
$DQB: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "eedaeXsvllPu4q22TlKmtCU", "$DQB");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jO = void 0;
var o = t("../$KL"), a = t("../math/$DPR"), u = t("../lib/$DPP"), b = t("../component/$DOZ"), h = t("../component/$DPN"), l = t("../component/$DPO"), f = t("../math/$DPS"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bnk = new f.default();
return e;
}
e.prototype.filter = function() {
return u.$iZ.allOf(b.default, l.default, h.default);
};
e.prototype.update = function(t) {
for (var e = 0, i = 0, n = t; i < n.length; i++) {
var r = n[i], u = r.hJ, b = this.context.bll(u.bix), h = r.hw;
if (b && b.hw) {
var l = b.hw;
switch (u.biy) {
case 0:
var f, c = a.$jB.bma(u.biz * l.bkw + l.x, 3), s = a.$jB.bma(u.biA + l.y, 3);
f = "number" == typeof u.biD ? u.biD : a.$jB.bma(u.biB + l.z, 3);
e = u.biC ? b.hw.bkw * u.biC : 1;
o.default.Fp(h, c, s, f, e, h.bky);
break;

case 1:
var p = r.hy, d = a.$jB.bma(this.dt * p.worldTimeScale * p.hitPauseTimeScale * p.mainTimeScale, 4);
u.biG = a.$jB.bma(u.biG + a.$jB.bma(d * u.biE));
var v = a.$jB.bma(u.biG * Math.PI / 180);
h.x = a.$jB.bma(l.x + a.$jB.bma(Math.cos(v)) * u.biF);
h.y = a.$jB.bma(l.y + a.$jB.bma(Math.sin(v)) * u.biF);
}
}
}
};
return e;
}(u.$iZ.$jj);
i.$jO = c;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../component/$DOZ": "$DOZ",
"../component/$DPN": "$DPN",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS"
} ],
$DQC: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "3fafchsnI1CRpsnrF50/gor", "$DQC");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
var o = t("../lib/$DPP"), a = t("../math/$DPR"), u = t("../../../game/src/battle/types/$DYS"), b = t("../component/$DPB"), h = t("../component/$DPO"), l = t("../math/$DPQ"), f = t("../math/$DPS"), c = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bnl = new f.default();
e.bnm = new f.default();
return e;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return o.$iZ.allOf(h.default, b.default);
};
e.prototype.update = function(t) {
for (var e = this.bnl, i = this.bnm, n = 0, r = !1, o = 0, h = t; o < h.length; o++) {
var f = (W = h[o]).hA;
if (f.biS) {
r = f.biR && W.hw.bkw === u.$uo.$uU.c;
i.x = W.hw.x;
i.y = W.hw.y;
n = W.hw.z;
for (var c = f.biU, s = f.biO.biL, p = 0; p < s.length; p++) {
var d = (C = s[p]).shapeInfo, v = (G = C.world).blH;
if (d.t === u.$uo.$uX.c) {
var $ = d.ax, m = d.ay, y = d.w * c, g = d.h * c, D = G.blL, j = D[0], P = D[1], E = D[2], x = D[3];
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
j.x = e.x - $ * y;
j.y = e.y - m * g;
P.x = e.x + (1 - $) * y;
P.y = j.y;
E.x = P.x;
E.y = e.y + (1 - m) * g;
x.x = j.x;
x.y = E.y;
if (r) {
G.blK.x = -1 * G.blK.x + 2 * i.x;
j.x = -1 * j.x + 2 * i.x;
P.x = -1 * P.x + 2 * i.x;
E.x = -1 * E.x + 2 * i.x;
x.x = -1 * x.x + 2 * i.x;
}
var O = (A = j.x) < (X = P.x) ? A : X, k = A < X ? X : A, _ = (N = j.y) < (z = E.y) ? N : z, w = N < z ? z : N;
v.x = O;
v.y = _;
v.width = k - O;
v.height = w - _;
} else if (d.t === u.$uo.$uX.b) {
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
r && (G.blK.x = -1 * G.blK.x + 2 * i.x);
G.blJ = d.r * c;
v.x = G.blK.x - G.blJ;
v.y = G.blK.y - G.blJ;
v.width = 2 * G.blJ;
v.height = 2 * G.blJ;
} else if (d.t === u.$uo.$uX.d) {
(V = G.blI) || (V = G.blI = new l.$jx());
var F = a.$jB.bma(d.r * Math.PI / 180), B = ($ = d.ax, m = d.ay, y = d.w * c, g = d.h * c, 
G.blL);
j = B[0], P = B[1], E = B[2], x = B[3];
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
j.x = e.x - $ * y;
j.y = e.y - m * g;
P.x = e.x + (1 - $) * y;
P.y = j.y;
E.x = P.x;
E.y = e.y + (1 - m) * g;
x.x = j.x;
x.y = E.y;
if (r) {
G.blK.x = -1 * G.blK.x + 2 * i.x;
j.x = -j.x + 2 * i.x;
P.x = -P.x + 2 * i.x;
E.x = -E.x + 2 * i.x;
x.x = -x.x + 2 * i.x;
V.bls.x = -(y * (.5 - $) + e.x) + 2 * i.x;
F = -F;
} else V.bls.x = y * (.5 - $) + e.x;
V.bls.y = g * (.5 - m) + e.y;
a.$jB.blV(G.blK.x, G.blK.y, V.bls.x, V.bls.y, F, V.bls);
a.$jB.blV(G.blK.x, G.blK.y, j.x, j.y, F, j);
a.$jB.blV(G.blK.x, G.blK.y, P.x, P.y, F, P);
a.$jB.blV(G.blK.x, G.blK.y, E.x, E.y, F, E);
a.$jB.blV(G.blK.x, G.blK.y, x.x, x.y, F, x);
var M = Math.min(j.x, P.x, E.x, x.x), I = Math.min(j.y, P.y, E.y, x.y), R = Math.max(j.x, P.x, E.x, x.x), Y = Math.max(j.y, P.y, E.y, x.y);
v.x = M;
v.y = I;
v.width = R - M;
v.height = Y - I;
V.blA(V.bls.x, V.bls.y, y, g, F);
}
}
i.y = n;
var T = f.biP.biL;
for (p = 0; p < T.length; p++) {
var C, G;
d = (C = T[p]).shapeInfo, v = (G = C.world).blH;
if (d.t === u.$uo.$uX.c) {
$ = d.ax, m = d.ay, y = d.w * c, g = d.h * c;
var S = G.blL;
j = S[0], P = S[1], E = S[2], x = S[3];
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
j.x = e.x - $ * y;
j.y = e.y - m * g;
P.x = e.x + (1 - $) * y;
P.y = j.y;
E.x = P.x;
E.y = e.y + (1 - m) * g;
x.x = j.x;
x.y = E.y;
if (r) {
G.blK.x = -1 * G.blK.x + 2 * i.x;
j.x = -1 * j.x + 2 * i.x;
P.x = -1 * P.x + 2 * i.x;
E.x = -1 * E.x + 2 * i.x;
x.x = -1 * x.x + 2 * i.x;
}
var A, X, N, z, Z = (A = j.x) < (X = P.x) ? A : X, L = A < X ? X : A, K = (N = j.y) < (z = E.y) ? N : z, Q = N < z ? z : N;
v.x = Z;
v.y = K;
v.width = L - Z;
v.height = Q - K;
} else if (d.t === u.$uo.$uX.b) {
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
r && (G.blK.x = -1 * G.blK.x + 2 * i.x);
G.blJ = d.r * c;
v.x = G.blK.x - G.blJ;
v.y = G.blK.y - G.blJ;
v.width = 2 * G.blJ;
v.height = 2 * G.blJ;
} else if (d.t === u.$uo.$uX.d) {
var V;
(V = G.blI) || (V = G.blI = new l.$jx());
F = a.$jB.bma(d.r * Math.PI / 180), $ = d.ax, m = d.ay, y = d.w * c, g = d.h * c;
var J = G.blL;
j = J[0], P = J[1], E = J[2], x = J[3];
e.x = i.x + d.x;
e.y = i.y + d.y;
G.blK.bmn(e.x, e.y);
j.x = e.x - $ * y;
j.y = e.y - m * g;
P.x = e.x + (1 - $) * y;
P.y = j.y;
E.x = P.x;
E.y = e.y + (1 - m) * g;
x.x = j.x;
x.y = E.y;
if (r) {
G.blK.x = -1 * G.blK.x + 2 * i.x;
j.x = -j.x + 2 * i.x;
P.x = -P.x + 2 * i.x;
E.x = -E.x + 2 * i.x;
x.x = -x.x + 2 * i.x;
V.bls.x = -(y * (.5 - $) + e.x) + 2 * i.x;
F = -F;
} else V.bls.x = y * (.5 - $) + e.x;
V.bls.y = g * (.5 - m) + e.y;
a.$jB.blV(G.blK.x, G.blK.y, V.bls.x, V.bls.y, F, V.bls);
a.$jB.blV(G.blK.x, G.blK.y, j.x, j.y, F, j);
a.$jB.blV(G.blK.x, G.blK.y, P.x, P.y, F, P);
a.$jB.blV(G.blK.x, G.blK.y, E.x, E.y, F, E);
a.$jB.blV(G.blK.x, G.blK.y, x.x, x.y, F, x);
M = Math.min(j.x, P.x, E.x, x.x), I = Math.min(j.y, P.y, E.y, x.y), R = Math.max(j.x, P.x, E.x, x.x), 
Y = Math.max(j.y, P.y, E.y, x.y);
v.x = M;
v.y = I;
v.width = R - M;
v.height = Y - I;
V.blA(V.bls.x, V.bls.y, y, g, F);
}
}
}
}
for (var H = 0, U = t; H < U.length; H++) {
var W, q = (W = U[H]).hA;
if (q.biS) {
q.biQ.length = 0;
for (var tt = 0, et = t; tt < et.length; tt++) {
var it = et[tt], nt = it.hA;
if (nt.biS && it !== W && q.biT !== nt.biT) {
var rt = !1, ot = (s = q.biO.biL, nt.biO.biL);
for (p = 0; p < s.length; p++) {
var at = (yt = s[p]).shapeInfo, ut = yt.world;
if (!at.neg) {
for (var bt = 0; bt < ot.length; bt++) {
var ht = (gt = ot[bt]).shapeInfo, lt = gt.world;
if (!ht.neg && l.$jA.blS(ut.blH, lt.blH)) {
var ft = at.t === u.$uo.$uX.c, ct = ht.t === u.$uo.$uX.c, st = at.t === u.$uo.$uX.b, pt = ht.t === u.$uo.$uX.b, dt = at.t === u.$uo.$uX.d, vt = ht.t === u.$uo.$uX.d;
ft && ct ? rt = !0 : st && pt ? rt = b.circleCircle(ut, lt) : ft && pt ? rt = b.rectCircle(ut, lt) : st && ct ? rt = b.rectCircle(lt, ut) : dt && ct ? rt = l.$jA.blP(ut.blI, lt.blH) : dt && pt ? rt = l.$jA.blO(ut, lt, ut.blI.blt, ut.blI.blu, ut.blI.blv) : dt && vt ? rt = l.$jA.blM(ut.blI, lt.blI) : ft && vt ? rt = l.$jA.blP(lt.blI, ut.blH) : st && vt && (rt = l.$jA.blO(lt, ut, lt.blI.blt, lt.blI.blu, lt.blI.blv));
if (rt) break;
}
}
if (rt) break;
}
}
if (rt && !(q.biO.biM && b.IsCollider1InsideCollider2(nt, q) || nt.biO.biM && b.IsCollider1InsideCollider2(q, nt))) {
var $t = !1, mt = (T = q.biP.biL, nt.biP.biL);
for (p = 0; p < T.length; p++) {
var yt;
for (at = (yt = T[p]).shapeInfo, ut = yt.world, bt = 0; bt < mt.length; bt++) {
var gt;
ht = (gt = mt[bt]).shapeInfo, lt = gt.world;
if (l.$jA.blS(ut.blH, lt.blH)) {
ft = at.t === u.$uo.$uX.c, ct = ht.t === u.$uo.$uX.c, st = at.t === u.$uo.$uX.b, 
pt = ht.t === u.$uo.$uX.b, dt = at.t === u.$uo.$uX.d, vt = ht.t === u.$uo.$uX.d;
ft && ct ? $t = !0 : st && pt ? $t = b.circleCircle(ut, lt) : ft && pt ? $t = b.rectCircle(ut, lt) : st && ct ? $t = b.rectCircle(lt, ut) : dt && ct ? $t = l.$jA.blP(ut.blI, lt.blH) : dt && pt ? $t = l.$jA.blO(ut, lt, ut.blI.blt, ut.blI.blu, ut.blI.blv) : dt && vt ? $t = l.$jA.blM(ut.blI, lt.blI) : ft && vt ? $t = l.$jA.blP(lt.blI, ut.blH) : st && vt && ($t = l.$jA.blO(lt, ut, lt.blI.blt, lt.blI.blu, lt.blI.blv));
if ($t) break;
}
}
if ($t) break;
}
$t && q.biQ.push(it.hg);
}
}
}
}
}
};
return e;
}(o.$iZ.$jj);
i.default = c;
cc._RF.pop();
}, {
"../../../game/src/battle/types/$DYS": void 0,
"../component/$DPB": "$DPB",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPQ": "$DPQ",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS"
} ],
$DQD: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "fa3caM9OflApZ21k7DQ91dr", "$DQD");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jQ = void 0;
var o = t("../component/$DPD"), a = t("../lib/$DPP"), u = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return a.$iZ.allOf(o.$iN);
};
e.prototype.update = function(t) {
for (var e = 0, i = t; e < i.length; e++) {
var n = i[e];
this.context.blg(n);
}
};
return e;
}(a.$iZ.$jj);
i.$jQ = u;
cc._RF.pop();
}, {
"../component/$DPD": "$DPD",
"../lib/$DPP": "$DPP"
} ],
$DQE: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "8635dIz22BCKZ8Wddzxv8Wf", "$DQE");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jR = void 0;
var o = t("../$KL"), a = t("../math/$DPR"), u = t("../lib/$DPP"), b = t("../component/$DPF"), h = t("../component/$DPN"), l = t("../component/$DPO"), f = t("../component/$DOT"), c = t("../component/$DOU"), s = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.filter = function() {
return u.$iZ.allOf(l.default, h.default, b.default);
};
e.prototype.update = function(t) {
var e, i, n, r, u, b, h = this.dt;
this.bnn(this.context.blc, h);
for (var l, f = 0, c = 0, s = 0, p = 0, d = 0, v = 0, $ = !0, m = 0, y = !1, g = !1, D = 0, j = t; D < j.length; D++) {
var P = j[D];
if (0 !== (l = a.$jB.bma(h * P.hy.worldTimeScale * P.hy.hitPauseTimeScale * P.hy.hTimeScale, 4))) {
u = P.hw;
b = P.hz;
var E = P.hE;
g = (null === (e = P.hv) || void 0 === e ? void 0 : e.beB) > 0;
f = b.biZ;
c = b.bja;
s = b.bjb;
p = b.bjc;
d = b.bjd;
v = 0;
m = u.z;
if (y = E && E.bhR && o.default.ED(P)) {
var x = null !== (i = E.bhS.z) && void 0 !== i ? i : 600, O = a.$jB.bma(E.bhS.height - m);
s = Math.abs(O) > 8 ? O > 0 ? x : -x : 0;
} else v = !u.bkA && (null === (n = P.hv) || void 0 === n ? void 0 : n.bfa) && !(null == E ? void 0 : E.bit) ? a.$jB.bma(b.bje + a.$jB.bma(-(null === (r = P.hv) || void 0 === r ? void 0 : r.bfa) * E.bhW), 3) : b.bje;
$ = P.ic;
if (0 !== f || 0 !== p) {
if (!g) {
b.bjg > 10 && (p = -Math.sign(f) * b.bjg);
u.x = a.$jB.bma(u.x + f * l + p * l * l / 2, 3);
}
if (p) {
var k = b.biZ;
b.biZ = a.$jB.bma(b.biZ + p * l, 3);
Math.sign(b.biZ) !== Math.sign(k) && b.bjg > 10 && (b.biZ = 0);
}
}
if (0 !== c || 0 !== d) {
g || (u.y = a.$jB.bma(u.y + c * l + d * l * l / 2, 3));
d && (b.bja = a.$jB.bma(b.bja + d * l, 3));
}
if (0 !== s || 0 !== v) {
l = a.$jB.bma(h * P.hy.worldTimeScale * P.hy.hitPauseTimeScale * P.hy.vTimeScale, 4);
u.z = a.$jB.bma(u.z + s * l + v * l * l / 2, 3);
v && (b.bjb = a.$jB.bma(b.bjb + v * l, 3));
}
o.default.Fu(P, $);
if (y) {
E.bhS.height > m && E.bhS.height < u.z && (u.z = E.bhS.height);
E.bhS.height < m && E.bhS.height > u.z && (u.z = E.bhS.height);
}
}
}
};
e.prototype.bnn = function(t, e) {
for (var i = t.fr, n = 0, r = t.fC.bkN; n < r.length; n++) {
var o = r[n], u = o.ho(f.default);
if (u && i.bll(o.hg) && o.hA && o.hA.biS) {
var b = i.bll(u.bgF);
if (b && b.hD) for (var h = b.hD.biI, l = u.bgG, s = b.hw, p = 0, d = o.hA.biQ; p < d.length; p++) {
var v = d[p], $ = i.bll(v);
if ($) {
var m = $.ho(c.default);
if (m) {
var y = i.bll(m.bgK);
if (y && y.hD && y.hD.biI !== h) {
var g = y.hw;
if (g) {
var D = y.hy;
if (D) {
if (y.hv) {
var j = y.hv;
if (j.beB > 0) continue;
if (y.hE) {
if (y.hE.big) continue;
if (y.hE.bit) {
if (!u.bgG.inGrab) continue;
if (y.hE.bhJ !== u.bgF) continue;
}
}
if (j && j.ber) continue;
}
var P = a.$jB.bma(e * D.worldTimeScale * D.hitPauseTimeScale);
if (u.bgH) {
var E = a.$jB.bma(l.x * s.bkw + s.x, 3), x = a.$jB.bma(E - g.x), O = (_ = Math.sign(x)) * Math.min(_ * x, a.$jB.bma(u.bgH * P));
g.x = a.$jB.bma(g.x + O);
}
if (u.bgI) {
var k = a.$jB.bma(l.y + s.y, 3);
x = a.$jB.bma(k - g.y), O = (_ = Math.sign(x)) * Math.min(_ * x, a.$jB.bma(u.bgI * P));
g.y = a.$jB.bma(g.y + O);
}
if (u.bgJ) {
var _, w = a.$jB.bma(l.y + s.z, 3), F = (x = a.$jB.bma(w - g.z), O = (_ = Math.sign(x)) * Math.min(_ * x, a.$jB.bma(u.bgJ * P)), 
a.$jB.bma(g.z + O));
F < 0 && (F = 0);
g.z = F;
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
}(u.$iZ.$jj);
i.$jR = s;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../component/$DOT": "$DOT",
"../component/$DOU": "$DOU",
"../component/$DPF": "$DPF",
"../component/$DPN": "$DPN",
"../component/$DPO": "$DPO",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR"
} ],
$DQF: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4d80c2FLTlIrKiPFjyk3ofD", "$DQF");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jS = void 0;
var o = t("../$KL"), a = t("../math/$DPR"), u = t("../lib/$DPP"), b = t("../component/$DOO"), h = t("../component/$DOR"), l = t("../component/$DPJ"), f = t("../math/$DPS"), c = t("../component/$DPL"), s = t("../component/$DPH"), p = t("../component/$DPK"), d = t("../utils/$DQI"), v = t("../BattleBehaviorLogic"), $ = t("../../../main/src/$KV"), m = function(t, e) {
var i, n, r = t.c + (null !== (i = t.g.order) && void 0 !== i ? i : 0);
return e.c + (null !== (n = e.g.order) && void 0 !== n ? n : 0) - r;
}, y = function(t, e, i) {
return t >= i[0] && t < i[1] && e >= i[2] && e < i[3];
}, g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bno = new f.default();
e.bnp = new f.default();
return e;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return u.$iZ.allOf(s.default, b.default, h.default, l.default);
};
e.prototype.update = function(t) {
if (this.context.blc.gs().bhi.canCtrl) for (var e = 0, i = t; e < i.length; e++) {
o = i[e];
this.bnq(o);
} else for (var n = 0, r = t; n < r.length; n++) {
var o;
(o = r[n]).hx.bjX && (o.hx.bjN = 0);
}
};
e.prototype.bnq = function(t) {
var e, i, n, r, u, b, h, l, f, g, D = t.hv;
if (o.default.Ga(D)) {
var j = t.hx;
if (j.bjX) {
var P = t.hi.blc, E = t.hi.bln(c.default), x = t.hE;
if (x) {
x.bhA;
var O = t.hw, k = t.ho(s.default), _ = t.hy, w = (a.$jB.bma(this.dt * _.worldTimeScale * _.hitPauseTimeScale * _.mainTimeScale, 4), 
this.context.bll(t.hG.bgj)), F = null !== (e = null == w ? void 0 : w.hU) && void 0 !== e && e, B = (t.hG, 
this.bno);
B.x = O.x;
B.y = O.y;
var M = x.bho, I = null, R = P.fI;
if (R.cYz) if (R.cYD && !R.cYF) {
w = this.context.bll(R.cYE);
F = !0;
} else I = P.fE.bkN.find(function(t) {
return t.ho(p.default).bka === R.cYM;
});
var Y = k.bjq;
if (x.bhw.ai && 0 === Y.length) {
for (var T = 0, C = x.bhw.ai.acts; T < C.length; T++) {
var G = C[T];
(U = D.beg(G.id)) && o.default.FJ(t, U.Dg.id) && Y.push({
b: U.Dg.id,
f: U.Dg,
g: G,
c: U.Dg.upgradeLv,
d: 0,
h: 0,
i: 0
});
}
var S = D.bdU.find(function(t) {
return t.Dg.commonType === $.cvResUrl.commonSkillSquatCid;
});
S && Y.push({
b: S.Dg.id,
f: S.Dg,
g: {
id: S.Dg.id,
order: 1e4,
checkDir: 0,
area: [ 0, 99999, 0, 99999 ]
},
c: S.Dg.upgradeLv,
d: 0,
h: 0,
i: 0
});
d.default.bns(Y, m, 0, Y.length);
}
for (var A = new Set(), X = 0, N = x.bhr; X < N.length; X++) {
var z = N[X];
if (z.sid) {
if (!(U = D.beg(z.sid))) continue;
if (!o.default.Ff(t, U)) continue;
if (!o.default.Fj(t, z.tgs)) continue;
A.add(z.sid);
} else if (z.csid) {
if (!(U = D.beh(z.csid))) continue;
if (!o.default.Ff(t, U)) continue;
if (!o.default.Fj(t, z.tgs)) continue;
A.add(U.Dg.id);
}
}
if ((null == M ? void 0 : M.behit) && k.bjr && (lt = o.default.FH(t.hx, o.default.FJ(t, k.bjr.f.id)))) {
lt.b = 0;
k.bjr.h = 0;
k.bjr.i = 0;
k.bjr = null;
}
var Z = 9999, L = 9999;
if (w && (null === (i = w.hE) || void 0 === i ? void 0 : i.bhw)) {
Z = a.$jB.bma(w.hw.x + (null !== (r = null === (n = w.hE.bhw.aimAnchor) || void 0 === n ? void 0 : n.x) && void 0 !== r ? r : 0) - t.hw.x);
L = a.$jB.bma(w.hw.y + (null !== (b = null === (u = w.hE.bhw.aimAnchor) || void 0 === u ? void 0 : u.y) && void 0 !== b ? b : 0) - t.hw.y);
}
if (F && !j.bjM) {
var K = k.bjr;
if ((null == K ? void 0 : K.i) > 0) K.i = Math.max(a.$jB.bma(K.i - this.dt, 4), 0); else if ((null == K ? void 0 : K.h) > 0) K.h = Math.max(a.$jB.bma(K.h - this.dt, 4), 0); else if (!I) {
for (var Q = !1, V = [], J = 0, H = Y; J < H.length; J++) {
var U, W = H[J], q = W.g;
if ((U = D.beg(W.b)) && o.default.Ff(t, U)) {
if (q.tgs) {
for (var tt = q.tgs, et = !0, it = 0; it < tt.length; it++) {
var nt = tt[it], rt = v.default.CT(t, null, nt, null);
if (nt[2] ? rt : !rt) {
et = !1;
break;
}
}
if (!et) continue;
}
if (A.has(W.b)) {
V.push(W);
var ot = q.area, at = Math.abs(Z);
if (y(at, Math.abs(L), ot) && (!q.onlyElite || (null == w ? void 0 : w.hS))) {
if (0 !== q.checkDir && at > 50 && -1 === Math.sign(Z * t.hw.bkw)) {
if (!((null == M ? void 0 : M.changeDir) || M.move || M.idle)) continue;
t.hw.bkw *= -1;
}
if (k.bjr && k.bjr.b !== W.b) {
var ut = o.default.FH(t.hx, o.default.FJ(t, k.bjr.f.id));
ut && (ut.b = 0);
}
k.bjr = W;
Q = !0;
k.bjn = .2;
j.bjM || (j.bjN = 0);
var bt;
bt = U.Dg.commonType && U.Dg.cmdId ? U.Dg.cmdId : o.default.FJ(t, U.Dg.id);
if (lt = o.default.FH(t.hx, bt)) {
lt.c = 1;
if (null == q ? void 0 : q.chargeTime) {
W.h = q.chargeTime;
lt.b = 1;
}
}
break;
}
}
}
}
if (!Q) {
var ht = V[0];
if (ht) {
if (k.bjr && k.bjr.b !== ht.sid) {
var lt;
(lt = o.default.FH(t.hx, o.default.FJ(t, k.bjr.f.id))) && (lt.b = 0);
}
k.bjr = ht;
}
}
}
}
if (k.bjn > 0) {
k.bjn = a.$jB.bma(k.bjn - this.dt, 4);
j.bjM || (j.bjN = 0);
k.bjs = 0;
} else if (I) {
k.bjs = .3;
k.bjt[0] = 0;
k.bjt[1] = 0;
k.bjt[2] = 0;
k.bjt[3] = 0;
} else if (F) if (k.bjr) {
ot = k.bjr.g.area;
k.bjs = .25;
k.bjt[0] = ot[0];
k.bjt[1] = ot[1];
k.bjt[2] = ot[2];
k.bjt[3] = ot[3];
} else {
var ft = null === (l = null === (h = x.bhw) || void 0 === h ? void 0 : h.ai) || void 0 === l ? void 0 : l.move;
if (ft) {
k.bjs = ft.time;
k.bjt[0] = ft.param[0];
k.bjt[1] = ft.param[1];
k.bjt[2] = ft.param[2];
k.bjt[3] = ft.param[3];
}
} else {
j.bjM || (j.bjN = 0);
k.bjs = 0;
}
if (k.bjs && !j.bjM) {
k.bjo = a.$jB.bma(this.dt + (k.bjo || 0), 4);
if (k.bjo >= k.bjs) {
k.bjo = 0;
var ct = k.bjt, st = null == w ? void 0 : w.hw;
I && (st = I.hw);
var pt = this.bnp;
if (st) {
pt.x = st.x;
pt.y = st.y;
if (null === (g = null === (f = null == w ? void 0 : w.hE) || void 0 === f ? void 0 : f.bhw) || void 0 === g ? void 0 : g.aimAnchor) {
pt.x = a.$jB.bma(st.x + w.hE.bhw.aimAnchor.x);
pt.y = a.$jB.bma(st.y + w.hE.bhw.aimAnchor.y);
}
} else {
pt.x = a.$jB.bma(B.x + 800 * a.$jB.bma(P.ge() - .5));
pt.y = a.$jB.bma(B.y + 800 * a.$jB.bma(P.ge() - .5));
}
var dt = B.x - pt.x, vt = pt.x + (ct[0] + a.$jB.bma((ct[1] - ct[0]) * P.ge(), 3)) * Math.sign(dt);
vt = Math.max(Math.min(vt, E.bkm), E.bkl);
var $t = P.ge() > .5 ? 1 : -1, mt = pt.y + (ct[2] + a.$jB.bma((ct[3] - ct[2]) * P.ge(), 3)) * $t;
mt = Math.max(Math.min(mt, E.bko), E.bkn);
k.bjl.x = vt;
k.bjl.y = mt;
}
var yt = k.bjl.x - B.x, gt = k.bjl.y - B.y, Dt = 0, jt = 0;
Math.abs(yt) > 30 && (Dt = .6 * Math.sign(yt));
Math.abs(gt) > 30 && (jt = .6 * Math.sign(gt));
if (0 === Dt && 0 === jt) j.bjN = 0; else {
j.bjN = 1;
j.bjO.x = Dt;
j.bjO.y = jt;
j.bjP = 0;
}
}
}
}
}
};
return e;
}(u.$iZ.$jj);
i.$jS = g;
cc._RF.pop();
}, {
"../$KL": "$KL",
"../../../main/src/$KV": void 0,
"../BattleBehaviorLogic": "BattleBehaviorLogic",
"../component/$DOO": "$DOO",
"../component/$DOR": "$DOR",
"../component/$DPH": "$DPH",
"../component/$DPJ": "$DPJ",
"../component/$DPK": "$DPK",
"../component/$DPL": "$DPL",
"../lib/$DPP": "$DPP",
"../math/$DPR": "$DPR",
"../math/$DPS": "$DPS",
"../utils/$DQI": "$DQI"
} ],
$DQG: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b2e3ccGm15AIq9XN2elMQ6H", "$DQG");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jT = void 0;
var o = t("../lib/$DPP"), a = t("../component/$DPJ"), u = t("../component/$DPN"), b = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.init = function() {};
e.prototype.filter = function() {
return o.$iZ.allOf(a.default, u.default);
};
e.prototype.update = function(t) {
for (var e = 0, i = t; e < i.length; e++) {
var n = i[e];
n.hx.bjR.forEach(function(t) {
t.c ? t.c = 0 : t.d = 0;
});
n.hx.bjS.forEach(function(t) {
t.c ? t.c = 0 : t.d = 0;
});
}
};
return e;
}(o.$iZ.$jj);
i.$jT = b;
cc._RF.pop();
}, {
"../component/$DPJ": "$DPJ",
"../component/$DPN": "$DPN",
"../lib/$DPP": "$DPP"
} ],
$DQH: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ed669UyLh9HEZYhTjYceT5b", "$DQH");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jU = void 0;
var o = t("../lib/$DPP"), a = t("../component/$DPL"), u = t("../component/$DPN"), b = function(t) {
r(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.init = function() {};
e.prototype.destroy = function() {};
e.prototype.filter = function() {
return o.$iZ.allOf(a.default);
};
e.prototype.update = function(t) {
for (var e = t[0].ho(a.default).bkp, i = e.length, n = this.context.bln(u.default).worldTimeScale, r = i - 1; r > -1; r--) {
var o = e[r];
o.time += this.dt * n;
o.time > o.duration && e.splice(r, 1);
}
};
return e;
}(o.$iZ.$jj);
i.$jU = b;
cc._RF.pop();
}, {
"../component/$DPL": "$DPL",
"../component/$DPN": "$DPN",
"../lib/$DPP": "$DPP"
} ],
$DQI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "699bdb+7wpCIKDm6kWKRwH/", "$DQI");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = function() {
function t() {}
t.bnr = function(t) {
var e = Array.isArray(t) ? [] : {};
if (t && "object" == typeof t) for (var i in t) if (t.hasOwnProperty(i)) {
var n = t[i];
n instanceof Object ? e[i] = this.bnr(n) : e[i] = n;
}
return e;
};
t.bns = function(t, e, i, n) {
for (var r = i + 1; r < n; r++) {
for (var o = t[r], a = r - 1; a >= i; a--) {
var u = t[a];
if (!(e(u, o) > 0)) break;
t[a + 1] = u;
}
t[a + 1] = o;
}
return t;
};
return t;
}();
i.default = n;
cc._RF.pop();
}, {} ],
$DQJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "80103XDbv1AO4HsZuC60nK+", "$DQJ");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$jW = void 0;
var n = function() {
function t(t) {
void 0 === t && (t = 0);
this.bnt = 0;
this.bnt = t;
}
t.prototype.bnu = function(t) {
return 0 != (this.bnt & t);
};
t.prototype.bnv = function(t) {
return (this.bnt & t) == t;
};
t.prototype.bnw = function(t) {
this.bnt |= t;
};
t.prototype.bnx = function(t) {
this.bnt &= ~t;
};
t.prototype.bny = function() {
this.bnt = 0;
};
t.prototype.bnz = function(t) {
this.bnt = t;
};
t.prototype.bnA = function() {
return this.bnt;
};
return t;
}();
i.$jW = n;
cc._RF.pop();
}, {} ],
$KI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "113574DiDtEt7I9AHonlDoQ", "$KI");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("./math/$DPR"), r = t("../../game/src/battle/$DSO"), o = function() {
function t() {}
t.CW = function(t, e, i, o) {
var a, u, b, h, l, f, c, s, p, d, v, $, m, y, g, D, j, P, E, x, O, k;
if ("number" == typeof t) return t;
if (Array.isArray(t)) {
switch (t[0]) {
case "attr":
return null !== (u = null === (a = e.hv) || void 0 === a ? void 0 : a.beI(t[1])) && void 0 !== u ? u : 0;

case "ap":
return Math.ceil(n.$jB.bma((null !== (h = null === (b = e.hv) || void 0 === b ? void 0 : b.bfc) && void 0 !== h ? h : 0) * (null !== (l = t[1]) && void 0 !== l ? l : 1)));

case "HPMAX":
return Math.ceil(n.$jB.bma((null !== (c = null === (f = e.hv) || void 0 === f ? void 0 : f.bev) && void 0 !== c ? c : 0) * (null !== (s = t[1]) && void 0 !== s ? s : 1)));

case "MPMAX":
return Math.ceil(n.$jB.bma((null !== (d = null === (p = e.hv) || void 0 === p ? void 0 : p.bex) && void 0 !== d ? d : 0) * (null !== (v = t[1]) && void 0 !== v ? v : 1)));

case "var":
return null !== ($ = e.hE.bhA[t[1]]) && void 0 !== $ ? $ : 0;

case "sk":
var _ = t[1], w = e.hv.beg(_);
return w ? w.Dl(t[2]) : 0;

case "range":
return n.$jB.bma(t[1] + n.$jB.bma(t[2] - t[1]) * Math.min(1, this.CW(t[3], e, i) / this.CW(t[4], e, i)));

case "bf0":
return null !== (m = null == i ? void 0 : i.Dr[0]) && void 0 !== m ? m : 0;

case "bf1":
return null !== (y = null == i ? void 0 : i.Dr[1]) && void 0 !== y ? y : 0;

case "bf2":
return null !== (g = null == i ? void 0 : i.Dr[2]) && void 0 !== g ? g : 0;

case "bf3":
return null !== (D = null == i ? void 0 : i.Dr[3]) && void 0 !== D ? D : 0;

case "bf4":
return null !== (j = null == i ? void 0 : i.Dr[4]) && void 0 !== j ? j : 0;

case "bf5":
return null !== (P = null == i ? void 0 : i.Dr[5]) && void 0 !== P ? P : 0;

case "originBf0":
return null !== (E = null == o ? void 0 : o.Dr[0]) && void 0 !== E ? E : 0;

case "originBf1":
return null !== (x = null == o ? void 0 : o.Dr[1]) && void 0 !== x ? x : 0;

case "originBf2":
return null !== (O = null == o ? void 0 : o.Dr[2]) && void 0 !== O ? O : 0;

case "originBf3":
return null !== (k = null == o ? void 0 : o.Dr[3]) && void 0 !== k ? k : 0;

case "bfEventParam0":
return null == i ? void 0 : i.DF;

case "bfEventParam1":
return null == i ? void 0 : i.DG;

case "bfEventParam2":
return null == i ? void 0 : i.DH;

case "bfOrigin":
var F = e.hi.bll(i.DB);
return F ? this.CW(t[1], F, i) : 0;

case "bfOriginCreator":
var B = e.hi.bll(i.DB);
if (B && B.hE) {
var M = e.hi.bll(B.hE.bhQ);
return M ? this.CW(t[1], M, i) : 0;
}
return 0;

case "+":
return n.$jB.bma(this.CW(t[1], e, i) + this.CW(t[2], e, i));

case "-":
return n.$jB.bma(this.CW(t[1], e, i) - this.CW(t[2], e, i));

case "*":
return n.$jB.bma(this.CW(t[1], e, i) * this.CW(t[2], e, i));

case "/":
return n.$jB.bma(this.CW(t[1], e, i) / this.CW(t[2], e, i));

case "ceil":
return Math.ceil(n.$jB.bma(this.CW(t[1], e, i)));

case "floor":
return Math.floor(n.$jB.bma(this.CW(t[1], e, i)));

case "WBVar":
var I = e.hE.bhA.WBState || 0, R = r.default.bEC.bGi().find(function(t) {
return t.id === I;
});
return R ? R.attackRate : 0;
}
return t;
}
return t;
};
return t;
}();
i.default = o;
cc._RF.pop();
}, {
"../../game/src/battle/$DSO": void 0,
"./math/$DPR": "$DPR"
} ],
$KJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5425eBldDxGg7YEGq3JbLYn", "$KJ");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), o = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$fL = i.$fK = i.$fJ = i.$fI = i.$fH = i.$fG = i.$fF = i.$fE = i.$fD = i.$fC = i.$fB = i.$fA = i.$fz = i.$fy = i.$fx = i.registerSpecialCmd = i.battleCmdClassCtorMap = i.$fw = void 0;
var a = t("./$KL"), u = t("./component/$DPE"), b = t("../../game/src/battle/types/$DYO"), h = t("../../game/src/battle/types/$DYQ"), l = t("../../game/src/battle/types/$DYS"), f = t("../../game/src/battle/$DSO"), c = t("./component/$DPH"), s = t("../../main/src/$KV"), p = function() {
function t() {}
t.prototype.eY = function() {
return 0;
};
t.prototype.eZ = function() {};
t.prototype.fa = function() {};
t.eX = null;
return t;
}();
i.$fw = p;
window.$fN = window.$fN || {};
$fN.getCmd = function(t) {
return i.battleCmdClassCtorMap.get(t);
};
i.battleCmdClassCtorMap = new Map();
function d(t) {
return function(e) {
if (!i.battleCmdClassCtorMap.has(t) && t > 0) {
e.eX = t;
i.battleCmdClassCtorMap.set(t, e);
} else console.warn("registerSpecialCmd cmdId error", e.name);
};
}
i.registerSpecialCmd = d;
var v = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 0);
var i = t.call(this) || this;
i.roomId = e;
return i;
}
i = e;
e.prototype.eY = function() {
return this.roomId << 9 | i.eX;
};
e.prototype.eZ = function(t) {
this.roomId = t >> 9;
};
e.prototype.fa = function(t, e) {
t.fJ && (this.roomId && this.roomId !== t.fJ || (e.cYh = 1));
};
var i;
return i = o([ d(b.$ug.M) ], e);
}(p);
i.$fx = v;
var $ = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 1);
var i = t.call(this) || this;
i.timeScale = e;
return i;
}
i = e;
e.prototype.eY = function() {
return Math.floor(1e4 * this.timeScale) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = (t >> 9) / 1e4;
this.timeScale = e;
};
e.prototype.fa = function(t) {
t.go(this.timeScale);
};
var i;
return i = o([ d(b.$ug.B) ], e);
}(p);
i.$fy = $;
var m = function(t) {
r(e, t);
function e() {
return t.call(this) || this;
}
i = e;
e.prototype.eY = function() {
return i.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
var i = t.gi(e.cXZ);
i && i.hx && (i.hx.bjU = 1);
};
var i;
return i = o([ d(b.$ug.C) ], e);
}(p);
i.$fz = m;
var y = function(t) {
r(e, t);
function e() {
return t.call(this) || this;
}
i = e;
e.prototype.eY = function() {
return i.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t) {
t.fF.forEach(function(e) {
var i = t.gi(e.cXZ);
i && i.hv.bdU.forEach(function(t) {
if (t.Di() > 0) {
if (t.maxCdTime > 0) {
t.cd = 0;
t.cdTest = 0;
} else t.cdTest = 1;
t.Da > 1 ? t.Da = 0 : t.Da = t.Dj();
}
});
});
};
var i;
return i = o([ d(b.$ug.D) ], e);
}(p);
i.$fA = y;
var g = function(t) {
r(e, t);
function e(e) {
void 0 === e && (e = 1);
var i = t.call(this) || this;
i.reviveOpt = e;
return i;
}
i = e;
e.prototype.eY = function() {
return Math.floor(this.reviveOpt) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.reviveOpt = e;
};
e.prototype.fa = function(t, e) {
switch (this.reviveOpt) {
case 1:
a.default.Gt(t, e.cXL);
break;

case 0:
a.default.Gs(t, e.cXL);
}
};
var i;
return i = o([ d(b.$ug.E) ], e);
}(p);
i.$fB = g;
var D = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = b.$ug.H;
return e;
}
i = e;
e.prototype.eY = function() {
return i.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
if (!t.fe) {
var i = t.gi(e.cXZ);
if (i) if (i.hv.bfG() < 3147483648) {
i.hv.beJ(f.GameAttriTableEnum.hpMax, 3147483648);
i.hv.beu = i.hv.bev;
i.hv.beJ(f.GameAttriTableEnum.ap, 1e5);
i.hv.beJ(f.GameAttriTableEnum.sp, 1e5);
i.hv.beJ(f.GameAttriTableEnum.speed, 15e3);
i.hv.beJ(f.GameAttriTableEnum.atkSpeed, 1e4);
i.hv.beJ(f.GameAttriTableEnum.castSpeed, 1e4);
i.hv.beJ(f.GameAttriTableEnum.finalDamageRed, 1e4);
} else {
i.hv.beK(f.GameAttriTableEnum.hpMax, 3147483648);
i.hv.beu = i.hv.bev;
i.hv.beK(f.GameAttriTableEnum.ap, 1e5);
i.hv.beK(f.GameAttriTableEnum.sp, 1e5);
i.hv.beK(f.GameAttriTableEnum.speed, 15e3);
i.hv.beK(f.GameAttriTableEnum.atkSpeed, 1e4);
i.hv.beK(f.GameAttriTableEnum.castSpeed, 1e4);
i.hv.beK(f.GameAttriTableEnum.finalDamageRed, 1e4);
}
}
};
var i;
return i = o([ d(b.$ug.H) ], e);
}(p);
i.$fC = D;
var j = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = b.$ug.I;
return e;
}
i = e;
e.prototype.eY = function() {
return i.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
if (!t.fe) {
var i = t.gi(e.cXZ);
i && t.fu.bkN.forEach(function(t) {
if (t.hx && t.hD.biI !== i.hD.biI) {
t.hX(i, t.hv.beu);
a.default.GJ(t, 0, 0);
}
});
}
};
var i;
return i = o([ d(b.$ug.I) ], e);
}(p);
i.$fD = j;
var P = function(t) {
r(e, t);
function e() {
var e = t.call(this) || this;
e.cmdId = b.$ug.J;
return e;
}
i = e;
e.prototype.eY = function() {
return i.eX;
};
e.prototype.eZ = function() {};
e.prototype.fa = function(t, e) {
var i = t.gi(e.cXZ);
if (i) {
var n = i.hw;
t.fD.bkN.forEach(function(t) {
var e = t.ho(u.default);
if (a.default.Gd(n, t.hw) < e.biX) {
var r = t.hE.bhw.interfactive;
if (r) {
r.tlg && a.default.FM(i, null, r.tlg, null);
r.lg && a.default.FM(t, null, r.lg, null);
}
}
});
}
};
var i;
return i = o([ d(b.$ug.J) ], e);
}(p);
i.$fE = P;
var E = function(t) {
r(e, t);
function e(e) {
var i = t.call(this) || this;
i.consumeIndex = e;
return i;
}
i = e;
e.prototype.eY = function() {
return Math.floor(1e4 * this.consumeIndex) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = (t >> 9) / 1e4;
this.consumeIndex = e;
};
e.prototype.fa = function(t, e) {
var i, n = t.gi(e.cXZ);
if (n) {
var r = null === (i = n.hL) || void 0 === i ? void 0 : i.bjF[this.consumeIndex];
if (r) {
a.default.Fe(n, r);
t.gu(l.$uo.$vk.w, n, r.DY.id);
}
}
};
var i;
return i = o([ d(b.$ug.N) ], e);
}(p);
i.$fF = E;
var x = function(t) {
r(e, t);
function e(e) {
var i = t.call(this) || this;
i.autoNum = e;
return i;
}
i = e;
e.prototype.eY = function() {
return this.autoNum << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.autoNum = e;
};
e.prototype.fa = function(t, e) {
var i = Boolean(this.autoNum);
if (e.cYg !== i) {
e.cYg = i;
switch (t.fM.f) {
case h.$uj.e:
break;

default:
var n = t.gi(e.cXZ);
if (!n) return;
if (e.cYg) a.default.Et(n); else {
n.hx.bjN = 0;
n.hq(c.default);
}
}
t.gu(l.$uo.$vk.o);
}
};
var i;
return i = o([ d(b.$ug.L) ], e);
}(p);
i.$fG = x;
var O = function(t) {
r(e, t);
function e(e) {
var i = t.call(this) || this;
i.level = e;
return i;
}
i = e;
e.prototype.eY = function() {
return Math.max(Math.min(Math.floor(this.level), 999), 1) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.level = e;
};
e.prototype.fa = function(t, e) {
var i = t.gi(e.cXZ);
if (i) {
a.default.Ep(i, this.level);
t.gu(l.$uo.$vk.bl);
}
};
var i;
return i = o([ d(b.$ug.O) ], e);
}(p);
i.$fH = O;
var k = function(t) {
r(e, t);
function e(e, i) {
var n = t.call(this) || this;
n.playerPosition = e;
n.offlineState = i;
return n;
}
i = e;
e.prototype.eY = function() {
return (this.playerPosition << 8 | this.offlineState) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9, i = e >> 8, n = 255 & e;
this.playerPosition = i;
this.offlineState = n;
};
e.prototype.fa = function(t) {
var e = this, i = t.fF.find(function(t) {
return t.cXO === e.playerPosition;
});
i && (i.cYn = 1 === this.offlineState);
};
var i;
return i = o([ d(b.$ug.P) ], e);
}(p);
i.$fI = k;
var _ = function(t) {
r(e, t);
function e(e) {
var i = t.call(this) || this;
i.buffId = e;
return i;
}
i = e;
e.prototype.eY = function() {
var t;
return (null !== (t = this.buffId) && void 0 !== t ? t : 0) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.buffId = e;
};
e.prototype.fa = function(t) {
for (var e = 0, i = t.fF; e < i.length; e++) {
var n = i[e], r = t.gi(n.cXZ);
r && a.default.FN(r, this.buffId, [], r.hg);
}
};
var i;
return i = o([ d(b.$ug.R) ], e);
}(p);
i.$fJ = _;
var w = function(t) {
r(e, t);
function e(e, i) {
var n = t.call(this) || this;
n.cmdId = e;
n.position = i;
return n;
}
i = e;
e.prototype.eY = function() {
return (this.cmdId << 8 | this.position) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.cmdId = e >> 8;
this.position = 255 & e;
};
e.prototype.fa = function(t, e) {
var i = this;
if (t.fF.find(function(t) {
return t.cXO === i.position;
})) {
var n = t.gi(e.cXZ);
if (n) {
var r = n.hL.bjG[this.cmdId];
r && n.hx.bjT.push({
c: this.cmdId,
d: r,
b: this.position
});
}
}
};
var i;
return i = o([ d(b.$ug.S) ], e);
}(p);
i.$fK = w;
var F = function(t) {
r(e, t);
function e(e) {
var i = t.call(this) || this;
i.eventId = e;
return i;
}
i = e;
e.prototype.eY = function() {
var t;
return (null !== (t = this.eventId) && void 0 !== t ? t : 0) << 9 | i.eX;
};
e.prototype.eZ = function(t) {
var e = t >> 9;
this.eventId = e;
};
e.prototype.fa = function(t) {
var e;
switch (this.eventId) {
case l.$uo.$vi.b:
var i = t.fF[0];
if (i) {
if (!(o = t.gi(i.cXZ)).hU) return;
a.default.EF(o, l.$uo.$uP.b);
var n = t.gF()[0];
if (!(null == n ? void 0 : n.hU)) return;
a.default.EF(n, "10001");
o.hx.bjV = !0;
}
break;

case l.$uo.$vi.c:
var r = t.fF[0];
if (r) {
var o;
if (!(null == (o = t.gi(r.cXZ)) ? void 0 : o.hU)) return;
o.hx.bjV = !1;
var u = null === (e = o.hv) || void 0 === e ? void 0 : e.bdU.find(function(t) {
return t.Dg.commonType === s.cvResUrl.commonSkillDodge2Cid;
});
if (a.default.Ff(o, u)) {
a.default.Fg(o, u);
a.default.EF(o, l.$uo.$uP.l);
}
}
}
};
var i;
return i = o([ d(b.$ug.T) ], e);
}(p);
i.$fL = F;
cc._RF.pop();
}, {
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYO": void 0,
"../../game/src/battle/types/$DYQ": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../main/src/$KV": void 0,
"./$KL": "$KL",
"./component/$DPE": "$DPE",
"./component/$DPH": "$DPH"
} ],
$KK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "0d14c0gdaRPR65TGQXWfn0g", "$KK");
var n = this && this.__decorate || function(t, e, i, n) {
var r, o = arguments.length, a = o < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) a = Reflect.decorate(t, e, i, n); else for (var u = t.length - 1; u >= 0; u--) (r = t[u]) && (a = (o < 3 ? r(a) : o > 3 ? r(e, i, a) : r(e, i)) || a);
return o > 3 && a && Object.defineProperty(e, i, a), a;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$ge = void 0;
var r = t("../../bingo/src/utils/$DOB"), o = t("../../game/src/battle/$DSO");
(function(t) {
var e = function() {
function t() {
this.CX = 0;
this.Dc = !1;
this.Dd = !1;
this.De = 0;
this.Df = -1;
this.cdTest = 1;
}
t.prototype.Dh = function(t, e, i) {
this.CX = t;
this.Dg = o.default.bEC.bFb(this.CX);
this.De = e;
this.CY = 0;
this.cd = 0;
this.Dd = i;
this.cdTest = 1;
this.Dc = !0;
this.maxCdTime = this.Di();
this.Da = this.Dj();
this.Db = this.Dk();
this.Dg.bOriginal && (this.De = Math.max(this.Dg.bOriginal, this.De));
"number" == typeof this.Dg.storageCapacityIndex ? this.CZ = this.Dl(this.Dg.storageCapacityIndex) : this.CZ = 0;
};
t.prototype.Di = function() {
var t;
return this.Dg && null !== (t = this.Dd ? this.Dg.pveCD : this.Dg.pvpCD) && void 0 !== t ? t : 0;
};
t.prototype.Dj = function() {
var t;
if (!(null === (t = this.Dg) || void 0 === t ? void 0 : t.mpCost)) return 0;
var e = this.Dg.mpCost[this.Dd ? 0 : 1];
return e && e[0] + (Math.min(this.Dg.skillMaxLv, this.De) - 1) * e[1] || 0;
};
t.prototype.Dk = function() {
var t;
if (!(null === (t = this.Dg) || void 0 === t ? void 0 : t.hpPercentCost)) return 0;
var e = this.Dg.hpPercentCost[this.Dd ? 0 : 1];
return e && e[0] + (Math.min(this.Dg.skillMaxLv, this.De) - 1) * e[1] || 0;
};
t.prototype.Dl = function(t) {
var e;
return (null === (e = this.Dg) || void 0 === e ? void 0 : e.pveBase) ? this.Dd ? this.Dg.pveBase[t] + (Math.min(this.Dg.skillMaxLv, this.De) - 1) * this.Dg.pveUpgrade[t] || 0 : this.Dg.pvpBase[t] + (Math.min(this.Dg.skillMaxLv, this.De) - 1) * this.Dg.pvpUpgrade[t] || 0 : 0;
};
t.prototype.Dm = function() {};
t.prototype.Dn = function() {
return [];
};
n([ r.pooledFloat ], t.prototype, "cd", void 0);
n([ r.pooledFloat ], t.prototype, "maxCdTime", void 0);
n([ r.pooledInt ], t.prototype, "cdTest", void 0);
return t;
}();
t.$gf = e;
var i = function() {
function t() {
this.Do = 0;
this.Dp = null;
this.Dq = null;
this.Dr = [];
this.Ds = [];
this.Dt = [];
this.Du = 0;
this.Dv = 0;
this.Dw = 0;
this.Dx = 0;
this.Dy = 0;
this.Dz = 0;
this.DA = 0;
this.DB = 0;
this.DC = 0;
this.DD = 0;
this.DE = 0;
this.DF = 0;
this.DG = 0;
this.DH = 0;
}
t.prototype.DI = function(t) {
this.Dp = t;
this.Dq = o.default.bEC.bFf(this.Dp);
this.DB = 0;
this.Dr.length = 0;
this.Ds.length = 0;
this.Dt.length = 0;
this.DF = 0;
this.DG = 0;
this.DH = 0;
this.DC = 0;
this.DD = 0;
this.DE = 0;
};
t.prototype.DJ = function() {};
t.prototype.DK = function() {
return [];
};
return t;
}();
t.$gg = i;
t.$gh = function() {
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
var a = function() {
function t() {
this.DL = 0;
this.DS = !1;
this.DT = !1;
this.DU = 0;
this.DV = null;
this.DW = !1;
this.DX = !1;
}
t.prototype.DZ = function(t, e) {
var i, n;
this.DL = t;
this.DL > 0 ? this.DY = o.default.bEC.bFa(this.DL) : this.DY = null;
this.DM = 0;
this.DO = 0;
this.DP = null !== (n = null === (i = this.DY) || void 0 === i ? void 0 : i.cd) && void 0 !== n ? n : 0;
this.DV = e;
this.DW = !1;
this.DX = !1;
if (this.DY) for (var r = 0, a = this.DY.set; r < a.length; r++) switch (a[r]) {
case 11922:
this.DW = !0;
break;

case 11923:
this.DX = !0;
}
};
return t;
}();
t.$gi = a;
})(i.$ge || (i.$ge = {}));
cc._RF.pop();
}, {
"../../bingo/src/utils/$DOB": void 0,
"../../game/src/battle/$DSO": void 0
} ],
$KL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "9ebdcJ6pydMOb240RkMalIf", "$KL");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("./BattleBehaviorLogic"), r = t("./component/$DOO"), o = t("./component/$DOQ"), a = t("./component/$DOR"), u = t("./component/$DOS"), b = t("./component/$DOU"), h = t("./component/$DOY"), l = t("./component/$DPA"), f = t("./component/$DPB"), c = t("./component/$DOX"), s = t("./component/$DPF"), p = t("./component/$DPJ"), d = t("./component/$DPL"), v = t("./component/$DPN"), $ = t("./component/$DPO"), m = t("./component/$DOZ"), y = t("./component/$DOP"), g = t("../../game/src/battle/types/$DYS"), D = t("./math/$DPR"), j = t("./math/$DPS"), P = t("./utils/$DQI"), E = t("./$KI"), x = t("./component/$DPI"), O = t("../../game/src/battle/$DSO"), k = t("./$KK"), _ = t("./math/$DPT"), w = t("./component/$DOT"), F = t("./component/$DPK"), B = t("./component/$DOV"), M = t("./system/$DPY"), I = t("./component/$DOW"), R = t("./component/$DPH"), Y = t("../../main/src/tbl/TblEnum"), T = t("../../main/src/$KV"), C = function() {
function t() {}
t.Ef = function(t, e, i) {
var n = null;
(n = 0 === this.Ee.length ? new k.$ge.$gh() : this.Ee.pop()).atkEntity = t;
n.fromEntity = e;
n.target = i;
n.hitX = 0;
n.hitY = 0;
n.hitZ = 0;
n.hitInfo = null;
n.isCrt = !1;
n.beforeHP = 0;
n.finalDamage = 0;
n.isPhysical = !0;
n.elementValue = 0;
return n;
};
t.Eg = function(t) {
t.atkEntity = null;
t.fromEntity = null;
t.target = null;
this.Ee.push(t);
};
t.Ei = function() {
return 0 === this.Eh.length ? new k.$ge.$gf() : this.Eh.pop();
};
t.Ej = function(t) {
this.Eh.push(t);
};
t.Ek = function() {
this.Eh.length = 0;
};
t.El = function(e, i, n, r, o) {
var a, u, b, h, l = e.hv;
l.bdP = n;
var f = l.bdQ = O.default.bEC.bEW(l.bdP);
l.bet = f.speed;
l.bfa = f.gravity;
l.bfb = f.jump;
l.bdV = null !== (a = f.defaultWeaponType) && void 0 !== a ? a : 0;
if (null === (u = null == o ? void 0 : o.wear) || void 0 === u ? void 0 : u.equipList) {
var c = O.default.bEC.bFL(o.wear.equipList);
if (c) {
var s = O.default.bEC.bFM(c.itemId);
s && (l.bdV = s.childType);
}
}
l.bey = r;
var p = e.hi.blc.fM.l;
if ((null == o ? void 0 : o.skillDataList) && (null == o ? void 0 : o.wear)) {
if (p) {
l.bdS.cWn(n, r, {
isPve: e.hi.blc.gM,
skillDataList: o.skillDataList,
wears: [],
pet: null,
archive: null,
adventureIndex: [],
mountIds: [],
guildBuffList: null
});
l.bdS.cVT.cUV();
l.bdS.cVV.cUV();
l.bdS.cVX.cUV();
l.bdS.cVZ.cUV();
l.bdS.cWb.cUV();
} else l.bdS.cWn(n, r, {
isPve: e.hi.blc.gM,
skillDataList: o.skillDataList,
wears: null === (b = o.wear) || void 0 === b ? void 0 : b.equipList,
pet: null === (h = o.troop) || void 0 === h ? void 0 : h.pet,
archive: o.archive,
adventureIndex: o.adventureIndex,
mountIds: o.mountIds,
guildBuffList: o.guildBuffList
});
l.bdS.cVR();
} else l.bdS.cVR();
var d = l.bdS.cVN;
for (var v in i) {
var $ = i[v];
d.cVz(Number(v), $);
}
l.beu = l.bev;
l.bew = l.bex;
l.bdT = null == o ? void 0 : o.skillDataList;
t.Em(l, e.hi.blc.gM);
};
t.Em = function(e, i) {
for (var n = 0, r = e.bdU; n < r.length; n++) {
var o = r[n];
t.Ej(o);
}
e.bdU.length = 0;
var a = e.bdQ.skillGroup;
a || (a = O.default.bEC.bFc());
if (e.bdT) for (var u = 0, b = a; u < b.length; u++) {
s = b[u];
var h = e.bdS.cWy(s);
if (!(h < 1)) {
(c = t.Ei()).Dh(s, h, i);
e.bdU.push(c);
}
} else for (var l = 0, f = a; l < f.length; l++) {
var c, s = f[l];
(c = t.Ei()).Dh(s, 1, i);
e.bdU.push(c);
}
};
t.En = function() {};
t.Eo = function() {};
t.Ep = function(e, i) {
var n = e.hv;
if (n) {
n.bey = i;
if (n.bez) {
e.hY(e, e.hv.bev);
e.hZ(e, e.hv.bex);
t.FN(e, 11007, [], e.hg);
}
}
};
t.Eq = function(t, e, i, n) {
var r, o, a;
switch (e.type) {
case g.$uo.$ve.b:
return t.hi.bll(t.hE.bhG);

case g.$uo.$ve.c:
var u = t.hE.bhH.find(function(t) {
return t.id === e.id;
});
return u ? t.hi.bll(u.eid) : null;

case g.$uo.$ve.d:
return !t.hG && (b = t.hi.bll(t.hE.bhF)) ? t.hi.bll(null === (r = b.hG) || void 0 === r ? void 0 : r.bgj) : t.hi.bll(null === (o = t.hG) || void 0 === o ? void 0 : o.bgj);

case g.$uo.$ve.e:
return (null == i ? void 0 : i.target) ? i.target : null;

case g.$uo.$ve.f:
return t;

case g.$uo.$ve.g:
if (n) {
var b;
return t.hi.bll(n.DB);
}
return null;

case g.$uo.$ve.h:
return t.hi.bll(t.hE.bhQ);

case g.$uo.$ve.i:
for (var h = 0, l = t.hi.blc.fu.bkN; h < l.length; h++) {
var f = l[h];
if (f.hS) return f;
}
return null;

case g.$uo.$ve.j:
return this.Er(t, e.radius, !0, 1);

case g.$uo.$ve.k:
var c = null === (a = t.hv) || void 0 === a ? void 0 : a.bdU.find(function(t) {
return t.CX === e.id;
});
return c ? t.hi.blc.gi(c.Df) : null;
}
return null;
};
t.Er = function(t, e, i, n) {
var r, o, a, u, b = t.hi.blc.fu.bkN, h = e * e;
if (0 === n) {
for (var l = Number.MAX_SAFE_INTEGER, f = -1, c = null, s = 0, p = b; s < p.length; s++) if ((null === (o = (j = p[s]).hE) || void 0 === o || !o.biv) && j.hD.biI !== t.hD.biI && j.hU) {
var d = D.$jB.bma(t.hw.x - j.hw.x, 3), v = D.$jB.bma(t.hw.y - j.hw.y, 3);
if ((x = D.$jB.bma(d * d + v * v, 3)) <= h) if (i) {
if (x < l) {
l = x;
c = j;
}
} else if (x > f) {
f = x;
c = j;
}
}
return c;
}
for (var $ = ((r = {})[O.GameEnemyTypeTableEnum.Boss] = [], r[O.GameEnemyTypeTableEnum.Elite] = [], 
r[O.GameEnemyTypeTableEnum.Normal] = [], r[0] = [], r), m = -1, y = 0, g = b; y < g.length; y++) {
var j;
if ((j = g[y]).hD && (null === (a = j.hE) || void 0 === a || !a.biv) && j.hD.biI !== t.hD.biI && j.hU) {
var E = j.hD.biJ;
if (!(E < m)) {
var x;
d = D.$jB.bma(t.hw.x - j.hw.x, 3), v = D.$jB.bma(t.hw.y - j.hw.y, 3);
if (!((x = D.$jB.bma(d * d + v * v, 3)) > h)) {
E > m && (m = E);
null === (u = $[E]) || void 0 === u || u.push({
e: j,
dis: x
});
}
}
}
}
var k = $[m];
if (!k || 0 === k.length) return null;
P.default.bns(k, function(t, e) {
return t.dis - e.dis;
}, 0, k.length);
return i ? k[0].e : k[k.length - 1].e;
};
t.Es = function(t, e) {
var i, n, r, o, a, u, b = O.default.bEC.bFo(e);
if (b) {
var h = t.hn(y.default);
h.bfJ = e;
h.bfQ = b;
h.bfS = 0;
h.bfT = 0;
h.bfU = 0;
h.bfV = 0;
h.bfW = !0;
h.bfX.length = 0;
h.bfY = null;
h.bfR.x = null !== (n = null === (i = t.hw) || void 0 === i ? void 0 : i.x) && void 0 !== n ? n : 0;
h.bfR.y = null !== (o = null === (r = t.hw) || void 0 === r ? void 0 : r.y) && void 0 !== o ? o : 0;
if (b.acts) for (var l = t.hv, f = 0, c = b.acts; f < c.length; f++) {
var s = c[f], p = l.bei(s.cmdId);
if (p) {
p.cd = null !== (a = s.initTime) && void 0 !== a ? a : 0;
p.maxCdTime = null !== (u = s.cdTime) && void 0 !== u ? u : 0;
p.CY = 0;
p.CZ = 0;
p.Da = 0;
p.Db = 0;
}
} else b.playerAI;
} else O.default.bEC.bEG("not find ai", e);
};
t.Et = function(t) {
var e, i, n, r, o = t.hn(R.default);
o.bjm = 0;
o.bjn = 0;
o.bjo = 0;
o.bjp = !0;
o.bjq.length = 0;
o.bjr = null;
o.bjs = 0;
o.bjt[0] = 0;
o.bjt[1] = 0;
o.bjt[2] = 0;
o.bjt[3] = 0;
o.bjl.x = null !== (i = null === (e = t.hw) || void 0 === e ? void 0 : e.x) && void 0 !== i ? i : 0;
o.bjl.y = null !== (r = null === (n = t.hw) || void 0 === n ? void 0 : n.y) && void 0 !== r ? r : 0;
};
t.Eu = function(t, e) {
var i = t.hI;
if (i) {
i.bfK = e;
i.bfP = 0;
i.bfO = 0;
i.bfN = {};
}
};
t.Ev = function(t, e, i, n) {
var r = t.hn(l.default);
r.biI = e;
r.biJ = i;
r.biK = n;
};
t.Ew = function(t, e) {
var i = O.default.bEC.bFj(e), n = t.hn(o.default);
n.bgb = e;
n.bgc = 0;
n.bgd = i;
n.bge = i ? O.default.bEC.bFn(i.actsId) : null;
n.bgf = 0;
n.bgg.length = 0;
n.bgh = 0;
};
t.Ex = function(e, i) {
var n, r, o, a, u, b;
if (e.hv) {
e.hv.beu = Math.ceil(D.$jB.bma(i * e.hv.bev));
if (e.hv.bdT) for (var h = e.hi.blc.gN, l = 0, f = e.hv.bdU; l < f.length; l++) {
var c = f[l];
c.maxCdTime = e.hv.bfB(c);
h && (c.cd = null !== (n = c.Dg.pvpInitialCD) && void 0 !== n ? n : 0);
}
for (var s = 0, p = e.hv.bdS.cVN.cUF; s < p.length; s++) {
var d = p[s];
t.FN(e, d, null, e.hg, null);
}
if (null === (o = null === (r = e.hE) || void 0 === r ? void 0 : r.bhw) || void 0 === o ? void 0 : o.buffs) for (var v = 0, $ = null === (u = null === (a = e.hE) || void 0 === a ? void 0 : a.bhw) || void 0 === u ? void 0 : u.buffs; v < $.length; v++) {
d = $[v];
var m = O.default.bEC.bFf(d);
1 === (null !== (b = null == m ? void 0 : m.addType) && void 0 !== b ? b : 1) ? t.FN(e, d, [], 0) : O.default.bEC.bEG("behavior buffs " + d + " addType != 1 ");
}
if (e.hS && e.hi.blc.fM.m) for (var y = 0, g = e.hi.blc.fM.m; y < g.length; y++) {
d = g[y];
t.FN(e, d, [], 0);
}
}
};
t.Ey = function(e) {
t.FQ(e, 11002);
e.hE.bhN.forEach(function(t) {
t.destroyTime = 0;
});
e.hE.bhw.newRound && t.FM(e, null, e.hE.bhw.newRound, e);
t.EC(e);
};
t.Ez = function(e, i, n, r, o, a, u) {
var b;
void 0 === n && (n = -1);
void 0 === r && (r = -1);
void 0 === a && (a = {});
void 0 === u && (u = !1);
var l = O.default.bEC.bFq(i);
if (!l) {
l = O.default.bEC.bFq(100);
O.default.bEC.bEG("no find behaviorCfg", i);
}
var f = e.hn(h.default);
f.bho = null;
f.bhp = t.Ea;
f.bhq = null;
f.bhr = t.Ea;
f.bhs.length = 0;
f.bht = {};
f.bhu = {};
f.bhv = i;
f.bhw = l;
f.bhx = 0;
f.bhy = null;
f.bhz[0] = 0;
f.bhz[1] = 0;
f.bhA = a;
f.bhB = -1;
f.bhC = 0;
f.bhD = {};
f.bhE = {};
f.bhF = -1 === r ? e.hg : r;
f.bhG = n;
f.bhH.length = 0;
f.bhI.length = 0;
f.bhJ = -1;
f.bhK = -1;
f.bhL = null;
f.bhM.length = 0;
f.bhN.length = 0;
f.bir();
f.bhQ = -1;
f.bhR = !1;
f.bhS = null;
f.bhT = 0;
f.bhU = 0;
f.bhV = 0;
f.bhW = 1;
f.bhX = 0;
f.bhY = null;
f.bhZ = -1;
f.bia = 0;
f.bib = null;
f.bic.length = 0;
f.bid.b = 0;
f.bid.c.length = 0;
f.bhw.acts && e.hF && (e.hF.bge = f.bhw.acts);
t.EF(e, String(o || f.bis));
if (!e.hv && f.bhw.buffs) for (var c = 0, s = f.bhw.buffs; c < s.length; c++) {
var p = s[c], d = O.default.bEC.bFf(p);
1 === (null !== (b = null == d ? void 0 : d.addType) && void 0 !== b ? b : 1) ? t.FN(e, p, [], 0) : O.default.bEC.bEG("behavior buffs " + p + " addType != 1 ");
}
};
t.EA = function(e, i, r) {
if (e.hE) {
e.hE.bhw.newRound && t.FM(e, null, e.hE.bhw.newRound, e);
if (e.hE.bhy && e.hE.bhy.els) for (var o = 0, a = e.hE.bhy.els; o < a.length; o++) {
var u = a[o];
n.default.CU(e, null, u);
}
var b = e.hE;
b.bhv = i;
b.bhw = O.default.bEC.bFq(i);
b.bhB = -1;
b.bhC = 0;
b.bir();
b.bhy = null;
b.bhx = 0;
e.hv && !e.hv.bez && (r = g.$uo.$uP.e);
t.EF(e, String(r || b.bis));
if (b.bhw.buffs) for (var h = 0, l = b.bhw.buffs; h < l.length; h++) {
var f = l[h];
t.FN(e, f, [], 0);
}
e.hi.blc.gu(g.$uo.$vk.bc, e);
}
};
t.EB = function(t) {
return !!t.hv && !!t.hv.bez && !t.hv.bdR.bnu(g.$uo.$vb.c | g.$uo.$vb.b) && -1 === t.hE.bhJ;
};
t.EC = function(e) {
if (e.hE && e.hv && !e.hv.bfD() && -1 === e.hE.bhJ && e.hv.bez) {
t.EF(e, e.hE.bis);
e._if();
return !0;
}
};
t.ED = function(t) {
if (t.hv) {
if (t.hv.bfD()) return !1;
if (!t.hv.bez) return !1;
}
return !(t.hE.bit || t.hE.bhO.hurtbackTime > 0 && t.hE.bhO.hurtbackType !== g.$uo.$uN.b);
};
t.EE = function(e, i) {
void 0 === i && (i = !1);
var n = e.hE;
if (n) {
var r = e.hi;
t.ES(e, null);
if (n.bhs.length > 0) for (var o = 0, a = n.bhs.slice(); o < a.length; o++) {
var u = a[o];
t.FP(e, u, i);
}
t.EW(e);
t.EK(e);
n.bhM.length && t.FE(e, -1);
for (var b = 0, h = n.bic; b < h.length; b++) {
var l = h[b], f = r.bll(null == l ? void 0 : l.f);
f && f.hP();
}
n.bic.length = 0;
}
};
t.EF = function(t, e) {
this.EI(t, e);
};
t.EG = function(t, e) {
t.hE.bho = e;
if (e) {
var i = t.hv;
if (!i) {
var n = t.hi.bll(t.hE.bhF);
i = null == n ? void 0 : n.hv;
}
if (i) {
if (e.mhts) {
var r = i.bfz(e.mhts);
t.hy.mainTimeScale = r;
t.hy.hTimeScale = r;
} else {
if (e.mts) {
r = i.bfz(e.mts);
t.hy.mainTimeScale = r;
}
if (e.hts) {
r = i.bfz(e.hts);
t.hy.hTimeScale = r;
}
}
if (e.vts) {
r = i.bfz(e.vts);
t.hy.vTimeScale = r;
}
}
}
};
t.EH = function(t, e) {
if (e) {
t.hy.hTimeScale = 1;
t.hy.vTimeScale = 1;
t.hy.mainTimeScale = 1;
}
};
t.EI = function(e, i, n, r) {
var o, a, u = e.hE, b = null !== (o = u.bhw.behaviors[i]) && void 0 !== o ? o : r;
if (n && n[0]) {
u.bhz[0] = n[0];
u.bhz[1] = n[1];
var h = O.default.bEC.bFw(u.bhz[0]);
if (!h) return;
b = h.behaviors[u.bhz[1]];
} else {
if (!b) {
switch (i) {
case g.$uo.$uP.h:
case g.$uo.$uP.g:
case g.$uo.$uP.e:
case g.$uo.$uP.f:
case g.$uo.$uP.i:
b = O.default.bEC.bFx(i);
}
if (!b) return;
}
u.bhz[0] = 0;
u.bhz[1] = 0;
}
if (u.bhO.hurtbackTime) {
u.bhO.hurtbackTime = 0;
switch (u.bhO.hurtbackType) {
case g.$uo.$uN.b:
case g.$uo.$uN.c:
e.hz && t.Fm(e.hz, 0, 0, 0);
}
}
t.ES(e, b);
u.bhx = i;
u.bhy = b;
u.bhp = b.cls;
u.bhr = b.cgs;
u.bhq = b.els;
t.EH(e, u.bho);
t.EG(e, b.status);
if (null === (a = b.status) || void 0 === a ? void 0 : a.changeDirOnUse) {
var l = e.hx;
l && l.bjX && l.bjN && (e.hO || (l.bjO.x > .2 ? e.hw.bkw = 1 : l.bjO.x < -.2 && (e.hw.bkw = -1)));
}
u.bhB = -1;
u.bhC = 0;
u.bhD = {};
u.bhE = {};
this.ER(e);
};
t.EJ = function(e, i) {
var n, r, o = null !== (n = e.hD.biI) && void 0 !== n ? n : 0, a = e.hE, h = e.hF;
if (null == h ? void 0 : h.bge) {
var l = h.bge[i.isHurtAction ? a.bhO.actId : i.actId];
if (l) {
var c = i.start || 0, s = "number" == typeof i.end ? i.end : l.d, p = D.$jB.bma(Math.max(s - c, 0), 4), d = "number" == typeof i.timeScale ? i.timeScale : 1, v = e.hi;
e.hy.actionTimeScale = d;
a.bib = i;
a.bhX = 0;
a.bhZ = e.hi.blc.fi;
a.bhY = {
b: p,
d: l.an,
e: i.loop,
c: d,
i: i.start,
j: i.end,
f: 0,
g: 0,
h: 0,
m: i.posLerpRatio
};
for (var m = a.bic, y = l.cs, g = i.bodies, j = function(i) {
var n = g[i], l = y.find(function(t) {
return t.n === n.name;
});
if (!l) {
O.default.bEC.bEG("actions" + (null === (r = h.bgd) || void 0 === r ? void 0 : r.actsId) + " no find:" + n.name);
l = {
n: "",
fs: []
};
}
var s = m[i];
s || (s = m[i] = {
e: 1,
b: null,
c: "",
f: -1,
d: 0,
g: []
});
s.b = n;
s.e = 1;
s.d = 0;
s.c = l.n;
s.g = [ {
region: [ 0, p ],
collider: null
} ];
for (var d = s.g, D = 0, j = l.fs; D < j.length; D++) {
var P = j[D], x = P.t - c;
if (x > 0) {
if (x > p) break;
var k = d.length - 1;
d[k].region[1] = x;
x <= p && (d[k + 1] = {
region: [ x, p ],
collider: P.c
});
} else d[0].collider = P.c;
}
var _ = v.bll(s.f);
if (!_) {
_ = v.ble();
s.f = _.hg;
_.hn($.default);
_.hn(f.default);
_.hV(e.hW);
}
_.hA.biT = o;
_.hA.biS = 0;
_.hA.biQ.length = 0;
_.hq(b.default);
_.hq(w.default);
_.hq(u.default);
if (_.hM) {
M.$jL.bni(_);
_.hq(B.default);
}
switch (n.type) {
case 1:
_.hn(b.default).bgK = e.hg;
break;

case 2:
_.hn(u.default);
var F = _.hB;
F.bgt = e.hg;
F.bgu = a.bhF;
F.bgq = n.attr;
F.bgv = {};
F.bgw = 0;
F.bgx = 0;
F.bgy = 0;
F.bgz = 0;
F.bgp = n.type;
var I = n.attr.damageFactor;
if (n.attr.skId) {
F.bgE = n.attr.skIndex;
var R = e.hv;
R || (T = v.bll(F.bgu)) && (R = T.hv);
if (R) {
var Y = R.beg(n.attr.skId);
if (Y) {
F.bgB = 2 !== Y.Dg.damageType;
F.bgD = n.attr.skId;
F.bgA = Y.Dl(n.attr.skIndex) / O.GameAttributeConstRatioRange;
}
} else {
F.bgD = 0;
F.bgB = 2 !== n.attr.damageType;
F.bgA = 0;
}
} else {
F.bgD = 0;
var T = v.bll(a.bhF);
F.bgA = T ? E.default.CW(I, T, null, null) : 0;
0 === n.attr.damageType && T.hv ? F.bgB = T.hv.bfc >= T.hv.bfd : F.bgB = 2 !== n.attr.damageType;
}
if (n.attr.target) switch (n.attr.target.type) {
case 2:
var C = a.bhA.atkTargetEid;
F.bgC = "number" == typeof C ? C : -1;
break;

case 3:
var G = a.bhI[0];
G && (F.bgC = G.eid);
} else F.bgC = -1;
break;

case 3:
(S = _.hn(w.default)).bgF = e.hg;
S.bgG = n.attraction;
S.bgH = n.attraction.velX ? E.default.CW(n.attraction.velX, e, null, null) : 0;
S.bgI = n.attraction.velY ? E.default.CW(n.attraction.velY, e, null, null) : 0;
S.bgJ = n.attraction.velZ ? E.default.CW(n.attraction.velZ, e, null, null) : 0;
break;

case 4:
var S;
(S = _.hn(B.default)).bgL = e.hg;
S.bgM = n.event;
S.bgN.length = 0;
}
t.Fq(_.hw, e.hw);
var A = d[0].collider;
_.hA.biU = void 0 === n.scale ? 1 : E.default.CW(n.scale, e, null, null);
A ? t.FX(_.hA, A) : _.hA.biS = 0;
}, P = 0; P < g.length; P++) j(P);
for (P = g.length; P < m.length; P++) {
var x = m[P];
if (x) {
var k = v.bll(x.f);
k && k.ib();
}
}
m.length = g.length;
var _ = a.bid;
_.b = 0;
_.c = [ {
region: [ 0, p ],
anim: {}
} ];
var F = _.c, I = F[0], R = l.fs || [];
for (P = 0; P < R.length; P++) {
var Y = R[P], T = Y.t - c;
if (T > 0) {
if (T > p) break;
var C = F.length - 1;
F[C].region[1] = T;
if (T <= p) if (F[C + 1]) {
var G = F[C + 1];
G.region[0] = T;
G.region[1] = p;
G.anim = Y.a;
} else F[C + 1] = {
region: [ T, p ],
anim: Y.a
};
} else F[0].anim = Y.a;
}
a.bhY.f = I.anim.x;
a.bhY.g = I.anim.y;
a.bhY.h = I.anim.a;
}
}
};
t.EK = function(e) {
var i;
(null === (i = e.hE) || void 0 === i ? void 0 : i.bhI.length) && t.EV(e, {
z: 320,
hitDirType: 0,
force: 300
});
};
t.EL = function(e, i, n, r) {
var o = e.hE.bhO;
o.hurtbackType = g.$uo.$uN.b;
o.slideTime = i;
o.actId = r;
t.EK(e);
t.EI(e, g.$uo.$uP.f);
o.hurtbackTime = n;
};
t.EM = function(e) {
var i = e.hE.bhO;
i.hurtbackType = g.$uo.$uN.h;
i.slideTime = 0;
i.actId = 0;
t.EK(e);
t.EI(e, g.$uo.$uP.i);
t.Fm(e.hz, 0, 0, 0);
i.hurtbackTime = .9;
};
t.EN = function(e) {
var i = e.hE.bhO;
i.hurtbackType = g.$uo.$uN.d;
i.slideTime = 0;
i.actId = 0;
t.EK(e);
t.EI(e, g.$uo.$uP.f);
i.hurtbackTime = 999999;
};
t.EO = function(e) {
e.hE && t.EF(e, g.$uo.$uP.h);
};
t.EP = function(e) {
e.hq(m.default);
var i = e.hi.bll(e.hE.bhJ);
if (i && i.hE) for (var n = i.hE.bhI, r = 0; r < n.length; r++) if (n[r].eid === e.hg) {
n.splice(r, 1);
break;
}
e.hE.bhJ = -1;
e.hE.bhI.length && t.EV(e, {
z: 320,
hitDirType: 0,
force: 300
});
};
t.EQ = function() {};
t.ER = function(e) {
for (var i = e.hE, r = e.hy, o = e.hv, a = i.bhp, u = i.bhD, b = 0, h = a.length; b < h; b++) {
if (void 0 === (m = a[b]).tts) {
if (u[b] >= 1) continue;
} else if (0 === m.tts) ; else if (u[b] >= m.tts) continue;
var l = !0;
if ((y = m.tgs) && y.length > 0) for (var f = 0, c = y.length; f < c; f++) {
var s = y[f], p = n.default.CT(e, null, s, null);
if (s[2] ? p : !p) {
l = !1;
break;
}
}
if (l) {
u[b] ? u[b] += 1 : u[b] = 1;
if (1 === n.default.CU(e, null, m, null) || m.break) break;
}
}
for (var d = i.bhr, v = i.bhE, $ = (b = 0, d.length); b < $; b++) {
var m, y;
if (void 0 === (m = d[b]).tts) {
if (v[b] >= 1) continue;
} else if (0 === m.tts) ; else if (v[b] >= m.tts) continue;
for (l = !0, f = 0, c = (y = m.tgs).length; f < c; f++) {
s = y[f], p = n.default.CT(e, null, s, null);
if (s[2] ? p : !p) {
l = !1;
break;
}
}
if (l) {
if (o && (m.sid || m.csid)) {
if (0 === r.mainTimeScale) continue;
var g = null;
if (m.sid) {
var D = m.sid;
g = o.beg(D);
} else for (var j = m.csid, P = o.bdU, E = 0, x = P.length; E < x; E++) if (P[E].Dg.commonType === j) {
g = P[E];
break;
}
if (!g) continue;
if (!t.Ff(e, g)) continue;
t.Fg(e, g);
if (e.hL) {
var O = e.hL.bjH[g.Dg.id];
O && (e.hx.bjZ(O).f = -9999);
}
}
v[b] ? v[b] += 1 : v[b] = 1;
if (1 === n.default.CU(e, null, m, null) || m.break) break;
}
}
};
t.ES = function(t, e) {
var i, r = t.hE, o = r.bhq;
r.bhq = null;
if (o) {
for (var a = null === (i = r.bhy) || void 0 === i ? void 0 : i.ssid, u = 0, b = r.bhp; u < b.length; u++) if ((s = b[u]).autoClear) {
if (s.notClearInSid && (null == e ? void 0 : e.ssid) === a) continue;
n.default.CV(t, null, s);
}
if (r.bhr.length > 0) for (var h = 0, l = r.bhr; h < l.length; h++) if ((s = l[h]).autoClear) {
if (s.notClearInSid && (null == e ? void 0 : e.ssid) === a) continue;
n.default.CV(t, null, s);
}
if (o.length > 0) for (var f = 0, c = o; f < c.length; f++) {
var s;
if (!(s = c[f]).notClearInSid || (null == e ? void 0 : e.ssid) !== a) if (0 === s.tgs.length) n.default.CU(t, null, s); else {
for (var p = !0, d = 0; d < s.tgs.length; d++) {
var v = s.tgs[d], $ = n.default.CT(t, null, v);
if (v[2] ? $ : !$) {
p = !1;
break;
}
}
p && n.default.CU(t, null, s);
}
}
}
};
t.ET = function() {};
t.EU = function() {};
t.EV = function(e, i) {
var n, r = e.hE;
if (r && !(r.bhI.length < 1)) {
for (var o = 0, a = r.bhI; o < a.length; o++) {
var u = a[o], b = e.hi.bll(u.eid);
if (b) {
b.hE.bhJ = -1;
b.hq(m.default);
if (b.hw.z <= 0 && b.hv && (b.hE.bim || b.im || (null === (n = b.hv) || void 0 === n ? void 0 : n.beS))) t.EC(b); else {
b.hE.bhT = 0;
t.Fu(e, e.ic);
var h = t.Fz(e, b, i.hitDirType);
t.EZ(b, i, h);
}
}
}
r.bhI.length = 0;
}
};
t.EW = function(t) {
var e = t.hE;
if (e) {
var i = t.hi, n = i.bll(e.bhG);
e.bhG = -1;
if (n && n.hE && (a = n.hE.bhH).length) for (var r = 0; r < a.length; r++) if (a[r].eid === t.hg) {
a.splice(r, 1);
break;
}
t.hq(m.default);
var o = i.bll(e.bhJ);
e.bhJ = -1;
if (o && o.hE) {
var a = o.hE.bhI;
for (r = 0; r < a.length; r++) if (a[r].eid === t.hg) {
a.splice(r, 1);
break;
}
}
}
};
t.EX = function(e) {
if (e.hE.bhI.length > 0) for (var i = 0, n = e.hE.bhI; i < n.length; i++) {
var r = n[i], o = e.hi.bll(r.eid);
o && t.EY(e, o);
}
};
t.EY = function(e, i) {
var n, r = e.hE, o = r.bhL;
if (o) {
var a, u = r.bhK;
if (o.createEnt) if (i.hJ) u = i.hJ.bix; else {
var b = o.createEnt, h = t.FB(e, b, {
target: i
});
u = h.hg;
b.id && b.behaviorId && !b.ai && e.hE.bhH.push({
id: b.id,
eid: h.hg
});
}
t.EJ(i, o.action);
if (null == (a = -1 !== u ? e.hi.bll(u) : e) ? void 0 : a.hw) {
var l = a.hw, f = i.hw;
if ((null === (n = i.hv) || void 0 === n ? void 0 : n.beS) || i.im || "number" != typeof o.z || !i.hE.bim) {
o.dir && (i.hw.bkw = a.hw.bkw * o.dir);
if ("number" == typeof o.x) {
var c = i.hn(m.default);
c.bix = a.hg;
c.biD = null;
c.biC = o.dir || 1;
c.biy = 0;
c.biz = "number" == typeof o.x ? o.x : (f.x - l.x) * (0 !== c.biC ? l.bkw : 1);
c.biA = "number" == typeof o.y ? o.y : f.y - l.y;
c.biB = "number" == typeof o.z ? o.z : f.z - l.z;
} else {
t.Fm(i.hz, 0, 0, 0);
i.hq(m.default);
}
}
}
}
};
t.EZ = function(e, i, n) {
var r = e.hE.bhO;
r.hurtbackType = g.$uo.$uN.e;
r.slideTime = 0;
r.actId = 0;
t.EK(e);
e.hE.bhW = 1;
var o = i.z, a = e.hE.bhT > 1;
if (i.fly) {
i.groundBounce && (e.hE.bhU = i.groundBounce);
i.wallBounce && (e.hE.bhV = i.wallBounce);
}
i.z > 0 && e.hw.z <= 0 && (e.hw.z = 1);
if (-1 === i.groundBounce) {
e.hE.bhT = 2;
a = !0;
}
if (a) {
var u = e.hN;
if (u && e.il() <= 0 && e.hU) {
r.hurtbackTime = 0;
t.Fc(e);
u.bgV && t.FN(e, 11005, null, e.hg);
} else {
o = D.$jB.bma(.65 * o * e.ij(), 3);
t.EF(e, g.$uo.$uP.e);
t.Fm(e.hz, n * i.force, 0, o);
r.hurtbackTime = 99999;
e.ig();
}
} else {
e.hE.bhT = 1;
t.EF(e, g.$uo.$uP.e);
t.Fm(e.hz, n * i.force, 0, D.$jB.bma(o * e.ij()));
r.hurtbackTime = 99999;
e.ig();
}
};
t.Fa = function(e, i) {
var n = e.hE.bhO;
n.hurtbackType = g.$uo.$uN.f;
n.slideTime = 0;
n.actId = 0;
t.EK(e);
e.hE.bhW = 1;
t.EF(e, g.$uo.$uP.g);
n.hurtbackTime = i;
e.ii();
};
t.Fb = function(t) {
t.hE.bhT = 0;
t.hE.bhU = 0;
t.hE.bhV = 0;
t.hE.bhW = 1;
};
t.Fc = function(e) {
var i, n;
t.Fm(e.hz, 0, 0, 0);
t.Fb(e);
if (e.hU) {
e.id ? e.hi.blc.gN && t.FN(e, 11003, null, e.hg) : (null === (i = e.hN) || void 0 === i ? void 0 : i.bgV) && t.FN(e, 11004, null, e.hg);
(null === (n = e.hv) || void 0 === n ? void 0 : n.bfD()) ? t.EL(e, 0, 0, g.$uo.commonHitActions[1]) : t.EC(e);
t.Gu(e, null, g.$uo.$vh.j, null);
}
};
t.Fd = function(t, e) {
var i;
return !(null === (i = t.hv) || void 0 === i || !i.bez || e.DO || t.hi.blc.fN);
};
t.Fe = function(e, i) {
var n;
if (e.hL) {
i.DO = i.DP;
i.DY && i.DY.buffid && t.FN(e, i.DY.buffid, null !== (n = i.DY.type) && void 0 !== n ? n : null, e.hg, null);
}
};
t.Ff = function(t, e) {
return !(null == e || !e.Dc || t.hv.bew < t.hv.bfC(e) || e.Db > 0 && t.hv.beG() < e.Db || (e.CZ > 0 ? e.CY <= 0 : e.cd));
};
t.Fg = function(e, i) {
var n, r = e.hv;
if (r) {
if (i.CZ > 0) i.CY -= 1; else {
i.maxCdTime = r.bfB(i);
i.cd = i.maxCdTime;
}
if (i.Da) {
r.bew = Math.max(0, r.bew - r.bfC(i));
e.hi.blc.gu(g.$uo.$vk.A, e);
}
if (i.Db) {
var o = r.beu, a = o - Math.max(1, o - Math.floor(D.$jB.bma(r.bev * (i.Db / O.GameAttributeConstRatioRange))));
e.hX(e, a);
}
e.hv && e.hi.blc.gu(g.$uo.$vk.u, e, i.CX);
if (null === (n = i.Dg) || void 0 === n ? void 0 : n.selectPlayer) {
var u = e.hx.bjT.find(function(t) {
return t.d === i.CX;
});
if (u) {
var b = e.hi.blc.fF.find(function(t) {
return t.cXO === u.b;
});
b && (i.Df = b.cXZ);
}
}
t.Gu(e, null, g.$uo.$vh.b, i);
}
};
t.Fh = function() {
return !0;
};
t.Fi = function() {};
t.Fj = function(t, e) {
for (var i = 0; i < e.length; i++) {
var r = e[i];
if (r[0] !== g.$uo.$vf.c && r[0] !== g.$uo.$vf.m && r[0] !== g.$uo.$vf.M) {
var o = n.default.CT(t, null, r, null);
if (r[2] ? o : !o) return !1;
}
}
return !0;
};
t.Fk = function(t, e, i) {
var n = t.hx;
if (!n) return 0;
var r = t.hw;
if (!r) return 0;
var o = new j.default(r.x, r.y), a = e.bmq(o, new j.default()), u = D.$jB.blZ(a.bmp());
if (a.bmt() < i * i) n.bjN = 0; else {
var b = o.bmn(1, 0), h = D.$jB.bma(Math.atan2(u.y, u.x), 3);
if (1 === n.bjN) {
var l = D.$jB.bma(h - n.bjP);
if (l > .06) {
var f = 2 * Math.PI;
(l %= f) > Math.PI ? l -= f : l < -Math.PI && (l += f);
l > Math.PI / 3 && (h = D.$jB.bma(n.bjP + .5 * l, 3));
}
} else n.bjN = 1;
D.$jB.blX(b, h);
n.bjO.bmn(b.x, b.y);
n.bjP = h;
}
};
t.Fl = function(t) {
return t.hy ? t.hy.actionTimeScale : 1;
};
t.Fm = function(t, e, i, n) {
t.biZ = e;
t.bja = i;
t.bjb = n;
t.bjc = 0;
t.bjd = 0;
t.bje = 0;
t.bjg = 0;
t.bjk = null;
return t;
};
t.Fn = function(t, e) {
t.bji = e;
};
t.Fo = function(t, e) {
t.bjh = e;
};
t.Fp = function(t, e, i, n, r, o) {
void 0 === r && (r = g.$uo.$uU.b);
void 0 === o && (o = 1e4);
t.x = e;
t.y = i;
t.z = n;
t.bkw = r;
t.bkx = 0;
t.bky = o;
};
t.Fq = function(t, e) {
t.x = e.x;
t.y = e.y;
t.z = e.z;
t.bkw = e.bkw;
t.bkx = e.bkx;
t.bky = e.bky;
};
t.Fr = function() {};
t.Fs = function() {};
t.Ft = function(t, e) {
var i, n = t.hE;
if (t.hF) {
var r = e.anim;
if (void 0 !== e.transfer && t.hF.bgd.transfer) {
var o = t.ho(F.default), a = null !== (i = null == o ? void 0 : o.bkc) && void 0 !== i && i, u = t.hF.bgd.transfer;
r = e.transfer ? a && u.bossOpen || u.open : a && u.bossClose || u.close;
}
n.bhZ = t.hi.blc.fi;
n.bhY = {
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
t.Fu = function(t, e) {
var i;
void 0 === e && (e = !0);
var n = t.hw;
if (e) {
var r = t.hi.bln(d.default);
n.x = Math.max(Math.min(n.x, r.bkm), r.bkl);
n.y = Math.max(Math.min(n.y, r.bko), r.bkn);
n.z = Math.max(Math.min(n.z, 5e3), 0);
}
if (null === (i = t.hz) || void 0 === i ? void 0 : i.bjk) {
var o = t.hz.bjk;
if (1 === o.type) {
var a = o.param[0], u = o.param[1], b = o.param[2];
if (a > 0) {
var h = n.x - u, l = n.y - b, f = D.$jB.bma(h * h + l * l, 5);
if (f > a * a && f > 0) {
var c = D.$jB.bma(a / Math.sqrt(f));
n.x = D.$jB.bma(u + h * c, 3);
n.y = D.$jB.bma(b + l * c, 3);
}
}
}
}
};
t.Fv = function(e, i, n, r) {
var o = t.Ec, a = D.$jB.bma(2 * (e * n + i * r), 5);
o.x = D.$jB.bma(e - n * a, 3);
o.y = D.$jB.bma(i - r * a, 3);
return o;
};
t.Fw = function(e, i, n, r) {
var o = this.Eb;
o.x = D.$jB.bma(i * e.bkw + e.x, 3);
o.y = D.$jB.bma(n + e.y, 3);
o.z = D.$jB.bma(r + e.z, 3);
return t.Eb;
};
t.Fx = function(e, i, n, r, o, a) {
var u = D.$jB.bma(n + e, 3), b = D.$jB.bma(r + i, 3);
-1 === o && (b = D.$jB.bma(-1 * b + 2 * i, 3));
D.$jB.blV(e, i, u, b, a, t.Ec);
return t.Ec;
};
t.Fy = function(e, i) {
var n = D.$jB.bma(e * e + i * i, 5);
if (n > 0) {
n = D.$jB.bma(1 / Math.sqrt(n), 5);
e = D.$jB.bma(e * n, 5);
i = D.$jB.bma(i * n, 5);
} else {
e = 1;
i = 0;
}
var r = t.Ec;
r.x = e;
r.y = i;
return r;
};
t.Fz = function(t, e, i) {
var n = g.$uo.$uU.c;
switch (i) {
case g.$uo.$uZ.b:
n = t.hw.bkw;
break;

case g.$uo.$uZ.c:
n = -1 * t.hw.bkw;
break;

case g.$uo.$uZ.d:
n = Math.sign(e.hw.x - t.hw.x);
break;

case g.$uo.$uZ.e:
n = Math.sign(t.hw.x - e.hw.x);
break;

case g.$uo.$uZ.f:
n = e.hw.bkw;
break;

case g.$uo.$uZ.g:
n = -1 * e.hw.bkw;
}
return n || g.$uo.$uU.c;
};
t.FA = function() {
return !1;
};
t.FB = function(e, i, n) {
var o, a, u, b, h, l, f, c, p, v, m, y, g, j, P, x, k, _, w, F = e.hi, B = e.hw, M = (null !== (o = i.dir) && void 0 !== o ? o : 1) * B.bkw, I = 0, R = 0, Y = 0;
if (i.createSceneRandomPos) {
var T = e.hi.bln(d.default);
I = D.$jB.bma(F.blc.gg(T.bkl, T.bkm), 3);
R = D.$jB.bma(F.blc.gg(T.bkn, T.bko), 3);
Y = 0;
} else if (i.randomInCircle) {
var C = i.randomInCircle, G = D.$jB.bma(F.blc.gg(0, 2 * Math.PI), 6), S = D.$jB.bma(F.blc.gg(C.innerR, C.outerR));
I = D.$jB.bma(B.x + (null !== (a = i.x) && void 0 !== a ? a : 0) * B.bkw + D.$jB.bma(Math.cos(G) * S), 3);
R = D.$jB.bma(B.y + (null !== (u = i.y) && void 0 !== u ? u : 0) + D.$jB.bma(Math.sin(G) * S), 3);
Y = 0;
} else if (1 === i.entPos) {
if (n) {
var A = n;
I = null !== (b = A.hitX) && void 0 !== b ? b : 0;
R = null !== (h = A.hitY) && void 0 !== h ? h : 0;
Y = null !== (l = A.hitZ) && void 0 !== l ? l : 0;
}
} else if (2 === i.entPos) {
if (n) {
var X = n.target;
I = (N = t.Fw(X.hw, D.$jB.bma((null !== (f = i.x) && void 0 !== f ? f : 0) + (i.xRand ? i.xRand * F.blc.ge() : 0), 3), D.$jB.bma((null !== (c = i.y) && void 0 !== c ? c : 0) + (i.yRand ? i.yRand * F.blc.ge() : 0), 3), D.$jB.bma((null !== (p = i.z) && void 0 !== p ? p : 0) + (i.zRand ? i.zRand * F.blc.ge() : 0), 3))).x;
R = N.y;
Y = N.z;
}
} else {
var N;
I = (N = t.Fw(B, D.$jB.bma((null !== (v = i.x) && void 0 !== v ? v : 0) + (i.xRand ? i.xRand * F.blc.ge() : 0), 3), D.$jB.bma((null !== (m = i.y) && void 0 !== m ? m : 0) + (i.yRand ? i.yRand * F.blc.ge() : 0), 3), D.$jB.bma((null !== (y = i.z) && void 0 !== y ? y : 0) + (i.zRand ? i.zRand * F.blc.ge() : 0), 3))).x;
R = N.y;
Y = N.z;
}
"number" == typeof i.worldZ && (Y = i.worldZ);
"number" == typeof i.worldX && (I = i.worldX);
"number" == typeof i.worldY && (R = i.worldY);
var z = e.hD ? e.hD.biI : 0, Z = -1, L = -1;
if (!i.ai) {
Z = e.hg;
L = e.hE ? e.hE.bhF : -1;
}
var K = F.ble();
t.FF(K);
t.FL(K);
t.Ev(K, z, 0, -1);
var Q = K.hn(s.default);
t.Fm(Q, 0, 0, 0);
t.Fn(Q, "number" != typeof i.inside || Boolean(i.inside));
var V = K.hn($.default);
t.Fp(V, I, R, Y, M, i.zorder);
var J = {};
if (i.extendsVals) for (var H = e.hE.bhA, U = 0, W = i.extendsVals; U < W.length; U++) J[it = W[U]] = H[it] || 0;
if (i.initVars) for (var q = 0, tt = i.initVars; q < tt.length; q++) {
var et = tt[q], it = et[0], nt = et[1];
J[it] = E.default.CW(nt, e, null);
}
if (i.attrInit || i.valueId) {
var rt = e.hv;
if (!rt) {
var ot = e.hi.bll(e.hE.bhF);
ot && (rt = ot.hv);
}
var at = null, ut = 0;
if (i.valueId) {
var bt = $fc.cfgMgr.tbl.EnemyValueTable.findByKey(i.valueId);
ut = bt.level;
at = O.default.bEC.bFH(bt);
}
if (rt && i.attrInit) {
at = at || {};
ut = ut || rt.bey;
for (var ht = rt.bdS.cVN, lt = 0, ft = i.attrInit; lt < ft.length; lt++) {
var ct = ft[lt], st = ct[0], pt = E.default.CW(null !== (g = ct[1]) && void 0 !== g ? g : 1, e, null), dt = E.default.CW(null !== (j = ct[2]) && void 0 !== j ? j : 0, e, null);
at[st] = Math.floor((null !== (P = ht.cVt(st)) && void 0 !== P ? P : 0) * pt) + dt;
}
}
if (at) {
K.hn(r.default).bed();
t.El(K, at, i.roleId, ut, null);
}
}
if (i.projectile) if ("number" == typeof i.projectile.angle) {
G = D.$jB.bma(i.projectile.angle * Math.PI / 180, 6);
var vt = D.$jB.bma(D.$jB.bma(Math.cos(G)) * i.projectile.speed) * K.hw.bkw, $t = D.$jB.bma(D.$jB.bma(Math.sin(G)) * i.projectile.speed);
this.Fm(Q, vt, $t, null !== (x = i.projectile.speedZ) && void 0 !== x ? x : 0);
} else this.Fm(Q, D.$jB.bma((null !== (k = i.projectile.speedX) && void 0 !== k ? k : 0) * K.hw.bkw), null !== (_ = i.projectile.speedY) && void 0 !== _ ? _ : 0, null !== (w = i.projectile.speedZ) && void 0 !== w ? w : 0);
i.appearanceId && t.Ew(K, i.appearanceId);
if (i.behaviorId) {
t.Ez(K, i.behaviorId, Z, L, i.startStateId, J);
e.hv && (K.hE.bhQ = -1 !== e.hE.bhF ? e.hE.bhF : e.hg);
}
var mt = 0;
if (K.hv) {
if ("number" == typeof i.camp) t.Ev(K, i.camp, 0, -2); else {
e.hD.biJ > 0 && (mt = O.GameEnemyTypeTableEnum.Normal);
t.Ev(K, z, mt, -2);
}
t.Ex(K, 1);
}
if (i.bindHitTarget) {
var yt = n && n.target;
if (yt && yt.hw) {
var gt = D.$jB.bma(K.hw.x - yt.hw.x + 0) * yt.hw.bkw, Dt = D.$jB.bma(K.hw.y - yt.hw.y + 0), jt = D.$jB.bma(K.hw.z - yt.hw.z + 0);
t.FC(K, yt, {
dir: 1,
x: gt,
y: Dt,
z: jt
});
}
}
if (i.ai) {
t.Es(K, i.ai);
t.FZ(K, !0);
}
return K;
};
t.FC = function(e, i, n) {
var r, o, a, u;
if (i.hw) {
e.hJ && t.FD(e);
var b = e.hn(m.default);
b.bix = i.hg;
b.biC = null !== (r = n.dir) && void 0 !== r ? r : 1;
b.biz = n.x;
b.biA = n.y;
b.biB = n.z;
b.biD = null !== (o = n.fz) && void 0 !== o ? o : null;
b.biy = null !== (a = n.type) && void 0 !== a ? a : 0;
if (1 === n.type) {
b.biE = null !== (u = n.surroundSpeed) && void 0 !== u ? u : 0;
var h = t.Ec;
h.x = D.$jB.bma(e.hw.x - i.hw.x);
h.y = D.$jB.bma(e.hw.y - i.hw.y);
var l = D.$jB.bma(Math.atan2(h.y, h.x));
b.biG = D.$jB.bma(180 * l / Math.PI);
b.biF = D.$jB.bma(h.bmr());
b.biH = 0;
} else {
b.biE = 0;
b.biG = 0;
b.biF = 0;
b.biH = 0;
}
i.hE.bhM.push({
id: 0,
eid: e.hg
});
}
};
t.FD = function(t) {
if (t.hJ) {
var e = t.hi.bll(t.hJ.bix);
if (null == e ? void 0 : e.hE) {
var i = e.hE.bhM.findIndex(function(e) {
return e.eid === t.hg;
});
-1 !== i && e.hE.bhM.splice(i, 1);
}
t.hq(m.default);
}
};
t.FE = function(t, e) {
if (t.hE && t.hE.bhM) for (var i = t.hE.bhM.length - 1; i >= 0; i--) {
var n = t.hE.bhM[i];
if (-1 == e || n.id === e) {
var r = t.hi.bll(n.eid);
if (r) {
r.hq(m.default);
t.hE.bhM.splice(i, 1);
}
}
}
};
t.FF = function(e) {
var i = e.hn(p.default);
i.bjM = 0;
i.bjK = "";
i.bjW = 1;
i.bjV = !1;
t.FG(e);
return i;
};
t.FG = function(t) {
var e = t.hx;
if (e) {
e.bjO.bmn(0, 0);
e.bjP = 0;
e.bjN = -1;
e.bjR.forEach(function(t) {
t.c = 0;
t.b = 0;
t.d = 0;
t.e = 0;
t.f = -9999;
});
e.bjS.forEach(function(t) {
t.c = 0;
t.b = 0;
t.d = 0;
t.e = 0;
t.f = -9999;
});
e.bjT.length = 0;
}
};
t.FH = function(t, e) {
if (e) {
var i = t.bjR.get(e);
if (!i) {
i = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
t.bjR.set(e, i);
}
return i;
}
};
t.FI = function(t, e) {
var i = t.bjS.get(e);
if (!i) {
i = {
b: 0,
c: 0,
d: 0,
e: 0,
f: -9999
};
t.bjS.set(e, i);
}
return i;
};
t.FJ = function(t, e) {
return t.hL ? t.hL.bjH[e] : 0;
};
t.FK = function(t, e, i) {
if (t.bku < i) {
t.hitPauseTimeScale = e;
t.bku = i;
}
};
t.FL = function(t) {
var e = t.hn(v.default);
e.actionTimeScale = 1;
var i = t.hi.bln(v.default);
e.worldTimeScale = i.worldTimeScale;
e.hitPauseTimeScale = 1;
e.mainTimeScale = 1;
e.bku = 0;
e.actionTimeScale = 1;
return e;
};
t.FM = function(e, i, r, o) {
for (var a = 0, u = r; a < u.length; a++) {
var b = u[a], h = !0, l = b.tgs;
if (l && l.length > 0) for (var f = 0, c = l.length; f < c; f++) {
var s = l[f], p = n.default.CT(e, i, s, o);
if (s[2] ? p : !p) {
h = !1;
break;
}
}
if (h) {
b.cs ? t.FM(e, i, b.cs, o) : n.default.CU(e, i, b, o);
if (b.break) break;
} else b.fcs && t.FM(e, i, b.fcs, o);
}
};
t.FN = function(e, i, n, r, o) {
var a, u, b, h, l = e.hE;
if (l) {
var f = O.default.bEC.bFf(i);
if (f) {
if (f.abnormalType) {
if (null === (a = e.hv) || void 0 === a ? void 0 : a.bek) return;
if (f.abnormalType <= g.$uo.$va.j) {
if (l.bih || l.bik || !e.hv || !e.hv.bez) return;
switch (f.abnormalType) {
case g.$uo.$va.f:
for (var c = 0, s = l.bhs; c < s.length; c++) ($ = s[c]).Dq.abnormalType === g.$uo.$va.f && t.FP(e, $, !1);
}
}
}
var p = null !== (u = f.addType) && void 0 !== u ? u : 1;
if (0 !== p) switch (p) {
case 1:
if (l.bht[i]) return;
break;

case 2:
if (l.bht[i]) for (var d = 0, v = l.bhs; d < v.length; d++) {
var $;
if (($ = v[d]).Dp === i) {
$.Dw = -1;
$.Dx = 0;
return;
}
}
break;

case 3:
l.bht[i] && t.FQ(e, i);
}
var m = new k.$ge.$gg();
m.DI(i);
m.DA = e.hg;
m.DB = null != r ? r : 0;
null === (b = m.Dq.param) || void 0 === b || b.forEach(function(t, i) {
var r;
m.Dr.push(E.default.CW(null !== (r = n && n[i]) && void 0 !== r ? r : t, e, m, o));
});
l.bip(m);
if (f.specialType) switch (f.specialType) {
case g.$uo.$vc.b:
m.DF = Math.ceil(null !== (h = m.Dr[0]) && void 0 !== h ? h : 0);
e.ia();
break;

case g.$uo.$vc.c:
if (e.hv) {
var y = e.hv.bfH();
if (y) {
m.DF = y;
e.hv.beL(y, 1);
}
}
}
m.Dw = -1;
m.Dx = 0;
m.Du = E.default.CW(f.duration, e, m);
m.Dv = f.interval ? E.default.CW(f.interval, e, m) : 0;
m.Dy = m.Dv;
if (m.Dq.attr) for (var D = 0, j = m.Dq.attr; D < j.length; D++) {
var P = j[D];
m.Ds.push([ P[0], m.Dr[P[1]] ]);
}
if (m.Dq.attr2) for (var x = 0, _ = m.Dq.attr2; x < _.length; x++) {
P = _[x];
m.Dt.push([ P[0], m.Dr[P[1]] ]);
}
if (e.hv) {
for (var w = e.hv, F = 0, B = m.Ds; F < B.length; F++) {
var M = B[F];
w.beJ(M[0], M[1]);
}
for (var I = 0, R = m.Dt; I < R.length; I++) {
M = R[I];
w.beL(M[0], M[1]);
}
}
f.abnormalType && t.FR(e, m);
f.enterLg && m.DA === e.hg && t.FM(e, m, m.Dq.enterLg, null);
}
}
};
t.FO = function(t, e) {
if (e.Dq.status) for (var i = 0, n = e.Dq.status; i < n.length; i++) {
var r = n[i];
switch (r[0]) {
case "lockBtn":
break;

case "lockMinHP":
if (!t.hv) break;
for (var o = 0, a = 0, u = t.hE.bhs; a < u.length; a++) {
var b = u[a];
if (b.Dq.status) for (var h = 0, l = b.Dq.status; h < l.length; h++) {
var f = l[h];
f[0] === r[0] && o < f[1] && (o = f[1]);
}
}
t.hv.lockMinHP = o;
}
}
};
t.FP = function(e, i, n) {
var r;
if (i.DA !== e.hg) return !1;
if (-1 === i.Dz) return !1;
i.Dz = -1;
null === (r = e.hE) || void 0 === r || r.biq(i.Do);
var o = i.Dq;
if (n) o.deleteLg && t.FM(e, i, o.deleteLg, null); else {
if (o.specialType) switch (o.specialType) {
case g.$uo.$vc.c:
if (e.hv) {
var a = i.DF;
a && e.hv.beM(a, 1);
}
}
o.abnormalType && t.FS(e, i);
if (e.hv) {
for (var u = e.hv, b = 0, h = i.Ds; b < h.length; b++) {
var l = h[b];
u.beK(l[0], l[1]);
}
for (var f = 0, c = i.Dt; f < c.length; f++) {
l = c[f];
u.beM(l[0], l[1]);
}
}
o.exitLg && t.FM(e, i, o.exitLg, null);
o.deleteLg && t.FM(e, i, o.deleteLg, null);
}
return !0;
};
t.FQ = function(e, i) {
for (var n = e.hE.bhs, r = 0; r < n.length; r++) {
var o = n[r];
if (o.Dq.id === i) {
t.FP(e, o, !1);
break;
}
}
};
t.FR = function(e, i) {
var n;
if (e.hv && e.hE) {
var r = i.Dq.abnormalType, o = 1 << r - 1;
if (!e.hv.bdR.bnu(o)) {
e.hv.bdR.bnw(o);
if (r <= g.$uo.$va.j) switch (r) {
case g.$uo.$va.b:
t.EK(e);
0 === e.hE.bhO.hurtbackTime && t.EL(e, 0, 0, g.$uo.commonHitActions[1]);
break;

case g.$uo.$va.c:
case g.$uo.$va.g:
t.Fm(e.hz, 0, 0, 0);
t.EK(e);
t.Fb(e);
t.EL(e, 0, 0, g.$uo.commonHitActions[1]);
}
}
if (r === g.$uo.$va.k) {
var a = null === (n = i.Dq) || void 0 === n ? void 0 : n.abnormalAttr;
if (a) {
i.DD = 0;
i.DE = Math.ceil(D.$jB.bma(void 0 !== a[0] ? i.Dr[a[0]] : 0));
i.DC = void 0 !== a[1] ? i.Dr[a[1]] : 1;
}
}
}
};
t.FS = function(e, i) {
if (e.hv && e.hE) {
var n = i.Dq.abnormalType, r = 1 << n - 1;
if (e.hv.bdR.bnu(r) && !e.hE.bhs.find(function(t) {
return t.Dq.abnormalType === n;
})) {
e.hv.bdR.bnx(r);
if (n <= g.$uo.$va.j) switch (n) {
case g.$uo.$va.d:
case g.$uo.$va.e:
t.EC(e);
e.hG && (e.hG.bgn = g.$uo.$uS.b);
e.hx && (e.hx.bjN = 0);
break;

case g.$uo.$va.f:
break;

case g.$uo.$va.b:
case g.$uo.$va.c:
case g.$uo.$va.g:
0 === e.hE.bhO.hurtbackTime && t.EC(e);
}
}
}
};
t.FT = function(e, i, n) {
var r;
if (e.hv) {
i.DD = D.$jB.bma(i.DD + n);
if (i.DD > i.DC) {
i.DD = D.$jB.bma(i.DD - i.DC);
if (0 !== i.DE) {
var o = i ? e.hi.bll(i.DB) : null;
if (!e.hv.bez) return;
var a = e;
if ((null == o ? void 0 : o.hv) && (null === (r = null == a ? void 0 : a.hv) || void 0 === r ? void 0 : r.bez)) {
var u = e.hi.blc, b = t.Ef(o, o, a);
b.isPhysical = o.hv.bfc > o.hv.bfd;
b.elementValue = 0;
var h = t.FV(u, b, i.DE / O.GameAttributeConstRatioRange, !1);
e.hX(o, h);
!a.hv.bez && t.GJ(a, 0, 0);
u.gu(g.$uo.$vk.E, a, h, !1);
t.Eg(b);
}
}
}
}
};
t.FU = function(t) {
var e = t.hz.biZ;
if (e > 0) {
var i = t.hi.bln(d.default);
if (t.hw.x >= i.bkm) {
t.hz.biZ = D.$jB.bma(-1 * e * .2, 4);
if (t.hE.bhV) {
t.hE.bhT = 1;
t.hE.bhW = 1;
t.hz.bjb = t.hE.bhV;
t.hE.bhV = 0;
}
}
} else {
i = t.hi.bln(d.default);
if (t.hw.x <= i.bkl) {
t.hz.biZ = D.$jB.bma(-1 * e * .2, 4);
if (t.hE.bhV) {
t.hE.bhT = 1;
t.hE.bhW = 1;
t.hz.bjb = t.hE.bhV;
t.hE.bhV = 0;
}
}
}
};
t.FV = function(t, e, i, n) {
var r, o = e.atkEntity.hv, a = e.target.hv, u = O.GameAttributeConstRatioRange, b = e.isPhysical, h = O.default.bEC.bEA, l = 1;
if (e.elementValue > 0) {
var f = Number.MIN_SAFE_INTEGER;
if (e.elementValue & g.$uo.$uJ.b) {
f = Math.max(f, o.bfo - a.bfs);
g.$uo.$uJ.b;
}
if (e.elementValue & g.$uo.$uJ.c) {
f = Math.max(f, o.bfp - a.bft);
g.$uo.$uJ.c;
}
if (e.elementValue & g.$uo.$uJ.d) {
f = Math.max(f, o.bfq - a.bfu);
g.$uo.$uJ.d;
}
if (e.elementValue & g.$uo.$uJ.e) {
f = Math.max(f, o.bfr - a.bfv);
g.$uo.$uJ.e;
}
l = Math.max(D.$jB.bma(1 + h.streArg1 + D.$jB.bma(f / h.streArg2)), 0);
}
var c = o.bfe(b), s = o.bey, p = a.bff(b), d = Math.min(D.$jB.bma(D.$jB.bma(p / (s * h.defArg1 + p)) + o.bfg(b) / u), b ? h.maxApDefRate : h.maxSpDefRate), v = Math.ceil(o.bfh(b)), $ = D.$jB.bma(v / (a.bey * h.critArg1 + v * h.critArg2)), m = D.$jB.bma($ + o.bfi(b) / u), y = D.$jB.bma(o.bfj / u + h.baseCritDamage), j = o.bfw, P = Math.max(u + o.bfk - a.bfl, 0), E = o.bfm, x = o.bfn, k = 1;
(null === (r = e.hitInfo) || void 0 === r ? void 0 : r.skId) && (k = o.bdS.cVN.cVC(e.hitInfo.skId));
var _ = n && m > t.ge();
e.isCrt = _;
var w = D.$jB.bma(P / u * (u + E) / u * (u + x) / u * (u + j) / u * l * k * D.$jB.bma(1 - d) * (_ ? y : 1));
return Math.ceil(D.$jB.bma(c * D.$jB.bma(i * w)));
};
t.FW = function() {};
t.FX = function(t, e, i) {
t.biS = e.e;
t.biQ.length = 0;
t.biO.biN(e.y);
t.biP.biN(e.z);
t.biR = e.d || 0;
"number" == typeof i && (t.biU = i);
return t;
};
t.FY = function(t) {
return t < 90 && t > -90;
};
t.FZ = function(t, e) {
var i = t.hG;
i || (i = t.hn(a.default));
i.bgj = -1;
i.bgn = g.$uo.$uS.b;
i.bgi = e;
i.bgo = 0;
};
t.Ga = function(t) {
return t.bez;
};
t.Gb = function(t, e) {
return this.Gc(t.x, t.y, e.x, e.y);
};
t.Gc = function(t, e, i, n) {
var r = i - t, o = n - e;
return D.$jB.bma(180 * Math.atan2(o, r) / Math.PI, 4);
};
t.Gd = function(t, e) {
var i = e.x - t.x, n = e.y - t.y;
return D.$jB.bma(Math.sqrt(i * i + n * n), 3);
};
t.Ge = function(t, e) {
for (var i = 0, n = e; i < n.length; i++) {
var r = n[i];
t.findAnimation(r[0]) && t.findAnimation(r[1]) && t.setMix(r[0], r[1], r[2]);
}
};
t.Gf = function(t, e) {
t.bhg = e;
var i = O.default.bEC.bET(t.bhf), n = i && i[e];
if (n) {
t.bhh = n.cls;
t.bhi = n.status;
t.bhj = 0;
t.bhk = 0;
t.bhl = {};
}
};
t.Gg = function(e, i, n) {
var r = t.FH(e, i);
if (r) {
r.b = n ? 1 : 0;
n ? r.c = 1 : r.d = 1;
}
};
t.Gh = function(t, e) {
t.bkg = e;
var i = O.default.bEC.bES(e);
if (i) {
t.bkh = i.visibleArea[0];
t.bki = i.visibleArea[1];
t.bkj = i.visibleArea[2];
t.bkk = i.visibleArea[3];
t.bkl = i.walkableArea[0];
t.bkm = i.walkableArea[1];
t.bkn = O.default.bEC.bFs(i.walkableArea[2]);
t.bko = O.default.bEC.bFs(i.walkableArea[3]);
t.bkr = i;
t.bkp.length = 0;
}
};
t.Gi = function() {};
t.Gj = function(e) {
e.fr.bkW.forEach(function(e) {
e.hW ? e.hv && t.EC(e) : e.hi.blc.gB(e);
});
};
t.Gk = function(e) {
var i = e.fI;
(null == i ? void 0 : i.cYG) && i.cYu.forEach(function(n, r) {
if (n) {
var o = r + 1, a = i.cYG.transfers.find(function(t) {
return Number(t.id) == o;
});
if (a) {
var u = e.gL(i.cYs, i.cYt, o), b = e.fH.get(u);
if (b) {
var h = e.fr.ble(), l = h.hn(F.default);
l.bka = o;
l.bkd = 0;
l.bke = 0;
l.bkb = !1;
l.bkf = 0;
l.bkc = b.cYJ;
t.FL(h);
var f = h.hn($.default);
t.Fp(f, a.x, O.default.bEC.bFs(a.y), 0, 1);
t.Ew(h, a.resId);
t.Ez(h, T.cvBehaviorId.entityBehavior101);
}
}
}
});
};
t.Gl = function(e, i, n) {
var o = e.fr.ble();
i.cXE = o.hg;
t.FF(o);
t.FL(o);
t.FZ(o, !0);
t.Ev(o, i.cXB, i.cXr, n);
var a = o.hn(s.default);
t.Fm(a, 0, 0, 0);
t.Fo(a, 0);
t.Fn(a, !0);
var u = o.hn($.default);
t.Fp(u, i.cXu, i.cXv, i.cXw, i.cXy);
var b = o.hn(r.default).bed();
t.El(o, i.cXC, i.cXs, i.cXz, null);
t.Ew(o, b.bdQ.appearanceId);
t.Ez(o, b.bfI(e.gN));
t.Ex(o, 1);
var h = o.hn(I.default);
h.bha(1);
h.bgV = !1;
i.cXD && t.Es(o, i.cXD);
return o;
};
t.Gm = function(e) {
var i = e.fI;
i && i.cYC.forEach(function(n) {
if (n.cXH) {
if (i.cYF) return;
var o = e.fr.ble();
i.cYE = o.hg;
t.FL(o);
t.Ev(o, n.cXB, 0, 0);
var a = o.hn($.default);
t.Fp(a, n.cXu, n.cXv, n.cXw, n.cXy);
var u = o.hn(s.default);
t.Fm(u, 0, 0, 0);
t.Fo(u, 0);
t.Fn(u, !0);
o.hn(r.default).bed();
t.El(o, n.cXC, n.cXs, n.cXz, null);
t.Ew(o, 210200240);
t.Ez(o, T.cvBehaviorId.entityBehavior203);
t.Ex(o, 1);
} else if (!i.cYz) {
var b = e.fr.ble();
n.cXE = b.hg;
t.FF(b);
t.FL(b);
t.FZ(b, !0);
t.Ev(b, n.cXB, n.cXr, n.cXF);
if (!e.fM.n) if (n.cXr === O.GameEnemyTypeTableEnum.Boss) {
(h = b.hn(I.default)).bha(2);
h.bgV = !0;
} else if (n.cXr === O.GameEnemyTypeTableEnum.Elite) {
var h;
(h = b.hn(I.default)).bha(2);
}
var l = b.hn(s.default);
t.Fm(l, 0, 0, 0);
t.Fo(l, 0);
t.Fn(l, !0);
var f = b.hn($.default);
t.Fp(f, n.cXu, n.cXv, n.cXw, n.cXy);
var c = b.hn(r.default).bed();
t.El(b, n.cXC, n.cXs, n.cXz, null);
t.Ew(b, c.bdQ.appearanceId);
t.Ez(b, c.bfI(e.gN));
t.Ex(b, 1);
n.cXD && t.Es(b, n.cXD);
}
});
};
t.Gn = function(t, e) {
var i = $fc.cfgMgr.tbl.EnemyRandomTable.VOList.filter(function(t) {
return t.name === e;
});
if (0 === i.length) return null;
var n = t.gf(i.map(function(t) {
return t.weight;
}));
return i[n];
};
t.Go = function(e) {
var i = e.fI;
if (i && !i.cYF) {
i.cYF = !0;
e.gP();
e.fI.cYz = !1;
for (var n = 0, o = e.fE.bkN; n < o.length; n++) {
var a = o[n], u = a.ho(F.default);
u.bkb = !1;
u.bkd = 0;
u.bke = 0;
t.EF(a, "1");
}
t.Gf(e.fr.bln(c.default), g.$uo.$uQ.c);
e.gu(g.$uo.$vk.t);
i.cYC.forEach(function(i) {
if (i.cXH) {
var n = O.GameEnemyTypeTableEnum.Boss, o = t.Gn(e, 1);
if (o) {
var a = o.roleCfgId, u = O.default.bEC.bEW(a);
if (u) {
var b = e.fr.ble();
i.cXE = b.hg;
t.FF(b);
t.FL(b);
t.FZ(b, !0);
t.Ev(b, i.cXB, n, i.cXF);
var h = b.hn(I.default);
h.bha(2);
h.bgV = !0;
var l = b.hn(s.default);
t.Fm(l, 0, 0, 0);
t.Fo(l, 0);
t.Fn(l, !0);
var f = b.hn($.default);
t.Fp(f, i.cXu, i.cXv, i.cXw, i.cXy);
var c = b.hn(r.default).bed();
t.El(b, i.cXC, a, i.cXz, null);
t.Ew(b, c.bdQ.appearanceId);
t.Ez(b, c.bfI(e.gN));
t.Ex(b, 1);
t.FN(b, T.cvResUrl.buff_11003, [ 1 ], b.hg);
u.AI && t.Es(b, u.AI);
}
}
}
});
}
};
t.Gp = function(e, i) {
var n = e.fr, o = e.fI;
o && e.fF.forEach(function(a, u) {
var b, h, l = a.cYm[0], f = n.ble();
a.cXZ = f.hg;
a.cYa = f.hg;
t.FF(f).bjK = a.cXL;
var c = f.hn(x.default);
c.bjI(a.cXL, null === (b = l.personal) || void 0 === b ? void 0 : b.fashion, l.wear.equipList, null === (h = l.fashion) || void 0 === h ? void 0 : h.morphList, l.troop);
c.bjJ(l.roleCfgId, e.gM);
t.FL(f);
t.FZ(f, !0);
t.Ev(f, a.cXQ, 0, -1);
var p = f.hn(s.default);
t.Fm(p, 0, 0, 0);
t.Fo(p, 0);
t.Fn(p, !0);
var d = f.hn($.default);
if (i) {
var v = o.cYG.transfers.find(function(t) {
return Number(t.id) === i;
});
if (v) {
a.cYi = v.x;
a.cYj = O.default.bEC.bFs(v.y);
a.cYk = 0;
a.cYl = g.$uo.$uU.b;
}
} else {
var m = o.cYG.born || [ 500, 300, 1 ];
if (m) {
a.cYi = m[0];
a.cYj = O.default.bEC.bFs(m[1]) + 300 * (u > 0 ? u % 2 == 0 ? -Math.floor(u / 2) : Math.ceil(u / 2) : 0);
a.cYk = 0;
a.cYl = m[2];
}
}
t.Fp(d, a.cYi, a.cYj, a.cYk, a.cYl);
var y = f.hn(I.default);
y.bha(1, e.gN);
y.bgV = !1;
var D = f.hn(r.default).bed();
t.El(f, l.baseAttrs, l.roleCfgId, l.level, l);
var j = e.gC() ? 7100001 : D.bdQ.appearanceId;
t.Ew(f, j);
t.Ez(f, D.bfI(e.gN));
t.Ex(f, 1);
f.hV(!0);
l.appendInfo && l.appendInfo.assistCount > 0 && t.FN(f, 11010, [ 1e3 * l.appendInfo.assistCount, 1e3 * l.appendInfo.assistCount, 1e3 * l.appendInfo.assistCount ], f.hg);
a.cXP && t.Et(f);
});
};
t.Gq = function(e) {
var i = e.fr;
if (e.fI) {
var n = e.fr.bln(d.default), o = D.$jB.bma((n.bkn + n.bko) / 2), a = D.$jB.bma((n.bkl + n.bkm) / 2), u = {};
u[1] = [ {
x: D.$jB.bma(a - 400),
y: o,
dir: 1
} ];
u[2] = [ {
x: D.$jB.bma(a + 400),
y: o,
dir: -1
} ];
e.fF.forEach(function(t) {
t.cXQ = t.cXO;
});
e.fF.forEach(function(n) {
var o, a, b = u[n.cXQ];
if (b) {
var h = n.cYm[0], l = $fc.cfgMgr.tbl.MatchPvpAttrTable.findByKey(h.roleCfgId);
if (l) {
var f = i.ble();
n.cXZ = f.hg;
n.cYa = f.hg;
t.FF(f).bjK = n.cXL;
var c = f.hn(x.default);
c.bjI(n.cXL, null === (o = h.personal) || void 0 === o ? void 0 : o.fashion, h.wear.equipList, null === (a = h.fashion) || void 0 === a ? void 0 : a.morphList, h.troop);
c.bjJ(h.roleCfgId, e.gM);
t.FL(f);
t.FZ(f, !0);
t.Ev(f, n.cXQ, 0, -1);
var p = f.hn(s.default);
t.Fm(p, 0, 0, 0);
t.Fo(p, 0);
t.Fn(p, !0);
var d = f.hn($.default), v = b[0];
n.cYi = v.x;
n.cYj = v.y;
n.cYk = 0;
n.cYl = v.dir;
t.Fp(d, n.cYi, n.cYj, n.cYk, n.cYl);
var m = f.hn(I.default);
m.bha(1, e.gN);
m.bgV = !1;
var y = f.hn(r.default).bed(), g = O.default.bEC.bFH(l);
t.El(f, g, h.roleCfgId, l.lv, h);
t.Ew(f, y.bdQ.appearanceId);
t.Ez(f, y.bfI(e.gN));
t.Ex(f, 1);
f.hV(!0);
n.cXL < 0 && t.Et(f);
}
}
});
}
};
t.Gr = function(e, i, n) {
var o = e.fr;
if (e.fI) {
var a = O.default.bEC.bFd(i).map(function(t) {
return {
id: t,
level: 1
};
}), u = {
playerId: "",
baseAttrs: {},
roleCfgId: i,
skinId: 0,
personal: null,
skillDataList: [ {
type: Y.TblEnum.SkillTypeTableEnum.PVE_SKILL,
skillData: a
}, {
type: Y.TblEnum.SkillTypeTableEnum.PVP_SKILL,
skillData: a
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
}, b = new g.$uo.$uq();
b.id = -1;
b.roleData = u;
b.camp = 2;
b.nick = "";
b.score = 0;
b.title = 0;
b.power = 0;
b.botAILv = 0;
b.position = 2;
b.heros = [ u ];
var h = new g.$uo.$us(b), l = e.fF.findIndex(function(t) {
return t.cXO === b.position;
});
l >= 0 ? e.fF[l] = h : e.fF.push(h);
var f = e.fr.bln(d.default), c = D.$jB.bma((f.bkn + f.bko) / 2), p = D.$jB.bma((f.bkl + f.bkm) / 2), v = {};
v[1] = [ {
x: D.$jB.bma(p - 400),
y: c,
dir: 1
} ];
v[2] = [ {
x: D.$jB.bma(p + 400),
y: c,
dir: -1
} ];
var m = v[2];
if (m) {
var y = $fc.cfgMgr.tbl.MatchPvpAttrTable.findByKey(i);
if (y) {
var j = o.ble();
h.cXZ = j.hg;
h.cYa = j.hg;
t.FF(j).bjK = h.cXL;
var P = j.hn(x.default);
P.bjI(h.cXL, null, [], [], null);
P.bjJ(i, e.gM);
t.FL(j);
t.FZ(j, !0);
t.Ev(j, 2, 0, -1);
var E = j.hn(s.default);
t.Fm(E, 0, 0, 0);
t.Fo(E, 0);
t.Fn(E, !0);
var k = j.hn($.default), _ = m[0], w = _.x, F = _.y, B = _.dir;
t.Fp(k, w, F, 0, B);
var M = j.hn(I.default);
M.bha(1, e.gN);
M.bgV = !1;
var R = j.hn(r.default).bed(), T = O.default.bEC.bFH(y);
t.El(j, T, u.roleCfgId, y.lv, u);
t.Ew(j, R.bdQ.appearanceId);
t.Ez(j, R.bfI(e.gN));
t.Ex(j, 1);
j.hV(!0);
n && t.Et(j);
return j;
}
}
}
};
t.Gs = function(t, e) {
if (!t.fN) {
var i = t.gk(e);
if (i) {
i.cXX = 0;
i.cXY = 0;
i.cXW += 1;
var n = t.fr.bll(i.cXZ);
if (n && !n.hv.bez) {
i.cXX = 2;
t.gu(g.$uo.$vk.bb, i.cXL);
}
}
}
};
t.Gt = function(e, i) {
if (!e.fN) {
e.fQ = 0;
if (0 === e.fR) for (var n = 0, r = e.fF; n < r.length; n++) {
var o = r[n], a = e.gi(o.cXZ);
a && (a.hv.bez || (o.cXX = 0));
}
var u = e.gk(i);
if (u) {
u.cXX = 0;
u.cXY = 0;
u.cXW += 1;
e.gu(g.$uo.$vk.bb, u.cXL);
var b = e.fr.bll(u.cXZ);
if (b) {
b.hv.bfF();
if (!b.hv.bez) {
e.fr.bln(c.default);
b.hY(b, b.hv.bev);
b.hZ(b, b.hv.bex);
t.EF(b, String(b.hE.bis));
t.FN(b, 11008, [], b.hg);
}
}
}
}
};
t.Gu = function(e, i, n, r) {
var o;
if (null === (o = e.hE) || void 0 === o ? void 0 : o.bhu[n]) for (var a = 0, u = e.hE.bhs; a < u.length; a++) {
var b = u[a];
b.Dq.event === n && t.FM(e, b, b.Dq.eventLg, r);
}
};
t.Gv = function(e, i, n, r, o, a) {
for (var u = 0, b = e.hE.bhs; u < b.length; u++) {
var h = b[u];
if (h.Dq.event === i) {
h.DF = r;
h.DG = o;
h.DH = a;
t.FM(e, h, h.Dq.eventLg, n);
}
}
};
t.Gw = function() {};
t.Gx = function(t, e, i, n) {
var r, o = t.hE, a = o && o.bhA;
switch (e[0]) {
case "==":
return (a[e[1]] || 0) === e[2];

case ">":
return (a[e[1]] || 0) > e[2];

case "<":
return (a[e[1]] || 0) < e[2];

case ">=":
return (a[e[1]] || 0) >= e[2];

case "<=":
return (a[e[1]] || 0) <= e[2];

case "==v":
return (a[e[1]] || 0) === (a[e[2]] || 0);

case ">v":
return (a[e[1]] || 0) > (a[e[2]] || 0);

case "<v":
return (a[e[1]] || 0) < (a[e[2]] || 0);

case ">=v":
return (a[e[1]] || 0) >= (a[e[2]] || 0);

case "<=v":
return (a[e[1]] || 0) <= (a[e[2]] || 0);

case "mp>=":
return !!t.hv && D.$jB.bma(t.hv.bew / t.hv.bex) >= e[1];

case "hp<=":
return !!t.hv && D.$jB.bma(t.hv.beu / t.hv.bev) <= e[1];

case "hpRange":
if (!t.hv) return !1;
var u = D.$jB.bma(t.hv.beu / t.hv.bev);
return u > e[1] && u <= e[2];

case "lostHp>=":
return !!t.hv && t.hv.bev - t.hv.beu >= e[1];

case "hpValue<=":
return !!t.hv && t.hv.beu <= e[1];

case "isFlipY":
return !1;

case "isPlayer":
return t.hx.bjY;

case "isBoss":
return t.hS;

case "eventSkillBtn":
case "targetBehaviorId":
var b = n;
return !!b && !!b.target && (null === (r = b.target.hE) || void 0 === r ? void 0 : r.bhv) === e[1];

case "stateTime>=":
return !!t.hE && (a[e[1]] || 0) <= t.hE.bhC;

case "vel.z<":
return t.hz.bjb < (a[e[1]] || 0);

case "joy.x*dir>":
return !!t.hx.bjN && t.hx.bjO.x * t.hw.bkw > e[1];

case "joy.x*dir<":
return !!t.hx.bjN && t.hx.bjO.x * t.hw.bkw < e[1];

case "abs(joy.y)>":
return !!t.hx.bjN && Math.abs(t.hx.bjO.y) > e[1];

case "joy.y>":
return !!t.hx.bjN && t.hx.bjO.y > e[1];

case "joy.y<":
return !!t.hx.bjN && t.hx.bjO.y < e[1];

case "a.z<":
return t.hw.z < e[1];

case "a.z>=":
return t.hw.z >= e[1];

case "a.lv<":
return t.hv.bey < e[1];

case "a.lv>=":
return t.hv.bey >= e[1];
}
return !1;
};
t.Gy = function() {
return !1;
};
t.Gz = function(t, e, i, n) {
var r = Math.abs(D.$jB.bma(e.x - t.x, 3)), o = Math.abs(D.$jB.bma(e.y - t.y, 3));
return r < i && o < n;
};
t.GA = function(e, i) {
if (e.fI) {
var n = e.gL(e.fI.cYs, e.fI.cYt, i), r = e.fH.get(n);
if (r) {
e.fJ = r.cYr;
e.gu(g.$uo.$vk.m);
e.fF.forEach(function(t) {
t.cYh = 0;
});
t.Gf(e.gs(), g.$uo.$uQ.d);
} else O.default.bEC.bEG("not found next room:", e.fI.cYs, e.fI.cYt, i);
}
};
t.GB = function(e) {
if (e.fI && e.fG.find(function(t) {
return t.cYr === e.fJ;
})) {
for (var i = !0, n = 0, r = e.fF; n < r.length; n++) {
var o = r[n];
if (!o.cYn && !o.cYh) {
i = !1;
break;
}
}
i && t.GC(e);
}
};
t.GC = function(e) {
if (e.fI && 0 !== e.fJ) {
var i = e.fG.find(function(t) {
return t.cYr === e.fJ;
});
if (i) {
e.fJ = 0;
e.fI.cYA = !0;
var n = -1, r = i.cYs - e.fI.cYs, o = i.cYt - e.fI.cYt;
if (1 === o) n = 1; else if (-1 === o) n = 2; else if (-1 === r) n = 3; else {
if (1 !== r) return;
n = 4;
}
this.Gj(e);
var a = e.fI = i;
e.gQ();
e.gy();
var u = e.fr.bln(d.default);
t.Gh(u, a.cYx);
t.Gk(e);
t.Gm(e);
var b = e.gK(n);
e.fF.forEach(function(i, r) {
var o = e.gi(i.cXZ);
if ((null == o ? void 0 : o.hv) && t.Ga(o.hv)) {
t.Ey(o);
if (n) {
var u = a.cYG.transfers.find(function(t) {
return Number(t.id) === b;
});
if (u) {
i.cYi = u.x;
i.cYj = O.default.bEC.bFs(u.y);
i.cYk = 0;
i.cYl = o.hw.bkw;
switch (n) {
case 1:
i.cYj += 355;
break;

case 2:
i.cYj -= 355;
break;

case 3:
i.cYi -= 205;
break;

case 4:
i.cYi += 205;
}
}
} else {
var h = a.cYG.born || [ 500, 300, 1 ];
if (h) {
i.cYi = h[0] + 350 * r;
i.cYj = O.default.bEC.bFs(h[1]);
i.cYk = 0;
i.cYl = h[2];
}
}
t.Fp(o.hw, i.cYi, i.cYj, i.cYk, i.cYl);
}
});
e.fF.forEach(function(t) {
t.cYh = 0;
});
t.Gf(e.gs(), g.$uo.$uQ.g);
e.gu(g.$uo.$vk.n);
}
}
};
t.GD = function(e, i) {
if (e.fI) {
var n = e.fu.bkN;
if (e.fI.cYz) {
var r = e.fF, o = e.fE.bkN, a = void 0, u = 0, b = !1, h = 0;
r.forEach(function(t) {
e.gi(t.cXZ).hU && !t.cYn && h++;
});
for (var l = function(i) {
var n = i.ho(F.default);
if (!n.bkb) {
n.bkb = !0;
t.EF(i, "2");
}
n.bkf = 0;
r.forEach(function(r) {
var o = e.gi(r.cXZ);
o.hU && !r.cYn && o.hE.biu && t.Gz(o.hw, i.hw, 200, 350) && n.bkf++;
});
if (n.bkf > u) {
u = n.bkf;
a = n;
b = !1;
} else n.bkf === u && (b = !0);
}, f = 0, s = o; f < s.length; f++) l(X = s[f]);
if (!b && a) if (a.bkf === h) for (var p = 0, d = o; p < d.length; p++) if ((N = (X = d[p]).ho(F.default)).bka === a.bka) {
a.bkd = D.$jB.bma(a.bkd + i);
a.bke = 0;
if (a.bkd > .5) {
t.GA(e, N.bka);
return;
}
} else {
N.bke = 0;
N.bkd = 0;
} else for (var v = 0, $ = o; v < $.length; v++) if ((N = (X = $[v]).ho(F.default)).bka === a.bka) {
a.bkd = 0;
a.bke = D.$jB.bma(a.bke + i);
if (a.bkd > 5) {
t.GA(e, N.bka);
return;
}
} else {
N.bke = 0;
N.bkd = 0;
} else for (var m = 0, y = o; m < y.length; m++) {
(N = (X = y[m]).ho(F.default)).bke = 0;
N.bkd = 0;
}
return !1;
}
for (var j = 0, P = 0, E = 0, x = n; E < x.length; E++) {
var O = x[E];
if (O.hD.biJ > 0) if (O.hS) {
if (O.hv.bez) {
P++;
j++;
}
} else O.hv.bdW || j++;
}
for (var k = 0, _ = 0, w = 0, B = e.fF; w < B.length; w++) {
var M = B[w];
(C = e.gi(M.cXZ)) && C.hv.bez && k++;
}
if (0 === e.fR) for (var I = 0, R = e.fF; I < R.length; I++) {
M = R[I];
if ((C = e.gi(M.cXZ)) && !C.hv.bez) {
if (2 !== M.cXX) if (M.cXW < e.fP) {
if (0 === M.cXX || 1 === M.cXX) {
M.cXX = 1;
C.hi.blc.gu(g.$uo.$vk.bb, M.cXL);
}
} else M.cXX = 2;
2 !== M.cXX && _++;
}
} else for (var Y = 0, T = e.fF; Y < T.length; Y++) {
var C;
M = T[Y];
if ((C = e.gi(M.cXZ)) && !C.hv.bez) {
if (2 !== M.cXX) if (M.cXW < e.fP) if (0 === e.fR) {
if (0 === M.cXX || 1 === M.cXX) {
M.cXX = 1;
C.hi.blc.gu(g.$uo.$vk.bb, M.cXL);
}
} else {
M.cXY = D.$jB.bma(M.cXY + i);
if (M.cXY >= e.fR) {
M.cXX = 2;
C.hi.blc.gu(g.$uo.$vk.bb, M.cXL);
} else if (0 === M.cXX || 1 === M.cXX) {
M.cXX = 1;
C.hi.blc.gu(g.$uo.$vk.bb, M.cXL);
}
} else M.cXX = 2;
2 !== M.cXX && _++;
}
}
var G = !1;
if (e.fn) {
if (!e.fn) return !1;
G = e.fk >= e.fn;
}
if (e.fI.cYJ) {
if (!e.fI.cYz && (0 === j || 0 === P && !1 !== e.fM.o)) {
e.fI.cYz = !0;
t.GF(e.fr.blm());
t.Gf(e.fr.bln(c.default), g.$uo.$uQ.e);
e.fI.cYP = e.fk;
e.gu(g.$uo.$vk.l);
e.gu(g.$uo.$vk.p);
return !0;
}
} else if (!e.fI.cYz && 0 === j) {
e.fI.cYz = !0;
for (var S = 0, A = o = e.fE.bkN; S < A.length; S++) {
var X, N;
(N = (X = A[S]).ho(F.default)).bkb = !0;
t.EF(X, "2");
}
e.fI.cYP = e.fk;
t.Gf(e.fr.bln(c.default), g.$uo.$uQ.f);
e.gu(g.$uo.$vk.l);
return !0;
}
if (G && k > 0) {
e.fI.cYz = !1;
t.Gf(e.fr.bln(c.default), g.$uo.$uQ.h);
return !0;
}
if (_ + k === 0) {
t.GE(e);
return !0;
}
return !1;
}
};
t.GE = function(e) {
e.fI.cYz = !1;
t.Gf(e.fr.bln(c.default), g.$uo.$uQ.i);
};
t.GF = function(e) {
for (var i = 0, n = e.hi.blc.gF(); i < n.length; i++) {
var r = n[i];
r.hX(r, r.hv.beu);
t.GJ(r, 400, 500);
}
};
t.GG = function(t) {
var e = 0;
if (t.hE && t.hE.bic.length) {
var i = t.hE.bic.find(function(t) {
return 1 === t.b.type;
});
if (i) {
var n = t.hi.bll(i.f);
(null == n ? void 0 : n.hA) && (e = n.hA.biP.biL[0].world.blH.blG);
}
}
return e;
};
t.GH = function() {
return !0;
};
t.GI = function() {};
t.GJ = function(e, i, n) {
if (e.hv) {
e.hv.beu = 0;
e.hE.bhW = 1;
t.EW(e);
t.EK(e);
var r = {
z: n,
hitDirType: 0,
force: i
};
t.EZ(e, r, -1 * e.hw.bkw);
}
};
t.GK = function(e) {
var i;
t.Fm(e.hz, 0, 0, 0);
if (e.hv) {
e.hv.bfE();
(e.hR || -2 === (null === (i = e.hD) || void 0 === i ? void 0 : i.biK) || 1 === e.hv.bdX) && t.EO(e);
if (e.hE) {
for (var n = 0, r = e.hE.bhs; n < r.length; n++) {
var o = r[n];
o.Dq.removeBuffOnDie && t.FP(e, o, !1);
}
t.Gu(e, null, g.$uo.$vh.e);
}
e.hi.blc.gu(g.$uo.$vk.x, e);
}
e.hy && (e.hy.hitPauseTimeScale = 1);
};
t.GL = function(t) {
if (t.hD.biJ > 0) {
var e = t.hi.blc.fI, i = {
b: null == e ? void 0 : e.cYr,
c: t.hD.biK,
d: t.hw.x,
e: t.hw.y,
f: t.hw.z
};
t.hi.blc.gu(g.$uo.$vk.q, i, t);
}
};
t.GM = function(t, e, i, n) {
var r = t.hE.bhA;
switch (e[0]) {
case "=Random":
r[e[1]] = Math.floor(1e4 * t.hi.blc.ge());
break;

case "+=dt":
var o = t.hy, a = D.$jB.bma(t.hi.blc.fp * o.bkv, 4);
r[e[1]] = D.$jB.bma((r[e[1]] || 0) + a, 4);
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
var u = t.hv;
if (u) {
u.bew = Math.max(Math.min(u.bew + E.default.CW(e[1], t, i), u.bex), 0);
t.hi.blc.gu(g.$uo.$vk.A, t);
}
break;

case "aimDisCenterX":
if (t.hG && -1 !== t.hG.bgj) {
var b = t.hi.bll(t.hG.bgj);
t.hw && (null == b ? void 0 : b.hw) && t.hw.x;
}
break;

case "targetVar=":
if (n) {
var h = n.target;
r[e[1]] = h.hE.bhA[e[1]];
}
break;

case "setSkVal":
r[e[1]] = E.default.CW(e[2], t, i);
break;

case "=pos":
r[e[1]] = t.hw.x;
e[2] && (r[e[2]] = t.hw.y);
e[3] && (r[e[3]] = t.hw.z);
break;

case "+=f":
r[e[1]] = D.$jB.bma((r[e[1]] || 0) + E.default.CW(e[2], t, i));
break;

case "v1->v2":
var l = r[e[1]] || 0, f = r[e[2]] || 0, c = e[3] || 0, s = e[4] || 0, p = e[5] || 0, d = Math.floor(D.$jB.bma(l / c));
if (d > 0) {
r[e[1]] = D.$jB.bma(l - D.$jB.bma(d * c));
r[e[2]] = Math.min(D.$jB.bma(f + d * s), p);
}
break;

case "lockHp":
t.hv.lockMinHP = Math.ceil(E.default.CW(e[1], t, i));
break;

case "WBState":
var v = t.hv.finalHurtValue;
r.WBState = O.default.bEC.bGj(v);
break;

case "=castProg":
r[e[1]] = D.$jB.bma(t.hE.bhP.currentTime / t.hE.bhP.time);
}
};
t.GN = function(e) {
var i = e.hi.blc;
if (i) {
var n = i.gk(e.hx.bjK);
if (n) {
var o = n.cYm[0];
if (o) {
var a = e.hE;
if (a.bhs.length > 0) for (var u = 0, b = a.bhs.slice(); u < b.length; u++) {
var h = b[u];
t.FP(e, h, !1);
}
t.EC(e);
if (i.gM) {
e.hn(r.default).bed();
t.El(e, o.baseAttrs, o.roleCfgId, o.level, o);
} else {
var l = $fc.cfgMgr.tbl.MatchPvpAttrTable.findByKey(o.roleCfgId);
if (!l) return;
e.hn(r.default).bed();
var f = O.default.bEC.bFH(l);
t.El(e, f, o.roleCfgId, l.lv, o);
}
t.Ex(e, 1);
for (var c = 0, s = e.hv.bdU; c < s.length; c++) {
var p = s[c];
p.maxCdTime = e.hv.bfB(p);
p.cd = 0;
}
}
}
}
};
t.GO = function(e, i, n, r) {
var o, a, u = e.hi.blc;
if (u) {
var b = u.gk(e.hx.bjK);
if (b) {
var h = b.cYm[0];
if (h && h.troop) {
h.troop.pet = r;
h.wear.equipList = n;
h.skillDataList = i;
t.GN(e);
var l = e.hn(x.default);
l.bjI(b.cXL, null === (o = h.personal) || void 0 === o ? void 0 : o.fashion, h.wear.equipList, null === (a = h.fashion) || void 0 === a ? void 0 : a.morphList, h.troop);
l.bjJ(h.roleCfgId, u.gM);
u.gu(g.$uo.$vk.bs, e);
}
}
}
};
t.GP = function(t, e) {
var i, n, r = t.hi.blc;
if (r) {
var o = r.gk(t.hx.bjK);
if (o) {
var a = o.cYm[0];
if (a && a.troop) {
a.troop.skillButton = e;
var u = t.hn(x.default);
u.bjI(o.cXL, null === (i = a.personal) || void 0 === i ? void 0 : i.fashion, a.wear.equipList, null === (n = a.fashion) || void 0 === n ? void 0 : n.morphList, a.troop);
u.bjJ(a.roleCfgId, r.gM);
r.gu(g.$uo.$vk.bt, t);
}
}
}
};
t.GQ = function(e, i) {
var n = i[0];
i[1];
switch (n) {
case g.$uo.$vj.b:
t.Go(e);
break;

case g.$uo.$vj.c:
e.gu(g.$uo.$vk.bx, g.$uo.$vj.c);
}
};
t.Ea = [];
t.Eb = new _.default();
t.Ec = new j.default();
t.Ed = new j.default();
t.Ee = [];
t.Eh = [];
return t;
}();
i.default = C;
cc._RF.pop();
}, {
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../main/src/$KV": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$KI": "$KI",
"./$KK": "$KK",
"./BattleBehaviorLogic": "BattleBehaviorLogic",
"./component/$DOO": "$DOO",
"./component/$DOP": "$DOP",
"./component/$DOQ": "$DOQ",
"./component/$DOR": "$DOR",
"./component/$DOS": "$DOS",
"./component/$DOT": "$DOT",
"./component/$DOU": "$DOU",
"./component/$DOV": "$DOV",
"./component/$DOW": "$DOW",
"./component/$DOX": "$DOX",
"./component/$DOY": "$DOY",
"./component/$DOZ": "$DOZ",
"./component/$DPA": "$DPA",
"./component/$DPB": "$DPB",
"./component/$DPF": "$DPF",
"./component/$DPH": "$DPH",
"./component/$DPI": "$DPI",
"./component/$DPJ": "$DPJ",
"./component/$DPK": "$DPK",
"./component/$DPL": "$DPL",
"./component/$DPN": "$DPN",
"./component/$DPO": "$DPO",
"./math/$DPR": "$DPR",
"./math/$DPS": "$DPS",
"./math/$DPT": "$DPT",
"./system/$DPY": "$DPY",
"./utils/$DQI": "$DQI"
} ],
$KM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "bf300swXHBGALb+VrIhFtl3", "$KM");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("./$KL"), r = t("./math/$DPR"), o = t("./lib/$DPP"), a = t("./component/$DOO"), u = t("./component/$DPN"), b = t("./$KP"), h = t("./system/$DPU"), l = t("./system/$DPX"), f = t("./system/$DQC"), c = t("./system/$DQD"), s = t("./system/$DQE"), p = t("./system/$DQG"), d = t("./system/$DQA"), v = t("./system/$DPW"), $ = t("./component/$DOX"), m = t("./system/$DPZ"), y = t("./system/$DPV"), g = t("./system/$DQB"), D = t("../../game/src/battle/types/$DYQ"), j = t("../../game/src/battle/types/$DYS"), P = t("./$KO"), E = t("./component/$DPL"), x = t("../../game/src/battle/types/$DYO"), O = t("./system/$DQH"), k = t("./$KJ"), _ = t("./component/$DPO"), w = t("./component/$DOQ"), F = t("./component/$DPF"), B = t("./component/$DPB"), M = t("./component/$DOZ"), I = t("./component/$DOY"), R = t("./component/$DOS"), Y = t("../../game/src/battle/$DSO"), T = t("./component/$DOU"), C = t("./component/$DOT"), G = t("./component/$DPK"), S = t("../../game/src/battle/$DSP"), A = t("./system/$DPY"), X = t("../../game/src/battle/types/$DYL"), N = t("./system/$DQF"), z = t("../../bingo/src/utils/$DOB"), Z = t("../../main/src/tbl/TblEnum"), L = function() {
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
t.prototype.fZ = function(t, e, i) {
Y.default.bEC.bGA();
if (z.ANTI_CHEAT_ENABLED) {
z.$hV.Yd.Yn();
z.$hW.YF.YS();
z.$hZ.Zn.Zs(80);
}
this.fr = new o.$iZ.$jp(b.$gq, o.$iZ.getComponentConstructors(), this);
this.fs = new o.$iZ.$jn();
this.fs.add(new s.$jR(this.fr));
this.fs.add(new g.$jO(this.fr));
this.fs.add(new y.$jF(this.fr));
this.fs.add(new N.$jS(this.fr));
this.fs.add(new h.$jE(this.fr));
this.fs.add(new d.$jN(this.fr));
this.fs.add(new v.$jG(this.fr));
this.fs.add(new f.default(this.fr));
this.fs.add(new l.$jJ(this.fr));
this.fs.add(new A.$jL(this.fr));
this.fs.add(new m.$jM(this.fr));
this.fs.add(new O.$jU(this.fr));
this.fs.add(new p.$jT(this.fr));
this.fs.add(new c.$jQ(this.fr));
this.fs.init();
this.ft = this.fr.blh(o.$iZ.allOf(u.default));
this.fu = this.fr.blh(o.$iZ.allOf(a.default));
this.fv = this.fr.blh(o.$iZ.allOf(w.default, _.default));
this.fw = this.fr.blh(o.$iZ.allOf(_.default, u.default, F.default));
this.fx = this.fr.blh(o.$iZ.allOf(_.default, M.default));
this.fy = this.fr.blh(o.$iZ.allOf(_.default, B.default));
this.fz = this.fr.blh(o.$iZ.allOf(I.default));
this.fA = this.fr.blh(o.$iZ.allOf(R.default));
this.fB = this.fr.blh(o.$iZ.allOf(T.default));
this.fC = this.fr.blh(o.$iZ.allOf(C.default));
this.fE = this.fr.blh(o.$iZ.allOf(G.default));
this.fi = 0;
this.fq = 0;
this.gc(j.$uo.LOGIC_FRAME_RATE);
this.gd(i);
this.fj = t;
this.fk = 0;
this.fF = e.map(function(t) {
return new j.$uo.$us(t);
});
this.gw();
this.gx();
this.fb = !0;
return !0;
};
t.prototype.ga = function(t, e, i) {
this.fe = !0;
this.fZ(t, e, i);
};
t.prototype.gb = function() {
this.fb = !1;
n.default.Ek();
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
this.fr.blp();
this.fr = null;
}
this.fi = 0;
this.fq = 0;
};
t.prototype.gc = function(t) {
this.fh = Math.max(t, 10);
this.fp = r.$jB.bma(1 / this.fh, 6);
};
t.prototype.gd = function(t) {
this.ff = t;
};
t.prototype.ge = function() {
var t = (9301 * this.ff + 49297) % 233280;
return this.ff = t, t / 233280;
};
t.prototype.gf = function(t) {
for (var e = 0, i = 0; i < t.length; i++) e += t[i];
var n = this.ge() * e, r = 0;
for (i = 0; i < t.length; i++) if (n <= (r += t[i])) return i;
return 0;
};
t.prototype.gg = function(t, e) {
return Math.floor(this.ge() * (e - t + 1)) + t;
};
t.prototype.gh = function(t, e) {
var i = this;
this.fd = e;
this.fi++;
this.fq = r.$jB.bma(this.fi * this.fp, 5);
var o = this.gs().bhi;
t && t.forEach(function(t) {
var e, o = i.gl(t.position);
if (o) {
var a = i.gi(o.cYa);
if (a) {
var u = a.hx;
if (t.events && t.events.length > 0) for (var b = 0, h = t.events; b < h.length; b++) h[b].relive && n.default.Gt(i, o.cXL);
for (var l = 0, f = t.frames; l < f.length; l++) {
var c = f[l], s = c & Math.pow(2, 9) - 1;
if (s > x.$ug.A) {
var p = k.battleCmdClassCtorMap.get(s);
if (p) {
var d = new p();
d.eZ(c);
d.fa(i, o);
}
} else {
if (s) {
u.bjM = 1;
u.bjN = 1;
var v = s - 1;
(null === (e = a.hv) || void 0 === e ? void 0 : e.beA) > 0 && (v += 180);
var $ = r.$jB.bma(v * Math.PI / 180, 6);
u.bjP = $;
u.bjQ = v;
u.bjO.x = r.$jB.bma(Math.cos($));
u.bjO.y = r.$jB.bma(Math.sin($));
} else {
u.bjM = 0;
u.bjN = 0;
}
for (var m = c >> 9, y = 1; y <= x.MAX_FRAME_SYNC_CMDBIT; y++) {
var g = y, D = m >> y - 1 & 1, j = x.FRAME_SYNC_BIT2CMD[g], P = n.default.FH(u, j);
if (P) {
var E = Number(D);
P.b = E;
P.e = 0;
if (E) {
P.f = i.fq;
P.c = 1;
} else P.d = 1;
}
}
}
}
}
}
});
var a = o.canCtrl;
this.fF.forEach(function(t) {
var e = i.gi(t.cYa);
e && (e.hx.bjW = a);
});
this.fs.execute(this.fp);
this.fF.forEach(function(t) {
var e = i.gi(t.cYa);
e && (e.hx.bjT.length = 0);
});
this.fi % 60 == 0 && this.gH();
};
t.prototype.gi = function(t) {
return this.fr ? this.fr.bll(t) : null;
};
t.prototype.gj = function() {
var t, e, i;
return null !== (i = null === (e = null === (t = this.fL) || void 0 === t ? void 0 : t.partyHpMultiplier) || void 0 === e ? void 0 : e[this.fF.length - 1]) && void 0 !== i ? i : 1;
};
t.prototype.gk = function(t) {
return this.fF.find(function(e) {
return e.cXL === t;
});
};
t.prototype.gl = function(t) {
return this.fF.find(function(e) {
return e.cXO === t;
});
};
t.prototype.gm = function() {
return this.fr ? this.fr.bln(u.default).worldTimeScale : 0;
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
var t = this.fV, e = this.fr.bln(u.default);
if (e.worldTimeScale !== t) {
e.worldTimeScale = t;
for (var i = 0, n = this.ft.bkN; i < n.length; i++) n[i].hy.worldTimeScale = t;
}
}
};
t.prototype.gq = function(t, e) {
this.fo = t.hg;
for (var i = 0, n = this.ft.bkN; i < n.length; i++) {
var r = n[i];
r.hg === t.hg || e && r.hg === e ? r.hy.mainTimeScale = 1 : r.hy.mainTimeScale = 0;
}
};
t.prototype.gr = function() {
this.fo = 0;
for (var t = 0, e = this.ft.bkN; t < e.length; t++) e[t].hy.mainTimeScale = 1;
};
t.prototype.gs = function() {
var t;
return null === (t = this.fr) || void 0 === t ? void 0 : t.bln($.default);
};
t.prototype.gt = function(t) {
return this.fr ? this.fr.bln(t) : null;
};
t.prototype.gu = function(t, e, i, n, r, o, a) {
this.fc && (void 0 !== a ? this.fc(t, e, i, n, r, o, a) : void 0 !== o ? this.fc(t, e, i, n, r, o) : void 0 !== r ? this.fc(t, e, i, n, r) : void 0 !== n ? this.fc(t, e, i, n) : void 0 !== i ? this.fc(t, e, i) : void 0 !== e ? this.fc(t, e) : this.fc(t));
};
t.prototype.gv = function() {
return [];
};
t.prototype.gw = function() {
var t, e, i, n, o, a, u, b, h, l, f, c, s, p = this;
this.fN = !1;
var d = this.fj.instanceId;
this.fK = Y.default.bEC.bEN(d);
if (!this.fK) {
console.error("no instanceCfg");
return !1;
}
this.fL = Y.default.bEC.bEO(this.fK.type);
if (!this.fL) {
console.error("no instanceTypeCfg");
return !1;
}
this.fM = D.BattleModeConstant[this.fK.type];
if (!this.fM) {
console.error("no modeConstant");
return !1;
}
this.fS = null !== (t = this.fM.e) && void 0 !== t && t;
this.fj.traning && this.fj.traningParam && (this.fS = null !== (i = null === (e = this.fj.traningParam) || void 0 === e ? void 0 : e.d) && void 0 !== i && i);
if (Y.GameInstanceTypeTableEnum.RAID === this.fK.type) {
var v = Y.default.bEC.bGp(d);
v && (this.fP = v.reviveMaxCount);
} else {
this.fP = null !== (n = this.fL.reliveCount) && void 0 !== n ? n : 2;
this.fP > 0 && S.$lN.bGN(null === (a = null === (o = this.fj) || void 0 === o ? void 0 : o.svrInstanceParams) || void 0 === a ? void 0 : a.playMode) && (this.fP = 99999);
}
var $ = Y.default.bEC.bER(d), m = !1;
switch (this.fM.f) {
case D.$uj.b:
this.fm = 1;
this.fj.guide && (this.fm = 100);
this.fn = null !== (u = this.fM.k) && void 0 !== u ? u : 0;
this.fR = 0;
var y = this.gj();
this.fG = [];
if (!(I = $[0])) return !1;
if (this.fj.traning) {
this.fm = 4;
var g = new j.$uo.$ut();
g.cYr = 1;
g.cYu = [];
g.cYs = 0;
g.cYt = 0;
g.cYz = !1;
g.cYA = !1;
g.cYJ = !0;
g.cYy = !0;
g.cYx = this.fj.sceneId;
var P = g.cYG = Y.default.bEC.bES(g.cYx);
if (!P) {
var E = $fc.cfgMgr.Json.get("scenes");
for (var x in E) {
g.cYx = x;
g.cYG = E[x];
break;
}
}
this.fG.push(g);
this.fH.set(this.gI(g.cYs, g.cYt), g);
(null === (b = this.fj.traningParam) || void 0 === b ? void 0 : b.e) && this.gM && this.fj.traningParam.e.forEach(function(t, e) {
var i = t.heros[0], n = new j.$uo.$up();
n.cXs = i.roleCfgId;
n.cXr = Y.GameEnemyTypeTableEnum.Normal;
n.cXz = n.cXz || 60;
n.cXC = i.baseAttrs;
n.cXB = 2;
n.cXD = 0;
n.cXE = -1;
n.cXF = e;
var r = P.born || [ 500, 300, 1 ];
n.cXu = r[0] + 300 * (e + 1);
n.cXv = Y.default.bEC.bFs(r[1]);
n.cXw = 0;
n.cXy = j.$uo.$uU.b;
g.cYC.push(n);
});
} else for (var O = function(t) {
var e, i = new j.$uo.$ut();
i.cYw = t;
i.cYz = !1;
i.cYA = !1;
i.cYr = t.room;
i.cYu = t.transferCode.slice(0);
i.cYv = ((e = {})[1] = i.cYu[0] ? 1 : 0, e[2] = i.cYu[1] ? 1 : 0, e[3] = i.cYu[2] ? 1 : 0, 
e[4] = i.cYu[3] ? 1 : 0, e);
i.cYs = t.mapX;
i.cYt = t.mapY;
i.cYJ = t.roomType === Y.GameRoomTypeTableEnum.BOSS || 1 === $.length;
i.cYy = t.roomType === Y.GameRoomTypeTableEnum.BEGIN || 1 === $.length;
if (!i.cYw) {
Y.default.bEC.bEG("不存在的房间:" + i.cYr);
m = !0;
return "break";
}
i.cYx = i.cYw.sceneName;
var n = i.cYG = Y.default.bEC.bES(i.cYx);
if (!n) {
Y.default.bEC.bEG("不存在的地图场景:" + i.cYr + "/" + i.cYx);
m = !0;
return "break";
}
var o = null !== (h = i.cYw.enemyId) && void 0 !== h ? h : [], a = null !== (l = i.cYw.enemyValue) && void 0 !== l ? l : [], u = null !== (f = i.cYw.enemyPos) && void 0 !== f ? f : [], b = n.role, d = S.$lN.bGO(null === (s = null === (c = k.fj) || void 0 === c ? void 0 : c.svrInstanceParams) || void 0 === s ? void 0 : s.playMode), v = n.walkableArea[0], g = n.walkableArea[1], D = Y.default.bEC.bFs(n.walkableArea[2]), P = Y.default.bEC.bFs(n.walkableArea[3]), E = g - v, x = P - D, O = Math.ceil((v + g) / 2), _ = Math.ceil((D + P) / 2), w = $fc.cfgMgr.tbl.EnemyValueTable;
o.forEach(function(t, e) {
var n = w.findByKey(a[e]);
if (n) {
if (n.type === Y.GameEnemyTypeTableEnum.Hell) {
if (!d) return;
i.cYD = !0;
}
var o = Y.default.bEC.bEW(t);
if (o) {
var h = new j.$uo.$up();
h.cXs = t;
h.cXr = n.type;
h.cXz = n.level;
h.cXC = Y.default.bEC.bFH(n, y);
h.cXB = 2;
h.cXD = o.AI;
h.cXE = -1;
h.cXF = e;
var l = u[e];
h.cXw = -1;
if (l) {
var f = b.find(function(t) {
return t.id === l;
});
if (f) {
h.cXu = f.x;
h.cXv = Y.default.bEC.bFs(f.y);
h.cXw = 0;
h.cXy = f.dir;
} else Y.default.bEC.bEG("not find enemy pos:" + t + " " + l);
}
if (-1 === h.cXw) {
h.cXu = O + Math.floor(r.$jB.bma(r.$jB.bma(.62 * r.$jB.bma(p.ge() - .5)) * E));
h.cXv = _ + Math.floor(r.$jB.bma(r.$jB.bma(.62 * r.$jB.bma(p.ge() - .5)) * x));
h.cXw = 0;
h.cXy = j.$uo.$uU.c;
if (n.type === Y.GameEnemyTypeTableEnum.Hell) {
h.cXu = O;
h.cXv = _;
h.cXw = 0;
h.cXy = j.$uo.$uU.b;
}
}
i.cYC.push(h);
}
} else {
Y.default.bEC.bEG("enemy value not find:" + a[e]);
n = w.VOList[0];
}
});
k.fG.push(i);
k.fH.set(k.gI(i.cYs, i.cYt), i);
}, k = this, _ = 0, w = $; _ < w.length && "break" !== O(F = w[_]); _++) ;
break;

case D.$uj.c:
this.fm = 2;
this.fn = 0;
this.fR = 0;
this.fG = [];
var F = $[0];
(R = new j.$uo.$ut()).cYw = F;
R.cYz = !1;
R.cYA = !1;
R.cYr = F.room;
R.cYu = [];
R.cYv = {};
R.cYs = 0;
R.cYt = 0;
R.cYJ = !1;
R.cYy = !1;
var B = $fc.cfgMgr.tbl.RoomPvpInfoTable.VOList, M = B[Math.floor(this.ge() * B.length)];
this.fn = M.time;
R.cYx = M.sceneName;
R.cYG = Y.default.bEC.bES(R.cYx);
this.fG.push(R);
this.fH.set(this.gI(R.cYs, R.cYt), R);
break;

case D.$uj.f:
this.fm = 7;
this.fn = 0;
this.fR = 0;
if (!(I = $[0])) return !1;
this.fG = [];
(R = new j.$uo.$ut()).cYr = 1;
R.cYu = [];
R.cYs = 0;
R.cYt = 0;
R.cYz = !1;
R.cYA = !1;
R.cYJ = !0;
R.cYy = !0;
R.cYx = I.sceneName;
if (!(R.cYG = Y.default.bEC.bES(R.cYx))) {
E = $fc.cfgMgr.Json.get("scenes");
for (var x in E) {
R.cYx = x;
R.cYG = E[x];
break;
}
}
this.fG.push(R);
this.fH.set(this.gI(R.cYs, R.cYt), R);
break;

case D.$uj.g:
this.fm = 8;
this.fn = 0;
this.fR = 0;
var I, R;
if (!(I = $[0])) return !1;
this.fG = [];
(R = new j.$uo.$ut()).cYr = 1;
R.cYu = [];
R.cYs = 0;
R.cYt = 0;
R.cYz = !1;
R.cYA = !1;
R.cYJ = !0;
R.cYy = !0;
R.cYx = I.sceneName;
if (!(R.cYG = Y.default.bEC.bES(R.cYx))) {
E = $fc.cfgMgr.Json.get("scenes");
for (var x in E) {
R.cYx = x;
R.cYG = E[x];
break;
}
}
this.fG.push(R);
this.fH.set(this.gI(R.cYs, R.cYt), R);
}
return m;
};
t.prototype.gx = function() {
switch (this.fM.f) {
case D.$uj.b:
this.fI = this.fG.find(function(t) {
return t.cYy;
});
this.fJ = 0;
if (!this.fI) return;
this.gQ();
this.fr.blm().hV(!0);
this.gy();
(e = this.fr.bln($.default)).bhn(0);
e.bhf = this.fm;
n.default.Gf(e, j.$uo.$uQ.b);
var t = this.fr.bln(E.default);
n.default.Gh(t, this.fI.cYx);
n.default.Gk(this);
n.default.Gp(this, 0);
n.default.Gm(this);
break;

case D.$uj.c:
this.fI = this.fG[0];
this.fJ = 0;
if (!this.fI) return;
this.fr.blm().hV(!0);
this.gy();
(e = this.fr.bln($.default)).bhn(0);
e.bhf = this.fm;
n.default.Gf(e, j.$uo.$uQ.b);
t = this.fr.bln(E.default);
n.default.Gh(t, this.fI.cYx);
n.default.Gq(this);
break;

case D.$uj.f:
this.fI = this.fG.find(function(t) {
return t.cYy;
});
this.fJ = 0;
if (!this.fI) return;
this.fr.blm().hV(!0);
this.gy();
(e = this.fr.bln($.default)).bhn(0);
e.bhf = this.fm;
n.default.Gf(e, j.$uo.$uQ.b);
t = this.fr.bln(E.default);
n.default.Gh(t, this.fI.cYx);
n.default.Gp(this, 0);
break;

case D.$uj.g:
this.fI = this.fG[0];
this.fJ = 0;
if (!this.fI) return;
this.fr.blm().hV(!0);
this.gy();
var e;
(e = this.fr.bln($.default)).bhn(0);
e.bhf = this.fm;
n.default.Gf(e, j.$uo.$uQ.b);
t = this.fr.bln(E.default);
n.default.Gh(t, this.fI.cYx);
n.default.Gq(this);
}
};
t.prototype.gy = function() {
this.fT = {
type: j.$uo.$uH.c,
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
this.gu(j.$uo.$vk.br);
};
t.prototype.gA = function(t, e) {
for (var i, n = t.slice(), r = t.length, o = 0; o < e; o++) {
var a = Math.floor(this.ge() * (r - o)) + o;
i = [ n[a], n[o] ], n[o] = i[0], n[a] = i[1];
}
n.length = e;
return n;
};
t.prototype.gB = function(t) {
n.default.EE(t);
t.hP();
};
t.prototype.gC = function() {
return !!this.fj.guide;
};
t.prototype.gD = function() {
var t, e, i;
return null !== (i = null === (e = null === (t = this.gs()) || void 0 === t ? void 0 : t.bhi) || void 0 === e ? void 0 : e.guideId) && void 0 !== i ? i : 0;
};
t.prototype.gE = function() {
var t;
return null === (t = this.gs()) || void 0 === t ? void 0 : t.bhi;
};
t.prototype.gF = function() {
for (var t, e = [], i = 0, n = this.fu.bkN; i < n.length; i++) {
var r = n[i];
r.hv.bez && 1 != (null === (t = r.hD) || void 0 === t ? void 0 : t.biI) && e.push(r);
}
return e;
};
t.prototype.gG = function() {
this.go(0);
this.fr.bln($.default).bhn(3, [ this.fF[0].cXL ]);
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
t.prototype.gL = function(t, e, i) {
switch (i) {
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
return t.cYD && !t.cYF;
}) || this.fG.find(function(t) {
return t.cYJ;
});
};
t.prototype.gP = function() {
for (var t = 0, e = this.fG; t < e.length; t++) e[t].cYM = -1;
this.gQ();
};
t.prototype.gQ = function() {
var t = this.fI;
if (t && -1 === t.cYM) {
t.cYM = 0;
if (this.fG.length > 1) {
for (var e = this.gO(), i = void 0, n = void 0, r = [], o = 0, a = this.fG; o < a.length; o++) {
var u = a[o], b = {
b: u.cYs,
c: u.cYt,
d: 0,
e: 0,
i: 0,
j: u.cYv
};
r[b.b] || (r[b.b] = []);
r[b.b][b.c] = b;
u === t && (i = b);
u === e && (n = b);
}
if (i && n) {
var h = X.aStar(i, n, r);
if (h.length > 0 && h[1]) {
var l = h[1].b - h[0].b, f = h[1].c - h[0].c;
t.cYM = l ? 1 === l ? 4 : 3 : 1 === f ? 1 : 2;
}
}
}
}
};
Object.defineProperty(t.prototype, "gR", {
get: function() {
var t, e;
return null !== (e = null === (t = this.fI) || void 0 === t ? void 0 : t.cYr) && void 0 !== e ? e : 0;
},
enumerable: !1,
configurable: !0
});
t.prototype.gS = function() {
var t, e = null === (t = this.fu) || void 0 === t ? void 0 : t.bkN;
if (!e) return 0;
for (var i = 0, n = 0, r = e; n < r.length; n++) {
var o = r[n];
if (o.hS) {
i += o.hv.finalHurtValue;
break;
}
}
return Math.floor(i);
};
t.prototype.gT = function() {
for (var t = [], e = 0, i = this.fF; e < i.length; e++) {
var n = i[e], r = this.gi(n.cXZ);
if (r) {
var o = r.hv.beu, a = r.hv.bev, u = r.hv.bew, b = r.hv.bfc, h = r.hv.bfd, l = r.hw.x, f = r.hw.y, c = r.hv.finalDamage, s = r.hv.finalHurtValue;
t.push({
id: n.cXL,
hp: o,
hpMax: a,
mp: u,
ap: b,
sp: h,
x: l,
y: f,
totalDamage: c,
totalHurt: s
});
}
}
var p = this.fI, d = 0;
p && (d = p.cYz ? 1 : 0);
return {
frameId: this.fi,
result: d,
randomSeed: this.ff,
battleTime: this.fk,
players: t
};
};
t.prototype.gU = function() {
return P.computeCheckNum(this);
};
t.prototype.gV = function(t, e, i, r) {
n.default.GO(t, e, i, r);
};
t.prototype.gW = function(t, e) {
n.default.GP(t, e);
};
t.prototype.gX = function(t, e) {
n.default.GN(t);
this.gY(t, e);
};
t.prototype.gY = function(t, e) {
if (e.bEp[e.playerCoolingTime.bEh]) {
var i = e.bEp[e.playerCoolingTime.bEh][0];
n.default.FQ(t, 11013);
n.default.FN(t, 11013, [ 1e4 * i / 100 ], t.hg, null);
} else n.default.FQ(t, 11013);
if (e.bEr[e.playerCriticalStrikeRateIncrease.bEh]) {
i = e.bEr[e.playerCriticalStrikeRateIncrease.bEh][0];
n.default.FQ(t, 11014);
n.default.FN(t, 11014, [ 1e4 * i / 100 ], t.hg, null);
} else n.default.FQ(t, 11014);
if (e.bEs[e.playerSpeedIncrease.bEh]) {
i = e.bEs[e.playerSpeedIncrease.bEh][0];
n.default.FQ(t, 11015);
n.default.FN(t, 11015, [ 1e4 * i / 100 ], t.hg, null);
} else n.default.FQ(t, 11015);
};
t.prototype.gZ = function() {
var t = this.fr.bln($.default);
t.bhn(0);
n.default.Gf(t, 102);
};
t.prototype.ha = function() {
var t = this.fr.bln($.default);
t.bhn(0);
n.default.Gf(t, 101);
};
t.prototype.hb = function() {
var t = this.fr.bln($.default);
t.bhn(0);
n.default.Gf(t, 2);
};
t.prototype.hc = function(t) {
n.default.GN(t);
};
t.prototype.hd = function(t, e) {
if (Y.default.bEC.bEW(t)) {
var i = 1 === e.enemyFight.bEh, r = n.default.Gr(this, t, i);
r.hv.beD = 1;
r.hv.bdX = 1;
if (r.hN) {
0 === e.airProcect.bEh && (r.hN.bgX = -1);
0 === e.lieProcect.bEh && (r.hN.bgY = -1);
0 === e.standProcect.bEh && (r.hN.bgW = -1);
}
}
};
t.prototype.he = function() {
var t = this.fF.findIndex(function(t) {
return 2 === t.cXO;
});
t >= 0 && this.fF.splice(t, 1);
};
Object.defineProperty(t.prototype, "hf", {
get: function() {
var t, e, i;
return null !== (i = null === (e = null === (t = this.fj) || void 0 === t ? void 0 : t.svrInstanceParams) || void 0 === e ? void 0 : e.playMode) && void 0 !== i ? i : Z.TblEnum.InstancePlayModeTableEnum.NORMAL;
},
enumerable: !1,
configurable: !0
});
return t;
}();
i.default = L;
window.$fN = window.$fN || {};
$fN.$go = L;
cc._RF.pop();
}, {
"../../bingo/src/utils/$DOB": void 0,
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/$DSP": void 0,
"../../game/src/battle/types/$DYL": void 0,
"../../game/src/battle/types/$DYO": void 0,
"../../game/src/battle/types/$DYQ": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$KJ": "$KJ",
"./$KL": "$KL",
"./$KO": "$KO",
"./$KP": "$KP",
"./component/$DOO": "$DOO",
"./component/$DOQ": "$DOQ",
"./component/$DOS": "$DOS",
"./component/$DOT": "$DOT",
"./component/$DOU": "$DOU",
"./component/$DOX": "$DOX",
"./component/$DOY": "$DOY",
"./component/$DOZ": "$DOZ",
"./component/$DPB": "$DPB",
"./component/$DPF": "$DPF",
"./component/$DPK": "$DPK",
"./component/$DPL": "$DPL",
"./component/$DPN": "$DPN",
"./component/$DPO": "$DPO",
"./lib/$DPP": "$DPP",
"./math/$DPR": "$DPR",
"./system/$DPU": "$DPU",
"./system/$DPV": "$DPV",
"./system/$DPW": "$DPW",
"./system/$DPX": "$DPX",
"./system/$DPY": "$DPY",
"./system/$DPZ": "$DPZ",
"./system/$DQA": "$DQA",
"./system/$DQB": "$DQB",
"./system/$DQC": "$DQC",
"./system/$DQD": "$DQD",
"./system/$DQE": "$DQE",
"./system/$DQF": "$DQF",
"./system/$DQG": "$DQG",
"./system/$DQH": "$DQH"
} ],
$KN: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "200dcjMwZVCE7edEZDT63kp", "$KN");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.default = function() {
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
$KO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b242ezcukFN9IGNGkg5jO6l", "$KO");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.fnv1a32 = i.computeCheckNum = void 0;
i.computeCheckNum = function(t) {
for (var e = 0, i = 0, n = t.fF; i < n.length; i++) {
var r = n[i], o = t.gi(r.cXZ);
o && (e += o.hv.finalDamage);
}
return e;
};
i.fnv1a32 = function(t) {
for (var e = 2166136261, i = 0; i < t.length; i++) e = (e ^= 255 & t[i]) + (e << 1) + (e << 4) + (e << 7) + (e << 8) + (e << 24) >>> 0;
return e >>> 0;
};
cc._RF.pop();
}, {} ],
$KP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "3935aJcVctE9bLr5hHbkyAG", "$KP");
var n, r = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
});
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$gq = void 0;
var o = t("./lib/$DPP"), a = t("./component/$DOO"), u = t("./component/$DOQ"), b = t("./component/$DOR"), h = t("./component/$DOS"), l = t("./component/$DOU"), f = t("./component/$DOY"), c = t("./component/$DPA"), s = t("./component/$DPB"), p = t("./component/$DPD"), d = t("./component/$DOX"), v = t("./component/$DPF"), $ = t("./component/$DPJ"), m = t("./component/$DPN"), y = t("./component/$DPO"), g = t("./component/$DOP"), D = t("./component/$DOZ"), j = t("./component/$DPC"), P = t("./component/$DPI"), E = t("../../game/src/battle/$DSO"), x = t("./component/$DPG"), O = t("../../game/src/battle/types/$DYS"), k = t("./component/$DOV"), _ = t("./$KL"), w = t("./system/$DPY"), F = t("./component/$DOW"), B = t("./math/$DPR"), M = t("./component/$DPH"), I = null;
function R(t, e, i) {
t[e.tid] = i;
}
function Y() {
var t = [];
R(t, a.default, function(t, e) {
return t.hv = e;
});
R(t, y.default, function(t, e) {
return t.hw = e;
});
R(t, $.default, function(t, e) {
return t.hx = e;
});
R(t, m.default, function(t, e) {
return t.hy = e;
});
R(t, v.default, function(t, e) {
return t.hz = e;
});
R(t, s.default, function(t, e) {
return t.hA = e;
});
R(t, h.default, function(t, e) {
return t.hB = e;
});
R(t, l.default, function(t, e) {
return t.hC = e;
});
R(t, c.default, function(t, e) {
return t.hD = e;
});
R(t, f.default, function(t, e) {
return t.hE = e;
});
R(t, u.default, function(t, e) {
return t.hF = e;
});
R(t, b.default, function(t, e) {
return t.hG = e;
});
R(t, d.default, function(t, e) {
return t.hH = e;
});
R(t, g.default, function(t, e) {
return t.hI = e;
});
R(t, D.default, function(t, e) {
return t.hJ = e;
});
R(t, j.default, function(t, e) {
return t.hK = e;
});
R(t, P.default, function(t, e) {
return t.hL = e;
});
R(t, k.default, function(t, e) {
return t.hM = e;
});
R(t, F.default, function(t, e) {
return t.hN = e;
});
R(t, M.default, function(t, e) {
return t.hO = e;
});
return t;
}
var T = function(t) {
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
var i = (I || (I = Y()))[t];
void 0 !== i && i(this, e);
};
e.prototype.hl = function() {
for (var t = I || (I = Y()), e = 0; e < t.length; e++) {
var i = t[e];
void 0 !== i && i(this, this.hj[e]);
}
};
e.prototype.hP = function() {
if (this.hM) {
w.$jL.bni(this);
this.hq(k.default);
}
this.hn(p.$iN);
};
e.prototype.hQ = function() {
return this.hp(p.$iN);
};
Object.defineProperty(e.prototype, "hR", {
get: function() {
var t;
return (null === (t = this.hD) || void 0 === t ? void 0 : t.biJ) > 0 && this.hD.biJ !== E.GameEnemyTypeTableEnum.Boss;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hS", {
get: function() {
var t;
return (null === (t = this.hD) || void 0 === t ? void 0 : t.biJ) === E.GameEnemyTypeTableEnum.Boss;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hT", {
get: function() {
var t;
return !(null === (t = this.hD) || void 0 === t || !t.biJ || this.hD.biJ !== E.GameEnemyTypeTableEnum.Boss && this.hD.biJ !== E.GameEnemyTypeTableEnum.Elite);
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "hU", {
get: function() {
var t;
return null === (t = this.hv) || void 0 === t ? void 0 : t.bez;
},
enumerable: !1,
configurable: !0
});
e.prototype.hV = function(t) {
t ? this.hn(x.default) : this.hq(x.default);
};
Object.defineProperty(e.prototype, "hW", {
get: function() {
return !!this.ho(x.default);
},
enumerable: !1,
configurable: !0
});
e.prototype.hX = function(t, e, i) {
var n, r, o;
if (this.hv && !(e <= 0)) {
for (var a = this.hv, u = e; -1 !== a.bej; ) {
var b = this.hE.bhs[a.bej];
if ((null === (n = null == b ? void 0 : b.Dq) || void 0 === n ? void 0 : n.specialType) === O.$uo.$vc.b) {
if (!((u -= b.DF) > 0)) {
u = 0;
b.DF -= e;
break;
}
b.DF = 0;
a.bej = -1;
_.default.FP(this, b, !1);
this.ia();
} else a.bej = -1;
}
var h = a.beu;
a.beu = Math.max(a.beu - u, a.lockMinHP);
a.finalHurtValue += e;
if (a.beD && a.beu <= 0) {
a.beu = a.bev;
if (this.hN) {
this.hN.bgO = 1;
this.hN.bhb();
this.hN.bhd();
this.hN.bhc();
}
}
if (!a.bez) {
this.hS && this.hi.blc.fI && this.hi.blc.fI.cYN.push(this.hg);
_.default.GL(this);
}
if (null == t ? void 0 : t.hv) {
t.hv.finalDamage += e;
(null === (r = null == i ? void 0 : i.hitInfo) || void 0 === r ? void 0 : r.skId) && t.hi.blc.gu(O.$uo.$vk.v, t, i.hitInfo.skId, e);
}
if (null === (o = this.hE) || void 0 === o ? void 0 : o.bhu[O.$uo.$vh.f]) {
var l = h - a.beu;
_.default.Gv(this, O.$uo.$vh.f, null, l);
}
this.hi.blc.gu(O.$uo.$vk.y, this);
}
};
e.prototype.hY = function(t, e) {
if (this.hv && !(e <= 0)) {
var i = this.hv;
i.beu = Math.min(i.beu + e, i.bev);
this.hi.blc.gu(O.$uo.$vk.y, this);
}
};
e.prototype.hZ = function(t, e) {
var i = this.hv;
i && (i.bew = Math.min(i.bew + e, i.bex));
};
e.prototype.ia = function() {
var t, e, i = this.hv;
if (i) {
if (-1 !== i.bej) {
var n = this.hE.bhs[i.bej];
(null === (t = null == n ? void 0 : n.Dq) || void 0 === t ? void 0 : t.specialType) !== O.$uo.$vc.b && (i.bej = -1);
}
if (-1 === i.bej) for (var r = 0, o = this.hE.bhs; r < o.length; r++) {
var a = o[r];
if ((null === (e = a.Dq) || void 0 === e ? void 0 : e.specialType) === O.$uo.$vc.b) {
i.bej = a.Do;
if (-1 !== i.bej) break;
}
}
}
};
e.prototype.ib = function() {
this.hM && w.$jL.bni(this);
this.hi.blg(this);
};
Object.defineProperty(e.prototype, "ic", {
get: function() {
return this.hv ? this.hv.beo <= 0 : this.hz.bji;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "id", {
get: function() {
var t;
return (null === (t = this.hx) || void 0 === t ? void 0 : t.bjY) || !1;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "ie", {
get: function() {
var t;
return null === (t = this.hi.blc) || void 0 === t ? void 0 : t.fI;
},
enumerable: !1,
configurable: !0
});
e.prototype._if = function() {
var t = this.hN;
if (t) {
t.bhd();
t.bhc();
}
};
e.prototype.ig = function() {
var t = this.hN;
if (t) {
if (-1 !== t.bgX && -1 === t.bgP && t.bgO > t.bgX) {
t.bgP = t.bgO;
t.bgQ = this.hi.blc.fk;
}
t.bhb();
}
};
e.prototype.ih = function() {
var t = this.hN;
if (t && -1 !== t.bgW) {
-1 === t.bgT && t.bgO > t.bgW && (t.bgT = t.bgO);
if (-1 !== t.bgT) {
t.bgU = this.hi.blc.fk;
if (B.$jB.bma(Math.max(t.bgT - this.hv.beF(), 0)) > t.bgW) {
t.bhb();
_.default.EM(this);
}
}
}
};
e.prototype.ii = function() {
var t = this.hN;
if (t && -1 !== t.bgY && -1 === (null == t ? void 0 : t.bgR) && t.bgO > t.bgY) {
t.bgR = t.bgO;
t.bgS = this.hi.blc.fk;
}
};
e.prototype.ij = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.bgP) {
var e = B.$jB.bma(Math.max(t.bgP - this.hv.beF(), 0));
return e < t.bgX ? 1 : B.$jB.bma(Math.max(.7 - e, 0));
}
return 1;
};
e.prototype.ik = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.bgR) {
var e = B.$jB.bma(Math.max(t.bgR - this.hv.beF(), 0));
return 0 === e ? 1 : Math.max(B.$jB.bma(1 - B.$jB.bma(e / t.bgY)), t.bgZ);
}
return 1;
};
e.prototype.il = function() {
if (!this.hN) return 1;
if (!this.hv) return 1;
var t = this.hN;
if (-1 !== t.bgR) {
var e = B.$jB.bma(Math.max(t.bgR - this.hv.beF(), 0));
return 0 === e ? 1 : B.$jB.bma(1 - B.$jB.bma(e / t.bgY));
}
return 1;
};
Object.defineProperty(e.prototype, "im", {
get: function() {
var t, e;
return (null === (t = this.hv) || void 0 === t ? void 0 : t.beE) > 0 || (null === (e = this.hE) || void 0 === e ? void 0 : e.bii);
},
enumerable: !1,
configurable: !0
});
return e;
}(o.$iZ.$jc);
i.$gq = T;
cc._RF.pop();
}, {
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYS": void 0,
"./$KL": "$KL",
"./component/$DOO": "$DOO",
"./component/$DOP": "$DOP",
"./component/$DOQ": "$DOQ",
"./component/$DOR": "$DOR",
"./component/$DOS": "$DOS",
"./component/$DOU": "$DOU",
"./component/$DOV": "$DOV",
"./component/$DOW": "$DOW",
"./component/$DOX": "$DOX",
"./component/$DOY": "$DOY",
"./component/$DOZ": "$DOZ",
"./component/$DPA": "$DPA",
"./component/$DPB": "$DPB",
"./component/$DPC": "$DPC",
"./component/$DPD": "$DPD",
"./component/$DPF": "$DPF",
"./component/$DPG": "$DPG",
"./component/$DPH": "$DPH",
"./component/$DPI": "$DPI",
"./component/$DPJ": "$DPJ",
"./component/$DPN": "$DPN",
"./component/$DPO": "$DPO",
"./lib/$DPP": "$DPP",
"./math/$DPR": "$DPR",
"./system/$DPY": "$DPY"
} ],
BattleBehaviorLogic: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "02b39bTqzNBEKeSUGdGgGm1", "BattleBehaviorLogic");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("./$KL"), r = t("./math/$DPR"), o = t("./component/$DOR"), a = t("./component/$DPF"), u = t("./component/$DPL"), b = t("./component/$DOX"), h = t("./component/$DPN"), l = t("../../game/src/battle/types/$DYS"), f = t("./math/$DPS"), c = t("./$KI"), s = t("./$KK"), p = t("../../game/src/battle/$DSO"), d = t("./component/$DPH"), v = t("./component/$DPM"), $ = t("../../main/src/tbl/TblEnum"), m = l.$uo.$vf, y = l.$uo.$uW, g = function() {
function t() {}
t.CT = function(t, e, i, o) {
var a, h, f, s, p, v, y;
switch (i[0]) {
case m.b:
var g = t.hE;
return "number" == typeof (T = i[1])[1] ? g.bhC >= T[0] && g.bhC <= T[1] || g.bhC > T[1] && g.bhB < T[0] : g.bhB < T[0] && g.bhC >= T[0];

case m.c:
return !(!(rt = t.hx) || !rt.bjX) && (1 === (null == (ut = n.default.FH(rt, i[1][0])) ? void 0 : ut.c) || (!(!i[1][2] || !ut.b) || !!i[1][1] && r.$jB.bma(t.hi.blc.fq - ut.f) < i[1][1]));

case m.e:
var D = t.hz;
if (D) {
var j = t.hi.bln(u.default), P = !1, E = i[1] && i[1][0] || 0;
0 !== D.biZ && (P = D.biZ > 0 ? t.hw.x >= j.bkm + E : t.hw.x <= j.bkl - E);
return !!P || 0 !== D.bja && (D.bja > 0 ? t.hw.y >= j.bko + E : t.hw.y <= j.bkn - E);
}
return !1;

case m.f:
return t.hw.z <= 0;

case m.g:
for (var x = 0, O = (g = t.hE).bic; x < O.length; x++) {
var k = O[x];
if (2 === k.b.type) {
var _ = t.hi.bll(k.f);
if (_ && _.hB.bgw > 0) return !0;
}
}
return !1;

case m.h:
g = t.hE;
var w = i[1];
return g.bhI.length >= w.count;

case m.i:
return !(!(rt = t.hx) || !rt.bjX) && (1 === rt.bjN && (!(Math.abs(rt.bjO.x) < .32) && Math.sign(t.hw.bkw * rt.bjO.x) === i[1].dir));

case m.j:
return !(!t.hx || !t.hx.bjX) && 1 === t.hx.bjN;

case m.m:
return !(!t.hx || !t.hx.bjX) && n.default.FH(t.hx, i[1][0]).b;

case m.n:
return !!t.hG && t.hG.bgm < i[1][0];

case m.o:
for (var F = i[1], B = F.id, M = F.count, I = 0, R = 0, Y = t.hE.bhH; R < Y.length; R++) Y[R].id === B && I++;
return I >= M;

case m.p:
if (t.hw) {
j = t.hi.bln(u.default);
return t.hw.y !== j.bkn;
}
return !1;

case m.q:
return !!t.hE && -1 !== t.hE.bhJ;

case m.r:
if (!t.hx || !t.hx.bjX) return !1;
if (1 !== t.hx.bjN) return !1;
if (!t.hw) return !1;
var T = i[1], C = t.hx.bjQ;
return T[0] > T[1] ? C > 180 ? C > T[0] : C <= T[1] : t.hx.bjQ > T[0] && t.hx.bjQ <= T[1];

case m.s:
g = t.hE;
var G = i[1][0];
return !!g && g.bhO[G] === i[1][1];

case m.t:
var S = t;
i[1][2] && (S = t.hi.bll(t.hE.bhF));
return !!(g = S.hE).bho && (void 0 === (nt = i[1][1]) ? !!g.bho[i[1][0]] : g.bho[i[1][0]] === nt);

case m.u:
var A = t.hG;
if (!A) return !1;
var X = t.hi.bll(A.bgj);
if (!X) return !1;
if (!(g = X.hE)) return !1;
switch (i[1][0]) {
case 99:
return g.bho && g.bho.behit;

case 98:
default:
return !1;
}

case m.v:
return !1;

case m.w:
return t.hU;

case m.x:
T = i[1];
return n.default.Gx(t, T, e, o);

case m.y:
var N = null === (a = t.hv) || void 0 === a ? void 0 : a.bdR;
if (0 === N.bnt) return !1;
if (nt = i[1]) return N.bnu(1 << nt - 1);
var z = (1 << l.$uo.$va.j - 1) - 1;
return (N.bnt & z) > 0;

case m.A:
if (!(Z = null == o ? void 0 : o.target)) return !1;
T = i[1];
return n.default.Gx(Z, T, e, o);

case m.z:
if (null == o || !o.target) return !1;
w = i[1];
var Z = o.target;
switch (w[0]) {
case "dis<":
return n.default.Gd(Z.hw, t.hw) < c.default.CW(w[1], t, null);
}

case m.B:
var L = i[1];
return (null === (h = t.hE) || void 0 === h ? void 0 : h.bht[L]) > 0;

case m.C:
if (!(null == (Z = null == o ? void 0 : o.target) ? void 0 : Z.hE)) return !1;
L = i[1];
return (null == Z ? void 0 : Z.hE.bht[L]) > 0;

case m.D:
if (!e) return !1;
T = i[1];
var K = e.Dx;
return "number" == typeof T[1] ? K >= T[0] && K <= T[1] : e.Dw < T[0] && K >= T[0];

case m.E:
return (null === (f = t.hI) || void 0 === f ? void 0 : f.bfJ) > 0 || !!t.ho(d.default);

case m.F:
return -1 !== (w = i[1]).indexOf(t.hv.bdV);

case m.G:
return t.hi.blc.gM;

case m.H:
var Q = i[1], V = t.hD.biI;
return t.hi.blc.fu.bkN.filter(function(e) {
return e.hg !== t.hg && e.hg > 0 && e.hv.bez && V === e.hD.biI && (!Q.roleCfgId || e.hv.bdQ.id === Q.roleCfgId);
}).length >= Q.count;

case m.I:
w = i[1];
var J = t.hv;
if (!J) {
var H = t.hi.bll(t.hE.bhF);
H && (J = H.hv);
}
return !!J && !!(lt = J.beg(w.id)) && lt.De >= w.level;

case m.J:
Z = void 0;
if ((w = i[1]).find && (Z = n.default.Eq(t, w.find, o, e)) && (null == Z ? void 0 : Z.hw)) {
var U = t.hw.x, W = Z.hw.x, q = t.hw.bkw, tt = Z.hw.bkw;
switch (w.dirType) {
case 1:
return tt == l.$uo.$uU.b && U >= W || tt == l.$uo.$uU.c && U <= W;

case 2:
return tt == l.$uo.$uU.b && U <= W || tt == l.$uo.$uU.c && U >= W;

case 3:
return tt == l.$uo.$uU.b && q == l.$uo.$uU.c && U >= W || tt == l.$uo.$uU.c && q == l.$uo.$uU.b && U <= W;

case 4:
return tt == l.$uo.$uU.b && q == l.$uo.$uU.c && U <= W || tt == l.$uo.$uU.c && q == l.$uo.$uU.b && U >= W;
}
}
return !1;

case m.K:
w = i[1], Z = void 0;
var et = o;
if ("boolean" != typeof et.isPhysical) return !1;
if (w.damageType) {
var it = et.isPhysical ? 1 : 2;
if (w.damageType !== it) return !1;
}
return !w.elementValue || 0 != (w.elementValue & et.elementValue);

case m.L:
var nt;
w = i[1];
return (nt = Math.floor(r.$jB.bma(1e4 * t.hi.blc.ge()))) < c.default.CW(w, t, e);

case m.M:
var rt;
if (!(rt = t.hx) || !rt.bjX) return !1;
if (i[1][3]) {
for (var ot = 0, at = rt.bjT; ot < at.length; ot++) if (at[ot].d === i[1][0]) return !0;
return !1;
}
var ut, bt = n.default.FJ(t, i[1][0]);
if (!bt) return !1;
if (1 === (ut = n.default.FH(rt, bt)).c) return !0;
if (i[1][2] && ut.b) return !0;
var ht = null !== (s = i[1][1]) && void 0 !== s ? s : .25;
return !!ht && r.$jB.bma(t.hi.blc.fq - ut.f) < ht;

case m.N:
var lt;
w = i[1];
if (!(lt = o)) return !1;
if (w.levelRange) {
var ft = lt.Dg.upgradeLv;
if (ft < w.levelRange[0] || ft > w.levelRange[1]) return !1;
}
return !0;

case m.O:
return !(w = i[1]).progress || r.$jB.bma(t.hE.bhP.currentTime / t.hE.bhP.time) >= w.progress;

case m.P:
w = i[1];
return t.hE.bib.actId === w[0];

case m.Q:
return "boolean" != typeof (w = i[1]).isAbyss || (null === (v = null === (p = t.hi.blc.fj) || void 0 === p ? void 0 : p.svrInstanceParams) || void 0 === v ? void 0 : v.playMode) === $.TblEnum.InstancePlayModeTableEnum.ABYSS;

case m.X:
if (g = t.hI) return "number" == typeof (T = i[1])[1] ? g.bfP >= T[0] && g.bfP <= T[1] || g.bfP >= T[1] && g.bfO <= T[0] : g.bfO <= T[0] && g.bfP >= T[0];

case m.T:
T = i[1];
var ct = t.hi.bln(b.default);
return "number" == typeof T[1] ? ct.bhk >= T[0] && ct.bhk <= T[1] || ct.bhk >= T[1] && ct.bhj <= T[0] : ct.bhj <= T[0] && ct.bhk >= T[0];

case m.R:
for (var st = null, pt = 0, dt = t.hi.blc.fu.bkN; pt < dt.length; pt++) {
var vt = dt[pt];
if (n.default.Ga(vt.hv)) if (null === st) st = vt.hD.biI; else if (st !== vt.hD.biI) return !1;
}
return !0;

case m.S:
var $t = t.hi.blc;
return !!$t.fn && $t.fk >= $t.fn;

case m.U:
var mt = t.hH, yt = (T = i[1], mt && mt.bhm);
switch (T[0]) {
case "==":
return (yt[T[1]] || 0) === T[2];

case ">":
return (yt[T[1]] || 0) > T[2];

case "<":
return (yt[T[1]] || 0) < T[2];

case ">=":
return (yt[T[1]] || 0) >= T[2];

case "<=":
return (yt[T[1]] || 0) <= T[2];

case "player1MPIsMax":
return !1;
}
return !1;

case m.V:
return null === (y = t.hi.blc.fI) || void 0 === y ? void 0 : y.cYz;

case m.W:
for (var gt = 0, Dt = t.hi.blc.fu.bkN; gt < Dt.length; gt++) if ((vt = Dt[gt]).hv.bez && 2 === vt.hD.biI) return !1;
return !0;

case m.Y:
return t.hE.bhT >= 2;
}
return !1;
};
t.CU = function(e, i, d, $) {
var m, g, D, j, P, E, x, O, k, _, w, F, B, M, I, R, Y, T, C, G, S, A, X, N, z, Z, L, K, Q, V, J, H, U, W, q, tt, et, it, nt, rt, ot, at, ut, bt, ht, lt, ft, ct, st, pt, dt, vt, $t;
switch (d.t) {
case y.b:
if (null === (g = e.hv) || void 0 === g ? void 0 : g.bdW) return 0;
n.default.EF(e, d.p[0]);
return 1;

case y.c:
e.hi.blc.gu(l.$uo.$vk.F, e, d.p);
break;

case y.d:
d.p[2] ? i.Dr[0] && e.hi.blc.gu(l.$uo.$vk.H, e, i.Dr[0], d.p[1]) : e.hi.blc.gu(l.$uo.$vk.H, e, d.p[0], d.p[1]);
break;

case y.e:
n.default.EJ(e, d.p);
break;

case y.f:
n.default.EE(e);
e.hP();
return 1;

case y.g:
break;

case y.h:
e.hi.blc.gu(l.$uo.$vk.I, e, d.p[0]);
return 0;

case y.i:
e.hE.bhO.hurtbackTime = 0;
e._if();
n.default.Fb(e);
return 0;

case y.j:
if (e.hE.bhO.hurtbackTime > 0 && 1 === (null === (D = e.hE) || void 0 === D ? void 0 : D.bhO.actId)) {
var mt = e.hz;
mt.biZ = 0;
mt.bja = 0;
return 0;
}
var yt = e.hi.bln(u.default);
if (0 !== (Dt = e.hz).biZ) if (e.hw.x >= yt.bkm) {
var gt = n.default.Fv(Dt.biZ, Dt.bja, -1, 0);
Dt.biZ = gt.x;
Dt.bja = gt.y;
e.hi.blc.gu(l.$uo.$vk.J, e, 4);
} else if (e.hw.x <= yt.bkl) {
gt = n.default.Fv(Dt.biZ, Dt.bja, 1, 0);
Dt.biZ = gt.x;
Dt.bja = gt.y;
e.hi.blc.gu(l.$uo.$vk.J, e, 3);
}
if (0 !== Dt.bja) if (e.hw.y >= yt.bko) {
gt = n.default.Fv(Dt.biZ, Dt.bja, 0, -1);
Dt.biZ = gt.x;
Dt.bja = gt.y;
e.hi.blc.gu(l.$uo.$vk.J, e, 1);
} else if (e.hw.y <= yt.bkn) {
gt = n.default.Fv(Dt.biZ, Dt.bja, 0, 1);
Dt.biZ = gt.x;
Dt.bja = gt.y;
e.hi.blc.gu(l.$uo.$vk.J, e, 2);
}
break;

case y.bS:
var Dt, jt = d.p;
if (!(Dt = e.hz)) return 0;
if (0 === Dt.biZ && 0 === Dt.bja) return 0;
var Pt = "_rebEnt_" + (null !== (j = jt.tag) && void 0 !== j ? j : "0"), Et = (hn = e.hE.bhA)[Pt] || 0;
if (null != jt.count && Et >= jt.count) return 0;
var xt = 0, Ot = 0;
if (jt.find) {
if (!(ni = n.default.Eq(e, jt.find, $, i)) || !ni.hw) return 0;
xt = e.hw.x - ni.hw.x;
Ot = e.hw.y - ni.hw.y;
} else {
yt = e.hi.bln(u.default);
if (Dt.biZ > 0 && e.hw.x >= yt.bkm) {
xt = -1;
Ot = 0;
} else if (Dt.biZ < 0 && e.hw.x <= yt.bkl) {
xt = 1;
Ot = 0;
} else if (Dt.bja > 0 && e.hw.y >= yt.bko) {
xt = 0;
Ot = -1;
} else {
if (!(Dt.bja < 0 && e.hw.y <= yt.bkn)) return 0;
xt = 0;
Ot = 1;
}
}
var kt = Math.sqrt(xt * xt + Ot * Ot);
if (kt <= 0) return 0;
var _t = xt / kt, wt = Ot / kt;
if (Dt.biZ * _t + Dt.bja * wt >= 0) return 0;
var Ft = (gt = n.default.Fv(Dt.biZ, Dt.bja, _t, wt)).x, Bt = gt.y, Mt = null !== (P = jt.accel) && void 0 !== P ? P : 1;
if (1 !== Mt) {
Ft = r.$jB.bma(Ft * Mt, 3);
Bt = r.$jB.bma(Bt * Mt, 3);
}
Dt.biZ = Ft;
Dt.bja = Bt;
if (!1 !== jt.syncAngle) {
e.hw.bkw = 1;
var It = r.$jB.bma(180 * Math.atan2(Bt, Ft) / Math.PI, 4);
e.hw.bkx = It;
}
hn[Pt] = Et + 1;
e.hi.blc.gu(l.$uo.$vk.J, e, 0);
break;

case y.k:
e.hi.blc.gu(l.$uo.$vk.K, e, d.p);
break;

case y.l:
var Rt = d.p, Yt = n.default.FB(e, Rt, $);
Rt.id && Rt.behaviorId && !Rt.ai && e.hE.bhH.push({
id: Rt.id,
eid: Yt.hg
});
break;

case y.m:
for (var Tt = d.p, Ct = 0, Gt = e.hE.bhH.filter(function(t) {
return t.id === Tt.id;
}); Ct < Gt.length; Ct++) {
var St = Gt[Ct];
(qt = e.hi.bll(St.eid)) && qt.hx && (n.default.FH(qt.hx, Tt.cmd).c = 1);
}
break;

case y.n:
Rt = d.p;
var At = e.hz;
At.bjg = 0;
if (void 0 !== Rt.x) {
var Xt = Rt.x;
Rt.xr && (Xt = r.$jB.bma(Xt + e.hi.blc.ge() * Rt.xr, 4));
At.biZ = Xt * e.hw.bkw;
}
if (void 0 !== Rt.y) {
Xt = Rt.y;
Rt.yr && (Xt = r.$jB.bma(Xt + e.hi.blc.ge() * Rt.yr, 4));
At.bja = Xt;
}
if (void 0 !== Rt.z) {
Xt = Rt.z;
Rt.zr && (Xt = r.$jB.bma(Xt + e.hi.blc.ge() * Rt.zr, 4));
At.bjb = Xt;
}
if (void 0 !== Rt.ax) {
Xt = Rt.ax;
At.bjc = Xt * e.hw.bkw;
}
if (void 0 !== Rt.ay) {
Xt = Rt.ay;
At.bjd = Xt;
}
if (void 0 !== Rt.az) {
Xt = Rt.az;
At.bje = Xt;
}
if (void 0 !== Rt.xyAngle || void 0 !== Rt.xyAngleRandAdd) {
var Nt = null !== (E = Rt.speed) && void 0 !== E ? E : 0;
Rt.speedRandAdd && (Nt = r.$jB.bma(Nt + e.hi.blc.ge() * Rt.speedRandAdd, 4));
var zt = null !== (x = Rt.speedAcc) && void 0 !== x ? x : 0, Zt = Rt.xyAngle;
Rt.xyAngleRandAdd && (Zt = r.$jB.bma(Zt + e.hi.blc.ge() * Rt.xyAngleRandAdd, 4));
Zt = r.$jB.bma(Zt * Math.PI / 180, 4);
var Lt = r.$jB.bma(Math.cos(Zt)), Kt = r.$jB.bma(Math.sin(Zt));
At.biZ = r.$jB.bma(Nt * Lt * e.hw.bkw);
At.bja = r.$jB.bma(Nt * Kt);
At.bjc = r.$jB.bma(zt * Lt * e.hw.bkw);
At.bjd = r.$jB.bma(zt * Kt);
}
void 0 !== Rt.gzs && (e.hE.bhW = Rt.gzs);
break;

case y.o:
if ("number" == typeof (jt = d.p).probability && !(jt.probability > e.hi.blc.ge())) return 0;
n.default.FN(e, d.p.id, jt.param, e.hg, i);
break;

case y.p:
n.default.FQ(e, d.p.id);
break;

case y.q:
if (!(tr = e).hv) break;
if (!(null === (O = e.hx) || void 0 === O ? void 0 : O.bjX)) break;
if (!(Jt = tr.hx).bjN) {
tr.hz.biZ = 0;
tr.hz.bja = 0;
return;
}
var Qt = d.p && d.p[0];
Nt = Qt ? c.default.CW(Qt, e, i) : null !== (_ = null === (k = tr.hv) || void 0 === k ? void 0 : k.bet) && void 0 !== _ ? _ : 0;
e.hE.bho && n.default.EG(e, e.hE.bho);
var Vt = .32;
Math.abs(Jt.bjO.x) > Vt ? tr.hz.biZ = Math.sign(Jt.bjO.x) * Nt : tr.hz.biZ = 0;
Math.abs(Jt.bjO.y) > Vt ? tr.hz.bja = Math.floor(Math.sign(Jt.bjO.y) * Nt * l.$uo.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y) : tr.hz.bja = 0;
break;

case y.r:
e.hG && (e.hG.bgi = Boolean(d.p));
break;

case y.s:
if (!(null === (w = e.hx) || void 0 === w ? void 0 : w.bjX)) break;
if (null === (F = e.hI) || void 0 === F ? void 0 : F.bfQ) break;
var Jt = e.hx;
if (!e.hw) break;
if (Jt.bjN && Math.abs(Jt.bjO.x) > .07) {
var Ht = Jt.bjO.x > 0;
e.hw.bkw = Ht ? l.$uo.$uU.b : l.$uo.$uU.c;
}
break;

case y.t:
e.hi.blc.gu(l.$uo.$vk.M, e, d.p);
break;

case y.bD:
e.hi.blc.gu(l.$uo.$vk.N, e, d.p);
break;

case y.bE:
Xt = c.default.CW(d.p.val, e, i), jt = {
scale: d.p.scale,
val: Xt,
effId: d.p.effId,
x: d.p.x,
y: d.p.y
};
e.hi.blc.gu(l.$uo.$vk.O, e, jt);
break;

case y.u:
if (e.hw) {
jt = d.p;
var Ut = e.hw;
jt.x && (Ut.x = r.$jB.bma(Ut.x + jt.x * Ut.bkw));
jt.y && (Ut.y = r.$jB.bma(Ut.y + jt.y));
jt.z && (Ut.z = r.$jB.bma(Ut.z + jt.z));
"number" == typeof jt.dir && (Ut.bkw = jt.dir);
jt.varX && "number" == typeof e.hE.bhA[jt.varX] && (Ut.x = e.hE.bhA[jt.varX]);
jt.varY && "number" == typeof e.hE.bhA[jt.varY] && (Ut.y = e.hE.bhA[jt.varY]);
e.ic && n.default.Fu(e, !0);
}
return 0;

case y.v:
(di = e.hi.bln(u.default)).bkp.push({
eid: e.hg,
duration: d.p[0],
time: 0
});
return 0;

case y.w:
var Wt = e.hi.blc;
d.p ? Wt.gq(e) : Wt.gr(e.hg);
return 0;

case y.x:
e.hi.blc.gu(l.$uo.$vk.Q, e, d.p);
return 0;

case y.y:
e.hi.blc.gu(l.$uo.$vk.R, e, d.p);
return 0;

case y.z:
n.default.EV(e, d.p);
return 0;

case y.A:
if (e.hw) {
var qt = null;
if (d.p.targetIdInVar) {
var te = null === (B = e.hE) || void 0 === B ? void 0 : B.bhA[d.p.targetIdInVar];
qt = e.hi.bll(te);
} else if (d.p.child) {
for (var ee = 0, ie = e.hE.bhH; ee < ie.length; ee++) if ((Le = ie[ee]).id === d.p.child) {
qt = e.hi.bll(Le.eid);
break;
}
} else e.hG && (qt = e.hi.bll(e.hG.bgj));
if (qt && qt.hw) {
Ut = e.hw;
var ne = qt.hw, re = null !== (M = d.p.x) && void 0 !== M ? M : 0, oe = null !== (I = d.p.y) && void 0 !== I ? I : 0;
if (d.p.neartest) {
0 == (Gi = ne.x - Ut.x) && (Gi = 1);
Ut.x = r.$jB.bma(ne.x - Math.sign(Gi) * re);
Ut.y = r.$jB.bma(ne.y + oe);
Ut.bkw = Math.sign(Gi);
} else {
Ut.x = r.$jB.bma(ne.x + r.$jB.bma(ne.bkw * re));
Ut.y = r.$jB.bma(ne.y + oe);
}
e.ic && n.default.Fu(e, !0);
}
}
return 0;

case y.B:
if ((jt = d.p).raduisChange) {
e.hJ && (e.hJ.biF = r.$jB.bma(e.hJ.biF + jt.raduisChange));
return;
}
if (jt.surroundSpeedChange) {
e.hJ && (e.hJ.biE = r.$jB.bma(e.hJ.biE + jt.surroundSpeedChange));
return;
}
if (jt.cancel) {
n.default.FD(e);
return;
}
qt = void 0;
if (!(null == (qt = jt.find ? n.default.Eq(e, jt.find, $, i) : e.hi.bll(e.hE.bhG)) ? void 0 : qt.hw)) return 0;
n.default.FC(e, qt, jt);
return 0;

case y.C:
return 0;

case y.D:
break;

case y.E:
Nt = d.p[0];
d.p[1] && (Nt += r.$jB.bma(e.hi.blc.ge() * d.p[1], 3));
var ae = e.hz;
ae || (ae = e.hn(a.default));
0 === ae.bjh && (ae.bjh = 1);
ae.bjh = Math.sign(ae.bjh) * (Math.abs(ae.bjh) + Math.abs(Nt));
break;

case y.F:
e.hi.blc.gu(l.$uo.$vk.S, e);
return 0;

case y.G:
if (d.p) {
e.hG || n.default.FZ(e, !1);
e.hG.bgi = !!d.p[0];
e.hG.bgo = null !== (R = d.p[1]) && void 0 !== R ? R : 0;
} else e.hq(o.default);
break;

case y.H:
Wt = e.hi.blc;
if (!e.hD) return;
var ue = e.hD.biI, be = (Ut = e.hw, d.p.dis), he = (Xt = d.p.val, null === (Y = d.p.sameCamp) || void 0 === Y || Y), le = null === (T = d.p.floatValue) || void 0 === T || T, fe = Math.floor(c.default.CW(Xt, e, i));
if (be) for (var ce = 0, se = Wt.fu.bkN; ce < se.length; ce++) {
var pe = se[ce];
if (pe.hD.biI === ue === he && n.default.Ga(pe.hv) && n.default.Gd(pe.hw, Ut) < be) if (fe > 0) {
pe.hY(e, fe);
le && e.hi.blc.gu(l.$uo.$vk.E, pe, -fe, !1);
} else if (fe < 0) {
var de = Math.abs(fe);
pe.hX(e, de);
!pe.hv.bez && n.default.GJ(pe, 0, 0);
d.p.emitEvent && n.default.Gu(pe, null, l.$uo.$vh.d, null);
le && e.hi.blc.gu(l.$uo.$vk.E, pe, -fe, !1);
}
} else if (null === (C = e.hv) || void 0 === C ? void 0 : C.bez) if (fe > 0) {
e.hY(e, fe);
e.hi.blc.gu(l.$uo.$vk.E, e, -fe, !1);
} else if (fe < 0) {
de = Math.abs(fe);
e.hX(e, de);
!e.hv.bez && n.default.GJ(e, 0, 0);
d.p.emitEvent && n.default.Gu(e, null, l.$uo.$vh.d, null);
le && e.hi.blc.gu(l.$uo.$vk.E, e, -fe, !1);
}
break;

case y.I:
n.default.GM(e, d.p, i, $);
return 0;

case y.J:
var ve = e.hE, $e = (Rt = d.p)[2];
(hn = ve.bhA)[$e] || (hn[$e] = 0);
var me = Rt[0].map(function(t) {
return c.default.CW(t, e, i);
}), ye = e.hi.blc.gf(me);
(0 !== me[0] || 0 !== ye) && (hn[$e] = Rt[1][ye] || 0);
return 0;

case y.K:
e.hi.blc.gu(l.$uo.$vk.T, e, d.p);
return 0;

case y.L:
ve = e.hE;
var ge = null;
if (null == $ ? void 0 : $.target) ge = $.target; else {
var De = null;
if (e.hG && -1 !== e.hG.bgj) De = e; else {
var je = e.hi.bll(ve.bhF);
je && (De = je);
}
De && De.hG && (ge = e.hi.bll(De.hG.bgj));
}
if (ge && ge.hw) {
Ut = e.hw, ne = ge.hw;
var Pe = r.$jB.bma(Math.atan2(ne.y - Ut.y, ne.x - Ut.x), 4);
Zt = r.$jB.bma(180 * Pe / Math.PI, 3);
n.default.Fr(Ut, Zt);
}
return 0;

case y.M:
var Ee = e.hE.bhA[d.p[0]] || d.p[1] || "1";
n.default.EF(e, Ee);
return 0;

case y.N:
qt = null;
if ((jt = d.p).find) {
if (!(qt = n.default.Eq(e, jt.find, $, i))) return 0;
} else {
var xe = $;
if (!xe) return 0;
if (!xe.target) return 0;
qt = xe.target;
}
if ("number" == typeof jt.probability && !(jt.probability > e.hi.blc.ge())) return 0;
n.default.FN(qt, jt.id, jt.param, e.hg);
return 0;

case y.O:
e.hE.bhL = d.p;
if (d.p.bindChildId) {
for (var Oe = (Ae = e.hE.bhH).length - 1; Oe >= 0; Oe--) if (Ae[Oe].id === d.p.bindChildId) {
e.hE.bhK = Ae[Oe].eid;
break;
}
} else e.hE.bhK = -1;
n.default.EX(e);
return 0;

case y.P:
var ke = d.p;
switch (ke.index) {
case 1:
var _e = e.hD.biI, we = e.hi.blc.fu.bkN.filter(function(t) {
return t.hg > 0 && t.hv.bez && _e !== t.hD.biI;
});
ke.var && (we = we.filter(function(t) {
return t.hE && !!t.hE.bhA[ke.var];
}));
if (ke.excludeEid) {
var Fe = e.hE.bhA[ke.excludeEid];
we = we.filter(function(t) {
return t.hg !== Fe;
});
}
ke.isPlayer && (we = we.filter(function(t) {
return !!t.hx.bjK;
}));
ke.randomNum && (Ze = we.length) > 1 && (we = e.hi.blc.gA(we, Math.min(ke.randomNum, Ze)));
for (var Be = 0, Me = we; Be < Me.length; Be++) if ((Je = Me[Be]).hg > -1 && n.default.Ga(Je.hv)) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: Je
});
ke.tlg && n.default.FM(Je, i, ke.tlg, {
target: e
});
}
break;

case 2:
for (var Ie = e.hE.bhI, Re = e.hi, Ye = 0, Te = Ie; Ye < Te.length; Ye++) {
var Ce = Te[Ye];
if (Je = Re.bll(Ce.eid)) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: Je
});
ke.tlg && n.default.FM(Je, i, ke.tlg, {
target: e
});
}
}
break;

case 3:
ue = e.hD.biI;
for (var Ge = 0, Se = we = e.hi.blc.fu.bkN; Ge < Se.length; Ge++) if ((Je = Se[Ge]).hg > -1 && Je.hg !== e.hg && n.default.Ga(Je.hv) && ue === Je.hD.biI) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: Je
});
ke.tlg && n.default.FM(Je, i, ke.tlg, {
target: e
});
}
if (ke.includeSelf) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: e
});
ke.tlg && n.default.FM(e, i, ke.tlg, {
target: e
});
}
break;

case 4:
break;

case 5:
var Ae;
for (Oe = (Ae = Array.from(e.hi.bkW.values())).length - 1; Oe >= 0; Oe--) {
var Xe = Ae[Oe];
if (Xe.hE && Xe.hE.bhA[ke.var] && (qt = e.hi.bll(Xe.hg))) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: qt
});
ke.tlg && n.default.FM(qt, i, ke.tlg, {
target: e
});
}
}
break;

case 6:
break;

case 7:
var Ne = (ze = e.hi).bll(e.hE.bhG);
if (Ne) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: Ne
});
ke.tlg && n.default.FM(Ne, i, ke.tlg, {
target: e
});
}
break;

case 8:
break;

case 9:
var ze = e.hi, Ze = 0;
for (we = [], Oe = e.hE.bhH.length - 1; Oe >= 0; Oe--) {
var Le;
if ((Le = e.hE.bhH[Oe]).id === ke.groupId && (qt = ze.bll(Le.eid))) {
Ze++;
we.push(qt);
}
if (ke.num && Ze >= ke.num) break;
}
if (ke.randomNum) {
var Ke = we.length;
Ke > 1 && (we = e.hi.blc.gA(we, Math.min(ke.randomNum, Ke)));
}
for (var Qe = 0, Ve = we; Qe < Ve.length; Qe++) {
var Je = Ve[Qe];
ke.lg && n.default.FM(e, i, ke.lg, {
target: Je
});
ke.tlg && n.default.FM(Je, i, ke.tlg, {
target: e
});
}
break;

case 10:
we = e.hi.blc.fu.bkN;
ke.isPlayer && (we = we.filter(function(t) {
return !!t.hx.bjK;
}));
ke.var && (we = we.filter(function(t) {
return t.hE && !!t.hE.bhA[ke.var];
}));
if (ke.excludeEid) {
var He = e.hE.bhA[ke.excludeEid];
we = we.filter(function(t) {
return t.hg !== He;
});
}
ke.roleId && (we = we.filter(function(t) {
return t.hv && ke.roleId === t.hv.bdQ.id;
}));
ke.isAlive && (we = we.filter(function(t) {
return n.default.Ga(t.hv);
}));
if (ke.isOrigin) {
var Ue = e.hg;
we = we.filter(function(t) {
return t.hE && t.hE.bhF === Ue;
});
}
ke.randomNum && (Ze = we.length) > 1 && (we = e.hi.blc.gA(we, Math.min(ke.randomNum, Ze)));
for (var We = 0, qe = we; We < qe.length; We++) if ((Je = qe[We]).hg > -1) {
ke.lg && n.default.FM(e, i, ke.lg, {
target: Je
});
ke.tlg && n.default.FM(Je, i, ke.tlg, {
target: e
});
}
}
return 0;

case y.Q:
var ti = null !== (G = (Rt = d.p).worldX) && void 0 !== G ? G : 0, ei = null !== (S = Rt.worldY) && void 0 !== S ? S : 0;
if (Rt.center) {
var ii = e.hi.bln(u.default);
ti = r.$jB.bma(ii.bkl + .5 * (ii.bkm - ii.bkl));
ei = r.$jB.bma(ii.bkn + .5 * (ii.bko - ii.bkn));
}
var ni = void 0;
if (Rt.find) {
if (!(ni = n.default.Eq(e, Rt.find, $, i))) {
Rt.noTargetMoveX && (e.hw.x = r.$jB.bma(e.hw.x + Rt.noTargetMoveX * e.hw.bkw));
return 0;
}
if (null == ni ? void 0 : ni.hw) {
ti = null == ni ? void 0 : ni.hw.x;
ei = null == ni ? void 0 : ni.hw.y;
Rt.x && (ti = r.$jB.bma(ti + Rt.x * ni.hw.bkw));
Rt.y && (ei = r.$jB.bma(ei + Rt.y));
}
}
Ut = e.hw;
if ("number" == typeof Rt.time) if (0 !== Rt.time) {
var ri = r.$jB.bma(ti - e.hw.x), oi = r.$jB.bma(ei - e.hw.y);
Rt.LimitDisX && Math.abs(ri) > Rt.LimitDisX && (ri = Math.sign(ri) * Rt.LimitDisX);
Rt.LimitDisY && Math.abs(oi) > Rt.LimitDisY && (oi = Math.sign(oi) * Rt.LimitDisY);
if (Math.abs(ri) > 5) {
e.hz.biZ = r.$jB.bma(ri / Rt.time, 3);
Rt.changeDir && (e.hw.bkw = Math.sign(e.hz.biZ) || 1);
} else e.hz.biZ = 0;
Math.abs(oi) > 5 ? e.hz.bja = r.$jB.bma(oi / Rt.time, 3) : e.hz.bja = 0;
} else {
Ut.x = ti;
Ut.y = ei;
Rt.changeDir && (null == ni ? void 0 : ni.hw) && (ni.hw.x - Ut.x > 0 ? e.hw.bkw = 1 : e.hw.bkw = -1);
} else if (Rt.speed) {
ri = r.$jB.bma(ti - e.hw.x), oi = r.$jB.bma(ei - e.hw.y);
Math.abs(ri) < 5 && (ri = 0);
Math.abs(oi) < 5 && (oi = 0);
n.default.Ec.bmn(ri, oi);
var ai = r.$jB.blZ(n.default.Ec);
e.hz.biZ = r.$jB.bma(ai.x * Rt.speed);
e.hz.bja = r.$jB.bma(ai.y * Rt.speed);
Rt.changeDir && (e.hw.bkw = Math.sign(e.hz.biZ) || 1);
if (Rt.speedAcc) {
e.hz.bjc = r.$jB.bma(ai.x * Rt.speedAcc);
e.hz.bjd = r.$jB.bma(ai.y * Rt.speedAcc);
} else {
e.hz.bjc = 0;
e.hz.bjd = 0;
}
}
return 0;

case y.R:
0 === (jt = d.p).id ? n.default.EC(e) : n.default.EU(e, jt.id);
return 0;

case y.S:
(null === (N = null === (X = null === (A = e.hE) || void 0 === A ? void 0 : A.bhw) || void 0 === X ? void 0 : X.heroNoramlSkillCmds) || void 0 === N ? void 0 : N.length) > 0 && n.default.ET(e, null === (Z = null === (z = e.hE) || void 0 === z ? void 0 : z.bhw) || void 0 === Z ? void 0 : Z.heroNoramlSkillCmds);
return 0;

case y.T:
e.hi.blc.gu(l.$uo.$vk.X, e, d.p);
return 0;

case y.U:
var ui = d.p;
if (0 === ui.d) {
var bi = e.hE.bhN.filter(function(t) {
return t.id !== ui.id;
});
e.hE.bhN = bi;
} else e.hE.bhN.find(function(t) {
return t.id === ui.id;
}) || e.hE.bhN.push({
id: ui.id,
time: 0,
destroyTime: ui.d,
vfxInfo: ui.vfx
});
return 0;

case y.V:
(null == $ ? void 0 : $.target) && n.default.GM(null == $ ? void 0 : $.target, d.p, i, $);
return 0;

case y.W:
jt = d.p;
e.hE.bhR = jt.height > 0;
e.hE.bhS = jt;
return 0;

case y.X:
n.default.Ft(e, d.p);
return 0;

case y.Y:
var hi = d.p;
if (hi.modelCfgId) if (-1 === (fi = e.hF.bgg.findIndex(function(t) {
return t.id === hi.id;
}))) e.hF.bgg.push({
id: hi.id,
modelId: hi.modelCfgId
}); else {
var li = e.hF.bgg[fi];
li.id = hi.id;
li.modelId = hi.modelCfgId;
} else {
var fi;
-1 !== (fi = e.hF.bgg.findIndex(function(t) {
return t.id === hi.id;
})) && e.hF.bgg.splice(fi, 1);
}
e.hF.bgh = e.hi.blc.fi;
return 0;

case y.Z:
return 0;

case y.ba:
var ci = (Rt = d.p)[0];
-1 === ci && (ci = null === (K = null === (L = e.hv) || void 0 === L ? void 0 : L.bdQ) || void 0 === K ? void 0 : K.AI);
n.default.Es(e, ci);
return 0;

case y.bb:
var si = d.p, pi = si.remainingPercent;
si.skId && (null == (An = e.hv.beg(si.skId)) ? void 0 : An.cd) > 0 && (An.cd = Math.max(0, r.$jB.bma(An.cd - r.$jB.bma(pi * An.cd))));
si.lvRange && e.hv.bdU.forEach(function(t) {
t.cd > 0 && t.Dg && t.Dg.upgradeLv >= si.lvRange[0] && t.Dg.upgradeLv <= si.lvRange[1] && (t.cd = Math.max(0, r.$jB.bma(t.cd - r.$jB.bma(pi * t.cd))));
});
return 0;

case y.bc:
Rt = d.p;
var di = e.hi.bln(u.default);
Array.isArray(Rt.layers) && (di.bkq = Rt.layers);
e.hi.blc.gu(l.$uo.$vk.Z);
return 0;

case y.bd:
d.p && n.default.Fr(e.hw, d.p[0]);
return 0;

case y.be:
var vi = d.p;
if (vi) {
Zt = n.default.Gc(e.hw.x, e.hw.y, vi[0], vi[1]);
n.default.Fr(e.hw, Zt);
}
return 0;

case y.bf:
return 0;

case y.bg:
jt = d.p;
e.hi.blc.gu(l.$uo.$vk.ba, e, jt);
return 0;

case y.bh:
jt = d.p;
e.hi.blc.gu(l.$uo.$vk.bo, e, jt);
return 0;

case y.bi:
if (d.p) {
if (ni = n.default.Eq(e, d.p.find, $)) {
n.default.FZ(e, !0);
e.hG.bgj = ni.hg;
e.hG.bgn = l.$uo.$uS.c;
e.hG.bgm = 9999;
}
} else e.hG && (e.hG.bgn = l.$uo.$uS.b);
return 0;

case y.bj:
ni = e;
d.p && (ni = null == $ ? void 0 : $.target);
if (e.hx.bjY && ni) {
var $i = e.hi.blc.gk(e.hx.bjK);
$i && ($i.cYa = ni.hg);
}
return 0;

case y.bk:
jt = d.p;
e.hi.blc.gu(l.$uo.$vk.bp, e, jt);
return 0;

case y.bl:
jt = d.p;
return 0;

case y.cj:
n.default.Eu(e, d.p[0]);
return 0;

case y.ck:
e.hx && (n.default.FH(e.hx, d.p[0]).c = 1);
return 0;

case y.cl:
var mi = e.hI, yi = e.hG, gi = e.hw;
if (!mi) return 0;
if (!yi) return 0;
var Di = e.hi.bll(e.hG.bgj);
if (!Di) return 0;
var ji = Di.hw;
if (!ji) return 0;
yt = e.hi.bln(u.default);
var Pi = d.p[0] || 650, Ei = d.p[1] || 90, xi = d.p[2] || 0, Oi = yi.bgl.bmu(new f.default()), ki = r.$jB.bma((e.hi.blc.ge() - .5) * Ei, 3), _i = r.$jB.bma(Pi * (e.hi.blc.ge() - .5) + xi, 3), wi = r.$jB.bma(ki * Math.PI / 180, 3), Fi = Oi.bmp();
r.$jB.blX(Fi, wi);
r.$jB.blY(Fi, _i);
var Bi = r.$jB.bma(Fi.x + ji.x, 3), Mi = r.$jB.bma(Fi.y + ji.y, 3);
Bi = Math.max(Math.min(Bi, yt.bkm), yt.bkl);
Mi = Math.max(Math.min(Mi, yt.bko), yt.bkn);
mi.bfM.bmn(Bi, Mi);
return 0;

case y.cm:
if (!(Jt = e.hx)) return 0;
if (!(mi = e.hI)) return 0;
if (!(gi = e.hw)) return 0;
var Ii = new f.default(gi.x, gi.y), Ri = mi.bfM.bmq(Ii, new f.default()), Yi = r.$jB.blZ(Ri.bmp());
if (Ri.bmt() < 2500) Jt.bjN = 0; else {
var Ti = Ii.bmn(1, 0), Ci = r.$jB.bma(Math.atan2(Yi.y, Yi.x), 3);
if (1 === Jt.bjN) {
var Gi;
if ((Gi = r.$jB.bma(Ci - Jt.bjP)) > .06) {
var Si = 2 * Math.PI;
(Gi %= Si) > Math.PI ? Gi -= Si : Gi < -Math.PI && (Gi += Si);
Gi > Math.PI / 3 && (Ci = r.$jB.bma(Jt.bjP + .5 * Gi, 3));
}
} else Jt.bjN = 1;
r.$jB.blX(Ti, Ci);
Jt.bjO.bmn(Ti.x, Ti.y);
Jt.bjP = Ci;
}
return 0;

case y.cn:
if (Fn = e.hE) {
var Ai = Fn.bhA[d.p[0]];
Ai && n.default.Eu(e, Ai);
}
return 0;

case y.co:
e.hx && n.default.Gg(e.hx, d.p[0], d.p[1]);
return 0;

case y.cp:
if (e.hx) {
(Jt = e.hx).bjN = d.p[0];
if (d.p[0] && e.hG) {
Jt.bjO.bmo(e.hG.bgl);
Jt.bjP = r.$jB.bma(Math.atan2(Jt.bjO.y, Jt.bjO.x), 3);
}
}
return 0;

case y.bs:
var Xi = e.hi.bln(b.default);
n.default.Gf(Xi, d.p[0]);
return 1;

case y.bm:
Object.assign(e.hi.blc.fT, d.p);
e.hi.blc.gu(l.$uo.$vk.P, e, d.p);
return 0;

case y.bn:
e.hi.blc.gu(l.$uo.$vk.U, d.p);
return 0;

case y.bt:
e.hi.blc.gu(l.$uo.$vk.V, d.p);
return 0;

case y.bp:
(Wt = e.hi.blc).gr(0);
Wt.go(r.$jB.bma(d.p[0], 3));
return 0;

case y.bx:
var Ni = e.hi.blc;
Ni.gr(0);
var zi = [];
Ni.fF.forEach(function(t) {
if (t.cYo()) {
var e = Ni.gi(t.cXZ);
if (e && e.hv.bez) for (var i = 0, n = e.hv.beb; i < n.length; i++) {
var r = n[i];
Math.abs(r.frameId - Ni.fi) < 3 && zi.push(t.cXL);
}
}
});
if (zi.length > 0) {
Ni.go(0);
e.hi.bln(b.default).bhn(3, zi);
} else {
Ni.go(0);
e.hi.bln(b.default).bhn(.5);
}
e.hi.blc.gu(l.$uo.$vk.j);
return 0;

case y.bX:
we = (Wt = e.hi.blc).fu.bkN;
var Zi = 0, Li = !1;
Wt.fn > 0 && 0 === Math.max(Wt.fn - Wt.fk, 0) && (Li = !0);
for (var Ki = 0, Qi = we; Ki < Qi.length; Ki++) {
var Vi = Qi[Ki];
n.default.Ga(Vi.hv) && Vi.hD.biJ > 0 && Zi++;
}
Wt.fI.cYz = 0 === Zi && !Li;
return 0;

case y.bY:
var Ji = e.hi.blc, Hi = Ji.fI.cYI[0];
Hi = {
winner: 0,
duration: Ji.fk,
infos: []
};
Ji.fI.cYI.push(Hi);
Ji.fF.forEach(function(t) {
var e = t.cYo();
if (e) {
var i = Ji.gi(t.cXZ);
if (i) {
var n = e.hpRate;
e.hpRate = i.hv.beu > 0 ? r.$jB.bma(i.hv.beu / i.hv.bev) : 0;
e.hp = i.hv.beu;
Hi.infos.push({
playerId: t.cXL,
heroId: e.roleCfgId,
hp: [ n, e.hpRate ]
});
}
}
});
var Ui = 0, Wi = 0;
Ji.fF.forEach(function(t) {
var e = t.cYo();
e && e.hp > Wi && (Wi = e.hp);
});
Ji.fF.forEach(function(t) {
var e = t.cYo();
e && e.hp === Wi && Ui++;
});
Ji.fF.forEach(function(t) {
var e = t.cYo();
if (e) if (Ui > 1) Ji.fI.cYB = -1; else if (e.hp === Wi) {
Ji.fI.cYB = t.cXQ;
Hi.winner = t.cXL;
}
});
Ji.fI.cYz = !0;
return 0;

case y.bZ:
case y.ca:
return 0;

case y.cb:
n.default.GD(e.hi.blc, e.hi.blc.fp);
return 0;

case y.ce:
return 0;

case y.cf:
n.default.GB(e.hi.blc, e.hi.blc.fp);
return 0;

case y.cg:
n.default.GE(e.hi.blc);
return 0;

case y.ch:
(Wt = e.hi.blc).gu(l.$uo.$vk.bw);
return 0;

case y._cc:
var qi = e.hi.blc;
gt = qi.fI.cYI[0];
gt = {
winner: 0,
duration: qi.fk,
infos: []
};
qi.fI.cYI.push(gt);
qi.fF.forEach(function(t) {
t.cYd = 0;
t.cYm.forEach(function(e) {
var i = qi.gi(e.entityId);
if (i) {
e.hpRate = i.hv.beu > 0 ? r.$jB.bma(i.hv.beu / i.hv.bev) : 0;
e.hp = i.hv.beu;
t.cYd += Math.ceil(r.$jB.bma(100 * e.hpRate));
}
});
});
var tn = 0, en = 0;
qi.fF.forEach(function(t) {
t.cYd > en && (en = t.cYd);
});
qi.fF.forEach(function(t) {
t.cYd === en && tn++;
});
if (tn > 1) qi.fI.cYB = -1; else {
var nn = qi.fF.find(function(t) {
return t.cYd === en;
});
qi.fI.cYB = nn.cXQ;
gt.winner = nn.cXL;
}
qi.fI.cYz = !0;
return 0;

case y.cd:
Wt = e.hi.blc;
return 0;

case y.ci:
Wt = e.hi.blc;
n.default.GQ(Wt, d.p);
return 0;

case y.bq:
if ((Wt = e.hi.blc).fI.cYz) if (Wt.gN) for (var rn = 0, on = we = Wt.fu.bkN; rn < on.length; rn++) (Je = on[rn]).hD.biI === Wt.fI.cYB && n.default.Ga(Je.hv) && Je.hx.bjK && Je.hE && Je.hE.bhw.winBehaviorId && n.default.EF(Je, Je.hE.bhw.winBehaviorId); else for (var an = 0, un = we = Wt.fu.bkN; an < un.length; an++) {
Je = un[an];
n.default.Ga(Je.hv) && Je.hx.bjK && Je.hE && Je.hE.bhw.winBehaviorId && n.default.EF(Je, Je.hE.bhw.winBehaviorId);
}
return 0;

case y.bo:
Wt = e.hi.blc;
e.hi.blc.gz();
return 0;

case y.br:
var bn = e.ie;
if (bn) {
bn.cYz || (bn.cYO = e.hi.blc.fk);
e.hi.blc.gu(l.$uo.$vk.k, bn.cYz);
}
return 0;

case y.bu:
ve = e.hi.bln(b.default), Rt = d.p;
var hn = ve.bhm;
switch (Rt[0]) {
case "=Random":
hn[Rt[1]] = Math.floor(1e4 * e.hi.blc.ge());
break;

case "+=dt":
var ln = e.hi.bln(h.default), fn = r.$jB.bma(e.hi.blc.fp * ln.worldTimeScale, 4);
hn[Rt[1]] = r.$jB.bma((hn[Rt[1]] || 0) + fn, 4);
break;

case "=":
hn[Rt[1]] = Rt[2];
break;

case "+=":
hn[Rt[1]] = (hn[Rt[1]] || 0) + Rt[2];
break;

case "-=":
hn[Rt[1]] = (hn[Rt[1]] || 0) - Rt[2];
break;

case "*=":
hn[Rt[1]] = Math.floor((hn[Rt[1]] || 0) * Rt[2]);
break;

case "/=":
hn[Rt[1]] = Math.floor((hn[Rt[1]] || 0) / Rt[2]);
break;

case "+":
hn[Rt[1]] = (hn[Rt[2]] || 0) + (hn[Rt[3]] || 0);
break;

case "-":
hn[Rt[1]] = (hn[Rt[2]] || 0) - (hn[Rt[3]] || 0);
break;

case "*":
hn[Rt[1]] = Math.floor((hn[Rt[2]] || 0) * (hn[Rt[3]] || 0));
break;

case "/":
hn[Rt[1]] = Math.floor((hn[Rt[2]] || 0) / (hn[Rt[3]] || 0));
}
return 0;

case y.bv:
e.hi.blc.gu(l.$uo.$vk.f, d.p);
return 0;

case y.bw:
return 0;

case y.by:
for (var cn = 0, sn = we = (Wt = e.hi.blc).fu.bkN; cn < sn.length; cn++) (Je = sn[cn]).hx.bjY && n.default.Ga(Je.hv) && Je.hE && Je.hE.bhw.winBehaviorId && n.default.EF(Je, Je.hE.bhw.showBehaviorId);
return 0;

case y.bz:
for (var pn = 0, dn = e.hE.bhs.slice(); pn < dn.length; pn++) {
var vn = dn[pn];
vn.Dq.abnormalType && n.default.FP(e, vn, !1);
}
return 0;

case y.bA:
jt = d.p;
var $n = $ instanceof s.$ge.$gh ? $ : null, mn = (qt = null !== (Q = jt.target && n.default.Eq(e, jt.target, $, i)) && void 0 !== Q ? Q : null == $ ? void 0 : $.target, 
null !== (V = jt.form && n.default.Eq(e, jt.form, $, i)) && void 0 !== V ? V : e);
Wt = e.hi.blc;
if (jt.val) {
fe = c.default.CW(jt.val, e, i);
if ((null == mn ? void 0 : mn.hv) && (null === (J = null == qt ? void 0 : qt.hv) || void 0 === J ? void 0 : J.bez)) {
var yn = n.default.Ef(mn, mn, qt);
yn.isPhysical = mn.hv.bfc > mn.hv.bfd;
yn.elementValue = null !== (H = jt.elementValue) && void 0 !== H ? H : 0;
var gn = n.default.FV(Wt, yn, fe / p.GameAttributeConstRatioRange, !0);
yn.finalDamage = gn;
qt.hX(mn, gn);
!qt.hv.bez && n.default.GJ(qt, 0, 0);
Wt.gu(l.$uo.$vk.E, qt, gn, yn.isCrt);
n.default.Eg(yn);
}
} else if (jt.fixVal) {
fe = c.default.CW(jt.fixVal, e, i);
if (null === (U = null == qt ? void 0 : qt.hv) || void 0 === U ? void 0 : U.bez) {
gn = fe;
qt.hX(mn, gn);
!qt.hv.bez && n.default.GJ(qt, 0, 0);
Wt.gu(l.$uo.$vk.E, qt, gn, !1);
}
} else if (jt.addVal && (null == mn ? void 0 : mn.hv) && (null === (W = null == qt ? void 0 : qt.hv) || void 0 === W ? void 0 : W.bez)) {
fe = c.default.CW(jt.addVal, e, i);
var Dn = mn.hv, jn = qt.hv, Pn = null !== (q = jt.elementValue) && void 0 !== q ? q : 0, En = 1;
if (Pn > 0) {
var xn = p.default.bEC.bEA, On = Number.MIN_SAFE_INTEGER;
if (Pn & l.$uo.$uJ.b) {
On = Math.max(On, Dn.bfo - jn.bfs);
l.$uo.$uJ.b;
}
if (Pn & l.$uo.$uJ.c) {
On = Math.max(On, Dn.bfp - jn.bft);
l.$uo.$uJ.c;
}
if (Pn & l.$uo.$uJ.d) {
On = Math.max(On, Dn.bfq - jn.bfu);
l.$uo.$uJ.d;
}
if (Pn & l.$uo.$uJ.e) {
On = Math.max(On, Dn.bfr - jn.bfv);
l.$uo.$uJ.e;
}
En = Math.max(r.$jB.bma(1 + xn.streArg1 + r.$jB.bma(On / xn.streArg2)), 0);
}
gn = Math.ceil(r.$jB.bma(i.DF * (fe / p.GameAttributeConstRatioRange) * En));
qt.hX(mn, gn, $n);
!qt.hv.bez && n.default.GJ(qt, 0, 0);
Wt.gu(l.$uo.$vk.E, qt, gn, !1);
}
return 0;

case y.bB:
jt = d.p;
for (var kn = 0, _n = e.hE.bic; kn < _n.length; kn++) {
var wn = _n[kn];
wn.c === jt.name && (wn.e = jt.enable ? 1 : 0);
}
return 0;

case y.bC:
e.hw.bkw *= -1;
return 0;

case y.bF:
return 0;

case y.bG:
i.DF = 1;
i.DG = null !== (tt = d.p.reflectW) && void 0 !== tt ? tt : 0;
i.DH = null !== (et = d.p.damageW) && void 0 !== et ? et : 0;
return 0;

case y.bH:
jt = d.p;
var Fn = e.hE, Bn = e.hy, Mn = e.hv;
if (!Fn.bhw.changeGroup) return;
if (0 === Bn.mainTimeScale) return 0;
var In = null;
if (jt.consumeSid) {
In = Mn.beg(jt.consumeSid);
if (!n.default.Ff(e, In)) return 0;
}
var Rn = Fn.bhw.changeGroup[jt.changesGroup];
if (!Rn) return 0;
for (Oe = 0; Oe < Rn.length; Oe++) {
for (var Yn = Rn[Oe], Tn = !0, Cn = 0; Cn < Yn.tgs.length; Cn++) {
var Gn = Yn.tgs[Cn], Sn = t.CT(e, null, Gn, null);
if (Gn[2] ? Sn : !Sn) {
Tn = !1;
break;
}
}
if (Tn && (Yn.t !== y.b || Yn.p[0] !== Fn.bhx)) {
if (Mn && (Yn.sid || Yn.csid)) {
if (0 === Bn.mainTimeScale) continue;
var An = null;
if (Yn.sid) {
var Xn = Yn.sid;
An = Mn.beg(Xn);
} else An = Mn.bdU.find(function(t) {
return t.Dg.commonType === d.csid;
});
if (!An) continue;
if (!n.default.Ff(e, An)) continue;
n.default.Fg(e, An);
if (e.hL) {
var Nn = e.hL.bjH[An.Dg.id];
Nn && (e.hx.bjZ(Nn).f = -9999);
}
In && n.default.Fg(e, In);
}
if (1 === (gt = t.CU(e, null, Yn, null))) {
e.hi.blc.gu(l.$uo.$vk.bn, e, l.$uo.spawnGhostParam);
return;
}
}
}
return 0;

case y.bI:
jt = d.p, ve = e.hE;
if (jt.start) {
if (!(ni = null == $ ? void 0 : $.target)) return 0;
ve.bhA.__PosToEnemyRule1_target = ni.hg;
ve.bhA.__PosToEnemyRule1_record = ((m = {})[ni.hg] = 1, m);
return 0;
}
we = e.hi.blc.fu.bkN;
for (var zn = 9999, Zn = 0, Ln = null, Kn = (ue = e.hD.biI, e.hw), Qn = ve.bhA.__PosToEnemyRule1_record || {}, Vn = 0, Jn = we; Vn < Jn.length; Vn++) if ((Je = Jn[Vn]).hg > -1 && Je.hv.bez && ue !== Je.hD.biI && !(null === (it = Je.hE) || void 0 === it ? void 0 : it.biv)) {
if ((be = n.default.Gd(Je.hw, Kn)) > jt.range) continue;
if (ve.bhA.__PosToEnemyRule1_target === Je.hg) continue;
Qn[Je.hg] = Qn[Je.hg] || 0;
var Hn = Qn[Je.hg];
if (zn > Hn || Zn > be && zn === Hn) {
Ln = Je;
zn = Hn;
Zn = be;
}
}
Ln && (Qn[Ln.hg] += 1);
ve.bhA.__PosToEnemyRule1_target = null !== (nt = null == Ln ? void 0 : Ln.hg) && void 0 !== nt ? nt : -1;
return 0;

case y.bJ:
e.hi.blc.gu(l.$uo.$vk.bm, e, d.p);
return 0;

case y.bK:
if (!e.hD) return;
Xt = d.p.val, fe = Math.floor(c.default.CW(Xt, e, i));
(null === (rt = e.hv) || void 0 === rt ? void 0 : rt.bez) && e.hZ(e, fe);
break;

case y.bL:
if (0 === d.p.size) e.hq(v.default); else {
var Un = e.hn(v.default);
Un.bks = null !== (ot = d.p.size) && void 0 !== ot ? ot : 100;
Un.bkt = null !== (at = d.p.offsetX) && void 0 !== at ? at : 0;
}
break;

case y.bM:
e.hi.blc.gu(l.$uo.$vk.bd, e, d.p);
break;

case y.bN:
ti = null !== (bt = null === (ut = e.hw) || void 0 === ut ? void 0 : ut.x) && void 0 !== bt ? bt : 0, 
ei = null !== (lt = null === (ht = e.hw) || void 0 === ht ? void 0 : ht.y) && void 0 !== lt ? lt : 0;
var Wn = null !== (ct = null === (ft = e.hw) || void 0 === ft ? void 0 : ft.z) && void 0 !== ct ? ct : 0, qn = {
uniqueId: 1,
dropList: d.p.slice()
};
e.hi.blc.gu(l.$uo.$vk.bj, ti, ei, Wn, qn, !1);
break;

case y.bO:
e.hi.blc.gu(l.$uo.$vk.bk);
break;

case y.bP:
e.hz && (e.hz.bjk = {
type: 1,
param: [ d.p.range, e.hw.x + (null !== (st = d.p.x) && void 0 !== st ? st : 0) * e.hw.bkw, e.hw.y + (null !== (pt = d.p.y) && void 0 !== pt ? pt : 0) ]
});
break;

case y.bQ:
if (!(mn = e.hi.blc.gi(null === (dt = e.hE) || void 0 === dt ? void 0 : dt.bhF))) break;
if (!mn.hv) break;
if (!(null === (vt = mn.hx) || void 0 === vt ? void 0 : vt.bjX)) break;
Jt = mn.hx;
var tr = e;
if (!Jt.bjN) {
tr.hz.biZ = 0;
tr.hz.bja = 0;
return;
}
Nt = d.p && d.p[0], Vt = .32;
Math.abs(Jt.bjO.x) > Vt ? tr.hz.biZ = Math.sign(Jt.bjO.x) * Nt : tr.hz.biZ = 0;
Math.abs(Jt.bjO.y) > Vt ? tr.hz.bja = Math.floor(Math.sign(Jt.bjO.y) * Nt * l.$uo.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y) : tr.hz.bja = 0;
break;

case y.bR:
e.hi.blc.gu(l.$uo.$vk.j);
return 0;

case y.bT:
jt = d.p;
if (!(ni = n.default.Eq(e, jt.find, null, i))) return 0;
fe = c.default.CW(jt.getVar, ni, i);
e.hE.bhA[jt.var] = fe;
return 0;

case y.bU:
if (0 === (jt = d.p).type) e.hE.bhP.type = 0; else {
var er = e.hE.bhP;
er.type = jt.type;
er.currentTime = 0;
er.speed = 1;
var ir = c.default.CW(jt.time, e, i);
if (e.hv) if (1 === jt.type) {
er.time = ir;
er.speed = e.hv.bfz(3);
} else if (2 === jt.type) {
e.hv.bfy > 0 && (ir = r.$jB.bma(ir * (Math.max(p.GameAttributeConstRatioRange - e.hv.bfy, 0) / p.GameAttributeConstRatioRange)));
er.time = ir;
er.speed = 1;
}
}
return 0;

case y.bW:
jt = d.p;
if (e.hx && e.hx.bjN) {
if (e.hx.bjO.y < -.5) {
n.default.EF(e, jt.state2);
return 1;
}
if (e.hx.bjO.y > .5) {
n.default.EF(e, jt.state1);
return 1;
}
}
if (!(qt = e.hi.bll(null === ($t = e.hG) || void 0 === $t ? void 0 : $t.bgj)) || !qt.hw) {
n.default.EF(e, jt.state1);
return 1;
}
var nr = (Ut = qt.hw).x - e.hw.x, rr = Ut.y - e.hw.y;
if (e.hw.bkw !== Math.sign(nr) || Math.abs(rr) > jt.yWidth) {
n.default.EF(e, jt.state1);
return 1;
}
(ri = Math.abs(nr)) < jt.disXMin && n.default.GG(qt) < jt.zMax ? n.default.EF(e, jt.state2) : n.default.EF(e, jt.state1);
return 0;

case y.cq:
e.hi.blc.gu(l.$uo.$vk.be, e, d.p);
return 0;

case y.cr:
e.hi.blc.gu(l.$uo.$vk.bf, e, d.p);
return 0;

case y.cs:
e.hi.blc.gu(l.$uo.$vk.bg, e, d.p);
return 0;

case y.ct:
e.hi.blc.gu(l.$uo.$vk.bh, d.p);
return 0;

case y.cu:
e.hi.blc.gu(l.$uo.$vk.bi, d.p[0], {
chatType: d.p[1],
chatData: d.p[2]
});
return 0;

case y._cv:
var or = "";
e.hE && (or = " b:{" + e.hE.bhB + "," + e.hE.bhC + "}");
p.default.bEC.bEE("logic log:" + or + " " + JSON.stringify(d.p));
return 0;
}
return 0;
};
t.CV = function(t, e, i) {
switch (i.t) {
case y.c:
i.p.id && t.hi.blc.gu(l.$uo.$vk.G, t, i.p.id);
break;

case y.U:
i.p.id && t.hi.blc.gu(l.$uo.$vk.bq, t, i.p.id);
break;

case y.bU:
t.hE.bhP.type = 0;
break;

case y.n:
t.hz && n.default.Fm(t.hz, 0, 0, 0);
void 0 !== i.p.gzs && (t.hE.bhW = 1);
break;

case y.Q:
t.hz && n.default.Fm(t.hz, 0, 0, 0);
break;

case y.t:
t.hi.blc.gu(l.$uo.$vk.M, t, null);
break;

case y.o:
i.p.id && n.default.FQ(t, i.p.id);
break;

case y.bL:
t.hq(v.default);
break;

case y.Y:
var r = i.p;
if (r.id) {
var o = t.hF.bgg.findIndex(function(t) {
return t.id === r.id;
});
-1 !== o && t.hF.bgg.splice(o, 1);
t.hF.bgh = t.hi.blc.fi;
}
}
};
return t;
}();
i.default = g;
cc._RF.pop();
}, {
"../../game/src/battle/$DSO": void 0,
"../../game/src/battle/types/$DYS": void 0,
"../../main/src/tbl/TblEnum": void 0,
"./$KI": "$KI",
"./$KK": "$KK",
"./$KL": "$KL",
"./component/$DOR": "$DOR",
"./component/$DOX": "$DOX",
"./component/$DPF": "$DPF",
"./component/$DPH": "$DPH",
"./component/$DPL": "$DPL",
"./component/$DPM": "$DPM",
"./component/$DPN": "$DPN",
"./math/$DPR": "$DPR",
"./math/$DPS": "$DPS"
} ]
}, {}, [ "$KI", "$KJ", "$KK", "$KL", "$KM", "$KN", "$KO", "$KP", "BattleBehaviorLogic", "$DOO", "$DOP", "$DOQ", "$DOR", "$DOS", "$DOT", "$DOU", "$DOV", "$DOW", "$DOX", "$DOY", "$DOZ", "$DPA", "$DPB", "$DPC", "$DPD", "$DPE", "$DPF", "$DPG", "$DPH", "$DPI", "$DPJ", "$DPK", "$DPL", "$DPM", "$DPN", "$DPO", "$DPP", "$DPQ", "$DPR", "$DPS", "$DPT", "$DPU", "$DPV", "$DPW", "$DPX", "$DPY", "$DPZ", "$DQA", "$DQB", "$DQC", "$DQD", "$DQE", "$DQF", "$DQG", "$DQH", "$DQI", "$DQJ" ]);