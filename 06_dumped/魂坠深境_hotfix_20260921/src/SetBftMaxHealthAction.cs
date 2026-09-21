using UnityEngine;

public class SetBftMaxHealthAction : GameAction
{
	public Item source;

	public BattleFieldTransform targetBFT;

	public int targetValue;

	public SetBftMaxHealthAction(int targetValue, Item source, BattleFieldTransform targetBft)
	{
		this.targetValue = targetValue;
		this.source = source;
		targetBFT = targetBft;
	}

	protected override void Update()
	{
		targetBFT.maxHealth = targetValue;
		int num = Mathf.Min(targetBFT.health, targetValue);
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
