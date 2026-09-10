using UnityEngine;

public class ActivateSkillRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string skillId;

	public override string NetworkSaveArg0
	{
		get
		{
			return skillId;
		}
		set
		{
			skillId = value;
		}
	}

	private ActivateSkillRecord()
	{
	}

	public ActivateSkillRecord(GameSkill skill)
	{
		skillId = skill.skillDef.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		SkillDef skillDef = applicationState.assetCatalog.FullIdToCTAsset<SkillDef>(skillId);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_activate_skill_record_arg0.FormatWith(skillDef.skillName.ColorText(Color.yellow));
	}
}
