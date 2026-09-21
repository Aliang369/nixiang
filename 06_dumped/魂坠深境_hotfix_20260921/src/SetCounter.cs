using System.Collections.Generic;

public class SetCounter : Effect
{
	public ItemTargetSelector target;

	public ValueGenerator counterGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	public bool countsAsChapterReset;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = target?.GetTargetItems(eve);
		int? num = counterGenerator?.GenerateValue(eve);
		if (list == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			if (item != null)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				Effect.EnqueueAction(eve, new SetCounterAction(num.Value, item, eve.CasterItem, countsAsChapterReset));
				hitEffect?.Play(eve, item);
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(counterGenerator?.GenerateValue(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, counterGenerator };
	}
}
