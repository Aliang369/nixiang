using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaSet : CellUI<GachaPoolState>
{
	[Serializable]
	public class GachaSetJumpPreviewSetup
	{
		public GameButton button;

		public PlayerCharacterDef character;
	}

	[SerializeField]
	private TextMeshProUGUI poolName;

	[SerializeField]
	private TextMeshProUGUI time;

	[SerializeField]
	private TextMeshProUGUI remainGachaTimesText;

	[SerializeField]
	private GameObject remainGachaTimesTextArea;

	[SerializeField]
	private TextMeshProUGUI drawSingleCountText;

	[SerializeField]
	private TextMeshProUGUI drawCountText;

	[SerializeField]
	private GameObject gachaGrandPrizeArea;

	[SerializeField]
	private Image gachaGrandPrizeIcon;

	[SerializeField]
	private TextMeshProUGUI gachaGrandPrizeCounterText;

	[SerializeField]
	private TextMeshProUGUI gachaGrandPrizeRewardText;

	[SerializeField]
	public GameButton drawSingleButton;

	[SerializeField]
	private MatterCell drawSingleConsumeMatterCell;

	[SerializeField]
	private GameObject singleDrawFreeHint;

	[SerializeField]
	public GameButton drawTenButton;

	[SerializeField]
	private MatterCell drawTenConsumeMatterCell;

	[SerializeField]
	private GameObject tenDrawFreeHint;

	[SerializeField]
	private GameButton openGachaRuleInfoPanelButton;

	[SerializeField]
	public Animator animator;

	[SerializeField]
	private List<GachaSetJumpPreviewSetup> jumpPreviewSetups;

	[SerializeField]
	private GameButton selectWishButton;

	[SerializeField]
	private GameButton changeWishButton;

	[SerializeField]
	private GlobalItemDisplay currentWishGlobalItemDisplay;

	[SerializeField]
	private GameObject makeWishHint;

	[SerializeField]
	private GameObject changeWishHint;

	[SerializeField]
	private GameObject wishFinishedHint;

	[SerializeField]
	private GameButton characterIconButton;

	[SerializeField]
	private TextMeshProUGUI wishResetTime;

	public GameObject wishArea;

	public GameObject gachaWishHint;

	public GameObject gachaIntroPityHint;

	[NonSerialized]
	private GachaPanel parentGachaPanel;

	[NonSerialized]
	private bool drawRequestInFlight;

	public GachaPoolState PoolState => base.setupData;

	private MatterDef creditCurrencyMatter => ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter;

	private GachaResultDisplay gachaResultDisplay => CellUI<GachaPoolState>.fortressState.uiManager.gachaResultDisplay;

	private int MaxDrawCount
	{
		get
		{
			int num = 10;
			if (PoolState.def.enableGachaCountLimitRule && PoolState.RemainDrawTimesInThisLimitResetPeriod < num)
			{
				return PoolState.RemainDrawTimesInThisLimitResetPeriod;
			}
			if (PoolState.def.enableFreeDraw && PoolState.RemainFreeDrawTimesInThisResetPeriod > 0 && PoolState.RemainFreeDrawTimesInThisResetPeriod < num)
			{
				return PoolState.RemainFreeDrawTimesInThisResetPeriod;
			}
			return num;
		}
	}

	protected override void Init()
	{
		InitUI(drawSingleConsumeMatterCell);
		InitUI(drawTenConsumeMatterCell);
		InitUI(currentWishGlobalItemDisplay);
		parentGachaPanel = GetUIInParents<GachaPanel>();
		drawSingleButton.onClick.AddListener(delegate
		{
			Draw(1);
		});
		drawTenButton.onClick.AddListener(delegate
		{
			Draw(MaxDrawCount);
		});
		openGachaRuleInfoPanelButton.onClick.AddListener(delegate
		{
			parentGachaPanel.gachaRUleInfoPanel.SetActive(value: true);
			parentGachaPanel.gachaRUleInfoPanelText.text = PoolState.def.baseRuleInfo;
			parentGachaPanel.gachaContentDetailText.text = CellUI<GachaPoolState>.applicationState.gachaManager.GetGachaContentDescription(PoolState.def);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<GachaPoolState>.applicationState, "主仪式间-探寻规则");
		});
		selectWishButton.onClick.AddListener(delegate
		{
			parentGachaPanel.gachaWishSelectPanel.Setup(PoolState);
			parentGachaPanel.gachaWishSelectPanel.SetActiveAndDirty(active: true);
		});
		changeWishButton.onClick.AddListener(delegate
		{
			parentGachaPanel.gachaWishSelectPanel.Setup(PoolState);
			parentGachaPanel.gachaWishSelectPanel.SetActiveAndDirty(active: true);
		});
		characterIconButton.onClick.AddListener(delegate
		{
			CellUI<GachaPoolState>.fortressState.uiManager.characterInfoBasePanel.ShowUI(PoolState.wishCharacter.GetState(CellUI<GachaPoolState>.applicationState));
		});
		foreach (GachaSetJumpPreviewSetup jumpPreview in jumpPreviewSetups.OrEmptyIfNull())
		{
			jumpPreview.button.onClick.AddListener(delegate
			{
				UIHelper.HideAllInfo(base.transform);
				CellUI<GachaPoolState>.fortressState.uiManager.characterInfoBasePanel.ShowUI(jumpPreview.character.GetState(CellUI<GachaPoolState>.applicationState));
			});
		}
		animator.cullingMode = (AnimatorCullingMode)0;
		currentWishGlobalItemDisplay.SetInfoPanel(CellUI<GachaPoolState>.fortressState.uiManager.universalCommonInfoPanel);
		void AfterGachaDraw(GlobalRewardPackage reward)
		{
			reward.GiveAll(CellUI<GachaPoolState>.applicationState);
			gachaResultDisplay.PlayDrawAnim(reward, delegate
			{
				SetDirty();
				parentGachaPanel.SetDirty();
			});
		}
		void Draw(int count)
		{
			if (PoolState.valid && PoolState.gachaHistoryReady && !drawRequestInFlight && !gachaResultDisplay.gameObject.activeInHierarchy)
			{
				MatterInfo costOfDrawX = PoolState.GetCostOfDrawX(count);
				if (!costOfDrawX.IsPayable(CellUI<GachaPoolState>.applicationState))
				{
					if (PoolState.def.singleDrawCost.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.gachaTicketMatter)
					{
						parentGachaPanel.ShowBuyGachaTicketsPopup(new MatterInfo(costOfDrawX.matterDef, costOfDrawX.count - CellUI<GachaPoolState>.applicationState.matterManager.GachaTicketMatter.count), delegate
						{
							Draw(count);
						}, CellUI<GachaPoolState>.applicationState.gachaManager.ConvertToGachaTicketMatterPurchase);
					}
					else if (PoolState.def.singleDrawCost.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.limitGachaTicketMatter)
					{
						parentGachaPanel.ShowBuyGachaTicketsPopup(new MatterInfo(costOfDrawX.matterDef, costOfDrawX.count - CellUI<GachaPoolState>.applicationState.matterManager.GetMatterCount(ScriptableObjectSingleton<FortressDataManager>.instance.limitGachaTicketMatter)), delegate
						{
							Draw(count);
						}, CellUI<GachaPoolState>.applicationState.gachaManager.ConvertToLimitGachaTicketMatterPurchase);
					}
					else
					{
						CellUI<GachaPoolState>.fortressState.uiManager.quickHintPanel.HintText(CellUI<GachaPoolState>.textData.term_matter_arg0_is_not_enough.FormatWith(costOfDrawX.matterDef.matterName));
					}
				}
				else if (GlobalConfigManager.instance.enableNetwork)
				{
					GeneralGameAsyncOperation<GachaDrawRequest, GachaDrawResult> op = new GeneralGameAsyncOperation<GachaDrawRequest, GachaDrawResult>(new GachaDrawRequest
					{
						GachaPoolId = PoolState.def.FullID,
						GachaTimes = count
					}, CellUI<GachaPoolState>.applicationState.ChillyRoomSDK.Purchase.GachaDrawXAsync);
					drawRequestInFlight = true;
					op.AutoRetry().ManualRetry();
					op.OnComplete(delegate
					{
						GlobalRewardPackage globalRewardPackage = CellUI<GachaPoolState>.applicationState.gachaManager.DrawX(op.OpResult, PoolState, count);
						if (globalRewardPackage != null)
						{
							AfterGachaDraw(globalRewardPackage);
						}
					}).OnOver(delegate
					{
						drawRequestInFlight = false;
						SetDirty();
					}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_gacha_draw);
				}
			}
		}
	}

	public IEnumerator Show()
	{
		ForceUpdate();
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => (UnityEngine.Object)(object)animator == null || !((Behaviour)(object)animator).enabled);
	}

	public IEnumerator Hide()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => (UnityEngine.Object)(object)animator == null || !((Behaviour)(object)animator).enabled);
	}

	protected override void UpdateDisplay()
	{
		poolName.text = PoolState.def.poolDisplayName;
		if (!PoolState.def.endDate.HasValue)
		{
			time.gameObject.SetActive(value: false);
		}
		else
		{
			time.gameObject.SetActive(value: true);
			time.text = PoolState.def.endDate.Value.LocalDateTime.ToString(CellUI<GachaPoolState>.textData.term_end_date);
		}
		drawSingleConsumeMatterCell.Setup(PoolState.def.singleDrawCost);
		drawTenConsumeMatterCell.Setup(PoolState.drawMaxCost);
		drawSingleCountText.text = CellUI<GachaPoolState>.textData.term_draw_Count.FormatWith(1);
		drawCountText.text = CellUI<GachaPoolState>.textData.term_draw_Count.FormatWith(MaxDrawCount);
		drawSingleButton.interactable = PoolState.valid && PoolState.gachaHistoryReady && !drawRequestInFlight && MaxDrawCount > 0;
		drawTenButton.interactable = PoolState.valid && PoolState.gachaHistoryReady && !drawRequestInFlight && MaxDrawCount > 0;
		if (PoolState.def.enableGrandPrizeRule && !PoolState.GrandPrizeDrawed)
		{
			gachaGrandPrizeArea.gameObject.SetActive(value: true);
			if (gachaGrandPrizeIcon != null)
			{
				PlayerCharacterDef grandPrizeCharacter = PoolState.def.grandPrizeCharacter;
				gachaGrandPrizeIcon.sprite = grandPrizeCharacter?.obsession?.icon ?? grandPrizeCharacter?.globalItemIcon ?? grandPrizeCharacter?.headIcon;
			}
			QualityLevel gachaContentQualityLevel = GachaPoolDef.GetGachaContentQualityLevel(PoolState.def.grandPrizeCharacter, GachaPoolDef.GachaContentType.Obsession);
			string text = PoolState.RemainTimesForGrandPrize.ToString();
			string text2 = PoolState.def.grandPrizeCharacter.characterName.BoldText().QualityColorText(gachaContentQualityLevel);
			gachaGrandPrizeCounterText.text = CellUI<GachaPoolState>.textData.term_gacha_grand_prize_remian_times_arg0.FormatWith(text);
			gachaGrandPrizeRewardText.text = CellUI<GachaPoolState>.textData.term_gacha_grand_prize_get_character_arg0.FormatWith(text2);
		}
		else
		{
			gachaGrandPrizeArea.gameObject.SetActive(value: false);
		}
		if (PoolState.def.enableGachaCountLimitRule || (PoolState.def.enableFreeDraw && PoolState.RemainFreeDrawTimesInThisResetPeriod > 0))
		{
			remainGachaTimesTextArea.gameObject.SetActive(value: true);
			if (PoolState.def.enableGachaCountLimitRule)
			{
				remainGachaTimesText.text = CellUI<GachaPoolState>.textData.term_gacha_remain_gacha_times_in_period_arg0_count_arg1_limit_arg2.FormatWith(CellUI<GachaPoolState>.textData.GetThisTimePeriodTerm(PoolState.def.gachaCountLimitResetType), PoolState.RemainDrawTimesInThisLimitResetPeriod, PoolState.def.gachaCountLimit);
			}
			if (PoolState.def.enableFreeDraw)
			{
				remainGachaTimesText.text = CellUI<GachaPoolState>.textData.term_gacha_remain_free_gacha_times_in_period_arg0_count_arg1_limit_arg2.FormatWith(CellUI<GachaPoolState>.textData.GetThisTimePeriodTerm(PoolState.def.freeDrawResetTimeType), PoolState.RemainFreeDrawTimesInThisResetPeriod, PoolState.def.freeDrawCount);
			}
		}
		else
		{
			remainGachaTimesTextArea.gameObject.SetActive(value: false);
		}
		wishArea.gameObject.SetActive(value: false);
		changeWishHint.SetActive(value: false);
		makeWishHint.SetActive(value: false);
		selectWishButton.gameObject.SetActive(value: false);
		currentWishGlobalItemDisplay.gameObject.SetActive(value: false);
		if (PoolState.def.enableGachaWishRule)
		{
			wishArea.gameObject.SetActive(value: true);
			if (PoolState.WishUsedThisWeek)
			{
				wishFinishedHint.SetActive(value: true);
				wishResetTime.text = TextDisplayHelper.GetTimeSpanText(ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod).UtcDateTime - DateTime.UtcNow);
			}
			else if (PoolState.wishCharacter == null)
			{
				selectWishButton.gameObject.SetActive(value: true);
				makeWishHint.SetActive(value: true);
			}
			else
			{
				changeWishHint.SetActive(value: true);
				currentWishGlobalItemDisplay.gameObject.SetActive(value: true);
				currentWishGlobalItemDisplay.Setup(new MatterInfo(PoolState.wishCharacter.obsession, 1));
			}
		}
		int remainFreeDrawTimesInThisResetPeriod = PoolState.RemainFreeDrawTimesInThisResetPeriod;
		drawSingleConsumeMatterCell.gameObject.SetActive(remainFreeDrawTimesInThisResetPeriod <= 0);
		drawTenConsumeMatterCell.gameObject.SetActive(remainFreeDrawTimesInThisResetPeriod <= 0);
		singleDrawFreeHint.SetActive(remainFreeDrawTimesInThisResetPeriod > 0);
		tenDrawFreeHint.SetActive(remainFreeDrawTimesInThisResetPeriod > 0);
		bool enableGachaWishRule = PoolState.def.enableGachaWishRule;
		bool flag = PoolState.def.enableIntroSoftPityRule && PoolState.gachaDrawHistoryInfos.All((GachaHistoryInfo h) => h.qualityLevel != PoolState.def.softPityQualityLevel);
		if (gachaWishHint != null)
		{
			gachaWishHint.SetActive(enableGachaWishRule && !flag);
		}
		if (gachaIntroPityHint != null)
		{
			gachaIntroPityHint.SetActive(flag);
		}
		RefreshJumpPreviewCharacterNames();
	}

	private void RefreshJumpPreviewCharacterNames()
	{
		foreach (GachaSetJumpPreviewSetup item in jumpPreviewSetups.OrEmptyIfNull())
		{
			if (item?.button == null || item.character == null)
			{
				continue;
			}
			TextMeshProUGUI[] componentsInChildren = item.button.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
			foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
			{
				if (textMeshProUGUI != null && !string.IsNullOrWhiteSpace(textMeshProUGUI.text))
				{
					textMeshProUGUI.text = item.character.characterName;
				}
			}
		}
	}
}
