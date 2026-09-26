using Proto.Enums;

public class zbBuffFreeze : zbBuffBase
{
	public override void RefreshProperty()
	{
		durationTime = GetAttrValueByType(EAttrSkillType.FreezeTime) + extraTime;
	}
}
