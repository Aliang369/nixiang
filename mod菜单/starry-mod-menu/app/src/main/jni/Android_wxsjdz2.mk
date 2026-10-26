# ============================================================
#  Android_wxsjdz2.mk — 无限升级 12.10701 / vc1644 定制号 wxsjdz2 专用
#
#  独立于 wxsj（12 线旧号）与 wxsj_vc1637（11 线）：
#  只使用 MainModern.cpp 注册表入口（Games/Wxsjdz2Entry.cpp 的 GameMod_Get），
#  不改公共 Android.mk / Main.cpp / GameConfig.h。
#
#  载荷（P2-2 加密，密文进包；明文只在运行时用票里的 kx 解开）：
#    live = 现网 2,028,544（fnv 0x1bf6c08f1ba38809）+ 防封基线（无票路径）
#    hf2  = 回退 2,030,080（fnv 0x91f9acbd064fa875）
#  ⇒ 后台未登记载荷 sha / 无票 ⇒ 落防封基线（防封 on、功能 off），不会明文裸奔
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsjdz2_dll_live
LOCAL_SRC_FILES := Games/Wxsjdz2PayloadDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsjdz2_dll_hf2
LOCAL_SRC_FILES := Games/Wxsjdz2Hf2Dll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsjdz2_dll_antiban
LOCAL_SRC_FILES := Games/Wxsjdz2AntibanDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -DSG_PAYLOAD_ENC=1 -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -DSG_PAYLOAD_ENC=1 -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# wxsjdz2 专用入口：MainModern 只经 GameMod_Get() 取游戏配置。
LOCAL_SRC_FILES := MainModern.cpp \
                   Games/Wxsjdz2Mod.cpp \
                   Games/Wxsjdz2Entry.cpp \
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

LOCAL_STATIC_LIBRARIES := Dobby wxsjdz2_dll_live wxsjdz2_dll_hf2 wxsjdz2_dll_antiban

include $(BUILD_SHARED_LIBRARY)
