using System;

public class ItemQualityLevelConstraint : ItemConstraint
{
	public enum CompareType
	{
		Equal,
		Lower,
		Higher
	}

	public QualityLevel level;

	public CompareType type;

	public override bool? CheckItemAccept(Item item)
	{
		return type switch
		{
			CompareType.Equal => item.qualityLevel == level, 
			CompareType.Lower => item.qualityLevel < level, 
			CompareType.Higher => item.qualityLevel > level, 
			_ => throw new Exception($"type error: {(int)type}"), 
		};
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			qualityLevel = new NodeDisplayInfo.Element<QualityLevel>(level)
		};
	}
}
