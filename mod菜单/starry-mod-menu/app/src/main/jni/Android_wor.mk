# ============================================================
#  Android_wor.mk — 境界守望者（Watcher of Realms 国际服）私有构建文件
#
#  本作：com.ap.watcherofrealms 1.6.38.829.1（Google Play 国际服）
#  母本：Android_cxswz.mk（国服潮汐守望者，仅差异区不同）
#  阶段：e2 功能阶段（无敌 + 伤害倍率；e1 环境阶段已真机验收）
#        C1 = 设备侧 wor_cfg.txt 可覆盖（看 WorMod.cpp 的「C1：设备侧配置化」段）
#        C2 = WOR_DIAG_FULL 诊断总开关（见下方 ifdef WOR_DIAG_FULL）
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_wor.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_wor NDK_LIBS_OUT=../../libs_wor -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_wor/）
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# Keystone 已移除（20260909 实测零调用死重 43KB：KittyAsm 是唯一使用者而其从未编译）。
# 将来需要 KittyAsm 运行时汇编时再加回 prebuilt 声明并在下方 LOCAL_STATIC_LIBRARIES 加 Keystone。

include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll
LOCAL_SRC_FILES := Games/WxsjPatchedDll.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区（复制/生成后只改这里）====
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
# Unity il2cpp 系保持下面默认（含 Il2CppApi）；
# Cocos 系去掉 Unity/Il2CppApi.cpp，按需增 Games/LtzjAceSoft.cpp 等过检配套
LOCAL_SRC_FILES := MainModern.cpp Games/WorMod.cpp Games/WorEntry.cpp Unity/Il2CppApi.cpp

# 非 ACE 游戏 + 菜单 dex 已在包内（外科 zip 组装）⇒ B 类兜底：
# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad，不加本文件即 ld.lld undefined symbol。
# 境界守望者包内无 libtprt.so（保护 = Hades libhades.so + 自研 libmoba.so +
# libshadowhook.so），菜单类在新增 classesN.dex 且由 Manifest 的
# StarryInitProvider 拉起 ⇒ 用 no-op 桩。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# ==== C2：诊断总开关（本作私有，2026-10-04 加）====
# 构建侧开关名 = STARRY_WOR_DIAG（与上面的 STARRY_TRACE 同风格，由 wor_build_so.sh 传）；
# 它只负责翻译成 C 侧宏 -DWOR_DIAG_FULL=：
#     不传（或 STARRY_WOR_DIAG=1）→ Games/WorMod.cpp 内建默认 1 = 排障构建（S1~S6 诊断 + S3 扫描全编入）
#     STARRY_WOR_DIAG=0         → 正式构建：so 更小、日志只留功能行
# 出正式包：ndk-build ...  STARRY_WOR_DIAG=0   （或先 export STARRY_WOR_DIAG=0）
# =0 时整段编掉的对象（定义 + 全部调用点，见 WorMod.cpp 里的 #if WOR_DIAG_FULL）：
#     DumpClassFields / DumpClassMethods / ProbeName / ProbeShortAll / ProbeMethodName /
#     WorSafeStr / WorClassLookupSelfTest / DumpBattleDiagnostics / ScanOneImage /
#     ScanAllImages / WorScanThread / StartScanThread / DumpTargetFields
# 注意：这些符号**没有跨文件引用**（Games/ 下 Archero2Mod.cpp 与 EndfieldMod.cpp 只是
#       各自同名的私有副本，签名不同）⇒ 包掉不会引发链接错误。
# 若只想在**运行时**静音（不重编），改设备侧 wor_cfg.txt 的 `diag = 0`（C1）。
ifdef STARRY_WOR_DIAG
LOCAL_CFLAGS    += -DWOR_DIAG_FULL=$(STARRY_WOR_DIAG)
LOCAL_CPPFLAGS  += -DWOR_DIAG_FULL=$(STARRY_WOR_DIAG)
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
