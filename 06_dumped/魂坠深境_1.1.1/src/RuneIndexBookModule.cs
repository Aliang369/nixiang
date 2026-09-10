using ContainerOfSoul.Display.Fortress.IndexDisplay;

public class RuneIndexBookModule : RuneDisplay.DisplayModule
{
	public IndexBookContentMask indexBookContentMask;

	public override void Init()
	{
		base.display.InitUI(indexBookContentMask);
	}

	public override void Setup()
	{
		indexBookContentMask.Setup(base.display.rune.def);
	}
}
