using System;

public class YuanQiActivityMissionTalentEventData
{
	public readonly PlayerCharacterState character;

	public readonly TalentDef talent;

	public readonly DateTime triggerTime;

	public YuanQiActivityMissionTalentEventData(PlayerCharacterState character, TalentDef talent)
	{
		this.character = character;
		this.talent = talent;
		triggerTime = DateTime.UtcNow;
	}
}
