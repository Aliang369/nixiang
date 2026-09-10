using System.Collections.Generic;

public class ChangeBftMaxHealth : Effect
{
	public enum Op
	{
		Change = 0,
		Set = 2
	}

	public bool baseAdd;

	public ValueGenerator deltaGenerator;

	public Op op;

	public GameEvent.EventBFTType target;

	public bool hideNFX;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int num = ((deltaGenerator != null) ? deltaGenerator.GenerateValue(eve).Value : 0);
		BattleFieldTransform eventBFT = eve.GetEventBFT(target);
		int delta = num;
		Op op = this.op;
		if (op != Op.Change && op == Op.Set)
		{
			delta = num - (int)eventBFT.health;
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new ChangeBftMaxHealthAction(eventBFT, delta, baseAdd, hideNFX));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
