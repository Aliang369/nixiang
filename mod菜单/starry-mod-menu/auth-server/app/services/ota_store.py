"""游戏补丁 OTA：每 game_id 只保留一份 latest；JSON 与 so 可共存合并。"""
from __future__ import annotations

import hashlib
import hmac
import json
import shutil
from pathlib import Path
from typing import Any

from ..config import get_settings


def ota_root() -> Path:
    settings = get_settings()
    root = getattr(settings, "ota_data_dir", None) or ""
    if root:
        p = Path(root)
    else:
        p = Path(__file__).resolve().parent.parent.parent / "data" / "ota"
    p.mkdir(parents=True, exist_ok=True)
    return p


def game_dir(game_id: str) -> Path:
    gid = (game_id or "").strip()
    if not gid or "/" in gid or "\\" in gid or ".." in gid:
        raise ValueError("invalid game_id")
    d = ota_root() / gid / "latest"
    d.mkdir(parents=True, exist_ok=True)
    return d


def _sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def _sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def _sign(payload: str) -> str:
    secret = get_settings().grant_hmac_secret.encode("utf-8")
    return hmac.new(secret, payload.encode("utf-8"), hashlib.sha256).hexdigest()


def sign_payload(game_id: str, ver: int, json_sha: str, so_sha: str, note: str) -> str:
    # game_id|ver|json_sha|so_sha|note
    raw = f"{game_id}|{ver}|{json_sha or '-'}|{so_sha or '-'}|{note or ''}"
    return _sign(raw)


def verify_payload(game_id: str, ver: int, json_sha: str, so_sha: str, note: str, sig: str) -> bool:
    expect = sign_payload(game_id, ver, json_sha, so_sha, note)
    return hmac.compare_digest(expect, (sig or "").strip().lower())


def read_manifest(game_id: str) -> dict[str, Any] | None:
    gid = (game_id or "").strip()
    if not gid or "/" in gid or "\\" in gid or ".." in gid:
        return None
    # 勿 mkdir：未发布的游戏不应因查询产生空目录
    path = ota_root() / gid / "latest" / "manifest.json"
    if not path.is_file():
        return None
    return json.loads(path.read_text(encoding="utf-8"))


def publish(
    game_id: str,
    ver: int,
    note: str = "",
    patch_json: bytes | None = None,
    so_bytes: bytes | None = None,
) -> dict[str, Any]:
    """合并写入 latest。

    JSON 与 so **兼容共存**，不是二选一：
    - 只传 JSON → 更新 patch.json，**保留**已有 so
    - 只传 so → 更新 libstarry_magic.so，**保留**已有 JSON
    - 两个都传 → 一起更新
    - 版本号 ver 必须推进；客户端按 ver 判断有无更新，重启后同时用上两边产物
    """
    if not patch_json and not so_bytes:
        raise ValueError("需要 patch.json 或 so 至少一个（未传的一侧会保留旧文件）")
    d = game_dir(game_id)
    old_man = read_manifest(game_id) or {}
    old_ver = int(old_man.get("ver") or 0)
    if int(ver) <= old_ver:
        raise ValueError(f"版本号须大于当前 latest（当前 v{old_ver}）")

    tmp = d / "_tmp"
    if tmp.exists():
        shutil.rmtree(tmp)
    tmp.mkdir(parents=True, exist_ok=True)

    # 先把未更换的旧文件拷进 tmp，再覆盖新上传的
    old_json = d / "patch.json"
    old_so = d / "libstarry_magic.so"
    if patch_json is None and old_json.is_file():
        shutil.copy2(old_json, tmp / "patch.json")
    if so_bytes is None and old_so.is_file():
        shutil.copy2(old_so, tmp / "libstarry_magic.so")

    if patch_json is not None:
        json.loads(patch_json.decode("utf-8"))
        (tmp / "patch.json").write_bytes(patch_json)
    if so_bytes is not None:
        (tmp / "libstarry_magic.so").write_bytes(so_bytes)

    json_path = tmp / "patch.json"
    so_path = tmp / "libstarry_magic.so"
    has_json = json_path.is_file()
    has_so = so_path.is_file()
    if not has_json and not has_so:
        raise ValueError("合并后仍无任何补丁文件")

    json_sha = _sha256_file(json_path) if has_json else ""
    so_sha = _sha256_file(so_path) if has_so else ""
    note = (note or "").strip()[:200]
    sig = sign_payload(game_id, int(ver), json_sha, so_sha, note)
    manifest = {
        "game_id": game_id,
        "ver": int(ver),
        "note": note,
        "has_json": has_json,
        "has_so": has_so,
        "json_sha256": json_sha,
        "so_sha256": so_sha,
        "merged_from": {
            "json": "upload" if patch_json is not None else ("keep" if has_json else "none"),
            "so": "upload" if so_bytes is not None else ("keep" if has_so else "none"),
        },
        "files": {
            "patch.json": "patch.json" if has_json else None,
            "libstarry_magic.so": "libstarry_magic.so" if has_so else None,
        },
        "sig": sig,
        "restart_required": True,
    }
    (tmp / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )

    # 原子替换：整目录换最新（tmp 已含合并后的完整集合）
    for name in ("manifest.json", "patch.json", "libstarry_magic.so"):
        old = d / name
        if old.exists():
            old.unlink()
    for p in tmp.iterdir():
        shutil.move(str(p), str(d / p.name))
    shutil.rmtree(tmp, ignore_errors=True)
    return manifest


