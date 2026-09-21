public class GuideModule_CrystalFreeze : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnterCrystalFreeze, _OnBattleStart);
		void _OnBattleStart(GameEvent.Type type, GameEvent eve)
		{
			EnemyEncounterType enemyEncounterType = eve.gameState.currentEncounter.enemyAI.enemyEncounterType;
			if (enemyEncounterType == EnemyEncounterType.Boss || enemyEncounterType == EnemyEncounterType.Elite)
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_crystalFreeze));
				applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnterCrystalFreeze, _OnBattleStart);
				Finish();
			}
		}
	}
}
