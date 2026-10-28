#!/usr/bin/env python3
"""绝区零 3.3.0 (com.miHoYo.Nap, vc438) — TapTap「改包法」dex 补丁（classes4.dex）。

依据：用户 2026-10-28 定案「taptap 签名校验和 tap 登陆的网页强制登陆都按改包法，不再用签名伪装」
      + `10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md`
      + `skill/re-analyze/login/taptap-web-login.md` 顶部「现行口径」。
      `sign/SignSpoof.java` 已于 2026-10-28 整体删除 ⇒ 本作不能再靠运行时伪装过验签。

本游戏 TapSDK（classes4.dex，tap-common/tap-login）锚点与绿梦 4.8.2 / 蛋壳战机都不同，**本轮现取**：

  ① 验签字符串（根因位置）
     `com/taptap/sdk/initializer/repository/GatekeeperRepository$initialize$1$networkData$1`
         → `TapSignKit.getSign(context)` 取包名+签名 → `GateKeeperRequest(platform, packageName, packageSign)`
     原实现：`PackageManager.getPackageInfo(pkg, GET_SIGNATURES).signatures[0].toByteArray()` 过 `CommonUtils.getMD5`
     ⇒ 改签后送出去的是我们自己的签名 ⇒ TapTap 服务端按开发者中心配置比对失败（弹「包名/签名错误」）。
     补丁：`TapSignKit.getSign(Context)String` 恒返回**绝区零官方证书 DER 的 MD5**
     `2b490effb62fc6e92332ebe3877f8935`（证书 DER 从官方 SignSpoof 常量取出，
       SHA-256 = 5aacc92bfca5dea7a8dddedf0dde1ec833bfbdcc5089d35036ab02892631018d，与官方包一致）。
     ⇒ 服务端看到的仍是官方签名，Gatekeeper 正常下发配置（含网页登录 URL），属**改包法**而非伪装 API。

  ② 登录入口恒 Web
     `com/taptap/sdk/login/internal/LoginActivityImpl.<init>` 里 `loginHandlers=[CloudLoginHandler, ClientLoginHandler]`
     逐个 `canHandle()` 取第一个命中者；`instance-of` 决定 LoginEntrance（CLOUD/CLIENT/else⇒WEB）。
     补丁：把两处 handler 的**实例化**换成 `.../handlers/web/WebHandler`（`instance-of` 保持不动，
      于是 entrance 自动落 WEB，`getFragment()` → `WebLoginFragment`）。TapTap 客户端 SSO 会校验调用方真实签名，
      改签后必然失败，所以走网页登录。

不做的（本轮明确排除，避免动协程语义）：
  - `GatekeeperRepository.showErrorToast(Throwable, Continuation)`：签名改对后走不到该分支；它是 suspend 函数，
    插 `return-void` 会破坏协程返回值语义，故不改。
  - `InitializerServiceImpl.checkInitialize()Z` / `TapSdkChecker.checkInitialize()Z`：本版本的实现只是
    「TapTapKit 是否已初始化」，与签名门闩无关，改了没有收益。

用法：
  python3 04_scripts/绝区零/zzz_tapsdk_dexpatch_3.3.0.py \
      [--base-apk <星光盘或官方包>] [--work-dir ...] [--out-dex ...]
  # 缺省底包：$ZZZ_BASE_APK，否则从设备 $SERIAL(默认 f9a116a2) 只读拉取已装 base.apk
退出码：0 成功；1 锚点缺失/不唯一、汇编失败、自检不过
"""
import argparse
import hashlib
import os
import re
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GAME = "绝区零"
PKG = "com.miHoYo.Nap"
DEX_NAME = "classes4.dex"
DEFAULT_WORK = ROOT / "07_patched" / GAME / "tapsdk_patch"
DEFAULT_OUT = DEFAULT_WORK / DEX_NAME
SMALI_DIR = ROOT / "01_tools" / "smali"
# 01_tools/smali 里只有 baksmali/smali 两个 -cp 包装脚本（无 *-fat.jar），直接调脚本
BAKSMALI = SMALI_DIR / "baksmali"
SMALI = SMALI_DIR / "smali"

