using System;
using System.Collections.Generic;

public class EveObjStanceDefSelector : StanceDefSelector
{
	public GameEvent.EveObjType eveObjType;

	public override List<StanceDef> GenerateStanceDefs(GameEvent eve)
	{
		throw new NotImplementedException();
	}

	public override StanceDef GenerateSingleStanceDef(GameEvent eve)
	{
		return eve.GetEveObj<Stance>(eveObjType)?.def;
	}
}
