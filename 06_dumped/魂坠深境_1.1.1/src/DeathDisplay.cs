using UnityEngine;
using UnityEngine.UI;

public class DeathDisplay : UIBase
{
	[SerializeField]
	private Text text;

	protected override void Init()
	{
		UIBase.gameState.actionManager.actionFrameFinishEvent += CheckDeath;
	}

	protected override void Dispose()
	{
		UIBase.gameState.actionManager.actionFrameFinishEvent -= CheckDeath;
	}

	private void CheckDeath()
	{
		if ((int)UIBase.gameState.playerState.bft.health <= 0)
		{
			text.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_you_are_dead;
		}
		else
		{
			text.text = "";
		}
	}
}
