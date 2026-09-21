public class SetItemSkinAction : ExecuteAction
{
	public Item target;

	public ItemSkinDef skinDef;

	public bool upgrade;

	public SetItemSkinAction(Item target, ItemSkinDef skinDef, bool upgrade)
	{
		this.target = target;
		this.skinDef = skinDef;
		this.upgrade = upgrade;
	}

	protected override void Update()
	{
		if (target.upgraded)
		{
			target.Downgrade(gameState);
		}
		if (target != null && skinDef != null)
		{
			target.SetSkin(skinDef, upgrade);
		}
	}
}
