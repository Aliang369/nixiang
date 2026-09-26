using System;
using System.Collections.Generic;

public class zbMonsterBuffRegister
{
	private Dictionary<int, Func<zbMonsterBuffBase>> buffRegistry = new Dictionary<int, Func<zbMonsterBuffBase>>();

	private void RegisterMod(int buffIndex, Func<zbMonsterBuffBase> modConstructor)
	{
		if (!buffRegistry.ContainsKey(buffIndex))
		{
			buffRegistry.Add(buffIndex, modConstructor);
		}
	}

	public zbMonsterBuffBase CreateBuff(int buffId)
	{
		zbMonsterBuffCsvRecord recordByKey = zbDataConfig.Instance.monsterBuffCsvTable.GetRecordByKey(buffId);
		if (recordByKey == null)
		{
			return null;
		}
		if (buffRegistry.ContainsKey(recordByKey.buffIndex))
		{
			return buffRegistry[recordByKey.buffIndex]();
		}
		return null;
	}

	public void Init()
	{
		RegisterMod(1, () => new zbMonsterBuffIndex1());
	}

	public void Free()
	{
	}
}
