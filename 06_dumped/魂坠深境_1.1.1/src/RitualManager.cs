using System.Collections.Generic;

public class RitualManager : InGameManager
{
	[SaveField(null, null)]
	public List<Pray> currentPrays;

	[SaveField(null, null)]
	public bool inited;

	[SaveField(null, null)]
	public int prayLimit;

	public override void Init(GameStateInfo gameState)
	{
		if (currentPrays == null)
		{
			currentPrays = new List<Pray>();
		}
		if (!inited)
		{
			inited = true;
			prayLimit = 3;
		}
	}
}
