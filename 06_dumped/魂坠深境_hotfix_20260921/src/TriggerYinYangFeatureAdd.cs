public class TriggerYinYangFeatureAdd : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		Item casterItem = eve.CasterItem;
		if (casterItem == null)
		{
			return NodeCastResult.Failure;
		}
		YinYangFeatureAbility yinYangDisplayModule = YinYangFeatureAbility.GetYinYangDisplayModule(casterItem);
		if (yinYangDisplayModule == null)
		{
			return NodeCastResult.Failure;
		}
		yinYangDisplayModule.TriggerAdd(eve);
		return NodeCastResult.Success;
	}
}
