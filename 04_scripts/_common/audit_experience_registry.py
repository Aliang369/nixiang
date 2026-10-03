#!/usr/bin/env python3
"""Audit the structured metadata of experience cards.

Legacy cards without frontmatter are reported as legacy (not failed); new cards
based on the template must carry the required registry fields.
"""
import argparse
import json
import subprocess
import sys
from pathlib import Path

REQUIRED = ("experience_id", "status", "scope", "source", "evidence", "targets")
STATUSES = {"candidate", "approved", "merged", "rejected", "stale"}
APPROVAL_REQUIRED = {"approved", "merged"}
ID_RE = __import__("re").compile(r"^exp-[0-9]{8}-[a-z0-9]+(?:-[a-z0-9]+)*$")


def resolve_ref(root, value):
    value = value.strip().strip("`")
    path_text = value.split("#", 1)[0]
    if not path_text or "://" in path_text or path_text.startswith("mailto:"):
        return None
    path = Path(path_text)
    return path if path.is_absolute() else root / path


def root_for_card(path):
    # .../10_docs/_templates/<card>.md -> repository root
    return path.resolve().parents[2]


def cards(root):
    directory = root / "10_docs" / "_templates"
    return sorted(p for p in directory.glob("经验*.md") if p.name != "经验候选卡.md")


def frontmatter(path):
    lines = path.read_text(encoding="utf-8").splitlines()
    if not lines or lines[0].strip() != "---":
        return None
    try:
        end = lines.index("---", 1)
    except ValueError:
        return None
    values = {}
    for line in lines[1:end]:
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        if ":" not in line:
            return {"_error": f"无法解析 frontmatter 行: {line}"}
        key, value = line.split(":", 1)
        values[key.strip()] = value.strip().strip('"\'')
    return values


def audit(path):
    data = frontmatter(path)
    if data is None:
        return {"path": str(path), "status": "legacy", "issues": []}
    issues = []
    if "_error" in data:
        issues.append(data["_error"])
        return {"path": str(path), "status": "invalid", "issues": issues}
    missing = [key for key in REQUIRED if not data.get(key)]
    issues.extend(f"缺少 {key}" for key in missing)
    if data.get("status") not in STATUSES:
        issues.append(f"status 无效: {data.get('status', '<missing>')}")
    if data.get("experience_id") and not ID_RE.fullmatch(data["experience_id"]):
        issues.append(f"experience_id 格式无效: {data['experience_id']}")
    for key in ("source", "evidence", "targets"):
        if data.get(key):
            target = resolve_ref(root_for_card(path), data[key])
            if target is not None and not target.exists():
                issues.append(f"{key} 不存在: {data[key]}")
    if data.get("status") in APPROVAL_REQUIRED and not data.get("approved_by"):
        issues.append(f"{data['status']} 状态必须有 approved_by")
    if data.get("status") == "merged":
        commit = data.get("merged_commit", "")
        if not commit:
            issues.append("merged 状态必须有 merged_commit")
        else:
            try:
                subprocess.run(["git", "-C", str(root_for_card(path)), "cat-file", "-e", f"{commit}^{{commit}}"],
                               check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            except (OSError, subprocess.CalledProcessError):
                issues.append(f"merged_commit 不是当前仓库可解析的 commit: {commit}")
    targets = data.get("targets", "")
    if targets and targets.lower() in {"none", "无", "待定"}:
        issues.append("targets 不能使用占位值")
    return {"path": str(path), "status": "ok" if not issues else "invalid", "issues": issues}


def audit_registry(root):
    results = [audit(path) for path in cards(root)]
    id_paths = {}
    for item in results:
        data = frontmatter(Path(item["path"]))
        if data and data.get("experience_id"):
            id_paths.setdefault(data["experience_id"], []).append(item)
    for identity, entries in id_paths.items():
        if len(entries) > 1:
            for item in entries:
                item["issues"].append(f"重复 experience_id: {identity}")
                item["status"] = "invalid"
    invalid = [item for item in results if item["status"] == "invalid"]
    return {"cards": len(results), "legacy": sum(x["status"] == "legacy" for x in results),
            "invalid": len(invalid), "results": results}


def main(argv=None):
    parser = argparse.ArgumentParser(description="审计经验卡生命周期元数据")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--json", action="store_true", dest="as_json")
    args = parser.parse_args(argv)
    payload = audit_registry(args.root)
    invalid = [item for item in payload["results"] if item["status"] == "invalid"]
    if args.as_json:
        print(json.dumps(payload, ensure_ascii=False, indent=2))
    else:
        print(f"经验卡审计: cards={payload['cards']} legacy={payload['legacy']} invalid={payload['invalid']}")
        for item in invalid:
            print(f"FAIL {item['path']}: {'; '.join(item['issues'])}")
    return 1 if invalid else 0


if __name__ == "__main__":
    sys.exit(main())
