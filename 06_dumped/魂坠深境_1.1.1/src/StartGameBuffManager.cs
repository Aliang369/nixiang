using System.Collections.Generic;

public class StartGameBuffManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<StartGameBuff> currentStartGameBuffs;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (currentStartGameBuffs == null)
		{
			currentStartGameBuffs = new List<StartGameBuff>();
		}
		currentStartGameBuffs.RemoveAll((StartGameBuff b) => b == null || !b.IsActivatable(applicationState));
	}

	public void AddStartGameBuff(StartGameBuff buff)
	{
		base.applicationState.globalUnlockManager.RecordGotObject(buff.def);
		currentStartGameBuffs.Add(buff);
	}

	public bool WithAnyActivatableStartGameBuff()
	{
		if (currentStartGameBuffs.Count > 0)
		{
			return currentStartGameBuffs.Exists((StartGameBuff p) => p.IsActivatable(base.applicationState));
		}
		return false;
	}

	public void TestUnlockAll()
	{
		foreach (StartGameBuffDef validAsset in base.applicationState.assetCatalog.startGameBuffCatalog.ValidAssets)
		{
			currentStartGameBuffs.Add(new StartGameBuff(validAsset));
		}
	}
}
