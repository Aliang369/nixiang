public class GuideModule_OpenSelectSkill : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.OpenSelectSkillPanelEvent.AddListener(_OnShowPanel);
		void _OnShowPanel()
		{
			EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_activateSkill));
			applicationState.runningGameState.applicationState.eventManager.OpenSelectSkillPanelEvent.RemoveListener(_OnShowPanel);
			Finish();
		}
	}
}
