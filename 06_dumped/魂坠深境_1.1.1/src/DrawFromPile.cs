using System.Collections.Generic;

public class DrawFromPile : Effect
{
	public ValueGenerator drawCountGen;

	public ValueGenerator insertIndexGen;

	public GameEvent.EventBFTType targetBFT;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFXConfig transformEffect;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(targetBFT);
		int num = ((drawCountGen == null) ? 1 : drawCountGen.GenerateValue(eve).Value);
		if (eventBFT != null && eventBFT.isPlayer)
		{
			transformEffect?.Play(eve, eve.CasterPivot.GetPivot(eve.gameState), eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.DrawPileButton));
		}
		else if (eventBFT != null && eventBFT.isEnemy)
		{
			transformEffect?.Play(eve, eve.CasterPivot.GetPivot(eve.gameState), eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.EnemyPivot));
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		for (int i = 0; i < num; i++)
		{
			Effect.EnqueueAction(eve, new DrawFromPileAction(eventBFT, eve.CasterItem, (insertIndexGen != null) ? insertIndexGen.GenerateValue(eve).Value : 0));
		}
		if (eventBFT != null && eventBFT.isPlayer)
		{
			hitEffect?.Play(eve.gameState, eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.DrawPileButton));
		}
		else if (eventBFT != null && eventBFT.isEnemy)
		{
			hitEffect?.Play(eve.gameState, eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.EnemyPivot));
		}
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int? value = ((drawCountGen == null) ? new int?(1) : drawCountGen.GenerateValue(eve));
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(value)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawCountGen, insertIndexGen };
	}
}
