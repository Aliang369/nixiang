# ============================================================
#  Android_zmws.mk — 造梦无双私有构建（modern 注册表入口）
#
#  构建（独立 obj，防并行会话互踩）:
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_zmws.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_zmws NDK_LIBS_OUT=../../libs_zmws -j
#
#  注：-DSTARRY_GAME_ZMWS 激活 GameConfig.h 的 GameCocos:: 符号段
#      （ZmwsMod.cpp 依赖 TARGET_LIB / Symbols::SE_EVAL_STRING）。
#      造梦无双无热更 dll 需求，不链 WxsjPatchedDll.a。
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions \
                   -DENABLE_COCOS_HOOK -DSTARRY_GAME_ZMWS
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
# Cocos 系：MainModern 入口 + ZmwsMod + ZmwsEntry，无 Unity/Il2CppApi.cpp
LOCAL_SRC_FILES := MainModern.cpp Games/ZmwsMod.cpp Games/ZmwsEntry.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

ifdef STARRY_ZMWS_PROBE
LOCAL_CFLAGS    += -DSTARRY_ZMWS_PROBE=1
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
