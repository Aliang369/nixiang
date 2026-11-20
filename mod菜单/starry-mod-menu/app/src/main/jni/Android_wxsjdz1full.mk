# ============================================================
#  Android_wxsjdz1full.mk — 无限升级【星光版 22 槽·带防封按钮变体】13.101 / TapTap 官方包 专用
#
#  ★ 与 Android_wxsjdz1new.mk（fix1, 21 槽）的唯一差异 ★
#    多一个 feat 21「Toggle_防封」（→ block_report 键），且初始化**不写** block_report=1；
#    其余源文件/挂点/载荷/公共区完全一致。两个 so 并存，用户按需装。
#
#  ★ 独立于以下三套，产物/obj 目录互不复用 ★
#    - wxsj（Android_wxsj_legacy.mk）  = 13.x legacy 入口（Main.cpp 分支，11 项写死）
#    - wxsjdz1（Android_wxsjdz1.mk）   = 断网版 com.xingzuodaoshi.infinityOffline / vc1647
#    - wxsjdz2（Android_wxsjdz2.mk）   = 12 线定制
#    本 mk = 官方 TapTap 包 com.xingzuodaoshi.infinityUpgrade 走 MainModern 注册表，
#    game_id = wxsjdz1（后台定制1；**不新开号**），tag = wxsjdz1_full，菜单 22 槽。
#
#  只使用 MainModern.cpp 注册表入口（Games/Wxsjdz1FullEntry.cpp 的 GameMod_Get），
#  不改公共 Android.mk / Main.cpp / MainModern.cpp / GameConfig.h。
#
#  载荷形态（明文内嵌，无卡密 ⇒ 不走 P2-2 票控加密，**不定义 SG_PAYLOAD_ENC**）：
#    Games/Wxsjdz1New19FeatDll.a = 19 项 allfix 明文补丁 **fix1**（2,764,288 B / fnv 0x768949eea17b309d
#    / md5 a184d32e8592134849e223db270a349e），objcopy 自 07_patched/无限升级/13101_new/
#    targetfuntional_13101new_allfix_19feat.dll；符号 _binary_wxsjdz1new_19feat_start/_end。
#    ★ 与 New 变体**共用同一个 .a**（不重复 objcopy），两个 so 内嵌载荷逐字节相同。
#    旧 allfix 的 Games/WxsjPatchedDll_hf6.a 保留未覆盖（仍属 13.x legacy mk 的载荷）。
#    ⚠ .bin/.a 属可再生产物（硬约束 A#14）：收尾按清单删除，禁靠保留明文当基线。
#
#  构建（NDK_OUT/NDK_LIBS_OUT 必须绝对路径，且不同模式必须换目录）：
#    NDK_PROJECT_PATH=<app 目录绝对路径> APP_BUILD_SCRIPT=$PWD/Android_wxsjdz1full.mk \
#    NDK_APPLICATION_MK=$PWD/Application.mk APP_ABI=arm64-v8a \
#    NDK_OUT=$PWD/obj_wxsjdz1full NDK_LIBS_OUT=$PWD/libs_wxsjdz1full
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# ------------------------------------------------------------
# 补丁载荷（明文内嵌，无卡密 ⇒ 不走 P2-2 票控加密，**不定义 SG_PAYLOAD_ENC**）
#   现用：19 项 allfix（官方 2,748,416 B → 补丁 2,764,288 B / fnv 0x2a37160e3bcf5b81，
#         超长 +15,872 ⇒ 运行时恒走 mode=selfarray）
#   符号名由 objcopy 的输入文件名决定：_binary_wxsjdz1new_19feat_start / _end
# ------------------------------------------------------------
include $(CLEAR_VARS)
LOCAL_MODULE := wxsjdz1full_dll
LOCAL_SRC_FILES := Games/Wxsjdz1New19FeatDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# wxsjdz1full 专用入口：MainModern 只经 GameMod_Get() 取游戏配置（22 槽）。
LOCAL_SRC_FILES := MainModern.cpp \
                   Games/Wxsjdz1FullMod.cpp \
                   Games/Wxsjdz1FullEntry.cpp \
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

LOCAL_STATIC_LIBRARIES := Dobby wxsjdz1full_dll

include $(BUILD_SHARED_LIBRARY)