def list_latest() -> list[dict[str, Any]]:
    """列出所有已发布 latest 补丁（每游戏一行）。"""
    root = ota_root()
    rows: list[dict[str, Any]] = []
    if not root.is_dir():
        return rows
    for child in sorted(root.iterdir(), key=lambda p: p.name):
        if not child.is_dir() or child.name.startswith("_"):
            continue
        man_path = child / "latest" / "manifest.json"
        if not man_path.is_file():
            continue
        try:
            man = json.loads(man_path.read_text(encoding="utf-8"))
        except Exception:
            continue
        if not isinstance(man, dict):
            continue
        ver = int(man.get("ver") or 0)
        has_json = bool(man.get("has_json"))
        has_so = bool(man.get("has_so"))
        # 仅列出已真正发布的 latest（有版本且至少一侧文件）
        if ver <= 0 or (not has_json and not has_so):
            continue
        rows.append(
            {
                "game_id": man.get("game_id") or child.name,
                "ver": ver,
                "note": man.get("note") or "",
                "has_json": has_json,
                "has_so": has_so,
                "json_sha256": man.get("json_sha256") or "",
                "so_sha256": man.get("so_sha256") or "",
                "merged_from": man.get("merged_from") or {},
            }
        )
    # 新版本在上
    rows.sort(key=lambda r: (-int(r.get("ver") or 0), str(r.get("game_id") or "")))
    return rows


def file_path(game_id: str, name: str) -> Path | None:
    allowed = {"manifest.json", "patch.json", "libstarry_magic.so"}
    if name not in allowed:
        return None
    p = game_dir(game_id) / name
    return p if p.is_file() else None


def clear(game_id: str, what: str = "all") -> dict[str, Any]:
    """删除该游戏 latest 补丁，节省磁盘。

    what:
      - all: 删掉整个 latest 目录（JSON+so+manifest）
      - json: 只删 patch.json，保留 so，重签 manifest
      - so: 只删 so，保留 JSON，重签 manifest
    """
    d = game_dir(game_id)
    what = (what or "all").strip().lower()
    if what not in ("all", "json", "so"):
        raise ValueError("what 须为 all / json / so")

    if what == "all":
        parent = d.parent  # data/ota/{game_id}
        if d.exists():
            shutil.rmtree(d)
        # 若 game 目录空了也清掉
        if parent.is_dir() and not any(parent.iterdir()):
            shutil.rmtree(parent, ignore_errors=True)
        return {"game_id": game_id, "cleared": "all", "has_json": False, "has_so": False, "ver": 0}

    old = read_manifest(game_id) or {}
    note = str(old.get("note") or "")
    ver = int(old.get("ver") or 0)
    if ver <= 0:
        ver = 1

    json_path = d / "patch.json"
    so_path = d / "libstarry_magic.so"
    if what == "json" and json_path.is_file():
        json_path.unlink()
    if what == "so" and so_path.is_file():
        so_path.unlink()

    has_json = json_path.is_file()
    has_so = so_path.is_file()
    if not has_json and not has_so:
        if d.exists():
            shutil.rmtree(d)
        parent = ota_root() / game_id
        if parent.is_dir() and not any(parent.iterdir()):
            shutil.rmtree(parent, ignore_errors=True)
        return {"game_id": game_id, "cleared": what, "has_json": False, "has_so": False, "ver": 0}

    # 推进 ver 并重签，避免客户端仍认为旧清单有效
    ver = ver + 1
    json_sha = _sha256_file(json_path) if has_json else ""
    so_sha = _sha256_file(so_path) if has_so else ""
    note2 = (note + f" [cleared:{what}]").strip()[:200]
    sig = sign_payload(game_id, ver, json_sha, so_sha, note2)
    manifest = {
        "game_id": game_id,
        "ver": ver,
        "note": note2,
        "has_json": has_json,
        "has_so": has_so,
        "json_sha256": json_sha,
        "so_sha256": so_sha,
        "merged_from": {"json": "keep" if has_json else "none", "so": "keep" if has_so else "none"},
        "files": {
            "patch.json": "patch.json" if has_json else None,
            "libstarry_magic.so": "libstarry_magic.so" if has_so else None,
        },
        "sig": sig,
        "restart_required": True,
    }
    (d / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    return {"game_id": game_id, "cleared": what, **{k: manifest[k] for k in ("ver", "has_json", "has_so", "note")}}
