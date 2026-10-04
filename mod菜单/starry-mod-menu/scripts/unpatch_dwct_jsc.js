#!/usr/bin/env node
/**
 * 还原动物餐厅 v6 包内已烘焙的「免广告 / 免内购」JS 补丁，并去掉分析用 debug 注入。
 * 原函数体已不在包内：
 *   - 广告：按插屏广告平行结构重建（GDTAdStatus + showRewardAd）
 *   - 支付：改为返回 status:-1（关闭免单）；真实渠道函数无法从本包恢复
 *     → 之后由星光菜单 DwctMod 在开关打开时再注入免内购
 */
const fs = require('fs');
const path = require('path');
const pako = require((process.env.NIXIANG || require('os').homedir() + '/Documents/nixiang') + '/11_proj_tools' + '/动物餐厅' + '/cocos_jsc_decrypt' + '/node_modules' + '/pako');
const xxtea = require((process.env.NIXIANG || require('os').homedir() + '/Documents/nixiang') + '/11_proj_tools' + '/动物餐厅' + '/cocos_jsc_decrypt' + '/node_modules' + '/xxtea-node');

const KEY = '431FEWJOTe-gecTW';
const ROOT = process.argv[2] || (process.env.NIXIANG || require('os').homedir() + '/Documents/nixiang') + '/07_patched' + '/动物餐厅' + '/apktool_12.35_modmenu' + '/assets' + '/assets';

function findMatchingBrace(s, openIdx) {
  let depth = 0, i = openIdx, q = null, esc = false, lineComment = false, blockComment = false;
  for (; i < s.length; i++) {
    const c = s[i], n = s[i + 1];
    if (lineComment) { if (c === '\n') lineComment = false; continue; }
    if (blockComment) { if (c === '*' && n === '/') { blockComment = false; i++; } continue; }
    if (q) { if (esc) { esc = false; continue; } if (c === '\\') { esc = true; continue; } if (c === q) q = null; continue; }
    if (c === '/' && n === '/') { lineComment = true; i++; continue; }
    if (c === '/' && n === '*') { blockComment = true; i++; continue; }
    if (c === '"' || c === "'" || c === '`') { q = c; continue; }
    if (c === '{') depth++;
    else if (c === '}') { depth--; if (depth === 0) return i; }
  }
  throw new Error('unbalanced braces');
}

function replaceFunction(src, marker, replacement) {
  const idx = src.indexOf(marker);
  if (idx < 0) throw new Error('marker not found: ' + marker);
  const open = src.indexOf('{', idx);
  const close = findMatchingBrace(src, open);
  return src.slice(0, idx) + replacement + src.slice(close + 1);
}

function dec(buf) {
  const d = xxtea.decrypt(buf, xxtea.toBytes(KEY));
  if (!d) throw new Error('decrypt fail');
  return Buffer.from(pako.ungzip(new Uint8Array(d))).toString('utf8');
}

function enc(plain) {
  const gz = pako.gzip(Buffer.from(plain, 'utf8'), { level: 9 });
  return Buffer.from(xxtea.encrypt(gz, xxtea.toBytes(KEY)));
}

