using System;
using System.Linq;
using ContainerOfSoul.Display.Fortress.GameConfig;
using UnityEngine;

public class StarMapPanel : FortressUIManager.SubPanel
{
	public GameConfigPanel gameConfigPanel;

	public ConstellationDetailPanel constellationDetailPanel;

	public GameButton backButton;

	public RectTransform starMapParent;

	public ChallengeStarMap starMap;

	[SerializeField]
	private DailyChallengeClientConfig dailyChallengeConfig;

	[SerializeField]
	private bool overrideDailyChallengeMockScenario;

	[SerializeField]
	private DailyChallengeMockScenario dailyChallengeMockScenario;

	[NonSerialized]
	private DailyChallengePanel dailyChallengePanel;

	[NonSerialized]
	private DailyChallengeEntryView dailyChallengeEntry;

	[NonSerialized]
	private DailyChallengeEntryLayoutView dailyChallengeEntryLayout;

	protected override void Init()
	{
		base.Init();
		InitUI(gameConfigPanel);
		InitUI(constellationDetailPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		UIBase.applicationState.eventManager.ContractRefreshedEvent.AddListener(_OnDailyContractRefreshed);
		Setup(UIBase.applicationState.seasonManager.currentSeason.def.challengeStarMap);
		EnsureDailyChallengeUI();
		SetDirty();
	}

	public void Setup(ChallengeStarMap starMapPrefab)
	{
		if (starMap != null)
		{
			DisposeUI(starMap);
			UnityEngine.Object.Destroy(starMap.gameObject);
		}
		starMap = starMapPrefab.InstantiatePrefabIn(starMapParent);
		starMap.rectTransform.anchoredPosition = Vector2.zero;
		InitUI(starMap);
		RefreshChallengeEntryBindings();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.ContractRefreshedEvent.RemoveListener(_OnDailyContractRefreshed);
	}

	public void OpenConstellation(MapPreviewConstellation constellation)
	{
		OpenConstellation((constellation != null) ? constellation.constellation : null);
	}

	public void OpenConstellation(Constellation constellation)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		constellation = ResolveCatalogConstellation(constellation);
		if (!(constellation == null) && constellation.CanOpen(DateTime.UtcNow))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_stargaze_constellation_select);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "观星室-星座界面", constellation.name);
			constellationDetailPanel.Setup(constellation);
			constellationDetailPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			SetDirty();
		}
	}

	public Constellation ResolveCatalogConstellation(Constellation constellation)
	{
		if (constellation == null)
		{
			return null;
		}
		return UIBase.applicationState.assetCatalog.FullIdToCTAsset<Constellation>(constellation.FullId) ?? constellation;
	}

	public void OpenDailyChallenge()
	{
		TryOpenDailyChallenge();
	}

	public bool CanOpenDailyChallenge()
	{
		EnsureDailyChallengeUI();
		return dailyChallengePanel != null;
	}

	public bool TryOpenDailyChallenge()
	{
		if (!CanOpenDailyChallenge())
		{
			return false;
		}
		dailyChallengePanel.Open();
		return dailyChallengePanel.gameObject.activeSelf;
	}

	public bool TryResolveOpenConstellation(ContractDef contractDef, out Constellation constellation)
	{
		constellation = null;
		if (contractDef == null || starMap == null)
		{
			return false;
		}
		string contractId = contractDef.FullID;
		constellation = starMap.ChildrenConstellations.FirstOrDefault((MapPreviewConstellation candidate) => candidate != null && candidate.CanOpen() && (candidate.constellation?.AllContractDefs.Any((ContractDef definition) => definition != null && (definition == contractDef || definition.FullID == contractId)) ?? false))?.constellation;
		return constellation != null;
	}

	private void EnsureDailyChallengeUI()
	{
		if (dailyChallengeConfig == null)
		{
			Debug.LogError("StarMapPanel is missing DailyChallengeClientConfig.", this);
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = ((UIBase.fortressState.uiManager.panels1 != null) ? UIBase.fortressState.uiManager.panels1 : transform);
		if (dailyChallengeEntryLayout == null && dailyChallengeConfig.starMapEntryLayoutPrefab != null)
		{
			dailyChallengeEntryLayout = UnityEngine.Object.Instantiate(dailyChallengeConfig.starMapEntryLayoutPrefab, transform);
			dailyChallengeEntryLayout.name = dailyChallengeConfig.starMapEntryLayoutPrefab.name;
			RectTransform rectTransform = dailyChallengeEntryLayout.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
			}
			dailyChallengeEntryLayout.transform.SetAsLastSibling();
		}
		if (dailyChallengePanel == null)
		{
			if (dailyChallengeConfig.panelPrefab == null)
			{
				Debug.LogError("DailyChallengeClientConfig is missing its panel prefab.", dailyChallengeConfig);
				return;
			}
			dailyChallengePanel = UnityEngine.Object.Instantiate(dailyChallengeConfig.panelPrefab, transform2);
			dailyChallengePanel.name = dailyChallengeConfig.panelPrefab.name;
			dailyChallengePanel.gameObject.SetActive(value: false);
			DailyChallengeMockScenario scenario = (overrideDailyChallengeMockScenario ? dailyChallengeMockScenario : dailyChallengeConfig.defaultMockScenario);
			dailyChallengePanel.Setup(this, dailyChallengeConfig, scenario, overrideDailyChallengeMockScenario);
			InitUI(dailyChallengePanel);
		}
		RefreshChallengeEntryBindings();
		if (dailyChallengeEntryLayout == null && dailyChallengeEntry == null)
		{
			if (dailyChallengeConfig.entryPrefab == null)
			{
				Debug.LogError("DailyChallengeClientConfig is missing its entry prefab.", dailyChallengeConfig);
				return;
			}
			dailyChallengeEntry = UnityEngine.Object.Instantiate(dailyChallengeConfig.entryPrefab, transform);
			dailyChallengeEntry.name = dailyChallengeConfig.entryPrefab.name;
			dailyChallengeEntry.Setup(this);
		}
	}

	private void RefreshChallengeEntryBindings()
	{
		bool flag = dailyChallengeEntryLayout != null;
		if (starMap != null)
		{
			starMap.gameObject.SetActive(!flag);
		}
		if (dailyChallengeEntry != null)
		{
			dailyChallengeEntry.gameObject.SetActive(!flag);
		}
		if (flag)
		{
			dailyChallengeEntryLayout.SetupEntries(this, dailyChallengeConfig);
		}
	}

	private void _OnBackButtonClicked()
	{
		UIBase.fortressState.uiManager.ToState(UIBase.fortressState.uiManager.mainUIPanel);
		UIHelper.HideAllInfo(null);
	}

	private void _OnDailyContractRefreshed(ContractState contract)
	{
		RefreshChallengeEntryBindings();
		SetDirty();
	}
}
