using System.Collections.Generic;

public class Pray : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable
{
	[SaveField(null, null)]
	public int index;

	[SaveField(null, null)]
	public BattleFieldTransform owner;

	[SaveField(null, null)]
	public IEffectObjectDisplay praySource;

	public PrayAbility PrayAbilityNode => praySource.GetChildren().IndexOrNull(index) as PrayAbility;

	public BattleFieldTransform OwnerBft => owner;

	private Pray()
	{
	}

	public Pray(IEffectObjectDisplay source, int index)
	{
		praySource = source;
		this.index = index;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return praySource.GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (GameEvent.Type item in PrayAbilityNode.GetAllSubscribeGameEventType())
		{
			yield return item;
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return PrayAbilityNode.CheckTriggerGameEventAble(eve);
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		PrayAbilityNode.TriggerGameEvent(eve);
	}

	public string GetDescription(GameStateInfo gs, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatEffectObjectDescription(this, PrayAbilityNode.prayDescription, gs, out generator);
	}

	public void TriggerEffect(GameStateInfo gs)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.PrayEffect, GameEvent.TriggerType.Real);
		gameEvent.gameState = gs;
		gameEvent.Caster = this;
		PrayAbilityNode.prayTriggerEffect.Cast(gameEvent);
	}
}
