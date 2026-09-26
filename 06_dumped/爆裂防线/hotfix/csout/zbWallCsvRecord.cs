using System;

[Serializable]
public class zbWallCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public string animation { get; set; }

	public string carAnimation { get; set; }

	public float carOffset { get; set; }

	public string backEffect { get; set; }

	public string backGround { get; set; }

	public int quality { get; set; }

	public int propId { get; set; }

	public int convertPropId { get; set; }

	public int offset { get; set; }

	public int starUpgradeId { get; set; }

	public int open { get; set; }

	public int sort { get; set; }

	public string skillIcon { get; set; }

	public int wallSkill1 { get; set; }

	public int wallSkill2 { get; set; }

	public int wallSkill3 { get; set; }

	public int wallSkill4 { get; set; }

	public int wallSkill5 { get; set; }

	public string banner { get; set; }

	public string preShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallCsvRecord Read(zbBinReader r)
	{
		return new zbWallCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			animation = r.ReadString(),
			carAnimation = r.ReadString(),
			carOffset = r.ReadFloat(),
			backEffect = r.ReadString(),
			backGround = r.ReadString(),
			quality = r.ReadInt(),
			propId = r.ReadInt(),
			convertPropId = r.ReadInt(),
			offset = r.ReadInt(),
			starUpgradeId = r.ReadInt(),
			open = r.ReadInt(),
			sort = r.ReadInt(),
			skillIcon = r.ReadString(),
			wallSkill1 = r.ReadInt(),
			wallSkill2 = r.ReadInt(),
			wallSkill3 = r.ReadInt(),
			wallSkill4 = r.ReadInt(),
			wallSkill5 = r.ReadInt(),
			banner = r.ReadString(),
			preShow = r.ReadString()
		};
	}
}
