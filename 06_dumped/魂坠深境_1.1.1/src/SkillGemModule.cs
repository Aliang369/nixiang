public class SkillGemModule : SkillInfoDisplay.DisplayModule
{
	public SkillGemArea skillGemArea;

	public override void Init()
	{
		base.display.InitUI(skillGemArea);
	}

	public override void Setup()
	{
		skillGemArea.Setup(base.display.setupData);
	}

	public override void UpdateDisplay()
	{
		skillGemArea.Setup(base.display.setupData);
	}
}
