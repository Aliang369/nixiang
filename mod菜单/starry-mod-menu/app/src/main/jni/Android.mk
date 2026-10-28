LOCAL_PATH := $(call my-dir)

# ============================================================
#  ⚠️ LEGACY 双轨告示（2026-09-09 起，勿删）
#  1. 接新游戏禁止改 Main.cpp / 本文件的激活段：
#     用 scripts/new_game_mod.sh <Game> 生成 Mod+Entry+私有 mk，
#     走 MainModern.cpp（注册表模式，共享文件零接触）。
#  2. 存量游戏继续用本文件；重新出包时顺手迁移注册表：
#     迁移 = 写 Entry + 切私有 mk + 删 Main.cpp 里该游戏的 #if 分支组。
#  3. Main.cpp 严禁改动（含注释）：OBFUSCATE 宏用 __LINE__ 做混淆
#     种子，行号偏移即产物漂移（20260909 实测注释 +12 行 md5 变）。
#  4. 堆积病史：Main.cpp 曾因每游戏 6~13 处 #if 堆叠产出过
#     Wxsj 残留混入 Endfield、CWJH 死分支等复制粘贴 bug。
# ============================================================

# ============================================================
#  Dobby (Hook 框架) — 静态库
# ============================================================
include $(CLEAR_VARS)
LOCAL_MODULE := Dobby
LOCAL_SRC_FILES := Dobby/$(TARGET_ARCH_ABI)/libdobby.a
include $(PREBUILT_STATIC_LIBRARY)

# ============================================================
#  Keystone (汇编引擎) — 静态库
# ============================================================
include $(CLEAR_VARS)
LOCAL_MODULE := Keystone
LOCAL_SRC_FILES := KittyMemory/Deps/Keystone/libs-android/$(TARGET_ARCH_ABI)/libkeystone.a
include $(PREBUILT_STATIC_LIBRARY)

# ============================================================
#  WXSJ 内置 patched targetfuntional.dll（objcopy 静态库）
#  v7.14 单一版本：只内置官方 hotfix2 / 2,030,080 对应的那份补丁 dll。
#  （v7.13 的 wxsj_dll_v711 / 第二份内置已废弃：官方灰度切换窗口已结束）
# ============================================================
include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll_hf2
LOCAL_SRC_FILES := Games/WxsjPatchedDll_hf2.a
include $(PREBUILT_STATIC_LIBRARY)
include $(CLEAR_VARS)
LOCAL_MODULE := wxsj_dll_hf3
LOCAL_SRC_FILES := Games/WxsjPatchedDll_hf3.a
include $(PREBUILT_STATIC_LIBRARY)

# ============================================================
#  星光魔法 MOD 菜单 — 主模块
# ============================================================
include $(CLEAR_VARS)

LOCAL_MODULE    := starry_magic

LOCAL_CFLAGS    := -w -s -Wno-error=format-security -fvisibility=hidden -fpermissive -fexceptions
LOCAL_CPPFLAGS  := -w -s -Wno-error=format-security -fvisibility=hidden -std=c++17 -fpermissive -fexceptions
LOCAL_LDFLAGS   := -Wl,--gc-sections,--strip-all
LOCAL_LDLIBS    := -llog -landroid

LOCAL_ARM_MODE  := arm

# ========== 切游戏：只启用下面「一组」里的一行 ==========
# 鉴权 game_id 由 GameConfig.h 根据 STARRY_GAME_* 自动映射，勿再写 STARRY_AUTH_GAME_ID。
# 正式游戏还需 ENABLE_*_HOOK + LOCAL_SRC_FILES 编入对应 Games/*Mod.cpp（见下方注释）。
#
# 自测菜单壳（无游戏逻辑）
# LOCAL_CFLAGS    += -DSTARRY_GAME_SELF
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_CSTJJS
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_NEON
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_HZSJ
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_MILTHM
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_ZXR
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_ZMXY
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WZYM
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_MATCH2
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_LTZJ
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_YWGN
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_PCR
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GYWS
# 造梦无双 1.33
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_ZMWS

