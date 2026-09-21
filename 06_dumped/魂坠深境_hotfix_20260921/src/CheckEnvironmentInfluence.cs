public class CheckEnvironmentInfluence : CheckNode
{
	public EnvironmentInfluenceDef def;

	public GameEvent.EveObjType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.GetEveObj(type) is EnvironmentInfluence environmentInfluence && (def == null || environmentInfluence.def == def))
		{
			return true;
		}
		return false;
	}
}