function unpatchMain(js) {
  // 去掉分析用 NET 日志
  js = js.replace(
    /o\.verifySign\(e, i\) \? n\(0, i\) : n\(-1\); try\{ console\.log\("\[NET\] "[^;]+; \}catch\(err\)\{\}/g,
    'o.verifySign(e, i) ? n(0, i) : n(-1);'
  );
  // 去掉 SELFTEST setTimeout 注入
  js = js.replace(
    /setTimeout\(function\(\)\{ try\{ m\.getLibyActivityInfo[\s\S]*?\},12000\); if \(-19 == n\.status\) \{/g,
    'if (-19 == n.status) {'
  );
  // 去掉 debug 超时加长
  js = js.replace(
    /u = 5e3; "debug_sync_item" == e && \(u = 3e4\); "addActivityItem" == e && \(u = 3e4\);/g,
    'u = 5e3;'
  );

  // 还原免广告（按插屏平行结构 + AppActivity.showRewardAd）
  js = replaceFunction(js, 'isAdReady: function() {',
    'isAdReady: function() { if (v.isAppOnly()) return !0 === this.GDTAdStatus; return !!(this.adInfo && 2 === this.adInfo.status || this.adInfoLong && 2 === this.adInfoLong.status); }');
  js = replaceFunction(js, 'realShowAd: function(t, i, n, a) {',
    'realShowAd: function(t, i, n, a) {\n' +
    'n && (i = i.bind(n));\n' +
    'var r = this;\n' +
    'if (v.isAppOnly()) {\n' +
    'w.RewardAdCallback = function(e) {\n' +
    '"number" == typeof e && (e = String(e));\n' +
    'if ("0" === e) {\n' +
    'r.GDTAdAction = 2;\n' +
    'r.GDTAdStatus = !1;\n' +
    'r.appsflyer_rewardAds && r.appsflyer_rewardAds();\n' +
    'i && i(0);\n' +
    '} else {\n' +
    'r.GDTAdStatus = !1;\n' +
    'i && i("-1" === e || "-1" == e ? -1 : e);\n' +
    '}\n' +
    '};\n' +
    'v.isIosOnly() ? jsb.reflection.callStaticMethod("AdSDKManager", "showRewardVideoAD") : v.isAndroidOnly() && jsb.reflection.callStaticMethod("org/cocos2dx/javascript/AppActivity", "showRewardAd", "()V");\n' +
    '} else if ("undefined" != typeof wx && this.adInfo && this.adInfo.videoAd) {\n' +
    'var o = this.adInfo.isLong && this.adInfoLong && this.adInfoLong.videoAd ? this.adInfoLong.videoAd : this.adInfo.videoAd;\n' +
    'var s = function(e) { i && i(e); };\n' +
    'o.show().catch(function() { return o.load().then(function() { return o.show(); }); }).then(function() {}).catch(function() { s(-1); });\n' +
    'o.onClose && o.onClose(function(e) { s(e && e.isEnded || e && e.count ? 0 : -1); });\n' +
    '} else {\n' +
    'i && i(-1);\n' +
    '}\n' +
    '}');
  return js;
}

function unpatchScript(js) {
  const fail2 = 't.prototype.NAME = function(a, b) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }';
  const fail1 = 't.prototype.NAME = function(a) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }';
  const methods = [
    ['t.prototype.androidWxPay = function(t, i) {', fail2.replace('NAME', 'androidWxPay').replace('a, b', 't, i').replace(/payType:a/, 'payType:t').replace(/pay_situation:b/, 'pay_situation:i')],
    ['t.prototype.sdkboxPay = function(e) {', fail1.replace('NAME', 'sdkboxPay').replace('(a)', '(e)')],
    ['t.prototype.wxMiNiGamePay = function(e, t) {', fail2.replace('NAME', 'wxMiNiGamePay').replace('a, b', 'e, t')],
    ['t.prototype.qqMiNiPay = function(e) {', fail1.replace('NAME', 'qqMiNiPay').replace('(a)', '(e)')],
    ['t.prototype.wxIOSPay = function(e, t) {', fail2.replace('NAME', 'wxIOSPay').replace('a, b', 'e, t')],
    ['t.prototype.huaweiPMSPay = function(e, t) {', fail2.replace('NAME', 'huaweiPMSPay').replace('a, b', 'e, t')],
    ['t.prototype.googlePay = function(e, t) {', fail2.replace('NAME', 'googlePay').replace('a, b', 'e, t')],
    ['t.prototype.globalIosPay = function(e, t) {', fail2.replace('NAME', 'globalIosPay').replace('a, b', 'e, t')],
    ['t.prototype.inlandIosPay = function(e, t) {', fail2.replace('NAME', 'inlandIosPay').replace('a, b', 'e, t')],
  ];
  // 显式写失败桩，避免 replace 模板搞乱
  const stubs = {
    't.prototype.androidWxPay = function(t, i) {':
      't.prototype.androidWxPay = function(t, i) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.sdkboxPay = function(e) {':
      't.prototype.sdkboxPay = function(e) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.wxMiNiGamePay = function(e, t) {':
      't.prototype.wxMiNiGamePay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.qqMiNiPay = function(e) {':
      't.prototype.qqMiNiPay = function(e) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.wxIOSPay = function(e, t) {':
      't.prototype.wxIOSPay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.huaweiPMSPay = function(e, t) {':
      't.prototype.huaweiPMSPay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.googlePay = function(e, t) {':
      't.prototype.googlePay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.globalIosPay = function(e, t) {':
      't.prototype.globalIosPay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
    't.prototype.inlandIosPay = function(e, t) {':
      't.prototype.inlandIosPay = function(e, t) { return new Promise(function(res){ res({status:-1, orderItem:null}); }); }',
  };
  for (const [marker, body] of Object.entries(stubs)) {
    js = replaceFunction(js, marker, body);
  }
  js = replaceFunction(js, 't.prototype.changeOrderStatus = function(e) {',
    't.prototype.changeOrderStatus = function(e) { return Promise.resolve(!1); }');
  if (js.includes('fakepay')) {
    throw new Error('fakepay still present after unpatch');
  }
  return js;
}

const mainPath = path.join(ROOT, 'main/index.jsc');
const scriptPath = path.join(ROOT, 'Script/index.jsc');
let main = dec(fs.readFileSync(mainPath));
let script = dec(fs.readFileSync(scriptPath));
console.log('before: isAdReady stub=', /isAdReady: function\(\) \{ return true; \}/.test(main),
  'fakepay=', (script.match(/fakepay/g) || []).length);

main = unpatchMain(main);
script = unpatchScript(script);

console.log('after: isAdReady restored=', /GDTAdStatus/.test(main) && /isAdReady: function/.test(main),
  'showRewardAd=', main.includes('showRewardAd'),
  'fakepay=', (script.match(/fakepay/g) || []).length,
  'status:-1=', (script.match(/status:-1/g) || []).length);

fs.writeFileSync(mainPath, enc(main));
fs.writeFileSync(scriptPath, enc(script));
console.log('wrote', mainPath);
console.log('wrote', scriptPath);
