public class TriggerForerunnerLegacy : Effect
{
	public bool useConfiguredLegacy;

	[LocalizeText]
	public string note;

	public ItemDefinition itemDef;

	public ItemSkinDef itemSkinDef;

	public bool upgradeItem = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ForerunnerManager forerunnerManager = eve.gameState.applicationState.forerunnerManager;
		ForerunnerLegacy forerunnerLegacy;
		if (useConfiguredLegacy)
		{
			if (itemDef == null || itemSkinDef == null || itemSkinDef.attachItem != itemDef)
			{
				return NodeCastResult.Failure;
			}
			Item item = new Item(itemDef).SetSkin(itemSkinDef, upgradeItem);
			forerunnerLegacy = forerunnerManager.GenerateStoryForerunnerLegacy(note, item);
		}
		else
		{
			forerunnerLegacy = forerunnerManager.GenerateDefaultForerunnerLegacy();
		}
		StoryNoteInfo forerunnerLegacy2 = StoryNoteInfo.GetForerunnerLegacy(forerunnerLegacy);
		forerunnerLegacy2.canBack = false;
		Effect.EnqueueAction(eve, new TriggerStoryNoteAction(forerunnerLegacy2, halt: true));
		return NodeCastResult.Success;
	}
}
