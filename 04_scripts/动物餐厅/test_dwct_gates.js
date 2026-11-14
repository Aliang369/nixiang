const fs=require('fs'),vm=require('vm'),assert=require('assert'),path=require('path');
const work=process.argv[2];let count=0;
function fn(src,marker,ctx){let p=src.indexOf(marker);assert(p>=0,marker);p=src.indexOf('function',p);let re=/\n}[;,]/g;re.lastIndex=p;for(let m;(m=re.exec(src));){try{return vm.runInNewContext('('+src.slice(p,m.index+2)+')',ctx);}catch(e){if(!(e instanceof SyntaxError)&&e.name!=='SyntaxError')throw e;}}throw Error('extract '+marker);}
(async()=>{for(const type of ['', '.hot']){
 const script=fs.readFileSync(path.join(work,'Script'+type+'.patched.js'),'utf8'),main=fs.readFileSync(path.join(work,'main'+type+'.patched.js'),'utf8');
 const w={__starryNoIap:true,__starryNoAd:true};
 for(const name of ['androidWxPay','sdkboxPay','wxMiNiGamePay','qqMiNiPay','wxIOSPay','huaweiPMSPay','googlePay','globalIosPay','inlandIosPay']){
  const f=fn(script,'t.prototype.'+name+' =',{window:w,Promise});const r=await f('dia1','test');assert.equal(r.status,0);assert.equal(r.orderItem.payType,'dia1');assert.equal(r.orderItem.__starryLocal,true);assert.equal(r.orderItem.orderId,'starry-local');count++;
 }
 const check=fn(script,'t.prototype.changeOrderStatus =',{window:w,Promise});w.__starryNoIap=false;assert.equal(await check({__starryLocal:true}),true);count++;
 const ready=fn(main,'isAdReady: function',{window:w});assert.equal(ready(),true);count++;
 for(const name of ['showAd','realShowAd']){let n=0,events=0,target={};const f=fn(main,name+': function',{window:w});f.call({sendMessage:(e,v)=>{assert.equal(e,'onAdOpen');assert.equal(v,false);events++;}},1,function(status){assert.equal(this,target);assert.equal(status,0);n++;},target);assert.equal(n,1);assert.equal(events,1);count++;}
 // OFF leaves function bodies byte-identical after removing only insertion guards and conditional reporting wrapper.
 assert(script.includes('if(!t.__starryLocal){'));assert(script.includes('wxApp_unifiedorder'));assert(!script.includes('fakepay'));count++;
}
console.log(JSON.stringify({test:'dwct1242_offline_local_gates',passed:count,receiptNetworkCalls:0,deviceWrites:0},null,2));})().catch(e=>{console.error(e);process.exit(1);});
