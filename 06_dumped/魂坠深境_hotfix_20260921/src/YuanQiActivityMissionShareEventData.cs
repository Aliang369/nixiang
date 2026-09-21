using System;

public class YuanQiActivityMissionShareEventData
{
	public readonly GameRecord record;

	public readonly PlayerCharacterDef character;

	public readonly string source;

	public readonly DateTime triggerTime;

	public YuanQiActivityMissionShareEventData(GameRecord record, PlayerCharacterDef character, string source)
	{
		this.record = record;
		this.character = character;
		this.source = source;
		triggerTime = DateTime.UtcNow;
	}
}
