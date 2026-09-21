using System.Collections.Generic;

public class AddInitInfluence : GameConfigEffect
{
	public InfluenceDef influence;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initInfluence.Add(new Influence(influence));
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refInfluences = new NodeDisplayInfo.Element<List<Influence>>(new List<Influence>
			{
				new Influence(influence)
			})
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { influence };
	}
}
