public class GuideModule_Influence : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddItem, _OnAddInfluence);
		void _OnAddInfluence(GameEvent.Type type, GameEvent gameEvent)
		{
			if (gameEvent.TargetInfluence != null)
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_bless));
				applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddInfluence, _OnAddInfluence);
				Finish();
			}
		}
	}
}
