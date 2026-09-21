using System.Collections;
using UnityEngine;

public class PVPEndPointWorldEvent : WorldEvent
{
	public WorldEventChoiceText choiceText;

	protected override void Init()
	{
		base.Init();
		WorldEventChoice worldEventChoice = new WorldEventChoice();
		worldEventChoice.descriptionText = choiceText.choiceDescription;
		worldEventChoice.buttonText = choiceText.choiceName;
		worldEventChoice.executeAction = delegate
		{
			EnqueueAction(new EnumeratorAction(EndRewardCor));
			EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Win));
		};
		currentDisplayInfo.AddChoice(worldEventChoice);
	}

	private IEnumerator EndRewardCor()
	{
		GlobalRewardPackage reward = new GlobalRewardPackage(new GlobalRewardInfo
		{
			matter = new MatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter, ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusOnChallengeGameWin)
		});
		gameState.UIManager.globalRewardInfoDisplay.Setup(reward);
		yield return new WaitUntil(() => !gameState.UIManager.globalRewardInfoDisplay.gameObject.activeInHierarchy);
	}
}
