#!/usr/bin/env python3
"""Patch 镇邪人 main.js so the src bundle is loaded from bundled patched files."""
from __future__ import annotations

import hashlib
import json
import sys
import zipfile
from pathlib import Path

GUARD = r'''
;(function(){
  function log(msg){try{if(typeof console!=='undefined')console.log(msg);}catch(e){}}
  function wrapBundle(orig){
    if(typeof orig!=='function'||orig.__zxrWrapped)return orig;
    function wrapped(nameOrUrl,options,onComplete){
      try{
        var name=cc.path.basename(nameOrUrl);
        if(name!=='src'||/^\w+:\/\/.*/.test(nameOrUrl))return orig.apply(this,arguments);
        var am=cc.assetManager,d=am.downloader;
        var version=options.version||d.bundleVers.src||'d5507';
        var base='assets/remote/src';
        options.__cacheBundleRoot__='src';
        log('[zxr] local src loader version='+version);
        d['.json'](base+'/config.'+version+'.json',options,function(err,response){
          if(err){onComplete(err,null);return;}
          var out=response;out&&(out.base=base+'/');
          d['.js'](base+'/index.'+version+'.js',options,function(err2){
            if(err2){onComplete(err2,null);return;}
            log('[zxr] local src loaded version='+version+' patched='+(typeof window.__starryZxrOn==='function'));
            onComplete(null,out);
          });
        });
        return;
      }catch(e){log('[zxr] local src loader failed '+e);return orig.apply(this,arguments);}
    }
    wrapped.__zxrWrapped=true;
    return wrapped;
  }
  function install(){
    try{
      if(typeof cc==='undefined'||!cc.assetManager||!cc.assetManager.downloader)return false;
      var d=cc.assetManager.downloader;
      if(typeof d.register==='function'&&!d.register.__zxrWrapped){
        var reg=d.register;
        var wrappedRegister=function(one,two){
          if(one&&typeof one==='object'&&typeof one.bundle==='function')one.bundle=wrapBundle(one.bundle);
          if(one==='bundle'&&typeof two==='function')two=wrapBundle(two);
          return reg.apply(this,arguments);
        };
        wrappedRegister.__zxrWrapped=true;
        d.register=wrappedRegister;
      }
      if(typeof d['bundle']==='function')d['bundle']=wrapBundle(d['bundle']);
      if(d['bundle']&&d['bundle'].__zxrWrapped){log('[zxr] local src bundle loader installed');return true;}
    }catch(e){log('[zxr] local src install failed '+e);}
    return false;
  }
  if(!install()){
    var n=0,t=setInterval(function(){if(install()||++n>40)clearInterval(t);},250);
  }
})();/*starry_zxr_main_guard_v1*/
'''


def main() -> int:
    if len(sys.argv) != 3:
        print("usage: patch_main_zxr.py <base.apk> <out.js>", file=sys.stderr)
        return 2
    apk = Path(sys.argv[1])
    out = Path(sys.argv[2])
    with zipfile.ZipFile(apk) as z:
        main_js = z.read("assets/main.js").decode("utf-8")
    if "starry_zxr_main_guard_v1" in main_js:
        raise SystemExit("main.js already contains guard")
    patched = main_js.rstrip() + "\n" + GUARD
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(patched, encoding="utf-8", newline="")
    summary = {
        "apk": str(apk),
        "out": str(out),
        "sha256": hashlib.sha256(patched.encode("utf-8")).hexdigest(),
        "size": len(patched.encode("utf-8")),
    }
    out.with_suffix(".json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"patched_main={summary['size']} sha256={summary['sha256']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
