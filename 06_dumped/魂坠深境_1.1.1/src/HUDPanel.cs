using UnityEngine;

public class HUDPanel : UIBase
{
	[SerializeField]
	private DeathDisplay deathDisplay;

	[SerializeField]
	private ChallengeHintPanel challengeHintPanel;

	[SerializeField]
	public CoinCrystalHintPanel coinCrystalHintPanel;

	protected override void Init()
	{
		InitUI(deathDisplay);
		InitUI(challengeHintPanel);
		InitUI(coinCrystalHintPanel);
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinishEvent;
	}

	protected override void Dispose()
	{
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinishEvent;
	}

	private void _OnActionFrameFinishEvent()
	{
		SetDirty();
	}
}
