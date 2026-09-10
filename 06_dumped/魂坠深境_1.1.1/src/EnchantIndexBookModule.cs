using ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnchantIndexBookModule : EnchantDisplay.DisplayModule
{
	public IndexBookContentMask indexBookContentMask;

	public override void Init()
	{
		base.display.InitUI(indexBookContentMask);
	}

	public override void Setup()
	{
		indexBookContentMask.Setup(base.display.enchant.def);
	}
}
