using System;
using System.Threading.Tasks;
using ChillyRoom.Agreement.V1;
using ChillyRoom.Censor.V1;
using ChillyRoom.Common.UI;
using ChillyRoom.Infra.Messaging;
using ChillyRoom.Privacy;
using ChillyRoom.RPC;
using ChillyRoom.RealName.Client;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.Accounts;
using ChillyRoom.Services.Accounts.Features;
using ChillyRoom.Services.Accounts.Localize;
using ChillyRoom.Services.Accounts.Sdk;
using ChillyRoom.Services.ContainerOfSoul;
using ChillyRoom.Services.Core;
using ChillyRoom.Services.Core.Libs;
using ChillyRoom.Services.Utils.I2.Loc;
using CoreKit.Agent;
using CoreKit.Config;
using MQTTnet.Client;
using Newtonsoft.Json;
using UnityEngine;

namespace Generated.ChillyRoomSdkClient;

public class ChillyRoomService
{
	public AccountSdk Accounts;

	public Payment Payment;

	public Mail Mail;

	public ChillyRoomServiceCore Core;

	internal GameConfig Config;

	private static ChillyRoomService Sdk;

	public MessagingClient MessagingClient;

	private string _liveUrl = "live.test.chilly.tech";

	private int _gameId;

	public RealNameClient RealName;

	public AntiAddictionClient AntiAddiction;

	private string _realNameGateWay = "https://aas.chillyroom.com";

	public ValidationClient CensorValidation;

	public AgreementClient Agreement;

	public CardSkinClient CardSkin;

	public CharacterClient Character;

	public ForerunnerClient Forerunner;

	public GameplayClient Gameplay;

	public DailyChallengeClient DailyChallenge;

	public GameRecordClient GameRecord;

	public GuideClient Guide;

	public IndexBookClient IndexBook;

	public MailBoxClient MailBox;

	public GiftCodeClient GiftCode;

	public PleaseReviewPopupClient PleaseReviewPopup;

	public DataOptimizationClient DataOptimization;

	public ProfileDataClient ProfileData;

	public ProgressClient Progress;

	public PurchaseClient Purchase;

	public PVPClient PVP;

	public RankClient Rank;

	public RoutineClient Routine;

	public SeasonClient Season;

	public SkillClient Skill;

	public PlayerReportClient PlayerReport;

	public LootClient Loot;

	public LimitedTimeActivityClient LimitedTimeActivity;

	public IntermissionPlayClient IntermissionPlay;

	public YuanQiActivityClient YuanQiActivity;

	public NewbieFestivalClient NewbieFestival;

	public TestClient TestClient;

	public ForceUpdateClient ForceUpdate;

	public ChillyRoomService AddAccountsIntegration(Action<AccountSdkConfigBuilder> buildOptions)
	{
		Accounts = AccountSdk.Bootstrap(new AccountSdkConfig
		{
			Core = Core,
			Config = AccountSdkConfigBuilder.Build(Config, buildOptions)
		});
		return this;
	}

	public ChillyRoomService AddAccountsIntegration()
	{
		Accounts = AccountSdk.Bootstrap(new AccountSdkConfig
		{
			Core = Core,
			Config = Config
		});
		return this;
	}

	public ChillyRoomService AddPaymentIntegration(bool disableLoadingScreen = false)
	{
		Payment = new Payment(MinimalChillyRoomService.GetMinimalSDK().HttpClient, disableLoadingScreen);
		return this;
	}

	public ChillyRoomService AddMailIntegration()
	{
		Mail = new Mail(MinimalChillyRoomService.GetMinimalSDK().HttpClient);
		return this;
	}

	private ChillyRoomService()
	{
		Config = SdkConfigManager.gameConfig;
		Core = MinimalChillyRoomService.GetCore();
	}

	private ChillyRoomService(string endpoint, GameConfig config, string language)
	{
		Config = config;
		Core = new ChillyRoomServiceCore(endpoint, language, Config);
		RPCClient.InstallGlobalStrictChillyroomCertificateValidation();
	}

	public ChillyRoomService SwitchCoreLanguage(SystemLanguage language)
	{
		Core.Context.GameLang = language.ToString();
		string text = language.ToString();
		if (text == "ChineseSimplified")
		{
			text = "Chinese";
		}
		ChillyRoom.Services.Utils.I2.Loc.LocalizationManager.CurrentLanguage = (ChillyRoom.Services.Utils.I2.Loc.LocalizationManager.HasLanguage(text) ? text : Application.systemLanguage.ToString());
		return this;
	}

