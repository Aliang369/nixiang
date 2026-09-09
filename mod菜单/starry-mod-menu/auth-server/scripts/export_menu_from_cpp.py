#!/usr/bin/env python3
"""从 Main.cpp g_featureList 导出可门控功能，便于后台「菜单模板」录入。

用法:
  python3 scripts/export_menu_from_cpp.py [path/to/Main.cpp]

默认路径相对仓库根: app/src/main/jni/Main.cpp
输出 JSON 到 stdout；在管理端「菜单模板」页按 编号|名称 更新即可（写数据库，无需改 menu_catalog.py）。
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DEFAULT_CPP = ROOT / "app" / "src" / "main" / "jni" / "Main.cpp"

GATE_PREFIXES = ("Toggle_", "Button_", "Slider_", "Spinner_", "InputText_", "InputValue_")


def parse_feature_list(cpp_text: str) -> list[dict]:
    # 抓取 g_featureList 数组字面量
    m = re.search(r"g_featureList\[\]\s*=\s*\{(.*?)\};", cpp_text, re.S)
    if not m:
        raise SystemExit("未找到 g_featureList")
    body = m.group(1)
    items = re.findall(r'"([^"]+)"', body)
    out = []
    for i, spec in enumerate(items):
        if spec.startswith("Category_"):
            continue
        name = spec
        for p in GATE_PREFIXES:
            if spec.startswith(p):
                name = spec[len(p) :]
                # Slider_标签:min:max → 标签
                if p == "Slider_" and ":" in name:
                    name = name.split(":", 1)[0]
                break
        out.append({"feat_num": i, "name": name, "spec": spec})
    return out


def main() -> None:
    path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_CPP
    feats = parse_feature_list(path.read_text(encoding="utf-8"))
    print(json.dumps({"source": str(path), "features": feats}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
