using System;
using System.Reflection;
using ChillyRoom.Services.Accounts.Sdk;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootstrapExtra : MonoBehaviour
{
	public static BootstrapExtra instance;

	[SerializeField]
	public LoadingPanel loadingPanel;

	[SerializeField]
	public DebugPanel debugPanel;

	[SerializeField]
	public RectTransform chillyRoomViewsParent;

	[SerializeField]
	public AsyncOperationWaitPanel asyncOperationWaitPanelPrefab;

	[SerializeField]
	public WaitPanelBase waitPanelBase;

	[SerializeField]
	public BlockPanel blockPanel;

	[SerializeField]
	public ADPanel adPanel;

	[SerializeField]
	public RectTransform asyncOperationWaitPanelParent;

	[SerializeField]
	private TMP_FontAsset chillyRoomFont;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	public Image bootstrapBackground;

	[NonSerialized]
	public GameAsyncOperationManager gameAsyncOperationManager;

	[NonSerialized]
	public FortressReturnTransitionCover fortressReturnTransitionCover;

	[NonSerialized]
	private WatermarkOverlay watermarkOverlay;

	[NonSerialized]
	private HiddenWatermarkOverlay hiddenWatermarkOverlay;

	[NonSerialized]
	private bool? watermarkEnabled;

	private void Awake()
	{
		instance = this;
		if (CSCommon.showDebugPanel)
		{
			debugPanel.Init();
		}
		else
		{
			debugPanel.gameObject.SetActive(value: false);
		}
		loadingPanel.Init();
		waitPanelBase.Init();
		blockPanel.Init();
		adPanel.Init();
		fortressReturnTransitionCover = FortressReturnTransitionCover.Create(loadingPanel);
		gameAsyncOperationManager = new GameAsyncOperationManager(this);
		typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(TMP_Settings.instance, chillyRoomFont);
		UpdateVersionText();
	}

	private void Update()
	{
		gameAsyncOperationManager.Update();
		TryEnsureWatermark();
	}

	private void TryEnsureWatermark()
	{
		if (watermarkEnabled == false || !MinimalChillyRoomService.CheckSDKInit)
		{
			return;
		}
		GlobalConfigManager.DistroConfigInfo distroConfigInfo = GlobalConfigManager.instance?.CurrentDistroConfigInfo;
		if (distroConfigInfo == null)
		{
			return;
		}
		watermarkEnabled = distroConfigInfo.isCreatorDistro;
		if (watermarkEnabled == true)
		{
			if (watermarkOverlay == null)
			{
				watermarkOverlay = WatermarkOverlay.Create(base.transform);
			}
			if (hiddenWatermarkOverlay == null)
			{
				hiddenWatermarkOverlay = HiddenWatermarkOverlay.Create(base.transform);
			}
		}
	}

	public string GetVersionText()
	{
		string displayVersion = AppVersionDisplay.GetDisplayVersion();
		try
		{
			AccountStateManager accountStateManager = AccountStateManager.Instance;
			if (accountStateManager != null && accountStateManager.HasLoggedIn)
			{
				return $"PID{ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId}  {displayVersion}";
			}
		}
		catch
		{
		}
		return displayVersion;
	}

	public void UpdateVersionText()
	{
		versionText.text = GetVersionText();
	}
}