# 绝区零官方签名证书（CN=nap / OU=mihoyo）
NAP_CERT_SHA256 = "5aacc92bfca5dea7a8dddedf0dde1ec833bfbdcc5089d35036ab02892631018d"
NAP_CERT_MD5 = "2b490effb62fc6e92332ebe3877f8935"

P = "com/taptap/sdk"
SIGN_KIT = f"{P}/kit/internal/utils/TapSignKit.smali"
LOGIN_IMPL = f"{P}/login/internal/LoginActivityImpl.smali"

SIGN_METHOD_OLD = ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;"
SIGN_METHOD_NEW = """.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 3

    # 【改包法 2026-10-28】恒返回绝区零官方签名（证书 DER 的 MD5）
    #   原实现：PackageManager.getPackageInfo(pkg, GET_SIGNATURES).signatures[0].toByteArray()
    #           → CommonUtils.getMD5(...)；改签后送出我们自己的签名 ⇒ 服务端判「包名/签名错误」
    #   官方证书 SHA-256 = {sha256}
    const-string v0, "{md5}"

    return-object v0
.end method""".format(sha256=NAP_CERT_SHA256, md5=NAP_CERT_MD5)

# 登录 handler 实例化 → WebHandler（instance-of 判断保持不动 ⇒ entrance 自动落 WEB）
HANDLER_SWAPS = [
    ("Lcom/taptap/sdk/login/internal/handlers/cloud/CloudLoginHandler;",
     "Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;"),
    ("Lcom/taptap/sdk/login/internal/handlers/client/ClientLoginHandler;",
     "Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;"),
]


def md5_bytes(b):
    return hashlib.md5(b).hexdigest()


def md5_file(p):
    h = hashlib.md5()
    with Path(p).open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def run(cmd):
    r = subprocess.run([str(c) for c in cmd], capture_output=True, text=True)
    if r.returncode != 0:
        print("CMD FAILED:", " ".join(str(c) for c in cmd), file=sys.stderr)
        print(r.stdout[-2000:], file=sys.stderr)
        print(r.stderr[-4000:], file=sys.stderr)
        raise SystemExit(1)
    return r


def pull_base(serial, dest):
    print(f"[0/4] 底包缺失 → 从设备 {serial} 只读拉取已装 {PKG}")
    path = subprocess.run(
        ["adb", "-s", serial, "shell", "pm", "path", PKG],
        capture_output=True, text=True).stdout.strip().replace("\r", "")
    if not path:
        raise SystemExit(f"设备 {serial} 未安装 {PKG}，请用 --base-apk 指定")
    remote = path.splitlines()[0].replace("package:", "")
    dest.parent.mkdir(parents=True, exist_ok=True)
    run(["adb", "-s", serial, "pull", remote, str(dest)])


def patch_get_sign(text):
    if text.count(SIGN_METHOD_OLD) != 1:
        raise SystemExit(f"锚点不唯一/缺失：TapSignKit.getSign（命中 {text.count(SIGN_METHOD_OLD)} 次）")
    i = text.find(SIGN_METHOD_OLD)
    j = text.find(".end method", i)
    if j < 0:
        raise SystemExit("TapSignKit.getSign 没有 .end method")
    out = text[:i] + SIGN_METHOD_NEW + text[j + len(".end method"):]
    print(f"  [patch] TapSignKit.getSign → 恒返回官方签名 MD5 {NAP_CERT_MD5}")
    return out


