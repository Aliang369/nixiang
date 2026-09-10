using ContainerOfSoul.Display.Fortress.IndexDisplay;

public class InfluenceIndexBookModule : InfluenceDisplay.DisplayModule
{
	public IndexBookContentMask indexBookContentMask;

	public override void Init()
	{
		base.display.InitUI(indexBookContentMask);
	}

	public override void Setup()
	{
		indexBookContentMask.Setup(base.display.influence.def);
	}
}
