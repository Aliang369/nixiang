# ============================================================
#  Android_sf3.mk — 暗影格斗3 (com.wanda.sf3.wd) 私有构建文件
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_sf3.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_sf3 NDK_LIBS_OUT=../../libs_sf3 -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_<tag>/）
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
# Unity IL2CPP 系：MainModern + Sf3Mod + Sf3Entry + Il2CppApi
LOCAL_SRC_FILES := MainModern.cpp Games/Sf3Mod.cpp Games/Sf3Entry.cpp Unity/Il2CppApi.cpp

# 非 ACE 游戏兜底桩（2026-10-28 补）：MainModern.cpp 的 JNI_OnLoad 无条件引用
# SignalQuarantine_Start / FgoBoot_InjectEarlyFromOnLoad，不加本文件即
# ld.lld undefined symbol。本包无 libtprt.so（保护 = 图灵盾 libturingad.so +
# libthemis.so），菜单在 classes8.dex 由 Manifest 的 StarryInitProvider 拉起 ⇒ no-op 桩。
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
