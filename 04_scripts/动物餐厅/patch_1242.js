// Verified 12.42 byte input; local order marker prevents any fake-order verification after a toggle changes.
const fs=require('fs'),path=require('path'),pako=require('pako'),xxtea=require('xxtea-node'),crypto=require('crypto');
const work=process.argv[2]; const key=xxtea.toBytes('431FEWJOTe-gecTW');
const bootstrap=`
/* STARRY_DWCT1242_LOCAL_V1 */
(function(){function g(n){try{return !!jsb.reflection.callStaticMethod("com/starry/magic/cocos/DwctHooks",n,"()Z");}catch(e){return false;}}Object.defineProperty(window,"__starryNoIap",{configurable:true,get:function(){return g("shouldSkipIap");}});Object.defineProperty(window,"__starryNoAd",{configurable:true,get:function(){return g("shouldSkipAd");}});})();
`;
function once(src,marker,prelude){if(src.split(marker).length!==2)throw Error('expected unique marker: '+marker);return src.replace(marker,marker+'\n'+prelude+'\n');}
function dec(b){const d=xxtea.decrypt(b,key);if(!d)throw Error('decrypt');return Buffer.from(pako.ungzip(d)).toString('utf8');}
function enc(s){return Buffer.from(xxtea.encrypt(pako.gzip(Buffer.from(s),{level:9}),key));}
const hashes={Script:'d93c4c17f0fe35806528cbc94b1112c886f3c298f8d290188a2cbda0f8bb5759',main:'468b217c0b93fc69f6c79deccf0b5b6b88362619329ca1f48d3d68759c0491c0'};
let script=fs.readFileSync(path.join(work,'index.original.js'),'utf8'),main=fs.readFileSync(path.join(work,'main.original.js'),'utf8');
for(const [name,s] of [['Script',script],['main',main]])if(crypto.createHash('sha256').update(s).digest('hex')!==hashes[name])throw Error('input hash '+name);
const two=['androidWxPay = function(t, i)','wxMiNiGamePay = function(e, t)','wxIOSPay = function(e, t)','huaweiPMSPay = function(e, t)','googlePay = function(e, t)','globalIosPay = function(e, t)','inlandIosPay = function(e, t)'];
const one=['sdkboxPay = function(e)','qqMiNiPay = function(e)'];
for(const m of two.concat(one))script=once(script,'t.prototype.'+m+' {','if(window.__starryNoIap)return Promise.resolve({status:0,orderItem:{orderId:"starry-local",__starryLocal:true,payType:arguments[0],pay_situation:arguments[1]}});');
script=once(script,'t.prototype.changeOrderStatus = function(e) {','if(e&&e.__starryLocal)return Promise.resolve(true);');
// Preserve local finishReceiveOrder item-specific effects. Only local-origin orders skip paid-purchase declarations.
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
main=once(main,'isAdReady: function() {','if(window.__starryNoAd)return true;');
const ad='if(window.__starryNoAd){n&&(i=i.bind(n));this.sendMessage("onAdOpen",false);if(i)i(0);return;}';
main=once(main,'showAd: function(t, i, n, a) {',ad);
main=once(main,'realShowAd: function(t, i, n, a) {',ad);
if(process.env.DWCT_LOGIN_DIAG==='1'){
 const sig='"inland" == y.app_type && f.isAndroidOnly() && (a = jsb.reflection.callStaticMethod("org/cocos2dx/javascript/util/DeviceHelper", "getSignature", "(Ljava/lang/String;)Ljava/lang/String;", "SHA"));';
 main=once(main,sig,'console.log("[DwctLoginDiag] checkLogin signatureIdentity="+(a==="94F80FC5161E41BAD58C6541E739920425BAE53D"?"official":a==="821C92E404AB5FDAC8DBF157C2E467E71440917D"?"current":"other"));');
 main=once(main,'m.request("checkLogin", v, "GET", function(t, n) {','console.log("[DwctLoginDiag] response transport="+t+" status="+(n&&typeof n.status==="number"?n.status:"missing"));');
 main=once(main,'this.descLbl.string = "当前版本似乎存在问题，请下载官方正版应用";','console.log("[DwctLoginDiag] updateAppDlg opened");');
 main='console.log("[DwctLoginDiag] bundle main inputSha256='+hashes.main+'");\n'+main;
}
const result=[];
for(const [name,s] of [['Script',script],['main',main]]){const plain=bootstrap+s;new (require('vm').Script)(plain);const out=enc(plain);if(dec(out)!==plain)throw Error('roundtrip '+name);fs.writeFileSync(path.join(work,name+'.patched.js'),plain);fs.writeFileSync(path.join(work,name+'.patched.jsc'),out);result.push({name,bytes:out.length,sha256:crypto.createHash('sha256').update(out).digest('hex')});}
fs.writeFileSync(path.join(work,'patch_report.json'),JSON.stringify({baseSha256:'b759601c5c9197901269a0b8ac8d31171b578fa08207ef02b565b52245a4f705',inputJs:hashes,output:result,iapChannelGates:9,orderVerifyGate:1,adGates:3,localOrderMarker:true},null,2));console.log(JSON.stringify(result));
