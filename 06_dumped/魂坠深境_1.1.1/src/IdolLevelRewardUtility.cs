using System.Linq;

public static class IdolLevelRewardUtility
{
	public static bool IsClassReward(GlobalRewardSetup reward)
	{
		if (reward is ForgetShrinePointReward)
		{
			return true;
		}
		if (reward is UnlockCTSOGlobalReward unlockCTSOGlobalReward && !unlockCTSOGlobalReward.unlocks.IsEmptyOrNull() && unlockCTSOGlobalReward.unlocks.All((CTScriptableObject u) => u is ItemDefinition))
		{
			return true;
		}
		return false;
	}
}
