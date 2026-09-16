#!/usr/bin/env python3
"""Patch Cocos JSB engine so the remote src bundle resolves to bundled local files."""
from __future__ import annotations

import hashlib
import json
import sys
import zipfile
from pathlib import Path

OLD = '''if (remoteBundles[bundleName]) {
      var relateUrl = "/remote/".concat(bundleName);
      var rootUrl = cc.assetManager.getRemoteRoot(options.retryCount);
      url = "".concat(rootUrl).concat(relateUrl);
      cacheManager.makeBundleFolder(bundleName);
    } else {
      url = "assets/".concat(bundleName);
    }'''

NEW = '''if (bundleName === "src") {
      url = "assets/remote/src";
      cacheManager.makeBundleFolder(bundleName);
    } else if (remoteBundles[bundleName]) {
      var relateUrl = "/remote/".concat(bundleName);
      var rootUrl = cc.assetManager.getRemoteRoot(options.retryCount);
      url = "".concat(rootUrl).concat(relateUrl);
      cacheManager.makeBundleFolder(bundleName);
    } else {
      url = "assets/".concat(bundleName);
    }'''


def main() -> int:
    if len(sys.argv) != 3:
        print("usage: patch_jsb_engine_zxr.py <base.apk> <out.js>", file=sys.stderr)
        return 2
    apk = Path(sys.argv[1])
    out = Path(sys.argv[2])
    with zipfile.ZipFile(apk) as z:
        src = z.read("assets/jsb-adapter/jsb-engine.js").decode("utf-8")
    if src.count(OLD) != 1:
        raise SystemExit(f"jsb-engine remote branch count={src.count(OLD)}")
    patched = src.replace(OLD, NEW, 1)
    patched = patched.replace("out && (out.base = url + '/');", "out && (out.base = url + '/'); if (bundleName === 'src') out.encrypted = false;", 1)
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
    print(f"patched_jsb_engine={summary['size']} sha256={summary['sha256']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
