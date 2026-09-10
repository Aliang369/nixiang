using System.Collections.Generic;

public class ChangeBftHealth : Effect
{
	public enum Op
	{
		Add,
		Sub,
		Set
	}

	public ValueGenerator deltaGenerator;

	public Op op;

	public GameEvent.EventBFTType target;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFX3DConfig transformEffect3D;

	public bool realHealth;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int num = ((deltaGenerator != null) ? deltaGenerator.GenerateValue(eve).Value : 0);
		BattleFieldTransform eventBFT = eve.GetEventBFT(target);
		int num2 = num;
		switch (op)
		{
		case Op.Sub:
			num2 = -num;
			break;
		case Op.Set:
			num2 = num - (int)eventBFT.health;
			break;
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new ChangeBftHealthAction(eventBFT, num2, eve.CasterItem, null, realHealth, num2 > 0));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
