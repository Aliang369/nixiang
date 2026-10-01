# ============================================================
#  Android_neon.mk — 霓虹深渊（com.shangyoo.neon）私有构建（modern 注册表入口）
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_neon.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<abs>/app/src/obj_neon NDK_LIBS_OUT=<abs>/app/src/libs_neon -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_<tag>/）
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
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
LOCAL_SRC_FILES := MainModern.cpp Games/NeonMod.cpp Games/NeonEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start / FgoBoot_InjectEarlyFromOnLoad。
# 官方霓虹 3.2.860 包内 lib/arm64-v8a 只有 libBugly_Native/libil2cpp/libmain/libunity/libtapsdkcore/
# libxlua 等，无 libtprt.so（无 ACE 看门狗）→ 用 no-op 兜底。
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
