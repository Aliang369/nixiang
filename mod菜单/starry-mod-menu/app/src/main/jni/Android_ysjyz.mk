# ============================================================
#  Android_ysjyz.mk — 异世界勇者私有构建（modern 注册表入口）
#
#  目标：com.mudamuda.idlewow 8.1.1 / Cocos Creator 2.4.2 / libcocos2djs.so
#
#  构建（独立 obj，并行会话互不踩）:
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_ysjyz.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<starry根绝对路径>/obj_ysjyz NDK_LIBS_OUT=<starry根绝对路径>/libs_ysjyz -j
#
#  注：Cocos 系 → 不链 Unity/Il2CppApi.cpp、不链 WxsjPatchedDll.a、
#      不依赖 GameConfig.h（目标库/符号在 YsjYzMod.cpp 内本地定义）。
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区（复制/生成后只改这里）====
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
LOCAL_SRC_FILES := MainModern.cpp Games/YsjYzMod.cpp Games/YsjYzEntry.cpp

# MainModern.cpp:653/663 的 JNI_OnLoad 自 2026-09-22 起无条件引用
# SignalQuarantine_Start / FgoBoot_InjectEarlyFromOnLoad。异世界勇者官方包
# lib/arm64/ 内无 libtprt.so（无 ACE 看门狗），菜单 dex 已在 classes3.dex 并由
# Manifest 直接拉起 → 走 B 类：加 NoAceStubs.cpp 兜底为 no-op，
# 不编 SignalQuarantine.cpp / FgoBoot.cpp。
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

LOCAL_STATIC_LIBRARIES := Dobby

include $(BUILD_SHARED_LIBRARY)
