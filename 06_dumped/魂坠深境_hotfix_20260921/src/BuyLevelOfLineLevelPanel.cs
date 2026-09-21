using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;

public class BuyLevelOfLineLevelPanel : UIBase
{
	public RangeSliderWithAdjustButton addLevelSlider;

	public GameButton backButton;

	public GameButton confirmButton;

	public TextMeshProUGUI buyTargetLevelHintText;

	public TextMeshProUGUI targetLevelText;

	public TextMeshProUGUI notEnoughCostHint;

	public GlobalItemDisplay extraRewardItemDisplayPrefab;

	public RectTransform extraRewardItemParent;

	public MatterCellGroup upgradeCostMatterCellGroup;

	private LevelLine levelLine;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> extraRewardItemDisplayPool;

	private SeasonDetailPanel seasonDetailPanelParent;

	public MatterCell matterCell;

	private int DeltaAddLevel => addLevelSlider.CurrentValue;

	protected override void Init()
	{
		InitUI(upgradeCostMatterCellGroup);
		InitUI(matterCell);
		seasonDetailPanelParent = GetUIInParents<SeasonDetailPanel>();
		extraRewardItemDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(extraRewardItemDisplayPrefab, extraRewardItemParent);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		addLevelSlider.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
	}

	public void Setup(LevelLine levelLine)
	{
		this.levelLine = levelLine;
		addLevelSlider.CurrentValue = 1;
		SetActiveAndDirty(active: true);
	}

	protected override void UpdateDisplay()
	{
		if (levelLine == null)
		{
			return;
		}
		int num = levelLine.MaxLevelIndex - levelLine.currentLevelIndex;
		if (num <= 0)
		{
			confirmButton.interactable = false;
			notEnoughCostHint.gameObject.SetActive(value: true);
			notEnoughCostHint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_max_level;
			extraRewardItemDisplayPool.Sync(new List<IGlobalItemDisplaySetup>());
			upgradeCostMatterCellGroup.Setup(new List<MatterInfo>());
			buyTargetLevelHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_max_level;
			targetLevelText.text = levelLine.currentLevelIndex.ToString();
			return;
		}
		addLevelSlider.MaxValue = num;
		addLevelSlider.MinValue = 1;
		addLevelSlider.CurrentValue = Mathf.Clamp(addLevelSlider.CurrentValue, 1, num);
		matterCell.Setup(UIBase.applicationState.matterManager.CreditMatter);
		int num2 = levelLine.currentLevelIndex + DeltaAddLevel;
		List<GlobalRewardSetup> unGotRewardInLevelRange = levelLine.GetUnGotRewardInLevelRange(UIBase.applicationState, levelLine.currentLevelIndex, num2);
		List<MatterInfo> list = levelLine.levelLineDef.manualBuyLevelCost.Select((MatterInfo m) => new MatterInfo(m.matterDef, m.count * DeltaAddLevel)).ToList();
		bool interactable = true;
		notEnoughCostHint.gameObject.SetActive(value: false);
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			MatterInfo matterInfo = list[num3];
			if (!UIBase.applicationState.matterManager.HasEnoughMatter(matterInfo))
			{
				interactable = false;
				notEnoughCostHint.gameObject.SetActive(value: true);
				notEnoughCostHint.text = base.textData.term_not_enough.FormatWith(matterInfo.matterDef.matterName);
				break;
			}
		}
		confirmButton.interactable = interactable;
		extraRewardItemDisplayPool.Sync(((IEnumerable<GlobalRewardSetup>)unGotRewardInLevelRange).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
		upgradeCostMatterCellGroup.Setup(list);
		buyTargetLevelHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_buy_level_to_get_arg0_level_and_get_sub_rewards.FormatWith(num2);
		targetLevelText.text = num2.ToString();
	}

	private void _OnConfirmButtonClicked()
	{
		if (levelLine.MaxLevelIndex - levelLine.currentLevelIndex <= 0)
		{
			confirmButton.interactable = false;
			SetDirty();
		}
		else if (GlobalConfigManager.instance.enableNetwork)
		{
			ManualAddLevelLineLevelRequest request = new ManualAddLevelLineLevelRequest(levelLine, DeltaAddLevel);
			GeneralGameAsyncOperation<ManualAddLevelLineLevelRequest, ChangeExpResult> op = new GeneralGameAsyncOperation<ManualAddLevelLineLevelRequest, ChangeExpResult>(request, UIBase.applicationState.ChillyRoomSDK.Season.ManualAddLevelLineLevelAsync);
			op.OnComplete(delegate
			{
				LevelLine.ChangeExpResult changeResult = op.OpResult.ToClient(new NetworkConvertContext(UIBase.applicationState));
				AfterBuy(changeResult);
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_level_line);
		}
		void AfterBuy(LevelLine.ChangeExpResult changeResult)
		{
			changeResult.Excute(UIBase.applicationState);
			List<MatterInfo> matterInfos = levelLine.levelLineDef.manualBuyLevelCost.Select((MatterInfo m) => new MatterInfo(m.matterDef, m.count * DeltaAddLevel)).ToList();
			UIBase.applicationState.matterManager.ConsumeMatter(matterInfos, "购买等级");
			SetActiveAndDirty(active: false);
			seasonDetailPanelParent.SetDirty();
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_buy_line_level.FormatWith(levelLine.currentLevelIndex));
			ThinkingAnalyticsExtends.TrackFortressManualAddLevelLineLevel(UIBase.applicationState, levelLine, changeResult.initLeveLIndex, changeResult.finalLevelIndex - changeResult.initLeveLIndex);
			SetDirty();
		}
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirty(active: false);
	}
}
