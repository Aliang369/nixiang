using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.Accounts;
using ChillyRoom.Services.Accounts.Sdk;
using ChillyRoom.Services.Accounts.Utils;
using ChillyRoom.Services.ContainerOfSoul;
using ChillyRoom.Services.Core;
using CoreKit.Config;
using Generated.ChillyRoomSdkClient;
using MainMenu;
using UnityEngine;
using UnityEngine.Video;

public class MainSetter : DebugPanel.Setter
{
	private const string _name = "主要";

	public static VideoPlayer v;

	public override string name => "主要";

	private MainMenuPanel.LoginPhase? loginPhase
	{
		get
		{
			return Bootstrap.instance.mainMenuUIManager?.mainMenuPanel?.loginPhase;
		}
		set
		{
			MainMenuPanel mainMenuPanel = Bootstrap.instance.mainMenuUIManager?.mainMenuPanel;
			if (mainMenuPanel != null && value.HasValue)
			{
				mainMenuPanel.loginPhase = value.Value;
			}
		}
	}

	public MainSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
	}

	private void OpenForerunnerLegacyEncounterStoryNote()
	{
		StoryNotePanel storyNotePanel = base.gameState?.UIManager?.storyNotePanel ?? base.fortressState?.uiManager?.storyNotePanel;
		if (storyNotePanel == null)
		{
			Debug.LogWarning("[先行者遗物-遇到信件] 当前场景没有可用的 StoryNotePanel");
			return;
		}
		ForerunnerLegacy forerunnerLegacy = new ForerunnerLegacy
		{
			guid = Guid.NewGuid().ToString(),
			sourcePlayerId = 987654321L,
			signature = "调试先行者",
			note = "愿这封跨越深境的信件，能帮助后来者验证举报界面的布局与交互。",
			storyOnly = false,
			shareLegacy = true,
			leaveTime = DateTime.Now.AddHours(-2.0),
			expireTime = DateTime.Now.AddDays(7.0)
		};
		storyNotePanel.Setup(StoryNoteInfo.GetForerunnerLegacy(forerunnerLegacy));
		storyNotePanel.EnableReportDebugSimulation();
		storyNotePanel.ShowUI();
		debugPanel.debugContentPanel.SetActive(value: false);
	}

	private void OpenForerunnerLegacyOverview()
	{
		ForerunnerLegacyOverviewPanel forerunnerLegacyOverviewPanel = base.fortressState?.uiManager?.forerunnerLegacyOverviewPanel;
		if (forerunnerLegacyOverviewPanel == null)
		{
			Debug.LogWarning("[先行者遗物-查看已有信件] 当前场景没有可用的 ForerunnerLegacyOverviewPanel");
			return;
		}
		forerunnerLegacyOverviewPanel.ShowUI();
		debugPanel.debugContentPanel.SetActive(value: false);
	}

	private static async Task FinishAllGuidesOnServer()
	{
		await DebugPanel.Setter.applicationState.ChillyRoomSDK.Guide.FinishIntroAsync();
		foreach (GuideModule item in ScriptableObjectSingleton<GuideData>.instance.guideModules.Where((GuideModule module) => module != null))
		{
			await DebugPanel.Setter.applicationState.ChillyRoomSDK.Guide.FinishGuideModuleAsync(new FinishGuideModuleRequest(item));
		}
	}

	private static async Task UnlockAllClavisOnServer()
	{
		foreach (ClavisDef item in DebugPanel.Setter.applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets.Where((ClavisDef clavis) => clavis != null))
		{
			if (!DebugPanel.Setter.applicationState.clavisManager.IsUnlocked(item))
			{
				await DebugPanel.Setter.applicationState.ChillyRoomSDK.Progress.UnlockClavisAsync(new UnlockClavisRequest(item));
			}
		}
	}

	private async Task LoginNewGuest(bool skipGuide)
	{
		string tag = (skipGuide ? "[新账号登录(跳过引导)]" : "[新账号登录]");
		AccountSdk accounts = ChillyRoomService.GetSdk().Accounts;
		ChillyRoomServiceCore core = accounts.StateManager.Core;
		core.Cache.ClearGuestId(core.Context.DistroId);
		core.Cache.ClearSdkStateCache();
		IResult<SdkState, Failure<AccountSdk.LoginErrorCode>> result = await accounts.Login((UniversalSdkType)11);
		if (!result.IsOk())
		{
			Debug.LogError(tag + " 游客登录失败：" + result.Error?.Description);
			return;
		}
		float startTime = Time.realtimeSinceStartup;
		while (loginPhase != MainMenuPanel.LoginPhase.AfterLogin)
		{
			if (Time.realtimeSinceStartup - startTime > 15f)
			{
				Debug.LogWarning(tag + " 等待登录完成超时（可能卡在实名/防沉迷）");
				return;
			}
			await Task.Yield();
		}
		loginPhase = MainMenuPanel.LoginPhase.AfterConfirmLogin;
		string text = accounts.StateManager.State.User.PlayerId.ToString();
		Debug.Log(tag + " 新游客 playerId=" + text);
		Bootstrap.instance.InitLoad(text, dropCurrentGame: false, skipGuide);
		Bootstrap.instance.bootstrapExtra.debugPanel.debugContentPanel.gameObject.SetActive(value: false);
	}
}
