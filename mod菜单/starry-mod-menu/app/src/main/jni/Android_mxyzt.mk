# ============================================================
#  Android_modern_template.mk — 新游戏私有构建模板（modern 注册表入口）
#
#  生成：new_game_mod.sh 自动复制为 Android_<tag>.mk 并替换 Mxytz
#  手工使用：复制本文件 → 替换两处 Mxytz → 差异区按注释调整
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_<tag>.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_<tag> NDK_LIBS_OUT=../../libs_<tag> -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_<tag>/）
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

# 自完整性加固层：**已按 2026-10-27 用户裁决下线**（加固不再全局强制，
#   见 10_docs/冒险与征途/去加固_待办_20261027.md 步骤 1；2026-11-02 复刻轮执行本步骤）。
#   原 -DSG_HAVE_ZLIB=1 / -DSG_REQUIRE_INTEGRITY=1 / -DSG_INTEG_HEADER_FILE='"sg_integ_mxyzt.h"' 全部撤掉 ⇒
#   SgIntegrity/SgZip 不再参与编译，SgAuth::live() 里的 integ 判据随之编译消失（只认票 + 派生密钥，fail-closed 不变）。
#   ★为什么必须撤：本作 dex 每轮重编（菜单模板刷新）⇒ 编进 so 的 classes4.dex 哈希必然过期 ⇒
#   完整性 fail-closed 会让功能**全静默失效**（不崩、不报错），这正是历史上最难定位的一类假故障。
#   剩余步骤 2–4（MainModern.cpp / SgAuth.h / 删头文件）属共享件，留给规范优化会话。
#   本作是 native 型（无 boot js）⇒ 本来就没有 -DSG_PAYLOAD_ENC。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
# 构建戳保留（OTA 守卫判新旧用，装机闸门「查8」认它）：modern 入口零引用
# kStarryBuildStamp ⇒ 不加 --undefined 会被 --gc-sections 整段回收，so 里查不到戳。
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区（复制/生成后只改这里）====
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
# Unity il2cpp 系保持下面默认（含 Il2CppApi）；
# Cocos 系去掉 Unity/Il2CppApi.cpp，按需增 Games/LtzjAceSoft.cpp 等过检配套
LOCAL_SRC_FILES := MainModern.cpp Games/MxytzMod.cpp Games/MxytzBattle.cpp Games/MxytzEntry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp:653/663 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad。冒险与征途官方 1.5.9 的 lib/arm64-v8a 共 19 个 so，
# 无 libtprt.so（无 ACE）⇒ 走 B 类：加 NoAceStubs.cpp 兜底为 no-op，
# 不加即 ld.lld: undefined symbol: SignalQuarantine_Start。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

# 诊断脚手架编译期隔离（2026-09-30 伊莫复盘 P0-1）：
#   带 STARRY_DIAG=1 构建 ⇒ DLOG/诊断 Lua chunk 进 so，产物名带 -diag
#   不带（默认）        ⇒ so 内**零诊断代码**（宏展开 ((void)0)，chunk 为空串）
#   配套：ModDiag.h / ModDiagLua.h（复制即可用）+ _common/check_release_clean.sh 门禁
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

LOCAL_STATIC_LIBRARIES := Dobby wxsj_dll

include $(BUILD_SHARED_LIBRARY)
