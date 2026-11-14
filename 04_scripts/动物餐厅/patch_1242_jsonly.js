// patch_1242_jsonly.js — 12.42「官方原签 + root 本地热更」两开关的 JS-only 补丁器（游戏私有）
// 与 patch_1242.js 的差别只有一处：开关来源从 JSB(DwctHooks Java 类) 换成纯 JS 配置文件
//   <writablePath>/dwct_flags.json {"noAd":bool,"noIap":bool}   —— 见 js_only_flags.js
// 其余锚点/订单语义与审计过的 patch_1242.js 逐字一致；任何锚点漂移即抛错，不猜。
// 用法: node patch_1242_jsonly.js --work <dir> --main <main.js> --script <Script.js> [--outdir <dir>]
const fs=require('fs'),path=require('path'),pako=require('pako'),xxtea=require('xxtea-node'),crypto=require('crypto'),vm=require('vm');
const args=process.argv.slice(2); const opt={work:'',main:'',script:'',outdir:''};
for(let i=0;i<args.length;i++){const a=args[i];if(a==='--work')opt.work=args[++i];else if(a==='--main')opt.main=args[++i];else if(a==='--script')opt.script=args[++i];else if(a==='--outdir')opt.outdir=args[++i];else throw Error('unknown arg '+a);}
if(!opt.work||!opt.main||!opt.script)throw Error('need --work --main --script');
const outdir=opt.outdir||opt.work; fs.mkdirSync(outdir,{recursive:true});
const key=xxtea.toBytes('431FEWJOTe-gecTW');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const ALLOWED={
  main:{'468b217c0b93fc69f6c79deccf0b5b6b88362619329ca1f48d3d68759c0491c0':'apk-12.42-main','8d66d859d6ae0fc9cb77a52b6b4f2207ceb232d6a547a7dcd6fca22ec21a5a96':'device-hot-main-11.29.2'},
  Script:{'d93c4c17f0fe35806528cbc94b1112c886f3c298f8d290188a2cbda0f8bb5759':'apk-12.42-Script','1da0378f823e435b2d2b2c6ff2fef9ceaf888a3241aaca3c4143e620d6a16535':'device-hot-Script'}
};
const gate=fs.readFileSync(path.join(__dirname,'js_only_flags.js'),'utf8');
if(gate.indexOf('__starryGate')<0)throw Error('js_only_flags.js 缺 __starryGate');
const bootstrap='/* STARRY_DWCT1242_JSONLY_V1 */\n'+gate;
function once(src,marker,prelude){if(src.split(marker).length!==2)throw Error('expected unique marker: '+marker);return src.replace(marker,marker+'\n'+prelude+'\n');}
function enc(s){return Buffer.from(xxtea.encrypt(pako.gzip(Buffer.from(s),{level:9}),key));}
function dec(b){const d=xxtea.decrypt(b,key);if(!d)throw Error('decrypt');return Buffer.from(pako.ungzip(d)).toString('utf8');}
const NOAD='window.__starryGate&&window.__starryGate.noAd()';
const NOIAP='window.__starryGate&&window.__starryGate.noIap()';
let script=fs.readFileSync(opt.script,'utf8'),main=fs.readFileSync(opt.main,'utf8');
const inSha={main:sha(Buffer.from(main)),Script:sha(Buffer.from(script))};
for(const [n,h] of Object.entries(inSha))if(!ALLOWED[n][h])throw Error('未登记输入 '+n+' sha256='+h);
const two=['androidWxPay = function(t, i)','wxMiNiGamePay = function(e, t)','wxIOSPay = function(e, t)','huaweiPMSPay = function(e, t)','googlePay = function(e, t)','globalIosPay = function(e, t)','inlandIosPay = function(e, t)'];
const one=['sdkboxPay = function(e)','qqMiNiPay = function(e)'];
for(const m of two.concat(one))script=once(script,'t.prototype.'+m+' {','if('+NOIAP+')return Promise.resolve({status:0,orderItem:{orderId:"starry-local",__starryLocal:true,payType:arguments[0],pay_situation:arguments[1]}});');
script=once(script,'t.prototype.changeOrderStatus = function(e) {','if(e&&e.__starryLocal)return Promise.resolve(true);');
const reports=`f.taHelper.trackPayment(a, p);
d.uploadShopPurchaseData({
purchaseType: t.payType
});
c.reportUserBehaviorBranchAnalytics({
behaviorType: t.payType,
eventType: 2
});
l.reportEvent("shop_" + t.payType);`;
if(script.split(reports).length!==2)throw Error('purchase report block drift');
script=script.replace(reports,'if(!t.__starryLocal){\n'+reports+'\n}else{g.default.ins.setDataSync("has_paid_before",1);s.changeData("cumulativePay",u.pay,function(){});g.default.ins.setDataSync("last_pay_time",c.getServerTime());}');
main=once(main,'isAdReady: function() {','if('+NOAD+')return true;');
const ad='if('+NOAD+'){n&&(i=i.bind(n));this.sendMessage("onAdOpen",false);if(i)i(0);return;}';
main=once(main,'showAd: function(t, i, n, a) {',ad);
main=once(main,'realShowAd: function(t, i, n, a) {',ad);
const outs=[];
for(const [name,s] of [['Script',script],['main',main]]){
  const plain=bootstrap+'\n'+s;
  new vm.Script(plain);                       // 语法门
  const out=enc(plain);
  if(dec(out)!==plain)throw Error('roundtrip '+name);
  const f=path.join(outdir,name+'.jsonly.patched.js');
  fs.writeFileSync(f,plain);
  fs.writeFileSync(path.join(outdir,name+'.jsonly.patched.jsc'),out);
  outs.push({name,input:inSha[name],plainSha256:sha(Buffer.from(plain)),jscBytes:out.length,jscSha256:sha(out),js:f});
}
const report={variant:'jsonly',flagSource:'<writablePath>/dwct_flags.json',needsJava:false,inputJs:inSha,gates:{iapChannels:9,orderVerify:1,adAnchors:3},output:outs};
fs.writeFileSync(path.join(outdir,'jsonly_patch_report.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify(report,null,2));
