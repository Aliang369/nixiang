# ============================================================
#  Bszz / 宝石战争 私有构建脚本
#
#  2026-10-30 对齐 Android_modern_template.mk（此前是 2026-09-20 的老副本，
#  落后模板 5 项框架演进：-fno-emulated-tls / --exclude-libs,ALL /
#  -Wl,--undefined=kStarryBuildStamp / Unity/Il2CppApi.cpp / Games/NoAceStubs.cpp）。
#  游戏另类差异：宝石战争属 B 类装配（无 libtprt.so / 无 ACE，菜单 dex 由 Manifest 的
#  StarryInitProvider 拉起、外科 zip 组装）⇒ 用 NoAceStubs.cpp，不引 SignalQuarantine/
#  FgoBoot。Unity il2cpp 系 ⇒ 保留 Unity/Il2CppApi.cpp。
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_bszz.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_bszz NDK_LIBS_OUT=../../libs_bszz -j
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

# -fno-emulated-tls【2026-10-05 绝区零事故，默认带上】：
#   Android clang 默认 emutls，任何 TU 用了 thread_local 就会生成 UND __emutls_get_address；
#   而该符号只有 NDK 版 libc++_shared.so 弱定义 —— 游戏自带的旧 libc++_shared / 模拟器垫片
#   都没有 ⇒ dlopen 失败、菜单静默降级 Java 版、hook 全哑。
#   自检：llvm-nm -D --undefined-only <so> | grep emutls（应为空）。
# --exclude-libs,ALL 符号卫生（libChiy 类事故）：禁止静态库成员符号进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
# FW-13（2026-10-16）：**必须强制保留** Includes/Utils.cpp 里烘焙的 OTA 构建戳。
#   modern 入口 MainModern.cpp 零引用 kStarryBuildStamp（唯一引用者是 legacy Main.cpp），
#   --gc-sections 会把它整段回收 ⇒ so 内没有 STARRYSTAMP1:EPOCH= ⇒ Java OtaSoStamp.apkStamp()=0
#   ⇒ 覆盖安装后永不清 starry_ota 旧 so ⇒ 玩家跑旧 so。
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
LOCAL_SRC_FILES := MainModern.cpp Games/BszzMod.cpp Games/BszzEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad，不加本文件即 ld.lld: undefined symbol。
# B 类（无 libtprt.so / 无 ACE）⇒ no-op 桩：不装 seccomp、不屏蔽致命信号、不做早期 dex 注入。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# 诊断脚手架编译期隔离（2026-09-30 伊莫复盘 P0-1）：默认零诊断代码。
ifdef STARRY_DIAG
LOCAL_CFLAGS    += -DSTARRY_DIAG=1
endif

# ==== 公共区（勿改）====
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
