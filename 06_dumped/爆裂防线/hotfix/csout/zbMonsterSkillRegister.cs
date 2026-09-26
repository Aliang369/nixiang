using System;
using System.Collections.Generic;

public class zbMonsterSkillRegister
{
	private Dictionary<int, Func<zbMonsterSkillBase>> modRegistry = new Dictionary<int, Func<zbMonsterSkillBase>>();

	private void RegisterMonsterSkill(int modIndex, Func<zbMonsterSkillBase> modConstructor)
	{
		if (!modRegistry.ContainsKey(modIndex))
		{
			modRegistry.Add(modIndex, modConstructor);
		}
	}

	public zbMonsterSkillBase CreateMonsterSkill(int skillId)
	{
		if (skillId == 0)
		{
			return null;
		}
		zbMonsterSkillCsvRecord recordByKey = zbDataConfig.Instance.monsterSkillCsvTable.GetRecordByKey(skillId);
		if (recordByKey == null)
		{
			return null;
		}
		if (modRegistry.ContainsKey(recordByKey.skillIndex))
		{
			return modRegistry[recordByKey.skillIndex]();
		}
		return new zbMonsterSkillBase();
	}

	public void Init()
	{
		RegisterMonsterSkill(10000, () => new zbMonsterSkillIndex10000());
		RegisterMonsterSkill(10001, () => new zbMonsterSkillIndex10001());
		RegisterMonsterSkill(10002, () => new zbMonsterSkillIndex10002());
		RegisterMonsterSkill(10021, () => new zbMonsterSkillIndex10021());
		RegisterMonsterSkill(10051, () => new zbMonsterSkillIndex10051());
		RegisterMonsterSkill(10061, () => new zbMonsterSkillIndex10061());
		RegisterMonsterSkill(10081, () => new zbMonsterSkillIndex10081());
		RegisterMonsterSkill(10101, () => new zbMonsterSkillIndex10101());
		RegisterMonsterSkill(10111, () => new zbMonsterSkillIndex10111());
		RegisterMonsterSkill(10112, () => new zbMonsterSkillIndex10112());
		RegisterMonsterSkill(10121, () => new zbMonsterSkillIndex10121());
		RegisterMonsterSkill(10122, () => new zbMonsterSkillIndex10122());
		RegisterMonsterSkill(10131, () => new zbMonsterSkillIndex10131());
		RegisterMonsterSkill(10151, () => new zbMonsterSkillIndex10151());
		RegisterMonsterSkill(10161, () => new zbMonsterSkillIndex10161());
		RegisterMonsterSkill(10171, () => new zbMonsterSkillIndex10171());
		RegisterMonsterSkill(10172, () => new zbMonsterSkillIndex10172());
		RegisterMonsterSkill(10181, () => new zbMonsterSkillIndex10181());
		RegisterMonsterSkill(10201, () => new zbMonsterSkillIndex10201());
		RegisterMonsterSkill(10211, () => new zbMonsterSkillIndex10211());
		RegisterMonsterSkill(10212, () => new zbMonsterSkillIndex10212());
		RegisterMonsterSkill(10222, () => new zbMonsterSkillIndex10222());
		RegisterMonsterSkill(10223, () => new zbMonsterSkillIndex10223());
		RegisterMonsterSkill(10261, () => new zbMonsterSkillIndex10261());
		RegisterMonsterSkill(10262, () => new zbMonsterSkillIndex10262());
		RegisterMonsterSkill(10271, () => new zbMonsterSkillIndex10271());
		RegisterMonsterSkill(10281, () => new zbMonsterSkillIndex10281());
		RegisterMonsterSkill(10283, () => new zbMonsterSkillIndex10283());
		RegisterMonsterSkill(10302, () => new zbMonsterSkillIndex10302());
		RegisterMonsterSkill(10311, () => new zbMonsterSkillIndex10311());
		RegisterMonsterSkill(10321, () => new zbMonsterSkillIndex10321());
		RegisterMonsterSkill(10322, () => new zbMonsterSkillIndex10322());
		RegisterMonsterSkill(10332, () => new zbMonsterSkillIndex10332());
		RegisterMonsterSkill(10352, () => new zbMonsterSkillIndex10352());
		RegisterMonsterSkill(10353, () => new zbMonsterSkillIndex10353());
		RegisterMonsterSkill(10362, () => new zbMonsterSkillIndex10362());
		RegisterMonsterSkill(10373, () => new zbMonsterSkillIndex10373());
		RegisterMonsterSkill(10382, () => new zbMonsterSkillIndex10382());
		RegisterMonsterSkill(10391, () => new zbMonsterSkillIndex10391());
		RegisterMonsterSkill(10431, () => new zbMonsterSkillIndex10431());
		RegisterMonsterSkill(10441, () => new zbMonsterSkillIndex10441());
		RegisterMonsterSkill(20011, () => new zbMonsterSkillIndex20011());
		RegisterMonsterSkill(20081, () => new zbMonsterSkillIndex20081());
		RegisterMonsterSkill(20091, () => new zbMonsterSkillIndex20091());
		RegisterMonsterSkill(20101, () => new zbMonsterSkillIndex20101());
		RegisterMonsterSkill(20102, () => new zbMonsterSkillIndex20102());
		RegisterMonsterSkill(20132, () => new zbMonsterSkillIndex20132());
		RegisterMonsterSkill(20135, () => new zbMonsterSkillIndex20135());
		RegisterMonsterSkill(20161, () => new zbMonsterSkillIndex20161());
		RegisterMonsterSkill(20162, () => new zbMonsterSkillIndex20162());
		RegisterMonsterSkill(20163, () => new zbMonsterSkillIndex20163());
		RegisterMonsterSkill(20192, () => new zbMonsterSkillIndex20192());
		RegisterMonsterSkill(20211, () => new zbMonsterSkillIndex20211());
		RegisterMonsterSkill(30012, () => new zbMonsterSkillIndex30012());
		RegisterMonsterSkill(30021, () => new zbMonsterSkillIndex30021());
		RegisterMonsterSkill(30042, () => new zbMonsterSkillIndex30042());
		RegisterMonsterSkill(30051, () => new zbMonsterSkillIndex30051());
		RegisterMonsterSkill(30062, () => new zbMonsterSkillIndex30062());
		RegisterMonsterSkill(30072, () => new zbMonsterSkillIndex30072());
		RegisterMonsterSkill(30092, () => new zbMonsterSkillIndex30092());
		RegisterMonsterSkill(30111, () => new zbMonsterSkillIndex30111());
		RegisterMonsterSkill(30112, () => new zbMonsterSkillIndex30112());
		RegisterMonsterSkill(30113, () => new zbMonsterSkillIndex30113());
		RegisterMonsterSkill(30121, () => new zbMonsterSkillIndex30121());
		RegisterMonsterSkill(30122, () => new zbMonsterSkillIndex30122());
		RegisterMonsterSkill(30123, () => new zbMonsterSkillIndex30123());
		RegisterMonsterSkill(30131, () => new zbMonsterSkillIndex30131());
		RegisterMonsterSkill(30132, () => new zbMonsterSkillIndex30132());
		RegisterMonsterSkill(30133, () => new zbMonsterSkillIndex30133());
		RegisterMonsterSkill(30141, () => new zbMonsterSkillIndex30141());
		RegisterMonsterSkill(30142, () => new zbMonsterSkillIndex30142());
		RegisterMonsterSkill(30143, () => new zbMonsterSkillIndex30143());
		RegisterMonsterSkill(30151, () => new zbMonsterSkillIndex30151());
		RegisterMonsterSkill(30152, () => new zbMonsterSkillIndex30152());
		RegisterMonsterSkill(30153, () => new zbMonsterSkillIndex30153());
		RegisterMonsterSkill(40091, () => new zbMonsterSkillIndex40091());
		RegisterMonsterSkill(40111, () => new zbMonsterSkillIndex40111());
		RegisterMonsterSkill(40151, () => new zbMonsterSkillIndex40151());
		RegisterMonsterSkill(40402, () => new zbMonsterSkillIndex40402());
		RegisterMonsterSkill(40421, () => new zbMonsterSkillIndex40421());
		RegisterMonsterSkill(40462, () => new zbMonsterSkillIndex40462());
	}
}
