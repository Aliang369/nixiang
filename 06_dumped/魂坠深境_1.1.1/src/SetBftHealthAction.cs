using UnityEngine;

public class SetBftHealthAction : GameAction
{
	public Item source;

	public BattleFieldTransform targetBFT;

	public int targetValue;

	public SetBftHealthAction(int targetValue, Item source, BattleFieldTransform targetBft)
	{
		this.targetValue = targetValue;
		this.source = source;
		targetBFT = targetBft;
	}

	protected override void Update()
	{
		int b = targetBFT.maxHealth;
		int num = Mathf.Min(targetValue, b);
		targetBFT.health = num;
		if (targetBFT.isPlayer && num <= 0)
		{
			EnqueueAction(delegate
			{
				gameState.playerState.CheckDeath();
			});
		}
		FinishAction();
	}
}
