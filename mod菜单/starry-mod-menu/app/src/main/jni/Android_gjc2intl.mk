# ============================================================
#  Android_gjc2intl.mk — 弓箭传说2国际版 (com.facai.archeroii) 私有构建文件
#
#  modern 注册表入口：MainModern.cpp + Archero2IntlMod.cpp + Gjc2IntlEntry.cpp
#  （2026-10-17 从 legacy Main.cpp 路线迁出；共享 Main.cpp / Android.mk /
#    GameConfig.h 零改动 —— 原 legacy 路线要求把公共 Android.mk 里
#    STARRY_GAME_GJC2INTL 那行取消注释，切游戏时被注释掉即构建失败）
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_gjc2intl.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<starry根>/obj_gjc2intl NDK_LIBS_OUT=<starry根>/libs_gjc2intl -j
#
#  ACE 档位：本包有 libtersafe2.so（国际轻量 TSS，无 libtprt）——按仓库现行
#  统一做法走 B 类 no-op 桩 Games/NoAceStubs.cpp（37/37 个私有 mk 同款，
#  含绝区零/崩坏3/命运/终末地这些带完整 ACE 者），保持与 8-23 已验证构建行为一致。
#
#  STARRY_GAME_GJC2INTL / ENABLE_UNITY_HOOK 仅供 Archero2IntlMod.cpp 经
#  GameConfig.h 取 PACKAGE / GAME_NAME / TARGET_LIB；MainModern.cpp 不读 GameConfig.h。
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

# -fno-emulated-tls：防 emutls UND 符号导致 dlopen 失败（2026-10-05 绝区零事故）
# --exclude-libs,ALL：禁止静态库成员符号进动态符号表（libChiy 类事故）
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
# FW-13：强制保留 Includes/Utils.cpp 烘焙的 OTA 构建戳（modern 入口零引用，
# --gc-sections 会整段回收 ⇒ Java OtaSoStamp.apkStamp()=0 ⇒ 覆盖安装后不清旧 so）
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2INTL
LOCAL_CPPFLAGS  += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2INTL
LOCAL_SRC_FILES := MainModern.cpp Games/Archero2IntlMod.cpp Games/Gjc2IntlEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad —— B 类 no-op 桩（见文件头 ACE 档位说明）
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# 探查脚手架（默认关闭）：`GJC2INTL_PROBE=1` 构建 ⇒ 一次性类型探查进 so，仅供分析用，
# release 产物必须**零探查代码**（so 内 grep TYPLIST/DUMPNAMED/ATTR 应为 0）
ifdef GJC2INTL_PROBE
LOCAL_CFLAGS    += -DGJC2INTL_PROBE=1
LOCAL_CPPFLAGS  += -DGJC2INTL_PROBE=1
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
