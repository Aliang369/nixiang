using System;
using System.Collections.Generic;
using ContainerOfSoul.TargetRequirement;

public class ActivatePotential : GameConfigEffect
{
	public CheckNode triggerAbleCheckNode;

	public CastPrepare targetRequirement;

	public Effect effectNode;

	public bool TriggerAble(TalentDef talent, GameStateInfo gameState)
	{
		GameEvent eve = GenerateGameEvent(talent, gameState);
		if (triggerAbleCheckNode != null)
		{
			return triggerAbleCheckNode.Check(eve) == true;
		}
		return true;
	}

	public void Trigger(TalentDef talent, GameStateInfo gameState, Action onConfirm)
	{
		GameEvent eve = GenerateGameEvent(talent, gameState);
		if (targetRequirement != null)
		{
			targetRequirement.TriggerPrepare(eve, delegate
			{
				effectNode?.Cast(eve);
				gameState.playerState.activatedPotentialTalents.Add(talent);
				TriggerActivatePotential();
				gameState.recordManager.TrackJournalRecord(new ActivatePotentialTalentRecord(talent));
				onConfirm?.Invoke();
			});
		}
		else
		{
			effectNode?.Cast(eve);
			gameState.playerState.activatedPotentialTalents.Add(talent);
			TriggerActivatePotential();
			gameState.recordManager.TrackJournalRecord(new ActivatePotentialTalentRecord(talent));
			onConfirm?.Invoke();
		}
		void TriggerActivatePotential()
		{
			gameState?.applicationState?.eventManager?.gameEvent.TriggerEvent(eve.type, eve);
		}
	}

	private GameEvent GenerateGameEvent(TalentDef talent, GameStateInfo gameState)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.ActivatePotential, GameEvent.TriggerType.Real);
		gameEvent.SetCaster(talent);
		gameEvent.gameState = gameState;
		return gameEvent;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerAbleCheckNode, targetRequirement, effectNode };
	}
}
