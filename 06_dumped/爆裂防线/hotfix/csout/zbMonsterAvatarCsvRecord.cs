using System;

[Serializable]
public class zbMonsterAvatarCsvRecord : zbTableBaseRecord
{
	public int avatarId { get; set; }

	public int hudOffsetY { get; set; }

	public string fightBone { get; set; }

	public string icon { get; set; }

	public string bigIcon { get; set; }

	public string shadow { get; set; }

	public int pvpTurnOver { get; set; }

	public float shadowScale { get; set; }

	public int hitTime { get; set; }

	public int boxWidth { get; set; }

	public int boxHeight { get; set; }

	public int boxOffset { get; set; }

	public string dieColor { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterAvatarCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterAvatarCsvRecord
		{
			avatarId = r.ReadInt(),
			hudOffsetY = r.ReadInt(),
			fightBone = r.ReadString(),
			icon = r.ReadString(),
			bigIcon = r.ReadString(),
			shadow = r.ReadString(),
			pvpTurnOver = r.ReadInt(),
			shadowScale = r.ReadFloat(),
			hitTime = r.ReadInt(),
			boxWidth = r.ReadInt(),
			boxHeight = r.ReadInt(),
			boxOffset = r.ReadInt(),
			dieColor = r.ReadString()
		};
	}
}
