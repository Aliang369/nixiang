#!/usr/bin/env python3
"""蛋壳战机 国服 1.2.1 (vc44) — TapTap「改包法」dex 补丁（classes4.dex）。

依据：用户 2026-10-28 定案「taptap 签名校验和 tap 登陆的网页强制登陆都按改包法，不再用签名伪装」
      + `10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md`。
本游戏 TapSDK（tap-login_release，4.x）与绿梦 4.8.2 的类名不同，锚点必须现取：

  ① `com/taptap/sdk/initializer/service/InitializerServiceImpl`
       `checkInitialize(Z)Z` / `checkLocalInitialize(Z)Z` —— 业务统一门闩
       （实现=`TapTapSdkInitializer.Companion.getInitState() instanceof TapInitState$Success`，
         失败即 showToast+loge 并 return false ⇒ 登录等全部静默失败）
  ② `com/taptap/sdk/initializer/repository/GatekeeperRepository`
       `isGatekeeperLoadedSuccessfully()Z` → 恒 true；`showErrorToast(TapNetworkException)` → return-void
  ③ `com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.provide(LoginRequest)`
       `aget v2, v3, v2` 之后插 `const/4 v2, 0x3` ⇒ 恒落 WebLoginFragment 分支（case 3 = WEB）

用法：
  python 04_scripts/蛋壳战机/dkzj_tapsdk_dexpatch_1.2.1_cn.py \
      [--base-apk ...] [--work-dir ...] [--out-dex ...]
退出码：0 成功；1 锚点不唯一/编译失败
"""
import argparse
import hashlib
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "蛋壳战机"
DEFAULT_APK = ROOT / "02_apks" / GAME / "蛋壳战机_1.2.1_official_vc44_from_emulator.apk"
DEFAULT_WORK = ROOT / "07_patched" / GAME / "tapsdk_patch"
DEFAULT_OUT = DEFAULT_WORK / "classes4.dex"
BAKSMALI = ROOT / "01_tools" / "smali" / "baksmali-fat.jar"
SMALI = ROOT / "01_tools" / "smali" / "smali-fat.jar"

P = "com/taptap/sdk"

REPLACE = [
    # (相对 smali 路径, 旧片段标识, 新片段, 说明)
    (
        f"{P}/initializer/service/InitializerServiceImpl.smali",
        ".method public checkInitialize(Z)Z",
        """.method public checkInitialize(Z)Z
    .registers 5

    # 【改包法】恒放行：原实现查 TapInitState$Success，失败 showToast+loge 并 return 0
    const/4 p1, 0x1

    return p1
.end method""",
        "checkInitialize(Z)Z → 恒 true",
    ),
    (
        f"{P}/initializer/service/InitializerServiceImpl.smali",
        ".method public checkLocalInitialize(Z)Z",
        """.method public checkLocalInitialize(Z)Z
    .registers 5

    # 【改包法】恒放行（原实现要求 Success 或 InProgress）
    const/4 p1, 0x1

    return p1
.end method""",
        "checkLocalInitialize(Z)Z → 恒 true",
    ),
    (
        f"{P}/initializer/repository/GatekeeperRepository.smali",
        ".method public final isGatekeeperLoadedSuccessfully()Z",
        """.method public final isGatekeeperLoadedSuccessfully()Z
    .registers 2

    # 【改包法】恒 true（原实现看 _initStateFlow.replayCache 最后一条是否 Success）
    const/4 v0, 0x1

    return v0
.end method""",
        "isGatekeeperLoadedSuccessfully()Z → 恒 true",
    ),
    (
        f"{P}/initializer/repository/GatekeeperRepository.smali",
        ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V",
        """.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
    .registers 5

    # 【改包法】掐掉验签失败弹窗（保留 .registers 原值，改小会 smali 编译失败）
    return-void
.end method""",
        "showErrorToast(TapNetworkException) → no-op",
    ),
]

