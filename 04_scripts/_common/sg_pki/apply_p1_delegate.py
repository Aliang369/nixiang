# -*- coding: utf-8 -*-
"""P1-1：把 Main.cpp / MainModern.cpp 的授权链改成对 Includes/SgAuth.h 的委托。

为什么用脚本而不是手改：两处文件都是 100KB 级、且已有大量并发改动历史，
逐处 Edit 容易错位。这里每条替换都**断言命中次数**，命中数不符就整体不动（不写盘）。

改动清单（每个文件相同）：
  1) include "Includes/hmac_sha256.h" 之后插入 "Includes/SgAuth.h"
  2) grant_pepper()      → sgauth::legacy_pepper()
  3) session_live()      → sgauth::live()            （单 bool 变三判据）
  4) clear_runtime_session() 开头加 sgauth::reset()
  5) apply_grant_utf8() 整个函数体 → sgauth::apply() 委托（v2 RSA / v1 HMAC）
"""
import io
import os
import re
import sys

ROOT = r"E:\nixiang"
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
TARGETS = ["MainModern.cpp", "Main.cpp"]

APPLY_NEW = '''/** 票格式与验签逻辑的**单一实现**在 Includes/SgAuth.h：
 *    v2: 2|device|game|exp|mask|kid.<512hex RSA 签名>   ← so 里只有公钥，签不出来
 *    v1: 1|device|game|exp|menu|card|kid.<64hex HMAC>   ← 过渡期兼容，一版后退役
 *  这里只做委托，任何新逻辑都写进 SgAuth.h，不要在这两个主文件里再复制一份。
 */
static bool apply_grant_utf8(const char* grant, const char* device, const char* game) {
    if (!sgauth::apply(grant, device, game)) return false;
    g_rt_ok = true;       // 旧标志位保留：只作诱饵装饰，放行只看 sgauth::live()
    g_authorized = true;  // 同步诱饵开关，避免「只改诱饵就能开」的错觉
    LOGI("[V2] s=1 m=%x", (unsigned)sgauth::feature_mask());
    return true;
}'''


def replace_function(text, sig, new_body):
    """按函数签名定位，按大括号配对截断，替换整个函数体。"""
    i = text.find(sig)
    if i < 0:
        raise SystemExit("找不到函数签名: " + sig)
    if text.find(sig, i + 1) >= 0:
        raise SystemExit("函数签名出现多次，拒绝猜: " + sig)
    # 从签名前的注释块起点开始（保留注释由 new_body 自带）
    start = text.rfind("\n/**", 0, i)
    start = start + 1 if start >= 0 else i
    brace = text.find("{", i)
    depth = 0
    j = brace
    while j < len(text):
        if text[j] == "{":
            depth += 1
        elif text[j] == "}":
            depth -= 1
            if depth == 0:
                break
        j += 1
    if depth != 0:
        raise SystemExit("大括号不配对: " + sig)
    j += 1
    return text[:start] + new_body + text[j:]


def sub_once(text, old, new, label):
    n = text.count(old)
    if n != 1:
        raise SystemExit("替换点 %s 命中 %d 次（要求 1 次）" % (label, n))
    return text.replace(old, new, 1)


def patch(path):
    src = io.open(path, "r", encoding="utf-8").read()
    t = src

    t = sub_once(t, '#include "Includes/hmac_sha256.h"\n',
                 '#include "Includes/hmac_sha256.h"\n'
                 '#include "Includes/SgAuth.h"   // 授权链单一实现（P1：非对称票 / 掩码 / 三判据）\n',
                 "include SgAuth.h")

    t = sub_once(t,
                 '''// 须与 auth-server GRANT_HMAC_SECRET / grant_hmac_secret 默认一致
static const char* grant_pepper() {
    return OBFUSCATE("sg-grant-v1-7f3a9c2e");
}''',
                 '''// 旧 v1(HMAC) 票密钥：逻辑已收进 Includes/SgAuth.h::legacy_pepper()
//（过渡期保留一版，退役时连同服务端 grant_hmac_secret 一起删）
static const char* grant_pepper() {
    return sgauth::legacy_pepper();
}''',
                 "grant_pepper 委托")

    t = sub_once(t,
                 '''static bool session_live() {
    return g_rt_ok;
}''',
                 '''static bool session_live() {
    // 三条独立判据（票标志 / 票派生密钥自检 / 完整性摘要）都在 SgAuth.h。
    // 单点改一个 bool 不再能全开 —— 这是 P1 要拆掉的那个「一个点断卡密」。
    return sgauth::live();
}''',
                 "session_live 委托")

    t = sub_once(t,
                 "static void clear_runtime_session() {\n    g_rt_ok = false;",
                 "static void clear_runtime_session() {\n    sgauth::reset();      // 先清授权链状态（单一实现）\n    g_rt_ok = false;",
                 "clear_runtime_session 加 reset")

    t = replace_function(t, "static bool apply_grant_utf8(const char* grant, const char* device, const char* game)", APPLY_NEW)

    if t == src:
        raise SystemExit("没有发生任何改动: " + path)
    io.open(path, "w", encoding="utf-8", newline="\n").write(t)
    return len(src), len(t)


def main():
    dry = "--dry" in sys.argv
    for name in TARGETS:
        p = os.path.join(JNI, name)
        if dry:
            print("[dry] 将处理", p)
            continue
        a, b = patch(p)
        print("patched %-16s %d B -> %d B" % (name, a, b))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
