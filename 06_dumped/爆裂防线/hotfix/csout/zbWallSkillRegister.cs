using System;
using System.Collections.Generic;

public class zbWallSkillRegister
{
	private Dictionary<int, Func<zbWallSkillHandleBase>> wallSkillRegistry = new Dictionary<int, Func<zbWallSkillHandleBase>>();

	private void RegisterMod(int wallIndex, Func<zbWallSkillHandleBase> modConstructor)
	{
		if (!wallSkillRegistry.ContainsKey(wallIndex))
		{
			wallSkillRegistry.Add(wallIndex, modConstructor);
		}
	}

	public zbWallSkillHandleBase CreateDefense(int modId)
	{
		zbWallSkillCsvRecord recordByKey = zbDataConfig.Instance.wallSkillCsvTable.GetRecordByKey(modId);
		if (recordByKey == null)
		{
			return null;
		}
		if (wallSkillRegistry.ContainsKey(recordByKey.wallIndex))
		{
			return wallSkillRegistry[recordByKey.wallIndex]();
		}
		return null;
	}

	public void Init()
	{
		RegisterMod(5021, () => new zbWallSkillIndex5021());
		RegisterMod(5031, () => new zbWallSkillIndex5031());
		RegisterMod(5041, () => new zbWallSkillIndex5041());
		RegisterMod(5051, () => new zbWallSkillIndex5051());
		RegisterMod(5061, () => new zbWallSkillIndex5061());
		RegisterMod(5071, () => new zbWallSkillIndex5071());
		RegisterMod(5081, () => new zbWallSkillIndex5081());
	}
}
