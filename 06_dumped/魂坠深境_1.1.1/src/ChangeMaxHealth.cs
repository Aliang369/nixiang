using System.Collections.Generic;

public class ChangeMaxHealth : Effect
{
	public enum Op
	{
		Add,
		Sub,
		Set
	}

	public bool baseAdd = true;

	public ValueGenerator deltaGenerator;

	public Op op;

	public GameEvent.EventBFTType target;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(target);
		int value = ((deltaGenerator != null) ? deltaGenerator.GenerateValue(eve).Value : 0);
		int deltaValue = GetDeltaValue(value, eventBFT);
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new ChangeBftMaxHealthAction(eventBFT, deltaValue, baseAdd));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	private int GetDeltaValue(int value, BattleFieldTransform bft)
	{
		return op switch
		{
			Op.Sub => -value, 
			Op.Set => value - (int)bft.maxHealth, 
			_ => value, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