# provide() 里 aget 之后强插 case 3（WEB）
INSERT_AFTER = (
    f"{P}/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali",
    "    aget v2, v3, v2\n",
    "    aget v2, v3, v2\n\n    # 【改包法】恒走 WEB 分支（case 3 = WebLoginFragment），强制网页登录\n    const/4 v2, 0x3\n",
    "DefaultTapLoginFragmentProvider.provide → 恒 WebLoginFragment",
)


def md5(p: Path) -> str:
    h = hashlib.md5()
    with p.open("rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()


def run(cmd):
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode != 0:
        print("CMD FAILED:", " ".join(map(str, cmd)), file=sys.stderr)
        print(r.stdout[-2000:], file=sys.stderr)
        print(r.stderr[-4000:], file=sys.stderr)
        raise SystemExit(1)
    return r


def replace_method(text: str, anchor: str, new_block: str, label: str) -> str:
    i = text.find(anchor)
    if i < 0:
        raise SystemExit(f"锚点缺失：{label}（{anchor}）")
    if text.find(anchor, i + 1) >= 0:
        raise SystemExit(f"锚点不唯一：{label}")
    j = text.find(".end method", i)
    if j < 0:
        raise SystemExit(f"锚点方法没有 .end method：{label}")
    j += len(".end method")
    out = text[:i] + new_block + text[j:]
    print(f"  [patch] {label}")
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base-apk", default=str(DEFAULT_APK))
    ap.add_argument("--work-dir", default=str(DEFAULT_WORK))
    ap.add_argument("--out-dex", default=str(DEFAULT_OUT))
    a = ap.parse_args()

    base = Path(a.base_apk)
    work = Path(a.work_dir)
    out_dex = Path(a.out_dex)
    dec = work / "dec"
    work.mkdir(parents=True, exist_ok=True)
    raw = work / "classes4.dex"

    if not base.is_file():
        raise SystemExit(f"底包不存在：{base}")
    if not BAKSMALI.is_file() or not SMALI.is_file():
        raise SystemExit("缺 smali/baksmali fat jar（01_tools/smali）")

    with zipfile.ZipFile(base) as z:
        data = z.read("classes4.dex")
    raw.write_bytes(data)
    print(f"[1/4] classes4.dex ← 底包（{len(data)} B, md5 {hashlib.md5(data).hexdigest()}）")

    if dec.exists():
        import shutil
        shutil.rmtree(dec)
    print("[2/4] baksmali 反汇编")
    run(["java", "-jar", str(BAKSMALI), "d", "-o", str(dec), str(raw)])

    print("[3/4] 应用补丁")
    for rel, anchor, new_block, label in REPLACE:
        f = dec / rel
        if not f.is_file():
            raise SystemExit(f"文件不存在：{rel}")
        t = f.read_text(encoding="utf-8")
        f.write_text(replace_method(t, anchor, new_block, label), encoding="utf-8")

    rel, anchor, inserted, label = INSERT_AFTER
    f = dec / rel
    t = f.read_text(encoding="utf-8")
    if t.count(anchor) != 1:
        raise SystemExit(f"插入点不唯一：{label}（命中 {t.count(anchor)} 次）")
    t = t.replace(anchor, inserted, 1)
    f.write_text(t, encoding="utf-8")
    print(f"  [patch] {label}")

    print("[4/4] smali 汇编 → 新 classes4.dex")
    out_dex.parent.mkdir(parents=True, exist_ok=True)
    run(["java", "-jar", str(SMALI), "assemble", "-o", str(out_dex), str(dec)])

    blob = out_dex.read_bytes()
    checks = [
        (b"const/4 p1, 0x1" if False else b"TapInitializer", True),  # 仅确保 dex 可读，常量仍在（日志 tag 未删）
    ]
    for needle, _ in checks:
        print(f"  [check] {needle.decode(errors='ignore')} present={needle in blob}")
    print(f"[done] {out_dex}  {len(blob)} B  md5 {md5(out_dex)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
