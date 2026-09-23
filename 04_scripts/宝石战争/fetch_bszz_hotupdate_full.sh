#!/usr/bin/env bash
# Download the complete HotUpdate payload described by the decrypted manifest.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
MANIFEST="${BSZZ_HOTUPDATE_MANIFEST:-$NIXIANG_ROOT/06_dumped/宝石战争/${BSZZ_VERSION:-6.0.013}/static/manifest.json}"
OUT_ROOT="${BSZZ_HOTUPDATE_FULL_ROOT:-$NIXIANG_ROOT/07_patched/宝石战争/${BSZZ_VERSION:-6.0.013}/official_hotupdate}"
META="$OUT_ROOT/download_summary.json"

[[ -f "$MANIFEST" ]] || { echo "ERROR: missing manifest: $MANIFEST" >&2; exit 1; }

python3 - "$MANIFEST" "$OUT_ROOT" "$META" <<'PY'
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path
from urllib.parse import quote
from urllib.request import Request, urlopen
import hashlib
import json
import os
import sys
import tempfile

manifest_path = Path(sys.argv[1])
out_root = Path(sys.argv[2])
meta_path = Path(sys.argv[3])
manifest_bytes = manifest_path.read_bytes()
manifest = json.loads(manifest_bytes)
base_url = manifest["cdnHotVRootUrl"].rstrip("/") + "/"
files = manifest["files"]


def fingerprint(path):
    h = hashlib.md5()
    size = 0
    with path.open("rb") as fh:
        while True:
            chunk = fh.read(1024 * 1024)
            if not chunk:
                break
            h.update(chunk)
            size += len(chunk)
    return size, h.hexdigest()


def is_valid(path, item):
    if not path.is_file() or path.stat().st_size != item["size"]:
        return False
    return fingerprint(path)[1] == item["hash"]


def download(item):
    rel = item["path"]
    dest = out_root / rel
    if is_valid(dest, item):
        return {"path": rel, "status": "cached", **item}

    dest.parent.mkdir(parents=True, exist_ok=True)
    url = base_url + "/".join(quote(part) for part in rel.split("/"))
    last_error = None
    for attempt in range(1, 4):
        tmp = None
        try:
            with tempfile.NamedTemporaryFile(
                prefix=dest.name + ".", suffix=".part", dir=dest.parent, delete=False
            ) as fh:
                tmp = Path(fh.name)
                request = Request(
                    url,
                    headers={
                        "User-Agent": "nixiang-hotupdate-fetch/1.0",
                        "Accept-Encoding": "identity",
                    },
                )
                with urlopen(request, timeout=120) as response:
                    while True:
                        chunk = response.read(1024 * 1024)
                        if not chunk:
                            break
                        fh.write(chunk)
            size, md5 = fingerprint(tmp)
            if size != item["size"]:
                raise RuntimeError(f"size mismatch: actual={size}, expected={item['size']}")
            if md5 != item["hash"]:
                raise RuntimeError(f"md5 mismatch: actual={md5}, expected={item['hash']}")
            os.replace(tmp, dest)
            return {"path": rel, "status": "downloaded", **item}
        except Exception as exc:
            last_error = exc
            if tmp is not None:
                tmp.unlink(missing_ok=True)
            if attempt < 3:
                continue
    raise RuntimeError(f"{rel}: {last_error}")


results = []
failures = []
with ThreadPoolExecutor(max_workers=8) as pool:
    futures = [pool.submit(download, item) for item in files]
    for future in as_completed(futures):
        try:
            result = future.result()
            results.append(result)
            print(f"[{len(results):03d}/{len(files):03d}] {result['status']} {result['path']}", flush=True)
        except Exception as exc:
            failures.append(str(exc))
            print(f"[FAIL] {exc}", file=sys.stderr, flush=True)

if failures:
    raise SystemExit("download failures:\n" + "\n".join(failures))

for item in files:
    dest = out_root / item["path"]
    size, md5 = fingerprint(dest)
    if size != item["size"] or md5 != item["hash"]:
        raise SystemExit(f"final verification failed: {item['path']}")

installed_manifest = out_root / "Android/manifest.json"
installed_manifest.parent.mkdir(parents=True, exist_ok=True)
installed_manifest.write_bytes(manifest_bytes)

meta = {
    "manifest": str(manifest_path),
    "manifest_md5": hashlib.md5(manifest_bytes).hexdigest(),
    "manifest_version": manifest["version"],
    "cdn_hotupdate_root": manifest["cdnHotVRootUrl"],
    "output_root": str(out_root),
    "file_count": len(files),
    "total_size": sum(item["size"] for item in files),
    "files": sorted(
        [
            {
                "path": item["path"],
                "size": item["size"],
                "md5": item["hash"],
                "status": next(result["status"] for result in results if result["path"] == item["path"]),
            }
            for item in files
        ],
        key=lambda item: item["path"],
    ),
}
meta_path.write_text(json.dumps(meta, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({key: meta[key] for key in ("manifest_version", "file_count", "total_size", "output_root")}, ensure_ascii=False, indent=2))
PY

echo "hotupdate_full_root=$OUT_ROOT"
echo "download_summary=$META"
