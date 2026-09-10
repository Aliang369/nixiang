using System.Collections.Generic;
using System.Linq;

public class EnemyInfluenceDisplayArea : InfluenceDisplayArea
{
	protected override BattleFieldTransform BftOwner => UIBase.gameState.battleState?.enemy?.bft;

	protected override IList<Influence> DisplayInfluences
	{
		get
		{
			IList<Influence> displayInfluences = base.DisplayInfluences;
			InfluenceDef disorderDef = UIBase.gameState?.dungeonMapManager?.currentLocation?.disorderCriticalInfluence;
			if (displayInfluences == null || disorderDef == null)
			{
				return displayInfluences;
			}
			return displayInfluences.Where((Influence influence) => influence.def != disorderDef).ToList();
		}
	}
}
