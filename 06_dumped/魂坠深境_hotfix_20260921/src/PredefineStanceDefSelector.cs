using System;
using System.Collections.Generic;

public class PredefineStanceDefSelector : StanceDefSelector
{
	public StanceDef stanceDef;

	public override List<StanceDef> GenerateStanceDefs(GameEvent eve)
	{
		throw new NotImplementedException();
	}

	public override StanceDef GenerateSingleStanceDef(GameEvent eve)
	{
		return stanceDef;
	}
}
