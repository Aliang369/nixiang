(function(global, undefined) {
"use strict";
(function(e, t, r) {
var n = global.protobuf = function r(n) {
var i = t[n];
i || e[n][0].call(i = t[n] = {
exports: {}
}, r, i, i.exports);
return i.exports;
}(r[0]);
"function" == typeof define && define.amd && define([ "long" ], function(e) {
if (e && e.isLong) {
n.util.Long = e;
n.configure();
}
return n;
});
"object" == typeof module && module && module.exports && (module.exports = n);
})({
1: [ function(e, t, r) {
t.exports = function(e, t) {
var r = new Array(arguments.length - 1), n = 0, i = 2, o = !0;
for (;i < arguments.length; ) r[n++] = arguments[i++];
return new Promise(function(i, a) {
r[n] = function(e) {
if (o) {
o = !1;
if (e) a(e); else {
for (var t = new Array(arguments.length - 1), r = 0; r < t.length; ) t[r++] = arguments[r];
i.apply(null, t);
}
}
};
try {
e.apply(t || null, r);
} catch (e) {
if (o) {
o = !1;
a(e);
}
}
});
};
}, {} ],
2: [ function(e, t, r) {
var n = r;
n.length = function(e) {
var t = e.length;
if (!t) return 0;
for (var r = 0; --t % 4 > 1 && "=" === e.charAt(t); ) ++r;
return Math.ceil(3 * e.length) / 4 - r;
};
for (var i = new Array(64), o = new Array(123), a = 0; a < 64; ) o[i[a] = a < 26 ? a + 65 : a < 52 ? a + 71 : a < 62 ? a - 4 : a - 59 | 43] = a++;
n.encode = function(e, t, r) {
for (var n, o = null, a = [], l = 0, s = 0; t < r; ) {
var u = e[t++];
switch (s) {
case 0:
a[l++] = i[u >> 2];
n = (3 & u) << 4;
s = 1;
break;

case 1:
a[l++] = i[n | u >> 4];
n = (15 & u) << 2;
s = 2;
break;

case 2:
a[l++] = i[n | u >> 6];
a[l++] = i[63 & u];
s = 0;
}
if (l > 8191) {
(o || (o = [])).push(String.fromCharCode.apply(String, a));
l = 0;
}
}
if (s) {
a[l++] = i[n];
a[l++] = 61;
1 === s && (a[l++] = 61);
}
if (o) {
l && o.push(String.fromCharCode.apply(String, a.slice(0, l)));
return o.join("");
}
return String.fromCharCode.apply(String, a.slice(0, l));
};
n.decode = function(e, t, r) {
for (var n, i = r, a = 0, l = 0; l < e.length; ) {
var s = e.charCodeAt(l++);
if (61 === s && a > 1) break;
if ((s = o[s]) === undefined) throw Error("invalid encoding");
switch (a) {
case 0:
n = s;
a = 1;
break;

case 1:
t[r++] = n << 2 | (48 & s) >> 4;
n = s;
a = 2;
break;

case 2:
t[r++] = (15 & n) << 4 | (60 & s) >> 2;
n = s;
a = 3;
break;

case 3:
t[r++] = (3 & n) << 6 | s;
a = 0;
}
}
if (1 === a) throw Error("invalid encoding");
return r - i;
};
n.test = function(e) {
return /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(e);
};
}, {} ],
3: [ function(e, t, r) {
t.exports = n;
function n(e, t) {
if ("string" == typeof e) {
t = e;
e = undefined;
}
var r = [];
function i(e) {
if ("string" != typeof e) {
var t = o();
n.verbose && console.log("codegen: " + t);
t = "return " + t;
if (e) {
for (var a = Object.keys(e), l = new Array(a.length + 1), s = new Array(a.length), u = 0; u < a.length; ) {
l[u] = a[u];
s[u] = e[a[u++]];
}
l[u] = t;
return Function.apply(null, l).apply(null, s);
}
return Function(t)();
}
for (var c = new Array(arguments.length - 1), d = 0; d < c.length; ) c[d] = arguments[++d];
d = 0;
e = e.replace(/%([%dfijs])/g, function(e, t) {
var r = c[d++];
switch (t) {
case "d":
case "f":
return String(Number(r));

case "i":
return String(Math.floor(r));

case "j":
return JSON.stringify(r);

case "s":
return String(r);
}
return "%";
});
if (d !== c.length) throw Error("parameter count mismatch");
r.push(e);
return i;
}
function o(n) {
return "function " + (n || t || "") + "(" + (e && e.join(",") || "") + "){\n  " + r.join("\n  ") + "\n}";
}
i.toString = o;
return i;
}
n.verbose = !1;
}, {} ],
4: [ function(e, t, r) {
t.exports = n;
function n() {
this._listeners = {};
}
n.prototype.on = function(e, t, r) {
(this._listeners[e] || (this._listeners[e] = [])).push({
fn: t,
ctx: r || this
});
return this;
};
n.prototype.off = function(e, t) {
if (e === undefined) this._listeners = {}; else if (t === undefined) this._listeners[e] = []; else for (var r = this._listeners[e], n = 0; n < r.length; ) r[n].fn === t ? r.splice(n, 1) : ++n;
return this;
};
n.prototype.emit = function(e) {
var t = this._listeners[e];
if (t) {
for (var r = [], n = 1; n < arguments.length; ) r.push(arguments[n++]);
for (n = 0; n < t.length; ) t[n].fn.apply(t[n++].ctx, r);
}
return this;
};
}, {} ],
5: [ function(e, t, r) {
t.exports = o;
var n = e(1), i = e(7)("fs");
function o(e, t, r) {
if ("function" == typeof t) {
r = t;
t = {};
} else t || (t = {});
return r ? !t.xhr && i && i.readFile ? i.readFile(e, function(n, i) {
return n && "undefined" != typeof XMLHttpRequest ? o.xhr(e, t, r) : n ? r(n) : r(null, t.binary ? i : i.toString("utf8"));
}) : o.xhr(e, t, r) : n(o, this, e, t);
}
o.xhr = function(e, t, r) {
var n = new XMLHttpRequest();
n.onreadystatechange = function() {
if (4 !== n.readyState) return undefined;
if (0 !== n.status && 200 !== n.status) return r(Error("status " + n.status));
if (t.binary) {
var e = n.response;
if (!e) {
e = [];
for (var i = 0; i < n.responseText.length; ++i) e.push(255 & n.responseText.charCodeAt(i));
}
return r(null, "undefined" != typeof Uint8Array ? new Uint8Array(e) : e);
}
return r(null, n.responseText);
};
if (t.binary) {
"overrideMimeType" in n && n.overrideMimeType("text/plain; charset=x-user-defined");
n.responseType = "arraybuffer";
}
n.open("GET", e);
n.send();
};
}, {
1: 1,
7: 7
} ],
6: [ function(e, t, r) {
t.exports = n(n);
function n(e) {
"undefined" != typeof Float32Array ? function() {
var t = new Float32Array([ -0 ]), r = new Uint8Array(t.buffer), n = 128 === r[3];
function i(e, n, i) {
t[0] = e;
n[i] = r[0];
n[i + 1] = r[1];
n[i + 2] = r[2];
n[i + 3] = r[3];
}
function o(e, n, i) {
t[0] = e;
n[i] = r[3];
n[i + 1] = r[2];
n[i + 2] = r[1];
n[i + 3] = r[0];
}
e.writeFloatLE = n ? i : o;
e.writeFloatBE = n ? o : i;
function a(e, n) {
r[0] = e[n];
r[1] = e[n + 1];
r[2] = e[n + 2];
r[3] = e[n + 3];
return t[0];
}
function l(e, n) {
r[3] = e[n];
r[2] = e[n + 1];
r[1] = e[n + 2];
r[0] = e[n + 3];
return t[0];
}
e.readFloatLE = n ? a : l;
e.readFloatBE = n ? l : a;
}() : function() {
function t(e, t, r, n) {
var i = t < 0 ? 1 : 0;
i && (t = -t);
if (0 === t) e(1 / t > 0 ? 0 : 2147483648, r, n); else if (isNaN(t)) e(2143289344, r, n); else if (t > 3.4028234663852886e38) e((i << 31 | 2139095040) >>> 0, r, n); else if (t < 1.1754943508222875e-38) e((i << 31 | Math.round(t / 1.401298464324817e-45)) >>> 0, r, n); else {
var o = Math.floor(Math.log(t) / Math.LN2);
e((i << 31 | o + 127 << 23 | 8388607 & Math.round(t * Math.pow(2, -o) * 8388608)) >>> 0, r, n);
}
}
e.writeFloatLE = t.bind(null, i);
e.writeFloatBE = t.bind(null, o);
function r(e, t, r) {
var n = e(t, r), i = 2 * (n >> 31) + 1, o = n >>> 23 & 255, a = 8388607 & n;
return 255 === o ? a ? NaN : Infinity * i : 0 === o ? 1.401298464324817e-45 * i * a : i * Math.pow(2, o - 150) * (a + 8388608);
}
e.readFloatLE = r.bind(null, a);
e.readFloatBE = r.bind(null, l);
}();
"undefined" != typeof Float64Array ? function() {
var t = new Float64Array([ -0 ]), r = new Uint8Array(t.buffer), n = 128 === r[7];
function i(e, n, i) {
t[0] = e;
n[i] = r[0];
n[i + 1] = r[1];
n[i + 2] = r[2];
n[i + 3] = r[3];
n[i + 4] = r[4];
n[i + 5] = r[5];
n[i + 6] = r[6];
n[i + 7] = r[7];
}
function o(e, n, i) {
t[0] = e;
n[i] = r[7];
n[i + 1] = r[6];
n[i + 2] = r[5];
n[i + 3] = r[4];
n[i + 4] = r[3];
n[i + 5] = r[2];
n[i + 6] = r[1];
n[i + 7] = r[0];
}
e.writeDoubleLE = n ? i : o;
e.writeDoubleBE = n ? o : i;
function a(e, n) {
r[0] = e[n];
r[1] = e[n + 1];
r[2] = e[n + 2];
r[3] = e[n + 3];
r[4] = e[n + 4];
r[5] = e[n + 5];
r[6] = e[n + 6];
r[7] = e[n + 7];
return t[0];
}
function l(e, n) {
r[7] = e[n];
r[6] = e[n + 1];
r[5] = e[n + 2];
r[4] = e[n + 3];
r[3] = e[n + 4];
r[2] = e[n + 5];
r[1] = e[n + 6];
r[0] = e[n + 7];
return t[0];
}
e.readDoubleLE = n ? a : l;
e.readDoubleBE = n ? l : a;
}() : function() {
function t(e, t, r, n, i, o) {
var a = n < 0 ? 1 : 0;
a && (n = -n);
if (0 === n) {
e(0, i, o + t);
e(1 / n > 0 ? 0 : 2147483648, i, o + r);
} else if (isNaN(n)) {
e(0, i, o + t);
e(2146959360, i, o + r);
} else if (n > 1.7976931348623157e308) {
e(0, i, o + t);
e((a << 31 | 2146435072) >>> 0, i, o + r);
} else {
var l;
if (n < 2.2250738585072014e-308) {
e((l = n / 5e-324) >>> 0, i, o + t);
e((a << 31 | l / 4294967296) >>> 0, i, o + r);
} else {
var s = Math.floor(Math.log(n) / Math.LN2);
1024 === s && (s = 1023);
e(4503599627370496 * (l = n * Math.pow(2, -s)) >>> 0, i, o + t);
e((a << 31 | s + 1023 << 20 | 1048576 * l & 1048575) >>> 0, i, o + r);
}
}
}
e.writeDoubleLE = t.bind(null, i, 0, 4);
e.writeDoubleBE = t.bind(null, o, 4, 0);
function r(e, t, r, n, i) {
var o = e(n, i + t), a = e(n, i + r), l = 2 * (a >> 31) + 1, s = a >>> 20 & 2047, u = 4294967296 * (1048575 & a) + o;
return 2047 === s ? u ? NaN : Infinity * l : 0 === s ? 5e-324 * l * u : l * Math.pow(2, s - 1075) * (u + 4503599627370496);
}
e.readDoubleLE = r.bind(null, a, 0, 4);
e.readDoubleBE = r.bind(null, l, 4, 0);
}();
return e;
}
function i(e, t, r) {
t[r] = 255 & e;
t[r + 1] = e >>> 8 & 255;
t[r + 2] = e >>> 16 & 255;
t[r + 3] = e >>> 24;
}
function o(e, t, r) {
t[r] = e >>> 24;
t[r + 1] = e >>> 16 & 255;
t[r + 2] = e >>> 8 & 255;
t[r + 3] = 255 & e;
}
function a(e, t) {
return (e[t] | e[t + 1] << 8 | e[t + 2] << 16 | e[t + 3] << 24) >>> 0;
}
function l(e, t) {
return (e[t] << 24 | e[t + 1] << 16 | e[t + 2] << 8 | e[t + 3]) >>> 0;
}
}, {} ],
7: [ function(require, module, exports) {
module.exports = inquire;
function inquire(moduleName) {
try {
var mod = eval("quire".replace(/^/, "re"))(moduleName);
if (mod && (mod.length || Object.keys(mod).length)) return mod;
} catch (e) {}
return null;
}
}, {} ],
8: [ function(e, t, r) {
var n = r, i = n.isAbsolute = function(e) {
return /^(?:\/|\w+:)/.test(e);
}, o = n.normalize = function(e) {
var t = (e = e.replace(/\\/g, "/").replace(/\/{2,}/g, "/")).split("/"), r = i(e), n = "";
r && (n = t.shift() + "/");
for (var o = 0; o < t.length; ) ".." === t[o] ? o > 0 && ".." !== t[o - 1] ? t.splice(--o, 2) : r ? t.splice(o, 1) : ++o : "." === t[o] ? t.splice(o, 1) : ++o;
return n + t.join("/");
};
n.resolve = function(e, t, r) {
r || (t = o(t));
if (i(t)) return t;
r || (e = o(e));
return (e = e.replace(/(?:\/|^)[^/]+$/, "")).length ? o(e + "/" + t) : t;
};
}, {} ],
9: [ function(e, t, r) {
t.exports = function(e, t, r) {
var n = r || 8192, i = n >>> 1, o = null, a = n;
return function(r) {
if (r < 1 || r > i) return e(r);
if (a + r > n) {
o = e(n);
a = 0;
}
var l = t.call(o, a, a += r);
7 & a && (a = 1 + (7 | a));
return l;
};
};
}, {} ],
10: [ function(e, t, r) {
var n = r;
n.length = function(e) {
for (var t = 0, r = 0, n = 0; n < e.length; ++n) if ((r = e.charCodeAt(n)) < 128) t += 1; else if (r < 2048) t += 2; else if (55296 == (64512 & r) && 56320 == (64512 & e.charCodeAt(n + 1))) {
++n;
t += 4;
} else t += 3;
return t;
};
n.read = function(e, t, r) {
if (r - t < 1) return "";
for (var n, i = null, o = [], a = 0; t < r; ) {
if ((n = e[t++]) < 128) o[a++] = n; else if (n > 191 && n < 224) o[a++] = (31 & n) << 6 | 63 & e[t++]; else if (n > 239 && n < 365) {
n = ((7 & n) << 18 | (63 & e[t++]) << 12 | (63 & e[t++]) << 6 | 63 & e[t++]) - 65536;
o[a++] = 55296 + (n >> 10);
o[a++] = 56320 + (1023 & n);
} else o[a++] = (15 & n) << 12 | (63 & e[t++]) << 6 | 63 & e[t++];
if (a > 8191) {
(i || (i = [])).push(String.fromCharCode.apply(String, o));
a = 0;
}
}
if (i) {
a && i.push(String.fromCharCode.apply(String, o.slice(0, a)));
return i.join("");
}
return String.fromCharCode.apply(String, o.slice(0, a));
};
n.write = function(e, t, r) {
for (var n, i, o = r, a = 0; a < e.length; ++a) if ((n = e.charCodeAt(a)) < 128) t[r++] = n; else if (n < 2048) {
t[r++] = n >> 6 | 192;
t[r++] = 63 & n | 128;
} else if (55296 == (64512 & n) && 56320 == (64512 & (i = e.charCodeAt(a + 1)))) {
n = 65536 + ((1023 & n) << 10) + (1023 & i);
++a;
t[r++] = n >> 18 | 240;
t[r++] = n >> 12 & 63 | 128;
t[r++] = n >> 6 & 63 | 128;
t[r++] = 63 & n | 128;
} else {
t[r++] = n >> 12 | 224;
t[r++] = n >> 6 & 63 | 128;
t[r++] = 63 & n | 128;
}
return r - o;
};
}, {} ],
11: [ function(e, t, r) {
t.exports = o;
var n, i = /\/|\./;
function o(e, t) {
if (!i.test(e)) {
e = "google/protobuf/" + e + ".proto";
t = {
nested: {
google: {
nested: {
protobuf: {
nested: t
}
}
}
}
};
}
o[e] = t;
}
o("any", {
Any: {
fields: {
type_url: {
type: "string",
id: 1
},
value: {
type: "bytes",
id: 2
}
}
}
});
o("duration", {
Duration: n = {
fields: {
seconds: {
type: "int64",
id: 1
},
nanos: {
type: "int32",
id: 2
}
}
}
});
o("timestamp", {
Timestamp: n
});
o("empty", {
Empty: {
fields: {}
}
});
o("struct", {
Struct: {
fields: {
fields: {
keyType: "string",
type: "Value",
id: 1
}
}
},
Value: {
oneofs: {
kind: {
oneof: [ "nullValue", "numberValue", "stringValue", "boolValue", "structValue", "listValue" ]
}
},
fields: {
nullValue: {
type: "NullValue",
id: 1
},
numberValue: {
type: "double",
id: 2
},
stringValue: {
type: "string",
id: 3
},
boolValue: {
type: "bool",
id: 4
},
structValue: {
type: "Struct",
id: 5
},
listValue: {
type: "ListValue",
id: 6
}
}
},
NullValue: {
values: {
NULL_VALUE: 0
}
},
ListValue: {
fields: {
values: {
rule: "repeated",
type: "Value",
id: 1
}
}
}
});
o("wrappers", {
DoubleValue: {
fields: {
value: {
type: "double",
id: 1
}
}
},
FloatValue: {
fields: {
value: {
type: "float",
id: 1
}
}
},
Int64Value: {
fields: {
value: {
type: "int64",
id: 1
}
}
},
UInt64Value: {
fields: {
value: {
type: "uint64",
id: 1
}
}
},
Int32Value: {
fields: {
value: {
type: "int32",
id: 1
}
}
},
UInt32Value: {
fields: {
value: {
type: "uint32",
id: 1
}
}
},
BoolValue: {
fields: {
value: {
type: "bool",
id: 1
}
}
},
StringValue: {
fields: {
value: {
type: "string",
id: 1
}
}
},
BytesValue: {
fields: {
value: {
type: "bytes",
id: 1
}
}
}
});
o.get = function(e) {
return o[e] || null;
};
}, {} ],
12: [ function(e, t, r) {
var n = r, i = e(15), o = e(37);
function a(e, t, r, n) {
if (t.resolvedType) if (t.resolvedType instanceof i) {
e("switch(d%s){", n);
for (var o = t.resolvedType.values, a = Object.keys(o), l = 0; l < a.length; ++l) {
t.repeated && o[a[l]] === t.typeDefault && e("default:");
e("case%j:", a[l])("case %i:", o[a[l]])("m%s=%j", n, o[a[l]])("break");
}
e("}");
} else e('if(typeof d%s!=="object")', n)("throw TypeError(%j)", t.fullName + ": object expected")("m%s=types[%i].fromObject(d%s)", n, r, n); else {
var s = !1;
switch (t.type) {
case "double":
case "float":
e("m%s=Number(d%s)", n, n);
break;

case "uint32":
case "fixed32":
e("m%s=d%s>>>0", n, n);
break;

case "int32":
case "sint32":
case "sfixed32":
e("m%s=d%s|0", n, n);
break;

case "uint64":
s = !0;

case "int64":
case "sint64":
case "fixed64":
case "sfixed64":
e("if(util.Long)")("(m%s=util.Long.fromValue(d%s)).unsigned=%j", n, n, s)('else if(typeof d%s==="string")', n)("m%s=parseInt(d%s,10)", n, n)('else if(typeof d%s==="number")', n)("m%s=d%s", n, n)('else if(typeof d%s==="object")', n)("m%s=new util.LongBits(d%s.low>>>0,d%s.high>>>0).toNumber(%s)", n, n, n, s ? "true" : "");
break;

case "bytes":
e('if(typeof d%s==="string")', n)("util.base64.decode(d%s,m%s=util.newBuffer(util.base64.length(d%s)),0)", n, n, n)("else if(d%s.length)", n)("m%s=d%s", n, n);
break;

case "string":
e("m%s=String(d%s)", n, n);
break;

case "bool":
e("m%s=Boolean(d%s)", n, n);
}
}
return e;
}
n.fromObject = function(e) {
var t = e.fieldsArray, r = o.codegen([ "d" ], e.name + "$fromObject")("if(d instanceof this.ctor)")("return d");
if (!t.length) return r("return new this.ctor");
r("var m=new this.ctor");
for (var n = 0; n < t.length; ++n) {
var l = t[n].resolve(), s = o.safeProp(l.name);
if (l.map) {
r("if(d%s){", s)('if(typeof d%s!=="object")', s)("throw TypeError(%j)", l.fullName + ": object expected")("m%s={}", s)("for(var ks=Object.keys(d%s),i=0;i<ks.length;++i){", s);
a(r, l, n, s + "[ks[i]]")("}")("}");
} else if (l.repeated) {
r("if(d%s){", s)("if(!Array.isArray(d%s))", s)("throw TypeError(%j)", l.fullName + ": array expected")("m%s=[]", s)("for(var i=0;i<d%s.length;++i){", s);
a(r, l, n, s + "[i]")("}")("}");
} else {
l.resolvedType instanceof i || r("if(d%s!=null){", s);
a(r, l, n, s);
l.resolvedType instanceof i || r("}");
}
}
return r("return m");
};
function l(e, t, r, n) {
if (t.resolvedType) t.resolvedType instanceof i ? e("d%s=o.enums===String?types[%i].values[m%s]:m%s", n, r, n, n) : e("d%s=types[%i].toObject(m%s,o)", n, r, n); else {
var o = !1;
switch (t.type) {
case "double":
case "float":
e("d%s=o.json&&!isFinite(m%s)?String(m%s):m%s", n, n, n, n);
break;

case "uint64":
o = !0;

case "int64":
case "sint64":
case "fixed64":
case "sfixed64":
e('if(typeof m%s==="number")', n)("d%s=o.longs===String?String(m%s):m%s", n, n, n)("else")("d%s=o.longs===String?util.Long.prototype.toString.call(m%s):o.longs===Number?new util.LongBits(m%s.low>>>0,m%s.high>>>0).toNumber(%s):m%s", n, n, n, n, o ? "true" : "", n);
break;

case "bytes":
e("d%s=o.bytes===String?util.base64.encode(m%s,0,m%s.length):o.bytes===Array?Array.prototype.slice.call(m%s):m%s", n, n, n, n, n);
break;

default:
e("d%s=m%s", n, n);
}
}
return e;
}
n.toObject = function(e) {
var t = e.fieldsArray.slice().sort(o.compareFieldsById);
if (!t.length) return o.codegen()("return {}");
for (var r = o.codegen([ "m", "o" ], e.name + "$toObject")("if(!o)")("o={}")("var d={}"), n = [], a = [], s = [], u = 0; u < t.length; ++u) t[u].partOf || (t[u].resolve().repeated ? n : t[u].map ? a : s).push(t[u]);
if (n.length) {
r("if(o.arrays||o.defaults){");
for (u = 0; u < n.length; ++u) r("d%s=[]", o.safeProp(n[u].name));
r("}");
}
if (a.length) {
r("if(o.objects||o.defaults){");
for (u = 0; u < a.length; ++u) r("d%s={}", o.safeProp(a[u].name));
r("}");
}
if (s.length) {
r("if(o.defaults){");
for (u = 0; u < s.length; ++u) {
var c = s[u], d = o.safeProp(c.name);
c.resolvedType instanceof i ? r("d%s=o.enums===String?%j:%j", d, c.resolvedType.valuesById[c.typeDefault], c.typeDefault) : c.long ? r("if(util.Long){")("var n=new util.Long(%i,%i,%j)", c.typeDefault.low, c.typeDefault.high, c.typeDefault.unsigned)("d%s=o.longs===String?n.toString():o.longs===Number?n.toNumber():n", d)("}else")("d%s=o.longs===String?%j:%i", d, c.typeDefault.toString(), c.typeDefault.toNumber()) : c.bytes ? r("d%s=o.bytes===String?%j:%s", d, String.fromCharCode.apply(String, c.typeDefault), "[" + Array.prototype.slice.call(c.typeDefault).join(",") + "]") : r("d%s=%j", d, c.typeDefault);
}
r("}");
}
var p = !1;
for (u = 0; u < t.length; ++u) {
c = t[u];
var f = e._fieldsArray.indexOf(c);
d = o.safeProp(c.name);
if (c.map) {
if (!p) {
p = !0;
r("var ks2");
}
r("if(m%s&&(ks2=Object.keys(m%s)).length){", d, d)("d%s={}", d)("for(var j=0;j<ks2.length;++j){");
l(r, c, f, d + "[ks2[j]]")("}");
} else if (c.repeated) {
r("if(m%s&&m%s.length){", d, d)("d%s=[]", d)("for(var j=0;j<m%s.length;++j){", d);
l(r, c, f, d + "[j]")("}");
} else {
r("if(m%s!=null&&m.hasOwnProperty(%j)){", d, c.name);
l(r, c, f, d);
c.partOf && r("if(o.oneofs)")("d%s=%j", o.safeProp(c.partOf.name), c.name);
}
r("}");
}
return r("return d");
};
}, {
15: 15,
37: 37
} ],
13: [ function(e, t, r) {
t.exports = function(e) {
var t = o.codegen([ "r", "l" ], e.name + "$decode")("if(!(r instanceof Reader))")("r=Reader.create(r)")("var c=l===undefined?r.len:r.pos+l,m=new this.ctor" + (e.fieldsArray.filter(function(e) {
return e.map;
}).length ? ",k" : ""))("while(r.pos<c){")("var t=r.uint32()");
e.group && t("if((t&7)===4)")("break");
t("switch(t>>>3){");
for (var r = 0; r < e.fieldsArray.length; ++r) {
var l = e._fieldsArray[r].resolve(), s = l.resolvedType instanceof n ? "int32" : l.type, u = "m" + o.safeProp(l.name);
t("case %i:", l.id);
if (l.map) {
t("r.skip().pos++")("if(%s===util.emptyObject)", u)("%s={}", u)("k=r.%s()", l.keyType)("r.pos++");
i.long[l.keyType] !== undefined ? i.basic[s] === undefined ? t('%s[typeof k==="object"?util.longToHash(k):k]=types[%i].decode(r,r.uint32())', u, r) : t('%s[typeof k==="object"?util.longToHash(k):k]=r.%s()', u, s) : i.basic[s] === undefined ? t("%s[k]=types[%i].decode(r,r.uint32())", u, r) : t("%s[k]=r.%s()", u, s);
} else if (l.repeated) {
t("if(!(%s&&%s.length))", u, u)("%s=[]", u);
i.packed[s] !== undefined && t("if((t&7)===2){")("var c2=r.uint32()+r.pos")("while(r.pos<c2)")("%s.push(r.%s())", u, s)("}else");
i.basic[s] === undefined ? t(l.resolvedType.group ? "%s.push(types[%i].decode(r))" : "%s.push(types[%i].decode(r,r.uint32()))", u, r) : t("%s.push(r.%s())", u, s);
} else i.basic[s] === undefined ? t(l.resolvedType.group ? "%s=types[%i].decode(r)" : "%s=types[%i].decode(r,r.uint32())", u, r) : t("%s=r.%s()", u, s);
t("break");
}
t("default:")("r.skipType(t&7)")("break")("}")("}");
for (r = 0; r < e._fieldsArray.length; ++r) {
var c = e._fieldsArray[r];
c.required && t("if(!m.hasOwnProperty(%j))", c.name)("throw util.ProtocolError(%j,{instance:m})", a(c));
}
return t("return m");
};
var n = e(15), i = e(36), o = e(37);
function a(e) {
return "missing required '" + e.name + "'";
}
}, {
15: 15,
36: 36,
37: 37
} ],
14: [ function(e, t, r) {
t.exports = function(e) {
for (var t, r = o.codegen([ "m", "w" ], e.name + "$encode")("if(!w)")("w=Writer.create()"), l = e.fieldsArray.slice().sort(o.compareFieldsById), s = 0; s < l.length; ++s) {
var u = l[s].resolve(), c = e._fieldsArray.indexOf(u), d = u.resolvedType instanceof n ? "int32" : u.type, p = i.basic[d];
t = "m" + o.safeProp(u.name);
if (u.map) {
r("if(%s!=null&&m.hasOwnProperty(%j)){", t, u.name)("for(var ks=Object.keys(%s),i=0;i<ks.length;++i){", t)("w.uint32(%i).fork().uint32(%i).%s(ks[i])", (u.id << 3 | 2) >>> 0, 8 | i.mapKey[u.keyType], u.keyType);
p === undefined ? r("types[%i].encode(%s[ks[i]],w.uint32(18).fork()).ldelim().ldelim()", c, t) : r(".uint32(%i).%s(%s[ks[i]]).ldelim()", 16 | p, d, t);
r("}")("}");
} else if (u.repeated) {
r("if(%s!=null&&%s.length){", t, t);
if (u.packed && i.packed[d] !== undefined) r("w.uint32(%i).fork()", (u.id << 3 | 2) >>> 0)("for(var i=0;i<%s.length;++i)", t)("w.%s(%s[i])", d, t)("w.ldelim()"); else {
r("for(var i=0;i<%s.length;++i)", t);
p === undefined ? a(r, u, c, t + "[i]") : r("w.uint32(%i).%s(%s[i])", (u.id << 3 | p) >>> 0, d, t);
}
r("}");
} else {
u.optional && r("if(%s!=null&&m.hasOwnProperty(%j))", t, u.name);
p === undefined ? a(r, u, c, t) : r("w.uint32(%i).%s(%s)", (u.id << 3 | p) >>> 0, d, t);
}
}
return r("return w");
};
var n = e(15), i = e(36), o = e(37);
function a(e, t, r, n) {
return t.resolvedType.group ? e("types[%i].encode(%s,w.uint32(%i)).uint32(%i)", r, n, (t.id << 3 | 3) >>> 0, (t.id << 3 | 4) >>> 0) : e("types[%i].encode(%s,w.uint32(%i).fork()).ldelim()", r, n, (t.id << 3 | 2) >>> 0);
}
}, {
15: 15,
36: 36,
37: 37
} ],
15: [ function(e, t, r) {
t.exports = a;
var n = e(24);
((a.prototype = Object.create(n.prototype)).constructor = a).className = "Enum";
var i = e(23), o = e(37);
function a(e, t, r) {
n.call(this, e, r);
if (t && "object" != typeof t) throw TypeError("values must be an object");
this.valuesById = {};
this.values = Object.create(this.valuesById);
this.comments = {};
this.reserved = undefined;
if (t) for (var i = Object.keys(t), o = 0; o < i.length; ++o) "number" == typeof t[i[o]] && (this.valuesById[this.values[i[o]] = t[i[o]]] = i[o]);
}
a.fromJSON = function(e, t) {
var r = new a(e, t.values, t.options);
r.reserved = t.reserved;
return r;
};
a.prototype.toJSON = function() {
return o.toObject([ "options", this.options, "values", this.values, "reserved", this.reserved && this.reserved.length ? this.reserved : undefined ]);
};
a.prototype.add = function(e, t, r) {
if (!o.isString(e)) throw TypeError("name must be a string");
if (!o.isInteger(t)) throw TypeError("id must be an integer");
if (this.values[e] !== undefined) throw Error("duplicate name '" + e + "' in " + this);
if (this.isReservedId(t)) throw Error("id " + t + " is reserved in " + this);
if (this.isReservedName(e)) throw Error("name '" + e + "' is reserved in " + this);
if (this.valuesById[t] !== undefined) {
if (!this.options || !this.options.allow_alias) throw Error("duplicate id " + t + " in " + this);
this.values[e] = t;
} else this.valuesById[this.values[e] = t] = e;
this.comments[e] = r || null;
return this;
};
a.prototype.remove = function(e) {
if (!o.isString(e)) throw TypeError("name must be a string");
var t = this.values[e];
if (null == t) throw Error("name '" + e + "' does not exist in " + this);
delete this.valuesById[t];
delete this.values[e];
delete this.comments[e];
return this;
};
a.prototype.isReservedId = function(e) {
return i.isReservedId(this.reserved, e);
};
a.prototype.isReservedName = function(e) {
return i.isReservedName(this.reserved, e);
};
}, {
23: 23,
24: 24,
37: 37
} ],
16: [ function(e, t, r) {
t.exports = u;
var n = e(24);
((u.prototype = Object.create(n.prototype)).constructor = u).className = "Field";
var i, o = e(15), a = e(36), l = e(37), s = /^required|optional|repeated$/;
u.fromJSON = function(e, t) {
return new u(e, t.id, t.type, t.rule, t.extend, t.options);
};
function u(e, t, r, i, o, u) {
if (l.isObject(i)) {
u = i;
i = o = undefined;
} else if (l.isObject(o)) {
u = o;
o = undefined;
}
n.call(this, e, u);
if (!l.isInteger(t) || t < 0) throw TypeError("id must be a non-negative integer");
if (!l.isString(r)) throw TypeError("type must be a string");
if (i !== undefined && !s.test(i = i.toString().toLowerCase())) throw TypeError("rule must be a string rule");
if (o !== undefined && !l.isString(o)) throw TypeError("extend must be a string");
this.rule = i && "optional" !== i ? i : undefined;
this.type = r;
this.id = t;
this.extend = o || undefined;
this.required = "required" === i;
this.optional = !this.required;
this.repeated = "repeated" === i;
this.map = !1;
this.message = null;
this.partOf = null;
this.typeDefault = null;
this.defaultValue = null;
this.long = !!l.Long && a.long[r] !== undefined;
this.bytes = "bytes" === r;
this.resolvedType = null;
this.extensionField = null;
this.declaringField = null;
this._packed = null;
}
Object.defineProperty(u.prototype, "packed", {
get: function() {
null === this._packed && (this._packed = !1 !== this.getOption("packed"));
return this._packed;
}
});
u.prototype.setOption = function(e, t, r) {
"packed" === e && (this._packed = null);
return n.prototype.setOption.call(this, e, t, r);
};
u.prototype.toJSON = function() {
return l.toObject([ "rule", "optional" !== this.rule && this.rule || undefined, "type", this.type, "id", this.id, "extend", this.extend, "options", this.options ]);
};
u.prototype.resolve = function() {
if (this.resolved) return this;
if ((this.typeDefault = a.defaults[this.type]) === undefined) {
this.resolvedType = (this.declaringField ? this.declaringField.parent : this.parent).lookupTypeOrEnum(this.type);
this.resolvedType instanceof i ? this.typeDefault = null : this.typeDefault = this.resolvedType.values[Object.keys(this.resolvedType.values)[0]];
}
if (this.options && null != this.options.default) {
this.typeDefault = this.options.default;
this.resolvedType instanceof o && "string" == typeof this.typeDefault && (this.typeDefault = this.resolvedType.values[this.typeDefault]);
}
if (this.options) {
!0 !== this.options.packed && (this.options.packed === undefined || !this.resolvedType || this.resolvedType instanceof o) || delete this.options.packed;
Object.keys(this.options).length || (this.options = undefined);
}
if (this.long) {
this.typeDefault = l.Long.fromNumber(this.typeDefault, "u" === this.type.charAt(0));
Object.freeze && Object.freeze(this.typeDefault);
} else if (this.bytes && "string" == typeof this.typeDefault) {
var e;
l.base64.test(this.typeDefault) ? l.base64.decode(this.typeDefault, e = l.newBuffer(l.base64.length(this.typeDefault)), 0) : l.utf8.write(this.typeDefault, e = l.newBuffer(l.utf8.length(this.typeDefault)), 0);
this.typeDefault = e;
}
this.map ? this.defaultValue = l.emptyObject : this.repeated ? this.defaultValue = l.emptyArray : this.defaultValue = this.typeDefault;
this.parent instanceof i && (this.parent.ctor.prototype[this.name] = this.defaultValue);
return n.prototype.resolve.call(this);
};
u.d = function(e, t, r, n) {
"function" == typeof t ? t = l.decorateType(t).name : t && "object" == typeof t && (t = l.decorateEnum(t).name);
return function(i, o) {
l.decorateType(i.constructor).add(new u(o, e, t, r, {
default: n
}));
};
};
u._configure = function(e) {
i = e;
};
}, {
15: 15,
24: 24,
36: 36,
37: 37
} ],
17: [ function(e, t, r) {
var n = t.exports = e(18);
n.build = "light";
n.load = function(e, t, r) {
if ("function" == typeof t) {
r = t;
t = new n.Root();
} else t || (t = new n.Root());
return t.load(e, r);
};
n.loadSync = function(e, t) {
t || (t = new n.Root());
return t.loadSync(e);
};
n.encoder = e(14);
n.decoder = e(13);
n.verifier = e(40);
n.converter = e(12);
n.ReflectionObject = e(24);
n.Namespace = e(23);
n.Root = e(29);
n.Enum = e(15);
n.Type = e(35);
n.Field = e(16);
n.OneOf = e(25);
n.MapField = e(20);
n.Service = e(33);
n.Method = e(22);
n.Message = e(21);
n.wrappers = e(41);
n.types = e(36);
n.util = e(37);
n.ReflectionObject._configure(n.Root);
n.Namespace._configure(n.Type, n.Service);
n.Root._configure(n.Type);
n.Field._configure(n.Type);
}, {
12: 12,
13: 13,
14: 14,
15: 15,
16: 16,
18: 18,
20: 20,
21: 21,
22: 22,
23: 23,
24: 24,
25: 25,
29: 29,
33: 33,
35: 35,
36: 36,
37: 37,
40: 40,
41: 41
} ],
18: [ function(e, t, r) {
var n = r;
n.build = "minimal";
n.Writer = e(42);
n.BufferWriter = e(43);
n.Reader = e(27);
n.BufferReader = e(28);
n.util = e(39);
n.rpc = e(31);
n.roots = e(30);
n.configure = i;
function i() {
n.Reader._configure(n.BufferReader);
n.util._configure();
}
n.Writer._configure(n.BufferWriter);
i();
}, {
27: 27,
28: 28,
30: 30,
31: 31,
39: 39,
42: 42,
43: 43
} ],
19: [ function(e, t, r) {
var n = t.exports = e(17);
n.build = "full";
n.tokenize = e(34);
n.parse = e(26);
n.common = e(11);
n.Root._configure(n.Type, n.parse, n.common);
}, {
11: 11,
17: 17,
26: 26,
34: 34
} ],
20: [ function(e, t, r) {
t.exports = a;
var n = e(16);
((a.prototype = Object.create(n.prototype)).constructor = a).className = "MapField";
var i = e(36), o = e(37);
function a(e, t, r, i, a) {
n.call(this, e, t, i, a);
if (!o.isString(r)) throw TypeError("keyType must be a string");
this.keyType = r;
this.resolvedKeyType = null;
this.map = !0;
}
a.fromJSON = function(e, t) {
return new a(e, t.id, t.keyType, t.type, t.options);
};
a.prototype.toJSON = function() {
return o.toObject([ "keyType", this.keyType, "type", this.type, "id", this.id, "extend", this.extend, "options", this.options ]);
};
a.prototype.resolve = function() {
if (this.resolved) return this;
if (i.mapKey[this.keyType] === undefined) throw Error("invalid key type: " + this.keyType);
return n.prototype.resolve.call(this);
};
a.d = function(e, t, r) {
"function" == typeof r ? r = o.decorateType(r).name : r && "object" == typeof r && (r = o.decorateEnum(r).name);
return function(n, i) {
o.decorateType(n.constructor).add(new a(i, e, t, r));
};
};
}, {
16: 16,
36: 36,
37: 37
} ],
21: [ function(e, t, r) {
t.exports = i;
var n = e(39);
function i(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) this[t[r]] = e[t[r]];
}
i.create = function(e) {
return this.$type.create(e);
};
i.encode = function(e, t) {
return this.$type.encode(e, t);
};
i.encodeDelimited = function(e, t) {
return this.$type.encodeDelimited(e, t);
};
i.decode = function(e) {
return this.$type.decode(e);
};
i.decodeDelimited = function(e) {
return this.$type.decodeDelimited(e);
};
i.verify = function(e) {
return this.$type.verify(e);
};
i.fromObject = function(e) {
return this.$type.fromObject(e);
};
i.toObject = function(e, t) {
return this.$type.toObject(e, t);
};
i.prototype.toJSON = function() {
return this.$type.toObject(this, n.toJSONOptions);
};
}, {
39: 39
} ],
22: [ function(e, t, r) {
t.exports = o;
var n = e(24);
((o.prototype = Object.create(n.prototype)).constructor = o).className = "Method";
var i = e(37);
function o(e, t, r, o, a, l, s) {
if (i.isObject(a)) {
s = a;
a = l = undefined;
} else if (i.isObject(l)) {
s = l;
l = undefined;
}
if (t !== undefined && !i.isString(t)) throw TypeError("type must be a string");
if (!i.isString(r)) throw TypeError("requestType must be a string");
if (!i.isString(o)) throw TypeError("responseType must be a string");
n.call(this, e, s);
this.type = t || "rpc";
this.requestType = r;
this.requestStream = !!a || undefined;
this.responseType = o;
this.responseStream = !!l || undefined;
this.resolvedRequestType = null;
this.resolvedResponseType = null;
}
o.fromJSON = function(e, t) {
return new o(e, t.type, t.requestType, t.responseType, t.requestStream, t.responseStream, t.options);
};
o.prototype.toJSON = function() {
return i.toObject([ "type", "rpc" !== this.type && this.type || undefined, "requestType", this.requestType, "requestStream", this.requestStream, "responseType", this.responseType, "responseStream", this.responseStream, "options", this.options ]);
};
o.prototype.resolve = function() {
if (this.resolved) return this;
this.resolvedRequestType = this.parent.lookupType(this.requestType);
this.resolvedResponseType = this.parent.lookupType(this.responseType);
return n.prototype.resolve.call(this);
};
}, {
24: 24,
37: 37
} ],
23: [ function(e, t, r) {
t.exports = c;
var n = e(24);
((c.prototype = Object.create(n.prototype)).constructor = c).className = "Namespace";
var i, o, a = e(15), l = e(16), s = e(37);
c.fromJSON = function(e, t) {
return new c(e, t.options).addJSON(t.nested);
};
function u(e) {
if (!e || !e.length) return undefined;
for (var t = {}, r = 0; r < e.length; ++r) t[e[r].name] = e[r].toJSON();
return t;
}
c.arrayToJSON = u;
c.isReservedId = function(e, t) {
if (e) for (var r = 0; r < e.length; ++r) if ("string" != typeof e[r] && e[r][0] <= t && e[r][1] >= t) return !0;
return !1;
};
c.isReservedName = function(e, t) {
if (e) for (var r = 0; r < e.length; ++r) if (e[r] === t) return !0;
return !1;
};
function c(e, t) {
n.call(this, e, t);
this.nested = undefined;
this._nestedArray = null;
}
function d(e) {
e._nestedArray = null;
return e;
}
Object.defineProperty(c.prototype, "nestedArray", {
get: function() {
return this._nestedArray || (this._nestedArray = s.toArray(this.nested));
}
});
c.prototype.toJSON = function() {
return s.toObject([ "options", this.options, "nested", u(this.nestedArray) ]);
};
c.prototype.addJSON = function(e) {
if (e) for (var t, r = Object.keys(e), n = 0; n < r.length; ++n) {
t = e[r[n]];
this.add((t.fields !== undefined ? i.fromJSON : t.values !== undefined ? a.fromJSON : t.methods !== undefined ? o.fromJSON : t.id !== undefined ? l.fromJSON : c.fromJSON)(r[n], t));
}
return this;
};
c.prototype.get = function(e) {
return this.nested && this.nested[e] || null;
};
c.prototype.getEnum = function(e) {
if (this.nested && this.nested[e] instanceof a) return this.nested[e].values;
throw Error("no such enum");
};
c.prototype.add = function(e) {
if (!(e instanceof l && e.extend !== undefined || e instanceof i || e instanceof a || e instanceof o || e instanceof c)) throw TypeError("object must be a valid nested object");
if (this.nested) {
var t = this.get(e.name);
if (t) {
if (!(t instanceof c && e instanceof c) || t instanceof i || t instanceof o) throw Error("duplicate name '" + e.name + "' in " + this);
for (var r = t.nestedArray, n = 0; n < r.length; ++n) e.add(r[n]);
this.remove(t);
this.nested || (this.nested = {});
e.setOptions(t.options, !0);
}
} else this.nested = {};
this.nested[e.name] = e;
e.onAdd(this);
return d(this);
};
c.prototype.remove = function(e) {
if (!(e instanceof n)) throw TypeError("object must be a ReflectionObject");
if (e.parent !== this) throw Error(e + " is not a member of " + this);
delete this.nested[e.name];
Object.keys(this.nested).length || (this.nested = undefined);
e.onRemove(this);
return d(this);
};
c.prototype.define = function(e, t) {
if (s.isString(e)) e = e.split("."); else if (!Array.isArray(e)) throw TypeError("illegal path");
if (e && e.length && "" === e[0]) throw Error("path must be relative");
for (var r = this; e.length > 0; ) {
var n = e.shift();
if (r.nested && r.nested[n]) {
if (!((r = r.nested[n]) instanceof c)) throw Error("path conflicts with non-namespace objects");
} else r.add(r = new c(n));
}
t && r.addJSON(t);
return r;
};
c.prototype.resolveAll = function() {
for (var e = this.nestedArray, t = 0; t < e.length; ) e[t] instanceof c ? e[t++].resolveAll() : e[t++].resolve();
return this.resolve();
};
c.prototype.lookup = function(e, t, r) {
if ("boolean" == typeof t) {
r = t;
t = undefined;
} else t && !Array.isArray(t) && (t = [ t ]);
if (s.isString(e) && e.length) {
if ("." === e) return this.root;
e = e.split(".");
} else if (!e.length) return this;
if ("" === e[0]) return this.root.lookup(e.slice(1), t);
var n = this.get(e[0]);
if (n) {
if (1 === e.length) {
if (!t || t.indexOf(n.constructor) > -1) return n;
} else if (n instanceof c && (n = n.lookup(e.slice(1), t, !0))) return n;
} else for (var i = 0; i < this.nestedArray.length; ++i) if (this._nestedArray[i] instanceof c && (n = this._nestedArray[i].lookup(e, t, !0))) return n;
return null === this.parent || r ? null : this.parent.lookup(e, t);
};
c.prototype.lookupType = function(e) {
var t = this.lookup(e, [ i ]);
if (!t) throw Error("no such type");
return t;
};
c.prototype.lookupEnum = function(e) {
var t = this.lookup(e, [ a ]);
if (!t) throw Error("no such Enum '" + e + "' in " + this);
return t;
};
c.prototype.lookupTypeOrEnum = function(e) {
var t = this.lookup(e, [ i, a ]);
if (!t) throw Error("no such Type or Enum '" + e + "' in " + this);
return t;
};
c.prototype.lookupService = function(e) {
var t = this.lookup(e, [ o ]);
if (!t) throw Error("no such Service '" + e + "' in " + this);
return t;
};
c._configure = function(e, t) {
i = e;
o = t;
};
}, {
15: 15,
16: 16,
24: 24,
37: 37
} ],
24: [ function(e, t, r) {
t.exports = o;
o.className = "ReflectionObject";
var n, i = e(37);
function o(e, t) {
if (!i.isString(e)) throw TypeError("name must be a string");
if (t && !i.isObject(t)) throw TypeError("options must be an object");
this.options = t;
this.name = e;
this.parent = null;
this.resolved = !1;
this.comment = null;
this.filename = null;
}
Object.defineProperties(o.prototype, {
root: {
get: function() {
for (var e = this; null !== e.parent; ) e = e.parent;
return e;
}
},
fullName: {
get: function() {
for (var e = [ this.name ], t = this.parent; t; ) {
e.unshift(t.name);
t = t.parent;
}
return e.join(".");
}
}
});
o.prototype.toJSON = function() {
throw Error();
};
o.prototype.onAdd = function(e) {
this.parent && this.parent !== e && this.parent.remove(this);
this.parent = e;
this.resolved = !1;
var t = e.root;
t instanceof n && t._handleAdd(this);
};
o.prototype.onRemove = function(e) {
var t = e.root;
t instanceof n && t._handleRemove(this);
this.parent = null;
this.resolved = !1;
};
o.prototype.resolve = function() {
if (this.resolved) return this;
this.root instanceof n && (this.resolved = !0);
return this;
};
o.prototype.getOption = function(e) {
return this.options ? this.options[e] : undefined;
};
o.prototype.setOption = function(e, t, r) {
r && this.options && this.options[e] !== undefined || ((this.options || (this.options = {}))[e] = t);
return this;
};
o.prototype.setOptions = function(e, t) {
if (e) for (var r = Object.keys(e), n = 0; n < r.length; ++n) this.setOption(r[n], e[r[n]], t);
return this;
};
o.prototype.toString = function() {
var e = this.constructor.className, t = this.fullName;
return t.length ? e + " " + t : e;
};
o._configure = function(e) {
n = e;
};
}, {
37: 37
} ],
25: [ function(e, t, r) {
t.exports = a;
var n = e(24);
((a.prototype = Object.create(n.prototype)).constructor = a).className = "OneOf";
var i = e(16), o = e(37);
function a(e, t, r) {
if (!Array.isArray(t)) {
r = t;
t = undefined;
}
n.call(this, e, r);
if (t !== undefined && !Array.isArray(t)) throw TypeError("fieldNames must be an Array");
this.oneof = t || [];
this.fieldsArray = [];
}
a.fromJSON = function(e, t) {
return new a(e, t.oneof, t.options);
};
a.prototype.toJSON = function() {
return o.toObject([ "options", this.options, "oneof", this.oneof ]);
};
function l(e) {
if (e.parent) for (var t = 0; t < e.fieldsArray.length; ++t) e.fieldsArray[t].parent || e.parent.add(e.fieldsArray[t]);
}
a.prototype.add = function(e) {
if (!(e instanceof i)) throw TypeError("field must be a Field");
e.parent && e.parent !== this.parent && e.parent.remove(e);
this.oneof.push(e.name);
this.fieldsArray.push(e);
e.partOf = this;
l(this);
return this;
};
a.prototype.remove = function(e) {
if (!(e instanceof i)) throw TypeError("field must be a Field");
var t = this.fieldsArray.indexOf(e);
if (t < 0) throw Error(e + " is not a member of " + this);
this.fieldsArray.splice(t, 1);
(t = this.oneof.indexOf(e.name)) > -1 && this.oneof.splice(t, 1);
e.partOf = null;
return this;
};
a.prototype.onAdd = function(e) {
n.prototype.onAdd.call(this, e);
for (var t = 0; t < this.oneof.length; ++t) {
var r = e.get(this.oneof[t]);
if (r && !r.partOf) {
r.partOf = this;
this.fieldsArray.push(r);
}
}
l(this);
};
a.prototype.onRemove = function(e) {
for (var t, r = 0; r < this.fieldsArray.length; ++r) (t = this.fieldsArray[r]).parent && t.parent.remove(t);
n.prototype.onRemove.call(this, e);
};
a.d = function() {
for (var e = new Array(arguments.length), t = 0; t < arguments.length; ) e[t] = arguments[t++];
return function(t, r) {
o.decorateType(t.constructor).add(new a(r, e));
Object.defineProperty(t, r, {
get: o.oneOfGetter(e),
set: o.oneOfSetter(e)
});
};
};
}, {
16: 16,
24: 24,
37: 37
} ],
26: [ function(e, t, r) {
t.exports = I;
I.filename = null;
I.defaults = {
keepCase: !1
};
var n = e(34), i = e(29), o = e(35), a = e(16), l = e(20), s = e(25), u = e(15), c = e(33), d = e(22), p = e(36), f = e(37), y = /^[1-9][0-9]*$/, h = /^-?[1-9][0-9]*$/, g = /^0[x][0-9a-fA-F]+$/, b = /^-?0[x][0-9a-fA-F]+$/, m = /^0[0-7]+$/, w = /^-?0[0-7]+$/, v = /^(?![eE])[0-9]*(?:\.[0-9]*)?(?:[eE][+-]?[0-9]+)?$/, O = /^[a-zA-Z_][a-zA-Z_0-9]*$/, $ = /^(?:\.?[a-zA-Z_][a-zA-Z_0-9]*)+$/, R = /^(?:\.[a-zA-Z][a-zA-Z_0-9]*)+$/;
function I(e, t, r) {
if (!(t instanceof i)) {
r = t;
t = new i();
}
r || (r = I.defaults);
var k, P, _, x, A, j = n(e), S = j.next, T = j.push, C = j.peek, L = j.skip, D = j.cmnt, G = !0, E = !1, W = t, H = r.keepCase ? function(e) {
return e;
} : f.camelCase;
function B(e, t, r) {
var n = I.filename;
r || (I.filename = null);
return Error("illegal " + (t || "token") + " '" + e + "' (" + (n ? n + ", " : "") + "line " + j.line + ")");
}
function U() {
var e, t = [];
do {
if ('"' !== (e = S()) && "'" !== e) throw B(e);
t.push(S());
L(e);
e = C();
} while ('"' === e || "'" === e);
return t.join("");
}
function N(e) {
var t = S();
switch (t) {
case "'":
case '"':
T(t);
return U();

case "true":
case "TRUE":
return !0;

case "false":
case "FALSE":
return !1;
}
try {
return M(t, !0);
} catch (r) {
if (e && $.test(t)) return t;
throw B(t, "value");
}
}
function F(e, t) {
var r, n;
do {
!t || '"' !== (r = C()) && "'" !== r ? e.push([ n = V(S()), L("to", !0) ? V(S()) : n ]) : e.push(U());
} while (L(",", !0));
L(";");
}
function M(e, t) {
var r = 1;
if ("-" === e.charAt(0)) {
r = -1;
e = e.substring(1);
}
switch (e) {
case "inf":
case "INF":
case "Inf":
return Infinity * r;

case "nan":
case "NAN":
case "Nan":
case "NaN":
return NaN;

case "0":
return 0;
}
if (y.test(e)) return r * parseInt(e, 10);
if (g.test(e)) return r * parseInt(e, 16);
if (m.test(e)) return r * parseInt(e, 8);
if (v.test(e)) return r * parseFloat(e);
throw B(e, "number", t);
}
function V(e, t) {
switch (e) {
case "max":
case "MAX":
case "Max":
return 536870911;

case "0":
return 0;
}
if (!t && "-" === e.charAt(0)) throw B(e, "id");
if (h.test(e)) return parseInt(e, 10);
if (b.test(e)) return parseInt(e, 16);
if (w.test(e)) return parseInt(e, 8);
throw B(e, "id");
}
function q() {
if (k !== undefined) throw B("package");
k = S();
if (!$.test(k)) throw B(k, "name");
W = W.define(k);
L(";");
}
function J() {
var e, t = C();
switch (t) {
case "weak":
e = _ || (_ = []);
S();
break;

case "public":
S();

default:
e = P || (P = []);
}
t = U();
L(";");
e.push(t);
}
function X() {
L("=");
x = U();
if (!(E = "proto3" === x) && "proto2" !== x) throw B(x, "syntax");
L(";");
}
function Y(e, t) {
switch (t) {
case "option":
ie(e, t);
L(";");
return !0;

case "message":
K(e, t);
return !0;

case "enum":
re(e, t);
return !0;

case "service":
se(e, t);
return !0;

case "extend":
ce(e, t);
return !0;
}
return !1;
}
function z(e, t, r) {
var n = j.line;
if (e) {
e.comment = D();
e.filename = I.filename;
}
if (L("{", !0)) {
for (var i; "}" !== (i = S()); ) t(i);
L(";", !0);
} else {
r && r();
L(";");
e && "string" != typeof e.comment && (e.comment = D(n));
}
}
function K(e, t) {
if (!O.test(t = S())) throw B(t, "type name");
var r = new o(t);
z(r, function(e) {
if (!Y(r, e)) switch (e) {
case "map":
ee(r);
break;

case "required":
case "optional":
case "repeated":
Q(r, e);
break;

case "oneof":
te(r, e);
break;

case "extensions":
F(r.extensions || (r.extensions = []));
break;

case "reserved":
F(r.reserved || (r.reserved = []), !0);
break;

default:
if (!E || !$.test(e)) throw B(e);
T(e);
Q(r, "optional");
}
});
e.add(r);
}
function Q(e, t, r) {
var n = S();
if ("group" !== n) {
if (!$.test(n)) throw B(n, "type");
var i = S();
if (!O.test(i)) throw B(i, "name");
i = H(i);
L("=");
var o = new a(i, V(S()), n, t, r);
z(o, function(e) {
if ("option" !== e) throw B(e);
ie(o, e);
L(";");
}, function() {
le(o);
});
e.add(o);
E || !o.repeated || p.packed[n] === undefined && p.basic[n] !== undefined || o.setOption("packed", !1, !0);
} else Z(e, t);
}
function Z(e, t) {
var r = S();
if (!O.test(r)) throw B(r, "name");
var n = f.lcFirst(r);
r === n && (r = f.ucFirst(r));
L("=");
var i = V(S()), l = new o(r);
l.group = !0;
var s = new a(n, i, r, t);
s.filename = I.filename;
z(l, function(e) {
switch (e) {
case "option":
ie(l, e);
L(";");
break;

case "required":
case "optional":
case "repeated":
Q(l, e);
break;

default:
throw B(e);
}
});
e.add(l).add(s);
}
function ee(e) {
L("<");
var t = S();
if (p.mapKey[t] === undefined) throw B(t, "type");
L(",");
var r = S();
if (!$.test(r)) throw B(r, "type");
L(">");
var n = S();
if (!O.test(n)) throw B(n, "name");
L("=");
var i = new l(H(n), V(S()), t, r);
z(i, function(e) {
if ("option" !== e) throw B(e);
ie(i, e);
L(";");
}, function() {
le(i);
});
e.add(i);
}
function te(e, t) {
if (!O.test(t = S())) throw B(t, "name");
var r = new s(H(t));
z(r, function(e) {
if ("option" === e) {
ie(r, e);
L(";");
} else {
T(e);
Q(r, "optional");
}
});
e.add(r);
}
function re(e, t) {
if (!O.test(t = S())) throw B(t, "name");
var r = new u(t);
z(r, function(e) {
switch (e) {
case "option":
ie(r, e);
L(";");
break;

case "reserved":
F(r.reserved || (r.reserved = []), !0);
break;

default:
ne(r, e);
}
});
e.add(r);
}
function ne(e, t) {
if (!O.test(t)) throw B(t, "name");
L("=");
var r = V(S(), !0), n = {};
z(n, function(e) {
if ("option" !== e) throw B(e);
ie(n, e);
L(";");
}, function() {
le(n);
});
e.add(t, r, n.comment);
}
function ie(e, t) {
var r = L("(", !0);
if (!$.test(t = S())) throw B(t, "name");
var n = t;
if (r) {
L(")");
n = "(" + n + ")";
t = C();
if (R.test(t)) {
n += t;
S();
}
}
L("=");
oe(e, n);
}
function oe(e, t) {
if (L("{", !0)) do {
if (!O.test(A = S())) throw B(A, "name");
if ("{" === C()) oe(e, t + "." + A); else {
L(":");
"{" === C() ? oe(e, t + "." + A) : ae(e, t + "." + A, N(!0));
}
} while (!L("}", !0)); else ae(e, t, N(!0));
}
function ae(e, t, r) {
e.setOption && e.setOption(t, r);
}
function le(e) {
if (L("[", !0)) {
do {
ie(e, "option");
} while (L(",", !0));
L("]");
}
return e;
}
function se(e, t) {
if (!O.test(t = S())) throw B(t, "service name");
var r = new c(t);
z(r, function(e) {
if (!Y(r, e)) {
if ("rpc" !== e) throw B(e);
ue(r, e);
}
});
e.add(r);
}
function ue(e, t) {
var r = t;
if (!O.test(t = S())) throw B(t, "name");
var n, i, o, a, l = t;
L("(");
L("stream", !0) && (i = !0);
if (!$.test(t = S())) throw B(t);
n = t;
L(")");
L("returns");
L("(");
L("stream", !0) && (a = !0);
if (!$.test(t = S())) throw B(t);
o = t;
L(")");
var s = new d(l, r, n, o, i, a);
z(s, function(e) {
if ("option" !== e) throw B(e);
ie(s, e);
L(";");
});
e.add(s);
}
function ce(e, t) {
if (!$.test(t = S())) throw B(t, "reference");
var r = t;
z(null, function(t) {
switch (t) {
case "required":
case "repeated":
case "optional":
Q(e, t, r);
break;

default:
if (!E || !$.test(t)) throw B(t);
T(t);
Q(e, "optional", r);
}
});
}
for (;null !== (A = S()); ) switch (A) {
case "package":
if (!G) throw B(A);
q();
break;

case "import":
if (!G) throw B(A);
J();
break;

case "syntax":
if (!G) throw B(A);
X();
break;

case "option":
if (!G) throw B(A);
ie(W, A);
L(";");
break;

default:
if (Y(W, A)) {
G = !1;
continue;
}
throw B(A);
}
I.filename = null;
return {
package: k,
imports: P,
weakImports: _,
syntax: x,
root: t
};
}
}, {
15: 15,
16: 16,
20: 20,
22: 22,
25: 25,
29: 29,
33: 33,
34: 34,
35: 35,
36: 36,
37: 37
} ],
27: [ function(e, t, r) {
t.exports = s;
var n, i = e(39), o = i.LongBits, a = i.utf8;
function l(e, t) {
return RangeError("index out of range: " + e.pos + " + " + (t || 1) + " > " + e.len);
}
function s(e) {
this.buf = e;
this.pos = 0;
this.len = e.length;
}
var u = "undefined" != typeof Uint8Array ? function(e) {
if (e instanceof Uint8Array || Array.isArray(e)) return new s(e);
throw Error("illegal buffer");
} : function(e) {
if (Array.isArray(e)) return new s(e);
throw Error("illegal buffer");
};
s.create = i.Buffer ? function(e) {
return (s.create = function(e) {
return i.Buffer.isBuffer(e) ? new n(e) : u(e);
})(e);
} : u;
s.prototype._slice = i.Array.prototype.subarray || i.Array.prototype.slice;
s.prototype.uint32 = function() {
var e = 4294967295;
return function() {
e = (127 & this.buf[this.pos]) >>> 0;
if (this.buf[this.pos++] < 128) return e;
e = (e | (127 & this.buf[this.pos]) << 7) >>> 0;
if (this.buf[this.pos++] < 128) return e;
e = (e | (127 & this.buf[this.pos]) << 14) >>> 0;
if (this.buf[this.pos++] < 128) return e;
e = (e | (127 & this.buf[this.pos]) << 21) >>> 0;
if (this.buf[this.pos++] < 128) return e;
e = (e | (15 & this.buf[this.pos]) << 28) >>> 0;
if (this.buf[this.pos++] < 128) return e;
if ((this.pos += 5) > this.len) {
this.pos = this.len;
throw l(this, 10);
}
return e;
};
}();
s.prototype.int32 = function() {
return 0 | this.uint32();
};
s.prototype.sint32 = function() {
var e = this.uint32();
return e >>> 1 ^ -(1 & e) | 0;
};
function c() {
var e = new o(0, 0), t = 0;
if (!(this.len - this.pos > 4)) {
for (;t < 3; ++t) {
if (this.pos >= this.len) throw l(this);
e.lo = (e.lo | (127 & this.buf[this.pos]) << 7 * t) >>> 0;
if (this.buf[this.pos++] < 128) return e;
}
e.lo = (e.lo | (127 & this.buf[this.pos++]) << 7 * t) >>> 0;
return e;
}
for (;t < 4; ++t) {
e.lo = (e.lo | (127 & this.buf[this.pos]) << 7 * t) >>> 0;
if (this.buf[this.pos++] < 128) return e;
}
e.lo = (e.lo | (127 & this.buf[this.pos]) << 28) >>> 0;
e.hi = (e.hi | (127 & this.buf[this.pos]) >> 4) >>> 0;
if (this.buf[this.pos++] < 128) return e;
t = 0;
if (this.len - this.pos > 4) for (;t < 5; ++t) {
e.hi = (e.hi | (127 & this.buf[this.pos]) << 7 * t + 3) >>> 0;
if (this.buf[this.pos++] < 128) return e;
} else for (;t < 5; ++t) {
if (this.pos >= this.len) throw l(this);
e.hi = (e.hi | (127 & this.buf[this.pos]) << 7 * t + 3) >>> 0;
if (this.buf[this.pos++] < 128) return e;
}
throw Error("invalid varint encoding");
}
s.prototype.bool = function() {
return 0 !== this.uint32();
};
function d(e, t) {
return (e[t - 4] | e[t - 3] << 8 | e[t - 2] << 16 | e[t - 1] << 24) >>> 0;
}
s.prototype.fixed32 = function() {
if (this.pos + 4 > this.len) throw l(this, 4);
return d(this.buf, this.pos += 4);
};
s.prototype.sfixed32 = function() {
if (this.pos + 4 > this.len) throw l(this, 4);
return 0 | d(this.buf, this.pos += 4);
};
function p() {
if (this.pos + 8 > this.len) throw l(this, 8);
return new o(d(this.buf, this.pos += 4), d(this.buf, this.pos += 4));
}
s.prototype.float = function() {
if (this.pos + 4 > this.len) throw l(this, 4);
var e = i.float.readFloatLE(this.buf, this.pos);
this.pos += 4;
return e;
};
s.prototype.double = function() {
if (this.pos + 8 > this.len) throw l(this, 4);
var e = i.float.readDoubleLE(this.buf, this.pos);
this.pos += 8;
return e;
};
s.prototype.bytes = function() {
var e = this.uint32(), t = this.pos, r = this.pos + e;
if (r > this.len) throw l(this, e);
this.pos += e;
return Array.isArray(this.buf) ? this.buf.slice(t, r) : t === r ? new this.buf.constructor(0) : this._slice.call(this.buf, t, r);
};
s.prototype.string = function() {
var e = this.bytes();
return a.read(e, 0, e.length);
};
s.prototype.skip = function(e) {
if ("number" == typeof e) {
if (this.pos + e > this.len) throw l(this, e);
this.pos += e;
} else do {
if (this.pos >= this.len) throw l(this);
} while (128 & this.buf[this.pos++]);
return this;
};
s.prototype.skipType = function(e) {
switch (e) {
case 0:
this.skip();
break;

case 1:
this.skip(8);
break;

case 2:
this.skip(this.uint32());
break;

case 3:
for (;;) {
if (4 == (e = 7 & this.uint32())) break;
this.skipType(e);
}
break;

case 5:
this.skip(4);
break;

default:
throw Error("invalid wire type " + e + " at offset " + this.pos);
}
return this;
};
s._configure = function(e) {
n = e;
var t = i.Long ? "toLong" : "toNumber";
i.merge(s.prototype, {
int64: function() {
return c.call(this)[t](!1);
},
uint64: function() {
return c.call(this)[t](!0);
},
sint64: function() {
return c.call(this).zzDecode()[t](!1);
},
fixed64: function() {
return p.call(this)[t](!0);
},
sfixed64: function() {
return p.call(this)[t](!1);
}
});
};
}, {
39: 39
} ],
28: [ function(e, t, r) {
t.exports = o;
var n = e(27);
(o.prototype = Object.create(n.prototype)).constructor = o;
var i = e(39);
function o(e) {
n.call(this, e);
}
i.Buffer && (o.prototype._slice = i.Buffer.prototype.slice);
o.prototype.string = function() {
var e = this.uint32();
return this.buf.utf8Slice(this.pos, this.pos = Math.min(this.pos + e, this.len));
};
}, {
27: 27,
39: 39
} ],
29: [ function(e, t, r) {
t.exports = d;
var n = e(23);
((d.prototype = Object.create(n.prototype)).constructor = d).className = "Root";
var i, o, a, l = e(16), s = e(15), u = e(25), c = e(37);
function d(e) {
n.call(this, "", e);
this.deferred = [];
this.files = [];
}
d.fromJSON = function(e, t) {
t || (t = new d());
e.options && t.setOptions(e.options);
return t.addJSON(e.nested);
};
d.prototype.resolvePath = c.path.resolve;
function p() {}
d.prototype.load = function e(t, r, n) {
if ("function" == typeof r) {
n = r;
r = undefined;
}
var i = this;
if (!n) return c.asPromise(e, i, t, r);
var l = n === p;
function s(e, t) {
if (n) {
var r = n;
n = null;
if (l) throw e;
r(e, t);
}
}
function u(e, t) {
try {
c.isString(t) && "{" === t.charAt(0) && (t = JSON.parse(t));
if (c.isString(t)) {
o.filename = e;
var n, a = o(t, i, r), u = 0;
if (a.imports) for (;u < a.imports.length; ++u) (n = i.resolvePath(e, a.imports[u])) && d(n);
if (a.weakImports) for (u = 0; u < a.weakImports.length; ++u) (n = i.resolvePath(e, a.weakImports[u])) && d(n, !0);
} else i.setOptions(t.options).addJSON(t.nested);
} catch (e) {
s(e);
}
l || f || s(null, i);
}
function d(e, t) {
var r = e.lastIndexOf("google/protobuf/");
if (r > -1) {
var o = e.substring(r);
o in a && (e = o);
}
if (!(i.files.indexOf(e) > -1)) {
i.files.push(e);
if (e in a) if (l) u(e, a[e]); else {
++f;
setTimeout(function() {
--f;
u(e, a[e]);
});
} else if (l) {
var d;
try {
d = c.fs.readFileSync(e).toString("utf8");
} catch (e) {
t || s(e);
return;
}
u(e, d);
} else {
++f;
c.fetch(e, function(r, o) {
--f;
n && (r ? t ? f || s(null, i) : s(r) : u(e, o));
});
}
}
}
var f = 0;
c.isString(t) && (t = [ t ]);
for (var y, h = 0; h < t.length; ++h) (y = i.resolvePath("", t[h])) && d(y);
if (l) return i;
f || s(null, i);
return undefined;
};
d.prototype.loadSync = function(e, t) {
if (!c.isNode) throw Error("not supported");
return this.load(e, t, p);
};
d.prototype.resolveAll = function() {
if (this.deferred.length) throw Error("unresolvable extensions: " + this.deferred.map(function(e) {
return "'extend " + e.extend + "' in " + e.parent.fullName;
}).join(", "));
return n.prototype.resolveAll.call(this);
};
var f = /^[A-Z]/;
function y(e, t) {
var r = t.parent.lookup(t.extend);
if (r) {
var n = new l(t.fullName, t.id, t.type, t.rule, undefined, t.options);
n.declaringField = t;
t.extensionField = n;
r.add(n);
return !0;
}
return !1;
}
d.prototype._handleAdd = function(e) {
if (e instanceof l) e.extend === undefined || e.extensionField || y(0, e) || this.deferred.push(e); else if (e instanceof s) f.test(e.name) && (e.parent[e.name] = e.values); else if (!(e instanceof u)) {
if (e instanceof i) for (var t = 0; t < this.deferred.length; ) y(0, this.deferred[t]) ? this.deferred.splice(t, 1) : ++t;
for (var r = 0; r < e.nestedArray.length; ++r) this._handleAdd(e._nestedArray[r]);
f.test(e.name) && (e.parent[e.name] = e);
}
};
d.prototype._handleRemove = function(e) {
if (e instanceof l) {
if (e.extend !== undefined) if (e.extensionField) {
e.extensionField.parent.remove(e.extensionField);
e.extensionField = null;
} else {
var t = this.deferred.indexOf(e);
t > -1 && this.deferred.splice(t, 1);
}
} else if (e instanceof s) f.test(e.name) && delete e.parent[e.name]; else if (e instanceof n) {
for (var r = 0; r < e.nestedArray.length; ++r) this._handleRemove(e._nestedArray[r]);
f.test(e.name) && delete e.parent[e.name];
}
};
d._configure = function(e, t, r) {
i = e;
o = t;
a = r;
};
}, {
15: 15,
16: 16,
23: 23,
25: 25,
37: 37
} ],
30: [ function(e, t, r) {
t.exports = {};
}, {} ],
31: [ function(e, t, r) {
r.Service = e(32);
}, {
32: 32
} ],
32: [ function(e, t, r) {
t.exports = i;
var n = e(39);
(i.prototype = Object.create(n.EventEmitter.prototype)).constructor = i;
function i(e, t, r) {
if ("function" != typeof e) throw TypeError("rpcImpl must be a function");
n.EventEmitter.call(this);
this.rpcImpl = e;
this.requestDelimited = Boolean(t);
this.responseDelimited = Boolean(r);
}
i.prototype.rpcCall = function e(t, r, i, o, a) {
if (!o) throw TypeError("request must be specified");
var l = this;
if (!a) return n.asPromise(e, l, t, r, i, o);
if (!l.rpcImpl) {
setTimeout(function() {
a(Error("already ended"));
}, 0);
return undefined;
}
try {
return l.rpcImpl(t, r[l.requestDelimited ? "encodeDelimited" : "encode"](o).finish(), function(e, r) {
if (e) {
l.emit("error", e, t);
return a(e);
}
if (null === r) {
l.end(!0);
return undefined;
}
if (!(r instanceof i)) try {
r = i[l.responseDelimited ? "decodeDelimited" : "decode"](r);
} catch (e) {
l.emit("error", e, t);
return a(e);
}
l.emit("data", r, t);
return a(null, r);
});
} catch (e) {
l.emit("error", e, t);
setTimeout(function() {
a(e);
}, 0);
return undefined;
}
};
i.prototype.end = function(e) {
if (this.rpcImpl) {
e || this.rpcImpl(null, null, null);
this.rpcImpl = null;
this.emit("end").off();
}
return this;
};
}, {
39: 39
} ],
33: [ function(e, t, r) {
t.exports = l;
var n = e(23);
((l.prototype = Object.create(n.prototype)).constructor = l).className = "Service";
var i = e(22), o = e(37), a = e(31);
function l(e, t) {
n.call(this, e, t);
this.methods = {};
this._methodsArray = null;
}
l.fromJSON = function(e, t) {
var r = new l(e, t.options);
if (t.methods) for (var n = Object.keys(t.methods), o = 0; o < n.length; ++o) r.add(i.fromJSON(n[o], t.methods[n[o]]));
t.nested && r.addJSON(t.nested);
return r;
};
l.prototype.toJSON = function() {
var e = n.prototype.toJSON.call(this);
return o.toObject([ "options", e && e.options || undefined, "methods", n.arrayToJSON(this.methodsArray) || {}, "nested", e && e.nested || undefined ]);
};
Object.defineProperty(l.prototype, "methodsArray", {
get: function() {
return this._methodsArray || (this._methodsArray = o.toArray(this.methods));
}
});
function s(e) {
e._methodsArray = null;
return e;
}
l.prototype.get = function(e) {
return this.methods[e] || n.prototype.get.call(this, e);
};
l.prototype.resolveAll = function() {
for (var e = this.methodsArray, t = 0; t < e.length; ++t) e[t].resolve();
return n.prototype.resolve.call(this);
};
l.prototype.add = function(e) {
if (this.get(e.name)) throw Error("duplicate name '" + e.name + "' in " + this);
if (e instanceof i) {
this.methods[e.name] = e;
e.parent = this;
return s(this);
}
return n.prototype.add.call(this, e);
};
l.prototype.remove = function(e) {
if (e instanceof i) {
if (this.methods[e.name] !== e) throw Error(e + " is not a member of " + this);
delete this.methods[e.name];
e.parent = null;
return s(this);
}
return n.prototype.remove.call(this, e);
};
l.prototype.create = function(e, t, r) {
for (var n, i = new a.Service(e, t, r), l = 0; l < this.methodsArray.length; ++l) {
var s = o.lcFirst((n = this._methodsArray[l]).resolve().name).replace(/[^$\w_]/g, "");
i[s] = o.codegen([ "r", "c" ], o.isReserved(s) ? s + "_" : s)("return this.rpcCall(m,q,s,r,c)")({
m: n,
q: n.resolvedRequestType.ctor,
s: n.resolvedResponseType.ctor
});
}
return i;
};
}, {
22: 22,
23: 23,
31: 31,
37: 37
} ],
34: [ function(e, t, r) {
t.exports = p;
var n = /[\s{}=;:[\],'"()<>]/g, i = /(?:"([^"\\]*(?:\\.[^"\\]*)*)")/g, o = /(?:'([^'\\]*(?:\\.[^'\\]*)*)')/g, a = /^ *[*/]+ */, l = /\n/g, s = /\s/, u = /\\(.?)/g, c = {
0: "\0",
r: "\r",
n: "\n",
t: "\t"
};
function d(e) {
return e.replace(u, function(e, t) {
switch (t) {
case "\\":
case "":
return t;

default:
return c[t] || "";
}
});
}
p.unescape = d;
function p(e) {
var t = 0, r = (e = e.toString()).length, u = 1, c = null, p = null, f = 0, y = !1, h = [], g = null;
function b(e) {
return Error("illegal " + e + " (line " + u + ")");
}
function m() {
var r = "'" === g ? o : i;
r.lastIndex = t - 1;
var n = r.exec(e);
if (!n) throw b("string");
t = r.lastIndex;
$(g);
g = null;
return d(n[1]);
}
function w(t) {
return e.charAt(t);
}
function v(t, r) {
c = e.charAt(t++);
f = u;
y = !1;
var n, i = t - 3;
do {
if (--i < 0 || "\n" === (n = e.charAt(i))) {
y = !0;
break;
}
} while (" " === n || "\t" === n);
for (var o = e.substring(t, r).split(l), s = 0; s < o.length; ++s) o[s] = o[s].replace(a, "").trim();
p = o.join("\n").trim();
}
function O() {
if (h.length > 0) return h.shift();
if (g) return m();
var i, o, a, l, c;
do {
if (t === r) return null;
i = !1;
for (;s.test(a = w(t)); ) {
"\n" === a && ++u;
if (++t === r) return null;
}
if ("/" === w(t)) {
if (++t === r) throw b("comment");
if ("/" === w(t)) {
c = "/" === w(l = t + 1);
for (;"\n" !== w(++t); ) if (t === r) return null;
++t;
c && v(l, t - 1);
++u;
i = !0;
} else {
if ("*" !== (a = w(t))) return "/";
c = "*" === w(l = t + 1);
do {
"\n" === a && ++u;
if (++t === r) throw b("comment");
o = a;
a = w(t);
} while ("*" !== o || "/" !== a);
++t;
c && v(l, t - 2);
i = !0;
}
}
} while (i);
var d = t;
n.lastIndex = 0;
if (!n.test(w(d++))) for (;d < r && !n.test(w(d)); ) ++d;
var p = e.substring(t, t = d);
'"' !== p && "'" !== p || (g = p);
return p;
}
function $(e) {
h.push(e);
}
function R() {
if (!h.length) {
var e = O();
if (null === e) return null;
$(e);
}
return h[0];
}
return Object.defineProperty({
next: O,
peek: R,
push: $,
skip: function(e, t) {
var r = R();
if (r === e) {
O();
return !0;
}
if (!t) throw b("token '" + r + "', '" + e + "' expected");
return !1;
},
cmnt: function(e) {
var t = null;
if (e === undefined) f !== u - 1 || "*" !== c && !y || (t = p); else {
f < e && R();
f !== e || y || "/" !== c || (t = p);
}
return t;
}
}, "line", {
get: function() {
return u;
}
});
}
}, {} ],
35: [ function(e, t, r) {
t.exports = m;
var n = e(23);
((m.prototype = Object.create(n.prototype)).constructor = m).className = "Type";
var i = e(15), o = e(25), a = e(16), l = e(20), s = e(33), u = e(21), c = e(27), d = e(42), p = e(37), f = e(14), y = e(13), h = e(40), g = e(12), b = e(41);
function m(e, t) {
n.call(this, e, t);
this.fields = {};
this.oneofs = undefined;
this.extensions = undefined;
this.reserved = undefined;
this.group = undefined;
this._fieldsById = null;
this._fieldsArray = null;
this._oneofsArray = null;
this._ctor = null;
}
Object.defineProperties(m.prototype, {
fieldsById: {
get: function() {
if (this._fieldsById) return this._fieldsById;
this._fieldsById = {};
for (var e = Object.keys(this.fields), t = 0; t < e.length; ++t) {
var r = this.fields[e[t]], n = r.id;
if (this._fieldsById[n]) throw Error("duplicate id " + n + " in " + this);
this._fieldsById[n] = r;
}
return this._fieldsById;
}
},
fieldsArray: {
get: function() {
return this._fieldsArray || (this._fieldsArray = p.toArray(this.fields));
}
},
oneofsArray: {
get: function() {
return this._oneofsArray || (this._oneofsArray = p.toArray(this.oneofs));
}
},
ctor: {
get: function() {
return this._ctor || (this.ctor = m.generateConstructor(this)());
},
set: function(e) {
var t = e.prototype;
if (!(t instanceof u)) {
(e.prototype = new u()).constructor = e;
p.merge(e.prototype, t);
}
e.$type = e.prototype.$type = this;
p.merge(e, u, !0);
this._ctor = e;
for (var r = 0; r < this.fieldsArray.length; ++r) this._fieldsArray[r].resolve();
var n = {};
for (r = 0; r < this.oneofsArray.length; ++r) n[this._oneofsArray[r].resolve().name] = {
get: p.oneOfGetter(this._oneofsArray[r].oneof),
set: p.oneOfSetter(this._oneofsArray[r].oneof)
};
r && Object.defineProperties(e.prototype, n);
}
}
});
m.generateConstructor = function(e) {
for (var t, r = p.codegen([ "p" ], e.name), n = 0; n < e.fieldsArray.length; ++n) (t = e._fieldsArray[n]).map ? r("this%s={}", p.safeProp(t.name)) : t.repeated && r("this%s=[]", p.safeProp(t.name));
return r("if(p)for(var ks=Object.keys(p),i=0;i<ks.length;++i)if(p[ks[i]]!=null)")("this[ks[i]]=p[ks[i]]");
};
function w(e) {
e._fieldsById = e._fieldsArray = e._oneofsArray = null;
delete e.encode;
delete e.decode;
delete e.verify;
return e;
}
m.fromJSON = function(e, t) {
var r = new m(e, t.options);
r.extensions = t.extensions;
r.reserved = t.reserved;
for (var u = Object.keys(t.fields), c = 0; c < u.length; ++c) r.add(("undefined" != typeof t.fields[u[c]].keyType ? l.fromJSON : a.fromJSON)(u[c], t.fields[u[c]]));
if (t.oneofs) for (u = Object.keys(t.oneofs), c = 0; c < u.length; ++c) r.add(o.fromJSON(u[c], t.oneofs[u[c]]));
if (t.nested) for (u = Object.keys(t.nested), c = 0; c < u.length; ++c) {
var d = t.nested[u[c]];
r.add((d.id !== undefined ? a.fromJSON : d.fields !== undefined ? m.fromJSON : d.values !== undefined ? i.fromJSON : d.methods !== undefined ? s.fromJSON : n.fromJSON)(u[c], d));
}
t.extensions && t.extensions.length && (r.extensions = t.extensions);
t.reserved && t.reserved.length && (r.reserved = t.reserved);
t.group && (r.group = !0);
return r;
};
m.prototype.toJSON = function() {
var e = n.prototype.toJSON.call(this);
return p.toObject([ "options", e && e.options || undefined, "oneofs", n.arrayToJSON(this.oneofsArray), "fields", n.arrayToJSON(this.fieldsArray.filter(function(e) {
return !e.declaringField;
})) || {}, "extensions", this.extensions && this.extensions.length ? this.extensions : undefined, "reserved", this.reserved && this.reserved.length ? this.reserved : undefined, "group", this.group || undefined, "nested", e && e.nested || undefined ]);
};
m.prototype.resolveAll = function() {
for (var e = this.fieldsArray, t = 0; t < e.length; ) e[t++].resolve();
var r = this.oneofsArray;
t = 0;
for (;t < r.length; ) r[t++].resolve();
return n.prototype.resolveAll.call(this);
};
m.prototype.get = function(e) {
return this.fields[e] || this.oneofs && this.oneofs[e] || this.nested && this.nested[e] || null;
};
m.prototype.add = function(e) {
if (this.get(e.name)) throw Error("duplicate name '" + e.name + "' in " + this);
if (e instanceof a && e.extend === undefined) {
if (this._fieldsById ? this._fieldsById[e.id] : this.fieldsById[e.id]) throw Error("duplicate id " + e.id + " in " + this);
if (this.isReservedId(e.id)) throw Error("id " + e.id + " is reserved in " + this);
if (this.isReservedName(e.name)) throw Error("name '" + e.name + "' is reserved in " + this);
e.parent && e.parent.remove(e);
this.fields[e.name] = e;
e.message = this;
e.onAdd(this);
return w(this);
}
if (e instanceof o) {
this.oneofs || (this.oneofs = {});
this.oneofs[e.name] = e;
e.onAdd(this);
return w(this);
}
return n.prototype.add.call(this, e);
};
m.prototype.remove = function(e) {
if (e instanceof a && e.extend === undefined) {
if (!this.fields || this.fields[e.name] !== e) throw Error(e + " is not a member of " + this);
delete this.fields[e.name];
e.parent = null;
e.onRemove(this);
return w(this);
}
if (e instanceof o) {
if (!this.oneofs || this.oneofs[e.name] !== e) throw Error(e + " is not a member of " + this);
delete this.oneofs[e.name];
e.parent = null;
e.onRemove(this);
return w(this);
}
return n.prototype.remove.call(this, e);
};
m.prototype.isReservedId = function(e) {
return n.isReservedId(this.reserved, e);
};
m.prototype.isReservedName = function(e) {
return n.isReservedName(this.reserved, e);
};
m.prototype.create = function(e) {
return new this.ctor(e);
};
m.prototype.setup = function() {
for (var e = this.fullName, t = [], r = 0; r < this.fieldsArray.length; ++r) t.push(this._fieldsArray[r].resolve().resolvedType);
this.encode = f(this)({
Writer: d,
types: t,
util: p
});
this.decode = y(this)({
Reader: c,
types: t,
util: p
});
this.verify = h(this)({
types: t,
util: p
});
this.fromObject = g.fromObject(this)({
types: t,
util: p
});
this.toObject = g.toObject(this)({
types: t,
util: p
});
var n = b[e];
if (n) {
var i = Object.create(this);
i.fromObject = this.fromObject;
this.fromObject = n.fromObject.bind(i);
i.toObject = this.toObject;
this.toObject = n.toObject.bind(i);
}
return this;
};
m.prototype.encode = function(e, t) {
return this.setup().encode(e, t);
};
m.prototype.encodeDelimited = function(e, t) {
return this.encode(e, t && t.len ? t.fork() : t).ldelim();
};
m.prototype.decode = function(e, t) {
return this.setup().decode(e, t);
};
m.prototype.decodeDelimited = function(e) {
e instanceof c || (e = c.create(e));
return this.decode(e, e.uint32());
};
m.prototype.verify = function(e) {
return this.setup().verify(e);
};
m.prototype.fromObject = function(e) {
return this.setup().fromObject(e);
};
m.prototype.toObject = function(e, t) {
return this.setup().toObject(e, t);
};
m.d = function(e) {
return function(t) {
p.decorateType(t, e);
};
};
}, {
12: 12,
13: 13,
14: 14,
15: 15,
16: 16,
20: 20,
21: 21,
23: 23,
25: 25,
27: 27,
33: 33,
37: 37,
40: 40,
41: 41,
42: 42
} ],
36: [ function(e, t, r) {
var n = r, i = e(37), o = [ "double", "float", "int32", "uint32", "sint32", "fixed32", "sfixed32", "int64", "uint64", "sint64", "fixed64", "sfixed64", "bool", "string", "bytes" ];
function a(e, t) {
var r = 0, n = {};
t |= 0;
for (;r < e.length; ) n[o[r + t]] = e[r++];
return n;
}
n.basic = a([ 1, 5, 0, 0, 0, 5, 5, 0, 0, 0, 1, 1, 0, 2, 2 ]);
n.defaults = a([ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, !1, "", i.emptyArray, null ]);
n.long = a([ 0, 0, 0, 1, 1 ], 7);
n.mapKey = a([ 0, 0, 0, 5, 5, 0, 0, 0, 1, 1, 0, 2 ], 2);
n.packed = a([ 1, 5, 0, 0, 0, 5, 5, 0, 0, 0, 1, 1, 0 ]);
}, {
37: 37
} ],
37: [ function(e, t, r) {
var n, i, o = t.exports = e(39), a = e(30);
o.codegen = e(3);
o.fetch = e(5);
o.path = e(8);
o.fs = o.inquire("fs");
o.toArray = function(e) {
if (e) {
for (var t = Object.keys(e), r = new Array(t.length), n = 0; n < t.length; ) r[n] = e[t[n++]];
return r;
}
return [];
};
o.toObject = function(e) {
for (var t = {}, r = 0; r < e.length; ) {
var n = e[r++], i = e[r++];
i !== undefined && (t[n] = i);
}
return t;
};
var l = /\\/g, s = /"/g;
o.isReserved = function(e) {
return /^(?:do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import|public|return|static|switch|typeof|default|extends|finally|package|private|continue|debugger|function|arguments|interface|protected|implements|instanceof)$/.test(e);
};
o.safeProp = function(e) {
return !/^[$\w_]+$/.test(e) || o.isReserved(e) ? '["' + e.replace(l, "\\\\").replace(s, '\\"') + '"]' : "." + e;
};
o.ucFirst = function(e) {
return e.charAt(0).toUpperCase() + e.substring(1);
};
var u = /_([a-z])/g;
o.camelCase = function(e) {
return e.substring(0, 1) + e.substring(1).replace(u, function(e, t) {
return t.toUpperCase();
});
};
o.compareFieldsById = function(e, t) {
return e.id - t.id;
};
o.decorateType = function(t, r) {
if (t.$type) {
if (r && t.$type.name !== r) {
o.decorateRoot.remove(t.$type);
t.$type.name = r;
o.decorateRoot.add(t.$type);
}
return t.$type;
}
n || (n = e(35));
var i = new n(r || t.name);
o.decorateRoot.add(i);
i.ctor = t;
Object.defineProperty(t, "$type", {
value: i,
enumerable: !1
});
Object.defineProperty(t.prototype, "$type", {
value: i,
enumerable: !1
});
return i;
};
var c = 0;
o.decorateEnum = function(t) {
if (t.$type) return t.$type;
i || (i = e(15));
var r = new i("Enum" + c++, t);
o.decorateRoot.add(r);
Object.defineProperty(t, "$type", {
value: r,
enumerable: !1
});
return r;
};
Object.defineProperty(o, "decorateRoot", {
get: function() {
return a.decorated || (a.decorated = new (e(29))());
}
});
}, {
15: 15,
29: 29,
3: 3,
30: 30,
35: 35,
39: 39,
5: 5,
8: 8
} ],
38: [ function(e, t, r) {
t.exports = i;
var n = e(39);
function i(e, t) {
this.lo = e >>> 0;
this.hi = t >>> 0;
}
var o = i.zero = new i(0, 0);
o.toNumber = function() {
return 0;
};
o.zzEncode = o.zzDecode = function() {
return this;
};
o.length = function() {
return 1;
};
var a = i.zeroHash = "\0\0\0\0\0\0\0\0";
i.fromNumber = function(e) {
if (0 === e) return o;
var t = e < 0;
t && (e = -e);
var r = e >>> 0, n = (e - r) / 4294967296 >>> 0;
if (t) {
n = ~n >>> 0;
r = ~r >>> 0;
if (++r > 4294967295) {
r = 0;
++n > 4294967295 && (n = 0);
}
}
return new i(r, n);
};
i.from = function(e) {
if ("number" == typeof e) return i.fromNumber(e);
if (n.isString(e)) {
if (!n.Long) return i.fromNumber(parseInt(e, 10));
e = n.Long.fromString(e);
}
return e.low || e.high ? new i(e.low >>> 0, e.high >>> 0) : o;
};
i.prototype.toNumber = function(e) {
if (!e && this.hi >>> 31) {
var t = 1 + ~this.lo >>> 0, r = ~this.hi >>> 0;
t || (r = r + 1 >>> 0);
return -(t + 4294967296 * r);
}
return this.lo + 4294967296 * this.hi;
};
i.prototype.toLong = function(e) {
return n.Long ? new n.Long(0 | this.lo, 0 | this.hi, Boolean(e)) : {
low: 0 | this.lo,
high: 0 | this.hi,
unsigned: Boolean(e)
};
};
var l = String.prototype.charCodeAt;
i.fromHash = function(e) {
return e === a ? o : new i((l.call(e, 0) | l.call(e, 1) << 8 | l.call(e, 2) << 16 | l.call(e, 3) << 24) >>> 0, (l.call(e, 4) | l.call(e, 5) << 8 | l.call(e, 6) << 16 | l.call(e, 7) << 24) >>> 0);
};
i.prototype.toHash = function() {
return String.fromCharCode(255 & this.lo, this.lo >>> 8 & 255, this.lo >>> 16 & 255, this.lo >>> 24, 255 & this.hi, this.hi >>> 8 & 255, this.hi >>> 16 & 255, this.hi >>> 24);
};
i.prototype.zzEncode = function() {
var e = this.hi >> 31;
this.hi = ((this.hi << 1 | this.lo >>> 31) ^ e) >>> 0;
this.lo = (this.lo << 1 ^ e) >>> 0;
return this;
};
i.prototype.zzDecode = function() {
var e = -(1 & this.lo);
this.lo = ((this.lo >>> 1 | this.hi << 31) ^ e) >>> 0;
this.hi = (this.hi >>> 1 ^ e) >>> 0;
return this;
};
i.prototype.length = function() {
var e = this.lo, t = (this.lo >>> 28 | this.hi << 4) >>> 0, r = this.hi >>> 24;
return 0 === r ? 0 === t ? e < 16384 ? e < 128 ? 1 : 2 : e < 2097152 ? 3 : 4 : t < 16384 ? t < 128 ? 5 : 6 : t < 2097152 ? 7 : 8 : r < 128 ? 9 : 10;
};
}, {
39: 39
} ],
39: [ function(e, t, r) {
var n = r;
n.asPromise = e(1);
n.base64 = e(2);
n.EventEmitter = e(4);
n.float = e(6);
n.inquire = e(7);
n.utf8 = e(10);
n.pool = e(9);
n.LongBits = e(38);
n.emptyArray = Object.freeze ? Object.freeze([]) : [];
n.emptyObject = Object.freeze ? Object.freeze({}) : {};
n.isNode = Boolean(global.process && global.process.versions && global.process.versions.node);
n.isInteger = Number.isInteger || function(e) {
return "number" == typeof e && isFinite(e) && Math.floor(e) === e;
};
n.isString = function(e) {
return "string" == typeof e || e instanceof String;
};
n.isObject = function(e) {
return e && "object" == typeof e;
};
n.isset = n.isSet = function(e, t) {
var r = e[t];
return !(null == r || !e.hasOwnProperty(t)) && ("object" != typeof r || (Array.isArray(r) ? r.length : Object.keys(r).length) > 0);
};
n.Buffer = function() {
try {
var e = n.inquire("buffer").Buffer;
return e.prototype.utf8Write ? e : null;
} catch (e) {
return null;
}
}();
n._Buffer_from = null;
n._Buffer_allocUnsafe = null;
n.newBuffer = function(e) {
return "number" == typeof e ? n.Buffer ? n._Buffer_allocUnsafe(e) : new n.Array(e) : n.Buffer ? n._Buffer_from(e) : "undefined" == typeof Uint8Array ? e : new Uint8Array(e);
};
n.Array = "undefined" != typeof Uint8Array ? Uint8Array : Array;
n.Long = global.dcodeIO && global.dcodeIO.Long || n.inquire("long");
n.key2Re = /^true|false|0|1$/;
n.key32Re = /^-?(?:0|[1-9][0-9]*)$/;
n.key64Re = /^(?:[\\x00-\\xff]{8}|-?(?:0|[1-9][0-9]*))$/;
n.longToHash = function(e) {
return e ? n.LongBits.from(e).toHash() : n.LongBits.zeroHash;
};
n.longFromHash = function(e, t) {
var r = n.LongBits.fromHash(e);
return n.Long ? n.Long.fromBits(r.lo, r.hi, t) : r.toNumber(Boolean(t));
};
function i(e, t, r) {
for (var n = Object.keys(t), i = 0; i < n.length; ++i) e[n[i]] !== undefined && r || (e[n[i]] = t[n[i]]);
return e;
}
n.merge = i;
n.lcFirst = function(e) {
return e.charAt(0).toLowerCase() + e.substring(1);
};
function o(e) {
function t(e, r) {
if (!(this instanceof t)) return new t(e, r);
Object.defineProperty(this, "message", {
get: function() {
return e;
}
});
Error.captureStackTrace ? Error.captureStackTrace(this, t) : Object.defineProperty(this, "stack", {
value: new Error().stack || ""
});
r && i(this, r);
}
(t.prototype = Object.create(Error.prototype)).constructor = t;
Object.defineProperty(t.prototype, "name", {
get: function() {
return e;
}
});
t.prototype.toString = function() {
return this.name + ": " + this.message;
};
return t;
}
n.newError = o;
n.ProtocolError = o("ProtocolError");
n.oneOfGetter = function(e) {
for (var t = {}, r = 0; r < e.length; ++r) t[e[r]] = 1;
return function() {
for (var e = Object.keys(this), r = e.length - 1; r > -1; --r) if (1 === t[e[r]] && this[e[r]] !== undefined && null !== this[e[r]]) return e[r];
};
};
n.oneOfSetter = function(e) {
return function(t) {
for (var r = 0; r < e.length; ++r) e[r] !== t && delete this[e[r]];
};
};
n.toJSONOptions = {
longs: String,
enums: String,
bytes: String,
json: !0
};
n._configure = function() {
var e = n.Buffer;
if (e) {
n._Buffer_from = e.from !== Uint8Array.from && e.from || function(t, r) {
return new e(t, r);
};
n._Buffer_allocUnsafe = e.allocUnsafe || function(t) {
return new e(t);
};
} else n._Buffer_from = n._Buffer_allocUnsafe = null;
};
}, {
1: 1,
10: 10,
2: 2,
38: 38,
4: 4,
6: 6,
7: 7,
9: 9
} ],
40: [ function(e, t, r) {
t.exports = function(e) {
var t = i.codegen([ "m" ], e.name + "$verify")('if(typeof m!=="object"||m===null)')("return%j", "object expected"), r = {};
e.oneofsArray.length && t("var p={}");
for (var n = 0; n < e.fieldsArray.length; ++n) {
var s = e._fieldsArray[n].resolve(), u = "m" + i.safeProp(s.name);
s.optional && t("if(%s!=null&&m.hasOwnProperty(%j)){", u, s.name);
if (s.map) {
t("if(!util.isObject(%s))", u)("return%j", o(s, "object"))("var k=Object.keys(%s)", u)("for(var i=0;i<k.length;++i){");
l(t, s, "k[i]");
a(t, s, n, u + "[k[i]]")("}");
} else if (s.repeated) {
t("if(!Array.isArray(%s))", u)("return%j", o(s, "array"))("for(var i=0;i<%s.length;++i){", u);
a(t, s, n, u + "[i]")("}");
} else {
if (s.partOf) {
var c = i.safeProp(s.partOf.name);
1 === r[s.partOf.name] && t("if(p%s===1)", c)("return%j", s.partOf.name + ": multiple values");
r[s.partOf.name] = 1;
t("p%s=1", c);
}
a(t, s, n, u);
}
s.optional && t("}");
}
return t("return null");
};
var n = e(15), i = e(37);
function o(e, t) {
return e.name + ": " + t + (e.repeated && "array" !== t ? "[]" : e.map && "object" !== t ? "{k:" + e.keyType + "}" : "") + " expected";
}
function a(e, t, r, i) {
if (t.resolvedType) if (t.resolvedType instanceof n) {
e("switch(%s){", i)("default:")("return%j", o(t, "enum value"));
for (var a = Object.keys(t.resolvedType.values), l = 0; l < a.length; ++l) e("case %i:", t.resolvedType.values[a[l]]);
e("break")("}");
} else e("{")("var e=types[%i].verify(%s);", r, i)("if(e)")("return%j+e", t.name + ".")("}"); else switch (t.type) {
case "int32":
case "uint32":
case "sint32":
case "fixed32":
case "sfixed32":
e("if(!util.isInteger(%s))", i)("return%j", o(t, "integer"));
break;

case "int64":
case "uint64":
case "sint64":
case "fixed64":
case "sfixed64":
e("if(!util.isInteger(%s)&&!(%s&&util.isInteger(%s.low)&&util.isInteger(%s.high)))", i, i, i, i)("return%j", o(t, "integer|Long"));
break;

case "float":
case "double":
e('if(typeof %s!=="number")', i)("return%j", o(t, "number"));
break;

case "bool":
e('if(typeof %s!=="boolean")', i)("return%j", o(t, "boolean"));
break;

case "string":
e("if(!util.isString(%s))", i)("return%j", o(t, "string"));
break;

case "bytes":
e('if(!(%s&&typeof %s.length==="number"||util.isString(%s)))', i, i, i)("return%j", o(t, "buffer"));
}
return e;
}
function l(e, t, r) {
switch (t.keyType) {
case "int32":
case "uint32":
case "sint32":
case "fixed32":
case "sfixed32":
e("if(!util.key32Re.test(%s))", r)("return%j", o(t, "integer key"));
break;

case "int64":
case "uint64":
case "sint64":
case "fixed64":
case "sfixed64":
e("if(!util.key64Re.test(%s))", r)("return%j", o(t, "integer|Long key"));
break;

case "bool":
e("if(!util.key2Re.test(%s))", r)("return%j", o(t, "boolean key"));
}
return e;
}
}, {
15: 15,
37: 37
} ],
41: [ function(e, t, r) {
var n = r, i = e(21);
n[".google.protobuf.Any"] = {
fromObject: function(e) {
if (e && e["@type"]) {
var t = this.lookup(e["@type"]);
if (t) {
var r = "." === e["@type"].charAt(0) ? e["@type"].substr(1) : e["@type"];
return this.create({
type_url: "/" + r,
value: t.encode(t.fromObject(e)).finish()
});
}
}
return this.fromObject(e);
},
toObject: function(e, t) {
if (t && t.json && e.type_url && e.value) {
var r = e.type_url.substring(e.type_url.lastIndexOf("/") + 1), n = this.lookup(r);
n && (e = n.decode(e.value));
}
if (!(e instanceof this.ctor) && e instanceof i) {
var o = e.$type.toObject(e, t);
o["@type"] = e.$type.fullName;
return o;
}
return this.toObject(e, t);
}
};
}, {
21: 21
} ],
42: [ function(e, t, r) {
t.exports = d;
var n, i = e(39), o = i.LongBits, a = i.base64, l = i.utf8;
function s(e, t, r) {
this.fn = e;
this.len = t;
this.next = undefined;
this.val = r;
}
function u() {}
function c(e) {
this.head = e.head;
this.tail = e.tail;
this.len = e.len;
this.next = e.states;
}
function d() {
this.len = 0;
this.head = new s(u, 0, 0);
this.tail = this.head;
this.states = null;
}
d.create = i.Buffer ? function() {
return (d.create = function() {
return new n();
})();
} : function() {
return new d();
};
d.alloc = function(e) {
return new i.Array(e);
};
i.Array !== Array && (d.alloc = i.pool(d.alloc, i.Array.prototype.subarray));
d.prototype._push = function(e, t, r) {
this.tail = this.tail.next = new s(e, t, r);
this.len += t;
return this;
};
function p(e, t, r) {
t[r] = 255 & e;
}
function f(e, t) {
this.len = e;
this.next = undefined;
this.val = t;
}
f.prototype = Object.create(s.prototype);
f.prototype.fn = function(e, t, r) {
for (;e > 127; ) {
t[r++] = 127 & e | 128;
e >>>= 7;
}
t[r] = e;
};
d.prototype.uint32 = function(e) {
this.len += (this.tail = this.tail.next = new f((e >>>= 0) < 128 ? 1 : e < 16384 ? 2 : e < 2097152 ? 3 : e < 268435456 ? 4 : 5, e)).len;
return this;
};
d.prototype.int32 = function(e) {
return e < 0 ? this._push(y, 10, o.fromNumber(e)) : this.uint32(e);
};
d.prototype.sint32 = function(e) {
return this.uint32((e << 1 ^ e >> 31) >>> 0);
};
function y(e, t, r) {
for (;e.hi; ) {
t[r++] = 127 & e.lo | 128;
e.lo = (e.lo >>> 7 | e.hi << 25) >>> 0;
e.hi >>>= 7;
}
for (;e.lo > 127; ) {
t[r++] = 127 & e.lo | 128;
e.lo = e.lo >>> 7;
}
t[r++] = e.lo;
}
d.prototype.uint64 = function(e) {
var t = o.from(e);
return this._push(y, t.length(), t);
};
d.prototype.int64 = d.prototype.uint64;
d.prototype.sint64 = function(e) {
var t = o.from(e).zzEncode();
return this._push(y, t.length(), t);
};
d.prototype.bool = function(e) {
return this._push(p, 1, e ? 1 : 0);
};
function h(e, t, r) {
t[r] = 255 & e;
t[r + 1] = e >>> 8 & 255;
t[r + 2] = e >>> 16 & 255;
t[r + 3] = e >>> 24;
}
d.prototype.fixed32 = function(e) {
return this._push(h, 4, e >>> 0);
};
d.prototype.sfixed32 = d.prototype.fixed32;
d.prototype.fixed64 = function(e) {
var t = o.from(e);
return this._push(h, 4, t.lo)._push(h, 4, t.hi);
};
d.prototype.sfixed64 = d.prototype.fixed64;
d.prototype.float = function(e) {
return this._push(i.float.writeFloatLE, 4, e);
};
d.prototype.double = function(e) {
return this._push(i.float.writeDoubleLE, 8, e);
};
var g = i.Array.prototype.set ? function(e, t, r) {
t.set(e, r);
} : function(e, t, r) {
for (var n = 0; n < e.length; ++n) t[r + n] = e[n];
};
d.prototype.bytes = function(e) {
var t = e.length >>> 0;
if (!t) return this._push(p, 1, 0);
if (i.isString(e)) {
var r = d.alloc(t = a.length(e));
a.decode(e, r, 0);
e = r;
}
return this.uint32(t)._push(g, t, e);
};
d.prototype.string = function(e) {
var t = l.length(e);
return t ? this.uint32(t)._push(l.write, t, e) : this._push(p, 1, 0);
};
d.prototype.fork = function() {
this.states = new c(this);
this.head = this.tail = new s(u, 0, 0);
this.len = 0;
return this;
};
d.prototype.reset = function() {
if (this.states) {
this.head = this.states.head;
this.tail = this.states.tail;
this.len = this.states.len;
this.states = this.states.next;
} else {
this.head = this.tail = new s(u, 0, 0);
this.len = 0;
}
return this;
};
d.prototype.ldelim = function() {
var e = this.head, t = this.tail, r = this.len;
this.reset().uint32(r);
if (r) {
this.tail.next = e.next;
this.tail = t;
this.len += r;
}
return this;
};
d.prototype.finish = function() {
for (var e = this.head.next, t = this.constructor.alloc(this.len), r = 0; e; ) {
e.fn(e.val, t, r);
r += e.len;
e = e.next;
}
return t;
};
d._configure = function(e) {
n = e;
};
}, {
39: 39
} ],
43: [ function(e, t, r) {
t.exports = a;
var n = e(42);
(a.prototype = Object.create(n.prototype)).constructor = a;
var i = e(39), o = i.Buffer;
function a() {
n.call(this);
}
a.alloc = function(e) {
return (a.alloc = i._Buffer_allocUnsafe)(e);
};
var l = o && o.prototype instanceof Uint8Array && "set" === o.prototype.set.name ? function(e, t, r) {
t.set(e, r);
} : function(e, t, r) {
if (e.copy) e.copy(t, r, 0, e.length); else for (var n = 0; n < e.length; ) t[r++] = e[n++];
};
a.prototype.bytes = function(e) {
i.isString(e) && (e = i._Buffer_from(e, "base64"));
var t = e.length >>> 0;
this.uint32(t);
t && this._push(l, t, e);
return this;
};
function s(e, t, r) {
e.length < 40 ? i.utf8.write(e, t, r) : t.utf8Write(e, r);
}
a.prototype.string = function(e) {
var t = o.byteLength(e);
this.uint32(t);
t && this._push(s, t, e);
return this;
};
}, {
39: 39,
42: 42
} ]
}, {}, [ 19 ]);
})("object" == typeof window && window || "object" == typeof self && self || this);

var $protobuf = window.protobuf, $Reader = ($protobuf.roots.default = window, $protobuf.Reader), $Writer = $protobuf.Writer, $util = $protobuf.util, $root = $protobuf.roots.default || ($protobuf.roots.default = {});

$root.pb = function() {
var e, t, r = {};
function n(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function i(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function o(e) {
if (this.IDList = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function a(e) {
if (this.Entrances = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function l(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function s(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function u(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function c(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function d(e) {
if (this.RewardList = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function p(e) {
if (this.Param = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function f(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function y(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function h(e) {
if (this.RewardList = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function g(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function b(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function m(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function w(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function v(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function O(e) {
if (this.Cooldowns = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function $(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function R(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function I(e) {
if (this.Announcements = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function k(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function P(e) {
if (this.Ids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function _(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function x(e) {
if (this.Fighters = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function A(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function j(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function S(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function T(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function C(e) {
if (this.Options = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function L(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function D(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function G(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function E(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function W(e) {
if (this.Times = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function H(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function B(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function U(e) {
if (this.Options = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function N(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function F(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function M(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function V(e) {
if (this.FreeReward = [], this.PayReward1 = [], this.PayReward2 = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function q(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function J(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function X(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Y(e) {
if (this.OpenBPIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function z(e) {
if (this.Boxes = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function K(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Q(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Z(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ee(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function te(e) {
if (this.Gems = [], this.WearGems = [], this.HeroWearGems = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function re(e) {
if (this.Ids = [], this.Attributes = [], this.ExtraIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ne(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ie(e) {
if (this.Ids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function oe(e) {
if (this.WearGems = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ae(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function le(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function se(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ue(e) {
if (this.Gems = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ce(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function de(e) {
if (this.Gids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function pe(e) {
if (this.Gems = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function fe(e) {
if (this.Gems = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ye(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function he(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ge(e) {
if (this.Gids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function be(e) {
if (this.Gems = [], this.Groups = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function me(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function we(e) {
if (this.Attributes = [], this.ExtraIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ve(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Oe(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function $e(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Re(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ie(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ke(e) {
if (this.HeadIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Pe(e) {
if (this.Talents = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function _e(e) {
if (this.Heros = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function xe(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ae(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function je(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Se(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Te(e) {
if (this.HeroConfigs = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ce(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Le(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function De(e) {
if (this.Students = [], this.OnceStudents = [], this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ge(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ee(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function We(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function He(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Be(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ue(e) {
if (this.Events = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ne(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Fe(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Me(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ve(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function qe(e) {
if (this.XlsDatas = [], this.Monster = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Je(e) {
if (this.Heros = [], this.UpgradeOptions = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Xe(e) {
if (this.AllIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ye(e) {
if (this.AllIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ze(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ke(e) {
if (this.ClearRewardStatus = [], this.BanHeroIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Qe(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ze(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function et(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function tt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function rt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function nt(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function it(e) {
if (this.Equips = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ot(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function at(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function lt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function st(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ut(e) {
if (this.class = [], this.valid = [], this.skill = [], this.attribute_correct = [], 
this.attaack_effect = [], this.summoned = [], this.label = [], this.sound1 = [], 
this.sound2 = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ct(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function dt(e) {
if (this.type = [], this.class = [], this.valid = [], this.skill = [], this.attaack_effect = [], 
this.def = [], this.characteristic = [], this.action = [], this.sound1 = [], this.sound2 = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function pt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ft(e) {
if (this.type = [], this.class = [], this.valid = [], this.skill = [], this.attaack_effect = [], 
this.def = [], this.characteristic = [], this.action = [], this.sound1 = [], this.sound2 = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function yt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ht(e) {
if (this.triggerOpp = [], this.skillGroupType = [], this.variable = [], this.condition = [], 
this.aim = [], this.damage = [], this.damage_range = [], this.move = [], this.movie_effect = [], 
this.effect_change = [], this.statusChange = [], this.statusReplace = [], this.parameters = [], 
this.sound = [], this.actionOth = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function gt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function bt(e) {
if (this.type = [], this.targetStatus = [], this.notargetStatus = [], this.targetHp = [], 
this.targetClass = [], this.targetEntity = [], this.notargetEntity = [], this.targetGrade = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function mt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function wt(e) {
if (this.bufftype = [], this.variable = [], this.condition = [], this.silent = [], 
this.effect = [], this.skillChange = [], this.resistanceType = [], this.attributeRev = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function vt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ot(e) {
if (this.unlock = [], this.premise1 = [], this.premise2 = [], this.aim_skill = [], 
this.up_skill = [], this.change_actionOth = [], this.change_addStatus = [], this.summon = [], 
this.soldiers_change = [], this.soldiers_upgrade = [], this.soldiers_skill = [], 
this.type = [], this.summon2 = [], this.attribute_correct = [], this.attribute_correct2 = [], 
this.equip_skill = [], this.numTable = [], this.numTxt = [], this.icon2 = [], this.aim_type = [], 
this.type2 = [], this.skillGroupAdd = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function $t(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Rt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function It(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function kt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Pt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function _t(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function xt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function At(e) {
if (this.equip_skill = [], this.upgrade = [], this.attribute_correct = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function jt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function St(e) {
if (this.position = [], this.limit = [], this.value1 = [], this.merge = [], this.upgrade = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Tt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ct(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Lt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Dt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Gt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Et(e) {
if (this.enemy_group = [], this.boss_group = [], this.waves_group = [], this.waves_num = [], 
this.enemy_num = [], this.wave_interval = [], this.grow_up = [], this.rewardFirst1 = [], 
this.rewardFirst2 = [], this.rewardFirst3 = [], this.premise_wave = [], this.reward = [], 
this.reward2 = [], this.grow_up_hp = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Wt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ht(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Bt(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ut(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Nt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ft(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Mt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Vt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function qt(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Jt(e) {
if (this.Mails = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Xt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Yt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function zt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Kt(e) {
if (this.MailIds = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Qt(e) {
if (this.upgrades = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Zt(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function er(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function tr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function rr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function nr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ir(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function or(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ar(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function lr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function sr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ur(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function cr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function dr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function pr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function fr(e) {
if (this.XlsVersions = {}, e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function yr(e) {
if (this.Res = [], this.XlsDatas = [], this.Hints = [], this.Levels = [], this.Heros = [], 
this.CanShowHeads = [], this.AdvertCooldowns = [], this.Statuses = [], this.XlsVersions = {}, 
this.OpenBPIds = [], this.ChallengeLevels = [], this.ActEntrances = [], this.HeroConfigs = [], 
e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function hr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function gr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function br(e) {
if (this.Players = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function mr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function wr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function vr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Or(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function $r(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Rr(e) {
if (this.Res = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ir(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function kr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Pr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function _r(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function xr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ar(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function jr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Sr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Tr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Cr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Lr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Dr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Gr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Er(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Wr(e) {
if (this.Monster = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Hr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Br(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ur(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Nr(e) {
if (this.Heros = [], this.WearRunes = [], this.WearGems = [], this.HeroWearGems = [], 
this.ChallengeLevels = [], this.Ranks = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Fr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Mr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Vr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function qr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Jr(e) {
if (this.Skills = [], this.Entrys = [], this.SkillOptions = [], this.EntryOptions = [], 
this.Talents = [], this.Skins = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Xr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Yr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function zr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Kr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Qr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Zr(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function en(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function tn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function rn(e) {
if (this.Attributes = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function nn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function on(e) {
if (this.WearRuneGids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function an(e) {
if (this.Runes = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ln(e) {
if (this.Runes = [], this.WearRunes = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function sn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function un(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function cn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function dn(e) {
if (this.Gids = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function pn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function fn(e) {
if (this.Boxs = [], this.Resources = [], this.Jades = [], this.Coins = [], this.Vips = [], 
this.Gifts = [], this.PayGifts = [], this.LevelGifts = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function yn(e) {
if (this.GuaranteedReward = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function hn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function gn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function bn(e) {
if (this.Boxs = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function mn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function wn(e) {
if (this.Resources = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function vn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function On(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function $n(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Rn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function In(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function kn(e) {
if (this.Jades = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Pn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function _n(e) {
if (this.Coins = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function xn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function An(e) {
if (this.Vips = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function jn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Sn(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Tn(e) {
if (this.Gifts = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Cn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Ln(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Dn(e) {
if (this.Goods = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Gn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function En(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Wn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Hn(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Bn(e) {
if (this.Gifts = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Un(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Nn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Fn(e) {
if (this.WorkShops = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Mn(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Vn(e) {
if (this.Rewards = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function qn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Jn(e) {
if (this.RewardPreview = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Xn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Yn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function zn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Kn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Qn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function Zn(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ei(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ti(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ri(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ni(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ii(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function oi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ai(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function li(e) {
if (this.Items = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function si(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ui(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function ci(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function di(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function pi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function fi(e) {
if (this.Friends = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function yi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function hi(e) {
if (this.FriendApplys = [], e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function gi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function bi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
function mi(e) {
if (e) for (var t = Object.keys(e), r = 0; r < t.length; ++r) null != e[t[r]] && (this[t[r]] = e[t[r]]);
}
return r.GetDouyinRewardArg = (n.prototype.Type = 0, n.create = function(e) {
return new n(e);
}, n.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
t;
}, n.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, n.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetDouyinRewardArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Type = e.int32() : e.skipType(7 & i);
}
return n;
}, n.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, n.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, n), r.FetchActLabelListArg = (i.prototype.Enter = 0, i.create = function(e) {
return new i(e);
}, i.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Enter && Object.hasOwnProperty.call(e, "Enter") && t.uint32(8).int32(e.Enter), 
t;
}, i.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, i.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchActLabelListArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Enter = e.int32() : e.skipType(7 & i);
}
return n;
}, i.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, i.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Enter && e.hasOwnProperty("Enter") && !$util.isInteger(e.Enter) ? "Enter: integer expected" : null;
}, i), r.ActivityLabelList = (o.prototype.IDList = $util.emptyArray, o.create = function(e) {
return new o(e);
}, o.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.IDList && e.IDList.length) {
t.uint32(10).fork();
for (var r = 0; r < e.IDList.length; ++r) t.int32(e.IDList[r]);
t.ldelim();
}
return t;
}, o.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, o.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityLabelList(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.IDList && n.IDList.length || (n.IDList = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.IDList.push(e.int32()); else n.IDList.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, o.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, o.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.IDList && e.hasOwnProperty("IDList")) {
if (!Array.isArray(e.IDList)) return "IDList: array expected";
for (var t = 0; t < e.IDList.length; ++t) if (!$util.isInteger(e.IDList[t])) return "IDList: integer[] expected";
}
return null;
}, o), r.UpdateEntranceArg = (a.prototype.Entrances = $util.emptyArray, a.create = function(e) {
return new a(e);
}, a.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Entrances && e.Entrances.length) for (var r = 0; r < e.Entrances.length; ++r) $root.pb.ActEntrance.encode(e.Entrances[r], t.uint32(10).fork()).ldelim();
return t;
}, a.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, a.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateEntranceArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Entrances && n.Entrances.length || (n.Entrances = []), n.Entrances.push($root.pb.ActEntrance.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, a.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, a.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Entrances && e.hasOwnProperty("Entrances")) {
if (!Array.isArray(e.Entrances)) return "Entrances: array expected";
for (var t = 0; t < e.Entrances.length; ++t) {
var r = $root.pb.ActEntrance.verify(e.Entrances[t]);
if (r) return "Entrances." + r;
}
}
return null;
}, a), r.ActEntrance = (l.prototype.Enter = 0, l.prototype.RemainTime = 0, l.create = function(e) {
return new l(e);
}, l.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Enter && Object.hasOwnProperty.call(e, "Enter") && t.uint32(8).int32(e.Enter), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(16).int32(e.RemainTime), 
t;
}, l.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, l.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActEntrance(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Enter = e.int32();
break;

case 2:
n.RemainTime = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, l.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, l.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Enter && e.hasOwnProperty("Enter") && !$util.isInteger(e.Enter) ? "Enter: integer expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, l), r.ActivityReceiveStatus = (e = {}, (t = Object.create(e))[e[0] = "ActivityUnknow"] = 0, 
t[e[1] = "CanReceive"] = 1, t[e[2] = "HasReceive"] = 2, t[e[3] = "CanNotReceive"] = 3, 
t[e[4] = "AlreadyOwned"] = 4, t), r.TargetActivity = (s.prototype.Id = 0, s.create = function(e) {
return new s(e);
}, s.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
t;
}, s.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, s.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetActivity(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.int32() : e.skipType(7 & i);
}
return n;
}, s.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, s.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, s), r.ActivityData = (u.prototype.Id = 0, u.prototype.Data = $util.newBuffer([]), 
u.prototype.RemainTime = 0, u.create = function(e) {
return new u(e);
}, u.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Data && Object.hasOwnProperty.call(e, "Data") && t.uint32(18).bytes(e.Data), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(24).int32(e.RemainTime), 
t;
}, u.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, u.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityData(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Data = e.bytes();
break;

case 3:
n.RemainTime = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, u.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, u.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Data && e.hasOwnProperty("Data") && !(e.Data && "number" == typeof e.Data.length || $util.isString(e.Data)) ? "Data: buffer expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, u), r.ActivityRewardList = (c.prototype.Rewards = $util.emptyArray, c.create = function(e) {
return new c(e);
}, c.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) $root.pb.ActivityReward.encode(e.Rewards[r], t.uint32(10).fork()).ldelim();
return t;
}, c.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, c.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityRewardList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.ActivityReward.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, c.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, c.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) {
var r = $root.pb.ActivityReward.verify(e.Rewards[t]);
if (r) return "Rewards." + r;
}
}
return null;
}, c), r.ActivityReward = (d.prototype.Rid = 0, d.prototype.ReceiveStatus = 0, d.prototype.RewardCondition = "", 
d.prototype.RewardList = $util.emptyArray, d.create = function(e) {
return new d(e);
}, d.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rid && Object.hasOwnProperty.call(e, "Rid") && t.uint32(8).int32(e.Rid), 
null != e.ReceiveStatus && Object.hasOwnProperty.call(e, "ReceiveStatus") && t.uint32(16).int32(e.ReceiveStatus), 
null != e.RewardCondition && Object.hasOwnProperty.call(e, "RewardCondition") && t.uint32(26).string(e.RewardCondition), 
null != e.RewardList && e.RewardList.length) for (var r = 0; r < e.RewardList.length; ++r) t.uint32(34).string(e.RewardList[r]);
return t;
}, d.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, d.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityReward(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Rid = e.int32();
break;

case 2:
n.ReceiveStatus = e.int32();
break;

case 3:
n.RewardCondition = e.string();
break;

case 4:
n.RewardList && n.RewardList.length || (n.RewardList = []), n.RewardList.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, d.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, d.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rid && e.hasOwnProperty("Rid") && !$util.isInteger(e.Rid)) return "Rid: integer expected";
if (null != e.ReceiveStatus && e.hasOwnProperty("ReceiveStatus")) switch (e.ReceiveStatus) {
default:
return "ReceiveStatus: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
if (null != e.RewardCondition && e.hasOwnProperty("RewardCondition") && !$util.isString(e.RewardCondition)) return "RewardCondition: string expected";
if (null != e.RewardList && e.hasOwnProperty("RewardList")) {
if (!Array.isArray(e.RewardList)) return "RewardList: array expected";
for (var t = 0; t < e.RewardList.length; ++t) if (!$util.isString(e.RewardList[t])) return "RewardList: string[] expected";
}
return null;
}, d), r.GetActivityRewardArg = (p.prototype.Id = 0, p.prototype.Rid = 0, p.prototype.Param = $util.emptyArray, 
p.create = function(e) {
return new p(e);
}, p.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Rid && Object.hasOwnProperty.call(e, "Rid") && t.uint32(16).int32(e.Rid), 
null != e.Param && e.Param.length) {
t.uint32(26).fork();
for (var r = 0; r < e.Param.length; ++r) t.int32(e.Param[r]);
t.ldelim();
}
return t;
}, p.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, p.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetActivityRewardArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Rid = e.int32();
break;

case 3:
if (n.Param && n.Param.length || (n.Param = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Param.push(e.int32()); else n.Param.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, p.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, p.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Rid && e.hasOwnProperty("Rid") && !$util.isInteger(e.Rid)) return "Rid: integer expected";
if (null != e.Param && e.hasOwnProperty("Param")) {
if (!Array.isArray(e.Param)) return "Param: array expected";
for (var t = 0; t < e.Param.length; ++t) if (!$util.isInteger(e.Param[t])) return "Param: integer[] expected";
}
return null;
}, p), r.GetActivityRewardReply = (f.prototype.Activity = null, f.prototype.Reward = null, 
f.create = function(e) {
return new f(e);
}, f.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Activity && Object.hasOwnProperty.call(e, "Activity") && $root.pb.ActivityData.encode(e.Activity, t.uint32(10).fork()).ldelim(), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
t;
}, f.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, f.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetActivityRewardReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Activity = $root.pb.ActivityData.decode(e, e.uint32());
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, f.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, f.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Activity && e.hasOwnProperty("Activity") && (t = $root.pb.ActivityData.verify(e.Activity)) ? "Activity." + t : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null;
var t;
}, f), r.ActivityDiscountData = (y.prototype.UnlockRid = 0, y.prototype.BeginDay = 0, 
y.prototype.Rewards = $util.emptyArray, y.create = function(e) {
return new y(e);
}, y.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.UnlockRid && Object.hasOwnProperty.call(e, "UnlockRid") && t.uint32(8).int32(e.UnlockRid), 
null != e.BeginDay && Object.hasOwnProperty.call(e, "BeginDay") && t.uint32(16).int32(e.BeginDay), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) $root.pb.ActivityDiscount.encode(e.Rewards[r], t.uint32(26).fork()).ldelim();
return t;
}, y.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, y.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityDiscountData(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.UnlockRid = e.int32();
break;

case 2:
n.BeginDay = e.int32();
break;

case 3:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.ActivityDiscount.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, y.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, y.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.UnlockRid && e.hasOwnProperty("UnlockRid") && !$util.isInteger(e.UnlockRid)) return "UnlockRid: integer expected";
if (null != e.BeginDay && e.hasOwnProperty("BeginDay") && !$util.isInteger(e.BeginDay)) return "BeginDay: integer expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) {
var r = $root.pb.ActivityDiscount.verify(e.Rewards[t]);
if (r) return "Rewards." + r;
}
}
return null;
}, y), r.ActivityDiscount = (h.prototype.Rid = 0, h.prototype.ReceiveStatus = 0, 
h.prototype.RewardCondition = "", h.prototype.RewardList = $util.emptyArray, h.prototype.Delay = 0, 
h.prototype.IsCirculate = !1, h.create = function(e) {
return new h(e);
}, h.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rid && Object.hasOwnProperty.call(e, "Rid") && t.uint32(8).int32(e.Rid), 
null != e.ReceiveStatus && Object.hasOwnProperty.call(e, "ReceiveStatus") && t.uint32(16).int32(e.ReceiveStatus), 
null != e.RewardCondition && Object.hasOwnProperty.call(e, "RewardCondition") && t.uint32(26).string(e.RewardCondition), 
null != e.RewardList && e.RewardList.length) for (var r = 0; r < e.RewardList.length; ++r) t.uint32(34).string(e.RewardList[r]);
return null != e.Delay && Object.hasOwnProperty.call(e, "Delay") && t.uint32(40).int32(e.Delay), 
null != e.IsCirculate && Object.hasOwnProperty.call(e, "IsCirculate") && t.uint32(48).bool(e.IsCirculate), 
t;
}, h.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, h.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ActivityDiscount(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Rid = e.int32();
break;

case 2:
n.ReceiveStatus = e.int32();
break;

case 3:
n.RewardCondition = e.string();
break;

case 4:
n.RewardList && n.RewardList.length || (n.RewardList = []), n.RewardList.push(e.string());
break;

case 5:
n.Delay = e.int32();
break;

case 6:
n.IsCirculate = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, h.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, h.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rid && e.hasOwnProperty("Rid") && !$util.isInteger(e.Rid)) return "Rid: integer expected";
if (null != e.ReceiveStatus && e.hasOwnProperty("ReceiveStatus")) switch (e.ReceiveStatus) {
default:
return "ReceiveStatus: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
if (null != e.RewardCondition && e.hasOwnProperty("RewardCondition") && !$util.isString(e.RewardCondition)) return "RewardCondition: string expected";
if (null != e.RewardList && e.hasOwnProperty("RewardList")) {
if (!Array.isArray(e.RewardList)) return "RewardList: array expected";
for (var t = 0; t < e.RewardList.length; ++t) if (!$util.isString(e.RewardList[t])) return "RewardList: string[] expected";
}
return null != e.Delay && e.hasOwnProperty("Delay") && !$util.isInteger(e.Delay) ? "Delay: integer expected" : null != e.IsCirculate && e.hasOwnProperty("IsCirculate") && "boolean" != typeof e.IsCirculate ? "IsCirculate: boolean expected" : null;
}, h), r.Resource = (g.prototype.Type = 0, g.prototype.Amount = 0, g.create = function(e) {
return new g(e);
}, g.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.Amount && Object.hasOwnProperty.call(e, "Amount") && t.uint32(16).int32(e.Amount), 
t;
}, g.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, g.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Resource(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.Amount = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, g.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, g.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null != e.Amount && e.hasOwnProperty("Amount") && !$util.isInteger(e.Amount) ? "Amount: integer expected" : null;
}, g), r.ServerStatus = (b.prototype.Status = 0, b.prototype.Message = "", b.create = function(e) {
return new b(e);
}, b.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Status && Object.hasOwnProperty.call(e, "Status") && t.uint32(8).int32(e.Status), 
null != e.Message && Object.hasOwnProperty.call(e, "Message") && t.uint32(18).string(e.Message), 
t;
}, b.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, b.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ServerStatus(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Status = e.int32();
break;

case 2:
n.Message = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, b.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, b.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Status && e.hasOwnProperty("Status")) switch (e.Status) {
default:
return "Status: enum value expected";

case 0:
case 1:
}
return null != e.Message && e.hasOwnProperty("Message") && !$util.isString(e.Message) ? "Message: string expected" : null;
}, b.StatusEnum = (e = {}, (t = Object.create(e))[e[0] = "Normal"] = 0, t[e[1] = "Maintain"] = 1, 
t), b), r.RewardListItem = (m.prototype.Type = "", m.prototype.Id = "", m.prototype.Amount = 0, 
m.create = function(e) {
return new m(e);
}, m.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(10).string(e.Type), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(18).string(e.Id), 
null != e.Amount && Object.hasOwnProperty.call(e, "Amount") && t.uint32(24).int32(e.Amount), 
t;
}, m.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, m.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RewardListItem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.string();
break;

case 2:
n.Id = e.string();
break;

case 3:
n.Amount = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, m.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, m.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isString(e.Type) ? "Type: string expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null != e.Amount && e.hasOwnProperty("Amount") && !$util.isInteger(e.Amount) ? "Amount: integer expected" : null;
}, m), r.RewardList = (w.prototype.Rewards = $util.emptyArray, w.create = function(e) {
return new w(e);
}, w.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) $root.pb.RewardListItem.encode(e.Rewards[r], t.uint32(10).fork()).ldelim();
return t;
}, w.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, w.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RewardList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.RewardListItem.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, w.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, w.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) {
var r = $root.pb.RewardListItem.verify(e.Rewards[t]);
if (r) return "Rewards." + r;
}
}
return null;
}, w), r.AdvertType = (e = {}, (t = Object.create(e))[e[0] = "AdtUnknow"] = 0, t[e[1] = "AdtShopRes"] = 1, 
t[e[2] = "AdtSpeedPass"] = 2, t[e[3] = "AdtShopBox"] = 3, t[e[4] = "AdtShopRandRes"] = 4, 
t), r.AdvertCooldown = (v.prototype.Type = 0, v.prototype.ModelId = "", v.prototype.Cd = 0, 
v.create = function(e) {
return new v(e);
}, v.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.ModelId && Object.hasOwnProperty.call(e, "ModelId") && t.uint32(18).string(e.ModelId), 
null != e.Cd && Object.hasOwnProperty.call(e, "Cd") && t.uint32(24).int32(e.Cd), 
t;
}, v.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, v.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AdvertCooldown(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.ModelId = e.string();
break;

case 3:
n.Cd = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, v.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, v.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Type && e.hasOwnProperty("Type")) switch (e.Type) {
default:
return "Type: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
return null != e.ModelId && e.hasOwnProperty("ModelId") && !$util.isString(e.ModelId) ? "ModelId: string expected" : null != e.Cd && e.hasOwnProperty("Cd") && !$util.isInteger(e.Cd) ? "Cd: integer expected" : null;
}, v), r.AdvertCooldownList = (O.prototype.Cooldowns = $util.emptyArray, O.create = function(e) {
return new O(e);
}, O.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Cooldowns && e.Cooldowns.length) for (var r = 0; r < e.Cooldowns.length; ++r) $root.pb.AdvertCooldown.encode(e.Cooldowns[r], t.uint32(10).fork()).ldelim();
return t;
}, O.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, O.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AdvertCooldownList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Cooldowns && n.Cooldowns.length || (n.Cooldowns = []), n.Cooldowns.push($root.pb.AdvertCooldown.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, O.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, O.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Cooldowns && e.hasOwnProperty("Cooldowns")) {
if (!Array.isArray(e.Cooldowns)) return "Cooldowns: array expected";
for (var t = 0; t < e.Cooldowns.length; ++t) {
var r = $root.pb.AdvertCooldown.verify(e.Cooldowns[t]);
if (r) return "Cooldowns." + r;
}
}
return null;
}, O), r.WatchAdvertArg = ($.prototype.Type = 0, $.prototype.ModelId = "", $.prototype.Skip = !1, 
$.create = function(e) {
return new $(e);
}, $.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.ModelId && Object.hasOwnProperty.call(e, "ModelId") && t.uint32(18).string(e.ModelId), 
null != e.Skip && Object.hasOwnProperty.call(e, "Skip") && t.uint32(24).bool(e.Skip), 
t;
}, $.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, $.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WatchAdvertArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.ModelId = e.string();
break;

case 3:
n.Skip = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, $.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, $.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Type && e.hasOwnProperty("Type")) switch (e.Type) {
default:
return "Type: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
return null != e.ModelId && e.hasOwnProperty("ModelId") && !$util.isString(e.ModelId) ? "ModelId: string expected" : null != e.Skip && e.hasOwnProperty("Skip") && "boolean" != typeof e.Skip ? "Skip: boolean expected" : null;
}, $), r.Announcement = (R.prototype.Id = "", R.prototype.Title = "", R.prototype.Content = "", 
R.prototype.IsRead = !1, R.create = function(e) {
return new R(e);
}, R.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
null != e.Title && Object.hasOwnProperty.call(e, "Title") && t.uint32(18).string(e.Title), 
null != e.Content && Object.hasOwnProperty.call(e, "Content") && t.uint32(26).string(e.Content), 
null != e.IsRead && Object.hasOwnProperty.call(e, "IsRead") && t.uint32(32).bool(e.IsRead), 
t;
}, R.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, R.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Announcement(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.string();
break;

case 2:
n.Title = e.string();
break;

case 3:
n.Content = e.string();
break;

case 4:
n.IsRead = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, R.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, R.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null != e.Title && e.hasOwnProperty("Title") && !$util.isString(e.Title) ? "Title: string expected" : null != e.Content && e.hasOwnProperty("Content") && !$util.isString(e.Content) ? "Content: string expected" : null != e.IsRead && e.hasOwnProperty("IsRead") && "boolean" != typeof e.IsRead ? "IsRead: boolean expected" : null;
}, R), r.AnnouncementList = (I.prototype.Announcements = $util.emptyArray, I.create = function(e) {
return new I(e);
}, I.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Announcements && e.Announcements.length) for (var r = 0; r < e.Announcements.length; ++r) $root.pb.Announcement.encode(e.Announcements[r], t.uint32(10).fork()).ldelim();
return t;
}, I.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, I.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AnnouncementList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Announcements && n.Announcements.length || (n.Announcements = []), 
n.Announcements.push($root.pb.Announcement.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, I.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, I.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Announcements && e.hasOwnProperty("Announcements")) {
if (!Array.isArray(e.Announcements)) return "Announcements: array expected";
for (var t = 0; t < e.Announcements.length; ++t) {
var r = $root.pb.Announcement.verify(e.Announcements[t]);
if (r) return "Announcements." + r;
}
}
return null;
}, I), r.TargetAnnouncement = (k.prototype.Id = "", k.create = function(e) {
return new k(e);
}, k.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
t;
}, k.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, k.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetAnnouncement(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.string() : e.skipType(7 & i);
}
return n;
}, k.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, k.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null;
}, k), r.BatchReadAnnouncement = (P.prototype.Ids = $util.emptyArray, P.create = function(e) {
return new P(e);
}, P.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Ids && e.Ids.length) for (var r = 0; r < e.Ids.length; ++r) t.uint32(10).string(e.Ids[r]);
return t;
}, P.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, P.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BatchReadAnnouncement(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Ids && n.Ids.length || (n.Ids = []), n.Ids.push(e.string())) : e.skipType(7 & i);
}
return n;
}, P.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, P.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Ids && e.hasOwnProperty("Ids")) {
if (!Array.isArray(e.Ids)) return "Ids: array expected";
for (var t = 0; t < e.Ids.length; ++t) if (!$util.isString(e.Ids[t])) return "Ids: string[] expected";
}
return null;
}, P), r.BeginBattle = (_.prototype.Battle = null, _.create = function(e) {
return new _(e);
}, _.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Battle && Object.hasOwnProperty.call(e, "Battle") && $root.pb.Battle.encode(e.Battle, t.uint32(10).fork()).ldelim(), 
t;
}, _.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, _.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BeginBattle(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Battle = $root.pb.Battle.decode(e, e.uint32()) : e.skipType(7 & i);
}
return n;
}, _.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, _.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Battle && e.hasOwnProperty("Battle") && (e = $root.pb.Battle.verify(e.Battle)) ? "Battle." + e : null;
}, _), r.Battle = (x.prototype.Id = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
x.prototype.Type = 0, x.prototype.Fighters = $util.emptyArray, x.prototype.Level = 0, 
x.create = function(e) {
return new x(e);
}, x.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int64(e.Id), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(16).int32(e.Type), 
null != e.Fighters && e.Fighters.length) for (var r = 0; r < e.Fighters.length; ++r) $root.pb.Fighter.encode(e.Fighters[r], t.uint32(26).fork()).ldelim();
return null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(32).int32(e.Level), 
t;
}, x.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, x.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Battle(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int64();
break;

case 2:
n.Type = e.int32();
break;

case 3:
n.Fighters && n.Fighters.length || (n.Fighters = []), n.Fighters.push($root.pb.Fighter.decode(e, e.uint32()));
break;

case 4:
n.Level = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, x.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, x.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !($util.isInteger(e.Id) || e.Id && $util.isInteger(e.Id.low) && $util.isInteger(e.Id.high))) return "Id: integer|Long expected";
if (null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type)) return "Type: integer expected";
if (null != e.Fighters && e.hasOwnProperty("Fighters")) {
if (!Array.isArray(e.Fighters)) return "Fighters: array expected";
for (var t = 0; t < e.Fighters.length; ++t) {
var r = $root.pb.Fighter.verify(e.Fighters[t]);
if (r) return "Fighters." + r;
}
}
return null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null;
}, x), r.Fighter = (A.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
A.prototype.Name = "", A.create = function(e) {
return new A(e);
}, A.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
t;
}, A.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, A.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Fighter(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, A.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, A.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null;
}, A), r.BattleCommand = (j.prototype.Type = 0, j.prototype.Data = $util.newBuffer([]), 
j.create = function(e) {
return new j(e);
}, j.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.Data && Object.hasOwnProperty.call(e, "Data") && t.uint32(18).bytes(e.Data), 
t;
}, j.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, j.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleCommand(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.Data = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, j.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, j.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Type && e.hasOwnProperty("Type")) switch (e.Type) {
default:
return "Type: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
return null != e.Data && e.hasOwnProperty("Data") && !(e.Data && "number" == typeof e.Data.length || $util.isString(e.Data)) ? "Data: buffer expected" : null;
}, j), r.BattleCmdType = (e = {}, (t = Object.create(e))[e[0] = "BATTLE_CMD_TYPE_READY"] = 0, 
t[e[1] = "BATTLE_CMD_TYPE_SELECT_HERO"] = 1, t[e[2] = "BATTLE_CMD_TYPE_SELECT_OPTION"] = 2, 
t[e[3] = "BATTLE_CMD_TYPE_KILL_BOSS"] = 3, t[e[4] = "BATTLE_CMD_TYPE_END"] = 4, 
t), r.BattleSelectHeroCommand = (S.prototype.HeroId = 0, S.prototype.IsBan = !1, 
S.create = function(e) {
return new S(e);
}, S.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.IsBan && Object.hasOwnProperty.call(e, "IsBan") && t.uint32(16).bool(e.IsBan), 
t;
}, S.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, S.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectHeroCommand(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.IsBan = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, S.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, S.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId) ? "HeroId: integer expected" : null != e.IsBan && e.hasOwnProperty("IsBan") && "boolean" != typeof e.IsBan ? "IsBan: boolean expected" : null;
}, S), r.BattleOption = (T.prototype.OptionId = 0, T.prototype.Type = 0, T.create = function(e) {
return new T(e);
}, T.encode = function(e, t) {
return t = t || $Writer.create(), null != e.OptionId && Object.hasOwnProperty.call(e, "OptionId") && t.uint32(8).int32(e.OptionId), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(16).int32(e.Type), 
t;
}, T.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, T.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleOption(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.OptionId = e.int32();
break;

case 2:
n.Type = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, T.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, T.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.OptionId && e.hasOwnProperty("OptionId") && !$util.isInteger(e.OptionId) ? "OptionId: integer expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, T), r.BattleSelectOptionCommand = (C.prototype.Options = $util.emptyArray, C.prototype.IsFinish = !1, 
C.create = function(e) {
return new C(e);
}, C.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Options && e.Options.length) for (var r = 0; r < e.Options.length; ++r) $root.pb.BattleOption.encode(e.Options[r], t.uint32(10).fork()).ldelim();
return null != e.IsFinish && Object.hasOwnProperty.call(e, "IsFinish") && t.uint32(16).bool(e.IsFinish), 
t;
}, C.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, C.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectOptionCommand(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Options && n.Options.length || (n.Options = []), n.Options.push($root.pb.BattleOption.decode(e, e.uint32()));
break;

case 2:
n.IsFinish = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, C.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, C.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Options && e.hasOwnProperty("Options")) {
if (!Array.isArray(e.Options)) return "Options: array expected";
for (var t = 0; t < e.Options.length; ++t) {
var r = $root.pb.BattleOption.verify(e.Options[t]);
if (r) return "Options." + r;
}
}
return null != e.IsFinish && e.hasOwnProperty("IsFinish") && "boolean" != typeof e.IsFinish ? "IsFinish: boolean expected" : null;
}, C), r.BattleKillBossCommand = (L.prototype.BossCnt = 0, L.create = function(e) {
return new L(e);
}, L.encode = function(e, t) {
return t = t || $Writer.create(), null != e.BossCnt && Object.hasOwnProperty.call(e, "BossCnt") && t.uint32(8).int32(e.BossCnt), 
t;
}, L.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, L.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleKillBossCommand(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.BossCnt = e.int32() : e.skipType(7 & i);
}
return n;
}, L.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, L.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.BossCnt && e.hasOwnProperty("BossCnt") && !$util.isInteger(e.BossCnt) ? "BossCnt: integer expected" : null;
}, L), r.BattleEndCommand = (D.prototype.IsWin = !1, D.prototype.Wave = 0, D.create = function(e) {
return new D(e);
}, D.encode = function(e, t) {
return t = t || $Writer.create(), null != e.IsWin && Object.hasOwnProperty.call(e, "IsWin") && t.uint32(8).bool(e.IsWin), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(16).int32(e.Wave), 
t;
}, D.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, D.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleEndCommand(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.IsWin = e.bool();
break;

case 2:
n.Wave = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, D.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, D.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.IsWin && e.hasOwnProperty("IsWin") && "boolean" != typeof e.IsWin ? "IsWin: boolean expected" : null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave) ? "Wave: integer expected" : null;
}, D), r.BattleAction = (G.prototype.Id = 0, G.prototype.Data = $util.newBuffer([]), 
G.create = function(e) {
return new G(e);
}, G.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Data && Object.hasOwnProperty.call(e, "Data") && t.uint32(18).bytes(e.Data), 
t;
}, G.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, G.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Data = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, G.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, G.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id")) switch (e.Id) {
default:
return "Id: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
case 5:
case 6:
case 7:
}
return null != e.Data && e.hasOwnProperty("Data") && !(e.Data && "number" == typeof e.Data.length || $util.isString(e.Data)) ? "Data: buffer expected" : null;
}, G), r.BattleActionId = (e = {}, (t = Object.create(e))[e[0] = "BATTLE_ACTION_ID_NONE"] = 0, 
t[e[1] = "BATTLE_ACTION_ID_SELECT_HERO_STAGE"] = 1, t[e[2] = "BATTLE_ACTION_ID_SELECT_HERO"] = 2, 
t[e[3] = "BATTLE_ACTION_ID_SELECT_OPTION_STAGE"] = 3, t[e[4] = "BATTLE_ACTION_ID_SELECT_OPTION"] = 4, 
t[e[5] = "BATTLE_ACTION_ID_BEGIN"] = 5, t[e[6] = "BATTLE_ACTION_ID_END"] = 6, t[e[7] = "BATTLE_ACTION_ID_READY_STAGE"] = 7, 
t), r.BattleHeroTime = (E.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
E.prototype.RemainTime = 0, E.create = function(e) {
return new E(e);
}, E.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(16).int32(e.RemainTime), 
t;
}, E.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, E.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleHeroTime(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.RemainTime = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, E.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, E.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, E), r.BattleSelectHeroStageAction = (W.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
W.prototype.IsBan = !1, W.prototype.Times = $util.emptyArray, W.create = function(e) {
return new W(e);
}, W.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.IsBan && Object.hasOwnProperty.call(e, "IsBan") && t.uint32(16).bool(e.IsBan), 
null != e.Times && e.Times.length) for (var r = 0; r < e.Times.length; ++r) $root.pb.BattleHeroTime.encode(e.Times[r], t.uint32(26).fork()).ldelim();
return t;
}, W.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, W.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectHeroStageAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.IsBan = e.bool();
break;

case 3:
n.Times && n.Times.length || (n.Times = []), n.Times.push($root.pb.BattleHeroTime.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, W.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, W.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high))) return "Uid: integer|Long expected";
if (null != e.IsBan && e.hasOwnProperty("IsBan") && "boolean" != typeof e.IsBan) return "IsBan: boolean expected";
if (null != e.Times && e.hasOwnProperty("Times")) {
if (!Array.isArray(e.Times)) return "Times: array expected";
for (var t = 0; t < e.Times.length; ++t) {
var r = $root.pb.BattleHeroTime.verify(e.Times[t]);
if (r) return "Times." + r;
}
}
return null;
}, W), r.BattleSelectHeroAction = (H.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
H.prototype.HeroId = 0, H.prototype.IsBan = !1, H.create = function(e) {
return new H(e);
}, H.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(16).int32(e.HeroId), 
null != e.IsBan && Object.hasOwnProperty.call(e, "IsBan") && t.uint32(24).bool(e.IsBan), 
t;
}, H.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, H.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectHeroAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.HeroId = e.int32();
break;

case 3:
n.IsBan = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, H.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, H.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId) ? "HeroId: integer expected" : null != e.IsBan && e.hasOwnProperty("IsBan") && "boolean" != typeof e.IsBan ? "IsBan: boolean expected" : null;
}, H), r.BattleSelectOptionStageAction = (B.prototype.SelectCnt = 0, B.prototype.RemainTime = 0, 
B.create = function(e) {
return new B(e);
}, B.encode = function(e, t) {
return t = t || $Writer.create(), null != e.SelectCnt && Object.hasOwnProperty.call(e, "SelectCnt") && t.uint32(8).int32(e.SelectCnt), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(16).int32(e.RemainTime), 
t;
}, B.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, B.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectOptionStageAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.SelectCnt = e.int32();
break;

case 2:
n.RemainTime = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, B.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, B.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.SelectCnt && e.hasOwnProperty("SelectCnt") && !$util.isInteger(e.SelectCnt) ? "SelectCnt: integer expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, B), r.BattleSelectOptionAction = (U.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
U.prototype.Options = $util.emptyArray, U.create = function(e) {
return new U(e);
}, U.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Options && e.Options.length) for (var r = 0; r < e.Options.length; ++r) $root.pb.BattleOption.encode(e.Options[r], t.uint32(18).fork()).ldelim();
return t;
}, U.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, U.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleSelectOptionAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Options && n.Options.length || (n.Options = []), n.Options.push($root.pb.BattleOption.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, U.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, U.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high))) return "Uid: integer|Long expected";
if (null != e.Options && e.hasOwnProperty("Options")) {
if (!Array.isArray(e.Options)) return "Options: array expected";
for (var t = 0; t < e.Options.length; ++t) {
var r = $root.pb.BattleOption.verify(e.Options[t]);
if (r) return "Options." + r;
}
}
return null;
}, U), r.BattleEndAction = (N.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
N.prototype.IsWin = !1, N.prototype.Reason = 0, N.prototype.Wave = 0, N.create = function(e) {
return new N(e);
}, N.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.IsWin && Object.hasOwnProperty.call(e, "IsWin") && t.uint32(16).bool(e.IsWin), 
null != e.Reason && Object.hasOwnProperty.call(e, "Reason") && t.uint32(24).int32(e.Reason), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(32).int32(e.Wave), 
t;
}, N.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, N.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleEndAction(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.IsWin = e.bool();
break;

case 3:
n.Reason = e.int32();
break;

case 4:
n.Wave = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, N.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, N.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.IsWin && e.hasOwnProperty("IsWin") && "boolean" != typeof e.IsWin ? "IsWin: boolean expected" : null != e.Reason && e.hasOwnProperty("Reason") && !$util.isInteger(e.Reason) ? "Reason: integer expected" : null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave) ? "Wave: integer expected" : null;
}, N), r.BattleReadyStageAction = (F.prototype.RemainTime = 0, F.create = function(e) {
return new F(e);
}, F.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(8).int32(e.RemainTime), 
t;
}, F.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, F.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattleReadyStageAction(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.RemainTime = e.int32() : e.skipType(7 & i);
}
return n;
}, F.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, F.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, F), r.BattlePass = (M.prototype.Level = 0, M.prototype.Exp = 0, M.prototype.Pay30 = !1, 
M.prototype.Pay98 = !1, M.prototype.Items = $util.emptyArray, M.prototype.RemainResetCnt = 0, 
M.prototype.RemainTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, M.create = function(e) {
return new M(e);
}, M.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Exp && Object.hasOwnProperty.call(e, "Exp") && t.uint32(16).int32(e.Exp), 
null != e.Pay30 && Object.hasOwnProperty.call(e, "Pay30") && t.uint32(24).bool(e.Pay30), 
null != e.Pay98 && Object.hasOwnProperty.call(e, "Pay98") && t.uint32(32).bool(e.Pay98), 
null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.BattlePassItem.encode(e.Items[r], t.uint32(42).fork()).ldelim();
return null != e.RemainResetCnt && Object.hasOwnProperty.call(e, "RemainResetCnt") && t.uint32(48).int32(e.RemainResetCnt), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(56).int64(e.RemainTime), 
t;
}, M.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, M.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattlePass(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Exp = e.int32();
break;

case 3:
n.Pay30 = e.bool();
break;

case 4:
n.Pay98 = e.bool();
break;

case 5:
n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.BattlePassItem.decode(e, e.uint32()));
break;

case 6:
n.RemainResetCnt = e.int32();
break;

case 7:
n.RemainTime = e.int64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, M.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, M.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.Exp && e.hasOwnProperty("Exp") && !$util.isInteger(e.Exp)) return "Exp: integer expected";
if (null != e.Pay30 && e.hasOwnProperty("Pay30") && "boolean" != typeof e.Pay30) return "Pay30: boolean expected";
if (null != e.Pay98 && e.hasOwnProperty("Pay98") && "boolean" != typeof e.Pay98) return "Pay98: boolean expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.BattlePassItem.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null != e.RemainResetCnt && e.hasOwnProperty("RemainResetCnt") && !$util.isInteger(e.RemainResetCnt) ? "RemainResetCnt: integer expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !($util.isInteger(e.RemainTime) || e.RemainTime && $util.isInteger(e.RemainTime.low) && $util.isInteger(e.RemainTime.high)) ? "RemainTime: integer|Long expected" : null;
}, M), r.BattlePassItem = (V.prototype.Level = 0, V.prototype.MaxExp = 0, V.prototype.FreeReward = $util.emptyArray, 
V.prototype.PayReward1 = $util.emptyArray, V.prototype.PayReward2 = $util.emptyArray, 
V.prototype.FreeRewardReceived = !1, V.prototype.PayReward1Received = !1, V.prototype.PayReward2Received = !1, 
V.create = function(e) {
return new V(e);
}, V.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.MaxExp && Object.hasOwnProperty.call(e, "MaxExp") && t.uint32(16).int32(e.MaxExp), 
null != e.FreeReward && e.FreeReward.length) for (var r = 0; r < e.FreeReward.length; ++r) t.uint32(26).string(e.FreeReward[r]);
if (null != e.PayReward1 && e.PayReward1.length) for (r = 0; r < e.PayReward1.length; ++r) t.uint32(34).string(e.PayReward1[r]);
if (null != e.PayReward2 && e.PayReward2.length) for (r = 0; r < e.PayReward2.length; ++r) t.uint32(42).string(e.PayReward2[r]);
return null != e.FreeRewardReceived && Object.hasOwnProperty.call(e, "FreeRewardReceived") && t.uint32(48).bool(e.FreeRewardReceived), 
null != e.PayReward1Received && Object.hasOwnProperty.call(e, "PayReward1Received") && t.uint32(56).bool(e.PayReward1Received), 
null != e.PayReward2Received && Object.hasOwnProperty.call(e, "PayReward2Received") && t.uint32(64).bool(e.PayReward2Received), 
t;
}, V.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, V.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BattlePassItem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.MaxExp = e.int32();
break;

case 3:
n.FreeReward && n.FreeReward.length || (n.FreeReward = []), n.FreeReward.push(e.string());
break;

case 4:
n.PayReward1 && n.PayReward1.length || (n.PayReward1 = []), n.PayReward1.push(e.string());
break;

case 5:
n.PayReward2 && n.PayReward2.length || (n.PayReward2 = []), n.PayReward2.push(e.string());
break;

case 6:
n.FreeRewardReceived = e.bool();
break;

case 7:
n.PayReward1Received = e.bool();
break;

case 8:
n.PayReward2Received = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, V.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, V.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.MaxExp && e.hasOwnProperty("MaxExp") && !$util.isInteger(e.MaxExp)) return "MaxExp: integer expected";
if (null != e.FreeReward && e.hasOwnProperty("FreeReward")) {
if (!Array.isArray(e.FreeReward)) return "FreeReward: array expected";
for (var t = 0; t < e.FreeReward.length; ++t) if (!$util.isString(e.FreeReward[t])) return "FreeReward: string[] expected";
}
if (null != e.PayReward1 && e.hasOwnProperty("PayReward1")) {
if (!Array.isArray(e.PayReward1)) return "PayReward1: array expected";
for (t = 0; t < e.PayReward1.length; ++t) if (!$util.isString(e.PayReward1[t])) return "PayReward1: string[] expected";
}
if (null != e.PayReward2 && e.hasOwnProperty("PayReward2")) {
if (!Array.isArray(e.PayReward2)) return "PayReward2: array expected";
for (t = 0; t < e.PayReward2.length; ++t) if (!$util.isString(e.PayReward2[t])) return "PayReward2: string[] expected";
}
return null != e.FreeRewardReceived && e.hasOwnProperty("FreeRewardReceived") && "boolean" != typeof e.FreeRewardReceived ? "FreeRewardReceived: boolean expected" : null != e.PayReward1Received && e.hasOwnProperty("PayReward1Received") && "boolean" != typeof e.PayReward1Received ? "PayReward1Received: boolean expected" : null != e.PayReward2Received && e.hasOwnProperty("PayReward2Received") && "boolean" != typeof e.PayReward2Received ? "PayReward2Received: boolean expected" : null;
}, V), r.TargetBattlePass = (q.prototype.Id = 0, q.create = function(e) {
return new q(e);
}, q.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
t;
}, q.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, q.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetBattlePass(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.int32() : e.skipType(7 & i);
}
return n;
}, q.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, q.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, q), r.GetBattlePassRewardArg = (J.prototype.Id = 0, J.prototype.Level = 0, J.prototype.Grade = 0, 
J.create = function(e) {
return new J(e);
}, J.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.Grade && Object.hasOwnProperty.call(e, "Grade") && t.uint32(24).int32(e.Grade), 
t;
}, J.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, J.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetBattlePassRewardArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.Grade = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, J.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, J.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Grade && e.hasOwnProperty("Grade") && !$util.isInteger(e.Grade) ? "Grade: integer expected" : null;
}, J), r.BuyBattlePassArg = (X.prototype.Id = 0, X.prototype.Grade = 0, X.create = function(e) {
return new X(e);
}, X.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Grade && Object.hasOwnProperty.call(e, "Grade") && t.uint32(16).int32(e.Grade), 
t;
}, X.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, X.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyBattlePassArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Grade = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, X.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, X.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Grade && e.hasOwnProperty("Grade") && !$util.isInteger(e.Grade) ? "Grade: integer expected" : null;
}, X), r.UpdateBattlePassEntranceArg = (Y.prototype.OpenBPIds = $util.emptyArray, 
Y.create = function(e) {
return new Y(e);
}, Y.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.OpenBPIds && e.OpenBPIds.length) {
t.uint32(10).fork();
for (var r = 0; r < e.OpenBPIds.length; ++r) t.int32(e.OpenBPIds[r]);
t.ldelim();
}
return t;
}, Y.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Y.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateBattlePassEntranceArg(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.OpenBPIds && n.OpenBPIds.length || (n.OpenBPIds = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.OpenBPIds.push(e.int32()); else n.OpenBPIds.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, Y.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Y.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.OpenBPIds && e.hasOwnProperty("OpenBPIds")) {
if (!Array.isArray(e.OpenBPIds)) return "OpenBPIds: array expected";
for (var t = 0; t < e.OpenBPIds.length; ++t) if (!$util.isInteger(e.OpenBPIds[t])) return "OpenBPIds: integer[] expected";
}
return null;
}, Y), r.BoxInfo = (z.prototype.Boxes = $util.emptyArray, z.create = function(e) {
return new z(e);
}, z.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Boxes && e.Boxes.length) for (var r = 0; r < e.Boxes.length; ++r) $root.pb.SelectBox.encode(e.Boxes[r], t.uint32(10).fork()).ldelim();
return t;
}, z.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, z.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BoxInfo(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Boxes && n.Boxes.length || (n.Boxes = []), n.Boxes.push($root.pb.SelectBox.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, z.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, z.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Boxes && e.hasOwnProperty("Boxes")) {
if (!Array.isArray(e.Boxes)) return "Boxes: array expected";
for (var t = 0; t < e.Boxes.length; ++t) {
var r = $root.pb.SelectBox.verify(e.Boxes[t]);
if (r) return "Boxes." + r;
}
}
return null;
}, z), r.SelectBox = (K.prototype.BoxId = "", K.prototype.Rewards = $util.emptyArray, 
K.prototype.Amount = 0, K.create = function(e) {
return new K(e);
}, K.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.BoxId && Object.hasOwnProperty.call(e, "BoxId") && t.uint32(10).string(e.BoxId), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) $root.pb.BoxReward.encode(e.Rewards[r], t.uint32(18).fork()).ldelim();
return null != e.Amount && Object.hasOwnProperty.call(e, "Amount") && t.uint32(24).int32(e.Amount), 
t;
}, K.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, K.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SelectBox(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.BoxId = e.string();
break;

case 2:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.BoxReward.decode(e, e.uint32()));
break;

case 3:
n.Amount = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, K.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, K.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.BoxId && e.hasOwnProperty("BoxId") && !$util.isString(e.BoxId)) return "BoxId: string expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) {
var r = $root.pb.BoxReward.verify(e.Rewards[t]);
if (r) return "Rewards." + r;
}
}
return null != e.Amount && e.hasOwnProperty("Amount") && !$util.isInteger(e.Amount) ? "Amount: integer expected" : null;
}, K), r.BoxReward = (Q.prototype.Id = 0, Q.prototype.Reward = "", Q.create = function(e) {
return new Q(e);
}, Q.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && t.uint32(18).string(e.Reward), 
t;
}, Q.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Q.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BoxReward(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Reward = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Q.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Q.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Reward && e.hasOwnProperty("Reward") && !$util.isString(e.Reward) ? "Reward: string expected" : null;
}, Q), r.SelectBoxRewardArg = (Z.prototype.BoxId = "", Z.prototype.Id = 0, Z.create = function(e) {
return new Z(e);
}, Z.encode = function(e, t) {
return t = t || $Writer.create(), null != e.BoxId && Object.hasOwnProperty.call(e, "BoxId") && t.uint32(10).string(e.BoxId), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(16).int32(e.Id), 
t;
}, Z.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Z.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SelectBoxRewardArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.BoxId = e.string();
break;

case 2:
n.Id = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Z.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Z.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.BoxId && e.hasOwnProperty("BoxId") && !$util.isString(e.BoxId) ? "BoxId: string expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, Z), r.SelectBoxRewardReply = (ee.prototype.Reward = null, ee.prototype.Box = null, 
ee.create = function(e) {
return new ee(e);
}, ee.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Box && Object.hasOwnProperty.call(e, "Box") && $root.pb.BoxInfo.encode(e.Box, t.uint32(18).fork()).ldelim(), 
t;
}, ee.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ee.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SelectBoxRewardReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Box = $root.pb.BoxInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ee.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ee.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Box && e.hasOwnProperty("Box") && (t = $root.pb.BoxInfo.verify(e.Box)) ? "Box." + t : null;
var t;
}, ee), r.GemInfo = (te.prototype.Gems = $util.emptyArray, te.prototype.WearGems = $util.emptyArray, 
te.prototype.HeroWearGems = $util.emptyArray, te.prototype.GemSuccinct = null, te.create = function(e) {
return new te(e);
}, te.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gems && e.Gems.length) for (var r = 0; r < e.Gems.length; ++r) $root.pb.Gem.encode(e.Gems[r], t.uint32(10).fork()).ldelim();
if (null != e.WearGems && e.WearGems.length) for (r = 0; r < e.WearGems.length; ++r) $root.pb.WearGem.encode(e.WearGems[r], t.uint32(18).fork()).ldelim();
if (null != e.HeroWearGems && e.HeroWearGems.length) for (r = 0; r < e.HeroWearGems.length; ++r) $root.pb.HeroWearGem.encode(e.HeroWearGems[r], t.uint32(26).fork()).ldelim();
return null != e.GemSuccinct && Object.hasOwnProperty.call(e, "GemSuccinct") && $root.pb.GemSuccinctInfo.encode(e.GemSuccinct, t.uint32(34).fork()).ldelim(), 
t;
}, te.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, te.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GemInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gems && n.Gems.length || (n.Gems = []), n.Gems.push($root.pb.Gem.decode(e, e.uint32()));
break;

case 2:
n.WearGems && n.WearGems.length || (n.WearGems = []), n.WearGems.push($root.pb.WearGem.decode(e, e.uint32()));
break;

case 3:
n.HeroWearGems && n.HeroWearGems.length || (n.HeroWearGems = []), n.HeroWearGems.push($root.pb.HeroWearGem.decode(e, e.uint32()));
break;

case 4:
n.GemSuccinct = $root.pb.GemSuccinctInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, te.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, te.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gems && e.hasOwnProperty("Gems")) {
if (!Array.isArray(e.Gems)) return "Gems: array expected";
for (var t = 0; t < e.Gems.length; ++t) if (r = $root.pb.Gem.verify(e.Gems[t])) return "Gems." + r;
}
if (null != e.WearGems && e.hasOwnProperty("WearGems")) {
if (!Array.isArray(e.WearGems)) return "WearGems: array expected";
for (t = 0; t < e.WearGems.length; ++t) if (r = $root.pb.WearGem.verify(e.WearGems[t])) return "WearGems." + r;
}
if (null != e.HeroWearGems && e.hasOwnProperty("HeroWearGems")) {
if (!Array.isArray(e.HeroWearGems)) return "HeroWearGems: array expected";
for (t = 0; t < e.HeroWearGems.length; ++t) if (r = $root.pb.HeroWearGem.verify(e.HeroWearGems[t])) return "HeroWearGems." + r;
}
var r;
return null != e.GemSuccinct && e.hasOwnProperty("GemSuccinct") && (r = $root.pb.GemSuccinctInfo.verify(e.GemSuccinct)) ? "GemSuccinct." + r : null;
}, te), r.Gem = (re.prototype.Gid = 0, re.prototype.Id = 0, re.prototype.Lock = !1, 
re.prototype.Ids = $util.emptyArray, re.prototype.Type = 0, re.prototype.Attributes = $util.emptyArray, 
re.prototype.IsHero = !1, re.prototype.ExtraIds = $util.emptyArray, re.create = function(e) {
return new re(e);
}, re.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(16).int32(e.Id), 
null != e.Lock && Object.hasOwnProperty.call(e, "Lock") && t.uint32(24).bool(e.Lock), 
null != e.Ids && e.Ids.length) {
t.uint32(34).fork();
for (var r = 0; r < e.Ids.length; ++r) t.int32(e.Ids[r]);
t.ldelim();
}
if (null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(40).int32(e.Type), 
null != e.Attributes && e.Attributes.length) for (r = 0; r < e.Attributes.length; ++r) $root.pb.GemAttribute.encode(e.Attributes[r], t.uint32(50).fork()).ldelim();
if (null != e.IsHero && Object.hasOwnProperty.call(e, "IsHero") && t.uint32(56).bool(e.IsHero), 
null != e.ExtraIds && e.ExtraIds.length) for (r = 0; r < e.ExtraIds.length; ++r) $root.pb.GemExtraId.encode(e.ExtraIds[r], t.uint32(66).fork()).ldelim();
return t;
}, re.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, re.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Gem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gid = e.int32();
break;

case 2:
n.Id = e.int32();
break;

case 3:
n.Lock = e.bool();
break;

case 4:
if (n.Ids && n.Ids.length || (n.Ids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Ids.push(e.int32()); else n.Ids.push(e.int32());
break;

case 5:
n.Type = e.int32();
break;

case 6:
n.Attributes && n.Attributes.length || (n.Attributes = []), n.Attributes.push($root.pb.GemAttribute.decode(e, e.uint32()));
break;

case 7:
n.IsHero = e.bool();
break;

case 8:
n.ExtraIds && n.ExtraIds.length || (n.ExtraIds = []), n.ExtraIds.push($root.pb.GemExtraId.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, re.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, re.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid)) return "Gid: integer expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Lock && e.hasOwnProperty("Lock") && "boolean" != typeof e.Lock) return "Lock: boolean expected";
if (null != e.Ids && e.hasOwnProperty("Ids")) {
if (!Array.isArray(e.Ids)) return "Ids: array expected";
for (var t = 0; t < e.Ids.length; ++t) if (!$util.isInteger(e.Ids[t])) return "Ids: integer[] expected";
}
if (null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type)) return "Type: integer expected";
if (null != e.Attributes && e.hasOwnProperty("Attributes")) {
if (!Array.isArray(e.Attributes)) return "Attributes: array expected";
for (t = 0; t < e.Attributes.length; ++t) if (r = $root.pb.GemAttribute.verify(e.Attributes[t])) return "Attributes." + r;
}
if (null != e.IsHero && e.hasOwnProperty("IsHero") && "boolean" != typeof e.IsHero) return "IsHero: boolean expected";
if (null != e.ExtraIds && e.hasOwnProperty("ExtraIds")) {
if (!Array.isArray(e.ExtraIds)) return "ExtraIds: array expected";
var r;
for (t = 0; t < e.ExtraIds.length; ++t) if (r = $root.pb.GemExtraId.verify(e.ExtraIds[t])) return "ExtraIds." + r;
}
return null;
}, re), r.GemAttribute = (ne.prototype.Id = 0, ne.prototype.Value = 0, ne.create = function(e) {
return new ne(e);
}, ne.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Value && Object.hasOwnProperty.call(e, "Value") && t.uint32(16).int32(e.Value), 
t;
}, ne.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ne.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GemAttribute(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Value = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ne.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ne.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Value && e.hasOwnProperty("Value") && !$util.isInteger(e.Value) ? "Value: integer expected" : null;
}, ne), r.GemExtraId = (ie.prototype.Ids = $util.emptyArray, ie.create = function(e) {
return new ie(e);
}, ie.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Ids && e.Ids.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Ids.length; ++r) t.int32(e.Ids[r]);
t.ldelim();
}
return t;
}, ie.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ie.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GemExtraId(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.Ids && n.Ids.length || (n.Ids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Ids.push(e.int32()); else n.Ids.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, ie.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ie.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Ids && e.hasOwnProperty("Ids")) {
if (!Array.isArray(e.Ids)) return "Ids: array expected";
for (var t = 0; t < e.Ids.length; ++t) if (!$util.isInteger(e.Ids[t])) return "Ids: integer[] expected";
}
return null;
}, ie), r.WearGemList = (oe.prototype.WearGems = $util.emptyArray, oe.create = function(e) {
return new oe(e);
}, oe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.WearGems && e.WearGems.length) for (var r = 0; r < e.WearGems.length; ++r) $root.pb.WearGem.encode(e.WearGems[r], t.uint32(10).fork()).ldelim();
return t;
}, oe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, oe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearGemList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.WearGems && n.WearGems.length || (n.WearGems = []), n.WearGems.push($root.pb.WearGem.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, oe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, oe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.WearGems && e.hasOwnProperty("WearGems")) {
if (!Array.isArray(e.WearGems)) return "WearGems: array expected";
for (var t = 0; t < e.WearGems.length; ++t) {
var r = $root.pb.WearGem.verify(e.WearGems[t]);
if (r) return "WearGems." + r;
}
}
return null;
}, oe), r.WearGem = (ae.prototype.Slot = 0, ae.prototype.Gid = 0, ae.prototype.IsInvalid = !1, 
ae.create = function(e) {
return new ae(e);
}, ae.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Slot && Object.hasOwnProperty.call(e, "Slot") && t.uint32(8).int32(e.Slot), 
null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(16).int32(e.Gid), 
null != e.IsInvalid && Object.hasOwnProperty.call(e, "IsInvalid") && t.uint32(24).bool(e.IsInvalid), 
t;
}, ae.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ae.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearGem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Slot = e.int32();
break;

case 2:
n.Gid = e.int32();
break;

case 3:
n.IsInvalid = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ae.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ae.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Slot && e.hasOwnProperty("Slot") && !$util.isInteger(e.Slot) ? "Slot: integer expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null != e.IsInvalid && e.hasOwnProperty("IsInvalid") && "boolean" != typeof e.IsInvalid ? "IsInvalid: boolean expected" : null;
}, ae), r.WearGemInfo = (le.prototype.Slot = 0, le.prototype.GemInfo = null, le.prototype.IsInvalid = !1, 
le.create = function(e) {
return new le(e);
}, le.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Slot && Object.hasOwnProperty.call(e, "Slot") && t.uint32(8).int32(e.Slot), 
null != e.GemInfo && Object.hasOwnProperty.call(e, "GemInfo") && $root.pb.Gem.encode(e.GemInfo, t.uint32(18).fork()).ldelim(), 
null != e.IsInvalid && Object.hasOwnProperty.call(e, "IsInvalid") && t.uint32(24).bool(e.IsInvalid), 
t;
}, le.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, le.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearGemInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Slot = e.int32();
break;

case 2:
n.GemInfo = $root.pb.Gem.decode(e, e.uint32());
break;

case 3:
n.IsInvalid = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, le.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, le.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Slot && e.hasOwnProperty("Slot") && !$util.isInteger(e.Slot)) return "Slot: integer expected";
if (null != e.GemInfo && e.hasOwnProperty("GemInfo")) {
var t = $root.pb.Gem.verify(e.GemInfo);
if (t) return "GemInfo." + t;
}
return null != e.IsInvalid && e.hasOwnProperty("IsInvalid") && "boolean" != typeof e.IsInvalid ? "IsInvalid: boolean expected" : null;
}, le), r.TargetGem = (se.prototype.Gid = 0, se.create = function(e) {
return new se(e);
}, se.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
t;
}, se.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, se.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetGem(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Gid = e.int32() : e.skipType(7 & i);
}
return n;
}, se.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, se.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null;
}, se), r.GemList = (ue.prototype.Gems = $util.emptyArray, ue.create = function(e) {
return new ue(e);
}, ue.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gems && e.Gems.length) for (var r = 0; r < e.Gems.length; ++r) $root.pb.Gem.encode(e.Gems[r], t.uint32(10).fork()).ldelim();
return t;
}, ue.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ue.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GemList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Gems && n.Gems.length || (n.Gems = []), n.Gems.push($root.pb.Gem.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, ue.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ue.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gems && e.hasOwnProperty("Gems")) {
if (!Array.isArray(e.Gems)) return "Gems: array expected";
for (var t = 0; t < e.Gems.length; ++t) {
var r = $root.pb.Gem.verify(e.Gems[t]);
if (r) return "Gems." + r;
}
}
return null;
}, ue), r.LockGemArg = (ce.prototype.Gid = 0, ce.prototype.Lock = !1, ce.create = function(e) {
return new ce(e);
}, ce.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
null != e.Lock && Object.hasOwnProperty.call(e, "Lock") && t.uint32(16).bool(e.Lock), 
t;
}, ce.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ce.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LockGemArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gid = e.int32();
break;

case 2:
n.Lock = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ce.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ce.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null != e.Lock && e.hasOwnProperty("Lock") && "boolean" != typeof e.Lock ? "Lock: boolean expected" : null;
}, ce), r.ComposeGemArg = (de.prototype.Gids = $util.emptyArray, de.prototype.IsSuccinct = !1, 
de.create = function(e) {
return new de(e);
}, de.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gids && e.Gids.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Gids.length; ++r) t.int32(e.Gids[r]);
t.ldelim();
}
return null != e.IsSuccinct && Object.hasOwnProperty.call(e, "IsSuccinct") && t.uint32(16).bool(e.IsSuccinct), 
t;
}, de.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, de.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ComposeGemArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
if (n.Gids && n.Gids.length || (n.Gids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Gids.push(e.int32()); else n.Gids.push(e.int32());
break;

case 2:
n.IsSuccinct = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, de.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, de.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gids && e.hasOwnProperty("Gids")) {
if (!Array.isArray(e.Gids)) return "Gids: array expected";
for (var t = 0; t < e.Gids.length; ++t) if (!$util.isInteger(e.Gids[t])) return "Gids: integer[] expected";
}
return null != e.IsSuccinct && e.hasOwnProperty("IsSuccinct") && "boolean" != typeof e.IsSuccinct ? "IsSuccinct: boolean expected" : null;
}, de), r.HeroWearGem = (pe.prototype.HeroId = 0, pe.prototype.Gems = $util.emptyArray, 
pe.create = function(e) {
return new pe(e);
}, pe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.Gems && e.Gems.length) for (var r = 0; r < e.Gems.length; ++r) $root.pb.WearGem.encode(e.Gems[r], t.uint32(18).fork()).ldelim();
return t;
}, pe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, pe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroWearGem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.Gems && n.Gems.length || (n.Gems = []), n.Gems.push($root.pb.WearGem.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, pe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, pe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId)) return "HeroId: integer expected";
if (null != e.Gems && e.hasOwnProperty("Gems")) {
if (!Array.isArray(e.Gems)) return "Gems: array expected";
for (var t = 0; t < e.Gems.length; ++t) {
var r = $root.pb.WearGem.verify(e.Gems[t]);
if (r) return "Gems." + r;
}
}
return null;
}, pe), r.HeroWearGemInfo = (fe.prototype.HeroId = 0, fe.prototype.Gems = $util.emptyArray, 
fe.create = function(e) {
return new fe(e);
}, fe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.Gems && e.Gems.length) for (var r = 0; r < e.Gems.length; ++r) $root.pb.WearGemInfo.encode(e.Gems[r], t.uint32(18).fork()).ldelim();
return t;
}, fe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, fe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroWearGemInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.Gems && n.Gems.length || (n.Gems = []), n.Gems.push($root.pb.WearGemInfo.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, fe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, fe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId)) return "HeroId: integer expected";
if (null != e.Gems && e.hasOwnProperty("Gems")) {
if (!Array.isArray(e.Gems)) return "Gems: array expected";
for (var t = 0; t < e.Gems.length; ++t) {
var r = $root.pb.WearGemInfo.verify(e.Gems[t]);
if (r) return "Gems." + r;
}
}
return null;
}, fe), r.HeroWearGemArg = (ye.prototype.HeroId = 0, ye.prototype.Gid = 0, ye.prototype.Slot = 0, 
ye.create = function(e) {
return new ye(e);
}, ye.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(16).int32(e.Gid), 
null != e.Slot && Object.hasOwnProperty.call(e, "Slot") && t.uint32(24).int32(e.Slot), 
t;
}, ye.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ye.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroWearGemArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.Gid = e.int32();
break;

case 3:
n.Slot = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ye.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ye.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId) ? "HeroId: integer expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null != e.Slot && e.hasOwnProperty("Slot") && !$util.isInteger(e.Slot) ? "Slot: integer expected" : null;
}, ye), r.ComposeAllGemArg = (he.prototype.IsHero = !1, he.create = function(e) {
return new he(e);
}, he.encode = function(e, t) {
return t = t || $Writer.create(), null != e.IsHero && Object.hasOwnProperty.call(e, "IsHero") && t.uint32(8).bool(e.IsHero), 
t;
}, he.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, he.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ComposeAllGemArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.IsHero = e.bool() : e.skipType(7 & i);
}
return n;
}, he.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, he.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.IsHero && e.hasOwnProperty("IsHero") && "boolean" != typeof e.IsHero ? "IsHero: boolean expected" : null;
}, he), r.FetchComposeAllGemReply = (ge.prototype.Gids = $util.emptyArray, ge.create = function(e) {
return new ge(e);
}, ge.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gids && e.Gids.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Gids.length; ++r) t.int32(e.Gids[r]);
t.ldelim();
}
return t;
}, ge.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ge.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchComposeAllGemReply(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.Gids && n.Gids.length || (n.Gids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Gids.push(e.int32()); else n.Gids.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, ge.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ge.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gids && e.hasOwnProperty("Gids")) {
if (!Array.isArray(e.Gids)) return "Gids: array expected";
for (var t = 0; t < e.Gids.length; ++t) if (!$util.isInteger(e.Gids[t])) return "Gids: integer[] expected";
}
return null;
}, ge), r.ComposeAllGemReply = (be.prototype.Gems = $util.emptyArray, be.prototype.Groups = $util.emptyArray, 
be.create = function(e) {
return new be(e);
}, be.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gems && e.Gems.length) for (var r = 0; r < e.Gems.length; ++r) $root.pb.Gem.encode(e.Gems[r], t.uint32(10).fork()).ldelim();
if (null != e.Groups && e.Groups.length) {
t.uint32(18).fork();
for (r = 0; r < e.Groups.length; ++r) t.int32(e.Groups[r]);
t.ldelim();
}
return t;
}, be.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, be.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ComposeAllGemReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gems && n.Gems.length || (n.Gems = []), n.Gems.push($root.pb.Gem.decode(e, e.uint32()));
break;

case 2:
if (n.Groups && n.Groups.length || (n.Groups = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Groups.push(e.int32()); else n.Groups.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, be.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, be.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gems && e.hasOwnProperty("Gems")) {
if (!Array.isArray(e.Gems)) return "Gems: array expected";
for (var t = 0; t < e.Gems.length; ++t) {
var r = $root.pb.Gem.verify(e.Gems[t]);
if (r) return "Gems." + r;
}
}
if (null != e.Groups && e.hasOwnProperty("Groups")) {
if (!Array.isArray(e.Groups)) return "Groups: array expected";
for (t = 0; t < e.Groups.length; ++t) if (!$util.isInteger(e.Groups[t])) return "Groups: integer[] expected";
}
return null;
}, be), r.SuccinctGemArg = (me.prototype.Gid = 0, me.prototype.Index = 0, me.prototype.IsChange = !1, 
me.create = function(e) {
return new me(e);
}, me.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
null != e.Index && Object.hasOwnProperty.call(e, "Index") && t.uint32(16).int32(e.Index), 
null != e.IsChange && Object.hasOwnProperty.call(e, "IsChange") && t.uint32(24).bool(e.IsChange), 
t;
}, me.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, me.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SuccinctGemArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gid = e.int32();
break;

case 2:
n.Index = e.int32();
break;

case 3:
n.IsChange = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, me.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, me.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null != e.Index && e.hasOwnProperty("Index") && !$util.isInteger(e.Index) ? "Index: integer expected" : null != e.IsChange && e.hasOwnProperty("IsChange") && "boolean" != typeof e.IsChange ? "IsChange: boolean expected" : null;
}, me), r.GemSuccinctInfo = (we.prototype.Gid = 0, we.prototype.Id = 0, we.prototype.Attributes = $util.emptyArray, 
we.prototype.ExtraIds = $util.emptyArray, we.prototype.Index = 0, we.prototype.Type = 0, 
we.create = function(e) {
return new we(e);
}, we.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(16).int32(e.Id), 
null != e.Attributes && e.Attributes.length) for (var r = 0; r < e.Attributes.length; ++r) $root.pb.GemAttribute.encode(e.Attributes[r], t.uint32(26).fork()).ldelim();
if (null != e.ExtraIds && e.ExtraIds.length) for (r = 0; r < e.ExtraIds.length; ++r) $root.pb.GemExtraId.encode(e.ExtraIds[r], t.uint32(34).fork()).ldelim();
return null != e.Index && Object.hasOwnProperty.call(e, "Index") && t.uint32(40).int32(e.Index), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(48).int32(e.Type), 
t;
}, we.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, we.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GemSuccinctInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gid = e.int32();
break;

case 2:
n.Id = e.int32();
break;

case 3:
n.Attributes && n.Attributes.length || (n.Attributes = []), n.Attributes.push($root.pb.GemAttribute.decode(e, e.uint32()));
break;

case 4:
n.ExtraIds && n.ExtraIds.length || (n.ExtraIds = []), n.ExtraIds.push($root.pb.GemExtraId.decode(e, e.uint32()));
break;

case 5:
n.Index = e.int32();
break;

case 6:
n.Type = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, we.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, we.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid)) return "Gid: integer expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Attributes && e.hasOwnProperty("Attributes")) {
if (!Array.isArray(e.Attributes)) return "Attributes: array expected";
for (var t = 0; t < e.Attributes.length; ++t) if (r = $root.pb.GemAttribute.verify(e.Attributes[t])) return "Attributes." + r;
}
if (null != e.ExtraIds && e.hasOwnProperty("ExtraIds")) {
if (!Array.isArray(e.ExtraIds)) return "ExtraIds: array expected";
var r;
for (t = 0; t < e.ExtraIds.length; ++t) if (r = $root.pb.GemExtraId.verify(e.ExtraIds[t])) return "ExtraIds." + r;
}
return null != e.Index && e.hasOwnProperty("Index") && !$util.isInteger(e.Index) ? "Index: integer expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, we), r.ReplySuccinctGemArg = (ve.prototype.IsAgree = !1, ve.create = function(e) {
return new ve(e);
}, ve.encode = function(e, t) {
return t = t || $Writer.create(), null != e.IsAgree && Object.hasOwnProperty.call(e, "IsAgree") && t.uint32(8).bool(e.IsAgree), 
t;
}, ve.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ve.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ReplySuccinctGemArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.IsAgree = e.bool() : e.skipType(7 & i);
}
return n;
}, ve.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ve.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.IsAgree && e.hasOwnProperty("IsAgree") && "boolean" != typeof e.IsAgree ? "IsAgree: boolean expected" : null;
}, ve), r.HabbyBindArg = (Oe.prototype.AuthCode = "", Oe.prototype.Env = "", Oe.prototype.TgaDeviceId = "", 
Oe.prototype.TgaDistinctId = "", Oe.create = function(e) {
return new Oe(e);
}, Oe.encode = function(e, t) {
return t = t || $Writer.create(), null != e.AuthCode && Object.hasOwnProperty.call(e, "AuthCode") && t.uint32(10).string(e.AuthCode), 
null != e.Env && Object.hasOwnProperty.call(e, "Env") && t.uint32(18).string(e.Env), 
null != e.TgaDeviceId && Object.hasOwnProperty.call(e, "TgaDeviceId") && t.uint32(26).string(e.TgaDeviceId), 
null != e.TgaDistinctId && Object.hasOwnProperty.call(e, "TgaDistinctId") && t.uint32(34).string(e.TgaDistinctId), 
t;
}, Oe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Oe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HabbyBindArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.AuthCode = e.string();
break;

case 2:
n.Env = e.string();
break;

case 3:
n.TgaDeviceId = e.string();
break;

case 4:
n.TgaDistinctId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Oe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Oe.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.AuthCode && e.hasOwnProperty("AuthCode") && !$util.isString(e.AuthCode) ? "AuthCode: string expected" : null != e.Env && e.hasOwnProperty("Env") && !$util.isString(e.Env) ? "Env: string expected" : null != e.TgaDeviceId && e.hasOwnProperty("TgaDeviceId") && !$util.isString(e.TgaDeviceId) ? "TgaDeviceId: string expected" : null != e.TgaDistinctId && e.hasOwnProperty("TgaDistinctId") && !$util.isString(e.TgaDistinctId) ? "TgaDistinctId: string expected" : null;
}, Oe), r.HabbyBindReply = ($e.prototype.HabbyId = "", $e.create = function(e) {
return new $e(e);
}, $e.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HabbyId && Object.hasOwnProperty.call(e, "HabbyId") && t.uint32(10).string(e.HabbyId), 
t;
}, $e.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, $e.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HabbyBindReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.HabbyId = e.string() : e.skipType(7 & i);
}
return n;
}, $e.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, $e.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HabbyId && e.hasOwnProperty("HabbyId") && !$util.isString(e.HabbyId) ? "HabbyId: string expected" : null;
}, $e), r.HabbyAuthArg = (Re.prototype.AuthCode = "", Re.prototype.Env = "", Re.create = function(e) {
return new Re(e);
}, Re.encode = function(e, t) {
return t = t || $Writer.create(), null != e.AuthCode && Object.hasOwnProperty.call(e, "AuthCode") && t.uint32(10).string(e.AuthCode), 
null != e.Env && Object.hasOwnProperty.call(e, "Env") && t.uint32(18).string(e.Env), 
t;
}, Re.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Re.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HabbyAuthArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.AuthCode = e.string();
break;

case 2:
n.Env = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Re.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Re.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.AuthCode && e.hasOwnProperty("AuthCode") && !$util.isString(e.AuthCode) ? "AuthCode: string expected" : null != e.Env && e.hasOwnProperty("Env") && !$util.isString(e.Env) ? "Env: string expected" : null;
}, Re), r.UpdateHeadArg = (Ie.prototype.Url = "", Ie.create = function(e) {
return new Ie(e);
}, Ie.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Url && Object.hasOwnProperty.call(e, "Url") && t.uint32(10).string(e.Url), 
t;
}, Ie.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ie.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateHeadArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Url = e.string() : e.skipType(7 & i);
}
return n;
}, Ie.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ie.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Url && e.hasOwnProperty("Url") && !$util.isString(e.Url) ? "Url: string expected" : null;
}, Ie), r.UpdateShowHead = (ke.prototype.HeadIds = $util.emptyArray, ke.create = function(e) {
return new ke(e);
}, ke.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeadIds && e.HeadIds.length) for (var r = 0; r < e.HeadIds.length; ++r) t.uint32(10).string(e.HeadIds[r]);
return t;
}, ke.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ke.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateShowHead(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.HeadIds && n.HeadIds.length || (n.HeadIds = []), n.HeadIds.push(e.string())) : e.skipType(7 & i);
}
return n;
}, ke.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ke.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeadIds && e.hasOwnProperty("HeadIds")) {
if (!Array.isArray(e.HeadIds)) return "HeadIds: array expected";
for (var t = 0; t < e.HeadIds.length; ++t) if (!$util.isString(e.HeadIds[t])) return "HeadIds: string[] expected";
}
return null;
}, ke), r.HeroInfo = (Pe.prototype.Id = 0, Pe.prototype.Level = 0, Pe.prototype.TalentPoints = 0, 
Pe.prototype.Talents = $util.emptyArray, Pe.prototype.Amount = 0, Pe.prototype.FreeRemainTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
Pe.create = function(e) {
return new Pe(e);
}, Pe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.TalentPoints && Object.hasOwnProperty.call(e, "TalentPoints") && t.uint32(24).int32(e.TalentPoints), 
null != e.Talents && e.Talents.length) for (var r = 0; r < e.Talents.length; ++r) $root.pb.HeroTalent.encode(e.Talents[r], t.uint32(34).fork()).ldelim();
return null != e.Amount && Object.hasOwnProperty.call(e, "Amount") && t.uint32(40).int32(e.Amount), 
null != e.FreeRemainTime && Object.hasOwnProperty.call(e, "FreeRemainTime") && t.uint32(48).int64(e.FreeRemainTime), 
t;
}, Pe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Pe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.TalentPoints = e.int32();
break;

case 4:
n.Talents && n.Talents.length || (n.Talents = []), n.Talents.push($root.pb.HeroTalent.decode(e, e.uint32()));
break;

case 5:
n.Amount = e.int32();
break;

case 6:
n.FreeRemainTime = e.int64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Pe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Pe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.TalentPoints && e.hasOwnProperty("TalentPoints") && !$util.isInteger(e.TalentPoints)) return "TalentPoints: integer expected";
if (null != e.Talents && e.hasOwnProperty("Talents")) {
if (!Array.isArray(e.Talents)) return "Talents: array expected";
for (var t = 0; t < e.Talents.length; ++t) {
var r = $root.pb.HeroTalent.verify(e.Talents[t]);
if (r) return "Talents." + r;
}
}
return null != e.Amount && e.hasOwnProperty("Amount") && !$util.isInteger(e.Amount) ? "Amount: integer expected" : null != e.FreeRemainTime && e.hasOwnProperty("FreeRemainTime") && !($util.isInteger(e.FreeRemainTime) || e.FreeRemainTime && $util.isInteger(e.FreeRemainTime.low) && $util.isInteger(e.FreeRemainTime.high)) ? "FreeRemainTime: integer|Long expected" : null;
}, Pe), r.HeroInfoList = (_e.prototype.Heros = $util.emptyArray, _e.create = function(e) {
return new _e(e);
}, _e.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Heros && e.Heros.length) for (var r = 0; r < e.Heros.length; ++r) $root.pb.HeroInfo.encode(e.Heros[r], t.uint32(10).fork()).ldelim();
return t;
}, _e.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, _e.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroInfoList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Heros && n.Heros.length || (n.Heros = []), n.Heros.push($root.pb.HeroInfo.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, _e.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, _e.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Heros && e.hasOwnProperty("Heros")) {
if (!Array.isArray(e.Heros)) return "Heros: array expected";
for (var t = 0; t < e.Heros.length; ++t) {
var r = $root.pb.HeroInfo.verify(e.Heros[t]);
if (r) return "Heros." + r;
}
}
return null;
}, _e), r.HeroTalent = (xe.prototype.Id = 0, xe.prototype.Level = 0, xe.create = function(e) {
return new xe(e);
}, xe.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
t;
}, xe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, xe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroTalent(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, xe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, xe.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null;
}, xe), r.TargetHero = (Ae.prototype.Id = 0, Ae.create = function(e) {
return new Ae(e);
}, Ae.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
t;
}, Ae.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ae.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetHero(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.int32() : e.skipType(7 & i);
}
return n;
}, Ae.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ae.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, Ae), r.UpLevelHeroTalentArg = (je.prototype.HeroId = 0, je.prototype.TalentId = 0, 
je.create = function(e) {
return new je(e);
}, je.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.TalentId && Object.hasOwnProperty.call(e, "TalentId") && t.uint32(16).int32(e.TalentId), 
t;
}, je.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, je.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpLevelHeroTalentArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.TalentId = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, je.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, je.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId) ? "HeroId: integer expected" : null != e.TalentId && e.hasOwnProperty("TalentId") && !$util.isInteger(e.TalentId) ? "TalentId: integer expected" : null;
}, je), r.UpLevelHeroReply = (Se.prototype.Reward = null, Se.prototype.Hero = null, 
Se.create = function(e) {
return new Se(e);
}, Se.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Hero && Object.hasOwnProperty.call(e, "Hero") && $root.pb.HeroInfo.encode(e.Hero, t.uint32(18).fork()).ldelim(), 
t;
}, Se.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Se.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpLevelHeroReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Hero = $root.pb.HeroInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Se.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Se.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Hero && e.hasOwnProperty("Hero") && (t = $root.pb.HeroInfo.verify(e.Hero)) ? "Hero." + t : null;
var t;
}, Se), r.HeroConfigList = (Te.prototype.HeroConfigs = $util.emptyArray, Te.create = function(e) {
return new Te(e);
}, Te.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeroConfigs && e.HeroConfigs.length) for (var r = 0; r < e.HeroConfigs.length; ++r) $root.pb.HeroConfig.encode(e.HeroConfigs[r], t.uint32(10).fork()).ldelim();
return t;
}, Te.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Te.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroConfigList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.HeroConfigs && n.HeroConfigs.length || (n.HeroConfigs = []), n.HeroConfigs.push($root.pb.HeroConfig.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Te.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Te.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeroConfigs && e.hasOwnProperty("HeroConfigs")) {
if (!Array.isArray(e.HeroConfigs)) return "HeroConfigs: array expected";
for (var t = 0; t < e.HeroConfigs.length; ++t) {
var r = $root.pb.HeroConfig.verify(e.HeroConfigs[t]);
if (r) return "HeroConfigs." + r;
}
}
return null;
}, Te), r.HeroConfig = (Ce.prototype.Id = 0, Ce.prototype.Consume = 0, Ce.prototype.GetExp = 0, 
Ce.create = function(e) {
return new Ce(e);
}, Ce.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Consume && Object.hasOwnProperty.call(e, "Consume") && t.uint32(16).int32(e.Consume), 
null != e.GetExp && Object.hasOwnProperty.call(e, "GetExp") && t.uint32(24).int32(e.GetExp), 
t;
}, Ce.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ce.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HeroConfig(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Consume = e.int32();
break;

case 3:
n.GetExp = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ce.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ce.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Consume && e.hasOwnProperty("Consume") && !$util.isInteger(e.Consume) ? "Consume: integer expected" : null != e.GetExp && e.hasOwnProperty("GetExp") && !$util.isInteger(e.GetExp) ? "GetExp: integer expected" : null;
}, Ce), r.HintType = (e = {}, (t = Object.create(e))[e[0] = "Unknow"] = 0, t[e[1] = "Mail"] = 1, 
t[e[2] = "Announcement"] = 2, t[e[3] = "UnReadChat"] = 3, t[e[4] = "FriendApply"] = 4, 
t[e[5] = "Indenture"] = 5, t[e[100] = "BattlePass"] = 100, t[e[1e3] = "Activity"] = 1e3, 
t), r.Hint = (Le.prototype.Type = 0, Le.prototype.Count = 0, Le.create = function(e) {
return new Le(e);
}, Le.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.Count && Object.hasOwnProperty.call(e, "Count") && t.uint32(16).int32(e.Count), 
t;
}, Le.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Le.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Hint(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.Count = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Le.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Le.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null != e.Count && e.hasOwnProperty("Count") && !$util.isInteger(e.Count) ? "Count: integer expected" : null;
}, Le), r.IndentureReply = (De.prototype.Teacher = null, De.prototype.Students = $util.emptyArray, 
De.prototype.OnceStudents = $util.emptyArray, De.prototype.Rewards = $util.emptyArray, 
De.create = function(e) {
return new De(e);
}, De.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Teacher && Object.hasOwnProperty.call(e, "Teacher") && $root.pb.IndentureItem.encode(e.Teacher, t.uint32(10).fork()).ldelim(), 
null != e.Students && e.Students.length) for (var r = 0; r < e.Students.length; ++r) $root.pb.IndentureItem.encode(e.Students[r], t.uint32(18).fork()).ldelim();
if (null != e.OnceStudents && e.OnceStudents.length) for (r = 0; r < e.OnceStudents.length; ++r) $root.pb.IndentureItem.encode(e.OnceStudents[r], t.uint32(26).fork()).ldelim();
if (null != e.Rewards && e.Rewards.length) for (r = 0; r < e.Rewards.length; ++r) $root.pb.IndentureRewardInfo.encode(e.Rewards[r], t.uint32(34).fork()).ldelim();
return t;
}, De.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, De.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Teacher = $root.pb.IndentureItem.decode(e, e.uint32());
break;

case 2:
n.Students && n.Students.length || (n.Students = []), n.Students.push($root.pb.IndentureItem.decode(e, e.uint32()));
break;

case 3:
n.OnceStudents && n.OnceStudents.length || (n.OnceStudents = []), n.OnceStudents.push($root.pb.IndentureItem.decode(e, e.uint32()));
break;

case 4:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.IndentureRewardInfo.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, De.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, De.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Teacher && e.hasOwnProperty("Teacher") && (r = $root.pb.IndentureItem.verify(e.Teacher))) return "Teacher." + r;
if (null != e.Students && e.hasOwnProperty("Students")) {
if (!Array.isArray(e.Students)) return "Students: array expected";
for (var t = 0; t < e.Students.length; ++t) if (r = $root.pb.IndentureItem.verify(e.Students[t])) return "Students." + r;
}
if (null != e.OnceStudents && e.hasOwnProperty("OnceStudents")) {
if (!Array.isArray(e.OnceStudents)) return "OnceStudents: array expected";
for (t = 0; t < e.OnceStudents.length; ++t) if (r = $root.pb.IndentureItem.verify(e.OnceStudents[t])) return "OnceStudents." + r;
}
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
var r;
for (t = 0; t < e.Rewards.length; ++t) if (r = $root.pb.IndentureRewardInfo.verify(e.Rewards[t])) return "Rewards." + r;
}
return null;
}, De), r.IndentureRewardInfo = (Ge.prototype.Rewards = $util.emptyArray, Ge.prototype.IsReward = !1, 
Ge.prototype.CanReward = !1, Ge.create = function(e) {
return new Ge(e);
}, Ge.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(10).string(e.Rewards[r]);
return null != e.IsReward && Object.hasOwnProperty.call(e, "IsReward") && t.uint32(16).bool(e.IsReward), 
null != e.CanReward && Object.hasOwnProperty.call(e, "CanReward") && t.uint32(24).bool(e.CanReward), 
t;
}, Ge.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ge.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureRewardInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string());
break;

case 2:
n.IsReward = e.bool();
break;

case 3:
n.CanReward = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ge.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ge.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null != e.IsReward && e.hasOwnProperty("IsReward") && "boolean" != typeof e.IsReward ? "IsReward: boolean expected" : null != e.CanReward && e.hasOwnProperty("CanReward") && "boolean" != typeof e.CanReward ? "CanReward: boolean expected" : null;
}, Ge), r.IndentureItem = (Ee.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Ee.prototype.Name = "", Ee.prototype.Head = "", Ee.prototype.LastOnlineTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
Ee.prototype.Level = 0, Ee.prototype.TotalCoupon = 0, Ee.prototype.CanRewardCoupon = 0, 
Ee.prototype.IndentureTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, Ee.prototype.IsIndentured = !1, 
Ee.prototype.Students = 0, Ee.prototype.IsNew = !1, Ee.create = function(e) {
return new Ee(e);
}, Ee.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
null != e.LastOnlineTime && Object.hasOwnProperty.call(e, "LastOnlineTime") && t.uint32(32).int64(e.LastOnlineTime), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(40).int32(e.Level), 
null != e.TotalCoupon && Object.hasOwnProperty.call(e, "TotalCoupon") && t.uint32(48).int32(e.TotalCoupon), 
null != e.CanRewardCoupon && Object.hasOwnProperty.call(e, "CanRewardCoupon") && t.uint32(56).int32(e.CanRewardCoupon), 
null != e.IndentureTime && Object.hasOwnProperty.call(e, "IndentureTime") && t.uint32(64).int64(e.IndentureTime), 
null != e.IsIndentured && Object.hasOwnProperty.call(e, "IsIndentured") && t.uint32(72).bool(e.IsIndentured), 
null != e.Students && Object.hasOwnProperty.call(e, "Students") && t.uint32(80).int32(e.Students), 
null != e.IsNew && Object.hasOwnProperty.call(e, "IsNew") && t.uint32(88).bool(e.IsNew), 
t;
}, Ee.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ee.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureItem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

case 3:
n.Head = e.string();
break;

case 4:
n.LastOnlineTime = e.int64();
break;

case 5:
n.Level = e.int32();
break;

case 6:
n.TotalCoupon = e.int32();
break;

case 7:
n.CanRewardCoupon = e.int32();
break;

case 8:
n.IndentureTime = e.int64();
break;

case 9:
n.IsIndentured = e.bool();
break;

case 10:
n.Students = e.int32();
break;

case 11:
n.IsNew = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ee.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ee.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head) ? "Head: string expected" : null != e.LastOnlineTime && e.hasOwnProperty("LastOnlineTime") && !($util.isInteger(e.LastOnlineTime) || e.LastOnlineTime && $util.isInteger(e.LastOnlineTime.low) && $util.isInteger(e.LastOnlineTime.high)) ? "LastOnlineTime: integer|Long expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.TotalCoupon && e.hasOwnProperty("TotalCoupon") && !$util.isInteger(e.TotalCoupon) ? "TotalCoupon: integer expected" : null != e.CanRewardCoupon && e.hasOwnProperty("CanRewardCoupon") && !$util.isInteger(e.CanRewardCoupon) ? "CanRewardCoupon: integer expected" : null != e.IndentureTime && e.hasOwnProperty("IndentureTime") && !($util.isInteger(e.IndentureTime) || e.IndentureTime && $util.isInteger(e.IndentureTime.low) && $util.isInteger(e.IndentureTime.high)) ? "IndentureTime: integer|Long expected" : null != e.IsIndentured && e.hasOwnProperty("IsIndentured") && "boolean" != typeof e.IsIndentured ? "IsIndentured: boolean expected" : null != e.Students && e.hasOwnProperty("Students") && !$util.isInteger(e.Students) ? "Students: integer expected" : null != e.IsNew && e.hasOwnProperty("IsNew") && "boolean" != typeof e.IsNew ? "IsNew: boolean expected" : null;
}, Ee), r.RelieveArg = (We.prototype.RelieveUid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
We.create = function(e) {
return new We(e);
}, We.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RelieveUid && Object.hasOwnProperty.call(e, "RelieveUid") && t.uint32(8).uint64(e.RelieveUid), 
t;
}, We.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, We.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RelieveArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.RelieveUid = e.uint64() : e.skipType(7 & i);
}
return n;
}, We.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, We.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RelieveUid && e.hasOwnProperty("RelieveUid") && !($util.isInteger(e.RelieveUid) || e.RelieveUid && $util.isInteger(e.RelieveUid.low) && $util.isInteger(e.RelieveUid.high)) ? "RelieveUid: integer|Long expected" : null;
}, We), r.IndentureRewardReply = (He.prototype.Reward = null, He.create = function(e) {
return new He(e);
}, He.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
t;
}, He.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, He.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureRewardReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Reward = $root.pb.RewardList.decode(e, e.uint32()) : e.skipType(7 & i);
}
return n;
}, He.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, He.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (e = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + e : null;
}, He), r.IndentureTargetStudent = (Be.prototype.StudentUid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Be.create = function(e) {
return new Be(e);
}, Be.encode = function(e, t) {
return t = t || $Writer.create(), null != e.StudentUid && Object.hasOwnProperty.call(e, "StudentUid") && t.uint32(8).uint64(e.StudentUid), 
t;
}, Be.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Be.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureTargetStudent(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.StudentUid = e.uint64() : e.skipType(7 & i);
}
return n;
}, Be.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Be.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.StudentUid && e.hasOwnProperty("StudentUid") && !($util.isInteger(e.StudentUid) || e.StudentUid && $util.isInteger(e.StudentUid.low) && $util.isInteger(e.StudentUid.high)) ? "StudentUid: integer|Long expected" : null;
}, Be), r.IndentureRewardInfoReply = (Ue.prototype.Events = $util.emptyArray, Ue.create = function(e) {
return new Ue(e);
}, Ue.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Events && e.Events.length) for (var r = 0; r < e.Events.length; ++r) $root.pb.IndentureEvent.encode(e.Events[r], t.uint32(10).fork()).ldelim();
return t;
}, Ue.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ue.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureRewardInfoReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Events && n.Events.length || (n.Events = []), n.Events.push($root.pb.IndentureEvent.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Ue.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ue.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Events && e.hasOwnProperty("Events")) {
if (!Array.isArray(e.Events)) return "Events: array expected";
for (var t = 0; t < e.Events.length; ++t) {
var r = $root.pb.IndentureEvent.verify(e.Events[t]);
if (r) return "Events." + r;
}
}
return null;
}, Ue), r.IndentureEventTypeEnum = (e = {}, (t = Object.create(e))[e[0] = "IETUnknow"] = 0, 
t[e[1] = "IETLevelUp"] = 1, t[e[2] = "IETRecharge"] = 2, t), r.IndentureEventStateEnum = (e = {}, 
(t = Object.create(e))[e[0] = "IESUnknow"] = 0, t[e[1] = "IESHasOver"] = 1, t[e[2] = "IESCanOver"] = 2, 
t[e[3] = "IESHasReward"] = 3, t), r.IndentureEvent = (Ne.prototype.RewardCoupon = 0, 
Ne.prototype.EventType = 0, Ne.prototype.EventState = 0, Ne.create = function(e) {
return new Ne(e);
}, Ne.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RewardCoupon && Object.hasOwnProperty.call(e, "RewardCoupon") && t.uint32(8).int32(e.RewardCoupon), 
null != e.EventType && Object.hasOwnProperty.call(e, "EventType") && t.uint32(16).int32(e.EventType), 
null != e.EventState && Object.hasOwnProperty.call(e, "EventState") && t.uint32(24).int32(e.EventState), 
t;
}, Ne.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ne.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IndentureEvent(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.RewardCoupon = e.int32();
break;

case 2:
n.EventType = e.int32();
break;

case 3:
n.EventState = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ne.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ne.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.RewardCoupon && e.hasOwnProperty("RewardCoupon") && !$util.isInteger(e.RewardCoupon)) return "RewardCoupon: integer expected";
if (null != e.EventType && e.hasOwnProperty("EventType")) switch (e.EventType) {
default:
return "EventType: enum value expected";

case 0:
case 1:
case 2:
}
if (null != e.EventState && e.hasOwnProperty("EventState")) switch (e.EventState) {
default:
return "EventState: enum value expected";

case 0:
case 1:
case 2:
case 3:
}
return null;
}, Ne), r.GiveIndentureRewardArg = (Fe.prototype.RewardGid = 0, Fe.create = function(e) {
return new Fe(e);
}, Fe.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RewardGid && Object.hasOwnProperty.call(e, "RewardGid") && t.uint32(8).int32(e.RewardGid), 
t;
}, Fe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Fe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GiveIndentureRewardArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.RewardGid = e.int32() : e.skipType(7 & i);
}
return n;
}, Fe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Fe.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RewardGid && e.hasOwnProperty("RewardGid") && !$util.isInteger(e.RewardGid) ? "RewardGid: integer expected" : null;
}, Fe), r.GiveIndentureRewardReply = (Me.prototype.Reward = null, Me.create = function(e) {
return new Me(e);
}, Me.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
t;
}, Me.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Me.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GiveIndentureRewardReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Reward = $root.pb.RewardList.decode(e, e.uint32()) : e.skipType(7 & i);
}
return n;
}, Me.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Me.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (e = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + e : null;
}, Me), r.TargetLevel = (Ve.prototype.Level = 0, Ve.prototype.Difficulty = 0, Ve.create = function(e) {
return new Ve(e);
}, Ve.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Difficulty && Object.hasOwnProperty.call(e, "Difficulty") && t.uint32(16).int32(e.Difficulty), 
t;
}, Ve.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ve.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetLevel(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Difficulty = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ve.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ve.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Difficulty && e.hasOwnProperty("Difficulty") && !$util.isInteger(e.Difficulty) ? "Difficulty: integer expected" : null;
}, Ve), r.BeginLevelReply = (qe.prototype.RandomSeed = 0, qe.prototype.XlsDatas = $util.emptyArray, 
qe.prototype.Monster = $util.emptyArray, qe.create = function(e) {
return new qe(e);
}, qe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.RandomSeed && Object.hasOwnProperty.call(e, "RandomSeed") && t.uint32(8).int32(e.RandomSeed), 
null != e.XlsDatas && e.XlsDatas.length) for (var r = 0; r < e.XlsDatas.length; ++r) $root.pb.XlsData.encode(e.XlsDatas[r], t.uint32(18).fork()).ldelim();
if (null != e.Monster && e.Monster.length) {
t.uint32(26).fork();
for (r = 0; r < e.Monster.length; ++r) t.int32(e.Monster[r]);
t.ldelim();
}
return t;
}, qe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, qe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BeginLevelReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.RandomSeed = e.int32();
break;

case 2:
n.XlsDatas && n.XlsDatas.length || (n.XlsDatas = []), n.XlsDatas.push($root.pb.XlsData.decode(e, e.uint32()));
break;

case 3:
if (n.Monster && n.Monster.length || (n.Monster = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Monster.push(e.int32()); else n.Monster.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, qe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, qe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.RandomSeed && e.hasOwnProperty("RandomSeed") && !$util.isInteger(e.RandomSeed)) return "RandomSeed: integer expected";
if (null != e.XlsDatas && e.hasOwnProperty("XlsDatas")) {
if (!Array.isArray(e.XlsDatas)) return "XlsDatas: array expected";
for (var t = 0; t < e.XlsDatas.length; ++t) {
var r = $root.pb.XlsData.verify(e.XlsDatas[t]);
if (r) return "XlsDatas." + r;
}
}
if (null != e.Monster && e.hasOwnProperty("Monster")) {
if (!Array.isArray(e.Monster)) return "Monster: array expected";
for (t = 0; t < e.Monster.length; ++t) if (!$util.isInteger(e.Monster[t])) return "Monster: integer[] expected";
}
return null;
}, qe), r.EndLevelArg = (Je.prototype.Level = 0, Je.prototype.NumFrames = 0, Je.prototype.IsWin = !1, 
Je.prototype.HP = 0, Je.prototype.Wave = 0, Je.prototype.HeroDeadCnt = 0, Je.prototype.IsQuit = !1, 
Je.prototype.Progress = 0, Je.prototype.Heros = $util.emptyArray, Je.prototype.Ext = "", 
Je.prototype.UpgradeOptions = $util.emptyArray, Je.prototype.BossCnt = 0, Je.create = function(e) {
return new Je(e);
}, Je.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.NumFrames && Object.hasOwnProperty.call(e, "NumFrames") && t.uint32(16).int32(e.NumFrames), 
null != e.IsWin && Object.hasOwnProperty.call(e, "IsWin") && t.uint32(24).bool(e.IsWin), 
null != e.HP && Object.hasOwnProperty.call(e, "HP") && t.uint32(32).int32(e.HP), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(40).int32(e.Wave), 
null != e.HeroDeadCnt && Object.hasOwnProperty.call(e, "HeroDeadCnt") && t.uint32(48).int32(e.HeroDeadCnt), 
null != e.IsQuit && Object.hasOwnProperty.call(e, "IsQuit") && t.uint32(56).bool(e.IsQuit), 
null != e.Progress && Object.hasOwnProperty.call(e, "Progress") && t.uint32(64).int32(e.Progress), 
null != e.Heros && e.Heros.length) for (var r = 0; r < e.Heros.length; ++r) $root.pb.EndLevelHeroInfo.encode(e.Heros[r], t.uint32(74).fork()).ldelim();
if (null != e.Ext && Object.hasOwnProperty.call(e, "Ext") && t.uint32(82).string(e.Ext), 
null != e.UpgradeOptions && e.UpgradeOptions.length) for (r = 0; r < e.UpgradeOptions.length; ++r) $root.pb.EndLevelUpgradeOption.encode(e.UpgradeOptions[r], t.uint32(90).fork()).ldelim();
return null != e.BossCnt && Object.hasOwnProperty.call(e, "BossCnt") && t.uint32(96).int32(e.BossCnt), 
t;
}, Je.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Je.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.EndLevelArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.NumFrames = e.int32();
break;

case 3:
n.IsWin = e.bool();
break;

case 4:
n.HP = e.int32();
break;

case 5:
n.Wave = e.int32();
break;

case 6:
n.HeroDeadCnt = e.int32();
break;

case 7:
n.IsQuit = e.bool();
break;

case 8:
n.Progress = e.int32();
break;

case 9:
n.Heros && n.Heros.length || (n.Heros = []), n.Heros.push($root.pb.EndLevelHeroInfo.decode(e, e.uint32()));
break;

case 10:
n.Ext = e.string();
break;

case 11:
n.UpgradeOptions && n.UpgradeOptions.length || (n.UpgradeOptions = []), n.UpgradeOptions.push($root.pb.EndLevelUpgradeOption.decode(e, e.uint32()));
break;

case 12:
n.BossCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Je.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Je.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.NumFrames && e.hasOwnProperty("NumFrames") && !$util.isInteger(e.NumFrames)) return "NumFrames: integer expected";
if (null != e.IsWin && e.hasOwnProperty("IsWin") && "boolean" != typeof e.IsWin) return "IsWin: boolean expected";
if (null != e.HP && e.hasOwnProperty("HP") && !$util.isInteger(e.HP)) return "HP: integer expected";
if (null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave)) return "Wave: integer expected";
if (null != e.HeroDeadCnt && e.hasOwnProperty("HeroDeadCnt") && !$util.isInteger(e.HeroDeadCnt)) return "HeroDeadCnt: integer expected";
if (null != e.IsQuit && e.hasOwnProperty("IsQuit") && "boolean" != typeof e.IsQuit) return "IsQuit: boolean expected";
if (null != e.Progress && e.hasOwnProperty("Progress") && !$util.isInteger(e.Progress)) return "Progress: integer expected";
if (null != e.Heros && e.hasOwnProperty("Heros")) {
if (!Array.isArray(e.Heros)) return "Heros: array expected";
for (var t = 0; t < e.Heros.length; ++t) if (r = $root.pb.EndLevelHeroInfo.verify(e.Heros[t])) return "Heros." + r;
}
if (null != e.Ext && e.hasOwnProperty("Ext") && !$util.isString(e.Ext)) return "Ext: string expected";
if (null != e.UpgradeOptions && e.hasOwnProperty("UpgradeOptions")) {
if (!Array.isArray(e.UpgradeOptions)) return "UpgradeOptions: array expected";
var r;
for (t = 0; t < e.UpgradeOptions.length; ++t) if (r = $root.pb.EndLevelUpgradeOption.verify(e.UpgradeOptions[t])) return "UpgradeOptions." + r;
}
return null != e.BossCnt && e.hasOwnProperty("BossCnt") && !$util.isInteger(e.BossCnt) ? "BossCnt: integer expected" : null;
}, Je), r.EndLevelHeroInfo = (Xe.prototype.Id = 0, Xe.prototype.Level = 0, Xe.prototype.UpgradeCnt = 0, 
Xe.prototype.AllIds = $util.emptyArray, Xe.create = function(e) {
return new Xe(e);
}, Xe.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.UpgradeCnt && Object.hasOwnProperty.call(e, "UpgradeCnt") && t.uint32(24).int32(e.UpgradeCnt), 
null != e.AllIds && e.AllIds.length) {
t.uint32(34).fork();
for (var r = 0; r < e.AllIds.length; ++r) t.int32(e.AllIds[r]);
t.ldelim();
}
return t;
}, Xe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Xe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.EndLevelHeroInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.UpgradeCnt = e.int32();
break;

case 4:
if (n.AllIds && n.AllIds.length || (n.AllIds = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.AllIds.push(e.int32()); else n.AllIds.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Xe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Xe.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.UpgradeCnt && e.hasOwnProperty("UpgradeCnt") && !$util.isInteger(e.UpgradeCnt)) return "UpgradeCnt: integer expected";
if (null != e.AllIds && e.hasOwnProperty("AllIds")) {
if (!Array.isArray(e.AllIds)) return "AllIds: array expected";
for (var t = 0; t < e.AllIds.length; ++t) if (!$util.isInteger(e.AllIds[t])) return "AllIds: integer[] expected";
}
return null;
}, Xe), r.EndLevelUpgradeOption = (Ye.prototype.Id = 0, Ye.prototype.AllIds = $util.emptyArray, 
Ye.create = function(e) {
return new Ye(e);
}, Ye.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.AllIds && e.AllIds.length) {
t.uint32(18).fork();
for (var r = 0; r < e.AllIds.length; ++r) t.int32(e.AllIds[r]);
t.ldelim();
}
return t;
}, Ye.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ye.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.EndLevelUpgradeOption(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
if (n.AllIds && n.AllIds.length || (n.AllIds = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.AllIds.push(e.int32()); else n.AllIds.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ye.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ye.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.AllIds && e.hasOwnProperty("AllIds")) {
if (!Array.isArray(e.AllIds)) return "AllIds: array expected";
for (var t = 0; t < e.AllIds.length; ++t) if (!$util.isInteger(e.AllIds[t])) return "AllIds: integer[] expected";
}
return null;
}, Ye), r.EndLevelReply = (ze.prototype.Info = null, ze.prototype.Reward = null, 
ze.prototype.NextInfo = null, ze.prototype.OldRank = 0, ze.prototype.RankAnimation = null, 
ze.create = function(e) {
return new ze(e);
}, ze.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Info && Object.hasOwnProperty.call(e, "Info") && $root.pb.LevelInfo.encode(e.Info, t.uint32(10).fork()).ldelim(), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
null != e.NextInfo && Object.hasOwnProperty.call(e, "NextInfo") && $root.pb.LevelInfo.encode(e.NextInfo, t.uint32(26).fork()).ldelim(), 
null != e.OldRank && Object.hasOwnProperty.call(e, "OldRank") && t.uint32(32).int32(e.OldRank), 
null != e.RankAnimation && Object.hasOwnProperty.call(e, "RankAnimation") && $root.pb.LevelRankInfo.encode(e.RankAnimation, t.uint32(42).fork()).ldelim(), 
t;
}, ze.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ze.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.EndLevelReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Info = $root.pb.LevelInfo.decode(e, e.uint32());
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 3:
n.NextInfo = $root.pb.LevelInfo.decode(e, e.uint32());
break;

case 4:
n.OldRank = e.int32();
break;

case 5:
n.RankAnimation = $root.pb.LevelRankInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ze.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ze.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Info && e.hasOwnProperty("Info") && (t = $root.pb.LevelInfo.verify(e.Info)) ? "Info." + t : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.NextInfo && e.hasOwnProperty("NextInfo") && (t = $root.pb.LevelInfo.verify(e.NextInfo)) ? "NextInfo." + t : null != e.OldRank && e.hasOwnProperty("OldRank") && !$util.isInteger(e.OldRank) ? "OldRank: integer expected" : null != e.RankAnimation && e.hasOwnProperty("RankAnimation") && (t = $root.pb.LevelRankInfo.verify(e.RankAnimation)) ? "RankAnimation." + t : null;
var t;
}, ze), r.LevelInfo = (Ke.prototype.Level = 0, Ke.prototype.Wave = 0, Ke.prototype.Clear = !1, 
Ke.prototype.ClearRewardStatus = $util.emptyArray, Ke.prototype.Progress = 0, Ke.prototype.PerfectClear = 0, 
Ke.prototype.PerfectClearRank = 0, Ke.prototype.Difficulty = 0, Ke.prototype.BanHeroIds = $util.emptyArray, 
Ke.create = function(e) {
return new Ke(e);
}, Ke.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(16).int32(e.Wave), 
null != e.Clear && Object.hasOwnProperty.call(e, "Clear") && t.uint32(24).bool(e.Clear), 
null != e.ClearRewardStatus && e.ClearRewardStatus.length) {
t.uint32(34).fork();
for (var r = 0; r < e.ClearRewardStatus.length; ++r) t.int32(e.ClearRewardStatus[r]);
t.ldelim();
}
if (null != e.Progress && Object.hasOwnProperty.call(e, "Progress") && t.uint32(40).int32(e.Progress), 
null != e.PerfectClear && Object.hasOwnProperty.call(e, "PerfectClear") && t.uint32(48).int32(e.PerfectClear), 
null != e.PerfectClearRank && Object.hasOwnProperty.call(e, "PerfectClearRank") && t.uint32(56).int32(e.PerfectClearRank), 
null != e.Difficulty && Object.hasOwnProperty.call(e, "Difficulty") && t.uint32(64).int32(e.Difficulty), 
null != e.BanHeroIds && e.BanHeroIds.length) {
t.uint32(74).fork();
for (r = 0; r < e.BanHeroIds.length; ++r) t.int32(e.BanHeroIds[r]);
t.ldelim();
}
return t;
}, Ke.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ke.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Wave = e.int32();
break;

case 3:
n.Clear = e.bool();
break;

case 4:
if (n.ClearRewardStatus && n.ClearRewardStatus.length || (n.ClearRewardStatus = []), 
2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.ClearRewardStatus.push(e.int32()); else n.ClearRewardStatus.push(e.int32());
break;

case 5:
n.Progress = e.int32();
break;

case 6:
n.PerfectClear = e.int32();
break;

case 7:
n.PerfectClearRank = e.int32();
break;

case 8:
n.Difficulty = e.int32();
break;

case 9:
if (n.BanHeroIds && n.BanHeroIds.length || (n.BanHeroIds = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.BanHeroIds.push(e.int32()); else n.BanHeroIds.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ke.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ke.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave)) return "Wave: integer expected";
if (null != e.Clear && e.hasOwnProperty("Clear") && "boolean" != typeof e.Clear) return "Clear: boolean expected";
if (null != e.ClearRewardStatus && e.hasOwnProperty("ClearRewardStatus")) {
if (!Array.isArray(e.ClearRewardStatus)) return "ClearRewardStatus: array expected";
for (var t = 0; t < e.ClearRewardStatus.length; ++t) if (!$util.isInteger(e.ClearRewardStatus[t])) return "ClearRewardStatus: integer[] expected";
}
if (null != e.Progress && e.hasOwnProperty("Progress") && !$util.isInteger(e.Progress)) return "Progress: integer expected";
if (null != e.PerfectClear && e.hasOwnProperty("PerfectClear") && !$util.isInteger(e.PerfectClear)) return "PerfectClear: integer expected";
if (null != e.PerfectClearRank && e.hasOwnProperty("PerfectClearRank") && !$util.isInteger(e.PerfectClearRank)) return "PerfectClearRank: integer expected";
if (null != e.Difficulty && e.hasOwnProperty("Difficulty") && !$util.isInteger(e.Difficulty)) return "Difficulty: integer expected";
if (null != e.BanHeroIds && e.hasOwnProperty("BanHeroIds")) {
if (!Array.isArray(e.BanHeroIds)) return "BanHeroIds: array expected";
for (t = 0; t < e.BanHeroIds.length; ++t) if (!$util.isInteger(e.BanHeroIds[t])) return "BanHeroIds: integer[] expected";
}
return null;
}, Ke), r.GetClearLevelFirstRewardArg = (Qe.prototype.Level = 0, Qe.prototype.RewardIdx = 0, 
Qe.create = function(e) {
return new Qe(e);
}, Qe.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.RewardIdx && Object.hasOwnProperty.call(e, "RewardIdx") && t.uint32(16).int32(e.RewardIdx), 
t;
}, Qe.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Qe.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetClearLevelFirstRewardArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.RewardIdx = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Qe.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Qe.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.RewardIdx && e.hasOwnProperty("RewardIdx") && !$util.isInteger(e.RewardIdx) ? "RewardIdx: integer expected" : null;
}, Qe), r.SpeedPassLevelArg = (Ze.prototype.Level = 0, Ze.prototype.Cnt = 0, Ze.create = function(e) {
return new Ze(e);
}, Ze.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Cnt && Object.hasOwnProperty.call(e, "Cnt") && t.uint32(16).int32(e.Cnt), 
t;
}, Ze.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ze.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SpeedPassLevelArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Cnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ze.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ze.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Cnt && e.hasOwnProperty("Cnt") && !$util.isInteger(e.Cnt) ? "Cnt: integer expected" : null;
}, Ze), r.SpeedPassLevelReply = (et.prototype.Reward = null, et.prototype.SpeedPassCnt = 0, 
et.create = function(e) {
return new et(e);
}, et.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.SpeedPassCnt && Object.hasOwnProperty.call(e, "SpeedPassCnt") && t.uint32(16).int32(e.SpeedPassCnt), 
t;
}, et.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, et.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SpeedPassLevelReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.SpeedPassCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, et.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, et.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Reward && e.hasOwnProperty("Reward")) {
var t = $root.pb.RewardList.verify(e.Reward);
if (t) return "Reward." + t;
}
return null != e.SpeedPassCnt && e.hasOwnProperty("SpeedPassCnt") && !$util.isInteger(e.SpeedPassCnt) ? "SpeedPassCnt: integer expected" : null;
}, et), r.UpdateSpeedPass = (tt.prototype.Cnt = 0, tt.create = function(e) {
return new tt(e);
}, tt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Cnt && Object.hasOwnProperty.call(e, "Cnt") && t.uint32(8).int32(e.Cnt), 
t;
}, tt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, tt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateSpeedPass(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Cnt = e.int32() : e.skipType(7 & i);
}
return n;
}, tt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, tt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Cnt && e.hasOwnProperty("Cnt") && !$util.isInteger(e.Cnt) ? "Cnt: integer expected" : null;
}, tt), r.FetchLevelRewardArg = (rt.prototype.Level = 0, rt.prototype.Wave = 0, 
rt.create = function(e) {
return new rt(e);
}, rt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(16).int32(e.Wave), 
t;
}, rt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, rt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchLevelRewardArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Wave = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, rt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, rt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave) ? "Wave: integer expected" : null;
}, rt), r.FetchLevelRewardReply = (nt.prototype.Rewards = $util.emptyArray, nt.create = function(e) {
return new nt(e);
}, nt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(10).string(e.Rewards[r]);
return t;
}, nt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, nt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchLevelRewardReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string())) : e.skipType(7 & i);
}
return n;
}, nt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, nt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null;
}, nt), r.ChallengeLevelInfo = (it.prototype.Type = 0, it.prototype.MaxLevel = 0, 
it.prototype.Score = 0, it.prototype.Equips = $util.emptyArray, it.create = function(e) {
return new it(e);
}, it.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.MaxLevel && Object.hasOwnProperty.call(e, "MaxLevel") && t.uint32(16).int32(e.MaxLevel), 
null != e.Score && Object.hasOwnProperty.call(e, "Score") && t.uint32(24).int32(e.Score), 
null != e.Equips && e.Equips.length) for (var r = 0; r < e.Equips.length; ++r) $root.pb.ChallengeEquip.encode(e.Equips[r], t.uint32(34).fork()).ldelim();
return t;
}, it.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, it.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ChallengeLevelInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.MaxLevel = e.int32();
break;

case 3:
n.Score = e.int32();
break;

case 4:
n.Equips && n.Equips.length || (n.Equips = []), n.Equips.push($root.pb.ChallengeEquip.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, it.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, it.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type)) return "Type: integer expected";
if (null != e.MaxLevel && e.hasOwnProperty("MaxLevel") && !$util.isInteger(e.MaxLevel)) return "MaxLevel: integer expected";
if (null != e.Score && e.hasOwnProperty("Score") && !$util.isInteger(e.Score)) return "Score: integer expected";
if (null != e.Equips && e.hasOwnProperty("Equips")) {
if (!Array.isArray(e.Equips)) return "Equips: array expected";
for (var t = 0; t < e.Equips.length; ++t) {
var r = $root.pb.ChallengeEquip.verify(e.Equips[t]);
if (r) return "Equips." + r;
}
}
return null;
}, it), r.BeginChallengeLevelArg = (ot.prototype.Level = 0, ot.prototype.Type = 0, 
ot.create = function(e) {
return new ot(e);
}, ot.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(16).int32(e.Type), 
t;
}, ot.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ot.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BeginChallengeLevelArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Type = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ot.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ot.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, ot), r.EndChallengeLevelReply = (at.prototype.Info = null, at.prototype.Reward = null, 
at.prototype.OldRank = 0, at.prototype.RankAnimation = null, at.create = function(e) {
return new at(e);
}, at.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Info && Object.hasOwnProperty.call(e, "Info") && $root.pb.ChallengeLevelInfo.encode(e.Info, t.uint32(10).fork()).ldelim(), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
null != e.OldRank && Object.hasOwnProperty.call(e, "OldRank") && t.uint32(24).int32(e.OldRank), 
null != e.RankAnimation && Object.hasOwnProperty.call(e, "RankAnimation") && $root.pb.LevelRankInfo.encode(e.RankAnimation, t.uint32(34).fork()).ldelim(), 
t;
}, at.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, at.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.EndChallengeLevelReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Info = $root.pb.ChallengeLevelInfo.decode(e, e.uint32());
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 3:
n.OldRank = e.int32();
break;

case 4:
n.RankAnimation = $root.pb.LevelRankInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, at.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, at.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Info && e.hasOwnProperty("Info") && (t = $root.pb.ChallengeLevelInfo.verify(e.Info)) ? "Info." + t : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.OldRank && e.hasOwnProperty("OldRank") && !$util.isInteger(e.OldRank) ? "OldRank: integer expected" : null != e.RankAnimation && e.hasOwnProperty("RankAnimation") && (t = $root.pb.LevelRankInfo.verify(e.RankAnimation)) ? "RankAnimation." + t : null;
var t;
}, at), r.ChallengeEquip = (lt.prototype.Id = 0, lt.prototype.RemainCnt = 0, lt.prototype.Type = 0, 
lt.create = function(e) {
return new lt(e);
}, lt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(16).int32(e.RemainCnt), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(24).int32(e.Type), 
t;
}, lt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, lt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ChallengeEquip(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.RemainCnt = e.int32();
break;

case 3:
n.Type = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, lt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, lt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt) ? "RemainCnt: integer expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, lt), r.xlsdata_lead = (st.prototype.Items = $util.emptyArray, st.create = function(e) {
return new st(e);
}, st.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_lead_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, st.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, st.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_lead(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_lead_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, st.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, st.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_lead_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, st), r.xls_lead_item = (ut.prototype.__id__ = 0, ut.prototype.name = 0, ut.prototype.desTra = 0, 
ut.prototype.class = $util.emptyArray, ut.prototype.res = "", ut.prototype.hp = "", 
ut.prototype.atk = 0, ut.prototype.atkSpe = 0, ut.prototype.valid = $util.emptyArray, 
ut.prototype.skill = $util.emptyArray, ut.prototype.range = 0, ut.prototype.missile_effect = "", 
ut.prototype.volume = "", ut.prototype.def = 0, ut.prototype.speed = 0, ut.prototype.sort = 0, 
ut.prototype.attribute_correct = $util.emptyArray, ut.prototype.block = 0, ut.prototype.atkNum = 0, 
ut.prototype.attaack_effect = $util.emptyArray, ut.prototype.lock = 0, ut.prototype.summoned = $util.emptyArray, 
ut.prototype.label = $util.emptyArray, ut.prototype.sound1 = $util.emptyArray, ut.prototype.sound2 = $util.emptyArray, 
ut.create = function(e) {
return new ut(e);
}, ut.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(16).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(24).int32(e.desTra), 
null != e.class && e.class.length) {
t.uint32(34).fork();
for (var r = 0; r < e.class.length; ++r) t.int32(e.class[r]);
t.ldelim();
}
if (null != e.res && Object.hasOwnProperty.call(e, "res") && t.uint32(42).string(e.res), 
null != e.hp && Object.hasOwnProperty.call(e, "hp") && t.uint32(50).string(e.hp), 
null != e.atk && Object.hasOwnProperty.call(e, "atk") && t.uint32(56).int32(e.atk), 
null != e.atkSpe && Object.hasOwnProperty.call(e, "atkSpe") && t.uint32(64).int32(e.atkSpe), 
null != e.valid && e.valid.length) {
t.uint32(74).fork();
for (r = 0; r < e.valid.length; ++r) t.int32(e.valid[r]);
t.ldelim();
}
if (null != e.skill && e.skill.length) {
t.uint32(82).fork();
for (r = 0; r < e.skill.length; ++r) t.int32(e.skill[r]);
t.ldelim();
}
if (null != e.range && Object.hasOwnProperty.call(e, "range") && t.uint32(88).int32(e.range), 
null != e.missile_effect && Object.hasOwnProperty.call(e, "missile_effect") && t.uint32(98).string(e.missile_effect), 
null != e.volume && Object.hasOwnProperty.call(e, "volume") && t.uint32(106).string(e.volume), 
null != e.def && Object.hasOwnProperty.call(e, "def") && t.uint32(112).int32(e.def), 
null != e.speed && Object.hasOwnProperty.call(e, "speed") && t.uint32(120).int32(e.speed), 
null != e.sort && Object.hasOwnProperty.call(e, "sort") && t.uint32(128).int32(e.sort), 
null != e.attribute_correct && e.attribute_correct.length) for (r = 0; r < e.attribute_correct.length; ++r) t.uint32(138).string(e.attribute_correct[r]);
if (null != e.block && Object.hasOwnProperty.call(e, "block") && t.uint32(144).int32(e.block), 
null != e.atkNum && Object.hasOwnProperty.call(e, "atkNum") && t.uint32(152).int32(e.atkNum), 
null != e.attaack_effect && e.attaack_effect.length) {
t.uint32(162).fork();
for (r = 0; r < e.attaack_effect.length; ++r) t.int32(e.attaack_effect[r]);
t.ldelim();
}
if (null != e.lock && Object.hasOwnProperty.call(e, "lock") && t.uint32(168).int32(e.lock), 
null != e.summoned && e.summoned.length) {
t.uint32(178).fork();
for (r = 0; r < e.summoned.length; ++r) t.int32(e.summoned[r]);
t.ldelim();
}
if (null != e.label && e.label.length) {
t.uint32(186).fork();
for (r = 0; r < e.label.length; ++r) t.int32(e.label[r]);
t.ldelim();
}
if (null != e.sound1 && e.sound1.length) for (r = 0; r < e.sound1.length; ++r) t.uint32(194).string(e.sound1[r]);
if (null != e.sound2 && e.sound2.length) for (r = 0; r < e.sound2.length; ++r) t.uint32(202).string(e.sound2[r]);
return t;
}, ut.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ut.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_lead_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.name = e.int32();
break;

case 3:
n.desTra = e.int32();
break;

case 4:
if (n.class && n.class.length || (n.class = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.class.push(e.int32()); else n.class.push(e.int32());
break;

case 5:
n.res = e.string();
break;

case 6:
n.hp = e.string();
break;

case 7:
n.atk = e.int32();
break;

case 8:
n.atkSpe = e.int32();
break;

case 9:
if (n.valid && n.valid.length || (n.valid = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.valid.push(e.int32()); else n.valid.push(e.int32());
break;

case 10:
if (n.skill && n.skill.length || (n.skill = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.skill.push(e.int32()); else n.skill.push(e.int32());
break;

case 11:
n.range = e.int32();
break;

case 12:
n.missile_effect = e.string();
break;

case 13:
n.volume = e.string();
break;

case 14:
n.def = e.int32();
break;

case 15:
n.speed = e.int32();
break;

case 16:
n.sort = e.int32();
break;

case 17:
n.attribute_correct && n.attribute_correct.length || (n.attribute_correct = []), 
n.attribute_correct.push(e.string());
break;

case 18:
n.block = e.int32();
break;

case 19:
n.atkNum = e.int32();
break;

case 20:
if (n.attaack_effect && n.attaack_effect.length || (n.attaack_effect = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.attaack_effect.push(e.int32()); else n.attaack_effect.push(e.int32());
break;

case 21:
n.lock = e.int32();
break;

case 22:
if (n.summoned && n.summoned.length || (n.summoned = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.summoned.push(e.int32()); else n.summoned.push(e.int32());
break;

case 23:
if (n.label && n.label.length || (n.label = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.label.push(e.int32()); else n.label.push(e.int32());
break;

case 24:
n.sound1 && n.sound1.length || (n.sound1 = []), n.sound1.push(e.string());
break;

case 25:
n.sound2 && n.sound2.length || (n.sound2 = []), n.sound2.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ut.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ut.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.class && e.hasOwnProperty("class")) {
if (!Array.isArray(e.class)) return "class: array expected";
for (var t = 0; t < e.class.length; ++t) if (!$util.isInteger(e.class[t])) return "class: integer[] expected";
}
if (null != e.res && e.hasOwnProperty("res") && !$util.isString(e.res)) return "res: string expected";
if (null != e.hp && e.hasOwnProperty("hp") && !$util.isString(e.hp)) return "hp: string expected";
if (null != e.atk && e.hasOwnProperty("atk") && !$util.isInteger(e.atk)) return "atk: integer expected";
if (null != e.atkSpe && e.hasOwnProperty("atkSpe") && !$util.isInteger(e.atkSpe)) return "atkSpe: integer expected";
if (null != e.valid && e.hasOwnProperty("valid")) {
if (!Array.isArray(e.valid)) return "valid: array expected";
for (t = 0; t < e.valid.length; ++t) if (!$util.isInteger(e.valid[t])) return "valid: integer[] expected";
}
if (null != e.skill && e.hasOwnProperty("skill")) {
if (!Array.isArray(e.skill)) return "skill: array expected";
for (t = 0; t < e.skill.length; ++t) if (!$util.isInteger(e.skill[t])) return "skill: integer[] expected";
}
if (null != e.range && e.hasOwnProperty("range") && !$util.isInteger(e.range)) return "range: integer expected";
if (null != e.missile_effect && e.hasOwnProperty("missile_effect") && !$util.isString(e.missile_effect)) return "missile_effect: string expected";
if (null != e.volume && e.hasOwnProperty("volume") && !$util.isString(e.volume)) return "volume: string expected";
if (null != e.def && e.hasOwnProperty("def") && !$util.isInteger(e.def)) return "def: integer expected";
if (null != e.speed && e.hasOwnProperty("speed") && !$util.isInteger(e.speed)) return "speed: integer expected";
if (null != e.sort && e.hasOwnProperty("sort") && !$util.isInteger(e.sort)) return "sort: integer expected";
if (null != e.attribute_correct && e.hasOwnProperty("attribute_correct")) {
if (!Array.isArray(e.attribute_correct)) return "attribute_correct: array expected";
for (t = 0; t < e.attribute_correct.length; ++t) if (!$util.isString(e.attribute_correct[t])) return "attribute_correct: string[] expected";
}
if (null != e.block && e.hasOwnProperty("block") && !$util.isInteger(e.block)) return "block: integer expected";
if (null != e.atkNum && e.hasOwnProperty("atkNum") && !$util.isInteger(e.atkNum)) return "atkNum: integer expected";
if (null != e.attaack_effect && e.hasOwnProperty("attaack_effect")) {
if (!Array.isArray(e.attaack_effect)) return "attaack_effect: array expected";
for (t = 0; t < e.attaack_effect.length; ++t) if (!$util.isInteger(e.attaack_effect[t])) return "attaack_effect: integer[] expected";
}
if (null != e.lock && e.hasOwnProperty("lock") && !$util.isInteger(e.lock)) return "lock: integer expected";
if (null != e.summoned && e.hasOwnProperty("summoned")) {
if (!Array.isArray(e.summoned)) return "summoned: array expected";
for (t = 0; t < e.summoned.length; ++t) if (!$util.isInteger(e.summoned[t])) return "summoned: integer[] expected";
}
if (null != e.label && e.hasOwnProperty("label")) {
if (!Array.isArray(e.label)) return "label: array expected";
for (t = 0; t < e.label.length; ++t) if (!$util.isInteger(e.label[t])) return "label: integer[] expected";
}
if (null != e.sound1 && e.hasOwnProperty("sound1")) {
if (!Array.isArray(e.sound1)) return "sound1: array expected";
for (t = 0; t < e.sound1.length; ++t) if (!$util.isString(e.sound1[t])) return "sound1: string[] expected";
}
if (null != e.sound2 && e.hasOwnProperty("sound2")) {
if (!Array.isArray(e.sound2)) return "sound2: array expected";
for (t = 0; t < e.sound2.length; ++t) if (!$util.isString(e.sound2[t])) return "sound2: string[] expected";
}
return null;
}, ut), r.xlsdata_entity = (ct.prototype.Items = $util.emptyArray, ct.create = function(e) {
return new ct(e);
}, ct.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_entity_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, ct.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ct.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_entity(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_entity_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, ct.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ct.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_entity_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, ct), r.xls_entity_item = (dt.prototype.__id__ = 0, dt.prototype.type = $util.emptyArray, 
dt.prototype.name = 0, dt.prototype.desTra = 0, dt.prototype.class = $util.emptyArray, 
dt.prototype.grade = 0, dt.prototype.corpse = 0, dt.prototype.res = "", dt.prototype.volume = "", 
dt.prototype.collision = "", dt.prototype.hp = "", dt.prototype.hitDef = 0, dt.prototype.atk = "", 
dt.prototype.valid = $util.emptyArray, dt.prototype.atkSpe = 0, dt.prototype.skill = $util.emptyArray, 
dt.prototype.range = 0, dt.prototype.missile_effect = "", dt.prototype.attaack_effect = $util.emptyArray, 
dt.prototype.def = $util.emptyArray, dt.prototype.move_def = 0, dt.prototype.characteristic = $util.emptyArray, 
dt.prototype.speed = 0, dt.prototype.action = $util.emptyArray, dt.prototype.color = "", 
dt.prototype.block = 0, dt.prototype.atkNum = 0, dt.prototype.sound1 = $util.emptyArray, 
dt.prototype.sound2 = $util.emptyArray, dt.create = function(e) {
return new dt(e);
}, dt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.type && e.type.length) {
t.uint32(18).fork();
for (var r = 0; r < e.type.length; ++r) t.int32(e.type[r]);
t.ldelim();
}
if (null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(24).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(32).int32(e.desTra), 
null != e.class && e.class.length) {
t.uint32(42).fork();
for (r = 0; r < e.class.length; ++r) t.int32(e.class[r]);
t.ldelim();
}
if (null != e.grade && Object.hasOwnProperty.call(e, "grade") && t.uint32(48).int32(e.grade), 
null != e.corpse && Object.hasOwnProperty.call(e, "corpse") && t.uint32(56).int32(e.corpse), 
null != e.res && Object.hasOwnProperty.call(e, "res") && t.uint32(66).string(e.res), 
null != e.volume && Object.hasOwnProperty.call(e, "volume") && t.uint32(74).string(e.volume), 
null != e.collision && Object.hasOwnProperty.call(e, "collision") && t.uint32(82).string(e.collision), 
null != e.hp && Object.hasOwnProperty.call(e, "hp") && t.uint32(90).string(e.hp), 
null != e.hitDef && Object.hasOwnProperty.call(e, "hitDef") && t.uint32(96).int32(e.hitDef), 
null != e.atk && Object.hasOwnProperty.call(e, "atk") && t.uint32(106).string(e.atk), 
null != e.valid && e.valid.length) {
t.uint32(114).fork();
for (r = 0; r < e.valid.length; ++r) t.int32(e.valid[r]);
t.ldelim();
}
if (null != e.atkSpe && Object.hasOwnProperty.call(e, "atkSpe") && t.uint32(120).int32(e.atkSpe), 
null != e.skill && e.skill.length) {
t.uint32(130).fork();
for (r = 0; r < e.skill.length; ++r) t.int32(e.skill[r]);
t.ldelim();
}
if (null != e.range && Object.hasOwnProperty.call(e, "range") && t.uint32(136).int32(e.range), 
null != e.missile_effect && Object.hasOwnProperty.call(e, "missile_effect") && t.uint32(146).string(e.missile_effect), 
null != e.attaack_effect && e.attaack_effect.length) {
t.uint32(154).fork();
for (r = 0; r < e.attaack_effect.length; ++r) t.int32(e.attaack_effect[r]);
t.ldelim();
}
if (null != e.def && e.def.length) for (r = 0; r < e.def.length; ++r) t.uint32(162).string(e.def[r]);
if (null != e.move_def && Object.hasOwnProperty.call(e, "move_def") && t.uint32(168).int32(e.move_def), 
null != e.characteristic && e.characteristic.length) for (r = 0; r < e.characteristic.length; ++r) $root.pb.xls_string_array.encode(e.characteristic[r], t.uint32(178).fork()).ldelim();
if (null != e.speed && Object.hasOwnProperty.call(e, "speed") && t.uint32(184).int32(e.speed), 
null != e.action && e.action.length) {
t.uint32(194).fork();
for (r = 0; r < e.action.length; ++r) t.int32(e.action[r]);
t.ldelim();
}
if (null != e.color && Object.hasOwnProperty.call(e, "color") && t.uint32(202).string(e.color), 
null != e.block && Object.hasOwnProperty.call(e, "block") && t.uint32(208).int32(e.block), 
null != e.atkNum && Object.hasOwnProperty.call(e, "atkNum") && t.uint32(216).int32(e.atkNum), 
null != e.sound1 && e.sound1.length) for (r = 0; r < e.sound1.length; ++r) t.uint32(226).string(e.sound1[r]);
if (null != e.sound2 && e.sound2.length) for (r = 0; r < e.sound2.length; ++r) t.uint32(234).string(e.sound2[r]);
return t;
}, dt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, dt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_entity_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
if (n.type && n.type.length || (n.type = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.type.push(e.int32()); else n.type.push(e.int32());
break;

case 3:
n.name = e.int32();
break;

case 4:
n.desTra = e.int32();
break;

case 5:
if (n.class && n.class.length || (n.class = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.class.push(e.int32()); else n.class.push(e.int32());
break;

case 6:
n.grade = e.int32();
break;

case 7:
n.corpse = e.int32();
break;

case 8:
n.res = e.string();
break;

case 9:
n.volume = e.string();
break;

case 10:
n.collision = e.string();
break;

case 11:
n.hp = e.string();
break;

case 12:
n.hitDef = e.int32();
break;

case 13:
n.atk = e.string();
break;

case 14:
if (n.valid && n.valid.length || (n.valid = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.valid.push(e.int32()); else n.valid.push(e.int32());
break;

case 15:
n.atkSpe = e.int32();
break;

case 16:
if (n.skill && n.skill.length || (n.skill = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.skill.push(e.int32()); else n.skill.push(e.int32());
break;

case 17:
n.range = e.int32();
break;

case 18:
n.missile_effect = e.string();
break;

case 19:
if (n.attaack_effect && n.attaack_effect.length || (n.attaack_effect = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.attaack_effect.push(e.int32()); else n.attaack_effect.push(e.int32());
break;

case 20:
n.def && n.def.length || (n.def = []), n.def.push(e.string());
break;

case 21:
n.move_def = e.int32();
break;

case 22:
n.characteristic && n.characteristic.length || (n.characteristic = []), n.characteristic.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 23:
n.speed = e.int32();
break;

case 24:
if (n.action && n.action.length || (n.action = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.action.push(e.int32()); else n.action.push(e.int32());
break;

case 25:
n.color = e.string();
break;

case 26:
n.block = e.int32();
break;

case 27:
n.atkNum = e.int32();
break;

case 28:
n.sound1 && n.sound1.length || (n.sound1 = []), n.sound1.push(e.string());
break;

case 29:
n.sound2 && n.sound2.length || (n.sound2 = []), n.sound2.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, dt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, dt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.type && e.hasOwnProperty("type")) {
if (!Array.isArray(e.type)) return "type: array expected";
for (var t = 0; t < e.type.length; ++t) if (!$util.isInteger(e.type[t])) return "type: integer[] expected";
}
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.class && e.hasOwnProperty("class")) {
if (!Array.isArray(e.class)) return "class: array expected";
for (t = 0; t < e.class.length; ++t) if (!$util.isInteger(e.class[t])) return "class: integer[] expected";
}
if (null != e.grade && e.hasOwnProperty("grade") && !$util.isInteger(e.grade)) return "grade: integer expected";
if (null != e.corpse && e.hasOwnProperty("corpse") && !$util.isInteger(e.corpse)) return "corpse: integer expected";
if (null != e.res && e.hasOwnProperty("res") && !$util.isString(e.res)) return "res: string expected";
if (null != e.volume && e.hasOwnProperty("volume") && !$util.isString(e.volume)) return "volume: string expected";
if (null != e.collision && e.hasOwnProperty("collision") && !$util.isString(e.collision)) return "collision: string expected";
if (null != e.hp && e.hasOwnProperty("hp") && !$util.isString(e.hp)) return "hp: string expected";
if (null != e.hitDef && e.hasOwnProperty("hitDef") && !$util.isInteger(e.hitDef)) return "hitDef: integer expected";
if (null != e.atk && e.hasOwnProperty("atk") && !$util.isString(e.atk)) return "atk: string expected";
if (null != e.valid && e.hasOwnProperty("valid")) {
if (!Array.isArray(e.valid)) return "valid: array expected";
for (t = 0; t < e.valid.length; ++t) if (!$util.isInteger(e.valid[t])) return "valid: integer[] expected";
}
if (null != e.atkSpe && e.hasOwnProperty("atkSpe") && !$util.isInteger(e.atkSpe)) return "atkSpe: integer expected";
if (null != e.skill && e.hasOwnProperty("skill")) {
if (!Array.isArray(e.skill)) return "skill: array expected";
for (t = 0; t < e.skill.length; ++t) if (!$util.isInteger(e.skill[t])) return "skill: integer[] expected";
}
if (null != e.range && e.hasOwnProperty("range") && !$util.isInteger(e.range)) return "range: integer expected";
if (null != e.missile_effect && e.hasOwnProperty("missile_effect") && !$util.isString(e.missile_effect)) return "missile_effect: string expected";
if (null != e.attaack_effect && e.hasOwnProperty("attaack_effect")) {
if (!Array.isArray(e.attaack_effect)) return "attaack_effect: array expected";
for (t = 0; t < e.attaack_effect.length; ++t) if (!$util.isInteger(e.attaack_effect[t])) return "attaack_effect: integer[] expected";
}
if (null != e.def && e.hasOwnProperty("def")) {
if (!Array.isArray(e.def)) return "def: array expected";
for (t = 0; t < e.def.length; ++t) if (!$util.isString(e.def[t])) return "def: string[] expected";
}
if (null != e.move_def && e.hasOwnProperty("move_def") && !$util.isInteger(e.move_def)) return "move_def: integer expected";
if (null != e.characteristic && e.hasOwnProperty("characteristic")) {
if (!Array.isArray(e.characteristic)) return "characteristic: array expected";
for (t = 0; t < e.characteristic.length; ++t) {
var r = $root.pb.xls_string_array.verify(e.characteristic[t]);
if (r) return "characteristic." + r;
}
}
if (null != e.speed && e.hasOwnProperty("speed") && !$util.isInteger(e.speed)) return "speed: integer expected";
if (null != e.action && e.hasOwnProperty("action")) {
if (!Array.isArray(e.action)) return "action: array expected";
for (t = 0; t < e.action.length; ++t) if (!$util.isInteger(e.action[t])) return "action: integer[] expected";
}
if (null != e.color && e.hasOwnProperty("color") && !$util.isString(e.color)) return "color: string expected";
if (null != e.block && e.hasOwnProperty("block") && !$util.isInteger(e.block)) return "block: integer expected";
if (null != e.atkNum && e.hasOwnProperty("atkNum") && !$util.isInteger(e.atkNum)) return "atkNum: integer expected";
if (null != e.sound1 && e.hasOwnProperty("sound1")) {
if (!Array.isArray(e.sound1)) return "sound1: array expected";
for (t = 0; t < e.sound1.length; ++t) if (!$util.isString(e.sound1[t])) return "sound1: string[] expected";
}
if (null != e.sound2 && e.hasOwnProperty("sound2")) {
if (!Array.isArray(e.sound2)) return "sound2: array expected";
for (t = 0; t < e.sound2.length; ++t) if (!$util.isString(e.sound2[t])) return "sound2: string[] expected";
}
return null;
}, dt), r.xlsdata_monster = (pt.prototype.Items = $util.emptyArray, pt.create = function(e) {
return new pt(e);
}, pt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_monster_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, pt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, pt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_monster(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_monster_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, pt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, pt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_monster_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, pt), r.xls_monster_item = (ft.prototype.__id__ = 0, ft.prototype.type = $util.emptyArray, 
ft.prototype.name = 0, ft.prototype.desTra = 0, ft.prototype.class = $util.emptyArray, 
ft.prototype.grade = 0, ft.prototype.corpse = 0, ft.prototype.res = "", ft.prototype.volume = "", 
ft.prototype.collision = "", ft.prototype.hp = "", ft.prototype.hitDef = 0, ft.prototype.atk = "", 
ft.prototype.valid = $util.emptyArray, ft.prototype.atkSpe = 0, ft.prototype.skill = $util.emptyArray, 
ft.prototype.range = 0, ft.prototype.missile_effect = "", ft.prototype.attaack_effect = $util.emptyArray, 
ft.prototype.def = $util.emptyArray, ft.prototype.move_def = 0, ft.prototype.characteristic = $util.emptyArray, 
ft.prototype.speed = 0, ft.prototype.action = $util.emptyArray, ft.prototype.color = "", 
ft.prototype.block = 0, ft.prototype.atkNum = 0, ft.prototype.sound1 = $util.emptyArray, 
ft.prototype.sound2 = $util.emptyArray, ft.prototype.atlas = "", ft.create = function(e) {
return new ft(e);
}, ft.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.type && e.type.length) {
t.uint32(18).fork();
for (var r = 0; r < e.type.length; ++r) t.int32(e.type[r]);
t.ldelim();
}
if (null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(24).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(32).int32(e.desTra), 
null != e.class && e.class.length) {
t.uint32(42).fork();
for (r = 0; r < e.class.length; ++r) t.int32(e.class[r]);
t.ldelim();
}
if (null != e.grade && Object.hasOwnProperty.call(e, "grade") && t.uint32(48).int32(e.grade), 
null != e.corpse && Object.hasOwnProperty.call(e, "corpse") && t.uint32(56).int32(e.corpse), 
null != e.res && Object.hasOwnProperty.call(e, "res") && t.uint32(66).string(e.res), 
null != e.volume && Object.hasOwnProperty.call(e, "volume") && t.uint32(74).string(e.volume), 
null != e.collision && Object.hasOwnProperty.call(e, "collision") && t.uint32(82).string(e.collision), 
null != e.hp && Object.hasOwnProperty.call(e, "hp") && t.uint32(90).string(e.hp), 
null != e.hitDef && Object.hasOwnProperty.call(e, "hitDef") && t.uint32(96).int32(e.hitDef), 
null != e.atk && Object.hasOwnProperty.call(e, "atk") && t.uint32(106).string(e.atk), 
null != e.valid && e.valid.length) {
t.uint32(114).fork();
for (r = 0; r < e.valid.length; ++r) t.int32(e.valid[r]);
t.ldelim();
}
if (null != e.atkSpe && Object.hasOwnProperty.call(e, "atkSpe") && t.uint32(120).int32(e.atkSpe), 
null != e.skill && e.skill.length) {
t.uint32(130).fork();
for (r = 0; r < e.skill.length; ++r) t.int32(e.skill[r]);
t.ldelim();
}
if (null != e.range && Object.hasOwnProperty.call(e, "range") && t.uint32(136).int32(e.range), 
null != e.missile_effect && Object.hasOwnProperty.call(e, "missile_effect") && t.uint32(146).string(e.missile_effect), 
null != e.attaack_effect && e.attaack_effect.length) {
t.uint32(154).fork();
for (r = 0; r < e.attaack_effect.length; ++r) t.int32(e.attaack_effect[r]);
t.ldelim();
}
if (null != e.def && e.def.length) for (r = 0; r < e.def.length; ++r) t.uint32(162).string(e.def[r]);
if (null != e.move_def && Object.hasOwnProperty.call(e, "move_def") && t.uint32(168).int32(e.move_def), 
null != e.characteristic && e.characteristic.length) for (r = 0; r < e.characteristic.length; ++r) $root.pb.xls_string_array.encode(e.characteristic[r], t.uint32(178).fork()).ldelim();
if (null != e.speed && Object.hasOwnProperty.call(e, "speed") && t.uint32(184).int32(e.speed), 
null != e.action && e.action.length) {
t.uint32(194).fork();
for (r = 0; r < e.action.length; ++r) t.int32(e.action[r]);
t.ldelim();
}
if (null != e.color && Object.hasOwnProperty.call(e, "color") && t.uint32(202).string(e.color), 
null != e.block && Object.hasOwnProperty.call(e, "block") && t.uint32(208).int32(e.block), 
null != e.atkNum && Object.hasOwnProperty.call(e, "atkNum") && t.uint32(216).int32(e.atkNum), 
null != e.sound1 && e.sound1.length) for (r = 0; r < e.sound1.length; ++r) t.uint32(226).string(e.sound1[r]);
if (null != e.sound2 && e.sound2.length) for (r = 0; r < e.sound2.length; ++r) t.uint32(234).string(e.sound2[r]);
return null != e.atlas && Object.hasOwnProperty.call(e, "atlas") && t.uint32(242).string(e.atlas), 
t;
}, ft.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ft.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_monster_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
if (n.type && n.type.length || (n.type = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.type.push(e.int32()); else n.type.push(e.int32());
break;

case 3:
n.name = e.int32();
break;

case 4:
n.desTra = e.int32();
break;

case 5:
if (n.class && n.class.length || (n.class = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.class.push(e.int32()); else n.class.push(e.int32());
break;

case 6:
n.grade = e.int32();
break;

case 7:
n.corpse = e.int32();
break;

case 8:
n.res = e.string();
break;

case 9:
n.volume = e.string();
break;

case 10:
n.collision = e.string();
break;

case 11:
n.hp = e.string();
break;

case 12:
n.hitDef = e.int32();
break;

case 13:
n.atk = e.string();
break;

case 14:
if (n.valid && n.valid.length || (n.valid = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.valid.push(e.int32()); else n.valid.push(e.int32());
break;

case 15:
n.atkSpe = e.int32();
break;

case 16:
if (n.skill && n.skill.length || (n.skill = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.skill.push(e.int32()); else n.skill.push(e.int32());
break;

case 17:
n.range = e.int32();
break;

case 18:
n.missile_effect = e.string();
break;

case 19:
if (n.attaack_effect && n.attaack_effect.length || (n.attaack_effect = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.attaack_effect.push(e.int32()); else n.attaack_effect.push(e.int32());
break;

case 20:
n.def && n.def.length || (n.def = []), n.def.push(e.string());
break;

case 21:
n.move_def = e.int32();
break;

case 22:
n.characteristic && n.characteristic.length || (n.characteristic = []), n.characteristic.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 23:
n.speed = e.int32();
break;

case 24:
if (n.action && n.action.length || (n.action = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.action.push(e.int32()); else n.action.push(e.int32());
break;

case 25:
n.color = e.string();
break;

case 26:
n.block = e.int32();
break;

case 27:
n.atkNum = e.int32();
break;

case 28:
n.sound1 && n.sound1.length || (n.sound1 = []), n.sound1.push(e.string());
break;

case 29:
n.sound2 && n.sound2.length || (n.sound2 = []), n.sound2.push(e.string());
break;

case 30:
n.atlas = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ft.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ft.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.type && e.hasOwnProperty("type")) {
if (!Array.isArray(e.type)) return "type: array expected";
for (var t = 0; t < e.type.length; ++t) if (!$util.isInteger(e.type[t])) return "type: integer[] expected";
}
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.class && e.hasOwnProperty("class")) {
if (!Array.isArray(e.class)) return "class: array expected";
for (t = 0; t < e.class.length; ++t) if (!$util.isInteger(e.class[t])) return "class: integer[] expected";
}
if (null != e.grade && e.hasOwnProperty("grade") && !$util.isInteger(e.grade)) return "grade: integer expected";
if (null != e.corpse && e.hasOwnProperty("corpse") && !$util.isInteger(e.corpse)) return "corpse: integer expected";
if (null != e.res && e.hasOwnProperty("res") && !$util.isString(e.res)) return "res: string expected";
if (null != e.volume && e.hasOwnProperty("volume") && !$util.isString(e.volume)) return "volume: string expected";
if (null != e.collision && e.hasOwnProperty("collision") && !$util.isString(e.collision)) return "collision: string expected";
if (null != e.hp && e.hasOwnProperty("hp") && !$util.isString(e.hp)) return "hp: string expected";
if (null != e.hitDef && e.hasOwnProperty("hitDef") && !$util.isInteger(e.hitDef)) return "hitDef: integer expected";
if (null != e.atk && e.hasOwnProperty("atk") && !$util.isString(e.atk)) return "atk: string expected";
if (null != e.valid && e.hasOwnProperty("valid")) {
if (!Array.isArray(e.valid)) return "valid: array expected";
for (t = 0; t < e.valid.length; ++t) if (!$util.isInteger(e.valid[t])) return "valid: integer[] expected";
}
if (null != e.atkSpe && e.hasOwnProperty("atkSpe") && !$util.isInteger(e.atkSpe)) return "atkSpe: integer expected";
if (null != e.skill && e.hasOwnProperty("skill")) {
if (!Array.isArray(e.skill)) return "skill: array expected";
for (t = 0; t < e.skill.length; ++t) if (!$util.isInteger(e.skill[t])) return "skill: integer[] expected";
}
if (null != e.range && e.hasOwnProperty("range") && !$util.isInteger(e.range)) return "range: integer expected";
if (null != e.missile_effect && e.hasOwnProperty("missile_effect") && !$util.isString(e.missile_effect)) return "missile_effect: string expected";
if (null != e.attaack_effect && e.hasOwnProperty("attaack_effect")) {
if (!Array.isArray(e.attaack_effect)) return "attaack_effect: array expected";
for (t = 0; t < e.attaack_effect.length; ++t) if (!$util.isInteger(e.attaack_effect[t])) return "attaack_effect: integer[] expected";
}
if (null != e.def && e.hasOwnProperty("def")) {
if (!Array.isArray(e.def)) return "def: array expected";
for (t = 0; t < e.def.length; ++t) if (!$util.isString(e.def[t])) return "def: string[] expected";
}
if (null != e.move_def && e.hasOwnProperty("move_def") && !$util.isInteger(e.move_def)) return "move_def: integer expected";
if (null != e.characteristic && e.hasOwnProperty("characteristic")) {
if (!Array.isArray(e.characteristic)) return "characteristic: array expected";
for (t = 0; t < e.characteristic.length; ++t) {
var r = $root.pb.xls_string_array.verify(e.characteristic[t]);
if (r) return "characteristic." + r;
}
}
if (null != e.speed && e.hasOwnProperty("speed") && !$util.isInteger(e.speed)) return "speed: integer expected";
if (null != e.action && e.hasOwnProperty("action")) {
if (!Array.isArray(e.action)) return "action: array expected";
for (t = 0; t < e.action.length; ++t) if (!$util.isInteger(e.action[t])) return "action: integer[] expected";
}
if (null != e.color && e.hasOwnProperty("color") && !$util.isString(e.color)) return "color: string expected";
if (null != e.block && e.hasOwnProperty("block") && !$util.isInteger(e.block)) return "block: integer expected";
if (null != e.atkNum && e.hasOwnProperty("atkNum") && !$util.isInteger(e.atkNum)) return "atkNum: integer expected";
if (null != e.sound1 && e.hasOwnProperty("sound1")) {
if (!Array.isArray(e.sound1)) return "sound1: array expected";
for (t = 0; t < e.sound1.length; ++t) if (!$util.isString(e.sound1[t])) return "sound1: string[] expected";
}
if (null != e.sound2 && e.hasOwnProperty("sound2")) {
if (!Array.isArray(e.sound2)) return "sound2: array expected";
for (t = 0; t < e.sound2.length; ++t) if (!$util.isString(e.sound2[t])) return "sound2: string[] expected";
}
return null != e.atlas && e.hasOwnProperty("atlas") && !$util.isString(e.atlas) ? "atlas: string expected" : null;
}, ft), r.xlsdata_skill2 = (yt.prototype.Items = $util.emptyArray, yt.create = function(e) {
return new yt(e);
}, yt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_skill_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, yt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, yt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_skill2(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_skill_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, yt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, yt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_skill_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, yt), r.xls_skill_item = (ht.prototype.__id__ = 0, ht.prototype.triggerOpp = $util.emptyArray, 
ht.prototype.skillGroup = "", ht.prototype.skillGroupType = $util.emptyArray, ht.prototype.name = 0, 
ht.prototype.desTra = 0, ht.prototype.type = "", ht.prototype.variable = $util.emptyArray, 
ht.prototype.condition = $util.emptyArray, ht.prototype.aim = $util.emptyArray, 
ht.prototype.damage = $util.emptyArray, ht.prototype.damage_delay = 0, ht.prototype.damage_range = $util.emptyArray, 
ht.prototype.cd = 0, ht.prototype.move = $util.emptyArray, ht.prototype.res = "", 
ht.prototype.movie_effect = $util.emptyArray, ht.prototype.movie_special = "", ht.prototype.effect_change = $util.emptyArray, 
ht.prototype.statusChange = $util.emptyArray, ht.prototype.statusReplace = $util.emptyArray, 
ht.prototype.parameters = $util.emptyArray, ht.prototype.damage_num = 0, ht.prototype.damage_interval = 0, 
ht.prototype.speed = 0, ht.prototype.attack_range = 0, ht.prototype.sound = $util.emptyArray, 
ht.prototype.isContinued = 0, ht.prototype.actionOth = $util.emptyArray, ht.prototype.getRes = "", 
ht.prototype.damage_type = "", ht.prototype.running_fire = "", ht.prototype.repeat_fire = "", 
ht.prototype.salvo = "", ht.create = function(e) {
return new ht(e);
}, ht.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.triggerOpp && e.triggerOpp.length) for (var r = 0; r < e.triggerOpp.length; ++r) t.uint32(18).string(e.triggerOpp[r]);
if (null != e.skillGroup && Object.hasOwnProperty.call(e, "skillGroup") && t.uint32(26).string(e.skillGroup), 
null != e.skillGroupType && e.skillGroupType.length) for (r = 0; r < e.skillGroupType.length; ++r) $root.pb.xls_integer_array.encode(e.skillGroupType[r], t.uint32(34).fork()).ldelim();
if (null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(40).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(48).int32(e.desTra), 
null != e.type && Object.hasOwnProperty.call(e, "type") && t.uint32(58).string(e.type), 
null != e.variable && e.variable.length) {
t.uint32(66).fork();
for (r = 0; r < e.variable.length; ++r) t.int32(e.variable[r]);
t.ldelim();
}
if (null != e.condition && e.condition.length) for (r = 0; r < e.condition.length; ++r) t.uint32(74).string(e.condition[r]);
if (null != e.aim && e.aim.length) {
t.uint32(82).fork();
for (r = 0; r < e.aim.length; ++r) t.int32(e.aim[r]);
t.ldelim();
}
if (null != e.damage && e.damage.length) for (r = 0; r < e.damage.length; ++r) t.uint32(90).string(e.damage[r]);
if (null != e.damage_delay && Object.hasOwnProperty.call(e, "damage_delay") && t.uint32(101).float(e.damage_delay), 
null != e.damage_range && e.damage_range.length) for (r = 0; r < e.damage_range.length; ++r) $root.pb.xls_string_array.encode(e.damage_range[r], t.uint32(106).fork()).ldelim();
if (null != e.cd && Object.hasOwnProperty.call(e, "cd") && t.uint32(112).int32(e.cd), 
null != e.move && e.move.length) for (r = 0; r < e.move.length; ++r) $root.pb.xls_string_array.encode(e.move[r], t.uint32(122).fork()).ldelim();
if (null != e.res && Object.hasOwnProperty.call(e, "res") && t.uint32(130).string(e.res), 
null != e.movie_effect && e.movie_effect.length) for (r = 0; r < e.movie_effect.length; ++r) t.uint32(138).string(e.movie_effect[r]);
if (null != e.movie_special && Object.hasOwnProperty.call(e, "movie_special") && t.uint32(146).string(e.movie_special), 
null != e.effect_change && e.effect_change.length) for (r = 0; r < e.effect_change.length; ++r) t.uint32(154).string(e.effect_change[r]);
if (null != e.statusChange && e.statusChange.length) for (r = 0; r < e.statusChange.length; ++r) $root.pb.xls_string_array.encode(e.statusChange[r], t.uint32(162).fork()).ldelim();
if (null != e.statusReplace && e.statusReplace.length) {
t.uint32(170).fork();
for (r = 0; r < e.statusReplace.length; ++r) t.int32(e.statusReplace[r]);
t.ldelim();
}
if (null != e.parameters && e.parameters.length) {
t.uint32(178).fork();
for (r = 0; r < e.parameters.length; ++r) t.float(e.parameters[r]);
t.ldelim();
}
if (null != e.damage_num && Object.hasOwnProperty.call(e, "damage_num") && t.uint32(184).int32(e.damage_num), 
null != e.damage_interval && Object.hasOwnProperty.call(e, "damage_interval") && t.uint32(197).float(e.damage_interval), 
null != e.speed && Object.hasOwnProperty.call(e, "speed") && t.uint32(200).int32(e.speed), 
null != e.attack_range && Object.hasOwnProperty.call(e, "attack_range") && t.uint32(208).int32(e.attack_range), 
null != e.sound && e.sound.length) for (r = 0; r < e.sound.length; ++r) t.uint32(218).string(e.sound[r]);
if (null != e.isContinued && Object.hasOwnProperty.call(e, "isContinued") && t.uint32(224).int32(e.isContinued), 
null != e.actionOth && e.actionOth.length) for (r = 0; r < e.actionOth.length; ++r) $root.pb.xls_string_array.encode(e.actionOth[r], t.uint32(234).fork()).ldelim();
return null != e.getRes && Object.hasOwnProperty.call(e, "getRes") && t.uint32(242).string(e.getRes), 
null != e.damage_type && Object.hasOwnProperty.call(e, "damage_type") && t.uint32(250).string(e.damage_type), 
null != e.running_fire && Object.hasOwnProperty.call(e, "running_fire") && t.uint32(258).string(e.running_fire), 
null != e.repeat_fire && Object.hasOwnProperty.call(e, "repeat_fire") && t.uint32(266).string(e.repeat_fire), 
null != e.salvo && Object.hasOwnProperty.call(e, "salvo") && t.uint32(274).string(e.salvo), 
t;
}, ht.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ht.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_skill_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.triggerOpp && n.triggerOpp.length || (n.triggerOpp = []), n.triggerOpp.push(e.string());
break;

case 3:
n.skillGroup = e.string();
break;

case 4:
n.skillGroupType && n.skillGroupType.length || (n.skillGroupType = []), n.skillGroupType.push($root.pb.xls_integer_array.decode(e, e.uint32()));
break;

case 5:
n.name = e.int32();
break;

case 6:
n.desTra = e.int32();
break;

case 7:
n.type = e.string();
break;

case 8:
if (n.variable && n.variable.length || (n.variable = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.variable.push(e.int32()); else n.variable.push(e.int32());
break;

case 9:
n.condition && n.condition.length || (n.condition = []), n.condition.push(e.string());
break;

case 10:
if (n.aim && n.aim.length || (n.aim = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.aim.push(e.int32()); else n.aim.push(e.int32());
break;

case 11:
n.damage && n.damage.length || (n.damage = []), n.damage.push(e.string());
break;

case 12:
n.damage_delay = e.float();
break;

case 13:
n.damage_range && n.damage_range.length || (n.damage_range = []), n.damage_range.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 14:
n.cd = e.int32();
break;

case 15:
n.move && n.move.length || (n.move = []), n.move.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 16:
n.res = e.string();
break;

case 17:
n.movie_effect && n.movie_effect.length || (n.movie_effect = []), n.movie_effect.push(e.string());
break;

case 18:
n.movie_special = e.string();
break;

case 19:
n.effect_change && n.effect_change.length || (n.effect_change = []), n.effect_change.push(e.string());
break;

case 20:
n.statusChange && n.statusChange.length || (n.statusChange = []), n.statusChange.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 21:
if (n.statusReplace && n.statusReplace.length || (n.statusReplace = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.statusReplace.push(e.int32()); else n.statusReplace.push(e.int32());
break;

case 22:
if (n.parameters && n.parameters.length || (n.parameters = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.parameters.push(e.float()); else n.parameters.push(e.float());
break;

case 23:
n.damage_num = e.int32();
break;

case 24:
n.damage_interval = e.float();
break;

case 25:
n.speed = e.int32();
break;

case 26:
n.attack_range = e.int32();
break;

case 27:
n.sound && n.sound.length || (n.sound = []), n.sound.push(e.string());
break;

case 28:
n.isContinued = e.int32();
break;

case 29:
n.actionOth && n.actionOth.length || (n.actionOth = []), n.actionOth.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 30:
n.getRes = e.string();
break;

case 31:
n.damage_type = e.string();
break;

case 32:
n.running_fire = e.string();
break;

case 33:
n.repeat_fire = e.string();
break;

case 34:
n.salvo = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ht.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ht.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.triggerOpp && e.hasOwnProperty("triggerOpp")) {
if (!Array.isArray(e.triggerOpp)) return "triggerOpp: array expected";
for (var t = 0; t < e.triggerOpp.length; ++t) if (!$util.isString(e.triggerOpp[t])) return "triggerOpp: string[] expected";
}
if (null != e.skillGroup && e.hasOwnProperty("skillGroup") && !$util.isString(e.skillGroup)) return "skillGroup: string expected";
if (null != e.skillGroupType && e.hasOwnProperty("skillGroupType")) {
if (!Array.isArray(e.skillGroupType)) return "skillGroupType: array expected";
for (t = 0; t < e.skillGroupType.length; ++t) if (r = $root.pb.xls_integer_array.verify(e.skillGroupType[t])) return "skillGroupType." + r;
}
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.type && e.hasOwnProperty("type") && !$util.isString(e.type)) return "type: string expected";
if (null != e.variable && e.hasOwnProperty("variable")) {
if (!Array.isArray(e.variable)) return "variable: array expected";
for (t = 0; t < e.variable.length; ++t) if (!$util.isInteger(e.variable[t])) return "variable: integer[] expected";
}
if (null != e.condition && e.hasOwnProperty("condition")) {
if (!Array.isArray(e.condition)) return "condition: array expected";
for (t = 0; t < e.condition.length; ++t) if (!$util.isString(e.condition[t])) return "condition: string[] expected";
}
if (null != e.aim && e.hasOwnProperty("aim")) {
if (!Array.isArray(e.aim)) return "aim: array expected";
for (t = 0; t < e.aim.length; ++t) if (!$util.isInteger(e.aim[t])) return "aim: integer[] expected";
}
if (null != e.damage && e.hasOwnProperty("damage")) {
if (!Array.isArray(e.damage)) return "damage: array expected";
for (t = 0; t < e.damage.length; ++t) if (!$util.isString(e.damage[t])) return "damage: string[] expected";
}
if (null != e.damage_delay && e.hasOwnProperty("damage_delay") && "number" != typeof e.damage_delay) return "damage_delay: number expected";
if (null != e.damage_range && e.hasOwnProperty("damage_range")) {
if (!Array.isArray(e.damage_range)) return "damage_range: array expected";
for (t = 0; t < e.damage_range.length; ++t) if (r = $root.pb.xls_string_array.verify(e.damage_range[t])) return "damage_range." + r;
}
if (null != e.cd && e.hasOwnProperty("cd") && !$util.isInteger(e.cd)) return "cd: integer expected";
if (null != e.move && e.hasOwnProperty("move")) {
if (!Array.isArray(e.move)) return "move: array expected";
for (t = 0; t < e.move.length; ++t) if (r = $root.pb.xls_string_array.verify(e.move[t])) return "move." + r;
}
if (null != e.res && e.hasOwnProperty("res") && !$util.isString(e.res)) return "res: string expected";
if (null != e.movie_effect && e.hasOwnProperty("movie_effect")) {
if (!Array.isArray(e.movie_effect)) return "movie_effect: array expected";
for (t = 0; t < e.movie_effect.length; ++t) if (!$util.isString(e.movie_effect[t])) return "movie_effect: string[] expected";
}
if (null != e.movie_special && e.hasOwnProperty("movie_special") && !$util.isString(e.movie_special)) return "movie_special: string expected";
if (null != e.effect_change && e.hasOwnProperty("effect_change")) {
if (!Array.isArray(e.effect_change)) return "effect_change: array expected";
for (t = 0; t < e.effect_change.length; ++t) if (!$util.isString(e.effect_change[t])) return "effect_change: string[] expected";
}
if (null != e.statusChange && e.hasOwnProperty("statusChange")) {
if (!Array.isArray(e.statusChange)) return "statusChange: array expected";
for (t = 0; t < e.statusChange.length; ++t) if (r = $root.pb.xls_string_array.verify(e.statusChange[t])) return "statusChange." + r;
}
if (null != e.statusReplace && e.hasOwnProperty("statusReplace")) {
if (!Array.isArray(e.statusReplace)) return "statusReplace: array expected";
for (t = 0; t < e.statusReplace.length; ++t) if (!$util.isInteger(e.statusReplace[t])) return "statusReplace: integer[] expected";
}
if (null != e.parameters && e.hasOwnProperty("parameters")) {
if (!Array.isArray(e.parameters)) return "parameters: array expected";
for (t = 0; t < e.parameters.length; ++t) if ("number" != typeof e.parameters[t]) return "parameters: number[] expected";
}
if (null != e.damage_num && e.hasOwnProperty("damage_num") && !$util.isInteger(e.damage_num)) return "damage_num: integer expected";
if (null != e.damage_interval && e.hasOwnProperty("damage_interval") && "number" != typeof e.damage_interval) return "damage_interval: number expected";
if (null != e.speed && e.hasOwnProperty("speed") && !$util.isInteger(e.speed)) return "speed: integer expected";
if (null != e.attack_range && e.hasOwnProperty("attack_range") && !$util.isInteger(e.attack_range)) return "attack_range: integer expected";
if (null != e.sound && e.hasOwnProperty("sound")) {
if (!Array.isArray(e.sound)) return "sound: array expected";
for (t = 0; t < e.sound.length; ++t) if (!$util.isString(e.sound[t])) return "sound: string[] expected";
}
if (null != e.isContinued && e.hasOwnProperty("isContinued") && !$util.isInteger(e.isContinued)) return "isContinued: integer expected";
if (null != e.actionOth && e.hasOwnProperty("actionOth")) {
if (!Array.isArray(e.actionOth)) return "actionOth: array expected";
var r;
for (t = 0; t < e.actionOth.length; ++t) if (r = $root.pb.xls_string_array.verify(e.actionOth[t])) return "actionOth." + r;
}
return null != e.getRes && e.hasOwnProperty("getRes") && !$util.isString(e.getRes) ? "getRes: string expected" : null != e.damage_type && e.hasOwnProperty("damage_type") && !$util.isString(e.damage_type) ? "damage_type: string expected" : null != e.running_fire && e.hasOwnProperty("running_fire") && !$util.isString(e.running_fire) ? "running_fire: string expected" : null != e.repeat_fire && e.hasOwnProperty("repeat_fire") && !$util.isString(e.repeat_fire) ? "repeat_fire: string expected" : null != e.salvo && e.hasOwnProperty("salvo") && !$util.isString(e.salvo) ? "salvo: string expected" : null;
}, ht), r.xlsdata_target2 = (gt.prototype.Items = $util.emptyArray, gt.create = function(e) {
return new gt(e);
}, gt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_target_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, gt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, gt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_target2(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_target_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, gt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, gt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_target_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, gt), r.xls_target_item = (bt.prototype.__id__ = 0, bt.prototype.type = $util.emptyArray, 
bt.prototype.view = 0, bt.prototype.random = "", bt.prototype.targetCamp = 0, bt.prototype.targetStatus = $util.emptyArray, 
bt.prototype.notargetStatus = $util.emptyArray, bt.prototype.targetHp = $util.emptyArray, 
bt.prototype.targetClass = $util.emptyArray, bt.prototype.targetEntity = $util.emptyArray, 
bt.prototype.notargetEntity = $util.emptyArray, bt.prototype.targetGrade = $util.emptyArray, 
bt.create = function(e) {
return new bt(e);
}, bt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.type && e.type.length) {
t.uint32(18).fork();
for (var r = 0; r < e.type.length; ++r) t.int32(e.type[r]);
t.ldelim();
}
if (null != e.view && Object.hasOwnProperty.call(e, "view") && t.uint32(24).int32(e.view), 
null != e.random && Object.hasOwnProperty.call(e, "random") && t.uint32(34).string(e.random), 
null != e.targetCamp && Object.hasOwnProperty.call(e, "targetCamp") && t.uint32(40).int32(e.targetCamp), 
null != e.targetStatus && e.targetStatus.length) {
t.uint32(50).fork();
for (r = 0; r < e.targetStatus.length; ++r) t.int32(e.targetStatus[r]);
t.ldelim();
}
if (null != e.notargetStatus && e.notargetStatus.length) {
t.uint32(58).fork();
for (r = 0; r < e.notargetStatus.length; ++r) t.int32(e.notargetStatus[r]);
t.ldelim();
}
if (null != e.targetHp && e.targetHp.length) {
t.uint32(66).fork();
for (r = 0; r < e.targetHp.length; ++r) t.int32(e.targetHp[r]);
t.ldelim();
}
if (null != e.targetClass && e.targetClass.length) {
t.uint32(74).fork();
for (r = 0; r < e.targetClass.length; ++r) t.int32(e.targetClass[r]);
t.ldelim();
}
if (null != e.targetEntity && e.targetEntity.length) {
t.uint32(82).fork();
for (r = 0; r < e.targetEntity.length; ++r) t.int32(e.targetEntity[r]);
t.ldelim();
}
if (null != e.notargetEntity && e.notargetEntity.length) {
t.uint32(90).fork();
for (r = 0; r < e.notargetEntity.length; ++r) t.int32(e.notargetEntity[r]);
t.ldelim();
}
if (null != e.targetGrade && e.targetGrade.length) for (r = 0; r < e.targetGrade.length; ++r) t.uint32(98).string(e.targetGrade[r]);
return t;
}, bt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, bt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_target_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
if (n.type && n.type.length || (n.type = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.type.push(e.int32()); else n.type.push(e.int32());
break;

case 3:
n.view = e.int32();
break;

case 4:
n.random = e.string();
break;

case 5:
n.targetCamp = e.int32();
break;

case 6:
if (n.targetStatus && n.targetStatus.length || (n.targetStatus = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.targetStatus.push(e.int32()); else n.targetStatus.push(e.int32());
break;

case 7:
if (n.notargetStatus && n.notargetStatus.length || (n.notargetStatus = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.notargetStatus.push(e.int32()); else n.notargetStatus.push(e.int32());
break;

case 8:
if (n.targetHp && n.targetHp.length || (n.targetHp = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.targetHp.push(e.int32()); else n.targetHp.push(e.int32());
break;

case 9:
if (n.targetClass && n.targetClass.length || (n.targetClass = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.targetClass.push(e.int32()); else n.targetClass.push(e.int32());
break;

case 10:
if (n.targetEntity && n.targetEntity.length || (n.targetEntity = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.targetEntity.push(e.int32()); else n.targetEntity.push(e.int32());
break;

case 11:
if (n.notargetEntity && n.notargetEntity.length || (n.notargetEntity = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.notargetEntity.push(e.int32()); else n.notargetEntity.push(e.int32());
break;

case 12:
n.targetGrade && n.targetGrade.length || (n.targetGrade = []), n.targetGrade.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, bt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, bt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.type && e.hasOwnProperty("type")) {
if (!Array.isArray(e.type)) return "type: array expected";
for (var t = 0; t < e.type.length; ++t) if (!$util.isInteger(e.type[t])) return "type: integer[] expected";
}
if (null != e.view && e.hasOwnProperty("view") && !$util.isInteger(e.view)) return "view: integer expected";
if (null != e.random && e.hasOwnProperty("random") && !$util.isString(e.random)) return "random: string expected";
if (null != e.targetCamp && e.hasOwnProperty("targetCamp") && !$util.isInteger(e.targetCamp)) return "targetCamp: integer expected";
if (null != e.targetStatus && e.hasOwnProperty("targetStatus")) {
if (!Array.isArray(e.targetStatus)) return "targetStatus: array expected";
for (t = 0; t < e.targetStatus.length; ++t) if (!$util.isInteger(e.targetStatus[t])) return "targetStatus: integer[] expected";
}
if (null != e.notargetStatus && e.hasOwnProperty("notargetStatus")) {
if (!Array.isArray(e.notargetStatus)) return "notargetStatus: array expected";
for (t = 0; t < e.notargetStatus.length; ++t) if (!$util.isInteger(e.notargetStatus[t])) return "notargetStatus: integer[] expected";
}
if (null != e.targetHp && e.hasOwnProperty("targetHp")) {
if (!Array.isArray(e.targetHp)) return "targetHp: array expected";
for (t = 0; t < e.targetHp.length; ++t) if (!$util.isInteger(e.targetHp[t])) return "targetHp: integer[] expected";
}
if (null != e.targetClass && e.hasOwnProperty("targetClass")) {
if (!Array.isArray(e.targetClass)) return "targetClass: array expected";
for (t = 0; t < e.targetClass.length; ++t) if (!$util.isInteger(e.targetClass[t])) return "targetClass: integer[] expected";
}
if (null != e.targetEntity && e.hasOwnProperty("targetEntity")) {
if (!Array.isArray(e.targetEntity)) return "targetEntity: array expected";
for (t = 0; t < e.targetEntity.length; ++t) if (!$util.isInteger(e.targetEntity[t])) return "targetEntity: integer[] expected";
}
if (null != e.notargetEntity && e.hasOwnProperty("notargetEntity")) {
if (!Array.isArray(e.notargetEntity)) return "notargetEntity: array expected";
for (t = 0; t < e.notargetEntity.length; ++t) if (!$util.isInteger(e.notargetEntity[t])) return "notargetEntity: integer[] expected";
}
if (null != e.targetGrade && e.hasOwnProperty("targetGrade")) {
if (!Array.isArray(e.targetGrade)) return "targetGrade: array expected";
for (t = 0; t < e.targetGrade.length; ++t) if (!$util.isString(e.targetGrade[t])) return "targetGrade: string[] expected";
}
return null;
}, bt), r.xlsdata_status2 = (mt.prototype.Items = $util.emptyArray, mt.create = function(e) {
return new mt(e);
}, mt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_status_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, mt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, mt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_status2(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_status_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, mt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, mt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_status_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, mt), r.xlsdata_status_item = (wt.prototype.__id__ = 0, wt.prototype.name = 0, 
wt.prototype.desTra = 0, wt.prototype.icon = "", wt.prototype.bufftype = $util.emptyArray, 
wt.prototype.resSet = "", wt.prototype.superposition = 0, wt.prototype.resistance = "", 
wt.prototype.shield = "", wt.prototype.variable = $util.emptyArray, wt.prototype.condition = $util.emptyArray, 
wt.prototype.attackFor = 0, wt.prototype.moveRev = 0, wt.prototype.silent = $util.emptyArray, 
wt.prototype.damageRev = "", wt.prototype.hpRev = "", wt.prototype.atkRev = "", 
wt.prototype.atkSpeRev = 0, wt.prototype.hpRev_type = 0, wt.prototype.hurtRev = "", 
wt.prototype.effect = $util.emptyArray, wt.prototype.skillChange = $util.emptyArray, 
wt.prototype.resistanceType = $util.emptyArray, wt.prototype.change = "", wt.prototype.transformation = 0, 
wt.prototype.colorChange = "", wt.prototype.defRev = "", wt.prototype.attributeRev = $util.emptyArray, 
wt.prototype.moveFor = 0, wt.create = function(e) {
return new wt(e);
}, wt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(16).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(24).int32(e.desTra), 
null != e.icon && Object.hasOwnProperty.call(e, "icon") && t.uint32(34).string(e.icon), 
null != e.bufftype && e.bufftype.length) {
t.uint32(42).fork();
for (var r = 0; r < e.bufftype.length; ++r) t.int32(e.bufftype[r]);
t.ldelim();
}
if (null != e.resSet && Object.hasOwnProperty.call(e, "resSet") && t.uint32(50).string(e.resSet), 
null != e.superposition && Object.hasOwnProperty.call(e, "superposition") && t.uint32(56).int32(e.superposition), 
null != e.resistance && Object.hasOwnProperty.call(e, "resistance") && t.uint32(66).string(e.resistance), 
null != e.shield && Object.hasOwnProperty.call(e, "shield") && t.uint32(74).string(e.shield), 
null != e.variable && e.variable.length) {
t.uint32(82).fork();
for (r = 0; r < e.variable.length; ++r) t.int32(e.variable[r]);
t.ldelim();
}
if (null != e.condition && e.condition.length) for (r = 0; r < e.condition.length; ++r) t.uint32(90).string(e.condition[r]);
if (null != e.attackFor && Object.hasOwnProperty.call(e, "attackFor") && t.uint32(96).int32(e.attackFor), 
null != e.moveRev && Object.hasOwnProperty.call(e, "moveRev") && t.uint32(104).int32(e.moveRev), 
null != e.silent && e.silent.length) {
t.uint32(114).fork();
for (r = 0; r < e.silent.length; ++r) t.int32(e.silent[r]);
t.ldelim();
}
if (null != e.damageRev && Object.hasOwnProperty.call(e, "damageRev") && t.uint32(122).string(e.damageRev), 
null != e.hpRev && Object.hasOwnProperty.call(e, "hpRev") && t.uint32(130).string(e.hpRev), 
null != e.atkRev && Object.hasOwnProperty.call(e, "atkRev") && t.uint32(138).string(e.atkRev), 
null != e.atkSpeRev && Object.hasOwnProperty.call(e, "atkSpeRev") && t.uint32(144).int32(e.atkSpeRev), 
null != e.hpRev_type && Object.hasOwnProperty.call(e, "hpRev_type") && t.uint32(152).int32(e.hpRev_type), 
null != e.hurtRev && Object.hasOwnProperty.call(e, "hurtRev") && t.uint32(162).string(e.hurtRev), 
null != e.effect && e.effect.length) for (r = 0; r < e.effect.length; ++r) $root.pb.xls_string_array.encode(e.effect[r], t.uint32(170).fork()).ldelim();
if (null != e.skillChange && e.skillChange.length) for (r = 0; r < e.skillChange.length; ++r) t.uint32(178).string(e.skillChange[r]);
if (null != e.resistanceType && e.resistanceType.length) {
t.uint32(186).fork();
for (r = 0; r < e.resistanceType.length; ++r) t.int32(e.resistanceType[r]);
t.ldelim();
}
if (null != e.change && Object.hasOwnProperty.call(e, "change") && t.uint32(194).string(e.change), 
null != e.transformation && Object.hasOwnProperty.call(e, "transformation") && t.uint32(200).int32(e.transformation), 
null != e.colorChange && Object.hasOwnProperty.call(e, "colorChange") && t.uint32(210).string(e.colorChange), 
null != e.defRev && Object.hasOwnProperty.call(e, "defRev") && t.uint32(218).string(e.defRev), 
null != e.attributeRev && e.attributeRev.length) for (r = 0; r < e.attributeRev.length; ++r) t.uint32(226).string(e.attributeRev[r]);
return null != e.moveFor && Object.hasOwnProperty.call(e, "moveFor") && t.uint32(232).int32(e.moveFor), 
t;
}, wt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, wt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_status_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.name = e.int32();
break;

case 3:
n.desTra = e.int32();
break;

case 4:
n.icon = e.string();
break;

case 5:
if (n.bufftype && n.bufftype.length || (n.bufftype = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.bufftype.push(e.int32()); else n.bufftype.push(e.int32());
break;

case 6:
n.resSet = e.string();
break;

case 7:
n.superposition = e.int32();
break;

case 8:
n.resistance = e.string();
break;

case 9:
n.shield = e.string();
break;

case 10:
if (n.variable && n.variable.length || (n.variable = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.variable.push(e.int32()); else n.variable.push(e.int32());
break;

case 11:
n.condition && n.condition.length || (n.condition = []), n.condition.push(e.string());
break;

case 12:
n.attackFor = e.int32();
break;

case 13:
n.moveRev = e.int32();
break;

case 14:
if (n.silent && n.silent.length || (n.silent = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.silent.push(e.int32()); else n.silent.push(e.int32());
break;

case 15:
n.damageRev = e.string();
break;

case 16:
n.hpRev = e.string();
break;

case 17:
n.atkRev = e.string();
break;

case 18:
n.atkSpeRev = e.int32();
break;

case 19:
n.hpRev_type = e.int32();
break;

case 20:
n.hurtRev = e.string();
break;

case 21:
n.effect && n.effect.length || (n.effect = []), n.effect.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 22:
n.skillChange && n.skillChange.length || (n.skillChange = []), n.skillChange.push(e.string());
break;

case 23:
if (n.resistanceType && n.resistanceType.length || (n.resistanceType = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.resistanceType.push(e.int32()); else n.resistanceType.push(e.int32());
break;

case 24:
n.change = e.string();
break;

case 25:
n.transformation = e.int32();
break;

case 26:
n.colorChange = e.string();
break;

case 27:
n.defRev = e.string();
break;

case 28:
n.attributeRev && n.attributeRev.length || (n.attributeRev = []), n.attributeRev.push(e.string());
break;

case 29:
n.moveFor = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, wt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, wt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.icon && e.hasOwnProperty("icon") && !$util.isString(e.icon)) return "icon: string expected";
if (null != e.bufftype && e.hasOwnProperty("bufftype")) {
if (!Array.isArray(e.bufftype)) return "bufftype: array expected";
for (var t = 0; t < e.bufftype.length; ++t) if (!$util.isInteger(e.bufftype[t])) return "bufftype: integer[] expected";
}
if (null != e.resSet && e.hasOwnProperty("resSet") && !$util.isString(e.resSet)) return "resSet: string expected";
if (null != e.superposition && e.hasOwnProperty("superposition") && !$util.isInteger(e.superposition)) return "superposition: integer expected";
if (null != e.resistance && e.hasOwnProperty("resistance") && !$util.isString(e.resistance)) return "resistance: string expected";
if (null != e.shield && e.hasOwnProperty("shield") && !$util.isString(e.shield)) return "shield: string expected";
if (null != e.variable && e.hasOwnProperty("variable")) {
if (!Array.isArray(e.variable)) return "variable: array expected";
for (t = 0; t < e.variable.length; ++t) if (!$util.isInteger(e.variable[t])) return "variable: integer[] expected";
}
if (null != e.condition && e.hasOwnProperty("condition")) {
if (!Array.isArray(e.condition)) return "condition: array expected";
for (t = 0; t < e.condition.length; ++t) if (!$util.isString(e.condition[t])) return "condition: string[] expected";
}
if (null != e.attackFor && e.hasOwnProperty("attackFor") && !$util.isInteger(e.attackFor)) return "attackFor: integer expected";
if (null != e.moveRev && e.hasOwnProperty("moveRev") && !$util.isInteger(e.moveRev)) return "moveRev: integer expected";
if (null != e.silent && e.hasOwnProperty("silent")) {
if (!Array.isArray(e.silent)) return "silent: array expected";
for (t = 0; t < e.silent.length; ++t) if (!$util.isInteger(e.silent[t])) return "silent: integer[] expected";
}
if (null != e.damageRev && e.hasOwnProperty("damageRev") && !$util.isString(e.damageRev)) return "damageRev: string expected";
if (null != e.hpRev && e.hasOwnProperty("hpRev") && !$util.isString(e.hpRev)) return "hpRev: string expected";
if (null != e.atkRev && e.hasOwnProperty("atkRev") && !$util.isString(e.atkRev)) return "atkRev: string expected";
if (null != e.atkSpeRev && e.hasOwnProperty("atkSpeRev") && !$util.isInteger(e.atkSpeRev)) return "atkSpeRev: integer expected";
if (null != e.hpRev_type && e.hasOwnProperty("hpRev_type") && !$util.isInteger(e.hpRev_type)) return "hpRev_type: integer expected";
if (null != e.hurtRev && e.hasOwnProperty("hurtRev") && !$util.isString(e.hurtRev)) return "hurtRev: string expected";
if (null != e.effect && e.hasOwnProperty("effect")) {
if (!Array.isArray(e.effect)) return "effect: array expected";
for (t = 0; t < e.effect.length; ++t) {
var r = $root.pb.xls_string_array.verify(e.effect[t]);
if (r) return "effect." + r;
}
}
if (null != e.skillChange && e.hasOwnProperty("skillChange")) {
if (!Array.isArray(e.skillChange)) return "skillChange: array expected";
for (t = 0; t < e.skillChange.length; ++t) if (!$util.isString(e.skillChange[t])) return "skillChange: string[] expected";
}
if (null != e.resistanceType && e.hasOwnProperty("resistanceType")) {
if (!Array.isArray(e.resistanceType)) return "resistanceType: array expected";
for (t = 0; t < e.resistanceType.length; ++t) if (!$util.isInteger(e.resistanceType[t])) return "resistanceType: integer[] expected";
}
if (null != e.change && e.hasOwnProperty("change") && !$util.isString(e.change)) return "change: string expected";
if (null != e.transformation && e.hasOwnProperty("transformation") && !$util.isInteger(e.transformation)) return "transformation: integer expected";
if (null != e.colorChange && e.hasOwnProperty("colorChange") && !$util.isString(e.colorChange)) return "colorChange: string expected";
if (null != e.defRev && e.hasOwnProperty("defRev") && !$util.isString(e.defRev)) return "defRev: string expected";
if (null != e.attributeRev && e.hasOwnProperty("attributeRev")) {
if (!Array.isArray(e.attributeRev)) return "attributeRev: array expected";
for (t = 0; t < e.attributeRev.length; ++t) if (!$util.isString(e.attributeRev[t])) return "attributeRev: string[] expected";
}
return null != e.moveFor && e.hasOwnProperty("moveFor") && !$util.isInteger(e.moveFor) ? "moveFor: integer expected" : null;
}, wt), r.xlsdata_upgrade_options = (vt.prototype.Items = $util.emptyArray, vt.create = function(e) {
return new vt(e);
}, vt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xls_upgrade_options_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, vt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, vt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_upgrade_options(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xls_upgrade_options_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, vt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, vt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xls_upgrade_options_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, vt), r.xls_upgrade_options_item = (Ot.prototype.__id__ = 0, Ot.prototype.name = 0, 
Ot.prototype.desTra = 0, Ot.prototype.icon = "", Ot.prototype.isNew = 0, Ot.prototype.unlock = $util.emptyArray, 
Ot.prototype.premise1 = $util.emptyArray, Ot.prototype.premise2 = $util.emptyArray, 
Ot.prototype.options_num = 0, Ot.prototype.aim_skill = $util.emptyArray, Ot.prototype.up_skill = $util.emptyArray, 
Ot.prototype.change_actionOth = $util.emptyArray, Ot.prototype.change_damage = "", 
Ot.prototype.intensity_up = "", Ot.prototype.damageRange_up = "", Ot.prototype.speed_up = "", 
Ot.prototype.range_up = "", Ot.prototype.change_triggerCd = "", Ot.prototype.running_fire = "", 
Ot.prototype.salvo = "", Ot.prototype.change_damage_num = "", Ot.prototype.change_damage_interval = "", 
Ot.prototype.change_addStatus = $util.emptyArray, Ot.prototype.summon = $util.emptyArray, 
Ot.prototype.soldiers_change = $util.emptyArray, Ot.prototype.soldiers_upgrade = $util.emptyArray, 
Ot.prototype.soldiers_skill = $util.emptyArray, Ot.prototype.select = 0, Ot.prototype.type = $util.emptyArray, 
Ot.prototype.summon2 = $util.emptyArray, Ot.prototype.attribute_correct = $util.emptyArray, 
Ot.prototype.attribute_correct2 = $util.emptyArray, Ot.prototype.equip_skill = $util.emptyArray, 
Ot.prototype.active = 0, Ot.prototype.numTable = $util.emptyArray, Ot.prototype.numTxt = $util.emptyArray, 
Ot.prototype.repeat_fire = "", Ot.prototype.icon2 = $util.emptyArray, Ot.prototype.aim_type = $util.emptyArray, 
Ot.prototype.aimAdd = "", Ot.prototype.type2 = $util.emptyArray, Ot.prototype.skillGroupAdd = $util.emptyArray, 
Ot.create = function(e) {
return new Ot(e);
}, Ot.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(16).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(24).int32(e.desTra), 
null != e.icon && Object.hasOwnProperty.call(e, "icon") && t.uint32(34).string(e.icon), 
null != e.isNew && Object.hasOwnProperty.call(e, "isNew") && t.uint32(40).int32(e.isNew), 
null != e.unlock && e.unlock.length) for (var r = 0; r < e.unlock.length; ++r) t.uint32(50).string(e.unlock[r]);
if (null != e.premise1 && e.premise1.length) for (r = 0; r < e.premise1.length; ++r) $root.pb.xls_integer_array.encode(e.premise1[r], t.uint32(58).fork()).ldelim();
if (null != e.premise2 && e.premise2.length) for (r = 0; r < e.premise2.length; ++r) $root.pb.xls_integer_array.encode(e.premise2[r], t.uint32(66).fork()).ldelim();
if (null != e.options_num && Object.hasOwnProperty.call(e, "options_num") && t.uint32(72).int32(e.options_num), 
null != e.aim_skill && e.aim_skill.length) {
t.uint32(82).fork();
for (r = 0; r < e.aim_skill.length; ++r) t.int32(e.aim_skill[r]);
t.ldelim();
}
if (null != e.up_skill && e.up_skill.length) for (r = 0; r < e.up_skill.length; ++r) t.uint32(90).string(e.up_skill[r]);
if (null != e.change_actionOth && e.change_actionOth.length) for (r = 0; r < e.change_actionOth.length; ++r) $root.pb.xls_string_array.encode(e.change_actionOth[r], t.uint32(98).fork()).ldelim();
if (null != e.change_damage && Object.hasOwnProperty.call(e, "change_damage") && t.uint32(106).string(e.change_damage), 
null != e.intensity_up && Object.hasOwnProperty.call(e, "intensity_up") && t.uint32(114).string(e.intensity_up), 
null != e.damageRange_up && Object.hasOwnProperty.call(e, "damageRange_up") && t.uint32(122).string(e.damageRange_up), 
null != e.speed_up && Object.hasOwnProperty.call(e, "speed_up") && t.uint32(130).string(e.speed_up), 
null != e.range_up && Object.hasOwnProperty.call(e, "range_up") && t.uint32(138).string(e.range_up), 
null != e.change_triggerCd && Object.hasOwnProperty.call(e, "change_triggerCd") && t.uint32(146).string(e.change_triggerCd), 
null != e.running_fire && Object.hasOwnProperty.call(e, "running_fire") && t.uint32(154).string(e.running_fire), 
null != e.salvo && Object.hasOwnProperty.call(e, "salvo") && t.uint32(162).string(e.salvo), 
null != e.change_damage_num && Object.hasOwnProperty.call(e, "change_damage_num") && t.uint32(170).string(e.change_damage_num), 
null != e.change_damage_interval && Object.hasOwnProperty.call(e, "change_damage_interval") && t.uint32(178).string(e.change_damage_interval), 
null != e.change_addStatus && e.change_addStatus.length) for (r = 0; r < e.change_addStatus.length; ++r) $root.pb.xls_string_array.encode(e.change_addStatus[r], t.uint32(186).fork()).ldelim();
if (null != e.summon && e.summon.length) for (r = 0; r < e.summon.length; ++r) t.uint32(194).string(e.summon[r]);
if (null != e.soldiers_change && e.soldiers_change.length) for (r = 0; r < e.soldiers_change.length; ++r) t.uint32(202).string(e.soldiers_change[r]);
if (null != e.soldiers_upgrade && e.soldiers_upgrade.length) for (r = 0; r < e.soldiers_upgrade.length; ++r) t.uint32(210).string(e.soldiers_upgrade[r]);
if (null != e.soldiers_skill && e.soldiers_skill.length) for (r = 0; r < e.soldiers_skill.length; ++r) t.uint32(218).string(e.soldiers_skill[r]);
if (null != e.select && Object.hasOwnProperty.call(e, "select") && t.uint32(224).int32(e.select), 
null != e.type && e.type.length) {
t.uint32(234).fork();
for (r = 0; r < e.type.length; ++r) t.int32(e.type[r]);
t.ldelim();
}
if (null != e.summon2 && e.summon2.length) for (r = 0; r < e.summon2.length; ++r) t.uint32(242).string(e.summon2[r]);
if (null != e.attribute_correct && e.attribute_correct.length) for (r = 0; r < e.attribute_correct.length; ++r) t.uint32(250).string(e.attribute_correct[r]);
if (null != e.attribute_correct2 && e.attribute_correct2.length) for (r = 0; r < e.attribute_correct2.length; ++r) t.uint32(258).string(e.attribute_correct2[r]);
if (null != e.equip_skill && e.equip_skill.length) for (r = 0; r < e.equip_skill.length; ++r) t.uint32(266).string(e.equip_skill[r]);
if (null != e.active && Object.hasOwnProperty.call(e, "active") && t.uint32(272).int32(e.active), 
null != e.numTable && e.numTable.length) for (r = 0; r < e.numTable.length; ++r) $root.pb.xls_string_array.encode(e.numTable[r], t.uint32(282).fork()).ldelim();
if (null != e.numTxt && e.numTxt.length) for (r = 0; r < e.numTxt.length; ++r) $root.pb.xls_string_array.encode(e.numTxt[r], t.uint32(290).fork()).ldelim();
if (null != e.repeat_fire && Object.hasOwnProperty.call(e, "repeat_fire") && t.uint32(298).string(e.repeat_fire), 
null != e.icon2 && e.icon2.length) {
t.uint32(306).fork();
for (r = 0; r < e.icon2.length; ++r) t.int32(e.icon2[r]);
t.ldelim();
}
if (null != e.aim_type && e.aim_type.length) {
t.uint32(314).fork();
for (r = 0; r < e.aim_type.length; ++r) t.int32(e.aim_type[r]);
t.ldelim();
}
if (null != e.aimAdd && Object.hasOwnProperty.call(e, "aimAdd") && t.uint32(322).string(e.aimAdd), 
null != e.type2 && e.type2.length) {
t.uint32(330).fork();
for (r = 0; r < e.type2.length; ++r) t.int32(e.type2[r]);
t.ldelim();
}
if (null != e.skillGroupAdd && e.skillGroupAdd.length) for (r = 0; r < e.skillGroupAdd.length; ++r) $root.pb.xls_string_array.encode(e.skillGroupAdd[r], t.uint32(338).fork()).ldelim();
return t;
}, Ot.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ot.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_upgrade_options_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.name = e.int32();
break;

case 3:
n.desTra = e.int32();
break;

case 4:
n.icon = e.string();
break;

case 5:
n.isNew = e.int32();
break;

case 6:
n.unlock && n.unlock.length || (n.unlock = []), n.unlock.push(e.string());
break;

case 7:
n.premise1 && n.premise1.length || (n.premise1 = []), n.premise1.push($root.pb.xls_integer_array.decode(e, e.uint32()));
break;

case 8:
n.premise2 && n.premise2.length || (n.premise2 = []), n.premise2.push($root.pb.xls_integer_array.decode(e, e.uint32()));
break;

case 9:
n.options_num = e.int32();
break;

case 10:
if (n.aim_skill && n.aim_skill.length || (n.aim_skill = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.aim_skill.push(e.int32()); else n.aim_skill.push(e.int32());
break;

case 11:
n.up_skill && n.up_skill.length || (n.up_skill = []), n.up_skill.push(e.string());
break;

case 12:
n.change_actionOth && n.change_actionOth.length || (n.change_actionOth = []), n.change_actionOth.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 13:
n.change_damage = e.string();
break;

case 14:
n.intensity_up = e.string();
break;

case 15:
n.damageRange_up = e.string();
break;

case 16:
n.speed_up = e.string();
break;

case 17:
n.range_up = e.string();
break;

case 18:
n.change_triggerCd = e.string();
break;

case 19:
n.running_fire = e.string();
break;

case 20:
n.salvo = e.string();
break;

case 21:
n.change_damage_num = e.string();
break;

case 22:
n.change_damage_interval = e.string();
break;

case 23:
n.change_addStatus && n.change_addStatus.length || (n.change_addStatus = []), n.change_addStatus.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 24:
n.summon && n.summon.length || (n.summon = []), n.summon.push(e.string());
break;

case 25:
n.soldiers_change && n.soldiers_change.length || (n.soldiers_change = []), n.soldiers_change.push(e.string());
break;

case 26:
n.soldiers_upgrade && n.soldiers_upgrade.length || (n.soldiers_upgrade = []), n.soldiers_upgrade.push(e.string());
break;

case 27:
n.soldiers_skill && n.soldiers_skill.length || (n.soldiers_skill = []), n.soldiers_skill.push(e.string());
break;

case 28:
n.select = e.int32();
break;

case 29:
if (n.type && n.type.length || (n.type = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.type.push(e.int32()); else n.type.push(e.int32());
break;

case 30:
n.summon2 && n.summon2.length || (n.summon2 = []), n.summon2.push(e.string());
break;

case 31:
n.attribute_correct && n.attribute_correct.length || (n.attribute_correct = []), 
n.attribute_correct.push(e.string());
break;

case 32:
n.attribute_correct2 && n.attribute_correct2.length || (n.attribute_correct2 = []), 
n.attribute_correct2.push(e.string());
break;

case 33:
n.equip_skill && n.equip_skill.length || (n.equip_skill = []), n.equip_skill.push(e.string());
break;

case 34:
n.active = e.int32();
break;

case 35:
n.numTable && n.numTable.length || (n.numTable = []), n.numTable.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 36:
n.numTxt && n.numTxt.length || (n.numTxt = []), n.numTxt.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 37:
n.repeat_fire = e.string();
break;

case 38:
if (n.icon2 && n.icon2.length || (n.icon2 = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.icon2.push(e.int32()); else n.icon2.push(e.int32());
break;

case 39:
if (n.aim_type && n.aim_type.length || (n.aim_type = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.aim_type.push(e.int32()); else n.aim_type.push(e.int32());
break;

case 40:
n.aimAdd = e.string();
break;

case 41:
if (n.type2 && n.type2.length || (n.type2 = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.type2.push(e.int32()); else n.type2.push(e.int32());
break;

case 42:
n.skillGroupAdd && n.skillGroupAdd.length || (n.skillGroupAdd = []), n.skillGroupAdd.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ot.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ot.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.icon && e.hasOwnProperty("icon") && !$util.isString(e.icon)) return "icon: string expected";
if (null != e.isNew && e.hasOwnProperty("isNew") && !$util.isInteger(e.isNew)) return "isNew: integer expected";
if (null != e.unlock && e.hasOwnProperty("unlock")) {
if (!Array.isArray(e.unlock)) return "unlock: array expected";
for (var t = 0; t < e.unlock.length; ++t) if (!$util.isString(e.unlock[t])) return "unlock: string[] expected";
}
if (null != e.premise1 && e.hasOwnProperty("premise1")) {
if (!Array.isArray(e.premise1)) return "premise1: array expected";
for (t = 0; t < e.premise1.length; ++t) if (r = $root.pb.xls_integer_array.verify(e.premise1[t])) return "premise1." + r;
}
if (null != e.premise2 && e.hasOwnProperty("premise2")) {
if (!Array.isArray(e.premise2)) return "premise2: array expected";
for (t = 0; t < e.premise2.length; ++t) if (r = $root.pb.xls_integer_array.verify(e.premise2[t])) return "premise2." + r;
}
if (null != e.options_num && e.hasOwnProperty("options_num") && !$util.isInteger(e.options_num)) return "options_num: integer expected";
if (null != e.aim_skill && e.hasOwnProperty("aim_skill")) {
if (!Array.isArray(e.aim_skill)) return "aim_skill: array expected";
for (t = 0; t < e.aim_skill.length; ++t) if (!$util.isInteger(e.aim_skill[t])) return "aim_skill: integer[] expected";
}
if (null != e.up_skill && e.hasOwnProperty("up_skill")) {
if (!Array.isArray(e.up_skill)) return "up_skill: array expected";
for (t = 0; t < e.up_skill.length; ++t) if (!$util.isString(e.up_skill[t])) return "up_skill: string[] expected";
}
if (null != e.change_actionOth && e.hasOwnProperty("change_actionOth")) {
if (!Array.isArray(e.change_actionOth)) return "change_actionOth: array expected";
for (t = 0; t < e.change_actionOth.length; ++t) if (r = $root.pb.xls_string_array.verify(e.change_actionOth[t])) return "change_actionOth." + r;
}
if (null != e.change_damage && e.hasOwnProperty("change_damage") && !$util.isString(e.change_damage)) return "change_damage: string expected";
if (null != e.intensity_up && e.hasOwnProperty("intensity_up") && !$util.isString(e.intensity_up)) return "intensity_up: string expected";
if (null != e.damageRange_up && e.hasOwnProperty("damageRange_up") && !$util.isString(e.damageRange_up)) return "damageRange_up: string expected";
if (null != e.speed_up && e.hasOwnProperty("speed_up") && !$util.isString(e.speed_up)) return "speed_up: string expected";
if (null != e.range_up && e.hasOwnProperty("range_up") && !$util.isString(e.range_up)) return "range_up: string expected";
if (null != e.change_triggerCd && e.hasOwnProperty("change_triggerCd") && !$util.isString(e.change_triggerCd)) return "change_triggerCd: string expected";
if (null != e.running_fire && e.hasOwnProperty("running_fire") && !$util.isString(e.running_fire)) return "running_fire: string expected";
if (null != e.salvo && e.hasOwnProperty("salvo") && !$util.isString(e.salvo)) return "salvo: string expected";
if (null != e.change_damage_num && e.hasOwnProperty("change_damage_num") && !$util.isString(e.change_damage_num)) return "change_damage_num: string expected";
if (null != e.change_damage_interval && e.hasOwnProperty("change_damage_interval") && !$util.isString(e.change_damage_interval)) return "change_damage_interval: string expected";
if (null != e.change_addStatus && e.hasOwnProperty("change_addStatus")) {
if (!Array.isArray(e.change_addStatus)) return "change_addStatus: array expected";
for (t = 0; t < e.change_addStatus.length; ++t) if (r = $root.pb.xls_string_array.verify(e.change_addStatus[t])) return "change_addStatus." + r;
}
if (null != e.summon && e.hasOwnProperty("summon")) {
if (!Array.isArray(e.summon)) return "summon: array expected";
for (t = 0; t < e.summon.length; ++t) if (!$util.isString(e.summon[t])) return "summon: string[] expected";
}
if (null != e.soldiers_change && e.hasOwnProperty("soldiers_change")) {
if (!Array.isArray(e.soldiers_change)) return "soldiers_change: array expected";
for (t = 0; t < e.soldiers_change.length; ++t) if (!$util.isString(e.soldiers_change[t])) return "soldiers_change: string[] expected";
}
if (null != e.soldiers_upgrade && e.hasOwnProperty("soldiers_upgrade")) {
if (!Array.isArray(e.soldiers_upgrade)) return "soldiers_upgrade: array expected";
for (t = 0; t < e.soldiers_upgrade.length; ++t) if (!$util.isString(e.soldiers_upgrade[t])) return "soldiers_upgrade: string[] expected";
}
if (null != e.soldiers_skill && e.hasOwnProperty("soldiers_skill")) {
if (!Array.isArray(e.soldiers_skill)) return "soldiers_skill: array expected";
for (t = 0; t < e.soldiers_skill.length; ++t) if (!$util.isString(e.soldiers_skill[t])) return "soldiers_skill: string[] expected";
}
if (null != e.select && e.hasOwnProperty("select") && !$util.isInteger(e.select)) return "select: integer expected";
if (null != e.type && e.hasOwnProperty("type")) {
if (!Array.isArray(e.type)) return "type: array expected";
for (t = 0; t < e.type.length; ++t) if (!$util.isInteger(e.type[t])) return "type: integer[] expected";
}
if (null != e.summon2 && e.hasOwnProperty("summon2")) {
if (!Array.isArray(e.summon2)) return "summon2: array expected";
for (t = 0; t < e.summon2.length; ++t) if (!$util.isString(e.summon2[t])) return "summon2: string[] expected";
}
if (null != e.attribute_correct && e.hasOwnProperty("attribute_correct")) {
if (!Array.isArray(e.attribute_correct)) return "attribute_correct: array expected";
for (t = 0; t < e.attribute_correct.length; ++t) if (!$util.isString(e.attribute_correct[t])) return "attribute_correct: string[] expected";
}
if (null != e.attribute_correct2 && e.hasOwnProperty("attribute_correct2")) {
if (!Array.isArray(e.attribute_correct2)) return "attribute_correct2: array expected";
for (t = 0; t < e.attribute_correct2.length; ++t) if (!$util.isString(e.attribute_correct2[t])) return "attribute_correct2: string[] expected";
}
if (null != e.equip_skill && e.hasOwnProperty("equip_skill")) {
if (!Array.isArray(e.equip_skill)) return "equip_skill: array expected";
for (t = 0; t < e.equip_skill.length; ++t) if (!$util.isString(e.equip_skill[t])) return "equip_skill: string[] expected";
}
if (null != e.active && e.hasOwnProperty("active") && !$util.isInteger(e.active)) return "active: integer expected";
if (null != e.numTable && e.hasOwnProperty("numTable")) {
if (!Array.isArray(e.numTable)) return "numTable: array expected";
for (t = 0; t < e.numTable.length; ++t) if (r = $root.pb.xls_string_array.verify(e.numTable[t])) return "numTable." + r;
}
if (null != e.numTxt && e.hasOwnProperty("numTxt")) {
if (!Array.isArray(e.numTxt)) return "numTxt: array expected";
for (t = 0; t < e.numTxt.length; ++t) if (r = $root.pb.xls_string_array.verify(e.numTxt[t])) return "numTxt." + r;
}
if (null != e.repeat_fire && e.hasOwnProperty("repeat_fire") && !$util.isString(e.repeat_fire)) return "repeat_fire: string expected";
if (null != e.icon2 && e.hasOwnProperty("icon2")) {
if (!Array.isArray(e.icon2)) return "icon2: array expected";
for (t = 0; t < e.icon2.length; ++t) if (!$util.isInteger(e.icon2[t])) return "icon2: integer[] expected";
}
if (null != e.aim_type && e.hasOwnProperty("aim_type")) {
if (!Array.isArray(e.aim_type)) return "aim_type: array expected";
for (t = 0; t < e.aim_type.length; ++t) if (!$util.isInteger(e.aim_type[t])) return "aim_type: integer[] expected";
}
if (null != e.aimAdd && e.hasOwnProperty("aimAdd") && !$util.isString(e.aimAdd)) return "aimAdd: string expected";
if (null != e.type2 && e.hasOwnProperty("type2")) {
if (!Array.isArray(e.type2)) return "type2: array expected";
for (t = 0; t < e.type2.length; ++t) if (!$util.isInteger(e.type2[t])) return "type2: integer[] expected";
}
if (null != e.skillGroupAdd && e.hasOwnProperty("skillGroupAdd")) {
if (!Array.isArray(e.skillGroupAdd)) return "skillGroupAdd: array expected";
var r;
for (t = 0; t < e.skillGroupAdd.length; ++t) if (r = $root.pb.xls_string_array.verify(e.skillGroupAdd[t])) return "skillGroupAdd." + r;
}
return null;
}, Ot), r.xlsdata_wall = ($t.prototype.Items = $util.emptyArray, $t.create = function(e) {
return new $t(e);
}, $t.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_wall_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, $t.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, $t.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_wall(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_wall_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, $t.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, $t.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_wall_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, $t), r.xlsdata_wall_item = (Rt.prototype.__id__ = 0, Rt.prototype.hp = "", Rt.prototype.atk = "", 
Rt.prototype.range = 0, Rt.prototype.missile_effect = "", Rt.prototype.atkSpe = 0, 
Rt.create = function(e) {
return new Rt(e);
}, Rt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.hp && Object.hasOwnProperty.call(e, "hp") && t.uint32(18).string(e.hp), 
null != e.atk && Object.hasOwnProperty.call(e, "atk") && t.uint32(26).string(e.atk), 
null != e.range && Object.hasOwnProperty.call(e, "range") && t.uint32(32).int32(e.range), 
null != e.missile_effect && Object.hasOwnProperty.call(e, "missile_effect") && t.uint32(42).string(e.missile_effect), 
null != e.atkSpe && Object.hasOwnProperty.call(e, "atkSpe") && t.uint32(48).int32(e.atkSpe), 
t;
}, Rt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Rt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_wall_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.hp = e.string();
break;

case 3:
n.atk = e.string();
break;

case 4:
n.range = e.int32();
break;

case 5:
n.missile_effect = e.string();
break;

case 6:
n.atkSpe = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Rt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Rt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__) ? "__id__: integer expected" : null != e.hp && e.hasOwnProperty("hp") && !$util.isString(e.hp) ? "hp: string expected" : null != e.atk && e.hasOwnProperty("atk") && !$util.isString(e.atk) ? "atk: string expected" : null != e.range && e.hasOwnProperty("range") && !$util.isInteger(e.range) ? "range: integer expected" : null != e.missile_effect && e.hasOwnProperty("missile_effect") && !$util.isString(e.missile_effect) ? "missile_effect: string expected" : null != e.atkSpe && e.hasOwnProperty("atkSpe") && !$util.isInteger(e.atkSpe) ? "atkSpe: integer expected" : null;
}, Rt), r.xlsdata_attribute = (It.prototype.Items = $util.emptyArray, It.create = function(e) {
return new It(e);
}, It.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_attribute_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, It.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, It.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_attribute(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_attribute_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, It.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, It.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_attribute_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, It), r.xlsdata_attribute_item = (kt.prototype.__id__ = 0, kt.prototype.name = 0, 
kt.prototype.desTra = 0, kt.prototype.sort = 0, kt.prototype.num = 0, kt.prototype.base = 0, 
kt.create = function(e) {
return new kt(e);
}, kt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(16).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(24).int32(e.desTra), 
null != e.sort && Object.hasOwnProperty.call(e, "sort") && t.uint32(32).int32(e.sort), 
null != e.num && Object.hasOwnProperty.call(e, "num") && t.uint32(40).int32(e.num), 
null != e.base && Object.hasOwnProperty.call(e, "base") && t.uint32(48).int32(e.base), 
t;
}, kt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, kt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_attribute_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.name = e.int32();
break;

case 3:
n.desTra = e.int32();
break;

case 4:
n.sort = e.int32();
break;

case 5:
n.num = e.int32();
break;

case 6:
n.base = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, kt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, kt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__) ? "__id__: integer expected" : null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name) ? "name: integer expected" : null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra) ? "desTra: integer expected" : null != e.sort && e.hasOwnProperty("sort") && !$util.isInteger(e.sort) ? "sort: integer expected" : null != e.num && e.hasOwnProperty("num") && !$util.isInteger(e.num) ? "num: integer expected" : null != e.base && e.hasOwnProperty("base") && !$util.isInteger(e.base) ? "base: integer expected" : null;
}, kt), r.xlsdata_attribute_upgrade = (Pt.prototype.Items = $util.emptyArray, Pt.create = function(e) {
return new Pt(e);
}, Pt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_attribute_upgrade_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, Pt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Pt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_attribute_upgrade(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_attribute_upgrade_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Pt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Pt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_attribute_upgrade_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, Pt), r.xlsdata_attribute_upgrade_item = (_t.prototype.__id__ = 0, _t.prototype.quality1 = "", 
_t.prototype.quality2 = "", _t.prototype.quality3 = "", _t.prototype.quality4 = "", 
_t.prototype.quality5 = "", _t.prototype.quality6 = "", _t.prototype.quality7 = "", 
_t.prototype.quality8 = "", _t.prototype.quality9 = "", _t.prototype.quality10 = "", 
_t.prototype.limitationMin = "", _t.prototype.limitationMax = "", _t.create = function(e) {
return new _t(e);
}, _t.encode = function(e, t) {
return t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.quality1 && Object.hasOwnProperty.call(e, "quality1") && t.uint32(18).string(e.quality1), 
null != e.quality2 && Object.hasOwnProperty.call(e, "quality2") && t.uint32(26).string(e.quality2), 
null != e.quality3 && Object.hasOwnProperty.call(e, "quality3") && t.uint32(34).string(e.quality3), 
null != e.quality4 && Object.hasOwnProperty.call(e, "quality4") && t.uint32(42).string(e.quality4), 
null != e.quality5 && Object.hasOwnProperty.call(e, "quality5") && t.uint32(50).string(e.quality5), 
null != e.quality6 && Object.hasOwnProperty.call(e, "quality6") && t.uint32(58).string(e.quality6), 
null != e.quality7 && Object.hasOwnProperty.call(e, "quality7") && t.uint32(66).string(e.quality7), 
null != e.quality8 && Object.hasOwnProperty.call(e, "quality8") && t.uint32(74).string(e.quality8), 
null != e.quality9 && Object.hasOwnProperty.call(e, "quality9") && t.uint32(82).string(e.quality9), 
null != e.quality10 && Object.hasOwnProperty.call(e, "quality10") && t.uint32(90).string(e.quality10), 
null != e.limitationMin && Object.hasOwnProperty.call(e, "limitationMin") && t.uint32(98).string(e.limitationMin), 
null != e.limitationMax && Object.hasOwnProperty.call(e, "limitationMax") && t.uint32(106).string(e.limitationMax), 
t;
}, _t.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, _t.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_attribute_upgrade_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.quality1 = e.string();
break;

case 3:
n.quality2 = e.string();
break;

case 4:
n.quality3 = e.string();
break;

case 5:
n.quality4 = e.string();
break;

case 6:
n.quality5 = e.string();
break;

case 7:
n.quality6 = e.string();
break;

case 8:
n.quality7 = e.string();
break;

case 9:
n.quality8 = e.string();
break;

case 10:
n.quality9 = e.string();
break;

case 11:
n.quality10 = e.string();
break;

case 12:
n.limitationMin = e.string();
break;

case 13:
n.limitationMax = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, _t.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, _t.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__) ? "__id__: integer expected" : null != e.quality1 && e.hasOwnProperty("quality1") && !$util.isString(e.quality1) ? "quality1: string expected" : null != e.quality2 && e.hasOwnProperty("quality2") && !$util.isString(e.quality2) ? "quality2: string expected" : null != e.quality3 && e.hasOwnProperty("quality3") && !$util.isString(e.quality3) ? "quality3: string expected" : null != e.quality4 && e.hasOwnProperty("quality4") && !$util.isString(e.quality4) ? "quality4: string expected" : null != e.quality5 && e.hasOwnProperty("quality5") && !$util.isString(e.quality5) ? "quality5: string expected" : null != e.quality6 && e.hasOwnProperty("quality6") && !$util.isString(e.quality6) ? "quality6: string expected" : null != e.quality7 && e.hasOwnProperty("quality7") && !$util.isString(e.quality7) ? "quality7: string expected" : null != e.quality8 && e.hasOwnProperty("quality8") && !$util.isString(e.quality8) ? "quality8: string expected" : null != e.quality9 && e.hasOwnProperty("quality9") && !$util.isString(e.quality9) ? "quality9: string expected" : null != e.quality10 && e.hasOwnProperty("quality10") && !$util.isString(e.quality10) ? "quality10: string expected" : null != e.limitationMin && e.hasOwnProperty("limitationMin") && !$util.isString(e.limitationMin) ? "limitationMin: string expected" : null != e.limitationMax && e.hasOwnProperty("limitationMax") && !$util.isString(e.limitationMax) ? "limitationMax: string expected" : null;
}, _t), r.xlsdata_equip = (xt.prototype.Items = $util.emptyArray, xt.create = function(e) {
return new xt(e);
}, xt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_equip_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, xt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, xt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_equip(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_equip_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, xt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, xt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_equip_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, xt), r.xlsdata_equip_item = (At.prototype.__id__ = 0, At.prototype.name = 0, 
At.prototype.desTra = 0, At.prototype.icon = "", At.prototype.type = 0, At.prototype.num = 0, 
At.prototype.quality = 0, At.prototype.equip_skill = $util.emptyArray, At.prototype.upgrade = $util.emptyArray, 
At.prototype.attribute_correct = $util.emptyArray, At.create = function(e) {
return new At(e);
}, At.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(16).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(24).int32(e.desTra), 
null != e.icon && Object.hasOwnProperty.call(e, "icon") && t.uint32(34).string(e.icon), 
null != e.type && Object.hasOwnProperty.call(e, "type") && t.uint32(40).int32(e.type), 
null != e.num && Object.hasOwnProperty.call(e, "num") && t.uint32(48).int32(e.num), 
null != e.quality && Object.hasOwnProperty.call(e, "quality") && t.uint32(56).int32(e.quality), 
null != e.equip_skill && e.equip_skill.length) {
t.uint32(66).fork();
for (var r = 0; r < e.equip_skill.length; ++r) t.int32(e.equip_skill[r]);
t.ldelim();
}
if (null != e.upgrade && e.upgrade.length) for (r = 0; r < e.upgrade.length; ++r) $root.pb.xls_string_array.encode(e.upgrade[r], t.uint32(74).fork()).ldelim();
if (null != e.attribute_correct && e.attribute_correct.length) for (r = 0; r < e.attribute_correct.length; ++r) t.uint32(82).string(e.attribute_correct[r]);
return t;
}, At.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, At.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_equip_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.name = e.int32();
break;

case 3:
n.desTra = e.int32();
break;

case 4:
n.icon = e.string();
break;

case 5:
n.type = e.int32();
break;

case 6:
n.num = e.int32();
break;

case 7:
n.quality = e.int32();
break;

case 8:
if (n.equip_skill && n.equip_skill.length || (n.equip_skill = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.equip_skill.push(e.int32()); else n.equip_skill.push(e.int32());
break;

case 9:
n.upgrade && n.upgrade.length || (n.upgrade = []), n.upgrade.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

case 10:
n.attribute_correct && n.attribute_correct.length || (n.attribute_correct = []), 
n.attribute_correct.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, At.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, At.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.icon && e.hasOwnProperty("icon") && !$util.isString(e.icon)) return "icon: string expected";
if (null != e.type && e.hasOwnProperty("type") && !$util.isInteger(e.type)) return "type: integer expected";
if (null != e.num && e.hasOwnProperty("num") && !$util.isInteger(e.num)) return "num: integer expected";
if (null != e.quality && e.hasOwnProperty("quality") && !$util.isInteger(e.quality)) return "quality: integer expected";
if (null != e.equip_skill && e.hasOwnProperty("equip_skill")) {
if (!Array.isArray(e.equip_skill)) return "equip_skill: array expected";
for (var t = 0; t < e.equip_skill.length; ++t) if (!$util.isInteger(e.equip_skill[t])) return "equip_skill: integer[] expected";
}
if (null != e.upgrade && e.hasOwnProperty("upgrade")) {
if (!Array.isArray(e.upgrade)) return "upgrade: array expected";
for (t = 0; t < e.upgrade.length; ++t) {
var r = $root.pb.xls_string_array.verify(e.upgrade[t]);
if (r) return "upgrade." + r;
}
}
if (null != e.attribute_correct && e.hasOwnProperty("attribute_correct")) {
if (!Array.isArray(e.attribute_correct)) return "attribute_correct: array expected";
for (t = 0; t < e.attribute_correct.length; ++t) if (!$util.isString(e.attribute_correct[t])) return "attribute_correct: string[] expected";
}
return null;
}, At), r.xlsdata_equip_skill = (jt.prototype.Items = $util.emptyArray, jt.create = function(e) {
return new jt(e);
}, jt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_equip_skill_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, jt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, jt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_equip_skill(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_equip_skill_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, jt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, jt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_equip_skill_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, jt), r.xlsdata_equip_skill_item = (St.prototype.__id__ = 0, St.prototype.type = 0, 
St.prototype.typeLimit = 0, St.prototype.position = $util.emptyArray, St.prototype.quality = 0, 
St.prototype.limit = $util.emptyArray, St.prototype.rare = 0, St.prototype.name = 0, 
St.prototype.desTra = 0, St.prototype.value1 = $util.emptyArray, St.prototype.value2 = 0, 
St.prototype.merge = $util.emptyArray, St.prototype.upgrade = $util.emptyArray, 
St.create = function(e) {
return new St(e);
}, St.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.type && Object.hasOwnProperty.call(e, "type") && t.uint32(16).int32(e.type), 
null != e.typeLimit && Object.hasOwnProperty.call(e, "typeLimit") && t.uint32(24).int32(e.typeLimit), 
null != e.position && e.position.length) {
t.uint32(34).fork();
for (var r = 0; r < e.position.length; ++r) t.int32(e.position[r]);
t.ldelim();
}
if (null != e.quality && Object.hasOwnProperty.call(e, "quality") && t.uint32(40).int32(e.quality), 
null != e.limit && e.limit.length) {
t.uint32(50).fork();
for (r = 0; r < e.limit.length; ++r) t.int32(e.limit[r]);
t.ldelim();
}
if (null != e.rare && Object.hasOwnProperty.call(e, "rare") && t.uint32(56).int32(e.rare), 
null != e.name && Object.hasOwnProperty.call(e, "name") && t.uint32(64).int32(e.name), 
null != e.desTra && Object.hasOwnProperty.call(e, "desTra") && t.uint32(72).int32(e.desTra), 
null != e.value1 && e.value1.length) {
t.uint32(82).fork();
for (r = 0; r < e.value1.length; ++r) t.float(e.value1[r]);
t.ldelim();
}
if (null != e.value2 && Object.hasOwnProperty.call(e, "value2") && t.uint32(93).float(e.value2), 
null != e.merge && e.merge.length) {
t.uint32(98).fork();
for (r = 0; r < e.merge.length; ++r) t.int32(e.merge[r]);
t.ldelim();
}
if (null != e.upgrade && e.upgrade.length) for (r = 0; r < e.upgrade.length; ++r) $root.pb.xls_string_array.encode(e.upgrade[r], t.uint32(106).fork()).ldelim();
return t;
}, St.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, St.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_equip_skill_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.type = e.int32();
break;

case 3:
n.typeLimit = e.int32();
break;

case 4:
if (n.position && n.position.length || (n.position = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.position.push(e.int32()); else n.position.push(e.int32());
break;

case 5:
n.quality = e.int32();
break;

case 6:
if (n.limit && n.limit.length || (n.limit = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.limit.push(e.int32()); else n.limit.push(e.int32());
break;

case 7:
n.rare = e.int32();
break;

case 8:
n.name = e.int32();
break;

case 9:
n.desTra = e.int32();
break;

case 10:
if (n.value1 && n.value1.length || (n.value1 = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.value1.push(e.float()); else n.value1.push(e.float());
break;

case 11:
n.value2 = e.float();
break;

case 12:
if (n.merge && n.merge.length || (n.merge = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.merge.push(e.int32()); else n.merge.push(e.int32());
break;

case 13:
n.upgrade && n.upgrade.length || (n.upgrade = []), n.upgrade.push($root.pb.xls_string_array.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, St.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, St.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.type && e.hasOwnProperty("type") && !$util.isInteger(e.type)) return "type: integer expected";
if (null != e.typeLimit && e.hasOwnProperty("typeLimit") && !$util.isInteger(e.typeLimit)) return "typeLimit: integer expected";
if (null != e.position && e.hasOwnProperty("position")) {
if (!Array.isArray(e.position)) return "position: array expected";
for (var t = 0; t < e.position.length; ++t) if (!$util.isInteger(e.position[t])) return "position: integer[] expected";
}
if (null != e.quality && e.hasOwnProperty("quality") && !$util.isInteger(e.quality)) return "quality: integer expected";
if (null != e.limit && e.hasOwnProperty("limit")) {
if (!Array.isArray(e.limit)) return "limit: array expected";
for (t = 0; t < e.limit.length; ++t) if (!$util.isInteger(e.limit[t])) return "limit: integer[] expected";
}
if (null != e.rare && e.hasOwnProperty("rare") && !$util.isInteger(e.rare)) return "rare: integer expected";
if (null != e.name && e.hasOwnProperty("name") && !$util.isInteger(e.name)) return "name: integer expected";
if (null != e.desTra && e.hasOwnProperty("desTra") && !$util.isInteger(e.desTra)) return "desTra: integer expected";
if (null != e.value1 && e.hasOwnProperty("value1")) {
if (!Array.isArray(e.value1)) return "value1: array expected";
for (t = 0; t < e.value1.length; ++t) if ("number" != typeof e.value1[t]) return "value1: number[] expected";
}
if (null != e.value2 && e.hasOwnProperty("value2") && "number" != typeof e.value2) return "value2: number expected";
if (null != e.merge && e.hasOwnProperty("merge")) {
if (!Array.isArray(e.merge)) return "merge: array expected";
for (t = 0; t < e.merge.length; ++t) if (!$util.isInteger(e.merge[t])) return "merge: integer[] expected";
}
if (null != e.upgrade && e.hasOwnProperty("upgrade")) {
if (!Array.isArray(e.upgrade)) return "upgrade: array expected";
for (t = 0; t < e.upgrade.length; ++t) {
var r = $root.pb.xls_string_array.verify(e.upgrade[t]);
if (r) return "upgrade." + r;
}
}
return null;
}, St), r.xlsdata_function_price = (Tt.prototype.Items = $util.emptyArray, Tt.create = function(e) {
return new Tt(e);
}, Tt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_function_price_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, Tt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Tt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_function_price(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_function_price_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Tt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Tt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_function_price_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, Tt), r.xlsdata_function_price_item = (Ct.prototype.__id__ = 0, Ct.prototype.type = "", 
Ct.prototype.price = 0, Ct.create = function(e) {
return new Ct(e);
}, Ct.encode = function(e, t) {
return t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.type && Object.hasOwnProperty.call(e, "type") && t.uint32(18).string(e.type), 
null != e.price && Object.hasOwnProperty.call(e, "price") && t.uint32(24).int32(e.price), 
t;
}, Ct.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ct.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_function_price_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.type = e.string();
break;

case 3:
n.price = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ct.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ct.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__) ? "__id__: integer expected" : null != e.type && e.hasOwnProperty("type") && !$util.isString(e.type) ? "type: string expected" : null != e.price && e.hasOwnProperty("price") && !$util.isInteger(e.price) ? "price: integer expected" : null;
}, Ct), r.xlsdata_function_unlock = (Lt.prototype.Items = $util.emptyArray, Lt.create = function(e) {
return new Lt(e);
}, Lt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_function_unlock_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, Lt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Lt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_function_unlock(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_function_unlock_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Lt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Lt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_function_unlock_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, Lt), r.xlsdata_function_unlock_item = (Dt.prototype.__id__ = 0, Dt.prototype.lock = 0, 
Dt.create = function(e) {
return new Dt(e);
}, Dt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.lock && Object.hasOwnProperty.call(e, "lock") && t.uint32(16).int32(e.lock), 
t;
}, Dt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Dt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_function_unlock_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.lock = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Dt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Dt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__) ? "__id__: integer expected" : null != e.lock && e.hasOwnProperty("lock") && !$util.isInteger(e.lock) ? "lock: integer expected" : null;
}, Dt), r.xlsdata_enemyLevel = (Gt.prototype.Items = $util.emptyArray, Gt.create = function(e) {
return new Gt(e);
}, Gt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.xlsdata_enemyLevel_item.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, Gt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Gt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_enemyLevel(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.xlsdata_enemyLevel_item.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Gt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Gt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.xlsdata_enemyLevel_item.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, Gt), r.xlsdata_enemyLevel_item = (Et.prototype.__id__ = 0, Et.prototype.waves = 0, 
Et.prototype.physical = 0, Et.prototype.enemy_group = $util.emptyArray, Et.prototype.cream_pro = "", 
Et.prototype.boss_group = $util.emptyArray, Et.prototype.waves_group = $util.emptyArray, 
Et.prototype.waves_num = $util.emptyArray, Et.prototype.enemy_num = $util.emptyArray, 
Et.prototype.wave_interval = $util.emptyArray, Et.prototype.grow_up = $util.emptyArray, 
Et.prototype.hp_up = 0, Et.prototype.atk_up = 0, Et.prototype.rewardFirst1 = $util.emptyArray, 
Et.prototype.rewardFirst2 = $util.emptyArray, Et.prototype.rewardFirst3 = $util.emptyArray, 
Et.prototype.premise_wave = $util.emptyArray, Et.prototype.reward = $util.emptyArray, 
Et.prototype.reward2 = $util.emptyArray, Et.prototype.grow_up_hp = $util.emptyArray, 
Et.create = function(e) {
return new Et(e);
}, Et.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.__id__ && Object.hasOwnProperty.call(e, "__id__") && t.uint32(8).int32(e.__id__), 
null != e.waves && Object.hasOwnProperty.call(e, "waves") && t.uint32(16).int32(e.waves), 
null != e.physical && Object.hasOwnProperty.call(e, "physical") && t.uint32(24).int32(e.physical), 
null != e.enemy_group && e.enemy_group.length) {
t.uint32(34).fork();
for (var r = 0; r < e.enemy_group.length; ++r) t.int32(e.enemy_group[r]);
t.ldelim();
}
if (null != e.cream_pro && Object.hasOwnProperty.call(e, "cream_pro") && t.uint32(42).string(e.cream_pro), 
null != e.boss_group && e.boss_group.length) for (r = 0; r < e.boss_group.length; ++r) $root.pb.xls_integer_array.encode(e.boss_group[r], t.uint32(50).fork()).ldelim();
if (null != e.waves_group && e.waves_group.length) for (r = 0; r < e.waves_group.length; ++r) $root.pb.xls_integer_array.encode(e.waves_group[r], t.uint32(58).fork()).ldelim();
if (null != e.waves_num && e.waves_num.length) {
t.uint32(66).fork();
for (r = 0; r < e.waves_num.length; ++r) t.int32(e.waves_num[r]);
t.ldelim();
}
if (null != e.enemy_num && e.enemy_num.length) {
t.uint32(74).fork();
for (r = 0; r < e.enemy_num.length; ++r) t.int32(e.enemy_num[r]);
t.ldelim();
}
if (null != e.wave_interval && e.wave_interval.length) {
t.uint32(82).fork();
for (r = 0; r < e.wave_interval.length; ++r) t.int32(e.wave_interval[r]);
t.ldelim();
}
if (null != e.grow_up && e.grow_up.length) {
t.uint32(90).fork();
for (r = 0; r < e.grow_up.length; ++r) t.int32(e.grow_up[r]);
t.ldelim();
}
if (null != e.hp_up && Object.hasOwnProperty.call(e, "hp_up") && t.uint32(96).int32(e.hp_up), 
null != e.atk_up && Object.hasOwnProperty.call(e, "atk_up") && t.uint32(104).int32(e.atk_up), 
null != e.rewardFirst1 && e.rewardFirst1.length) for (r = 0; r < e.rewardFirst1.length; ++r) t.uint32(114).string(e.rewardFirst1[r]);
if (null != e.rewardFirst2 && e.rewardFirst2.length) for (r = 0; r < e.rewardFirst2.length; ++r) t.uint32(122).string(e.rewardFirst2[r]);
if (null != e.rewardFirst3 && e.rewardFirst3.length) for (r = 0; r < e.rewardFirst3.length; ++r) t.uint32(130).string(e.rewardFirst3[r]);
if (null != e.premise_wave && e.premise_wave.length) for (r = 0; r < e.premise_wave.length; ++r) t.uint32(138).string(e.premise_wave[r]);
if (null != e.reward && e.reward.length) for (r = 0; r < e.reward.length; ++r) t.uint32(146).string(e.reward[r]);
if (null != e.reward2 && e.reward2.length) for (r = 0; r < e.reward2.length; ++r) t.uint32(154).string(e.reward2[r]);
if (null != e.grow_up_hp && e.grow_up_hp.length) {
t.uint32(162).fork();
for (r = 0; r < e.grow_up_hp.length; ++r) t.int32(e.grow_up_hp[r]);
t.ldelim();
}
return t;
}, Et.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Et.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xlsdata_enemyLevel_item(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.__id__ = e.int32();
break;

case 2:
n.waves = e.int32();
break;

case 3:
n.physical = e.int32();
break;

case 4:
if (n.enemy_group && n.enemy_group.length || (n.enemy_group = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.enemy_group.push(e.int32()); else n.enemy_group.push(e.int32());
break;

case 5:
n.cream_pro = e.string();
break;

case 6:
n.boss_group && n.boss_group.length || (n.boss_group = []), n.boss_group.push($root.pb.xls_integer_array.decode(e, e.uint32()));
break;

case 7:
n.waves_group && n.waves_group.length || (n.waves_group = []), n.waves_group.push($root.pb.xls_integer_array.decode(e, e.uint32()));
break;

case 8:
if (n.waves_num && n.waves_num.length || (n.waves_num = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.waves_num.push(e.int32()); else n.waves_num.push(e.int32());
break;

case 9:
if (n.enemy_num && n.enemy_num.length || (n.enemy_num = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.enemy_num.push(e.int32()); else n.enemy_num.push(e.int32());
break;

case 10:
if (n.wave_interval && n.wave_interval.length || (n.wave_interval = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.wave_interval.push(e.int32()); else n.wave_interval.push(e.int32());
break;

case 11:
if (n.grow_up && n.grow_up.length || (n.grow_up = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.grow_up.push(e.int32()); else n.grow_up.push(e.int32());
break;

case 12:
n.hp_up = e.int32();
break;

case 13:
n.atk_up = e.int32();
break;

case 14:
n.rewardFirst1 && n.rewardFirst1.length || (n.rewardFirst1 = []), n.rewardFirst1.push(e.string());
break;

case 15:
n.rewardFirst2 && n.rewardFirst2.length || (n.rewardFirst2 = []), n.rewardFirst2.push(e.string());
break;

case 16:
n.rewardFirst3 && n.rewardFirst3.length || (n.rewardFirst3 = []), n.rewardFirst3.push(e.string());
break;

case 17:
n.premise_wave && n.premise_wave.length || (n.premise_wave = []), n.premise_wave.push(e.string());
break;

case 18:
n.reward && n.reward.length || (n.reward = []), n.reward.push(e.string());
break;

case 19:
n.reward2 && n.reward2.length || (n.reward2 = []), n.reward2.push(e.string());
break;

case 20:
if (n.grow_up_hp && n.grow_up_hp.length || (n.grow_up_hp = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.grow_up_hp.push(e.int32()); else n.grow_up_hp.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Et.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Et.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.__id__ && e.hasOwnProperty("__id__") && !$util.isInteger(e.__id__)) return "__id__: integer expected";
if (null != e.waves && e.hasOwnProperty("waves") && !$util.isInteger(e.waves)) return "waves: integer expected";
if (null != e.physical && e.hasOwnProperty("physical") && !$util.isInteger(e.physical)) return "physical: integer expected";
if (null != e.enemy_group && e.hasOwnProperty("enemy_group")) {
if (!Array.isArray(e.enemy_group)) return "enemy_group: array expected";
for (var t = 0; t < e.enemy_group.length; ++t) if (!$util.isInteger(e.enemy_group[t])) return "enemy_group: integer[] expected";
}
if (null != e.cream_pro && e.hasOwnProperty("cream_pro") && !$util.isString(e.cream_pro)) return "cream_pro: string expected";
if (null != e.boss_group && e.hasOwnProperty("boss_group")) {
if (!Array.isArray(e.boss_group)) return "boss_group: array expected";
for (t = 0; t < e.boss_group.length; ++t) if (r = $root.pb.xls_integer_array.verify(e.boss_group[t])) return "boss_group." + r;
}
if (null != e.waves_group && e.hasOwnProperty("waves_group")) {
if (!Array.isArray(e.waves_group)) return "waves_group: array expected";
var r;
for (t = 0; t < e.waves_group.length; ++t) if (r = $root.pb.xls_integer_array.verify(e.waves_group[t])) return "waves_group." + r;
}
if (null != e.waves_num && e.hasOwnProperty("waves_num")) {
if (!Array.isArray(e.waves_num)) return "waves_num: array expected";
for (t = 0; t < e.waves_num.length; ++t) if (!$util.isInteger(e.waves_num[t])) return "waves_num: integer[] expected";
}
if (null != e.enemy_num && e.hasOwnProperty("enemy_num")) {
if (!Array.isArray(e.enemy_num)) return "enemy_num: array expected";
for (t = 0; t < e.enemy_num.length; ++t) if (!$util.isInteger(e.enemy_num[t])) return "enemy_num: integer[] expected";
}
if (null != e.wave_interval && e.hasOwnProperty("wave_interval")) {
if (!Array.isArray(e.wave_interval)) return "wave_interval: array expected";
for (t = 0; t < e.wave_interval.length; ++t) if (!$util.isInteger(e.wave_interval[t])) return "wave_interval: integer[] expected";
}
if (null != e.grow_up && e.hasOwnProperty("grow_up")) {
if (!Array.isArray(e.grow_up)) return "grow_up: array expected";
for (t = 0; t < e.grow_up.length; ++t) if (!$util.isInteger(e.grow_up[t])) return "grow_up: integer[] expected";
}
if (null != e.hp_up && e.hasOwnProperty("hp_up") && !$util.isInteger(e.hp_up)) return "hp_up: integer expected";
if (null != e.atk_up && e.hasOwnProperty("atk_up") && !$util.isInteger(e.atk_up)) return "atk_up: integer expected";
if (null != e.rewardFirst1 && e.hasOwnProperty("rewardFirst1")) {
if (!Array.isArray(e.rewardFirst1)) return "rewardFirst1: array expected";
for (t = 0; t < e.rewardFirst1.length; ++t) if (!$util.isString(e.rewardFirst1[t])) return "rewardFirst1: string[] expected";
}
if (null != e.rewardFirst2 && e.hasOwnProperty("rewardFirst2")) {
if (!Array.isArray(e.rewardFirst2)) return "rewardFirst2: array expected";
for (t = 0; t < e.rewardFirst2.length; ++t) if (!$util.isString(e.rewardFirst2[t])) return "rewardFirst2: string[] expected";
}
if (null != e.rewardFirst3 && e.hasOwnProperty("rewardFirst3")) {
if (!Array.isArray(e.rewardFirst3)) return "rewardFirst3: array expected";
for (t = 0; t < e.rewardFirst3.length; ++t) if (!$util.isString(e.rewardFirst3[t])) return "rewardFirst3: string[] expected";
}
if (null != e.premise_wave && e.hasOwnProperty("premise_wave")) {
if (!Array.isArray(e.premise_wave)) return "premise_wave: array expected";
for (t = 0; t < e.premise_wave.length; ++t) if (!$util.isString(e.premise_wave[t])) return "premise_wave: string[] expected";
}
if (null != e.reward && e.hasOwnProperty("reward")) {
if (!Array.isArray(e.reward)) return "reward: array expected";
for (t = 0; t < e.reward.length; ++t) if (!$util.isString(e.reward[t])) return "reward: string[] expected";
}
if (null != e.reward2 && e.hasOwnProperty("reward2")) {
if (!Array.isArray(e.reward2)) return "reward2: array expected";
for (t = 0; t < e.reward2.length; ++t) if (!$util.isString(e.reward2[t])) return "reward2: string[] expected";
}
if (null != e.grow_up_hp && e.hasOwnProperty("grow_up_hp")) {
if (!Array.isArray(e.grow_up_hp)) return "grow_up_hp: array expected";
for (t = 0; t < e.grow_up_hp.length; ++t) if (!$util.isInteger(e.grow_up_hp[t])) return "grow_up_hp: integer[] expected";
}
return null;
}, Et), r.xls_integer_array = (Wt.prototype.Items = $util.emptyArray, Wt.create = function(e) {
return new Wt(e);
}, Wt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Items.length; ++r) t.int32(e.Items[r]);
t.ldelim();
}
return t;
}, Wt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Wt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_integer_array(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.Items && n.Items.length || (n.Items = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Items.push(e.int32()); else n.Items.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, Wt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Wt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) if (!$util.isInteger(e.Items[t])) return "Items: integer[] expected";
}
return null;
}, Wt), r.xls_string_array = (Ht.prototype.Items = $util.emptyArray, Ht.create = function(e) {
return new Ht(e);
}, Ht.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) t.uint32(10).string(e.Items[r]);
return t;
}, Ht.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ht.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_string_array(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push(e.string())) : e.skipType(7 & i);
}
return n;
}, Ht.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ht.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) if (!$util.isString(e.Items[t])) return "Items: string[] expected";
}
return null;
}, Ht), r.xls_float_array = (Bt.prototype.Items = $util.emptyArray, Bt.create = function(e) {
return new Bt(e);
}, Bt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Items.length; ++r) t.float(e.Items[r]);
t.ldelim();
}
return t;
}, Bt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Bt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.xls_float_array(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.Items && n.Items.length || (n.Items = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Items.push(e.float()); else n.Items.push(e.float()); else e.skipType(7 & i);
}
return n;
}, Bt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Bt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) if ("number" != typeof e.Items[t]) return "Items: number[] expected";
}
return null;
}, Bt), r.XlsData = (Ut.prototype.Name = "", Ut.prototype.Data = $util.newBuffer([]), 
Ut.create = function(e) {
return new Ut(e);
}, Ut.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(10).string(e.Name), 
null != e.Data && Object.hasOwnProperty.call(e, "Data") && t.uint32(18).bytes(e.Data), 
t;
}, Ut.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ut.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.XlsData(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Name = e.string();
break;

case 2:
n.Data = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ut.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ut.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null != e.Data && e.hasOwnProperty("Data") && !(e.Data && "number" == typeof e.Data.length || $util.isString(e.Data)) ? "Data: buffer expected" : null;
}, Ut), r.FetchLevelRankReply = (Nt.prototype.RankList = null, Nt.prototype.TermRankList = null, 
Nt.create = function(e) {
return new Nt(e);
}, Nt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RankList && Object.hasOwnProperty.call(e, "RankList") && $root.pb.LevelRankInfo.encode(e.RankList, t.uint32(10).fork()).ldelim(), 
null != e.TermRankList && Object.hasOwnProperty.call(e, "TermRankList") && $root.pb.LevelRankInfo.encode(e.TermRankList, t.uint32(18).fork()).ldelim(), 
t;
}, Nt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Nt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchLevelRankReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.RankList = $root.pb.LevelRankInfo.decode(e, e.uint32());
break;

case 2:
n.TermRankList = $root.pb.LevelRankInfo.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Nt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Nt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RankList && e.hasOwnProperty("RankList") && (t = $root.pb.LevelRankInfo.verify(e.RankList)) ? "RankList." + t : null != e.TermRankList && e.hasOwnProperty("TermRankList") && (t = $root.pb.LevelRankInfo.verify(e.TermRankList)) ? "TermRankList." + t : null;
var t;
}, Nt), r.LevelRankInfo = (Ft.prototype.Items = $util.emptyArray, Ft.prototype.MyRank = 0, 
Ft.prototype.MyLevel = 0, Ft.prototype.MyWave = 0, Ft.create = function(e) {
return new Ft(e);
}, Ft.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.LevelRankItem.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return null != e.MyRank && Object.hasOwnProperty.call(e, "MyRank") && t.uint32(16).int32(e.MyRank), 
null != e.MyLevel && Object.hasOwnProperty.call(e, "MyLevel") && t.uint32(24).int32(e.MyLevel), 
null != e.MyWave && Object.hasOwnProperty.call(e, "MyWave") && t.uint32(32).int32(e.MyWave), 
t;
}, Ft.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ft.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelRankInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.LevelRankItem.decode(e, e.uint32()));
break;

case 2:
n.MyRank = e.int32();
break;

case 3:
n.MyLevel = e.int32();
break;

case 4:
n.MyWave = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ft.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ft.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.LevelRankItem.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null != e.MyRank && e.hasOwnProperty("MyRank") && !$util.isInteger(e.MyRank) ? "MyRank: integer expected" : null != e.MyLevel && e.hasOwnProperty("MyLevel") && !$util.isInteger(e.MyLevel) ? "MyLevel: integer expected" : null != e.MyWave && e.hasOwnProperty("MyWave") && !$util.isInteger(e.MyWave) ? "MyWave: integer expected" : null;
}, Ft), r.LevelRankItem = (Mt.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Mt.prototype.Name = "", Mt.prototype.Head = "", Mt.prototype.Level = 0, Mt.prototype.Wave = 0, 
Mt.prototype.Rank = 0, Mt.prototype.Score = 0, Mt.create = function(e) {
return new Mt(e);
}, Mt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(32).int32(e.Level), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(40).int32(e.Wave), 
null != e.Rank && Object.hasOwnProperty.call(e, "Rank") && t.uint32(48).int32(e.Rank), 
null != e.Score && Object.hasOwnProperty.call(e, "Score") && t.uint32(56).int32(e.Score), 
t;
}, Mt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Mt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelRankItem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

case 3:
n.Head = e.string();
break;

case 4:
n.Level = e.int32();
break;

case 5:
n.Wave = e.int32();
break;

case 6:
n.Rank = e.int32();
break;

case 7:
n.Score = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Mt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Mt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head) ? "Head: string expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave) ? "Wave: integer expected" : null != e.Rank && e.hasOwnProperty("Rank") && !$util.isInteger(e.Rank) ? "Rank: integer expected" : null != e.Score && e.hasOwnProperty("Score") && !$util.isInteger(e.Score) ? "Score: integer expected" : null;
}, Mt), r.FetchChallengeLevelRankArg = (Vt.prototype.Type = 0, Vt.create = function(e) {
return new Vt(e);
}, Vt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
t;
}, Vt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Vt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FetchChallengeLevelRankArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Type = e.int32() : e.skipType(7 & i);
}
return n;
}, Vt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Vt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, Vt), r.Mail = (qt.prototype.Id = "", qt.prototype.Time = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
qt.prototype.IsReward = !1, qt.prototype.IsRead = !1, qt.prototype.Rewards = $util.emptyArray, 
qt.prototype.Type = 0, qt.prototype.Arg = $util.newBuffer([]), qt.create = function(e) {
return new qt(e);
}, qt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
null != e.Time && Object.hasOwnProperty.call(e, "Time") && t.uint32(16).int64(e.Time), 
null != e.IsReward && Object.hasOwnProperty.call(e, "IsReward") && t.uint32(24).bool(e.IsReward), 
null != e.IsRead && Object.hasOwnProperty.call(e, "IsRead") && t.uint32(32).bool(e.IsRead), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(42).string(e.Rewards[r]);
return null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(48).int32(e.Type), 
null != e.Arg && Object.hasOwnProperty.call(e, "Arg") && t.uint32(58).bytes(e.Arg), 
t;
}, qt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, qt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Mail(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.string();
break;

case 2:
n.Time = e.int64();
break;

case 3:
n.IsReward = e.bool();
break;

case 4:
n.IsRead = e.bool();
break;

case 5:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string());
break;

case 6:
n.Type = e.int32();
break;

case 7:
n.Arg = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, qt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, qt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id)) return "Id: string expected";
if (null != e.Time && e.hasOwnProperty("Time") && !($util.isInteger(e.Time) || e.Time && $util.isInteger(e.Time.low) && $util.isInteger(e.Time.high))) return "Time: integer|Long expected";
if (null != e.IsReward && e.hasOwnProperty("IsReward") && "boolean" != typeof e.IsReward) return "IsReward: boolean expected";
if (null != e.IsRead && e.hasOwnProperty("IsRead") && "boolean" != typeof e.IsRead) return "IsRead: boolean expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null != e.Arg && e.hasOwnProperty("Arg") && !(e.Arg && "number" == typeof e.Arg.length || $util.isString(e.Arg)) ? "Arg: buffer expected" : null;
}, qt), r.MailList = (Jt.prototype.Mails = $util.emptyArray, Jt.create = function(e) {
return new Jt(e);
}, Jt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Mails && e.Mails.length) for (var r = 0; r < e.Mails.length; ++r) $root.pb.Mail.encode(e.Mails[r], t.uint32(10).fork()).ldelim();
return t;
}, Jt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Jt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.MailList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Mails && n.Mails.length || (n.Mails = []), n.Mails.push($root.pb.Mail.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Jt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Jt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Mails && e.hasOwnProperty("Mails")) {
if (!Array.isArray(e.Mails)) return "Mails: array expected";
for (var t = 0; t < e.Mails.length; ++t) {
var r = $root.pb.Mail.verify(e.Mails[t]);
if (r) return "Mails." + r;
}
}
return null;
}, Jt), r.TargetMail = (Xt.prototype.Id = "", Xt.create = function(e) {
return new Xt(e);
}, Xt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
t;
}, Xt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Xt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetMail(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.string() : e.skipType(7 & i);
}
return n;
}, Xt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Xt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null;
}, Xt), r.CustomMailArg = (Yt.prototype.Title = "", Yt.prototype.Content = "", Yt.create = function(e) {
return new Yt(e);
}, Yt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Title && Object.hasOwnProperty.call(e, "Title") && t.uint32(10).string(e.Title), 
null != e.Content && Object.hasOwnProperty.call(e, "Content") && t.uint32(18).string(e.Content), 
t;
}, Yt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Yt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.CustomMailArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Title = e.string();
break;

case 2:
n.Content = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Yt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Yt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Title && e.hasOwnProperty("Title") && !$util.isString(e.Title) ? "Title: string expected" : null != e.Content && e.hasOwnProperty("Content") && !$util.isString(e.Content) ? "Content: string expected" : null;
}, Yt), r.TestReward251210MailArg = (zt.prototype.Coin = 0, zt.prototype.Jade = 0, 
zt.create = function(e) {
return new zt(e);
}, zt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Coin && Object.hasOwnProperty.call(e, "Coin") && t.uint32(8).int32(e.Coin), 
null != e.Jade && Object.hasOwnProperty.call(e, "Jade") && t.uint32(16).int32(e.Jade), 
t;
}, zt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, zt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TestReward251210MailArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Coin = e.int32();
break;

case 2:
n.Jade = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, zt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, zt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Coin && e.hasOwnProperty("Coin") && !$util.isInteger(e.Coin) ? "Coin: integer expected" : null != e.Jade && e.hasOwnProperty("Jade") && !$util.isInteger(e.Jade) ? "Jade: integer expected" : null;
}, zt), r.MailRewardResult = (Kt.prototype.MailIds = $util.emptyArray, Kt.prototype.Reward = null, 
Kt.prototype.ErrCode = 0, Kt.create = function(e) {
return new Kt(e);
}, Kt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.MailIds && e.MailIds.length) for (var r = 0; r < e.MailIds.length; ++r) t.uint32(10).string(e.MailIds[r]);
return null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
null != e.ErrCode && Object.hasOwnProperty.call(e, "ErrCode") && t.uint32(24).int32(e.ErrCode), 
t;
}, Kt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Kt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.MailRewardResult(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.MailIds && n.MailIds.length || (n.MailIds = []), n.MailIds.push(e.string());
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 3:
n.ErrCode = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Kt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Kt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.MailIds && e.hasOwnProperty("MailIds")) {
if (!Array.isArray(e.MailIds)) return "MailIds: array expected";
for (var t = 0; t < e.MailIds.length; ++t) if (!$util.isString(e.MailIds[t])) return "MailIds: string[] expected";
}
if (null != e.Reward && e.hasOwnProperty("Reward")) {
var r = $root.pb.RewardList.verify(e.Reward);
if (r) return "Reward." + r;
}
return null != e.ErrCode && e.hasOwnProperty("ErrCode") && !$util.isInteger(e.ErrCode) ? "ErrCode: integer expected" : null;
}, Kt), r.MineInfo = (Qt.prototype.Level = 0, Qt.prototype.Physical = 0, Qt.prototype.RemainBuyCnt = 0, 
Qt.prototype.upgrades = $util.emptyArray, Qt.create = function(e) {
return new Qt(e);
}, Qt.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Physical && Object.hasOwnProperty.call(e, "Physical") && t.uint32(16).int32(e.Physical), 
null != e.RemainBuyCnt && Object.hasOwnProperty.call(e, "RemainBuyCnt") && t.uint32(24).int32(e.RemainBuyCnt), 
null != e.upgrades && e.upgrades.length) {
t.uint32(34).fork();
for (var r = 0; r < e.upgrades.length; ++r) t.int32(e.upgrades[r]);
t.ldelim();
}
return t;
}, Qt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Qt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.MineInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Physical = e.int32();
break;

case 3:
n.RemainBuyCnt = e.int32();
break;

case 4:
if (n.upgrades && n.upgrades.length || (n.upgrades = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.upgrades.push(e.int32()); else n.upgrades.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Qt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Qt.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.Physical && e.hasOwnProperty("Physical") && !$util.isInteger(e.Physical)) return "Physical: integer expected";
if (null != e.RemainBuyCnt && e.hasOwnProperty("RemainBuyCnt") && !$util.isInteger(e.RemainBuyCnt)) return "RemainBuyCnt: integer expected";
if (null != e.upgrades && e.hasOwnProperty("upgrades")) {
if (!Array.isArray(e.upgrades)) return "upgrades: array expected";
for (var t = 0; t < e.upgrades.length; ++t) if (!$util.isInteger(e.upgrades[t])) return "upgrades: integer[] expected";
}
return null;
}, Qt), r.UpLevelMineReply = (Zt.prototype.Mine = null, Zt.prototype.Reward = null, 
Zt.create = function(e) {
return new Zt(e);
}, Zt.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Mine && Object.hasOwnProperty.call(e, "Mine") && $root.pb.MineInfo.encode(e.Mine, t.uint32(10).fork()).ldelim(), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
t;
}, Zt.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Zt.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpLevelMineReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Mine = $root.pb.MineInfo.decode(e, e.uint32());
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Zt.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Zt.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Mine && e.hasOwnProperty("Mine") && (t = $root.pb.MineInfo.verify(e.Mine)) ? "Mine." + t : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null;
var t;
}, Zt), r.MessageID = (e = {}, (t = Object.create(e))[e[0] = "UNKNOW_MessageID"] = 0, 
t[e[100] = "C2S_LOGIN"] = 100, t[e[101] = "C2S_LOGIN2"] = 101, t[e[102] = "C2S_ACCOUNT_LOGIN"] = 102, 
t[e[103] = "C2S_REGISTER_ACCOUNT"] = 103, t[e[104] = "C2S_SDK_ACCOUNT_LOGIN"] = 104, 
t[e[105] = "C2S_TOURIST_REGISTER_ACCOUNT"] = 105, t[e[106] = "C2S_TOURIST_BIND_ACCOUNT"] = 106, 
t[e[107] = "C2S_GET_IP"] = 107, t[e[108] = "C2S_BIND_INVITE_CODE"] = 108, t[e[109] = "C2S_FATIGUE_CERTIFICATION"] = 109, 
t[e[110] = "C2S_PING"] = 110, t[e[111] = "C2S_FETCH_VERSION"] = 111, t[e[112] = "C2S_GM_COMMAND"] = 112, 
t[e[113] = "C2S_BEGIN_LEVEL"] = 113, t[e[114] = "C2S_END_LEVEL"] = 114, t[e[115] = "C2S_FETCH_LEVEL_RANK"] = 115, 
t[e[116] = "C2S_FETCH_MAIL_LIST"] = 116, t[e[117] = "C2S_GET_MAIL_REWARD"] = 117, 
t[e[118] = "C2S_READ_MAIL"] = 118, t[e[119] = "C2S_GET_ALL_MAIL_REWARD"] = 119, 
t[e[120] = "C2S_GET_CLEAR_LEVEL_FIRST_REWARD"] = 120, t[e[121] = "C2S_UPLEVEL_WALL"] = 121, 
t[e[122] = "C2S_ACTIVE_WALL_TALENT"] = 122, t[e[123] = "C2S_WEAR_RUNE"] = 123, t[e[124] = "C2S_DE_RUNE"] = 124, 
t[e[125] = "C2S_UPLEVEL_RUNE"] = 125, t[e[126] = "C2S_SELL_RUNE"] = 126, t[e[127] = "C2S_UPLEVEL_HERO"] = 127, 
t[e[128] = "C2S_UPLEVEL_HERO_TALENT"] = 128, t[e[129] = "C2S_RESET_HERO_TALENT"] = 129, 
t[e[130] = "C2S_FETCH_SHOP_DATA"] = 130, t[e[131] = "C2S_BUY_SHOP_BOX"] = 131, t[e[132] = "C2S_DEL_ALL_READ_MAIL"] = 132, 
t[e[133] = "C2S_WEAR_GEM"] = 133, t[e[134] = "C2S_DE_GEM"] = 134, t[e[135] = "C2S_SELL_GEM"] = 135, 
t[e[136] = "C2S_FETCH_COMPOSE_ALL_GEM_CONSUME"] = 136, t[e[137] = "C2S_COMPOSE_ALL_GEM"] = 137, 
t[e[138] = "C2S_LOCK_GEM"] = 138, t[e[139] = "C2S_SPEED_PASS_LEVEL"] = 139, t[e[140] = "C2S_ENTER_NEXT_CHAPTER"] = 140, 
t[e[141] = "C2S_BUY_SHOP_RES"] = 141, t[e[142] = "C2S_BUY_SHOP_JADE"] = 142, t[e[143] = "C2S_TEST_RECHARGE"] = 143, 
t[e[144] = "C2S_IOS_CREATE_ORDER"] = 144, t[e[145] = "C2S_IOS_RECHARGE"] = 145, 
t[e[146] = "C2S_SDK_CREATE_ORDER"] = 146, t[e[147] = "C2S_KSGAME_SUBSIDY"] = 147, 
t[e[148] = "C2S_UPDATE_NAME"] = 148, t[e[149] = "C2S_UPDATE_HEAD"] = 149, t[e[150] = "C2S_CREATE_PLAYER"] = 150, 
t[e[151] = "C2S_DEL_PLAYER"] = 151, t[e[152] = "C2S_FETCH_PLAYER_INFO"] = 152, t[e[153] = "C2S_FETCH_DEL_ACCOUNT_VERIFY_CODE"] = 153, 
t[e[154] = "C2S_COMPOSE_GEM"] = 154, t[e[155] = "C2S_FETCH_LEVEL_REWARD"] = 155, 
t[e[156] = "C2S_EXCHANGE_GIFT_CODE"] = 156, t[e[157] = "C2S_FETCH_ADVERT_CD"] = 157, 
t[e[158] = "C2S_WATCH_ADVERT"] = 158, t[e[159] = "C2S_BEGIN_ADVERT"] = 159, t[e[160] = "C2S_SELECT_WALL_OPTION"] = 160, 
t[e[161] = "C2S_BUY_VIP"] = 161, t[e[162] = "C2S_GET_WORKSHOP_REWARD"] = 162, t[e[163] = "C2S_UPLEVEL_WORKSHOP"] = 163, 
t[e[164] = "C2S_FETCH_WORKSHOP"] = 164, t[e[165] = "C2S_WORKSHOP_UPLEVEL_SPEED_UP"] = 165, 
t[e[166] = "C2S_BUY_MINE"] = 166, t[e[167] = "C2S_UPLEVEL_MINE"] = 167, t[e[168] = "C2S_FETCH_WORLD_CHAT"] = 168, 
t[e[169] = "C2S_SEND_WORLD_CHAT"] = 169, t[e[170] = "C2S_CLOSE_MAINTALK"] = 170, 
t[e[171] = "C2S_TRANSLATE_TEXT"] = 171, t[e[172] = "C2S_FETCH_SYSTEM_CHAT"] = 172, 
t[e[173] = "C2S_BUY_SHOP_GIFT"] = 173, t[e[174] = "C2S_GOOGLE_PURCHASE"] = 174, 
t[e[175] = "C2S_HERO_WEAR_GEM"] = 175, t[e[176] = "C2S_HERO_DE_GEM"] = 176, t[e[177] = "C2S_SWAP_WEAR_RUNE"] = 177, 
t[e[178] = "C2S_HABBY_BIND"] = 178, t[e[179] = "C2S_RESET_WALL_TALENT"] = 179, t[e[180] = "C2S_HABBY_AUTH"] = 180, 
t[e[181] = "C2S_FETCH_BATTLE_PASS"] = 181, t[e[182] = "C2S_GET_BATTLE_PASS_REWARD"] = 182, 
t[e[183] = "C2S_BUY_BATTLE_PASS"] = 183, t[e[184] = "C2S_RESET_BATTLE_PASS"] = 184, 
t[e[185] = "C2S_FETCH_ANNOUNCEMENT_LIST"] = 185, t[e[186] = "C2S_READ_ANNOUNCEMENT"] = 186, 
t[e[187] = "C2S_BATCH_READ_ANNOUNCEMENT"] = 187, t[e[188] = "C2S_BEGIN_CHALLENGE_LEVEL"] = 188, 
t[e[189] = "C2S_END_CHALLENGE_LEVEL"] = 189, t[e[190] = "C2S_FETCH_CHALLENGE_LEVEL_RANK"] = 190, 
t[e[191] = "C2S_REFRESH_CHALLENGE_EQUIP"] = 191, t[e[192] = "C2S_FETCH_ACTIVITY_LABEL_LIST"] = 192, 
t[e[193] = "C2S_FETCH_ACTIVITY_DETAIL"] = 193, t[e[194] = "C2S_GET_ACTIVITY_REWARD"] = 194, 
t[e[195] = "C2S_CLOSE_ACTIVITY"] = 195, t[e[196] = "C2S_FETCH_FRIEND_LIST"] = 196, 
t[e[197] = "C2S_ADD_FRIEND_APPLY"] = 197, t[e[198] = "C2S_FETCH_FRIEND_APPLY_LIST"] = 198, 
t[e[199] = "C2S_REPLY_FRIEND_APPLY"] = 199, t[e[200] = "C2S_DEL_FRIEND"] = 200, 
t[e[201] = "C2S_SEND_PRIVATE_CHAT"] = 201, t[e[202] = "C2S_READ_PRIVATE_CHAT"] = 202, 
t[e[203] = "C2S_FETCH_UNREAD_PRIVATE_CHAT"] = 203, t[e[204] = "C2S_FETCH_INDENTURE_DATA"] = 204, 
t[e[205] = "C2S_RELIEVE_INDENTURE"] = 205, t[e[206] = "C2S_OPEN_INDENTURE_REWARD"] = 206, 
t[e[207] = "C2S_INDENTURE_REWARD"] = 207, t[e[208] = "C2S_INDENTURE_GET_REWARDS"] = 208, 
t[e[209] = "C2S_SUCCINCT_GEM"] = 209, t[e[210] = "C2S_REPLY_SUCCINCT_GEM"] = 210, 
t[e[211] = "C2S_WALL_WEAR_SKIN"] = 211, t[e[212] = "C2S_BUY_SHOP_PAY_GIFT"] = 212, 
t[e[213] = "C2S_BIND_WECHAT"] = 213, t[e[214] = "C2S_BUY_RAND_SHOP_RES"] = 214, 
t[e[215] = "C2S_REFRESH_RAND_SHOP_RES"] = 215, t[e[216] = "C2S_BUY_LEVEL_GIFT"] = 216, 
t[e[217] = "C2S_BEGIN_LEVEL_GIFT"] = 217, t[e[218] = "C2S_RESET_WALL_ENTRY"] = 218, 
t[e[219] = "C2S_SELECT_BOX_REWARD"] = 219, t[e[220] = "C2S_CONTINUE_LEVEL"] = 220, 
t[e[221] = "C2S_SEND_SMS_CODE"] = 221, t[e[222] = "C2S_SMS_LOGIN"] = 222, t[e[223] = "C2S_GET_DOUYIN_REWARD"] = 223, 
t[e[224] = "C2S_BATTLE_EXECUTE_COMMAND"] = 224, t[e[225] = "C2S_BEGIN_MATCH"] = 225, 
t[e[226] = "C2S_CANCEL_MATCH"] = 226, t[e[227] = "C2S_FETCH_RANK_ACTIVITY"] = 227, 
t[e[228] = "C2S_GET_RANK_ACTIVITY_REWARD"] = 228, t[e[4e3] = "S2S_NEW_WMAIL"] = 4e3, 
t[e[4001] = "S2S_ADD_FRIEND_APPLY"] = 4001, t[e[4002] = "S2S_ADD_FRIEND"] = 4002, 
t[e[4003] = "S2S_DEL_FRIEND"] = 4003, t[e[4004] = "S2S_RECEIVE_PRIVATE_CHAT"] = 4004, 
t[e[5e3] = "S2C_SYNC_RESOURCE"] = 5e3, t[e[5001] = "S2C_KICK_OUT"] = 5001, t[e[5002] = "S2C_UPDATE_VERSION"] = 5002, 
t[e[5004] = "S2C_TELL_ME"] = 5004, t[e[5005] = "S2C_ON_SERVER_STATUS_UPDATE"] = 5005, 
t[e[5006] = "S2C_NOTIFY_FATIGUE_ONLINE_TIME"] = 5006, t[e[5007] = "S2C_UPDATE_PHYSICAL_RECOVERY"] = 5007, 
t[e[5008] = "S2C_UPDATE_HINT"] = 5008, t[e[5009] = "S2C_UPDATE_RUNE"] = 5009, t[e[5010] = "S2C_UPDATE_SHOP_BOX"] = 5010, 
t[e[5011] = "S2C_UPDATE_GEM"] = 5011, t[e[5012] = "S2C_UPDATE_WEAR_GEM"] = 5012, 
t[e[5013] = "S2C_UPDATE_SHOP_RES"] = 5013, t[e[5014] = "S2C_UPDATE_HERO"] = 5014, 
t[e[5015] = "S2C_UPDATE_SHOP_JADE"] = 5015, t[e[5016] = "S2C_NOTIFY_SDK_RECHARGE_RESULT"] = 5016, 
t[e[5017] = "S2C_UPDATE_SHOP_COIN"] = 5017, t[e[5018] = "S2C_UPDATE_SHOW_HEAD"] = 5018, 
t[e[5019] = "S2C_ON_CROSS_DAY"] = 5019, t[e[5020] = "S2C_UPDATE_ADVERT_CD"] = 5020, 
t[e[5021] = "S2C_UPDATE_SHOP_VIP"] = 5021, t[e[5022] = "S2C_ADD_OUT_STATUS"] = 5022, 
t[e[5023] = "S2C_DEL_OUT_STATUS"] = 5023, t[e[5024] = "S2C_UPDATE_WORKSHOP"] = 5024, 
t[e[5025] = "S2C_UPDATE_MINE"] = 5025, t[e[5026] = "S2C_UPDATE_SPEED_PASS"] = 5026, 
t[e[5027] = "S2C_SYNC_WORLD_CHAT"] = 5027, t[e[5028] = "S2C_BROADCAST_RADIO"] = 5028, 
t[e[5029] = "S2C_SYNC_SYSTEM_CHAT"] = 5029, t[e[5030] = "S2C_UPDATE_SHOP_GIFT"] = 5030, 
t[e[5031] = "S2C_UPDATE_WEAR_RUNE"] = 5031, t[e[5032] = "S2C_UPDATE_BATTLE_PASS_ENTRANCE"] = 5032, 
t[e[5033] = "S2C_NOTIFY_MAIL_REWARD_RESULT"] = 5033, t[e[5034] = "S2C_UPDATE_CHALLENGE_EQUIP"] = 5034, 
t[e[5035] = "S2C_UPDATE_ACTIVITY_DETAIL"] = 5035, t[e[5036] = "S2C_UPDATE_ENTRANCE"] = 5036, 
t[e[5037] = "S2C_UPDATE_WALL"] = 5037, t[e[5038] = "S2C_UPDATE_HERO_CONFIG"] = 5038, 
t[e[5039] = "S2C_SYNC_PRIVATE_CHAT"] = 5039, t[e[5040] = "S2C_ADD_FRIEND"] = 5040, 
t[e[5041] = "S2C_BE_DEL_FRIEND"] = 5041, t[e[5042] = "S2C_UPDATE_INDENTURE_DATA"] = 5042, 
t[e[5043] = "S2C_UPDATE_WALL_SKIN"] = 5043, t[e[5044] = "S2C_UPDATE_SHOP_PAY_GIFT"] = 5044, 
t[e[5045] = "S2C_UPDATE_RAND_SHOP_RES"] = 5045, t[e[5046] = "S2C_UPDATE_LEVEL_GIFT"] = 5046, 
t[e[5047] = "S2C_ADD_SELECT_BOX"] = 5047, t[e[5048] = "S2C_UPDATE_ALL_HERO"] = 5048, 
t[e[5049] = "S2C_BEGIN_BATTLE"] = 5049, t[e[5050] = "S2C_BATTLE_ACTION"] = 5050, 
t[e[5051] = "S2C_UPDATE_CHALLENGE"] = 5051, t), r.OutStatus = (er.prototype.StatusId = "", 
er.prototype.RemainTime = 0, er.prototype.EndTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
er.create = function(e) {
return new er(e);
}, er.encode = function(e, t) {
return t = t || $Writer.create(), null != e.StatusId && Object.hasOwnProperty.call(e, "StatusId") && t.uint32(10).string(e.StatusId), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(16).int32(e.RemainTime), 
null != e.EndTime && Object.hasOwnProperty.call(e, "EndTime") && t.uint32(24).int64(e.EndTime), 
t;
}, er.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, er.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.OutStatus(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.StatusId = e.string();
break;

case 2:
n.RemainTime = e.int32();
break;

case 3:
n.EndTime = e.int64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, er.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, er.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.StatusId && e.hasOwnProperty("StatusId") && !$util.isString(e.StatusId) ? "StatusId: string expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null != e.EndTime && e.hasOwnProperty("EndTime") && !($util.isInteger(e.EndTime) || e.EndTime && $util.isInteger(e.EndTime.low) && $util.isInteger(e.EndTime.high)) ? "EndTime: integer|Long expected" : null;
}, er), r.TargetOutStatus = (tr.prototype.StatusId = "", tr.create = function(e) {
return new tr(e);
}, tr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.StatusId && Object.hasOwnProperty.call(e, "StatusId") && t.uint32(10).string(e.StatusId), 
t;
}, tr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, tr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetOutStatus(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.StatusId = e.string() : e.skipType(7 & i);
}
return n;
}, tr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, tr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.StatusId && e.hasOwnProperty("StatusId") && !$util.isString(e.StatusId) ? "StatusId: string expected" : null;
}, tr), r.IosRechargeArg = (rr.prototype.GoodsId = "", rr.prototype.Receipt = "", 
rr.prototype.TransactionId = "", rr.create = function(e) {
return new rr(e);
}, rr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Receipt && Object.hasOwnProperty.call(e, "Receipt") && t.uint32(18).string(e.Receipt), 
null != e.TransactionId && Object.hasOwnProperty.call(e, "TransactionId") && t.uint32(26).string(e.TransactionId), 
t;
}, rr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, rr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IosRechargeArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Receipt = e.string();
break;

case 3:
n.TransactionId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, rr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, rr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.Receipt && e.hasOwnProperty("Receipt") && !$util.isString(e.Receipt) ? "Receipt: string expected" : null != e.TransactionId && e.hasOwnProperty("TransactionId") && !$util.isString(e.TransactionId) ? "TransactionId: string expected" : null;
}, rr), r.IosRechargeReply = (nr.prototype.Result = null, nr.prototype.Sandbox = !1, 
nr.create = function(e) {
return new nr(e);
}, nr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Result && Object.hasOwnProperty.call(e, "Result") && $root.pb.RechargeResult.encode(e.Result, t.uint32(10).fork()).ldelim(), 
null != e.Sandbox && Object.hasOwnProperty.call(e, "Sandbox") && t.uint32(16).bool(e.Sandbox), 
t;
}, nr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, nr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.IosRechargeReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Result = $root.pb.RechargeResult.decode(e, e.uint32());
break;

case 2:
n.Sandbox = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, nr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, nr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Result && e.hasOwnProperty("Result")) {
var t = $root.pb.RechargeResult.verify(e.Result);
if (t) return "Result." + t;
}
return null != e.Sandbox && e.hasOwnProperty("Sandbox") && "boolean" != typeof e.Sandbox ? "Sandbox: boolean expected" : null;
}, nr), r.RechargeResult = (ir.prototype.Ok = !1, ir.prototype.Reward = null, ir.prototype.GoodsId = "", 
ir.prototype.CpOrderId = "", ir.prototype.ChannelOrderId = "", ir.prototype.Price = 0, 
ir.prototype.Coin = 0, ir.prototype.Params = "", ir.create = function(e) {
return new ir(e);
}, ir.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Ok && Object.hasOwnProperty.call(e, "Ok") && t.uint32(8).bool(e.Ok), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(26).string(e.GoodsId), 
null != e.CpOrderId && Object.hasOwnProperty.call(e, "CpOrderId") && t.uint32(34).string(e.CpOrderId), 
null != e.ChannelOrderId && Object.hasOwnProperty.call(e, "ChannelOrderId") && t.uint32(42).string(e.ChannelOrderId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(48).int32(e.Price), 
null != e.Coin && Object.hasOwnProperty.call(e, "Coin") && t.uint32(56).int32(e.Coin), 
null != e.Params && Object.hasOwnProperty.call(e, "Params") && t.uint32(66).string(e.Params), 
t;
}, ir.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ir.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RechargeResult(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Ok = e.bool();
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 3:
n.GoodsId = e.string();
break;

case 4:
n.CpOrderId = e.string();
break;

case 5:
n.ChannelOrderId = e.string();
break;

case 6:
n.Price = e.int32();
break;

case 7:
n.Coin = e.int32();
break;

case 8:
n.Params = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ir.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ir.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Ok && e.hasOwnProperty("Ok") && "boolean" != typeof e.Ok) return "Ok: boolean expected";
if (null != e.Reward && e.hasOwnProperty("Reward")) {
var t = $root.pb.RewardList.verify(e.Reward);
if (t) return "Reward." + t;
}
return null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.CpOrderId && e.hasOwnProperty("CpOrderId") && !$util.isString(e.CpOrderId) ? "CpOrderId: string expected" : null != e.ChannelOrderId && e.hasOwnProperty("ChannelOrderId") && !$util.isString(e.ChannelOrderId) ? "ChannelOrderId: string expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isInteger(e.Price) ? "Price: integer expected" : null != e.Coin && e.hasOwnProperty("Coin") && !$util.isInteger(e.Coin) ? "Coin: integer expected" : null != e.Params && e.hasOwnProperty("Params") && !$util.isString(e.Params) ? "Params: string expected" : null;
}, ir), r.TargetPayGoods = (or.prototype.GoodsId = "", or.create = function(e) {
return new or(e);
}, or.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
t;
}, or.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, or.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetPayGoods(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.GoodsId = e.string() : e.skipType(7 & i);
}
return n;
}, or.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, or.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null;
}, or), r.SdkCreateOrderReply = (ar.prototype.CpOrderId = "", ar.prototype.ChannelOrderId = "", 
ar.prototype.CanWxgameIosUseMidas = !1, ar.create = function(e) {
return new ar(e);
}, ar.encode = function(e, t) {
return t = t || $Writer.create(), null != e.CpOrderId && Object.hasOwnProperty.call(e, "CpOrderId") && t.uint32(10).string(e.CpOrderId), 
null != e.ChannelOrderId && Object.hasOwnProperty.call(e, "ChannelOrderId") && t.uint32(18).string(e.ChannelOrderId), 
null != e.CanWxgameIosUseMidas && Object.hasOwnProperty.call(e, "CanWxgameIosUseMidas") && t.uint32(24).bool(e.CanWxgameIosUseMidas), 
t;
}, ar.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ar.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SdkCreateOrderReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.CpOrderId = e.string();
break;

case 2:
n.ChannelOrderId = e.string();
break;

case 3:
n.CanWxgameIosUseMidas = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ar.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ar.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.CpOrderId && e.hasOwnProperty("CpOrderId") && !$util.isString(e.CpOrderId) ? "CpOrderId: string expected" : null != e.ChannelOrderId && e.hasOwnProperty("ChannelOrderId") && !$util.isString(e.ChannelOrderId) ? "ChannelOrderId: string expected" : null != e.CanWxgameIosUseMidas && e.hasOwnProperty("CanWxgameIosUseMidas") && "boolean" != typeof e.CanWxgameIosUseMidas ? "CanWxgameIosUseMidas: boolean expected" : null;
}, ar), r.SdkCreateOrderArg = (lr.prototype.GoodsId = "", lr.prototype.XsbRiskControlInfo = "", 
lr.prototype.PayChannel = "", lr.prototype.IsWxgameIosUseMidas = !1, lr.create = function(e) {
return new lr(e);
}, lr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.XsbRiskControlInfo && Object.hasOwnProperty.call(e, "XsbRiskControlInfo") && t.uint32(18).string(e.XsbRiskControlInfo), 
null != e.PayChannel && Object.hasOwnProperty.call(e, "PayChannel") && t.uint32(26).string(e.PayChannel), 
null != e.IsWxgameIosUseMidas && Object.hasOwnProperty.call(e, "IsWxgameIosUseMidas") && t.uint32(32).bool(e.IsWxgameIosUseMidas), 
t;
}, lr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, lr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SdkCreateOrderArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.XsbRiskControlInfo = e.string();
break;

case 3:
n.PayChannel = e.string();
break;

case 4:
n.IsWxgameIosUseMidas = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, lr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, lr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.XsbRiskControlInfo && e.hasOwnProperty("XsbRiskControlInfo") && !$util.isString(e.XsbRiskControlInfo) ? "XsbRiskControlInfo: string expected" : null != e.PayChannel && e.hasOwnProperty("PayChannel") && !$util.isString(e.PayChannel) ? "PayChannel: string expected" : null != e.IsWxgameIosUseMidas && e.hasOwnProperty("IsWxgameIosUseMidas") && "boolean" != typeof e.IsWxgameIosUseMidas ? "IsWxgameIosUseMidas: boolean expected" : null;
}, lr), r.KsgameSubsidyReply = (sr.prototype.Subsidy = "", sr.create = function(e) {
return new sr(e);
}, sr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Subsidy && Object.hasOwnProperty.call(e, "Subsidy") && t.uint32(10).string(e.Subsidy), 
t;
}, sr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, sr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.KsgameSubsidyReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Subsidy = e.string() : e.skipType(7 & i);
}
return n;
}, sr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, sr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Subsidy && e.hasOwnProperty("Subsidy") && !$util.isString(e.Subsidy) ? "Subsidy: string expected" : null;
}, sr), r.GooglePurchaseArg = (ur.prototype.GoodsId = "", ur.prototype.PurchaseToken = "", 
ur.create = function(e) {
return new ur(e);
}, ur.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.PurchaseToken && Object.hasOwnProperty.call(e, "PurchaseToken") && t.uint32(18).string(e.PurchaseToken), 
t;
}, ur.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ur.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GooglePurchaseArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.PurchaseToken = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ur.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ur.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.PurchaseToken && e.hasOwnProperty("PurchaseToken") && !$util.isString(e.PurchaseToken) ? "PurchaseToken: string expected" : null;
}, ur), r.FatigueCertificationArg = (cr.prototype.Account = null, cr.prototype.IdentityCardNumber = "", 
cr.prototype.Name = "", cr.create = function(e) {
return new cr(e);
}, cr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && $root.pb.TargetAccount.encode(e.Account, t.uint32(10).fork()).ldelim(), 
null != e.IdentityCardNumber && Object.hasOwnProperty.call(e, "IdentityCardNumber") && t.uint32(18).string(e.IdentityCardNumber), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(26).string(e.Name), 
t;
}, cr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, cr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FatigueCertificationArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Account = $root.pb.TargetAccount.decode(e, e.uint32());
break;

case 2:
n.IdentityCardNumber = e.string();
break;

case 3:
n.Name = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, cr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, cr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Account && e.hasOwnProperty("Account")) {
var t = $root.pb.TargetAccount.verify(e.Account);
if (t) return "Account." + t;
}
return null != e.IdentityCardNumber && e.hasOwnProperty("IdentityCardNumber") && !$util.isString(e.IdentityCardNumber) ? "IdentityCardNumber: string expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null;
}, cr), r.FatigueCertificationReply = (dr.prototype.Account = null, dr.prototype.Errcode = 0, 
dr.create = function(e) {
return new dr(e);
}, dr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && $root.pb.AccountAuthReply.encode(e.Account, t.uint32(10).fork()).ldelim(), 
null != e.Errcode && Object.hasOwnProperty.call(e, "Errcode") && t.uint32(16).int32(e.Errcode), 
t;
}, dr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, dr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FatigueCertificationReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Account = $root.pb.AccountAuthReply.decode(e, e.uint32());
break;

case 2:
n.Errcode = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, dr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, dr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Account && e.hasOwnProperty("Account")) {
var t = $root.pb.AccountAuthReply.verify(e.Account);
if (t) return "Account." + t;
}
if (null != e.Errcode && e.hasOwnProperty("Errcode")) switch (e.Errcode) {
default:
return "Errcode: enum value expected";

case 0:
case 3e4:
case 30001:
case 30002:
case 30003:
case 30004:
case 30005:
case 30006:
case 30007:
}
return null;
}, dr), r.FatigueErrcode = (e = {}, (t = Object.create(e))[e[0] = "FeUnknow"] = 0, 
t[e[3e4] = "FeNotRealName"] = 3e4, t[e[30001] = "FeUnder8Y"] = 30001, t[e[30002] = "FeOverOnlineTime"] = 30002, 
t[e[30003] = "FeNotLoginTimePeriod"] = 30003, t[e[30004] = "FePayOver50"] = 30004, 
t[e[30005] = "FePayOver100"] = 30005, t[e[30006] = "FeMonthPayOver200"] = 30006, 
t[e[30007] = "FeMonthPayOver400"] = 30007, t), r.FatigueGroupEnum = (e = {}, (t = Object.create(e))[e[0] = "FgNotRealName"] = 0, 
t[e[1] = "FgOver18Y"] = 1, t[e[2] = "FgOver16Under18Y"] = 2, t[e[3] = "FgOver8Under16Y"] = 3, 
t[e[4] = "FgUnder8Y"] = 4, t), r.KickOut = (pr.prototype.Reason = 0, pr.create = function(e) {
return new pr(e);
}, pr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reason && Object.hasOwnProperty.call(e, "Reason") && t.uint32(8).int32(e.Reason), 
t;
}, pr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, pr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.KickOut(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Reason = e.int32() : e.skipType(7 & i);
}
return n;
}, pr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, pr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Reason && e.hasOwnProperty("Reason")) switch (e.Reason) {
default:
return "Reason: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
case 5:
}
return null;
}, pr.ReasonEnum = (e = {}, (t = Object.create(e))[e[0] = "Unknow"] = 0, t[e[1] = "Relogin"] = 1, 
t[e[2] = "Forbid"] = 2, t[e[3] = "OverOnlineTime"] = 3, t[e[4] = "NotLoginTimePeriod"] = 4, 
t[e[5] = "ForbidAd"] = 5, t), pr), r.AccountTypeEnum = (e = {}, (t = Object.create(e))[e[0] = "UnknowAccountType"] = 0, 
t[e[1] = "Ios"] = 1, t[e[2] = "Android"] = 2, t[e[3] = "Wxgame"] = 3, t[e[4] = "WxgameIos"] = 4, 
t[e[5] = "Qqgame"] = 5, t[e[6] = "QqgameIos"] = 6, t[e[7] = "Opgame"] = 7, t[e[8] = "Dygame"] = 8, 
t[e[9] = "PC"] = 9, t[e[10] = "Mac"] = 10, t[e[11] = "DygameIos"] = 11, t), r.LoginArg = (fr.prototype.Account = null, 
fr.prototype.AccountType = 0, fr.prototype.SdkName = "", fr.prototype.SdkHead = "", 
fr.prototype.AuthToken = "", fr.prototype.Device = "", fr.prototype.Device2 = null, 
fr.prototype.ShareUid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, fr.prototype.Lan = "", 
fr.prototype.Version = "", fr.prototype.WxLaunchData = "", fr.prototype.Params = "", 
fr.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, fr.prototype.XlsVersions = $util.emptyObject, 
fr.prototype.WxgameFrom = 0, fr.prototype.HuohuanChannel = null, fr.create = function(e) {
return new fr(e);
}, fr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && $root.pb.TargetAccount.encode(e.Account, t.uint32(10).fork()).ldelim(), 
null != e.AccountType && Object.hasOwnProperty.call(e, "AccountType") && t.uint32(16).int32(e.AccountType), 
null != e.SdkName && Object.hasOwnProperty.call(e, "SdkName") && t.uint32(26).string(e.SdkName), 
null != e.SdkHead && Object.hasOwnProperty.call(e, "SdkHead") && t.uint32(34).string(e.SdkHead), 
null != e.AuthToken && Object.hasOwnProperty.call(e, "AuthToken") && t.uint32(42).string(e.AuthToken), 
null != e.Device && Object.hasOwnProperty.call(e, "Device") && t.uint32(50).string(e.Device), 
null != e.Device2 && Object.hasOwnProperty.call(e, "Device2") && $root.pb.DeviceInfo.encode(e.Device2, t.uint32(58).fork()).ldelim(), 
null != e.ShareUid && Object.hasOwnProperty.call(e, "ShareUid") && t.uint32(64).uint64(e.ShareUid), 
null != e.Lan && Object.hasOwnProperty.call(e, "Lan") && t.uint32(74).string(e.Lan), 
null != e.Version && Object.hasOwnProperty.call(e, "Version") && t.uint32(82).string(e.Version), 
null != e.WxLaunchData && Object.hasOwnProperty.call(e, "WxLaunchData") && t.uint32(90).string(e.WxLaunchData), 
null != e.Params && Object.hasOwnProperty.call(e, "Params") && t.uint32(98).string(e.Params), 
null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(104).uint64(e.Uid), 
null != e.XlsVersions && Object.hasOwnProperty.call(e, "XlsVersions")) for (var r = Object.keys(e.XlsVersions), n = 0; n < r.length; ++n) t.uint32(114).fork().uint32(10).string(r[n]).uint32(18).string(e.XlsVersions[r[n]]).ldelim();
return null != e.WxgameFrom && Object.hasOwnProperty.call(e, "WxgameFrom") && t.uint32(120).int32(e.WxgameFrom), 
null != e.HuohuanChannel && Object.hasOwnProperty.call(e, "HuohuanChannel") && $root.pb.HuoHuanChannel.encode(e.HuohuanChannel, t.uint32(130).fork()).ldelim(), 
t;
}, fr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, fr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LoginArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Account = $root.pb.TargetAccount.decode(e, e.uint32());
break;

case 2:
n.AccountType = e.int32();
break;

case 3:
n.SdkName = e.string();
break;

case 4:
n.SdkHead = e.string();
break;

case 5:
n.AuthToken = e.string();
break;

case 6:
n.Device = e.string();
break;

case 7:
n.Device2 = $root.pb.DeviceInfo.decode(e, e.uint32());
break;

case 8:
n.ShareUid = e.uint64();
break;

case 9:
n.Lan = e.string();
break;

case 10:
n.Version = e.string();
break;

case 11:
n.WxLaunchData = e.string();
break;

case 12:
n.Params = e.string();
break;

case 13:
n.Uid = e.uint64();
break;

case 14:
n.XlsVersions === $util.emptyObject && (n.XlsVersions = {});
for (var o = e.uint32() + e.pos, a = "", l = ""; e.pos < o; ) {
var s = e.uint32();
switch (s >>> 3) {
case 1:
a = e.string();
break;

case 2:
l = e.string();
break;

default:
e.skipType(7 & s);
}
}
n.XlsVersions[a] = l;
break;

case 15:
n.WxgameFrom = e.int32();
break;

case 16:
n.HuohuanChannel = $root.pb.HuoHuanChannel.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, fr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, fr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
var t;
if (null != e.Account && e.hasOwnProperty("Account") && (t = $root.pb.TargetAccount.verify(e.Account))) return "Account." + t;
if (null != e.AccountType && e.hasOwnProperty("AccountType")) switch (e.AccountType) {
default:
return "AccountType: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
case 5:
case 6:
case 7:
case 8:
case 9:
case 10:
case 11:
}
if (null != e.SdkName && e.hasOwnProperty("SdkName") && !$util.isString(e.SdkName)) return "SdkName: string expected";
if (null != e.SdkHead && e.hasOwnProperty("SdkHead") && !$util.isString(e.SdkHead)) return "SdkHead: string expected";
if (null != e.AuthToken && e.hasOwnProperty("AuthToken") && !$util.isString(e.AuthToken)) return "AuthToken: string expected";
if (null != e.Device && e.hasOwnProperty("Device") && !$util.isString(e.Device)) return "Device: string expected";
if (null != e.Device2 && e.hasOwnProperty("Device2") && (t = $root.pb.DeviceInfo.verify(e.Device2))) return "Device2." + t;
if (null != e.ShareUid && e.hasOwnProperty("ShareUid") && !($util.isInteger(e.ShareUid) || e.ShareUid && $util.isInteger(e.ShareUid.low) && $util.isInteger(e.ShareUid.high))) return "ShareUid: integer|Long expected";
if (null != e.Lan && e.hasOwnProperty("Lan") && !$util.isString(e.Lan)) return "Lan: string expected";
if (null != e.Version && e.hasOwnProperty("Version") && !$util.isString(e.Version)) return "Version: string expected";
if (null != e.WxLaunchData && e.hasOwnProperty("WxLaunchData") && !$util.isString(e.WxLaunchData)) return "WxLaunchData: string expected";
if (null != e.Params && e.hasOwnProperty("Params") && !$util.isString(e.Params)) return "Params: string expected";
if (null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high))) return "Uid: integer|Long expected";
if (null != e.XlsVersions && e.hasOwnProperty("XlsVersions")) {
if (!$util.isObject(e.XlsVersions)) return "XlsVersions: object expected";
for (var r = Object.keys(e.XlsVersions), n = 0; n < r.length; ++n) if (!$util.isString(e.XlsVersions[r[n]])) return "XlsVersions: string{k:string} expected";
}
return null != e.WxgameFrom && e.hasOwnProperty("WxgameFrom") && !$util.isInteger(e.WxgameFrom) ? "WxgameFrom: integer expected" : null != e.HuohuanChannel && e.hasOwnProperty("HuohuanChannel") && (t = $root.pb.HuoHuanChannel.verify(e.HuohuanChannel)) ? "HuohuanChannel." + t : null;
}, fr), r.LoginReply = (yr.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
yr.prototype.Res = $util.emptyArray, yr.prototype.Name = "", yr.prototype.Head = "", 
yr.prototype.Ver = null, yr.prototype.IsAdmin = !1, yr.prototype.CreateTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
yr.prototype.FatigueGroup = 0, yr.prototype.IsNew = !1, yr.prototype.IP = "", yr.prototype.IsForbid = !1, 
yr.prototype.FuncConfig = null, yr.prototype.XlsDatas = $util.emptyArray, yr.prototype.HeadFrame = "", 
yr.prototype.PhysicalRecovery = 0, yr.prototype.UnlockLevel = 0, yr.prototype.SdkHead = "", 
yr.prototype.Hints = $util.emptyArray, yr.prototype.Levels = $util.emptyArray, yr.prototype.Wall = null, 
yr.prototype.Rune = null, yr.prototype.Heros = $util.emptyArray, yr.prototype.Shop = null, 
yr.prototype.Gem = null, yr.prototype.LastChapter = null, yr.prototype.levelChapter = 0, 
yr.prototype.CanShowHeads = $util.emptyArray, yr.prototype.LoginDays = null, yr.prototype.DelPlayerCooldown = 0, 
yr.prototype.AdvertCooldowns = $util.emptyArray, yr.prototype.Statuses = $util.emptyArray, 
yr.prototype.WorkShops = null, yr.prototype.Mine = null, yr.prototype.SpeedPassCnt = 0, 
yr.prototype.XlsVersions = $util.emptyObject, yr.prototype.AccountCreateTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
yr.prototype.OpenBPIds = $util.emptyArray, yr.prototype.ChallengeLevels = $util.emptyArray, 
yr.prototype.ActEntrances = $util.emptyArray, yr.prototype.HeroConfigs = $util.emptyArray, 
yr.prototype.Server = 0, yr.prototype.IsBoundWechat = !1, yr.prototype.Box = null, 
yr.prototype.RankActivity = null, yr.create = function(e) {
return new yr(e);
}, yr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Res && e.Res.length) for (var r = 0; r < e.Res.length; ++r) $root.pb.Resource.encode(e.Res[r], t.uint32(18).fork()).ldelim();
if (null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(26).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(34).string(e.Head), 
null != e.Ver && Object.hasOwnProperty.call(e, "Ver") && $root.pb.Version.encode(e.Ver, t.uint32(42).fork()).ldelim(), 
null != e.IsAdmin && Object.hasOwnProperty.call(e, "IsAdmin") && t.uint32(48).bool(e.IsAdmin), 
null != e.CreateTime && Object.hasOwnProperty.call(e, "CreateTime") && t.uint32(56).int64(e.CreateTime), 
null != e.FatigueGroup && Object.hasOwnProperty.call(e, "FatigueGroup") && t.uint32(64).int32(e.FatigueGroup), 
null != e.IsNew && Object.hasOwnProperty.call(e, "IsNew") && t.uint32(72).bool(e.IsNew), 
null != e.IP && Object.hasOwnProperty.call(e, "IP") && t.uint32(82).string(e.IP), 
null != e.IsForbid && Object.hasOwnProperty.call(e, "IsForbid") && t.uint32(88).bool(e.IsForbid), 
null != e.FuncConfig && Object.hasOwnProperty.call(e, "FuncConfig") && $root.pb.FunctionConfig.encode(e.FuncConfig, t.uint32(98).fork()).ldelim(), 
null != e.XlsDatas && e.XlsDatas.length) for (r = 0; r < e.XlsDatas.length; ++r) $root.pb.XlsData.encode(e.XlsDatas[r], t.uint32(106).fork()).ldelim();
if (null != e.HeadFrame && Object.hasOwnProperty.call(e, "HeadFrame") && t.uint32(114).string(e.HeadFrame), 
null != e.PhysicalRecovery && Object.hasOwnProperty.call(e, "PhysicalRecovery") && t.uint32(120).int32(e.PhysicalRecovery), 
null != e.UnlockLevel && Object.hasOwnProperty.call(e, "UnlockLevel") && t.uint32(128).int32(e.UnlockLevel), 
null != e.SdkHead && Object.hasOwnProperty.call(e, "SdkHead") && t.uint32(138).string(e.SdkHead), 
null != e.Hints && e.Hints.length) for (r = 0; r < e.Hints.length; ++r) $root.pb.Hint.encode(e.Hints[r], t.uint32(146).fork()).ldelim();
if (null != e.Levels && e.Levels.length) for (r = 0; r < e.Levels.length; ++r) $root.pb.LevelInfo.encode(e.Levels[r], t.uint32(154).fork()).ldelim();
if (null != e.Wall && Object.hasOwnProperty.call(e, "Wall") && $root.pb.WallInfo.encode(e.Wall, t.uint32(162).fork()).ldelim(), 
null != e.Rune && Object.hasOwnProperty.call(e, "Rune") && $root.pb.RuneInfo.encode(e.Rune, t.uint32(170).fork()).ldelim(), 
null != e.Heros && e.Heros.length) for (r = 0; r < e.Heros.length; ++r) $root.pb.HeroInfo.encode(e.Heros[r], t.uint32(178).fork()).ldelim();
if (null != e.Shop && Object.hasOwnProperty.call(e, "Shop") && $root.pb.ShopData.encode(e.Shop, t.uint32(186).fork()).ldelim(), 
null != e.Gem && Object.hasOwnProperty.call(e, "Gem") && $root.pb.GemInfo.encode(e.Gem, t.uint32(194).fork()).ldelim(), 
null != e.LastChapter && Object.hasOwnProperty.call(e, "LastChapter") && $root.pb.Chapter.encode(e.LastChapter, t.uint32(202).fork()).ldelim(), 
null != e.levelChapter && Object.hasOwnProperty.call(e, "levelChapter") && t.uint32(208).int32(e.levelChapter), 
null != e.CanShowHeads && e.CanShowHeads.length) for (r = 0; r < e.CanShowHeads.length; ++r) t.uint32(218).string(e.CanShowHeads[r]);
if (null != e.LoginDays && Object.hasOwnProperty.call(e, "LoginDays") && $root.pb.PlayerLoginDay.encode(e.LoginDays, t.uint32(226).fork()).ldelim(), 
null != e.DelPlayerCooldown && Object.hasOwnProperty.call(e, "DelPlayerCooldown") && t.uint32(232).int32(e.DelPlayerCooldown), 
null != e.AdvertCooldowns && e.AdvertCooldowns.length) for (r = 0; r < e.AdvertCooldowns.length; ++r) $root.pb.AdvertCooldown.encode(e.AdvertCooldowns[r], t.uint32(242).fork()).ldelim();
if (null != e.Statuses && e.Statuses.length) for (r = 0; r < e.Statuses.length; ++r) $root.pb.OutStatus.encode(e.Statuses[r], t.uint32(250).fork()).ldelim();
if (null != e.WorkShops && Object.hasOwnProperty.call(e, "WorkShops") && $root.pb.WorkShopInfo.encode(e.WorkShops, t.uint32(258).fork()).ldelim(), 
null != e.Mine && Object.hasOwnProperty.call(e, "Mine") && $root.pb.MineInfo.encode(e.Mine, t.uint32(266).fork()).ldelim(), 
null != e.SpeedPassCnt && Object.hasOwnProperty.call(e, "SpeedPassCnt") && t.uint32(272).int32(e.SpeedPassCnt), 
null != e.XlsVersions && Object.hasOwnProperty.call(e, "XlsVersions")) {
var n = Object.keys(e.XlsVersions);
for (r = 0; r < n.length; ++r) t.uint32(282).fork().uint32(10).string(n[r]).uint32(18).string(e.XlsVersions[n[r]]).ldelim();
}
if (null != e.AccountCreateTime && Object.hasOwnProperty.call(e, "AccountCreateTime") && t.uint32(288).int64(e.AccountCreateTime), 
null != e.OpenBPIds && e.OpenBPIds.length) {
t.uint32(298).fork();
for (r = 0; r < e.OpenBPIds.length; ++r) t.int32(e.OpenBPIds[r]);
t.ldelim();
}
if (null != e.ChallengeLevels && e.ChallengeLevels.length) for (r = 0; r < e.ChallengeLevels.length; ++r) $root.pb.ChallengeLevelInfo.encode(e.ChallengeLevels[r], t.uint32(306).fork()).ldelim();
if (null != e.ActEntrances && e.ActEntrances.length) for (r = 0; r < e.ActEntrances.length; ++r) $root.pb.ActEntrance.encode(e.ActEntrances[r], t.uint32(314).fork()).ldelim();
if (null != e.HeroConfigs && e.HeroConfigs.length) for (r = 0; r < e.HeroConfigs.length; ++r) $root.pb.HeroConfig.encode(e.HeroConfigs[r], t.uint32(322).fork()).ldelim();
return null != e.Server && Object.hasOwnProperty.call(e, "Server") && t.uint32(328).int32(e.Server), 
null != e.IsBoundWechat && Object.hasOwnProperty.call(e, "IsBoundWechat") && t.uint32(336).bool(e.IsBoundWechat), 
null != e.Box && Object.hasOwnProperty.call(e, "Box") && $root.pb.BoxInfo.encode(e.Box, t.uint32(346).fork()).ldelim(), 
null != e.RankActivity && Object.hasOwnProperty.call(e, "RankActivity") && $root.pb.RankActivityData.encode(e.RankActivity, t.uint32(354).fork()).ldelim(), 
t;
}, yr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, yr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LoginReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Res && n.Res.length || (n.Res = []), n.Res.push($root.pb.Resource.decode(e, e.uint32()));
break;

case 3:
n.Name = e.string();
break;

case 4:
n.Head = e.string();
break;

case 5:
n.Ver = $root.pb.Version.decode(e, e.uint32());
break;

case 6:
n.IsAdmin = e.bool();
break;

case 7:
n.CreateTime = e.int64();
break;

case 8:
n.FatigueGroup = e.int32();
break;

case 9:
n.IsNew = e.bool();
break;

case 10:
n.IP = e.string();
break;

case 11:
n.IsForbid = e.bool();
break;

case 12:
n.FuncConfig = $root.pb.FunctionConfig.decode(e, e.uint32());
break;

case 13:
n.XlsDatas && n.XlsDatas.length || (n.XlsDatas = []), n.XlsDatas.push($root.pb.XlsData.decode(e, e.uint32()));
break;

case 14:
n.HeadFrame = e.string();
break;

case 15:
n.PhysicalRecovery = e.int32();
break;

case 16:
n.UnlockLevel = e.int32();
break;

case 17:
n.SdkHead = e.string();
break;

case 18:
n.Hints && n.Hints.length || (n.Hints = []), n.Hints.push($root.pb.Hint.decode(e, e.uint32()));
break;

case 19:
n.Levels && n.Levels.length || (n.Levels = []), n.Levels.push($root.pb.LevelInfo.decode(e, e.uint32()));
break;

case 20:
n.Wall = $root.pb.WallInfo.decode(e, e.uint32());
break;

case 21:
n.Rune = $root.pb.RuneInfo.decode(e, e.uint32());
break;

case 22:
n.Heros && n.Heros.length || (n.Heros = []), n.Heros.push($root.pb.HeroInfo.decode(e, e.uint32()));
break;

case 23:
n.Shop = $root.pb.ShopData.decode(e, e.uint32());
break;

case 24:
n.Gem = $root.pb.GemInfo.decode(e, e.uint32());
break;

case 25:
n.LastChapter = $root.pb.Chapter.decode(e, e.uint32());
break;

case 26:
n.levelChapter = e.int32();
break;

case 27:
n.CanShowHeads && n.CanShowHeads.length || (n.CanShowHeads = []), n.CanShowHeads.push(e.string());
break;

case 28:
n.LoginDays = $root.pb.PlayerLoginDay.decode(e, e.uint32());
break;

case 29:
n.DelPlayerCooldown = e.int32();
break;

case 30:
n.AdvertCooldowns && n.AdvertCooldowns.length || (n.AdvertCooldowns = []), n.AdvertCooldowns.push($root.pb.AdvertCooldown.decode(e, e.uint32()));
break;

case 31:
n.Statuses && n.Statuses.length || (n.Statuses = []), n.Statuses.push($root.pb.OutStatus.decode(e, e.uint32()));
break;

case 32:
n.WorkShops = $root.pb.WorkShopInfo.decode(e, e.uint32());
break;

case 33:
n.Mine = $root.pb.MineInfo.decode(e, e.uint32());
break;

case 34:
n.SpeedPassCnt = e.int32();
break;

case 35:
n.XlsVersions === $util.emptyObject && (n.XlsVersions = {});
for (var o = e.uint32() + e.pos, a = "", l = ""; e.pos < o; ) {
var s = e.uint32();
switch (s >>> 3) {
case 1:
a = e.string();
break;

case 2:
l = e.string();
break;

default:
e.skipType(7 & s);
}
}
n.XlsVersions[a] = l;
break;

case 36:
n.AccountCreateTime = e.int64();
break;

case 37:
if (n.OpenBPIds && n.OpenBPIds.length || (n.OpenBPIds = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.OpenBPIds.push(e.int32()); else n.OpenBPIds.push(e.int32());
break;

case 38:
n.ChallengeLevels && n.ChallengeLevels.length || (n.ChallengeLevels = []), n.ChallengeLevels.push($root.pb.ChallengeLevelInfo.decode(e, e.uint32()));
break;

case 39:
n.ActEntrances && n.ActEntrances.length || (n.ActEntrances = []), n.ActEntrances.push($root.pb.ActEntrance.decode(e, e.uint32()));
break;

case 40:
n.HeroConfigs && n.HeroConfigs.length || (n.HeroConfigs = []), n.HeroConfigs.push($root.pb.HeroConfig.decode(e, e.uint32()));
break;

case 41:
n.Server = e.int32();
break;

case 42:
n.IsBoundWechat = e.bool();
break;

case 43:
n.Box = $root.pb.BoxInfo.decode(e, e.uint32());
break;

case 44:
n.RankActivity = $root.pb.RankActivityData.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, yr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, yr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high))) return "Uid: integer|Long expected";
if (null != e.Res && e.hasOwnProperty("Res")) {
if (!Array.isArray(e.Res)) return "Res: array expected";
for (var t = 0; t < e.Res.length; ++t) if (n = $root.pb.Resource.verify(e.Res[t])) return "Res." + n;
}
if (null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name)) return "Name: string expected";
if (null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head)) return "Head: string expected";
if (null != e.Ver && e.hasOwnProperty("Ver") && (n = $root.pb.Version.verify(e.Ver))) return "Ver." + n;
if (null != e.IsAdmin && e.hasOwnProperty("IsAdmin") && "boolean" != typeof e.IsAdmin) return "IsAdmin: boolean expected";
if (null != e.CreateTime && e.hasOwnProperty("CreateTime") && !($util.isInteger(e.CreateTime) || e.CreateTime && $util.isInteger(e.CreateTime.low) && $util.isInteger(e.CreateTime.high))) return "CreateTime: integer|Long expected";
if (null != e.FatigueGroup && e.hasOwnProperty("FatigueGroup")) switch (e.FatigueGroup) {
default:
return "FatigueGroup: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
if (null != e.IsNew && e.hasOwnProperty("IsNew") && "boolean" != typeof e.IsNew) return "IsNew: boolean expected";
if (null != e.IP && e.hasOwnProperty("IP") && !$util.isString(e.IP)) return "IP: string expected";
if (null != e.IsForbid && e.hasOwnProperty("IsForbid") && "boolean" != typeof e.IsForbid) return "IsForbid: boolean expected";
if (null != e.FuncConfig && e.hasOwnProperty("FuncConfig") && (n = $root.pb.FunctionConfig.verify(e.FuncConfig))) return "FuncConfig." + n;
if (null != e.XlsDatas && e.hasOwnProperty("XlsDatas")) {
if (!Array.isArray(e.XlsDatas)) return "XlsDatas: array expected";
for (t = 0; t < e.XlsDatas.length; ++t) if (n = $root.pb.XlsData.verify(e.XlsDatas[t])) return "XlsDatas." + n;
}
if (null != e.HeadFrame && e.hasOwnProperty("HeadFrame") && !$util.isString(e.HeadFrame)) return "HeadFrame: string expected";
if (null != e.PhysicalRecovery && e.hasOwnProperty("PhysicalRecovery") && !$util.isInteger(e.PhysicalRecovery)) return "PhysicalRecovery: integer expected";
if (null != e.UnlockLevel && e.hasOwnProperty("UnlockLevel") && !$util.isInteger(e.UnlockLevel)) return "UnlockLevel: integer expected";
if (null != e.SdkHead && e.hasOwnProperty("SdkHead") && !$util.isString(e.SdkHead)) return "SdkHead: string expected";
if (null != e.Hints && e.hasOwnProperty("Hints")) {
if (!Array.isArray(e.Hints)) return "Hints: array expected";
for (t = 0; t < e.Hints.length; ++t) if (n = $root.pb.Hint.verify(e.Hints[t])) return "Hints." + n;
}
if (null != e.Levels && e.hasOwnProperty("Levels")) {
if (!Array.isArray(e.Levels)) return "Levels: array expected";
for (t = 0; t < e.Levels.length; ++t) if (n = $root.pb.LevelInfo.verify(e.Levels[t])) return "Levels." + n;
}
if (null != e.Wall && e.hasOwnProperty("Wall") && (n = $root.pb.WallInfo.verify(e.Wall))) return "Wall." + n;
if (null != e.Rune && e.hasOwnProperty("Rune") && (n = $root.pb.RuneInfo.verify(e.Rune))) return "Rune." + n;
if (null != e.Heros && e.hasOwnProperty("Heros")) {
if (!Array.isArray(e.Heros)) return "Heros: array expected";
for (t = 0; t < e.Heros.length; ++t) if (n = $root.pb.HeroInfo.verify(e.Heros[t])) return "Heros." + n;
}
if (null != e.Shop && e.hasOwnProperty("Shop") && (n = $root.pb.ShopData.verify(e.Shop))) return "Shop." + n;
if (null != e.Gem && e.hasOwnProperty("Gem") && (n = $root.pb.GemInfo.verify(e.Gem))) return "Gem." + n;
if (null != e.LastChapter && e.hasOwnProperty("LastChapter") && (n = $root.pb.Chapter.verify(e.LastChapter))) return "LastChapter." + n;
if (null != e.levelChapter && e.hasOwnProperty("levelChapter") && !$util.isInteger(e.levelChapter)) return "levelChapter: integer expected";
if (null != e.CanShowHeads && e.hasOwnProperty("CanShowHeads")) {
if (!Array.isArray(e.CanShowHeads)) return "CanShowHeads: array expected";
for (t = 0; t < e.CanShowHeads.length; ++t) if (!$util.isString(e.CanShowHeads[t])) return "CanShowHeads: string[] expected";
}
if (null != e.LoginDays && e.hasOwnProperty("LoginDays") && (n = $root.pb.PlayerLoginDay.verify(e.LoginDays))) return "LoginDays." + n;
if (null != e.DelPlayerCooldown && e.hasOwnProperty("DelPlayerCooldown") && !$util.isInteger(e.DelPlayerCooldown)) return "DelPlayerCooldown: integer expected";
if (null != e.AdvertCooldowns && e.hasOwnProperty("AdvertCooldowns")) {
if (!Array.isArray(e.AdvertCooldowns)) return "AdvertCooldowns: array expected";
for (t = 0; t < e.AdvertCooldowns.length; ++t) if (n = $root.pb.AdvertCooldown.verify(e.AdvertCooldowns[t])) return "AdvertCooldowns." + n;
}
if (null != e.Statuses && e.hasOwnProperty("Statuses")) {
if (!Array.isArray(e.Statuses)) return "Statuses: array expected";
for (t = 0; t < e.Statuses.length; ++t) if (n = $root.pb.OutStatus.verify(e.Statuses[t])) return "Statuses." + n;
}
if (null != e.WorkShops && e.hasOwnProperty("WorkShops") && (n = $root.pb.WorkShopInfo.verify(e.WorkShops))) return "WorkShops." + n;
if (null != e.Mine && e.hasOwnProperty("Mine") && (n = $root.pb.MineInfo.verify(e.Mine))) return "Mine." + n;
if (null != e.SpeedPassCnt && e.hasOwnProperty("SpeedPassCnt") && !$util.isInteger(e.SpeedPassCnt)) return "SpeedPassCnt: integer expected";
if (null != e.XlsVersions && e.hasOwnProperty("XlsVersions")) {
if (!$util.isObject(e.XlsVersions)) return "XlsVersions: object expected";
var r = Object.keys(e.XlsVersions);
for (t = 0; t < r.length; ++t) if (!$util.isString(e.XlsVersions[r[t]])) return "XlsVersions: string{k:string} expected";
}
if (null != e.AccountCreateTime && e.hasOwnProperty("AccountCreateTime") && !($util.isInteger(e.AccountCreateTime) || e.AccountCreateTime && $util.isInteger(e.AccountCreateTime.low) && $util.isInteger(e.AccountCreateTime.high))) return "AccountCreateTime: integer|Long expected";
if (null != e.OpenBPIds && e.hasOwnProperty("OpenBPIds")) {
if (!Array.isArray(e.OpenBPIds)) return "OpenBPIds: array expected";
for (t = 0; t < e.OpenBPIds.length; ++t) if (!$util.isInteger(e.OpenBPIds[t])) return "OpenBPIds: integer[] expected";
}
if (null != e.ChallengeLevels && e.hasOwnProperty("ChallengeLevels")) {
if (!Array.isArray(e.ChallengeLevels)) return "ChallengeLevels: array expected";
for (t = 0; t < e.ChallengeLevels.length; ++t) if (n = $root.pb.ChallengeLevelInfo.verify(e.ChallengeLevels[t])) return "ChallengeLevels." + n;
}
if (null != e.ActEntrances && e.hasOwnProperty("ActEntrances")) {
if (!Array.isArray(e.ActEntrances)) return "ActEntrances: array expected";
for (t = 0; t < e.ActEntrances.length; ++t) if (n = $root.pb.ActEntrance.verify(e.ActEntrances[t])) return "ActEntrances." + n;
}
if (null != e.HeroConfigs && e.hasOwnProperty("HeroConfigs")) {
if (!Array.isArray(e.HeroConfigs)) return "HeroConfigs: array expected";
var n;
for (t = 0; t < e.HeroConfigs.length; ++t) if (n = $root.pb.HeroConfig.verify(e.HeroConfigs[t])) return "HeroConfigs." + n;
}
return null != e.Server && e.hasOwnProperty("Server") && !$util.isInteger(e.Server) ? "Server: integer expected" : null != e.IsBoundWechat && e.hasOwnProperty("IsBoundWechat") && "boolean" != typeof e.IsBoundWechat ? "IsBoundWechat: boolean expected" : null != e.Box && e.hasOwnProperty("Box") && (n = $root.pb.BoxInfo.verify(e.Box)) ? "Box." + n : null != e.RankActivity && e.hasOwnProperty("RankActivity") && (n = $root.pb.RankActivityData.verify(e.RankActivity)) ? "RankActivity." + n : null;
}, yr), r.AccountLoginArg = (hr.prototype.Channel = "", hr.prototype.Account = "", 
hr.prototype.Password = "", hr.prototype.SubChannel = "", hr.prototype.Lan = "", 
hr.create = function(e) {
return new hr(e);
}, hr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
null != e.Account && Object.hasOwnProperty.call(e, "Account") && t.uint32(26).string(e.Account), 
null != e.Password && Object.hasOwnProperty.call(e, "Password") && t.uint32(34).string(e.Password), 
null != e.SubChannel && Object.hasOwnProperty.call(e, "SubChannel") && t.uint32(42).string(e.SubChannel), 
null != e.Lan && Object.hasOwnProperty.call(e, "Lan") && t.uint32(50).string(e.Lan), 
t;
}, hr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, hr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AccountLoginArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Channel = e.string();
break;

case 3:
n.Account = e.string();
break;

case 4:
n.Password = e.string();
break;

case 5:
n.SubChannel = e.string();
break;

case 6:
n.Lan = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, hr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, hr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel) ? "Channel: string expected" : null != e.Account && e.hasOwnProperty("Account") && !$util.isString(e.Account) ? "Account: string expected" : null != e.Password && e.hasOwnProperty("Password") && !$util.isString(e.Password) ? "Password: string expected" : null != e.SubChannel && e.hasOwnProperty("SubChannel") && !$util.isString(e.SubChannel) ? "SubChannel: string expected" : null != e.Lan && e.hasOwnProperty("Lan") && !$util.isString(e.Lan) ? "Lan: string expected" : null;
}, hr), r.SdkAccountLoginArg = (gr.prototype.Channel = "", gr.prototype.LoginChannel = "", 
gr.prototype.ChannelID = "", gr.prototype.SdkToken = "", gr.prototype.TouristAccount = null, 
gr.prototype.SubChannel = "", gr.prototype.Device2 = null, gr.prototype.Lan = "", 
gr.prototype.AccountType = 0, gr.create = function(e) {
return new gr(e);
}, gr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
null != e.LoginChannel && Object.hasOwnProperty.call(e, "LoginChannel") && t.uint32(18).string(e.LoginChannel), 
null != e.ChannelID && Object.hasOwnProperty.call(e, "ChannelID") && t.uint32(26).string(e.ChannelID), 
null != e.SdkToken && Object.hasOwnProperty.call(e, "SdkToken") && t.uint32(34).string(e.SdkToken), 
null != e.TouristAccount && Object.hasOwnProperty.call(e, "TouristAccount") && $root.pb.AccountLoginArg.encode(e.TouristAccount, t.uint32(42).fork()).ldelim(), 
null != e.SubChannel && Object.hasOwnProperty.call(e, "SubChannel") && t.uint32(50).string(e.SubChannel), 
null != e.Device2 && Object.hasOwnProperty.call(e, "Device2") && $root.pb.DeviceInfo.encode(e.Device2, t.uint32(58).fork()).ldelim(), 
null != e.Lan && Object.hasOwnProperty.call(e, "Lan") && t.uint32(66).string(e.Lan), 
null != e.AccountType && Object.hasOwnProperty.call(e, "AccountType") && t.uint32(72).int32(e.AccountType), 
t;
}, gr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, gr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SdkAccountLoginArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Channel = e.string();
break;

case 2:
n.LoginChannel = e.string();
break;

case 3:
n.ChannelID = e.string();
break;

case 4:
n.SdkToken = e.string();
break;

case 5:
n.TouristAccount = $root.pb.AccountLoginArg.decode(e, e.uint32());
break;

case 6:
n.SubChannel = e.string();
break;

case 7:
n.Device2 = $root.pb.DeviceInfo.decode(e, e.uint32());
break;

case 8:
n.Lan = e.string();
break;

case 9:
n.AccountType = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, gr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, gr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel)) return "Channel: string expected";
if (null != e.LoginChannel && e.hasOwnProperty("LoginChannel") && !$util.isString(e.LoginChannel)) return "LoginChannel: string expected";
if (null != e.ChannelID && e.hasOwnProperty("ChannelID") && !$util.isString(e.ChannelID)) return "ChannelID: string expected";
if (null != e.SdkToken && e.hasOwnProperty("SdkToken") && !$util.isString(e.SdkToken)) return "SdkToken: string expected";
var t;
if (null != e.TouristAccount && e.hasOwnProperty("TouristAccount") && (t = $root.pb.AccountLoginArg.verify(e.TouristAccount))) return "TouristAccount." + t;
if (null != e.SubChannel && e.hasOwnProperty("SubChannel") && !$util.isString(e.SubChannel)) return "SubChannel: string expected";
if (null != e.Device2 && e.hasOwnProperty("Device2") && (t = $root.pb.DeviceInfo.verify(e.Device2))) return "Device2." + t;
if (null != e.Lan && e.hasOwnProperty("Lan") && !$util.isString(e.Lan)) return "Lan: string expected";
if (null != e.AccountType && e.hasOwnProperty("AccountType")) switch (e.AccountType) {
default:
return "AccountType: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
case 5:
case 6:
case 7:
case 8:
case 9:
case 10:
case 11:
}
return null;
}, gr), r.AccountAuthReply = (br.prototype.ServerSt = null, br.prototype.Ok = !1, 
br.prototype.ChannelID = "", br.prototype.Ext = "", br.prototype.BeInvited = !1, 
br.prototype.FatigueErr = 0, br.prototype.AuthToken = "", br.prototype.Players = $util.emptyArray, 
br.prototype.IsNew = !1, br.prototype.HabbyGameAccountId = "", br.create = function(e) {
return new br(e);
}, br.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.ServerSt && Object.hasOwnProperty.call(e, "ServerSt") && $root.pb.ServerStatus.encode(e.ServerSt, t.uint32(10).fork()).ldelim(), 
null != e.Ok && Object.hasOwnProperty.call(e, "Ok") && t.uint32(16).bool(e.Ok), 
null != e.ChannelID && Object.hasOwnProperty.call(e, "ChannelID") && t.uint32(26).string(e.ChannelID), 
null != e.Ext && Object.hasOwnProperty.call(e, "Ext") && t.uint32(34).string(e.Ext), 
null != e.BeInvited && Object.hasOwnProperty.call(e, "BeInvited") && t.uint32(40).bool(e.BeInvited), 
null != e.FatigueErr && Object.hasOwnProperty.call(e, "FatigueErr") && t.uint32(48).int32(e.FatigueErr), 
null != e.AuthToken && Object.hasOwnProperty.call(e, "AuthToken") && t.uint32(58).string(e.AuthToken), 
null != e.Players && e.Players.length) for (var r = 0; r < e.Players.length; ++r) $root.pb.AccountPlayer.encode(e.Players[r], t.uint32(66).fork()).ldelim();
return null != e.IsNew && Object.hasOwnProperty.call(e, "IsNew") && t.uint32(72).bool(e.IsNew), 
null != e.HabbyGameAccountId && Object.hasOwnProperty.call(e, "HabbyGameAccountId") && t.uint32(82).string(e.HabbyGameAccountId), 
t;
}, br.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, br.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AccountAuthReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.ServerSt = $root.pb.ServerStatus.decode(e, e.uint32());
break;

case 2:
n.Ok = e.bool();
break;

case 3:
n.ChannelID = e.string();
break;

case 4:
n.Ext = e.string();
break;

case 5:
n.BeInvited = e.bool();
break;

case 6:
n.FatigueErr = e.int32();
break;

case 7:
n.AuthToken = e.string();
break;

case 8:
n.Players && n.Players.length || (n.Players = []), n.Players.push($root.pb.AccountPlayer.decode(e, e.uint32()));
break;

case 9:
n.IsNew = e.bool();
break;

case 10:
n.HabbyGameAccountId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, br.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, br.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.ServerSt && e.hasOwnProperty("ServerSt") && (t = $root.pb.ServerStatus.verify(e.ServerSt))) return "ServerSt." + t;
if (null != e.Ok && e.hasOwnProperty("Ok") && "boolean" != typeof e.Ok) return "Ok: boolean expected";
if (null != e.ChannelID && e.hasOwnProperty("ChannelID") && !$util.isString(e.ChannelID)) return "ChannelID: string expected";
if (null != e.Ext && e.hasOwnProperty("Ext") && !$util.isString(e.Ext)) return "Ext: string expected";
if (null != e.BeInvited && e.hasOwnProperty("BeInvited") && "boolean" != typeof e.BeInvited) return "BeInvited: boolean expected";
if (null != e.FatigueErr && e.hasOwnProperty("FatigueErr")) switch (e.FatigueErr) {
default:
return "FatigueErr: enum value expected";

case 0:
case 3e4:
case 30001:
case 30002:
case 30003:
case 30004:
case 30005:
case 30006:
case 30007:
}
if (null != e.AuthToken && e.hasOwnProperty("AuthToken") && !$util.isString(e.AuthToken)) return "AuthToken: string expected";
if (null != e.Players && e.hasOwnProperty("Players")) {
if (!Array.isArray(e.Players)) return "Players: array expected";
for (var t, r = 0; r < e.Players.length; ++r) if (t = $root.pb.AccountPlayer.verify(e.Players[r])) return "Players." + t;
}
return null != e.IsNew && e.hasOwnProperty("IsNew") && "boolean" != typeof e.IsNew ? "IsNew: boolean expected" : null != e.HabbyGameAccountId && e.hasOwnProperty("HabbyGameAccountId") && !$util.isString(e.HabbyGameAccountId) ? "HabbyGameAccountId: string expected" : null;
}, br), r.AccountPlayer = (mr.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
mr.prototype.Name = "", mr.prototype.Head = "", mr.prototype.CreateTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
mr.prototype.IsLastLogin = !1, mr.prototype.Server = 0, mr.create = function(e) {
return new mr(e);
}, mr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
null != e.CreateTime && Object.hasOwnProperty.call(e, "CreateTime") && t.uint32(32).int64(e.CreateTime), 
null != e.IsLastLogin && Object.hasOwnProperty.call(e, "IsLastLogin") && t.uint32(40).bool(e.IsLastLogin), 
null != e.Server && Object.hasOwnProperty.call(e, "Server") && t.uint32(48).int32(e.Server), 
t;
}, mr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, mr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.AccountPlayer(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

case 3:
n.Head = e.string();
break;

case 4:
n.CreateTime = e.int64();
break;

case 5:
n.IsLastLogin = e.bool();
break;

case 6:
n.Server = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, mr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, mr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head) ? "Head: string expected" : null != e.CreateTime && e.hasOwnProperty("CreateTime") && !($util.isInteger(e.CreateTime) || e.CreateTime && $util.isInteger(e.CreateTime.low) && $util.isInteger(e.CreateTime.high)) ? "CreateTime: integer|Long expected" : null != e.IsLastLogin && e.hasOwnProperty("IsLastLogin") && "boolean" != typeof e.IsLastLogin ? "IsLastLogin: boolean expected" : null != e.Server && e.hasOwnProperty("Server") && !$util.isInteger(e.Server) ? "Server: integer expected" : null;
}, mr), r.CreatePlayerArg = (wr.prototype.Account = null, wr.create = function(e) {
return new wr(e);
}, wr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && $root.pb.TargetAccount.encode(e.Account, t.uint32(10).fork()).ldelim(), 
t;
}, wr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, wr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.CreatePlayerArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Account = $root.pb.TargetAccount.decode(e, e.uint32()) : e.skipType(7 & i);
}
return n;
}, wr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, wr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Account && e.hasOwnProperty("Account") && (e = $root.pb.TargetAccount.verify(e.Account)) ? "Account." + e : null;
}, wr), r.DelAccountVerifyCode = (vr.prototype.Code = "", vr.create = function(e) {
return new vr(e);
}, vr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Code && Object.hasOwnProperty.call(e, "Code") && t.uint32(10).string(e.Code), 
t;
}, vr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, vr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.DelAccountVerifyCode(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Code = e.string() : e.skipType(7 & i);
}
return n;
}, vr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, vr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Code && e.hasOwnProperty("Code") && !$util.isString(e.Code) ? "Code: string expected" : null;
}, vr), r.DelPlayerReply = (Or.prototype.Cooldown = 0, Or.create = function(e) {
return new Or(e);
}, Or.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Cooldown && Object.hasOwnProperty.call(e, "Cooldown") && t.uint32(8).int32(e.Cooldown), 
t;
}, Or.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Or.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.DelPlayerReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Cooldown = e.int32() : e.skipType(7 & i);
}
return n;
}, Or.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Or.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Cooldown && e.hasOwnProperty("Cooldown") && !$util.isInteger(e.Cooldown) ? "Cooldown: integer expected" : null;
}, Or), r.RegisterAccount = ($r.prototype.Channel = "", $r.prototype.Account = "", 
$r.prototype.Password = "", $r.prototype.SubChannel = "", $r.create = function(e) {
return new $r(e);
}, $r.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
null != e.Account && Object.hasOwnProperty.call(e, "Account") && t.uint32(18).string(e.Account), 
null != e.Password && Object.hasOwnProperty.call(e, "Password") && t.uint32(26).string(e.Password), 
null != e.SubChannel && Object.hasOwnProperty.call(e, "SubChannel") && t.uint32(34).string(e.SubChannel), 
t;
}, $r.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, $r.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RegisterAccount(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Channel = e.string();
break;

case 2:
n.Account = e.string();
break;

case 3:
n.Password = e.string();
break;

case 4:
n.SubChannel = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, $r.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, $r.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel) ? "Channel: string expected" : null != e.Account && e.hasOwnProperty("Account") && !$util.isString(e.Account) ? "Account: string expected" : null != e.Password && e.hasOwnProperty("Password") && !$util.isString(e.Password) ? "Password: string expected" : null != e.SubChannel && e.hasOwnProperty("SubChannel") && !$util.isString(e.SubChannel) ? "SubChannel: string expected" : null;
}, $r), r.ResourceModify = (Rr.prototype.Res = $util.emptyArray, Rr.create = function(e) {
return new Rr(e);
}, Rr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Res && e.Res.length) for (var r = 0; r < e.Res.length; ++r) $root.pb.Resource.encode(e.Res[r], t.uint32(10).fork()).ldelim();
return t;
}, Rr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Rr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ResourceModify(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Res && n.Res.length || (n.Res = []), n.Res.push($root.pb.Resource.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Rr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Rr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Res && e.hasOwnProperty("Res")) {
if (!Array.isArray(e.Res)) return "Res: array expected";
for (var t = 0; t < e.Res.length; ++t) {
var r = $root.pb.Resource.verify(e.Res[t]);
if (r) return "Res." + r;
}
}
return null;
}, Rr), r.Version = (Ir.prototype.V1 = 0, Ir.prototype.V2 = 0, Ir.prototype.V3 = 0, 
Ir.create = function(e) {
return new Ir(e);
}, Ir.encode = function(e, t) {
return t = t || $Writer.create(), null != e.V1 && Object.hasOwnProperty.call(e, "V1") && t.uint32(8).int32(e.V1), 
null != e.V2 && Object.hasOwnProperty.call(e, "V2") && t.uint32(16).int32(e.V2), 
null != e.V3 && Object.hasOwnProperty.call(e, "V3") && t.uint32(24).int32(e.V3), 
t;
}, Ir.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ir.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Version(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.V1 = e.int32();
break;

case 2:
n.V2 = e.int32();
break;

case 3:
n.V3 = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ir.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ir.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.V1 && e.hasOwnProperty("V1") && !$util.isInteger(e.V1) ? "V1: integer expected" : null != e.V2 && e.hasOwnProperty("V2") && !$util.isInteger(e.V2) ? "V2: integer expected" : null != e.V3 && e.hasOwnProperty("V3") && !$util.isInteger(e.V3) ? "V3: integer expected" : null;
}, Ir), r.TouristRegisterAccountArg = (kr.prototype.Channel = "", kr.create = function(e) {
return new kr(e);
}, kr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
t;
}, kr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, kr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TouristRegisterAccountArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Channel = e.string() : e.skipType(7 & i);
}
return n;
}, kr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, kr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel) ? "Channel: string expected" : null;
}, kr), r.TouristRegisterAccountRelpy = (Pr.prototype.Account = "", Pr.prototype.Password = "", 
Pr.create = function(e) {
return new Pr(e);
}, Pr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && t.uint32(10).string(e.Account), 
null != e.Password && Object.hasOwnProperty.call(e, "Password") && t.uint32(18).string(e.Password), 
t;
}, Pr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Pr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TouristRegisterAccountRelpy(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Account = e.string();
break;

case 2:
n.Password = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Pr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Pr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Account && e.hasOwnProperty("Account") && !$util.isString(e.Account) ? "Account: string expected" : null != e.Password && e.hasOwnProperty("Password") && !$util.isString(e.Password) ? "Password: string expected" : null;
}, Pr), r.TouristBindAccountArg = (_r.prototype.TouristAccount = null, _r.prototype.BindAccount = null, 
_r.create = function(e) {
return new _r(e);
}, _r.encode = function(e, t) {
return t = t || $Writer.create(), null != e.TouristAccount && Object.hasOwnProperty.call(e, "TouristAccount") && $root.pb.AccountLoginArg.encode(e.TouristAccount, t.uint32(10).fork()).ldelim(), 
null != e.BindAccount && Object.hasOwnProperty.call(e, "BindAccount") && $root.pb.SdkAccountLoginArg.encode(e.BindAccount, t.uint32(34).fork()).ldelim(), 
t;
}, _r.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, _r.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TouristBindAccountArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.TouristAccount = $root.pb.AccountLoginArg.decode(e, e.uint32());
break;

case 4:
n.BindAccount = $root.pb.SdkAccountLoginArg.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, _r.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, _r.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.TouristAccount && e.hasOwnProperty("TouristAccount") && (t = $root.pb.AccountLoginArg.verify(e.TouristAccount)) ? "TouristAccount." + t : null != e.BindAccount && e.hasOwnProperty("BindAccount") && (t = $root.pb.SdkAccountLoginArg.verify(e.BindAccount)) ? "BindAccount." + t : null;
var t;
}, _r), r.TellMe = (xr.prototype.Msg = "", xr.prototype.Text = 0, xr.create = function(e) {
return new xr(e);
}, xr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Msg && Object.hasOwnProperty.call(e, "Msg") && t.uint32(10).string(e.Msg), 
null != e.Text && Object.hasOwnProperty.call(e, "Text") && t.uint32(16).int32(e.Text), 
t;
}, xr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, xr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TellMe(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Msg = e.string();
break;

case 2:
n.Text = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, xr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, xr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Msg && e.hasOwnProperty("Msg") && !$util.isString(e.Msg) ? "Msg: string expected" : null != e.Text && e.hasOwnProperty("Text") && !$util.isInteger(e.Text) ? "Text: integer expected" : null;
}, xr), r.Pong = (Ar.prototype.ServerTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
Ar.create = function(e) {
return new Ar(e);
}, Ar.encode = function(e, t) {
return t = t || $Writer.create(), null != e.ServerTime && Object.hasOwnProperty.call(e, "ServerTime") && t.uint32(8).int64(e.ServerTime), 
t;
}, Ar.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ar.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Pong(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.ServerTime = e.int64() : e.skipType(7 & i);
}
return n;
}, Ar.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ar.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.ServerTime && e.hasOwnProperty("ServerTime") && !($util.isInteger(e.ServerTime) || e.ServerTime && $util.isInteger(e.ServerTime.low) && $util.isInteger(e.ServerTime.high)) ? "ServerTime: integer|Long expected" : null;
}, Ar), r.BindInviteCode = (jr.prototype.Account = null, jr.prototype.InviteCode = "", 
jr.create = function(e) {
return new jr(e);
}, jr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Account && Object.hasOwnProperty.call(e, "Account") && $root.pb.TargetAccount.encode(e.Account, t.uint32(10).fork()).ldelim(), 
null != e.InviteCode && Object.hasOwnProperty.call(e, "InviteCode") && t.uint32(18).string(e.InviteCode), 
t;
}, jr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, jr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BindInviteCode(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Account = $root.pb.TargetAccount.decode(e, e.uint32());
break;

case 2:
n.InviteCode = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, jr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, jr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Account && e.hasOwnProperty("Account")) {
var t = $root.pb.TargetAccount.verify(e.Account);
if (t) return "Account." + t;
}
return null != e.InviteCode && e.hasOwnProperty("InviteCode") && !$util.isString(e.InviteCode) ? "InviteCode: string expected" : null;
}, jr), r.TargetAccount = (Sr.prototype.Channel = "", Sr.prototype.LoginChannel = "", 
Sr.prototype.SubChannel = "", Sr.prototype.ChannelUid = "", Sr.create = function(e) {
return new Sr(e);
}, Sr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
null != e.LoginChannel && Object.hasOwnProperty.call(e, "LoginChannel") && t.uint32(18).string(e.LoginChannel), 
null != e.SubChannel && Object.hasOwnProperty.call(e, "SubChannel") && t.uint32(26).string(e.SubChannel), 
null != e.ChannelUid && Object.hasOwnProperty.call(e, "ChannelUid") && t.uint32(34).string(e.ChannelUid), 
t;
}, Sr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Sr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetAccount(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Channel = e.string();
break;

case 2:
n.LoginChannel = e.string();
break;

case 3:
n.SubChannel = e.string();
break;

case 4:
n.ChannelUid = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Sr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Sr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel) ? "Channel: string expected" : null != e.LoginChannel && e.hasOwnProperty("LoginChannel") && !$util.isString(e.LoginChannel) ? "LoginChannel: string expected" : null != e.SubChannel && e.hasOwnProperty("SubChannel") && !$util.isString(e.SubChannel) ? "SubChannel: string expected" : null != e.ChannelUid && e.hasOwnProperty("ChannelUid") && !$util.isString(e.ChannelUid) ? "ChannelUid: string expected" : null;
}, Sr), r.GmCommand = (Tr.prototype.Command = "", Tr.create = function(e) {
return new Tr(e);
}, Tr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Command && Object.hasOwnProperty.call(e, "Command") && t.uint32(10).string(e.Command), 
t;
}, Tr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Tr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GmCommand(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Command = e.string() : e.skipType(7 & i);
}
return n;
}, Tr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Tr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Command && e.hasOwnProperty("Command") && !$util.isString(e.Command) ? "Command: string expected" : null;
}, Tr), r.TargetPlayer = (Cr.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Cr.create = function(e) {
return new Cr(e);
}, Cr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
t;
}, Cr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Cr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetPlayer(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Uid = e.uint64() : e.skipType(7 & i);
}
return n;
}, Cr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Cr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null;
}, Cr), r.DeviceInfo = (Lr.prototype.idfa = "", Lr.prototype.imei = "", Lr.prototype.oaid = "", 
Lr.prototype.device = "", Lr.prototype.device_model = "", Lr.prototype.device_brand = "", 
Lr.prototype.os_version = "", Lr.prototype.ua = "", Lr.prototype.caid = "", Lr.prototype.TgaDeviceId = "", 
Lr.prototype.BundleId = "", Lr.create = function(e) {
return new Lr(e);
}, Lr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.idfa && Object.hasOwnProperty.call(e, "idfa") && t.uint32(10).string(e.idfa), 
null != e.imei && Object.hasOwnProperty.call(e, "imei") && t.uint32(18).string(e.imei), 
null != e.oaid && Object.hasOwnProperty.call(e, "oaid") && t.uint32(26).string(e.oaid), 
null != e.device && Object.hasOwnProperty.call(e, "device") && t.uint32(34).string(e.device), 
null != e.device_model && Object.hasOwnProperty.call(e, "device_model") && t.uint32(42).string(e.device_model), 
null != e.device_brand && Object.hasOwnProperty.call(e, "device_brand") && t.uint32(50).string(e.device_brand), 
null != e.os_version && Object.hasOwnProperty.call(e, "os_version") && t.uint32(58).string(e.os_version), 
null != e.ua && Object.hasOwnProperty.call(e, "ua") && t.uint32(66).string(e.ua), 
null != e.caid && Object.hasOwnProperty.call(e, "caid") && t.uint32(74).string(e.caid), 
null != e.TgaDeviceId && Object.hasOwnProperty.call(e, "TgaDeviceId") && t.uint32(82).string(e.TgaDeviceId), 
null != e.BundleId && Object.hasOwnProperty.call(e, "BundleId") && t.uint32(90).string(e.BundleId), 
t;
}, Lr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Lr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.DeviceInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.idfa = e.string();
break;

case 2:
n.imei = e.string();
break;

case 3:
n.oaid = e.string();
break;

case 4:
n.device = e.string();
break;

case 5:
n.device_model = e.string();
break;

case 6:
n.device_brand = e.string();
break;

case 7:
n.os_version = e.string();
break;

case 8:
n.ua = e.string();
break;

case 9:
n.caid = e.string();
break;

case 10:
n.TgaDeviceId = e.string();
break;

case 11:
n.BundleId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Lr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Lr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.idfa && e.hasOwnProperty("idfa") && !$util.isString(e.idfa) ? "idfa: string expected" : null != e.imei && e.hasOwnProperty("imei") && !$util.isString(e.imei) ? "imei: string expected" : null != e.oaid && e.hasOwnProperty("oaid") && !$util.isString(e.oaid) ? "oaid: string expected" : null != e.device && e.hasOwnProperty("device") && !$util.isString(e.device) ? "device: string expected" : null != e.device_model && e.hasOwnProperty("device_model") && !$util.isString(e.device_model) ? "device_model: string expected" : null != e.device_brand && e.hasOwnProperty("device_brand") && !$util.isString(e.device_brand) ? "device_brand: string expected" : null != e.os_version && e.hasOwnProperty("os_version") && !$util.isString(e.os_version) ? "os_version: string expected" : null != e.ua && e.hasOwnProperty("ua") && !$util.isString(e.ua) ? "ua: string expected" : null != e.caid && e.hasOwnProperty("caid") && !$util.isString(e.caid) ? "caid: string expected" : null != e.TgaDeviceId && e.hasOwnProperty("TgaDeviceId") && !$util.isString(e.TgaDeviceId) ? "TgaDeviceId: string expected" : null != e.BundleId && e.hasOwnProperty("BundleId") && !$util.isString(e.BundleId) ? "BundleId: string expected" : null;
}, Lr), r.FunctionConfig = (Dr.prototype.Debug = !1, Dr.prototype.HabbyEnabled = !1, 
Dr.create = function(e) {
return new Dr(e);
}, Dr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Debug && Object.hasOwnProperty.call(e, "Debug") && t.uint32(8).bool(e.Debug), 
null != e.HabbyEnabled && Object.hasOwnProperty.call(e, "HabbyEnabled") && t.uint32(16).bool(e.HabbyEnabled), 
t;
}, Dr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Dr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FunctionConfig(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Debug = e.bool();
break;

case 2:
n.HabbyEnabled = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Dr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Dr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Debug && e.hasOwnProperty("Debug") && "boolean" != typeof e.Debug ? "Debug: boolean expected" : null != e.HabbyEnabled && e.hasOwnProperty("HabbyEnabled") && "boolean" != typeof e.HabbyEnabled ? "HabbyEnabled: boolean expected" : null;
}, Dr), r.GetIpReply = (Gr.prototype.IP = "", Gr.create = function(e) {
return new Gr(e);
}, Gr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.IP && Object.hasOwnProperty.call(e, "IP") && t.uint32(10).string(e.IP), 
t;
}, Gr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Gr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetIpReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.IP = e.string() : e.skipType(7 & i);
}
return n;
}, Gr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Gr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.IP && e.hasOwnProperty("IP") && !$util.isString(e.IP) ? "IP: string expected" : null;
}, Gr), r.UpdatePhysicalRecovery = (Er.prototype.RemainTime = 0, Er.create = function(e) {
return new Er(e);
}, Er.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(8).int32(e.RemainTime), 
t;
}, Er.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Er.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdatePhysicalRecovery(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.RemainTime = e.int32() : e.skipType(7 & i);
}
return n;
}, Er.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Er.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime) ? "RemainTime: integer expected" : null;
}, Er), r.Chapter = (Wr.prototype.Id = 0, Wr.prototype.Level = 0, Wr.prototype.RandomSeed = 0, 
Wr.prototype.Type = 0, Wr.prototype.Monster = $util.emptyArray, Wr.create = function(e) {
return new Wr(e);
}, Wr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.RandomSeed && Object.hasOwnProperty.call(e, "RandomSeed") && t.uint32(24).int32(e.RandomSeed), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(32).int32(e.Type), 
null != e.Monster && e.Monster.length) {
t.uint32(42).fork();
for (var r = 0; r < e.Monster.length; ++r) t.int32(e.Monster[r]);
t.ldelim();
}
return t;
}, Wr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Wr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Chapter(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.RandomSeed = e.int32();
break;

case 4:
n.Type = e.int32();
break;

case 5:
if (n.Monster && n.Monster.length || (n.Monster = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Monster.push(e.int32()); else n.Monster.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Wr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Wr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.RandomSeed && e.hasOwnProperty("RandomSeed") && !$util.isInteger(e.RandomSeed)) return "RandomSeed: integer expected";
if (null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type)) return "Type: integer expected";
if (null != e.Monster && e.hasOwnProperty("Monster")) {
if (!Array.isArray(e.Monster)) return "Monster: array expected";
for (var t = 0; t < e.Monster.length; ++t) if (!$util.isInteger(e.Monster[t])) return "Monster: integer[] expected";
}
return null;
}, Wr), r.UpdateName = (Hr.prototype.Name = "", Hr.create = function(e) {
return new Hr(e);
}, Hr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(10).string(e.Name), 
t;
}, Hr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Hr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpdateName(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Name = e.string() : e.skipType(7 & i);
}
return n;
}, Hr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Hr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null;
}, Hr), r.PlayerLoginDay = (Br.prototype.TotalLoginDays = 0, Br.prototype.RegisterDays = 0, 
Br.create = function(e) {
return new Br(e);
}, Br.encode = function(e, t) {
return t = t || $Writer.create(), null != e.TotalLoginDays && Object.hasOwnProperty.call(e, "TotalLoginDays") && t.uint32(8).int32(e.TotalLoginDays), 
null != e.RegisterDays && Object.hasOwnProperty.call(e, "RegisterDays") && t.uint32(16).int32(e.RegisterDays), 
t;
}, Br.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Br.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.PlayerLoginDay(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.TotalLoginDays = e.int32();
break;

case 2:
n.RegisterDays = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Br.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Br.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.TotalLoginDays && e.hasOwnProperty("TotalLoginDays") && !$util.isInteger(e.TotalLoginDays) ? "TotalLoginDays: integer expected" : null != e.RegisterDays && e.hasOwnProperty("RegisterDays") && !$util.isInteger(e.RegisterDays) ? "RegisterDays: integer expected" : null;
}, Br), r.CrossDayArg = (Ur.prototype.LoginDays = null, Ur.create = function(e) {
return new Ur(e);
}, Ur.encode = function(e, t) {
return t = t || $Writer.create(), null != e.LoginDays && Object.hasOwnProperty.call(e, "LoginDays") && $root.pb.PlayerLoginDay.encode(e.LoginDays, t.uint32(10).fork()).ldelim(), 
t;
}, Ur.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ur.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.CrossDayArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.LoginDays = $root.pb.PlayerLoginDay.decode(e, e.uint32()) : e.skipType(7 & i);
}
return n;
}, Ur.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ur.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.LoginDays && e.hasOwnProperty("LoginDays") && (e = $root.pb.PlayerLoginDay.verify(e.LoginDays)) ? "LoginDays." + e : null;
}, Ur), r.PlayerInfo = (Nr.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Nr.prototype.Name = "", Nr.prototype.Head = "", Nr.prototype.Heros = $util.emptyArray, 
Nr.prototype.Wall = null, Nr.prototype.WearRunes = $util.emptyArray, Nr.prototype.WearGems = $util.emptyArray, 
Nr.prototype.HeroWearGems = $util.emptyArray, Nr.prototype.ChallengeLevels = $util.emptyArray, 
Nr.prototype.IsFriend = !1, Nr.prototype.Server = 0, Nr.prototype.Ranks = $util.emptyArray, 
Nr.create = function(e) {
return new Nr(e);
}, Nr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
null != e.Heros && e.Heros.length) for (var r = 0; r < e.Heros.length; ++r) $root.pb.HeroInfo.encode(e.Heros[r], t.uint32(34).fork()).ldelim();
if (null != e.Wall && Object.hasOwnProperty.call(e, "Wall") && $root.pb.WallInfo.encode(e.Wall, t.uint32(42).fork()).ldelim(), 
null != e.WearRunes && e.WearRunes.length) for (r = 0; r < e.WearRunes.length; ++r) $root.pb.WearRuneInfo.encode(e.WearRunes[r], t.uint32(50).fork()).ldelim();
if (null != e.WearGems && e.WearGems.length) for (r = 0; r < e.WearGems.length; ++r) $root.pb.WearGemInfo.encode(e.WearGems[r], t.uint32(58).fork()).ldelim();
if (null != e.HeroWearGems && e.HeroWearGems.length) for (r = 0; r < e.HeroWearGems.length; ++r) $root.pb.HeroWearGemInfo.encode(e.HeroWearGems[r], t.uint32(66).fork()).ldelim();
if (null != e.ChallengeLevels && e.ChallengeLevels.length) for (r = 0; r < e.ChallengeLevels.length; ++r) $root.pb.ChallengeLevelInfo.encode(e.ChallengeLevels[r], t.uint32(74).fork()).ldelim();
if (null != e.IsFriend && Object.hasOwnProperty.call(e, "IsFriend") && t.uint32(80).bool(e.IsFriend), 
null != e.Server && Object.hasOwnProperty.call(e, "Server") && t.uint32(88).int32(e.Server), 
null != e.Ranks && e.Ranks.length) for (r = 0; r < e.Ranks.length; ++r) $root.pb.LevelRank.encode(e.Ranks[r], t.uint32(98).fork()).ldelim();
return t;
}, Nr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Nr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.PlayerInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

case 3:
n.Head = e.string();
break;

case 4:
n.Heros && n.Heros.length || (n.Heros = []), n.Heros.push($root.pb.HeroInfo.decode(e, e.uint32()));
break;

case 5:
n.Wall = $root.pb.WallInfo.decode(e, e.uint32());
break;

case 6:
n.WearRunes && n.WearRunes.length || (n.WearRunes = []), n.WearRunes.push($root.pb.WearRuneInfo.decode(e, e.uint32()));
break;

case 7:
n.WearGems && n.WearGems.length || (n.WearGems = []), n.WearGems.push($root.pb.WearGemInfo.decode(e, e.uint32()));
break;

case 8:
n.HeroWearGems && n.HeroWearGems.length || (n.HeroWearGems = []), n.HeroWearGems.push($root.pb.HeroWearGemInfo.decode(e, e.uint32()));
break;

case 9:
n.ChallengeLevels && n.ChallengeLevels.length || (n.ChallengeLevels = []), n.ChallengeLevels.push($root.pb.ChallengeLevelInfo.decode(e, e.uint32()));
break;

case 10:
n.IsFriend = e.bool();
break;

case 11:
n.Server = e.int32();
break;

case 12:
n.Ranks && n.Ranks.length || (n.Ranks = []), n.Ranks.push($root.pb.LevelRank.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Nr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Nr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high))) return "Uid: integer|Long expected";
if (null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name)) return "Name: string expected";
if (null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head)) return "Head: string expected";
if (null != e.Heros && e.hasOwnProperty("Heros")) {
if (!Array.isArray(e.Heros)) return "Heros: array expected";
for (var t = 0; t < e.Heros.length; ++t) if (r = $root.pb.HeroInfo.verify(e.Heros[t])) return "Heros." + r;
}
if (null != e.Wall && e.hasOwnProperty("Wall") && (r = $root.pb.WallInfo.verify(e.Wall))) return "Wall." + r;
if (null != e.WearRunes && e.hasOwnProperty("WearRunes")) {
if (!Array.isArray(e.WearRunes)) return "WearRunes: array expected";
for (t = 0; t < e.WearRunes.length; ++t) if (r = $root.pb.WearRuneInfo.verify(e.WearRunes[t])) return "WearRunes." + r;
}
if (null != e.WearGems && e.hasOwnProperty("WearGems")) {
if (!Array.isArray(e.WearGems)) return "WearGems: array expected";
for (t = 0; t < e.WearGems.length; ++t) if (r = $root.pb.WearGemInfo.verify(e.WearGems[t])) return "WearGems." + r;
}
if (null != e.HeroWearGems && e.hasOwnProperty("HeroWearGems")) {
if (!Array.isArray(e.HeroWearGems)) return "HeroWearGems: array expected";
for (t = 0; t < e.HeroWearGems.length; ++t) if (r = $root.pb.HeroWearGemInfo.verify(e.HeroWearGems[t])) return "HeroWearGems." + r;
}
if (null != e.ChallengeLevels && e.hasOwnProperty("ChallengeLevels")) {
if (!Array.isArray(e.ChallengeLevels)) return "ChallengeLevels: array expected";
for (t = 0; t < e.ChallengeLevels.length; ++t) if (r = $root.pb.ChallengeLevelInfo.verify(e.ChallengeLevels[t])) return "ChallengeLevels." + r;
}
if (null != e.IsFriend && e.hasOwnProperty("IsFriend") && "boolean" != typeof e.IsFriend) return "IsFriend: boolean expected";
if (null != e.Server && e.hasOwnProperty("Server") && !$util.isInteger(e.Server)) return "Server: integer expected";
if (null != e.Ranks && e.hasOwnProperty("Ranks")) {
if (!Array.isArray(e.Ranks)) return "Ranks: array expected";
var r;
for (t = 0; t < e.Ranks.length; ++t) if (r = $root.pb.LevelRank.verify(e.Ranks[t])) return "Ranks." + r;
}
return null;
}, Nr), r.LevelRank = (Fr.prototype.Rank = 0, Fr.prototype.Level = 0, Fr.prototype.Wave = 0, 
Fr.prototype.Score = 0, Fr.prototype.Type = 0, Fr.create = function(e) {
return new Fr(e);
}, Fr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Rank && Object.hasOwnProperty.call(e, "Rank") && t.uint32(8).int32(e.Rank), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.Wave && Object.hasOwnProperty.call(e, "Wave") && t.uint32(24).int32(e.Wave), 
null != e.Score && Object.hasOwnProperty.call(e, "Score") && t.uint32(32).int32(e.Score), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(40).int32(e.Type), 
t;
}, Fr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Fr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelRank(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Rank = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.Wave = e.int32();
break;

case 4:
n.Score = e.int32();
break;

case 5:
n.Type = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Fr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Fr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Rank && e.hasOwnProperty("Rank") && !$util.isInteger(e.Rank) ? "Rank: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.Wave && e.hasOwnProperty("Wave") && !$util.isInteger(e.Wave) ? "Wave: integer expected" : null != e.Score && e.hasOwnProperty("Score") && !$util.isInteger(e.Score) ? "Score: integer expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null;
}, Fr), r.ExchangeGiftCodeArg = (Mr.prototype.Code = "", Mr.create = function(e) {
return new Mr(e);
}, Mr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Code && Object.hasOwnProperty.call(e, "Code") && t.uint32(10).string(e.Code), 
t;
}, Mr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Mr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ExchangeGiftCodeArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Code = e.string() : e.skipType(7 & i);
}
return n;
}, Mr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Mr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Code && e.hasOwnProperty("Code") && !$util.isString(e.Code) ? "Code: string expected" : null;
}, Mr), r.BindWechatReply = (Vr.prototype.ChannelUid = "", Vr.prototype.Nickname = "", 
Vr.prototype.Head = "", Vr.create = function(e) {
return new Vr(e);
}, Vr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.ChannelUid && Object.hasOwnProperty.call(e, "ChannelUid") && t.uint32(10).string(e.ChannelUid), 
null != e.Nickname && Object.hasOwnProperty.call(e, "Nickname") && t.uint32(18).string(e.Nickname), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
t;
}, Vr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Vr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BindWechatReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.ChannelUid = e.string();
break;

case 2:
n.Nickname = e.string();
break;

case 3:
n.Head = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Vr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Vr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.ChannelUid && e.hasOwnProperty("ChannelUid") && !$util.isString(e.ChannelUid) ? "ChannelUid: string expected" : null != e.Nickname && e.hasOwnProperty("Nickname") && !$util.isString(e.Nickname) ? "Nickname: string expected" : null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head) ? "Head: string expected" : null;
}, Vr), r.HuoHuanChannel = (qr.prototype.Channel = "", qr.prototype.SubChannel = "", 
qr.prototype.AppId = "", qr.prototype.GameId = "", qr.create = function(e) {
return new qr(e);
}, qr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Channel && Object.hasOwnProperty.call(e, "Channel") && t.uint32(10).string(e.Channel), 
null != e.SubChannel && Object.hasOwnProperty.call(e, "SubChannel") && t.uint32(18).string(e.SubChannel), 
null != e.AppId && Object.hasOwnProperty.call(e, "AppId") && t.uint32(26).string(e.AppId), 
null != e.GameId && Object.hasOwnProperty.call(e, "GameId") && t.uint32(34).string(e.GameId), 
t;
}, qr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, qr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.HuoHuanChannel(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Channel = e.string();
break;

case 2:
n.SubChannel = e.string();
break;

case 3:
n.AppId = e.string();
break;

case 4:
n.GameId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, qr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, qr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Channel && e.hasOwnProperty("Channel") && !$util.isString(e.Channel) ? "Channel: string expected" : null != e.SubChannel && e.hasOwnProperty("SubChannel") && !$util.isString(e.SubChannel) ? "SubChannel: string expected" : null != e.AppId && e.hasOwnProperty("AppId") && !$util.isString(e.AppId) ? "AppId: string expected" : null != e.GameId && e.hasOwnProperty("GameId") && !$util.isString(e.GameId) ? "GameId: string expected" : null;
}, qr), r.WallInfo = (Jr.prototype.Level = 0, Jr.prototype.Skills = $util.emptyArray, 
Jr.prototype.Entrys = $util.emptyArray, Jr.prototype.SkillOptions = $util.emptyArray, 
Jr.prototype.EntryOptions = $util.emptyArray, Jr.prototype.TalentPoints = 0, Jr.prototype.Talents = $util.emptyArray, 
Jr.prototype.Skins = $util.emptyArray, Jr.prototype.WearSkin = "", Jr.prototype.EntryResetLevel = 0, 
Jr.create = function(e) {
return new Jr(e);
}, Jr.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(8).int32(e.Level), 
null != e.Skills && e.Skills.length) for (var r = 0; r < e.Skills.length; ++r) $root.pb.WallSkill.encode(e.Skills[r], t.uint32(18).fork()).ldelim();
if (null != e.Entrys && e.Entrys.length) for (r = 0; r < e.Entrys.length; ++r) $root.pb.WallEntry.encode(e.Entrys[r], t.uint32(26).fork()).ldelim();
if (null != e.SkillOptions && e.SkillOptions.length) {
t.uint32(34).fork();
for (r = 0; r < e.SkillOptions.length; ++r) t.int32(e.SkillOptions[r]);
t.ldelim();
}
if (null != e.EntryOptions && e.EntryOptions.length) {
t.uint32(42).fork();
for (r = 0; r < e.EntryOptions.length; ++r) t.int32(e.EntryOptions[r]);
t.ldelim();
}
if (null != e.TalentPoints && Object.hasOwnProperty.call(e, "TalentPoints") && t.uint32(48).int32(e.TalentPoints), 
null != e.Talents && e.Talents.length) for (r = 0; r < e.Talents.length; ++r) $root.pb.WallTalent.encode(e.Talents[r], t.uint32(58).fork()).ldelim();
if (null != e.Skins && e.Skins.length) for (r = 0; r < e.Skins.length; ++r) $root.pb.WallSkin.encode(e.Skins[r], t.uint32(66).fork()).ldelim();
return null != e.WearSkin && Object.hasOwnProperty.call(e, "WearSkin") && t.uint32(74).string(e.WearSkin), 
null != e.EntryResetLevel && Object.hasOwnProperty.call(e, "EntryResetLevel") && t.uint32(80).int32(e.EntryResetLevel), 
t;
}, Jr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Jr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WallInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Level = e.int32();
break;

case 2:
n.Skills && n.Skills.length || (n.Skills = []), n.Skills.push($root.pb.WallSkill.decode(e, e.uint32()));
break;

case 3:
n.Entrys && n.Entrys.length || (n.Entrys = []), n.Entrys.push($root.pb.WallEntry.decode(e, e.uint32()));
break;

case 4:
if (n.SkillOptions && n.SkillOptions.length || (n.SkillOptions = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.SkillOptions.push(e.int32()); else n.SkillOptions.push(e.int32());
break;

case 5:
if (n.EntryOptions && n.EntryOptions.length || (n.EntryOptions = []), 2 == (7 & i)) for (o = e.uint32() + e.pos; e.pos < o; ) n.EntryOptions.push(e.int32()); else n.EntryOptions.push(e.int32());
break;

case 6:
n.TalentPoints = e.int32();
break;

case 7:
n.Talents && n.Talents.length || (n.Talents = []), n.Talents.push($root.pb.WallTalent.decode(e, e.uint32()));
break;

case 8:
n.Skins && n.Skins.length || (n.Skins = []), n.Skins.push($root.pb.WallSkin.decode(e, e.uint32()));
break;

case 9:
n.WearSkin = e.string();
break;

case 10:
n.EntryResetLevel = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Jr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Jr.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.Skills && e.hasOwnProperty("Skills")) {
if (!Array.isArray(e.Skills)) return "Skills: array expected";
for (var t = 0; t < e.Skills.length; ++t) if (r = $root.pb.WallSkill.verify(e.Skills[t])) return "Skills." + r;
}
if (null != e.Entrys && e.hasOwnProperty("Entrys")) {
if (!Array.isArray(e.Entrys)) return "Entrys: array expected";
for (t = 0; t < e.Entrys.length; ++t) if (r = $root.pb.WallEntry.verify(e.Entrys[t])) return "Entrys." + r;
}
if (null != e.SkillOptions && e.hasOwnProperty("SkillOptions")) {
if (!Array.isArray(e.SkillOptions)) return "SkillOptions: array expected";
for (t = 0; t < e.SkillOptions.length; ++t) if (!$util.isInteger(e.SkillOptions[t])) return "SkillOptions: integer[] expected";
}
if (null != e.EntryOptions && e.hasOwnProperty("EntryOptions")) {
if (!Array.isArray(e.EntryOptions)) return "EntryOptions: array expected";
for (t = 0; t < e.EntryOptions.length; ++t) if (!$util.isInteger(e.EntryOptions[t])) return "EntryOptions: integer[] expected";
}
if (null != e.TalentPoints && e.hasOwnProperty("TalentPoints") && !$util.isInteger(e.TalentPoints)) return "TalentPoints: integer expected";
if (null != e.Talents && e.hasOwnProperty("Talents")) {
if (!Array.isArray(e.Talents)) return "Talents: array expected";
for (t = 0; t < e.Talents.length; ++t) if (r = $root.pb.WallTalent.verify(e.Talents[t])) return "Talents." + r;
}
if (null != e.Skins && e.hasOwnProperty("Skins")) {
if (!Array.isArray(e.Skins)) return "Skins: array expected";
var r;
for (t = 0; t < e.Skins.length; ++t) if (r = $root.pb.WallSkin.verify(e.Skins[t])) return "Skins." + r;
}
return null != e.WearSkin && e.hasOwnProperty("WearSkin") && !$util.isString(e.WearSkin) ? "WearSkin: string expected" : null != e.EntryResetLevel && e.hasOwnProperty("EntryResetLevel") && !$util.isInteger(e.EntryResetLevel) ? "EntryResetLevel: integer expected" : null;
}, Jr), r.WallSkill = (Xr.prototype.Id = 0, Xr.prototype.Level = 0, Xr.create = function(e) {
return new Xr(e);
}, Xr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
t;
}, Xr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Xr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WallSkill(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Xr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Xr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null;
}, Xr), r.WallEntry = (Yr.prototype.Id = 0, Yr.prototype.Level = 0, Yr.create = function(e) {
return new Yr(e);
}, Yr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
t;
}, Yr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Yr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WallEntry(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Yr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Yr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null;
}, Yr), r.WallTalent = (zr.prototype.Id = 0, zr.prototype.Level = 0, zr.create = function(e) {
return new zr(e);
}, zr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
t;
}, zr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, zr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WallTalent(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, zr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, zr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null;
}, zr), r.TargetTalent = (Kr.prototype.Id = 0, Kr.create = function(e) {
return new Kr(e);
}, Kr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
t;
}, Kr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Kr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetTalent(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.int32() : e.skipType(7 & i);
}
return n;
}, Kr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Kr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, Kr), r.SelectWallOptionArg = (Qr.prototype.SkillId = 0, Qr.prototype.EntryId = 0, 
Qr.create = function(e) {
return new Qr(e);
}, Qr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.SkillId && Object.hasOwnProperty.call(e, "SkillId") && t.uint32(8).int32(e.SkillId), 
null != e.EntryId && Object.hasOwnProperty.call(e, "EntryId") && t.uint32(16).int32(e.EntryId), 
t;
}, Qr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Qr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SelectWallOptionArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.SkillId = e.int32();
break;

case 2:
n.EntryId = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Qr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Qr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.SkillId && e.hasOwnProperty("SkillId") && !$util.isInteger(e.SkillId) ? "SkillId: integer expected" : null != e.EntryId && e.hasOwnProperty("EntryId") && !$util.isInteger(e.EntryId) ? "EntryId: integer expected" : null;
}, Qr), r.TargetWallSkin = (Zr.prototype.Id = "", Zr.create = function(e) {
return new Zr(e);
}, Zr.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
t;
}, Zr.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Zr.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetWallSkin(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.string() : e.skipType(7 & i);
}
return n;
}, Zr.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Zr.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null;
}, Zr), r.WallSkin = (en.prototype.Id = "", en.prototype.Amount = 0, en.create = function(e) {
return new en(e);
}, en.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(10).string(e.Id), 
null != e.Amount && Object.hasOwnProperty.call(e, "Amount") && t.uint32(16).int32(e.Amount), 
t;
}, en.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, en.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WallSkin(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.string();
break;

case 2:
n.Amount = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, en.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, en.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null != e.Amount && e.hasOwnProperty("Amount") && !$util.isInteger(e.Amount) ? "Amount: integer expected" : null;
}, en), r.ResetWallEntryArg = (tn.prototype.EntryId = 0, tn.create = function(e) {
return new tn(e);
}, tn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.EntryId && Object.hasOwnProperty.call(e, "EntryId") && t.uint32(8).int32(e.EntryId), 
t;
}, tn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, tn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ResetWallEntryArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.EntryId = e.int32() : e.skipType(7 & i);
}
return n;
}, tn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, tn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.EntryId && e.hasOwnProperty("EntryId") && !$util.isInteger(e.EntryId) ? "EntryId: integer expected" : null;
}, tn), r.Rune = (rn.prototype.Id = 0, rn.prototype.Gid = 0, rn.prototype.Suit = 0, 
rn.prototype.Rare = 0, rn.prototype.Level = 0, rn.prototype.Score = 0, rn.prototype.Attributes = $util.emptyArray, 
rn.create = function(e) {
return new rn(e);
}, rn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(16).int32(e.Gid), 
null != e.Suit && Object.hasOwnProperty.call(e, "Suit") && t.uint32(24).int32(e.Suit), 
null != e.Rare && Object.hasOwnProperty.call(e, "Rare") && t.uint32(32).int32(e.Rare), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(40).int32(e.Level), 
null != e.Score && Object.hasOwnProperty.call(e, "Score") && t.uint32(48).int32(e.Score), 
null != e.Attributes && e.Attributes.length) for (var r = 0; r < e.Attributes.length; ++r) $root.pb.RuneAttribute.encode(e.Attributes[r], t.uint32(58).fork()).ldelim();
return t;
}, rn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, rn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Rune(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Gid = e.int32();
break;

case 3:
n.Suit = e.int32();
break;

case 4:
n.Rare = e.int32();
break;

case 5:
n.Level = e.int32();
break;

case 6:
n.Score = e.int32();
break;

case 7:
n.Attributes && n.Attributes.length || (n.Attributes = []), n.Attributes.push($root.pb.RuneAttribute.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, rn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, rn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid)) return "Gid: integer expected";
if (null != e.Suit && e.hasOwnProperty("Suit") && !$util.isInteger(e.Suit)) return "Suit: integer expected";
if (null != e.Rare && e.hasOwnProperty("Rare") && !$util.isInteger(e.Rare)) return "Rare: integer expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.Score && e.hasOwnProperty("Score") && !$util.isInteger(e.Score)) return "Score: integer expected";
if (null != e.Attributes && e.hasOwnProperty("Attributes")) {
if (!Array.isArray(e.Attributes)) return "Attributes: array expected";
for (var t = 0; t < e.Attributes.length; ++t) {
var r = $root.pb.RuneAttribute.verify(e.Attributes[t]);
if (r) return "Attributes." + r;
}
}
return null;
}, rn), r.RuneAttribute = (nn.prototype.Id = 0, nn.prototype.Value = 0, nn.prototype.UpCnt = 0, 
nn.create = function(e) {
return new nn(e);
}, nn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Value && Object.hasOwnProperty.call(e, "Value") && t.uint32(16).int32(e.Value), 
null != e.UpCnt && Object.hasOwnProperty.call(e, "UpCnt") && t.uint32(24).int32(e.UpCnt), 
t;
}, nn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, nn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RuneAttribute(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Value = e.int32();
break;

case 3:
n.UpCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, nn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, nn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null != e.Value && e.hasOwnProperty("Value") && !$util.isInteger(e.Value) ? "Value: integer expected" : null != e.UpCnt && e.hasOwnProperty("UpCnt") && !$util.isInteger(e.UpCnt) ? "UpCnt: integer expected" : null;
}, nn), r.WearRune = (on.prototype.HeroId = 0, on.prototype.WearRuneGids = $util.emptyArray, 
on.create = function(e) {
return new on(e);
}, on.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.WearRuneGids && e.WearRuneGids.length) {
t.uint32(18).fork();
for (var r = 0; r < e.WearRuneGids.length; ++r) t.int32(e.WearRuneGids[r]);
t.ldelim();
}
return t;
}, on.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, on.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearRune(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
if (n.WearRuneGids && n.WearRuneGids.length || (n.WearRuneGids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.WearRuneGids.push(e.int32()); else n.WearRuneGids.push(e.int32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, on.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, on.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId)) return "HeroId: integer expected";
if (null != e.WearRuneGids && e.hasOwnProperty("WearRuneGids")) {
if (!Array.isArray(e.WearRuneGids)) return "WearRuneGids: array expected";
for (var t = 0; t < e.WearRuneGids.length; ++t) if (!$util.isInteger(e.WearRuneGids[t])) return "WearRuneGids: integer[] expected";
}
return null;
}, on), r.WearRuneInfo = (an.prototype.HeroId = 0, an.prototype.Runes = $util.emptyArray, 
an.create = function(e) {
return new an(e);
}, an.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.Runes && e.Runes.length) for (var r = 0; r < e.Runes.length; ++r) $root.pb.Rune.encode(e.Runes[r], t.uint32(18).fork()).ldelim();
return t;
}, an.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, an.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearRuneInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.Runes && n.Runes.length || (n.Runes = []), n.Runes.push($root.pb.Rune.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, an.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, an.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId)) return "HeroId: integer expected";
if (null != e.Runes && e.hasOwnProperty("Runes")) {
if (!Array.isArray(e.Runes)) return "Runes: array expected";
for (var t = 0; t < e.Runes.length; ++t) {
var r = $root.pb.Rune.verify(e.Runes[t]);
if (r) return "Runes." + r;
}
}
return null;
}, an), r.RuneInfo = (ln.prototype.Runes = $util.emptyArray, ln.prototype.WearRunes = $util.emptyArray, 
ln.prototype.CanShow = !1, ln.create = function(e) {
return new ln(e);
}, ln.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Runes && e.Runes.length) for (var r = 0; r < e.Runes.length; ++r) $root.pb.Rune.encode(e.Runes[r], t.uint32(10).fork()).ldelim();
if (null != e.WearRunes && e.WearRunes.length) for (r = 0; r < e.WearRunes.length; ++r) $root.pb.WearRune.encode(e.WearRunes[r], t.uint32(18).fork()).ldelim();
return null != e.CanShow && Object.hasOwnProperty.call(e, "CanShow") && t.uint32(24).bool(e.CanShow), 
t;
}, ln.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ln.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RuneInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Runes && n.Runes.length || (n.Runes = []), n.Runes.push($root.pb.Rune.decode(e, e.uint32()));
break;

case 2:
n.WearRunes && n.WearRunes.length || (n.WearRunes = []), n.WearRunes.push($root.pb.WearRune.decode(e, e.uint32()));
break;

case 3:
n.CanShow = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ln.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ln.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Runes && e.hasOwnProperty("Runes")) {
if (!Array.isArray(e.Runes)) return "Runes: array expected";
for (var t = 0; t < e.Runes.length; ++t) if (r = $root.pb.Rune.verify(e.Runes[t])) return "Runes." + r;
}
if (null != e.WearRunes && e.hasOwnProperty("WearRunes")) {
if (!Array.isArray(e.WearRunes)) return "WearRunes: array expected";
var r;
for (t = 0; t < e.WearRunes.length; ++t) if (r = $root.pb.WearRune.verify(e.WearRunes[t])) return "WearRunes." + r;
}
return null != e.CanShow && e.hasOwnProperty("CanShow") && "boolean" != typeof e.CanShow ? "CanShow: boolean expected" : null;
}, ln), r.TargetRune = (sn.prototype.Gid = 0, sn.create = function(e) {
return new sn(e);
}, sn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
t;
}, sn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, sn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetRune(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Gid = e.int32() : e.skipType(7 & i);
}
return n;
}, sn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, sn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null;
}, sn), r.UpLevelRuneArg = (un.prototype.Gid = 0, un.prototype.UpLevel = 0, un.create = function(e) {
return new un(e);
}, un.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(8).int32(e.Gid), 
null != e.UpLevel && Object.hasOwnProperty.call(e, "UpLevel") && t.uint32(16).int32(e.UpLevel), 
t;
}, un.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, un.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.UpLevelRuneArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Gid = e.int32();
break;

case 2:
n.UpLevel = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, un.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, un.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null != e.UpLevel && e.hasOwnProperty("UpLevel") && !$util.isInteger(e.UpLevel) ? "UpLevel: integer expected" : null;
}, un), r.WearRuneArg = (cn.prototype.HeroId = 0, cn.prototype.Gid = 0, cn.create = function(e) {
return new cn(e);
}, cn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HeroId && Object.hasOwnProperty.call(e, "HeroId") && t.uint32(8).int32(e.HeroId), 
null != e.Gid && Object.hasOwnProperty.call(e, "Gid") && t.uint32(16).int32(e.Gid), 
t;
}, cn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, cn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WearRuneArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId = e.int32();
break;

case 2:
n.Gid = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, cn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, cn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HeroId && e.hasOwnProperty("HeroId") && !$util.isInteger(e.HeroId) ? "HeroId: integer expected" : null != e.Gid && e.hasOwnProperty("Gid") && !$util.isInteger(e.Gid) ? "Gid: integer expected" : null;
}, cn), r.SellRuneArg = (dn.prototype.Gids = $util.emptyArray, dn.create = function(e) {
return new dn(e);
}, dn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gids && e.Gids.length) {
t.uint32(10).fork();
for (var r = 0; r < e.Gids.length; ++r) t.int32(e.Gids[r]);
t.ldelim();
}
return t;
}, dn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, dn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SellRuneArg(); e.pos < r; ) {
var i = e.uint32();
if (i >>> 3 == 1) if (n.Gids && n.Gids.length || (n.Gids = []), 2 == (7 & i)) for (var o = e.uint32() + e.pos; e.pos < o; ) n.Gids.push(e.int32()); else n.Gids.push(e.int32()); else e.skipType(7 & i);
}
return n;
}, dn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, dn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gids && e.hasOwnProperty("Gids")) {
if (!Array.isArray(e.Gids)) return "Gids: array expected";
for (var t = 0; t < e.Gids.length; ++t) if (!$util.isInteger(e.Gids[t])) return "Gids: integer[] expected";
}
return null;
}, dn), r.SwapWearRuneArg = (pn.prototype.HeroId1 = 0, pn.prototype.HeroId2 = 0, 
pn.create = function(e) {
return new pn(e);
}, pn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.HeroId1 && Object.hasOwnProperty.call(e, "HeroId1") && t.uint32(8).int32(e.HeroId1), 
null != e.HeroId2 && Object.hasOwnProperty.call(e, "HeroId2") && t.uint32(16).int32(e.HeroId2), 
t;
}, pn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, pn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SwapWearRuneArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.HeroId1 = e.int32();
break;

case 2:
n.HeroId2 = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, pn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, pn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.HeroId1 && e.hasOwnProperty("HeroId1") && !$util.isInteger(e.HeroId1) ? "HeroId1: integer expected" : null != e.HeroId2 && e.hasOwnProperty("HeroId2") && !$util.isInteger(e.HeroId2) ? "HeroId2: integer expected" : null;
}, pn), r.ShopData = (fn.prototype.Boxs = $util.emptyArray, fn.prototype.Resources = $util.emptyArray, 
fn.prototype.Jades = $util.emptyArray, fn.prototype.Coins = $util.emptyArray, fn.prototype.Vips = $util.emptyArray, 
fn.prototype.Gifts = $util.emptyArray, fn.prototype.PayGifts = $util.emptyArray, 
fn.prototype.RandResources = null, fn.prototype.LevelGifts = $util.emptyArray, fn.create = function(e) {
return new fn(e);
}, fn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Boxs && e.Boxs.length) for (var r = 0; r < e.Boxs.length; ++r) $root.pb.ShopBox.encode(e.Boxs[r], t.uint32(10).fork()).ldelim();
if (null != e.Resources && e.Resources.length) for (r = 0; r < e.Resources.length; ++r) $root.pb.ShopRes.encode(e.Resources[r], t.uint32(18).fork()).ldelim();
if (null != e.Jades && e.Jades.length) for (r = 0; r < e.Jades.length; ++r) $root.pb.ShopJade.encode(e.Jades[r], t.uint32(26).fork()).ldelim();
if (null != e.Coins && e.Coins.length) for (r = 0; r < e.Coins.length; ++r) $root.pb.ShopCoin.encode(e.Coins[r], t.uint32(34).fork()).ldelim();
if (null != e.Vips && e.Vips.length) for (r = 0; r < e.Vips.length; ++r) $root.pb.ShopVip.encode(e.Vips[r], t.uint32(42).fork()).ldelim();
if (null != e.Gifts && e.Gifts.length) for (r = 0; r < e.Gifts.length; ++r) $root.pb.ShopGift.encode(e.Gifts[r], t.uint32(50).fork()).ldelim();
if (null != e.PayGifts && e.PayGifts.length) for (r = 0; r < e.PayGifts.length; ++r) $root.pb.ShopGift.encode(e.PayGifts[r], t.uint32(58).fork()).ldelim();
if (null != e.RandResources && Object.hasOwnProperty.call(e, "RandResources") && $root.pb.ShopRandRes.encode(e.RandResources, t.uint32(66).fork()).ldelim(), 
null != e.LevelGifts && e.LevelGifts.length) for (r = 0; r < e.LevelGifts.length; ++r) $root.pb.LevelGift.encode(e.LevelGifts[r], t.uint32(74).fork()).ldelim();
return t;
}, fn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, fn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopData(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Boxs && n.Boxs.length || (n.Boxs = []), n.Boxs.push($root.pb.ShopBox.decode(e, e.uint32()));
break;

case 2:
n.Resources && n.Resources.length || (n.Resources = []), n.Resources.push($root.pb.ShopRes.decode(e, e.uint32()));
break;

case 3:
n.Jades && n.Jades.length || (n.Jades = []), n.Jades.push($root.pb.ShopJade.decode(e, e.uint32()));
break;

case 4:
n.Coins && n.Coins.length || (n.Coins = []), n.Coins.push($root.pb.ShopCoin.decode(e, e.uint32()));
break;

case 5:
n.Vips && n.Vips.length || (n.Vips = []), n.Vips.push($root.pb.ShopVip.decode(e, e.uint32()));
break;

case 6:
n.Gifts && n.Gifts.length || (n.Gifts = []), n.Gifts.push($root.pb.ShopGift.decode(e, e.uint32()));
break;

case 7:
n.PayGifts && n.PayGifts.length || (n.PayGifts = []), n.PayGifts.push($root.pb.ShopGift.decode(e, e.uint32()));
break;

case 8:
n.RandResources = $root.pb.ShopRandRes.decode(e, e.uint32());
break;

case 9:
n.LevelGifts && n.LevelGifts.length || (n.LevelGifts = []), n.LevelGifts.push($root.pb.LevelGift.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, fn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, fn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Boxs && e.hasOwnProperty("Boxs")) {
if (!Array.isArray(e.Boxs)) return "Boxs: array expected";
for (var t = 0; t < e.Boxs.length; ++t) if (r = $root.pb.ShopBox.verify(e.Boxs[t])) return "Boxs." + r;
}
if (null != e.Resources && e.hasOwnProperty("Resources")) {
if (!Array.isArray(e.Resources)) return "Resources: array expected";
for (t = 0; t < e.Resources.length; ++t) if (r = $root.pb.ShopRes.verify(e.Resources[t])) return "Resources." + r;
}
if (null != e.Jades && e.hasOwnProperty("Jades")) {
if (!Array.isArray(e.Jades)) return "Jades: array expected";
for (t = 0; t < e.Jades.length; ++t) if (r = $root.pb.ShopJade.verify(e.Jades[t])) return "Jades." + r;
}
if (null != e.Coins && e.hasOwnProperty("Coins")) {
if (!Array.isArray(e.Coins)) return "Coins: array expected";
for (t = 0; t < e.Coins.length; ++t) if (r = $root.pb.ShopCoin.verify(e.Coins[t])) return "Coins." + r;
}
if (null != e.Vips && e.hasOwnProperty("Vips")) {
if (!Array.isArray(e.Vips)) return "Vips: array expected";
for (t = 0; t < e.Vips.length; ++t) if (r = $root.pb.ShopVip.verify(e.Vips[t])) return "Vips." + r;
}
if (null != e.Gifts && e.hasOwnProperty("Gifts")) {
if (!Array.isArray(e.Gifts)) return "Gifts: array expected";
for (t = 0; t < e.Gifts.length; ++t) if (r = $root.pb.ShopGift.verify(e.Gifts[t])) return "Gifts." + r;
}
if (null != e.PayGifts && e.hasOwnProperty("PayGifts")) {
if (!Array.isArray(e.PayGifts)) return "PayGifts: array expected";
for (t = 0; t < e.PayGifts.length; ++t) if (r = $root.pb.ShopGift.verify(e.PayGifts[t])) return "PayGifts." + r;
}
if (null != e.RandResources && e.hasOwnProperty("RandResources") && (r = $root.pb.ShopRandRes.verify(e.RandResources))) return "RandResources." + r;
if (null != e.LevelGifts && e.hasOwnProperty("LevelGifts")) {
if (!Array.isArray(e.LevelGifts)) return "LevelGifts: array expected";
var r;
for (t = 0; t < e.LevelGifts.length; ++t) if (r = $root.pb.LevelGift.verify(e.LevelGifts[t])) return "LevelGifts." + r;
}
return null;
}, fn), r.ShopBox = (yn.prototype.GoodsId = "", yn.prototype.BoxId = "", yn.prototype.Price = "", 
yn.prototype.Ten = 0, yn.prototype.GuaranteedCnt = 0, yn.prototype.GuaranteedReward = $util.emptyArray, 
yn.prototype.DailyCnt = 0, yn.prototype.AdvertCnt = 0, yn.prototype.Sort = 0, yn.prototype.MaxDailyCnt = 0, 
yn.prototype.FreeCnt = 0, yn.create = function(e) {
return new yn(e);
}, yn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.BoxId && Object.hasOwnProperty.call(e, "BoxId") && t.uint32(18).string(e.BoxId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(26).string(e.Price), 
null != e.Ten && Object.hasOwnProperty.call(e, "Ten") && t.uint32(32).int32(e.Ten), 
null != e.GuaranteedCnt && Object.hasOwnProperty.call(e, "GuaranteedCnt") && t.uint32(40).int32(e.GuaranteedCnt), 
null != e.GuaranteedReward && e.GuaranteedReward.length) for (var r = 0; r < e.GuaranteedReward.length; ++r) t.uint32(50).string(e.GuaranteedReward[r]);
return null != e.DailyCnt && Object.hasOwnProperty.call(e, "DailyCnt") && t.uint32(56).int32(e.DailyCnt), 
null != e.AdvertCnt && Object.hasOwnProperty.call(e, "AdvertCnt") && t.uint32(64).int32(e.AdvertCnt), 
null != e.Sort && Object.hasOwnProperty.call(e, "Sort") && t.uint32(72).int32(e.Sort), 
null != e.MaxDailyCnt && Object.hasOwnProperty.call(e, "MaxDailyCnt") && t.uint32(80).int32(e.MaxDailyCnt), 
null != e.FreeCnt && Object.hasOwnProperty.call(e, "FreeCnt") && t.uint32(88).int32(e.FreeCnt), 
t;
}, yn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, yn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopBox(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.BoxId = e.string();
break;

case 3:
n.Price = e.string();
break;

case 4:
n.Ten = e.int32();
break;

case 5:
n.GuaranteedCnt = e.int32();
break;

case 6:
n.GuaranteedReward && n.GuaranteedReward.length || (n.GuaranteedReward = []), n.GuaranteedReward.push(e.string());
break;

case 7:
n.DailyCnt = e.int32();
break;

case 8:
n.AdvertCnt = e.int32();
break;

case 9:
n.Sort = e.int32();
break;

case 10:
n.MaxDailyCnt = e.int32();
break;

case 11:
n.FreeCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, yn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, yn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId)) return "GoodsId: string expected";
if (null != e.BoxId && e.hasOwnProperty("BoxId") && !$util.isString(e.BoxId)) return "BoxId: string expected";
if (null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price)) return "Price: string expected";
if (null != e.Ten && e.hasOwnProperty("Ten") && !$util.isInteger(e.Ten)) return "Ten: integer expected";
if (null != e.GuaranteedCnt && e.hasOwnProperty("GuaranteedCnt") && !$util.isInteger(e.GuaranteedCnt)) return "GuaranteedCnt: integer expected";
if (null != e.GuaranteedReward && e.hasOwnProperty("GuaranteedReward")) {
if (!Array.isArray(e.GuaranteedReward)) return "GuaranteedReward: array expected";
for (var t = 0; t < e.GuaranteedReward.length; ++t) if (!$util.isString(e.GuaranteedReward[t])) return "GuaranteedReward: string[] expected";
}
return null != e.DailyCnt && e.hasOwnProperty("DailyCnt") && !$util.isInteger(e.DailyCnt) ? "DailyCnt: integer expected" : null != e.AdvertCnt && e.hasOwnProperty("AdvertCnt") && !$util.isInteger(e.AdvertCnt) ? "AdvertCnt: integer expected" : null != e.Sort && e.hasOwnProperty("Sort") && !$util.isInteger(e.Sort) ? "Sort: integer expected" : null != e.MaxDailyCnt && e.hasOwnProperty("MaxDailyCnt") && !$util.isInteger(e.MaxDailyCnt) ? "MaxDailyCnt: integer expected" : null != e.FreeCnt && e.hasOwnProperty("FreeCnt") && !$util.isInteger(e.FreeCnt) ? "FreeCnt: integer expected" : null;
}, yn), r.BuyShopBoxArg = (hn.prototype.GoodsId = "", hn.prototype.BuyCnt = 0, hn.prototype.Advert = !1, 
hn.prototype.Free = !1, hn.create = function(e) {
return new hn(e);
}, hn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.BuyCnt && Object.hasOwnProperty.call(e, "BuyCnt") && t.uint32(16).int32(e.BuyCnt), 
null != e.Advert && Object.hasOwnProperty.call(e, "Advert") && t.uint32(24).bool(e.Advert), 
null != e.Free && Object.hasOwnProperty.call(e, "Free") && t.uint32(32).bool(e.Free), 
t;
}, hn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, hn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopBoxArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.BuyCnt = e.int32();
break;

case 3:
n.Advert = e.bool();
break;

case 4:
n.Free = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, hn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, hn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.BuyCnt && e.hasOwnProperty("BuyCnt") && !$util.isInteger(e.BuyCnt) ? "BuyCnt: integer expected" : null != e.Advert && e.hasOwnProperty("Advert") && "boolean" != typeof e.Advert ? "Advert: boolean expected" : null != e.Free && e.hasOwnProperty("Free") && "boolean" != typeof e.Free ? "Free: boolean expected" : null;
}, hn), r.BuyShopBoxReply = (gn.prototype.Reward = null, gn.prototype.Box = null, 
gn.create = function(e) {
return new gn(e);
}, gn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Box && Object.hasOwnProperty.call(e, "Box") && $root.pb.ShopBox.encode(e.Box, t.uint32(18).fork()).ldelim(), 
t;
}, gn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, gn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopBoxReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Box = $root.pb.ShopBox.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, gn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, gn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Box && e.hasOwnProperty("Box") && (t = $root.pb.ShopBox.verify(e.Box)) ? "Box." + t : null;
var t;
}, gn), r.ShopBoxList = (bn.prototype.Boxs = $util.emptyArray, bn.create = function(e) {
return new bn(e);
}, bn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Boxs && e.Boxs.length) for (var r = 0; r < e.Boxs.length; ++r) $root.pb.ShopBox.encode(e.Boxs[r], t.uint32(10).fork()).ldelim();
return t;
}, bn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, bn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopBoxList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Boxs && n.Boxs.length || (n.Boxs = []), n.Boxs.push($root.pb.ShopBox.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, bn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, bn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Boxs && e.hasOwnProperty("Boxs")) {
if (!Array.isArray(e.Boxs)) return "Boxs: array expected";
for (var t = 0; t < e.Boxs.length; ++t) {
var r = $root.pb.ShopBox.verify(e.Boxs[t]);
if (r) return "Boxs." + r;
}
}
return null;
}, bn), r.ShopRes = (mn.prototype.GoodsId = "", mn.prototype.Resource = "", mn.prototype.Price = "", 
mn.prototype.RemainCnt = 0, mn.prototype.Cooldown = 0, mn.create = function(e) {
return new mn(e);
}, mn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Resource && Object.hasOwnProperty.call(e, "Resource") && t.uint32(18).string(e.Resource), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(26).string(e.Price), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(32).int32(e.RemainCnt), 
null != e.Cooldown && Object.hasOwnProperty.call(e, "Cooldown") && t.uint32(40).int32(e.Cooldown), 
t;
}, mn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, mn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopRes(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Resource = e.string();
break;

case 3:
n.Price = e.string();
break;

case 4:
n.RemainCnt = e.int32();
break;

case 5:
n.Cooldown = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, mn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, mn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.Resource && e.hasOwnProperty("Resource") && !$util.isString(e.Resource) ? "Resource: string expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price) ? "Price: string expected" : null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt) ? "RemainCnt: integer expected" : null != e.Cooldown && e.hasOwnProperty("Cooldown") && !$util.isInteger(e.Cooldown) ? "Cooldown: integer expected" : null;
}, mn), r.ShopResList = (wn.prototype.Resources = $util.emptyArray, wn.create = function(e) {
return new wn(e);
}, wn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Resources && e.Resources.length) for (var r = 0; r < e.Resources.length; ++r) $root.pb.ShopRes.encode(e.Resources[r], t.uint32(18).fork()).ldelim();
return t;
}, wn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, wn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopResList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 2 ? (n.Resources && n.Resources.length || (n.Resources = []), n.Resources.push($root.pb.ShopRes.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, wn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, wn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Resources && e.hasOwnProperty("Resources")) {
if (!Array.isArray(e.Resources)) return "Resources: array expected";
for (var t = 0; t < e.Resources.length; ++t) {
var r = $root.pb.ShopRes.verify(e.Resources[t]);
if (r) return "Resources." + r;
}
}
return null;
}, wn), r.BuyShopResArg = (vn.prototype.GoodsId = "", vn.prototype.BuyCnt = 0, vn.create = function(e) {
return new vn(e);
}, vn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.BuyCnt && Object.hasOwnProperty.call(e, "BuyCnt") && t.uint32(16).int32(e.BuyCnt), 
t;
}, vn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, vn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopResArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.BuyCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, vn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, vn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.BuyCnt && e.hasOwnProperty("BuyCnt") && !$util.isInteger(e.BuyCnt) ? "BuyCnt: integer expected" : null;
}, vn), r.BuyShopResReply = (On.prototype.Reward = null, On.prototype.Resource = null, 
On.create = function(e) {
return new On(e);
}, On.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Resource && Object.hasOwnProperty.call(e, "Resource") && $root.pb.ShopRes.encode(e.Resource, t.uint32(18).fork()).ldelim(), 
t;
}, On.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, On.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopResReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Resource = $root.pb.ShopRes.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, On.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, On.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Resource && e.hasOwnProperty("Resource") && (t = $root.pb.ShopRes.verify(e.Resource)) ? "Resource." + t : null;
var t;
}, On), r.ShopJade = ($n.prototype.GoodsId = "", $n.prototype.Jade = 0, $n.prototype.IsDouble = !1, 
$n.prototype.Price = 0, $n.prototype.Extra = 0, $n.create = function(e) {
return new $n(e);
}, $n.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Jade && Object.hasOwnProperty.call(e, "Jade") && t.uint32(16).int32(e.Jade), 
null != e.IsDouble && Object.hasOwnProperty.call(e, "IsDouble") && t.uint32(24).bool(e.IsDouble), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(32).int32(e.Price), 
null != e.Extra && Object.hasOwnProperty.call(e, "Extra") && t.uint32(40).int32(e.Extra), 
t;
}, $n.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, $n.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopJade(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Jade = e.int32();
break;

case 3:
n.IsDouble = e.bool();
break;

case 4:
n.Price = e.int32();
break;

case 5:
n.Extra = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, $n.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, $n.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.Jade && e.hasOwnProperty("Jade") && !$util.isInteger(e.Jade) ? "Jade: integer expected" : null != e.IsDouble && e.hasOwnProperty("IsDouble") && "boolean" != typeof e.IsDouble ? "IsDouble: boolean expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isInteger(e.Price) ? "Price: integer expected" : null != e.Extra && e.hasOwnProperty("Extra") && !$util.isInteger(e.Extra) ? "Extra: integer expected" : null;
}, $n), r.BuyShopJadeArg = (Rn.prototype.GoodsId = "", Rn.create = function(e) {
return new Rn(e);
}, Rn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
t;
}, Rn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Rn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopJadeArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.GoodsId = e.string() : e.skipType(7 & i);
}
return n;
}, Rn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Rn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null;
}, Rn), r.BuyShopJadeReply = (In.prototype.Reward = null, In.prototype.Jade = null, 
In.create = function(e) {
return new In(e);
}, In.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Jade && Object.hasOwnProperty.call(e, "Jade") && $root.pb.ShopJade.encode(e.Jade, t.uint32(18).fork()).ldelim(), 
t;
}, In.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, In.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopJadeReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Jade = $root.pb.ShopJade.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, In.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, In.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Jade && e.hasOwnProperty("Jade") && (t = $root.pb.ShopJade.verify(e.Jade)) ? "Jade." + t : null;
var t;
}, In), r.ShopJadeList = (kn.prototype.Jades = $util.emptyArray, kn.create = function(e) {
return new kn(e);
}, kn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Jades && e.Jades.length) for (var r = 0; r < e.Jades.length; ++r) $root.pb.ShopJade.encode(e.Jades[r], t.uint32(10).fork()).ldelim();
return t;
}, kn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, kn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopJadeList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Jades && n.Jades.length || (n.Jades = []), n.Jades.push($root.pb.ShopJade.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, kn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, kn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Jades && e.hasOwnProperty("Jades")) {
if (!Array.isArray(e.Jades)) return "Jades: array expected";
for (var t = 0; t < e.Jades.length; ++t) {
var r = $root.pb.ShopJade.verify(e.Jades[t]);
if (r) return "Jades." + r;
}
}
return null;
}, kn), r.ShopCoin = (Pn.prototype.GoodsId = "", Pn.prototype.Price = 0, Pn.prototype.CoinCnt = 0, 
Pn.prototype.Desc = 0, Pn.prototype.Icon = "", Pn.create = function(e) {
return new Pn(e);
}, Pn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(16).int32(e.Price), 
null != e.CoinCnt && Object.hasOwnProperty.call(e, "CoinCnt") && t.uint32(24).int32(e.CoinCnt), 
null != e.Desc && Object.hasOwnProperty.call(e, "Desc") && t.uint32(32).int32(e.Desc), 
null != e.Icon && Object.hasOwnProperty.call(e, "Icon") && t.uint32(42).string(e.Icon), 
t;
}, Pn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Pn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopCoin(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Price = e.int32();
break;

case 3:
n.CoinCnt = e.int32();
break;

case 4:
n.Desc = e.int32();
break;

case 5:
n.Icon = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Pn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Pn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isInteger(e.Price) ? "Price: integer expected" : null != e.CoinCnt && e.hasOwnProperty("CoinCnt") && !$util.isInteger(e.CoinCnt) ? "CoinCnt: integer expected" : null != e.Desc && e.hasOwnProperty("Desc") && !$util.isInteger(e.Desc) ? "Desc: integer expected" : null != e.Icon && e.hasOwnProperty("Icon") && !$util.isString(e.Icon) ? "Icon: string expected" : null;
}, Pn), r.ShopCoinList = (_n.prototype.Coins = $util.emptyArray, _n.create = function(e) {
return new _n(e);
}, _n.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Coins && e.Coins.length) for (var r = 0; r < e.Coins.length; ++r) $root.pb.ShopCoin.encode(e.Coins[r], t.uint32(10).fork()).ldelim();
return t;
}, _n.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, _n.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopCoinList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Coins && n.Coins.length || (n.Coins = []), n.Coins.push($root.pb.ShopCoin.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, _n.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, _n.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Coins && e.hasOwnProperty("Coins")) {
if (!Array.isArray(e.Coins)) return "Coins: array expected";
for (var t = 0; t < e.Coins.length; ++t) {
var r = $root.pb.ShopCoin.verify(e.Coins[t]);
if (r) return "Coins." + r;
}
}
return null;
}, _n), r.ShopVip = (xn.prototype.VipId = "", xn.prototype.Price = "", xn.prototype.Resource = "", 
xn.prototype.Duration = 0, xn.create = function(e) {
return new xn(e);
}, xn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.VipId && Object.hasOwnProperty.call(e, "VipId") && t.uint32(10).string(e.VipId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(18).string(e.Price), 
null != e.Resource && Object.hasOwnProperty.call(e, "Resource") && t.uint32(26).string(e.Resource), 
null != e.Duration && Object.hasOwnProperty.call(e, "Duration") && t.uint32(32).int32(e.Duration), 
t;
}, xn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, xn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopVip(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.VipId = e.string();
break;

case 2:
n.Price = e.string();
break;

case 3:
n.Resource = e.string();
break;

case 4:
n.Duration = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, xn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, xn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.VipId && e.hasOwnProperty("VipId") && !$util.isString(e.VipId) ? "VipId: string expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price) ? "Price: string expected" : null != e.Resource && e.hasOwnProperty("Resource") && !$util.isString(e.Resource) ? "Resource: string expected" : null != e.Duration && e.hasOwnProperty("Duration") && !$util.isInteger(e.Duration) ? "Duration: integer expected" : null;
}, xn), r.ShopVipList = (An.prototype.Vips = $util.emptyArray, An.create = function(e) {
return new An(e);
}, An.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Vips && e.Vips.length) for (var r = 0; r < e.Vips.length; ++r) $root.pb.ShopVip.encode(e.Vips[r], t.uint32(10).fork()).ldelim();
return t;
}, An.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, An.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopVipList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Vips && n.Vips.length || (n.Vips = []), n.Vips.push($root.pb.ShopVip.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, An.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, An.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Vips && e.hasOwnProperty("Vips")) {
if (!Array.isArray(e.Vips)) return "Vips: array expected";
for (var t = 0; t < e.Vips.length; ++t) {
var r = $root.pb.ShopVip.verify(e.Vips[t]);
if (r) return "Vips." + r;
}
}
return null;
}, An), r.BuyVipArg = (jn.prototype.VipId = "", jn.create = function(e) {
return new jn(e);
}, jn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.VipId && Object.hasOwnProperty.call(e, "VipId") && t.uint32(10).string(e.VipId), 
t;
}, jn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, jn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyVipArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.VipId = e.string() : e.skipType(7 & i);
}
return n;
}, jn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, jn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.VipId && e.hasOwnProperty("VipId") && !$util.isString(e.VipId) ? "VipId: string expected" : null;
}, jn), r.ShopGift = (Sn.prototype.GoodsId = "", Sn.prototype.Price = "", Sn.prototype.RemainCnt = 0, 
Sn.prototype.CanBuy = !1, Sn.prototype.Rewards = $util.emptyArray, Sn.create = function(e) {
return new Sn(e);
}, Sn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(18).string(e.Price), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(24).int32(e.RemainCnt), 
null != e.CanBuy && Object.hasOwnProperty.call(e, "CanBuy") && t.uint32(32).bool(e.CanBuy), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(42).string(e.Rewards[r]);
return t;
}, Sn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Sn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopGift(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Price = e.string();
break;

case 3:
n.RemainCnt = e.int32();
break;

case 4:
n.CanBuy = e.bool();
break;

case 5:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Sn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Sn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId)) return "GoodsId: string expected";
if (null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price)) return "Price: string expected";
if (null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt)) return "RemainCnt: integer expected";
if (null != e.CanBuy && e.hasOwnProperty("CanBuy") && "boolean" != typeof e.CanBuy) return "CanBuy: boolean expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null;
}, Sn), r.ShopGiftList = (Tn.prototype.Gifts = $util.emptyArray, Tn.create = function(e) {
return new Tn(e);
}, Tn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gifts && e.Gifts.length) for (var r = 0; r < e.Gifts.length; ++r) $root.pb.ShopGift.encode(e.Gifts[r], t.uint32(10).fork()).ldelim();
return t;
}, Tn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Tn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopGiftList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Gifts && n.Gifts.length || (n.Gifts = []), n.Gifts.push($root.pb.ShopGift.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Tn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Tn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gifts && e.hasOwnProperty("Gifts")) {
if (!Array.isArray(e.Gifts)) return "Gifts: array expected";
for (var t = 0; t < e.Gifts.length; ++t) {
var r = $root.pb.ShopGift.verify(e.Gifts[t]);
if (r) return "Gifts." + r;
}
}
return null;
}, Tn), r.BuyShopGiftArg = (Cn.prototype.GoodsId = "", Cn.create = function(e) {
return new Cn(e);
}, Cn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
t;
}, Cn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Cn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopGiftArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.GoodsId = e.string() : e.skipType(7 & i);
}
return n;
}, Cn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Cn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null;
}, Cn), r.BuyShopGiftReply = (Ln.prototype.Reward = null, Ln.prototype.Gift = null, 
Ln.create = function(e) {
return new Ln(e);
}, Ln.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Gift && Object.hasOwnProperty.call(e, "Gift") && $root.pb.ShopGift.encode(e.Gift, t.uint32(18).fork()).ldelim(), 
t;
}, Ln.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Ln.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyShopGiftReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Gift = $root.pb.ShopGift.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Ln.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Ln.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Gift && e.hasOwnProperty("Gift") && (t = $root.pb.ShopGift.verify(e.Gift)) ? "Gift." + t : null;
var t;
}, Ln), r.ShopRandRes = (Dn.prototype.AdvertCnt = 0, Dn.prototype.Jade = 0, Dn.prototype.Goods = $util.emptyArray, 
Dn.create = function(e) {
return new Dn(e);
}, Dn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.AdvertCnt && Object.hasOwnProperty.call(e, "AdvertCnt") && t.uint32(8).int32(e.AdvertCnt), 
null != e.Jade && Object.hasOwnProperty.call(e, "Jade") && t.uint32(16).int32(e.Jade), 
null != e.Goods && e.Goods.length) for (var r = 0; r < e.Goods.length; ++r) $root.pb.RandRes.encode(e.Goods[r], t.uint32(26).fork()).ldelim();
return t;
}, Dn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Dn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ShopRandRes(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.AdvertCnt = e.int32();
break;

case 2:
n.Jade = e.int32();
break;

case 3:
n.Goods && n.Goods.length || (n.Goods = []), n.Goods.push($root.pb.RandRes.decode(e, e.uint32()));
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Dn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Dn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.AdvertCnt && e.hasOwnProperty("AdvertCnt") && !$util.isInteger(e.AdvertCnt)) return "AdvertCnt: integer expected";
if (null != e.Jade && e.hasOwnProperty("Jade") && !$util.isInteger(e.Jade)) return "Jade: integer expected";
if (null != e.Goods && e.hasOwnProperty("Goods")) {
if (!Array.isArray(e.Goods)) return "Goods: array expected";
for (var t = 0; t < e.Goods.length; ++t) {
var r = $root.pb.RandRes.verify(e.Goods[t]);
if (r) return "Goods." + r;
}
}
return null;
}, Dn), r.RandRes = (Gn.prototype.GoodsId = 0, Gn.prototype.Resource = "", Gn.prototype.Price = "", 
Gn.prototype.RemainCnt = 0, Gn.create = function(e) {
return new Gn(e);
}, Gn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(8).int32(e.GoodsId), 
null != e.Resource && Object.hasOwnProperty.call(e, "Resource") && t.uint32(18).string(e.Resource), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(26).string(e.Price), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(32).int32(e.RemainCnt), 
t;
}, Gn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Gn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RandRes(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.int32();
break;

case 2:
n.Resource = e.string();
break;

case 3:
n.Price = e.string();
break;

case 4:
n.RemainCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Gn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Gn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isInteger(e.GoodsId) ? "GoodsId: integer expected" : null != e.Resource && e.hasOwnProperty("Resource") && !$util.isString(e.Resource) ? "Resource: string expected" : null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price) ? "Price: string expected" : null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt) ? "RemainCnt: integer expected" : null;
}, Gn), r.BuyRandShopResArg = (En.prototype.GoodsId = 0, En.create = function(e) {
return new En(e);
}, En.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(8).int32(e.GoodsId), 
t;
}, En.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, En.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyRandShopResArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.GoodsId = e.int32() : e.skipType(7 & i);
}
return n;
}, En.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, En.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isInteger(e.GoodsId) ? "GoodsId: integer expected" : null;
}, En), r.BuyRandShopResReply = (Wn.prototype.Reward = null, Wn.prototype.RemainCnt = 0, 
Wn.create = function(e) {
return new Wn(e);
}, Wn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(16).int32(e.RemainCnt), 
t;
}, Wn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Wn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyRandShopResReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.RemainCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Wn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Wn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Reward && e.hasOwnProperty("Reward")) {
var t = $root.pb.RewardList.verify(e.Reward);
if (t) return "Reward." + t;
}
return null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt) ? "RemainCnt: integer expected" : null;
}, Wn), r.LevelGift = (Hn.prototype.GoodsId = "", Hn.prototype.Price = "", Hn.prototype.RemainCnt = 0, 
Hn.prototype.RemainTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, Hn.prototype.Rewards = $util.emptyArray, 
Hn.create = function(e) {
return new Hn(e);
}, Hn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
null != e.Price && Object.hasOwnProperty.call(e, "Price") && t.uint32(18).string(e.Price), 
null != e.RemainCnt && Object.hasOwnProperty.call(e, "RemainCnt") && t.uint32(24).int32(e.RemainCnt), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(32).int64(e.RemainTime), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(42).string(e.Rewards[r]);
return t;
}, Hn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Hn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelGift(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.GoodsId = e.string();
break;

case 2:
n.Price = e.string();
break;

case 3:
n.RemainCnt = e.int32();
break;

case 4:
n.RemainTime = e.int64();
break;

case 5:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Hn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Hn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId)) return "GoodsId: string expected";
if (null != e.Price && e.hasOwnProperty("Price") && !$util.isString(e.Price)) return "Price: string expected";
if (null != e.RemainCnt && e.hasOwnProperty("RemainCnt") && !$util.isInteger(e.RemainCnt)) return "RemainCnt: integer expected";
if (null != e.RemainTime && e.hasOwnProperty("RemainTime") && !($util.isInteger(e.RemainTime) || e.RemainTime && $util.isInteger(e.RemainTime.low) && $util.isInteger(e.RemainTime.high))) return "RemainTime: integer|Long expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null;
}, Hn), r.LevelGiftList = (Bn.prototype.Gifts = $util.emptyArray, Bn.create = function(e) {
return new Bn(e);
}, Bn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Gifts && e.Gifts.length) for (var r = 0; r < e.Gifts.length; ++r) $root.pb.LevelGift.encode(e.Gifts[r], t.uint32(10).fork()).ldelim();
return t;
}, Bn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Bn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.LevelGiftList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Gifts && n.Gifts.length || (n.Gifts = []), n.Gifts.push($root.pb.LevelGift.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Bn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Bn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Gifts && e.hasOwnProperty("Gifts")) {
if (!Array.isArray(e.Gifts)) return "Gifts: array expected";
for (var t = 0; t < e.Gifts.length; ++t) {
var r = $root.pb.LevelGift.verify(e.Gifts[t]);
if (r) return "Gifts." + r;
}
}
return null;
}, Bn), r.BuyLevelGiftArg = (Un.prototype.GoodsId = "", Un.create = function(e) {
return new Un(e);
}, Un.encode = function(e, t) {
return t = t || $Writer.create(), null != e.GoodsId && Object.hasOwnProperty.call(e, "GoodsId") && t.uint32(10).string(e.GoodsId), 
t;
}, Un.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Un.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyLevelGiftArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.GoodsId = e.string() : e.skipType(7 & i);
}
return n;
}, Un.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Un.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.GoodsId && e.hasOwnProperty("GoodsId") && !$util.isString(e.GoodsId) ? "GoodsId: string expected" : null;
}, Un), r.BuyLevelGiftReply = (Nn.prototype.Reward = null, Nn.prototype.Gift = null, 
Nn.create = function(e) {
return new Nn(e);
}, Nn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(10).fork()).ldelim(), 
null != e.Gift && Object.hasOwnProperty.call(e, "Gift") && $root.pb.LevelGift.encode(e.Gift, t.uint32(18).fork()).ldelim(), 
t;
}, Nn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Nn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BuyLevelGiftReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

case 2:
n.Gift = $root.pb.LevelGift.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Nn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Nn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Reward && e.hasOwnProperty("Reward") && (t = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + t : null != e.Gift && e.hasOwnProperty("Gift") && (t = $root.pb.LevelGift.verify(e.Gift)) ? "Gift." + t : null;
var t;
}, Nn), r.WorkShopInfo = (Fn.prototype.WorkShops = $util.emptyArray, Fn.create = function(e) {
return new Fn(e);
}, Fn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.WorkShops && e.WorkShops.length) for (var r = 0; r < e.WorkShops.length; ++r) $root.pb.WorkShop.encode(e.WorkShops[r], t.uint32(10).fork()).ldelim();
return t;
}, Fn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Fn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WorkShopInfo(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.WorkShops && n.WorkShops.length || (n.WorkShops = []), n.WorkShops.push($root.pb.WorkShop.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, Fn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Fn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.WorkShops && e.hasOwnProperty("WorkShops")) {
if (!Array.isArray(e.WorkShops)) return "WorkShops: array expected";
for (var t = 0; t < e.WorkShops.length; ++t) {
var r = $root.pb.WorkShop.verify(e.WorkShops[t]);
if (r) return "WorkShops." + r;
}
}
return null;
}, Fn), r.WorkShop = (Mn.prototype.Id = 0, Mn.prototype.Level = 0, Mn.prototype.isUplevel = !1, 
Mn.prototype.UplevelRemainTime = 0, Mn.prototype.RewardTotalTime = 0, Mn.prototype.RewardRemainTime = 0, 
Mn.prototype.Rewards = $util.emptyArray, Mn.prototype.RewardLevel = 0, Mn.create = function(e) {
return new Mn(e);
}, Mn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(16).int32(e.Level), 
null != e.isUplevel && Object.hasOwnProperty.call(e, "isUplevel") && t.uint32(24).bool(e.isUplevel), 
null != e.UplevelRemainTime && Object.hasOwnProperty.call(e, "UplevelRemainTime") && t.uint32(32).int32(e.UplevelRemainTime), 
null != e.RewardTotalTime && Object.hasOwnProperty.call(e, "RewardTotalTime") && t.uint32(40).int32(e.RewardTotalTime), 
null != e.RewardRemainTime && Object.hasOwnProperty.call(e, "RewardRemainTime") && t.uint32(48).int32(e.RewardRemainTime), 
null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) $root.pb.WorkShopReward.encode(e.Rewards[r], t.uint32(58).fork()).ldelim();
return null != e.RewardLevel && Object.hasOwnProperty.call(e, "RewardLevel") && t.uint32(64).int32(e.RewardLevel), 
t;
}, Mn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Mn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WorkShop(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Id = e.int32();
break;

case 2:
n.Level = e.int32();
break;

case 3:
n.isUplevel = e.bool();
break;

case 4:
n.UplevelRemainTime = e.int32();
break;

case 5:
n.RewardTotalTime = e.int32();
break;

case 6:
n.RewardRemainTime = e.int32();
break;

case 7:
n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push($root.pb.WorkShopReward.decode(e, e.uint32()));
break;

case 8:
n.RewardLevel = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Mn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Mn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id)) return "Id: integer expected";
if (null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level)) return "Level: integer expected";
if (null != e.isUplevel && e.hasOwnProperty("isUplevel") && "boolean" != typeof e.isUplevel) return "isUplevel: boolean expected";
if (null != e.UplevelRemainTime && e.hasOwnProperty("UplevelRemainTime") && !$util.isInteger(e.UplevelRemainTime)) return "UplevelRemainTime: integer expected";
if (null != e.RewardTotalTime && e.hasOwnProperty("RewardTotalTime") && !$util.isInteger(e.RewardTotalTime)) return "RewardTotalTime: integer expected";
if (null != e.RewardRemainTime && e.hasOwnProperty("RewardRemainTime") && !$util.isInteger(e.RewardRemainTime)) return "RewardRemainTime: integer expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) {
var r = $root.pb.WorkShopReward.verify(e.Rewards[t]);
if (r) return "Rewards." + r;
}
}
return null != e.RewardLevel && e.hasOwnProperty("RewardLevel") && !$util.isInteger(e.RewardLevel) ? "RewardLevel: integer expected" : null;
}, Mn), r.WorkShopReward = (Vn.prototype.Rewards = $util.emptyArray, Vn.create = function(e) {
return new Vn(e);
}, Vn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Rewards && e.Rewards.length) for (var r = 0; r < e.Rewards.length; ++r) t.uint32(10).string(e.Rewards[r]);
return t;
}, Vn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Vn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.WorkShopReward(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Rewards && n.Rewards.length || (n.Rewards = []), n.Rewards.push(e.string())) : e.skipType(7 & i);
}
return n;
}, Vn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Vn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Rewards && e.hasOwnProperty("Rewards")) {
if (!Array.isArray(e.Rewards)) return "Rewards: array expected";
for (var t = 0; t < e.Rewards.length; ++t) if (!$util.isString(e.Rewards[t])) return "Rewards: string[] expected";
}
return null;
}, Vn), r.TargetWorkShop = (qn.prototype.Id = 0, qn.create = function(e) {
return new qn(e);
}, qn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(8).int32(e.Id), 
t;
}, qn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, qn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TargetWorkShop(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Id = e.int32() : e.skipType(7 & i);
}
return n;
}, qn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, qn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isInteger(e.Id) ? "Id: integer expected" : null;
}, qn), r.RankActivityPhase = (e = {}, (t = Object.create(e))[e[0] = "RankActivityUnknown"] = 0, 
t[e[1] = "RankActivityNotOpen"] = 1, t[e[2] = "RankActivityActive"] = 2, t[e[3] = "RankActivityPublicity"] = 3, 
t[e[4] = "RankActivityClaimable"] = 4, t), r.RankActivityData = (Jn.prototype.Phase = 0, 
Jn.prototype.RemainTime = 0, Jn.prototype.RankInfo = null, Jn.prototype.RewardStatus = 0, 
Jn.prototype.RewardPreview = $util.emptyArray, Jn.prototype.LevelRewardStatus = 0, 
Jn.prototype.ReceiveRewardTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
Jn.create = function(e) {
return new Jn(e);
}, Jn.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Phase && Object.hasOwnProperty.call(e, "Phase") && t.uint32(8).int32(e.Phase), 
null != e.RemainTime && Object.hasOwnProperty.call(e, "RemainTime") && t.uint32(16).int32(e.RemainTime), 
null != e.RankInfo && Object.hasOwnProperty.call(e, "RankInfo") && $root.pb.LevelRankInfo.encode(e.RankInfo, t.uint32(26).fork()).ldelim(), 
null != e.RewardStatus && Object.hasOwnProperty.call(e, "RewardStatus") && t.uint32(32).int32(e.RewardStatus), 
null != e.RewardPreview && e.RewardPreview.length) for (var r = 0; r < e.RewardPreview.length; ++r) t.uint32(42).string(e.RewardPreview[r]);
return null != e.LevelRewardStatus && Object.hasOwnProperty.call(e, "LevelRewardStatus") && t.uint32(48).int32(e.LevelRewardStatus), 
null != e.ReceiveRewardTime && Object.hasOwnProperty.call(e, "ReceiveRewardTime") && t.uint32(56).int64(e.ReceiveRewardTime), 
t;
}, Jn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Jn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RankActivityData(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Phase = e.int32();
break;

case 2:
n.RemainTime = e.int32();
break;

case 3:
n.RankInfo = $root.pb.LevelRankInfo.decode(e, e.uint32());
break;

case 4:
n.RewardStatus = e.int32();
break;

case 5:
n.RewardPreview && n.RewardPreview.length || (n.RewardPreview = []), n.RewardPreview.push(e.string());
break;

case 6:
n.LevelRewardStatus = e.int32();
break;

case 7:
n.ReceiveRewardTime = e.int64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Jn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Jn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Phase && e.hasOwnProperty("Phase")) switch (e.Phase) {
default:
return "Phase: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
if (null != e.RemainTime && e.hasOwnProperty("RemainTime") && !$util.isInteger(e.RemainTime)) return "RemainTime: integer expected";
if (null != e.RankInfo && e.hasOwnProperty("RankInfo")) {
var t = $root.pb.LevelRankInfo.verify(e.RankInfo);
if (t) return "RankInfo." + t;
}
if (null != e.RewardStatus && e.hasOwnProperty("RewardStatus")) switch (e.RewardStatus) {
default:
return "RewardStatus: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
if (null != e.RewardPreview && e.hasOwnProperty("RewardPreview")) {
if (!Array.isArray(e.RewardPreview)) return "RewardPreview: array expected";
for (var r = 0; r < e.RewardPreview.length; ++r) if (!$util.isString(e.RewardPreview[r])) return "RewardPreview: string[] expected";
}
if (null != e.LevelRewardStatus && e.hasOwnProperty("LevelRewardStatus")) switch (e.LevelRewardStatus) {
default:
return "LevelRewardStatus: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
return null != e.ReceiveRewardTime && e.hasOwnProperty("ReceiveRewardTime") && !($util.isInteger(e.ReceiveRewardTime) || e.ReceiveRewardTime && $util.isInteger(e.ReceiveRewardTime.low) && $util.isInteger(e.ReceiveRewardTime.high)) ? "ReceiveRewardTime: integer|Long expected" : null;
}, Jn), r.GetRankActivityRewardArg = (Xn.prototype.IsLevel = !1, Xn.create = function(e) {
return new Xn(e);
}, Xn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.IsLevel && Object.hasOwnProperty.call(e, "IsLevel") && t.uint32(8).bool(e.IsLevel), 
t;
}, Xn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Xn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetRankActivityRewardArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.IsLevel = e.bool() : e.skipType(7 & i);
}
return n;
}, Xn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Xn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.IsLevel && e.hasOwnProperty("IsLevel") && "boolean" != typeof e.IsLevel ? "IsLevel: boolean expected" : null;
}, Xn), r.GetRankActivityRewardReply = (Yn.prototype.RewardStatus = 0, Yn.prototype.Reward = null, 
Yn.create = function(e) {
return new Yn(e);
}, Yn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RewardStatus && Object.hasOwnProperty.call(e, "RewardStatus") && t.uint32(8).int32(e.RewardStatus), 
null != e.Reward && Object.hasOwnProperty.call(e, "Reward") && $root.pb.RewardList.encode(e.Reward, t.uint32(18).fork()).ldelim(), 
t;
}, Yn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Yn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.GetRankActivityRewardReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.RewardStatus = e.int32();
break;

case 2:
n.Reward = $root.pb.RewardList.decode(e, e.uint32());
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Yn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Yn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.RewardStatus && e.hasOwnProperty("RewardStatus")) switch (e.RewardStatus) {
default:
return "RewardStatus: enum value expected";

case 0:
case 1:
case 2:
case 3:
case 4:
}
return null != e.Reward && e.hasOwnProperty("Reward") && (e = $root.pb.RewardList.verify(e.Reward)) ? "Reward." + e : null;
}, Yn), r.BroadcastRadio = (zn.prototype.Type = 0, zn.prototype.Arg = $util.newBuffer([]), 
zn.create = function(e) {
return new zn(e);
}, zn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(8).int32(e.Type), 
null != e.Arg && Object.hasOwnProperty.call(e, "Arg") && t.uint32(18).bytes(e.Arg), 
t;
}, zn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, zn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.BroadcastRadio(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Type = e.int32();
break;

case 2:
n.Arg = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, zn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, zn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Type && e.hasOwnProperty("Type") && !$util.isInteger(e.Type) ? "Type: integer expected" : null != e.Arg && e.hasOwnProperty("Arg") && !(e.Arg && "number" == typeof e.Arg.length || $util.isString(e.Arg)) ? "Arg: buffer expected" : null;
}, zn), r.RadioPlayer = (Kn.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
Kn.prototype.Name = "", Kn.create = function(e) {
return new Kn(e);
}, Kn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
t;
}, Kn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Kn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioPlayer(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Kn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Kn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null;
}, Kn), r.RadioCustom = (Qn.prototype.Content = "", Qn.create = function(e) {
return new Qn(e);
}, Qn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Content && Object.hasOwnProperty.call(e, "Content") && t.uint32(10).string(e.Content), 
t;
}, Qn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Qn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioCustom(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Content = e.string() : e.skipType(7 & i);
}
return n;
}, Qn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Qn.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Content && e.hasOwnProperty("Content") && !$util.isString(e.Content) ? "Content: string expected" : null;
}, Qn), r.RadioLevelEndGet = (Zn.prototype.Player = null, Zn.prototype.Type = "", 
Zn.prototype.Id = "", Zn.prototype.Level = 0, Zn.prototype.IsSpeedPass = !1, Zn.prototype.IsClear = !1, 
Zn.create = function(e) {
return new Zn(e);
}, Zn.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.RadioPlayer.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(18).string(e.Type), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(26).string(e.Id), 
null != e.Level && Object.hasOwnProperty.call(e, "Level") && t.uint32(32).int32(e.Level), 
null != e.IsSpeedPass && Object.hasOwnProperty.call(e, "IsSpeedPass") && t.uint32(40).bool(e.IsSpeedPass), 
null != e.IsClear && Object.hasOwnProperty.call(e, "IsClear") && t.uint32(48).bool(e.IsClear), 
t;
}, Zn.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, Zn.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioLevelEndGet(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.RadioPlayer.decode(e, e.uint32());
break;

case 2:
n.Type = e.string();
break;

case 3:
n.Id = e.string();
break;

case 4:
n.Level = e.int32();
break;

case 5:
n.IsSpeedPass = e.bool();
break;

case 6:
n.IsClear = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, Zn.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, Zn.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.RadioPlayer.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.Type && e.hasOwnProperty("Type") && !$util.isString(e.Type) ? "Type: string expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null != e.Level && e.hasOwnProperty("Level") && !$util.isInteger(e.Level) ? "Level: integer expected" : null != e.IsSpeedPass && e.hasOwnProperty("IsSpeedPass") && "boolean" != typeof e.IsSpeedPass ? "IsSpeedPass: boolean expected" : null != e.IsClear && e.hasOwnProperty("IsClear") && "boolean" != typeof e.IsClear ? "IsClear: boolean expected" : null;
}, Zn), r.RadioShopBoxGetGem = (ei.prototype.Player = null, ei.prototype.Id = "", 
ei.prototype.BoxId = "", ei.create = function(e) {
return new ei(e);
}, ei.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.RadioPlayer.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(18).string(e.Id), 
null != e.BoxId && Object.hasOwnProperty.call(e, "BoxId") && t.uint32(26).string(e.BoxId), 
t;
}, ei.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ei.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioShopBoxGetGem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.RadioPlayer.decode(e, e.uint32());
break;

case 2:
n.Id = e.string();
break;

case 3:
n.BoxId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ei.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ei.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.RadioPlayer.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null != e.BoxId && e.hasOwnProperty("BoxId") && !$util.isString(e.BoxId) ? "BoxId: string expected" : null;
}, ei), r.RadioCompose = (ti.prototype.Player = null, ti.prototype.Type = "", ti.prototype.Id = "", 
ti.create = function(e) {
return new ti(e);
}, ti.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.RadioPlayer.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(18).string(e.Type), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(26).string(e.Id), 
t;
}, ti.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ti.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioCompose(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.RadioPlayer.decode(e, e.uint32());
break;

case 2:
n.Type = e.string();
break;

case 3:
n.Id = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ti.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ti.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.RadioPlayer.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.Type && e.hasOwnProperty("Type") && !$util.isString(e.Type) ? "Type: string expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null;
}, ti), r.RadioWorkshop = (ri.prototype.Player = null, ri.prototype.Type = "", ri.prototype.Id = "", 
ri.create = function(e) {
return new ri(e);
}, ri.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.RadioPlayer.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.Type && Object.hasOwnProperty.call(e, "Type") && t.uint32(18).string(e.Type), 
null != e.Id && Object.hasOwnProperty.call(e, "Id") && t.uint32(26).string(e.Id), 
t;
}, ri.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ri.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.RadioWorkshop(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.RadioPlayer.decode(e, e.uint32());
break;

case 2:
n.Type = e.string();
break;

case 3:
n.Id = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ri.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ri.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.RadioPlayer.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.Type && e.hasOwnProperty("Type") && !$util.isString(e.Type) ? "Type: string expected" : null != e.Id && e.hasOwnProperty("Id") && !$util.isString(e.Id) ? "Id: string expected" : null;
}, ri), r.SendSmsCodeArg = (ni.prototype.Phone = "", ni.create = function(e) {
return new ni(e);
}, ni.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Phone && Object.hasOwnProperty.call(e, "Phone") && t.uint32(10).string(e.Phone), 
t;
}, ni.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ni.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SendSmsCodeArg(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Phone = e.string() : e.skipType(7 & i);
}
return n;
}, ni.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ni.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Phone && e.hasOwnProperty("Phone") && !$util.isString(e.Phone) ? "Phone: string expected" : null;
}, ni), r.SendSmsCodeReply = (ii.prototype.RetryAfterSeconds = 0, ii.create = function(e) {
return new ii(e);
}, ii.encode = function(e, t) {
return t = t || $Writer.create(), null != e.RetryAfterSeconds && Object.hasOwnProperty.call(e, "RetryAfterSeconds") && t.uint32(8).int32(e.RetryAfterSeconds), 
t;
}, ii.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ii.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SendSmsCodeReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.RetryAfterSeconds = e.int32() : e.skipType(7 & i);
}
return n;
}, ii.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ii.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.RetryAfterSeconds && e.hasOwnProperty("RetryAfterSeconds") && !$util.isInteger(e.RetryAfterSeconds) ? "RetryAfterSeconds: integer expected" : null;
}, ii), r.SmsLoginArg = (oi.prototype.Phone = "", oi.prototype.Code = "", oi.prototype.DeviceId = "", 
oi.create = function(e) {
return new oi(e);
}, oi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Phone && Object.hasOwnProperty.call(e, "Phone") && t.uint32(10).string(e.Phone), 
null != e.Code && Object.hasOwnProperty.call(e, "Code") && t.uint32(18).string(e.Code), 
null != e.DeviceId && Object.hasOwnProperty.call(e, "DeviceId") && t.uint32(42).string(e.DeviceId), 
t;
}, oi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, oi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SmsLoginArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Phone = e.string();
break;

case 2:
n.Code = e.string();
break;

case 5:
n.DeviceId = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, oi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, oi.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Phone && e.hasOwnProperty("Phone") && !$util.isString(e.Phone) ? "Phone: string expected" : null != e.Code && e.hasOwnProperty("Code") && !$util.isString(e.Code) ? "Code: string expected" : null != e.DeviceId && e.hasOwnProperty("DeviceId") && !$util.isString(e.DeviceId) ? "DeviceId: string expected" : null;
}, oi), r.SmsLoginReply = (ai.prototype.SmsToken = "", ai.prototype.SmsTokenExpiresAt = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
ai.create = function(e) {
return new ai(e);
}, ai.encode = function(e, t) {
return t = t || $Writer.create(), null != e.SmsToken && Object.hasOwnProperty.call(e, "SmsToken") && t.uint32(10).string(e.SmsToken), 
null != e.SmsTokenExpiresAt && Object.hasOwnProperty.call(e, "SmsTokenExpiresAt") && t.uint32(16).int64(e.SmsTokenExpiresAt), 
t;
}, ai.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ai.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SmsLoginReply(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.SmsToken = e.string();
break;

case 2:
n.SmsTokenExpiresAt = e.int64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ai.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ai.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.SmsToken && e.hasOwnProperty("SmsToken") && !$util.isString(e.SmsToken) ? "SmsToken: string expected" : null != e.SmsTokenExpiresAt && e.hasOwnProperty("SmsTokenExpiresAt") && !($util.isInteger(e.SmsTokenExpiresAt) || e.SmsTokenExpiresAt && $util.isInteger(e.SmsTokenExpiresAt.low) && $util.isInteger(e.SmsTokenExpiresAt.high)) ? "SmsTokenExpiresAt: integer|Long expected" : null;
}, ai), r.ChatList = (li.prototype.Items = $util.emptyArray, li.create = function(e) {
return new li(e);
}, li.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Items && e.Items.length) for (var r = 0; r < e.Items.length; ++r) $root.pb.ChatItem.encode(e.Items[r], t.uint32(10).fork()).ldelim();
return t;
}, li.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, li.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ChatList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Items && n.Items.length || (n.Items = []), n.Items.push($root.pb.ChatItem.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, li.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, li.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Items && e.hasOwnProperty("Items")) {
if (!Array.isArray(e.Items)) return "Items: array expected";
for (var t = 0; t < e.Items.length; ++t) {
var r = $root.pb.ChatItem.verify(e.Items[t]);
if (r) return "Items." + r;
}
}
return null;
}, li), r.ChatItem = (si.prototype.Player = null, si.prototype.Msg = "", si.prototype.Time = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
si.prototype.IsStructured = !1, si.prototype.EncodedMsg = $util.newBuffer([]), si.create = function(e) {
return new si(e);
}, si.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.PlayerBaseInfo.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.Msg && Object.hasOwnProperty.call(e, "Msg") && t.uint32(18).string(e.Msg), 
null != e.Time && Object.hasOwnProperty.call(e, "Time") && t.uint32(24).int64(e.Time), 
null != e.IsStructured && Object.hasOwnProperty.call(e, "IsStructured") && t.uint32(32).bool(e.IsStructured), 
null != e.EncodedMsg && Object.hasOwnProperty.call(e, "EncodedMsg") && t.uint32(42).bytes(e.EncodedMsg), 
t;
}, si.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, si.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ChatItem(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.PlayerBaseInfo.decode(e, e.uint32());
break;

case 2:
n.Msg = e.string();
break;

case 3:
n.Time = e.int64();
break;

case 4:
n.IsStructured = e.bool();
break;

case 5:
n.EncodedMsg = e.bytes();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, si.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, si.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.PlayerBaseInfo.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.Msg && e.hasOwnProperty("Msg") && !$util.isString(e.Msg) ? "Msg: string expected" : null != e.Time && e.hasOwnProperty("Time") && !($util.isInteger(e.Time) || e.Time && $util.isInteger(e.Time.low) && $util.isInteger(e.Time.high)) ? "Time: integer|Long expected" : null != e.IsStructured && e.hasOwnProperty("IsStructured") && "boolean" != typeof e.IsStructured ? "IsStructured: boolean expected" : null != e.EncodedMsg && e.hasOwnProperty("EncodedMsg") && !(e.EncodedMsg && "number" == typeof e.EncodedMsg.length || $util.isString(e.EncodedMsg)) ? "EncodedMsg: buffer expected" : null;
}, si), r.SendWorldChatArg = (ui.prototype.Msg = "", ui.prototype.IsStructured = !1, 
ui.create = function(e) {
return new ui(e);
}, ui.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Msg && Object.hasOwnProperty.call(e, "Msg") && t.uint32(10).string(e.Msg), 
null != e.IsStructured && Object.hasOwnProperty.call(e, "IsStructured") && t.uint32(16).bool(e.IsStructured), 
t;
}, ui.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ui.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SendWorldChatArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Msg = e.string();
break;

case 2:
n.IsStructured = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ui.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ui.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Msg && e.hasOwnProperty("Msg") && !$util.isString(e.Msg) ? "Msg: string expected" : null != e.IsStructured && e.hasOwnProperty("IsStructured") && "boolean" != typeof e.IsStructured ? "IsStructured: boolean expected" : null;
}, ui), r.PlayerBaseInfo = (ci.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
ci.prototype.Name = "", ci.prototype.Head = "", ci.create = function(e) {
return new ci(e);
}, ci.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.Name && Object.hasOwnProperty.call(e, "Name") && t.uint32(18).string(e.Name), 
null != e.Head && Object.hasOwnProperty.call(e, "Head") && t.uint32(26).string(e.Head), 
t;
}, ci.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, ci.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.PlayerBaseInfo(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.Name = e.string();
break;

case 3:
n.Head = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, ci.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, ci.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.Name && e.hasOwnProperty("Name") && !$util.isString(e.Name) ? "Name: string expected" : null != e.Head && e.hasOwnProperty("Head") && !$util.isString(e.Head) ? "Head: string expected" : null;
}, ci), r.TranslateTextArg = (di.prototype.Text = "", di.prototype.Lang = "", di.create = function(e) {
return new di(e);
}, di.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Text && Object.hasOwnProperty.call(e, "Text") && t.uint32(10).string(e.Text), 
null != e.Lang && Object.hasOwnProperty.call(e, "Lang") && t.uint32(18).string(e.Lang), 
t;
}, di.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, di.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TranslateTextArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Text = e.string();
break;

case 2:
n.Lang = e.string();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, di.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, di.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Text && e.hasOwnProperty("Text") && !$util.isString(e.Text) ? "Text: string expected" : null != e.Lang && e.hasOwnProperty("Lang") && !$util.isString(e.Lang) ? "Lang: string expected" : null;
}, di), r.TranslateTextReply = (pi.prototype.Text = "", pi.create = function(e) {
return new pi(e);
}, pi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Text && Object.hasOwnProperty.call(e, "Text") && t.uint32(10).string(e.Text), 
t;
}, pi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, pi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.TranslateTextReply(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? n.Text = e.string() : e.skipType(7 & i);
}
return n;
}, pi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, pi.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Text && e.hasOwnProperty("Text") && !$util.isString(e.Text) ? "Text: string expected" : null;
}, pi), r.FriendList = (fi.prototype.Friends = $util.emptyArray, fi.create = function(e) {
return new fi(e);
}, fi.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.Friends && e.Friends.length) for (var r = 0; r < e.Friends.length; ++r) $root.pb.Friend.encode(e.Friends[r], t.uint32(10).fork()).ldelim();
return t;
}, fi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, fi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FriendList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.Friends && n.Friends.length || (n.Friends = []), n.Friends.push($root.pb.Friend.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, fi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, fi.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Friends && e.hasOwnProperty("Friends")) {
if (!Array.isArray(e.Friends)) return "Friends: array expected";
for (var t = 0; t < e.Friends.length; ++t) {
var r = $root.pb.Friend.verify(e.Friends[t]);
if (r) return "Friends." + r;
}
}
return null;
}, fi), r.Friend = (yi.prototype.Player = null, yi.prototype.IsOnline = !1, yi.prototype.LastOnlineTime = $util.Long ? $util.Long.fromBits(0, 0, !1) : 0, 
yi.prototype.UnReadChatCnt = 0, yi.create = function(e) {
return new yi(e);
}, yi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Player && Object.hasOwnProperty.call(e, "Player") && $root.pb.PlayerBaseInfo.encode(e.Player, t.uint32(10).fork()).ldelim(), 
null != e.IsOnline && Object.hasOwnProperty.call(e, "IsOnline") && t.uint32(16).bool(e.IsOnline), 
null != e.LastOnlineTime && Object.hasOwnProperty.call(e, "LastOnlineTime") && t.uint32(24).int64(e.LastOnlineTime), 
null != e.UnReadChatCnt && Object.hasOwnProperty.call(e, "UnReadChatCnt") && t.uint32(32).int32(e.UnReadChatCnt), 
t;
}, yi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, yi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.Friend(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Player = $root.pb.PlayerBaseInfo.decode(e, e.uint32());
break;

case 2:
n.IsOnline = e.bool();
break;

case 3:
n.LastOnlineTime = e.int64();
break;

case 4:
n.UnReadChatCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, yi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, yi.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Player && e.hasOwnProperty("Player")) {
var t = $root.pb.PlayerBaseInfo.verify(e.Player);
if (t) return "Player." + t;
}
return null != e.IsOnline && e.hasOwnProperty("IsOnline") && "boolean" != typeof e.IsOnline ? "IsOnline: boolean expected" : null != e.LastOnlineTime && e.hasOwnProperty("LastOnlineTime") && !($util.isInteger(e.LastOnlineTime) || e.LastOnlineTime && $util.isInteger(e.LastOnlineTime.low) && $util.isInteger(e.LastOnlineTime.high)) ? "LastOnlineTime: integer|Long expected" : null != e.UnReadChatCnt && e.hasOwnProperty("UnReadChatCnt") && !$util.isInteger(e.UnReadChatCnt) ? "UnReadChatCnt: integer expected" : null;
}, yi), r.FriendApplyList = (hi.prototype.FriendApplys = $util.emptyArray, hi.create = function(e) {
return new hi(e);
}, hi.encode = function(e, t) {
if (t = t || $Writer.create(), null != e.FriendApplys && e.FriendApplys.length) for (var r = 0; r < e.FriendApplys.length; ++r) $root.pb.PlayerBaseInfo.encode(e.FriendApplys[r], t.uint32(10).fork()).ldelim();
return t;
}, hi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, hi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.FriendApplyList(); e.pos < r; ) {
var i = e.uint32();
i >>> 3 == 1 ? (n.FriendApplys && n.FriendApplys.length || (n.FriendApplys = []), 
n.FriendApplys.push($root.pb.PlayerBaseInfo.decode(e, e.uint32()))) : e.skipType(7 & i);
}
return n;
}, hi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, hi.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.FriendApplys && e.hasOwnProperty("FriendApplys")) {
if (!Array.isArray(e.FriendApplys)) return "FriendApplys: array expected";
for (var t = 0; t < e.FriendApplys.length; ++t) {
var r = $root.pb.PlayerBaseInfo.verify(e.FriendApplys[t]);
if (r) return "FriendApplys." + r;
}
}
return null;
}, hi), r.ReplyFriendApplyArg = (gi.prototype.Uid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, 
gi.prototype.IsAgree = !1, gi.create = function(e) {
return new gi(e);
}, gi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Uid && Object.hasOwnProperty.call(e, "Uid") && t.uint32(8).uint64(e.Uid), 
null != e.IsAgree && Object.hasOwnProperty.call(e, "IsAgree") && t.uint32(16).bool(e.IsAgree), 
t;
}, gi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, gi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.ReplyFriendApplyArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Uid = e.uint64();
break;

case 2:
n.IsAgree = e.bool();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, gi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, gi.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Uid && e.hasOwnProperty("Uid") && !($util.isInteger(e.Uid) || e.Uid && $util.isInteger(e.Uid.low) && $util.isInteger(e.Uid.high)) ? "Uid: integer|Long expected" : null != e.IsAgree && e.hasOwnProperty("IsAgree") && "boolean" != typeof e.IsAgree ? "IsAgree: boolean expected" : null;
}, gi), r.SendPrivateChatArg = (bi.prototype.Msg = "", bi.prototype.IsStructured = !1, 
bi.prototype.ToUid = $util.Long ? $util.Long.fromBits(0, 0, !0) : 0, bi.create = function(e) {
return new bi(e);
}, bi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Msg && Object.hasOwnProperty.call(e, "Msg") && t.uint32(10).string(e.Msg), 
null != e.IsStructured && Object.hasOwnProperty.call(e, "IsStructured") && t.uint32(16).bool(e.IsStructured), 
null != e.ToUid && Object.hasOwnProperty.call(e, "ToUid") && t.uint32(24).uint64(e.ToUid), 
t;
}, bi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, bi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.SendPrivateChatArg(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Msg = e.string();
break;

case 2:
n.IsStructured = e.bool();
break;

case 3:
n.ToUid = e.uint64();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, bi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, bi.verify = function(e) {
return "object" != typeof e || null === e ? "object expected" : null != e.Msg && e.hasOwnProperty("Msg") && !$util.isString(e.Msg) ? "Msg: string expected" : null != e.IsStructured && e.hasOwnProperty("IsStructured") && "boolean" != typeof e.IsStructured ? "IsStructured: boolean expected" : null != e.ToUid && e.hasOwnProperty("ToUid") && !($util.isInteger(e.ToUid) || e.ToUid && $util.isInteger(e.ToUid.low) && $util.isInteger(e.ToUid.high)) ? "ToUid: integer|Long expected" : null;
}, bi), r.PrivateChatNotify = (mi.prototype.Chat = null, mi.prototype.UnReadCnt = 0, 
mi.create = function(e) {
return new mi(e);
}, mi.encode = function(e, t) {
return t = t || $Writer.create(), null != e.Chat && Object.hasOwnProperty.call(e, "Chat") && $root.pb.ChatItem.encode(e.Chat, t.uint32(10).fork()).ldelim(), 
null != e.UnReadCnt && Object.hasOwnProperty.call(e, "UnReadCnt") && t.uint32(16).int32(e.UnReadCnt), 
t;
}, mi.encodeDelimited = function(e, t) {
return this.encode(e, t).ldelim();
}, mi.decode = function(e, t) {
e instanceof $Reader || (e = $Reader.create(e));
for (var r = void 0 === t ? e.len : e.pos + t, n = new $root.pb.PrivateChatNotify(); e.pos < r; ) {
var i = e.uint32();
switch (i >>> 3) {
case 1:
n.Chat = $root.pb.ChatItem.decode(e, e.uint32());
break;

case 2:
n.UnReadCnt = e.int32();
break;

default:
e.skipType(7 & i);
}
}
return n;
}, mi.decodeDelimited = function(e) {
return e instanceof $Reader || (e = new $Reader(e)), this.decode(e, e.uint32());
}, mi.verify = function(e) {
if ("object" != typeof e || null === e) return "object expected";
if (null != e.Chat && e.hasOwnProperty("Chat")) {
var t = $root.pb.ChatItem.verify(e.Chat);
if (t) return "Chat." + t;
}
return null != e.UnReadCnt && e.hasOwnProperty("UnReadCnt") && !$util.isInteger(e.UnReadCnt) ? "UnReadCnt: integer expected" : null;
}, mi), r;
}();