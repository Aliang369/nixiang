# AndroidManifest 结构报告（由 xmltree 自动解析）

来源: `/tmp/xxyz/dexscan/manifest.xmltree`（aapt2 dump xmltree）

## application

| 属性 | 值 |
|---|---|
| name | `com.hero.herousdkcocos.HeroUSDKApplication` |

## provider 清单（30 个）

| # | name | authorities | initOrder | process | exported | grantUri | directBoot |
|---|---|---|---|---|---|---|---|
| 1 | `com.byazt.hh.DownloaderServerManager` | `com.xxyz.sdc.pangle.servermanager.downloader.com.bytedance.sdk.openadsdk.adhost` | - | :downloader | false | - | - |
| 2 | `com.byazt.hh.MainServerManager` | `com.xxyz.sdc.pangle.servermanager.main` | - | - | false | - | - |
| 3 | `com.byazt.yx.TTMultiProvider` | `com.xxyz.sdc.TTMultiProvider` | - | - | false | - | - |
| 4 | `com.byazt.ozr.MainServerManager` | `com.xxyz.sdc.pangle.servermanager.main` | - | - | false | - | - |
| 5 | `com.byazt.hii.MainProcessProviderProxy` | `com.xxyz.sdc.pangle.provider.proxy.main` | - | - | false | - | - |
| 6 | `com.byazt.yr.FileProvider` | `com.xxyz.sdc.pangle.fileprovider` | - | - | false | true | - |
| 7 | `com.bytedance.android.openliveplugin.process.server.LiveServerManager` | `com.xxyz.sdc.bytedance.android.openliveplugin.process.server.LiveServerManager` | - | :bytelive | false | - | - |
| 8 | `com.byted.live.lite.ServerManager_bytelive` | `com.xxyz.sdc.pangle.servermanager.bytelive.com.byted.live.lite` | - | :bytelive | false | - | - |
| 9 | `com.byted.live.lite.ServerManager_push` | `com.xxyz.sdc.pangle.servermanager.push.com.byted.live.lite` | - | :push | false | - | - |
| 10 | `com.byted.live.lite.ServerManager_downloader` | `com.xxyz.sdc.pangle.servermanager.downloader.com.byted.live.lite` | - | :downloader | false | - | - |
| 11 | `com.bytedance.tools.ToolsProvider` | `com.xxyz.sdc.ToolsProvider` | - | - | - | - | - |
| 12 | `com.aliyun.sls.android.producer.provider.SLSContentProvider` | `com.xxyz.sdc.sls_provider` | - | - | false | - | - |
| 13 | `com.ultrasdk.MyFileProvider` | `com.xxyz.sdc.ultrasdk.provider` | - | - | false | true | - |
| 14 | `com.taptap.sdk.update.internal.TapUpdateFileProvider` | `com.xxyz.sdc.com.taptap.sdk.update.internal.fileProvider` | - | - | false | true | - |
| 15 | `com.taptap.sdk.update.download.OkDownloadProvider` | `com.xxyz.sdc.com.taptap.services.update.okdownload` | - | - | false | - | - |
| 16 | `com.taptap.sdk.kit.internal.TapTapKitInitProvider` | `com.xxyz.sdc.TapTapKitInitProvider` | - | - | false | - | - |
| 17 | `com.taptap.sdk.themis.lite.ThemisProvider` | `com.xxyz.sdc.themisLite` | - | - | false | - | - |
| 18 | `com.ultra.analytics.android.sdk.data.DataContentProvider` | `com.xxyz.sdc.DataContentProvider` | - | - | false | - | - |
| 19 | `com.bytedance.ttgame.library.luffy.provider.LuffyProvider` | `com.xxyz.sdc.library.luffy.provider` | - | - | true | - | - |
| 20 | `com.bytedance.frankie.provider.FrankieProvider` | `com.xxyz.sdc.frankie` | - | - | false | - | - |
| 21 | `com.volcengine.zeus.servermanager.MainServerManager` | `com.xxyz.sdc.zeus.servermanager.main` | - | - | false | - | - |
| 22 | `com.volcengine.zeus.provider.MainProcessProviderProxy` | `com.xxyz.sdc.zeus.provider.proxy.main` | - | - | false | - | - |
| 23 | `com.volcengine.zeus.FileProvider` | `com.xxyz.sdc.zeus.fileprovider` | - | - | - | true | - |
| 24 | `gbsdk.android.arch.lifecycle.ProcessLifecycleOwnerInitializer` | `com.xxyz.sdc.lifecycle-trojan` | - | - | false | - | - |
| 25 | `com.bytedance.ttgame.tob.common.host.base.impl.applog.ApkChannelFileProvider` | `com.xxyz.sdc.ApkChannelFileProvider` | 999999 | - | false | - | - |
| 26 | `com.bytedance.ttgame.tob.common.host.main.GBFileProvider` | `com.xxyz.sdc.osdkcommon.fileprovider` | - | - | false | true | - |
| 27 | `com.bytedance.ttgame.tob.packer.common.StubServerManager$PushServerManager` | `com.xxyz.sdc.zeus.servermanager.push.com.bytedance.ttgame.tob.common` | - | :push | false | - | - |
| 28 | `com.bytedance.android.sdk.bdticketguard.TicketGuardProvider` | `com.xxyz.sdc.TicketGuardProvider` | - | - | false | - | - |
| 29 | `com.bytedance.ttgame.tob.packer.screenrecord.StubServerManager$SrServerManager` | `com.xxyz.sdc.zeus.servermanager.sr.gbsdk.plugin.screenrecord` | - | :sr | false | - | - |
| 30 | `com.qq.gdt.action.GDTInitProvider` | `com.xxyz.sdc.GDTInitProvider` | - | - | false | - | - |

