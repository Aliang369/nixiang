#!/usr/bin/env python3
"""战斗合同检查：① 源码常量/字段名/地址口径；② 纯计算边界 fixture。

3.2（2026-11-24）新增段：ApplyHp 的 argBase 五槽 +0/+8/+16/+24/+32、argc 双口径（4=不含 this
的 metadata 口径 / 5=含 this 的槽位口径）、3.2 IL 锚点（rva 0xFF20C / body 1594B / get_isGod
IL_021C / 首次 set_curHp IL_0249）。2.4 的旧值（body 1265B / IL_0173 / IL_0197）作为历史分支
必须继续留在注释里 —— 两个版本门限不同，删掉就丢了对照。

3.3（2026-11-25）新增段：挂点常量 show 0x1CFF134 / interp 0x19F8A30 / size 66,297,952、探针表 {33, kOff33Show}
与三点取二分支、两处版本门放行 33、ApplyHp 3.3 锚点（rva 0x104200 / body 1610B / isGod IL_021E /
首次 set_curHp IL_024B）、argBase 五槽不变；3.2 与 2.4 的旧值整段保留作历史分支。

3.5（2026-11-28）新增段：挂点常量 show 0x1D04B30 / interp 0x19FE258 / timescale 0x37CA8F0 /
size 66,377,504（sha256 26d210e1…4f6c）、探针**表首**插 {35, kOff35Show}、两处版本门放行 35、
ApplyHp 3.5 锚点（rva 0x10B848 / body 1610B / fat 12 / maxstack 5 / locals 8 / 首个 set_curHp IL_024B /
第三处 IL_042C / OnDie.Invoke IL_0542 / blt IL_0282）、IL_04BE 的 neg 只属训练假人（不在 curHp 路径）、
访问器 RVA 四行、反篡改第三方法名 **VerifyPackageContents**（file 0x2B7C680）。

注意 3.5 与 3.3 的 show/interp 首字**完全相同**：首字判据已无区分力，只有「地址表 + size」能定版本
⇒ 本脚本同时断言 3.3 分支仍在（删掉 3.3 就丢了 size 66,297,952 那一侧的证据）。
「审计/特权未调用」一项按**调用形态**判（不出现 "get_isGod" 字符串字面量、不出现 get_isGod( 调用），
注释里记录访问器名字（合同需要）不算调用。

注意边界：①是静态文本约束（防"改一处忘一处"），②是规则级 fixture；
两者都**不能**替代运行时验证（真机未测）。
"""
import math
import pathlib
import sys

CPP = pathlib.Path("mod菜单/starry-mod-menu/app/src/main/jni/Games/旷野征途Mod.cpp")
t = CPP.read_text(encoding="utf-8")
# 3.5（2026-11-28）：早期装钩靠 Entry 的 .onLoad 接线，一并纳入断言（缺文件则判失败）
ENTRY = pathlib.Path("mod菜单/starry-mod-menu/app/src/main/jni/Games/旷野征途Entry.cpp")
e = ENTRY.read_text(encoding="utf-8") if ENTRY.exists() else ""

