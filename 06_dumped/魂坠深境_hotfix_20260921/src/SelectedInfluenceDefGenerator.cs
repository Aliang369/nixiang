using System.Collections.Generic;
using System.Linq;

public class SelectedInfluenceDefGenerator : InfluenceDefGenerator
{
	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		return (from inf in eve.GetSelectedValues<Influence>()
			select inf.def).ToList();
	}
}
