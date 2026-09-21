using System.Collections.Generic;

public class CheckSelect : Effect
{
	public enum CheckType
	{
		First,
		All
	}

	public class CheckSelectItme
	{
		public CheckNode checker;

		public Effect effect;
	}

	public List<CheckSelectItme> checkSelectItmes = new List<CheckSelectItme>();

	public CheckType checkType;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (checkSelectItmes == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (CheckSelectItme checkSelectItme in checkSelectItmes)
		{
			if (checkSelectItme.checker != null && checkSelectItme.checker.Check(eve) == true)
			{
				checkSelectItme.effect?.Cast(eve);
				if (checkType == CheckType.First)
				{
					break;
				}
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (checkSelectItmes == null)
		{
			return list;
		}
		foreach (CheckSelectItme checkSelectItme in checkSelectItmes)
		{
			list.Add(checkSelectItme.checker);
			list.Add(checkSelectItme.effect);
		}
		return list;
	}
}
