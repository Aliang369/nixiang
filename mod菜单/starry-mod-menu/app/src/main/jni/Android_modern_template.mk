# ============================================================
#  Android_modern_template.mk — 新游戏私有构建模板（modern 注册表入口）
#
#  生成：new_game_mod.sh 自动复制为 Android_<tag>.mk 并替换 {GAME}
#  手工使用：复制本文件 → 替换两处 {GAME} → 差异区按注释调整
#
#  构建铁律（并行会话互不踩 / obj 不污染）：
#    cd app/src/main/jni && ndk-build NDK_PROJECT_PATH=../.. \
#      APP_BUILD_SCRIPT=./Android_<tag>.mk NDK_APPLICATION_MK=./Application.mk \
#      NDK_OUT=../../obj_<tag> NDK_LIBS_OUT=../../libs_<tag> -j
#  切游戏永远不需要清共享 app/obj（各游戏独立 obj_<tag>/）
#
#  ★ABI（2026-10-31 公主连结雷电轮定，默认双 ABI 就绪）
#    出包前先问底包：python3 04_scripts/_common/detect_abis.py <底包.apk>  →  例 "arm64-v8a x86_64"
#    然后把该结果原样喂给 APP_ABI="<结果>"（一次构建同时产出两套 so）：
#      · 底包带 lib/x86_64 ⇒ 雷电/模拟器**原生 x86_64** 直跑，必须出 x86_64 so，
#        否则进程内 dlopen("libstarry_magic.so") 恒失败 → 菜单显示「暂无功能列表（native 未就绪）」
#        （2026-10-31 PCR 实证：UnsatifsfiedLinkError: dlopen failed: library "libstarry_magic.so" not found）
#      · 底包只有 arm64 ⇒ 只出 arm64；雷电用 houdini 转译执行 arm64，能跑但 Dobby inline hook
#        稳定性不保证（无限升级崩、伊莫「能装但长期性未知」）⇒ 这类不要承诺「模拟器自动适配」
#    so 落位：每个 ABI 各自 `app/src/main/jniLibs/<abi>/libstarry_magic.so`（Gradle 只打包 jniLibs）
#    可移植性前提：挂点必须**by-name / RVA-free**（写死 RVA、KittyAsm 运行时汇编的 Mod 换 ABI 即废）
# ============================================================
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# Keystone 已移除（20260909 实测零调用死重 43KB：KittyAsm 是唯一使用者而其从未编译）。
# 将来需要 KittyAsm 运行时汇编时再加回 prebuilt 声明并在下方 LOCAL_STATIC_LIBRARIES 加 Keystone。

# wxsj_dll prebuilt 已移除（2026-10-31）：`Games/WxsjPatchedDll*.a` 只属于无限升级，
# 且被 .gitignore 忽略、大多数 checkout 里没有 ⇒ 模板一被复制就 "LOCAL_SRC_FILES points to a
# missing file" 直接 abort（绿梦当年只得手工删掉这一段）。需要它的游戏自己加回。

include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

# -fno-emulated-tls【2026-10-05 绝区零事故，默认带上】：
#   Android clang 默认 emutls，任何 TU 用了 thread_local 就会生成 UND __emutls_get_address；
#   而该符号只有 NDK 版 libc++_shared.so 弱定义 —— 游戏自带的旧 libc++_shared / MuMu 的
#   arm64 垫片 libc 都没有 ⇒ dlopen 失败、菜单静默降级 Java 版、hook 全哑（日志特征：
#   `StarryNativeLoad: loadLibrary starry_magic failed ... cannot locate symbol "__emutls_get_address"`）。
#   自检：llvm-nm -D --undefined-only <so> | grep emutls（应为空）。
# --exclude-libs,ALL 符号卫生（libChiy 类事故）：禁止静态库成员符号（Dobby* 等）进动态符号表。
LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions -fno-emulated-tls
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions -fno-emulated-tls
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all,--exclude-libs,ALL
# FW-13（2026-10-16）：**必须强制保留** Includes/Utils.cpp 里烘焙的 OTA 构建戳。
#   modern 入口 MainModern.cpp 零引用 kStarryBuildStamp（唯一引用者是 legacy Main.cpp），
#   上面这行 --gc-sections 会把它整段回收 ⇒ so 内没有 STARRYSTAMP1:EPOCH= ⇒
#   Java OtaSoStamp.apkStamp()=0 ⇒ PlayOtaManager 判据恒假 ⇒ 覆盖安装后永不清 starry_ota
#   旧 so ⇒ 玩家跑旧 so（小米 byron 实测「打开就闪退」）。共享源已加 retain，此处再兜一层：
LOCAL_LDFLAGS   += -Wl,--undefined=kStarryBuildStamp
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ==== 游戏差异区（复制/生成后只改这里）====
# 源文件三件套：现代入口 + 游戏 Mod + 注册表 Entry
# Unity il2cpp 系保持下面默认（含 Il2CppApi）；
# Cocos 系去掉 Unity/Il2CppApi.cpp，按需增 Games/LtzjAceSoft.cpp 等过检配套
LOCAL_SRC_FILES := MainModern.cpp Games/{GAME}Mod.cpp Games/{GAME}Entry.cpp Unity/Il2CppApi.cpp

# MainModern.cpp 的 JNI_OnLoad 无条件引用 SignalQuarantine_Start /
# FgoBoot_InjectEarlyFromOnLoad，不加本文件即 ld.lld: undefined symbol。
# 默认按 B 类（无 libtprt.so / 无 ACE，菜单 dex 由 Manifest 的 StarryInitProvider
# 拉起）用 no-op 桩：不装 seccomp、不屏蔽致命信号、不做早期 dex 注入。
# A 类（包内含 libtprt.so / ACE 加固）改为下面两行，删掉 NoAceStubs.cpp：
#   LOCAL_SRC_FILES += Games/SignalQuarantine.cpp Games/FgoBoot.cpp
# 参考已落地样例：Android_jfqs.mk（B 类）、Android_jjdm.mk / Android_mygwzd.mk（A 类）。
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

LOCAL_STATIC_LIBRARIES := Dobby

include $(BUILD_SHARED_LIBRARY)
