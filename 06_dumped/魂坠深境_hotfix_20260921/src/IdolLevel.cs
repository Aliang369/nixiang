using System.Collections.Generic;

public class IdolLevel
{
	public int level;

	[LocalizeText("等级描述")]
	public string description;

	public List<GlobalRewardSetup> reward;
}
