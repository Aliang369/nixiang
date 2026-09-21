using UnityEngine;

public class SetEnvironmentInfluenceAction : ExecuteAction
{
	public EnvironmentInfluenceDef target;

	public bool hint;

	private const float particleDestroyDelay = 2f;

	public SetEnvironmentInfluenceAction(EnvironmentInfluenceDef target, bool hint)
	{
		this.target = target;
		this.hint = hint;
	}

	protected override void Update()
	{
		if (gameState.currentEncounter == null)
		{
			return;
		}
		if (gameState.currentEncounter.environmentInfluence != null)
		{
			if (hint)
			{
				EnvironmentInfluence oldInf = gameState.currentEncounter.environmentInfluence;
				EnqueueAction(new EnumeratorAction(() => gameState.UIManager.environmentInfluenceHintDisplay.HideInfluenceCor(oldInf)));
			}
			StopEnvironmentInfluence(gameState.currentEncounter.environmentInfluence);
			gameState.currentEncounter.environmentInfluence = null;
		}
		if (target != null)
		{
			gameState.currentEncounter.environmentInfluence = new EnvironmentInfluence(target);
			if (hint)
			{
				EnvironmentInfluence newInf = gameState.currentEncounter.environmentInfluence;
				EnqueueAction(new EnumeratorAction(() => gameState.UIManager.environmentInfluenceHintDisplay.ShowInfluenceCor(newInf)));
			}
			if (target.spawnEffectPrefab != null)
			{
				gameState.currentEncounter.environmentInfluence.spawnEffectInstance = Object.Instantiate(target.spawnEffectPrefab, gameState.currentEncounter.room.enemySpawnPivot);
			}
		}
		gameState.applicationState.renderingManager.ApplyEnvironmentRenderOverride(gameState.currentEncounter.room, gameState.currentEncounter.environmentInfluence?.def, target != null);
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerSetEnvironmentInfluence(gameState.currentEncounter?.environmentInfluence);
		}));
	}

	private void StopEnvironmentInfluence(EnvironmentInfluence influence)
	{
		if (influence.spawnEffectInstance != null)
		{
			ParticleSystem[] componentsInChildren = gameState.currentEncounter.environmentInfluence.spawnEffectInstance.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(true, (ParticleSystemStopBehavior)1);
			}
			Object.Destroy(gameState.currentEncounter.environmentInfluence.spawnEffectInstance, 2f);
		}
	}
}
