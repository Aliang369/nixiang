# ============================================================
#  Android_mxzz.mk — 冒险战争 1.1.036 (vc7, com.mxzz.zbml) 私有构建
#
#  由 Android_modern_template.mk / Android_hkld.mk 派生（new_game_mod.sh 口径）。
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_mxzz.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<绝对路径>/obj_mxzz NDK_LIBS_OUT=<绝对路径>/libs_mxzz -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_mxzz/）
#  NDK_OUT / NDK_LIBS_OUT **必须绝对路径**：相对路径会被 -C 解析到 app/ 下 → 旧 so 顶包假成功。
#
#  ★ABI：底包 1.1.036_7 只带 arm64-v8a + armeabi-v7a（无 x86_64）
#    ⇒ 默认 APP_ABI="arm64-v8a"；模拟器走 houdini/原生转译，Dobby inline hook 稳定性不保证，
#      不做「模拟器自动适配」承诺。请以 detect_abis.py 的实测输出为准。
# ============================================================
LOCAL_PATH := $(call my-dir)

# 冒险战争：Cecil 补丁版热更程序集由 llvm-objcopy 内嵌为静态库（符号见 MxzzMod.cpp：
# _binary_mxzz_patched_dll_bin_start/end）。生成：04_scripts/冒险战争/mxzz_build_so.sh
include $(CLEAR_VARS)
LOCAL_MODULE := MxzzPatchedDll
LOCAL_SRC_FILES := libs_mxzz/MxzzPatchedDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

# -fno-emulated-tls【2026-10-05 绝区零事故，默认带上】：
#   Android clang 默认 emutls，任何 TU 用了 thread_local 就会生成 UND __emutls_get_address；
#   而该符号只有 NDK 版 libc++_shared.so 弱定义 —— 游戏自带的旧 libc++_shared / 模拟器垫片
#   都没有 ⇒ dlopen 失败、菜单静默降级 Java 版、hook 全哑。
# --exclude-libs,ALL 符号卫生：禁止静态库成员符号（Dobby* 等）进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
LOCAL_SRC_FILES := MainModern.cpp Games/MxzzMod.cpp Games/MxzzEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad ⇒ B 类（无 libtprt.so / 无 ACE）用 no-op 桩。
# A 类（包内含 libtprt.so / ACE 加固）改为：
#   LOCAL_SRC_FILES += Games/SignalQuarantine.cpp Games/FgoBoot.cpp
# 本包 lib 清单未见 libtprt.so / ACE，暂按 B 类；re-analyst 若判 A 类再改。
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

LOCAL_STATIC_LIBRARIES := Dobby MxzzPatchedDll

include $(BUILD_SHARED_LIBRARY)
