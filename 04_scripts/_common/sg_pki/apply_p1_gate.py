# -*- coding: utf-8 -*-
"""P1-4 / P1-5：功能级门禁接入 + 去掉指向门禁的明文路标串。

P1-4：nativeSetFeature 开启条件从「会话活」升级为「该功能位被票授权」（feat_ok）。
      掩码默认全开 ⇒ 现网行为不变；后台可按功能关。
P1-5：`[验证] …` 这类串在逆向里等于「门禁在这里」的指路牌，改成 4~8 字符短码，
      保留「能自证装上/被调用」的能力（判读脚本仍能读到成功与失败两种结果）。
"""
import io
import os
import sys

ROOT = r"E:\nixiang"
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
FILES = ["MainModern.cpp", "Main.cpp"]

REPL = [
    # ---- P1-4：功能位门禁 ----
    ('''    const char* name = env->GetStringUTFChars(featName, nullptr);
    // 未授权时禁止开启；关闭始终允许（用于 lock / OnFeatureChanged 回落）
    if (enabled && !session_live()) {
        LOGW("[开关] 拒绝开启 #%d %s（未授权）", featNum, name ? name : "");
        env->ReleaseStringUTFChars(featName, name);
        return;
    }''',
     '''    const char* name = env->GetStringUTFChars(featName, nullptr);
    // 未授权时禁止开启；关闭始终允许（用于 lock / OnFeatureChanged 回落）。
    // P1-4：判据从「会话活」升级到「该功能位被这张票授权」（feat_ok），
    // 掩码默认全开 ⇒ 现网行为不变；后台可按功能单独关。
    if (enabled && !sgauth::feat_ok((int)featNum)) {
        LOGW("[SW] d #%d", featNum);
        env->ReleaseStringUTFChars(featName, name);
        return;
    }'''),

    # ---- P1-5：诱饵日志去路标 ----
    ('''    LOGI("[验证] flag=%s live=%s", authorized ? "true" : "false", session_live() ? "1" : "0");''',
     '''    LOGI("[V2] f=%d l=%d", authorized ? 1 : 0, session_live() ? 1 : 0);'''),

    ('''    if (!ok) {
        if (wasLive) {
            LOGW("[验证] grant rejected (session kept)");
        } else {
            clear_runtime_session();
            LOGW("[验证] grant rejected");
        }
        return JNI_FALSE;
    }''',
     '''    if (!ok) {
        if (!wasLive) clear_runtime_session();
        LOGW("[V2] r=%d", wasLive ? 1 : 0);
        return JNI_FALSE;
    }'''),
]


def main():
    total = 0
    for name in FILES:
        p = os.path.join(JNI, name)
        s = io.open(p, "r", encoding="utf-8").read()
        t = s
        for old, new in REPL:
            n = t.count(old)
            if n != 1:
                raise SystemExit("[%s] 替换点命中 %d 次（要求 1）：%r" % (name, n, old[:40]))
            t = t.replace(old, new, 1)
        if t == s:
            raise SystemExit("[%s] 无改动" % name)
        io.open(p, "w", encoding="utf-8", newline="\n").write(t)
        print("patched %-16s %d -> %d B" % (name, len(s), len(t)))
        total += 1
    print("ok files=%d" % total)


if __name__ == "__main__":
    main()
