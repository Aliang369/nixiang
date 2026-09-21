using System.Collections.Generic;

public class EveObjRuneGenerator : RuneGenerator
{
	public GameEvent.EveObjType type;

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		Rune eveObj = eve.GetEveObj<Rune>(type);
		if (eveObj == null)
		{
			return null;
		}
		return new List<Rune> { eveObj };
	}
}
