using System.Collections.Generic;
using UnityEngine;

public class TriggerCustomWorldEvent : Effect
{
	public List<NodeBasedWorldEvent.WorldEventChoiceSetup> choiceSetups;

	[LocalizeText("自定义事件名称")]
	public string worldEventName;

	[LocalizeText("自定义事件描述")]
	public string worldEventDescription;

	public bool overlayWorldEvent;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		EncounterSetupInfo setupInfo = eve.gameState.currentEncounter.setupInfo;
		if (setupInfo == null)
		{
			return NodeCastResult.Failure;
		}
		WorldEvent originWorldEvent = setupInfo.fixedEventTemplate;
		if (eve.gameState.battleState.isInBattle)
		{
			Effect.EnqueueAction(eve, new StopBattleAction(FinishBattleEncounterAction.FinishType.Break));
		}
		NodeBasedWorldEvent nodeBasedWorldEvent = ScriptableObject.CreateInstance<NodeBasedWorldEvent>();
		nodeBasedWorldEvent.EventName = worldEventName;
		nodeBasedWorldEvent.EventDescription = worldEventDescription;
		nodeBasedWorldEvent.setups = choiceSetups;
		setupInfo.fixedEventTemplate = nodeBasedWorldEvent;
		Effect.EnqueueAction(eve, new TransitReloadRoomAction(setupInfo));
		if (overlayWorldEvent)
		{
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				eve.gameState.currentEncounter.afterEncounterEnd = delegate
				{
					setupInfo.fixedEventTemplate = originWorldEvent;
					Effect.EnqueueAction(eve, new TransitReloadRoomAction(setupInfo));
				};
			}));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (NodeBasedWorldEvent.WorldEventChoiceSetup item in choiceSetups.OrEmptyIfNull())
		{
			list.Add(item);
		}
		return list;
	}
}
