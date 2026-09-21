public class CheckYinYangConvert : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return null;
		}
		return ((YinYangFeatureAbility)eve.CasterItem.GetDef().abilities.Find((Ability ability) => ability is YinYangFeatureAbility))?.CheckYinYangConvert(eve);
	}
}
