# 动物大联盟 — 私有构建（Cocos2d-x JS，不编 Unity/Il2CppApi）
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll
LOCAL_SRC_FILES := Games/WxsjPatchedDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid
LOCAL_ARM_MODE  := arm

ifneq ($(STARRY_DWDLM_GEN),)
LOCAL_CFLAGS += -I$(STARRY_DWDLM_GEN)
LOCAL_CPPFLAGS += -I$(STARRY_DWDLM_GEN)
endif

LOCAL_SRC_FILES := MainModern.cpp Games/DwdlmMod.cpp Games/DwdlmEntry.cpp

# Games/DwdlmSecShell.cpp：环境阶段**唯一**的壳链接入点（独立模块，不并入 DwdlmMod.cpp）。
#   导出：DwdlmSecShell_Start() / DwdlmSecShell_State()
#         Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart(JNIEnv*, jclass)
#   与 Java 侧映射：DwdlmAppComponentFactory.nativeEnvStart 是 private static native，
#   形参按 static 规则是 jclass（无重载 ⇒ 无签名后缀），符号名与上面一致。
#   两个入口（JNI_OnLoad → Dwdlm_OnLoad；以及 nativeEnvStart）都会调 Start，
#   其内部 g_state.exchange(IDLE→RESOLVING) 保证只起一个后台线程 ⇒ 第二次幂等。
#   与旧入口不冲突：DwdlmApplication.prepareSecureShell 的 JNI 符号仍在 DwdlmMod.cpp。
LOCAL_SRC_FILES += Games/DwdlmSecShell.cpp

# MainModern.cpp:657/663 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad。本作 1.0.25 是 APKSecure 壳（libsecret /
# libEncryptorP / libzeus_direct_dex），包内无 libtprt.so（无 ACE），菜单 dex 由
# 外科 zip 打进 classes2.dex 并经 Manifest Provider 拉起 ⇒ B 类，用 no-op 桩兜底，
# 不编 SignalQuarantine.cpp / FgoBoot.cpp（避免装 seccomp / 重复注入 dex）。
# 漏掉即 ld.lld: undefined symbol（2026-11-05 实测）。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# 环境阶段构建（第一阶段：菜单进游戏能正常玩，六项功能不实施）。
# 与 STARRY_SHELL_ONLY 正交。ENV_ONLY 在**编译期**做两件事：
#   ① 剔除玩法 JS 补丁通道（DwdlmPatchJs.h / evalString·runScript splice）并把 Entry 回调置空；
#   ② 剔除 1.0.23 旧壳/启动链（InstallPathProbe / PrepareSecureShell / StartEarlyHookThread，
#      含 libsecret 0x20008·0x70318 等硬偏移与 boot-flow 硬地址），壳侧只剩
#      Games/DwdlmSecShell.cpp（从当前 1.0.25 映像导出符号序言反推状态字，不 hook 任何函数）。
# ⇒ ENV 下 STARRY_DWDLM_GEN 可缺省（生成头只在非 ENV 编译时 include）。
# 传入方式：ndk-build ... STARRY_DWDLM_ENV_ONLY=1
ifdef STARRY_DWDLM_ENV_ONLY
LOCAL_CFLAGS    += -DDWDLM_ENV_ONLY=1
LOCAL_CPPFLAGS  += -DDWDLM_ENV_ONLY=1
endif

ifdef STARRY_SHELL_ONLY
LOCAL_CFLAGS    += -DDWDLM_SHELL_ONLY=1
LOCAL_CPPFLAGS  += -DDWDLM_SHELL_ONLY=1
endif

ifdef STARRY_SHELL_STATE_ONLY
LOCAL_CFLAGS    += -DDWDLM_SHELL_ONLY=1 -DDWDLM_SHELL_STATE_ONLY=1
LOCAL_CPPFLAGS  += -DDWDLM_SHELL_ONLY=1 -DDWDLM_SHELL_STATE_ONLY=1
endif

ifdef STARRY_DATA_GUARD_ONLY
LOCAL_CFLAGS    += -DDWDLM_SHELL_ONLY=1 -DDWDLM_DATA_GUARD_ONLY=1
LOCAL_CPPFLAGS  += -DDWDLM_SHELL_ONLY=1 -DDWDLM_DATA_GUARD_ONLY=1
endif

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
