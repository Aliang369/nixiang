using Proto.Types;

public class zbCombatChecker
{
	public static bool IsValidBattleInfo(BattleInfo battleInfo)
	{
		if (battleInfo == null)
		{
			return false;
		}
		if (battleInfo.AttackerTeamInfo.MasterTeamInfo.ModList.Count == 0)
		{
			return false;
		}
		if (battleInfo.AttackerTeamInfo.MasterTeamInfo.DefenseInfo.AttrList.Count == 0)
		{
			return false;
		}
		return true;
	}
}
