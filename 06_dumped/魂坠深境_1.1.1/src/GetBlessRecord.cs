using UnityEngine;

public class GetBlessRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string influenceId;

	public override string NetworkSaveArg0
	{
		get
		{
			return influenceId;
		}
		set
		{
			influenceId = value;
		}
	}

	private GetBlessRecord()
	{
	}

	public GetBlessRecord(Influence inf)
	{
		influenceId = inf.def.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		InfluenceDef influenceDef = applicationState.assetCatalog.FullIdToCTAsset<InfluenceDef>(influenceId);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_bless_arg0.FormatWith(influenceDef.InfluenceName.ColorText(Color.yellow));
	}
}
