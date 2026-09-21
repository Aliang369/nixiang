using UnityEngine;

public class EncounterWorldEventRecord : GameJournalRecord
{
	[SaveField(null, null)]
	private string worldEventID;

	public override string NetworkSaveArg0
	{
		get
		{
			return worldEventID;
		}
		set
		{
			worldEventID = value;
		}
	}

	private EncounterWorldEventRecord()
	{
	}

	public EncounterWorldEventRecord(WorldEvent worldEvent)
	{
		worldEventID = worldEvent.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		WorldEvent worldEvent = applicationState.assetCatalog.FullIdToCTAsset<WorldEvent>(worldEventID);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_trigger_worldEvent_arg0.FormatWith(worldEvent.EventName.ColorText(Color.yellow));
	}
}
