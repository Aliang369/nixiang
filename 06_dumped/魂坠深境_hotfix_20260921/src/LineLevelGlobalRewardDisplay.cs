using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineLevelGlobalRewardDisplay : CellUI<(GlobalRewardSetup rewardSetup, LevelLine line, LevelLineDef.LineLevel level)>, IDisplayModulesController
{
	public GameObject getEffect;

	public GameObject bigRewardHint;

	public GameObject middleRewardHint;

	public GameObject seasonPassGetHint;

	public GameObject seasonPassLockHint;

	public TextMeshProUGUI seasonPassGetButtonText;

	public Image GotHintIcon;

	public GlobalItemDisplay globalRewardSetupCellDisplay;

	private GlobalRewardSetup rewardSetup;

	private LevelLine line;

	private LevelLineDef.LineLevel level;

	public GameObject manualGetHint;

	public GameButton manualGetButton;

	private bool IsUngotReward => line.GetAllUnGotRewardInLevelRange(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState).Contains(base.setupData.rewardSetup);

	protected override void Init()
	{
		InitUI(globalRewardSetupCellDisplay);
		if (CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.fortressState?.uiManager?.universalCommonInfoPanel != null)
		{
			this.SetInfoPanel(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.fortressState.uiManager.universalCommonInfoPanel);
		}
		manualGetButton.onClick.AddListener(delegate
		{
			if (CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.fortressState != null && line.WithAnyUngotReward(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState))
			{
				line.GenerateAndGiveAllUnGotLevelLineReward(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState, delegate(List<LevelLine.ChangeExpResult.LevelRewardInfo> levelups)
				{
					List<GlobalRewardInfo> rewards = levelups.SelectMany((LevelLine.ChangeExpResult.LevelRewardInfo r) => new GlobalRewardInfo[2] { r.baseReward, r.appendReward }).WhereNotNull().ToList();
					GlobalRewardPackage rewardPack = new GlobalRewardPackage(rewards);
					CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState.StartCoroutine(() => CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(rewardPack));
					ThinkingAnalyticsExtends.TrackFortressManualGetLevelLineReward(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState, line, levelups.Select((LevelLine.ChangeExpResult.LevelRewardInfo u) => line.GetLineLevel(u.levelIndex)).ToList());
				});
			}
		});
	}

	protected override void OnSetup((GlobalRewardSetup rewardSetup, LevelLine line, LevelLineDef.LineLevel level) info)
	{
		(rewardSetup, line, level) = info;
		getEffect.gameObject.SetActive(value: false);
		int lineLevelIndex = line.GetLineLevelIndex(level);
		bool flag = lineLevelIndex < line.currentLevelIndex;
		LevelLineDef.LineLevel.LevelRewardType levelRewardType = ((rewardSetup == level.reward) ? level.baseRewardType : level.appendRewardType);
		bool flag2 = (rewardSetup == level.reward && line.baseRewardGotLevels.Contains(lineLevelIndex)) || (rewardSetup == level.appendReward && line.appendRewardGotLevels.Contains(lineLevelIndex));
		bigRewardHint.gameObject.SetActive(levelRewardType == LevelLineDef.LineLevel.LevelRewardType.Big);
		middleRewardHint.gameObject.SetActive(levelRewardType == LevelLineDef.LineLevel.LevelRewardType.Middle);
		GotHintIcon.gameObject.SetActive(flag2);
		seasonPassGetHint.gameObject.SetActive(rewardSetup == level.appendReward);
		seasonPassLockHint.gameObject.SetActive(!CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState.seasonManager.currentSeason.seasonPassUnlocked);
		if (!flag2 && flag)
		{
			if (rewardSetup == level.appendReward && !CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState.seasonManager.currentSeason.seasonPassUnlocked)
			{
				seasonPassGetButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_need_season_pass;
			}
			else
			{
				seasonPassGetButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_wait_for_got;
			}
		}
		globalRewardSetupCellDisplay.Setup(rewardSetup);
		manualGetHint.gameObject.SetActive(IsUngotReward);
		manualGetButton.gameObject.SetActive(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.fortressState != null && line.WithAnyUngotReward(CellUI<(GlobalRewardSetup, LevelLine, LevelLineDef.LineLevel)>.applicationState));
	}
}
