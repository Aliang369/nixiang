using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class DifficultySelectionPanel : UIBase
{
	private struct RectLayout(RectTransform rect)
	{
		private Vector2 anchorMin = rect.anchorMin;

		private Vector2 anchorMax = rect.anchorMax;

		private Vector2 anchoredPosition = rect.anchoredPosition;

		private Vector2 sizeDelta = rect.sizeDelta;

		private Vector2 pivot = rect.pivot;

		public void Apply(RectTransform rect)
		{
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = sizeDelta;
			rect.pivot = pivot;
		}
	}

	public class TextConfig
	{
		[LocalizeText("零碎文本")]
		public string difficultyHint = "更高难度的深境伴随着更丰厚的回报，可得到更多的深境经验";

		[LocalizeText("零碎文本")]
		public string get_gem = "，有概率获得最高为 <color=#f5e7c9>{0}</color> 级的宝石";

		[LocalizeText("零碎文本")]
		public string character_progress = "{0}职业进度：{1}";

		[LocalizeText("零碎文本")]
		public string firstFinishRewardHint = "首次通关";

		[LocalizeText("零碎文本")]
		public string classFirstFinishRewardHint = "{0}首通";

		[LocalizeText("零碎文本")]
		public string difficultyLockedDescription = "？？？";
	}

	[SerializeField]
	public CellUIPrefabPoolConfig<DifficultySelectionPanelRewardDisplay, IGlobalItemDisplaySetup> pool;

	[SerializeField]
	private CellUIPrefabPoolConfig<DifficultySelectionPanelRewardDisplay, IGlobalItemDisplaySetup> classRewardPool;

	[SerializeField]
	public TextMeshProUGUI descriptionText;

	[SerializeField]
	public TextMeshProUGUI subTitle;

	[SerializeField]
	public RectTransform difficultyRewardArea;

	[SerializeField]
	public CellUIPrefabPoolConfig<DifficultyInfoCell, DifficultyInfo> difficultyInfoCellPool;

	[SerializeField]
	private RectTransform classRewardParent;

	[SerializeField]
	private TextMeshProUGUI classDifficultyFinishRewardHintText;

	[SerializeField]
	private WheelScrollRect wheelScrollRect;

	[SerializeField]
	private Image classProgressIcon;

	[SerializeField]
	private TextMeshProUGUI classProgressText;

	[NonSerialized]
	private RectLayout rewardHintSingleLayout;

	[NonSerialized]
	public GameConfigPanel parentConfigPanel;

	[NonSerialized]
	public DifficultyInfoCell current;

	public DifficultyInfo finalDifficulty => current?.finalDifficulty;

	public Contract contract => parentConfigPanel.config.contract;

	private TextConfig textConfig => base.textData.difficultySelectionPanelText;

	protected override void Init()
	{
		Init(pool);
		Init(classRewardPool);
		Init(difficultyInfoCellPool);
		parentConfigPanel = GetUIInParents<GameConfigPanel>();
		wheelScrollRect.snappingElementFilter = IsSnappingElementConfirmable;
		WheelScrollRect obj = wheelScrollRect;
		obj.onEndDrag = (Action)Delegate.Combine(obj.onEndDrag, new Action(SelectNearestSnappingCell));
		SetDirty();
	}

	protected override void Dispose()
	{
		WheelScrollRect obj = wheelScrollRect;
		obj.onEndDrag = (Action)Delegate.Remove(obj.onEndDrag, new Action(SelectNearestSnappingCell));
	}

	protected override void UpdateDisplay()
	{
		if (!(finalDifficulty == null))
		{
			descriptionText.text = (IsDifficultyConfirmable(finalDifficulty.difficultyLevel) ? finalDifficulty.GetFullDetailDescription(UIBase.applicationState) : textConfig.difficultyLockedDescription);
			RefreshReward();
			RefreshCharacterClassProgress();
			int num = ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusLevelConfig[finalDifficulty.difficultyLevel].SelectMany((KeyValuePair<EnemyEncounterType, Dictionary<int, int>> dic) => dic.Value.Keys).Max();
			subTitle.text = textConfig.difficultyHint + textConfig.get_gem.FormatWith(num);
			EnsureCurrentDifficultyConfirmable();
		}
		void RefreshReward()
		{
			pool.DespawnAll();
			classRewardPool?.DespawnAll();
			difficultyRewardArea.gameObject.SetActive(value: false);
			if (ShouldDisplaySharedDifficultyRewards(parentConfigPanel.config))
			{
				List<GlobalRewardSetup> accountFirstFinishRewardInDifficulty = contract.contractDef.GetAccountFirstFinishRewardInDifficulty(UIBase.applicationState, finalDifficulty.difficultyLevel);
				List<GlobalRewardSetup> classFirstFinishRewardInDifficulty = contract.contractDef.GetClassFirstFinishRewardInDifficulty(UIBase.applicationState, finalDifficulty.difficultyLevel);
				if (!accountFirstFinishRewardInDifficulty.IsNullOrEmpty() || !classFirstFinishRewardInDifficulty.IsNullOrEmpty())
				{
					difficultyRewardArea.gameObject.SetActive(value: true);
					if (contract.contractDef.contractType == ContractDef.ContractType.BaseGame)
					{
						classDifficultyFinishRewardHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_difficulty_class_arg0_finish_reward.FormatWith(GetCurrentCharacterClassName());
						foreach (GlobalRewardSetup item in accountFirstFinishRewardInDifficulty.OrEmptyIfNull())
						{
							pool.Spawn(item).SetGot(IsAccountDifficultyRewardObtained(finalDifficulty.difficultyLevel), textConfig.firstFinishRewardHint);
						}
						{
							foreach (GlobalRewardSetup item2 in classFirstFinishRewardInDifficulty.OrEmptyIfNull())
							{
								classRewardPool.Spawn(item2).SetGot(IsClassDifficultyRewardObtained(finalDifficulty.difficultyLevel), textConfig.classFirstFinishRewardHint.FormatWith(GetCurrentCharacterClassName()));
							}
							return;
						}
					}
					foreach (GlobalRewardSetup item3 in classFirstFinishRewardInDifficulty.OrEmptyIfNull())
					{
						pool.Spawn(item3).SetGot(IsClassDifficultyRewardObtained(finalDifficulty.difficultyLevel), textConfig.classFirstFinishRewardHint.FormatWith(GetCurrentCharacterClassName()));
					}
				}
			}
		}
	}

	public static bool ShouldDisplaySharedDifficultyRewards(NewGameConfigData config)
	{
		if (config == null)
		{
			return true;
		}
		return !config.IsDailyChallenge;
	}

	public void ApplyCurrentDifficultyToConfig()
	{
		if (!(finalDifficulty == null) && IsDifficultyConfirmable(finalDifficulty.difficultyLevel))
		{
			contract.difficultyLevel = finalDifficulty.difficultyLevel;
		}
	}

	public void SetCurrentDifficulty(DifficultyLevel difficultyLevel)
	{
		DifficultyInfoCell difficultyInfoCell = difficultyInfoCellPool.pool.GetActiveElements().FirstOrDefault((DifficultyInfoCell c) => c.finalDifficulty.difficultyLevel == difficultyLevel);
		if (difficultyInfoCell == null)
		{
			Debug.LogError($"Cannot select difficulty because its cell is missing: {difficultyLevel}", this);
		}
		else
		{
			SetCurrentCell(difficultyInfoCell, snapToCell: true);
		}
	}

	public bool TrySelectCell(DifficultyInfoCell cell)
	{
		if (!IsCellConfirmable(cell))
		{
			return false;
		}
		SetCurrentCell(cell, snapToCell: false);
		return true;
	}

	private string GetCurrentCharacterClassName()
	{
		return parentConfigPanel?.config?.currentCharacter?.attachClass?.characterName ?? UIBase.applicationState.characterManager.currentCharacter?.def?.attachClass?.characterName ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_class;
	}

	private void RefreshCharacterClassProgress()
	{
		CharacterClassDef attachClass = parentConfigPanel.config.currentCharacter.attachClass;
		DifficultyLevel difficultyLevel = UIBase.applicationState.idolManager.GetMainlineDifficulty(attachClass.mClass);
		if (difficultyLevel < DifficultyLevel.Normal)
		{
			difficultyLevel = DifficultyLevel.Normal;
		}
		if (difficultyLevel > DifficultyLevel.NightmareD10)
		{
			difficultyLevel = DifficultyLevel.NightmareD10;
		}
		classProgressIcon.sprite = attachClass.professionIcon;
		classProgressText.text = textConfig.character_progress.FormatWith(attachClass.characterName, UIBase.applicationState.difficultyManager.GetDifficultyInfo(difficultyLevel)?.difficultyName);
	}

	public bool IsDifficultyConfirmable(DifficultyLevel level)
	{
		return (parentConfigPanel?.config)?.CanSelectDifficulty(UIBase.applicationState, level) ?? UIBase.applicationState.difficultyManager.IsDifficultyUnlocked(level);
	}

	private bool IsCellConfirmable(DifficultyInfoCell cell)
	{
		if (cell?.finalDifficulty != null)
		{
			return IsDifficultyConfirmable(cell.finalDifficulty.difficultyLevel);
		}
		return false;
	}

	private bool IsSnappingElementConfirmable(RectTransform element)
	{
		return IsCellConfirmable(element.GetComponent<DifficultyInfoCell>());
	}

	private void SetCurrentCell(DifficultyInfoCell cell, bool snapToCell)
	{
		if (IsCellConfirmable(cell))
		{
			current = cell;
			contract.difficultyLevel = cell.finalDifficulty.difficultyLevel;
			if (snapToCell)
			{
				wheelScrollRect.SnapToElementInstant(current.rectTransform);
			}
			SetDirty();
		}
	}

	private void EnsureCurrentDifficultyConfirmable()
	{
		if (!IsCellConfirmable(current))
		{
			SetCurrentCell(GetHighestConfirmableCell(), snapToCell: true);
		}
	}

	private DifficultyInfoCell GetHighestConfirmableCell()
	{
		return GetHighestConfirmableCell(difficultyInfoCellPool.pool.GetActiveElements());
	}

	private DifficultyInfoCell GetHighestConfirmableCell(IEnumerable<DifficultyInfoCell> cells)
	{
		return (from c in cells.Where(IsCellConfirmable)
			orderby c.finalDifficulty.difficultyLevel descending
			select c).FirstOrDefault();
	}

	private bool IsAccountDifficultyRewardObtained(DifficultyLevel level)
	{
		if (contract.contractDef.contractType == ContractDef.ContractType.BaseGame)
		{
			return UIBase.applicationState.difficultyManager.IsDifficultyFinished(level);
		}
		return UIBase.applicationState.contractManager.GetContractState(contract.contractDef).IsDifficultyFinished(level);
	}

	private bool IsClassDifficultyRewardObtained(DifficultyLevel level)
	{
		if (contract.contractDef.contractType != ContractDef.ContractType.BaseGame)
		{
			return UIBase.applicationState.contractManager.GetContractState(contract.contractDef).IsDifficultyFinished(level);
		}
		CharacterClassDef characterClassDef = parentConfigPanel?.config?.currentCharacter?.attachClass ?? UIBase.applicationState.characterManager.currentCharacter?.def?.attachClass;
		if (characterClassDef != null)
		{
			return level <= UIBase.applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass);
		}
		return false;
	}

	public void Setup()
	{
		base.gameObject.SetActive(value: true);
		wheelScrollRect.ResetContentPositionForRebuild();
		difficultyInfoCellPool.DespawnAll();
		bool isDailyChallenge = parentConfigPanel.config.IsDailyChallenge;
		List<DifficultyInfo> list = UIBase.applicationState.assetCatalog.difficultyCatalog.ValidAssets.Where((DifficultyInfo d) => d.difficultyLevel >= DifficultyLevel.Normal && ((!isDailyChallenge) ? contract.contractDef.CheckDifficultyLevelAble(d) : d.contractTypes.Contains(ContractDef.ContractType.BaseGame))).ToList();
		list.Sort((DifficultyInfo a, DifficultyInfo b) => a.difficultyLevel.CompareTo(b.difficultyLevel));
		difficultyInfoCellPool.pool.DestroyInActive();
		List<DifficultyInfoCell> list2 = (from d in list
			select difficultyInfoCellPool.Spawn(d) into c
			where c != null
			select c).ToList();
		for (int num = 0; num < list2.Count; num++)
		{
			DifficultyInfoCell difficultyInfoCell = list2[num];
			difficultyInfoCell.transform.SetSiblingIndex(num + 1);
			difficultyInfoCell.rectTransform.anchoredPosition = Vector2.zero;
			difficultyInfoCell.rectTransform.localScale = Vector3.one;
		}
		current = list2.FirstOrDefault((DifficultyInfoCell c) => c.finalDifficulty.difficultyLevel == contract.difficultyLevel && IsCellConfirmable(c)) ?? GetHighestConfirmableCell(list2);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-准备-难度选择");
		SetDirty();
	}

	public void OnShow()
	{
		if (current != null)
		{
			contract.difficultyLevel = current.finalDifficulty.difficultyLevel;
			ForceUpdateDifficultyCells();
			wheelScrollRect.ResetContentPositionForRebuild();
			wheelScrollRect.RebuildContentLayoutForSnapping();
			base.rectTransform.RefreshLayout();
			wheelScrollRect.SnapToElementInstant(current.rectTransform);
		}
	}

	private void ForceUpdateDifficultyCells()
	{
		foreach (DifficultyInfoCell activeElement in difficultyInfoCellPool.pool.GetActiveElements())
		{
			activeElement._InternalUpdate(hierarchy: false, forceUpdate: true);
		}
	}

	private void SelectNearestSnappingCell()
	{
		wheelScrollRect.StopMovement();
		RectTransform nearestElementToSnappingTarget = wheelScrollRect.GetNearestElementToSnappingTarget();
		if (nearestElementToSnappingTarget == null)
		{
			return;
		}
		DifficultyInfoCell component = nearestElementToSnappingTarget.GetComponent<DifficultyInfoCell>();
		if (IsCellConfirmable(component))
		{
			if (component != current)
			{
				component.Select();
			}
		}
		else
		{
			EnsureCurrentDifficultyConfirmable();
		}
	}
}
