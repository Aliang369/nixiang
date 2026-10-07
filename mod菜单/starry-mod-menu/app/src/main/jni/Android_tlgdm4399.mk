# ============================================================
#  Android_tlgdm4399.mk — 弹力果冻 4399 渠道（com.ginicat.sweet.m4399）
#
#  与 Android_tlgd.mk（TapTap 渠道 com.ginicat.sweet）的差异：
#    ① 入口 = Games/TlgdM4399Entry.cpp（authGameId "tlgd"，功能位顺序同 TapTap 版）
#    ② Mod 复用同一份 Games/TlgdTapTapMod.cpp，但用 -DSTARRY_TLGD_M4399
#       把 HybridCLR 公共解释器命中点文件偏移切到 4399 的 0xBAAB0C
#       （TapTap 2.4.0 同名函数在 0xBABB0C；两包逐指令比对 26 条中 25 条一致）
#    ③ 独立 obj/libs 目录，绝不与 TapTap 构建互踩
#    ④ + Games/NoAceStubs.cpp（MainModern.cpp 2026-09-22 起的无条件引用兜底）
#    ⑤ − wxsj_dll（Android_tlgd.mk 里的 2MB 误抄死重，见下方说明）
#
#  构建铁律（绝对路径；相对路径会被 -C 解析到 app/ 下 → 假成功）：
#    cd app/src/main/jni && ndk-build -C app/src/main/jni NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_tlgdm4399.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_tlgdm4399 NDK_LIBS_OUT=../../libs_tlgdm4399 -j
#  实际入口 = 04_scripts/弹力果冻4399/tlgd4399_build_so.sh
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# ⚠【2026-10-07 实锤，勿照抄 Android_tlgd.mk 的这一段】
# Games/WxsjPatchedDll.a 是**无限世界**的 targetfuntional.dll 补丁二进制，被误抄进
# Android_tlgd.mk 的模板拷贝里。全仓源码零引用（grep targetfuntional_dll_bin 无命中），
# 但它不是 ar 归档而是裸 ELF .o（`file` 输出：ELF 64-bit LSB relocatable, ARM aarch64），
# 于是链接器把它的 .data 整段带进产物：实测 libstarry_magic.so 2,191,784 B；
# 去掉后 166,568 B（对照旧 4399 定稿 so 132,560 B / TapTap 定稿 so 157,104 B
# —— 说明历次定稿从来没有带上它）。本 mk 因此**不**链接该库：
# 省约 2MB APK/OTA 体积，且不留别的游戏的可执行数据在包里。
#
# include $(CLEAR_VARS)
# LOCAL_MODULE := wxsj_dll
# LOCAL_SRC_FILES := Games/WxsjPatchedDll.a
# include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# 4399 渠道标记：切换 Games/TlgdTapTapMod.cpp 内的 kFileTrampoline
LOCAL_CFLAGS    += -DSTARRY_TLGD_M4399

# ==== 游戏差异区 ====
LOCAL_SRC_FILES := MainModern.cpp Games/TlgdTapTapMod.cpp Games/TlgdM4399Entry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp:653/663 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad（2026-09-22 起）。弹力果冻 4399 属 B 类：
#   ① 包内 37 个 so 中无 libtprt.so（无 ACE 看门狗）
#   ② 菜单 dex 已作为 classes2.dex 打进包内（Manifest 的 application 指向
#      com.starry.magic.StarryApp），不需要 JNI_OnLoad 早期注入 dex
# 故用 no-op 兜底（不装 seccomp / 不屏蔽致命信号 / 不重复注入 dex）。
# 漏掉即 ld.lld: undefined symbol —— 2026-10-07 首次构建已实锤。
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
