using UnityEngine;

public class CheckDamageKill : CheckNode
{
	[SerializeField]
	private GameEvent.EventBFTType eventBFT;

	public override bool? Check(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.AfterDamage && eve.type != GameEvent.Type.AfterEnemyDefeated)
		{
			return false;
		}
		BattleFieldTransform battleFieldTransform = eve.GetEventBFT(eventBFT);
		if (battleFieldTransform == null)
		{
			return false;
		}
		return battleFieldTransform.GetTotalHealth() <= 0 && eve.damage != null && eve.damage.subCounter > 0;
	}
}