# LOCAL_SRC_FILES := Main.cpp Games/HzsjMod.cpp Unity/Il2CppApi.cpp
# 自测：无 Games/*Mod（纯菜单 UI）→ LOCAL_SRC_FILES := Main.cpp
# cstjjs：Games/CstjjsMod.cpp + ENABLE_UNITY_HOOK
# ywgn：Games/YwgnMod.cpp + Unity/Il2CppApi.cpp
# neon：Games/NeonMod.cpp（纯 RVA + Dobby，不用 Il2CppApi）
# hzsj：Games/HzsjMod.cpp + Unity/Il2CppApi.cpp
# pcr：Games/PcrMod.cpp + Unity/Il2CppApi.cpp（公主连结）
# 孤勇无双：Games/GywsMod.cpp + Unity/Il2CppApi.cpp
# LOCAL_SRC_FILES := Main.cpp Games/GywsMod.cpp Unity/Il2CppApi.cpp
# 造梦无双：Games/ZmwsMod.cpp + ENABLE_COCOS_HOOK
# LOCAL_SRC_FILES := Main.cpp Games/ZmwsMod.cpp

# zjmc：Games/ZjmcMod.cpp（纯 RVA + Dobby）
# milthm：Games/MilthmMod.cpp
# 造梦西游：Games/ZmxyMod.cpp + ENABLE_COCOS_HOOK
# 镇邪人：Games/ZxrMod.cpp + ENABLE_COCOS_HOOK
# 消消英雄2：Games/Match2Mod.cpp + ENABLE_UNITY_HOOK
# 物质与魔法：Games/WzymMod.cpp + ENABLE_UNITY_HOOK
# 雷霆：Games/LtzjMod.cpp + ENABLE_COCOS_HOOK

# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_HZSJ
# LOCAL_SRC_FILES := Main.cpp Games/HzsjMod.cpp Unity/Il2CppApi.cpp
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_PCR
# LOCAL_SRC_FILES := Main.cpp Games/PcrMod.cpp Unity/Il2CppApi.cpp
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZHMR
# LOCAL_SRC_FILES := Main.cpp Games/ZhmrMod.cpp Unity/Il2CppApi.cpp
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GYWS
# LOCAL_SRC_FILES := Main.cpp Games/GywsMod.cpp Unity/Il2CppApi.cpp
# 弓箭传说2 1.1.8
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
# LOCAL_SRC_FILES := Main.cpp Games/Archero2Mod.cpp Unity/Il2CppApi.cpp
# 弓箭传说2 facai 国际 1.0.1（独立 game_id=gjc2intl，勿与 gjc2 混编）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2INTL
# LOCAL_SRC_FILES := Main.cpp Games/Archero2IntlMod.cpp Unity/Il2CppApi.cpp
# milthm 6.0.5 TapTap · Unity IL2CPP 自动跳舞
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_MILTHM
# LOCAL_SRC_FILES := Main.cpp Games/MilthmMod.cpp
# 生存33天 1.0.23 TapTap
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SC33
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# ifdef SC33_SPLICE_ENABLED
# LOCAL_CFLAGS    += -DSC33_SPLICE_ENABLED=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/Sc33Mod.cpp
# 伯吉的温馨厨房 1.0.1 TapTap · 金币/广告券
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_BJDW
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/BjdwMod.cpp
# 无悔华夏 3.8.280 九游 · 延迟 hook（默认零挂点，防启动闪退）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHX
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp
# 追迹迷城 1.0.20260824 TapTap
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
# LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp
# 星光魔法模板 App 自测（纯菜单 UI，game_id=self）
# LOCAL_CFLAGS    += -DSTARRY_GAME_SELF
# LOCAL_SRC_FILES := Main.cpp
# 王国大作战：前线 1.1.7646 TapTap · HybridCLR+xLua Trace
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_QLFH
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/QlfhMod.cpp
# 战魂铭人 3.5.0 TapTap · Unity IL2CPP + HybridCLR
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZHMR
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/ZhmrMod.cpp Unity/Il2CppApi.cpp
# 公主连结 11.7.2 · GC 弱句柄敌我校验 + 属性 999999 输入框
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_PCR
# LOCAL_SRC_FILES := Main.cpp Games/PcrMod.cpp Unity/Il2CppApi.cpp
# 无悔华夏 3.8.280 九游 · 2026-08-29 Round2（资源车路线 + 身份重建）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHX
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp
# 无悔华夏 3.8.280 好游快爆 com.syyx.whrhx · 2026-08-30（KB RVA 表 + 捕获式身份，game_id=whhxkb）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHXKB
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp
# 弓箭传说2 1.1.8 国服 · HybridCLR 偷技能/隐身/全屏
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/Archero2Mod.cpp Unity/Il2CppApi.cpp
# 蜀汉传卧龙篇 100.22.0 · 自研 Cocos2d-x C++（libMyGame.so）· 已终止（2026-09-02，见 10_docs/蜀汉传卧龙篇/）
# LOCAL_CFLAGS    += -DSTARRY_GAME_SHC
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_CFLAGS    += -DSHC_EXIT_FORENSICS
# LOCAL_SRC_FILES := Main.cpp Games/ShcMod.cpp
# 无悔华夏 3.8.280 好游快爆 · Trace 诊断版
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHXKB
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp
# 黑暗领域 1.0 · Cocos Creator 3.8 / libcocos.so（免广告+技能表）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_HALY
# LOCAL_SRC_FILES := Main.cpp Games/HalyMod.cpp
# 弓箭传说2 1.1.8 国服 TapTap（同上，勿重复激活）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
# LOCAL_SRC_FILES := Main.cpp Games/Archero2Mod.cpp Unity/Il2CppApi.cpp
# 遗弃之地 0.1.57 TapTap · Cocos3.8.5 JS明文 四功能（2026-09-07 接入，零 native hook）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_YQZD
# LOCAL_SRC_FILES := Main.cpp Games/YqzdMod.cpp
# 动物餐厅 12.39 · Cocos 免广告/免内购（2026-09-07 版本更新，锚点全命中）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_DWCT
# LOCAL_SRC_FILES := Main.cpp Games/DwctMod.cpp
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GJC2
# LOCAL_SRC_FILES := Main.cpp Games/Archero2Mod.cpp Unity/Il2CppApi.cpp
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_LTZJ -DSTARRY_CAVE_INTEGRITY_SOFT
# LOCAL_SRC_FILES := Main.cpp Games/LtzjMod.cpp Games/LtzjAceSoft.cpp
# 长生天机降世 0.9.10
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_CSTJJS
# LOCAL_SRC_FILES := Main.cpp Games/CstjjsMod.cpp Unity/Il2CppApi.cpp
# 追迹迷城 1.0.20260816
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
# LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp
# 动物餐厅 12.38 · 热更后 runtime 免内购（OTA so）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_DWCT
# LOCAL_SRC_FILES := Main.cpp Games/DwctMod.cpp
# 雷霆战机 1.25.31 · 零 Root 冷启（不改 cocos；CaveBoot/AceSoft 仅分析用）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_LTZJ
# LOCAL_SRC_FILES := Main.cpp Games/LtzjMod.cpp Games/LtzjAceSoft.cpp
# 山河旅探 1.4.38 · ILRuntime 热更 DLL 解锁章节/服饰
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_SHLT
# LOCAL_SRC_FILES := Main.cpp Games/ShltMod.cpp Unity/Il2CppApi.cpp
# 动物餐厅 12.38 · 热更 jsc 门控在 Java DwctIapGate；so 只同步 __starryNoIap
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_DWCT
# LOCAL_SRC_FILES := Main.cpp Games/DwctMod.cpp
# 弹力果冻 2.4.0 · HybridCLR 跳板 HpHandle/无敌/全屏秒杀/无限技能
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_TLGD
# LOCAL_SRC_FILES := Main.cpp Games/TlgdMod.cpp Unity/Il2CppApi.cpp
# 掌门下山 1.0.2 · 结案：服务端权威战斗，客户端无敌/秒杀不可行（文档见 10_docs/掌门下山）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZMXS
# LOCAL_SRC_FILES := Main.cpp Games/ZmxsMod.cpp Unity/Il2CppApi.cpp
# 独奏骑士 1.1.760 · HybridCLR 跳板 calTotalDamage/无敌/无限技能
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_DZQS
# LOCAL_SRC_FILES := Main.cpp Games/DzqsMod.cpp Unity/Il2CppApi.cpp
# 孤勇无双 1.0.3 · SkillEffectSystem ReceiveDamage/OnDamageEffect + HpComponent.DeductHp
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_GYWS
# LOCAL_SRC_FILES := Main.cpp Games/GywsMod.cpp Unity/Il2CppApi.cpp
# 山海幻境 3.7 · Cocos GXR/GXD 十倍/无敌/百倍
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SHHJ
# LOCAL_SRC_FILES := Main.cpp
# 音舞光年 0.300.42 · Trace 证实出分 invoke MP + cfg.isAutoPlay 实例
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_YWGN
# LOCAL_SRC_FILES := Main.cpp Games/YwgnMod.cpp Unity/Il2CppApi.cpp
# 追迹迷城 1.0.20260821
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
# LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp
# 镇邪人 1.0.3 · Cocos2djs se::evalString 战斗拼接 + 原生菜单
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_ZXR
# LOCAL_SRC_FILES := Main.cpp Games/ZxrMod.cpp
# 消消英雄2 1.2.28 · xLua AN_1/2/3（2026-10-08 激活）；Trace 用 STARRY_LUA_DUMP_TRACE=1 + APP_CFLAGS
# 2026-10-10 切走（追迹迷城 1.0.20261009 出包），恢复时取消下面两行注释
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_MATCH2
# LOCAL_SRC_FILES := Main.cpp Games/Match2Mod.cpp Unity/Il2CppApi.cpp
# 超维进化 2.2（cwjh）— Cocos2djs se::evalString splice（20261006 0.3.16 热更：doDmg 首参 r→n，定义签名扫描）
# 每次 splice 逐功能打印 f0-f9 hits；miss 一律 LOGW
# 2026-10-08 切走（消消英雄2 1.2.28 出包）→ 恢复时取消下面两行注释
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_CWJH
# LOCAL_SRC_FILES := Main.cpp Games/CwjhMod.cpp
# 我嘎嘎乱杀 1.3.6 · BattleUtility CalculateDamage/ApplyDamage + BattleTimeMgr
# Trace：ndk-build 传 STARRY_TRACE=1
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WGGLS
# LOCAL_SRC_FILES += Main.cpp Games/WgglsMod.cpp Unity/Il2CppApi.cpp
# 追迹迷城 1.0.20261009 / vc101201 TapTap（2026-10-10 重 dump＋RVA 重定位见 ZjmcMod.cpp）
# 上一版 0912 的 RVA 已全部作废；五项功能与 17 个 Hook 目标签名未变
# 2026-10-12 切走（无限升级 12.10701/vc1644 星空版出包），恢复时取消下面两行注释
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
# LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp
# 我的勇者 7.6.1（wdyz）— Battle 结算钩：五倍攻击(敌 ×5)/无敌(HeroSettle 0)
# 2026-09-07 星光自注入验证（不用 frida；TryEnterCd 已证死路，勿挂技能簇）
# 2026-09-20 7.6.1(vc203) 重 dump：RVA 全漂移 → 双路径按名兜底 + 新 RVA 回填
# 2026-09-21 切走（超维进化 0.3.x 热更重校出包）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WDYZ
# LOCAL_SRC_FILES := Main.cpp Games/WdyzMod.cpp Unity/Il2CppApi.cpp
# TryResolve 已加 Il2CppReady 门控（init 期半初始化 image SIGSEGV 实锤）
# 长生天机降世 1.0.3 — EnterFrameFromInterpreter file 0x2657350（VA 0x265b350）
# 2026-10-14 从真机 1.0.3 官方包重新取证：函数入口/两处 bl/帧压入 stp 与 1.0.2 同址同目标
#   （唯一版本相关常量 FILE_ENTER_INTERP 未变）；CstjjsMod 的 Il2CppReady gating 原样复用
# 历史：1.0.2 2026-09-29 / 1.0.1 file 0x2657084 / 0.9.10 file 0x2656920
# 2026-10-26 切走（无限升级 12.10701 vc1644 星空版复刻出包），恢复时取消下面两行注释
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_CSTJJS
# LOCAL_SRC_FILES := Main.cpp Games/CstjjsMod.cpp Unity/Il2CppApi.cpp
# 偶像梦幻祭2 4.4.8684 — Es2Mod 探测 v1（dump-only：il2cpp 全量枚举→判定函数定位）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ES2
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/Es2Mod.cpp Unity/Il2CppApi.cpp
# 超维进化（cwjh）— Cocos2djs se::evalString splice（20260921 0.3.5 热更：doDmg 14 参/前缀动态匹配）
# 防御·抗性新增形态C锚点（o/c + StatusDef.getMinEdr）；回血回蓝×4、暴击+60pp、闪避+35pp
# 无限升级 12.10701 / vc1644 — 热更 dll 字节替换（2026-10-12 星空版出包，激活；走 scripts/wxsj_so_only.sh）
# 2026-09-23 白天：官方再推热更，dll 1960960 → 2030080 B；已用 DIAG 包抓到新 dll 并重打补丁，
#   kOrigDllLen 已改 2030080，DIAG 开关按约定移除（勿再打开，否则 release 构建被脚本拒绝）。
# 2026-10-12：现网热更已切到 hotfix3 2028544（清单 F12671617101211 / 2026-09-24），
#   WxsjMod.cpp 的 (len,fnv) 双变体表已含 hf3，故本次出包同时内置 hf2+hf3。
# 我的勇者 7.6.1（wdyz）— 2026-09-29 切走（长生天机降世 1.0.2 出包），恢复时取消下面两行注释
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WDYZ
# LOCAL_SRC_FILES := Main.cpp Games/WdyzMod.cpp Unity/Il2CppApi.cpp
# 独奏骑士 1.1.768：仅当环境变量 STARRY_GAME_DZQS=1 时覆盖默认目标，避免影响其他游戏的激活分支。
ifdef STARRY_GAME_DZQS
LOCAL_CFLAGS    := $(filter-out -DSTARRY_GAME_WDYZ,$(LOCAL_CFLAGS)) -DENABLE_UNITY_HOOK -DSTARRY_GAME_DZQS
LOCAL_SRC_FILES := Main.cpp Games/DzqsMod.cpp Unity/Il2CppApi.cpp
endif
# 无限升级 12.10701 / vc1644 — 2026-10-26 激活（官方原版 12.10701 复刻星空版；走 scripts/wxsj_so_only.sh）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WXSJ
# LOCAL_SRC_FILES := Main.cpp Games/WxsjMod.cpp
# 战魂铭人 3.7.0 / vc20680 — 2026-10-28 激活（TapTap 官方原版复刻星空版）
# 热更 GameLogic.dll 与 3.6.0 同一份（md5 7ecee205f25de60af3abec8796339994），依赖类/字段名全在位，
# ZhmrMod.cpp 零改动复用；证据 10_docs/战魂铭人/战魂铭人_3.7.0_复刻记录_20261028.md
# 20260908 SIGABRT 修复：类查齐后隔轮再读 offsets（热更 layout 中途 field_get_offset 必崩）
LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZHMR
LOCAL_SRC_FILES := Main.cpp Games/ZhmrMod.cpp Unity/Il2CppApi.cpp
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WxsjMod.cpp
# LOCAL_STATIC_LIBRARIES += wxsj_dll
# LOCAL_CFLAGS    += -DSTARRY_GAME_SELF
# LOCAL_SRC_FILES := Main.cpp Games/CwjhMod.cpp
# 山海幻境 3.8 · Cocos GXR/GXD 无敌/百倍（2026-09-06 版本更新）
# LOCAL_CFLAGS    += -DENABLE_COCOS_HOOK -DSTARRY_GAME_SHHJ
# LOCAL_SRC_FILES := Main.cpp Games/ShhjMod.cpp
# 终末地 1.5.3 · mod 菜单（v11：探索包——类普查+HP字段探针，数据写路线）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ENDFIELD
# LOCAL_SRC_FILES := Main.cpp Games/EndfieldMod.cpp Unity/Il2CppApi.cpp
# 无悔华夏 3.8.280 九游 · Trace 候选 RVA（WhrhxMod 纯 RVA）
# LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_WHRHX
# ifdef STARRY_TRACE
# LOCAL_CFLAGS    += -DSTARRY_TRACE=1
# endif
# LOCAL_SRC_FILES := Main.cpp Games/WhrhxMod.cpp Unity/Il2CppApi.cpp

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

LOCAL_STATIC_LIBRARIES := Dobby Keystone wxsj_dll_hf2 wxsj_dll_hf3

include $(BUILD_SHARED_LIBRARY)
