# ============================================================
#  Android_wxsjdz1.mk — 无限升级【断网版】12.10701 / vc1647 专用（第三方包）
#
#  ★ 独立于 wxsj（官方 12 线 vc1644）与 wxsjdz2（官方 12 线定制 vc1644）★
#    底包 com.xingzuodaoshi.infinityOffline / vc1647，别人改过的断网包；
#    载荷 DLL / 挂点 RVA / 后台模板三者都不通用，禁与本 mk 互相复用产物。
#
#  只使用 MainModern.cpp 注册表入口（Games/Wxsjdz1Entry.cpp 的 GameMod_Get），
#  不改公共 Android.mk / Main.cpp / MainModern.cpp / GameConfig.h。
#
#  载荷形态（骨架阶段未接，见下方 TODO 段）：
#    断网版无卡密 ⇒ 不走 P2-2 票控加密（**不定义 SG_PAYLOAD_ENC**），
#    补丁 DLL 明文内嵌（objcopy 成 .a），与 11 线「无加固形态」一致。
#
#  构建（NDK_OUT/NDK_LIBS_OUT 必须绝对路径，且不同模式必须换目录）：
#    NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_wxsjdz1.mk \
#    NDK_APPLICATION_MK=./Application.mk APP_ABI=arm64-v8a \
#    NDK_OUT=$PWD/obj_wxsjdz1 NDK_LIBS_OUT=$PWD/libs_wxsjdz1
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# ------------------------------------------------------------
# 补丁载荷（明文内嵌，无卡密 ⇒ 不走 P2-2 票控加密，**不定义 SG_PAYLOAD_ENC**）
#   payload = ilpatch_dz1（30 patches）→ ilpatch_delta_dz1（+1）
#   来源 DLL 身份：1995776 B / fnv 0xaca5988be25f5119（本断网包实际装载的那份）
#   补丁 2,008,576 B（比官方数组多 12,800 B）⇒ native 侧自建数组注入，详见 Wxsjdz1Mod.cpp
#   常量头：Includes/sg_plain_wxsjdz1.h（gen_plain_header_dz1.py 生成）
# ⚠ .bin/.a 属可再生产物（硬约束 A#14）：收尾按清单删除，禁靠保留明文当基线。
# ------------------------------------------------------------
include $(CLEAR_VARS)
LOCAL_MODULE := wxsjdz1_dll
LOCAL_SRC_FILES := Games/Wxsjdz1PayloadDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# wxsjdz1 专用入口：MainModern 只经 GameMod_Get() 取游戏配置。
LOCAL_SRC_FILES := MainModern.cpp \
                   Games/Wxsjdz1Mod.cpp \
                   Games/Wxsjdz1Entry.cpp \
                   Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# ==== 公共区 ====
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

LOCAL_STATIC_LIBRARIES := Dobby wxsjdz1_dll

include $(BUILD_SHARED_LIBRARY)
