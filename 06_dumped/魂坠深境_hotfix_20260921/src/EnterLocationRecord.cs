using System;
using UnityEngine;

public class EnterLocationRecord : GameJournalRecord
{
	[SaveField(null, null)]
	private PathLocation.LocationType locationType;

	public override string NetworkSaveArg0
	{
		get
		{
			return locationType.ToString();
		}
		set
		{
			locationType = Enum.Parse<PathLocation.LocationType>(value);
		}
	}

	private EnterLocationRecord()
	{
	}

	public EnterLocationRecord(PathLocation location)
	{
		locationType = location.type;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_enter_location_arg0.FormatWith(PathLocation.GetLocationTypeName(locationType).ColorText(Color.yellow));
	}
}
