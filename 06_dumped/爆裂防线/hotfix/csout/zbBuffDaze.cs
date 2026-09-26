using Proto.Enums;

public class zbBuffDaze : zbBuffBase
{
	public override void RefreshProperty()
	{
		durationTime = GetAttrValueByType(EAttrSkillType.DazeTime) + extraTime;
	}
}
