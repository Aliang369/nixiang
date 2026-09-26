using Proto.Enums;

public class zbReminderHandler
{
	public void Init()
	{
	}

	public EFuncType GetFuncTypeByReminderType(EReminderType reminderType)
	{
		EFuncType result = EFuncType.EmptyValue;
		switch (reminderType)
		{
		case EReminderType.ArenaRecord:
			result = EFuncType.Arena;
			break;
		case EReminderType.HelpAward:
			result = EFuncType.EliteQuest;
			break;
		case EReminderType.LadderRecord:
		case EReminderType.PeakArenaRecord:
			result = EFuncType.CrossLadder;
			break;
		case EReminderType.MineRecord:
		case EReminderType.CrossMineRecord:
			result = EFuncType.MineWar;
			break;
		case EReminderType.TruckRecord:
			result = EFuncType.Truck;
			break;
		case EReminderType.LegionWar:
			result = EFuncType.LegionWar;
			break;
		}
		return result;
	}

	public bool HasReminderNotify(EReminderType reminderType)
	{
		EFuncType funcTypeByReminderType = GetFuncTypeByReminderType(reminderType);
		if (zbSelf.Instance.funHandler.IsLockNoTip(funcTypeByReminderType))
		{
			return false;
		}
		return zbServerData.sGameData.ReminderList.Contains((int)reminderType);
	}
}
