# ============================================================
#  Android_wdyz.mk — 我的勇者（wdyz）私有构建（modern 注册表入口）
#
#  来源：Android_modern_template.mk 复制，{GAME} → Wdyz
#  B 类（无 libtprt.so / 无 ACE）：Games/NoAceStubs.cpp
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

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
# STARRY_GAME_WDYZ / ENABLE_UNITY_HOOK 供 WdyzMod.cpp 经 GameConfig.h 取 TARGET_LIB/PACKAGE
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WDYZ
LOCAL_CPPFLAGS  += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WDYZ
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
LOCAL_SRC_FILES := MainModern.cpp Games/WdyzMod.cpp Games/WdyzEntry.cpp Unity/Il2CppApi.cpp
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

ifdef STARRY_DIAG
LOCAL_CFLAGS    += -DSTARRY_DIAG=1
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
