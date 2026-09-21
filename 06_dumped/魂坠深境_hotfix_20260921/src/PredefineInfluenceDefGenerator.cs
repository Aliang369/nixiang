using System.Collections.Generic;
using UnityEngine;

public class PredefineInfluenceDefGenerator : InfluenceDefGenerator
{
	[SerializeField]
	private List<InfluenceDef> influenceDefs;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		return influenceDefs;
	}
}