	public static void AddEssentialConverters(JsonSerializerSettings settings)
	{
		settings.Converters.Add(new LongToStringConverter());
		settings.Converters.Add(new NumericDictionaryConverter());
	}

	public static ChillyRoomService GetSdk()
	{
		if (Sdk == null)
		{
			Debug.LogError("先调用ChillyRoomService初始化");
			return null;
		}
		return Sdk;
	}

	public static ChillyRoomServiceCore GetCore()
	{
		return MinimalChillyRoomService.GetCore();
	}

	public static ChillyRoomService Bootstrap()
	{
		Sdk = new ChillyRoomService();
		return Sdk;
	}

	private async Task SubscribeMailBoxAttachmentTopic()
	{
		long id = AccountStateManager.Instance.State.User.Id;
		MessagingClient.OpenSubscribingConnection($"{_gameId}/mailbox-attachments/{id}").OnReceiveStringMessage += new ReceivedStringMessageDelegate(OnMailboxClaimAttachments);
	}

	private void OnMailboxClaimAttachments(string message, string _)
	{
		Debug.Log("Received MailboxClaimAttachments Topic: " + message);
		SdkEvents.MailClaimTopic();
	}

	private async Task SubscribeUserStatusTopic()
	{
		long id = AccountStateManager.Instance.State.User.Id;
		MessagingClient.OpenSubscribingConnection($"common/user-status/{id}");
	}

