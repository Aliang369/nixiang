using System.Collections.Generic;
using GameRandom;

public class RandomNode : Effect
{
	public struct RandomSelection
	{
		public Effect castNode;

		public ValueGenerator weightGenerator;
	}

	public List<RandomSelection> selections;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (selections == null || selections.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		selections.SelectRandomWeight((RandomSelection s) => (s.weightGenerator != null) ? s.weightGenerator.GenerateValue(eve).Value : 0, eve.gameState.randomManager.NextDouble).castNode?.Cast(eve);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (selections != null)
		{
			foreach (RandomSelection selection in selections)
			{
				list.Add(selection.castNode);
				list.Add(selection.weightGenerator);
			}
		}
		return list;
	}
}
