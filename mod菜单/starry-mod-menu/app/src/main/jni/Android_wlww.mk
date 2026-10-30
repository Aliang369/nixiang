# ============================================================
#  Android_modern_template.mk — 新游戏私有构建模板（modern 注册表入口）
#
#  生成：new_game_mod.sh 自动复制为 Android_<tag>.mk 并替换 为了吾王
#  手工使用：复制本文件 → 替换两处 为了吾王 → 差异区按注释调整
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_<tag>.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_<tag> NDK_LIBS_OUT=../../libs_<tag> -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_<tag>/）
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# Keystone 已移除（20260909 实测零调用死重 43KB：KittyAsm 是唯一使用者而其从未编译）。
# 将来需要 KittyAsm 运行时汇编时再加回 prebuilt 声明并在下方 LOCAL_STATIC_LIBRARIES 加 Keystone。

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll
LOCAL_SRC_FILES := Games/WxsjPatchedDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

# -fno-emulated-tls【2026-10-05 绝区零事故，默认带上】：
#   Android clang 默认 emutls，任何 TU 用了 thread_local 就会生成 UND __emutls_get_address；
#   而该符号只有 NDK 版 libc++_shared.so 弱定义 —— 游戏自带的旧 libc++_shared / MuMu 的
#   arm64 垫片 libc 都没有 ⇒ dlopen 失败、菜单静默降级 Java 版、hook 全哑（日志特征：
#   `StarryNativeLoad: loadLibrary starry_magic failed ... cannot locate symbol "__emutls_get_address"`）。
#   自检：llvm-nm -D --undefined-only <so> | grep emutls（应为空）。
# --exclude-libs,ALL 符号卫生（libChiy 类事故）：禁止静态库成员符号（Dobby* 等）进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
# FW-13（2026-10-16）：**必须强制保留** Includes/Utils.cpp 里烘焙的 OTA 构建戳。
#   modern 入口 MainModern.cpp 零引用 kStarryBuildStamp（唯一引用者是 legacy Main.cpp），
#   上面这行 --gc-sections 会把它整段回收 ⇒ so 内没有 STARRYSTAMP1:EPOCH= ⇒
#   Java OtaSoStamp.apkStamp()=0 ⇒ PlayOtaManager 判据恒假 ⇒ 覆盖安装后永不清 starry_ota
#   旧 so ⇒ 玩家跑旧 so（小米 byron 实测「打开就闪退」）。共享源已加 retain，此处再兜一层：
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区（复制/生成后只改这里）====
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
# Unity il2cpp 系保持下面默认（含 Il2CppApi）；
# Cocos 系去掉 Unity/Il2CppApi.cpp，按需增 Games/LtzjAceSoft.cpp 等过检配套
LOCAL_SRC_FILES := MainModern.cpp Games/WlwwMod.cpp Games/WlwwEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad，不加本文件即 ld.lld: undefined symbol。
# 【2026-10-30 定案：本作按 A 类】底包 com.ftk1.m3839 3.0.4 含 libtprt.so +
# assets/__tpinfo.tsc（腾讯 TP 加固，代码页 r--p 按需解密）⇒ 必须装信号隔离：
# JNI_OnLoad 第一行 SignalQuarantine_Start() 才能在 libtprt.so 之后 ~10s 的
# 静默击杀（SIGBUS/SIGSEGV，不走 libc 导出）之前占位。B 类 no-op 桩会全程失守。
# FgoBoot.cpp 依赖 CrashTrap_Start（同名源在 Games/CrashTrap.cpp），一并编入。
# 本包为直装形态（classes10.dex + Manifest 的 StarryInitProvider 拉起菜单），
# 故 FgoBoot 的早期 dex 注入在本作只是空转：找不到 assets/starry_dex/starry.dex
# 与内嵌 dex 即 return false 并打 [fgo] 日志，不影响 Provider 路径。
# 参考已落地样例：Android_jfqs.mk（B 类）、Android_jjdm.mk（A 类全配、
# 另带 AceStub/Ts2DummyExports —— 那是 ACE 专用，TP 不需要，本作未带）。
LOCAL_SRC_FILES += Games/SignalQuarantine.cpp Games/FgoBoot.cpp Games/CrashTrap.cpp
# games/WlwwEarlyBoot.cpp（2026-10-30 加）：本作不能改 Manifest，so 由 libil2cpp 的 DT_INIT
# dlopen 进来 ⇒ native dlopen 不触发 JNI_OnLoad，必须自己用构造函数把
# SignalQuarantine_Start() + 手工 JNI_OnLoad 跑起来。详见该文件头。
LOCAL_SRC_FILES += Games/WlwwEarlyBoot.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# 诊断脚手架编译期隔离（2026-09-30 伊莫复盘 P0-1）：
#   带 STARRY_DIAG=1 构建 ⇒ DLOG/诊断 Lua chunk 进 so，产物名带 -diag
#   不带（默认）        ⇒ so 内**零诊断代码**（宏展开 ((void)0)，chunk 为空串）
#   配套：ModDiag.h / ModDiagLua.h（复制即可用）+ _common/check_release_clean.sh 门禁
ifdef STARRY_DIAG
LOCAL_CFLAGS    += -DSTARRY_DIAG=1
endif

# ==== 公共区（勿动）====
LOCAL_SRC_FILES += xDL/xdl.c \
                   xDL/xdl_iterate.c \
                   xDL/xdl_linker.c \
                   xDL/xdl_util.c \
                   xDL/xdl_lzma.c

LOCAL_SRC_FILES += Includes/Utils.cpp \
                   Includes/hmac_sha256.c

LOCAL_C_INCLUDES := $(LOCAL_PATH) \
                    $(LOCAL_PATH)/Includes \
                    $(LOCAL_PATH)/Unity \
                    $(LOCAL_PATH)/Games \
                    $(LOCAL_PATH)/Dobby \
                    $(LOCAL_PATH)/KittyMemory \
                    $(LOCAL_PATH)/KittyMemory/Deps/Keystone/includes \
                    $(LOCAL_PATH)/xDL

LOCAL_STATIC_LIBRARIES := Dobby wxsj_dll

include $(BUILD_SHARED_LIBRARY)
