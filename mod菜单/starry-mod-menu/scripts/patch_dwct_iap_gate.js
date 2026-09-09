#!/usr/bin/env node
/**
 * 在明文逻辑中给支付相关函数加上 window.__starryNoIap 开关门（保留原函数体）。
 * 适用于动物餐厅 12.37 / 12.38（PayMentModel 方法签名未变）。
 * 菜单只需 evalString 设置 __starryNoIap=true，无需再靠 require monkey-patch。
 */
const fs = require('fs');
const path = require('path');
const pako = require('/Users/aliang/Documents/nixiang/11_proj_tools/动物餐厅/cocos_jsc_decrypt/node_modules/pako');
const xxtea = require('/Users/aliang/Documents/nixiang/11_proj_tools/动物餐厅/cocos_jsc_decrypt/node_modules/xxtea-node');

const KEY = '431FEWJOTe-gecTW';
const ROOT = process.argv[2] || '/Users/aliang/Documents/nixiang/07_patched/动物餐厅/apktool_12.37_modmenu/assets/assets';

function dec(buf) {
  const d = xxtea.decrypt(buf, xxtea.toBytes(KEY));
  if (!d) throw new Error('decrypt fail');
  return Buffer.from(pako.ungzip(new Uint8Array(d))).toString('utf8');
}
function enc(plain) {
  const gz = pako.gzip(Buffer.from(plain, 'utf8'), { level: 9 });
  return Buffer.from(xxtea.encrypt(gz, xxtea.toBytes(KEY)));
}

/** 在 function(...) { 后插入 prelude（已插入则跳过） */
function insertAfterOpenBrace(src, marker, prelude) {
  const idx = src.indexOf(marker);
  if (idx < 0) throw new Error('marker not found: ' + marker);
  const open = src.indexOf('{', idx);
  if (open < 0) throw new Error('no { after ' + marker);
  // 已打过门
  const peek = src.slice(open, open + 120);
  if (peek.includes('__starryNoIap')) {
    console.log('skip (already gated):', marker.slice(0, 60));
    return src;
  }
  return src.slice(0, open + 1) + '\n' + prelude + '\n' + src.slice(open + 1);
}

const scriptPath = path.join(ROOT, 'Script/index.jsc');
let js = dec(fs.readFileSync(scriptPath));

const gate2 = 'if("undefined"!=typeof window&&window.__starryNoIap)return Promise.resolve({status:0,orderItem:{orderId:"fakepay",payType:arguments[0],pay_situation:arguments[1]}});';
const gate1 = 'if("undefined"!=typeof window&&window.__starryNoIap)return Promise.resolve({status:0,orderItem:{orderId:"fakepay",payType:arguments[0]}});';
const gateCos = 'if("undefined"!=typeof window&&window.__starryNoIap)return Promise.resolve(!0);';

const twoArg = [
  't.prototype.androidWxPay = function(t, i) {',
  't.prototype.wxMiNiGamePay = function(e, t) {',
  't.prototype.wxIOSPay = function(e, t) {',
  't.prototype.huaweiPMSPay = function(e, t) {',
  't.prototype.googlePay = function(e, t) {',
  't.prototype.globalIosPay = function(e, t) {',
  't.prototype.inlandIosPay = function(e, t) {',
];
const oneArg = [
  't.prototype.sdkboxPay = function(e) {',
  't.prototype.qqMiNiPay = function(e) {',
];

for (const m of twoArg) js = insertAfterOpenBrace(js, m, gate2);
for (const m of oneArg) js = insertAfterOpenBrace(js, m, gate1);
js = insertAfterOpenBrace(js, 't.prototype.changeOrderStatus = function(e) {', gateCos);

if ((js.match(/__starryNoIap/g) || []).length < 10) {
  throw new Error('expected >=10 __starryNoIap gates, got ' + (js.match(/__starryNoIap/g) || []).length);
}

fs.writeFileSync(scriptPath, enc(js));
console.log('patched', scriptPath, 'gates=', (js.match(/__starryNoIap/g) || []).length);
