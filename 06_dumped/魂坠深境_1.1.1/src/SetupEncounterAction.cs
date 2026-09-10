using System.Collections;
using FMODUnity;

public class SetupEncounterAction : AbstractEnumeratorAction
{
	private readonly EncounterSetupInfo setupInfo;

	public SetupEncounterAction(EncounterSetupInfo info)
	{
		setupInfo = info;
	}

	protected override IEnumerator Run()
	{
		gameState.currentEncounter?.Clear();
		if (setupInfo != null)
		{
			gameState.currentEncounter = new EncounterInfo();
			yield return gameState.currentEncounter.SetupCor(setupInfo, gameState);
			PlayEnterBattleSound();
		}
	}

	private void PlayEnterBattleSound()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (gameState.currentEncounter == null || !(gameState.currentEncounter.triggerEvent is EnterBattleEvent))
		{
			return;
		}
		CharacterAI enemyAI = gameState.currentEncounter.enemyAI;
		if (enemyAI != null)
		{
			CharacterAIComponent component = enemyAI.component;
			if ((object)component != null)
			{
				_ = component.battleBgm;
				if (true && !((EventReference)(ref gameState.currentEncounter.enemyAI.component.battleBgm)).IsNull)
				{
					Bootstrap.instance.audioManager.PlaySustained(gameState.currentEncounter.enemyAI.component.battleBgm, "BgmTag");
					return;
				}
			}
		}
		CharacterAI enemyAI2 = gameState.currentEncounter.enemyAI;
		if (enemyAI2 != null && enemyAI2.enemyEncounterType == EnemyEncounterType.Normal)
		{
			Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.bgm_general_battle_normal, "BgmTag");
		}
		else
		{
			Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.bgm_general_battle_elite, "BgmTag");
		}
	}
}
