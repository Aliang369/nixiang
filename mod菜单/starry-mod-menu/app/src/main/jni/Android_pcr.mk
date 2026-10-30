# ============================================================
#  Android_pcr.mk — 公主连结 11.7.2（com.bilibili.priconne）私有构建
#
#  为什么单独一份：
#    ① 公共 Android.mk 的激活段固定一款游戏，且无条件声明 wxsj_dll_hf2/hf3
#       两个 PREBUILT 静态库；这两个 .a 被 .gitignore 忽略、本机没有 ⇒
#       公共 mk 在本 checkout 直接 abort（"LOCAL_SRC_FILES points to a missing file"）。
#    ② 雷电模拟器（x86_64）适配轮需要一次同时产出 arm64-v8a + x86_64 两份 so，
#       私有 mk 不干扰其他游戏的出包状态。
#  形态：legacy 入口（Main.cpp + #if STARRY_GAME_PCR），与公共 mk 的 PCR 分支逐字等价。
#
#  构建（两 ABI 一次出）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_pcr.mk NDK_APPLICATION_MK=./Application.mk \
#      APP_ABI="arm64-v8a x86_64" NDK_OUT=<abs>/obj_pcr NDK_LIBS_OUT=<abs>/libs_pcr -j8
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

# -fno-emulated-tls：见 Android_modern_template.mk（绝区零 emutls 事故）；
#   本游戏 x86_64 侧没有自带 libc++_shared，emutls 未定义符号会直接 dlopen 失败。
# --exclude-libs,ALL：静态库成员符号（Dobby*）不进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_PCR
LOCAL_SRC_FILES := Main.cpp Games/PcrMod.cpp Unity/Il2CppApi.cpp

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
