# ============================================================
#  Android_self_modern.mk — SELF 自测壳（modern 注册表入口）构建
#
#  用途：MainModern.cpp 框架验收（菜单全控件 + 桥接 19 项）。
#  与公共 Android.mk / Android_self_test.mk（legacy）隔离；
#  构建务必带独立 obj：NDK_OUT=app/src/obj_self_modern
#
#  构建命令（示例）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_self_modern.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_self_modern NDK_LIBS_OUT=../../libs_self_modern
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# Keystone 已移除（20260909 实测零调用死重 43KB：KittyAsm 是唯一使用者而其从未编译）。
# 将来需要 KittyAsm 运行时汇编时再加回：
#   include $(CLEAR_VARS)
#   LOCAL_MODULE := Keystone
#   LOCAL_SRC_FILES := KittyMemory/Deps/Keystone/libs-android/$(TARGET_ARCH_ABI)/libkeystone.a
#   include $(PREBUILT_STATIC_LIBRARY)
#   并在下方 LOCAL_STATIC_LIBRARIES 加 Keystone

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

# modern 注册表入口：MainModern + SelfEntry（无游戏 Mod）
LOCAL_SRC_FILES := MainModern.cpp Games/SelfEntry.cpp

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
