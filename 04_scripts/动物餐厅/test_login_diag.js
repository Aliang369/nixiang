const fs=require('fs'),vm=require('vm'),assert=require('assert'),path=require('path');const work=process.argv[2];let checks=0;
for(const stem of ['main.patched.js','main.hot.patched.js']){
 const text=fs.readFileSync(path.join(work,stem),'utf8'),logs=[];
 assert(text.includes('[DwctLoginDiag]'));checks++;
 const marker='m.request("checkLogin", v, "GET", function(t, n) {';let p=text.indexOf(marker)+marker.length;assert(p>=marker.length);
 const end=text.indexOf('n.data && f.initBuffetFriendInfo',p);const body=text.slice(p,end);
 for(const status of [-18,-19,-10000]){let view=null,code=null;const context={console:{log:(s)=>logs.push(s)},e:()=>({default:{ins:{pushView:(s)=>view=s}}}),i:(v)=>code=v};vm.runInNewContext('(function(t,n){'+body+'}})(0,{status:'+status+'})',context);assert.equal(view,status===-18?'updateAppDlg':status===-19?'limitVersionDlg':null);if(status===-10000)assert.equal(code,-10000);checks++;}
 assert(logs.every(s=>!s.includes('openid')&&!s.includes('token')));checks++;
}
console.log(JSON.stringify({test:'login diagnostic original error preservation',passed:checks,realNetworkRequests:0},null,2));