	public ChillyRoomService AddMessagingIntegration()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		_gameId = int.Parse(SdkConfigManager.gameConfig.Id);
		MessagingClient = new MessagingClient(_gameId, ((int)CSCommon.netWorkEndPointType == 3) ? "live.prod.chillyroom.com" : "live.test.chilly.tech");
		MessagingClient.OnConnectionSuccess += new ConnectedDelegate(MessagingClient_OnConnect);
		MessagingClient.OnDisConnect += new DisConnectedDelegate(MessagingClient_OnDisConnect);
		SdkEvents.LoginResultAction += MessagingClient_Login;
		SdkEvents.LogoutAction += MessagingClient_Logout;
		SdkEvents.OnAccountChange += MessagingClient_AccountChange;
		return this;
	}

	private void MessagingClient_OnConnect()
	{
		Debug.Log("MQTT server connected");
	}

	private void MessagingClient_OnDisConnect(MqttClientDisconnectReason reason, bool wasConnected)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)reason == 152)
		{
			AgentHub.Instance.ChillyRoomMainThreadDispatcher.ExecuteUpdate((Action)delegate
			{
				string netLocalization = Singleton<Localization>.Instance.GetNetLocalization("common/sys_notice", "系统通知");
				string netLocalization2 = Singleton<Localization>.Instance.GetNetLocalization("cloudsave/confirm", "确认");
				string netLocalization3 = Singleton<Localization>.Instance.GetNetLocalization("account/login_other_location", "该账号已在其他地点登录");
				CommonUIUtils.ShowWindowOneButton(netLocalization, netLocalization3, netLocalization2, (Action)delegate
				{
					Application.Quit();
				});
			});
		}
		Debug.Log($"Disconnect from MQTT server, reason: {reason}, wasConnected: {wasConnected}");
	}

	private void MessagingClient_AccountChange(IResult<SdkState, Failure<AccountSdk.LoginErrorCode>> ret)
	{
		ConnectToMessageBroker();
	}

	private async void MessagingClient_Logout(bool ret)
	{
		Debug.Log("账户登出成功，断开到 MQTT server 的连接");
		await MessagingClient.Disconnect().ConfigureAwait(continueOnCapturedContext: false);
	}

	private void MessagingClient_Login(IResult<AccountSuccessResult, Failure<AccountFailResult>> ret)
	{
		if (ret.IsOk())
		{
			Debug.Log("账户登录成功，准备连接到 MQTT server");
			ConnectToMessageBroker();
		}
	}

	private async Task ConnectToMessageBroker()
	{
		await MessagingClient.ConnectAsUser(AccountStateManager.Instance.State.User.Id, AccountStateManager.Instance.State.Session.Token);
		await SubscribeUserStatusTopic();
	}

	public ChillyRoomService AddRealNameIntegration()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		if (Core.Client.Client.AsHttpClient().BaseAddress.Host.EndsWith(".room"))
		{
			_realNameGateWay = "https://idcheck.chilly.room";
		}
		if (Core.Client.Client.AsHttpClient().BaseAddress.Host.EndsWith(".tech"))
		{
			_realNameGateWay = "https://aas.test.chilly.tech";
		}
		Debug.Log("[MT] Detected the current game gateway as: " + Core.Client.Client.AsHttpClient().BaseAddress.Host + ", automatically adjusting the real-name gateway to: " + _realNameGateWay + ".");
		RealName = new RealNameClient(Core.Client.Client.AsHttpClient(), _realNameGateWay);
		AntiAddiction = new AntiAddictionClient(RealName);
		ServerTimePoller.SetAutoSyncServerTimeFunc((Func<IObservable<DateTimeOffset>>)(() => Core.GetUtcDateTimeFromRemoteServer()));
		return this;
	}

	public ChillyRoomService AddCensorValidationIntegration()
	{
		CensorValidation = new ValidationClient("", Core.Client.Client.AsHttpClient());
		return this;
	}

	public ChillyRoomService AddAgreementIntegration()
	{
		Agreement = new AgreementClient("", Core.Client.Client.AsHttpClient());
		PrivacyViewManager.Instance.SetCommonHttpClient(Agreement);
		PrivacyViewManager.Instance.SetWebClient(Core.WebViewClient);
		return this;
	}

	public ChillyRoomService ContainerOfSoulIntegratioIn()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		string baseUrl = (((int)CSCommon.netWorkEndPointType == 0) ? "http://127.0.0.1:8080" : "");
		CardSkin = new CardSkinClient(baseUrl, Core.Client.Client.AsHttpClient());
		Character = new CharacterClient(baseUrl, Core.Client.Client.AsHttpClient());
		Forerunner = new ForerunnerClient(baseUrl, Core.Client.Client.AsHttpClient());
		Gameplay = new GameplayClient(baseUrl, Core.Client.Client.AsHttpClient());
		Gameplay.EnableRequestCompression = true;
		DailyChallenge = new DailyChallengeClient(baseUrl, Core.Client.Client.AsHttpClient());
		GameRecord = new GameRecordClient(baseUrl, Core.Client.Client.AsHttpClient());
		Guide = new GuideClient(baseUrl, Core.Client.Client.AsHttpClient());
		IndexBook = new IndexBookClient(baseUrl, Core.Client.Client.AsHttpClient());
		MailBox = new MailBoxClient(baseUrl, Core.Client.Client.AsHttpClient());
		GiftCode = new GiftCodeClient(baseUrl, Core.Client.Client.AsHttpClient());
		PleaseReviewPopup = new PleaseReviewPopupClient(baseUrl, Core.Client.Client.AsHttpClient());
		DataOptimization = new DataOptimizationClient(baseUrl, Core.Client.Client.AsHttpClient());
		ProfileData = new ProfileDataClient(baseUrl, Core.Client.Client.AsHttpClient());
		Progress = new ProgressClient(baseUrl, Core.Client.Client.AsHttpClient());
		Purchase = new PurchaseClient(baseUrl, Core.Client.Client.AsHttpClient());
		PVP = new PVPClient(baseUrl, Core.Client.Client.AsHttpClient());
		Rank = new RankClient(baseUrl, Core.Client.Client.AsHttpClient());
		Routine = new RoutineClient(baseUrl, Core.Client.Client.AsHttpClient());
		Season = new SeasonClient(baseUrl, Core.Client.Client.AsHttpClient());
		Skill = new SkillClient(baseUrl, Core.Client.Client.AsHttpClient());
		PlayerReport = new PlayerReportClient(baseUrl, Core.Client.Client.AsHttpClient());
		Loot = new LootClient(baseUrl, Core.Client.Client.AsHttpClient());
		LimitedTimeActivity = new LimitedTimeActivityClient(baseUrl, Core.Client.Client.AsHttpClient());
		IntermissionPlay = new IntermissionPlayClient(baseUrl, Core.Client.Client.AsHttpClient());
		YuanQiActivity = new YuanQiActivityClient(baseUrl, Core.Client.Client.AsHttpClient());
		NewbieFestival = new NewbieFestivalClient(baseUrl, Core.Client.Client.AsHttpClient());
		TestClient = new TestClient(baseUrl, Core.Client.Client.AsHttpClient());
		ForceUpdate = new ForceUpdateClient(baseUrl, MinimalChillyRoomService.GetMinimalSDK().HttpClient);
		return this;
	}

	public void AfterLogin()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)CSCommon.netWorkEndPointType == 0)
		{
			AccountStateManager stateManager = Sdk.Accounts.StateManager;
			MinimalChillyRoomService.GetMinimalSDK().AddContext("x-user-id", stateManager.State.User.Id.ToString());
			MinimalChillyRoomService.GetMinimalSDK().AddContext("x-player-id", stateManager.State.User.PlayerId.ToString());
			MinimalChillyRoomService.GetMinimalSDK().AddContext("x-player-shard", "1051");
		}
	}
}
