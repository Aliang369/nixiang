# -*- coding: utf-8 -*-
"""P1-6 接线：native 自完整性校验（APK 路径注入 + 首次/低频复验）+ mk 开 zlib + Java 传路径。"""
import io
import os
import re

ROOT = r"E:\nixiang"
JNI = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni")
MM = os.path.join(JNI, "MainModern.cpp")
MK = os.path.join(JNI, "Android_ysxz.mk")
JAVA = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "java", "com",
                    "starry", "magic")
MB = os.path.join(JAVA, "bridge", "ModBridge.java")
SIP = os.path.join(JAVA, "StarryInitProvider.java")


def sub_once(s, old, new, label):
    n = s.count(old)
    if n != 1:
        raise SystemExit("[%s] 命中 %d 次（要求 1）" % (label, n))
    return s.replace(old, new, 1)


def patch_mainmodern():
    s = io.open(MM, "r", encoding="utf-8").read()
    t = sub_once(
        s,
        '#include "Includes/SgAuth.h"   // 授权链单一实现（P1：非对称票 / 掩码 / 三判据）',
        '#include "Includes/SgAuth.h"   // 授权链单一实现（P1：非对称票 / 掩码 / 三判据）\n'
        '#include "Includes/SgIntegrity.h"  // 自完整性校验（P1-6，仅 SG_HAVE_ZLIB 构建启用）',
        "include SgIntegrity")

    t = sub_once(
        t,
        "static bool g_rt_ok        = false;    // 服务端短票验签通过",
        "static bool g_rt_ok        = false;    // 服务端短票验签通过（诱饵装饰，放行看 SgAuth）\n"
        "static char g_apkPath[512] = {0};      // 自身 APK 路径（Java 传入，自完整性校验用）",
        "g_apkPath")

    # 注入路径 + 首轮校验的 JNI
    t = sub_once(
        t,
        "static jboolean mb_nativeHasLiveSession(JNIEnv* env, jclass clazz) {\n    return session_live() ? JNI_TRUE : JNI_FALSE;\n}",
        "static jboolean mb_nativeHasLiveSession(JNIEnv* env, jclass clazz) {\n"
        "#ifdef SG_HAVE_ZLIB\n"
        "    // P1-6：低频复验（60s 一次）——防「启动时通过、之后被换」\n"
        "    sginteg::maybe_recheck(g_apkPath, 60);\n"
        "#endif\n"
        "    return session_live() ? JNI_TRUE : JNI_FALSE;\n"
        "}\n"
        "\n"
        "// ===== P1-6：接收自身 APK 路径并立刻跑一次完整性校验 =====\n"
        "static void mb_nativeSetApkPath(JNIEnv* env, jclass clazz, jstring jPath) {\n"
        "    if (!jPath) return;\n"
        "    const char* p = env->GetStringUTFChars(jPath, nullptr);\n"
        "    if (p) {\n"
        "        snprintf(g_apkPath, sizeof(g_apkPath), \"%s\", p);\n"
        "        env->ReleaseStringUTFChars(jPath, p);\n"
        "    }\n"
        "#ifdef SG_HAVE_ZLIB\n"
        "    int ok = sginteg::run(g_apkPath);\n"
        "    LOGI(\"[V2] g=%d\", ok);\n"
        "#endif\n"
        "}",
        "nativeSetApkPath impl")

    # 注册（与 nativeHasLiveSession 同一批）
    m = re.search(r'( *ok = regOne\(env, bridge, OBFUSCATE\("nativeHasLiveSession"\).*\n)', t)
    if not m:
        raise SystemExit("找不到 nativeHasLiveSession 注册行")
    reg = m.group(1)
    add = ('    ok = regOne(env, bridge, OBFUSCATE("nativeSetApkPath"), OBFUSCATE("(Ljava/lang/String;)V"),'
           ' (void*)mb_nativeSetApkPath) && ok;\n')
    if "nativeSetApkPath" not in t:
        t = t.replace(reg, reg + add, 1)

    io.open(MM, "w", encoding="utf-8", newline="\n").write(t)
    print("patched MainModern.cpp (+SgIntegrity, +g_apkPath, +nativeSetApkPath)")


def patch_mk():
    s = io.open(MK, "r", encoding="utf-8").read()
    t = s
    if "SG_HAVE_ZLIB" not in t:
        t = sub_once(t, "LOCAL_CFLAGS    := -w -s",
                     "LOCAL_CFLAGS    := -w -s -DSG_HAVE_ZLIB=1 -DSG_REQUIRE_INTEGRITY=1", "CFLAGS")
        t = sub_once(t, "LOCAL_CPPFLAGS  := -w -s",
                     "LOCAL_CPPFLAGS  := -w -s -DSG_HAVE_ZLIB=1 -DSG_REQUIRE_INTEGRITY=1", "CPPFLAGS")
    if "-lz" not in t:
        t = sub_once(t, "LOCAL_LDLIBS    := -llog -landroid",
                     "LOCAL_LDLIBS    := -llog -landroid -lz", "lz")
    io.open(MK, "w", encoding="utf-8", newline="\n").write(t)
    print("patched Android_ysxz.mk (+SG_HAVE_ZLIB/SG_REQUIRE_INTEGRITY, -lz)")


def patch_java():
    s = io.open(MB, "r", encoding="utf-8").read()
    t = sub_once(s, "    private static native boolean nativeApplyGrant(String grant, String deviceId, String gameId);",
                 "    private static native boolean nativeApplyGrant(String grant, String deviceId, String gameId);\n"
                 "\n"
                 "    /** P1-6：把自身 APK 路径交给 native 做自完整性校验 */\n"
                 "    private static native void nativeSetApkPath(String apkPath);\n"
                 "\n"
                 "    /** 入口：进程起来时调一次（native 会立刻校验并影响 session） */\n"
                 "    public static void setApkPath(android.content.Context ctx) {\n"
                 "        try {\n"
                 "            if (ctx != null) nativeSetApkPath(ctx.getPackageCodePath());\n"
                 "        } catch (Throwable ignored) {\n"
                 "            // 旧 so 没有该导出：静默跳过（不能因此崩进程）\n"
                 "        }\n"
                 "    }",
                 "ModBridge.nativeSetApkPath")
    io.open(MB, "w", encoding="utf-8", newline="\n").write(t)
    print("patched ModBridge.java (+nativeSetApkPath/setApkPath)")

    s = io.open(SIP, "r", encoding="utf-8").read()
    t = sub_once(s, '            Log.i(TAG, "native ensureLoaded=" + ok + " path=" + StarryNativeLoad.loadedPath());',
                 '            Log.i(TAG, "native ensureLoaded=" + ok + " path=" + StarryNativeLoad.loadedPath());\n'
                 '            // P1-6：把自身 APK 路径交给 native，立刻跑一次自完整性校验\n'
                 '            com.starry.magic.bridge.ModBridge.setApkPath(application);',
                 "StarryInitProvider.setApkPath")
    io.open(SIP, "w", encoding="utf-8", newline="\n").write(t)
    print("patched StarryInitProvider.java (+setApkPath 调用)")


if __name__ == "__main__":
    patch_mainmodern()
    patch_mk()
    patch_java()
    print("done")
