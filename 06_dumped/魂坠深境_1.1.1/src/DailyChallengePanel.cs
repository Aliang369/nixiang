using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.UI;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyChallengePanel : UIBase, IFrameUpdate
{
	[Header("Navigation")]
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI backButtonLabel;

	[Header("Activity")]
	[SerializeField]
	private TextMeshProUGUI activityTitleText;

	[SerializeField]
	private GameButton activityHelpButton;

	[SerializeField]
	private TextMeshProUGUI activityHelpButtonLabel;

	[SerializeField]
	private Image constellationImage;

	[SerializeField]
	private TextMeshProUGUI constellationHintText;

	[SerializeField]
	private TextMeshProUGUI completionRewardTitleText;

	[SerializeField]
	private GlobalItemDisplay rewardDisplay;

	[SerializeField]
	private TextMeshProUGUI rewardCountText;

	[SerializeField]
	private TextMeshProUGUI weeklyCountdownLabelText;

	[SerializeField]
	private TextMeshProUGUI weeklyCountdownText;

	[Header("Challenge")]
	[SerializeField]
	private TextMeshProUGUI dailyCountdownLabelText;

	[SerializeField]
	private TextMeshProUGUI challengeRemainingText;

	[SerializeField]
	private TextMeshProUGUI gameplayContractLabel;

	[SerializeField]
	private TextMeshProUGUI gameplayContractText;

	[SerializeField]
	private TextMeshProUGUI scoreContractLabel;

	[SerializeField]
	private TextMeshProUGUI scoreContractText;

	[SerializeField]
	private GameButton startButton;

	[SerializeField]
	private TextMeshProUGUI startButtonLabel;

	[Header("Ranking")]
	[SerializeField]
	private TextMeshProUGUI rankingTitleText;

	[SerializeField]
	private List<DailyChallengeRankingRewardTierView> rankingRewardTierViews = new List<DailyChallengeRankingRewardTierView>();

	[SerializeField]
	private GameButton rankingHelpButton;

	[SerializeField]
	private TextMeshProUGUI rankingHelpButtonLabel;

	[SerializeField]
	private TextMeshProUGUI rankingRankHeaderText;

	[SerializeField]
	private TextMeshProUGUI rankingPlayerHeaderText;

	[SerializeField]
	private TextMeshProUGUI rankingDifficultyHeaderText;

	[SerializeField]
	private TextMeshProUGUI rankingScoreHeaderText;

	[SerializeField]
	private TextMeshProUGUI rankingStatusText;

	[SerializeField]
	private List<DailyChallengeRankingRowView> rankingRows = new List<DailyChallengeRankingRowView>();

	[SerializeField]
	private DailyChallengeRankingRowView selfRankingRow;

	[Header("Help Overlay")]
	[SerializeField]
	private RectTransform helpOverlay;

	[SerializeField]
	private TextMeshProUGUI helpTitleText;

	[SerializeField]
	private TextMeshProUGUI helpContentText;

	[SerializeField]
	private List<GameButton> helpTabButtons = new List<GameButton>();

	[SerializeField]
	private List<TextMeshProUGUI> helpTabLabels = new List<TextMeshProUGUI>();

	[SerializeField]
	private Sprite helpTabNormalSprite;

	[SerializeField]
	private Sprite helpTabSelectedSprite;

	[SerializeField]
	private GameButton closeHelpButton;

	[SerializeField]
	private TextMeshProUGUI closeHelpButtonLabel;

	private IDailyChallengeAsyncDataProvider provider;

	private IDailyChallengeClock clock;

	private DailyChallengePageState state;

	private StarMapPanel starMapPanel;

	private DailyChallengeClientConfig clientConfig;

	private DailyChallengeMockScenario mockScenario;

	private bool useMockProvider;

	[NonSerialized]
	private TextLinkObjectDisplayer gameplayContractLinkDisplayer;

	private string[] helpTabTitles = Array.Empty<string>();

	private string[] helpTabContents = Array.Empty<string>();

	private int activeHelpTab;

	private float nextClockRefresh;

	private bool refreshedAtBoundary;

	private ReocrdInfoPanel dailyChallengeRecordPanel;

	private bool isRefreshing;

	private bool refreshAfterCurrentRequest;

	private string loadError;

	private int requestVersion;

	public IReadOnlyList<DailyChallengeRankingRowView> RankingRows => rankingRows;

	public IReadOnlyList<DailyChallengeRankingRewardTierView> RankingRewardTierViews => rankingRewardTierViews;

	public DailyChallengeRankingRowView SelfRankingRow => selfRankingRow;

	public void Setup(StarMapPanel host, DailyChallengeClientConfig config, DailyChallengeMockScenario scenario, bool useMock)
	{
		starMapPanel = host;
		clientConfig = config;
		mockScenario = scenario;
		useMockProvider = useMock;
	}

	protected override void Init()
	{
		if (starMapPanel == null || clientConfig == null)
		{
			throw new InvalidOperationException("DailyChallengePanel must be set up before InitUI.");
		}
		backButton.onClick.AddListener(Close);
		gameplayContractLinkDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, gameplayContractText, gameplayContractText.rectTransform);
		activityHelpButton.onClick.AddListener(OpenHelp);
		rankingHelpButton.gameObject.SetActive(value: false);
		startButton.onClick.AddListener(HandlePrimaryAction);
		for (int i = 0; i < helpTabButtons.Count; i++)
		{
			int tabIndex = i;
			helpTabButtons[i].onClick.AddListener(delegate
			{
				SelectHelpTab(tabIndex);
			});
		}
		closeHelpButton.onClick.AddListener(delegate
		{
			helpOverlay.gameObject.SetActive(value: false);
		});
		EnsureRankingRows();
		foreach (DailyChallengeRankingRowView rankingRow in rankingRows)
		{
			rankingRow.Initialize(OpenRecord);
		}
		selfRankingRow.Initialize(OpenRecord);
		EnsureRecordPanel();
		InitUI(rewardDisplay);
		constellationImage.sprite = clientConfig.constellationPlaceholder;
		ApplyLocalizedTexts();
		clock = new SystemDailyChallengeClock();
		provider = CreateProvider();
		helpOverlay.gameObject.SetActive(value: false);
	}

	private void EnsureRankingRows()
	{
		int num = Mathf.Max(1, clientConfig.leaderboardRowLimit);
		if (rankingRows.Count < num)
		{
			if (rankingRows.Count == 0 || rankingRows[0] == null || clientConfig.rankingRowPrefab == null)
			{
				throw new InvalidOperationException("DailyChallengePanel requires a ranking row template.");
			}
			Transform transform = rankingRows[0].transform.parent;
			while (rankingRows.Count < num)
			{
				DailyChallengeRankingRowView dailyChallengeRankingRowView = UnityEngine.Object.Instantiate(clientConfig.rankingRowPrefab, transform);
				dailyChallengeRankingRowView.name = $"RankingRow{rankingRows.Count + 1}";
				rankingRows.Add(dailyChallengeRankingRowView);
			}
		}
	}

	public async void Open()
	{
		provider = CreateProvider();
		state = null;
		loadError = null;
		isRefreshing = false;
		refreshedAtBoundary = false;
		base.gameObject.SetActive(value: true);
		base.transform.SetAsLastSibling();
		SetDirty();
		await RefreshProviderAsync();
	}

	protected override void UpdateDisplay()
	{
		ApplyLocalizedTexts();
		RefreshLocalizedHelpIfOpen();
		if (state == null)
		{
			state = provider?.GetCurrentState();
		}
		if (state == null)
		{
			ApplyUnavailableState();
			return;
		}
		startButton.interactable = true;
		startButtonLabel.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_start;
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		int num = Mathf.Max(0, state.rewardLimitPerWeek - state.rewardClaimedThisWeek);
		rewardCountText.text = instance.term_daily_challenge_reward_status_arg0_arg1.FormatWith(state.rewardLimitPerWeek, num);
		ApplyCompletionReward();
		ApplyRankingRewards();
		rankingStatusText.text = string.Empty;
		if (state.gameplayContract == null)
		{
			gameplayContractLabel.text = string.Empty;
			gameplayContractText.text = instance.term_daily_challenge_contract_missing;
			gameplayContractLinkDisplayer.SetGenerator(null);
		}
		else
		{
			gameplayContractLabel.text = state.gameplayContract.Name;
			gameplayContractText.text = state.gameplayContract.GetContractDescription(UIBase.applicationState, out var generator);
			gameplayContractLinkDisplayer.SetGenerator(generator);
		}
		if (state.scoreCompetitionContract == null)
		{
			scoreContractLabel.text = string.Empty;
			scoreContractText.text = instance.term_daily_challenge_contract_missing;
		}
		else
		{
			scoreContractLabel.text = state.scoreCompetitionContract.displayName;
			scoreContractText.text = DailyChallengeDisplayText.GetScoreContractBodyText(state.scoreCompetitionContract);
		}
		for (int i = 0; i < rankingRows.Count; i++)
		{
			DailyChallengeRankingRow row = ((i < state.topRanking.Count) ? state.topRanking[i] : null);
			rankingRows[i].Set(row, clientConfig.defaultAvatar);
		}
		if (state.playerRanking == null)
		{
			string currentPlayerName = UIBase.applicationState?.playerInfoManager?.DisplayName ?? string.Empty;
			selfRankingRow.SetUnchallenged(clientConfig.defaultAvatar, currentPlayerName);
		}
		else
		{
			selfRankingRow.Set(state.playerRanking, clientConfig.defaultAvatar, isSelf: true);
		}
		UpdateCountdownTexts(clock.UtcNow);
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && !(Time.unscaledTime < nextClockRefresh) && state != null)
		{
			nextClockRefresh = Time.unscaledTime + 1f;
			DateTime utcNow = clock.UtcNow;
			UpdateCountdownTexts(utcNow);
			if (!refreshedAtBoundary && utcNow >= GetChallengeBoundaryUtc())
			{
				refreshedAtBoundary = true;
				RefreshProviderAsync();
			}
		}
	}

	private void ApplyLocalizedTexts()
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		activityTitleText.text = instance.term_daily_challenge_title;
		backButtonLabel.text = string.Empty;
		activityHelpButtonLabel.text = string.Empty;
		constellationHintText.text = instance.term_daily_challenge_constellation_placeholder;
		completionRewardTitleText.text = instance.term_daily_challenge_completion_reward;
		dailyCountdownLabelText.text = instance.term_daily_challenge_daily_countdown;
		weeklyCountdownLabelText.text = instance.term_daily_challenge_weekly_countdown;
		startButtonLabel.text = instance.term_daily_challenge_start;
		rankingTitleText.text = instance.term_daily_challenge_ranking;
		rankingHelpButtonLabel.text = string.Empty;
		rankingRankHeaderText.text = instance.term_daily_challenge_ranking_rank;
		rankingPlayerHeaderText.text = instance.term_daily_challenge_ranking_player;
		rankingDifficultyHeaderText.text = instance.term_daily_challenge_ranking_difficulty;
		rankingScoreHeaderText.text = instance.term_daily_challenge_ranking_score;
		closeHelpButtonLabel.text = instance.term_daily_challenge_close;
	}

	private void StartChallenge()
	{
		if (!(state?.gameplayContract == null))
		{
			NewGameConfigData currentNewGameConfig = UIBase.applicationState.newGameConfigManager.currentNewGameConfig;
			if (currentNewGameConfig != null)
			{
				DifficultyLevel dailyChallengeInitialDifficulty = UIBase.applicationState.newGameConfigManager.GetDailyChallengeInitialDifficulty(currentNewGameConfig);
				Contract contract = state.gameplayContract.GenerateContract(UIBase.applicationState.assetCatalog, dailyChallengeInitialDifficulty);
				currentNewGameConfig.contract = contract;
				currentNewGameConfig.dailyChallengeContext = new DailyChallengeRunContext
				{
					challengeId = state.challengeId,
					isDailyChallenge = true,
					scoreCompetitionContract = state.scoreCompetitionContract,
					rotationResetUtc = state.rotationResetUtc,
					settlementUtc = state.settlementUtc
				};
				starMapPanel.gameConfigPanel.Setup(currentNewGameConfig);
				manager.ShowUI(starMapPanel.gameConfigPanel);
				Close();
			}
		}
	}

	private void HandlePrimaryAction()
	{
		if (!string.IsNullOrEmpty(loadError))
		{
			RefreshProviderAsync();
		}
		else
		{
			StartChallenge();
		}
	}

	private async void OpenRecord(long playerId, string recordId)
	{
		if (provider == null || string.IsNullOrWhiteSpace(recordId))
		{
			return;
		}
		int version = requestVersion;
		try
		{
			rankingStatusText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_loading;
			GameRecord gameRecord = await provider.GetRecordDetailAsync(playerId, recordId);
			if (version == requestVersion && gameRecord != null && base.gameObject.activeInHierarchy)
			{
				EnsureRecordPanel();
				Transform overlayRoot = ((UIBase.fortressState.uiManager.panels1 != null) ? UIBase.fortressState.uiManager.panels1 : base.transform.parent);
				dailyChallengeRecordPanel?.ShowAsOverlay(gameRecord, overlayRoot);
				SetDirty();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
			rankingStatusText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_record_load_failed;
		}
	}

	private IDailyChallengeAsyncDataProvider CreateProvider()
	{
		bool enableNetwork = GlobalConfigManager.instance.enableNetwork;
		if (enableNetwork && !useMockProvider)
		{
			return new DailyChallengeNetworkDataProvider(UIBase.applicationState, clientConfig, clock);
		}
		DailyChallengeMockScenario scenario = ((enableNetwork || useMockProvider) ? mockScenario : DailyChallengeMockScenario.FullRanking);
		return new DailyChallengeMockDataProvider(UIBase.applicationState, clientConfig, scenario, clock);
	}

	private async Task RefreshProviderAsync()
	{
		if (provider == null || isRefreshing)
		{
			return;
		}
		isRefreshing = true;
		loadError = null;
		int version = ++requestVersion;
		SetDirty();
		try
		{
			DailyChallengePageState dailyChallengePageState = await provider.RefreshAsync();
			if (version != requestVersion || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			state = dailyChallengePageState;
			refreshedAtBoundary = state != null && clock.UtcNow >= GetChallengeBoundaryUtc();
		}
		catch (Exception exception)
		{
			if (version != requestVersion)
			{
				return;
			}
			state = null;
			loadError = ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_load_failed;
			Debug.LogException(exception, this);
		}
		finally
		{
			if (version == requestVersion)
			{
				isRefreshing = false;
				SetDirty();
			}
		}
		if (version == requestVersion && refreshAfterCurrentRequest)
		{
			refreshAfterCurrentRequest = false;
			await RefreshProviderAsync();
		}
	}

	private void ApplyUnavailableState()
	{
		string text = (string.IsNullOrEmpty(loadError) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_loading : loadError);
		gameplayContractText.text = text;
		scoreContractText.text = text;
		gameplayContractLabel.text = string.Empty;
		scoreContractLabel.text = string.Empty;
		rankingStatusText.text = text;
		rewardCountText.text = text;
		weeklyCountdownText.text = string.Empty;
		challengeRemainingText.text = string.Empty;
		startButton.interactable = !isRefreshing && !string.IsNullOrEmpty(loadError);
		startButtonLabel.text = (string.IsNullOrEmpty(loadError) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_loading : ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_retry);
		foreach (DailyChallengeRankingRowView rankingRow in rankingRows)
		{
			rankingRow.Set(null, clientConfig.defaultAvatar);
		}
		selfRankingRow.Set(null, clientConfig.defaultAvatar, isSelf: true);
		rewardDisplay.gameObject.SetActive(value: false);
	}

	private void ApplyRankingRewards()
	{
		List<DailyChallengeClientConfig.LeaderboardRewardTier> leaderboardRewardTiers = clientConfig.leaderboardRewardTiers;
		Sprite icon = ((clientConfig.completionRewardMatter != null) ? clientConfig.completionRewardMatter.icon : null);
		for (int i = 0; i < rankingRewardTierViews.Count; i++)
		{
			DailyChallengeClientConfig.LeaderboardRewardTier leaderboardRewardTier = ((i < leaderboardRewardTiers.Count) ? leaderboardRewardTiers[i] : null);
			rankingRewardTierViews[i].gameObject.SetActive(leaderboardRewardTier != null);
			if (leaderboardRewardTier != null)
			{
				rankingRewardTierViews[i].Set(leaderboardRewardTier.maxRank, icon, leaderboardRewardTier.rewardAmount);
			}
		}
	}

	private void ApplyCompletionReward()
	{
		DailyChallengeRewardPresentation dailyChallengeRewardPresentation = ((state.completionRewards.Count > 0) ? state.completionRewards[0] : null);
		if (!(dailyChallengeRewardPresentation?.asset is MatterDef matterDef))
		{
			rewardDisplay.gameObject.SetActive(value: false);
			return;
		}
		rewardDisplay.gameObject.SetActive(value: true);
		rewardDisplay.Setup(new MatterReward(matterDef, dailyChallengeRewardPresentation.count));
	}

	private void EnsureRecordPanel()
	{
		if (!(dailyChallengeRecordPanel != null))
		{
			if (clientConfig.recordInfoPanelPrefab == null)
			{
				Debug.LogError("DailyChallengeClientConfig is missing its record info panel prefab.", clientConfig);
				return;
			}
			Transform transform = ((UIBase.fortressState.uiManager.panels1 != null) ? UIBase.fortressState.uiManager.panels1 : base.transform.parent);
			dailyChallengeRecordPanel = UnityEngine.Object.Instantiate(clientConfig.recordInfoPanelPrefab, transform);
			dailyChallengeRecordPanel.name = clientConfig.recordInfoPanelPrefab.name;
			dailyChallengeRecordPanel.gameObject.SetActive(value: false);
			InitUI(dailyChallengeRecordPanel);
		}
	}

	private void OpenHelp()
	{
		activeHelpTab = 0;
		helpOverlay.gameObject.SetActive(value: true);
		ApplyLocalizedHelp();
	}

	private void RefreshLocalizedHelpIfOpen()
	{
		if (helpOverlay.gameObject.activeSelf)
		{
			ApplyLocalizedHelp();
		}
	}

	private void ApplyLocalizedHelp()
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		helpTabTitles = instance.GetDailyChallengeHelpTabTitles();
		helpTabContents = instance.GetDailyChallengeHelpTabContents();
		if (helpTabContents == null || helpTabContents.Length == 0)
		{
			helpTabContents = new string[1] { instance.term_daily_challenge_help_missing };
		}
		if (helpTabTitles == null || helpTabTitles.Length != helpTabContents.Length)
		{
			helpTabTitles = new string[helpTabContents.Length];
		}
		activeHelpTab = Mathf.Clamp(activeHelpTab, 0, helpTabContents.Length - 1);
		RefreshHelpTab();
	}

	private void SelectHelpTab(int tabIndex)
	{
		if (tabIndex >= 0 && tabIndex < helpTabContents.Length)
		{
			activeHelpTab = tabIndex;
			RefreshHelpTab();
		}
	}

	private void RefreshHelpTab()
	{
		helpTitleText.text = helpTabTitles[activeHelpTab];
		helpContentText.text = (string.IsNullOrWhiteSpace(helpTabContents[activeHelpTab]) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_help_missing : DailyChallengeDisplayText.GetTmpSafeText(helpTabContents[activeHelpTab].Trim()));
		for (int i = 0; i < helpTabButtons.Count; i++)
		{
			bool flag = i == activeHelpTab;
			helpTabButtons[i].interactable = !flag;
			if (helpTabButtons[i].targetGraphic is Image image)
			{
				image.sprite = (flag ? helpTabSelectedSprite : helpTabNormalSprite);
			}
			if (i < helpTabLabels.Count)
			{
				helpTabLabels[i].text = ((i < helpTabTitles.Length) ? helpTabTitles[i] : string.Empty);
			}
		}
	}

	private void UpdateCountdownTexts(DateTime now)
	{
		_ = ScriptableObjectSingleton<TextDataPreset>.instance;
		weeklyCountdownText.text = FormatTime(DailyChallengeClock.Remaining(state.weeklyResetUtc, now));
		challengeRemainingText.text = FormatTime(DailyChallengeClock.Remaining(GetChallengeBoundaryUtc(), now));
	}

	private DateTime GetChallengeBoundaryUtc()
	{
		if (!(state.rotationResetUtc <= state.settlementUtc))
		{
			return state.settlementUtc;
		}
		return state.rotationResetUtc;
	}

	private void Close()
	{
		requestVersion++;
		helpOverlay.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	private static string FormatTime(TimeSpan span)
	{
		if (span.TotalDays >= 1.0)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_duration_days_arg0_arg1.FormatWith((int)span.TotalDays, $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}");
		}
		return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
	}
}