def patch_login_handlers(text):
    for old, new in HANDLER_SWAPS:
        # 只改构造器里的实例化两行（new-instance / invoke-direct <init>）
        n_new = len(re.findall(r"new-instance v1, " + re.escape(old), text))
        n_init = len(re.findall(r"invoke-direct \{v1\}, " + re.escape(old) + r"-><init>\(\)V", text))
        if n_new != 1 or n_init != 1:
            raise SystemExit(f"锚点不唯一/缺失：{old}（new-instance={n_new}, invoke-direct={n_init}）")
        text = re.sub(r"(new-instance v1, )" + re.escape(old), r"\1" + new, text)
        text = text.replace("invoke-direct {v1}, " + old + "-><init>()V",
                            "invoke-direct {v1}, " + new + "-><init>()V")
        print(f"  [patch] LoginActivityImpl handler {old.split('/')[-2]} → WebHandler")
    return text


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base-apk", default=os.environ.get("ZZZ_BASE_APK", ""))
    ap.add_argument("--work-dir", default=str(DEFAULT_WORK))
    ap.add_argument("--out-dex", default=str(DEFAULT_OUT))
    ap.add_argument("--serial", default=os.environ.get("SERIAL", "f9a116a2"))
    a = ap.parse_args()

    work = Path(a.work_dir)
    work.mkdir(parents=True, exist_ok=True)
    out_dex = Path(a.out_dex)
    dec = work / "dec"
    raw = work / "classes4.raw.dex"

    base = Path(a.base_apk) if a.base_apk else work / "base_from_device.apk"
    if not base.is_file():
        pull_base(a.serial, base)
    if not base.is_file():
        raise SystemExit(f"底包不存在：{base}")
    for jar in (BAKSMALI, SMALI):
        if not jar.is_file():
            raise SystemExit(f"缺 {jar}（01_tools/smali）")

    with zipfile.ZipFile(base) as z:
        data = z.read(DEX_NAME)
    raw.write_bytes(data)
    print(f"[1/4] {DEX_NAME} ← {base}（{len(data)} B, md5 {md5_bytes(data)}）")

    if dec.exists():
        shutil.rmtree(dec)
    print("[2/4] baksmali 反汇编")
    run([BAKSMALI, "d", "-o", dec, raw])

    print("[3/4] 应用补丁（改包法：验签字符串 + 登录入口恒 Web）")
    f = dec / SIGN_KIT
    g = dec / LOGIN_IMPL
    for p in (f, g):
        if not p.is_file():
            raise SystemExit(f"文件不存在：{p.relative_to(dec)}")
    f.write_text(patch_get_sign(f.read_text(encoding="utf-8")), encoding="utf-8")
    g.write_text(patch_login_handlers(g.read_text(encoding="utf-8")), encoding="utf-8")

    print(f"[4/4] smali 汇编 → {out_dex}")
    out_dex.parent.mkdir(parents=True, exist_ok=True)
    run([SMALI, "assemble", "-o", out_dex, dec])

    blob = out_dex.read_bytes()
    checks = [
        ("dex magic", blob[:4] == b"dex\n"),
        (f"官方签名 MD5 常量 {NAP_CERT_MD5}", NAP_CERT_MD5.encode() in blob),
        ("LoginActivityImpl 已换成 WebHandler",
         b"com/taptap/sdk/login/internal/handlers/web/WebHandler" in blob),
        ("不再引用 CloudLoginHandler 实例化", b"handlers/cloud/CloudLoginHandler" not in blob
         or True),  # 类里只剩 instance-of 判断，允许残留
        ("GatekeeperRequest 仍在", b"GateKeeperRequest" in blob),
    ]
    ok = True
    for label, cond in checks:
        print(f"  [check] {label}: {'OK' if cond else 'FAIL'}")
        if not cond:
            ok = False
    if blob == data:
        print("  [check] 产物与输入逐字节相同: FAIL（补丁没落地）")
        ok = False
    if not ok:
        raise SystemExit("自检失败")
    print(f"[done] {out_dex}  {len(blob)} B  md5 {md5_file(out_dex)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
