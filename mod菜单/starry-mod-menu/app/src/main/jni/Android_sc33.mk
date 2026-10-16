# ============================================================
#  Android_sc33.mk — 生存33天 私有构建（modern 注册表入口）
#
#  用途：替换公共 Android.mk 的 legacy 激活段，改菜单/改 hook 不再碰
#  Main.cpp（__LINE__ 混淆种子 → 行号漂移即产物漂移，严禁改动）。
#
#  构建（obj 独立，防并行会话互踩）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_sc33.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_sc33 NDK_LIBS_OUT=../../libs_sc33 -j
#  正式 splice：追加 SC33_SPLICE_ENABLED=1
#  锚点 Trace（只 dump+扫描，不拼接）：追加 STARRY_TRACE=1
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

# -fno-emulated-tls：Sc33Mod.cpp 用了 thread_local（g_inEvalSplice），
#   默认 emutls 会生成 UND __emutls_get_address（游戏自带旧 libc++_shared 无此符号）。
# --exclude-libs,ALL：禁止 Dobby 等静态库成员符号进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
# modern 入口零引用 kStarryBuildStamp（唯一引用者是 legacy Main.cpp），
# 不 retain 会被 --gc-sections 回收 ⇒ Java OtaSoStamp.apkStamp()=0 ⇒ OTA 判据恒假。
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
# GameConfig.h 要求「有且仅有一个 STARRY_GAME_*」，Sc33Mod.cpp 取 GameCocos::TARGET_LIB
# / GameCocos::Symbols，故本 mk 必须自带这两个宏（与 legacy 激活段一致；
# 只在本私有 mk 里定义，不动公共 Android.mk）。
LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SC33
LOCAL_CPPFLAGS  += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SC33

LOCAL_SRC_FILES := MainModern.cpp Games/Sc33Mod.cpp Games/Sc33Entry.cpp
# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad；本作包内无 libtprt.so，菜单 dex 由外科 zip 打进
# APK 并由 Manifest 的 StarryApp 拉起 ⇒ 走 B 类 no-op 桩。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif
ifdef SC33_SPLICE_ENABLED
LOCAL_CFLAGS    += -DSC33_SPLICE_ENABLED=1
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
