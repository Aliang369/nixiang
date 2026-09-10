public class GuideModule_XiaKe : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnBattleStart);
		void _OnBattleStart(GameEvent.Type type, GameEvent eve)
		{
			if (eve.gameState.playerState.CharacterDef.attachClass.mClass == CharacterClassType.XiaKe)
			{
				EnqueueAction(new ActionAction(delegate
				{
					applicationState.runningGameState.UIManager.trigramsDisplay.hintButton.gameObject.SetActive(value: false);
				}));
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_XiaKe));
				EnqueueAction(new ActionAction(delegate
				{
					applicationState.runningGameState.UIManager.trigramsDisplay.PlayHintAnim();
				}));
				applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnBattleStart);
				Finish();
			}
		}
	}
}
