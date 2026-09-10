using UnityEngine;

public class ActivatePotentialTalentRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string talentId;

	public override string NetworkSaveArg0
	{
		get
		{
			return talentId;
		}
		set
		{
			talentId = value;
		}
	}

	private ActivatePotentialTalentRecord()
	{
	}

	public ActivatePotentialTalentRecord(TalentDef talent)
	{
		talentId = talent.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		TalentDef talentDef = applicationState.assetCatalog.FullIdToCTAsset<TalentDef>(talentId);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_activate_potential_talent_arg0.FormatWith(talentDef.talentName.ColorText(Color.yellow));
	}
}
