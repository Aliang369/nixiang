using System;
using System.Collections.Generic;

public class zbPetRegister
{
	private Dictionary<int, Func<zbPetSkillHandleBase>> petSkillHandlerRegistry = new Dictionary<int, Func<zbPetSkillHandleBase>>();

	private void RegisterPetSkillHandler(int skillIndex, Func<zbPetSkillHandleBase> modConstructor)
	{
		if (!petSkillHandlerRegistry.ContainsKey(skillIndex))
		{
			petSkillHandlerRegistry.Add(skillIndex, modConstructor);
		}
	}

	public zbPetSkillHandleBase CreatePetSkillHandler(int skillId)
	{
		zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(skillId);
		if (recordByKey == null)
		{
			return null;
		}
		if (petSkillHandlerRegistry.ContainsKey(recordByKey.idIndex))
		{
			return petSkillHandlerRegistry[recordByKey.idIndex]();
		}
		return null;
	}

	public void Init()
	{
		RegisterPetSkillHandler(100, () => new zbPetSkill0100());
		RegisterPetSkillHandler(200, () => new zbPetSkill0200());
		RegisterPetSkillHandler(300, () => new zbPetSkill0300());
		RegisterPetSkillHandler(400, () => new zbPetSkill0400());
		RegisterPetSkillHandler(500, () => new zbPetSkill0500());
		RegisterPetSkillHandler(600, () => new zbPetSkill0600());
		RegisterPetSkillHandler(700, () => new zbPetSkill0700());
		RegisterPetSkillHandler(800, () => new zbPetSkill0800());
		RegisterPetSkillHandler(900, () => new zbPetSkill0900());
		RegisterPetSkillHandler(1000, () => new zbPetSkill1000());
		RegisterPetSkillHandler(1100, () => new zbPetSkill1100());
		RegisterPetSkillHandler(1200, () => new zbPetSkill1200());
		RegisterPetSkillHandler(1300, () => new zbPetSkill1300());
		RegisterPetSkillHandler(1400, () => new zbPetSkill1400());
	}
}
