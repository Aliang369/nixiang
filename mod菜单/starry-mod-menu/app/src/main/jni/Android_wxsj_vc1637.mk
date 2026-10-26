# ============================================================
#  Android_wxsj_vc1637.mk — 无限升级 11.266018 / vc1637 专用
#
#  独立于 12.x：只使用 MainModern.cpp 注册表入口，不改公共
#  Android.mk / Main.cpp / GameConfig.h。
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# 三变体（2026-10-26）：hf2=2030080 / hf4=2028544(现网 1bf6c08f) / hf3=2028544
# 已停用 base=1960960（2026-09-23 之前的官方 DLL，无 AddItem 4 参形态，承载不了新功能）
include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_vc1637_dll_hf2
LOCAL_SRC_FILES := Games/WxsjVc1637Hf2Dll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_vc1637_dll_hf4
LOCAL_SRC_FILES := Games/WxsjVc1637Hf4Dll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_vc1637_dll_antiban
LOCAL_SRC_FILES := Games/WxsjVc1637AntibanDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_vc1637_dll_hf3
LOCAL_SRC_FILES := Games/WxsjVc1637Hf3Dll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -DSG_PAYLOAD_ENC=1 -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
# 双形态出包（2026-10-27 用户定）：
#   默认（不传 WXSJ_ANTIBAN_MENU）＝ 屏蔽版：防封撤出菜单、随 so 常开（15 项，转生=16）
#   WXSJ_ANTIBAN_MENU=1            ＝ 正常版：菜单保留「防封」开关（16 项，防封=16、转生=17）
ifneq ($(WXSJ_ANTIBAN_MENU),)
LOCAL_CFLAGS    += -DWXSJ_ANTIBAN_MENU
endif
LOCAL_CPPFLAGS  := -w -s -DSG_PAYLOAD_ENC=1 -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# vc1637 专用入口：MainModern 只经 GameMod_Get() 取游戏配置。
LOCAL_SRC_FILES := MainModern.cpp \
                   Games/WxsjVc1637Mod.cpp \
                   Games/WxsjVc1637Entry.cpp \
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

LOCAL_STATIC_LIBRARIES := Dobby wxsj_vc1637_dll_hf2 wxsj_vc1637_dll_hf4 wxsj_vc1637_dll_hf3 wxsj_vc1637_dll_antiban

include $(BUILD_SHARED_LIBRARY)