checks = [
  ("param 索引常量", "constexpr int kIdxAtker = 0;" in t and "constexpr int kIdxDmg   = 2;" in t),
  ("槽位常量", "constexpr int kSlotDmg  = 24;" in t and "constexpr int kSlotThis = 0;" in t),
  ("调用 get_param 用常量", "p_mgetParam(mi, kIdxAtker)" in t and "p_mgetParam(mi, kIdxDmg)" in t),
  ("读槽用常量", "(char*)argBase + kSlotDmg" in t and "(char*)argBase + kSlotThis" in t),
  ("无裸索引残留", "p_mgetParam(mi, 1)" not in t and "p_mgetParam(mi, 3)" not in t),
  ("player backing field", "<player>k__BackingField" in t),
  ("Instance 静态字段", 'class_get_field(mgr, "Instance")' in t),
  ("hook 用 file offset", "0x19a5004u" in t and "0x19a9004u" not in t),
  # 「不调用」按调用形态判：字符串字面量（按名解析）与调用括号都不得出现；注释里写名字不算调用。
  ("审计/特权未调用", "CombatDamageAudit::" not in t and "PrivilegeService::" not in t
                      and '"get_isGod"' not in t and "get_isGod(" not in t),
  # ---- 3.3 合同（2026-11-25；底包 libil2cpp 66,297,952 B / HotUpdate sha256 3c21663c…34280）----
  ("3.3 挂点常量", "kOff33Show   = 0x1CFF134u" in t and "kOff33Interp = 0x19F8A30u" in t
                   and "kSize33Lib = 66297952ull" in t),
  # 3.5 起 3.3 不再是表首（3.5 首插表首），只断言 3.3 仍在探针表里且分支还在。
  ("3.3 探针表 + 分支", "{33, kOff33Show}" in t and "if (pr.ver == 33) {" in t
                          and "verprobe 3.3 cond" in t),
  ("3.3 版本门放行", "verHook == 33" in t and "g_libVer.load() == 33" in t
                    and "(ver == 33) ? kOff33Timescale" in t),
  ("3.3 ApplyHp 合同锚点", "0x104200" in t and "body 1610 B" in t
                           and "IL_021E" in t and "IL_024B" in t),
  ("3.3 argBase 五槽不变", "argBase 五槽 +0/+8/+16/+24/+32" in t),
  # ---- 3.5 合同（2026-11-28 静态实测；底包 libil2cpp 66,377,504 B / sha256 26d210e1…4f6c）----
  ("3.5 挂点常量", "kOff35Show   = 0x1D04B30u" in t and "kOff35Interp = 0x19FE258u" in t
                   and "kOff35Timescale = 0x37CA8F0u" in t and "kSize35Lib = 66377504ull" in t),
  ("3.5 探针表首 + 分支", "{{35, kOff35Show}" in t and "if (pr.ver == 35) {" in t
                          and "verprobe 3.5 cond show=%d interp=%d size=%d/%llu" in t
                          and "fsz35 == kSize35Lib" in t),
  ("3.5 两处版本门放行", "verHook == 35" in t and "g_libVer.load() == 35" in t
                        and "(ver == 35) ? kOff35Timescale" in t
                        and "(ver == 35) ? kOff35Interp" in t
                        and "(ver == 35) ? kOff35Show" in t
                        and "(ver == 35) ? 0x1D048A8u" in t),
  ("3.5 ApplyHp 合同锚点", "rva 0x10B848" in t and "body 1610 B" in t and "fat 头 12 B" in t
                           and "maxstack 5" in t and "locals 8" in t and "argc 4" in t
                           and "IL_024B" in t and "IL_0265" in t and "IL_042C" in t
                           and "IL_0542" in t and "IL_0282" in t),
  ("3.5 neg 只属训练假人", "IL_04BE" in t and "不在 curHp 路径上" in t),
  ("3.5 访问器 RVA", "get_curHp 0x10EA86/7" in t and "set_curHp 0x10EA8E/8" in t
                     and "get_isGod 0x10DE1D/24" in t and "get_maxHp 0x10E920/15" in t),
  ("3.5 反篡改方法名", "VerifyPackageContents" in t and "0x2B7C680" in t
                       and "VerifyReleaseManifest" in t),
  # 3.5 反篡改绕行钩（2026-11-28）：冷启 ~1.4s 自杀 ⇒ 两个 bool 判定点无条件钉 true
  ("3.5 反篡改绕行钩常量/序言", "kOff35VerifySigner   = 0x2B7BE58u" in t
                                and "kOff35VerifyContents = 0x2B7C680u" in t
                                and "kInsn35VerifySigner   = 0xD10183FFu" in t
                                and "kInsn35VerifyContents = 0xD101C3FFu" in t),
  ("3.5 反篡改两钩都装", 'HookAtRva("AntiTamper"' in t and "Hooked_VerifyCurrentSigner" in t
                        and "Hooked_VerifyPackageContents" in t
                        and "antitamper: skip ver=%d" in t
                        and "MODHIT tag=AntiTamper which=%s" in t),
  ("反篡改/退出探针都装在 lib ready、不在懒装门内",
   "    InstallAntiTamperHooks();\n    InstallExitProbeHooks();" in t
   and t.count("InstallAntiTamperHooks()") == 2 and t.count("InstallExitProbeHooks()") == 2
   and t.index("    InstallExitProbeHooks();")
       < t.index("    SyncHooks();", t.index("    InstallExitProbeHooks();"))),
  # 3.5 退出探针（2026-11-28）：反篡改两钩 MODHIT=0 ⇒ 抓真正发起退出的方法（Application.Quit 路径）
  ("3.5 退出探针常量/序言", "kOff35RequestExit  = 0x2B8E1DCu" in t
                            and "kOff35RevConfirm   = 0x2B8E620u" in t
                            and "kInsn35RequestExit = 0xD10183FFu" in t
                            and "kInsn35RevConfirm  = 0xD10143FFu" in t),
  ("3.5 退出探针日志格式", r'exit-probe RequestExit reason=\"%s\"' in t
                          and "exit-probe caller rva=0x%llX" in t
                          and "MODHIT tag=ExitProbe which=%s" in t
                          and "exit-probe RecordRevisionConfirmation stage=" in t),
  ("退出探针 RequestExit 不调 orig / RevConfirm 照调",
   "if (g_origRequestExit)" not in t
   and "if (g_origRevConfirm) g_origRevConfirm(stage, details, mi);" in t),
  # 第二轮退出探针（2026-11-28）：RequestExit 也 MODHIT=0 ⇒ 主嫌 ExitModifiedPackage(killProcess)
  ("3.5 killProcess/失败阶段探针常量", "kOff35ExitModified  = 0x2B7BB58u" in t
                                       and "kOff35ShowPkgErr    = 0x2BB5020u" in t
                                       and "kOff35ShowBlocking  = 0x2BBCD00u" in t
                                       and "kInsn35ExitModified = 0xF81D0FFEu" in t),
  ("3.5 退出探针七钩都装", t.count('HookAtRva("ExitProbe"') == 7
                           and "Hooked_ExitModifiedPackage" in t
                           and "Hooked_ShowPackageIntegrityError" in t
                           and "Hooked_ShowBlockingError" in t
                           and "Hooked_ReadInstalledIdentity" in t
                           and "Hooked_LambdaB0" in t),
  # 返回值探针（2026-11-28 第三轮）：ReadInstalledIdentity 自身调 VerifyCurrentSigner(@+0x580)
  # ⇒ 两钩 0 命中说明它提前 return false，必须看它的 ret/out 参数
  ("3.5 返回值探针常量/序言", "kOff35ReadIdentity = 0x2B7ABF8u" in t
                              and "kInsn35ReadIdentity = 0xD10243FFu" in t
                              and "kOff35LambdaB0   = 0x2B7D9A8u" in t
                              and "kInsn35LambdaB0  = 0xA9410408u" in t),
  ("3.5 返回值探针先调 orig",
   "const bool ret = g_origReadIdentity(pApk, pVc, mi);" in t
   and "const bool ret = g_origLambdaB0(self, mi);" in t
   and "exit-probe ReadInstalledIdentity ret=%d apk=" in t
   and "exit-probe lambda b__0 ret=%d" in t),
  # 2026-11-28 第五轮：时序（钩子晚于校验）+ 根因（ReadInstalledIdentity 直接 false）双修
  ("early-init 线程（5ms 轮询，抢在 <Start> 前）",
   "KyztEarlyInitThread" in t and "usleep(5000);" in t
   and "for (int i = 0; i < 2000; ++i)" in t
   and "early-init: lib appeared after %lld ms" in t
   and "isLibraryLoaded(TPL_TARGET_LIB)" in t),
  ("TryInit 防重入 CAS", "g_initStarted.compare_exchange_strong" in t
                          and "g_initStarted.store(false);" in t),
  ("Entry 接上 onLoad", ".onLoad = 旷野征途_OnLoad" in e
                         and "void 旷野征途_OnLoad(JavaVM* vm)" in t),
  ("ReadInstalledIdentity 强制通过", "FORCE-PASS" in t and "force=%d" in t
                                     and 'Il2Cpp::ResolveExport(\"il2cpp_string_new\")' in t
                                     and "*(long long*)pVc = 35;" in t),
  ("killProcess 钩不调 orig、另两钩照调", "if (g_origExitModified)" not in t
                                          and "if (g_origShowPkgErr) g_origShowPkgErr(ex, mi);" in t
                                          and "if (g_origShowBlocking) g_origShowBlocking(stage, ex, mi);" in t),
  ("3.5 killProcess/阶段日志格式", "exit-probe ExitModifiedPackage caller rva=0x%llX" in t
                                   and "exit-probe ShowBlockingError stage=" in t
                                   and "exit-probe: skip ver=%d" in t),
  ("3.3/3.2/2.4/1.8 历史分支未删", "{33, kOff33Show}" in t and "{32, kOff32Show}" in t
                                   and "{24, 0x1CCE6CCu}" in t and "{18, 0x1C8A9ACu}" in t
                                   and "body 1594 B" in t and "body=1265 B" in t
                                   and "kOff33Timescale = 0x37B9344u" in t),
  ("3.2 历史分支保留", "body 1594 B" in t and "IL_021C" in t and "IL_0249" in t),
  # ---- 3.2 合同（2026-11-24 从 HotUpdate.decrypted.dll 反推；sha256 812e6563…d551b0）----
  ("3.2 argBase 五槽", "constexpr int kSlotSeat = 16;" in t and "constexpr int kSlotOnDie= 32;" in t),
  ("3.2 argc 双口径放行", "constexpr int kApplyHpArgc = 4;" in t
                          and "kApplyHpArgcWithThis = 5;" in t),
  ("argc 无字面量硬编码", "argc == 4" not in t and "argc == 5" not in t),
  ("3.2 ApplyHp 合同锚点", "0x0FF20C" in t and "body 1594 B" in t
                           and "IL_021C" in t and "IL_0249" in t),
  ("2.4 历史合同保留", "body=1265 B" in t and "IL_0173" in t and "IL_0197" in t),
  ("落血语义注释", "set_curHp(curHp + intDmg)" in t),
]

