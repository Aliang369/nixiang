using System;

public class SelectSkillInfo
{
	public bool canCancel;

	public Action<GameSkill> confirmCallback;

	public Action onCancel;
}
