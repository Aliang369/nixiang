public class GuideModule_Loot : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.showItemBonusSelectionPanelEvent.AddListener(_OnShowPanel);
		void _OnShowPanel(SelectGetItemBonus bonus)
		{
		}
	}
}
