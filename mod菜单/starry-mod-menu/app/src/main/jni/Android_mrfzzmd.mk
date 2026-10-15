# ============================================================
#  Android_mrfzzmd.mk — 明日方舟：终末地（com.hypergryph.endfield）
#  modern 注册表入口 + 存量 legacy Mod 迁移（2026-10-16）
#
#  构建（NDK_OUT/LIBS_OUT 必须绝对路径：相对路径被 ndk-build -C
#  解析到 app/ 下 → 校验到旧 so 假成功，2026-09-10 事故）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_mrfzzmd.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=<starry根>/obj_mrfzzmd NDK_LIBS_OUT=<starry根>/libs_mrfzzmd -j
#
#  为什么不用公共 Android.mk（legacy 切换）：2026-10-15 m00007 定
#  「Main.cpp / MainModern.cpp / GameConfig.h / 公共 Android.mk 禁改」，
#  存量游戏重新出包一律迁移到私有 mk + Entry（SWITCH_GAME.md 双轨制）。
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
# 必须强制保留 Includes/Utils.cpp 里烘焙的构建戳（OTA 守卫判「包内 so 是否比玩家已下载的 OTA so 新」）：
#   戳的唯一引用点是 **legacy `Main.cpp:2539`**；modern 入口 `MainModern.cpp` 零引用
#   ⇒ `--gc-sections` 把未引用的 const 回收 ⇒ so 内无 `STARRYSTAMP1:EPOCH=` ⇒ Java 侧
#   `OtaSoStamp.apkStamp()` 返回 0 ⇒ `PlayOtaManager:375` 的 `if (apkStamp > 0L && …)` 恒假
#   ⇒ **永远 keep OTA** ⇒ 覆盖安装后仍优先加载玩家的旧 OTA so（"新包装上了、跑的却是旧 so"）。
#   2026-10-16 小米 byron 实测即此形态（崩溃帧 so build-id 24da7cd7… ≠ 包内 13e40d97…，清数据后正常）。
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区 ====
# 源文件三件套：现代入口 + 游戏 Mod（存量 v14.3 复用）+ 注册表 Entry
LOCAL_SRC_FILES := MainModern.cpp Games/EndfieldMod.cpp Games/EndfieldEntry.cpp Unity/Il2CppApi.cpp

# ENABLE_UNITY_HOOK 必需：GameConfig.h 的 GameUnity 段（含 TARGET_LIB）
# 在 #ifdef ENABLE_UNITY_HOOK 内，而 EndfieldMod.cpp 依赖 GameUnity::TARGET_LIB。
# STARRY_GAME_ENDFIELD 供 GameConfig.h 映射 game_id=mrfzzmd / PACKAGE。
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ENDFIELD

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad（2026-09-22 起），不加即 ld.lld: undefined symbol。
# 本游戏取 **no-op 桩**，依据＝与已验收的 1.5.3 legacy 包行为一致：
#   · 1.5.3 星空包走 legacy Main.cpp，grep 实证该文件**从不引用**这两个符号
#     ⇒ 当时既未装 seccomp、也未屏蔽致命信号、更无 payload 早期 dex 注入；
#     用户实测四功能生效、ACE（libtprt）未击杀，故 no-op 等价于当时行为。
#   · 交付形态＝APK 内 classes7.dex + Manifest 的 StarryInitProvider（provider-only），
#     不是 payload 形态；A 类的 FgoBoot 早期注入会去找 assets/starry.dex，属另一形态。
#   · ACE 引擎本轮零升级（libtersafe2/libtprt 与 1.5.3 字节级同尺寸），无新增击杀面。
LOCAL_SRC_FILES += Games/NoAceStubs.cpp

ifdef STARRY_TRACE
LOCAL_CFLAGS    += -DSTARRY_TRACE=1
endif

ifdef STARRY_DIAG
LOCAL_CFLAGS    += -DSTARRY_DIAG=$(STARRY_DIAG)
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
