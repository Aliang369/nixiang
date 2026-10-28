# ============================================================
#  Android_hzsj.mk — 魂坠深境私有构建（modern 注册表入口，2026-09-10 迁移）
#
#  构建（独立 obj，防并行会话互踩）:
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_hzsj.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_hzsj NDK_LIBS_OUT=../../libs_hzsj -j
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# 【2026-10-29 Windows 构建修复】wxsj_dll 是「无限升级 patched targetfunctional.dll」静态库的
# 复制粘贴残留（同一残留散落在 ~40 个 Android_*.mk 里，见 Android.mk 头部注释）；魂坠深境源码
# 不引用它的任何符号。而 Games/WxsjPatchedDll.a **未被 git 跟踪**（只在跑过 wxsj 构建的机器上存在），
# 于是 Windows 侧 ndk-build 直接 `LOCAL_SRC_FILES points to a missing file` 中止。
# ⇒ wildcard 守卫：有该 .a 时行为与原先完全一致（mac 侧不受影响），没有时整体跳过。
ifneq ($(wildcard $(LOCAL_PATH)/Games/WxsjPatchedDll.a),)
include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll
LOCAL_SRC_FILES := Games/WxsjPatchedDll.a
include $(PREBUILT_STATIC_LIBRARY)
endif

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
# Unity il2cpp 系：MainModern 入口 + HzsjMod + HzsjEntry + Il2CppApi
# ENABLE_UNITY_HOOK/STARRY_GAME_HZSJ 供 GameConfig.h 映射 game_id 与 GameUnity::TARGET_LIB
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_HZSJ
LOCAL_SRC_FILES := MainModern.cpp Games/HzsjMod.cpp Games/HzsjEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad，不加本文件即 ld.lld: undefined symbol。
# 魂坠深境走 B 类（无 libtprt.so / 无 ACE，菜单 dex 由 Manifest 的 StarryInitProvider
# 拉起）→ 用 no-op 桩：不装 seccomp、不屏蔽致命信号、不做早期 dex 注入。
# A 类（包内含 libtprt.so / ACE 加固）改为下面两行，删掉 NoAceStubs.cpp：
#   LOCAL_SRC_FILES += Games/SignalQuarantine.cpp Games/FgoBoot.cpp
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
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
                    $(LOCAL_PATH)/xDL

ifneq ($(wildcard $(LOCAL_PATH)/Games/WxsjPatchedDll.a),)
LOCAL_STATIC_LIBRARIES := Dobby wxsj_dll
else
LOCAL_STATIC_LIBRARIES := Dobby
endif

include $(BUILD_SHARED_LIBRARY)
