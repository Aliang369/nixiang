window.__require = function t(e, n, i) {
function o(c, s) {
if (!n[c]) {
if (!e[c]) {
var a = c.split("/");
a = a[a.length - 1];
if (!e[a]) {
var h = "function" == typeof __require && __require;
if (!s && h) return h(a, !0);
if (r) return r(a, !0);
throw new Error("Cannot find module '" + c + "'");
}
c = a;
}
var u = n[c] = {
exports: {}
};
e[c][0].call(u.exports, function(t) {
return o(e[c][1][t] || t);
}, u, u.exports, t, e, n, i);
}
return n[c].exports;
}
for (var r = "function" == typeof __require && __require, c = 0; c < i.length; c++) o(i[c]);
return o;
}({
$KN: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "20d7ddFmJpCiqkEOCPcCXOz", "$KN");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$gC = n.$gB = void 0;
var c, s, a = cc._decorator, h = a.ccclass, u = a.property;
(function(t) {
t[t.b = 0] = "b";
t[t.c = 1] = "c";
t[t.d = 2] = "d";
t[t.e = 3] = "e";
})(c = n.$gB || (n.$gB = {}));
(function(t) {
t.b = "MGXXLBlock_CLICK";
})(s = n.$gC || (n.$gC = {}));
var f = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.pattern = null;
e.mask = null;
e.patterns = [];
e.JU = null;
e.JV = !1;
e.JW = c.b;
e.JX = 0;
e.Kc = !1;
e.Kd = 0;
e.Ke = cc.v2();
e.Kf = cc.v2();
e.Kg = 0;
e.Kh = 0;
e.Ki = 0;
e.Kj = 0;
e.Kk = 250;
e.Kl = 250;
e.Km = -10;
e.Kn = 70;
e.Ko = 3;
e.Kp = 0;
e.Kq = 20;
return e;
}
e.prototype.onLoad = function() {
this.node.on(cc.Node.EventType.TOUCH_END, this.JY, this);
};
e.prototype.onDestroy = function() {
this.node.off(cc.Node.EventType.TOUCH_END, this.JY, this);
this.unschedule(this.Ku);
cc.Tween.stopAllByTarget(this.node);
};
e.prototype.JY = function() {
var t = new cc.Event.EventCustom(s.b, !0);
t.setUserData(this);
this.node.dispatchEvent(t);
};
e.prototype.JZ = function(t) {
cc.Tween.stopAllByTarget(this.node);
this.unschedule(this.Ku);
this.Kc = !1;
this.JW = c.b;
this.JU = t;
this.node.opacity = 255;
this.node.angle = 0;
this.JV = !1;
this.mask && (this.mask.active = !1);
this.pattern && (this.pattern.node.active = !0);
this.Ka();
};
e.prototype.Ka = function() {
if (this.JU && this.pattern && (this.JX !== this.JU.type || !this.pattern.spriteFrame)) {
var t = this.patterns[this.JU.type - 1];
t || cc.warn("[MGXXL] 方块图案缺失 type=" + this.JU.type);
this.pattern.spriteFrame = t || null;
this.JX = this.JU.type;
}
};
e.prototype.Kb = function(t, e) {
void 0 === e && (e = !0);
this.JV = t;
this.mask && (this.mask.active = t);
this.pattern && (this.pattern.node.active = e);
};
e.prototype.Kr = function(t, e, n) {
var i = this;
if (!this.Kc) {
this.Kd = 0;
this.Kc = !0;
this.Ke.x = n.x;
this.Ke.y = n.y;
var o = cc.v2(.5 * cc.winSize.width, .61 * cc.winSize.height);
this.Kf = e.convertToNodeSpaceAR(o);
this.Ks();
this.schedule(this.Ku, .01);
cc.tween(this.node).delay(.5).call(function() {
if (i.isValid && i.JU) {
i.JU.type = t;
i.Ka();
}
}).start();
}
};
e.prototype.Ks = function() {
var t = this.Kv(this.Kf, this.Ke), e = (90, -90 + Math.floor(181 * Math.random()));
this.Kg = t * Math.PI / 180;
this.Kh = (t + e) * Math.PI / 180;
this.Ki = (this.Kh - this.Kg) / this.Kq;
this.Kj = Math.sqrt(this.Kf.sub(this.Ke).mag());
this.Kk = 250;
this.Kl = Math.ceil(100 * Math.random()) + 70;
this.Km = -(this.Kj - this.Kl) / this.Kq;
this.Ko = (this.Kk - this.Kl) / this.Kn;
var n = (720 - e - 30) / this.Kn;
this.Kp = n * Math.PI / 180;
};
e.prototype.Kt = function() {
var t = this;
cc.tween(this.node).to(.1, {
x: this.Ke.x,
y: this.Ke.y
}).call(function() {
t.Kc = !1;
}).start();
};
e.prototype.Ku = function() {
this.Kd += 1;
if (this.Kd <= this.Kq) {
this.Kj += this.Km;
this.Kg += this.Ki;
} else {
if (this.Kd > 2 * this.Kn + this.Kq) {
this.unschedule(this.Ku);
this.Kt();
return;
}
this.Kd < this.Kn + this.Kq ? this.Kj < this.Kk && (this.Kj += this.Ko) : this.Kj = Math.max(60, this.Kj - 3 * this.Ko);
this.Kg += this.Kp;
}
this.node.x = this.Kj * Math.cos(this.Kg) + this.Kf.x;
this.node.y = this.Kj * Math.sin(this.Kg) + this.Kf.y;
};
e.prototype.Kv = function(t, e) {
var n = e.x - t.x, i = e.y - t.y, o = 0;
if (0 !== n) {
var r = Math.abs(i / n);
o = n > 0 ? i >= 0 ? Math.atan(r) : 2 * Math.PI - Math.atan(r) : i >= 0 ? Math.PI - Math.atan(r) : Math.PI + Math.atan(r);
} else o = i > 0 ? Math.PI / 2 : -Math.PI / 2;
return 180 * o / Math.PI;
};
r([ u(cc.Sprite) ], e.prototype, "pattern", void 0);
r([ u(cc.Node) ], e.prototype, "mask", void 0);
r([ u(cc.SpriteFrame) ], e.prototype, "patterns", void 0);
return r([ h ], e);
}(cc.Component);
n.default = f;
cc._RF.pop();
}, {} ],
$KO: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "29919ogLiNBc6uZon0m+A19", "$KO");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("./core/$UD"), s = t("./$KN"), a = t("./$KP"), h = cc._decorator.ccclass, u = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.Kw = null;
e.Kx = [];
return e;
}
e.prototype.onLoad = function() {
var t = this.node.children[0], e = this.node.children[1];
if (t && e) {
e.zIndex = 1e3;
var n = a.MGXXLDefine.blockSize.w * a.MGXXLDefine.blockScale, i = a.MGXXLDefine.blockSize.h * a.MGXXLDefine.blockScale;
t.width = n * a.MGXXLDefine.crushAreaCount + 38;
t.height = i + 44;
t.y = a.MGXXLDefine.crushImg.bottom;
e.width = t.width + 30;
}
};
e.prototype.Ky = function(t) {
this.Kw = t;
this.node.zIndex = a.MGXXLDefine.areaZIndex.crush;
};
e.prototype.Kz = function(t) {
for (var e = t.JU.type, n = this.Kx.length - 1; n >= 0; n--) if (this.Kx[n].JU.type === e) return n + 1;
return this.Kx.length;
};
e.prototype.KA = function(t) {
var e = this, n = this.Kz(t);
this.Kx.splice(n, 0, t);
var i = this.node.convertToNodeSpaceAR(t.node.convertToWorldSpaceAR(cc.Vec2.ZERO));
t.node.parent = this.node;
t.node.setPosition(i);
t.node.zIndex = 0;
t.JW = s.$gB.c;
cc.Tween.stopAllByTarget(t.node);
cc.tween(t.node).to(a.MGXXLDefine.actionTime, {
x: this.KB(n),
y: this.KC()
}).call(function() {
if (e.isValid && cc.isValid(t)) {
t.JW = s.$gB.d;
e.KD();
e.scheduleOnce(function() {
e.isValid && e.KE();
}, 0);
}
}).start();
this.KD();
return n;
};
e.prototype.KB = function(t) {
var e = a.MGXXLDefine.blockScale * a.MGXXLDefine.blockSize.w;
return t * e + .5 * e + -e * a.MGXXLDefine.crushAreaCount * .5;
};
e.prototype.KC = function() {
return a.MGXXLDefine.blockScale * a.MGXXLDefine.blockSize.h * .5 + a.MGXXLDefine.crushImg.bottom;
};
e.prototype.KD = function() {
for (var t = function(t) {
var n = e.Kx[t];
if (n.JW !== s.$gB.d) return "continue";
var i = e.KB(t);
if (Math.abs(n.node.x - i) < .1) return "continue";
n.JW = s.$gB.c;
cc.Tween.stopAllByTarget(n.node);
cc.tween(n.node).to(.15, {
x: i,
y: e.KC()
}).call(function() {
cc.isValid(n) && (n.JW = s.$gB.d);
}).start();
}, e = this, n = 0; n < this.Kx.length; n++) t(n);
};
e.prototype.KE = function() {
for (var t = a.MGXXLDefine.crushCondCount, e = 0; e + t <= this.Kx.length; e++) {
for (var n = this.Kx[e].JU.type, i = !0, o = 1; o < t; o++) if (this.Kx[e + o].JU.type !== n) {
i = !1;
break;
}
if (i) {
for (var r = 0, s = this.Kx.splice(e, t); r < s.length; r++) {
var h = s[r];
this.Kw.scene.LD(h.node.convertToWorldSpaceAR(cc.Vec3.ZERO));
this.Kw.scene.LC(h.node);
}
this.KD();
this.Kw.clearRecord();
c.default.cJD("break");
break;
}
}
this.Kw.checkEnd();
};
e.prototype.KF = function() {
var t = this.Kw.records.pop();
if (!t) return null;
var e = this.Kx.findIndex(function(e) {
return a.MGXXLHelper.getBlockIndex(e.JU) === t.index;
});
if (-1 === e) return null;
var n = this.Kx[e];
if (n.JW === s.$gB.c) {
this.Kw.records.push(t);
return null;
}
this.Kx.splice(e, 1);
this.KD();
return n;
};
e.prototype.KG = function() {
for (var t = 0, e = this.Kx; t < e.length; t++) {
var n = e[t];
this.Kw.scene.LC(n.node);
}
this.Kx.length = 0;
};
return r([ h ], e);
}(cc.Component);
n.default = u;
cc._RF.pop();
}, {
"./$KN": "$KN",
"./$KP": "$KP",
"./core/$UD": "$UD"
} ],
$KP: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "745c4uAKBBO27OshPvMN/1X", "$KP");
Object.defineProperty(n, "__esModule", {
value: !0
});
n.MGXXLGame = n.MGXXLHelper = n.MGXXLState = n.MGXXLDefine = n.MGXXL_PATTERN_COUNT = n.MGXXLProp = n.MGXXL_EDIT_CONFIG = void 0;
var i, o, r = t("./core/$UD"), c = t("./$KN"), s = {
offset: [ 0, 310 ],
blockOffsetX: -11,
blockArray: [ [ 1, 2, 0, 2 ], [ 1, 2, 1, 2 ], [ 1, 2, 2, 2 ], [ 1, 2, 3, 2 ], [ 1, 2, 4, 2 ], [ 1, 2, 5, 2 ], [ 1, 2, 6, 2 ], [ 1, 2, 7, 2 ], [ 1, 2, 8, 2 ], [ 1, 2, 9, 2 ], [ 21, 2, 0, 2 ], [ 21, 2, 1, 2 ], [ 21, 2, 2, 2 ], [ 21, 2, 3, 2 ], [ 21, 2, 4, 2 ], [ 21, 2, 5, 2 ], [ 21, 2, 6, 2 ], [ 21, 2, 7, 2 ], [ 21, 2, 8, 2 ], [ 21, 2, 9, 2 ], [ 1, 10, 0, 1 ], [ 1, 10, 1, 1 ], [ 1, 10, 2, 1 ], [ 1, 10, 3, 1 ], [ 1, 10, 4, 1 ], [ 1, 10, 5, 1 ], [ 1, 10, 6, 1 ], [ 1, 10, 7, 1 ], [ 1, 10, 8, 1 ], [ 1, 10, 9, 1 ], [ 21, 10, 0, 1 ], [ 21, 10, 1, 1 ], [ 21, 10, 2, 1 ], [ 21, 10, 3, 1 ], [ 21, 10, 4, 1 ], [ 21, 10, 5, 1 ], [ 21, 10, 6, 1 ], [ 21, 10, 7, 1 ], [ 21, 10, 8, 1 ], [ 21, 10, 9, 1 ], [ 8, 2, 9, 0 ], [ 10, 2, 9, 0 ], [ 12, 2, 9, 0 ], [ 4, 2, 9, 0 ], [ 9, 4, 9, 0 ], [ 11, 4, 9, 0 ], [ 14, 2, 9, 0 ], [ 13, 4, 9, 0 ], [ 18, 2, 9, 0 ], [ 8, 6, 9, 0 ], [ 10, 6, 9, 0 ], [ 12, 6, 9, 0 ], [ 14, 6, 9, 0 ], [ 9, 8, 9, 0 ], [ 11, 8, 9, 0 ], [ 13, 8, 9, 0 ], [ 4, 4, 9, 0 ], [ 4, 6, 9, 0 ], [ 18, 4, 9, 0 ], [ 18, 6, 9, 0 ], [ 18, 10, 9, 0 ], [ 5, 2, 8, 0 ], [ 5, 4, 8, 0 ], [ 5, 6, 8, 0 ], [ 9, 2, 8, 0 ], [ 11, 2, 8, 0 ], [ 13, 2, 8, 0 ], [ 9, 4, 8, 0 ], [ 11, 4, 8, 0 ], [ 13, 4, 8, 0 ], [ 10, 7, 8, 0 ], [ 12, 7, 8, 0 ], [ 4, 10, 9, 0 ], [ 16, 10, 8, 0 ], [ 5, 10, 8, 0 ], [ 10, 6, 7, 0 ], [ 12, 6, 7, 0 ], [ 10, 4, 7, 0 ], [ 12, 4, 7, 0 ], [ 7, 2, 7, 0 ], [ 11, 2, 7, 0 ], [ 13, 2, 7, 0 ], [ 9, 2, 7, 0 ], [ 15, 2, 7, 0 ], [ 6, 4, 7, 0 ], [ 6, 6, 7, 0 ], [ 16, 4, 7, 0 ], [ 16, 6, 7, 0 ], [ 5, 10, 7, 0 ], [ 16, 10, 7, 0 ], [ 17, 2, 8, 0 ], [ 17, 4, 8, 0 ], [ 17, 6, 8, 0 ], [ 9, 3, 6, 0 ], [ 11, 3, 6, 0 ], [ 13, 3, 6, 0 ], [ 9, 5, 6, 0 ], [ 11, 5, 6, 0 ], [ 13, 5, 6, 0 ], [ 7, 4, 6, 0 ], [ 7, 6, 6, 0 ], [ 15, 4, 6, 0 ], [ 15, 6, 6, 0 ], [ 5, 2, 6, 0 ], [ 17, 2, 6, 0 ], [ 5, 8, 6, 0 ], [ 17, 8, 6, 0 ], [ 10, 8, 6, 0 ], [ 12, 8, 6, 0 ], [ 12, 5, 5, 0 ], [ 13, 3, 5, 0 ], [ 11, 3, 5, 0 ], [ 9, 3, 5, 0 ], [ 8, 5, 5, 0 ], [ 10, 7, 5, 0 ], [ 12, 7, 5, 0 ], [ 14, 7, 5, 0 ], [ 6, 2, 5, 0 ], [ 16, 2, 5, 0 ], [ 5, 7, 5, 0 ], [ 5, 5, 5, 0 ], [ 16, 5, 5, 0 ], [ 16, 7, 5, 0 ], [ 18, 10, 5, 0 ], [ 4, 10, 5, 0 ], [ 9, 8, 4, 0 ], [ 11, 8, 4, 0 ], [ 7, 6, 4, 0 ], [ 9, 6, 4, 0 ], [ 11, 6, 4, 0 ], [ 13, 6, 4, 0 ], [ 8, 4, 4, 0 ], [ 10, 4, 4, 0 ], [ 12, 4, 4, 0 ], [ 7, 2, 3, 0 ], [ 9, 2, 3, 0 ], [ 13, 2, 3, 0 ], [ 11, 2, 3, 0 ], [ 14, 4, 3, 0 ], [ 12, 4, 3, 0 ], [ 10, 4, 3, 0 ], [ 8, 4, 3, 0 ], [ 6, 4, 3, 0 ], [ 7, 6, 3, 0 ], [ 9, 6, 3, 0 ], [ 11, 6, 3, 0 ], [ 13, 6, 3, 0 ], [ 9, 3, 2, 0 ], [ 11, 3, 2, 0 ], [ 13, 3, 2, 0 ], [ 14, 5, 2, 0 ], [ 13, 7, 2, 0 ], [ 11, 7, 2, 0 ], [ 9, 7, 2, 0 ], [ 10, 5, 2, 0 ], [ 8, 5, 2, 0 ], [ 7, 7, 1, 0 ], [ 6, 7, 0, 0 ], [ 9, 5, 1, 0 ], [ 10, 5, 0, 0 ], [ 11, 5, 1, 0 ], [ 12, 5, 0, 0 ], [ 9, 3, 1, 0 ], [ 11, 3, 1, 0 ], [ 13, 3, 1, 0 ], [ 7, 2, 0, 0 ], [ 14, 6, 1, 0 ], [ 15, 6, 0, 0 ], [ 12, 7, 1, 0 ], [ 12, 7, 0, 0 ], [ 9, 7, 1, 0 ], [ 8, 7, 0, 0 ], [ 10, 3, 0, 0 ], [ 12, 3, 0, 0 ] ]
};
n.MGXXL_EDIT_CONFIG = {
offset: [ 0, 310 ],
blockOffsetX: -11,
blockTypes: [],
blockArray: []
};
(function(t) {
t[t.MOVE_OUT = 1] = "MOVE_OUT";
t[t.UNDO = 2] = "UNDO";
t[t.SHUFFLE = 3] = "SHUFFLE";
t[t.REVIVE = 4] = "REVIVE";
})(i = n.MGXXLProp || (n.MGXXLProp = {}));
n.MGXXL_PATTERN_COUNT = 16;
n.MGXXLDefine = {
crushCondCount: 3,
crushAreaCount: 7,
moveOutCount: 3,
blockScale: .45,
blockSize: {
w: 154,
h: 170
},
crushImg: {
bottom: 60
},
props: [ {
uiIndex: 0,
prop: i.MOVE_OUT,
icon: "textures/common/btn001",
name: "移出道具",
desc: "移出三个砖块并把它们堆放在旁边"
}, {
uiIndex: 1,
prop: i.UNDO,
icon: "textures/common/btn002",
name: "撤回道具",
desc: "撤回上一次点击砖块并把它放到原位置"
}, {
uiIndex: 2,
prop: i.SHUFFLE,
icon: "textures/common/btn003",
name: "洗牌道具",
desc: "随机打乱所有未使用的所有砖块"
} ],
reviveProp: {
uiIndex: 2,
prop: i.REVIVE,
icon: "textures/common/btn001",
name: "复活",
desc: "复活并使用移出道具"
},
areaZIndex: {
grid: 100,
crush: 300,
out: 200
},
actionTime: .2,
shuffleTime: 3
};
(function(t) {
t[t.NONE = 0] = "NONE";
t[t.PLAYING = 1] = "PLAYING";
t[t.PAUSE = 2] = "PAUSE";
t[t.END = 3] = "END";
})(o = n.MGXXLState || (n.MGXXLState = {}));
var a = function() {
function t() {}
t.getStackIndex = function(t) {
return t.x + "_" + t.y;
};
t.getBlockIndex = function(t) {
return t.x + "_" + t.y + "_" + t.z + "_" + t.stack;
};
t.getGridPosition = function(t) {
var e = n.MGXXLDefine.blockSize.w * n.MGXXLDefine.blockScale, i = n.MGXXLDefine.blockSize.h * n.MGXXLDefine.blockScale, o = t.x * e * .5, r = -t.y * i * .5;
switch (t.stack) {
case 1:
r += t.z * i * .1;
break;

case 2:
r -= t.z * i * .1;
break;

case 3:
o -= t.z * e * .1;
break;

case 4:
o += t.z * e * .1;
}
return {
x: o,
y: r
};
};
t.shuffle = function(t) {
for (var e = t.length - 1; e > 0; e--) {
var n = Math.floor(Math.random() * (e + 1)), i = t[n];
t[n] = t[e];
t[e] = i;
}
};
t.rnd = function(t, e) {
return t + Math.floor(Math.random() * (e - t + 1));
};
t.cloneObj = function(t) {
return JSON.parse(JSON.stringify(t));
};
t.parseLevelCfg = function(t) {
if (!t || !Array.isArray(t.blockArray)) throw new Error("关卡数据缺少 blockArray");
for (var e = {
offset: Array.isArray(t.offset) && t.offset.length >= 2 ? [ t.offset[0], t.offset[1] ] : [ 0, 310 ],
blockOffsetX: "number" == typeof t.blockOffsetX ? t.blockOffsetX : -11,
blockArray: [],
blockTypes: Array.isArray(t.blockTypes) ? t.blockTypes : []
}, n = 0, i = t.blockArray; n < i.length; n++) {
var o = i[n];
if (!Array.isArray(o) || o.length < 4) throw new Error("关卡方块数据格式错误: " + JSON.stringify(o));
e.blockArray.push({
x: o[0],
y: o[1],
z: o[2],
stack: o[3],
type: 0
});
}
return e;
};
t.stringifyLevelCfg = function(t) {
for (var e = {
offset: [ t.offset[0], t.offset[1] ],
blockOffsetX: t.blockOffsetX,
blockArray: [],
blockTypes: t.blockTypes || []
}, n = 0, i = t.blockArray; n < i.length; n++) {
var o = i[n];
e.blockArray.push([ o.x, o.y, o.z, o.stack ]);
}
return JSON.stringify(e);
};
t.buildTypeArray = function(e) {
var i = e.blockArray.length, o = [];
if (Array.isArray(e.blockTypes) && e.blockTypes.length > 0) {
for (var c = 0, s = e.blockTypes; c < s.length; c++) for (var a = s[c], h = 0; h < a[0]; h++) o.push(a[1], a[1], a[1]);
if (o.length !== i) {
r.default.cJR("blockTypes 数量(" + o.length + ")与方块数(" + i + ")不一致，改用默认分配");
o.length = 0;
}
}
if (0 === o.length) for (h = 0; h < i; h++) o.push(Math.floor(h / 3) % n.MGXXL_PATTERN_COUNT + 1);
t.shuffle(o);
return o;
};
return t;
}();
n.MGXXLHelper = a;
var h = function() {
function t(t) {
this.config = null;
this.scene = null;
this.state = o.NONE;
this.time = 0;
this.propData = new Map();
this.actionTime = 0;
this.revivedTimes = 0;
this.records = [];
this.isEditerMode = !1;
this.editLayerNum = 0;
this.stateBeforePause = o.NONE;
this.isEnding = !1;
this.scene = t;
this.scene.grid.KL(this);
this.scene.crush.Ky(this);
this.scene.out.Lg(this);
}
t.prototype.start = function(t) {
t ? this.config = t : this.config && this.config !== n.MGXXL_EDIT_CONFIG || (this.config = a.parseLevelCfg(s));
this.time = 0;
this.actionTime = 0;
this.revivedTimes = 0;
this.isEnding = !1;
this.clearRecord();
this.state = o.PLAYING;
var e = a.cloneObj(this.config), i = e.blockArray.length % n.MGXXLDefine.crushCondCount;
if (0 !== i) {
r.default.cJR("关卡方块数 " + e.blockArray.length + " 不是 3 的倍数，已裁掉末尾 " + i + " 块");
e.blockArray.length -= i;
}
var c = n.MGXXLDefine.blockSize.w * n.MGXXLDefine.blockScale;
this.scene.grid.node.setPosition(e.offset[0] + e.blockOffsetX * c * .5, e.offset[1]);
for (var h = a.buildTypeArray(e), u = 0; u < e.blockArray.length; u++) e.blockArray[u].type = h[u];
this.scene.crush.KG();
this.scene.out.Lm();
this.scene.grid.KM(e.blockArray);
this.propData.clear();
for (var f = 0, l = n.MGXXLDefine.props; f < l.length; f++) {
var p = l[f];
this.propData.set(p.prop, {
count: 0,
usedCount: 0
});
}
this.scene.Ly();
if (this.isEditerMode) {
this.scene.grid.La(!0);
this.scene.grid.KZ(this.editLayerNum);
this.scene.Lz();
}
r.default.cJN();
};
t.prototype.pause = function() {
if (this.state === o.PLAYING) {
this.stateBeforePause = this.state;
this.state = o.PAUSE;
}
};
t.prototype.resume = function() {
this.state === o.PAUSE && (this.state = this.stateBeforePause === o.NONE ? o.PLAYING : this.stateBeforePause);
};
t.prototype.update = function(t) {
if (this.state === o.PLAYING) {
this.actionTime = Math.max(this.actionTime - t, 0);
this.time += t;
}
};
t.prototype.checkEnd = function() {
var t = this;
if (this.state === o.PLAYING && !this.isEnding) {
var e = this.scene.crush.Kx.length;
if (e + this.scene.grid.KW() + this.scene.out.Lf.length === 0) {
this.state = o.END;
this.isEnding = !0;
r.default.cJM(this.time);
var i = {
game: this,
result: 1
};
r.default.cJG("MGXXLResultView", i);
} else if (e >= n.MGXXLDefine.crushAreaCount) {
this.state = o.END;
this.isEnding = !0;
var c = function() {
var e = {
game: t,
result: 0
};
r.default.cJG("MGXXLResultView", e);
};
if (this.revivedTimes < 1 && this.getMoveOutCapacity() > 0) {
i = {
cfg: n.MGXXLDefine.reviveProp,
ret: function(e) {
if (cc.isValid(t.scene)) if (e) {
t.revivedTimes++;
t.isEnding = !1;
t.state = o.PLAYING;
t.doMoveOutBlock();
r.default.cJD("prop");
} else c();
}
};
r.default.cJG("MGXXLPropView", i);
} else c();
}
}
};
t.prototype.exitGame = function() {
this.state = o.END;
r.default.cJA();
};
t.prototype.clickBlock = function(t) {
if (!this.isInAction() && cc.isValid(t) && t.JU) if (t.JW === c.$gB.b) {
if (t.JV) return;
this.pushAction(n.MGXXLDefine.actionTime);
this.scene.grid.KU(t);
this.scene.crush.KA(t);
this.records.push({
index: a.getBlockIndex(t.JU)
});
} else if (t.JW === c.$gB.e) {
this.clearRecord();
this.pushAction(n.MGXXLDefine.actionTime);
this.scene.out.Ll(t);
this.scene.crush.KA(t);
}
};
t.prototype.getMoveOutCapacity = function() {
return Math.max(0, n.MGXXLDefine.crushAreaCount - this.scene.out.Lf.length);
};
t.prototype.doMoveOutBlock = function() {
var t = Math.min(n.MGXXLDefine.moveOutCount, this.scene.crush.Kx.length, this.getMoveOutCapacity());
if (t < 1) return !1;
this.pushAction(n.MGXXLDefine.actionTime);
for (var e = 0, i = this.scene.crush.Kx.splice(0, t); e < i.length; e++) {
var o = i[e];
this.scene.out.Lj(o);
}
this.scene.crush.KD();
this.clearRecord();
return !0;
};
t.prototype.useProp = function(t) {
if (this.isInAction()) return !1;
var e = this.propData.get(t);
if (!e || e.count < 1) return !1;
switch (t) {
case i.MOVE_OUT:
if (!this.doMoveOutBlock()) return !1;
break;

case i.UNDO:
if (!this.records.length) return !1;
var o = this.scene.crush.KF();
if (!o) return !1;
this.pushAction(n.MGXXLDefine.actionTime);
this.scene.grid.KV(o);
break;

case i.SHUFFLE:
if (this.scene.grid.KW() < 2) return !1;
this.pushAction(n.MGXXLDefine.shuffleTime);
this.clearRecord();
var r = this.scene.grid.KX();
a.shuffle(r);
this.scene.grid.KY(r);
break;

default:
return !1;
}
e.count -= 1;
e.usedCount += 1;
this.scene.Ly();
return !0;
};
t.prototype.checkCanUseProp = function(t) {
if (this.isInAction()) return !1;
switch (t) {
case i.MOVE_OUT:
if (this.scene.crush.Kx.length < 1) {
r.default.cJJ("没有可以移出的砖块");
return !1;
}
if (this.getMoveOutCapacity() < 1) {
r.default.cJJ("移出区已满");
return !1;
}
break;

case i.UNDO:
if (!this.records.length) {
r.default.cJJ("没有可以撤回的砖块");
return !1;
}
break;

case i.SHUFFLE:
if (this.scene.grid.KW() < 2) {
r.default.cJJ("没有可以打乱的砖块");
return !1;
}
}
return !0;
};
t.prototype.addProp = function(t) {
var e = this.propData.get(t);
if (e) {
e.count += 1;
this.scene.Ly();
}
};
t.prototype.pushAction = function(t) {
t > this.actionTime && (this.actionTime = t);
};
t.prototype.isInAction = function() {
return this.actionTime > 0 || this.state !== o.PLAYING || this.isEditerMode;
};
t.prototype.clearRecord = function() {
this.records.length = 0;
};
t.prototype.setEditerLayer = function(t) {
this.editLayerNum = Math.max(0, t);
this.scene.grid.KZ(this.editLayerNum);
this.scene.Lz();
};
t.prototype.setEditerMode = function(t) {
if (this.isEditerMode !== t) {
this.isEditerMode = t;
this.scene.grid.La(t);
t && this.scene.grid.KZ(this.editLayerNum);
this.scene.Lz();
}
};
return t;
}();
n.MGXXLGame = h;
cc._RF.pop();
}, {
"./$KN": "$KN",
"./core/$UD": "$UD"
} ],
$KQ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "3f937bHFDZJua1WTQpZoQPt", "$KQ");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("./$KN"), s = t("./$KP"), a = cc._decorator.ccclass, h = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.KH = null;
e.KI = [];
e.KJ = [];
e.KK = null;
return e;
}
e.prototype.KL = function(t) {
this.KH = t;
this.node.zIndex = s.MGXXLDefine.areaZIndex.grid;
};
e.prototype.KM = function(t) {
this.KP();
for (var e = 0, n = t; e < n.length; e++) {
var i = n[e], o = this.KN(i);
o && (i.stack ? this.KO(s.MGXXLHelper.getStackIndex(i)).b.push(o) : this.KI.push(o));
}
for (var r = 0, c = this.KJ; r < c.length; r++) c[r].b.sort(function(t, e) {
return t.JU.z - e.JU.z;
});
this.KQ();
};
e.prototype.KN = function(t) {
var e = this.KH.scene.LB();
if (!e) return null;
var n = e.getComponent(c.default);
if (!n) {
cc.warn("[MGXXL] 方块预制缺少 MGXXLBlock 组件");
e.destroy();
return null;
}
e.scale = s.MGXXLDefine.blockScale;
e.active = !0;
this.node.addChild(e);
n.JZ(t);
var i = s.MGXXLHelper.getGridPosition(t);
e.setPosition(i.x, i.y);
e.zIndex = t.z;
return n;
};
e.prototype.KO = function(t) {
var e = this.KJ.find(function(e) {
return e.c === t;
});
if (!e) {
e = {
c: t,
b: []
};
this.KJ.push(e);
}
return e;
};
e.prototype.KP = function() {
for (var t = 0, e = this.KI; t < e.length; t++) {
var n = e[t];
this.KH.scene.LC(n.node);
}
this.KI.length = 0;
for (var i = 0, o = this.KJ; i < o.length; i++) for (var r = 0, c = o[i].b; r < c.length; r++) {
n = c[r];
this.KH.scene.LC(n.node);
}
this.KJ.length = 0;
};
e.prototype.KQ = function() {
this.KT();
for (var t = 0, e = this.KJ; t < e.length; t++) {
var n = e[t];
this.KS(n);
}
};
e.prototype.KR = function(t) {
var e = this.KJ.find(function(e) {
return e.c === t;
});
e && this.KS(e);
};
e.prototype.KS = function(t) {
var e = t.b[t.b.length - 1];
if (e) for (var n = 0, i = t.b; n < i.length; n++) {
var o = i[n];
o.Kb(o !== e, o === e);
}
};
e.prototype.KT = function() {
for (var t = 0, e = this.KI; t < e.length; t++) {
for (var n = e[t], i = !1, o = 0, r = this.KI; o < r.length; o++) {
var c = r[o];
if (c.JU.z > n.JU.z && Math.abs(c.JU.x - n.JU.x) < 2 && Math.abs(c.JU.y - n.JU.y) < 2) {
i = !0;
break;
}
}
n.Kb(i, !0);
}
};
e.prototype.KU = function(t) {
if (t.JU.stack) {
var e = s.MGXXLHelper.getStackIndex(t.JU), n = this.KJ.find(function(t) {
return t.c === e;
});
if (n && n.b[n.b.length - 1] === t) {
n.b.pop();
this.KS(n);
}
} else {
var i = this.KI.indexOf(t);
if (-1 !== i) {
this.KI.splice(i, 1);
this.KT();
}
}
};
e.prototype.KV = function(t) {
var e = this, n = this.node.convertToNodeSpaceAR(t.node.convertToWorldSpaceAR(cc.Vec2.ZERO));
t.node.parent = this.node;
t.node.setPosition(n);
t.node.zIndex = 999;
var i = t.JU, o = s.MGXXLHelper.getGridPosition(i);
i.stack ? this.KO(s.MGXXLHelper.getStackIndex(i)).b.push(t) : this.KI.push(t);
this.node.zIndex = 1e3;
t.JW = c.$gB.c;
cc.Tween.stopAllByTarget(t.node);
cc.tween(t.node).to(s.MGXXLDefine.actionTime, {
x: o.x,
y: o.y
}).call(function() {
if (e.isValid) {
e.node.zIndex = s.MGXXLDefine.areaZIndex.grid;
if (cc.isValid(t)) {
t.JW = c.$gB.b;
t.node.zIndex = i.z;
i.stack ? e.KR(s.MGXXLHelper.getStackIndex(i)) : e.KT();
}
}
}).start();
};
e.prototype.KW = function() {
for (var t = this.KI.length, e = 0, n = this.KJ; e < n.length; e++) t += n[e].b.length;
return t;
};
e.prototype.KX = function() {
for (var t = this.KI.map(function(t) {
return t.JU.type;
}), e = 0, n = this.KJ; e < n.length; e++) {
var i = n[e];
t = t.concat(i.b.map(function(t) {
return t.JU.type;
}));
}
return t;
};
e.prototype.KY = function(t) {
var e = this, n = 0;
this.KI.forEach(function(i) {
i.Kr(t[n++], e.node, i.node.position);
});
for (var i = 0, o = this.KJ; i < o.length; i++) o[i].b.forEach(function(i) {
i.Kr(t[n++], e.node, i.node.position);
});
};
e.prototype.KZ = function(t) {
this.KI.forEach(function(e) {
e.node.active = e.JU.z === t;
e.Kb(!1);
});
for (var e = 0, n = this.KJ; e < n.length; e++) for (var i = 0, o = n[e].b; i < o.length; i++) o[i].node.active = !1;
};
e.prototype.La = function(t) {
if (t) {
this.KK || this.Lb();
this.KK && (this.KK.active = !0);
} else {
this.KK && (this.KK.active = !1);
this.KI.forEach(function(t) {
t.node.active = !0;
});
for (var e = 0, n = this.KJ; e < n.length; e++) for (var i = 0, o = n[e].b; i < o.length; i++) o[i].node.active = !0;
this.KQ();
}
};
e.prototype.Lb = function() {
var t = this, e = this.KH.scene.node.getChildByName("btnAdd");
if (e) {
this.KK = new cc.Node("editLayer");
this.node.addChild(this.KK, 1e3);
for (var n = function(n) {
for (var o = function(o) {
var r = {
x: n,
y: o,
z: 0,
stack: 0,
type: 1
}, c = s.MGXXLHelper.getGridPosition(r), a = cc.instantiate(e);
a.active = !0;
i.KK.addChild(a);
a.setPosition(c.x, c.y);
a.on("click", function() {
return t.Lc(n, o);
}, i);
}, r = 0; r < 11; r++) o(r);
}, i = this, o = 0; o < 23; o++) n(o);
} else cc.warn("[MGXXL] 场景缺少 btnAdd 模板，无法进入编辑模式");
};
e.prototype.Lc = function(t, e) {
var n = {
x: t,
y: e,
z: this.KH.editLayerNum,
stack: 0,
type: 1
}, i = s.MGXXLHelper.getBlockIndex(n), o = this.KI.findIndex(function(t) {
return s.MGXXLHelper.getBlockIndex(t.JU) === i;
});
if (-1 === o) {
for (var r = 0, c = this.KI; r < c.length; r++) {
var a = c[r];
if (a.JU.z === n.z && Math.abs(a.JU.x - n.x) < 2 && Math.abs(a.JU.y - n.y) < 2) return;
}
var h = this.KN(n);
if (h) {
h.Kb(!1);
this.KI.push(h);
this.KH.scene.Lz();
this.KH.scene.LA();
}
} else {
this.KH.scene.LC(this.KI[o].node);
this.KI.splice(o, 1);
this.KH.scene.Lz();
this.KH.scene.LA();
}
};
e.prototype.Ld = function(t) {
var e = s.MGXXLHelper.cloneObj(s.MGXXL_EDIT_CONFIG);
t && this.KH.config && (e.blockTypes = this.KH.config.blockTypes);
for (var n = [], i = 0, o = this.KJ; i < o.length; i++) {
var r = o[i];
n.push.apply(n, r.b.map(function(t) {
return t.JU;
}));
}
e.blockArray = n.concat(this.KI.map(function(t) {
return t.JU;
}));
return e;
};
return r([ a ], e);
}(cc.Component);
n.default = h;
cc._RF.pop();
}, {
"./$KN": "$KN",
"./$KP": "$KP"
} ],
$KR: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "94024NzWgRHC6L8umoYCWAm", "$KR");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("./$KN"), s = t("./$KP"), a = cc._decorator.ccclass, h = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.Le = null;
e.Lf = [];
return e;
}
e.prototype.onLoad = function() {
this.node.y = -200;
};
e.prototype.Lg = function(t) {
this.Le = t;
this.node.zIndex = s.MGXXLDefine.areaZIndex.out;
};
e.prototype.Lh = function(t) {
var e = s.MGXXLDefine.blockScale * s.MGXXLDefine.blockSize.w;
return t * e + .5 * e + -e * s.MGXXLDefine.crushAreaCount * .5;
};
e.prototype.Li = function() {
return s.MGXXLDefine.blockScale * s.MGXXLDefine.blockSize.h * .5;
};
e.prototype.Lj = function(t) {
var e = this, n = this.Lf.length;
this.Lf.push(t);
var i = this.node.convertToNodeSpaceAR(t.node.convertToWorldSpaceAR(cc.Vec2.ZERO));
t.node.parent = this.node;
t.node.setPosition(i);
t.node.zIndex = 0;
t.JW = c.$gB.c;
this.node.zIndex = 1e3;
cc.Tween.stopAllByTarget(t.node);
cc.tween(t.node).to(s.MGXXLDefine.actionTime, {
x: this.Lh(n),
y: this.Li()
}).call(function() {
if (e.isValid) {
e.node.zIndex = s.MGXXLDefine.areaZIndex.out;
if (cc.isValid(t)) {
t.JW = c.$gB.e;
e.Lk();
}
}
}).start();
this.Lk();
};
e.prototype.Lk = function() {
for (var t = function(t) {
var n = e.Lf[t];
if (n.JW !== c.$gB.e) return "continue";
var i = e.Lh(t);
if (Math.abs(n.node.x - i) < .1) return "continue";
n.JW = c.$gB.c;
cc.Tween.stopAllByTarget(n.node);
cc.tween(n.node).to(.15, {
x: i,
y: e.Li()
}).call(function() {
cc.isValid(n) && (n.JW = c.$gB.e);
}).start();
}, e = this, n = 0; n < this.Lf.length; n++) t(n);
};
e.prototype.Ll = function(t) {
var e = this.Lf.indexOf(t);
if (-1 !== e) {
this.Lf.splice(e, 1);
this.Lk();
}
};
e.prototype.Lm = function() {
for (var t = 0, e = this.Lf; t < e.length; t++) {
var n = e[t];
this.Le.scene.LC(n.node);
}
this.Lf.length = 0;
};
return r([ a ], e);
}(cc.Component);
n.default = h;
cc._RF.pop();
}, {
"./$KN": "$KN",
"./$KP": "$KP"
} ],
$KS: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "f31a7zTUopIlKfSMfQDYSTI", "$KS");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("./core/$UD"), s = t("./$KN"), a = t("./$KO"), h = t("./$KP"), u = t("./$KQ"), f = t("./$KR"), l = cc._decorator, p = l.ccclass, d = l.property, y = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.grid = null;
e.crush = null;
e.out = null;
e.blockPrefab = null;
e.scoreParticle = null;
e.Ln = null;
e.Lo = new cc.NodePool();
e.Lp = new cc.NodePool();
e.Lq = null;
return e;
}
e.prototype.onLoad = function() {
if (c.default.cJP(!!this.grid && !!this.crush && !!this.out, "MGXXLScene 缺少 grid/crush/out 绑定")) {
this.Lr();
this.Lt();
this.Lv();
this.node.on(s.$gC.b, this.Lw, this);
}
};
e.prototype.start = function() {
var t = this.node.getChildByName("leftTop");
t && (t.x = -cc.winSize.width / 2 + c.default.cJV().x);
this.Ln = new h.MGXXLGame(this);
this.Ln.start();
this.Lx();
c.default.cJE("bgm");
};
e.prototype.update = function(t) {
this.Ln && this.Ln.update(t);
this.Lx();
};
e.prototype.onDestroy = function() {
cc.systemEvent.targetOff(this);
this.node.off(s.$gC.b, this.Lw, this);
this.unscheduleAllCallbacks();
this.Lo.clear();
this.Lp.clear();
this.Ln = null;
};
e.prototype.Lr = function() {
var t = this, e = this.node.getChildByName("leftTop");
if (c.default.cJP(!!e, "MGXXLScene 缺少 leftTop")) {
this.Lq = e.getChildByName("btnExit");
if (this.Lq) {
this.Lq.active = !1;
this.Lq.on("click", this.Ls, this);
}
var n = e.getChildByName("btn0");
n && (n.active = !1);
var i = e.getChildByName("btn1");
i && i.on("click", function() {
!t.Ln || t.Ln.isEditerMode || c.default.cJI() || c.default.cJG("MGXXLRankingView");
}, this);
var o = e.getChildByName("btnEdit");
if (o) {
o.active = !1;
o.on("click", function() {
t.Ln && t.Ln.setEditerMode(!t.Ln.isEditerMode);
}, this);
}
}
};
e.prototype.Ls = function() {
if (this.Ln && !this.Ln.isEditerMode && !c.default.cJI()) {
this.Ln.pause();
var t = {
game: this.Ln
};
c.default.cJG("MGXXLPauseView", t);
}
};
e.prototype.Lt = function() {
var t = this, e = this.node.getChildByName("btns");
if (c.default.cJP(!!e, "MGXXLScene 缺少 btns")) for (var n = function(n) {
var o = e.children[n.uiIndex];
if (!c.default.cJP(!!o, "缺少道具按钮 " + n.uiIndex)) return "continue";
o.on("click", function() {
return t.Lu(n);
}, i);
}, i = this, o = 0, r = h.MGXXLDefine.props; o < r.length; o++) n(r[o]);
};
e.prototype.Lu = function(t) {
var e = this;
if (this.Ln && !this.Ln.isInAction() && !c.default.cJI()) {
var n = this.Ln.propData.get(t.prop);
if (n) if (n.usedCount >= 1) c.default.cJJ("本局该道具已使用"); else if (this.Ln.checkCanUseProp(t.prop)) {
var i = {
cfg: t,
checkCanUse: function() {
return !!e.Ln && e.Ln.checkCanUseProp(t.prop);
},
ret: function(n) {
if (n && e.isValid && e.Ln) {
e.Ln.addProp(t.prop);
e.Ln.useProp(t.prop) && c.default.cJD("prop");
}
}
};
c.default.cJG("MGXXLPropView", i);
}
}
};
e.prototype.Lv = function() {
var t = this.node.getChildByName("panelLayerEdit");
t && (t.active = !1);
};
e.prototype.Lw = function(t) {
t.stopPropagation();
if (this.Ln && !c.default.cJI()) {
var e = t.getUserData();
this.Ln.clickBlock(e);
}
};
e.prototype.Lx = function() {
if (this.Lq) {
var t = c.default.cJu();
this.Lq.active !== t && (this.Lq.active = t);
}
};
e.prototype.Ly = function() {
var t = this.node.getChildByName("btns");
if (t && this.Ln) for (var e = 0, n = h.MGXXLDefine.props; e < n.length; e++) {
var i = n[e], o = this.Ln.propData.get(i.prop), r = t.children[i.uiIndex];
if (o && r) {
var c = r.getChildByName("lblCount"), s = c ? c.getComponent(cc.Label) : null;
s && (s.string = o.usedCount < 1 ? "1" : "0");
}
}
};
e.prototype.Lz = function() {
var t = this.node.getChildByName("panelLayerEdit");
if (t && this.Ln) {
t.active = this.Ln.isEditerMode;
if (t.active) {
var e = t.getChildByName("lblLayerNum"), n = t.getChildByName("lblTotal");
e && (e.getComponent(cc.Label).string = "第" + (this.Ln.editLayerNum + 1) + "层");
n && (n.getComponent(cc.Label).string = this.grid.KW() + "个");
}
}
};
e.prototype.LA = function() {
var t = this.node.getChildByName("panelLayerEdit");
if (t) {
var e = t.getChildByName("inputImport"), n = e ? e.getComponent(cc.EditBox) : null;
n && (n.string = h.MGXXLHelper.stringifyLevelCfg(this.grid.Ld(!1)));
}
};
e.prototype.LB = function() {
return this.Lo.size() > 0 ? this.Lo.get() : c.default.cJP(!!this.blockPrefab, "MGXXLScene 缺少 blockPrefab") ? cc.instantiate(this.blockPrefab) : null;
};
e.prototype.LC = function(t) {
if (cc.isValid(t)) {
cc.Tween.stopAllByTarget(t);
this.isValid ? this.Lo.put(t) : t.destroy();
}
};
e.prototype.LD = function(t) {
var e = this;
if (this.scoreParticle && this.isValid) {
var n = this.Lp.get();
n || (n = cc.instantiate(this.scoreParticle));
this.node.addChild(n, 2e3);
n.setPosition(this.node.convertToNodeSpaceAR(t));
var i = n.getComponent(cc.ParticleSystem);
i && i.resetSystem();
this.scheduleOnce(function() {
cc.isValid(n) && (e.isValid ? e.Lp.put(n) : n.destroy());
}, .6);
}
};
e.prototype.LE = function() {
var t = h.MGXXLHelper.stringifyLevelCfg(this.grid.Ld(!0));
c.default.cJQ("导出关卡:", t);
if (cc.sys.isBrowser && window.Blob) {
var e = new Blob([ t ]), n = URL.createObjectURL(e), i = document.createElement("a");
i.href = n;
i.download = "level.json";
i.click();
}
};
e.prototype.LF = function() {
var t = this.node.getChildByName("panelLayerEdit"), e = t ? t.getChildByName("inputImport") : null, n = e ? e.getComponent(cc.EditBox) : null, i = n ? n.string : "";
if (i) try {
var o = h.MGXXLHelper.parseLevelCfg(JSON.parse(i));
this.Ln.start(o);
} catch (t) {
c.default.cJR("导入关卡失败", t);
c.default.cJJ("关卡数据格式错误");
} else this.Ln.start(h.MGXXL_EDIT_CONFIG);
};
r([ d(u.default) ], e.prototype, "grid", void 0);
r([ d(a.default) ], e.prototype, "crush", void 0);
r([ d(f.default) ], e.prototype, "out", void 0);
r([ d(cc.Prefab) ], e.prototype, "blockPrefab", void 0);
r([ d(cc.Prefab) ], e.prototype, "scoreParticle", void 0);
return r([ p ], e);
}(cc.Component);
n.default = y;
cc._RF.pop();
}, {
"./$KN": "$KN",
"./$KO": "$KO",
"./$KP": "$KP",
"./$KQ": "$KQ",
"./$KR": "$KR",
"./core/$UD": "$UD"
} ],
$UD: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "71ae2mREyZBobTX1Sl4FbHj", "$UD");
var i = this && this.__awaiter || function(t, e, n, i) {
return new (n || (n = Promise))(function(o, r) {
function c(t) {
try {
a(i.next(t));
} catch (t) {
r(t);
}
}
function s(t) {
try {
a(i.throw(t));
} catch (t) {
r(t);
}
}
function a(t) {
t.done ? o(t.value) : (e = t.value, e instanceof n ? e : new n(function(t) {
t(e);
})).then(c, s);
var e;
}
a((i = i.apply(t, e || [])).next());
});
}, o = this && this.__generator || function(t, e) {
var n, i, o, r, c = {
label: 0,
sent: function() {
if (1 & o[0]) throw o[1];
return o[1];
},
trys: [],
ops: []
};
return r = {
next: s(0),
throw: s(1),
return: s(2)
}, "function" == typeof Symbol && (r[Symbol.iterator] = function() {
return this;
}), r;
function s(t) {
return function(e) {
return a([ t, e ]);
};
}
function a(r) {
if (n) throw new TypeError("Generator is already executing.");
for (;c; ) try {
if (n = 1, i && (o = 2 & r[0] ? i.return : r[0] ? i.throw || ((o = i.return) && o.call(i), 
0) : i.next) && !(o = o.call(i, r[1])).done) return o;
(i = 0, o) && (r = [ 2 & r[0], o.value ]);
switch (r[0]) {
case 0:
case 1:
o = r;
break;

case 4:
c.label++;
return {
value: r[1],
done: !1
};

case 5:
c.label++;
i = r[1];
r = [ 0 ];
continue;

case 7:
r = c.ops.pop();
c.trys.pop();
continue;

default:
if (!(o = c.trys, o = o.length > 0 && o[o.length - 1]) && (6 === r[0] || 2 === r[0])) {
c = 0;
continue;
}
if (3 === r[0] && (!o || r[1] > o[0] && r[1] < o[3])) {
c.label = r[1];
break;
}
if (6 === r[0] && c.label < o[1]) {
c.label = o[1];
o = r;
break;
}
if (o && c.label < o[2]) {
c.label = o[2];
c.ops.push(r);
break;
}
o[2] && c.ops.pop();
c.trys.pop();
continue;
}
r = e.call(t, c);
} catch (t) {
r = [ 6, t ];
i = 0;
} finally {
n = o = 0;
}
if (5 & r[0]) throw r[1];
return {
value: r[0] ? r[1] : void 0,
done: !0
};
}
}, r = this && this.__spreadArrays || function() {
for (var t = 0, e = 0, n = arguments.length; e < n; e++) t += arguments[e].length;
var i = Array(t), o = 0;
for (e = 0; e < n; e++) for (var r = arguments[e], c = 0, s = r.length; c < s; c++, 
o++) i[o] = r[c];
return i;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
n.$so = n.MG_EXIT_UNLOCK_SECONDS = n.MG_BRIDGE_KEY = n.MG_BUNDLE_NAME = void 0;
var c, s = t("./$UE");
n.MG_BUNDLE_NAME = "MiniGameXXL";
n.MG_BRIDGE_KEY = "MGXXLBridge";
n.MG_BRIDGE_KEY = "MGXXLBridge";
n.MG_EXIT_UNLOCK_SECONDS = 180;
(function(t) {
t.b = "MG_EXIT";
})(c = n.$so || (n.$so = {}));
var a = function() {
function t() {}
t.cJo = function(t, e, n) {
this.cJn && this.cJp();
this.cJb = t;
this.cJk = n;
this.cJl = Date.now();
this.cJh = [];
this.cJi = 0;
var i = new cc.Node("MiniGameRoot");
i.zIndex = cc.macro.MAX_ZINDEX;
this.cJw(i);
i.on(cc.Node.EventType.TOUCH_START, function() {}, i);
e.addChild(i);
this.cJc = i;
this.cJd = this.cJv("scene", 0);
this.cJe = this.cJv("view", 1);
this.cJf = this.cJv("tip", 2);
this.cJn = !0;
};
t.cJp = function() {
this.cJF();
cc.audioEngine.stopAllEffects();
this.cJh = [];
this.cJi = 0;
if (cc.isValid(this.cJc)) {
this.cJc.removeFromParent(!1);
this.cJc.destroy();
}
this.cJc = null;
this.cJd = null;
this.cJe = null;
this.cJf = null;
this.cJb = null;
this.cJk = !1;
this.cJl = 0;
this.cJn = !1;
this.cJg.targetOff(this);
};
t.cJq = function() {
return this.cJn && cc.isValid(this.cJc);
};
t.cJr = function() {
return this.cJd;
};
t.cJs = function() {
return this.cJb;
};
t.cJt = function() {
return this.cJl ? (Date.now() - this.cJl) / 1e3 : 0;
};
t.cJu = function() {
return this.cJk && this.cJt() >= n.MG_EXIT_UNLOCK_SECONDS;
};
t.cJv = function(t, e) {
var n = new cc.Node(t);
n.zIndex = e;
this.cJw(n);
this.cJc.addChild(n);
return n;
};
t.cJw = function(t) {
t.setContentSize(cc.winSize.width, cc.winSize.height);
var e = t.addComponent(cc.Widget);
e.isAlignTop = e.isAlignBottom = e.isAlignLeft = e.isAlignRight = !0;
e.top = e.bottom = e.left = e.right = 0;
e.alignMode = cc.Widget.AlignMode.ON_WINDOW_RESIZE;
};
t.cJx = function(t, e, n) {
this.cJg.on(t, e, n);
};
t.cJy = function(t, e, n) {
this.cJg.off(t, e, n);
};
t.cJz = function(t) {
for (var e, n = [], i = 1; i < arguments.length; i++) n[i - 1] = arguments[i];
(e = this.cJg).emit.apply(e, r([ t ], n));
};
t.cJA = function() {
this.cJz(c.b);
};
t.cJB = function(t, e) {
var n = this;
return new Promise(function(i, o) {
n.cJb ? n.cJb.load(t, e, function(t, e) {
t ? o(t) : i(e);
}) : o(new Error("[MGXXL] bundle 未加载: " + t));
});
};
t.cJC = function(t, e) {
return i(this, void 0, Promise, function() {
var n, i;
return o(this,function AWAITF2086(o){switch (o.label) {
case 0:
if (!cc.isValid(t)) return [ 2 ];
o.label = 1;

case 1:
o.trys.push([ 1, 3, , 4 ]);
return [ 4, this.cJB(e, cc.SpriteFrame) ];

case 2:
n = o.sent();
cc.isValid(t) && (t.spriteFrame = n);
return [ 3, 4 ];

case 3:
i = o.sent();
this.cJR("加载图片失败", e, i);
return [ 3, 4 ];

case 4:
return [ 2 ];
}
});
});
};
t.cJD = function(t) {
return i(this, void 0, Promise, function() {
var e, n;
return o(this,function AWAITF2087(i){switch (i.label) {
case 0:
i.trys.push([ 0, 2, , 3 ]);
return [ 4, this.cJB("sounds/" + t, cc.AudioClip) ];

case 1:
e = i.sent();
this.cJn && cc.audioEngine.playEffect(e, !1);
return [ 3, 3 ];

case 2:
n = i.sent();
this.cJR("播放音效失败", t, n);
return [ 3, 3 ];

case 3:
return [ 2 ];
}
});
});
};
t.cJE = function(t) {
return i(this, void 0, Promise, function() {
var e, n;
return o(this,function AWAITF2088(i){switch (i.label) {
case 0:
i.trys.push([ 0, 2, , 3 ]);
return [ 4, this.cJB("sounds/" + t, cc.AudioClip) ];

case 1:
e = i.sent();
if (!this.cJn) return [ 2 ];
this.cJF();
this.cJj = cc.audioEngine.playMusic(e, !0);
return [ 3, 3 ];

case 2:
n = i.sent();
this.cJR("播放背景音乐失败", t, n);
return [ 3, 3 ];

case 3:
return [ 2 ];
}
});
});
};
t.cJF = function() {
if (-1 !== this.cJj) {
cc.audioEngine.stopMusic();
this.cJj = -1;
}
};
t.cJG = function(t, e) {
return i(this, void 0, Promise, function() {
var n, i, r, c;
return o(this,function AWAITF2089(o){switch (o.label) {
case 0:
n = null;
this.cJi++;
o.label = 1;

case 1:
o.trys.push([ 1, 3, 4, 5 ]);
return [ 4, this.cJB("prefabs/" + t, cc.Prefab) ];

case 2:
n = o.sent();
return [ 3, 5 ];

case 3:
i = o.sent();
this.cJR("加载界面失败", t, i);
return [ 2, null ];

case 4:
this.cJi = Math.max(0, this.cJi - 1);
return [ 7 ];

case 5:
if (!this.cJq() || !cc.isValid(this.cJe)) return [ 2, null ];
r = cc.instantiate(n);
c = r.getComponent(s.default);
if (!this.cJP(!!c, "界面根节点缺少 MGView 组件: " + t)) {
r.destroy();
return [ 2, null ];
}
c.ti = e;
c.tj = t;
r.zIndex = this.cJh.length;
this.cJh.push(c);
this.cJe.addChild(r);
return [ 2, c ];
}
});
});
};
t.cJH = function(t) {
var e = this.cJh.indexOf(t);
-1 !== e && this.cJh.splice(e, 1);
};
t.cJI = function() {
return this.cJh.length > 0 || this.cJi > 0;
};
t.cJJ = function(t) {
if (this.cJq() && cc.isValid(this.cJf)) {
var e = new cc.Node("tip"), n = e.addComponent(cc.Label);
n.string = t;
n.fontSize = 30;
n.lineHeight = 36;
var i = e.addComponent(cc.LabelOutline);
i.color = cc.Color.BLACK;
i.width = 2;
e.setPosition(0, 0);
this.cJf.addChild(e);
cc.tween(e).to(1, {
y: 120,
opacity: 0
}, {
easing: "sineOut"
}).call(function() {
cc.isValid(e) && e.destroy();
}).start();
} else this.cJQ("tip:", t);
};
t.cJK = function() {
if (this.cJm) return this.cJm;
var t = null;
try {
var e = cc.sys.localStorage.getItem("MGXXL_SAVE_V1");
e && (t = JSON.parse(e));
} catch (t) {
this.cJR("读取存档失败", t);
}
t && Array.isArray(t.clearRecords) || (t = {
clearRecords: [],
attemptDay: "",
attemptCount: 0
});
this.cJm = t;
return t;
};
t.cJL = function() {
if (this.cJm) try {
cc.sys.localStorage.setItem("MGXXL_SAVE_V1", JSON.stringify(this.cJm));
} catch (t) {
this.cJR("写入存档失败", t);
}
};
t.cJM = function(t) {
var e = this.cJK(), n = {
time: Math.max(0, Math.floor(t)),
ts: Date.now()
};
e.clearRecords.push(n);
e.clearRecords.sort(function(t, e) {
return t.time - e.time;
});
e.clearRecords.length > 20 && (e.clearRecords.length = 20);
this.cJL();
};
t.cJN = function() {
var t = this.cJK(), e = this.cJT(Date.now());
if (t.attemptDay !== e) {
t.attemptDay = e;
t.attemptCount = 0;
}
t.attemptCount += 1;
this.cJL();
return t.attemptCount;
};
t.cJO = function() {
var t = this.cJK();
return t.attemptDay === this.cJT(Date.now()) ? t.attemptCount : 0;
};
t.cJP = function(t, e) {
t || cc.warn("[MGXXL] 断言失败: " + e);
return !!t;
};
t.cJQ = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
cc.log.apply(cc, r([ "[MGXXL]" ], t));
};
t.cJR = function() {
for (var t = [], e = 0; e < arguments.length; e++) t[e] = arguments[e];
cc.warn.apply(cc, r([ "[MGXXL]" ], t));
};
t.cJS = function(t) {
var e = Math.max(0, Math.floor(t)), n = Math.floor(e / 60), i = e % 60;
return (n < 10 ? "0" + n : "" + n) + ":" + (i < 10 ? "0" + i : "" + i);
};
t.cJT = function(t) {
var e = new Date(t), n = e.getMonth() + 1, i = e.getDate();
return e.getFullYear() + "-" + (n < 10 ? "0" + n : n) + "-" + (i < 10 ? "0" + i : i);
};
t.cJU = function(t) {
var e = new Date(t), n = function(t) {
return t < 10 ? "0" + t : "" + t;
};
return n(e.getMonth() + 1) + "-" + n(e.getDate()) + " " + n(e.getHours()) + ":" + n(e.getMinutes());
};
t.cJV = function() {
return cc.sys.getSafeAreaRect();
};
t.cJb = null;
t.cJc = null;
t.cJd = null;
t.cJe = null;
t.cJf = null;
t.cJg = new cc.EventTarget();
t.cJh = [];
t.cJi = 0;
t.cJj = -1;
t.cJk = !1;
t.cJl = 0;
t.cJm = null;
t.cJn = !1;
return t;
}();
n.default = a;
var h = {
init: function(t, e, n) {
return a.cJo(t, e, n);
},
destroy: function() {
return a.cJp();
},
getSceneLayer: function() {
return a.cJr();
},
onExit: function(t, e) {
return a.cJx(c.b, t, e);
},
offExit: function(t, e) {
return a.cJy(c.b, t, e);
}
};
window[n.MG_BRIDGE_KEY] = h;
cc._RF.pop();
}, {
"./$UE": "$UE"
} ],
$UE: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ef5e663ejlPcoouOHFT6fE7", "$UE");
var i, o = this && this.__extends || (i = function(t, e) {
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
var r = t("./$UD"), c = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.ti = null;
e.tj = "";
return e;
}
e.prototype.onLoad = function() {
this.tk();
};
e.prototype.start = function() {
this.tl();
};
e.prototype.onDestroy = function() {
this.tm();
r.default.cJH(this);
};
e.prototype.tk = function() {};
e.prototype.tl = function() {};
e.prototype.tm = function() {};
e.prototype.tn = function() {
r.default.cJH(this);
cc.isValid(this.node) && this.node.destroy();
};
e.prototype.to = function(t) {
var e = cc.find(t, this.node);
r.default.cJP(!!e, this.tj + " 缺少节点 " + t);
return e;
};
e.prototype.tp = function(t) {
var e = this.to(t);
return e ? e.getComponent(cc.Label) : null;
};
e.prototype.tq = function(t) {
var e = this.to(t);
return e ? e.getComponent(cc.Sprite) : null;
};
e.prototype.tr = function(t, e) {
var n = this, i = "string" == typeof t ? this.to(t) : t;
i && i.on("click", function() {
n.isValid && e();
}, this);
};
e.prototype.ts = function(t, e) {
var n = this.tp(t);
n && (n.string = e);
};
e.prototype.tt = function(t, e) {
var n = this.to(t);
n && (n.active = e);
};
return e;
}(cc.Component);
n.default = c;
cc._RF.pop();
}, {
"./$UD": "$UD"
} ],
$UF: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ae8a3NKjJdM5I8GbftdrMW+", "$UF");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UE"), s = cc._decorator.ccclass, a = function(t) {
o(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.tk = function() {
var t = this;
this.tr("content/btnClose", function() {
return t.tn();
});
this.tr("mask", function() {
return t.tn();
});
};
e.prototype.tl = function() {
this.ts("content/title/lblTitle", "每日奖励");
var t = this.to("content/list"), e = t ? t.getComponent(cc.ScrollView) : null;
if (e && e.content) for (var n = 0, i = e.content.children; n < i.length; n++) i[n].active = !1;
};
return r([ s ], e);
}(c.default);
n.default = a;
cc._RF.pop();
}, {
"../core/$UE": "$UE"
} ],
$UG: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "fbc58HYVZFP8aanMNgzbB5g", "$UG");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UD"), s = t("../core/$UE"), a = cc._decorator.ccclass, h = function(t) {
o(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.tk = function() {
var t = this;
this.tr("content/btnReplay", function() {
return t.cJW();
});
this.tr("content/btnExit", function() {
return t.cJX();
});
this.tr("content/btnClose", function() {
return t.cJY();
});
this.tr("mask", function() {
return t.cJY();
});
};
e.prototype.tl = function() {
this.ts("content/title/lblTitle", "暂停");
this.ts("content/btnExit/lbl", "退出游戏");
this.tt("content/btnExit", c.default.cJu());
};
e.prototype.cJW = function() {
var t = this.ti;
this.tn();
if (t && t.game) {
t.game.resume();
t.game.start();
}
};
e.prototype.cJX = function() {
var t = this.ti;
this.tn();
t && t.game && t.game.exitGame();
};
e.prototype.cJY = function() {
var t = this.ti;
this.tn();
t && t.game && t.game.resume();
};
return r([ a ], e);
}(s.default);
n.default = h;
cc._RF.pop();
}, {
"../core/$UD": "$UD",
"../core/$UE": "$UE"
} ],
$UH: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "2c893YHhr1H3K9ZgBM174+G", "$UH");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UD"), s = t("../core/$UE"), a = cc._decorator, h = a.ccclass, u = a.property, f = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.btnClose = null;
e.btnGiveUp = null;
e.btnReward = null;
e.rewardIcon = null;
e.propIcon = null;
e.cJZ = !1;
return e;
}
e.prototype.tk = function() {
var t = this;
this.btnClose && this.tr(this.btnClose.node, function() {
return t.cKb();
});
this.btnGiveUp && this.tr(this.btnGiveUp.node, function() {
return t.cKb();
});
this.btnReward && this.tr(this.btnReward.node, function() {
return t.cKa();
});
this.rewardIcon && (this.rewardIcon.node.active = !1);
};
e.prototype.tl = function() {
var t = this.ti;
if (c.default.cJP(!!t && !!t.cfg, "MGXXLPropView 缺少参数")) {
this.ts("content/lblDesc", t.cfg.desc);
this.ts("content/title/lblTitle", t.cfg.name);
this.ts("content/btnReceive/lbl", "使用");
this.ts("content/btnGiveUp/lbl", "放弃");
this.propIcon && c.default.cJC(this.propIcon, t.cfg.icon);
} else this.tn();
};
e.prototype.tm = function() {
this.cKc(!1);
};
e.prototype.cKa = function() {
var t = this.ti;
if (!t || !t.checkCanUse || t.checkCanUse()) {
this.cKc(!0);
this.tn();
}
};
e.prototype.cKb = function() {
this.cKc(!1);
this.tn();
};
e.prototype.cKc = function(t) {
if (!this.cJZ) {
this.cJZ = !0;
c.default.cJH(this);
var e = this.ti;
e && e.ret && e.ret(t);
}
};
r([ u(cc.Button) ], e.prototype, "btnClose", void 0);
r([ u(cc.Button) ], e.prototype, "btnGiveUp", void 0);
r([ u(cc.Button) ], e.prototype, "btnReward", void 0);
r([ u(cc.Sprite) ], e.prototype, "rewardIcon", void 0);
r([ u(cc.Sprite) ], e.prototype, "propIcon", void 0);
return r([ h ], e);
}(s.default);
n.default = f;
cc._RF.pop();
}, {
"../core/$UD": "$UD",
"../core/$UE": "$UE"
} ],
$UI: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ee9fcwHGzdMfrKsXNY/RaMp", "$UI");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UD"), s = t("../core/$UE"), a = t("./$UK"), h = cc._decorator.ccclass, u = function(t) {
o(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.tk = function() {
var t = this;
this.tr("content/btnClose", function() {
return t.tn();
});
this.tr("mask", function() {
return t.tn();
});
};
e.prototype.tl = function() {
this.ts("content/title/lblTitle", "通关记录");
this.cKd();
this.cKe();
};
e.prototype.cKd = function() {
var t = this.to("content/list"), e = t ? t.getComponent(cc.ScrollView) : null;
if (e && e.content) {
var n = e.content, i = n.children[0];
if (c.default.cJP(!!i, "MGXXLRankingView 列表缺少模板")) {
i.active = !1;
for (var o = c.default.cJK().clearRecords, r = 0; r < o.length; r++) {
var s = n.children[r + 1];
if (!s) {
s = cc.instantiate(i);
n.addChild(s);
}
s.active = !0;
var h = s.getComponent(a.default);
h && h.cKh(o[r], r + 1);
}
for (r = o.length + 1; r < n.children.length; r++) n.children[r].active = !1;
e.scrollToTop(0);
}
}
};
e.prototype.cKe = function() {
var t = this.to("content/mine");
if (t) {
var e = c.default.cJK().clearRecords[0];
t.active = !!e;
if (e) {
var n = t.getComponent(a.default);
n && n.cKh(e, 1, !0);
}
}
};
return r([ h ], e);
}(s.default);
n.default = u;
cc._RF.pop();
}, {
"../core/$UD": "$UD",
"../core/$UE": "$UE",
"./$UK": "$UK"
} ],
$UJ: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "f29d6jLvZhE757lEiAktizK", "$UJ");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UD"), s = t("../core/$UE"), a = cc._decorator.ccclass, h = function(t) {
o(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
e.prototype.tk = function() {
var t = this;
this.tr("content/btns/btnReplay", function() {
return t.cKf();
});
this.tr("content/btns/btnExit", function() {
return t.cKg();
});
var e = cc.find("content/spine", this.node);
e && (e.active = !1);
};
e.prototype.tl = function() {
var t = this.ti;
if (c.default.cJP(!!t && !!t.game, "MGXXLResultView 缺少参数")) {
var e = !!t.result;
this.ts("content/lblTitle", e ? "挑战成功" : "挑战失败");
this.tt("content/imgLight", e);
this.tt("content/text1", !0);
this.ts("content/text1/lbl2", c.default.cJO() + "次");
this.tt("content/text2", e);
this.ts("content/text2/lbl2", c.default.cJS(t.game.time));
if (e) {
this.tt("content/btns/btnReplay", !1);
this.ts("content/btns/btnExit/lbl", c.default.cJu() ? "进入游戏" : "再来一次");
this.tt("content/btns/btnExit", !0);
c.default.cJD("win");
var n = cc.find("content/imgLight", this.node);
n && cc.tween(n).by(2, {
angle: -360
}).repeatForever().start();
} else {
this.tt("content/btns/btnReplay", !0);
this.ts("content/btns/btnExit/lbl", "退出游戏");
this.tt("content/btns/btnExit", c.default.cJu());
c.default.cJD("fail");
}
} else this.tn();
};
e.prototype.cKf = function() {
var t = this.ti;
this.tn();
t && t.game && t.game.start();
};
e.prototype.cKg = function() {
var t = this.ti;
this.tn();
t && t.game && (c.default.cJu() ? t.game.exitGame() : t.game.start());
};
return r([ a ], e);
}(s.default);
n.default = h;
cc._RF.pop();
}, {
"../core/$UD": "$UD",
"../core/$UE": "$UE"
} ],
$UK: [ function(t, e, n) {
"use strict";
cc._RF.push(e, "ae3a7NBl81HfIUKypkGry0o", "$UK");
var i, o = this && this.__extends || (i = function(t, e) {
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
}), r = this && this.__decorate || function(t, e, n, i) {
var o, r = arguments.length, c = r < 3 ? e : null === i ? i = Object.getOwnPropertyDescriptor(e, n) : i;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) c = Reflect.decorate(t, e, n, i); else for (var s = t.length - 1; s >= 0; s--) (o = t[s]) && (c = (r < 3 ? o(c) : r > 3 ? o(e, n, c) : o(e, n)) || c);
return r > 3 && c && Object.defineProperty(e, n, c), c;
};
Object.defineProperty(n, "__esModule", {
value: !0
});
var c = t("../core/$UD"), s = cc._decorator, a = s.ccclass, h = s.property, u = function(t) {
o(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.icons = [];
return e;
}
e.prototype.onLoad = function() {
for (var t = 0, e = [ "back", "avatar", "frame" ]; t < e.length; t++) {
var n = e[t], i = this.node.getChildByName(n);
i && (i.active = !1);
}
};
e.prototype.cKh = function(t, e, n) {
void 0 === n && (n = !1);
if (t) {
var i = this.node.getChildByName("order0"), o = this.node.getChildByName("order1"), r = e >= 1 && e <= this.icons.length && !!this.icons[e - 1];
if (i) {
i.active = r;
r && (i.getComponent(cc.Sprite).spriteFrame = this.icons[e - 1]);
}
if (o) {
o.active = !r;
r || (o.getComponent(cc.Label).string = "" + e);
}
this.cKi("nickName", n ? "最佳记录" : c.default.cJU(t.ts));
this.cKi("lblTimes", n ? c.default.cJU(t.ts) : "");
this.cKi("lblTime", c.default.cJS(t.time));
}
};
e.prototype.cKi = function(t, e) {
var n = this.node.getChildByName(t), i = n ? n.getComponent(cc.Label) : null;
i && (i.string = e);
};
r([ h(cc.SpriteFrame) ], e.prototype, "icons", void 0);
return r([ a ], e);
}(cc.Component);
n.default = u;
cc._RF.pop();
}, {
"../core/$UD": "$UD"
} ]
}, {}, [ "$KN", "$KO", "$KP", "$KQ", "$KR", "$KS", "$UD", "$UE", "$UF", "$UG", "$UH", "$UI", "$UJ", "$UK" ]);