## LAUNCHER / MAIN Activity（1 个）

- `com.hero.herousdkcocos.HeroUSDKSplashActivity`  exported=true  launchMode=2 process=- orientation=1 theme=@0x01030007 (line=54)

## Activity 总数与 process 变体（共 162）

- **process=-** → 148 个
  - `com.cocos.game.AppActivity`
  - `com.cocos.lib.CocosEditBoxActivity`
  - `com.hero.herousdkcocos.HeroUSDKSplashActivity`
  - `com.ss.android.downloadlib.activity.TTDelegateActivity`
  - `com.byazt.fl.JumpKllkActivity`
  - `com.ss.android.socialbase.appdownloader.view.DownloadHandleNotificationActivity`
  - `com.ss.android.socialbase.appdownloader.view.DownloadTaskDeleteActivity`
  - `com.byazt.v.JumpUnknownSourceActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTNativePageActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTWebPageActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTSevenScreenWebPageActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTVideoWebPageActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTVideoScrollWebPageActivity`
  - `com.bytedance.sdk.openadsdk.core.activity.base.TTDelegateActivity`
  - `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTRewardVideoActivity`
  - `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTFullScreenVideoActivity`
  - `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTRewardVideoLandscapeActivity`
  - `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTFullScreenVideoLandscapeActivity`
  - `com.byazt.ff.Stub_Standard_Activity`
  - `com.byazt.ff.Stub_Standard_Portrait_Activity`
  - `com.byazt.ff.Stub_Standard_Activity_T`
  - `com.byazt.ff.Stub_Standard_Landscape_Activity`
  - `com.byazt.ff.Stub_Activity`
  - `com.byazt.ff.Stub_SingleTask_Activity_T`
  - `com.byazt.ff.Stub_SingleTask_Activity`
  - `com.byazt.uag.ApiDownloadHandleNotificationActivity`
  - `com.bytedance.android.openliveplugin.stub.activity.DouyinAuthorizeActivityProxy`
  - `com.bytedance.android.openliveplugin.stub.activity.StubActivity$Activity_Portrait`
  - `com.bytedance.android.openliveplugin.stub.activity.StubActivity$Activity_Portrait2`
  - `com.bytedance.android.openliveplugin.stub.activity.StubActivity$Activity_Portrait3`
  - `com.bytedance.android.openliveplugin.stub.activity.StubActivity$Activity_Portrait4`
  - `com.bytedance.android.openliveplugin.stub.activity.StubActivity$Activity_Portrait5`
  - `com.byted.live.lite.Activity_main_singleTask1`
  - `com.byted.live.lite.Activity_main_singleTask2`
  - `com.byted.live.lite.Activity_main_singleTask3`
  - `com.byted.live.lite.Activity_main_singleTask4`
  - `com.byted.live.lite.Activity_main_standard`
  - `com.byted.live.lite.Activity_main_singleTop1`
  - `com.byted.live.lite.Activity_main_singleTop2`
  - `com.byted.live.lite.Activity_main_singleInstance1`
  - `com.byted.live.lite.Activity_main_singleTop5`
  - `com.byted.live.lite.Activity_main_singleTop6`
  - `com.byted.live.lite.Activity_main_singleTop3`
  - `com.byted.live.lite.Activity_main_singleTop4`
  - `com.bytedance.msdk.api.activity.TTDelegateActivity`
  - `com.bytedance.msdk.api.activity.TTTransparentActivity`
  - `com.ultrasdk.browser.BrowserActivity`
  - `com.hu.zxlib.base.ScannerActivity`
  - `com.hu.scan.permission.PermissionActivity`
  - `com.ultrasdk.activity.HuslActivity`
  - `com.bytedance.tools.ui.ToolsActivity`
  - `com.bytedance.tools.ui.ui.ToolRitDetailActivity`
  - `com.bytedance.tools.ui.FeedBackActivity`
  - `com.bytedance.tools.ui.webview.ToolsBaseInfoActivity`
  - `com.bytedance.tools.ui.webview.ToolsAdPreviewDetailActivity`
  - `com.bytedance.tools.ui.webview.MediationAdnDetailActivity`
  - `com.bytedance.tools.ui.webview.MediationRitInfoActivity`
  - `com.bytedance.tools.ui.webview.MediationRitDetailActivity`
  - `com.bytedance.mtesttools.act.TestToolMainActivity`
  - `com.bytedance.mtesttools.act.AdnMainActivity`
  - `com.bytedance.mtesttools.act.AdnDetailActivity`
  - `com.bytedance.mtesttools.act.AdRitDetailActivity`
  - `com.bytedance.mtesttools.act.AdSlotDetailActivity`
  - `com.ultrasdk.deep.DeepSActivity`
  - `com.ultrasdk.activity.HulActivity`
  - `com.ultrasdk.activity.HupActivity`
  - `com.ultrasdk.activity.HuslActivity`
  - `com.ultrasdk.activity.HuspActivity`
  - `com.ultrasdk.browser.BrowserActivity`
  - `com.jdpaysdk.author.AuthorActivity`
  - `com.tencent.tauth.AuthActivity`
  - `com.xxyz.sdc.wxapi.WXEntryActivity`
  - `com.xxyz.sdc.wxapi.WXPayEntryActivity`
  - `com.jdpaysdk.author.browser.BrowserActivity`
  - `com.tencent.connect.common.AssistActivity`
  - `com.chuanglan.shanyan_sdk.view.CmccLoginAuthActivity`
  - `com.ultrasdk.official.activity.BaseActivity`
  - `com.ultrasdk.official.activity.ZZActivity`
  - `com.ultrasdk.official.floatdlg.FloatDialog`
  - `com.ultrasdk.official.floatdlg.VoucherWebActivity`
  - `com.ultrasdk.official.floatdlg.HelpCenterActivity`
  - `com.ultrasdk.official.floatdlg.FloatBigActivity`
  - `com.ultrasdk.official.floatdlg.WiKiActivity`
  - `com.ultrasdk.official.floatdlg.VideoActivity`
  - `com.ultrasdk.official.floatdlg.KFActivity`
  - `com.ultrasdk.official.activity.CustomWebAct`
  - `com.alipay.sdk.app.H5PayActivity`
  - `com.alipay.sdk.app.H5AuthActivity`
  - `com.alipay.sdk.app.PayResultActivity`
  - `com.alipay.sdk.app.AlipayResultActivity`
  - `com.alipay.sdk.app.H5OpenAuthActivity`
  - `com.alipay.sdk.app.APayEntranceActivity`
  - `com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity`
  - `com.ss.android.dypay.activity.DyPayInstallGuideActivity`
  - `com.ss.android.dypay.activity.DyPayEntranceActivity`
  - `com.ultrasdk.activity.HulActivity`
  - `com.ultrasdk.activity.HupActivity`
  - `com.ultrasdk.activity.HuslActivity`
  - `com.ultrasdk.activity.HuspActivity`
  - `com.bytedance.ads.convert.BDBridgeActivity`
  - `com.hu.plugin.ad.douyin.DouYinDeepSActivity`
  - `com.bytedance.ttgame.tob.common.host.base.impl.debug.DebugInfoActivity`
  - `com.bytedance.ttgame.tob.common.host.framework.permission.PermissionActivity`
  - `com.bytedance.ttgame.tob.common.host.main.plugin.PluginUpdateStateActivity`
  - `com.bytedance.ttgame.tob.common.host.main.debug.DebugModeActivity`
  - `com.bytedance.ttgame.tob.common.host.main.debug.InterceptorActivity`
  - `com.bytedance.ttgame.tob.packer.common.StubActivity$AppCompat`
  - `com.bytedance.ttgame.tob.packer.common.StubActivity$AppCompat_T`
  - `com.bytedance.ttgame.tob.packer.common.StubActivity$Activity`
  - `com.bytedance.ttgame.tob.packer.common.StubActivity$Activity_T`
  - `com.bytedance.ttgame.tob.packer.debug.StubActivity$AppCompat`
  - `com.bytedance.ttgame.tob.packer.debug.StubActivity$AppCompat_T`
  - `com.bytedance.ttgame.tob.packer.debug.StubActivity$Activity`
  - `com.bytedance.ttgame.tob.packer.debug.StubActivity$Activity_T`
  - `com.bytedance.ttgame.tob.packer.debug.StubActivity$AppCompat_Portrait`
  - `com.bytedance.applog.migrate.MigrateDetectorActivity`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$Activity`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$Activity_T`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$AppCompat`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$AppCompat_T`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$AppCompat_T_SingleInstanceActivity`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$AppCompat_T_SingleTaskActivity`
  - `com.bytedance.ttgame.tob.packer.datalink.StubActivity$AppCompat_T_SingleTopActivity`
  - `com.bytedance.ttgame.tob.optional.aweme.impl.activity.DouYinEntryActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.DouyinCardActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.ProfileEntryActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.video.DouYinVideoActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.game.NewVideoActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.game.LandscapeVideoActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.game.PortraitVideoActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.game.ReversePortraitVideoActivity`
  - `com.bytedance.sdk.open.aweme.openprofile.ui.game.ReverseLandscapeVideoActivity`
  - `com.bytedance.sdk.open.aweme.openverifiedexchange.ui.webview.WebViewActivity`
  - `com.bytedance.sdk.open.douyin.ui.DouYinWebAuthorizeActivity`
  - `com.bytedance.ttgame.tob.doulink.impl.ui.DoulinkAlertActivity`
  - `com.bytedance.ttgame.tob.optional.share.impl.callback.DouYinEntryActivity`
  - `com.bytedance.ttgame.tob.packer.share.StubActivity$AppCompat`
  - `com.bytedance.ttgame.tob.packer.share.StubActivity$AppCompat_T`
  - `com.bytedance.ttgame.tob.packer.share.StubActivity$Activity`
  - `com.bytedance.ttgame.tob.packer.share.StubActivity$Activity_T`
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$AppCompat`
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$AppCompat_T`
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$Activity`
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$Activity_T`
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$AppCompat_Behind_SingleTop1`
  - `com.hu.plugin.auth.douyin.HDouYinEntryActivity`
  - `com.bytedance.sdk.open.douyin.ui.DouYinWebAuthorizeActivity`
  - `com.bytedance.sdk.open.douyin.impl.openapp.startapp.DouYinSDKEntryActivity`
- **process=:bytelive** → 13 个
  - `com.bytedance.android.openliveplugin.stub.activity.DouyinAuthorizeActivityLiveProcessProxy`
  - `com.byted.live.lite.Activity_bytelive_singleTask4`
  - `com.byted.live.lite.Activity_bytelive_singleTop3`
  - `com.byted.live.lite.Activity_bytelive_singleTop2`
  - `com.byted.live.lite.Activity_bytelive_singleTop5`
  - `com.byted.live.lite.Activity_bytelive_singleTop4`
  - `com.byted.live.lite.Activity_bytelive_standard`
  - `com.byted.live.lite.Activity_bytelive_singleTop1`
  - `com.byted.live.lite.Activity_bytelive_singleTask1`
  - `com.byted.live.lite.Activity_bytelive_singleTop6`
  - `com.byted.live.lite.Activity_bytelive_singleTask2`
  - `com.byted.live.lite.Activity_bytelive_singleTask3`
  - `com.byted.live.lite.Activity_bytelive_singleInstance1`
- **process=:sr** → 1 个
  - `com.bytedance.ttgame.tob.packer.screenrecord.StubActivity$Activity_Sr_T_SingleTop1`

## splash / login / sdk 相关 Activity & Service

- activity `com.hero.herousdkcocos.HeroUSDKSplashActivity` exported=true process=- (line=54)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTNativePageActivity` exported=false process=- (line=79)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTWebPageActivity` exported=false process=- (line=80)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTSevenScreenWebPageActivity` exported=false process=- (line=81)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTVideoWebPageActivity` exported=false process=- (line=82)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTVideoScrollWebPageActivity` exported=false process=- (line=83)
- activity `com.bytedance.sdk.openadsdk.core.activity.base.TTDelegateActivity` exported=false process=- (line=84)
- activity `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTRewardVideoActivity` exported=false process=- (line=85)
- activity `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTFullScreenVideoActivity` exported=false process=- (line=86)
- activity `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTRewardVideoLandscapeActivity` exported=false process=- (line=87)
- activity `com.bytedance.sdk.openadsdk.core.component.reward.activity.TTFullScreenVideoLandscapeActivity` exported=false process=- (line=88)
- activity `com.bytedance.android.openliveplugin.stub.activity.DouyinAuthorizeActivityProxy` exported=true process=- (line=105)
- activity `com.bytedance.android.openliveplugin.stub.activity.DouyinAuthorizeActivityLiveProcessProxy` exported=true process=:bytelive (line=106)
- activity `com.bytedance.msdk.api.activity.TTDelegateActivity` exported=false process=- (line=140)
- activity `com.bytedance.msdk.api.activity.TTTransparentActivity` exported=false process=- (line=141)
- activity `com.ultrasdk.browser.BrowserActivity` exported=- process=- (line=142)
- activity `com.ultrasdk.activity.HuslActivity` exported=- process=- (line=145)
- activity `com.ultrasdk.deep.DeepSActivity` exported=true process=- (line=166)
- activity `com.ultrasdk.activity.HulActivity` exported=- process=- (line=174)
- activity `com.ultrasdk.activity.HupActivity` exported=- process=- (line=175)
- activity `com.ultrasdk.activity.HuslActivity` exported=- process=- (line=176)
- activity `com.ultrasdk.activity.HuspActivity` exported=- process=- (line=177)
- activity `com.ultrasdk.browser.BrowserActivity` exported=- process=- (line=181)
- activity `com.jdpaysdk.author.AuthorActivity` exported=true process=- (line=185)
- activity `com.tencent.tauth.AuthActivity` exported=true process=- (line=193)
- activity `com.jdpaysdk.author.browser.BrowserActivity` exported=false process=- (line=203)
- activity `com.chuanglan.shanyan_sdk.view.CmccLoginAuthActivity` exported=false process=- (line=205)
- activity `com.ultrasdk.official.activity.BaseActivity` exported=- process=- (line=213)
- activity `com.ultrasdk.official.activity.ZZActivity` exported=- process=- (line=214)
- activity `com.ultrasdk.official.floatdlg.FloatDialog` exported=- process=- (line=215)
- activity `com.ultrasdk.official.floatdlg.VoucherWebActivity` exported=- process=- (line=216)
- activity `com.ultrasdk.official.floatdlg.HelpCenterActivity` exported=- process=- (line=217)
- activity `com.ultrasdk.official.floatdlg.FloatBigActivity` exported=- process=- (line=218)
- activity `com.ultrasdk.official.floatdlg.WiKiActivity` exported=- process=- (line=219)
- activity `com.ultrasdk.official.floatdlg.VideoActivity` exported=- process=- (line=220)
- activity `com.ultrasdk.official.floatdlg.KFActivity` exported=- process=- (line=221)
- activity `com.ultrasdk.official.activity.CustomWebAct` exported=- process=- (line=222)
- activity `com.alipay.sdk.app.H5PayActivity` exported=false process=- (line=223)
- activity `com.alipay.sdk.app.H5AuthActivity` exported=false process=- (line=224)
- activity `com.alipay.sdk.app.PayResultActivity` exported=true process=- (line=225)
- activity `com.alipay.sdk.app.AlipayResultActivity` exported=true process=- (line=226)
- activity `com.alipay.sdk.app.H5OpenAuthActivity` exported=false process=- (line=227)
- activity `com.alipay.sdk.app.APayEntranceActivity` exported=false process=- (line=228)
- activity `com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity` exported=true process=- (line=229)
- activity `com.ultrasdk.activity.HulActivity` exported=- process=- (line=239)
- activity `com.ultrasdk.activity.HupActivity` exported=- process=- (line=240)
- activity `com.ultrasdk.activity.HuslActivity` exported=- process=- (line=241)
- activity `com.ultrasdk.activity.HuspActivity` exported=- process=- (line=242)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.DouyinCardActivity` exported=true process=- (line=300)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.ProfileEntryActivity` exported=true process=- (line=301)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.video.DouYinVideoActivity` exported=false process=- (line=302)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.game.NewVideoActivity` exported=false process=- (line=303)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.game.LandscapeVideoActivity` exported=false process=- (line=304)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.game.PortraitVideoActivity` exported=false process=- (line=305)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.game.ReversePortraitVideoActivity` exported=false process=- (line=306)
- activity `com.bytedance.sdk.open.aweme.openprofile.ui.game.ReverseLandscapeVideoActivity` exported=false process=- (line=307)
- activity `com.bytedance.sdk.open.aweme.openverifiedexchange.ui.webview.WebViewActivity` exported=- process=- (line=309)
- activity `com.bytedance.sdk.open.douyin.ui.DouYinWebAuthorizeActivity` exported=- process=- (line=310)
- activity `com.hu.plugin.auth.douyin.HDouYinEntryActivity` exported=true process=- (line=332)
- activity `com.bytedance.sdk.open.douyin.ui.DouYinWebAuthorizeActivity` exported=- process=- (line=333)
- activity `com.bytedance.sdk.open.douyin.impl.openapp.startapp.DouYinSDKEntryActivity` exported=true process=- (line=334)

## application meta-data

- `android.app.lib_name` = `-`
- `appKey` = `-`
- `productId` = `-`
- `endpoint` = `-`
- `bdcAccesskeyID` = `-`
- `bdcAccessKeySecret` = `-`
- `logProjectName` = `-`
- `logStoreName` = `-`
- `debugMode` = `-`
- `com.google.android.gms.games.APP_ID` = `-`
- `android.support.FILE_PROVIDER_PATHS` = `-`
- `U_P_ID` = `-`
- `U_C_ID` = `-`
- `U_C_I_ID` = `-`
- `SERVER_ID` = `-`
- `PROJECT_ID` = `-`
- `PRODUCT_ID` = `-`
- `android.support.FILE_PROVIDER_PATHS` = `-`
- `com.taptap.services.update.loader` = `-`
- `com.ultra.analytics.android.MainProcessName` = `-`
- `com.ultra.analytics.android.version` = `-`
- `hume_convert.AppConvert.sdk.version` = `-`
- `gbsdk.android.support.FILE_PROVIDER_PATHS` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_common_host` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_common_plugin` = `-`
- `ZEUS_PLUGIN_GBSDK_COMMON` = `-`
- `ZEUS_PLUGIN_GBSDK_DEBUG` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_optional_applog` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_optional_datalink_plugin` = `-`
- `ZEUS_PLUGIN_GBSDK_DATALINK` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_optional_aweme` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_optional_share_plugin` = `-`
- `ZEUS_PLUGIN_GBSDK_SHARE` = `-`
- `com.bytedance.ttgame.tob.artifact_version.gbsdk_optional_screenrecord_plugin` = `-`
- `ZEUS_PLUGIN_GBSDK_SCREENRECORD` = `-`
- `com.ultrasdk.business` = `-`
- `TINKER_ID` = `-`

## queries（包可见性声明，1 个块）

### queries block (line=338)
- packages: `com.ss.android.ugc.aweme`, `com.ss.android.ugc.aweme.lite`, `com.ss.android.ugc.live`, `com.ss.android.ugc.aweme.hubble`, `com.ss.android.yumme.video`, `my.maya.android`, `com.ss.android.ugc.aweme`, `com.ss.android.ugc.aweme.lite`, `com.ss.android.ugc.live`, `com.ss.android.ugc.aweme.hubble`, `com.ss.android.yumme.video`, `com.ss.android.ugc.aweme`, `com.ss.android.ugc.aweme.lite`, `com.ss.android.article.news`, `com.ss.android.article.lite`, `com.dragon.read`, `com.jingdong.app.mall`, `com.tencent.mobileqq`, `com.tencent.tim`, `com.tencent.minihd.qq`, `com.tencent.qqlite`, `com.taptap`, `com.taptap.global`, `com.tencent.mm`, `com.xmcy.hykb`, `com.eg.android.AlipayGphone`, `com.ss.android.ugc.aweme`, `com.ss.android.ugc.aweme.lite`, `com.eg.android.AlipayGphoneRC`, `hk.alipay.wallet`, `hk.alipay.walletRC`, `com.taptap.global.lite`, `com.ss.android.ugc.aweme`, `com.ss.android.ugc.aweme.lite`
  - intent actions=['android.intent.action.VIEW'] categories=['android.intent.category.BROWSABLE'] data=[{'http://schemas.android.com/apk/res/android:scheme': 'https'}]
  - intent actions=['android.intent.action.VIEW'] categories=['android.intent.category.BROWSABLE'] data=[{'http://schemas.android.com/apk/res/android:scheme': 'http'}]
  - intent actions=['*'] categories=[] data=[{'http://schemas.android.com/apk/res/android:scheme': 'tbopen'}]
  - intent actions=['com.taptap.sdk.action'] categories=['android.intent.category.DEFAULT'] data=[]
  - intent actions=['com.taptap.global.sdk.action'] categories=['android.intent.category.DEFAULT'] data=[]
  - intent actions=['android.intent.action.VIEW'] categories=['android.intent.category.BROWSABLE'] data=[{'http://schemas.android.com/apk/res/android:scheme': 'https'}]
  - intent actions=['android.intent.action.VIEW'] categories=['android.intent.category.BROWSABLE'] data=[{'http://schemas.android.com/apk/res/android:scheme': 'http'}]
