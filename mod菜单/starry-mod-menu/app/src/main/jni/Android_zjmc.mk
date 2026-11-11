# ============================================================
#  Android_zjmc.mk — 追迹迷城（com.TeaMelo.DungeonChasers）私有构建
#
#  为什么单独一份：
#    公共 Android.mk 的激活段（:67）固定为注释态、且无条件声明
#    wxsj_dll_hf2/hf3 PREBUILT 静态库；切公共 mk 会污染其他游戏的出包状态，
#    且公共 mk 属受保护文件（游戏执行会话禁改）。
#  形态：legacy 入口（Main.cpp + #if STARRY_GAME_ZJMC），与公共 mk 的 ZJMC 分支逐字等价。
#
#  构建：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_zjmc.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<abs>/obj_zjmc NDK_LIBS_OUT=<abs>/libs_zjmc -j8
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

# ==== 游戏差异区 ====
# ZjmcMod.cpp 只用 Dobby（无 KittyAsm / 无 Keystone），故不链 Keystone。
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp Unity/Il2CppApi.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# ==== 公共区（与公共 Android.mk 一致）====
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
