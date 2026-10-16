# ============================================================
#  Android_mwdl.mk — 迷雾大陆私有构建（modern 注册表入口）
#
#  构建（独立 obj，防并行会话互踩）:
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_mwdl.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_mwdl NDK_LIBS_OUT=../../libs_mwdl -j
# ============================================================
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

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
# Cocos 系：MainModern 入口 + MwdlMod + MwdlEntry，无 Unity/Il2CppApi.cpp
LOCAL_SRC_FILES := MainModern.cpp Games/MwdlMod.cpp Games/MwdlEntry.cpp

# 非 ACE 游戏 + 菜单 dex 已在包内（外科 zip 组装）⇒ B 类兜底：
# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad（2026-09-22 起），不加本文件即 ld.lld undefined symbol。
# 迷雾大陆包内 native 无 libtprt.so（无 ACE；只有 ucrash/umeng/crashsdk 上报库），
# 菜单类在 classes5.dex 且由 Manifest 注册的 com.starry.magic.menu.MenuService 拉起，
# 不做早期 dex 注入 ⇒ 用 no-op 桩。
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
                    $(LOCAL_PATH)/KittyMemory/Deps/Keystone/includes \
                    $(LOCAL_PATH)/xDL

LOCAL_STATIC_LIBRARIES := Dobby wxsj_dll

include $(BUILD_SHARED_LIBRARY)
