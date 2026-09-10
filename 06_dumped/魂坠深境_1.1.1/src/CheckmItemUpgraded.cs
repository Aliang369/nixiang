public class CheckmItemUpgraded : CheckNode
{
	public ItemSkinDef skinDefBranch;

	public bool forceFalse;

	private string SkinSetName => skinDefBranch?.attachSkinSet.skinSetName;

	public override bool? Check(GameEvent eve)
	{
		if (forceFalse)
		{
			return false;
		}
		return eve != null && eve.CasterItem?.upgraded == true && ((skinDefBranch == null && eve.CasterItem.skin == null) || (skinDefBranch != null && eve.CasterItem.skin == skinDefBranch));
	}
}
