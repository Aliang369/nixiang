window.__require = function t(e, i, o) {
function n(s, a) {
if (!i[s]) {
if (!e[s]) {
var l = s.split("/");
l = l[l.length - 1];
if (!e[l]) {
var c = "function" == typeof __require && __require;
if (!a && c) return c(l, !0);
if (r) return r(l, !0);
throw new Error("Cannot find module '" + s + "'");
}
s = l;
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
$KH: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "0ead7OHxpRBh42JtoPK3BRM", "$KH");
var o = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$gu = i.$gt = i.$gs = i.$gr = i.$gq = void 0;
var r = t("../../bingo/src/$JX"), s = t("../../game/src/perf/$TF"), a = t("../../game/src/$KL"), l = t("../../game/src/perfab/common/$CCC"), c = t("../../game/src/utils/$TW"), h = t("../battleLogic/$KC"), b = t("../battleLogic/math/$NP"), u = t("../../game/src/battle/$QM"), p = t("../../game/src/battle/types/$WY"), f = function() {
function t() {}
Object.defineProperty(t.prototype, "mg", {
get: function() {
var t;
return null === (t = this.lZ) || void 0 === t ? void 0 : t.ng;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(t.prototype, "mh", {
get: function() {
var t;
return null === (t = this.lY) || void 0 === t ? void 0 : t.jO;
},
enumerable: !1,
configurable: !0
});
return t;
}();
i.$gq = f;
var d = function() {
function t() {
this.Hp = 0;
this.Hq = 0;
this.Hr = [];
this.Hs = [];
this.Ht = 0;
this.Hu = 0;
this.Hv = 0;
}
t.prototype.Hw = function(t) {
this.Hu = 0;
this.Ht = t;
};
return t;
}();
i.$gr = d;
i.$gs = function() {
this.Hy = [];
this.HB = 1;
this.HC = !0;
this.HF = 1;
this.HG = 1;
this.HH = 1;
this.HI = 1;
};
var v = function() {
function t() {
this.HZ = new Map();
this.Ia = !0;
this.Ib = new Map();
this.Ic = new Map();
this.Id = new Map();
this.Ig = null;
this.Ih = 0;
this.Ii = !1;
this.Ij = !1;
this.Ik = !1;
this.Il = null;
this.Im = null;
this.In = null;
this.Io = new Map();
this.Ip = null;
this.Iq = null;
this.Ir = null;
this.Is = {};
}
t.prototype.It = function() {
return this.HJ.nh.gi(this.HK);
};
t.prototype.Iu = function() {
return this.HJ.ng.gi(this.HK);
};
t.prototype.Iv = function() {
return this.HJ.mE ? this.Ij && !this.Ii ? Math.min(3.2 * this.HJ.ng.fp, this.HJ.nH() / 1e3) : Math.min(this.HJ.mN, .5) : Math.min(3.2 * this.HJ.ng.fp, this.HJ.nH() / 1e3);
};
t.prototype.Iw = function() {
var t, e = this.HJ.ng.gi(this.HK);
e && (this.Ii = -1 !== (null === (t = e.hE) || void 0 === t ? void 0 : t.bjS));
};
t.prototype.Ix = function() {};
t.prototype.Iy = function(t) {
if (t.HE) {
this.HJ.nb.bFX(t.HD, t.HE.node);
t.HE = null;
}
this.Ib.delete(t.Hx);
};
t.prototype.Iz = function() {
var t, e = this;
if (this.Ib.size > 0) {
this.Ib.forEach(function(t) {
e.Iy(t);
});
this.Ib.clear();
}
if (this.Ie) {
if (this.Ie.btW) {
this.Ie.btU ? this.HJ.nb.bFX(this.Ie.btW.resUrl, this.Ie.btW.node) : this.Ie.btW.node.destroy();
this.Ie.btW = null;
}
this.HJ.nb.bFX(r.$fb.ResUrl.EntityRender, this.Ie.node);
this.Ie.btV = null;
this.Ie = null;
}
this.IG();
this.IH();
this.II();
if (this.Id) {
this.Id.forEach(function(t) {
t.HE && t.HE.node.destroy();
});
this.Id.clear();
}
this.Is = null;
this.Ic.forEach(function(t) {
t.HE && t.HE.node.destroy();
});
if (null === (t = this.If) || void 0 === t ? void 0 : t.isValid) {
this.If.fLT(c.CC_GROUP_INDEX.UI);
$eZ.prefabMgr.cPD(a.default.commonComp.petModel, this.If.node);
}
this.If = null;
if (this.Iq) {
this.Iq.isValid && this.HJ.nb.bFX(r.$fb.ResUrl.BattleCastBar, this.Iq.node);
this.Iq = null;
}
};
t.prototype.IA = function() {
var t, e, i = null === (t = this.HM) || void 0 === t ? void 0 : t.topPos;
return null !== (e = i ? i[1] : 0) && void 0 !== e ? e : 0;
};
t.prototype.IB = function() {
var t, e, i = null === (t = this.HM) || void 0 === t ? void 0 : t.topPos;
return (null !== (e = i ? i[1] : 0) && void 0 !== e ? e : 0) + p.$uD.TOWN_SCENE_DEFINE.TITLE_TOP_OFFSET;
};
t.prototype.IC = function() {
return this.IA() + 80;
};
t.prototype.ID = function() {
return o(this, void 0, void 0, function() {
var t;
return n(this,function AWAITF1(e){switch (e.label) {
case 0:
return [ 4, $eZ.prefabMgr.cPC(a.default.commonComp.petModel) ];

case 1:
if (!(t = e.sent())) return [ 2 ];
if (!this.Ih) return [ 2 ];
if (this.If) return [ 2 ];
t.scale = 1;
this.HJ.ne._in.sceneLayer.addChild(t, p.$uD.TOWN_SCENE_DEFINE.ROLE_ZORDER);
this.If = t.getComponent(l.$Ql);
this.If.fLT(c.CC_GROUP_INDEX.BATTLE);
this.If.fLU(this.Ih);
return [ 2 ];
}
});
});
};
t.prototype.IE = function(t) {
var e, i, o;
if (this.If) {
this.If.fLL !== this.Ih && this.If.fLU(this.Ih);
var n = this.If.fLM;
if (n) {
var r = n.pet, a = this.If.node, c = this.HQ + this.HT * (null !== (e = r.x) && void 0 !== e ? e : -150), h = this.HR + (null !== (i = r.y) && void 0 !== i ? i : 0), b = null !== (o = r.z) && void 0 !== o ? o : 0, f = .15;
if (!this.Ig) {
this.Ig = {
b: c,
c: h,
d: b,
e: this.HT,
f: 0
};
f = 1;
}
if (this.Ig.f) {
this.Ig.f = 0;
f = 1;
this.Ig.b = c;
this.Ig.c = h;
this.Ig.d = b;
}
var d = c - this.Ig.b, v = h - this.Ig.c, y = Math.abs(d), g = Math.abs(v), m = 0;
if (y > 30) {
var _ = Math.min(5 * y, 2e3);
m = Math.sign(d) * _;
}
var $ = 0;
g > 50 && ($ = (_ = Math.min(5 * g, 2e3)) * Math.sign(v));
var P = 0;
if (m) {
P = t * m;
Math.abs(P) > y ? this.Ig.b = c : this.Ig.b += P;
}
if ($) {
P = t * $;
Math.abs(P) > g ? this.Ig.c = h : this.Ig.c += P;
}
var w = Math.abs(Math.abs(m)) + Math.abs($) > 200;
w && Math.abs(m) > 350 ? this.Ig.e = m > 0 ? p.$uD.$vj.b : p.$uD.$vj.c : this.Ig.e !== this.HT && (this.Ig.e = this.HT);
this.If.fMa(w ? l.$Qn.d : l.$Qn.c);
this.If.fMh(this.Ig.e);
a.x = this.IF(a.x, this.Ig.b, f);
a.y = this.IF(a.y, u.default.bGR.bHJ(this.Ig.c, this.Ig.d), f);
s.HeatIsolate.cyX || (a.zIndex = u.default.bGR.bHU(this.Ig.c));
}
} else this.ID();
};
t.prototype.IF = function(t, e, i) {
if (1 === i) return e;
var o = e - t;
return Math.abs(o) < 4 ? e : t + o * i;
};
t.prototype.IG = function() {
if (this.Il) {
this.Il.isValid && this.HJ.nb.bFX(r.$fb.ResUrl.EntityShadow, this.Il.node);
this.Il = null;
}
};
t.prototype.IH = function() {
if (this.Im) {
this.Im.isValid && $eZ.prefabMgr.cPE(this.Im.node);
this.Im = null;
}
};
t.prototype.II = function() {
if (this.In) {
if (this.In.isValid) {
this.In.bwd();
this.HJ.nb.bFX(r.$fb.ResUrl.RoleTopContent, this.In.node);
}
this.In = null;
}
if (this.Ip) {
this.Ip.isValid && this.HJ.nb.bFX(r.$fb.ResUrl.RoleTopProtectProgress, this.Ip.node);
this.Ip = null;
}
};
t.prototype.IJ = function(t) {
this.HU = b.$jG.bof(t);
this.HV = t * Math.PI / 180;
this.HX = h.default.Gu(t) ? 1 : -1;
};
t.prototype.IK = function(t, e) {
this.HU = b.$jG.bof(t);
this.HV = t * Math.PI / 180;
this.HX = e;
};
t.prototype.IL = function() {
if (this.Ie) {
var t = this.Ie.node, e = this.Iu();
if (null == e ? void 0 : e.hw) {
this.HQ = e.hw.x;
this.HR = e.hw.y;
this.HS = e.hw.z;
t.x = this.HQ;
t.y = u.default.bGR.bHJ(this.HR, this.HS);
}
}
};
return t;
}();
i.$gt = v;
var y = function() {
function t() {
this.IU = !0;
this.IV = !1;
}
t.prototype.Ja = function() {};
t.prototype.Jb = function() {
if (this.IY) {
this.IY.isValid && this.IM.nb.bFX(r.$fb.ResUrl.DropItemRender, this.IY.node);
this.IY = null;
}
if (this.IZ) {
this.IZ.isValid && this.IM.nb.bFX(r.$fb.ResUrl.DropItemText, this.IZ.node);
this.IZ = null;
}
};
t.prototype.Jc = function() {};
t.prototype.Jd = function() {
var t, e;
if (null === (t = this.IY) || void 0 === t ? void 0 : t.isValid) {
var i = this.IY.node;
i.x = this.IO;
i.y = u.default.bGR.bHJ(this.IP, this.IQ);
if (null === (e = this.IZ) || void 0 === e ? void 0 : e.isValid) {
var o = this.IZ.node;
o.x = this.IO;
o.y = u.default.bGR.bHJ(this.IP, this.IQ) - 40;
}
}
};
return t;
}();
i.$gu = y;
i.default = function() {
this.Je = null;
this.Jf = 0;
this.Jg = 0;
this.Jh = cc.v2();
this.Ji = 0;
this.Jj = cc.color();
this.Jk = 255;
this.Jl = 660;
this.Jm = 0;
this.Jn = 0;
this.Jo = 0;
this.Jp = 0;
this.Jq = 0;
this.Jr = .05;
this.Js = .12;
};
cc._RF.pop();
}, {
"../../bingo/src/$JX": void 0,
"../../game/src/$KL": void 0,
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../game/src/perf/$TF": void 0,
"../../game/src/perfab/common/$CCC": void 0,
"../../game/src/utils/$TW": void 0,
"../battleLogic/$KC": void 0,
"../battleLogic/math/$NP": void 0
} ],
$KI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5ff5aK2+8JOwpmTKQaiMa7V", "$KI");
var o = this && this.__awaiter || function(t, e, i, o) {
return new (i || (i = Promise))(function(n, r) {
function s(t) {
try {
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$gw = void 0;
var r = t("../../game/src/$KK"), s = t("../../bingo/src/$JX"), a = (t("../battleLogic/$KC"), 
t("../battleLogic/component/$NJ")), l = t("../../game/src/battle/types/$WW"), c = t("../../game/src/battle/types/$WY"), h = t("./compnents/$OI"), b = t("./$KH"), u = t("./compnents/$ON"), p = t("./compnents/$OO"), f = t("../../game/src/battle/$QK"), d = t("./ui/$PT"), v = t("../../bingo/src/utils/$ML"), y = t("../../game/src/battle/$QM"), g = t("../../game/src/battle/$QA"), m = t("./compnents/$OM"), _ = t("./compnents/$OJ"), $ = t("../../game/src/utils/$TW"), P = t("./render/$PB"), w = t("./render/$PA"), I = t("./render/$PE"), O = t("./render/$PI"), E = t("../../main/src/$KM"), x = t("../../game/src/manager/$SN"), S = t("./compnents/$OL"), M = t("../../game/src/manager/role/$CAD"), C = t("../../game/src/perf/$TF"), T = function() {
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
this.iE = cc.color();
this.iF = 0;
this.iS = "";
this.je = 1;
this.jG = [];
this.jU = function(t) {
e.ip.nb.bFX(s.$fb.ResUrl.fightText, t);
};
this.ip = t;
this.iD = new b.default();
}
t.prototype.iG = function() {
var t = this;
$eZ.adMgr.Vo();
if ($eZ.adMgr.TD() && !$eZ.platform.cpN.cpB.isReportScene) {
var e = $eZ.adMgr.Tn();
if (e && e.enter_scene && -1 != e.enter_scene.indexOf("3041") && "2" == e.feed_game_channel) {
$eZ.platform.cpN.cpB.isReportScene = !0;
var i = Math.max(Math.ceil(Date.now() - s.$fb.startTimer), 1e3);
$eZ.adMgr.Ws(i);
}
}
var o = cc.find("Canvas/GameRoot"), n = o.getChildByName("battleScene");
if (!n) {
var a = this.ip.nb.bFB(s.$fb.ResUrl.battleScene);
if (!a) return;
n = cc.instantiate(a);
o.addChild(n, -1, "battleScene");
}
this._in || (this._in = n.getComponent(f.default));
this.iS = "";
this.iT();
this.iu = this.ip.mB;
this.ip.ng.fF.forEach(function(e) {
var i = new b.$gr();
i.Hn = e.dca;
i.Ho = e.dco;
t.is.set(e.dca, i);
});
this.it.x = cc.winSize.width;
this.it.y = cc.winSize.height;
this.jI();
this._in.bGc();
this._in.bGd();
this.jP();
this.jK();
$eZ.localMgr.cmD(x.$oI.e);
this.iv = this.ip.mz;
this.io.push(new h.$kc());
this.io.push(new u.$kj());
this.io.push(new p.$kk());
$fK.ClassCollisionDebugDrawHelper && this.io.push(new $fK.ClassCollisionDebugDrawHelper());
switch (this.ip.ng.fM.f) {
case l.$uy.b:
this.io.push(new S.$kh());
this.io.push(new m.$ki());
this.io.push(new _.$kf());
break;

case l.$uy.c:
case l.$uy.f:
case l.$uy.g:
this.io.push(new _.$kf());
}
this.io.forEach(function(e) {
if (!e.lY) {
e.lZ = r.default.JE;
e.lY = t;
e.ma();
}
});
this.ip.nk.bBd(c.$uD.$vA.x, this.jb, this);
this.ip.nk.bBd(c.$uD.$vA.B, this.jc, this);
this.ip.nk.bBd(c.$uD.$vA.C, this.ja, this);
this.ip.nk.bBd(c.$uD.$vA.D, this.jg, this);
this.ip.nk.bBd(c.$uD.$vA.K, this.jh, this);
this.ip.nk.bBd(c.$uD.$vA.L, this.ji, this);
this.ip.nk.bBd(c.$uD.$vA.H, this.jn, this);
this.ip.nk.bBd(c.$uD.$vA.I, this.jo, this);
this.ip.nk.bBd(c.$uD.$vA.z, this.jx, this);
this.ip.nk.bBd(c.$uD.$vA.J, this.jj, this);
this.ip.nk.bBd(c.$uD.$vA.T, this.jl, this);
this.ip.nk.bBd(c.$uD.$vA.E, this.jk, this);
this.ip.nk.bBd(c.$uD.$vA.V, this.jp, this);
this.ip.nk.bBd(c.$uD.$vA.U, this.jm, this);
this.ip.nk.bBd(c.$uD.$vA.f, this.jq, this);
this.ip.nk.bBd(c.$uD.$vA.be, this.jr, this);
this.ip.nk.bBd(c.$uD.$vA.bf, this.js, this);
this.ip.nk.bBd(c.$uD.$vA.bg, this.jt, this);
this.ip.nk.bBd(c.$uD.$vA.bh, this.ju, this);
this.ip.nk.bBd(c.$uD.$vA.X, this.jw, this);
this.ip.nk.bBd(c.$uD.$vA.i, this.jy, this);
this.ip.nk.bBd(c.$uD.$vA.m, this.jz, this);
this.ip.nk.bBd(c.$uD.$vA.n, this.jA, this);
this.ip.nk.bBd(c.$uD.$vA.Z, this.jB, this);
this.ip.nk.bBd(c.$uD.$vA.ba, this.jC, this);
this.ip.nk.bBd(c.$uD.$vA.bi, this.jD, this);
this.ip.nk.bBd(c.$uD.$vA.p, this.jF, this);
this.ip.nk.bBd(c.$uD.$vA.bn, this.jE, this);
this.ip.nk.bBd(c.$uD.$vA.br, this.ka, this);
this.ip.nk.bBd(c.$uD.$vA.s, this.jZ, this);
$eZ.role.kn(M.$Oi.$On.c, this.jY, this);
};
t.prototype.iH = function() {
var t, e;
this.iF++;
this.io.forEach(function(t) {
if (t.lY) {
t.mb();
t.lY = null;
}
});
this.io.length = 0;
this.iq.clear();
this.ir.length = 0;
this.is.clear();
this.jO = null;
null === (t = this._in) || void 0 === t || t.bGa();
null === (e = this._in) || void 0 === e || e.bGe();
this.iC.clear();
this.ip.nk.bBf(this);
this.ip.nl.bBf(this);
$eZ.battleServer.kp(this);
$eZ.role.kp(this);
this.iB = !1;
$eZ.role.fvg(!1);
};
t.prototype.iI = function(t) {
this.io.forEach(function(e) {
return e.md(t);
});
};
t.prototype.iJ = function(t) {
var e = this;
this.iq.forEach(function(t, i) {
e.ip.ng.gi(i) || e.iQ(t);
});
this.io.forEach(function(e) {
return e.me(t);
});
};
t.prototype.iK = function() {
this.io.forEach(function(t) {
return t.mf();
});
this.iL();
this.jI();
this.iT();
this.iU();
};
t.prototype.iL = function() {
var t = this;
this.is.forEach(function(e) {
var i = t.ip.ng.gk(e.Hn);
i && (e.Ho = i.dco);
});
};
t.prototype.iM = function(t) {
var e = this, i = this.ip.ng;
if (i) {
this.iq.forEach(function(t, o) {
i.gi(o) || e.iQ(t);
});
this.io.forEach(function(e) {
return e.mc(t);
});
this.iT();
this.jO || this.jP();
this.jN(t);
}
};
t.prototype.iN = function(t) {
var e = this.ip.ng.gi(t);
return e ? this.iO(e) : null;
};
t.prototype.iO = function(t) {
var e;
if (t.hF) {
var i = t.hg, o = t.hF.bik, n = this.iq.get(i);
if (n && n.HL !== o) if (n.Ij) {
var r = n.Iu();
if (r) {
o = null === (e = (t = r).hF) || void 0 === e ? void 0 : e.bik;
if (n.HL !== o) {
this.iQ(n);
n = null;
}
}
} else {
this.iQ(n);
n = null;
}
if (!n) {
var s = t, a = !1;
n = new b.$gt();
if (t.hx && t.hx.blT) {
a = t.hx.blT === this.ip.mB;
n.Ij = !a && this.ip.mz;
}
n.Ik = a;
n.HJ = this.ip;
n.HK = i;
n.HL = t.hF.bik;
n.HM = t.hF.bim;
n.Iw();
n.Ij && (s = n.Iu() || s);
var l = s.hw;
n.HQ = l.x;
n.HR = l.y;
n.HS = l.z;
n.HT = l.bmF;
n.HU = l.bmG;
n.HY = l.bmH;
this.iq.set(i, n);
}
return n;
}
};
t.prototype.iP = function(t) {
return this.iq.get(t);
};
t.prototype.iQ = function(t) {
t.Iz();
this.iq.delete(t.HK);
};
t.prototype.iR = function() {
if (0 !== this.ir.length) {
for (var t = 0, e = this.ir; t < e.length; t++) e[t].Jb();
this.ir.length = 0;
}
};
t.prototype.iT = function() {
var t = this.ip.ng.gt(a.default).bmp;
if (this.iS !== t) {
var e = this.ip.ng.gt(a.default).bmA;
if (e) {
var i = r.default.JE.nb.bFB(e.resUrl);
if (i) {
this.iS = t;
var o = this._in.sceneLayer.getChildByName("mapNode");
o && o.destroy();
this.iR();
var n = cc.instantiate(i);
this._in.sceneLayer.addChild(n, 0, "mapNode");
var s = this._in.mainCamera;
s.depth = 30;
s.node.active = !0;
s.backgroundColor = cc.Color.TRANSPARENT;
s.node.setPosition(0, 0);
for (var l = s.node.parent, c = l.children, h = 1; h < c.length; h++) c[h].destroy();
this.iz = [ {
camera: s,
motion: 1,
depth: 30,
group: 1
} ];
for (var b = 0, u = n.children; b < u.length; b++) {
var p = u[b], f = p.name.split("_");
if ("layer" === f[0]) {
var d = Number(f[1]), v = Number(f[2] || 0);
if (0 == d) p.groupIndex = 1; else {
var y = this.iz.length + 1;
p.groupIndex = y;
p.x = .5 * this.it.x * (v - 1);
var g = cc.instantiate(s.node);
l.addChild(g);
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
this.iU();
}
}
}
};
t.prototype.iU = function() {
var t, e = null === (t = this.ip.ng) || void 0 === t ? void 0 : t.gt(a.default);
if (e && 0 !== e.bmz.length) {
var i = this._in.sceneLayer.getChildByName("mapNode");
if (i) for (var o = 0, n = i.children; o < n.length; o++) {
var r = n[o], s = r.name.split("_");
if ("layer" === s[0]) {
var l = Number(s[1]);
0 !== l && (-1 !== e.bmz.indexOf(l) ? r.active = !0 : r.active = !1);
}
}
}
};
t.prototype.iV = function(t) {
return t.hx && t.hx.blT === this.iu;
};
t.prototype.iW = function(t) {
var e, i, o, n = null === (e = t.hx) || void 0 === e ? void 0 : e.blT;
if (!n) {
var r = this.ip.ng.gi(null === (i = t.hE) || void 0 === i ? void 0 : i.bjO);
r && (n = null === (o = r.hx) || void 0 === o ? void 0 : o.blT);
}
return n === this.iu;
};
t.prototype.iX = function(t) {
var e, i, o, n, r, a = t;
(null === (e = a.hx) || void 0 === e ? void 0 : e.blT) || (a = this.ip.ng.gi(null === (i = t.hE) || void 0 === i ? void 0 : i.bjO));
var l = null === (o = null == a ? void 0 : a.hx) || void 0 === o ? void 0 : o.blT;
if (!l) return !1;
if (l === this.iu) return !1;
var c = null === (n = this.ip.ny()) || void 0 === n ? void 0 : n.dcf;
return !!s.$fb.utils.bet(c) && (null === (r = a.hD) || void 0 === r ? void 0 : r.bkR) === c;
};
t.prototype.iY = function() {
return this.ip.ng.gi(this.iu);
};
t.prototype.iZ = function() {
this.jO && this.jO.bzn({
type: 0
});
};
t.prototype.ja = function() {
this.ip.mV || this.ip.mW;
};
t.prototype.jb = function(t) {
var e, i;
if (!this.ip.mV && !this.ip.mW) {
var o = null === (i = null === (e = t.hv) || void 0 === e ? void 0 : e.bfZ) || void 0 === i ? void 0 : i.deadSound;
o && this.ip.nc.bAV(o[Math.floor(o.length * Math.random())]);
var n = this.iP(t.hg);
(null == n ? void 0 : n.Ie) && n.Ie.buX();
}
};
t.prototype.jc = function(t) {
this.ip.mV || this.ip.mW || this.jf(t);
};
t.prototype.jd = function() {
this.ip.mV;
};
t.prototype.jf = function(t) {
var e, i, o, n, s, a, l, h, b;
if (!C.HeatIsolate.cyV) {
var u = t.fromEntity, p = t.atkEntity, f = t.target, d = t.finalDamage;
if (u.hw) {
var v = $eZ.localMgr.cmD(x.$oI.e), g = t.hitInfo, m = null === (e = t.hitInfo) || void 0 === e ? void 0 : e.vfx, _ = Date.now();
if (f && (!(I = this.iC.get(f.hg)) || _ - I >= 1e3 * this.je)) {
this.iC.set(f.hg, _);
var $ = null === (o = null === (i = f.hv) || void 0 === i ? void 0 : i.bfZ) || void 0 === o ? void 0 : o.hitSound;
$ && this.ip.nc.bAV($[Math.floor($.length * Math.random())]);
}
var P = t.hitInfo.audio;
if (P) {
var w = null !== (n = P.minInterval) && void 0 !== n ? n : .03;
if (u) {
var I;
if (!(I = this.iC.get(u.hg)) || _ - I >= 1e3 * w) {
this.iC.set(u.hg, _);
var O = P.name;
if (Array.isArray(O)) {
var S = O[Math.floor(O.length * Math.random())];
S && this.ip.nc.bAV(S);
} else this.ip.nc.bAV(O);
}
}
}
if (g.shakeCamera) {
var M = this.is.get(this.iu);
if (M && ((null === (s = null == f ? void 0 : f.hx) || void 0 === s ? void 0 : s.blT) === this.iu || (null === (a = null == p ? void 0 : p.hx) || void 0 === a ? void 0 : a.blT) === this.iu)) {
M.Hw(g.shakeCamera);
r.default.JE.nD(g.shakeCamera);
}
}
var T = null !== (h = null === (l = t.fromEntity.hw) || void 0 === l ? void 0 : l.bmF) && void 0 !== h ? h : 1;
if (null == m ? void 0 : m.id) {
var R = null !== (b = m.angle) && void 0 !== b ? b : 0;
m.angleRand && (R += m.angleRand * Math.random());
var k = m.id;
m.id2 > 0 && Math.random() > .5 && (k = m.id2);
this.jW(k, t.hitX, t.hitY - 4, t.hitZ, T, R);
}
t.elementValue & c.$uD.$uY.b && this.jW(E.cvResUrl.vfx_fire, t.hitX, t.hitY - 4, t.hitZ, T, 0);
t.elementValue & c.$uD.$uY.c && this.jW(E.cvResUrl.vfx_ice, t.hitX, t.hitY - 4, t.hitZ, T, 0);
t.elementValue & c.$uD.$uY.d && this.jW(E.cvResUrl.vfx_light, t.hitX, t.hitY - 4, t.hitZ, T, 0);
t.elementValue & c.$uD.$uY.e && this.jW(E.cvResUrl.vfx_dark, t.hitX, t.hitY - 4, t.hitZ, T, 0);
v && d && this.jQ(t.hitX, y.default.bGR.bHJ(t.hitY, t.hitZ), d + "", t.isCrt);
}
}
};
t.prototype.jg = function(t, e, i, o) {
o.hw;
};
t.prototype.jh = function() {
this.ip.mV || this.ip.mW;
};
t.prototype.ji = function() {
this.ip.mV || this.ip.mW;
};
t.prototype.jj = function(t, e) {
if (!this.ip.mV && !this.ip.mW) {
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
this.jW(8, o.x, o.y, i, 0, 1);
}
};
t.prototype.jk = function(t, e) {
var i;
if (!this.ip.mV && $eZ.localMgr.cmD(x.$oI.e)) {
var o = this.ip.ng.gi(t.hg);
if (o) {
var n = this.iP(o.hg);
if (n && n.Ie) {
var r = null === (i = n.HM) || void 0 === i ? void 0 : i.topPos, s = r ? r[1] : 0;
e >= 0 ? this.jV(n.Ie.node.x, n.Ie.node.y + s, Math.abs(e) + "", 4) : this.jV(n.Ie.node.x, n.Ie.node.y + s, Math.abs(e) + "", 3);
}
}
}
};
t.prototype.jl = function(t, e) {
this._in && this.jO.bzm(e.time, e.url, e.x, e.y, e.angle, e.sx, e.sy);
};
t.prototype.jm = function(t) {
if (this._in) {
var e = this.ip.nb.bFB(t[0]);
e && this.jO.byY(e, t[1], t[2]);
}
};
t.prototype.jn = function(t, e, i) {
return o(this, void 0, void 0, function() {
var o, r, s;
return n(this,function AWAITF2(n){switch (n.label) {
case 0:
if (this.ip.mW) return [ 2 ];
o = 0;
if (!Array.isArray(e)) return [ 3, 2 ];
r = e[Math.floor(e.length * Math.random())];
return [ 4, this.ip.nc.bAV(r) ];

case 1:
o = n.sent();
return [ 3, 4 ];

case 2:
return [ 4, this.ip.nc.bAV(e) ];

case 3:
o = n.sent();
n.label = 4;

case 4:
if (!i) return [ 2 ];
if (!o) return [ 2 ];
if (!(s = this.iP(t.hg))) return [ 2 ];
s.Is[i] = o;
return [ 2 ];
}
});
});
};
t.prototype.jo = function(t, e) {
return o(this, void 0, void 0, function() {
var i, o;
return n(this,function AWAITF3(){if (!e) return [ 2 ];
i = this.iP(t.hg);
if (!(o = null == i ? void 0 : i.Is[e])) return [ 2 ];
$eZ.audioMgr.cKC(o);
return [ 2 ];
});
});
};
t.prototype.jp = function(t) {
if (3 === t[1]) this.ip.nc.bAX(); else if (1 === t[1]) this.ip.nc.bAW(t[0]); else {
if (!t[0]) return 0;
this.ip.nc.bAV(t[0]);
}
};
t.prototype.jq = function() {};
t.prototype.jr = function(t, e) {
var i = e[2];
this.jO.bzj(e[0], e[1], i);
};
t.prototype.js = function(t, e) {
var i = this.jO, o = e;
if (e[2]) {
o = v.default.bez(e[2]);
if (Array.isArray(o)) for (var n = t.hH.bjv, r = 0; r < o.length; r++) {
var s = o[r];
"string" == typeof s && (o[r] = s.replace(/#{([A-Za-z]+)}/, function(t, e) {
return n[e];
}));
}
}
i.bzk(e[0], e[1], o);
};
t.prototype.jt = function(t, e) {
var i;
null === (i = this.jO) || void 0 === i || i.bzl(e[0]);
};
t.prototype.ju = function(t) {
var e;
null === (e = this.jO) || void 0 === e || e.bzh(t[0], t[1]);
};
t.prototype.jv = function(t) {
var e;
null === (e = this.jO) || void 0 === e || e.bzv(t);
};
t.prototype.jw = function(t, e) {
var i;
e.isOnlySelf && !this.iV(t) || null === (i = this.jO) || void 0 === i || i.bzn(e);
};
t.prototype.jx = function(t) {
this.ip.mW || t.hx && this.iV(t) && this.jO.bzc(t.hv.bgy);
};
t.prototype.jy = function() {
this.jX();
$eZ.battleMgr.bDv.bDs() && this.jO.bAa();
};
t.prototype.jz = function() {};
t.prototype.jA = function() {
this.iT();
this.jI();
this.iq.forEach(function(t) {
var e;
(null === (e = t.Ie) || void 0 === e ? void 0 : e.isValid) && t.IL();
});
this.iZ(0);
this.jX();
};
t.prototype.jB = function() {
this.iU();
};
t.prototype.jC = function(t, e) {
var i = this.jO;
i.isValid && i.bzx(t.hg + "_" + e[0], e[2], e[3]);
};
t.prototype.jD = function(t, e) {
var i, o = this.jO;
if (o.isValid) {
var n = 0;
if (null === (i = this.ip.ng) || void 0 === i ? void 0 : i.gN) {
n = 1 === this.ip.mC ? 0 : 1;
n = t ? n : 0 === n ? 1 : 0;
}
o.bzE(n, e);
}
};
t.prototype.jE = function(t, e) {
var i, o, n, r, s, a, l, c, h, b, u, p = this.iP(t.hg);
if (p) {
var f = null === (i = p.Ie) || void 0 === i ? void 0 : i.mainNode;
if (f) {
var d = p.Ie.node, v = this.iD;
v.Jj.r = null !== (o = e.r) && void 0 !== o ? o : 255;
v.Jj.g = null !== (n = e.g) && void 0 !== n ? n : 255;
v.Jj.b = null !== (r = e.b) && void 0 !== r ? r : 255;
v.Jk = null !== (s = e.a) && void 0 !== s ? s : 200;
v.Jl = null !== (a = e.size) && void 0 !== a ? a : 660;
v.Jn = 280;
v.Jo = null !== (l = e.shaderType) && void 0 !== l ? l : 0;
v.Jp = null !== (c = e.blend) && void 0 !== c ? c : 0;
v.Jq = null !== (h = e.offsetZIndex) && void 0 !== h ? h : 0;
v.Jr = null !== (b = e.t1) && void 0 !== b ? b : .05;
v.Js = null !== (u = e.t2) && void 0 !== u ? u : .12;
this.jM(f, d.x, d.y, d.zIndex, this.iD);
}
}
};
t.prototype.jF = function() {
if (!this.ip.mV && !this.ip.mW) {
var t = this.ip.ng.fI;
if (t) {
var e = t.ddc[t.ddc.length - 1], i = this.ip.ng.gi(e);
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
i.hi.bnl.gu(c.$uD.$vA.F, i, o);
}
}
}
};
t.prototype.jH = function() {};
t.prototype.jI = function() {
var t, e = null === (t = this.ip.ng) || void 0 === t ? void 0 : t.fT;
e && this.is.forEach(function(t) {
var i = {
b: 0,
c: 0,
d: 0,
e: 0,
f: e.type,
g: t.Ho,
h: e.fixedZoomRatio,
i: e.cameraLerpRatio
};
t.Hr = [ i ];
});
};
t.prototype.jJ = function() {};
t.prototype.jK = function() {
this._in.uiLayer.opacity = 255;
0 !== this.iA && (this.iA = 0);
};
t.prototype.jL = function(t) {
var e = this.is.get(this.iu);
if (e) {
e.Hw(t);
r.default.JE.nD(t);
}
};
t.prototype.jM = function(t, e, i, o, n) {
var r, a = this;
if (cc.isValid(t)) {
var l = null === (r = this._in) || void 0 === r ? void 0 : r.sceneLayer;
if (cc.isValid(l)) {
var c = this.ip.nb.bFN(s.$fb.ResUrl.ghost);
if (c) {
var h = n.Jm, b = n.Jn, u = n.Jl, p = n.Jk, d = n.Jo, v = n.Jj.r, y = n.Jj.g, g = n.Jj.b, m = n.Jp, _ = n.Jr, w = n.Js, I = this.iF;
c.opacity = 0;
c.groupIndex = $.CC_GROUP_INDEX.BATTLE;
c.x = e + h;
c.y = i + b;
c.parent = l;
c.zIndex = o + n.Jq;
c.setScale(1.6);
var O = c.getComponent(P.default), E = this.iE;
E.r = v;
E.g = y;
E.b = g;
E.a = 255;
O.bvm(d, E);
O.bvn(m);
f.default.bGo(function() {
if (I === a.iF && cc.isValid(c)) if (cc.isValid(t) && t.activeInHierarchy) {
var e = f.default.bGt(t, u, h, b, 1.6);
if (e) {
O.bvl(e);
c.opacity = p;
cc.Tween.stopAllByTarget(c);
cc.tween(c).delay(_).to(w, {
opacity: 0
}).call(function() {
a.ip.nb.bFX(s.$fb.ResUrl.ghost, c);
}).start();
} else a.ip.nb.bFX(s.$fb.ResUrl.ghost, c);
} else a.ip.nb.bFX(s.$fb.ResUrl.ghost, c);
});
}
}
}
};
t.prototype.jN = function() {
this._in.bFY || this._in.bGd();
var t = this.ip.ng.gt(a.default).bmy.length > 0;
this._in.bGg(t);
};
t.prototype.jP = function() {
if (!this.jO) {
var t = r.default.JE.nb.bFB(s.$fb.ResUrl.BattleUI);
if (t) {
var e = cc.instantiate(t), i = $eZ.fguiMgr.cSx();
this._in.uiLayer.setContentSize(i.width, i.height);
this._in.uiLayer.setPosition(this._in.uiLayer.parent.convertToNodeSpaceAR(cc.Canvas.instance.node.convertToWorldSpaceAR(cc.v2())));
this._in.uiLayer.destroyAllChildren();
this._in.uiLayer.addChild(e);
this.jO = e.getComponent(d.default);
}
}
};
t.prototype.jQ = function(t, e, i, o) {
this.jV(t, e, i, o ? 2 : 1);
};
t.prototype.jT = function(e, i) {
var o, n = null === (o = this._in) || void 0 === o ? void 0 : o.mainCamera;
if (!n || !n.isValid) return !0;
var r = n.zoomRatio || 1, s = .5 + t.jR, a = this.it.x / r * s, l = this.it.y / r * s, c = n.node;
return Math.abs(e - c.x) <= a && Math.abs(i - c.y) <= l;
};
t.prototype.jV = function(t, e, i, o) {
if (this.jT(t, e)) {
var n = this.ip.nb.bFN(s.$fb.ResUrl.fightText);
if (n) {
n.zIndex = c.$uD.TOWN_SCENE_DEFINE.FIGHT_TEXT;
n.setPosition(t, e);
n.getComponent(w.default).bvj(i, this.jU, o, 0, 1);
n.parent = this._in.sceneLayer;
}
}
};
t.prototype.jW = function(t, e, i, o, n, r, s) {
var a;
void 0 === s && (s = c.$uD.TOWN_SCENE_DEFINE.ROLE_ZORDER);
var l = null === (a = this._in) || void 0 === a ? void 0 : a.sceneLayer;
if (l) {
var h = this.ip.nb, b = y.default.bGR.bHS(t);
if (b) {
var u, p = r, f = e + (b.posX || 0) * n, d = y.default.bGR.bHJ(i, o) + (b.posY || 0), v = (b.scaleX || 1) * n, m = b.scaleY || 1, _ = s;
_ === c.$uD.TOWN_SCENE_DEFINE.ROLE_ZORDER && (_ = y.default.bGR.bHU(i));
var $ = y.default.bGR.bHV(b.resType) || b.resUrl;
if (!(u = h.bFN($))) return this;
l.addChild(u, _);
u.setPosition(f, d);
switch (b.resType) {
case g.DISPLAY_RES_TYPE.MOVIE_CLIP:
u.angle = p * n;
u.scaleX = v;
u.scaleY = m;
var P = u.getComponent(I.default);
P.setTimeScale(1);
P.bvW(b.movieClipId, !1, function() {
h.bFX($, u);
});
break;

case g.DISPLAY_RES_TYPE.SPINE:
u.angle = p * n;
u.scaleX = v;
u.scaleY = m;
var w = u.getComponent(O.$kL);
w.setTimeScale(1);
w.mm(Boolean(b.premultAlpha));
w.mo(b.resUrl, b.animName, !1, function() {
h.bFX($, u);
});
break;

case g.DISPLAY_RES_TYPE.PREFAB:
u.angle = p;
u.scaleX = v;
u.scaleY = m;
var E = u.getComponent(g.IVFXItem);
E.prefabPlay && E.prefabPlay(function() {
h.bFX($, u);
});
}
return this;
}
}
};
t.prototype.jX = function() {
var t, e, i, o = null === (e = null === (t = this.ip) || void 0 === t ? void 0 : t.ng) || void 0 === e ? void 0 : e.fI;
if (o) {
this.ip.ng.fI;
var n = null === (i = o.dcL) || void 0 === i ? void 0 : i.BGM;
n ? this.ip.nc.bAW(n) : this.ip.nc.bAW("");
}
};
t.prototype.jY = function() {
if (this.ip && this.iV(this.ip.nz) && $eZ.role.fuO) if (this.ip.ng.fN) this.iB = !0; else {
var t = $eZ.role.fuO.fuD.level;
this.ip.nd.bCv(t);
}
};
t.prototype.jZ = function() {
$eZ.role.fvg(!1);
if (this.iB) {
this.iB = !1;
var t = $eZ.role.fuO.fuD.level;
this.ip.nd.bCv(t);
}
};
t.prototype.ka = function() {
$eZ.role.fvg(!0);
};
t.prototype.kb = function(t) {
if (this.iu !== t) {
this.ip.mB = t;
this.iu = t;
this.ip.ng.gu(c.$uD.$vA.o);
}
};
t.jR = .1;
t.jS = 18;
return t;
}();
i.$gw = T;
window.$fK = window.$fK || {};
$fK.$gx = T;
cc._RF.pop();
}, {
"../../bingo/src/$JX": void 0,
"../../bingo/src/utils/$ML": void 0,
"../../game/src/$KK": void 0,
"../../game/src/battle/$QA": void 0,
"../../game/src/battle/$QK": void 0,
"../../game/src/battle/$QM": void 0,
"../../game/src/battle/types/$WW": void 0,
"../../game/src/battle/types/$WY": void 0,
"../../game/src/manager/$SN": void 0,
"../../game/src/manager/role/$CAD": void 0,
"../../game/src/perf/$TF": void 0,
"../../game/src/utils/$TW": void 0,
"../../main/src/$KM": void 0,
"../battleLogic/$KC": void 0,
"../battleLogic/component/$NJ": void 0,
"./$KH": "$KH",
"./compnents/$OI": "$OI",
"./compnents/$OJ": "$OJ",
"./compnents/$OL": "$OL",
"./compnents/$OM": "$OM",
"./compnents/$ON": "$ON",
"./compnents/$OO": "$OO",
"./render/$PA": "$PA",
"./render/$PB": "$PB",
"./render/$PE": "$PE",
"./render/$PI": "$PI",
"./ui/$PT": "$PT"
} ],
$OI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c6ff4Z1W/hILpveZlsAAEK0", "$OI");
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
i.$kc = i.LevelShakePoints = void 0;
var r = t("../../battleLogic/component/$NJ"), s = t("../../battleLogic/component/$NL"), a = t("../../../game/src/battle/types/$WY"), l = t("../$KH"), c = t("../../../game/src/battle/$QM"), h = cc.v2(), b = 1, u = (cc.v2(), 
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
e.bpK = cc.v2();
e.bpL = .2;
e.bpM = -1;
e.bpN = 0;
e.bpO = -1;
e.bpP = cc.v2();
e.bpQ = cc.v2();
return e;
}
e.prototype.ma = function() {
this.lZ.nk.bBd(a.$uD.$vA.P, this.bpR, this);
this.lZ.nk.bBd(a.$uD.$vA.Q, this.bpS, this);
this.lZ.nk.bBd(a.$uD.$vA.S, this.bpT, this);
this.lZ.nk.bBd(a.$uD.$vA.R, this.bpU, this);
this.lZ.nk.bBd(a.$uD.$vA.i, this.bpV, this);
this.lZ.nk.bBd(a.$uD.$vA.n, this.bpW, this);
};
e.prototype.mb = function() {
var t, e;
null === (t = this.lZ) || void 0 === t || t.nk.bBf(this);
null === (e = this.lZ) || void 0 === e || e.nl.bBf(this);
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.prototype.mf = function() {};
e.prototype.mc = function(t) {
var e, o, n, l, f, d = this.lZ.ng.gt(r.default), v = this.lZ.ng.gt(s.default).worldTimeScale;
this.lY.is.forEach(function(e) {
for (var i = e.Hs, o = i.length - 1; o > -1; o--) if (0 !== (r = i[o]).d) {
r.c += t * v;
r.c > r.d && i.splice(o, 1);
}
var n = e.Hr;
for (o = n.length - 1; o > -1; o--) {
var r;
if (0 !== (r = n[o]).e) {
r.d += t * v;
r.d > r.e && n.splice(o, 1);
}
}
});
var y = this.lY.is.get(this.lY.iu);
if (y) {
var g = y.Hs, m = g[g.length - 1];
y.Hv = m ? m.e : 0;
var _ = this.bpK;
if (-1 !== y.Hu) {
var $ = i.LevelShakePoints[y.Ht] && i.LevelShakePoints[y.Ht][y.Hu];
y.Hu += 1;
if ($) {
_.x = $[0];
_.y = $[1];
} else {
_.x = 0;
_.y = 0;
y.Hu = -1;
}
} else if (y.Hv > 0) {
_.x = Math.random() * y.Hv;
_.y = Math.random() * y.Hv;
} else {
_.x = 0;
_.y = 0;
}
var P = y.Hr, w = P[P.length - 1];
if (w) {
this.bpN = w.g;
this.bpM = w.h;
this.bpL = w.i;
if (w.f === a.$uD.$uW.d) {
b = w.h;
h.x = d.bmq + (d.bmr - d.bmq) / 2;
h.y = d.bms + (d.bmt - d.bms) / 2;
} else {
var I = this.lZ.ng.gi(this.bpN);
if (!I) return;
var O = this.lY.iO(I);
if (!O) return;
if (-1 === this.bpO) {
h.x = O.HQ + O.HT * (null !== (e = w.k) && void 0 !== e ? e : 0);
h.y = c.default.bGR.bHJ(O.HR, 0) + (null !== (o = w.l) && void 0 !== o ? o : 0);
if (F = null === (n = O.HM) || void 0 === n ? void 0 : n.camera) {
h.x += null !== (l = F[0]) && void 0 !== l ? l : 0;
h.y += null !== (f = F[1]) && void 0 !== f ? f : 0;
}
b = 1.5;
-1 !== this.bpM && (b = this.bpM);
b = Math.max(b + .1, 0);
this.lY.it.div(b, u);
if (!w.j) {
var E = h.x - .5 * u.x, x = h.x + .5 * u.x, S = h.y - .5 * u.y, M = h.y + .5 * u.y, C = d.bmq - E, T = x - d.bmr, R = d.bms - S, k = M - d.bmt;
if (u.x > d.bmr - d.bmq) h.x = .5 * (d.bmr - d.bmq) + d.bmq; else {
C > 0 && (h.x += C);
T > 0 && (h.x -= T);
}
if (u.y > d.bmt - d.bms) h.y = .5 * (d.bmt - d.bms) + d.bms; else {
R > 0 && (h.y += R);
k > 0 && (h.y -= k);
}
}
} else {
b = this.bpO;
h.set(this.bpP);
}
}
var A = this.lY._in;
if (A && A.mainCamera) {
if (0 === t) {
p.x = h.x;
p.y = h.y;
} else this.bpQ.lerp(h, this.bpL, p);
this.bpQ.set(p);
var N = A.mainCamera.zoomRatio, D = N + (b - N) * this.bpL;
p.addSelf(this.bpK);
A.mainCamera.node.setPosition(p);
A.mainCamera.zoomRatio = D;
for (var j = p, H = D, B = 0, L = this.lY.iz; B < L.length; B++) {
var F, Z = (F = L[B]).camera;
if (Z && Z.isValid && Z !== A.mainCamera) {
Z.node.setPosition(j.x * F.motion, 1 * j.y);
Z.zoomRatio = H;
}
}
}
}
}
};
e.prototype.bpR = function(t, e) {
var i = this;
this.lY.iL();
this.lY.is.forEach(function(t) {
var o = t.Hr.find(function(t) {
return 0 === t.c;
});
if (o) {
if (o.f === a.$uD.$uW.d) return;
o.b = i.lZ.ng.fi;
o.f = e.type;
o.g = t.Ho;
o.h = e.fixedZoomRatio;
o.i = e.cameraLerpRatio;
o.j = e.ignoreCameraCorrection || !1;
o.k = e.offsetX || 0;
o.l = e.offsetY || 0;
} else {
var n = {
b: i.lZ.ng.fi,
c: 0,
d: 0,
e: 0,
f: e.type,
g: t.Ho,
h: e.fixedZoomRatio,
i: e.cameraLerpRatio,
j: e.ignoreCameraCorrection || !1,
k: e.offsetX || 0,
l: e.offsetY || 0
};
t.Hr = [ n ];
}
});
};
e.prototype.bpS = function(t, e) {
var i = this, o = e[3] || 2;
if (!this.lZ.ng.fM.i) switch (o) {
case 1:
this.lY.is.forEach(function(o) {
if (o.Ho === t.hg) {
var n = {
b: i.lZ.ng.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uD.$uW.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
o.Hr.push(n);
}
});
break;

case 2:
this.lY.is.forEach(function(o) {
var n = {
b: i.lZ.ng.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uD.$uW.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
o.Hr.push(n);
});
break;

case 3:
t.hE.bjR.forEach(function(o) {
i.lY.is.forEach(function(n) {
if (n.Ho === o.eid) {
var r = {
b: i.lZ.ng.fi,
c: t.hg,
d: 0,
e: e[0],
f: a.$uD.$uW.c,
g: t.hg,
h: e[1] || -1,
i: e[2],
j: e.length > 3 && e[4],
k: e.length > 4 ? e[5] : 0,
l: e.length > 5 ? e[6] : 0
};
n.Hr.push(r);
}
});
});
}
};
e.prototype.bpT = function(t) {
var e = t.hg;
this.lY.is.forEach(function(t) {
for (var i = t.Hr, o = i.length - 1; o > -1; o--) i[o].c === e && i.splice(o, 1);
});
};
e.prototype.bpU = function(t, e) {
var i = this.lY.is.get(this.lY.iu);
i && (2 === e.length ? i.Hs.push({
b: t.hg,
e: e[1] || 6,
d: e[0],
c: 0
}) : i.Hw(e[0]));
};
e.prototype.bpV = function() {
this.mc(0);
};
e.prototype.bpW = function() {
this.mc(0);
};
return e;
}(l.$gq);
i.$kc = f;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../battleLogic/component/$NJ": void 0,
"../../battleLogic/component/$NL": void 0
} ],
$OJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "641a0kUC3hCYrkjWq5dIqK/", "$OJ");
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
var r = t("../../../bingo/src/$JX"), s = t("../../../game/src/battle/$QA"), a = t("../../../game/src/battle/types/$WU"), l = t("../../../game/src/battle/types/$WW"), c = t("../../../game/src/battle/types/$WX"), h = t("../../../game/src/battle/types/$WY"), b = t("../../../game/src/battle/$QN"), u = t("../../../main/src/$KM"), p = t("../../battleLogic/$KC"), f = t("../$KH"), d = cc.v2(), v = cc.v2(), y = function(t) {
t.cgSid = 0;
}, g = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bpX = new Set();
e.bpY = 0;
e.bpZ = -1;
e.bqa = 0;
e.bqb = -1;
e.bqc = 0;
e.bqd = 0;
e.bqe = "";
e.bqf = [];
e.bqg = [];
return e;
}
e.prototype.ma = function() {
this.bqh();
this.lZ.nk.bBd(h.$uD.$vA.o, this.bqi, this);
this.lZ.nk.bBd(h.$uD.$vA.n, this.bqj, this);
this.lZ.nk.bBd(h.$uD.$vA.t, this.bql, this);
this.lZ.nk.bBd(h.$uD.$vA.bc, this.bqm, this);
this.lZ.nk.bBd(h.$uD.$vA.B, this.bqn, this);
this.lZ.nk.bBd(h.$uD.$vA.l, this.bqk, this);
this.lZ.nk.bBd(h.$uD.$vA.bl, this.bqv, this);
this.lZ.nk.bBd(h.$uD.$vA.bt, this.bqE, this);
this.lZ.nk.bBd(h.$uD.$vA.bA, this.bqF, this);
};
e.prototype.mb = function() {
var t;
null === (t = this.lZ) || void 0 === t || t.nk.bBf(this);
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.prototype.mc = function(t) {
this.bqp(t);
this.lZ.ng.gN ? this.bqq(t) : this.bqr(t);
};
e.prototype.mf = function() {
this.bqx();
};
e.prototype.bqh = function() {
var t, e, i, o, n = this.mg, s = this.mh;
if (null == s ? void 0 : s.isValid) {
this.bqb = -1;
this.lZ.nm && s.bzJ();
if (n.gN) {
s.byO();
s.minmapNode.parent.active = !1;
s.nodeTime.active = !0;
s.nodeLeftTop.active = !1;
s.nodePvpTop.active = !0;
s.node_autoTxt.active = !1;
this.bqG();
if (n.fM.f === l.$uy.g) {
s.nodePvpTop.getChildByName("btnPause").active = !1;
s.bzX();
}
} else {
s.byN();
this.mh.minmapNode.parent.active = !0;
this.mh.nodeTime.active = n.fn > 0;
this.mh.nodeLeftTop.active = !0;
this.mh.nodePvpTop.active = !1;
s.node_autoTxt.active = !1;
var a = this.lZ.nx();
a && (null === (t = s.byz) || void 0 === t || t.bAJ(a.hv.bgD, a.hv.bgE));
s.btnPause.active = !n.gC();
s.btnAutoBattle.active = n.fM.h && !n.gC();
if (n.fM.h) {
s.bzM();
this.bqw();
s.bzO();
}
this.mh.bzU();
if (n.fK.type === r.$fb.tblEnum.InstanceTypeTableEnum.WORLD_BOSS_TEAM) {
this.mh.nodeTime.active = !1;
s.bzV();
}
if (n.fM.f === l.$uy.f) {
s.bzW();
s.minmapNode.parent.active = !1;
this.mh.nodeLeftTop.active = !1;
}
var c = null === (o = null === (i = null === (e = this.lZ.my) || void 0 === e ? void 0 : e.bCC) || void 0 === i ? void 0 : i.params) || void 0 === o ? void 0 : o.playMode;
s.nodePractice.active = b.$lS.bJc(c);
s.abyssInfoUI.node.active = b.$lS.bJd(c);
}
s.bzB(!1);
if (n.fn) {
s.battleTime.node.active = !0;
this.bqe = "";
s.battleTime.string = "";
} else {
this.bqe = " --";
s.battleTime.string = " --";
}
s.bzC(this.lZ.mz);
this.lZ.np() && s.byP();
this.bqy();
this.bqx();
}
};
e.prototype.bqi = function() {
this.bqw();
this.bqx();
};
e.prototype.bqj = function() {
var t;
null === (t = this.mh) || void 0 === t || t.bzU();
};
e.prototype.bqk = function() {
var t;
null === (t = this.mh) || void 0 === t || t.bzU();
};
e.prototype.bql = function() {
var t;
null === (t = this.mh) || void 0 === t || t.bzU();
};
e.prototype.bqm = function(t) {
this.lY.iV(t);
};
e.prototype.bqn = function(t) {
var e, i, o, n, r;
if (this.lY.iV(t.atkEntity) && !0 !== (null === (o = null === (i = null === (e = t.target) || void 0 === e ? void 0 : e.hE) || void 0 === i ? void 0 : i.bjF) || void 0 === o ? void 0 : o.hideInfo) && this.bpZ !== (null === (n = t.target) || void 0 === n ? void 0 : n.hg)) {
this.bpZ = null === (r = t.target) || void 0 === r ? void 0 : r.hg;
this.bqa = t.beforeHP;
}
};
e.prototype.bqo = function(t, e) {
for (var i = 0; i < t.length; i++) {
var o = t[i];
if (o.Dv.commonType === e) return o;
}
return null;
};
e.prototype.bqp = function() {
var t, e, i, o = this.mg, n = this.mh;
if (null == n ? void 0 : n.isValid) {
if (o.fn) {
var r = Math.floor(Math.max(0, o.fn - o.fk)), a = "";
if (r > 60) {
var l = r % 60;
a = Math.floor(r / 60) + ":" + l;
} else a = "" + r;
n.bzB(r < 20);
if (this.bqe !== a) {
this.bqe = a;
n.battleTime.string = a;
}
}
var c = this.lZ.nx();
if (c) {
var b = c.hv, f = b.bgd, g = c.hE;
if (g) {
var m = n.byw;
if (m.splitOpenPanel.node.active && ($ = null == (_ = this.lY.iP(c.hg)) ? void 0 : _.Ie) && $.isValid) {
$.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R, d);
this.lY._in.mainCamera.getWorldToScreenPoint(d, v);
m.splitOpenPanel.node.parent.convertToNodeSpaceAR(v, d);
d.y += 117;
m.splitOpenPanel.node.setPosition(d);
}
if (m.selectPlayerPanel.node.active) {
var _, $;
if (($ = null == (_ = this.lY.iP(c.hg)) ? void 0 : _.Ie) && $.isValid) {
$.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R, d);
this.lY._in.mainCamera.getWorldToScreenPoint(d, v);
m.selectPlayerPanel.node.parent.convertToNodeSpaceAR(v, d);
d.y += 117;
m.selectPlayerPanel.node.setPosition(d);
}
}
for (var P = null, w = null, I = 0; I < f.length; I++) {
var O = (it = f[I]).Dv.commonType;
P || O !== u.cvResUrl.commonSkillDodge2Cid ? w || O !== u.cvResUrl.commonSkillDodgeCid || (w = it) : P = it;
}
var E = !!(null === (t = g.bjx) || void 0 === t ? void 0 : t.dodgeSkill);
m.fEh.forEach(y);
if (null === (e = g.bjx) || void 0 === e ? void 0 : e.skBtnCges) for (var x = 0, S = g.bjx.skBtnCges; x < S.length; x++) {
var M = S[x];
if (m.fEh.has(M.originBtnId)) {
var C = M.sid;
if (M.csid) (J = this.bqo(f, M.csid)) && (C = J.Dm); else if (!b.bgp(M.sid)) continue;
if (!C) continue;
m.fEh.get(M.originBtnId).cgSid = C;
}
}
for (var T = 0, R = m.fEj; T < R.length; T++) if (!(z = R[T]).AU && m.fEh.has(z.AT)) {
C = (M = m.fEh.get(z.AT)).cgSid > 0 ? M.cgSid : M.sid;
z.AZ !== C && z.Bc(C);
}
var k = this.bpX;
k.clear();
for (var A = 0, N = g.bjA; A < N.length; A++) {
var D = N[A];
D.sid ? D.tgs.length > 1 ? p.default.FD(c, D.tgs) && k.add(D.sid) : k.add(D.sid) : D.csid && (J = this.bqo(f, D.csid)) && k.add(J.Dm);
if (D.t === h.$uD.$vl.bI) {
var j = D.p.changesGroup, H = !0;
if (D.p.consumeSid) {
var B = b.bgp(D.p.consumeSid);
H = B && p.default.Fz(c, B);
}
if (H) {
var L = g.bjF.changeGroup[D.p.changesGroup];
if (L && L.length > 0) for (var F = 0, Z = L; F < Z.length; F++) {
var Y = Z[F];
Y.sid && k.add(Y.sid);
if (Y.csid) {
var J;
(J = this.bqo(f, Y.csid)) && k.add(J.Dm);
}
}
"Dodge" === j && (E = !0);
}
}
}
k.has(u.cvResUrl.commonDodge2Sid) && (E = !0);
for (var X = 0, q = m.fEj; X < q.length; X++) {
var z = q[X];
if (it = b.bgp(z.AZ)) if (z.AU || z.AT !== s.BATTLE_BTN_$0.DODGE) {
var V = k.has(it.Dm) && p.default.Fz(c, it);
z.Bm(it.Do > 1, it.Dn, 1 - it.cd / it.maxCdTime, it.Do === it.Dn, V, it.cd);
} else if (E && P) {
P.Dm !== z.AZ && z.Bc(P.Dm);
var K = p.default.Fz(c, P);
z.Bm(P.Do > 1, P.Dn, 1 - P.cd / P.maxCdTime, P.Do === P.Dn, K, P.cd);
} else {
var U = !!P;
w.Dm !== z.AZ && z.Bc(w.Dm);
var G = k.has(it.Dm) && p.default.Fz(c, w);
z.Bm(U && P.Do > 1, U ? P.Dn : 0, 1 - it.cd / it.maxCdTime, !0, G, w.cd);
}
}
if (m.splitBtn.node.active) for (var W = m.splitBtn.fFH, Q = 0; Q < W.length; Q++) if (it = b.bgp(W[Q])) {
V = k.has(it.Dm) && p.default.Fz(c, it);
m.splitBtn.fFN(Q, it.Do > 1, it.Dn, 1 - it.cd / it.maxCdTime, it.Do === it.Dn, V, it.cd);
}
for (var tt = 0, et = f; tt < et.length; tt++) {
var it;
if ((null === (i = null == (it = et[tt]) ? void 0 : it.Dv) || void 0 === i ? void 0 : i.isShowPassiveSkillIcon) && (z = m.fDU[it.Dm])) {
z.fER(it.Dm);
z.fEY(!0, it.Dn, 1 - it.cd / it.maxCdTime, it.Do === it.Dn, !0, it.cd);
}
}
}
}
}
};
e.prototype.bqq = function(t) {
var e = this.mg, i = this.mh;
if (null == i ? void 0 : i.isValid) {
if (this.lZ.mz) {
this.bqd += t;
this.bqd > .2 && (this.bqd = 0);
}
this.lZ.ng.fF.forEach(function(t) {
var o = e.gi(t.dco);
if (null == o ? void 0 : o.hv) {
var n = o.hv, r = t.dcd - 1, s = i.byB[r];
if (s) {
s.bAv(n.bgD, n.bgE);
s.bAw(n.bgF, n.bgG);
if (o.hN) {
var a = o.hN;
s.bAA(-1 === a.bjc ? 0 : Math.max(a.bjc - a.bjf, 0), -1 === a.biY ? 0 : Math.max(a.biY - a.bjg, 0), -1 === a.bja ? 0 : Math.max(a.bja - a.bjh, 0));
}
}
}
});
}
};
e.prototype.bqr = function(t) {
var e, i, o, n, r, s, a, l = this.mg, c = this.mh;
if (null == c ? void 0 : c.isValid) {
var h = c.byw, b = this.lZ.nx();
if (b) {
var u = b.hv;
null === (e = c.byz) || void 0 === e || e.bAJ(u.bgD, u.bgE);
null === (i = c.byz) || void 0 === i || i.bAK(u.bgF, u.bgG);
null === (o = c.byz) || void 0 === o || o.bAI(u.bgH);
null === (n = c.byz) || void 0 === n || n.bAN(this.bqt(b, this.bqf));
this.lZ.np() && (null === (r = c.byA) || void 0 === r || r.bxH(this.bqu(this.lZ.ng.fF)));
if (-1 !== this.bpZ) {
var f = this.bpZ !== this.bqb;
this.bqb = this.bpZ;
var d = l.gi(this.bpZ);
if (f) this.bpY = 0; else if (!(null == d ? void 0 : d.hU)) {
this.bpY += t;
if (this.bpY > 1) {
this.bpY = 0;
this.bpZ = -1;
}
}
if (d) {
c.byy.node.active || (c.byy.node.active = !0);
var v = d.hv, y = v.bfZ, g = $eZ.i18n.LV(y.nameText), m = null === (s = d.hx) || void 0 === s ? void 0 : s.blT;
if (m) {
var _ = this.lZ.nw(m);
_ && (g = _.dbY.nick + " - " + g);
}
if (c.byy) {
c.byy.bAm(y.id, v.bgH);
c.byy.bAl(this.bqt(d, this.bqg));
var $ = Math.min(1e3, v.bgE), P = v.bgD;
if (f) {
c.byy.bAg.btC(this.bqa, $);
if (this.bqa !== P) {
c.byy.bAg.btv > P && c.byy.bAn();
c.byy.bAg.btB(P, $, v.bgM > 0);
}
} else {
c.byy.bAg.btv > P && c.byy.bAn();
c.byy.bAg.btB(P, $, v.bgM > 0);
}
}
}
} else c.byy.node.active && (c.byy.node.active = !1);
var w = b.hL, I = u.bgO(), O = u.bgQ(), E = $eZ.timeMgr.cLv;
if (w.blO.length > 0 && this.lZ.ng.fM.q) for (var x = 0; x < w.blO.length; x++) {
var S = w.blO[x], M = h.fDT[x];
if (M) {
var C = $eZ.bag.bXe(S.Ee), T = p.default.Fx(b, S) && C > 0;
M.Bn(!0, C, 1 - S.Eh / S.Ei, !1, T, S.Eh);
if (T && (null === (a = S.Eo) || void 0 === a ? void 0 : a.isAuto)) {
var R = !1;
S.Ep && 100 * I < S.Eo.applyValue && (R = !0);
S.Eq && 100 * O < S.Eo.applyValue && (R = !0);
if (R && E - this.bqc > 500) {
this.bqc = E;
this.bqD(x);
}
}
}
}
}
}
};
e.prototype.bqs = function(t) {
return this.bqt(t, []);
};
e.prototype.bqt = function(t, e) {
var i = 0, o = t && t.hE;
if (o) for (var n = o.bjB, r = 0; r < n.length; r++) {
var s = n[r], a = s.DF && s.DF.icon;
if (a) {
var l = s.DJ > 0 ? cc.misc.clampf((s.DJ - s.DM) / s.DJ, 0, 1) : -1, c = e[i];
if (!c) {
c = {
b: "",
c: 0
};
e[i] = c;
}
c.b = "ccui:icon/buff/" + a;
c.c = l;
i++;
}
}
e.length = i;
return e;
};
e.prototype.bqu = function(t) {
var e = [], i = this.mg;
if (!i) return e;
for (var o = 0, n = t; o < n.length; o++) {
var r = n[o], s = i.gi(r.dco);
if (null == s ? void 0 : s.hv) {
var a = s.hv;
e.push({
b: r.dca,
c: a.bfZ.avatar,
d: r.dcb,
e: a.bgD,
f: a.bgE,
g: a.finalDamage,
h: this.bqs(s),
i: $eZ.battleServer.getNetworkStatus(r.dca),
j: r.dcc
});
}
}
return e;
};
e.prototype.bqv = function() {
var t, e = this.mh;
if (e) {
var i = this.lZ.nx();
if (i) {
var o = i.hv;
o && (null === (t = e.byz) || void 0 === t || t.bAI(o.bgH));
}
}
};
e.prototype.bqw = function() {
var t, e = this.mh;
if (e) {
var i = this.lZ.nL();
if (i && i.fM.h) {
var o = this.lZ.ny();
e.bzL(null !== (t = null == o ? void 0 : o.dcv) && void 0 !== t && t);
}
}
};
e.prototype.bqx = function() {
var t = this.mh;
if (t && this.lZ.nL()) {
this.lZ.ny();
t.byv.node.active = !this.lZ.nm;
t.bzT(!this.lZ.nm);
}
};
e.prototype.bqy = function() {
var t, e, i, o, n, l, h, b, u, p, f, d, v, y, g, m, _, $, P, w, I, O, E, x, S, M, C = this.lZ.nx();
if (C) {
this.lZ.nd.bBH.clear();
var T, R, k = new c.$uC(), A = new c.$uB(), N = C.hL;
if (!(null === (t = N.blN) || void 0 === t ? void 0 : t.skillButton) || this.lZ.ng.fj.traning) {
A.dbq();
var D = C.hv.bgd.find(function(t) {
return 1 === t.Dv.commonType;
}), j = C.hv.bgd.find(function(t) {
return 2 === t.Dv.commonType;
}), H = C.hv.bgd.find(function(t) {
return 3 === t.Dv.commonType;
});
A.dbi[s.BATTLE_BTN_$0.NORMAL].dba = null !== (e = null == D ? void 0 : D.Dm) && void 0 !== e ? e : 0;
A.dbi[s.BATTLE_BTN_$0.JUMP].dba = null !== (i = null == j ? void 0 : j.Dm) && void 0 !== i ? i : 0;
A.dbi[s.BATTLE_BTN_$0.DODGE].dba = null !== (o = null == H ? void 0 : H.Dm) && void 0 !== o ? o : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL1].dba = null !== (n = N.blP[a.$uv.g]) && void 0 !== n ? n : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL2].dba = null !== (l = N.blP[a.$uv.h]) && void 0 !== l ? l : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL3].dba = null !== (h = N.blP[a.$uv.i]) && void 0 !== h ? h : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL4].dba = null !== (b = N.blP[a.$uv.j]) && void 0 !== b ? b : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL5].dba = null !== (u = N.blP[a.$uv.k]) && void 0 !== u ? u : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL6].dba = null !== (p = N.blP[a.$uv.l]) && void 0 !== p ? p : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL7].dba = null !== (f = N.blP[a.$uv.m]) && void 0 !== f ? f : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL8].dba = null !== (d = N.blP[a.$uv.n]) && void 0 !== d ? d : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL9].dba = null !== (v = N.blP[a.$uv.o]) && void 0 !== v ? v : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL10].dba = null !== (y = N.blP[a.$uv.p]) && void 0 !== y ? y : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL11].dba = null !== (g = N.blP[a.$uv.q]) && void 0 !== g ? g : 0;
A.dbi[s.BATTLE_BTN_$0.SKILL12].dba = null !== (m = N.blP[a.$uv.r]) && void 0 !== m ? m : 0;
A.dbj.daY([ null !== (_ = N.blP[a.$uv.v]) && void 0 !== _ ? _ : 0, null !== ($ = N.blP[a.$uv.v + 1]) && void 0 !== $ ? $ : 0, null !== (P = N.blP[a.$uv.v + 2]) && void 0 !== P ? P : 0, null !== (w = N.blP[a.$uv.v + 3]) && void 0 !== w ? w : 0 ]);
A.dbi.forEach(function(t) {
t.dba > 0 && (C.hv.bgp(t.dba) || (t.dba = 0));
});
} else {
var B = $eZ.skill.csT.csO(r.$fb.tblEnum.SkillTypeTableEnum.PVE_SKILL), L = $eZ.skill.csT.csO(this.lZ.ng.gN ? r.$fb.tblEnum.SkillTypeTableEnum.PVP_SKILL : r.$fb.tblEnum.SkillTypeTableEnum.PVE_SKILL);
A.dbm(B);
A.dbn(L);
}
T = [ k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.d] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.e] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.f] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.g] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.h] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.i] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.j] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.k] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.l] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.m] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.n] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.o] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.p] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.q] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.r] - 1) ];
R = [ k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.v] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.v + 1] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.v + 2] - 1), k.dbC(a.FRAME_SYNC_CMD2BIT[a.$uv.v + 3] - 1) ];
for (var F in a.FRAME_SYNC_CMD2BIT) {
var Z = a.FRAME_SYNC_CMD2BIT[F];
this.lZ.nd.bBH.set(k.dbC(Z - 1), Number(F));
}
var Y = (null === (I = C.hD) || void 0 === I ? void 0 : I.bkR) || 1, J = [];
this.lZ.ng.fF.forEach(function(t) {
t.dcf === Y && J.push({
position: t.dcd,
baseInfo: {
confId: t.dcB[0].roleCfgId,
level: t.dcc,
fightpoint: 0,
name: t.dcb,
roleId: t.dca,
version: "",
personal: null,
skill: []
}
});
});
null === (O = this.mh.byw) || void 0 === O || O.fEn(A, J, 0, this.bqA.bind(this), this.bqB.bind(this), this.bqC.bind(this));
(null === (E = this.lZ.my.bCX.e) || void 0 === E ? void 0 : E.c) || null === (x = this.mh.byw) || void 0 === x || x.fEF(T, R);
C.hL && this.lZ.ng.fM.q && (null === (S = this.mh.byw) || void 0 === S || S.fEp(C.hL.blO, this.bqD.bind(this)));
var X = this.bqz();
null === (M = this.mh.byw) || void 0 === M || M.fEq(X, C.hv.bfZ.id);
}
};
e.prototype.bqz = function() {
var t, e = this.lZ.nx();
if (!(null == e ? void 0 : e.hv)) return [];
for (var i = [], o = 0, n = e.hv.bgd; o < n.length; o++) {
var r = n[o];
(null === (t = null == r ? void 0 : r.Dv) || void 0 === t ? void 0 : t.isShowPassiveSkillIcon) && i.push(r);
}
return i;
};
e.prototype.bqA = function(t, e, i) {
if (this.lZ.nx() && t) {
var o = s.BATTLE_BTN_2_CMD[i];
o && this.lZ.nd.bBZ(e, o);
}
};
e.prototype.bqB = function(t, e, i) {
if (this.lZ.nx() && t) {
var o = a.$uv.v + i;
this.lZ.nd.bBZ(e, o);
}
};
e.prototype.bqC = function(t, e, i, o, n) {
if (this.lZ.nx() && t) {
var r = s.BATTLE_BTN_2_CMD[e];
r && n && this.lZ.nd.bCl(r, n);
}
};
e.prototype.bqD = function(t) {
var e, i, o, n = this.lZ.nx();
if (null == n ? void 0 : n.hL) {
var r = n.hL.blO[t];
r && p.default.Fx(n, r) && ($eZ.bag.bXe(r.Ee) <= 0 || this.lZ.nd.bCw(t, r.Ee, null === (o = null === (i = null === (e = this.lZ.my) || void 0 === e ? void 0 : e.bCC) || void 0 === i ? void 0 : i.params) || void 0 === o ? void 0 : o.playMode));
}
};
e.prototype.bqE = function() {
this.bqy();
};
e.prototype.bqF = function() {
this.mh && this.bqG();
};
e.prototype.bqG = function() {
var t = this, e = this.mh;
e && [ 0, 1 ].forEach(function(i) {
var o = e.byB[i];
if (o) {
var n = t.lZ.ng.fF.find(function(t) {
return t.dcd === i + 1;
});
if (n) {
var r = t.lZ.ng.gi(n.dco);
if (null == r ? void 0 : r.hv) {
var s = r.hv;
o.node.active = !0;
o.bAq(s.bgH, s.bfZ.id, n.dcb, n.dca);
}
} else o.node.active = !1;
}
});
};
return e;
}(f.$gq);
i.$kf = g;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QN": void 0,
"../../../game/src/battle/types/$WU": void 0,
"../../../game/src/battle/types/$WW": void 0,
"../../../game/src/battle/types/$WX": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../../main/src/$KM": void 0,
"../../battleLogic/$KC": void 0
} ],
$OK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "697e9U5gwBAMaQoizQ+s6HS", "$OK");
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
i.$kg = void 0;
var r = function(t) {
n(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.ma = function() {
this.bqH();
};
e.prototype.mb = function() {
var t;
null === (t = this.lZ) || void 0 === t || t.nk.bBf(this);
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.prototype.mf = function() {};
e.prototype.mc = function() {};
e.prototype.bqH = function() {};
e.prototype.bqI = function() {
if (this.lY._in) {
var t = this.mg, e = this.mh, i = t.gs().bjn, o = 0, n = 0;
if (i && i.duration > 0) {
var r = i.moments[0], s = t.gk(r);
if (s) {
var a = s.dcD();
if (a) {
o = a.roleCfgId;
n = a.skinId;
}
}
}
e.bzH(o, n);
if (t.fn) {
var l = Math.floor(Math.max(0, t.fn - t.fk)), c = "";
if (l > 60) {
var h = l % 60;
c = Math.floor(l / 60) + ":" + h;
} else c = "" + l;
e.bzB(l < 20);
e.battleTime.string = c;
}
}
};
return e;
}(t("../$KH").$gq);
i.$kg = r;
cc._RF.pop();
}, {
"../$KH": "$KH"
} ],
$OL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "f35c1fLq0NKoY5C+gcdsCYA", "$OL");
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
i.$kh = void 0;
var r = t("../../../game/src/battle/$QM"), s = t("../../../game/src/battle/types/$WY"), a = t("../../../game/src/$KL"), l = t("../../../game/src/perfab/town/$CCG"), c = t("../../battleLogic/component/$NI"), h = t("../$KH"), b = cc.v2(), u = (new Set(), 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bqJ = 0;
e.bqK = !1;
e.bqL = [];
e.bqN = 120;
e.bqO = cc.v2();
return e;
}
e.prototype.mf = function() {};
e.prototype.ma = function() {
this.bqJ = 0;
};
e.prototype.mb = function() {
this.lZ.nk.bBf(this);
this.bqM = null;
this.bqL.length = 0;
};
e.prototype.mc = function(t) {
if (this.lY._in) {
var e = this.mg.fI;
if (e) if (e.dcO && 0 !== e.ddb) {
this.bqJ += 3 * t;
var i = this.lZ.nx();
if (i) {
var o = this.lY.iO(i);
b.x = o.HQ;
b.y = o.HR;
var n = this.mg.fE.bmW.find(function(t) {
return t.ho(c.default).bmj === e.ddb;
});
if (n) {
this.bqK = !0;
this.bqQ(n.hw.x, n.hw.y);
this.bqP();
}
}
} else this.bqK && this.bqR();
}
};
e.prototype.bqP = function() {
if (this.bqL.length) for (var t = 0, e = this.bqL; t < e.length; t++) e[t].opacity = 255 * (.5 * (.5 * Math.sin(this.bqJ) + .5) + .5);
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.prototype.bqQ = function(t, e) {
var i = this.lY._in;
if (i) {
var o = this.lZ.nx();
if (o) {
var n = this.lY.iO(o), c = t - n.HQ > 0 ? 1 : -1, h = e - n.HR > 0 ? 1 : -1, b = Math.abs(n.HQ - t), u = Math.abs(n.HR - e), p = Math.sqrt(b * b + u * u), f = this.bqN, d = Math.floor(p / f);
this.bqO.x = n.HQ;
this.bqO.y = n.HR;
for (var v = 0; v < d; v++) {
var y = this.bqO.x, g = this.bqO.y, m = this.bqO.x + f * c, _ = this.bqO.y + f * s.$uD.TOWN_SCENE_DEFINE.SCENE_MOVE_SPEED_SCALE_Y * h;
(1 == c && m > t || -1 == c && m < t) && (m = t);
(1 == h && _ > e || -1 == h && _ < e) && (_ = e);
this.bqO.x = m;
this.bqO.y = _;
var $ = Math.abs(m - t) + Math.abs(_ - e) < 15, P = r.default.bGR.bHI(this.bqO.y);
if (I = this.bqL[v]) I.setPosition(cc.misc.lerp(I.position.x, this.bqO.x, .3), cc.misc.lerp(I.position.y, P, .3)); else {
(I = $eZ.prefabMgr.cPB(a.default.sceneComp.Arrow)).__cvResCfg = a.default.sceneComp.Arrow;
this.bqL.push(I);
i.sceneLayer.addChild(I, s.$uD.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
I.setPosition(this.bqO.x, P);
}
I.angle = this.bqS(y, r.default.bGR.bHI(g), this.bqO.x, P);
if (v == d - 1 || $) {
if (!this.bqM) {
var w = $eZ.prefabMgr.cPB(a.default.sceneComp.GO);
w.__cvResCfg = a.default.sceneComp.GO;
i.sceneLayer.addChild(w, 0, "goArrow");
this.bqM = w.getComponent(l.default);
}
this.bqM.node.active = !0;
this.bqM.node.zIndex = s.$uD.TOWN_SCENE_DEFINE.FIGHT_TEXT;
if (b > u) {
1 == c ? this.bqM.fMX() : this.bqM.fMW();
this.bqM.node.setPosition(1 == c ? t - 200 : t + 200, r.default.bGR.bHI(e));
} else {
this.bqM.node.setPosition(t, r.default.bGR.bHI(e) + 100);
this.bqM.fMY();
}
}
if ($) break;
}
for (v = this.bqL.length - 1; v >= d; v--) {
var I = this.bqL[v];
$eZ.prefabMgr.cPE(I);
this.bqL.splice(v, 1);
}
}
}
};
e.prototype.bqR = function() {
var t;
this.bqK = !1;
(null === (t = this.bqM) || void 0 === t ? void 0 : t.isValid) && (this.bqM.node.active = !1);
this.bqL.forEach(function(t) {
$eZ.prefabMgr.cPE(t);
});
this.bqL.length = 0;
};
e.prototype.bqS = function(t, e, i, o) {
return 180 * Math.atan2(o - e, i - t) / Math.PI;
};
return e;
}(h.$gq));
i.$kh = u;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../../game/src/$KL": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../../game/src/perfab/town/$CCG": void 0,
"../../battleLogic/component/$NI": void 0
} ],
$OM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ee9deZ6XbhBEYdrcl1bTO+l", "$OM");
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
i.$ki = void 0;
var s = t("../../../bingo/src/$JX"), a = t("../../../game/src/battle/$QM"), l = t("../../../game/src/battle/types/$WY"), c = t("../../../main/src/$KM"), h = t("../../../main/src/tbl/TblEnum"), b = t("../../battleLogic/component/$NJ"), u = t("../../battleLogic/component/$NL"), p = t("../$KH"), f = t("../render/$OU"), d = t("../render/$OV"), v = t("../render/$OW"), y = (cc.v2(), 
new Set(), (n = {})[h.TblEnum.QualityTypeTableEnum.COMMON] = c.cvResUrl.mc_drop_PuTong_pick, 
n[h.TblEnum.QualityTypeTableEnum.SENIOR] = c.cvResUrl.mc_drop_Gaoji_pick, n[h.TblEnum.QualityTypeTableEnum.RARE] = c.cvResUrl.mc_drop_XiYou_pick, 
n[h.TblEnum.QualityTypeTableEnum.EPIC] = c.cvResUrl.mc_drop_ShenQi_pick, n[h.TblEnum.QualityTypeTableEnum.LEGEND] = c.cvResUrl.mc_drop_ShiShi_pick, 
n), g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bqT = [];
return e;
}
e.prototype.mf = function() {};
e.prototype.ma = function() {
this.bqT.length = 0;
this.lZ.nk.bBd(l.$uD.$vA.bj, this.bqZ, this);
this.lZ.nk.bBd(l.$uD.$vA.bk, this.bra, this);
this.lZ.nk.bBd(l.$uD.$vA.m, this.brc, this);
this.lZ.nk.bBd(l.$uD.$vA.p, this.brd, this);
};
e.prototype.mb = function() {
this.lZ.nk.bBf(this);
this.bqT = null;
};
e.prototype.mc = function(t) {
var i, o = this.lY._in;
if (o) {
var n = this.lZ.nx();
if (n) for (var r = this.lY.iO(n), c = this.lZ.ng.gt(b.default), p = this.lZ.ng.gt(u.default).worldTimeScale, f = this.lZ.mH ? 0 : 1, y = t * p * f, g = this.lY.ir.length - 1; g >= 0; --g) {
var m = this.lY.ir[g], _ = a.default.bGR.bHn(m.IX.itemId);
if (_) {
var $ = null !== (i = _.quality) && void 0 !== i ? i : 0;
if (!m.IY) {
var P = this.lZ.nb.bFN(s.$fb.ResUrl.DropItemRender);
if (!P) continue;
o.sceneLayer.addChild(P, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + $);
m.IY = P.getComponent(d.default);
m.IY.btj(m.IX);
}
if ($ >= h.TblEnum.QualityTypeTableEnum.EPIC && !m.IZ && (I = this.lZ.nb.bFN(s.$fb.ResUrl.DropItemText))) {
o.sceneLayer.addChild(I, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + 10 + $);
m.IZ = I.getComponent(v.default);
m.IZ.btp(m.IX);
}
m.IY.setTimeScale(p * f);
if (m.IV) {
if (this.bqV(r.HQ, r.HR, m.IO, m.IP) && m.IU) {
var w = this.lY.ir.splice(g, 1)[0];
this.bqW(w);
}
} else {
m.IO += m.IR * y;
m.IP += m.IS * y;
m.IQ += m.IT * y + -2300 * y * y / 2;
if (m.IQ <= 0) {
m.IQ = 0;
m.IV = !0;
if (m.IY) {
m.IY.btm();
m.IY.node.zIndex = m.IU ? a.default.bGR.bHU(m.IP) : l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT;
}
if (!m.IZ) {
var I;
if (I = this.lZ.nb.bFN(s.$fb.ResUrl.DropItemText)) {
o.sceneLayer.addChild(I, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT + 10 + $);
m.IZ = I.getComponent(v.default);
m.IZ.btp(m.IX);
}
}
}
m.IT += -2300 * y;
e.bqU(m, c);
m.Jd();
}
}
}
}
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.bqU = function(t, e) {
t.IO = Math.max(Math.min(t.IO, e.bmv), e.bmu);
t.IP = Math.max(Math.min(t.IP, e.bmx), e.bmw);
};
e.prototype.bqV = function(t, e, i, o) {
return Math.abs(i - t) < l.$uD.TOWN_SCENE_DEFINE.dropRange[0] && Math.abs(o - e) < l.$uD.TOWN_SCENE_DEFINE.dropRange[1];
};
e.prototype.bqW = function(t, e) {
var i;
void 0 === e && (e = !1);
if (t) {
var o = this.lZ.ny();
if (o.dco) {
var n = this.lY.iP(o.dco);
if ((null == n ? void 0 : n.Ie) && t.IY) {
var r = this.lZ.nb.bFN(c.cvResUrl.DropItemPickBall);
if (r) {
this.lY._in.sceneLayer.addChild(r, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT);
r.x = t.IO;
r.y = t.IY.node.y;
r.getComponent(f.default).btc(t.IX, o.dco);
}
}
}
var s = null === (i = t.IY) || void 0 === i ? void 0 : i.btg, a = y[null == s ? void 0 : s.quality];
a && this.lY.jW(a, t.IO, t.IP, t.IQ, 1, 0, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM);
t.Jb();
}
};
e.prototype.bqX = function(t, e) {
var i;
if (t) {
if (e) {
var o = this.lY.iP(e.hg);
if (null == o ? void 0 : o.Ie) {
var n = this.lZ.nb.bFN(c.cvResUrl.DropItemPickBall);
if (n) {
this.lY._in.sceneLayer.addChild(n, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM_TXT);
n.x = t.IO;
n.y = t.IY.node.y;
n.getComponent(f.default).btc(t.IX, e.hg);
}
}
}
var r = null === (i = t.IY) || void 0 === i ? void 0 : i.btg, s = y[null == r ? void 0 : r.quality];
s && this.lY.jW(s, t.IO, t.IP, t.IQ, 1, 0, l.$uD.TOWN_SCENE_DEFINE.DROP_ITEM);
t.Jb();
}
};
e.prototype.bqY = function() {
for (var t = 0, e = this.lY.ir.slice(); t < e.length; t++) {
var i = e[t];
this.bqW(i);
}
this.lY.ir.length = 0;
};
e.prototype.bqZ = function(t, i, o, n, r) {
var l, c;
void 0 === r && (r = !0);
if (null == n ? void 0 : n.dropList) {
var h = null === (c = null === (l = this.lZ) || void 0 === l ? void 0 : l.ng) || void 0 === c ? void 0 : c.gt(b.default);
if (h) for (var u = 0, f = n.dropList; u < f.length; u++) {
var d = f[u], v = a.default.bGR.bHn(d.itemId);
if (v && v.id !== s.$fb.tblEnum.ItemTableEnum.EXP_ID) {
var y = new p.$gu();
y.IM = this.lY.ip;
this.lY.ir.push(y);
y.IW = n.uniqueId;
y.IX = d;
y.IO = t;
y.IP = i;
y.IQ = Math.max(o, 70);
e.bqU(y, h);
var g = 360 * Math.random() * Math.PI / 180, m = 50 + Math.floor(350 * Math.random());
y.IR = Math.cos(g) * m;
y.IS = Math.sin(g) * m;
y.IT = 600 + Math.floor(300 * Math.random());
y.IU = r;
}
}
}
};
e.prototype.bra = function(t) {
for (var e = 0, i = this.lY.ir.slice(); e < i.length; e++) {
var o = i[e];
this.bqX(o, t);
}
this.lY.ir.length = 0;
};
e.prototype.brb = function() {};
e.prototype.brc = function() {
this.bqY();
};
e.prototype.brd = function() {
var t = this;
this.lY.ip.ng.gC() || $eZ.timeMgr.cLz(3e3, 1, function() {
var e, i;
(null === (i = null === (e = t.lY) || void 0 === e ? void 0 : e.ip) || void 0 === i ? void 0 : i.nn()) && t.lY.ir && t.lY.ir.length > 0 && t.bqY();
});
};
return e;
}(p.$gq);
i.$ki = g;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../../main/src/$KM": void 0,
"../../../main/src/tbl/TblEnum": void 0,
"../../battleLogic/component/$NJ": void 0,
"../../battleLogic/component/$NL": void 0,
"../render/$OU": "$OU",
"../render/$OV": "$OV",
"../render/$OW": "$OW"
} ],
$ON: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b2b5c8GSCBLlZ1Gw+yq++06", "$ON");
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
i.$kj = void 0;
var r = t("../../battleLogic/$KC"), s = t("../../battleLogic/component/$MX"), a = t("../../battleLogic/component/$NJ"), l = t("../$KH"), c = t("../../battleLogic/component/$MR"), h = t("../../battleLogic/component/$MS"), b = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bre = cc.v2();
e.brf = !0;
return e;
}
e.prototype.ma = function() {};
e.prototype.mb = function() {};
e.prototype.mc = function() {
for (var t, i, o, n, s, l, c, h = this.lZ.ng, b = this.lZ.nI(), u = h.gt(a.default), p = this.lZ.mH ? 0 : 1, f = 0, d = 0, v = 0, y = 0, g = 0, m = 0, _ = !0, $ = 0, P = 0, w = !1, I = 0, O = h.fw.bmW; I < O.length; I++) {
var E = O[I];
if (!E.hJ && (c = this.lY.iO(E))) {
$ = b * (E = c.Iu() || E).hy.worldTimeScale * E.hy.hitPauseTimeScale * E.hy.hTimeScale * p;
s = E.hw;
f = (l = E.hz).bli;
d = l.blj;
v = l.blk;
y = l.bll;
g = l.blm;
m = 0;
var x = E.hE;
w = (null === (t = E.hv) || void 0 === t ? void 0 : t.bgK) > 0;
P = s.z;
if (x && x.bka && r.default.EW(E)) {
var S = null !== (i = x.bkb.z) && void 0 !== i ? i : 600, M = x.bkb.height - P;
v = Math.abs(M) > 8 ? M > 0 ? S : -S : l.blk = 0;
} else m = !s.bmJ && (null === (o = E.hv) || void 0 === o ? void 0 : o.bhj) && !(null == x ? void 0 : x.bkC) ? l.bln + -(null === (n = E.hv) || void 0 === n ? void 0 : n.bhj) * E.hE.bkf : l.bln;
_ = E.ic;
if (0 === f && 0 === y || w) c.HQ = s.x; else {
l.blp > 10 && (y = -Math.sign(f) * l.blp);
c.HQ = s.x + f * $ + y * $ * $ / 2;
}
c.HR = 0 === d && 0 === g || w ? s.y : s.y + d * $ + g * $ * $ / 2;
if (0 !== v || 0 !== m) {
$ = b * E.hy.worldTimeScale * E.hy.hitPauseTimeScale * E.hy.vTimeScale * p;
c.HS = s.z + v * $ + m * $ * $ / 2;
} else c.HS = s.z;
_ && e.brg(c, u);
if (l.blt) {
var C = l.blt;
if (1 === C.type) {
var T = C.param[0], R = C.param[1], k = C.param[2];
if (T > 0) {
var A = c.HQ - R, N = c.HR - k, D = A * A + N * N;
if (D > T * T && D > 0) {
var j = T / Math.sqrt(D);
c.HQ = R + A * j;
c.HR = k + N * j;
}
}
}
}
}
}
this.brh(b);
this.bri(b);
};
e.brg = function(t, e) {
t.HQ = Math.max(Math.min(t.HQ, e.bmv), e.bmu);
t.HR = Math.max(Math.min(t.HR, e.bmx), e.bmw);
t.HS = Math.max(Math.min(t.HS, 5e3), 0);
};
e.prototype.md = function() {};
e.prototype.me = function() {};
e.prototype.mf = function() {};
e.prototype.brh = function(t) {
for (var e, i = this.lZ.ng, o = i.fr, n = 0, r = 0, s = i.fC.bmW; r < s.length; r++) {
var a = s[r], l = a.ho(c.default);
if (l && o.bnu(a.hg) && a.hA && a.hA) {
var b = o.bnu(l.biO);
if (b && b.hD) for (var u = b.hD.bkR, p = l.biP, f = b.hw, d = 0, v = a.hA.bkZ; d < v.length; d++) {
var y = v[d], g = o.bnu(y);
if (g) {
var m = g.ho(h.default);
if (m) {
var _ = o.bnu(m.biT);
if (_ && _.hD && _.hD.bkR !== u) {
e = this.lY.iO(_);
if (_.hy) {
if (_.hv) {
var $ = _.hv;
if ($.bgK > 0) continue;
if (_.hE) {
if (_.hE.bkp) continue;
if (_.hE.bkC) continue;
}
if ($ && $.bgA) continue;
}
n = t * _.hy.worldTimeScale * _.hy.hitPauseTimeScale;
if (l.biQ) {
var P = p.x * f.bmF + f.x - e.HQ, w = (I = Math.sign(P)) * Math.min(I * P, l.biQ * n);
e.HQ = e.HQ + w;
}
if (l.biR) {
P = p.y + f.y - e.HR, w = (I = Math.sign(P)) * Math.min(I * P, l.biR * n);
e.HR = e.HR + w;
}
if (l.biS) {
P = p.z + f.z - e.HS, w = (I = Math.sign(P)) * Math.min(I * P, l.biS * n);
var I, O = e.HS + w;
O < 0 && (O = 0);
e.HS = O;
}
}
}
}
}
}
}
}
};
e.prototype.bri = function(t) {
for (var e, i = this.lZ.ng, o = 0, n = (this.bre, 0), r = i.fx.bmW; n < r.length; n++) {
var a = r[n];
if (e = this.lY.iO(a)) {
var l = a.ho(s.default);
if (l) {
var c = i.fr.bnu(l.bkG);
if (c) {
var h = this.lY.iO(c);
if (h) {
var b = h;
switch (l.bkH) {
case 0:
var u, p = l.bkI * b.HT + b.HQ, f = l.bkJ + b.HR;
u = "number" == typeof l.bkM ? l.bkM : l.bkK + b.HS;
o = l.bkL ? c.hw.bmF * l.bkL : 1;
e.HQ = p;
e.HR = f;
e.HS = u;
e.HT = o;
break;

case 1:
var d = this.lZ.mH ? 0 : 1, v = t * a.hy.worldTimeScale * a.hy.hitPauseTimeScale * a.hy.mainTimeScale * d, y = (l.bkP + l.bkN * v) * Math.PI / 180;
e.HQ = b.HQ + Math.cos(y) * l.bkO;
e.HR = b.HR + Math.sin(y) * l.bkO;
}
}
}
}
}
}
};
return e;
}(l.$gq);
i.$kj = b;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../battleLogic/$KC": void 0,
"../../battleLogic/component/$MR": void 0,
"../../battleLogic/component/$MS": void 0,
"../../battleLogic/component/$MX": void 0,
"../../battleLogic/component/$NJ": void 0
} ],
$OO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b60f61atiBOGJRERHvBSVsX", "$OO");
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
i.$kk = void 0;
var r = t("../../../game/src/perf/$TF"), s = t("../../../game/src/battle/types/$WY"), a = t("../$KH"), l = t("../render/$OZ"), c = t("../../../game/src/$KK"), h = t("../../../game/src/manager/$SN"), b = t("../../../game/src/battle/$QM"), u = t("../../../bingo/src/$JX"), p = t("../../../game/src/perfab/common/$CBS"), f = t("../render/$PJ"), d = t("../render/$PF"), v = t("../../../game/src/battle/$QA"), y = t("../render/$PE"), g = t("../../../game/src/$KL"), m = t("../../../game/src/perfab/town/$CCI"), _ = t("../../battleLogic/component/$NK"), $ = t("../../../game/src/perfab/common/$CCD"), P = t("../../battleLogic/$JZ"), w = t("../render/$OP"), I = t("../render/$PG"), O = (cc.v2(), 
new Set()), E = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.brn = -1;
return e;
}
e.prototype.brk = function(t, e, i) {
var o, n, r, a, l = null === (o = e.Ie) || void 0 === o ? void 0 : o.node;
if (l) {
var h, f = l.x, v = l.y, y = t.hD.bkR;
h = e.Ik ? 1 : y === i ? 2 : t.hS ? 3 : 2;
if (t.hL) {
if (!e.Im && t.hx.blT) {
var _ = this.lZ.nw(t.hx.blT);
if (_) {
var $ = $eZ.prefabMgr.cPB(g.default.sceneComp.RoleTopInfoRender);
$.__cvResCfg = g.default.sceneComp.RoleTopInfoRender;
this.lY._in.sceneLayer.addChild($, s.$uD.TOWN_SCENE_DEFINE.ROLE_TOP_INFO);
e.Im = $.getComponent(m.default);
(null === (n = this.lZ.my.bCX.e) || void 0 === n ? void 0 : n.c) ? e.Im.fNg("", 0) : e.Im.fNg(_.dbY.nick, t.hL.blH);
e.Im.node.setPosition(f, v + e.IB());
}
}
e.Im && e.Im.node.setPosition(f, v + e.IB());
if (t.hN && e.HJ.ng.gN) {
if (!e.Ip && (O = c.default.JE.nb.bFN(u.$fb.ResUrl.RoleTopProtectProgress))) {
e.Ip = O.getComponent(I.$kJ);
c.default.JE.ne._in.sceneLayer.addChild(e.Ip.node, s.$uD.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 2);
}
if (e.Ip) {
var P = t.hN, w = t.hv.bgO();
e.Ip.bwe(-1 !== P.bjc, (P.bjc - w) / P.bjf);
e.Ip.bwf(-1 !== P.biY, (P.biY - w) / P.bjg);
e.Ip.bwg(-1 !== P.bja, (P.bja - w) / P.bjh);
e.Ip.node.setPosition(f, v + e.IA() + 80);
}
}
}
if (!e.Il && (O = this.lY.ip.nb.bFN(u.$fb.ResUrl.EntityShadow))) {
e.Il = O.getComponent(p.default);
this.lY._in.sceneLayer.addChild(e.Il.node, s.$uD.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
e.Il.fGT(h, null === (r = e.HM) || void 0 === r ? void 0 : r.shadowSize);
O.setPosition(f, (O.y, b.default.bGR.bHJ(e.HR, 0)));
}
if ((null === (a = e.Il) || void 0 === a ? void 0 : a.node) && e.Ie.btX) {
(O = e.Il.node).setPosition(f, cc.misc.lerp(O.y, b.default.bGR.bHJ(e.HR, 0), .37));
O.opacity = e.Ie.bub;
}
if (!e.In) {
var O;
if (O = c.default.JE.nb.bFN(u.$fb.ResUrl.RoleTopContent)) {
e.In = O.getComponent(d.default);
c.default.JE.ne._in.sceneLayer.addChild(e.In.node, s.$uD.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 1);
}
}
if (e.In) {
e.In.node.setPosition(f, b.default.bGR.bHJ(e.HR, e.HS + e.IA()));
e.In.bwb(e.Io);
}
}
};
e.prototype.ma = function() {
this.brn = -1;
this.lZ.nk.bBd(s.$uD.$vA.F, this.bro, this);
this.lZ.nk.bBd(s.$uD.$vA.G, this.brl, this);
this.lZ.nk.bBd(s.$uD.$vA.bq, this.brm, this);
this.lZ.nk.bBd(s.$uD.$vA.M, this.brq, this);
this.lZ.nk.bBd(s.$uD.$vA.N, this.brr, this);
this.lZ.nk.bBd(s.$uD.$vA.O, this.brs, this);
this.lZ.nk.bBd(s.$uD.$vA.bo, this.brv, this);
this.lZ.nk.bBd(s.$uD.$vA.bp, this.brw, this);
this.lZ.nk.bBd(s.$uD.$vA.bm, this.bry, this);
this.lZ.nk.bBd(s.$uD.$vA.n, this.brz, this);
this.lZ.nk.bBd(s.$uD.$vA.bd, this.brA, this);
this.lZ.nk.bBd(s.$uD.$vA.bs, this.brB, this);
$eZ.eventMgr.kn(c.default.JC.cGn, this.brx, this);
$eZ.eventMgr.kn(c.default.JC.cGp, this.brx, this);
this.brj = this.brk;
};
e.prototype.mb = function() {
this.brn = -1;
this.lZ.nk.bBf(this);
this.lZ.nl.bBf(this);
$eZ.eventMgr.kp(this);
};
e.prototype.mc = function(t) {
var e, i, o, n, c, h, d, v = this, y = this.lY._in;
if (y) for (var g = this.lZ, m = (g.nx(), null !== (i = null === (e = g.ny()) || void 0 === e ? void 0 : e.dcf) && void 0 !== i ? i : 0), $ = g.ng.fv.bmW, I = this.lY.iA > 0, E = this.lZ.mH ? 0 : 1, x = this.lY.iv, S = function(e) {
var i = M.lY.iO(e);
i.HY = e.hw.bmH;
var $ = i.Ie, S = i.Iu() || e, C = S.hE, T = S.hF, R = S.hv, k = S.hy.worldTimeScale * E, A = k * S.hy.hitPauseTimeScale * S.hy.mainTimeScale;
(null == R ? void 0 : R.bgf) && (A = 0);
if (!$) {
if (!(Q = g.nb.bFN(u.$fb.ResUrl.EntityRender))) return "continue";
$ = Q.getComponent(l.$kB);
y.sceneLayer.addChild(Q);
i.Ie = $;
$.bue(i, T.bik, g.nb);
$.node.setPosition(i.HQ, b.default.bGR.bHJ(i.HR, i.HS));
$.bul(i.HU);
if (e.hL) {
var N = $.buV(), D = b.default.bGR.bHZ(0, e.hL.blL, e.hL.blM, e.hL.blN);
if (null == N ? void 0 : N.bsy) {
N.bsy(D.models, 2);
$.bvb(D.weaponUpgrade);
}
i.Ih = e.hL.blK;
}
}
var j = $.node;
if ($.buf !== A) {
$.buh(A);
i.Ib.forEach(function(t) {
t.HE && t.HC && t.HE.setTimeScale(A * t.HB);
});
i.Ic.forEach(function(t) {
t.HE && t.HE.setTimeScale(A * t.HB);
});
}
if ($.bug !== k) {
$.bug = k;
i.Ib.forEach(function(t) {
t.HE && !t.HC && t.HE.setTimeScale(k * t.HB);
});
i.Id.forEach(function(t) {
t.HE && t.HE.setWorldTimeScale(k);
});
}
var H = null == C ? void 0 : C.bjx, B = !1;
if (C) {
B = !!(null === (st = C.bjx) || void 0 === st ? void 0 : st.move);
var L = C.bkh;
L && $.buE(L, C.bki);
}
var F = 1;
i.HT = e.hw.bmF;
if ((null == H ? void 0 : H.xzAngleSyncMove) && S.hz) {
if (0 !== S.hz.blk && 0 !== S.hz.bli) {
var Z = Math.atan2(S.hz.blk, S.hz.bli);
$.bul(180 * Z / Math.PI);
}
} else $.bul(i.HU);
$.bum(i.HT);
var Y = i.HQ, J = b.default.bGR.bHJ(i.HR, i.HS);
if (!i.Ii || !x) {
if (M.lZ.mz) if (B) F = .32; else {
var X = S.hz, q = X ? X.bli * X.bli + X.blj * X.blj : 0;
F = .2 + Math.min(q / M.lY.ix, .4);
} else F = B ? .5 : 1;
if (F < 1) {
Y = cc.misc.lerp(j.x, Y, F);
J = cc.misc.lerp(j.y, J, F);
}
}
j.x !== Y && (j.x = Y);
j.y !== J && (j.y = J);
if (i.HY === s.$uD.TOWN_SCENE_DEFINE.ROLE_ZORDER) {
if (!r.HeatIsolate.cyX) {
var z = b.default.bGR.bHU(i.HR + T.bio);
j.zIndex !== z && (j.zIndex = z);
}
} else j.zIndex !== i.HY && (j.zIndex = i.HY);
var V = i.Ir;
if (V) switch (V.Jf) {
case 1:
var K = $.node.x, U = $.node.y, G = K - V.Jh.x, W = U - V.Jh.y;
if (G * G + W * W > V.Ji * V.Ji) {
V.Jh.x = K;
V.Jh.y = U;
var Q = $.node;
M.lY.jM($.mainNode, Q.x, Q.y, Q.zIndex, V);
}
break;

case 2:
V.Jg += t;
if (V.Jg > V.Ji) {
V.Jg -= V.Ji;
Q = $.node;
M.lY.jM($.mainNode, Q.x, Q.y, Q.zIndex, V);
}
}
var tt = i.Ia && !I ? 255 : 0;
j.opacity !== tt && (j.opacity = tt);
if ($.buA() && (null == (N = $.buV()) ? void 0 : N.bsy) && T.biq > 0 && T.biq !== i.HP) {
i.HP = T.biq;
N.bsy(T.bip.map(function(t) {
return t.modelId;
}), 1e3);
N.bsA();
}
if (R) {
$.bus(R.bgB > 0);
var et = 0;
R.bgI && (R.bgA > 0 ? et = 2 : R.bgz > 0 && (et = 3));
$.bux(et);
O.clear();
if (C) for (var it = 0, ot = C.bjB; it < ot.length; it++) {
var nt = ot[it];
if (nt.DF.vfx) for (var rt = 0, st = nt.DF.vfx; rt < st.length; rt++) {
var at = st[rt];
O.add(at);
}
}
i.Ic.forEach(function(t, e) {
if (O.has(e)) O.delete(e); else {
var o = i.Ic.get(e);
if (o.HE) {
i.HJ.nb.bFX(o.HD, o.HE.node);
o.HE = null;
}
i.Ic.delete(o.Hx);
}
});
O.forEach(function(t) {
var o, n, r = i.Ic.get(t);
if (!r || !r.HE) {
var s = b.default.bGR.bHA(t);
if (s && (!s.visOnlySelf || v.lY.iW(e))) {
if (!r) {
(r = new a.$gs()).Hx = s.id;
r.HH = null !== (o = s.sx) && void 0 !== o ? o : 1;
r.HI = null !== (n = s.sy) && void 0 !== n ? n : 1;
s.sxEx && (r.HF = P.default.Dl(s.sxEx, e, null));
s.syEx && (r.HG = P.default.Dl(s.syEx, e, null));
i.Ic.set(r.Hx, r);
}
$.bur(r, s, s.ignoreDir ? $.node : $.dirNode, !0);
}
}
});
}
i.Ih && i.IE(t);
if (e.hD && e.hv) M.brj && M.brj(e, i, m); else {
var lt = e.ho(_.default);
if (i.Ie) if (lt) {
if (!i.Il && (Q = M.lY.ip.nb.bFN(u.$fb.ResUrl.EntityShadow))) {
i.Il = Q.getComponent(p.default);
M.lY._in.sceneLayer.addChild(i.Il.node, s.$uD.TOWN_SCENE_DEFINE.SHADOW_ZORDER + 1);
i.Il.fGT(2, lt.bmB);
}
i.Il.fGS !== lt.bmB && i.Il.fGT(2, lt.bmB);
i.Il.node.setPosition(i.Ie.node.x + lt.bmC * i.HT, b.default.bGR.bHJ(i.HR, 0));
} else i.Il && i.IG();
}
i.Ib.forEach(function(t) {
if (t.Hy.length) {
for (var i = 0, o = 0; o < t.Hy.length; o++) {
var n = t.Hy[o];
n.visOnlySelf && !v.lY.iW(e) || $.bur(t, n, n.ignoreDir ? $.node : $.dirNode, n.syncMainTime) || (t.Hy[i++] = n);
}
t.Hy.length = i;
}
});
if (C) {
O.clear();
var ct = C.bjW;
if (ct.length > 0) for (var ht = 0; ht < ct.length; ht++) {
var bt = ct[ht];
O.add(bt.id);
var ut = i.Id.get(bt.id);
if (!ut) {
(ut = new a.$gs()).Hx = bt.id;
i.Id.set(ut.Hx, ut);
}
if (!ut.HE) {
$.bur(ut, bt.vfxInfo, y.sceneLayer, !1);
if (ut.HE) if (null === (o = bt.vfxInfo.warningArea) || void 0 === o ? void 0 : o.lockLine) {
var pt = ut.HE;
pt && pt.bwO && pt.bwO(S);
} else {
ut.HE.node.removeComponent(f.default);
ut.HE.node.addComponent(f.default).bwo(null !== (n = bt.vfxInfo.x) && void 0 !== n ? n : 0, null !== (c = bt.vfxInfo.y) && void 0 !== c ? c : 0, null !== (h = bt.vfxInfo.sx) && void 0 !== h ? h : 1, null !== (d = bt.vfxInfo.sy) && void 0 !== d ? d : 1);
}
}
}
i.Id.forEach(function(t, e) {
if (!O.has(e)) {
var o = i.Id.get(e);
if (o.HE) {
i.HJ.nb.bFX(o.HD, o.HE.node);
o.HE = null;
}
i.Id.delete(o.Hx);
}
});
if (C.bjY.type > 0) {
if (!i.Iq && (Q = M.lY.ip.nb.bFN(u.$fb.ResUrl.BattleCastBar))) {
i.Iq = Q.getComponent(w.default);
M.lY._in.sceneLayer.addChild(i.Iq.node, s.$uD.TOWN_SCENE_DEFINE.ROLE_TOP_INFO + 3);
}
if (i.Iq) {
i.Iq.brF(C.bjY.type, C.bjY.time, C.bjY.speed, k);
i.Iq.node.setPosition($.node.x, $.node.y + i.IC());
}
} else i.Iq && i.Iq.brH();
}
}, M = this, C = 0, T = $; C < T.length; C++) S(T[C]);
};
e.prototype.md = function(t) {
this.lY.iq.forEach(function(e) {
e.Ib.forEach(function(e) {
e.Hz > t && (e.HA = 0);
});
});
};
e.prototype.me = function() {
this.lY.iq.forEach(function(t) {
t.Ie;
t.Ib.forEach(function(e) {
0 === e.HA && t.Iy(e);
});
});
};
e.prototype.mf = function() {};
e.prototype.brl = function(t, e) {
var i = this.lY.iO(t), o = i.Ib.get(e);
o && i.Iy(o);
};
e.prototype.brm = function(t, e) {
var i = this.lY.iO(t), o = i.Id.get(e);
o && i.Iy(o);
};
e.prototype.bro = function(t, e) {
var i, o;
if (0 !== e.mount) {
var n = this.lY.iO(t);
if (n && (!this.lZ.mE || !n.Ij)) {
var r = n.Ib.get(e.id);
if (e.destroy) r && n.Iy(r); else if (e.ignoreTeamVfxSwitch || !$eZ.localMgr.cmD(h.$oI.i) || !this.lY.iX(t)) {
if (r) this.lZ.mW || r.Hy.push(e); else {
(r = new a.$gs()).Hx = e.id || this.brn--;
r.Hy.length = 0;
n.Ib.set(r.Hx, r);
r.Hy.push(e);
}
r.HH = null !== (i = e.sx) && void 0 !== i ? i : 1;
r.HI = null !== (o = e.sy) && void 0 !== o ? o : 1;
e.sxEx && (r.HF = P.default.Dl(e.sxEx, t, null));
e.syEx && (r.HG = P.default.Dl(e.syEx, t, null));
r.Hz = this.lZ.ng.fi;
r.HA = 1;
}
}
}
};
e.prototype.brp = function(t, e) {
if (0 !== e.mount) {
var i = this.lY.iO(t);
if (i) {
var o = i.Ib.get(e.id);
if (e.destroy) o && i.Iy(o); else {
if (!o) {
(o = new a.$gs()).Hx = e.id || this.brn--;
o.Hy.length = 0;
i.Ib.set(o.Hx, o);
o.Hy.push(e);
}
o.Hz = this.lZ.nh.fi;
o.HA = 1;
}
}
}
};
e.prototype.brq = function(t, e) {
if (!this.lZ.mV && !this.lZ.mW) {
var i = this.lY.iP(null == t ? void 0 : t.hg);
i && (this.lZ.mE && i.Ij || this.bru(i, e));
}
};
e.prototype.brr = function(t, e) {
var i = this.lY.iO(t);
i && i.Ie && i.Ie.buG(e.time, e.val);
};
e.prototype.brs = function(t, e) {
var i = this.lY.iO(t);
i && i.Ie && i.Io.set(e.effId, e);
};
e.prototype.brt = function(t, e) {
if (!this.lZ.mV) {
var i = this.lY.iP(null == t ? void 0 : t.hg);
i && (null == i ? void 0 : i.Ij) && this.bru(i, e);
}
};
e.prototype.bru = function(t, e) {
var i, o, n, r, s, l, c, h, b, u;
if (e) {
var p = t.Ir;
p || (p = t.Ir = new a.default());
if (p.Je !== e) {
p.Je = e;
var f = e.mode || 1;
p.Jf = f;
p.Jj.r = null !== (i = e.r) && void 0 !== i ? i : 255;
p.Jj.g = null !== (o = e.g) && void 0 !== o ? o : 255;
p.Jj.b = null !== (n = e.b) && void 0 !== n ? n : 255;
p.Jk = null !== (r = e.a) && void 0 !== r ? r : 200;
p.Jl = null !== (s = e.size) && void 0 !== s ? s : 660;
p.Jn = 280;
p.Jo = null !== (l = e.shaderType) && void 0 !== l ? l : 0;
p.Jp = null !== (c = e.blend) && void 0 !== c ? c : 0;
p.Jq = null !== (h = e.offsetZIndex) && void 0 !== h ? h : 0;
p.Jr = null !== (b = e.t1) && void 0 !== b ? b : .05;
p.Js = null !== (u = e.t2) && void 0 !== u ? u : .12;
switch (f) {
case 1:
p.Jh.x = 999999;
p.Jh.y = 999999;
p.Ji = e.param || 130;
break;

case 2:
var d = e.param || .07;
p.Jg = d + 1e-6;
p.Ji = d;
}
}
} else t.Ir && (t.Ir = null);
};
e.prototype.brv = function(t, e) {
var i = this.lY.iO(t);
i && i.Ie && i.Ie.buF(e.name, e.visible);
};
e.prototype.brw = function(t, e) {
this.lY.iO(t).Ia = e.visible;
};
e.prototype.brx = function() {
var t;
null === (t = this.lY) || void 0 === t || t.iq.forEach(function(t) {
if (t.Ie) {
t.Ie.buL = $eZ.localMgr.cmD(h.$oI.h);
t.Ie.buQ = $eZ.localMgr.cmD(h.$oI.g);
}
});
};
e.prototype.bry = function(t, e) {
var i, o = this, n = t.hg, r = null === (i = this.lY) || void 0 === i ? void 0 : i.iP(t.hg);
if (r && t.hE) {
var s = t.hw, l = t.hE.bjJ[e.varX] || 0, c = t.hE.bjJ[e.varY] || 0, h = l, u = b.default.bGR.bHI(c) + e.offsetY, p = s.x, f = b.default.bGR.bHJ(s.y, s.z) + e.offsetY, d = Math.floor((u + s.y) / 2), g = p - h, m = f - u;
Math.abs(g) < 5 && (g = p - (h = s.x + -20 * s.bmF));
var _ = e.extendLength || 0, $ = cc.v2(g, m).negSelf().normalizeSelf().mul(.5 * _), P = Math.sqrt(g * g + m * m) + _, w = 180 * Math.atan2(m, g) / Math.PI, I = Math.abs(w) > 90 ? -1 : 1;
-1 == I && (w = Math.sign(w) * (180 - Math.abs(w)));
var O = e.height || 20;
e.heightRandAdd && (O += Math.random() * e.heightRandAdd);
var E = b.default.bGR.bHV(v.DISPLAY_RES_TYPE.MOVIE_CLIP), x = this.lZ.nb.bFN(E);
if (x) {
var S = new a.$gs();
S.Hx = this.brn--;
S.Hy.length = 0;
r.Ib.set(S.Hx, S);
x.x = h + $.x;
x.y = u + $.y;
x.scaleX = 1 * I;
x.scaleY = 1;
x.angle = 0;
var M = S.HE = x.getComponent(y.default), C = (this.lZ.mH ? 0 : 1) * t.hy.worldTimeScale, T = b.default.bGR.bHU(d - 2);
this.lY._in.sceneLayer.addChild(x, T);
M.sprite.spriteFrame = null;
M.sprite.sizeMode = cc.Sprite.SizeMode.CUSTOM;
M.sprite.node.angle = w;
M.sprite.node.width = P;
M.sprite.node.height = O;
M.setTimeScale(C);
M.bvW(e.resId, !1, function() {
var t, e;
if (M.isValid) {
M.sprite.sizeMode = cc.Sprite.SizeMode.RAW;
M.sprite.node.angle = 0;
var i = null === (t = o.lY) || void 0 === t ? void 0 : t.iP(n);
if (i) i.Iy(S); else {
S.HE = null;
null === (e = M.node) || void 0 === e || e.destroy();
}
}
});
}
}
};
e.prototype.brz = function() {
var t;
null === (t = this.lY) || void 0 === t || t.iq.forEach(function(t) {
t.Ig && t.Ih && t.If && (t.Ig.f = 1);
});
};
e.prototype.brA = function(t, e) {
var i, o = null === (i = this.lY) || void 0 === i ? void 0 : i.iP(t.hg);
(null == o ? void 0 : o.Ie) && o.Ie.buZ($.$Qp.c, e[0]);
};
e.prototype.brB = function(t) {
var e, i = null === (e = this.lY) || void 0 === e ? void 0 : e.iP(t.hg);
i && this.lY.iQ(i);
};
return e;
}(a.$gq);
i.$kk = E;
cc._RF.pop();
}, {
"../$KH": "$KH",
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/$KL": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../../game/src/manager/$SN": void 0,
"../../../game/src/perf/$TF": void 0,
"../../../game/src/perfab/common/$CBS": void 0,
"../../../game/src/perfab/common/$CCD": void 0,
"../../../game/src/perfab/town/$CCI": void 0,
"../../battleLogic/$JZ": void 0,
"../../battleLogic/component/$NK": void 0,
"../render/$OP": "$OP",
"../render/$OZ": "$OZ",
"../render/$PE": "$PE",
"../render/$PF": "$PF",
"../render/$PG": "$PG",
"../render/$PJ": "$PJ"
} ],
$OP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "e0d0cLHNp5N16d7JJ2Welcf", "$OP");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.skeleton = null;
e.brC = 0;
e.brD = 1;
e.brE = 0;
return e;
}
e.prototype.ks = function() {
this.skeleton.setCompleteListener(this.brG.bind(this));
};
e.prototype.brF = function(t, e, i, o) {
if (0 !== t) {
if (this.brC !== t) {
this.brE = e;
this.brC = t;
switch (t) {
case 1:
this.skeleton.setAnimation(0, "laoding", !1);
break;

case 2:
this.skeleton.setAnimation(0, "laoding3", !1);
}
this.brD = Math.min(Math.max(1 / e, .01), 100);
}
this.node.active = this.brC > 0;
this.skeleton.timeScale = i * o * this.brD;
}
};
e.prototype.brG = function() {
2 === this.brC && this.skeleton.setAnimation(0, "laoding4", !0);
};
e.prototype.brH = function() {
if (0 !== this.brC) {
this.brC = 0;
this.node.active = !1;
}
};
e.prototype.reuseSelf = function() {
this.brC = 0;
};
r([ c(sp.Skeleton) ], e.prototype, "skeleton", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$OQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "01e74SrYoxFK4IVa8EMFfbG", "$OQ");
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
var s = t("../../../bingo/src/$JX"), a = t("../ui/$PM"), l = cc._decorator, c = l.ccclass, h = l.property, b = function(t) {
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
e.brI = new a.$kW();
e.brJ = "";
return e;
}
e.prototype.ks = function() {
this.brK();
};
e.prototype.brK = function() {
this.brI.bxh || this.brI.bxg(this.buffList ? this.buffList.node : null, this.buffTemplate);
};
e.prototype.brL = function(t) {
this.brK();
this.brN(t.c);
this.brO(t.d, t.e);
this.brI.bxi(t.f || []);
this.brM(t.b);
this.lblLevel && (this.lblLevel.string = t.g.toString());
this.lblName && (this.lblName.string = t.h);
};
e.prototype.brM = function(t) {
this.networkStatusNodeList && this.networkStatusNodeList.forEach(function(e, i) {
e.active = i === t;
});
};
e.prototype.brN = function(t) {
if (this.avatar && t && this.brJ !== t) {
this.brJ = t;
this.lE(this.avatar, t);
}
};
e.prototype.brO = function(t, e) {
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
return r([ c ], e);
}(s.$fb.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../ui/$PM": "$PM"
} ],
$OR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "bc315BXFNZPo58wcXXAqKDU", "$OR");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.avatar = null;
e.lblName = null;
e.lblLevel = null;
e.damageProgressBar = null;
e.lblDamagePercent = null;
e.lblDamageValue = null;
e.brP = "";
e.networkStatusNodeList = [];
return e;
}
e.prototype.brQ = function(t) {
this.brR(t.b);
this.lblName && (this.lblName.string = t.c || "");
this.brS(t.f);
var e = cc.misc.clampf(t.e, 0, 1);
this.damageProgressBar && (this.damageProgressBar.progress = e);
this.lblDamagePercent && (this.lblDamagePercent.string = (100 * e).toFixed(1) + "%");
this.lblDamageValue && (this.lblDamageValue.string = s.$fb.utils.beX(Math.max(0, Math.floor(t.d))));
this.lblLevel && (this.lblLevel.string = t.g.toString());
};
e.prototype.brR = function(t) {
if (this.avatar && t && this.brP !== t) {
this.brP = t;
this.lE(this.avatar, t);
}
};
e.prototype.brS = function(t) {
this.networkStatusNodeList && this.networkStatusNodeList.forEach(function(e, i) {
e.active = i === t;
});
};
r([ c(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ c(cc.Label) ], e.prototype, "lblName", void 0);
r([ c(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ c(cc.ProgressBar) ], e.prototype, "damageProgressBar", void 0);
r([ c(cc.Label) ], e.prototype, "lblDamagePercent", void 0);
r([ c(cc.Label) ], e.prototype, "lblDamageValue", void 0);
r([ c([ cc.Node ]) ], e.prototype, "networkStatusNodeList", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$OS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "28e39LvYH1Ca4nQ0pjQWDC1", "$OS");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$kq = void 0;
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/battle/$QM"), h = t("../../../game/src/perfab/common/$CBU"), b = t("../../../game/src/perfab/common/$CCB"), u = t("../../../main/src/$KM"), p = t("../../../game/src/utils/$TY"), f = t("./$PI"), d = t("../../../game/src/perf/$TF"), v = cc._decorator, y = v.ccclass, g = (v.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = u.cvResUrl.changeableSpine;
e.brU = [];
e.brV = new Map();
e.brW = !1;
e.brX = null;
e.brY = 0;
e.brZ = 0;
e.bsa = !1;
e.bsb = [];
e.bsc = new Map();
e.bsd = 1;
e.bsv = [];
e.bsw = -1;
e.bsx = new Map();
e.bsO = null;
return e;
}
e.prototype.onLoad = function() {
this.bsd = this.sp.node.scale;
};
e.prototype.ky = function() {
t.prototype.ky.call(this);
this.bsc.clear();
this.kG();
};
e.prototype.kG = function() {
this.bse();
this.sp && this.sp.isValid && this.sp.skeletonData === this.brT && (this.sp.skeletonData = null);
this.brT && this.brT.isValid && this.brT.destroy();
this.bsc.clear();
this.brT = null;
this.brV.clear();
this.bsw = -1;
this.bsv.length = 0;
this.bsx.clear();
this.bsa = !1;
this.bsb.length = 0;
this.bsN();
};
e.prototype.bse = function() {
this.brV.forEach(function(t) {
b.$Qf.fLA(t.k);
});
};
e.prototype.reuseSelf = function() {
t.prototype.reuseSelf.call(this);
this.kG();
};
e.prototype.kH = function(t) {
this.bsa && this.bsf();
if (0 !== this.bsb.length) for (var e = 0, i = this.bsb; e < i.length; e++) {
var o = i[e], n = o.b;
if (b.$Qf.fLE(n, t)) if (n.b === b.$Qk.e) this.bsp(o.c, !1); else for (var r = n.c[n.e], s = 0, a = o.c; s < a.length; s++) {
var l = a[s];
this.bsm(l, r);
}
}
};
e.prototype.bsf = function() {
var t = this;
this.bsa = !1;
this.bsb.length = 0;
var e = new Map();
this.brV.forEach(function(i) {
var o = i.k;
if (b.$Qf.fLz(o)) {
var n = e.get(o);
if (!n) {
var r = {
b: o,
c: []
};
e.set(o, r);
t.bsb.push(r);
n = r;
}
n.c.push(i);
}
});
};
e.prototype.bsg = function() {
var t = [];
this.brV.forEach(function(e) {
t.push(e);
});
return t;
};
e.prototype.bsh = function(t) {
var e = [];
if (!t) return e;
this.brV.forEach(function(i) {
i.k === t && e.push(i);
});
return e;
};
e.prototype.bsi = function(t) {
var e = t.k;
t.k = b.$Qf.fLx();
t.j = null;
e && (this.bsh(e).length > 0 || b.$Qf.fLA(e));
};
e.prototype.bsj = function(t) {
var e = this.sp.animation, i = this.sp.loop, o = new sp.SkeletonData();
this.brT = o;
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
this.bsL() && s && s[0] && this.bsM(s[0]) && (o.textures[0] = this.brX);
p.$sk.cHz(this.sp, p.$sk.cHu);
this.sp.skeletonData = o;
this.brT.addRef();
this.sp.animation = e;
this.sp.loop = i;
};
e.prototype.ms = function(t) {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF4(){this.bsj(t);
this.bsA();
return [ 2 ];
});
});
};
e.prototype.bsk = function(t, e) {
if (this.sp) return this.sp.getAttachment(t, e);
};
e.prototype.bsl = function(t, e) {
if (t && e && t.e !== e) {
t.e = e;
this.bsm(t, e);
}
};
e.prototype.bsm = function(t, e, i) {
void 0 === i && (i = !1);
if (this.sp && e) {
var o = t.k, n = t.j;
if (i || n !== e) {
var r = e.getTexture();
if (r) {
r.packable = !1;
t.j = e;
var s = t.b;
cc.sys.isNative ? this.bsS(t, e, s.slot, s.attachment) : b.$Qf.fLy(o) ? o.b === b.$Qk.c ? this.bso(t, e) : this.bsr(t, e) : this.brW ? this.bss(t, e) : this.bsn(t, e, n) || this.bsr(t, e);
}
}
}
};
e.prototype.bsn = function(t, e, i) {
var o = t.c;
if (!o || !o.texture) return !1;
if (o === t.d) return !1;
if (!i) return !1;
if (i.isRotated() !== e.isRotated()) return !1;
var n = i.getRect(), r = e.getRect();
if (n.width !== r.width || n.height !== r.height) return !1;
var s = i.getOriginalSize(), a = e.getOriginalSize();
if (s.width !== a.width || s.height !== a.height) return !1;
var l = i.uv, c = e.uv;
if (!l || !c || l.length !== c.length) return !1;
for (var h = 0; h < c.length; h++) if (l[h] !== c[h]) return !1;
var b = this.bsk(t.b.slot, t.b.attachment);
if (!b) return !1;
b.width = o.width;
b.height = o.height;
o.texture.setRealTexture(e.getTexture());
return !0;
};
e.prototype.bso = function(t, e) {
var i = t.b, o = this.bsk(i.slot, i.attachment);
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
var l = e.getRect(), c = e.getOriginalSize(), h = e.getOffset(), b = e.isRotated(), u = e.uv;
if (u && u.length >= 8) if (b) {
s.u = u[4];
s.v = u[3];
s.u2 = u[2];
s.v2 = u[5];
} else {
s.u = u[0];
s.v = u[7];
s.u2 = u[6];
s.v2 = u[1];
}
s.rotate = b;
s.width = l.width;
s.height = l.height;
s.originalWidth = c.width;
s.originalHeight = c.height;
s.offsetX = (c.width - l.width) / 2 + h.x + (i.offsetX || 0);
s.offsetY = (c.height - l.height) / 2 + h.y + (i.offsetY || 0);
if (o instanceof sp.spine.MeshAttachment) {
o.region = s;
o.updateUVs();
} else {
o.width = c.width * (i.scaleX || 1);
o.height = c.height * (i.scaleY || 1);
o.setRegion(s);
o.updateOffset();
}
}
} else $eZ.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bsp = function(t, e) {
var i = this;
if (t.length) {
var o = t[0].k, n = o.c[o.e];
if (n) if (!e || b.$Qc.fKW()) {
o.u = 0;
o.p = o.e;
o.q = o.j;
b.$Qe.fLq(o, n, function(t) {
if (i.isValid && i.sp && o.b === b.$Qk.e) {
var e = i.bsh(o);
if (e.length) if (t) for (var n = 0, r = e; n < r.length; n++) {
var s = r[n];
i.bsm(s, o.o);
} else i.bsq(e);
}
}) || this.bsq(t);
} else {
o.u += 1;
o.u >= b.$Qf.fLu && this.bsq(t);
}
}
};
e.prototype.bsq = function(t) {
if (t.length) {
var e = t[0].k;
b.$Qf.fLB(e);
e.h = null;
e.b = b.$Qk.c;
this.bsa = !0;
for (var i = 0, o = t; i < o.length; i++) {
var n = o[i];
this.bsm(n, e.c[e.e], !0);
}
}
};
e.prototype.bsr = function(t, e) {
var i = t.b, o = this.bsk(i.slot, i.attachment);
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
var a = e.isRotated(), l = a ? r.height : r.width, c = a ? r.width : r.height, h = e.uv;
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
var b = e.getRect();
s.u = b.x / n.width;
s.u2 = (b.x + b.width) / n.width;
s.v = (n.height - b.y - b.height) / n.height;
s.v2 = (n.height - b.y) / n.height;
}
s.width = l * (i.scaleX || 1);
s.height = c * (i.scaleY || 1);
s.originalWidth = l;
s.originalHeight = c;
s.rotate = a;
s.offsetX = i.offsetX || 0;
s.offsetY = i.offsetY || 0;
s.renderObject = s;
var u = new sp.SkeletonTexture({
width: n.width,
height: n.height
});
u.setRealTexture(n);
s.texture = u;
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
} else $eZ.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bss = function(t, e) {
var i = t.b, o = this.bsk(i.slot, i.attachment);
if (o) {
if (!t.d) {
var n = o.region;
n && n.page && (t.d = n);
}
var r = t.d;
if (r) if (r.rotate) this.bsr(t, e); else {
var s = r.x, a = r.y, l = r.originalWidth, c = r.originalHeight, h = e.getOriginalSize();
if (h.width * h.height / (l * c) > 2) this.bsr(t, e); else {
if (null == s || null == a) {
s = Math.round(r.u * this.brY);
a = Math.round(r.v * this.brZ);
l = Math.round((r.u2 - r.u) * this.brY);
c = Math.round((r.v2 - r.v) * this.brZ);
}
var b = a, u = e.getOriginalSize();
o.width = u.width;
o.height = u.height;
if (this.bsR(e, s, b, l, c)) {
if (o.region !== r) if (o instanceof sp.spine.MeshAttachment) {
o.region = r;
o.updateUVs();
} else {
o.setRegion(r);
o.updateOffset();
} else o instanceof sp.spine.MeshAttachment ? o.updateUVs() : o.updateOffset();
t.c = r;
} else {
cc.log("尺寸不匹配:", i.url, i.key || "", "设计尺寸：", l, c, "实际尺寸：", h.width, h.height);
this.bsr(t, e);
}
}
} else this.bsr(t, e);
} else $eZ.logMgr.warn("not find " + i.slot + ", " + i.attachment);
};
e.prototype.bst = function(t, e) {
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
e.prototype.bsu = function(t, e) {
var i = this.sp.findSlot(t);
if (i) if (cc.sys.isNative) i.setColorA && i.setColorA(e ? 1 : 0); else {
if (e) {
var o = this.bsc.get(t);
if (o) {
i.setAttachment(o);
this.bsc.delete(t);
} else if (!i.getAttachment()) {
var n = i.data && i.data.attachmentName;
if (n) {
var r = this.sp.getAttachment(t, n);
r && i.setAttachment(r);
}
}
} else {
if (this.bsc.has(t)) return;
var s = i.getAttachment();
s && this.bsc.set(t, s);
i.setAttachment(null);
}
i.color.a = e ? 1 : 0;
}
};
e.prototype.bsy = function(t, e) {
this.bsv = this.bsv.filter(function(t) {
return t.c !== e;
});
for (var i = 0, o = t; i < o.length; i++) {
var n = {
b: o[i],
c: e
};
this.bsv.push(n);
}
};
e.prototype.bsz = function() {};
e.prototype.bsA = function() {
for (var t, e, i = this, o = [], n = function(e) {
var i = c.default.bGR.bHw(e.b);
if (!i) return "continue";
var n = null !== (t = i.type) && void 0 !== t ? t : i.id, r = o.find(function(t) {
return t.type === n;
});
r ? r.data.c < e.c && (r.data = e) : o.push({
data: e,
type: n
});
}, r = 0, h = this.bsv; r < h.length; r++) n(d = h[r]);
for (var u = [], p = 0, f = o; p < f.length; p++) {
var d = f[p].data;
(m = c.default.bGR.bHw(d.b)) && u.push.apply(u, m.hideType);
}
for (var v = 0, y = o; v < y.length; v++) {
var g = y[v];
if (-1 === u.indexOf(g.type)) {
var m;
d = g.data;
if (m = c.default.bGR.bHw(d.b)) {
if (m.boneVis) for (var _ = 0, $ = m.boneVis; _ < $.length; _++) {
var P = $[_];
this.bst(P.name, P.visible);
}
if (m.slotVis) for (var w = 0, I = m.slotVis; w < I.length; w++) {
P = I[w];
this.bsu(P.name, P.visible);
}
for (var O = 0, E = m.skins; O < E.length; O++) {
var x = E[O], S = x.slot + "_" + x.attachment, M = this.brV.get(S);
if (!M) {
M = {
h: 0,
b: x,
c: null,
e: null,
f: "",
g: d.b,
i: null,
j: null,
k: b.$Qf.fLx()
};
this.brV.set(S, M);
}
M.h = null !== (e = m.type) && void 0 !== e ? e : 0;
var C = b.$Qf.fLw(M.b.movieClipId), T = b.$Qf.fLw(x.movieClipId);
if (M.b.url !== x.url || M.b.key !== x.key || C !== T) {
M.b = x;
M.i = null;
M.e = null;
this.bsi(M);
this.bsa = !0;
} else M.b = x;
}
}
}
}
b.$Qg.fLK(this.bsg());
this.bsa = !0;
var R = new Map();
this.brV.forEach(function(t) {
var e = t.b;
b.$Qf.fLw(e.movieClipId) || (e.key ? R.set(e.url, new l.$fb.ResCfg(e.url, cc.SpriteAtlas)) : R.set(e.url, new l.$fb.ResCfg(e.url, cc.SpriteFrame)));
});
var k = 1, A = function() {
(k -= 1) > 0 || i.isValid && i.sp && i.sp.node && (i.sp.node.scale = i.bsd);
}, N = [];
this.brV.forEach(function(t) {
b.$Qf.fLw(t.b.movieClipId) && -1 === N.indexOf(t.k) && N.push(t.k);
});
for (var D = 0, j = N; D < j.length; D++) {
var H = j[D];
k += 1;
this.bsB(H, A);
}
this.sp.node.scale = this.brV.size > 0 ? 0 : this.bsd;
this.bsD(R, function(t) {
i.isValid && i.brV.forEach(function(e) {
return s(i, void 0, void 0, function() {
var i, o, n, r, s;
return a(this,function AWAITF5(){i = e.b;
o = this.bsE(i);
e.f = o;
if (b.$Qf.fLw(i.movieClipId)) return [ 2 ];
if (n = t.get(i.url)) {
r = null;
if (i.key) {
s = n.Da;
r = s.getSpriteFrame(i.key);
} else r = n.Da;
if (r) {
this.bsl(e, r);
this.bsH(e);
}
}
return [ 2 ];
});
});
});
A();
});
};
e.prototype.bsB = function(t, e) {
var i = this, o = this.bsh(t);
if (o.length) {
var n = o[0].b, r = b.$Qf.fLw(n.movieClipId);
if (r) {
n.url;
if (t.A === r && t.c.length > 0) {
var s = o[0].h ? this.bsx.get(o[0].h) : null;
this.bsC(o, s);
e();
} else b.$Qf.fLC(t, r, function() {
return i.isValid;
}, function(o) {
if (o && i.isValid && i.sp) {
var n = i.bsh(t);
if (n.length) {
for (var r = 0, s = n; r < s.length; r++) s[r].e = t.c[0];
var a = n[0].h ? i.bsx.get(n[0].h) : null;
i.bsC(n, a);
e();
} else e();
} else e();
});
} else e();
} else e();
};
e.prototype.bsC = function(t, e) {
if (t.length) {
var i = t[0].k;
this.bsa = !0;
if (e || i.v || cc.sys.isNative) {
for (var o = 0, n = t; o < n.length; o++) (c = n[o]).i = e;
if (b.$Qf.fLD(i, e)) {
this.bsp(t, !1);
return;
}
$eZ.logMgr.warn("[ChangeableSpine] 合成目标建立失败，部位降级为逐帧 UV，slot=" + t[0].b.slot);
} else for (var r = 0, s = t; r < s.length; r++) (c = s[r]).i = null;
i.h = null;
i.b = b.$Qk.c;
for (var a = 0, l = t; a < l.length; a++) {
var c = l[a];
this.bsm(c, i.c[i.e], !0);
}
}
};
e.prototype.bsD = function(t, e) {
var i = this, o = 0, n = t.size, r = new Map();
d.HeatIsolate.cyY ? e(r) : 0 !== n ? t.forEach(function(t, s) {
i.lB(t, function(t, i) {
o++;
i && i.Da && r.set(s, i);
o == n && e(r);
});
}) : e(r);
};
e.prototype.bsE = function(t) {
if (!t) return "";
var e = b.$Qf.fLw(t.movieClipId);
return e ? "mc#" + e : t.url ? t.key ? t.url + "#" + t.key : t.url : "";
};
e.prototype.bsF = function(t, e) {
null === e ? this.bsx.delete(t) : this.bsx.set(t, e);
this.bsG();
};
e.prototype.bsG = function() {
var t = this;
this.brV.forEach(function(e) {
0 !== e.h && e.e && t.bsH(e);
});
};
e.prototype.bsH = function(t) {
var e;
if (t.h && t.e) {
var i = this.bsx.get(t.h), o = t.k;
b.$Qf.fLw(t.b.movieClipId) ? b.$Qf.fLy(o) ? this.bsK(t, i) : t.i = i : i ? i === t.i && i.mode === (null === (e = o.h) || void 0 === e ? void 0 : e.mode) || this.bsI(t, i) : this.bsJ(t);
}
};
e.prototype.bsI = function(t, e) {
var i = t.k;
t.i = e;
i.c = [ t.e ];
i.d = 0;
i.e = 0;
i.g = 0;
i.f = 0;
i.v = t.e.isRotated();
this.bsC([ t ], e);
};
e.prototype.bsJ = function(t) {
var e = t.k;
if (e.b !== b.$Qk.b || t.i || e.h) {
b.$Qf.fLB(e);
this.bsa = !0;
e.b = b.$Qk.b;
e.c = [];
e.e = 0;
e.f = 0;
e.v = !1;
t.i = null;
this.bsm(t, t.e, !0);
}
};
e.prototype.bsK = function(t, e) {
var i;
(e !== t.i || e && e.mode !== (null === (i = t.k.h) || void 0 === i ? void 0 : i.mode)) && this.bsC(this.bsh(t.k), e);
};
e.prototype.bsL = function() {
return !cc.sys.isNative && !!h.$PI.fGZ;
};
e.prototype.bsM = function(t) {
if (!t || !t.isValid) return !1;
var e = t.width, i = t.height;
if (!e || !i) return !1;
var o = new cc.RenderTexture();
o.initWithSize(e, i);
o.packable = !1;
o.setFilters(cc.Texture2D.Filter.LINEAR, cc.Texture2D.Filter.LINEAR);
o.setWrapMode(cc.Texture2D.WrapMode.CLAMP_TO_EDGE, cc.Texture2D.WrapMode.CLAMP_TO_EDGE);
this.brX = o;
this.brY = e;
this.brZ = i;
this.brW = !0;
if (!this.bsR(new cc.SpriteFrame(t), 0, 0, e, i, !0)) {
console.log("拷贝 atlas 失败");
this.bsN();
return !1;
}
return !0;
};
e.prototype.bsN = function() {
this.brX && this.brX.isValid && this.brX.destroy();
this.brX = null;
this.brW = !1;
this.brY = 0;
this.brZ = 0;
this.bsQ();
};
e.prototype.bsP = function(t, e) {
void 0 === t && (t = null);
void 0 === e && (e = 2048);
if (!this.brX || !this.brX.isValid) {
$eZ.logMgr.warn("[ChangeableSpine] showDebugAtlas: _atlasRT not available");
return null;
}
var i = t || cc.find("Canvas");
if (!this.bsO || !this.bsO.isValid) {
this.bsO = new cc.Node("DEBUG_AtlasRT");
var o = this.bsO.addComponent(cc.Sprite);
o.sizeMode = cc.Sprite.SizeMode.CUSTOM;
o.trim = !1;
}
if (this.bsO.parent !== i) {
this.bsO.removeFromParent();
i.addChild(this.bsO);
}
var n = new cc.SpriteFrame(this.brX);
this.bsO.getComponent(cc.Sprite).spriteFrame = n;
var r = this.brY, s = this.brZ;
if (e > 0) {
var a = Math.min(1, e / Math.max(r, s));
r = Math.round(r * a);
s = Math.round(s * a);
}
this.bsO.setContentSize(r, s);
this.bsO.zIndex = 9999;
return this.bsO;
};
e.prototype.bsQ = function() {
this.bsO && this.bsO.isValid && this.bsO.destroy();
this.bsO = null;
};
e.prototype.bsR = function(t, e, i, o, n, r) {
void 0 === r && (r = !1);
if (!this.brX || !this.brX.isValid) return !1;
if (!t) return !1;
var s = t.getTexture();
if (!s || !s.isValid) return !1;
var a = t.getOriginalSize(), l = t;
t.isRotated() || a.width !== s.width || a.height !== s.height || (l = new cc.SpriteFrame(s));
if (!h.$PI.fHf()) return !1;
var c = h.$PI.fHc, b = h.$PI.fHd, u = h.$PI.fHe;
if (!(c && c.isValid && u && u.isValid)) return !1;
c.orthoSize = this.brZ / 2;
c.node.setPosition(this.brY / 2, this.brZ / 2);
u.spriteFrame = l;
b.setAnchorPoint(.5, .5);
b.setContentSize(o, n);
b.setPosition(e + o / 2 - this.brY / 2, i + n / 2 - this.brZ / 2);
b.scaleY = -1;
b.active = !0;
u.srcBlendFactor = cc.macro.BlendFactor.ONE;
u.dstBlendFactor = cc.macro.BlendFactor.ZERO;
c.clearFlags = r ? cc.Camera.ClearFlags.COLOR | cc.Camera.ClearFlags.DEPTH | cc.Camera.ClearFlags.STENCIL : 0;
c.backgroundColor = cc.color(0, 0, 0, 0);
c.targetTexture = this.brX;
var p = !1;
try {
c.render(b);
p = !0;
} catch (t) {
p = !1;
console.log("blitToAtlas camera.render error", t);
} finally {
c.targetTexture = null;
b.scaleY = 1;
u.spriteFrame = null;
u.srcBlendFactor = cc.macro.BlendFactor.SRC_ALPHA;
u.dstBlendFactor = cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
}
return p;
};
e.prototype.bsS = function(t, e, i, o) {
var n = this.sp.getAttachment(i, o);
if (n) {
var r = e.getTexture();
if (r) {
var s = e.getRect(), a = e.getOffset(), l = e.isRotated(), c = new cc.Rect();
c.x = e.uv[l ? 4 : 0];
c.y = e.uv[l ? 3 : 7];
c.w = e.uv[l ? 2 : 6];
c.h = e.uv[l ? 5 : 1];
var h = {
rect: s,
origSize: s,
offset: a,
degrees: l,
uv: c
};
if (n.updateUVs) {
if (!t.d) {
var b = n.region;
b && b.page && (t.d = b);
}
var u = t.d;
if (u) {
var p = cc.rect(u.originalWidth, u.originalHeight);
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
} else $eZ.logMgr.warn("[ChangeableSpine] updateRegionSp attachment null, slot=" + i + ", att=" + o);
};
return r([ y ], e);
}(f.$kL));
i.$kq = g;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/perf/$TF": void 0,
"../../../game/src/perfab/common/$CBU": void 0,
"../../../game/src/perfab/common/$CCB": void 0,
"../../../game/src/utils/$TY": void 0,
"../../../main/src/$KM": void 0,
"./$PI": "$PI"
} ],
$OT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "db72btlVR9NgZawEg0rZJH4", "$OT");
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
i.$ku = void 0;
var s = t("../../../game/src/battle/$QA"), a = cc._decorator, l = a.ccclass, c = (a.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
return e;
}
e.prototype.ku = function() {};
e.prototype.ky = function() {};
e.prototype.setTimeScale = function() {};
e.prototype.reuseSelf = function() {
this.lN();
};
return r([ l ], e);
}(s.IVFXItem));
i.$ku = c;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0
} ],
$OU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "dcbbbHptf9PqY/HecCOqBEK", "$OU");
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
var s = t("../../../game/src/battle/$QA"), a = t("../../../game/src/battle/$QM"), l = t("../../../game/src/$KK"), c = t("../../../main/src/$KM"), h = t("./$PE"), b = cc._decorator, u = b.ccclass, p = (b.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bsT = 0;
e.bsV = null;
e.bsW = null;
e.bsX = 0;
e.bsY = 0;
e.bsZ = 0;
e.bta = 0;
e.btb = 0;
return e;
}
e.prototype.ku = function() {};
e.prototype.btc = function(t, e) {
this.bsZ = e;
this.bsX = this.node.x;
this.bsY = this.node.y;
this.bta = 400;
this.btb = 300;
this.bsT = 1;
var i = this.bsU = a.default.bGR.bHn(t.itemId);
if (i) {
if (!this.bsW) {
var o = l.default.JE.nb.bFN(c.cvResUrl.movieClip);
this.node.addChild(o, -1);
this.bsV = o;
this.bsW = o.getComponent(h.default);
}
this.bsW.node.scale = 1;
this.bsW.node.angle = 0;
this.bsW.node.setPosition(0, 0);
this.bsW.setTimeScale(1);
this.bsW.bvU(a.default.bGR.bHo(i.quality));
this.bsW.bvW(c.cvResUrl.mc_drop_PuTong_ball, !0, function() {});
}
};
e.prototype.btd = function() {};
e.prototype.kH = function(t) {
var e, i, o;
switch (this.bsT) {
case 1:
(u = this.node.y - this.bsY) > 50 ? this.bsT = 2 : this.node.y += 400 * t;
break;

case 2:
var n = null === (i = null === (e = l.default.JE) || void 0 === e ? void 0 : e.ne) || void 0 === i ? void 0 : i.iP(this.bsZ);
if (!n) {
l.default.JE.nb.bFX(c.cvResUrl.DropItemPickBall, this.node);
return;
}
var r = null === (o = n.Ie) || void 0 === o ? void 0 : o.node;
if (r) {
var s = this.node, a = r.x, h = r.y + 105, b = a - s.x, u = h - s.y, p = Math.abs(b), f = Math.abs(u), d = Math.min(t * this.bta, p), v = Math.min(t * this.btb, f);
s.x = s.x + Math.sign(b) * d;
s.y = s.y + Math.sign(u) * v;
this.bta += 3e3 * t;
this.btb += 3e3 * t;
if (p < 10 && f < 10) {
this.bsT = 3;
this.bte();
}
}
}
};
e.prototype.bte = function() {
var t = this;
this.bsW.bvW(c.cvResUrl.mc_drop_PuTong_ball_light, !1, function() {
var e;
(null === (e = t.node) || void 0 === e ? void 0 : e.isValid) && l.default.JE.nb.bFX(c.cvResUrl.DropItemPickBall, t.node);
});
};
e.prototype.reuseSelf = function() {
this.btd();
this.lN();
};
return r([ u ], e);
}(s.IVFXItem));
i.default = p;
cc._RF.pop();
}, {
"../../../game/src/$KK": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../main/src/$KM": void 0,
"./$PE": "$PE"
} ],
$OV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7d48bi6MARPz5LzFsOYoXtP", "$OV");
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
var a = t("../../../bingo/src/$JX"), l = t("../../../game/src/battle/$QA"), c = t("../../../game/src/battle/$QM"), h = t("../../../game/src/$KK"), b = t("../../../main/src/$KM"), u = t("../../../main/src/tbl/TblEnum"), p = t("./$PE"), f = cc._decorator, d = f.ccclass, v = f.property, y = ((n = {})[u.TblEnum.QualityTypeTableEnum.EPIC] = [ b.cvResUrl.mc_drop_ShenQi_light0, b.cvResUrl.mc_drop_ShenQi_light1, b.cvResUrl.mc_drop_ShenQi_light2, 3, 1.4, 2 ], 
n[u.TblEnum.QualityTypeTableEnum.LEGEND] = [ b.cvResUrl.mc_drop_ShiShi_light0, b.cvResUrl.mc_drop_ShiShi_light1, b.cvResUrl.mc_drop_ShiShi_light2, 4, 1.5, 2 ], 
n), g = function(t) {
r(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.icon = null;
e.main = null;
e.btf = 0;
e.bth = null;
e.bti = null;
e.btk = 1;
return e;
}
e.prototype.ku = function() {};
e.prototype.btj = function(t) {
var e = this;
this.main.scale = .62;
this.main.angle = 0;
this.btf = 0;
this.node.opacity = 255;
this.icon.spriteFrame = null;
var i = this.btg = c.default.bGR.bHn(t.itemId);
if (i) {
this.lE(this.icon, i.icon);
this.icon.node.angle = 0;
if (i.type === a.$fb.tblEnum.ItemTypeTableEnum.EQUIP) {
if (this.bti) return;
var o = y[i.quality];
i.quality >= u.TblEnum.QualityTypeTableEnum.EPIC && h.default.JE.nc.bAV(b.cvResUrl.legendary_epic_item_fall);
if (o) {
var n = h.default.JE.nb.bFN(b.cvResUrl.movieClip);
this.node.addChild(n, 100);
n.scale = o[3];
n.angle = 0;
n.setPosition(0, 0);
var r = n.getComponent(p.default);
this.bti = r;
this.btl();
r.bvW(o[0], !1, function() {
e.bti = null;
n.isValid && h.default.JE.nb.bFX(b.cvResUrl.movieClip, n);
}, .2 * Math.random());
}
}
}
};
e.prototype.setTimeScale = function(t) {
if (this.btk !== t) {
this.btk = t;
this.btl();
}
};
e.prototype.btl = function() {
var t;
(null === (t = this.bti) || void 0 === t ? void 0 : t.isValid) && this.bti.setTimeScale(1 * this.btk);
};
e.prototype.btm = function() {
var t;
if (!this.btf) {
this.main.angle = 0;
this.btf = 1;
if ((null === (t = this.btg) || void 0 === t ? void 0 : t.type) === a.$fb.tblEnum.ItemTypeTableEnum.EQUIP) {
var e = y[this.btg.quality];
if (e) {
var i = h.default.JE.nb.bFN(b.cvResUrl.movieClip);
this.node.addChild(i, 100);
i.scale = e[5];
i.angle = 0;
i.setPosition(0, 0);
(o = i.getComponent(p.default)).setTimeScale(1);
o.bvW(e[2], !1, function() {
i.isValid && h.default.JE.nb.bFX(b.cvResUrl.movieClip, i);
});
var o, n = this.bth;
if (!this.bth) {
var r = h.default.JE.nb.bFN(b.cvResUrl.movieClip);
this.node.addChild(r, -1);
n = this.bth = r;
}
n.scale = e[4];
n.angle = 0;
n.setPosition(0, 0);
(o = n.getComponent(p.default)).setTimeScale(1);
o.bvW(e[1], !0, function() {});
}
}
}
};
e.prototype.btn = function() {
var t;
if (this.icon) {
this.icon._cv_loading_sp = null;
this.icon.spriteFrame = null;
}
(null === (t = this.bth) || void 0 === t ? void 0 : t.isValid) && h.default.JE.nb.bFX(b.cvResUrl.movieClip, this.bth);
this.bth = null;
};
e.prototype.kH = function(t) {
this.btf || (this.main.angle += 450 * t);
};
e.prototype.reuseSelf = function() {
this.btn();
this.lN();
};
s([ v(cc.Sprite) ], e.prototype, "icon", void 0);
s([ v(cc.Node) ], e.prototype, "main", void 0);
return s([ d ], e);
}(l.IVFXItem);
i.default = g;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../main/src/$KM": void 0,
"../../../main/src/tbl/TblEnum": void 0,
"./$PE": "$PE"
} ],
$OW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ec6907avmJJoY/qk1PpX6qH", "$OW");
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
var s = t("../../../game/src/battle/$QA"), a = t("../../../game/src/battle/$QM"), l = cc._decorator, c = l.ccclass, h = l.property, b = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.itemName = null;
e.bto = null;
return e;
}
i = e;
e.prototype.start = function() {};
e.prototype.btp = function(t) {
var e = this.bto = a.default.bGR.bHn(t.itemId);
if (e) {
var o = $eZ.i18n.LV(e.text);
t.amount > 1 && (o = o + "x" + t.amount);
this.itemName.string = o;
this.itemName.node.color = a.default.bGR.bHo(e.quality);
i.btq(this.itemName);
this.node.getChildByName("img").width = this.itemName.node.width + 14;
}
};
e.btq = function(t) {
t._forceUpdateRenderData();
};
e.prototype.reuseSelf = function() {
this.lN();
};
var i;
r([ h(cc.Label) ], e.prototype, "itemName", void 0);
return i = r([ c ], e);
}(s.IVFXItem);
i.default = b;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0
} ],
$OX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "035e735NfBM6KIuNvi6ifxm", "$OX");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.sp = null;
e.sfArray = [];
e.btr = null;
return e;
}
e.prototype.ks = function() {};
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
e.prototype.bts = function(t) {
this.sp.spriteFrame = this.sfArray[t];
};
r([ c(cc.Sprite) ], e.prototype, "sp", void 0);
r([ c(cc.SpriteFrame) ], e.prototype, "sfArray", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$OY: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "116ec6gzcFD/4snNFoR/gbp", "$OY");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = [ cc.color(216, 0, 0), cc.color(248, 128, 0), cc.color(144, 200, 0), cc.color(0, 120, 200), cc.color(136, 128, 248) ], b = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.current = null;
e.mid = null;
e.next = null;
e.text = null;
e.btt = 0;
e.btu = 0;
e.btv = 0;
e.btw = 0;
e.btx = 0;
e.bty = 360;
e.btz = !1;
e.btA = [];
return e;
}
e.prototype.ks = function() {
this.mid.node.active = !1;
this.text.node.zIndex = 100;
this.next.node.active = !1;
this.bty = this.current.node.width;
};
e.prototype.kH = function() {
if (this.btu > this.btv) {
var t = this.btu - this.btv;
this.btu = this.btu + .128 * (this.btv - this.btu);
t / this.btw < .05 && (this.btu = Math.max(this.btu - .05 * this.btw, this.btv));
this.btt = this.btu % this.btw;
0 === this.btt && this.btu > 0 && (this.btt = this.btw);
this.btE();
}
};
e.prototype.btB = function(t, e, i) {
this.btw = e;
if (this.btz !== i) {
this.btz = i;
this.text.string = "";
}
this.btD(t);
};
e.prototype.btC = function(t, e) {
this.btw = e;
this.btu = this.btv = t;
this.btt = this.btu % this.btw;
0 === this.btt && this.btu > 0 && (this.btt = this.btw);
this.btH();
this.btE();
};
e.prototype.btD = function(t) {
if (this.btv !== t) {
this.btv = t;
var e = t - this.btu, i = e < 0;
if (Math.abs(e) > this.btw && i) ; else {
var o = this.btt;
this.btu = t;
this.btt = this.btu % this.btw;
0 === this.btt && this.btu > 0 && (this.btt = this.btw);
this.btE();
i && this.btF(this.btt, Math.abs(e), o);
}
}
};
e.prototype.btE = function() {
this.btx = Math.ceil(this.btu / this.btw);
var t = Math.max(0, this.btx - 1);
if (0 === t) this.next.node.active = !1; else {
this.next.node.active = !0;
this.next.node.color = h[(t - 1 + h.length) % h.length];
}
this.btK(this.current, this.btt);
this.current.node.color = h[t % h.length];
this.text.string = this.btz ? "" : "x " + this.btx;
};
e.prototype.btF = function(t, e, i) {
if (e > this.btw || i <= e) {
this.btG(0, i / this.btw);
if (this.btv > this.btw) {
var o = t / this.btw, n = 1 - o;
this.btG(o, n);
}
} else this.btG(t / this.btw, e / this.btw);
};
e.prototype.btG = function(t, e) {
var i = this, o = this.btI();
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
i.btJ(o);
}).start();
};
e.prototype.btH = function() {
for (var t = 3; t < this.node.childrenCount; t++) this.btJ(this.node.children[t]);
};
e.prototype.btI = function() {
return this.btA.length > 0 ? this.btA.pop() : cc.instantiate(this.mid.node);
};
e.prototype.btJ = function(t) {
cc.Tween.stopAllByTarget(t);
t.removeFromParent(!1);
this.btA.push(t);
};
e.prototype.btK = function(t, e) {
t.fillRange = e / this.btw;
};
r([ c(cc.Sprite) ], e.prototype, "current", void 0);
r([ c(cc.Sprite) ], e.prototype, "mid", void 0);
r([ c(cc.Sprite) ], e.prototype, "next", void 0);
r([ c(cc.Label) ], e.prototype, "text", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$OZ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "1efb89RQJBCjoU8TxTtFvGs", "$OZ");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$kB = void 0;
var l = t("./$PE"), c = t("./$PH"), h = t("./$PI"), b = t("../../../game/src/battle/$QK"), u = t("./$PB"), p = t("../../../game/src/battle/$QA"), f = t("../../../game/src/$KK"), d = t("../../../bingo/src/$JX"), v = t("../../../bingo/src/utils/$ML"), y = t("../../../game/src/utils/$TW"), g = t("../../../game/src/battle/$QM"), m = t("./$OS"), _ = t("../../../game/src/utils/$TY"), $ = t("../../../game/src/perfab/common/$CCD"), P = t("./$PK"), w = t("../../../game/src/perf/$TF"), I = cc._decorator, O = I.ccclass, E = I.property, x = (cc.color(255, 80, 80), 
[ cc.color().fromHEX("00f0ff"), cc.color().fromHEX("ffff30"), cc.color().fromHEX("ff4848") ]), S = cc.color(), M = function() {
function t(t, e) {
this.btL = 0;
this.btM = null;
this.btN = null;
this.btO = null;
this.btM = t;
this.btL = e;
}
t.prototype.btP = function() {
this.btS();
this.btM = null;
this.btO = null;
this.btN = null;
};
t.prototype.btQ = function(t) {
var e, i, o, n;
this.btO = t;
if (this.btO && (null === (e = this.btM) || void 0 === e ? void 0 : e.isValid)) switch (null !== (i = this.btO.resType) && void 0 !== i ? i : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
if (!this.btN) {
this.btN = f.default.JE.nb.bFN(d.$fb.ResUrl.spineEffect);
if (!this.btN) return;
this.btM.addChild(this.btN, -10);
}
this.btN.zIndex = this.btO.zIndex;
var r = this.btN.getComponent(h.$kL);
if (!r) return;
var s = r.node, a = this.btO;
cc.Tween.stopAllByTarget(s);
s.active = !1;
if (a.delay || a.interval) cc.tween(s).delay(a.delay || 0).call(function() {
cc.tween(s).call(function() {
r.mo(a.url, a.anim || "idle", !1);
s.active = !0;
}).delay(a.interval).union().repeatForever().start();
}).start(); else {
r.mo(a.url, a.anim || "idle", !0);
s.active = !0;
}
r.setTimeScale(null !== (o = a.tScale) && void 0 !== o ? o : 1);
r.mm(Boolean(a.premultAlpha));
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
if (!this.btN) {
this.btN = f.default.JE.nb.bFN(d.$fb.ResUrl.movieClip);
if (!this.btN) return;
this.btM.addChild(this.btN, -10);
}
this.btN.zIndex = this.btO.zIndex;
var c = this.btN.getComponent(l.default);
if (!c) return;
var b = c.node, u = this.btO;
cc.Tween.stopAllByTarget(b);
b.active = !1;
if (u.delay || u.interval) cc.tween(b).delay(u.delay || 0).call(function() {
cc.tween(b).call(function() {
c.bvW(u.url, !0);
b.active = !0;
}).delay(u.interval).union().repeatForever().start();
}).start(); else {
c.bvW(u.url, !0);
b.active = !0;
}
c.setTimeScale(null !== (n = u.tScale) && void 0 !== n ? n : 1);
}
};
t.prototype.btR = function() {
var t, e, i, o, n, r, s, a;
if (this.btM && this.btN) {
var l = g.default.bGR.bHy(this.btL);
if (l) {
var c = l.fashion, h = null !== (t = null == c ? void 0 : c.scale) && void 0 !== t ? t : 1, b = null !== (e = null == c ? void 0 : c.x) && void 0 !== e ? e : 0, u = null !== (i = null == c ? void 0 : c.y) && void 0 !== i ? i : 0, p = this.btO, f = this.btM.scaleX, d = this.btN;
d.zIndex = null !== (o = p.zIndex) && void 0 !== o ? o : -10;
d.scaleX = h * (null !== (n = p.scaleX) && void 0 !== n ? n : 1) * f;
d.scaleY = h * (null !== (r = p.scaleY) && void 0 !== r ? r : 1);
d.x = ((null !== (s = p.x) && void 0 !== s ? s : 0) + b) * f;
d.y = (null !== (a = p.y) && void 0 !== a ? a : 0) + l.offsetY + u;
}
}
};
t.prototype.btS = function() {
var t;
if (this.btN && cc.isValid(this.btN)) if (this.btO) switch (null !== (t = this.btO.resType) && void 0 !== t ? t : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
cc.Tween.stopAllByTarget(this.btN);
f.default.JE.nb.bFX(d.$fb.ResUrl.spineEffect, this.btN);
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
cc.Tween.stopAllByTarget(this.btN);
f.default.JE.nb.bFX(d.$fb.ResUrl.movieClip, this.btN);
break;

default:
this.btN.destroy();
} else this.btN.destroy();
this.btO = null;
this.btN = null;
};
return t;
}(), C = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.dirNode = null;
e.mainNode = null;
e.btT = 1;
e.btU = !0;
e.btW = null;
e.btX = null;
e.btY = null;
e.btZ = 0;
e.bua = !1;
e.bub = 255;
e.buc = null;
e.buf = 1;
e.bug = 1;
e.buk = 0;
e.bun = [];
e.but = null;
e.buu = 0;
e.buv = 0;
e.buw = 0;
e.buB = -1;
e.buC = "";
e.buD = !1;
e.buH = !0;
e.buI = 0;
e.buJ = [];
e.buM = !0;
e.buN = 0;
e.buO = [];
e.buY = null;
return e;
}
e.prototype.ks = function() {};
e.prototype.ku = function() {
cc.director.on(cc.Director.EVENT_AFTER_DRAW, this.buy, this);
};
e.prototype.ky = function() {
cc.director.targetOff(this);
this.bup();
this.btX = null;
this.btY = null;
this.btV = null;
this.btW = null;
this.buP(0);
this.buK(0);
};
e.prototype.bud = function(t) {
for (var e = p.COMMON_SKEL_MIX_ANIM.filter(function(e) {
return t.findAnimation(e);
}), i = 0, o = p.ALL_SKEL_ANIM_LIST.filter(function(e) {
return t.findAnimation(e);
}); i < o.length; i++) for (var n = o[i], r = 0, s = e; r < s.length; r++) {
var a = s[r];
n !== a && t.setMix(n, a, .15);
}
};
e.prototype.bue = function(t, e, i) {
var o;
this.buC = "";
this.btV = t;
this.btY = i;
this.btX = g.default.bGR.bHy(e);
if (this.btX) {
this.buf = 1;
this.bug = 1;
this.btT = 1;
this.mainNode.opacity = this.bub = 255;
this.buc = null;
this.mainNode.destroyAllChildren();
this.btW = null;
this.mainNode.active = !f.default.JD.cdE;
this.mainNode.setPosition(0, 0);
this.btU = this.btX.resType !== p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE;
switch (this.btX.resType) {
case p.DISPLAY_RES_TYPE.SPINE:
var n = g.default.bGR.bHV(this.btX.resType), r = this.btY.bFN(n);
if (!r) break;
this.mainNode.addChild(r, 1);
(s = r.getComponent(h.$kL)).mn(_.$sk.cHx());
r.scaleX = this.btX.scaleX;
r.scaleY = this.btX.scaleY;
r.setPosition(this.btX.offsetX, this.btX.offsetY);
r.angle = 0;
r.active = !1;
s.mi = null;
s.mm(Boolean(this.btX.premultAlpha));
s.mq(this.btX.resUrl).then(function(t) {
t && (r.active = !0);
});
this.btW = s;
this.btW.resUrl = n;
break;

case p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE:
n = g.default.bGR.bHV(this.btX.resType);
var s, a = this.btY.bFN(n);
if (!a) break;
this.mainNode.addChild(a, 1);
(s = a.getComponent(m.$kq)).mn(_.$sk.cHu);
a.scaleX = this.btX.scaleX;
a.scaleY = this.btX.scaleY;
a.setPosition(this.btX.offsetX, this.btX.offsetY);
a.angle = 0;
a.active = !1;
s.mi = null;
s.mm(Boolean(this.btX.premultAlpha));
s.mq(this.btX.resUrl).then(function(t) {
t && (a.active = !0);
});
this.btW = s;
this.btW.resUrl = n;
s.bsy(null !== (o = this.btX.models) && void 0 !== o ? o : [], 1);
break;

case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
n = g.default.bGR.bHV(this.btX.resType);
var c = this.btY.bFN(n);
if (!c) break;
this.mainNode.addChild(c, 1);
var b = c.getComponent(l.default);
c.scaleX = this.btX.scaleX;
c.scaleY = this.btX.scaleY;
c.setPosition(this.btX.offsetX, this.btX.offsetY);
c.angle = 0;
b.bvW(this.btX.resUrl, !0);
this.btW = b;
this.btW.resUrl = n;
}
this.buj();
}
};
e.prototype.buh = function(t) {
if (this.buf !== t) {
this.buf = t;
this.buj();
}
};
e.prototype.bui = function(t) {
if (this.btT !== t) {
this.btT = t;
this.buj();
}
};
e.prototype.buj = function() {
this.btW && this.btW.setTimeScale(this.buf * this.btT);
};
e.prototype.bul = function(t) {
t = v.default.ber(t);
if (this.buk !== t) {
this.buk = t;
this.dirNode.angle = t;
}
};
e.prototype.bum = function(t) {
if (this.dirNode.scaleX !== t) {
this.dirNode.scaleX = t;
this.bva();
}
};
e.prototype.buo = function(t) {
var e = this.bun.findIndex(function(e) {
return e.id === t;
}), i = this.bun[e];
if (i) {
this.bun.splice(e, 1);
cc.isValid(i.node) && this.btY.bFX(i.nodeUrl, i.node);
}
};
e.prototype.bup = function() {
for (var t = 0; t < this.bun.length; t++) {
var e = this.bun[t];
cc.isValid(e.node) && this.btY.bFX(e.nodeUrl, e.node);
}
this.bun.length = 0;
};
e.prototype.buq = function(t) {
for (var e = 0; e < this.bun.length; e++) this.bun[e].node.active = t;
if (this.buO.length > 0) for (var i = 0, o = this.buO; i < o.length; i++) (s = o[i]).btN && (s.btN.opacity = t ? 255 : 0);
if (this.buJ.length > 0) for (var n = 0, r = this.buJ; n < r.length; n++) {
var s;
(s = r[n]).btN && (s.btN.opacity = t ? 255 : 0);
}
};
e.prototype.bur = function(t, e, i, o) {
var n, r, s = this;
if (!t) return !1;
if (t.HE) {
r = t.HE.node;
"number" == typeof e.z && (r.zIndex = e.z);
"number" == typeof e.x && (r.x = e.x);
"number" == typeof e.y && (r.y = e.y);
"number" == typeof e.angle && (r.angle = e.angle);
} else {
var a = g.default.bGR.bHV(e.resType);
e.resType === p.DISPLAY_RES_TYPE.PREFAB && (a = String(e.resUrl));
if (!(r = this.btY.bFN(a))) return !1;
var b = "number" == typeof e.z ? e.z : 1;
i.addChild(r, b);
cc.Tween.stopAllByTarget(r);
r.opacity = 255;
r.x = "number" == typeof e.x ? e.x : 0;
r.y = "number" == typeof e.y ? e.y : 0;
r.angle = "number" == typeof e.angle ? e.angle : 0;
t.HD = a;
}
r.scaleX = t.HH * t.HF;
r.scaleY = t.HI * t.HG;
var u = null !== (n = e.timeScale) && void 0 !== n ? n : 1;
t.HB = u;
t.HC = null == o || o;
var f = t.HC ? this.buf : this.bug;
switch (e.resType) {
case p.DISPLAY_RES_TYPE.MOVIE_CLIP:
if (!e.resUrl) break;
var d = r.getComponent(l.default);
if (!d) return !0;
"string" == typeof e.color && d.bvU(S.fromHEX(e.color));
t.HE = d;
d.setTimeScale(f * u);
e.autoDestroy ? d.bvW(e.resUrl, !1, function() {
s.btV && s.btV.Iy(t);
}, e.animStart, e.animEnd) : d.bvW(e.resUrl, Boolean(e.loop), null, e.animStart, e.animEnd);
break;

case p.DISPLAY_RES_TYPE.SPINE:
if (!e.resUrl) break;
if (!e.anim) break;
var v = r.getComponent(h.$kL);
if (!v) return !0;
t.HE = v;
v.mn(null == e.animStart && null == e.animEnd);
v.setTimeScale(f * u);
v.mm(Boolean(e.premultAlpha));
e.autoDestroy ? v.mo(e.resUrl, e.anim, !1, function() {
s.btV.Iy(t);
}, e.animStart, e.animEnd) : v.mo(e.resUrl, e.anim, Boolean(e.loop), null, e.animStart, e.animEnd);
break;

case p.DISPLAY_RES_TYPE.SINGLE_TEXTURE:
if (!e.resUrl) break;
var y = r.getComponent(c.$kK);
if (!y) return !0;
t.HE = y;
y.setTimeScale(f);
y.bwh({
resUrl: e.resUrl,
blendMode: e.blend
});
"string" == typeof e.color && y.bwi(S.fromHEX(e.color));
break;

case p.DISPLAY_RES_TYPE.PREFAB:
var m = r.getComponent(p.IVFXItem);
t.HE = m;
m.prefabPlay && m.prefabPlay(function() {
e.autoDestroy && s.btV.Iy(t);
});
break;

case p.DISPLAY_RES_TYPE.VIDEO:
break;

case p.DISPLAY_RES_TYPE.WARNING_AREA:
if (!e.warningArea) break;
var _ = r.getComponent(P.$kN);
if (!_) return !0;
t.HE = _;
_.setTimeScale(f * u);
_.bwN(e.warningArea, function() {
e.autoDestroy && s.btV.Iy(t);
});
}
r.active = !0;
e.stopAllAct && cc.Tween.stopAllByTarget(r);
if (e.acts) for (var $ = 0, w = e.acts; $ < w.length; $++) {
for (var I = w[$], O = cc.tween(r), E = 0, x = I; E < x.length; E++) {
var M = x[E];
O[M.t].apply(O, M.p);
}
O.start();
}
return !0;
};
e.prototype.bus = function(t) {
if (this.bua !== t) {
this.bua = t;
this.bua || (this.mainNode.opacity = this.bub);
}
};
e.prototype.bux = function(t) {
if (this.buu !== t) {
this.buu = t;
this.buu || this.but && (this.but.active = !1);
}
};
e.prototype.kH = function(t) {
if (this.buc) {
this.bub += this.buc.speed * t;
if (this.buc.speed > 0 && this.bub >= this.buc.val || this.buc.speed < 0 && this.bub <= this.buc.val) {
this.bub = this.buc.val;
this.buc = null;
}
this.mainNode.opacity = this.bub;
}
if (this.bua) {
this.btZ += t;
if (this.btZ > .07) {
this.btZ -= .07;
this.mainNode.opacity === this.bub ? this.mainNode.opacity = this.bub : this.mainNode.opacity = .2 * this.bub;
}
}
};
e.prototype.buy = function() {
this.buz();
};
e.prototype.buz = function() {
if (this.btX) if (this.buu) if (0 === this.buv) {
this.buv = this.buw;
if (!this.but) {
var t = this.btY.bFN(d.$fb.ResUrl.ghost);
if (!t) return;
this.but = t;
t.groupIndex = y.CC_GROUP_INDEX.BATTLE;
t.zIndex = -10;
t.parent = this.node;
}
this.but.active = !0;
var e = 660, i = 0, o = 280;
if (this.btX.ghostParam) {
e = this.btX.ghostParam[0];
i = this.btX.ghostParam[1];
o = this.btX.ghostParam[2];
}
var n = b.default.bGt(this.mainNode, e, i, o, 1.6);
this.but.x = i;
this.but.y = o;
this.but.setScale(1.6);
this.but.opacity = 255;
var r = this.but.getComponent(u.default), s = 480 / e * .006, a = x[this.buu - 1];
if (a) {
r.bvm(1, a, s);
r.bvl(n);
}
} else this.buv--; else this.but && (this.but.active = !1);
};
e.prototype.reuseSelf = function() {
this.buB = -1;
this.buC = "";
this.buP(0);
this.buK(0);
this.buY && (this.buY.node.active = !1);
};
e.prototype.buA = function() {
var t = this.btX;
if (!t) return !1;
if (t.resType !== p.DISPLAY_RES_TYPE.SPINE && t.resType !== p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE) return !1;
var e = this.btW;
return null == e ? void 0 : e.mp(t.resUrl);
};
e.prototype.buE = function(t, e) {
void 0 === e && (e = 0);
var i = t.h || 0;
this.mainNode.angle !== i && (this.mainNode.angle = i);
var o = this.btX;
if (o && (o.resType === p.DISPLAY_RES_TYPE.SPINE || o.resType === p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE)) {
this.buC = t.d;
var n = this.btW;
if (n && n.mp(o.resUrl)) {
this.bui(t.c);
var r = n.sp, s = Boolean(t.e);
if (t.d !== r.animation || e !== this.buB) {
this.buB = e;
this.buD = s;
if (a = r.setAnimation(0, t.d, s)) {
t.i && (a.animationStart = t.i);
"number" == typeof t.j && (a.animationEnd = t.j);
}
} else if (!_.$sk.cHt && this.buD !== s) {
this.buD = s;
var a;
if ((a = r.getCurrent(0)) && a.loop !== s) {
r.setAnimation(0, t.d, s);
t.i && (a.animationStart = t.i);
"number" == typeof t.j && (a.animationEnd = t.j);
}
}
}
n && n.mr(t.f || 0, t.g || 0);
}
};
e.prototype.buF = function(t, e) {
var i = this.btX;
if (i && i.resType === p.DISPLAY_RES_TYPE.SPINE) {
var o = this.btW;
if (!o || o.mp(i.resUrl)) {
var n = o.sp.findBone(t);
if (n) {
n.scaleX = e ? 1 : 0;
n.scaleY = e ? 1 : 0;
}
}
}
};
e.prototype.buG = function(t, e) {
0 == t ? this.bub = e : this.buc = {
speed: (e - this.bub) / t,
val: e
};
};
e.prototype.buK = function(t) {
var e = this;
this.buI = t;
this.buH && t ? this.buS(this.buI).then(function() {
e.isValid && e.buI === t && e.buT(e.buH ? t : 0, e.buJ);
}) : this.buT(0, this.buJ);
};
Object.defineProperty(e.prototype, "buL", {
set: function(t) {
if (this.buH !== t) {
this.buH = t;
this.buI && this.buK(this.buI);
}
},
enumerable: !1,
configurable: !0
});
e.prototype.buP = function(t) {
var e = this;
this.buN = t;
this.buM && t ? this.buS(t).then(function() {
e.isValid && e.buN === t && e.buT(e.buM ? t : 0, e.buO);
}) : this.buT(0, this.buO);
};
Object.defineProperty(e.prototype, "buQ", {
set: function(t) {
if (this.buM !== t) {
this.buM = t;
this.buN && this.buP(this.buN);
}
},
enumerable: !1,
configurable: !0
});
e.prototype.buR = function(t) {
for (var e = 0, i = t; e < i.length; e++) i[e].btR();
};
e.prototype.buS = function(t) {
var e;
return s(this, void 0, void 0, function() {
var i, o, n, r, s;
return a(this,function AWAITF6(a){switch (a.label) {
case 0:
if (w.HeatIsolate.cyY) return [ 2, !0 ];
if (!(i = $eZ.gameRes.chJ(t))) return [ 2, !0 ];
o = [];
for (n = 0, r = i.res; n < r.length; n++) {
s = r[n];
switch (null !== (e = s.resType) && void 0 !== e ? e : p.DISPLAY_RES_TYPE.SPINE) {
case p.DISPLAY_RES_TYPE.SPINE:
o.push(this.lK(new d.$fb.ResCfg(s.url, sp.SkeletonData), !1));
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
e.prototype.buT = function(t, e) {
var i;
if (t) {
if (!this.btX) return;
var o = $eZ.gameRes.chJ(t);
if (!o) return;
for (var n = 0; n < o.res.length; n++) {
var r = o.res[n], s = e[n];
if (!s) {
s = new M(this.dirNode, this.btX.id);
e.push(s);
}
s.btN && (null === (i = s.btO) || void 0 === i ? void 0 : i.resType) !== r.resType && s.btS();
s.btQ(r);
}
for (;n < e.length; n++) e[n].btP();
e.length = o.res.length;
this.buR(e);
} else if (e.length) {
for (var a = 0, l = e; a < l.length; a++) (r = l[a]).btP();
e.length = 0;
}
};
e.prototype.buU = function() {
return this.btW;
};
e.prototype.buV = function() {
return this.btW;
};
e.prototype.buW = function(t) {
if (this.btX.resType === p.DISPLAY_RES_TYPE.SPINE || p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE) {
var e = this.btW;
e && e.mu(t);
}
};
e.prototype.buX = function() {
if (this.btX && (this.btX.resType === p.DISPLAY_RES_TYPE.SPINE || p.DISPLAY_RES_TYPE.CHANGABLEB_SPINE)) {
var t = this.btW;
t && t.mw(.3);
}
};
e.prototype.buZ = function(t, e) {
return s(this, void 0, void 0, function() {
var i, o;
return a(this,function AWAITF7(n){switch (n.label) {
case 0:
return w.HeatIsolate.cyY && !this.buY ? [ 2 ] : this.buY ? [ 3, 2 ] : [ 4, $eZ.prefabMgr.cPC(new d.$fb.ResCfg(d.$fb.ResUrl.SceneBubble, cc.Prefab)) ];

case 1:
i = n.sent();
if (!this.isValid) return [ 2 ];
if (this.buY) return [ 2 ];
o = i.getComponent($.default);
this.buY = o;
this.node.addChild(i, 101);
n.label = 2;

case 2:
this.buY.node.active = !0;
this.buY.fMp(t, e);
this.bva();
return [ 2 ];
}
});
});
};
e.prototype.bva = function() {
var t, e, i, o;
if (this.buY) if (null === (t = this.btX) || void 0 === t ? void 0 : t.topPos) {
var n = null === (e = this.btX) || void 0 === e ? void 0 : e.topPos;
this.buY.node.x = (null !== (i = n[0]) && void 0 !== i ? i : 0) * this.dirNode.scaleX;
this.buY.node.y = (null !== (o = n[1]) && void 0 !== o ? o : 0) + 50;
} else this.buY.node.y = 200;
};
e.prototype.bvb = function(t) {
var e = this.buV();
if (null == e ? void 0 : e.bsF) {
var i = $eZ.gameHelper.chr(t);
e.bsF(10, i);
}
};
r([ E(cc.Node) ], e.prototype, "dirNode", void 0);
r([ E(cc.Node) ], e.prototype, "mainNode", void 0);
return r([ O ], e);
}(d.$fb.Component);
i.$kB = C;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../bingo/src/utils/$ML": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QK": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/perf/$TF": void 0,
"../../../game/src/perfab/common/$CCD": void 0,
"../../../game/src/utils/$TW": void 0,
"../../../game/src/utils/$TY": void 0,
"./$OS": "$OS",
"./$PB": "$PB",
"./$PE": "$PE",
"./$PH": "$PH",
"./$PI": "$PI",
"./$PK": "$PK"
} ],
$PA: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4c5b7K4v6lP5qT9taT2082Q", "$PA");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.text = null;
e.fonts = [];
e.bvc = null;
e.bvd = !1;
e.bve = 0;
e.bvf = 0;
e.bvg = 0;
e.bvh = 0;
e.bvi = 0;
return e;
}
e.prototype.bvj = function(t, e, i, o, n) {
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
var l = (Math.random() - .5) * Math.PI * s, c = Math.cos(l), h = Math.sin(l), b = o * c - n * h, u = o * h + n * c, p = this.node;
this.bvf = p.x;
this.bvg = p.y;
this.bvh = b * a;
this.bvi = u * a;
this.bve = 0;
this.bvd = !0;
this.bvc = e;
p.scale = 0;
p.opacity = 255;
};
e.prototype.kH = function(t) {
if (this.bvd) {
var e, i = this.node, o = this.bve + t;
this.bve = o;
if (o < .15) e = o / .15 * .7; else {
var n = (o - .15) / .45;
e = .7 + (1 - .7) * (n > 1 ? 1 : n);
}
i.setPosition(this.bvf + this.bvh * e, this.bvg + this.bvi * e);
o < .15 ? i.scale = o / .15 * 2.25 : o < .3 ? i.scale = 2.25 + (o - .15) / .15 * -.75 : 1.5 !== i.scale && (i.scale = 1.5);
if (o >= .52) {
i.opacity = 0;
this.bvd = !1;
var r = this.bvc;
this.bvc = null;
r && r(i);
} else o >= .42 && (i.opacity = Math.round(255 * (1 - (o - .42) / .1)));
}
};
e.prototype.kx = function() {
this.bvd = !1;
this.bvc = null;
};
r([ c(cc.Label) ], e.prototype, "text", void 0);
r([ c(cc.BitmapFont) ], e.prototype, "fonts", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$PB: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c20e7qR6XNHyaPlwAfho8l/", "$PB");
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
var s = t("../../../bingo/src/$JX"), a = t("../../../game/src/battle/$QK"), l = cc._decorator, c = l.ccclass, h = l.property, b = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.ghostSprite = null;
e.materials = [];
e.bvk = cc.v2();
return e;
}
e.prototype.ks = function() {};
e.prototype.onEnable = function() {};
e.prototype.onDestroy = function() {
this.reuseSelf();
};
e.prototype.bvl = function(t) {
var e = this.ghostSprite;
if (e.spriteFrame) {
var i = e.spriteFrame, o = i.getTexture();
e.spriteFrame = null;
o instanceof cc.RenderTexture && a.default.bGk(o);
i.destroy();
}
var n = new cc.SpriteFrame(t);
n.setFlipY(!0);
e.spriteFrame = n;
};
e.prototype.bvm = function(t, e, i) {
var o = this.materials[t];
if (o) {
if (1 === t) {
this.bvk.x = this.bvk.y = i;
o.setProperty("outlineSize", this.bvk);
}
this.ghostSprite.setMaterial(0, o);
this.ghostSprite.node.color = e;
}
};
e.prototype.bvn = function(t) {
this.ghostSprite.dstBlendFactor = t ? cc.macro.BlendFactor.ONE : cc.macro.BlendFactor.ONE_MINUS_SRC_ALPHA;
};
e.prototype.reuseSelf = function() {
var t = this.ghostSprite;
if (t.spriteFrame) {
var e = t.spriteFrame, i = e.getTexture();
t.spriteFrame = null;
i instanceof cc.RenderTexture && a.default.bGk(i);
e.destroy();
}
};
r([ h(cc.Sprite) ], e.prototype, "ghostSprite", void 0);
r([ h(cc.Material) ], e.prototype, "materials", void 0);
return r([ c ], e);
}(s.$fb.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QK": void 0
} ],
$PC: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "51257geVWlFkZTmi7QnRjyd", "$PC");
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
var s = t("../../../game/src/battle/$QA"), a = t("./$PE"), l = cc._decorator, c = l.ccclass, h = (l.property, 
function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bvo = null;
return e;
}
e.prototype.ky = function() {
this.bvo = null;
};
e.prototype.prefabPlay = function(t) {
var e = this;
this.bvo = t;
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
n.bvW(14, !1);
o.opacity = 255;
o.scale = 2;
o.angle = 360 * Math.random();
o.stopAllActions();
cc.tween(o).delay(.1).to(.2, {
opacity: 0
}).call(function() {
e.bvo && e.bvo();
}).start();
};
return r([ c ], e);
}(s.IVFXItem));
i.default = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0,
"./$PE": "$PE"
} ],
$PD: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "827886jTORAv5jBUCh0Qew+", "$PD");
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
i.$kF = void 0;
var l = t("../../../bingo/src/$JX"), c = cc._decorator, h = c.ccclass, b = c.property, u = function(t) {
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
e.prototype.bvq = function(t, e, o, n, r) {
var s = t.dcY;
if (t.dcS) {
var a = $eZ.battleMgr.bDv.bDd >= 1;
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
this.bvs(t);
if (r && o) this.bvt(); else {
cc.Tween.stopAllByTarget(this.mask);
this.mask.active = !(e || t.dcO);
this.mask.opacity = 110;
}
if (e && o && t.ddb > 0) {
this.arrow.active = !0;
var l = i.bvp[t.ddb];
if (l) {
this.arrow.setPosition(l.x, l.y);
this.arrow.angle = l.angle;
}
} else this.arrow.active = !1;
};
e.prototype.bvs = function(t) {
for (var e = this.pathNode.children, o = 0, n = 1; n <= 4; n++) if (t.dcK[n]) {
var r = e[++o - 1];
if (!r) {
r = cc.instantiate(e[0]);
this.pathNode.addChild(r);
}
r.active = !0;
r.angle = i.bvr[n].angle;
}
for (n = o; n < e.length; n++) e[n].active = !1;
this.pathDotNode.active = 1 === o;
};
e.prototype.bvt = function() {
this.mask.active = !0;
cc.Tween.stopAllByTarget(this.mask);
cc.tween(this.mask).repeatForever(cc.tween().to(.25, {
opacity: 0
}).to(.25, {
opacity: 110
})).start();
};
var i;
e.bvp = ((n = {})[1] = {
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
e.bvr = ((r = {})[1] = {
angle: 0
}, r[2] = {
angle: 180
}, r[3] = {
angle: 90
}, r[4] = {
angle: -90
}, r);
a([ b(cc.SpriteFrame) ], e.prototype, "iconSF", void 0);
a([ b(cc.SpriteFrame) ], e.prototype, "lineSF", void 0);
a([ b(cc.Sprite) ], e.prototype, "icon", void 0);
a([ b(cc.Node) ], e.prototype, "pathNode", void 0);
a([ b(cc.Node) ], e.prototype, "pathDotNode", void 0);
a([ b(cc.Node) ], e.prototype, "mask", void 0);
a([ b(cc.Node) ], e.prototype, "arrow", void 0);
return i = a([ h ], e);
}(l.$fb.Component);
i.$kF = u;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$PE: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "8aad7/28v5DAo1DQY73uJxb", "$PE");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
}, l = this && this.__spreadArrays || function() {
for (var t = 0, e = 0, i = arguments.length; e < i; e++) t += arguments[e].length;
var o = Array(t), n = 0;
for (e = 0; e < i; e++) for (var r = arguments[e], s = 0, a = r.length; s < a; s++, 
n++) o[n] = r[s];
return o;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.$kG = void 0;
var c = t("../../../bingo/src/$JX"), h = t("../../../game/src/battle/$QA"), b = t("../../../game/src/battle/$QM"), u = t("../../../game/src/perf/$TF"), p = cc._decorator, f = p.ccclass, d = p.property, v = function() {
function t() {
this.bvw = new Map();
this.bvx = new Map();
this.bvA = null;
}
t.bvv = function() {
null == t.bvu && (t.bvu = new t());
return t.bvu;
};
t.prototype.bvy = function() {
this.bvx.forEach(function(t) {
for (var e = 0, i = t; e < i.length; e++) i[e].decRef();
});
this.bvx.clear();
this.bvw.forEach(function(t, e) {
var i = b.default.bGR.bHO(e);
i && $eZ.resMgr.gfW(i);
});
this.bvw.clear();
};
t.prototype.bvz = function(t) {
return s(this, void 0, Promise, function() {
var e, i, o, n, r, s, l, h, p, f, d;
return a(this,function AWAITF8(a){switch (a.label) {
case 0:
if (u.HeatIsolate.cyY) return [ 2, [] ];
if (!(e = this.bvx.get(t.id))) {
e = [];
this.bvx.set(t.id, e);
}
if (e.length) return [ 2, e ];
if (!(i = b.default.bGR.bHN(t.resName))) return [ 2, e ];
o = b.default.bGR.bHO(t.resName);
return (n = this.bvw.get(t.resName)) ? [ 3, 2 ] : [ 4, $eZ.resMgr.geS(new c.$fb.ResCfg(o, cc.Texture2D)) ];

case 1:
r = a.sent();
n = r.Da;
e.length && n && r.Df();
a.label = 2;

case 2:
if (e.length) return [ 2, e ];
if (!n) return [ 2, e ];
this.bvw.has(t.resName) || this.bvw.set(t.resName, n);
s = i.fs;
if (t.slice) {
h = t.slice[0];
if ((p = t.slice[1]) > h) for (d = h; d <= p; d++) (l = s[d]) && e.push(new cc.SpriteFrame(n, cc.rect(l[0], l[1], l[2], l[3]), l[4], cc.v2(l[5], l[6]), cc.size(l[7], l[8]))); else for (d = h; d >= p; d--) (l = s[d]) && e.push(new cc.SpriteFrame(n, cc.rect(l[0], l[1], l[2], l[3]), l[4], cc.v2(l[5], l[6]), cc.size(l[7], l[8])));
} else {
f = Math.min(s.length, t.frameCount || 999);
for (d = 0; d < f; d++) {
l = s[d];
e.push(new cc.SpriteFrame(n, cc.rect(l[0], l[1], l[2], l[3]), l[4], cc.v2(l[5], l[6]), cc.size(l[7], l[8])));
}
}
return [ 2, e ];
}
});
});
};
t.prototype.bvB = function() {
this.bvA || (this.bvA = this.bvC());
return this.bvA;
};
t.prototype.bvC = function() {
var t = new cc.Node(), e = new cc.Node();
t.addChild(e);
var i = t.addComponent(y), o = e.addComponent(cc.Sprite);
i.sprite = o;
return i;
};
t.bvu = null;
return t;
}();
i.$kG = v;
window.$fK = window.$fK || {};
$fK.mcMgr = v.bvv();
var y = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sprite = null;
e.bvD = null;
e.bvE = .05;
e.bvF = 1;
e.bvG = 0;
e.bvH = -1;
e.bvI = 0;
e.bvJ = !1;
e.bvK = [];
e.bvL = 0;
e.bvM = -1;
e.bvN = !1;
e.bvO = 0;
e.bvP = 1;
e.bvQ = null;
return e;
}
e.prototype.ku = function() {
this.sprite.trim = !1;
};
e.prototype.ky = function() {
this.bvK = null;
this.bvQ = null;
this.bvD = null;
this.bvN = !1;
};
e.prototype.bvR = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF9(o){switch (o.label) {
case 0:
t = this.bvD;
this.sprite.spriteFrame = null;
this.bvK.length = 0;
return [ 4, v.bvv().bvz(t) ];

case 1:
e = o.sent();
if (!this.isValid) return [ 2 ];
if (this.bvD.id !== t.id) return [ 2 ];
this.bvK = l(e);
(i = this.bvK[this.bvL]) && (this.sprite.spriteFrame = i);
return [ 2 ];
}
});
});
};
e.prototype.kH = function(t) {
var e = this;
if (this.bvN) {
this.bvO += t * this.bvP;
if (this.bvP >= 0 ? this.bvO >= this.bvG : this.bvO <= this.bvI) if (this.bvJ) this.bvP >= 0 ? this.bvO = this.bvI : this.bvO = this.bvG; else if (this.bvD.fateOut) {
this.bvN = !1;
cc.tween(this.sprite.node).to(this.bvD.fateOut, {
opacity: 0,
scale: 0
}).call(function() {
e.bvX();
if (e.bvQ) {
e.bvQ(e);
e.bvQ = null;
}
}).start();
} else {
this.bvX();
if (this.bvQ) {
this.bvQ(this);
this.bvQ = null;
}
}
this.bvL = Math.floor(this.bvO / this.bvE);
this.bvL < 0 ? this.bvL = 0 : this.bvL > this.bvF - 1 && (this.bvL = this.bvF - 1);
if (this.bvM !== this.bvL) {
this.bvM = this.bvL;
var i = this.bvK[this.bvL];
i && (this.sprite.spriteFrame = i);
}
}
};
e.prototype.bvS = function(t) {
var e = b.default.bGR.bHQ(t);
if (e) {
var i = b.default.bGR.bHN(e.resName);
if (i) {
this.bvD = e;
this.bvE = 1 / (e.fps || 30);
var o = i.fs.length;
if (e.slice) {
var n = e.slice[0], r = e.slice[1];
o = (r - n) * (r > n ? 1 : -1) + 1;
} else e.frameCount && (o = Math.min(e.frameCount, o));
this.bvT(o, this.bvH);
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
this.bvR();
}
} else $eZ.logMgr.cMd("not find:" + t);
};
e.prototype.bvT = function(t, e) {
this.bvF = t;
this.bvG = this.bvF * this.bvE;
-1 !== e && (this.bvG = Math.max(Math.min(this.bvG, e), this.bvI));
};
e.prototype.reuseSelf = function() {
cc.Tween.stopAllByTarget(this.node);
this.sprite.node.color = h.BATTLE_COLOR_R.WHITE;
this.node.angle = 0;
this.bvN = !1;
this.bvQ = null;
};
e.prototype.bvU = function(t) {
this.sprite.node.color = t;
};
e.prototype.bvV = function(t) {
if (this.node.groupIndex !== t) {
this.node.groupIndex = t;
this.sprite.node.groupIndex = t;
}
};
e.prototype.setTimeScale = function(t) {
this.bvP = t;
};
e.prototype.bvW = function(t, e, i, o, n) {
void 0 === e && (e = !1);
void 0 === o && (o = 0);
void 0 === n && (n = -1);
this.bvQ = i;
this.bvH = n;
this.bvI = o;
this.bvS(t);
if (this.bvD) {
this.bvN = !0;
this.bvL = 0;
this.bvM = -1;
this.bvJ = e;
this.bvP >= 0 ? this.bvO = this.bvI : this.bvO = this.bvG;
}
};
e.prototype.bvX = function() {
this.bvN = !1;
};
r([ d(cc.Sprite) ], e.prototype, "sprite", void 0);
return r([ f ], e);
}(h.IVFXItem);
i.default = y;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/perf/$TF": void 0
} ],
$PF: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "b900cFNsHRNybavll2BuoQg", "$PF");
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
var s = t("../../../bingo/src/$JX"), a = t("../../../game/src/$KK"), l = t("./$PE"), c = cc._decorator, h = c.ccclass, b = c.property, u = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.state = null;
e.bvY = new Map();
e.bvZ = 40;
e.bwa = 10;
return e;
}
e.prototype.start = function() {};
e.prototype.update = function() {};
e.prototype.bwb = function(t) {
var e = this, i = 0;
t.forEach(function(t, o) {
var n, r, c, h = e.bvY.get(o), b = t.val;
if (b > 0) {
for (var u = h || [], p = Math.max(0, b - u.length), f = 0; f < p && (y = a.default.JE.nb.bFN(s.$fb.ResUrl.movieClip)); f++) {
var d = y.getComponent(l.default);
y.angle = 0;
y.scale = null !== (n = t.scale) && void 0 !== n ? n : 1;
e.state.addChild(y);
d.bvW(Number(o), !0);
u.push(y);
}
var v = b * e.bvZ / 2 - .5 * e.bvZ;
for (f = 0; f < u.length; f++) {
var y;
(y = u[f]).active = f < b;
f < b && y.setPosition(f * e.bvZ - v + (null !== (r = t.x) && void 0 !== r ? r : 0), i * e.bvZ + (i - 1) * e.bwa + .5 * e.bvZ + (null !== (c = t.y) && void 0 !== c ? c : 0));
}
e.bvY.set(o, u);
i++;
} else h && e.bwc(o);
});
this.state.height = i * this.bvZ + (i - 1) * this.bwa;
};
e.prototype.bwc = function(t) {
var e = this.bvY.get(t);
if (e) {
for (var i = e.length - 1; i >= 0; i--) a.default.JE.nb.bFX(s.$fb.ResUrl.movieClip, e[i]);
this.bvY.delete(t);
}
};
e.prototype.bwd = function() {
var t = this;
this.bvY.forEach(function(e, i) {
t.bwc(i);
});
this.bvY.clear();
};
r([ b(cc.Node) ], e.prototype, "state", void 0);
return r([ h ], e);
}(cc.Component);
i.default = u;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0,
"./$PE": "$PE"
} ],
$PG: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "83952fISRtDDo7qVcE2A0nK", "$PG");
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
i.$kJ = void 0;
var s = t("../../../game/src/battle/$QA"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.standProgressSprite = null;
e.airProgressSprite = null;
e.lieProgressSprite = null;
return e;
}
e.prototype.ku = function() {};
e.prototype.bwe = function(t, e) {
this.standProgressSprite.node.parent.active = t;
t && (this.standProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
e.prototype.bwf = function(t, e) {
this.airProgressSprite.node.parent.active = t;
t && (this.airProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
e.prototype.bwg = function(t, e) {
this.lieProgressSprite.node.parent.active = t;
t && (this.lieProgressSprite.fillRange = Math.max(0, Math.min(1, e)));
};
r([ c(cc.Sprite) ], e.prototype, "standProgressSprite", void 0);
r([ c(cc.Sprite) ], e.prototype, "airProgressSprite", void 0);
r([ c(cc.Sprite) ], e.prototype, "lieProgressSprite", void 0);
return r([ l ], e);
}(s.IVFXItem);
i.$kJ = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0
} ],
$PH: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7bf1eZ54FJH2rZWJDZJgaN1", "$PH");
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
i.$kK = void 0;
var s = t("../../../game/src/battle/$QA"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sprite = null;
return e;
}
e.prototype.bwh = function(t) {
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
this.lE(a, t.resUrl);
};
e.prototype.setTimeScale = function() {};
e.prototype.bwi = function(t) {
this.sprite.node.color = t;
};
e.prototype.reuseSelf = function() {
this.sprite.node.color = s.BATTLE_COLOR_R.WHITE;
this.lN();
};
r([ c(cc.Sprite) ], e.prototype, "sprite", void 0);
return r([ l ], e);
}(s.IVFXItem);
i.$kK = h;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0
} ],
$PI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "da352tRwgVGvosTaekbsIP5", "$PI");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$kL = void 0;
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/battle/$QA"), h = t("../../../game/src/utils/$TY"), b = t("../../../game/src/perf/$TF"), u = cc._decorator, p = u.ccclass, f = u.property, d = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.resUrl = "";
e.sp = null;
e.mask = null;
e.mi = null;
e.mj = "";
e.mk = "";
e.mt = !1;
e.mv = !1;
return e;
}
e.prototype.onLoad = function() {
this.sp.setCompleteListener(this.ml.bind(this));
};
e.prototype.ky = function() {
this.mi = null;
this.mj = "";
};
e.prototype.ml = function() {
this.mi && this.mi();
};
e.prototype.mm = function(t) {
this.sp.premultipliedAlpha = t;
};
e.prototype.mn = function(t) {
h.$sk.cHz(this.sp, t);
};
e.prototype.mo = function(t, e, i, o, n, r) {
return s(this, void 0, void 0, function() {
var s, l;
return a(this,function AWAITF10(a){switch (a.label) {
case 0:
a.trys.push([ 0, 2, , 3 ]);
this.mi = o;
this.sp.node.active = !1;
return [ 4, this.mq(t) ];

case 1:
a.sent();
if (this.isValid && this.mp(t)) {
this.sp.node.active = !0;
if (s = this.sp.setAnimation(0, e, i)) {
n && (s.animationStart = n);
r && (s.animationEnd = r);
}
}
return [ 3, 3 ];

case 2:
l = a.sent();
$eZ.logMgr.info("SpineEffect play:", l);
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
e.prototype.mp = function(t) {
return this.mk === t;
};
e.prototype.mq = function(t) {
return s(this, void 0, void 0, function() {
var e;
return a(this,function AWAITF11(i){switch (i.label) {
case 0:
if (b.HeatIsolate.cyY) return [ 2, !1 ];
if (this.mk === t) return [ 2, !0 ];
if (this.mj === t) return [ 2, !1 ];
this.mj = t;
i.label = 1;

case 1:
i.trys.push([ 1, 3, , 4 ]);
return [ 4, this.lK(new l.$fb.ResCfg(t, sp.SkeletonData)) ];

case 2:
if ((e = i.sent()) && e.Da) {
if (this.mj === t && this.isValid) {
this.mj = "";
this.mk = t;
this.ms(e.Da);
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
e.prototype.mr = function(t, e) {
var i = this.sp.node;
if (i.x !== t || i.y !== e) {
i.x = t;
i.y = e;
}
};
e.prototype.ms = function(t) {
this.sp.skeletonData = t;
};
e.prototype.mu = function(t) {
if (this.mt !== t) {
this.mt = t;
var e = this.sp.getMaterial(0);
e.setProperty("enableFlashWhite", t ? 1 : 0);
this.sp.setMaterial(0, e);
}
};
e.prototype.mw = function(t) {
this.mv = !0;
this.mu(!0);
this.scheduleOnce(this.mx.bind(this), t);
};
e.prototype.mx = function() {
this.mu(!1);
this.mv = !1;
};
e.prototype.reuseSelf = function() {
this.mr(0, 0);
this.node.x = 0;
this.node.y = 0;
this.mask.enabled = !1;
this.node.anchorX = .5;
this.node.opacity = 255;
this.node.angle = 0;
this.mu(!1);
this.mk = "";
this.mj = "";
this.sp.isValid && (this.sp.skeletonData = null);
this.lN();
};
r([ f({
type: sp.Skeleton
}) ], e.prototype, "sp", void 0);
r([ f(cc.Mask) ], e.prototype, "mask", void 0);
return r([ p ], e);
}(c.IVFXItem);
i.$kL = d;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/perf/$TF": void 0,
"../../../game/src/utils/$TY": void 0
} ],
$PJ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "c8e50z3cxdBaoOLdXCXkpsK", "$PJ");
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
var s = cc._decorator, a = s.ccclass, l = (s.property, function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.bwj = 0;
e.bwk = 0;
e.bwl = 1;
e.bwm = 1;
e.bwn = cc.v2();
return e;
}
e.prototype.bwo = function(t, e, i, o) {
this.bwj = t;
this.bwk = e;
this.bwl = i;
this.bwm = o;
this.bwp();
};
e.prototype.bwp = function() {
var t = cc.Camera.findCamera(this.node);
if (t) {
var e = t.zoomRatio;
t.zoomRatio = 1;
var i = t.getScreenToWorldPoint(cc.Vec2.ZERO_R, this.bwn);
t.zoomRatio = e;
var o = 1 / e;
this.node.scaleX = o * this.bwl;
this.node.scaleY = o * this.bwm;
this.node.setPosition(this.bwj * o + i.x, this.bwk * o + i.y);
}
};
e.prototype.onDisable = function() {
this.destroy();
};
e.prototype.update = function() {
this.bwp();
};
return r([ a ], e);
}(cc.Component));
i.default = l;
cc._RF.pop();
}, {} ],
$PK: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "6529cVI+DBM+LiFS1V4swgQ", "$PK");
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
i.$kN = i.$kS = i.$kR = void 0;
var s, a, l, c = t("../../../game/src/battle/$QA"), h = t("../../battleLogic/$KC"), b = t("../../../game/src/battle/types/$WY"), u = cc._decorator, p = u.ccclass, f = u.property;
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(s = i.$kR || (i.$kR = {}));
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(a = i.$kS || (i.$kS = {}));
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
t[t.d = 2] = "d";
t[t.e = 3] = "e";
t[t.f = 4] = "f";
})(l || (l = {}));
var d = cc.color(255, 0, 0, 255), v = cc.color(255, 0, 0, 127.5), y = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.commonSprite = null;
e.negSprite = null;
e.fanSprite = null;
e.resUrl = "";
e.bwq = null;
e.bwr = null;
e.bws = l.f;
e.bwt = 0;
e.bwu = 0;
e.bwv = 0;
e.bww = 0;
e.bwx = null;
e.bwy = 1;
e.bwz = null;
e.bwA = null;
e.bwB = 80;
e.bwC = .333;
e.bwD = 4;
return e;
}
e.prototype.ku = function() {};
e.prototype.bwE = function(t) {
var e = null;
this.commonSprite && (this.commonSprite.node.active = !1);
this.negSprite && (this.negSprite.node.active = !1);
this.fanSprite && (this.fanSprite.node.active = !1);
"common" === t ? e = this.commonSprite : "neg" === t ? e = this.negSprite : "fan" === t && (e = this.fanSprite);
this.bwq = e;
if (this.bwq) {
cc.isValid(this.bwq.node) && (this.bwq.node.active = !0);
this.bwr = this.bwq.getMaterial(0);
this.bwr.setProperty("fillProgress", 0);
this.bwr.setProperty("alpha", 1);
this.bwr.setProperty("shapeType", s.b);
this.bwr.setProperty("fillMode", 0);
this.bwr.setProperty("glowSize", 4);
this.bwr.setProperty("glowStrength", 2);
this.bwr.setProperty("outlineWidth", 2);
this.bwr.setProperty("outlineColor", d);
this.bwr.setProperty("fillColor", v);
this.bwr.setProperty("nodeSize", cc.v2(0, 0));
this.bwr.setProperty("shapePadding", cc.v2(0, 0));
this.bwq.node.width = 0;
this.bwq.node.height = 0;
this.bwq.node.anchorX = .5;
this.bwq.node.anchorY = .5;
this.bwq.node.angle = 0;
this.bwq.node.scaleX = 1;
this.bwq.node.scaleY = 1;
this.bwq.node.color = cc.Color.WHITE;
this.bwq.node.opacity = 255;
} else this.bwr = null;
};
e.prototype.bwF = function(t, e) {
this.bwE("common");
if (this.bwr) {
this.bwH(t);
this.bwQ(null != t.k ? t.k : 1, null != t.l ? t.l : .4, null != t.m ? t.m : .1, e);
}
};
e.prototype.bwG = function(t, e) {
this.bwE("common");
if (this.bwr) {
this.bwI(t);
this.bwQ(null != t.l ? t.l : 1, null != t.m ? t.m : .4, null != t.n ? t.n : .1, e);
}
};
e.prototype.bwH = function(t) {
var e = null != t.j ? t.j : .333, i = null != t.h ? t.h : 4, o = 2 * t.b + 2 * i, n = 2 * t.b * e + 2 * i, r = this.bwq.node;
r.width = o;
r.height = n;
r.anchorX = .5;
r.anchorY = .5;
this.bwr.setProperty("shapeType", s.b);
this.bwr.setProperty("fillMode", 0);
this.bwP(o, n, i / o, i / n, t.d || d, t.e || v, null != t.f ? t.f : 2, null != t.g ? t.g : 0, i, null != t.i ? t.i : 2);
};
e.prototype.bwI = function(t) {
var e = null != t.k ? t.k : .333, i = null != t.i ? t.i : 4, o = t.c * e, n = t.b, r = o + 2 * i, l = this.bwq.node;
l.width = n;
l.height = r;
l.anchorX = t.o;
l.anchorY = t.p;
var c = null != t.d ? t.d : a.b;
this.bwr.setProperty("shapeType", s.c);
this.bwr.setProperty("fillMode", c);
this.bwP(n, r, 0, i / r, t.e || d, t.f || v, null != t.g ? t.g : 2, null != t.h ? t.h : 0, i, null != t.j ? t.j : 2);
};
e.prototype.bwJ = function(t, e) {
this.bwE("neg");
if (this.bwr) {
this.bwH(t);
var i = t.b, o = null != t.o ? t.o : .3 * i, n = i > 0 ? o / i : .3;
this.bwr.setProperty("innerRatio", Math.max(0, Math.min(1, n)));
this.bwQ(null != t.k ? t.k : 1, null != t.l ? t.l : .4, null != t.m ? t.m : .1, e);
}
};
e.prototype.bwK = function(t, e) {
this.bwE("neg");
if (this.bwr) {
this.bwI(t);
var i = null != t.k ? t.k : .333, o = t.c * i * .5, n = null != t.r ? t.r : .3 * o, r = o > 0 ? n / o : .3;
this.bwr.setProperty("innerRatio", Math.max(0, Math.min(1, r)));
this.bwQ(null != t.l ? t.l : 1, null != t.m ? t.m : .4, null != t.n ? t.n : .1, e);
}
};
e.prototype.bwL = function(t, e) {
this.bwE("fan");
if (this.bwr) {
this.bwM(t);
this.bwQ(null != t.n ? t.n : 1, null != t.o ? t.o : .4, null != t.p ? t.p : .1, e);
}
};
e.prototype.bwM = function(t) {
var e = null != t.m ? t.m : .333, i = null != t.k ? t.k : 4, o = 2 * t.b + 2 * i, n = 2 * t.b * e + 2 * i, r = this.bwq.node;
r.width = o;
r.height = n;
r.anchorX = .5;
r.anchorY = .5;
var s = t.b, a = 0;
t.c && (a = null != t.d ? t.d : .3 * s);
var l = s > 0 ? a / s : 0, c = null != t.e ? t.e : 0, h = null != t.f ? t.f : 60, b = Math.PI / 180, u = c * b, p = .5 * Math.max(0, Math.min(180, h)) * b;
this.bwr.setProperty("innerRatio", Math.max(0, Math.min(1, l)));
this.bwr.setProperty("fanAngle", u);
this.bwr.setProperty("fanArcHalf", p);
this.bwP(o, n, i / o, i / n, t.g || d, t.h || v, null != t.i ? t.i : 2, null != t.j ? t.j : 0, i, null != t.l ? t.l : 2);
};
e.prototype.prefabPlay = function() {};
e.prototype.bwN = function(t, e) {
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
t.isNeg ? this.bwK(r, e) : this.bwG(r, e);
if (t.lockLine) {
this.bwA = t.lockLine;
this.bwB = null !== (n = t.h) && void 0 !== n ? n : 80;
this.bwC = .333;
this.bwD = 4;
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
t.isNeg ? this.bwJ(s, e) : this.bwF(s, e);
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
this.bwL(a, e);
}
};
e.prototype.setTimeScale = function(t) {
this.bwy = t;
};
e.prototype.bwO = function(t) {
this.bwz = t;
this.bwA && this.bwR();
};
e.prototype.reuseSelf = function() {
this.bws = l.f;
this.bwt = 0;
this.bwx = null;
this.bwy = 1;
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
this.bwq = null;
this.bwr = null;
this.bwz = null;
this.bwA = null;
this.lN();
};
e.prototype.bwP = function(t, e, i, o, n, r, s, a, l, c) {
this.bwr.setProperty("nodeSize", cc.v2(t, e));
this.bwr.setProperty("shapePadding", cc.v2(i, o));
this.bwr.setProperty("outlineColor", n);
this.bwr.setProperty("fillColor", r);
this.bwr.setProperty("outlineWidth", s);
this.bwr.setProperty("innerGradient", a);
this.bwr.setProperty("glowSize", l);
this.bwr.setProperty("glowStrength", c);
};
e.prototype.bwQ = function(t, e, i, o) {
this.bwu = t;
this.bwv = e;
this.bww = i;
this.bwx = o || null;
this.bws = l.b;
this.bwt = 0;
this.bwr.setProperty("fillProgress", 0);
this.bwr.setProperty("alpha", 1);
};
e.prototype.update = function(t) {
if (this.bws !== l.f && this.bwr) {
this.bwA && this.bwz && this.bwR();
this.bwt += t * this.bwy * this.worldTimeScale;
switch (this.bws) {
case l.b:
this.bws = l.c;
this.bwt = 0;
break;

case l.c:
var e = Math.min(this.bwt / this.bwu, 1);
this.bwr.setProperty("fillProgress", e);
if (e >= 1) {
this.bws = l.d;
this.bwt = 0;
}
break;

case l.d:
if (this.bwt >= this.bwv) {
this.bws = l.e;
this.bwt = 0;
}
break;

case l.e:
e = Math.min(this.bwt / this.bww, 1);
this.bwr.setProperty("alpha", 1 - e);
if (e >= 1) {
this.bws = l.f;
if (this.bwx) {
this.bwx();
this.bwx = null;
}
}
}
}
};
e.prototype.bwR = function() {
if (this.bwq && this.bwr) {
var t = this.bwz;
if (t && t.hw) {
var e = this.bwA, i = e.from || {
type: b.$uD.$vu.f
}, o = h.default.EJ(t, i), n = h.default.EJ(t, e.to);
if (o && o.hw && n && n.hw) {
var r = b.$uD.TOWN_SCENE_DEFINE.SCENE_SCALE_Y, s = n.hw.x - o.hw.x, a = (n.hw.y - o.hw.y) * r, l = Math.sqrt(s * s + a * a);
e.extend && (l += e.extend);
var c = 180 * Math.atan2(a, s) / Math.PI, u = this.bwB * this.bwC + 2 * this.bwD, p = this.bwq.node;
p.width = l;
p.height = u;
p.anchorX = 0;
p.anchorY = .5;
p.angle = c;
this.node.x = o.hw.x;
this.node.y = o.hw.y * r + (o.hw.z || 0);
this.bwr.setProperty("nodeSize", cc.v2(l, u));
}
}
}
};
r([ f(cc.Sprite) ], e.prototype, "commonSprite", void 0);
r([ f(cc.Sprite) ], e.prototype, "negSprite", void 0);
r([ f(cc.Sprite) ], e.prototype, "fanSprite", void 0);
return r([ p ], e);
}(c.IVFXItem);
i.$kN = y;
cc._RF.pop();
}, {
"../../../game/src/battle/$QA": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../battleLogic/$KC": void 0
} ],
$PL: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7b739nic0tHhJhDEs2wKhEj", "$PL");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/manager/$RV"), h = cc._decorator, b = h.ccclass, u = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.btn_again = null;
e.progressBar_abyss = null;
e.lbl_abyss_progress = null;
e.bwS = !1;
return e;
}
e.prototype.ks = function() {};
e.prototype.ku = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF12(){this.kP(this.btn_again.node, this.bwW, this);
this.btn_again.node.active = !1;
this.bwS = !1;
this.bwX();
$eZ.chapter.kn(c.$nCCHAPTER_EVENT$nA.b, this.bwY, this);
return [ 2 ];
});
});
};
e.prototype.ky = function() {
$eZ.chapter.kp(this);
};
e.prototype.kH = function() {
this.bwT();
};
e.prototype.bwT = function() {
var t = $eZ.battleMgr.bDu.nz;
if (t) {
var e = t.hi.bnl.fI;
if (e && !$eZ.battleMgr.bDv.bCS && t.hi.bnl.hf === l.$fb.tblEnum.InstancePlayModeTableEnum.ABYSS) {
var i = t.hi.bnl.gi(e.dcT);
if (null == i ? void 0 : i.hw) {
var o = e.dcU && e.dcO && this.bwU(t.hw.x, t.hw.y, i.hw.x - 100, i.hw.y - 150, 200, 300);
this.bwV(o);
}
}
}
};
e.prototype.bwU = function(t, e, i, o, n, r) {
return t >= i && t <= i + n && e >= o && e <= o + r;
};
e.prototype.bwV = function(t) {
if (this.bwS !== t) {
this.bwS = t;
this.btn_again.node.active = t;
}
};
e.prototype.bwW = function() {
var t, e, i, o, n;
if (!$eZ.battleMgr.bDv.bCS) {
var r = null === (e = null === (t = $eZ.battleMgr.bDv) || void 0 === t ? void 0 : t.bCD) || void 0 === e ? void 0 : e.instanceId, s = null === (n = null === (o = null === (i = $eZ.battleMgr.bDv) || void 0 === i ? void 0 : i.bCD) || void 0 === o ? void 0 : o.svrInstanceParams) || void 0 === n ? void 0 : n.playMode;
if (r && s) {
var a = {
id: r,
playMode: s
};
$eZ.battleMgr.bEo(a);
}
}
};
e.prototype.bwX = function() {
var t, e = $eZ.chapter.caQ(null === (t = $eZ.battleMgr.bDv) || void 0 === t ? void 0 : t.bCU);
this.progressBar_abyss.progress = e;
this.lbl_abyss_progress.string = Math.floor(100 * Math.max(0, Math.min(e, 1))) + "%";
};
e.prototype.bwY = function() {
this.bwX();
};
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_again", void 0);
r([ u({
type: cc.ProgressBar,
displayName: ""
}) ], e.prototype, "progressBar_abyss", void 0);
r([ u({
type: cc.Label,
displayName: ""
}) ], e.prototype, "lbl_abyss_progress", void 0);
return r([ b ], e);
}(l.$fb.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/manager/$RV": void 0
} ],
$PM: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ded44IYS4xNEowmwHimGe+K", "$PM");
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
i.$kW = void 0;
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.buffIcon = null;
e.cdSprite = null;
e.bwZ = "";
e.bxa = NaN;
return e;
}
e.prototype.bxb = function() {
if (!this.buffIcon) {
var t = this.node.getChildByName("icon");
t && (this.buffIcon = t.getComponent(cc.Sprite));
}
if (!this.cdSprite) {
var e = this.node.getChildByName("mask"), i = e && e.children.length > 0 ? e.children[0] : null;
i && (this.cdSprite = i.getComponent(cc.Sprite));
}
};
e.prototype.bxc = function(t, e) {
this.bxb();
if (this.buffIcon && this.bwZ !== t) {
this.bwZ = t;
this.lE(this.buffIcon, t);
}
if (this.cdSprite) if (e < 0) {
this.cdSprite.node.active && (this.cdSprite.node.active = !1);
this.bxa = e;
} else {
this.cdSprite.node.active || (this.cdSprite.node.active = !0);
var i = cc.misc.clampf(e, 0, 1);
if (this.bxa !== i) {
this.bxa = i;
this.cdSprite.fillRange = i;
}
}
};
r([ c(cc.Sprite) ], e.prototype, "buffIcon", void 0);
r([ c(cc.Sprite) ], e.prototype, "cdSprite", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
var b = function() {
function t() {
this.bxd = null;
this.bxe = null;
this.bxf = [];
}
t.prototype.bxg = function(t, e) {
this.bxd = t;
this.bxe = e;
this.bxe && this.bxe.isValid && (this.bxe.active = !1);
};
Object.defineProperty(t.prototype, "bxh", {
get: function() {
return !!(this.bxd && this.bxd.isValid && this.bxe && this.bxe.isValid);
},
enumerable: !1,
configurable: !0
});
t.prototype.bxi = function(t) {
if (this.bxh) {
for (var e = this.bxf.length; e < t.length; e++) {
var i = cc.instantiate(this.bxe);
i.active = !0;
this.bxd.addChild(i);
var o = i.getComponent(h) || i.addComponent(h);
this.bxf.push(o);
}
for (e = this.bxf.length - 1; e >= t.length; e--) {
var n = this.bxf[e];
n && n.node && n.node.isValid && n.node.destroy();
this.bxf.splice(e, 1);
}
for (e = 0; e < t.length; e++) this.bxf[e].bxc(t[e].b, t[e].c);
}
};
t.prototype.bxj = function() {
this.bxi([]);
};
return t;
}();
i.$kW = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$PN: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "17ca3N+5L5OwpGVw/qJGWri", "$PN");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$kZ = void 0;
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/$KK"), h = cc._decorator, b = h.ccclass, u = h.property;
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
t[t.d = 2] = "d";
})(i.$kZ || (i.$kZ = {}));
var p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.node_emojiContainer = null;
return e;
}
Object.defineProperty(e.prototype, "bxl", {
set: function(t) {
this.bxk = t;
},
enumerable: !1,
configurable: !0
});
e.prototype.ks = function() {
this.bxm = c.default.personal.personalData.getCurSettingEmoji();
};
e.prototype.ku = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF13(n){switch (n.label) {
case 0:
t = 0;
n.label = 1;

case 1:
return t < this.bxm.length ? (e = this.node_emojiContainer.children[t]) ? [ 3, 3 ] : [ 4, this.lC("game:res/prefab/common/emojiItem") ] : [ 3, 5 ];

case 2:
i = n.sent();
if (!this.isValid) return [ 2 ];
(e = cc.instantiate(i)).setParent(this.node_emojiContainer);
e.scale = .65;
n.label = 3;

case 3:
(o = e.getComponent(EmojiItem)).initEmoji(this.bxm[t]);
o.clickCallBack = this.bxn.bind(this);
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
e.prototype.bxn = function(t) {
var e;
return s(this, void 0, void 0, function() {
var i;
return a(this,function AWAITF14(o){switch (o.label) {
case 0:
i = JSON.stringify({
type: 2,
chatType: 1,
chatData: t
});
return [ 4, $eZ.battleServer.cVw(i) ];

case 1:
o.sent();
this.node.active = !1;
null === (e = this.bxk) || void 0 === e || e.call(this, t);
return [ 2 ];
}
});
});
};
r([ u({
type: cc.Node,
displayName: ""
}) ], e.prototype, "node_emojiContainer", void 0);
return r([ b ], e);
}(l.$fb.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0
} ],
$PO: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "17f04VWJMNKvoT4LgJJYOyC", "$PO");
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
i.$ld = void 0;
var s, a = t("../../../bingo/src/$JX"), l = t("../render/$OQ"), c = t("../render/$OR"), h = cc._decorator, b = h.ccclass, u = h.property, p = function(t, e) {
return e.g - t.g;
};
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
})(s = i.$ld || (i.$ld = {}));
var f = function() {
function t() {
this.bxo = null;
this.bxp = null;
this.bxq = null;
this.bxr = [];
}
t.prototype.bxs = function(t, e, i) {
this.bxo = t;
this.bxp = e;
this.bxq = i;
this.bxp && this.bxp.isValid && (this.bxp.active = !1);
};
Object.defineProperty(t.prototype, "bxt", {
get: function() {
return !!(this.bxo && this.bxo.isValid && this.bxp && this.bxp.isValid);
},
enumerable: !1,
configurable: !0
});
t.prototype.bxu = function(t) {
if (!this.bxt) return this.bxr;
for (var e = this.bxr.length; e < t; e++) {
var i = cc.instantiate(this.bxp);
i.active = !0;
this.bxo.addChild(i);
this.bxr.push(this.bxq(i));
}
for (e = this.bxr.length - 1; e >= t; e--) {
var o = this.bxr[e];
o && o.node && o.node.isValid && o.node.destroy();
this.bxr.splice(e, 1);
}
return this.bxr;
};
t.prototype.bxv = function() {
this.bxu(0);
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
e.bxw = new f();
e.bxx = new f();
e.bxy = [];
e.bxz = [];
e.bxA = s.b;
e.bxB = !1;
return e;
}
e.prototype.ks = function() {
this.bxC();
};
e.prototype.bxC = function() {
if (!this.bxB) {
this.bxB = !0;
this.bxw.bxs(this.cell1Container ? this.cell1Container.node : null, this.cell1Prefab, function(t) {
return t.getComponent(l.default);
});
this.bxx.bxs(this.cell2Container ? this.cell2Container.node : null, this.cell2Prefab, function(t) {
return t.getComponent(c.default);
});
this.bxE();
this.bxF(this.bxD(), !1);
}
};
e.prototype.bxD = function() {
var t = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
if (t) for (var e = 0; e < t.length; e++) if (t[e] && t[e].isChecked) return e === s.c ? s.c : s.b;
return s.b;
};
e.prototype.bxE = function() {
var t = this, e = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
e && e.forEach(function(e, i) {
e && e.node.on("toggle", function() {
e.isChecked && t.bxF(i === s.c ? s.c : s.b, !1);
}, t);
});
};
e.prototype.bxF = function(t, e) {
void 0 === e && (e = !0);
this.bxA = t;
this.cell1Container && (this.cell1Container.node.active = t === s.b);
this.cell2Container && (this.cell2Container.node.active = t === s.c);
e && this.bxG(t);
};
e.prototype.bxG = function(t) {
var e = this.switchToggleContainer ? this.switchToggleContainer.toggleItems : null;
if (e) for (var i = 0; i < e.length; i++) e[i] && (e[i].isChecked = i === t);
};
e.prototype.bxH = function(t) {
this.bxC();
this.bxy = t || [];
this.bxI();
this.bxJ();
};
e.prototype.bxI = function() {
for (var t, e = this.bxw.bxu(this.bxy.length), i = 0; i < e.length; i++) {
var o = this.bxy[i];
e[i].brL({
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
e.prototype.bxJ = function() {
for (var t, e = 0, i = 0; i < this.bxy.length; i++) e += Math.max(0, this.bxy[i].g);
var o = this.bxz, n = this.bxy.length;
o.length = n;
for (i = 0; i < n; i++) o[i] = this.bxy[i];
o.sort(p);
var r = this.bxx.bxu(o.length);
for (i = 0; i < r.length; i++) {
var s = o[i], a = e > 0 ? Math.max(0, s.g) / e : 0;
r[i].brQ({
b: s.c,
c: s.d,
d: s.g,
e: a,
f: s.i,
g: null !== (t = s.j) && void 0 !== t ? t : 0
});
}
};
e.prototype.bxK = function() {
this.bxy = [];
this.bxw.bxv();
this.bxx.bxv();
};
r([ u(cc.Node) ], e.prototype, "cell1Prefab", void 0);
r([ u(cc.Node) ], e.prototype, "cell2Prefab", void 0);
r([ u(cc.Layout) ], e.prototype, "cell1Container", void 0);
r([ u(cc.Layout) ], e.prototype, "cell2Container", void 0);
r([ u(cc.ToggleContainer) ], e.prototype, "switchToggleContainer", void 0);
return r([ b ], e);
}(a.$fb.Component);
i.default = d;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../render/$OQ": "$OQ",
"../render/$OR": "$OR"
} ],
$PP: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "400fa8nzi9Njb6GM/gmVxWq", "$PP");
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
var s = t("../../../bingo/src/$JX"), a = cc._decorator, l = a.ccclass, c = a.property, h = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.avatarbg = null;
e.avatar = null;
e.avatarbg2 = null;
e.line = null;
return e;
}
e.prototype.ks = function() {};
e.prototype.ku = function() {};
e.prototype.bxL = function(t, e, i) {
s.$fb.utils.bex(this.avatar, !i);
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
e.prototype.bxM = function(t, e) {
if (t._battleUI_sprite !== e) {
t._battleUI_sprite = e;
this.lB(new s.$fb.ResCfg(e, cc.SpriteFrame), function(i, o) {
i || t.isValid && t._battleUI_sprite === e && (t.spriteFrame = o.Da);
});
}
};
r([ c(cc.Sprite) ], e.prototype, "avatarbg", void 0);
r([ c(cc.Sprite) ], e.prototype, "avatar", void 0);
r([ c(cc.Sprite) ], e.prototype, "avatarbg2", void 0);
r([ c(cc.Sprite) ], e.prototype, "line", void 0);
return r([ l ], e);
}(s.$fb.Component);
i.default = h;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0
} ],
$PQ: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "33b70HR+JdEaK6TqtRwrQVT", "$PQ");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/$KK"), h = cc._decorator, b = h.ccclass, u = h.property, p = function(t) {
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
e.bxN = [];
e.bxO = [ 1, 2, 4, 8 ];
return e;
}
e.prototype.ks = function() {};
e.prototype.ku = function() {
return s(this, void 0, void 0, function() {
var t = this;
return a(this,function AWAITF15(){this.kP(this.btn_pause.node, this.bxQ, this);
this.kP(this.btn_changeCamera.node, this.bxR, this);
this.bxN = [ this.btn_option_0, this.btn_option_1, this.btn_option_2, this.btn_option_3 ];
this.bxO.forEach(function(e, i) {
var o = t.bxN[i];
o.node.getChildByName("lblTitle").getComponent(cc.Label).string = "x" + e;
o.node.on("click", function() {
c.default.JE.mX = e;
t.bxP();
});
});
this.bxP();
this.btn_pause.node.children[1].getComponent(cc.Label).string = "暂停";
return [ 2 ];
});
});
};
e.prototype.kH = function() {
this.lbl_progress.string = c.default.JE.mY + "/" + c.default.JE.my.bCT.length;
};
e.prototype.bxP = function() {
var t = this;
this.bxN.forEach(function(e, i) {
var o = t.bxO.indexOf(c.default.JE.mX);
e.node.children[0].active = i !== o;
e.node.children[1].active = i === o;
});
};
e.prototype.bxQ = function() {
c.default.JE.mH = !c.default.JE.mH;
c.default.JE.mH && $eZ.fguiMgr.cSs({
msgType: l.$fb.MsgBoxType.BattlePlaybackExitConfirm,
message: "是否退出战斗回放？",
btnOK: function() {
$eZ.battleMgr.bDY();
},
btnCancel: function() {
c.default.JE.mH = !1;
},
okStr: " 确定",
cancelStr: "取消"
});
this.bxP();
};
e.prototype.bxR = function() {
var t = c.default.JE.ne.iu, e = c.default.JE.ng.fF.findIndex(function(e) {
return e.dca === t;
});
if (-1 !== e) {
e = (e + 1) % c.default.JE.ng.fF.length;
var i = c.default.JE.ng.fF[e].dca;
c.default.JE.ne.kb(i);
}
};
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_pause", void 0);
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_0", void 0);
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_1", void 0);
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_2", void 0);
r([ u({
type: cc.Button,
displayName: ""
}) ], e.prototype, "btn_option_3", void 0);
r([ u(cc.Label) ], e.prototype, "lbl_progress", void 0);
r([ u(cc.Button) ], e.prototype, "btn_changeCamera", void 0);
return r([ b ], e);
}(l.$fb.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0
} ],
$PR: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4e496nVfq1EzImioNNqu/qx", "$PR");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/battle/types/$WY"), h = t("../../../game/src/$KK"), b = cc._decorator, u = b.ccclass, p = b.property, f = function(t) {
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
e.bxS = !1;
return e;
}
e.prototype.ku = function() {
this.kP(this.btn_pet.node, this.bxT, this);
this.kP(this.btn_skill.node, this.bxU, this);
this.kP(this.btn_bag.node, this.bxV, this);
this.kP(this.btn_resetStatus.node, this.bxW, this);
this.kP(this.btn_pause.node, this.bxX, this);
this.kP(this.btn_record.node, this.bxY, this);
this.kP(this.btn_start.node, this.bya, this);
this.kP(this.btn_stop.node, this.byb, this);
this.node_time.active = !1;
this.bxS = !1;
this.byc();
$eZ.battleMgr.bDu.nk.bBd(c.$uD.$vA.by, this.byd, this);
$eZ.battleMgr.bDu.nz && ($eZ.battleMgr.bDu.nz.hv.bgM = 1);
};
e.prototype.ky = function() {
$eZ.battleMgr.bDu.nk.bBf(this);
};
e.prototype.kH = function() {};
e.prototype.bxT = function() {
$eZ.fguiMgr.cRP(h.default.JB.PetPanel);
};
e.prototype.bxU = function() {
$eZ.fguiMgr.cRP(h.default.JB.SkillPanel);
};
e.prototype.bxV = function() {
$eZ.fguiMgr.cRP(h.default.JB.BagPanel);
};
e.prototype.bxW = function() {
if ($eZ.battleMgr.bDu.nz) {
$eZ.battleMgr.bDu.ng.hc($eZ.battleMgr.bDu.nz);
$eZ.battleMgr.bDu.nz.hv.bgM = 1;
$eZ.fguiMgr.cRI("重置状态");
}
};
e.prototype.bxX = function() {
var t = this;
$eZ.battleMgr.bDu.mH = !0;
$eZ.fguiMgr.cSt({
msgType: l.$fb.MsgBoxType.BattlePvpTrainingExitConfirm,
message: "确实退出战斗？",
okStr: "确定",
cancelStr: "取消",
btnOK: function() {
return s(t, void 0, void 0, function() {
return a(this,function AWAITF16(){$eZ.battleMgr.bDZ();
return [ 2 ];
});
});
},
btnCancel: function() {
$eZ.battleMgr.bDu.mH = !1;
}
});
};
e.prototype.bxY = function() {
this.bxZ();
};
e.prototype.bxZ = function() {
var t, e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my;
if (e) {
var i = {
b: e.bDn(),
c: $eZ.battleMgr.bDu.ng ? $eZ.battleMgr.bDu.ng.fk : 0
};
$eZ.fguiMgr.cRP(h.default.JB.DamagestatisticsPanel, i);
}
};
e.prototype.bya = function() {
$eZ.fguiMgr.cRP(h.default.JB.PvpTrainingSettingPanel);
};
e.prototype.byb = function() {
$eZ.battleMgr.bDu.ng.he();
$eZ.battleMgr.bDu.nd.bCt();
this.bxS = !1;
this.byc(!0);
};
e.prototype.byc = function(t) {
void 0 === t && (t = !1);
this.btn_bag.node.active = !this.bxS;
this.btn_skill.node.active = !this.bxS;
this.btn_start.node.active = !this.bxS;
this.btn_stop.node.active = this.bxS;
t && $eZ.battleMgr.bDu.nk.bBh(c.$uD.$vA.bA, this.bxS);
};
e.prototype.byd = function(t) {
$eZ.battleMgr.bDu.ng.hd(t, $eZ.battleMgr.bDv.bCR);
this.bxS = !0;
this.byc(!0);
this.bxW();
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
return r([ u ], e);
}(l.$fb.Component);
i.default = f;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/battle/types/$WY": void 0
} ],
$PS: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "d85f9hQYLZBKJOsGardUq2e", "$PS");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/battle/$QM"), h = t("../../../game/src/battle/types/$WY"), b = t("../../../game/src/$KK"), u = t("../../battleLogic/$KC"), p = cc._decorator, f = p.ccclass, d = p.property, v = function(t) {
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
e.bye = !1;
return e;
}
e.prototype.ku = function() {
this.kP(this.btn_pet.node, this.byf, this);
this.kP(this.btn_skill.node, this.byg, this);
this.kP(this.btn_bag.node, this.byh, this);
this.kP(this.btn_resetStatus.node, this.byi, this);
this.kP(this.btn_pause.node, this.byj, this);
this.kP(this.btn_record.node, this.byk, this);
this.kP(this.btn_option.node, this.bym, this);
this.kP(this.btn_summon.node, this.byn, this);
this.kP(this.btn_clear.node, this.byo, this);
this.kP(this.btn_cancel.node, this.byp, this);
this.node_time.active = !1;
$eZ.battleMgr.bDu.nk.bBd(h.$uD.$vA.bu, this.byq, this);
$eZ.battleMgr.bDu.nk.bBd(h.$uD.$vA.bw, this.bys, this);
this.byt();
$eZ.battleMgr.bDu.nz && ($eZ.battleMgr.bDu.nz.hv.bgM = 1);
};
e.prototype.ky = function() {
$eZ.battleMgr.bDu.nk.bBf(this);
};
e.prototype.kH = function() {
this.byr();
};
e.prototype.byf = function() {
$eZ.fguiMgr.cRP(b.default.JB.PetPanel);
};
e.prototype.byg = function() {
$eZ.fguiMgr.cRP(b.default.JB.SkillPanel);
};
e.prototype.byh = function() {
$eZ.fguiMgr.cRP(b.default.JB.BagPanel);
};
e.prototype.byi = function() {
if ($eZ.battleMgr.bDu.nz && !$eZ.battleMgr.bDu.my.bCQ.bGN()) {
$eZ.battleMgr.bDu.ng.gX($eZ.battleMgr.bDu.nz, $eZ.battleMgr.bDu.my.bCQ);
$eZ.battleMgr.bDu.nz.hv.bgM = 1;
$eZ.fguiMgr.cRI("重置状态");
}
};
e.prototype.byj = function() {
var t = this;
$eZ.battleMgr.bDu.mH = !0;
$eZ.fguiMgr.cSt({
msgType: l.$fb.MsgBoxType.BattleTrainingExitConfirm,
message: "确实退出战斗？",
okStr: "确定",
cancelStr: "取消",
btnOK: function() {
return s(t, void 0, void 0, function() {
return a(this,function AWAITF17(){$eZ.battleMgr.bDZ();
return [ 2 ];
});
});
},
btnCancel: function() {
$eZ.battleMgr.bDu.mH = !1;
}
});
};
e.prototype.byk = function() {
$eZ.battleMgr.bDu.my.bCQ.bGN() || this.byl();
};
e.prototype.byl = function() {
var t, e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my;
if (e) {
var i = {
b: e.bDn(),
c: $eZ.battleMgr.bDu.ng ? $eZ.battleMgr.bDu.ng.fk : 0
};
$eZ.fguiMgr.cRP(b.default.JB.DamagestatisticsPanel, i);
}
};
e.prototype.bym = function() {
$eZ.battleMgr.bDu.my.bCQ.bGN() || $eZ.fguiMgr.cRP(b.default.JB.PveTrainingSettingPanel);
};
e.prototype.byn = function() {
var t, e, i, o, n;
if ($eZ.battleMgr.bDu.ng.gF().length >= 6) $eZ.fguiMgr.cRI("已达到召唤怪物上限"); else {
var r = null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCQ;
if (r) {
var s = r.bGN();
if (!s || this.bye) {
var a = null !== (i = r.bGB[r.enemyIndex.bGw]) && void 0 !== i ? i : 0;
if (a) {
var b = null !== (n = null === (o = r.bGC[r.enemyLevel.bGw]) || void 0 === o ? void 0 : o[0]) && void 0 !== n ? n : 1, p = a;
if (c.default.bGR.bHl(p)) {
var f = new h.$uD.$uE();
f.dbH = p;
f.dbO = b;
f.dbS = 0;
f.dbQ = 2;
f.dbG = 0;
f.dbR = c.default.bGR.bHY($eZ.cfgMgr.tbl.RoleTable.VOList[1].id, f.dbO);
f.dbR[l.$fb.tblEnum.AttributeTableEnum.hpMax] = 1e7;
var d = $eZ.battleMgr.bDu.nz;
f.dbJ = d.hw.x + 200 * d.hw.bmF;
f.dbK = d.hw.y;
f.dbL = 0;
f.dbN = -1 * d.hw.bmF;
var v = u.default.GH($eZ.battleMgr.bDu.ng, f, 0);
v.hv.bgM = 1;
v.hv.bgg = 1;
1 === r.enemyFight.bGw && u.default.EL(v, 21001011);
1 === r.enemyCannotGrab.bGw && u.default.Gh(v, 11012, null, v.hg, null);
1 === r.enemyStiff.bGw && u.default.Gh(v, 11011, null, v.hg, null);
if (s) {
this.bye = !1;
$eZ.battleMgr.bDu.ng.gX($eZ.battleMgr.bDu.nz, $eZ.battleMgr.bDu.my.bCQ);
$eZ.battleMgr.bDv.bDo();
$eZ.battleMgr.bDu.ng.gZ();
$eZ.battleMgr.bDu.nk.bBh(h.$uD.$vA.bv);
this.byt();
}
}
}
}
}
}
};
e.prototype.byo = function() {
$eZ.battleMgr.bDu.my.bCQ.bGN() || $eZ.battleMgr.bDu.nd.bCt();
};
e.prototype.byp = function() {
var t, e, i = null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCQ;
if (i && $eZ.battleMgr.bDu.my.bCQ.bGN()) {
$eZ.battleMgr.bDu.ng.hb();
this.bye = !1;
i.modeIndex.bGw = 0;
this.byt();
}
};
e.prototype.byq = function() {
var t, e, i, o, n = null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCQ;
if (n && n.bGN()) {
var r = $eZ.battleMgr.bDu.ng;
if (r) {
var s = null !== (o = null === (i = n.bGI[n.modeTimeLimitIndex.bGw]) || void 0 === i ? void 0 : i[0]) && void 0 !== o ? o : 0;
r.fn = s;
r.fk = 0;
$eZ.battleMgr.bDu.nd.bCt();
$eZ.battleMgr.bDu.ng.ha();
this.bye = !0;
this.byt();
this.byr();
}
}
};
e.prototype.byr = function() {
if (this.node_time.active) {
var t = $eZ.battleMgr.bDu.ng;
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
e.prototype.bys = function() {
var t, e, i = null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCQ;
if (i) {
this.byl();
this.bye = !1;
i.modeIndex.bGw = 0;
this.byt();
}
};
e.prototype.byt = function() {
var t, e, i = null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCQ;
if (i) {
var o = i.bGN();
this.btn_bag.node.active = !o;
this.btn_pet.node.active = !o;
this.btn_skill.node.active = !o;
this.node_time.active = o;
this.btn_clear.node.opacity = o ? 150 : 255;
this.btn_resetStatus.node.opacity = o ? 150 : 255;
this.btn_option.node.opacity = o ? 150 : 255;
this.btn_record.node.opacity = o ? 150 : 255;
$eZ.battleMgr.bDu.nd.bBF = !this.bye;
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
}(l.$fb.Component);
i.default = v;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/battle/$QM": void 0,
"../../../game/src/battle/types/$WY": void 0,
"../../battleLogic/$KC": void 0
} ],
$PT: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "937bdIjLgRN1Y25Z8NwtYvG", "$PT");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
i.$lm = void 0;
var l, c = t("../../../game/src/perfab/battleUI/$CBM"), h = t("../../../bingo/src/$JX"), b = t("../../../game/src/$KK"), u = t("../../../game/src/$KL"), p = t("../../../game/src/manager/$SN"), f = t("../../../game/src/battle/$QA"), d = t("../../../bingo/src/utils/$ML"), v = t("../../../game/src/perfab/battleUI/$CBE"), y = t("./$PV"), g = t("./$PX"), m = t("../render/$PD"), _ = t("./$PW"), $ = t("./$PU"), P = t("./$PO"), w = t("./$PS"), I = t("./$PR"), O = t("./$PL"), E = t("../../../game/src/perfab/battleUI/$CBJ"), x = t("../../../game/src/perfab/battleUI/$CBI"), S = t("../../../game/src/perfab/common/$CBZ"), M = t("../../../game/src/perfab/common/$CCA"), C = cc._decorator, T = C.ccclass, R = C.property;
(function(t) {
t[t.b = 1] = "b";
t[t.c = 2] = "c";
})(l = i.$lm || (i.$lm = {}));
var k = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.byu = [];
e.byv = null;
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
e.byw = null;
e.byx = null;
e.byy = null;
e.byz = null;
e.byA = null;
e.byB = [];
e.byC = null;
e.byD = null;
e.byE = null;
e.byF = null;
e.byG = null;
e.byH = null;
e.byI = null;
e.byJ = cc.color();
e.byK = new Map();
e.byL = l.b;
e.uiSP = null;
e.uiSPDatas = [ null ];
e.bzr = null;
e.bzs = null;
e.bzt = !1;
e.bzw = "";
e.bzA = !1;
e.bzI = null;
e.bzN = null;
return e;
}
e.prototype.ks = function() {
var t, e;
null === (t = cc.MultTextures) || void 0 === t || t.addScope(this.node);
this.byH = this.node.getChildByName("layerArrow");
var i = cc.instantiate(this.joystickPrefab);
this.node.addChild(i, -1, "joysitck");
this.byv = i.getComponent(c.default);
var o = null === (e = $eZ.skill.csT.csO(b.default.JE.ng.gN ? h.$fb.tblEnum.SkillTypeTableEnum.PVP_SKILL : h.$fb.tblEnum.SkillTypeTableEnum.PVE_SKILL)) || void 0 === e ? void 0 : e.dbh;
if (o) {
this.byv.node.scale = o.dbd;
this.byv.node.x = o.dbb;
this.byv.node.y = o.dbc;
} else {
this.byv.node.scale = 1;
this.byv.node.x = -.35 * cc.winSize.width;
this.byv.node.y = -180;
}
b.default.JE.nd.bBT(this.byv);
this.byV();
this.comboNode.opacity = 0;
this.kZ("skillInsert").zIndex = 100;
this.uiSP.node.zIndex = 101;
this.kZ("nodeTips").y = 120;
var n = cc.instantiate(this.optBtnUIPrefab);
this.node.addChild(n);
this.byw = n.getComponent(v.default);
this.byw.node.x = cc.winSize.width / 2 - $eZ.fguiMgr.cSy;
this.byw.node.y = -cc.winSize.height / 2;
this.kP(this.btnPause, this.byM, this);
this.kP(this.nodePvpTop.getChildByName("btnPause"), this.byM, this);
this.kP(this.kZ("chatBtn"), this.bzD, this);
this.kP(this.btnAutoBattle, this.bzQ, this);
$eZ.eventMgr.kn(b.default.JC.cGm, this.byV, this);
this.abyssInfoUI.node.active = !1;
};
e.prototype.ku = function() {
var t, e, i, o;
this.lbl_uid.string = "" + (null !== (o = null === (i = null === (e = null === (t = b.default.JE) || void 0 === t ? void 0 : t.my) || void 0 === e ? void 0 : e.bCC) || void 0 === i ? void 0 : i.instUid) && void 0 !== o ? o : "");
this.lbl_uid.node.zIndex = 2e3;
};
e.prototype.ky = function() {
var t;
this.byK.clear();
if (this.bzN) {
M.default.fKP(this.bzN, this.bzN.node);
this.bzN = null;
}
$eZ.eventMgr.cKZ(this);
cc.systemEvent.targetOff(this);
null === (t = cc.MultTextures) || void 0 === t || t.removeScope(this.node);
};
e.prototype.byM = function() {
b.default.JE.nm || $eZ.fguiMgr.cRP(b.default.JB.BattleSettingPanel);
};
e.prototype.byN = function() {
if (!this.byy) {
var t = cc.instantiate(this.panelEnemyInfoPrefab);
this.node.addChild(t);
t.x = -200;
t.y = cc.winSize.height / 2 - 30;
this.byy = t.getComponent(y.default);
}
if (!this.byz) {
t = cc.instantiate(this.panelSelfPlayerInfoPrefab);
this.node.addChild(t);
t.x = 0;
t.y = -cc.winSize.height / 2 + 50;
this.byz = t.getComponent(g.default);
}
};
e.prototype.byO = function() {
if (!(this.byB.length > 0)) {
var t = this.node.getChildByName("panelHero1");
t.active = !0;
var e = cc.instantiate(t);
this.node.addChild(e);
e.x = -t.x;
e.y = t.y;
e.scaleX = -1;
this.byB = [ t.getComponent(_.default), e.getComponent(_.default) ];
this.byB.forEach(function(t, e) {
t.bAp(e);
});
}
};
e.prototype.byP = function() {
if (!this.byA) {
var t = cc.instantiate(this.battleGroupStatusPanelPrefab);
this.node.addChild(t);
this.byA = t.getComponent(P.default);
}
};
e.prototype.byQ = function(t) {
this.byL = t;
var e = this.kZ("nodeCenterTop");
if (e) {
e.active = !0;
e.getChildByName("pause").active = t === l.b;
e.getChildByName("skip").active = t === l.c;
}
};
e.prototype.byR = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF18(n){switch (n.label) {
case 0:
return (t = this.node.getChildByName("__GAMEDEBUGUI__")) ? [ 3, 2 ] : [ 4, this.lK(new h.$fb.ResCfg("editor:prefabs/EditorMainUI", cc.Prefab)) ];

case 1:
if ((e = n.sent()) && e.Da) {
i = e.Da;
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
e.prototype.byS = function() {
return s(this, void 0, void 0, function() {
var t, e, i, o;
return a(this,function AWAITF19(n){switch (n.label) {
case 0:
return (t = this.node.getChildByName("__GAMEGMUI__")) ? [ 3, 2 ] : [ 4, this.lK(new h.$fb.ResCfg("game:res/panel/battle/BattleGMUI", cc.Prefab)) ];

case 1:
e = n.sent();
i = e.Da;
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
e.prototype.byT = function() {
this.byK.clear();
};
e.prototype.byU = function(t, e) {
cc.systemEvent.emit(t, e);
};
e.prototype.byV = function() {
this.isValid && this.byv && this.byv.fGo($eZ.localMgr.cmD(p.$oI.f) ? c.$Pz.b : c.$Pz.c);
};
e.prototype.byW = function(t, e) {
return s(this, void 0, void 0, function() {
var i, o, n, r;
return a(this,function AWAITF20(){t > 1 && (t = 1);
if (!(i = this.byu[t])) return [ 2 ];
(o = i.getChildByName("networkStatus")).active = !0;
n = cc.Color.RED;
e < 96 ? n = cc.Color.GREEN : e < 320 && (n = cc.Color.ORANGE);
(r = o.getChildByName("lbl").getComponent(cc.Label)).string = Math.min(e, 999) + "ms";
r.node.color = n;
return [ 2 ];
});
});
};
e.prototype.byX = function(t, e) {
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
e.prototype.byY = function(t, e, i) {
var o = this;
void 0 === e && (e = "animation");
void 0 === i && (i = !0);
if (t) {
this.uiSP.skeletonData = t;
this.uiSP.node.active = !0;
this.uiSP.premultipliedAlpha = !1;
var n = this.uiSP.setAnimation(0, e || "animation", !1);
this.unschedule(this.byZ);
i ? this.uiSP.setTrackCompleteListener(n, function() {
o.uiSP.node.active = !1;
}) : this.scheduleOnce(this.byZ, 1);
} else console.warn("no skeletonData");
};
e.prototype.byZ = function() {
this.uiSP.node.active = !1;
};
e.prototype.bza = function() {
this.uiSP.timeScale = 0;
};
e.prototype.bzb = function() {
this.uiSP.timeScale = 1;
};
e.prototype.bzc = function(t) {
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
e.prototype.bzd = function(t) {
if (this.isValid && this.byH) {
for (var e = 0; e < t.length; e++) {
var i = this.byH.children[e], o = t[e];
if (!i) {
i = cc.instantiate(this.byH.children[0]);
this.byH.addChild(i, 99);
}
i.active = !0;
i.setPosition(o.b);
i.angle = 180 * o.c / Math.PI + 90;
i.getComponent(cc.Sprite).spriteFrame = this.tipArrowArray[o.d];
}
for (;e < this.byH.children.length; e++) this.byH.children[e].active = !1;
}
};
e.prototype.bze = function() {};
e.prototype.bzf = function() {};
e.prototype.bzg = function() {};
e.prototype.bzh = function() {};
e.prototype.bzi = function(t, e) {
if (t._battleUI_sprite !== e) {
t._battleUI_sprite = e;
this.lB(new h.$fb.ResCfg(e, cc.SpriteFrame), function(i, o) {
if (!i) {
var n = o.Da;
t.isValid && t._battleUI_sprite === e && (t.spriteFrame = n);
}
});
}
};
e.prototype.bzj = function(t, e, i, o) {
var n, r;
void 0 === o && (o = 100);
var s = this.byK.get(t);
if (!s) {
var a = b.default.JE.nb.bFN(e);
if (!a) return;
this.node.addChild(a, o);
s = {
id: t,
node: a
};
this.byK.set(t, s);
}
var l = this.node;
if (i.left) {
var c = i.left;
Math.abs(c) < 1 && (c = l.width * c);
s.node.x = -l.width / 2 + c;
}
if (i.right) {
c = i.right;
Math.abs(c) < 1 && (c = l.width * c);
s.node.x = l.width / 2 - c;
}
if (i.top) {
c = i.top;
Math.abs(c) < 1 && (c = l.height * c);
s.node.y = l.height / 2 - c;
}
if (i.bottom) {
c = i.bottom;
Math.abs(c) < 1 && (c = l.height * c);
s.node.y = -l.height / 2 + c;
}
i.offsetX && (s.node.x += i.offsetX);
i.offsetY && (s.node.y += i.offsetY);
if ("number" == typeof i.btn) {
var h = null === (n = this.byw) || void 0 === n ? void 0 : n.fDS[i.btn];
if (null === (r = null == h ? void 0 : h.frame) || void 0 === r ? void 0 : r.node) {
var u = h.frame.node.convertToWorldSpaceAR(cc.Vec2.ZERO_R), p = l.convertToNodeSpaceAR(u);
s.node.setPosition(p);
}
}
};
e.prototype.bzk = function(t, e, i) {
var o = this.byK.get(t);
o && o.node.emit(e, i);
};
e.prototype.bzl = function(t) {
var e = this.byK.get(t);
if (e) {
this.byK.delete(t);
e.node.destroy();
}
};
e.prototype.bzm = function(t, e, i, o, n, r, s) {
var a = this.kZ("skillInsert");
if (a) {
cc.Tween.stopAllByTarget(a);
a.active = !0;
cc.tween(a).delay(t).call(function() {
a.isValid && (a.active = !1);
}).start();
var l = a.children[0];
l.getComponent(cc.Sprite).spriteFrame = b.default.JE.nb.bFB(e);
l.x = i || 0;
l.y = o || 0;
l.angle = n || 0;
l.scaleX = r || 1;
l.scaleY = s || 1;
}
};
e.prototype.bzn = function(t) {
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
s.color = cc.Color.fromHEX(this.byJ, t.color);
s.setAnchorPoint(.5, .5);
s.opacity = 0;
var l = null !== (i = t.param[0]) && void 0 !== i ? i : .1, c = (a = null !== (o = t.param[1]) && void 0 !== o ? o : .1, 
null !== (n = t.param[2]) && void 0 !== n ? n : .1), h = null !== (r = t.param[3]) && void 0 !== r ? r : 255;
cc.tween(s).to(l, {
opacity: h
}).delay(a).to(c, {
opacity: 0
}).start();
s.zIndex = -1;
}
}
};
e.prototype.bzo = function(t, e, i, o) {
void 0 === o && (o = 0);
var n = this.byu[t];
n.active = !0;
n.getChildByName("lblHeroName").getComponent(cc.Label).string = "" + i;
e && this.bzi(n.getChildByName("avatar").getComponent(cc.Sprite), e);
n.getChildByName("lbl_uid").getComponent(cc.Label).string = "" + (o > 0 ? o : "");
};
e.prototype.bzp = function(t, e, i, o) {
void 0 === o && (o = !0);
var n = this.byu[t], r = e / i, s = n.getChildByName("progHP").getComponent(cc.ProgressBar);
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
e.prototype.bzq = function(t, e, i, o) {
var n = this.byu[t], r = e / i, s = n.getChildByName("progMP").getComponent(cc.ProgressBar);
n.getChildByName("lblMP").getComponent(cc.Label).string = e + "/" + i;
d.default.bex(s.barSprite, !o);
s.progress = r;
};
e.prototype.bzu = function() {};
e.prototype.bzv = function() {};
e.prototype.bzx = function(t, e, i) {
var o = this, n = this.kZ("nodeTips"), r = n.getChildByName("txt1").getComponent(cc.RichText);
r.string = e;
if (n) {
this.bzw = t;
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
o.bzw = "";
}).hide().start();
}
};
e.prototype.bzy = function(t, e) {
var i = this.kZ("nodeTips");
i.getChildByName("txt1").getComponent(cc.RichText).string = e;
this.bzw = t;
if (i) {
i.opacity = 255;
cc.Tween.stopAllByTarget(i);
}
};
e.prototype.bzz = function() {
var t = this.kZ("nodeTips");
if (t) {
t.opacity = 0;
cc.Tween.stopAllByTarget(t);
this.bzw = "";
}
};
e.prototype.bzB = function(t) {
if (this.bzA !== t) {
this.bzA = t;
this.battleTime.node.color = this.bzA ? f.BATTLE_COLOR_R.RED : f.BATTLE_COLOR_R.YELLOW2;
cc.Tween.stopAllByTarget(this.battleTime.node);
this.battleTime.node.opacity = 255;
this.bzA && cc.tween(this.battleTime.node).set({
opacity: 0
}).delay(.1).set({
opacity: 255
}).delay(.25).union().repeatForever().start();
}
};
e.prototype.bzC = function() {};
e.prototype.bzD = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF21(){this.node.getChildByName("BattleChatPanel");
return [ 2 ];
});
});
};
e.prototype.bzE = function(t, e) {
1 == e.chatType ? this.bzF(t, e.chatData) : this.bzG(t, e.chatData);
};
e.prototype.bzF = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF22(){return [ 2 ];
});
});
};
e.prototype.bzG = function() {};
e.prototype.bzH = function() {};
e.prototype.bzJ = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF23(o){switch (o.label) {
case 0:
return this.bzI ? [ 3, 2 ] : [ 4, this.lK(new h.$fb.ResCfg("battleRes:prefabs/battle/BattlePlaybackUI", cc.Prefab)) ];

case 1:
t = o.sent();
e = t.Da;
if (this.isValid) {
if (this.bzI) return [ 2 ];
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
e.prototype.bzK = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF24(o){switch (o.label) {
case 0:
return this.byx ? [ 3, 2 ] : [ 4, this.lK(new h.$fb.ResCfg("game:res/battleRes/prefabs/battle/BattleScoringCompetitionUI", cc.Prefab)) ];

case 1:
t = o.sent();
e = t.Da;
if (this.isValid) {
if (this.byx) return [ 2 ];
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
e.prototype.bzL = function(t) {
this.btnAutoBattle.children[0].active = !t;
this.btnAutoBattle.children[1].active = t;
this.btnAutoBattle.children[2].getComponent(cc.Label).string = t ? "取消自动" : "自动战斗";
this.node_autoTxt.active = t;
};
e.prototype.bzM = function() {
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
e.prototype.bzO = function() {
if (this.btnAutoBattle.active && !b.default.JE.nm && $eZ.monCard.fxa.fwW() && $eZ.funcOpen.cfQ(h.$fb.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN, !1) && !$eZ.localMgr.cma()) {
if (!this.bzN) {
var t = $eZ.prefabMgr.cPB(u.default.commonComp.movieClip2);
if (!t) return;
t.setPosition(0, 0);
this.btnAutoBattle.addChild(t, 100);
this.bzN = t.getComponent(S.default);
}
$eZ.localMgr.cmb(!0);
this.bzN.node.active = !0;
this.bzN.play(h.$fb.ResUrl.MC_3021, !0);
}
};
e.prototype.bzP = function() {
this.bzN && M.default.fKO(this.bzN);
};
e.prototype.bzQ = function() {
var t, e, i;
if (b.default.JE.nn()) {
var o = b.default.JE.ny();
if (o && !b.default.JE.nm) if ((null === (t = b.default.JE.my.bCX) || void 0 === t ? void 0 : t.d) || $eZ.monCard.fxa.fwW()) {
this.bzP();
$eZ.battleMgr.bDE = !o.dcv;
b.default.JE.nd.bCq(!o.dcv);
h.$fb.tracking.cJa($eZ.battleMgr.bDE ? 1 : 0, (null === (i = b.default.JE.my) || void 0 === i ? void 0 : i.bDq()) || "");
} else {
if (!$eZ.funcOpen.cfQ(h.$fb.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN, !1)) {
var n = $eZ.cfgMgr.tbl.FunctionCondTable.findFirst(function(t) {
return t.funcId == h.$fb.tblEnum.FunctionUnlockTableEnum.MONTH_CARD_OPEN;
});
if (n && (null === (e = n.taskIds) || void 0 === e ? void 0 : e.length) > 0) {
var r = $eZ.cfgMgr.tbl.TaskTable.findByKey(n.taskIds[0]);
$eZ.fguiMgr.cRI($eZ.i18n.LV(null == r ? void 0 : r.desc));
}
return;
}
$eZ.fguiMgr.cRI("激活特权后解锁");
}
}
};
e.prototype.bzR = function(t) {
this.byI && (this.byI.active = t);
};
e.prototype.bzS = function(t) {
t && !(t.length <= 0) && this.byI;
};
e.prototype.bzT = function(t) {
this.byw && (this.byw.node.active = t);
};
e.prototype.bzU = function() {
var t, e = null === (t = b.default.JE) || void 0 === t ? void 0 : t.ng;
if (e) {
var i = e.fI;
if (i) {
var o = "";
if (e.fK.type === h.$fb.tblEnum.InstanceTypeTableEnum.TOWER) {
var n = $eZ.destroyTower.ceM.ceC(e.fK.id);
(null == n ? void 0 : n.floor) && (o = $eZ.i18n.LV(e.fK.name) + " " + n.floor + "层");
} else o = $eZ.i18n.LV(e.fK.name) || "";
this.minmapNode.parent.getChildByName("lblTitle").getComponent(cc.Label).string = o;
for (var r = [ 3, 3 ], s = [ 1, 1 ], a = this.minmapNode.getChildByName("content"), l = a.children[0], c = l.width + 2, u = [], p = 1; p <= 4; p++) i.dcK[p] && u.push(i.ddg(p));
var f = -1, d = i.ddb ? i.ddg(i.ddb) : null, v = d ? e.gJ(d.x, d.y) : null;
for (p = 0; p < r[0]; p++) for (var y = function(t) {
var o = p - s[0], n = t - s[1], r = o + i.dcH, h = n + i.dcI;
f++;
var b = a.children[f];
b || ((b = cc.instantiate(l)).parent = a);
var d = e.gJ(r, h);
b.active = !!d;
if (d) {
var y = d === i;
b.active && (b.zIndex = y ? 1 : 0);
b.x = o * c;
b.y = n * c;
var g = b.getComponent(m.$kF), _ = -1 !== u.findIndex(function(t) {
return t.x == r && t.y == h;
}), $ = v === d;
g.bvq(d, y, i.dcO, _, $);
}
}, g = 0; g < r[1]; g++) y(g);
}
}
};
e.prototype.bzV = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF25(o){switch (o.label) {
case 0:
return this.byC ? [ 2 ] : [ 4, this.lK(new h.$fb.ResCfg("battleRes:prefabs/battle/ui/BattleWorldBossPanel", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.byC) return [ 2 ];
(i = cc.instantiate(e)).x = .5 * this.node.width - 60;
i.y = .5 * this.node.height - 20;
this.node.addChild(i, 2);
this.byC = i.getComponent($.default);
return [ 2 ];
}
});
});
};
e.prototype.bzW = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF26(o){switch (o.label) {
case 0:
return this.byD ? [ 2 ] : [ 4, this.lK(new h.$fb.ResCfg("battleRes:prefabs/battle/ui/BattleTrainingUI", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.byD) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 2);
this.byD = i.getComponent(w.default);
return [ 2 ];
}
});
});
};
e.prototype.bzX = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF27(o){switch (o.label) {
case 0:
return this.byE ? [ 2 ] : [ 4, this.lK(new h.$fb.ResCfg("battleRes:prefabs/battle/ui/BattlePvpTrainingUI", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.byE) return [ 2 ];
i = cc.instantiate(e);
this.node.addChild(i, 2);
this.byE = i.getComponent(I.default);
return [ 2 ];
}
});
});
};
e.prototype.bzY = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF28(o){switch (o.label) {
case 0:
return this.byF ? [ 2 ] : [ 4, this.lK(new h.$fb.ResCfg("game:res/prefab/battle/BattleSkillGuidePanel", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.byF) return [ 2 ];
(i = cc.instantiate(e)).y = .5 * this.node.height - 150;
this.node.addChild(i, 2);
this.byF = i.getComponent(E.default);
return [ 2 ];
}
});
});
};
e.prototype.bzZ = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF29(o){switch (o.label) {
case 0:
return this.byG ? [ 2 ] : this.byw && this.byw.node ? [ 4, this.lK(new h.$fb.ResCfg("game:res/prefab/battle/guide/BattleSkillClickTip", cc.Prefab)) ] : [ 2 ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
if (!this.isValid) return [ 2 ];
if (this.byG) return [ 2 ];
if (!this.byw || !this.byw.node) return [ 2 ];
i = cc.instantiate(e);
this.byw.node.addChild(i, 1e3);
this.byG = i.getComponent(x.default);
return [ 2 ];
}
});
});
};
e.prototype.bAa = function() {
return s(this, void 0, void 0, function() {
var t, e, i;
return a(this,function AWAITF30(o){switch (o.label) {
case 0:
return [ 4, this.lK(new h.$fb.ResCfg("battleRes:prefabs/battle/other/AbyssEnterText", cc.Prefab)) ];

case 1:
t = o.sent();
if (!(e = t.Da)) return [ 2 ];
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
r([ R(cc.Prefab) ], e.prototype, "joystickPrefab", void 0);
r([ R(cc.Prefab) ], e.prototype, "optBtnUIPrefab", void 0);
r([ R(cc.Prefab) ], e.prototype, "panelEnemyInfoPrefab", void 0);
r([ R(cc.Prefab) ], e.prototype, "panelSelfPlayerInfoPrefab", void 0);
r([ R(cc.Prefab) ], e.prototype, "battleGroupStatusPanelPrefab", void 0);
r([ R(cc.Node) ], e.prototype, "selfHero", void 0);
r([ R(cc.Node) ], e.prototype, "enemyHero", void 0);
r([ R(cc.Label) ], e.prototype, "battleTime", void 0);
r([ R(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ R(cc.Node) ], e.prototype, "btnPause", void 0);
r([ R(cc.Node) ], e.prototype, "btnExpress", void 0);
r([ R(cc.Node) ], e.prototype, "baoXiang", void 0);
r([ R(cc.Node) ], e.prototype, "panelSkillConf", void 0);
r([ R(cc.Node) ], e.prototype, "emoticonFrame", void 0);
r([ R(cc.Label) ], e.prototype, "comboCount", void 0);
r([ R(cc.Node) ], e.prototype, "comboNode", void 0);
r([ R(cc.Node) ], e.prototype, "nodeDefeatLayer", void 0);
r([ R(cc.SpriteFrame) ], e.prototype, "tipArrowArray", void 0);
r([ R(cc.Node) ], e.prototype, "btnAutoBattle", void 0);
r([ R(cc.Node) ], e.prototype, "node_autoTxt", void 0);
r([ R(cc.Label) ], e.prototype, "lbl_uid", void 0);
r([ R(cc.Node) ], e.prototype, "minmapNode", void 0);
r([ R(cc.Node) ], e.prototype, "nodeLeftTop", void 0);
r([ R(cc.Node) ], e.prototype, "nodePvpTop", void 0);
r([ R(cc.Node) ], e.prototype, "nodeTime", void 0);
r([ R(cc.Node) ], e.prototype, "nodePractice", void 0);
r([ R(O.default) ], e.prototype, "abyssInfoUI", void 0);
r([ R(sp.Skeleton) ], e.prototype, "uiSP", void 0);
r([ R(sp.SkeletonData) ], e.prototype, "uiSPDatas", void 0);
return r([ T ], e);
}(h.$fb.Component);
i.default = k;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../bingo/src/utils/$ML": void 0,
"../../../game/src/$KK": void 0,
"../../../game/src/$KL": void 0,
"../../../game/src/battle/$QA": void 0,
"../../../game/src/manager/$SN": void 0,
"../../../game/src/perfab/battleUI/$CBE": void 0,
"../../../game/src/perfab/battleUI/$CBI": void 0,
"../../../game/src/perfab/battleUI/$CBJ": void 0,
"../../../game/src/perfab/battleUI/$CBM": void 0,
"../../../game/src/perfab/common/$CBZ": void 0,
"../../../game/src/perfab/common/$CCA": void 0,
"../render/$PD": "$PD",
"./$PL": "$PL",
"./$PO": "$PO",
"./$PR": "$PR",
"./$PS": "$PS",
"./$PU": "$PU",
"./$PV": "$PV",
"./$PW": "$PW",
"./$PX": "$PX"
} ],
$PU: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "3b718OKVV9DTprYt5MM3wzH", "$PU");
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
l(o.next(t));
} catch (t) {
r(t);
}
}
function a(t) {
try {
l(o.throw(t));
} catch (t) {
r(t);
}
}
function l(t) {
t.done ? n(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(s, a);
var e;
}
l((o = o.apply(t, e || [])).next());
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
return l([ t, e ]);
};
}
function l(r) {
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
var l = t("../../../bingo/src/$JX"), c = t("../../../game/src/battle/$QM"), h = cc._decorator, b = h.ccclass, u = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblRecord = null;
e.lblName = null;
e.lblState = null;
e.lblLeftTime = null;
e.imgBox = null;
e.progressSprite = null;
e.bAb = 0;
return e;
}
e.prototype.ku = function() {
this.lblName.string = "";
this.bAb = 0;
};
e.prototype.kH = function() {
var t, e, i, o, n = c.default.bGR.bIx(), r = (null === (e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.ng) || void 0 === e ? void 0 : e.gS()) || 0, s = c.default.bGR.bIy(r), a = s === n[n.length - 1].id, l = this.bAb;
this.bAb = s;
s > l && this.bAd();
this.lblState.string = "阶段" + s;
if (a) this.bAc(1); else {
var h = (null === (i = n[s - 1]) || void 0 === i ? void 0 : i.damage) || 0, b = (r - h) / (((null === (o = n[s]) || void 0 === o ? void 0 : o.damage) || 0) - h);
this.bAc(b > 1e-4 ? b : 0);
}
this.lblRecord.string = "" + r;
this.bAf();
};
e.prototype.bAc = function(t) {
this.progressSprite.fillRange = t;
};
e.prototype.bAd = function() {
this.bAe();
};
e.prototype.bAe = function() {
return s(this, void 0, void 0, function() {
return a(this,function AWAITF31(){return [ 2 ];
});
});
};
e.prototype.bAf = function() {
var t, e = null === (t = $eZ.battleMgr.bDu) || void 0 === t ? void 0 : t.ng;
if (e && e.fn) {
var i, o = Math.floor(Math.max(0, e.fn - e.fk)), n = o % 60, r = Math.floor(o / 60);
i = (r < 10 ? "0" + r : r) + ":" + (n < 10 ? "0" + n : n);
this.lblLeftTime.string = i;
}
};
r([ u(cc.Label) ], e.prototype, "lblRecord", void 0);
r([ u(cc.Label) ], e.prototype, "lblName", void 0);
r([ u(cc.Label) ], e.prototype, "lblState", void 0);
r([ u(cc.Label) ], e.prototype, "lblLeftTime", void 0);
r([ u(cc.Sprite) ], e.prototype, "imgBox", void 0);
r([ u(cc.Sprite) ], e.prototype, "progressSprite", void 0);
return r([ b ], e);
}(l.$fb.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QM": void 0
} ],
$PV: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "28635SOoEhGspua5e5dvyfr", "$PV");
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
var s = t("../../../bingo/src/$JX"), a = t("../../../game/src/battle/$QM"), l = t("../render/$OY"), c = t("./$PM"), h = cc._decorator, b = h.ccclass, u = h.property, p = function(t) {
n(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.lblName = null;
e.lblLevel = null;
e.spAvatar = null;
e.hpBar = null;
e.buffList = null;
e.buffItem = null;
e.bAg = null;
e.bAh = 0;
e.bAi = -1;
e.bAj = new c.$kW();
return e;
}
e.prototype.ku = function() {
this.bAg = this.hpBar.getComponent(l.default);
this.bAk();
};
e.prototype.bAk = function() {
this.bAj.bxg(this.buffList ? this.buffList.node : null, this.buffItem);
};
e.prototype.bAl = function(t) {
this.bAj.bxi(t);
};
e.prototype.bAm = function(t, e) {
if (this.bAi !== e) {
this.bAi = e;
this.lblLevel.string = "Lv." + e;
}
if (this.bAh !== t) {
this.bAh = t;
var i = a.default.bGR.bHl(t);
if (i) {
this.lblName.string = $eZ.i18n.LV(i.nameText);
this.lE(this.spAvatar, i.avatar);
}
}
};
e.prototype.bAn = function() {
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
r([ u(cc.Label) ], e.prototype, "lblName", void 0);
r([ u(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ u(cc.Sprite) ], e.prototype, "spAvatar", void 0);
r([ u(cc.Node) ], e.prototype, "hpBar", void 0);
r([ u(cc.Layout) ], e.prototype, "buffList", void 0);
r([ u(cc.Node) ], e.prototype, "buffItem", void 0);
return r([ b ], e);
}(s.$fb.Component);
i.default = p;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QM": void 0,
"../render/$OY": "$OY",
"./$PM": "$PM"
} ],
$PW: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "7277aKfXLdKUbaG25i1igsM", "$PW");
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
var s = t("../../../bingo/src/$JX"), a = t("../../../game/src/battle/$QM"), l = cc._decorator, c = l.ccclass, h = l.property, b = function(t) {
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
e.bAo = 0;
e.bAr = -1;
e.bAs = -1;
e.bAt = -1;
e.bAu = -1;
e.bAz = 263;
return e;
}
e.prototype.ks = function() {
this.bAz = this.HPSprite.node.width;
};
e.prototype.ku = function() {};
e.prototype.kx = function() {};
e.prototype.kH = function() {};
e.prototype.bAp = function(t) {
this.bAo = t;
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
e.prototype.bAq = function(t, e, i, o) {
var n = a.default.bGR.bHl(e);
this.lE(this.avatar, n.avatar);
this.nick.string = i;
this.uid.string = "" + (o > 0 ? o : "");
};
e.prototype.bAv = function(t, e) {
if (this.bAr !== t || this.bAs !== e) {
this.bAr = t;
this.bAs = e;
this.lblHP.string = t + "/" + e;
var i = 0 !== e ? t / e : 0;
this.bAx(i);
}
};
e.prototype.bAw = function(t, e) {
if (this.bAt !== t || this.bAu !== e) {
this.bAt = t;
this.bAu = e;
var i = 0 !== e ? t / e : 0;
this.bAy(i);
}
};
e.prototype.bAx = function(t) {
this.HPSprite.fillRange = t;
};
e.prototype.bAy = function(t) {
this.MPSprite.fillRange = t;
};
e.prototype.bAA = function(t, e, i) {
var o = 0;
if (t > 0) {
o = Math.max(t, 0) * this.bAz;
this.backHurtProgressArrow.node.x = o;
this.backHurtProgressArrow.node.opacity = 255;
} else this.backHurtProgressArrow.node.opacity = 0;
if (e > 0) {
o = Math.max(e, 0) * this.bAz;
this.flyHurtProgressArrow.node.x = o;
this.flyHurtProgressArrow.node.opacity = 255;
} else this.flyHurtProgressArrow.node.opacity = 0;
if (i > 0) {
o = Math.max(i, 0) * this.bAz;
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
return r([ c ], e);
}(s.$fb.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"../../../game/src/battle/$QM": void 0
} ],
$PX: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "821c3cK+ANF7ZnGaT3Y8idh", "$PX");
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
var s = t("../../../bingo/src/$JX"), a = t("./$PM"), l = cc._decorator, c = l.ccclass, h = l.property, b = function(t) {
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
e.bAB = new a.$kW();
e.bAC = -1;
e.bAD = -1;
e.bAE = -1;
e.bAF = -1;
e.bAG = -1;
return e;
}
e.prototype.ks = function() {
this.buffItem || (this.buffItem = this.node.getChildByName("buffNode"));
this.bAB.bxg(this.buffList ? this.buffList.node : null, this.buffItem);
};
Object.defineProperty(e.prototype, "bAH", {
get: function() {
this.buffItem || (this.buffItem = this.node.getChildByName("buffNode"));
return this.buffItem;
},
enumerable: !1,
configurable: !0
});
e.prototype.bAI = function(t) {
if (this.bAG !== t) {
this.bAG = t;
this.lblLevel.string = t.toString();
}
};
e.prototype.bAJ = function(t, e) {
if (this.bAC !== t || this.bAD !== e) {
this.bAC = t;
this.bAD = e;
this.lblHP.string = t + "/" + e;
var i = 0 !== e ? t / e : 0;
this.bAL(i);
}
};
e.prototype.bAK = function(t, e) {
if (this.bAE !== t || this.bAF !== e) {
this.bAE = t;
this.bAF = e;
var i = 0 !== e ? t / e : 0;
this.bAM(i);
}
};
e.prototype.bAL = function(t) {
this.HPSprite.fillRange = t;
};
e.prototype.bAM = function(t) {
this.MPSprite.fillRange = t;
};
e.prototype.bAN = function(t) {
this.bAB.bxi(t);
};
r([ h(cc.Label) ], e.prototype, "lblLevel", void 0);
r([ h(cc.Label) ], e.prototype, "lblHP", void 0);
r([ h(cc.Label) ], e.prototype, "lblMP", void 0);
r([ h(cc.Sprite) ], e.prototype, "HPSprite", void 0);
r([ h(cc.Sprite) ], e.prototype, "MPSprite", void 0);
r([ h(cc.Layout) ], e.prototype, "buffList", void 0);
r([ h(cc.Node) ], e.prototype, "buffItem", void 0);
return r([ c ], e);
}(s.$fb.Component);
i.default = b;
cc._RF.pop();
}, {
"../../../bingo/src/$JX": void 0,
"./$PM": "$PM"
} ]
}, {}, [ "$KH", "$KI", "$OI", "$OJ", "$OK", "$OL", "$OM", "$ON", "$OO", "$OP", "$OQ", "$OR", "$OS", "$OT", "$OU", "$OV", "$OW", "$OX", "$OY", "$OZ", "$PA", "$PB", "$PC", "$PD", "$PE", "$PF", "$PG", "$PH", "$PI", "$PJ", "$PK", "$PL", "$PM", "$PN", "$PO", "$PP", "$PQ", "$PR", "$PS", "$PT", "$PU", "$PV", "$PW", "$PX" ]);