CLAMP = -2.1474835e9

def apply_x100(d):
    """镜像 cpp：仅当 d<0 时乘 100，结果 < CLAMP 则钳位；NaN/0/正数原样返回。"""
    if not (d < 0.0):
        return d
    m = d * 100.0
    return CLAMP if m < CLAMP else m

FIXTURES = [
    (-1.0, -100.0),
    (-0.5, -50.0),
    (-1.0e8, CLAMP),
    (-2.0e7, -2.0e9),   # -2e7*100 = -2e9 > CLAMP ⇒ 不钳位（边界外）
    (50.0, 50.0),
    (0.0, 0.0),
    (-0.0, -0.0),
]

def main():
    bad = [n for n, ok in checks if not ok]
    for n, ok in checks:
        print(("PASS " if ok else "FAIL ") + n)
    bad_fx = []
    for src, want in FIXTURES:
        got = apply_x100(src)
        if got != want:
            bad_fx.append((src, got, want))
    nan_out = apply_x100(float("nan"))
    if not math.isnan(nan_out):
        bad_fx.append(("NaN", nan_out, "NaN"))
    print(("PASS " if not bad_fx else "FAIL ") + "FIXTURE x100/clamp boundary (%d cases)" % (len(FIXTURES) + 1))
    if bad_fx:
        print("   mismatches:", bad_fx)
    ok = (not bad) and (not bad_fx)
    print("结论：" + ("一致" if ok else "不一致"))
    return 0 if ok else 1

if __name__ == "__main__":
    sys.exit(main())
