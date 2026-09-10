using System.Collections.Generic;

public class SetHealth : Effect
{
	public GameEvent.EventBFTType target;

	public ValueGenerator valueGenerator;

	public TransformNFX3DConfig transformEffect3D;

	public HitNFX3DConfig hitEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(target);
		int? num = valueGenerator?.GenerateValue(eve);
		if (eventBFT == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new SetBftHealthAction(num.Value, eve.CasterItem, eventBFT));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
