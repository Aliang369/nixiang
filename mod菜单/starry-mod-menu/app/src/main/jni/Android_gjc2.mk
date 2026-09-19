# ============================================================
#  Android_gjc2.mk — 弓箭传说2 国服 (com.xq.archeroii.ua) 私有构建文件
#
#  modern 注册表入口：MainModern.cpp + Archero2Mod.cpp + Gjc2Entry.cpp
#  （2026-09-19 从 legacy Main.cpp 路线迁出；共享 Main.cpp / Android.mk 零改动）
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_gjc2.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<starry根>/obj_gjc2 NDK_LIBS_OUT=<starry根>/libs_gjc2 -j
#
#  STARRY_GAME_GJC2 / ENABLE_UNITY_HOOK 仅供 Archero2Mod.cpp 经 GameConfig.h
#  取 PACKAGE / GAME_NAME / TARGET_LIB；MainModern.cpp 不读 GameConfig.h。
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
# Unity IL2CPP / HybridCLR：MainModern + Archero2Mod + Gjc2Entry + Il2CppApi
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
LOCAL_CPPFLAGS  += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
LOCAL_SRC_FILES := MainModern.cpp Games/Archero2Mod.cpp Games/Gjc2Entry.cpp Unity/Il2CppApi.cpp

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
