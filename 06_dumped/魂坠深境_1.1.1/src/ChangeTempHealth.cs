using System.Collections.Generic;

public class ChangeTempHealth : Effect
{
	public GameEvent.EventBFTType bft;

	public ValueGenerator deltaGenerator;

	public TransformNFX3DConfig transformEffect3D;

	public HitNFX3DConfig hitEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int? num = deltaGenerator?.GenerateValue(eve);
		BattleFieldTransform eventBFT = eve.GetEventBFT(bft);
		if (eventBFT == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new ChangeTempHealthAction(num.Value, eventBFT));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
