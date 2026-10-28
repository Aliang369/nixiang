# 长生天机降世 1.0.4 装机实测证据（2026-10-28，雷电 emulator-5554）

> 目的：原文日志（1.2 M / 14 k 行）不入库，这里只留**决定性行** + 最短复现命令。
> 采集：`adb -s emulator-5554 logcat -d`（进程 pid 24269，包 com.netkuu.changsheng.tjjs，1.0.4 / vc1040）

## 1. 菜单框架与挂点落地
```text
10-28 21:40:58.267 24269 24269 I StarryNativeLoad: loaded apk lib-dir so: /data/app/~~IfqfTEahyGq9akjePyeeOw==/com.netkuu.changsheng.tjjs-3cnKqRmXE7JebK8gLf-JLQ==/lib/arm64/libstarry_magic.so
10-28 21:40:58.712 24269 24269 I MenuService: auth game_id=cstjjs
10-28 21:40:58.747 24269 24269 I NativeMenu: attached ACTIVITY host=UnityPlayerActivity
10-28 21:41:00.754 24269 24296 I Mod_Menu: [Cstjjs] enter-interp maps VA=0x400025a5bf4c file=0x2657F4C miH=0x0
10-28 21:41:00.860 24269 24296 I Mod_Menu: [Cstjjs] hooked enter-interp @ 0x400025a5bf4c
10-28 21:41:00.860 24269 24296 I Mod_Menu: [Cstjjs] 初始化成功 RoleSide=0x74 miH=0x0
```

## 2. 两项功能命中真实扣血路径（feat1 十倍伤害 / feat2 无敌）
```text
10-28 21:46:51.630 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=1 side=1 -50 -> 0
10-28 21:46:51.635 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=2 side=1 -50 -> 0
10-28 21:46:51.981 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=3 side=1 -50 -> 0
10-28 21:46:51.983 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=4 side=1 -50 -> 0
10-28 21:46:55.635 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=5 side=2 -5 -> -50
10-28 21:46:55.636 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=6 side=2 -5 -> -50
10-28 21:46:57.914 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=7 side=2 -24 -> -240
10-28 21:47:03.066 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=8 side=1 -50 -> 0
10-28 21:47:03.067 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=9 side=1 -50 -> 0
10-28 21:47:03.508 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=10 side=1 -50 -> 0
10-28 21:47:03.509 24269 24293 I Mod_Menu: [Cstjjs] patch Health n=11 side=1 -50 -> 0
```

## 3. 开关切换（21:48:48 两次 feat 变更；切换者无法区分人工/自动，按规定不当作 L3 回执）
```text
10-28 21:46:36.809 24269 24269 I Mod_Menu: [Cstjjs] feat 2 -> 1
10-28 21:46:37.231 24269 24269 I Mod_Menu: [Cstjjs] feat 1 -> 1
10-28 21:48:48.565 24269 24269 I Mod_Menu: [Cstjjs] feat 1 -> 1
10-28 21:48:48.568 24269 24269 I Mod_Menu: [Cstjjs] feat 2 -> 1
10-28 21:48:49.373 24269 24269 I Mod_Menu: [Cstjjs] feat 1 -> 1
10-28 21:48:49.375 24269 24269 I Mod_Menu: [Cstjjs] feat 2 -> 1
```

## 4. 登录链路（TapTap 内嵌登录页被拉起，登录后进战斗）
```text
10-28 21:42:02.028   368   559 I ActivityTaskManager: START u0 {cmp=com.netkuu.changsheng.tjjs/com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity (has extras)} with LAUNCH_SINGLE_TASK from uid
10-28 21:41:57.942 24269 24293 I Unity   : ET.DlgNetkuuLoginSystem:RegisterUIEvent(DlgNetkuuLogin) (at D:\workspace\CardGame\ET-EUI\Unity\Codes\HotfixView\Netkuu\UI\DlgNetkuuLogin\DlgNetkuuLoginSystem
10-28 21:41:57.969 24269 24293 I Unity   : ET.DlgNetkuuLoginSystem:ShowAgreementDialog(DlgNetk
```

## 5. 最短复现
```bash
adb -s emulator-5554 logcat -c
adb -s emulator-5554 shell am start -n com.netkuu.changsheng.tjjs/com.unity3d.player.UnityPlayerActivity
adb -s emulator-5554 logcat -d | grep -aE "Cstjjs|StarryNativeLoad|MenuService"
```

## 6. 「非法登录」判据的取证（离线，不需跑游戏）
```bash
# 1) 从设备热更缓存拉运行时程序集（root）：
#   /sdcard/Android/data/com.netkuu.changsheng.tjjs/files/yoo/DefaultPackage/CacheFiles/28/2881b5583742ca26faabf74502d508ae/__data
# 2) UnityPy 解包 → Code.dll；3) ilspycmd -t ET.DlgNetkuuLoginSystem 反编译：
#   见 06_dumped/长生天机降世/1.0.4/decomp/decomp_hot_login.cs:460
python3 04_scripts/长生天机降世/decomp_hot.sh
```

## 7. 「非法登录」判据原文（运行时热更 `Code.dll` v1.0.4.5）

`ET.DlgNetkuuLoginSystem` 第 460 行；完整反编译 `06_dumped/长生天机降世/1.0.4/decomp/decomp_hot_login.cs`（本地保留、不入库）：

```csharp
if (Application.version.Equals("1.0.4") && MobilePlatformHelper.SafeGetFileSize() > 2277634048u)
{
    PopUpMsgDlgHelper.ShowPopUpMsgWait(self.DomainScene(), "警告", "警告！检测到非法登录！如有疑问请联系客服！",
        "关闭", isShowExit: false, delegate { MobilePlatformHelper.CallExitApp(1); }).Coroutine();
    return;
}
```

- 阈值 `2,277,634,048` = 官方底包 `2,277,631,016` ＋ **3,032 B** 余量。
- 三条黑名单（`classes4.dex` / `assets/overlay_integrity.dat` / `lib/arm64-v8a/libbattlehook.so`）在本版本**只进统计上报**（同文件 `form3.AddField("fs", MobilePlatformHelper.ScanForKnownArtifacts(SuspiciousZipEntries).Count)`），不参与登录门闩。
- APK 内置旧版 Code.dll 的同类型反编译为 1.0.3 版**双条件**（`decomp_ET_DlgNetkuuLoginSystem.cs:1404`：`version=="1.0.3" && size>1939166000 && Count>0`）—— 文档旧口径的来源，1.0.4 起失效。
- AOT 侧判定链（`dump.cs` RVA）：`SafeGetFileSize` 0x4FCBF1C、`ScanForKnownArtifacts` 0x4FCC008、`GetLoadedSharedLibraries` 0x4FCAE84、`ComputeWholeApkSHA256Async` 0x4FCB3C4、`GetBaseApkPath` 0x4FCB8D0。
