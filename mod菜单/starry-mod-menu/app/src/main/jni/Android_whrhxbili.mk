# ============================================================
#  Android_whrhxbili.mk - 无悔华夏 B 站渠道 3.8.280 私有构建
#
#  目标：com.yhhy.whrhx.bilibili / Unity IL2CPP / libil2cpp.so
#  壳：网易易盾 NIS（新版，.gnu.fragment/.draft/.stub）+ 乐变 overlay
#  路线：无壳重建（真机运行时 so 与九游 79adda77… 字节一致 ⇒ 直接复用 WHRHX 分支与 RVA 表）
#  构建输出到独立 libs_whrhxbili，避免与共享 Android.mk / 其它渠道互相覆盖。
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

# WHRHX 分支提供功能实现；WHRHXBILI 只覆盖 game_id / 包名 / 构建标签
# ★ 不使用 emulated TLS：APP_PLATFORM=android-21 时 NDK 默认给 arm64 也生成 emutls 引用
#   （__emutls_get_address），而本游戏自带的 libc++_shared.so 是旧版、不导出该符号 ⇒
#   装机后 dlopen 直接失败（2026-10-21 实测：cannot locate symbol "__emutls_get_address"）。
LOCAL_CFLAGS    += -fno-emulated-tls
LOCAL_CPPFLAGS  += -fno-emulated-tls
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHX -DSTARRY_GAME_WHRHXBILI
LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

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
