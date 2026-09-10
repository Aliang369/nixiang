using System;
using System.Globalization;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class ReocrdInfoPanel : UIBase, ISelectablePanel
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private TextMeshProUGUI recordTitleText;

	[SerializeField]
	private TextMeshProUGUI playerIdText;

	[SerializeField]
	private TextMeshProUGUI finishDateTimeText;

	[SerializeField]
	private TextMeshProUGUI characterNameText;

	[FormerlySerializedAs("characterSkinNameText")]
	[SerializeField]
	private TextMeshProUGUI characterClassNameText;

	[SerializeField]
	private TextMeshProUGUI contractNameText;

	[SerializeField]
	private TextMeshProUGUI contractDescriptionText;

	[SerializeField]
	private TextMeshProUGUI recordFinishTimeText;

	[SerializeField]
	private Image characterAvater;

	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private Transform itemDisplayParent;

	[SerializeField]
	private InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	private Transform influenceParent;

	[SerializeField]
	private RecordSkillCell recordSkillCellPrefab;

	[SerializeField]
	private Transform recordSkillParent;

	[SerializeField]
	public ItemInfoDisplayBase itemInfoDisplay;

	[SerializeField]
	public SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	public InfluenceDisplay influenceInfoDisplay;

	[SerializeField]
	public RectTransform gameJournalArea;

	[SerializeField]
	public TextMeshProUGUI gameJournalText;

	[SerializeField]
	public GameButton closeGameJournalAreaButton;

	[SerializeField]
	public GameButton openGameJournalAreaButton;

	[SerializeField]
	public RecordRankingScoreDetailPanel recordRankingScoreDetailPanel;

	[SerializeField]
	public TextMeshProUGUI rankingScoreText;

	[SerializeField]
	public GameButton openRankingScoreDetailButton;

	[SerializeField]
	private DailyChallengeRecordSettlementPanel dailyChallengeSettlementPanel;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceCellPool;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPrefabPool;

	[NonSerialized]
	private GameRecord record;

	[NonSerialized]
	private CellUIPrefabPool<RecordSkillCell, GameRecord.SkillRecord> skillDisplayPrefabPool;

	[NonSerialized]
	private Transform overlayOriginalParent;

	[NonSerialized]
	private int overlayOriginalSiblingIndex;

	[NonSerialized]
	private Vector2 overlayOriginalAnchorMin;

	[NonSerialized]
	private Vector2 overlayOriginalAnchorMax;

	[NonSerialized]
	private Vector2 overlayOriginalPivot;

	[NonSerialized]
	private Vector2 overlayOriginalAnchoredPosition;

	[NonSerialized]
	private Vector2 overlayOriginalSizeDelta;

	public void Setup(GameRecord r)
	{
		record = r;
		SetDirty();
	}

	public void Unset()
	{
		SetDirty();
	}

	public void ShowAsOverlay(GameRecord value, Transform overlayRoot)
	{
		if (value == null || overlayRoot == null)
		{
			return;
		}
		if (overlayOriginalParent == null)
		{
			overlayOriginalParent = base.transform.parent;
			overlayOriginalSiblingIndex = base.transform.GetSiblingIndex();
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null)
			{
				overlayOriginalAnchorMin = rectTransform.anchorMin;
				overlayOriginalAnchorMax = rectTransform.anchorMax;
				overlayOriginalPivot = rectTransform.pivot;
				overlayOriginalAnchoredPosition = rectTransform.anchoredPosition;
				overlayOriginalSizeDelta = rectTransform.sizeDelta;
			}
		}
		base.transform.SetParent(overlayRoot, worldPositionStays: false);
		RectTransform rectTransform2 = base.transform as RectTransform;
		if (rectTransform2 != null)
		{
			rectTransform2.anchorMin = Vector2.zero;
			rectTransform2.anchorMax = Vector2.one;
			rectTransform2.offsetMin = Vector2.zero;
			rectTransform2.offsetMax = Vector2.zero;
		}
		base.transform.SetAsLastSibling();
		Setup(value);
		base.gameObject.SetActive(value: true);
	}

	protected override void Init()
	{
		InitUI(itemInfoDisplay);
		InitUI(skillInfoDisplay);
		InitUI(influenceInfoDisplay);
		InitUI(recordRankingScoreDetailPanel);
		itemDisplayPrefabPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemDisplayParent);
		influenceCellPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, influenceParent);
		skillDisplayPrefabPool = GetPrefabPool<RecordSkillCell, GameRecord.SkillRecord>(recordSkillCellPrefab, recordSkillParent);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		closeGameJournalAreaButton.onClick.AddListener(_OnClickCloseGameJournalAreaButton);
		openGameJournalAreaButton.onClick.AddListener(_OnClickOpenGameJournalAreaButton);
		openRankingScoreDetailButton.onClick.AddListener(_OnClickOpenRankingScoreDetailButton);
		UIBase.fortressState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyUIAreaClicked);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.fortressState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyUIAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		bool num = dailyChallengeSettlementPanel != null && dailyChallengeSettlementPanel.Setup(record);
		playerIdText.text = record.playerName + "(" + record.playerId + ")";
		recordTitleText.text = record.GameOverTitle;
		finishDateTimeText.text = record.finishTime.ToLocalTime().ToString(CultureInfo.CurrentCulture);
		characterNameText.text = record.character.characterName;
		characterClassNameText.text = record.character.attachClass.characterName;
		contractNameText.text = record.contract.ContractName;
		contractDescriptionText.text = record.contract.GetContractDescription(UIBase.applicationState, out var _);
		if (!num)
		{
			DailyChallengeRunContext dailyChallengeContext = record.dailyChallengeContext;
			if (dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge && record.dailyChallengeContext.scoreCompetitionContract != null)
			{
				TextMeshProUGUI textMeshProUGUI = contractDescriptionText;
				textMeshProUGUI.text = textMeshProUGUI.text + Environment.NewLine + Environment.NewLine + "得分竞赛契约" + Environment.NewLine + DailyChallengeDisplayText.GetScoreContractText(record.dailyChallengeContext.scoreCompetitionContract);
			}
		}
		recordFinishTimeText.text = TextDisplayHelper.GetDetailTimeSpanText(record.gameTime);
		characterAvater.sprite = record.character.fullBody;
		itemDisplayPrefabPool.DespawnAll();
		foreach (GameRecord.ItemRecord item in record.deck)
		{
			itemDisplayPrefabPool.Spawn(item.DisplayItem);
		}
		influenceCellPool.DespawnAll();
		foreach (GameRecord.InfluenceRecord influence in record.influences)
		{
			influenceCellPool.Spawn(influence.DisplayInfluence);
		}
		skillDisplayPrefabPool.DespawnAll();
		foreach (GameRecord.SkillRecord skillInfo in record.skillInfos)
		{
			RecordSkillCell skillCell = skillDisplayPrefabPool.Spawn(skillInfo);
			RecordSkillCell recordSkillCell = skillCell;
			recordSkillCell.onClick = (Action)Delegate.Combine(recordSkillCell.onClick, (Action)delegate
			{
				HideAllInfoDisplays();
				skillInfoDisplay.Setup(skillCell.skillInfo.DisplaySkill);
			});
		}
		gameJournalText.text = record.gameJournal.GenerateJournalText(UIBase.fortressState.applicationState);
		gameJournalText.text += record.GenerateDetailText();
		rankingScoreText.text = record.rankScore.ToString();
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		HideAllInfoDisplays();
		itemInfoDisplay.Setup(item);
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		itemInfoDisplay.Unset();
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return itemInfoDisplay.item == item;
	}

	private void _OnBackButtonClicked()
	{
		itemInfoDisplay.Unset();
		base.gameObject.SetActive(value: false);
		RestoreOverlayParent();
	}

	private void RestoreOverlayParent()
	{
		if (!(overlayOriginalParent == null))
		{
			base.transform.SetParent(overlayOriginalParent, worldPositionStays: false);
			base.transform.SetSiblingIndex(Mathf.Min(overlayOriginalSiblingIndex, overlayOriginalParent.childCount - 1));
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = overlayOriginalAnchorMin;
				rectTransform.anchorMax = overlayOriginalAnchorMax;
				rectTransform.pivot = overlayOriginalPivot;
				rectTransform.anchoredPosition = overlayOriginalAnchoredPosition;
				rectTransform.sizeDelta = overlayOriginalSizeDelta;
			}
			overlayOriginalParent = null;
		}
	}

	private void _OnEmptyUIAreaClicked()
	{
		HideAllInfoDisplays();
	}

	private void _OnClickOpenGameJournalAreaButton()
	{
		gameJournalArea.gameObject.SetActive(value: true);
	}

	private void _OnClickCloseGameJournalAreaButton()
	{
		gameJournalArea.gameObject.SetActive(value: false);
	}

	private void _OnClickOpenRankingScoreDetailButton()
	{
		recordRankingScoreDetailPanel.Setup(record);
		recordRankingScoreDetailPanel.gameObject.SetActive(value: true);
	}

	private void HideAllInfoDisplays()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UIHelper.HideAllInfo(null);
		}
		itemInfoDisplay.Unset();
		skillInfoDisplay.Unset();
		influenceInfoDisplay.Unset();
	}
}
