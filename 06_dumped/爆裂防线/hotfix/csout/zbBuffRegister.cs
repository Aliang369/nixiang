using System;
using System.Collections.Generic;

public class zbBuffRegister
{
	private Dictionary<int, Func<zbBuffBase>> buffRegistry = new Dictionary<int, Func<zbBuffBase>>();

	private void RegisterMod(int buffIndex, Func<zbBuffBase> modConstructor)
	{
		if (!buffRegistry.ContainsKey(buffIndex))
		{
			buffRegistry.Add(buffIndex, modConstructor);
		}
	}

	public zbBuffBase CreateBuff(int buffId)
	{
		zbSkillBuffCsvRecord recordByKey = zbDataConfig.Instance.skillBuffCsvTable.GetRecordByKey(buffId);
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
		RegisterMod(1, () => new zbBuffSlowDown());
		RegisterMod(2, () => new zbBuffFreeze());
		RegisterMod(3, () => new zbBuffParalysis());
		RegisterMod(4, () => new zbBuffDaze());
		RegisterMod(5, () => new zbBuffBurn());
		RegisterMod(6, () => new zbBuffIce());
		RegisterMod(7, () => new zbBuffDieTodo());
		RegisterMod(8, () => new zbBuffBeDamageUp());
		RegisterMod(9, () => new zbBuffBeHitTodo());
		RegisterMod(10, () => new zbBuffIndex10());
		RegisterMod(11, () => new zbBuffIndex11());
		RegisterMod(12, () => new zbBuffIndex12());
		RegisterMod(13, () => new zbBuffIndex13());
		RegisterMod(14, () => new zbBuffIndex14());
		RegisterMod(15, () => new zbBuffIndex15());
		RegisterMod(16, () => new zbBuffIndex16());
		RegisterMod(17, () => new zbBuffIndex17());
		RegisterMod(29, () => new zbBuffIndex29());
		RegisterMod(30, () => new zbBuffIndex30());
		RegisterMod(34, () => new zbBuffIndex34());
	}

	public void Free()
	{
	}
}
