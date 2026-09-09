# ============================================================
# ES2（偶像梦幻祭2）私有构建脚本 — 与公共 Android.mk（其他会话可能切到别的游戏）隔离
# 用法：surgical_es2_4.4.8684.sh 已指向本文件；勿把公共 Android.mk 的激活段抢占
# ============================================================
LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)
include $(CLEAR_VARS)
LOCAL_MODULE := Keystone
LOCAL_SRC_FILES := KittyMemory/Deps/Keystone/libs-android/$(TARGET_ARCH_ABI)/libkeystone.a
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
# 偶像梦幻祭2 4.4.8684 — Es2Mod（探测 v1.1：dlopen 符号修复 + il2cpp 全量枚举 dump）
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ES2
ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif
LOCAL_SRC_FILES := Main.cpp Games/Es2Mod.cpp Unity/Il2CppApi.cpp
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
LOCAL_STATIC_LIBRARIES := Dobby Keystone wxsj_dll
include $(BUILD_SHARED_LIBRARY)
