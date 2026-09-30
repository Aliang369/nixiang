#!/usr/bin/env python3
"""代码生成器公共工具：幂等写盘。

为什么必须幂等（2026-09-30 勇士之心实测）：
    生成器无条件 `write_text` 会刷新生成物 mtime → ndk-build/gradle 判定输入变化 →
    重编中间 `.o` → 若重建结果逐字节相同，`build_so` 阶段的
    「本次构建产生了新 .o 但 libstarry_magic.so 未变化，禁止通过」门禁
    会把每次构建都判为陈旧包，形成**永久误报**，排查成本一轮以上。

用法：
    import sys
    from pathlib import Path
    sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "_common"))
    from gen_util import write_if_changed

    write_if_changed(out_path, text)          # 内容相同则只打印 unchanged
    write_if_changed(out_path, text, encoding="ascii")
"""
from __future__ import annotations

from pathlib import Path
from typing import Union

__all__ = ["write_if_changed"]


def write_if_changed(path: Union[str, Path], text: str, encoding: str = "utf-8") -> bool:
    """内容有变化才落盘；返回 True 表示写了盘，False 表示内容相同（mtime 不变）。

    打印格式与既有 gen_*.py 保持一致（`wrote … bytes=N` / `unchanged … bytes=N`），
    便于日志比对。
    """
    target = Path(path)
    old = target.read_text(encoding=encoding) if target.exists() else None
    if old == text:
        print(f"unchanged {target} bytes={len(text)}")
        return False
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(text, encoding=encoding)
    print(f"wrote {target} bytes={len(text)}")
    return True
