using Proto.Enums;

public class zbBuffParalysis : zbBuffBase
{
	public override void RefreshProperty()
	{
		durationTime = GetAttrValueByType(EAttrSkillType.ParalysisTime) + extraTime;
	}
}
