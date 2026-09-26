using System;

[Serializable]
public class zbDungeonEventCsvRecord : zbTableBaseRecord
{
	public int eventId { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string eventSpine { get; set; }

	public int npcAvatar { get; set; }

	public int tiggerType { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDungeonEventCsvRecord Read(zbBinReader r)
	{
		return new zbDungeonEventCsvRecord
		{
			eventId = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			eventSpine = r.ReadString(),
			npcAvatar = r.ReadInt(),
			tiggerType = r.ReadInt()
		};
	}
}
