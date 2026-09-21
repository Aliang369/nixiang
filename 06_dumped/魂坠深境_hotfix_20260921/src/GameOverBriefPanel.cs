using System;
using System.Collections;
using System.Text;
using Common.UI;
using CommonUtil.NShare;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverBriefPanel : UIBase
{
	[SerializeField]
	public float detailInterval = 0.3f;

	[SerializeField]
	public CanvasGroup canvasGroup;

	[SerializeField]
	protected ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	protected Transform itemSlotDisplayParent;

	[SerializeField]
	protected GridLayoutGroup itemGridLayoutGroup;

	[SerializeField]
	protected InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	protected Transform influenceSlotDisplayParent;

	[SerializeField]
	protected GridLayoutGroup influenceGridLayoutGroup;

	[SerializeField]
	protected SkillDisplay skillDisplayPrefab;

	[SerializeField]
	protected Transform skillSlotDisplayParent;

	[SerializeField]
	protected GridLayoutGroup skillGridLayoutGroup;

	[SerializeField]
	public RectTransform gameJournalArea;

	[SerializeField]
	public TextMeshProUGUI gameJournalText;

	[SerializeField]
	public GameButton closeGameJournalAreaButton;

	[SerializeField]
	public GameButton openGameJournalAreaButton;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI briefDetailText;

	[SerializeField]
	private TextMeshProUGUI contractNameText;

	[SerializeField]
	private TextMeshProUGUI contractDescriptionText;

	[SerializeField]
	private RectTransform contractArea;

	[SerializeField]
	private ExpChangeRecordCell ExpChangeRecordCellPrefab;

	[SerializeField]
	private Transform ExpChangeRecordPivot;

	[SerializeField]
	private MatterCell commonCurrencyCell;

	[SerializeField]
	private TextMeshProUGUI expSumDisplayText;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> skillDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<ExpChangeRecordCell, GameRecord.ExpChangeRecord> expChangeRecordCellPool;

	[NonSerialized]
	private GameRecord record;

	[NonSerialized]
	private WrapperEnumerator toUnlockPanelCoroutine;

	[NonSerialized]
	private Vector2 itemGridLayoutDefaultSpacing;

	[NonSerialized]
	private Vector2 influenceGridLayoutDefaultSpacing;

	[NonSerialized]
	private Vector2 skillGridLayoutDefaultSpacing;

	[NonSerialized]
	private float contractAreaDefaultPreferredHeight = -1f;

	[NonSerialized]
	private bool shareInProgress;

	[SerializeField]
	private DailyChallengeTextPanel dailyChallengeScorePanel;

	[SerializeField]
	public GameButton shareButton;

	protected override void Init()
	{
		InitUI(commonCurrencyCell);
		UniversalCommonInfoPanel universalCommonInfoPanel = UIBase.gameState.UIManager.universalCommonInfoPanel;
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemSlotDisplayParent);
		itemDisplayPool.ConfigInfoPanel(universalCommonInfoPanel);
		influenceDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, influenceSlotDisplayParent);
		influenceDisplayPool.ConfigInfoPanel(universalCommonInfoPanel);
		skillDisplayPool = GetPrefabPool<SkillDisplay, GameSkill>(skillDisplayPrefab, skillSlotDisplayParent);
		skillDisplayPool.ConfigInfoPanel(universalCommonInfoPanel);
		expChangeRecordCellPool = GetPrefabPool<ExpChangeRecordCell, GameRecord.ExpChangeRecord>(ExpChangeRecordCellPrefab, ExpChangeRecordPivot);
		itemGridLayoutDefaultSpacing = itemGridLayoutGroup.spacing;
		influenceGridLayoutDefaultSpacing = influenceGridLayoutGroup.spacing;
		skillGridLayoutDefaultSpacing = skillGridLayoutGroup.spacing;
		openGameJournalAreaButton.onClick.AddListener(_OnClickOpenGameJournalAreaButton);
		closeGameJournalAreaButton.onClick.AddListener(_OnClickCloseGameJournalAreaButton);
		shareButton.onClick.AddListener(_OnClickEndGameShareButton);
	}

	public void Setup(GameRecord record)
	{
		shareInProgress = false;
		this.record = record;
		base.gameObject.SetActive(value: true);
		SetupGameRecordBrief();
		UIBase.gameState.StartCoroutine(() => DisplayExpRecordCoroutine(this.record));
		SetDirty();
	}

	private void SetupGameRecordBrief()
	{
		StringBuilder stringBuilder = new StringBuilder();
		TimeSpan timeSpan = new TimeSpan(record.gameTime);
		stringBuilder.Append(ScriptableObjectSingleton<TextDataPreset>.instance.term_game_record_time_arg0.FormatWith(timeSpan.ToString("hh\\:mm\\:ss")) + Environment.NewLine);
		stringBuilder.Append(ScriptableObjectSingleton<TextDataPreset>.instance.term_game_progress_arg0.FormatWith(record.progress) + Environment.NewLine);
		Contract contract = record.contract;
		if (contract == null || contract.contractDef?.IsDifficultyIsolated != true)
		{
			stringBuilder.Append(ScriptableObjectSingleton<TextDataPreset>.instance.term_difficulty_arg0.FormatWith(UIBase.applicationState.difficultyManager.GetDifficultyInfo(record.contract.difficultyLevel).difficultyName) + Environment.NewLine);
		}
		briefDetailText.text = stringBuilder.ToString();
		titleText.text = record.GameOverTitle;
		commonCurrencyCell.gameObject.SetActive(value: false);
		contractArea.gameObject.SetActive(record.contract.contractDef.contractType != ContractDef.ContractType.BaseGame);
		contractNameText.text = record.contract.ContractName;
		contractDescriptionText.text = record.contract.GetContractDescription(UIBase.applicationState, out var _);
		DailyChallengeSettlement dailyChallengeSettlement = record.dailyChallengeSettlement;
		DailyChallengeScoreContractPresentation contract2 = dailyChallengeSettlement?.scoreContract ?? record.dailyChallengeContext?.scoreCompetitionContract;
		DailyChallengeRunContext dailyChallengeContext = record.dailyChallengeContext;
		bool flag = dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge && dailyChallengeSettlement != null;
		ApplyDailyChallengeMode(flag);
		dailyChallengeScorePanel?.Setup(flag ? DailyChallengeDisplayText.GetSettlementTitleText(contract2) : ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_score_breakdown_title, DailyChallengeDisplayText.GetGameOverSettlementText(dailyChallengeSettlement), flag);
		itemDisplayPool.DespawnAll();
		foreach (GameRecord.ItemRecord item in record.deck)
		{
			itemDisplayPool.Spawn(item.DisplayItem);
		}
		itemGridLayoutGroup.AutoFitLayoutXSpacing(2, itemGridLayoutDefaultSpacing.x);
		influenceDisplayPool.DespawnAll();
		foreach (GameRecord.InfluenceRecord influence in record.influences)
		{
			influenceDisplayPool.Spawn(influence.DisplayInfluence);
		}
		influenceGridLayoutGroup.AutoFitLayoutXSpacing(1, influenceGridLayoutDefaultSpacing.x);
		skillDisplayPool.DespawnAll();
		foreach (GameRecord.SkillRecord skillInfo in record.skillInfos)
		{
			skillDisplayPool.Spawn(skillInfo.DisplaySkill);
		}
		skillGridLayoutGroup.AutoFitLayoutXSpacing(1, skillGridLayoutDefaultSpacing.x);
		gameJournalText.text = record.gameJournal.GenerateJournalText(UIBase.applicationState);
		gameJournalText.text += record.GenerateDetailText();
	}

	private void ApplyDailyChallengeMode(bool isDailyChallenge)
	{
		contractNameText.gameObject.SetActive(!isDailyChallenge);
		contractDescriptionText.gameObject.SetActive(!isDailyChallenge);
		dailyChallengeScorePanel?.gameObject.SetActive(isDailyChallenge);
		Image component = contractArea.GetComponent<Image>();
		if (component != null)
		{
			component.enabled = !isDailyChallenge;
		}
		LayoutElement component2 = contractArea.GetComponent<LayoutElement>();
		if (!(component2 == null))
		{
			if (contractAreaDefaultPreferredHeight < 0f)
			{
				contractAreaDefaultPreferredHeight = component2.preferredHeight;
			}
			component2.preferredHeight = (isDailyChallenge ? 270f : contractAreaDefaultPreferredHeight);
			LayoutRebuilder.MarkLayoutForRebuild(contractArea.parent as RectTransform);
		}
	}

	private IEnumerator DisplayExpRecordCoroutine(GameRecord record)
	{
		yield return new WaitUntil(() => record.gameoverResolveFinished);
		expChangeRecordCellPool.DespawnAll();
		expSumDisplayText.text = "0";
		for (int i = 0; i < record.expChangeRecords.Count; i++)
		{
			expChangeRecordCellPool.Spawn(record.expChangeRecords[i]);
			expSumDisplayText.text = GameRecord.ExpChangeRecord.GetExpSum(record.expChangeRecords.GetRange(0, i + 1)).ToString();
			yield return new WaitForSeconds(detailInterval);
		}
		commonCurrencyCell.gameObject.SetActive(value: true);
		commonCurrencyCell.Setup(new MatterInfo(UIBase.applicationState.matterManager.CommonCurrencyMatter.matterDef, record.commonCurrency));
	}

	private void _OnClickOpenGameJournalAreaButton()
	{
		gameJournalArea.gameObject.SetActive(value: true);
	}

	private void _OnClickCloseGameJournalAreaButton()
	{
		gameJournalArea.gameObject.SetActive(value: false);
	}

	private void _OnClickEndGameShareButton()
	{
		if (shareInProgress)
		{
			return;
		}
		shareInProgress = true;
		try
		{
			StartCoroutine(ShareAndReportActivityMission());
		}
		catch
		{
			shareInProgress = false;
			throw;
		}
	}

	private IEnumerator ShareAndReportActivityMission()
	{
		GameOverPanel gameOverPanel = GetUIInParents<GameOverPanel>();
		try
		{
			yield return NativeShareManager.Share(delegate(bool isSuccess)
			{
				UIBase.applicationState.eventManager.shareCompletedEvent.TriggerEvent(isSuccess);
			}, gameOverPanel.BeginShareCapture, gameOverPanel.RestoreShareCaptureControls);
			UIBase.applicationState.eventManager.activityMissionShareEvent.TriggerEvent(new YuanQiActivityMissionShareEventData(record, record.character, "GameOverBriefPanel"));
		}
		finally
		{
			GameOverBriefPanel gameOverBriefPanel = this;
			gameOverPanel.RestoreShareCaptureControls();
			gameOverBriefPanel.shareInProgress = false;
		}
	}
}
