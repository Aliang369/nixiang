public class SkillPanel_Side : SkillPanel
{
	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		SkillPanel.SetSideSkillPosition(pool.GetActiveElements());
	}
}
